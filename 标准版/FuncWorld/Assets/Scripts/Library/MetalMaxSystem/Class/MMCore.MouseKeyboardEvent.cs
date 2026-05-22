#region 预处理指令(须靠最前)
//↓制作UnityMOD环境下手动启用(如BepInEx)
//#define UNITY_STANDALONE

//#define MONOGAME //MonoGame插件下启用(包括XNA框架)

#if !(UNITY_EDITOR || UNITY_STANDALONE || NET5_0_OR_GREATER)
↓仅针对MMCore.cs:非Unity、NET5+则启用NETFRAMEWORK(否则即便Unity的Framework也不启用)
#define NETFRAMEWORK
#endif

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Globalization;//判断英文字符用到
using System.Linq;//混肴处理字符串转义时用到
//↓防止与System.Windows.Forms.Timer混淆
using Timer = System.Threading.Timer;

#region 环境适配
#if UNITY_EDITOR || UNITY_STANDALONE
//Unity环境:编辑器、独立应用程序(不包括Web播放器)
using Mathf = UnityEngine.Mathf;
using Debug = UnityEngine.Debug;
using Vector2F = UnityEngine.Vector2;
using Vector3F = UnityEngine.Vector3;
#else
//其他.Net环境,如纯VS2022下C#环境Framwork4.8、Net5+及加载插件MonoGame、XNA的情况
using System.Diagnostics;
using System.Diagnostics.Metrics;
//↓可使用.Net中的Debug.WriteLine
using Debug = System.Diagnostics.Debug;
#if WINDOWS || NET5_0_OR_GREATER || NETFRAMEWORK
//↓支持WINDOWS框架下识别硬件标识等(若依然是灰色,请手动添加或安装程序集)
using System.Management;
using Microsoft.Win32;
using System.Windows;
#endif
#if NETFRAMEWORK
//使用VS2022的NETFRAMEWORK4.8框架时校准Mathf
using Mathf = System.Math;
#else
using Mathf = System.MathF;
#endif
#if MONOGAME
//使用VS2022的MonoGame插件框架时,校准2F3F向量
using Vector2F = Microsoft.Xna.Framework.Vector2;
using Vector3F = Microsoft.Xna.Framework.Vector3;
#else
using Vector2F = System.Numerics.Vector2;
using Vector3F = System.Numerics.Vector3;
using MetalMaxSystem;
#endif
#endif
#endregion

namespace MetalMaxSystem
{
    public partial class MMCore
    {
        #region 键鼠事件

        //开关
        public static bool chargeEnable = true;
        public static bool doubleClickEnable = true;

        //调试报告
        public static bool chargeDebug = true;
        public static bool doubleClickDebug = true;

        //其他
        public static float chargeDeltaValue = 1.0f; //蓄力增量
        public static float doubleClickDeltaValue = 0.05f; //双击增量(与MainUpdate频率一致)
        public static float doubleClickTimeLimit = 0.25f; //双击时间限制(单位秒)
        public static float doubleClickRange = 0.1f; //双击时鼠标的偏移量不能超过此值

        //↓降频版事件转发,推荐注册
        public static KeyDownEventFuncref KeyDownEvent;
        public static KeyDoubleClickEventFuncref KeyDoubleClickEvent;
        public static KeyUpEventFuncref KeyUpEvent;
        public static MouseMoveEventFuncref MouseMoveEvent;
        public static MouseDownEventFuncref MouseDownEvent;
        public static MouseDoubleClickEventFuncref MouseDoubleClickEvent;
        public static MouseUpEventFuncref MouseUpEvent;

        #region Functions 键鼠事件启停封装

        /// <summary>
        /// 启动MMCore内部键鼠事件循环.
        /// RecordService监听底层钩子记录状态,MainUpdate周期间隔读取状态并判断发送事件,避免直接挂底层事件导致阻塞卡顿.
        /// </summary>
        /// <param name="player">玩家编号,有效编号1~15</param>
        /// <param name="isBackground">是否后台线程运行</param>
        public static void StartKeyMouseEvent(int player = 1, bool isBackground = true)
        {
            if (keyMouseEventState || recordService != null) return;
            keyMouseEventState = true;
            recordService = (player <= 0 || player >= 16) ? new RecordService() : new RecordService(player);
            recordService.mainUpdateState = true;

            // 开启底层钩子监听
            recordService.StartMouseHook();
            recordService.StartKeyboardHook();

            // 注册UpdateKeyMouseEventLoop到MainUpdate的Update事件
            MainUpdate.Update += UpdateKeyMouseEventLoop;

            // 启动MainUpdate(50ms周期运行=20Frames Per Second)
            MainUpdate.Period = 50;
            MainUpdate.Duetime = 0;
            MainUpdate.Run(isBackground); //键鼠事件以后台线程运行即可
        }

        /// <summary>
        /// 停止MMCore内部键鼠事件循环
        /// </summary>
        public static void StopKeyMouseEventLoop()
        {
            if (!keyMouseEventState) return;
            // 通知停止MainUpdate
            MainUpdate.TimerStop = true;
            // 注销事件
            MainUpdate.Update -= UpdateKeyMouseEventLoop;
            // 停止底层钩子监听
            recordService.StopMouseHook();
            recordService.StopKeyboardHook();
            // 清理资源
            recordService = null;
            // 状态重置
            keyMouseEventState = false;
        }

        /// <summary>
        /// 更新键鼠事件(在MainUpdate周期调用).
        /// 读取RecordService记录的状态并判断是否发送KeyDown/KeyUp/MouseMove/MouseDown/MouseUp事件.
        /// </summary>
        private static void UpdateKeyMouseEventLoop(object sender = null, EventArgs e = null)
        {
            if (recordService == null || !keyMouseEventState || !recordService.mainUpdateState) return;
            MouseEventHandler(recordService);
            KeyboardEventHandler(recordService);
        }

        private static void MouseEventHandler(RecordService recordService)
        {
            switch (recordService.MouseWParam)
            {
                case MouseHook.WM_MOUSEMOVE:
                    //鼠标移动位置(整数UI坐标)
                    //MouseMove(recordService.PlayerID, recordService.X, recordService.Y); //直接函数测试
                    MouseMoveEvent?.Invoke(recordService.PlayerID, recordService.X, recordService.Y);
                    break;
                case MouseHook.WM_LBUTTONDOWN:
                    //鼠标左键按下
                    MouseDownEvent?.Invoke(recordService.PlayerID, MMCore.c_mouseButtonLeft, recordService.X, recordService.Y);
                    break;
                case MouseHook.WM_LBUTTONUP:
                    //鼠标左键弹起
                    MouseUpEvent?.Invoke(recordService.PlayerID, MMCore.c_mouseButtonLeft, recordService.X, recordService.Y);
                    break;
                case MouseHook.WM_RBUTTONDOWN:
                    //鼠标右键按下
                    MouseDownEvent?.Invoke(recordService.PlayerID, MMCore.c_mouseButtonRight, recordService.X, recordService.Y);
                    break;
                case MouseHook.WM_RBUTTONUP:
                    //鼠标右键弹起
                    MouseUpEvent?.Invoke(recordService.PlayerID, MMCore.c_mouseButtonRight, recordService.X, recordService.Y);
                    break;
                case MouseHook.WM_LBUTTONDBLCLK:
                    //鼠标左键双击
                    MouseDoubleClickEvent?.Invoke(recordService.PlayerID, MMCore.c_mouseButtonLeft, recordService.X, recordService.Y);
                    break;
                case MouseHook.WM_RBUTTONDBLCLK:
                    //鼠标右键双击
                    MouseDoubleClickEvent?.Invoke(recordService.PlayerID, MMCore.c_mouseButtonRight, recordService.X, recordService.Y);
                    break;
            }
        }

        //↓键盘双击事件专用

        /// <summary>
        /// 上一次按下按键的虚拟键码,默认-1表示无按键.
        /// </summary>
        private static int lastKeyDownValue = -1;
        /// <summary>
        /// 上一次按下按键的时间,默认最小值.
        /// </summary>
        private static DateTime lastKeyDownTime = DateTime.MinValue;
        /// <summary>
        /// 双击事件间隔时间(毫秒)
        /// </summary>
        private static int doubleClickIntervalMs = 250;

        private static void KeyboardEventHandler(RecordService recordService)
        {
            //热键判断
            if (recordService.KeyWParam‌ == KeyboardHook.WM_KEYDOWN || recordService.KeyWParam‌ == KeyboardHook.WM_SYSKEYDOWN)
            {
                //按下
                KeyDownEvent?.Invoke(recordService.PlayerID, recordService.VKCode);
                //键盘双击
                if (lastKeyDownValue == recordService.VKCode && (DateTime.Now - lastKeyDownTime).TotalMilliseconds <= doubleClickIntervalMs)
                {
                    lastKeyDownValue = -1;
                    lastKeyDownTime = DateTime.MinValue;
                    KeyDoubleClickEvent?.Invoke(recordService.PlayerID, recordService.VKCode);
                }
                else
                {
                    lastKeyDownValue = recordService.VKCode;
                    lastKeyDownTime = DateTime.Now;
                }
            }
            else if (recordService.KeyWParam‌ == KeyboardHook.WM_KEYUP || recordService.KeyWParam‌ == KeyboardHook.WM_SYSKEYUP)
            {
                //松开
                KeyUpEvent?.Invoke(recordService.PlayerID, recordService.VKCode);
            }
        }

        #endregion

        #region Functions 键鼠事件动作主体(测试专用,不推荐使用)

        //由MainUpdate监听RecordService实例字段来判断发送,由于底层事件的发送频率过高,应由MainUpdate降频后转发.

        /// <summary>
        /// 注册底层键鼠事件.由于底层事件的发送频率过高,不推荐注册耗时动作.
        /// </summary>
        /// <param name="cover">true:覆盖注册,false:追加注册</param>
        public static void AddVisualKeyMouseEvent(RecordService keyMouseRecordService, bool cover, KeyDownEventFuncref keyDown, KeyUpEventFuncref keyUp, MouseMoveEventFuncref mouseMove, MouseDownEventFuncref mouseDown, MouseUpEventFuncref mouseUp)
        {
            if (cover)
            {
                keyMouseRecordService.KeyDownEvent = keyDown;
                keyMouseRecordService.KeyUpEvent = keyUp;
                keyMouseRecordService.MouseMoveEvent = mouseMove;
                keyMouseRecordService.MouseDownEvent = mouseDown;
                keyMouseRecordService.MouseUpEvent = mouseUp;
            }
            {
                keyMouseRecordService.KeyDownEvent += keyDown;
                keyMouseRecordService.KeyUpEvent += keyUp;
                keyMouseRecordService.MouseMoveEvent += mouseMove;
                keyMouseRecordService.MouseDownEvent += mouseDown;
                keyMouseRecordService.MouseUpEvent += mouseUp;
            }
        }

        /// <summary>
        /// 注销底层键鼠事件.
        /// </summary>
        /// <param name="lp_null">true注销全部,否则仅注销预制键鼠事件</param>
        public static void DelVisualKeyMouseEvent(RecordService keyMouseRecordService, bool lp_null = false, KeyDownEventFuncref keyDown = null, KeyUpEventFuncref keyUp = null, MouseMoveEventFuncref mouseMove = null, MouseDownEventFuncref mouseDown = null, MouseUpEventFuncref mouseUp = null)
        {
            if (lp_null)
            {
                keyMouseRecordService.KeyDownEvent = null;
                keyMouseRecordService.KeyUpEvent = null;
                keyMouseRecordService.MouseMoveEvent = null;
                keyMouseRecordService.MouseDownEvent = null;
                keyMouseRecordService.MouseUpEvent = null;
            }
            {
                keyMouseRecordService.KeyDownEvent -= keyDown;
                keyMouseRecordService.KeyUpEvent -= keyUp;
                keyMouseRecordService.MouseMoveEvent -= mouseMove;
                keyMouseRecordService.MouseDownEvent -= mouseDown;
                keyMouseRecordService.MouseUpEvent -= mouseUp;
            }
        }

        /// <summary>
        /// 键盘按下事件主要动作(加入按键监听并传参执行)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        public static bool KeyDown(int player, int key)
        {
            bool torf = !StopKeyMouseEvent[player];
            Player.KeyDownState[player, key] = torf;  //当前按键状态值
            Player.KeyDown[player, key] = true;  //当前按键值

            if (StopKeyMouseEvent[player] == false)
            {
                Player.KeyDownLoopOneBitNum[player] += 1; //玩家当前注册的按键队列数量
                UserDataTable<int>.Save2(true, "KeyDownLoopOneBit", player, Player.KeyDownLoopOneBitNum[player], key);
                //↑存储玩家注册序号对应按键队列键位
                UserDataTable<bool>.Save2(true, "KeyDownLoopOneBitKey", player, key, true); //存储玩家按键队列键位状态
                //---------------------------------------------------------------------蓄力管理
                if (chargeEnable == true)
                {
                    HD_RegKXL(key, ThreadStringBuilder.Concat("IntGroup_XuLi", player));
                    HD_SetKeyFloatXL(player, key, 1.0f);
                }
                //---------------------------------------------------------------------双击管理
                if (doubleClickEnable == true)
                {
                    float lv_a = HD_ReturnKeyFloatSJ(player, key);
                    if ((0.0 < lv_a) && (lv_a <= doubleClickTimeLimit))
                    {
                        //符合双击标准,发送事件
                        Send_KeySJEvent(player, key, doubleClickTimeLimit - lv_a);
                    }
                    else
                    {
                        HD_RegKSJ(key, ThreadStringBuilder.Concat("IntGroup_DoubleClicked", player)); //HD_注册按键
                        HD_SetKeyFloatSJ(player, key, doubleClickTimeLimit);
                    }
                }
                //---------------------------------------------------------------------
                KeyDownGlobalEvent(key, true, player);
            }
            return torf;
        }

        /// <summary>
        /// 键盘弹起事件主要动作(加入按键监听并传参执行)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyUp(int player, int key)
        {
            bool torf = !StopKeyMouseEvent[player];
            Player.KeyDownState[player, key] = false;  //当前按键状态值,本事件始终为false
            Player.KeyDown[player, key] = false;  //当前按键值

            if (StopKeyMouseEvent[player] == false)
            {
                //直接执行动作或通知延迟弹起函数去执行动作
                if (UserDataTable<bool>.Load2(true, "KeyDownLoopOneBitKey", player, key) == false)
                {
                    //弹起时无该键动作队列(由延迟弹起执行完),则直接执行本次事件动作
                    KeyUpFunc(player, key);
                }
                else
                {
                    //弹起时有该键动作队列,通知延迟弹起函数运行(按键队列>0时,清空一次队列并执行它们的动作)
                    UserDataTable<bool>.Save2(true, "KeyDownLoopOneBitEnd", player, key, true);
                }
            }
            return torf;
        }

        /// <summary>
        /// 键盘弹起事件处理函数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool KeyUpFunc(int player, int key)
        {
            bool torf = true;
            if (StopKeyMouseEvent[player] == true)
            {
                torf = false;
            }
            else
            {
                KeyDownGlobalEvent(key, false, player);
            }
            return torf;
        }

        /// <summary>
        /// 鼠标移动事件主要动作(加入按键监听并传参执行)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        public static void MouseMove(int player, int uiX, int uiY)
        {
            Vector3F mouseVector = new Vector3F(uiX, uiY, 0f);
            float unitTerrainHeight = 0f, unitHeight = 0f;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (StopKeyMouseEvent[player] == false)
            {
                Player.MouseVector2F[player] = new Vector2F(mouseVector.x, mouseVector.y);

                //↓注意取出来的是该点最高位Unit
                try { unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight")); }
                catch { }
                try { unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height")); }
                catch { }

                Player.MouseVectorX[player] = mouseVector.x;
                Player.MouseVectorY[player] = mouseVector.y;
                Player.MouseVectorZ[player] = mouseVector.z;
                Player.MouseVectorZFixed[player] = mouseVector.z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVectorFixed[player] = new Vector3F(mouseVector.x, mouseVector.y, Player.MouseVectorZFixed[player]);
                Player.MouseVector[player] = mouseVector;
                //下面2个动作应该要从二维点读取单位(可多个),将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVectorUnitTerrain[player] = new Vector3F(mouseVector.x, mouseVector.y, mouseVector.z - unitTerrainHeight);
                Player.MouseVectorTerrain[player] = new Vector3F(mouseVector.x, mouseVector.y, mouseVector.z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时,计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度,用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, mouseVector);
                }
            }
#else
            if (StopKeyMouseEvent[player] == false)
            {
                Player.MouseVector2F[player] = new Vector2F(mouseVector.X, mouseVector.Y);

                //↓注意取出来的是该点最高位Unit
                try { unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight")); }
                catch { }
                try { unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height")); }
                catch { }

                Player.MouseVectorX[player] = mouseVector.X;
                Player.MouseVectorY[player] = mouseVector.Y;
                Player.MouseVectorZ[player] = mouseVector.Z;
                Player.MouseVectorZFixed[player] = mouseVector.Z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVectorFixed[player] = new Vector3F(mouseVector.X, mouseVector.Y, Player.MouseVectorZFixed[player]);
                Player.MouseVector[player] = mouseVector;
                //下面2个动作应该要从二维点读取单位(可多个),将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVectorUnitTerrain[player] = new Vector3F(mouseVector.X, mouseVector.Y, mouseVector.Z - unitTerrainHeight);
                Player.MouseVectorTerrain[player] = new Vector3F(mouseVector.X, mouseVector.Y, mouseVector.Z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时,计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度,用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, mouseVector);
                }
            }
#endif
        }

        /// <summary>
        /// 鼠标按下事件主要动作(加入按键监听并传参执行)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        /// <returns></returns>
        public static bool MouseDown(int player, int key, int uiX, int uiY)
        {
            Vector3F mouseVector = new Vector3F(uiX, uiY, 0f);
            float unitTerrainHeight = 0f, unitHeight = 0f;

            bool torf = !StopKeyMouseEvent[player];
            Player.MouseDownState[player, key] = torf;  //当前按键状态
            Player.MouseDown[player, key] = true;  //当前按键值

            if (key == c_mouseButtonLeft)
            {
                Player.MouseDownLeft[player] = true;
            }
            if (key == c_mouseButtonRight)
            {
                Player.MouseDownRight[player] = true;
            }
            if (key == c_mouseButtonMiddle)
            {
                Player.MouseDownMiddle[player] = true;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (StopKeyMouseEvent[player] == false)
            {
                Player.MouseVector2F[player] = new Vector2F(uiX, uiY);

                //↓注意取出来的是该点最高位Unit
                try { unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight")); }
                catch { }
                try { unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height")); }
                catch { }

                Player.MouseVectorX[player] = uiX;
                Player.MouseVectorY[player] = uiY;
                Player.MouseVectorZ[player] = mouseVector.z;
                Player.MouseVectorZFixed[player] = mouseVector.z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVectorFixed[player] = new Vector3F(uiX, uiY, Player.MouseVectorZFixed[player]);
                Player.MouseVector[player] = mouseVector;
                //下面2个动作应该要从二维点读取单位(可多个),将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVectorUnitTerrain[player] = new Vector3F(uiX, uiY, mouseVector.z - unitTerrainHeight);
                Player.MouseVectorTerrain[player] = new Vector3F(uiX, uiY, mouseVector.z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时,计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度,用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, mouseVector);
                }

                //---------------------------------------------------------------------
                Player.MouseDownLoopOneBitNum[player] += 1;
                UserDataTable<int>.Save2(true, "MouseDownLoopOneBit", player, Player.MouseDownLoopOneBitNum[player], key);
                UserDataTable<bool>.Save2(true, "MouseDownLoopOneBitKey", player, key, true);
                //---------------------------------------------------------------------
                if (chargeDebug == true)
                {
                    HD_RegKXL(key, ThreadStringBuilder.Concat("IntGroup_XuLi", player)); //HD_注册按键
                    HD_SetKeyFloatXL(player, key, 1.0f);
                }
                //---------------------------------------------------------------------
                if (doubleClickDebug == true)
                {
                    HD_RegPTwo(Player.MouseVector2F[player], ThreadStringBuilder.Concat("DoubleClicked_PTwo_", player));
                    float lv_a = HD_ReturnKeyFloatSJ(player, key);
                    if ((0.0 < lv_a) && (lv_a <= doubleClickTimeLimit) && HD_PTwoRangeTrue(ThreadStringBuilder.Concat("DoubleClicked_PTwo_", player)))
                    {
                        //符合双击标准(鼠标双击多个2点验证),发送事件
                        Send_MouseSJEvent(player, key, doubleClickTimeLimit - lv_a, uiX, uiY, mouseVector);
                    }
                    else
                    {
                        HD_RegKSJ(key, ThreadStringBuilder.Concat("IntGroup_DoubleClicked", player)); //HD_注册按键
                        HD_SetKeyFloatSJ(player, key, doubleClickTimeLimit);
                    }
                }
                //---------------------------------------------------------------------
                MouseDownFunc(player, key, uiX, uiY, mouseVector);
            }
#else
            if (StopKeyMouseEvent[player] == false)
            {
                Player.MouseVector2F[player] = new Vector2F(uiX, uiY);

                //↓注意取出来的是该点最高位Unit
                try { unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight")); }
                catch { }
                try { unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height")); }
                catch { }

                Player.MouseVectorX[player] = uiX;
                Player.MouseVectorY[player] = uiY;
                Player.MouseVectorZ[player] = mouseVector.Z;
                Player.MouseVectorZFixed[player] = mouseVector.Z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVectorFixed[player] = new Vector3F(uiX, uiY, Player.MouseVectorZFixed[player]);
                Player.MouseVector[player] = mouseVector;
                //下面2个动作应该要从二维点读取单位(可多个),将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVectorUnitTerrain[player] = new Vector3F(uiX, uiY, mouseVector.Z - unitTerrainHeight);
                Player.MouseVectorTerrain[player] = new Vector3F(uiX, uiY, mouseVector.Z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时,计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度,用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离(由于点击的位置是单位头顶位置,2个单位重叠则返回最高位的,所以玩家会点到最高位单位)
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, mouseVector);
                }

                //---------------------------------------------------------------------
                Player.MouseDownLoopOneBitNum[player] += 1;
                UserDataTable<int>.Save2(true, "MouseDownLoopOneBit", player, Player.MouseDownLoopOneBitNum[player], key);
                UserDataTable<bool>.Save2(true, "MouseDownLoopOneBitKey", player, key, true);
                //---------------------------------------------------------------------
                if (chargeEnable == true)
                {
                    HD_RegKXL(key, ThreadStringBuilder.Concat("IntGroup_XuLi", player)); //HD_注册按键
                    HD_SetKeyFloatXL(player, key, 1.0f);
                }
                //---------------------------------------------------------------------
                if (doubleClickEnable == true)
                {
                    HD_RegPTwo(Player.MouseVector2F[player], ThreadStringBuilder.Concat("DoubleClicked_PTwo_", player));
                    float lv_a = HD_ReturnKeyFloatSJ(player, key);
                    if ((0.0f < lv_a) && (lv_a <= doubleClickTimeLimit) && HD_PTwoRangeTrue(ThreadStringBuilder.Concat("DoubleClicked_PTwo_", player)))
                    {
                        //符合双击标准(鼠标双击多个2点验证),发送事件
                        Send_MouseSJEvent(player, key, doubleClickTimeLimit - lv_a, uiX, uiY, Player.MouseVector[player]);
                    }
                    else
                    {
                        HD_RegKSJ(key, ThreadStringBuilder.Concat("IntGroup_DoubleClicked", player)); //HD_注册按键
                        HD_SetKeyFloatSJ(player, key, doubleClickTimeLimit);
                    }
                }
                //---------------------------------------------------------------------
                MouseDownFunc(player, key, uiX, uiY, mouseVector);
            }
#endif
            return torf;
        }

        /// <summary>
        /// 鼠标按下事件处理函数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="mouseVector">鼠标在3D世界中的坐标</param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        /// <returns></returns>
        internal static bool MouseDownFunc(int player, int key, int uiX = 0, int uiY = 0, Vector3F? mouseVector = null)
        {
            //Variable Declarations
            bool torf = true;

            //Implementation
            if (StopKeyMouseEvent[player] == true)
            {
                //阻止按键事件时强制取消按键状态
                Player.MouseDownState[player, key] = false;
                if (key == c_mouseButtonLeft)
                {
                    Player.MouseDownLeft[player] = false;
                }
                if (key == c_mouseButtonRight)
                {
                    Player.MouseDownRight[player] = false;
                }
                if (key == c_mouseButtonMiddle)
                {
                    Player.MouseDownMiddle[player] = false;
                }
                torf = false;
            }
            else
            {
                MouseDownGlobalEvent(key, true, player);
            }
            return torf;
        }

        /// <summary>
        /// 鼠标弹起事件主要动作(加入按键监听并传参执行)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        public static bool MouseUp(int player, int key, int uiX, int uiY)
        {
            bool torf = !StopKeyMouseEvent[player];
            Player.MouseDownState[player, key] = false;  //当前按键状态值,本事件始终为false
            Player.MouseDown[player, key] = false;  //当前按键值
            if (key == c_mouseButtonLeft)
            {
                Player.MouseDownLeft[player] = false;
            }
            if (key == c_mouseButtonRight)
            {
                Player.MouseDownRight[player] = false;
            }
            if (key == c_mouseButtonMiddle)
            {
                Player.MouseDownMiddle[player] = false;
            }

            if (StopKeyMouseEvent[player] == false)
            {
                //直接执行动作或通知延迟弹起函数去执行动作
                if (UserDataTable<bool>.Load2(true, "MouseDownLoopOneBitKey", player, key) == false)
                {
                    //弹起时无该键动作队列(由延迟弹起执行完),则直接执行本次事件动作
                    MouseUpFunc(player, key);
                }
                else
                {
                    //弹起时有该键动作队列,通知延迟弹起函数运行(按键队列>0时,清空一次队列并执行它们的动作)
                    UserDataTable<bool>.Save2(true, "MouseDownLoopOneBitEnd", player, key, true);
                }
            }
            return torf;
        }

        /// <summary>
        /// 鼠标弹起事件处理函数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        internal static bool MouseUpFunc(int player, int key)
        {
            bool torf = true;
            if (StopKeyMouseEvent[player] == true)
            {
                torf = false;
            }
            else
            {
                MouseDownGlobalEvent(key, false, player);
            }
            return torf;
        }

        /// <summary>
        /// 处理按键队列的延迟弹起.会按序执行键鼠事件动作队列,需加入到每帧执行(并遍历全部玩家).
        /// </summary>
        /// <param name="player"></param>
        public static void KeyMouseUpWait(int player)
        {
            int key;
            int ae, be, a, ai = 1, bi = 1;
            //玩家有鼠标按键事件动作队列时
            if (Player.MouseDownLoopOneBitNum[player] > 0)
            {
                ae = Player.MouseDownLoopOneBitNum[player];//获取动作队列数量
                a = 1;
                for (; ((ai >= 0 && a <= ae) || (ai < 0 && a >= ae)); a += ai)
                {
                    key = UserDataTable<int>.Load2(true, "MouseDownLoopOneBit", player, a);//读取玩家指定动作队列按键
                    if (UserDataTable<bool>.Load2(true, "MouseDownLoopOneBitEnd", player, key) == true)//判断玩家指定按键的动作队列是否结束
                    {
                        //若该键的动作队列结束,重置按键状态
                        if (key == c_mouseButtonLeft)
                        {
                            Player.MouseDown[player, c_mouseButtonLeft] = false;
                        }
                        if (key == c_mouseButtonRight)
                        {
                            Player.MouseDown[player, c_mouseButtonRight] = false;
                        }
                        if (key == c_mouseButtonMiddle)
                        {
                            Player.MouseDown[player, c_mouseButtonMiddle] = false;
                        }
                        //
                        MouseDownFunc(player, key, Player.MouseUIX[player], Player.MouseUIY[player], Player.MouseVector[player]);
                    }
                    UserDataTable<int>.Clear2(true, "MouseDownLoopOneBit", player, a);
                    UserDataTable<bool>.Clear2(true, "MouseDownLoopOneBitKey", player, key);
                    UserDataTable<bool>.Clear2(true, "MouseDownLoopOneBitEnd", player, key);
                }
                Player.MouseDownLoopOneBitNum[player] = 0; //动作全部执行,全队列清空
            }
            //玩家有键盘按键事件动作队列时
            if (Player.KeyDownLoopOneBitNum[player] > 0)//获取动作队列数量
            {
                be = Player.KeyDownLoopOneBitNum[player];
                a = 1;
                for (; ((bi >= 0 && a <= be) || (bi < 0 && a >= be)); a += bi)
                {
                    key = UserDataTable<int>.Load2(true, "KeyDownLoopOneBit", player, a);//读取玩家指定动作队列按键
                    if (UserDataTable<bool>.Load2(true, "KeyDownLoopOneBitEnd", player, key) == true)//判断玩家指定按键的动作队列是否结束
                    {
                        //若该键的动作队列结束,重置按键状态
                        Player.KeyDown[player, key] = false;
                        KeyUpFunc(player, key);
                    }
                    UserDataTable<int>.Clear2(true, "KeyDownLoopOneBit", player, a);
                    UserDataTable<bool>.Clear2(true, "KeyDownLoopOneBitKey", player, key);
                    UserDataTable<bool>.Clear2(true, "KeyDownLoopOneBitEnd", player, key);
                }
                Player.KeyDownLoopOneBitNum[player] = 0; //全键盘队列清空
            }
        }

        #region Functions 键鼠事件委托管理(与键鼠事件动作主体将执行的委托小组进行互动).

        //------------------------------------↓KeyDownEventStart↓-----------------------------------------

        /// <summary>
        /// 将(1个或多个)委托函数注册到键盘按键事件(或者说给委托函数添加指定事件,完成事件注册).
        /// 注册指定键盘按键的委托函数,每个键盘按键最大注册数量限制(8),超过则什么也不做
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        public static void RegistKeyEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MMCore_KeyEventFuncref_");//注册注销时进行等待
            ThreadWaitSet("MMCore_KeyEventFuncref_", true);
            if (keyEventFuncrefGroupNum[key] >= c_regKeyMax)
            {
                return;
            }
            keyEventFuncrefGroupNum[key] += 1;//注册成功记录+1
            keyEventFuncrefGroup[key, keyEventFuncrefGroupNum[key]] = funcref;//这里采用等于,设计为覆盖
            ThreadWaitSet("MMCore_KeyEventFuncref_", false);
        }
        /// <summary>
        /// 注册指定键盘按键的委托函数(登录在指定注册序号num位置)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="num">不能超过最大注册数量限制(8)</param>
        /// <param name="funcref"></param>
        public static void RegistKeyEventFuncref(int key, int num, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MMCore_KeyEventFuncref_");//注册注销时进行等待
            ThreadWaitSet("MMCore_KeyEventFuncref_", true);
            keyEventFuncrefGroup[key, num] = funcref;
            ThreadWaitSet("MMCore_KeyEventFuncref_", false);
        }

        /// <summary>
        /// 注销指定键盘按键的委托函数(发生序号重排)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        public static void RemoveKeyEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MMCore_KeyEventFuncref_");
            ThreadWaitSet("MMCore_KeyEventFuncref_", true);
            try
            {
                int currentCount = keyEventFuncrefGroupNum[key];
                int a = 1;
                while (a <= currentCount)
                {
                    if (keyEventFuncrefGroup[key, a] == funcref)
                    {
                        // 1. 将后续元素前移，覆盖当前被删除的元素
                        for (int b = a; b < currentCount; b++)
                        {
                            keyEventFuncrefGroup[key, b] = keyEventFuncrefGroup[key, b + 1];
                        }
                        // 2. 清空最后一个冗余位置（防止内存泄漏或幽灵引用）
                        keyEventFuncrefGroup[key, currentCount] = null;

                        // 3. 总数减一
                        currentCount--;
                        keyEventFuncrefGroupNum[key] = currentCount;
                    }
                    else
                    {
                        // 只有当不匹配时，才检查下一个索引
                        a++;
                    }
                }
            }
            finally
            {
                ThreadWaitSet("MMCore_KeyEventFuncref_", false);
            }
        }

        /// <summary>
        /// 返回指定键盘按键注册函数的序号
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        /// <returns>错误时返回-1</returns>
        public static int GetKeyEventFuncrefNearestNum(int key, KeyMouseEventFuncref funcref)
        {
            int num = -1;
            for (int a = 1; a <= keyEventFuncrefGroupNum[key]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (keyEventFuncrefGroup[key, a] == funcref)
                {
                    //返回最近的函数序号
                    num = a;
                    break;
                }
            }
            return num;
        }

        /// <summary>
        /// 返回指定键盘按键指定函数的注册数量(>1则注册了多个同样的函数)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        /// <returns></returns>
        public static int GetKeyEventFuncrefCount(int key, KeyMouseEventFuncref funcref)
        {
            int count = 0;
            for (int a = 1; a <= keyEventFuncrefGroupNum[key]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (keyEventFuncrefGroup[key, a] == funcref)
                {
                    count += 1;
                }
            }

            return count;
        }

        /// <summary>
        /// 归并键盘按键指定函数(如存在则移除该函数注册并序号重排,之后重新注册1次)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        /// <returns></returns>
        public static bool RedoKeyEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MMCore_KeyEventFuncref_");//注册注销时进行等待
            ThreadWaitSet("MMCore_KeyEventFuncref_", true);
            bool result = false;
            int num = GetKeyEventFuncrefCount(key, funcref);
            if (num > 1)
            {
                result = true;
                //发现重复函数,移除后重新注册
                RemoveKeyEventFuncref(key, funcref);
                RegistKeyEventFuncref(key, funcref);
            }
            ThreadWaitSet("MMCore_KeyEventFuncref_", false);
            return result;
        }

        /// <summary>
        /// 全局键盘按键事件,对指定键盘按键执行委托函数动作集合
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keydown"></param>
        /// <param name="player"></param>
        public static void KeyDownGlobalEvent(int key, bool keydown, int player)
        {
            for (int a = 1; a <= keyEventFuncrefGroupNum[key]; a += 1)
            {
                //这里不开新线程,是否另开线程运行宜由委托函数去写
                keyEventFuncrefGroup[key, a](keydown, player);//执行键盘按键委托
            }
        }

        //--------------------------------------↑KeyDownEventEnd↑-----------------------------------------

        //------------------------------------↓MouseDownEventStart↓---------------------------------------

        /// <summary>
        /// 将(1个或多个)委托函数注册到鼠标按键事件(或者说给委托函数添加指定事件,完成事件注册).
        /// 注册指定鼠标键位的委托函数,每个鼠标按键最大注册数量限制(24),超过则什么也不做
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        public static void RegistMouseEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MouseEventFuncref");//注册注销时进行等待
            ThreadWaitSet("MouseEventFuncref", true);
            if (mouseEventFuncrefGroupNum[key] >= c_regMouseMax)
            {
                return;
            }
            mouseEventFuncrefGroupNum[key] += 1;//注册成功记录+1
            mouseEventFuncrefGroup[key, mouseEventFuncrefGroupNum[key]] = funcref;//这里采用等于,设计为覆盖
            ThreadWaitSet("MouseEventFuncref", false);
        }
        /// <summary>
        /// 注册指定鼠标键位的委托函数(登录在指定注册序号num位置)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="num">不能超过最大注册数量限制(24)</param>
        /// <param name="funcref"></param>
        public static void RegistMouseEventFuncref(int key, int num, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MouseEventFuncref");//注册注销时进行等待
            ThreadWaitSet("MouseEventFuncref", true);
            mouseEventFuncrefGroup[key, num] = funcref;
            ThreadWaitSet("MouseEventFuncref", false);
        }

        /// <summary>
        /// 注销指定鼠标键位的委托函数(发生序号重排)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        public static void RemoveMouseEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MouseEventFuncref");
            ThreadWaitSet("MouseEventFuncref", true);
            try
            {
                int currentCount = mouseEventFuncrefGroupNum[key];
                int a = 1;
                while (a <= currentCount)
                {
                    if (mouseEventFuncrefGroup[key, a] == funcref)
                    {
                        // 1. 将后续元素前移，覆盖当前被删除的元素
                        for (int b = a; b < currentCount; b++)
                        {
                            mouseEventFuncrefGroup[key, b] = mouseEventFuncrefGroup[key, b + 1];
                        }

                        // 2. 清空最后一个冗余位置（防止内存泄漏或幽灵引用）
                        mouseEventFuncrefGroup[key, currentCount] = null;

                        // 3. 总数减一
                        currentCount--;
                        mouseEventFuncrefGroupNum[key] = currentCount;
                    }
                    else
                    {
                        // 只有当不匹配时，才检查下一个索引
                        a++;
                    }
                }
            }
            finally
            {
                ThreadWaitSet("MouseEventFuncref", false);
            }
        }

        /// <summary>
        /// 返回指定鼠标键位注册函数的序号
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        /// <returns>错误时返回-1</returns>
        public static int GetMouseEventFuncrefNearestNum(int key, KeyMouseEventFuncref funcref)
        {
            int num = -1;
            for (int a = 1; a <= mouseEventFuncrefGroupNum[key]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (mouseEventFuncrefGroup[key, a] == funcref)
                {
                    //返回最近的函数序号
                    num = a;
                    break;
                }
            }
            return num;
        }

        /// <summary>
        /// 返回指定鼠标键位指定注册函数的数量(>1则注册了多个同样的函数)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        /// <returns></returns>
        public static int GetMouseEventFuncrefCount(int key, KeyMouseEventFuncref funcref)
        {
            int count = 0;
            for (int a = 1; a <= mouseEventFuncrefGroupNum[key]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (mouseEventFuncrefGroup[key, a] == funcref)
                {
                    count += 1;
                }
            }
            return count;
        }

        /// <summary>
        /// 归并鼠标按键指定函数(如存在则移除该函数注册并序号重排,之后重新注册1次)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        /// <returns></returns>
        public static bool RedoMouseEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            bool torf = false;
            int num = GetMouseEventFuncrefCount(key, funcref);
            if (num > 1)
            {
                torf = true;
                //发现重复函数,移除后重新注册
                RemoveMouseEventFuncref(key, funcref);
                RegistMouseEventFuncref(key, funcref);
            }
            return torf;
        }

        /// <summary>
        /// 全局鼠标按键事件,对指定鼠标按键执行委托函数动作集合
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keydown"></param>
        /// <param name="player"></param>
        public static void MouseDownGlobalEvent(int key, bool keydown, int player)
        {
            int a = 1;
            for (; a <= mouseEventFuncrefGroupNum[key]; a += 1)
            {
                //这里不开新线程,是否另开线程运行宜由委托函数去写
                mouseEventFuncrefGroup[key, a](keydown, player);//执行鼠标按键委托
            }
        }

        //------------------------------------↑MouseDownEventEnd↑-----------------------------------------

        #endregion

        #endregion

        #region 蓄力、双击相关方法

        public static void ChargeManager()
        {
            int lv_key;
            int lv_p;
            float KXL;
            PlayerGroup PG;

            string lv_keyStorage;      //蓄力键存储区名称
            int lv_numMax;             //最大注册数量
            int lv_index;              //当前索引
            const int lv_step = 1;    //循环步长
            PG = CurrentUserGroup();
            lv_p = -1;
            while (true)
            {
                lv_p = PlayerGroupNextPlayer(PG, lv_p);
                if (lv_p < 0) { break; }
                lv_keyStorage = ThreadStringBuilder.Concat("IntGroup_Charge", lv_p);
                lv_numMax = HD_ReturnKXLNumMax(lv_keyStorage);
                lv_index = 1;
                for (; lv_index <= lv_numMax; lv_index += lv_step)
                {
                    lv_key = HD_ReturnKXLTagFromRegNum(lv_index, lv_keyStorage);
                    if (lv_key > 98)
                    {
                        //是鼠标按键
                        if (Player.MouseDownState[lv_p, (lv_key - 98)] == true)
                        {
                            //蓄力
                            KXL = HD_ReturnKeyFloatXL(lv_p, lv_key) + chargeDeltaValue;
                            HD_SetKeyFloatXL(lv_p, lv_key, KXL);
                        }
                        else
                        {
                            //未蓄力
                            KXL = HD_ReturnKeyFloatXL(lv_p, lv_key) - chargeDeltaValue;
                            HD_SetKeyFloatXL(lv_p, lv_key, KXL);
                        }
                    }
                    else
                    {
                        //是键盘按键
                        if (Player.KeyDownState[lv_p, lv_key] == true)
                        {
                            //蓄力
                            KXL = HD_ReturnKeyFloatXL(lv_p, lv_key) + chargeDeltaValue;
                            HD_SetKeyFloatXL(lv_p, lv_key, KXL);
                        }
                        else
                        {
                            //未蓄力
                            KXL = HD_ReturnKeyFloatXL(lv_p, lv_key) - chargeDeltaValue;
                            HD_SetKeyFloatXL(lv_p, lv_key, KXL);
                        }
                    }
                    //蓄力值低于原始值,则清空
                    if (HD_ReturnKeyFloatXL(lv_p, lv_key) < 1.0)
                    {
                        HD_SetKeyFloatXL(lv_p, lv_key, 0);
                    }
                    //蓄力值调试输出
                    if ((HD_ReturnKeyFloatXL(lv_p, lv_key) != 0) && (chargeDebug == true))
                    {
                        Tell(ThreadStringBuilder.Concat("P", lv_p, "蓄力值[", lv_key, "]", " => "), HD_ReturnKeyFloatXL(lv_p, lv_key));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void DoubleClickManager()
        {
            int lv_key;
            int lv_p;
            float KSJ;
            float lv_delta;           //双击衰减增量
            PlayerGroup PG;

            string lv_keyStorage;      //双击键存储区名称
            int lv_numMax;             //最大注册数量
            int lv_index;              //当前索引
            const int lv_step = 1;    //循环步长
            PG = CurrentUserGroup();
            lv_p = -1;
            while (true)
            {
                lv_p = PlayerGroupNextPlayer(PG, lv_p);
                if (lv_p < 0) { break; }
                lv_keyStorage = ThreadStringBuilder.Concat("IntGroup_DoubleClicked", lv_p); //取得玩家区域
                lv_numMax = HD_ReturnKSJNumMax(lv_keyStorage); //该玩家区域需要处理的注册数量
                lv_index = 1;
                lv_delta = doubleClickDeltaValue;
                for (; lv_index <= lv_numMax; lv_index += lv_step)
                {
                    lv_key = HD_ReturnKSJTagFromRegNum(lv_index, lv_keyStorage); //取得每个序号对应的双击注册键
                    if ((HD_ReturnKeyFloatSJ(lv_p, lv_key) != -1.0))
                    {  //跳过不需要处理的键（双击值为-1）
                       //无论按键是什么,总是进行双击值的衰减,双击管理无需像蓄力管理那样判断鼠标键盘弹起状态
                        if (HD_ReturnKeyFloatSJ(lv_p, lv_key) >= 0.0)
                        {
                            KSJ = HD_ReturnKeyFloatSJ(lv_p, lv_key) - lv_delta;
                            HD_SetKeyFloatSJ(lv_p, lv_key, KSJ);
                        }
                        //Debug
                        if (HD_ReturnKeyFloatSJ(lv_p, lv_key) < 0.0)
                        {
                            HD_SetKeyFloatSJ(lv_p, lv_key, -1.0f);
                        }
                        //调试双击值
                        if ((HD_ReturnKeyFloatSJ(lv_p, lv_key) != -1.0) && (doubleClickDebug == true))
                        {
                            Tell(ThreadStringBuilder.Concat("P", lv_p, "双击值[", lv_key, "]", " => "), HD_ReturnKeyFloatSJ(lv_p, lv_key));
                        }
                    }
                }
            }
        }

        public static void Send_KeySJEvent(int lp_player, int lp_key, float lp_deltaTime)
        {
            //时间差仅作为调试,并不需要传入事件
            KeyDoubleClickEvent?.Invoke(lp_player, lp_key);
        }

        public static void Send_MouseSJEvent(int lp_player, int lp_key, float lp_deltaTime, int lp_x = 0, int lp_y = 0, Vector3F? lp_mouseVector = null)
        {
            //时间差仅作为调试,并不需要传入事件
            MouseDoubleClickEvent?.Invoke(lp_player, lp_key, lp_x, lp_y);
        }

        public static void HD_RegKSJ(int lp_key, string lp_keyStorage)
        {
            string lv_str;
            int lv_num;
            int lv_i;
            int lv_key;              //要注册的按键值
            int lv_numMax;           //最大注册数量
            int lv_index;            //当前索引
            lv_str = (lp_keyStorage + "KSJ");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_key = lp_key;
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_key);
                UserDataTable<bool>.Save1(true, ("HD_IfKTag" + lv_str), lv_key, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    lv_numMax = lv_num;
                    lv_index = 1;
                    for (; lv_index <= lv_numMax; lv_index += 1)
                    {
                        lv_i = lv_index;
                        if ((UserDataTable<int>.Load1(true, (lv_str + "Tag"), lv_i) == lv_key))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_key);
                                UserDataTable<bool>.Save1(true, ("HD_IfKTag" + lv_str), lv_key, true);
                            }

                        }
                    }
                }

            }
        }

        public static int HD_ReturnKSJNumMax(string lp_keyStorage)
        {
            return UserDataTable<int>.Load0(true, (lp_keyStorage + "KSJNum"));
        }

        public static int HD_ReturnKSJTagFromRegNum(int lp_regNum, string lp_keyStorage)
        {
            return UserDataTable<int>.Load1(true, (lp_keyStorage + "KSJTag"), lp_regNum);
        }

        public static void HD_RegKXL(int lp_key, string lp_keyStorage)
        {
            string lv_str;
            int lv_num;
            int lv_i;
            int lv_key;              //要注册的按键值
            int lv_numMax;           //最大注册数量
            int lv_index;            //当前索引
            lv_str = (lp_keyStorage + "KXL");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_key = lp_key;
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_key);
                UserDataTable<bool>.Save1(true, ("HD_IfKTag" + lv_str), lv_key, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    lv_numMax = lv_num;
                    lv_index = 1;
                    for (; lv_index <= lv_numMax; lv_index += 1)
                    {
                        lv_i = lv_index;
                        if ((UserDataTable<int>.Load1(true, (lv_str + "Tag"), lv_i) == lv_key))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_key);
                                UserDataTable<bool>.Save1(true, ("HD_IfKTag" + lv_str), lv_key, true);
                            }

                        }
                    }
                }
            }
        }

        public static int HD_ReturnKXLNumMax(string lp_key)
        {
            return UserDataTable<int>.Load0(true, (lp_key + "KXLNum"));
        }

        /// <summary>
        /// 返回序号对应按键（蓄力专用）
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_keyStorage"></param>
        /// <returns></returns>
        public static int HD_ReturnKXLTagFromRegNum(int lp_regNum, string lp_keyStorage)
        {
            return UserDataTable<int>.Load1(true, (lp_keyStorage + "KXLTag"), lp_regNum);
        }

        public static void HD_SetKeyFloatXL(int lp_player, int lp_key, float lp_value)
        {
            UserDataTable<float>.Save1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key, lp_value);
        }

        public static void SetChargeDeltaValue(float lp_value)
        {
            chargeDeltaValue = lp_value;
        }

        public static float HD_ReturnKeyFloatXL(int lp_player, int lp_key)
        {
            return UserDataTable<float>.Load1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key);
        }

        public static float HD_ReturnKeyFloatXL_Mouse(int lp_player, int lp_mouse)
        {
            return UserDataTable<float>.Load1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_mouse + 98);
        }

        public static float HD_ReturnKeyFloatXL_Keyboard(int lp_player, int lp_key)
        {
            return UserDataTable<float>.Load1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key);
        }

        public static void HD_SetKeyFloatSJ(int lp_player, int lp_key, float lp_value)
        {
            UserDataTable<float>.Save1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key, lp_value);
        }

        public static void SetDoubleClickDeltaValue(float lp_value)
        {
            doubleClickDeltaValue = lp_value;
        }

        public static float HD_ReturnKeyFloatSJ(int lp_player, int lp_key)
        {
            return UserDataTable<float>.Load1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key);
        }

        public static float HD_ReturnKeyFloatSJ_Mouse(int lp_player, int lp_mouse)
        {
            return UserDataTable<float>.Load1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_mouse + 98);
        }

        public static float HD_ReturnKeyFloatSJ_Keyboard(int lp_player, int lp_key)
        {
            return UserDataTable<float>.Load1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key);
        }

        public static void SetDoubleClickTimeLimit(float lp_time)
        {
            doubleClickTimeLimit = lp_time;
        }

        #endregion

        #region 虚拟键位转换

        /// <summary>
        /// 将Windows虚拟键码转换为MMCore键位常量(如c_keyShift)
        /// </summary>
        /// <param name="virtualKey">Windows虚拟键码</param>
        /// <returns>MMCore键位常量,键盘按键返回0~98,鼠标按键返回99~103,无法识别则返回-1.</returns>
        public static int ConvertVirtualKeyToMMCoreKey(int virtualKey)
        {
            switch (virtualKey)
            {
                case 0x01:
                    return MMCore.c_mouseButtonLeft + 98;
                case 0x02:
                    return MMCore.c_mouseButtonRight + 98;
                case 0x04:
                    return MMCore.c_mouseButtonMiddle + 98;
                case 0x05:
                    return MMCore.c_mouseButtonXButton1 + 98;
                case 0x06:
                    return MMCore.c_mouseButtonXButton2 + 98;
                case 0x10:
                    return MMCore.c_keyShift;
                case 0x11:
                    return MMCore.c_keyControl;
                case 0x12:
                    return MMCore.c_keyAlt;
                case 0x30:
                    return MMCore.c_key0;
                case 0x31:
                    return MMCore.c_key1;
                case 0x32:
                    return MMCore.c_key2;
                case 0x33:
                    return MMCore.c_key3;
                case 0x34:
                    return MMCore.c_key4;
                case 0x35:
                    return MMCore.c_key5;
                case 0x36:
                    return MMCore.c_key6;
                case 0x37:
                    return MMCore.c_key7;
                case 0x38:
                    return MMCore.c_key8;
                case 0x39:
                    return MMCore.c_key9;
                case 0x41:
                    return MMCore.c_keyA;
                case 0x42:
                    return MMCore.c_keyB;
                case 0x43:
                    return MMCore.c_keyC;
                case 0x44:
                    return MMCore.c_keyD;
                case 0x45:
                    return MMCore.c_keyE;
                case 0x46:
                    return MMCore.c_keyF;
                case 0x47:
                    return MMCore.c_keyG;
                case 0x48:
                    return MMCore.c_keyH;
                case 0x49:
                    return MMCore.c_keyI;
                case 0x4A:
                    return MMCore.c_keyJ;
                case 0x4B:
                    return MMCore.c_keyK;
                case 0x4C:
                    return MMCore.c_keyL;
                case 0x4D:
                    return MMCore.c_keyM;
                case 0x4E:
                    return MMCore.c_keyN;
                case 0x4F:
                    return MMCore.c_keyO;
                case 0x50:
                    return MMCore.c_keyP;
                case 0x51:
                    return MMCore.c_keyQ;
                case 0x52:
                    return MMCore.c_keyR;
                case 0x53:
                    return MMCore.c_keyS;
                case 0x54:
                    return MMCore.c_keyT;
                case 0x55:
                    return MMCore.c_keyU;
                case 0x56:
                    return MMCore.c_keyV;
                case 0x57:
                    return MMCore.c_keyW;
                case 0x58:
                    return MMCore.c_keyX;
                case 0x59:
                    return MMCore.c_keyY;
                case 0x5A:
                    return MMCore.c_keyZ;
                case 0x20:
                    return MMCore.c_keySpace;
                case 0xC0:
                    return MMCore.c_keyGrave;
                case 0x60:
                    return MMCore.c_keyNumPad0;
                case 0x61:
                    return MMCore.c_keyNumPad1;
                case 0x62:
                    return MMCore.c_keyNumPad2;
                case 0x63:
                    return MMCore.c_keyNumPad3;
                case 0x64:
                    return MMCore.c_keyNumPad4;
                case 0x65:
                    return MMCore.c_keyNumPad5;
                case 0x66:
                    return MMCore.c_keyNumPad6;
                case 0x67:
                    return MMCore.c_keyNumPad7;
                case 0x68:
                    return MMCore.c_keyNumPad8;
                case 0x69:
                    return MMCore.c_keyNumPad9;
                case 0x6B:
                    return MMCore.c_keyNumPadPlus;
                case 0x6D:
                    return MMCore.c_keyNumPadMinus;
                case 0x6A:
                    return MMCore.c_keyNumPadMultiply;
                case 0x6F:
                    return MMCore.c_keyNumPadDivide;
                case 0x6E:
                    return MMCore.c_keyNumPadDecimal;
                case 0xBB:
                    return MMCore.c_keyEquals;
                case 0xBD:
                    return MMCore.c_keyMinus;
                case 0xDB:
                    return MMCore.c_keyBracketOpen;
                case 0xDD:
                    return MMCore.c_keyBracketClose;
                case 0xDC:
                    return MMCore.c_keyBackSlash;
                case 0xBA:
                    return MMCore.c_keySemiColon;
                case 0xDE:
                    return MMCore.c_keyApostrophe;
                case 0xBC:
                    return MMCore.c_keyComma;
                case 0xBE:
                    return MMCore.c_keyPeriod;
                case 0xBF:
                    return MMCore.c_keySlash;
                case 0x1B:
                    return MMCore.c_keyEscape;
                case 0x0D:
                    return MMCore.c_keyEnter;
                case 0x08:
                    return MMCore.c_keyBackSpace;
                case 0x09:
                    return MMCore.c_keyTab;
                case 0x25:
                    return MMCore.c_keyLeft;
                case 0x26:
                    return MMCore.c_keyUp;
                case 0x27:
                    return MMCore.c_keyRight;
                case 0x28:
                    return MMCore.c_keyDown;
                case 0x2D:
                    return MMCore.c_keyInsert;
                case 0x2E:
                    return MMCore.c_keyDelete;
                case 0x24:
                    return MMCore.c_keyHome;
                case 0x23:
                    return MMCore.c_keyEnd;
                case 0x21:
                    return MMCore.c_keyPageUp;
                case 0x22:
                    return MMCore.c_keyPageDown;
                case 0x14:
                    return MMCore.c_keyCapsLock;
                case 0x90:
                    return MMCore.c_keyNumLock;
                case 0x91:
                    return MMCore.c_keyScrollLock;
                case 0x13:
                    return MMCore.c_keyPause;
                case 0x2C:
                    return MMCore.c_keyPrintScreen;
                case 0xB0:
                    return MMCore.c_keyNextTrack;
                case 0xB1:
                    return MMCore.c_keyPrevTrack;
                case 0x70:
                    return MMCore.c_keyF1;
                case 0x71:
                    return MMCore.c_keyF2;
                case 0x72:
                    return MMCore.c_keyF3;
                case 0x73:
                    return MMCore.c_keyF4;
                case 0x74:
                    return MMCore.c_keyF5;
                case 0x75:
                    return MMCore.c_keyF6;
                case 0x76:
                    return MMCore.c_keyF7;
                case 0x77:
                    return MMCore.c_keyF8;
                case 0x78:
                    return MMCore.c_keyF9;
                case 0x79:
                    return MMCore.c_keyF10;
                case 0x7A:
                    return MMCore.c_keyF11;
                case 0x7B:
                    return MMCore.c_keyF12;
                default:
                    return MMCore.c_keyNone;
            }
        }

        /// <summary>
        /// 将MMCore键位常量(如c_keyShift)转换回Windows虚拟键码
        /// </summary>
        /// <param name="mmcoreKey">MMCore键位常量(填入范围:键盘0~98,鼠标99~103)</param>
        /// <returns>Windows虚拟键码(1~254),如果无法识别则返回-1</returns>
        public static int ConvertMMCoreKeyToVirtualKey(int mmcoreKey)
        {
            switch (mmcoreKey)
            {
                case MMCore.c_keyShift:
                    return 0x10;
                case MMCore.c_keyControl:
                    return 0x11;
                case MMCore.c_keyAlt:
                    return 0x12;
                case MMCore.c_key0:
                    return 0x30;
                case MMCore.c_key1:
                    return 0x31;
                case MMCore.c_key2:
                    return 0x32;
                case MMCore.c_key3:
                    return 0x33;
                case MMCore.c_key4:
                    return 0x34;
                case MMCore.c_key5:
                    return 0x35;
                case MMCore.c_key6:
                    return 0x36;
                case MMCore.c_key7:
                    return 0x37;
                case MMCore.c_key8:
                    return 0x38;
                case MMCore.c_key9:
                    return 0x39;
                case MMCore.c_keyA:
                    return 0x41;
                case MMCore.c_keyB:
                    return 0x42;
                case MMCore.c_keyC:
                    return 0x43;
                case MMCore.c_keyD:
                    return 0x44;
                case MMCore.c_keyE:
                    return 0x45;
                case MMCore.c_keyF:
                    return 0x46;
                case MMCore.c_keyG:
                    return 0x47;
                case MMCore.c_keyH:
                    return 0x48;
                case MMCore.c_keyI:
                    return 0x49;
                case MMCore.c_keyJ:
                    return 0x4A;
                case MMCore.c_keyK:
                    return 0x4B;
                case MMCore.c_keyL:
                    return 0x4C;
                case MMCore.c_keyM:
                    return 0x4D;
                case MMCore.c_keyN:
                    return 0x4E;
                case MMCore.c_keyO:
                    return 0x4F;
                case MMCore.c_keyP:
                    return 0x50;
                case MMCore.c_keyQ:
                    return 0x51;
                case MMCore.c_keyR:
                    return 0x52;
                case MMCore.c_keyS:
                    return 0x53;
                case MMCore.c_keyT:
                    return 0x54;
                case MMCore.c_keyU:
                    return 0x55;
                case MMCore.c_keyV:
                    return 0x56;
                case MMCore.c_keyW:
                    return 0x57;
                case MMCore.c_keyX:
                    return 0x58;
                case MMCore.c_keyY:
                    return 0x59;
                case MMCore.c_keyZ:
                    return 0x5A;
                case MMCore.c_keySpace:
                    return 0x20;
                case MMCore.c_keyGrave:
                    return 0xC0;
                case MMCore.c_keyNumPad0:
                    return 0x60;
                case MMCore.c_keyNumPad1:
                    return 0x61;
                case MMCore.c_keyNumPad2:
                    return 0x62;
                case MMCore.c_keyNumPad3:
                    return 0x63;
                case MMCore.c_keyNumPad4:
                    return 0x64;
                case MMCore.c_keyNumPad5:
                    return 0x65;
                case MMCore.c_keyNumPad6:
                    return 0x66;
                case MMCore.c_keyNumPad7:
                    return 0x67;
                case MMCore.c_keyNumPad8:
                    return 0x68;
                case MMCore.c_keyNumPad9:
                    return 0x69;
                case MMCore.c_keyNumPadPlus:
                    return 0x6B;
                case MMCore.c_keyNumPadMinus:
                    return 0x6D;
                case MMCore.c_keyNumPadMultiply:
                    return 0x6A;
                case MMCore.c_keyNumPadDivide:
                    return 0x6F;
                case MMCore.c_keyNumPadDecimal:
                    return 0x6E;
                case MMCore.c_keyEquals:
                    return 0xBB;
                case MMCore.c_keyMinus:
                    return 0xBD;
                case MMCore.c_keyBracketOpen:
                    return 0xDB;
                case MMCore.c_keyBracketClose:
                    return 0xDD;
                case MMCore.c_keyBackSlash:
                    return 0xDC;
                case MMCore.c_keySemiColon:
                    return 0xBA;
                case MMCore.c_keyApostrophe:
                    return 0xDE;
                case MMCore.c_keyComma:
                    return 0xBC;
                case MMCore.c_keyPeriod:
                    return 0xBE;
                case MMCore.c_keySlash:
                    return 0xBF;
                case MMCore.c_keyEscape:
                    return 0x1B;
                case MMCore.c_keyEnter:
                    return 0x0D;
                case MMCore.c_keyBackSpace:
                    return 0x08;
                case MMCore.c_keyTab:
                    return 0x09;
                case MMCore.c_keyLeft:
                    return 0x25;
                case MMCore.c_keyUp:
                    return 0x26;
                case MMCore.c_keyRight:
                    return 0x27;
                case MMCore.c_keyDown:
                    return 0x28;
                case MMCore.c_keyInsert:
                    return 0x2D;
                case MMCore.c_keyDelete:
                    return 0x2E;
                case MMCore.c_keyHome:
                    return 0x24;
                case MMCore.c_keyEnd:
                    return 0x23;
                case MMCore.c_keyPageUp:
                    return 0x21;
                case MMCore.c_keyPageDown:
                    return 0x22;
                case MMCore.c_keyCapsLock:
                    return 0x14;
                case MMCore.c_keyNumLock:
                    return 0x90;
                case MMCore.c_keyScrollLock:
                    return 0x91;
                case MMCore.c_keyPause:
                    return 0x13;
                case MMCore.c_keyPrintScreen:
                    return 0x2C;
                case MMCore.c_keyNextTrack:
                    return 0xB0;
                case MMCore.c_keyPrevTrack:
                    return 0xB1;
                case MMCore.c_keyF1:
                    return 0x70;
                case MMCore.c_keyF2:
                    return 0x71;
                case MMCore.c_keyF3:
                    return 0x72;
                case MMCore.c_keyF4:
                    return 0x73;
                case MMCore.c_keyF5:
                    return 0x74;
                case MMCore.c_keyF6:
                    return 0x75;
                case MMCore.c_keyF7:
                    return 0x76;
                case MMCore.c_keyF8:
                    return 0x77;
                case MMCore.c_keyF9:
                    return 0x78;
                case MMCore.c_keyF10:
                    return 0x79;
                case MMCore.c_keyF11:
                    return 0x7A;
                case MMCore.c_keyF12:
                    return 0x7B;
                case MMCore.c_mouseButtonLeft + 98:
                    return 0x01;
                case MMCore.c_mouseButtonRight + 98:
                    return 0x02;
                case MMCore.c_mouseButtonMiddle + 98:
                    return 0x04;
                case MMCore.c_mouseButtonXButton1 + 98:
                    return 0x05;
                case MMCore.c_mouseButtonXButton2 + 98:
                    return 0x06;
                default:
                    return -1;
            }
        }

        #endregion

        #endregion

        #region 二点组

        //反复存储2个点,用来比对距离差的小组

        public static void HD_RegPTwo(Vector2F lp_vector2F, string lp_keyStorage)
        {
            string lv_str;
            int lv_num;
            int Auto_val;
            int lv_i;
            int lv_d;
            lv_str = (lp_keyStorage + "PTwo");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            Auto_val = lv_num;
            if (Auto_val == 0)
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<Vector2F>.Save1(true, (lv_str + "Tag"), lv_i, lp_vector2F);
            }
            else if (Auto_val == 1)
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<Vector2F>.Save1(true, (lv_str + "Tag"), lv_i, lp_vector2F);
            }
            else if (Auto_val == 2)
            {
                lv_d = UserDataTable<int>.Load0(true, "PTwo_Num" + lv_str);
                if (lv_d == 0)
                {
                    UserDataTable<Vector2F>.Save1(true, (lv_str + "Tag"), 1, lp_vector2F);
                    UserDataTable<int>.Save0(true, "PTwo_Num" + lv_str, 1);
                }
                else
                {
                    UserDataTable<Vector2F>.Save1(true, (lv_str + "Tag"), 2, lp_vector2F);
                    UserDataTable<int>.Save0(true, "PTwo_Num" + lv_str, 0);
                }
            }
        }

        public static void HD_ClearPTwo(string lp_keyStorage)
        {
            string lv_str;
            lv_str = (lp_keyStorage + "PTwo");
            UserDataTable<Vector2F>.Clear1(true, (lv_str + "Tag"), 1);
            UserDataTable<Vector2F>.Clear1(true, (lv_str + "Tag"), 2);
            UserDataTable<int>.Clear0(true, "PTwo_Num" + lv_str);
        }

        public static int HD_ReturnPTwoNumMax(string lp_keyStorage)
        {
            string lv_str;
            int lv_num;
            lv_str = (lp_keyStorage + "PTwo");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            return lv_num;
        }

        public static Vector2F HD_ReturnPTwoTagFromRegNum(int lp_regNum, string lp_keyStorage)
        {
            string lv_str;
            Vector2F lv_e782B9;
            lv_str = (lp_keyStorage + "PTwo");
            lv_e782B9 = UserDataTable<Vector2F>.Load1(true, (lv_str + "Tag"), lp_regNum);
            return lv_e782B9;
        }

        public static float HD_PTwoRange(string lp_keyStorage)
        {
            Vector2F lv_a;
            Vector2F lv_b;
            float lv_s;
            lv_a = HD_ReturnPTwoTagFromRegNum(1, lp_keyStorage);
            lv_b = HD_ReturnPTwoTagFromRegNum(2, lp_keyStorage);
            lv_s = Distance(lv_a, lv_b);
            return lv_s;
        }

        public static bool HD_PTwoRangeTrue(string lp_keyStorage)
        {
            Vector2F lv_a;
            Vector2F lv_b;
            float lv_s;
            bool lv_torf = false;
            lv_a = HD_ReturnPTwoTagFromRegNum(1, lp_keyStorage);
            lv_b = HD_ReturnPTwoTagFromRegNum(2, lp_keyStorage);
            lv_s = Distance(lv_a, lv_b);
            if ((lv_s <= doubleClickRange))
            {
                lv_torf = true;
                if ((doubleClickDebug == true))
                {
                    Tell(("鼠标双击距离差：" + lv_s + " <= " + doubleClickRange));
                }
            }
            else
            {
                if ((doubleClickDebug == true))
                {
                    Tell(("鼠标双击距离差：" + lv_s + " > " + doubleClickRange));
                }
            }
            return lv_torf;
        }

        #endregion

        #region 玩家组

        // playergroup 类型定义 - 存储玩家组信息
        public class PlayerGroup
        {
            public List<int> players = new List<int>();
        }

        // CurrentUserGroup - 返回当前本地用户组（支持多人联机）
        public static PlayerGroup CurrentUserGroup()
        {
            PlayerGroup pg = new PlayerGroup();

            // 遍历所有可能的玩家,收集本地用户
            for (int i = 1; i <= Game.c_maxPlayers; i++)
            {
                // 如果是本地用户,添加到组中
                if (Player.LocalUser[i])
                {
                    pg.players.Add(i);
                }
            }

            // 如果没有找到本地用户（可能是初始化阶段或单机模式）,默认添加玩家1
            if (pg.players.Count == 0)
            {
                pg.players.Add(1);
            }

            return pg;
        }

        // PlayerGroupNextPlayer - 遍历玩家组
        public static int PlayerGroupNextPlayer(PlayerGroup lp_pg, int lp_current)
        {
            if (lp_pg == null || lp_pg.players.Count == 0)
                return -1;

            if (lp_current < 0)
            {
                // 首次调用,返回第一个玩家
                return lp_pg.players[0];
            }

            int index = lp_pg.players.IndexOf(lp_current);
            if (index < 0 || index >= lp_pg.players.Count - 1)
            {
                return -1; // 遍历结束
            }

            return lp_pg.players[index + 1];
        }

        #endregion

    }
}
