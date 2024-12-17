//using System.Collections;
//using System.IO;
//using UnityEngine;

//namespace Test.Example.Garbage
//{
//    public class TEG_LargeTextFileReader : MonoBehaviour
//    {
//        private string filePath = Application.dataPath + "/MapIndex/地图纹理编号.txt";
//        public string fileContent = "";
//        private const int bytesReadPerFrame = 1024; // 每帧读取的字节数
//        int i = 0;

//        void StartReadFileInChunks()
//        {
//            StartCoroutine(ReadFileInChunks());
//        }

//        IEnumerator ReadFileInChunks()
//        {
//            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
//            {
//                byte[] buffer = new byte[bytesReadPerFrame];
//                int bytesRead;

//                // 由于文件可能包含多行，或者即使是一行也可能很长，我们需要不断读取直到文件结束
//                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
//                {
//                    i++;
//                    // 将读取的字节转换为字符串并拼接到fileContent中
//                    string chunk = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
//                    fileContent += chunk;

//                    // 不需要检查stream.Position == stream.Length，因为只要还有数据可读，循环就会继续
//                    // 等待下一帧
//                    Debug.Log(i);
//                    yield return new WaitForSeconds(0.02f);
//                }

//                // 文件读取完成，可以在这里处理fileContent
//                Debug.Log("文件读取完成: " + fileContent);
//            }
//        }
//    }
//}