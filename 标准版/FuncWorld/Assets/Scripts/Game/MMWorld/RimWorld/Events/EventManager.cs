using UnityEngine;
using System.Collections.Generic;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 事件管理器
    /// 管理游戏中的随机事件
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        #region 单例

        public static EventManager Instance { get; private set; }

        #endregion

        #region 设置

        /// <summary>
        /// 基础事件触发概率(每天)
        /// </summary>
        public float baseEventChance = 0.2f;

        /// <summary>
        /// 最小事件间隔(天数)
        /// </summary>
        public int minEventInterval = 3;

        /// <summary>
        /// 危险事件概率权重
        /// </summary>
        public float dangerEventWeight = 0.3f;

        /// <summary>
        /// 中立事件概率权重
        /// </summary>
        public float neutralEventWeight = 0.4f;

        /// <summary>
        /// 正面事件概率权重
        /// </summary>
        public float positiveEventWeight = 0.3f;

        #endregion

        #region 当前状态

        /// <summary>
        /// 当前活动事件
        /// </summary>
        public GameEvent currentEvent;

        /// <summary>
        /// 事件队列
        /// </summary>
        public Queue<GameEvent> eventQueue = new Queue<GameEvent>();

        /// <summary>
        /// 距离上一次事件的天数
        /// </summary>
        private int daysSinceLastEvent;

        /// <summary>
        /// 事件强度(随天数增加)
        /// </summary>
        private float eventIntensity;

        #endregion

        #region 事件

        public event Action<GameEvent> OnEventTriggered;
        public event Action<GameEvent> OnEventCompleted;
        public event Action<GameEvent> OnEventCanceled;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 订阅天数变化事件
            TimeManager.Instance.OnDayChanged += HandleDayChanged;
        }

        #endregion

        #region 事件触发逻辑

        /// <summary>
        /// 处理天数变化
        /// </summary>
        private void HandleDayChanged(int day)
        {
            daysSinceLastEvent++;
            eventIntensity = 1 + (day / 10f) * 0.1f; // 每10天增加10%强度

            // 检查是否应该触发事件
            if (daysSinceLastEvent >= minEventInterval)
            {
                TryTriggerEvent();
            }
        }

        /// <summary>
        /// 尝试触发事件
        /// </summary>
        private void TryTriggerEvent()
        {
            float roll = UnityEngine.Random.value;

            // 根据概率权重决定是否触发事件
            if (roll < baseEventChance * eventIntensity)
            {
                GameEvent newEvent = GenerateRandomEvent();
                if (newEvent != null)
                {
                    TriggerEvent(newEvent);
                }
            }
        }

        /// <summary>
        /// 生成随机事件
        /// </summary>
        private GameEvent GenerateRandomEvent()
        {
            // 根据权重选择事件类型
            float roll = UnityEngine.Random.value;
            float totalWeight = dangerEventWeight + neutralEventWeight + positiveEventWeight;

            // 确定事件类别
            if (roll < dangerEventWeight / totalWeight)
            {
                return GenerateDangerEvent();
            }
            else if (roll < (dangerEventWeight + neutralEventWeight) / totalWeight)
            {
                return GenerateNeutralEvent();
            }
            else
            {
                return GeneratePositiveEvent();
            }
        }

        /// <summary>
        /// 生成危险事件
        /// </summary>
        private GameEvent GenerateDangerEvent()
        {
            List<EventType> dangerEvents = new List<EventType>
            {
                EventType.Raid,
                EventType.PredatorAttack,
                EventType.DiseaseOutbreak,
                EventType.Fire,
                EventType.ToxicFallout,
                EventType.MechCluster
            };

            EventType selected = dangerEvents[UnityEngine.Random.Range(0, dangerEvents.Count)];
            return CreateEvent(selected);
        }

        /// <summary>
        /// 生成中立事件
        /// </summary>
        private GameEvent GenerateNeutralEvent()
        {
            List<EventType> neutralEvents = new List<EventType>
            {
                EventType.CaravanArrival,
                EventType.WandererJoin,
                EventType.ShipCrash,
                EventType.AnimalHerd,
                EventType.WeatherExtremes
            };

            EventType selected = neutralEvents[UnityEngine.Random.Range(0, neutralEvents.Count)];
            return CreateEvent(selected);
        }

        /// <summary>
        /// 生成正面事件
        /// </summary>
        private GameEvent GeneratePositiveEvent()
        {
            List<EventType> positiveEvents = new List<EventType>
            {
                EventType.GoodWeather,
                EventType.Bounty,
                EventType.TradeCaravan,
                EventType.GiftShip,
                EventType.MineralDiscovery
            };

            EventType selected = positiveEvents[UnityEngine.Random.Range(0, positiveEvents.Count)];
            return CreateEvent(selected);
        }

        /// <summary>
        /// 创建事件
        /// </summary>
        private GameEvent CreateEvent(EventType type)
        {
            GameEvent gameEvent = new GameEvent();
            gameEvent.eventType = type;
            gameEvent.intensity = eventIntensity;
            gameEvent.timeRemaining = GetEventDuration(type);

            // 设置事件参数
            SetEventParameters(gameEvent);

            return gameEvent;
        }

        /// <summary>
        /// 设置事件参数
        /// </summary>
        private void SetEventParameters(GameEvent gameEvent)
        {
            switch (gameEvent.eventType)
            {
                case EventType.Raid:
                    gameEvent.title = "袭击!";
                    gameEvent.description = "一群掠夺者正在接近你的殖民地!";
                    gameEvent.parameter1 = Mathf.RoundToInt(3 + eventIntensity * 2); // 袭击者数量
                    break;

                case EventType.CaravanArrival:
                    gameEvent.title = "商队到达";
                    gameEvent.description = "一支商队到达了你的殖民地.";
                    break;

                case EventType.WandererJoin:
                    gameEvent.title = "流浪者请求加入";
                    gameEvent.description = "一名流浪者希望加入你的殖民地.";
                    break;

                case EventType.PredatorAttack:
                    gameEvent.title = "野兽来袭";
                    gameEvent.description = "一只野兽正在攻击殖民地!";
                    break;

                case EventType.DiseaseOutbreak:
                    gameEvent.title = "疾病爆发";
                    gameEvent.description = "殖民地中爆发了疾病!";
                    break;

                case EventType.Fire:
                    gameEvent.title = "火灾!";
                    gameEvent.description = "建筑着火了!";
                    break;

                case EventType.GoodWeather:
                    gameEvent.title = "好天气";
                    gameEvent.description = "天气变得晴朗宜人.";
                    break;

                case EventType.Bounty:
                    gameEvent.title = "赏金";
                    gameEvent.description = "发现了一笔意外之财!";
                    gameEvent.parameter1 = Mathf.RoundToInt(100 + eventIntensity * 50);
                    break;

                case EventType.TradeCaravan:
                    gameEvent.title = "贸易商队";
                    gameEvent.description = "一支贸易商队到达,可以进行交易.";
                    break;

                case EventType.ShipCrash:
                    gameEvent.title = "飞船坠毁";
                    gameEvent.description = "一艘飞船在附近坠毁了!";
                    break;

                case EventType.ToxicFallout:
                    gameEvent.title = "有毒尘埃";
                    gameEvent.description = "有毒尘埃正在飘落!";
                    gameEvent.timeRemaining = 600f; // 10分钟
                    break;

                case EventType.AnimalHerd:
                    gameEvent.title = "动物迁徙";
                    gameEvent.description = "一群动物正在附近迁徙.";
                    break;

                case EventType.GiftShip:
                    gameEvent.title = "礼物飞船";
                    gameEvent.description = "一艘礼物飞船到达!";
                    break;

                case EventType.MineralDiscovery:
                    gameEvent.title = "矿脉发现";
                    gameEvent.description = "在殖民地附近发现了新的矿脉!";
                    break;

                case EventType.WeatherExtremes:
                    gameEvent.title = "极端天气";
                    gameEvent.description = "极端天气正在影响殖民地!";
                    break;

                case EventType.MechCluster:
                    gameEvent.title = "机械集群";
                    gameEvent.description = "一群机械族正在接近!";
                    gameEvent.parameter1 = Mathf.RoundToInt(1 + eventIntensity);
                    break;
            }
        }

        /// <summary>
        /// 获取事件持续时间
        /// </summary>
        private float GetEventDuration(EventType type)
        {
            switch (type)
            {
                case EventType.ToxicFallout:
                    return 600f; // 10分钟
                case EventType.DiseaseOutbreak:
                    return 300f; // 5分钟
                case EventType.Fire:
                    return 60f; // 1分钟
                default:
                    return 30f; // 30秒
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerEvent(GameEvent gameEvent)
        {
            // 如果当前有事件正在处理,加入队列
            if (currentEvent != null)
            {
                eventQueue.Enqueue(gameEvent);
                return;
            }

            currentEvent = gameEvent;
            currentEvent.status = EventStatus.Active;
            daysSinceLastEvent = 0;

            // 触发事件效果
            ApplyEventEffects(gameEvent);

            OnEventTriggered?.Invoke(gameEvent);
            Debug.Log($"触发事件: {gameEvent.title}");

            // 显示事件通知
            UIManager.Instance.ShowEventNotification(gameEvent);
        }

        /// <summary>
        /// 应用事件效果
        /// </summary>
        private void ApplyEventEffects(GameEvent gameEvent)
        {
            switch (gameEvent.eventType)
            {
                case EventType.Raid:
                    SpawnRaiders(gameEvent.parameter1);
                    break;

                case EventType.Bounty:
                    AddBounty(gameEvent.parameter1);
                    break;

                case EventType.DiseaseOutbreak:
                    SpreadDisease();
                    break;

                case EventType.Fire:
                    StartFire();
                    break;

                case EventType.WandererJoin:
                    SpawnWanderer();
                    break;

                case EventType.ToxicFallout:
                    StartToxicFallout();
                    break;

                case EventType.MechCluster:
                    SpawnMechs(gameEvent.parameter1);
                    break;
            }
        }

        /// <summary>
        /// 生成袭击者
        /// </summary>
        private void SpawnRaiders(int count)
        {
            Debug.Log($"生成 {count} 名袭击者");
            // TODO: 实现袭击者生成逻辑
        }

        /// <summary>
        /// 添加赏金
        /// </summary>
        private void AddBounty(int amount)
        {
            ResourceManager.Instance.AddResource(ThingDefDatabase.GetThingDef("Steel"), amount);
            Debug.Log($"获得赏金: {amount} 钢材");
        }

        /// <summary>
        /// 传播疾病
        /// </summary>
        private void SpreadDisease()
        {
            Pawn[] pawns = FindObjectsOfType<Pawn>();
            foreach (var pawn in pawns)
            {
                if (UnityEngine.Random.value < 0.3f)
                {
                    // TODO: 实现疾病系统
                    Debug.Log($"{pawn.name} 生病了");
                }
            }
        }

        /// <summary>
        /// 开始火灾
        /// </summary>
        private void StartFire()
        {
            // TODO: 实现火灾系统
            Debug.Log("发生火灾!");
        }

        /// <summary>
        /// 生成流浪者
        /// </summary>
        private void SpawnWanderer()
        {
            // TODO: 实现流浪者生成逻辑
            Debug.Log("流浪者请求加入");
        }

        /// <summary>
        /// 开始有毒尘埃
        /// </summary>
        private void StartToxicFallout()
        {
            // TODO: 实现有毒尘埃效果
            Debug.Log("有毒尘埃开始飘落!");
        }

        /// <summary>
        /// 生成机械族
        /// </summary>
        private void SpawnMechs(int count)
        {
            Debug.Log($"生成 {count} 个机械族");
            // TODO: 实现机械族生成逻辑
        }

        /// <summary>
        /// 完成事件
        /// </summary>
        public void CompleteEvent()
        {
            if (currentEvent == null) return;

            currentEvent.status = EventStatus.Completed;
            OnEventCompleted?.Invoke(currentEvent);

            Debug.Log($"事件完成: {currentEvent.title}");

            // 处理下一个事件
            currentEvent = null;
            if (eventQueue.Count > 0)
            {
                TriggerEvent(eventQueue.Dequeue());
            }
        }

        /// <summary>
        /// 取消事件
        /// </summary>
        public void CancelEvent()
        {
            if (currentEvent == null) return;

            currentEvent.status = EventStatus.Canceled;
            OnEventCanceled?.Invoke(currentEvent);

            Debug.Log($"事件取消: {currentEvent.title}");

            // 处理下一个事件
            currentEvent = null;
            if (eventQueue.Count > 0)
            {
                TriggerEvent(eventQueue.Dequeue());
            }
        }

        #endregion

        #region 手动触发事件

        /// <summary>
        /// 手动触发事件
        /// </summary>
        public void TriggerEventManually(EventType type)
        {
            GameEvent gameEvent = CreateEvent(type);
            TriggerEvent(gameEvent);
        }

        #endregion
    }

    #region 事件类

    [Serializable]
    public class GameEvent
    {
        public EventType eventType;
        public string title;
        public string description;
        public float intensity;
        public float timeRemaining;
        public int parameter1;
        public int parameter2;
        public EventStatus status;
    }

    public enum EventType
    {
        // 危险事件
        Raid,            // 袭击
        PredatorAttack,  // 野兽攻击
        DiseaseOutbreak, // 疾病爆发
        Fire,            // 火灾
        ToxicFallout,    // 有毒尘埃
        MechCluster,     // 机械集群

        // 中立事件
        CaravanArrival,  // 商队到达
        WandererJoin,    // 流浪者加入
        ShipCrash,       // 飞船坠毁
        AnimalHerd,      // 动物迁徙
        WeatherExtremes, // 极端天气

        // 正面事件
        GoodWeather,     // 好天气
        Bounty,          // 赏金
        TradeCaravan,    // 贸易商队
        GiftShip,        // 礼物飞船
        MineralDiscovery // 矿脉发现
    }

    public enum EventStatus
    {
        Pending,    // 等待中
        Active,     // 进行中
        Completed,  // 已完成
        Canceled    // 已取消
    }

    #endregion
}