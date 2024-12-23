//using System.Net.Sockets;
//using System.Net;
//using System.Text;
//using System.Threading;
//using UnityEngine;

//namespace Test.Example
//{
//    /// <summary>
//    /// 模拟服务器测试用UnityWebRequest接收客户端数据
//    /// </summary>
//    public class TE_WebServer : MonoBehaviour
//    {
//        private TcpListener listener;
//        private const int Port = 8000;

//        private void Start()
//        {
//            // 初始化TcpListener，监听任何IP地址上的指定端口
//            listener = new TcpListener(IPAddress.Any, Port);

//            // 开始监听传入的连接请求
//            listener.Start();

//            // 打印服务器启动消息到Unity控制台
//            Debug.Log("HTTP Server started on port " + Port + "...");

//            // 开始一个新的线程来处理客户端连接，以避免阻塞Unity的主线程
//            Thread serverThread = new Thread(HandleClients);
//            serverThread.IsBackground = true; // 设置为后台线程，以便在应用程序退出时能够正确终止
//            serverThread.Start();
//        }

//        private void HandleClients()
//        {
//            while (true)
//            {
//                // 等待传入的连接请求
//                TcpClient client = listener.AcceptTcpClient();

//                // 处理客户端连接
//                NetworkStream stream = client.GetStream();
//                byte[] buffer = new byte[256]; // 定义一个缓冲区来存储接收到的数据
//                int bytesRead = stream.Read(buffer, 0, buffer.Length); // 读取数据

//                // 解析HTTP请求（这里只处理简单的GET请求）
//                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//                if (request.StartsWith("GET / "))
//                {
//                    // 发送HTTP响应给客户端
//                    string response = @"HTTP/1.1 200 OK\spriteRenderer\nContent-Type: text/plain\spriteRenderer\n\spriteRenderer\nMessage received from Unity client";
//                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
//                    stream.Write(responseBytes, 0, responseBytes.Length);
//                }
//                else
//                {
//                    // 如果不是GET请求，发送404响应
//                    string notFoundResponse = @"HTTP/1.1 404 Not Found\spriteRenderer\n\spriteRenderer\n";
//                    byte[] notFoundResponseBytes = Encoding.UTF8.GetBytes(notFoundResponse);
//                    stream.Write(notFoundResponseBytes, 0, notFoundResponseBytes.Length);
//                }

//                // 关闭流和客户端连接
//                stream.Close();
//                client.Close();
//            }
//        }

//        private void OnApplicationQuit()
//        {
//            // 当应用程序退出时，停止监听器
//            listener.Stop();
//        }
//        // 重要提示‌：
//        // ‌这不是一个完整的HTTP服务器‌：代码只是一个简单HTTP服务器实现，它能处理基本GET请求并发送响应，但是它没实现完整HTTP协议规范如处理POST请求、头信息解析、持久连接等
//        // ‌线程使用‌：由于Unity的主线程不应该被阻塞，我们在Start方法中启动了一个新的线程来处理客户端连接，这确保Unity的渲染和其他操作能够继续进行而不会被网络通信所阻塞
//        // ‌资源管理‌：在实际应用中可能要更精细地管理资源如限制同时处理的客户端连接数，使用连接池来重用TcpClient实例，或实现更高级的错误处理和日志记录
//        // ‌安全性‌：如果服务器部署到公共网络上，请确保添加适当的安全措施，比如加密（SSL/TLS）、身份验证和访问控制
//        // ‌性能‌：代码在每次接收到客户端连接时都会创建一个新的TcpClient和NetworkStream实例，在高性能场景下这可能会导致资源耗尽，需要实现连接池或其他优化策略来管理这些资源

//    }
//}