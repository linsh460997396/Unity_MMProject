//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 用于快速创建/获取的常用UGUI对象.
    /// </summary>
    public class UGUITemplate : MonoBehaviour
    {
        //内置字体(编辑器环境放在Resources目录)
        private static TMP_FontAsset _fontFZYaSong;
        /// <summary>
        /// 方正雅宋中文字体.请将对应的字体文件放在Resources/Fonts目录下,并命名为"FZ_YaSong SDF"以供加载.
        /// </summary>
        public static TMP_FontAsset FontFZYaSong
        {
            set
            {
                _fontFZYaSong = value;
            }
            get
            {
                if (_fontFZYaSong == null)
                {
                    _fontFZYaSong = Resources.Load<TMP_FontAsset>("Fonts/FZ_YaSong SDF");
                    if (_fontFZYaSong == null) _fontFZYaSong = TMP_Settings.defaultFontAsset;
                    if (_fontFZYaSong == null)
                    {
                        Debug.LogWarning("未能找到FZ_YaSong SDF字体及内置默认字体");
                    }
                }
                return _fontFZYaSong;
            }
        }

        private static TMP_FontAsset _fontMetalMax;
        /// <summary>
        /// MetalMax中文字体.请将对应的字体文件放在Resources/Fonts目录下,并命名为"MM_VonwaonBitmap SDF"以供加载.
        /// </summary>
        public static TMP_FontAsset FontMetalMax
        {
            set
            {
                _fontMetalMax = value;
            }
            get
            {
                if (_fontMetalMax == null)
                {
                    _fontMetalMax = Resources.Load<TMP_FontAsset>("Fonts/MM_VonwaonBitmap SDF");
                    if (_fontMetalMax == null) _fontMetalMax = TMP_Settings.defaultFontAsset;
                    if (_fontMetalMax == null)
                    {
                        Debug.LogWarning("未能找到MM_VonwaonBitmap SDF字体");
                    }
                }
                return _fontMetalMax;
            }
        }

        private static TMP_FontAsset _defaultFont;
        public static TMP_FontAsset DefaultFont
        {
            set
            {
                _defaultFont = value;
                if (_defaultFont == null) _defaultFont = FontFZYaSong;
            }
            get
            {
                if (_defaultFont == null)
                {
                    _defaultFont = FontFZYaSong;
                }
                return _defaultFont;
            }
        }

        private static string _name;
        public static string Name
        {
            get { if (string.IsNullOrEmpty(_name)) return "GameUI"; return _name; }
            set { if (!string.IsNullOrEmpty(value)) _name = value; }
        }

        private static string _eventSystemName;
        public static string EventSystemName
        {
            get { if (string.IsNullOrEmpty(_eventSystemName)) return "EventSystem"; return _eventSystemName; }
            set { if (!string.IsNullOrEmpty(value)) _eventSystemName = value; }
        }

        /// <summary>
        /// 输入模式检测结果的枚举
        /// </summary>
        public enum InputSupportType
        {
            None,           // 都不支持（极罕见）
            Legacy_Standalone,         // 仅支持旧版 (StandaloneInputModule)
            New_InputSystemUI,      // 仅支持新版 (InputSystemUIInputModule)
            Both            // 两者类都存在（通常对应 Project Settings 中的 "Both" 模式）
        }

        /// <summary>
        /// 检查当前环境中可用的 InputModule 类型
        /// </summary>
        public static InputSupportType CheckAvailableInputModule()
        {
            bool hasLegacy = false;
            bool hasNew = false;

            // 1. 检查旧版模块 (通常位于 UnityEngine.UI 程序集)
            // StandaloneInputModule 是 UGUI 核心部分，大多数 Unity 版本都包含
            Type legacyType = Type.GetType("UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.UI");
            if (legacyType != null)
            {
                hasLegacy = true;
            }

            // 2. 检查新版模块 (位于 Unity.InputSystem 程序集)
            // 如果游戏没装 Input System Package，这个类型会是 null
            Type newType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (newType != null)
            {
                hasNew = true;
            }

            if (hasLegacy && hasNew) return InputSupportType.Both;
            if (hasLegacy) return InputSupportType.Legacy_Standalone;
            if (hasNew) return InputSupportType.New_InputSystemUI;

            return InputSupportType.None;
        }
        /// <summary>
        /// 内部辅助：根据反射检测结果添加模块.
        /// </summary>
        /// <param name="go"></param>
        /// <param name="inputSupportType">Input模块类型,默认优先添加旧版模块,因为不需要额外配置 Actions Asset,兼容性最稳.可选指定类型</param>
        public static void AddCompatibleModule(GameObject go, InputSupportType? inputSupportType = null)
        {
            InputSupportType support;
            if (inputSupportType == null)
            {
                support = CheckAvailableInputModule();
            }
            else
            {
                support = inputSupportType.Value;
            }

            switch (support)
            {
                case InputSupportType.New_InputSystemUI:
                    // 注意：新版模块若无 Actions Asset 绑定，可能无法响应点击
                    // 但在没有引用的情况下，这是唯一能添加的途径
                    Type newType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                    if (newType != null) go.AddComponent(newType);
                    break;

                case InputSupportType.Legacy_Standalone:
                case InputSupportType.Both:
                    // 优先使用旧版，因为不需要额外配置 Actions Asset，兼容性最稳
                    Type legacyType = Type.GetType("UnityEngine.EventSystems.StandaloneInputModule, UnityEngine.UI");
                    if (legacyType != null) go.AddComponent(legacyType);
                    break;

                case InputSupportType.None:
                    Debug.LogWarning("未拥有任何InputModule类型,UI将无法交互!");
                    break;
            }
        }

        /// <summary>
        /// 文本自适应模式
        /// </summary>
        public enum AutoSizeMode
        {
            None,           // 不自动调整，使用 RectTransform 当前设置的尺寸或锚点拉伸
            WidthOnly,      // 宽度固定(由maxWidth决定或当前宽)，高度自适应内容
            HeightOnly,     // 高度固定(由maxHeight决定或当前高)，宽度自适应内容
            Both            // 宽高均自适应内容 (忽略 maxWidth/maxHeight 限制，除非设为0)
        }

        /// <summary>
        /// 应用自适应尺寸逻辑
        /// </summary>
        private static void ApplyAutoSize(TextMeshProUGUI tmpText, RectTransform rectTransform, AutoSizeMode mode, float maxWidth, float maxHeight)
        {
            if (mode == AutoSizeMode.None)
                return;

            // 如果锚点是拉伸状态（Min != Max），通常意味着我们希望它跟随父物体缩放，
            // 此时手动设置 sizeDelta 可能会与锚点冲突。
            // 这里我们假设：如果开启了 AutoSize，我们倾向于使用精确的 sizeDelta，
            // 或者将锚点重置为中心点/左上角以便 sizeDelta 生效。
            // 为了简化，如果检测到锚点拉伸，我们暂时不强制覆盖 sizeDelta，除非模式是 Both 且用户明确想要包裹内容。

            // 获取首选尺寸
            // GetPreferredValues(string text, float width, float height)
            // width: 如果 > 0，表示在此宽度内换行；如果 <= 0，表示不限制宽度（单行）
            // height: 通常传 0，让高度无限延伸以容纳所有行

            float calcWidth = (mode == AutoSizeMode.WidthOnly || mode == AutoSizeMode.Both) ? maxWidth : 0f;

            // 注意：如果 maxWidth <= 0 且模式需要宽度限制，TMP 将不会换行，可能导致文本溢出
            Vector2 preferredSize = tmpText.GetPreferredValues(tmpText.text, calcWidth, 0f);

            // 根据模式应用尺寸
            switch (mode)
            {
                case AutoSizeMode.WidthOnly:
                    // 宽度保持当前 RectTransform 的宽度（或 maxWidth），高度自适应
                    // 如果 maxWidth > 0，则使用 maxWidth，否则使用当前 rect 的宽度
                    float finalWidth = maxWidth > 0 ? maxWidth : rectTransform.rect.width;
                    rectTransform.sizeDelta = new Vector2(finalWidth, preferredSize.y);
                    break;

                case AutoSizeMode.HeightOnly:
                    // 宽度自适应，高度保持当前或 maxHeight
                    float finalHeight = maxHeight > 0 ? maxHeight : rectTransform.rect.height;
                    rectTransform.sizeDelta = new Vector2(preferredSize.x, finalHeight);
                    break;

                case AutoSizeMode.Both:
                    // 宽高完全自适应
                    // 如果 maxWidth > 0，preferredSize.x 不会超过 maxWidth（因为上面传参限制了换行）
                    rectTransform.sizeDelta = new Vector2(preferredSize.x, preferredSize.y);

                    // 可选：如果希望锚点也配合自适应，可以将锚点重置为 Center 或 TopLeft
                    // 这里保持用户传入的锚点不变，但 sizeDelta 会生效
                    break;
            }
        }

        /// <summary>
        /// 获取或创建EventSystem用于处理UI交互事件.
        /// </summary>
        /// <param name="inputSupportType">Input模块类型,默认优先添加旧版模块,因为不需要额外配置 Actions Asset,兼容性最稳.可选指定类型</param>
        /// <returns></returns>
        public static GameObject GetEventSystem(InputSupportType? inputSupportType = null)
        {
            EventSystem existing = GameObject.FindObjectOfType<EventSystem>();
            if (existing != null)
            {
                //检查模块完整性
                if (existing.currentInputModule == null)
                {
                    //无输入模块时,尝试添加一个兼容的输入模块
                    AddCompatibleModule(existing.gameObject, inputSupportType);
                }
                else
                {
                    //安全获取当前激活的输入模块
                    BaseInputModule module = existing.currentInputModule;
                    if (module != null)
                    {
                        Debug.Log($"当前输入模块: {module.GetType().Name}");
                    }
                }
                //记录到runtimePrefab字典中
                if (!UnityUtilities.runtimePrefab.ContainsKey(Name)) UnityUtilities.runtimePrefab.Add(Name, existing.gameObject);
                return existing.gameObject;
            }

            //如果没有就全新创建
            return UnityUtilities.GetOrCreatePrefab(EventSystemName, go =>
            {
                //添加EventSystem组件
                go.AddComponent<EventSystem>();
                //添加InputModule组件(处理鼠标/键盘输入)
                AddCompatibleModule(go, inputSupportType);
                GameObject.DontDestroyOnLoad(go);
                Debug.Log($"预制体已创建: " + EventSystemName);
            });
        }

        /// <summary>
        /// 对话框_游戏UI界面(最上层UI)
        /// </summary>
        public static GameObject Dialog_GameUI
        {
            get
            {
                return UnityUtilities.GetOrCreatePrefab(Name, go =>
                {
                    Canvas canvas_GameUI = go.GetComponent<Canvas>();
                    if (canvas_GameUI == null)
                    {
                        //画布
                        canvas_GameUI = go.AddComponent<Canvas>();
                        canvas_GameUI.renderMode = RenderMode.ScreenSpaceOverlay; //设置为屏幕空间覆盖模式,该模式下UI会覆盖在所有游戏对象之上
                        canvas_GameUI.sortingOrder = 9999; //确保UI在最上层显示

                        //画布缩放器
                        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; //根据屏幕分辨率缩放UI
                        scaler.referenceResolution = new Vector2(1920, 1080); //参考分辨率进行缩放,这里设置为1920x1080适配大部分屏幕,缩放时保持宽高比,比如在1920x1080的屏幕上显示正常,在1280x720的屏幕上会缩小但保持比例
                        go.AddComponent<GraphicRaycaster>();

                        //背景区域面板
                        RectTransform bgRect = go.GetComponent<RectTransform>(); //加Canvas组件时Unity会自动添加RectTransform组件
                                                                                 //设置背景区域面板覆盖整个画布
                        bgRect.anchorMin = Vector2.zero;
                        bgRect.anchorMax = Vector2.one;
                        bgRect.offsetMin = Vector2.zero;
                        bgRect.offsetMax = Vector2.zero;
                        //背景图
                        Image bgImage = go.AddComponent<Image>();
                        bgImage.color = new Color(0.05f, 0.05f, 0.08f, 1f); //深空黑色背景
                    }
                    GameObject.DontDestroyOnLoad(go);
                    Debug.Log($"预制体已创建: {Name}");
                });
            }
        }

        /// <summary>
        /// 创建并插入一个可自定义锚点和偏移的面板到对话框或控件中.
        /// 注:偏移量决定了大小和位置.
        /// </summary>
        /// <param name="parent">父节点,即对话框(GameObject)或控件(组件)的Transform</param>
        /// <param name="name">面板GameObject的名称,默认"Panel"</param>
        /// <param name="anchorMin">最小锚点 (默认左下角 0,0)</param>
        /// <param name="anchorMax">最大锚点 (默认右上角 1,1)</param>
        /// <param name="offsetMin">左下偏移量 (默认 0,0)</param>
        /// <param name="offsetMax">右上偏移量 (默认 0,0)</param>
        /// <param name="pivot">轴心点 (默认中心 0.5,0.5)</param>
        /// <returns>面板控件(RectTransform)</returns>
        public static RectTransform Control_PanelCreate(Transform parent, string name = "Panel", Vector2? anchorMin = null, Vector2? anchorMax = null, Vector2? offsetMin = null, Vector2? offsetMax = null, Vector2? pivot = null)
        {
            // 1. 创建对象
            GameObject panelObj = new GameObject(name);
            // 2. 设置父节点(false保持本地变换,防止缩放错乱,true则继承父物体的变换)
            panelObj.transform.SetParent(parent, false);
            // 3. 获取组件
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            // 4. 应用参数 (使用 ?? 运算符提供默认值)
            // 默认行为：拉伸填充父物体 (Stretch-Stretch)
            rectTransform.anchorMin = anchorMin ?? Vector2.zero;
            rectTransform.anchorMax = anchorMax ?? Vector2.one;
            // 默认行为：无额外偏移
            rectTransform.offsetMin = offsetMin ?? Vector2.zero;
            rectTransform.offsetMax = offsetMax ?? Vector2.zero;
            // 默认行为：轴心在中心
            rectTransform.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            return rectTransform;
        }

        /// <summary>
        /// 创建并插入文本标签(支持自适应尺寸)到对话框或控件中.
        /// </summary>
        /// <param name="parent">父节点,即对话框(GameObject)或控件(组件)的Transform</param>
        /// <param name="title">内容字符串</param>
        /// <param name="name">GameObject名称，默认"Title"</param>
        /// <param name="anchorMin">最小锚点</param>
        /// <param name="anchorMax">最大锚点</param>
        /// <param name="font">字体资产</param>
        /// <param name="color">文本颜色</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="alignment">文本对齐方式</param>
        /// <param name="autoSize">自适应模式，默认为 None（保持原有锚点行为）</param>
        /// <param name="maxWidth">最大宽度限制。若 autoSize 为 WidthOnly 或 Both，且此值 > 0，则在此宽度内换行</param>
        /// <param name="maxHeight">最大高度限制。暂未用于截断，主要用于预留接口</param>
        /// <returns>文本标签控件(TextMeshProUGUI)</returns>
        public static TextMeshProUGUI Control_TitleCreate(Transform parent, string title, string name = "Title", Vector2? anchorMin = null, Vector2? anchorMax = null, TMP_FontAsset font = null, Color? color = null, float fontSize = 42f, TextAlignmentOptions alignment = TextAlignmentOptions.Center, AutoSizeMode autoSize = AutoSizeMode.None, float maxWidth = 0f, float maxHeight = 0f)
        {
            // 1. 创建 GameObject 并设置父节点和名称
            GameObject titleObj = new GameObject(name);
            titleObj.transform.SetParent(parent);

            // 2. 添加 RectTransform 并设置锚点
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();

            Vector2 min = anchorMin ?? new Vector2(0.3f, 0.78f);
            Vector2 max = anchorMax ?? new Vector2(0.7f, 0.9f);

            titleRect.anchorMin = min;
            titleRect.anchorMax = max;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            // 3. 添加 TextMeshProUGUI 组件并配置属性
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();

            titleText.font = font ?? DefaultFont;
            titleText.text = title;
            titleText.color = color ?? Color.white;
            titleText.fontSize = fontSize;
            titleText.alignment = alignment;

            // 4. 处理自适应逻辑
            ApplyAutoSize(titleText, titleRect, autoSize, maxWidth, maxHeight);

            return titleText;
        }

    }
}
#endif