//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using UnityEngine;

//namespace Test.Example
//{
//    /// <summary>
//    /// 模拟服务器测试使用C#自带TcpClient接收客户端数据
//    /// </summary>
//    public class TE_TcpServer : MonoBehaviour
//    {
//        void Start()
//        {
//            Test1();
//        }

//        void Test1()
//        {
//            // 创建一个TcpListener实例，监听任何IP地址上的8001端口
//            TcpListener listener = new TcpListener(IPAddress.Any, 8001);

//            // 开始监听传入的连接请求
//            listener.Start();

//            // 在控制台（或Unity的控制台，如果这是在Unity中的话）中打印服务器启动消息
//            Debug.Log("Server started on port 8001...");

//            try
//            {
//                // 进入一个无限循环，持续接受和处理客户端连接
//                while (true)
//                {
//                    // 接受一个传入的连接请求，并返回一个TcpClient实例
//                    TcpClient client = listener.AcceptTcpClient();

//                    // 获取用于读写数据的网络流
//                    NetworkStream stream = client.GetStream();

//                    // 创建一个长度为256字节的缓冲区，用于存储接收到的数据
//                    byte[] buffer = new byte[256];

//                    // 从网络流中读取数据到缓冲区，并返回实际读取的字节数
//                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

//                    // 将缓冲区中的字节转换为字符串，假设数据是UTF8编码的
//                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

//                    // 在控制台中打印接收到的消息
//                    Console.WriteLine("Received: " + request);

//                    // 准备发送给客户端的响应消息
//                    byte[] response = Encoding.UTF8.GetBytes("Message received");

//                    // 将响应消息写入网络流，发送给客户端
//                    stream.Write(response, 0, response.Length);

//                    // 关闭网络流和客户端连接
//                    // 注意：在实际应用中，通常不建议在每次处理完一个客户端后就立即关闭监听器，
//                    // 因为这样做会导致服务器停止监听新的连接请求。这里只是为了演示如何关闭流和客户端。
//                    // 在一个真实的服务器中，你通常会在一个单独的线程或任务中处理每个客户端，
//                    // 并且在客户端断开连接或处理完成后才关闭流和客户端对象。
//                    stream.Close();
//                    client.Close();
//                }
//            }
//            catch (Exception e)
//            {
//                // 如果发生异常，则在控制台中打印错误信息
//                Debug.Log("Server error: " + e.Message);
//            }
//            finally
//            {
//                // 无论是否发生异常，都停止监听器
//                // 注意：在实际应用中，你通常不会在每次处理完一个客户端后就停止监听器，
//                // 除非你是故意要关闭服务器。在这个例子中，由于我们在try块中有一个无限循环，
//                // 所以finally块实际上永远不会被执行到（除非发生异常导致循环中断）。
//                // 如果你想要在某个条件下优雅地关闭服务器，你应该在循环外部控制这个条件，
//                // 并在适当的时候调用listener.Stop()方法。
//                listener.Stop();
//            }
//        }

//        //重要提示‌：
//        //在实际服务器应用程序中，通常不会在处理完一个客户端后立即关闭监听器，而会让监听器继续运行以便接受和处理更多的客户端连接
//        //在上面的代码中，while (true) 循环会导致服务器持续运行，直到发生异常为止
//        //在实际应用中，可能需要一种机制来优雅地中断循环并关闭服务器（例如通过用户输入、信号量、定时器或某种关闭条件）
//        //每次接受一个客户端连接时，都会创建一个新的 TcpClient 实例和一个新的 NetworkStream 实例，在处理完客户端的请求后，应该关闭这些实例以释放资源
//        //然而在这个例子中，由于我们在每次循环迭代结束时都关闭了客户端和流，所以实际上不会累积太多的未关闭资源
//        //但是在更复杂的场景中，可能需要更仔细地管理这些资源
//    }
//}