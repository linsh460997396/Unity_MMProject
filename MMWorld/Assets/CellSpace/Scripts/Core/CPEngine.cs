using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using MetalMaxSystem.Unity;

//网络功能存在旧版UNet方法（Obsolete黄字警告），建议使用升级后的
namespace CellSpace
{
    #region 枚举

    /// <summary>
    /// 单元的6个面
    /// </summary>
    public enum Facing
    {
        /// <summary>
        /// 上面
        /// </summary>
        up,
        /// <summary>
        /// 下面
        /// </summary>
        down,
        /// <summary>
        /// 右面
        /// </summary>
        right,
        /// <summary>
        /// 左面
        /// </summary>
        left,
        /// <summary>
        /// 前面
        /// </summary>
        forward,
        /// <summary>
        /// 后面
        /// </summary>
        back
    }

    /// <summary>
    /// 朝向
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// 朝上
        /// </summary>
        up,
        /// <summary>
        /// 朝下
        /// </summary>
        down,
        /// <summary>
        /// 朝右
        /// </summary>
        right,
        /// <summary>
        /// 朝左
        /// </summary>
        left,
        /// <summary>
        /// 朝前
        /// </summary>
        forward,
        /// <summary>
        /// 朝后
        /// </summary>
        back
    }

    /// <summary>
    /// 透明度
    /// </summary>
    public enum Transparency
    {
        /// <summary>
        /// 实心
        /// </summary>
        solid,
        /// <summary>
        /// 半透明
        /// </summary>
        semiTransparent,
        /// <summary>
        /// 透明
        /// </summary>
        transparent
    }

    /// <summary>
    /// 碰撞类型
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// 立方体
        /// </summary>
        cube,
        /// <summary>
        /// 网格
        /// </summary>
        mesh,
        /// <summary>
        /// 无
        /// </summary>
        none
    }

    #endregion

    /// <summary>
    /// 存储全局引擎设置，并提供一些静态功能用于数据转换等。组件用法：Unity中随便新建一个空对象“CPEngine”，把脚本拖到组件位置即挂载（Unity要求一个cs文件只能一个类，且类名须与文件名一致）
    /// Start、Update生命周期方法是Unity控制脚本执行的关键，即使存在Awake但上述方法不存在时在编辑器界面不提供组件启禁用选项
    /// </summary>
    public class CPEngine : MonoBehaviour
    {
        //私有或静态字段不被自动序列化到Inspetor，想要被序列化请使用[SerializeField]特性，想不被序列化请使用[NonSerialized]特性

        #region 字段、属性方法（I前缀的变量表示接口变量，可被覆写）

        // CellSpaceEngine的每个静态变量都有一个非静态的等价物。非静态变量的名称与静态变量相同，只是在开头用小写的L，使用这些变量是为了能够在Unity中编辑这些变量，包括在引擎设置窗口中。
        // 在编辑器的Awake功能中，非静态变量被应用于它们的静态对应(通过场景中的Engine游戏对象)，所以在运行时改变非静态变量不会产生任何影响。

        //OP结构（含静态声明的共享对象池），可存储预制体实例化后的GameObject
        public static OP[] PrefabOPs;
        /// <summary>
        /// 纹理ID字符列表，0为大地图（第2材质），1~239为小地图（第3材质）,240为龙珠世界地图（第4材质），一共241个
        /// </summary>
        [NonSerialized] public static List<string>[] mapContents = new List<string>[241];
        /// <summary>
        /// 纹理ID列表，0为大地图（第2材质），1~239为小地图（第3材质）,240为龙珠世界地图（第4材质），一共241个
        /// </summary>
        [NonSerialized] public static List<ushort>[] mapIDs = new List<ushort>[241];
        /// <summary>
        /// 存储场景图片宽度信息的列表数组，供地图创建函数使用
        /// </summary>
        [NonSerialized] public static List<ushort> mapWidths = new List<ushort>();
        /// <summary>
        /// 存储默认碰撞类型为Cube的纹理ID列表数组，数组索引0是人1是车。
        /// 碰撞条件：单元纹理ID在人车默认碰撞列表 且 该场景单元ID可进入字段=假（默认都是假，加载场景后可针对如小地图屋子进行设置真）。
        /// 大地图上许多屋子都是可以踩的，只有小地图上的屋子入口能不能踩需要额外状态判断。
        /// </summary>
        [NonSerialized]
        public static List<ushort>[] cubeColliderIDs = new List<ushort>[]
        {
            new List<ushort>(),
            new List<ushort>()
        };

        /// <summary>
        /// 当前活动世界的名称（对应存储世界数据的文件夹）
        /// </summary>
        public static string WorldName;

        /// <summary>
        /// 世界数据文件的路径（默认路径为/application_root/world_name/），可通过直接编辑Engine脚本内的UpdateWorldPath私有函数来更改世界路径。
        /// </summary>
        public static string WorldPath;

        /// <summary>
        /// 单元路径（Unity项目中块预制体的路径），这是块编辑器用来查找块的。
        /// </summary>
        public static string BlocksPath;

        /// <summary>
        /// 当前活动世界的种子，可用于程序地形生成，种子存储在世界数据文件夹中
        /// </summary>
        public static int WorldSeed;

        /// <summary>
        /// [GUI界面输入]世界数据文件的路径（默认路径为/application_root/world_name/），可通过直接编辑Engine脚本内的UpdateWorldPath私有函数来更改世界路径。
        /// </summary>
        public string lWorldName = "DefaultCellSpace";

        /// <summary>
        /// [GUI界面输入]单元路径（Unity项目中块预制体的路径），这是块编辑器用来查找块的。
        /// </summary>
        public string lBlocksPath;

        /// <summary>
        /// 三维立体像素块（单元）预制体，在块编辑器中定义，数组索引对应于块的体素ID（单元预制体种类）。
        /// </summary>
        public static GameObject[] Blocks;
        /// <summary>
        /// [GUI界面输入]三维立体像素块（单元）预制体，在块编辑器中定义，数组索引对应于块的体素ID（单元预制体种类）。
        /// </summary>
        public GameObject[] lBlocks;

        // 团块创建设置（团块就是这些单元的集合、由小块堆组成的大块）

        /// <summary>
        /// 衍生团块的最大正负垂直块索引，即团块自动创建时高度范围限值（如果是3，表示原始团块上下可以产生3个团块）
        /// </summary>
        public static int HeightRange;
        /// <summary>
        /// 从原点到生成团块的水平距离(以团块为单位)，是团块自动创建时的距离限制（如果是8，则始终在玩家原始团块周围保证有8范围的团块）
        /// </summary>
        public static int ChunkSpawnDistance;
        /// <summary>
        /// 一个团块的边长(以单元为单位)，即团块自动创建时的正方形单边尺寸边长（如果是16，则团块由16^3个单元组成）
        /// </summary>
        public static int ChunkSideLength;
        /// <summary>
        /// 团块自动摧毁时的判断距离（如果是3，则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁，摧毁时会存入档案以便玩家走过来时读取生成）
        /// </summary>
        public static int ChunkDespawnDistance;

        /// <summary>
        /// 地形高度（世界坐标）
        /// </summary>
        private static int _terrainHeight;
        /// <summary>
        /// 地形高度（世界坐标）
        /// </summary>
        public static int TerrainHeight
        {
            get
            {
                return _terrainHeight;
            }

            set
            {
                _terrainHeight = value;
            }
        }

        // 团块创建设置，GUI界面输入

        /// <summary>
        /// [GUI界面输入]衍生团块的最大正负垂直块索引，即团块自动创建时高度范围限值（如果是3，表示高度Y最大是3个团块范围，如团块是16边长那么Y最大高度48）
        /// </summary>
        public int lHeightRange;
        /// <summary>
        /// [GUI界面输入]从原点到生成团块的水平距离(以团块为单位)，是团块自动创建时的距离限制（如果是8，则始终在玩家周围保证有8范围的团块）
        /// </summary>
        public int lChunkSpawnDistance;
        /// <summary>
        /// [GUI界面输入]一个团块的边长(以单元为单位)，即团块自动创建时的正方形单边尺寸边长（如果是16，则团块由16*16*16个单元组成）
        /// </summary>
        public int lChunkSideLength;
        /// <summary>
        /// [GUI界面输入]团块自动摧毁时的判断距离（如果是3，则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁，摧毁时会存入档案以便玩家走过来时读取生成）
        /// </summary>
        public int lChunkDespawnDistance;

        // 纹理设置

        /// <summary>
        /// 纹理表边长/纹理单元边长的倍率（用于计算单元纹理划区大小），形象说明的话相当于每个精灵在整张图片中的比例的倒数，填8则X方向均分8个纹理单元
        /// </summary>
        public static float[] TextureUnitX;
        /// <summary>
        /// 纹理表边长/纹理单元边长的倍率（用于计算单元纹理划区大小），形象说明的话相当于每个精灵在整张图片中的比例的倒数，填8则Y方向均分8个纹理单元
        /// </summary>
        public static float[] TextureUnitY;
        /// <summary>
        /// 纹理表上纹理之间的填充，作为单个单元纹理大小的一小部分。
        /// 即每个纹理单元之间填充缝的大小（UV划区时向内缩进比例），填充以避免取到别的纹理单元
        /// </summary>
        public static float TexturePadX;
        /// <summary>
        /// 纹理表上纹理之间的填充，作为单个单元纹理大小的一小部分。
        /// 即每个纹理单元之间填充缝的大小（UV划区时向内缩进比例），填充以避免取到别的纹理单元
        /// </summary>
        public static float TexturePadY;

        // 纹理设置，GUI界面输入

        /// <summary>
        /// [GUI界面输入]纹理表边长/纹理单元边长的倍率（用于计算单元纹理划区大小），形象说明的话相当于每个精灵在整张图片中的比例的倒数，填8则X方向均分8个纹理单元
        /// </summary>
        public float[] lTextureUnitX;
        /// <summary>
        /// [GUI界面输入]纹理表边长/纹理单元边长的倍率（用于计算单元纹理划区大小），形象说明的话相当于每个精灵在整张图片中的比例的倒数，填8则X方向均分8个纹理单元
        /// </summary>
        public float[] lTextureUnitY;
        /// <summary>
        /// [GUI界面输入]纹理表上纹理之间的填充，作为单个单元纹理大小的一小部分。
        /// 即每个纹理单元之间填充缝的大小（UV划区时向内缩进比例），填充以避免取到别的纹理单元
        /// </summary>
        public float lTexturePadX;
        /// <summary>
        /// [GUI界面输入]纹理表上纹理之间的填充，作为单个单元纹理大小的一小部分。
        /// 即每个纹理单元之间填充缝的大小（UV划区时向内缩进比例），填充以避免取到别的纹理单元
        /// </summary>
        public float lTexturePadY;

        // 平台设置

        /// <summary>
        /// 目标（预期）帧率，并非实际，如果计时器记录每帧处理用时超过它，则可以将动作放在下一帧继续步进，防止卡在这一帧（让外围团块慢慢生成），所以这个值并非期望越高越好而是应尽量贴近实际。
        /// </summary>
        public static int TargetFPS;
        /// <summary>
        /// 每帧的团块保存上限（决定每帧保存团块的最大处理速率），用于跟ChunkManager的当前帧的团块已保存数量SavesThisFrame进行比对
        /// </summary>
        public static int MaxChunkSaves;
        /// <summary>
        /// 团块数据请求上限：每个客户端一次可以在服务器中排队的最大团块数据请求数(0=无限制)。如客户端生成数据块的速度太快且你发现你的服务器无法跟上数据请求的速度，那么降低这个限制。
        /// </summary>
        public static int MaxChunkDataRequests;

        // 平台设置，GUI界面输入

        /// <summary>
        /// [GUI界面输入]目标（预期）帧率，并非实际，如果计时器记录每帧处理用时超过它，则可以将动作放在下一帧继续步进，防止卡在这一帧（让外围团块慢慢生成），所以这个值并非期望越高越好而是应尽量贴近实际。
        /// </summary>
        public int lTargetFPS;
        /// <summary>
        /// [GUI界面输入]团块保存上限（决定每帧保存团块的最大处理速率）
        /// </summary>
        public int lMaxChunkSaves;
        /// <summary>
        /// [GUI界面输入]团块数据请求上限：每个客户端一次可以在服务器中排队的最大团块数据请求数(0=无限制)。
        /// 如客户端生成数据块的速度太快且你发现你的服务器无法跟上数据请求的速度，那么降低这个限制。
        /// </summary>
        public int lMaxChunkDataRequests;

        // 全局设置
        /// <summary>
        /// Unet模式，启用状态为true将使用旧版Unet网络功能（不推荐），反之使用新版NetCode
        /// </summary>
        public static bool UnetMode = true;
        /// <summary>
        /// 横版模式，启用时地面使用XY平面坐标系（默认正高度Z-），否则采用XZ平面（默认正高度Y+）。横版模式默认取消了Z轴延伸，该轴只留最大1体素块(以左下为原点)插入在Z=0的（pixelX,pixelY）索引点。
        /// 可在CellChunkMeshCreator类中修改具体要显示的体素块的面，默认仅创建体素块的back面（透过屏幕直接看到的体素块背面）。
        /// </summary>
        public static bool HorizontalMode = true;
        /// <summary>
        /// 多维横版属性。开启后，横版模式下可将体素块的前面永久创建出来（否则不创建），但上下左右仍要CheckAdjacent来判断是否创建；若关闭，横版模式下仅创建体素块的back面。
        /// </summary>
        public static bool MutiHorizontal = false;
        /// <summary>
        /// 保持地形高度（避免创建地貌时因噪声函数产生高度上的起伏，HorizontalMode决定默认正高度Y+还是Z-）。开启后请结合TerrainHeight确定地表高度。
        /// 注意：创建地形的CPTerrainGenerator示范类中可存在属性覆盖，在此设置并不作为最终值，请按需设计。
        /// </summary>
        public static bool KeepTerrainHeight = false;
        /// <summary>
        /// 创建团块时，即使创建范围使得任何团块索引都无效，至少在(0,0,0)插入一个团块
        /// </summary>
        public static bool KeepOneChunk = true;
        /// <summary>
        /// 单元的侧面可见（前提是没有与它们接壤的团块实例）。
        /// 启用后对于邻团不存在情况CheckAdjacent()函数总是返回真而不用对比朝向是否朝上才返回真。
        /// 对于邻团存在情况则考虑相邻体素块及执行此检查的体素块的透明度：
        /// 1）此体素透明，若相邻体素也透明返回假（禁止在一个完全透明体素块旁边绘制另一个透明块），否则真（允许在实体或半透明旁边画一个透明的块）；
        /// 2）此体素非完全透明情况，若相邻体素是固态返回假（禁止在实体体素块旁边画实体或半透明体素块），否则真（允许在透明和半透明体素块旁绘制一个实心或半透明体素块）。
        /// </summary>
        public static bool ShowBorderFaces;
        /// <summary>
        /// 产生碰撞体（为false则团块将不会生成任何Colliders）
        /// </summary>
        public static bool GenerateColliders;
        /// <summary>
        /// 发送镜头注视事件（如果为true, CameraEventsSender组件将把事件发送到主摄像机视场中心指着的单元）
        /// </summary>
        public static bool SendCameraLookEvents;
        /// <summary>
        /// 发送鼠标指针事件（如果为true, CameraEventsSender组件将将把事件发送到当前鼠标光标指着的单元）
        /// </summary>
        public static bool SendCursorEvents;
        /// <summary>
        /// 允许多人玩家（团块将从服务器请求单元数据而不是从硬盘生成或加载，另外Voxel.ChangeBlock、Cell.PlaceBlock和Voxel.DestroyBlock会将单元变化发送到服务器以便重新分发给其他连接的玩家）
        /// </summary>
        public static bool EnableMultiplayer;
        /// <summary>
        /// 用于确定网络同步轨道位置的处理方式，服务器检查玩家的位置以确定是否需要将单元更改发送给该玩家，客户端则会通过ChunkLoader脚本向服务器发送一个玩家位置更新。
        /// 在多人游戏中，通常需要将物体的位置同步到其他客户端。轨道位置是指物体沿着一条路径或轨道移动时所处的位置。如果将MultiplayerTrackPosition字段设置为true，则表示该物体的位置将在网络上进行同步，且每个客户端都将跟踪该物体的轨道位置。
        /// 如果将MultiplayerTrackPosition字段设置为false，则表示该物体的位置不会在网络上进行同步，而客户端将不会跟踪其轨道位置。
        /// 在具有大量移动物体的多人游戏中，使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能。例如，如果某个物体在场景中静止不动，则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步。
        /// </summary>
        public static bool MultiplayerTrackPosition;
        /// <summary>
        /// 保存单元数据。为false则团块将不会加载或保存单元数据，反之在生成团块时总是会生成新的数据。
        /// </summary>
        public static bool SaveCellData;
        /// <summary>
        /// 产生网格
        /// </summary>
        public static bool GenerateMeshes;

        // [GUI界面输入]全局设置

        /// <summary>
        /// [GUI界面输入]单元的侧面可见（前提是没有与它们接壤的团块实例）
        /// </summary>
        public bool lShowBorderFaces;
        /// <summary>
        /// [GUI界面输入]产生碰撞体（为false则团块将不会生成任何Colliders）
        /// </summary>
        public bool lGenerateColliders;
        /// <summary>
        /// [GUI界面输入]发送镜头注视事件（如果为true, CameraEventsSender组件将把事件发送到主摄像机视场中心指着的单元）
        /// </summary>
        public bool lSendCameraLookEvents;
        /// <summary>
        /// [GUI界面输入]发送鼠标指针事件（如果为true, CameraEventsSender组件将将把事件发送到当前鼠标光标指着的单元）
        /// </summary>
        public bool lSendCursorEvents;
        /// <summary>
        /// [GUI界面输入]允许多人玩家（团块将从服务器请求单元数据而不是从硬盘生成或加载，另外Voxel.ChangeBlock、Cell.PlaceBlock和Voxel.DestroyBlock会将单元变化发送到服务器以便重新分发给其他连接的玩家）
        /// </summary>
        public bool lEnableMultiplayer;
        /// <summary>
        /// [GUI界面输入]用于确定网络同步轨道位置的处理方式，服务器检查玩家的位置以确定是否需要将单元更改发送给该玩家，客户端则会通过ChunkLoader脚本向服务器发送一个玩家位置更新。
        /// 在多人游戏中，通常需要将物体的位置同步到其他客户端。轨道位置是指物体沿着一条路径或轨道移动时所处的位置。如果将MultiplayerTrackPosition字段设置为true，则表示该物体的位置将在网络上进行同步，且每个客户端都将跟踪该物体的轨道位置。
        /// 如果将MultiplayerTrackPosition字段设置为false，则表示该物体的位置不会在网络上进行同步，而客户端将不会跟踪其轨道位置。
        /// 在具有大量移动物体的多人游戏中，使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能。例如，如果某个物体在场景中静止不动，则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步。
        /// </summary>
        public bool lMultiplayerTrackPosition;
        /// <summary>
        /// [GUI界面输入]保存单元数据。为false则团块将不会加载或保存单元数据，反之在生成团块时总是会生成新的数据。
        /// </summary>
        public bool lSaveCellData;
        /// <summary>
        /// [GUI界面输入]产生网格
        /// </summary>
        public bool lGenerateMeshes;

        /// <summary>
        /// 团块超时：如一团块通过ChunkManager.SpawnChunk动作创建，但在ChunkTimeout这段时间内没有被访问，它将被销毁(在保存它的单元数据后)。
        /// 来自客户端的单元数据请求和团块中的单元变化将重置计时器。当值为0将禁用此功能。
        /// </summary>
        public static float ChunkTimeout;
        /// <summary>
        /// 团块超时：如一团块通过ChunkManager.SpawnChunk动作创建，但在ChunkTimeout这段时间内没有被访问，它将被销毁(在保存它的单元数据后)。
        /// 来自客户端的单元数据请求和团块中的单元变化将重置计时器。当值为0将禁用此功能。
        /// </summary>
        public float lChunkTimeout;
        /// <summary>
        /// 允许团块超时（若Engine.ChunkTimeout>0则此变量会自动设为true）
        /// </summary>
        public static bool EnableChunkTimeout;

        //其他

        /// <summary>
        /// 团块边长的平方（用于定义容器大小）
        /// </summary>
        public static int SquaredSideLength;
        /// <summary>
        /// 处理网络通信和同步的游戏物体对象
        /// </summary>
        public static GameObject Network;
        /// <summary>
        /// 引擎类型实例
        /// </summary>
        public static CPEngine EngineInstance;
        /// <summary>
        /// 团块管理器实例
        /// </summary>
        public static CellChunkManager ChunkManagerInstance;
        /// <summary>
        /// 团块预制体的大小（缩放比例）
        /// </summary>
        public static Vector3 ChunkScale;
        /// <summary>
        /// 引擎初始化状态
        /// </summary>
        public static bool Initialized;

        #endregion

        // ==== initialization ====

        /// <summary>
        /// 创建预制体实例。因GUI手填地块预制体太慢，这里用脚本批处理创建预制体实例化后的GameObject。
        /// 虽然它们在创建后会立即出现在场景，但因要重复利用所以创建后退入对象池会失活隐藏（请在需要时取出并激活GameObject）。
        /// 注意本函数创建的预制体实例会填充内部调用的Blocks数组。
        /// </summary>
        /// <param name="cellID"></param>
        /// <param name="vector"></param>
        /// <param name="subMeshIndex"></param>
        /// <param name="torf">默认true在游戏物体创建后退回OP对象池（待使用时取出），反之创建后的游戏物体在场景中是激活状态</param>
        static void CreatePrefab(ushort cellID, ushort subMeshIndex, Vector2 vector, bool torf = true)
        {
            string name = "cell_" + cellID;
            CPEngine.PrefabOPs[cellID].gameObject = new GameObject(name);
            Cell cell = CPEngine.PrefabOPs[cellID].gameObject.AddComponent<Cell>();//在GameObject上添加单元组件对象
            cell.VName = name;
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = vector;
            cell.VTransparency = Transparency.solid;
            //非GUI编辑器填写而是采用手写录入的默认按cube碰撞类型
            cell.VColliderType = ColliderType.cube;
            //根据纹理编号来设置单元碰撞类型
            //if (cubeColliderIDs.Contains(cellID))
            //{//有碰撞类型
            //    cell.VColliderType = ColliderType.cube;
            //}
            //else
            //{//无碰撞类型
            //    cell.VColliderType = ColliderType.none;
            //}
            cell.VSubmeshIndex = subMeshIndex; //大地图纹理在索引1的材质里
            cell.VRotation = MeshRotation.none;
            Blocks[cellID] = PrefabOPs[cellID].gameObject;//填充内部调用的Blocks数组
            OP.pool.Push(PrefabOPs[cellID]);//将OP上的游戏物体对象退回栈，待使用时取出
        }

        /// <summary>
        /// 批创建预制体实例。自动识别网格渲染器对应材质主纹理并按行列分割UV，批转化为预制体实例并存入CPEngine.PrefabOPs数组
        /// </summary>
        /// <param name="cellID">特征图的第一个CellID</param>
        /// <param name="endID">特征图的最后一个CellID</param>
        /// <param name="textureRow">Y方向行数</param>
        /// <param name="textureCol">X方向列数</param>
        /// <param name="subMeshIndex">网格渲染器的材质索引</param>
        /// <param name="torf">默认true当lBlocks为null时才进行CreatePrefab（若lBlocks已有GUI填入的地块预制体则直接使用），否则总是CreatePrefab（不使用GUI填的地块预制体）</param>
        /// <param name="XIncrement">默认true则UV划区时（左下为原点）先以X方向自增，若为flase则先以Y方向自增</param>
        static void CreateTexPrefabBatch(ushort cellID, ushort endID, ushort subMeshIndex, ushort textureCol, ushort textureRow, bool torf = true, bool XIncrement = true)
        {
            ushort index = cellID;
            ushort x = 0; //当前X坐标
            ushort y = 0; //当前Y坐标
            if (XIncrement)
            {
                //遍历所有切片，注意检查索引是否越界
                for (ushort row = 0; row < textureRow; row++)
                {//Y自增时，X重置为0
                    x = 0;
                    for (ushort col = 0; col < textureCol; col++)
                    {
                        //检查索引是否越界
                        if (index >= PrefabOPs.Length)
                        {
                            Debug.LogError("Index out of range!");
                            return; //返回或处理越界情况
                        }
                        if (torf == false || (torf == true && Blocks[index] == null))
                        {//torf为false时总是CreatePrefab，为true时仅当lBlocks无值才进行CreatePrefab
                            CreatePrefab(index, subMeshIndex, new Vector2(x, y));
                        }
                        else
                        {//若已有GUI填入的地块预制体则直接使用
                            PrefabOPs[index].gameObject = Instantiate(GetCellGameObject(index)); //OP对象绑定各自预制体实例化后的GameObject（由于是GUI填入的，Cell参数会自动填充）
                        }
                        index++;
                        //下个处理ID超过特征图最后CellID时直接跳出函数
                        if (index > endID) { return; }
                        x++;
                    }
                    y++;
                }
            }
            else
            {
                //遍历所有切片，注意检查索引是否越界
                for (ushort col = 0; col < textureCol; col++)
                {//X自增时，Y重置为0
                    y = 0;
                    for (ushort row = 0; row < textureRow; row++)
                    {
                        //检查索引是否越界
                        if (index >= PrefabOPs.Length)
                        {
                            Debug.LogError("Index out of range!");
                            return; //返回或处理越界情况
                        }
                        if (torf == false || (torf == true && Blocks[index] == null))
                        {//torf为false时总是CreatePrefab，为true时仅当lBlocks无值才进行CreatePrefab
                            CreatePrefab(index, subMeshIndex, new Vector2(x, y));
                        }
                        else
                        {//若已有GUI填入的地块预制体则直接使用
                            PrefabOPs[index].gameObject = Instantiate(GetCellGameObject(index)); //OP对象绑定各自预制体实例化后的GameObject（由于是GUI填入的，Cell参数会自动填充）
                        }
                        index++;
                        //下个处理ID超过特征图最后CellID时直接跳出函数
                        if (index > endID) { return; }
                        y++;
                    }
                    x++;
                }
            }
        }

        /// <summary>
        /// 读各场景纹理ID文本（用于地图自动绘制）
        /// </summary>
        static void LoadTXT()
        {
            //读取ID文本前初始化数组中的每个List元素（用于存放场景纹理ID）
            for (int i = 0; i < mapContents.Length; i++)
            {
                mapContents[i] = new List<string>();
            }
            for (int i = 0; i < mapIDs.Length; i++)
            {
                mapIDs[i] = new List<ushort>();
            }

            //0，重装机兵大地图
            TextAsset textAsset = Resources.Load<TextAsset>("MapIndex/World");
            string tempContent = textAsset.text;
            string[] fields = tempContent.Split(',');
            mapContents[0].AddRange(fields); //分割好的世界纹理ID放到数组0
                                             //string combinedString = string.Join(",", mapContents[0]);
                                             //Debug.Log(combinedString);
                                             // 将字符串转换为ushort并存储到mapIDs数组中
            for (int i = 0; i < fields.Length; i++)
            {
                mapIDs[0].Add(ushort.Parse(fields[i]));
            }
            //string joinedString = string.Join(",", mapIDs[0].Select(pixelX => pixelX.ToString()));
            //Debug.Log(joinedString); //Debug.Log(mapIDs[0].Count);

            //1~239（对应重装机兵小地图0~238.txt）
            string filePath;
            for (int i = 0; i <= 238; i++)
            {
                filePath = "MapIndex/" + i.ToString();//使用Resources方法的路径不需要文件后缀
                textAsset = Resources.Load<TextAsset>(filePath);
                tempContent = textAsset.text;
                fields = tempContent.Split(',');
                mapContents[i + 1].AddRange(fields); //239个小地图场景纹理ID存放在数组索引1~239
                                                     // 将字符串转换为ushort并存储到mapIDs数组中
                for (int j = 0; j < fields.Length; j++)
                {
                    // 使用ushort.Parse来转换字符串到ushort
                    mapIDs[i + 1].Add(ushort.Parse(fields[j]));
                }
            }
            //小地图239个场景图片宽度信息文本
            textAsset = Resources.Load<TextAsset>("MapIndex/Width");
            tempContent = textAsset.text;
            fields = tempContent.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                mapWidths.Add(ushort.Parse(fields[i])); //首次添加用Add而不是赋值动作
            }

            //240，龙珠大地图
            textAsset = Resources.Load<TextAsset>("MapIndex/LZWorld");
            tempContent = textAsset.text;
            fields = tempContent.Split(',');
            mapContents[240].AddRange(fields); //分割好的世界纹理ID放到数组0
                                               //string combinedString = string.Join(",", mapContents[0]);
                                               //Debug.Log(combinedString);
                                               // 将字符串转换为ushort并存储到mapIDs数组中
            for (int i = 0; i < fields.Length; i++)
            {
                mapIDs[240].Add(ushort.Parse(fields[i]));
            }
        }

        /// <summary>
        /// 更新块（必须在使用前完整刷新地块数据）
        /// </summary>
        static void BlocksRefresh()
        {
            //Unity不能直接判断一个未实例化的预制体上是否附加了特定的组件，因为未实例化的预制体本身只是一个模板（用于创建具体的GameObject场景实例）
            //预制体上的组件只有在实例化后才会真正存在并可被访问，此处我们将频繁使用的预制体实例化后存入对象池
            //根据材质纹理数量进行规划
            ushort num0 = 11; //材质[0]主纹理默认有11个地块纹理，是GUI界面手动拖入的Cell预制体数量，这些已有填入的预制体，将在实例化后存入对象池
            //接下来是其他材质主纹理所划分的地块纹理数量（因数量巨多，不再手动从GUI拖入）
            ushort num1 = (ushort)(num0 + 152);//163 
            ushort num2 = (ushort)(num1 + 1360);//1523
            ushort num3 = (ushort)(num2 + 892);//2415（测试用的龙珠大地图特征纹理）

            PrefabOPs = new OP[Blocks.Length]; //请在GUI界面lBlocks预填数组容量，以便此处自动创建同样Blocks.Length个OP对象，所有OP对象共享对象池
            OP.pool = new(Blocks.Length); //对象池预填充，容量为Blocks.Length（即便Push数量超过初始容量，栈也会自动扩容）

            //手动拖入的Cell预制体数量是num0个：cell_0~10（材质[0]主纹理上的）
            CreateTexPrefabBatch(0, (ushort)(num0 - 1), 0, (ushort)TextureUnitX[0], (ushort)TextureUnitY[0]);

            //额外添加152个大地图纹理：cell_11~162（材质[1]主纹理上的）
            //↓大地图19行8列会自动按起始和结尾参数转换出152个预制体实例，函数会绑定它们到OP对象gameObject字段
            CreateTexPrefabBatch(num0, (ushort)(num1 - 1), 1, (ushort)TextureUnitX[1], (ushort)TextureUnitY[1]);

            //额外添加1360个小地图纹理：cell_163~1522（材质[2]主纹理上的）
            //↓小地图170行8列会自动转换出1360个预制体实例
            CreateTexPrefabBatch(num1, (ushort)(num2 - 1), 2, (ushort)TextureUnitX[2], (ushort)TextureUnitY[2]);

            //额外添加892小地图纹理：cell_1523~2414（材质[3]主纹理上的），包括CellID_0共2415个Block数组元素
            //↓龙珠地图目前是临时测试用的，50行18列会自动转换出892个预制体实例
            CreateTexPrefabBatch(num2, (ushort)(num3 - 1), 3, (ushort)TextureUnitX[3], (ushort)TextureUnitY[3]);

            //读取重装机兵全场景纹理ID文本
            LoadTXT();
        }

        /// <summary>
        /// 获取体素ID对应材质渲染器上的材质索引
        /// </summary>
        /// <param name="cellId"></param>
        /// <returns></returns>
        public static ushort GetSubMeshIndex(ushort cellId)
        {
            ushort torf = 0;
            if (cellId >= 11)
            {
                if (cellId < 163) { torf = 1; }
                else if (cellId < 1523) { torf = 2; }
                else if (cellId < 2415) { torf = 3; }
                else { Debug.Log("纹理ID超出材质子网格索引上限！"); }
            }
            return torf;
        }

        //Awake方法在脚本实例被加载时被调用，通常发生在游戏对象被创建或脚本被添加到游戏对象上
        public void Awake()
        {
            //本函数负责引擎初始化

            //↓录制默认碰撞（碰撞条件：单元纹理ID在人车默认碰撞列表 且 该场景单元ID可进入字段=假（默认都是假，加载场景后可针对如小地图屋子进行设置真））

            //人的默认碰撞类型
            for (ushort i = 11; i <= 20; i++) { cubeColliderIDs[0].Add(i); }//大地图左下第一个纹理从11编号起算
            for (ushort i = 23; i <= 24; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 23; i <= 24; i++) { cubeColliderIDs[0].Add(i); }
            cubeColliderIDs[0].Add(29);//栅栏
            for (ushort i = 34; i <= 37; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 39; i <= 46; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 48; i <= 53; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 55; i <= 57; i++) { cubeColliderIDs[0].Add(i); }
            cubeColliderIDs[0].Add(60);
            cubeColliderIDs[0].Add(62);
            cubeColliderIDs[0].Add(67);
            for (ushort i = 80; i <= 84; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 90; i <= 102; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 109; i <= 112; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 115; i <= 116; i++) { cubeColliderIDs[0].Add(i); }
            cubeColliderIDs[0].Add(118);
            for (ushort i = 121; i <= 124; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 151; i <= 155; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 159; i <= 162; i++) { cubeColliderIDs[0].Add(i); }
            //↑大地图碰撞录制完毕

            //车的默认碰撞类型
            for (ushort i = 11; i <= 20; i++) { cubeColliderIDs[1].Add(i); }//山河
            for (ushort i = 23; i <= 25; i++) { cubeColliderIDs[1].Add(i); }//多了小树
            cubeColliderIDs[1].Add(29);//栅栏
            for (ushort i = 34; i <= 37; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 48; i <= 53; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 55; i <= 57; i++) { cubeColliderIDs[1].Add(i); }
            cubeColliderIDs[1].Add(60);
            cubeColliderIDs[1].Add(62);
            cubeColliderIDs[1].Add(67);
            cubeColliderIDs[1].Add(72); cubeColliderIDs[1].Add(73);
            for (ushort i = 80; i <= 84; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 90; i <= 102; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 109; i <= 112; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 115; i <= 116; i++) { cubeColliderIDs[1].Add(i); }
            cubeColliderIDs[1].Add(118);
            for (ushort i = 121; i <= 124; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 151; i <= 155; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 159; i <= 162; i++) { cubeColliderIDs[1].Add(i); }
            //↑大地图碰撞录制完毕

            EngineInstance = this; //this关键字引用了当前类的一个实例，但它不能用在静态字段的初始化中，所以写在这
            //获取对象上的团块管理器组件实例（这里指名为"CellChunkManager"的脚本类型组件实例化后的对象）
            ChunkManagerInstance = GetComponent<CellChunkManager>();
            //读取GUI界面输入里填的世界名称
            WorldName = lWorldName;
            //更新世界存档的路径
            UpdateWorldPath();

            #region 将GUI界面输入数据赋值给实际运作的字段属性

            BlocksPath = lBlocksPath;
            Blocks = lBlocks; //从GUI界面拖拽到Engine的Cell预制体（只拖拽了一部分，剩下太多了所以接下来用代码追加）

            TargetFPS = lTargetFPS;
            MaxChunkSaves = lMaxChunkSaves;
            MaxChunkDataRequests = lMaxChunkDataRequests;

            TextureUnitX = lTextureUnitX;
            TextureUnitY = lTextureUnitY;
            TexturePadX = lTexturePadX;
            TexturePadY = lTexturePadY;
            GenerateColliders = lGenerateColliders;
            ShowBorderFaces = lShowBorderFaces;
            EnableMultiplayer = lEnableMultiplayer;
            MultiplayerTrackPosition = lMultiplayerTrackPosition;
            SaveCellData = lSaveCellData;
            GenerateMeshes = lGenerateMeshes;

            ChunkSpawnDistance = lChunkSpawnDistance;
            HeightRange = lHeightRange;
            ChunkDespawnDistance = lChunkDespawnDistance;

            SendCameraLookEvents = lSendCameraLookEvents;
            SendCursorEvents = lSendCursorEvents;

            ChunkSideLength = lChunkSideLength;
            SquaredSideLength = lChunkSideLength * lChunkSideLength;

            #endregion

            BlocksRefresh();

            //建立已加载的区域组（字典<string, string[]>）
            CellChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
            //建立临时团块数据组（字典<string, string>）
            CellChunkDataFiles.TempChunkData = new Dictionary<string, string>();

            //如GUI界面输入的lChunkTimeout<= 0.00001，则不允许团块处理超时，否则允许超时并将lChunkTimeout赋值给游戏逻辑频繁互动用的属性字段
            if (lChunkTimeout <= 0.00001f)
            {
                EnableChunkTimeout = false;
            }
            else
            {
                EnableChunkTimeout = true;
                ChunkTimeout = lChunkTimeout;
            }

#if UNITY_WEBPLAYER
            //当前平台是WebPlayer，本地化存储应取消
            lSaveVoxelData = false;
            SaveVoxelData = false;
#else
            //当前平台不是WebPlayer
#endif

            //遮罩层设置

            //如果26层名不为空则输出警告
            if (LayerMask.LayerToName(26) != "" && LayerMask.LayerToName(26) != "UniblocksNoCollide")
            {
                Debug.LogWarning("CellSpace: Layer 26 is reserved for CellSpace, it is automatically set to ignore collision with all layers." +
                                 "第26层是为Uniblocks保留的，它被自动设置为忽略与所有图层的碰撞！");
            }
            for (int i = 0; i < 31; i++)
            {
                //Unity有32个可用的层0~31，此处设置第i~26之间的对象不发生碰撞（亦能穿过彼此而无碰撞事件）
                Physics.IgnoreLayerCollision(i, 26);
            }

            #region 检查团块

            //检查GUI界面输入里预定义的单元种类计数
            if (Blocks.Length < 1)
            {
                Debug.LogError("CellSpace: The blocks array is empty! Use the Block Editor to update the blocks array." +
                    "单元是空的！使用块编辑器来更新！");
                Debug.Break();
            }

            //检查第一个单元（空块）是否存在，如不存在或没有单元组件则报错
            if (Blocks[0] == null)
            {
                Debug.LogError("CellSpace: Cannot find the empty block prefab (id 0)!" +
                    "找不到空块预制体（id 0）！");
                Debug.Break();
            }
            else if (Blocks[0].GetComponent<Cell>() == null)
            {
                Debug.LogError("CellSpace: Empty block prefab (id 0) does not have the Cell component attached!" +
                    "空块预制体(id 0)没有单元组件");
                Debug.Break();
            }

            #endregion

            #region 检查设置

            //检查团块边长（至少为1才有效）
            if (ChunkSideLength < 1)
            {
                Debug.LogError("CellSpace: CellChunk side length must be greater than 0!" +
                    "团块边长必须大于0");
                Debug.Break(); //暂停编辑器运行
            }

            //如果团块生成距离<1则被置为0（不再生成），默认是8
            if (ChunkSpawnDistance < 1)
            {
                ChunkSpawnDistance = 0;
                if (KeepOneChunk == false)
                {
                    Debug.LogWarning("CellSpace: CellChunk spawn distance is 0." + "团块生成距离为0且KeepOneChunk=假，无法生成空间团块！");
                }
            }

            //如果高度范围小于0，则高度范围将被置为0，默认是3
            if (HeightRange < 0)
            {
                HeightRange = 0;
                Debug.LogWarning("CellSpace: CellChunk height range can'transform be a negative number! Setting chunk height range to 0." +
                    "团块高度范围不能是一个负数！已被重置为0");
            }

            //检查团块数据请求上限
            if (MaxChunkDataRequests < 0)
            {
                MaxChunkDataRequests = 0;
                Debug.LogWarning("CellSpace: Max chunk data requests can'transform be a negative number! Setting max chunk data requests to 0." +
                    "团块数据请求上限不能是负数！已被重置为0");
            }

            #endregion

            //检查材质
            GameObject chunkPrefab = GetComponent<CellChunkManager>().ChunkObject; //获取团块管理器中关联的团块物体对象作为团块预制体
            int materialCount = chunkPrefab.GetComponent<Renderer>().sharedMaterials.Length - 1; //（额外）材质计数=团块预制体渲染组件的共享材质数量-1

            //遍历配置里预定义的单元
            for (ushort i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i] != null)
                {
                    //获取单元
                    Cell cell = Blocks[i].GetComponent<Cell>();

                    //如果单元子网格索引<0则报错
                    if (cell.VSubmeshIndex < 0)
                    {
                        Debug.LogError("CellSpace: Cell " + i + " has a material index lower than 0! Material index must be 0 or greater." +
                            "单元的材质索引小于0！必须大于等于0");
                        Debug.Break();
                    }

                    //如单元子网格索引大于（额外）材质计数则报错（使用自定义的额外材质索引后没给它上材质）
                    if (cell.VSubmeshIndex > materialCount)
                    {
                        //单元使用了GUI界面输入中自定义材质索引，但团块预制体只有（额外）材质计数+1个材质附着，设置一个更低的材质索引或附着更多材质到团块预制体！
                        Debug.LogError("CellSpace: Cell " + i + " uses material index " + cell.VSubmeshIndex + ", but the chunk prefab only has " + (materialCount + 1) + " material(s) attached. Set a lower material index or attach more materials to the chunk prefab.");
                        Debug.Break();
                    }
                }
            }

            //质量配置，检查抗锯齿功能，关闭后可防止边缘混叠和视觉缝隙，应默认关闭
            if (QualitySettings.antiAliasing > 0)
            {
                Debug.LogWarning("CellSpace: Anti-aliasing is enabled. This may cause seam lines to appear between blocks. If you see lines between blocks, try disabling anti-aliasing, switching to deferred rendering path, or adding some texture padding in the engine settings." +
                    "启用了抗锯齿，这可能导致在块之间出现接缝线！如果你看到块之间的线条，试着禁用抗锯齿，切换到延迟渲染路径，或者在引擎设置中添加一些纹理填充。");
            }

            //引擎初始化状态
            Initialized = true;

        }

        // ==== world data ====

        /// <summary>
        /// 更新世界路径（定位到名为 “Worlds” 的存档目录，不同系统位置不一）
        /// </summary>
        private static void UpdateWorldPath()
        {
            //"../"是通用的文件系统路径表示法，用于表示上一级目录
            //下列动作意味着从 Application.dataPath 所指向的目录开始，导航到上一级目录（即 Application.dataPath 的父目录），再定位到名为 “Worlds” 的目录
            WorldPath = Application.dataPath + "/../Worlds/" + WorldName + "/"; // you can set World Path here
                                                                                //WorldPath = "/mnt/sdcard/UniblocksWorlds/" + CPEngine.WorldName + "/"; // example mobile path for Android
        }

        /// <summary>
        /// 设置活动世界名称（设置后世界种子将被重置为0，并刷新用于档案存储的世界路径）。可用本函数在运行时更改世界名称。
        /// </summary>
        /// <param name="worldName"></param>
        public static void SetWorldName(string worldName)
        {
            WorldName = worldName;
            WorldSeed = 0;
            UpdateWorldPath();
        }

        /// <summary>
        /// 从文件中读取当前活动世界的种子，或者如果没有找到种子文件则随机生成一个新的种子，并将其存储在Engine.WorldSeed变量中。
        /// </summary>
        public static void GetSeed()
        { // reads the world seed from file if it exists, else creates a new seed and saves it to file

            //if (Application.isWebPlayer) { // don'transform save to file if webplayer		
            //	CPEngine.WorldSeed = Random.CameraLookRange (ushort.MinValue, ushort.MaxValue);
            //	return;
            //}		

#if UNITY_WEBPLAYER
            //当前平台是WebPlayer，本地化存储应取消
            Engine.WorldSeed = Random.Range (ushort.MinValue, ushort.MaxValue
            return;
#else
            //当前平台不是WebPlayer
#endif
            //存在种子文件则读取
            if (File.Exists(WorldPath + "seed"))
            {
                //创建文件的读取流
                StreamReader reader = new StreamReader(WorldPath + "seed");
                WorldSeed = int.Parse(reader.ReadToEnd()); //读取全部字符串后转为数字，作为世界种子
                reader.Close();
            }
            else
            {
                //循环的目的是确保生成的 WorldSeed 值不为 0
                while (WorldSeed == 0)
                {
                    //创建一个新的种子
                    WorldSeed = Random.Range(ushort.MinValue, ushort.MaxValue);
                }
                Directory.CreateDirectory(WorldPath); //如文件夹存在则不会创建新的，该动作不会抛出异常无需用if (!Directory.Exists(WorldPath))判断
                StreamWriter writer = new StreamWriter(WorldPath + "seed"); //指定文件路径，创建一个写入流
                writer.Write(WorldSeed.ToString()); //为文件写入内容字符串
                //在执行 Close 方法之前调用 Flush 方法可以确保所有数据在关闭文件之前被正确地写入
                writer.Flush();
                writer.Close();
                //虽然在大多数情况下，调用 Close 方法时会自动调用 Flush 方法，但在某些特殊情况下，例如当文件系统繁忙或者出现其他问题时，数据可能无法正确地写入文件
                //在这种情况下显式调用 Flush 方法可确保数据在关闭文件之前被正确地写入，确保在程序异常情况下数据不会丢失（像关闭文件之前程序崩溃或出现异常）
            }
        }

        /// <summary>
        /// 将所有当前实例化团块的数据保存到磁盘，在Engine.MaxChunkSaves中可指定每帧保存团块的最大处理速率。
        /// </summary>
        public static void SaveWorld()
        { // saves the data over multiple frames

            //实例调用继承自父类的方法来异步处理存档（使用了Unity的协程）
            EngineInstance.StartCoroutine(CellChunkDataFiles.SaveAllChunks());
        }

        /// <summary>
        /// 将所有当前实例化的团块数据保存到磁盘存档，单帧动作一次全执行，这很可能会使游戏冻结几秒钟，因此不建议在游戏过程中使用此功能。
        /// </summary>
        public static void SaveWorldInstant()
        { // writes data from TempChunkData into region files

            CellChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====	

        /// <summary>
        /// 获取体素ID对应的单元预制体
        /// </summary>
        /// <param name="cellId">体素ID（单元预制体种类）</param>
        /// <returns>返回体素ID对应单元种类的游戏物体对象，体素ID=0或65535时返回空块</returns>
        public static GameObject GetCellGameObject(ushort cellId)
        {
            try
            {
                //如果体素ID达到ushort数据类型的最大值65535，那么归零（防止从负数开始）
                if (cellId == ushort.MaxValue) cellId = 0;
                GameObject cellObject = Blocks[cellId];//获取体素ID对应单元种类的预制体
                //检查单元对象上的单元组件
                if (cellObject.GetComponent<Cell>() == null)
                {
                    Debug.LogError("CellSpace: Cell id " + cellId + " does not have the Cell component attached!" +
                        "游戏物体对象的单元组件不存在！返回空块！");
                    return Blocks[0];
                }
                else
                {
                    return cellObject;
                }

            }
            catch (System.Exception)
            {
                //报错并指出无效体素ID
                Debug.LogError("CellSpace: Invalid cell id: " + cellId);
                return Blocks[0];
            }
        }

        /// <summary>
        /// 获取体素ID对应的单元预制体的单元类型组件
        /// </summary>
        /// <param name="cellID">体素ID（单元预制体种类）</param>
        /// <returns>返回体素ID对应单元上的单元类型组件，体素ID=0或65535时返回空块上的单元类型组件</returns>
        public static Cell GetCellType(ushort cellID)
        {
            try
            {
                //如果体素ID达到ushort数据类型的最大值65535，那么归零（防止从负数开始）
                if (cellID == ushort.MaxValue) cellID = 0;
                Cell cell = Blocks[cellID].GetComponent<Cell>();//获取体素ID对应单元上的单元类型组件
                if (cell == null)
                {
                    //单元组件不存在
                    Debug.LogError("CellSpace: Cell ID " + cellID + " does not have the Cell component attached!");
                    return null;
                }
                else
                {
                    //返回体素ID对应单元上的单元类型组件
                    return cell;
                }

            }
            catch (System.Exception)
            {
                //报错并指出无效体素ID
                Debug.LogError("CellSpace: Invalid Cell ID: " + cellID);
                return null;
            }
        }

        /// <summary>
        /// 使用指定原点、方向和范围执行光线投射，并返回单元索引，其中包含命中团块的游戏物体(CellInfo.chunk)、命中单元的索引(CellInfo.index)及与命中面相邻单元索引(CellInfo.adjacentindex)。
        /// “ignoreTransparent”为true时光线投射将穿透透明或半透明的单元，若没有击中则返回null。注意：如果碰撞体生成被禁用，此函数将不起作用，另在2D模式下Z=0但默认插入一个立方体，只是纹理在Up面改为Forward
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static CellInfo CellRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        { // a raycast which returns the index of the hit cell and the gameobject of the hit chunk

            RaycastHit hit = new RaycastHit(); //创建射线投射器hit

            //利用物理引擎投射光线，hit的绘制从origin（摄像机位置）出发沿direction（摄像机前方）方向，最大距离range
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                //如果从hit碰撞体组件对象里能获取到团块或团块扩展组件（这里因为Collider是继承Component的，所以可直接使用父类的GetComponent方法获取当前游戏物体对象身上的其他兄弟组件）
                if (hit.collider.GetComponent<CellChunk>() != null || hit.collider.GetComponent<CellChunkExtension>() != null)
                { // check if we're actually hitting a chunk.检查我们是否真的击中了团块

                    GameObject hitObject = hit.collider.gameObject; //从碰撞体组件中获得游戏物体对象并赋值给hitObject

                    if (hitObject.GetComponent<CellChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk.如果我们击中的是网状容器而不是团块（判断依据是网状容器拥有大块扩展组件），注意网格容器是团块大小的，虽是团块的子对象但它不是单元）
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object.将网格容器替换为实际的团块对象（它是网格容器对象的父级对象）
                    }

                    //根据hit碰撞面法线方向来推离或推进hit位置，后将新位置转为在团块的本地局部坐标（相对位置）来获取单元索引（false指不获取相邻单元，则推进hit到所碰单元内部），最终将hit新位置进行四舍五入修正以靠近最近顶点作为单元索引返回
                    CPIndex hitIndex = hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, false);

                    //忽略透明（功能尚未完善）
                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent cell is hit.当一个透明单元被击中时，再次通过光线投射穿透透明单元
                        ushort hitCell = hitObject.GetComponent<CellChunk>().GetCellID(hitIndex.x, hitIndex.y, hitIndex.z); //通过单元索引从团块里获得体素ID（单元预制体种类）
                        //如果命中的单元类型的VTransparency属性!=实心，说明是透明或半透明
                        if (GetCellType(hitCell).VTransparency != Transparency.solid)
                        {
                            Vector3 newOrigin = hit.point; //存储hit坐标
                            newOrigin.y -= 0.5f; // push the new raycast down a bit.将hit向下高度移动0.5（基本上hit跑到所选单元内部）
                            return CellRaycast(newOrigin, Vector3.down, range - hit.distance, true); //递归调用函数自身，以新点开始重新向下射出射线，来完成剩余距离碰撞检测（true指获取相邻单元）
                                                                                                     //这段代码只能处理向下的透明单元，其他方向（如向上、向左、向右等）也透明那么无法正确地“穿透”


                        }
                    }

                    return new CellInfo(
                                         hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, false), // get hit cell index.获取击中单元的索引
                                         hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, true), // get adjacent cell index.获取相邻单元的索引
                                         hitObject.GetComponent<CellChunk>()); // get chunk.获取团块
                }
            }

            //其他情况
            return null;
        }

        /// <summary>
        /// 使用指定射线和范围执行光线投射，并返回VoxelInfo，其中包含命中团块GameObject(CellInfo.chunk)、命中单元的索引(CellInfo.index)及与命中面相邻单元的索引(CellInfo. adjacentindex)。
        /// “ignoreTransparent”为true时光线投射将穿透透明或半透明的单元。若没有击中任何块，则返回null。注意：如果碰撞体生成被禁用，此函数将不起作用。
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static CellInfo CellRaycast(Ray ray, float range, bool ignoreTransparent)
        {
            return CellRaycast(ray.origin, ray.direction, range, ignoreTransparent);
        }

        /// <summary>
        /// 返回与给定世界位置相对应的团块索引
        /// </summary>
        /// <param name="position">团块位置</param>
        /// <returns></returns>
        public static CPIndex PositionToChunkIndex(Vector3 position)
        {
            CPIndex chunkIndex;
            if (CPEngine.HorizontalMode)
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength);
            }
            else
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            }
            return chunkIndex;
        }

        /// <summary>
        /// 返回与给定世界位置相对应的团块游戏对象，若团块没有实例化则返回null。
        /// </summary>
        /// <param name="position">团块位置</param>
        /// <returns></returns>
        public static GameObject PositionToChunk(Vector3 position)
        {
            CPIndex chunkIndex;
            if (CPEngine.HorizontalMode)
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength);
            }
            else
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            }

            return CellChunkManager.GetChunk(chunkIndex);

        }

        /// <summary>
        /// 将位置转换成单元信息（其中包含与给定世界位置对应的单元，如单元的团块没被实例化则返回null）
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static CellInfo PositionToCellInfo(Vector3 position)
        {
            GameObject chunkObject = PositionToChunk(position);
            if (chunkObject != null)
            {
                CellChunk chunk = chunkObject.GetComponent<CellChunk>();
                CPIndex cellIndex = chunk.PositionToCellIndex(position);
                return new CellInfo(cellIndex, chunk);
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 返回指定单元中心点的世界位置。
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <returns></returns>
        public static Vector3 CellInfoToPosition(CellInfo cellInfo)
        {
            return cellInfo.chunk.GetComponent<CellChunk>().CellIndexToPosition(cellInfo.index);
        }

        // ==== mesh creator ====

        //块编辑器中如果勾选Custom Mesh则启用默认材质，只有不勾选Custom Mesh时可以自定义材质，也就是可以选择图片纹理偏移
        //图片纹理偏移有两种：第一种是勾选定义单面纹理，会出现6个面让你逐一填写纹理偏移点；第二种是不勾选，那么6个面采用相同纹理偏移点
        //使用纹理偏移点获取预制图片上的纹理：左下角精灵是(0,0)，而(1,0)表示右边精灵，每个精灵的大小由引擎设置中Texture unit决定
        //Texture unit数值填8（内部UV划区时是倒数比例0.125）表示对整张图片切割为8*8=64个均分的精灵，MC插件魔改后成为CellSpace库，支持行列不同划区数量及识别多个材质主纹理

        /// <summary>
        /// 获取纹理偏移点
        /// </summary>
        /// <param name="cellID">体素ID（单元预制体种类）</param>
        /// <param name="facing">面向</param>
        /// <returns>如没定义纹理则返回Vector2(0, 0)，如单元没用自定义单面纹理则返回顶部纹理点，如请求一个没定义的纹理则抓取最后定义纹理点来返回</returns>
        public static Vector2 GetTextureOffset(ushort cellID, Facing facing)
        {
            //获取单元的类型
            Cell cellType = GetCellType(cellID);
            //获取纹理数组（二维向量点阵小组）
            Vector2[] textureArray = cellType.VTexture;
            if (textureArray.Length == 0)
            { // in case there are no textures defined, return a default texture.以防万一如果没有定义纹理，则返回默认的(0, 0)
                Debug.LogWarning("CellSpace: Block " + cellID.ToString() + " has no defined textures! Using default texture.");
                return new Vector2(0, 0);
            }
            else if (cellType.VCustomSides == false)
            { // if this cell isn'transform using custom side textures, return the Up texture.如果这个单元没有使用自定义单面纹理，直接返回单元朝上的那面纹理点（6个面共用）
                return textureArray[0];
            }
            //将面向这个枚举类型转整型（上下左右前后默认对应0~5），如面向所代表的数值超过纹理点阵长度-1，将被判定为没有定义纹理（自定义6面却忘了填完它们）
            else if ((int)facing > textureArray.Length - 1)
            { // if we're asking for a texture that's not defined, grab the last defined texture instead.如果我们请求了一个没有定义的纹理，则抓取最后定义的纹理点（剩下的面会采用它代表的精灵的纹理）
                return textureArray[textureArray.Length - 1];
            }
            else
            {
                //正常返回一个对应面向索引的纹理点
                return textureArray[(int)facing];
            }
        }
    }
}

// VS2022编写本脚本，在右侧解决方案窗口，引用中添加dll等方法库文件，之后可用其内指定命名空间的具体方法进行编程
// 在C#编程中，命名空间（Namespace）是一种组织代码的方式，它可以帮助我们避免类名、方法名等之间的冲突，并提供一种逻辑上的分组机制
// UnityEngine 命名空间
// 主要作用：提供Unity游戏引擎的核心功能。UnityEngine命名空间包含了创建和管理Unity游戏所需的所有基础类和接口。
// 包含内容：这个命名空间包含用于场景管理、对象操作、渲染、物理、输入处理、网络、音频、用户界面、动画、脚本生命周期管理等功能的类。例如，Transform 类用于表示和操作游戏对象的位置、旋转和缩放；GameObject 类是Unity场景中的基本构建块；MonoBehaviour 类是所有脚本组件的基类，它提供了如Start和Update等生命周期方法。
// System.Collections.Generic 命名空间
// 主要作用：提供了一系列泛型集合类，这些类用于存储和管理数据集合，如列表、字典、集合、队列等。
// 包含内容：这个命名空间包含了如List<T>（泛型列表）、Dictionary<TKey, TValue>（键值对集合）、HashSet<T>（集合，不包含重复元素）、Queue<T>（队列）等类。这些类为数据存储和操作提供了高效和灵活的方式。
// System.IO 命名空间
// 主要作用：提供文件和数据流的基本输入/输出功能。System.IO命名空间包含用于文件和数据流操作的类，如文件读写、目录管理、数据流处理等。
// 包含内容：这个命名空间中的类允许你创建文件、读取文件内容、写入文件、删除文件、管理目录结构、处理数据流等。例如，File 类提供了静态方法用于文件的创建、复制、删除、移动和打开；Directory 类用于创建、删除和移动目录；StreamReader 和 StreamWriter 类用于从文件中读取文本和向文件中写入文本。
// 在Unity项目中，通常会通过引用这些命名空间来使用它们提供的类和功能。例如，在脚本文件的开头使用using UnityEngine;语句，就可以让你在脚本中直接使用Unity引擎提供的所有类和功能，而无需每次都写出完整的命名空间路径。
