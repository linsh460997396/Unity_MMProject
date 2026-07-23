using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MMWorld.SimAI
{
    /// <summary>
    /// UI管理器
    /// 管理游戏中的所有UI界面
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region 单例

        public static UIManager Instance { get; private set; }

        #endregion

        #region UI组件

        /// <summary>
        /// 主画布
        /// </summary>
        private Canvas mainCanvas;

        /// <summary>
        /// 顶部状态栏
        /// </summary>
        private GameObject topBar;

        /// <summary>
        /// 资源栏
        /// </summary>
        private GameObject resourceBar;

        /// <summary>
        /// 殖民者面板
        /// </summary>
        private GameObject pawnPanel;

        /// <summary>
        /// 建造菜单
        /// </summary>
        private GameObject buildMenu;

        /// <summary>
        /// 事件通知面板
        /// </summary>
        private GameObject eventNotificationPanel;

        /// <summary>
        /// 时间显示
        /// </summary>
        private TextMeshProUGUI timeText;

        /// <summary>
        /// 天气显示
        /// </summary>
        private TextMeshProUGUI weatherText;

        /// <summary>
        /// 温度显示
        /// </summary>
        private TextMeshProUGUI temperatureText;

        #endregion

        #region 设置

        /// <summary>
        /// 字体
        /// </summary>
        public TMP_FontAsset font;

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
            // 创建UI
            CreateUI();

            // 订阅事件
            TimeManager.Instance.OnTimeChanged += UpdateTimeDisplay;
            TimeManager.Instance.OnDayChanged += UpdateDayDisplay;
            TimeManager.Instance.OnSeasonChanged += UpdateSeasonDisplay;
            WeatherManager.Instance.OnWeatherChanged += UpdateWeatherDisplay;
            WeatherManager.Instance.OnTemperatureChanged += UpdateTemperatureDisplay;
            ResourceManager.Instance.OnResourceChanged += UpdateResourceDisplay;
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建UI
        /// </summary>
        private void CreateUI()
        {
            // 创建主画布
            CreateMainCanvas();

            // 创建顶部状态栏
            CreateTopBar();

            // 创建资源栏
            CreateResourceBar();

            // 创建殖民者面板
            CreatePawnPanel();

            // 创建建造菜单
            CreateBuildMenu();

            // 创建事件通知面板
            CreateEventNotificationPanel();
        }

        /// <summary>
        /// 创建主画布
        /// </summary>
        private void CreateMainCanvas()
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            canvasObj.transform.SetParent(transform);
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建事件系统
            CreateEventSystem();
        }

        /// <summary>
        /// 创建事件系统
        /// </summary>
        private void CreateEventSystem()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        /// <summary>
        /// 创建顶部状态栏
        /// </summary>
        private void CreateTopBar()
        {
            topBar = new GameObject("TopBar");
            topBar.transform.SetParent(mainCanvas.transform);
            RectTransform topBarRect = topBar.AddComponent<RectTransform>();
            topBarRect.anchorMin = new Vector2(0, 0.9f);
            topBarRect.anchorMax = new Vector2(1, 1);
            topBarRect.offsetMin = Vector2.zero;
            topBarRect.offsetMax = Vector2.zero;

            Image bgImage = topBar.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // 时间显示
            CreateTimeDisplay(topBar.transform);

            // 天气显示
            CreateWeatherDisplay(topBar.transform);

            // 温度显示
            CreateTemperatureDisplay(topBar.transform);

            // 时间控制按钮
            CreateTimeControls(topBar.transform);
        }

        /// <summary>
        /// 创建时间显示
        /// </summary>
        private void CreateTimeDisplay(Transform parent)
        {
            GameObject timeObj = new GameObject("TimeDisplay");
            timeObj.transform.SetParent(parent);
            RectTransform rect = timeObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.02f, 0.1f);
            rect.anchorMax = new Vector2(0.15f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            timeText = timeObj.AddComponent<TextMeshProUGUI>();
            timeText.font = font;
            timeText.fontSize = 24;
            timeText.color = Color.white;
            timeText.alignment = TextAlignmentOptions.Left;
            UpdateTimeDisplay(0);
        }

        /// <summary>
        /// 创建天气显示
        /// </summary>
        private void CreateWeatherDisplay(Transform parent)
        {
            GameObject weatherObj = new GameObject("WeatherDisplay");
            weatherObj.transform.SetParent(parent);
            RectTransform rect = weatherObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.45f, 0.1f);
            rect.anchorMax = new Vector2(0.55f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            weatherText = weatherObj.AddComponent<TextMeshProUGUI>();
            weatherText.font = font;
            weatherText.fontSize = 24;
            weatherText.color = Color.white;
            weatherText.alignment = TextAlignmentOptions.Center;
            UpdateWeatherDisplay(WeatherType.Clear);
        }

        /// <summary>
        /// 创建温度显示
        /// </summary>
        private void CreateTemperatureDisplay(Transform parent)
        {
            GameObject tempObj = new GameObject("TemperatureDisplay");
            tempObj.transform.SetParent(parent);
            RectTransform rect = tempObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.85f, 0.1f);
            rect.anchorMax = new Vector2(0.98f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            temperatureText = tempObj.AddComponent<TextMeshProUGUI>();
            temperatureText.font = font;
            temperatureText.fontSize = 24;
            temperatureText.color = Color.white;
            temperatureText.alignment = TextAlignmentOptions.Right;
            UpdateTemperatureDisplay(20f);
        }

        /// <summary>
        /// 创建时间控制按钮
        /// </summary>
        private void CreateTimeControls(Transform parent)
        {
            float buttonWidth = 60;
            float buttonHeight = 40;

            // 暂停按钮
            CreateTimeButton(parent, "PauseButton", "⏸", new Vector2(0.35f, 0.5f), buttonWidth, buttonHeight, () =>
            {
                TimeManager.Instance.Pause();
            });

            // 正常速度按钮
            CreateTimeButton(parent, "NormalSpeedButton", "1×", new Vector2(0.39f, 0.5f), buttonWidth, buttonHeight, () =>
            {
                TimeManager.Instance.SetTimeSpeed(1f);
            });

            // 2倍速度按钮
            CreateTimeButton(parent, "DoubleSpeedButton", "2×", new Vector2(0.43f, 0.5f), buttonWidth, buttonHeight, () =>
            {
                TimeManager.Instance.SetTimeSpeed(2f);
            });

            // 3倍速度按钮
            CreateTimeButton(parent, "TripleSpeedButton", "3×", new Vector2(0.47f, 0.5f), buttonWidth, buttonHeight, () =>
            {
                TimeManager.Instance.SetTimeSpeed(3f);
            });
        }

        /// <summary>
        /// 创建时间控制按钮
        /// </summary>
        private void CreateTimeButton(Transform parent, string name, string text, Vector2 position, 
            float width, float height, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(position.x - width / 1920f, position.y - height / 1080f / 2);
            btnRect.anchorMax = new Vector2(position.x + width / 1920f, position.y + height / 1080f / 2);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.3f, 0.3f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;
            btn.onClick.AddListener(onClick);

            // 按钮文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.font = font;
            tmpText.text = text;
            tmpText.color = Color.white;
            tmpText.fontSize = 18;
            tmpText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// 创建资源栏
        /// </summary>
        private void CreateResourceBar()
        {
            resourceBar = new GameObject("ResourceBar");
            resourceBar.transform.SetParent(mainCanvas.transform);
            RectTransform rect = resourceBar.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.25f, 0.15f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bgImage = resourceBar.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(resourceBar.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.8f);
            titleRect.anchorMax = new Vector2(0.95f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.font = font;
            titleText.text = "资源";
            titleText.fontSize = 20;
            titleText.color = new Color(0.9f, 0.7f, 0.3f);
            titleText.alignment = TextAlignmentOptions.Left;

            // 资源列表
            UpdateResourceDisplay(null, 0);
        }

        /// <summary>
        /// 创建殖民者面板
        /// </summary>
        private void CreatePawnPanel()
        {
            pawnPanel = new GameObject("PawnPanel");
            pawnPanel.transform.SetParent(mainCanvas.transform);
            RectTransform rect = pawnPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.75f, 0);
            rect.anchorMax = new Vector2(1, 0.4f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bgImage = pawnPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(pawnPanel.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.font = font;
            titleText.text = "殖民者";
            titleText.fontSize = 20;
            titleText.color = new Color(0.9f, 0.7f, 0.3f);
            titleText.alignment = TextAlignmentOptions.Left;

            // 显示殖民者列表
            UpdatePawnDisplay();
        }

        /// <summary>
        /// 创建建造菜单
        /// </summary>
        private void CreateBuildMenu()
        {
            buildMenu = new GameObject("BuildMenu");
            buildMenu.transform.SetParent(mainCanvas.transform);
            RectTransform rect = buildMenu.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f - 0.2f, 0.5f - 0.3f);
            rect.anchorMax = new Vector2(0.5f + 0.2f, 0.5f + 0.3f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            buildMenu.SetActive(false);

            Image bgImage = buildMenu.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(buildMenu.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.9f);
            titleRect.anchorMax = new Vector2(0.95f, 0.98f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.font = font;
            titleText.text = "建造菜单";
            titleText.fontSize = 24;
            titleText.color = new Color(0.9f, 0.7f, 0.3f);
            titleText.alignment = TextAlignmentOptions.Center;

            // 关闭按钮
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(buildMenu.transform);
            RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.92f, 0.9f);
            closeRect.anchorMax = new Vector2(0.98f, 0.98f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            Image closeImage = closeBtnObj.AddComponent<Image>();
            closeImage.color = new Color(0.6f, 0.2f, 0.2f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImage;
            closeBtn.onClick.AddListener(() => buildMenu.SetActive(false));
            TextMeshProUGUI closeText = closeBtnObj.AddComponent<TextMeshProUGUI>();
            closeText.font = font;
            closeText.text = "X";
            closeText.fontSize = 20;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;

            // 建造按钮网格
            CreateBuildButtons();
        }

        /// <summary>
        /// 创建建造按钮
        /// </summary>
        private void CreateBuildButtons()
        {
            var buildingDefs = BuildingDefDatabase.GetAllBuildingDefs();
            float buttonWidth = 150;
            float buttonHeight = 40;
            float spacing = 10;
            int columns = 2;

            for (int i = 0; i < buildingDefs.Count; i++)
            {
                BuildingDef def = buildingDefs[i];
                float x = 0.05f + (i % columns) * (buttonWidth + spacing) / 384f; // 0.2*1920=384
                float y = 0.75f - (i / columns) * (buttonHeight + spacing) / 648f; // 0.6*1080=648

                GameObject btnObj = new GameObject($"BuildButton_{def.defName}");
                btnObj.transform.SetParent(buildMenu.transform);
                RectTransform btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(x, y - buttonHeight / 1080f / 2);
                btnRect.anchorMax = new Vector2(x + buttonWidth / 384f, y + buttonHeight / 1080f / 2);
                btnRect.offsetMin = Vector2.zero;
                btnRect.offsetMax = Vector2.zero;

                Image btnImage = btnObj.AddComponent<Image>();
                btnImage.color = new Color(0.2f, 0.4f, 0.2f);

                Button btn = btnObj.AddComponent<Button>();
                btn.targetGraphic = btnImage;

                // 检查材料是否足够
                bool canBuild = ConstructionManager.Instance.HasEnoughMaterials(def);
                if (!canBuild)
                {
                    btnImage.color = new Color(0.3f, 0.3f, 0.3f);
                    btn.interactable = false;
                }

                btn.onClick.AddListener(() => OnBuildButtonClicked(def));

                // 按钮文本
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                tmpText.font = font;
                tmpText.text = def.label;
                tmpText.color = Color.white;
                tmpText.fontSize = 16;
                tmpText.alignment = TextAlignmentOptions.Center;
            }
        }

        /// <summary>
        /// 创建事件通知面板
        /// </summary>
        private void CreateEventNotificationPanel()
        {
            eventNotificationPanel = new GameObject("EventNotificationPanel");
            eventNotificationPanel.transform.SetParent(mainCanvas.transform);
            RectTransform rect = eventNotificationPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.3f, 0.4f);
            rect.anchorMax = new Vector2(0.7f, 0.6f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            eventNotificationPanel.SetActive(false);

            Image bgImage = eventNotificationPanel.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.1f, 0.1f, 0.95f);

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(eventNotificationPanel.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.65f);
            titleRect.anchorMax = new Vector2(0.95f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.font = font;
            titleText.fontSize = 32;
            titleText.color = new Color(0.9f, 0.3f, 0.3f);
            titleText.alignment = TextAlignmentOptions.Center;

            // 描述
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(eventNotificationPanel.transform);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.05f, 0.25f);
            descRect.anchorMax = new Vector2(0.95f, 0.6f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.font = font;
            descText.fontSize = 20;
            descText.color = Color.white;
            descText.alignment = TextAlignmentOptions.Center;

            // 确认按钮
            GameObject confirmBtnObj = new GameObject("ConfirmButton");
            confirmBtnObj.transform.SetParent(eventNotificationPanel.transform);
            RectTransform confirmRect = confirmBtnObj.AddComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.35f, 0.05f);
            confirmRect.anchorMax = new Vector2(0.65f, 0.2f);
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;
            Image confirmImage = confirmBtnObj.AddComponent<Image>();
            confirmImage.color = new Color(0.2f, 0.5f, 0.2f);
            Button confirmBtn = confirmBtnObj.AddComponent<Button>();
            confirmBtn.targetGraphic = confirmImage;
            confirmBtn.onClick.AddListener(HideEventNotification);
            TextMeshProUGUI confirmText = confirmBtnObj.AddComponent<TextMeshProUGUI>();
            confirmText.font = font;
            confirmText.text = "知道了";
            confirmText.fontSize = 20;
            confirmText.color = Color.white;
            confirmText.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region UI更新

        /// <summary>
        /// 更新时间显示
        /// </summary>
        private void UpdateTimeDisplay(float time)
        {
            TimeManager timeManager = TimeManager.Instance;
            timeText.text = $"第 {timeManager.currentDay} 天 {timeManager.GetSeasonName(timeManager.currentSeason)} - {timeManager.GetCurrentTimeString()}";
        }

        /// <summary>
        /// 更新天数显示
        /// </summary>
        private void UpdateDayDisplay(int day)
        {
            UpdateTimeDisplay(TimeManager.Instance.currentTime);
        }

        /// <summary>
        /// 更新季节显示
        /// </summary>
        private void UpdateSeasonDisplay(Season season)
        {
            UpdateTimeDisplay(TimeManager.Instance.currentTime);
        }

        /// <summary>
        /// 更新天气显示
        /// </summary>
        private void UpdateWeatherDisplay(WeatherType weather)
        {
            weatherText.text = WeatherManager.Instance.GetWeatherName(weather);
        }

        /// <summary>
        /// 更新温度显示
        /// </summary>
        private void UpdateTemperatureDisplay(float temperature)
        {
            temperatureText.text = WeatherManager.Instance.GetTemperatureString();
        }

        /// <summary>
        /// 更新资源显示
        /// </summary>
        private void UpdateResourceDisplay(ThingDef thingDef, int count)
        {
            // 清除现有资源项
            foreach (Transform child in resourceBar.transform)
            {
                if (child.name.StartsWith("Resource_"))
                {
                    Destroy(child.gameObject);
                }
            }

            // 添加资源项
            int index = 0;
            foreach (var pair in ResourceManager.Instance.globalResources)
            {
                if (pair.Value > 0)
                {
                    CreateResourceItem(resourceBar.transform, pair.Key, pair.Value, index);
                    index++;
                }
            }
        }

        /// <summary>
        /// 创建资源项
        /// </summary>
        private void CreateResourceItem(Transform parent, ThingDef thingDef, int count, int index)
        {
            GameObject itemObj = new GameObject($"Resource_{thingDef.defName}");
            itemObj.transform.SetParent(parent);
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            float y = 0.65f - index * 0.12f;
            rect.anchorMin = new Vector2(0.05f, y);
            rect.anchorMax = new Vector2(0.95f, y + 0.1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = itemObj.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.text = $"{thingDef.label}: {count}";
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
        }

        /// <summary>
        /// 更新殖民者显示
        /// </summary>
        private void UpdatePawnDisplay()
        {
            // 清除现有殖民者项
            foreach (Transform child in pawnPanel.transform)
            {
                if (child.name.StartsWith("Pawn_"))
                {
                    Destroy(child.gameObject);
                }
            }

            // 添加殖民者项
            Pawn[] pawns = FindObjectsOfType<Pawn>();
            for (int i = 0; i < pawns.Length && i < 5; i++)
            {
                CreatePawnItem(pawnPanel.transform, pawns[i], i);
            }
        }

        /// <summary>
        /// 创建殖民者项
        /// </summary>
        private void CreatePawnItem(Transform parent, Pawn pawn, int index)
        {
            GameObject itemObj = new GameObject($"Pawn_{pawn.name}");
            itemObj.transform.SetParent(parent);
            RectTransform rect = itemObj.AddComponent<RectTransform>();
            float y = 0.75f - index * 0.15f;
            rect.anchorMin = new Vector2(0.05f, y);
            rect.anchorMax = new Vector2(0.95f, y + 0.13f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bgImage = itemObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f);

            Button btn = itemObj.AddComponent<Button>();
            btn.targetGraphic = bgImage;
            btn.onClick.AddListener(() => ShowPawnDetails(pawn));

            // 名称
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(itemObj.transform);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.9f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.font = font;
            nameText.text = pawn.name;
            nameText.fontSize = 16;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Left;

            // 健康
            GameObject healthObj = new GameObject("Health");
            healthObj.transform.SetParent(itemObj.transform);
            RectTransform healthRect = healthObj.AddComponent<RectTransform>();
            healthRect.anchorMin = new Vector2(0.55f, 0.6f);
            healthRect.anchorMax = new Vector2(0.95f, 0.8f);
            healthRect.offsetMin = Vector2.zero;
            healthRect.offsetMax = Vector2.zero;

            // 健康条背景
            Image healthBg = healthObj.AddComponent<Image>();
            healthBg.color = new Color(0.3f, 0.3f, 0.3f);

            // 健康条
            GameObject healthBarObj = new GameObject("HealthBar");
            healthBarObj.transform.SetParent(healthObj.transform);
            RectTransform healthBarRect = healthBarObj.AddComponent<RectTransform>();
            healthBarRect.anchorMin = Vector2.zero;
            healthBarRect.anchorMax = new Vector2(pawn.health / 100f, 1);
            healthBarRect.offsetMin = Vector2.zero;
            healthBarRect.offsetMax = Vector2.zero;
            Image healthBar = healthBarObj.AddComponent<Image>();
            healthBar.color = pawn.health > 60 ? new Color(0.2f, 0.7f, 0.2f) :
                            pawn.health > 30 ? new Color(0.7f, 0.7f, 0.2f) :
                            new Color(0.7f, 0.2f, 0.2f);
        }

        #endregion

        #region UI操作

        /// <summary>
        /// 显示事件通知
        /// </summary>
        public void ShowEventNotification(GameEvent gameEvent)
        {
            eventNotificationPanel.SetActive(true);

            TextMeshProUGUI titleText = eventNotificationPanel.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = eventNotificationPanel.transform.Find("Description").GetComponent<TextMeshProUGUI>();

            titleText.text = gameEvent.title;
            descText.text = gameEvent.description;
        }

        /// <summary>
        /// 隐藏事件通知
        /// </summary>
        public void HideEventNotification()
        {
            eventNotificationPanel.SetActive(false);
            EventManager.Instance.CompleteEvent();
        }

        /// <summary>
        /// 显示建造菜单
        /// </summary>
        public void ShowBuildMenu()
        {
            buildMenu.SetActive(true);
        }

        /// <summary>
        /// 隐藏建造菜单
        /// </summary>
        public void HideBuildMenu()
        {
            buildMenu.SetActive(false);
        }

        /// <summary>
        /// 显示殖民者详情
        /// </summary>
        private void ShowPawnDetails(Pawn pawn)
        {
            Debug.Log($"显示殖民者详情: {pawn.name}");
            // TODO: 实现殖民者详情面板
        }

        /// <summary>
        /// 建造按钮点击
        /// </summary>
        private void OnBuildButtonClicked(BuildingDef def)
        {
            Debug.Log($"选择建造: {def.label}");
            // TODO: 实现建造位置选择
            HideBuildMenu();
        }

        #endregion
    }
}