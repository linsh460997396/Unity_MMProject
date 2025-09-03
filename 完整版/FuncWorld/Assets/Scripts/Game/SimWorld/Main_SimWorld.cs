using UnityEngine;

namespace SimWorld
{
    public partial class Main_SimWorld : MonoBehaviour
    {

        // 拖拽时先点击 锁 图标锁定

        // 编辑器中拖拽 带法线的材质球到此 ( texture packer 插件 生成的那个, 需要核查法线贴图是否正确 )
        public Material material;

        // 这个拖 Packages\Universal\Materials\Sprite-Unlit-Default
        public Material minimap_material;

        // 这个拖 Mini Map Canvas 节点
        public Canvas minimap_canvas;

        // 这个拖 Mini Map Camera 节点
        public Camera minimap_camera;

        // 编辑器中 分组, 多选 拖拽 精灵图集到此 ( texture packer 插件 生成的那个, 展开再 shift 多选 )
        public Sprite[] sprites_font_outline;
        public Sprite[] sprites_explosions;
        public Sprite[] sprites_player;
        public Sprite[] sprites_bullets;
        public Sprite[] sprites_monster01;
        public Sprite[] sprites_monster02;
        public Sprite[] sprites_monster03;
        public Sprite[] sprites_monster04;
        public Sprite[] sprites_monster05;
        public Sprite[] sprites_monster06;
        public Sprite[] sprites_monster07;
        public Sprite[] sprites_monster08;
        // ...


        // 地图格子数量
        internal const int numRows = 1024, numCols = 1024;

        // 地图总宽度
        internal const float gridWidth = numCols * Main_SimWorld.gridSize;

        // 地图总高度
        internal const float gridHeight = numRows * Main_SimWorld.gridSize;

        // 地图中心点坐标
        internal const float gridWidth_2 = gridWidth / 2, gridHeight_2 = gridHeight / 2;
        internal const float gridCenterX = gridWidth_2, gridCenterY = gridHeight_2;

        // 逻辑帧率
        internal const int fps = 60;

        // 逻辑帧率间隔时长
        internal const float frameDelay = 1.0f / fps;

        // 设计分辨率
        internal const float designWidth = 1920, designHeight = 1080;

        // 设计分辨率的一半 方便计算和使用
        internal const float designWidth_2 = designWidth / 2, designHeight_2 = designHeight / 2;

        // 设计分辨率到 摄像头坐标 的转换系数 
        internal const float designWidthToCameraRatio = 19 / designWidth;    // todo: 需要进一步找准这个数据

        // 每个格子的直径( 正方形 )
        internal const float gridSize = 32;

        // 一些常数
        internal const float sqrt2 = 1.414213562373095f;
        internal const float sqrt2_1 = 0.7071067811865475f;

        // 当前是否已开启 minimap
        internal bool minimapEnabled = true;

        //// 当前帧是否已执行过 update 函数( 每帧重置 )
        //internal bool updated = false;

        // 当前总的运行帧编号
        internal int time = 0;

        // 用于稳定调用 逻辑 Update 的时间累计变量
        internal float timePool = 0;

        // 当前玩家( 玩家可跨越关卡存在, 故放置在此 )
        internal SWPlayer player;

        // 当前关卡
        internal SWStage stage;

        // 空间索引容器 要用到的找最近所需要的格子偏移数组( all stage 公用 )
        internal static SWSpaceRingDiffuseData spaceRDD = new(100, (int)gridSize);


        void Start()
        {

            // 初始化玩家输入系统
            InitInputAction();

            // 初始化 HDR 显示模式
            //try
            //{
            //    HDROutputSettings.main.RequestHDRModeChange(true);
            //}
            //catch (Exception e)
            //{
            //    Debug.Log(e);
            //}

            // 初始化 底层绘制对象池
            GO.Init(material, 20000);

            // 初始化 玩家
            player = new SWPlayer(this);

            // 初始化起始关卡
            //stage = new SWStage1(this);
            stage = new SWStage2(this);
        }

        void Update()
        {
            // 处理输入( 只是填充 playerMoving 等状态值 )
            HandlePlayerInput();

            var timeBak = time;

            // 按设计帧率驱动游戏逻辑
            timePool += Time.deltaTime;
            if (timePool > frameDelay)
            {
                timePool -= frameDelay;
                ++time;
                stage.Update();
            }

            //updated = timeBak != time;

            // 若启用了小地图,那么只在 update 执行过的帧,才启用 minimap 的 camera,提升点性能
            if (minimapEnabled)
            {
                minimap_camera.enabled = (timeBak != time) && ((time & 1) == 0);
            }

            // 同步显示
            stage.Draw();
        }

        void OnDrawGizmos()
        {
            if (stage == null) return;
            stage.DrawGizmos();
        }

        void OnDestroy()
        {
            stage.Destroy();
            GO.Destroy();
        }

        internal void SetStage(SWStage newStage)
        {
            stage.Destroy();
            stage = newStage;
        }

        // 启用 禁用 小地图( 可间歇 开关 minimapCamera 来提升性能 )
        internal void EnableMinimap(bool b = true)
        {
            if (minimapEnabled == b) return;
            minimapEnabled = b;
            minimap_canvas.enabled = b;
            minimap_camera.enabled = b;
        }

    }    
}

//显示设置改为dx12可在editor里观察hdr效果

//// 利用反射来读取 Main_SimWorld 里面的怪物配置
//var st = typeof(Main_SimWorld);
//var fs = st.GetFields(BindingFlags.Public | BindingFlags.Instance);
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
