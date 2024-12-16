using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CellSpace
{
    //把单元空间管理系统当成一个魔方，可修改CPEngine的属性，如横版模式屏蔽Z轴延伸，只使用魔方的XY面进行开发
    //CellItem:单元体（单元内的活动物体）     //Cell：单元（2D单元格3D体素块）
    //2D空间:单元Ce11=单元格，尺寸默认1^2，16~2=256个组成的矩阵为Ce11Chunk，Ce11Chunk边长10则组成1个Region
    //3D空间:单元Ce11=体素（Voxe1），尺寸默认1~3，16~3=4096个组成的矩阵为Chunk，Chunk边长10则组成1个Region
    //单元网格不一定要是立方体，可任意编辑后刷新到团块空间，比如当你挖掉一个单元时可修改局部单元为碎土块网格实现7日杀、英灵神殿那样的挖掘地面效果
    //存档文件名以区域为准，开放世界随探索区域增加而增加存档文件
    //不希望被推动的地面、团块（空间容器）不需要设置刚体只需设置网格碰撞器，有3D刚体和碰撞的对象自然会踩在上面

    /// <summary>
    /// 体素单元（Cell/Voxel）的团块组件：管理团块的各种基本功能，存储团块单元数据等
    /// </summary>
    public class CellChunk : MonoBehaviour
    {
        #region 双向链表管理CellItem_声明部分
        //双向链表和链表池的运用主要提升数据结构灵活性（操作效率）和内存管理的性能
        //‌双向链表‌：每个节点包含两个指针，分别指向前驱和后继节点，这使得从任意节点出发都能方便地访问前驱和后继节点，提高了操作的灵活性。适用于需要频繁进行前后遍历、插入和删除操作的场景，如各种不需要排序的数据列表管理‌
        //‌链表池‌：通过维护一个空闲节点池来减少内存分配和释放的次数，提高内存使用效率。在链表频繁进行插入和删除操作时，能够复用已删除的节点，避免内存碎片的产生，从而提升性能。它的实现通常涉及节点池的初始化、节点的分配与回收等步骤‌。

        /// <summary>
        /// 空间内单元尺寸，默认1.0
        /// </summary>
        [NonSerialized] public float cellSize;
        /// <summary>
        /// 空间内单元尺寸的倒数（_1_gridSize = 1 / gridSize）
        /// </summary>
        [NonSerialized] public float _1_cellSize;
        /// <summary>
        /// 空间最大边界尺寸（自动计算），结果=gridSize*边长方向单元数量
        /// </summary>
        [NonSerialized] public float maxSize;
        /// <summary>
        /// 空间容器内单元体（CellItem）的数量。尽管容器没固定最大容量，但可用于状态检查是否为空或已满
        /// </summary>
        [NonSerialized] public int numCells;
        /// <summary>
        /// 存放网格容器即单元体的数组（类似单位组），HorizontalMode决定初始化元素数量按SideLength的平方（2D）或三次方（3D）个。
        /// 同时是个链表池（其元素通过本类方法修改时自动记录字段中的前驱后驱节点），若要存储的角色怪物数量较少且无需频繁刷新的情况可另开单位组来遍历。
        /// 当你创建一个怪物类继承CellItem，它将继承使用空间检索等方法（目前仅容器内2D查找，3D请用MC插件方法检索容器或小格，读里面怪物组、索引节点怪物，或专门开一个单位组）。
        /// </summary>
        [NonSerialized] public CellItem[] cellItems;
        #endregion

        #region CellChunk data

        /// <summary>
        /// 团块的单元数据数组，其中包含团块中每个体素ID（即单元种类，使用Cell.GetID和Cell.SetID函数访问）
        /// 假设边长4形成4*4*4=64个单元组成团块，原点是左下顶点，那最后一个单元索引是(3,3,3)，代表其顶点在第3深度第3高度往右第3，是数组中第64个元素用[63]表示（第一个元素是[0]），
        /// 那么由(3,3,3)返回[63]的公式CellData[(z * SquaredSideLength) + (z * SideLength) + pixelX]=3*16+3*4+3=63，边长为任意时同理。
        /// 当采用横版模式（HorizontalMode为真）时Z值默认为0
        /// </summary>
        public ushort[] CellData; // main cell data array
        /// <summary>
        /// 团块索引(pixelX,z,z)，这与它在世界上的位置直接相关，当采用横版模式（HorizontalMode为真）时Z值默认为0。
        /// 团块的位置始终是ChunkIndex*CPEngine.ChunkSideLength（比如对于团块来说其索引位置增1就实际经过默认16个单元长度，很容易理解）。
        /// </summary>
        public CPIndex ChunkIndex; // corresponds to the position of the chunk
        /// <summary>
        /// 包含对团块所有直接相邻团块的引用数组。这些团块按照Direction枚举的顺序存储(上、下、右、左、前、后)，例如NeighborChunks[0]返回这个上面的团块。
        /// 这个数组只有在一个团块需要检查它相邻团块的单元数据时才会被填充和更新，比如在更新团块的网格时，这意味着在某些时候这个数组不会完全更新，可手动调用GetNeighbors()来立即更新这个数组。
        /// </summary>
        public CellChunk[] NeighborChunks; // references to GameObjects of neighbor chunks
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
        public bool DisableMesh; // for chunks spawned from Server; if true, the chunk will not build a mesh
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
        /// 当此团块完成生成或加载单元数据后为True
        /// </summary>
        public bool CellsDone; // true when this chunk has finished generating or loading cell data

        // Semi-constants.

        /// <summary>
        /// 团块边长（世界绝对坐标长度值），与团块索引相乘可得到团块左下角在世界坐标系的插入点
        /// </summary>
        public int SideLength;
        /// <summary>
        /// 团块边长平方（3D模式下，区域中团块数组索引计算时与Z轴相乘的系数）
        /// </summary>
        private int SquaredSideLength;
        /// <summary>
        /// 网格创建者
        /// </summary>
        private CellChunkMeshCreator MeshCreator;

        // object prefabs

        /// <summary>
        /// 网格容器（附加网格碰撞器预制体CellChunkAdditionalMesh）
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
            ChunkIndex = new CPIndex(transform.position);
            //读取团块预设边长
            SideLength = CPEngine.ChunkSideLength;
            //确定团块预设边长的平方
            SquaredSideLength = SideLength * SideLength;
            //建立当前团块的相邻团块组（防止遍历时超限，数组上限+1）
            NeighborChunks = new CellChunk[6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
            //获取团块网格创建器
            MeshCreator = GetComponent<CellChunkMeshCreator>();
            //流程新鲜状态=真
            Fresh = true;

            // Register chunk.注册本团块
            CellChunkManager.RegisterChunk(this);

            if (CPEngine.HorizontalMode)
            {
                CellData = new ushort[SideLength * SideLength];
            }
            else
            {
             // Clear the cell data.清空团块单元数据数组（创建一个新的ushort数组来处理新数据）
                CellData = new ushort[SideLength * SideLength * SideLength];
            }

            // Set actual position.设置团块在世界的实际位置（世界坐标系中的插入点=团块索引*团块边长）
            transform.position = ChunkIndex.ToVector3() * SideLength;

            // multiply by scale.如果团块缩放比例不是默认的1.0，则实际位置要根据缩放情况进行修改，2D模式下Z=0所以不会变化
            transform.position = new Vector3(transform.position.x * transform.localScale.x, transform.position.y * transform.localScale.y, transform.position.z * transform.localScale.z);

            // Grab cell data.获取团块的单元数据

            //多人模式下且本机并非服务器
            if (CPEngine.UnetMode)
            {//使用Unet模式
                if (CPEngine.EnableMultiplayer && !Network.isServer)
                {
                    //从服务器获取数据
                    StartCoroutine(RequestCellData());
                }
                //允许存储单元数据时尝试从磁盘加载单元数据
                else if (CPEngine.SaveCellData && TryLoadCellData() == true)
                {
                    // data is loaded through TryLoadCellData()
                    //尝试从磁盘加载单元数据，TryLoadCellData()这个动作在条件里已经完成
                }
                else
                {
                    //不存在则生成新的单元数据
                    GenerateCellData();
                }
            }
        }

        /// <summary>
        /// 从磁盘加载单元数据。
        /// </summary>
        /// <returns></returns>
        public bool TryLoadCellData()
        { // returns true if data was loaded successfully, false if data was not found
            //尝试从文件加载团块的单元数据，如果未找到数据则返回false。
            return GetComponent<CellChunkDataFiles>().LoadData();
        }

        /// <summary>
        /// 生成单元数据。在安排地形生成器的脚本里调用GenerateVoxelData()
        /// </summary>
        public void GenerateCellData()
        { //Calls GenerateCellData() in the script assigned in the CPTerrainGenerator variable.
            GetComponent<CPTerrainGenerator>().InitializeGenerator(); //初始化地形生成器
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
            //当团块未完成单元的生成或加载，或者所有相邻单元未准备好数据
            while (CellsDone == false || AllNeighborsHaveData() == false)
            {
                //如果团块管理器主动停止序列
                if (CellChunkManager.StopSpawning)
                { // interrupt if the chunk spawn sequence is stopped. This will be restarted in the correct order from CellChunkManager
                    //如果团块管理器主动停止序列则中断，这将从团块管理器中以正确的顺序重新启动
                    yield break;
                }
                //协程停止，等待当前帧刷新画面
                yield return new WaitForEndOfFrame();

            }
            //添加当前团块到更新队列
            CellChunkManager.AddChunkToUpdateQueue(this);
        }

        /// <summary>
        /// 检查所有相邻团块是否准备好数据
        /// </summary>
        /// <returns>如至少有一个相邻团块是已知的但还没有准备好数据，那么返回false</returns>
        private bool AllNeighborsHaveData()
        { // returns false if at least one neighbor is known but doesn'transform have data ready yet
            //遍历每个相邻团块
            foreach (CellChunk neighbor in NeighborChunks)
            {
                //相邻团块不为空
                if (neighbor != null)
                {
                    //如果有任意相邻团块未完成生成或加载单元数据
                    if (neighbor.CellsDone == false)
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
            CellChunkManager.UnRegisterChunk(this);
        }

        // ==== data =======================================================================================

        /// <summary>
        /// 清除团块的单元数据数组（存储着具体单元的种类）。
        /// </summary>
        public void ClearCellData()
        {
            if (CPEngine.HorizontalMode)
            {
             //指向了一个新的实例数组
                CellData = new ushort[SideLength * SideLength];
            }
            else
            {
             //指向了一个新的实例数组
                CellData = new ushort[SideLength * SideLength * SideLength];
            }

        }

        /// <summary>
        /// 返回单元数据数组长度（2D/3D分别为团块边长的平方/立方大小个元素）。
        /// </summary>
        /// <returns></returns>
        public int GetDataLength()
        {
            return CellData.Length;
        }

        // == set cell

        /// <summary>
        /// 更改指定数组索引处的单元数据（即修改单元的种类），函数采用平面1D数组索引作为参数而不是x,z,z的2D/3D空间坐标。
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <param name="data">体素ID，将变更成这个单元种类</param>
        public void SetCellSimple(int rawIndex, ushort data)
        {
            //团块边长的立方个索引的第rawIndex个元素=具体单元的种类
            CellData[rawIndex] = data;
        }

        /// <summary>
        /// 更改指定索引处的单元数据（即修改单元的种类）但不更新网格。此外，与SetCell不同，团块索引不能超过团块边界(例如x不能小于0且大于块边长-1)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">体素ID，将变更成这个单元种类</param>
        public void SetCellSimple(int x, int y, int z, ushort data)
        {
            if (CPEngine.HorizontalMode)
            {
                CellData[(y * SideLength) + x] = data;
            }
            else
            {
                CellData[(z * SquaredSideLength) + (y * SideLength) + x] = data;
            }
        }
        /// <summary>
        /// 更改指定索引处的单元数据（即修改单元的种类）但不更新网格。此外，与SetCell不同，团块索引不能超过团块边界(例如x不能小于0且大于块边长-1)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data">体素ID，将变更成这个单元种类</param>
        public void SetCellSimple(int x, int y, ushort data)
        {
            CellData[(y * SideLength) + x] = data;
        }

        /// <summary>
        /// 更改指定索引处的单元数据（即修改单元的种类）但不更新网格。此外，与SetCell不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">体素ID，将变更成这个单元种类</param>
        public void SetCellSimple(CPIndex index, ushort data)
        {
            if (CPEngine.HorizontalMode)
            {
                CellData[(index.y * SideLength) + index.x] = data;
            }
            else
            {
                CellData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x] = data;
            }
        }

        /// <summary>
        /// 更改指定索引处的单元数据（即修改单元的种类）。如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的单元数据（如当前已实例化）。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">体素ID，将变更成这个单元种类</param>
        /// <param name="updateMesh"></param>
        public void SetCell(int x, int y, int z, ushort data, bool updateMesh)
        {
            if (CPEngine.HorizontalMode)
            {
                SetCell(x, y, data, updateMesh);
            }
            else
            {
             // if outside of this chunk, change in neighbor instead (if possible)
                if (x < 0)
                {
                    if (NeighborChunks[(int)Direction.left] != null)
                        NeighborChunks[(int)Direction.left].SetCell(x + SideLength, y, z, data, updateMesh); return;
                }
                else if (x >= SideLength)
                {
                    if (NeighborChunks[(int)Direction.right] != null)
                        NeighborChunks[(int)Direction.right].SetCell(x - SideLength, y, z, data, updateMesh); return;
                }
                else if (y < 0)
                {
                    if (NeighborChunks[(int)Direction.down] != null)
                        NeighborChunks[(int)Direction.down].SetCell(x, y + SideLength, z, data, updateMesh); return;
                }
                else if (y >= SideLength)
                {
                    if (NeighborChunks[(int)Direction.up] != null)
                        NeighborChunks[(int)Direction.up].SetCell(x, y - SideLength, z, data, updateMesh); return;
                }
                else if (z < 0)
                {
                    if (NeighborChunks[(int)Direction.back] != null)
                        NeighborChunks[(int)Direction.back].SetCell(x, y, z + SideLength, data, updateMesh); return;
                }
                else if (z >= SideLength)
                {
                    if (NeighborChunks[(int)Direction.forward] != null)
                        NeighborChunks[(int)Direction.forward].SetCell(x, y, z - SideLength, data, updateMesh); return;
                }
                CellData[(z * SquaredSideLength) + (y * SideLength) + x] = data;
                if (updateMesh)
                {
                    UpdateNeighborsIfNeeded(x, y, z);
                    FlagToUpdate();
                }
            }
        }
        /// <summary>
        /// 更改指定索引处的单元数据（即修改单元的种类）。如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的单元数据（如当前已实例化）。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data">体素ID，将变更成这个单元种类</param>
        /// <param name="updateMesh"></param>
        public void SetCell(int x, int y, ushort data, bool updateMesh)
        {
            // if outside of this chunk, change in neighbor instead (if possible)
            if (x < 0)
            {
                if (NeighborChunks[(int)Direction.left] != null)
                    NeighborChunks[(int)Direction.left].SetCell(x + SideLength, y, data, updateMesh); return;
            }
            else if (x >= SideLength)
            {
                if (NeighborChunks[(int)Direction.right] != null)
                    NeighborChunks[(int)Direction.right].SetCell(x - SideLength, y, data, updateMesh); return;
            }
            else if (y < 0)
            {
                if (NeighborChunks[(int)Direction.down] != null)
                    NeighborChunks[(int)Direction.down].SetCell(x, y + SideLength, data, updateMesh); return;
            }
            else if (y >= SideLength)
            {
                if (NeighborChunks[(int)Direction.up] != null)
                    NeighborChunks[(int)Direction.up].SetCell(x, y - SideLength, data, updateMesh); return;
            }
            CellData[(y * SideLength) + x] = data;
            if (updateMesh)
            {
                UpdateNeighborsIfNeeded(x, y);
                FlagToUpdate();
            }
        }

        /// <summary>
        /// 更改指定索引处的单元数据（即修改单元的种类）。如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的单元数据（如当前已实例化）。
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">体素ID，将变更成这个单元种类</param>
        /// <param name="updateMesh"></param>
        public void SetCell(CPIndex index, ushort data, bool updateMesh)
        {
            if (CPEngine.HorizontalMode)
            {
                SetCell(index.x, index.y, data, updateMesh);
            }
            else
            {
                SetCell(index.x, index.y, index.z, data, updateMesh);
            }
        }

        // == get cell

        /// <summary>
        /// 返回指定数组索引处的单元数据（即修改单元的种类），函数采用平面1D数组索引作为参数而不是x,z,z的2D/3D空间坐标。
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <returns></returns>
        public ushort GetCellSimple(int rawIndex)
        {
            return CellData[rawIndex];
        }

        /// <summary>
        /// 返回指定索引处的单元数据（即修改单元的种类）。与GetCell不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ushort GetCellSimple(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                return CellData[(y * SideLength) + x];
            }
            else
            {
                return CellData[(z * SquaredSideLength) + (y * SideLength) + x];
            }
        }
        /// <summary>
        /// 返回指定索引处的单元数据（即修改单元的种类）。与GetCell不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ushort GetCellSimple(int x, int y)
        {
            return CellData[(y * SideLength) + x];
        }

        /// <summary>
        /// 返回指定索引处的单元数据（即修改单元的种类）。与GetCell不同，团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1)。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetCellSimple(CPIndex index)
        {
            if (CPEngine.HorizontalMode)
            {
                return CellData[(index.y * SideLength) + index.x];
            }
            else
            {
                return CellData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x];
            }
        }

        /// <summary>
        /// 返回指定单元索引处的单元数据（即修改单元的种类）。当单元索引超过团块边界时将返回相邻团块中的单元数据（如当前已实例化），若没有实例化则返回一个ushort.MaxValue（单元种类ID的最大上限值65535）
        /// </summary>
        /// <param name="x">单元索引</param>
        /// <param name="y">单元索引</param>
        /// <param name="z">单元索引</param>
        /// <returns></returns>
        public ushort GetCellID(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                return GetCellID(x, y);
            }
            else
            {
             //单元索引出了本团块，就去相邻团块寻找单元
                if (x < 0)
                {
                    if (NeighborChunks[(int)Direction.left] != null)
                    {
                        return NeighborChunks[(int)Direction.left].GetCellID(x + SideLength, y, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (x >= SideLength)
                {
                    if (NeighborChunks[(int)Direction.right] != null)
                    {
                        return NeighborChunks[(int)Direction.right].GetCellID(x - SideLength, y, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (y < 0)
                {
                    if (NeighborChunks[(int)Direction.down] != null)
                    {
                        return NeighborChunks[(int)Direction.down].GetCellID(x, y + SideLength, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (y >= SideLength)
                {
                    if (NeighborChunks[(int)Direction.up] != null)
                    {
                        return NeighborChunks[(int)Direction.up].GetCellID(x, y - SideLength, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (z < 0)
                {
                    if (NeighborChunks[(int)Direction.back] != null)
                    {
                        return NeighborChunks[(int)Direction.back].GetCellID(x, y, z + SideLength);
                    }
                    else return ushort.MaxValue;
                }
                else if (z >= SideLength)
                {
                    if (NeighborChunks[(int)Direction.forward] != null)
                    {
                        return NeighborChunks[(int)Direction.forward].GetCellID(x, y, z - SideLength);
                    }
                    else return ushort.MaxValue;
                }
                else
                {
                    return CellData[(z * SquaredSideLength) + (y * SideLength) + x];
                }
            }
        }
        /// <summary>
        /// 返回指定单元索引处的单元数据（即修改单元的种类）。当单元索引超过团块边界时将返回相邻团块中的单元数据（如当前已实例化），若没有实例化则返回一个ushort.MaxValue（单元种类ID的最大上限值65535）
        /// </summary>
        /// <param name="x">单元索引</param>
        /// <param name="y">单元索引</param>
        /// <returns></returns>
        public ushort GetCellID(int x, int y)
        {
            //单元索引出了本团块，就去相邻团块寻找单元
            if (x < 0)
            {
                if (NeighborChunks[(int)Direction.left] != null)
                {
                    return NeighborChunks[(int)Direction.left].GetCellID(x + SideLength, y);
                }
                else return ushort.MaxValue;
            }
            else if (x >= SideLength)
            {
                if (NeighborChunks[(int)Direction.right] != null)
                {
                    return NeighborChunks[(int)Direction.right].GetCellID(x - SideLength, y);
                }
                else return ushort.MaxValue;
            }
            else if (y < 0)
            {
                if (NeighborChunks[(int)Direction.down] != null)
                {
                    return NeighborChunks[(int)Direction.down].GetCellID(x, y + SideLength);
                }
                else return ushort.MaxValue;
            }
            else if (y >= SideLength)
            {
                if (NeighborChunks[(int)Direction.up] != null)
                {
                    return NeighborChunks[(int)Direction.up].GetCellID(x, y - SideLength);
                }
                else return ushort.MaxValue;
            }
            else
            {
                return CellData[(y * SideLength) + x];
            }
        }
        /// <summary>
        /// 返回指定索引处的单元数据（即修改单元的种类）。当团块索引超过团块边界时将返回相邻团块中的单元数据（如当前已实例化），若没有实例化则返回一个ushort.MaxValue（单元种类ID的最大上限值65535）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetCellID(CPIndex index)
        {
            if (CPEngine.HorizontalMode)
            {
                return GetCellID(index.x, index.y);
            }
            else
            {
                return GetCellID(index.x, index.y, index.z);
            }
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
            CellChunkManager.SavesThisFrame = 0;
        }

        public void LateUpdate()
        {
            // timeout.允许团块超时时的检查
            if (CPEngine.EnableChunkTimeout && EnableTimeout)
            {
                //允许团块超时情况下，记录团块被生成了多久
                Lifetime += Time.deltaTime;
                //如果团块的已生成时间超过了团块允许的超时时间
                if (Lifetime > CPEngine.ChunkTimeout)
                {
                    //将团块打上移除标记
                    FlaggedToRemove = true;
                }
            }

            //团块更新标记+可以开始新的团块单元数据生成（加载）+没有禁用网格生成+引擎设置允许生成网格
            if (FlaggedToUpdate && CellsDone && !DisableMesh && CPEngine.GenerateMeshes)
            { // check if we should update the mesh
                FlaggedToUpdate = false; //关闭当前团块更新标记
                RebuildMesh(); //团块网格重建
            }

            //标记了移除
            if (FlaggedToRemove)
            {
                //允许保存单元数据
                if (CPEngine.SaveCellData)
                { // save data over time, destroy chunk when done
                    //如果当前未在存储团块的单元数据
                    if (CellChunkDataFiles.SavingChunks == false)
                    { // only destroy chunks if they are not being saved currently
                        //当前帧的已保存团块数量<上限
                        if (CellChunkManager.SavesThisFrame < CPEngine.MaxChunkSaves)
                        {
                            //当前帧的已保存团块数量+1
                            CellChunkManager.SavesThisFrame++;
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
        /// 保存团块的单元数据到内存（TempChunkData），内部调用了ChunkDataFiles类的SaveData方法，如当前平台是WebPlayer则本地化存储会取消（本函数无效）
        /// </summary>
        private void SaveData()
        {
            //禁用了单元数据保存
            if (CPEngine.SaveCellData == false)
            {
                Debug.LogWarning("CellSpace: Saving is disabled. You can enable it in the CPEngine Settings.禁用了单元数据保存功能，请在引擎设置中启用它");
                return;
            }

            GetComponent<CellChunkDataFiles>().SaveData();

            //if (Application.isWebPlayer == false) {	
            //	GetComponent<CellChunkDataFiles>().SaveData();		
            //}

#if UNITY_WEBPLAYER
            //当前平台是WebPlayer，本地化存储应取消
#else
            //当前平台不是WebPlayer
            GetComponent<CellChunkDataFiles>().SaveData();
#endif
        }

        // ==== Neighbors =======================================================================================

        /// <summary>
        /// 更新所有相邻团块的网格
        /// </summary>
        public void ConnectNeighbors()
        { // update the mesh on all neighbors that have a mesh but don'transform know about this chunk yet, and also pass them the reference to this chunk
            int max;
            int loop = 0;
            int i = loop;
            if (CPEngine.HorizontalMode)
            {
                max = 4;
            }
            else
            {
                max = 6;
            }

            while (loop < max)
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
        /// 如果相邻团块游戏对象为空，则从6个相邻团块获取Chunk组件实例，并且赋值给本团块的NeighborChunks属性数组
        /// </summary>
        public void GetNeighbors()
        { // assign the neighbor chunk gameobjects to the NeighborChunks array
            int x = ChunkIndex.x;
            int y = ChunkIndex.y;
            if (CPEngine.HorizontalMode)
            {
                if (NeighborChunks[0] == null) NeighborChunks[0] = CellChunkManager.GetChunkComponent(x, y + 1);
                if (NeighborChunks[1] == null) NeighborChunks[1] = CellChunkManager.GetChunkComponent(x, y - 1);
                if (NeighborChunks[2] == null) NeighborChunks[2] = CellChunkManager.GetChunkComponent(x + 1, y);
                if (NeighborChunks[3] == null) NeighborChunks[3] = CellChunkManager.GetChunkComponent(x - 1, y);
            }
            else
            {
                int z = ChunkIndex.z;
                if (NeighborChunks[0] == null) NeighborChunks[0] = CellChunkManager.GetChunkComponent(x, y + 1, z);
                if (NeighborChunks[1] == null) NeighborChunks[1] = CellChunkManager.GetChunkComponent(x, y - 1, z);
                if (NeighborChunks[2] == null) NeighborChunks[2] = CellChunkManager.GetChunkComponent(x + 1, y, z);
                if (NeighborChunks[3] == null) NeighborChunks[3] = CellChunkManager.GetChunkComponent(x - 1, y, z);
                if (NeighborChunks[4] == null) NeighborChunks[4] = CellChunkManager.GetChunkComponent(x, y, z + 1);
                if (NeighborChunks[5] == null) NeighborChunks[5] = CellChunkManager.GetChunkComponent(x, y, z - 1);
            }
        }

        /// <summary>
        /// 返回给定方向上与给定团块索引相邻的团块索引。例如(0,0,0,Direction.left)将返回(-1,0,0)。2D模式不考虑Z轴及前后邻居团块
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public CPIndex GetAdjacentIndex(CPIndex index, Direction direction)
        {
            if (CPEngine.HorizontalMode)
            {
                return GetAdjacentIndex(index.x, index.y, direction);
            }
            else
            {
                return GetAdjacentIndex(index.x, index.y, index.z, direction);
            }
        }
        /// <summary>
        /// 返回给定方向上与给定团块索引(pixelX,z,z)相邻的团块索引。例如(0,0,0,Direction.left)将返回(-1,0,0)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public CPIndex GetAdjacentIndex(int x, int y, int z, Direction direction)
        { // converts pixelX,z,z direction into a specific index
            if (CPEngine.HorizontalMode)
            {
                return GetAdjacentIndex(x, y, direction);
            }
            else
            {
                if (direction == Direction.down) return new CPIndex(x, y - 1, z);
                else if (direction == Direction.up) return new CPIndex(x, y + 1, z);
                else if (direction == Direction.left) return new CPIndex(x - 1, y, z);
                else if (direction == Direction.right) return new CPIndex(x + 1, y, z);
                else if (direction == Direction.back) return new CPIndex(x, y, z - 1);
                else if (direction == Direction.forward) return new CPIndex(x, y, z + 1);
                else
                {
                    Debug.LogError("CellChunk.GetAdjacentIndex failed! Returning default index.");
                    return new CPIndex(x, y, z);
                }
            }
        }
        /// <summary>
        /// 返回给定方向上与给定团块索引(pixelX,z)相邻的团块索引。例如(0,0,Direction.left)将返回(-1,0)。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public CPIndex GetAdjacentIndex(int x, int y, Direction direction)
        { // converts pixelX,z direction into a specific index

            if (direction == Direction.down) return new CPIndex(x, y - 1);
            else if (direction == Direction.up) return new CPIndex(x, y + 1);
            else if (direction == Direction.left) return new CPIndex(x - 1, y);
            else if (direction == Direction.right) return new CPIndex(x + 1, y);
            else
            {
                Debug.LogError("CellChunk.GetAdjacentIndex failed! Returning default index.");
                return new CPIndex(x, y);
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
            if (CPEngine.HorizontalMode)
            {
                UpdateNeighborsIfNeeded(x, y);
            }
            else
            {
                if (x == 0 && NeighborChunks[(int)Direction.left] != null)
                {
                    NeighborChunks[(int)Direction.left].GetComponent<CellChunk>().FlagToUpdate();
                }
                else if (x == SideLength - 1 && NeighborChunks[(int)Direction.right] != null)
                {
                    NeighborChunks[(int)Direction.right].GetComponent<CellChunk>().FlagToUpdate();
                }
                if (y == 0 && NeighborChunks[(int)Direction.down] != null)
                {
                    NeighborChunks[(int)Direction.down].GetComponent<CellChunk>().FlagToUpdate();
                }
                else if (y == SideLength - 1 && NeighborChunks[(int)Direction.up] != null)
                {
                    NeighborChunks[(int)Direction.up].GetComponent<CellChunk>().FlagToUpdate();
                }
                if (z == 0 && NeighborChunks[(int)Direction.back] != null)
                {
                    NeighborChunks[(int)Direction.back].GetComponent<CellChunk>().FlagToUpdate();
                }
                else if (z == SideLength - 1 && NeighborChunks[(int)Direction.forward] != null)
                {
                    NeighborChunks[(int)Direction.forward].GetComponent<CellChunk>().FlagToUpdate();
                }
            }
        }
        /// <summary>
        /// 在需要时更新相邻团块：如果团块索引位于团块的边界，则对位于该边界的相邻团块贴上更新标记
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateNeighborsIfNeeded(int x, int y)
        { // if the index lies at the border of a chunk, FlagToUpdate the neighbor at that border
            if (x == 0 && NeighborChunks[(int)Direction.left] != null)
            {
                NeighborChunks[(int)Direction.left].GetComponent<CellChunk>().FlagToUpdate();
            }
            else if (x == SideLength - 1 && NeighborChunks[(int)Direction.right] != null)
            {
                NeighborChunks[(int)Direction.right].GetComponent<CellChunk>().FlagToUpdate();
            }
            if (y == 0 && NeighborChunks[(int)Direction.down] != null)
            {
                NeighborChunks[(int)Direction.down].GetComponent<CellChunk>().FlagToUpdate();
            }
            else if (y == SideLength - 1 && NeighborChunks[(int)Direction.up] != null)
            {
                NeighborChunks[(int)Direction.up].GetComponent<CellChunk>().FlagToUpdate();
            }
        }

        // ==== position / cell index =======================================================================================

        /// <summary>
        /// 返回单元在给定世界位置的团块索引。请注意，位置以及因此返回的团块索引可以在团块的边界之外。
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public CPIndex PositionToCellIndex(Vector3 position)
        {
            //世界绝对坐标转父级容器内的相对坐标
            Vector3 point = transform.InverseTransformPoint(position);
            if (CPEngine.HorizontalMode)
            {
                CPIndex index = new CPIndex(0, 0);
                index.x = Mathf.RoundToInt(point.x);
                index.y = Mathf.RoundToInt(point.y);
                return index;
            }
            else
            {
             // round it to get an int which we can convert to the cell index
                CPIndex index = new CPIndex(0, 0, 0);
                index.x = Mathf.RoundToInt(point.x);
                index.y = Mathf.RoundToInt(point.y);
                index.z = Mathf.RoundToInt(point.z);
                return index;
            }
        }

        /// <summary>
        /// 返回给定单元索引中心的世界绝对位置。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 CellIndexToPosition(CPIndex index)
        {
            Vector3 localPoint = index.ToVector3(); // convert index to chunk's local position
            return transform.TransformPoint(localPoint); // convert local position to world space
        }

        /// <summary>
        /// 返回给定单元索引的世界绝对位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Vector3 CellIndexToPosition(int x, int y, int z)
        {

            Vector3 localPoint = new Vector3(x, y, z); // convert index to chunk's local positio
            return transform.TransformPoint(localPoint);// convert local position to world space
        }
        /// <summary>
        /// 返回给定单元索引的世界绝对位置。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 CellIndexToPosition(int x, int y)
        {

            Vector3 localPoint = new Vector3(x, y, 0); // convert index to chunk's local positio
            return transform.TransformPoint(localPoint);// convert local position to world space
        }

        /// <summary>
        /// 返回给定世界位置的单元索引。根据给定的法线方向和returnAdjacent布尔值偏移半个单元距离，这通常在对单元进行光线投射时使用。
        /// 当光线投射击中单元壁时，命中位置将被推入单元内(returnAdjacent ==false)或推入相邻单元内(returnAdjacent ==true)，因此返回被光线投射击中的单元（或被击中单元壁附近相邻单元）。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="returnAdjacent"></param>
        /// <returns></returns>
        public CPIndex PositionToCellIndex(Vector3 position, Vector3 normal, bool returnAdjacent)
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


            // round it to get an int which we can convert to the cell index.四舍五入得到一个整数，我们可以将其转换为单元索引
            CPIndex index = new CPIndex(0, 0, 0);
            //四舍五入到最近的顶点
            index.x = Mathf.RoundToInt(point.x);
            index.y = Mathf.RoundToInt(point.y);
            if (!CPEngine.HorizontalMode)
            {
                index.z = Mathf.RoundToInt(point.z);
            }
            return index; //将修正后的顶点作为单元的索引返回
        }


        // ==== network ==============

        /// <summary>
        /// [NetWork]当前有多少团块数据请求在服务器上为客户端排队，当服务器每次收到团块数据请求时增1，当服务器已接收团块数据时减1
        /// </summary>
        public static int CurrentChunkDataRequests; // how many chunk requests are currently queued in the server for this client. Increased by 1 every time a chunk requests data, and reduced by 1 when a chunk receives data.

        /// <summary>
        /// [NetWork][协程]请求单元数据：等待直到连接到服务器，然后发送这个团块单元数据的请求到服务器，如果没有连接就重置计数器
        /// </summary>
        /// <returns></returns>
        IEnumerator RequestCellData()
        { // waits until we're connected to a server and then sends a request for cell data for this chunk to the server.
          // 等待直到连接到服务器，然后发送这个团块单元数据的请求到服务器
            while (!Network.isClient)
            {
                CurrentChunkDataRequests = 0; // reset the counter if we're not connected.如果没有连接就重置计数器
                yield return new WaitForEndOfFrame();
            }
            while (CPEngine.MaxChunkDataRequests != 0 && CurrentChunkDataRequests >= CPEngine.MaxChunkDataRequests)
            {
                yield return new WaitForEndOfFrame();
            }

            CurrentChunkDataRequests++;
            CPEngine.Network.GetComponent<NetworkView>().RPC("SendCellData", RPCMode.Server, Network.player, ChunkIndex.x, ChunkIndex.y, ChunkIndex.z);
        }
        #endregion

        #region 双向链表管理CellItem_函数部分
        public CellChunk(int SideLength_, float cellSize_)
        {
#if UNITY_EDITOR
            //条件失败时进行断言，容器空间内的相对坐标化为索引的计算要求坐标必须是正值（容器左下角始终为原点插入点）
            Debug.Assert(SideLength_ > 0, "SideLength_ must be greater than 0.");
            Debug.Assert(cellSize_ > 0, "cellSize_ must be greater than 0.");
#endif
            SideLength = SideLength_;
            cellSize = cellSize_;
            _1_cellSize = 1f / cellSize_; //如果网格容器（单元体）尺寸16就是0.625
                                          //空间内总尺寸
            maxSize = cellSize * SideLength;
            if (cellItems == null)
            {
                if (CPEngine.HorizontalMode)
                {
                    //2D模式，新的容器（存储单元体），怪物对象类继承CellItem后的多个实例可在1平方单元格的空间游荡
                    cellItems = new CellItem[SideLength * SideLength];
                }
                else
                {
                    //3D模式下多个怪物对象可在1立方单元的空间游荡
                    cellItems = new CellItem[SideLength * SideLength * SideLength];
                }

            }
            else
            {
                // 使用null填充数组cellItems，这通常是在构造函数中初始化数组时使用的操作。
                // 假设cellItems已经被声明为一个数组变量，这一步将确保数组中的每个元素都被设置为null
                Array.Fill(cellItems, null); //空间项（网格容器）数组充满

                // 重新调整数组cellItems的大小，新的大小为SideLength^边长个数，并将调整后的数组重新赋值给cellItems。
                // 这一步可能会创建一个新的数组，如果原数组大小与新的大小不同，原数组的内容将被复制到新数组中（或部分复制，取决于大小变化）。
                // 如果新大小大于原大小，新元素将被设置为默认值（对于引用类型是null，对于值类型是零或相应的默认值）。

                if (CPEngine.HorizontalMode)
                {
                    //2D模式
                    Array.Resize(ref cellItems, SideLength * SideLength);
                }
                else
                {
                    //3D模式
                    Array.Resize(ref cellItems, SideLength * SideLength * SideLength);
                }

            }
        }

        /// <summary>
        /// 为空间容器（双向链表）添加网格容器（单元体），比如添加一些继承CellItem的怪物类对象
        /// </summary>
        /// <param name="c"></param>
        public void Add(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.chunk == this);
            Debug.Assert(c.index == -1);
            Debug.Assert(c.nodePrev == null);
            Debug.Assert(c.nodeNext == null);
            Debug.Assert(c.x >= 0 && c.x < maxSize); //容器内相对位置需有效
            Debug.Assert(c.y >= 0 && c.y < maxSize);
#endif
            int idx;
            // 从坐标返回网格容器（单元体）在空间容器内的索引
            if (CPEngine.HorizontalMode)
            {
                idx = PosToIndexH2D(c.x, c.y); // calc rIdx & cIdx
            }
            else
            {
                idx = PosToIndex(c.x, c.y, c.z); // calc rIdx & cIdx
            }

#if UNITY_EDITOR
            Debug.Assert(cellItems[idx] == null || cellItems[idx].nodePrev == null);
#endif

            // 进行Link
            if (cellItems[idx] != null)
            {
                //如果空间容器索引对应单元体存在，则将新单元体作为该单元体的前驱节点
                cellItems[idx].nodePrev = c;
            }
            //将空间容器索引对应单元体作为新单元体的后驱节点（可为null）
            c.nodeNext = cellItems[idx];
            c.index = idx; //刷新新单元体的空间索引为idx
            cellItems[idx] = c; //新单元体作为空间容器索引对应单元体
                                //如果只有1个节点，作为头部节点呈现：【Prev=null】【C】【Next=null】
#if UNITY_EDITOR
            Debug.Assert(cellItems[idx].nodePrev == null);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
#endif
            //空间容器中网格容器（单元体）数量自增
            ++numCells;
        }

        /// <summary>
        /// 从空间容器中移除网格容器（单元体），比如移除一些继承CellItem的怪物类对象
        /// </summary>
        /// <param name="c"></param>
        public void Remove(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.chunk == this);
            Debug.Assert(c.nodePrev == null && cellItems[c.index] == c || c.nodePrev.nodeNext == c && cellItems[c.index] != c);
            Debug.Assert(c.nodeNext == null || c.nodeNext.nodePrev == c);
            //Debug.Assert(cellItems[c.index] include c);
#endif

            // unlink
            if (c.nodePrev != null)
            {  //如果目标单元体有前驱节点（说明它不是头部节点）
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] != c);
#endif
                //将目标单元体前驱节点（对应单元体）的后驱节点更换为目标单元体的后驱节点（目标单元体被移除，所以前后节点相连）
                c.nodePrev.nodeNext = c.nodeNext;
                if (c.nodeNext != null)
                {
                    //如果目标单元体的后驱节点不为空（不是最后一个），将后驱节点的前驱节点设置为要移除目标单元体的前驱节点（目标单元体被移除，所以前后节点相连）
                    c.nodeNext.nodePrev = c.nodePrev;
                    c.nodeNext = null; //清空要删除的目标单元体的后驱节点
                }
                c.nodePrev = null; //清空要删除的目标单元体的前驱节点
            }
            else
            {
                //如果目标单元体无前驱节点（说明它是头部节点）
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] == c);
#endif
                //目标位置的空间单元体被后驱节点替换（目标单元体作为头部节点被移除，所以后驱节点占位）
                cellItems[c.index] = c.nodeNext;
                if (c.nodeNext != null)
                {
                    //如果目标单元体的后驱节点不为null，该后驱节点的前驱节点设置为null（后驱节点作为头部节点了）
                    c.nodeNext.nodePrev = null;
                    c.nodeNext = null; //清空要删除的目标单元体的后驱节点
                }
            }
#if UNITY_EDITOR
            Debug.Assert(cellItems[c.index] != c);
#endif
            c.index = -1; //初始化目标单元体的空间索引
            c.chunk = null; //清空目标单元体的空间容器

            //空间容器中网格容器（单元体）数量自减
            --numCells;
        }

        /// <summary>
        /// 更新一个Cell对象在空间容器中的索引位置（同时更新双向链表），一般用于活动物体（继承CellItem的角色怪物类对象）在容器频繁移动时的刷新
        /// </summary>
        /// <param name="c"></param>
        public void Refresh(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.chunk == this);
            Debug.Assert(c.index > -1);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
            //Debug.Assert(cellItems[c.index] include c);
#endif
            //获取单元体当前坐标
            var x = c.x;
            var y = c.y;
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxSize);
            Debug.Assert(y >= 0 && y < maxSize);
#endif
            //将当前坐标转换为空间内的索引
            int cIdx = (int)(x * _1_cellSize);
            int rIdx = (int)(y * _1_cellSize);
            int idx;
            if (!CPEngine.HorizontalMode)
            {
                var z = c.z;
                int hIdx = (int)(z * _1_cellSize);
                idx = hIdx * SideLength * SideLength + rIdx * SideLength + cIdx;
            }
            else
            {
                idx = rIdx * SideLength + cIdx;
            }
            //如果单元格尺寸是2.0，那么_1_cellSize=0.5，最终得到的cIdx是修正后的值，最终单元体所在（1.5,1.5）对应单元格索引是0
#if UNITY_EDITOR
            Debug.Assert(idx <= cellItems.Length);
#endif
            //单元体的位置与其spaceIndex字段相同
            if (idx == c.index) return;  //空间索引无变化时直接返回

            //开始更新变化
            // unlink
            if (c.nodePrev != null)
            {  // isn'transform header 非头部单元体
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] != c);
#endif
                c.nodePrev.nodeNext = c.nodeNext;
                if (c.nodeNext != null)
                { //非尾部单元体
                    c.nodeNext.nodePrev = c.nodePrev;
                    //c.nodeNext = {};
                }
                //c.nodePrev = {};
            }
            else
            {
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] == c);
#endif
                cellItems[c.index] = c.nodeNext;
                if (c.nodeNext != null)
                {
                    c.nodeNext.nodePrev = null;
                    //c.nodeNext = {};
                }
            }
            //c.index = -1;
#if UNITY_EDITOR
            Debug.Assert(cellItems[c.index] != c);
            Debug.Assert(idx != c.index);
#endif

            // link
            if (cellItems[idx] != null)
            {
                cellItems[idx].nodePrev = c;
            }
            c.nodePrev = null;
            c.nodeNext = cellItems[idx];
            cellItems[idx] = c;
            c.index = idx;
#if UNITY_EDITOR
            Debug.Assert(cellItems[idx].nodePrev == null);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
#endif
        }

        #region 2D空间检索方法，在MC插件代码部分有一些3D空间检索方法，额外检索方法可一并写在此处
        /// <summary>
        /// [2D横版模式]返回网格容器（单元体）位置在空间容器中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndexH2D(float x, float y)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxSize);
            Debug.Assert(y >= 0 && y < maxSize);
#endif
            int cIdx = (int)(x * _1_cellSize); //直接取整
            int rIdx = (int)(y * _1_cellSize);
            int idx = rIdx * SideLength + cIdx; //化作2D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= cellItems.Length); //超限报停
#endif
            return idx;
        }
        /// <summary>
        /// 2D返回网格容器（单元体）在空间容器中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndex2D(float x, float z)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxSize);
            Debug.Assert(z >= 0 && z < maxSize);
#endif
            int cIdx = (int)(x * _1_cellSize); //直接取整
            int rIdx = (int)(z * _1_cellSize);
            int idx = rIdx * SideLength + cIdx; //化作2D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= cellItems.Length); //超限报停
#endif
            return idx;
        }

        /// <summary>
        /// 3D返回网格容器（单元体）在空间容器中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndex(float x, float y, float z)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxSize);
            Debug.Assert(y >= 0 && y < maxSize);
#endif
            int cIdx = (int)(x * _1_cellSize); //直接取整
            int rIdx = (int)(y * _1_cellSize);
            int hIdx = (int)(z * _1_cellSize);
            int idx = hIdx * SideLength * SideLength + rIdx * SideLength + cIdx; //化作3D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= cellItems.Length); //超限报停
#endif
            return idx;
        }

        /// <summary>
        /// [2D横版模式]在九宫范围内找出第1个网格容器（单元体）并返回
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByNineBoxGridH2D(float x, float y, float radius)
        {
            // 5
            int cIdx = (int)(x * _1_cellSize);
            if (cIdx < 0 || cIdx >= SideLength) return null;
            int rIdx = (int)(y * _1_cellSize);
            if (rIdx < 0 || rIdx >= SideLength) return null;
            int idx = rIdx * SideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 6
            ++cIdx;
            if (cIdx >= SideLength) return null;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 3
            ++rIdx;
            if (rIdx >= SideLength) return null;
            idx += SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return null;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 4
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return null;
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            return null;
        }
        /// <summary>
        /// 2D在九宫范围内找出第1个网格容器（单元体）并返回
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByNineBoxGrid2D(float x, float z, float radius)
        {
            // 5
            int cIdx = (int)(x * _1_cellSize);
            if (cIdx < 0 || cIdx >= SideLength) return null;
            int rIdx = (int)(z * _1_cellSize);
            if (rIdx < 0 || rIdx >= SideLength) return null;
            int idx = rIdx * SideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 6
            ++cIdx;
            if (cIdx >= SideLength) return null;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 3
            ++rIdx;
            if (rIdx >= SideLength) return null;
            idx += SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return null;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 4
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return null;
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            return null;
        }

        /// <summary>
        /// [2D横版模式]遍历坐标周围九宫格内的网格容器（单元体）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC，但这种应该是无所谓的, 里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGrid(float x, float y, Func<CellItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * _1_cellSize);
            if (cIdx < 0 || cIdx >= SideLength) return;
            int rIdx = (int)(y * _1_cellSize);
            if (rIdx < 0 || rIdx >= SideLength) return;
            int idx = rIdx * SideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= SideLength) return;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= SideLength) return;
            idx += SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
        }
        /// <summary>
        /// 2D遍历坐标周围九宫格内的网格容器（单元体）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC，但这种应该是无所谓的, 里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGrid2D(float x, float z, Func<CellItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * _1_cellSize);
            if (cIdx < 0 || cIdx >= SideLength) return;
            int rIdx = (int)(z * _1_cellSize);
            if (rIdx < 0 || rIdx >= SideLength) return;
            int idx = rIdx * SideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= SideLength) return;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= SideLength) return;
            idx += SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= SideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
        }

        /// <summary>
        /// [2D横版模式]圆形扩散遍历找出边距最近的1个网格容器（单元体）并返回
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns></returns>
        public CellItem FindNearestByRangeH2D(CellRingDiffuseXY d, float x, float y, float maxDistance)
        {
            int cIdxBase = (int)(x * _1_cellSize);
            if (cIdxBase < 0 || cIdxBase >= SideLength) return null;
            int rIdxBase = (int)(y * _1_cellSize);
            if (rIdxBase < 0 || rIdxBase >= SideLength) return null;
            var searchRange = maxDistance + cellSize;

            CellItem rtv = null;
            float maxV = 0;

            var lens = d.lens;
            var idxs = d.idxys;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= SideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= SideLength) continue;
                    var cidx = rIdx * SideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vy = c.y - y;
                        var dd = vx * vx + vy * vy;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;


                        if (v > maxV)
                        {
                            rtv = c;
                            maxV = v;
                        }
                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return rtv;
        }
        /// <summary>
        /// 2D圆形扩散遍历找出边距最近的1个网格容器（单元体）并返回
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns></returns>
        public CellItem FindNearestByRange2D(CellRingDiffuseXZ d, float x, float z, float maxDistance)
        {
            int cIdxBase = (int)(x * _1_cellSize);
            if (cIdxBase < 0 || cIdxBase >= SideLength) return null;
            int rIdxBase = (int)(z * _1_cellSize);
            if (rIdxBase < 0 || rIdxBase >= SideLength) return null;
            var searchRange = maxDistance + cellSize;

            CellItem rtv = null;
            float maxV = 0;

            var lens = d.lens;
            var idxs = d.idxzs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= SideLength) continue;
                    var rIdx = rIdxBase + tmp.z;
                    if (rIdx < 0 || rIdx >= SideLength) continue;
                    var cidx = rIdx * SideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vz = c.z - z;
                        var dd = vx * vx + vz * vz;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;


                        if (v > maxV)
                        {
                            rtv = c;
                            maxV = v;
                        }
                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return rtv;
        }

        /// <summary>
        /// 2D圆形扩散遍历找范围内最多n个网格容器（单元体）的结果的存储容器
        /// </summary>
        public List<CellDistanceInfo> result_FindNearestN_2D = new();

        /// <summary>
        /// 2D空间[横版模式]：圆形扩散遍历找出范围内边缘最近的最多n个结果
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="maxDistance">限制结果集的最大边距</param>
        /// <param name="n"></param>
        /// <returns>返回实际个数</returns>
        public int FindNearestNByRangeH2D(CellRingDiffuseXY d, float x, float y, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * _1_cellSize);
            if (cIdxBase < 0 || cIdxBase >= SideLength) return 0;
            int rIdxBase = (int)(y * _1_cellSize);
            if (rIdxBase < 0 || rIdxBase >= SideLength) return 0;
            //searchRange决定了要扫多远的格子
            var searchRange = maxDistance + cellSize;

            var os = result_FindNearestN_2D;
            os.Clear();

            var lens = d.lens;
            var idxs = d.idxys;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= SideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= SideLength) continue;
                    var cidx = rIdx * SideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vy = c.y - y;
                        var dd = vx * vx + vy * vy;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > 0)
                        {
                            if (os.Count < n)
                            {
                                os.Add(new CellDistanceInfo { distance = v, cell = c });
                                if (os.Count == n)
                                {
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, cell = c };
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                        }

                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return os.Count;
        }
        /// <summary>
        /// 2D空间：圆形扩散遍历找出范围内边缘最近的最多n个结果
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="maxDistance">限制结果集的最大边距</param>
        /// <param name="n"></param>
        /// <returns>返回实际个数</returns>
        public int FindNearestNByRange2D(CellRingDiffuseXZ d, float x, float z, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * _1_cellSize);
            if (cIdxBase < 0 || cIdxBase >= SideLength) return 0;
            int rIdxBase = (int)(z * _1_cellSize);
            if (rIdxBase < 0 || rIdxBase >= SideLength) return 0;
            //searchRange决定了要扫多远的格子
            var searchRange = maxDistance + cellSize;

            var os = result_FindNearestN_2D;
            os.Clear();

            var lens = d.lens;
            var idxs = d.idxzs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= SideLength) continue;
                    var rIdx = rIdxBase + tmp.z;
                    if (rIdx < 0 || rIdx >= SideLength) continue;
                    var cidx = rIdx * SideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vz = c.z - z;
                        var dd = vx * vx + vz * vz;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > 0)
                        {
                            if (os.Count < n)
                            {
                                os.Add(new CellDistanceInfo { distance = v, cell = c });
                                if (os.Count == n)
                                {
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, cell = c };
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                        }

                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return os.Count;
        }

        /// <summary>
        /// 排序result_FindNearestN，注：若改用.Sort(); 函数会造成 128 byte GC
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void Quick_Sort(int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(left, right);
                if (pivot > 1)
                {
                    Quick_Sort(left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    Quick_Sort(pivot + 1, right);
                }
            }
        }

        /// <summary>
        /// 快速排序左右2个result_FindNearestN数组元素，如果存在相同距离则结束并返回右侧距离结果，否则进行交换将较小的距离放在左边数组
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private int Partition(int left, int right)
        {
            var arr = result_FindNearestN_2D;
            var pivot = arr[left];
            while (true)
            {
                while (arr[left].distance < pivot.distance)
                {
                    left++;
                }
                while (arr[right].distance > pivot.distance)
                {
                    right--;
                }
                if (left < right)
                {
                    if (arr[left].distance == arr[right].distance) return right;
                    var temp = arr[left];
                    arr[left] = arr[right];
                    arr[right] = temp;
                }
                else return right;
            }
        }
        #endregion

        #endregion
    }
}

//将3D模型导入Unity并拖动到场景中时会自动为GameObject添加网格过滤器和网格渲染器组件，并将模型的Mesh数据存储在网格过滤器中，然后网格渲染器会引用这个Mesh数据来进行渲染
//在MC插件中，空间团块作为1个GameObject，可使用预制体，也可通过代码给空对象添加网格渲染器、网格过滤器（3D项目中这2个一对）
//3D网格碰撞器的碰撞面mesh可独立设置，注意即使对象用了3D网格渲染器+网格过滤器，代码仍可添加2D碰撞器组件（即便Unity发出警告与3D渲染器冲突，不过建议还是统一）
