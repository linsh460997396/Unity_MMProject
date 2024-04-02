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
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Uniblocks
{
    #region 枚举

    /// <summary>
    /// 体素块的6个面
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
    /// 存储全局引擎设置，并提供一些静态功能用于数据转换等。组件用法：Unity中随便新建一个空对象“Engine”，把脚本拖到组件位置即挂载（Unity要求一个cs文件只能一个类，且类名须与文件名一致）
    /// </summary>
    public class Engine : MonoBehaviour
    {
        //私有或静态字段不被自动序列化到Inspetor，想要被序列化请使用[SerializeField]特性，想不被序列化请使用[NonSerialized]特性

        #region 字段、属性方法

        // Engine的每个静态变量都有一个非静态的等价物。非静态变量的名称与静态变量相同，只是在开头用小写的L，使用这些变量是为了能够在Unity中编辑这些变量，包括在引擎设置窗口中。
        // 在编辑器的Awake功能中，非静态变量被应用于它们的静态对应(通过场景中的Engine游戏对象)，所以在运行时改变非静态变量不会产生任何影响。

        /// <summary>
        /// 当前活动世界的名称（对应存储世界数据的文件夹）
        /// </summary>
        public static string WorldName;

        /// <summary>
        /// 世界数据文件的路径（默认路径为/application_root/world_name/），可通过直接编辑Engine脚本内的UpdateWorldPath私有函数来更改世界路径。
        /// </summary>
        public static string WorldPath;

        /// <summary>
        /// 体素块路径（Unity项目中块预制体的路径），这是块编辑器用来查找块的。
        /// </summary>
        public static string BlocksPath;

        /// <summary>
        /// 当前活动世界的种子，可用于程序地形生成，种子存储在世界数据文件夹中
        /// </summary>
        public static int WorldSeed;

        /// <summary>
        /// [GUI界面输入]世界数据文件的路径（默认路径为/application_root/world_name/），可通过直接编辑Engine脚本内的UpdateWorldPath私有函数来更改世界路径。
        /// </summary>
        public string lWorldName = "Default";

        /// <summary>
        /// [GUI界面输入]体素块路径（Unity项目中块预制体的路径），这是块编辑器用来查找块的。
        /// </summary>
        public string lBlocksPath;

        /// <summary>
        /// 三维立体像素块（体素块）预制体，在块编辑器中定义，数组索引对应于块的体素ID（体素块预制体种类）。
        /// </summary>
        public static GameObject[] Blocks;
        /// <summary>
        /// [GUI界面输入]三维立体像素块（体素块）预制体，在块编辑器中定义，数组索引对应于块的体素ID（体素块预制体种类）。
        /// </summary>
        public GameObject[] lBlocks;

        // 团块创建设置（团块就是这些体素块的集合、由小块堆组成的大块）

        /// <summary>
        /// 衍生团块的最大正负垂直块索引，即团块自动创建时高度范围限值（如果是3，表示高度Y最大是3个团块范围，如果团块是16边长，那么Y最大高度48）
        /// </summary>
        public static int HeightRange;
        /// <summary>
        /// 从原点到生成团块的水平距离(以团块为单位)，是团块自动创建时的距离限制（如果是8，则始终在玩家周围保证有8范围的团块）
        /// </summary>
        public static int ChunkSpawnDistance;
        /// <summary>
        /// 一个团块的边长(以体素为单位)，即团块自动创建时的正方形单边尺寸边长（如果是16，则团块由16*16*16个体素块组成）
        /// </summary>
        public static int ChunkSideLength;
        /// <summary>
        /// 团块自动摧毁时的判断距离（如果是3，则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁，摧毁时会存入档案以便玩家走过来时读取生成）
        /// </summary>
        public static int ChunkDespawnDistance;

        /// <summary>
        /// 保持地形高度
        /// </summary>
        public static bool KeepTerrainHeight;
        /// <summary>
        /// 地形高度
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
        /// [GUI界面输入]衍生团块的最大正负垂直块索引，即团块自动创建时高度范围限值（如果是3，表示高度Y最大是3个团块范围，如果团块是16边长，那么Y最大高度48）
        /// </summary>
        public int lHeightRange;
        /// <summary>
        /// [GUI界面输入]从原点到生成团块的水平距离(以团块为单位)，是团块自动创建时的距离限制（如果是8，则始终在玩家周围保证有8范围的团块）
        /// </summary>
        public int lChunkSpawnDistance;
        /// <summary>
        /// [GUI界面输入]一个团块的边长(以体素为单位)，即团块自动创建时的正方形单边尺寸边长（如果是16，则团块由16*16*16个体素块组成）
        /// </summary>
        public int lChunkSideLength;
        /// <summary>
        /// [GUI界面输入]团块自动摧毁时的判断距离（如果是3，则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁，摧毁时会存入档案以便玩家走过来时读取生成）
        /// </summary>
        public int lChunkDespawnDistance;

        // 纹理设置

        /// <summary>
        /// 纹理单元倍率（一个块的纹理边长与纹理表边长之比，用于计算体素块纹理），形象说明的话相当于每个精灵在整张图片中的比例，默认是0.125说明整张图片被横竖均分8x8个纹理单元
        /// </summary>
        public static float TextureUnit;
        /// <summary>
        /// 纹理表上纹理之间的填充，作为单个体素块纹理大小的一小部分。即每个纹理单元之间填充缝的大小（依然是整张图片的倍率，如图片是512x512像素，要在纹理各单元间填充1像素需填写1/512），填充以避免取到别的纹理单元
        /// </summary>
        public static float TexturePadding;

        // 纹理设置，GUI界面输入

        /// <summary>
        /// [GUI界面输入]纹理单元倍率（一个块的纹理边长与纹理表边长之比，用于计算体素块纹理），形象说明的话相当于每个精灵在整张图片中的比例，默认是0.125说明整张图片被横竖均分8x8个纹理单元
        /// </summary>
        public float lTextureUnit;
        /// <summary>
        /// [GUI界面输入]纹理表上纹理之间的填充，作为单个体素块纹理大小的一小部分。即每个纹理单元之间填充缝的大小（依然是整张图片的倍率，如图片是512x512像素，要在纹理各单元间填充1像素需填写1/512），填充以避免取到别的纹理单元
        /// </summary>
        public float lTexturePadding;

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
        /// [GUI界面输入]团块数据请求上限：每个客户端一次可以在服务器中排队的最大团块数据请求数(0=无限制)。如客户端生成数据块的速度太快且你发现你的服务器无法跟上数据请求的速度，那么降低这个限制。
        /// </summary>
        public int lMaxChunkDataRequests;

        // 全局设置

        /// <summary>
        /// 体素块的侧面可见（前提是没有与它们接壤的团块实例）
        /// </summary>
        public static bool ShowBorderFaces;
        /// <summary>
        /// 产生碰撞体（为false则团块将不会生成任何Colliders）
        /// </summary>
        public static bool GenerateColliders;
        /// <summary>
        /// 发送镜头注视事件（如果为true, CameraEventsSender组件将把事件发送到主摄像机视场中心指着的体素块）
        /// </summary>
        public static bool SendCameraLookEvents;
        /// <summary>
        /// 发送鼠标指针事件（如果为true, CameraEventsSender组件将将把事件发送到当前鼠标光标指着的体素块）
        /// </summary>
        public static bool SendCursorEvents;
        /// <summary>
        /// 允许多人玩家（团块将从服务器请求体素数据而不是从硬盘生成或加载，另外Voxel.ChangeBlock、Voxel.PlaceBlock和Voxel.DestroyBlock会将体素变化发送到服务器以便重新分发给其他连接的玩家）
        /// </summary>
        public static bool EnableMultiplayer;
        /// <summary>
        /// 用于确定网络同步轨道位置的处理方式，服务器检查玩家的位置以确定是否需要将体素更改发送给该玩家，客户端则会通过ChunkLoader脚本向服务器发送一个玩家位置更新。
        /// 在多人游戏中，通常需要将物体的位置同步到其他客户端。轨道位置是指物体沿着一条路径或轨道移动时所处的位置。如果将MultiplayerTrackPosition字段设置为true，则表示该物体的位置将在网络上进行同步，且每个客户端都将跟踪该物体的轨道位置。
        /// 如果将MultiplayerTrackPosition字段设置为false，则表示该物体的位置不会在网络上进行同步，而客户端将不会跟踪其轨道位置。
        /// 在具有大量移动物体的多人游戏中，使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能。例如，如果某个物体在场景中静止不动，则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步。
        /// </summary>
        public static bool MultiplayerTrackPosition;
        /// <summary>
        /// 保存体素数据。为false则团块将不会加载或保存体素数据，反之在生成团块时总是会生成新的数据。
        /// </summary>
        public static bool SaveVoxelData;
        /// <summary>
        /// 产生网格
        /// </summary>
        public static bool GenerateMeshes;

        // 全局设置，GUI界面输入

        /// <summary>
        /// [GUI界面输入]体素块的侧面可见（前提是没有与它们接壤的团块实例）
        /// </summary>
        public bool lShowBorderFaces;
        /// <summary>
        /// [GUI界面输入]产生碰撞体（为false则团块将不会生成任何Colliders）
        /// </summary>
        public bool lGenerateColliders;
        /// <summary>
        /// [GUI界面输入]发送镜头注视事件（如果为true, CameraEventsSender组件将把事件发送到主摄像机视场中心指着的体素块）
        /// </summary>
        public bool lSendCameraLookEvents;
        /// <summary>
        /// [GUI界面输入]发送鼠标指针事件（如果为true, CameraEventsSender组件将将把事件发送到当前鼠标光标指着的体素块）
        /// </summary>
        public bool lSendCursorEvents;
        /// <summary>
        /// [GUI界面输入]允许多人玩家（团块将从服务器请求体素数据而不是从硬盘生成或加载，另外Voxel.ChangeBlock、Voxel.PlaceBlock和Voxel.DestroyBlock会将体素变化发送到服务器以便重新分发给其他连接的玩家）
        /// </summary>
        public bool lEnableMultiplayer;
        /// <summary>
        /// [GUI界面输入]用于确定网络同步轨道位置的处理方式，服务器检查玩家的位置以确定是否需要将体素更改发送给该玩家，客户端则会通过ChunkLoader脚本向服务器发送一个玩家位置更新。
        /// 在多人游戏中，通常需要将物体的位置同步到其他客户端。轨道位置是指物体沿着一条路径或轨道移动时所处的位置。如果将MultiplayerTrackPosition字段设置为true，则表示该物体的位置将在网络上进行同步，且每个客户端都将跟踪该物体的轨道位置。
        /// 如果将MultiplayerTrackPosition字段设置为false，则表示该物体的位置不会在网络上进行同步，而客户端将不会跟踪其轨道位置。
        /// 在具有大量移动物体的多人游戏中，使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能。例如，如果某个物体在场景中静止不动，则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步。
        /// </summary>
        public bool lMultiplayerTrackPosition;
        /// <summary>
        /// [GUI界面输入]保存体素数据。为false则团块将不会加载或保存体素数据，反之在生成团块时总是会生成新的数据。
        /// </summary>
        public bool lSaveVoxelData;
        /// <summary>
        /// [GUI界面输入]产生网格
        /// </summary>
        public bool lGenerateMeshes;

        /// <summary>
        /// 团块超时：如一团块通过ChunkManager.SpawnChunk动作创建，但在ChunkTimeout这段时间内没有被访问，它将被销毁(在保存它的体素数据后)。
        /// 来自客户端的体素数据请求和团块中的体素变化将重置计时器。当值为0将禁用此功能。
        /// </summary>
        public static float ChunkTimeout;
        /// <summary>
        /// 团块超时：如一团块通过ChunkManager.SpawnChunk动作创建，但在ChunkTimeout这段时间内没有被访问，它将被销毁(在保存它的体素数据后)。
        /// 来自客户端的体素数据请求和团块中的体素变化将重置计时器。当值为0将禁用此功能。
        /// </summary>
        public float lChunkTimeout;
        /// <summary>
        /// 允许团块超时（如果Engine.ChunkTimeout>0这个变量会自动设置为true）
        /// </summary>
        public static bool EnableChunkTimeout;

        //其他

        /// <summary>
        /// 正方形边长（用于定义网格或地图大小），它等于接口数据中所填团块边长的平方，如团块边长是16，那么大地图的边长是256
        /// </summary>
        public static int SquaredSideLength;
        /// <summary>
        /// 处理网络通信和同步的游戏物体对象
        /// </summary>
        public static GameObject UniblocksNetwork;
        /// <summary>
        /// Uniblocks引擎类型实例
        /// </summary>
        public static Engine EngineInstance;
        /// <summary>
        /// 团块管理器实例
        /// </summary>
        public static ChunkManager ChunkManagerInstance;
        /// <summary>
        /// 团块预制体的大小（缩放比例）
        /// </summary>
        public static Vector3 ChunkScale;
        /// <summary>
        /// Uniblocks引擎初始化状态
        /// </summary>
        public static bool Initialized;

        #endregion

        // ==== initialization ====

        public void Awake()
        {
            //本函数负责Uniblocks引擎初始化

            EngineInstance = this; //this关键字引用了当前类的一个实例，但它不能用在静态字段的初始化中，所以写在这
            //获取对象上的团块管理器组件实例（这里指名为"ChunkManager"的脚本类型组件实例化后的对象）
            ChunkManagerInstance = GetComponent<ChunkManager>();
            //读取GUI界面输入里填的世界名称
            WorldName = lWorldName;
            //更新世界存档的路径
            UpdateWorldPath();

            #region 将GUI界面输入数据赋值给实际运作的字段属性

            BlocksPath = lBlocksPath;
            Blocks = lBlocks;

            TargetFPS = lTargetFPS;
            MaxChunkSaves = lMaxChunkSaves;
            MaxChunkDataRequests = lMaxChunkDataRequests;

            TextureUnit = lTextureUnit;
            TexturePadding = lTexturePadding;
            GenerateColliders = lGenerateColliders;
            ShowBorderFaces = lShowBorderFaces;
            EnableMultiplayer = lEnableMultiplayer;
            MultiplayerTrackPosition = lMultiplayerTrackPosition;
            SaveVoxelData = lSaveVoxelData;
            GenerateMeshes = lGenerateMeshes;

            ChunkSpawnDistance = lChunkSpawnDistance;
            HeightRange = lHeightRange;
            ChunkDespawnDistance = lChunkDespawnDistance;

            SendCameraLookEvents = lSendCameraLookEvents;
            SendCursorEvents = lSendCursorEvents;

            ChunkSideLength = lChunkSideLength;
            SquaredSideLength = lChunkSideLength * lChunkSideLength;

            #endregion

            //建立已加载的区域组（字典<string, string[]>）
            ChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
            //建立临时团块数据组（字典<string, string>）
            ChunkDataFiles.TempChunkData = new Dictionary<string, string>();

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
                Debug.LogWarning("Uniblocks: Layer 26 is reserved for Uniblocks, it is automatically set to ignore collision with all layers." +
                                 "第26层是为Uniblocks保留的，它被自动设置为忽略与所有图层的碰撞！");
            }
            for (int i = 0; i < 31; i++)
            {
                //Unity有32个可用的层0~31，此处设置第i~26之间的对象不发生碰撞（亦能穿过彼此而无碰撞事件）
                Physics.IgnoreLayerCollision(i, 26);
            }

            #region 检查团块

            //检查GUI界面输入里预定义的体素块种类计数
            if (Blocks.Length < 1)
            {
                Debug.LogError("Uniblocks: The blocks array is empty! Use the Block Editor to update the blocks array." +
                    "体素块是空的！使用块编辑器来更新！");
                Debug.Break();
            }

            //检查第一个体素块（空块）是否存在，如不存在或没有体素组件则报错
            if (Blocks[0] == null)
            {
                Debug.LogError("Uniblocks: Cannot find the empty block prefab (id 0)!" +
                    "找不到空块预制体（id 0）！");
                Debug.Break();
            }
            else if (Blocks[0].GetComponent<Voxel>() == null)
            {
                Debug.LogError("Uniblocks: Empty block prefab (id 0) does not have the Voxel component attached!" +
                    "空块预制体(id 0)没有体素组件");
                Debug.Break();
            }

            #endregion

            #region 检查设置

            //检查团块边长（至少为1才有效）
            if (ChunkSideLength < 1)
            {
                Debug.LogError("Uniblocks: Chunk side length must be greater than 0!" +
                    "团块边长必须大于0");
                Debug.Break(); //暂停编辑器运行
            }

            //如果团块生成距离<1则被置为0（不再生成），默认是8
            if (ChunkSpawnDistance < 1)
            {
                ChunkSpawnDistance = 0;
                Debug.LogWarning("Uniblocks: Chunk spawn distance is 0. No chunks will spawn!" +
                    "团块生成距离为0，不会生成块");
            }

            //如果高度范围小于0，则高度范围将被置为0，默认是3
            if (HeightRange < 0)
            {
                HeightRange = 0;
                Debug.LogWarning("Uniblocks: Chunk height range can't be a negative number! Setting chunk height range to 0." +
                    "团块高度范围不能是一个负数！已被重置为0");
            }

            //检查团块数据请求上限
            if (MaxChunkDataRequests < 0)
            {
                MaxChunkDataRequests = 0;
                Debug.LogWarning("Uniblocks: Max chunk data requests can't be a negative number! Setting max chunk data requests to 0." +
                    "团块数据请求上限不能是负数！已被重置为0");
            }

            #endregion

            //检查材质
            GameObject chunkPrefab = GetComponent<ChunkManager>().ChunkObject; //获取团块管理器中关联的团块物体对象作为团块预制体
            int materialCount = chunkPrefab.GetComponent<Renderer>().sharedMaterials.Length - 1; //（额外）材质计数=团块预制体渲染组件的共享材质数量-1

            //遍历配置里预定义的体素块
            for (ushort i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i] != null)
                {
                    //获取体素
                    Voxel voxel = Blocks[i].GetComponent<Voxel>();

                    //如果体素子网格索引<0则报错
                    if (voxel.VSubmeshIndex < 0)
                    {
                        Debug.LogError("Uniblocks: Voxel " + i + " has a material index lower than 0! Material index must be 0 or greater." +
                            "体素的材质索引小于0！必须大于等于0");
                        Debug.Break();
                    }

                    //如体素子网格索引大于（额外）材质计数则报错（使用自定义的额外材质索引后没给它上材质）
                    if (voxel.VSubmeshIndex > materialCount)
                    {
                        //体素使用了GUI界面输入中自定义材质索引，但团块预制体只有（额外）材质计数+1个材质附着，设置一个更低的材质索引或附着更多材质到团块预制体！
                        Debug.LogError("Uniblocks: Voxel " + i + " uses material index " + voxel.VSubmeshIndex + ", but the chunk prefab only has " + (materialCount + 1) + " material(s) attached. Set a lower material index or attach more materials to the chunk prefab.");
                        Debug.Break();
                    }
                }
            }

            //质量配置，检查抗锯齿功能，关闭后可防止边缘混叠和视觉缝隙，应默认关闭
            if (QualitySettings.antiAliasing > 0)
            {
                Debug.LogWarning("Uniblocks: Anti-aliasing is enabled. This may cause seam lines to appear between blocks. If you see lines between blocks, try disabling anti-aliasing, switching to deferred rendering path, or adding some texture padding in the engine settings." +
                    "启用了抗锯齿，这可能导致在块之间出现接缝线！如果你看到块之间的线条，试着禁用抗锯齿，切换到延迟渲染路径，或者在引擎设置中添加一些纹理填充。");
            }

            //Uniblocks引擎初始化状态
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
                                                                                //WorldPath = "/mnt/sdcard/UniblocksWorlds/" + Engine.WorldName + "/"; // example mobile path for Android
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

            //if (Application.isWebPlayer) { // don't save to file if webplayer		
            //	Engine.WorldSeed = Random.CameraLookRange (ushort.MinValue, ushort.MaxValue);
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
            EngineInstance.StartCoroutine(ChunkDataFiles.SaveAllChunks());
        }

        /// <summary>
        /// 将所有当前实例化的团块数据保存到磁盘存档，单帧动作一次全执行，这很可能会使游戏冻结几秒钟，因此不建议在游戏过程中使用此功能。
        /// </summary>
        public static void SaveWorldInstant()
        { // writes data from TempChunkData into region files

            ChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====	

        /// <summary>
        /// 获取体素ID对应的体素块预制体
        /// </summary>
        /// <param name="voxelId">体素ID（体素块预制体种类）</param>
        /// <returns>返回体素ID对应体素块种类的游戏物体对象，体素ID=0或65535时返回空块</returns>
        public static GameObject GetVoxelGameObject(ushort voxelId)
        {
            try
            {
                //如果体素ID达到ushort数据类型的最大值65535，那么归零（防止从负数开始）
                if (voxelId == ushort.MaxValue) voxelId = 0;
                GameObject voxelObject = Blocks[voxelId];//获取体素ID对应体素块种类的游戏物体对象
                //检查体素对象上的体素组件
                if (voxelObject.GetComponent<Voxel>() == null)
                {
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!" +
                        "游戏物体对象的体素组件不存在！返回空块！");
                    return Blocks[0];
                }
                else
                {
                    return voxelObject;
                }

            }
            catch (System.Exception)
            {
                //报错并指出无效体素ID
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return Blocks[0];
            }
        }

        /// <summary>
        /// 获取体素ID对应的体素块预制体的体素类型组件
        /// </summary>
        /// <param name="voxelId">体素ID（体素块预制体种类）</param>
        /// <returns>返回体素ID对应体素块上的体素类型组件，体素ID=0或65535时返回空块上的体素类型组件</returns>
        public static Voxel GetVoxelType(ushort voxelId)
        {
            try
            {
                //如果体素ID达到ushort数据类型的最大值65535，那么归零（防止从负数开始）
                if (voxelId == ushort.MaxValue) voxelId = 0;
                Voxel voxel = Blocks[voxelId].GetComponent<Voxel>();//获取体素ID对应体素块上的体素类型组件
                if (voxel == null)
                {
                    //体素组件不存在
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!");
                    return null;
                }
                else
                {
                    //返回体素ID对应体素块上的体素类型组件
                    return voxel;
                }

            }
            catch (System.Exception)
            {
                //报错并指出无效体素ID
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return null;
            }
        }

        /// <summary>
        /// 使用指定原点、方向和范围执行光线投射，并返回体素索引，其中包含命中团块的游戏物体(VoxelInfo.chunk)、命中体素的索引(VoxelInfo.index)及与命中面相邻体素索引(VoxelInfo.adjacentindex)。
        /// “ignoreTransparent”为true时光线投射将穿透透明或半透明的体素块，若没有击中则返回null。注意：如果碰撞体生成被禁用，此函数将不起作用。
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static VoxelInfo VoxelRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        { // a raycast which returns the index of the hit voxel and the gameobject of the hit chunk

            RaycastHit hit = new RaycastHit(); //创建射线投射器hit

            //利用物理引擎投射光线，hit的绘制从origin（摄像机位置）出发沿direction（摄像机前方）方向，最大距离range
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                //如果从hit碰撞体组件对象里能获取到团块或团块扩展组件（这里因为Collider是继承Component的，所以可直接使用父类的GetComponent方法获取当前游戏物体对象身上的其他兄弟组件）
                if (hit.collider.GetComponent<Chunk>() != null || hit.collider.GetComponent<ChunkExtension>() != null)
                { // check if we're actually hitting a chunk.检查我们是否真的击中了团块

                    GameObject hitObject = hit.collider.gameObject; //从碰撞体组件中获得游戏物体对象并赋值给hitObject

                    if (hitObject.GetComponent<ChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk.如果我们击中的是网状容器而不是团块（判断依据是网状容器拥有大块扩展组件），注意网格容器是团块大小的，虽是团块的子对象但它不是体素块）
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object.将网格容器替换为实际的团块对象（它是网格容器对象的父级对象）
                    }

                    //根据hit碰撞面法线方向来推离或推进hit位置，后将新位置转为在团块的本地局部坐标（相对位置）来获取体素索引（false指不获取相邻体素，则推进hit到所碰体素块内部），最终将hit新位置进行四舍五入修正以靠近最近顶点作为体素索引返回
                    Index hitIndex = hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false);

                    //忽略透明（功能尚未完善）
                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent voxel is hit.当一个透明体素被击中时，再次通过光线投射穿透透明体素
                        ushort hitVoxel = hitObject.GetComponent<Chunk>().GetVoxel(hitIndex.x, hitIndex.y, hitIndex.z); //通过体素索引从团块里获得体素ID（体素块预制体种类）
                        //如果命中的体素类型的VTransparency属性!=实心，说明是透明或半透明
                        if (GetVoxelType(hitVoxel).VTransparency != Transparency.solid)
                        {
                            Vector3 newOrigin = hit.point; //存储hit坐标
                            newOrigin.y -= 0.5f; // push the new raycast down a bit.将hit向下高度移动0.5（基本上hit跑到所选体素块内部）
                            return VoxelRaycast(newOrigin, Vector3.down, range - hit.distance, true); //递归调用函数自身，以新点开始重新向下射出射线，来完成剩余距离碰撞检测（true指获取相邻体素）
                                                                                                      //这段代码只能处理向下的透明体素，其他方向（如向上、向左、向右等）也透明那么无法正确地“穿透”


                        }
                    }

                    return new VoxelInfo(
                                         hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false), // get hit voxel index.获取击中体素的索引
                                         hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, true), // get adjacent voxel index.获取相邻体素的索引
                                         hitObject.GetComponent<Chunk>()); // get chunk.获取团块
                }
            }

            //其他情况
            return null;
        }

        /// <summary>
        /// 使用指定射线和范围执行光线投射，并返回VoxelInfo，其中包含命中团块GameObject(VoxelInfo.chunk)、命中体素的索引(VoxelInfo.index)及与命中面相邻体素的索引(VoxelInfo. adjacentindex)。
        /// “ignoreTransparent”为true时光线投射将穿透透明或半透明的体素块。若没有击中任何块，则返回null。注意：如果碰撞体生成被禁用，此函数将不起作用。
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static VoxelInfo VoxelRaycast(Ray ray, float range, bool ignoreTransparent)
        {
            return VoxelRaycast(ray.origin, ray.direction, range, ignoreTransparent);
        }

        /// <summary>
        /// 返回与给定世界位置相对应的团块索引
        /// </summary>
        /// <param name="position">团块位置</param>
        /// <returns></returns>
        public static Index PositionToChunkIndex(Vector3 position)
        {
            Index chunkIndex = new Index(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            return chunkIndex;
        }

        /// <summary>
        /// 返回与给定世界位置相对应的团块游戏对象，若团块没有实例化则返回null。
        /// </summary>
        /// <param name="position">团块位置</param>
        /// <returns></returns>
        public static GameObject PositionToChunk(Vector3 position)
        {
            Index chunkIndex = new Index(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            return ChunkManager.GetChunk(chunkIndex);

        }

        /// <summary>
        /// 将位置转换成体素信息（其中包含与给定世界位置对应的体素，如体素的团块没被实例化则返回null）
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static VoxelInfo PositionToVoxelInfo(Vector3 position)
        {
            GameObject chunkObject = PositionToChunk(position);
            if (chunkObject != null)
            {
                Chunk chunk = chunkObject.GetComponent<Chunk>();
                Index voxelIndex = chunk.PositionToVoxelIndex(position);
                return new VoxelInfo(voxelIndex, chunk);
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 返回指定体素中心点的世界位置。
        /// </summary>
        /// <param name="voxelInfo"></param>
        /// <returns></returns>
        public static Vector3 VoxelInfoToPosition(VoxelInfo voxelInfo)
        {
            return voxelInfo.chunk.GetComponent<Chunk>().VoxelIndexToPosition(voxelInfo.index);
        }




        // ==== mesh creator ====

        //块编辑器中如果勾选Custom Mesh则启用默认材质，只有不勾选Custom Mesh时可以自定义材质，也就是可以选择图片纹理偏移
        //图片纹理偏移有两种：第一种是勾选定义单面纹理，会出现6个面让你逐一填写纹理偏移点；第二种是不勾选，那么6个面采用相同纹理偏移点
        //使用纹理偏移点获取预制图片上的纹理：左下角精灵是(0, 0)，而(0, 1)表示右边精灵，每个精灵的大小由Uniblocks引擎设置中Texture unit决定
        //Texture unit数值是0.125则表示对整张图片切割为8*8=64个均分的精灵

        /// <summary>
        /// 获取纹理偏移点
        /// </summary>
        /// <param name="voxel">体素ID（体素块预制体种类）</param>
        /// <param name="facing">面向</param>
        /// <returns>如没定义纹理则返回Vector2(0, 0)，如体素没用自定义单面纹理则返回顶部纹理点，如请求一个没定义的纹理则抓取最后定义纹理点来返回</returns>
        public static Vector2 GetTextureOffset(ushort voxel, Facing facing)
        {
            //获取体素的类型
            Voxel voxelType = GetVoxelType(voxel);
            //获取纹理数组（二维向量点阵小组）
            Vector2[] textureArray = voxelType.VTexture;

            if (textureArray.Length == 0)
            { // in case there are no textures defined, return a default texture.以防万一如果没有定义纹理，则返回默认的(0, 0)
                Debug.LogWarning("Uniblocks: Block " + voxel.ToString() + " has no defined textures! Using default texture.");
                return new Vector2(0, 0);
            }
            else if (voxelType.VCustomSides == false)
            { // if this voxel isn't using custom side textures, return the Up texture.如果这个体素没有使用自定义单面纹理，直接返回体素块朝上的那面纹理点（6个面共用）
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
