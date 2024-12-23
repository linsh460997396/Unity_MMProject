using System;
#if UNITY_EDITOR|| UNITY_STANDALONE
//Unity编辑器、独立应用程序（不包括Web播放器）
using Vector3F = UnityEngine.Vector3;
#elif MonoGame
//使用VS2022的MonoGame插件框架
using Vector3F = Microsoft.Xna.Framework.Vector3;
#else
using Vector3F = System.Numerics.Vector3;
#endif

namespace MetalMaxSystem
{
    //虚拟键码：https://learn.microsoft.com/zh-cn/windows/win32/inputdev/virtual-key-codes?redirectedfrom=MSDN
    //CSDN原址：https://blog.csdn.net/qq_43851684/article/details/113096306

    /// <summary>
    /// 监听服务
    /// </summary>
    public class RecordService
    {
        #region 字段及其属性方法

        private bool _defaultEvent = false;
        /// <summary>
        /// 判断键鼠总控预制事件是否已注册，通过函数“AddKeyMouseEvent”可快捷将本类实例中5个预制事件KeyDown、KeyUp、MouseMove、MouseDown、MouseUp注册给库内预制函数引用，从而使用按键总控管理衍生的所有功能（比如将“移动”、“发射火箭”等函数动作注册给Q键）
        /// </summary>
        public bool DefaultEvent
        {
            get
            {
                return _defaultEvent;
            }

            set
            {
                _defaultEvent = value;
            }
        }

        /// <summary>
        /// 鼠标钩子实例
        /// </summary>
        private readonly MouseHook MyMouseHook;
        /// <summary>
        /// 键盘钩子实例
        /// </summary>
        private readonly KeyboardHook MyKeyboardHook;

        //↓用户可自定义函数引用（委托），在监听到事件发生时去执行
        public KeyDownEventFuncref KeyDownEvent;
        public KeyDoubleClickEventFuncref KeyDoubleClickEvent;
        public KeyUpEventFuncref KeyUpEvent;
        public MouseMoveEventFuncref MouseMoveEvent;
        public MouseDownEventFuncref MouseDownEvent;
        public MouseLDoubleClickEventFuncref MouseLDoubleClickEvent;
        public MouseRDoubleClickEventFuncref MouseRDoubleClickEvent;
        public MouseUpEventFuncref MouseUpEvent;

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
        /// [构造函数]创建监听服务，可自定用户玩家编号（范围是1-15）
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
                if (_x < 0) { _x = 0; }
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
                if (_y < 0) { _y = 0; }
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
                if (_z < 0) { _z = 0; }
                return _z;
            }
            set { _z = value; }
        }

        /// <summary>
        /// 被按下的鼠标按键
        /// </summary>
        public int WParam { get; set; }

        /// <summary>
        /// 键钮状态
        /// </summary>
        public int KeyStatus { get; set; }

        /// <summary>
        /// 键码
        /// </summary>
        public int KeyValue { get; set; }

        private bool[] _CtrlAlt = new bool[2] { false, false };

        /// <summary>
        /// Ctrl 和 Alt
        /// </summary>
        public bool[] CtrlAlt
        {
            get { return _CtrlAlt; }
            set { _CtrlAlt = value; }
        }

        private bool[] _OneToNine = new bool[10] { false, false, false, false, false, false, false, false, false, false };

        /// <summary>
        /// 1-9
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

        #endregion

        //以下处理动作不宜放时间复杂度高的函数动作，建议只记录变量变化，由其他线程读取这些变量后决定触发动作

        #region 鼠标事件处理

        /// <summary>
        /// 鼠标事件处理函数，对接系统底层非托管事件，并按用户设定线程的周期间隔将指定状态记录到托管数据，本函数不做其他消耗时间的处理，请另外读取这些状态数据来组织动作
        /// </summary>
        /// <param name="wParam">鼠标事件状态</param>
        /// <param name="mouseMsg">存储着鼠标信息</param>
        private void MouseEventHandler(int wParam, MouseHook.MouseHookStruct mouseMsg)
        {
            this.WParam = wParam;
            switch (wParam)
            {
                case MouseHook.WM_MOUSEMOVE:
                    // 记录鼠标移动位置
                    X = mouseMsg.pt.x;//UI坐标，是整数
                    Y = mouseMsg.pt.y;//UI坐标，是整数

                    //注：到此记录了WParam和X,Y即可，不宜写时间复杂度较高的逻辑，剩下的"按键总控"功能（给按键注册注销更换委托函数，用于蓄力、移动、释放技能、按弹菜单等游戏世界逻辑）通过读取本类信息另外开线程去制作即可

                    //思考：X,Y如何转化为世界坐标？

                    //Z从上述（X,Y）的信息中获得
                    try
                    {
                        //鼠标的移动会出界导致负数，X和Y在小于0时其属性方法必须纠正为0
                        Z = Game.MapHeight + Game.TerrainHeight[X, Y] + MMCore.DictionaryFloatLoad0(true, "Unit.TerrainHeight");
                    }
                    catch (Exception ex)
                    {
                        // 捕获异常并打印错误信息
                        //Debug.WriteLine("Error:X{0},Y{1}", X, Y);
                        //Debug.Log(ex.Message); //需引用Unity方法库
                        Console.WriteLine(ex.Message);
                        // 抛出异常，将错误信息传递给上层调用者
                        throw;
                    }

                    //MouseMoveEvent?.Invoke(PlayerID, new Vector3F(X, Y, Z), X, Y);//当没给函数注册事件时不运行
                    if (MouseMoveEvent != null)
                    {
                        MouseMoveEvent.Invoke(PlayerID, new Vector3F(X, Y, Z), X, Y);
                    }

                    break;
                case MouseHook.WM_LBUTTONDOWN:
                    // 记录鼠标左键按下
                    //MouseDownEvent?.Invoke(PlayerID, MMCore.c_mouseButtonLeft, SWPlayer.MouseVector3F[PlayerID], X, Y);
                    if (MouseDownEvent != null)
                    {
                        MouseDownEvent.Invoke(PlayerID, MMCore.c_mouseButtonLeft, Player.MouseVector3F[PlayerID], X, Y);
                    }

                    break;
                case MouseHook.WM_LBUTTONUP:
                    // 记录鼠标左键弹起
                    //MouseUpEvent?.Invoke(PlayerID, MMCore.c_mouseButtonLeft, SWPlayer.MouseVector3F[PlayerID], X, Y);
                    if (MouseUpEvent != null)
                    {
                        MouseUpEvent.Invoke(PlayerID, MMCore.c_mouseButtonLeft, Player.MouseVector3F[PlayerID], X, Y);
                    }

                    break;
                case MouseHook.WM_LBUTTONDBLCLK:
                    // 记录鼠标左键双击
                    //MouseLDoubleClickEvent?.Invoke(PlayerID, SWPlayer.MouseVector3F[PlayerID], X, Y);
                    if (MouseLDoubleClickEvent != null)
                    {
                        MouseLDoubleClickEvent.Invoke(PlayerID, Player.MouseVector3F[PlayerID], X, Y);
                    }

                    break;
                case MouseHook.WM_RBUTTONDOWN:
                    // 记录鼠标右键按下
                    //MouseDownEvent?.Invoke(PlayerID, MMCore.c_mouseButtonRight, SWPlayer.MouseVector3F[PlayerID], X, Y);
                    if (MouseDownEvent != null)
                    {
                        MouseDownEvent.Invoke(PlayerID, MMCore.c_mouseButtonRight, Player.MouseVector3F[PlayerID], X, Y);
                    }

                    break;
                case MouseHook.WM_RBUTTONUP:
                    // 记录鼠标右键弹起
                    //MouseUpEvent?.Invoke(PlayerID, MMCore.c_mouseButtonRight, SWPlayer.MouseVector3F[PlayerID], X, Y);
                    if (MouseUpEvent != null)
                    {
                        MouseUpEvent.Invoke(PlayerID, MMCore.c_mouseButtonRight, Player.MouseVector3F[PlayerID], X, Y);
                    }

                    break;
                case MouseHook.WM_RBUTTONDBLCLK:
                    // 记录鼠标右键双击
                    //MouseRDoubleClickEvent?.Invoke(PlayerID, SWPlayer.MouseVector3F[PlayerID], X, Y);
                    if (MouseRDoubleClickEvent != null)
                    {
                        MouseRDoubleClickEvent.Invoke(PlayerID, Player.MouseVector3F[PlayerID], X, Y);
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
         * 1-9 -》 49-57
         * 0 -》48
         */

        /// <summary>
        /// 键盘事件处理函数，对接系统底层非托管事件，并按用户设定线程的周期间隔将指定状态记录到托管数据，本函数不做其他消耗时间的处理，请另外读取这些状态数据来组织动作
        /// </summary>
        /// <param name="wParam">键盘事件状态</param>
        /// <param name="keyboardHookStruct">存储着虚拟键码</param>
        private void KeyboardEventHandler(Int32 wParam, KeyboardHook.KeyboardHookStruct keyboardHookStruct)
        {
            KeyStatus = wParam;
            KeyValue = keyboardHookStruct.vkCode;
            // 热键判断
            if (KeyStatus == KeyboardHook.WM_KEYDOWN || KeyStatus == KeyboardHook.WM_SYSKEYDOWN)
            {
                // 按下某个按钮
                switch (KeyValue)
                {
                    case CTRL:
                        CtrlAlt[0] = true;
                        break;
                    case ALT:
                        CtrlAlt[1] = true;
                        break;
                }
                if (KeyValue >= ZERO && KeyValue <= (ZERO + 9))
                {
                    // 按下了0-9
                    int temp = KeyValue - ZERO;
                    OneToNine[temp] = true;
                }
            }
            else if (KeyStatus == KeyboardHook.WM_KEYUP || KeyStatus == KeyboardHook.WM_SYSKEYUP)
            {
                // 松开某个按钮
                switch (KeyValue)
                {
                    case CTRL:
                        CtrlAlt[0] = false;
                        break;
                    case ALT:
                        CtrlAlt[1] = false;
                        break;
                }
                if (KeyValue >= ZERO && KeyValue <= (ZERO + 9))
                {
                    // 按下了0-9
                    int temp = KeyValue - ZERO;
                    OneToNine[temp] = false;
                }
            }
        }

        /// <summary>
        /// 是否按下了 Ctrl + alt + 0-9
        /// </summary>
        /// <returns>返回0-9代表按下了Ctrl+alt+0-9，返回-1代表没有按下</returns>
        public int IsPressTarget()
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
        #endregion

        #endregion
    }
}
