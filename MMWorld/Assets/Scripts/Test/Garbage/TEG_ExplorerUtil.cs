//using System.Diagnostics;
//using System;

//namespace Test.Example.Garbage
//{
//    public class TEG_ExplorerUtil
//    {
//        public static void OpenExplorer(string folderPath)
//        {
//            //process.Start(folderPath);//直接打开文件夹

//            // 检查文件夹路径是否为空或null
//            if (string.IsNullOrEmpty(folderPath))
//            {
//                Debug.WriteLine("文件夹路径为空或null");
//                return;
//            }

//            // 创建ProcessStartInfo对象，设置要打开的命令和参数
//            ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
//            psi.Arguments = folderPath;

//            // 创建Process对象，设置启动信息，并启动进程
//            Process process = new Process();
//            process.StartInfo = psi;
//            try
//            {
//                process.Start();
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("打开文件夹时出错: " + ex.Message);
//            }
//            finally
//            {
//                // 确保进程被正确关闭
//                process.Close();
//            }
//        }
//    }
//}
