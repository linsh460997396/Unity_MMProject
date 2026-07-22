using MetalMaxSystem.Unity;
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
    public class GameUI : MonoBehaviour
    {
        #region 字段

        private static string _name = "GameUI";
        public static string Name
        {
            get { if (string.IsNullOrEmpty(_name)) return "GameUI"; return _name; }
            set { if (!string.IsNullOrEmpty(value)) _name = value; }
        }

        private static GameUI _instance;
        public static GameUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = GameObject.Find(Name);
                    if (obj == null) obj = new GameObject(Name);
                    if (obj.GetComponent<GameUI>() == null) _instance = obj.AddComponent<GameUI>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        /// <summary>
        /// 星球选择对话框
        /// </summary>
        private GameObject ui_GameObject_PlanetSelect;

        /// <summary>
        /// 加载进度对话框
        /// </summary>
        private GameObject ui_GameObject_ProgressLoading;

        /// <summary>
        /// 进度条控件
        /// </summary>
        private Slider ui_Slider_ProgressLoading;

        /// <summary>
        /// 进度文本标签控件
        /// </summary>
        private TextMeshProUGUI ui_TextMeshProUGUI_ProgressLoading;

        /// <summary>
        /// 星球预设列表
        /// </summary>
        private List<PlanetPreset> planetPresets = new List<PlanetPreset>();

        /// <summary>
        /// 当前选中的星球预设
        /// </summary>
        private PlanetPreset selectedPlanet;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        private bool isLoading = false;

        /// <summary>
        /// GameUI状态.true表示已初始化
        /// </summary>
        private bool state = false;

        #endregion

        #region UI创建

        public static GameUI Create()
        {
            return Instance;
        }

        /// <summary>
        /// 初始化主要UI(主菜单、星球选择等)
        /// </summary>
        private void Start()
        {
            if (state) return;
            UI_GameObject_MainMenu()?.SetActive(true);
            InitializePlanetPresets();
            state = true;
        }

        /// <summary>
        /// 创建GameUI界面(最上层UI)
        /// </summary>
        /// <param name="eventSystem">是否顺带创建事件系统(只需插件一个即可激活UGUI事件处理)</param>
        private void UICreate_GameUI(bool eventSystem = true)
        {
            GameObject obj_GameUI = GameUI.Instance.gameObject;
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_" + Name, obj_GameUI);

            Canvas canvas_GameUI = obj_GameUI.GetComponent<Canvas>();
            if (canvas_GameUI == null)
            {
                //画布
                canvas_GameUI = obj_GameUI.AddComponent<Canvas>();
                canvas_GameUI.renderMode = RenderMode.ScreenSpaceOverlay; //设置为屏幕空间覆盖模式,该模式下UI会覆盖在所有游戏对象之上
                canvas_GameUI.sortingOrder = 9999; //确保UI在最上层显示

                //画布缩放器
                CanvasScaler scaler = obj_GameUI.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; //根据屏幕分辨率缩放UI
                scaler.referenceResolution = new Vector2(1920, 1080); //参考分辨率进行缩放,这里设置为1920x1080适配大部分屏幕,缩放时保持宽高比,比如在1920x1080的屏幕上显示正常,在1280x720的屏幕上会缩小但保持比例
                obj_GameUI.AddComponent<GraphicRaycaster>();

                //背景区域面板
                RectTransform bgRect = obj_GameUI.GetComponent<RectTransform>(); //加Canvas组件时Unity会自动添加RectTransform组件
                //设置背景区域面板覆盖整个画布
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                //背景图
                Image bgImage = obj_GameUI.AddComponent<Image>();
                bgImage.color = new Color(0.05f, 0.05f, 0.08f, 1f); //深空黑色背景
            }
            MetalMaxSystem.DataTable<Canvas>.Save0(true, "UI_Canvas_" + Name, canvas_GameUI);

            if (eventSystem)
            {
                SpriteSpacePrefab.GetEventSystem();
            }
        }
        /// <summary>
        /// GameUI画布控件
        /// </summary>
        /// <returns></returns>
        public Canvas UI_Canvas_GameUI()
        {
            var result = MetalMaxSystem.DataTable<Canvas>.Load0(true, "UI_Canvas_" + Name);
            if (result == null)
            {
                UICreate_GameUI();
                result = MetalMaxSystem.DataTable<Canvas>.Load0(true, "UI_Canvas_" + Name);
            }
            return result;
        }
        /// <summary>
        /// GameUI对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_GameUI()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_" + Name);
            if (result == null)
            {
                UICreate_GameUI();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_" + Name);
            }
            return result;
        }
        /// <summary>
        /// 创建主菜单界面
        /// </summary>
        private void UICreate_MainMenu()
        {
            GameObject obj = new GameObject("MainMenu");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_MainMenu", obj);
            obj.transform.SetParent(UI_GameObject_GameUI().transform);
            RectTransform menuRect = obj.AddComponent<RectTransform>();
            menuRect.anchorMin = Vector2.zero;
            menuRect.anchorMax = Vector2.one;
            menuRect.offsetMin = Vector2.zero;
            menuRect.offsetMax = Vector2.zero;

            UICreate_MainTitle(menuRect);
            UICreate_SubTitle(menuRect);
            UICreate_MenuButtons(menuRect);
        }
        /// <summary>
        /// 主菜单对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_MainMenu()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_MainMenu");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_MainMenu");
            }
            return result;
        }

        /// <summary>
        /// 创建主文本标签界面
        /// </summary>
        /// <param name="parent"></param>
        private void UICreate_MainTitle(RectTransform parent)
        {
            // 主文本标签
            GameObject obj = new GameObject("MainTitle");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_MainTitle", obj);
            obj.transform.SetParent(parent);
            RectTransform titleRect = obj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.6f);
            titleRect.anchorMax = new Vector2(0.7f, 0.85f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero; //设置文本标签位置和大小,这里设置为屏幕上方居中
            TextMeshProUGUI titleText = obj.AddComponent<TextMeshProUGUI>();
            titleText.font = SpriteSpacePrefab.FontFZYaSong;
            titleText.text = "FuncWorld";
            titleText.color = new Color(0.9f, 0.7f, 0.3f); //金色
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;
        }
        /// <summary>
        /// 主文本标签对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_MainTitle()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_MainTitle");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_MainTitle");
            }
            return result;
        }
        /// <summary>
        /// 创建副文本标签界面
        /// </summary>
        /// <param name="parent"></param>
        private void UICreate_SubTitle(RectTransform parent)
        {
            // 副文本标签
            GameObject obj = new GameObject("SubTitle");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_SubTitle", obj);
            obj.transform.SetParent(parent);
            RectTransform subRect = obj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.3f, 0.52f);
            subRect.anchorMax = new Vector2(0.7f, 0.58f);
            subRect.offsetMin = Vector2.zero;
            subRect.offsetMax = Vector2.zero;
            TextMeshProUGUI subText = obj.AddComponent<TextMeshProUGUI>();
            subText.font = SpriteSpacePrefab.FontFZYaSong;
            subText.text = "一个类似环世界的沙盒游戏";
            subText.color = new Color(0.7f, 0.7f, 0.7f);
            subText.fontSize = 24;
            subText.alignment = TextAlignmentOptions.Center;
        }
        /// <summary>
        /// 副文本标签对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_SubTitle()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_SubTitle");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_SubTitle");
            }
            return result;
        }

        /// <summary>
        /// 创建菜单按钮界面
        /// </summary>
        private void UICreate_MenuButtons(RectTransform parent)
        {
            float buttonWidth = 300;
            float buttonHeight = 60;
            float spacing = 15;
            float startY = 0.38f;

            // 新建世界按钮
            UICreate_MenuButton(parent, "NewWorldButton", "新建世界 [N]",
                new Vector2(0.5f, startY), buttonWidth, buttonHeight,
                new Color(0.2f, 0.6f, 0.3f), OnNewWorldClicked);

            // 加载世界按钮
            UICreate_MenuButton(parent, "LoadWorldButton", "加载世界 [L]",
                new Vector2(0.5f, startY - (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.3f, 0.4f, 0.6f), OnLoadWorldClicked);

            // 选项按钮
            UICreate_MenuButton(parent, "OptionsButton", "选项 [O]",
                new Vector2(0.5f, startY - 2 * (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.5f, 0.5f, 0.5f), OnOptionsClicked);

            // 关于按钮
            UICreate_MenuButton(parent, "AboutButton", "关于 [A]",
                new Vector2(0.5f, startY - 3 * (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.5f, 0.5f, 0.5f), OnAboutClicked);

            // 退出游戏按钮
            UICreate_MenuButton(parent, "QuitButton", "退出游戏 [Q]",
                new Vector2(0.5f, startY - 4 * (buttonHeight + spacing) / 1080f), buttonWidth, buttonHeight,
                new Color(0.6f, 0.2f, 0.2f), OnQuitClicked);
        }

        /// <summary>
        /// 创建菜单按钮界面
        /// </summary>
        /// <param name="parent">父对象</param>
        /// <param name="name">按钮名称</param>
        /// <param name="text">按钮文本</param>
        /// <param name="anchorY">按钮锚点Y坐标</param>
        /// <param name="width">按钮宽度</param>
        /// <param name="height">按钮高度</param>
        /// <param name="normalColor">按钮正常颜色</param>
        /// <param name="onClick">点击事件</param>
        private void UICreate_MenuButton(RectTransform parent, string name, string text, Vector2 anchorY, float width, float height, Color normalColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_" + name, btnObj);
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
            tmpText.font = SpriteSpacePrefab.FontFZYaSong;
            tmpText.text = text;
            tmpText.color = Color.white;
            tmpText.fontSize = 24;
            tmpText.alignment = TextAlignmentOptions.Center;

            // 添加Hover效果
            AddButtonHoverEffect(btn, btnImage, normalColor);
        }
        /// <summary>
        /// 新建世界_按钮对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_NewWorldButton()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_NewWorldButton");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_NewWorldButton");
            }
            return result;
        }
        /// <summary>
        /// 加载世界_按钮对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_LoadWorldButton()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_LoadWorldButton");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_LoadWorldButton");
            }
            return result;
        }
        /// <summary>
        /// 选项_按钮对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_OptionsButton()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_OptionsButton");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_OptionsButton");
            }
            return result;
        }
        /// <summary>
        /// 关于_按钮对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_AboutButton()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_AboutButton");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_AboutButton");
            }
            return result;
        }
        /// <summary>
        /// 退出游戏_按钮对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_QuitButton()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_QuitButton");
            if (result == null)
            {
                UICreate_MainMenu();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_QuitButton");
            }
            return result;
        }

        /// <summary>
        /// 添加按钮悬停效果.用于按钮在鼠标悬停时改变颜色,增强交互感
        /// </summary>
        /// <param name="btn">按钮</param>
        /// <param name="btnImage">按钮图像</param>
        /// <param name="normalColor">按钮正常颜色</param>
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
        /// 创建星球选择界面
        /// </summary>
        private void UICreate_PlanetSelect()
        {
            ui_GameObject_PlanetSelect = new GameObject("PlanetSelect");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_PlanetSelect", ui_GameObject_PlanetSelect);
            ui_GameObject_PlanetSelect.transform.SetParent(UI_Canvas_GameUI().transform);
            RectTransform panelRect = ui_GameObject_PlanetSelect.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero; // 设置面板覆盖整个画布

            // 半透明背景
            GameObject bg = new GameObject("Background");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_PlanetSelect_Background", bg);
            bg.transform.SetParent(ui_GameObject_PlanetSelect.transform);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);

            // 文本标签
            UICreate_Title(ui_GameObject_PlanetSelect.transform, "选择星球类型");

            // 星球卡片网格
            float cardWidth = 350;
            float cardHeight = 250;
            float spacingX = 30;
            float spacingY = 30;
            int columns = 4;
            float startX = 0.1f;
            float startY = 0.45f;

            for (int i = 0; i < planetPresets.Count; i++)
            {
                float x = startX + (i % columns) * (cardWidth + spacingX) / 1920f + cardWidth / 1920f / 2;
                float y = startY - (i / columns) * (cardHeight + spacingY) / 1080f;
                UICreate_PlanetCard(planetPresets[i], ui_GameObject_PlanetSelect.transform, x, y, cardWidth, cardHeight);
            }

            // 创建返回按钮界面
            UICreate_BackButton(ui_GameObject_PlanetSelect.transform, OnBackToMainMenu);
        }
        /// <summary>
        /// 星球选择界面对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_PlanetSelect()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_PlanetSelect");
            if (result == null)
            {
                UICreate_PlanetSelect();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_PlanetSelect");
            }
            return result;
        }
        /// <summary>
        /// 星球选择界面背景对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_PlanetSelect_Background()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_PlanetSelect_Background");
            if (result == null)
            {
                UICreate_PlanetSelect();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_PlanetSelect_Background");
            }
            return result;
        }

        /// <summary>
        /// 创建文本标签界面
        /// </summary>
        private void UICreate_Title(Transform parent, string title)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.78f);
            titleRect.anchorMax = new Vector2(0.7f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.font = SpriteSpacePrefab.FontFZYaSong;
            titleText.text = title;
            titleText.color = Color.white;
            titleText.fontSize = 42;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// 创建星球卡片界面
        /// </summary>
        private void UICreate_PlanetCard(PlanetPreset preset, Transform parent, float x, float y, float width, float height)
        {
            GameObject card = new GameObject($"PlanetCard_{preset.id}");
            card.transform.SetParent(parent);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(x - width / 1920f / 2, y - height / 1080f / 2);
            cardRect.anchorMax = new Vector2(x + width / 1920f / 2, y + height / 1080f / 2);
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
            nameText.font = SpriteSpacePrefab.FontFZYaSong;
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
            descText.font = SpriteSpacePrefab.FontFZYaSong;
            descText.text = preset.description;
            descText.color = Color.gray;
            descText.fontSize = 18;
            descText.alignment = TextAlignmentOptions.Center;

            // 点击事件
            cardBtn.onClick.AddListener(() => OnPlanetSelected(preset));
        }

        /// <summary>
        /// 创建返回按钮界面
        /// </summary>
        private void UICreate_BackButton(Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject("BackButton");
            btnObj.transform.SetParent(parent);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f - 120f / 1920f, 0.05f);
            btnRect.anchorMax = new Vector2(1f, 0.05f + 56f / 1080f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = new Color(0.6f, 0.4f, 0.2f);

            Button btn = btnObj.AddComponent<Button>();
            MetalMaxSystem.DataTable<Button>.Save0(true, "UI_Button_MainMenu_PlanetSelect_Back", btn);
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
            tmpText.font = SpriteSpacePrefab.FontFZYaSong;
            tmpText.text = "返回";
            tmpText.color = Color.white;
            tmpText.fontSize = 24;
            tmpText.alignment = TextAlignmentOptions.Center;
        }
        /// <summary>
        /// 返回按钮控件
        /// </summary>
        /// <returns></returns>
        public Button UI_Button_MainMenu_PlanetSelect_Back()
        {
            var result = MetalMaxSystem.DataTable<Button>.Load0(true, "UI_Button_MainMenu_PlanetSelect_Back");
            if (result == null)
            {
                UICreate_PlanetSelect();
                result = MetalMaxSystem.DataTable<Button>.Load0(true, "UI_Button_MainMenu_PlanetSelect_Back");
            }
            return result;
        }

        #endregion

        #region 加载面板

        /// <summary>
        /// 创建进度加载界面
        /// </summary>
        private void UICreate_ProgressLoading(string message = "正在加载...")
        {
            ui_GameObject_ProgressLoading = new GameObject("ProgressLoading");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_ProgressLoading", ui_GameObject_ProgressLoading);
            ui_GameObject_ProgressLoading.transform.SetParent(UI_GameObject_GameUI().transform);
            RectTransform panelRect = ui_GameObject_ProgressLoading.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 背景
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(ui_GameObject_ProgressLoading.transform);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            // 加载文本
            GameObject textObj = new GameObject("LoadingText");
            textObj.transform.SetParent(ui_GameObject_ProgressLoading.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.3f, 0.55f);
            textRect.anchorMax = new Vector2(0.7f, 0.65f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            ui_TextMeshProUGUI_ProgressLoading = textObj.AddComponent<TextMeshProUGUI>();
            MetalMaxSystem.DataTable<TextMeshProUGUI>.Save0(true, "UI_TextMeshProUGUI_ProgressLoading", ui_TextMeshProUGUI_ProgressLoading);
            ui_TextMeshProUGUI_ProgressLoading.font = SpriteSpacePrefab.FontFZYaSong;
            ui_TextMeshProUGUI_ProgressLoading.text = message;
            ui_TextMeshProUGUI_ProgressLoading.color = Color.white;
            ui_TextMeshProUGUI_ProgressLoading.fontSize = 32;
            ui_TextMeshProUGUI_ProgressLoading.alignment = TextAlignmentOptions.Center;

            // 进度条背景
            GameObject sliderBg = new GameObject("SliderBackground");
            sliderBg.transform.SetParent(ui_GameObject_ProgressLoading.transform);
            RectTransform sliderBgRect = sliderBg.AddComponent<RectTransform>();
            sliderBgRect.anchorMin = new Vector2(0.25f, 0.4f);
            sliderBgRect.anchorMax = new Vector2(0.75f, 0.48f);
            sliderBgRect.offsetMin = Vector2.zero;
            sliderBgRect.offsetMax = Vector2.zero;
            Image sliderBgImage = sliderBg.AddComponent<Image>();
            sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f);

            // 进度条
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(ui_GameObject_ProgressLoading.transform);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.25f, 0.4f);
            sliderRect.anchorMax = new Vector2(0.75f, 0.48f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            ui_Slider_ProgressLoading = sliderObj.AddComponent<Slider>();
            MetalMaxSystem.DataTable<Slider>.Save0(true, "UI_Slider_ProgressLoading", ui_Slider_ProgressLoading);
            ui_Slider_ProgressLoading.minValue = 0;
            ui_Slider_ProgressLoading.maxValue = 100;
            ui_Slider_ProgressLoading.value = 0;
        }
        /// <summary>
        /// 进度加载界面对话框
        /// </summary>
        /// <returns></returns>
        public GameObject UI_GameObject_ProgressLoading()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_ProgressLoading");
            if (result == null)
            {
                UICreate_ProgressLoading();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_ProgressLoading");
            }
            return result;
        }

        public TextMeshProUGUI UI_TextMeshProUGUI_ProgressLoading()
        {
            var result = MetalMaxSystem.DataTable<TextMeshProUGUI>.Load0(true, "UI_TextMeshProUGUI_ProgressLoading");
            if (result == null)
            {
                UICreate_ProgressLoading();
                result = MetalMaxSystem.DataTable<TextMeshProUGUI>.Load0(true, "UI_TextMeshProUGUI_ProgressLoading");
            }
            return result;
        }

        public Slider UI_Slider_ProgressLoading()
        {
            var result = MetalMaxSystem.DataTable<Slider>.Load0(true, "UI_Slider_ProgressLoading");
            if (result == null)
            {
                UICreate_ProgressLoading();
                result = MetalMaxSystem.DataTable<Slider>.Load0(true, "UI_Slider_ProgressLoading");
            }
            return result;
        }

        /// <summary>
        /// 更新加载进度条和文本
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="message"></param>
        public void UpdateLoadingProgress(float progress, string message)
        {
            UI_Slider_ProgressLoading().value = progress;
            if (message != null)
            {
                UI_TextMeshProUGUI_ProgressLoading().text = message;
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
            UI_GameObject_PlanetSelect()?.SetActive(true);
        }

        /// <summary>
        /// 加载世界按钮点击
        /// </summary>
        private void OnLoadWorldClicked()
        {
            Debug.Log("[开局菜单] 点击了【加载世界】");
            // TODO: 实现加载世界功能
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
            UI_GameObject_PlanetSelect()?.SetActive(false);
            UI_GameObject_ProgressLoading()?.SetActive(false);
            UI_GameObject_MainMenu()?.SetActive(true);
        }

        #endregion

        #region 世界创建流程

        /// <summary>
        /// 创建新世界协程
        /// </summary>
        private System.Collections.IEnumerator CreateNewWorld(PlanetPreset preset)
        {
            isLoading = true;
            UpdateLoadingProgress(0f, "正在创建世界...");

            // 阶段1: 初始化 (10%)
            UpdateLoadingProgress(10f, "正在初始化...");
            yield return new WaitForSeconds(0.3f);

            // 阶段2: 生成星球 (30%)
            UpdateLoadingProgress(30f, "正在生成星球...");
            yield return new WaitForSeconds(0.3f);

            // 隐藏菜单,显示星球
            UI_GameObject_ProgressLoading()?.SetActive(false);
            UI_GameObject_PlanetSelect()?.SetActive(false);
            UI_GameObject_MainMenu()?.SetActive(false);

            // 等待用户选择六边形区域
            UpdateLoadingProgress(50f, "请点击星球上的六边形区域...");
            HexTile selectedTile = null;
            bool tileSelected = false;

            // 创建星球控制器并监听选择事件
            HexPlanetController planetController = CreatePlanetController();
            planetController.onTileSelected += (tile) =>
            {
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
            GameMain.Run();
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

            // 隐藏加载条
            UI_GameObject_ProgressLoading()?.SetActive(false);
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
        /// 世界创建完成后的处理
        /// </summary>
        private void OnWorldCreated(PlanetPreset preset, int selectedTileId)
        {
            Debug.Log($"[开局菜单] 世界创建完成! 星球类型: {preset.displayName}, 选中Tile: {selectedTileId}");
            // 通知游戏管理器开始游戏
            GameManager.Instance?.StartGame(preset);
        }

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
