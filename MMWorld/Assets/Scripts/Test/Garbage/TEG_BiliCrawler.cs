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

//         //��Ԫ�����أ�
//         HtmlNode obj = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img");
//         string objUal = obj.Attributes["src"].Value;
//         MMCore.Download(objUal, "123.jpg", @"C:\Users\linsh\Desktop\Download\", true);
//         Debug.Log("������ɣ�");
//         MMCore.DelDirectory(@"C:\Users\linsh\Desktop\Download\temp");

//         //����������ҳȫ������Ԫ�أ�
//         //HtmlNodeCollection objNodes = doc.DocumentNode.SelectNodes("//img");
//         //if (objNodes != null)
//         //{
//         //    foreach (HtmlNode objNode in objNodes)
//         //    {
//         //        string objUrl = objNode.GetAttributeValue("src", string.Empty);
//         //        if (!string.IsNullOrEmpty(objUrl))
//         //        {
//         //            ////������URL����չ���Ƿ�Ϊ.jfif
//         //            ////if (objUrl.EndsWith(".jfif", StringComparison.OrdinalIgnoreCase)) { }
//         //            //string filePath = Path.Combine(@"C:\Users\linsh\Desktop\Download\", Path.GetFileName(new Uri(objUrl).LocalPath));
//         //            //MMCore.Download(objUrl, Path.GetFileName(filePath), @"C:\Users\linsh\Desktop\Download\", true);
//         //            //Debug.Log($"�������: {filePath}");

//         //            // ��ȡ������չ��
//         //            string objectExtension = Path.GetExtension(objUrl);
//         //            // �����������ʽ����
//         //            //string objectExtensionPattern = @"\.gif$";

//         //            string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png|jfif)$";
//         //            //(? i)������һ������ģʽ���η�������ָ����������ƥ��Ӧ���ǲ����ִ�Сд�ġ�
//         //            //\.��ƥ��һ������ĵ�ţ�.������Ϊ���������ʽ�е����һ�������ַ���������Ҫ�÷�б�ܽ���ת�塣
//         //            //(gif | jpg | jpeg | png)������һ�������飬��ƥ��gif��jpg��jpeg��png�е��κ�һ����| ���߼�������������ڷָ���ͬ��ѡ�
//         //            //$��ƥ���ַ����Ľ�β��ȷ��������չ����ƥ�䲢��û�������ַ��������

//         //            // ��������չ���Ƿ�ƥ��.gif, .jpg, .jpeg, .png�ȣ������ִ�Сд��
//         //            if (Regex.IsMatch(objectExtension, objectExtensionPattern))
//         //            {
//         //                string filePath = Path.Combine(@"C:\Users\linsh\Desktop\Download\", Path.GetFileName(new Uri(objUrl).LocalPath));
//         //                MMCore.Download(objUrl, Path.GetFileName(new Uri(objUrl).LocalPath), @"C:\Users\linsh\Desktop\Download\", true);
//         //                Debug.Log($"�������: {filePath}");
//         //                MMCore.DelDirectory(@"C:\Users\linsh\Desktop\Download\temp");
//         //            }

//         //            //��ģʽ�ַ�����ָ�������ִ�Сд��
//         //            //string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png)$";
//         //            //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern);
//         //            //��Regex.IsMatch������ָ�������ִ�Сд��
//         //            //string objectExtensionPattern = @"\.(gif|jpg|jpeg|png)$";
//         //            //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern, RegexOptions.IgnoreCase);
//         //            //���ַ�������õ���ͬ�Ľ��������һ�ַ������Ӽ���ֱ��
//         //        }
//         //    }
//         //}
//         //else
//         //{
//         //    Debug.Log("û���ҵ�����Ԫ�ء�");
//         //}
//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }
// }
// }