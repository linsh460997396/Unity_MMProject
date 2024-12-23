// using UnityEngine;
// using MetalMaxSystem;
// using HtmlAgilityPack;

// namespace Test.Example.Garbage
// {
// public class TEG_BiliCrawler : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
//         //XDown.Download(@"https://ac.qq.com/Comic/ComicInfo/id/542330", @"/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img", @"C:\Users\linsh\Desktop\Download\123.jpg");

//         HtmlDocument doc = new();
//         doc.LoadHtml(MMCore.CreateGetHttpResponse("https://ac.qq.com/Comic/ComicInfo/id/542330"));

//         HtmlNode obj = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img");
//         string objUal = obj.Attributes["src"].Value;
//         MMCore.Download(objUal, "123.jpg", @"C:\Users\linsh\Desktop\Download\", true);
//         Debug.Log("下载完成！");
//         MMCore.DelDirectory(@"C:\Users\linsh\Desktop\Download\temp");

//         //HtmlNodeCollection objNodes = doc.DocumentNode.SelectNodes("//img");
//         //if (objNodes != null)
//         //{
//         //    foreach (HtmlNode objNode in objNodes)
//         //    {
//         //        string objUrl = objNode.GetAttributeValue("src", string.Empty);
//         //        if (!string.IsNullOrEmpty(objUrl))
//         //        {
//         //            ////if (objUrl.EndsWith(".jfif", StringComparison.OrdinalIgnoreCase)) { }
//         //            //string filePath = Path.Combine(@"C:\Users\linsh\Desktop\Download\", Path.GetFileName(new Uri(objUrl).LocalPath));
//         //            //MMCore.Download(objUrl, Path.GetFileName(filePath), @"C:\Users\linsh\Desktop\Download\", true);
//         //            //Debug.Log($"filePath: {filePath}");
//         //            string objectExtension = Path.GetExtension(objUrl);
//         //            //string objectExtensionPattern = @"\.gif$";

//         //            string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png|jfif)$";
//         //            if (Regex.IsMatch(objectExtension, objectExtensionPattern))
//         //            {
//         //                string filePath = Path.Combine(@"C:\Users\linsh\Desktop\Download\", Path.GetFileName(new Uri(objUrl).LocalPath));
//         //                MMCore.Download(objUrl, Path.GetFileName(new Uri(objUrl).LocalPath), @"C:\Users\linsh\Desktop\Download\", true);
//         //                Debug.Log($"filePath: {filePath}");
//         //                MMCore.DelDirectory(@"C:\Users\linsh\Desktop\Download\temp");
//         //            }

//         //            //string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png)$";
//         //            //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern);
//         //            //string objectExtensionPattern = @"\.(gif|jpg|jpeg|png)$";
//         //            //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern, RegexOptions.IgnoreCase);
//         //        }
//         //    }
//         //}
//         //else
//         //{
//         //    Debug.Log("");
//         //}
//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }
// }
// }