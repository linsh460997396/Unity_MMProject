using System.Threading;

namespace MetalMaxSystem
{
    /// <summary>
    /// 副循环触发器，请给函数注册事件（语法：SubUpdate.Awake/Start/Update/End/Destroy +=/-= 任意符合事件参数格式的函数的名称如MyFunc，其声明为void MyFun(object sender, EventArgs e)，sender传递本类实例（其他类型也可），e传递额外事件参数类的信息），TriggerStart方法将自动创建独立触发器线程并启动周期触发器（主体事件发布动作），启动前可用Duetime、Period属性方法设定Update阶段每次循环的前摇和间隔，启动后按序执行Awake/Start/Update/End/Destroy被这5种事件注册过的委托函数，其中事件Update阶段是一个计时器循环，直到用户手动调用Stop属性方法，该属性为false时会让计时器到期退出Update循环，而计时器所在父线程（即触发器线程）将运行End和Destory事件
    /// </summary>
    public static class SubUpdate
    {
        #region 字段及其属性

        /// <summary>
        /// 副循环触发器线程
        /// </summary>
        public static Thread Thread { get; private set; }//不提供外部赋值

        /// <summary>
        /// 副循环触发器Update阶段，用来实现周期循环的计时器
        /// </summary>
        public static Timer Timer { get; private set; }//不提供外部赋值

        /// <summary>
        /// 副循环触发器自动复位事件对象（用来向副循环触发器线程发送信号），属性动作AutoResetEvent.Set()可让触发器线程终止（效果等同SubUpdate.TimerStop = true）
        /// </summary>
        public static AutoResetEvent AutoResetEvent { get; private set; }//不提供外部赋值

        /// <summary>
        /// 副循环触发器Update事件运行次数
        /// </summary>
        public static int InvokeCount { get; set; }

        /// <summary>
        /// 副循环触发器状态，手动设置为false则计时器工作时将收到信号退出循环（不执行Update事件），计时器所在父线程将运行End和Destory事件
        /// </summary>
        public static bool TimerStop { get; set; }

        /// <summary>
        /// 副循环触发器Update阶段前摇时间，设置后每次循环前都会等待
        /// </summary>
        public static int Duetime { get; set; }
        /// <summary>
        /// 副循环触发器Update阶段间隔运行时间
        /// </summary>
        public static int Period { get; set; }

        #endregion

        #region 静态构造函数

        /// <summary>
        /// 副循环触发器状态监控类（用来读写InvokeCount、TimerStop属性），计时器实例创建时本类方法CheckStatus以参数填入被反复执行，副循环触发器Update事件被执行时创建计时器的父线程（SubUpdate.Thread）将暂停，直到该方法确认到TimerStop为真，退出计时器循环，并通知计时器所在父线程恢复运行（将执行End和Destory事件）
        /// </summary>
        static SubUpdate()
        {
            InvokeCount = 0;
            TimerStop = false;
        }

        #endregion

        #region 函数

        /// <summary>
        /// 副循环触发器的计时器实例创建时以参数填入、被反复执行的函数，Update事件被执行时创建计时器的父线程将暂停，直到本函数确认到TimerStop为真，退出计时器循环，并通知计时器所在父线程恢复运行（将执行End和Destory事件）。一般不需要用户操作，TimerStop为true时手动调用会额外增加Update次数
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
                Update();
            }
        }

        /// <summary>
        /// 开启副循环触发器（默认0.05现实时间秒，如需修改请在开启前用属性方法SubUpdate.Period、SubUpdate.Duetime来调整计时器Update阶段的间隔、前摇，若已经开启想要修改，可使用SubUpdate.Timer.Change）
        /// </summary>
        /// <param name="isBackground"></param>
        public static void Start(bool isBackground)
        {
            if (Thread == null)
            {
                Thread = new Thread(Func) { IsBackground = isBackground };
                Thread.Start();
            }
        }

        /// <summary>
        /// 副循环触发器方法，若Update阶段属性未定义则默认每轮前摇0ms、间隔50ms
        /// </summary>
        private static void Func()//内部使用
        {
            if (Duetime < 0) { Duetime = 0; }
            if (Period <= 0) { Period = 50; }
            Action(Duetime, Period);
        }

        /// <summary>
        /// 副循环触发器唤醒阶段运行一次，允许主动调用
        /// </summary>
        public static void Awake()
        {
            MMCore.EntryGlobalEvent(Entry.SubAwake);
        }

        /// <summary>
        /// 副循环触发器开始阶段运行一次，允许主动调用
        /// </summary>
        public static void Start()
        {
            MMCore.EntryGlobalEvent(Entry.SubStart);
        }

        /// <summary>
        /// 副循环触发器每轮更新运行，主动调用时跟Unity引擎一样只运行一次
        /// </summary>
        public static void Update()
        {
            MMCore.EntryGlobalEvent(Entry.SubUpdate);
        }

        /// <summary>
        /// 副循环触发器结束阶段运行一次，允许主动调用
        /// </summary>
        public static void End()
        {
            MMCore.EntryGlobalEvent(Entry.SubEnd);
        }

        /// <summary>
        /// 副循环触发器摧毁阶段运行一次，允许主动调用
        /// </summary>
        public static void Destroy()
        {
            MMCore.EntryGlobalEvent(Entry.SubDestroy);
        }

        /// <summary>
        /// 副循环触发器主体事件发布动作（重复执行则什么也不做），若Update阶段属性未定义则默认每轮前摇0ms、间隔50ms
        /// </summary>
        private static void Action()//内部使用
        {
            if (AutoResetEvent == null)
            {
                AutoResetEvent = new AutoResetEvent(false);
                Awake();
                Start();
                if (Duetime < 0) { Duetime = 0; }
                if (Period <= 0) { Period = 50; }
                //Timer自带线程，第一参数填入要间隔执行的方法（参数须符合委托类型），第二参数填状态对象，如自动复位事件对象来控制其他线程启停
                Timer = new Timer(CheckStatus, AutoResetEvent, Duetime, Period);
                AutoResetEvent.WaitOne();
                End();
                AutoResetEvent.WaitOne();
                Timer.Dispose();
                Destroy();
            }

        }

        /// <summary>
        /// 副循环触发器主体事件发布动作（重复执行则什么也不做），可自定义Update阶段属性Duetime（前摇）、Period（间隔）
        /// </summary>
        /// <param name="duetime">Updata阶段执行开始前等待（毫秒），仅生效一次</param>
        /// <param name="period">Updata阶段执行间隔（毫秒）</param>
        private static void Action(int duetime, int period)//内部使用
        {
            if (AutoResetEvent == null)
            {
                AutoResetEvent = new AutoResetEvent(false);
                Awake();
                Start();
                if (duetime < 0) { Duetime = 0; }
                if (period <= 0) { Period = 50; }
                //Timer自带线程，第一参数填入要间隔执行的方法（参数须符合委托类型），第二参数填状态对象，如自动复位事件对象来控制其他线程启停
                Timer = new Timer(CheckStatus, AutoResetEvent, Duetime, Period);
                AutoResetEvent.WaitOne();
                End();
                AutoResetEvent.WaitOne();
                Timer.Dispose();
                Destroy();
            }

        }

        #endregion

    }
}
