using UnityEngine;
using System.Collections;

namespace Uniblocks
{
    /// <summary>
    /// �ſ�����������ſ�ĸ��ֻ������ܣ��洢�ſ��������ݵȡ�
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        // Chunk data

        /// <summary>
        /// �ſ�������������飬���а����ſ���ÿ������ID-�����ؿ����ࣨʹ��GetVoxel��SetVoxel�������ʣ�
        /// ����߳�4�γ�4*4*4=64�����ؿ�����ſ飬ԭ�������¶��㣬�����һ�����ؿ�������(3,3,3)�������䶥���ڵ�3��ȵ�3�߶����ҵ�3���������е�64��Ԫ����[63]��ʾ����һ��Ԫ����[0]����
        /// ��ô��(3,3,3)����[63]�Ĺ�ʽVoxelData[(z * SquaredSideLength) + (y * SideLength) + x]=3*16+3*4+3=63���߳�Ϊ����ʱͬ��
        /// </summary>
        public ushort[] VoxelData; // main voxel data array
        /// <summary>
        /// �ſ�����(x,y,z)���������������ϵ�λ��ֱ����ء��ſ��λ��ʼ����ChunkIndex*Engine.ChunkSideLength����������ſ���˵������λ����1��ʵ�ʾ���Ĭ��16�����ؿ鳤�ȣ���������⣩��
        /// </summary>
        public Index ChunkIndex; // corresponds to the position of the chunk
        /// <summary>
        /// �������ſ�����ֱ�������ſ���������顣��Щ�ſ鰴��Directionö�ٵ�˳��洢(�ϡ��¡��ҡ���ǰ����)������NeighborChunks[0]�������������ſ顣
        /// �������ֻ����һ���ſ���Ҫ����������ſ����������ʱ�Żᱻ���͸��£������ڸ����ſ������ʱ������ζ����ĳЩʱ��������鲻����ȫ���£����ֶ�����GetNeighbors()����������������顣
        /// </summary>
        public Chunk[] NeighborChunks; // references to GameObjects of neighbor chunks
        /// <summary>
        /// �ſ��ǿյ�״̬
        /// </summary>
        public bool Empty;

        // Settings & flags

        /// <summary>
        /// ��������״̬
        /// </summary>
        public bool Fresh = true;
        /// <summary>
        /// ����ʱ״̬
        /// </summary>
        public bool EnableTimeout;
        /// <summary>
        /// ��������״̬�����ڴ�UniblocksServer�������ſ飺���Ϊtrue�����ſ鲻�ṹ������
        /// </summary>
        public bool DisableMesh; // for chunks spawned from UniblocksServer; if true, the chunk will not build a mesh
        /// <summary>
        /// �ѱ����Ϊ��Ҫ�Ƴ�״̬
        /// </summary>
        private bool FlaggedToRemove;
        /// <summary>
        /// ��¼�ſ鱻���ɶ����
        /// </summary>
        public float Lifetime; // how long since the chunk has been spawned

        // update queue

        /// <summary>
        /// �ſ���±��
        /// </summary>
        public bool FlaggedToUpdate;
        /// <summary>
        /// ���ſ���¶���
        /// </summary>
        public bool InUpdateQueue;
        /// <summary>
        /// �����ſ�������ɻ�����������ݺ�ΪTrue
        /// </summary>
        public bool VoxelsDone; // true when this chunk has finished generating or loading voxel data

        // Semi-constants.

        /// <summary>
        /// �ſ�߳�
        /// </summary>
        public int SideLength;
        /// <summary>
        /// �ſ�߳�ƽ��
        /// </summary>
        private int SquaredSideLength;
        /// <summary>
        /// ���񴴽���
        /// </summary>
        private ChunkMeshCreator MeshCreator;

        // object prefabs

        /// <summary>
        /// ����������Ԥ���壩
        /// </summary>
        public GameObject MeshContainer;
        /// <summary>
        /// �ſ���ײ�壨Ԥ���壩
        /// </summary>
        public GameObject ChunkCollider;

        // ==== maintenance ===========================================================================================

        public void Awake()
        { // chunk initialization (load/generate data, set position, etc.)

            // Set variables

            //�ڽű������ص��ſ�λ�ý����ſ����������ſ龭���ſ�������ű�ʵ������������
            ChunkIndex = new Index(transform.position);
            //��ȡ�ſ�Ԥ��߳�
            SideLength = Engine.ChunkSideLength;
            //ȷ���ſ�Ԥ��߳���ƽ��
            SquaredSideLength = SideLength * SideLength;
            //������ǰ�ſ�������ſ��飨��ֹ����ʱ���ޣ���������+1��
            NeighborChunks = new Chunk[6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
            //��ȡ�ſ����񴴽���
            MeshCreator = GetComponent<ChunkMeshCreator>();
            //��������״̬=��
            Fresh = true;

            // Register chunk.ע�᱾�ſ�
            ChunkManager.RegisterChunk(this);

            // Clear the voxel data.����ſ������������飨����һ���µ�ushort���������������ݣ�
            VoxelData = new ushort[SideLength * SideLength * SideLength];

            // Set actual position.�����ſ��������ʵ��λ�ã�Ҳ������Ҫ��������ƽ���ſ�ֻҪ�����ſ������߶ȣ�
            transform.position = ChunkIndex.ToVector3() * SideLength;

            // multiply by scale.����ſ����ű�������Ĭ�ϵ�1.0����ʵ��λ��Ҫ����������������޸�
            transform.position = new Vector3(transform.position.x * transform.localScale.x, transform.position.y * transform.localScale.y, transform.position.z * transform.localScale.z);

            // Grab voxel data.��ȡ�ſ����������

            //����ģʽ�±������Ƿ�����
            if (Engine.EnableMultiplayer && !Network.isServer)
            {
                //�ӷ�������ȡ����
                StartCoroutine(RequestVoxelData());
            }
            //����洢��������ʱ���ԴӴ��̼�����������
            else if (Engine.SaveVoxelData && TryLoadVoxelData() == true)
            {
                // data is loaded through TryLoadVoxelData()
                //���ԴӴ��̼����������ݣ�TryLoadVoxelData()����������������Ѿ����
            }
            else
            {
                //�������������µ���������
                GenerateVoxelData();
            }

        }

        /// <summary>
        /// �Ӵ��̼����������ݡ�
        /// </summary>
        /// <returns></returns>
        public bool TryLoadVoxelData()
        { // returns true if data was loaded successfully, false if data was not found
            //���Դ��ļ������ſ���������ݣ����δ�ҵ������򷵻�false��
            return GetComponent<ChunkDataFiles>().LoadData(); 
        }

        /// <summary>
        /// �����������ݡ��ڰ��ŵ����������Ľű������GenerateVoxelData()
        /// </summary>
        public void GenerateVoxelData()
        { //Calls GenerateVoxelData() in the script assigned in the TerrainGenerator variable.
            GetComponent<TerrainGenerator>().InitializeGenerator(); //��ʼ������������
        }

        /// <summary>
        /// ���ſ��������֪�����ſ������׼������ʱ�����ſ���ӵ����¶���
        /// </summary>
        public void AddToQueueWhenReady()
        { // adds chunk to the UpdateQueue when this chunk and all known neighbors have their data ready
            StartCoroutine(DoAddToQueueWhenReady());
        }

        /// <summary>
        /// [Э��]���ſ��������֪�����ſ������׼������ʱ�����ſ���ӵ����¶���
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAddToQueueWhenReady()
        {
            //���ſ�δ������ص����ɻ���أ�����������������δ׼��������
            while (VoxelsDone == false || AllNeighborsHaveData() == false)
            {
                //����ſ����������ֹͣ����
                if (ChunkManager.StopSpawning)
                { // interrupt if the chunk spawn sequence is stopped. This will be restarted in the correct order from ChunkManager
                    //����ſ����������ֹͣ�������жϣ��⽫���ſ������������ȷ��˳����������
                    yield break;
                }
                //Э��ֹͣ���ȴ���ǰ֡ˢ�»���
                yield return new WaitForEndOfFrame();

            }
            //��ӵ�ǰ�ſ鵽���¶���
            ChunkManager.AddChunkToUpdateQueue(this);
        }

        /// <summary>
        /// ������������ſ��Ƿ�׼��������
        /// </summary>
        /// <returns>��������һ�������ſ�����֪�ĵ���û��׼�������ݣ���ô����false</returns>
        private bool AllNeighborsHaveData()
        { // returns false if at least one neighbor is known but doesn't have data ready yet
            //����ÿ�������ſ�
            foreach (Chunk neighbor in NeighborChunks)
            {
                //�����ſ鲻Ϊ��
                if (neighbor != null)
                {
                    //��������������ſ�δ������ɻ������������
                    if (neighbor.VoxelsDone == false)
                    {
                        //����û��׼����
                        return false;
                    }
                }
            }
            //��׼������
            return true;
        }

        /// <summary>
        /// �ݻ��ſ�ʵ��������
        /// </summary>
        private void OnDestroy()
        { // OnDestroy() ��һ�� MonoBehaviour ����������Ϸ���󼴽�������ʱ���á���ͨ����������Ϸ����ɾ���򵱳������ڼ���ʱ��
            ChunkManager.UnregisterChunk(this);
        }


        // ==== data =======================================================================================

        /// <summary>
        /// ����ſ�������������飨�洢�ž������ؿ�����ࣩ��
        /// </summary>
        public void ClearVoxelData()
        {
            //ָ����һ���µ�ʵ������
            VoxelData = new ushort[SideLength * SideLength * SideLength];
        }

        /// <summary>
        /// ���������������鳤�ȣ��ſ�߳���������С��Ԫ�أ���
        /// </summary>
        /// <returns></returns>
        public int GetDataLength()
        {
            return VoxelData.Length;
        }


        // == set voxel

        /// <summary>
        /// ����ָ���������������������ݣ����޸����ؿ�����ࣩ����������ƽ��1D����������Ϊ����������x,y,z��3D�ռ����ꡣ
        /// </summary>
        /// <param name="rawIndex">ƽ��1D��������</param>
        /// <param name="data">����ID���������������ؿ�����</param>
        public void SetVoxelSimple(int rawIndex, ushort data)
        {
            //�ſ�߳��������������ĵ�rawIndex��Ԫ��=�������ؿ������
            VoxelData[rawIndex] = data;
        }

        /// <summary>
        /// ����ָ�����������������ݣ����޸����ؿ�����ࣩ�����������񡣴��⣬��SetVoxel��ͬ���ſ��������ܳ����ſ�߽�(����x����С��0�Ҵ��ڿ�߳�-1)��
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">����ID���������������ؿ�����</param>
        public void SetVoxelSimple(int x, int y, int z, ushort data)
        {
            VoxelData[(z * SquaredSideLength) + (y * SideLength) + x] = data;
        }

        /// <summary>
        /// ����ָ�����������������ݣ����޸����ؿ�����ࣩ�����������񡣴��⣬��SetVoxel��ͬ���ſ��������ܳ����ſ�߽�(����x����С��0�Ҵ����ſ�߳�-1)��
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">����ID���������������ؿ�����</param>
        public void SetVoxelSimple(Index index, ushort data)
        {
            VoxelData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x] = data;
        }

        /// <summary>
        /// ����ָ�����������������ݣ����޸����ؿ�����ࣩ�����updateMeshΪtrue����Ա���ſ��������и��¡����ſ����������ſ�߽�ʱ���ı���Ӧ�ſ��е��������ݣ��統ǰ��ʵ��������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="data">����ID���������������ؿ�����</param>
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
        /// ����ָ�����������������ݣ����޸����ؿ�����ࣩ�����updateMeshΪtrue����Ա���ſ��������и��¡����ſ����������ſ�߽�ʱ���ı���Ӧ�ſ��е��������ݣ��統ǰ��ʵ��������
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data">����ID���������������ؿ�����</param>
        /// <param name="updateMesh"></param>
        public void SetVoxel(Index index, ushort data, bool updateMesh)
        {
            SetVoxel(index.x, index.y, index.z, data, updateMesh);
        }

        // == get voxel

        /// <summary>
        /// ����ָ���������������������ݣ����޸����ؿ�����ࣩ����������ƽ��1D����������Ϊ����������x,y,z��3D�ռ����ꡣ
        /// </summary>
        /// <param name="rawIndex">ƽ��1D��������</param>
        /// <returns></returns>
        public ushort GetVoxelSimple(int rawIndex)
        {
            return VoxelData[rawIndex];
        }

        /// <summary>
        /// ����ָ�����������������ݣ����޸����ؿ�����ࣩ����GetVoxel��ͬ���ſ��������ܳ����ſ�߽�(����x����С��0�Ҵ����ſ�߳�-1)��
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
        /// ����ָ�����������������ݣ����޸����ؿ�����ࣩ����GetVoxel��ͬ���ſ��������ܳ����ſ�߽�(����x����С��0�Ҵ����ſ�߳�-1)��
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetVoxelSimple(Index index)
        {
            return VoxelData[(index.z * SquaredSideLength) + (index.y * SideLength) + index.x];
        }

        /// <summary>
        /// ����ָ���������������������ݣ����޸����ؿ�����ࣩ�����������������ſ�߽�ʱ�����������ſ��е��������ݣ��統ǰ��ʵ����������û��ʵ�����򷵻�һ��ushort.MaxValue�����ؿ�����ID���������ֵ65535��
        /// </summary>
        /// <param name="x">��������</param>
        /// <param name="y">��������</param>
        /// <param name="z">��������</param>
        /// <returns></returns>
        public ushort GetVoxel(int x, int y, int z)
        {
            //�����������˱��ſ飬��ȥ�����ſ�Ѱ������
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
        /// ����ָ�����������������ݣ����޸����ؿ�����ࣩ�����ſ����������ſ�߽�ʱ�����������ſ��е��������ݣ��統ǰ��ʵ����������û��ʵ�����򷵻�һ��ushort.MaxValue�����ؿ�����ID���������ֵ65535��
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort GetVoxel(Index index)
        {
            return GetVoxel(index.x, index.y, index.z);
        }


        // ==== Flags =======================================================================================

        /// <summary>
        /// ���ſ������Ƴ����
        /// </summary>
        public void FlagToRemove()
        {
            FlaggedToRemove = true;
        }

        /// <summary>
        /// ���ſ����ϸ��±��
        /// </summary>
        public void FlagToUpdate()
        {
            FlaggedToUpdate = true;
        }

        // ==== Update ====

        public void Update()
        {
            //��ǰ֡���ſ��ѱ�����������
            ChunkManager.SavesThisFrame = 0;
        }

        public void LateUpdate()
        {
            // timeout.�����ſ鳬ʱʱ�ļ��
            if (Engine.EnableChunkTimeout && EnableTimeout)
            {
                //�����ſ鳬ʱ����£���¼�ſ鱻�����˶��
                Lifetime += Time.deltaTime;
                //����ſ��������ʱ�䳬�����ſ�����ĳ�ʱʱ��
                if (Lifetime > Engine.ChunkTimeout)
                {
                    //���ſ�����Ƴ����
                    FlaggedToRemove = true;
                }
            }

            //�ſ���±��+���Կ�ʼ�µ��ſ������������ɣ����أ�+û�н�����������+��������������������
            if (FlaggedToUpdate && VoxelsDone && !DisableMesh && Engine.GenerateMeshes)
            { // check if we should update the mesh
                FlaggedToUpdate = false; //�رյ�ǰ�ſ���±��
                RebuildMesh(); //�ſ������ؽ�
            }

            //������Ƴ�
            if (FlaggedToRemove)
            {
                //��������������
                if (Engine.SaveVoxelData)
                { // save data over time, destroy chunk when done
                    //�����ǰδ�ڴ洢�ſ����������
                    if (ChunkDataFiles.SavingChunks == false)
                    { // only destroy chunks if they are not being saved currently
                        //��ǰ֡���ѱ����ſ�����<����
                        if (ChunkManager.SavesThisFrame < Engine.MaxChunkSaves)
                        {
                            //��ǰ֡���ѱ����ſ�����+1
                            ChunkManager.SavesThisFrame++;
                            //�����ſ�����
                            SaveData();
                            //�ݻ��ſ�ʵ��
                            Destroy(this.gameObject);
                        }
                    }
                }
                else
                { // if saving is disabled, destroy immediately.������ò������棬����ֻ�������ݻ��ſ�ʵ��
                    Destroy(this.gameObject);
                }

            }
        }

        /// <summary>
        /// �ſ������ؽ��������ؽ��ſ�����Ȼ��������������ſ������
        /// </summary>
        public void RebuildMesh()
        {
            //�����ؽ��ſ�����
            MeshCreator.RebuildMesh();
            //�������������ſ������
            ConnectNeighbors();
        }

        /// <summary>
        /// �����ſ���������ݵ��ڴ棨TempChunkData�����ڲ�������ChunkDataFiles���SaveData�������統ǰƽ̨��WebPlayer�򱾵ػ��洢��ȡ������������Ч��
        /// </summary>
        private void SaveData()
        {
            //�������������ݱ���
            if (Engine.SaveVoxelData == false)
            {
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.�������������ݱ��湦�ܣ���������������������");
                return;
            }

            GetComponent<ChunkDataFiles>().SaveData();

            //if (Application.isWebPlayer == false) {	
            //	GetComponent<ChunkDataFiles>().SaveData();		
            //}

#if UNITY_WEBPLAYER
            //��ǰƽ̨��WebPlayer�����ػ��洢Ӧȡ��
#else
            //��ǰƽ̨����WebPlayer
            GetComponent<ChunkDataFiles>().SaveData();
#endif
        }

        // ==== Neighbors =======================================================================================

        /// <summary>
        /// �������������ſ������
        /// </summary>
        public void ConnectNeighbors()
        { // update the mesh on all neighbors that have a mesh but don't know about this chunk yet, and also pass them the reference to this chunk

            int loop = 0;
            int i = loop;

            while (loop < 6)
            {
                if (loop % 2 == 0)
                { // for even indexes, add one; for odd, subtract one (because the neighbors are in opposite direction to this chunk)
                    //����ż��������1������������1(��Ϊ�����ſ��뱾�ſ鷽���෴��i���ڴ������ſ�ص����ſ�)
                    i = loop + 1;
                }
                else
                {
                    i = loop - 1;
                }
                //��������ſ鲻Ϊ���������ſ�ġ����������������Ĺ������������񣩲�Ϊ��
                if (NeighborChunks[loop] != null && NeighborChunks[loop].gameObject.GetComponent<MeshFilter>().sharedMesh != null)
                {
                    //��������ſ�������ſ飨����i�����ǻص����ſ飩Ϊ��
                    if (NeighborChunks[loop].NeighborChunks[i] == null)
                    {
                        //�������ſ����������֪�����ſ������׼������ʱ������������ſ���ӵ����¶���
                        NeighborChunks[loop].AddToQueueWhenReady();
                        NeighborChunks[loop].NeighborChunks[i] = this;//�������ſ������"�����ſ�"���и�ֵ
                    }
                }
                //����ѭ�����ڵ����������ſ�
                loop++;
            }
        }

        /// <summary>
        /// ��������ſ���Ϸ����δ�գ����6�������ſ��ȡChunk���ʵ�������Ҹ�ֵ�����ſ��NeighborChunks��������
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
        /// ���ظ���������������ſ��������ڵ��ſ�����������(0,0,0,Direction.left)������(-1,0,0)��
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Index GetAdjacentIndex(Index index, Direction direction)
        {
            return GetAdjacentIndex(index.x, index.y, index.z, direction);
        }
        /// <summary>
        /// ���ظ���������������ſ�����(x,y,z)���ڵ��ſ�����������(0,0,0,Direction.left)������(-1,0,0)��
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
        /// ����Ҫʱ���������ſ飺����ſ�����λ���ſ�ı߽磬���λ�ڸñ߽�������ſ����ϸ��±��
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
        /// ���������ڸ�������λ�õ��ſ���������ע�⣬λ���Լ���˷��ص��ſ������������ſ�ı߽�֮�⡣
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
        /// ���ظ��������������ĵľ�������λ�á�
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 VoxelIndexToPosition(Index index)
        {

            Vector3 localPoint = index.ToVector3(); // convert index to chunk's local position
            return transform.TransformPoint(localPoint);// convert local position to world space

        }

        /// <summary>
        /// ���ظ��������������ĵľ�������λ�á�
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
        /// ���ظ�������λ�õ��������������ݸ����ķ��߷����returnAdjacent����ֵƫ�ư�����ؿ���룬��ͨ���ڶ����ؿ���й���Ͷ��ʱʹ�á�
        /// ������Ͷ��������ؿ��ʱ������λ�ý����������ؿ���(returnAdjacent ==false)�������������ؿ���(returnAdjacent ==true)����˷��ر�����Ͷ����е����ؿ飨�򱻻������ؿ�ڸ����������ؿ飩��
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="returnAdjacent"></param>
        /// <returns></returns>
        public Index PositionToVoxelIndex(Vector3 position, Vector3 normal, bool returnAdjacent)
        { // converts the absolute position to the index of the voxel

            if (returnAdjacent == false)
            {
                position = position - (normal * 0.25f); // push the hit point into the cube.��������ײ��λ�����������壨�ط��߷����������0.25��ȴ���
            }
            else
            {
                position = position + (normal * 0.25f); // push the hit point outside of the cube.��������ײ��λ���Ƴ������壨�ط��߷����˵�����0.25���봦��
            }

            // convert world position to chunk's local position.������λ��ת��Ϊ�ſ�ľֲ�λ��
            Vector3 point = transform.InverseTransformPoint(position);


            // round it to get an int which we can convert to the voxel index.��������õ�һ�����������ǿ��Խ���ת��Ϊ��������
            Index index = new Index(0, 0, 0);
            //�������뵽����Ķ���
            index.x = Mathf.RoundToInt(point.x);
            index.y = Mathf.RoundToInt(point.y);
            index.z = Mathf.RoundToInt(point.z);

            return index; //��������Ķ�����Ϊ���ص���������
        }


        // ==== network ==============

        /// <summary>
        /// [NetWork]��ǰ�ж����ſ����������ڷ�������Ϊ�ͻ����Ŷӣ���������ÿ���յ��ſ���������ʱ��1�����������ѽ����ſ�����ʱ��1
        /// </summary>
        public static int CurrentChunkDataRequests; // how many chunk requests are currently queued in the server for this client. Increased by 1 every time a chunk requests data, and reduced by 1 when a chunk receives data.

        /// <summary>
        /// [NetWork][Э��]�����������ݣ��ȴ�ֱ�����ӵ���������Ȼ��������ſ��������ݵ����󵽷����������û�����Ӿ����ü�����
        /// </summary>
        /// <returns></returns>
        IEnumerator RequestVoxelData()
        { // waits until we're connected to a server and then sends a request for voxel data for this chunk to the server.
          // �ȴ�ֱ�����ӵ���������Ȼ��������ſ��������ݵ����󵽷�����
            while (!Network.isClient)
            {
                CurrentChunkDataRequests = 0; // reset the counter if we're not connected.���û�����Ӿ����ü�����
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
