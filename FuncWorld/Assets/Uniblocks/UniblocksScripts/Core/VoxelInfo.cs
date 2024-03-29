namespace Uniblocks
{
    // Stores a chunk GameObject and an index of a specific voxel in that chunk.
    // The chunk is stored in VoxelInfo. chunk, and the voxel index in
    // VoxelInfo.index.  This class can also store the index of one adjacent
    // voxel, which is used in the VoxelRaycast function - VoxelInfo.index
    // stores the index of the voxel hit by the raycast, and
    // VoxelInfo. adjacentIndex stores the voxel adjacent to the hit face.

    /// <summary>
    /// 体素信息（相比体素类型，它相当于在实例建立时额外存储了团块游戏物体和团块中特定体素的索引）。
    /// 这个类还可存储相邻体素索引，这在Engine类的VoxelRaycast函数中用到：index存储被光线投射击中的体素索引，adjacentIndex则存储被击中面相邻的体素。
    /// </summary>
    public class VoxelInfo
    {
        /// <summary>
        /// 体素索引
        /// </summary>
        public Index index;
        /// <summary>
        /// 相邻体素索引
        /// </summary>
        public Index adjacentIndex;
        /// <summary>
        /// 团块
        /// </summary>
        public Chunk chunk;

        /// <summary>
        /// 用体素索引(x,y,z)和团块对象'chunk'创建一个新的VoxelInfo。
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setZ"></param>
        /// <param name="setChunk"></param>
        public VoxelInfo(int setX, int setY, int setZ, Chunk setChunk)
        {
            this.index.x = setX;
            this.index.y = setY;
            this.index.z = setZ;

            this.chunk = setChunk;
        }
        /// <summary>
        /// 用体素索引(x,y,z)，相邻体素索引(x,y,z)和团块对象'chunk'创建一个新的VoxelInfo。
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setZ"></param>
        /// <param name="setXa"></param>
        /// <param name="setYa"></param>
        /// <param name="setZa"></param>
        /// <param name="setChunk"></param>
        public VoxelInfo(int setX, int setY, int setZ, int setXa, int setYa, int setZa, Chunk setChunk)
        {
            this.index.x = setX;
            this.index.y = setY;
            this.index.z = setZ;

            this.adjacentIndex.x = setXa;
            this.adjacentIndex.y = setYa;
            this.adjacentIndex.z = setZa;

            this.chunk = setChunk;
        }
        /// <summary>
        /// 用体素索引和团块对象'chunk'创建一个新的VoxelInfo。
        /// </summary>
        /// <param name="setIndex"></param>
        /// <param name="setChunk"></param>
        public VoxelInfo(Index setIndex, Chunk setChunk)
        {
            this.index = setIndex;

            this.chunk = setChunk;
        }
        /// <summary>
        /// 用体素索引，相邻体素索引和团块对象'chunk'创建一个新的VoxelInfo。
        /// </summary>
        /// <param name="setIndex"></param>
        /// <param name="setAdjacentIndex"></param>
        /// <param name="setChunk"></param>
        public VoxelInfo(Index setIndex, Index setAdjacentIndex, Chunk setChunk)
        {
            this.index = setIndex;
            this.adjacentIndex = setAdjacentIndex;

            this.chunk = setChunk;
        }

        /// <summary>
        /// 返回体素ID（体素块预制体种类）
        /// </summary>
        /// <returns></returns>
        public ushort GetVoxel()
        {
            return chunk.GetVoxel(index);
        }
        /// <summary>
        /// 返回与体素ID对应的体素（组件）。
        /// </summary>
        /// <returns></returns>
        public Voxel GetVoxelType()
        {
            return Engine.GetVoxelType(chunk.GetVoxel(index));
        }
        /// <summary>
        /// 返回相邻体素的体素ID（体素块预制体种类）
        /// </summary>
        /// <returns></returns>
        public ushort GetAdjacentVoxel()
        {
            return chunk.GetVoxel(adjacentIndex);
        }
        /// <summary>
        /// 返回与相邻体素ID对应的体素（组件）。
        /// </summary>
        /// <returns></returns>
        public Voxel GetAdjacentVoxelType()
        {
            return Engine.GetVoxelType(chunk.GetVoxel(adjacentIndex));
        }

        /// <summary>
        /// 更改存储在VoxelInfo中的体素数据，如果updateMesh为true，则对标记团块的网格进行更新。当团块索引超过团块边界时将改变相应团块中的体素数据（如当前已实例化）。
        /// </summary>
        /// <param name="data">体素ID，将变更成这个体素块种类</param>
        /// <param name="updateMesh"></param>
        public void SetVoxel(ushort data, bool updateMesh)
        {
            chunk.SetVoxel(index, data, updateMesh);
        }

    }

}