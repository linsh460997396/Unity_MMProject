using System.ComponentModel;
using System.Threading;
using System.Timers;

namespace MetalMaxSystem
{
    /// <summary>
    /// 主循环触发器(静态类),请给函数注册事件(语法:MainUpdate.Awake/Start/Update/End/Destroy +=/-= 任意符合事件参数格式的函数的名称如MyFunc,其声明为void MyFun(object sender, EventArgs e),sender传递本类实例(其他类型也可),e传递额外事件参数类的信息),TriggerStart方法将自动创建独立触发器线程并启动周期触发器(主体事件发布动作),启动前可用Duetime、Period属性方法设定Update阶段每次循环的前摇和间隔,启动后按序执行Awake/Start/Update/End/Destroy被这5种事件注册过的委托函数,其中事件Update阶段是一个计时器循环,直到用户手动调用Stop属性方法,该属性为false时会让计时器到期退出Update循环,而计时器所在父线程(即触发器线程)将运行End和Destory事件
    /// </summary>
    public static class MainUpdate
    {
        #region 字段及其属性

        /// <summary>
        /// 主循环触发器线程
        /// </summary>
        public static Thread Thread { get; private set; } //不提供外部赋值

        /// <summary>
        /// 主循环触发器Update阶段,用来实现周期循环的计时器(支持小数毫秒间隔)
        /// </summary>
        public static System.Timers.Timer Timer { get; private set; } //不提供外部赋值

        /// <summary>
        /// 主循环触发器自动复位事件对象(用来向主循环触发器线程发送信号),属性动作AutoResetEvent.Set()可让触发器线程终止(效果等同MainUpdate.TimerStop = true)
        /// </summary>
        public static AutoResetEvent AutoResetEvent { get; private set; } //不提供外部赋值

        /// <summary>
        /// 主循环触发器Update事件运行次数(使用ulong类型防止溢出,约可运行18万亿次@50ms间隔)
        /// </summary>
        public static ulong InvokeCount { get; set; }

        /// <summary>
        /// 主循环触发器状态,手动设置为false则计时器工作时将收到信号退出循环(不执行Update事件),计时器所在父线程将运行End和Destory事件
        /// </summary>
        public static bool TimerStop { get; set; }

        /// <summary>
        /// 主循环触发器Update阶段前摇时间,设置后每次循环前都会等待
        /// </summary>
        public static int Duetime { get; set; }

        /// <summary>
        /// 主循环触发器Update阶段间隔运行时间(毫秒，支持小数如62.5ms)
        /// </summary>
        public static double Period { get; set; }

        #region 事件处理器列表（支持多处理器注册）

        private static EventHandlerList _eventHandlers = new EventHandlerList();
        private static readonly object _awakeEventKey = new object();
        private static readonly object _startEventKey = new object();
        private static readonly object _updateEventKey = new object();
        private static readonly object _endEventKey = new object();
        private static readonly object _destroyEventKey = new object();

        /// <summary>
        /// 主循环唤醒事件（运行一次）
        /// </summary>
        public static event EntryEventFuncref Awake
        {
            add { _eventHandlers.AddHandler(_awakeEventKey, value); }
            remove { _eventHandlers.RemoveHandler(_awakeEventKey, value); }
        }

        /// <summary>
        /// 主循环开始事件（运行一次）
        /// </summary>
        public static event EntryEventFuncref Start
        {
            add { _eventHandlers.AddHandler(_startEventKey, value); }
            remove { _eventHandlers.RemoveHandler(_startEventKey, value); }
        }

        /// <summary>
        /// 主循环更新事件（每帧运行）
        /// </summary>
        public static event EntryEventFuncref Update
        {
            add { _eventHandlers.AddHandler(_updateEventKey, value); }
            remove { _eventHandlers.RemoveHandler(_updateEventKey, value); }
        }

        /// <summary>
        /// 主循环结束事件（运行一次）
        /// </summary>
        public static event EntryEventFuncref End
        {
            add { _eventHandlers.AddHandler(_endEventKey, value); }
            remove { _eventHandlers.RemoveHandler(_endEventKey, value); }
        }

        /// <summary>
        /// 主循环销毁事件（运行一次）
        /// </summary>
        public static event EntryEventFuncref Destroy
        {
            add { _eventHandlers.AddHandler(_destroyEventKey, value); }
            remove { _eventHandlers.RemoveHandler(_destroyEventKey, value); }
        }

        #endregion

        #endregion

        #region 静态构造函数

        /// <summary>
        /// 主循环触发器状态监控类(用来读写InvokeCount、TimerStop属性),计时器实例创建时本类方法CheckStatus以参数填入被反复执行,主循环触发器Update事件被执行时创建计时器的父线程(MainUpdate.Thread)将暂停,直到该方法确认到TimerStop为真,退出计时器循环,并通知计时器所在父线程恢复运行(将执行End和Destory事件)
        /// </summary>
        static MainUpdate()
        {
            InvokeCount = 0;
            TimerStop = false;
        }

        #endregion

        #region 函数

        /// <summary>
        /// 主循环触发器的计时器实例创建时以参数填入、被反复执行的函数,Update事件被执行时创建计时器的父线程将暂停,直到本函数确认到TimerStop为真,退出计时器循环,并通知计时器所在父线程恢复运行(将执行End和Destory事件).一般不需要用户操作,TimerStop为true时手动调用会额外增加Update次数
        /// </summary>
        /// <param name="state"></param>
        public static void CheckStatus(object state)
        {
            if (TimerStop)
            {
                ((AutoResetEvent)state).Set();
            }
            else
            {
                InvokeCount++;
                OnUpdate();
            }
        }

        /// <summary>
        /// 开启主循环触发器(默认0.05现实时间秒,如需修改请在开启前用属性方法MainUpdate.Period、MainUpdate.Duetime来调整计时器Update阶段的间隔、前摇,若已经开启想要修改,可使用MainUpdate.Timer.Change)
        /// </summary>
        /// <param name="isBackground"></param>
        public static void Run(bool isBackground)
        {
            if (Thread == null)
            {
                Thread = new Thread(Action) { IsBackground = isBackground };
                Thread.Start();
            }
        }

        /// <summary>
        /// 主循环触发器唤醒阶段运行一次,允许主动调用
        /// </summary>
        public static void OnAwake()
        {
            ((EntryEventFuncref)_eventHandlers[_awakeEventKey])?.Invoke();
            MMCore.EntryGlobalEvent(Entry.MainAwake);
        }

        /// <summary>
        /// 主循环触发器开始阶段运行一次,允许主动调用
        /// </summary>
        public static void OnStart()
        {
            ((EntryEventFuncref)_eventHandlers[_startEventKey])?.Invoke();
            MMCore.EntryGlobalEvent(Entry.MainStart);
        }

        /// <summary>
        /// 主循环触发器每轮更新运行,主动调用时跟Unity引擎一样只运行一次
        /// </summary>
        public static void OnUpdate()
        {
            ((EntryEventFuncref)_eventHandlers[_updateEventKey])?.Invoke();
            MMCore.EntryGlobalEvent(Entry.MainUpdate);
        }

        /// <summary>
        /// 主循环触发器结束阶段运行一次,允许主动调用
        /// </summary>
        public static void OnEnd()
        {
            ((EntryEventFuncref)_eventHandlers[_endEventKey])?.Invoke();
            MMCore.EntryGlobalEvent(Entry.MainEnd);
        }

        /// <summary>
        /// 主循环触发器摧毁阶段运行一次,允许主动调用
        /// </summary>
        public static void OnDestroy()
        {
            ((EntryEventFuncref)_eventHandlers[_destroyEventKey])?.Invoke();
            MMCore.EntryGlobalEvent(Entry.MainDestroy);
            Reset();
        }

        /// <summary>
        /// 重置主循环触发器状态,允许重新启动(线程终止后调用此方法可恢复初始状态)
        /// </summary>
        public static void Reset()
        {
            Timer?.Dispose();
            Timer = null;
            AutoResetEvent?.Dispose();
            AutoResetEvent = null;
            Thread = null;
            TimerStop = false;
            InvokeCount = 0;
        }

        /// <summary>
        /// 主循环触发器主体事件发布动作(重复执行则什么也不做),若Update阶段属性未定义则默认每轮前摇0ms、间隔50ms
        /// </summary>
        private static void Action() //内部使用
        {
            if (AutoResetEvent == null)
            {
                AutoResetEvent = new AutoResetEvent(false);
                OnAwake();
                OnStart();
                if (Duetime < 0)
                {
                    Duetime = 0;
                }
                if (Period <= 0)
                {
                    Period = 50;
                }
                // 使用 System.Timers.Timer 支持小数毫秒间隔(如62.5ms)
                Timer = new System.Timers.Timer();
                Timer.Interval = Period;
                Timer.Elapsed += (sender, e) => CheckStatus(AutoResetEvent);
                System.Threading.Thread.Sleep(Duetime); // 前摇等待
                Timer.Start();
                AutoResetEvent.WaitOne();
                Timer.Stop();
                OnEnd();
                AutoResetEvent.WaitOne();
                Timer.Dispose();
                OnDestroy();
            }
        }

        /// <summary>
        /// 主循环触发器主体事件发布动作(重复执行则什么也不做),可自定义Update阶段属性Duetime(前摇)、Period(间隔)
        /// </summary>
        /// <param name="duetime">Updata阶段执行开始前等待(毫秒),仅生效一次</param>
        /// <param name="period">Updata阶段执行间隔(毫秒，支持小数如62.5ms)</param>
        private static void Action(int duetime, double period) //内部使用
        {
            if (AutoResetEvent == null)
            {
                AutoResetEvent = new AutoResetEvent(false);
                OnAwake();
                OnStart();
                Duetime = duetime < 0 ? 0 : duetime;
                Period = period <= 0 ? 50 : period;
                // 使用 System.Timers.Timer 支持小数毫秒间隔(如62.5ms)
                Timer = new System.Timers.Timer();
                Timer.Interval = Period;
                Timer.Elapsed += (sender, e) => CheckStatus(AutoResetEvent);
                System.Threading.Thread.Sleep(Duetime); // 前摇等待
                Timer.Start();
                AutoResetEvent.WaitOne();
                Timer.Stop();
                OnEnd();
                AutoResetEvent.WaitOne();
                Timer.Dispose();
                OnDestroy();
            }
        }

        #endregion
    }
}

#region 介绍

//// 1. 注册全局事件（在程序初始化时，通过MMCore注册到Entry枚举对应的事件）
//// 方式一：单独注册单个函数
//// 例如：MMCore.RegistEntryEventFuncref(Entry.MainAwake, OnMainAwake);  // 注册主循环唤醒事件
////       MMCore.RegistEntryEventFuncref(Entry.MainStart, OnMainStart);  // 注册主循环开始事件
////       MMCore.RegistEntryEventFuncref(Entry.MainUpdate, OnMainUpdate);  // 注册主循环更新事件
////       MMCore.RegistEntryEventFuncref(Entry.MainEnd, OnMainEnd);  // 注册主循环结束事件
////       MMCore.RegistEntryEventFuncref(Entry.MainDestroy, OnMainDestroy);  // 注册主循环销毁事件
////
//// 方式二：使用多播委托注册多个函数
//// EntryEventFuncref mainUpdateHandlers = null;
//// mainUpdateHandlers += OnUpdate1;
//// mainUpdateHandlers += OnUpdate2;
//// mainUpdateHandlers += OnUpdate3;
//// MMCore.RegistEntryEventFuncref(Entry.MainUpdate, mainUpdateHandlers);
////
//// 方式三：简洁事件注册（推荐，最简洁的方式）
//// MainUpdate.Awake += OnMainAwake;    // 注册主循环唤醒事件
//// MainUpdate.Start += OnMainStart;    // 注册主循环开始事件
//// MainUpdate.Update += OnMainUpdate;  // 注册主循环更新事件
//// MainUpdate.End += OnMainEnd;        // 注册主循环结束事件
//// MainUpdate.Destroy += OnMainDestroy; // 注册主循环销毁事件
////
//// 注销事件
//// MainUpdate.Awake -= OnMainAwake;    // 注销主循环唤醒事件
////
//// ⚠️ 注意：与 TimerUpdate/Trigger 的区别
//// - MainUpdate/SubUpdate 是静态类，**现在也支持多次 += 注册多个事件处理器**（使用 EventHandlerList）
//// - TimerUpdate/Trigger 是实例类，支持直接多次 += 注册多个事件处理器
//// - 两种方式可以混用：通过 += 注册的处理器 和 通过 MMCore.RegistEntryEventFuncref 注册的处理器都会被执行

//// 2. 配置参数（可选，不配置则使用默认值）
//MainUpdate.Duetime = 0;   // 前摇时间（毫秒）
//MainUpdate.Period = 50;   // 更新间隔（毫秒）

//// 3. 启动（创建独立线程运行）
//MainUpdate.Run(isBackground: true);

//// 4. 运行中可以读取状态
//ulong count = MainUpdate.InvokeCount;  // 获取Update执行次数

//// 5. 停止（设置标志，等待当前周期结束后优雅退出）
//MainUpdate.TimerStop = true;

//// 6. 停止后可以重新启动
//MainUpdate.Run(isBackground: true);  // OnDestroy已自动重置状态

#endregion