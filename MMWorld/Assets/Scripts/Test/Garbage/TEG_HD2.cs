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
//            Debug.Log("ʹ��HD_ReturnIntCV " + $"��ʱ��{elapsedTime.TotalMilliseconds}ms");

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
//            Debug.Log("��ԭʼ�ֵ� " + $"��ʱ��{elapsedTime.TotalMilliseconds}ms");

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
//            Debug.Log("��ԭʼ�ֵ䵫ToString() " + $"��ʱ��{elapsedTime.TotalMilliseconds}ms");

//            //string A = "A";
//            //string B = "B"; //���string B = "A"; �·�����ǣ�1 1 1 1 ����ΪC#��Ա�2���ַ��������ֵ����
//            //Debug.Log(string.Format("�����{0}, {1}, {2}, {3}", MMCore.THD_RegObjectTagAndReturn(A), MMCore.THD_RegObjectTagAndReturn(A), 
//            //    MMCore.THD_RegObjectTagAndReturn(B), MMCore.THD_RegObjectTagAndReturn(B)));

//            //Unit C = new Unit(); //װ�����ͬһ��
//            //Debug.Log(string.Format("�����{0}, {1}", MMCore.THD_RegObjectTagAndReturn(A), MMCore.THD_RegObjectTagAndReturn(A)));

//            ////����װ�����ͬһ��
//            //Debug.Log(string.Format("�����{0}, {1}", 1, 1));

//        }
//    }
//}
