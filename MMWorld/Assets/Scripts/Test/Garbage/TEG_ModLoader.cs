//using System;
//using System.IO;
//using System.Reflection;
//using UnityEngine;

////����Ҫ��̬����DLLʱ��ʹ��System.Reflection�����ռ��е�Assembly.LoadFrom��Assembly.LoadFile����
//namespace Test.Example.Garbage
//{
//    public class TEG_ModLoader
//    {
//        // �ص����������Ͷ��壬�������ص�����û�в���Ҳû�з���ֵ
//        public delegate void FinishLoadDllCallback();

//        // DLL���ط���
//        public static void LoadDLL(string modName, string methodName = "Start", FinishLoadDllCallback callBack = null)
//        {
//            // ��ȡDLL�ļ���·�������Ը�Ϊ֧���Զ�ɨ�貢��ӵ��ַ������飬����Mod�������أ�
//            string dllPath = Path.Combine(Application.persistentDataPath, "Mods/" + modName + "/Scripts/" + modName + ".dll");

//            try
//            {
//                // ����DLL�ļ�
//                Assembly assembly = Assembly.LoadFrom(dllPath);

//                // ��ȡDLL�е����ͣ��������MOD�е���������Ϊ"ModLoader"����λ����modName��ͬ�������ռ���
//                Type typeToInstantiate = assembly.GetType(modName + ".ModLoader");

//                // ����DLL�е����͵�ʵ��
//                object instance = Activator.CreateInstance(typeToInstantiate);

//                // ��ȡ������Ϣ������methodName��Ҫ���õķ���������
//                MethodInfo methodInfo = typeToInstantiate.GetMethod(methodName);

//                // ��鷽���Ƿ����
//                if (methodInfo != null)
//                {
//                    // ����������ڣ�������
//                    methodInfo.Invoke(instance, null); //Start()�������޲�������null

//                    // ���ûص�����
//                    callBack?.Invoke();

//                    //��ΪcallBack�Ĳ������У�methodInfo.Invoke����object��callBack��ί�����Ͷ��󣬿ɹҵ��������������ಥί�У���callBack(object)ִ��ʱ��Щ���������objectΪ����ִ��
//                    //callBack?.Invoke(methodInfo.Invoke(instance, null));
//                }
//                else
//                {
//                    // ���������ڣ�������Ӧ�Ĵ���
//                    Debug.LogError("ָ���ķ���������: " + methodName);
//                }
//            }
//            catch (Exception ex)
//            {
//                // �����κ��쳣�����������Ϣ
//                Debug.LogError("����DLLʱ��������: " + ex.Message);
//            }
//        }
//    }
//}
