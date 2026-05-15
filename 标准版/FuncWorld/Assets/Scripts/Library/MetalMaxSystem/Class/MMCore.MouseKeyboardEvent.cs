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
#endif
#endif
#endregion

namespace MetalMaxSystem
{
    public partial class MMCore
    {
        public static bool chargeDebug = true;
        public static bool doubleClickDebug = true;

        public static float chargeDeltaValue = 1.0f; //蓄力增量
        public static float doubleClickDeltaValue = 0.0625f; //双击增量
        public static float doubleClickTimeLimit = 0.25f; //双击时间限制(单位秒)

        public delegate void KeyDoubleClickFuncref(int player, int key, float timeDiff);
        public delegate void MouseDoubleClickFuncref(int player, int mouseButton, float timeDiff);
        public static event KeyDoubleClickFuncref KeyDoubleClickEvent;
        public static event MouseDoubleClickFuncref MouseDoubleClickEvent;

        #region 方法

        public static void ChargeManager()
        {
            int lv_key;
            int lv_p;
            float KXL;
            PlayerGroup PG;

            string auto88703A65_vs;
            int auto88703A65_ae;
            int auto88703A65_va;
            const int auto88703A65_ai = 1;
            PG = CurrentUserGroup();
            lv_p = -1;
            while (true)
            {
                lv_p = PlayerGroupNextPlayer(PG, lv_p);
                if (lv_p < 0) { break; }
                auto88703A65_vs = ThreadStringBuilder.Concat("IntGroup_Charge", lv_p);
                auto88703A65_ae = HD_ReturnKXLNumMax(auto88703A65_vs);
                auto88703A65_va = 1;
                for (; ((auto88703A65_ai >= 0 && auto88703A65_va <= auto88703A65_ae) || (auto88703A65_ai < 0 && auto88703A65_va >= auto88703A65_ae)); auto88703A65_va += auto88703A65_ai)
                {
                    lv_key = HD_ReturnKXLTagFromRegNum(auto88703A65_va, auto88703A65_vs);
                    if (lv_key > 98)
                    {
                        //是鼠标按键
                        if (Player.MouseDownState[lv_p, (lv_key - 98)] == true)
                        {
                            //蓄力
                            KXL = HD_ReturnKeyfloatXL(lv_p, lv_key) + chargeDeltaValue;
                            HD_SetKeyfloatXL(lv_p, lv_key, KXL);
                        }
                        else
                        {
                            //未蓄力
                            KXL = HD_ReturnKeyfloatXL(lv_p, lv_key) - chargeDeltaValue;
                            HD_SetKeyfloatXL(lv_p, lv_key, KXL);
                        }
                    }
                    else
                    {
                        //是键盘按键
                        if (Player.KeyDownState[lv_p, lv_key] == true)
                        {
                            //蓄力
                            KXL = HD_ReturnKeyfloatXL(lv_p, lv_key) + chargeDeltaValue;
                            HD_SetKeyfloatXL(lv_p, lv_key, KXL);
                        }
                        else
                        {
                            //未蓄力
                            KXL = HD_ReturnKeyfloatXL(lv_p, lv_key) - chargeDeltaValue;
                            HD_SetKeyfloatXL(lv_p, lv_key, KXL);
                        }
                    }
                    //蓄力值低于原始值,则清空
                    if (HD_ReturnKeyfloatXL(lv_p, lv_key) < 1.0)
                    {
                        HD_SetKeyfloatXL(lv_p, lv_key, 0);
                    }
                    //蓄力值调试输出
                    if ((HD_ReturnKeyfloatXL(lv_p, lv_key) != 0) && (chargeDebug == true))
                    {
                        Tell(ThreadStringBuilder.Concat("P", lv_p, "蓄力值[", lv_key, "]", " => "), HD_ReturnKeyfloatXL(lv_p, lv_key));
                    }
                }
            }
        }

        public static void DoubleClickManager()
        {
            int lv_key;
            int lv_p;
            float KSJ;
            float i;
            PlayerGroup PG;

            string Auto_vs;
            int Auto_ae;
            int Auto_va;
            const int Auto_ai = 1;
            PG = CurrentUserGroup();
            lv_p = -1;
            while (true)
            {
                lv_p = PlayerGroupNextPlayer(PG, lv_p);
                if (lv_p < 0) { break; }
                Auto_vs = ThreadStringBuilder.Concat("IntGroup_DoubleClicked", lv_p); //取得玩家区域
                Auto_ae = HD_ReturnKSJNumMax(Auto_vs); //该玩家区域需要处理的注册数量
                Auto_va = 1;
                i = doubleClickDeltaValue;
                for (; ((Auto_ai >= 0 && Auto_va <= Auto_ae) || (Auto_ai < 0 && Auto_va >= Auto_ae)); Auto_va += Auto_ai)
                {
                    lv_key = HD_ReturnKSJTagFromRegNum(Auto_va, Auto_vs); //取得每个序号对应的双击注册键
                    if ((HD_ReturnKeyFloat_DoubleClick(lv_p, lv_key) != -1.0))
                    {  //跳过不需要处理的键（双击值为-1）
                       //无论按键是什么,总是进行双击值的衰减,双击管理无需像蓄力管理那样判断鼠标键盘弹起状态
                        if (HD_ReturnKeyFloat_DoubleClick(lv_p, lv_key) >= 0.0)
                        {
                            KSJ = HD_ReturnKeyFloat_DoubleClick(lv_p, lv_key) - i;
                            HD_SetKeyFloat_DoubleClick(lv_p, lv_key, KSJ);
                        }
                        //Debug
                        if (HD_ReturnKeyFloat_DoubleClick(lv_p, lv_key) < 0.0)
                        {
                            HD_SetKeyFloat_DoubleClick(lv_p, lv_key, -1.0f);
                        }
                        //调试双击值
                        if ((HD_ReturnKeyFloat_DoubleClick(lv_p, lv_key) != -1.0) && (doubleClickDebug == true))
                        {
                            Tell(ThreadStringBuilder.Concat("P", lv_p, "双击值[", lv_key, "]", " => "), HD_ReturnKeyFloat_DoubleClick(lv_p, lv_key));
                        }
                    }
                }
            }
        }

        public static void Send_KeyDoubleClicked(int lp_player, int lp_key, float lp_deltaTime)
        {
            KeyDoubleClickEvent?.Invoke(lp_player, lp_key, lp_deltaTime);
        }

        public static void Send_MouseDoubleClicked(int lp_player, int lp_key, float lp_deltaTime, Vector2F lp__3DE782B9, int lp_x, int lp_y)
        {
            MouseDoubleClickEvent?.Invoke(lp_player, lp_key, lp_deltaTime);
        }

        public static void HD_RegKSJ(int lp_key, string lp_keyStorage)
        {
            string lv_str;
            int lv_num;
            int lv_i;
            int lv_e6A087E7ADBE;
            int autoE2130D1D_ae;
            int autoE2130D1D_var;
            lv_str = (lp_keyStorage + "KSJ");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_e6A087E7ADBE = lp_key;
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_e6A087E7ADBE);
                DataTableBoolSave1(true, ("HD_IfKTag" + lv_str), lv_e6A087E7ADBE, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    autoE2130D1D_ae = lv_num;
                    autoE2130D1D_var = 1;
                    for (; autoE2130D1D_var <= autoE2130D1D_ae; autoE2130D1D_var += 1)
                    {
                        lv_i = autoE2130D1D_var;
                        if ((DataTableIntLoad1(true, (lv_str + "Tag"), lv_i) == lv_e6A087E7ADBE))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_e6A087E7ADBE);
                                DataTableBoolSave1(true, ("HD_IfKTag" + lv_str), lv_e6A087E7ADBE, true);
                            }

                        }
                    }
                }

            }
        }

        public static int HD_ReturnKSJNumMax(string lp_keyStorage)
        {
            return DataTableIntLoad0(true, (lp_keyStorage + "KSJNum"));
        }

        public static int HD_ReturnKSJTagFromRegNum(int lp_regNum, string lp_keyStorage)
        {
            return DataTableIntLoad1(true, (lp_keyStorage + "KSJTag"), lp_regNum);
        }

        public static void HD_RegKXL(int lp_key, string lp_keyStorage)
        {
            string lv_str;
            int lv_num;
            int lv_i;
            int lv_e6A087E7ADBE;
            int autoAD1568D7_ae;
            int autoAD1568D7_var;
            lv_str = (lp_keyStorage + "KXL");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_e6A087E7ADBE = lp_key;
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_e6A087E7ADBE);
                DataTableBoolSave1(true, ("HD_IfKTag" + lv_str), lv_e6A087E7ADBE, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    autoAD1568D7_ae = lv_num;
                    autoAD1568D7_var = 1;
                    for (; autoAD1568D7_var <= autoAD1568D7_ae; autoAD1568D7_var += 1)
                    {
                        lv_i = autoAD1568D7_var;
                        if ((DataTableIntLoad1(true, (lv_str + "Tag"), lv_i) == lv_e6A087E7ADBE))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_e6A087E7ADBE);
                                DataTableBoolSave1(true, ("HD_IfKTag" + lv_str), lv_e6A087E7ADBE, true);
                            }

                        }
                    }
                }
            }
        }

        public static int HD_ReturnKXLNumMax(string lp_key)
        {
            return DataTableIntLoad0(true, (lp_key + "KXLNum"));
        }

        /// <summary>
        /// 返回序号对应按键（蓄力专用）
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_keyStorage"></param>
        /// <returns></returns>
        public static int HD_ReturnKXLTagFromRegNum(int lp_regNum, string lp_keyStorage)
        {
            return DataTableIntLoad1(true, (lp_keyStorage + "KXLTag"), lp_regNum);
        }

        public static void HD_SetKeyfloatXL(int lp_player, int lp_key, float lp_value)
        {
            DataTableFloatSave1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key, lp_value);
        }

        public static void SetChargeDeltaValue(float lp_e89384E58A9BE5A29EE9878F)
        {
            chargeDeltaValue = lp_e89384E58A9BE5A29EE9878F;
        }

        public static float HD_ReturnKeyfloatXL(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key);
        }

        public static float HD_ReturnKeyfloatXL_Mouse(int lp_player, int lp_mouse)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_mouse + 98);
        }

        public static float HD_ReturnKeyfloatXL_Keyboard(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key);
        }

        public static void HD_SetKeyFloat_DoubleClick(int lp_player, int lp_key, float lp_value)
        {
            DataTableFloatSave1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key, lp_value);
        }

        public static void SetDoubleClickDeltaValue(float lp_value)
        {
            doubleClickDeltaValue = lp_value;
        }

        public static float HD_ReturnKeyFloat_DoubleClick(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key);
        }

        public static float HD_ReturnKeyFloat_DoubleClick_Mouse(int lp_player, int lp_mouse)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_mouse + 98);
        }

        public static float HD_ReturnKeyFloat_DoubleClick_Keyboard(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key);
        }

        public static void SetDoubleClickTimeLimit(float lp_time)
        {
            doubleClickTimeLimit = lp_time;
        }

        #endregion

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

    }
}

/*
## 一、完整架构图
┌─────────────────────────────────────────────────────────────────────────────┐
│                         整体协作架构                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────────────────────┐                                              │
│   │    操作系统底层硬件      │                                              │
│   │      输入事件           │                                              │
│   └─────────────────────────┘                                              │
│              ↓                                                              │
│   ┌───────────────────────────────────────────────────┐                    │
│   │  KeyMouseHook (Windows 系统钩子)                  │                    │
│   │  - MouseHook (监听 WM_MOUSEMOVE/WM_LBUTTONDOWN 等)│                    │
│   │  - KeyboardHook (监听 WM_KEYDOWN/WM_KEYUP 等)     │                    │
│   └───────────────────────────────────────────────────┘                    │
│              ↓                                                              │
│   ┌───────────────────────────────────────────────────┐                    │
│   │     RecordService (输入服务层)                    │                    │
│   │  ├─ 接收钩子事件 (MouseEventHandler/KeyboardEventHandler) │                    │
│   │  ├─ 转换为标准化格式 (PlayerID, key, Vector3F...) │                    │
│   │  ├─ 触发事件: KeyDownEvent / MouseDownEvent...   │                    │
│   │  └─ 通过 AddKeyMouseEvent 绑定到 MMCore          │                    │
│   └───────────────────────────────────────────────────┘                    │
│              ↓                                                              │
│   ┌─────────────────────────────────────────────────────────────────────┐ │
│   │          MMCore (按键事件总控)                                       │ │
│   │  ┌───────────────────────────────────────────────────────────────┐   │ │
│   │  │ 状态存储 (Player 类) - 按键状态/鼠标位置                         │   │ │
│   │  ├───────────────────────────────────────────────────────────────┤   │ │
│   │  │ KeyDown(player, key) -> Player.KeyDownState[] ->                │   │ │
│   │  │ KeyDownGlobalEvent(key, true, player) -> 执行注册的委托         │   │ │
│   │  ├───────────────────────────────────────────────────────────────┤   │ │
│   │  │ 委托管理 - RegistKeyEventFuncref() / RegistMouseEventFuncref() │   │ │
│   │  └───────────────────────────────────────────────────────────────┘   │ │
│   └─────────────────────────────────────────────────────────────────────┘ │
│              ↓                                                              │
│   ┌─────────────────────────────────────────────────────────────────────┐ │
│   │        MainUpdate / SubUpdate (周期更新触发器)                        │ │
│   │  ┌───────────────────────────────────────────────────────────────┐ │ │
│   │  │ MainUpdate 线程 (默认间隔 50ms)                                │ │ │
│   │  │  - Awake()  -> EntryGlobalEvent(MainAwake)                    │ │ │
│   │  │  - Start()  -> EntryGlobalEvent(MainStart)                    │ │ │
│   │  │  - Update() -> EntryGlobalEvent(MainUpdate) <- 每帧调用      │ │ │
│   │  │  - End()    -> EntryGlobalEvent(MainEnd)                      │ │ │
│   │  │  - Destroy() -> EntryGlobalEvent(MainDestroy)                 │ │ │
│   │  ├───────────────────────────────────────────────────────────────┤ │ │
│   │  │ SubUpdate 线程 (另一个周期触发器, 可并行)                      │ │ │
│   │  └───────────────────────────────────────────────────────────────┘ │ │
│   └─────────────────────────────────────────────────────────────────────┘ │
│              ↓                                                              │
│   ┌─────────────────────────────────────────────────────────────────────┐ │
│   │       游戏系统每帧更新 (ChargeManager, DoubleClickManager)          │ │
│   │  - ChargeManager: 遍历 CurrentUserGroup,更新蓄力值                │ │
│   │  - DoubleClickManager: 遍历 CurrentUserGroup,衰减双击计时器      │ │
│   │  - MouseKeyUpWait: 处理按键队列的延迟弹起                          │ │
│   └─────────────────────────────────────────────────────────────────────┘ │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

## 二、各组件详解

### 1. KeyMouseHook - 系统底层钩子

// RecordService.cs 中的初始化
MyMouseHook = MouseHook.GetMouseHook();
MyKeyboardHook = KeyboardHook.GetKeyboardHook();

// 钩子工作原理: 拦截系统级 WM_* 消息
// WM_MOUSEMOVE, WM_LBUTTONDOWN, WM_KEYDOWN 等

### 2. RecordService - 输入服务层

**功能**：
- 接收钩子事件
- 转换为标准格式 (包含 PlayerID)
- 通过 AddKeyMouseEvent 绑定到 MMCore

// RecordService.cs:223
private void MouseEventHandler(int wParam, MouseHook.MouseHookStruct mouseMsg)
{
    // 1. 记录鼠标位置 X/Y/Z
    X = mouseMsg.pt.x;
    Y = mouseMsg.pt.y;
    
    // 2. 触发绑定的事件
    MouseDownEvent?.Invoke(PlayerID, key, Player.MouseVector3F[PlayerID], X, Y);
}

// MMCore.cs:18246 - 绑定 MMCore 函数到 RecordService
public static void AddKeyMouseEvent(RecordService keyMouseRecordService, bool cover)
{
    keyMouseRecordService.KeyDownEvent = KeyDown;      // 绑定!
    keyMouseRecordService.KeyUpEvent = KeyUp;
    // ...
}

### 3. MMCore - 按键事件总控

**工作流程**：

// MMCore.cs:18301 - KeyDown
internal static bool KeyDown(int player, int key)
{
    // 1. 检查 StopKeyMouseEvent
    bool torf = !StopKeyMouseEvent[player];
    
    // 2. 更新 Player 状态
    Player.KeyDownState[player, key] = torf;
    Player.KeyDown[player, key] = true;
    
    // 3. 记录到按键队列 (支持延迟弹起)
    Player.KeyDownLoopOneBitNum[player] += 1;
    DataTableIntSave2(true, "KeyDownLoopOneBit", player, ...);
    
    // 4. 触发全局事件 -> 执行注册的委托
    KeyDownGlobalEvent(key, true, player);
}

// MMCore.cs:18931 - 执行委托
public static void KeyDownGlobalEvent(int key, bool keydown, int player)
{
    for (int a = 1; a <= keyEventFuncrefGroupNum[key]; a++)
    {
        keyEventFuncrefGroup[key, a](keydown, player);
    }
}

### 4. MainUpdate/SubUpdate - 周期更新

**MainUpdate.cs:96**：

private static void Func()
{
    if (Duetime < 0) { Duetime = 0; }
    if (Period <= 0) { Period = 50; }  // 默认 50ms (20FPS)
    Action(Duetime, Period);
}

// Action() 执行流程:
// Awake() -> EntryGlobalEvent(MainAwake)
// Start() -> EntryGlobalEvent(MainStart)
// Timer -> CheckStatus() -> Update() -> EntryGlobalEvent(MainUpdate) (每50ms)
// End() -> EntryGlobalEvent(MainEnd)
// Destroy() -> EntryGlobalEvent(MainDestroy)
```

### 5. 游戏系统更新 (ChargeManager / DoubleClickManager)

**MMCore.MouseKeyboardEvent.cs:50 - ChargeManager**：

public static void ChargeManager()
{
    // 1. 获取当前本地用户组 (支持多人联机)
    PlayerGroup PG = CurrentUserGroup();
    int lv_p = -1;
    
    // 2. 遍历组内每个玩家
    while (true)
    {
        lv_p = PlayerGroupNextPlayer(PG, lv_p);
        if (lv_p < 0) break;
        
        // 3. 读取该玩家注册的蓄力键
        string auto88703A65_vs = ThreadStringBuilder.Concat("IntGroup_Charge", lv_p);
        int auto88703A65_ae = HD_ReturnKXLNumMax(auto88703A65_vs);
        
        // 4. 更新每个蓄力键的值
        for (auto88703A65_va = 1; ...)
        {
            if (Player.MouseDownState[lv_p, key] == true)
                HD_SetKeyfloatXL(lv_p, key, value + chargeDeltaValue);  // + 1
            else
                HD_SetKeyfloatXL(lv_p, key, value - chargeDeltaValue);  // - 1
        }
    }
}

## 三、多人联机协作关键点

| 机制 | 说明 |
|------|------|
| **PlayerID** | RecordService 实例绑定 PlayerID (1-15) |
| **LocalUser[]** | Player.LocalUser[player] 标记本地用户 |
| **CurrentUserGroup()** | 返回所有 LocalUser=true 的玩家 |
| **遍历策略** | ChargeManager 等使用 `PlayerGroupNextPlayer(PG, lv_p)` 遍历 |

## 四、完整使用示例

// 1. 创建 RecordService 实例
RecordService recordService = new RecordService(1);  // 绑定玩家1

// 2. 绑定到 MMCore
MMCore.AddKeyMouseEvent(recordService, cover: true);

// 3. 注册按键委托
MMCore.RegistKeyEventFuncref(MMCore.c_keyW, OnKeyW_Pressed);
MMCore.RegistKeyEventFuncref(MMCore.c_keyMouseLeft, OnMouseLeft_Pressed);

// 4. 注册蓄力/双击
MMCore.HD_RegKXL(MMCore.c_keyMouseLeft, "IntGroup_Charge1");
MMCore.HD_RegKSJ(MMCore.c_keyMouseLeft, "IntGroup_DoubleClick1");

// 5. 注册到 MainUpdate 每帧更新
MMCore.RegistEntryEventFuncref(Entry.MainUpdate, MMCore.ChargeManager);
MMCore.RegistEntryEventFuncref(Entry.MainUpdate, MMCore.DoubleClickManager);
MMCore.RegistEntryEventFuncref(Entry.MainUpdate, () => MMCore.MouseKeyUpWait(1));

// 6. 启动 MainUpdate
MainUpdate.Start(isBackground: true);

// 7. 启动钩子
recordService.StartMouseHook();
recordService.StartKeyboardHook();

这就是整套系统的完整协作流程！通过钩子 -> 服务 -> 总控 -> 周期更新 -> 游戏逻辑,实现了灵活且支持多人的按键事件管理.
 */