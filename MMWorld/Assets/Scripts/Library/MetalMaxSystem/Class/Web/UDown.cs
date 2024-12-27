#if UNITY_EDITOR || UNITY_STANDALONE
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace MetalMaxSystem.Unity
{
    public class UDown : MonoBehaviour
    {
        /// <summary>
        /// 开始下载文件
        /// </summary>
        /// <param name="url">文件的URL</param>
        /// <param name="savePath">保存文件的路径</param>
        public void Download(string url, string savePath)
        {
            StartCoroutine(DownloadFile(url, savePath));
        }

        /// <summary>
        /// 采用协程（异步）的方式下载文件
        /// </summary>
        /// <param name="fileUrl">文件的URL</param>
        /// <param name="savePath">保存文件的路径</param>
        /// <returns></returns>
        private IEnumerator DownloadFile(string fileUrl, string savePath)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(fileUrl))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(webRequest.error);
                    yield break;
                }

                byte[] fileData = webRequest.downloadHandler.data;
                string filePath = Path.Combine(Application.persistentDataPath, savePath);
                File.WriteAllBytes(filePath, fileData);

                Debug.Log("File downloaded and saved to: " + filePath);
            }
        }

        public IEnumerator DownloadCoroutine(string url, string targetFilePath, string objectRegex = "\"(http[s]?://.*?jpg)\"")
        {
            //使用WebClient下载网页内容
            using (WebClient client = new WebClient())
            {
                string htmlContent = client.DownloadString(url);

                //使用正则表达式提取图片URL
                Match match = Regex.Match(htmlContent, objectRegex);
                if (match.Success)
                {
                    string objectUrl = match.Groups[1].Value;

                    //下载图片并保存到本地
                    byte[] objectBytes = client.DownloadData(objectUrl);
                    File.WriteAllBytes(targetFilePath, objectBytes);

                    Debug.Log("Object downloaded and saved to: " + targetFilePath);
                }
                else
                {
                    Debug.LogError("Failed to find object URL in HTML content.");
                }
            }

            yield return null;
        }
    }
}
#endif
