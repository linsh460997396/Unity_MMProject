//using MetalMaxSystem;
//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using UnityEngine;

//namespace TestOnly
//{
//    /// <summary>
//    /// 测试入口
//    /// </summary>
//    public class TEG_HD6 : MonoBehaviour
//    {
//        Dictionary<int, string> DataTableCV = new Dictionary<int, string>();
//        ConcurrentDictionary<int, string> CDataTableCV = new ConcurrentDictionary<int, string>();

//        Hashtable HashTableCV = new Hashtable();
//        DateTime startTime, endTime;
//        TimeSpan elapsedTime;

//        void Start()
//        {
//            MMCore.HD_SetIntCV(0, "Compared", "true");

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                MMCore.HD_SetIntCV(0, "ABC", "true");
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用封装字典 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //24.9995ms 内部拼字符导致变慢

//            ThreadStringBuilder.ConcatType = true;

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                MMCore.HD_SetIntCV(0, "ABC", "true");
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用封装字典及string.Concat " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //32.9999ms string.Concat组合数字、字符、字符串混合情况性能较StringBuilder更差

//            ThreadStringBuilder.ConcatType = false;
//            MMCore.DataTableHashType = true;

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                MMCore.HD_SetIntCV(0, "ABC", "true");
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用封装HashTable " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //24.0005ms 内部拼字符导致变慢

//            ThreadStringBuilder.ConcatType = true;

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                MMCore.HD_SetIntCV(0, "ABC", "true");
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用封装HashTable及string.Concat " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //21.0015ms 内部拼字符导致变慢

//            ThreadStringBuilder.ConcatType = false;
//            MMCore.DataTableHashType = false;

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                DataTableCV[0] = "true";
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("用原始字典 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //0.9977ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                HashTableCV[0] = "true";
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("用原始HashTable " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1.9998ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                CDataTableCV[0] = "true";
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("用跨线程字典 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //4.0016ms 跨线程字典默认速度约等于原始字典.ToString()的速度

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                string.Concat("key", "_", "1", "_", "2", "_", "3", "_", "4");
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用string.Concat组合纯字符串 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //15.9989ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                string.Concat("key", "_", 1, "_", 2, "_", 3, "_", 4);
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用string.Concat组合带数字 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //38.0029ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 10000; i++)
//            {
//                ThreadStringBuilder.Concat("key", '_', 1, '_', 2, '_', 3, '_', 4);
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用ThreadStringBuilder.Concat组合带数字、字符 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //21.0163ms
//        }
//    }
//}
