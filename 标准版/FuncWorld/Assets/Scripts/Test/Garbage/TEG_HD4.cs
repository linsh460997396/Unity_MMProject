//using MetalMaxSystem;
//using System;
//using UnityEngine;

//namespace TestOnly
//{
//    /// <summary>
//    /// 测试入口
//    /// </summary>
//    public class TEG_HD4 : MonoBehaviour
//    {
//        void Start()
//        {
//            DateTime startTime, endTime;
//            TimeSpan elapsedTime;
//            var sb = ThreadStringBuilder.Get();

//            startTime = DateTime.Now;
//            for (int i = 0; i < 1000000; i++)
//            {
//                string.Concat("key", "_", "1", "_", "2", "_", "3", "_", "4");
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用string.Concat " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1000.6052ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 1000000; i++)
//            {
//                string.Concat("key", "_", 1, "_", 2, "_", 3, "_", 4);
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用string.Concat " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //2419.4078ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 1000000; i++)
//            {
//                string.Concat("key", '_', 1, '_', 2, '_', 3, '_', 4);
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用string.Concat带char " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //3196.407ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 1000000; i++)
//            {
//                Combi.BuildStringEfficiently("key", "_", 1, "_", 2, "_", 3, "_", 4);
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用BuildStringEfficiently " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //2284.2821ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 1000000; i++)
//            {
//                Combi.BuildStringEfficiently("key", '_', 1, '_', 2, '_', 3, '_', 4);
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用BuildStringEfficiently带char " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1717.7399ms

//            startTime = DateTime.Now;
//            for (int i = 0; i < 1000000; i++)
//            {
//                sb.Clear();
//                sb.Append("key");      // 追加字符串
//                sb.Append('_');        // 追加字符
//                sb.Append(1);          // 追加整数（会自动转换为字符串）
//                sb.Append('_');
//                sb.Append(2);
//                sb.Append('_');
//                sb.Append(3);
//                sb.Append('_');
//                sb.Append(4);
//            }
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            Debug.Log("使用Append拼接 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1137.5445ms
//        }
//    }
//}