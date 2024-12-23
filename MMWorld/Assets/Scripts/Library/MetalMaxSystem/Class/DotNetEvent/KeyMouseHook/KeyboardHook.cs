using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MetalMaxSystem
{
    //虚拟键码：https://learn.microsoft.com/zh-cn/windows/win32/inputdev/virtual-key-codes?redirectedfrom=MSDN
    //CSDN原址：https://blog.csdn.net/qq_43851684/article/details/113096306

    /// <summary>
    /// 键盘钩子
    /// </summary>
    public class KeyboardHook
    {
        #region 常数和结构

        #region wParam键盘输入通知

        public const int WM_KEYDOWN = 0x100;    // 键盘被按下
        public const int WM_KEYUP = 0x101;      // 键盘被松开
        public const int WM_SYSKEYDOWN = 0x104; // 系统键被按下，例如Alt、Ctrl等键
        public const int WM_SYSKEYUP = 0x105;   // 系统键被松开，例如Alt、Ctrl等键
        public const int WH_KEYBOARD_LL = 13;

        #endregion

        [StructLayout(LayoutKind.Sequential)] //声明键盘钩子的封送结构类型 
        public class KeyboardHookStruct

        {
            public int vkCode; //表示一个在1到254间的虚似键盘码 
            public int scanCode; //表示硬件扫描码 
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        #endregion

        #region 成员变量、委托、事件
        /// <summary>
        /// 钩子程序（常规）委托类型。
        /// 定义了一个钩子过程必须遵循的签名，以便能够作为回调函数被系统调用。
        /// </summary>
        /// <param name="nCode">钩子代码，表示钩子事件的类型。由系统定义，不同的钩子类型有不同的代码。</param>
        /// <param name="wParam">与钩子事件相关的wParam参数，其含义取决于具体的钩子类型。</param>
        /// <param name="lParam">与钩子事件相关的lParam参数，通常是一个指向事件相关结构的指针。</param>
        /// <returns>钩子过程处理完事件后应返回的值。这个值通常会被传递给钩子链中的下一个钩子过程，或者由系统用于特定的目的。</returns>
        private delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        /// <summary>
        /// 键盘回调（常规）委托类型
        /// </summary>
        /// <param name="wParam">按键的状态</param>
        /// <param name="keyboardHookStruct">存储着虚拟键码</param>
        public delegate void GlobalKeyboardHandler(Int32 wParam, KeyboardHookStruct keyboardHookStruct);
        /// <summary>
        /// 全局键盘回调事件（内存唯一，实例通用）
        /// </summary>
        private static event GlobalKeyboardHandler Handlers;
        /// <summary>
        /// 全局键盘函数引用（内存唯一，实例通用），是由钩子程序（常规）委托类型所声明的委托变量
        /// </summary>
        private static HookProc keyboardHookProcedure;
        /// <summary>
        /// 全局键盘钩子句柄（内存唯一，实例通用）
        /// </summary>
        private static int hHook;
        /// <summary>
        /// 锁
        /// </summary>
        private readonly object lockObject = new object();
        /// <summary>
        /// 当前状态是否已经启动
        /// </summary>
        private volatile bool isStart = false;
        #endregion

        #region Win32的Api
        /// <summary>
        /// 安装钩子的函数。
        /// 将一个钩子过程安装到钩子链中，以便能够拦截和处理特定的消息或事件。
        /// </summary>
        /// <param name="idHook">钩子的类型。这是一个由系统定义的常量，表示要安装的钩子类型（如键盘钩子、鼠标钩子等）。</param>
        /// <param name="lpfn">指向钩子过程的委托。当钩子事件发生时，系统会调用这个委托指向的函数。</param>
        /// <param name="hInstance">包含lpfn所指的子程的DLL的句柄。如果dwThreadId参数为0或是一由别的进程创建的线程的标识，hMod必须为包含lpfn所指的子程的DLL的句柄。
        /// 如果dwThreadId标识当前进程创建的一个线程，而且子程代码位于当前进程，hMod必须为NULL。
        /// 可以很简单的设定其为本应用程序的实例句柄(this.Handle).</param>
        /// <param name="threadId">与安装的钩子子程相关联的线程的标识符。
        /// 如果为0，钩子子程与所有的线程关联，即为全局钩子。
        /// </param>
        /// <returns>如果函数成功，返回值是钩子的句柄。如果函数失败，返回值为0。
        /// 返回的句柄可以在之后用于卸载钩子。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        /// <summary>
        /// 卸下钩子的函数。
        /// 从钩子链中移除一个先前安装的钩子过程。
        /// </summary>
        /// <param name="idHook">要卸载的钩子的句柄。这个句柄应该是之前通过SetWindowsHookEx函数安装钩子时返回的句柄。</param>
        /// <returns>如果函数成功，返回值为true。如果函数失败，返回值为false。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        /// <summary>
        /// 调用下一个钩挂的函数，将钩子链中的控制权传递给下一个钩子过程。
        /// 这是钩子链机制的一部分，允许当前钩子处理完毕后，继续让其他钩子处理该消息或事件。
        /// </summary>
        /// <param name="idHook">钩子的句柄，由SetWindowsHookEx函数返回。</param>
        /// <param name="nCode">钩子代码，表示钩子事件的类型。这个值由系统定义，不同的钩子类型有不同的代码。</param>
        /// <param name="wParam">与钩子事件相关的wParam参数，其含义取决于具体的钩子类型。</param>
        /// <param name="lParam">与钩子事件相关的lParam参数，通常是一个指向事件相关结构的指针。</param>
        /// <returns>如果函数成功，返回值是由下一个钩子过程返回的值。如果当前钩子过程是钩子链中的最后一个，或者钩子链被某个钩子过程中断，则返回特定的值。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>
        /// 获取一个模块（如DLL或EXE文件）的句柄。
        /// 这个函数通常用于获取包含特定函数或资源的模块的句柄，以便后续的操作（如动态加载函数）。
        /// </summary>
        /// <param name="lpModuleName">要获取句柄的模块的名称。如果为NULL，则获取当前进程的模块句柄。</param>
        /// <returns>如果函数成功，返回值是指定模块的句柄。如果函数失败，返回值为NULL。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        #region 单例模式
        private static volatile KeyboardHook MyKeyboard;
        private readonly static object createLock = new object();
        private KeyboardHook() { }
        public static KeyboardHook GetKeyboardHook()
        {
            if (MyKeyboard == null)
            {
                lock (createLock)
                {
                    if (MyKeyboard == null)
                    {
                        MyKeyboard = new KeyboardHook();
                    }
                }
            }
            return MyKeyboard;
        }
        #endregion

        /// <summary>
        /// 安装钩子
        /// </summary>
        public void Start()
        {
            if (isStart)
            {
                return;
            }
            lock (lockObject)
            {
                if (isStart)
                {
                    return;
                }
                if (Handlers == null)
                {
                    throw new Exception("Please set handler first!Then run Start");
                }
                keyboardHookProcedure = new HookProc(KeyboardHookProc);
                Process cProcess = Process.GetCurrentProcess();
                ProcessModule cModule = cProcess.MainModule;
                var mh = GetModuleHandle(cModule.ModuleName);
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHookProcedure, mh, 0);
                isStart = true;
            }
        }

        /// <summary>
        /// 卸载钩子
        /// </summary>
        public void Stop()
        {
            if (!isStart)
            {
                return;
            }
            lock (lockObject)
            {
                if (!isStart)
                {
                    return;
                }
                UnhookWindowsHookEx(hHook);
                // 清除所有事件
                Handlers = null;
                isStart = false;
            }
        }

        /// <summary>
        /// 键盘的系统回调函数
        /// </summary>
        /// <param name="nCode">如果代码小于零，则挂钩过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。此参数可以是下列值之一(见虚拟键码)</param>
        /// <param name="wParam">鼠标输入通知，代表发生的鼠标的事件，各种虚拟键事件发生后传入此参（WM_）</param>
        /// <param name="lParam">平台特定整数类型（是一个结构体），用于本机资源（存储着互动窗口句柄等相关事件信息）多线程安全使用</param>
        /// <returns></returns>
        private static int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            //如果该消息被丢弃（nCode<0）或者没有事件绑定处理程序则不会触发事件
            if ((nCode >= 0) && Handlers != null)
            {
                KeyboardHookStruct KeyDataFromHook = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                Handlers(wParam, KeyDataFromHook);
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// 添加按键的回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void AddKeyboardHandler(GlobalKeyboardHandler handler)
        {
            Handlers += handler;
        }

        /// <summary>
        /// 删除指定按键的回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveKeyboardHandler(GlobalKeyboardHandler handler)
        {
            if (Handlers != null)
            {
                Handlers -= handler;
            }
        }
    }
}

//volatile关键字表明该字段可能会被多个线程同时访问
//其值的更改必须对所有线程立即可见：当一个线程修改了一个被volatile修饰的字段时，这个修改会立即反映到主内存中，而不是仅仅停留在该线程的本地缓存中。这意味着其他线程在访问这个字段时，会直接从主内存中读取最新的值，而不是从它们的本地缓存中读取可能已经过时的值。
//‌防止编译器优化‌：告诉编译器不要对这个字段的访问进行优化，特别是不要将其值缓存到寄存器或本地内存中。这是因为编译器通常会对代码进行优化以提高性能，但在多线程环境下，这种优化可能会导致数据不一致性问题。
//‌不保证原子性‌：重要的是要理解volatile并不保证对字段的访问是原子的。也就是说，如果多个线程同时对同一个volatile字段进行读写操作，仍然可能会出现竞态条件和数据不一致性问题。因此，volatile通常用于简单的状态标志或检查点，而不是用于需要原子性操作的场景。
//‌与lock、Interlocked等机制的比较‌：在需要确保线程安全的情况下，volatile关键字通常是不足够的。对于需要原子性操作的场景如递增、递减或疯狂更新复杂数据结构应该使用lock语句、Interlocked类提供的原子操作方法或其他同步机制来确保线程安全。
//‌使用场景‌：volatile关键字的一个常见使用场景是实现轻量级的线程通信，例如使用volatile字段作为线程之间传递状态信息的标志。另一个场景是访问由非C#代码（如C或C++编写的库）修改的字段，这些字段可能会被直接映射到硬件或操作系统的资源上。

//event关键字用于声明一个事件（一种特殊的委托），它提供了发布/订阅模式，允许一个或多个事件处理程序（订阅者）与事件源（发布者）进行通信。当在类型中声明一个事件时，实际上是在创建一个可以附加和分离事件处理程序的机制。
//‌事件（event）与委托（delegate）的区别‌：
//使用event关键字声明的是一个事件，意味着可以使用+=和-=运算符来附加和分离事件处理程序
//当只声明委托类型（delegate）的变量（不使用event关键字）时，只是一个普通委托实例，虽可像调用普通方法一样调用它，但不能像事件那样附加和分离处理程序
//‌封装性‌：使用event关键字限制了外部代码直接访问委托实例，从而防止了外部代码随意更改事件处理程序列表
//没使用event关键字，外部代码直接访问和修改委托实例可能会导致不可预测的行为或安全漏洞
//‌事件模型‌：使用event关键字符合C#的事件模型，这是.NET框架中用于处理事件的标准方式，代码更容易与其他.NET代码集成和互操作，不使用event关键字而直接使用委托可能会导致代码与其他期望使用事件模型的.NET代码不兼容
//‌语法限制‌：当使用event关键字时，不能像调用普通方法那样直接调用事件，你需要通过触发事件（通常是在事件源内部）来间接调用附加的事件处理程序。对于普通的委托变量在调用上稍微自由，可直接调用它就像调用任何函数一样。
//特定情况下，如果打算让外部代码能够附加和分离事件处理程序（例如当键盘事件发生时），那么应该用event关键字，这样就可利用C#事件模型来管理事件处理程序，并确保代码具有良好的封装性和可维护性。
