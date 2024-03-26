using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// Controls spawning and destroying chunks.控制团块的诞生和摧毁

namespace Uniblocks
{
    //核心组件用法：Unity中随便新建一个空对象，挂载脚本

    /// <summary>
    /// 团块管理器（核心组件N0.2）：控制团块的创建和摧毁
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {

        /// <summary>
        /// 团块预制体（Chunk prefab）
        /// </summary>
        public GameObject ChunkObject;

        // chunk lists

        /// <summary>
        /// 团块字典
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
        /// 当前帧团块已保存数量
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
        /// 团夸管理器初始化状态
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

            Chunks = new Dictionary<string, Chunk>();
            ChunkUpdateQueue = new List<Chunk>();
            frameStopwatch = new Stopwatch();

            //set correct scale of trigger collider and additional mesh collider.设置触发器碰撞体和附加网格碰撞器的正确比例

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
        /// 重启帧计时器（统计用时）
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
            if (ChunkUpdateQueue.Contains(chunk) == false)
            {
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
        /// 处理团块队列循环（协程），当SpawnChunks未运行时从Update调用
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
        /// 注册团块（将对团块的引用添加到全局团块字典中，以ChunkIndex为键区，Chunk为值）
        /// </summary>
        /// <param name="chunk"></param>
        public static void RegisterChunk(Chunk chunk)
        { // adds a reference to the chunk to the global chunk list
            Chunks.Add(chunk.ChunkIndex.ToString(), chunk);
        }

        /// <summary>
        /// 注销团块（从全局团块字典中）
        /// </summary>
        /// <param name="chunk"></param>
        public static void UnregisterChunk(Chunk chunk)
        {
            Chunks.Remove(chunk.ChunkIndex.ToString());
        }

        /// <summary>
        /// 返回指定Index(x, y, z)的团块的游戏物体对象，如果对象没有实例化则返回null
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
        /// 返回指定Index的团块的游戏物体对象，如果对象没有实例化则返回null
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
        /// 返回指定Index(x, y, z)的团块
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
        /// 返回指定Index的团块
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
        /// 在三维空间点创建1个团块的游戏物体对象（该点将作为团块在世界的索引位置），前提是该团块索引尚未实例化团块对象
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
        /// 在团块索引位置创建1个团块的游戏物体对象（该点将作为团块在世界的索引位置），前提是该团块索引尚未实例化团块对象
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
        /// 【服务器】在团块索引位置创建1个团块的游戏物体对象（该点将作为团块在世界的索引位置），前提是该团块索引尚未实例化团块对象，本函数会禁用网格生成并启用超时（由服务器在多人游戏中使用），但如果团块实例已存在则不会禁用网格生成，也不为已经生成的块启用超时
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
        /// 【服务器】在三维空间点创建1个团块的游戏物体对象（该点将作为团块在世界的索引位置），前提是该团块索引尚未实例化团块对象，本函数会禁用网格生成并启用超时（由服务器在多人游戏中使用），但如果团块实例已存在则不会禁用网格生成，也不为已经生成的块启用超时
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns>返回团块的游戏物体对象</returns>
        public static GameObject SpawnChunkFromServer(int x, int y, int z)
        {
            return SpawnChunkFromServer(new Index(x, y, z));
        }

        /// <summary>
        /// 创建团块具体动作
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        GameObject DoSpawnChunk(Index index)
        {
            GameObject chunkObject = Instantiate(ChunkObject, index.ToVector3(), transform.rotation) as GameObject;
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            AddChunkToUpdateQueue(chunk);
            return chunkObject;
        }

        /// <summary>
        /// 创建团块
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
        /// 创建团块
        /// </summary>
        /// <param name="position"></param>
        public static void SpawnChunks(Vector3 position)
        {
            Index index = Engine.PositionToChunkIndex(position);
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// 创建团块
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(int x, int y, int z)
        { // take chunk index, no conversion needed
            Engine.ChunkManagerInstance.TrySpawnChunks(x, y, z);
        }
        public static void SpawnChunks(Index index)
        {
            Engine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// 尝试创建团块
        /// </summary>
        /// <param name="index"></param>
        private void TrySpawnChunks(Index index)
        {
            TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// 尝试创建团块
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
                //如果已经在生成团块，标记以等上一轮结束后再次生成，它使用最后请求的位置作为原点
                LastRequest = new Index(x, y, z);
                SpawnQueue = 1; //创建队列计数为1
                StopSpawning = true; //主动打断创建状态
                ChunkUpdateQueue.Clear(); //清除团块更新队列
            }
        }

        public void Update()
        {
            //当有团块创建队列且团块管理器设置完成时
            if (SpawnQueue == 1 && Done == true)
            { // if there is a chunk spawn queued up, and if the previous one has finished, run the queued chunk spawn.当团块创建正在排队时，如前一个已完成，运行队列中的团块创建动作
                SpawnQueue = 0;
                StartSpawnChunks(LastRequest.x, LastRequest.y, LastRequest.z);
            }

            // if not currently spawning chunks, process any queued chunks here instead.如果当前没有生成团块动作，则在这里处理任何排队的
            if (!SpawningChunks && !ProcessChunkQueueLoopActive && ChunkUpdateQueue != null && ChunkUpdateQueue.Count > 0)
            {
                //协程步进处理
                StartCoroutine(ProcessChunkQueueLoop());
            }

            ResetFrameStopwatch();
        }


        /// <summary>
        /// 开始创建团块
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        private void StartSpawnChunks(int originX, int originY, int originZ)
        {
            SpawningChunks = true;
            Done = false;

            int range = Engine.ChunkSpawnDistance;

            //创建缺失的团块
            StartCoroutine(SpawnMissingChunks(originX, originY, originZ, range));
        }

        /// <summary>
        /// 创建缺失的团块
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int originZ, int range)
        {
            int heightRange = Engine.HeightRange;

            // clear update queue - it will be repopulated again in the correct order in the following loop
            //清除更新队列 - 它将在下面的循环中以正确的顺序重新填充
            ChunkUpdateQueue = new List<Chunk>();

            // flag chunks not in range for removal.标记团块不在移除范围内
            ChunksToDestroy = new List<Chunk>();
            foreach (Chunk chunk in Chunks.Values)
            {
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

                            if (Mathf.Abs(y) <= heightRange)
                            { // skip chunks outside of height range.跳过高度范围之外的团块
                                if (Mathf.Abs(originX - x) + Mathf.Abs(originZ - z) < range * 1.3f)
                                { // skip corners.跳过角落

                                    // pause loop while the queue is not empty.当队列不为空时暂停循环
                                    while (ChunkUpdateQueue.Count > 0)
                                    {
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
                                                    neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation) as GameObject;
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
