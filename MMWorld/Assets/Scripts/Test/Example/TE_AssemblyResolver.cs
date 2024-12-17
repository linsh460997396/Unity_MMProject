// using System;
// using System.Reflection;
// using System.Resources;
// using System.Xml.Linq;

// namespace Test.Example
// {
//     public class AssemblyResolver
//     {
//         public static void HookupAssemblyResolve()
//         {
//             AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(
//                 CurrentDomain_AssemblyResolve
//             );
//         }
//         /// <summary>
//         /// 解析程序集
//         /// </summary>
//         /// <param name="sender">事件发送者</param>
//         /// <param name="args">包含程序集名称的ResolveEventArgs</param>
//         /// <returns>解析到的程序集</returns>
//         private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
//         {
//             //获取dll名称，命令参数有逗号则读取到逗号长度为DLL名称，否则整个作为DLL名称
//             string dllName = args.Name.Contains(",")
//                 ? args.Name.Substring(0, args.Name.IndexOf(','))
//                 : args.Name;
//             // dll名称处理（替换"."为"_"，并忽略指定名称的资源dll）
//             dllName = dllName.Replace(".", "_");
//             if (dllName.EndsWith("_resources"))
//             {
//                 return null;
//             }
//             // 获取命名空间（这里假设主程序集的第一个类型的命名空间即为所需命名空间）
//             // 注意：这种方法可能不总是可靠的，特别是如果主程序集不包含任何类型或者第一个类型的命名空间不是所需的
//             string namespaceName = Assembly.GetEntryAssembly().GetTypes()[0].Namespace;
//             // 加载dll（这里使用资源管理器从嵌入的资源中加载dll的字节数组）
//             // 注意：这里假设dll已经作为嵌入资源添加到项目中，并且资源名与dllName相匹配
//             ResourceManager rm = new ResourceManager(
//                 namespaceName + ".Properties.Resources",
//                 Assembly.GetEntryAssembly()
//             );
//             byte[] bytes = (byte[])rm.GetObject(dllName);
//             // 如果找到了字节数组，则加载程序集
//             if (bytes != null)
//             {
//                 return Assembly.Load(bytes);
//             }
//             // 如果没有找到，则返回null，让.NET框架继续它的解析过程
//             return null;
//         }
//     }
// }

// WinForms项目中的生成操作说明
// ‌嵌入的资源‌	文件被嵌入到编译后的程序集中，成为程序集的一部分。这适用于图像、音频、视频、配置文件或其他需要在运行时访问的文件。
// 嵌入的资源可以通过System.Reflection.Assembly类的相关方法（如GetManifestResourceStream）在运行时访问。
// ‌内容‌	文件被复制到输出目录，但不嵌入到程序集中。这适用于需要在运行时访问但不需要嵌入到程序集中的文件，如数据库文件、外部配置文件等。
// ‌编译‌	通常用于源代码文件（如C#文件）。这些文件将被编译成中间语言（IL）代码，并最终嵌入到程序集中。
// ‌无操作‌	文件不会被编译或复制到输出目录。这通常用于那些不需要在编译过程中处理的文件，如项目文档、临时文件等。
// ‌链接器输入‌	文件被用作链接器输入，这通常用于C++项目或混合项目（包含C#和C++代码的项目）。在WinForms项目中，这个选项较少使用。

// 对于嵌入的资源文件，选择“嵌入的资源”作为生成操作后，您可以在运行时通过以下方式访问这些资源：
// using System;
// using System.Reflection;
// using System.IO;
// // 获取当前执行程序集
// Assembly assembly = Assembly.GetExecutingAssembly();
// // 获取嵌入资源的名称（命名空间.文件名）
// string resourceName = "YourNamespace.YourResourceName";
// // 获取嵌入资源的流
// using (Stream stream = assembly.GetManifestResourceStream(resourceName))
// {
//     if (stream != null)
//     {
//         // 读取或处理资源
//         // 例如，将资源读取为字节数组
//         byte[] resourceBytes = new byte[stream.Length];
//         stream.Read(resourceBytes, 0, (int)stream.Length);
//         // 或者，将资源读取为字符串（如果资源是文本文件）
//         // using (StreamReader reader = new StreamReader(stream))
//         // {
//         //     string resourceText = reader.ReadToEnd();
//         // }
//     }
// }
// 请注意，在上面的代码中，YourNamespace应替换为包含嵌入资源的命名空间，YourResourceName应替换为嵌入资源的文件名（包括扩展名）。
// 如果资源文件位于项目的根目录下，则resourceName可能只需要文件名（包括扩展名）。
// 如果资源文件位于项目的某个文件夹中，则resourceName应包括文件夹路径（使用点.作为路径分隔符）。