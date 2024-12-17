//using System.Collections;
//using UnityEngine;
//using UnityEngine.Networking;

//namespace Test.Example
//{
//    /// <summary>
//    /// 模拟客户端测试用UnityWebRequest向服务器发送数据
//    /// </summary>
//    public class TE_WebClient : MonoBehaviour
//    {
//        void Start()
//        {
//            StartCoroutine(SendRequest());
//        }

//        //使用UnityWebRequest来发送一个简单的GET请求到本地服务器（这个服务器可用任何能处理HTTP请求的服务器软件）
//        //模拟客户端和服务器将使用另一个Unity项目或一个简单的C#控制台应用程序来作为服务器响应请求
//        //以下Unity客户端代码：
//        IEnumerator SendRequest()
//        {
//            // 使用using语句确保UnityWebRequest实例在使用后被正确释放
//            using (UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost:8000"))
//            {
//                // 发送请求并等待响应
//                // 注意：这里使用了协程（coroutine），因此需要在Unity的MonoBehaviour类中调用这个方法，
//                // 并且需要使用StartCoroutine来启动协程。
//                yield return webRequest.SendWebRequest();

//                // 检查请求的结果是否为成功
//                if (webRequest.result == UnityWebRequest.Result.Success)
//                {
//                    // 如果请求成功，打印接收到的响应文本
//                    // webRequest.downloadHandler.text 包含了响应的文本内容
//                    Debug.Log("Received: " + webRequest.downloadHandler.text);
//                }
//                else
//                {
//                    // 如果请求失败，打印错误信息
//                    // webRequest.error 包含了失败的原因
//                    Debug.LogError("Error: " + webRequest.error);
//                }
//            }
//            // 当using语句的作用域结束时，UnityWebRequest实例会被自动释放
//        }
//        // 重要提示‌：
//        // ‌协程使用‌：由于yield return webRequest.SendWebRequest();这行代码，这个方法必须作为一个协程在Unity的MonoBehaviour类中调用。
//        // 例如，你可以在Start或Update方法中使用StartCoroutine(YourMethodName());来启动这个协程。
//        // ‌错误处理‌：在检查webRequest.result时，代码只处理了UnityWebRequest.Result.Success的情况。
//        // 还有其他可能的结果，如UnityWebRequest.Result.ConnectionError或UnityWebRequest.Result.ProtocolError，你可能需要根据你的需求来处理这些情况。
//        // ‌资源释放‌：使用using语句可以确保UnityWebRequest实例在使用后被正确释放，这是处理IDisposable对象的最佳实践。
//        // ‌URL和端口‌：确保URL（http://localhost:8000）和端口（8000）是正确的，并且你的服务器正在该地址上运行。如果URL或端口不正确，请求将失败。
//        // ‌响应处理‌：在这个例子中，响应文本是通过webRequest.downloadHandler.text获取的。如果你的响应包含二进制数据或其他类型的内容，你可能需要使用不同的方法来处理这些数据。
//        // 例如，你可以使用webRequest.downloadHandler.data来获取原始的字节数组。

//    }
//}