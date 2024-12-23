namespace CellSpace
{
    // Stores a chunk GameObject and an index of a specific voxel in that chunk.
    // The chunk is stored in CellInfo. chunk, and the voxel index in
    // CellInfo.index.  This class can also store the index of one adjacent
    // voxel, which is used in the CellRaycast function - CellInfo.index
    // stores the index of the voxel hit by the raycast, and
    // CellInfo. adjacentIndex stores the voxel adjacent to the hit face.

    /// <summary>
    /// 体素单元信息。相比单元类型它相当于在实例建立时额外存储了团块游戏物体和团块中特定单元的索引（位置信息）。
    /// 这个类还可存储相邻单元索引，这在CPEngine类的CellRaycast函数中用到：index存储被光线投射击中的单元索引，adjacentIndex则存储被击中面相邻的单元。
    /// </summary>
    public class CellInfo
    {
        /// <summary>
        /// 单元索引
        /// </summary>
        public CPIndex index;
        /// <summary>
        /// 相邻单元索引
        /// </summary>
        public CPIndex adjacentIndex;
        /// <summary>
        /// 团块
        /// </summary>
        public CellChunk chunk;

        /// <summary>
        /// 用单元索引(pixelX,pixelY,z)和团块对象'chunk'创建一个新的CellInfo。
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setZ"></param>
        /// <param name="setChunk"></param>
        public CellInfo(int setX, int setY, int setZ, CellChunk setChunk)
        {
            this.index.x = setX;
            this.index.y = setY;
            if (!CPEngine.HorizontalMode)
            {
                this.index.z = setZ;
            }
            this.chunk = setChunk;
        }
        /// <summary>
        /// 用单元索引(pixelX,pixelY)和团块对象'chunk'创建一个新的CellInfo。
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setChunk"></param>
        public CellInfo(int setX, int setY, CellChunk setChunk)
        {
            this.index.x = setX;
            this.index.y = setY;
            this.chunk = setChunk;
        }

        /// <summary>
        /// 用单元索引(pixelX,pixelY,z)，相邻单元索引(pixelX,pixelY,z)和团块对象'chunk'创建一个新的CellInfo。
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setZ"></param>
        /// <param name="setXa"></param>
        /// <param name="setYa"></param>
        /// <param name="setZa"></param>
        /// <param name="setChunk"></param>
        public CellInfo(int setX, int setY, int setZ, int setXa, int setYa, int setZa, CellChunk setChunk)
        {
            this.index.x = setX;
            this.index.y = setY;
            if (!CPEngine.HorizontalMode)
            {
                this.index.z = setZ;
            }
            this.adjacentIndex.x = setXa;
            this.adjacentIndex.y = setYa;
            if (!CPEngine.HorizontalMode)
            {
                this.adjacentIndex.z = setZa;
            }
            this.chunk = setChunk;
        }
        /// <summary>
        /// 用单元索引(pixelX,pixelY)，相邻单元索引(pixelX,pixelY)和团块对象'chunk'创建一个新的CellInfo。
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setXa"></param>
        /// <param name="setYa"></param>
        /// <param name="setChunk"></param>
        public CellInfo(int setX, int setY, int setXa, int setYa, CellChunk setChunk)
        {
            this.index.x = setX;
            this.index.y = setY;
            this.adjacentIndex.x = setXa;
            this.adjacentIndex.y = setYa;
            this.chunk = setChunk;
        }

        /// <summary>
        /// 用单元索引和团块对象'chunk'创建一个新的CellInfo。
        /// </summary>
        /// <param name="setIndex"></param>
        /// <param name="setChunk"></param>
        public CellInfo(CPIndex setIndex, CellChunk setChunk)
        {
            this.index = setIndex;
            this.chunk = setChunk;
        }
        /// <summary>
        /// 用单元索引，相邻单元索引和团块对象'chunk'创建一个新的CellInfo。
        /// </summary>
        /// <param name="setIndex"></param>
        /// <param name="setAdjacentIndex"></param>
        /// <param name="setChunk"></param>
        public CellInfo(CPIndex setIndex, CPIndex setAdjacentIndex, CellChunk setChunk)
        {
            this.index = setIndex;
            this.adjacentIndex = setAdjacentIndex;
            this.chunk = setChunk;
        }

        /// <summary>
        /// 返回体素ID（体素块预制体种类）
        /// </summary>
        /// <returns></returns>
        public ushort GetCellID()
        {
            return chunk.GetCellID(index);
        }
        /// <summary>
        /// 返回与体素ID对应的单元（组件）。
        /// </summary>
        /// <returns></returns>
        public Cell GetCellType()
        {
            return CPEngine.GetCellType(chunk.GetCellID(index));
        }
        /// <summary>
        /// 返回相邻单元的体素ID（体素块预制体种类）
        /// </summary>
        /// <returns></returns>
        public ushort GetAdjacentCell()
        {
            return chunk.GetCellID(adjacentIndex);
        }
        /// <summary>
        /// 返回与相邻体素ID对应的单元（组件）。
        /// </summary>
        /// <returns></returns>
        public Cell GetAdjacentCellType()
        {
            return CPEngine.GetCellType(chunk.GetCellID(adjacentIndex));
        }

        /// <summary>
        /// 更改存储在CellInfo中的单元数据，如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的单元数据（如当前已实例化）。
        /// </summary>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        /// <param name="updateMesh"></param>
        public void SetCell(ushort data, bool updateMesh)
        {
            chunk.SetCell(index, data, updateMesh);
        }

    }

}