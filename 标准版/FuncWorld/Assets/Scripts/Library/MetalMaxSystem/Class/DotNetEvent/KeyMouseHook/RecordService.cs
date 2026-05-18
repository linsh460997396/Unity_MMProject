//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
using System;
#if UNITY_EDITOR || UNITY_STANDALONE
//Unity编辑器、独立应用程序(不包括Web播放器)
using Vector3F = UnityEngine.Vector3;
#elif MONOGAME
//使用VS2022的MonoGame插件框架
using Vector3F = Microsoft.Xna.Framework.Vector3;
#else
using Vector3F = System.Numerics.Vector3;
#endif

namespace MetalMaxSystem
{
    //虚拟键码:https://learn.microsoft.com/zh-cn/windows/win32/inputdev/virtual-key-codes?redirectedfrom=MSDN

    /// <summary>
    /// 监听服务
    /// </summary>
    public class RecordService
    {
        #region 字段及其属性方法

        //↓键盘双击事件专用

        /// <summary>
        /// 上一次按下按键的虚拟键码,默认-1表示无按键.
        /// </summary>
        private int lastKeyDownValue = -1;
        /// <summary>
        /// 上一次按下按键的时间,默认最小值.
        /// </summary>
        private DateTime lastKeyDownTime = DateTime.MinValue;
        /// <summary>
        /// 双击事件间隔时间(毫秒)
        /// </summary>
        private const int doubleClickIntervalMs = 250;

        /// <summary>
        /// 当RecordService配合MainUpdate监听键鼠事件,该状态为真.
        /// </summary>
        public bool mainUpdateState = false;

        /// <summary>
        /// 鼠标钩子实例
        /// </summary>
        private readonly MouseHook MyMouseHook;

        /// <summary>
        /// 键盘钩子实例
        /// </summary>
        private readonly KeyboardHook MyKeyboardHook;

        //↓用户可自定义函数引用(委托),默认是空的,它们在监听到事件发生时会自动去执行,但底层事件频率高,除非特殊需要,否则不推荐在上面添加耗时方法,可使用降频版事件转发(详见MMCore键鼠事件启停封装).
        public KeyDownEventFuncref KeyDownEvent;
        public KeyUpEventFuncref KeyUpEvent;
        public MouseMoveEventFuncref MouseMoveEvent;
        public MouseDownEventFuncref MouseDownEvent;
        public MouseUpEventFuncref MouseUpEvent;
        public MouseDoubleClickEventFuncref MouseDoubleClickEvent;
        public KeyDoubleClickEventFuncref KeyDoubleClickEvent; //只有按键双击需要根据按键间隔来封装发送
        #endregion

        #region 钩子开关

        /// <summary>
        /// [构造函数]创建监听服务并默认用户玩家编号=MMCore.LocalID
        /// </summary>
        public RecordService()
        {
            PlayerID = MMCore.LocalID;
            MyMouseHook = MouseHook.GetMouseHook();
            MyKeyboardHook = KeyboardHook.GetKeyboardHook();
        }

        /// <summary>
        /// [构造函数]创建监听服务,可自定用户玩家编号(范围是1-15)
        /// </summary>
        public RecordService(int player)
        {
            PlayerID = player;
            MyMouseHook = MouseHook.GetMouseHook();
            MyKeyboardHook = KeyboardHook.GetKeyboardHook();
        }

        /// <summary>
        /// 开启鼠标钩子
        /// </summary>
        /// <param name="handler"></param>
        public void StartMouseHook()
        {
            MyMouseHook.AddMouseHandler(MouseEventHandler);
            MyMouseHook.Start();
        }

        /// <summary>
        /// 关闭鼠标钩子
        /// </summary>
        public void StopMouseHook()
        {
            MyMouseHook.Stop();
        }

        /// <summary>
        /// 开启键盘钩子
        /// </summary>
        /// <param name="handler"></param>
        public void StartKeyboardHook()
        {
            MyKeyboardHook.AddKeyboardHandler(KeyboardEventHandler);
            MyKeyboardHook.Start();
        }

        /// <summary>
        /// 关闭键盘钩子
        /// </summary>
        public void StopKeyboardHook()
        {
            MyKeyboardHook.Stop();
        }

        #endregion

        #region 热键操作

        #region 成员变量

        private int _x;

        /// <summary>
        /// 鼠标当前位置的X坐标
        /// </summary>
        public int X
        {
            get
            {
                if (_x < 0)
                {
                    _x = 0;
                }
                return _x;
            }
            set { _x = value; }
        }

        private int _y;

        /// <summary>
        /// 鼠标当前位置的Y坐标
        /// </summary>
        public int Y
        {
            get
            {
                if (_y < 0)
                {
                    _y = 0;
                }
                return _y;
            }
            set { _y = value; }
        }

        private float _z;

        /// <summary>
        /// 鼠标当前位置的Z坐标
        /// </summary>
        public float Z
        {
            get
            {
                if (_z < 0)
                {
                    _z = 0;
                }
                return _z;
            }
            set { _z = value; }
        }

        /// <summary>
        /// 鼠标按键状态标志
        /// </summary>
        public int MouseWParam { get; set; }
        /// <summary>
        /// 键盘按键状态标志
        /// </summary>
        public int KeyWParam‌ { get; set; }
        /// <summary>
        /// 键盘虚拟键码值(1~254)
        /// </summary>
        public int VKCode { get; set; }

        private bool[] _CtrlAlt = new bool[2] { false, false };
        /// <summary>
        /// 同时按下Ctrl和Alt
        /// </summary>
        public bool[] CtrlAlt
        {
            get { return _CtrlAlt; }
            set { _CtrlAlt = value; }
        }

        private bool[] _OneToNine = new bool[10]
        {
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false,
            false
        };
        /// <summary>
        /// 1-9(主键盘)
        /// </summary>
        public bool[] OneToNine
        {
            get { return _OneToNine; }
            set { _OneToNine = value; }
        }

        /// <summary>
        /// 玩家编号
        /// </summary>
        public int PlayerID { get; set; }

        #region 状态记录字段(用于MainUpdate降频读取)

        /// <summary>
        /// 键盘按键状态数组(虚拟键码->状态)
        /// </summary>
        private bool[] virtualKeyStates = new bool[256];

        /// <summary>
        /// 鼠标按键状态数组(1-5对应MMcore鼠标按键)
        /// </summary>
        private bool[] mouseStates = new bool[6];

        /// <summary>
        /// 鼠标双击状态数组(1-5对应MMcore鼠标按键)
        /// </summary>
        private bool[] mouseDoubleClickStates = new bool[6];

        /// <summary>
        /// 获取指定虚拟键码的按键状态
        /// </summary>
        /// <param name="virtualKey">虚拟键码(1-254)</param>
        /// <returns>true表示按下</returns>
        public bool GetVirtualKeyState(int virtualKey)
        {
            if (virtualKey >= 0 && virtualKey < virtualKeyStates.Length)
                return virtualKeyStates[virtualKey];
            return false;
        }

        /// <summary>
        /// 设置指定虚拟键码的按键状态
        /// </summary>
        /// <param name="virtualKey">虚拟键码(1-254)</param>
        /// <param name="pressed">是否按下</param>
        public void SetVirtualKeyState(int virtualKey, bool pressed)
        {
            if (virtualKey >= 0 && virtualKey < virtualKeyStates.Length)
                virtualKeyStates[virtualKey] = pressed;
        }

        /// <summary>
        /// 获取指定鼠标按键的状态
        /// </summary>
        /// <param name="mouseButton">采用MMCore鼠标按键(1-5)</param>
        /// <returns>true表示按下</returns>
        public bool GetMouseState(int mouseButton)
        {
            if (mouseButton >= 1 && mouseButton <= 5)
                return mouseStates[mouseButton];
            return false;
        }

        /// <summary>
        /// 设置指定鼠标按键的状态
        /// </summary>
        /// <param name="mouseButton">采用MMCore鼠标按键(1-5)</param>
        /// <param name="pressed">是否按下</param>
        public void SetMouseState(int mouseButton, bool pressed)
        {
            if (mouseButton >= 1 && mouseButton <= 5)
                mouseStates[mouseButton] = pressed;
        }

        /// <summary>
        /// 获取指定鼠标按键的双击状态
        /// </summary>
        /// <param name="mouseButton">采用MMCore鼠标按键(1-5)</param>
        /// <returns>true表示双击</returns>
        public bool GetMouseDoubleClickState(int mouseButton)
        {
            if (mouseButton >= 1 && mouseButton <= 5) return mouseDoubleClickStates[mouseButton];
            return false;
        }

        /// <summary>
        /// 设置指定鼠标按键的双击状态
        /// </summary>
        /// <param name="mouseButton">采用MMCore鼠标按键(1-5)</param>
        /// <param name="doubleClicked">是否双击</param>
        public void SetMouseDoubleClickState(int mouseButton, bool doubleClicked)
        {
            if (mouseButton >= 1 && mouseButton <= 5)
                mouseDoubleClickStates[mouseButton] = doubleClicked;
        }

        #endregion

        #endregion

        #region 鼠标事件处理

        /// <summary>
        /// 鼠标事件处理函数,对接系统底层非托管事件,并按用户设定线程的周期间隔将指定状态记录到托管数据,本函数不应做消耗时间的动作.
        /// </summary>
        /// <param name="wParam">按键状态标志</param>
        /// <param name="mouseMsg">存储着鼠标信息</param>
        private void MouseEventHandler(int wParam, MouseHook.MouseHookStruct mouseMsg)
        {
            MouseWParam = wParam;
            switch (wParam)
            {
                case MouseHook.WM_MOUSEMOVE:
                    //鼠标移动位置(整数UI坐标)
                    X = mouseMsg.pt.x;
                    Y = mouseMsg.pt.y;
                    if (MouseMoveEvent != null)
                    {
                        MouseMoveEvent.Invoke(PlayerID, X, Y);
                    }
                    break;
                case MouseHook.WM_LBUTTONDOWN:
                    //鼠标左键按下
                    SetMouseState(MMCore.c_mouseButtonLeft, true);
                    if (MouseDownEvent != null)
                    {
                        MouseDownEvent.Invoke(PlayerID, MMCore.c_mouseButtonLeft, X, Y);
                    }
                    break;
                case MouseHook.WM_LBUTTONUP:
                    //鼠标左键弹起
                    SetMouseState(MMCore.c_mouseButtonLeft, false);
                    if (MouseUpEvent != null)
                    {
                        MouseUpEvent.Invoke(PlayerID, MMCore.c_mouseButtonLeft, X, Y);
                    }
                    break;
                case MouseHook.WM_RBUTTONDOWN:
                    //鼠标右键按下
                    SetMouseState(MMCore.c_mouseButtonRight, true);
                    if (MouseDownEvent != null)
                    {
                        MouseDownEvent.Invoke(PlayerID, MMCore.c_mouseButtonRight, X, Y);
                    }
                    break;
                case MouseHook.WM_RBUTTONUP:
                    //鼠标右键弹起
                    SetMouseState(MMCore.c_mouseButtonRight, false);
                    if (MouseUpEvent != null)
                    {
                        MouseUpEvent.Invoke(PlayerID, MMCore.c_mouseButtonRight, X, Y);
                    }
                    break;
                case MouseHook.WM_LBUTTONDBLCLK:
                    //鼠标左键双击
                    SetMouseDoubleClickState(MMCore.c_mouseButtonLeft, true);
                    if (MouseDoubleClickEvent != null)
                    {
                        MouseDoubleClickEvent.Invoke(PlayerID, MMCore.c_mouseButtonLeft, X, Y);
                    }
                    break;
                case MouseHook.WM_RBUTTONDBLCLK:
                    //鼠标右键双击
                    SetMouseDoubleClickState(MMCore.c_mouseButtonRight, true);
                    if (MouseDoubleClickEvent != null)
                    {
                        MouseDoubleClickEvent.Invoke(PlayerID, MMCore.c_mouseButtonRight, X, Y);
                    }
                    break;
            }
        }

        #endregion

        #region 键盘事件处理
        //虚拟键码
        private const int CTRL = 162;
        private const int ALT = 164;
        private const int ZERO = 48;

        /*
         * ctrl 162
         * alt 164
         * 1-9 -> 49-57
         * 0 -> 48
         */

        /// <summary>
        /// 键盘事件处理函数,对接系统底层非托管事件,并按用户设定线程的周期间隔将指定状态记录到托管数据,本函数不应做消耗时间的动作.
        /// </summary>
        /// <param name="wParam">按键状态标志</param>
        /// <param name="keyboardHookStruct">存储着虚拟键码</param>
        private void KeyboardEventHandler(Int32 wParam, KeyboardHook.KeyboardHookStruct keyboardHookStruct)
        {
            KeyWParam‌ = wParam;
            VKCode = keyboardHookStruct.vkCode;
            //热键判断
            if (KeyWParam‌ == KeyboardHook.WM_KEYDOWN || KeyWParam‌ == KeyboardHook.WM_SYSKEYDOWN)
            {
                //按下
                switch (VKCode)
                {
                    case CTRL:
                        CtrlAlt[0] = true;
                        break;
                    case ALT:
                        CtrlAlt[1] = true;
                        break;
                }
                if (VKCode >= ZERO && VKCode <= (ZERO + 9))
                {
                    //按下了0-9(主键盘)
                    OneToNine[VKCode - ZERO] = true;
                }
                SetVirtualKeyState(VKCode, true);
                if (KeyDownEvent != null)
                {
                    KeyDownEvent.Invoke(PlayerID, VKCode);
                }
                //键盘双击
                if (lastKeyDownValue == VKCode && (DateTime.Now - lastKeyDownTime).TotalMilliseconds <= doubleClickIntervalMs)
                {
                    lastKeyDownValue = -1;
                    lastKeyDownTime = DateTime.MinValue;
                    if (KeyDoubleClickEvent != null)
                    {
                        KeyDoubleClickEvent.Invoke(PlayerID, VKCode);
                    }
                }
                else
                {
                    lastKeyDownValue = VKCode;
                    lastKeyDownTime = DateTime.Now;
                }
            }
            else if (KeyWParam‌ == KeyboardHook.WM_KEYUP || KeyWParam‌ == KeyboardHook.WM_SYSKEYUP)
            {
                //松开
                switch (VKCode)
                {
                    case CTRL:
                        CtrlAlt[0] = false;
                        break;
                    case ALT:
                        CtrlAlt[1] = false;
                        break;
                }
                if (VKCode >= ZERO && VKCode <= (ZERO + 9))
                {
                    //按下了0-9(主键盘)
                    OneToNine[VKCode - ZERO] = false;
                }
                SetVirtualKeyState(VKCode, false);
                if (KeyUpEvent != null)
                {
                    KeyUpEvent.Invoke(PlayerID, VKCode);
                }
            }
        }

        /// <summary>
        /// 是否按下了Ctrl + Alt + 0-9(主键盘)
        /// </summary>
        /// <returns>返回0-9代表按下了Ctrl + Alt + 0-9(主键盘),返回-1代表没有按下</returns>
        public int IsPressCtrlAltNum()
        {
            if ((!CtrlAlt[0]) || (!CtrlAlt[1]))
            {
                return -1;
            }
            for (int i = 0; i < 10; i++)
            {
                if (OneToNine[i])
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 是否按下了Ctrl + 0-9(主键盘)
        /// </summary>
        /// <returns>返回0-9代表按下了Ctrl + 0-9(主键盘),返回-1代表没有按下</returns>
        public int IsPressCtrlNum()
        {
            if (!CtrlAlt[0])
            {
                return -1;
            }
            for (int i = 0; i < 10; i++)
            {
                if (OneToNine[i])
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 是否按下了Alt + 0-9(主键盘)
        /// </summary>
        /// <returns>返回0-9代表按下了Alt + 0-9(主键盘),返回-1代表没有按下</returns>
        public int IsPressAltNum()
        {
            if (!CtrlAlt[1])
            {
                return -1;
            }
            for (int i = 0; i < 10; i++)
            {
                if (OneToNine[i])
                {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #endregion
    }
}
