//using System;
//using System.IO;
//using System.Reflection;
//using UnityEngine;

////当需要动态加载DLL时可使用System.Reflection命名空间中的Assembly.LoadFrom或Assembly.LoadFile方法
//namespace Test.Example.Garbage
//{
//    public class TEG_ModLoader
//    {
//        // 回调函数的类型定义，这里假设回调函数没有参数也没有返回值
//        public delegate void FinishLoadDllCallback();

//        // DLL加载方法
//        public static void LoadDLL(string modName, string methodName = "Start", FinishLoadDllCallback callBack = null)
//        {
//            // 获取DLL文件的路径（可以改为支持自动扫描并添加到字符串数组，进行Mod批量加载）
//            string dllPath = Path.Combine(Application.persistentDataPath, "Mods/" + modName + "/Scripts/" + modName + ".dll");

//            try
//            {
//                // 加载DLL文件
//                Assembly assembly = Assembly.LoadFrom(dllPath);

//                // 获取DLL中的类型，这里假设MOD中的主类命名为"ModLoader"并且位于与modName相同的命名空间中
//                Type typeToInstantiate = assembly.GetType(modName + ".ModLoader");

//                // 创建DLL中的类型的实例
//                object instance = Activator.CreateInstance(typeToInstantiate);

//                // 获取方法信息，假设methodName是要调用的方法的名称
//                MethodInfo methodInfo = typeToInstantiate.GetMethod(methodName);

//                // 检查方法是否存在
//                if (methodInfo != null)
//                {
//                    // 如果方法存在，调用它
//                    methodInfo.Invoke(instance, null); //Start()方法若无参数则填null

//                    // 调用回调函数
//                    callBack?.Invoke();

//                    //作为callBack的参数运行，methodInfo.Invoke返回object，callBack是委托类型对象，可挂单个或多个方法（多播委托），callBack(object)执行时这些方法逐个以object为参数执行
//                    //callBack?.Invoke(methodInfo.Invoke(instance, null));
//                }
//                else
//                {
//                    // 方法不存在，进行相应的处理
//                    Debug.LogError("指定的方法不存在: " + methodName);
//                }
//            }
//            catch (Exception ex)
//            {
//                // 捕获任何异常并输出错误信息
//                Debug.LogError("加载DLL时发生错误: " + ex.Message);
//            }
//        }
//    }
//}
