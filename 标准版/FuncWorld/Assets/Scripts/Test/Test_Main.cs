using MetalMaxSystem;
using System;
using UnityEngine;

namespace TestOnly
{
    /// <summary>
    /// 测试入口
    /// </summary>
    public class Test_Main : MonoBehaviour
    {
        void Start()
        {
            DateTime startTime, endTime;
            TimeSpan elapsedTime;

            startTime = DateTime.Now;
            for (int i = 0; i < 1000000; i++)
            {
                string.Concat("key", "_", "1", "_", "2", "_", "3", "_", "4");
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("使用string.Concat " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1156.83ms

            startTime = DateTime.Now;
            for (int i = 0; i < 1000000; i++)
            {
                string.Concat("key", "_", 1, "_", 2, "_", 3, "_", 4);
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("使用string.Concat带数字 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //2865.6832ms

            startTime = DateTime.Now;
            for (int i = 0; i < 1000000; i++)
            {
                ThreadStringBuilder.Concat("key", '_', 1, '_', 2, '_', 3, '_', 4);
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("使用ThreadStringBuilder.Concat " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1609.9135ms
        }
    }
}