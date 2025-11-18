using MetalMaxSystem;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Test.Example.Garbage
{
    public class TEG_HD3 : MonoBehaviour
    {
        Dictionary<string, string> DataTableSCV = new Dictionary<string, string>();
        Dictionary<int, string> DataTableCV = new Dictionary<int, string>();
        ConcurrentDictionary<string, string> CDataTableSCV = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<int, string> CDataTableCV = new ConcurrentDictionary<int, string>();
        Hashtable HashTableCV = new Hashtable();
        DateTime startTime, endTime;
        TimeSpan elapsedTime;

        void Start()
        {
            startTime = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                if (MMCore.HD_ReturnIntCV(i, "Compared") != "true")
                {
                    MMCore.HD_SetIntCV(i, "Compared", "true");
                }
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("使用封装字典 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //27.3604ms 内部拼字符导致变慢

            MMCore.DataTableType = true;

            startTime = DateTime.Now;
            for (int i = 10000; i < 20000; i++)
            {
                if (MMCore.HD_ReturnIntCV(i, "Compared") == null || MMCore.HD_ReturnIntCV(i, "Compared") != "true")
                {
                    MMCore.HD_SetIntCV(i, "Compared", "true");
                }
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("使用封装HashTable " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //26.0574ms 内部拼字符导致变慢

            startTime = DateTime.Now;
            for (int i = 20000; i < 30000; i++)
            {
                if (!(DataTableCV.ContainsKey(i) && DataTableCV[i] == "true"))
                {
                    DataTableCV[i] = "true";
                }
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("用原始字典 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1.02ms

            startTime = DateTime.Now;
            for (int i = 20000; i < 30000; i++)
            {
                if (!(DataTableSCV.ContainsKey(i.ToString()) && DataTableSCV[i.ToString()] == "true"))
                {
                    DataTableSCV[i.ToString()] = "true";
                }
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("用原始字典且ToString() " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //6.9783ms ToString导致变慢

            startTime = DateTime.Now;
            for (int i = 30000; i < 40000; i++)
            {
                if (!(CDataTableCV.ContainsKey(i) && CDataTableCV[i] == "true"))
                {
                    CDataTableCV[i] = "true";
                }
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("用跨线程字典 " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //6.9533ms 跨线程字典默认速度约等于原始字典.ToString()的速度

            startTime = DateTime.Now;
            for (int i = 40000; i < 50000; i++)
            {
                if (!(CDataTableSCV.ContainsKey(i.ToString()) && CDataTableSCV[i.ToString()] == "true"))
                {
                    CDataTableSCV[i.ToString()] = "true";
                }
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("用跨线程字典且ToString() " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //16.9541ms 跨线程+ToString()更慢

            startTime = DateTime.Now;
            for (int i = 50000; i < 60000; i++)
            {
                if (!(HashTableCV.ContainsKey(i) && HashTableCV[i] == "true"))
                {
                    HashTableCV[i] = "true";
                }
            }
            endTime = DateTime.Now;
            elapsedTime = endTime - startTime;
            Debug.Log("用原始HashTable " + $"耗时:{elapsedTime.TotalMilliseconds}ms"); //1.9947ms

            //string A = "A";
            //string B = "B"; //若string B = "A"; 下方结果是:1 1 1 1 ,因为C#会对比2个字符串里面的值内容
            //Debug.Log(string.Format("结果:{0}, {1}, {2}, {3}", MMCore.THD_RegObjectTagAndReturn(A), MMCore.THD_RegObjectTagAndReturn(A), 
            //    MMCore.THD_RegObjectTagAndReturn(B), MMCore.THD_RegObjectTagAndReturn(B)));

            //Unit C = new Unit(); //装箱后是同一个
            //Debug.Log(string.Format("结果:{0}, {1}", MMCore.THD_RegObjectTagAndReturn(A), MMCore.THD_RegObjectTagAndReturn(A)));

            ////数字装箱后是同一个
            //Debug.Log(string.Format("结果:{0}, {1}", 1, 1));

        }
    }
}
