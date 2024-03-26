using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

// Controls spawning and destroying chunks.�����ſ�ĵ����ʹݻ�

namespace Uniblocks
{
    //��������÷���Unity������½�һ���ն��󣬹��ؽű�

    /// <summary>
    /// �ſ���������������N0.2���������ſ�Ĵ����ʹݻ�
    /// </summary>
    public class ChunkManager : MonoBehaviour
    {

        /// <summary>
        /// �ſ�Ԥ���壨Chunk prefab��
        /// </summary>
        public GameObject ChunkObject;

        // chunk lists

        /// <summary>
        /// �ſ��ֵ�
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
        /// ��ǰ֡�ſ��ѱ�������
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

            Chunks = new Dictionary<string, Chunk>();
            ChunkUpdateQueue = new List<Chunk>();
            frameStopwatch = new Stopwatch();

            //set correct scale of trigger collider and additional mesh collider.���ô�������ײ��͸���������ײ������ȷ����

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
        /// ����֡��ʱ����ͳ����ʱ��
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
            if (ChunkUpdateQueue.Contains(chunk) == false)
            {
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
        /// �����ſ����ѭ����Э�̣�����SpawnChunksδ����ʱ��Update����
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
        /// ע���ſ飨�����ſ��������ӵ�ȫ���ſ��ֵ��У���ChunkIndexΪ������ChunkΪֵ��
        /// </summary>
        /// <param name="chunk"></param>
        public static void RegisterChunk(Chunk chunk)
        { // adds a reference to the chunk to the global chunk list
            Chunks.Add(chunk.ChunkIndex.ToString(), chunk);
        }

        /// <summary>
        /// ע���ſ飨��ȫ���ſ��ֵ��У�
        /// </summary>
        /// <param name="chunk"></param>
        public static void UnregisterChunk(Chunk chunk)
        {
            Chunks.Remove(chunk.ChunkIndex.ToString());
        }

        /// <summary>
        /// ����ָ��Index(x, y, z)���ſ����Ϸ��������������û��ʵ�����򷵻�null
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
        /// ����ָ��Index���ſ����Ϸ��������������û��ʵ�����򷵻�null
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
        /// ����ָ��Index(x, y, z)���ſ�
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
        /// ����ָ��Index���ſ�
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
        /// ����ά�ռ�㴴��1���ſ����Ϸ������󣨸õ㽫��Ϊ�ſ������������λ�ã���ǰ���Ǹ��ſ�������δʵ�����ſ����
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
        /// ���ſ�����λ�ô���1���ſ����Ϸ������󣨸õ㽫��Ϊ�ſ������������λ�ã���ǰ���Ǹ��ſ�������δʵ�����ſ����
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
        /// �������������ſ�����λ�ô���1���ſ����Ϸ������󣨸õ㽫��Ϊ�ſ������������λ�ã���ǰ���Ǹ��ſ�������δʵ�����ſ���󣬱�����������������ɲ����ó�ʱ���ɷ������ڶ�����Ϸ��ʹ�ã���������ſ�ʵ���Ѵ����򲻻�����������ɣ�Ҳ��Ϊ�Ѿ����ɵĿ����ó�ʱ
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
        /// ��������������ά�ռ�㴴��1���ſ����Ϸ������󣨸õ㽫��Ϊ�ſ������������λ�ã���ǰ���Ǹ��ſ�������δʵ�����ſ���󣬱�����������������ɲ����ó�ʱ���ɷ������ڶ�����Ϸ��ʹ�ã���������ſ�ʵ���Ѵ����򲻻�����������ɣ�Ҳ��Ϊ�Ѿ����ɵĿ����ó�ʱ
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns>�����ſ����Ϸ�������</returns>
        public static GameObject SpawnChunkFromServer(int x, int y, int z)
        {
            return SpawnChunkFromServer(new Index(x, y, z));
        }

        /// <summary>
        /// �����ſ���嶯��
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        GameObject DoSpawnChunk(Index index)
        {
            GameObject chunkObject = Instantiate(ChunkObject, index.ToVector3(), transform.rotation) as GameObject;
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            AddChunkToUpdateQueue(chunk);
            return chunkObject;
        }

        /// <summary>
        /// �����ſ�
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
        /// �����ſ�
        /// </summary>
        /// <param name="position"></param>
        public static void SpawnChunks(Vector3 position)
        {
            Index index = Engine.PositionToChunkIndex(position);
            Engine.ChunkManagerInstance.TrySpawnChunks(index);
        }

        /// <summary>
        /// �����ſ�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static void SpawnChunks(int x, int y, int z)
        { // take chunk index, no conversion needed
            Engine.ChunkManagerInstance.TrySpawnChunks(x, y, z);
        }
        public static void SpawnChunks(Index index)
        {
            Engine.ChunkManagerInstance.TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// ���Դ����ſ�
        /// </summary>
        /// <param name="index"></param>
        private void TrySpawnChunks(Index index)
        {
            TrySpawnChunks(index.x, index.y, index.z);
        }

        /// <summary>
        /// ���Դ����ſ�
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
                //����Ѿ��������ſ飬����Ե���һ�ֽ������ٴ����ɣ���ʹ����������λ����Ϊԭ��
                LastRequest = new Index(x, y, z);
                SpawnQueue = 1; //�������м���Ϊ1
                StopSpawning = true; //������ϴ���״̬
                ChunkUpdateQueue.Clear(); //����ſ���¶���
            }
        }

        public void Update()
        {
            //�����ſ鴴���������ſ�������������ʱ
            if (SpawnQueue == 1 && Done == true)
            { // if there is a chunk spawn queued up, and if the previous one has finished, run the queued chunk spawn.���ſ鴴�������Ŷ�ʱ����ǰһ������ɣ����ж����е��ſ鴴������
                SpawnQueue = 0;
                StartSpawnChunks(LastRequest.x, LastRequest.y, LastRequest.z);
            }

            // if not currently spawning chunks, process any queued chunks here instead.�����ǰû�������ſ鶯�����������ﴦ���κ��Ŷӵ�
            if (!SpawningChunks && !ProcessChunkQueueLoopActive && ChunkUpdateQueue != null && ChunkUpdateQueue.Count > 0)
            {
                //Э�̲�������
                StartCoroutine(ProcessChunkQueueLoop());
            }

            ResetFrameStopwatch();
        }


        /// <summary>
        /// ��ʼ�����ſ�
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        private void StartSpawnChunks(int originX, int originY, int originZ)
        {
            SpawningChunks = true;
            Done = false;

            int range = Engine.ChunkSpawnDistance;

            //����ȱʧ���ſ�
            StartCoroutine(SpawnMissingChunks(originX, originY, originZ, range));
        }

        /// <summary>
        /// ����ȱʧ���ſ�
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerator SpawnMissingChunks(int originX, int originY, int originZ, int range)
        {
            int heightRange = Engine.HeightRange;

            // clear update queue - it will be repopulated again in the correct order in the following loop
            //������¶��� - �����������ѭ��������ȷ��˳���������
            ChunkUpdateQueue = new List<Chunk>();

            // flag chunks not in range for removal.����ſ鲻���Ƴ���Χ��
            ChunksToDestroy = new List<Chunk>();
            foreach (Chunk chunk in Chunks.Values)
            {
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

                            if (Mathf.Abs(y) <= heightRange)
                            { // skip chunks outside of height range.�����߶ȷ�Χ֮����ſ�
                                if (Mathf.Abs(originX - x) + Mathf.Abs(originZ - z) < range * 1.3f)
                                { // skip corners.��������

                                    // pause loop while the queue is not empty.�����в�Ϊ��ʱ��ͣѭ��
                                    while (ChunkUpdateQueue.Count > 0)
                                    {
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
                                                    neighborChunk = Instantiate(ChunkObject, neighborIndex.ToVector3(), transform.rotation) as GameObject;
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
