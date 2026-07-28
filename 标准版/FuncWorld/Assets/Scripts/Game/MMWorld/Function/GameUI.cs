using MetalMaxSystem.Unity;
using UnityHexPlanet;
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
    public class GameUI : UGUITemplate
    {
        #region 字段

        /// <summary>
        /// 星球选择对话框
        /// </summary>
        private static GameObject ui_GameObject_PlanetSelect;

        /// <summary>
        /// 加载进度对话框
        /// </summary>
        private static GameObject ui_GameObject_ProgressLoading;

        /// <summary>
        /// 进度条控件
        /// </summary>
        private static Slider ui_Slider_ProgressLoading;

        /// <summary>
        /// 进度文本标签控件
        /// </summary>
        private static TextMeshProUGUI ui_TextMeshProUGUI_ProgressLoading;

        /// <summary>
        /// 星球预设列表
        /// </summary>
        private static List<PlanetPreset> planetPresets = new List<PlanetPreset>();

        /// <summary>
        /// 当前选中的星球预设
        /// </summary>
        private static PlanetPreset selectedPlanet;

        /// <summary>
        /// 是否正在加载
        /// </summary>
        private static bool isLoading = false;

        /// <summary>
        /// 游戏UI状态.true表示已初始化必要的上层UI.
        /// </summary>
        public static bool initialized = false;

        #endregion

        #region UI创建

        private static GameUI _instance;
        public static GameUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UGUITemplate.Dialog_GameUI.AddComponent<GameUI>();
                }
                return _instance;
            }
        }
        /// <summary>
        /// 创建游戏UI.在场景加载完成后调用,初始化必要的上层UI
        /// 初始化星球预设列表,并根据选中的星球预设创建游戏世界.
        /// 调用时会自动触发单例初始化,无需手动先访问Instance.
        /// </summary>
        /// <param name="showMainMenu"> 是否显示主菜单.默认显示. </param>
        public static GameUI Create(bool showMainMenu = true)
        {
            if (!initialized)
            {
                initialized = true; //预防高频创建Bug,提前将结果标记为已初始化

                #region 初始化全部上层必要UI

                //创建并激活主菜单
                UI_GameObject_MainMenu()?.SetActive(showMainMenu);
                //初始化星球预设
                InitializePlanetPresets();

                #endregion

            }

            return Instance;
        }

        /// <summary>
        /// 创建主菜单界面
        /// </summary>
        private static void UICreate_MainMenu()
        {
            GameObject obj = new GameObject("MainMenu");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_MainMenu", obj);
            obj.transform.SetParent(Dialog_GameUI.transform);
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
        public static GameObject UI_GameObject_MainMenu()
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
        private static void UICreate_MainTitle(RectTransform parent)
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
            titleText.font = UGUITemplate.FontFZYaSong;
            titleText.text = "FuncWorld";
            titleText.color = new Color(0.9f, 0.7f, 0.3f); //金色
            titleText.fontSize = 72;
            titleText.alignment = TextAlignmentOptions.Center;
        }
        /// <summary>
        /// 主文本标签对话框
        /// </summary>
        /// <returns></returns>
        public static GameObject UI_GameObject_MainTitle()
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
        private static void UICreate_SubTitle(RectTransform parent)
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
            subText.font = UGUITemplate.FontFZYaSong;
            subText.text = "一个类似环世界的沙盒游戏";
            subText.color = new Color(0.7f, 0.7f, 0.7f);
            subText.fontSize = 24;
            subText.alignment = TextAlignmentOptions.Center;
        }
        /// <summary>
        /// 副文本标签对话框
        /// </summary>
        /// <returns></returns>
        public static GameObject UI_GameObject_SubTitle()
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
        private static void UICreate_MenuButtons(RectTransform parent)
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
        private static void UICreate_MenuButton(RectTransform parent, string name, string text, Vector2 anchorY, float width, float height, Color normalColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(name);
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_" + UGUITemplate.GameUIName, btnObj);
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
            tmpText.font = UGUITemplate.FontFZYaSong;
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
        public static GameObject UI_GameObject_NewWorldButton()
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
        public static GameObject UI_GameObject_LoadWorldButton()
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
        public static GameObject UI_GameObject_OptionsButton()
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
        public static GameObject UI_GameObject_AboutButton()
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
        public static GameObject UI_GameObject_QuitButton()
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
        private static void AddButtonHoverEffect(Button btn, Image btnImage, Color normalColor)
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
        private static void InitializePlanetPresets()
        {
            planetPresets.Add(new PlanetPreset("EarthLike", "类地星球", "一个适宜居住的绿色世界"));
            planetPresets.Add(new PlanetPreset("Desert", "沙漠星球", "炎热的沙尘世界"));
            planetPresets.Add(new PlanetPreset("Ice", "冰冻星球", "寒冷的冰雪世界"));
            planetPresets.Add(new PlanetPreset("Volcanic", "火山星球", "充满岩浆的炽热世界"));
        }

        /// <summary>
        /// 创建星球选择界面
        /// </summary>
        private static void UICreate_PlanetSelect()
        {
            ui_GameObject_PlanetSelect = new GameObject("PlanetSelect");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_PlanetSelect", ui_GameObject_PlanetSelect);
            //加false,防止子对象继承父对象的缩放和旋转
            ui_GameObject_PlanetSelect.transform.SetParent(Dialog_GameUI.transform, false);
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
        public static GameObject UI_GameObject_PlanetSelect()
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
        public static GameObject UI_GameObject_PlanetSelect_Background()
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
        private static void UICreate_Title(Transform parent, string title)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.3f, 0.78f);
            titleRect.anchorMax = new Vector2(0.7f, 0.9f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.font = UGUITemplate.FontFZYaSong;
            titleText.text = title;
            titleText.color = Color.white;
            titleText.fontSize = 42;
            titleText.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// 创建星球卡片界面
        /// </summary>
        private static void UICreate_PlanetCard(PlanetPreset preset, Transform parent, float x, float y, float width, float height)
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
            nameText.font = UGUITemplate.FontFZYaSong;
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
            descText.font = UGUITemplate.FontFZYaSong;
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
        private static void UICreate_BackButton(Transform parent, UnityEngine.Events.UnityAction onClick)
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
            tmpText.font = UGUITemplate.FontFZYaSong;
            tmpText.text = "返回";
            tmpText.color = Color.white;
            tmpText.fontSize = 24;
            tmpText.alignment = TextAlignmentOptions.Center;
        }
        /// <summary>
        /// 返回按钮控件
        /// </summary>
        /// <returns></returns>
        public static Button UI_Button_MainMenu_PlanetSelect_Back()
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
        private static void UICreate_ProgressLoading(string message = "正在加载...")
        {
            ui_GameObject_ProgressLoading = new GameObject("ProgressLoading");
            MetalMaxSystem.DataTable<GameObject>.Save0(true, "UI_GameObject_ProgressLoading", ui_GameObject_ProgressLoading);
            ui_GameObject_ProgressLoading.transform.SetParent(Dialog_GameUI.transform);
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
            ui_TextMeshProUGUI_ProgressLoading.font = UGUITemplate.FontFZYaSong;
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
        public static GameObject UI_GameObject_ProgressLoading()
        {
            var result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_ProgressLoading");
            if (result == null)
            {
                UICreate_ProgressLoading();
                result = MetalMaxSystem.DataTable<GameObject>.Load0(true, "UI_GameObject_ProgressLoading");
            }
            return result;
        }

        public static TextMeshProUGUI UI_TextMeshProUGUI_ProgressLoading()
        {
            var result = MetalMaxSystem.DataTable<TextMeshProUGUI>.Load0(true, "UI_TextMeshProUGUI_ProgressLoading");
            if (result == null)
            {
                UICreate_ProgressLoading();
                result = MetalMaxSystem.DataTable<TextMeshProUGUI>.Load0(true, "UI_TextMeshProUGUI_ProgressLoading");
            }
            return result;
        }

        public static Slider UI_Slider_ProgressLoading()
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
        public static void UpdateLoadingProgress(float progress, string message)
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
        private static void OnNewWorldClicked()
        {
            Debug.Log("[开局菜单] 点击了【新建世界】");
            UI_GameObject_PlanetSelect()?.SetActive(true);
        }

        /// <summary>
        /// 加载世界按钮点击
        /// </summary>
        private static void OnLoadWorldClicked()
        {
            Debug.Log("[开局菜单] 点击了【加载世界】");
            // TODO: 实现加载世界功能
        }

        /// <summary>
        /// 选项按钮点击
        /// </summary>
        private static void OnOptionsClicked()
        {
            Debug.Log("[开局菜单] 点击了【选项】");
            // TODO: 显示选项菜单
        }

        /// <summary>
        /// 关于按钮点击
        /// </summary>
        private static void OnAboutClicked()
        {
            Debug.Log("[开局菜单] 点击了【关于】");
            // TODO: 显示关于面板
        }

        /// <summary>
        /// 退出游戏按钮点击
        /// </summary>
        private static void OnQuitClicked()
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
        private static void OnPlanetSelected(PlanetPreset preset)
        {
            Debug.Log($"[开局菜单] 选择了星球: {preset.displayName}");
            selectedPlanet = preset;
            Instance.StartCoroutine(CreateNewWorld(preset));
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private static void OnBackToMainMenu()
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
        private static System.Collections.IEnumerator CreateNewWorld(PlanetPreset preset)
        {
            isLoading = true;
            UpdateLoadingProgress(0f, "正在创建世界...");
            yield return new WaitForSeconds(0.3f);
            UpdateLoadingProgress(10f, "正在初始化...");
            yield return new WaitForSeconds(0.3f);
            UpdateLoadingProgress(30f, "正在生成星球...");
            yield return new WaitForSeconds(0.3f);

            //隐藏菜单
            UI_GameObject_PlanetSelect()?.SetActive(false);
            UI_GameObject_MainMenu()?.SetActive(false);
            UGUITemplate.Control_GameUIBackground.enabled = false;
            UI_GameObject_ProgressLoading()?.SetActive(false);

            // 禁用原始主相机（正交2.5D模式，不适合3D星球视角）
            UnityUtilities.MainCamera.SetActive(false);

            TileRaycast tileRaycast;
            HexPlanetManager planetManager = CreatePlanetRoot(out tileRaycast);

            // 等待玩家点击星球Tile（独立射线检测，不依赖TileRaycast的事件）
            HexTile selectedTile = null;
            while (selectedTile == null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    selectedTile = RaycastToTile(UnityUtilities.PlanetCamera.GetComponent<Camera>());
                    if (selectedTile != null)
                    {
                        Debug.Log($"[GameUI] 选中Tile: ID={selectedTile.id}");
                    }
                }
                yield return null;
            }

            // 禁用TileRaycast组件（停止Demo的多选/光标逻辑）
            tileRaycast.enabled = false;

            //关闭星球相机
            UnityUtilities.PlanetCamera.SetActive(false);

            // 恢复原始主相机
            UnityUtilities.MainCamera.SetActive(true);

            UI_GameObject_ProgressLoading()?.SetActive(true);
            UpdateLoadingProgress(70f, "初始化游戏框架...");
            yield return new WaitForSeconds(0.3f);
            GameMain.Run();
            UpdateLoadingProgress(85f, "创建256x256地面...");
            yield return new WaitForSeconds(0.3f);

            int tileId = selectedTile.id;
            if (MapIndex.Instance != null)
            {
                yield return MapIndex.Instance.StartCoroutine(MapIndex.Instance.CreateMap(tileId, 256, 256));
            }
            yield return new WaitForSeconds(0.3f);

            UpdateLoadingProgress(100f, "创建完成!");
            yield return new WaitForSeconds(0.3f);
            UI_GameObject_ProgressLoading()?.SetActive(false);

            if (planetManager != null)
            {
                Destroy(planetManager.gameObject);
            }

            // 切换到地图
            if (MapIndex.Instance != null)
            {
                MapIndex.Instance.SwitchToMap(tileId);
            }

            // 通知游戏开始
            OnWorldCreated(preset, tileId);
        }

        /// <summary>
        /// 创建星球 - 使用HexSphere组件直接构建
        /// </summary>
        private static HexPlanetManager CreatePlanetRoot(out TileRaycast tileRaycast)
        {
            GameObject planetRoot = new GameObject("PlanetRoot");
            HexPlanetManager manager = planetRoot.AddComponent<HexPlanetManager>();

            HexPlanet hexPlanet = ScriptableObject.CreateInstance<HexPlanet>();
            //HexPlanet hexPlanet = UnityUtilities.SpecialAssets.scriptableObjects[2] as HexPlanet; //使用ScriptableObject星球数据
            hexPlanet.radius = 100f;
            hexPlanet.subdivisions = 5;
            hexPlanet.chunkSubdivisions = 2;

            hexPlanet.chunkMaterial = UnityUtilities.SpecialAssets.materials[1];
            hexPlanet.chunkMaterial.color = new Color(0.4f, 0.6f, 0.3f);

            // PerlinTerrainGenerator terrainGen = ScriptableObject.CreateInstance<PerlinTerrainGenerator>();
            // terrainGen.octaves = 4;
            // terrainGen.persistence = 0.72f;
            // terrainGen.lacunarity = 2.91f;
            // terrainGen.minHeight = 0f;
            // terrainGen.maxHeight = 0f;
            // terrainGen.noiseScaling = 4f;
            // terrainGen.colorHeights = new List<PerlinTerrainGenerator.ColorHeight>
            // {
            //     new PerlinTerrainGenerator.ColorHeight { color = new Color32(0, 85, 42, 255), maxHeight = 17.5f },
            //     new PerlinTerrainGenerator.ColorHeight { color = new Color32(121, 128, 176, 255), maxHeight = 18.5f },
            //     new PerlinTerrainGenerator.ColorHeight { color = new Color32(3, 200, 64, 255), maxHeight = 21.5f },
            //     new PerlinTerrainGenerator.ColorHeight { color = new Color32(1, 80, 128, 255), maxHeight = 23.5f },
            //     new PerlinTerrainGenerator.ColorHeight { color = new Color32(255, 255, 255, 255), maxHeight = 100f },
            // };

            RandomTerrainGenerator terrainGen = ScriptableObject.CreateInstance<RandomTerrainGenerator>();
            terrainGen.minHeight = 0f;
            terrainGen.maxHeight = 0f;
            terrainGen.colors = new List<Color32>
            {
                new Color32(0, 85, 42, 255),
                new Color32(121, 128, 176, 255),
                new Color32(3, 200, 64, 255),
                new Color32(1, 80, 128, 255),
                new Color32(255, 255, 255, 255),
            };


            hexPlanet.terrainGenerator = terrainGen;
            manager.hexPlanet = hexPlanet;
            manager.UpdateRenderObjects();

            // 星球轨道相机控制（挂载在星球专用相机上）
            CameraOrbit orbit = PlanetCameraOrbit;
            orbit.origin = planetRoot;
            orbit.orbitRadius = 190f;
            orbit.orbitSpeed = 60f;
            orbit.smoothness = 0.1f;

            // 鼠标悬浮Tile高亮 - 使用LineRenderer
            LineRenderer lr = planetRoot.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 0;
            lr.startWidth = 0.2f;
            lr.endWidth = 0.2f;
            lr.numCornerVertices = 0;
            lr.numCapVertices = 0;
            lr.startColor = new Color(1f, 1f, 0f, 0.9f);
            lr.endColor = new Color(1f, 1f, 0f, 0.9f);

            // 添加TileRaycast组件用于悬浮高亮（使用PlanetCamera作为Camera.main）
            tileRaycast = planetRoot.AddComponent<TileRaycast>();
            tileRaycast.showEditorGUI = false; // 关闭Demo编辑器GUI，仅保留LineRenderer悬浮高亮

            Debug.Log("[GameUI] 星球创建完成: radius=100, subdivisions=5, terrain=Random");
            return manager;
        }

        private static CameraOrbit _planetCameraOrbit;
        public static CameraOrbit PlanetCameraOrbit
        {
            get
            {
                if (_planetCameraOrbit == null)
                {
                    _planetCameraOrbit = UnityUtilities.PlanetCamera.GetComponent<CameraOrbit>();
                    if (_planetCameraOrbit == null) _planetCameraOrbit = UnityUtilities.PlanetCamera.AddComponent<CameraOrbit>();
                }
                return _planetCameraOrbit;
            }
        }

        /// <summary>
        /// 射线检测点击的Tile（参考TileRaycast.cs的检测逻辑）
        /// </summary>
        private static HexTile RaycastToTile(Camera cam)
        {
            int raycastMask = LayerMask.GetMask("HexPlanet");
            if (cam == null) return null;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, raycastMask)) return null;

            HexChunkRenderer hcr = hit.transform.gameObject.GetComponent<HexChunkRenderer>();
            if (hcr == null) return null;

            HexChunk hc = hcr.GetHexChunk();
            if (hc == null) return null;

            Vector3 localHit = hit.point - hcr.transform.position;
            return hc.GetClosestTileAngle(localHit);
        }

        /// <summary>
        /// 世界创建完成后的处理
        /// </summary>
        private static void OnWorldCreated(PlanetPreset preset, int selectedTileId)
        {
            Debug.Log($"[开局菜单] 世界创建完成! 星球类型: {preset.displayName}, 选中Tile: {selectedTileId}");
            // 通知游戏管理器开始游戏
            GameManager.Instance?.StartGame(preset, selectedTileId);
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
