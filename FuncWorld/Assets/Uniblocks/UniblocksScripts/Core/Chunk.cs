using UnityEngine;
using System.Collections;

namespace Uniblocks
{
    /// <summary>
    /// 管理团块的各种基本功能，存储体素数据等。
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        // Chunk data

        /// <summary>
        /// 主数据数组，其中包含团块中每个体素块ID（使用GetVoxel和SetVoxel函数访问）
        /// </summary>
        public ushort[] VoxelData; // main voxel data array
        /// <summary>
        /// 团块索引(x,y,z)，这与它在世界上的位置直接相关。团块的位置始终是ChunkIndex*Engine.ChunkSideLength（比如对于团块来说其索引位置增1就实际经过默认16个体素块长度，很容易理解）。
        /// </summary>
        public Index ChunkIndex; // corresponds to the position of the chunk
        /// <summary>
        /// 包含对团块所有直接相邻团块的引用数组。这些团块按照Direction枚举的顺序存储(上、下、右、左、前、后)，例如NeighborChunks[0]返回这个上面的团块。
        /// 这个数组只有在一个团块需要检查它相邻团块的体素数据时才会被填充和更新，比如在更新团块的网格时，这意味着在某些时候这个数组不会完全更新，可手动调用GetNeighbors()来立即更新这个数组。
        /// </summary>
        public Chunk[] NeighborChunks; // references to GameObjects of neighbor chunks
        /// <summary>
        /// 团块是空的状态
        /// </summary>
        public bool Empty;

        // Settings & flags

        /// <summary>
        /// 流程新鲜状态
        /// </summary>
        public bool Fresh = true;
        /// <summary>
        /// 允许超时状态
        /// </summary>
        public bool EnableTimeout;
        /// <summary>
        /// 禁用网格状态（对于从UniblocksServer派生的团块：如果为true，则团块不会构建网格）
        /// </summary>
        public bool DisableMesh; // for chunks spawned from UniblocksServer; if true, the chunk will not build a mesh
        /// <summary>
        /// 已被标记为需要移除状态
        /// </summary>
        private bool FlaggedToRemove;
        /// <summary>
        /// 记录团块被生成多久了
        /// </summary>
        public float Lifetime; // how long since the chunk has been spawned

        // update queue
        public bool FlaggedToUpdate,
        InUpdateQueue,
        VoxelsDone; // true when this chunk has finished generating or loading voxel data


        // Semi-constants
        public int SideLength;
        private int SquaredSideLength;

        private ChunkMeshCreator MeshCreator;

        // object prefabs
        public GameObject MeshContainer, ChunkCollider;



        // ==== maintenance ===========================================================================================

        public void Awake()
        { // chunk initialization (load/generate data, set position, etc.)

            // Set variables
            ChunkIndex = new Index(transform.position);
            SideLength = Engine.ChunkSideLength;
            SquaredSideLength = SideLength * SideLength;
            NeighborChunks = new Chunk[6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
            MeshCreator = GetComponent<ChunkMeshCreator>();
            Fresh = true;

            // Register chunk
            ChunkManager.RegisterChunk(this);

            // Clear the voxel data
            VoxelData = new ushort[SideLength * SideLength * SideLength];

            // Set actual position
            transform.position = ChunkIndex.ToVector3() * SideLength;

            // multiply by scale
            transform.position = new Vector3(transform.position.x * transform.localScale.x, transform.position.y * transform.localScale.y, transform.position.z * transform.localScale.z);

            // Grab voxel data
            if (Engine.EnableMultiplayer && !Network.isServer)
            {
                StartCoroutine(RequestVoxelData()); // if multiplayer, get data from server
            }
            else if (Engine.SaveVoxelData && TryLoadVoxelData() == true)
            {
                // data is loaded through TryLoadVoxelData()
            }
            else
            {
                GenerateVoxelData();
            }

        }

        public bool TryLoadVoxelData()
        { // returns true if data was loaded successfully, false if data was not found
            return GetComponent<ChunkDataFiles>().LoadData();
        }

        public void GenerateVoxelData()
        {
            GetComponent<TerrainGenerator>().InitializeGenerator();
        }

        public void AddToQueueWhenReady()
        { // adds chunk to the UpdateQueue when this chunk and all known neighbors have their data ready
            StartCoroutine(DoAddToQueueWhenReady());
        }
        private IEnumerator DoAddToQueueWhenReady()
        {
            while (VoxelsDone == false || AllNeighborsHaveData() == false)
            {
                if (ChunkManager.StopSpawning)
                { // interrupt if the chunk spawn sequence is stopped. This will be restarted in the correct order from ChunkManager
                    yield break;
                }
                yield return new WaitForEndOfFrame();

            }
            ChunkManager.AddChunkToUpdateQueue(this);
        }

        private bool AllNeighborsHaveData()
        { // returns false if at least one neighbor is known but doesn't have data ready yet
            foreach (Chunk neighbor in NeighborChunks)
            {
                if (neighbor != null)
                {
                    if (neighbor.VoxelsDone == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void OnDestroy()
        {
            ChunkManager.UnregisterChunk(this);
        }


        // ==== data =======================================================================================

        public void ClearVoxelData()
        {
            VoxelData = new ushort[SideLength * SideLength * SideLength];
        }

        public int GetDataLength()
        {
            return VoxelData.Length;
        }


        // == set voxel

        /// <summary>
        /// 更改指定数组索引处的体素数据(平面1D数组索引，而不是x,y,z的3D空间坐标)。
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        public void SetVoxelSimple(int rawIndex, ushort data)
        {
            VoxelData[rawIndex] = data;
        }
        /// <summary>
        /// 更改指定索引处的体素数据但不更新网格。此外，与SetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于块边长-1)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        public void SetVoxelSimple(int x, int y, int z, ushort data)
        {
            VoxelData[(z * SquaredSideLength) + (y * SideLength) + x] = data;
        }
        /// <summary>
        /// 更改指定索引处的体素数据但不更新网格。此外，与SetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        public void SetVoxelSimple(Index index, ushort data)
        {
            VoxelData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x] = data;
        }
        /// <summary>
        /// 更改指定索引处的体素数据。如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的体素数据（如当前已实例化）。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        /// <param name="updateMesh"></param>
        public void SetVoxel(int x, int y, int z, ushort data, bool updateMesh)
        {

            // if outside of this chunk, change in neighbor instead (if possible)
            if (x < 0)
            {
                if (NeighborChunks[(int)Direction.left] != null)
                    NeighborChunks[(int)Direction.left].SetVoxel(x + SideLength, y, z, data, updateMesh); return;
            }
            else if (x >= SideLength)
            {
                if (NeighborChunks[(int)Direction.right] != null)
                    NeighborChunks[(int)Direction.right].SetVoxel(x - SideLength, y, z, data, updateMesh); return;
            }
            else if (y < 0)
            {
                if (NeighborChunks[(int)Direction.down] != null)
                    NeighborChunks[(int)Direction.down].SetVoxel(x, y + SideLength, z, data, updateMesh); return;
            }
            else if (y >= SideLength)
            {
                if (NeighborChunks[(int)Direction.up] != null)
                    NeighborChunks[(int)Direction.up].SetVoxel(x, y - SideLength, z, data, updateMesh); return;
            }
            else if (z < 0)
            {
                if (NeighborChunks[(int)Direction.back] != null)
                    NeighborChunks[(int)Direction.back].SetVoxel(x, y, z + SideLength, data, updateMesh); return;
            }
            else if (z >= SideLength)
            {
                if (NeighborChunks[(int)Direction.forward] != null)
                    NeighborChunks[(int)Direction.forward].SetVoxel(x, y, z - SideLength, data, updateMesh); return;
            }

            VoxelData[(z * SquaredSideLength) + (y * SideLength) + x] = data;

            if (updateMesh)
            {
                UpdateNeighborsIfNeeded(x, y, z);
                FlagToUpdate();
            }
        }
        /// <summary>
        /// 更改指定索引处的体素数据。如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的体素数据（如当前已实例化）。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        /// <param name="updateMesh"></param>
        public void SetVoxel(Index index, ushort data, bool updateMesh)
        {
            SetVoxel(index.x, index.y, index.z, data, updateMesh);
        }

        // == get voxel

        /// <summary>
        /// 返回指定数组索引处的体素数据(平面1D数组索引，而不是x,y,z的3D空间坐标)。
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <returns></returns>
        public ushort GetVoxelSimple(int rawIndex)
        {
            return VoxelData[rawIndex];
        }
        /// <summary>
        /// 返回指定索引处的体素数据。与GetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ushort GetVoxelSimple(int x, int y, int z)
        {
            return VoxelData[(z * SquaredSideLength) + (y * SideLength) + x];
        }
        /// <summary>
        /// 返回指定索引处的体素数据。与GetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetVoxelSimple(Index index)
        {
            return VoxelData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x];
        }
        /// <summary>
        /// 返回指定索引处的体素数据。当团块索引超过团块边界时将返回相应团块中的体素数据（如当前已实例化），若没有实例化则返回一个ushort.MaxValue（体素块种类ID的最大上限值65535）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ushort GetVoxel(int x, int y, int z)
        {

            if (x < 0)
            {
                if (NeighborChunks[(int)Direction.left] != null)
                {
                    return NeighborChunks[(int)Direction.left].GetVoxel(x + SideLength, y, z);
                }
                else return ushort.MaxValue;
            }
            else if (x >= SideLength)
            {
                if (NeighborChunks[(int)Direction.right] != null)
                {
                    return NeighborChunks[(int)Direction.right].GetVoxel(x - SideLength, y, z);
                }
                else return ushort.MaxValue;
            }
            else if (y < 0)
            {
                if (NeighborChunks[(int)Direction.down] != null)
                {
                    return NeighborChunks[(int)Direction.down].GetVoxel(x, y + SideLength, z);
                }
                else return ushort.MaxValue;
            }
            else if (y >= SideLength)
            {
                if (NeighborChunks[(int)Direction.up] != null)
                {
                    return NeighborChunks[(int)Direction.up].GetVoxel(x, y - SideLength, z);
                }
                else return ushort.MaxValue;
            }
            else if (z < 0)
            {
                if (NeighborChunks[(int)Direction.back] != null)
                {
                    return NeighborChunks[(int)Direction.back].GetVoxel(x, y, z + SideLength);
                }
                else return ushort.MaxValue;
            }
            else if (z >= SideLength)
            {
                if (NeighborChunks[(int)Direction.forward] != null)
                {
                    return NeighborChunks[(int)Direction.forward].GetVoxel(x, y, z - SideLength);
                }
                else return ushort.MaxValue;
            }


            else
            {
                return VoxelData[(z * SquaredSideLength) + (y * SideLength) + x];
            }
        }
        /// <summary>
        /// 返回指定索引处的体素数据。当团块索引超过团块边界时将返回相应团块中的体素数据（如当前已实例化），若没有实例化则返回一个ushort.MaxValue（体素块种类ID的最大上限值65535）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetVoxel(Index index)
        {
            return GetVoxel(index.x, index.y, index.z);
        }


        // ==== Flags =======================================================================================

        /// <summary>
        /// 给团块贴上移除标记
        /// </summary>
        public void FlagToRemove()
        {
            FlaggedToRemove = true;
        }

        /// <summary>
        /// 给团块贴上更新标记
        /// </summary>
        public void FlagToUpdate()
        {
            FlaggedToUpdate = true;
        }

        // ==== Update ====

        public void Update()
        {
            //当前帧的团块已保存数量清零
            ChunkManager.SavesThisFrame = 0;
        }

        public void LateUpdate()
        {

            // timeout
            if (Engine.EnableChunkTimeout && EnableTimeout)
            {
                Lifetime += Time.deltaTime;
                if (Lifetime > Engine.ChunkTimeout)
                {
                    FlaggedToRemove = true;
                }
            }

            if (FlaggedToUpdate && VoxelsDone && !DisableMesh && Engine.GenerateMeshes)
            { // check if we should update the mesh
                FlaggedToUpdate = false;
                RebuildMesh();
            }

            if (FlaggedToRemove)
            {

                if (Engine.SaveVoxelData)
                { // save data over time, destroy chunk when done
                    if (ChunkDataFiles.SavingChunks == false)
                    { // only destroy chunks if they are not being saved currently
                        if (ChunkManager.SavesThisFrame < Engine.MaxChunkSaves)
                        {
                            ChunkManager.SavesThisFrame++;
                            SaveData();
                            Destroy(this.gameObject);
                        }
                    }
                }

                else
                { // if saving is disabled, destroy immediately
                    Destroy(this.gameObject);
                }

            }
        }

        public void RebuildMesh()
        {
            MeshCreator.RebuildMesh();
            ConnectNeighbors();
        }


        private void SaveData()
        {

            if (Engine.SaveVoxelData == false)
            {
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
                return;
            }

            GetComponent<ChunkDataFiles>().SaveData();

            //if (Application.isWebPlayer == false) {	
            //	GetComponent<ChunkDataFiles>().SaveData();		
            //}

#if UNITY_WEBPLAYER
            //当前平台是WebPlayer，本地化存储应取消
#else
            //当前平台不是WebPlayer
            GetComponent<ChunkDataFiles>().SaveData();
#endif
        }



        // ==== Neighbors =======================================================================================

        public void ConnectNeighbors()
        { // update the mesh on all neighbors that have a mesh but don't know about this chunk yet, and also pass them the reference to this chunk

            int loop = 0;
            int i = loop;

            while (loop < 6)
            {
                if (loop % 2 == 0)
                { // for even indexes, add one; for odd, subtract one (because the neighbors are in opposite direction to this chunk)
                    i = loop + 1;
                }
                else
                {
                    i = loop - 1;
                }

                if (NeighborChunks[loop] != null && NeighborChunks[loop].gameObject.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    if (NeighborChunks[loop].NeighborChunks[i] == null)
                    {
                        NeighborChunks[loop].AddToQueueWhenReady();
                        NeighborChunks[loop].NeighborChunks[i] = this;
                    }
                }

                loop++;
            }
        }

        public void GetNeighbors()
        { // assign the neighbor chunk gameobjects to the NeighborChunks array

            int x = ChunkIndex.x;
            int y = ChunkIndex.y;
            int z = ChunkIndex.z;

            if (NeighborChunks[0] == null) NeighborChunks[0] = ChunkManager.GetChunkComponent(x, y + 1, z);
            if (NeighborChunks[1] == null) NeighborChunks[1] = ChunkManager.GetChunkComponent(x, y - 1, z);
            if (NeighborChunks[2] == null) NeighborChunks[2] = ChunkManager.GetChunkComponent(x + 1, y, z);
            if (NeighborChunks[3] == null) NeighborChunks[3] = ChunkManager.GetChunkComponent(x - 1, y, z);
            if (NeighborChunks[4] == null) NeighborChunks[4] = ChunkManager.GetChunkComponent(x, y, z + 1);
            if (NeighborChunks[5] == null) NeighborChunks[5] = ChunkManager.GetChunkComponent(x, y, z - 1);

        }

        /// <summary>
        /// 返回给定方向上与给定团块索引相邻的团块索引。例如(0,0,0,Direction.left)将返回(-1,0,0)。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Index GetAdjacentIndex(Index index, Direction direction)
        {
            return GetAdjacentIndex(index.x, index.y, index.z, direction);
        }
        /// <summary>
        /// 返回给定方向上与给定团块索引(x,y,z)相邻的团块索引。例如(0,0,0,Direction.left)将返回(-1,0,0)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Index GetAdjacentIndex(int x, int y, int z, Direction direction)
        { // converts x,y,z, direction into a specific index

            if (direction == Direction.down) return new Index(x, y - 1, z);
            else if (direction == Direction.up) return new Index(x, y + 1, z);
            else if (direction == Direction.left) return new Index(x - 1, y, z);
            else if (direction == Direction.right) return new Index(x + 1, y, z);
            else if (direction == Direction.back) return new Index(x, y, z - 1);
            else if (direction == Direction.forward) return new Index(x, y, z + 1);


            else
            {
                Debug.LogError("Chunk.GetAdjacentIndex failed! Returning default index.");
                return new Index(x, y, z);
            }
        }

        /// <summary>
        /// 在需要时更新相邻团块：如果团块索引位于团块的边界，则对位于该边界的相邻团块贴上更新标记
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void UpdateNeighborsIfNeeded(int x, int y, int z)
        { // if the index lies at the border of a chunk, FlagToUpdate the neighbor at that border

            if (x == 0 && NeighborChunks[(int)Direction.left] != null)
            {
                NeighborChunks[(int)Direction.left].GetComponent<Chunk>().FlagToUpdate();
            }

            else if (x == SideLength - 1 && NeighborChunks[(int)Direction.right] != null)
            {
                NeighborChunks[(int)Direction.right].GetComponent<Chunk>().FlagToUpdate();
            }

            if (y == 0 && NeighborChunks[(int)Direction.down] != null)
            {
                NeighborChunks[(int)Direction.down].GetComponent<Chunk>().FlagToUpdate();
            }

            else if (y == SideLength - 1 && NeighborChunks[(int)Direction.up] != null)
            {
                NeighborChunks[(int)Direction.up].GetComponent<Chunk>().FlagToUpdate();
            }

            if (z == 0 && NeighborChunks[(int)Direction.back] != null)
            {
                NeighborChunks[(int)Direction.back].GetComponent<Chunk>().FlagToUpdate();
            }

            else if (z == SideLength - 1 && NeighborChunks[(int)Direction.forward] != null)
            {
                NeighborChunks[(int)Direction.forward].GetComponent<Chunk>().FlagToUpdate();
            }
        }


        // ==== position / voxel index =======================================================================================

        /// <summary>
        /// 返回体素在给定世界位置的团块索引。请注意，位置以及因此返回的团块索引可以在团块的边界之外。
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Index PositionToVoxelIndex(Vector3 position)
        {

            Vector3 point = transform.InverseTransformPoint(position);

            // round it to get an int which we can convert to the voxel index
            Index index = new Index(0, 0, 0);
            index.x = Mathf.RoundToInt(point.x);
            index.y = Mathf.RoundToInt(point.y);
            index.z = Mathf.RoundToInt(point.z);

            return index;
        }

        /// <summary>
        /// 返回给定体素索引中心的绝对世界位置。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 VoxelIndexToPosition(Index index)
        {

            Vector3 localPoint = index.ToVector3(); // convert index to chunk's local position
            return transform.TransformPoint(localPoint);// convert local position to world space

        }

        /// <summary>
        /// 返回给定体素索引中心的绝对世界位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Vector3 VoxelIndexToPosition(int x, int y, int z)
        {

            Vector3 localPoint = new Vector3(x, y, z); // convert index to chunk's local positio
            return transform.TransformPoint(localPoint);// convert local position to world space
        }

        /// <summary>
        /// 返回给定世界位置的体素索引。根据给定的法线方向和returnAdjacent布尔值偏移半个体素块距离，这通常在对体素块进行光线投射时使用。
        /// 当光线投射击中体素块壁时，命中位置将被推入体素块内(returnAdjacent ==false)或推入相邻体素块内(returnAdjacent ==true)，因此返回被光线投射击中的体素块（或被击中体素块壁附近相邻体素块）。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="returnAdjacent"></param>
        /// <returns></returns>
        public Index PositionToVoxelIndex(Vector3 position, Vector3 normal, bool returnAdjacent)
        { // converts the absolute position to the index of the voxel

            if (returnAdjacent == false)
            {
                position = position - (normal * 0.25f); // push the hit point into the cube.将射线碰撞器位置推入立方体（沿法线方向进到里面0.25深度处）
            }
            else
            {
                position = position + (normal * 0.25f); // push the hit point outside of the cube.将射线碰撞器位置推出立方体（沿法线方向退到面外0.25距离处）
            }

            // convert world position to chunk's local position.将世界位置转换为团块的局部位置
            Vector3 point = transform.InverseTransformPoint(position);


            // round it to get an int which we can convert to the voxel index.四舍五入得到一个整数，我们可以将其转换为体素索引
            Index index = new Index(0, 0, 0);
            //四舍五入到最近的顶点
            index.x = Mathf.RoundToInt(point.x);
            index.y = Mathf.RoundToInt(point.y);
            index.z = Mathf.RoundToInt(point.z);

            return index; //将修正后的顶点作为体素的索引返回
        }


        // ==== network ==============

        /// <summary>
        /// [NetWork]当前有多少团块数据请求在服务器上为客户端排队，当服务器每次收到团块数据请求时增1，当服务器已接收团块数据时减1
        /// </summary>
        public static int CurrentChunkDataRequests; // how many chunk requests are currently queued in the server for this client. Increased by 1 every time a chunk requests data, and reduced by 1 when a chunk receives data.

        /// <summary>
        /// [NetWork][协程]请求体素数据：等待直到连接到服务器，然后发送这个团块体素数据的请求到服务器，如果没有连接就重置计数器
        /// </summary>
        /// <returns></returns>
        IEnumerator RequestVoxelData()
        { // waits until we're connected to a server and then sends a request for voxel data for this chunk to the server.
          // 等待直到连接到服务器，然后发送这个团块体素数据的请求到服务器
            while (!Network.isClient)
            {
                CurrentChunkDataRequests = 0; // reset the counter if we're not connected.如果没有连接就重置计数器
                yield return new WaitForEndOfFrame();
            }
            while (Engine.MaxChunkDataRequests != 0 && CurrentChunkDataRequests >= Engine.MaxChunkDataRequests)
            {
                yield return new WaitForEndOfFrame();
            }

            CurrentChunkDataRequests++;
            Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC("SendVoxelData", RPCMode.Server, Network.player, ChunkIndex.x, ChunkIndex.y, ChunkIndex.z);
        }

    }

}
