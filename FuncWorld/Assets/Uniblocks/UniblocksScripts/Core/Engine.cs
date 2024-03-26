using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Uniblocks
{
    #region ö��

    /// <summary>
    /// ���ؿ��6����
    /// </summary>
    public enum Facing
    {
        /// <summary>
        /// ����
        /// </summary>
        up, 
        /// <summary>
        /// ����
        /// </summary>
        down, 
        /// <summary>
        /// ����
        /// </summary>
        right, 
        /// <summary>
        /// ����
        /// </summary>
        left, 
        /// <summary>
        /// ǰ��
        /// </summary>
        forward, 
        /// <summary>
        /// ����
        /// </summary>
        back
    }

    /// <summary>
    /// ����
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// ����
        /// </summary>
        up, 
        /// <summary>
        /// ����
        /// </summary>
        down, 
        /// <summary>
        /// ����
        /// </summary>
        right, 
        /// <summary>
        /// ����
        /// </summary>
        left, 
        /// <summary>
        /// ��ǰ
        /// </summary>
        forward, 
        /// <summary>
        /// ����
        /// </summary>
        back
    }

    /// <summary>
    /// ͸����
    /// </summary>
    public enum Transparency
    {
        /// <summary>
        /// ��̬
        /// </summary>
        solid,
        /// <summary>
        /// ��͸��
        /// </summary>
        semiTransparent, 
        /// <summary>
        /// ͸��
        /// </summary>
        transparent
    }

    /// <summary>
    /// ��ײ����
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// ������
        /// </summary>
        cube, 
        /// <summary>
        /// ����
        /// </summary>
        mesh, 
        /// <summary>
        /// ��
        /// </summary>
        none
    }

    #endregion

    //��������÷���Unity������½�һ���ն��󣬹��ؽű�

    /// <summary>
    /// Uniblocks�������ͣ��������N0.1��
    /// </summary>
    public class Engine : MonoBehaviour
    {
        //˽�л�̬�ֶβ����Զ����л���Inspetor����Ҫ�����л���ʹ��[SerializeField]���ԣ��벻�����л���ʹ��[NonSerialized]����

        #region �ֶΡ����Է���

        /// <summary>
        /// �������ƣ����ڵ���
        /// </summary>
        public static string WorldName;

        /// <summary>
        /// ����·�������ڵ���
        /// </summary>
        public static string WorldPath;

        /// <summary>
        /// ���ؿ�·�������ڵ���
        /// </summary>
        public static string BlocksPath;

        /// <summary>
        /// �������ӣ����ڵ���
        /// </summary>
        public static int WorldSeed;

        /// <summary>
        /// �������ƣ�GUI��������
        /// </summary>
        public string lWorldName = "Default";

        /// <summary>
        /// ���ؿ�·����GUI��������
        /// </summary>
        public string lBlocksPath;

        /// <summary>
        /// ��ά�������ؿ飨���ؿ飩���ڿ�༭���ж���
        /// </summary>
        public static GameObject[] Blocks;
        /// <summary>
        /// ��ά�������ؿ飨���ؿ飩��GUI��������
        /// </summary>
        public GameObject[] lBlocks;

        // �ſ鴴�����ã��ſ������Щ���ؿ�ļ��ϡ���С�����ɵĴ�飩

        /// <summary>
        /// �ſ��Զ�����ʱ�߶ȷ�Χ���ƣ������3����ʾ�߶�Y�����3���ſ鷶Χ������ſ���16�߳�����ôY���߶�48��
        /// </summary>
        public static int HeightRange;
        /// <summary>
        /// �ſ��Զ�����ʱ�������ƣ������8����ʼ���������Χ��֤��8��Χ���ſ飩
        /// </summary>
        public static int ChunkSpawnDistance;
        /// <summary>
        /// �ſ��Զ�����ʱ�ĳߴ�߳��������16�����ſ���16*16*16�����ؿ���ɣ�
        /// </summary>
        public static int ChunkSideLength;
        /// <summary>
        /// �ſ��Զ��ݻ�ʱ���жϾ��루�����3�����ſ���ھ������ChunkSpawnDistance+3���ſ����ʱ���дݻ٣��ݻ�ʱ����뵵���Ա�����߹���ʱ��ȡ���ɣ�
        /// </summary>
        public static int ChunkDespawnDistance;

        // �ſ鴴�����ã�GUI��������

        /// <summary>
        /// �ſ��Զ�����ʱ�߶ȷ�Χ���ƣ������3����ʾ�߶�Y�����3���ſ鷶Χ������ſ���16�߳�����ôY���߶�48��
        /// </summary>
        public int lHeightRange;
        /// <summary>
        /// �ſ��Զ�����ʱ�������ƣ������8����ʼ���������Χ��֤��8��Χ���ſ飩
        /// </summary>
        public int lChunkSpawnDistance;
        /// <summary>
        /// �ſ��Զ�����ʱ�ĳߴ�߳��������16�����ſ���16*16*16�����ؿ���ɣ�
        /// </summary>
        public int lChunkSideLength;
        /// <summary>
        /// �ſ��Զ��ݻ�ʱ���жϾ��루�����3�����ſ���ھ������ChunkSpawnDistance+3���ſ����ʱ���дݻ٣��ݻ�ʱ����뵵���Ա�����߹���ʱ��ȡ���ɣ�
        /// </summary>
        public int lChunkDespawnDistance;

        // ��������

        /// <summary>
        /// ����Ԫ���ʣ�����˵���Ļ��൱��ÿ������������ͼƬ�еı�����Ĭ����0.125˵������ͼƬ����������8x8������Ԫ
        /// </summary>
        public static float TextureUnit;
        /// <summary>
        /// ÿ������Ԫ֮������Ĵ�С����Ȼ������ͼƬ�ı��ʣ���ͼƬ��512x512���أ�Ҫ���������Ԫ�����1��������д1/512��������Ա���ȡ���������Ԫ
        /// </summary>
        public static float TexturePadding;

        // �������ã�GUI��������

        /// ����Ԫ���ʣ�����˵���Ļ��൱��ÿ������������ͼƬ�еı�����Ĭ����0.125˵������ͼƬ����������8x8������Ԫ
        /// </summary>
        public float lTextureUnit;
        /// ÿ������Ԫ֮������Ĵ�С����Ȼ������ͼƬ�ı��ʣ���ͼƬ��512x512���أ�Ҫ���������Ԫ�����1��������д1/512��������Ա���ȡ���������Ԫ
        /// </summary>
        public float lTexturePadding;

        // ƽ̨����

        /// <summary>
        /// Ŀ��֡��
        /// </summary>
        public static int TargetFPS;
        /// <summary>
        /// �ſ鱣������
        /// </summary>
        public static int MaxChunkSaves;
        /// <summary>
        /// �ſ�������������
        /// </summary>
        public static int MaxChunkDataRequests;

        // ƽ̨���ã�GUI��������

        /// <summary>
        /// Ŀ��֡��
        /// </summary>
        public int lTargetFPS;
        /// <summary>
        /// �ſ鱣�����ޣ����ڶ������ߣ�
        /// </summary>
        public int lMaxChunkSaves;
        /// <summary>
        /// �ſ������������ޣ����ڶ������ߣ�
        /// </summary>
        public int lMaxChunkDataRequests;

        // ȫ������

        public static bool ShowBorderFaces;
        /// <summary>
        /// ������ײ��
        /// </summary>
        public static bool GenerateColliders;
        /// <summary>
        /// ���;�ͷע���¼�
        /// </summary>
        public static bool SendCameraLookEvents;
        /// <summary>
        /// �������ָ���¼�
        /// </summary>
        public static bool SendCursorEvents;
        /// <summary>
        /// ����������
        /// </summary>
        public static bool EnableMultiplayer;
        /// <summary>
        /// ����ȷ������ͬ�����λ�õĴ���ʽ��
        /// �ڶ�����Ϸ�У�ͨ����Ҫ�������λ��ͬ���������ͻ��ˡ����λ����ָ��������һ��·�������ƶ�ʱ������λ�á������MultiplayerTrackPosition�ֶ�����Ϊtrue�����ʾ�������λ�ý��������Ͻ���ͬ������ÿ���ͻ��˶������ٸ�����Ĺ��λ�á�
        /// �����MultiplayerTrackPosition�ֶ�����Ϊfalse�����ʾ�������λ�ò����������Ͻ���ͬ�������ͻ��˽������������λ�á�
        /// �ھ��д����ƶ�����Ķ�����Ϸ�У�ʹ��MultiplayerTrackPosition�ֶο��Լ�������ͨ������������ܡ����磬���ĳ�������ڳ����о�ֹ����������MultiplayerTrackPosition�ֶ�����Ϊfalse���Ա��ⲻ��Ҫ������ͬ����
        /// </summary>
        public static bool MultiplayerTrackPosition;
        //������������
        public static bool SaveVoxelData;
        //��������
        public static bool GenerateMeshes;

        // ȫ�����ã�GUI��������

        /// <summary>
        /// ȫ�����ã�GUI��������
        /// </summary>
        public bool lShowBorderFaces;
        /// <summary>
        /// ������ײ��
        /// </summary>
        public bool lGenerateColliders;
        /// <summary>
        /// ���;�ͷע���¼�
        /// </summary>
        public bool lSendCameraLookEvents;
        /// <summary>
        /// �������ָ���¼�
        /// </summary>
        public bool lSendCursorEvents;
        /// <summary>
        /// ����������
        /// </summary>
        public bool lEnableMultiplayer;
        /// <summary>
        /// ����ȷ������ͬ�����λ�õĴ���ʽ��
        /// �ڶ�����Ϸ�У�ͨ����Ҫ�������λ��ͬ���������ͻ��ˡ����λ����ָ��������һ��·�������ƶ�ʱ������λ�á������MultiplayerTrackPosition�ֶ�����Ϊtrue�����ʾ�������λ�ý��������Ͻ���ͬ������ÿ���ͻ��˶������ٸ�����Ĺ��λ�á�
        /// �����MultiplayerTrackPosition�ֶ�����Ϊfalse�����ʾ�������λ�ò����������Ͻ���ͬ�������ͻ��˽������������λ�á�
        /// �ھ��д����ƶ�����Ķ�����Ϸ�У�ʹ��MultiplayerTrackPosition�ֶο��Լ�������ͨ������������ܡ����磬���ĳ�������ڳ����о�ֹ����������MultiplayerTrackPosition�ֶ�����Ϊfalse���Ա��ⲻ��Ҫ������ͬ����
        /// </summary>
        public bool lMultiplayerTrackPosition;
        /// <summary>
        /// ������������
        /// </summary>
        public bool lSaveVoxelData;
        /// <summary>
        /// ��������
        /// </summary>
        public bool lGenerateMeshes;

        /// <summary>
        /// �ſ鳬ʱ
        /// </summary>
        public static float ChunkTimeout;
        /// <summary>
        /// �ſ鳬ʱ��GUI��������
        /// </summary>
        public float lChunkTimeout;
        /// <summary>
        /// �����ſ鳬ʱ
        /// </summary>
        public static bool EnableChunkTimeout;

        //����

        /// <summary>
        /// �����α߳������ڶ���������ͼ��С���������ڽӿ������������ſ�߳���ƽ�������ſ�߳���16����ô���ͼ�ı߳���256
        /// </summary>
        public static int SquaredSideLength;
        /// <summary>
        /// ��������ͨ�ź�ͬ������Ϸ�������
        /// </summary>
        public static GameObject UniblocksNetwork;
        /// <summary>
        /// Uniblocks��������ʵ��
        /// </summary>
        public static Engine EngineInstance;
        /// <summary>
        /// �ſ������ʵ��
        /// </summary>
        public static ChunkManager ChunkManagerInstance;
        /// <summary>
        /// �ſ����ű���
        /// </summary>
        public static Vector3 ChunkScale;
        /// <summary>
        /// Uniblocks�����ʼ��״̬
        /// </summary>
        public static bool Initialized;

        #endregion

        // ==== initialization ====

        public void Awake()
        {
            EngineInstance = this; //this�ؼ��������˵�ǰ���һ��ʵ���������������ھ�̬�ֶεĳ�ʼ���У�����д����
            //��ȡ�����ϵ��ſ���������ʵ��������ָ��Ϊ"ChunkManager"�Ľű��������ʵ������Ķ���
            ChunkManagerInstance = GetComponent<ChunkManager>();
            //��ȡGUI���������������������
            WorldName = lWorldName;

            UpdateWorldPath();

            #region ��ʼ���ӿ����ݣ������ø�ֵ��ʵ���������ֶ�����

            BlocksPath = lBlocksPath;
            Blocks = lBlocks;

            TargetFPS = lTargetFPS;
            MaxChunkSaves = lMaxChunkSaves;
            MaxChunkDataRequests = lMaxChunkDataRequests;

            TextureUnit = lTextureUnit;
            TexturePadding = lTexturePadding;
            GenerateColliders = lGenerateColliders;
            ShowBorderFaces = lShowBorderFaces;
            EnableMultiplayer = lEnableMultiplayer;
            MultiplayerTrackPosition = lMultiplayerTrackPosition;
            SaveVoxelData = lSaveVoxelData;
            GenerateMeshes = lGenerateMeshes;

            ChunkSpawnDistance = lChunkSpawnDistance;
            HeightRange = lHeightRange;
            ChunkDespawnDistance = lChunkDespawnDistance;

            SendCameraLookEvents = lSendCameraLookEvents;
            SendCursorEvents = lSendCursorEvents;

            ChunkSideLength = lChunkSideLength;
            SquaredSideLength = lChunkSideLength * lChunkSideLength;

            #endregion

            //�Ѽ��ص������飨�ֵ�<string, string[]>��
            ChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
            //��ʱ�ſ������飨�ֵ�<string, string>��
            ChunkDataFiles.TempChunkData = new Dictionary<string, string>();

            //��GUI��������lChunkTimeout<= 0.00001���������ſ鴦��ʱ����������ʱ�ҽ�GUI�������������ʱ��ֵ��ֵ�������ֶ�
            if (lChunkTimeout <= 0.00001f)
            {
                EnableChunkTimeout = false;
            }
            else
            {
                EnableChunkTimeout = true;
                ChunkTimeout = lChunkTimeout;
            }

#if UNITY_WEBPLAYER
            //��ǰƽ̨��WebPlayer�����ػ��洢Ӧȡ��
            lSaveVoxelData = false;
            SaveVoxelData = false;
#else
            //��ǰƽ̨����WebPlayer
#endif

            //���ֲ�����

            //���26������Ϊ�����������
            if (LayerMask.LayerToName(26) != "" && LayerMask.LayerToName(26) != "UniblocksNoCollide")
            {
                Debug.LogWarning("Uniblocks: Layer 26 is reserved for Uniblocks, it is automatically set to ignore collision with all layers." +
                                 "��26����ΪUniblocks�����ģ������Զ�����Ϊ����������ͼ�����ײ��");
            }
            for (int i = 0; i < 31; i++)
            {
                //Unity��32�����õĲ�0~31���˴����õ�i~26֮��Ķ��󲻷�����ײ�����ܴ����˴˶�����ײ�¼���
                Physics.IgnoreLayerCollision(i, 26);
            }

            #region ����ſ�

            //���GUI����������Ԥ��������ؿ��������
            if (Blocks.Length < 1)
            {
                Debug.LogError("Uniblocks: The blocks array is empty! Use the Block Editor to update the blocks array." +
                    "���ؿ��ǿյģ�ʹ�ÿ�༭�������£�");
                Debug.Break();
            }

            //����һ�����ؿ飨�տ飩�Ƿ���ڣ��粻���ڻ�û����������򱨴�
            if (Blocks[0] == null)
            {
                Debug.LogError("Uniblocks: Cannot find the empty block prefab (id 0)!" +
                    "�Ҳ����տ�Ԥ���壨id 0����");
                Debug.Break();
            }
            else if (Blocks[0].GetComponent<Voxel>() == null)
            {
                Debug.LogError("Uniblocks: Empty block prefab (id 0) does not have the Voxel component attached!" +
                    "�տ�Ԥ����(id 0)û���������");
                Debug.Break();
            }

            #endregion

            #region �������

            //����ſ�߳�������Ϊ1����Ч��
            if (ChunkSideLength < 1)
            {
                Debug.LogError("Uniblocks: Chunk side length must be greater than 0!" +
                    "�ſ�߳��������0");
                //��ͣ�༭������
                Debug.Break();
            }

            //����ſ����ɾ���<1����Ϊ0���������ɣ���Ĭ����8
            if (ChunkSpawnDistance < 1)
            {
                ChunkSpawnDistance = 0;
                Debug.LogWarning("Uniblocks: Chunk spawn distance is 0. No chunks will spawn!" +
                    "�ſ����ɾ���Ϊ0���������ɿ�");
            }

            //����߶ȷ�ΧС��0����߶ȷ�Χ������Ϊ0��Ĭ����3
            if (HeightRange < 0)
            {
                HeightRange = 0;
                Debug.LogWarning("Uniblocks: Chunk height range can't be a negative number! Setting chunk height range to 0." +
                    "�ſ�߶ȷ�Χ������һ���������ѱ�����Ϊ0");
            }

            //����ſ�������������
            if (MaxChunkDataRequests < 0)
            {
                MaxChunkDataRequests = 0;
                Debug.LogWarning("Uniblocks: Max chunk data requests can't be a negative number! Setting max chunk data requests to 0." +
                    "�ſ������������޲����Ǹ������ѱ�����Ϊ0");
            }

            #endregion

            //������
            GameObject chunkPrefab = GetComponent<ChunkManager>().ChunkObject; //��ȡ�ſ�������й������ſ����������Ϊ�ſ�Ԥ����
            int materialCount = chunkPrefab.GetComponent<Renderer>().sharedMaterials.Length - 1; //�����⣩���ʼ���=�ſ�Ԥ������Ⱦ����Ĺ����������-1

            //����������Ԥ��������ؿ�
            for (ushort i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i] != null)
                {
                    //��ȡ����
                    Voxel voxel = Blocks[i].GetComponent<Voxel>();

                    //�����������������<0�򱨴�
                    if (voxel.VSubmeshIndex < 0)
                    {
                        Debug.LogError("Uniblocks: Voxel " + i + " has a material index lower than 0! Material index must be 0 or greater." +
                            "���صĲ�������С��0��������ڵ���0");
                        Debug.Break();
                    }

                    //�������������������ڣ����⣩���ʼ����򱨴�ʹ���Զ���Ķ������������û�����ϲ��ʣ�
                    if (voxel.VSubmeshIndex > materialCount)
                    {
                        //����ʹ����GUI�����������Զ���������������ſ�Ԥ����ֻ�У����⣩���ʼ���+1�����ʸ��ţ�����һ�����͵Ĳ����������Ÿ�����ʵ��ſ�Ԥ���壡
                        Debug.LogError("Uniblocks: Voxel " + i + " uses material index " + voxel.VSubmeshIndex + ", but the chunk prefab only has " + (materialCount + 1) + " material(s) attached. Set a lower material index or attach more materials to the chunk prefab.");
                        Debug.Break();
                    }
                }
            }

            //�������ã���鿹��ݹ��ܣ��رպ�ɷ�ֹ��Ե������Ӿ���϶��ӦĬ�Ϲر�
            if (QualitySettings.antiAliasing > 0)
            {
                Debug.LogWarning("Uniblocks: Anti-aliasing is enabled. This may cause seam lines to appear between blocks. If you see lines between blocks, try disabling anti-aliasing, switching to deferred rendering path, or adding some texture padding in the engine settings." +
                    "�����˿���ݣ�����ܵ����ڿ�֮����ֽӷ��ߣ�����㿴����֮������������Ž��ÿ���ݣ��л����ӳ���Ⱦ·�����������������������һЩ������䡣");
            }


            Initialized = true;

        }

        // ==== world data ====

        /// <summary>
        /// ��������·������λ����Ϊ ��Worlds�� �Ĵ浵Ŀ¼����ͬϵͳλ�ò�һ��
        /// </summary>
        private static void UpdateWorldPath()
        {
            //"../"��ͨ�õ��ļ�ϵͳ·����ʾ�������ڱ�ʾ��һ��Ŀ¼
            //���ж�����ζ�Ŵ� Application.dataPath ��ָ���Ŀ¼��ʼ����������һ��Ŀ¼���� Application.dataPath �ĸ�Ŀ¼�����ٶ�λ����Ϊ ��Worlds�� ��Ŀ¼
            WorldPath = Application.dataPath + "/../Worlds/" + WorldName + "/"; // you can set World Path here
                                                                                //WorldPath = "/mnt/sdcard/UniblocksWorlds/" + Engine.WorldName + "/"; // example mobile path for Android
        }

        /// <summary>
        /// �����������֣����ú��������ӽ�������Ϊ0������ˢ�����ڵ���������·����
        /// </summary>
        /// <param name="worldName"></param>
        public static void SetWorldName(string worldName)
        {
            WorldName = worldName;
            WorldSeed = 0;
            UpdateWorldPath();
        }

        /// <summary>
        /// ��ȡ�������ӣ����������ļ��ж�ȡ�����򴴽�һ�������Ӳ����䱣�浽�ļ��У�
        /// </summary>
        public static void GetSeed()
        { // reads the world seed from file if it exists, else creates a new seed and saves it to file

            //if (Application.isWebPlayer) { // don't save to file if webplayer			
            //	Engine.WorldSeed = Random.Range (ushort.MinValue, ushort.MaxValue);
            //	return;
            //}		

            if (File.Exists(WorldPath + "seed"))
            {
                //�����������ȡ
                StreamReader reader = new StreamReader(WorldPath + "seed");
                WorldSeed = int.Parse(reader.ReadToEnd());
                reader.Close();
            }
            else
            {
                //ѭ����Ŀ����ȷ�����ɵ� WorldSeed ֵ��Ϊ 0
                while (WorldSeed == 0)
                {
                    WorldSeed = Random.Range(ushort.MinValue, ushort.MaxValue);
                }
                Directory.CreateDirectory(WorldPath); //���ļ��д����򲻻ᴴ���µģ��ö��������׳��쳣������if (!Directory.Exists(WorldPath))�ж�
                StreamWriter writer = new StreamWriter(WorldPath + "seed"); //ָ���ļ�·��������һ��д����
                writer.Write(WorldSeed.ToString()); //Ϊ�ļ�д�������ַ���
                //��ִ�� Close ����֮ǰ���� Flush ��������ȷ�����������ڹر��ļ�֮ǰ����ȷ��д�롣
                writer.Flush();
                writer.Close();
                //��Ȼ�ڴ��������£����� Close ����ʱ���Զ����� Flush ����������ĳЩ��������£����統�ļ�ϵͳ��æ���߳�����������ʱ�����ݿ����޷���ȷ��д���ļ�
                //�������������ʽ���� Flush ������ȷ�������ڹر��ļ�֮ǰ����ȷ��д�룬ȷ���ڳ����쳣��������ݲ��ᶪʧ����ر��ļ�֮ǰ�������������쳣��
            }
        }

        /// <summary>
        /// ������֡������
        /// </summary>
        public static void SaveWorld()
        { // saves the data over multiple frames

            //ʵ�����ü̳��Ը���ķ������첽����浵��ʹ����Unity��Э�̣�
            EngineInstance.StartCoroutine(ChunkDataFiles.SaveAllChunks());
        }

        /// <summary>
        /// ��TempChunkData�е�����д�������ļ�
        /// </summary>
        public static void SaveWorldInstant()
        { // writes data from TempChunkData into region files

            ChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====	

        /// <summary>
        /// ��ȡ���ص���Ϸ�������
        /// </summary>
        /// <param name="voxelId">����ID</param>
        /// <returns></returns>
        public static GameObject GetVoxelGameObject(ushort voxelId)
        {
            try
            {
                if (voxelId == ushort.MaxValue) voxelId = 0;
                GameObject voxelObject = Blocks[voxelId];
                if (voxelObject.GetComponent<Voxel>() == null)
                {
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!" +
                        "��Ϸ��������������������ڣ����ؿտ飡");
                    return Blocks[0];
                }
                else
                {
                    return voxelObject;
                }

            }
            catch (System.Exception)
            {
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return Blocks[0];
            }
        }

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <param name="voxelId">����ID</param>
        /// <returns></returns>
        public static Voxel GetVoxelType(ushort voxelId)
        {
            try
            {
                if (voxelId == ushort.MaxValue) voxelId = 0;
                Voxel voxel = Blocks[(int)voxelId].GetComponent<Voxel>();
                if (voxel == null)
                {
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!");
                    return null;
                }
                else
                {
                    return voxel;
                }

            }
            catch (System.Exception)
            {
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return null;
            }
        }

        /// <summary>
        /// һ������Ͷ�䣬�����ش�������������Ϣ���洢���ص����������������ſ鼰�����ſ��λ����Ϣ��
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static VoxelInfo VoxelRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        { // a raycast which returns the index of the hit voxel and the gameobject of the hit chunk

            RaycastHit hit = new RaycastHit(); //��������Ͷ����hit

            //������������Ͷ����ߣ�hit�Ļ��ƴ�origin������direction����������range
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                //�����hit��ײ�����ܻ�ȡ���ſ���ſ���չ���
                if (hit.collider.GetComponent<Chunk>() != null
                    || hit.collider.GetComponent<ChunkExtension>() != null)
                { // check if we're actually hitting a chunk.��������Ƿ���Ļ������ſ�

                    GameObject hitObject = hit.collider.gameObject; //����ײ���л����Ϸ�������

                    if (hitObject.GetComponent<ChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk.������ǻ��е�����״�����������ſ飨�ж���������״����ӵ�д����չ�������ע�������������ſ��С�ģ������ſ��Ӷ������������ؿ飩
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object.�����������滻Ϊʵ�ʵ��ſ��������������������ĸ�������
                    }

                    //ͨ������Ͷ�������ꡢ���߷�����Ӵ����γɵķ��߷�������ȡ��������������ȡ�������أ�
                    Index hitIndex = hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false);

                    //����͸��
                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent voxel is hit.��һ��͸�����ر�����ʱ���ٴ�ͨ������Ͷ�䴩͸͸������
                        ushort hitVoxel = hitObject.GetComponent<Chunk>().GetVoxel(hitIndex.x, hitIndex.y, hitIndex.z); //ͨ�������������ſ����������أ����ţ�
                        //������е��������͵�VTransparency����=͸��
                        if (GetVoxelType(hitVoxel).VTransparency != Transparency.solid)
                        {
                            Vector3 newOrigin = hit.point; //�洢hit����
                            newOrigin.y -= 0.5f; // push the new raycast down a bit.��hit�����ƶ�0.5
                            return VoxelRaycast(newOrigin, Vector3.down, range - hit.distance, true); //����������Ϣ
                        }
                    }

                    return new VoxelInfo(
                                         hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false), // get hit voxel index.��ȡ�������ص�����
                                         hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, true), // get adjacent voxel index.��ȡ�������ص�����
                                         hitObject.GetComponent<Chunk>()); // get chunk.��ȡ�ſ�
                }
            }

            //�������
            return null;
        }

        /// <summary>
        /// һ������Ͷ�䣬�����ش�������������Ϣ���洢���ص����������������ſ鼰�����ſ��λ����Ϣ��
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static VoxelInfo VoxelRaycast(Ray ray, float range, bool ignoreTransparent)
        {
            return VoxelRaycast(ray.origin, ray.direction, range, ignoreTransparent);
        }

        /// <summary>
        /// ��λ��ת�����ſ�����
        /// </summary>
        /// <param name="position">�ſ�λ��</param>
        /// <returns></returns>
        public static Index PositionToChunkIndex(Vector3 position)
        {
            Index chunkIndex = new Index(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            return chunkIndex;
        }

        /// <summary>
        /// ��λ��ת�����ſ�
        /// </summary>
        /// <param name="position">�ſ�λ��</param>
        /// <returns></returns>
        public static GameObject PositionToChunk(Vector3 position)
        {
            Index chunkIndex = new Index(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            return ChunkManager.GetChunk(chunkIndex);

        }

        /// <summary>
        /// ��λ��ת����������Ϣ
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static VoxelInfo PositionToVoxelInfo(Vector3 position)
        {
            GameObject chunkObject = PositionToChunk(position);
            if (chunkObject != null)
            {
                Chunk chunk = chunkObject.GetComponent<Chunk>();
                Index voxelIndex = chunk.PositionToVoxelIndex(position);
                return new VoxelInfo(voxelIndex, chunk);
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// ��������Ϣת����λ��
        /// </summary>
        /// <param name="voxelInfo"></param>
        /// <returns></returns>
        public static Vector3 VoxelInfoToPosition(VoxelInfo voxelInfo)
        {
            return voxelInfo.chunk.GetComponent<Chunk>().VoxelIndexToPosition(voxelInfo.index);
        }




        // ==== mesh creator ====

        //��༭���������ѡCustom Mesh������Ĭ�ϲ��ʣ�ֻ�в���ѡCustom Meshʱ�����Զ�����ʣ�Ҳ���ǿ���ѡ��ͼƬ����ƫ��
        //ͼƬ����ƫ�������֣���һ���ǹ�ѡ���嵥�����������6����������һ��д����ƫ�Ƶ㣻�ڶ����ǲ���ѡ����ô6���������ͬ����ƫ�Ƶ�
        //ʹ������ƫ�Ƶ��ȡԤ��ͼƬ�ϵ��������½Ǿ�����(0, 0)����(0, 1)��ʾ�ұ߾��飬ÿ������Ĵ�С��Uniblocks����������Texture unit����
        //Texture unit��ֵ��0.125���ʾ������ͼƬ�и�Ϊ8*8=64�����ֵľ���

        /// <summary>
        /// ��ȡ����ƫ�Ƶ�
        /// </summary>
        /// <param name="voxel">���أ����ţ�</param>
        /// <param name="facing">����</param>
        /// <returns>��û���������򷵻�Vector2(0, 0)��������û���Զ��嵥�������򷵻ض�������㣬������һ��û�����������ץȡ����������������</returns>
        public static Vector2 GetTextureOffset(ushort voxel, Facing facing)
        {
            //��ȡ���ص�����
            Voxel voxelType = GetVoxelType(voxel);
            //��ȡ�������飨��ά��������С�飩
            Vector2[] textureArray = voxelType.VTexture;

            if (textureArray.Length == 0)
            { // in case there are no textures defined, return a default texture.�Է���һ���û�ж��������򷵻�Ĭ�ϵ�(0, 0)
                Debug.LogWarning("Uniblocks: Block " + voxel.ToString() + " has no defined textures! Using default texture.");
                return new Vector2(0, 0);
            }
            else if (voxelType.VCustomSides == false)
            { // if this voxel isn't using custom side textures, return the Up texture.����������û��ʹ���Զ��嵥������ֱ�ӷ������ؿ鳯�ϵ���������㣨6���湲�ã�
                return textureArray[0];
            }
            //���������ö������ת���ͣ���������ǰ��Ĭ�϶�Ӧ0~5�������������������ֵ����������󳤶�-1�������ж�Ϊû�ж��������Զ���6��ȴ�����������ǣ�
            else if ((int)facing > textureArray.Length - 1)
            { // if we're asking for a texture that's not defined, grab the last defined texture instead.�������������һ��û�ж����������ץȡ����������㣨ʣ�µ�������������ľ��������
                return textureArray[textureArray.Length - 1];
            }
            else
            {
                //��������һ����Ӧ���������������
                return textureArray[(int)facing];
            }
        }


    }

}
