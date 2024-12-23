using System;
using System.Runtime.InteropServices;

namespace MetalMaxSystem
{
    //虚拟键码：https://learn.microsoft.com/zh-cn/windows/win32/inputdev/virtual-key-codes?redirectedfrom=MSDN
    //CSDN原址：https://blog.csdn.net/qq_43851684/article/details/113096306

    /// <summary>
    /// 鼠标钩子
    /// </summary>
    public class MouseHook
    {
        #region 常量（用于匹配鼠标输入通知）
        public const int WM_MOUSEMOVE = 0x200; // 鼠标移动
        public const int WM_LBUTTONDOWN = 0x201;// 鼠标左键按下
        public const int WM_RBUTTONDOWN = 0x204;// 鼠标右键按下
        public const int WM_MBUTTONDOWN = 0x207;// 鼠标中键按下
        public const int WM_LBUTTONUP = 0x202;// 鼠标左键抬起
        public const int WM_RBUTTONUP = 0x205;// 鼠标右键抬起
        public const int WM_MBUTTONUP = 0x208;// 鼠标中键抬起
        public const int WM_LBUTTONDBLCLK = 0x203;// 鼠标左键双击
        public const int WM_RBUTTONDBLCLK = 0x206;// 鼠标右键双击
        public const int WM_MBUTTONDBLCLK = 0x209;// 鼠标中键双击
        public const int WH_MOUSE_LL = 14; //可以截获整个系统所有模块的鼠标事件。
        #endregion

        #region 成员变量、回调函数、事件
        /// <summary>
        /// 钩子程序（常规）委托类型，特征：int HookProc(int nCode, Int32 wParam, IntPtr lParam)
        /// </summary>
        /// <param name="nCode">如果代码小于零，则挂钩过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。此参数可以是下列值之一(见虚拟键码)</param>
        /// <param name="wParam">鼠标输入通知，代表发生的鼠标的事件，各种虚拟键事件发生后传入此参（WM_）</param>
        /// <param name="lParam">平台特定整数类型（是一个结构体），用于本机资源（存储着互动窗口句柄等相关事件信息）多线程安全使用</param>
        /// <returns></returns>
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        /// <summary>
        /// 鼠标（常规）委托类型
        /// </summary>
        /// <param name="wParam"> 鼠标输入通知，代表发生的鼠标的事件，各种虚拟键事件发生后传入此参（WM_）</param>
        /// <param name="mouseMsg">鼠标钩子结构体，存储着鼠标位置、互动窗口句柄等相关事件信息</param>
        public delegate void GlobalMouseHandler(Int32 wParam, MouseHookStruct mouseMsg);
        /// <summary>
        /// 全局鼠标回调事件（内存唯一，实例通用）
        /// </summary>
        private static event GlobalMouseHandler OnMouseActivity;
        /// <summary>
        /// 全局鼠标函数引用（内存唯一，实例通用），是由钩子程序（常规）委托类型所声明的委托变量
        /// </summary>
        private HookProc mouseHookProcedure;
        /// <summary>
        /// 全局鼠标钩子句柄（内存唯一，实例通用）
        /// </summary>
        private static int hMouseHook = 0; 
        /// <summary>
        /// 锁
        /// </summary>
        private readonly object lockObject = new object();
        /// <summary>
        /// 当前状态是否已经启动
        /// </summary>
        private bool isStart = false;
        #endregion

        #region Win32的API
        /// <summary>
        /// 钩子结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            /// <summary>
            /// 指定在屏幕坐标系下，包含有光标x、y坐标的POINT结构（鼠标位置）
            /// </summary>
            public POINT pt;
            /// <summary>
            /// 希望对鼠标事件做出响应、接收鼠标消息的窗体的句柄
            /// </summary>
            public int hWnd;
            /// <summary>
            /// 指定点击测试值，查看WM_NCHITTEST消息可以得到值的列表
            /// </summary>
            public int wHitTestCode;
            /// <summary>
            /// 指定和该消息相关联的附加信息
            /// </summary>
            public int dwExtraInfo;
        }

        /// <summary>
        /// 声明一个Point的封送类型  
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

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
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        /// <summary>
        /// 卸下钩子的函数。
        /// 从钩子链中移除一个先前安装的钩子过程。
        /// </summary>
        /// <param name="idHook">要卸载的钩子的句柄。这个句柄应该是之前通过SetWindowsHookEx函数安装钩子时返回的句柄。</param>
        /// <returns>如果函数成功，返回值为true。如果函数失败，返回值为false。</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

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
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        /// <summary>
        /// 获取一个模块（如DLL或EXE文件）的句柄。
        /// 这个函数通常用于获取包含特定函数或资源的模块的句柄，以便后续的操作（如动态加载函数）。
        /// </summary>
        /// <param name="lpModuleName">要获取句柄的模块的名称。如果为NULL，则获取当前进程的模块句柄。</param>
        /// <returns>如果函数成功，返回值是指定模块的句柄。如果函数失败，返回值为NULL。</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        #region 构造(单例模式)与析构函数
        private static volatile MouseHook MyMouseHook;
        private readonly static object createLock = new object();
        private MouseHook() { }

        public static MouseHook GetMouseHook()
        {
            if (MyMouseHook == null)
            {
                lock (createLock)
                {
                    if (MyMouseHook == null)
                    {
                        MyMouseHook = new MouseHook();
                    }
                }
            }
            return MyMouseHook;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~MouseHook()
        {
            Stop();
        }
        #endregion

        /// <summary>
        /// 启动全局钩子
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
                if (OnMouseActivity == null)
                {
                    throw new Exception("Please set handler first!Then run Start");
                }
                // 安装鼠标钩子
                if (hMouseHook == 0)
                {
                    // 生成一个HookProc的实例.
                    mouseHookProcedure = new HookProc(MouseHookProc);
                    hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, mouseHookProcedure, Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                    //假设装置失败停止钩子
                    if (hMouseHook == 0)
                    {
                        Stop();
                        throw new Exception("SetWindowsHookEx failed.");
                    }
                }
                isStart = true;
            }
        }

        /// <summary>
        /// 停止全局钩子
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
                bool retMouse = true;
                if (hMouseHook != 0)
                {
                    retMouse = UnhookWindowsHookEx(hMouseHook);
                    hMouseHook = 0;
                }
                // 假设卸下钩子失败
                if (!(retMouse))
                    throw new Exception("UnhookWindowsHookEx failed.");
                // 删除所有事件
                OnMouseActivity = null;
                // 标志位改变
                isStart = false;
            }
        }

        /// <summary>
        /// 鼠标钩子回调函数
        /// </summary>
        private static int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            // 假设正常执行而且用户要监听鼠标的消息
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                OnMouseActivity(wParam, MyMouseHookStruct);
            }
            // 启动下一次钩子
            return CallNextHookEx(hMouseHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// 注册全局鼠标事件
        /// </summary>
        /// <param name="handler"></param>
        public void AddMouseHandler(GlobalMouseHandler handler)
        {
            OnMouseActivity += handler;
        }

        /// <summary>
        /// 注销全局鼠标事件
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveMouseHandler(GlobalMouseHandler handler)
        {
            if (OnMouseActivity != null)
            {
                OnMouseActivity -= handler;
            }
        }
    }
}
