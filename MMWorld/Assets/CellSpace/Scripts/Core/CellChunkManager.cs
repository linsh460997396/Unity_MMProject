using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// Controls spawning and destroying chunks

namespace CellSpace
{
    /// <summary>
    /// 团块管理器组件：控制团块创建和摧毁。组件用法：Unity中随便新建一个空对象“CPEngine”，把脚本拖到组件位置即挂载（Unity要求一个cs文件只能一个类，且类名须与文件名一致）
    /// </summary>
    public class CellChunkManager : MonoBehaviour
    {

        /// <summary>
        /// 团块预制体（CellChunk Prefab），在数据上它已经是个实例，可看成活动内存模板数据，但尚未被用于继续实例化到场景（待成为场景实例）
        /// </summary>
        public GameObject ChunkObject;

        // chunk lists

        /// <summary>
        /// 团块组（字典）：存储场景中所有Chunk实例的引用，团块通过Awake调用ChunkManager.RegisterChunk将自己添加到字典中，字符串key对应Chunk的索引并遵循“pixelX,pixelY,z”的格式）
        /// </summary>
        public static Dictionary<string, CellChunk> Chunks;

        /// <summary>
        /// 团块更新队列（列表存储按更新优先级排序的团块，在ProcessChunkQueue循环中处理）
        /// </summary>
        private static List<CellChunk> ChunkUpdateQueue; // stores chunks ordered by update priority. Processed in the ProcessChunkQueue loop
        /// <summary>
        /// 团块销毁列表（存放在SpawnChunks结束时销毁的chunk）
        /// </summary>
        private static List<CellChunk> ChunksToDestroy; // chunks to be destroyed at the end of SpawnChunks

        /// <summary>
        /// 当前帧的团块已保存数量（用于跟每帧的团块保存上限MaxChunkSaves进行比对）
        /// </summary>
        public static int SavesThisFrame;


        // global flags

        /// <summary>
        /// 如果ChunkManager当前正在生成块，则为true
        /// </summary>
        public static bool SpawningChunks; // true if the CellChunkManager is currently spawning chunks
        /// <summary>
        /// 当为true时，当前的SpawnChunks序列动作将被终止(然后将其设置为false)
        /// </summary>
        public static bool StopSpawning; // when true, the current SpawnChunks sequence is aborted (and this is set back to false afterwards)
        /// <summary>
        /// 团块管理器初始化状态
        /// </summary>
        public static bool Initialized;

        // local flags

        /// <summary>
        /// 决定是否可以开始创建团块
        /// </summary>
        private bool Done;
        /// <summary>
        /// 最后请求的索引
        /// </summary>
        private CPIndex LastRequest;
        /// <summary>
        /// 目标游戏周期 = 1f / CPEngine.TargetFPS
        /// </summary>
        private float targetFrameDuration;
        /// <summary>
        /// 帧计时器（统计用时）
        /// </summary>
        private Stopwatch frameStopwatch;
        /// <summary>
        /// 团块创建队列
        /// </summary>
        private int SpawnQueue;

        void Start()
        {
            //设定游戏周期（每帧秒数）
            targetFrameDuration = 1f / CPEngine.TargetFPS;
            //建立团块组（字典）
            Chunks = new Dictionary<string, CellChunk>();
            //建立团块更新列表
            ChunkUpdateQueue = new List<CellChunk>();
            //建立帧计时器（负责记录处理用时）
            frameStopwatch = new Stopwatch();

            //set correct scale of trigger collider and additional mesh collider.设置触发器碰撞体和附加网格碰撞器的正确比例

            //设置团块缩放比例为团块实例当前的缩放比例
            CPEngine.ChunkScale = ChunkObject.transform.localScale;
            //设置附加网格碰撞器的正确比例
            ChunkObject.GetComponent<CellChunk>().MeshContainer.transform.localScale = ChunkObject.transform.localScale;
            //设置触发器碰撞体的正确比例
            ChunkObject.GetComponent<CellChunk>().ChunkCollider.transform.localScale = ChunkObject.transform.localScale;

            //基本设置完成
            Done = true;
            //团块创建状态重置为假
            SpawningChunks = false;
            //团块管理器初始化完成
            Initialized = true;
        }

        /// <summary>
        /// 重启帧计时器（统计每帧处理用时）
        /// </summary>
        private void ResetFrameStopwatch()
        {
            frameStopwatch.Stop();
            frameStopwatch.Reset();
            frameStopwatch.Start();
        }

        /// <summary>
        /// 添加团块到更新队列
        /// </summary>
        /// <param name="chunk"></param>
        public static void AddChunkToUpdateQueue(CellChunk chunk)
        {
            //检查团块更新队列中是已否包含对象团块
            if (ChunkUpdateQueue.Contains(chunk) == false)
            {
                //团块更新队列中添加对象团块
                ChunkUpdateQueue.Add(chunk);
            }
        }

        /// <summary>
        /// 处理团块队列（从SpawnChunks循环调用来更新块网格）：
        /// 更新第一个块并将其从队列中删除,当前团块不为空且没有被禁用网格时重新建立网格,当前团块的Fresh属性置为假并从队列中删除它
        /// </summary>
        private void ProcessChunkQueue()
        { // called from the SpawnChunks loop to update chunk meshes

            // update the first chunk and remove it from the queue.更新第一个块并将其从队列中删除
            CellChunk currentChunk = ChunkUpdateQueue[0];

            if (!currentChunk.Empty && !currentChunk.DisableMesh)
            {
                //当前团块不为空且没有被禁用网格时重新建立网格
                currentChunk.RebuildMesh();
            }
            //当前团块的Fresh属性置为假
            currentChunk.Fresh = false;
            //从队列中删除它
            ChunkUpdateQueue.RemoveAt(0);
        }

        /// <summary>
        /// 处理团块队列循环（协程）的激活状态
        /// </summary>
        private bool ProcessChunkQueueLoopActive;
        /// <summary>
        /// 处理团块队列循环（协程），当SpawnChunks未运行时从Update调用。
        /// 当有团块更新队列（计数不为0）且团块创建状态为真且没有手动强行终止时，处理团块队列。
        /// 如果帧计时器已流逝时间超过目标游戏周期则进入协程，在渲染完所有摄像机和GUI后等待该帧结束。
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProcessChunkQueueLoop()
        { // called from Update when SpawnChunks is not running
            ProcessChunkQueueLoopActive = true;
            while (ChunkUpdateQueue.Count > 0 && !SpawningChunks && !StopSpawning)
            {
                //当有团块更新队列（计数不为0）且团块创建状态为真且没有手动强行终止时，处理团块队列
                ProcessChunkQueue();
                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                {
                    //如果帧计时器已流逝时间超过目标游戏周期则进入协程，在渲染完所有摄像机和GUI后等待该帧结束
                    yield return new WaitForEndOfFrame();
                }
            }
            ProcessChunkQueueLoopActive = false;
        }

        /// <summary>
        /// 注册团块（将对团块的引用添加到全局团块组字典中，以ChunkIndex为键区，Chunk为值），这是由团块在Awake函数中自动完成的。
        /// </summary>
        /// <param name="chunk"></param>
        public static void RegisterChunk(CellChunk chunk)
        { // adds a reference to the chunk to the global chunk list
            Chunks.Add(chunk.ChunkIndex.ToString(), chunk);
        }

        /// <summary>
        /// 注销团块（从全局团块字典中），这是在团块被销毁时自动完成的。
        /// </summary>
        /// <param name="chunk"></param>
        public static void UnRegisterChunk(CellChunk chunk)
        {
            Chunks.Remove(chunk.ChunkIndex.ToString());
        }

        /// <summary>
        /// 返回指定Index(pixelX, pixelY, z)的团块的游戏物体对象，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject GetChunk(int x, int y, int z)
        { // returns the gameObject of the chunk with the specified pixelX,pixelY,z, or returns null if the object is not instantiated
            if (CPEngine.HorizontalMode)
            {
                return GetChunk(new CPIndex(x, y));
            }
            else
            {
                return GetChunk(new CPIndex(x, y, z));
            }
        }
        /// <summary>
        /// 返回指定Index(pixelX, pixelY, z)的团块的游戏物体对象，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static GameObject GetChunk(int x, int y)
        { // returns the gameObject of the chunk with the specified pixelX,pixelY, or returns null if the object is not instantiated
            return GetChunk(new CPIndex(x, y));
        }

        /// <summary>
        /// 返回指定Index的团块的游戏物体对象，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static GameObject GetChunk(CPIndex index)
        {
            CellChunk chunk = GetChunkComponent(index);
            if (chunk == null)
            {
                return null;
            }
            else
            {
                return chunk.gameObject;
            }
        }

        /// <summary>
        /// 返回指定Index(pixelX, pixelY, z)的团块，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static CellChunk GetChunkComponent(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                return GetChunkComponent(new CPIndex(x, y));
            }
            else
            {
                return GetChunkComponent(new CPIndex(x, y, z));
            }
        }
        /// <summary>
        /// 返回指定Index(pixelX, pixelY)的团块，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static CellChunk GetChunkComponent(int x, int y)
        {
            return GetChunkComponent(new CPIndex(x, y));
        }

        /// <summary>
        /// 返回指定Index的团块，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static CellChunk GetChunkComponent(CPIndex index)
        {
            string indexString = index.ToString();
            if (Chunks.ContainsKey(indexString))
            {
                return Chunks[indexString];
            }
            else
            {
                return null;
            }
        }


        // ==== spawn chunks functions ====

        /// <summary>
        /// 生成带有索引(pixelX,pixelY,z)的单个团块并返回该游戏对象（如已生成则返回已生成的游戏对象）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject SpawnChunk(int x, int y, int z)
        { // spawns a single chunk (only if it's not already spawned)
            if (CPEngine.HorizontalMode)
            {
                return SpawnChunk(x, y);
            }
            else
            {
                GameObject chunk = GetChunk(x, y, z);
                if (chunk == null)
                {
                    return CPEngine.ChunkManagerInstance.DoSpawnChunk(new CPIndex(x, y, z));
                }
                else return chunk;
            }
        }
        /// <summary>
        /// 生成带有索引(pixelX,pixelY)的单个团块并返回该游戏对象（如已生成则返回已生成的游戏对象）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject SpawnChunk(int x, int y)
        { // spawns a single chunk (only if it's not already spawned)
            GameObject chunk = GetChunk(x, y);
            if (chunk == null)
            {
                return CPEngine.ChunkManagerInstance.DoSpawnChunk(new CPIndex(x, y));
            }
            else return chunk;
        }

        /// <summary>
        /// 生成带有索引的单个团块并返回该游戏对象（如已生成则返回已生成的游戏对象）
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns></returns>
        public static GameObject SpawnChunk(CPIndex index)
        { // spawns a single chunk (only if it's not already spawned)

            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                return CPEngine.ChunkManagerInstance.DoSpawnChunk(index);
            }
            else return chunk;
        }

        /// <summary>
        /// 【服务器】用索引生成单个团块的游戏对象并返回（索引点将作为团块在世界的位置），前提是该索引位置尚未实例化团块对象。本函数会禁用网格生成并启用超时（由服务器在多人游戏中使用），但若团块实例已存在则不会禁用网格生成和启用超时，仅简单地返回该对象。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns>返回团块的游戏物体对象</returns>
        public static GameObject SpawnChunkFromServer(CPIndex index)
        { // spawns a chunk and disables mesh generation and enables timeout (used by the server in multiplayer)
            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                chunk = CPEngine.ChunkManagerInstance.DoSpawnChunk(index);
                CellChunk chunkComponent = chunk.GetComponent<CellChunk>();
                chunkComponent.EnableTimeout = true;
                chunkComponent.DisableMesh = true;
                return chunk;
            }
            else return chunk; // don'transform disable mesh generation and don'transform enable timeout for chunks that are already spawned
        }

        /// <summary>
        /// 【服务器】用索引(pixelX,pixelY,z)生成单个团块的游戏对象并返回（索引点将作为团块在世界的位置，2D模式中z可为0），前提是该索引位置尚未实例化团块对象。本函数会禁用网格生成并启用超时（由服务器在多人游戏中使用），但若团块实例已存在则不会禁用网格生成和启用超时，仅简单地返回该对象。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns>返回团块的游戏物体对象</returns>
        public static GameObject SpawnChunkFromServer(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                return SpawnChunkFromServer(new CPIndex(x, y));
            }
            else
            {
                return SpawnChunkFromServer(new CPIndex(x, y, z));
            }
        }
        /// <summary>
        /// 【服务器】用索引(pixelX,pixelY)生成单个团块的游戏对象并返回（索引点将作为团块在世界的位置，2D模式中z可为0），前提是该索引位置尚未实例化团块对象。本函数会禁用网格生成并启用超时（由服务器在多人游戏中使用），但若团块实例已存在则不会禁用网格生成和启用超时，仅简单地返回该对象。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns>返回团块的游戏物体对象</returns>
        public static GameObject SpawnChunkFromServer(int x, int y)
        {
            return SpawnChunkFromServer(new CPIndex(x, y));
        }

        /// <summary>
        /// 创建团块具体动作：用团块预制体在团块索引位置创建团块游戏物体，获取对象上的团块组件，将团块组件对象添加到更队列，最后返回团块游戏物体。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns></returns>
        GameObject DoSpawnChunk(CPIndex index)
        {
            //用团块预制体在团块索引位置创建团块游戏物体
            GameObject chunkObject = Instantiate(ChunkObject, index.ToVector3(), transform.rotation);
            //获取对象上的团块组件
            CellChunk chunk = chunkObject.GetComponent<CellChunk>();
            //将团块组件对象添加到更队列
            AddChunkToUpdateQueue(chunk);
            //返回团块游戏物体
            return chunkObject;
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时取世界位置(pixelX,pixelY,z)转换为团块索引，然后尝试以该索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(float x, float y, float z)
        { // take world pos, convert to chunk index.取世界位置，转换为块索引
            CPIndex index;
            if (CPEngine.HorizontalMode)
            {
                index = CPEngine.PositionToChunkIndex(new Vector3(x, y, 0f));
            }
            else
            {
                index = CPEngine.PositionToChunkIndex(new Vector3(x, y, z));
            }
            CPEngine.ChunkManagerInstance.TrySpawnChunks(index);
        }
        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时取世界位置(pixelX,pixelY,0f)转换为团块索引，然后尝试以该索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用2个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SpawnChunks(float x, float y)
        { // take world pos, convert to chunk index.取世界位置，转换为块索引
            CPIndex index = CPEngine.PositionToChunkIndex(new Vector3(x, y, 0f));
            CPEngine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时取世界位置转换为团块索引，然后尝试以该索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="position">支持2D/3D的Vector</param>
        public static void SpawnChunks(Vector3 position)
        {
            CPIndex index = CPEngine.PositionToChunkIndex(position);
            CPEngine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引(pixelX,pixelY,z)为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// 世界位置转团块索引示范：currentPos = CPEngine.PositionToChunkIndex(engineTransform.position);
        /// </summary>
        /// <param name="x">团块索引</param>
        /// <param name="y">团块索引</param>
        /// <param name="z">团块索引</param>
        public static void SpawnChunks(int x, int y, int z)
        { // take chunk index, no conversion needed
            if (CPEngine.HorizontalMode)
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(x, y);
            }
            else
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(x, y, z);
            }

        }
        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="index"></param>
        public static void SpawnChunks(CPIndex index)
        {
            if (CPEngine.HorizontalMode)
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y);
            }
            else
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y, index.z);
            }
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="index"></param>
        private void TrySpawnChunks(CPIndex index)
        {
            if (CPEngine.HorizontalMode)
            {
                TrySpawnChunks(index.x, index.y);
            }
            else
            {
                TrySpawnChunks(index.x, index.y, index.z);
            }
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引(pixelX,pixelY,z)为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void TrySpawnChunks(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                TrySpawnChunks(x, y);
            }
            else
            {
                if (Done == true)
                { //如果当前没有生成团块动作，那就正常地生成它们
                    StartSpawnChunks(x, y, z);
                }
                else
                {
                    //如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点
                    LastRequest = new CPIndex(x, y, z);
                    SpawnQueue = 1; //创建队列计数为1
                    StopSpawning = true; //主动打断创建状态（SpawnChunks序列将被终止）
                    ChunkUpdateQueue.Clear(); //清除团块更新队列
                }
            }
        }
        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引(pixelX,pixelY)为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用2个int或1个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void TrySpawnChunks(int x, int y)
        {
            if (Done == true)
            { // if we're not spawning chunks at the moment, just spawn them normally.如果当前没有生成团块动作，那就正常地生成它们
                StartSpawnChunks(x, y);
            }
            else
            {
                //if we are spawning chunks already, flag to spawn again once the previous round is finished using the last requested position as origin.
                //如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点
                LastRequest = new CPIndex(x, y);
                SpawnQueue = 1; //创建队列计数为1
                StopSpawning = true; //主动打断创建状态（SpawnChunks序列将被终止）
                ChunkUpdateQueue.Clear(); //清除团块更新队列
            }
        }

        /// <summary>
        /// 每帧监听并执行相应团块（地形）创建任务
        /// </summary>
        public void Update()
        {
            //当有团块创建队列且团块管理器同意开始创建团块时
            if (SpawnQueue == 1 && Done == true)
            { // if there is a chunk spawn queued up, and if the previous one has finished, run the queued chunk spawn.当有新的团块创建任务，若团块管理器通知前一个已完成，那就开始下个创建团块任务

                //团块创建队列置零
                SpawnQueue = 0;
                if (CPEngine.HorizontalMode)
                {
                    //开始创建团块（地形）：SpawningChunks=真，团块管理器调为禁止创建团块（Done=false，防止Update中开始下个创建团块动作而是转为步进当前这个团块创建任务直到完成），最后启动协程来创建缺失的团块
                    StartSpawnChunks(LastRequest.x, LastRequest.y);
                }
                else
                {
                    //开始创建团块（地形）：SpawningChunks=真，团块管理器调为禁止创建团块（Done=false，防止Update中开始下个创建团块动作而是转为步进当前这个团块创建任务直到完成），最后启动协程来创建缺失的团块
                    StartSpawnChunks(LastRequest.x, LastRequest.y, LastRequest.z);
                }
            }
            // if not currently spawning chunks, process any queued chunks here instead.若当前没新的团块创建任务，在这里步进当前团块创建任务直到完成
            // （但没利用Update来加速步进而是启用新协程自动步进，会在完成时设定状态来开始下一个协程）
            if (!SpawningChunks && !ProcessChunkQueueLoopActive && ChunkUpdateQueue != null && ChunkUpdateQueue.Count > 0)
            {
                //启动协程来处理团块创建任务直到完成
                StartCoroutine(ProcessChunkQueueLoop());
            }
            //重启帧计时器
            ResetFrameStopwatch();
        }

        /// <summary>
        /// 开始创建团块（地形）：SpawningChunks=真，团块管理器调为禁止创建团块（Done=false，防止Update中开始下个创建团块动作而是转为步进当前这个团块创建任务直到完成），最后启动协程来创建缺失的团块
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        private void StartSpawnChunks(int originX, int originY, int originZ)
        {
            //团块创建状态为真
            SpawningChunks = true;
            //团块管理器调为禁止创建团块（防止Update中开始下个创建团块动作而是转为步进当前这个团块创建任务直到完成）
            Done = false;
            //获取团块创建距离
            int range = CPEngine.ChunkSpawnDistance;
            //启动协程来创建缺失的团块
            if (CPEngine.HorizontalMode)
            {
                StartCoroutine(SpawnMissingChunks(originX, originY, range));
            }
            else
            {
                StartCoroutine(SpawnMissingChunks(originX, originY, originZ, range));
            }
        }
        /// <summary>
        /// 开始创建团块（地形）：SpawningChunks=真，团块管理器调为禁止创建团块（Done=false，防止Update中开始下个创建团块动作而是转为步进当前这个团块创建任务直到完成），最后启动协程来创建缺失的团块
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        private void StartSpawnChunks(int originX, int originY)
        {
            //团块创建状态为真
            SpawningChunks = true;
            //团块管理器调为禁止创建团块（防止Update中开始下个创建团块动作而是转为步进当前这个团块创建任务直到完成）
            Done = false;
            //获取团块创建距离
            int range = CPEngine.ChunkSpawnDistance;
            //启动协程来创建缺失的团块
            StartCoroutine(SpawnMissingChunks(originX, originY, range));
        }

        /// <summary>
        /// [协程]创建缺失的团块（会先将距离过远的团块添加到摧毁队列）
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int originZ, int range)
        {
            if (CPEngine.HorizontalMode)
            {
                SpawnMissingChunks(originX, originY, range);
            }
            else
            {
                //获取团块创建高度范围限值（Y轴）
                int heightRange = CPEngine.HeightRange;
                //清除团块更新队列，它将在下面的循环中以正确的顺序重新填充
                ChunkUpdateQueue = new List<CellChunk>();
                //标记过的团块不在移除范围内
                ChunksToDestroy = new List<CellChunk>();
                //遍历团块组中每个存储的团块实例，验证是否需要摧毁三维距离过远的
                foreach (CellChunk chunk in Chunks.Values)
                {
                    //团块索引点（世界位置）与角色在世界位置如超过团块创建距离+ChunkDespawnDistance（默认8+3=11）
                    if (Vector2.Distance(new Vector2(chunk.ChunkIndex.x, chunk.ChunkIndex.z), new Vector2(originX, originZ)) > range + CPEngine.ChunkDespawnDistance)
                    {
                        //将这些XZ平面距离过远的团块添加到摧毁队列
                        ChunksToDestroy.Add(chunk);
                    }
                    //如果团块索引点（世界位置）的高度超过团块创建距离+ChunkDespawnDistance（默认8+3=11）
                    else if (Mathf.Abs(chunk.ChunkIndex.y - originY) > range + CPEngine.ChunkDespawnDistance)
                    {
                        //将这些垂直距离（Y轴高度）过远的团块添加到摧毁队列
                        ChunksToDestroy.Add(chunk);
                    }
                }
                //主循环开始创建缺失的团块
                for (int currentLoop = 0; currentLoop <= range; currentLoop++)
                {//遍历团块创建距离（0是原地块，1是周围扩展一块...以此推类）
                    for (var x = originX - currentLoop; x <= originX + currentLoop; x++)
                    { //遍历范围内所有可能的团块索引
                        for (var y = originY - currentLoop; y <= originY + currentLoop; y++)
                        {
                            for (var z = originZ - currentLoop; z <= originZ + currentLoop; z++)
                            {
                                //索引的高度绝对值不能超过高度范围，那么这个索引才是有效的
                                if (Mathf.Abs(y) <= heightRange)
                                { //验证是否跳过高度范围之外的团块
                                    //角落团块在XZ平面是根号2倍边长距离 < 1.3倍，所以会达不到那个索引，同时验证是否KeepOneChunk（至少保证原点插入1个团块）
                                    if (Mathf.Abs(originX - x) + Mathf.Abs(originZ - z) < range * 1.3f ||(CPEngine.KeepOneChunk && x == 0 && y == 0 && z == 0)) 
                                    { //这里跳过了距离玩家过远的角落团块，若触发唯一团块创建条件，除非禁用邻团创建，否则除原点团块外周围依然会创建6个邻团（透明状态）

                                        //当团块更新队列不为空时
                                        while (ChunkUpdateQueue.Count > 0)
                                        {
                                            //处理团块队列
                                            ProcessChunkQueue();
                                            //如果帧计时器经过时间超过了目标帧率设定的时间
                                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                            {
                                                //协程暂停让当前帧进行渲染直到下次继续剩余动作
                                                yield return new WaitForEndOfFrame();
                                            }
                                        }
                                        //返回指定索引的团块
                                        CellChunk currentChunk = GetChunkComponent(x, y, z);
                                        //已存在但尚未构建网格的团块应添加到更新队列中
                                        if (currentChunk != null)
                                        {
                                            //服务器生成的没有网格的团块应更改为常规团块（进行属性修改）
                                            if (currentChunk.DisableMesh || currentChunk.EnableTimeout)
                                            {
                                                //禁用网格=假
                                                currentChunk.DisableMesh = false;
                                                //允许超时=假
                                                currentChunk.EnableTimeout = false;
                                                //流程新鲜状态=真
                                                currentChunk.Fresh = true;
                                            }
                                            //流程新鲜状态为真时
                                            if (currentChunk.Fresh)
                                            {
                                                //刷出相邻团块
                                                for (int d = 0; d < 6; d++)
                                                {
                                                    CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                                    GameObject neighborChunk = GetChunk(neighborIndex);
                                                    if (neighborChunk == null)
                                                    {
                                                        neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                                    }
                                                    //总是添加相邻团块到NeighborChunks以防它还没有在那
                                                    currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();
                                                    //如果帧计时器经过时间超过了目标帧率设定的时间，协程暂停让当前帧进行渲染直到下次继续剩余动作
                                                    if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                                    {
                                                        yield return new WaitForEndOfFrame();
                                                    }
                                                    //如果团块管理器通知"终止团块创建序列"
                                                    if (StopSpawning)
                                                    {
                                                        //结束序列动作
                                                        EndSequence();
                                                        yield break;
                                                    }
                                                }

                                                //当前团块不存在
                                                if (currentChunk != null)
                                                    //当团块和所有已知邻居的数据准备就绪时，将团块添加到更新队列
                                                    currentChunk.AddToQueueWhenReady();
                                            }
                                        }
                                        else
                                        {
                                            //如果chunk不存在，则创建新的chunk(当它的数据准备好时会将自己添加到更新队列中)
                                            // spawn chunk.团块创建
                                            GameObject newChunk = Instantiate(ChunkObject, new Vector3(x, y, z), transform.rotation); // Spawn a new chunk.团块实例化到场景（是从几乎空的团块预制体创建的）
                                            currentChunk = newChunk.GetComponent<CellChunk>();
                                            // spawn neighbor chunks if they're not spawned yet.当相邻团块没在创建时创建它们，循环6次代表6个朝向（枚举整数索引0-5）
                                            for (int d = 0; d < 6; d++)
                                            {
                                                //返回与给定方向上index相邻的新索引
                                                CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                                //获取相邻团块
                                                GameObject neighborChunk = GetChunk(neighborIndex);
                                                if (neighborChunk == null)
                                                {
                                                    //相邻团块不存在则采用团块预制体进行实例化
                                                    neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                                }
                                                // always add the neighbor to NeighborChunks, in case it's not there already
                                                //总是添加相邻团块到NeighborChunks属性数组，以防它还没有在那里
                                                currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();
                                                // continue loop in next frame if the current frame time is exceeded.如果超出当前帧时间，协程暂停让当前帧进行渲染直到下次继续剩余动作
                                                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                                {
                                                    yield return new WaitForEndOfFrame();
                                                }
                                                //如果团块管理器通知"终止团块创建序列"
                                                if (StopSpawning)
                                                {
                                                    //结束序列动作
                                                    EndSequence();
                                                    yield break;
                                                }
                                            }
                                            //当前团块不存在
                                            if (currentChunk != null)
                                                //当团块和所有已知邻居的数据准备就绪时，将团块添加到更新队列
                                                currentChunk.AddToQueueWhenReady();
                                        }
                                    }
                                }

                                // continue loop in next frame if the current frame time is exceeded.如果超出当前帧时间，协程暂停让当前帧进行渲染直到下次继续剩余动作
                                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                {
                                    yield return new WaitForEndOfFrame();
                                }
                                //如果团块管理器通知"终止团块创建序列"
                                if (StopSpawning)
                                {
                                    //结束序列动作
                                    EndSequence();
                                    yield break;
                                }
                            }
                        }
                    }
                }
                //协程暂停让当前帧进行渲染直到下次继续剩余动作
                yield return new WaitForEndOfFrame();
                //结束序列动作
                EndSequence();
            }

        }
        /// <summary>
        /// [协程]创建缺失的团块（会先将距离过远的团块添加到摧毁队列）
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int range)
        {
            //获取团块创建高度范围限值
            int heightRange = CPEngine.HeightRange;
            // clear update queue - it will be repopulated again in the correct order in the following loop
            //清除团块更新队列 - 它将在下面的循环中以正确的顺序重新填充
            ChunkUpdateQueue = new List<CellChunk>();
            // flag chunks not in range for removal.标记过的团块不在移除范围内
            ChunksToDestroy = new List<CellChunk>();
            //遍历团块组中每个存储的团块实例
            foreach (CellChunk chunk in Chunks.Values)
            {
                //团块索引点（世界位置）与角色在世界位置如超过团块创建距离+ChunkDespawnDistance（默认8+3=11）
                if (Vector2.Distance(new Vector2(chunk.ChunkIndex.x, chunk.ChunkIndex.y), new Vector2(originX, originY)) > range + CPEngine.ChunkDespawnDistance)
                {
                    //将这些XY平面距离过远的团块添加到摧毁队列
                    ChunksToDestroy.Add(chunk);
                }
                //如果团块索引点（世界位置）的高度超过团块创建距离+ChunkDespawnDistance（默认8+3=11）
                else if (Mathf.Abs(chunk.ChunkIndex.z) > range + CPEngine.ChunkDespawnDistance)
                { // destroy chunks outside of vertical range.摧毁垂直距离过远的团块
                    ChunksToDestroy.Add(chunk);
                }
            }

            //主循环开始创建缺失的团块
            for (int currentLoop = 0; currentLoop <= range; currentLoop++)
            {
                for (var x = originX - currentLoop; x <= originX + currentLoop; x++)
                { // iterate through all potential chunk indexes within range.遍历范围内所有可能的团块索引
                    for (var y = originY - currentLoop; y <= originY + currentLoop; y++)
                    {

                        //索引的高度绝对值不能超过高度范围（比如3个团块高度），那么这个索引才是有效的
                        if (Mathf.Abs(y) <= heightRange)
                        { // skip chunks outside of height range.这里跳过了高度范围之外的团块
                            //角落团块在XZ平面是根号2倍边长距离 < 1.3倍，所以会达不到那个索引，同时验证是否KeepOneChunk（至少保证原点插入1个团块）
                            if (Mathf.Abs(originX - x) + Mathf.Abs(originY - y) < range * 1.3f || (CPEngine.KeepOneChunk && x==0 && y == 0))
                            { // skip corners.这里跳过了距离玩家过远的角落团块

                                //当团块更新队列不为空时
                                while (ChunkUpdateQueue.Count > 0)
                                {
                                    //处理团块队列
                                    ProcessChunkQueue();
                                    //如果帧计时器经过时间超过了目标帧率设定的时间
                                    if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                    {
                                        //协程暂停让当前帧进行渲染直到下次继续剩余动作
                                        yield return new WaitForEndOfFrame();
                                    }
                                }

                                //返回指定索引的团块
                                CellChunk currentChunk = GetChunkComponent(x, y);


                                // chunks that already exist but haven'transform had their mesh built yet should be added to the update queue.已存在但尚未构建网格的团块应添加到更新队列中
                                if (currentChunk != null)
                                {

                                    // chunks without meshes spawned by server should be changed to regular chunks.服务器生成的没有网格的团块应更改为常规团块（进行属性修改）
                                    if (currentChunk.DisableMesh || currentChunk.EnableTimeout)
                                    {
                                        //禁用网格=假
                                        currentChunk.DisableMesh = false;
                                        //允许超时=假
                                        currentChunk.EnableTimeout = false;
                                        //流程新鲜状态=真
                                        currentChunk.Fresh = true;
                                    }

                                    //流程新鲜状态为真时
                                    if (currentChunk.Fresh)
                                    {
                                        // spawn neighbor chunks.刷出相邻团块（横版模式只要上下右左）
                                        for (int d = 0; d < 4; d++)
                                        {
                                            CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                            GameObject neighborChunk = GetChunk(neighborIndex);
                                            if (neighborChunk == null)
                                            {
                                                neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                            }

                                            // always add the neighbor to NeighborChunks, in case it's not there already.总是添加相邻团块到NeighborChunks，以防它还没有在那
                                            currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();

                                            // continue loop in next frame if the current frame time is exceeded.如果帧计时器经过时间超过了目标帧率设定的时间，协程暂停让当前帧进行渲染直到下次继续剩余动作
                                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                            {
                                                yield return new WaitForEndOfFrame();
                                            }
                                            //如果团块管理器通知"终止团块创建序列"
                                            if (StopSpawning)
                                            {
                                                //结束序列动作
                                                EndSequence();
                                                yield break;
                                            }
                                        }

                                        //当前团块不存在
                                        if (currentChunk != null)
                                            //当团块和所有已知邻居的数据准备就绪时，将团块添加到更新队列
                                            currentChunk.AddToQueueWhenReady();
                                    }
                                }
                                else
                                {
                                    // if chunk doesn'transform exist, create new chunk (it adds itself to the update queue when its data is ready)
                                    //如果chunk不存在，则创建新的chunk(当它的数据准备好时会将自己添加到更新队列中)

                                    // spawn chunk.团块创建
                                    GameObject newChunk = Instantiate(ChunkObject, new Vector3(x, y, 0), transform.rotation); // Spawn a new chunk.团块实例化到场景（是从几乎空的团块预制体创建的）
                                    currentChunk = newChunk.GetComponent<CellChunk>();

                                    // spawn neighbor chunks if they're not spawned yet.当相邻团块没在创建时创建它们，循环4次代表4个朝向上下右左（枚举整数索引0-3）
                                    for (int d = 0; d < 4; d++)
                                    {
                                        //返回与给定方向上index相邻的新索引
                                        CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                        //获取相邻团块
                                        GameObject neighborChunk = GetChunk(neighborIndex);
                                        if (neighborChunk == null)
                                        {
                                            //相邻团块不存在则采用团块预制体进行实例化
                                            neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                        }

                                        // always add the neighbor to NeighborChunks, in case it's not there already
                                        //总是添加相邻团块到NeighborChunks属性数组，以防它还没有在那里
                                        currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();

                                        // continue loop in next frame if the current frame time is exceeded.如果超出当前帧时间，协程暂停让当前帧进行渲染直到下次继续剩余动作
                                        if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                        {
                                            yield return new WaitForEndOfFrame();
                                        }
                                        //如果团块管理器通知"终止团块创建序列"
                                        if (StopSpawning)
                                        {
                                            //结束序列动作
                                            EndSequence();
                                            yield break;
                                        }
                                    }

                                    //当前团块不存在
                                    if (currentChunk != null)
                                        //当团块和所有已知邻居的数据准备就绪时，将团块添加到更新队列
                                        currentChunk.AddToQueueWhenReady();
                                }

                            }
                        }



                        // continue loop in next frame if the current frame time is exceeded.如果超出当前帧时间，协程暂停让当前帧进行渲染直到下次继续剩余动作
                        if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        //如果团块管理器通知"终止团块创建序列"
                        if (StopSpawning)
                        {
                            //结束序列动作
                            EndSequence();
                            yield break;
                        }

                    }
                }
            }

            //协程暂停让当前帧进行渲染直到下次继续剩余动作
            yield return new WaitForEndOfFrame();
            //结束序列动作
            EndSequence();
        }

        /// <summary>
        /// 结束序列，具体动作：创建团块动作=假，卸载未被引用的资源，终止序列状态=假，遍历团块摧毁列表为每个团块添上移除标记
        /// </summary>
        private void EndSequence()
        {
            //创建团块动作=假
            SpawningChunks = false;
            //卸载未被引用的资源
            Resources.UnloadUnusedAssets();
            //允许开始创建新团块
            Done = true;
            //终止序列状态=假
            StopSpawning = false;
            //遍历团块摧毁列表
            foreach (CellChunk chunk in ChunksToDestroy)
            {
                //为每个团块添上移除标记
                chunk.FlagToRemove();
            }
        }
    }
}

// 主循环这段代码用于处理游戏世界中“团块”（Chunks）的生成和销毁
// 是一个典型的用于实现无限或大型游戏世界的“分块加载”（chunk-based loading）技术的例子
// 通过动态生成和销毁团块，游戏可以在保持性能的同时，根据玩家或角色的位置，动态地生成和销毁团块，以优化性能和内存使用。
// 以下是功能详解：
// 函数定义：SpawnMissingChunks函数接受四个参数：originX、originY、originZ（它们代表角色的世界位置）和range（团块生成和销毁的范围）。
// 初始化：
// 获取游戏引擎定义的团块生成高度范围heightRange。
// 清空团块更新队列ChunkUpdateQueue和待销毁团块列表ChunksToDestroy。
// 标记待销毁团块：
// 遍历当前存在的所有团块。
// 如果某个团块与角色的距离超过range + CPEngine.ChunkDespawnDistance（默认为11），则将其标记为待销毁。
// 如果团块在垂直方向上超过范围，也将其标记为待销毁。
// 生成新团块：
// 通过三层嵌套循环，遍历以角色为中心、range为半径的立方体范围内的所有潜在团块索引。
// 检查每个潜在团块的Y坐标是否在允许的高度范围内。
// 跳过距离角色过远（超过range * 1.3f）的角落团块。
// 如果更新队列不为空，则暂停循环并处理队列中的团块。
// 使用GetChunkComponent(pixelX, pixelY, z)函数来尝试获取或生成指定坐标处的团块。如团块已经存在则根据属性（如DisableMesh和EnableTimeout）来决定是否重新启用它。

// 逻辑总结：
// 遍历所有已存在的团块，检查哪些团块距离指定的原点（originX, originY, originZ）超出了指定的范围（range + CPEngine.ChunkDespawnDistance），将这些团块加入待销毁列表 ChunksToDestroy。
// 遍历指定范围内的团块，根据一定的条件生成新的团块或更新已有的团块。
// 对于每个团块，检查其高度是否在指定的范围内，如果是，则根据一定的条件生成新的团块或更新已有的团块。
// 在生成或更新团块的过程中，会根据一定的条件进行一些操作，如处理团块队列、实例化邻近团块、添加团块到队列等。
// 会检查是否需要在每帧结束前暂停生成，以控制生成的速率。如果生成过程中需要停止生成，则会结束生成序列。