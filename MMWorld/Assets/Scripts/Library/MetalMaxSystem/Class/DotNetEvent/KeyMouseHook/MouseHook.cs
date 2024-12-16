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
        /// 钩子回调常规函数引用（委托类型），特征：int HookProc(int nCode, Int32 wParam, IntPtr lParam)
        /// </summary>
        /// <param name="nCode">如果代码小于零，则挂钩过程必须将消息传递给CallNextHookEx函数，而无需进一步处理，并且应返回CallNextHookEx返回的值。此参数可以是下列值之一(见虚拟键码)</param>
        /// <param name="wParam">鼠标输入通知，代表发生的鼠标的事件，各种虚拟键事件发生后传入此参（WM_）</param>
        /// <param name="lParam">平台特定整数类型（是一个结构体），用于本机资源（存储着互动窗口句柄等相关事件信息）多线程安全使用</param>
        /// <returns></returns>
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        /// <summary>
        /// 全局鼠标常规函数引用（委托类型）
        /// </summary>
        /// <param name="wParam"> 鼠标输入通知，代表发生的鼠标的事件，各种虚拟键事件发生后传入此参（WM_）</param>
        /// <param name="mouseMsg">鼠标钩子结构体，存储着鼠标位置、互动窗口句柄等相关事件信息</param>
        public delegate void MyMouseEventHandler(Int32 wParam, MouseHookStruct mouseMsg);
        private event MyMouseEventHandler OnMouseActivity;
        // 声明鼠标钩子事件类型
        private HookProc _mouseHookProcedure;
        private static int _hMouseHook = 0; // 鼠标钩子句柄
        // 锁
        private readonly object lockObject = new object();
        // 当前状态,是否已经启动
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

        //声明一个Point的封送类型  
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        // 装置钩子的函数
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        // 卸下钩子的函数
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        // 下一个钩挂的函数
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);
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
                if (_hMouseHook == 0)
                {
                    // 生成一个HookProc的实例.
                    _mouseHookProcedure = new HookProc(MouseHookProc);
                    _hMouseHook = SetWindowsHookEx(WH_MOUSE_LL, _mouseHookProcedure, Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]), 0);
                    //假设装置失败停止钩子
                    if (_hMouseHook == 0)
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
                if (_hMouseHook != 0)
                {
                    retMouse = UnhookWindowsHookEx(_hMouseHook);
                    _hMouseHook = 0;
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
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            // 假设正常执行而且用户要监听鼠标的消息
            if ((nCode >= 0) && (OnMouseActivity != null))
            {
                MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                OnMouseActivity(wParam, MyMouseHookStruct);
            }
            // 启动下一次钩子
            return CallNextHookEx(_hMouseHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// 注册全局鼠标事件
        /// </summary>
        /// <param name="handler"></param>
        public void AddMouseHandler(MyMouseEventHandler handler)
        {
            OnMouseActivity += handler;
        }

        /// <summary>
        /// 注销全局鼠标事件
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveMouseHandler(MyMouseEventHandler handler)
        {
            if (OnMouseActivity != null)
            {
                OnMouseActivity -= handler;
            }
        }
    }
}
