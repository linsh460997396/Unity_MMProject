//using MetalMaxSystem;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Test.Example.Garbage
//{
//    public class TEG_HD2 : MonoBehaviour
//    {
//        Dictionary<string, string> DataTableSCV = new Dictionary<string, string>();
//        Dictionary<int, string> DataTableCV = new Dictionary<int, string>();
//        DateTime startTime, endTime;
//        TimeSpan elapsedTime;

//        void Start()
//        {
//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                if (MMCore.HD_ReturnIntCV(i, "Compared") != "true")
//                {
//                    MMCore.HD_SetIntCV(i, "Compared", "true");
//                }
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用HD_ReturnIntCV " + $"耗时：{elapsedTime.TotalMilliseconds}ms");

//            startTime = DateTime.Now;
//            for (int i = 10000; i < 20000; i++)
//            {
//                if (!(DataTableCV.ContainsKey(i) && DataTableCV[i] == "true"))
//                {
//                    DataTableCV[i] = "true";
//                }
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("用原始字典 " + $"耗时：{elapsedTime.TotalMilliseconds}ms");

//            startTime = DateTime.Now;
//            for (int i = 20000; i < 30000; i++)
//            {
//                if (!(DataTableSCV.ContainsKey(i.ToString()) && DataTableSCV[i.ToString()] == "true"))
//                {
//                    DataTableSCV[i.ToString()] = "true";
//                }
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("用原始字典但ToString() " + $"耗时：{elapsedTime.TotalMilliseconds}ms");

//            //string A = "A";
//            //string B = "B"; //如果string B = "A"; 下方结果是：1 1 1 1 ，因为C#会对比2个字符串里面的值内容
//            //Debug.Log(string.Format("结果：{0}, {1}, {2}, {3}", MMCore.THD_RegObjectTagAndReturn(A), MMCore.THD_RegObjectTagAndReturn(A), 
//            //    MMCore.THD_RegObjectTagAndReturn(B), MMCore.THD_RegObjectTagAndReturn(B)));

//            //Unit C = new Unit(); //装箱后是同一个
//            //Debug.Log(string.Format("结果：{0}, {1}", MMCore.THD_RegObjectTagAndReturn(A), MMCore.THD_RegObjectTagAndReturn(A)));

//            ////数字装箱后是同一个
//            //Debug.Log(string.Format("结果：{0}, {1}", 1, 1));

//        }
//    }
//}
