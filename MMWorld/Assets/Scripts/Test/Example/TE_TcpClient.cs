//using System;
//using System.Net.Sockets;
//using System.Text;
//using UnityEngine;

//namespace Test.Example
//{
//    /// <summary>
//    /// 模拟客户端测试C#自带的TcpClient向服务器发送数据
//    /// </summary>
//    public class TE_TcpClient : MonoBehaviour
//    {
//        void Start()
//        {
//            Test1();
//        }

//        /// <summary>
//        /// 相比Socket，TcpClient对套接字进行了封装，使得操作更为简单。使用TcpClient设计网络层更方便，无需深入了解复杂的Socket API。
//        /// </summary>
//        void Test1()
//        {
//            // 创建新的TCP客户端实例
//            TcpClient client = new TcpClient();

//            try
//            {
//                // 连接到本地服务器，使用IP地址127.0.0.1（即localhost）和端口8001
//                client.Connect("127.0.0.1", 8001);

//                // 创建获取用于读写数据的网络流实例
//                NetworkStream stream = client.GetStream();

//                // 发送数据到服务器
//                string message = "Hello from Unity!";
//                byte[] data = Encoding.UTF8.GetBytes(message); // 将字符串转换为字节数组
//                stream.Write(data, 0, data.Length); // 将字节数组写入网络流，发送到服务器

//                // 接收来自服务器的响应
//                byte[] responseBuffer = new byte[256]; // 创建一个长度为256的字节数组作为接收缓冲区
//                int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length); // 从网络流中读取数据到缓冲区，返回实际读取的字节数
//                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead); // 将缓冲区中的字节转换为字符串
//                Debug.Log("Received from server: " + response); // 在Unity控制台中打印接收到的响应

//                // 关闭网络流和客户端连接
//                stream.Close();
//                client.Close();
//            }
//            catch (Exception e)
//            {
//                // 如果发生异常，则在Unity控制台中打印错误信息
//                Debug.LogError("Client error: " + e.Message);
//            }
//            // 注意：这里没有使用using语句，因为TcpClient的实例是在try块外部创建的
//            // 如果希望在异常发生时自动关闭客户端，可以将TcpClient的创建放在using语句内
//            // 但是，由于在catch块中已经手动关闭了客户端，所以这里不是必需的
//        }

//        void Test2()
//        {
//            // 使用using语句确保TcpClient实例在使用后被正确释放
//            using (TcpClient client = new TcpClient())
//            {
//                try
//                {
//                    // 连接到本地服务器
//                    client.Connect("127.0.0.1", 8001);

//                    // 获取用于读写数据的网络流
//                    NetworkStream stream = client.GetStream();

//                    // 发送数据到服务器
//                    string message = "Hello from Unity!";
//                    byte[] data = Encoding.UTF8.GetBytes(message);
//                    stream.Write(data, 0, data.Length);

//                    // 接收来自服务器的响应
//                    byte[] responseBuffer = new byte[256]; // 假设响应不会超过256字节
//                    int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
//                    string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
//                    Debug.Log("Received from server: " + response);

//                    // 注意：这里不需要手动关闭stream和client，因为using语句会处理它们
//                }
//                catch (Exception e)
//                {
//                    // 如果发生异常，则在Unity控制台中打印错误信息
//                    Debug.LogError("Client error: " + e.Message);

//                    // 注意：在using语句中，即使发生异常，TcpClient的实例也会被正确释放
//                }
//            }
//            // 此时，TcpClient实例已经被释放，无需再手动调用Close方法
//        }
//    }
//}