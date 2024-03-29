namespace Uniblocks
{
    // Stores a chunk GameObject and an index of a specific voxel in that chunk.
    // The chunk is stored in VoxelInfo. chunk, and the voxel index in
    // VoxelInfo.index.  This class can also store the index of one adjacent
    // voxel, which is used in the VoxelRaycast function - VoxelInfo.index
    // stores the index of the voxel hit by the raycast, and
    // VoxelInfo. adjacentIndex stores the voxel adjacent to the hit face.

    /// <summary>
    /// ������Ϣ������������ͣ����൱����ʵ������ʱ����洢���ſ���Ϸ������ſ����ض����ص���������
    /// ����໹�ɴ洢������������������Engine���VoxelRaycast�������õ���index�洢������Ͷ����е�����������adjacentIndex��洢�����������ڵ����ء�
    /// </summary>
    public class VoxelInfo
    {
        /// <summary>
        /// ��������
        /// </summary>
        public Index index;
        /// <summary>
        /// ������������
        /// </summary>
        public Index adjacentIndex;
        /// <summary>
        /// �ſ�
        /// </summary>
        public Chunk chunk;

        /// <summary>
        /// ����������(x,y,z)���ſ����'chunk'����һ���µ�VoxelInfo��
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
        /// ����������(x,y,z)��������������(x,y,z)���ſ����'chunk'����һ���µ�VoxelInfo��
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
        /// �������������ſ����'chunk'����һ���µ�VoxelInfo��
        /// </summary>
        /// <param name="setIndex"></param>
        /// <param name="setChunk"></param>
        public VoxelInfo(Index setIndex, Chunk setChunk)
        {
            this.index = setIndex;

            this.chunk = setChunk;
        }
        /// <summary>
        /// ���������������������������ſ����'chunk'����һ���µ�VoxelInfo��
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
        /// ��������ID�����ؿ�Ԥ�������ࣩ
        /// </summary>
        /// <returns></returns>
        public ushort GetVoxel()
        {
            return chunk.GetVoxel(index);
        }
        /// <summary>
        /// ����������ID��Ӧ�����أ��������
        /// </summary>
        /// <returns></returns>
        public Voxel GetVoxelType()
        {
            return Engine.GetVoxelType(chunk.GetVoxel(index));
        }
        /// <summary>
        /// �����������ص�����ID�����ؿ�Ԥ�������ࣩ
        /// </summary>
        /// <returns></returns>
        public ushort GetAdjacentVoxel()
        {
            return chunk.GetVoxel(adjacentIndex);
        }
        /// <summary>
        /// ��������������ID��Ӧ�����أ��������
        /// </summary>
        /// <returns></returns>
        public Voxel GetAdjacentVoxelType()
        {
            return Engine.GetVoxelType(chunk.GetVoxel(adjacentIndex));
        }

        /// <summary>
        /// ���Ĵ洢��VoxelInfo�е��������ݣ����updateMeshΪtrue����Ա���ſ��������и��¡����ſ����������ſ�߽�ʱ���ı���Ӧ�ſ��е��������ݣ��統ǰ��ʵ��������
        /// </summary>
        /// <param name="data">����ID���������������ؿ�����</param>
        /// <param name="updateMesh"></param>
        public void SetVoxel(ushort data, bool updateMesh)
        {
            chunk.SetVoxel(index, data, updateMesh);
        }

    }

}