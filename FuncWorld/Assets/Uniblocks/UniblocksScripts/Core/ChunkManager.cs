using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// Controls spawning and destroying chunks

namespace Uniblocks
{
    /// <summary>
    /// �ſ�������������ſ�Ĵ����ʹݻ١�����÷���Unity������½�һ���ն���Engine�����ѽű��ϵ����λ�ü����أ�UnityҪ��һ��cs�ļ�ֻ��һ���࣬�����������ļ���һ�£�
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {

        /// <summary>
        /// �ſ�Ԥ���壨Chunk Prefab��
        /// </summary>
        public GameObject ChunkObject;

        // chunk lists

        /// <summary>
        /// �ſ��飨�ֵ䣩���洢����������Chunkʵ�������ã��ſ�ͨ��Awake����ChunkManager.RegisterChunk���Լ���ӵ��ֵ��У��ַ���key��ӦChunk����������ѭ��x,y,z���ĸ�ʽ��
        /// </summary>
        public static Dictionary<string, Chunk> Chunks;

        /// <summary>
        /// �ſ���¶��У��б�洢���������ȼ�������ſ飬��ProcessChunkQueueѭ���д���
        /// </summary>
        private static List<Chunk> ChunkUpdateQueue; // stores chunks ordered by update priority. Processed in the ProcessChunkQueue loop
        /// <summary>
        /// �ſ������б������SpawnChunks����ʱ���ٵ�chunk��
        /// </summary>
        private static List<Chunk> ChunksToDestroy; // chunks to be destroyed at the end of SpawnChunks

        /// <summary>
        /// ��ǰ֡���ſ��ѱ������������ڸ�ÿ֡���ſ鱣������MaxChunkSaves���бȶԣ�
        /// </summary>
        public static int SavesThisFrame;


        // global flags

        /// <summary>
        /// ���ChunkManager��ǰ�������ɿ飬��Ϊtrue
        /// </summary>
        public static bool SpawningChunks; // true if the ChunkManager is currently spawning chunks
        /// <summary>
        /// ��Ϊtrueʱ����ǰ��SpawnChunks���н�����ֹ(Ȼ��������Ϊfalse)
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
        private Index LastRequest;
        /// <summary>
        /// Ŀ����Ϸ���� = 1f / Engine.TargetFPS
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
            targetFrameDuration = 1f / Engine.TargetFPS;
            //�����ſ��飨�ֵ䣩
            Chunks = new Dictionary<string, Chunk>();
            //�����ſ�����б�
            ChunkUpdateQueue = new List<Chunk>();
            //����֡��ʱ���������¼������ʱ��
            frameStopwatch = new Stopwatch();

            //set correct scale of trigger collider and additional mesh collider.���ô�������ײ��͸���������ײ������ȷ����

            //�����ſ����ű���Ϊ�ſ�ʵ����ǰ�����ű���
            Engine.ChunkScale = ChunkObject.transform.localScale;
            //���ø���������ײ������ȷ����
            ChunkObject.GetComponent<Chunk>().MeshContainer.transform.localScale = ChunkObject.transform.localScale;
            //���ô�������ײ�����ȷ����
            ChunkObject.GetComponent<Chunk>().ChunkCollider.transform.localScale = ChunkObject.transform.localScale;

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
        public static void AddChunkToUpdateQueue(Chunk chunk)
        {
            //����ſ���¶��������ѷ���������ſ�
            if (ChunkUpdateQueue.Contains(chunk) == false)
            {
                //�ſ���¶�������Ӷ����ſ�
                ChunkUpdateQueue.Add(chunk);
            }
        }

        /// <summary>
        /// �����ſ���У���SpawnChunksѭ�����������¿�����
        /// </summary>
        private void ProcessChunkQueue()
        { // called from the SpawnChunks loop to update chunk meshes

            // update the first chunk and remove it from the queue.���µ�һ���鲢����Ӷ�����ɾ��
            Chunk currentChunk = ChunkUpdateQueue[0];

            if (!currentChunk.Empty && !currentChunk.DisableMesh)
            {
                //��ǰ�ſ鲻Ϊ����û�б���������ʱ�����½�������
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
        public static void RegisterChunk(Chunk chunk)
        { // adds a reference to the chunk to the global chunk list
            Chunks.Add(chunk.ChunkIndex.ToString(), chunk);
        }

        /// <summary>
        /// ע���ſ飨��ȫ���ſ��ֵ��У����������ſ鱻����ʱ�Զ���ɵġ�
        /// </summary>
        /// <param name="chunk"></param>
        public static void UnregisterChunk(Chunk chunk)
        {
            Chunks.Remove(chunk.ChunkIndex.ToString());
        }

        /// <summary>
        /// ����ָ��Index(x, y, z)���ſ����Ϸ��������������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject GetChunk(int x, int y, int z)
        { // returns the gameObject of the chunk with the specified x,y,z, or returns null if the object is not instantiated

            return GetChunk(new Index(x, y, z));
        }

        /// <summary>
        /// ����ָ��Index���ſ����Ϸ��������������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static GameObject GetChunk(Index index)
        {

            Chunk chunk = GetChunkComponent(index);
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
        /// ����ָ��Index(x, y, z)���ſ飬�������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Chunk GetChunkComponent(int x, int y, int z)
        {

            return GetChunkComponent(new Index(x, y, z));
        }

        /// <summary>
        /// ����ָ��Index���ſ飬�������û��ʵ�����򷵻�null��ע�⣺����ܻ��е�����������һ֡����̫��Ρ�
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Chunk GetChunkComponent(Index index)
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
        /// ���ɴ�������(x,y,z)�ĵ����ſ鲢���ظ���Ϸ�������������򷵻������ɵ���Ϸ����
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static GameObject SpawnChunk(int x, int y, int z)
        { // spawns a single chunk (only if it's not already spawned)

            GameObject chunk = GetChunk(x, y, z);
            if (chunk == null)
            {
                return Engine.ChunkManagerInstance.DoSpawnChunk(new Index(x, y, z));
            }
            else return chunk;
        }

        /// <summary>
        /// ���ɴ��������ĵ����ſ鲢���ظ���Ϸ�������������򷵻������ɵ���Ϸ����
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns></returns>
        public static GameObject SpawnChunk(Index index)
        { // spawns a single chunk (only if it's not already spawned)

            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                return Engine.ChunkManagerInstance.DoSpawnChunk(index);
            }
            else return chunk;
        }

        /// <summary>
        /// �������������������ɵ����ſ����Ϸ���󲢷��أ������㽫��Ϊ�ſ��������λ�ã���ǰ���Ǹ�����λ����δʵ�����ſ���󡣱�����������������ɲ����ó�ʱ���ɷ������ڶ�����Ϸ��ʹ�ã��������ſ�ʵ���Ѵ����򲻻�����������ɺ����ó�ʱ�����򵥵ط��ظö���
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns>�����ſ����Ϸ�������</returns>
        public static GameObject SpawnChunkFromServer(Index index)
        { // spawns a chunk and disables mesh generation and enables timeout (used by the server in multiplayer)
            GameObject chunk = GetChunk(index);
            if (chunk == null)
            {
                chunk = Engine.ChunkManagerInstance.DoSpawnChunk(index);
                Chunk chunkComponent = chunk.GetComponent<Chunk>();
                chunkComponent.EnableTimeout = true;
                chunkComponent.DisableMesh = true;
                return chunk;
            }
            else return chunk; // don't disable mesh generation and don't enable timeout for chunks that are already spawned
        }

        /// <summary>
        /// ����������������(x,y,z)���ɵ����ſ����Ϸ���󲢷��أ������㽫��Ϊ�ſ��������λ�ã���ǰ���Ǹ�����λ����δʵ�����ſ���󡣱�����������������ɲ����ó�ʱ���ɷ������ڶ�����Ϸ��ʹ�ã��������ſ�ʵ���Ѵ����򲻻�����������ɺ����ó�ʱ�����򵥵ط��ظö���
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns>�����ſ����Ϸ�������</returns>
        public static GameObject SpawnChunkFromServer(int x, int y, int z)
        {
            return SpawnChunkFromServer(new Index(x, y, z));
        }

        /// <summary>
        /// �����ſ���嶯�������ſ�Ԥ�������ſ�����λ�ô����ſ���Ϸ���壬��ȡ�����ϵ��ſ���������ſ����������ӵ������У���󷵻��ſ���Ϸ���塣
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns></returns>
        GameObject DoSpawnChunk(Index index)
        {
            //���ſ�Ԥ�������ſ�����λ�ô����ſ���Ϸ����
            GameObject chunkObject = Instantiate(ChunkObject, index.ToVector3(), transform.rotation);
            //��ȡ�����ϵ��ſ����
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            //���ſ����������ӵ�������
            AddChunkToUpdateQueue(chunk);
            //�����ſ���Ϸ����
            return chunkObject;
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱȡ����λ��(x,y,z)ת��Ϊ�ſ�������Ȼ�����Ը�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(float x, float y, float z)
        { // take world pos, convert to chunk index.ȡ����λ�ã�ת��Ϊ������
            Index index = Engine.PositionToChunkIndex(new Vector3(x, y, z));
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱȡ����λ��ת��Ϊ�ſ�������Ȼ�����Ը�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="position"></param>
        public static void SpawnChunks(Vector3 position)
        {
            Index index = Engine.PositionToChunkIndex(position);
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����(x,y,z)Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(int x, int y, int z)
        { // take chunk index, no conversion needed
            Engine.ChunkManagerInstance.TrySpawnChunks(x, y, z);
        }
        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="index"></param>
        public static void SpawnChunks(Index index)
        {
            Engine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="index"></param>
        private void TrySpawnChunks(Index index)
        {
            TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// ���ſ������ͬ�⿪ʼ�����ſ飨˵����ǰû�������ſ鶯����ʱ�����Ը����ſ�����(x,y,z)Ϊԭ�㴴���ſ飨���Σ���
        /// ����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ�㣬�������м���Ϊ1��������ϴ���״̬��SpawnChunks���н�����ֹ��������ſ���¶���
        /// ע�⣺����ʹ��������������Vector3��Ϊ������SpawnChunks�������Ѳ�����Ϊ����λ�ò��������ſ�֮ǰ����ת��Ϊ�ſ�������
        /// �����ʹ������int��һ��Index�������򲻻�ִ��ת������ֱ��ʹ���ṩ������λ�á�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void TrySpawnChunks(int x, int y, int z)
        {

            if (Done == true)
            { // if we're not spawning chunks at the moment, just spawn them normally.�����ǰû�������ſ鶯�����Ǿ���������������
                StartSpawnChunks(x, y, z);
            }
            else
            { 
                //if we are spawning chunks already, flag to spawn again once the previous round is finished using the last requested position as origin.
                //����Ѿ��������ſ飬����Ե���һ�ֽ����������ɣ���ʹ����������λ����Ϊԭ��
                LastRequest = new Index(x, y, z);
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
                //��ʼ�����ſ飨���Σ���SpawningChunks=�棬�ſ��������Ϊ��ֹ�����ſ飨Done=false����ֹUpdate�п�ʼ�¸������ſ鶯������תΪ������ǰ����ſ鴴������ֱ����ɣ����������Э��������ȱʧ���ſ�
                StartSpawnChunks(LastRequest.x, LastRequest.y, LastRequest.z); 
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
            int range = Engine.ChunkSpawnDistance;
            //����Э��������ȱʧ���ſ�
            StartCoroutine(SpawnMissingChunks(originX, originY, originZ, range));
        }

        /// <summary>
        /// [Э��]����ȱʧ���ſ�
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int originZ, int range)
        {
            //��ȡ�ſ鴴���߶ȷ�Χ��ֵ
            int heightRange = Engine.HeightRange;
            // clear update queue - it will be repopulated again in the correct order in the following loop
            //����ſ���¶��� - �����������ѭ��������ȷ��˳���������
            ChunkUpdateQueue = new List<Chunk>();
            // flag chunks not in range for removal.��ǹ����ſ鲻���Ƴ���Χ��
            ChunksToDestroy = new List<Chunk>();
            //�����ſ�����ÿ���洢���ſ�ʵ��
            foreach (Chunk chunk in Chunks.Values)
            {
                //�ſ������㣨����λ�ã����ɫ������λ���糬���ſ鴴������+ChunkDespawnDistance��Ĭ��8+3=11��
                if (Vector2.Distance(new Vector2(chunk.ChunkIndex.x, chunk.ChunkIndex.z), new Vector2(originX, originZ)) > range + Engine.ChunkDespawnDistance)
                {
                    ChunksToDestroy.Add(chunk);
                }

                else if (Mathf.Abs(chunk.ChunkIndex.y - originY) > range + Engine.ChunkDespawnDistance)
                { // destroy chunks outside of vertical range.�ݻٴ�ֱ��Χ����ſ�
                    ChunksToDestroy.Add(chunk);
                }
            }


            //��ѭ��
            for (int currentLoop = 0; currentLoop <= range; currentLoop++)
            {
                for (var x = originX - currentLoop; x <= originX + currentLoop; x++)
                { // iterate through all potential chunk indexes within range.������Χ�����п��ܵ��ſ�����
                    for (var y = originY - currentLoop; y <= originY + currentLoop; y++)
                    {
                        for (var z = originZ - currentLoop; z <= originZ + currentLoop; z++)
                        {
                            //����ֵ���ܳ����߶ȷ�Χ
                            if (Mathf.Abs(y) <= heightRange)
                            { // skip chunks outside of height range.�����߶ȷ�Χ֮����ſ�
                                if (Mathf.Abs(originX - x) + Mathf.Abs(originZ - z) < range * 1.3f)
                                { // skip corners.����������ҹ�Զ�Ľ����ſ�

                                    // pause loop while the queue is not empty.�����в�Ϊ��ʱ��ͣѭ��
                                    while (ChunkUpdateQueue.Count > 0)
                                    {
                                        //�����ſ����
                                        ProcessChunkQueue();
                                        if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                        {
                                            yield return new WaitForEndOfFrame();
                                        }
                                    }

                                    Chunk currentChunk = GetChunkComponent(x, y, z);


                                    // chunks that already exist but haven't had their mesh built yet should be added to the update queue.�Ѵ��ڵ���δ����������ſ�Ӧ��ӵ����¶�����
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
                                            // spawn neighbor chunks.ˢ�������ſ�
                                            for (int d = 0; d < 6; d++)
                                            {
                                                Index neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                                GameObject neighborChunk = GetChunk(neighborIndex);
                                                if (neighborChunk == null)
                                                {
                                                    neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation);
                                                }

                                                // always add the neighbor to NeighborChunks, in case it's not there already.������������ſ鵽NeighborChunks���Է�����û������
                                                currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<Chunk>();

                                                // continue loop in next frame if the current frame time is exceeded.���������ǰ֡ʱ�䣬������һ֡����ѭ��
                                                if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                                {
                                                    yield return new WaitForEndOfFrame();
                                                }
                                                if (StopSpawning)
                                                {
                                                    EndSequence();
                                                    yield break;
                                                }
                                            }

                                            if (currentChunk != null)
                                                currentChunk.AddToQueueWhenReady();
                                        }


                                    }

                                    else
                                    {
                                        // if chunk doesn't exist, create new chunk (it adds itself to the update queue when its data is ready)
                                        //���chunk�����ڣ��򴴽��µ�chunk(����������׼����ʱ�������Լ���ӵ����¶�����)

                                        // spawn chunk
                                        GameObject newChunk = Instantiate(ChunkObject, new Vector3(x, y, z), transform.rotation) as GameObject; // Spawn a new chunk.
                                        currentChunk = newChunk.GetComponent<Chunk>();

                                        // spawn neighbor chunks if they're not spawned yet.�������ſ�û�ڴ���ʱ��������
                                        for (int d = 0; d < 6; d++)
                                        {
                                            Index neighborIndex = currentChunk.ChunkIndex.GetAdjacentIndex((Direction)d);
                                            GameObject neighborChunk = GetChunk(neighborIndex);
                                            if (neighborChunk == null)
                                            {
                                                neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation) as GameObject;
                                            }

                                            // always add the neighbor to NeighborChunks, in case it's not there already
                                            //������������ſ鵽NeighborChunks���Է�����û��������
                                            currentChunk.NeighborChunks[d] = neighborChunk.GetComponent<Chunk>();

                                            // continue loop in next frame if the current frame time is exceeded.���������ǰ֡ʱ�䣬������һ֡����ѭ��
                                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                                            {
                                                yield return new WaitForEndOfFrame();
                                            }
                                            if (StopSpawning)
                                            {
                                                EndSequence();
                                                yield break;
                                            }
                                        }

                                        if (currentChunk != null)
                                            currentChunk.AddToQueueWhenReady();



                                    }

                                }
                            }



                            // continue loop in next frame if the current frame time is exceeded.���������ǰ֡ʱ�䣬������һ֡����ѭ��
                            if (frameStopwatch.Elapsed.TotalSeconds >= targetFrameDuration)
                            {
                                yield return new WaitForEndOfFrame();
                            }
                            if (StopSpawning)
                            {
                                EndSequence();
                                yield break;
                            }


                        }
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            EndSequence();
        }

        /// <summary>
        /// ��������
        /// </summary>
        private void EndSequence()
        {
            //�����ſ鶯��=��
            SpawningChunks = false;
            //ж��δ�����õ���Դ
            Resources.UnloadUnusedAssets();
            Done = true;
            StopSpawning = false;

            foreach (Chunk chunk in ChunksToDestroy)
            {
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
// ���ĳ���ſ����ɫ�ľ��볬��range + Engine.ChunkDespawnDistance��Ĭ��Ϊ11����������Ϊ�����١�
// ����ſ��ڴ�ֱ�����ϳ�����Χ��Ҳ������Ϊ�����١�
// �������ſ飺
// ͨ������Ƕ��ѭ���������Խ�ɫΪ���ġ�rangeΪ�뾶�������巶Χ�ڵ�����Ǳ���ſ�������
// ���ÿ��Ǳ���ſ��Y�����Ƿ�������ĸ߶ȷ�Χ�ڡ�
// ���������ɫ��Զ������range * 1.3f���Ľ����ſ顣
// ������¶��в�Ϊ�գ�����ͣѭ������������е��ſ顣
// ʹ��GetChunkComponent(x, y, z)���������Ի�ȡ������ָ�����괦���ſ顣���ſ��Ѿ�������������ԣ���DisableMesh��EnableTimeout���������Ƿ�������������

// �߼��ܽ᣺
// ���������Ѵ��ڵ��ſ飬�����Щ�ſ����ָ����ԭ�㣨originX, originY, originZ��������ָ���ķ�Χ��range + Engine.ChunkDespawnDistance��������Щ�ſ����������б� ChunksToDestroy��
// ����ָ����Χ�ڵ��ſ飬����һ�������������µ��ſ��������е��ſ顣
// ����ÿ���ſ飬�����߶��Ƿ���ָ���ķ�Χ�ڣ�����ǣ������һ�������������µ��ſ��������е��ſ顣
// �����ɻ�����ſ�Ĺ����У������һ������������һЩ�������紦���ſ���С�ʵ�����ڽ��ſ顢����ſ鵽���еȡ�
// �����Ƿ���Ҫ��ÿ֡����ǰ��ͣ���ɣ��Կ������ɵ����ʡ�������ɹ�������Ҫֹͣ���ɣ��������������С�