using UnityEngine;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// RimWorld游戏集成器
    /// 负责初始化和整合所有RimWorld风格的游戏系统
    /// </summary>
    public class RimWorldGameIntegrator : MonoBehaviour
    {
        #region 单例

        public static RimWorldGameIntegrator Instance { get; private set; }

        #endregion

        #region 系统引用

        [Header("RimWorld Systems")]
        public TimeManager timeManager;
        public WeatherManager weatherManager;
        public ResourceManager resourceManager;
        public ConstructionManager constructionManager;
        public EventManager eventManager;
        public UIManager uiManager;

        #endregion

        #region 状态

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool isInitialized { get; private set; }

        #endregion

        #region 事件

        public event Action OnSystemsInitialized;
        public event Action OnGameStarted;

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
            // 延迟一帧初始化,确保其他组件已Awake
            Invoke(nameof(InitializeSystems), 0.1f);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化所有系统
        /// </summary>
        private void InitializeSystems()
        {
            Debug.Log("[RimWorldGameIntegrator] 开始初始化RimWorld系统...");

            // 1. 初始化定义数据库
            InitializeDefDatabases();

            // 2. 创建核心管理器
            CreateCoreManagers();

            // 3. 初始化各系统
            InitializeTimeManager();
            InitializeWeatherManager();
            InitializeResourceManager();
            InitializeConstructionManager();
            InitializeEventManager();
            InitializeUIManager();

            // 4. 创建初始殖民者
            CreateInitialPawns();

            isInitialized = true;
            OnSystemsInitialized?.Invoke();

            Debug.Log("[RimWorldGameIntegrator] RimWorld系统初始化完成!");
        }

        /// <summary>
        /// 初始化定义数据库
        /// </summary>
        private void InitializeDefDatabases()
        {
            Debug.Log("[RimWorldGameIntegrator] 初始化定义数据库...");
            ThingDefDatabase.Initialize();
            BuildingDefDatabase.Initialize();
        }

        /// <summary>
        /// 创建核心管理器
        /// </summary>
        private void CreateCoreManagers()
        {
            // 时间管理器
            if (timeManager == null)
            {
                GameObject timeObj = new GameObject("TimeManager");
                timeManager = timeObj.AddComponent<TimeManager>();
            }

            // 天气管理器
            if (weatherManager == null)
            {
                GameObject weatherObj = new GameObject("WeatherManager");
                weatherManager = weatherObj.AddComponent<WeatherManager>();
            }

            // 资源管理器
            if (resourceManager == null)
            {
                GameObject resourceObj = new GameObject("ResourceManager");
                resourceManager = resourceObj.AddComponent<ResourceManager>();
            }

            // 建造管理器
            if (constructionManager == null)
            {
                GameObject constructionObj = new GameObject("ConstructionManager");
                constructionManager = constructionObj.AddComponent<ConstructionManager>();
            }

            // 事件管理器
            if (eventManager == null)
            {
                GameObject eventObj = new GameObject("EventManager");
                eventManager = eventObj.AddComponent<EventManager>();
            }

            // UI管理器
            if (uiManager == null)
            {
                GameObject uiObj = new GameObject("UIManager");
                uiManager = uiObj.AddComponent<UIManager>();
            }
        }

        /// <summary>
        /// 初始化时间管理器
        /// </summary>
        private void InitializeTimeManager()
        {
            timeManager.dayLength = 600f; // 10分钟一天
            timeManager.seasonLength = 15; // 15天一个季节
            timeManager.sunriseTime = 180f; // 6:00 AM
            timeManager.sunsetTime = 480f; // 8:00 PM
        }

        /// <summary>
        /// 初始化天气管理器
        /// </summary>
        private void InitializeWeatherManager()
        {
            weatherManager.baseTemperature = 20f;
            weatherManager.temperatureVariation = 10f;
            weatherManager.weatherDuration = 180f; // 3分钟换一次天气
        }

        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        private void InitializeResourceManager()
        {
            // 资源管理器会自动初始化默认资源
        }

        /// <summary>
        /// 初始化建造管理器
        /// </summary>
        private void InitializeConstructionManager()
        {
            constructionManager.constructionSpeedMultiplier = 1f;
        }

        /// <summary>
        /// 初始化事件管理器
        /// </summary>
        private void InitializeEventManager()
        {
            eventManager.baseEventChance = 0.2f;
            eventManager.minEventInterval = 3;
        }

        /// <summary>
        /// 初始化UI管理器
        /// </summary>
        private void InitializeUIManager()
        {
            // UI管理器会自动创建UI
        }

        /// <summary>
        /// 创建初始殖民者
        /// </summary>
        private void CreateInitialPawns()
        {
            Debug.Log("[RimWorldGameIntegrator] 创建初始殖民者...");

            // 创建3个初始殖民者
            CreatePawn("张三", Gender.Male, 35);
            CreatePawn("李四", Gender.Female, 28);
            CreatePawn("王五", Gender.Male, 42);
        }

        /// <summary>
        /// 创建殖民者
        /// </summary>
        private Pawn CreatePawn(string name, Gender gender, int age)
        {
            GameObject pawnObj = new GameObject(name);
            pawnObj.transform.position = new Vector3(128 + UnityEngine.Random.Range(-5, 5), 0.5f, 128 + UnityEngine.Random.Range(-5, 5));

            Pawn pawn = pawnObj.AddComponent<Pawn>();
            pawn.name = name;
            pawn.gender = gender;
            pawn.age = age;
            pawn.health = 100;
            pawn.healthState = HealthState.Healthy;

            // 随机设置一些技能
            pawn.skills.cooking.level = UnityEngine.Random.Range(1, 10);
            pawn.skills.construction.level = UnityEngine.Random.Range(1, 10);
            pawn.skills.growing.level = UnityEngine.Random.Range(1, 10);
            pawn.skills.mining.level = UnityEngine.Random.Range(1, 10);
            pawn.skills.social.level = UnityEngine.Random.Range(1, 10);

            // 设置工作偏好
            pawn.work.cookingEnabled = true;
            pawn.work.craftingEnabled = true;
            pawn.work.constructionEnabled = true;
            pawn.work.growingEnabled = true;
            pawn.work.miningEnabled = true;
            pawn.work.haulingEnabled = true;

            // 创建简单的可视化模型
            CreatePawnVisual(pawnObj, gender);

            Debug.Log($"创建殖民者: {name}, {gender}, {age}岁");
            return pawn;
        }

        /// <summary>
        /// 创建殖民者可视化模型
        /// </summary>
        private void CreatePawnVisual(GameObject pawnObj, Gender gender)
        {
            // 身体(胶囊形状)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(pawnObj.transform);
            body.transform.localPosition = new Vector3(0, 0.5f, 0);
            body.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);

            // 头部
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(pawnObj.transform);
            head.transform.localPosition = new Vector3(0, 1.1f, 0);
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            // 腿(两个立方体)
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(pawnObj.transform);
            leftLeg.transform.localPosition = new Vector3(-0.1f, 0.1f, 0);
            leftLeg.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);

            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(pawnObj.transform);
            rightLeg.transform.localPosition = new Vector3(0.1f, 0.1f, 0);
            rightLeg.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);

            // 根据性别设置颜色
            Color bodyColor = gender == Gender.Male ? 
                new Color(0.6f, 0.6f, 0.6f) : new Color(0.8f, 0.7f, 0.7f);
            
            foreach (Renderer renderer in pawnObj.GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = bodyColor;
            }

            // 头部颜色(肤色)
            Renderer headRenderer = head.GetComponent<Renderer>();
            headRenderer.material.color = new Color(0.9f, 0.75f, 0.6f);
        }

        #endregion

        #region 游戏控制

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            if (!isInitialized)
            {
                InitializeSystems();
            }

            // 启动时间流逝
            timeManager.Resume();

            OnGameStarted?.Invoke();
            Debug.Log("[RimWorldGameIntegrator] 游戏开始!");
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            timeManager.Pause();
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ResumeGame()
        {
            timeManager.Resume();
        }

        /// <summary>
        /// 设置游戏速度
        /// </summary>
        public void SetGameSpeed(float speed)
        {
            timeManager.SetTimeSpeed(speed);
        }

        #endregion

        #region 保存/加载

        /// <summary>
        /// 保存游戏
        /// </summary>
        public GameSaveData SaveGame()
        {
            return new GameSaveData
            {
                timeData = timeManager.Save(),
                weatherData = weatherManager.Save(),
                resourceData = resourceManager.Save()
            };
        }

        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame(GameSaveData data)
        {
            timeManager.Load(data.timeData);
            weatherManager.Load(data.weatherData);
            resourceManager.Load(data.resourceData);
        }

        #endregion
    }

    #region 保存数据类

    [Serializable]
    public class GameSaveData
    {
        public TimeData timeData;
        public WeatherData weatherData;
        public ResourceData resourceData;
    }

    #endregion
}