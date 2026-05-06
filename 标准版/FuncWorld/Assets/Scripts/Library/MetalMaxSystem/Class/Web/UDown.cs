//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE
using Cysharp.Threading.Tasks;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace MetalMaxSystem.Unity
{
    public class UDown : MonoBehaviour
    {
        private static UDown _instance;
        private static Queue<Action> actions = new Queue<Action>();
        private static Queue<IEnumerator> coroutines = new Queue<IEnumerator>();

        public static UDown Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("UDown");
                    _instance = obj.AddComponent<UDown>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        void Update()
        {
            //即使Enqueue已加锁,Update遍历队列时若不加锁可能遇到Enqueue执行到一半时主线程的Update开始遍历队列->读取到不一致的队列状态

            lock (actions)
            {
                while (actions.Count > 0)
                {
                    actions.Dequeue()?.Invoke();
                }
            }

            lock (coroutines)
            {
                while (coroutines.Count > 0)
                {
                    StartCoroutine(coroutines.Dequeue());
                }
            }
        }

        //↓封装的回调函数(支持在子线程使用,然后由主线程回调执行)

        /// <summary>
        /// 回调函数.
        /// 对Unity引擎组件实例相关动作执行回调(回到主线程调用Action).
        /// 示例MainThreadDispatcher.Instance.Invoke(() =>{涉及主线程对象的动作});
        /// </summary>
        /// <param name="action">这个匿名委托将被添加到队列,由一个专门处理回调的MonoBehaviour组件实例来跑</param>
        public static void Invoke(Action action)
        {
            if (action == null) return;

            lock (actions)
            {
                actions.Enqueue(action);
            }
        }

        /// <summary>
        /// 回调函数.
        /// 对Unity引擎组件实例相关动作执行回调(回到主线程调用协程).
        /// 示例MainThreadDispatcher.Instance.Invoke(MyCoroutine());
        /// </summary>
        public static void Invoke(IEnumerator coroutine)
        {
            if (coroutine == null) return;

            lock (coroutines)
            {
                coroutines.Enqueue(coroutine);
            }
        }

        /// <summary>
        /// 回调函数.直接传入协程方法体.
        /// 对Unity引擎组件实例相关动作执行回调(回到主线程调用协程).
        /// 示例MainThreadDispatcher.Instance.Invoke(() => {
        ///     yield return new WaitForSeconds(1);
        ///     Debug.Log("Delayed log");
        /// });
        /// </summary>
        public static void Invoke(Func<IEnumerator> coroutineFunc)
        {
            if (coroutineFunc == null) return;
            Invoke(coroutineFunc());
        }

        #region Unity主线程协程方案

        /// <summary>
        /// 当前下载请求的引用,用于支持取消下载等操作.在Download协程中赋值为当前的UnityWebRequest实例,在 CancelDownload 方法中调用 Abort() 来中止下载.
        /// 同时在请求完成后清除引用以避免误操作.
        /// </summary>
        private UnityWebRequest _currentRequest;

        /// <summary>
        /// 默认的文件URL正则表达式,用于从HTML内容中提取符合条件的URL字符串.
        /// </summary>
        public static Regex fileUrlRegex = new Regex("\"(?<url>https?://[^\"\\s]+\\.jpg)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //添加了?<url>就可以通过 match.Groups["url"].Value 来获取匹配的URL字符串,而不需要再进行一次字符串处理来去掉引号.
        //否则要用match.Groups[1].Value然后去掉两端的引号,写成 match.Groups[1].Value.Trim('"')

        /// <summary>
        /// 协程异步下载指定URL文件
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="savePath">保存文件路径</param>
        public void Download(string url, string savePath)
        {
            StartCoroutine(Download_Func(url, savePath));
        }

        /// <summary>
        /// 协程异步下载指定URL文件(带重试机制的文件下载)
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <param name="maxRetries">最大重试次数</param>
        public void DownloadWithRetry(string url, string savePath, int timeout = 60, int maxRetries = 3)
        {
            StartCoroutine(DownloadWithRetry_Func(url, savePath, timeout, maxRetries));
        }

        /// <summary>
        /// 协程异步下载指定URL文件(正则表达式匹配的内容)
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="saveFilePath">保存文件路径</param>
        /// <param name="objectRegex">正则表达式</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <param name="maxMatches">最大匹配数量（0表示无限制，默认1）</param>
        public void DownloadFilesWithRegex(string url, string saveFilePath, string xpath = "//img", string attribute = "src", string filterRegex = @"\.jpg$", int timeout = 60, int maxMatches = 5)
        {
            StartCoroutine(DownloadFilesWithRegex_Func(url, saveFilePath, xpath, attribute, filterRegex, timeout, maxMatches));
        }

        /// <summary>
        /// 协程异步下载指定URL文件
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="savePath">保存文件路径</param>
        /// <param name="timeout">超时时间(单位为秒),默认值60秒</param>
        /// <returns></returns>
        private IEnumerator Download_Func(string url, string savePath, int timeout = 60)
        {
            // 构建完整路径
            string fullPath = Path.Combine(Application.persistentDataPath, savePath);

            // 确保目录存在
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 检查是否支持断点续传(可选高级功能)
            // 如果需要断点续传,这里需要先用 HEAD 请求获取服务器文件大小,
            // 并检查本地文件是否存在,然后使用 new DownloadHandlerFile(fullPath, true) 追加模式.
            // 此处演示基础覆盖下载,若需断点续传请参考搜索结果中的 Range 头实现.

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                _currentRequest = request; // 记录引用以便取消

                // 设置超时时间
                request.timeout = timeout;

                // 获取HTML内容的请求不需设置下载处理器,因默认 DownloadHandlerBuffer 会将响应内容作为字符串存储在 downloadHandler.text,适合小文本数据处理.
                // 对大文件下载则用 DownloadHandlerFile 直接写入磁盘以节省内存.
                // 第二个参数 append 默认为 false (覆盖).若需断点续传设为 true 并配合 Range 头
                request.downloadHandler = new DownloadHandlerFile(fullPath);

                // 设置移除文件在中止时的行为 (Unity 2022+):自动清理下载失败或中止时产生的垃圾文件‌
                ((DownloadHandlerFile)request.downloadHandler).removeFileOnAbort = true;

                //等到请求完成,期间可以通过 CancelDownload() 方法中止下载,通过 _currentRequest.Abort() 来触发 removeFileOnAbort 自动清理下载文件
                //查询状态时可以通过 _currentRequest.isDone 来判断是否完成,通过 _currentRequest.result 来获取结果状态,通过 _currentRequest.error 来获取错误信息
                yield return request.SendWebRequest();

                _currentRequest = null; // 清除引用

                // 统一判断结果
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Download Error: {request.error} | Result: {request.result}");

                    // 下载失败时清理可能产生的空文件或损坏文件
                    if (File.Exists(fullPath))
                    {
                        try
                        {
                            File.Delete(fullPath);
                            Debug.Log("Corrupted file deleted.");

                            //用户取消下载‌ → removeFileOnAbort 自动清理
                            //‌服务器返回 404‌ → removeFileOnAbort ‌不生效‌ → 手动清理动作处理
                            //‌网络超时‌ → removeFileOnAbort 可能生效
                            //‌磁盘空间不足‌ → 两者都可能需要参与清理
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"Failed to delete corrupted file: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Debug.Log("File successfully downloaded and saved to: " + fullPath);
                    // 可在这里触发下载完成的回调事件
                }
            }
        }

        /// <summary>
        /// 协程异步下载指定URL文件(带重试机制的文件下载)
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <param name="maxRetries">最大重试次数</param>
        /// <returns>协程枚举器</returns>
        private IEnumerator DownloadWithRetry_Func(string url, string savePath, int timeout = 60, int maxRetries = 3)
        {
            int retryCount = 0;

            while (retryCount <= maxRetries)
            {
                bool success = false;
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    _currentRequest = request; // 记录引用以便取消
                    request.timeout = timeout;
                    request.downloadHandler = new DownloadHandlerFile(savePath);
                    ((DownloadHandlerFile)request.downloadHandler).removeFileOnAbort = true;

                    yield return request.SendWebRequest();
                    _currentRequest = null; // 清除引用

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"File downloaded successfully: {savePath}");
                        success = true;
                        break;
                    }
                    else
                    {
                        Debug.LogWarning($"Download failed (attempt {retryCount + 1}/{maxRetries + 1}): {request.error}");

                        // 清理失败文件
                        if (File.Exists(savePath))
                        {
                            try { File.Delete(savePath); }
                            catch { /* 忽略删除异常 */ }
                        }
                    }
                }

                if (!success && retryCount < maxRetries)
                {
                    retryCount++;
                    Debug.Log($"Retrying in 1 second... ({retryCount}/{maxRetries})");
                    yield return new WaitForSeconds(1);
                }
                else if (!success)
                {
                    Debug.LogError($"Failed to download after {maxRetries + 1} attempts: {url}");
                    break;
                }
            }
        }

        /// <summary>
        /// 协程异步下载指定URL文件中，符合XPath节点及正则表达式过滤的内容
        /// </summary>
        /// <param name="url">网页URL</param>
        /// <param name="saveFilePath">保存文件路径(自动追加后缀编号),若是相对路径则相对于Application.persistentDataPath</param>
        /// <param name="xpath">用于定位目标HTML节点的XPath表达式，默认为"//img"</param>
        /// <param name="attribute">从定位到的节点中提取哪个属性的值，默认为"src"</param>
        /// <param name="filterRegex">对提取出的属性值进行过滤的正则表达式。如果为null或空，则下载所有匹配节点的资源。</param>
        /// <param name="timeout">超时时间（秒，默认60）</param>
        /// <param name="maxMatches">最大匹配数量（0表示无限制，默认1）</param>
        /// <returns></returns>
        public IEnumerator DownloadFilesWithRegex_Func(string url, string saveFilePath, string xpath = "//img", string attribute = "src", string filterRegex = @"\.jpg$", int timeout = 60, int maxMatches = 5)
        {
            // 1. 参数验证
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("URL cannot be null or empty.");
                yield break;
            }

            // 2. 获取HTML内容
            string htmlContent;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                _currentRequest = request; // 记录引用以便取消
                request.timeout = timeout;
                yield return request.SendWebRequest();
                _currentRequest = null; // 清除引用

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download HTML: {request.error} | URL: {url}");
                    yield break;
                }

                htmlContent = request.downloadHandler.text;

                // 记录调试信息
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Debug.Log($"HTML downloaded successfully, length: {htmlContent.Length} chars");
#endif
            }

            // 3. 使用HtmlAgilityPack解析HTML并选取节点
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            var nodes = doc.DocumentNode.SelectNodes(xpath);

            if (nodes == null || nodes.Count == 0)
            {
                Debug.LogError($"No nodes found with XPath: {xpath}");
                Debug.LogWarning($"HTML preview (first 500 chars): {htmlContent.Substring(0, Math.Min(500, htmlContent.Length))}");
                yield break;
            }

            Debug.Log($"Found {nodes.Count} nodes with XPath: {xpath}");

            // 4. 准备正则过滤器
            Regex filter = GetCachedRegex(filterRegex);

            List<string> urlsToDownload = new List<string>();
            int processedCount = 0;

            // 5. 遍历节点，提取URL并应用正则过滤
            foreach (HtmlNode node in nodes)
            {
                string rawUrl = node.GetAttributeValue(attribute, string.Empty);
                if (string.IsNullOrEmpty(rawUrl)) continue;

                // URL规范化处理（参考DownloadMutiAsync中的逻辑）
                string absoluteUrl = NormalizeUrl(url, rawUrl);

                // 如果URL无效，跳过
                if (!Uri.IsWellFormedUriString(absoluteUrl, UriKind.Absolute))
                {
                    Debug.LogWarning($"Skipping invalid URL: {absoluteUrl}");
                    continue;
                }

                // 正则过滤
                if (filter != null && !filter.IsMatch(absoluteUrl))
                {
                    continue;
                }

                urlsToDownload.Add(absoluteUrl);
                processedCount++;

                // 限制匹配数量
                if (maxMatches > 0 && urlsToDownload.Count >= maxMatches)
                {
                    Debug.Log($"Reached maximum matches limit: {maxMatches}");
                    break;
                }
            }

            if (urlsToDownload.Count == 0)
            {
                Debug.LogError($"No URLs found after filtering. Processed {processedCount} nodes.");
                yield break;
            }

            Debug.Log($"Found {urlsToDownload.Count} URLs to download after filtering.");

            // 6. 批量下载处理
            for (int i = 0; i < urlsToDownload.Count; i++)
            {
                string fileUrl = urlsToDownload[i];

                // 生成目标路径（支持批量文件命名）
                string finalFilePath = GenerateTargetPath(saveFilePath, i, urlsToDownload.Count);

                Debug.Log($"Downloading [{i + 1}/{urlsToDownload.Count}]: {fileUrl}");
                Debug.Log($"Saving to: {finalFilePath}");

                // 调用优化的下载方法
                yield return DownloadWithRetry_Func(fileUrl, finalFilePath, timeout, maxRetries: 2);
            }

            Debug.Log($"Download completed. Total files: {urlsToDownload.Count}");
        }

        /// <summary>
        /// URL规范化辅助函数（将相对路径转换为绝对路径）
        /// </summary>
        /// <param name="baseUrl">基础URL（网页地址）</param>
        /// <param name="relativeOrAbsoluteUrl">相对或绝对URL</param>
        /// <returns>规范化后的绝对URL</returns>
        public string NormalizeUrl(string baseUrl, string relativeOrAbsoluteUrl)
        {
            if (string.IsNullOrEmpty(relativeOrAbsoluteUrl))
                return relativeOrAbsoluteUrl;

            // 处理协议相对URL（以"//"开头）
            if (relativeOrAbsoluteUrl.StartsWith("//"))
            {
                return "https:" + relativeOrAbsoluteUrl;
            }
            // 处理根相对路径（以"/"开头但不是"//"）
            else if (relativeOrAbsoluteUrl.StartsWith("/") && !relativeOrAbsoluteUrl.StartsWith("//"))
            {
                try
                {
                    Uri baseUri = new Uri(baseUrl);
                    return baseUri.Scheme + "://" + baseUri.Host + relativeOrAbsoluteUrl;
                }
                catch
                {
                    return relativeOrAbsoluteUrl;
                }
            }
            // 处理其他相对路径
            else if (!relativeOrAbsoluteUrl.StartsWith("http://") && !relativeOrAbsoluteUrl.StartsWith("https://"))
            {
                try
                {
                    Uri baseUri = new Uri(baseUrl);
                    Uri absoluteUri = new Uri(baseUri, relativeOrAbsoluteUrl);
                    return absoluteUri.ToString();
                }
                catch
                {
                    return relativeOrAbsoluteUrl;
                }
            }
            // 已经是绝对URL
            else
            {
                return relativeOrAbsoluteUrl;
            }
        }

        /// <summary>
        /// 取消当前协程下载
        /// </summary>
        public void CancelDownload()
        {
            if (_currentRequest != null && !_currentRequest.isDone)
            {
                _currentRequest.Abort();
                Debug.Log("Download aborted.");
            }
        }

        /// <summary>
        /// 缓存的Regex对象（避免重复编译）
        /// </summary>
        private static Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();

        /// <summary>
        /// 获取缓存的Regex对象,不存在则创建并缓存
        /// </summary>
        /// <param name="customRegex">自定义正则表达式</param>
        /// <returns>缓存的Regex对象</returns>
        private static Regex GetCachedRegex(string customRegex)
        {
            string regexPattern = !string.IsNullOrEmpty(customRegex) ? customRegex : fileUrlRegex.ToString();

            if (!_regexCache.TryGetValue(regexPattern, out Regex regex))
            {
                regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                _regexCache[regexPattern] = regex;
            }

            return regex;
        }

        /// <summary>
        /// 生成目标文件路径（支持批量文件自动编号）
        /// </summary>
        /// <param name="basePath">基础文件路径</param>
        /// <param name="index">当前文件索引</param>
        /// <param name="totalCount">总文件数量</param>
        /// <returns>生成的目标文件路径</returns>
        private static string GenerateTargetPath(string basePath, int index, int totalCount)
        {
            if (totalCount <= 1) return GetFullPath(basePath);

            // 为批量文件添加序号
            string directory = Path.GetDirectoryName(basePath);
            string fileName = Path.GetFileNameWithoutExtension(basePath);
            string extension = Path.GetExtension(basePath);

            string numberedFileName = $"{fileName}_{index + 1:000}{extension}";
            // 生成新的路径，如果原路径没有目录则直接使用编号文件名，否则组合目录和编号文件名
            string newPath = string.IsNullOrEmpty(directory) ? numberedFileName : Path.Combine(directory, numberedFileName);

            return GetFullPath(newPath);
        }

        /// <summary>
        /// 获取完整路径（安全处理）
        /// </summary>
        /// <param name="path">文件的绝对路径或相对路径("folder/file.txt")</param>
        /// <returns>Path.Combine(Application.persistentDataPath, path)</returns>
        private static string GetFullPath(string path)
        {
            // 如果路径已经是绝对路径则直接返回（可选安全检查）
            if (Path.IsPathRooted(path))
            {
                // 安全检查：防止目录遍历攻击
                string fullPath = Path.GetFullPath(path);
                // 可添加白名单检查
                return fullPath;
            }
            // 否则组合为持久化数据路径下的完整路径:‌C:\Users\<用户名>\AppData\LocalLow\<公司名>\<产品名>
            return Path.Combine(Application.persistentDataPath, path);
        }

        #endregion

        /// <summary>
        /// 异步解析单个节点并返回URL.
        /// </summary>
        /// <param name="url">网站的URL地址</param>
        /// <param name="node">HTML节点的XPath</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>返回节点解析后的URL</returns>
        public static async Task<string> LoadSingleNodeUrlAsync(string url, string node, int timeout = 60)
        {
            try
            {
                string html = await CreateRequestAsync(url, timeout);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode obj = doc.DocumentNode.SelectSingleNode(node);
                if (obj == null || !obj.Attributes.Contains("src"))
                {
                    return "";
                }
                return obj.Attributes["src"].Value;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading HTML from URL: {url} | Exception: {ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// 异步创建请求
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <returns>返回HTML内容字符串</returns>
        public static async Task<string> CreateRequestAsync(string url, int timeout = 60)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "Error: Invalid URL";
            }

            //return await Task.Run(() => LoadHtmlFromUrl(url, timeout));
            //直接调用异步方法,无需Task.Run
            return await LoadHtmlFromUrl(url, timeout);
        }
        /// <summary>
        /// 异步加载HTML内容
        /// </summary>
        /// <param name="url">网页URL</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>返回HTML内容字符串</returns>
        private static async Task<string> LoadHtmlFromUrl(string url, int timeout = 60)
        {
            //不需要跟HttpClient一样做单例,UnityWebRequest是一次性的请求对象,每次请求都需要创建新的实例.
            //单例模式适用需要共享状态或资源的对象(如HttpClient),而UnityWebRequest的设计就是为了每次请求独立使用,不需共享状态.直接创建新的UnityWebRequest实例即可,不必担心性能问题.
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = timeout;
                // 使用异步方式发送请求
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to load HTML: {request.error} | URL: {url}");
                    return null;
                }
                return request.downloadHandler.text;
            }
        }

        /// <summary>
        /// 异步下载文件并保存到本地
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="savePath">保存文件的完整路径（包含文件名）</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>是否下载成功</returns>
        public static async Task<bool> DownloadFileAsync(string url, string savePath, int timeout = 60)
        {
            // 创建请求对象
            UnityWebRequest request = UnityWebRequest.Get(url);

            // 配置下载处理器：直接写入磁盘，节省内存
            request.downloadHandler = new DownloadHandlerFile(savePath);

            request.timeout = timeout;

            // 关键设置：如果请求被中止或失败，自动删除已下载的部分文件，防止脏数据
            ((DownloadHandlerFile)request.downloadHandler).removeFileOnAbort = true;

            try
            {
                // 发送请求并异步等待完成
                // SendWebRequest 返回 AsyncOperation，在 Unity 2020+ 中可以直接 await
                await request.SendWebRequest();

                // 检查结果
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"文件下载成功: {savePath}");
                    return true;
                }
                else
                {
                    Debug.LogError($"文件下载失败: {request.error}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"下载过程中发生异常: {e.Message}");
                return false;
            }
            finally
            {
                // 无论成功与否，须手动释放资源
                request.Dispose();
            }
        }

        /// <summary>
        /// 异步下载多个文件并保存到本地
        /// </summary>
        /// <param name="urls">文件URL列表</param>
        /// <param name="saveDirectory">保存文件的目录</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns></returns>
        public static async Task DownloadFilesAsync(List<string> urls, string saveDirectory, int timeout = 60)
        {
            if (urls == null || urls.Count == 0)
            {
                Debug.LogWarning("没有提供要下载的URL列表!");
                return;
            }
            // 确保保存目录存在
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
            List<Task> downloadTasks = new List<Task>();
            for (int i = 0; i < urls.Count; i++)
            {
                string url = urls[i];
                string fileName = $"file_{i + 1:000}.jpg"; // 根据索引生成文件名
                string savePath = Path.Combine(saveDirectory, fileName);
                // 启动下载任务并添加到列表
                Task downloadTask = DownloadFileAsync(url, savePath, timeout);
                downloadTasks.Add(downloadTask);
            }
            // 等待所有下载任务完成
            await Task.WhenAll(downloadTasks);
            Debug.Log("所有文件下载完成!");
        }

        private static string _dftDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Download");
        /// <summary>
        /// 默认下载目录路径,初始值为桌面Download文件夹,可修改为其他路径以满足不同需求
        /// </summary>
        public static string DftDownloadPath
        {
            get { return _dftDownloadPath; }
            set { _dftDownloadPath = value; }
        }

        /// <summary>
        /// 异步下载指定网站的多个节点内容到指定目录并保存为自定义文件名.
        /// </summary>
        /// <param name="url">网站的URL地址</param>
        /// <param name="node">HTML节点的XPath,如"//img"</param>
        /// <param name="dirPath">下载目录路径,如 @"C:\Users\Admin\Desktop\Download</param>
        /// <param name="objectExtensionPattern">匹配文件扩展名的正则表达式(可修改):@"(?i)\.(gif|jpg|jpeg|png|jfif|webp)$"</param>
        /// <returns>下载是否成功</returns>
        public static async Task<bool> DownloadMutiAsync(string url, string node = "//img", string dirPath = null, string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png|jfif|webp)$", int timeout = 60)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = _dftDownloadPath;
            }

            Debug.Log($"开始下载，URL: {url}");
            Debug.Log($"XPath: {node}");
            Debug.Log($"保存目录: {dirPath}");

            try
            {
                string objUrl;
                string html = await CreateRequestAsync(url);
                Debug.Log($"HTML内容长度: {html?.Length ?? 0}");

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNodeCollection objNodes = doc.DocumentNode.SelectNodes(node);
                Debug.Log($"找到节点数量: {objNodes?.Count ?? 0}");

                if (objNodes != null)
                {
                    foreach (HtmlNode objNode in objNodes)
                    {
                        objUrl = objNode.GetAttributeValue("src", string.Empty);
                        Debug.Log($"发现图片URL: {objUrl}");

                        if (!string.IsNullOrEmpty(objUrl))
                        {
                            // 处理各种URL格式
                            if (objUrl.StartsWith("//"))
                            {
                                // 协议相对URL -> 添加https协议
                                objUrl = "https:" + objUrl;
                            }
                            else if (objUrl.StartsWith("/") && !objUrl.StartsWith("//"))
                            {
                                // 根相对路径 -> 拼接基础URL
                                Uri baseUri = new Uri(url);
                                objUrl = baseUri.Scheme + "://" + baseUri.Host + objUrl;
                            }
                            else if (!objUrl.StartsWith("http://") && !objUrl.StartsWith("https://"))
                            {
                                // 其他相对路径 -> 使用Uri类处理
                                Uri baseUri = new Uri(url);
                                Uri absoluteUri = new Uri(baseUri, objUrl);
                                objUrl = absoluteUri.ToString();
                            }
                            // 验证URL格式
                            if (!Uri.IsWellFormedUriString(objUrl, UriKind.Absolute))
                            {
                                MMCore.Tell($"警告: 跳过无效URL: {objUrl}");
                                continue;
                            }
                            if (Regex.IsMatch(Path.GetExtension(objUrl), objectExtensionPattern))
                            {
                                string fileName = Path.GetFileName(new Uri(objUrl).LocalPath);
                                Debug.Log($"fileName: {fileName}");
                                string fullPath = Path.Combine(dirPath, fileName);
                                Debug.Log($"下载到: {fullPath}");

                                bool success = await DownloadFileAsync(objUrl, fullPath, timeout);
                                //Debug.Log($"下载结果: {success}");
                            }
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}

//其他Mono实例类中使用示范
//UDown.Instance.Download("https://szcert.ebs.org.cn/Images/govIcon.gif", @"C:\Users\linsh\Desktop\Download\govIcon.gif");
//UDown.Instance.DownloadFilesWithRegex(url: "https://ac.qq.com/Comic/ComicInfo/id/542330", saveFilePath: @"C:\Users\linsh\Desktop\Download\A.jpg", xpath: "//img", attribute: "src", filterRegex: @"\.jpg$", maxMatches: 5);

#endif
