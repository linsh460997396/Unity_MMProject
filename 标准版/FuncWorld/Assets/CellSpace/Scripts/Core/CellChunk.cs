//#define BEPINEX //BepInEx制作UnityMOD时可手动启用

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 体素单元(Voxel/Cell)的团块组件:管理团块的各种基本功能,存储团块单元数据等
    /// </summary>
    public class CellChunk : MonoBehaviour
    {
        /// <summary>
        /// 代表团块空间的结构体对象.可在团块空间诞生时创建.
        /// </summary>
        [NonSerialized] public CellChunkOP entity;

        #region CellChunk data

        /// <summary>
        /// 团块的单元数据数组,其中包含团块中每个体素ID(即单元种类,使用Cell.GetID和Cell.SetID函数访问)
        /// 假设边长4形成4*4*4=64个单元组成团块,原点是左下顶点,那最后一个单元索引是(3,3,3),代表其顶点在第3深度第3高度往右第3,是数组中第64个元素用[63]表示(第一个元素是[0]),
        /// 那么由(3,3,3)返回[63]的公式CellData[(z * squaredSideLength) + (z * SideLength) + pixelX]=3*16+3*4+3=63,边长为任意时同理.
        /// 当采用横版模式(HorizontalMode为真)时Z值默认为0
        /// </summary>
        public ushort[] cellData; // main cell data array
        /// <summary>
        /// 团块索引(pixelX,z,z),这与它在世界上的位置直接相关,当采用横版模式(HorizontalMode为真)时Z值默认为0.
        /// 团块的位置始终是ChunkIndex*CPEngine.chunkSideLength(比如对于团块来说其索引位置增1就实际经过默认16个单元长度,很容易理解).
        /// </summary>
        public CPIndex chunkIndex; // corresponds to the position of the chunk
        /// <summary>
        /// 包含对团块所有直接相邻团块的引用数组.这些团块按照Direction枚举的顺序存储(上、下、右、左、前、后),例如NeighborChunks[0]返回这个上面的团块.
        /// 这个数组只有在一个团块需要检查它相邻团块的单元数据时才会被填充和更新,比如在更新团块的网格时,这意味着在某些时候这个数组不会完全更新,可手动调用GetNeighbors()来立即更新这个数组.
        /// </summary>
        public CellChunk[] neighborChunks; // references to GameObjects of neighbor chunks
        /// <summary>
        /// 团块是空的状态
        /// </summary>
        public bool empty;

        // Settings & flags

        /// <summary>
        /// 流程新鲜状态
        /// </summary>
        public bool fresh = true;
        /// <summary>
        /// 允许超时状态
        /// </summary>
        public bool enableTimeout;
        /// <summary>
        /// 禁用网格状态(对于从UniblocksServer派生的团块:若为true,则团块不会构建网格)
        /// </summary>
        public bool disableMesh; // for chunks spawned from Server; if true, the chunk will not build a mesh
        /// <summary>
        /// 已被标记为需要移除状态
        /// </summary>
        private bool flaggedToRemove;
        /// <summary>
        /// 记录团块被生成多久了
        /// </summary>
        public float lifetime; // how long since the chunk has been spawned

        // update queue

        /// <summary>
        /// 团块更新标记
        /// </summary>
        public bool flaggedToUpdate;
        /// <summary>
        /// 在团块更新队列
        /// </summary>
        public bool inUpdateQueue;
        /// <summary>
        /// 当此团块完成生成或加载单元数据后为True
        /// </summary>
        public bool cellsDone; // true when this chunk has finished generating or loading cell data

        // Semi-constants.

        /// <summary>
        /// 团块边长(世界绝对坐标长度值),与团块索引相乘可得到团块左下角在世界坐标系的插入点
        /// </summary>
        public int sideLength;
        /// <summary>
        /// 团块边长平方(3D模式下,区域中团块数组索引计算时与Z轴相乘的系数)
        /// </summary>
        private int squaredSideLength;
        /// <summary>
        /// 网格创建者
        /// </summary>
        private CellChunkMeshCreator meshCreator;

        // object prefabs

        /// <summary>
        /// 网格容器(附加网格碰撞器预制体CellChunkAdditionalMesh)
        /// </summary>
        public GameObject meshContainer;
        /// <summary>
        /// 团块碰撞体(预制体)
        /// </summary>
        public GameObject chunkCollider;

        // ==== maintenance ===========================================================================================

        ///// <summary>
        ///// [构造函数]团块空间.
        ///// </summary>
        //public CellChunk() {}

        public void Awake()
        { // chunk initialization (load/generate data, set position, etc.)

            if (CellSpacePrefab.initialized == true && CellSpacePrefab.awakeEnable.ContainsKey("CellChunk"))
            {
                // Set variables

                //在脚本所挂载的团块位置建立团块索引(该团块经由团块管理器脚本实例化到场景)
                chunkIndex = new CPIndex(transform.position);
                //读取团块预设边长
                sideLength = CPEngine.chunkSideLength;
                Debug.Log($"ChunkIndex={chunkIndex} SideLength={sideLength}");
                //确定团块预设边长的平方
                squaredSideLength = sideLength * sideLength;
                //建立当前团块的相邻团块组
                neighborChunks = new CellChunk[6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
                //获取团块网格创建器
                meshCreator = GetComponent<CellChunkMeshCreator>();
                //流程新鲜状态=真
                fresh = true;

                // Register chunk.注册本团块
                CellChunkManager.RegisterChunk(this);

                if (CPEngine.horizontalMode)
                {
                    cellData = new ushort[sideLength * sideLength];
                }
                else
                {
                    // Clear the cell data.清空团块单元数据数组(创建一个新的ushort数组来处理新数据)
                    cellData = new ushort[sideLength * sideLength * sideLength];
                }

                // Set actual position.设置团块在世界的实际位置(世界坐标系中的插入点=团块索引*团块边长)
                transform.position = chunkIndex.ToVector3() * sideLength;

                // multiply by scale.若团块缩放比例不是默认的1.0,则实际位置要根据缩放情况进行修改,2D横板模式下Z=0所以不会变化
                transform.position = new Vector3(transform.position.x * transform.localScale.x, transform.position.y * transform.localScale.y, transform.position.z * transform.localScale.z);

                // Grab cell data.获取团块的单元数据

                //多人模式下使用unet模式且本机并非服务器
                if (CPEngine.enableMultiplayer && CPEngine.netMode == NetMode.unet && !Network.isServer)
                {
                    //从服务器获取数据
                    StartCoroutine(RequestCellData());
                }
                //允许存储单元数据时尝试从磁盘加载单元数据
                else if (CPEngine.saveCellData && TryLoadCellData() == true)
                {
                    // data is loaded through TryLoadCellData()
                    //尝试从磁盘加载单元数据,TryLoadCellData()这个动作在条件判断时已执行
                }
                else
                {
                    //不存在则生成新的单元数据(地形生成器执行噪声种子去随机地形或使用程序内置地形)
                    GenerateCellData();
                }
            }
        }

        /// <summary>
        /// 从磁盘加载单元数据.
        /// </summary>
        /// <returns></returns>
        public bool TryLoadCellData()
        { // returns true if data was loaded successfully, false if data was not found
            //尝试从文件加载团块的单元数据,若未找到数据则返回false.
            return GetComponent<CellChunkDataFiles>().LoadData();
        }

        /// <summary>
        /// 生成单元数据.调用CPTerrainGenerator组件脚本中的GenerateCellData()方法.
        /// </summary>
        public void GenerateCellData()
        {
            GetComponent<CPTerrainGenerator>().InitializeGenerator(); //初始化地形生成器
        }

        /// <summary>
        /// 当团块和所有已知相邻团块的数据准备就绪时,将团块添加到更新队列
        /// </summary>
        public void AddToQueueWhenReady()
        { // adds chunk to the UpdateQueue when this chunk and all known neighbors have their data ready
            StartCoroutine(DoAddToQueueWhenReady());
        }

        /// <summary>
        /// [协程]当团块和所有已知相邻团块的数据准备就绪时,将团块添加到更新队列
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAddToQueueWhenReady()
        {
            //当团块未完成单元的生成或加载,或者所有相邻单元未准备好数据
            while (cellsDone == false || AllNeighborsHaveData() == false)
            {
                //若团块管理器主动停止序列
                if (CellChunkManager.StopSpawning)
                { // interrupt if the chunk spawn sequence is stopped. This will be restarted in the correct order from CellChunkManager
                    //若团块管理器主动停止序列则中断,这将从团块管理器中以正确的顺序重新启动
                    yield break;
                }
                //协程停止,等待当前帧刷新画面
                yield return CPEngine.waitForEndOfFrame;

            }
            //添加当前团块到更新队列
            CellChunkManager.AddChunkToUpdateQueue(this);
        }

        /// <summary>
        /// 检查所有相邻团块是否准备好数据
        /// </summary>
        /// <returns>如至少有一个相邻团块是已知的但还没有准备好数据,那么返回false</returns>
        private bool AllNeighborsHaveData()
        { // returns false if at least one neighbor is known but doesn'transform have data ready yet
            //遍历每个相邻团块
            foreach (CellChunk neighbor in neighborChunks)
            {
                //相邻团块不为空
                if (neighbor != null)
                {
                    //若有任意相邻团块未完成生成或加载单元数据
                    if (neighbor.cellsDone == false)
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
        /// 摧毁团块实例(自身)
        /// </summary>
        private void OnDestroy()
        {
            CellChunkManager.UnRegisterChunk(this);
        }

        // ==== data =======================================================================================

        /// <summary>
        /// 清除团块的单元数据数组(存储着具体单元的种类).
        /// </summary>
        public void ClearCellData()
        {
            if (CPEngine.horizontalMode)
            {
                //指向了一个新的实例数组
                cellData = new ushort[sideLength * sideLength];
            }
            else
            {
                //指向了一个新的实例数组
                cellData = new ushort[sideLength * sideLength * sideLength];
            }

        }

        /// <summary>
        /// 返回单元数据数组长度(2D/3D分别为团块边长的平方/立方大小个元素).
        /// </summary>
        /// <returns></returns>
        public int GetDataLength()
        {
            return cellData.Length;
        }

        // == set cell

        /// <summary>
        /// 更改指定数组索引处的单元数据(即修改单元的种类),函数采用平面1D数组索引作为参数而不是x,z,z的2D/3D空间坐标.
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <param name="data">体素ID,将变更成这个单元种类</param>
        public void SetCellSimple(int rawIndex, ushort data)
        {
            //团块边长的立方个索引的第rawIndex个元素=具体单元种类
            cellData[rawIndex] = data;
        }

        /// <summary>
        /// 更改指定索引处的单元数据(即修改单元的种类)但不更新网格.此外,与SetCell不同,团块索引不能超过团块边界(例如x不能小于0且大于块边长-1).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">体素ID,将变更成这个单元种类</param>
        public void SetCellSimple(int x, int y, int z, ushort data)
        {
            if (CPEngine.horizontalMode)
            {
                cellData[(y * sideLength) + x] = data;
            }
            else
            {
                cellData[(z * squaredSideLength) + (y * sideLength) + x] = data;
            }
        }
        /// <summary>
        /// 更改指定索引处的单元数据(即修改单元的种类)但不更新网格.此外,与SetCell不同,团块索引不能超过团块边界(例如x不能小于0且大于块边长-1).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data">体素ID,将变更成这个单元种类</param>
        public void SetCellSimple(int x, int y, ushort data)
        {
            cellData[(y * sideLength) + x] = data;
        }

        /// <summary>
        /// 更改指定索引处的单元数据(即修改单元的种类)但不更新网格.此外,与SetCell不同,团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">体素ID,将变更成这个单元种类</param>
        public void SetCellSimple(CPIndex index, ushort data)
        {
            if (CPEngine.horizontalMode)
            {
                cellData[(index.y * sideLength) + index.x] = data;
            }
            else
            {
                cellData[(index.z * squaredSideLength) + (index.y * sideLength) + index.x] = data;
            }
        }

        /// <summary>
        /// 更改指定索引处的单元数据(即修改单元的种类).若updateMesh为true,则对标记团块的网格进行更新.当团块索引超过团块边界时将改变相应团块中的单元数据(如当前已实例化).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">体素ID,将变更成这个单元种类</param>
        /// <param name="updateMesh"></param>
        public void SetCell(int x, int y, int z, ushort data, bool updateMesh)
        {
            if (CPEngine.horizontalMode)
            {
                SetCell(x, y, data, updateMesh);
            }
            else
            {
                // if outside of this chunk, change in neighbor instead (if possible)
                if (x < 0)
                {
                    if (neighborChunks[(int)Direction.left] != null)
                        neighborChunks[(int)Direction.left].SetCell(x + sideLength, y, z, data, updateMesh); return;
                }
                else if (x >= sideLength)
                {
                    if (neighborChunks[(int)Direction.right] != null)
                        neighborChunks[(int)Direction.right].SetCell(x - sideLength, y, z, data, updateMesh); return;
                }
                else if (y < 0)
                {
                    if (neighborChunks[(int)Direction.down] != null)
                        neighborChunks[(int)Direction.down].SetCell(x, y + sideLength, z, data, updateMesh); return;
                }
                else if (y >= sideLength)
                {
                    if (neighborChunks[(int)Direction.up] != null)
                        neighborChunks[(int)Direction.up].SetCell(x, y - sideLength, z, data, updateMesh); return;
                }
                else if (z < 0)
                {
                    if (neighborChunks[(int)Direction.back] != null)
                        neighborChunks[(int)Direction.back].SetCell(x, y, z + sideLength, data, updateMesh); return;
                }
                else if (z >= sideLength)
                {
                    if (neighborChunks[(int)Direction.forward] != null)
                        neighborChunks[(int)Direction.forward].SetCell(x, y, z - sideLength, data, updateMesh); return;
                }
                cellData[(z * squaredSideLength) + (y * sideLength) + x] = data;
                if (updateMesh)
                {
                    UpdateNeighborsIfNeeded(x, y, z);
                    FlagToUpdate();
                }
            }
        }
        /// <summary>
        /// 更改指定索引处的单元数据(即修改单元的种类).若updateMesh为true,则对标记团块的网格进行更新.当团块索引超过团块边界时将改变相应团块中的单元数据(如当前已实例化).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data">体素ID,将变更成这个单元种类</param>
        /// <param name="updateMesh"></param>
        public void SetCell(int x, int y, ushort data, bool updateMesh)
        {
            // if outside of this chunk, change in neighbor instead (if possible)
            if (x < 0)
            {
                if (neighborChunks[(int)Direction.left] != null)
                    neighborChunks[(int)Direction.left].SetCell(x + sideLength, y, data, updateMesh); return;
            }
            else if (x >= sideLength)
            {
                if (neighborChunks[(int)Direction.right] != null)
                    neighborChunks[(int)Direction.right].SetCell(x - sideLength, y, data, updateMesh); return;
            }
            else if (y < 0)
            {
                if (neighborChunks[(int)Direction.down] != null)
                    neighborChunks[(int)Direction.down].SetCell(x, y + sideLength, data, updateMesh); return;
            }
            else if (y >= sideLength)
            {
                if (neighborChunks[(int)Direction.up] != null)
                    neighborChunks[(int)Direction.up].SetCell(x, y - sideLength, data, updateMesh); return;
            }
            cellData[(y * sideLength) + x] = data;
            if (updateMesh)
            {
                UpdateNeighborsIfNeeded(x, y);
                FlagToUpdate();
            }
        }

        /// <summary>
        /// 更改指定索引处的单元数据(即修改单元的种类).若updateMesh为true,则对标记团块的网格进行更新.当团块索引超过团块边界时将改变相应团块中的单元数据(如当前已实例化).
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">体素ID,将变更成这个单元种类</param>
        /// <param name="updateMesh"></param>
        public void SetCell(CPIndex index, ushort data, bool updateMesh)
        {
            if (CPEngine.horizontalMode)
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
        /// 返回指定数组索引处的单元数据(即单元的种类),函数采用平面1D数组索引作为参数而不是x,z,z的2D/3D空间坐标.
        /// </summary>
        /// <param name="rawIndex">平面1D数组索引</param>
        /// <returns></returns>
        public ushort GetCellSimple(int rawIndex)
        {
            return cellData[rawIndex];
        }

        /// <summary>
        /// 返回指定索引处的单元数据(即单元的种类).与GetCell不同,团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public ushort GetCellSimple(int x, int y, int z)
        {
            if (CPEngine.horizontalMode)
            {
                return cellData[(y * sideLength) + x];
            }
            else
            {
                return cellData[(z * squaredSideLength) + (y * sideLength) + x];
            }
        }
        /// <summary>
        /// 返回指定索引处的单元数据(即单元的种类).与GetCell不同,团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ushort GetCellSimple(int x, int y)
        {
            return cellData[(y * sideLength) + x];
        }

        /// <summary>
        /// 返回指定索引处的单元数据(即单元的种类).与GetCell不同,团块索引不能超过团块边界(例如x不能小于0且大于团块边长-1).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetCellSimple(CPIndex index)
        {
            if (CPEngine.horizontalMode)
            {
                return cellData[(index.y * sideLength) + index.x];
            }
            else
            {
                return cellData[(index.z * squaredSideLength) + (index.y * sideLength) + index.x];
            }
        }

        /// <summary>
        /// 返回指定单元索引处的单元数据(即单元的种类).当单元索引超过团块边界时将返回相邻团块中的单元数据(如当前已实例化),若没有实例化则返回一个ushort.MaxValue(单元种类ID的最大上限值65535)
        /// </summary>
        /// <param name="x">单元索引</param>
        /// <param name="y">单元索引</param>
        /// <param name="z">单元索引</param>
        /// <returns></returns>
        public ushort GetCellID(int x, int y, int z)
        {
            if (CPEngine.horizontalMode)
            {
                return GetCellID(x, y);
            }
            else
            {
                //单元索引出了本团块,就去相邻团块寻找单元
                if (x < 0)
                {
                    if (neighborChunks[(int)Direction.left] != null)
                    {
                        return neighborChunks[(int)Direction.left].GetCellID(x + sideLength, y, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (x >= sideLength)
                {
                    if (neighborChunks[(int)Direction.right] != null)
                    {
                        return neighborChunks[(int)Direction.right].GetCellID(x - sideLength, y, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (y < 0)
                {
                    if (neighborChunks[(int)Direction.down] != null)
                    {
                        return neighborChunks[(int)Direction.down].GetCellID(x, y + sideLength, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (y >= sideLength)
                {
                    if (neighborChunks[(int)Direction.up] != null)
                    {
                        return neighborChunks[(int)Direction.up].GetCellID(x, y - sideLength, z);
                    }
                    else return ushort.MaxValue;
                }
                else if (z < 0)
                {
                    if (neighborChunks[(int)Direction.back] != null)
                    {
                        return neighborChunks[(int)Direction.back].GetCellID(x, y, z + sideLength);
                    }
                    else return ushort.MaxValue;
                }
                else if (z >= sideLength)
                {
                    if (neighborChunks[(int)Direction.forward] != null)
                    {
                        return neighborChunks[(int)Direction.forward].GetCellID(x, y, z - sideLength);
                    }
                    else return ushort.MaxValue;
                }
                else
                {
                    return cellData[(z * squaredSideLength) + (y * sideLength) + x];
                }
            }
        }
        /// <summary>
        /// 返回指定单元索引处的单元数据(即单元的种类).当单元索引超过团块边界时将返回相邻团块中的单元数据(如当前已实例化),若没有实例化则返回一个ushort.MaxValue(单元种类ID的最大上限值65535)
        /// </summary>
        /// <param name="x">单元索引</param>
        /// <param name="y">单元索引</param>
        /// <returns></returns>
        public ushort GetCellID(int x, int y)
        {
            //单元索引出了本团块,就去相邻团块寻找单元
            if (x < 0)
            {
                if (neighborChunks[(int)Direction.left] != null)
                {
                    return neighborChunks[(int)Direction.left].GetCellID(x + sideLength, y);
                }
                else return ushort.MaxValue;
            }
            else if (x >= sideLength)
            {
                if (neighborChunks[(int)Direction.right] != null)
                {
                    return neighborChunks[(int)Direction.right].GetCellID(x - sideLength, y);
                }
                else return ushort.MaxValue;
            }
            else if (y < 0)
            {
                if (neighborChunks[(int)Direction.down] != null)
                {
                    return neighborChunks[(int)Direction.down].GetCellID(x, y + sideLength);
                }
                else return ushort.MaxValue;
            }
            else if (y >= sideLength)
            {
                if (neighborChunks[(int)Direction.up] != null)
                {
                    return neighborChunks[(int)Direction.up].GetCellID(x, y - sideLength);
                }
                else return ushort.MaxValue;
            }
            else
            {
                return cellData[(y * sideLength) + x];
            }
        }
        /// <summary>
        /// 返回指定索引处的单元数据(即单元的种类).当团块索引超过团块边界时将返回相邻团块中的单元数据(如当前已实例化),若没有实例化则返回一个ushort.MaxValue(单元种类ID的最大上限值65535)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetCellID(CPIndex index)
        {
            if (CPEngine.horizontalMode)
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
            flaggedToRemove = true;
        }

        /// <summary>
        /// 给团块贴上更新标记
        /// </summary>
        public void FlagToUpdate()
        {
            flaggedToUpdate = true;
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
            if (CPEngine.enableChunkTimeout && enableTimeout)
            {
                //允许团块超时情况下,记录团块被生成了多久
                lifetime += Time.deltaTime;
                //若团块的已生成时间超过了团块允许的超时时间
                if (lifetime > CPEngine.chunkTimeout)
                {
                    //将团块打上移除标记
                    flaggedToRemove = true;
                }
            }

            //团块更新标记+可以开始新的团块单元数据生成(加载)+没有禁用网格生成+引擎设置允许生成网格
            if (flaggedToUpdate && cellsDone && !disableMesh && CPEngine.generateMeshes)
            { // check if we should update the mesh
                flaggedToUpdate = false; //关闭当前团块更新标记
                RebuildMesh(); //团块网格重建
            }

            //标记了移除
            if (flaggedToRemove)
            {
                //允许保存单元数据
                if (CPEngine.saveCellData)
                { // save data over time, destroy chunk when done
                    //若当前未在存储团块的单元数据
                    if (CellChunkDataFiles.SavingChunks == false)
                    { // only destroy chunks if they are not being saved currently
                        //当前帧的已保存团块数量<上限
                        if (CellChunkManager.SavesThisFrame < CPEngine.maxChunkSaves)
                        {
                            //当前帧的已保存团块数量+1
                            CellChunkManager.SavesThisFrame++;
                            //保存团块数据
                            SaveData();
                            //摧毁团块实例
                            if (CPEngine.useCellChunkOP)
                            {
                                if (CellChunkOP.dataOP.TryGetValue(chunkIndex, out CellChunkOP op))
                                {
                                    //结构体以当前状态的副本入栈顶,上面的GameObject失活代替摧毁
                                    CellChunkOP.Push(ref op);
                                }
                            }
                            else
                            {
                                Destroy(this.gameObject);
                            }

                        }
                    }
                }
                else
                { // if saving is disabled, destroy immediately.若设置不允许保存,这里只需立即摧毁团块实例
                    if (CPEngine.useCellChunkOP)
                    {
                        if (CellChunkOP.dataOP.TryGetValue(chunkIndex, out CellChunkOP op))
                        {
                            //结构体以当前状态的副本入栈顶,上面的GameObject失活代替摧毁
                            CellChunkOP.Push(ref op);
                        }
                    }
                    else
                    {
                        Destroy(this.gameObject);
                    }
                }

            }
        }

        /// <summary>
        /// 团块网格重建:立即重建团块网格,然后更新所有相邻团块的网格
        /// </summary>
        public void RebuildMesh()
        {
            //立即重建团块网格
            meshCreator.RebuildMesh();
            //更新所有相邻团块的网格
            ConnectNeighbors();
        }

        /// <summary>
        /// 保存团块的单元数据到内存(TempChunkData),内部调用了ChunkDataFiles类的SaveData方法,如当前是WebPlayer则本地化存储会取消(本函数无效)
        /// </summary>
        private void SaveData()
        {
            //禁用了单元数据保存
            if (CPEngine.saveCellData == false)
            {
                Debug.LogWarning("CellSpace: Saving is disabled. You can enable it in the CPEngine Settings.禁用了单元数据保存功能,请在引擎设置中启用它");
                return;
            }

            GetComponent<CellChunkDataFiles>().SaveData();

            //if (Application.isWebPlayer == false) {	
            //	GetComponent<CellChunkDataFiles>().SaveData();		
            //}

#if UNITY_WEBPLAYER
            //当前是WebPlayer,本地化存储应取消
#else
            //当前不是WebPlayer
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
            if (CPEngine.horizontalMode)
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
                    //对于偶数索引加1,对于奇数减1(因为相邻团块与本团块方向相反,i用于从相邻团块回到本团块)
                    i = loop + 1;
                }
                else
                {
                    i = loop - 1;
                }
                //若相邻团块不为空且相邻团块的“网格过滤器”组件的共享网格(主网格)不为空
                if (neighborChunks[loop] != null && neighborChunks[loop].gameObject.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    //若相邻团块的相邻团块(这里i作用是回到本团块)为空
                    if (neighborChunks[loop].neighborChunks[i] == null)
                    {
                        //当相邻团块和其所有已知相邻团块的数据准备就绪时,将这个相邻团块添加到更新队列
                        neighborChunks[loop].AddToQueueWhenReady();
                        neighborChunks[loop].neighborChunks[i] = this;//给相邻团块的属性"相邻团块"进行赋值
                    }
                }
                //继续循环相邻的其他几个团块
                loop++;
            }
        }

        /// <summary>
        /// 若相邻团块游戏对象为空,则从6个相邻团块获取Chunk组件实例,并且赋值给本团块的NeighborChunks属性数组
        /// </summary>
        public void GetNeighbors()
        { // assign the neighbor chunk gameobjects to the NeighborChunks array
            int x = chunkIndex.x;
            int y = chunkIndex.y;
            if (CPEngine.horizontalMode)
            {
                if (neighborChunks[0] == null) neighborChunks[0] = CellChunkManager.GetChunkComponent(x, y + 1);
                if (neighborChunks[1] == null) neighborChunks[1] = CellChunkManager.GetChunkComponent(x, y - 1);
                if (neighborChunks[2] == null) neighborChunks[2] = CellChunkManager.GetChunkComponent(x + 1, y);
                if (neighborChunks[3] == null) neighborChunks[3] = CellChunkManager.GetChunkComponent(x - 1, y);
            }
            else
            {
                int z = chunkIndex.z;
                if (neighborChunks[0] == null) neighborChunks[0] = CellChunkManager.GetChunkComponent(x, y + 1, z);
                if (neighborChunks[1] == null) neighborChunks[1] = CellChunkManager.GetChunkComponent(x, y - 1, z);
                if (neighborChunks[2] == null) neighborChunks[2] = CellChunkManager.GetChunkComponent(x + 1, y, z);
                if (neighborChunks[3] == null) neighborChunks[3] = CellChunkManager.GetChunkComponent(x - 1, y, z);
                if (neighborChunks[4] == null) neighborChunks[4] = CellChunkManager.GetChunkComponent(x, y, z + 1);
                if (neighborChunks[5] == null) neighborChunks[5] = CellChunkManager.GetChunkComponent(x, y, z - 1);
            }
        }

        /// <summary>
        /// 返回给定方向上与给定团块索引相邻的团块索引.例如(0,0,0,Direction.left)将返回(-1,0,0).2D横板模式不考虑Z轴及前后邻居团块
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public CPIndex GetAdjacentIndex(CPIndex index, Direction direction)
        {
            if (CPEngine.horizontalMode)
            {
                return GetAdjacentIndex(index.x, index.y, direction);
            }
            else
            {
                return GetAdjacentIndex(index.x, index.y, index.z, direction);
            }
        }
        /// <summary>
        /// 返回给定方向上与给定团块索引(pixelX,z,z)相邻的团块索引.例如(0,0,0,Direction.left)将返回(-1,0,0).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public CPIndex GetAdjacentIndex(int x, int y, int z, Direction direction)
        { // converts pixelX,z,z direction into a specific index
            if (CPEngine.horizontalMode)
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
        /// 返回给定方向上与给定团块索引(pixelX,z)相邻的团块索引.例如(0,0,Direction.left)将返回(-1,0).
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
        /// 在需要时更新相邻团块:若团块索引位于团块的边界,则对位于该边界的相邻团块贴上更新标记
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void UpdateNeighborsIfNeeded(int x, int y, int z)
        { // if the index lies at the border of a chunk, FlagToUpdate the neighbor at that border
            if (CPEngine.horizontalMode)
            {
                UpdateNeighborsIfNeeded(x, y);
            }
            else
            {
                if (x == 0 && neighborChunks[(int)Direction.left] != null)
                {
                    neighborChunks[(int)Direction.left].GetComponent<CellChunk>().FlagToUpdate();
                }
                else if (x == sideLength - 1 && neighborChunks[(int)Direction.right] != null)
                {
                    neighborChunks[(int)Direction.right].GetComponent<CellChunk>().FlagToUpdate();
                }
                if (y == 0 && neighborChunks[(int)Direction.down] != null)
                {
                    neighborChunks[(int)Direction.down].GetComponent<CellChunk>().FlagToUpdate();
                }
                else if (y == sideLength - 1 && neighborChunks[(int)Direction.up] != null)
                {
                    neighborChunks[(int)Direction.up].GetComponent<CellChunk>().FlagToUpdate();
                }
                if (z == 0 && neighborChunks[(int)Direction.back] != null)
                {
                    neighborChunks[(int)Direction.back].GetComponent<CellChunk>().FlagToUpdate();
                }
                else if (z == sideLength - 1 && neighborChunks[(int)Direction.forward] != null)
                {
                    neighborChunks[(int)Direction.forward].GetComponent<CellChunk>().FlagToUpdate();
                }
            }
        }
        /// <summary>
        /// 在需要时更新相邻团块:若团块索引位于团块的边界,则对位于该边界的相邻团块贴上更新标记
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void UpdateNeighborsIfNeeded(int x, int y)
        { // if the index lies at the border of a chunk, FlagToUpdate the neighbor at that border
            if (x == 0 && neighborChunks[(int)Direction.left] != null)
            {
                neighborChunks[(int)Direction.left].GetComponent<CellChunk>().FlagToUpdate();
            }
            else if (x == sideLength - 1 && neighborChunks[(int)Direction.right] != null)
            {
                neighborChunks[(int)Direction.right].GetComponent<CellChunk>().FlagToUpdate();
            }
            if (y == 0 && neighborChunks[(int)Direction.down] != null)
            {
                neighborChunks[(int)Direction.down].GetComponent<CellChunk>().FlagToUpdate();
            }
            else if (y == sideLength - 1 && neighborChunks[(int)Direction.up] != null)
            {
                neighborChunks[(int)Direction.up].GetComponent<CellChunk>().FlagToUpdate();
            }
        }

        // ==== position / cell index =======================================================================================

        /// <summary>
        /// 返回单元在给定世界位置的团块索引.请注意,位置以及因此返回的团块索引可以在团块的边界之外.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public CPIndex PositionToCellIndex(Vector3 position)
        {
            //世界绝对坐标转父级容器内的相对坐标
            Vector3 point = transform.InverseTransformPoint(position);
            if (CPEngine.horizontalMode)
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
        /// 返回给定单元索引中心的世界绝对位置.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 CellIndexToPosition(CPIndex index)
        {
            Vector3 localPoint = index.ToVector3(); // convert index to chunk's local position
            return transform.TransformPoint(localPoint); // convert local position to world space
        }

        /// <summary>
        /// 返回给定单元索引的世界绝对位置.
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
        /// 返回给定单元索引的世界绝对位置.
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
        /// 返回给定世界位置的单元索引.根据给定的法线方向和returnAdjacent布尔值偏移半个单元距离,这通常在对单元进行光线投射时使用.
        /// 当光线投射击中单元壁时,命中位置将被推入单元内(returnAdjacent ==false)或推入相邻单元内(returnAdjacent ==true),因此返回被光线投射击中的单元(或被击中单元壁附近相邻单元).
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="returnAdjacent"></param>
        /// <returns></returns>
        public CPIndex PositionToCellIndex(Vector3 position, Vector3 normal, bool returnAdjacent)
        { // converts the absolute position to the index of the voxel

            if (returnAdjacent == false)
            {
                position = position - (normal * 0.25f); // push the hit point into the cube.将射线碰撞器位置推入立方体(沿法线方向进到里面0.25深度处)
            }
            else
            {
                position = position + (normal * 0.25f); // push the hit point outside of the cube.将射线碰撞器位置推出立方体(沿法线方向退到面外0.25距离处)
            }

            // convert world position to chunk's local position.将世界位置转换为团块的局部位置
            Vector3 point = transform.InverseTransformPoint(position);


            // round it to get an int which we can convert to the cell index.四舍五入得到一个整数,我们可以将其转换为单元索引
            CPIndex index = new CPIndex(0, 0, 0);
            //四舍五入到最近的顶点
            index.x = Mathf.RoundToInt(point.x);
            index.y = Mathf.RoundToInt(point.y);
            if (!CPEngine.horizontalMode)
            {
                index.z = Mathf.RoundToInt(point.z);
            }
            return index; //将修正后的顶点作为单元的索引返回
        }

        // ==== network ==============

        /// <summary>
        /// [NetWork]当前有多少团块数据请求在服务器上为客户端排队,当服务器每次收到团块数据请求时增1,当服务器已接收团块数据时减1
        /// </summary>
        public static int CurrentChunkDataRequests; // how many chunk requests are currently queued in the server for this client. Increased by 1 every time a chunk requests data, and reduced by 1 when a chunk receives data.

        /// <summary>
        /// [NetWork][协程]请求单元数据:等待直到连接到服务器,然后发送这个团块单元数据的请求到服务器,若没有连接就重置计数器
        /// </summary>
        /// <returns></returns>
        private IEnumerator RequestCellData()
        { // waits until we're connected to a server and then sends a request for cell data for this chunk to the server.
          // 等待直到连接到服务器,然后发送这个团块单元数据的请求到服务器
            while (!Network.isClient)
            {
                CurrentChunkDataRequests = 0; // reset the counter if we're not connected.若没有连接就重置计数器
                yield return CPEngine.waitForEndOfFrame;
            }
            while (CPEngine.maxChunkDataRequests != 0 && CurrentChunkDataRequests >= CPEngine.maxChunkDataRequests)
            {
                yield return CPEngine.waitForEndOfFrame;
            }

            CurrentChunkDataRequests++;
            CPEngine.network.GetComponent<NetworkView>().RPC("SendCellData", RPCMode.Server, Network.player, chunkIndex.x, chunkIndex.y, chunkIndex.z);
        }
        #endregion

    }
}
