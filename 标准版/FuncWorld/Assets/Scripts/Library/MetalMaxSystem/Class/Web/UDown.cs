//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace MetalMaxSystem.Unity
{
    public class UDown : MonoBehaviour
    {
        public static Regex fileUrlRegex = new Regex("\"(https?://[^\"\\s]+\\.jpg)\"", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 开始下载文件
        /// </summary>
        public void Download(string url, string savePath)
        {
            StartCoroutine(DownloadFileAsync(url, savePath));
        }

        /// <summary>
        /// 协程异步:直接下载指定URL文件.
        /// </summary>
        private IEnumerator DownloadFileAsync(string url, string savePath)
        {
            // 构建完整路径
            string fullPath = Path.Combine(Application.persistentDataPath, savePath);

            // 确保目录存在
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // 使用DownloadHandlerFile直接写入磁盘,避免大文件占用大量内存
                webRequest.downloadHandler = new DownloadHandlerFile(fullPath);

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Download Error: {webRequest.error}");

                    // 下载失败时清理可能产生的空文件或损坏文件
                    if (File.Exists(fullPath))
                    {
                        try { File.Delete(fullPath); } catch { }
                    }
                }
                else
                {
                    Debug.Log("File downloaded and saved to: " + fullPath);
                }
            }
        }

        /// <summary>
        /// 协程异步:从网页提取文件链接,解析后下载.
        /// </summary>
        public IEnumerator DownloadCoroutine(string url, string targetFilePath, string objectRegex = null)
        {
            // 下载HTML内容
            string htmlContent;
            using (UnityWebRequest htmlRequest = UnityWebRequest.Get(url))
            {
                yield return htmlRequest.SendWebRequest();

                if (htmlRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download HTML: {htmlRequest.error}");
                    yield break;
                }

                htmlContent = htmlRequest.downloadHandler.text;
            }

            // 解析文件URL
            Regex regexToUse = !string.IsNullOrEmpty(objectRegex) ? new Regex(objectRegex) : fileUrlRegex;
            Match match = regexToUse.Match(htmlContent);

            if (!match.Success)
            {
                Debug.LogError("Failed to find object URL in HTML content.");
                yield break;
            }

            // Groups[1]代表第一个捕获组(括号内的内容)
            string imageUrl = match.Groups[1].Value;

            // 下载文件并处理目标路径:若传入的是相对路径则结合persistentDataPath,否则绝对路径直接用
            string finalImagePath;
            if (Path.IsPathRooted(targetFilePath))
            {
                finalImagePath = targetFilePath;
            }
            else
            {
                finalImagePath = Path.Combine(Application.persistentDataPath, targetFilePath);
            }

            // 确保文件保存目录存在
            string imageDirectory = Path.GetDirectoryName(finalImagePath);
            if (!string.IsNullOrEmpty(imageDirectory) && !Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            using (UnityWebRequest imageRequest = UnityWebRequest.Get(imageUrl))
            {
                // 同样使用 DownloadHandlerFile 以节省内存
                imageRequest.downloadHandler = new DownloadHandlerFile(finalImagePath);

                yield return imageRequest.SendWebRequest();

                if (imageRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download image: {imageRequest.error}");
                    // 清理失败的文件
                    if (File.Exists(finalImagePath))
                    {
                        try { File.Delete(finalImagePath); } catch { }
                    }
                }
                else
                {
                    Debug.Log("Object downloaded and saved to: " + finalImagePath);
                }
            }
        }
    }
}

#endif
