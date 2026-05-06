//using UnityEngine;
//using MetalMaxSystem;
//using HtmlAgilityPack;
//using System.Text.RegularExpressions;
//using System.IO;
//using System;

//namespace Test.Example.Garbage
//{
//    public class TEG_Web : MonoBehaviour
//    {
//        async void Start()
//        {
//            await UDown.DownloadMutiAsync("https://ac.qq.com/Comic/ComicInfo/id/542330");
//            List<string> urls = new List<string> { "https://szcert.ebs.org.cn/Images/govIcon.gif" };
//            await UDown.DownloadFilesAsync(urls, @"C:\Users\linsh\Desktop\Download");
//            UDown.Instance.Download("https://szcert.ebs.org.cn/Images/govIcon.gif", @"C:\Users\linsh\Desktop\Download\govIcon.gif");
//            UDown.Instance.DownloadWithRetry("https://szcert.ebs.org.cn/Images/govIcon.gif", @"C:\Users\linsh\Desktop\Download\govIcon.gif");
//            UDown.Instance.DownloadFilesWithRegex(url: "https://ac.qq.com/Comic/ComicInfo/id/542330", saveFilePath: @"C:\Users\linsh\Desktop\Download\A.jpg", xpath: "//img", attribute: "src", filterRegex: @"\.jpg$", maxMatches: 5);

//            await NetHelper.DownloadMutiAsync("https://ac.qq.com/Comic/ComicInfo/id/542330");
//            NetHelper.DownloadMuti("https://ac.qq.com/Comic/ComicInfo/id/542330");
//            NetHelper.SaveBiliDanmuToDesktop(await NetHelper.GetBiliDanmuAsync(31292268));
//            await NetHelper.DownloadAsync(@"https://ac.qq.com/Comic/ComicInfo/id/542330", @"/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img", @"C:\Users\linsh\Desktop\Download\1.jpg");
//            NetHelper.Download(@"https://ac.qq.com/Comic/ComicInfo/id/542330", @"/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img", @"C:\Users\linsh\Desktop\Download\2.jpg");

//            HtmlDocument doc = new HtmlDocument();
//            doc.LoadHtml(NetHelper.Get("https://ac.qq.com/Comic/ComicInfo/id/542330"));
//            HtmlNode obj = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img");
//            string objUal = obj.Attributes["src"].Value;
//            NetHelper.Download_Func(objUal, "3.jpg", @"C:\Users\linsh\Desktop\Download");
//            Debug.Log("下载完成！");

//            //从已加载到内存中的HTML文档里提取所有符合特定图片格式(gif, jpg, jpeg, png, jfif)的<img> 标签的src地址并将这些图片下载到本地指定文件夹‌
//            //在用SelectNodes前须先调用doc.LoadHtml方法将网页HTML源代码加载进doc对象
//            HtmlNodeCollection objNodes = doc.DocumentNode.SelectNodes("//img");
//            if (objNodes != null)
//            {
//                string objUrl, objectExtension, objectExtensionPattern, filePath;
//                string dftDownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Download");
//                foreach (HtmlNode objNode in objNodes)
//                {
//                    objUrl = objNode.GetAttributeValue("src", string.Empty);
//                    if (!string.IsNullOrEmpty(objUrl))
//                    {
//                        //if (objUrl.EndsWith(".jfif", StringComparison.OrdinalIgnoreCase)) { }
//                        //string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png)$";
//                        //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern);
//                        //string objectExtensionPattern = @"\.(gif|jpg|jpeg|png)$";
//                        //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern, RegexOptions.IgnoreCase);

//                        objectExtension = Path.GetExtension(objUrl);
//                        objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png|jfif)$";
//                        if (Regex.IsMatch(objectExtension, objectExtensionPattern))
//                        {
//                            filePath = Path.Combine(dftDownloadPath, Path.GetFileName(new Uri(objUrl).LocalPath));
//                            NetHelper.Download_Func(objUrl, Path.GetFileName(new Uri(objUrl).LocalPath), dftDownloadPath);
//                            Debug.Log($"filePath: {filePath}");
//                        }
//                    }
//                }
//            }
//            else
//            {
//                Debug.Log("objNodes = null");
//            }
//        }
//    }
//}