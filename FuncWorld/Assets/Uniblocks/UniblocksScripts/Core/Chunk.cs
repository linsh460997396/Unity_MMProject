using UnityEngine;
using System.Collections;

namespace Uniblocks
{
    /// <summary>
    /// 团块组件：管理团块的各种基本功能，存储团块体素数据等。
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        // Chunk data

        /// <summary>
        /// 团块的体素数据数组，其中包含团块中每个体素ID-即体素块种类（使用GetVoxel和SetVoxel函数访问）
        /// 假设边长4形成4*4*4=64个体素块组成团块，原点是左下顶点，那最后一个体素块索引是(3,3,3)，代表其顶点在第3深度第3高度往右第3，是数组中第64个元素用[63]表示（第一个元素是[0]），
        /// 那么由(3,3,3)返回[63]的公式VoxelData[(z * SquaredSideLength) + (y * SideLength) + x]=3*16+3*4+3=63，边长为任意时同理。
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

        /// <summary>
        /// 团块更新标记
        /// </summary>
        public bool FlaggedToUpdate;
        /// <summary>
        /// 在团块更新队列
        /// </summary>
        public bool InUpdateQueue;
        /// <summary>
        /// 当此团块完成生成或加载体素数据后为True
        /// </summary>
        public bool VoxelsDone; // true when this chunk has finished generating or loading voxel data

        // Semi-constants.

        /// <summary>
        /// 团块边长
        /// </summary>
        public int SideLength;
        /// <summary>
        /// 团块边长平方
        /// </summary>
        private int SquaredSideLength;
        /// <summary>
        /// 网格创建者
        /// </summary>
        private ChunkMeshCreator MeshCreator;

        // object prefabs

        /// <summary>
        /// 网格容器（预制体）
        /// </summary>
        public GameObject MeshContainer;
        /// <summary>
        /// 团块碰撞体（预制体）
        /// </summary>
        public GameObject ChunkCollider;

        // ==== maintenance ===========================================================================================

        public void Awake()
        { // chunk initialization (load/generate data, set position, etc.)

            // Set variables

            //在脚本所挂载的团块位置建立团块索引（该团块经由团块管理器脚本实例化到场景）
            ChunkIndex = new Index(transform.position);
            //读取团块预设边长
            SideLength = Engine.ChunkSideLength;
            //确定团块预设边长的平方
            SquaredSideLength = SideLength * SideLength;
            //建立当前团块的相邻团块组（防止遍历时超限，数组上限+1）
            NeighborChunks = new Chunk[6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
            //获取团块网格创建器
            MeshCreator = GetComponent<ChunkMeshCreator>();
            //流程新鲜状态=真
            Fresh = true;

            // Register chunk.注册本团块
            ChunkManager.RegisterChunk(this);

            // Clear the voxel data.清空团块体素数据数组（创建一个新的ushort数组来处理新数据）
            VoxelData = new ushort[SideLength * SideLength * SideLength];

            // Set actual position.设置团块在世界的实际位置（也就是想要控制生成平地团块只要控制团块索引高度）
            transform.position = ChunkIndex.ToVector3() * SideLength;

            // multiply by scale.如果团块缩放比例不是默认的1.0，则实际位置要根据缩放情况进行修改
            transform.position = new Vector3(transform.position.x * transform.localScale.x, transform.position.y * transform.localScale.y, transform.position.z * transform.localScale.z);

            // Grab voxel data.获取团块的体素数据

            //多人模式下本机并非服务器
            if (Engine.EnableMultiplayer && !Network.isServer)
            {
                //从服务器获取数据
                StartCoroutine(RequestVoxelData());
            }
            //允许存储体素数据时尝试从磁盘加载体素数据
            else if (Engine.SaveVoxelData && TryLoadVoxelData() == true)
            {
                // data is loaded through TryLoadVoxelData()
                //尝试从磁盘加载体素数据，TryLoadVoxelData()这个动作在条件里已经完成
            }
            else
            {
                //不存在则生成新的体素数据
                GenerateVoxelData();
            }

        }

        /// <summary>
        /// 从磁盘加载体素数据。
        /// </summary>
        /// <returns></returns>
        public bool TryLoadVoxelData()
        { // returns true if data was loaded successfully, false if data was not found
            //尝试从文件加载团块的体素数据，如果未找到数据则返回false。
            return GetComponent<ChunkDataFiles>().LoadData(); 
        }

        /// <summary>
        /// 生成体素数据。在安排地形生成器的脚本里调用GenerateVoxelData()
        /// </summary>
        public void GenerateVoxelData()
        { //Calls GenerateVoxelData() in the script assigned in the TerrainGenerator variable.
            GetComponent<TerrainGenerator>().InitializeGenerator(); //初始化地形生成器
        }

        /// <summary>
        /// 当团块和所有已知相邻团块的数据准备就绪时，将团块添加到更新队列
        /// </summary>
        public void AddToQueueWhenReady()
        { // adds chunk to the UpdateQueue when this chunk and all known neighbors have their data ready
            StartCoroutine(DoAddToQueueWhenReady());
        }

        /// <summary>
        /// [协程]当团块和所有已知相邻团块的数据准备就绪时，将团块添加到更新队列
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAddToQueueWhenReady()
        {
            //当团块未完成体素的生成或加载，或者所有相邻体素未准备好数据
            while (VoxelsDone == false || AllNeighborsHaveData() == false)
            {
                //如果团块管理器主动停止序列
                if (ChunkManager.StopSpawning)
                { // interrupt if the chunk spawn sequence is stopped. This will be restarted in the correct order from ChunkManager
                    //如果团块管理器主动停止序列则中断，这将从团块管理器中以正确的顺序重新启动
                    yield break;
                }
                //协程停止，等待当前帧刷新画面
                yield return new WaitForEndOfFrame();

            }
            //添加当前团块到更新队列
            ChunkManager.AddChunkToUpdateQueue(this);
        }

        /// <summary>
        /// 检查所有相邻团块是否准备好数据
        /// </summary>
        /// <returns>如至少有一个相邻团块是已知的但还没有准备好数据，那么返回false</returns>
        private bool AllNeighborsHaveData()
        { // returns false if at least one neighbor is known but doesn't have data ready yet
            //遍历每个相邻团块
            foreach (Chunk neighbor in NeighborChunks)
            {
                //相邻团块不为空
                if (neighbor != null)
                {
                    //如果有任意相邻团块未完成生成或加载体素数据
                    if (neighbor.VoxelsDone == false)
                    {
                        //返回没有准备好
                        return false;
                    }
                }
            }
            //都准备好了
            return true;
        }

        /// <summary>
        /// 摧毁团块实例（自身）
        /// </summary>
        private void OnDestroy()
        { // OnDestroy() 是一个 MonoBehaviour 方法，当游戏对象即将被销毁时调用。这通常发生在游戏对象被删除或当场景正在加载时。
            ChunkManager.UnregisterChunk(this);
        }


        // ==== data =======================================================================================

        /// <summary>
        /// 清除团块的体素数据数组（存储着具体体素块的种类）。
        /// </summary>
        public void ClearVoxelData()
        {
            //指向了一个新的实例数组
            VoxelData = new ushort[SideLength * SideLength * SideLength];
        }

        /// <summary>
        /// 返回体素数据数组长度（团块边长的立方大小个元素）。
        /// </summary>
        /// <returns></returns>
        public int GetDataLength()
        {
            return VoxelData.Length;
        }


        // == set voxel

        /// <summary>
        /// 更改指定数组索引处的体素数据（即修改体素块的种类），函数采用平面1D数组索引作为参数而不是x,y,z的3D空间坐标。
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        public void SetVoxelSimple(int rawIndex, ushort data)
        {
            //团块边长的立方个索引的第rawIndex个元素=具体体素块的种类
            VoxelData[rawIndex] = data;
        }

        /// <summary>
        /// 更改指定索引处的体素数据（即修改体素块的种类）但不更新网格。此外，与SetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于块边长-1)。
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
        /// 更改指定索引处的体素数据（即修改体素块的种类）但不更新网格。此外，与SetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        public void SetVoxelSimple(Index index, ushort data)
        {
            VoxelData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x] = data;
        }

        /// <summary>
        /// 更改指定索引处的体素数据（即修改体素块的种类）。如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的体素数据（如当前已实例化）。
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
        /// 更改指定索引处的体素数据（即修改体素块的种类）。如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的体素数据（如当前已实例化）。
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
        /// 返回指定数组索引处的体素数据（即修改体素块的种类），函数采用平面1D数组索引作为参数而不是x,y,z的3D空间坐标。
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <returns></returns>
        public ushort GetVoxelSimple(int rawIndex)
        {
            return VoxelData[rawIndex];
        }

        /// <summary>
        /// 返回指定索引处的体素数据（即修改体素块的种类）。与GetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
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
        /// 返回指定索引处的体素数据（即修改体素块的种类）。与GetVoxel不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetVoxelSimple(Index index)
        {
            return VoxelData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x];
        }

        /// <summary>
        /// 返回指定体素索引处的体素数据（即修改体素块的种类）。当体素索引超过团块边界时将返回相邻团块中的体素数据（如当前已实例化），若没有实例化则返回一个ushort.MaxValue（体素块种类ID的最大上限值65535）
        /// </summary>
        /// <param name="x">体素索引</param>
        /// <param name="y">体素索引</param>
        /// <param name="z">体素索引</param>
        /// <returns></returns>
        public ushort GetVoxel(int x, int y, int z)
        {
            //体素索引出了本团块，就去相邻团块寻找体素
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
        /// 返回指定索引处的体素数据（即修改体素块的种类）。当团块索引超过团块边界时将返回相邻团块中的体素数据（如当前已实例化），若没有实例化则返回一个ushort.MaxValue（体素块种类ID的最大上限值65535）
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
            // timeout.允许团块超时时的检查
            if (Engine.EnableChunkTimeout && EnableTimeout)
            {
                //允许团块超时情况下，记录团块被生成了多久
                Lifetime += Time.deltaTime;
                //如果团块的已生成时间超过了团块允许的超时时间
                if (Lifetime > Engine.ChunkTimeout)
                {
                    //将团块打上移除标记
                    FlaggedToRemove = true;
                }
            }

            //团块更新标记+可以开始新的团块体素数据生成（加载）+没有禁用网格生成+引擎设置允许生成网格
            if (FlaggedToUpdate && VoxelsDone && !DisableMesh && Engine.GenerateMeshes)
            { // check if we should update the mesh
                FlaggedToUpdate = false; //关闭当前团块更新标记
                RebuildMesh(); //团块网格重建
            }

            //标记了移除
            if (FlaggedToRemove)
            {
                //允许保存体素数据
                if (Engine.SaveVoxelData)
                { // save data over time, destroy chunk when done
                    //如果当前未在存储团块的体素数据
                    if (ChunkDataFiles.SavingChunks == false)
                    { // only destroy chunks if they are not being saved currently
                        //当前帧的已保存团块数量<上限
                        if (ChunkManager.SavesThisFrame < Engine.MaxChunkSaves)
                        {
                            //当前帧的已保存团块数量+1
                            ChunkManager.SavesThisFrame++;
                            //保存团块数据
                            SaveData();
                            //摧毁团块实例
                            Destroy(this.gameObject);
                        }
                    }
                }
                else
                { // if saving is disabled, destroy immediately.如果设置不允许保存，这里只需立即摧毁团块实例
                    Destroy(this.gameObject);
                }

            }
        }

        /// <summary>
        /// 团块网格重建：立即重建团块网格，然后更新所有相邻团块的网格
        /// </summary>
        public void RebuildMesh()
        {
            //立即重建团块网格
            MeshCreator.RebuildMesh();
            //更新所有相邻团块的网格
            ConnectNeighbors();
        }

        /// <summary>
        /// 保存团块的体素数据到内存（TempChunkData），内部调用了ChunkDataFiles类的SaveData方法，如当前平台是WebPlayer则本地化存储会取消（本函数无效）
        /// </summary>
        private void SaveData()
        {
            //禁用了体素数据保存
            if (Engine.SaveVoxelData == false)
            {
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.禁用了体素数据保存功能，请在引擎设置中启用它");
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

        /// <summary>
        /// 更新所有相邻团块的网格
        /// </summary>
        public void ConnectNeighbors()
        { // update the mesh on all neighbors that have a mesh but don't know about this chunk yet, and also pass them the reference to this chunk

            int loop = 0;
            int i = loop;

            while (loop < 6)
            {
                if (loop % 2 == 0)
                { // for even indexes, add one; for odd, subtract one (because the neighbors are in opposite direction to this chunk)
                    //对于偶数索引加1，对于奇数减1(因为相邻团块与本团块方向相反，i用于从相邻团块回到本团块)
                    i = loop + 1;
                }
                else
                {
                    i = loop - 1;
                }
                //如果相邻团块不为空且相邻团块的“网格过滤器”组件的共享网格（主网格）不为空
                if (NeighborChunks[loop] != null && NeighborChunks[loop].gameObject.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    //如果相邻团块的相邻团块（这里i作用是回到本团块）为空
                    if (NeighborChunks[loop].NeighborChunks[i] == null)
                    {
                        //当相邻团块和其所有已知相邻团块的数据准备就绪时，将这个相邻团块添加到更新队列
                        NeighborChunks[loop].AddToQueueWhenReady();
                        NeighborChunks[loop].NeighborChunks[i] = this;//给相邻团块的属性"相邻团块"进行赋值
                    }
                }
                //继续循环相邻的其他几个团块
                loop++;
            }
        }

        /// <summary>
        /// 如果相邻团块游戏对象未空，则从6个相邻团块获取Chunk组件实例，并且赋值给本团块的NeighborChunks属性数组
        /// </summary>
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
