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
        /// 固态
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

    //核心组件用法：Unity中随便新建一个空对象，挂载脚本

    /// <summary>
    /// Uniblocks引擎类型（核心组件N0.1）
    /// </summary>
    public class Engine : MonoBehaviour
    {
        //私有或静态字段不被自动序列化到Inspetor，想要被序列化请使用[SerializeField]特性，想不被序列化请使用[NonSerialized]特性

        #region 字段、属性方法

        /// <summary>
        /// 世界名称，用于档案
        /// </summary>
        public static string WorldName;

        /// <summary>
        /// 世界路径，用于档案
        /// </summary>
        public static string WorldPath;

        /// <summary>
        /// 体素块路径，用于档案
        /// </summary>
        public static string BlocksPath;

        /// <summary>
        /// 世界种子，用于档案
        /// </summary>
        public static int WorldSeed;

        /// <summary>
        /// 世界名称，GUI界面输入
        /// </summary>
        public string lWorldName = "Default";

        /// <summary>
        /// 体素块路径，GUI界面输入
        /// </summary>
        public string lBlocksPath;

        /// <summary>
        /// 三维立体像素块（体素块），在块编辑器中定义
        /// </summary>
        public static GameObject[] Blocks;
        /// <summary>
        /// 三维立体像素块（体素块），GUI界面输入
        /// </summary>
        public GameObject[] lBlocks;

        // 团块创建设置（团块就是这些体素块的集合、由小块堆组成的大块）

        /// <summary>
        /// 团块自动创建时高度范围限制（如果是3，表示高度Y最大是3个团块范围，如果团块是16边长，那么Y最大高度48）
        /// </summary>
        public static int HeightRange;
        /// <summary>
        /// 团块自动创建时距离限制（如果是8，则始终在玩家周围保证有8范围的团块）
        /// </summary>
        public static int ChunkSpawnDistance;
        /// <summary>
        /// 团块自动创建时的尺寸边长（如果是16，则团块由16*16*16个体素块组成）
        /// </summary>
        public static int ChunkSideLength;
        /// <summary>
        /// 团块自动摧毁时的判断距离（如果是3，则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁，摧毁时会存入档案以便玩家走过来时读取生成）
        /// </summary>
        public static int ChunkDespawnDistance;

        // 团块创建设置，GUI界面输入

        /// <summary>
        /// 团块自动创建时高度范围限制（如果是3，表示高度Y最大是3个团块范围，如果团块是16边长，那么Y最大高度48）
        /// </summary>
        public int lHeightRange;
        /// <summary>
        /// 团块自动创建时距离限制（如果是8，则始终在玩家周围保证有8范围的团块）
        /// </summary>
        public int lChunkSpawnDistance;
        /// <summary>
        /// 团块自动创建时的尺寸边长（如果是16，则团块由16*16*16个体素块组成）
        /// </summary>
        public int lChunkSideLength;
        /// <summary>
        /// 团块自动摧毁时的判断距离（如果是3，则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁，摧毁时会存入档案以便玩家走过来时读取生成）
        /// </summary>
        public int lChunkDespawnDistance;

        // 纹理设置

        /// <summary>
        /// 纹理单元倍率，形象说明的话相当于每个精灵在整张图片中的比例，默认是0.125说明整张图片被横竖均分8x8个纹理单元
        /// </summary>
        public static float TextureUnit;
        /// <summary>
        /// 每个纹理单元之间填充缝的大小（依然是整张图片的倍率，如图片是512x512像素，要在纹理各单元间填充1像素需填写1/512），填充以避免取到别的纹理单元
        /// </summary>
        public static float TexturePadding;

        // 纹理设置，GUI界面输入

        /// 纹理单元倍率，形象说明的话相当于每个精灵在整张图片中的比例，默认是0.125说明整张图片被横竖均分8x8个纹理单元
        /// </summary>
        public float lTextureUnit;
        /// 每个纹理单元之间填充缝的大小（依然是整张图片的倍率，如图片是512x512像素，要在纹理各单元间填充1像素需填写1/512），填充以避免取到别的纹理单元
        /// </summary>
        public float lTexturePadding;

        // 平台设置

        /// <summary>
        /// 目标帧率
        /// </summary>
        public static int TargetFPS;
        /// <summary>
        /// 团块保存上限
        /// </summary>
        public static int MaxChunkSaves;
        /// <summary>
        /// 团块数据请求上限
        /// </summary>
        public static int MaxChunkDataRequests;

        // 平台设置，GUI界面输入

        /// <summary>
        /// 目标帧率
        /// </summary>
        public int lTargetFPS;
        /// <summary>
        /// 团块保存上限（用于多人在线）
        /// </summary>
        public int lMaxChunkSaves;
        /// <summary>
        /// 团块数据请求上限（用于多人在线）
        /// </summary>
        public int lMaxChunkDataRequests;

        // 全局设置

        public static bool ShowBorderFaces;
        /// <summary>
        /// 产生碰撞体
        /// </summary>
        public static bool GenerateColliders;
        /// <summary>
        /// 发送镜头注视事件
        /// </summary>
        public static bool SendCameraLookEvents;
        /// <summary>
        /// 发送鼠标指针事件
        /// </summary>
        public static bool SendCursorEvents;
        /// <summary>
        /// 允许多人玩家
        /// </summary>
        public static bool EnableMultiplayer;
        /// <summary>
        /// 用于确定网络同步轨道位置的处理方式。
        /// 在多人游戏中，通常需要将物体的位置同步到其他客户端。轨道位置是指物体沿着一条路径或轨道移动时所处的位置。如果将MultiplayerTrackPosition字段设置为true，则表示该物体的位置将在网络上进行同步，且每个客户端都将跟踪该物体的轨道位置。
        /// 如果将MultiplayerTrackPosition字段设置为false，则表示该物体的位置不会在网络上进行同步，而客户端将不会跟踪其轨道位置。
        /// 在具有大量移动物体的多人游戏中，使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能。例如，如果某个物体在场景中静止不动，则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步。
        /// </summary>
        public static bool MultiplayerTrackPosition;
        //保存体素数据
        public static bool SaveVoxelData;
        //产生网格
        public static bool GenerateMeshes;

        // 全局设置，GUI界面输入

        /// <summary>
        /// 全局设置，GUI界面输入
        /// </summary>
        public bool lShowBorderFaces;
        /// <summary>
        /// 产生碰撞体
        /// </summary>
        public bool lGenerateColliders;
        /// <summary>
        /// 发送镜头注视事件
        /// </summary>
        public bool lSendCameraLookEvents;
        /// <summary>
        /// 发送鼠标指针事件
        /// </summary>
        public bool lSendCursorEvents;
        /// <summary>
        /// 允许多人玩家
        /// </summary>
        public bool lEnableMultiplayer;
        /// <summary>
        /// 用于确定网络同步轨道位置的处理方式。
        /// 在多人游戏中，通常需要将物体的位置同步到其他客户端。轨道位置是指物体沿着一条路径或轨道移动时所处的位置。如果将MultiplayerTrackPosition字段设置为true，则表示该物体的位置将在网络上进行同步，且每个客户端都将跟踪该物体的轨道位置。
        /// 如果将MultiplayerTrackPosition字段设置为false，则表示该物体的位置不会在网络上进行同步，而客户端将不会跟踪其轨道位置。
        /// 在具有大量移动物体的多人游戏中，使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能。例如，如果某个物体在场景中静止不动，则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步。
        /// </summary>
        public bool lMultiplayerTrackPosition;
        /// <summary>
        /// 保存体素数据
        /// </summary>
        public bool lSaveVoxelData;
        /// <summary>
        /// 产生网格
        /// </summary>
        public bool lGenerateMeshes;

        /// <summary>
        /// 团块超时
        /// </summary>
        public static float ChunkTimeout;
        /// <summary>
        /// 团块超时，GUI界面输入
        /// </summary>
        public float lChunkTimeout;
        /// <summary>
        /// 允许团块超时
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
        /// 团块缩放比例
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
            EngineInstance = this; //this关键字引用了当前类的一个实例，但它不能用在静态字段的初始化中，所以写在这
            //获取对象上的团块管理器组件实例（这里指名为"ChunkManager"的脚本类型组件实例化后的对象）
            ChunkManagerInstance = GetComponent<ChunkManager>();
            //读取GUI界面输入里填的世界名称
            WorldName = lWorldName;

            UpdateWorldPath();

            #region 初始化接口数据，将配置赋值给实际运作的字段属性

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

            //已加载的区域组（字典<string, string[]>）
            ChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
            //临时团块数据组（字典<string, string>）
            ChunkDataFiles.TempChunkData = new Dictionary<string, string>();

            //如GUI界面输入lChunkTimeout<= 0.00001，则不允许团块处理超时，否则允许超时且将GUI界面输入中所填超时数值赋值给属性字段
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
                //暂停编辑器运行
                Debug.Break();
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
        /// 设置世界名字（设置后世界种子将被重置为0，并且刷新用于档案的世界路径）
        /// </summary>
        /// <param name="worldName"></param>
        public static void SetWorldName(string worldName)
        {
            WorldName = worldName;
            WorldSeed = 0;
            UpdateWorldPath();
        }

        /// <summary>
        /// 获取世界种子（如存在则从文件中读取，否则创建一个新种子并将其保存到文件中）
        /// </summary>
        public static void GetSeed()
        { // reads the world seed from file if it exists, else creates a new seed and saves it to file

            //if (Application.isWebPlayer) { // don't save to file if webplayer			
            //	Engine.WorldSeed = Random.Range (ushort.MinValue, ushort.MaxValue);
            //	return;
            //}		

            if (File.Exists(WorldPath + "seed"))
            {
                //存在种子则读取
                StreamReader reader = new StreamReader(WorldPath + "seed");
                WorldSeed = int.Parse(reader.ReadToEnd());
                reader.Close();
            }
            else
            {
                //循环的目的是确保生成的 WorldSeed 值不为 0
                while (WorldSeed == 0)
                {
                    WorldSeed = Random.Range(ushort.MinValue, ushort.MaxValue);
                }
                Directory.CreateDirectory(WorldPath); //如文件夹存在则不会创建新的，该动作不会抛出异常无需用if (!Directory.Exists(WorldPath))判断
                StreamWriter writer = new StreamWriter(WorldPath + "seed"); //指定文件路径，创建一个写入流
                writer.Write(WorldSeed.ToString()); //为文件写入内容字符串
                //在执行 Close 方法之前调用 Flush 方法可以确保所有数据在关闭文件之前被正确地写入。
                writer.Flush();
                writer.Close();
                //虽然在大多数情况下，调用 Close 方法时会自动调用 Flush 方法，但在某些特殊情况下，例如当文件系统繁忙或者出现其他问题时，数据可能无法正确地写入文件
                //在这种情况下显式调用 Flush 方法可确保数据在关闭文件之前被正确地写入，确保在程序异常情况下数据不会丢失（像关闭文件之前程序崩溃或出现异常）
            }
        }

        /// <summary>
        /// 保存多个帧的数据
        /// </summary>
        public static void SaveWorld()
        { // saves the data over multiple frames

            //实例调用继承自父类的方法来异步处理存档（使用了Unity的协程）
            EngineInstance.StartCoroutine(ChunkDataFiles.SaveAllChunks());
        }

        /// <summary>
        /// 将TempChunkData中的数据写入区域文件
        /// </summary>
        public static void SaveWorldInstant()
        { // writes data from TempChunkData into region files

            ChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====	

        /// <summary>
        /// 获取体素的游戏物体对象
        /// </summary>
        /// <param name="voxelId">体素ID</param>
        /// <returns></returns>
        public static GameObject GetVoxelGameObject(ushort voxelId)
        {
            try
            {
                if (voxelId == ushort.MaxValue) voxelId = 0;
                GameObject voxelObject = Blocks[voxelId];
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
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return Blocks[0];
            }
        }

        /// <summary>
        /// 获取体素类型
        /// </summary>
        /// <param name="voxelId">体素ID</param>
        /// <returns></returns>
        public static Voxel GetVoxelType(ushort voxelId)
        {
            try
            {
                if (voxelId == ushort.MaxValue) voxelId = 0;
                Voxel voxel = Blocks[(int)voxelId].GetComponent<Voxel>();
                if (voxel == null)
                {
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!");
                    return null;
                }
                else
                {
                    return voxel;
                }

            }
            catch (System.Exception)
            {
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return null;
            }
        }

        /// <summary>
        /// 一个光线投射，它返回触碰到的体素信息（存储体素的索引、体素所在团块及其在团块的位置信息）
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static VoxelInfo VoxelRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        { // a raycast which returns the index of the hit voxel and the gameobject of the hit chunk

            RaycastHit hit = new RaycastHit(); //创建射线投射器hit

            //利用物理引擎投射光线，hit的绘制从origin出发沿direction方向，最大距离range
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                //如果从hit碰撞体里能获取到团块或团块扩展组件
                if (hit.collider.GetComponent<Chunk>() != null
                    || hit.collider.GetComponent<ChunkExtension>() != null)
                { // check if we're actually hitting a chunk.检查我们是否真的击中了团块

                    GameObject hitObject = hit.collider.gameObject; //从碰撞体中获得游戏物体对象

                    if (hitObject.GetComponent<ChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk.如果我们击中的是网状容器而不是团块（判断依据是网状容器拥有大块扩展组件），注意网格容器是团块大小的，虽是团块子对象但它不是体素块）
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object.将网格容器替换为实际的团块对象（它是网格容器对象的父级对象）
                    }

                    //通过射线投射器坐标、射线方向与接触面形成的法线方向来获取体素索引（不获取相邻体素）
                    Index hitIndex = hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false);

                    //忽略透明
                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent voxel is hit.当一个透明体素被击中时，再次通过光线投射穿透透明体素
                        ushort hitVoxel = hitObject.GetComponent<Chunk>().GetVoxel(hitIndex.x, hitIndex.y, hitIndex.z); //通过体素索引从团块组件获得体素（代号）
                        //如果命中的体素类型的VTransparency属性=透明
                        if (GetVoxelType(hitVoxel).VTransparency != Transparency.solid)
                        {
                            Vector3 newOrigin = hit.point; //存储hit坐标
                            newOrigin.y -= 0.5f; // push the new raycast down a bit.将hit向下移动0.5
                            return VoxelRaycast(newOrigin, Vector3.down, range - hit.distance, true); //返回体素信息
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
        /// 一个光线投射，它返回触碰到的体素信息（存储体素的索引、体素所在团块及其在团块的位置信息）
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
        /// 将位置转换成团块索引
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
        /// 将位置转换成团块
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
        /// 将位置转换成体素信息
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
        /// 将体素信息转换成位置
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
        /// <param name="voxel">体素（代号）</param>
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
