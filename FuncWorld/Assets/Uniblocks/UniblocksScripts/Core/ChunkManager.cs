using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// Controls spawning and destroying chunks

namespace Uniblocks
{
    /// <summary>
    /// 团块管理器，控制团块的创建和摧毁。组件用法：Unity中随便新建一个空对象“Engine”，把脚本拖到组件位置即挂载（Unity要求一个cs文件只能一个类，且类名须与文件名一致）
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {

        /// <summary>
        /// 团块预制体（Chunk Prefab）
        /// </summary>
        public GameObject ChunkObject;

        // chunk lists

        /// <summary>
        /// 团块组（字典）：存储场景中所有Chunk实例的引用，团块通过Awake调用ChunkManager.RegisterChunk将自己添加到字典中，字符串key对应Chunk的索引并遵循“x,y,z”的格式）
        /// </summary>
        public static Dictionary<string, Chunk> Chunks;

        /// <summary>
        /// 团块更新队列（列表存储按更新优先级排序的团块，在ProcessChunkQueue循环中处理）
        /// </summary>
        private static List<Chunk> ChunkUpdateQueue; // stores chunks ordered by update priority. Processed in the ProcessChunkQueue loop
        /// <summary>
        /// 团块销毁列表（存放在SpawnChunks结束时销毁的chunk）
        /// </summary>
        private static List<Chunk> ChunksToDestroy; // chunks to be destroyed at the end of SpawnChunks

        /// <summary>
        /// 当前帧的团块已保存数量（用于跟每帧的团块保存上限MaxChunkSaves进行比对）
        /// </summary>
        public static int SavesThisFrame;


        // global flags

        /// <summary>
        /// 如果ChunkManager当前正在生成块，则为true
        /// </summary>
        public static bool SpawningChunks; // true if the ChunkManager is currently spawning chunks
        /// <summary>
        /// 当为true时，当前的SpawnChunks序列将被终止(然后将其设置为false)
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
        private Index LastRequest;
        /// <summary>
        /// 目标游戏周期 = 1f / Engine.TargetFPS
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
            targetFrameDuration = 1f / Engine.TargetFPS;
            //建立团块组（字典）
            Chunks = new Dictionary<string, Chunk>();
            //建立团块更新列表
            ChunkUpdateQueue = new List<Chunk>();
            //建立帧计时器（负责记录处理用时）
            frameStopwatch = new Stopwatch();

            //set correct scale of trigger collider and additional mesh collider.设置触发器碰撞体和附加网格碰撞器的正确比例

            //设置团块缩放比例为团块实例当前的缩放比例
            Engine.ChunkScale = ChunkObject.transform.localScale;
            //设置附加网格碰撞器的正确比例
            ChunkObject.GetComponent<Chunk>().MeshContainer.transform.localScale = ChunkObject.transform.localScale;
            //设置触发器碰撞体的正确比例
            ChunkObject.GetComponent<Chunk>().ChunkCollider.transform.localScale = ChunkObject.transform.localScale;

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
        public static void AddChunkToUpdateQueue(Chunk chunk)
        {
            //检查团块更新队列中是已否包含对象团块
            if (ChunkUpdateQueue.Contains(chunk) == false)
            {
                //团块更新队列中添加对象团块
                ChunkUpdateQueue.Add(chunk);
            }
        }

        /// <summary>
        /// 处理团块队列（从SpawnChunks循环调用来更新块网格）
        /// </summary>
        private void ProcessChunkQueue()
        { // called from the SpawnChunks loop to update chunk meshes

            // update the first chunk and remove it from the queue.更新第一个块并将其从队列中删除
            Chunk currentChunk = ChunkUpdateQueue[0];

            if (!currentChunk.Empty && !currentChunk.DisableMesh)
            {
                //当前团块不为空且没有被禁用网格时，重新建立网格
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
        public static void RegisterChunk(Chunk chunk)
        { // adds a reference to the chunk to the global chunk list
            Chunks.Add(chunk.ChunkIndex.ToString(), chunk);
        }

        /// <summary>
        /// 注销团块（从全局团块字典中），这是在团块被销毁时自动完成的。
        /// </summary>
        /// <param name="chunk"></param>
        public static void UnregisterChunk(Chunk chunk)
        {
            Chunks.Remove(chunk.ChunkIndex.ToString());
        }

        /// <summary>
        /// 返回指定Index(x, y, z)的团块的游戏物体对象，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject GetChunk(int x, int y, int z)
        { // returns the gameObject of the chunk with the specified x,y,z, or returns null if the object is not instantiated

            return GetChunk(new Index(x, y, z));
        }

        /// <summary>
        /// 返回指定Index的团块的游戏物体对象，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static GameObject GetChunk(Index index)
        {

            Chunk chunk = GetChunkComponent(index);
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
        /// 返回指定Index(x, y, z)的团块，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Chunk GetChunkComponent(int x, int y, int z)
        {

            return GetChunkComponent(new Index(x, y, z));
        }

        /// <summary>
        /// 返回指定Index的团块，如果对象没有实例化则返回null。注意：这可能会有点慢，避免在一帧内用太多次。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Chunk GetChunkComponent(Index index)
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
        /// 生成带有索引(x,y,z)的单个团块并返回该游戏对象（如已生成则返回已生成的游戏对象）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject SpawnChunk(int x, int y, int z)
        { // spawns a single chunk (only if it's not already spawned)

            GameObject chunk = GetChunk(x, y, z);
            if (chunk == null)
            {
                return Engine.ChunkManagerInstance.DoSpawnChunk(new Index(x, y, z));
            }
            else return chunk;
        }

        /// <summary>
        /// 生成带有索引的单个团块并返回该游戏对象（如已生成则返回已生成的游戏对象）
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns></returns>
        public static GameObject SpawnChunk(Index index)
        { // spawns a single chunk (only if it's not already spawned)

            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                return Engine.ChunkManagerInstance.DoSpawnChunk(index);
            }
            else return chunk;
        }

        /// <summary>
        /// 【服务器】用索引生成单个团块的游戏对象并返回（索引点将作为团块在世界的位置），前提是该索引位置尚未实例化团块对象。本函数会禁用网格生成并启用超时（由服务器在多人游戏中使用），但若团块实例已存在则不会禁用网格生成和启用超时，仅简单地返回该对象。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns>返回团块的游戏物体对象</returns>
        public static GameObject SpawnChunkFromServer(Index index)
        { // spawns a chunk and disables mesh generation and enables timeout (used by the server in multiplayer)
            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                chunk = Engine.ChunkManagerInstance.DoSpawnChunk(index);
                Chunk chunkComponent = chunk.GetComponent<Chunk>();
                chunkComponent.EnableTimeout = true;
                chunkComponent.DisableMesh = true;
                return chunk;
            }
            else return chunk; // don't disable mesh generation and don't enable timeout for chunks that are already spawned
        }

        /// <summary>
        /// 【服务器】用索引(x,y,z)生成单个团块的游戏对象并返回（索引点将作为团块在世界的位置），前提是该索引位置尚未实例化团块对象。本函数会禁用网格生成并启用超时（由服务器在多人游戏中使用），但若团块实例已存在则不会禁用网格生成和启用超时，仅简单地返回该对象。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns>返回团块的游戏物体对象</returns>
        public static GameObject SpawnChunkFromServer(int x, int y, int z)
        {
            return SpawnChunkFromServer(new Index(x, y, z));
        }

        /// <summary>
        /// 创建团块具体动作：用团块预制体在团块索引位置创建团块游戏物体，获取对象上的团块组件，将团块组件对象添加到更队列，最后返回团块游戏物体。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns></returns>
        GameObject DoSpawnChunk(Index index)
        {
            //用团块预制体在团块索引位置创建团块游戏物体
            GameObject chunkObject = Instantiate(ChunkObject, index.ToVector3(), transform.rotation);
            //获取对象上的团块组件
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            //将团块组件对象添加到更队列
            AddChunkToUpdateQueue(chunk);
            //返回团块游戏物体
            return chunkObject;
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时取世界位置(x,y,z)转换为团块索引，然后尝试以该索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(float x, float y, float z)
        { // take world pos, convert to chunk index.取世界位置，转换为块索引
            Index index = Engine.PositionToChunkIndex(new Vector3(x, y, z));
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时取世界位置转换为团块索引，然后尝试以该索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="position"></param>
        public static void SpawnChunks(Vector3 position)
        {
            Index index = Engine.PositionToChunkIndex(position);
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引(x,y,z)为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(int x, int y, int z)
        { // take chunk index, no conversion needed
            Engine.ChunkManagerInstance.TrySpawnChunks(x, y, z);
        }
        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="index"></param>
        public static void SpawnChunks(Index index)
        {
            Engine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="index"></param>
        private void TrySpawnChunks(Index index)
        {
            TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// 当团块管理器同意开始创建团块（说明当前没有生成团块动作）时尝试以给定团块索引(x,y,z)为原点创建团块（地形）。
        /// 如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点，创建队列计数为1，主动打断创建状态（SpawnChunks序列将被终止）并清除团块更新队列
        /// 注意：若你使用三个浮点数或Vector3作为参数的SpawnChunks，它将把参数作为世界位置并在生成团块之前将其转换为团块索引。
        /// 如果你使用三个int或一个Index来做，则不会执行转换，会直接使用提供的索引位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void TrySpawnChunks(int x, int y, int z)
        {

            if (Done == true)
            { // if we're not spawning chunks at the moment, just spawn them normally.如果当前没有生成团块动作，那就正常地生成它们
                StartSpawnChunks(x, y, z);
            }
            else
            { 
                //if we are spawning chunks already, flag to spawn again once the previous round is finished using the last requested position as origin.
                //如果已经在生成团块，标记以等上一轮结束后再生成，它使用最后请求的位置作为原点
                LastRequest = new Index(x, y, z);
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
                //开始创建团块（地形）：SpawningChunks=真，团块管理器调为禁止创建团块（Done=false，防止Update中开始下个创建团块动作而是转为步进当前这个团块创建任务直到完成），最后启动协程来创建缺失的团块
                StartSpawnChunks(LastRequest.x, LastRequest.y, LastRequest.z); 
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
            int range = Engine.ChunkSpawnDistance;
            //启动协程来创建缺失的团块
            StartCoroutine(SpawnMissingChunks(originX, originY, originZ, range));
        }

        /// <summary>
        /// [协程]创建缺失的团块
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int originZ, int range)
        {
            //获取团块创建高度范围限值
            int heightRange = Engine.HeightRange;
            // clear update queue - it will be repopulated again in the correct order in the following loop
            //清除团块更新队列 - 它将在下面的循环中以正确的顺序重新填充
            ChunkUpdateQueue = new List<Chunk>();
            // flag chunks not in range for removal.标记过的团块不在移除范围内
            ChunksToDestroy = new List<Chunk>();
            //遍历团块组中每个存储的团块实例
            foreach (Chunk chunk in Chunks.Values)
            {
                //团块索引点（世界位置）与角色在世界位置如超过团块创建距离+ChunkDespawnDistance（默认8+3=11）
                if (Vector2.Distance(new Vector2(chunk.ChunkIndex.x, chunk.ChunkIndex.z), new Vector2(originX, originZ)) > range + Engine.ChunkDespawnDistance)
                {
                    ChunksToDestroy.Add(chunk);
                }

                else if (Mathf.Abs(chunk.ChunkIndex.y - originY) > range + Engine.ChunkDespawnDistance)
                { // destroy chunks outside of vertical range.摧毁垂直范围外的团块
                    ChunksToDestroy.Add(chunk);
                }
            }


            //主循环
            for (int currentLoop = 0; currentLoop <= range; currentLoop++)
            {
                for (var x = originX - currentLoop; x <= originX + currentLoop; x++)
                { // iterate through all potential chunk indexes within range.遍历范围内所有可能的团块索引
                    for (var y = originY - currentLoop; y <= originY + currentLoop; y++)
                    {
                        for (var z = originZ - currentLoop; z <= originZ + currentLoop; z++)
                        {
                            //绝对值不能超过高度范围
                            if (Mathf.Abs(y) <= heightRange)
                            { // skip chunks outside of height range.跳过高度范围之外的团块
                                if (Mathf.Abs(originX - x) + Mathf.Abs(originZ - z) < range * 1.3f)
                                { // skip corners.跳过距离玩家过远的角落团块

                                    // pause loop while the queue is not empty.当队列不为空时暂停循环
                                    while (ChunkUpdateQueue.Count > 0)
                                    {
                                        //处理团块队列
                                        ProcessChunkQueue();
                                        if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                        {
                                            yield return new WaitForEndOfFrame();
                                        }
                                    }

                                    Chunk currentChunk = GetChunkComponent(x, y, z);


                                    // chunks that already exist but haven't had their mesh built yet should be added to the update queue.已存在但尚未构建网格的团块应添加到更新队列中
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
                                            // spawn neighbor chunks.刷出相邻团块
                                            for (int d = 0; d < 6; d++)
                                            {
                                                Index neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                                GameObject neighborChunk = GetChunk(neighborIndex);
                                                if (neighborChunk == null)
                                                {
                                                    neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                                }

                                                // always add the neighbor to NeighborChunks, in case it's not there already.总是添加相邻团块到NeighborChunks，以防它还没有在那
                                                currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<Chunk>();

                                                // continue loop in next frame if the current frame time is exceeded.如果超出当前帧时间，则在下一帧继续循环
                                                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                                {
                                                    yield return new WaitForEndOfFrame();
                                                }
                                                if (StopSpawning)
                                                {
                                                    EndSequence();
                                                    yield break;
                                                }
                                            }

                                            if (currentChunk != null)
                                                currentChunk.AddToQueueWhenReady();
                                        }


                                    }

                                    else
                                    {
                                        // if chunk doesn't exist, create new chunk (it adds itself to the update queue when its data is ready)
                                        //如果chunk不存在，则创建新的chunk(当它的数据准备好时，它将自己添加到更新队列中)

                                        // spawn chunk
                                        GameObject newChunk = Instantiate(ChunkObject, new Vector3(x, y, z), transform.rotation) as GameObject; // Spawn a new chunk.
                                        currentChunk = newChunk.GetComponent<Chunk>();

                                        // spawn neighbor chunks if they're not spawned yet.当相邻团块没在创建时创建它们
                                        for (int d = 0; d < 6; d++)
                                        {
                                            Index neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                            GameObject neighborChunk = GetChunk(neighborIndex);
                                            if (neighborChunk == null)
                                            {
                                                neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation) as GameObject;
                                            }

                                            // always add the neighbor to NeighborChunks, in case it's not there already
                                            //总是添加相邻团块到NeighborChunks，以防它还没有在那里
                                            currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<Chunk>();

                                            // continue loop in next frame if the current frame time is exceeded.如果超出当前帧时间，则在下一帧继续循环
                                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                            {
                                                yield return new WaitForEndOfFrame();
                                            }
                                            if (StopSpawning)
                                            {
                                                EndSequence();
                                                yield break;
                                            }
                                        }

                                        if (currentChunk != null)
                                            currentChunk.AddToQueueWhenReady();



                                    }

                                }
                            }



                            // continue loop in next frame if the current frame time is exceeded.如果超出当前帧时间，则在下一帧继续循环
                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                            if (StopSpawning)
                            {
                                EndSequence();
                                yield break;
                            }


                        }
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            EndSequence();
        }

        /// <summary>
        /// 结束序列
        /// </summary>
        private void EndSequence()
        {
            //创建团块动作=假
            SpawningChunks = false;
            //卸载未被引用的资源
            Resources.UnloadUnusedAssets();
            Done = true;
            StopSpawning = false;

            foreach (Chunk chunk in ChunksToDestroy)
            {
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
// 如果某个团块与角色的距离超过range + Engine.ChunkDespawnDistance（默认为11），则将其标记为待销毁。
// 如果团块在垂直方向上超过范围，也将其标记为待销毁。
// 生成新团块：
// 通过三层嵌套循环，遍历以角色为中心、range为半径的立方体范围内的所有潜在团块索引。
// 检查每个潜在团块的Y坐标是否在允许的高度范围内。
// 跳过距离角色过远（超过range * 1.3f）的角落团块。
// 如果更新队列不为空，则暂停循环并处理队列中的团块。
// 使用GetChunkComponent(x, y, z)函数来尝试获取或生成指定坐标处的团块。如团块已经存在则根据属性（如DisableMesh和EnableTimeout）来决定是否重新启用它。

// 逻辑总结：
// 遍历所有已存在的团块，检查哪些团块距离指定的原点（originX, originY, originZ）超出了指定的范围（range + Engine.ChunkDespawnDistance），将这些团块加入待销毁列表 ChunksToDestroy。
// 遍历指定范围内的团块，根据一定的条件生成新的团块或更新已有的团块。
// 对于每个团块，检查其高度是否在指定的范围内，如果是，则根据一定的条件生成新的团块或更新已有的团块。
// 在生成或更新团块的过程中，会根据一定的条件进行一些操作，如处理团块队列、实例化邻近团块、添加团块到队列等。
// 会检查是否需要在每帧结束前暂停生成，以控制生成的速率。如果生成过程中需要停止生成，则会结束生成序列。