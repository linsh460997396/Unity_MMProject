using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// Controls spawning and destroying chunks

namespace CellSpace
{
    /// <summary>
    /// �ſ����������������ſ鴴���ʹݻ١�����÷���Unity������½�һ���ն���CPEngine�����ѽű��ϵ����λ�ü����أ�UnityҪ��һ��cs�ļ�ֻ��һ���࣬�����������ļ���һ�£�
    /// </summary>
    public class CellChunkManager : MonoBehaviour
    {

        /// <summary>
        /// �ſ�Ԥ���壨CellChunk Prefab���������������Ѿ��Ǹ�ʵ�����ɿ��ɻ�ڴ�ģ�����ݣ�����δ�����ڼ���ʵ����������������Ϊ����ʵ����
        /// </summary>
        public GameObject ChunkObject;

        // chunk lists

        /// <summary>
        /// �ſ��飨�ֵ䣩���洢����������Chunkʵ�������ã��ſ�ͨ��Awake����ChunkManager.RegisterChunk���Լ���ӵ��ֵ��У��ַ���key��ӦChunk����������ѭ��pixelX,pixelY,z���ĸ�ʽ��
        /// </summary>
        public static Dictionary<string, CellChunk> Chunks;

        /// <summary>
        /// �ſ���¶��У��б�洢���������ȼ�������ſ飬��ProcessChunkQueueѭ���д���
        /// </summary>
        private static List<CellChunk> ChunkUpdateQueue; // stores chunks ordered by update priority. Processed in the ProcessChunkQueue loop
        /// <summary>
        /// �ſ������б������SpawnChunks����ʱ���ٵ�chunk��
        /// </summary>
        private static List<CellChunk> ChunksToDestroy; // chunks to be destroyed at the end of SpawnChunks

        /// <summary>
        /// ��ǰ֡���ſ��ѱ������������ڸ�ÿ֡���ſ鱣������MaxChunkSaves���бȶԣ�
        /// </summary>
        public static int SavesThisFrame;


        // global flags

        /// <summary>
        /// ���ChunkManager��ǰ�������ɿ飬��Ϊtrue
        /// </summary>
        public static bool SpawningChunks; // true if the CellChunkManager is currently spawning chunks
        /// <summary>
        /// ��Ϊtrueʱ����ǰ��SpawnChunks���ж���������ֹ(Ȼ��������Ϊfalse)
        /// </summary>
        public static bool StopSpawning; // when true, the current SpawnChunks sequence is aborted (and this is set back to false afterwards)
        /// <summary>
        /// �ſ��������ʼ��״̬
        /// </summary>
        public static bool Initialized;

        // local flags

        /// <summary>
        /// �����Ƿ���Կ�ʼ�����ſ�
        /// </summary>
        private bool Done;
        /// <summary>
        /// ������������
        /// </summary>
        private CPIndex LastRequest;
        /// <summary>
        /// Ŀ����Ϸ���� = 1f / CPEngine.TargetFPS
        /// </summary>
        private float targetFrameDuration;
        /// <summary>
        /// ֡��ʱ����ͳ����ʱ��
        /// </summary>
        private Stopwatch frameStopwatch;
        /// <summary>
        /// �ſ鴴������
        /// </summary>
        private int SpawnQueue;

        void Start()
        {
            //�趨��Ϸ���ڣ�ÿ֡������
            targetFrameDuration = 1f / CPEngine.TargetFPS;
            //�����ſ��飨�ֵ䣩
            Chunks = new Dictionary<string, CellChunk>();
            //�����ſ�����б�
            ChunkUpdateQueue = new List<CellChunk>();
            //����֡��ʱ���������¼������ʱ��
            frameStopwatch = new Stopwatch();

            //set correct scale of trigger collider and additional mesh collider.���ô�������ײ��͸���������ײ������ȷ����

            //�����ſ����ű���Ϊ�ſ�ʵ����ǰ�����ű���
            CPEngine.ChunkScale = ChunkObject.transform.localScale;
            //���ø���������ײ������ȷ����
            ChunkObject.GetComponent<CellChunk>().MeshContainer.transform.localScale = ChunkObject.transform.localScale;
            //���ô�������ײ�����ȷ����
            ChunkObject.GetComponent<CellChunk>().ChunkCollider.transform.localScale = ChunkObject.transform.localScale;

            //�����������
            Done = true;
            //�ſ鴴��״̬����Ϊ��
            SpawningChunks = false;
            //�ſ��������ʼ�����
            Initialized = true;
        }

        /// <summary>
        /// ����֡��ʱ����ͳ��ÿ֡������ʱ��
        /// </summary>
        private void ResetFrameStopwatch()
        {
            frameStopwatch.Stop();
            frameStopwatch.Reset();
            frameStopwatch.Start();
        }

        /// <summary>
        /// ����ſ鵽���¶���
        /// </summary>
        /// <param name="chunk"></param>
        public static void AddChunkToUpdateQueue(CellChunk chunk)
        {
            //����ſ���¶��������ѷ���������ſ�
            if (ChunkUpdateQueue.Contains(chunk) == false)
            {
                //�ſ���¶�������Ӷ����ſ�
                ChunkUpdateQueue.Add(chunk);
            }
        }

        /// <summary>
        /// �����ſ���У���SpawnChunksѭ�����������¿����񣩣�
        /// ���µ�һ���鲢����Ӷ�����ɾ��,��ǰ�ſ鲻Ϊ����û�б���������ʱ���½�������,��ǰ�ſ��Fresh������Ϊ�ٲ��Ӷ�����ɾ����
        /// </summary>
        private void ProcessChunkQueue()
        { // called from the SpawnChunks loop to update chunk meshes

            // update the first chunk and remove it from the queue.���µ�һ���鲢����Ӷ�����ɾ��
            CellChunk currentChunk = ChunkUpdateQueue[0];

            if (!currentChunk.Empty && !currentChunk.DisableMesh)
            {
                //��ǰ�ſ鲻Ϊ����û�б���������ʱ���½�������
                currentChunk.RebuildMesh();
            }
            //��ǰ�ſ��Fresh������Ϊ��
            currentChunk.Fresh = false;
            //�Ӷ�����ɾ����
            ChunkUpdateQueue.RemoveAt(0);
        }

        /// <summary>
        /// �����ſ����ѭ����Э�̣��ļ���״̬
        /// </summary>
        private bool ProcessChunkQueueLoopActive;
        /// <summary>
        /// �����ſ����ѭ����Э�̣�����SpawnChunksδ����ʱ��Update���á�
        /// �����ſ���¶��У�������Ϊ0�����ſ鴴��״̬Ϊ����û���ֶ�ǿ����ֹʱ�������ſ���С�
        /// ���֡��ʱ��������ʱ�䳬��Ŀ����Ϸ���������Э�̣�����Ⱦ�������������GUI��ȴ���֡������
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProcessChunkQueueLoop()
        { // called from Update when SpawnChunks is not running
            ProcessChunkQueueLoopActive = true;
            while (ChunkUpdateQueue.Count > 0 && !SpawningChunks && !StopSpawning)
            {
                //�����ſ���¶��У�������Ϊ0�����ſ鴴��״̬Ϊ����û���ֶ�ǿ����ֹʱ�������ſ����
                ProcessChunkQueue();
                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                {
                    //���֡��ʱ��������ʱ�䳬��Ŀ����Ϸ���������Э�̣�����Ⱦ�������������GUI��ȴ���֡����
                    yield return new WaitForEndOfFrame();
                }
            }
            ProcessChunkQueueLoopActive = false;
        }

        /// <summary>
        /// ע���ſ飨�����ſ��������ӵ�ȫ���ſ����ֵ��У���ChunkIndexΪ������ChunkΪֵ�����������ſ���Awake�������Զ���ɵġ�
        /// </summary>
        /// <param name="chunk"></param>
        public static void RegisterChunk(CellChunk chunk)
        { // adds a reference to the chunk to the global chunk list
            Chunks.Add(chunk.ChunkIndex.ToString(), chunk);
        }

        /// <summary>
        /// ע���ſ飨��ȫ���ſ��ֵ��У����������ſ鱻����ʱ�Զ���ɵġ�
        /// </summary>
        /// <param name="chunk"></param>
        public static void UnRegisterChunk(CellChunk chunk)
        {
            Chunks.Remove(chunk.ChunkIndex.ToString());
        }

        /// <summary>
        /// ����ָ��Index(pixelX, pixelY, z)���ſ����Ϸ��������������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject GetChunk(int x, int y, int z)
        { // returns the gameObject of the chunk with the specified pixelX,pixelY,z, or returns null if the object is not instantiated
            if (CPEngine.HorizontalMode)
            {
                return GetChunk(new CPIndex(x, y));
            }
            else
            {
                return GetChunk(new CPIndex(x, y, z));
            }
        }
        /// <summary>
        /// ����ָ��Index(pixelX, pixelY, z)���ſ����Ϸ��������������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static GameObject GetChunk(int x, int y)
        { // returns the gameObject of the chunk with the specified pixelX,pixelY, or returns null if the object is not instantiated
            return GetChunk(new CPIndex(x, y));
        }

        /// <summary>
        /// ����ָ��Index���ſ����Ϸ��������������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static GameObject GetChunk(CPIndex index)
        {
            CellChunk chunk = GetChunkComponent(index);
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
        /// ����ָ��Index(pixelX, pixelY, z)���ſ飬�������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static CellChunk GetChunkComponent(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                return GetChunkComponent(new CPIndex(x, y));
            }
            else
            {
                return GetChunkComponent(new CPIndex(x, y, z));
            }
        }
        /// <summary>
        /// ����ָ��Index(pixelX, pixelY)���ſ飬�������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static CellChunk GetChunkComponent(int x, int y)
        {
            return GetChunkComponent(new CPIndex(x, y));
        }

        /// <summary>
        /// ����ָ��Index���ſ飬�������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static CellChunk GetChunkComponent(CPIndex index)
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
        /// ���ɴ�������(pixelX,pixelY,z)�ĵ����ſ鲢���ظ���Ϸ�������������򷵻������ɵ���Ϸ����
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject SpawnChunk(int x, int y, int z)
        { // spawns a single chunk (only if it's not already spawned)
            if (CPEngine.HorizontalMode)
            {
                return SpawnChunk(x, y);
            }
            else
            {
                GameObject chunk = GetChunk(x, y, z);
                if (chunk == null)
                {
                    return CPEngine.ChunkManagerInstance.DoSpawnChunk(new CPIndex(x, y, z));
                }
                else return chunk;
            }
        }
        /// <summary>
        /// ���ɴ�������(pixelX,pixelY)�ĵ����ſ鲢���ظ���Ϸ�������������򷵻������ɵ���Ϸ����
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject SpawnChunk(int x, int y)
        { // spawns a single chunk (only if it's not already spawned)
            GameObject chunk = GetChunk(x, y);
            if (chunk == null)
            {
                return CPEngine.ChunkManagerInstance.DoSpawnChunk(new CPIndex(x, y));
            }
            else return chunk;
        }

        /// <summary>
        /// ���ɴ��������ĵ����ſ鲢���ظ���Ϸ�������������򷵻������ɵ���Ϸ����
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns></returns>
        public static GameObject SpawnChunk(CPIndex index)
        { // spawns a single chunk (only if it's not already spawned)

            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                return CPEngine.ChunkManagerInstance.DoSpawnChunk(index);
            }
            else return chunk;
        }

        /// <summary>
        /// �������������������ɵ����ſ����Ϸ���󲢷��أ������㽫��Ϊ�ſ��������λ�ã���ǰ���Ǹ�����λ����δʵ�����ſ���󡣱�����������������ɲ����ó�ʱ���ɷ������ڶ�����Ϸ��ʹ�ã��������ſ�ʵ���Ѵ����򲻻�����������ɺ����ó�ʱ�����򵥵ط��ظö���
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns>�����ſ����Ϸ�������</returns>
        public static GameObject SpawnChunkFromServer(CPIndex index)
        { // spawns a chunk and disables mesh generation and enables timeout (used by the server in multiplayer)
            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                chunk = CPEngine.ChunkManagerInstance.DoSpawnChunk(index);
                CellChunk chunkComponent = chunk.GetComponent<CellChunk>();
                chunkComponent.EnableTimeout = true;
                chunkComponent.DisableMesh = true;
                return chunk;
            }
            else return chunk; // don'transform disable mesh generation and don'transform enable timeout for chunks that are already spawned
        }

        /// <summary>
        /// ����������������(pixelX,pixelY,z)���ɵ����ſ����Ϸ���󲢷��أ������㽫��Ϊ�ſ��������λ�ã�2Dģʽ��z��Ϊ0����ǰ���Ǹ�����λ����δʵ�����ſ���󡣱�����������������ɲ����ó�ʱ���ɷ������ڶ�����Ϸ��ʹ�ã��������ſ�ʵ���Ѵ����򲻻�����������ɺ����ó�ʱ�����򵥵ط��ظö���
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns>�����ſ����Ϸ�������</returns>
        public static GameObject SpawnChunkFromServer(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                return SpawnChunkFromServer(new CPIndex(x, y));
            }
            else
            {
                return SpawnChunkFromServer(new CPIndex(x, y, z));
            }
        }
        /// <summary>
        /// ����������������(pixelX,pixelY)���ɵ����ſ����Ϸ���󲢷��أ������㽫��Ϊ�ſ��������λ�ã�2Dģʽ��z��Ϊ0����ǰ���Ǹ�����λ����δʵ�����ſ���󡣱�����������������ɲ����ó�ʱ���ɷ������ڶ�����Ϸ��ʹ�ã��������ſ�ʵ���Ѵ����򲻻�����������ɺ����ó�ʱ�����򵥵ط��ظö���
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns>�����ſ����Ϸ�������</returns>
        public static GameObject SpawnChunkFromServer(int x, int y)
        {
            return SpawnChunkFromServer(new CPIndex(x, y));
        }

        /// <summary>
        /// �����ſ���嶯�������ſ�Ԥ�������ſ�����λ�ô����ſ���Ϸ���壬��ȡ�����ϵ��ſ���������ſ����������ӵ������У���󷵻��ſ���Ϸ���塣
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns></returns>
        GameObject DoSpawnChunk(CPIndex index)
        {
            //���ſ�Ԥ�������ſ�����λ�ô����ſ���Ϸ����
            GameObject chunkObject = Instantiate(ChunkObject, index.ToVector3(), transform.rotation);
            //��ȡ�����ϵ��ſ����
            CellChunk chunk = chunkObject.GetComponent<CellChunk>();
            //���ſ����������ӵ�������
            AddChunkToUpdateQueue(chunk);
            //�����ſ���Ϸ����
            return chunkObject;
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱȡ����λ��(pixelX,pixelY,z)ת��Ϊ�ſ�������Ȼ�����Ը�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(float x, float y, float z)
        { // take world pos, convert to chunk index.ȡ����λ�ã�ת��Ϊ������
            CPIndex index;
            if (CPEngine.HorizontalMode)
            {
                index = CPEngine.PositionToChunkIndex(new Vector3(x, y, 0f));
            }
            else
            {
                index = CPEngine.PositionToChunkIndex(new Vector3(x, y, z));
            }
            CPEngine.ChunkManagerInstance.TrySpawnChunks(index);
        }
        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱȡ����λ��(pixelX,pixelY,0f)ת��Ϊ�ſ�������Ȼ�����Ը�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ��2��int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void SpawnChunks(float x, float y)
        { // take world pos, convert to chunk index.ȡ����λ�ã�ת��Ϊ������
            CPIndex index = CPEngine.PositionToChunkIndex(new Vector3(x, y, 0f));
            CPEngine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱȡ����λ��ת��Ϊ�ſ�������Ȼ�����Ը�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="position">֧��2D/3D��Vector</param>
        public static void SpawnChunks(Vector3 position)
        {
            CPIndex index = CPEngine.PositionToChunkIndex(position);
            CPEngine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����(pixelX,pixelY,z)Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// ����λ��ת�ſ�����ʾ����currentPos = CPEngine.PositionToChunkIndex(engineTransform.position);
        /// </summary>
        /// <param name="x">�ſ�����</param>
        /// <param name="y">�ſ�����</param>
        /// <param name="z">�ſ�����</param>
        public static void SpawnChunks(int x, int y, int z)
        { // take chunk index, no conversion needed
            if (CPEngine.HorizontalMode)
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(x, y);
            }
            else
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(x, y, z);
            }

        }
        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="index"></param>
        public static void SpawnChunks(CPIndex index)
        {
            if (CPEngine.HorizontalMode)
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y);
            }
            else
            {
                CPEngine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y, index.z);
            }
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="index"></param>
        private void TrySpawnChunks(CPIndex index)
        {
            if (CPEngine.HorizontalMode)
            {
                TrySpawnChunks(index.x, index.y);
            }
            else
            {
                TrySpawnChunks(index.x, index.y, index.z);
            }
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����(pixelX,pixelY,z)Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void TrySpawnChunks(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                TrySpawnChunks(x, y);
            }
            else
            {
                if (Done == true)
                { //�����ǰû�������ſ鶯�����Ǿ���������������
                    StartSpawnChunks(x, y, z);
                }
                else
                {
                    //����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ��
                    LastRequest = new CPIndex(x, y, z);
                    SpawnQueue = 1; //�������м���Ϊ1
                    StopSpawning = true; //������ϴ���״̬��SpawnChunks���н�����ֹ��
                    ChunkUpdateQueue.Clear(); //����ſ���¶���
                }
            }
        }
        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����(pixelX,pixelY)Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ��2��int��1��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void TrySpawnChunks(int x, int y)
        {
            if (Done == true)
            { // if we're not spawning chunks at the moment, just spawn them normally.�����ǰû�������ſ鶯�����Ǿ���������������
                StartSpawnChunks(x, y);
            }
            else
            {
                //if we are spawning chunks already, flag to spawn again once the previous round is finished using the last requested position as origin.
                //����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ��
                LastRequest = new CPIndex(x, y);
                SpawnQueue = 1; //�������м���Ϊ1
                StopSpawning = true; //������ϴ���״̬��SpawnChunks���н�����ֹ��
                ChunkUpdateQueue.Clear(); //����ſ���¶���
            }
        }

        /// <summary>
        /// ÿ֡������ִ����Ӧ�ſ飨���Σ���������
        /// </summary>
        public void Update()
        {
            //�����ſ鴴���������ſ������ͬ�⿪ʼ�����ſ�ʱ
            if (SpawnQueue == 1 && Done == true)
            { // if there is a chunk spawn queued up, and if the previous one has finished, run the queued chunk spawn.�����µ��ſ鴴���������ſ������֪ͨǰһ������ɣ��ǾͿ�ʼ�¸������ſ�����

                //�ſ鴴����������
                SpawnQueue = 0;
                if (CPEngine.HorizontalMode)
                {
                    //��ʼ�����ſ飨���Σ���SpawningChunks=�棬�ſ��������Ϊ��ֹ�����ſ飨Done=false����ֹUpdate�п�ʼ�¸������ſ鶯������תΪ������ǰ����ſ鴴������ֱ����ɣ����������Э��������ȱʧ���ſ�
                    StartSpawnChunks(LastRequest.x, LastRequest.y);
                }
                else
                {
                    //��ʼ�����ſ飨���Σ���SpawningChunks=�棬�ſ��������Ϊ��ֹ�����ſ飨Done=false����ֹUpdate�п�ʼ�¸������ſ鶯������תΪ������ǰ����ſ鴴������ֱ����ɣ����������Э��������ȱʧ���ſ�
                    StartSpawnChunks(LastRequest.x, LastRequest.y, LastRequest.z);
                }
            }
            // if not currently spawning chunks, process any queued chunks here instead.����ǰû�µ��ſ鴴�����������ﲽ����ǰ�ſ鴴������ֱ�����
            // ����û����Update�����ٲ�������������Э���Զ��������������ʱ�趨״̬����ʼ��һ��Э�̣�
            if (!SpawningChunks && !ProcessChunkQueueLoopActive && ChunkUpdateQueue != null && ChunkUpdateQueue.Count > 0)
            {
                //����Э���������ſ鴴������ֱ�����
                StartCoroutine(ProcessChunkQueueLoop());
            }
            //����֡��ʱ��
            ResetFrameStopwatch();
        }

        /// <summary>
        /// ��ʼ�����ſ飨���Σ���SpawningChunks=�棬�ſ��������Ϊ��ֹ�����ſ飨Done=false����ֹUpdate�п�ʼ�¸������ſ鶯������תΪ������ǰ����ſ鴴������ֱ����ɣ����������Э��������ȱʧ���ſ�
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        private void StartSpawnChunks(int originX, int originY, int originZ)
        {
            //�ſ鴴��״̬Ϊ��
            SpawningChunks = true;
            //�ſ��������Ϊ��ֹ�����ſ飨��ֹUpdate�п�ʼ�¸������ſ鶯������תΪ������ǰ����ſ鴴������ֱ����ɣ�
            Done = false;
            //��ȡ�ſ鴴������
            int range = CPEngine.ChunkSpawnDistance;
            //����Э��������ȱʧ���ſ�
            if (CPEngine.HorizontalMode)
            {
                StartCoroutine(SpawnMissingChunks(originX, originY, range));
            }
            else
            {
                StartCoroutine(SpawnMissingChunks(originX, originY, originZ, range));
            }
        }
        /// <summary>
        /// ��ʼ�����ſ飨���Σ���SpawningChunks=�棬�ſ��������Ϊ��ֹ�����ſ飨Done=false����ֹUpdate�п�ʼ�¸������ſ鶯������תΪ������ǰ����ſ鴴������ֱ����ɣ����������Э��������ȱʧ���ſ�
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        private void StartSpawnChunks(int originX, int originY)
        {
            //�ſ鴴��״̬Ϊ��
            SpawningChunks = true;
            //�ſ��������Ϊ��ֹ�����ſ飨��ֹUpdate�п�ʼ�¸������ſ鶯������תΪ������ǰ����ſ鴴������ֱ����ɣ�
            Done = false;
            //��ȡ�ſ鴴������
            int range = CPEngine.ChunkSpawnDistance;
            //����Э��������ȱʧ���ſ�
            StartCoroutine(SpawnMissingChunks(originX, originY, range));
        }

        /// <summary>
        /// [Э��]����ȱʧ���ſ飨���Ƚ������Զ���ſ���ӵ��ݻٶ��У�
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int originZ, int range)
        {
            if (CPEngine.HorizontalMode)
            {
                SpawnMissingChunks(originX, originY, range);
            }
            else
            {
                //��ȡ�ſ鴴���߶ȷ�Χ��ֵ��Y�ᣩ
                int heightRange = CPEngine.HeightRange;
                //����ſ���¶��У������������ѭ��������ȷ��˳���������
                ChunkUpdateQueue = new List<CellChunk>();
                //��ǹ����ſ鲻���Ƴ���Χ��
                ChunksToDestroy = new List<CellChunk>();
                //�����ſ�����ÿ���洢���ſ�ʵ������֤�Ƿ���Ҫ�ݻ���ά�����Զ��
                foreach (CellChunk chunk in Chunks.Values)
                {
                    //�ſ������㣨����λ�ã����ɫ������λ���糬���ſ鴴������+ChunkDespawnDistance��Ĭ��8+3=11��
                    if (Vector2.Distance(new Vector2(chunk.ChunkIndex.x, chunk.ChunkIndex.z), new Vector2(originX, originZ)) > range + CPEngine.ChunkDespawnDistance)
                    {
                        //����ЩXZƽ������Զ���ſ���ӵ��ݻٶ���
                        ChunksToDestroy.Add(chunk);
                    }
                    //����ſ������㣨����λ�ã��ĸ߶ȳ����ſ鴴������+ChunkDespawnDistance��Ĭ��8+3=11��
                    else if (Mathf.Abs(chunk.ChunkIndex.y - originY) > range + CPEngine.ChunkDespawnDistance)
                    {
                        //����Щ��ֱ���루Y��߶ȣ���Զ���ſ���ӵ��ݻٶ���
                        ChunksToDestroy.Add(chunk);
                    }
                }
                //��ѭ����ʼ����ȱʧ���ſ�
                for (int currentLoop = 0; currentLoop <= range; currentLoop++)
                {//�����ſ鴴�����루0��ԭ�ؿ飬1����Χ��չһ��...�Դ����ࣩ
                    for (var x = originX - currentLoop; x <= originX + currentLoop; x++)
                    { //������Χ�����п��ܵ��ſ�����
                        for (var y = originY - currentLoop; y <= originY + currentLoop; y++)
                        {
                            for (var z = originZ - currentLoop; z <= originZ + currentLoop; z++)
                            {
                                //�����ĸ߶Ⱦ���ֵ���ܳ����߶ȷ�Χ����ô�������������Ч��
                                if (Mathf.Abs(y) <= heightRange)
                                { //��֤�Ƿ������߶ȷ�Χ֮����ſ�
                                    //�����ſ���XZƽ���Ǹ���2���߳����� < 1.3�������Ի�ﲻ���Ǹ�������ͬʱ��֤�Ƿ�KeepOneChunk�����ٱ�֤ԭ�����1���ſ飩
                                    if (Mathf.Abs(originX - x) + Mathf.Abs(originZ - z) < range * 1.3f ||(CPEngine.KeepOneChunk && x == 0 && y == 0 && z == 0)) 
                                    { //���������˾�����ҹ�Զ�Ľ����ſ飬������Ψһ�ſ鴴�����������ǽ������Ŵ����������ԭ���ſ�����Χ��Ȼ�ᴴ��6�����ţ�͸��״̬��

                                        //���ſ���¶��в�Ϊ��ʱ
                                        while (ChunkUpdateQueue.Count > 0)
                                        {
                                            //�����ſ����
                                            ProcessChunkQueue();
                                            //���֡��ʱ������ʱ�䳬����Ŀ��֡���趨��ʱ��
                                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                            {
                                                //Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                                                yield return new WaitForEndOfFrame();
                                            }
                                        }
                                        //����ָ���������ſ�
                                        CellChunk currentChunk = GetChunkComponent(x, y, z);
                                        //�Ѵ��ڵ���δ����������ſ�Ӧ��ӵ����¶�����
                                        if (currentChunk != null)
                                        {
                                            //���������ɵ�û��������ſ�Ӧ����Ϊ�����ſ飨���������޸ģ�
                                            if (currentChunk.DisableMesh || currentChunk.EnableTimeout)
                                            {
                                                //��������=��
                                                currentChunk.DisableMesh = false;
                                                //����ʱ=��
                                                currentChunk.EnableTimeout = false;
                                                //��������״̬=��
                                                currentChunk.Fresh = true;
                                            }
                                            //��������״̬Ϊ��ʱ
                                            if (currentChunk.Fresh)
                                            {
                                                //ˢ�������ſ�
                                                for (int d = 0; d < 6; d++)
                                                {
                                                    CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                                    GameObject neighborChunk = GetChunk(neighborIndex);
                                                    if (neighborChunk == null)
                                                    {
                                                        neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                                    }
                                                    //������������ſ鵽NeighborChunks�Է�����û������
                                                    currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();
                                                    //���֡��ʱ������ʱ�䳬����Ŀ��֡���趨��ʱ�䣬Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                                                    if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                                    {
                                                        yield return new WaitForEndOfFrame();
                                                    }
                                                    //����ſ������֪ͨ"��ֹ�ſ鴴������"
                                                    if (StopSpawning)
                                                    {
                                                        //�������ж���
                                                        EndSequence();
                                                        yield break;
                                                    }
                                                }

                                                //��ǰ�ſ鲻����
                                                if (currentChunk != null)
                                                    //���ſ��������֪�ھӵ�����׼������ʱ�����ſ���ӵ����¶���
                                                    currentChunk.AddToQueueWhenReady();
                                            }
                                        }
                                        else
                                        {
                                            //���chunk�����ڣ��򴴽��µ�chunk(����������׼����ʱ�Ὣ�Լ���ӵ����¶�����)
                                            // spawn chunk.�ſ鴴��
                                            GameObject newChunk = Instantiate(ChunkObject, new Vector3(x, y, z), transform.rotation); // Spawn a new chunk.�ſ�ʵ�������������ǴӼ����յ��ſ�Ԥ���崴���ģ�
                                            currentChunk = newChunk.GetComponent<CellChunk>();
                                            // spawn neighbor chunks if they're not spawned yet.�������ſ�û�ڴ���ʱ�������ǣ�ѭ��6�δ���6������ö����������0-5��
                                            for (int d = 0; d < 6; d++)
                                            {
                                                //���������������index���ڵ�������
                                                CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                                //��ȡ�����ſ�
                                                GameObject neighborChunk = GetChunk(neighborIndex);
                                                if (neighborChunk == null)
                                                {
                                                    //�����ſ鲻����������ſ�Ԥ�������ʵ����
                                                    neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                                }
                                                // always add the neighbor to NeighborChunks, in case it's not there already
                                                //������������ſ鵽NeighborChunks�������飬�Է�����û��������
                                                currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();
                                                // continue loop in next frame if the current frame time is exceeded.���������ǰ֡ʱ�䣬Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                                                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                                {
                                                    yield return new WaitForEndOfFrame();
                                                }
                                                //����ſ������֪ͨ"��ֹ�ſ鴴������"
                                                if (StopSpawning)
                                                {
                                                    //�������ж���
                                                    EndSequence();
                                                    yield break;
                                                }
                                            }
                                            //��ǰ�ſ鲻����
                                            if (currentChunk != null)
                                                //���ſ��������֪�ھӵ�����׼������ʱ�����ſ���ӵ����¶���
                                                currentChunk.AddToQueueWhenReady();
                                        }
                                    }
                                }

                                // continue loop in next frame if the current frame time is exceeded.���������ǰ֡ʱ�䣬Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                {
                                    yield return new WaitForEndOfFrame();
                                }
                                //����ſ������֪ͨ"��ֹ�ſ鴴������"
                                if (StopSpawning)
                                {
                                    //�������ж���
                                    EndSequence();
                                    yield break;
                                }
                            }
                        }
                    }
                }
                //Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                yield return new WaitForEndOfFrame();
                //�������ж���
                EndSequence();
            }

        }
        /// <summary>
        /// [Э��]����ȱʧ���ſ飨���Ƚ������Զ���ſ���ӵ��ݻٶ��У�
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int range)
        {
            //��ȡ�ſ鴴���߶ȷ�Χ��ֵ
            int heightRange = CPEngine.HeightRange;
            // clear update queue - it will be repopulated again in the correct order in the following loop
            //����ſ���¶��� - �����������ѭ��������ȷ��˳���������
            ChunkUpdateQueue = new List<CellChunk>();
            // flag chunks not in range for removal.��ǹ����ſ鲻���Ƴ���Χ��
            ChunksToDestroy = new List<CellChunk>();
            //�����ſ�����ÿ���洢���ſ�ʵ��
            foreach (CellChunk chunk in Chunks.Values)
            {
                //�ſ������㣨����λ�ã����ɫ������λ���糬���ſ鴴������+ChunkDespawnDistance��Ĭ��8+3=11��
                if (Vector2.Distance(new Vector2(chunk.ChunkIndex.x, chunk.ChunkIndex.y), new Vector2(originX, originY)) > range + CPEngine.ChunkDespawnDistance)
                {
                    //����ЩXYƽ������Զ���ſ���ӵ��ݻٶ���
                    ChunksToDestroy.Add(chunk);
                }
                //����ſ������㣨����λ�ã��ĸ߶ȳ����ſ鴴������+ChunkDespawnDistance��Ĭ��8+3=11��
                else if (Mathf.Abs(chunk.ChunkIndex.z) > range + CPEngine.ChunkDespawnDistance)
                { // destroy chunks outside of vertical range.�ݻٴ�ֱ�����Զ���ſ�
                    ChunksToDestroy.Add(chunk);
                }
            }

            //��ѭ����ʼ����ȱʧ���ſ�
            for (int currentLoop = 0; currentLoop <= range; currentLoop++)
            {
                for (var x = originX - currentLoop; x <= originX + currentLoop; x++)
                { // iterate through all potential chunk indexes within range.������Χ�����п��ܵ��ſ�����
                    for (var y = originY - currentLoop; y <= originY + currentLoop; y++)
                    {

                        //�����ĸ߶Ⱦ���ֵ���ܳ����߶ȷ�Χ������3���ſ�߶ȣ�����ô�������������Ч��
                        if (Mathf.Abs(y) <= heightRange)
                        { // skip chunks outside of height range.���������˸߶ȷ�Χ֮����ſ�
                            //�����ſ���XZƽ���Ǹ���2���߳����� < 1.3�������Ի�ﲻ���Ǹ�������ͬʱ��֤�Ƿ�KeepOneChunk�����ٱ�֤ԭ�����1���ſ飩
                            if (Mathf.Abs(originX - x) + Mathf.Abs(originY - y) < range * 1.3f || (CPEngine.KeepOneChunk && x==0 && y == 0))
                            { // skip corners.���������˾�����ҹ�Զ�Ľ����ſ�

                                //���ſ���¶��в�Ϊ��ʱ
                                while (ChunkUpdateQueue.Count > 0)
                                {
                                    //�����ſ����
                                    ProcessChunkQueue();
                                    //���֡��ʱ������ʱ�䳬����Ŀ��֡���趨��ʱ��
                                    if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                    {
                                        //Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                                        yield return new WaitForEndOfFrame();
                                    }
                                }

                                //����ָ���������ſ�
                                CellChunk currentChunk = GetChunkComponent(x, y);


                                // chunks that already exist but haven'transform had their mesh built yet should be added to the update queue.�Ѵ��ڵ���δ����������ſ�Ӧ��ӵ����¶�����
                                if (currentChunk != null)
                                {

                                    // chunks without meshes spawned by server should be changed to regular chunks.���������ɵ�û��������ſ�Ӧ����Ϊ�����ſ飨���������޸ģ�
                                    if (currentChunk.DisableMesh || currentChunk.EnableTimeout)
                                    {
                                        //��������=��
                                        currentChunk.DisableMesh = false;
                                        //����ʱ=��
                                        currentChunk.EnableTimeout = false;
                                        //��������״̬=��
                                        currentChunk.Fresh = true;
                                    }

                                    //��������״̬Ϊ��ʱ
                                    if (currentChunk.Fresh)
                                    {
                                        // spawn neighbor chunks.ˢ�������ſ飨���ģʽֻҪ��������
                                        for (int d = 0; d < 4; d++)
                                        {
                                            CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                            GameObject neighborChunk = GetChunk(neighborIndex);
                                            if (neighborChunk == null)
                                            {
                                                neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                            }

                                            // always add the neighbor to NeighborChunks, in case it's not there already.������������ſ鵽NeighborChunks���Է�����û������
                                            currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();

                                            // continue loop in next frame if the current frame time is exceeded.���֡��ʱ������ʱ�䳬����Ŀ��֡���趨��ʱ�䣬Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                            {
                                                yield return new WaitForEndOfFrame();
                                            }
                                            //����ſ������֪ͨ"��ֹ�ſ鴴������"
                                            if (StopSpawning)
                                            {
                                                //�������ж���
                                                EndSequence();
                                                yield break;
                                            }
                                        }

                                        //��ǰ�ſ鲻����
                                        if (currentChunk != null)
                                            //���ſ��������֪�ھӵ�����׼������ʱ�����ſ���ӵ����¶���
                                            currentChunk.AddToQueueWhenReady();
                                    }
                                }
                                else
                                {
                                    // if chunk doesn'transform exist, create new chunk (it adds itself to the update queue when its data is ready)
                                    //���chunk�����ڣ��򴴽��µ�chunk(����������׼����ʱ�Ὣ�Լ���ӵ����¶�����)

                                    // spawn chunk.�ſ鴴��
                                    GameObject newChunk = Instantiate(ChunkObject, new Vector3(x, y, 0), transform.rotation); // Spawn a new chunk.�ſ�ʵ�������������ǴӼ����յ��ſ�Ԥ���崴���ģ�
                                    currentChunk = newChunk.GetComponent<CellChunk>();

                                    // spawn neighbor chunks if they're not spawned yet.�������ſ�û�ڴ���ʱ�������ǣ�ѭ��4�δ���4��������������ö����������0-3��
                                    for (int d = 0; d < 4; d++)
                                    {
                                        //���������������index���ڵ�������
                                        CPIndex neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                        //��ȡ�����ſ�
                                        GameObject neighborChunk = GetChunk(neighborIndex);
                                        if (neighborChunk == null)
                                        {
                                            //�����ſ鲻����������ſ�Ԥ�������ʵ����
                                            neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                        }

                                        // always add the neighbor to NeighborChunks, in case it's not there already
                                        //������������ſ鵽NeighborChunks�������飬�Է�����û��������
                                        currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<CellChunk>();

                                        // continue loop in next frame if the current frame time is exceeded.���������ǰ֡ʱ�䣬Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                                        if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                        {
                                            yield return new WaitForEndOfFrame();
                                        }
                                        //����ſ������֪ͨ"��ֹ�ſ鴴������"
                                        if (StopSpawning)
                                        {
                                            //�������ж���
                                            EndSequence();
                                            yield break;
                                        }
                                    }

                                    //��ǰ�ſ鲻����
                                    if (currentChunk != null)
                                        //���ſ��������֪�ھӵ�����׼������ʱ�����ſ���ӵ����¶���
                                        currentChunk.AddToQueueWhenReady();
                                }

                            }
                        }



                        // continue loop in next frame if the current frame time is exceeded.���������ǰ֡ʱ�䣬Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
                        if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        //����ſ������֪ͨ"��ֹ�ſ鴴������"
                        if (StopSpawning)
                        {
                            //�������ж���
                            EndSequence();
                            yield break;
                        }

                    }
                }
            }

            //Э����ͣ�õ�ǰ֡������Ⱦֱ���´μ���ʣ�ද��
            yield return new WaitForEndOfFrame();
            //�������ж���
            EndSequence();
        }

        /// <summary>
        /// �������У����嶯���������ſ鶯��=�٣�ж��δ�����õ���Դ����ֹ����״̬=�٣������ſ�ݻ��б�Ϊÿ���ſ������Ƴ����
        /// </summary>
        private void EndSequence()
        {
            //�����ſ鶯��=��
            SpawningChunks = false;
            //ж��δ�����õ���Դ
            Resources.UnloadUnusedAssets();
            //����ʼ�������ſ�
            Done = true;
            //��ֹ����״̬=��
            StopSpawning = false;
            //�����ſ�ݻ��б�
            foreach (CellChunk chunk in ChunksToDestroy)
            {
                //Ϊÿ���ſ������Ƴ����
                chunk.FlagToRemove();
            }
        }
    }
}

// ��ѭ����δ������ڴ�����Ϸ�����С��ſ顱��Chunks�������ɺ�����
// ��һ�����͵�����ʵ�����޻������Ϸ����ġ��ֿ���ء���chunk-based loading������������
// ͨ����̬���ɺ������ſ飬��Ϸ�����ڱ������ܵ�ͬʱ��������һ��ɫ��λ�ã���̬�����ɺ������ſ飬���Ż����ܺ��ڴ�ʹ�á�
// �����ǹ�����⣺
// �������壺SpawnMissingChunks���������ĸ�������originX��originY��originZ�����Ǵ����ɫ������λ�ã���range���ſ����ɺ����ٵķ�Χ����
// ��ʼ����
// ��ȡ��Ϸ���涨����ſ����ɸ߶ȷ�ΧheightRange��
// ����ſ���¶���ChunkUpdateQueue�ʹ������ſ��б�ChunksToDestroy��
// ��Ǵ������ſ飺
// ������ǰ���ڵ������ſ顣
// ���ĳ���ſ����ɫ�ľ��볬��range + CPEngine.ChunkDespawnDistance��Ĭ��Ϊ11����������Ϊ�����١�
// ����ſ��ڴ�ֱ�����ϳ�����Χ��Ҳ������Ϊ�����١�
// �������ſ飺
// ͨ������Ƕ��ѭ���������Խ�ɫΪ���ġ�rangeΪ�뾶�������巶Χ�ڵ�����Ǳ���ſ�������
// ���ÿ��Ǳ���ſ��Y�����Ƿ�������ĸ߶ȷ�Χ�ڡ�
// ���������ɫ��Զ������range * 1.3f���Ľ����ſ顣
// ������¶��в�Ϊ�գ�����ͣѭ������������е��ſ顣
// ʹ��GetChunkComponent(pixelX, pixelY, z)���������Ի�ȡ������ָ�����괦���ſ顣���ſ��Ѿ�������������ԣ���DisableMesh��EnableTimeout���������Ƿ�������������

// �߼��ܽ᣺
// ���������Ѵ��ڵ��ſ飬�����Щ�ſ����ָ����ԭ�㣨originX, originY, originZ��������ָ���ķ�Χ��range + CPEngine.ChunkDespawnDistance��������Щ�ſ����������б� ChunksToDestroy��
// ����ָ����Χ�ڵ��ſ飬����һ�������������µ��ſ��������е��ſ顣
// ����ÿ���ſ飬�����߶��Ƿ���ָ���ķ�Χ�ڣ�����ǣ������һ�������������µ��ſ��������е��ſ顣
// �����ɻ�����ſ�Ĺ����У������һ������������һЩ�������紦���ſ���С�ʵ�����ڽ��ſ顢����ſ鵽���еȡ�
// �����Ƿ���Ҫ��ÿ֡����ǰ��ͣ���ɣ��Կ������ɵ����ʡ�������ɹ�������Ҫֹͣ���ɣ��������������С�