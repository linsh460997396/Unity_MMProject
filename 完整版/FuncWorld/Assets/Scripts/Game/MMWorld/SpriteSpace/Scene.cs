using CellSpace;
using System;
using UnityEngine;

namespace SpriteSpace
{
    //精灵空间管理框架(SpriteSpace).

    /// <summary>
    /// 场景.
    /// </summary>
    public class Scene : MonoBehaviour
    {
        //编辑器拖拽带法线的材质球到此
        [NonSerialized] public Material material;
        //编辑器拖Sprite-Unlit-Default到此,或不启用URP时用别的
        [NonSerialized] public Material minimapMaterial;

        public GameObject minimapCameraGO;
        public GameObject minimapCanvasGO;

        //编辑器拖游戏物体到此,会自动识别到组件
        [NonSerialized] public Camera minimapCamera;
        [NonSerialized] public Canvas minimapCanvas;

        //编辑器中分组,拖拽精灵图集到此(展开shift可多选)
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

        /// <summary>
        /// 当前帧(总的运行帧编号)
        /// </summary>
        public int time = 0;
        /// <summary>
        /// 用于稳定调用逻辑Update的时间累计变量
        /// </summary>
        public float timePool = 0;
        /// <summary>
        /// 当前玩家(可跨舞台存在故放置在此类)
        /// </summary>
        public Player player;
        /// <summary>
        /// 当前舞台
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 精灵空间行、列数量.如不再逻辑划分怪物区域,默认同CellSpace中团块空间的边长.
        /// </summary>
        public int gridNumRows = 256, gridNumCols = 256;
        /// <summary>
        /// 网格团块内的网格最大宽度尺寸(单位是逻辑像素非真实像素)
        /// </summary>
        public float gridWidth;
        /// <summary>
        /// 网格团块内的网格最大高度尺寸(单位是逻辑像素非真实像素)
        /// </summary>
        public float gridHeight;
        /// <summary>
        /// 网格团块的宽度尺寸中心(单位是逻辑像素非真实像素)
        /// </summary>
        [NonSerialized] public float gridWidth_2, gridHeight_2;
        /// <summary>
        /// 网格团块的高度尺寸中心(单位是逻辑像素非真实像素)
        /// </summary>
        public float gridChunkCenterX, gridChunkCenterY;
        /// <summary>
        /// 设计(目标)帧率.决定游戏逻辑更新频率.允许修改来调整游戏速度.它是逻辑值,并非引擎真实渲染帧率.
        /// </summary>
        public int tps = 60;
        /// <summary>
        /// 帧率间隔(逻辑帧率的倒数)
        /// </summary>
        public float frameDelay;
        /// <summary>
        /// 设计分辨率(单位是逻辑像素非真实像素),决定了屏幕(摄像机镜头)范围占空间总大小多少.
        /// 为了达到现实中屏幕分辨率,逻辑像素范围可取1920*1080这样的默认值.修改gridSize和grid行列数可调整网格团块一角或全部被屏幕看到的比例.
        /// </summary>
        public float designWidth = 3840, designHeight = 2160;
        /// <summary>
        /// 设计分辨率的一半(方便计算和使用)
        /// </summary>
        [NonSerialized] public float designWidth_2, designHeight_2;
        /// <summary>
        /// 设计分辨率到摄像头坐标的转换系数(允许渲染位置与逻辑位置不同,如戴森球那游戏的庞大星系不按实际距离尺寸制作时).
        /// 用于怪物坐标修正(渲染位置=逻辑位置*designWidthToCameraRatio),如不需要可设为1(无转换).在(0,1)区间调整.
        /// </summary>
        public float designWidthToCameraRatio = 1f;
        /// <summary>
        /// 网格尺寸(尺度单位是逻辑像素非真实像素),决定空间或地图总的逻辑像素大小及屏幕(摄像头)占据的大小.
        /// </summary>
        public float gridSize = 100f;
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
        /// 空间容器索引时要用到的找最近所需格子的偏移量数组(所有舞台公用)
        /// </summary>
        public SpaceRingDiffuseData spaceRDD;

        public static InputActions inputActions;
        public static Scene mainScene;

        void Start()
        {
            frameDelay = 1f / tps;
            gridWidth = gridNumCols * gridSize;
            gridHeight = gridNumRows * gridSize;
            gridWidth_2 = gridWidth / 2;
            gridHeight_2 = gridHeight / 2;
            gridChunkCenterX = gridWidth_2;
            gridChunkCenterY = gridHeight_2;
            designWidth_2 = designWidth / 2;
            designHeight_2 = designHeight / 2;
            spaceRDD = new(100, gridSize);

            material = SpriteSpacePrefab.material;
            minimapMaterial = material;
            minimapCamera = minimapCameraGO.GetComponent<Camera>();
            minimapCanvas = minimapCanvasGO.GetComponent<Canvas>();

            InitCamera();

            inputActions = new InputActions();
            inputActions.InitInputAction();



            //初始化玩家
            player = new Player(this);

            //初始化起始舞台(切场景地图时更换为新的舞台)
            //stage = new Stage1(this);

            #region 测试舞台
            //stage = new TestStage1(this); //直接测试大量数字特效
            stage = new TestStage2(this); //正常打怪测试
            #endregion
        }
        void Update()
        {
            //处理玩家输入
            inputActions.HandlePlayerInput();

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
            //if (minimapEnabled)
            //{
            //    //若游戏逻辑被运行且次数是偶数那么启用小地图镜头
            //    minimapCamera.enabled = timeBak != time && (time & 1) == 0; //位运算,功能等价(time % 2) == 0
            //}
            #endregion

            //同步绘制
            stage.Draw();
        }

        /// <summary>
        /// 绘制编辑器场景对象
        /// </summary>
        void OnDrawGizmos()
        {
            //验证是否有舞台,有则绘制无则返回
            if (stage == null) return;
            stage.DrawGizmos();
        }
        /// <summary>
        /// 摧毁舞台对象和对象池资源
        /// </summary>
        void OnDestroy()
        {
            stage.Destroy();
            GO.Destroy();
        }
        /// <summary>
        /// 设置舞台.
        /// </summary>
        /// <param name="newStage"></param>
        /// <param name="oldDestroy">默认true会摧毁当前舞台,可设置false以保留/param>
        public void SetStage(Stage newStage, bool oldDestroy = true)
        {
            if (oldDestroy) stage.Destroy();//清理旧舞台
            stage = newStage;//设置新的舞台
        }
        /// <summary>
        /// 启禁用小地图(可间歇开关minimapCamera来提升性能 )
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

        public void InitCamera()
        {
            if (Camera.main != null)
            {
                if (CPEngine.horizontalMode)
                {
                    //2D横板模式用正交投影
                    Camera.main.orthographic = true;
                    Camera.main.orthographicSize = Camera.main.orthographicSize * designWidthToCameraRatio;
                    //Debug.Log("[horizontalMode]正交镜头:摄像机默认正交尺寸=" + Camera.main.orthographicSize);
                    Camera.main.gameObject.transform.position = new Vector3(0, 0, -20);
                }
                else if (CPEngine.singleLayerTerrainMode)
                {
                    //3D单层地形模式用正交投影
                    Camera.main.orthographic = true;
                    Camera.main.orthographicSize = Camera.main.orthographicSize * designWidthToCameraRatio;
                    Debug.Log("[CPEngine.singleLayerTerrainMode]正交镜头:摄像机默认正交尺寸 = " + Camera.main.orthographicSize);
                    Camera.main.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0); //原横板模式设计的摄像机绕X轴顺时针转90度以俯视X-Z平面
                    Camera.main.gameObject.transform.position = new Vector3(0, 20, 0);
                }
                else
                {
                    Debug.LogError("SpriteSpace框架仅支持2D横板模式（X-Y平面）、3D单层地形模式（X-Z平面）");

                    ////正常3D模式的镜头应另行支持鼠标旋转屏
                    //Camera.main.gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
                    //Camera.main.gameObject.transform.position = new Vector3(0, 20, 0);
                    //Camera.main.orthographic = false;
                    ////if (designWidthToCameraRatio != 1f)
                    ////{
                    ////    //3D透视投影用到的是视野大小,若设计分辨率到摄像头坐标的转换系数不为1则调整摄像机的视野大小
                    ////    Camera.main.fieldOfView = Camera.main.fieldOfView * designWidthToCameraRatio;
                    ////}
                    //Debug.Log("[正常3D模式]透视镜头:摄像机默认视野大小=" + Camera.main.fieldOfView);
                }
            }
            else
            {
                Debug.LogError("没有找到主摄像机！");
            }
        }
    }
}

//利用反射来读取怪物配置
//var st = typeof(Scene); //获取类型信息
//获取私有方法,指定BindingFlags
//MethodInfo privateMethod = type.GetMethod("方法名",BindingFlags.NonPublic | BindingFlags.Instance);
//设置方法可访问（如需）
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