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

        #region 蓄力、双击相关方法

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
                    if ((HD_ReturnKeyFloatSJ(lv_p, lv_key) != -1.0))
                    {  //跳过不需要处理的键（双击值为-1）
                       //无论按键是什么,总是进行双击值的衰减,双击管理无需像蓄力管理那样判断鼠标键盘弹起状态
                        if (HD_ReturnKeyFloatSJ(lv_p, lv_key) >= 0.0)
                        {
                            KSJ = HD_ReturnKeyFloatSJ(lv_p, lv_key) - i;
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

        public static void HD_SetKeyFloatXL(int lp_player, int lp_key, float lp_value)
        {
            DataTableFloatSave1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key, lp_value);
        }

        public static void SetChargeDeltaValue(float lp_value)
        {
            chargeDeltaValue = lp_value;
        }

        public static float HD_ReturnKeyFloatXL(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key);
        }

        public static float HD_ReturnKeyFloatXL_Mouse(int lp_player, int lp_mouse)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_mouse + 98);
        }

        public static float HD_ReturnKeyFloatXL_Keyboard(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KXL", lp_player), lp_key);
        }

        public static void HD_SetKeyFloatSJ(int lp_player, int lp_key, float lp_value)
        {
            DataTableFloatSave1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key, lp_value);
        }

        public static void SetDoubleClickDeltaValue(float lp_value)
        {
            doubleClickDeltaValue = lp_value;
        }

        public static float HD_ReturnKeyFloatSJ(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key);
        }

        public static float HD_ReturnKeyFloatSJ_Mouse(int lp_player, int lp_mouse)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_mouse + 98);
        }

        public static float HD_ReturnKeyFloatSJ_Keyboard(int lp_player, int lp_key)
        {
            return DataTableFloatLoad1(true, ThreadStringBuilder.Concat("HD_CDFloat_KSJ", lp_player), lp_key);
        }

        public static void SetDoubleClickTimeLimit(float lp_time)
        {
            doubleClickTimeLimit = lp_time;
        }

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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            Auto_val = lv_num;
            if (Auto_val == 0)
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableVectorSave1(true, (lv_str + "Tag"), lv_i, lp_vector2F);
            }
            else if (Auto_val == 1)
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableVectorSave1(true, (lv_str + "Tag"), lv_i, lp_vector2F);
            }
            else if (Auto_val == 2)
            {
                lv_d = DataTableIntLoad0(true, "PTwo_Num" + lv_str);
                if (lv_d == 0)
                {
                    DataTableVectorSave1(true, (lv_str + "Tag"), 1, lp_vector2F);
                    DataTableIntSave0(true, "PTwo_Num" + lv_str, 1);
                }
                else
                {
                    DataTableVectorSave1(true, (lv_str + "Tag"), 2, lp_vector2F);
                    DataTableIntSave0(true, "PTwo_Num" + lv_str, 0);
                }
            }
        }

        public static void HD_ClearPTwo(string lp_keyStorage)
        {
            string lv_str;
            lv_str = (lp_keyStorage + "PTwo");
            DictionaryVectorClear1(true, (lv_str + "Tag"), 1);
            DictionaryVectorClear1(true, (lv_str + "Tag"), 2);
            DictionaryIntClear0(true, "PTwo_Num" + lv_str);
        }

        public static int HD_ReturnPTwoNumMax(string lp_keyStorage)
        {
            string lv_str;
            int lv_num;
            lv_str = (lp_keyStorage + "PTwo");
            lv_num = DictionaryIntLoad0(true, (lv_str + "Num"));
            return lv_num;
        }

        public static Vector2F? HD_ReturnPTwoTagFromRegNum(int lp_regNum, string lp_keyStorage)
        {
            string lv_str;
            Vector2F? lv_e782B9;
            lv_str = (lp_keyStorage + "PTwo");
            lv_e782B9 = DictionaryVectorLoad1_N(true, (lv_str + "Tag"), lp_regNum);
            return lv_e782B9;
        }

        public static float HD_PTwoRange(string lp_keyStorage)
        {
            Vector2F? lv_a;
            Vector2F? lv_b;
            float lv_s;
            lv_a = HD_ReturnPTwoTagFromRegNum(1, lp_keyStorage);
            lv_b = HD_ReturnPTwoTagFromRegNum(2, lp_keyStorage);
            if (((lv_a == null) || (lv_b == null)))
            {
                lv_s = -1.0f;
            }
            else
            {
                lv_s = Distance(lv_a.Value, lv_b.Value);
            }
            return lv_s;
        }

        public static bool HD_PTwoRangeTrue(string lp_keyStorage)
        {
            Vector2F? lv_a;
            Vector2F? lv_b;
            float lv_s;
            bool lv_torf = false;
            lv_a = HD_ReturnPTwoTagFromRegNum(1, lp_keyStorage);
            lv_b = HD_ReturnPTwoTagFromRegNum(2, lp_keyStorage);
            if (!((lv_a == null) || (lv_b == null)))
            {
                lv_s = Distance(lv_a.Value, lv_b.Value);
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
    }
}

#region MMCore键鼠事件使用介绍

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
│   │  ├─ 记录状态: X/Y/Z, GetMouseButtonState(), GetKeyState() │                    │
│   │  └─ 不直接触发事件,只记录当前状态供读取              │                    │
│   └───────────────────────────────────────────────────┘                    │
│              ↓                                                              │
│   ┌─────────────────────────────────────────────────────────────────────┐ │
│   │          MMCore (按键事件总控)                                       │ │
│   │  ┌───────────────────────────────────────────────────────────────┐   │ │
│   │  │ StartKeyMouseEventLoop() -> 初始化 RecordService, 启动钩子,   │   │ │
│   │  │   注册 MainUpdate.Update += UpdateKeyMouseState, 启动50ms循环 │   │ │
│   │  ├───────────────────────────────────────────────────────────────┤   │ │
│   │  │ UpdateKeyMouseState() (每50ms调用) -> 读取 RecordService 状态 │   │ │
│   │  │   比较上一帧状态,调用 KeyDown/KeyUp/MouseMove/MouseDown/MouseUp │   │ │
│   │  ├───────────────────────────────────────────────────────────────┤   │ │
│   │  │ KeyDown(player, key) -> Player.KeyDownState[] ->                │   │ │
│   │  │   KeyDownGlobalEvent(key, true, player) -> 执行注册的委托       │   │ │
│   │  ├───────────────────────────────────────────────────────────────┤   │ │
│   │  │ 委托管理 - KeyDownEvent / KeyUpEvent / MouseMoveEvent 等       │   │ │
│   │  └───────────────────────────────────────────────────────────────┘   │ │
│   └─────────────────────────────────────────────────────────────────────┘ │
│              ↓                                                              │
│   ┌─────────────────────────────────────────────────────────────────────┐ │
│   │        MainUpdate / SubUpdate (周期更新触发器)                        │ │
│   │  ┌───────────────────────────────────────────────────────────────┐ │ │
│   │  │ MainUpdate 线程 (默认间隔 50ms)                                │ │ │
│   │  │  - Awake()  -> EntryGlobalEvent(MainAwake)                    │ │ │
│   │  │  - Start()  -> EntryGlobalEvent(MainStart)                    │ │ │
│   │  │  - Update() -> UpdateKeyMouseState + 用户注册的方法        │ │ │
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
- 记录当前状态: X/Y/Z, GetMouseButtonState(), GetKeyState()
- 不直接触发事件，只提供状态读取

// RecordService.cs:223
private void MouseEventHandler(int wParam, MouseHook.MouseHookStruct mouseMsg)
{
    // 1. 记录鼠标位置 X/Y/Z
    X = mouseMsg.pt.x;
    Y = mouseMsg.pt.y;
    
    // 2. 记录状态(不触发事件)
    SetMouseButtonState(button, isDown);
}

### 3. MMCore - 按键事件总控

**工作流程**：
1. StartKeyMouseEventLoop() 初始化 RecordService，启动钩子，注册 MainUpdate.Update
2. MainUpdate.Update 每50ms调用 UpdateKeyMouseState()
3. UpdateKeyMouseState() 读取 RecordService 状态，比较差异后调用 KeyDown/KeyUp/MouseMove/MouseDown/MouseUp
4. 这些方法更新 Player 状态、按键队列，并调用 KeyDownGlobalEvent/MouseDownGlobalEvent 执行注册的委托

// MMCore.cs - 注册按键委托
MMCore.RegistKeyEventFuncref(int key, KeyMouseEventFuncref funcref);
MMCore.RegistMouseEventFuncref(int key, KeyMouseEventFuncref funcref);
// 参数: key(1~98键盘,1~5鼠标), funcref(委托函数: keydown为true按下false弹起, player玩家ID)

// MMCore.cs:19093 - 执行委托
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
//每个循环体（主副线程）上设计了5个基础事件,虽可追加方法体挂上运行但挂的都是MMCore核心方法,建议用TimerUpdate、Trigger类来新建线程

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

**重要说明**：
- 当前 `StartKeyMouseEventLoop(playerID, isBackground)` 是单玩家版本，内部只创建一个 RecordService 实例，绑定单个 PlayerID
- 若需支持真正的多人联机（每个玩家独立监听自己的输入），需分别为每个玩家创建独立的 RecordService 实例并各自启动钩子
- 此时不要使用 StartKeyMouseEventLoop，需手动管理多个 RecordService 实例

**多人联机使用示范**：

```csharp
public static class MultiplayerInputManager
{
    // 存储每个玩家的状态信息
    private static Dictionary<int, PlayerInputState> playerInputStates = new Dictionary<int, PlayerInputState>();

    // 每个玩家的状态类
    private class PlayerInputState
    {
        public RecordService Service;
        public int LastMouseX;
        public int LastMouseY;
        public bool[] LastMouseState = new bool[MMCore.c_mouseMax + 1];
        public bool[] LastKeyState = new bool[MMCore.c_keyMax + 1];
    }

    // 初始化多人输入系统
    public static void Initialize(int[] playerIDs)
    {
        foreach (int playerID in playerIDs)
        {
            var state = new PlayerInputState();
            state.Service = new RecordService(playerID);
            state.Service.StartMouseHook();
            state.Service.StartKeyboardHook();
            playerInputStates[playerID] = state;
        }

        // 注册到 MainUpdate，每帧更新所有玩家输入
        MMCore.RegistEntryEventFuncref(MMCore.Entry.MainUpdate, UpdateAllPlayerInputs);
    }

    // 每帧更新所有玩家输入
    private static void UpdateAllPlayerInputs()
    {
        foreach (var kvp in playerInputStates)
        {
            int player = kvp.Key;
            PlayerInputState state = kvp.Value;
            RecordService service = state.Service;

            // 检测鼠标移动
            if (service.X != state.LastMouseX || service.Y != state.LastMouseY)
            {
                MMCore.MouseMove(player, new Vector3F(service.X, service.Y, service.Z), service.X, service.Y);
                state.LastMouseX = service.X;
                state.LastMouseY = service.Y;
            }

            // 检测鼠标按键状态变化
            for (int button = 1; button <= MMCore.c_mouseMax; button++)
            {
                bool currentState = service.GetMouseButtonState(button);
                if (currentState != state.LastMouseState[button])
                {
                    if (currentState)
                    {
                        MMCore.MouseDown(player, button, new Vector3F(service.X, service.Y, service.Z), service.X, service.Y);
                    }
                    else
                    {
                        MMCore.MouseUp(player, button, new Vector3F(service.X, service.Y, service.Z), service.X, service.Y);
                    }
                    state.LastMouseState[button] = currentState;
                }
            }

            // 检测键盘状态变化
            for (int mmKey = 0; mmKey <= MMCore.c_keyMax; mmKey++)
            {
                int virtualKey = MMCore.ConvertMMCoreKeyToVirtualKey(mmKey);
                if (virtualKey >= 0)
                {
                    bool currentState = service.GetKeyState(virtualKey);
                    if (currentState != state.LastKeyState[mmKey])
                    {
                        if (currentState)
                        {
                            MMCore.KeyDown(player, mmKey);
                        }
                        else
                        {
                            MMCore.KeyUp(player, mmKey);
                        }
                        state.LastKeyState[mmKey] = currentState;
                    }
                }
            }
        }
    }

    // 停止所有玩家的输入监听
    public static void Shutdown()
    {
        foreach (var state in playerInputStates.Values)
        {
            state.Service.StopMouseHook();
            state.Service.StopKeyboardHook();
        }
        playerInputStates.Clear();
    }
}
```

## 四、完整使用示例

// 1. [必要]启动键鼠事件循环(内部自动创建RecordService、启动钩子、注册MainUpdate更新、启动50ms循环)
MMCore.StartKeyMouseEventLoop(playerID, isBackground);

// 2. [必要]注册按键/鼠标事件处理委托(键盘1~98,鼠标1~5含2个侧键)
// 委托签名: void Handler(bool keydown, int player) - keydown为true按下false弹起
MMCore.RegistKeyEventFuncref(key, (keydown, player) => { ... });
MMCore.RegistMouseEventFuncref(button, (keydown, player) => { ... });

// 3. [必要]注册MouseKeyUpWait到MainUpdate处理按键队列延迟弹起(遍历全部玩家)
MMCore.RegistEntryEventFuncref(MMCore.Entry.MainUpdate, () =>
{
    for (int player = 1; player <= 15; player++) MMCore.MouseKeyUpWait(player);
});

// 4. [非必要]启用蓄力/双击功能(如需使用)
MMCore.chargeEnable = true;
MMCore.doubleClickEnable = true;
if (MMCore.chargeEnable) MMCore.RegistEntryEventFuncref(MMCore.Entry.MainUpdate, MMCore.ChargeManager);
if (MMCore.doubleClickEnable) MMCore.RegistEntryEventFuncref(MMCore.Entry.MainUpdate, MMCore.DoubleClickManager);

// 5. [可选]资源清理(程序结束或不再使用时)
MMCore.StopKeyMouseEventLoop();
```

**注意**: StartKeyMouseEventLoop内部已自动完成RecordService创建、钩子启动、MainUpdate注册等操作,
无需手动管理这些底层细节.设计时如需区分玩家,直接在委托回调中通过player参数判断即可.
*/

#endregion