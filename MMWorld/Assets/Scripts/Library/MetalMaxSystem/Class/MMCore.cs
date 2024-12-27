#if !(UNITY_EDITOR || UNITY_STANDALONE || NET5_0_OR_GREATER)
#define NETFRAMEWORK
#endif
//#define MonoGame

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Concurrent;
//↓防止与System.Windows.Forms.Timer混淆
using Timer = System.Threading.Timer;
#if UNITY_EDITOR || UNITY_STANDALONE
//Unity编辑器、独立应用程序（不包括Web播放器）
using Mathf = UnityEngine.Mathf;
using Debug = UnityEngine.Debug;
using Vector2F = UnityEngine.Vector2;
using Vector3F = UnityEngine.Vector3;
#else
//其他.Net环境（Framwork4.8、Net8+）
using System.Diagnostics;
//↓可使用.Net中的Debug.WriteLine
using Debug = System.Diagnostics.Debug;
#if WINDOWS || NET8_0_OR_GREATER || NETFRAMEWORK
//↓支持WINDOWS框架下识别硬件标识等（如果依然是灰色，请手动添加或安装程序集）
using System.Management;
using Microsoft.Win32;
using System.Windows;
#endif
#if NETFRAMEWORK
//使用VS2022的NETFRAMEWORK4.8框架时，校准Mathf
using Mathf = System.Math;
#else
using Mathf = System.MathF;
#endif
#if MonoGame
//使用VS2022的MonoGame插件框架时，校准2F3F向量
using Vector2F = Microsoft.Xna.Framework.Vector2;
using Vector3F = Microsoft.Xna.Framework.Vector3;
#else
using Vector2F = System.Numerics.Vector2;
using Vector3F = System.Numerics.Vector3;
#endif
#endif

namespace MetalMaxSystem
{
    /// <summary>
    /// 核心类
    /// </summary>
    public static class MMCore
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

        //其他常量

        //暂无

        //键鼠函数引用上限及单键注册上限

        /// <summary>
        /// 键盘按键句柄上限（句柄范围0~98，无按键-1）
        /// </summary>
        public const int c_keyMax = 98;
        /// <summary>
        /// 每个键盘按键可注册函数上限
        /// </summary>
        public const int c_regKeyMax = 8;
        /// <summary>
        /// 鼠标按键句柄上限（句柄范围1~5，无按键0）
        /// </summary>
        public const int c_mouseMax = 5;
        /// <summary>
        /// 每个鼠标按键可注册函数上限
        /// </summary>
        public const int c_regMouseMax = 24;

        //主副循环入口函数引用上限及单入口注册上限

        /// <summary>
        /// 主副循环入口句柄上限（句柄范围0~9）
        /// </summary>
        private const int c_entryMax = 9;//内部使用，无需给用户使用
        /// <summary>
        /// 每个主副循环入口可注册函数上限
        /// </summary>
        private const int c_regEntryMax = 1;//内部使用，无需给用户使用

        #endregion

        #region "全局和局部变量"（不同作用域下的无属性字段）

        //类只有字段没变量说法，但理论上公有静态字段是该程序在内存中唯一的全局变量，无论类实例化多次或多线程从模板调用，它只生成一次副本直到程序结束才清理
        //而非静态（实例）类每次实例化都复制一份模板去形成多个副本，私有实例字段相当于类的局部变量
        //不标Static则类及其成员在结束时垃圾回收，标Static则副本唯一且程序结束才从内存消失
        //静态局部变量在函数结束时不参与垃圾回收，以便相同函数重复访问
        //静态数据是从模板形成的内存中唯一的可修改副本（不同类同名也不一样，要考虑命名空间和类名路径，无需担心重复）
        //数组元素数量上限均+1是习惯问题，防止某些循环以数组判断时最后退出还+1导致超限

        public static FileWriter fileWriter;

        /// <summary>
        /// 键盘按键已注册数量（每个数组元素算1个，即使它们+=多个委托函数）
        /// </summary>
        private static int[] keyEventFuncrefGroupNum = new int[c_keyMax + 1];//内部使用

        /// <summary>
        /// 鼠标按键的已注册数量（每个数组元素算1个，即使它们+=多个委托函数）
        /// </summary>
        private static int[] mouseEventFuncrefGroupNum = new int[c_mouseMax + 1];//内部使用

        private static bool[] _stopKeyMouseEvent = new bool[Game.c_maxPlayers + 1];
        /// <summary>
        /// 用户按键事件禁用状态（用于过场、剧情对话、特殊技能如禁锢时强制停用用户的按键事件）
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

        /// <summary>
        /// 主副循环每个入口的已注册数量（每个数组元素算1个，即使它们+=多个委托函数）
        /// </summary>
        private static int[] entryEventFuncrefGroupNum = new int[c_entryMax + 1];//内部使用

        private static bool[] _stopEntryEvent = new bool[Game.c_maxPlayers + 1];
        /// <summary>
        /// 主副循环事件禁用状态（用于特殊情况如个人处理队列过多、玩家间未同步时间过长情况下停用用户主副循环事件）
        /// </summary>
        public static bool[] StopEntryEvent
        {
            get
            {
                return _stopEntryEvent;
            }

            set
            {
                _stopEntryEvent = value;
            }
        }

        #region 哈希表&字典声明

        #region 字典（单线程读写，跨线程若只读也可安全使用）

        #region 值类型

        /// <summary>
        /// 全局字典<string,int> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, int> globalDictionaryInt = new Dictionary<string, int>();//内部使用
        /// <summary>
        /// 临时字典<string,int> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, int> localDictionaryInt = new Dictionary<string, int>();//内部使用

        /// <summary>
        /// 全局字典<string,long> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, long> globalDictionaryLong = new Dictionary<string, long>();//内部使用
        /// <summary>
        /// 临时字典<string,long> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, long> localDictionaryLong = new Dictionary<string, long>();//内部使用

        /// <summary>
        /// 全局字典<string,char> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, char> globalDictionaryChar = new Dictionary<string, char>();//内部使用
        /// <summary>
        /// 临时字典<string,char> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, char> localDictionaryChar = new Dictionary<string, char>();//内部使用

        /// <summary>
        /// 全局字典<string,float> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, float> globalDictionaryFloat = new Dictionary<string, float>();//内部使用
        /// <summary>
        /// 临时字典<string,float> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, float> localDictionaryFloat = new Dictionary<string, float>();//内部使用

        /// <summary>
        /// 全局字典<string,double> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, double> globalDictionaryDouble = new Dictionary<string, double>();//内部使用
        /// <summary>
        /// 临时字典<string,double> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, double> localDictionaryDouble = new Dictionary<string, double>();//内部使用

        /// <summary>
        /// 全局字典<string,bool> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, bool> globalDictionaryBool = new Dictionary<string, bool>();//内部使用
        /// <summary>
        /// 临时字典<string,bool> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, bool> localDictionaryBool = new Dictionary<string, bool>();//内部使用

        /// <summary>
        /// 全局字典<string,byte> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, byte> globalDictionaryByte = new Dictionary<string, byte>();//内部使用
        /// <summary>
        /// 临时字典<string,byte> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, byte> localDictionaryByte = new Dictionary<string, byte>();//内部使用

        /// <summary>
        /// 全局字典<string,Vector> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, Vector2F> globalDictionaryVector = new Dictionary<string, Vector2F>();//内部使用
        /// <summary>
        /// 临时字典<string,Vector> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, Vector2F> localDictionaryVector = new Dictionary<string, Vector2F>();//内部使用

        #endregion

        #region 引用类型

        /// <summary>
        /// 全局字典<string,Object> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, object> globalDictionaryObject = new Dictionary<string, object>();//内部使用
        /// <summary>
        /// 临时字典<string,Object> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, object> localDictionaryObject = new Dictionary<string, object>();//内部使用

        /// <summary>
        /// 全局字典<string,Timer> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, Timer> globalDictionaryTimer = new Dictionary<string, Timer>();//内部使用
        /// <summary>
        /// 临时字典<string,Timer> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, Timer> localDictionaryTimer = new Dictionary<string, Timer>();//内部使用

        /// <summary>
        /// 全局字典<string,string> （不排泄，直到程序结束）
        /// </summary>
        private static Dictionary<string, string> globalDictionaryString = new Dictionary<string, string>();//内部使用
        /// <summary>
        /// 临时字典<string,string> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static Dictionary<string, string> localDictionaryString = new Dictionary<string, string>();//内部使用

        #endregion

        #endregion

        #region 哈希表（跨线程）
        /// <summary>
        /// 全局哈希表（不排泄，直到程序结束）
        /// </summary>
        private static Hashtable globalHashTable = new Hashtable();//内部使用
        /// <summary>
        /// 临时哈希表（函数或动作集结束时应手动排泄）
        /// </summary>
        private static Hashtable localHashTable = new Hashtable();//内部使用
        #endregion

        #region 跨线程字典（专为并发访问设计的线程安全集合）

        #region 值类型

        /// <summary>
        /// 全局跨线程字典<string,int> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, int> globalCDictionaryInt = new ConcurrentDictionary<string, int>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,int> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, int> localCDictionaryInt = new ConcurrentDictionary<string, int>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,long> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, long> globalCDictionaryLong = new ConcurrentDictionary<string, long>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,long> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, long> localCDictionaryLong = new ConcurrentDictionary<string, long>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,char> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, char> globalCDictionaryChar = new ConcurrentDictionary<string, char>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,char> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, char> localCDictionaryChar = new ConcurrentDictionary<string, char>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,float> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, float> globalCDictionaryFloat = new ConcurrentDictionary<string, float>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,float> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, float> localCDictionaryFloat = new ConcurrentDictionary<string, float>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,double> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, double> globalCDictionaryDouble = new ConcurrentDictionary<string, double>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,double> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, double> localCDictionaryDouble = new ConcurrentDictionary<string, double>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,bool> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, bool> globalCDictionaryBool = new ConcurrentDictionary<string, bool>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,bool> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, bool> localCDictionaryBool = new ConcurrentDictionary<string, bool>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,byte> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, byte> globalCDictionaryByte = new ConcurrentDictionary<string, byte>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,byte> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, byte> localCDictionaryByte = new ConcurrentDictionary<string, byte>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,Vector> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, Vector2F> globalCDictionaryVector = new ConcurrentDictionary<string, Vector2F>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,Vector> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, Vector2F> localCDictionaryVector = new ConcurrentDictionary<string, Vector2F>();//内部使用

        #endregion

        #region 引用类型

        /// <summary>
        /// 全局跨线程字典<string,Object> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, object> globalCDictionaryObject = new ConcurrentDictionary<string, object>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,Object> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, object> localCDictionaryObject = new ConcurrentDictionary<string, object>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,Timer> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, Timer> globalCDictionaryTimer = new ConcurrentDictionary<string, Timer>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,Timer> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, Timer> localCDictionaryTimer = new ConcurrentDictionary<string, Timer>();//内部使用

        /// <summary>
        /// 全局跨线程字典<string,string> （不排泄，直到程序结束）
        /// </summary>
        private static ConcurrentDictionary<string, string> globalCDictionaryString = new ConcurrentDictionary<string, string>();//内部使用
        /// <summary>
        /// 临时跨线程字典<string,string> （函数或动作集结束时应手动排泄）
        /// </summary>
        private static ConcurrentDictionary<string, string> localCDictionaryString = new ConcurrentDictionary<string, string>();//内部使用

        #endregion

        #endregion

        #endregion

        //声明用于存放键盘、鼠标"按键事件引用类型"委托变量的二维数组集合（单元素也是集合能+=多个委托函数），C#自带委托列表类型能继续存储这些委托类型变量

        /// <summary>
        /// 键盘按键事件引用委托类型变量数组[c_keyMax + 1, c_regKeyMax + 1]，用于自定义委托函数注册
        /// </summary>
        private static KeyMouseEventFuncref[,] keyEventFuncrefGroup = new KeyMouseEventFuncref[c_keyMax + 1, c_regKeyMax + 1];//内部使用
        /// <summary>
        /// 鼠标按键事件引用委托类型变量数组[c_mouseMax + 1, c_regMouseMax + 1]，用于自定义委托函数注册
        /// </summary>
        private static KeyMouseEventFuncref[,] mouseEventFuncrefGroup = new KeyMouseEventFuncref[c_mouseMax + 1, c_regMouseMax + 1];//内部使用

        //声明用于存放"主副循环入口事件引用类型"委托变量二维数组集合

        /// <summary>
        /// 主副循环入口事件引用委托类型变量数组[c_entryMax + 1, c_regEntryMax + 1]，用于自定义委托函数注册
        /// </summary>
        private static EntryEventFuncref[,] entryEventFuncrefGroup = new EntryEventFuncref[c_entryMax + 1, c_regEntryMax + 1];//内部使用

        #endregion

        #region 构造函数

        /// <summary>
        /// 核心类
        /// </summary>
        static MMCore()
        {
            //这里可给字段进行第二次赋值或安排其他动作（字段的首次赋值是在声明时，同一次初始化执行顺序受动作所在上下文影响）
        }

        #endregion

        #region 字段及其属性方法

        //字段及其属性方法（避免不安全读写，private保护和隐藏字段，设计成只允许通过public修饰的属性方法间接去安全读写）
        //本库前缀单个_开头字段表示其拥有属性方法（若有双_开头表示自定义类型如委托）

        private static bool isDataTableTypeSet = false;
        private static bool _dataTableType = false;
        /// <summary>
        /// 数据表类型，为true时全局切换为哈希表（跨线程安全），其他情况默认采用字典（勿跨线程），可通过切换DataTableType来对比读写效率（只允许被改变一次，请在入口或使用数据表前进行设置）
        /// </summary>
        public static bool DataTableType
        {
            get
            {
                return _dataTableType;
            }
            set
            {
                if (!isDataTableTypeSet)
                {
                    _dataTableType = value;
                    isDataTableTypeSet = true;
                }
                else
                {
                    throw new InvalidOperationException("DataTableType can only be set once.");
                }
            }
        }
        /// <summary>
        /// 存储区容错处理当表键值存在时执行线程默认等待的间隔。常用于多线程触发器频繁写值，如大量注册注销动作使存储区数据重排序的，因表正在使用需排队等待完成才给执行下一个。执行原理：将调用该函数的当前线程反复挂起period毫秒，直到动作要写入的存储区闲置
        /// </summary>
        public static int dataTableThreadWaitPeriod = 50;

        /// <summary>
        /// 本地主机编号
        /// </summary>
        public static int LocalID { get; set; }

        private static int _directoryEmptyUserDefIndex = 0;
        /// <summary>
        /// 用户定义的空目录形式，以供内部判断：0是子文件（夹）数量为0，1是目录大小为0，2是前两者必须都符合，如果用户输入错误，本属性方法将纠正为默认值0
        /// </summary>
        public static int DirectoryEmptyUserDefIndex
        {
            get
            {
                return _directoryEmptyUserDefIndex;
            }
            //如果用户输入错误，纠正为默认值0
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

        #region Functions 数学公式

        /// <summary>
        /// 随机角度
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>返回[0,360)之间的随机角度</returns>
        public static double RandomAngle()
        {
            System.Random r = new System.Random(Guid.NewGuid().GetHashCode());
            return 360 * r.NextDouble();
        }

        /// <summary>
        /// 随机实数（不含最大值）
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>返回[min,max)之间的随机实数</returns>
        public static double RandomDouble(double min, double max)
        {
            System.Random r = new System.Random(Guid.NewGuid().GetHashCode());
            if (min <= max)
            {
                return min + (max - min) * r.NextDouble();
            }
            else
            {
                return max + (min - max) * r.NextDouble();
            }
        }

        /// <summary>
        /// 随机整数（不含最大值）
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns>返回[min,max)之间的随机整数</returns>
        public static int RandomInt(int min, int max)
        {
            System.Random r = new System.Random(Guid.NewGuid().GetHashCode());
            if (min <= max)
            {
                return r.Next(min, max);
            }
            else
            {
                return r.Next(max, min);
            }
        }

        /// <summary>
        /// 将Vector3转Vector2（去掉Z轴）
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
        ///  以实数返回二维坐标（pixelX,pixelY）与（a,b）形成的角度（单位：度）
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
            //180除以π（圆周率）的结果等于57.29578
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
        /// 以实数返回三维坐标（pixelX,pixelY,z）与（a,b,c）形成的角度（单位：度）
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
        /// 以实数返回二维点1点2形成的角度（单位：度）
        /// Returns the angle from point 1 to point 2 as a real value, in degrees
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
        /// 以实数返回三维点1点2形成的角度（单位：度）
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
        /// 以实数返回二维向量之间形成的角度（单位：度）
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
        /// 以实数返回三维向量之间形成的角度（单位：度）
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
        /// 以实数返回二维坐标（pixelX,pixelY）与（a,b）形成的距离（单位：m）
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
        /// 以实数返回二维点之间形成的距离（单位：m）
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
        /// 以实数返回二维向量之间形成的距离（单位：m）
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
        /// 以实数返回三维坐标（pixelX,pixelY,z）与（a,b,c）形成的距离（单位：度）
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
        /// 以实数返回三维点之间形成的距离（单位：m）
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

        /// <summary>
        /// 以实数返回三维向量之间形成的距离（单位：m）
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

        #endregion

        #region Functions 通用功能

        /// <summary>
        /// 如果是Unity引擎则使用Debug.Log(contents)，其他情况切换至.NET下的Debug.WriteLine(contents)
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
        /// 如果是Unity引擎则使用Debug.Log(contents)，其他情况切换至.NET下的Debug.WriteLine(contents)
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
        /// 将字符串转换为字节数组，再转成2位16进制字符串格式或转成10进制数字再转为3位8进制字符串格式，以供在Galaxy代码中混肴使用
        /// Galaxy代码会自动转转义8和16位格式字符串（\0及\pixelX）为ASCII值（数字）,再转为控制字符使用
        /// </summary>
        /// <param name="input"></param>
        /// <param name="r">向16进制转换的概率，否则向8进制转换</param>
        /// <returns></returns>
        public static string ConvertStringToHOMixed(string input, double r)
        {
            string result = "";
            //创建一个System.Random对象
            System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
            foreach (byte b in Encoding.UTF8.GetBytes(input))
            {
                //根据随机数和触发概率决定是否执行动作
                if (random.NextDouble() < r)
                {
                    //result += $"\\pixelX{b:X2}";
                    result += string.Format("\\pixelX{0:X2}", b);
                }
                else
                {
                    //result += $"\\0{Convert.ToString(b, 8)}";
                    result += string.Format("\\0{Convert.ToString(b, 8)}", b);
                }
            }
            return result;
        }

        /// <summary>
        /// 将字符串转换为字节数组，再转成10进制数字，再转为3位8进制字符串格式，以供在Galaxy代码中混肴使用
        /// Galaxy代码会自动转转义8和16位格式字符串（\0及\pixelX）为ASCII值（数字）,再转为控制字符使用
        /// 如八进制"\0124"、"\0114"表示十进制的84和76，Galaxy脚本中识别为"T"和"L"
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
        /// 将字符串转换为字节数组，再转成2位16进制字符串格式，以供在Galaxy代码中混肴使用
        /// Galaxy代码会自动转转义8和16位格式字符串（\0及\pixelX）为ASCII值（数字）,再转为控制字符使用
        /// 如十六进制"\x4C"表示十进制的84，Galaxy脚本中识别为"T"
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
            //使用 RegexOptions.Multiline 选项来指定模式应在多个行上进行匹配，并使用 RegexOptions.Singleline 选项来指定模式应在单个连续字符串上进行匹配
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
        /// 文件是否被占用（WIN32 API调用）
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
        /// 文件是否被占用（文件流判断法）
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
        /// 递归方式强制删除目录（进最里层删除文件使目录为空后删除这个空目录，层层递出时重复动作），删除前会去掉文件（夹）的Archive、ReadOnly、Hidden属性以确保删除
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
                //处理每个目录内部的文件（从里层开始删除）
                newInfo.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
                newInfo.Delete();
            }
            //对每个目录处理（从里层开始删除）
            dirInfo.Attributes &= ~(FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden);
            dirInfo.Delete(true);
        }

        /// <summary>
        /// 递归方式强制删除目录（进最里层删除文件使目录为空后删除这个空目录，层层递出时重复动作），删除前会去掉文件（夹）的Archive、ReadOnly、Hidden属性以确保删除
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DelDirectoryRecursively(string dirPath)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            DelDirectoryRecursively(dirInfo);

        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dirInfo"></param>
        /// <returns>删除成功返回真，否则返回假</returns>
        public static bool DelDirectory(DirectoryInfo dirInfo)
        {
            bool torf = false;
            if (dirInfo.Exists)
            {
                dirInfo.Delete(true);
                if (!dirInfo.Exists) { torf = true; }
            }
            return torf;
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns>删除返回真，否则返回假</returns>
        public static bool DelDirectory(string dirPath)
        {
            bool torf = false;
            if (Directory.Exists(dirPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                dirInfo.Delete(true);
                if (!dirInfo.Exists) { torf = true; }
            }
            return torf;
        }

        /// <summary>
        /// 删除文件到回收站功能专用属性，已添加Shell API特性[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        /// </summary>
        /// <param name="lpFileOp"></param>
        /// <returns></returns>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

        /// <summary>
        /// 删除文件到回收站功能专用结构体，已添加特性[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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
        /// 删除文件到回收站功能专用枚举，已添加特性[Flags]
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
        /// 删除文件到回收站
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="torf"></param>
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
                //不确认直接删除（通过或运算符集成准许撤销+不出现任何对话框）
                fileop.fFlags = (ushort)(SHFileOperationFlags.FOF_ALLOWUNDO | SHFileOperationFlags.FOF_NOCONFIRMATION);
            }
            else
            {
                //需要用户确认删除，文件操作属性清空
                fileop.fFlags = 0;
            }
            SHFileOperation(ref fileop);
        }

        /// <summary>
        /// 删除目录到回收站
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
                //不确认直接删除（通过或运算符集成准许撤销+不出现任何对话框）
                fileop.fFlags = (ushort)(SHFileOperationFlags.FOF_ALLOWUNDO | SHFileOperationFlags.FOF_NOCONFIRMATION);
            }
            else
            {
                //需要用户确认删除，文件操作属性清空
                fileop.fFlags = 0;
            }
            SHFileOperation(ref fileop);
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
            //判断给定的路径是否存在,如果不存在则退出
            if (!Directory.Exists(dirPath))
            {
                return 0;
            }
            long len = 0;
            //定义一个DirectoryInfo对象
            DirectoryInfo di = new DirectoryInfo(dirPath);
            //通过GetFiles方法,获取di目录中的所有文件的大小，量越大越慢
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

#if WINDOWS || NET8_0_OR_GREATER || NETFRAMEWORK
        /// <summary>
        /// 取得设备硬盘的卷序列号（在Unity、MonoGame不适用）
        /// </summary>
        /// <param name="diskSymbol">盘符</param>
        /// <returns>成功返回卷序列号，失败返回"uHnIk"</returns>
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
        /// 验证字符串是否为合法文件（夹）名称，可以是虚拟路径（本函数不验证其真实存在）
        /// </summary>
        /// <param name="path">文件（夹）路径全名</param>
        /// <returns></returns>
        public static bool IsDFPath(string path)
        {
            //发现带中文符号会识别不出，为中文符号继续追加（）【】：
            Regex regex = new Regex(
                @"^([a-zA-Z]:\\)([-\u4e00-\u9fa5\w\s.（）【】：~!@#$%^&()\[\]{}+=]+\\?)*$"
            );
            Match result = regex.Match(path);
            return result.Success;
        }

        /// <summary>
        /// 验证字符串路径的文件（夹）是否真实存在
        /// </summary>
        /// <param name="path">文件（夹）路径全名</param>
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
                //如果设置了ReadOnly和Directory，则FileAttributes等于16+1=17，二进制为00001001
                //如果没有设置目录位，则会得到零：
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
            //为了效率，只要验证当前层就可以了
            if (dir.GetFiles().Length + dir.GetDirectories().Length == 0)
            {
                torf = true;
            }
            return torf;
        }

        /// <summary>
        /// 验证路径是否为用户定义的空目录，通过MMCore.DirectoryEmptyUserDefIndex属性可定义空目录形式
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
                    } //以上两者
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
        /// 使用FileWriter写文本每行。写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区
        /// </summary>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区（即写入前清理StringBuilder）,true向缓冲区追加文本</param>
        public static void WriteLine(string value, bool bufferAppend = true)
        {
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
        }
        /// <summary>
        /// 使用FileWriter写文本每行（默认UTF-8）。写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区（直到参数end=true时写入文件，文件若不存在则自动新建）
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区（即写入前清理StringBuilder）,true向缓冲区追加文本</param>
        /// <param name="end">立即写入文件并清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件，true向文件末尾追加文本</param>
        public static void WriteLine(string path, string value, bool bufferAppend = true, bool end = false, bool fileAppend = false)
        {
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
            if (end)
            {
                fileWriter.Close(path, fileAppend, Encoding.UTF8);
            }
        }
        /// <summary>
        /// 使用FileWriter写文本每行。写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区（直到参数end=true时写入文件，文件若不存在则自动新建）
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区（即写入前清理StringBuilder）,true向缓冲区追加文本</param>
        /// <param name="encoding">编码</param>
        /// <param name="end">立即写入文件并清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件，true向文件末尾追加文本</param>
        public static void WriteLine(string path, string value, bool bufferAppend, Encoding encoding, bool end = false, bool fileAppend = false)
        {
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.WriteLine(value, !bufferAppend);
            if (end)
            {
                fileWriter.Close(path, fileAppend, encoding);
            }
        }

        /// <summary>
        /// 使用FileWriter写文本。写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区
        /// </summary>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区（即写入前清理StringBuilder）,true向缓冲区追加文本</param>
        public static void Write(string value, bool bufferAppend = true)
        {
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
        }
        /// <summary>
        /// 使用FileWriter写文本（默认UTF-8）。写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区（直到参数end=true时写入文件，文件若不存在则自动新建）
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区（即写入前清理StringBuilder）,true向缓冲区追加文本</param>
        /// <param name="end">立即写入文件并清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件，true向文件末尾追加文本</param>
        public static void Write(string path, string value, bool bufferAppend = true, bool end = false, bool fileAppend = false)
        {
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
            if (end)
            {
                fileWriter.Close(path, fileAppend, Encoding.UTF8);
            }
        }
        /// <summary>
        /// 使用FileWriter写文本。写入内容暂存在MMCore.fileWriter的StringBuilder类型的Buffer缓冲区（直到参数end=true时写入文件，文件若不存在则自动新建）
        /// </summary>
        /// <param name="path">要写入的文件路径</param>
        /// <param name="value">要写入的字符内容</param>
        /// <param name="bufferAppend">false覆盖缓冲区（即写入前清理StringBuilder）,true向缓冲区追加文本</param>
        /// <param name="encoding">编码</param>
        /// <param name="end">立即写入文件并清理StringBuilder缓冲区</param>
        /// <param name="fileAppend">false覆盖文件，true向文件末尾追加文本</param>
        public static void Write(string path, string value, bool bufferAppend, Encoding encoding, bool end = false, bool fileAppend = false)
        {
            if (fileWriter == null) { fileWriter = new FileWriter(); }
            fileWriter.Write(value, !bufferAppend);
            if (end)
            {
                fileWriter.Close(path, fileAppend, encoding);
            }
        }

        /// <summary>
        /// 立即写文本每行（默认UTF-8），文件若不存在则自动新建，StreamWriter默认缓冲区大小为8192个字节（8KB），满时自动写入文件，本函数使用using代码块，StreamWriter对象被关闭时，缓冲区中的数据也会被写入文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖，true是追加文本</param>
        public static void WriteLineNow(string path, string value, bool append, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, Encoding.UTF8, bufferSize))
            {
                sw.WriteLine(value);
                //sw.Flush(); 不等待sw.Close()即刻写入，对于遍历大量写入来说并不效率，故此时不写
            }
            //using代码块结束，StreamWriter对象被关闭，缓冲区中的数据被写入文件

        }
        /// <summary>
        /// 立即写文本每行，文件若不存在则自动新建，StreamWriter默认缓冲区大小为8192个字节（8KB），满时自动写入文件，本函数使用using代码块，StreamWriter对象被关闭时，缓冲区中的数据也会被写入文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖，true是追加文本</param>
        /// <param name="encoding"></param>
        public static void WriteLineNow(string path, string value, bool append, Encoding encoding, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, encoding, bufferSize))
            {
                sw.WriteLine(value);
                //sw.Flush(); 不等待sw.Close()即刻写入，对于遍历大量写入来说并不效率，故此时不写
            }
            //using代码块结束，StreamWriter对象被关闭，缓冲区中的数据被写入文件

        }

        /// <summary>
        /// 立即写文本（默认UTF-8），文件若不存在则自动新建，StreamWriter默认缓冲区大小为8192个字节（8KB），满时自动写入文件，本函数使用using代码块，StreamWriter对象被关闭时，缓冲区中的数据也会被写入文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖，true是追加文本</param>
        public static void WriteNow(string path, string value, bool append, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, Encoding.UTF8, bufferSize))
            {
                sw.Write(value);
                //sw.Flush(); 不等待sw.Close()即刻写入，对于遍历大量写入来说并不效率，故此时不写
            }
            //using代码块结束，StreamWriter对象被关闭，缓冲区中的数据被写入文件
        }
        /// <summary>
        /// 立即写文本，文件若不存在则自动新建，StreamWriter默认缓冲区大小为8192个字节（8KB），满时自动写入文件，本函数使用using代码块，StreamWriter对象被关闭时，缓冲区中的数据也会被写入文件。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="append">false是覆盖，true是追加文本</param>
        /// <param name="encoding"></param>
        public static void WriteNow(string path, string value, bool append, Encoding encoding, int bufferSize = 8192)
        {
            using (StreamWriter sw = new StreamWriter(path, append, encoding, bufferSize))
            {
                sw.Write(value);
                //sw.Flush(); 不等待sw.Close()即刻写入，对于遍历大量写入来说并不效率，故此时不写
            }
            //using代码块结束，StreamWriter对象被关闭，缓冲区中的数据被写入文件
        }

        /// <summary>
        /// 验证文件大小是否在用户定义的[a,b]范围
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

        /// <summary>
        /// 创建GET请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string CreateGetHttpResponse(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.50";
            try
            {
                HttpWebResponse webresponse = request.GetResponse() as HttpWebResponse;
                using (Stream s = webresponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(s, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return "requestFalse";
            }

        }

        /// <summary>
        /// 创建POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string CreatePostHttpResponse(string url, IDictionary<string, string> parameters, string ContentType = "application/pixelX-www-form-urlencoded")
        {
            HttpWebRequest request;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.50";
            request.ContentType = ContentType;
            //发送POST数据  
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        i++;
                    }
                }
                byte[] data = Encoding.ASCII.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                HttpWebResponse webresponse = request.GetResponse() as HttpWebResponse;
                using (Stream s = webresponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(s, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return "requestFalse";
            }
        }

        /// <summary>
        /// 下载指定网站的指定节点内容到指定目录并保存为自定义文件名
        /// 使用范例：
        /// HtmlDocument doc = new();
        /// doc.LoadHtml(MMCore.CreateGetHttpResponse("https://ac.qq.com/Comic/ComicInfo/id/542330"));
        /// HtmlNode img = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img");
        /// string imgUal = img.Attributes["src"].Value;
        /// MMCore.Download(imgUal, "123.jpg", @"C:\Users\Admin\Desktop\Download\", true);
        /// //MMCore.Tell("下载完成！");
        /// </summary>
        /// <param name="url">浏览器网址</param>
        /// <param name="filename">自定义文件名</param>
        /// <param name="path">下载路径，如 @"C:\Users\Admin\Desktop\Download\"</param>
        /// <param name="bufferAppend">发生文件重复时覆盖</param>
        /// <returns></returns>
        public static bool Download(string url, string filename, string path, bool cover)
        {
            string tempPath = Path.Combine(Path.GetDirectoryName(path), "temp");//确定临时目录全名路径
            string filepath = Path.Combine(path, filename);//确定最终下载文件全名路径
            Directory.CreateDirectory(tempPath);  //创建临时目录
            string tempFile = tempPath + "\\" + filename + ".temp"; //确定临时下载文件全名路径
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);    //临时下载文件存在则删除
            }
            FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            try
            {
                //设置参数
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //发送请求并获取相应回应数据
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                Stream responseStream = response.GetResponseStream();
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    fs.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                }
                responseStream.Close();
                responseStream.Dispose();
            }
            catch (Exception ex)
            {
                Tell(string.Format("错误: {0}", ex.Message));
                return false;
            }
            finally
            {
                fs.Close();
                fs.Dispose();
            }
            try
            {
                File.Move(tempFile, filepath);
            }
            catch
            {
                if (cover) { File.Delete(filepath); File.Move(tempFile, filepath); }
            }
            try
            {
                DelDirectory(tempPath);
            }
            catch
            {
                DelDirectoryRecursively(tempPath);
            }
            return true;
        }

        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
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
        /// 创建目录，若已存在则什么也不干
        /// </summary>
        /// <param name="path"></param>
        public static void CreatDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                DirectoryInfo directory = new DirectoryInfo(path);
                directory.Create();
            }
        }

        /// <summary>
        /// 创建文件，若已存在则什么也不干
        /// </summary>
        /// <param name="filepath"></param>
        public static void CreatFile(string filepath)
        {
            if (!File.Exists(filepath))
            {
                File.Create(filepath);
            }
        }

        /// <summary>
        /// 调用File.WriteAllText将文本内存写入文件（若路径不存在会尝试建立）
        /// </summary>
        /// <param name="fileSavePath"></param>
        /// <param name="content"></param>
        public static void SaveFile(string fileSavePath, string content)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(fileSavePath);

                //如果目录不存在，则创建它
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
        /// 调用File.WriteAllBytes将文本内存写入文件（若路径不存在会尝试建立）
        /// </summary>
        /// <param name="fileSavePath"></param>
        /// <param name="content"></param>
        public static void SaveFile(string fileSavePath, byte[] content)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(fileSavePath);

                //如果目录不存在，则创建它
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
#if WINDOWS || NET8_0_OR_GREATER || NETFRAMEWORK
        /// <summary>
        /// 用WinRAR解压带密码的压缩包
        /// </summary>
        /// <param name="zipFilePath">压缩包路径</param>
        /// <param name="unZipPath">解压后目录的路径</param>
        /// <param name="password">压缩包密码</param>
        /// <returns></returns>
        public static bool UnZip(string zipFilePath, string unZipPath, string password)
        {
            if (!IsOwnWinRAR())
            {
#if !NETFRAMEWORK
                MessageBox.Show("本机并未安装WinRAR,请安装该压缩软件!", "温馨提示");
#else
                MMCore.Tell("本机并未安装WinRAR,请安装该压缩软件!", "温馨提示");
#endif
                return false;
            }

            Process Process1 = new Process();
            Process1.StartInfo.FileName = "WinRAR.exe";
            Process1.StartInfo.CreateNoWindow = true;
            Process1.StartInfo.Arguments = " pixelX -p" + password + " " + zipFilePath + " " + unZipPath;
            Process1.Start();
            if (Process1.HasExited)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 判断系统上是否安装WinRAR
        /// </summary>
        /// <returns></returns>
        public static bool IsOwnWinRAR()
        {
            RegistryKey the_Reg =
                Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\WinRAR.exe");
            return !string.IsNullOrEmpty(the_Reg.GetValue("").ToString());

        }
#endif
#region 弹幕爬取

        //功能出处：https://blog.csdn.net/qq_15505341/article/details/79212070/

        /// <summary>
        /// 获取弹幕信息（本函数待改中请勿使用）
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static string Post(string room)
        {
            string postString = "roomid=" + room + "&token=&csrf_token=我是图中的马赛克";//要发送的数据
            byte[] postData = Encoding.UTF8.GetBytes(postString);//编码，尤其是汉字，事先要看下抓取网页的编码方式  
            string url = @"http://api.live.bilibili.com/ajax/msg";//地址  

            WebClient webClient = new WebClient();
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            webClient.Headers.Add("Content-Type", "application/pixelX-www-form-urlencoded");//采取POST方式必须加的header，如果改为GET方式的话就去掉这句话即可  
            webClient.Headers.Add("Cookie",
                "可耻的马赛克"
                );
            byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
            string srcString = Encoding.UTF8.GetString(responseData);//解码  
            return srcString;
        }

        /// <summary>
        /// 处理弹幕信息为中文（本函数待改中请勿使用）
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static List<string> GetDanMu(string room)
        {
            string danmu = Post(room);
            List<string> list = new List<string>();
            //正则匹配
            foreach (Match item in Regex.Matches(danmu, "text\":\".*?\""))
            {
                //截取字符串，将unicode码转换为中文
                list.Add(Regex.Unescape(item.Value.Substring(7, item.Value.Length - 8)));
            }
            return list;
        }


        #endregion

        #endregion

        #region Functions 数据表功能

        //注：用户与基础数据表通用键名，用户数据表的键名默认添加前缀"HD_"进行区分，用数据表设计其他功能时应再次添加不重复前缀以免与用户数据表键区重叠（参互动管理功能）

        #region 用户快捷数据表（哈希和字典二选一，默认采用字典，建议中途不要再切换否则请直接使用基础数据表）

        //建议使用泛型字典，值与引用类型尽量不混用，引用类型可以互转但值类型尽可能多

        #region 引用类型

        #region Object

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableObjectKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryObjectKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableObjectValueExists(bool place, object value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryObjectValueExists(place, value);
            }

        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object DataTableObjectGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableGetValue(place, key);
                default:
                    return DictionaryObjectGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableObjectClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryObjectClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableObjectClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryObjectClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableObjectClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryObjectClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableObjectClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryObjectClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableObjectClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryObjectClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableObjectRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryObjectRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableObjectSave0(bool place, string key, object val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryObjectSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableObjectSave1(bool place, string key, int lp_1, object val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryObjectSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableObjectSave2(bool place, string key, int lp_1, int lp_2, object val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryObjectSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableObjectSave3(bool place, string key, int lp_1, int lp_2, int lp_3, object val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryObjectSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableObjectSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, object val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryObjectSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object DataTableObjectLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad0(place, key);
                default:
                    return DictionaryObjectLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static object DataTableObjectLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryObjectLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static object DataTableObjectLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryObjectLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static object DataTableObjectLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryObjectLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static object DataTableObjectLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryObjectLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region String

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableStringKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryStringKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableStringValueExists(bool place, string value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryStringValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DataTableStringGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableGetValue(place, key).ToString();
                default:
                    return DictionaryStringGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableStringClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryStringClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableStringClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryStringClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableStringClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryStringClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableStringClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryStringClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableStringClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryStringClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableStringRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryStringRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableStringSave0(bool place, string key, string val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryStringSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableStringSave1(bool place, string key, int lp_1, string val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryStringSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableStringSave2(bool place, string key, int lp_1, int lp_2, string val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryStringSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableStringSave3(bool place, string key, int lp_1, int lp_2, int lp_3, string val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryStringSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableStringSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, string val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryStringSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DataTableStringLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad0(place, key).ToString();
                default:
                    return DictionaryStringLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static string DataTableStringLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad1(place, key, lp_1).ToString();
                default:
                    return DictionaryStringLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static string DataTableStringLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad2(place, key, lp_1, lp_2).ToString();
                default:
                    return DictionaryStringLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static string DataTableStringLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad3(place, key, lp_1, lp_2, lp_3).ToString();
                default:
                    return DictionaryStringLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static string DataTableStringLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4).ToString();
                default:
                    return DictionaryStringLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #endregion

        #region 值类型

        #region Int

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableIntKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryIntKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableIntValueExists(bool place, int value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryIntValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int DataTableIntGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (int)HashTableGetValue(place, key);
                default:
                    return DictionaryIntGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableIntClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryIntClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableIntClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryIntClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableIntClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryIntClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableIntClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryIntClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableIntClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryIntClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableIntRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryIntRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableIntSave0(bool place, string key, int val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryIntSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableIntSave1(bool place, string key, int lp_1, int val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryIntSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableIntSave2(bool place, string key, int lp_1, int lp_2, int val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryIntSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableIntSave3(bool place, string key, int lp_1, int lp_2, int lp_3, int val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryIntSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableIntSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, int val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryIntSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int? DataTableIntLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (int?)HashTableLoad0(place, key);
                default:
                    return DictionaryIntLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static int? DataTableIntLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (int?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryIntLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static int? DataTableIntLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (int?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryIntLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static int? DataTableIntLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (int?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryIntLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static int? DataTableIntLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (int?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryIntLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int DataTableIntLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (int)HashTableLoad0(place, key);
                default:
                    return DictionaryIntLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static int DataTableIntLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (int)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryIntLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static int DataTableIntLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (int)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryIntLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static int DataTableIntLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (int)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryIntLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static int DataTableIntLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (int)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryIntLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region Long

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableLongKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryLongKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableLongValueExists(bool place, long value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryLongValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long DataTableLongGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (long)HashTableGetValue(place, key);
                default:
                    return DictionaryLongGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableLongClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryLongClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableLongClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryLongClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableLongClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryLongClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableLongClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryLongClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableLongClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryLongClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableLongRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryLongRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableLongSave0(bool place, string key, long val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryLongSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableLongSave1(bool place, string key, int lp_1, long val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryLongSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableLongSave2(bool place, string key, int lp_1, int lp_2, long val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryLongSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableLongSave3(bool place, string key, int lp_1, int lp_2, int lp_3, long val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryLongSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableLongSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, long val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryLongSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long? DataTableLongLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (long?)HashTableLoad0(place, key);
                default:
                    return DictionaryLongLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static long? DataTableLongLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (long?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryLongLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static long? DataTableLongLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (long?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryLongLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static long? DataTableLongLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (long?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryLongLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static long? DataTableLongLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (long?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryLongLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long DataTableLongLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (long)HashTableLoad0(place, key);
                default:
                    return DictionaryLongLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static long DataTableLongLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (long)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryLongLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static long DataTableLongLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (long)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryLongLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static long DataTableLongLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (long)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryLongLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static long DataTableLongLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (long)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryLongLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region Char

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableCharKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryCharKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableCharValueExists(bool place, char value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryCharValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char DataTableCharGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (char)HashTableGetValue(place, key);
                default:
                    return DictionaryCharGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableCharClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryCharClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableCharClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryCharClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableCharClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryCharClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableCharClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryCharClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableCharClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryCharClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableCharRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryCharRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableCharSave0(bool place, string key, char val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryCharSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableCharSave1(bool place, string key, int lp_1, char val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryCharSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableCharSave2(bool place, string key, int lp_1, int lp_2, char val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryCharSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableCharSave3(bool place, string key, int lp_1, int lp_2, int lp_3, char val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryCharSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableCharSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, char val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryCharSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char? DataTableCharLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (char?)HashTableLoad0(place, key);
                default:
                    return DictionaryCharLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static char? DataTableCharLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (char?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryCharLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static char? DataTableCharLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (char?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryCharLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static char? DataTableCharLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (char?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryCharLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static char? DataTableCharLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (char?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryCharLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char DataTableCharLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (char)HashTableLoad0(place, key);
                default:
                    return DictionaryCharLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static char DataTableCharLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (char)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryCharLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static char DataTableCharLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (char)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryCharLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static char DataTableCharLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (char)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryCharLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static char DataTableCharLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (char)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryCharLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region Float

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableFloatKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryFloatKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableFloatValueExists(bool place, float value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryFloatValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float DataTableFloatGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (float)HashTableGetValue(place, key);
                default:
                    return DictionaryFloatGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableFloatClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryFloatClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableFloatClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryFloatClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableFloatClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryFloatClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableFloatClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryFloatClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableFloatClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryFloatClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableFloatRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryFloatRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableFloatSave0(bool place, string key, float val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryFloatSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableFloatSave1(bool place, string key, int lp_1, float val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryFloatSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableFloatSave2(bool place, string key, int lp_1, int lp_2, float val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryFloatSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableFloatSave3(bool place, string key, int lp_1, int lp_2, int lp_3, float val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryFloatSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableFloatSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, float val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryFloatSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float? DataTableFloatLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (float?)HashTableLoad0(place, key);
                default:
                    return DictionaryFloatLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static float? DataTableFloatLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (float?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryFloatLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static float? DataTableFloatLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (float?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryFloatLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static float? DataTableFloatLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (float?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryFloatLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static float? DataTableFloatLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (float?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryFloatLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float DataTableFloatLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (float)HashTableLoad0(place, key);
                default:
                    return DictionaryFloatLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static float DataTableFloatLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (float)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryFloatLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static float DataTableFloatLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (float)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryFloatLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static float DataTableFloatLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (float)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryFloatLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static float DataTableFloatLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (float)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryFloatLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region Double

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableDoubleKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryDoubleKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableDoubleValueExists(bool place, double value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryDoubleValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double DataTableDoubleGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (double)HashTableGetValue(place, key);
                default:
                    return DictionaryDoubleGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableDoubleClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryDoubleClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableDoubleClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryDoubleClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableDoubleClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryDoubleClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableDoubleClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryDoubleClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableDoubleClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryDoubleClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableDoubleRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryDoubleRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableDoubleSave0(bool place, string key, double val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryDoubleSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableDoubleSave1(bool place, string key, int lp_1, double val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryDoubleSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableDoubleSave2(bool place, string key, int lp_1, int lp_2, double val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryDoubleSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableDoubleSave3(bool place, string key, int lp_1, int lp_2, int lp_3, double val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryDoubleSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableDoubleSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, double val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryDoubleSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double? DataTableDoubleLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (double?)HashTableLoad0(place, key);
                default:
                    return DictionaryDoubleLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static double? DataTableDoubleLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (double?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryDoubleLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static double? DataTableDoubleLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (double?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryDoubleLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static double? DataTableDoubleLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (double?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryDoubleLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static double? DataTableDoubleLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (double?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryDoubleLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double DataTableDoubleLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (double)HashTableLoad0(place, key);
                default:
                    return DictionaryDoubleLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static double DataTableDoubleLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (double)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryDoubleLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static double DataTableDoubleLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (double)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryDoubleLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static double DataTableDoubleLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (double)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryDoubleLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static double DataTableDoubleLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (double)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryDoubleLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region Bool

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableBoolKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryBoolKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableBoolValueExists(bool place, bool value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryBoolValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableBoolGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool)HashTableGetValue(place, key);
                default:
                    return DictionaryBoolGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableBoolClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryBoolClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableBoolClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryBoolClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableBoolClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryBoolClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableBoolClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryBoolClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableBoolClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryBoolClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableBoolRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryBoolRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableBoolSave0(bool place, string key, bool val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryBoolSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableBoolSave1(bool place, string key, int lp_1, bool val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryBoolSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableBoolSave2(bool place, string key, int lp_1, int lp_2, bool val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryBoolSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableBoolSave3(bool place, string key, int lp_1, int lp_2, int lp_3, bool val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryBoolSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableBoolSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, bool val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryBoolSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool? DataTableBoolLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool?)HashTableLoad0(place, key);
                default:
                    return DictionaryBoolLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static bool? DataTableBoolLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryBoolLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static bool? DataTableBoolLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryBoolLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static bool? DataTableBoolLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryBoolLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static bool? DataTableBoolLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryBoolLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableBoolLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool)HashTableLoad0(place, key);
                default:
                    return DictionaryBoolLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static bool DataTableBoolLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryBoolLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static bool DataTableBoolLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryBoolLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static bool DataTableBoolLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryBoolLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static bool DataTableBoolLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (bool)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryBoolLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region Byte

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableByteKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryByteKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableByteValueExists(bool place, byte value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryByteValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte DataTableByteGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte)HashTableGetValue(place, key);
                default:
                    return DictionaryByteGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableByteClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryByteClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableByteClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryByteClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableByteClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryByteClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableByteClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryByteClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableByteClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryByteClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableByteRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryByteRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableByteSave0(bool place, string key, byte val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryByteSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableByteSave1(bool place, string key, int lp_1, byte val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryByteSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableByteSave2(bool place, string key, int lp_1, int lp_2, byte val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryByteSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableByteSave3(bool place, string key, int lp_1, int lp_2, int lp_3, byte val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryByteSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableByteSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, byte val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryByteSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte? DataTableByteLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte?)HashTableLoad0(place, key);
                default:
                    return DictionaryByteLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static byte? DataTableByteLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryByteLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static byte? DataTableByteLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryByteLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static byte? DataTableByteLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryByteLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static byte? DataTableByteLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryByteLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte DataTableByteLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte)HashTableLoad0(place, key);
                default:
                    return DictionaryByteLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static byte DataTableByteLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryByteLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static byte DataTableByteLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryByteLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static byte DataTableByteLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryByteLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static byte DataTableByteLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (byte)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryByteLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #region Vector2F

        /// <summary>
        /// 判断数据表键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DataTableVectorKeyExists(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableKeyExists(place, key);
                default:
                    return DictionaryVectorKeyExists(place, key);
            }
        }

        /// <summary>
        /// 判断数据表值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DataTableVectorValueExists(bool place, Vector2F value)
        {
            switch (DataTableType)
            {
                case true:
                    return HashTableValueExists(place, value);
                default:
                    return DictionaryVectorValueExists(place, value);
            }
        }

        /// <summary>
        /// 获取数据表键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Vector2F DataTableVectorGetValue(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F)HashTableGetValue(place, key);
                default:
                    return DictionaryVectorGetValue(place, key);
            }
        }

        /// <summary>
        /// 从数据表中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DataTableVectorClear0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear0(place, key);
                    break;
                default:
                    DictionaryVectorClear0(place, key);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DataTableVectorClear1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear1(place, key, lp_1);
                    break;
                default:
                    DictionaryVectorClear1(place, key, lp_1);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DataTableVectorClear2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear2(place, key, lp_1, lp_2);
                    break;
                default:
                    DictionaryVectorClear2(place, key, lp_1, lp_2);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DataTableVectorClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear3(place, key, lp_1, lp_2, lp_3);
                    break;
                default:
                    DictionaryVectorClear3(place, key, lp_1, lp_2, lp_3);
                    break;
            }
        }

        /// <summary>
        /// 从数据表中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DataTableVectorClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
                default:
                    DictionaryVectorClear4(place, key, lp_1, lp_2, lp_3, lp_4);
                    break;
            }
        }

        /// <summary>
        /// 移除数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DataTableVectorRemove(bool place, string key)//内部函数
        {
            switch (DataTableType)
            {
                case true:
                    HashTableRemove(place, key);
                    break;
                default:
                    DictionaryVectorRemove(place, key);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DataTableVectorSave0(bool place, string key, Vector2F val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave0(place, key, val);
                    break;
                default:
                    DictionaryVectorSave0(place, key, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DataTableVectorSave1(bool place, string key, int lp_1, Vector2F val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave1(place, key, lp_1, val);
                    break;
                default:
                    DictionaryVectorSave1(place, key, lp_1, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DataTableVectorSave2(bool place, string key, int lp_1, int lp_2, Vector2F val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave2(place, key, lp_1, lp_2, val);
                    break;
                default:
                    DictionaryVectorSave2(place, key, lp_1, lp_2, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DataTableVectorSave3(bool place, string key, int lp_1, int lp_2, int lp_3, Vector2F val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
                default:
                    DictionaryVectorSave3(place, key, lp_1, lp_2, lp_3, val);
                    break;
            }
        }

        /// <summary>
        /// 保存数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DataTableVectorSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, Vector2F val)
        {
            switch (DataTableType)
            {
                case true:
                    HashTableSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
                default:
                    DictionaryVectorSave4(place, key, lp_1, lp_2, lp_3, lp_4, val);
                    break;
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Vector2F? DataTableVectorLoad0_N(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F?)HashTableLoad0(place, key);
                default:
                    return DictionaryVectorLoad0_N(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static Vector2F? DataTableVectorLoad1_N(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F?)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryVectorLoad1_N(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static Vector2F? DataTableVectorLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F?)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryVectorLoad2_N(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static Vector2F? DataTableVectorLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F?)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryVectorLoad3_N(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static Vector2F? DataTableVectorLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F?)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryVectorLoad4_N(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        /// <summary>
        /// 读取数据表键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F DataTableVectorLoad0(bool place, string key)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F)HashTableLoad0(place, key);
                default:
                    return DictionaryVectorLoad0(place, key);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static Vector2F DataTableVectorLoad1(bool place, string key, int lp_1)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F)HashTableLoad1(place, key, lp_1);
                default:
                    return DictionaryVectorLoad1(place, key, lp_1);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static Vector2F DataTableVectorLoad2(bool place, string key, int lp_1, int lp_2)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F)HashTableLoad2(place, key, lp_1, lp_2);
                default:
                    return DictionaryVectorLoad2(place, key, lp_1, lp_2);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static Vector2F DataTableVectorLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F)HashTableLoad3(place, key, lp_1, lp_2, lp_3);
                default:
                    return DictionaryVectorLoad3(place, key, lp_1, lp_2, lp_3);
            }
        }

        /// <summary>
        /// 读取数据表键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns></returns>
        public static Vector2F DataTableVectorLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            switch (DataTableType)
            {
                case true:
                    return (Vector2F)HashTableLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
                default:
                    return DictionaryVectorLoad4(place, key, lp_1, lp_2, lp_3, lp_4);
            }
        }

        #endregion

        #endregion

        #endregion

        #region 基础数据表（提供哈希、字典、跨线程字典）

        #region 哈希表（任意类型）

        //使用哈希表设计存取任意类型（不支持泛型）

        /// <summary>
        /// 添加哈希表键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=临时</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void HashTableSet(bool place, string key, object val)//内部使用
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
        /// 从哈希表中移除Key。注：移除并不效率，如果要重复使用该键可赋空值）
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void HashTableClear0(bool place, string key)
        {
            HashTableRemove(place, key);
        }

        /// <summary>
        /// 从哈希表中移除Key[]，模拟1维数组。注：移除并不效率，如果要重复使用该键可赋空值）
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void HashTableClear1(bool place, string key, int lp_1)
        {
            HashTableRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从哈希表中移除Key[,]，模拟2维数组。注：移除并不效率，如果要重复使用该键可赋空值）
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void HashTableClear2(bool place, string key, int lp_1, int lp_2)
        {
            HashTableRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从哈希表中移除Key[,,]，模拟3维数组。注：移除并不效率，如果要重复使用该键可赋空值）
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void HashTableClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            HashTableRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从哈希表中移除Key[,,,]，模拟4维数组。注：移除并不效率，如果要重复使用该键可赋空值）
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void HashTableClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            HashTableRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除哈希表键值对。注：移除并不效率，如果要重复使用该键可赋空值）
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void HashTableRemove(bool place, string key)//内部函数
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
        /// 保存哈希表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void HashTableSave1(bool place, string key, int lp_1, object val)
        {
            HashTableSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存哈希表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void HashTableSave2(bool place, string key, int lp_1, int lp_2, object val)
        {
            HashTableSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存哈希表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void HashTableSave3(bool place, string key, int lp_1, int lp_2, int lp_3, object val)
        {
            HashTableSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存哈希表键值对，模拟4维数组
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
            HashTableSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
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
        /// 读取哈希表键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns></returns>
        public static object HashTableLoad1(bool place, string key, int lp_1)
        {
            if ((HashTableKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return HashTableGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取哈希表键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns></returns>
        public static object HashTableLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((HashTableKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return HashTableGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取哈希表键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns></returns>
        public static object HashTableLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((HashTableKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return HashTableGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取哈希表键值对，模拟4维数组
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
            if ((HashTableKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return HashTableGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region 字典（勿跨线程）

        //使用字典设计存取指定类型（采用泛型时尽可能多分类来防止装箱耗时，尽量存到指定类型，引用类型之间的互转除外，但平时大量遍历且常用的也可以拆分出来）

        #region 值类型

        #region Int字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryIntSet(bool place, string key, int val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryInt[key] = val;

                //if (globalDictionaryInt.Contains(key)) 
                //{
                //   globalDictionaryInt.Remove(key);
                //}
                //globalDictionaryInt.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryInt[key] = val;

                //if (localDictionaryInt.Contains(key)) { localDictionaryInt.Remove(key); }
                //localDictionaryInt.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryIntKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryInt.ContainsKey(key); }
            else { return localDictionaryInt.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryIntValueExists(bool place, int value)
        {
            if (place) { return globalDictionaryInt.ContainsValue(value); }
            else { return localDictionaryInt.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int DictionaryIntGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryInt[key]; }
            else { return localDictionaryInt[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryIntClear0(bool place, string key)
        {
            DictionaryIntRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryIntClear1(bool place, string key, int lp_1)
        {
            DictionaryIntRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryIntClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryIntRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryIntClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryIntRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryIntClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryIntRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryIntRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryInt.Remove(key); }
            else { localDictionaryInt.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryIntSave0(bool place, string key, int val)
        {
            DictionaryIntSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryIntSave1(bool place, string key, int lp_1, int val)
        {
            DictionaryIntSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryIntSave2(bool place, string key, int lp_1, int lp_2, int val)
        {
            DictionaryIntSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryIntSave3(bool place, string key, int lp_1, int lp_2, int lp_3, int val)
        {
            DictionaryIntSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryIntSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, int val)
        {
            DictionaryIntSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static int? DictionaryIntLoad0_N(bool place, string key)
        {
            if ((DictionaryIntKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryIntGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static int? DictionaryIntLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static int? DictionaryIntLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static int? DictionaryIntLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static int? DictionaryIntLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0</returns>
        public static int DictionaryIntLoad0(bool place, string key)
        {
            if ((DictionaryIntKeyExists(place, key) == false))
            {
                return 0;
            }
            return DictionaryIntGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0</returns>
        public static int DictionaryIntLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0</returns>
        public static int DictionaryIntLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0</returns>
        public static int DictionaryIntLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0</returns>
        public static int DictionaryIntLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0;
            }
            return DictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Long字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryLongSet(bool place, string key, long val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryLong[key] = val;

                //if (globalDictionaryLong.Contains(key)) 
                //{
                //   globalDictionaryLong.Remove(key);
                //}
                //globalDictionaryLong.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryLong[key] = val;

                //if (localDictionaryLong.Contains(key)) { localDictionaryLong.Remove(key); }
                //localDictionaryLong.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryLongKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryLong.ContainsKey(key); }
            else { return localDictionaryLong.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryLongValueExists(bool place, long value)
        {
            if (place) { return globalDictionaryLong.ContainsValue(value); }
            else { return localDictionaryLong.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long DictionaryLongGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryLong[key]; }
            else { return localDictionaryLong[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryLongClear0(bool place, string key)
        {
            DictionaryLongRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryLongClear1(bool place, string key, int lp_1)
        {
            DictionaryLongRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryLongClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryLongRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryLongClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryLongRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryLongClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryLongRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryLongRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryLong.Remove(key); }
            else { localDictionaryLong.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryLongSave0(bool place, string key, long val)
        {
            DictionaryLongSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryLongSave1(bool place, string key, int lp_1, long val)
        {
            DictionaryLongSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryLongSave2(bool place, string key, int lp_1, int lp_2, long val)
        {
            DictionaryLongSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryLongSave3(bool place, string key, int lp_1, int lp_2, int lp_3, long val)
        {
            DictionaryLongSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryLongSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, long val)
        {
            DictionaryLongSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static long? DictionaryLongLoad0_N(bool place, string key)
        {
            if ((DictionaryLongKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryLongGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static long? DictionaryLongLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static long? DictionaryLongLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static long? DictionaryLongLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static long? DictionaryLongLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0</returns>
        public static long DictionaryLongLoad0(bool place, string key)
        {
            if ((DictionaryLongKeyExists(place, key) == false))
            {
                return 0;
            }
            return DictionaryLongGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0</returns>
        public static long DictionaryLongLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0</returns>
        public static long DictionaryLongLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0</returns>
        public static long DictionaryLongLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0</returns>
        public static long DictionaryLongLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0;
            }
            return DictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Char字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryCharSet(bool place, string key, char val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryChar[key] = val;

                //if (globalDictionaryChar.Contains(key)) 
                //{
                //   globalDictionaryChar.Remove(key);
                //}
                //globalDictionaryChar.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryChar[key] = val;

                //if (localDictionaryChar.Contains(key)) { localDictionaryChar.Remove(key); }
                //localDictionaryChar.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryCharKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryChar.ContainsKey(key); }
            else { return localDictionaryChar.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryCharValueExists(bool place, char value)
        {
            if (place) { return globalDictionaryChar.ContainsValue(value); }
            else { return localDictionaryChar.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char DictionaryCharGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryChar[key]; }
            else { return localDictionaryChar[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryCharClear0(bool place, string key)
        {
            DictionaryCharRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryCharClear1(bool place, string key, int lp_1)
        {
            DictionaryCharRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryCharClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryCharRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryCharClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryCharRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryCharClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryCharRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryCharRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryChar.Remove(key); }
            else { localDictionaryChar.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryCharSave0(bool place, string key, char val)
        {
            DictionaryCharSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryCharSave1(bool place, string key, int lp_1, char val)
        {
            DictionaryCharSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryCharSave2(bool place, string key, int lp_1, int lp_2, char val)
        {
            DictionaryCharSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryCharSave3(bool place, string key, int lp_1, int lp_2, int lp_3, char val)
        {
            DictionaryCharSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryCharSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, char val)
        {
            DictionaryCharSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static char? DictionaryCharLoad0_N(bool place, string key)
        {
            if ((DictionaryCharKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryCharGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static char? DictionaryCharLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static char? DictionaryCharLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static char? DictionaryCharLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static char? DictionaryCharLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回' '</returns>
        public static char DictionaryCharLoad0(bool place, string key)
        {
            if ((DictionaryCharKeyExists(place, key) == false))
            {
                return ' ';
            }
            return DictionaryCharGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回' '</returns>
        public static char DictionaryCharLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return ' ';
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回' '</returns>
        public static char DictionaryCharLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return ' ';
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回' '</returns>
        public static char DictionaryCharLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return ' ';
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回' '</returns>
        public static char DictionaryCharLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return ' ';
            }
            return DictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Float字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryFloatSet(bool place, string key, float val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryFloat[key] = val;

                //if (globalDictionaryFloat.Contains(key)) 
                //{
                //   globalDictionaryFloat.Remove(key);
                //}
                //globalDictionaryFloat.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryFloat[key] = val;

                //if (localDictionaryFloat.Contains(key)) { localDictionaryFloat.Remove(key); }
                //localDictionaryFloat.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryFloatKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryFloat.ContainsKey(key); }
            else { return localDictionaryFloat.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryFloatValueExists(bool place, float value)
        {
            if (place) { return globalDictionaryFloat.ContainsValue(value); }
            else { return localDictionaryFloat.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float DictionaryFloatGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryFloat[key]; }
            else { return localDictionaryFloat[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryFloatClear0(bool place, string key)
        {
            DictionaryFloatRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryFloatClear1(bool place, string key, int lp_1)
        {
            DictionaryFloatRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryFloatClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryFloatRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryFloatClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryFloatRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryFloatClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryFloatRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryFloatRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryFloat.Remove(key); }
            else { localDictionaryFloat.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryFloatSave0(bool place, string key, float val)
        {
            DictionaryFloatSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryFloatSave1(bool place, string key, int lp_1, float val)
        {
            DictionaryFloatSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryFloatSave2(bool place, string key, int lp_1, int lp_2, float val)
        {
            DictionaryFloatSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryFloatSave3(bool place, string key, int lp_1, int lp_2, int lp_3, float val)
        {
            DictionaryFloatSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryFloatSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, float val)
        {
            DictionaryFloatSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static float? DictionaryFloatLoad0_N(bool place, string key)
        {
            if ((DictionaryFloatKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryFloatGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static float? DictionaryFloatLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static float? DictionaryFloatLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static float? DictionaryFloatLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static float? DictionaryFloatLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float DictionaryFloatLoad0(bool place, string key)
        {
            if ((DictionaryFloatKeyExists(place, key) == false))
            {
                return 0.0f;
            }
            return DictionaryFloatGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float DictionaryFloatLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0.0f;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float DictionaryFloatLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0.0f;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float DictionaryFloatLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0.0f;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float DictionaryFloatLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0.0f;
            }
            return DictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Double字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryDoubleSet(bool place, string key, double val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryDouble[key] = val;

                //if (globalDictionaryDouble.Contains(key)) 
                //{
                //   globalDictionaryDouble.Remove(key);
                //}
                //globalDictionaryDouble.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryDouble[key] = val;

                //if (localDictionaryDouble.Contains(key)) { localDictionaryDouble.Remove(key); }
                //localDictionaryDouble.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryDoubleKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryDouble.ContainsKey(key); }
            else { return localDictionaryDouble.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryDoubleValueExists(bool place, double value)
        {
            if (place) { return globalDictionaryDouble.ContainsValue(value); }
            else { return localDictionaryDouble.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double DictionaryDoubleGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryDouble[key]; }
            else { return localDictionaryDouble[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryDoubleClear0(bool place, string key)
        {
            DictionaryDoubleRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryDoubleClear1(bool place, string key, int lp_1)
        {
            DictionaryDoubleRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryDoubleClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryDoubleRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryDoubleClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryDoubleRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryDoubleClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryDoubleRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryDoubleRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryDouble.Remove(key); }
            else { localDictionaryDouble.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryDoubleSave0(bool place, string key, double val)
        {
            DictionaryDoubleSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryDoubleSave1(bool place, string key, int lp_1, double val)
        {
            DictionaryDoubleSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryDoubleSave2(bool place, string key, int lp_1, int lp_2, double val)
        {
            DictionaryDoubleSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryDoubleSave3(bool place, string key, int lp_1, int lp_2, int lp_3, double val)
        {
            DictionaryDoubleSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryDoubleSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, double val)
        {
            DictionaryDoubleSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static double? DictionaryDoubleLoad0_N(bool place, string key)
        {
            if (DictionaryDoubleKeyExists(place, key) == false)
            {
                return null;
            }
            return DictionaryDoubleGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static double? DictionaryDoubleLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static double? DictionaryDoubleLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static double? DictionaryDoubleLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static double? DictionaryDoubleLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0.0</returns>
        public static double DictionaryDoubleLoad0(bool place, string key)
        {
            if (DictionaryDoubleKeyExists(place, key) == false)
            {
                return 0.0;
            }
            return DictionaryDoubleGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0.0</returns>
        public static double DictionaryDoubleLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0.0;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0.0</returns>
        public static double DictionaryDoubleLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0.0;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0.0</returns>
        public static double DictionaryDoubleLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0.0;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0.0</returns>
        public static double DictionaryDoubleLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0.0;
            }
            return DictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Bool字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryBoolSet(bool place, string key, bool val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryBool[key] = val;

                //if (globalDictionaryBool.Contains(key)) 
                //{
                //   globalDictionaryBool.Remove(key);
                //}
                //globalDictionaryBool.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryBool[key] = val;

                //if (localDictionaryBool.Contains(key)) { localDictionaryBool.Remove(key); }
                //localDictionaryBool.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryBoolKeyExists(bool place, string key)
        {
            if (place)
            {
                return globalDictionaryBool.ContainsKey(key);
            }
            else { return localDictionaryBool.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryBoolValueExists(bool place, bool value)
        {
            if (place) { return globalDictionaryBool.ContainsValue(value); }
            else { return localDictionaryBool.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryBoolGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryBool[key]; }
            else { return localDictionaryBool[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryBoolClear0(bool place, string key)
        {
            DictionaryBoolRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryBoolClear1(bool place, string key, int lp_1)
        {
            DictionaryBoolRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryBoolClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryBoolRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryBoolClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryBoolRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryBoolClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryBoolRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryBoolRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryBool.Remove(key); }
            else { localDictionaryBool.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryBoolSave0(bool place, string key, bool val)
        {
            DictionaryBoolSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryBoolSave1(bool place, string key, int lp_1, bool val)
        {
            DictionaryBoolSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryBoolSave2(bool place, string key, int lp_1, int lp_2, bool val)
        {
            DictionaryBoolSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryBoolSave3(bool place, string key, int lp_1, int lp_2, int lp_3, bool val)
        {
            DictionaryBoolSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryBoolSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, bool val)
        {
            DictionaryBoolSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static bool? DictionaryBoolLoad0_N(bool place, string key)
        {
            if ((DictionaryBoolKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryBoolGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static bool? DictionaryBoolLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static bool? DictionaryBoolLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static bool? DictionaryBoolLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static bool? DictionaryBoolLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回false</returns>
        public static bool DictionaryBoolLoad0(bool place, string key)
        {
            if ((DictionaryBoolKeyExists(place, key) == false))
            {
                return false;
            }
            return DictionaryBoolGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回false</returns>
        public static bool DictionaryBoolLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return false;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回false</returns>
        public static bool DictionaryBoolLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return false;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回false</returns>
        public static bool DictionaryBoolLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return false;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回false</returns>
        public static bool DictionaryBoolLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return false;
            }
            return DictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Byte字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryByteSet(bool place, string key, byte val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryByte[key] = val;

                //if (globalDictionaryByte.Contains(key)) 
                //{
                //   globalDictionaryByte.Remove(key);
                //}
                //globalDictionaryByte.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryByte[key] = val;

                //if (localDictionaryByte.Contains(key)) { localDictionaryByte.Remove(key); }
                //localDictionaryByte.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryByteKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryByte.ContainsKey(key); }
            else { return localDictionaryByte.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryByteValueExists(bool place, byte value)
        {
            if (place) { return globalDictionaryByte.ContainsValue(value); }
            else { return localDictionaryByte.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte DictionaryByteGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryByte[key]; }
            else { return localDictionaryByte[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryByteClear0(bool place, string key)
        {
            DictionaryByteRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryByteClear1(bool place, string key, int lp_1)
        {
            DictionaryByteRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryByteClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryByteRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryByteClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryByteRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryByteClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryByteRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryByteRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryByte.Remove(key); }
            else { localDictionaryByte.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryByteSave0(bool place, string key, byte val)
        {
            DictionaryByteSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryByteSave1(bool place, string key, int lp_1, byte val)
        {
            DictionaryByteSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryByteSave2(bool place, string key, int lp_1, int lp_2, byte val)
        {
            DictionaryByteSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryByteSave3(bool place, string key, int lp_1, int lp_2, int lp_3, byte val)
        {
            DictionaryByteSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryByteSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, byte val)
        {
            DictionaryByteSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static byte? DictionaryByteLoad0_N(bool place, string key)
        {
            if ((DictionaryByteKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryByteGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static byte? DictionaryByteLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static byte? DictionaryByteLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static byte? DictionaryByteLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static byte? DictionaryByteLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0</returns>
        public static byte DictionaryByteLoad0(bool place, string key)
        {
            if ((DictionaryByteKeyExists(place, key) == false))
            {
                return 0;
            }
            return DictionaryByteGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0</returns>
        public static byte DictionaryByteLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0</returns>
        public static byte DictionaryByteLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0</returns>
        public static byte DictionaryByteLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0</returns>
        public static byte DictionaryByteLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0;
            }
            return DictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Vector2F字典

        //C#中结构体是值类型

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryVectorSet(bool place, string key, Vector2F val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryVector[key] = val;

                //if (globalDictionaryVector.Contains(key)) 
                //{
                //   globalDictionaryVector.Remove(key);
                //}
                //globalDictionaryVector.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryVector[key] = val;

                //if (localDictionaryVector.Contains(key)) { localDictionaryVector.Remove(key); }
                //localDictionaryVector.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryVectorKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryVector.ContainsKey(key); }
            else { return localDictionaryVector.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryVectorValueExists(bool place, Vector2F value)
        {
            if (place) { return globalDictionaryVector.ContainsValue(value); }
            else { return localDictionaryVector.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Vector2F DictionaryVectorGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryVector[key]; }
            else { return localDictionaryVector[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryVectorClear0(bool place, string key)
        {
            DictionaryVectorRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryVectorClear1(bool place, string key, int lp_1)
        {
            DictionaryVectorRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryVectorClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryVectorRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryVectorClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryVectorRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryVectorClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryVectorRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryVectorRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryVector.Remove(key); }
            else { localDictionaryVector.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryVectorSave0(bool place, string key, Vector2F val)
        {
            DictionaryVectorSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryVectorSave1(bool place, string key, int lp_1, Vector2F val)
        {
            DictionaryVectorSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryVectorSave2(bool place, string key, int lp_1, int lp_2, Vector2F val)
        {
            DictionaryVectorSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryVectorSave3(bool place, string key, int lp_1, int lp_2, int lp_3, Vector2F val)
        {
            DictionaryVectorSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryVectorSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, Vector2F val)
        {
            DictionaryVectorSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? DictionaryVectorLoad0_N(bool place, string key)
        {
            if ((DictionaryVectorKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryVectorGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? DictionaryVectorLoad1_N(bool place, string key, int lp_1)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? DictionaryVectorLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? DictionaryVectorLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? DictionaryVectorLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F DictionaryVectorLoad0(bool place, string key)
        {
            if ((DictionaryVectorKeyExists(place, key) == false))
            {
                return new Vector2F(0, 0);
            }
            return DictionaryVectorGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F DictionaryVectorLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F DictionaryVectorLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F DictionaryVectorLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F DictionaryVectorLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return DictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #endregion

        #region 引用类型

        #region Object字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryObjectSet(bool place, string key, object val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryObject[key] = val;

                //if (globalDictionaryObject.Contains(key)) 
                //{
                //   globalDictionaryObject.Remove(key);
                //}
                //globalDictionaryObject.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryObject[key] = val;

                //if (localDictionaryObject.Contains(key)) { localDictionaryObject.Remove(key); }
                //localDictionaryObject.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryObjectKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryObject.ContainsKey(key); }
            else { return localDictionaryObject.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryObjectValueExists(bool place, object value)
        {
            if (place) { return globalDictionaryObject.ContainsValue(value); }
            else { return localDictionaryObject.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object DictionaryObjectGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryObject[key]; }
            else { return localDictionaryObject[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryObjectClear0(bool place, string key)
        {
            DictionaryObjectRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryObjectClear1(bool place, string key, int lp_1)
        {
            DictionaryObjectRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryObjectClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryObjectRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryObjectClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryObjectRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryObjectClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryObjectRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryObjectRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryObject.Remove(key); }
            else { localDictionaryObject.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryObjectSave0(bool place, string key, object val)
        {
            DictionaryObjectSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryObjectSave1(bool place, string key, int lp_1, object val)
        {
            DictionaryObjectSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryObjectSave2(bool place, string key, int lp_1, int lp_2, object val)
        {
            DictionaryObjectSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryObjectSave3(bool place, string key, int lp_1, int lp_2, int lp_3, object val)
        {
            DictionaryObjectSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryObjectSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, object val)
        {
            DictionaryObjectSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static object DictionaryObjectLoad0(bool place, string key)
        {
            if ((DictionaryObjectKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryObjectGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static object DictionaryObjectLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryObjectGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static object DictionaryObjectLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryObjectGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static object DictionaryObjectLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryObjectGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static object DictionaryObjectLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryObjectGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region String字典

        //C#中字符串是引用类型

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void DictionaryStringSet(bool place, string key, string val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalDictionaryString[key] = val;

                //if (globalDictionaryString.Contains(key)) 
                //{
                //   globalDictionaryString.Remove(key);
                //}
                //globalDictionaryString.Add(key, val);

            }
            else
            {
                //存入局部字典
                localDictionaryString[key] = val;

                //if (localDictionaryString.Contains(key)) { localDictionaryString.Remove(key); }
                //localDictionaryString.Add(key, val);
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DictionaryStringKeyExists(bool place, string key)
        {
            if (place) { return globalDictionaryString.ContainsKey(key); }
            else { return localDictionaryString.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool DictionaryStringValueExists(bool place, string value)
        {
            if (place) { return globalDictionaryString.ContainsValue(value); }
            else { return localDictionaryString.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DictionaryStringGetValue(bool place, string key)
        {
            if (place) { return globalDictionaryString[key]; }
            else { return localDictionaryString[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void DictionaryStringClear0(bool place, string key)
        {
            DictionaryStringRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void DictionaryStringClear1(bool place, string key, int lp_1)
        {
            DictionaryStringRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void DictionaryStringClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryStringRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void DictionaryStringClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryStringRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void DictionaryStringClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryStringRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static void DictionaryStringRemove(bool place, string key)//内部函数
        {
            if (place) { globalDictionaryString.Remove(key); }
            else { localDictionaryString.Remove(key); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void DictionaryStringSave0(bool place, string key, string val)
        {
            DictionaryStringSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void DictionaryStringSave1(bool place, string key, int lp_1, string val)
        {
            DictionaryStringSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void DictionaryStringSave2(bool place, string key, int lp_1, int lp_2, string val)
        {
            DictionaryStringSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void DictionaryStringSave3(bool place, string key, int lp_1, int lp_2, int lp_3, string val)
        {
            DictionaryStringSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void DictionaryStringSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, string val)
        {
            DictionaryStringSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static string DictionaryStringLoad0(bool place, string key)
        {
            if ((DictionaryStringKeyExists(place, key) == false))
            {
                return null;
            }
            return DictionaryStringGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static string DictionaryStringLoad1(bool place, string key, int lp_1)
        {
            if ((DictionaryStringKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return DictionaryStringGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static string DictionaryStringLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((DictionaryStringKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return DictionaryStringGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static string DictionaryStringLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((DictionaryStringKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return DictionaryStringGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static string DictionaryStringLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((DictionaryStringKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return DictionaryStringGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #endregion

        #endregion

        #region 跨线程字典

        //使用字典设计存取指定类型

        #region 值类型

        #region Int字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryIntSet(bool place, string key, int val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryInt[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryInt[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryIntKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryInt.ContainsKey(key); }
            else { return localCDictionaryInt.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryIntValueExists(bool place, string key, int value)
        {
            bool torf;
            int tempValue = 0;
            if (place) { torf = globalCDictionaryInt.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryInt.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int CDictionaryIntGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryInt[key]; }
            else { return localCDictionaryInt[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryIntClear0(bool place, string key)
        {
            CDictionaryIntRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryIntClear1(bool place, string key, int lp_1)
        {
            CDictionaryIntRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryIntClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryIntRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryIntClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryIntRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryIntClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryIntRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryIntRemove(bool place, string key)//内部函数
        {
            int torf = 0;
            if (place) { return globalCDictionaryInt.TryRemove(key, out torf); }
            else { return localCDictionaryInt.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryIntSave0(bool place, string key, int val)
        {
            CDictionaryIntSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryIntSave1(bool place, string key, int lp_1, int val)
        {
            CDictionaryIntSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryIntSave2(bool place, string key, int lp_1, int lp_2, int val)
        {
            CDictionaryIntSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryIntSave3(bool place, string key, int lp_1, int lp_2, int lp_3, int val)
        {
            CDictionaryIntSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryIntSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, int val)
        {
            CDictionaryIntSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static int? CDictionaryIntLoad0_N(bool place, string key)
        {
            if ((CDictionaryIntKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryIntGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static int? CDictionaryIntLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static int? CDictionaryIntLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static int? CDictionaryIntLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static int? CDictionaryIntLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0</returns>
        public static int CDictionaryIntLoad0(bool place, string key)
        {
            if ((CDictionaryIntKeyExists(place, key) == false))
            {
                return 0;
            }
            return CDictionaryIntGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0</returns>
        public static int CDictionaryIntLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0</returns>
        public static int CDictionaryIntLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0</returns>
        public static int CDictionaryIntLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0</returns>
        public static int CDictionaryIntLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryIntKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryIntGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Long字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryLongSet(bool place, string key, long val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryLong[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryLong[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryLongKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryLong.ContainsKey(key); }
            else { return localCDictionaryLong.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryLongValueExists(bool place, string key, long value)
        {
            bool torf;
            long tempValue = 0L;
            if (place) { torf = globalCDictionaryLong.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryLong.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long CDictionaryLongGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryLong[key]; }
            else { return localCDictionaryLong[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryLongClear0(bool place, string key)
        {
            CDictionaryLongRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryLongClear1(bool place, string key, int lp_1)
        {
            CDictionaryLongRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryLongClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryLongRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryLongClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryLongRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryLongClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryLongRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryLongRemove(bool place, string key)//内部函数
        {
            long torf = 0L;
            if (place) { return globalCDictionaryLong.TryRemove(key, out torf); }
            else { return localCDictionaryLong.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryLongSave0(bool place, string key, long val)
        {
            CDictionaryLongSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryLongSave1(bool place, string key, int lp_1, long val)
        {
            CDictionaryLongSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryLongSave2(bool place, string key, int lp_1, int lp_2, long val)
        {
            CDictionaryLongSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryLongSave3(bool place, string key, int lp_1, int lp_2, int lp_3, long val)
        {
            CDictionaryLongSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryLongSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, long val)
        {
            CDictionaryLongSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static long? CDictionaryLongLoad0_N(bool place, string key)
        {
            if ((CDictionaryLongKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryLongGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static long? CDictionaryLongLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static long? CDictionaryLongLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static long? CDictionaryLongLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static long? CDictionaryLongLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0</returns>
        public static long CDictionaryLongLoad0(bool place, string key)
        {
            if ((CDictionaryLongKeyExists(place, key) == false))
            {
                return 0;
            }
            return CDictionaryLongGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0</returns>
        public static long CDictionaryLongLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0</returns>
        public static long CDictionaryLongLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0</returns>
        public static long CDictionaryLongLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0</returns>
        public static long CDictionaryLongLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryLongKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryLongGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Char字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryCharSet(bool place, string key, char val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryChar[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryChar[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryCharKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryChar.ContainsKey(key); }
            else { return localCDictionaryChar.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryCharValueExists(bool place, string key, char value)
        {
            bool torf;
            //char tempValue = '\0';
            char tempValue = new char();//注意未初始化值使用会导致编译错误
            if (place) { torf = globalCDictionaryChar.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryChar.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static char CDictionaryCharGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryChar[key]; }
            else { return localCDictionaryChar[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryCharClear0(bool place, string key)
        {
            CDictionaryCharRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryCharClear1(bool place, string key, int lp_1)
        {
            CDictionaryCharRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryCharClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryCharRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryCharClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryCharRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryCharClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryCharRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryCharRemove(bool place, string key)//内部函数
        {
            char torf = new char();//注意未初始化值使用会导致编译错误
            if (place) { return globalCDictionaryChar.TryRemove(key, out torf); }
            else { return localCDictionaryChar.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryCharSave0(bool place, string key, char val)
        {
            CDictionaryCharSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryCharSave1(bool place, string key, int lp_1, char val)
        {
            CDictionaryCharSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryCharSave2(bool place, string key, int lp_1, int lp_2, char val)
        {
            CDictionaryCharSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryCharSave3(bool place, string key, int lp_1, int lp_2, int lp_3, char val)
        {
            CDictionaryCharSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryCharSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, char val)
        {
            CDictionaryCharSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static char? CDictionaryCharLoad0_N(bool place, string key)
        {
            if ((CDictionaryCharKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryCharGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static char? CDictionaryCharLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static char? CDictionaryCharLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static char? CDictionaryCharLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static char? CDictionaryCharLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回' '</returns>
        public static char CDictionaryCharLoad0(bool place, string key)
        {
            if ((CDictionaryCharKeyExists(place, key) == false))
            {
                return ' ';
            }
            return CDictionaryCharGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回' '</returns>
        public static char CDictionaryCharLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return ' ';
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回' '</returns>
        public static char CDictionaryCharLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return ' ';
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回' '</returns>
        public static char CDictionaryCharLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return ' ';
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回' '</returns>
        public static char CDictionaryCharLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryCharKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return ' ';
            }
            return CDictionaryCharGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Float字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryFloatSet(bool place, string key, float val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryFloat[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryFloat[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryFloatKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryFloat.ContainsKey(key); }
            else { return localCDictionaryFloat.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryFloatValueExists(bool place, string key, float value)
        {
            bool torf;
            float tempValue = 0f;
            if (place) { torf = globalCDictionaryFloat.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryFloat.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static float CDictionaryFloatGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryFloat[key]; }
            else { return localCDictionaryFloat[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryFloatClear0(bool place, string key)
        {
            CDictionaryFloatRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryFloatClear1(bool place, string key, int lp_1)
        {
            CDictionaryFloatRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryFloatClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryFloatRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryFloatClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryFloatRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryFloatClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryFloatRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryFloatRemove(bool place, string key)//内部函数
        {
            float torf = 0f;
            if (place) { return globalCDictionaryFloat.TryRemove(key, out torf); }
            else { return localCDictionaryFloat.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryFloatSave0(bool place, string key, float val)
        {
            CDictionaryFloatSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryFloatSave1(bool place, string key, int lp_1, float val)
        {
            CDictionaryFloatSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryFloatSave2(bool place, string key, int lp_1, int lp_2, float val)
        {
            CDictionaryFloatSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryFloatSave3(bool place, string key, int lp_1, int lp_2, int lp_3, float val)
        {
            CDictionaryFloatSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryFloatSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, float val)
        {
            CDictionaryFloatSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static float? CDictionaryFloatLoad0_N(bool place, string key)
        {
            if ((CDictionaryFloatKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryFloatGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static float? CDictionaryFloatLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static float? CDictionaryFloatLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static float? CDictionaryFloatLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static float? CDictionaryFloatLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float CDictionaryFloatLoad0(bool place, string key)
        {
            if ((CDictionaryFloatKeyExists(place, key) == false))
            {
                return 0.0f;
            }
            return CDictionaryFloatGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float CDictionaryFloatLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0.0f;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float CDictionaryFloatLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0.0f;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float CDictionaryFloatLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0.0f;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0.0f</returns>
        public static float CDictionaryFloatLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryFloatKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0.0f;
            }
            return CDictionaryFloatGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Double字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryDoubleSet(bool place, string key, double val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryDouble[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryDouble[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryDoubleKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryDouble.ContainsKey(key); }
            else { return localCDictionaryDouble.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryDoubleValueExists(bool place, string key, double value)
        {
            bool torf;
            double tempValue = 0d;
            if (place) { torf = globalCDictionaryDouble.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryDouble.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double CDictionaryDoubleGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryDouble[key]; }
            else { return localCDictionaryDouble[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryDoubleClear0(bool place, string key)
        {
            CDictionaryDoubleRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryDoubleClear1(bool place, string key, int lp_1)
        {
            CDictionaryDoubleRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryDoubleClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryDoubleRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryDoubleClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryDoubleRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryDoubleClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryDoubleRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryDoubleRemove(bool place, string key)//内部函数
        {
            double torf = 0d;
            if (place) { return globalCDictionaryDouble.TryRemove(key, out torf); }
            else { return localCDictionaryDouble.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryDoubleSave0(bool place, string key, double val)
        {
            CDictionaryDoubleSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryDoubleSave1(bool place, string key, int lp_1, double val)
        {
            CDictionaryDoubleSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryDoubleSave2(bool place, string key, int lp_1, int lp_2, double val)
        {
            CDictionaryDoubleSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryDoubleSave3(bool place, string key, int lp_1, int lp_2, int lp_3, double val)
        {
            CDictionaryDoubleSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryDoubleSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, double val)
        {
            CDictionaryDoubleSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static double? CDictionaryDoubleLoad0_N(bool place, string key)
        {
            if (CDictionaryDoubleKeyExists(place, key) == false)
            {
                return null;
            }
            return CDictionaryDoubleGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static double? CDictionaryDoubleLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static double? CDictionaryDoubleLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static double? CDictionaryDoubleLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static double? CDictionaryDoubleLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0.0</returns>
        public static double CDictionaryDoubleLoad0(bool place, string key)
        {
            if (CDictionaryDoubleKeyExists(place, key) == false)
            {
                return 0.0;
            }
            return CDictionaryDoubleGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0.0</returns>
        public static double CDictionaryDoubleLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0.0;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0.0</returns>
        public static double CDictionaryDoubleLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0.0;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0.0</returns>
        public static double CDictionaryDoubleLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0.0;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0.0</returns>
        public static double CDictionaryDoubleLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryDoubleKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0.0;
            }
            return CDictionaryDoubleGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Bool字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryBoolSet(bool place, string key, bool val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryBool[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryBool[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryBoolKeyExists(bool place, string key)
        {
            if (place)
            {
                return globalCDictionaryBool.ContainsKey(key);
            }
            else { return localCDictionaryBool.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryBoolValueExists(bool place, string key, bool value)
        {
            bool torf;
            bool tempValue = false;
            if (place) { torf = globalCDictionaryBool.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryBool.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryBoolGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryBool[key]; }
            else { return localCDictionaryBool[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryBoolClear0(bool place, string key)
        {
            CDictionaryBoolRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryBoolClear1(bool place, string key, int lp_1)
        {
            CDictionaryBoolRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryBoolClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryBoolRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryBoolClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryBoolRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryBoolClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryBoolRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryBoolRemove(bool place, string key)//内部函数
        {
            bool torf = false;
            if (place) { return globalCDictionaryBool.TryRemove(key, out torf); }
            else { return localCDictionaryBool.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryBoolSave0(bool place, string key, bool val)
        {
            CDictionaryBoolSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryBoolSave1(bool place, string key, int lp_1, bool val)
        {
            CDictionaryBoolSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryBoolSave2(bool place, string key, int lp_1, int lp_2, bool val)
        {
            CDictionaryBoolSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryBoolSave3(bool place, string key, int lp_1, int lp_2, int lp_3, bool val)
        {
            CDictionaryBoolSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryBoolSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, bool val)
        {
            CDictionaryBoolSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static bool? CDictionaryBoolLoad0_N(bool place, string key)
        {
            if ((CDictionaryBoolKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryBoolGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static bool? CDictionaryBoolLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static bool? CDictionaryBoolLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static bool? CDictionaryBoolLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static bool? CDictionaryBoolLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回false</returns>
        public static bool CDictionaryBoolLoad0(bool place, string key)
        {
            if ((CDictionaryBoolKeyExists(place, key) == false))
            {
                return false;
            }
            return CDictionaryBoolGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回false</returns>
        public static bool CDictionaryBoolLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return false;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回false</returns>
        public static bool CDictionaryBoolLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return false;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回false</returns>
        public static bool CDictionaryBoolLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return false;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回false</returns>
        public static bool CDictionaryBoolLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryBoolKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return false;
            }
            return CDictionaryBoolGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Byte字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryByteSet(bool place, string key, byte val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryByte[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryByte[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryByteKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryByte.ContainsKey(key); }
            else { return localCDictionaryByte.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryByteValueExists(bool place, string key, byte value)
        {
            bool torf;
            byte tempValue = new byte();
            if (place) { torf = globalCDictionaryByte.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryByte.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte CDictionaryByteGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryByte[key]; }
            else { return localCDictionaryByte[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryByteClear0(bool place, string key)
        {
            CDictionaryByteRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryByteClear1(bool place, string key, int lp_1)
        {
            CDictionaryByteRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryByteClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryByteRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryByteClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryByteRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryByteClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryByteRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryByteRemove(bool place, string key)//内部函数
        {
            byte torf = new byte();
            if (place) { return globalCDictionaryByte.TryRemove(key, out torf); }
            else { return localCDictionaryByte.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryByteSave0(bool place, string key, byte val)
        {
            CDictionaryByteSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryByteSave1(bool place, string key, int lp_1, byte val)
        {
            CDictionaryByteSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryByteSave2(bool place, string key, int lp_1, int lp_2, byte val)
        {
            CDictionaryByteSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryByteSave3(bool place, string key, int lp_1, int lp_2, int lp_3, byte val)
        {
            CDictionaryByteSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryByteSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, byte val)
        {
            CDictionaryByteSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static byte? CDictionaryByteLoad0_N(bool place, string key)
        {
            if ((CDictionaryByteKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryByteGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static byte? CDictionaryByteLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static byte? CDictionaryByteLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static byte? CDictionaryByteLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static byte? CDictionaryByteLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回0</returns>
        public static byte CDictionaryByteLoad0(bool place, string key)
        {
            if ((CDictionaryByteKeyExists(place, key) == false))
            {
                return 0;
            }
            return CDictionaryByteGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回0</returns>
        public static byte CDictionaryByteLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回0</returns>
        public static byte CDictionaryByteLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回0</returns>
        public static byte CDictionaryByteLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回0</returns>
        public static byte CDictionaryByteLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryByteKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return 0;
            }
            return CDictionaryByteGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region Vector2F字典

        //C#中结构体是值类型

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryVectorSet(bool place, string key, Vector2F val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryVector[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryVector[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryVectorKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryVector.ContainsKey(key); }
            else { return localCDictionaryVector.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryVectorValueExists(bool place, string key, Vector2F value)
        {
            bool torf;
            Vector2F tempValue = new Vector2F();
            if (place) { torf = globalCDictionaryVector.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryVector.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Vector2F CDictionaryVectorGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryVector[key]; }
            else { return localCDictionaryVector[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryVectorClear0(bool place, string key)
        {
            CDictionaryVectorRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryVectorClear1(bool place, string key, int lp_1)
        {
            CDictionaryVectorRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryVectorClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryVectorRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryVectorClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryVectorRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryVectorClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryVectorRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryVectorRemove(bool place, string key)//内部函数
        {
            Vector2F torf = new Vector2F();
            if (place) { return globalCDictionaryVector.TryRemove(key, out torf); }
            else { return localCDictionaryVector.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryVectorSave0(bool place, string key, Vector2F val)
        {
            CDictionaryVectorSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryVectorSave1(bool place, string key, int lp_1, Vector2F val)
        {
            CDictionaryVectorSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryVectorSave2(bool place, string key, int lp_1, int lp_2, Vector2F val)
        {
            CDictionaryVectorSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryVectorSave3(bool place, string key, int lp_1, int lp_2, int lp_3, Vector2F val)
        {
            CDictionaryVectorSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryVectorSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, Vector2F val)
        {
            CDictionaryVectorSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? CDictionaryVectorLoad0_N(bool place, string key)
        {
            if ((CDictionaryVectorKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryVectorGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? CDictionaryVectorLoad1_N(bool place, string key, int lp_1)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? CDictionaryVectorLoad2_N(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? CDictionaryVectorLoad3_N(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static Vector2F? CDictionaryVectorLoad4_N(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F CDictionaryVectorLoad0(bool place, string key)
        {
            if ((CDictionaryVectorKeyExists(place, key) == false))
            {
                return new Vector2F(0, 0);
            }
            return CDictionaryVectorGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F CDictionaryVectorLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F CDictionaryVectorLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F CDictionaryVectorLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回new Vector2F(0,0)</returns>
        public static Vector2F CDictionaryVectorLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryVectorKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return new Vector2F(0, 0);
            }
            return CDictionaryVectorGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #endregion

        #region 引用类型

        #region Object字典

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryObjectSet(bool place, string key, object val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryObject[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryObject[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryObjectKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryObject.ContainsKey(key); }
            else { return localCDictionaryObject.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryObjectValueExists(bool place, string key, object value)
        {
            bool torf;
            object tempValue = null;
            if (place) { torf = globalCDictionaryObject.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryObject.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object CDictionaryObjectGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryObject[key]; }
            else { return localCDictionaryObject[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryObjectClear0(bool place, string key)
        {
            CDictionaryObjectRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryObjectClear1(bool place, string key, int lp_1)
        {
            CDictionaryObjectRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryObjectClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryObjectRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryObjectClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryObjectRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryObjectClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryObjectRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryObjectRemove(bool place, string key)//内部函数
        {
            object torf = null;
            if (place) { return globalCDictionaryObject.TryRemove(key, out torf); }
            else { return localCDictionaryObject.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryObjectSave0(bool place, string key, object val)
        {
            CDictionaryObjectSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryObjectSave1(bool place, string key, int lp_1, object val)
        {
            CDictionaryObjectSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryObjectSave2(bool place, string key, int lp_1, int lp_2, object val)
        {
            CDictionaryObjectSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryObjectSave3(bool place, string key, int lp_1, int lp_2, int lp_3, object val)
        {
            CDictionaryObjectSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryObjectSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, object val)
        {
            CDictionaryObjectSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static object CDictionaryObjectLoad0(bool place, string key)
        {
            if ((CDictionaryObjectKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryObjectGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static object CDictionaryObjectLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryObjectGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static object CDictionaryObjectLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryObjectGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static object CDictionaryObjectLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryObjectGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static object CDictionaryObjectLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryObjectKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryObjectGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #region String字典

        //C#中字符串是引用类型

        /// <summary>
        /// 添加字典键值对（重复添加则覆盖）
        /// </summary>
        /// <param name="place">true=全局，false=局部</param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        private static void CDictionaryStringSet(bool place, string key, string val)//内部使用
        {
            if (place)
            {
                //存入全局字典
                globalCDictionaryString[key] = val;
            }
            else
            {
                //存入局部字典
                localCDictionaryString[key] = val;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CDictionaryStringKeyExists(bool place, string key)
        {
            if (place) { return globalCDictionaryString.ContainsKey(key); }
            else { return localCDictionaryString.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CDictionaryStringValueExists(bool place, string key, string value)
        {
            bool torf;
            string tempValue = null;
            if (place) { torf = globalCDictionaryString.TryGetValue(key, out tempValue); }
            else { torf = localCDictionaryString.TryGetValue(key, out tempValue); }
            if (torf) { return tempValue == value; } else { return false; }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string CDictionaryStringGetValue(bool place, string key)
        {
            if (place) { return globalCDictionaryString[key]; }
            else { return localCDictionaryString[key]; }
        }

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        public static void CDictionaryStringClear0(bool place, string key)
        {
            CDictionaryStringRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[]，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        public static void CDictionaryStringClear1(bool place, string key, int lp_1)
        {
            CDictionaryStringRemove(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,]，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        public static void CDictionaryStringClear2(bool place, string key, int lp_1, int lp_2)
        {
            CDictionaryStringRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,]，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        public static void CDictionaryStringClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            CDictionaryStringRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 从字典中移除Key[,,,]，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        public static void CDictionaryStringClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            CDictionaryStringRemove(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        private static bool CDictionaryStringRemove(bool place, string key)//内部函数
        {
            string torf = null;
            if (place) { return globalCDictionaryString.TryRemove(key, out torf); }
            else { return localCDictionaryString.TryRemove(key, out torf); }
        }

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void CDictionaryStringSave0(bool place, string key, string val)
        {
            CDictionaryStringSet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="val"></param>
        public static void CDictionaryStringSave1(bool place, string key, int lp_1, string val)
        {
            CDictionaryStringSet(place, (key + "_" + lp_1.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="val"></param>
        public static void CDictionaryStringSave2(bool place, string key, int lp_1, int lp_2, string val)
        {
            CDictionaryStringSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="val"></param>
        public static void CDictionaryStringSave3(bool place, string key, int lp_1, int lp_2, int lp_3, string val)
        {
            CDictionaryStringSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()), val);
        }

        /// <summary>
        /// 保存字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <param name="val"></param>
        public static void CDictionaryStringSave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, string val)
        {
            CDictionaryStringSet(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()), val);
        }

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <returns>错误时返回null</returns>
        public static string CDictionaryStringLoad0(bool place, string key)
        {
            if ((CDictionaryStringKeyExists(place, key) == false))
            {
                return null;
            }
            return CDictionaryStringGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对，模拟1维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <returns>错误时返回null</returns>
        public static string CDictionaryStringLoad1(bool place, string key, int lp_1)
        {
            if ((CDictionaryStringKeyExists(place, (key + "_" + lp_1.ToString())) == false))
            {
                return null;
            }
            return CDictionaryStringGetValue(place, (key + "_" + lp_1.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟2维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <returns>错误时返回null</returns>
        public static string CDictionaryStringLoad2(bool place, string key, int lp_1, int lp_2)
        {
            if ((CDictionaryStringKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString())) == false))
            {
                return null;
            }
            return CDictionaryStringGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟3维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <returns>错误时返回null</returns>
        public static string CDictionaryStringLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            if ((CDictionaryStringKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString())) == false))
            {
                return null;
            }
            return CDictionaryStringGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString()));
        }

        /// <summary>
        /// 读取字典键值对，模拟4维数组
        /// </summary>
        /// <param name="place"></param>
        /// <param name="key"></param>
        /// <param name="lp_1"></param>
        /// <param name="lp_2"></param>
        /// <param name="lp_3"></param>
        /// <param name="lp_4"></param>
        /// <returns>错误时返回null</returns>
        public static string CDictionaryStringLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            if ((CDictionaryStringKeyExists(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString())) == false))
            {
                return null;
            }
            return CDictionaryStringGetValue(place, (key + "_" + lp_1.ToString() + "_" + lp_2.ToString() + "_" + lp_3.ToString() + "_" + lp_4.ToString()));
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #endregion

        #region Functions 互动管理（默认使用用户快捷数据表）

        //用数据表实现不同类型数据互动、信息管理

        #region 存储区状态队列管理

        /// <summary>
        /// 存储区容错处理函数，当哈希表键值存在时执行线程等待。常用于多线程触发器频繁写值，如大量注册注销动作使存储区数据重排序的，因哈希表正在使用需排队等待完成才给执行下一个。执行原理：将调用该函数的当前线程反复挂起dataTableThreadWaitPeriod毫秒，直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        public static void ThreadWait(string key)
        {
            while (DataTableBoolLoad0(true, "MMCore_ThreadWait_" + key) == true)
            {
                Thread.Sleep(dataTableThreadWaitPeriod); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数，当哈希表键值存在时执行线程等待。常用于多线程触发器频繁写值，如大量注册注销动作使存储区数据重排序的，因哈希表正在使用需排队等待完成才给执行下一个。执行原理：将调用该函数的当前线程反复挂起period毫秒，直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="period"></param>
        public static void ThreadWait(string key, int period)
        {
            while (DataTableBoolLoad0(true, "MMCore_ThreadWait_" + key) == true)
            {
                Thread.Sleep(period); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数，引发注册注销等存储区频繁重排序的动作，在函数开始/完成写入存储区时，应设置线程等待（val=1）/闲置（val=0）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">函数动作完成，所写入存储区闲置时填false，反之填true</param>
        private static void ThreadWaitSet(string key, bool val)
        {
            DataTableBoolSave0(true, "MMCore_ThreadWait_" + key, val);
        }

        /// <summary>
        /// 存储区容错处理函数，当哈希表键值存在时执行线程等待。常用于多线程触发器频繁写值，如大量注册注销动作使存储区数据重排序的，因哈希表正在使用需排队等待完成才给执行下一个。执行原理：将调用该函数的当前线程反复挂起dataTableThreadWaitPeriod毫秒，直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        public static void ThreadWait(bool place, string key)
        {
            while (DataTableBoolLoad0(place, "MMCore_ThreadWait_" + key) == true)
            {
                Thread.Sleep(dataTableThreadWaitPeriod); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数，当哈希表键值存在时执行线程等待。常用于多线程触发器频繁写值，如大量注册注销动作使存储区数据重排序的，因哈希表正在使用需排队等待完成才给执行下一个。执行原理：将调用该函数的当前线程反复挂起period毫秒，直到动作要写入的存储区闲置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="period"></param>
        public static void ThreadWait(bool place, string key, int period)
        {
            while (DataTableBoolLoad0(place, "MMCore_ThreadWait_" + key) == true)
            {
                Thread.Sleep(period); //将调用该函数的当前线程挂起
            }
        }

        /// <summary>
        /// 存储区容错处理函数，引发注册注销等存储区频繁重排序的动作，在函数开始/完成写入存储区时，应设置线程等待（val=1）/闲置（val=0）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">函数动作完成，所写入存储区闲置时填false，反之填true</param>
        private static void ThreadWaitSet(bool place, string key, bool val)
        {
            DataTableBoolSave0(place, "MMCore_ThreadWait_" + key, val);
        }

        #endregion

        #region 互动函数

        #region 任意类型

        //提示：可以将本类型作为模板修改后产生其他类型
        //提示：尽可能使用对口类型，以防值类型与引用类型发生转换时拆装箱降低性能

        //--------------------------------------------------------------------------------------------------
        //任意类型组Start
        //--------------------------------------------------------------------------------------------------

        #region 给对象注册句柄，对象和句柄形成双向映射关系

        //管理对象和句柄的双重映射关系的一对字典
        private static Dictionary<object, int> objectTag = new Dictionary<object, int>();
        private static Dictionary<int, object> tagObject = new Dictionary<int, object>();
        /// <summary>
        /// Object组中最大句柄（施行永续+1的方案）
        /// </summary>
        private static int objectJBNum = 0;

        /// <summary>
        /// 互动O_注册Object标签句柄并返回，若对象已被注册则返回其句柄，本函数不会进行重复注册
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns>返回一个Object的已注册标签</returns>
        private static int HD_RegObjectTagAndReturn(object lp_object)//内部使用
        {
            int lv_jBNum = objectJBNum;
            if (lv_jBNum == 0 || !objectTag.ContainsKey(lp_object))
            { //若最大句柄为0或虽最大句柄不为0但对象从未注册过，则最大句柄+1并作为对象句柄
                objectJBNum++; lv_jBNum++;
                //双向的映射关系
                objectTag[lp_object] = lv_jBNum;
                tagObject[lv_jBNum] = lp_object;
                return lv_jBNum;
            }
            else
            { //这是一个重复注册的对象，直接返回其句柄
                return objectTag[lp_object];
            }
        }

        /// <summary>
        /// 互动O_返回Object已注册标签句柄，对象未注册则返回0
        /// </summary>
        /// <param name="lp_object"></param>
        /// <returns>返回一个Object的已注册标签，错误返回0</returns>
        public static int HD_ReturnObjectTag(object lp_object)
        {
            int lv_jBNum = objectJBNum;
            if (lv_jBNum == 0 || !objectTag.ContainsKey(lp_object))
            { //若最大句柄为0或虽最大句柄不为0但对象从未注册过
                return lv_jBNum;
            }
            else
            { //这是一个已注册的对象，直接返回其句柄
                return objectTag[lp_object];
            }
        }

        /// <summary>
        /// 互动O_注册Object标签句柄并返回。为Object自动设置新的标签句柄，重复时会返回已注册的Object标签。这是一个内部函数，一般不需要手动使用
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
        /// <returns>返回一个Object的已注册标签，错误返回""</returns>
        public static string HD_ReturnObjectTagStr(object lp_object)
        {
            return HD_ReturnObjectTag(lp_object).ToString();
        }

        #endregion

        /// <summary>
        /// 互动O_注册Object(高级)。在指定Key存入Object，固有状态、固有自定义值是Object独一无二的标志（本函数重复注册会刷新），之后可用互动O_"返回Object注册总数"、"返回Object序号"、"返回序号对应Object"、"返回序号对应Object标签"、"返回Object自定义值"。Object组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Object组转为Key。固有状态相当于单位组单位活体，如需另外设置多个标记可使用"互动O_设定Object状态/自定义值"
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            lv_tag = HD_RegObjectTagAndReturn(lp_object);
            lv_tagStr = lv_tag.ToString();
            if ((lv_num == 0))
            {
                //首次注册
                lv_i = (lv_num + 1);
                //在组中的元素数量
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                //在组中的元素位置记录句柄字符
                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                //对象句柄有效状态
                DataTableBoolSave0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                //对象在组中的注册状态
                DataTableBoolSave1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
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
                        if (DataTableIntLoad1(true, lv_str + "Tag", lv_i) == lv_tag)
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                //在组中的元素数量
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                //在组中的元素位置记录句柄字符
                                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                //对象句柄有效状态
                                DataTableBoolSave0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                                //对象在组中的注册状态
                                DataTableBoolSave1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
                            }
                        }
                    }
                }
            }
            DataTableStringSave0(true, ("HD_ObjectState" + "_" + lv_tagStr), lp_inherentStats);
            DataTableStringSave0(true, ("HD_ObjectCV" + "_" + lv_tagStr), lp_inherentCustomValue);
        }

        /// <summary>
        /// 互动O_注册Object。在指定Key存入Object，固有状态、固有自定义值是Object独一无二的标志（本函数重复注册不会刷新），之后可用互动O_"返回Object注册总数"、"返回Object序号"、"返回序号对应Object"、"返回Object自定义值"。Object组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Object组转为Key。首次注册时固有状态自动为true（相当于单位组单位活体），之后只能通过"互动O_注册Object（高级）"改写，如需另外设置多个标记可使用"互动O_设定Object状态/自定义值"
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            lv_tag = HD_RegObjectTagAndReturn(lp_object);
            lv_tagStr = lv_tag.ToString();
            if ((lv_num == 0))
            {
                //首次注册
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                DataTableBoolSave0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                DataTableBoolSave1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
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
                        if (DataTableIntLoad1(true, lv_str + "Tag", lv_i) == lv_tag)
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                DataTableBoolSave0(true, (("HD_IfObjectTag") + "_" + lv_tagStr), true);
                                DataTableBoolSave1(true, ("IfObjectGTag" + lv_str), lv_tag, true);
                            }
                        }
                    }
                }
            }
            //从未注册过则进行首次修改为true
            if ((DataTableBoolKeyExists(true, ("HD_ObjectState" + "_" + lv_tag.ToString())) == false))
            {
                DataTableStringSave1(true, (("HD_ObjectState")), lv_tag, "true");
            }
        }

        /// <summary>
        /// 互动O_注销Object。用"互动O_注册Object"到Key，之后可用本函数彻底摧毁注册信息并将序号重排（包括Object标签有效状态、固有状态及自定义值）。注册注销同时进行会排队等待0.0625s直到没有注销动作，注销并不提升多少内存只是变量内容清空并序号重利用，非特殊要求一般不注销，而是用"互动O_设定Object状态"让Object状态失效（类似单位组的单位活体状态）。Object组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Object组转为Key。本函数无法摧毁用"互动O_设定Object状态/自定义值"创建的状态和自定义值，需手工填入""来排泄（非大量注销则提升内存量极小，可不管）。本函数参数Key若填Object组变量ID时会清空Object组专用状态
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
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
            if (lv_tag == 0) { return; } //如果对象没有注册过直接返回
            lv_str = (lp_key + "HD_Object");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tagStr = lv_tag.ToString();
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((DataTableIntLoad1(true, (lp_key + "HD_ObjectTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    //摧毁对象句柄有效状态
                    DataTableBoolClear0(true, "HD_IfObjectTag_" + lv_tagStr);
                    //摧毁对象在组中的注册状态（在其他组仍可能存在，可结合对象句柄有效状态一起判断）
                    DataTableBoolClear0(true, "IfObjectGTag" + lv_str + "_" + lv_tagStr);
                    //摧毁对象自身固有状态和固有自定义值
                    DataTableStringClear0(true, "HD_ObjectCV_" + lv_tagStr);
                    DataTableStringClear0(true, "HD_ObjectState_" + lv_tagStr);
                    //摧毁对象在组中的固有状态和固有自定义值
                    DataTableStringClear0(true, "HD_ObjectCV" + lv_str + "_" + lv_tagStr);
                    DataTableStringClear0(true, "HD_ObjectState" + lv_str + "_" + lv_tagStr);
                    //刷新组中的元素数量
                    DataTableIntSave0(true, (lp_key + "HD_ObjectNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = DataTableIntLoad1(true, (lp_key + "HD_ObjectTag"), lv_b + 1);
                        DataTableIntSave1(true, (lp_key + "HD_ObjectTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列，这里-1可以让挑选回滚，以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动O_移除Object。用"互动O_注册Object"到Key，之后可用本函数仅摧毁Key区注册的信息并将序号重排，用于Object组或多个键区仅移除Object（保留Object标签有效状态、固有值）。注册注销同时进行会排队等待0.0625s直到没有注销动作，注销并不提升多少内存只是变量内容清空并序号重利用，非特殊要求一般不注销，而是用"互动O_设定Object状态"让Object状态失效（类似单位组的单位活体状态）。Object组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Object组转为Key。本函数无法摧毁用"互动O_设定Object状态/自定义值"创建的状态和自定义值，需手工填入""来排泄（非大量注销则提升内存量极小，可不管）。本函数参数Key若填Object组变量ID时会清空Object组专用状态
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
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
            if (lv_tag == 0) { return; } //如果对象没有注册过直接返回
            lv_str = (lp_key + "HD_Object");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tagStr = lv_tag.ToString();
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((DataTableIntLoad1(true, (lp_key + "HD_ObjectTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    //摧毁对象在组中的注册状态（在其他组仍可能存在，可结合对象句柄有效状态一起判断）
                    DataTableBoolClear0(true, "IfObjectGTag" + lv_str + "_" + lv_tagStr);
                    //摧毁对象自身固有状态和固有自定义值
                    DataTableStringClear0(true, "HD_ObjectCV" + lv_str + "_" + lv_tagStr);
                    DataTableStringClear0(true, "HD_ObjectState" + lv_str + "_" + lv_tagStr);
                    //刷新组中的元素数量
                    DataTableIntSave0(true, (lp_key + "HD_ObjectNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = DataTableIntLoad1(true, (lp_key + "HD_ObjectTag"), lv_b + 1);
                        DataTableIntSave1(true, (lp_key + "HD_ObjectTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列，这里-1可以让挑选回滚，以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动O_返回Object注册总数。必须先使用"互动O_注册Object"才能返回指定Key里的注册总数。Object组使用时，可用"获取变量的内部名称"将Object组转为Key。
        /// </summary>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
        /// <returns></returns>
        public static int HD_ReturnObjectNumMax(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            //Implementation
            return lv_num;
        }

        /// <summary>
        /// 互动O_返回Object序号。使用"互动O_注册Object"后使用本函数可返回Key里的注册序号，Key无元素返回0，Key有元素但对象不在里面则返回-1，Object标签尚未注册则返回-2。Object组使用时，可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
        /// <returns>如果对象没有注册过直接返回0</returns>
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
            if (lv_tag == 0) { return 0; } //如果对象没有注册过直接返回0
            lv_str = (lp_key + "HD_Object");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
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
                            if ((DataTableIntLoad1(true, (lv_str + "Tag"), lv_i) == lv_tag))
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
        /// 互动O_返回序号对应Object。使用"互动O_注册Object"后，在参数填入注册序号可返回Object。Object组使用时，可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
        /// <returns></returns>
        public static object HD_ReturnObjectFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tag = DataTableIntLoad1(true, (lv_str + "Tag"), lp_regNum);
            //Implementation
            return HD_ReturnObjectFromTag(lv_tag);
        }

        /// <summary>
        /// 互动O_返回句柄标签对应Object。使用"互动O_注册Object"后，在参数填入句柄标签（整数）可返回Object，标签是Object的句柄。Object组使用时，可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_tag">句柄标签</param>
        /// <returns></returns>
        public static object HD_ReturnObjectFromTag(int lp_tag)
        {

            if (tagObject.ContainsKey(lp_tag))
            {
                //键存在，可以安全地访问
                return tagObject[lp_tag];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 互动O_返回序号对应Object标签句柄。使用"互动O_注册Object"后，在参数填入注册序号可返回Object标签（字符串）。Object组使用时，可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
        /// <returns></returns>
        public static string HD_ReturnObjectTagFromRegNumStr(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            string lv_tagStr;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tagStr = DataTableIntLoad1(true, (lv_str + "Tag"), lp_regNum).ToString();
            //Implementation
            return lv_tagStr;
        }

        /// <summary>
        /// 互动O_返回序号对应Object标签句柄。使用"互动O_注册Object"后，在参数填入注册序号可返回Object标签（整数）。Object组使用时，可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
        /// <returns></returns>
        public static int HD_ReturnObjectTagFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_tag = DataTableIntLoad1(true, (lv_str + "Tag"), lp_regNum);
            //Implementation
            return lv_tag;
        }

        /// <summary>
        /// 互动O_设置Object状态。必须先"注册"获得功能库内部句柄，再使用本函数给Object设定一个状态值，之后可用"互动O_返回Object状态"。类型参数用以记录多个不同状态，仅当"类型"参数填Object组ID转的Object串时，状态值"true"和"false"是Object的Object组专用状态值，用于内部函数筛选Object状态（相当于单位组单位索引是否有效），其他类型不会干扰系统内部，可随意填写。虽然注销时反向清空注册信息，但用"互动O_设定Object状态/自定义值"创建的值需要手工填入""来排泄（非大量注销则提升内存量极小，可不管）。注：固有状态值是注册函数赋予的系统内部变量（相当于单位组单位是否活体），只能通过"互动O_注册Object（高级）"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型，可填写任意状态标记如"State"</param>
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
            DataTableStringSave0(true, ("State" + lv_str + "_" + lv_tagStr), lp_stats);
        }

        /// <summary>
        /// 互动O_返回Object状态。使用"互动O_设定Object状态"后可使用本函数，将本函数参数"类型"设为空时返回固有值。类型参数用以记录多个不同状态，仅当"类型"参数为Object组ID转的字符串时，返回的状态值"true"和"false"是Object的Object组专用状态值，用于内部函数筛选Object状态（相当于单位组单位索引是否有效）
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型，可填写任意状态标记如"State"</param>
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
            lv_stats = DataTableStringLoad0(true, ("State" + lv_str + "_" + lv_tagStr));
            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动O_设置Object自定义值。必须先"注册"获得功能库内部句柄，再使用本函数设定Object的自定义值，之后可使用"互动O_返回Object自定义值"，类型参数用以记录多个不同自定义值。注：固有自定义值是注册函数赋予的系统内部变量，只能通过"互动O_注册Object（高级）"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型，可填写任意自定义值标记如"A"</param>
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
            DataTableStringSave0(true, ("CV" + lv_str + "_" + lv_tagStr), lp_customValue);
        }

        /// <summary>
        /// 互动O_返回Object自定义值。使用"互动O_设定Object自定义值"后可使用本函数，将本函数参数"类型"设为空时返回固有值，该参数用以记录多个不同自定义值
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储类型，可填写任意自定义值标记如"A"</param>
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
            lv_customValue = DataTableStringLoad0(true, ("CV" + lv_str + "_" + lv_tagStr));
            //Implementation
            return lv_customValue;
        }

        /// <summary>
        /// 互动O_返回Object固有状态。必须先使用"互动O_注册Object"才能返回到该值，固有状态是独一无二的标记（相当于单位组单位是否活体）
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
            lv_stats = DataTableStringLoad0(true, ("HD_ObjectState" + "_" + lv_tagStr));
            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动O_返回Object固有自定义值。必须先使用"互动O_注册Object"才能返回到该值，固有值是独一无二的标记
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
            lv_customValue = DataTableStringLoad0(true, ("HD_ObjectCV" + "_" + lv_tagStr));
            //Implementation
            return lv_customValue;
        }

        /// <summary>
        /// 互动O_设置Object的实数标记。必须先"注册"获得功能库内部句柄，再使用本函数让Object携带一个实数值，之后可使用"互动O_返回Object的实数标记"。Object组使用时，可用"获取变量的内部名称"将Object组转为Key
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
            DataTableDoubleSave0(true, ("HD_CDDouble_Object_" + lv_tagStr), lp_realNumTag);
        }

        /// <summary>
        /// 互动O_返回Object的实数标记。使用"互动O_设定Object的实数标记"后可使用本函数。Object组使用时，可用"获取变量的内部名称"将Object组转为Key
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
            return DataTableDoubleLoad0(true, ("HD_CDDouble_Object_" + lv_tagStr));
        }

        /// <summary>
        /// 互动O_返回Object标签句柄有效状态。将Object视作独一无二的个体，标签是它本身，有效状态则类似"单位是否有效"，当使用"互动O_注册Object"或"互动OG_添加Object到Object组"后激活Object有效状态（值为"true"），除非使用"互动O_注册Object（高级）"改写，否则直到注销才会摧毁
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
            return DataTableBoolLoad0(true, ("HD_IfObjectTag" + "_" + lv_tagStr));
        }

        /// <summary>
        /// 互动O_返回Object注册状态。使用"互动O_注册Object"或"互动OG_添加Object到Object组"后可使用本函数获取注册Object在Key中的注册状态，该状态只能注销或从Object组中移除时清空。Object组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Object组转为Key
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_key">存储键区，默认值"_Object"</param>
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
            return DataTableBoolLoad0(true, ("IfObjectGTag" + lv_str + "_" + lv_tagStr));
        }

        /// <summary>
        /// 互动OG_根据自定义值类型将Object组排序。根据Object携带的自定义值类型，对指定的Object组元素进行冒泡排序。Object组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区，默认填Object组名称</param>
        /// <param name="lp_cVStr">自定义值类型（要求自定义值是数字）</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
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
                    DataTableIntSave1(false, "HD_ObjStackOutTag", 1, lv_tag);
                    DataTableIntSave1(false, "HD_ObjStackOutTagValue", 1, lv_tagValue);
                    DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+"，IteraOrig="+IntToString(lv_a));
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
                            if (lv_tagValue < DataTableIntLoad1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_ObjStackOutTag", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                DataTableIntSave1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+"，当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > DataTableIntLoad1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnObjectTagFromRegNum(lv_b, lp_key))+"值"+IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b：" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize：" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae：" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c：" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_ObjStackOutTag", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                DataTableIntSave1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        DataTableIntSave1(false, "HD_ObjStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄，让元素交换
                //lv_tagStr = DataTableIntLoad1(true, (lp_key + "HD_ObjectTag"), lv_a).ToString(); //原始序号元素
                lv_tag = DataTableIntLoad1(false, "HD_ObjStackOutTag", lv_a);
                lv_tagValuestr = HD_ReturnObjectCV(HD_ReturnObjectFromTag(lv_tag), lp_cVStr);
                lv_tagValue = Convert.ToInt32(lv_tagValuestr);
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr));
                lv_b = DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号：" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a："+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    DataTableIntSave1(true, (lp_key + "HD_ObjectTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                      //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动OG_Object组排序。对指定的Object组元素进行冒泡排序（根据元素句柄）。Object组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区，默认填Object组名称</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
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
                    DataTableIntSave1(false, "HD_ObjStackOutTag", 1, lv_tag);
                    DataTableIntSave1(false, "HD_ObjStackOutTagValue", 1, lv_tagValue);
                    DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+"，IteraOrig="+IntToString(lv_a));
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
                            if (lv_tagValue < DataTableIntLoad1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_ObjStackOutTag", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                DataTableIntSave1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+"，当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > DataTableIntLoad1(false, "HD_ObjStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnObjectTagFromRegNum(lv_b, lp_key))+"值"+IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b：" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize：" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae：" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c：" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_ObjStackOutTag", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                DataTableIntSave1(false, "HD_ObjStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        DataTableIntSave1(false, "HD_ObjStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_ObjStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_ObjStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄，让元素交换
                //lv_tagStr = DataTableIntLoad1(true, (lp_key + "HD_ObjectTag"), lv_a).ToString(); //原始序号元素
                lv_tag = DataTableIntLoad1(false, "HD_ObjStackOutTag", lv_a);
                lv_tagValue = lv_tag;
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr));
                lv_b = DataTableIntLoad1(false, "HD_ObjStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号：" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a："+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    DataTableIntSave1(true, (lp_key + "HD_ObjectTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                      //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动OG_设定Object的Object组专用状态。给Object组的Object设定一个状态值（字符串），之后可用"互动O_返回Object、互动OG_返回Object组的Object状态"。状态值"true"和"false"是Object的Object组专用状态值，用于内部函数筛选字符状态（相当于单位组单位索引是否有效），而本函数可以重设干预，影响函数"互动OG_返回Object组元素数量（仅检索XX状态）"。与"互动O_设定Object状态"功能相同，只是状态参数在Object组中被固定为"Object组变量的内部ID"。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_objectGroup"></param>
        /// <param name="lp_groupState"></param>
        public static void HD_SetObjectGState(object lp_object, string lp_objectGroup, string lp_groupState)
        {
            HD_SetObjectState(lp_object, lp_objectGroup, lp_groupState);
        }

        /// <summary>
        /// 互动OG_返回Object的Object组专用状态。使用"互动O_设定Object、互动OG_设定Object组的Object状态"后可使用本函数。与"互动O_返回Object状态"功能相同，只是状态参数在Object组中被固定为"Object组变量的内部ID"。状态值"true"和"false"是Object的Object组专用状态值，用于内部函数筛选字符状态（相当于单位组单位索引是否有效）。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_objectGroup"></param>
        public static void HD_ReturnObjectGState(object lp_object, string lp_objectGroup)
        {
            HD_ReturnObjectState(lp_object, lp_objectGroup);
        }

        /// <summary>
        /// 互动OG_返回Object组元素序号对应元素。返回Object组元素序号指定Object。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static object HD_ReturnObjectFromObjectGFunc(int lp_regNum, string lp_gs)
        {
            return HD_ReturnObjectFromRegNum(lp_regNum, lp_gs);
        }

        /// <summary>
        /// 互动OG_返回Object组元素总数。返回指定Object组的元素数量。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        /// <returns>错误时返回0</returns>
        public static int HD_ReturnObjectGNumMax(string lp_gs)
        {
            return DataTableIntLoad0(true, lp_gs + "HD_ObjectNum");
        }

        /// <summary>
        /// 互动OG_返回Object组元素总数（仅检测Object组专用状态="true"）。返回指定Object组的元素数量。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动OG_返回Object组元素总数（仅检测Object组专用状态="false"）。返回指定Object组的元素数量。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动OG_返回Object组元素总数（仅检测Object组无效专用状态："false"或""）。返回指定Object组的元素数量（false、""、null）。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动OG_返回Object组元素总数（仅检测Object组指定专用状态）。返回指定Object组的元素数量。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动OG_添加Object到Object组。相同Object被认为是同一个，非高级功能不提供专用状态检查，如果Object没有设置过Object组专用状态，那么首次添加到Object组不会赋予"true"（之后可通过"互动O_设定Object状态"、"互动OG_设定Object组的Object状态"修改）。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_AddObjectToGroup_Simple(object lp_object, string lp_gs)
        {
            HD_RegObject_Simple(lp_object, lp_gs);
            //Simple方法没有组专用状态
        }

        /// <summary>
        /// 互动OG_添加Object到Object组（高级）。相同Object被认为是同一个，高级功能提供专用状态检查，如果Object没有设置过Object组专用状态，那么首次添加到Object组会赋予"true"（之后可通过"互动O_设定Object状态"、"互动OG_设定Object组的Object状态"修改）。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_AddObjectToGroup(object lp_object, string lp_gs)
        {
            //组中添加对象，不对其固有状态和固有自定义值进行任何修改，所以使用Simple
            HD_RegObject_Simple(lp_object, lp_gs);
            //高级方法设置组专用状态
            if (DataTableStringKeyExists(true, ("State" + lp_gs + "HD_Object_" + HD_RegObjectTagAndReturnStr(lp_object))) == false)
            {
                DataTableStringSave0(true, ("State" + lp_gs + "HD_Object_" + HD_RegObjectTagAndReturnStr(lp_object)), "true");
                //Console.WriteLine(lp_gs + "=>" + HD_RegObjectTagAndReturnStr(lp_object));
            }
        }

        /// <summary>
        /// 互动OG_移除Object组中的元素。使用"互动OG_添加Object到Object组"后可使用本函数进行移除元素。移除使用了"互动O_移除Object"，同一个存储区（Object组ID）序号重排，移除时该存储区如有其他操作会排队等待。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_object"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_RemoveObjectFromGroup(object lp_object, string lp_gs)
        {
            HD_RemoveObject(lp_object, lp_gs);
        }

        //互动OG_为Object组中的每个序号
        //GE（星际2的Galaxy Editor）的宏让编辑器保存时自动生成脚本并整合进脚本进行格式调整，C#仅参考需自行编写
        //#AUTOVAR(vs, string) = "#PARAM(group)";//"#PARAM(group)"是与字段、变量名一致的元素组名称，宏去声明string类型名为“Auto随机编号_vs”的自动变量，然后=右侧字符
        //#AUTOVAR(ae) = HD_ReturnObjectNumMax(#AUTOVAR(vs));//宏去声明默认int类型名为“Auto随机编号_ae”的自动变量，然后=右侧字符
        //#INITAUTOVAR(ai,increment)//宏去声明int类型名为“Auto随机编号_ai”的自动变量，用于下面for循环增量（increment是传入参数）
        //#PARAM(var) = #PARAM(s);//#PARAM(var)是传进来的参数，用作“当前被挑选到的元素”（任意变量-整数 lp_var）， #PARAM(s)是传进来的参数用作"开始"（int lp_s）
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #PARAM(var) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #PARAM(var) >= #AUTOVAR(ae)) ) ; #PARAM(var) += #AUTOVAR(ai) ) {
        //    #SUBFUNCS(actions)//代表用户GUI填写的所有动作
        //}

        /// <summary>
        /// 互动OG_为Object组中的每个序号。每次挑选的元素序号会自行在动作组（委托函数）中使用，委托函数特征：void SubActionTest(int lp_var)，参数lp_var即每次遍历到的元素序号，请自行组织它在委托函数内如何使用，SubActionTest可直接作为本函数最后一个参数填入，填入多个动作范例：SubVActionEventFuncref Actions += SubActionTest，然后Actions作为参数填入。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动OG_为Object组中的每个元素。每次挑选的元素会自行在动作组（委托函数）中使用，委托函数特征：void SubOActionEventFuncref(object lv_object)，参数lv_object即每次遍历到的元素，请自行组织它在委托函数内如何使用，SubActionTest可直接作为本函数最后一个参数填入，填入多个动作范例：SubVActionEventFuncref Actions += SubActionTest，然后Actions作为参数填入。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
                DataTableObjectSave0(false, "HD_ObjectGFor" + lv_vs + lv_va.ToString(), HD_ReturnObjectFromRegNum(lv_va, lv_vs));
            }
            lv_va = lp_start;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                lv_object = DataTableObjectLoad0(false, "HD_ObjectGFor" + lv_vs + lv_va.ToString());
                lp_funcref(lv_object);//用户填写的所有动作
            }
        }

        /// <summary>
        /// 互动OG_返回Object组中随机元素。返回指定Object组中的随机Object。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动OG_添加Object组到Object组。添加一个Object组A的元素到另一个Object组B，相同Object被认为是同一个。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
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
        /// 互动OG_从Object组移除Object组。将Object组A的元素从Object组B中移除，相同Object被认为是同一个。移除使用了"互动O_移除Object"，同一个存储区（Object组ID）序号重排，移除时该存储区如有其他操作会排队等待。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
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
        /// 互动OG_移除Object组全部元素。将Object组（Key区）存储的元素全部移除，相同Object被认为是同一个。移除时同一个存储区（Object组ID）序号不进行重排，但该存储区如有其他操作会排队等待。Object组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Object组到Object组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_key">存储键区，默认填Object组名称</param>
        public static void HD_RemoveObjectGAll(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_a;
            //Variable Initialization
            lv_str = (lp_key + "HD_Object");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                lv_tagStr = DataTableIntLoad1(true, (lp_key + "HD_ObjectTag"), lv_a).ToString();
                lv_num -= 1;
                DataTableBoolClear0(true, "IfObjectGTag" + lv_str + "_" + lv_tagStr);
                DataTableStringClear0(true, "HD_ObjectCV" + lv_str + "_" + lv_tagStr);
                DataTableStringClear0(true, "HD_ObjectState" + lv_str + "_" + lv_tagStr);
                DataTableIntSave0(true, (lp_key + "HD_ObjectNum"), lv_num);
            }
            ThreadWaitSet(true, lv_str, false);
        }

        //--------------------------------------------------------------------------------------------------
        //任意类型组End
        //--------------------------------------------------------------------------------------------------

        #endregion

        #region 字符串

        //提示：尽可能使用对口类型，以防值类型与引用类型发生转换时拆装箱降低性能

        //--------------------------------------------------------------------------------------------------
        //字符串组Start
        //--------------------------------------------------------------------------------------------------
        //设计方案：字符串的句柄就是它自己

        /// <summary>
        /// 互动S_注册String(高级)。在指定Key存入String，固有状态、固有自定义值是String独一无二的标志（本函数重复注册会刷新），之后可用互动S_"返回String注册总数"、"返回String序号"、"返回序号对应String"、"返回序号对应String标签"、"返回String自定义值"。String组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将String组转为Key。首次注册时固有状态为true（相当于单位组单位活体），如需另外设置多个标记可使用"互动S_设定String状态/自定义值"
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_string;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableStringSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                DataTableBoolSave0(true, (("HD_IfStringTag") + "_" + lv_tag), true); //保存标签的有效状态
                DataTableBoolSave0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true); //保存标签在区域的已注册状态
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
                        if ((DataTableStringLoad1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                DataTableStringSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                DataTableBoolSave0(true, (("HD_IfStringTag") + "_" + lv_tag), true);
                                DataTableBoolSave0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true);
                            }

                        }
                    }
                }

            }
            DataTableStringSave0(true, ("HD_StringState" + "_" + lv_tag), lp_inherentStats);
            DataTableStringSave0(true, ("HD_StringCV" + "_" + lv_tag), lp_inherentCustomValue);
        }

        /// <summary>
        /// 互动S_注册String。在指定Key存入String，固有状态、固有自定义值是String独一无二的标志（本函数重复注册不会刷新），之后可用互动S_"返回String注册总数"、"返回String序号"、"返回序号对应String"、"返回String自定义值"。String组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将String组转为Key。首次注册时固有状态为true（相当于单位组单位活体），之后只能通过"互动S_注册String（高级）"改写，如需另外设置多个标记可使用"互动S_设定String状态/自定义值"
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_string;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableStringSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                DataTableBoolSave0(true, (("HD_IfStringTag") + "_" + lv_tag), true);
                DataTableBoolSave0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true);
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
                        if ((DataTableStringLoad1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                DataTableStringSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                DataTableBoolSave0(true, (("HD_IfStringTag") + "_" + lv_tag), true);
                                DataTableBoolSave0(true, (("IfStringGTag" + lv_str) + "_" + lv_tag), true);
                            }

                        }
                    }
                }
            }
            //从未注册过则进行首次修改为true
            if ((DataTableBoolKeyExists(true, ("HD_StringState" + "_" + lv_tag)) == false))
            {
                DataTableStringSave0(true, ("HD_StringState" + "_" + lv_tag), "true");
            }
        }

        /// <summary>
        /// 互动S_注销String。用"互动S_注册String"到Key，之后可用本函数彻底摧毁注册信息并将序号重排（包括String标签有效状态、固有状态及自定义值）。注册注销同时进行会排队等待0.0625s直到没有注销动作，注销并不提升多少内存只是变量内容清空并序号重利用，非特殊要求一般不注销，而是用"互动S_设定String状态"让String状态失效（类似单位组的单位活体状态）。String组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将String组转为Key。本函数无法摧毁用"互动S_设定String状态/自定义值"创建的状态和自定义值，需手工填入""来排泄（非大量注销则提升内存量极小，可不管）。本函数参数Key若填String组变量ID时会清空String组专用状态
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_string;
            //Implementation
            if ((lv_tag != null))
            {
                ThreadWait(lv_str);
                ThreadWaitSet(true, lv_str, true);
                for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
                {
                    if ((DataTableStringLoad1(true, (lv_str + "Tag"), lv_a) == lv_tag))
                    {
                        lv_num -= 1;
                        DataTableBoolClear0(true, "HD_IfStringTag_" + lv_tag);
                        DataTableBoolClear0(true, "IfStringGTag" + lv_str + "_" + lv_tag);
                        DataTableStringClear0(true, "HD_StringCV_" + lv_tag);
                        DataTableStringClear0(true, "HD_StringState_" + lv_tag);
                        DataTableStringClear0(true, "HD_StringCV" + lv_str + "_" + lv_tag);
                        DataTableStringClear0(true, "HD_StringState" + lv_str + "_" + lv_tag);
                        DataTableIntSave0(true, (lp_key + "HD_StringNum"), lv_num);
                        for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                        {
                            lv_c = DataTableStringLoad1(true, (lp_key + "HD_StringTag"), lv_b + 1);
                            DataTableStringSave1(true, (lp_key + "HD_StringTag"), lv_b, lv_c);
                        }
                        //注销后触发序号重列，这里-1可以让挑选回滚，以再次检查重排后的当前挑选序号
                        lv_a -= 1;
                    }
                }
                ThreadWaitSet(true, lv_str, false);
            }
        }

        /// <summary>
        /// 互动S_移除String。用"互动S_注册String"到Key，之后可用本函数仅摧毁Key区注册的信息并将序号重排，用于String组或多个键区仅移除String（保留String标签有效状态、固有值）。注册注销同时进行会排队等待0.0625s直到没有注销动作，注销并不提升多少内存只是变量内容清空并序号重利用，非特殊要求一般不注销，而是用"互动S_设定String状态"让String状态失效（类似单位组的单位活体状态）。String组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将String组转为Key。本函数无法摧毁用"互动S_设定String状态/自定义值"创建的状态和自定义值，需手工填入""来排泄（非大量注销则提升内存量极小，可不管）。本函数参数Key若填String组变量ID时会清空String组专用状态
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_string;

            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((DataTableStringLoad1(true, (lp_key + "HD_StringTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    DataTableBoolClear0(true, "IfStringGTag" + lv_str + "_" + lv_tag);
                    DataTableStringClear0(true, "HD_StringCV" + lv_str + "_" + lv_tag);
                    DataTableStringClear0(true, "HD_StringState" + lv_str + "_" + lv_tag);
                    DataTableIntSave0(true, (lp_key + "HD_StringNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = DataTableStringLoad1(true, (lp_key + "HD_StringTag"), lv_b + 1);
                        DataTableStringSave1(true, (lp_key + "HD_StringTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列，这里-1可以让挑选回滚，以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动S_返回String注册总数。必须先使用"互动S_注册String"才能返回指定Key里的注册总数。String组使用时，可用"获取变量的内部名称"将String组转为Key。
        /// </summary>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
        /// <returns></returns>
        public static int HD_ReturnStringNumMax(string lp_key)
        {
            return DataTableIntLoad0(true, lp_key + "HD_StringNum");
        }

        /// <summary>
        /// 互动S_返回String序号。使用"互动S_注册String"后使用本函数可返回Key里的注册序号，Key无元素返回0，Key有元素但对象不在里面则返回-1，String标签尚未注册则返回-2。String组使用时，可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
        /// <returns>若返回成功将得到≥1的数，返回失败则为0</returns>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
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
                        if ((DataTableStringLoad1(true, (lv_str + "Tag"), lv_i) == lv_tag))
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
        /// 互动S_返回序号对应String。使用"互动S_注册String"后，在参数填入注册序号可返回String。String组使用时，可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
        /// <returns></returns>
        public static string HD_ReturnStringFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations And Initialization
            string lv_str = (lp_key + "HD_String");
            //Implementation
            return DataTableStringLoad1(true, (lv_str + "Tag"), lp_regNum);
        }

        /// <summary>
        /// 互动S_设置String状态。必须先"注册"获得功能库内部句柄，再使用本函数给String设定一个状态值，之后可用"互动S_返回String状态"。类型参数用以记录多个不同状态，仅当"类型"参数填String组ID转的String串时，状态值"true"和"false"是String的String组专用状态值，用于内部函数筛选String状态（相当于单位组单位索引是否有效），其他类型不会干扰系统内部，可随意填写。虽然注销时反向清空注册信息，但用"互动S_设定String状态/自定义值"创建的值需要手工填入""来排泄（非大量注销则提升内存量极小，可不管）。注：固有状态值是注册函数赋予的系统内部变量（相当于单位组单位是否活体），只能通过"互动S_注册String（高级）"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型，默认值"State"</param>
        /// <param name="lp_stats">状态</param>
        public static void HD_SetStringState(string lp_string, string lp_key, string lp_stats)
        {
            string lv_str = (lp_key + "HD_String");
            DataTableStringSave0(true, ("State" + lv_str + "_" + lp_string), lp_stats);
        }

        /// <summary>
        /// 互动S_返回String状态。使用"互动S_设定String状态"后可使用本函数，将本函数参数"类型"设为空时返回固有值。类型参数用以记录多个不同状态，仅当"类型"参数为String组ID转的字符串时，返回的状态值"true"和"false"是String的String组专用状态值，用于内部函数筛选String状态（相当于单位组单位索引是否有效）
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型，默认值"State"</param>
        /// <returns></returns>
        public static string HD_ReturnStringState(string lp_string, string lp_key)
        {
            string lv_str = (lp_key + "HD_String");
            return DataTableStringLoad0(true, ("State" + lv_str + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_设置String自定义值。必须先"注册"获得功能库内部句柄，再使用本函数设定String的自定义值，之后可使用"互动S_返回String自定义值"，类型参数用以记录多个不同自定义值。注：固有自定义值是注册函数赋予的系统内部变量，只能通过"互动S_注册String（高级）"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型，默认值"A"</param>
        /// <param name="lp_customValue">自定义值</param>
        public static void HD_SetStringCV(string lp_string, string lp_key, string lp_customValue)
        {
            string lv_str = (lp_key + "HD_String");
            DataTableStringSave0(true, ("CV" + lv_str + "_" + lp_string), lp_customValue);
        }

        /// <summary>
        /// 互动S_返回String自定义值。使用"互动S_设定String自定义值"后可使用本函数，将本函数参数"类型"设为空时返回固有值，该参数用以记录多个不同自定义值
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储类型，默认值"A"</param>
        /// <returns></returns>
        public static string HD_ReturnStringCV(string lp_string, string lp_key)
        {
            string lv_str = (lp_key + "HD_String");
            return DataTableStringLoad0(true, ("CV" + lv_str + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String固有状态。必须先使用"互动S_注册String"才能返回到该值，固有状态是独一无二的标记（相当于单位组单位是否活体）
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static string HD_ReturnStringState_Only(string lp_string)
        {
            return DataTableStringLoad0(true, ("HD_StringState" + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String固有自定义值。必须先使用"互动S_注册String"才能返回到该值，固有值是独一无二的标记
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static string HD_ReturnStringCV_Only(string lp_string)
        {
            return DataTableStringLoad0(true, ("HD_StringCV" + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_设置String的实数标记。必须先"注册"获得功能库内部句柄，再使用本函数让String携带一个实数值，之后可使用"互动S_返回String的实数标记"。String组使用时，可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_realNumTag">实数标记</param>
        public static void HD_SetStringDouble(string lp_string, double lp_realNumTag)
        {
            DataTableDoubleSave0(true, ("HD_CDDouble_String_" + lp_string), lp_realNumTag);
        }

        /// <summary>
        /// 互动S_返回String的实数标记。使用"互动S_设定String的实数标记"后可使用本函数。String组使用时，可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static double HD_ReturnStringDouble(string lp_string)
        {
            return DataTableDoubleLoad0(true, ("HD_CDDouble_String_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String标签句柄有效状态。将String视作独一无二的个体，标签是它本身，有效状态则类似"单位是否有效"，当使用"互动S_注册String"或"互动SG_添加String到String组"后激活String有效状态（值为"true"），除非使用"互动S_注册String（高级）"改写，否则直到注销才会摧毁
        /// </summary>
        /// <param name="lp_string"></param>
        /// <returns></returns>
        public static bool HD_ReturnIfStringTag(string lp_string)
        {
            return DataTableBoolLoad0(true, ("HD_IfStringTag" + "_" + lp_string));
        }

        /// <summary>
        /// 互动S_返回String注册状态。使用"互动S_注册String"或"互动SG_添加String到String组"后可使用本函数获取注册String在Key中的注册状态，该状态只能注销或从String组中移除时清空。String组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将String组转为Key
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_key">存储键区，默认值"_String"</param>
        /// <returns></returns>
        public static bool HD_ReturnIfStringTagKey(string lp_string, string lp_key)
        {
            string lv_str = (lp_key + "HD_String");
            return DataTableBoolLoad0(true, ("IfStringGTag" + lv_str + "_" + lp_string));
        }

        /// <summary>
        /// 互动SG_设定String的String组专用状态。给String组的String设定一个状态值（字符串），之后可用"互动S_返回String、互动SG_返回String组的String状态"。状态值"true"和"false"是String的String组专用状态值，用于内部函数筛选字符状态（相当于单位组单位索引是否有效），而本函数可以重设干预，影响函数"互动SG_返回String组元素数量（仅检索XX状态）"。与"互动S_设定String状态"功能相同，只是状态参数在String组中被固定为"String组变量的内部ID"。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_stringGroup"></param>
        /// <param name="lp_groupState"></param>
        public static void HD_SetStringGState(string lp_string, string lp_stringGroup, string lp_groupState)
        {
            HD_SetStringState(lp_string, lp_stringGroup, lp_groupState);
        }

        /// <summary>
        /// 互动SG_返回String的String组专用状态。使用"互动S_设定String、互动SG_设定String组的String状态"后可使用本函数。与"互动S_返回String状态"功能相同，只是状态参数在String组中被固定为"String组变量的内部ID"。状态值"true"和"false"是String的String组专用状态值，用于内部函数筛选字符状态（相当于单位组单位索引是否有效）。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_stringGroup"></param>
        public static void HD_ReturnStringGState(string lp_string, string lp_stringGroup)
        {
            HD_ReturnStringState(lp_string, lp_stringGroup);
        }

        /// <summary>
        /// 互动SG_返回String组元素序号对应元素。返回String组元素序号指定String。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static string HD_ReturnStringFromStringGFunc(int lp_regNum, string lp_gs)
        {
            return HD_ReturnStringFromRegNum(lp_regNum, lp_gs);
        }

        /// <summary>
        /// 互动SG_返回String组元素总数。返回指定String组的元素数量。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnStringGNumMax(string lp_gs)
        {
            return DataTableIntLoad0(true, lp_gs + "HD_StringNum");
        }

        /// <summary>
        /// 互动SG_返回String组元素总数（仅检测String组专用状态="true"）。返回指定String组的元素数量。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动SG_返回String组元素总数（仅检测String组专用状态="false"）。返回指定String组的元素数量。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动SG_返回String组元素总数（仅检测String组无效专用状态："false"或""）。返回指定String组的元素数量（false、""、null）。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动SG_返回String组元素总数（仅检测String组指定专用状态）。返回指定String组的元素数量。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动SG_添加String到String组。相同String被认为是同一个，非高级功能不提供专用状态检查，如果String没有设置过String组专用状态，那么首次添加到String组不会赋予"true"（之后可通过"互动S_设定String状态"、"互动SG_设定String组的String状态"修改）。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_AddStringToGroup_Simple(string lp_string, string lp_gs)
        {
            HD_RegString_Simple(lp_string, lp_gs);
        }

        /// <summary>
        /// 互动SG_添加String到String组（高级）。相同String被认为是同一个，高级功能提供专用状态检查，如果String没有设置过String组专用状态，那么首次添加到String组会赋予"true"（之后可通过"互动S_设定String状态"、"互动SG_设定String组的String状态"修改）。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_AddStringToGroup(string lp_string, string lp_gs)
        {
            //组中添加对象，不对其固有状态和固有自定义值进行任何修改，所以使用Simple
            HD_RegString_Simple(lp_string, lp_gs);
            if (DataTableStringKeyExists(true, ("State" + lp_gs + "HD_String_" + lp_string)) == false)
            {
                DataTableStringSave0(true, ("State" + lp_gs + "HD_String_" + lp_string), "true");
                //Console.WriteLine(lp_gs + "=>" + lp_string);
            }
        }

        /// <summary>
        /// 互动SG_移除String组中的元素。使用"互动SG_添加String到String组"后可使用本函数进行移除元素。移除使用了"互动S_移除String"，同一个存储区（String组ID）序号重排，移除时该存储区如有其他操作会排队等待。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_string"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_RemoveStringFromGroup(string lp_string, string lp_gs)
        {
            HD_RemoveString(lp_string, lp_gs);
        }

        //互动SG_为String组中的每个序号
        //GE（星际2的Galaxy Editor）的宏让编辑器保存时自动生成脚本并整合进脚本进行格式调整，C#仅参考需自行编写
        //#AUTOVAR(vs, string) = "#PARAM(group)";//"#PARAM(group)"是与字段、变量名一致的元素组名称，宏去声明string类型名为“Auto随机编号_vs”的自动变量，然后=右侧字符
        //#AUTOVAR(ae) = HD_ReturnStringNumMax(#AUTOVAR(vs));//宏去声明默认int类型名为“Auto随机编号_ae”的自动变量，然后=右侧字符
        //#INITAUTOVAR(ai,increment)//宏去声明int类型名为“Auto随机编号_ai”的自动变量，用于下面for循环增量（increment是传入参数）
        //#PARAM(var) = #PARAM(s);//#PARAM(var)是传进来的参数，用作“当前被挑选到的元素”（任意变量-整数 lp_var）， #PARAM(s)是传进来的参数用作"开始"（int lp_s）
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #PARAM(var) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #PARAM(var) >= #AUTOVAR(ae)) ) ; #PARAM(var) += #AUTOVAR(ai) ) {
        //    #SUBFUNCS(actions)//代表用户GUI填写的所有动作
        //}

        /// <summary>
        /// 互动SG_为String组中的每个序号。每次挑选的元素序号会自行在动作组（委托函数）中使用，委托函数特征：void SubActionTest(int lp_var)，参数lp_var即每次遍历到的元素序号，请自行组织它在委托函数内如何使用，SubActionTest可直接作为本函数最后一个参数填入，填入多个动作范例：SubVActionEventFuncref Actions += SubActionTest，然后Actions作为参数填入。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动SG_为String组中的每个元素。每次挑选的元素会自行在动作组（委托函数）中使用，委托函数特征：void SubSActionEventFuncref(string lp_str)，参数lv_str即每次遍历到的元素，请自行组织它在委托函数内如何使用，SubActionTest可直接作为本函数最后一个参数填入，填入多个动作范例：SubVActionEventFuncref Actions += SubActionTest，然后Actions作为参数填入。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
                DataTableStringSave0(false, "HD_StringGFor" + lv_vs + lv_va.ToString(), HD_ReturnStringFromRegNum(lv_va, lv_vs));
            }
            lv_va = lp_start;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                lv_str = DataTableStringLoad0(false, "HD_StringGFor" + lv_vs + lv_va.ToString());
                lp_funcref(lv_str);//用户填写的所有动作
            }
        }

        /// <summary>
        /// 互动SG_返回String组中随机元素。返回指定String组中的随机String。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动SG_添加String组到String组。添加一个String组A的元素到另一个String组B，相同String被认为是同一个。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
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
        /// 互动SG_从String组移除String组。将String组A的元素从String组B中移除，相同String被认为是同一个。移除使用了"互动S_移除String"，同一个存储区（String组ID）序号重排，移除时该存储区如有其他操作会排队等待。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
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
        /// 互动SG_移除String组全部元素。将String组（Key区）存储的元素全部移除，相同String被认为是同一个。移除时同一个存储区（String组ID）序号不进行重排，但该存储区如有其他操作会排队等待。String组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加String组到String组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_key">存储键区，默认填String组名称</param>
        public static void HD_RemoveStringGAll(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tag;
            int lv_a;
            //Variable Initialization
            lv_str = (lp_key + "HD_String");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                lv_tag = DataTableStringLoad1(true, (lp_key + "HD_StringTag"), lv_a);
                lv_num -= 1;
                DataTableBoolClear0(true, "IfStringGTag" + lv_str + "_" + lv_tag);
                DataTableStringClear0(true, "HD_StringCV" + lv_str + "_" + lv_tag);
                DataTableStringClear0(true, "HD_StringState" + lv_str + "_" + lv_tag);
                DataTableIntSave0(true, (lp_key + "HD_StringNum"), lv_num);
            }
            ThreadWaitSet(true, lv_str, false);
        }

        //--------------------------------------------------------------------------------------------------
        //字符串组End
        //--------------------------------------------------------------------------------------------------

        #endregion

        #region 数字

        //提示：尽可能使用对口类型，以防值类型与引用类型发生转换时拆装箱降低性能

        //--------------------------------------------------------------------------------------------------
        //数字组Start
        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// 互动I_注册Int(高级)。在指定Key存入Int，固有状态、固有自定义值是Int独一无二的标志（本函数重复注册会刷新），之后可用互动I_"返回Int注册总数"、"返回Int序号"、"返回序号对应Int"、"返回序号对应Int标签"、"返回Int自定义值"。Int组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Int组转为Key。首次注册时固有状态为true（相当于单位组单位活体），如需另外设置多个标记可使用"互动I_设定Int状态/自定义值"
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                DataTableBoolSave1(true, ("HD_IfIntTag"), lv_tag, true);
                DataTableBoolSave1(true, ("IfIntGTag" + lv_str), lv_tag, true);
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
                        if ((DataTableIntLoad1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                DataTableBoolSave1(true, ("HD_IfIntTag"), lv_tag, true);
                                DataTableBoolSave1(true, ("IfIntGTag" + lv_str), lv_tag, true);
                            }

                        }
                    }
                }

            }
            DataTableStringSave1(true, (("HD_IntState")), lv_tag, lp_inherentStats);
            DataTableStringSave1(true, (("HD_IntCV")), lv_tag, lp_inherentCustomValue);
        }

        /// <summary>
        /// 互动I_注册Int。在指定Key存入Int，固有状态、固有自定义值是Int独一无二的标志（本函数重复注册不会刷新），之后可用互动I_"返回Int注册总数"、"返回Int序号"、"返回序号对应Int"、"返回Int自定义值"。Int组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Int组转为Key。首次注册时固有状态为true（相当于单位组单位活体），之后只能通过"互动I_注册Int（高级）"改写，如需另外设置多个标记可使用"互动I_设定Int状态/自定义值"
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            if ((lv_num == 0))
            {
                lv_i = (lv_num + 1);
                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                DataTableBoolSave1(true, ("HD_IfIntTag"), lv_tag, true);
                DataTableBoolSave1(true, ("IfIntGTag" + lv_str), lv_tag, true);
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
                        if ((DataTableIntLoad1(true, (lv_str + "Tag"), lv_i) == lv_tag))
                        {
                            break;
                        }
                        else
                        {
                            if ((lv_i == lv_num))
                            {
                                lv_i = (lv_num + 1);
                                DataTableIntSave0(true, (lv_str + "Num"), lv_i);
                                DataTableIntSave1(true, (lv_str + "Tag"), lv_i, lv_tag);
                                DataTableBoolSave1(true, ("HD_IfIntTag"), lv_tag, true);
                                DataTableBoolSave1(true, ("IfIntGTag" + lv_str), lv_tag, true);
                            }

                        }
                    }
                }

            }
            //从未注册过则进行首次修改为true
            if ((DataTableBoolKeyExists(true, ("HD_IntState" + "_" + lv_tag.ToString())) == false))
            {
                DataTableStringSave1(true, (("HD_IntState")), lv_tag, "true");
            }
        }

        /// <summary>
        /// 互动I_注销Int。用"互动I_注册Int"到Key，之后可用本函数彻底摧毁注册信息并将序号重排（包括Int标签有效状态、固有状态及自定义值）。注册注销同时进行会排队等待0.0625s直到没有注销动作，注销并不提升多少内存只是变量内容清空并序号重利用，非特殊要求一般不注销，而是用"互动I_设定Int状态"让Int状态失效（类似单位组的单位活体状态）。Int组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Int组转为Key。本函数无法摧毁用"互动I_设定Int状态/自定义值"创建的状态和自定义值，需手工填入""来排泄（非大量注销则提升内存量极小，可不管）。本函数参数Key若填Int组变量ID时会清空Int组专用状态
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((DataTableIntLoad1(true, (lv_str + "Tag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    DataTableBoolClear1(true, "HD_IfIntTag", lv_tag);
                    DataTableBoolClear1(true, "IfIntGTag" + lv_str, lv_tag);
                    DataTableStringClear1(true, "HD_IntCV", lv_tag);
                    DataTableStringClear1(true, "HD_IntState", lv_tag);
                    DataTableStringClear1(true, "HD_IntCV" + lv_str, lv_tag);
                    DataTableStringClear1(true, "HD_IntState" + lv_str, lv_tag);
                    DataTableIntSave0(true, (lp_key + "HD_IntNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = DataTableIntLoad1(true, (lp_key + "HD_IntTag"), lv_b + 1);
                        DataTableIntSave1(true, (lp_key + "HD_IntTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列，这里-1可以让挑选回滚，以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动I_移除Int。用"互动I_注册Int"到Key，之后可用本函数仅摧毁Key区注册的信息并将序号重排，用于Int组或多个键区仅移除Int（保留Int标签有效状态、固有值）。注册注销同时进行会排队等待0.0625s直到没有注销动作，注销并不提升多少内存只是变量内容清空并序号重利用，非特殊要求一般不注销，而是用"互动I_设定Int状态"让Int状态失效（类似单位组的单位活体状态）。Int组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Int组转为Key。本函数无法摧毁用"互动I_设定Int状态/自定义值"创建的状态和自定义值，需手工填入""来排泄（非大量注销则提升内存量极小，可不管）。本函数参数Key若填Int组变量ID时会清空Int组专用状态
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            lv_tag = lp_integer;

            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                if ((DataTableIntLoad1(true, (lp_key + "HD_IntTag"), lv_a) == lv_tag))
                {
                    lv_num -= 1;
                    DataTableBoolClear1(true, "IfIntGTag" + lv_str, lv_tag);
                    DataTableStringClear1(true, "HD_IntCV" + lv_str, lv_tag);
                    DataTableStringClear1(true, "HD_IntState" + lv_str, lv_tag);
                    DataTableIntSave0(true, (lp_key + "HD_IntNum"), lv_num);
                    for (lv_b = lv_a; lv_b <= lv_num; lv_b += 1)
                    {
                        lv_c = DataTableIntLoad1(true, (lp_key + "HD_IntTag"), lv_b + 1);
                        DataTableIntSave1(true, (lp_key + "HD_IntTag"), lv_b, lv_c);
                    }
                    //注销后触发序号重列，这里-1可以让挑选回滚，以再次检查重排后的当前挑选序号
                    lv_a -= 1;
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动I_返回Int注册总数。必须先使用"互动I_注册Int"才能返回指定Key里的注册总数。Int组使用时，可用"获取变量的内部名称"将Int组转为Key。
        /// </summary>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
        /// <returns></returns>
        public static int HD_ReturnIntNumMax(string lp_key)
        {
            string lv_str = (lp_key + "HD_Int");
            return DataTableIntLoad0(true, (lv_str + "Num"));
        }

        /// <summary>
        /// 互动I_返回Int序号。使用"互动I_注册Int"后使用本函数可返回Key里的注册序号，Key无元素返回0，Key有元素但对象不在里面则返回-1，Int标签尚未注册则返回-2。Int组使用时，可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
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
                        if ((DataTableIntLoad1(true, (lv_str + "Tag"), lv_i) == lv_tag))
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
        /// 互动I_返回序号对应Int。使用"互动I_注册Int"后，在参数填入注册序号可返回Int。Int组使用时，可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_regNum"></param>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
        /// <returns></returns>
        public static int HD_ReturnIntFromRegNum(int lp_regNum, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_tag = DataTableIntLoad1(true, (lv_str + "Tag"), lp_regNum);
            //Implementation
            return lv_tag;
        }

        /// <summary>
        /// 互动I_设置Int状态。必须先"注册"获得功能库内部句柄，再使用本函数给Int设定一个状态值，之后可用"互动I_返回Int状态"。类型参数用以记录多个不同状态，仅当"类型"参数填Int组ID转的Int串时，状态值"true"和"false"是Int的Int组专用状态值，用于内部函数筛选Int状态（相当于单位组单位索引是否有效），其他类型不会干扰系统内部，可随意填写。虽然注销时反向清空注册信息，但用"互动I_设定Int状态/自定义值"创建的值需要手工填入""来排泄（非大量注销则提升内存量极小，可不管）。注：固有状态值是注册函数赋予的系统内部变量（相当于单位组单位是否活体），只能通过"互动I_注册Int（高级）"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型，默认值"State"</param>
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
            DataTableStringSave1(true, ("State" + lv_str), lv_tag, lp_stats);
        }

        /// <summary>
        /// 互动I_返回Int状态。使用"互动I_设定Int状态"后可使用本函数，将本函数参数"类型"设为空时返回固有值。类型参数用以记录多个不同状态，仅当"类型"参数为Int组ID转的字符串时，返回的状态值"true"和"false"是Int的Int组专用状态值，用于内部函数筛选Int状态（相当于单位组单位索引是否有效）
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型，默认值"State"</param>
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
            lv_stats = DataTableStringLoad1(true, ("State" + lv_str), lv_tag);

            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动I_设置Int自定义值。必须先"注册"获得功能库内部句柄，再使用本函数设定Int的自定义值，之后可使用"互动I_返回Int自定义值"，类型参数用以记录多个不同自定义值。注：固有自定义值是注册函数赋予的系统内部变量，只能通过"互动I_注册Int（高级）"函数或将本函数参数"类型"设为空时改写
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型，默认值"A"</param>
        /// <param name="lp_customValue">自定义值</param>
        public static void HD_SetIntCV(int lp_integer, string lp_key, string lp_customValue)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_tag = lp_integer;
            //Implementation
            DataTableStringSave1(true, (("CV" + lv_str)), lv_tag, lp_customValue);
        }

        /// <summary>
        /// 互动I_返回Int自定义值。使用"互动I_设定Int自定义值"后可使用本函数，将本函数参数"类型"设为空时返回固有值，该参数用以记录多个不同自定义值
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储类型，默认值"A"</param>
        /// <returns></returns>
        public static string HD_ReturnIntCV(int lp_integer, string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_tag;
            string lv_customValue;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_tag = lp_integer;
            lv_customValue = DataTableStringLoad1(true, (("CV" + lv_str)), lv_tag);
            //Implementation
            return lv_customValue;
        }

        /// <summary>
        /// 互动I_返回Int固有状态。必须先使用"互动I_注册Int"才能返回到该值，固有状态是独一无二的标记（相当于单位组单位是否活体）
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
            lv_stats = DataTableStringLoad1(true, ("HD_IntState"), lv_tag);
            //Implementation
            return lv_stats;
        }

        /// <summary>
        /// 互动I_返回Int固有自定义值。必须先使用"互动I_注册Int"才能返回到该值，固有值是独一无二的标记
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
            lv_customValue = DataTableStringLoad1(true, (("HD_IntCV")), lv_tag);
            //Implementation
            return lv_customValue;
        }

        /// <summary>
        /// 互动I_设置Int的实数标记。必须先"注册"获得功能库内部句柄，再使用本函数让Int携带一个实数值，之后可使用"互动I_返回Int的实数标记"。Int组使用时，可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_realNumTag">实数标记</param>
        public static void HD_SetIntDouble(int lp_integer, double lp_realNumTag)
        {
            DataTableDoubleSave1(true, "HD_CDDouble_Int", lp_integer, lp_realNumTag);
        }

        /// <summary>
        /// 互动I_返回Int的实数标记。使用"互动I_设定Int的实数标记"后可使用本函数。Int组使用时，可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <returns></returns>
        public static double HD_ReturnIntDouble(int lp_integer)
        {
            return DataTableDoubleLoad1(true, "HD_CDDouble_Int", lp_integer);
        }

        /// <summary>
        /// 互动I_返回Int标签句柄有效状态。将Int视作独一无二的个体，标签是它本身，有效状态则类似"单位是否有效"，当使用"互动I_注册Int"或"互动IG_添加Int到Int组"后激活Int有效状态（值为"true"），除非使用"互动I_注册Int（高级）"改写，否则直到注销才会摧毁
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <returns></returns>
        public static bool HD_ReturnIfIntTag(int lp_integer)
        {
            return DataTableBoolLoad1(true, ("HD_IfIntTag"), lp_integer);
        }

        /// <summary>
        /// 互动I_返回Int注册状态。使用"互动I_注册Int"或"互动IG_添加Int到Int组"后可使用本函数获取注册Int在Key中的注册状态，该状态只能注销或从Int组中移除时清空。Int组使用时，Key被强制为变量ID，可用"获取变量的内部名称"将Int组转为Key
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_key">存储键区，默认值"_Int"</param>
        /// <returns></returns>
        public static bool HD_ReturnIfIntTagKey(int lp_integer, string lp_key)
        {
            return DataTableBoolLoad1(true, ("IfIntGTag" + (lp_key + "HD_Int")), lp_integer);
        }

        /// <summary>
        /// 互动IG_根据自定义值类型将Int组排序。根据Int携带的自定义值类型，对指定的Int组元素进行冒泡排序。Int组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区，默认填Int组名称</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
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
                    DataTableIntSave1(false, "HD_IntStackOutTag", 1, lv_tag);
                    DataTableIntSave1(false, "HD_IntStackOutTagValue", 1, lv_tagValue);
                    DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+"，IteraOrig="+IntToString(lv_a));
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
                            if (lv_tagValue < DataTableIntLoad1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_IntStackOutTag", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                DataTableIntSave1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+"，当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > DataTableIntLoad1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnIntTagFromRegNum(lv_b, lp_key))+"值"+IntToString(DataTableIntLoad1(false, "IntStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b：" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize：" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae：" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c：" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_IntStackOutTag", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(DataTableIntLoad1(false, "IntStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(DataTableIntLoad1(false, "IntStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(DataTableIntLoad1(false, "IntStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                DataTableIntSave1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        DataTableIntSave1(false, "HD_IntStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄，让元素交换
                //lv_tagStr = DataTableIntLoad1(true, (lp_key + "IntTag"), lv_a).ToString(); //原始序号元素
                lv_tag = DataTableIntLoad1(false, "HD_IntStackOutTag", lv_a);
                lv_tagValuestr = HD_ReturnIntCV(lv_tag, lp_cVStr);
                lv_tagValue = Convert.ToInt32(lv_tagValuestr);
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr));
                lv_b = DataTableIntLoad1(false, "HD_IntStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号：" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a："+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    DataTableIntSave1(true, (lp_key + "HD_IntTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                   //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动IG_Int组排序。对指定的Int组元素进行冒泡排序（根据元素句柄）。Int组变量字符可通过"转换变量内部名称"获得
        /// </summary>
        /// <param name="lp_key">存储键区，默认填Int组名称</param>
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
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
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
                    DataTableIntSave1(false, "HD_IntStackOutTag", 1, lv_tag);
                    DataTableIntSave1(false, "HD_IntStackOutTagValue", 1, lv_tagValue);
                    DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", 1, lv_a);
                    //Console.WriteLine("尺寸" + IntToString(lv_intStackOutSize) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue)+"，IteraOrig="+IntToString(lv_a));
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
                            if (lv_tagValue < DataTableIntLoad1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                lv_intStackOutSize += 1;
                                autoHD_ae = (lv_b + 1);
                                lv_c = lv_intStackOutSize;
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_IntStackOutTag", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                }
                                DataTableIntSave1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                lv_bool = true;
                                break;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("大值靠前"+"，当前lv_b=" +IntToString(lv_b));
                            if (lv_tagValue > DataTableIntLoad1(false, "HD_IntStackOutTagValue", lv_b))
                            {
                                //Console.WriteLine("Num" + IntToString(lv_a) +"元素"+IntToString(lv_tagStr) +"值"+IntToString(lv_tagValue) + ">第Lv_b="+IntToString(lv_b)+"元素"+IntToString(HD_ReturnIntTagFromRegNum(lv_b, lp_key))+"值"+IntToString(DataTableIntLoad1(false, "IntStackOutTagValue", lv_b)));
                                //Console.WriteLine("生效的lv_b：" + IntToString(lv_b));
                                lv_intStackOutSize += 1;
                                //Console.WriteLine("lv_intStackOutSize：" + IntToString(lv_intStackOutSize));
                                autoHD_ae = (lv_b + 1);
                                //Console.WriteLine("autoHD_ae：" + IntToString(autoHD_ae));
                                lv_c = lv_intStackOutSize;
                                //Console.WriteLine("lv_c：" + IntToString(lv_c));
                                //Console.WriteLine("递减For lv_c=" + IntToString(lv_c) +"≥"+IntToString(autoHD_ae));
                                for (; ((autoHD_ai >= 0 && lv_c <= autoHD_ae) || (autoHD_ai < 0 && lv_c >= autoHD_ae)); lv_c += autoHD_ai)
                                {
                                    DataTableIntSave1(false, "HD_IntStackOutTag", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTag", (lv_c - 1)));
                                    //Console.WriteLine("交换元素" + IntToString(DataTableIntLoad1(false, "IntStackOutTag", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagValue", (lv_c - 1)));
                                    //Console.WriteLine("交换值" + IntToString(DataTableIntLoad1(false, "IntStackOutTagValue", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                    DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_c, DataTableIntLoad1(false, "HD_IntStackOutTagIteraOrig", (lv_c - 1)));
                                    //Console.WriteLine("交换新序值" + IntToString(DataTableIntLoad1(false, "IntStackOutTagIteraOrig", (lv_c - 1))) +"从序号"+IntToString(lv_c - 1) +"到"+IntToString(lv_c));
                                }
                                DataTableIntSave1(false, "HD_IntStackOutTag", lv_b, lv_tag);
                                //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_b, lv_tagValue);
                                //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到lv_b="+IntToString(lv_b) +"位置");
                                DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_b, lv_a);
                                //Console.WriteLine("值IteraOrig=lv_a=" + IntToString(lv_a) +"存到序号lv_b="+IntToString(lv_b) +"位置");
                                lv_bool = true;
                                break;
                            }
                        }
                    }
                    if ((lv_bool == false))
                    {
                        lv_intStackOutSize += 1;
                        DataTableIntSave1(false, "HD_IntStackOutTag", lv_intStackOutSize, lv_tag);
                        //Console.WriteLine("lv_tagStr=" + IntToString(lv_tagStr) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_IntStackOutTagValue", lv_intStackOutSize, lv_tagValue);
                        //Console.WriteLine("lv_tagValue=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                        DataTableIntSave1(false, "HD_IntStackOutTagIteraOrig", lv_intStackOutSize, lv_a);
                        //Console.WriteLine("IteraOrig=lv_a=" + IntToString(lv_tagValue) +"存到尺寸="+IntToString(lv_intStackOutSize) +"位置");
                    }
                }
            }
            autoE_ae = lv_num; //此时lv_intStackOutSize=Num
            lv_a = 1;
            //Console.WriteLine("最终处理For 1~" + IntToString(lv_num));
            for (; ((autoE_ai >= 0 && lv_a <= autoE_ae) || (autoE_ai < 0 && lv_a >= autoE_ae)); lv_a += autoE_ai)
            {
                //从序号里取出元素Tag、自定义值、新老句柄，让元素交换
                //lv_tagStr = DataTableIntLoad1(true, (lp_key + "IntTag"), lv_a).ToString(); //原始序号元素
                lv_tag = DataTableIntLoad1(false, "HD_IntStackOutTag", lv_a);
                lv_tagValue = lv_tag;
                //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr));
                lv_b = DataTableIntLoad1(false, "HD_IntStackOutTagIteraOrig", lv_a); //lv_tag的原序号位置
                                                                                     //Console.WriteLine("第"+IntToString(lv_a) +"个元素：" + IntToString(lv_tagStr) + "值"+ IntToString(lv_tagValue)+"原序号：" + IntToString(lv_tagStr));
                if (lv_a != lv_b)
                {
                    //Console.WriteLine("lv_a："+IntToString(lv_a) +"不等于lv_b" + IntToString(lv_b));
                    DataTableIntSave1(true, (lp_key + "HD_IntTag"), lv_a, lv_tag); //lv_tag放入新序号
                                                                                   //Console.WriteLine("元素"+IntToString(lv_tagStr) +"放入lv_b=" + IntToString(lv_b)+"位置");
                }
            }
            ThreadWaitSet(true, lv_str, false);
        }

        /// <summary>
        /// 互动IG_设定Int的Int组专用状态。给Int组的Int设定一个状态值（字符串），之后可用"互动I_返回Int、互动IG_返回Int组的Int状态"。状态值"true"和"false"是Int的Int组专用状态值，用于内部函数筛选字符状态（相当于单位组单位索引是否有效），而本函数可以重设干预，影响函数"互动IG_返回Int组元素数量（仅检索XX状态）"。与"互动I_设定Int状态"功能相同，只是状态参数在Int组中被固定为"Int组变量的内部ID"。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_integerGroup"></param>
        /// <param name="lp_groupState"></param>
        public static void HD_SetIntGState(int lp_integer, string lp_integerGroup, string lp_groupState)
        {
            HD_SetIntState(lp_integer, lp_integerGroup, lp_groupState);
        }

        /// <summary>
        /// 互动IG_返回Int的Int组专用状态。使用"互动I_设定Int、互动IG_设定Int组的Int状态"后可使用本函数。与"互动I_返回Int状态"功能相同，只是状态参数在Int组中被固定为"Int组变量的内部ID"。状态值"true"和"false"是Int的Int组专用状态值，用于内部函数筛选字符状态（相当于单位组单位索引是否有效）。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_integerGroup"></param>
        public static void HD_ReturnIntGState(int lp_integer, string lp_integerGroup)
        {
            HD_ReturnIntState(lp_integer, lp_integerGroup);
        }

        /// <summary>
        /// 互动IG_返回Int组元素序号对应元素。返回Int组元素序号指定Int。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_regNum">注册序号</param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnIntFromIntGFunc(int lp_regNum, string lp_gs)
        {
            return HD_ReturnIntFromRegNum(lp_regNum, lp_gs);
        }

        /// <summary>
        /// 互动IG_返回Int组元素总数。返回指定Int组的元素数量。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        /// <returns></returns>
        public static int HD_ReturnIntGNumMax(string lp_gs)
        {
            return DataTableIntLoad0(true, lp_gs + "HD_IntNum");
        }

        /// <summary>
        /// 互动IG_返回Int组元素总数（仅检测Int组专用状态="true"）。返回指定Int组的元素数量。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动IG_返回Int组元素总数（仅检测Int组专用状态="false"）。返回指定Int组的元素数量。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动IG_返回Int组元素总数（仅检测Int组无效专用状态："false"或""）。返回指定Int组的元素数量（false、""、null）。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动IG_返回Int组元素总数（仅检测Int组指定专用状态）。返回指定Int组的元素数量。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动IG_添加Int到Int组。相同Int被认为是同一个，非高级功能不提供专用状态检查，如果Int没有设置过Int组专用状态，那么首次添加到Int组不会赋予"true"（之后可通过"互动I_设定Int状态"、"互动IG_设定Int组的Int状态"修改）。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_AddIntToGroup_Simple(int lp_integer, string lp_gs)
        {
            HD_RegInt_Simple(lp_integer, lp_gs);
        }

        /// <summary>
        /// 互动IG_添加Int到Int组（高级）。相同Int被认为是同一个，高级功能提供专用状态检查，如果Int没有设置过Int组专用状态，那么首次添加到Int组会赋予"true"（之后可通过"互动I_设定Int状态"、"互动IG_设定Int组的Int状态"修改）。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_AddIntToGroup(int lp_integer, string lp_gs)
        {
            //组中添加对象，不对其固有状态和固有自定义值进行任何修改，所以使用Simple
            HD_RegInt_Simple(lp_integer, lp_gs);
            if (DataTableStringKeyExists(true, ("HD_IntState" + lp_gs + "HD_Int_" + lp_integer.ToString())) == false)
            {
                DataTableStringSave0(true, ("HD_IntState" + lp_gs + "HD_Int_" + lp_integer.ToString()), "true");
                //Console.WriteLine(lp_gs + "=>" + HD_RegIntTagAndReturn(lp_integer));
            }
        }

        /// <summary>
        /// 互动IG_移除Int组中的元素。使用"互动IG_添加Int到Int组"后可使用本函数进行移除元素。移除使用了"互动I_移除Int"，同一个存储区（Int组ID）序号重排，移除时该存储区如有其他操作会排队等待。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_integer"></param>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
        public static void HD_RemoveIntFromGroup(int lp_integer, string lp_gs)
        {
            HD_RemoveInt(lp_integer, lp_gs);
        }

        //互动IG_为Int组中的每个序号
        //GE（星际2的Galaxy Editor）的宏让编辑器保存时自动生成脚本并整合进脚本进行格式调整，C#仅参考需自行编写
        //#AUTOVAR(vs, string) = "#PARAM(group)";//"#PARAM(group)"是与字段、变量名一致的元素组名称，宏去声明string类型名为“Auto随机编号_vs”的自动变量，然后=右侧字符
        //#AUTOVAR(ae) = HD_ReturnIntNumMax(#AUTOVAR(vs));//宏去声明默认int类型名为“Auto随机编号_ae”的自动变量，然后=右侧字符
        //#INITAUTOVAR(ai,increment)//宏去声明int类型名为“Auto随机编号_ai”的自动变量，用于下面for循环增量（increment是传入参数）
        //#PARAM(var) = #PARAM(s);//#PARAM(var)是传进来的参数，用作“当前被挑选到的元素”（任意变量-整数 lp_var）， #PARAM(s)是传进来的参数用作"开始"（int lp_s）
        //for ( ; ( (#AUTOVAR(ai) >= 0 && #PARAM(var) <= #AUTOVAR(ae)) || (#AUTOVAR(ai) < 0 && #PARAM(var) >= #AUTOVAR(ae)) ) ; #PARAM(var) += #AUTOVAR(ai) ) {
        //    #SUBFUNCS(actions)//代表用户GUI填写的所有动作
        //}

        /// <summary>
        /// 互动IG_为Int组中的每个序号。每次挑选的元素序号会自行在动作组（委托函数）中使用，委托函数特征：void SubActionTest(int lp_var)，参数lp_var即每次遍历到的元素序号，请自行组织它在委托函数内如何使用，SubActionTest可直接作为本函数最后一个参数填入，填入多个动作范例：SubVActionEventFuncref Actions += SubActionTest，然后Actions作为参数填入。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动IG_为Int组中的每个元素。每次挑选的元素会自行在动作组（委托函数）中使用，委托函数特征：void SubActionTest(int lp_var)，参数lp_var即每次遍历到的元素，请自行组织它在委托函数内如何使用，SubActionTest可直接作为本函数最后一个参数填入，填入多个动作范例：SubVActionEventFuncref Actions += SubActionTest，然后Actions作为参数填入。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
                DataTableIntSave0(false, "HD_IntGFor" + lv_vs + lv_va.ToString(), HD_ReturnIntFromRegNum(lv_va, lv_vs));
            }
            lv_va = lp_start;
            for (; (lv_ai >= 0 && lv_va <= lv_ae) || (lv_ai < 0 && lv_va >= lv_ae); lv_va += lv_ai)
            {
                lv_vector = DataTableIntLoad0(false, "HD_IntGFor" + lv_vs + lv_va.ToString());
                lp_funcref(lv_vector);//用户填写的所有动作
            }
        }

        /// <summary>
        /// 互动IG_返回Int组中随机元素。返回指定Int组中的随机Int。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_gs">元素组的名称，建议与字段、变量名一致，数组使用时字符应写成：组[一维][二维]...以此类推</param>
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
        /// 互动IG_添加Int组到Int组。添加一个Int组A的元素到另一个Int组B，相同Int被认为是同一个。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
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
        /// 互动IG_从Int组移除Int组。将Int组A的元素从Int组B中移除，相同Int被认为是同一个。移除使用了"互动I_移除Int"，同一个存储区（Int组ID）序号重排，移除时该存储区如有其他操作会排队等待。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
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
        /// 互动IG_移除Int组全部元素。将Int组（Key区）存储的元素全部移除，相同Int被认为是同一个。移除时同一个存储区（Int组ID）序号不进行重排，但该存储区如有其他操作会排队等待。Int组目前不支持赋值其他变量，绝对ID对应绝对Key，可使用"添加Int组到Int组"函数来完成赋值需求
        /// </summary>
        /// <param name="lp_key">存储键区，默认填Int组名称</param>
        public static void HD_RemoveIntGAll(string lp_key)
        {
            //Variable Declarations
            string lv_str;
            int lv_num;
            string lv_tagStr;
            int lv_a;
            //Variable Initialization
            lv_str = (lp_key + "HD_Int");
            lv_num = DataTableIntLoad0(true, (lv_str + "Num"));
            //Implementation
            ThreadWait(lv_str);
            ThreadWaitSet(true, lv_str, true);
            for (lv_a = 1; lv_a <= lv_num; lv_a += 1)
            {
                lv_tagStr = DataTableIntLoad1(true, (lp_key + "HD_IntTag"), lv_a).ToString();
                lv_num -= 1;
                DataTableBoolClear0(true, "IfIntGTag" + lv_str + "_" + lv_tagStr);
                DataTableStringClear0(true, "HD_IntCV" + lv_str + "_" + lv_tagStr);
                DataTableStringClear0(true, "HD_IntState" + lv_str + "_" + lv_tagStr);
                DataTableIntSave0(true, (lp_key + "HD_IntNum"), lv_num);
            }
            ThreadWaitSet(true, lv_str, false);
        }

        //--------------------------------------------------------------------------------------------------
        //数字组End
        //--------------------------------------------------------------------------------------------------

        #endregion

        #endregion

        #endregion

        #region Functions 键鼠事件动作主体

        //加入按键监听并传参执行

        /// <summary>
        /// 注册键鼠总控预制事件。通过本函数可快捷将RecordService类实例中5个预制事件KeyDown、KeyUp、MouseMove、MouseDown、MouseUp注册给库内预制函数引用，从而使用按键总控管理衍生的所有功能（比如将“移动”、“发射火箭”等函数动作注册给Q键）
        /// </summary>
        /// <param name="cover">true：覆盖注册，false：追加注册</param>
        public static void AddKeyMouseEvent(RecordService keyMouseRecordService, bool cover)
        {
            if (cover)
            {
                //执行事件覆盖
                keyMouseRecordService.KeyDownEvent = KeyDown;
                keyMouseRecordService.KeyUpEvent = KeyUp;
                keyMouseRecordService.MouseMoveEvent = MouseMove;
                keyMouseRecordService.MouseDownEvent = MouseDown;
                keyMouseRecordService.MouseUpEvent = MouseUp;
            }
            else if (!keyMouseRecordService.DefaultEvent)
            {
                //执行事件追加
                keyMouseRecordService.KeyDownEvent += KeyDown;
                keyMouseRecordService.KeyUpEvent += KeyUp;
                keyMouseRecordService.MouseMoveEvent += MouseMove;
                keyMouseRecordService.MouseDownEvent += MouseDown;
                keyMouseRecordService.MouseUpEvent += MouseUp;
            }
            keyMouseRecordService.DefaultEvent = true;
        }

        /// <summary>
        /// 注销键鼠总控预制事件
        /// </summary>
        /// <param name="lp_null">true注销全部，否则仅注销预制事件</param>
        public static void DelKeyMouseEvent(RecordService keyMouseRecordService, bool lp_null)
        {
            if (lp_null)
            {
                //全事件清除
                keyMouseRecordService.KeyDownEvent = null;
                keyMouseRecordService.KeyUpEvent = null;
                keyMouseRecordService.MouseMoveEvent = null;
                keyMouseRecordService.MouseDownEvent = null;
                keyMouseRecordService.MouseUpEvent = null;
            }
            else if (keyMouseRecordService.DefaultEvent)
            {
                //仅移除预制事件
                keyMouseRecordService.KeyDownEvent -= KeyDown;
                keyMouseRecordService.KeyUpEvent -= KeyUp;
                keyMouseRecordService.MouseMoveEvent -= MouseMove;
                keyMouseRecordService.MouseDownEvent -= MouseDown;
                keyMouseRecordService.MouseUpEvent -= MouseUp;
            }
            keyMouseRecordService.DefaultEvent = false;
        }

        /// <summary>
        /// 键盘按下事件主要动作（加入按键监听并传参执行）
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        internal static bool KeyDown(int player, int key)
        {
            bool torf = !StopKeyMouseEvent[player];
            Player.KeyDownState[player, key] = torf;  //当前按键状态值
            Player.KeyDown[player, key] = true;  //当前按键值

            if (StopKeyMouseEvent[player] == false)
            {
                Player.KeyDownLoopOneBitNum[player] += 1; //玩家当前注册的按键队列数量
                HashTableSave2(true, "KeyDownLoopOneBit", player, Player.KeyDownLoopOneBitNum[player], key);
                //↑存储玩家注册序号对应按键队列键位
                HashTableSave2(true, "KeyDownLoopOneBitKey", player, key, true); //玩家按键队列键位状态
                //---------------------------------------------------------------------蓄力管理
                //if (XuLiGuanLi == true){
                //libBC0D3AAD_gf_HD_RegKXL(key, "IntGroup_XuLi" + IntToString(player)); //HD_注册蓄力按键
                //libBC0D3AAD_gf_HD_SetKeyFixedXL(player, key, 1.0);
                //}
                //---------------------------------------------------------------------双击管理
                //if (ShuangJiGuanLi == true){
                //    lv_a = libBC0D3AAD_gf_HD_ReturnKeyFixedSJ(player, key);
                //    if ((0.0 < lv_a) && (lv_a <= ShuangJiShiXian)){
                //        //符合双击标准，发送事件
                //        libBC0D3AAD_gf_Send_KeyDoubleClicked(player, key, ShuangJiShiXian - lv_a);
                //    } 
                //    else {   
                //        libBC0D3AAD_gf_HD_RegKSJ(key, "IntGroup_DoubleClicked" + IntToString(player)); //HD_注册按键
                //        libBC0D3AAD_gf_HD_SetKeyFixedSJ(player, key, ShuangJiShiXian);
                //    }
                //}
                //---------------------------------------------------------------------
                KeyDownGlobalEvent(key, true, player);
            }
            return torf;
        }

        /// <summary>
        /// 键盘弹起事件主要动作（加入按键监听并传参执行）
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static bool KeyUp(int player, int key)
        {
            bool torf = !StopKeyMouseEvent[player];
            Player.KeyDownState[player, key] = false;  //当前按键状态值，本事件始终为false
            Player.KeyDown[player, key] = false;  //当前按键值

            if (StopKeyMouseEvent[player] == false)
            {
                //直接执行动作或通知延迟弹起函数去执行动作
                if ((bool)HashTableLoad2(true, "KeyDownLoopOneBitKey", player, key) == false)
                {
                    //弹起时无该键动作队列（由延迟弹起执行完），则直接执行本次事件动作
                    KeyUpFunc(player, key);
                }
                else
                {
                    //弹起时有该键动作队列，通知延迟弹起函数运行（按键队列>0时，清空一次队列并执行它们的动作）
                    HashTableSave2(true, "KeyDownLoopOneBitEnd", player, key, true);
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
        /// 鼠标移动事件主要动作（加入按键监听并传参执行）
        /// </summary>
        /// <param name="player"></param>
        /// <param name="lp_mouseVector3F"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        internal static void MouseMove(int player, Vector3F lp_mouseVector3F, int uiX, int uiY)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (StopKeyMouseEvent[player] == false)
            {
                Player.MouseVector2F[player] = new Vector2F(lp_mouseVector3F.x, lp_mouseVector3F.y);

                //↓注意取出来的是该点最高位Unit
                float unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight"));
                float unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height"));

                Player.MouseVectorX[player] = lp_mouseVector3F.x;
                Player.MouseVectorY[player] = lp_mouseVector3F.y;
                Player.MouseVectorZ[player] = lp_mouseVector3F.z;
                Player.MouseVectorZFixed[player] = lp_mouseVector3F.z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVector3FFixed[player] = new Vector3F(lp_mouseVector3F.x, lp_mouseVector3F.y, Player.MouseVectorZFixed[player]);
                Player.MouseVector3F[player] = lp_mouseVector3F;
                //下面2个动作应该要从二维点读取单位（可多个），将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVector3FUnitTerrain[player] = new Vector3F(lp_mouseVector3F.x, lp_mouseVector3F.y, lp_mouseVector3F.z - unitTerrainHeight);
                Player.MouseVector3FTerrain[player] = new Vector3F(lp_mouseVector3F.x, lp_mouseVector3F.y, lp_mouseVector3F.z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时，计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度，用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, lp_mouseVector3F);
                }
            }
#else
            if (StopKeyMouseEvent[player] == false)
            {
                Player.MouseVector2F[player] = new Vector2F(lp_mouseVector3F.X, lp_mouseVector3F.Y);

                //↓注意取出来的是该点最高位Unit
                float unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight"));
                float unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height"));

                Player.MouseVectorX[player] = lp_mouseVector3F.X;
                Player.MouseVectorY[player] = lp_mouseVector3F.Y;
                Player.MouseVectorZ[player] = lp_mouseVector3F.Z;
                Player.MouseVectorZFixed[player] = lp_mouseVector3F.Z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVector3FFixed[player] = new Vector3F(lp_mouseVector3F.X, lp_mouseVector3F.Y, Player.MouseVectorZFixed[player]);
                Player.MouseVector3F[player] = lp_mouseVector3F;
                //下面2个动作应该要从二维点读取单位（可多个），将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVector3FUnitTerrain[player] = new Vector3F(lp_mouseVector3F.X, lp_mouseVector3F.Y, lp_mouseVector3F.Z - unitTerrainHeight);
                Player.MouseVector3FTerrain[player] = new Vector3F(lp_mouseVector3F.X, lp_mouseVector3F.Y, lp_mouseVector3F.Z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时，计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度，用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, lp_mouseVector3F);
                }
            }
#endif
        }

        /// <summary>
        /// 鼠标按下事件主要动作（加入按键监听并传参执行）
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="lp_mouseVector3F"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        /// <returns></returns>
        internal static bool MouseDown(int player, int key, Vector3F lp_mouseVector3F, int uiX, int uiY)
        {
            bool torf = !StopKeyMouseEvent[player];
            Player.MouseDownState[player, key] = torf;  //当前按键状态值
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
                Player.MouseVector2F[player] = new Vector2F(lp_mouseVector3F.x, lp_mouseVector3F.y);

                //↓注意取出来的是该点最高位Unit
                float unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight"));
                float unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height"));

                Player.MouseVectorX[player] = lp_mouseVector3F.x;
                Player.MouseVectorY[player] = lp_mouseVector3F.y;
                Player.MouseVectorZ[player] = lp_mouseVector3F.z;
                Player.MouseVectorZFixed[player] = lp_mouseVector3F.z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVector3FFixed[player] = new Vector3F(lp_mouseVector3F.x, lp_mouseVector3F.y, Player.MouseVectorZFixed[player]);
                Player.MouseVector3F[player] = lp_mouseVector3F;
                //下面2个动作应该要从二维点读取单位（可多个），将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVector3FUnitTerrain[player] = new Vector3F(lp_mouseVector3F.x, lp_mouseVector3F.y, lp_mouseVector3F.z - unitTerrainHeight);
                Player.MouseVector3FTerrain[player] = new Vector3F(lp_mouseVector3F.x, lp_mouseVector3F.y, lp_mouseVector3F.z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时，计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度，用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, lp_mouseVector3F);
                }

                //---------------------------------------------------------------------
                Player.MouseDownLoopOneBitNum[player] += 1;
                HashTableSave2(true, "MouseDownLoopOneBit", player, Player.MouseDownLoopOneBitNum[player], key);
                HashTableSave2(true, "MouseDownLoopOneBitKey", player, key, true);
                //---------------------------------------------------------------------
                //if (libBC0D3AAD_gv_XuLiGuanLi == true)
                //{
                //   libBC0D3AAD_gf_HD_RegKXL(lv_mouseButton, "libBC0D3AAD_gv_IntGroup_XuLi" + IntToString(lv_player)); //HD_注册按键
                //   libBC0D3AAD_gf_HD_SetKeyFixedXL(lv_player, lv_mouseButton, 1.0);
                //}
                ////---------------------------------------------------------------------
                //if (libBC0D3AAD_gv_ShuangJiGuanLi == true)
                //{
                //   libBC0D3AAD_gf_HD_RegPTwo(lv_point1, "DoubleClicked_PTwo_" + IntToString(lv_player));
                //   lv_a = libBC0D3AAD_gf_HD_ReturnKeyFixedSJ(lv_player, lv_mouseButton);
                //   if ((0.0 < lv_a) && (lv_a <= libBC0D3AAD_gv_ShuangJiShiXian) && libBC0D3AAD_gf_HD_PTwoRangeTrue("DoubleClicked_PTwo_" + IntToString(lv_player)))
                //   {
                //       //符合双击标准（鼠标双击多个2点验证），发送事件
                //       libBC0D3AAD_gf_Send_MouseDoubleClicked(lv_player, lv_mouseButton, libBC0D3AAD_gv_ShuangJiShiXian - lv_a, lv_point0, lv_uiX, lv_uiY);
                //   }
                //   else
                //   {
                //       libBC0D3AAD_gf_HD_RegKSJ(lv_mouseButton, "libBC0D3AAD_gv_IntGroup_DoubleClicked" + IntToString(lv_player)); //HD_注册按键
                //       libBC0D3AAD_gf_HD_SetKeyFixedSJ(lv_player, lv_mouseButton, libBC0D3AAD_gv_ShuangJiShiXian);
                //   }
                //}
                ////---------------------------------------------------------------------
                MouseDownFunc(player, key, lp_mouseVector3F, uiX, uiY);
            }
#else
            if (StopKeyMouseEvent[player] == false)
            {
                Player.MouseVector2F[player] = new Vector2F(lp_mouseVector3F.X, lp_mouseVector3F.Y);

                //↓注意取出来的是该点最高位Unit
                float unitTerrainHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.TerrainHeight"));
                float unitHeight = float.Parse(HD_ReturnObjectCV(Player.MouseVector2F[player], "Unit.Height"));

                Player.MouseVectorX[player] = lp_mouseVector3F.X;
                Player.MouseVectorY[player] = lp_mouseVector3F.Y;
                Player.MouseVectorZ[player] = lp_mouseVector3F.Z;
                Player.MouseVectorZFixed[player] = lp_mouseVector3F.Z - Game.MapHeight;

                Player.MouseUIX[player] = uiX;
                Player.MouseUIY[player] = uiY;

                Player.MouseVector3FFixed[player] = new Vector3F(lp_mouseVector3F.X, lp_mouseVector3F.Y, Player.MouseVectorZFixed[player]);
                Player.MouseVector3F[player] = lp_mouseVector3F;
                //下面2个动作应该要从二维点读取单位（可多个），将最高的单位的头顶坐标填入以修正鼠标Z点
                Player.MouseVector3FUnitTerrain[player] = new Vector3F(lp_mouseVector3F.X, lp_mouseVector3F.Y, lp_mouseVector3F.Z - unitTerrainHeight);
                Player.MouseVector3FTerrain[player] = new Vector3F(lp_mouseVector3F.X, lp_mouseVector3F.Y, lp_mouseVector3F.Z - unitTerrainHeight - unitHeight);

                //玩家控制单位存在时，计算鼠标距离控制单位的2D角度和3D距离
                if (Player.UnitControl[player] != null)
                {
                    //计算鼠标与控制单位的2D角度，用于调整角色在二维坐标系四象限内的的朝向
                    Player.MouseToUnitControlAngle[player] = AngleBetween(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的2D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange[player] = Distance(Player.UnitControl[player].Vector2F, Player.MouseVector2F[player]);
                    //计算鼠标与控制单位的3D距离（由于点击的位置是单位头顶位置，2个单位重叠则返回最高位的，所以玩家会点到最高位单位）
                    Player.MouseToUnitControlRange3F[player] = Distance(Player.UnitControl[player].Vector3F, lp_mouseVector3F);
                }

                //---------------------------------------------------------------------
                Player.MouseDownLoopOneBitNum[player] += 1;
                HashTableSave2(true, "MouseDownLoopOneBit", player, Player.MouseDownLoopOneBitNum[player], key);
                HashTableSave2(true, "MouseDownLoopOneBitKey", player, key, true);
                //---------------------------------------------------------------------
                //if (libBC0D3AAD_gv_XuLiGuanLi == true)
                //{
                //   libBC0D3AAD_gf_HD_RegKXL(lv_mouseButton, "libBC0D3AAD_gv_IntGroup_XuLi" + IntToString(lv_player)); //HD_注册按键
                //   libBC0D3AAD_gf_HD_SetKeyFixedXL(lv_player, lv_mouseButton, 1.0);
                //}
                ////---------------------------------------------------------------------
                //if (libBC0D3AAD_gv_ShuangJiGuanLi == true)
                //{
                //   libBC0D3AAD_gf_HD_RegPTwo(lv_point1, "DoubleClicked_PTwo_" + IntToString(lv_player));
                //   lv_a = libBC0D3AAD_gf_HD_ReturnKeyFixedSJ(lv_player, lv_mouseButton);
                //   if ((0.0 < lv_a) && (lv_a <= libBC0D3AAD_gv_ShuangJiShiXian) && libBC0D3AAD_gf_HD_PTwoRangeTrue("DoubleClicked_PTwo_" + IntToString(lv_player)))
                //   {
                //       //符合双击标准（鼠标双击多个2点验证），发送事件
                //       libBC0D3AAD_gf_Send_MouseDoubleClicked(lv_player, lv_mouseButton, libBC0D3AAD_gv_ShuangJiShiXian - lv_a, lv_point0, lv_uiX, lv_uiY);
                //   }
                //   else
                //   {
                //       libBC0D3AAD_gf_HD_RegKSJ(lv_mouseButton, "libBC0D3AAD_gv_IntGroup_DoubleClicked" + IntToString(lv_player)); //HD_注册按键
                //       libBC0D3AAD_gf_HD_SetKeyFixedSJ(lv_player, lv_mouseButton, libBC0D3AAD_gv_ShuangJiShiXian);
                //   }
                //}
                ////---------------------------------------------------------------------
                MouseDownFunc(player, key, lp_mouseVector3F, uiX, uiY);
            }
#endif
            return torf;
        }

        /// <summary>
        /// 鼠标按下事件处理函数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="lp_mouseVector3F"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        /// <returns></returns>
        internal static bool MouseDownFunc(int player, int key, Vector3F lp_mouseVector3F, int uiX, int uiY)
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
        /// 鼠标弹起事件主要动作（加入按键监听并传参执行）
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="lp_mouseVector3F"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        internal static bool MouseUp(int player, int key, Vector3F lp_mouseVector3F, int uiX, int uiY)
        {
            bool torf = !StopKeyMouseEvent[player];
            Player.MouseDownState[player, key] = false;  //当前按键状态值，本事件始终为false
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
                if ((bool)HashTableLoad2(true, "MouseDownLoopOneBitKey", player, key) == false)
                {
                    //弹起时无该键动作队列（由延迟弹起执行完），则直接执行本次事件动作
                    MouseUpFunc(player, key);
                }
                else
                {
                    //弹起时有该键动作队列，通知延迟弹起函数运行（按键队列>0时，清空一次队列并执行它们的动作）
                    HashTableSave2(true, "MouseDownLoopOneBitEnd", player, key, true);
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
        /// 键鼠弹起事件延迟执行函数，会按序执行键鼠事件动作队列，需加入到每帧执行（并遍历全部玩家）
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="lp_mouseVector3F"></param>
        /// <param name="uiX"></param>
        /// <param name="uiY"></param>
        internal static void MouseKeyUpWait(int player)
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
                    key = (int)HashTableLoad2(true, "MouseDownLoopOneBit", player, a);//读取玩家指定动作队列按键
                    if ((bool)HashTableLoad2(true, "MouseDownLoopOneBitEnd", player, key) == true)//判断玩家指定按键的动作队列是否结束
                    {
                        //如果该键的动作队列结束，重置按键状态
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
                        MouseDownFunc(player, key, Player.MouseVector3F[player], Player.MouseUIX[player], Player.MouseUIY[player]);
                    }
                    HashTableClear2(true, "MouseDownLoopOneBit", player, a);
                    HashTableClear2(true, "MouseDownLoopOneBitKey", player, key);
                    HashTableClear2(true, "MouseDownLoopOneBitEnd", player, key);
                }
                Player.MouseDownLoopOneBitNum[player] = 0; //动作全部执行，全队列清空
            }
            //玩家有键盘按键事件动作队列时
            if (Player.KeyDownLoopOneBitNum[player] > 0)//获取动作队列数量
            {
                be = Player.KeyDownLoopOneBitNum[player];
                a = 1;
                for (; ((bi >= 0 && a <= be) || (bi < 0 && a >= be)); a += bi)
                {
                    key = (int)HashTableLoad2(true, "KeyDownLoopOneBit", player, a);//读取玩家指定动作队列按键
                    if ((bool)HashTableLoad2(true, "KeyDownLoopOneBitEnd", player, key) == true)//判断玩家指定按键的动作队列是否结束
                    {
                        //如果该键的动作队列结束，重置按键状态
                        Player.KeyDown[player, key] = false;
                        KeyUpFunc(player, key);
                    }
                    HashTableClear2(true, "KeyDownLoopOneBit", player, a);
                    HashTableClear2(true, "KeyDownLoopOneBitKey", player, key);
                    HashTableClear2(true, "KeyDownLoopOneBitEnd", player, key);
                }
                Player.KeyDownLoopOneBitNum[player] = 0; //全键盘队列清空
            }
        }

        #endregion

        #region Functions 键鼠事件函数引用管理

        //可进行注册注销查询更换归并执行委托

        //------------------------------------↓KeyDownEventStart↓-----------------------------------------

        /// <summary>
        /// 将（1个或多个）委托函数注册到键盘按键事件（或者说给委托函数添加指定事件，完成事件注册）。
        /// 注册指定键盘按键的委托函数，每个键盘按键最大注册数量限制（8），超过则什么也不做
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
            keyEventFuncrefGroup[key, keyEventFuncrefGroupNum[key]] = funcref;//这里采用等于，设计为覆盖
            ThreadWaitSet("MMCore_KeyEventFuncref_", false);
        }
        /// <summary>
        /// 注册指定键盘按键的委托函数（登录在指定注册序号num位置）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="num">不能超过最大注册数量限制（8）</param>
        /// <param name="funcref"></param>
        public static void RegistKeyEventFuncref(int key, int num, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MMCore_KeyEventFuncref_");//注册注销时进行等待
            ThreadWaitSet("MMCore_KeyEventFuncref_", true);
            keyEventFuncrefGroup[key, num] = funcref;
            ThreadWaitSet("MMCore_KeyEventFuncref_", false);
        }

        /// <summary>
        /// 注销指定键盘按键的委托函数（发生序号重排）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        public static void RemoveKeyEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MMCore_KeyEventFuncref_");
            ThreadWaitSet("MMCore_KeyEventFuncref_", true);
            for (int a = 1; a <= keyEventFuncrefGroupNum[key]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (keyEventFuncrefGroup[key, a] == funcref)
                {
                    //该键位注册总数减一
                    keyEventFuncrefGroupNum[key] -= 1;
                    for (int b = a; b <= keyEventFuncrefGroupNum[key]; b += 1)
                    {
                        //将后序有效函数（如有）按序重排
                        keyEventFuncrefGroup[key, b] = keyEventFuncrefGroup[key, b];
                    }
                    //新的序号下从可疑序号重新开始检查，确保该函数在键位中彻底消失
                    a -= 1;
                }
            }
            ThreadWaitSet("MMCore_KeyEventFuncref_", false);
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
        /// 返回指定键盘按键指定函数的注册数量（>1则注册了多个同样的函数）
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
        /// 归并键盘按键指定函数（如存在则移除该函数注册并序号重排，之后重新注册1次）
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
                //发现重复函数，移除后重新注册
                RemoveKeyEventFuncref(key, funcref);
                RegistKeyEventFuncref(key, funcref);
            }
            ThreadWaitSet("MMCore_KeyEventFuncref_", false);
            return result;
        }

        /// <summary>
        /// 全局键盘按键事件，对指定键盘按键执行委托函数动作集合
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keydown"></param>
        /// <param name="player"></param>
        public static void KeyDownGlobalEvent(int key, bool keydown, int player)
        {
            for (int a = 1; a <= keyEventFuncrefGroupNum[key]; a += 1)
            {
                //这里不开新线程，是否另开线程运行宜由委托函数去写
                keyEventFuncrefGroup[key, a](keydown, player);//执行键盘按键委托
            }
        }

        //--------------------------------------↑KeyDownEventEnd↑-----------------------------------------

        //------------------------------------↓MouseDownEventStart↓---------------------------------------

        /// <summary>
        /// 将（1个或多个）委托函数注册到鼠标按键事件（或者说给委托函数添加指定事件，完成事件注册）。
        /// 注册指定鼠标键位的委托函数，每个鼠标按键最大注册数量限制（24），超过则什么也不做
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
            mouseEventFuncrefGroup[key, mouseEventFuncrefGroupNum[key]] = funcref;//这里采用等于，设计为覆盖
            ThreadWaitSet("MouseEventFuncref", false);
        }

        /// <summary>
        /// 注册指定鼠标键位的委托函数（登录在指定注册序号num位置）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="num">不能超过最大注册数量限制（24）</param>
        /// <param name="funcref"></param>
        public static void RegistMouseEventFuncref(int key, int num, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MouseEventFuncref");//注册注销时进行等待
            ThreadWaitSet("MouseEventFuncref", true);
            mouseEventFuncrefGroup[key, num] = funcref;
            ThreadWaitSet("MouseEventFuncref", false);
        }

        /// <summary>
        /// 注销指定鼠标键位的委托函数（发生序号重排）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="funcref"></param>
        public static void RemoveMouseEventFuncref(int key, KeyMouseEventFuncref funcref)
        {
            ThreadWait("MouseEventFuncref");
            ThreadWaitSet("MouseEventFuncref", true);
            for (int a = 1; a <= mouseEventFuncrefGroupNum[key]; a += 1)
            {
                if (mouseEventFuncrefGroup[key, a] == funcref)
                {
                    mouseEventFuncrefGroupNum[key] -= 1;
                    for (int b = a; b <= mouseEventFuncrefGroupNum[key]; b += 1)
                    {
                        mouseEventFuncrefGroup[key, b] = mouseEventFuncrefGroup[key, b];
                    }
                    a -= 1;
                }
            }
            ThreadWaitSet("MouseEventFuncref", false);
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
        /// 返回指定鼠标键位指定注册函数的数量（>1则注册了多个同样的函数）
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
        /// 归并鼠标按键指定函数（如存在则移除该函数注册并序号重排，之后重新注册1次）
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
                //发现重复函数，移除后重新注册
                RemoveMouseEventFuncref(key, funcref);
                RegistMouseEventFuncref(key, funcref);
            }
            return torf;
        }

        /// <summary>
        /// 全局鼠标按键事件，对指定鼠标按键执行委托函数动作集合
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keydown"></param>
        /// <param name="player"></param>
        public static void MouseDownGlobalEvent(int key, bool keydown, int player)
        {
            int a = 1;
            for (; a <= mouseEventFuncrefGroupNum[key]; a += 1)
            {
                //这里不开新线程，是否另开线程运行宜由委托函数去写
                mouseEventFuncrefGroup[key, a](keydown, player);//执行鼠标按键委托
            }
        }

        //------------------------------------↑MouseDownEventEnd↑-----------------------------------------

        #endregion

        #region Functions 主副循环入口事件函数管理

        //可进行注册注销查询更换归并执行委托

        //------------------------------------↓EntryFuncStart↓-----------------------------------------

        /// <summary>
        /// 将（1个或多个）委托函数注册到主副循环入口事件（或者说给委托函数添加指定事件，完成事件注册）。
        /// 注册指定主副循环入口的委托函数，每个入口最大注册数量限制（1），超过则什么也不做
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="funcref"></param>
        public static void RegistEntryEventFuncref(Entry entry, EntryEventFuncref funcref)
        {
            ThreadWait("EntryEventFuncref");//注册注销时进行等待
            ThreadWaitSet("EntryEventFuncref", true);
            if (entryEventFuncrefGroupNum[(int)entry] >= c_regEntryMax)
            {
                return;
            }
            entryEventFuncrefGroupNum[(int)entry] += 1;//注册成功记录+1
            entryEventFuncrefGroup[(int)entry, entryEventFuncrefGroupNum[(int)entry]] = funcref;//这里采用等于，设计为覆盖
            ThreadWaitSet("EntryEventFuncref", false);
        }

        /// <summary>
        /// 注册指定主副循环入口的委托函数（登录在指定注册序号num位置）
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="num">不能超过最大注册数量限制（8）</param>
        /// <param name="funcref"></param>
        public static void RegistEntryEventFuncref(Entry entry, int num, EntryEventFuncref funcref)
        {
            ThreadWait("EntryEventFuncref");//注册注销时进行等待
            ThreadWaitSet("EntryEventFuncref", true);
            entryEventFuncrefGroup[(int)entry, num] = funcref;
            ThreadWaitSet("EntryEventFuncref", false);
        }

        /// <summary>
        /// 注销指定主副循环入口的委托函数（发生序号重排）
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="funcref"></param>
        public static void RemoveEntryEventFuncref(Entry entry, EntryEventFuncref funcref)
        {
            ThreadWait("EntryEventFuncref");
            ThreadWaitSet("EntryEventFuncref", true);
            for (int a = 1; a <= entryEventFuncrefGroupNum[(int)entry]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (entryEventFuncrefGroup[(int)entry, a] == funcref)
                {
                    //该键位注册总数减一
                    entryEventFuncrefGroupNum[(int)entry] -= 1;
                    for (int b = a; b <= entryEventFuncrefGroupNum[(int)entry]; b += 1)
                    {
                        //将后序有效函数（如有）按序重排
                        entryEventFuncrefGroup[(int)entry, b] = entryEventFuncrefGroup[(int)entry, b];
                    }
                    //新的序号下从可疑序号重新开始检查，确保该函数在键位中彻底消失
                    a -= 1;
                }
            }
            ThreadWaitSet("EntryEventFuncref", false);
        }

        /// <summary>
        /// 返回指定主副循环入口注册函数的序号
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="funcref"></param>
        /// <returns>错误时返回-1</returns>
        public static int GetEntryEventFuncrefNearestNum(Entry entry, EntryEventFuncref funcref)
        {
            int num = -1;
            for (int a = 1; a <= entryEventFuncrefGroupNum[(int)entry]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (entryEventFuncrefGroup[(int)entry, a] == funcref)
                {
                    //返回最近的函数序号
                    num = a;
                    break;
                }
            }
            return num;
        }

        /// <summary>
        /// 返回指定主副循环入口指定函数的注册数量（>1则注册了多个同样的函数）
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="funcref"></param>
        /// <returns></returns>
        public static int GetEntryEventFuncrefCount(Entry entry, EntryEventFuncref funcref)
        {
            int count = 0;
            for (int a = 1; a <= entryEventFuncrefGroupNum[(int)entry]; a += 1)
            {
                //遍历检查所填函数注册序号
                if (entryEventFuncrefGroup[(int)entry, a] == funcref)
                {
                    count += 1;
                }
            }

            return count;
        }

        /// <summary>
        /// 归并主副循环入口指定函数（如存在则移除该函数注册并序号重排，之后重新注册1次）
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="funcref"></param>
        /// <returns></returns>
        public static bool RedoEntryEventFuncref(Entry entry, EntryEventFuncref funcref)
        {
            bool result = false;
            int num = GetEntryEventFuncrefCount(entry, funcref);
            if (num > 1)
            {
                result = true;
                //发现重复函数，移除后重新注册
                RemoveEntryEventFuncref(entry, funcref);
                RegistEntryEventFuncref(entry, funcref);
            }
            return result;
        }

        /// <summary>
        /// 全局主副循环入口事件，对指定入口执行委托函数动作集合
        /// </summary>
        /// <param name="entry"></param>
        public static void EntryGlobalEvent(Entry entry)
        {
            for (int a = 1; a <= entryEventFuncrefGroupNum[(int)entry]; a += 1)
            {
                entryEventFuncrefGroup[(int)entry, a]();//执行主副循环入口委托
            }
        }

        //------------------------------------↑EntryFuncEnd↑-----------------------------------------

        #endregion

    }
}

#region 小记
//本库由PC加载时推荐UTF-8（带BOM）编码以及CRLF尾行格式

//C#中实例方法与静态方法在内存都只存储一份，实例方法可使用this等指向实例，若明确不依赖实例则写静态方法为宜（减少以下性能开销）
//1.每次调用实例方法时都需要在调用栈上分配一定的空间来保存方法的局部变量和参数
//2.调用实例方法时会隐式地传递一个this引用指向调用该方法的对象实例（该引用在方法内部可用来访问对象的字段和方法）

//常量（const关键字修饰的字段）不会每次创建类的实例而重新分配内存，编译时就已确定其值并在程序整个生命周期都不会改变
//委托类型是顶级类型，故不支持Static修饰，用其声明的委托变量可被正常修饰

//对于类中字段，显式指定访问修饰符为宜，因为对于顶级类型（非嵌套），编译器并不会为它们设定默认的访问级别
//在结构体中，若未指定字段的访问修饰符，则这些字段会默认为public，其余一般会默认为private，对于顶级类型（非嵌套）一般会默认同类

// C#默认修饰符
// 类、结构体的默认修饰符是internal
// 类、结构体中所有成员默认修饰符是private
// 接口默认修饰符是internal
// 接口成员默认修饰符是public
// 枚举类型及成员默认修饰符是public，并且不允许显式指定其他访问修饰符（因为枚举的设计初衷就是为了提供一组可访问的常量集，如果允许设置其他访问修饰符将违背这一初衷）
// 委托的默认修饰符是internal
// 允许不同程序集访问的只有protected、protected internal和public，但前2者仅可访问不同程序集内的派生类
// protected可前插private（提高private访问权限，仅允许访问相同程序集内的派生类，不可跨程序集）
// protected可后跟internal（提高internal访问权限，允许访问不同程序集内的派生类）
// 静态构造函数不允许访问修饰符且不能带有任何参数（默认访问级别是私有的）

//‌Finalize方法：虽然C#允许定义Finalize方法来执行对象销毁前的清理工作，但这种方法通常不推荐使用
//因为它会增加垃圾回收的复杂性和开销，而且无法保证在何时被调用
//在现代C#编程中更推荐使用IDisposable接口和using语句来管理资源

//当类的实例被某个活动对象或静态字段引用，它就不会被垃圾回收，反之引用不存在时进行类的回收（逐步清理）
//‌逐步清理‌：垃圾回收器会递归地检查每个对象的引用情况，并回收整个不可达对象图（所有不再被程序中任何活动对象或静态字段引用的对象组成的集合）所占用的内存
//如果X类实例引用了其他对象，而这些对象又引用了其他对象，那么整个引用链上的对象都会被逐步清理掉
//所以C#中的自定义类哪怕没有制作Dispose方法，只需将引用=null即可，但写代码过程依然要尽量避免产生大量GC而降低性能
//当编写的类使用了非托管资源如文件流、数据库连接、图形对象等，应手动实现IDisposable接口并提供Dispose方法
//非托管资源是由操作系统直接管理的资源，不是.NET运行时的一部分，因此.NET垃圾回收器无法自动回收
//StringBuilder是托管类型，但Stream文件流对象（如StreamWriter）使用了非托管资源（如文件句柄）需要手动调用其Dispose或使用using块
//using块：动作末尾当Stream文件流对象被销毁时，Dispose会检查是否已调用Flush，如果没有它会自动调用Flush确保所有缓冲数据都被写入到文件或其他Stream文件流中

//静态类的成员（如字段、方法）必须是静态的，但静态字段可被赋值为实例对象的引用，静态方法内部也可创建类的实例
//静态字段在默认情况下会被初始化为它们的默认值（没赋值直接获取则返回该默认值），对于引用类型默认值是null

//DllImportAttribute常用于从非托管代码中导入函数，这是平台调用（P/Invoke）的一种常见方式
//LibraryImportAttribute是较新特性，在.NET 5及更高版本中引入，用于在编译时生成P/Invoke封送代码而不是在运行时（这可提高性能并减少启动延迟，因为不再需要在运行时解析DLL和函数）

//await关键字只能在async声明的异步函数内用，作用是等待一个异步操作的完成，并且不会阻塞调用线程
#endregion