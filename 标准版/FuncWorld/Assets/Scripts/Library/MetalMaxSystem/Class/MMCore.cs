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
using Unity.VisualScripting;
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
using System.Windows.Input;
using MetalMaxSystem;
#endif
#endif
#endregion

namespace MetalMaxSystem
{
    /// <summary>
    /// MM功能库核心类
    /// </summary>
    public static partial class MMCore
    {
        #region 常量

        //键盘按键映射

        public const int c_keyNone = -1;
        public const int c_keyShift = 0;
        public const int c_keyControl = 1;
        public const int c_keyAlt = 2;
        public const int c_key0 = 3;
        public const int c_key1 = 4;
        public const int c_key2 = 5;
        public const int c_key3 = 6;
        public const int c_key4 = 7;
        public const int c_key5 = 8;
        public const int c_key6 = 9;
        public const int c_key7 = 10;
        public const int c_key8 = 11;
        public const int c_key9 = 12;
        public const int c_keyA = 13;
        public const int c_keyB = 14;
        public const int c_keyC = 15;
        public const int c_keyD = 16;
        public const int c_keyE = 17;
        public const int c_keyF = 18;
        public const int c_keyG = 19;
        public const int c_keyH = 20;
        public const int c_keyI = 21;
        public const int c_keyJ = 22;
        public const int c_keyK = 23;
        public const int c_keyL = 24;
        public const int c_keyM = 25;
        public const int c_keyN = 26;
        public const int c_keyO = 27;
        public const int c_keyP = 28;
        public const int c_keyQ = 29;
        public const int c_keyR = 30;
        public const int c_keyS = 31;
        public const int c_keyT = 32;
        public const int c_keyU = 33;
        public const int c_keyV = 34;
        public const int c_keyW = 35;
        public const int c_keyX = 36;
        public const int c_keyY = 37;
        public const int c_keyZ = 38;
        public const int c_keySpace = 39;
        public const int c_keyGrave = 40;
        public const int c_keyNumPad0 = 41;
        public const int c_keyNumPad1 = 42;
        public const int c_keyNumPad2 = 43;
        public const int c_keyNumPad3 = 44;
        public const int c_keyNumPad4 = 45;
        public const int c_keyNumPad5 = 46;
        public const int c_keyNumPad6 = 47;
        public const int c_keyNumPad7 = 48;
        public const int c_keyNumPad8 = 49;
        public const int c_keyNumPad9 = 50;
        public const int c_keyNumPadPlus = 51;
        public const int c_keyNumPadMinus = 52;
        public const int c_keyNumPadMultiply = 53;
        public const int c_keyNumPadDivide = 54;
        public const int c_keyNumPadDecimal = 55;
        public const int c_keyEquals = 56;
        public const int c_keyMinus = 57;
        public const int c_keyBracketOpen = 58;
        public const int c_keyBracketClose = 59;
        public const int c_keyBackSlash = 60;
        public const int c_keySemiColon = 61;
        public const int c_keyApostrophe = 62;
        public const int c_keyComma = 63;
        public const int c_keyPeriod = 64;
        public const int c_keySlash = 65;
        public const int c_keyEscape = 66;
        public const int c_keyEnter = 67;
        public const int c_keyBackSpace = 68;
        public const int c_keyTab = 69;
        public const int c_keyLeft = 70;
        public const int c_keyUp = 71;
        public const int c_keyRight = 72;
        public const int c_keyDown = 73;
        public const int c_keyInsert = 74;
        public const int c_keyDelete = 75;
        public const int c_keyHome = 76;
        public const int c_keyEnd = 77;
        public const int c_keyPageUp = 78;
        public const int c_keyPageDown = 79;
        public const int c_keyCapsLock = 80;
        public const int c_keyNumLock = 81;
        public const int c_keyScrollLock = 82;
        public const int c_keyPause = 83;
        public const int c_keyPrintScreen = 84;
        public const int c_keyNextTrack = 85;
        public const int c_keyPrevTrack = 86;
        public const int c_keyF1 = 87;
        public const int c_keyF2 = 88;
        public const int c_keyF3 = 89;
        public const int c_keyF4 = 90;
        public const int c_keyF5 = 91;
        public const int c_keyF6 = 92;
        public const int c_keyF7 = 93;
        public const int c_keyF8 = 94;
        public const int c_keyF9 = 95;
        public const int c_keyF10 = 96;
        public const int c_keyF11 = 97;
        public const int c_keyF12 = 98;

        //鼠标按键映射

        public const int c_mouseButtonNone = 0;
        public const int c_mouseButtonLeft = 1;
        public const int c_mouseButtonMiddle = 2;
        public const int c_mouseButtonRight = 3;
        public const int c_mouseButtonXButton1 = 4;
        public const int c_mouseButtonXButton2 = 5;

        //全键共用序号时,键盘按键=0~98不变,鼠标按键:99 = mouseButtonLeft,100 = mouseButtonMiddle,101 = mouseButtonRight,102 =mouseButtonXButton1,103 = mouseButtonXButton2

        //键鼠函数引用上限及单键注册上限

        /// <summary>
        /// 键盘按键句柄上限(句柄范围0~98,无按键-1)
        /// </summary>
        public const int c_keyMax = 98;
        /// <summary>
        /// 每个键盘按键可注册函数上限
        /// </summary>
        public const int c_regKeyMax = 8;
        /// <summary>
        /// 鼠标按键句柄上限(句柄范围1~5,无按键0)
        /// </summary>
        public const int c_mouseMax = 5;
        /// <summary>
        /// 每个鼠标按键可注册函数上限
        /// </summary>
        public const int c_regMouseMax = 24;

        #endregion

        #region "全局和局部变量"(主要存放不同作用域下的无属性字段)

        //类只有字段没变量说法,但理论上公有静态字段是该程序在内存中唯一的全局变量,无论类实例化多次或多线程从模板调用,它只生成一次副本直到程序结束才清理
        //而非静态(实例)类每次实例化都复制一份模板去形成多个副本,私有实例字段相当于类的局部变量
        //不标Static则类及其成员在结束时垃圾回收,标Static则副本唯一且程序结束才从内存消失
        //函数内声明的静态局部变量在函数结束时也不参与垃圾回收,以便相同函数重复访问
        //静态数据是从模板形成的内存中唯一的可修改副本(不同类中声明同一名称也不一样,有命名空间和类名路径分隔,无需担心重复)
        //数组元素数量上限均+1是习惯,防止某些循环以数组判断时最后退出还+1导致超限

        /// <summary>
        /// MMCore.Write或WriteLine中的文件写入器
        /// </summary>
        public static FileWriter fileWriter;
        /// <summary>
        /// 是否在调用MMCore.Write或WriteLine、WriteLineFlush、WriteClose、WriteCopy时顺带Debug调试(不包含WriteLineNow和WriteNow)
        /// </summary>
        public static bool writeTell = false;

        /// <summary>
        /// 内部键鼠事件监听服务实例(由RecordService监听底层钩子并记录状态)
        /// </summary>
        private static RecordService recordService;
        /// <summary>
        /// 内部键鼠事件是否已启动
        /// </summary>
        private static bool keyMouseEventState = false;

        /// <summary>
        /// 静态随机数生成器(避免频繁创建Random实例)
        /// </summary>
        private static readonly Random _random = new Random();
        /// <summary>
        /// 随机数生成器的线程同步锁
        /// </summary>
        private static readonly object _randomLock = new object();

        /// <summary>
        /// 键盘按键已注册数量(每个数组元素算1个,即使它们+=多个委托函数)
        /// </summary>
        private static int[] keyEventFuncrefGroupNum = new int[c_keyMax + 1];//内部使用

        /// <summary>
        /// 鼠标按键的已注册数量(每个数组元素算1个,即使它们+=多个委托函数)
        /// </summary>
        private static int[] mouseEventFuncrefGroupNum = new int[c_mouseMax + 1];//内部使用

        #region 哈希表声明(更多类型请使用UserDataTable<T>、DataTable<T>、TDataTable<T>)

        //效率:字典 > 哈希表 >> 字典.ToString() > 跨线程字典

        #region 哈希表(可跨线程读写,但不支持泛型)
        /// <summary>
        /// 全局哈希表(不排泄,直到程序结束)
        /// </summary>
        private static Hashtable globalHashTable = new Hashtable();//内部使用
        /// <summary>
        /// 临时哈希表(函数或动作集结束时应手动排泄)
        /// </summary>
        private static Hashtable localHashTable = new Hashtable();//内部使用
        #endregion

        #endregion

        /// <summary>
        /// 键盘按键事件引用委托类型变量数组[c_keyMax + 1, c_regKeyMax + 1],用于自定义委托函数注册
        /// </summary>
        private static KeyMouseEventFuncref[,] keyEventFuncrefGroup = new KeyMouseEventFuncref[c_keyMax + 1, c_regKeyMax + 1];//内部使用
        /// <summary>
        /// 鼠标按键事件引用委托类型变量数组[c_mouseMax + 1, c_regMouseMax + 1],用于自定义委托函数注册
        /// </summary>
        private static KeyMouseEventFuncref[,] mouseEventFuncrefGroup = new KeyMouseEventFuncref[c_mouseMax + 1, c_regMouseMax + 1];//内部使用

        #endregion

        #region 字段及其属性方法

        //字段及其属性方法(避免不安全读写,private保护和隐藏字段,设计成只允许通过public修饰的属性方法间接去安全读写)
        //本库前缀单个_开头字段表示其拥有属性方法(若有双_开头表示自定义类型如委托)

        private static bool[] _stopKeyMouseEvent = new bool[Game.c_maxPlayers + 1];
        /// <summary>
        /// 用户按键事件禁用状态(用于过场、剧情对话、特殊技能如禁锢时强制停用用户的按键事件)
        /// </summary>
        public static bool[] StopKeyMouseEvent
        {
            get
            {
                return _stopKeyMouseEvent;
            }

            set
            {
                _stopKeyMouseEvent = value;
            }
        }

        private static Trigger _mainUpdate;
        /// <summary>
        /// 主计时器循环线程.是运行内置事件的核心周期触发器,默认运行间隔=0.0625s(现实时间)
        /// </summary>
        public static Trigger MainUpdate
        {
            get
            {
                if (_mainUpdate == null)
                {
                    _mainUpdate = new Trigger();
                }
                return _mainUpdate;
            }
            set
            {
                _mainUpdate = value;
            }
        }

        private static Trigger _subUpdate;
        /// <summary>
        /// 副计时器循环线程.是运行内置事件的备用周期触发器,默认运行间隔=1s(现实时间)
        /// </summary>
        public static Trigger SubUpdate
        {
            get
            {
                if (_subUpdate == null)
                {
                    _subUpdate = new Trigger();
                }
                return _subUpdate;
            }
            set
            {
                _subUpdate = value;
            }
        }

        /// <summary>
        /// 存储区容错处理当表键值存在时执行线程默认等待的间隔(50毫秒).常用于多线程触发器频繁写值,如大量注册注销动作使存储区数据重排序的,因表正在使用需排队等待完成才给执行下一个.执行原理:将调用该函数的当前线程反复挂起period毫秒,直到动作要写入的存储区闲置
        /// </summary>
        public static int dataTableThreadWaitPeriod = 50;

        /// <summary>
        /// 本地主机编号.默认-1(表示未设置).
        /// 作为玩家ID使用时:推荐用户1~15,中立0及设置为电脑的ID不算活动玩家,16是系统操作(上帝),17起算观战玩家.
        /// </summary>
        public static int LocalID { get; set; } = -1;

        private static int _directoryEmptyUserDefIndex = 0;
        /// <summary>
        /// 用户定义的空目录形式,以供内部判断:0是子文件(夹)数量为0,1是目录大小为0,2是前两者必须都符合,若用户输入错误,本属性方法将纠正为默认值0
        /// </summary>
        public static int DirectoryEmptyUserDefIndex
        {
            get
            {
                return _directoryEmptyUserDefIndex;
            }
            //若用户输入错误,纠正为默认值0
            set
            {
                if (value >= 0 && value <= 2)
                {
                    _directoryEmptyUserDefIndex = value;
                }
                else
                {
                    _directoryEmptyUserDefIndex = 0;
                }
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 核心类
        /// </summary>
        static MMCore()
        {
            //这里可给字段进行第二次赋值或安排其他动作(字段的首次赋值是在声明时,同一次初始化执行顺序受动作所在上下文影响)
        }

        #endregion

        #region Functions 数学公式

        /// <summary>
        /// 随机整数(不含最大值)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>返回[min,max)之间的随机整数</returns>
        public static int RandomInt(int min, int max)
        {
            // System.Random r = new System.Random(Guid.NewGuid().GetHashCode());
            // if (min <= max)
            // {
            //     return r.Next(min, max);
            // }
            // else
            // {
            //     return r.Next(max, min);
            lock (_randomLock)
            {
                if (min <= max)
                {
                    return _random.Next(min, max);
                }
                return _random.Next(max, min);
            }
        }

        /// <summary>
        /// 随机实数(不含最大值)
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>返回[min,max)之间的随机实数</returns>
        public static double RandomDouble(double min, double max)
        {
            // System.Random r = new System.Random(Guid.NewGuid().GetHashCode());
            // if (min <= max)
            // {
            //     return min + (max - min) * r.NextDouble();
            // }
            // else
            // {
            //     return max + (min - max) * r.NextDouble();
            lock (_randomLock)
            {
                if (min <= max)
                {
                    return min + (max - min) * _random.NextDouble();
                }
                return max + (min - max) * _random.NextDouble();
            }
        }

        /// <summary>
        /// 随机角度
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>返回[0,360)之间的随机角度</returns>
        public static double RandomAngle()
        {
            // System.Random r = new System.Random(Guid.NewGuid().GetHashCode());
            // return 360 * r.NextDouble();
            lock (_randomLock)
            {
                return 360 * _random.NextDouble();
            }
        }

        /// <summary>
        /// 度转弧度
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static double RadianToDegree(double radian)
        {
            return radian * (180.0 / Mathf.PI);
        }

        /// <summary>
        /// 度转弧度
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static float RadianToDegree(float radian)
        {
            return (float)(radian * (180.0 / Mathf.PI));
        }

        /// <summary>
        /// 将Vector3转Vector2(去掉Z轴)
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2F ToVector2F(Vector3F vector)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return new Vector2F(vector.x, vector.y);
#else
            return new Vector2F(vector.X, vector.Y);
#endif
        }

        /// <summary>
        ///  以实数返回二维坐标(x,y)与(a,b)形成的角度(单位:度)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AngleBetween(float x, float y, float a, float b)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Vector2F.Angle(new Vector2F(x, y), new Vector2F(a, b));
#else
            Vector2F vector1 = new Vector2F(x, y);
            Vector2F vector2 = new Vector2F(a, b);
            float dotProduct = Vector2F.Dot(vector1, vector2);
#if NETFRAMEWORK
            float magnitude1 = (float)Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y);
            float magnitude2 = (float)Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y);
            //180除以π(圆周率)的结果等于57.29578
            float angle = (float)(Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI);
#else
            float magnitude1 = Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y);
            float magnitude2 = Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y);
            float angle = Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI;
#endif

            return angle;
#endif
        }

        /// <summary>
        /// 以实数返回三维坐标(x,y,z)与(a,b,c)形成的角度(单位:度)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float AngleBetween(float x, float y, float z, float a, float b, float c)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Vector3F.Angle(new Vector3F(x, y, z), new Vector3F(a, b, z));
#else
            Vector3F vector1 = new Vector3F(x, y, z);
            Vector3F vector2 = new Vector3F(a, b, c);
            float dotProduct = Vector3F.Dot(vector1, vector2);
#if NETFRAMEWORK
            float magnitude1 = (float)Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y + vector1.Z * vector1.Z);
            float magnitude2 = (float)Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y + vector2.Z * vector2.Z);
            float angle = (float)(Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI);
#else
            float magnitude1 = Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y + vector1.Z * vector1.Z);
            float magnitude2 = Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y + vector2.Z * vector2.Z);
            float angle = Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI;
#endif
            return angle;
#endif
        }

        /// <summary>
        /// 以实数返回二维点1点2形成的角度(单位:度).
        /// Returns the angle from point 1 to point 2 as a real value, in degrees.
        /// </summary>
        /// <param name="point1">二维点</param>
        /// <param name="point2">二维点</param>
        /// <returns></returns>
        public static float AngleBetween(Point2F point1, Point2F point2)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Vector2F.Angle(new Vector2F(point1.x, point1.y), new Vector2F(point2.x, point2.y));
#else
            float X1 = point1.x, Y1 = point1.y, X2 = point2.y, Y2 = point2.y;
#if NETFRAMEWORK
            float angleOfLine = (float)(Mathf.Atan2((Y2 - Y1), (X2 - X1)) * 180 / Mathf.PI);
#else
            float angleOfLine = Mathf.Atan2((Y2 - Y1), (X2 - X1)) * 180 / Mathf.PI;
#endif
            return angleOfLine;
#endif
        }

        /// <summary>
        /// 以实数返回三维点1点2形成的角度(单位:度)
        /// </summary>
        /// <param name="point1">三维点</param>
        /// <param name="point2">三维点</param>
        /// <returns></returns>
        public static float AngleBetween(Point3F point1, Point3F point2)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Vector3F.Angle(new Vector3F(point1.x, point1.y, point1.z), new Vector3F(point2.x, point2.y, point2.z));
#else
            float X1 = point1.x, Y1 = point1.y, Z1 = point1.z;
            float X2 = point2.x, Y2 = point2.y, Z2 = point2.z;
            float dotProduct = X1 * X2 + Y1 * Y2 + Z1 * Z2;
#if NETFRAMEWORK
            float magnitude1 = (float)Mathf.Sqrt(X1 * X1 + Y1 * Y1 + Z1 * Z1);
            float magnitude2 = (float)Mathf.Sqrt(X2 * X2 + Y2 * Y2 + Z2 * Z2);
            float cosineOfAngle = dotProduct / (magnitude1 * magnitude2);
            float angle = (float)(Mathf.Acos(cosineOfAngle) * 180 / Mathf.PI);
#else
            float magnitude1 = Mathf.Sqrt(X1 * X1 + Y1 * Y1 + Z1 * Z1);
            float magnitude2 = Mathf.Sqrt(X2 * X2 + Y2 * Y2 + Z2 * Z2);
            float cosineOfAngle = dotProduct / (magnitude1 * magnitude2);
            float angle = Mathf.Acos(cosineOfAngle) * 180 / Mathf.PI;
#endif
            return angle;
#endif

        }

        /// <summary>
        /// 以实数返回二维向量之间形成的角度(单位:度)
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static float AngleBetween(Vector2F vector1, Vector2F vector2)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Vector2F.Angle(vector1, vector2);
#else
            float dotProduct = Vector2F.Dot(vector1, vector2);
#if NETFRAMEWORK
            float magnitude1 = (float)Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y);
            float magnitude2 = (float)Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y);
            float angle = (float)(Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI);
#else
            float magnitude1 = Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y);
            float magnitude2 = Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y);
            float angle = Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI;
#endif
            return angle;
#endif
        }

        /// <summary>
        /// 以实数返回三维向量之间形成的角度(单位:度)
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static float AngleBetween(Vector3F vector1, Vector3F vector2)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Vector3F.Angle(vector1, vector2);
#else
            float dotProduct = Vector3F.Dot(vector1, vector2);
#if NETFRAMEWORK
            float magnitude1 = (float)Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y + vector1.Z * vector1.Z);
            float magnitude2 = (float)Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y + vector2.Z * vector2.Z);
            float angle = (float)(Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI);
#else
            float magnitude1 = Mathf.Sqrt(vector1.X * vector1.X + vector1.Y * vector1.Y + vector1.Z * vector1.Z);
            float magnitude2 = Mathf.Sqrt(vector2.X * vector2.X + vector2.Y * vector2.Y + vector2.Z * vector2.Z);
            float angle = Mathf.Acos(dotProduct / (magnitude1 * magnitude2)) * 180 / Mathf.PI;
#endif
            return angle;
#endif
        }

        /// <summary>
        /// 以实数返回二维坐标(x,y)与(a,b)形成的距离(单位:m)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Distance(float x, float y, float a, float b)
        {
            float x1 = x;
            float y1 = y;

            float x2 = a;
            float y2 = b;
#if NETFRAMEWORK
            float result = (float)Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2));
#else
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2));
#endif
            return result;
        }

        /// <summary>
        /// 以实数返回三维坐标(pixelX,pixelY,z)与(a,b,c)形成的距离(单位:度)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float Distance(float x, float y, float z, float a, float b, float c)
        {
            float x1 = x;
            float y1 = y;
            float z1 = z;

            float x2 = a;
            float y2 = b;
            float z2 = c;
#if NETFRAMEWORK
            float result = (float)Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2) + Mathf.Pow((z1 - z2), 2));
#else
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2) + Mathf.Pow((z1 - z2), 2));
#endif
            return result;
        }

        /// <summary>
        /// 以实数返回二维向量之间形成的距离(单位:m)
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static float Distance(Vector2F vector1, Vector2F vector2)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            float x1 = vector1.x;
            float y1 = vector1.y;
            float x2 = vector2.x;
            float y2 = vector2.y;
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2));
#else
            float x1 = vector1.X;
            float y1 = vector1.Y;
            float x2 = vector2.X;
            float y2 = vector2.Y;
#if NETFRAMEWORK
            float result = (float)Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2));
#else
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2));
#endif
#endif
            return result;
        }

        /// <summary>
        /// 以实数返回三维向量之间形成的距离(单位:m)
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public static float Distance(Vector3F vector1, Vector3F vector2)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            float x1 = vector1.x;
            float y1 = vector1.y;
            float z1 = vector1.z;
            float x2 = vector2.x;
            float y2 = vector2.y;
            float z2 = vector2.z;
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2) + Mathf.Pow((z1 - z2), 2));
#else
            float x1 = vector1.X;
            float y1 = vector1.Y;
            float z1 = vector1.Z;
            float x2 = vector2.X;
            float y2 = vector2.Y;
            float z2 = vector2.Z;
#if NETFRAMEWORK
            float result = (float)Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2) + Mathf.Pow((z1 - z2), 2));
#else
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2) + Mathf.Pow((z1 - z2), 2));
#endif
#endif
            return result;
        }

        /// <summary>
        /// 以实数返回二维点之间形成的距离(单位:m)
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static float Distance(Point2F point1, Point2F point2)
        {
            float x1 = point1.x;
            float y1 = point1.y;

            float x2 = point2.x;
            float y2 = point2.y;
#if NETFRAMEWORK
            float result = (float)Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2));
#else
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2));
#endif
            return result;
        }

        /// <summary>
        /// 以实数返回三维点之间形成的距离(单位:m)
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static float Distance(Point3F point1, Point3F point2)
        {
            float x1 = point1.x;
            float y1 = point1.y;
            float z1 = point1.z;

            float x2 = point2.x;
            float y2 = point2.y;
            float z2 = point2.z;
#if NETFRAMEWORK
            float result = (float)Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2) + Mathf.Pow((z1 - z2), 2));
#else
            float result = Mathf.Sqrt(Mathf.Pow((x1 - x2), 2) + Mathf.Pow((y1 - y2), 2) + Mathf.Pow((z1 - z2), 2));
#endif
            return result;
        }

        #endregion

        #region Functions 通用功能

        /// <summary>
        /// 用ThreadStringBuilder构建键值字符串.
        /// 以baseKey为基础,在其后依次添加下划线和indices中的每个元素,形成一个新的字符串返回.如BuildKey("key", 1, 2)返回"key12"
        /// </summary>
        /// <param name="baseKey">基础键值</param>
        /// <param name="indices">要添加的索引数组</param>
        /// <returns>构建后的键值字符串</returns>
        public static string BuildKey<T>(string baseKey, params T[] indices)
        {
            if (indices.Length == 0) return baseKey;
            var sb = ThreadStringBuilder.Get();
            sb.Append(baseKey);
            foreach (var index in indices)
            {
                sb.Append(index);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 用ThreadStringBuilder构建键值字符串.
        /// 以baseKey为基础,在其后依次添加下划线和indices中的每个元素,形成一个新的字符串返回.如BuildKey("key", 1, 2)返回"key_1_2"
        /// </summary>
        /// <param name="baseKey">基础键值</param>
        /// <param name="indices">要添加的索引数组</param>
        /// <returns>构建后的键值字符串</returns>
        public static string BuildKeyWithSeparator<T>(string baseKey, params T[] indices)
        {
            if (indices.Length == 0) return baseKey;
            var sb = ThreadStringBuilder.Get();
            sb.Append(baseKey);
            foreach (var index in indices)
            {
                sb.Append('_');
                sb.Append(index);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 用ThreadStringBuilder构建键值字符串(使用指定分隔符).
        /// 以baseKey为基础,在其后依次添加分隔符和indices中的每个元素,形成一个新的字符串返回.
        /// 如 BuildKeyWithSeparator('_', "key", 1, 2) 返回 "key_1_2"
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <param name="baseKey">基础键值</param>
        /// <param name="indices">要添加的索引数组</param>
        /// <returns>构建后的键值字符串</returns>
        public static string BuildKeyWithSeparator<T>(char separator, string baseKey, params T[] indices)
        {
            if (indices.Length == 0) return baseKey;
            var sb = ThreadStringBuilder.Get();
            sb.Append(baseKey);
            foreach (var index in indices)
            {
                sb.Append(separator);
                sb.Append(index);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 内部调试输出(封装版).
        /// Unity环境调用Debug.Log(contents),其他情况使用.NET下的Debug.WriteLine(contents).
        /// 本方法仅推荐在快速测试结果时使用,调试情况请用未封装的原方法以便IDE识别跳转.
        /// </summary>
        /// <param name="contents">内容</param>
        /// <returns></returns>
        public static void Tell(string contents)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log(contents);
#else
            Debug.WriteLine(contents);
#endif
        }

        /// <summary>
        /// 内部调试输出(封装版).
        /// Unity环境调用Debug.LogFormat(contents, args),其他情况使用.NET下的Debug.WriteLine(string.Format(contents, args)).
        /// 本方法仅推荐在快速测试结果时使用,调试情况请用未封装的原方法以便IDE识别跳转.
        /// </summary>
        /// <param name="contents">内容</param>
        /// <param name="args">要组合的其他任意参数</param>
        /// <returns></returns>
        public static void Tell(string contents, params object[] args)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.LogFormat(contents, args);
#else
            Debug.WriteLine(string.Format(contents, args));
#endif
        }

        /// <summary>
        /// 将字符串转换为字节数组,再转成2位16进制字符串格式或转成10进制数字再转为3位8进制字符串格式,以供在Galaxy代码中混淆使用
        /// Galaxy代码会自动转转义8和16位格式字符串(\0及\pixelX)为ASCII值(数字),再转为控制字符使用
        /// </summary>
        /// <param name="input"></param>
        /// <param name="r">向16进制转换的概率,否则向8进制转换</param>
        /// <returns></returns>
        public static string ConvertStringToHOMixed(string input, double r)
        {
            string result = "";
            lock (_randomLock)
            {
                foreach (byte b in Encoding.UTF8.GetBytes(input))
                {
                    //根据随机数和触发概率决定是否执行动作
                    if (_random.NextDouble() < r)
                    {
                        result += string.Format("\\pixelX{0:X2}", b);
                    }
                    else
                    {
                        result += string.Format("\\0{Convert.ToString(b, 8)}", b);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 将字符串转换为字节数组,再转成10进制数字,再转为3位8进制字符串格式,以供在Galaxy代码中混淆使用
        /// Galaxy代码会自动转转义8和16位格式字符串(\0及\pixelX)为ASCII值(数字),再转为控制字符使用
        /// 如八进制"\0124"、"\0114"表示十进制的84和76,Galaxy脚本中识别为"T"和"L"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertStringToOctal(string input)
        {
            string result = "";
            foreach (byte b in Encoding.UTF8.GetBytes(input))
            {
                //result += $"\\0{Convert.ToString(b, 8)}";
                result += string.Format("\\0{Convert.ToString(b, 8)}", b);
            }
            return result;
        }

        /// <summary>
        /// 将字符串转换为字节数组,再转成2位16进制字符串格式,以供在Galaxy代码中混淆使用
        /// Galaxy代码会自动转转义8和16位格式字符串(\0及\pixelX)为ASCII值(数字),再转为控制字符使用
        /// 如十六进制"\x4C"表示十进制的84,Galaxy脚本中识别为"T"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ConvertStringToHex(string input)
        {
            string result = "";
            foreach (byte b in Encoding.UTF8.GetBytes(input))
            {
                //result += $"\\pixelX{b:X2}";
                result += string.Format("\\pixelX{b:X2}", b);
            }
            return result;
        }

        /// <summary>
        /// 去掉代码里的注释
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string RemoveComments(string code)
        {
            string pattern = @"//.*?$|/\*.*?\*/";
            //使用 RegexOptions.Multiline 选项来指定模式应在多个行上进行匹配,并使用 RegexOptions.Singleline 选项来指定模式应在单个连续字符串上进行匹配
            RegexOptions options = RegexOptions.Multiline | RegexOptions.Singleline;
            string result = Regex.Replace(code, pattern, string.Empty, options);
            return result;
        }

        /// <summary>
        /// 去掉代码里的空行
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string RemoveEmptyLines(string code)
        {
            string pattern = @"^\s*$";
            RegexOptions options = RegexOptions.Multiline;
            string result = Regex.Replace(code, pattern, string.Empty, options);
            return result;
        }

        #region 判断文件是否被占用

        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        /// <summary>
        /// 文件是否被占用(WIN32 API调用)
        /// </summary>
        /// <param name="fileFullNmae"></param>
        /// <returns></returns>
        public static bool IsOccupied(string fileFullNmae)
        {
            if (!File.Exists(fileFullNmae)) return false;
            IntPtr vHandle = _lopen(fileFullNmae, OF_READWRITE | OF_SHARE_DENY_NONE);
            var flag = vHandle == HFILE_ERROR;
            CloseHandle(vHandle);
            return flag;
        }

        /// <summary>
        /// 文件是否被占用(文件流判断法)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true表示正在使用,false没有使用 </returns>
        public static bool IsFileInUse(string fileName)
        {
            bool inUse = true;
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                inUse = false;
            }
            catch { }
            finally
            {
                if (fs != null) fs.Dispose();
            }
            return inUse;
        }

        #endregion

        /// <summary>
        /// 递归方式强制删除目录(进最里层删除文件使目录为空后删除这个空目录,层层递出时重复动作),删除前会去掉文件(夹)的Archive、ReadOnly、Hidden属性以确保删除
        /// </summary>
        /// <param name="dirInfo"></param>
        public static void DelDirectoryRecursively(DirectoryInfo dirInfo)
        {
            foreach (DirectoryInfo newInfo in dirInfo.GetDirectories())
            {
                DelDirectoryRecursively(newInfo);//递归遍历子目录
            }
            foreach (FileInfo newInfo in dirInfo.GetFiles())
            {
                //处理每个目录内部的文件(从里层开始删除)
                newInfo.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                newInfo.Delete();
            }
            //对每个目录处理(从里层开始删除)
            dirInfo.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
            dirInfo.Delete(true);
        }

        /// <summary>
        /// 递归方式强制删除目录(进最里层删除文件使目录为空后删除这个空目录,层层递出时重复动作),删除前会去掉文件(夹)的Archive、ReadOnly、Hidden属性以确保删除
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DelDirectoryRecursively(string dirPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            DelDirectoryRecursively(dirInfo);

        }

        /// <summary>
        /// 删除目录.
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <param name="force">是否强制删除,默认true</param>
        /// <returns>删除成功返回真,否则返回假</returns>
        public static bool DelDirectory(DirectoryInfo dirInfo, bool force = true)
        {
            bool torf = false;
            if (dirInfo.Exists)
            {
                try
                {
                    if (force)
                    {
                        // 先清除属性
                        dirInfo.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);

                        // 递归清除所有子目录和文件的相关属性
                        foreach (var info in dirInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
                        {
                            info.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                        }
                    }
                    dirInfo.Delete(true);
                    if (!dirInfo.Exists) { torf = true; }
                }
                catch (UnauthorizedAccessException)
                {
                    Tell($"没有权限删除目录: {dirInfo.FullName}");
                }
                catch (IOException)
                {
                    Tell($"目录正在被占用: {dirInfo.FullName}");
                }
                catch (Exception ex)
                {
                    Tell($"删除目录失败: {ex.Message}");
                }
            }
            return torf;
        }

        /// <summary>
        /// 删除目录.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="force">是否强制删除,默认true</param>
        /// <returns>删除成功返回真,否则返回假</returns>
        public static bool DelDirectory(string dirPath, bool force = true)
        {
            bool torf = false;
            if (Directory.Exists(dirPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                try
                {
                    if (force)
                    {
                        // 先清除属性
                        dirInfo.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);

                        // 递归清除所有子目录和文件的相关属性
                        foreach (var info in dirInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
                        {
                            info.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                        }
                    }
                    dirInfo.Delete(true);
                    if (!dirInfo.Exists) { torf = true; }
                }
                catch (UnauthorizedAccessException)
                {
                    Tell($"没有权限删除目录: {dirInfo.FullName}");
                }
                catch (IOException)
                {
                    Tell($"目录正在被占用: {dirInfo.FullName}");
                }
                catch (Exception ex)
                {
                    Tell($"删除目录失败: {ex.Message}");
                }
            }
            return torf;
        }

        /// <summary>
        /// 删除文件到回收站.已添加Shell API特性[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        /// </summary>
        /// <param name="lpFileOp"></param>
        /// <returns></returns>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        /// <summary>
        /// 删除文件到回收站功能专用结构体,已添加特性[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            public string pFrom;
            public string pTo;
            public ushort fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        /// <summary>
        /// 删除文件到回收站功能专用枚举,已添加特性[Flags]
        /// </summary>
        [Flags]
        public enum SHFileOperationFlags : ushort
        {
            /// <summary>
            /// 不出现错误确认或询问用户的对话框
            /// </summary>
            FOF_SILENT = 0x0004,
            /// <summary>
            /// 不出现任何对话框
            /// </summary>
            FOF_NOCONFIRMATION = 0x0010,
            /// <summary>
            /// 文件删除后可以放到回收站
            /// </summary>
            FOF_ALLOWUNDO = 0x0040,
            /// <summary>
            /// 不出现错误对话框
            /// </summary>
            FOF_NOERRORUI = 0x0400,
        }

        /// <summary>
        /// 删除文件到回收站.支持删除时用户确认.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="torf">删除时是否需要用户确认</param>
        public static void DelFileToRecycleBin(string filePath, bool torf)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT
            {
                wFunc = 0x003,//删除文件到回收站
                pFrom = filePath + '\0'//多个文件以 \0 分隔
            };
            if (!torf)
            {
                //不确认直接删除(通过或运算符集成准许撤销+不出现任何对话框)
                fileop.fFlags = (ushort)(SHFileOperationFlags.FOF_ALLOWUNDO | SHFileOperationFlags.FOF_NOCONFIRMATION);
            }
            else
            {
                //需要用户确认删除,文件操作属性清空
                fileop.fFlags = 0;
            }
            SHFileOperation(ref fileop);
        }

        /// <summary>
        /// 删除目录到回收站.支持删除时用户确认.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="torf">回收站删除提示</param>
        public static void DelDirectoryToRecycleBin(string dirPath, bool torf)
        {
            if (!Directory.Exists(dirPath))
            {
                return;
            }
            SHFILEOPSTRUCT fileop = new SHFILEOPSTRUCT
            {
                wFunc = 0x003,//删除文件到回收站
                pFrom = dirPath + '\0'//多个文件以 \0 分隔
            };
            if (!torf)
            {
                //不确认直接删除(通过或运算符集成准许撤销+不出现任何对话框)
                fileop.fFlags = (ushort)(SHFileOperationFlags.FOF_ALLOWUNDO | SHFileOperationFlags.FOF_NOCONFIRMATION);
            }
            else
            {
                //需要用户确认删除,文件操作属性清空
                fileop.fFlags = 0;
            }
            SHFileOperation(ref fileop);
        }

        /// <summary>
        /// 是否为汉字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsChineseCharacter(char c)
        {
            //检查是否是汉字(基本区块)
            return c >= '\u4e00' && c <= '\u9fff';
            //若需要支持更多区块可在这里添加额外的条件
        }
        /// <summary>
        /// 是否为中文标点符号
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsChinesePunctuation(char c)
        {
            //非英文标点符号且字符位于CJK符号和标点符号的范围内(大致)
            return ((!IsEnglishPunctuation(c)) && c >= '\u3000' && c <= '\u303f');
            //注意:这个范围可能不是完全准确的,需要根据实际情况调整
        }
        /// <summary>
        /// 是否为英文字母
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsEnglishCharacter(char c)
        {
            return (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z');
        }
        /// <summary>
        /// 是否为英文标点符号
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsEnglishPunctuation(char c)
        {
            UnicodeCategory category = char.GetUnicodeCategory(c);
            return category == UnicodeCategory.OpenPunctuation ||
                   category == UnicodeCategory.ClosePunctuation ||
                   category == UnicodeCategory.ConnectorPunctuation ||
                   category == UnicodeCategory.DashPunctuation ||
                   category == UnicodeCategory.InitialQuotePunctuation ||
                   category == UnicodeCategory.FinalQuotePunctuation ||
                   category == UnicodeCategory.OtherPunctuation;

            // char.GetUnicodeCategory方法获取字符的Unicode类别(UnicodeCategory),然后根据这个类别判断字符是否为标点符号
            // Unicode 标准将字符分为多个类别,其中与标点符号相关的类别包括:
            // OpenPunctuation:开标点符号,例如 (、[、{
            // ClosePunctuation:闭标点符号,例如 )、]、}
            // ConnectorPunctuation:连接标点符号,例如 _
            // DashPunctuation:破折号标点符号,例如 -、—
            // InitialQuotePunctuation:初始引号标点符号,例如 “、‘
            // FinalQuotePunctuation:结束引号标点符号,例如 ”、’
            // OtherPunctuation:其他标点符号,包括一些特殊的标点符号,例如 !、@、#、$、%、``、&、* 等
            // 若字符的 Unicode 类别属于上述任何一个类别,方法返回 true,否则返回 false
        }

        /// <summary>
        /// 是否十六进制字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsHexchar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }
        /// <summary>
        /// 是否八进制字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsOctchar(char c)
        {
            return (c >= '0' && c <= '7');
        }
        /// <summary>
        /// 是否十进制字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsDecchar(char c)
        {
            return (c >= '0' && c <= '9');
        }
        /// <summary>
        /// 是否十六进制字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsHexchar(byte c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }
        /// <summary>
        /// 是否八进制字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsOctchar(byte c)
        {
            return (c >= '0' && c <= '7');
        }
        /// <summary>
        /// 是否十进制字符
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsDecchar(byte c)
        {
            return (c >= '0' && c <= '9');
        }

        /// <summary>
        /// 对字符串中特殊字符(如中文、转义字符)进行处理,使输出后的字节数组(可直接ToString)完整代表原文要表达的直观内容.
        /// 应注意该直观内容是原文的解析结果,原文中的转义字符已被还原为实际字符.
        /// 函数在处理原文时支持处理多种转义序列,包括十六进制、八进制和常见的转义字符(如 \n, \t, \r 等).
        /// 通过这种方式,确保了字符串在某些特定上下文中(如网络传输、文件存储)能够正确解析.
        /// 若还原原文或应用于按字节混淆为8和16进制的,在遍历到字节\时应注意该字节在原文其实是两个\字符.
        /// 使用Encoding.UTF8.GetString(bytes)可以将字节数组转为字符串(切记不是bytes.ToString()).
        /// 另外星际2代码中"UI\\Box\\NanaKey_UI_CustomButton_01.dds" ="UI/Box/NanaKey_UI_CustomButton_01.dds",
        /// 两个\虽然是想要表示一个\的意思,但实际1个\不支持的,所以2个\\得改为一个/(或/的转义序列)而不是\的2个转义序列.
        /// </summary>
        /// <param name="Text">任意字符串,可以是夹在冒号间的原文</param>
        /// <param name="torf">true处理连续的两个\字符为1个\(即处理双\的转义),如需保留两个\或变成1个/或其他字符串时则应设置torf=false(默认)</param>
        /// <param name="torfString">torf=false时生效,当torfString不为null则连续2个\字符将被处理为自定义torfString(默认值"/"),只改torf=false而torfString=null时将保留双\</param>
        /// <returns></returns>
        public static byte[] Escape(string Text, bool torf = false, string torfString = "/")
        {
            //C#中的字符Char是Unicode字符,一个Char类型的变量占2个字节,即16位(值范围0-65535),1个Char类型的变量可以存储一个Unicode字符(包括单个中文)
            byte[] bytes = Encoding.UTF8.GetBytes(Text);//将字符串转换为8位字节数组(值范围0-255)
            //Console.WriteLine(BitConverter.GetBytes('地').Length);//2字节,用错了方法
            //BitConverter.GetBytes通常用于将基础数据类型(如整数、浮点数等)转换为字节数组
            //然而这个方法并不适用于字符类型(char)因为char在C#中是一个两字节的Unicode字符而BitConverter主要设计用于处理单字节、双字节、四字节、八字节等固定长度的数据类型
            List<byte> result = new List<byte>();
            for (int i = 0; i < bytes.Length;)
            {//遍历字节数组
                if (bytes[i] != '\\')
                {//若不是转义符,直接添加到结果中
                    result.Add(bytes[i]);
                    i++;
                }
                else
                {//若原文中有转义符,根据转义符的不同,进行处理,因为后续混淆转义2个字符分别是\和n的话不会被编译器认为是换行符(如"Hello\x5C\x6EWorld"输出后是Hello\nWorld)
                    i++;//扫描转义符后的1位字符
                    if (i < bytes.Length)
                    {//若不是最后一个字符
                        string s = "";
                        if (bytes[i] == 'x')
                        {//若该字节是x代表十六进制
                            s = "";
                            i++;//扫描指针继续前进1位
                            int k = 0;
                            while (k < 2 && i + k < bytes.Length && IsHexchar(bytes[i + k])) k++;
                            //十六进制字符的长度为2,这里扫描x之后最大2位字符
                            for (int j = 0; j < k; j++) s += (char)bytes[i + j];//s存入这2位字符
                            //s转换为字节
                            byte hexvalue = Convert.ToByte(s, 16);
                            result.Add(hexvalue);//添加到结果中
                            i += k;//扫描指针继续前进k位,以便下次扫描
                        }
                        else if (bytes[i] == '0')
                        {//若该字节0代表八进制(对0后面最大抓3位字符并转字节数组)
                            s = "";
                            i++;//扫描指针继续前进1位
                            int k = 0;
                            while (k < 3 && i + k < bytes.Length && IsOctchar(bytes[i + k])) k++;
                            //0到177:表示ASCII码表中0到127的字符(即DELETE字符),使用一到三位八进制数(有效转义字符范围)
                            //200到377:表示ASCII码表中128到255的字符,使用三位八进制数(在某些编译器或解释器中可能视为扩展八进制转义字符,用于表示负值(在signed char类型中)或超出ASCII码表范围的值)
                            //上述的k只有0~2没有3
                            for (int j = 0; j < k; j++) s += (char)bytes[i + j];//s存入这k位字符
                            byte hexvalue;
                            try
                            {
                                //尝试将s转换为字节
                                hexvalue = Convert.ToByte(s, 8);
                            }
                            catch
                            {//这里k必然等于3并且转换后的值超过255(转换字节出错),则应将k-1重新计算一次
                                s = "";
                                k--;
                                for (int j = 0; j < k; j++) s += (char)bytes[i + j];
                                if (s == "") hexvalue = 0;
                                else hexvalue = Convert.ToByte(s, 8);
                            }
                            result.Add(hexvalue);
                            i += k;
                        }
                        else if (char.IsDigit((char)bytes[i]))
                        {//若该字节代表的字符是1~9的数字
                            s = "";
                            int k = 0;
                            while (k < 3 && i + k < bytes.Length && IsHexchar(bytes[i + k])) k++;
                            //抓取包含该字节在内的3位十六进制字符(不会包含\,抓到什么就直接转原文字节,只抓3位方便万一是八进制如177)
                            for (int j = 0; j < k; j++) s += (char)bytes[i + j];
                            byte hexvalue;
                            try
                            {
                                hexvalue = Convert.ToByte(s, 8);
                            }
                            catch
                            {
                                s = "";
                                k--;
                                for (int j = 0; j < k; j++) s += (char)bytes[i + j];
                                hexvalue = Convert.ToByte(s, 8);
                            }
                            result.Add(hexvalue);
                            i += k;
                        }
                        else
                        {
                            //处理特殊含义的转义字符
                            switch ((char)bytes[i])
                            {
                                case 'n':
                                    //把原文中的@"\n"(2个字符)变成1个字符'\n',以便后续转义出来的字符被编译器或解释器正确解析(余同)
                                    result.Add((byte)'\n');
                                    break;
                                case 't':
                                    result.Add((byte)'\t');//是水平制表符(Tab)
                                    break;
                                case 'r':
                                    result.Add((byte)'\r');//是回车符(Carriage Return)
                                    break;
                                case '\\':
                                    if (torf)
                                    {//正常转义连续的两个\字符(变成1个)
                                        result.Add((byte)'\\');
                                    }
                                    else
                                    {
                                        if (torfString == null)
                                        {//只改torf=false而torfString=null(默认)时将保留双\
                                            result.Add((byte)'\\');
                                            result.Add((byte)'\\');
                                        }
                                        else
                                        {//用户自定义的torfString
                                            if (torfString != "")
                                            {//torfString不为空
                                                //连续的两个\字符处理变成torfString,用户可填写任意字符串(如/或其他)
                                                for (int j = 0; j < torfString.Length; j++)
                                                {
                                                    result.Add((byte)torfString[j]);
                                                }
                                            }
                                            else
                                            {//用户定义了空torfString
                                                break;//连续的两个\字符处理变成无
                                            }
                                        }
                                    }
                                    break;
                                case '\'':
                                    result.Add((byte)'\'');//是单引号字符(Single Quote)
                                    break;
                                case '\"':
                                    result.Add((byte)'\"');//是双引号字符(Double Quote)
                                    break;
                                case 'b':
                                    result.Add((byte)'\b');//是退格符(Backspace)
                                    break;
                                case 'f':
                                    result.Add((byte)'\f');//是换页符(Formfeed)
                                    break;
                                case 'v':
                                    result.Add((byte)'\v');//是垂直制表符(Vertical Tab)
                                    break;
                                default:
                                    //若是未知的转义符,保留原样2个字符
                                    result.Add((byte)'\\');
                                    result.Add(bytes[i]);
                                    break;
                            }

                            i++;
                        }
                    }
                }
            }

            //if (bFirst) return Escape(Encoding.UTF8.GetString(result.ToArray()), false);
            return result.ToArray();
        }

        /// <summary>
        /// 混淆处理(将字符串变转义序列).字符串中每个字符将被转换为八进制或十六进制(X2)表示
        /// </summary>
        /// <param name="str">任意字符串,可以是夹在冒号间的原文</param>
        /// <param name="torf">true处理连续的两个\字符为1个\(即处理双\的转义),如需保留两个\或变成1个/或其他字符串时则应设置torf=false(默认)</param>
        /// <param name="torfString">torf=false时生效,当torfString不为null则连续2个\字符将被处理为自定义torfString(默认值"/"),只改torf=false而torfString=null时将保留双\</param>
        /// <returns></returns>
        public static string Obfuscate(string str, bool torf = false, string torfString = "/")
        {
            byte[] bytes = Escape(str, torf, torfString);
            Random random = new Random();
            string result = string.Concat(bytes.Select(b =>
            {
                // 随机选择八进制或十六进制表示
                return random.Next(2) == 0
                    ? $"\\x{b:X2}" // 十六进制表示
                    : $"\\{Convert.ToString(b, 8).PadLeft(3, '0')}"; // 八进制表示,补足三位
            }));
            return result;
        }

        /// <summary>
        /// 将十六进制表示的英文+数字组合字符串转回中文表示(用于GalaxyEditor)
        /// </summary>
        /// <param name="hexString">十六位字符组成的字符串如"E58AA8E59BBEE6B58BE8AF95"</param>
        /// <returns>正常返回中文字符串,错误时返回空字符串</returns>
        public static string HexStringToChineseCharacter(string hexString)
        {
            try
            {
                byte[] bytes = new byte[hexString.Length / 2];//创建一个字节数组(C#每个字符Char占2字节16位)
                for (int i = 0; i < hexString.Length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                }
                string result = System.Text.Encoding.UTF8.GetString(bytes);
                return result;
            }
            catch { return ""; }
        }

        /// <summary>
        /// 将字节大小转字符串Byte、KB、MB、GB、TB、PB、EB、ZB、YB、NB形式
        /// </summary>
        /// <param name="Size">字节大小</param>
        /// <param name="Byte">true=强制输出字节单位</param>
        /// <returns></returns>
        public static string CountSize(long Size, bool Byte)
        {
            string strSize = "";
            if (Byte)
            {
                strSize = StrAddSymbol(Size.ToString(), 3, ",") + " Byte";
            }
            else
            {
                if (Size < 1024.00)
                    strSize = Size.ToString() + " Byte";
                else if (Size >= 1024.00 && Size < Math.Pow(1024, 2))
                    strSize = (Size / 1024.00).ToString("F2") + " KB";
                else if (Size >= Math.Pow(1024, 2) && Size < Math.Pow(1024, 3))
                    strSize = (Size / Math.Pow(1024, 2)).ToString("F2") + " MB";
                else if (Size >= Math.Pow(1024, 3) && Size < Math.Pow(1024, 4))
                    strSize = (Size / Math.Pow(1024, 3)).ToString("F2") + " GB";
                else if (Size >= Math.Pow(1024, 4) && Size < Math.Pow(1024, 5))
                    strSize = (Size / Math.Pow(1024, 4)).ToString("F2") + " TB";
                else if (Size >= Math.Pow(1024, 5) && Size < Math.Pow(1024, 6))
                    strSize = (Size / Math.Pow(1024, 5)).ToString("F2") + " PB";
                else if (Size >= Math.Pow(1024, 6) && Size < Math.Pow(1024, 7))
                    strSize = (Size / Math.Pow(1024, 6)).ToString("F2") + " EB";
                else if (Size >= Math.Pow(1024, 7) && Size < Math.Pow(1024, 8))
                    strSize = (Size / Math.Pow(1024, 7)).ToString("F2") + " ZB";
                else if (Size >= Math.Pow(1024, 8) && Size < Math.Pow(1024, 9))
                    strSize = (Size / Math.Pow(1024, 8)).ToString("F2") + " YB";
                else if (Size >= Math.Pow(1024, 9) && Size < Math.Pow(1024, 10))
                    strSize = (Size / Math.Pow(1024, 9)).ToString("F2") + " NB";
                else if (Size >= Math.Pow(1024, 10))
                    strSize = (Size / Math.Pow(1024, 10)).ToString("F2") + " DB";
            }
            return strSize;
        }
        /// <summary>
        /// 将字节大小转字符串Byte、KB、MB、GB、TB、PB、EB、ZB、YB、NB形式
        /// </summary>
        /// <param name="Size">字节大小</param>
        /// <param name="Byte">true=强制输出字节单位</param>
        /// <returns></returns>
        public static string CountSize(double Size, bool Byte)
        {
            string strSize = "";
            if (Byte)
            {
                strSize = StrAddSymbol(Size.ToString(), 3, ",") + " Byte";
            }
            else
            {
                if (Size < 1024.00)
                    strSize = Size.ToString() + " Byte";
                else if (Size >= 1024.00 && Size < Math.Pow(1024, 2))
                    strSize = (Size / 1024.00).ToString("F2") + " KB";
                else if (Size >= Math.Pow(1024, 2) && Size < Math.Pow(1024, 3))
                    strSize = (Size / Math.Pow(1024, 2)).ToString("F2") + " MB";
                else if (Size >= Math.Pow(1024, 3) && Size < Math.Pow(1024, 4))
                    strSize = (Size / Math.Pow(1024, 3)).ToString("F2") + " GB";
                else if (Size >= Math.Pow(1024, 4) && Size < Math.Pow(1024, 5))
                    strSize = (Size / Math.Pow(1024, 4)).ToString("F2") + " TB";
                else if (Size >= Math.Pow(1024, 5) && Size < Math.Pow(1024, 6))
                    strSize = (Size / Math.Pow(1024, 5)).ToString("F2") + " PB";
                else if (Size >= Math.Pow(1024, 6) && Size < Math.Pow(1024, 7))
                    strSize = (Size / Math.Pow(1024, 6)).ToString("F2") + " EB";
                else if (Size >= Math.Pow(1024, 7) && Size < Math.Pow(1024, 8))
                    strSize = (Size / Math.Pow(1024, 7)).ToString("F2") + " ZB";
                else if (Size >= Math.Pow(1024, 8) && Size < Math.Pow(1024, 9))
                    strSize = (Size / Math.Pow(1024, 8)).ToString("F2") + " YB";
                else if (Size >= Math.Pow(1024, 9) && Size < Math.Pow(1024, 10))
                    strSize = (Size / Math.Pow(1024, 9)).ToString("F2") + " NB";
                else if (Size >= Math.Pow(1024, 10))
                    strSize = (Size / Math.Pow(1024, 10)).ToString("F2") + " DB";
            }
            return strSize;
        }

        /// <summary>
        /// 为字符串str每隔every位添加symbol
        /// </summary>
        /// <param name="str"></param>
        /// <param name="every"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static string StrAddSymbol(string str, int every, string symbol)
        {
            string n = "";
            for (int i = str.Length - 1, j = 1; i >= 0; i--)
            {
                n = str[i].ToString() + n;
                if (j > 0 && i > 0 && (j % every == 0))
                {
                    n = symbol + n;
                    j = 0;
                }
                j++;
            }
            return n;
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static long GetFileLength(FileInfo fileInfo)
        {
            long len = 0;
            if (fileInfo.Exists)
            {
                len = fileInfo.Length;
            }
            return len;
        }
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="filePath">文件名完整路径</param>
        /// <returns></returns>
        public static long GetFileLength(string filePath)
        {
            long len = 0;
            if (File.Exists(filePath))
            {
                len = new FileInfo(filePath).Length;
            }
            return len;
        }

        /// <summary>
        /// 递归方法获取目录大小
        /// </summary>
        /// <param name="dirPath">目录完整路径</param>
        /// <returns></returns>
        public static long GetDirectoryLength(string dirPath)
        {
            //判断给定的路径是否存在,若不存在则退出
            if (!Directory.Exists(dirPath))
            {
                return 0;
            }
            long len = 0;
            //定义一个DirectoryInfo对象
            DirectoryInfo di = new DirectoryInfo(dirPath);
            //通过GetFiles方法,获取di目录中的所有文件的大小,量越大越慢
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }
            //获取di中所有的目录,并存到一个新的对象数组中,以进行递归
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }

#if WINDOWS && !(UNITY_EDITOR || UNITY_STANDALONE || MONOGAME) && (NETFRAMEWORK || NET5_0_OR_GREATER)
        /// <summary>
        /// 取得设备硬盘的卷序列号(在Unity、MonoGame不适用)
        /// </summary>
        /// <param name="diskSymbol">盘符</param>
        /// <returns>成功返回卷序列号,失败返回"uHnIk"</returns>
        public static string GetHardDiskID(string diskSymbol)
        {
            try
            {
                string hdInfo = "";
                ManagementObject disk = new ManagementObject(
                    "win32_logicaldisk.deviceid=\"" + diskSymbol + ":\""
                );
                hdInfo = disk.Properties["VolumeSerialNumber"].Value.ToString();
                disk = null;
                return hdInfo.Trim();
            }
            catch
            {
                return "uHnIk";
            }
        }
#endif

        /// <summary>
        /// 验证字符串是否为整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {
            Regex reg1 = new Regex(@"^[0-9]\d*$");
            return reg1.IsMatch(str);
        }

        /// <summary>
        /// 验证字符串是否为合法文件(夹)名称,可以是虚拟路径(本函数不验证其真实存在)
        /// </summary>
        /// <param name="path">文件(夹)路径全名,注意该字符串末尾没有斜杠</param>
        /// <returns></returns>
        public static bool IsDFPath(string path)
        {
            //发现带中文符号会识别不出,为中文符号继续追加()【】:
            Regex regex = new Regex(
                @"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.()【】:~!@#$%^&()\[\]{}+=]+\\?)*$"
            );
            Match result = regex.Match(path);
            return result.Success;
        }

        /// <summary>
        /// 验证字符串路径的文件(夹)是否真实存在
        /// </summary>
        /// <param name="path">文件(夹)路径全名</param>
        /// <returns></returns>
        public static bool IsDF(string path)
        {
            bool torf = false;
            if (Directory.Exists(path) || File.Exists(path))
            {
                torf = true;
            }
            return torf;
        }

        /// <summary>
        /// 判断目标属性是否为真实目录
        /// </summary>
        /// <param name="path">目录路径全名</param>
        /// <returns></returns>
        public static bool IsDirAttributes(string path)
        {
            FileInfo fi = new FileInfo(path);
            if ((fi.Attributes & FileAttributes.Directory) != 0)
            {
                //ReadOnly = 1,
                //Hidden = 2,
                //System = 4,
                //Directory = 16,
                //Archive = 32,
                //Device = 64,
                //若设置了ReadOnly和Directory,则FileAttributes等于16+1=17,二进制为00001001
                //若没有设置目录位,则会得到零:
                //File.GetAttributes(source) = 00000001
                // FileAttributes.Directory = 00001000 &
                //-------------------------------------
                //                            00000000
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证字符串路径的目录是否真实存在
        /// </summary>
        /// <param name="path">目录路径全名</param>
        /// <returns></returns>
        public static bool IsDir(string path)
        {
            bool torf = false;
            if (Directory.Exists(path))
            {
                torf = true;
            }
            return torf;
        }

        /// <summary>
        /// 验证字符串路径的文件是否真实存在
        /// </summary>
        /// <param name="path">文件路径全名</param>
        /// <returns></returns>
        public static bool IsFile(string path)
        {
            bool torf = false;
            if (File.Exists(path))
            {
                torf = true;
            }
            return torf;
        }

        /// <summary>
        /// 验证目录是否为空
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectoryEmpty(string path)
        {
            bool torf = false;
            DirectoryInfo dir = new DirectoryInfo(path);
            //为了效率,只验证当前层即可
            if (dir.GetFiles().Length + dir.GetDirectories().Length == 0)
            {
                torf = true;
            }
            return torf;
        }

        /// <summary>
        /// 验证路径是否为用户定义的空目录,通过MMCore.DirectoryEmptyUserDefIndex属性可定义空目录形式
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectoryEmptyUserDef(string path)
        {
            bool torf = false;
            switch (DirectoryEmptyUserDefIndex) //定义空目录形式
            {
                case 0:
                    if (IsDirectoryEmpty(path))
                    {
                        torf = true;
                    } //里面的子目录和文件数量均为0
                    break;
                case 1:
                    if (GetDirectoryLength(path) == 0)
                    {
                        torf = true;
                    } //目录大小为0
                    break;
                case 2:
                    if (IsDirectoryEmpty(path) && GetDirectoryLength(path) == 0)
                    {
                        torf = true;
                    } //以上两者都要满足
                    break;
                default:
                    if (IsDirectoryEmpty(path))
                    {
                        torf = true;
                    } //里面的子目录和文件数量均为0
                    break;
            }
            return torf;
        }

        /// <summary>
        /// 使用FileWriter写文本每行.写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区
        /// </summary>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        public static void WriteLine(string value, bool bufferAppend = true)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
        }
        /// <summary>
        /// 使用FileWriter写文本每行(默认UTF-8).写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区(直到参数end=true时写入文件,文件若不存在则自动新建)
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="end">立即写入文件并,若flush=false则清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        /// <param name="flush">是否使用Flush方法(不清空StringBuilder),默认false(使用Close方法,会清空StringBuilder)</param>
        public static void WriteLine(string path, string value, bool bufferAppend = true, bool end = false, bool fileAppend = false, bool flush = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
            if (end)
            {
                if (flush) { fileWriter.Flush(path, fileAppend, Encoding.UTF8); } else { fileWriter.Close(path, fileAppend, Encoding.UTF8); }
            }
        }
        /// <summary>
        /// 使用FileWriter写文本每行(默认UTF-8)到文件(若不存在则自动新建).FileWriter将调用Flush()方法,结束后保留MMCore.fileWriter的StringBuilder类型的Buffer缓冲区.
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        public static void WriteLineFlush(string path, string value, bool bufferAppend = true, bool fileAppend = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
            fileWriter.Flush(path, fileAppend, Encoding.UTF8);
        }
        /// <summary>
        /// 使用FileWriter写文本每行(默认UTF-8)到文件(若不存在则自动新建).FileWriter将调用Close()方法,结束后清理MMCore.fileWriter的StringBuilder类型的Buffer缓冲区.
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        public static void WriteLineClose(string path, string value, bool bufferAppend = true, bool fileAppend = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
            fileWriter.Close(path, fileAppend, Encoding.UTF8);
        }
        /// <summary>
        /// 使用FileWriter副本(复制MMCore.fileWriter的StringBuilder类型的Buffer缓冲区)后写文本每行(默认UTF-8)到文件(若不存在则自动新建).
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="end">立即写入文件,若flush=false则清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        /// <param name="flush">是否使用Flush方法(不清空StringBuilder),默认false(使用Close方法,会清空StringBuilder)</param>
        /// <returns></returns>
        public static FileWriter WriteLineCopy(string path, string value, bool bufferAppend = true, bool end = false, bool fileAppend = false, bool flush = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            FileWriter tempFileWriter = new FileWriter();
            if (fileWriter != null)
            {
                tempFileWriter.Buffer.Append(MMCore.fileWriter.Buffer);
            }
            tempFileWriter.WriteLine(value, !bufferAppend);
            if (end)
            {
                if (flush) { tempFileWriter.Flush(path, true, Encoding.UTF8); } else { tempFileWriter.Close(path, true, Encoding.UTF8); }
            }
            return tempFileWriter;
        }
        /// <summary>
        /// 使用FileWriter写文本每行.写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区(直到参数end=true时写入文件,文件若不存在则自动新建)
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="encoding">编码</param>
        /// <param name="end">立即写入文件并,若flush=false则清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        /// <param name="flush">是否使用Flush方法(不清空StringBuilder),默认false(使用Close方法,会清空StringBuilder)</param>
        public static void WriteLine(string path, string value, bool bufferAppend, Encoding encoding, bool end = false, bool fileAppend = false, bool flush = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
            if (end)
            {
                if (flush) { fileWriter.Flush(path, fileAppend, encoding); } else { fileWriter.Close(path, fileAppend, encoding); }
            }
        }

        /// <summary>
        /// 使用FileWriter写文本.写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区
        /// </summary>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        public static void Write(string value, bool bufferAppend = true)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
        }
        /// <summary>
        /// 使用FileWriter写文本(默认UTF-8).写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区(直到参数end=true时写入文件,文件若不存在则自动新建)
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="end">立即写入文件并,若flush=false则清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        /// <param name="flush">是否使用Flush方法(不清空StringBuilder),默认false(使用Close方法,会清空StringBuilder)</param>
        public static void Write(string path, string value, bool bufferAppend = true, bool end = false, bool fileAppend = false, bool flush = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
            if (end)
            {
                if (flush) { fileWriter.Flush(path, fileAppend, Encoding.UTF8); } else { fileWriter.Close(path, fileAppend, Encoding.UTF8); }
            }
        }
        /// <summary>
        /// 使用FileWriter写文本(默认UTF-8)到文件(若不存在则自动新建).FileWriter将调用Flush()方法,结束后保留MMCore.fileWriter的StringBuilder类型的Buffer缓冲区.
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        public static void WriteFlush(string path, string value, bool bufferAppend = true, bool fileAppend = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
            fileWriter.Flush(path, fileAppend, Encoding.UTF8);
        }
        /// <summary>
        /// 使用FileWriter写文本(默认UTF-8)到文件(若不存在则自动新建).FileWriter将调用Close()方法,结束后清理MMCore.fileWriter的StringBuilder类型的Buffer缓冲区.
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        public static void WriteClose(string path, string value, bool bufferAppend = true, bool fileAppend = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
            fileWriter.Close(path, fileAppend, Encoding.UTF8);
        }

        /// <summary>
        /// 使用FileWriter副本(复制MMCore.fileWriter的StringBuilder类型的Buffer缓冲区)后写文本(默认UTF-8)到文件(若不存在则自动新建).
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="end">立即写入文件,若flush=false则清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        /// <param name="flush">是否使用Flush方法(不清空StringBuilder),默认false(使用Close方法,会清空StringBuilder)</param>
        /// <returns></returns>
        public static FileWriter WriteCopy(string path, string value, bool bufferAppend = true, bool end = false, bool fileAppend = false, bool flush = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            FileWriter tempFileWriter = new FileWriter();
            if (fileWriter != null)
            {
                tempFileWriter.Buffer.Append(MMCore.fileWriter.Buffer);
            }
            tempFileWriter.Write(value, !bufferAppend);
            if (end)
            {
                if (flush) { tempFileWriter.Flush(path, true, Encoding.UTF8); } else { tempFileWriter.Close(path, true, Encoding.UTF8); }
            }
            return tempFileWriter;
        }
        /// <summary>
        /// 使用FileWriter写文本.写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区(直到参数end=true时写入文件,文件若不存在则自动新建)
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区(即写入前清理StringBuilder),true向缓冲区追加文本</param>
        /// <param name="encoding">编码</param>
        /// <param name="end">立即写入文件并,若flush=false则清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件,true向文件末尾追加文本</param>
        /// <param name="flush">是否使用Flush方法(不清空StringBuilder),默认false(使用Close方法,会清空StringBuilder)</param>
        public static void Write(string path, string value, bool bufferAppend, Encoding encoding, bool end = false, bool fileAppend = false, bool flush = false)
        {
            if (writeTell == true)
            {
                Tell(value);
            }
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
            if (end)
            {
                if (flush) { fileWriter.Flush(path, fileAppend, encoding); } else { fileWriter.Close(path, fileAppend, encoding); }
            }
        }

        /// <summary>
        /// 立即写文本每行(默认UTF-8),文件若不存在则自动新建.
        /// StreamWriter默认缓冲区大小为8192个字节(8KB),满时自动写入文件,本函数使用using代码块,StreamWriter对象被关闭时,缓冲区中的数据也会被写入文件.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖,true是追加文本</param>
        public static void WriteLineNow(string path, string value, bool append, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, Encoding.UTF8, bufferSize))
            {
                sw.WriteLine(value);
                //sw.Flush(); 不等待sw.Close()即刻写入,对于遍历大量写入来说并不效率,故此时不写
            }
            //using代码块结束,此时StreamWriter对象被关闭,缓冲区中的数据被写入文件

        }
        /// <summary>
        /// 立即写文本每行,文件若不存在则自动新建.
        /// StreamWriter默认缓冲区大小为8192个字节(8KB),满时自动写入文件,本函数使用using代码块,StreamWriter对象被关闭时,缓冲区中的数据也会被写入文件.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖,true是追加文本</param>
        /// <param name="encoding"></param>
        public static void WriteLineNow(string path, string value, bool append, Encoding encoding, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, encoding, bufferSize))
            {
                sw.WriteLine(value);
                //sw.Flush(); 不等待sw.Close()即刻写入,对于遍历大量写入来说并不效率,故此时不写
            }
            //using代码块结束,此时StreamWriter对象被关闭,缓冲区中的数据被写入文件

        }

        /// <summary>
        /// 立即写文本(默认UTF-8),文件若不存在则自动新建.
        /// StreamWriter默认缓冲区大小为8192个字节(8KB),满时自动写入文件,本函数使用using代码块,StreamWriter对象被关闭时,缓冲区中的数据也会被写入文件.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖,true是追加文本</param>
        public static void WriteNow(string path, string value, bool append, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, Encoding.UTF8, bufferSize))
            {
                sw.Write(value);
                //sw.Flush(); 不等待sw.Close()即刻写入,对于遍历大量写入来说并不效率,故此时不写
            }
            //using代码块结束,此时StreamWriter对象被关闭,缓冲区中的数据被写入文件
        }
        /// <summary>
        /// 立即写文本,文件若不存在则自动新建.
        /// StreamWriter默认缓冲区大小为8192个字节(8KB),满时自动写入文件,本函数使用using代码块,StreamWriter对象被关闭时,缓冲区中的数据也会被写入文件.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖,true是追加文本</param>
        /// <param name="encoding"></param>
        public static void WriteNow(string path, string value, bool append, Encoding encoding, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, encoding, bufferSize))
            {
                sw.Write(value);
                //sw.Flush(); 不等待sw.Close()即刻写入,对于遍历大量写入来说并不效率,故此时不写
            }
            //using代码块结束,此时StreamWriter对象被关闭,缓冲区中的数据被写入文件
        }

        /// <summary>
        /// 验证文件大小是否在[a,b]范围
        /// </summary>
        /// <param name="path"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsDFSizeInRange(string path, long a, long b)
        {
            bool torf = false;
            long x;
            for (int i = 0; i < 1; i++)
            {
                if (IsDir(path))
                {
                    x = GetDirectoryLength(path);
                }
                else if (IsFile(path))
                {
                    x = GetFileLength(path);
                }
                else { break; }
                if (x >= a && x <= b)
                {
                    torf = true;
                }
            }
            return torf;
        }

        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字,1=包含,默认为包含</param>
        ///<param name="useLow">是否包含小写字母,1=包含,默认为包含</param>
        ///<param name="useUpp">是否包含大写字母,1=包含,默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符,1=包含,默认为不包含</param>
        ///<param name="custom">要包含的自定义字符,直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length, bool useNum = true, bool useLow = true, bool useUpp = true, bool useSpe = false, string custom = null)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            System.Random r = new System.Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }

        /// <summary>
        /// 复制目录及其内容到新位置.
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="targetDir"></param>
        /// <param name="overwrite">重名时覆盖,默认true</param>
        /// <param name="random">重名不覆盖时随机命名,默认false(按序起名)</param>
        public static void CopyDirectory(string sourceDir, string targetDir, bool overwrite = true, bool random = false)
        {
            string targetFilePath, baseName, tempDirName, extension;
            CreateDirectory(targetDir);

            //处理文件
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));

                if (overwrite || !(File.Exists(targetFilePath) || Directory.Exists(targetFilePath)))
                {
                    File.Copy(file, targetFilePath, overwrite);
                }
                else if (random)
                {
                    //随机8位
                    baseName = Path.GetFileNameWithoutExtension(targetFilePath);
                    extension = Path.GetExtension(targetFilePath);
                    tempDirName = Path.GetDirectoryName(targetFilePath);
                    while (File.Exists(targetFilePath) || Directory.Exists(targetFilePath))
                    {
                        targetFilePath = Path.Combine(tempDirName, $"{baseName}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}");
                    }
                    File.Copy(file, targetFilePath);
                }
                else
                {
                    //按序重命名
                    baseName = Path.GetFileNameWithoutExtension(targetFilePath);
                    extension = Path.GetExtension(targetFilePath);
                    tempDirName = Path.GetDirectoryName(targetFilePath);
                    int counter = 2;
                    while (File.Exists(targetFilePath) || Directory.Exists(targetFilePath))
                    {
                        targetFilePath = Path.Combine(tempDirName, $"{baseName}({counter}){extension}"
                        );
                        counter++;
                    }
                    File.Copy(file, targetFilePath);
                }
            }
            //递归复制子目录
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(subDir, Path.Combine(targetDir, Path.GetFileName(subDir)), overwrite, random);
            }
        }

        /// <summary>
        /// 创建目录,若已存在则什么也不干
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                directory.Create();
            }
        }

        /// <summary>
        /// 创建文件,若已存在则什么也不干
        /// </summary>
        /// <param name="filepath"></param>
        public static void CreateFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                File.Create(filepath);
            }
        }

        /// <summary>
        /// 调用File.WriteAllText将文本内存写入文件(若路径不存在会尝试建立)
        /// </summary>
        /// <param name="fileSavePath"></param>
        /// <param name="content"></param>
        public static void SaveFile(string fileSavePath, string content)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(fileSavePath);

                //若目录不存在,则创建它
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                //写入文件内容
                File.WriteAllText(fileSavePath, content);
                //Tell("保存成功: " + fileSavePath);
            }
            catch (DirectoryNotFoundException e)
            {
                Tell("保存失败: 目录不存在 - " + e.Message);
            }
            catch (IOException e)
            {
                Tell("保存失败: I/O错误 - " + e.Message);
            }
            catch (Exception e)
            {
                Tell("保存失败: " + e.Message);
            }
        }

        /// <summary>
        /// 调用File.WriteAllBytes将文本内存写入文件(若路径不存在会尝试建立)
        /// </summary>
        /// <param name="fileSavePath"></param>
        /// <param name="content"></param>
        public static void SaveFile(string fileSavePath, byte[] content)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(fileSavePath);

                //若目录不存在,则创建它
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                //写入文件内容
                File.WriteAllBytes(fileSavePath, content);
                //Tell("保存成功: " + fileSavePath);
            }
            catch (DirectoryNotFoundException e)
            {
                Tell("保存失败: 目录不存在 - " + e.Message);
            }
            catch (IOException e)
            {
                Tell("保存失败: I/O错误 - " + e.Message);
            }
            catch (Exception e)
            {
                Tell("保存失败: " + e.Message);
            }
        }

        #endregion

        #region Functions 数据表功能

        //字典及跨线程字典,另详DataTable、CDataTable、TDataTable类

        #region 哈希表

        //Hashtable在在多线程可直接操作读但写默认不安全,多写时需开启内部锁保证线程数据同步,用Hashtable.Synchronized方法保安全
        //Hashtable不支持泛型,在查找不存在键时返回空

        /// <summary>
        /// 添加哈希表键值对(重复添加则覆盖)
        /// </summary>
        /// <param name="place">true=全局,false=临时</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void HashTableSet(bool place, string key, object val)
        {
            if (place)
            {
                //存入全局哈希表
                globalHashTable[key] = val;

                //if (globalHashTable.Contains(key)) 
                //{
                //   globalHashTable.Remove(key);
                //}
                //globalHashTable.Add(key, val);
            }
            else
            {
                //存入局部哈希表
                localHashTable[key] = val;

                //if (localHashTable.Contains(key)) { localHashTable.Remove(key); }
                //localHashTable.Add(key, val);
            }
        }

        /// <summary>
        /// 判断哈希表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HashTableKeyExists(bool place, string key)
        {
            if (place) { return globalHashTable.ContainsKey(key); }
            else { return localHashTable.ContainsKey(key); }
        }

        /// <summary>
        /// 判断哈希表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HashTableValueExists(bool place, object value)
        {
            if (place) { return globalHashTable.ContainsValue(value); }
            else { return localHashTable.ContainsValue(value); }
        }

        /// <summary>
        /// 获取哈希表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object HashTableGetValue(bool place, string key)
        {
            if (place) { return globalHashTable[key]; }
            else { return localHashTable[key]; }
        }

        /// <summary>
        /// 从哈希表中移除Key.注:移除并不效率,若要重复使用该键可赋空值)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void HashTableClear0(bool place, string key)
        {
            HashTableRemove(place, key);
        }

        /// <summary>
        /// 从哈希表中移除Key[],模拟1维数组.注:移除并不效率,若要重复使用该键可赋空值)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void HashTableClear1(bool place, string key, int lp_1)
        {
            HashTableRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1));
        }

        /// <summary>
        /// 从哈希表中移除Key[,],模拟2维数组.注:移除并不效率,若要重复使用该键可赋空值)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void HashTableClear2(bool place, string key, int lp_1, int lp_2)
        {
            HashTableRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2));
        }

        /// <summary>
        /// 从哈希表中移除Key[,,],模拟3维数组.注:移除并不效率,若要重复使用该键可赋空值)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void HashTableClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            HashTableRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3));
        }

        /// <summary>
        /// 从哈希表中移除Key[,,,],模拟4维数组.注:移除并不效率,若要重复使用该键可赋空值)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void HashTableClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            HashTableRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4));
        }

        /// <summary>
        /// 移除哈希表键值对.注:移除并不效率,若要重复使用该键可赋空值)
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void HashTableRemove(bool place, string key)
        {
            if (place) { globalHashTable.Remove(key); }
            else { localHashTable.Remove(key); }
        }

        /// <summary>
        /// 保存哈希表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void HashTableSave0(bool place, string key, object val)
        {
            HashTableSet(place, key, val);
        }

        /// <summary>
        /// 保存哈希表键值对,模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void HashTableSave1(bool place, string key, int lp_1, object val)
        {
            HashTableSet(place, ThreadStringBuilder.Concat(key, '_', lp_1), val);
        }

        /// <summary>
        /// 保存哈希表键值对,模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void HashTableSave2(bool place, string key, int lp_1, int lp_2, object val)
        {
            HashTableSet(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2), val);
        }

        /// <summary>
        /// 保存哈希表键值对,模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void HashTableSave3(bool place, string key, int lp_1, int lp_2, int lp_3, object val)
        {
            HashTableSet(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3), val);
        }

        /// <summary>
        /// 保存哈希表键值对,模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void HashTableSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, object val)
        {
            HashTableSet(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4), val);
        }

        /// <summary>
        /// 读取哈希表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object HashTableLoad0(bool place, string key)
        {
            if ((HashTableKeyExists(place, key) == false))
            {
                return null;
            }
            return HashTableGetValue(place, key);
        }

        /// <summary>
        /// 读取哈希表键值对,模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static object HashTableLoad1(bool place, string key, int lp_1)
        {
            if ((HashTableKeyExists(place, ThreadStringBuilder.Concat(key, '_', lp_1)) == false))
            {
                return null;
            }
            return HashTableGetValue(place, ThreadStringBuilder.Concat(key, '_', lp_1));
        }

        /// <summary>
        /// 读取哈希表键值对,模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static object HashTableLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((HashTableKeyExists(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2)) == false))
            {
                return null;
            }
            return HashTableGetValue(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2));
        }

        /// <summary>
        /// 读取哈希表键值对,模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static object HashTableLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((HashTableKeyExists(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3)) == false))
            {
                return null;
            }
            return HashTableGetValue(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3));
        }

        /// <summary>
        /// 读取哈希表键值对,模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static object HashTableLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((HashTableKeyExists(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4)) == false))
            {
                return null;
            }
            return HashTableGetValue(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4));
        }

        #endregion

        #endregion

        //数据表使用过程涉及高频字符串组合或类型转换,非纯字符串组合可用ThreadStringBuilder优化.

        #region Functions 互动管理(默认使用用户快捷数据表)

        //用数据表实现不同类型数据互动、信息管理

        #region 存储区状态队列管理

        /// <summary>
        /// 存储区容错处理函数,当哈希表键值存在时执行线程等待.常用于多线程触发器频繁写值,如大量注册注销动作使存储区数据重排序的,因哈希表正在使用需排队等待完成才给执行下一个.执行原理:将调用该函数的当前线程反复挂起dataTableThreadWaitPeriod毫秒,直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        public static void ThreadWait(string key)
        {
            while (UserDataTable<bool>.Load0(true, string.Concat("MMCore_ThreadWait_", key)) == true)
            {
                Thread.Sleep(dataTableThreadWaitPeriod); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数,当哈希表键值存在时执行线程等待.常用于多线程触发器频繁写值,如大量注册注销动作使存储区数据重排序的,因哈希表正在使用需排队等待完成才给执行下一个.执行原理:将调用该函数的当前线程反复挂起period毫秒,直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="period"></param>
        public static void ThreadWait(string key, int period)
        {
            while (UserDataTable<bool>.Load0(true, string.Concat("MMCore_ThreadWait_", key)) == true)
            {
                Thread.Sleep(period); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数,引发注册注销等存储区频繁重排序的动作,在函数开始/完成写入存储区时,应设置线程等待(val=1)/闲置(val=0)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">函数动作完成,所写入存储区闲置时填false,反之填true</param>
        private static void ThreadWaitSet(string key, bool val)
        {
            UserDataTable<bool>.Save0(true, string.Concat("MMCore_ThreadWait_", key), val);
        }

        /// <summary>
        /// 存储区容错处理函数,当哈希表键值存在时执行线程等待.常用于多线程触发器频繁写值,如大量注册注销动作使存储区数据重排序的,因哈希表正在使用需排队等待完成才给执行下一个.执行原理:将调用该函数的当前线程反复挂起dataTableThreadWaitPeriod毫秒,直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        public static void ThreadWait(bool place, string key)
        {
            while (UserDataTable<bool>.Load0(place, string.Concat("MMCore_ThreadWait_", key)) == true)
            {
                Thread.Sleep(dataTableThreadWaitPeriod); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数,当哈希表键值存在时执行线程等待.常用于多线程触发器频繁写值,如大量注册注销动作使存储区数据重排序的,因哈希表正在使用需排队等待完成才给执行下一个.执行原理:将调用该函数的当前线程反复挂起period毫秒,直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="period"></param>
        public static void ThreadWait(bool place, string key, int period)
        {
            while (UserDataTable<bool>.Load0(place, string.Concat("MMCore_ThreadWait_", key)) == true)
            {
                Thread.Sleep(period); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数,引发注册注销等存储区频繁重排序的动作,在函数开始/完成写入存储区时,应设置线程等待(val=1)/闲置(val=0)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">函数动作完成,所写入存储区闲置时填false,反之填true</param>
        private static void ThreadWaitSet(bool place, string key, bool val)
        {
            UserDataTable<bool>.Save0(place, string.Concat("MMCore_ThreadWait_", key), val);
        }

        #endregion

        #region 互动函数

        #region 任意类型

        //提示:可以将本类型作为模板修改后产生其他类型
        //提示:尽可能使用对口类型,以防值类型与引用类型发生转换时拆装箱降低性能

        //--------------------------------------------------------------------------------------------------
        //任意类型组Start
        //--------------------------------------------------------------------------------------------------

        #region 给对象注册句柄,对象和句柄形成双向映射关系

        //管理对象和句柄的双重映射关系的一对字典
        private static Dictionary<object, int> objectTag = new Dictionary<object, int>();
        private static Dictionary<int, object> tagObject = new Dictionary<int, object>();
        /// <summary>
        /// Object组中最大句柄(施行永续+1的方案)
        /// </summary>
        private static int objectJBNum = 0;

        /// <summary>
        /// 互动O_注册Object标签句柄并返回,若对象已被注册则返回其句柄,本函数不会进行重复注册
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns>返回一个Object的已注册标签</returns>
        private static int HD_RegObjectTagAndReturn(object lp_object)//内部使用
        {
            // int lv_jBNum = objectJBNum;
            // if (lv_jBNum == 0 || !objectTag.ContainsKey(lp_object))
            // { //若最大句柄为0或虽最大句柄不为0但对象从未注册过,则最大句柄+1并作为对象句柄
            //     objectJBNum++; lv_jBNum++;
            //     //双向的映射关系
            //     objectTag[lp_object] = lv_jBNum;
            //     tagObject[lv_jBNum] = lp_object;
            //     return lv_jBNum;
            // }
            // else
            // { //这是一个重复注册的对象,直接返回其句柄
            //     return objectTag[lp_object];
            // }
            //使用TryGetValue避免重复查找字典
            if (objectTag.TryGetValue(lp_object, out int existingTag))
            {
                //对象已注册,直接返回其句柄
                return existingTag;
            }
            
            //新注册:最大句柄+1并作为对象句柄
            objectJBNum++;
            int newTag = objectJBNum;
            objectTag[lp_object] = newTag;
            tagObject[newTag] = lp_object;
            return newTag;
        }

        /// <summary>
        /// 互动O_返回Object已注册标签句柄,对象未注册则返回0
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns>返回一个Object的已注册标签,错误返回0</returns>
        public static int HD_ReturnObjectTag(object lp_object)
        {
            //使用TryGetValue避免重复查找字典
            if (objectTag.TryGetValue(lp_object, out int existingTag))
            {
                return existingTag;
            }
            return objectJBNum;
        }

        /// <summary>
        /// 互动O_注册Object标签句柄并返回.为Object自动设置新的标签句柄,重复时会返回已注册的Object标签.这是一个内部函数,一般不需要手动使用
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns>返回一个Object的已注册标签""</returns>
        private static string HD_RegObjectTagAndReturnStr(object lp_object)//内部使用
        {
            return HD_RegObjectTagAndReturn(lp_object).ToString();
        }

        /// <summary>
        /// 互动O_返回Object已注册标签句柄
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns>返回一个Object的已注册标签,错误返回""</returns>
        public static string HD_ReturnObjectTagStr(object lp_object)
        {
            return HD_ReturnObjectTag(lp_object).ToString();
        }

        #endregion

        /// <summary>
        /// 互动O_注册Object(高级).在指定Key存入Object,固有状态、固有自定义值是Object独一无二的标志(本函数重复注册会刷新),之后可用互动O_"返回Object注册总数"、"返回Object序号"、"返回序号对应Object"、"返回序号对应Object标签"、"返回Object自定义值".Object组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Object组转为Key.固有状态相当于单位组单位活体,如需另外设置多个标记可使用"互动O_设定Object状态/自定义值"
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        /// <param name="lp_inherentStats">固有状态</param>
        /// <param name="lp_inherentCustomValue">固有自定义值</param>
        public static void HD_RegObject(object lp_object, string lp_key, string lp_inherentStats = "true", string lp_inherentCustomValue = "")
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_tag;
            int lv_i;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            lv_tag = HD_RegObjectTagAndReturn(lp_object);
            lv_tagStr = lv_tag.ToString();
            if ((lv_num == 0))
            {
                //首次注册
                lv_i = (lv_num + 1);
                //在组中的元素数量
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                //在组中的元素位置记录句柄字符
                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                //对象句柄有效状态
                UserDataTable<bool>.Save0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                //对象在组中的注册状态
                UserDataTable<bool>.Save1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if (UserDataTable<int>.Load1(true, lv_str + "Tag", lv_i) == lv_tag)
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                //在组中的元素数量
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                //在组中的元素位置记录句柄字符
                                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                //对象句柄有效状态
                                UserDataTable<bool>.Save0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                                //对象在组中的注册状态
                                UserDataTable<bool>.Save1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
                            }
                        }
                    }
                }
            }
            UserDataTable<string>.Save0(true, ("HD_ObjectState" + "_" + lv_tagStr), lp_inherentStats);
            UserDataTable<string>.Save0(true, ("HD_ObjectCV" + "_" + lv_tagStr), lp_inherentCustomValue);
        }

        /// <summary>
        /// 互动O_注册Object.在指定Key存入Object,固有状态、固有自定义值是Object独一无二的标志(本函数重复注册不会刷新),之后可用互动O_"返回Object注册总数"、"返回Object序号"、"返回序号对应Object"、"返回Object自定义值".Object组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Object组转为Key.首次注册时固有状态自动为true(相当于单位组单位活体),之后只能通过"互动O_注册Object(高级)"改写,如需另外设置多个标记可使用"互动O_设定Object状态/自定义值"
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        public static void HD_RegObject_Simple(object lp_object, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_tag;
            int lv_i;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            lv_tag = HD_RegObjectTagAndReturn(lp_object);
            lv_tagStr = lv_tag.ToString();
            if ((lv_num == 0))
            {
                //首次注册
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                UserDataTable<bool>.Save0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                UserDataTable<bool>.Save1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if (UserDataTable<int>.Load1(true, lv_str + "Tag", lv_i) == lv_tag)
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                UserDataTable<bool>.Save0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                                UserDataTable<bool>.Save1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
                            }
                        }
                    }
                }
            }
            //从未注册过则进行首次修改为true
            if ((UserDataTable<bool>.KeyExists(true, ("HD_ObjectState" + "_" + lv_tag.ToString())) == false))
            {
                UserDataTable<string>.Save1(true, (("HD_ObjectState")), lv_tag, "true");
            }
        }

        /// <summary>
        /// 互动O_注销Object.用"互动O_注册Object"到Key,之后可用本函数彻底摧毁注册信息并将序号重排(包括Object标签有效状态、固有状态及自定义值).注册注销同时进行会排队等待0.0625s直到没有注销动作,注销并不提升多少内存只是变量内容清空并序号重利用,非特殊要求一般不注销,而是用"互动O_设定Object状态"让Object状态失效(类似单位组的单位活体状态).Object组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Object组转为Key.本函数无法摧毁用"互动O_设定Object状态/自定义值"创建的状态和自定义值,需手工填入""来排泄(非大量注销则提升内存量极小,可不管).本函数参数Key若填Object组变量ID时会清空Object组专用状态
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        public static void HD_DestroyObject(object lp_object, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_tag;
            int lv_a;
            int lv_b;
            int lv_c;
            //Variable Initialization
            lv_tag = HD_ReturnObjectTag(lp_object);
            if (lv_tag == 0) { return; } //若对象没有注册过直接返回
            lv_str = (lp_key + "HD_Object");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tagStr = lv_tag.ToString();
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((UserDataTable<int>.Load1(true, (lp_key + "HD_ObjectTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    //摧毁对象句柄有效状态
                    UserDataTable<bool>.Clear0(true, "HD_IfObjectTag_" + lv_tagStr);
                    //摧毁对象在组中的注册状态(在其他组仍可能存在,可结合对象句柄有效状态一起判断)
                    UserDataTable<bool>.Clear0(true, "IfObjectGTag" + lv_str + "_" + lv_tagStr);
                    //摧毁对象自身固有状态和固有自定义值
                    UserDataTable<string>.Clear0(true, "HD_ObjectCV_" + lv_tagStr);
                    UserDataTable<string>.Clear0(true, "HD_ObjectState_" + lv_tagStr);
                    //摧毁对象在组中的固有状态和固有自定义值
                    UserDataTable<string>.Clear0(true, "HD_ObjectCV" + lv_str + "_" + lv_tagStr);
                    UserDataTable<string>.Clear0(true, "HD_ObjectState" + lv_str + "_" + lv_tagStr);
                    //刷新组中的元素数量
                    UserDataTable<int>.Save0(true, (lp_key + "HD_ObjectNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = UserDataTable<int>.Load1(true, (lp_key + "HD_ObjectTag"), lv_b + 1);
                        UserDataTable<int>.Save1(true, (lp_key + "HD_ObjectTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列,这里-1可以让挑选回滚,以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动O_移除Object.用"互动O_注册Object"到Key,之后可用本函数仅摧毁Key区注册的信息并将序号重排,用于Object组或多个键区仅移除Object(保留Object标签有效状态、固有值).注册注销同时进行会排队等待0.0625s直到没有注销动作,注销并不提升多少内存只是变量内容清空并序号重利用,非特殊要求一般不注销,而是用"互动O_设定Object状态"让Object状态失效(类似单位组的单位活体状态).Object组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Object组转为Key.本函数无法摧毁用"互动O_设定Object状态/自定义值"创建的状态和自定义值,需手工填入""来排泄(非大量注销则提升内存量极小,可不管).本函数参数Key若填Object组变量ID时会清空Object组专用状态
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        public static void HD_RemoveObject(object lp_object, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_tag;
            int lv_a;
            int lv_b;
            int lv_c;
            //Variable Initialization
            lv_tag = HD_ReturnObjectTag(lp_object);
            if (lv_tag == 0) { return; } //若对象没有注册过直接返回
            lv_str = (lp_key + "HD_Object");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tagStr = lv_tag.ToString();
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((UserDataTable<int>.Load1(true, (lp_key + "HD_ObjectTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    //摧毁对象在组中的注册状态(在其他组仍可能存在,可结合对象句柄有效状态一起判断)
                    UserDataTable<bool>.Clear0(true, "IfObjectGTag" + lv_str + "_" + lv_tagStr);
                    //摧毁对象自身固有状态和固有自定义值
                    UserDataTable<string>.Clear0(true, "HD_ObjectCV" + lv_str + "_" + lv_tagStr);
                    UserDataTable<string>.Clear0(true, "HD_ObjectState" + lv_str + "_" + lv_tagStr);
                    //刷新组中的元素数量
                    UserDataTable<int>.Save0(true, (lp_key + "HD_ObjectNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = UserDataTable<int>.Load1(true, (lp_key + "HD_ObjectTag"), lv_b + 1);
                        UserDataTable<int>.Save1(true, (lp_key + "HD_ObjectTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列,这里-1可以让挑选回滚,以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动O_返回Object注册总数.必须先使用"互动O_注册Object"才能返回指定Key里的注册总数.Object组使用时,可用"获取变量的内部名称"将Object组转为Key.
        /// </summary>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        /// <returns></returns>
        public static int HD_ReturnObjectNumMax(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            //Implementation
            return lv_num;
        }

        /// <summary>
        /// 互动O_返回Object序号.使用"互动O_注册Object"后使用本函数可返回Key里的注册序号,Key无元素返回0,Key有元素但对象不在里面则返回-1,Object标签尚未注册则返回-2.Object组使用时,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        /// <returns>若对象没有注册过直接返回0</returns>
        public static int HD_ReturnObjectNum(object lp_object, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_i;
            int lv_tag;
            int lv_torf;
            //Automatic Variable Declarations
            const int auto_n = 1;
            int auto_i;
            int auto_ae;
            int auto_var;
            //Variable Initialization
            lv_tag = HD_ReturnObjectTag(lp_object);
            if (lv_tag == 0) { return 0; } //若对象没有注册过直接返回0
            lv_str = (lp_key + "HD_Object");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_torf = -1;
            //Implementation
            for (auto_i = 1; auto_i <= auto_n; auto_i += 1)
            {
                if ((lv_num == 0))
                {
                    lv_torf = 0;
                }
                else
                {
                    if ((lv_num >= 1))
                    {
                        auto_ae = lv_num;
                        auto_var = 1;
                        for (; auto_var <= auto_ae; auto_var += 1)
                        {
                            lv_i = auto_var;
                            if ((UserDataTable<int>.Load1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                            {
                                lv_torf = lv_i;
                                break;
                            }
                        }
                    }
                }
            }
            return lv_torf;
        }

        /// <summary>
        /// 互动O_返回序号对应Object.使用"互动O_注册Object"后,在参数填入注册序号可返回Object.Object组使用时,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        /// <returns></returns>
        public static object HD_ReturnObjectFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tag = UserDataTable<int>.Load1(true, (lv_str + "Tag"), lp_regNum);
            //Implementation
            return HD_ReturnObjectFromTag(lv_tag);
        }

        /// <summary>
        /// 互动O_返回句柄标签对应Object.使用"互动O_注册Object"后,在参数填入句柄标签(整数)可返回Object,标签是Object的句柄.Object组使用时,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_tag">句柄标签</param>
        /// <returns></returns>
        public static object HD_ReturnObjectFromTag(int lp_tag)
        {

            if (tagObject.ContainsKey(lp_tag))
            {
                //键存在,可以安全地访问
                return tagObject[lp_tag];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 互动O_返回序号对应Object标签句柄.使用"互动O_注册Object"后,在参数填入注册序号可返回Object标签(字符串).Object组使用时,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        /// <returns></returns>
        public static string HD_ReturnObjectTagFromRegNumStr(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            string lv_tagStr;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tagStr = UserDataTable<int>.Load1(true, (lv_str + "Tag"), lp_regNum).ToString();
            //Implementation
            return lv_tagStr;
        }

        /// <summary>
        /// 互动O_返回序号对应Object标签句柄.使用"互动O_注册Object"后,在参数填入注册序号可返回Object标签(整数).Object组使用时,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        /// <returns></returns>
        public static int HD_ReturnObjectTagFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tag = UserDataTable<int>.Load1(true, (lv_str + "Tag"), lp_regNum);
            //Implementation
            return lv_tag;
        }

        /// <summary>
        /// 互动O_设置Object状态.必须先"注册"获得功能库内部句柄,再使用本函数给Object设定一个状态值,之后可用"互动O_返回Object状态".类型参数用以记录多个不同状态,仅当"类型"参数填Object组ID转的Object串时,状态值"true"和"false"是Object的Object组专用状态值,用于内部函数筛选Object状态(相当于单位组单位索引是否有效),其他类型不会干扰系统内部,可随意填写.虽然注销时反向清空注册信息,但用"互动O_设定Object状态/自定义值"创建的值需要手工填入""来排泄(非大量注销则提升内存量极小,可不管).注:固有状态值是注册函数赋予的系统内部变量(相当于单位组单位是否活体),只能通过"互动O_注册Object(高级)"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型,可填写任意状态标记如"State"</param>
        /// <param name="lp_stats">状态</param>
        public static void HD_SetObjectState(object lp_object, string lp_key, string lp_stats)
        {
            //Variable Declarations
            string lv_str;
            string lv_tagStr;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            //Implementation
            UserDataTable<string>.Save0(true, ("State" + lv_str + "_" + lv_tagStr), lp_stats);
        }

        /// <summary>
        /// 互动O_返回Object状态.使用"互动O_设定Object状态"后可使用本函数,将本函数参数"类型"设为空时返回固有值.类型参数用以记录多个不同状态,仅当"类型"参数为Object组ID转的字符串时,返回的状态值"true"和"false"是Object的Object组专用状态值,用于内部函数筛选Object状态(相当于单位组单位索引是否有效)
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型,可填写任意状态标记如"State"</param>
        /// <returns></returns>
        public static string HD_ReturnObjectState(object lp_object, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            string lv_tagStr;
            string lv_stats;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            lv_stats = UserDataTable<string>.Load0(true, ("State" + lv_str + "_" + lv_tagStr));
            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动O_设置Object自定义值.必须先"注册"获得功能库内部句柄,再使用本函数设定Object的自定义值,之后可使用"互动O_返回Object自定义值",类型参数用以记录多个不同自定义值.注:固有自定义值是注册函数赋予的系统内部变量,只能通过"互动O_注册Object(高级)"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型,可填写任意自定义值标记如"A"</param>
        /// <param name="lp_customValue">自定义值</param>
        public static void HD_SetObjectCV(object lp_object, string lp_key, string lp_customValue)
        {
            //Variable Declarations
            string lv_str;
            string lv_tagStr;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            //Implementation
            UserDataTable<string>.Save0(true, ("CV" + lv_str + "_" + lv_tagStr), lp_customValue);
        }

        /// <summary>
        /// 互动O_返回Object自定义值.使用"互动O_设定Object自定义值"后可使用本函数,将本函数参数"类型"设为空时返回固有值,该参数用以记录多个不同自定义值
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型,可填写任意自定义值标记如"A"</param>
        /// <returns></returns>
        public static string HD_ReturnObjectCV(object lp_object, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            string lv_tagStr;
            string lv_customValue;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            lv_customValue = UserDataTable<string>.Load0(true, ("CV" + lv_str + "_" + lv_tagStr));
            //Implementation
            return lv_customValue;
        }

        /// <summary>
        /// 互动O_返回Object固有状态.必须先使用"互动O_注册Object"才能返回到该值,固有状态是独一无二的标记(相当于单位组单位是否活体)
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns></returns>
        public static string HD_ReturnObjectState_Only(object lp_object)
        {
            //Variable Declarations
            string lv_tagStr;
            string lv_stats;
            //Variable Initialization
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            lv_stats = UserDataTable<string>.Load0(true, ("HD_ObjectState" + "_" + lv_tagStr));
            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动O_返回Object固有自定义值.必须先使用"互动O_注册Object"才能返回到该值,固有值是独一无二的标记
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns></returns>
        public static string HD_ReturnObjectCV_Only(object lp_object)
        {
            //Variable Declarations
            string lv_tagStr;
            string lv_customValue;
            //Variable Initialization
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            lv_customValue = UserDataTable<string>.Load0(true, ("HD_ObjectCV" + "_" + lv_tagStr));
            //Implementation
            return lv_customValue;
        }

        /// <summary>
        /// 互动O_设置Object的实数标记.必须先"注册"获得功能库内部句柄,再使用本函数让Object携带一个实数值,之后可使用"互动O_返回Object的实数标记".Object组使用时,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_realNumTag">实数标记</param>
        public static void HD_SetObjectDouble(object lp_object, double lp_realNumTag)
        {
            //Variable Declarations
            string lv_tagStr;
            //Variable Initialization
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            //Implementation
            UserDataTable<double>.Save0(true, ("HD_CDDouble_Object_" + lv_tagStr), lp_realNumTag);
        }

        /// <summary>
        /// 互动O_返回Object的实数标记.使用"互动O_设定Object的实数标记"后可使用本函数.Object组使用时,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns></returns>
        public static double HD_ReturnObjectDouble(object lp_object)
        {
            //Variable Declarations
            string lv_tagStr;
            //Variable Initialization
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            //Implementation
            return UserDataTable<double>.Load0(true, ("HD_CDDouble_Object_" + lv_tagStr));
        }

        /// <summary>
        /// 互动O_返回Object标签句柄有效状态.将Object视作独一无二的个体,标签是它本身,有效状态则类似"单位是否有效",当使用"互动O_注册Object"或"互动OG_添加Object到Object组"后激活Object有效状态(值为"true"),除非使用"互动O_注册Object(高级)"改写,否则直到注销才会摧毁
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns></returns>
        public static bool HD_ReturnIfObjectTag(object lp_object)
        {
            //Variable Declarations
            string lv_tagStr;
            //Variable Initialization
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            //Implementation
            return UserDataTable<bool>.Load0(true, ("HD_IfObjectTag" + "_" + lv_tagStr));
        }

        /// <summary>
        /// 互动O_返回Object注册状态.使用"互动O_注册Object"或"互动OG_添加Object到Object组"后可使用本函数获取注册Object在Key中的注册状态,该状态只能注销或从Object组中移除时清空.Object组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区,默认值"_Object"</param>
        /// <returns></returns>
        public static bool HD_ReturnIfObjectTagKey(object lp_object, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            string lv_tagStr;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tagStr = HD_ReturnObjectTag(lp_object).ToString();
            //Implementation
            return UserDataTable<bool>.Load0(true, ("IfObjectGTag" + lv_str + "_" + lv_tagStr));
        }

        /// <summary>
        /// 互动OG_根据自定义值类型将Object组排序.根据Object携带的自定义值类型,对指定的Object组元素进行冒泡排序.Object组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区,默认填Object组名称</param>
        /// <param name="lp_cVStr">自定义值类型(要求自定义值是数字)</param>
        /// <param name="lp_big">是否大值靠前</param>
        public static void HD_ObjectGSortCV(string lp_key, string lp_cVStr, bool lp_big)
        {
            //Variable Declarations
            int lv_a;
            int lv_b;
            int lv_c;
            bool lv_bool;
            int lv_tag;
            int lv_tagValue;
            string lv_str;
            int lv_num;
            int lv_intStackOutSize;
            string lv_tagValuestr;
            //Automatic Variable Declarations
            int autoB_ae;
            const int autoB_ai = 1;
            int autoC_ae;
            const int autoC_ai = 1;
            int autoHD_ae;
            const int autoHD_ai = -1;
            int autoE_ae;
            const int autoE_ai = 1;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_intStackOutSize = 0;
            //Implementation
            autoB_ae = lv_num;
            lv_a = 1;
            for (; ((autoB_ai >= 0 && lv_a <= autoB_ae) || (autoB_ai < 0 && lv_a >= autoB_ae)); lv_a += autoB_ai)
            {
                lv_tag = HD_ReturnObjectTagFromRegNum(lv_a, lp_key);
                lv_tagValuestr = HD_ReturnObjectCV(HD_ReturnObjectFromTag(lv_tag), lp_cVStr);
                lv_tagValue = Convert.ToInt32(lv_tagValuestr);
                //Console.WriteLine("循环" + IntToString(lv_a) +"tag"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue));
                if ((lv_intStackOutSize == 0))
                {
                    lv_intStackOutSize += 1;
                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", 1, lv_tag);
                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", 1, lv_tagValue);
                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+",IteraOrig="+IntToString(lv_a));
                }
                else
                {
                    lv_bool = false;
                    autoC_ae = lv_intStackOutSize;
                    lv_b = 1;
                    //Console.WriteLine("For" + IntToString(1) +"到"+IntToString(autoC_ae));
                    for (; ((autoC_ai >= 0 && lv_b <= autoC_ae) || (autoC_ai < 0 && lv_b >= autoC_ae)); lv_b += autoC_ai)
                    {
                        if (lp_big == false)
                        {
                            //Console.WriteLine("小值靠前");
                            if (lv_tagValue < UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+",当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnObjectTagFromRegNum(lv_b, lp_key))+"值"+IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b:" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize:" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae:" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c:" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄,让元素交换
                //lv_tagStr = UserDataTable<int>.Load1(true, (lp_key + "HD_ObjectTag"), lv_a).ToString(); //原始序号元素
                lv_tag = UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", lv_a);
                lv_tagValuestr = HD_ReturnObjectCV(HD_ReturnObjectFromTag(lv_tag), lp_cVStr);
                lv_tagValue = Convert.ToInt32(lv_tagValuestr);
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr));
                lv_b = UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号:" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a:"+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    UserDataTable<int>.Save1(true, (lp_key + "HD_ObjectTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                      //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动OG_Object组排序.对指定的Object组元素进行冒泡排序(根据元素句柄).Object组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区,默认填Object组名称</param>
        /// <param name="lp_big">是否大值靠前</param>
        public static void HD_ObjectGSort(string lp_key, bool lp_big)
        {
            //Automatic Variable Declarations
            //Implementation
            //Variable Declarations
            int lv_a;
            int lv_b;
            int lv_c;
            bool lv_bool;
            int lv_tag;
            int lv_tagValue;
            string lv_str;
            int lv_num;
            int lv_intStackOutSize;
            //Automatic Variable Declarations
            int autoB_ae;
            const int autoB_ai = 1;
            int autoC_ae;
            const int autoC_ai = 1;
            int autoHD_ae;
            const int autoHD_ai = -1;
            int autoE_ae;
            const int autoE_ai = 1;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_intStackOutSize = 0;
            //Implementation
            autoB_ae = lv_num;
            lv_a = 1;
            for (; ((autoB_ai >= 0 && lv_a <= autoB_ae) || (autoB_ai < 0 && lv_a >= autoB_ae)); lv_a += autoB_ai)
            {
                lv_tag = HD_ReturnObjectTagFromRegNum(lv_a, lp_key);
                lv_tagValue = lv_tag;
                //Console.WriteLine("循环" + IntToString(lv_a) +"tag"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue));
                if ((lv_intStackOutSize == 0))
                {
                    lv_intStackOutSize += 1;
                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", 1, lv_tag);
                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", 1, lv_tagValue);
                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+",IteraOrig="+IntToString(lv_a));
                }
                else
                {
                    lv_bool = false;
                    autoC_ae = lv_intStackOutSize;
                    lv_b = 1;
                    //Console.WriteLine("For" + IntToString(1) +"到"+IntToString(autoC_ae));
                    for (; ((autoC_ai >= 0 && lv_b <= autoC_ae) || (autoC_ai < 0 && lv_b >= autoC_ae)); lv_b += autoC_ai)
                    {
                        if (lp_big == false)
                        {
                            //Console.WriteLine("小值靠前");
                            if (lv_tagValue < UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+",当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnObjectTagFromRegNum(lv_b, lp_key))+"值"+IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b:" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize:" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae:" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c:" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        UserDataTable<int>.Save1(false, "HD_ObjStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_ObjStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_ObjStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄,让元素交换
                //lv_tagStr = UserDataTable<int>.Load1(true, (lp_key + "HD_ObjectTag"), lv_a).ToString(); //原始序号元素
                lv_tag = UserDataTable<int>.Load1(false, "HD_ObjStackOutTag", lv_a);
                lv_tagValue = lv_tag;
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr));
                lv_b = UserDataTable<int>.Load1(false, "HD_ObjStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号:" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a:"+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    UserDataTable<int>.Save1(true, (lp_key + "HD_ObjectTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                      //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动OG_设定Object的Object组专用状态.给Object组的Object设定一个状态值(字符串),之后可用"互动O_返回Object、互动OG_返回Object组的Object状态".状态值"true"和"false"是Object的Object组专用状态值,用于内部函数筛选字符状态(相当于单位组单位索引是否有效),而本函数可以重设干预,影响函数"互动OG_返回Object组元素数量(仅检索XX状态)".与"互动O_设定Object状态"功能相同,只是状态参数在Object组中被固定为"Object组变量的内部ID".Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_objectGroup"></param>
        /// <param name="lp_groupState"></param>
        public static void HD_SetObjectGState(object lp_object, string lp_objectGroup, string lp_groupState)
        {
            HD_SetObjectState(lp_object, lp_objectGroup, lp_groupState);
        }

        /// <summary>
        /// 互动OG_返回Object的Object组专用状态.使用"互动O_设定Object、互动OG_设定Object组的Object状态"后可使用本函数.与"互动O_返回Object状态"功能相同,只是状态参数在Object组中被固定为"Object组变量的内部ID".状态值"true"和"false"是Object的Object组专用状态值,用于内部函数筛选字符状态(相当于单位组单位索引是否有效).Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_objectGroup"></param>
        public static void HD_ReturnObjectGState(object lp_object, string lp_objectGroup)
        {
            HD_ReturnObjectState(lp_object, lp_objectGroup);
        }

        /// <summary>
        /// 互动OG_返回Object组元素序号对应元素.返回Object组元素序号指定Object.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static object HD_ReturnObjectFromObjectGFunc(int lp_regNum, string lp_gs)
        {
            return HD_ReturnObjectFromRegNum(lp_regNum, lp_gs);
        }

        /// <summary>
        /// 互动OG_返回Object组元素总数.返回指定Object组的元素数量.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns>错误时返回0</returns>
        public static int HD_ReturnObjectGNumMax(string lp_gs)
        {
            return UserDataTable<int>.Load0(true, lp_gs + "HD_ObjectNum");
        }

        /// <summary>
        /// 互动OG_返回Object组元素总数(仅检测Object组专用状态="true").返回指定Object组的元素数量.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnObjectGNumMax_StateTrueFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            object lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnObjectNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnObjectFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnObjectState(lv_c, lp_gs);
                if ((lv_b == "true"))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动OG_返回Object组元素总数(仅检测Object组专用状态="false").返回指定Object组的元素数量.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnObjectGNumMax_StateFalseFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            object lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnObjectNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnObjectFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnObjectState(lv_c, lp_gs);
                if ((lv_b == "false"))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动OG_返回Object组元素总数(仅检测Object组无效专用状态:"false"或"").返回指定Object组的元素数量(false、""、null).Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnObjectGNumMax_StateUselessFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            object lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnObjectNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnObjectFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnObjectState(lv_c, lp_gs);
                if (((lv_b == "false") || (lv_b == "") || (lv_b == null)))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动OG_返回Object组元素总数(仅检测Object组指定专用状态).返回指定Object组的元素数量.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_State">Object组专用状态</param>
        /// <returns></returns>
        public static int HD_ReturnObjectGNumMax_StateFunc_Specify(string lp_gs, string lp_State)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            object lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnObjectNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnObjectFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnObjectState(lv_c, lp_gs);
                if ((lv_b == lp_State))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动OG_添加Object到Object组.相同Object被认为是同一个,非高级功能不提供专用状态检查,若Object没有设置过Object组专用状态,那么首次添加到Object组不会赋予"true"(之后可通过"互动O_设定Object状态"、"互动OG_设定Object组的Object状态"修改).Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_AddObjectToGroup_Simple(object lp_object, string lp_gs)
        {
            HD_RegObject_Simple(lp_object, lp_gs);
            //Simple方法没有组专用状态
        }

        /// <summary>
        /// 互动OG_添加Object到Object组(高级).相同Object被认为是同一个,高级功能提供专用状态检查,若Object没有设置过Object组专用状态,那么首次添加到Object组会赋予"true"(之后可通过"互动O_设定Object状态"、"互动OG_设定Object组的Object状态"修改).Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_AddObjectToGroup(object lp_object, string lp_gs)
        {
            //组中添加对象,不对其固有状态和固有自定义值进行任何修改,所以使用Simple
            HD_RegObject_Simple(lp_object, lp_gs);
            //高级方法设置组专用状态
            if (UserDataTable<string>.KeyExists(true, ("State" + lp_gs + "HD_Object_" + HD_RegObjectTagAndReturnStr(lp_object))) == false)
            {
                UserDataTable<string>.Save0(true, ("State" + lp_gs + "HD_Object_" + HD_RegObjectTagAndReturnStr(lp_object)), "true");
                //Console.WriteLine(lp_gs + "=>" + HD_RegObjectTagAndReturnStr(lp_object));
            }
        }

        /// <summary>
        /// 互动OG_移除Object组中的元素.使用"互动OG_添加Object到Object组"后可使用本函数进行移除元素.移除使用了"互动O_移除Object",同一个存储区(Object组ID)序号重排,移除时该存储区如有其他操作会排队等待.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_RemoveObjectFromGroup(object lp_object, string lp_gs)
        {
            HD_RemoveObject(lp_object, lp_gs);
        }

        //互动OG_为Object组中的每个序号
        //GE(星际2的Galaxy Editor)的宏让编辑器保存时自动生成脚本并整合进脚本进行格式调整,C#仅参考需自行编写
        //#AUTOVAR(vs, string) = "#PARAM(group)";//"#PARAM(group)"是与字段、变量名一致的元素组名称,宏去声明string类型名为“Auto随机编号_vs”的自动变量,然后=右侧字符
        //#AUTOVAR(ae) = HD_ReturnObjectNumMax(#AUTOVAR(vs));//宏去声明默认int类型名为“Auto随机编号_ae”的自动变量,然后=右侧字符
        //#INITAUTOVAR(ai,increment)//宏去声明int类型名为“Auto随机编号_ai”的自动变量,用于下面for循环增量(increment是传入参数)
        //#PARAM(var) = #PARAM(s);//#PARAM(var)是传进来的参数,用作“当前被挑选到的元素”(任意变量-整数 lp_var), #PARAM(s)是传进来的参数用作"开始"(int lp_s)
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #PARAM(var) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #PARAM(var) >= #AUTOVAR(ae)) ) ; #PARAM(var) += #AUTOVAR(ai) ) {
        //    #SUBFUNCS(actions)//代表用户GUI填写的所有动作
        //}

        /// <summary>
        /// 互动OG_为Object组中的每个序号.每次挑选的元素序号会自行在动作组(委托函数)中使用,委托函数特征:void SubActionTest(int lp_var),参数lp_var即每次遍历到的元素序号,请自行组织它在委托函数内如何使用,SubActionTest可直接作为本函数最后一个参数填入,填入多个动作范例:SubVActionEventFuncref Actions += SubActionTest,然后Actions作为参数填入.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_start">开始</param>
        /// <param name="lp_increment">增量</param>
        /// <param name="lp_funcref">委托类型变量或函数引用</param>
        public static void HD_ForEachObjectNumFromGroup(string lp_gs, int lp_start, int lp_increment, SubVActionEventFuncref lp_funcref)
        {
            int lv_ae = HD_ReturnObjectNumMax(lp_gs);
            int lv_var = lp_start;
            int lv_ai = lp_increment;
            for (; (lv_ai >= 0 && lv_var <= lv_ae) || (lv_ai < 0 && lv_var >= lv_ae); lv_var += lv_ai)
            {
                lp_funcref(lv_var);//用户填写的所有动作
            }
        }

        //互动OG_为Object组中的每个元素
        //#AUTOVAR(vs, string) = "#PARAM(group)";
        //#AUTOVAR(ae) = HD_ReturnObjectNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= #PARAM(s);
        //#INITAUTOVAR(ai,increment)
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    DataTableSave(false, "ObjectGFor"+ #AUTOVAR(vs) + IntToString(#AUTOVAR(va)), HD_ReturnObjectFromRegNum(#AUTOVAR(va),#AUTOVAR(vs)));
        //}
        //#AUTOVAR(va)= #PARAM(s);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #PARAM(var) = DataTableLoad(false, "ObjectGFor"+ #AUTOVAR(vs) + IntToString(#AUTOVAR(va)));
        //    #SUBFUNCS(actions)
        //}

        /// <summary>
        /// 互动OG_为Object组中的每个元素.每次挑选的元素会自行在动作组(委托函数)中使用,委托函数特征:void SubOActionEventFuncref(object lv_object),参数lv_object即每次遍历到的元素,请自行组织它在委托函数内如何使用,SubActionTest可直接作为本函数最后一个参数填入,填入多个动作范例:SubVActionEventFuncref Actions += SubActionTest,然后Actions作为参数填入.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_start">开始</param>
        /// <param name="lp_increment">增量</param>
        /// <param name="lp_funcref">委托类型变量或函数引用</param>
        public static void HD_ForEachObjectFromGroup(string lp_gs, int lp_start, int lp_increment, SubOActionEventFuncref lp_funcref)
        {
            string lv_vs = lp_gs;
            int lv_ae = HD_ReturnObjectNumMax(lv_vs);
            int lv_va = lp_start;
            int lv_ai = lp_increment;
            object lv_object;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                UserDataTable<object>.Save0(false, "HD_ObjectGFor" + lv_vs + lv_va.ToString(), HD_ReturnObjectFromRegNum(lv_va, lv_vs));
            }
            lv_va = lp_start;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                lv_object = UserDataTable<object>.Load0(false, "HD_ObjectGFor" + lv_vs + lv_va.ToString());
                lp_funcref(lv_object);//用户填写的所有动作
            }
        }

        /// <summary>
        /// 互动OG_返回Object组中随机元素.返回指定Object组中的随机Object.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static object HD_ReturnRandomObjectFromObjectGFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_num;
            int lv_a;
            object lv_c = null;
            //Variable Initialization
            lv_num = HD_ReturnObjectNumMax(lp_gs);
            //Implementation
            if ((lv_num >= 1))
            {
                lv_a = RandomInt(1, lv_num);
                lv_c = HD_ReturnObjectFromRegNum(lv_a, lp_gs);
            }
            return lv_c;
        }

        //互动OG_添加Object组到Object组
        //#AUTOVAR(vs, string) = "#PARAM(groupA)";
        //#AUTOVAR(vsb, string) = "#PARAM(groupB)";
        //#AUTOVAR(ae) = HD_ReturnObjectNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= 1;
        //#AUTOVAR(ai)= 1;
        //#AUTOVAR(var);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #AUTOVAR(var) = HD_ReturnObjectFromRegNum(#AUTOVAR(va), #AUTOVAR(vs));
        //    HD_AddObjectToGroup(#AUTOVAR(var), #AUTOVAR(vsb));
        //}

        /// <summary>
        /// 互动OG_添加Object组到Object组.添加一个Object组A的元素到另一个Object组B,相同Object被认为是同一个.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_groupA"></param>
        /// <param name="lp_groupB"></param>
        public static void HD_AddObjectGToObjectG(string lp_groupA, string lp_groupB)
        {
            string lv_vsa = lp_groupA;
            string lv_vsb = lp_groupB;
            int lv_ae = HD_ReturnObjectNumMax(lv_vsa);
            int lv_va = 1;
            int lv_ai = 1;
            object lv_var;
            for (; ((lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae)); lv_va += lv_ai)
            {
                lv_var = HD_ReturnObjectFromRegNum(lv_va, lv_vsa);
                HD_AddObjectToGroup(lv_var, lv_vsb);
            }
        }

        //互动OG_从Object组移除Object组
        //#AUTOVAR(vs, string) = "#PARAM(groupA)";
        //#AUTOVAR(vsb, string) = "#PARAM(groupB)";
        //#AUTOVAR(ae) = HD_ReturnObjectNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= 1;
        //#AUTOVAR(ai)= 1;
        //#AUTOVAR(var);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #AUTOVAR(var) = HD_ReturnObjectFromRegNum(#AUTOVAR(va), #AUTOVAR(vs));
        //    HD_RemoveObject(#AUTOVAR(var), #AUTOVAR(vsb));
        //}

        /// <summary>
        /// 互动OG_从Object组移除Object组.将Object组A的元素从Object组B中移除,相同Object被认为是同一个.移除使用了"互动O_移除Object",同一个存储区(Object组ID)序号重排,移除时该存储区如有其他操作会排队等待.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_groupA"></param>
        /// <param name="lp_groupB"></param>
        public static void HD_ClearObjectGFromObjectG(string lp_groupA, string lp_groupB)
        {
            string lv_vsa = lp_groupA;
            string lv_vsb = lp_groupB;
            int lv_ae = HD_ReturnObjectNumMax(lv_vsa);
            int lv_va = 1;
            int lv_ai = 1;
            object lv_var;
            for (; ((lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae)); lv_va += lv_ai)
            {
                lv_var = HD_ReturnObjectFromRegNum(lv_va, lv_vsa);
                HD_RemoveObject(lv_var, lv_vsb);
            }
        }

        /// <summary>
        /// 互动OG_移除Object组全部元素.将Object组(Key区)存储的元素全部移除,相同Object被认为是同一个.移除时同一个存储区(Object组ID)序号不进行重排,但该存储区如有其他操作会排队等待.Object组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_key">存储键区,默认填Object组名称</param>
        public static void HD_RemoveObjectGAll(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_a;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                lv_tagStr = UserDataTable<int>.Load1(true, (lp_key + "HD_ObjectTag"), lv_a).ToString();
                lv_num -= 1;
                UserDataTable<bool>.Clear0(true, "IfObjectGTag" + lv_str + "_" + lv_tagStr);
                UserDataTable<string>.Clear0(true, "HD_ObjectCV" + lv_str + "_" + lv_tagStr);
                UserDataTable<string>.Clear0(true, "HD_ObjectState" + lv_str + "_" + lv_tagStr);
                UserDataTable<int>.Save0(true, (lp_key + "HD_ObjectNum"), lv_num);
            }
            ThreadWaitSet(true, lv_str, false);
        }

        //--------------------------------------------------------------------------------------------------
        //任意类型组End
        //--------------------------------------------------------------------------------------------------

        #endregion

        #region 字符串

        //提示:尽可能使用对口类型,以防值类型与引用类型发生转换时拆装箱降低性能

        //--------------------------------------------------------------------------------------------------
        //字符串组Start
        //--------------------------------------------------------------------------------------------------
        //设计方案:字符串的句柄就是它自己

        /// <summary>
        /// 互动S_注册String(高级).在指定Key存入String,固有状态、固有自定义值是String独一无二的标志(本函数重复注册会刷新),之后可用互动S_"返回String注册总数"、"返回String序号"、"返回序号对应String"、"返回序号对应String标签"、"返回String自定义值".String组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将String组转为Key.首次注册时固有状态为true(相当于单位组单位活体),如需另外设置多个标记可使用"互动S_设定String状态/自定义值"
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        /// <param name="lp_inherentStats">固有状态</param>
        /// <param name="lp_inherentCustomValue">固有自定义值</param>
        public static void HD_RegString(string lp_string, string lp_key, string lp_inherentStats = "true", string lp_inherentCustomValue = "")
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_i;
            string lv_tag;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;

            //Variable Initialization
            lv_str = (lp_key + "HD_String");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_string;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<string>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                UserDataTable<bool>.Save0(true, (("HD_IfStringTag") + "_" + lv_tag), true); //保存标签的有效状态
                UserDataTable<bool>.Save0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true); //保存标签在区域的已注册状态
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if ((UserDataTable<string>.Load1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                UserDataTable<string>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                UserDataTable<bool>.Save0(true, (("HD_IfStringTag") + "_" + lv_tag), true);
                                UserDataTable<bool>.Save0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true);
                            }

                        }
                    }
                }

            }
            UserDataTable<string>.Save0(true, ("HD_StringState" + "_" + lv_tag), lp_inherentStats);
            UserDataTable<string>.Save0(true, ("HD_StringCV" + "_" + lv_tag), lp_inherentCustomValue);
        }

        /// <summary>
        /// 互动S_注册String.在指定Key存入String,固有状态、固有自定义值是String独一无二的标志(本函数重复注册不会刷新),之后可用互动S_"返回String注册总数"、"返回String序号"、"返回序号对应String"、"返回String自定义值".String组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将String组转为Key.首次注册时固有状态为true(相当于单位组单位活体),之后只能通过"互动S_注册String(高级)"改写,如需另外设置多个标记可使用"互动S_设定String状态/自定义值"
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        public static void HD_RegString_Simple(string lp_string, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_i;
            string lv_tag;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;

            //Variable Initialization
            lv_str = (lp_key + "HD_String");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_string;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<string>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                UserDataTable<bool>.Save0(true, (("HD_IfStringTag") + "_" + lv_tag), true);
                UserDataTable<bool>.Save0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if ((UserDataTable<string>.Load1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                UserDataTable<string>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                UserDataTable<bool>.Save0(true, (("HD_IfStringTag") + "_" + lv_tag), true);
                                UserDataTable<bool>.Save0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true);
                            }

                        }
                    }
                }
            }
            //从未注册过则进行首次修改为true
            if ((UserDataTable<bool>.KeyExists(true, ("HD_StringState" + "_" + lv_tag)) == false))
            {
                UserDataTable<string>.Save0(true, ("HD_StringState" + "_" + lv_tag), "true");
            }
        }

        /// <summary>
        /// 互动S_注销String.用"互动S_注册String"到Key,之后可用本函数彻底摧毁注册信息并将序号重排(包括String标签有效状态、固有状态及自定义值).注册注销同时进行会排队等待0.0625s直到没有注销动作,注销并不提升多少内存只是变量内容清空并序号重利用,非特殊要求一般不注销,而是用"互动S_设定String状态"让String状态失效(类似单位组的单位活体状态).String组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将String组转为Key.本函数无法摧毁用"互动S_设定String状态/自定义值"创建的状态和自定义值,需手工填入""来排泄(非大量注销则提升内存量极小,可不管).本函数参数Key若填String组变量ID时会清空String组专用状态
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        public static void HD_DestroyString(string lp_string, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tag;
            int lv_a;
            int lv_b;
            string lv_c;
            //Variable Initialization
            lv_str = (lp_key + "HD_String");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_string;
            //Implementation
            if ((lv_tag != null))
            {
                ThreadWait(lv_str);
                ThreadWaitSet(true, lv_str, true);
                for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
                {
                    if ((UserDataTable<string>.Load1(true, (lv_str + "Tag"), lv_a) == lv_tag))
                    {
                        lv_num -= 1;
                        UserDataTable<bool>.Clear0(true, "HD_IfStringTag_" + lv_tag);
                        UserDataTable<bool>.Clear0(true, "IfStringGTag" + lv_str + "_" + lv_tag);
                        UserDataTable<string>.Clear0(true, "HD_StringCV_" + lv_tag);
                        UserDataTable<string>.Clear0(true, "HD_StringState_" + lv_tag);
                        UserDataTable<string>.Clear0(true, "HD_StringCV" + lv_str + "_" + lv_tag);
                        UserDataTable<string>.Clear0(true, "HD_StringState" + lv_str + "_" + lv_tag);
                        UserDataTable<int>.Save0(true, (lp_key + "HD_StringNum"), lv_num);
                        for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                        {
                            lv_c = UserDataTable<string>.Load1(true, (lp_key + "HD_StringTag"), lv_b + 1);
                            UserDataTable<string>.Save1(true, (lp_key + "HD_StringTag"), lv_b, lv_c);
                        }
                        //注销后触发序号重列,这里-1可以让挑选回滚,以再次检查重排后的当前挑选序号
                        lv_a -= 1;
                    }
                }
                ThreadWaitSet(true, lv_str, false);
            }
        }

        /// <summary>
        /// 互动S_移除String.用"互动S_注册String"到Key,之后可用本函数仅摧毁Key区注册的信息并将序号重排,用于String组或多个键区仅移除String(保留String标签有效状态、固有值).注册注销同时进行会排队等待0.0625s直到没有注销动作,注销并不提升多少内存只是变量内容清空并序号重利用,非特殊要求一般不注销,而是用"互动S_设定String状态"让String状态失效(类似单位组的单位活体状态).String组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将String组转为Key.本函数无法摧毁用"互动S_设定String状态/自定义值"创建的状态和自定义值,需手工填入""来排泄(非大量注销则提升内存量极小,可不管).本函数参数Key若填String组变量ID时会清空String组专用状态
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        public static void HD_RemoveString(string lp_string, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tag;
            int lv_a;
            int lv_b;
            string lv_c;

            //Variable Initialization
            lv_str = (lp_key + "HD_String");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_string;

            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((UserDataTable<string>.Load1(true, (lp_key + "HD_StringTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    UserDataTable<bool>.Clear0(true, "IfStringGTag" + lv_str + "_" + lv_tag);
                    UserDataTable<string>.Clear0(true, "HD_StringCV" + lv_str + "_" + lv_tag);
                    UserDataTable<string>.Clear0(true, "HD_StringState" + lv_str + "_" + lv_tag);
                    UserDataTable<int>.Save0(true, (lp_key + "HD_StringNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = UserDataTable<string>.Load1(true, (lp_key + "HD_StringTag"), lv_b + 1);
                        UserDataTable<string>.Save1(true, (lp_key + "HD_StringTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列,这里-1可以让挑选回滚,以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动S_返回String注册总数.必须先使用"互动S_注册String"才能返回指定Key里的注册总数.String组使用时,可用"获取变量的内部名称"将String组转为Key.
        /// </summary>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        /// <returns></returns>
        public static int HD_ReturnStringNumMax(string lp_key)
        {
            return UserDataTable<int>.Load0(true, lp_key + "HD_StringNum");
        }

        /// <summary>
        /// 互动S_返回String序号.使用"互动S_注册String"后使用本函数可返回Key里的注册序号,Key无元素返回0,Key有元素但对象不在里面则返回-1,String标签尚未注册则返回-2.String组使用时,可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        /// <returns>若返回成功将得到≥1的数,返回失败则为0</returns>
        public static int HD_ReturnStringNum(string lp_string, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_i;
            string lv_tag;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;

            //Variable Initialization
            lv_str = (lp_key + "HD_String");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_string;
            lv_i = 0;

            //Implementation
            if ((lv_num == 0))
            {
                lv_i = lv_num;
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if ((UserDataTable<string>.Load1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if (lv_i == lv_num) { lv_i = 0; }
                        }
                    }
                }

            }
            return lv_i;
        }

        /// <summary>
        /// 互动S_返回序号对应String.使用"互动S_注册String"后,在参数填入注册序号可返回String.String组使用时,可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        /// <returns></returns>
        public static string HD_ReturnStringFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations And Initialization
            string lv_str = (lp_key + "HD_String");
            //Implementation
            return UserDataTable<string>.Load1(true, (lv_str + "Tag"), lp_regNum);
        }

        /// <summary>
        /// 互动S_设置String状态.必须先"注册"获得功能库内部句柄,再使用本函数给String设定一个状态值,之后可用"互动S_返回String状态".类型参数用以记录多个不同状态,仅当"类型"参数填String组ID转的String串时,状态值"true"和"false"是String的String组专用状态值,用于内部函数筛选String状态(相当于单位组单位索引是否有效),其他类型不会干扰系统内部,可随意填写.虽然注销时反向清空注册信息,但用"互动S_设定String状态/自定义值"创建的值需要手工填入""来排泄(非大量注销则提升内存量极小,可不管).注:固有状态值是注册函数赋予的系统内部变量(相当于单位组单位是否活体),只能通过"互动S_注册String(高级)"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型,默认值"State"</param>
        /// <param name="lp_stats">状态</param>
        public static void HD_SetStringState(string lp_string, string lp_key, string lp_stats)
        {
            string lv_str = (lp_key + "HD_String");
            UserDataTable<string>.Save0(true, ("State" + lv_str + "_" + lp_string), lp_stats);
        }

        /// <summary>
        /// 互动S_返回String状态.使用"互动S_设定String状态"后可使用本函数,将本函数参数"类型"设为空时返回固有值.类型参数用以记录多个不同状态,仅当"类型"参数为String组ID转的字符串时,返回的状态值"true"和"false"是String的String组专用状态值,用于内部函数筛选String状态(相当于单位组单位索引是否有效)
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型,默认值"State"</param>
        /// <returns></returns>
        public static string HD_ReturnStringState(string lp_string, string lp_key)
        {
            string lv_str = (lp_key + "HD_String");
            return UserDataTable<string>.Load0(true, ("State" + lv_str + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_设置String自定义值.必须先"注册"获得功能库内部句柄,再使用本函数设定String的自定义值,之后可使用"互动S_返回String自定义值",类型参数用以记录多个不同自定义值.注:固有自定义值是注册函数赋予的系统内部变量,只能通过"互动S_注册String(高级)"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型,默认值"A"</param>
        /// <param name="lp_customValue">自定义值</param>
        public static void HD_SetStringCV(string lp_string, string lp_key, string lp_customValue)
        {
            string lv_str = (lp_key + "HD_String");
            UserDataTable<string>.Save0(true, ("CV" + lv_str + "_" + lp_string), lp_customValue);
        }

        /// <summary>
        /// 互动S_返回String自定义值.使用"互动S_设定String自定义值"后可使用本函数,将本函数参数"类型"设为空时返回固有值,该参数用以记录多个不同自定义值
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型,默认值"A"</param>
        /// <returns></returns>
        public static string HD_ReturnStringCV(string lp_string, string lp_key)
        {
            string lv_str = (lp_key + "HD_String");
            return UserDataTable<string>.Load0(true, ("CV" + lv_str + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String固有状态.必须先使用"互动S_注册String"才能返回到该值,固有状态是独一无二的标记(相当于单位组单位是否活体)
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static string HD_ReturnStringState_Only(string lp_string)
        {
            return UserDataTable<string>.Load0(true, ("HD_StringState" + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String固有自定义值.必须先使用"互动S_注册String"才能返回到该值,固有值是独一无二的标记
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static string HD_ReturnStringCV_Only(string lp_string)
        {
            return UserDataTable<string>.Load0(true, ("HD_StringCV" + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_设置String的实数标记.必须先"注册"获得功能库内部句柄,再使用本函数让String携带一个实数值,之后可使用"互动S_返回String的实数标记".String组使用时,可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_realNumTag">实数标记</param>
        public static void HD_SetStringDouble(string lp_string, double lp_realNumTag)
        {
            UserDataTable<double>.Save0(true, ("HD_CDDouble_String_" + lp_string), lp_realNumTag);
        }

        /// <summary>
        /// 互动S_返回String的实数标记.使用"互动S_设定String的实数标记"后可使用本函数.String组使用时,可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static double HD_ReturnStringDouble(string lp_string)
        {
            return UserDataTable<double>.Load0(true, ("HD_CDDouble_String_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String标签句柄有效状态.将String视作独一无二的个体,标签是它本身,有效状态则类似"单位是否有效",当使用"互动S_注册String"或"互动SG_添加String到String组"后激活String有效状态(值为"true"),除非使用"互动S_注册String(高级)"改写,否则直到注销才会摧毁
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static bool HD_ReturnIfStringTag(string lp_string)
        {
            return UserDataTable<bool>.Load0(true, ("HD_IfStringTag" + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String注册状态.使用"互动S_注册String"或"互动SG_添加String到String组"后可使用本函数获取注册String在Key中的注册状态,该状态只能注销或从String组中移除时清空.String组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区,默认值"_String"</param>
        /// <returns></returns>
        public static bool HD_ReturnIfStringTagKey(string lp_string, string lp_key)
        {
            string lv_str = (lp_key + "HD_String");
            return UserDataTable<bool>.Load0(true, ("IfStringGTag" + lv_str + "_" + lp_string));
        }

        /// <summary>
        /// 互动SG_设定String的String组专用状态.给String组的String设定一个状态值(字符串),之后可用"互动S_返回String、互动SG_返回String组的String状态".状态值"true"和"false"是String的String组专用状态值,用于内部函数筛选字符状态(相当于单位组单位索引是否有效),而本函数可以重设干预,影响函数"互动SG_返回String组元素数量(仅检索XX状态)".与"互动S_设定String状态"功能相同,只是状态参数在String组中被固定为"String组变量的内部ID".String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_stringGroup"></param>
        /// <param name="lp_groupState"></param>
        public static void HD_SetStringGState(string lp_string, string lp_stringGroup, string lp_groupState)
        {
            HD_SetStringState(lp_string, lp_stringGroup, lp_groupState);
        }

        /// <summary>
        /// 互动SG_返回String的String组专用状态.使用"互动S_设定String、互动SG_设定String组的String状态"后可使用本函数.与"互动S_返回String状态"功能相同,只是状态参数在String组中被固定为"String组变量的内部ID".状态值"true"和"false"是String的String组专用状态值,用于内部函数筛选字符状态(相当于单位组单位索引是否有效).String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_stringGroup"></param>
        public static void HD_ReturnStringGState(string lp_string, string lp_stringGroup)
        {
            HD_ReturnStringState(lp_string, lp_stringGroup);
        }

        /// <summary>
        /// 互动SG_返回String组元素序号对应元素.返回String组元素序号指定String.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static string HD_ReturnStringFromStringGFunc(int lp_regNum, string lp_gs)
        {
            return HD_ReturnStringFromRegNum(lp_regNum, lp_gs);
        }

        /// <summary>
        /// 互动SG_返回String组元素总数.返回指定String组的元素数量.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnStringGNumMax(string lp_gs)
        {
            return UserDataTable<int>.Load0(true, lp_gs + "HD_StringNum");
        }

        /// <summary>
        /// 互动SG_返回String组元素总数(仅检测String组专用状态="true").返回指定String组的元素数量.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnStringGNumMax_StateTrueFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            string lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Implementation
            auto_ae = HD_ReturnStringNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnStringFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnStringState(lv_c, lp_gs);
                if ((lv_b == "true"))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动SG_返回String组元素总数(仅检测String组专用状态="false").返回指定String组的元素数量.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnStringGNumMax_StateFalseFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            string lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Implementation
            auto_ae = HD_ReturnStringNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnStringFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnStringState(lv_c, lp_gs);
                if ((lv_b == "false"))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动SG_返回String组元素总数(仅检测String组无效专用状态:"false"或"").返回指定String组的元素数量(false、""、null).String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnStringGNumMax_StateUselessFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            string lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Implementation
            auto_ae = HD_ReturnStringNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = (string)HD_ReturnStringFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnStringState(lv_c, lp_gs);
                if (((lv_b == "false") || (lv_b == "") || (lv_b == null)))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动SG_返回String组元素总数(仅检测String组指定专用状态).返回指定String组的元素数量.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_State">String组专用状态</param>
        /// <returns></returns>
        public static int HD_ReturnStringGNumMax_StateFunc_Specify(string lp_gs, string lp_State)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            string lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Implementation
            auto_ae = HD_ReturnStringNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = (string)HD_ReturnStringFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnStringState(lv_c, lp_gs);
                if ((lv_b == lp_State))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动SG_添加String到String组.相同String被认为是同一个,非高级功能不提供专用状态检查,若String没有设置过String组专用状态,那么首次添加到String组不会赋予"true"(之后可通过"互动S_设定String状态"、"互动SG_设定String组的String状态"修改).String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_AddStringToGroup_Simple(string lp_string, string lp_gs)
        {
            HD_RegString_Simple(lp_string, lp_gs);
        }

        /// <summary>
        /// 互动SG_添加String到String组(高级).相同String被认为是同一个,高级功能提供专用状态检查,若String没有设置过String组专用状态,那么首次添加到String组会赋予"true"(之后可通过"互动S_设定String状态"、"互动SG_设定String组的String状态"修改).String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_AddStringToGroup(string lp_string, string lp_gs)
        {
            //组中添加对象,不对其固有状态和固有自定义值进行任何修改,所以使用Simple
            HD_RegString_Simple(lp_string, lp_gs);
            if (UserDataTable<string>.KeyExists(true, ("State" + lp_gs + "HD_String_" + lp_string)) == false)
            {
                UserDataTable<string>.Save0(true, ("State" + lp_gs + "HD_String_" + lp_string), "true");
                //Console.WriteLine(lp_gs + "=>" + lp_string);
            }
        }

        /// <summary>
        /// 互动SG_移除String组中的元素.使用"互动SG_添加String到String组"后可使用本函数进行移除元素.移除使用了"互动S_移除String",同一个存储区(String组ID)序号重排,移除时该存储区如有其他操作会排队等待.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_RemoveStringFromGroup(string lp_string, string lp_gs)
        {
            HD_RemoveString(lp_string, lp_gs);
        }

        //互动SG_为String组中的每个序号
        //GE(星际2的Galaxy Editor)的宏让编辑器保存时自动生成脚本并整合进脚本进行格式调整,C#仅参考需自行编写
        //#AUTOVAR(vs, string) = "#PARAM(group)";//"#PARAM(group)"是与字段、变量名一致的元素组名称,宏去声明string类型名为“Auto随机编号_vs”的自动变量,然后=右侧字符
        //#AUTOVAR(ae) = HD_ReturnStringNumMax(#AUTOVAR(vs));//宏去声明默认int类型名为“Auto随机编号_ae”的自动变量,然后=右侧字符
        //#INITAUTOVAR(ai,increment)//宏去声明int类型名为“Auto随机编号_ai”的自动变量,用于下面for循环增量(increment是传入参数)
        //#PARAM(var) = #PARAM(s);//#PARAM(var)是传进来的参数,用作“当前被挑选到的元素”(任意变量-整数 lp_var), #PARAM(s)是传进来的参数用作"开始"(int lp_s)
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #PARAM(var) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #PARAM(var) >= #AUTOVAR(ae)) ) ; #PARAM(var) += #AUTOVAR(ai) ) {
        //    #SUBFUNCS(actions)//代表用户GUI填写的所有动作
        //}

        /// <summary>
        /// 互动SG_为String组中的每个序号.每次挑选的元素序号会自行在动作组(委托函数)中使用,委托函数特征:void SubActionTest(int lp_var),参数lp_var即每次遍历到的元素序号,请自行组织它在委托函数内如何使用,SubActionTest可直接作为本函数最后一个参数填入,填入多个动作范例:SubVActionEventFuncref Actions += SubActionTest,然后Actions作为参数填入.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_start">开始</param>
        /// <param name="lp_increment">增量</param>
        /// <param name="lp_funcref">委托类型变量或函数引用</param>
        public static void HD_ForEachStringNumFromGroup(string lp_gs, int lp_start, int lp_increment, SubVActionEventFuncref lp_funcref)
        {
            int lv_ae = HD_ReturnStringNumMax(lp_gs);
            int lv_var = lp_start;
            int lv_ai = lp_increment;
            for (; (lv_ai >= 0 && lv_var <= lv_ae) || (lv_ai < 0 && lv_var >= lv_ae); lv_var += lv_ai)
            {
                lp_funcref(lv_var);//用户填写的所有动作
            }
        }

        //互动SG_为String组中的每个元素
        //#AUTOVAR(vs, string) = "#PARAM(group)";
        //#AUTOVAR(ae) = HD_ReturnStringNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= #PARAM(s);
        //#INITAUTOVAR(ai,increment)
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    DataTableSave(false, "StringGFor"+ #AUTOVAR(vs) + IntToString(#AUTOVAR(va)), HD_ReturnStringFromRegNum(#AUTOVAR(va),#AUTOVAR(vs)));
        //}
        //#AUTOVAR(va)= #PARAM(s);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #PARAM(var) = DataTableLoad(false, "StringGFor"+ #AUTOVAR(vs) + IntToString(#AUTOVAR(va)));
        //    #SUBFUNCS(actions)
        //}

        /// <summary>
        /// 互动SG_为String组中的每个元素.每次挑选的元素会自行在动作组(委托函数)中使用,委托函数特征:void SubSActionEventFuncref(string lp_str),参数lv_str即每次遍历到的元素,请自行组织它在委托函数内如何使用,SubActionTest可直接作为本函数最后一个参数填入,填入多个动作范例:SubVActionEventFuncref Actions += SubActionTest,然后Actions作为参数填入.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_start">开始</param>
        /// <param name="lp_increment">增量</param>
        /// <param name="lp_funcref">委托类型变量或函数引用</param>
        public static void HD_ForEachStringFromGroup(string lp_gs, int lp_start, int lp_increment, SubSActionEventFuncref lp_funcref)
        {
            string lv_vs = lp_gs;
            int lv_ae = HD_ReturnStringNumMax(lv_vs);
            int lv_va = lp_start;
            int lv_ai = lp_increment;
            string lv_str;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                UserDataTable<string>.Save0(false, "HD_StringGFor" + lv_vs + lv_va.ToString(), HD_ReturnStringFromRegNum(lv_va, lv_vs));
            }
            lv_va = lp_start;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                lv_str = UserDataTable<string>.Load0(false, "HD_StringGFor" + lv_vs + lv_va.ToString());
                lp_funcref(lv_str);//用户填写的所有动作
            }
        }

        /// <summary>
        /// 互动SG_返回String组中随机元素.返回指定String组中的随机String.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static string HD_ReturnRandomStringFromStringGFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_num;
            int lv_a;
            string lv_c = null;
            //Variable Initialization
            lv_num = HD_ReturnStringNumMax(lp_gs);
            //Implementation
            if ((lv_num >= 1))
            {
                lv_a = RandomInt(1, lv_num);
                lv_c = HD_ReturnStringFromRegNum(lv_a, lp_gs);
            }
            return lv_c;
        }

        //互动SG_添加String组到String组
        //#AUTOVAR(vs, string) = "#PARAM(groupA)";
        //#AUTOVAR(vsb, string) = "#PARAM(groupB)";
        //#AUTOVAR(ae) = HD_ReturnStringNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= 1;
        //#AUTOVAR(ai)= 1;
        //#AUTOVAR(var);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #AUTOVAR(var) = HD_ReturnStringFromRegNum(#AUTOVAR(va), #AUTOVAR(vs));
        //    HD_AddStringToGroup(#AUTOVAR(var), #AUTOVAR(vsb));
        //}


        /// <summary>
        /// 互动SG_添加String组到String组.添加一个String组A的元素到另一个String组B,相同String被认为是同一个.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_groupA"></param>
        /// <param name="lp_groupB"></param>
        public static void HD_AddStringGToStringG(string lp_groupA, string lp_groupB)
        {
            string lv_vsa = lp_groupA;
            string lv_vsb = lp_groupB;
            int lv_ae = HD_ReturnStringNumMax(lv_vsa);
            int lv_va = 1;
            int lv_ai = 1;
            string lv_var;
            for (; ((lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae)); lv_va += lv_ai)
            {
                lv_var = HD_ReturnStringFromRegNum(lv_va, lv_vsa);
                HD_AddStringToGroup(lv_var, lv_vsb);
            }
        }

        //互动SG_从String组移除String组
        //#AUTOVAR(vs, string) = "#PARAM(groupA)";
        //#AUTOVAR(vsb, string) = "#PARAM(groupB)";
        //#AUTOVAR(ae) = HD_ReturnStringNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= 1;
        //#AUTOVAR(ai)= 1;
        //#AUTOVAR(var);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #AUTOVAR(var) = HD_ReturnStringFromRegNum(#AUTOVAR(va), #AUTOVAR(vs));
        //    HD_RemoveString(#AUTOVAR(var), #AUTOVAR(vsb));
        //}

        /// <summary>
        /// 互动SG_从String组移除String组.将String组A的元素从String组B中移除,相同String被认为是同一个.移除使用了"互动S_移除String",同一个存储区(String组ID)序号重排,移除时该存储区如有其他操作会排队等待.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_groupA"></param>
        /// <param name="lp_groupB"></param>
        public static void HD_ClearStringGFromStringG(string lp_groupA, string lp_groupB)
        {
            string lv_vsa = lp_groupA;
            string lv_vsb = lp_groupB;
            int lv_ae = HD_ReturnStringNumMax(lv_vsa);
            int lv_va = 1;
            int lv_ai = 1;
            string lv_var;
            for (; ((lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae)); lv_va += lv_ai)
            {
                lv_var = HD_ReturnStringFromRegNum(lv_va, lv_vsa);
                HD_RemoveString(lv_var, lv_vsb);
            }
        }

        /// <summary>
        /// 互动SG_移除String组全部元素.将String组(Key区)存储的元素全部移除,相同String被认为是同一个.移除时同一个存储区(String组ID)序号不进行重排,但该存储区如有其他操作会排队等待.String组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_key">存储键区,默认填String组名称</param>
        public static void HD_RemoveStringGAll(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tag;
            int lv_a;
            //Variable Initialization
            lv_str = (lp_key + "HD_String");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                lv_tag = UserDataTable<string>.Load1(true, (lp_key + "HD_StringTag"), lv_a);
                lv_num -= 1;
                UserDataTable<bool>.Clear0(true, "IfStringGTag" + lv_str + "_" + lv_tag);
                UserDataTable<string>.Clear0(true, "HD_StringCV" + lv_str + "_" + lv_tag);
                UserDataTable<string>.Clear0(true, "HD_StringState" + lv_str + "_" + lv_tag);
                UserDataTable<int>.Save0(true, (lp_key + "HD_StringNum"), lv_num);
            }
            ThreadWaitSet(true, lv_str, false);
        }

        //--------------------------------------------------------------------------------------------------
        //字符串组End
        //--------------------------------------------------------------------------------------------------

        #endregion

        #region 数字

        //提示:尽可能使用对口类型,以防值类型与引用类型发生转换时拆装箱降低性能

        //--------------------------------------------------------------------------------------------------
        //数字组Start
        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// 互动I_注册Int(高级).在指定Key存入Int,固有状态、固有自定义值是Int独一无二的标志(本函数重复注册会刷新),之后可用互动I_"返回Int注册总数"、"返回Int序号"、"返回序号对应Int"、"返回序号对应Int标签"、"返回Int自定义值".Int组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Int组转为Key.首次注册时固有状态为true(相当于单位组单位活体),如需另外设置多个标记可使用"互动I_设定Int状态/自定义值"
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        /// <param name="lp_inherentStats">固有状态</param>
        /// <param name="lp_inherentCustomValue">固有自定义值</param>
        public static void HD_RegInt(int lp_integer, string lp_key, string lp_inherentStats = "true", string lp_inherentCustomValue = "")
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_i;
            int lv_tag;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;

            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                UserDataTable<bool>.Save1(true, ("HD_IfIntTag"), lv_tag, true);
                UserDataTable<bool>.Save1(true, ("IfIntGTag" + lv_str), lv_tag, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if ((UserDataTable<int>.Load1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                UserDataTable<bool>.Save1(true, ("HD_IfIntTag"), lv_tag, true);
                                UserDataTable<bool>.Save1(true, ("IfIntGTag" + lv_str), lv_tag, true);
                            }

                        }
                    }
                }

            }
            UserDataTable<string>.Save1(true, (("HD_IntState")), lv_tag, lp_inherentStats);
            UserDataTable<string>.Save1(true, (("HD_IntCV")), lv_tag, lp_inherentCustomValue);
        }

        /// <summary>
        /// 互动I_注册Int.在指定Key存入Int,固有状态、固有自定义值是Int独一无二的标志(本函数重复注册不会刷新),之后可用互动I_"返回Int注册总数"、"返回Int序号"、"返回序号对应Int"、"返回Int自定义值".Int组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Int组转为Key.首次注册时固有状态为true(相当于单位组单位活体),之后只能通过"互动I_注册Int(高级)"改写,如需另外设置多个标记可使用"互动I_设定Int状态/自定义值"
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        public static void HD_RegInt_Simple(int lp_integer, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_i;
            int lv_tag;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;

            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                UserDataTable<bool>.Save1(true, ("HD_IfIntTag"), lv_tag, true);
                UserDataTable<bool>.Save1(true, ("IfIntGTag" + lv_str), lv_tag, true);
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if ((UserDataTable<int>.Load1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                UserDataTable<int>.Save0(true, (lv_str + "Num"), lv_i);
                                UserDataTable<int>.Save1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                UserDataTable<bool>.Save1(true, ("HD_IfIntTag"), lv_tag, true);
                                UserDataTable<bool>.Save1(true, ("IfIntGTag" + lv_str), lv_tag, true);
                            }

                        }
                    }
                }

            }
            //从未注册过则进行首次修改为true
            if ((UserDataTable<bool>.KeyExists(true, ("HD_IntState" + "_" + lv_tag.ToString())) == false))
            {
                UserDataTable<string>.Save1(true, (("HD_IntState")), lv_tag, "true");
            }
        }

        /// <summary>
        /// 互动I_注销Int.用"互动I_注册Int"到Key,之后可用本函数彻底摧毁注册信息并将序号重排(包括Int标签有效状态、固有状态及自定义值).注册注销同时进行会排队等待0.0625s直到没有注销动作,注销并不提升多少内存只是变量内容清空并序号重利用,非特殊要求一般不注销,而是用"互动I_设定Int状态"让Int状态失效(类似单位组的单位活体状态).Int组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Int组转为Key.本函数无法摧毁用"互动I_设定Int状态/自定义值"创建的状态和自定义值,需手工填入""来排泄(非大量注销则提升内存量极小,可不管).本函数参数Key若填Int组变量ID时会清空Int组专用状态
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        public static void HD_DestroyInt(int lp_integer, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_tag;
            int lv_a;
            int lv_b;
            int lv_c;

            //Automatic Variable Declarations
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((UserDataTable<int>.Load1(true, (lv_str + "Tag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    UserDataTable<bool>.Clear1(true, "HD_IfIntTag", lv_tag);
                    UserDataTable<bool>.Clear1(true, "IfIntGTag" + lv_str, lv_tag);
                    UserDataTable<string>.Clear1(true, "HD_IntCV", lv_tag);
                    UserDataTable<string>.Clear1(true, "HD_IntState", lv_tag);
                    UserDataTable<string>.Clear1(true, "HD_IntCV" + lv_str, lv_tag);
                    UserDataTable<string>.Clear1(true, "HD_IntState" + lv_str, lv_tag);
                    UserDataTable<int>.Save0(true, (lp_key + "HD_IntNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = UserDataTable<int>.Load1(true, (lp_key + "HD_IntTag"), lv_b + 1);
                        UserDataTable<int>.Save1(true, (lp_key + "HD_IntTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列,这里-1可以让挑选回滚,以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动I_移除Int.用"互动I_注册Int"到Key,之后可用本函数仅摧毁Key区注册的信息并将序号重排,用于Int组或多个键区仅移除Int(保留Int标签有效状态、固有值).注册注销同时进行会排队等待0.0625s直到没有注销动作,注销并不提升多少内存只是变量内容清空并序号重利用,非特殊要求一般不注销,而是用"互动I_设定Int状态"让Int状态失效(类似单位组的单位活体状态).Int组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Int组转为Key.本函数无法摧毁用"互动I_设定Int状态/自定义值"创建的状态和自定义值,需手工填入""来排泄(非大量注销则提升内存量极小,可不管).本函数参数Key若填Int组变量ID时会清空Int组专用状态
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        public static void HD_RemoveInt(int lp_integer, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_tag;
            int lv_a;
            int lv_b;
            int lv_c;

            //Automatic Variable Declarations
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((UserDataTable<int>.Load1(true, (lp_key + "HD_IntTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    UserDataTable<bool>.Clear1(true, "IfIntGTag" + lv_str, lv_tag);
                    UserDataTable<string>.Clear1(true, "HD_IntCV" + lv_str, lv_tag);
                    UserDataTable<string>.Clear1(true, "HD_IntState" + lv_str, lv_tag);
                    UserDataTable<int>.Save0(true, (lp_key + "HD_IntNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = UserDataTable<int>.Load1(true, (lp_key + "HD_IntTag"), lv_b + 1);
                        UserDataTable<int>.Save1(true, (lp_key + "HD_IntTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列,这里-1可以让挑选回滚,以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动I_返回Int注册总数.必须先使用"互动I_注册Int"才能返回指定Key里的注册总数.Int组使用时,可用"获取变量的内部名称"将Int组转为Key.
        /// </summary>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        /// <returns></returns>
        public static int HD_ReturnIntNumMax(string lp_key)
        {
            string lv_str = (lp_key + "HD_Int");
            return UserDataTable<int>.Load0(true, (lv_str + "Num"));
        }

        /// <summary>
        /// 互动I_返回Int序号.使用"互动I_注册Int"后使用本函数可返回Key里的注册序号,Key无元素返回0,Key有元素但对象不在里面则返回-1,Int标签尚未注册则返回-2.Int组使用时,可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        /// <returns></returns>
        public static int HD_ReturnIntNum(int lp_integer, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            int lv_i = -1;
            int lv_tag;

            //Automatic Variable Declarations
            int auto_ae;
            int auto_var;

            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            if ((lv_num == 0))
            {
                lv_i = lv_num;
            }
            else
            {
                if ((lv_num >= 1))
                {
                    auto_ae = lv_num;
                    auto_var = 1;
                    for (; auto_var <= auto_ae; auto_var += 1)
                    {
                        lv_i = auto_var;
                        if ((UserDataTable<int>.Load1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if (lv_i == lv_num) { lv_i = 0; }
                        }
                    }
                }

            }
            return lv_i;
        }

        /// <summary>
        /// 互动I_返回序号对应Int.使用"互动I_注册Int"后,在参数填入注册序号可返回Int.Int组使用时,可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        /// <returns></returns>
        public static int HD_ReturnIntFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_tag = UserDataTable<int>.Load1(true, (lv_str + "Tag"), lp_regNum);
            //Implementation
            return lv_tag;
        }

        /// <summary>
        /// 互动I_设置Int状态.必须先"注册"获得功能库内部句柄,再使用本函数给Int设定一个状态值,之后可用"互动I_返回Int状态".类型参数用以记录多个不同状态,仅当"类型"参数填Int组ID转的Int串时,状态值"true"和"false"是Int的Int组专用状态值,用于内部函数筛选Int状态(相当于单位组单位索引是否有效),其他类型不会干扰系统内部,可随意填写.虽然注销时反向清空注册信息,但用"互动I_设定Int状态/自定义值"创建的值需要手工填入""来排泄(非大量注销则提升内存量极小,可不管).注:固有状态值是注册函数赋予的系统内部变量(相当于单位组单位是否活体),只能通过"互动I_注册Int(高级)"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型,默认值"State"</param>
        /// <param name="lp_stats">状态</param>
        public static void HD_SetIntState(int lp_integer, string lp_key, string lp_stats)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_tag = lp_integer;
            //Implementation
            UserDataTable<string>.Save1(true, ("State" + lv_str), lv_tag, lp_stats);
        }

        /// <summary>
        /// 互动I_返回Int状态.使用"互动I_设定Int状态"后可使用本函数,将本函数参数"类型"设为空时返回固有值.类型参数用以记录多个不同状态,仅当"类型"参数为Int组ID转的字符串时,返回的状态值"true"和"false"是Int的Int组专用状态值,用于内部函数筛选Int状态(相当于单位组单位索引是否有效)
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型,默认值"State"</param>
        /// <returns></returns>
        public static string HD_ReturnIntState(int lp_integer, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            string lv_stats;

            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_tag = lp_integer;
            lv_stats = UserDataTable<string>.Load1(true, ("State" + lv_str), lv_tag);

            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动I_设置Int自定义值.必须先"注册"获得功能库内部句柄,再使用本函数设定Int的自定义值,之后可使用"互动I_返回Int自定义值",类型参数用以记录多个不同自定义值.注:固有自定义值是注册函数赋予的系统内部变量,只能通过"互动I_注册Int(高级)"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型,默认值"A"</param>
        /// <param name="lp_customValue">自定义值</param>
        public static void HD_SetIntCV(int lp_integer, string lp_key, string lp_customValue)
        {
            UserDataTable<string>.Save1(true, string.Concat("CV", lp_key, "HD_Int"), lp_integer, lp_customValue);
        }

        /// <summary>
        /// 互动I_返回Int自定义值.使用"互动I_设定Int自定义值"后可使用本函数,将本函数参数"类型"设为空时返回固有值,该参数用以记录多个不同自定义值
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型,默认值"A"</param>
        /// <returns></returns>
        public static string HD_ReturnIntCV(int lp_integer, string lp_key)
        {
            return UserDataTable<string>.Load1(true, string.Concat("CV", lp_key, "HD_Int"), lp_integer);
        }

        /// <summary>
        /// 互动I_返回Int固有状态.必须先使用"互动I_注册Int"才能返回到该值,固有状态是独一无二的标记(相当于单位组单位是否活体)
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <returns></returns>
        public static string HD_ReturnIntState_Only(int lp_integer)
        {
            //Variable Declarations
            int lv_tag;
            string lv_stats;
            //Variable Initialization
            lv_tag = lp_integer;
            lv_stats = UserDataTable<string>.Load1(true, ("HD_IntState"), lv_tag);
            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动I_返回Int固有自定义值.必须先使用"互动I_注册Int"才能返回到该值,固有值是独一无二的标记
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <returns></returns>
        public static string HD_ReturnIntCV_Only(int lp_integer)
        {
            //Variable Declarations
            int lv_tag;
            string lv_customValue;
            //Variable Initialization
            lv_tag = lp_integer;
            lv_customValue = UserDataTable<string>.Load1(true, (("HD_IntCV")), lv_tag);
            //Implementation
            return lv_customValue;
        }

        /// <summary>
        /// 互动I_设置Int的实数标记.必须先"注册"获得功能库内部句柄,再使用本函数让Int携带一个实数值,之后可使用"互动I_返回Int的实数标记".Int组使用时,可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_realNumTag">实数标记</param>
        public static void HD_SetIntDouble(int lp_integer, double lp_realNumTag)
        {
            UserDataTable<double>.Save1(true, "HD_CDDouble_Int", lp_integer, lp_realNumTag);
        }

        /// <summary>
        /// 互动I_返回Int的实数标记.使用"互动I_设定Int的实数标记"后可使用本函数.Int组使用时,可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <returns></returns>
        public static double HD_ReturnIntDouble(int lp_integer)
        {
            return UserDataTable<double>.Load1(true, "HD_CDDouble_Int", lp_integer);
        }

        /// <summary>
        /// 互动I_返回Int标签句柄有效状态.将Int视作独一无二的个体,标签是它本身,有效状态则类似"单位是否有效",当使用"互动I_注册Int"或"互动IG_添加Int到Int组"后激活Int有效状态(值为"true"),除非使用"互动I_注册Int(高级)"改写,否则直到注销才会摧毁
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <returns></returns>
        public static bool HD_ReturnIfIntTag(int lp_integer)
        {
            return UserDataTable<bool>.Load1(true, ("HD_IfIntTag"), lp_integer);
        }

        /// <summary>
        /// 互动I_返回Int注册状态.使用"互动I_注册Int"或"互动IG_添加Int到Int组"后可使用本函数获取注册Int在Key中的注册状态,该状态只能注销或从Int组中移除时清空.Int组使用时,Key被强制为变量ID,可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区,默认值"_Int"</param>
        /// <returns></returns>
        public static bool HD_ReturnIfIntTagKey(int lp_integer, string lp_key)
        {
            return UserDataTable<bool>.Load1(true, ("IfIntGTag" + (lp_key + "HD_Int")), lp_integer);
        }

        /// <summary>
        /// 互动IG_根据自定义值类型将Int组排序.根据Int携带的自定义值类型,对指定的Int组元素进行冒泡排序.Int组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区,默认填Int组名称</param>
        /// <param name="lp_cVStr">自定义值类型</param>
        /// <param name="lp_big">是否大值靠前</param>
        public static void HD_IntGSortCV(string lp_key, string lp_cVStr, bool lp_big)
        {
            //Variable Declarations
            int lv_a;
            int lv_b;
            int lv_c;
            bool lv_bool;
            int lv_tag;
            int lv_tagValue;
            string lv_str;
            int lv_num;
            int lv_intStackOutSize;
            string lv_tagValuestr;
            //Automatic Variable Declarations
            int autoB_ae;
            const int autoB_ai = 1;
            int autoC_ae;
            const int autoC_ai = 1;
            int autoHD_ae;
            const int autoHD_ai = -1;
            int autoE_ae;
            const int autoE_ai = 1;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_intStackOutSize = 0;
            //Implementation
            autoB_ae = lv_num;
            lv_a = 1;
            for (; ((autoB_ai >= 0 && lv_a <= autoB_ae) || (autoB_ai < 0 && lv_a >= autoB_ae)); lv_a += autoB_ai)
            {
                lv_tag = HD_ReturnIntFromRegNum(lv_a, lp_key);
                lv_tagValuestr = HD_ReturnIntCV(lv_tag, lp_cVStr);
                lv_tagValue = Convert.ToInt32(lv_tagValuestr);
                //Console.WriteLine("循环" + IntToString(lv_a) +"tag"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue));
                if ((lv_intStackOutSize == 0))
                {
                    lv_intStackOutSize += 1;
                    UserDataTable<int>.Save1(false, "HD_IntStackOutTag", 1, lv_tag);
                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", 1, lv_tagValue);
                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+",IteraOrig="+IntToString(lv_a));
                }
                else
                {
                    lv_bool = false;
                    autoC_ae = lv_intStackOutSize;
                    lv_b = 1;
                    //Console.WriteLine("For" + IntToString(1) +"到"+IntToString(autoC_ae));
                    for (; ((autoC_ai >= 0 && lv_b <= autoC_ae) || (autoC_ai < 0 && lv_b >= autoC_ae)); lv_b += autoC_ai)
                    {
                        if (lp_big == false)
                        {
                            //Console.WriteLine("小值靠前");
                            if (lv_tagValue < UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+",当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnIntTagFromRegNum(lv_b, lp_key))+"值"+IntToString(UserDataTable<int>.Load1(false, "IntStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b:" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize:" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae:" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c:" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(UserDataTable<int>.Load1(false, "IntStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(UserDataTable<int>.Load1(false, "IntStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(UserDataTable<int>.Load1(false, "IntStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄,让元素交换
                //lv_tagStr = UserDataTable<int>.Load1(true, (lp_key + "IntTag"), lv_a).ToString(); //原始序号元素
                lv_tag = UserDataTable<int>.Load1(false, "HD_IntStackOutTag", lv_a);
                lv_tagValuestr = HD_ReturnIntCV(lv_tag, lp_cVStr);
                lv_tagValue = Convert.ToInt32(lv_tagValuestr);
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr));
                lv_b = UserDataTable<int>.Load1(false, "HD_IntStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号:" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a:"+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    UserDataTable<int>.Save1(true, (lp_key + "HD_IntTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                   //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动IG_Int组排序.对指定的Int组元素进行冒泡排序(根据元素句柄).Int组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区,默认填Int组名称</param>
        /// <param name="lp_big">是否大值靠前</param>
        public static void HD_IntGSort(string lp_key, bool lp_big)
        {
            //Automatic Variable Declarations
            //Implementation
            //Variable Declarations
            int lv_a;
            int lv_b;
            int lv_c;
            bool lv_bool;
            int lv_tag;
            int lv_tagValue;
            string lv_str;
            int lv_num;
            int lv_intStackOutSize;
            //Automatic Variable Declarations
            int autoB_ae;
            const int autoB_ai = 1;
            int autoC_ae;
            const int autoC_ai = 1;
            int autoHD_ae;
            const int autoHD_ai = -1;
            int autoE_ae;
            const int autoE_ai = 1;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            lv_intStackOutSize = 0;
            //Implementation
            autoB_ae = lv_num;
            lv_a = 1;
            for (; ((autoB_ai >= 0 && lv_a <= autoB_ae) || (autoB_ai < 0 && lv_a >= autoB_ae)); lv_a += autoB_ai)
            {
                lv_tag = HD_ReturnIntFromRegNum(lv_a, lp_key);
                lv_tagValue = lv_tag;
                //Console.WriteLine("循环" + IntToString(lv_a) +"tag"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue));
                if ((lv_intStackOutSize == 0))
                {
                    lv_intStackOutSize += 1;
                    UserDataTable<int>.Save1(false, "HD_IntStackOutTag", 1, lv_tag);
                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", 1, lv_tagValue);
                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+",IteraOrig="+IntToString(lv_a));
                }
                else
                {
                    lv_bool = false;
                    autoC_ae = lv_intStackOutSize;
                    lv_b = 1;
                    //Console.WriteLine("For" + IntToString(1) +"到"+IntToString(autoC_ae));
                    for (; ((autoC_ai >= 0 && lv_b <= autoC_ae) || (autoC_ai < 0 && lv_b >= autoC_ae)); lv_b += autoC_ai)
                    {
                        if (lp_big == false)
                        {
                            //Console.WriteLine("小值靠前");
                            if (lv_tagValue < UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+",当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnIntTagFromRegNum(lv_b, lp_key))+"值"+IntToString(UserDataTable<int>.Load1(false, "IntStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b:" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize:" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae:" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c:" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(UserDataTable<int>.Load1(false, "IntStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(UserDataTable<int>.Load1(false, "IntStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_c, UserDataTable<int>.Load1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(UserDataTable<int>.Load1(false, "IntStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        UserDataTable<int>.Save1(false, "HD_IntStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_IntStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        UserDataTable<int>.Save1(false, "HD_IntStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄,让元素交换
                //lv_tagStr = UserDataTable<int>.Load1(true, (lp_key + "IntTag"), lv_a).ToString(); //原始序号元素
                lv_tag = UserDataTable<int>.Load1(false, "HD_IntStackOutTag", lv_a);
                lv_tagValue = lv_tag;
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr));
                lv_b = UserDataTable<int>.Load1(false, "HD_IntStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素:" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号:" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a:"+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    UserDataTable<int>.Save1(true, (lp_key + "HD_IntTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                   //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动IG_设定Int的Int组专用状态.给Int组的Int设定一个状态值(字符串),之后可用"互动I_返回Int、互动IG_返回Int组的Int状态".状态值"true"和"false"是Int的Int组专用状态值,用于内部函数筛选字符状态(相当于单位组单位索引是否有效),而本函数可以重设干预,影响函数"互动IG_返回Int组元素数量(仅检索XX状态)".与"互动I_设定Int状态"功能相同,只是状态参数在Int组中被固定为"Int组变量的内部ID".Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_integerGroup"></param>
        /// <param name="lp_groupState"></param>
        public static void HD_SetIntGState(int lp_integer, string lp_integerGroup, string lp_groupState)
        {
            HD_SetIntState(lp_integer, lp_integerGroup, lp_groupState);
        }

        /// <summary>
        /// 互动IG_返回Int的Int组专用状态.使用"互动I_设定Int、互动IG_设定Int组的Int状态"后可使用本函数.与"互动I_返回Int状态"功能相同,只是状态参数在Int组中被固定为"Int组变量的内部ID".状态值"true"和"false"是Int的Int组专用状态值,用于内部函数筛选字符状态(相当于单位组单位索引是否有效).Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_integerGroup"></param>
        public static void HD_ReturnIntGState(int lp_integer, string lp_integerGroup)
        {
            HD_ReturnIntState(lp_integer, lp_integerGroup);
        }

        /// <summary>
        /// 互动IG_返回Int组元素序号对应元素.返回Int组元素序号指定Int.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnIntFromIntGFunc(int lp_regNum, string lp_gs)
        {
            return HD_ReturnIntFromRegNum(lp_regNum, lp_gs);
        }

        /// <summary>
        /// 互动IG_返回Int组元素总数.返回指定Int组的元素数量.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnIntGNumMax(string lp_gs)
        {
            return UserDataTable<int>.Load0(true, lp_gs + "HD_IntNum");
        }

        /// <summary>
        /// 互动IG_返回Int组元素总数(仅检测Int组专用状态="true").返回指定Int组的元素数量.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnIntGNumMax_StateTrueFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            int lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnIntNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnIntFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnIntState(lv_c, lp_gs);
                if ((lv_b == "true"))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动IG_返回Int组元素总数(仅检测Int组专用状态="false").返回指定Int组的元素数量.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnIntGNumMax_StateFalseFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            int lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnIntNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnIntFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnIntState(lv_c, lp_gs);
                if ((lv_b == "false"))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动IG_返回Int组元素总数(仅检测Int组无效专用状态:"false"或"").返回指定Int组的元素数量(false、""、null).Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnIntGNumMax_StateUselessFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            int lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnIntNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnIntFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnIntState(lv_c, lp_gs);
                if (((lv_b == "false") || (lv_b == "") || (lv_b == null)))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动IG_返回Int组元素总数(仅检测Int组指定专用状态).返回指定Int组的元素数量.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_State">Int组专用状态</param>
        /// <returns></returns>
        public static int HD_ReturnIntGNumMax_StateFunc_Specify(string lp_gs, string lp_State)
        {
            //Variable Declarations
            int lv_a;
            string lv_b;
            int lv_c;
            int lv_i = 0;
            //Automatic Variable Declarations
            int auto_ae;
            const int auto_ai = 1;
            //Variable Initialization
            lv_b = "";
            //Implementation
            auto_ae = HD_ReturnIntNumMax(lp_gs);
            lv_a = 1;
            for (; ((auto_ai >= 0 && lv_a <= auto_ae) || (auto_ai < 0 && lv_a >= auto_ae)); lv_a += auto_ai)
            {
                lv_c = HD_ReturnIntFromRegNum(lv_a, lp_gs);
                lv_b = HD_ReturnIntState(lv_c, lp_gs);
                if ((lv_b == lp_State))
                {
                    lv_i += 1;
                }
            }
            return lv_i;
        }

        /// <summary>
        /// 互动IG_添加Int到Int组.相同Int被认为是同一个,非高级功能不提供专用状态检查,若Int没有设置过Int组专用状态,那么首次添加到Int组不会赋予"true"(之后可通过"互动I_设定Int状态"、"互动IG_设定Int组的Int状态"修改).Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_AddIntToGroup_Simple(int lp_integer, string lp_gs)
        {
            HD_RegInt_Simple(lp_integer, lp_gs);
        }

        /// <summary>
        /// 互动IG_添加Int到Int组(高级).相同Int被认为是同一个,高级功能提供专用状态检查,若Int没有设置过Int组专用状态,那么首次添加到Int组会赋予"true"(之后可通过"互动I_设定Int状态"、"互动IG_设定Int组的Int状态"修改).Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_AddIntToGroup(int lp_integer, string lp_gs)
        {
            //组中添加对象,不对其固有状态和固有自定义值进行任何修改,所以使用Simple
            HD_RegInt_Simple(lp_integer, lp_gs);
            if (UserDataTable<string>.KeyExists(true, ("HD_IntState" + lp_gs + "HD_Int_" + lp_integer.ToString())) == false)
            {
                UserDataTable<string>.Save0(true, ("HD_IntState" + lp_gs + "HD_Int_" + lp_integer.ToString()), "true");
                //Console.WriteLine(lp_gs + "=>" + HD_RegIntTagAndReturn(lp_integer));
            }
        }

        /// <summary>
        /// 互动IG_移除Int组中的元素.使用"互动IG_添加Int到Int组"后可使用本函数进行移除元素.移除使用了"互动I_移除Int",同一个存储区(Int组ID)序号重排,移除时该存储区如有其他操作会排队等待.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        public static void HD_RemoveIntFromGroup(int lp_integer, string lp_gs)
        {
            HD_RemoveInt(lp_integer, lp_gs);
        }

        //互动IG_为Int组中的每个序号
        //GE(星际2的Galaxy Editor)的宏让编辑器保存时自动生成脚本并整合进脚本进行格式调整,C#仅参考需自行编写
        //#AUTOVAR(vs, string) = "#PARAM(group)";//"#PARAM(group)"是与字段、变量名一致的元素组名称,宏去声明string类型名为“Auto随机编号_vs”的自动变量,然后=右侧字符
        //#AUTOVAR(ae) = HD_ReturnIntNumMax(#AUTOVAR(vs));//宏去声明默认int类型名为“Auto随机编号_ae”的自动变量,然后=右侧字符
        //#INITAUTOVAR(ai,increment)//宏去声明int类型名为“Auto随机编号_ai”的自动变量,用于下面for循环增量(increment是传入参数)
        //#PARAM(var) = #PARAM(s);//#PARAM(var)是传进来的参数,用作“当前被挑选到的元素”(任意变量-整数 lp_var), #PARAM(s)是传进来的参数用作"开始"(int lp_s)
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #PARAM(var) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #PARAM(var) >= #AUTOVAR(ae)) ) ; #PARAM(var) += #AUTOVAR(ai) ) {
        //    #SUBFUNCS(actions)//代表用户GUI填写的所有动作
        //}

        /// <summary>
        /// 互动IG_为Int组中的每个序号.每次挑选的元素序号会自行在动作组(委托函数)中使用,委托函数特征:void SubActionTest(int lp_var),参数lp_var即每次遍历到的元素序号,请自行组织它在委托函数内如何使用,SubActionTest可直接作为本函数最后一个参数填入,填入多个动作范例:SubVActionEventFuncref Actions += SubActionTest,然后Actions作为参数填入.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_start">开始</param>
        /// <param name="lp_increment">增量</param>
        /// <param name="lp_funcref">委托类型变量或函数引用</param>
        public static void HD_ForEachIntNumFromGroup(string lp_gs, int lp_start, int lp_increment, SubVActionEventFuncref lp_funcref)
        {
            int lv_ae = HD_ReturnIntNumMax(lp_gs);
            int lv_var = lp_start;
            int lv_ai = lp_increment;
            for (; (lv_ai >= 0 && lv_var <= lv_ae) || (lv_ai < 0 && lv_var >= lv_ae); lv_var += lv_ai)
            {
                lp_funcref(lv_var);//用户填写的所有动作
            }
        }

        //互动IG_为Int组中的每个元素
        //#AUTOVAR(vs, string) = "#PARAM(group)";
        //#AUTOVAR(ae) = HD_ReturnIntNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= #PARAM(s);
        //#INITAUTOVAR(ai,increment)
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    DataTableSave(false, "IntGFor"+ #AUTOVAR(vs) + IntToString(#AUTOVAR(va)), HD_ReturnIntFromRegNum(#AUTOVAR(va),#AUTOVAR(vs)));
        //}
        //#AUTOVAR(va)= #PARAM(s);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #PARAM(var) = DataTableLoad(false, "IntGFor"+ #AUTOVAR(vs) + IntToString(#AUTOVAR(va)));
        //    #SUBFUNCS(actions)
        //}

        /// <summary>
        /// 互动IG_为Int组中的每个元素.每次挑选的元素会自行在动作组(委托函数)中使用,委托函数特征:void SubActionTest(int lp_var),参数lp_var即每次遍历到的元素,请自行组织它在委托函数内如何使用,SubActionTest可直接作为本函数最后一个参数填入,填入多个动作范例:SubVActionEventFuncref Actions += SubActionTest,然后Actions作为参数填入.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <param name="lp_start">开始</param>
        /// <param name="lp_increment">增量</param>
        /// <param name="lp_funcref">委托类型变量或函数引用</param>
        public static void HD_ForEachIntFromGroup(string lp_gs, int lp_start, int lp_increment, SubVActionEventFuncref lp_funcref)
        {
            string lv_vs = lp_gs;
            int lv_ae = HD_ReturnIntNumMax(lv_vs);
            int lv_va = lp_start;
            int lv_ai = lp_increment;
            int lv_vector;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                UserDataTable<int>.Save0(false, "HD_IntGFor" + lv_vs + lv_va.ToString(), HD_ReturnIntFromRegNum(lv_va, lv_vs));
            }
            lv_va = lp_start;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                lv_vector = UserDataTable<int>.Load0(false, "HD_IntGFor" + lv_vs + lv_va.ToString());
                lp_funcref(lv_vector);//用户填写的所有动作
            }
        }

        /// <summary>
        /// 互动IG_返回Int组中随机元素.返回指定Int组中的随机Int.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称,建议与字段、变量名一致,数组使用时字符应写成:组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnRandomIntFromIntGFunc(string lp_gs)
        {
            //Variable Declarations
            int lv_num;
            int lv_a;
            int lv_c = 0;
            //Variable Initialization
            lv_num = HD_ReturnIntNumMax(lp_gs);
            //Implementation
            if ((lv_num >= 1))
            {
                lv_a = RandomInt(1, lv_num);
                lv_c = HD_ReturnIntFromRegNum(lv_a, lp_gs);
            }
            return lv_c;
        }

        //互动IG_添加Int组到Int组
        //#AUTOVAR(vs, string) = "#PARAM(groupA)";
        //#AUTOVAR(vsb, string) = "#PARAM(groupB)";
        //#AUTOVAR(ae) = HD_ReturnIntNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= 1;
        //#AUTOVAR(ai)= 1;
        //#AUTOVAR(var);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #AUTOVAR(var) = HD_ReturnIntFromRegNum(#AUTOVAR(va), #AUTOVAR(vs));
        //    HD_AddIntToGroup(#AUTOVAR(var), #AUTOVAR(vsb));
        //}

        /// <summary>
        /// 互动IG_添加Int组到Int组.添加一个Int组A的元素到另一个Int组B,相同Int被认为是同一个.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_groupA"></param>
        /// <param name="lp_groupB"></param>
        public static void HD_AddIntGToIntG(string lp_groupA, string lp_groupB)
        {
            string lv_vsa = lp_groupA;
            string lv_vsb = lp_groupB;
            int lv_ae = HD_ReturnIntNumMax(lv_vsa);
            int lv_va = 1;
            int lv_ai = 1;
            int lv_var;
            for (; ((lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae)); lv_va += lv_ai)
            {
                lv_var = HD_ReturnIntFromRegNum(lv_va, lv_vsa);
                HD_AddIntToGroup(lv_var, lv_vsb);
            }
        }

        //互动IG_从Int组移除Int组
        //#AUTOVAR(vs, string) = "#PARAM(groupA)";
        //#AUTOVAR(vsb, string) = "#PARAM(groupB)";
        //#AUTOVAR(ae) = HD_ReturnIntNumMax(#AUTOVAR(vs));
        //#AUTOVAR(va)= 1;
        //#AUTOVAR(ai)= 1;
        //#AUTOVAR(var);
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #AUTOVAR(va) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #AUTOVAR(va) >= #AUTOVAR(ae)) ) ; #AUTOVAR(va) += #AUTOVAR(ai) ) {
        //    #AUTOVAR(var) = HD_ReturnIntFromRegNum(#AUTOVAR(va), #AUTOVAR(vs));
        //    HD_RemoveInt(#AUTOVAR(var), #AUTOVAR(vsb));
        //}

        /// <summary>
        /// 互动IG_从Int组移除Int组.将Int组A的元素从Int组B中移除,相同Int被认为是同一个.移除使用了"互动I_移除Int",同一个存储区(Int组ID)序号重排,移除时该存储区如有其他操作会排队等待.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_groupA"></param>
        /// <param name="lp_groupB"></param>
        public static void HD_RemoveIntGFromIntG(string lp_groupA, string lp_groupB)
        {
            string lv_vsa = lp_groupA;
            string lv_vsb = lp_groupB;
            int lv_ae = HD_ReturnIntNumMax(lv_vsa);
            int lv_va = 1;
            int lv_ai = 1;
            int lv_var;
            for (; ((lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae)); lv_va += lv_ai)
            {
                lv_var = HD_ReturnIntFromRegNum(lv_va, lv_vsa);
                HD_RemoveInt(lv_var, lv_vsb);
            }
        }

        /// <summary>
        /// 互动IG_移除Int组全部元素.将Int组(Key区)存储的元素全部移除,相同Int被认为是同一个.移除时同一个存储区(Int组ID)序号不进行重排,但该存储区如有其他操作会排队等待.Int组目前不支持赋值其他变量,绝对ID对应绝对Key,可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_key">存储键区,默认填Int组名称</param>
        public static void HD_RemoveIntGAll(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_a;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_num = UserDataTable<int>.Load0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                lv_tagStr = UserDataTable<int>.Load1(true, (lp_key + "HD_IntTag"), lv_a).ToString();
                lv_num -= 1;
                UserDataTable<bool>.Clear0(true, "IfIntGTag" + lv_str + "_" + lv_tagStr);
                UserDataTable<string>.Clear0(true, "HD_IntCV" + lv_str + "_" + lv_tagStr);
                UserDataTable<string>.Clear0(true, "HD_IntState" + lv_str + "_" + lv_tagStr);
                UserDataTable<int>.Save0(true, (lp_key + "HD_IntNum"), lv_num);
            }
            ThreadWaitSet(true, lv_str, false);
        }

        //--------------------------------------------------------------------------------------------------
        //数字组End
        //--------------------------------------------------------------------------------------------------

        #endregion

        #endregion

        #endregion

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
                        // 1. 将后续元素前移,覆盖当前被删除的元素
                        for (int b = a; b < currentCount; b++)
                        {
                            keyEventFuncrefGroup[key, b] = keyEventFuncrefGroup[key, b + 1];
                        }
                        // 2. 清空最后一个冗余位置(防止内存泄漏或幽灵引用)
                        keyEventFuncrefGroup[key, currentCount] = null;

                        // 3. 总数减一
                        currentCount--;
                        keyEventFuncrefGroupNum[key] = currentCount;
                    }
                    else
                    {
                        // 只有当不匹配时,才检查下一个索引
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
                        // 1. 将后续元素前移,覆盖当前被删除的元素
                        for (int b = a; b < currentCount; b++)
                        {
                            mouseEventFuncrefGroup[key, b] = mouseEventFuncrefGroup[key, b + 1];
                        }

                        // 2. 清空最后一个冗余位置(防止内存泄漏或幽灵引用)
                        mouseEventFuncrefGroup[key, currentCount] = null;

                        // 3. 总数减一
                        currentCount--;
                        mouseEventFuncrefGroupNum[key] = currentCount;
                    }
                    else
                    {
                        // 只有当不匹配时,才检查下一个索引
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
                    {  //跳过不需要处理的键(双击值为-1)
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
        /// 返回序号对应按键(蓄力专用)
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
                    Tell(("鼠标双击距离差:" + lv_s + " <= " + doubleClickRange));
                }
            }
            else
            {
                if ((doubleClickDebug == true))
                {
                    Tell(("鼠标双击距离差:" + lv_s + " > " + doubleClickRange));
                }
            }
            return lv_torf;
        }

        #endregion

        #region 玩家组

        /// <summary>
        /// CurrentUserGroup - 返回当前本地用户组(支持多人联机)
        /// </summary>
        /// <returns></returns>
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

            // 如果没有找到本地用户(可能是初始化阶段或单机模式),默认添加玩家1
            if (pg.players.Count == 0)
            {
                pg.players.Add(1);
            }

            return pg;
        }

        /// <summary>
        /// PlayerGroupNextPlayer - 遍历玩家组
        /// </summary>
        /// <param name="lp_pg"></param>
        /// <param name="lp_current"></param>
        /// <returns></returns>
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

#region 小记

//本库由PC加载时推荐UTF-8(带BOM)编码以及CRLF尾行格式(Unity及MonoGame亦如此)

//C#中实例方法与静态方法在内存都只存储一份,实例方法可使用this等指向实例,若明确不依赖实例则写静态方法为宜(减少以下性能开销)
//1.每次调用实例方法时都需要在调用栈上分配一定的空间来保存方法的局部变量和参数
//2.调用实例方法时会隐式地传递一个this引用指向调用该方法的对象实例(该引用在方法内部可用来访问对象的字段和方法)

//常量(const关键字修饰的字段)不会每次创建类的实例而重新分配内存,编译时就已确定其值并在程序整个生命周期都不会改变,内存方式相当于静态字段,但它在语义上不同,是编译时常量,而静态字段可在运行时被修改(若不是只读)
//委托(delegate)类型是顶级类型,故不支持Static修饰,但用其声明的委托变量/事件成员(Delegate Variables / Events)可被正常修饰

//‌编译器‌会‌为顶级类型和类成员设定默认访问级别
//显式指定访问修饰符‌是好习惯,可提高代码可读性、明确设计意图,防止因误解默认规则而导致安全隐患或维护困难

// C#默认修饰符
// 顶级类型‌(非嵌套的类、结构、接口、委托)默认访问修饰符是internal‌
// 类、结构体的默认修饰符是internal
// 类、结构体中所有成员默认修饰符是private
// 接口默认修饰符是internal
// 接口成员默认修饰符是public
// 枚举类型及成员默认修饰符是public,并且不允许显式指定其他访问修饰符(因为枚举的设计初衷就是为了提供一组可访问的常量集,若允许设置其他访问修饰符将违背这一初衷)
// 委托的默认修饰符是internal
// 允许不同程序集访问的只有protected、protected internal和public,但前2者仅可访问不同程序集内的派生类
// protected可前插private(提高private访问权限,仅允许访问相同程序集内的派生类,不可跨程序集)
// protected可后跟internal(提高internal访问权限,允许访问不同程序集内的派生类)
// 静态构造函数不允许访问修饰符且不能带有任何参数(默认访问级别是私有的)

//‌Finalize方法:虽然C#允许定义Finalize方法来执行对象销毁前的清理工作,但这种方法通常不推荐使用
//因为它会增加垃圾回收的复杂性和开销,而且无法保证在何时被调用
//在现代C#编程中更推荐使用IDisposable接口和using语句来管理资源

//当类的实例被某个活动对象或静态字段引用,它就不会被垃圾回收,反之引用不存在时进行类的回收(逐步清理)
//‌逐步清理‌:垃圾回收器会递归地检查每个对象的引用情况,并回收整个不可达对象图(所有不再被程序中任何活动对象或静态字段引用的对象组成的集合)所占用的内存
//若X类实例引用了其他对象,而这些对象又引用了其他对象,那么整个引用链上的对象都会被逐步清理掉
//所以C#中的自定义类哪怕没有制作Dispose方法,只需将引用=null即可,但写代码过程依然要尽量避免产生大量GC而降低性能
//当编写的类使用了非托管资源如文件流、数据库连接、图形对象等,应手动实现IDisposable接口并提供Dispose方法
//非托管资源是由操作系统直接管理的资源,不是.NET运行时的一部分,因此.NET垃圾回收器无法自动回收
//StringBuilder是托管类型,但Stream文件流对象(如StreamWriter)使用了非托管资源(如文件句柄)需要手动调用其Dispose或使用using块
//using块:动作末尾当Stream文件流对象被销毁时,Dispose会检查是否已调用Flush,若没有它会自动调用Flush确保所有缓冲数据都被写入到文件或其他Stream文件流中

//静态类的成员必须是静态的,但静态字段可被赋值为实例对象的引用,静态方法内部也可创建类的实例
//静态字段在默认情况下会被初始化为它们的默认值(没赋值直接获取则返回该默认值),对于引用类型默认值是null

//DllImportAttribute常用于从非托管代码中导入函数,这是平台调用(P/Invoke)的一种常见方式
//LibraryImportAttribute是较新特性,在.NET 5及更高版本中引入,用于在编译时生成P/Invoke封送代码而不是在运行时(提高性能并减少启动延迟,无需在运行时解析DLL和函数)

//await关键字只能在async声明的异步函数内用,作用是等待一个异步操作的完成,并且不会阻塞调用线程.

#endregion