using MMWorld.HexSphere;
using SpriteSpace;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MMWorld
{
    /// <summary>
    /// 游戏开局菜单控制器 - 类似环世界的开局界面
    /// 管理星球选择、世界创建等开局流程
    /// </summary>
    public class GameStartMenu : MonoBehaviour
    {
        #region 字段

        /// <summary>
        /// 单例实例
        /// </summary>
        public static GameStartMenu Instance { get; private set; }

        /// <summary>
        /// 菜单画布
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// 背景面板
        /// </summary>
        private GameObject backgroundPanel;

        /// <summary>
        /// 主菜单面板
        /// </summary>
        private GameObject mainMenuPanel;

        /// <summary>
        /// 星球选择面板
        /// </summary>
        private GameObject planetSelectPanel;

        /// <summary>
        /// 加载进度面板
        /// </summary>
        private GameObject loadingPanel;

        /// <summary>
        /// 进度条
        /// </summary>
        private Slider progressSlider;

        /// <summary>
        /// 进度文本
        /// </summary>
        private TextMeshProUGUI progressText;

        /// <summary>
        /// 星球预设列表
        /// </summary>
        private List<PlanetPreset> planetPresets = new List<PlanetPreset>();

        /// <summary>
        /// 当前选中的星球预设
        /// </summary>
        private PlanetPreset selectedPlanet;

        /// <summary>
        /// 字体
        /// </summary>
        private TMP_FontAsset font;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        private bool isLoading = false;

        /// <summary>
        /// UI是否已创建
        /// </summary>
        private bool uiCreated = false;

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
                return;
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化字体
        /// </summary>
        private void InitializeFont()
        {
            font = SpriteSpacePrefab.FontFZYaSong;
            if (font == null)
            {
                font = TMP_Settings.defaultFontAsset;
                Debug.LogWarning("未能找到字体,将使用内置默认字体");
            }
        }

        #endregion

        #region UI创建

        /// <summary>
        /// 创建开局菜单UI
        /// </summary>
        private void CreateStartMenuUI()
        {
            if (uiCreated) return;

            // 创建画布
            canvas = CreateCanvas();

            // 创建背景
            CreateBackground();

            // 创建主菜单面板
            CreateMainMenuPanel();

            // 初始化星球预设
            InitializePlanetPresets();

            uiCreated = true;
        }

        /// <summary>
        /// 创建画布
        /// </summary>
        private Canvas CreateCanvas()
        {
            GameObject canvasObj = new GameObject("StartMenuCanvas");
            canvasObj.transform.SetParent(transform);
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 9999;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 获取或创建事件系统
            SpriteSpace.SpriteSpacePrefab.GetEventSystem();

            return c;
        }

        /// <summary>
        /// 创建背景
        /// </summary>
        private void CreateBackground()
        {
            backgroundPanel = new GameObject("Background");
            backgroundPanel.transform.SetParent(canvas.transform);
            RectTransform bgRect = backgroundPanel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = backgroundPanel.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.08f, 1f); // 深空黑色背景
        }

        /// <summary>
        /// 创建主菜单面板
        /// </summary>
        private void CreateMainMenuPanel()
        {
            mainMenuPanel = new GameObject("MainMenuPanel");
            mainMenuPanel.transform.SetParent(canvas.transform);
            RectTransform menuRect = mainMenuPanel.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            // 游戏标题
            CreateTitle(menuRect);

            // 菜单按钮
            CreateMenuButtons(menuRect);
        }

        /// <summary>
        /// 创建标题
        /// </summary>
        private void CreateTitle(RectTransform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.6f);
            titleRect.anchorMax = new Vector2(0.7f, 0.85f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleText.font = font;
            titleText.text = "FuncWorld";
            titleText.color = new Color(0.9f, 0.7f, 0.3f); // 金色
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;

            // 副标题
            GameObject subtitleObj = new GameObject("Subtitle");
            subtitleObj.transform.SetParent(parent);
            RectTransform subRect = subtitleObj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.3f, 0.52f);
            subRect.anchorMax = new Vector2(0.7f, 0.58f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;
            TextMeshProUGUI subText = subtitleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) subText.font = font;
            subText.text = "一个类似环世界的沙盒游戏";
            subText.color = new Color(0.7f, 0.7f, 0.7f);
            subText.fontSize = 24;
            subText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// 创建菜单按钮
        /// </summary>
        private void CreateMenuButtons(RectTransform parent)
        {
            float buttonWidth = 300;
            float buttonHeight = 60;
            float spacing = 15;
            float startY = 0.38f;

            // 新建世界按钮
            CreateMenuButton(parent, "NewWorldButton", "新建世界 [N]",
                new Vector2(0.5f, startY), buttonWidth, buttonHeight,
                new Color(0.2f, 0.6f, 0.3f), OnNewWorldClicked);

            // 加载世界按钮
            CreateMenuButton(parent, "LoadWorldButton", "加载世界 [L]",
                new Vector2(0.5f, startY - (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.3f, 0.4f, 0.6f), OnLoadWorldClicked);

            // 选项按钮
            CreateMenuButton(parent, "OptionsButton", "选项 [O]",
                new Vector2(0.5f, startY - 2 * (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.5f, 0.5f, 0.5f), OnOptionsClicked);

            // 关于按钮
            CreateMenuButton(parent, "AboutButton", "关于 [A]",
                new Vector2(0.5f, startY - 3 * (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.5f, 0.5f, 0.5f), OnAboutClicked);

            // 退出游戏按钮
            CreateMenuButton(parent, "QuitButton", "退出游戏 [Q]",
                new Vector2(0.5f, startY - 4 * (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.6f, 0.2f, 0.2f), OnQuitClicked);
        }

        /// <summary>
        /// 创建菜单按钮
        /// </summary>
        private void CreateMenuButton(RectTransform parent, string name, string text, Vector2 anchorY,
            float width, float height, Color normalColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(anchorY.x - width / 1920f, anchorY.y - height / 1080f / 2);
            btnRect.anchorMax = new Vector2(anchorY.x + width / 1920f, anchorY.y + height / 1080f / 2);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = normalColor;

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
            if (font != null) tmpText.font = font;
            tmpText.text = text;
            tmpText.color = Color.white;
            tmpText.fontSize = 24;
            tmpText.alignment = TextAlignmentOptions.Center;

            // 添加Hover效果
            AddButtonHoverEffect(btn, btnImage, normalColor);
        }

        /// <summary>
        /// 添加按钮悬停效果
        /// </summary>
        private void AddButtonHoverEffect(Button btn, Image btnImage, Color normalColor)
        {
            ColorBlock colors = btn.colors;
            colors.highlightedColor = normalColor * 1.3f;
            colors.pressedColor = normalColor * 0.8f;
            colors.selectedColor = normalColor * 1.1f;
            btn.colors = colors;
        }

        #endregion

        #region 星球选择面板

        /// <summary>
        /// 初始化星球预设
        /// </summary>
        private void InitializePlanetPresets()
        {
            planetPresets.Add(new PlanetPreset("EarthLike", "类地星球", "一个适宜居住的绿色世界"));
            planetPresets.Add(new PlanetPreset("Desert", "沙漠星球", "炎热的沙尘世界"));
            planetPresets.Add(new PlanetPreset("Ice", "冰冻星球", "寒冷的冰雪世界"));
            planetPresets.Add(new PlanetPreset("Volcanic", "火山星球", "充满岩浆的炽热世界"));
        }

        /// <summary>
        /// 显示星球选择面板
        /// </summary>
        private void ShowPlanetSelectPanel()
        {
            if (planetSelectPanel != null)
            {
                planetSelectPanel.SetActive(true);
                return;
            }

            planetSelectPanel = new GameObject("PlanetSelectPanel");
            planetSelectPanel.transform.SetParent(canvas.transform);
            RectTransform panelRect = planetSelectPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 半透明背景
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(planetSelectPanel.transform);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // 标题
            CreatePanelTitle(planetSelectPanel.transform, "选择星球类型");

            // 星球卡片网格
            float cardWidth = 350;
            float cardHeight = 250;
            float spacingX = 30;
            float spacingY = 30;
            int columns = 4;
            float startX = 0.5f - (columns * (cardWidth + spacingX) - spacingX) / 1920f / 2;
            float startY = 0.45f;

            for (int i = 0; i < planetPresets.Count; i++)
            {
                float x = startX + (i % columns) * (cardWidth + spacingX) / 1920f;
                float y = startY - (i / columns) * (cardHeight + spacingY) / 1080f;
                CreatePlanetCard(planetPresets[i], planetSelectPanel.transform, x, y, cardWidth, cardHeight);
            }

            // 返回按钮
            CreateBackButton(planetSelectPanel.transform, OnBackToMainMenu);
        }

        /// <summary>
        /// 创建面板标题
        /// </summary>
        private void CreatePanelTitle(Transform parent, string title)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.78f);
            titleRect.anchorMax = new Vector2(0.7f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleText.font = font;
            titleText.text = title;
            titleText.color = Color.white;
            titleText.fontSize = 42;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// 创建星球卡片
        /// </summary>
        private void CreatePlanetCard(PlanetPreset preset, Transform parent, float x, float y, float width, float height)
        {
            GameObject card = new GameObject($"PlanetCard_{preset.id}");
            card.transform.SetParent(parent);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(x - width / 1920f, y - height / 1080f / 2);
            cardRect.anchorMax = new Vector2(x + width / 1920f, y + height / 1080f / 2);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            Image cardImage = card.AddComponent<Image>();
            cardImage.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            Button cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = cardImage;
            ColorBlock colors = cardBtn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.4f);
            cardBtn.colors = colors;

            // 星球名称
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(card.transform);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.05f, 0.7f);
            nameRect.anchorMax = new Vector2(0.95f, 0.9f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) nameText.font = font;
            nameText.text = preset.displayName;
            nameText.color = new Color(0.9f, 0.7f, 0.3f);
            nameText.fontSize = 28;
            nameText.alignment = TextAlignmentOptions.Center;

            // 星球描述
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(card.transform);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.05f, 0.2f);
            descRect.anchorMax = new Vector2(0.95f, 0.65f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            if (font != null) descText.font = font;
            descText.text = preset.description;
            descText.color = Color.gray;
            descText.fontSize = 18;
            descText.alignment = TextAlignmentOptions.Center;

            // 点击事件
            cardBtn.onClick.AddListener(() => OnPlanetSelected(preset));
        }

        /// <summary>
        /// 创建返回按钮
        /// </summary>
        private void CreateBackButton(Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject("BackButton");
            btnObj.transform.SetParent(parent);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.02f, 0.02f);
            btnRect.anchorMax = new Vector2(0.12f, 0.08f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.5f, 0.3f, 0.3f);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImage;
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmpText.font = font;
            tmpText.text = "返回";
            tmpText.color = Color.white;
            tmpText.fontSize = 20;
            tmpText.alignment = TextAlignmentOptions.Center;
        }

        #endregion

        #region 加载面板

        /// <summary>
        /// 显示加载面板
        /// </summary>
        private void ShowLoadingPanel(string message)
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
                return;
            }

            loadingPanel = new GameObject("LoadingPanel");
            loadingPanel.transform.SetParent(canvas.transform);
            RectTransform panelRect = loadingPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 背景
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(loadingPanel.transform);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            // 加载文本
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(loadingPanel.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.3f, 0.55f);
            textRect.anchorMax = new Vector2(0.7f, 0.65f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            progressText = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) progressText.font = font;
            progressText.text = message;
            progressText.color = Color.white;
            progressText.fontSize = 32;
            progressText.alignment = TextAlignmentOptions.Center;

            // 进度条背景
            GameObject sliderBg = new GameObject("SliderBackground");
            sliderBg.transform.SetParent(loadingPanel.transform);
            RectTransform sliderBgRect = sliderBg.AddComponent<RectTransform>();
            sliderBgRect.anchorMin = new Vector2(0.25f, 0.4f);
            sliderBgRect.anchorMax = new Vector2(0.75f, 0.48f);
            sliderBgRect.offsetMin = Vector2.zero;
            sliderBgRect.offsetMax = Vector2.zero;
            Image sliderBgImage = sliderBg.AddComponent<Image>();
            sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f);

            // 进度条
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(loadingPanel.transform);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.25f, 0.4f);
            sliderRect.anchorMax = new Vector2(0.75f, 0.48f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            progressSlider = sliderObj.AddComponent<Slider>();
            progressSlider.minValue = 0;
            progressSlider.maxValue = 100;
            progressSlider.value = 0;
        }

        /// <summary>
        /// 隐藏加载面板
        /// </summary>
        private void HideLoadingPanel()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 更新加载进度
        /// </summary>
        public void UpdateLoadingProgress(float progress, string message)
        {
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }
            if (progressText != null && message != null)
            {
                progressText.text = message;
            }
        }

        #endregion

        #region 按钮事件

        /// <summary>
        /// 新建世界按钮点击
        /// </summary>
        private void OnNewWorldClicked()
        {
            Debug.Log("[开局菜单] 点击了【新建世界】");
            ShowPlanetSelectPanel();
        }

        /// <summary>
        /// 加载世界按钮点击
        /// </summary>
        private void OnLoadWorldClicked()
        {
            Debug.Log("[开局菜单] 点击了【加载世界】");
            // TODO: 实现加载世界功能
            ShowLoadingPanel("正在加载世界...");
        }

        /// <summary>
        /// 选项按钮点击
        /// </summary>
        private void OnOptionsClicked()
        {
            Debug.Log("[开局菜单] 点击了【选项】");
            // TODO: 显示选项菜单
        }

        /// <summary>
        /// 关于按钮点击
        /// </summary>
        private void OnAboutClicked()
        {
            Debug.Log("[开局菜单] 点击了【关于】");
            // TODO: 显示关于面板
        }

        /// <summary>
        /// 退出游戏按钮点击
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[开局菜单] 点击了【退出游戏】");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 星球选中
        /// </summary>
        private void OnPlanetSelected(PlanetPreset preset)
        {
            Debug.Log($"[开局菜单] 选择了星球: {preset.displayName}");
            selectedPlanet = preset;
            StartCoroutine(CreateNewWorld(preset));
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private void OnBackToMainMenu()
        {
            if (planetSelectPanel != null)
            {
                planetSelectPanel.SetActive(false);
            }
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }
        }

        #endregion

        #region 世界创建流程

        /// <summary>
        /// 创建新世界协程
        /// </summary>
        private System.Collections.IEnumerator CreateNewWorld(PlanetPreset preset)
        {
            isLoading = true;
            ShowLoadingPanel("正在创建世界...");

            // 阶段1: 初始化 (10%)
            UpdateLoadingProgress(10f, "正在初始化...");
            yield return new WaitForSeconds(0.3f);

            // 阶段2: 生成星球 (30%)
            UpdateLoadingProgress(30f, "正在生成星球...");
            yield return new WaitForSeconds(0.3f);

            // 隐藏加载面板,显示星球选择界面
            HideLoadingPanel();
            ShowPlanetSelectionWithActualPlanet(preset);

            // 等待用户选择六边形区域
            UpdateLoadingProgress(50f, "请点击星球上的六边形区域...");
            HexTile selectedTile = null;
            bool tileSelected = false;

            // 创建星球控制器并监听选择事件
            HexPlanetController planetController = CreatePlanetController();
            planetController.onTileSelected += (tile) => {
                selectedTile = tile;
                tileSelected = true;
            };

            // 等待用户选择
            while (!tileSelected)
            {
                yield return null;
            }

            // 阶段3: 初始化框架 (70%)
            UpdateLoadingProgress(70f, "正在初始化游戏框架...");
            Main_MMWorld.Init();
            yield return new WaitForSeconds(0.5f);

            // 阶段4: 创建256x256地图 (85%)
            UpdateLoadingProgress(85f, "正在创建256x256地面...");
            int tileId = selectedTile.id;
            if (MapIndex.Instance != null)
            {
                yield return MapIndex.Instance.StartCoroutine(MapIndex.Instance.CreateMap(tileId, 256, 256));
            }
            yield return new WaitForSeconds(0.5f);

            // 阶段5: 完成 (100%)
            UpdateLoadingProgress(100f, "世界创建完成!");
            yield return new WaitForSeconds(0.5f);

            // 隐藏菜单和星球
            OnBackToMainMenu();
            HideStartMenu();
            Destroy(planetController.gameObject);

            // 切换到地图
            if (MapIndex.Instance != null)
            {
                MapIndex.Instance.SwitchToMap(tileId);
            }

            // 通知游戏开始
            OnWorldCreated(preset, tileId);
        }

        /// <summary>
        /// 创建星球控制器
        /// </summary>
        private HexPlanetController CreatePlanetController()
        {
            GameObject planetObj = new GameObject("HexPlanetController");
            HexPlanetController controller = planetObj.AddComponent<HexPlanetController>();
            return controller;
        }

        /// <summary>
        /// 显示星球选择界面(实际星球)
        /// </summary>
        private void ShowPlanetSelectionWithActualPlanet(PlanetPreset preset)
        {
            if (planetSelectPanel != null)
            {
                planetSelectPanel.SetActive(true);
                mainMenuPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 世界创建完成后的处理
        /// </summary>
        private void OnWorldCreated(PlanetPreset preset, int selectedTileId)
        {
            Debug.Log($"[开局菜单] 世界创建完成! 星球类型: {preset.displayName}, 选中Tile: {selectedTileId}");
            // 通知游戏管理器开始游戏
            GameManager.Instance?.StartGame(preset);
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 隐藏开局菜单
        /// </summary>
        public void HideStartMenu()
        {
            if (canvas != null)
            {
                canvas.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 显示开局菜单
        /// </summary>
        public void ShowStartMenu()
        {
            // 如果UI还没创建,先初始化
            if (canvas == null)
            {
                InitializeFont();
                CreateStartMenuUI();
            }

            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading => isLoading;

        #endregion
    }

    /// <summary>
    /// 星球预设数据
    /// </summary>
    public class PlanetPreset
    {
        public string id;
        public string displayName;
        public string description;

        public PlanetPreset(string id, string displayName, string description)
        {
            this.id = id;
            this.displayName = displayName;
            this.description = description;
        }
    }
}
