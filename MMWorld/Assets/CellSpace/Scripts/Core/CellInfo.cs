namespace CellSpace
{
    // Stores a chunk GameObject and an index of a specific voxel in that chunk.
    // The chunk is stored in CellInfo. chunk, and the voxel index in
    // CellInfo.index.  This class can also store the index of one adjacent
    // voxel, which is used in the CellRaycast function - CellInfo.index
    // stores the index of the voxel hit by the raycast, and
    // CellInfo. adjacentIndex stores the voxel adjacent to the hit face.

    /// <summary>
    /// ���ص�Ԫ��Ϣ����ȵ�Ԫ�������൱����ʵ������ʱ����洢���ſ���Ϸ������ſ����ض���Ԫ��������λ����Ϣ����
    /// ����໹�ɴ洢���ڵ�Ԫ����������CPEngine���CellRaycast�������õ���index�洢������Ͷ����еĵ�Ԫ������adjacentIndex��洢�����������ڵĵ�Ԫ��
    /// </summary>
    public class CellInfo
    {
        /// <summary>
        /// ��Ԫ����
        /// </summary>
        public CPIndex index;
        /// <summary>
        /// ���ڵ�Ԫ����
        /// </summary>
        public CPIndex adjacentIndex;
        /// <summary>
        /// �ſ�
        /// </summary>
        public CellChunk chunk;

        /// <summary>
        /// �õ�Ԫ����(pixelX,pixelY,z)���ſ����'chunk'����һ���µ�CellInfo��
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
        /// �õ�Ԫ����(pixelX,pixelY)���ſ����'chunk'����һ���µ�CellInfo��
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
        /// �õ�Ԫ����(pixelX,pixelY,z)�����ڵ�Ԫ����(pixelX,pixelY,z)���ſ����'chunk'����һ���µ�CellInfo��
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
        /// �õ�Ԫ����(pixelX,pixelY)�����ڵ�Ԫ����(pixelX,pixelY)���ſ����'chunk'����һ���µ�CellInfo��
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
        /// �õ�Ԫ�������ſ����'chunk'����һ���µ�CellInfo��
        /// </summary>
        /// <param name="setIndex"></param>
        /// <param name="setChunk"></param>
        public CellInfo(CPIndex setIndex, CellChunk setChunk)
        {
            this.index = setIndex;
            this.chunk = setChunk;
        }
        /// <summary>
        /// �õ�Ԫ���������ڵ�Ԫ�������ſ����'chunk'����һ���µ�CellInfo��
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
        /// ��������ID�����ؿ�Ԥ�������ࣩ
        /// </summary>
        /// <returns></returns>
        public ushort GetCellID()
        {
            return chunk.GetCellID(index);
        }
        /// <summary>
        /// ����������ID��Ӧ�ĵ�Ԫ���������
        /// </summary>
        /// <returns></returns>
        public Cell GetCellType()
        {
            return CPEngine.GetCellType(chunk.GetCellID(index));
        }
        /// <summary>
        /// �������ڵ�Ԫ������ID�����ؿ�Ԥ�������ࣩ
        /// </summary>
        /// <returns></returns>
        public ushort GetAdjacentCell()
        {
            return chunk.GetCellID(adjacentIndex);
        }
        /// <summary>
        /// ��������������ID��Ӧ�ĵ�Ԫ���������
        /// </summary>
        /// <returns></returns>
        public Cell GetAdjacentCellType()
        {
            return CPEngine.GetCellType(chunk.GetCellID(adjacentIndex));
        }

        /// <summary>
        /// ���Ĵ洢��CellInfo�еĵ�Ԫ���ݣ����updateMeshΪtrue����Ա���ſ��������и��¡����ſ����������ſ�߽�ʱ���ı���Ӧ�ſ��еĵ�Ԫ���ݣ��統ǰ��ʵ��������
        /// </summary>
        /// <param name="data">����ID���������������ؿ�����</param>
        /// <param name="updateMesh"></param>
        public void SetCell(ushort data, bool updateMesh)
        {
            chunk.SetCell(index, data, updateMesh);
        }

    }

}