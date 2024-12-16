using System.Runtime.InteropServices;
using System;
#if UNITY_EDITOR|| UNITY_STANDALONE
//WIN上的Unity编辑器、独立应用程序
using UnityEngine;
#else
using System.Diagnostics;
#endif

namespace MetalMaxSystem
{
    /// <summary>
    /// 加载自定义Dll的方法类，必须声明其内部函数才可以用
    /// </summary>
    public static class DllLoader
    {
        public static System.IntPtr dllHandle;

        /// <summary>
        /// 作为SetDefaultDllDirectories()参数，可全局恢复默认的DLL搜索路径。
        /// 这是 LOAD_LIBRARY_SEARCH_APPLICATION_DIR、LOAD_LIBRARY_SEARCH_SYSTEM32 和 LOAD_LIBRARY_SEARCH_USER_DIRS 的组合。
        /// 它代表应用程序在其DLL搜索路径中应包含的最大推荐目录数。
        /// </summary>
        const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
        /// <summary>
        /// 作为SetDefaultDllDirectories()参数，搜索应用程序的安装目录
        /// </summary>
        const int LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200;
        /// <summary>
        /// 作为SetDefaultDllDirectories()参数，搜索 %windows%\system32 目录
        /// </summary>
        const int LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;
        /// <summary>
        /// 作为SetDefaultDllDirectories()参数，搜索使用 AddDllDirectory 或 SetDllDirectory 函数明确添加的任何路径
        /// </summary>
        const int LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400;

        /*************************************************************************/
        // core funcs

        /// <summary>
        /// 在调用任何使用[DllImport]的方法之前用SetDllDirectory函数来设置搜索DLL的目录（多次调用会覆盖搜索目录），应用后对整个exe进程生效。
        /// SetDllDirectory("C:/Windows/SysWOW64");//32位的dll可放在这让64位系统兼容使用，Win32/64位的C:/Windows/System32反而是存放对应32/64位dll的。
        /// 函数设置的是线程局部的DLL搜索路径，仅对当前线程有效，如需更改全局DLL搜索路径应使用SetDefaultDllDirectories函数
        /// </summary>
        /// <param name="lpPathName"></param>
        /// <returns></returns>
        //指示C#编译器这个方法是从搜索路径中的第一个kernel32.dll中导入，函数执行失败它会通过调用SetLastError函数设置一个错误代码，这个错误代码可通过调用Marshal.GetLastWin32Error()方法在C#中获取
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory(string lpPathName);

        /// <summary>
        /// 添加[DllImport]的搜索路径
        /// </summary>
        /// <param name="lpPathName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddDllDirectory(string lpPathName);

        /// <summary>
        /// 更改全局DLL搜索路径，影响LoadLibrary和LoadLibraryEx函数搜索DLL的路径‌
        /// </summary>
        /// <param name="directoryFlags"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDefaultDllDirectories(int directoryFlags);

        /// <summary>
        /// 加载指定的DLL（动态链接库）文件到进程地址空间中
        /// </summary>
        /// <param name="lpLibFileName">要加载的DLL文件的路径。可以是绝对路径或相对路径，如果是相对路径则相对于当前进程的工作目录</param>
        /// <returns>返回该DLL模块的句柄</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpLibFileName);

        /// <summary>
        /// 加载指定的DLL（动态链接库）文件到进程地址空间中
        /// </summary>
        /// <param name="lpLibFileName">要加载的DLL文件的路径。可以是绝对路径或相对路径，如果是相对路径则相对于当前进程的工作目录</param>
        /// <param name="hFile">文件句柄，通常设置为IntPtr.Zero（空）表示不使用文件句柄来加载DLL</param>
        /// <param name="dwFlags">加载选项，用来改变系统搜索DLL的方式。常见标志： - LOAD_WITH_ALTERED_SEARCH_PATH（0x00000008）：改变搜索路径策略。 - DONT_RESOLVE_DLL_REFERENCES（0x00000001）：不解析DLL的引用。 - LOAD_IGNORE_CODE_AUTHZ_LEVEL（0x00000010）：忽略代码授权级别。</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hFile, uint dwFlags);

        /// <summary>
        /// 释放之前加载的动态链接库
        /// </summary>
        /// <param name="hModule">IntPtr是在.NET中用于表示指针或句柄的类型，参数表示要释放的动态链接库的模块句柄（通常通过LoadLibrary、LoadLibraryEx、LoadPackagedLibrary或GetModuleHandle函数获得）</param>
        /// <returns>返回一个布尔值，指示是否成功释放了动态链接库</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// 获取函数地址
        /// </summary>
        /// <param name="hModule">指定要获取函数地址的DLL的模块句柄（通常通过LoadLibrary、LoadLibraryEx、LoadPackagedLibrary或GetModuleHandle函数获得）</param>
        /// <param name="lpProcName">函数名或序号</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        /// <summary>
        /// 获取最近一次由其他函数设置的错误代码
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        static private extern uint GetLastError();

        /*************************************************************************/

        /// <summary>
        /// 读取Application.dataPath/Plugins/下的dll。注意Application.dataPath打包前是Assets文件夹下的路径，打包后是识别exe程序名称_Data文件夹下的路径
        /// </summary>
        /// <param name="type">填写typeof(Dll)即可</param>
        /// <param name="fileName">dllName.dll</param>
        public static void Load(System.Type type, string fileName = "dllName.dll", string folderPath = null)
        {
            Debug.Assert(dllHandle == System.IntPtr.Zero);

            if (folderPath == null)
            {
                folderPath =
#if UNITY_EDITOR || UNITY_STANDALONE
            Application.dataPath + "/Plugins";
#else
//C#的VS工程中Debug情况请完善dll目录,".."表示父目录（或上一级目录）
System.AppDomain.CurrentDomain.BaseDirectory +
#if DEBUG
        "../../../../x64/Debug";
#else
        "../../../../x64/Release";
#endif
#endif
            }
            dllHandle = LoadLibrary(folderPath + "/" + fileName);

            var fields = type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            foreach (var field in fields)
            {
                var fnPtr = GetProcAddress(dllHandle, field.Name);
                if (fnPtr != System.IntPtr.Zero)
                {
                    field.SetValue(null, Marshal.GetDelegateForFunctionPointer(fnPtr, field.FieldType));
                }
            }
        }

        public static void Unload()
        {
            Debug.Assert(dllHandle != System.IntPtr.Zero);
            FreeLibrary(dllHandle);
            dllHandle = System.IntPtr.Zero;
            // ...
        }
    }

    /// <summary>
    /// 自定义Dll的方法类，必须声明其内部函数才可以用
    /// </summary>
    public static class Dll
    {
        public delegate void __void_();
        public delegate int __int_();
        public delegate int __int_ptr_int(System.IntPtr buf, int len);
        public delegate int __int_float(float v);
        // ...

        /// <summary>
        /// 必须声明自制Dll的内部函数才可以用
        /// </summary>
        public static __int_ New;

        public static __int_ptr_int InitBuf;
        public static __int_float InitFrameDelay;

        public static __int_ Begin;
        public static __int_float Update;
        public static __int_ Draw;
        public static __int_ End;

        public static __int_ Delete;
        // ...
    }

    /*
     * reference:
    https://github.com/forrestthewoods/fts_unity_native_plugin_reloader

    // put following code into ENTRY class

    void Awake()
    {
        DllLoader.Load(typeof(Dll), "dll.dll");
        Dll.New();
        Debug.Log("dll loaded");
    }

    void OnDestroy() {
        Dll.Delete();
        DllLoader.Unload();
        Debug.Log("dll unloaded");
    }

    */

    /************************************************************************************************************************/
    /************************************************************************************************************************/
}

//调用任何使用[DllImport]的方法之前不使用SetDllDirectory函数来设置搜索DLL的目录，那么系统将按照默认的DLL搜索顺序来查找所需的DLL文件。
//Winform项目打包后，默认的DLL搜索顺序通常如下：
//‌应用程序所在目录‌：系统会首先在当前运行的应用程序所在的目录中搜索DLL文件。
//‌系统目录‌：接着，系统会搜索Windows的系统目录，这个目录通常可以通过GetSystemDirectory函数获取，对于64位系统来说，64位的DLL文件位于System32目录下，而32位的DLL文件（在64位系统中）则位于SysWOW64目录下。
//‌16位系统目录‌：这个目录通常指的是Windows目录中的System目录，但需要注意的是，在现代的64位Windows系统中，这个目录可能不再用于存放系统DLL文件。
//‌Windows目录‌：系统会搜索Windows目录，这个目录可以通过GetWindowsDirectory函数获取，它通常指向包含Windows系统文件的目录，如C:\Windows。
//‌当前工作目录‌：在某些情况下，系统也会搜索当前工作目录，即应用程序启动时所在的目录，但这个行为可能会受到SafeDllSearchMode注册表值的影响。
//‌PATH环境变量中的目录‌：最后，系统会搜索PATH环境变量中列出的目录。PATH环境变量是一个包含了一系列目录路径的字符串，系统会在这些目录中搜索所需的DLL文件。
//需要注意的是以上搜索顺序可能会受到某些因素的影响而发生变化，例如使用了LOAD_WITH_ALTERED_SEARCH_PATH标志调用LoadLibraryEx函数，或者在注册表中设置了SafeDllSearchMode键值等情况。
//此外，对于64位系统上的32位应用程序，系统还会有一个额外的重定向机制，即WOW64重定向。当32位应用程序尝试加载一个系统DLL时，系统可能会尝试从SysWOW64目录（而不是System32目录）中加载对应的32位版本DLL文件。

//Unity因有自己的编辑器和打包后的搜索路径，不采用用户/系统变量Path，这种情况请查阅：https://docs.unity3d.com/cn/2022.3/Manual/Plugins.html

// [DllImport("kernel32")]
// 这里[DllImport]属性定义了函数的P/Invoke签名，指示该方法是从非托管代码（如Windows API）中导入的
// "kernel32"搜索名称可不包含.dll后缀（编译器返回第一个符合名称的DLL文件，并会在运行时尝试加载它），kernel32.dll是Windows操作系统的一个核心库，是包含了许多底层系统函数的动态链接库数
// 有一些情况需包含完整的文件路径或者扩展名，如：
// 1）DLL文件不在系统的标准搜索路径中（如System32或SysWOW64目录）
// 2）想要加载一个具有不同扩展名的文件
// 3）想要确保加载的是特定版本的DLL，而不仅仅是名称匹配的第一个DLL
// 这些情况可在[DllImport]属性中指定完整的文件路径
// [DllImport("C:\\Path\\To\\Your\\CustomLibrary.dll")]
// static extern void YourFunction();
// 但大多数情况下只需要指定DLL的名称就足够了

//[return: MarshalAs(UnmanagedType.Bool)] //指定返回值类型应如何被.NET Framework封送（marshal）回C#，这里指示函数返回值应被当作一个布尔值来处理
// return：这指示该属性应用于方法的返回值；MarshalAs属性用于指定如何封送（marshal）数据即如何在不同的编程环境（如C#和C/C++）之间传递数据
// UnmanagedType.Bool：指定返回值类型应被当作布尔值来处理，通过指定UnmanagedType.Bool，.NET知道如何正确地将这个返回值封送为一个C#的布尔值
// 在C#中，当使用DllImport属性来声明一个外部方法（即从非托管代码中导入的方法）时，通常需要指定如何封送（marshal）该方法的参数和返回值
// 封送是指将数据从一种类型或内存布局转换为另一种，以便在不同的编程环境（如C#和C/C++）之间传递
// 对于返回值，如果不显式指定[return: MarshalAs(UnmanagedType.Bool)]，编译器会尝试根据方法的签名和上下文来推断返回值的类型
// 在大多数情况下，如果外部方法的返回类型是布尔值并且使用的是标准的Windows API函数，那么编译器通常能够正确地推断出返回值的类型并自动进行封送
// 显式指定[return: MarshalAs(UnmanagedType.Bool)]有几个好处：
// ‌1）清晰性‌：它明确指出了返回值的类型，使得代码更易于理解和维护。
// ‌2）准确性‌：在某些情况下，特别是当返回值的类型不是显而易见的，或者当你想确保特定的封送行为时，显式指定可以避免潜在的错误。
// ‌3）兼容性‌：对于某些特殊的返回值类型或封送行为，显式指定可以确保你的代码在不同的平台或.NET版本之间保持兼容性。
// 案例中，FreeLibrary函数返回一个布尔值，指示是否成功释放了动态链接库。即使不写[return: MarshalAs(UnmanagedType.Bool)]，编译器也很可能会正确地推断出返回值的类型并自动进行封送
// 但为了代码清晰性和准确性最好还是显式指定返回值的封送类型。

// SetLastError=true：这个参数告诉.NET如果FreeLibrary函数执行失败，它应通过调用Marshal.GetLastWin32Error()方法来获取错误代码，对错误处理和调试非常有用。
// extern：表示该方法是在外部实现的

// // 定义与DLL中函数签名匹配的委托
// [UnmanagedFunctionPointer(CallingConvention.StdCall)]
// public delegate int MyFunctionDelegate(int param);
// class Program
// {
//     // 导入GetProcAddress函数
//     [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
//     public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
//     // 导入LoadLibrary函数
//     [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
//     public static extern IntPtr LoadLibrary(string lpFileName);
//     // 导入FreeLibrary函数
//     [DllImport("kernel32.dll", SetLastError = true)]
//     [return: MarshalAs(UnmanagedType.Bool)]
//     public static extern bool FreeLibrary(IntPtr hModule);
//     static void Main(string[] args)
//     {
//         // 加载DLL
//         IntPtr hDll = LoadLibrary("example.dll");
//         if (hDll == IntPtr.Zero)
//         {
//             Console.WriteLine("Failed to load DLL");
//             return;
//         }
//         // 获取函数地址
//         IntPtr funcAddress = GetProcAddress(hDll, "MyFunction");
//         if (funcAddress == IntPtr.Zero)
//         {
//             Console.WriteLine("Failed to get function address");
//             FreeLibrary(hDll);
//             return;
//         }
//         // 将地址转换为委托
//         MyFunctionDelegate myFunction = (MyFunctionDelegate)Marshal.GetDelegateForFunctionPointer(funcAddress, typeof(MyFunctionDelegate));
//         // 调用函数
//         int result = myFunction(42);
//         Console.WriteLine("Function result: " + result);
//         // 释放DLL
//         FreeLibrary(hDll);
//     }
// }
