using System;
using System.ComponentModel;
using System.Threading;

namespace MetalMaxSystem
{
    /// <summary>
    /// 常规触发器，创建实例后请给函数注册事件（语法：Trigger.Awake/Start/Update/End/Destroy +=/-= 任意符合事件参数格式的函数的名称如MyFunc，其声明为void MyFun(object sender, EventArgs e)，sender传递本类实例（其他类型也可），e传递额外事件参数类的信息），Run方法将自动创建独立触发器线程并启动常规触发器（主体事件发布动作），启动前可用Duetime、Period属性方法设定Update阶段每次循环的前摇和间隔，启动后按序执行Awake/Start/Update/End/Destroy被这5种事件注册过的委托函数，其中事件Update阶段是一个计时器循环，直到用户手动调用TimerStop属性方法，该属性为false时会让计时器到期退出Update循环，而计时器所在父线程（即触发器线程）将运行End和Destory事件
    /// </summary>
    public class Trigger
    {
        #region 变量、字段及其属性方法

        /// <summary>
        /// 自动复位事件（用来控制触发线程信号）
        /// </summary>
        /// <summary>
        /// 自动复位事件，提供该属性方便随时读取，属性动作AutoResetEvent_Trigger.Set()可让触发器线程终止（效果等同Trigger.TimerState = true）
        /// </summary>
        public AutoResetEvent AutoResetEvent_Trigger { get; private set; }

        /// <summary>
        /// 常规触发器Update事件运行次数，该属性可随时读取或清零
        /// </summary>
        public int InvokeCount { get; set; }

        /// <summary>
        /// 常规触发器Update事件运行次数上限，该属性让计时器到期退出循环，计时器所在父线程将运行End和Destory事件
        /// </summary>
        public int InvokeCountMax { get; set; }

        /// <summary>
        /// 常规触发器Update事件前摇，未设置直接启动Run则默认为0
        /// </summary>
        public int Duetime { get; set; }

        /// <summary>
        /// 常规触发器的运行间隔属性，未设置直接启动Run则默认为1s
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// 常规触发器的状态属性，手动设置为false则计时器工作时将收到信号退出循环（不执行Update事件），计时器所在父线程将运行End和Destory事件
        /// </summary>
        public bool TimerState { get; set; }

        /// <summary>
        /// 事件委托列表，用来存储多个事件委托，用对象类型的键来取出，内部属性，用户不需要操作
        /// </summary>
        protected EventHandlerList _listEventDelegates = new EventHandlerList();

        /// <summary>
        /// 常规触发器主体事件发布动作所在线程的实例，提供该属性方便随时读取，但不允许不安全赋值
        /// </summary>
        public Thread Thread { get; private set; }

        /// <summary>
        /// 常规触发器执行Update事件的计时器实例，提供该属性方便随时读取，但不允许不安全赋值
        /// </summary>
        public Timer Timer { get; private set; }

        #endregion

        #region 定义区分委托对象的键

        //每个new object()都是一个单独的实例个体，所以定义的五个变量相当于内存ID不同的object类型的键
        //由于EventHandlerList类型的_listEventDelegates委托队列是实例成员，相同键返回的委托对象不会相同
        //所以即便创建了多个实例，以下键只需内存独此一份且私有只读

        /// <summary>
        /// 常规触发器用于返回事件委托队列中Awake事件委托对象的键
        /// </summary>
        private static readonly object awakeEventKey = new object();
        /// <summary>
        /// 常规触发器用于返回事件委托队列中Start事件委托对象的键
        /// </summary>
        private static readonly object startEventKey = new object();
        /// <summary>
        /// 常规触发器用于返回事件委托队列中Update事件委托对象的键
        /// </summary>
        private static readonly object updateEventKey = new object();
        /// <summary>
        /// 常规触发器用于返回事件委托队列中End事件委托对象的键
        /// </summary>
        private static readonly object endEventKey = new object();
        /// <summary>
        /// 常规触发器用于返回事件委托队列中Destroy事件委托对象的键
        /// </summary>
        private static readonly object destroyEventKey = new object();

        #endregion

        #region 声明事件委托

        //事件委托必须安全方式注册事件给函数，不能直接运行，而常规委托则相反。从事件委托列表通过键取出的事件委托可赋值给常规委托去执行，常规委托可以赋值给事件委托但不能交换顺序
        //声明事件委托变量（首字母大写），相比常规委托，事件委托因安全考虑无法直接被执行，通过OnAwake内部函数确保安全执行（其实是声明临时常规委托在赋值后执行）

        /// <summary>
        /// 将常规触发器的唤醒事件注册到函数，语法：Trigger.Awake +=/-= 实例或静态函数（特征格式：void 函数名(object sender, EventArgs e)）
        /// </summary>
        public event TriggerEventHandler Awake
        {
            // Add the input delegate to the collection.
            add
            {
                _listEventDelegates.AddHandler(awakeEventKey, value);
            }
            // Remove the input delegate from the collection.
            remove
            {
                _listEventDelegates.RemoveHandler(awakeEventKey, value);
            }
        }

        /// <summary>
        /// 将常规触发器的开始事件注册到函数，语法：Trigger.Start +=/-= 实例或静态函数（特征格式：void 函数名(object sender, EventArgs e)）
        /// </summary>
        public event TriggerEventHandler Start
        {
            add
            {
                _listEventDelegates.AddHandler(startEventKey, value);
            }
            remove
            {
                _listEventDelegates.RemoveHandler(startEventKey, value);
            }
        }

        /// <summary>
        /// 将常规触发器的开始事件注册到函数，语法：Trigger.Update +=/-= 实例或静态函数（特征格式：void 函数名(object sender, EventArgs e)）
        /// </summary>
        public event TriggerEventHandler Update
        {
            add
            {
                _listEventDelegates.AddHandler(updateEventKey, value);
            }
            remove
            {
                _listEventDelegates.RemoveHandler(updateEventKey, value);
            }
        }

        /// <summary>
        /// 将常规触发器的开始事件注册到函数，语法：Trigger.End +=/-= 实例或静态函数（特征格式：void 函数名(object sender, EventArgs e)）
        /// </summary>
        public event TriggerEventHandler End
        {
            add
            {
                _listEventDelegates.AddHandler(endEventKey, value);
            }
            remove
            {
                _listEventDelegates.RemoveHandler(endEventKey, value);
            }
        }

        /// <summary>
        /// 将常规触发器的开始事件注册到函数，语法：Trigger.Destroy +=/-= 实例或静态函数（特征格式：void 函数名(object sender, EventArgs e)）
        /// </summary>
        public event TriggerEventHandler Destroy
        {
            add
            {
                _listEventDelegates.AddHandler(destroyEventKey, value);
            }
            remove
            {
                _listEventDelegates.RemoveHandler(destroyEventKey, value);
            }
        }

        #endregion

        #region 构造函数（本类创建时的2个重载方法）

        /// <summary>
        /// 创建一个不会到期的常规触发器
        /// </summary>
        public Trigger()//构造函数
        {
            InvokeCount = 0;
            InvokeCountMax = 0;
            TimerState = false;
        }

        /// <summary>
        /// 创建一个有执行次数的常规触发器
        /// </summary>
        /// <param name="invokeCountMax">决定计时器Update阶段循环次数</param>
        public Trigger(int invokeCountMax)//构造函数
        {
            InvokeCount = 0;
            InvokeCountMax = invokeCountMax;
            TimerState = false;
        }

        #endregion

        //非静态（实例）方法可以访问类中的任何成员

        /// <summary>
        /// 计时器实例创建时以参数填入、被反复执行的函数，Update事件被执行时创建计时器的父线程将暂停，直到本函数确认到TimerState为真，退出计时器循环，并通知计时器所在父线程恢复运行（将执行End和Destory事件）
        /// </summary>
        /// <param name="state"></param>
        private void CheckStatus(object state)
        {
            if (TimerState)
            {
                ((AutoResetEvent)state).Set();
            }
            else if (InvokeCountMax > 0 && InvokeCount >= InvokeCountMax)
            {
                ((AutoResetEvent)state).Set();
            }
            else
            {
                InvokeCount++;
                OnUpdate(this, new EventArgs());
            }
        }

        //注：Action函数在特殊需要时再设置为公开（不让用户直接使用），用户使用Trigger.Run自带线程启动

        /// <summary>
        /// 常规触发器主体事件发布动作（重复执行则什么也不做），在当前线程创建常规触发器并执行事件委托（提前定义事件委托变量Trigger.Awake/Start/Update/End/Destroy += 要委托执行的函数，即完成事件注册到函数），可预先自定义计时器Updata阶段的执行间隔（否则默认以Duetime=0、Period=1000运行计时器）。注：若直接调用本函数则在计时器Updata阶段会暂停当前线程，不想暂停请额外开线程手动加载Action运行或使用Trigger.Run自带线程启动（推荐）
        /// </summary>
        private void Action()
        {
            if (AutoResetEvent_Trigger == null)
            {
                //private void OnAwake(object sender, EventArgs e)
                //sender参数用于传递指向事件源对象的引用，简单来讲就是当前的对象
                //sender参数也可填任意类型，填this的话传递后能从本实例类中得到字段值等信息，要传递事件变量可写在本类里
                //e参数是是EventArgs类型，简单来理解就是记录事件传递过来的额外信息
                //e参数可通过自定义类继承EventArgs类，里面记录额外事件变量，该类以参数填入完成传递

                AutoResetEvent_Trigger = new AutoResetEvent(false);
                //执行委托并传递事件参数
                OnAwake(this, new EventArgs());
                OnStart(this, new EventArgs());
                if (Duetime < 0) { Duetime = 0; }
                if (Period <= 0) { Period = 1000; }
                Timer = new Timer(CheckStatus, AutoResetEvent_Trigger, Duetime, Period);
                AutoResetEvent_Trigger.WaitOne();
                OnEnd(this, new EventArgs());
                AutoResetEvent_Trigger.WaitOne();
                Timer.Dispose();
                OnDestroy(this, new EventArgs());
            }
        }

        /// <summary>
        /// 常规触发器主体事件发布动作（重复执行则什么也不做），在当前线程创建常规触发器并执行事件委托（提前定义事件委托变量Trigger.Awake/Start/Update/End/Destroy += 要委托执行的函数，即完成事件注册到函数），可预先自定义计时器Updata阶段的执行间隔（否则默认以Duetime=0、Period=1000运行计时器）。注：若直接调用本函数则在计时器Updata阶段会暂停当前线程，不想暂停请额外开线程手动加载Action运行或使用Trigger.Run自带线程启动（推荐）
        /// </summary>
        /// <param name="duetime">Updata阶段执行开始前等待（毫秒），仅生效一次</param>
        /// <param name="period">Updata阶段执行间隔（毫秒）</param>
        private void Action(int duetime, int period)
        {
            if (AutoResetEvent_Trigger == null)
            {
                AutoResetEvent_Trigger = new AutoResetEvent(false);
                OnAwake(this, new EventArgs());
                OnStart(this, new EventArgs());
                if (Duetime < 0) { Duetime = 0; }
                if (Period <= 0) { Period = 1000; }
                Timer = new Timer(CheckStatus, AutoResetEvent_Trigger, duetime, period);
                AutoResetEvent_Trigger.WaitOne();
                OnEnd(this, new EventArgs());
                AutoResetEvent_Trigger.WaitOne();
                Timer.Dispose();
                OnDestroy(this, new EventArgs());
            }
        }

        #region 声明常规委托，安全执行事件委托并传递事件参数

        /// <summary>
        /// 计时器唤醒阶段时运行一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAwake(object sender, EventArgs e)
        {
            TriggerEventHandler triggerEventHandler = (TriggerEventHandler)_listEventDelegates[awakeEventKey];
            //triggerEventHandler?.Invoke(sender, e);
            if (triggerEventHandler != null)
            {
                triggerEventHandler.Invoke(sender, e);
            }
        }

        /// <summary>
        /// 常规触发器开始阶段运行一次
        /// </summary>
        private void OnStart(object sender, EventArgs e)
        {
            TriggerEventHandler triggerEventHandler = (TriggerEventHandler)_listEventDelegates[startEventKey];
            //triggerEventHandler?.Invoke(sender, e);
            if (triggerEventHandler != null)
            {
                triggerEventHandler.Invoke(sender, e);
            }
        }

        /// <summary>
        /// 常规触发器Update阶段按预设间隔反复运行
        /// </summary>
        private void OnUpdate(object sender, EventArgs e)
        {
            TriggerEventHandler triggerEventHandler = (TriggerEventHandler)_listEventDelegates[updateEventKey];
            //triggerEventHandler?.Invoke(sender, e);
            if (triggerEventHandler != null)
            {
                triggerEventHandler.Invoke(sender, e);
            }
        }

        /// <summary>
        /// 常规触发器结束阶段运行一次
        /// </summary>
        private void OnEnd(object sender, EventArgs e)
        {
            TriggerEventHandler triggerEventHandler = (TriggerEventHandler)_listEventDelegates[endEventKey];
            //triggerEventHandler?.Invoke(sender, e);
            if (triggerEventHandler != null)
            {
                triggerEventHandler.Invoke(sender, e);
            }
        }

        /// <summary>
        /// 常规触发器摧毁阶段运行一次
        /// </summary>
        private void OnDestroy(object sender, EventArgs e)
        {
            TriggerEventHandler triggerEventHandler = (TriggerEventHandler)_listEventDelegates[destroyEventKey];
            //triggerEventHandler?.Invoke(sender, e);
            if (triggerEventHandler != null)
            {
                triggerEventHandler.Invoke(sender, e);
            }
        }

        #endregion

        #region 自动创建线程执行常规触发器（模拟触发器运行）

        /// <summary>
        /// 自动创建线程启动常规触发器（模拟触发器运行），重复启动时什么也不做，未设置Update属性则默认以Duetime=0、Period=1000运行计时器循环
        /// </summary>
        /// <param name="isBackground">true将启动线程调整为后台线程</param>
        public void Run(bool isBackground)
        {
            if (Thread == null)
            {
                Thread = new Thread(Action) { IsBackground = isBackground };
                Thread.Start();
            }
        }

        #endregion

    }
}
