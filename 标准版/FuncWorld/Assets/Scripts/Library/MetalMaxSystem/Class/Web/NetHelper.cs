using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetalMaxSystem
{
    public static class NetHelper
    {
        
        private static string _dftUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.50";
        /// <summary>
        /// 默认User-Agent字符串,模拟常见浏览器以提高兼容性
        /// </summary>
        public static string DftUserAgent
        {
            get { return _dftUserAgent; }
            set { _dftUserAgent = value; }
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

        private static HttpClient _httpClient = new HttpClient()
        {
            // 设置默认超时时间
            Timeout = TimeSpan.FromSeconds(10)
        };
        /// <summary>
        /// 网络端口实例.
        /// 允许外部配置如添加默认请求头、调整超时等.
        /// 无必要时尽可能复用该实例,防止端口消耗殆尽,保持资源管理和性能优势.
        /// </summary>
        public static HttpClient HttpClient
        {
            get { return _httpClient; }
            set { _httpClient = value; }
        }


        // 静态构造函数,在字段初始化后执行
        static NetHelper()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_dftUserAgent);

            // 可接受所有MIME类型以提高兼容性,但可能增加某些请求响应时间,据实际需求启用
            // _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }

        /// <summary>
        /// 异步创建GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> CreateGetHttpResponseAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "Error: Invalid URL";
            }

            try
            {
                // 用HttpClient发送异步GET请求,并确保成功响应
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                // 确保响应状态码为成功,否则抛出异常
                response.EnsureSuccessStatusCode();
                // 异步读取并返回响应内容
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // 改进错误处理：返回具体错误信息，而不是打印到控制台
                return $"Error: Network - {ex.Message}";
            }
            catch (TaskCanceledException ex)
            {
                return $"Error: Timeout - {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: Unexpected - {ex.Message}";
            }
        }

        /// <summary>
        /// 异步发送POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">表单参数</param>
        /// <returns>响应内容</returns>
        public static async Task<string> CreatePostHttpResponseAsync(string url, IDictionary<string, string> parameters)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));
            }

            try
            {
                // 用FormUrlEncodedContent自动处理键值对编码和Content-Type
                var content = new FormUrlEncodedContent(parameters);

                // 发送异步POST请求
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);

                // 确保响应状态码为成功,否则抛出异常
                response.EnsureSuccessStatusCode();

                // 异步读取并返回响应内容
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // 网络错误、DNS失败、非2xx状态码等
                return $"Error: Network - {ex.Message}";
            }
            catch (TaskCanceledException ex)
            {
                // 超时
                return $"Error: Timeout - {ex.Message}";
            }
            catch (Exception ex)
            {
                // 其他未知错误
                return $"Error: Unexpected - {ex.Message}";
            }
        }

        /// <summary>
        /// 异步下载指定网站的指定节点内容到指定目录并保存为自定义文件名.
        /// </summary>
        /// <param name="htmlAttributeValue">解析后的网页节点对象属性值</param>
        /// <param name="fileName">要保存的文件名</param>
        /// <param name="dirPath">下载目录路径,如 @"C:\Users\Admin\Desktop\Download"</param>
        /// <returns>下载是否成功</returns>
        public static async Task<bool> DownloadFuncAsync(string htmlAttributeValue, string fileName, string dirPath)
        {
            if (string.IsNullOrWhiteSpace(htmlAttributeValue))
            {
                MMCore.Tell("错误: htmlAttributeValue为空");
                return false;
            }
            // 确保是绝对路径
            if (!Uri.IsWellFormedUriString(htmlAttributeValue, UriKind.Absolute))
            {
                MMCore.Tell($"错误: htmlAttributeValue '{htmlAttributeValue}' 不是有效绝对URI");
                return false;
            }
            string tempPath = Path.Combine(dirPath, "temp"); //临时目录全名路径
            string filePath = Path.Combine(dirPath, fileName); //最终文件全名路径
            string tempFile = Path.Combine(tempPath, fileName + ".temp"); //临时文件全名路径
            Directory.CreateDirectory(tempPath); //创建临时目录

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile); //临时文件存在则删除
            }

            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(htmlAttributeValue, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }

                // 移动临时文件到目标路径
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempFile, filePath);

                return true;
            }
            catch (Exception ex)
            {
                MMCore.Tell($"错误: {ex.Message}");
                return false;
            }
            finally
            {
                // 清理临时目录
                try
                {
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }
                }
                catch
                {
                    // 备用强制清理
                    try
                    {
                        MMCore.DelDirectoryRecursively(tempPath);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 异步下载指定网站的指定节点内容到指定目录并保存为自定义文件名.
        /// </summary>
        /// <param name="url">网站的URL地址</param>
        /// <param name="node">HTML节点的XPath</param>
        /// <param name="path">保存文件路径全名</param>
        /// <returns>下载是否成功</returns>
        public static async Task<bool> DownloadAsync(string url, string node, string path)
        {
            try
            {
                string html = await CreateGetHttpResponseAsync(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNode obj = doc.DocumentNode.SelectSingleNode(node);
                if (obj == null || !obj.Attributes.Contains("src"))
                {
                    return false;
                }
                string htmlAttributeValue = obj.Attributes["src"].Value;
                string fileName = Path.GetFileName(path);
                string dirPath = Path.GetDirectoryName(path);
                return await DownloadFuncAsync(htmlAttributeValue, fileName, dirPath);

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 异步下载指定网站的指定节点内容到指定目录并保存为自定义文件名.
        /// </summary>
        /// <param name="url">网站的URL地址</param>
        /// <param name="node">HTML节点的XPath,如"//img"</param>
        /// <param name="dirPath">下载目录路径,如 @"C:\Users\Admin\Desktop\Download</param>
        /// <param name="objectExtensionPattern">匹配文件扩展名的正则表达式(可修改):@"(?i)\.(gif|jpg|jpeg|png|jfif|webp)$"</param>
        /// <returns>下载是否成功</returns>
        public static async Task<bool> DownloadMutiAsync(string url, string node = "//img", string dirPath = null, string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png|jfif|webp)$")
        {
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = _dftDownloadPath;
            }
            try
            {
                string objUrl;
                string html = await CreateGetHttpResponseAsync(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                HtmlNodeCollection objNodes = doc.DocumentNode.SelectNodes(node);
                if (objNodes != null)
                {
                    foreach (HtmlNode objNode in objNodes)
                    {
                        objUrl = objNode.GetAttributeValue("src", string.Empty);
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
                                await DownloadFuncAsync(objUrl, Path.GetFileName(new Uri(objUrl).LocalPath), dirPath);
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

        /// <summary>
        /// 获取B站直播间最近弹幕（无需Cookie）
        /// </summary>
        /// <param name="roomId">直播间房间号,https://live.bilibili.com/{roomId}?...</param>
        public static async Task<List<DanmuItem>> GetBiliDanmuAsync(long roomId)
        {// 注释掉的旧代码用到System.Text.Json、System.Net.Http.Json,现改用 Newtonsoft.Json解析

            // 接口地址（B站开放API，无需登录）
            string url = $"https://api.live.bilibili.com/xlive/web-room/v1/dM/gethistory?roomid={roomId}";

            // 213是C酱直播间房间号
            // 直播间网址https://live.bilibili.com/{roomId}?...

            // 发送GET请求获取弹幕数据
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // 解析JSON响应
            //var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var json = JObject.Parse(await response.Content.ReadAsStringAsync());

            // 解析弹幕列表
            var danmuList = new List<DanmuItem>();
            //var rooms = json.GetProperty("data").GetProperty("room");
            var rooms = json["data"]?["room"];
            //if (rooms.ValueKind == JsonValueKind.Null)
            if (rooms == null || !rooms.HasValues) return danmuList;
            //foreach (var msg in rooms.EnumerateArray())
            foreach (var msg in rooms)
            {
                danmuList.Add(new DanmuItem
                {
                    //Text = msg.GetProperty("text").GetString(),
                    //UserName = msg.GetProperty("nickname").GetString(),
                    //Timeline = msg.GetProperty("timeline").GetString()

                    Text = msg["text"]?.ToString(),
                    UserName = msg["nickname"]?.ToString(),
                    Timeline = msg["timeline"]?.ToString()
                });
            }

            return danmuList;
        }

        /// <summary>
        /// 将B站弹幕列表保存到桌面
        /// </summary>
        /// <param name="danmuList">弹幕数据列表</param>
        /// <param name="fileName">文件名(包含时间戳)</param>
        public static void SaveBiliDanmuToDesktop(List<DanmuItem> danmuList, string fileName = null)
        {
            if (danmuList == null || danmuList.Count == 0)
            {
                // 没有弹幕数据可保存
                return;
            }

            // 获取桌面路径(当运行C#程序时该进程是依附于当前登录的‌用户会话运行)
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // 构建完整文件路径
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"BiliDanmu_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            }
            else if (!fileName.EndsWith(".json"))
            {
                fileName += ".json";
            }

            string fullPath = Path.Combine(desktopPath, fileName);

            try
            {
                // 序列化数据为JSON字符串 (Formatting.Indented 使文件可读性更好)
                string jsonContent = JsonConvert.SerializeObject(danmuList, Formatting.Indented);

                // 写入文件
                File.WriteAllText(fullPath, jsonContent, System.Text.Encoding.UTF8);

                MMCore.Tell($"成功保存 {danmuList.Count} 条弹幕到桌面: {fullPath}");
            }
            catch (Exception ex)
            {
                MMCore.Tell($"保存文件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 将B站弹幕列表保存到桌面
        /// </summary>
        /// <param name="roomId">直播间房间号,https://live.bilibili.com/{roomId}?...</param>
        /// <returns></returns>
        public static async Task SaveBiliDanmuToDesktop(long roomId)
        {
            SaveBiliDanmuToDesktop(await GetBiliDanmuAsync(roomId));
        }

        /// <summary>
        /// 将相对URL转换为绝对URL
        /// </summary>
        /// <param name="relativeUrl">相对URL</param>
        /// <param name="baseUrl">基础URL</param>
        /// <returns>绝对URL</returns>
        public static string ConvertToAbsoluteUrl(string relativeUrl, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl))
                return relativeUrl;

            // 如果已经是绝对URL，直接返回
            if (Uri.IsWellFormedUriString(relativeUrl, UriKind.Absolute))
                return relativeUrl;

            // 处理协议相对URL（//example.com/path）
            if (relativeUrl.StartsWith("//"))
                return "https:" + relativeUrl;

            // 处理相对路径（/path 或 path）
            Uri baseUri = new Uri(baseUrl);
            Uri absoluteUri = new Uri(baseUri, relativeUrl);
            return absoluteUri.ToString();
        }

        #region 旧方法(HttpWebRequest)
        //微软官方建议完全弃用HttpWebRequest,改用HttpClient,它能设计单例或长期存活服务,性能更好,资源管理更简单.

        /// <summary>
        /// 创建GET请求
        /// </summary>
        /// <param name="url">网址</param>
        /// <param name="timeout">超时毫秒数</param>
        /// <returns>响应内容</returns>
        [Obsolete("HttpWebRequest is obsolete. Use CreateGetHttpResponseAsync of HttpClient instead.")]
        public static string CreateGetHttpResponse(string url, int timeout = 10000)
        {
            if (string.IsNullOrWhiteSpace(url)) return "requestFalse";
            HttpWebRequest request = null; HttpWebResponse response = null;
            try
            {
                //创建请求
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.UserAgent = _dftUserAgent;
                request.Timeout = timeout;
                //创建响应
                response = (HttpWebResponse)request.GetResponse();

                //using确保流和响应被正确关闭
                using (Stream stream = response.GetResponseStream())
                {
                    //尝试从响应头获取编码,若获取不到则默认UTF-8
                    Encoding encoding = Encoding.UTF8;
                    if (!string.IsNullOrEmpty(response.CharacterSet))
                    {
                        try
                        {
                            // 使用响应头中的字符集名称获取编码,而非默认的UTF-8,以正确处理不同编码的网页
                            encoding = Encoding.GetEncoding(response.CharacterSet);
                        }
                        catch
                        {
                            // 字符集名称无效,按UTF-8
                        }
                    }
                    using (StreamReader reader = new StreamReader(stream, encoding))
                    {
                        // 使用正确的编码读取响应内容
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                // 区分网络错误和协议错误
                return $"Error: {ex.Status} - {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
            finally
            {
                // 确保即使发生异常,响应对象也被释放
                response?.Close();
            }
        }

        /// <summary>
        /// 创建POST请求
        /// </summary>
        /// <param name="url">请求的URL地址</param>
        /// <param name="parameters">请求的参数字典</param>
        /// <param name="timeout">请求超时时间(单位毫秒)</param>
        /// <param name="contentType">请求的内容类型,如"application/x-www-form-urlencoded"是表单数据</param>
        /// <returns>响应内容</returns>
        /// <exception cref="ArgumentException">URL为空或无效时抛出</exception>
        /// <exception cref="InvalidOperationException">HTTP请求失败时抛出</exception>
        [Obsolete("HttpWebRequest is obsolete. Use CreatePostHttpResponseAsync of HttpClient instead.")]
        public static string CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int timeout = 10000, string contentType = "application/x-www-form-urlencoded")
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be null or empty.", nameof(url));
            HttpWebRequest request = null; HttpWebResponse response = null;
            try
            {
                // 创建请求
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.UserAgent = _dftUserAgent;
                request.ContentType = contentType;
                request.Timeout = timeout;
                // 增加读写超时设置,避免在网络不稳定时长时间挂起
                request.ReadWriteTimeout = timeout;
                // 构建表单数据
                if (parameters != null && parameters.Count > 0)
                {
                    StringBuilder buffer = new StringBuilder();
                    int i = 0;
                    foreach (var key in parameters.Keys)
                    {
                        if (i > 0) buffer.Append("&");
                        // 用Uri.EscapeDataString对传入字符串进行编码,确保其符合URI 规范,以正确处理特殊字符和非ASCII字符
                        buffer.Append($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(parameters[key])}");
                        i++;
                    }
                    // 用UTF-8编码字节而非ASCII
                    byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
                    // 显式设置长度有助于某些服务器处理
                    request.ContentLength = data.Length;
                    // 写入请求流
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }
                // 获取响应
                response = (HttpWebResponse)request.GetResponse();
                // 尝试从响应头获取编码,若获取不到则默认UTF-8
                Encoding encoding = GetResponseEncoding(response);
                // 使用正确的编码读取响应内容
                using (Stream stream = response.GetResponseStream())
                {
                    // 使用响应头中的字符集名称获取编码,而非默认的UTF-8,以正确处理不同编码的网页
                    using (StreamReader reader = new StreamReader(stream, encoding))
                    {
                        // 使用正确的编码读取响应内容
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                string status = ex.Status.ToString();
                if (ex.Response is HttpWebResponse errorResponse)
                {
                    status += $" (HTTP {(int)errorResponse.StatusCode})";
                    errorResponse.Close();
                }
                throw new InvalidOperationException($"HTTP POST Request Failed: {status} - {ex.Message}", ex);
            }
            finally
            {
                response?.Close();
            }
        }

        /// <summary>
        /// 从响应头获取编码,默认UTF-8
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        [Obsolete("HttpWebRequest is obsolete. Use HttpClient instead.")]
        public static Encoding GetResponseEncoding(HttpWebResponse response)
        {
            Encoding encoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(response.CharacterSet))
            {
                try
                {
                    encoding = Encoding.GetEncoding(response.CharacterSet);
                }
                catch
                {
                    // 若字符集名称无效,回退到 UTF-8
                }
            }
            return encoding;
        }

        /// <summary>
        /// 下载指定网站的指定节点内容到指定目录并保存为自定义文件名.
        /// HttpWebRequest方式下载文件,适用于下载较大文件或需要更细粒度控制的场景.
        /// 缺点是代码较为冗长,且需要手动处理流和异常,不如HttpClient方式简洁和高效.建议在需要兼容旧环境或特定需求时使用,否则推荐使用DownloadFileAsync方法.
        /// 使用范例:
        /// HtmlDocument doc = new HtmlDocument();
        /// doc.LoadHtml(NetHelper.CreateGetHttpResponse("https://ac.qq.com/Comic/ComicInfo/id/542330"));
        /// HtmlNode img = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img");
        /// string imgUal = img.Attributes["src"].Value;
        /// NetHelper.Download(imgUal, "123.jpg", @"C:\Users\Admin\Desktop\Download");
        /// </summary>
        /// <param name="htmlAttributeValue">解析后的网页节点对象属性值</param>
        /// <param name="fileName">要保存的文件名</param>
        /// <param name="dirPath">下载目录路径(末尾不带斜杠),如 @"C:\Users\Admin\Desktop\Download"</param>
        /// <returns>下载是否成功</returns>
        [Obsolete("HttpWebRequest is obsolete. Use DownloadFuncAsync of HttpClient instead.")]
        public static bool DownloadFunc(string htmlAttributeValue, string fileName, string dirPath)
        {
            if (string.IsNullOrWhiteSpace(htmlAttributeValue))
            {
                MMCore.Tell("错误: htmlAttributeValue为空");
                return false;
            }
            // 确保是绝对路径
            if (!Uri.IsWellFormedUriString(htmlAttributeValue, UriKind.Absolute))
            {
                MMCore.Tell($"错误: htmlAttributeValue '{htmlAttributeValue}' 不是有效绝对URI");
                return false;
            }
            string tempPath = Path.Combine(dirPath, "temp"); //临时目录全名路径
            string filePath = Path.Combine(dirPath, fileName); //最终文件全名路径
            string tempFile = Path.Combine(tempPath, fileName + ".temp"); //临时文件全名路径
            Directory.CreateDirectory(tempPath); //创建临时目录
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile); //临时文件存在则删除
            }
            FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            try
            {
                //设置参数
                HttpWebRequest request = WebRequest.Create(htmlAttributeValue) as HttpWebRequest;
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse(),程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                responseStream.Close();
                responseStream.Dispose();
            }
            catch (Exception ex)
            {
                MMCore.Tell(string.Format("错误: {0}", ex.Message));
                return false;
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
            // 只有下载成功,文件流fs已关闭且不占用文件,代码才会走到这里
            try
            {
                File.Move(tempFile, filePath);
            }
            catch
            {
                File.Delete(filePath); File.Move(tempFile, filePath);
            }
            try
            {
                MMCore.DelDirectory(tempPath);
            }
            catch
            {
                MMCore.DelDirectoryRecursively(tempPath);
            }

            return true;
        }

        /// <summary>
        /// 下载指定网站的指定节点内容到指定目录并保存为自定义文件名.
        /// 示例：NetHelper.XDownload(@"https://ac.qq.com/Comic/ComicInfo/id/542330", @"/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img", @"C:\Users\Admin\Desktop\Download\123.jpg");
        /// </summary>
        /// <param name="url">网站的URL地址</param>
        /// <param name="node">HTML节点的XPath</param>
        /// <param name="path">保存文件路径全名"</param>
        /// <returns>下载是否成功</returns>
        [Obsolete("HttpWebRequest is obsolete. Use DownloadAsync of HttpClient instead.")]
        public static bool Download(string url, string node, string path)
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(CreateGetHttpResponse(url));
                HtmlNode obj = doc.DocumentNode.SelectSingleNode(node);
                string dirPath = string.Concat(Path.GetDirectoryName(path));
                return DownloadFunc(obj.Attributes["src"].Value, Path.GetFileName(path), dirPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 下载指定网站的指定节点内容到指定目录并保存为自定义文件名.
        /// </summary>
        /// <param name="url">网站的URL地址</param>
        /// <param name="node">HTML节点的XPath,如"//img"</param>
        /// <param name="dirPath">下载目录路径,如 @"C:\Users\Admin\Desktop\Download</param>
        /// <param name="objectExtensionPattern">匹配文件扩展名的正则表达式(可修改):@"(?i)\.(gif|jpg|jpeg|png|jfif|webp)$"</param>
        /// <returns>下载是否成功</returns>
        [Obsolete("HttpWebRequest is obsolete. Use DownloadMutiAsync of HttpClient instead.")]
        public static bool DownloadMuti(string url, string node = "//img", string dirPath = null, string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png|jfif|webp)$")
        {
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                dirPath = _dftDownloadPath;
            }
            try
            {
                //从已加载到内存中的HTML文档里提取所有符合特定格式如图片gif、jpg、jpeg、 png、jfif的<img>标签的src地址并将这些下载到本地指定文件夹‌
                //在用SelectNodes前须先调用doc.LoadHtml方法将网页HTML源代码加载进doc对象

                string objUrl;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(CreateGetHttpResponse(url));
                HtmlNodeCollection objNodes = doc.DocumentNode.SelectNodes(node);
                if (objNodes != null)
                {
                    foreach (HtmlNode objNode in objNodes)
                    {
                        objUrl = objNode.GetAttributeValue("src", string.Empty);
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
                                DownloadFunc(objUrl, Path.GetFileName(new Uri(objUrl).LocalPath), dirPath);
                            }
                        }
                    }
                }
                //else
                //{
                //    MMCore.Tell("objNodes = null");
                //}
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    public class DanmuItem
    {
        public string Text { get; set; }
        public string UserName { get; set; }
        public string Timeline { get; set; }
    }
}
