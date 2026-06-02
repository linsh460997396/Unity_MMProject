using MetalMaxSystem.Unity;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace SpriteSpace
{
    /// <summary>
    /// 资源组件.挂载到GameObject上,在编辑器中拖拽素材到对应字段,运行时会自动加载素材供全局使用.
    /// </summary>
    public class SpriteSpacePrefab : MonoBehaviour
    {
        /// <summary>
        /// 预制体字典.
        /// 当RuntimePrefab.Add(string key, Object obj,bool clone = false)中的clone参数为true时,资源为副本存储,
        /// 当clone参数为false时,直接存储对象,摧毁原对象会影响该字典内容,场景切换时未被DontDestroyOnLoad保护的实例会被Unity自动销毁‌,请做好保护.
        /// </summary>
        public static RuntimePrefab runtimePrefab = ScriptableObject.CreateInstance<RuntimePrefab>();
        /// <summary>
        /// 用来阻止挂组件时自动Awake一次.而Start、Update那些就不用阻止了,因为runtimePrefab不在场景,即便预制体上组件Enable也不起作用.
        /// </summary>
        public static Dictionary<string, bool> awakeEnable = new Dictionary<string, bool>();
        /// <summary>
        /// 预制体实例化后的父级GameObject.
        /// </summary>
        public static GameObject group;
        /// <summary>
        /// 预制体初始化是否完成.
        /// </summary>
        public static bool initialized;
        /// <summary>
        /// 外部素材目录.
        /// </summary>
        public static string externalAssetsPath;
        public static Material material;
        public static RenderTexture minimap;

        //内置精灵素材
        public static List<Sprite>[] Vehicle;
        public static List<Sprite>[] characters;
        public static List<Sprite>[] monsters;

        /// <summary>
        /// 预制体初始化方法.在使用SpriteSpacePrefab前必须调用此方法来确保预制体已被创建.
        /// </summary>
        public static void Init()
        {
            if (initialized) return;
            material = new Material(Shader.Find("Sprites/Default"));
            group = GameObject.Find("SpriteSpacePrefab") ?? new GameObject("SpriteSpacePrefab"); //创建存放SpriteSpace预制体实例的父级容器
            DontDestroyOnLoad(group);
            runtimePrefab.hideFlags = HideFlags.DontUnloadUnusedAsset; //资源持久化标记

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            LoadAssets();
            stopwatch.Stop();
            Debug.Log("LoadAllAssets" + $"耗时:{stopwatch.ElapsedMilliseconds}ms");

            //初始化底层绘制对象池(用于大量NPC、怪物等活动精灵个体对象复用GameObject,防止频繁创建摧毁导致掉帧问题)
            GameObject tempGroup = new GameObject("GOGroup");
            DontDestroyOnLoad(tempGroup);
            GO.Init(material, 20000, tempGroup);

            //主动检查并创建场景必需的对象(Sun、MainCamera、MinimapCamera、MinimapCanvas)
            //通过访问属性触发延迟创建逻辑
            //注意: MapEditorCanvas 不在此初始化,需要第一次按下M键时才初始化
            var sun = Sun;
            var mainCamera = MainCamera;
            var minimapCamera = MinimapCamera;
            var minimapCanvas = MinimapCanvas;

            //创建EventSystem用于UI交互
            GetEventSystem();

            initialized = true;
        }

        /// <summary>
        /// 获取Sun预制体.作为单例直接使用.
        /// 如不存在,会创建名为"Sun"的游戏物体并添加Light组件.
        /// 首次创建的预制体不激活.
        /// </summary>
        public static GameObject Sun
        {
            get
            {
                return GetOrCreatePrefab("Sun", go =>
                {
                    // 太阳位置在场景上方（Y=50），朝向地面倾斜照射
                    go.transform.SetPositionAndRotation(new Vector3(0f, 50f, 0f), Quaternion.Euler(60f, 28.5f, 90f));
                    go.transform.parent = group.transform;
                    Light light = go.AddComponent<Light>();
                    light.type = LightType.Directional; // 平行光（模拟太阳光）
                    light.intensity = 1.0f; // 光源强度（1.0为满强度）
                    light.range = 100f; // 光源范围（平行光此参数实际不影响光照）
                    light.color = Color.white; // 白色光源
                    // light.shadowStrength = 0.5f; // 阴影强度（0-1），当前关闭阴影
                    // light.shadowBias = 0.001f; // 阴影偏移，防止阴影贴图瑕疵
                    // light.shadowNormalBias = 0.001f; // 阴影法线偏移
                    light.cullingMask = ~0; // 照亮所有层（~0即-1，二进制取反）
                    go.SetActive(true); // 光源需要立即激活才能照亮场景
                    Debug.Log($"预制体已创建: Sun");
                });
            }
        }

        /// <summary>
        /// 获取MinimapCamera预制体.作为单例直接使用.
        /// 如不存在,会创建名为"MinimapCamera"的游戏物体并添加Camera组件和RenderTexture.
        /// 首次创建的预制体不激活.
        /// </summary>
        public static GameObject MinimapCamera
        {
            get
            {
                string name = "MinimapCamera";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = GameObject.Find(name);
                    if (tempGameObject == null)
                    {
                        tempGameObject = new GameObject(name);
                        tempGameObject.SetActive(false);
                        // 将小地图相机作为MainCamera的子对象，这样会自动跟随主摄像机移动
                        tempGameObject.transform.SetParent(MainCamera.transform);
                        // 设置相对于主摄像机的位置（在主摄像机位置）
                        tempGameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
                        tempGameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                        Camera camera = tempGameObject.AddComponent<Camera>();
                        camera.orthographic = true; // 正交投影（2D小地图常用）
                        camera.orthographicSize = 20f; // 正交视野大小
                        camera.clearFlags = CameraClearFlags.SolidColor; // 纯色清除
                        camera.backgroundColor = new Color(0, 0, 0, 0.24f); // 半透明黑色背景（#0000003C）
                        camera.cullingMask = ~0; // 渲染所有层
                        camera.depth = -1f; // 渲染顺序，小于MainCamera(0)，先渲染
                        camera.targetTexture = GetMiniMap(); // 输出到RenderTexture
                        tempGameObject.SetActive(true); // 小地图摄像机需要激活才能渲染画面
                        Debug.Log($"预制体已创建: {name}");
                    }
                    else
                    {
                        // 如果从场景中找到,确保它有正确的设置
                        Camera camera = tempGameObject.GetComponent<Camera>();
                        if (camera != null)
                        {
                            // 确保摄像机有targetTexture
                            if (camera.targetTexture == null)
                            {
                                camera.targetTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
                                camera.targetTexture.name = "MinimapRenderTexture";
                                Debug.Log("已为场景中存在的MinimapCamera创建targetTexture");
                            }
                            // 确保摄像机已激活
                            if (!tempGameObject.activeSelf)
                            {
                                tempGameObject.SetActive(true);
                            }
                        }
                    }
                    runtimePrefab.Add(name, tempGameObject);
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 获取MinimapCanvas预制体.作为单例直接使用.
        /// 如不存在,会创建名为"MinimapCanvas"的游戏物体并添加Canvas组件和RawImage子对象.
        /// 首次创建的预制体不激活.
        /// </summary>
        public static GameObject MinimapCanvas
        {
            get
            {
                GameObject rawImageGO;
                string name = "MinimapCanvas";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = GameObject.Find(name);
                    if (tempGameObject == null)
                    {
                        tempGameObject = new GameObject(name);
                        tempGameObject.SetActive(false);
                        tempGameObject.transform.SetParent(MainCamera.transform);
                        Canvas canvas = tempGameObject.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 渲染在屏幕空间，叠加于场景之上
                        canvas.sortingOrder = 0; // 0~100,决定UI渲染顺序,数值越大越靠上(遮挡其他UI)
                        CanvasScaler scaler = tempGameObject.AddComponent<CanvasScaler>();
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 跟随屏幕分辨率缩放
                        scaler.referenceResolution = new Vector2(1920f, 1080f); // 参考分辨率
                        tempGameObject.AddComponent<GraphicRaycaster>(); // 允许射线检测UI

                        // 创建RawImage子对象显示小地图画面
                        rawImageGO = new GameObject("MinimapRawImage");
                        rawImageGO.transform.SetParent(tempGameObject.transform);
                        rawImageGO.transform.localPosition = new Vector3(0f, 0f, 0f);
                        rawImageGO.transform.localScale = new Vector3(1f, 1f, 1f);
                        rawImageGO.layer = 5; // UI层

                        RawImage rawImage = rawImageGO.AddComponent<RawImage>();
                        // 小地图放在右上角,轴心点为右上角 (1, 1)，方便计算偏移
                        rawImage.rectTransform.pivot = new Vector2(1f, 1f);
                        rawImage.rectTransform.anchorMin = new Vector2(1f, 1f);
                        rawImage.rectTransform.anchorMax = new Vector2(1f, 1f);
                        // 固定尺寸：宽 480, 高 270
                        rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 480f);
                        rawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 270f);

                        // 设置RawImage显示MinimapCamera的渲染纹理
                        Camera minimapCamera = MinimapCamera.GetComponent<Camera>();
                        // depth: 主摄像机(MainCamera)=0，小地图相机=-1先渲染，UI覆盖在最上层=1
                        minimapCamera.depth = 1f;
                        minimapCamera.useOcclusionCulling = false; // 关闭遮挡剔除，省性能
                        if (minimapCamera != null)
                        {
                            Debug.Log($"MinimapCamera 存在: {minimapCamera.name}");
                            Debug.Log($"targetTexture: {minimapCamera.targetTexture}");
                            rawImage.texture = minimapCamera.targetTexture ?? GetMiniMap();
                        }
                        else
                        {
                            Debug.LogError("MinimapCamera 组件未找到!");
                            rawImage.texture = GetMiniMap();
                        }

                        tempGameObject.SetActive(true);
                        Debug.Log($"预制体已创建: {name}");
                    }
                    else
                    {
                        // 场景中找到MinimapCanvas,确保RawImage正确绑定到RenderTexture
                        rawImageGO = tempGameObject.transform.Find("MinimapRawImage")?.gameObject;
                        if (rawImageGO != null)
                        {
                            RawImage rawImage = rawImageGO.GetComponent<RawImage>();
                            if (rawImage != null)
                            {
                                Camera minimapCamera = MinimapCamera.GetComponent<Camera>();
                                rawImage.texture = (minimapCamera != null && minimapCamera.targetTexture != null)
                                    ? minimapCamera.targetTexture
                                    : GetMiniMap();
                            }
                        }
                        if (!tempGameObject.activeSelf)
                        {
                            tempGameObject.SetActive(true);
                        }
                    }
                    tempGameObject.layer = 5;
                    rawImageGO.layer = 5;
                    runtimePrefab.Add(name, tempGameObject);
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 获取MainCamera预制体.作为单例直接使用.
        /// 如不存在,会创建名为"MainCamera"的游戏物体并添加Camera组件.
        /// 首次创建的预制体不激活.
        /// </summary>
        public static GameObject MainCamera
        {
            get
            {
                return GetOrCreatePrefab("MainCamera", go =>
                {
                    // 主摄像机位置在正后方（Z=-20）
                    go.transform.SetPositionAndRotation(new Vector3(0f, 0f, -20f), Quaternion.identity);
                    go.transform.parent = group.transform;
                    Camera camera = go.AddComponent<Camera>();
                    camera.tag = "MainCamera"; // 设置标签以便Camera.main能识别
                    camera.clearFlags = CameraClearFlags.SolidColor; // 纯色背景，确保光源生效
                    camera.backgroundColor = new Color(0.2f, 0.4f, 0.6f, 1f); // 蓝色背景（天空色）
                    // cullingMask使用位运算指定渲染层：(1 << 层序号) 表示包含该层
                    // Default(0) | TransparentFX(1) | IgnoreRaycast(2) | Water(4) | UI(5)
                    camera.cullingMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5);
                    camera.depth = 0f; // 相机渲染顺序，数值越大越后渲染（-1为小地图相机）
                    camera.nearClipPlane = 0.3f; // 近裁剪面
                    camera.farClipPlane = 1000f; // 远裁剪面
                    camera.allowMSAA = true; // 开启多重采样抗锯齿
                    go.SetActive(true); // 主摄像机需要立即激活
                    Debug.Log($"预制体已创建: MainCamera");
                });
            }
        }

        /// <summary>
        /// 获取MapEditor画布.作为单例直接使用.
        /// 如不存在,会创建名为"MapEditorCanvas"的游戏物体并添加Canvas组件及所有UI元素.
        /// </summary>
        public static GameObject MapEditorCanvas
        {
            get
            {
                return GetOrCreatePrefab("MapEditorCanvas", go =>
                {
                    go.transform.parent = group.transform;
                    Canvas canvas = go.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 1000; // 高排序层级确保显示在最上层
                    CanvasScaler scaler = go.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    go.AddComponent<GraphicRaycaster>();
                    // 创建MapEditor所需的UI元素
                    CreateMapEditorUI(go);
                    go.SetActive(false); // 默认隐藏,按M键显示
                    Debug.Log($"预制体已创建: MapEditorCanvas");
                });
            }
        }

        /// <summary>
        /// 创建MapEditor的UI元素
        /// </summary>
        private static void CreateMapEditorUI(GameObject parent)
        {
            //加载中文字体
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/FZYaSongS-M-GB-Regular-SDF");
            if (font == null)
            {
                //尝试获取TextMeshPro的默认字体设置
                font = TMP_Settings.defaultFontAsset;
                if (font == null)
                {
                    Debug.LogWarning("未能找到FZYaSongS-M-GB -Regular-SDF字体和TextMeshPro默认字体，将使用内置默认字体");
                }
            }

            //创建背景面板
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(parent.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.02f, 0.02f);
            panelRect.anchorMax = new Vector2(0.35f, 0.98f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            //创建顶部提示标签
            GameObject labelHeadTip = new GameObject("label_headTip");
            labelHeadTip.transform.SetParent(panel.transform);
            RectTransform labelRect = labelHeadTip.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.05f, 0.92f);
            labelRect.anchorMax = new Vector2(0.95f, 0.98f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI labelText = labelHeadTip.AddComponent<TextMeshProUGUI>();
            if (labelText != null && font != null)
            {
                labelText.font = font;
            }
            if (labelText != null)
            {
                labelText.text = "地图编辑器 [M键切换]";
                labelText.color = Color.white;
                labelText.fontSize = 18;
                labelText.alignment = TextAlignmentOptions.Center;
            }

            //创建功能选择下拉框
            GameObject comboBox = new GameObject("comboBox_selectFunc");
            comboBox.transform.SetParent(panel.transform);
            RectTransform comboRect = comboBox.AddComponent<RectTransform>();
            comboRect.anchorMin = new Vector2(0.05f, 0.82f);
            comboRect.anchorMax = new Vector2(0.95f, 0.88f);
            comboRect.offsetMin = Vector2.zero;
            comboRect.offsetMax = Vector2.zero;
            Image comboBg = comboBox.AddComponent<Image>();
            comboBg.color = new Color(0.3f, 0.3f, 0.3f);
            TMP_Dropdown dropdown = comboBox.AddComponent<TMP_Dropdown>();
            dropdown.targetGraphic = comboBg;
            dropdown.options.Add(new TMP_Dropdown.OptionData("地图编辑"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("纹理编辑"));
            dropdown.value = 0;

            // 设置下拉框的字体（在添加选项后，itemText才会被初始化）
            if (dropdown.itemText != null && font != null)
            {
                dropdown.itemText.font = font;
            }
            if (dropdown.captionText != null && font != null)
            {
                dropdown.captionText.font = font;
            }

            //创建工作ID输入框
            GameObject textBoxID = new GameObject("textBox_workID");
            textBoxID.transform.SetParent(panel.transform);
            RectTransform textRect = textBoxID.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0.72f);
            textRect.anchorMax = new Vector2(0.45f, 0.78f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Image inputBg = textBoxID.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f);
            TMP_InputField inputField = textBoxID.AddComponent<TMP_InputField>();

            //创建输入框的文本组件（必须先创建这个才能设置fontAsset）
            GameObject textComponentGO = new GameObject("Text Component");
            textComponentGO.transform.SetParent(textBoxID.transform);
            RectTransform textCompRect = textComponentGO.AddComponent<RectTransform>();
            textCompRect.anchorMin = Vector2.zero;
            textCompRect.anchorMax = Vector2.one;
            textCompRect.offsetMin = new Vector2(10, 5);
            textCompRect.offsetMax = new Vector2(-10, -5);
            TextMeshProUGUI textComponent = textComponentGO.AddComponent<TextMeshProUGUI>();
            if (textComponent != null && font != null)
            {
                textComponent.font = font;
            }
            if (textComponent != null)
            {
                textComponent.color = Color.white;
                textComponent.alignment = TextAlignmentOptions.Left;
                inputField.textComponent = textComponent;
            }

            //创建placeholder文本组件
            GameObject placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(textBoxID.transform);
            RectTransform placeholderRect = placeholderGO.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 5);
            placeholderRect.offsetMax = new Vector2(-10, -5);
            TextMeshProUGUI placeholderText = placeholderGO.AddComponent<TextMeshProUGUI>();
            if (placeholderText != null && font != null)
            {
                placeholderText.font = font;
            }
            if (placeholderText != null)
            {
                placeholderText.text = "输入编号";
                placeholderText.color = new Color(0.5f, 0.5f, 0.5f); //灰色提示文字
                placeholderText.alignment = TextAlignmentOptions.Left;
                inputField.placeholder = placeholderText;
            }

            inputField.text = "0";

            //创建运行按钮
            GameObject buttonRun = new GameObject("button_run");
            buttonRun.transform.SetParent(panel.transform);
            RectTransform btnRect = buttonRun.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.55f, 0.72f);
            btnRect.anchorMax = new Vector2(0.75f, 0.78f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            Image btnImage = buttonRun.AddComponent<Image>();
            btnImage.color = new Color(0.3f, 0.6f, 0.3f);
            Button btn = buttonRun.AddComponent<Button>();
            btn.targetGraphic = btnImage;

            // 创建按钮文本子对象
            GameObject btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(buttonRun.transform);
            RectTransform btnTextRect = btnTextGO.AddComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            TextMeshProUGUI btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
            if (btnText != null && font != null)
            {
                btnText.font = font;
            }
            if (btnText != null)
            {
                btnText.text = "加载";
                btnText.alignment = TextAlignmentOptions.Center;
                btnText.color = Color.white;
            }

            //创建外部路径输入框
            GameObject textBoxPath = new GameObject("textBox_outsideResPath");
            textBoxPath.transform.SetParent(panel.transform);
            RectTransform pathRect = textBoxPath.AddComponent<RectTransform>();
            pathRect.anchorMin = new Vector2(0.05f, 0.62f);
            pathRect.anchorMax = new Vector2(0.95f, 0.68f);
            pathRect.offsetMin = Vector2.zero;
            pathRect.offsetMax = Vector2.zero;
            Image pathBg = textBoxPath.AddComponent<Image>();
            pathBg.color = new Color(0.2f, 0.2f, 0.2f);
            TMP_InputField pathInput = textBoxPath.AddComponent<TMP_InputField>();

            //创建输入框的文本组件（必须先创建这个才能设置fontAsset）
            GameObject pathTextGO = new GameObject("Text Component");
            pathTextGO.transform.SetParent(textBoxPath.transform);
            RectTransform pathTextRect = pathTextGO.AddComponent<RectTransform>();
            pathTextRect.anchorMin = Vector2.zero;
            pathTextRect.anchorMax = Vector2.one;
            pathTextRect.offsetMin = new Vector2(10, 5);
            pathTextRect.offsetMax = new Vector2(-10, -5);
            TextMeshProUGUI pathText = pathTextGO.AddComponent<TextMeshProUGUI>();
            if (pathText != null && font != null)
            {
                pathText.font = font;
            }
            if (pathText != null)
            {
                pathText.color = Color.white;
                pathText.alignment = TextAlignmentOptions.Left;
                pathInput.textComponent = pathText;
            }

            pathInput.text = UnityEngine.Application.dataPath + "/Resources/ColliderFiles/MapCollider.txt";
            pathInput.readOnly = true;

            //创建地图编辑器模式勾选框
            GameObject checkBoxMap = new GameObject("checkBox_mapEditorMode");
            checkBoxMap.transform.SetParent(panel.transform);
            RectTransform mapRect = checkBoxMap.AddComponent<RectTransform>();
            mapRect.anchorMin = new Vector2(0.05f, 0.52f);
            mapRect.anchorMax = new Vector2(0.45f, 0.58f);
            mapRect.offsetMin = Vector2.zero;
            mapRect.offsetMax = Vector2.zero;
            Toggle mapToggle = checkBoxMap.AddComponent<Toggle>();

            // 创建Toggle背景
            GameObject mapBg = new GameObject("Background");
            mapBg.transform.SetParent(checkBoxMap.transform);
            RectTransform mapBgRect = mapBg.AddComponent<RectTransform>();
            mapBgRect.anchorMin = new Vector2(0, 0.5f);
            mapBgRect.anchorMax = new Vector2(0, 0.5f);
            mapBgRect.sizeDelta = new Vector2(20, 20);
            mapBgRect.pivot = new Vector2(0.5f, 0.5f);
            mapBgRect.anchoredPosition = new Vector2(10, 0);
            Image mapBgImg = mapBg.AddComponent<Image>();
            mapBgImg.color = new Color(0.2f, 0.2f, 0.2f);
            mapToggle.targetGraphic = mapBgImg;

            // 创建Toggle勾选框
            GameObject mapCheck = new GameObject("Checkmark");
            mapCheck.transform.SetParent(mapBg.transform);
            RectTransform mapCheckRect = mapCheck.AddComponent<RectTransform>();
            mapCheckRect.anchorMin = Vector2.zero;
            mapCheckRect.anchorMax = Vector2.one;
            mapCheckRect.offsetMin = new Vector2(3, 3);
            mapCheckRect.offsetMax = new Vector2(-3, -3);
            Image mapCheckImg = mapCheck.AddComponent<Image>();
            mapCheckImg.color = Color.white;
            mapToggle.graphic = mapCheckImg;

            // 创建标签
            GameObject mapLabelGO = new GameObject("Label");
            mapLabelGO.transform.SetParent(checkBoxMap.transform);
            RectTransform mapLabelRect = mapLabelGO.AddComponent<RectTransform>();
            mapLabelRect.anchorMin = Vector2.zero;
            mapLabelRect.anchorMax = Vector2.one;
            mapLabelRect.offsetMin = new Vector2(40, 0);
            mapLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI mapLabel = mapLabelGO.AddComponent<TextMeshProUGUI>();
            if (mapLabel != null && font != null)
            {
                mapLabel.font = font;
            }
            if (mapLabel != null)
            {
                mapLabel.text = "纹理模式";
                mapLabel.alignment = TextAlignmentOptions.Left;
                mapLabel.color = Color.white;
            }

            //创建碰撞显示勾选框
            GameObject checkBoxCollider = new GameObject("checkBox_showCollider");
            checkBoxCollider.transform.SetParent(panel.transform);
            RectTransform colliderRect = checkBoxCollider.AddComponent<RectTransform>();
            colliderRect.anchorMin = new Vector2(0.55f, 0.52f);
            colliderRect.anchorMax = new Vector2(0.95f, 0.58f);
            colliderRect.offsetMin = Vector2.zero;
            colliderRect.offsetMax = Vector2.zero;
            Toggle colliderToggle = checkBoxCollider.AddComponent<Toggle>();

            // 创建Toggle背景
            GameObject colliderBg = new GameObject("Background");
            colliderBg.transform.SetParent(checkBoxCollider.transform);
            RectTransform colliderBgRect = colliderBg.AddComponent<RectTransform>();
            colliderBgRect.anchorMin = new Vector2(0, 0.5f);
            colliderBgRect.anchorMax = new Vector2(0, 0.5f);
            colliderBgRect.sizeDelta = new Vector2(20, 20);
            colliderBgRect.pivot = new Vector2(0.5f, 0.5f);
            colliderBgRect.anchoredPosition = new Vector2(10, 0);
            Image colliderBgImg = colliderBg.AddComponent<Image>();
            colliderBgImg.color = new Color(0.2f, 0.2f, 0.2f);
            colliderToggle.targetGraphic = colliderBgImg;

            // 创建Toggle勾选框
            GameObject colliderCheck = new GameObject("Checkmark");
            colliderCheck.transform.SetParent(colliderBg.transform);
            RectTransform colliderCheckRect = colliderCheck.AddComponent<RectTransform>();
            colliderCheckRect.anchorMin = Vector2.zero;
            colliderCheckRect.anchorMax = Vector2.one;
            colliderCheckRect.offsetMin = new Vector2(3, 3);
            colliderCheckRect.offsetMax = new Vector2(-3, -3);
            Image colliderCheckImg = colliderCheck.AddComponent<Image>();
            colliderCheckImg.color = Color.white;
            colliderToggle.graphic = colliderCheckImg;

            // 创建标签
            GameObject colliderLabelGO = new GameObject("Label");
            colliderLabelGO.transform.SetParent(checkBoxCollider.transform);
            RectTransform colliderLabelRect = colliderLabelGO.AddComponent<RectTransform>();
            colliderLabelRect.anchorMin = Vector2.zero;
            colliderLabelRect.anchorMax = Vector2.one;
            colliderLabelRect.offsetMin = new Vector2(40, 0);
            colliderLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI colliderLabel = colliderLabelGO.AddComponent<TextMeshProUGUI>();
            if (colliderLabel != null && font != null)
            {
                colliderLabel.font = font;
            }
            if (colliderLabel != null)
            {
                colliderLabel.text = "显示碰撞";
                colliderLabel.alignment = TextAlignmentOptions.Left;
                colliderLabel.color = Color.white;
            }

            //创建外部保存按钮
            GameObject buttonSave = new GameObject("button_outsideSave");
            buttonSave.transform.SetParent(panel.transform);
            RectTransform saveRect = buttonSave.AddComponent<RectTransform>();
            saveRect.anchorMin = new Vector2(0.05f, 0.42f);
            saveRect.anchorMax = new Vector2(0.95f, 0.48f);
            saveRect.offsetMin = Vector2.zero;
            saveRect.offsetMax = Vector2.zero;
            Image saveImage = buttonSave.AddComponent<Image>();
            saveImage.color = new Color(0.6f, 0.3f, 0.3f);
            Button saveBtn = buttonSave.AddComponent<Button>();
            saveBtn.targetGraphic = saveImage;

            // 创建按钮文本子对象
            GameObject saveTextGO = new GameObject("Text");
            saveTextGO.transform.SetParent(buttonSave.transform);
            RectTransform saveTextRect = saveTextGO.AddComponent<RectTransform>();
            saveTextRect.anchorMin = Vector2.zero;
            saveTextRect.anchorMax = Vector2.one;
            TextMeshProUGUI saveText = saveTextGO.AddComponent<TextMeshProUGUI>();
            if (saveText != null && font != null)
            {
                saveText.font = font;
            }
            if (saveText != null)
            {
                saveText.text = "保存碰撞文件 [Enter]";
                saveText.alignment = TextAlignmentOptions.Center;
                saveText.color = Color.white;
            }

            //创建操作说明标签
            GameObject labelHelp = new GameObject("label_help");
            labelHelp.transform.SetParent(panel.transform);
            RectTransform helpRect = labelHelp.AddComponent<RectTransform>();
            helpRect.anchorMin = new Vector2(0.05f, 0.02f);
            helpRect.anchorMax = new Vector2(0.95f, 0.35f);
            helpRect.offsetMin = Vector2.zero;
            helpRect.offsetMax = Vector2.zero;
            TextMeshProUGUI helpText = labelHelp.AddComponent<TextMeshProUGUI>();
            if (helpText != null && font != null)
            {
                helpText.font = font;
            }
            if (helpText != null)
            {
                helpText.text = "操作说明:\n" +
                               "1键 - 人碰撞(A0)\n" +
                               "2键 - 车碰撞(A1)\n" +
                               "3键 - 人车碰撞\n" +
                               "左键 - 添加标记\n" +
                               "右键 - 移除标记\n" +
                               "Enter - 保存文件\n" +
                               "`键 - 切换界面";
                helpText.color = Color.white;
                helpText.fontSize = 14;
                helpText.alignment = TextAlignmentOptions.TopLeft;
            }

            // ==================== 按钮点击事件 ====================

            // 加载按钮点击事件
            btn.onClick.AddListener(() =>
            {
                Debug.Log("[MapEditor] 点击了【加载】按钮");
                string idStr = inputField.text;
                Debug.Log($"[MapEditor] 加载地图ID: {idStr}");
                // 这里可以添加实际的加载逻辑
            });

            // 保存按钮点击事件
            saveBtn.onClick.AddListener(() =>
            {
                Debug.Log("[MapEditor] 点击了【保存碰撞文件】按钮");
                // 这里可以添加实际的保存逻辑
            });

            // 纹理模式Toggle事件
            mapToggle.onValueChanged.AddListener((isOn) =>
            {
                Debug.Log($"[MapEditor] 纹理模式Toggle: {(isOn ? "开启" : "关闭")}");
            });

            // 显示碰撞Toggle事件
            colliderToggle.onValueChanged.AddListener((isOn) =>
            {
                Debug.Log($"[MapEditor] 显示碰撞Toggle: {(isOn ? "开启" : "关闭")}");
            });
        }

        #region 功能函数

        /// <summary>
        /// 获取或创建预制体通用方法.
        /// </summary>
        /// <param name="name">预制体名称</param>
        /// <param name="onCreate">对象不存在时,创建后的配置回调(仅在新创建时调用)</param>
        /// <returns>预制体GameObject</returns>
        private static GameObject GetOrCreatePrefab(string name, System.Action<GameObject> onCreate = null)
        {
            if (runtimePrefab.ContainsKey(name))
                return runtimePrefab.Get(name) as GameObject;

            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                onCreate?.Invoke(go);
            }
            runtimePrefab.Add(name, go);
            return go;
        }

        /// <summary>
        /// 读取素材的总动作
        /// </summary>
        public static void LoadAssets()
        {
            //读取余下资源
            LoadAllResources();
        }

        /// <summary>
        /// 读取所有资源(怪物、角色、载具的纹理和精灵切片)
        /// </summary>
        public static void LoadAllResources()
        {
            LoadAllVehicle();
            LoadAllCharacters();
            LoadAllMonsters();
        }

        /// <summary>
        /// 读取怪物纹理和精灵切片
        /// </summary>
        public static void LoadAllMonsters()
        {
            monsters = new List<Sprite>[132];
            // 初始化数组中的每个List元素
            for (int i = 0; i < monsters.Length; i++)
            {
                monsters[i] = new List<Sprite>();
            }
            //以下不是手敲的..扫描文件夹进行的打印,散图读取效率还行就懒得合并了,因为Unity已经切片,读完不用分割精灵直接可以使用数组monsters[TypeIndex][SpriteIndex]
            monsters[0].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E01_火焰枪"));
            monsters[1].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E01_火焰炮"));
            monsters[2].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E02_加农炮"));
            monsters[3].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E02_野战炮"));
            monsters[4].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E03_扫描仪"));
            monsters[5].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E03_监视器"));
            monsters[6].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E04_催眠器"));
            monsters[7].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E04_声纳车"));
            monsters[8].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E05_歼灭者"));
            monsters[9].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E05_防御器"));
            monsters[10].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E06_暗堡"));
            monsters[11].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E06_碉堡"));
            monsters[12].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E07_神秘人"));
            monsters[13].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E07_神风弹"));
            monsters[14].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E08_合金鸟"));
            monsters[15].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E08_机器鸟"));
            monsters[16].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E09_光防御器"));
            monsters[17].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E10_磁铁"));
            monsters[18].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E11_追踪弹"));
            monsters[19].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E12_僵尸"));
            monsters[20].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E12_弗朗"));
            monsters[21].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E13_激光系统"));
            monsters[22].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E14_智能人"));
            monsters[23].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E15_机器人"));
            monsters[24].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L01_巨蚁"));
            monsters[25].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L01_酸蚁"));
            monsters[26].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L02_变形虫"));
            monsters[27].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L02_杀人虫"));
            monsters[28].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L02_超导虫"));
            monsters[29].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L03_彷生蜗牛"));
            monsters[30].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L03_毒蜗牛"));
            monsters[31].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L04_地雷龟"));
            monsters[32].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L04_炸弹龟"));
            monsters[33].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L05_毒蜘蛛"));
            monsters[34].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L06_喷火鳄"));
            monsters[35].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L06_异形鱼"));
            monsters[36].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L07_流氓"));
            monsters[37].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L07_瓦鲁部下"));
            monsters[38].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L08_马歇尔"));
            monsters[39].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L09_反坦克兵G"));
            monsters[40].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L09_反坦克兵O"));
            monsters[41].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L10_侦查者"));
            monsters[42].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L10_异形"));
            monsters[43].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L11_激光虫"));
            monsters[44].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L11_离子虫"));
            monsters[45].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L12_声波蛇"));
            monsters[46].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L12_声纳蛇"));
            monsters[47].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L13_喷火怪"));
            monsters[48].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L13_水怪"));
            monsters[49].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L13_波特"));
            monsters[50].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L14_雷达花"));
            monsters[51].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L15_帕鲁"));
            monsters[52].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L16_戈麦斯"));
            monsters[53].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L17_军蚁"));
            monsters[54].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L17_金蚁毯"));
            monsters[55].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L18_金蚁"));
            monsters[56].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L18_食人蚁"));
            monsters[57].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L19_水鬼"));
            monsters[58].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L19_水鬼H"));
            monsters[59].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L19_蛙人"));
            monsters[60].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L20_电磁花"));
            monsters[61].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L20_食人花"));
            monsters[62].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L20_魔鬼花"));
            monsters[63].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L21_反坦克炮H"));
            monsters[64].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L21_反坦克炮R"));
            monsters[65].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L21_战狗"));
            monsters[66].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S01_章鱼坦克"));
            monsters[67].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S01_章鱼炮"));
            monsters[68].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S02_步枪鸟"));
            monsters[69].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S02_飞狗"));
            monsters[70].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S03_机械虫"));
            monsters[71].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S03_激光蚓"));
            monsters[72].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S04_化学炮"));
            monsters[73].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S04_生物炮"));
            monsters[74].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S05_后备车"));
            monsters[75].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S05_狙击鸟"));
            monsters[76].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S06_侦查蜂"));
            monsters[77].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S06_毒蜂"));
            monsters[78].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S07_毒蝙蝠"));
            monsters[79].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S08_地狱"));
            monsters[80].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S08_大象"));
            monsters[81].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S08_蜈蚣"));
            monsters[82].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S09_导弹蛙"));
            monsters[83].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S10_巨蟹"));
            monsters[84].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S10_机械蟹"));
            monsters[85].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S11_巨型河马"));
            monsters[86].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S11_机械河马"));
            monsters[87].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S12_铁甲炮"));
            monsters[88].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_85自行炮"));
            monsters[89].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_坦克"));
            monsters[90].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_导弹车"));
            monsters[91].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_音速车"));
            monsters[92].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_ATM战车"));
            monsters[93].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_指挥车"));
            monsters[94].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_瓦鲁"));
            monsters[95].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_高速车"));
            monsters[96].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T03_抓吊"));
            monsters[97].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T03_粉碎机"));
            monsters[98].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T03_起重机"));
            monsters[99].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T04_急救车"));
            monsters[100].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T05_噪音车"));
            monsters[101].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T05_声波炮"));
            monsters[102].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T05_铲车"));
            monsters[103].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T06_导弹卡车"));
            monsters[104].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T07_无坐力炮"));
            monsters[105].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T07_狙击车"));
            monsters[106].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T08_沙漠虎"));
            monsters[107].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T08_沙漠车"));
            monsters[108].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T09_AT坦克B"));
            monsters[109].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T09_AT坦克R"));
            monsters[110].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T10_沙漠之舟"));
            monsters[111].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T10_猎杀者"));
            monsters[112].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T11_戈斯战车"));
            monsters[113].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T11_离子坦克"));
            monsters[114].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T12_两栖车"));
            monsters[115].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T12_龟式战车"));
            monsters[116].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T13_攻击机"));
            monsters[117].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T13_防御机器"));
            monsters[118].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T14_古炮"));
            monsters[119].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T14_神武炮"));
            monsters[120].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T15_装甲车"));
            monsters[121].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T15_车载炮"));
            monsters[122].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T16_侦查碟"));
            monsters[123].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T16_拦截碟"));
            monsters[124].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_电脑墙"));
            monsters[125].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_诺亚v1"));
            monsters[126].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_诺亚v2"));
            monsters[127].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_诺亚v3"));
            monsters[128].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X02_毒液枪"));
            monsters[129].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X02_生物枪"));
            monsters[130].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X03_鬼手"));
            monsters[131].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X04_巨型炮"));
        }

        /// <summary>
        /// 读取角色纹理和精灵切片
        /// </summary>
        public static void LoadAllCharacters()
        {
            characters = new List<Sprite>[54];
            // 初始化数组中的每个List元素
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = new List<Sprite>();
            }
            characters[0].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/主角"));
            characters[1].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/主角2"));
            characters[2].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/主角3"));
            characters[3].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/佛像"));
            characters[4].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/修理师傅"));
            characters[5].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/勇士"));
            characters[6].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/勇士2"));
            characters[7].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/勇士3"));
            characters[8].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商人"));
            characters[9].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商店员"));
            characters[10].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商店员2"));
            characters[11].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商店员3"));
            characters[12].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/大象"));
            characters[13].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/头"));
            characters[14].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/女孩"));
            characters[15].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/女孩2"));
            characters[16].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/妇女"));
            characters[17].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/小孩"));
            characters[18].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女"));
            characters[19].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女2"));
            characters[20].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女3"));
            characters[21].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女4"));
            characters[22].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男"));
            characters[23].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男2"));
            characters[24].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男3"));
            characters[25].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男4"));
            characters[26].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/异型鱼"));
            characters[27].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/战狗"));
            characters[28].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/按摩师"));
            characters[29].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/旅行者"));
            characters[30].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/旅馆服务员"));
            characters[31].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/明奇博士"));
            characters[32].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/普通勇士"));
            characters[33].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/普通的尸体"));
            characters[34].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人"));
            characters[35].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人2"));
            characters[36].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人3"));
            characters[37].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人4"));
            characters[38].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/死人"));
            characters[39].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/气功师"));
            characters[40].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/气功师2"));
            characters[41].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/流氓"));
            characters[42].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/烧焦的尸体"));
            characters[43].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/狼"));
            characters[44].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/红狼"));
            characters[45].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/老年人"));
            characters[46].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/老者"));
            characters[47].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/船夫"));
            characters[48].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人"));
            characters[49].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人2"));
            characters[50].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人3"));
            characters[51].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人4"));
            characters[52].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/记录员"));
            characters[53].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/马歇尔"));
        }

        /// <summary>
        /// 读取载具纹理和精灵切片
        /// </summary>
        public static void LoadAllVehicle()
        {
            Vehicle = new List<Sprite>[9];
            // 初始化数组中的每个List元素
            for (int i = 0; i < Vehicle.Length; i++)
            {
                Vehicle[i] = new List<Sprite>();
            }
            Vehicle[0].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/1号战车"));
            Vehicle[1].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/2号战车"));
            Vehicle[2].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/3号战车"));
            Vehicle[3].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/4号战车"));
            Vehicle[4].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/5号战车"));
            Vehicle[5].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/6号战车"));
            Vehicle[6].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/7号战车"));
            Vehicle[7].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/8号战车"));
            Vehicle[8].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/船"));
        }

        #endregion

        /// <summary>
        /// 获取GameMenuCanvas预制体.作为单例直接使用.
        /// 如不存在,会创建名为"GameMenuCanvas"的游戏物体并添加Canvas组件及所有UI元素.
        /// </summary>
        public static GameObject GameMenuCanvas
        {
            get
            {
                string name = "GameMenuCanvas";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = GameObject.Find(name);
                    if (tempGameObject == null)
                    {
                        tempGameObject = new GameObject(name);
                        tempGameObject.SetActive(false);
                        tempGameObject.transform.parent = group.transform;
                        Canvas canvas = tempGameObject.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvas.sortingOrder = 200; // 确保菜单在最上层
                        CanvasScaler scaler = tempGameObject.AddComponent<CanvasScaler>();
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1920f, 1080f);
                        tempGameObject.AddComponent<GraphicRaycaster>();

                        //创建GameMenu所需的UI元素
                        CreateGameMenuUI(tempGameObject);

                        runtimePrefab.Add(name, tempGameObject);
                        Debug.Log($"预制体已创建: {name}");
                    }
                    else
                    {
                        runtimePrefab.Add(name, tempGameObject);
                    }
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 创建游戏菜单的UI元素
        /// </summary>
        private static void CreateGameMenuUI(GameObject parent)
        {
            //加载中文字体
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/FZYaSongS-M-GB-Regular-SDF");
            if (font == null)
            {
                font = TMP_Settings.defaultFontAsset;
                if (font == null)
                {
                    Debug.LogWarning("未能找到字体，将使用内置默认字体");
                }
            }

            //创建半透明背景
            GameObject bgPanel = new GameObject("BackgroundPanel");
            bgPanel.transform.SetParent(parent.transform);
            RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = bgPanel.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.7f); // 半透明黑色背景

            //创建设置面板
            GameObject panel = new GameObject("SettingsPanel");
            panel.transform.SetParent(parent.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.35f, 0.25f);
            panelRect.anchorMax = new Vector2(0.65f, 0.75f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);

            //创建标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.05f, 0.85f);
            titleRect.anchorMax = new Vector2(0.95f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            if (titleText != null && font != null)
            {
                titleText.font = font;
            }
            if (titleText != null)
            {
                titleText.text = "游戏设置 [F1键切换]";
                titleText.color = Color.yellow;
                titleText.fontSize = 24;
                titleText.alignment = TextAlignmentOptions.Center;
            }

            //创建小地图设置标签
            GameObject minimapLabelObj = new GameObject("MinimapLabel");
            minimapLabelObj.transform.SetParent(panel.transform);
            RectTransform minimapLabelRect = minimapLabelObj.AddComponent<RectTransform>();
            minimapLabelRect.anchorMin = new Vector2(0.05f, 0.72f);
            minimapLabelRect.anchorMax = new Vector2(0.95f, 0.78f);
            minimapLabelRect.offsetMin = Vector2.zero;
            minimapLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI minimapLabel = minimapLabelObj.AddComponent<TextMeshProUGUI>();
            if (minimapLabel != null && font != null)
            {
                minimapLabel.font = font;
            }
            if (minimapLabel != null)
            {
                minimapLabel.text = "小地图设置";
                minimapLabel.color = Color.white;
                minimapLabel.fontSize = 18;
                minimapLabel.alignment = TextAlignmentOptions.Left;
            }

            //创建小地图缩放标签
            GameObject zoomLabelObj = new GameObject("ZoomLabel");
            zoomLabelObj.transform.SetParent(panel.transform);
            RectTransform zoomLabelRect = zoomLabelObj.AddComponent<RectTransform>();
            zoomLabelRect.anchorMin = new Vector2(0.05f, 0.58f);
            zoomLabelRect.anchorMax = new Vector2(0.4f, 0.64f);
            zoomLabelRect.offsetMin = Vector2.zero;
            zoomLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI zoomLabel = zoomLabelObj.AddComponent<TextMeshProUGUI>();
            if (zoomLabel != null && font != null)
            {
                zoomLabel.font = font;
            }
            if (zoomLabel != null)
            {
                zoomLabel.text = "缩放大小:";
                zoomLabel.color = Color.white;
                zoomLabel.fontSize = 16;
                zoomLabel.alignment = TextAlignmentOptions.Left;
            }

            //创建缩放值显示
            GameObject zoomValueObj = new GameObject("ZoomValue");
            zoomValueObj.transform.SetParent(panel.transform);
            RectTransform zoomValueRect = zoomValueObj.AddComponent<RectTransform>();
            zoomValueRect.anchorMin = new Vector2(0.75f, 0.58f);
            zoomValueRect.anchorMax = new Vector2(0.95f, 0.64f);
            zoomValueRect.offsetMin = Vector2.zero;
            zoomValueRect.offsetMax = Vector2.zero;
            TextMeshProUGUI zoomValue = zoomValueObj.AddComponent<TextMeshProUGUI>();
            if (zoomValue != null && font != null)
            {
                zoomValue.font = font;
            }
            if (zoomValue != null)
            {
                zoomValue.text = "50%";
                zoomValue.color = Color.cyan;
                zoomValue.fontSize = 16;
                zoomValue.alignment = TextAlignmentOptions.Right;
            }

            //创建缩放滑块
            GameObject sliderObj = new GameObject("ZoomSlider");
            sliderObj.transform.SetParent(panel.transform);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.05f, 0.50f);
            sliderRect.anchorMax = new Vector2(0.95f, 0.56f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            Slider zoomSlider = sliderObj.AddComponent<Slider>();
            zoomSlider.minValue = 10f;
            zoomSlider.maxValue = 200f;
            zoomSlider.value = 50f;
            zoomSlider.fillRect = sliderObj.GetComponent<RectTransform>();

            //创建全屏复选框
            GameObject fullscreenObj = new GameObject("FullscreenToggle");
            fullscreenObj.transform.SetParent(panel.transform);
            RectTransform fullscreenRect = fullscreenObj.AddComponent<RectTransform>();
            fullscreenRect.anchorMin = new Vector2(0.05f, 0.38f);
            fullscreenRect.anchorMax = new Vector2(0.95f, 0.46f);
            fullscreenRect.offsetMin = Vector2.zero;
            fullscreenRect.offsetMax = Vector2.zero;
            Toggle fullscreenToggle = fullscreenObj.AddComponent<Toggle>();

            // 创建Toggle背景
            GameObject fsBg = new GameObject("Background");
            fsBg.transform.SetParent(fullscreenObj.transform);
            RectTransform fsBgRect = fsBg.AddComponent<RectTransform>();
            fsBgRect.anchorMin = new Vector2(0, 0.5f);
            fsBgRect.anchorMax = new Vector2(0, 0.5f);
            fsBgRect.sizeDelta = new Vector2(20, 20);
            fsBgRect.pivot = new Vector2(0.5f, 0.5f);
            fsBgRect.anchoredPosition = new Vector2(10, 0);
            Image fsBgImg = fsBg.AddComponent<Image>();
            fsBgImg.color = new Color(0.2f, 0.2f, 0.2f);
            fullscreenToggle.targetGraphic = fsBgImg;

            // 创建Toggle勾选框
            GameObject fsCheck = new GameObject("Checkmark");
            fsCheck.transform.SetParent(fsBg.transform);
            RectTransform fsCheckRect = fsCheck.AddComponent<RectTransform>();
            fsCheckRect.anchorMin = Vector2.zero;
            fsCheckRect.anchorMax = Vector2.one;
            fsCheckRect.offsetMin = new Vector2(3, 3);
            fsCheckRect.offsetMax = new Vector2(-3, -3);
            Image fsCheckImg = fsCheck.AddComponent<Image>();
            fsCheckImg.color = Color.white;
            fullscreenToggle.graphic = fsCheckImg;

            // 创建标签
            GameObject fsLabelGO = new GameObject("Label");
            fsLabelGO.transform.SetParent(fullscreenObj.transform);
            RectTransform fsLabelRect = fsLabelGO.AddComponent<RectTransform>();
            fsLabelRect.anchorMin = Vector2.zero;
            fsLabelRect.anchorMax = Vector2.one;
            fsLabelRect.offsetMin = new Vector2(40, 0);
            fsLabelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI fullscreenLabel = fsLabelGO.AddComponent<TextMeshProUGUI>();
            if (fullscreenLabel != null && font != null)
            {
                fullscreenLabel.font = font;
            }
            if (fullscreenLabel != null)
            {
                fullscreenLabel.text = "小地图全屏显示";
                fullscreenLabel.color = Color.white;
                fullscreenLabel.fontSize = 16;
                fullscreenLabel.alignment = TextAlignmentOptions.Left;
            }

            //创建关闭按钮
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(panel.transform);
            RectTransform closeBtnRect = closeBtnObj.AddComponent<RectTransform>();
            closeBtnRect.anchorMin = new Vector2(0.05f, 0.08f);
            closeBtnRect.anchorMax = new Vector2(0.35f, 0.16f);
            closeBtnRect.offsetMin = Vector2.zero;
            closeBtnRect.offsetMax = Vector2.zero;
            Image closeBtnImage = closeBtnObj.AddComponent<Image>();
            closeBtnImage.color = new Color(0.6f, 0.3f, 0.3f);
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImage;

            // 创建按钮文本子对象
            GameObject closeBtnTextGO = new GameObject("Text");
            closeBtnTextGO.transform.SetParent(closeBtnObj.transform);
            RectTransform closeBtnTextRect = closeBtnTextGO.AddComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            TextMeshProUGUI closeBtnText = closeBtnTextGO.AddComponent<TextMeshProUGUI>();
            if (closeBtnText != null && font != null)
            {
                closeBtnText.font = font;
            }
            if (closeBtnText != null)
            {
                closeBtnText.text = "关闭 [F1]";
                closeBtnText.color = Color.white;
                closeBtnText.fontSize = 18;
                closeBtnText.alignment = TextAlignmentOptions.Center;
            }

            //创建退出游戏按钮
            GameObject quitBtnObj = new GameObject("QuitButton");
            quitBtnObj.transform.SetParent(panel.transform);
            RectTransform quitBtnRect = quitBtnObj.AddComponent<RectTransform>();
            quitBtnRect.anchorMin = new Vector2(0.65f, 0.08f);
            quitBtnRect.anchorMax = new Vector2(0.95f, 0.16f);
            quitBtnRect.offsetMin = Vector2.zero;
            quitBtnRect.offsetMax = Vector2.zero;
            Image quitBtnImage = quitBtnObj.AddComponent<Image>();
            quitBtnImage.color = new Color(0.8f, 0.2f, 0.2f);
            Button quitBtn = quitBtnObj.AddComponent<Button>();
            quitBtn.targetGraphic = quitBtnImage;

            // 创建按钮文本子对象
            GameObject quitBtnTextGO = new GameObject("Text");
            quitBtnTextGO.transform.SetParent(quitBtnObj.transform);
            RectTransform quitBtnTextRect = quitBtnTextGO.AddComponent<RectTransform>();
            quitBtnTextRect.anchorMin = Vector2.zero;
            quitBtnTextRect.anchorMax = Vector2.one;
            TextMeshProUGUI quitBtnText = quitBtnTextGO.AddComponent<TextMeshProUGUI>();
            if (quitBtnText != null && font != null)
            {
                quitBtnText.font = font;
            }
            if (quitBtnText != null)
            {
                quitBtnText.text = "退出游戏";
                quitBtnText.color = Color.white;
                quitBtnText.fontSize = 18;
                quitBtnText.alignment = TextAlignmentOptions.Center;
            }

            //创建返回开局菜单按钮
            GameObject returnBtnObj = new GameObject("ReturnButton");
            returnBtnObj.transform.SetParent(panel.transform);
            RectTransform returnBtnRect = returnBtnObj.AddComponent<RectTransform>();
            returnBtnRect.anchorMin = new Vector2(0.05f, 0.20f);
            returnBtnRect.anchorMax = new Vector2(0.35f, 0.28f);
            returnBtnRect.offsetMin = Vector2.zero;
            returnBtnRect.offsetMax = Vector2.zero;
            Image returnBtnImage = returnBtnObj.AddComponent<Image>();
            returnBtnImage.color = new Color(0.3f, 0.5f, 0.8f);
            Button returnBtn = returnBtnObj.AddComponent<Button>();
            returnBtn.targetGraphic = returnBtnImage;

            // 创建按钮文本子对象
            GameObject returnBtnTextGO = new GameObject("Text");
            returnBtnTextGO.transform.SetParent(returnBtnObj.transform);
            RectTransform returnBtnTextRect = returnBtnTextGO.AddComponent<RectTransform>();
            returnBtnTextRect.anchorMin = Vector2.zero;
            returnBtnTextRect.anchorMax = Vector2.one;
            TextMeshProUGUI returnBtnText = returnBtnTextGO.AddComponent<TextMeshProUGUI>();
            if (returnBtnText != null && font != null)
            {
                returnBtnText.font = font;
            }
            if (returnBtnText != null)
            {
                returnBtnText.text = "返回开局";
                returnBtnText.color = Color.white;
                returnBtnText.fontSize = 18;
                returnBtnText.alignment = TextAlignmentOptions.Center;
            }

            // 添加返回开局菜单按钮点击事件
            returnBtn.onClick.AddListener(() =>
            {
                Debug.Log("[F1菜单] 返回开局菜单");
                // 隐藏F1菜单
                GameObject functionMenu = runtimePrefab.Get("FunctionMenu") as GameObject;
                if (functionMenu != null)
                {
                    functionMenu.SetActive(false);
                }
                // 显示开局菜单（由MMWorldInitializer确保存在）
                MMWorld.GameStartMenu menu = FindObjectOfType<MMWorld.GameStartMenu>();
                if (menu != null)
                {
                    menu.ShowStartMenu();
                }
            });

            // 添加关闭按钮点击事件
            closeBtn.onClick.AddListener(() =>
            {
                Debug.Log("[F1菜单] 关闭菜单");
                GameObject functionMenu = runtimePrefab.Get("FunctionMenu") as GameObject;
                if (functionMenu != null)
                {
                    functionMenu.SetActive(false);
                }
            });

            // 添加退出游戏按钮点击事件
            quitBtn.onClick.AddListener(() =>
            {
                Debug.Log("[F1菜单] 退出游戏");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            //保存UI引用供外部访问
            minimapZoomSlider = zoomSlider;
            minimapZoomValueText = zoomValue;
            minimapFullscreenToggle = fullscreenToggle;

            //监听缩放滑块变化
            zoomSlider.onValueChanged.AddListener((value) =>
            {
                int percent = Mathf.RoundToInt(value);
                if (minimapZoomValueText != null)
                {
                    minimapZoomValueText.text = percent + "%";
                }
                ApplyMinimapZoom(value);
            });

            //监听全屏复选框变化
            fullscreenToggle.onValueChanged.AddListener((isFullscreen) =>
            {
                Debug.Log($"[F1菜单] 小地图全屏显示: {(isFullscreen ? "开启" : "关闭")}");
                ApplyMinimapFullscreen(isFullscreen);
            });
        }

        /// <summary>
        /// 小地图缩放滑块引用
        /// </summary>
        private static Slider minimapZoomSlider;

        /// <summary>
        /// 小地图缩放值文本引用
        /// </summary>
        private static TextMeshProUGUI minimapZoomValueText;

        /// <summary>
        /// 小地图全屏复选框引用
        /// </summary>
        private static Toggle minimapFullscreenToggle;

        /// <summary>
        /// 应用小地图缩放
        /// </summary>
        /// <param name="zoomValue">缩放值(10-200)</param>
        public static void ApplyMinimapZoom(float zoomValue)
        {
            GameObject minimapCanvas = MinimapCanvas;
            if (minimapCanvas != null)
            {
                GameObject rawImageObj = minimapCanvas.transform.Find("MinimapRawImage")?.gameObject;
                if (rawImageObj != null)
                {
                    RectTransform rect = rawImageObj.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        // 根据缩放值调整RawImage大小
                        float scale = zoomValue / 50f; // 以50为基准
                        rect.localScale = new Vector3(scale, scale, 1f);
                        Debug.Log($"小地图缩放已调整为: {zoomValue}%");
                    }
                }
            }
        }

        /// <summary>
        /// 应用小地图全屏/窗口模式
        /// </summary>
        /// <param name="isFullscreen">是否为全屏模式</param>
        public static void ApplyMinimapFullscreen(bool isFullscreen)
        {
            GameObject minimapCanvas = MinimapCanvas;
            if (minimapCanvas != null)
            {
                GameObject rawImageObj = minimapCanvas.transform.Find("MinimapRawImage")?.gameObject;
                if (rawImageObj != null)
                {
                    RectTransform rect = rawImageObj.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        if (isFullscreen)
                        {
                            // 全屏模式
                            rect.anchorMin = Vector2.zero;
                            rect.anchorMax = Vector2.one;
                            rect.offsetMin = Vector2.zero;
                            rect.offsetMax = Vector2.zero;
                            rect.localScale = Vector3.one;
                            Debug.Log("小地图已切换为全屏模式");
                        }
                        else
                        {
                            // 窗口模式（右上角）
                            rect.anchorMin = new Vector2(0.78f, 0.78f);
                            rect.anchorMax = new Vector2(0.98f, 0.98f);
                            rect.offsetMin = Vector2.zero;
                            rect.offsetMax = Vector2.zero;
                            float zoomValue = minimapZoomSlider != null ? minimapZoomSlider.value : 50f;
                            ApplyMinimapZoom(zoomValue);
                            Debug.Log("小地图已切换为窗口模式");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取小地图缩放值
        /// </summary>
        /// <returns>当前缩放值</returns>
        public static float GetMinimapZoom()
        {
            return minimapZoomSlider != null ? minimapZoomSlider.value : 50f;
        }

        /// <summary>
        /// 获取小地图是否全屏
        /// </summary>
        /// <returns>是否全屏</returns>
        public static bool IsMinimapFullscreen()
        {
            return minimapFullscreenToggle != null && minimapFullscreenToggle.isOn;
        }

        /// <summary>
        /// 获取小地图渲染纹理。如果不存在则创建，存在则直接返回。
        /// </summary>
        public static RenderTexture GetMiniMap()
        {
            // 1. 检查是否已存在且有效
            if (minimap != null && minimap.IsCreated())
            {
                return minimap;
            }

            // 2. 清理无效引用
            if (minimap != null)
            {
                minimap = null;
            }

            // 3. 使用描述符配置（兼容写法）
            RenderTextureDescriptor descriptor = new RenderTextureDescriptor(480, 270);

            // 【修改点】使用 RenderTextureFormat 枚举，而不是 GraphicsFormat
            descriptor.colorFormat = RenderTextureFormat.ARGB32;

            // 【修改点】深度格式也使用整数或默认值，避免使用 GraphicsFormat
            // 如果必须指定 D32_SFloat_S8_UInt，旧版 API 可能不支持直接设置
            // 这里使用 depth=32 让 Unity 自动选择最佳深度格式（通常包含模板缓冲）
            descriptor.depthBufferBits = 32;

            descriptor.msaaSamples = 1;       // 无抗锯齿
            descriptor.useMipMap = false;     // 禁用 Mipmap
            descriptor.autoGenerateMips = false;

            // 4. 创建纹理
            minimap = new RenderTexture(descriptor);
            minimap.name = "MinimapRT_Compatible";

            // 5. 设置运行时属性
            minimap.filterMode = FilterMode.Bilinear;
            minimap.wrapMode = TextureWrapMode.Clamp;
            minimap.enableRandomWrite = false; // 普通小地图关闭随机写入以节省性能

            // 6. 显式创建
            minimap.Create();

            return minimap;
        }

        /// <summary>
        /// 手动释放资源（用于场景切换或不再需要时）
        /// </summary>
        public static void Release()
        {
            if (minimap != null)
            {
                minimap.Release();
                minimap = null;
            }
        }

        /// <summary>
        /// 获取或创建EventSystem用于处理UI交互事件
        /// </summary>
        public static GameObject GetEventSystem()
        {
            EventSystem existingEventSystem = Object.FindObjectOfType<EventSystem>();
            if (existingEventSystem != null)
            {
                Debug.Log("EventSystem已存在，无需创建");
                return existingEventSystem.gameObject;
            }

            //创建EventSystem对象
            GameObject eventSystemGO = new GameObject("EventSystem");
            DontDestroyOnLoad(eventSystemGO);
            
            // 添加到group下（如果group存在）
            if (group != null)
            {
                eventSystemGO.transform.parent = group.transform;
            }

            //添加EventSystem组件
            eventSystemGO.AddComponent<EventSystem>();
            
            //添加StandaloneInputModule组件（处理鼠标/键盘输入）
            eventSystemGO.AddComponent<InputSystemUIInputModule>();

            Debug.Log("EventSystem已创建");
            return eventSystemGO;
        }

    }
}
