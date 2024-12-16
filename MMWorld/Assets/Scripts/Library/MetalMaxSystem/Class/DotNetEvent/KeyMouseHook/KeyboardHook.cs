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
        private static int hHook;
        private static HookProc KeyboardHookDelegate;
        /// <summary>
        /// 键盘回调委托
        /// </summary>
        /// <param name="wParam">按键的状态</param>
        /// <param name="keyboardHookStruct">存储着虚拟键码</param>
        public delegate void KeyboardHandler(Int32 wParam, KeyboardHookStruct keyboardHookStruct);
        // 键盘回调事件
        private static event KeyboardHandler Handlers;
        // 锁
        private readonly object lockObject = new object();
        // 当前状态,是否已经启动
        private volatile bool isStart = false;
        #endregion

        #region Win32的Api
        private delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        //安装钩子的函数 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //卸下钩子的函数 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        //下一个钩挂的函数 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

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
                KeyboardHookDelegate = new HookProc(KeyboardHookProc);
                Process cProcess = Process.GetCurrentProcess();
                ProcessModule cModule = cProcess.MainModule;
                var mh = GetModuleHandle(cModule.ModuleName);
                hHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookDelegate, mh, 0);
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
        public void AddKeyboardHandler(KeyboardHandler handler)
        {
            Handlers += handler;
        }

        /// <summary>
        /// 删除指定按键的回调函数
        /// </summary>
        /// <param name="handler"></param>
        public void RemoveKeyboardHandler(KeyboardHandler handler)
        {
            if (Handlers != null)
            {
                Handlers -= handler;
            }
        }
    }
}
