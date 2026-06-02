using CellSpace;
using UnityEngine;

namespace SpriteSpace
{
    //动态精灵空间管理框架(SpriteSpace).与静态地面体素单元空间框架(CellSpace)配合使用.
    //本框架管理场景中的活动对象(如怪物、子弹、特效、角色、道具等).支持2D横板、3D单层地形模式.

    /// <summary>
    /// 场景.
    /// </summary>
    public class Scene : MonoBehaviour
    {
        /// <summary>
        /// 屏幕主摄像机组件.若通过SpriteSpacePrefab.MainCamera获取则不需要修改,否则请自定义主摄像机.
        /// </summary>
        public static Camera mainCamera;

        public static Material material;
        public static Material minimapMaterial;

        //若通过编辑器拖游戏物体到公开非静态实例字段会自动识别到组件
        public static GameObject minimapCameraGO;
        public static GameObject minimapCanvasGO;
        public static Camera minimapCamera;
        public static Canvas minimapCanvas;

        //若通过编辑器SpriteEditor‌将大图切割并配置好子精灵可拖到公开非静态实例字段(shift可多选)
        public static Sprite[] sprites_font_outline;
        public static Sprite[] sprites_explosions;
        public static Sprite[] sprites_player;
        public static Sprite[] sprites_bullets;
        public static Sprite[] sprites_monster01;
        public static Sprite[] sprites_monster02;
        public static Sprite[] sprites_monster03;
        public static Sprite[] sprites_monster04;
        public static Sprite[] sprites_monster05;
        public static Sprite[] sprites_monster06;
        public static Sprite[] sprites_monster07;
        public static Sprite[] sprites_monster08;

        private CellSpace.CellGridContainer _gridContainer;
        /// <summary>
        /// 场景的网格空间容器.它是一个逻辑概念,不直接关联任何GameObject,管理场景中活动对象在网格空间中的状态和位置.
        /// </summary>
        public CellSpace.CellGridContainer GridContainer
        {
            set
            {
                _gridContainer = value;
            }
            get
            {
                return _gridContainer;
            }
        }

        private static int id = 0;

        /// <summary>
        /// 创建一个新的场景实例.它会尝试在当前Unity场景中查找名为"Scene"加上一个唯一编号的GameObject,
        /// 如果未找到则创建一个新的GameObject并添加Scene组件.
        /// </summary>
        /// <returns>新创建或找到的Scene实例</returns>
        public static Scene New()
        {
            var obj = GameObject.Find("Scene" + id);
            if (obj == null) { obj = new GameObject("Scene" + id++); }
            if (obj.GetComponent<Scene>() == null) obj.AddComponent<Scene>();
            return obj.GetComponent<Scene>();
        }

        /// <summary>
        /// 当前帧(总的运行帧编号)
        /// </summary>
        public int time = 0;
        /// <summary>
        /// 时间累计变量
        /// </summary>
        public float timePool = 0;
        /// <summary>
        /// 舞台
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 场景可操作的玩家(可跨舞台存在)
        /// </summary>
        public Player player;
        /// <summary>
        /// 网格尺寸(尺度单位是逻辑像素非真实像素),决定空间或地图逻辑像素及屏幕摄像头占据的大小.
        /// </summary>
        public float gridSize;
        /// <summary>
        /// 网格空间边长(单元数量).默认等同CellItemManager.sideLength或CPEngine.chunkSideLength.
        /// </summary>
        public int gridLength;
        /// <summary>
        /// 网格空间最大尺寸(逻辑大小非实际像素大小)
        /// </summary>
        public float gridMaxSize;
        /// <summary>
        /// 网格空间尺寸的一半(方便计算和使用)
        /// </summary>
        public float gridMaxSize_2;

        private int _tps = 60;
        /// <summary>
        /// 目标帧率.决定游戏逻辑更新频率.允许修改来调整游戏速度.它是逻辑值,并非引擎真实渲染帧率.
        /// </summary>
        public int TPS
        {
            set
            {
                if (value <= 0)
                {
                    return;
                }
                _tps = value;
                frameDelay = 1f / _tps;
            }
            get { return _tps; }
        }
        /// <summary>
        /// 目标帧率间隔(1/tps)
        /// </summary>
        public float frameDelay;
        /// <summary>
        /// 目标分辨率(非真实像素).影响渲染范围.
        /// </summary>
        public float designWidth = 3840, designHeight = 2160;
        /// <summary>
        /// 目标分辨率的一半(方便计算和使用).影响渲染范围及出怪点等游戏逻辑.
        /// </summary>
        public float designWidth_2, designHeight_2;
        /// <summary>
        /// 决定设计分辨率在正交摄像机空间占据的大小比例(默认值1f).orthographicSize = 10f * designWidthToCameraRatio.
        /// </summary>
        public float designWidthToCameraRatio = 1f;
        /// <summary>
        /// 精灵图片离地相对高度.
        /// </summary>
        public float aboveHeight = 0.1f;
        /// <summary>
        /// 小地图开启状态.
        /// </summary>
        public bool minimapEnabled = false;
        /// <summary>
        /// 是否创建小地图用的GameObject(默认不创建,节省性能).
        /// </summary>
        public bool mGOCreate = false;
        /// <summary>
        /// 单元环形扩散数据.它是一个预计算的数据结构,用于支持基于网格空间的范围扩散效果(如爆炸伤害范围、技能范围等).通过预计算可以提升运行时性能,避免重复计算扩散范围.
        /// </summary>
        public CellRingDiffuseData cellRingDiffuseData;

        /// <summary>
        /// 输入操作管理器.负责处理玩家输入并转换为游戏逻辑事件.它是一个公用单例对象.
        /// </summary>
        public static InputActions inputActions;
        /// <summary>
        /// 当前主要场景.
        /// </summary>
        public static Scene main;

        /// <summary>
        /// 场景初始化方法.它负责设置场景的网格空间容器、设计分辨率、目标帧率以及相关资源和组件的引用.在场景创建后调用一次,为后续游戏逻辑更新和渲染做好准备.
        /// </summary>
        /// <param name="cellGridContainer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Init(CellSpace.CellGridContainer cellGridContainer, float width = 3840, float height = 2160)
        {
            if (cellGridContainer != null) _gridContainer = cellGridContainer;

            frameDelay = 1f / _tps;
            gridSize = _gridContainer.CellSize;
            gridLength = _gridContainer.SideLength;
            gridMaxSize = gridLength * gridSize;
            gridMaxSize_2 = gridMaxSize / 2;
            designWidth = width;
            designHeight = height;
            designWidth_2 = designWidth / 2;
            designHeight_2 = designHeight / 2;
            cellRingDiffuseData = new CellRingDiffuseData(_gridContainer.SideLength, gridSize);

            if (material == null) material = SpriteSpacePrefab.material;
            if (minimapMaterial == null) minimapMaterial = SpriteSpacePrefab.material;
            if (minimapCameraGO == null) minimapCameraGO = SpriteSpacePrefab.MinimapCamera;
            if (minimapCanvasGO == null) minimapCanvasGO = SpriteSpacePrefab.MinimapCanvas;
            if (minimapCamera == null) minimapCamera = minimapCameraGO.GetComponent<Camera>();
            if (minimapCanvas == null) minimapCanvas = minimapCanvasGO.GetComponent<Canvas>();

            InitMainCamera();

            if (inputActions == null)
            {
                inputActions = new InputActions();
                inputActions.InitInputAction();
            }

            //初始化玩家
            //player = new Player(this);

            //初始化舞台
            //stage = new Stage1(this);

            //测试其他舞台
            //stage = new TestStage1(this); //直接测试大量数字特效
            //stage = new TestStage2(this); //正常打怪测试

        }

        /// <summary>
        /// 场景更新方法.按设计帧率驱动游戏逻辑更新,并在每帧同步调用舞台的Draw方法进行渲染.它是游戏循环的核心部分,负责处理玩家输入、更新游戏状态和渲染场景.
        /// </summary>
        private void Update()
        {
            if (inputActions == null || stage == null) return;

            //处理玩家输入
            inputActions.HandlePlayerInput();

            // 按F键切换AI/玩家控制模式
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleAIControl();
            }

            // 按M键切换地图编辑器显示
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMapEditor();
            }

            // 按N键切换小地图显示/隐藏
            if (Input.GetKeyDown(KeyCode.N))
            {
                ToggleMinimapCanvas();
            }

            // 按F1键切换游戏菜单显示/隐藏
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleGameMenu();
            }

            var timeBak = time;
            //按设计帧率驱动游戏逻辑
            timePool += Time.deltaTime;
            if (timePool > frameDelay)
            {
                timePool -= frameDelay;
                ++time;
                stage.Update();
            }

            #region 小地图更新频率

            //若启用了小地图那么只在Update执行过的帧才启用minimap的camera(提升点性能)
            if (minimapEnabled)
            {
                //若游戏逻辑被运行且次数是偶数那么启用小地图镜头
                minimapCamera.enabled = timeBak != time && (time & 1) == 0; //位运算,功能等价(time % 2) == 0
            }

            #endregion

            //同步绘制
            if (stage != null) stage.Draw();
        }

        /// <summary>
        /// 切换AI/玩家控制模式
        /// </summary>
        private void ToggleAIControl()
        {
            if (player != null)
            {
                player.isAIControl = !player.isAIControl;
                string mode = player.isAIControl ? "AI控制" : "玩家控制";
                Debug.Log($"控制模式已切换: {mode}");
            }
        }

        /// <summary>
        /// 切换地图编辑器显示/隐藏
        /// </summary>
        private void ToggleMapEditor()
        {
            GameObject mapEditorCanvas = SpriteSpacePrefab.MapEditorCanvas;
            if (mapEditorCanvas != null)
            {
                bool isActive = !mapEditorCanvas.activeSelf;
                mapEditorCanvas.SetActive(isActive);
                Debug.Log($"地图编辑器已{(isActive ? "打开" : "关闭")}");
            }
        }

        /// <summary>
        /// 切换小地图画布对象的显示/隐藏
        /// </summary>
        private void ToggleMinimapCanvas()
        {
            GameObject minimapCanvas = SpriteSpacePrefab.MinimapCanvas;
            if (minimapCanvas != null)
            {
                bool isActive = !minimapCanvas.activeSelf;
                minimapCanvas.SetActive(isActive);
                Debug.Log($"小地图画布已{(isActive ? "显示" : "隐藏")}");
            }
        }

        /// <summary>
        /// 切换游戏菜单显示/隐藏
        /// </summary>
        private void ToggleGameMenu()
        {
            GameObject gameMenuCanvas = SpriteSpacePrefab.GameMenuCanvas;
            if (gameMenuCanvas != null)
            {
                bool isActive = !gameMenuCanvas.activeSelf;
                gameMenuCanvas.SetActive(isActive);
                Debug.Log($"游戏菜单已{(isActive ? "打开" : "关闭")}");
            }
        }

        /// <summary>
        /// 绘制编辑器场景对象
        /// </summary>
        private void OnDrawGizmos()
        {
            //验证是否有舞台,有则绘制无则返回
            if (stage == null) return;
            stage.DrawGizmos();
        }

        /// <summary>
        /// 摧毁舞台对象和对象池资源
        /// </summary>
        private void OnDestroy()
        {
            if (stage != null) stage.Destroy();
        }

        /// <summary>
        /// 设置舞台.
        /// </summary>
        /// <param name="newStage"></param>
        /// <param name="oldDestroy">默认true会摧毁当前舞台,可设置false以保留/param>
        public void SetStage(Stage newStage, bool oldDestroy = true)
        {
            if (oldDestroy && stage != null) stage.Destroy();//清理旧舞台
            stage = newStage;//设置新的舞台
        }

        /// <summary>
        /// 启禁用小地图功能.
        /// </summary>
        /// <param name="torf">启动小地图(不填则默认为真)</param>
        public void EnableMinimap(bool torf = true)
        {
            minimapCanvasGO?.SetActive(torf);
            minimapCameraGO?.SetActive(torf);
            if (minimapEnabled == torf) return;//没变化时直接返回
            minimapEnabled = torf;//刷新场景minimapEnabled字段
            if (minimapCanvas != null) minimapCanvas.enabled = torf;//决定画布是否启用
            if (minimapCamera != null) minimapCamera.enabled = torf;//决定小地图摄像机是否启用
            Debug.Log("小地图已" + (torf ? "启用" : "禁用"));
        }

        /// <summary>
        /// 初始化主摄像机.根据当前游戏模式(2D横板或3D单层地形)设置摄像机的投影方式、位置和旋转.
        /// 如果未通过SpriteSpacePrefab.MainCamera获取到主摄像机,则会尝试从场景中查找名为"MainCamera"的摄像机对象.
        /// 如果仍然未找到,则会输出错误日志.
        /// </summary>
        public void InitMainCamera()
        {
            //使用SpriteSpacePrefab创建的主摄像机
            if (mainCamera == null) mainCamera = SpriteSpacePrefab.MainCamera.GetComponent<Camera>();

            if (mainCamera != null)
            {
                if (CPEngine.horizontalMode)
                {
                    //2D横板模式用正交投影
                    mainCamera.orthographic = true;
                    mainCamera.orthographicSize = 10f * designWidthToCameraRatio; //默认正交尺寸
                    mainCamera.gameObject.transform.position = new Vector3(0, 0, -20);
                }
                else if (CPEngine.singleLayerTerrainMode)
                {
                    //3D单层地形模式用正交投影
                    mainCamera.orthographic = true;
                    mainCamera.orthographicSize = 10f * designWidthToCameraRatio;
                    Debug.Log("[CPEngine.singleLayerTerrainMode]正交镜头:摄像机默认正交尺寸 = " + mainCamera.orthographicSize);
                    mainCamera.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0); //原横板模式设计的摄像机绕X轴顺时针转90度以俯视X-Z平面
                    mainCamera.gameObject.transform.position = new Vector3(0, 20, 0);
                }
                else
                {
                    Debug.LogError("SpriteSpace框架仅支持2D横板模式(X-Y平面)、3D单层地形模式(X-Z平面)");
                }
            }
            else
            {
                Debug.LogError("主摄像机组件获取失败！");
            }
        }
    }
}

//利用反射来读取怪物配置
//var st = typeof(Scene); //获取类型信息
//获取私有方法,指定BindingFlags
//MethodInfo privateMethod = type.GetMethod("方法名",BindingFlags.NonPublic | BindingFlags.Instance);
//设置方法可访问(如需)
//privateMethod?.SetAccessible(true);
//var fs = st.GetFields(BindingFlags.Public | BindingFlags.Instance); //如果要获取私有字段则换成BindingFlags.NonPublic
//foreach (var f in fs) {
//    if (f.FieldType.Name == "Sprite[]") {
//        if (f.Name.StartsWith("sprites_monster")) {
//            var ss = f.GetValue(scene) as Sprite[];
//            if (ss.Length > 0) {
//                spritess.Add(ss);
//            }
//        }
//    }
//}
//用反射获取MethodInfo后可将其转换为一个类型明确的委托,之后调用该方法时直接使用缓存的委托即可避免后续重复反射开销.

//本框架用于管理静态地面上的活动精灵,可配合静态地面体素单元空间框架(CellSpace)的2D横板、3D单层地形模式使用.
//框架自带双向链表支持2D管理检索场景中的活动对象(如怪物、子弹、特效、角色、道具等).
//可自行修改设计,如活动对象的基类GridItem继承CellSpace框架的CellItem时,可允许将它们添加到CellSpace框架的双向链表以支持3D管理和检索.
//随着具体游戏设计,与其他框架会耦合较深.SpriteSpace可脱离静态地面空间框架单独测试上面的活动对象,它原型是一个幸存者demo.
//Scene支持多实例切换,静态变量(如小地图开关)是设计让所有实例公用,常量(隐式静态)是用于赋值一次后不允许再修改(实例化后不允许变化)的数据.
//允许将一个单元划分得更细小来检索怪物在grid上的位置,同时用来设计屏幕范围(逻辑像素)在grid空间总的逻辑像素尺寸下的比例.
//坐标分世界(绝对)坐标、本地(local/相对父级物体/局部)坐标、逻辑(像素)坐标三种,前2种直接关联渲染位置,逻辑坐标则方便参与游戏逻辑的尺寸距离直接判断
//CellSpace里还有个怪物容器是本地坐标的(不冲突),注意区别即可