//using System;
//using System.Net.Sockets;
//using System.Text;
//using UnityEngine;

//namespace Test.Example
//{
//    /// <summary>
//    /// ģ��ͻ��˲���C#�Դ���TcpClient���������������
//    /// </summary>
//    public class TE_TcpClient : MonoBehaviour
//    {
//        void Start()
//        {
//            Test1();
//        }

//        /// <summary>
//        /// ���Socket��TcpClient���׽��ֽ����˷�װ��ʹ�ò�����Ϊ�򵥡�ʹ��TcpClient������������㣬���������˽⸴�ӵ�Socket API��
//        /// </summary>
//        void Test1()
//        {
//            // �����µ�TCP�ͻ���ʵ��
//            TcpClient client = new TcpClient();

//            try
//            {
//                // ���ӵ����ط�������ʹ��IP��ַ127.0.0.1����localhost���Ͷ˿�8001
//                client.Connect("127.0.0.1", 8001);

//                // ������ȡ���ڶ�д���ݵ�������ʵ��
//                NetworkStream stream = client.GetStream();

//                // �������ݵ�������
//                string message = "Hello from Unity!";
//                byte[] data = Encoding.UTF8.GetBytes(message); // ���ַ���ת��Ϊ�ֽ�����
//                stream.Write(data, 0, data.Length); // ���ֽ�����д�������������͵�������

//                // �������Է���������Ӧ
//                byte[] responseBuffer = new byte[256]; // ����һ������Ϊ256���ֽ�������Ϊ���ջ�����
//                int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length); // ���������ж�ȡ���ݵ�������������ʵ�ʶ�ȡ���ֽ���
//                string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead); // ���������е��ֽ�ת��Ϊ�ַ���
//                Debug.Log("Received from server: " + response); // ��Unity����̨�д�ӡ���յ�����Ӧ

//                // �ر��������Ϳͻ�������
//                stream.Close();
//                client.Close();
//            }
//            catch (Exception e)
//            {
//                // ��������쳣������Unity����̨�д�ӡ������Ϣ
//                Debug.LogError("Client error: " + e.Message);
//            }
//            // ע�⣺����û��ʹ��using��䣬��ΪTcpClient��ʵ������try���ⲿ������
//            // ���ϣ�����쳣����ʱ�Զ��رտͻ��ˣ����Խ�TcpClient�Ĵ�������using�����
//            // ���ǣ�������catch�����Ѿ��ֶ��ر��˿ͻ��ˣ��������ﲻ�Ǳ����
//        }

//        void Test2()
//        {
//            // ʹ��using���ȷ��TcpClientʵ����ʹ�ú���ȷ�ͷ�
//            using (TcpClient client = new TcpClient())
//            {
//                try
//                {
//                    // ���ӵ����ط�����
//                    client.Connect("127.0.0.1", 8001);

//                    // ��ȡ���ڶ�д���ݵ�������
//                    NetworkStream stream = client.GetStream();

//                    // �������ݵ�������
//                    string message = "Hello from Unity!";
//                    byte[] data = Encoding.UTF8.GetBytes(message);
//                    stream.Write(data, 0, data.Length);

//                    // �������Է���������Ӧ
//                    byte[] responseBuffer = new byte[256]; // ������Ӧ���ᳬ��256�ֽ�
//                    int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
//                    string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
//                    Debug.Log("Received from server: " + response);

//                    // ע�⣺���ﲻ��Ҫ�ֶ��ر�stream��client����Ϊusing���ᴦ������
//                }
//                catch (Exception e)
//                {
//                    // ��������쳣������Unity����̨�д�ӡ������Ϣ
//                    Debug.LogError("Client error: " + e.Message);

//                    // ע�⣺��using����У���ʹ�����쳣��TcpClient��ʵ��Ҳ�ᱻ��ȷ�ͷ�
//                }
//            }
//            // ��ʱ��TcpClientʵ���Ѿ����ͷţ��������ֶ�����Close����
//        }
//    }
//}