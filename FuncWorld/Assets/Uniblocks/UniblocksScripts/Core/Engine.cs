// VS2022��д���ű������Ҳ����������ڣ����������dll�ȷ������ļ���֮���������ָ�������ռ�ľ��巽�����б��
// ��C#����У������ռ䣨Namespace����һ����֯����ķ�ʽ�������԰������Ǳ�����������������֮��ĳ�ͻ�����ṩһ���߼��ϵķ������
// UnityEngine �����ռ�
// ��Ҫ���ã��ṩUnity��Ϸ����ĺ��Ĺ��ܡ�UnityEngine�����ռ�����˴����͹���Unity��Ϸ��������л�����ͽӿڡ�
// �������ݣ���������ռ�������ڳ������������������Ⱦ���������봦�����硢��Ƶ���û����桢�������ű��������ڹ���ȹ��ܵ��ࡣ���磬Transform �����ڱ�ʾ�Ͳ�����Ϸ�����λ�á���ת�����ţ�GameObject ����Unity�����еĻ��������飻MonoBehaviour �������нű�����Ļ��࣬���ṩ����Start��Update���������ڷ�����
// System.Collections.Generic �����ռ�
// ��Ҫ���ã��ṩ��һϵ�з��ͼ����࣬��Щ�����ڴ洢�͹������ݼ��ϣ����б��ֵ䡢���ϡ����еȡ�
// �������ݣ���������ռ��������List<T>�������б���Dictionary<TKey, TValue>����ֵ�Լ��ϣ���HashSet<T>�����ϣ��������ظ�Ԫ�أ���Queue<T>�����У����ࡣ��Щ��Ϊ���ݴ洢�Ͳ����ṩ�˸�Ч�����ķ�ʽ��
// System.IO �����ռ�
// ��Ҫ���ã��ṩ�ļ����������Ļ�������/������ܡ�System.IO�����ռ���������ļ����������������࣬���ļ���д��Ŀ¼��������������ȡ�
// �������ݣ���������ռ��е��������㴴���ļ�����ȡ�ļ����ݡ�д���ļ���ɾ���ļ�������Ŀ¼�ṹ�������������ȡ����磬File ���ṩ�˾�̬���������ļ��Ĵ��������ơ�ɾ�����ƶ��ʹ򿪣�Directory �����ڴ�����ɾ�����ƶ�Ŀ¼��StreamReader �� StreamWriter �����ڴ��ļ��ж�ȡ�ı������ļ���д���ı���
// ��Unity��Ŀ�У�ͨ����ͨ��������Щ�����ռ���ʹ�������ṩ����͹��ܡ����磬�ڽű��ļ��Ŀ�ͷʹ��using UnityEngine;��䣬�Ϳ��������ڽű���ֱ��ʹ��Unity�����ṩ��������͹��ܣ�������ÿ�ζ�д�������������ռ�·����
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
        /// ʵ��
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

    /// <summary>
    /// �洢ȫ���������ã����ṩһЩ��̬������������ת���ȡ�����÷���Unity������½�һ���ն���Engine�����ѽű��ϵ����λ�ü����أ�UnityҪ��һ��cs�ļ�ֻ��һ���࣬�����������ļ���һ�£�
    /// </summary>
    public class Engine : MonoBehaviour
    {
        //˽�л�̬�ֶβ����Զ����л���Inspetor����Ҫ�����л���ʹ��[SerializeField]���ԣ��벻�����л���ʹ��[NonSerialized]����

        #region �ֶΡ����Է���

        // Engine��ÿ����̬��������һ���Ǿ�̬�ĵȼ���Ǿ�̬�����������뾲̬������ͬ��ֻ���ڿ�ͷ��Сд��L��ʹ����Щ������Ϊ���ܹ���Unity�б༭��Щ�������������������ô����С�
        // �ڱ༭����Awake�����У��Ǿ�̬������Ӧ�������ǵľ�̬��Ӧ(ͨ�������е�Engine��Ϸ����)������������ʱ�ı�Ǿ�̬������������κ�Ӱ�졣

        /// <summary>
        /// ��ǰ���������ƣ���Ӧ�洢�������ݵ��ļ��У�
        /// </summary>
        public static string WorldName;

        /// <summary>
        /// ���������ļ���·����Ĭ��·��Ϊ/application_root/world_name/������ͨ��ֱ�ӱ༭Engine�ű��ڵ�UpdateWorldPath˽�к�������������·����
        /// </summary>
        public static string WorldPath;

        /// <summary>
        /// ���ؿ�·����Unity��Ŀ�п�Ԥ�����·���������ǿ�༭���������ҿ�ġ�
        /// </summary>
        public static string BlocksPath;

        /// <summary>
        /// ��ǰ���������ӣ������ڳ���������ɣ����Ӵ洢�����������ļ�����
        /// </summary>
        public static int WorldSeed;

        /// <summary>
        /// [GUI��������]���������ļ���·����Ĭ��·��Ϊ/application_root/world_name/������ͨ��ֱ�ӱ༭Engine�ű��ڵ�UpdateWorldPath˽�к�������������·����
        /// </summary>
        public string lWorldName = "Default";

        /// <summary>
        /// [GUI��������]���ؿ�·����Unity��Ŀ�п�Ԥ�����·���������ǿ�༭���������ҿ�ġ�
        /// </summary>
        public string lBlocksPath;

        /// <summary>
        /// ��ά�������ؿ飨���ؿ飩Ԥ���壬�ڿ�༭���ж��壬����������Ӧ�ڿ������ID�����ؿ�Ԥ�������ࣩ��
        /// </summary>
        public static GameObject[] Blocks;
        /// <summary>
        /// [GUI��������]��ά�������ؿ飨���ؿ飩Ԥ���壬�ڿ�༭���ж��壬����������Ӧ�ڿ������ID�����ؿ�Ԥ�������ࣩ��
        /// </summary>
        public GameObject[] lBlocks;

        // �ſ鴴�����ã��ſ������Щ���ؿ�ļ��ϡ���С�����ɵĴ�飩

        /// <summary>
        /// �����ſ�����������ֱ�����������ſ��Զ�����ʱ�߶ȷ�Χ��ֵ�������3����ʾ�߶�Y�����3���ſ鷶Χ������ſ���16�߳�����ôY���߶�48��
        /// </summary>
        public static int HeightRange;
        /// <summary>
        /// ��ԭ�㵽�����ſ��ˮƽ����(���ſ�Ϊ��λ)�����ſ��Զ�����ʱ�ľ������ƣ������8����ʼ���������Χ��֤��8��Χ���ſ飩
        /// </summary>
        public static int ChunkSpawnDistance;
        /// <summary>
        /// һ���ſ�ı߳�(������Ϊ��λ)�����ſ��Զ�����ʱ�������ε��߳ߴ�߳��������16�����ſ���16*16*16�����ؿ���ɣ�
        /// </summary>
        public static int ChunkSideLength;
        /// <summary>
        /// �ſ��Զ��ݻ�ʱ���жϾ��루�����3�����ſ���ھ������ChunkSpawnDistance+3���ſ����ʱ���дݻ٣��ݻ�ʱ����뵵���Ա�����߹���ʱ��ȡ���ɣ�
        /// </summary>
        public static int ChunkDespawnDistance;

        /// <summary>
        /// ���ֵ��θ߶�
        /// </summary>
        public static bool KeepTerrainHeight;
        /// <summary>
        /// ���θ߶�
        /// </summary>
        private static int _terrainHeight;
        /// <summary>
        /// ���θ߶ȣ��������꣩
        /// </summary>
        public static int TerrainHeight
        {
            get
            {
                return _terrainHeight;
            }

            set
            {
                _terrainHeight = value;
            }
        }

        // �ſ鴴�����ã�GUI��������

        /// <summary>
        /// [GUI��������]�����ſ�����������ֱ�����������ſ��Զ�����ʱ�߶ȷ�Χ��ֵ�������3����ʾ�߶�Y�����3���ſ鷶Χ������ſ���16�߳�����ôY���߶�48��
        /// </summary>
        public int lHeightRange;
        /// <summary>
        /// [GUI��������]��ԭ�㵽�����ſ��ˮƽ����(���ſ�Ϊ��λ)�����ſ��Զ�����ʱ�ľ������ƣ������8����ʼ���������Χ��֤��8��Χ���ſ飩
        /// </summary>
        public int lChunkSpawnDistance;
        /// <summary>
        /// [GUI��������]һ���ſ�ı߳�(������Ϊ��λ)�����ſ��Զ�����ʱ�������ε��߳ߴ�߳��������16�����ſ���16*16*16�����ؿ���ɣ�
        /// </summary>
        public int lChunkSideLength;
        /// <summary>
        /// [GUI��������]�ſ��Զ��ݻ�ʱ���жϾ��루�����3�����ſ���ھ������ChunkSpawnDistance+3���ſ����ʱ���дݻ٣��ݻ�ʱ����뵵���Ա�����߹���ʱ��ȡ���ɣ�
        /// </summary>
        public int lChunkDespawnDistance;

        // ��������

        /// <summary>
        /// ����Ԫ���ʣ�һ���������߳��������߳�֮�ȣ����ڼ������ؿ�����������˵���Ļ��൱��ÿ������������ͼƬ�еı�����Ĭ����0.125˵������ͼƬ����������8x8������Ԫ
        /// </summary>
        public static float TextureUnit;
        /// <summary>
        /// �����������֮�����䣬��Ϊ�������ؿ������С��һС���֡���ÿ������Ԫ֮������Ĵ�С����Ȼ������ͼƬ�ı��ʣ���ͼƬ��512x512���أ�Ҫ���������Ԫ�����1��������д1/512��������Ա���ȡ���������Ԫ
        /// </summary>
        public static float TexturePadding;

        // �������ã�GUI��������

        /// <summary>
        /// [GUI��������]����Ԫ���ʣ�һ���������߳��������߳�֮�ȣ����ڼ������ؿ�����������˵���Ļ��൱��ÿ������������ͼƬ�еı�����Ĭ����0.125˵������ͼƬ����������8x8������Ԫ
        /// </summary>
        public float lTextureUnit;
        /// <summary>
        /// [GUI��������]�����������֮�����䣬��Ϊ�������ؿ������С��һС���֡���ÿ������Ԫ֮������Ĵ�С����Ȼ������ͼƬ�ı��ʣ���ͼƬ��512x512���أ�Ҫ���������Ԫ�����1��������д1/512��������Ա���ȡ���������Ԫ
        /// </summary>
        public float lTexturePadding;

        // ƽ̨����

        /// <summary>
        /// Ŀ�꣨Ԥ�ڣ�֡�ʣ�����ʵ�ʣ������ʱ����¼ÿ֡������ʱ������������Խ�����������һ֡������������ֹ������һ֡������Χ�ſ��������ɣ����������ֵ��������Խ��Խ�ö���Ӧ��������ʵ�ʡ�
        /// </summary>
        public static int TargetFPS;
        /// <summary>
        /// ÿ֡���ſ鱣�����ޣ�����ÿ֡�����ſ����������ʣ������ڸ�ChunkManager�ĵ�ǰ֡���ſ��ѱ�������SavesThisFrame���бȶ�
        /// </summary>
        public static int MaxChunkSaves;
        /// <summary>
        /// �ſ������������ޣ�ÿ���ͻ���һ�ο����ڷ��������Ŷӵ�����ſ�����������(0=������)����ͻ����������ݿ���ٶ�̫�����㷢����ķ������޷���������������ٶȣ���ô����������ơ�
        /// </summary>
        public static int MaxChunkDataRequests;

        // ƽ̨���ã�GUI��������

        /// <summary>
        /// [GUI��������]Ŀ�꣨Ԥ�ڣ�֡�ʣ�����ʵ�ʣ������ʱ����¼ÿ֡������ʱ������������Խ�����������һ֡������������ֹ������һ֡������Χ�ſ��������ɣ����������ֵ��������Խ��Խ�ö���Ӧ��������ʵ�ʡ�
        /// </summary>
        public int lTargetFPS;
        /// <summary>
        /// [GUI��������]�ſ鱣�����ޣ�����ÿ֡�����ſ����������ʣ�
        /// </summary>
        public int lMaxChunkSaves;
        /// <summary>
        /// [GUI��������]�ſ������������ޣ�ÿ���ͻ���һ�ο����ڷ��������Ŷӵ�����ſ�����������(0=������)����ͻ����������ݿ���ٶ�̫�����㷢����ķ������޷���������������ٶȣ���ô����������ơ�
        /// </summary>
        public int lMaxChunkDataRequests;

        // ȫ������

        /// <summary>
        /// ���ؿ�Ĳ���ɼ���ǰ����û�������ǽ������ſ�ʵ����
        /// </summary>
        public static bool ShowBorderFaces;
        /// <summary>
        /// ������ײ�壨Ϊfalse���ſ齫���������κ�Colliders��
        /// </summary>
        public static bool GenerateColliders;
        /// <summary>
        /// ���;�ͷע���¼������Ϊtrue, CameraEventsSender��������¼����͵���������ӳ�����ָ�ŵ����ؿ飩
        /// </summary>
        public static bool SendCameraLookEvents;
        /// <summary>
        /// �������ָ���¼������Ϊtrue, CameraEventsSender����������¼����͵���ǰ�����ָ�ŵ����ؿ飩
        /// </summary>
        public static bool SendCursorEvents;
        /// <summary>
        /// ���������ң��ſ齫�ӷ����������������ݶ����Ǵ�Ӳ�����ɻ���أ�����Voxel.ChangeBlock��Voxel.PlaceBlock��Voxel.DestroyBlock�Ὣ���ر仯���͵��������Ա����·ַ����������ӵ���ң�
        /// </summary>
        public static bool EnableMultiplayer;
        /// <summary>
        /// ����ȷ������ͬ�����λ�õĴ���ʽ�������������ҵ�λ����ȷ���Ƿ���Ҫ�����ظ��ķ��͸�����ң��ͻ������ͨ��ChunkLoader�ű������������һ�����λ�ø��¡�
        /// �ڶ�����Ϸ�У�ͨ����Ҫ�������λ��ͬ���������ͻ��ˡ����λ����ָ��������һ��·�������ƶ�ʱ������λ�á������MultiplayerTrackPosition�ֶ�����Ϊtrue�����ʾ�������λ�ý��������Ͻ���ͬ������ÿ���ͻ��˶������ٸ�����Ĺ��λ�á�
        /// �����MultiplayerTrackPosition�ֶ�����Ϊfalse�����ʾ�������λ�ò����������Ͻ���ͬ�������ͻ��˽������������λ�á�
        /// �ھ��д����ƶ�����Ķ�����Ϸ�У�ʹ��MultiplayerTrackPosition�ֶο��Լ�������ͨ������������ܡ����磬���ĳ�������ڳ����о�ֹ����������MultiplayerTrackPosition�ֶ�����Ϊfalse���Ա��ⲻ��Ҫ������ͬ����
        /// </summary>
        public static bool MultiplayerTrackPosition;
        /// <summary>
        /// �����������ݡ�Ϊfalse���ſ齫������ػ򱣴��������ݣ���֮�������ſ�ʱ���ǻ������µ����ݡ�
        /// </summary>
        public static bool SaveVoxelData;
        /// <summary>
        /// ��������
        /// </summary>
        public static bool GenerateMeshes;

        // ȫ�����ã�GUI��������

        /// <summary>
        /// [GUI��������]���ؿ�Ĳ���ɼ���ǰ����û�������ǽ������ſ�ʵ����
        /// </summary>
        public bool lShowBorderFaces;
        /// <summary>
        /// [GUI��������]������ײ�壨Ϊfalse���ſ齫���������κ�Colliders��
        /// </summary>
        public bool lGenerateColliders;
        /// <summary>
        /// [GUI��������]���;�ͷע���¼������Ϊtrue, CameraEventsSender��������¼����͵���������ӳ�����ָ�ŵ����ؿ飩
        /// </summary>
        public bool lSendCameraLookEvents;
        /// <summary>
        /// [GUI��������]�������ָ���¼������Ϊtrue, CameraEventsSender����������¼����͵���ǰ�����ָ�ŵ����ؿ飩
        /// </summary>
        public bool lSendCursorEvents;
        /// <summary>
        /// [GUI��������]���������ң��ſ齫�ӷ����������������ݶ����Ǵ�Ӳ�����ɻ���أ�����Voxel.ChangeBlock��Voxel.PlaceBlock��Voxel.DestroyBlock�Ὣ���ر仯���͵��������Ա����·ַ����������ӵ���ң�
        /// </summary>
        public bool lEnableMultiplayer;
        /// <summary>
        /// [GUI��������]����ȷ������ͬ�����λ�õĴ���ʽ�������������ҵ�λ����ȷ���Ƿ���Ҫ�����ظ��ķ��͸�����ң��ͻ������ͨ��ChunkLoader�ű������������һ�����λ�ø��¡�
        /// �ڶ�����Ϸ�У�ͨ����Ҫ�������λ��ͬ���������ͻ��ˡ����λ����ָ��������һ��·�������ƶ�ʱ������λ�á������MultiplayerTrackPosition�ֶ�����Ϊtrue�����ʾ�������λ�ý��������Ͻ���ͬ������ÿ���ͻ��˶������ٸ�����Ĺ��λ�á�
        /// �����MultiplayerTrackPosition�ֶ�����Ϊfalse�����ʾ�������λ�ò����������Ͻ���ͬ�������ͻ��˽������������λ�á�
        /// �ھ��д����ƶ�����Ķ�����Ϸ�У�ʹ��MultiplayerTrackPosition�ֶο��Լ�������ͨ������������ܡ����磬���ĳ�������ڳ����о�ֹ����������MultiplayerTrackPosition�ֶ�����Ϊfalse���Ա��ⲻ��Ҫ������ͬ����
        /// </summary>
        public bool lMultiplayerTrackPosition;
        /// <summary>
        /// [GUI��������]�����������ݡ�Ϊfalse���ſ齫������ػ򱣴��������ݣ���֮�������ſ�ʱ���ǻ������µ����ݡ�
        /// </summary>
        public bool lSaveVoxelData;
        /// <summary>
        /// [GUI��������]��������
        /// </summary>
        public bool lGenerateMeshes;

        /// <summary>
        /// �ſ鳬ʱ����һ�ſ�ͨ��ChunkManager.SpawnChunk��������������ChunkTimeout���ʱ����û�б����ʣ�����������(�ڱ��������������ݺ�)��
        /// ���Կͻ��˵���������������ſ��е����ر仯�����ü�ʱ������ֵΪ0�����ô˹��ܡ�
        /// </summary>
        public static float ChunkTimeout;
        /// <summary>
        /// �ſ鳬ʱ����һ�ſ�ͨ��ChunkManager.SpawnChunk��������������ChunkTimeout���ʱ����û�б����ʣ�����������(�ڱ��������������ݺ�)��
        /// ���Կͻ��˵���������������ſ��е����ر仯�����ü�ʱ������ֵΪ0�����ô˹��ܡ�
        /// </summary>
        public float lChunkTimeout;
        /// <summary>
        /// �����ſ鳬ʱ�����Engine.ChunkTimeout>0����������Զ�����Ϊtrue��
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
        /// �ſ�Ԥ����Ĵ�С�����ű�����
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
            //����������Uniblocks�����ʼ��

            EngineInstance = this; //this�ؼ��������˵�ǰ���һ��ʵ���������������ھ�̬�ֶεĳ�ʼ���У�����д����
            //��ȡ�����ϵ��ſ���������ʵ��������ָ��Ϊ"ChunkManager"�Ľű��������ʵ������Ķ���
            ChunkManagerInstance = GetComponent<ChunkManager>();
            //��ȡGUI���������������������
            WorldName = lWorldName;
            //��������浵��·��
            UpdateWorldPath();

            #region ��GUI�����������ݸ�ֵ��ʵ���������ֶ�����

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

            //�����Ѽ��ص������飨�ֵ�<string, string[]>��
            ChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
            //������ʱ�ſ������飨�ֵ�<string, string>��
            ChunkDataFiles.TempChunkData = new Dictionary<string, string>();

            //��GUI���������lChunkTimeout<= 0.00001���������ſ鴦��ʱ����������ʱ����lChunkTimeout��ֵ����Ϸ�߼�Ƶ�������õ������ֶ�
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
                Debug.Break(); //��ͣ�༭������
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

            //Uniblocks�����ʼ��״̬
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
        /// ���û�������ƣ����ú��������ӽ�������Ϊ0����ˢ�����ڵ����洢������·���������ñ�����������ʱ�����������ơ�
        /// </summary>
        /// <param name="worldName"></param>
        public static void SetWorldName(string worldName)
        {
            WorldName = worldName;
            WorldSeed = 0;
            UpdateWorldPath();
        }

        /// <summary>
        /// ���ļ��ж�ȡ��ǰ���������ӣ��������û���ҵ������ļ����������һ���µ����ӣ�������洢��Engine.WorldSeed�����С�
        /// </summary>
        public static void GetSeed()
        { // reads the world seed from file if it exists, else creates a new seed and saves it to file

            //if (Application.isWebPlayer) { // don't save to file if webplayer		
            //	Engine.WorldSeed = Random.CameraLookRange (ushort.MinValue, ushort.MaxValue);
            //	return;
            //}		

#if UNITY_WEBPLAYER
            //��ǰƽ̨��WebPlayer�����ػ��洢Ӧȡ��
            Engine.WorldSeed = Random.Range (ushort.MinValue, ushort.MaxValue
            return;
#else
            //��ǰƽ̨����WebPlayer
#endif
            //���������ļ����ȡ
            if (File.Exists(WorldPath + "seed"))
            {
                //�����ļ��Ķ�ȡ��
                StreamReader reader = new StreamReader(WorldPath + "seed");
                WorldSeed = int.Parse(reader.ReadToEnd()); //��ȡȫ���ַ�����תΪ���֣���Ϊ��������
                reader.Close();
            }
            else
            {
                //ѭ����Ŀ����ȷ�����ɵ� WorldSeed ֵ��Ϊ 0
                while (WorldSeed == 0)
                {
                    //����һ���µ�����
                    WorldSeed = Random.Range(ushort.MinValue, ushort.MaxValue);
                }
                Directory.CreateDirectory(WorldPath); //���ļ��д����򲻻ᴴ���µģ��ö��������׳��쳣������if (!Directory.Exists(WorldPath))�ж�
                StreamWriter writer = new StreamWriter(WorldPath + "seed"); //ָ���ļ�·��������һ��д����
                writer.Write(WorldSeed.ToString()); //Ϊ�ļ�д�������ַ���
                //��ִ�� Close ����֮ǰ���� Flush ��������ȷ�����������ڹر��ļ�֮ǰ����ȷ��д��
                writer.Flush();
                writer.Close();
                //��Ȼ�ڴ��������£����� Close ����ʱ���Զ����� Flush ����������ĳЩ��������£����統�ļ�ϵͳ��æ���߳�����������ʱ�����ݿ����޷���ȷ��д���ļ�
                //�������������ʽ���� Flush ������ȷ�������ڹر��ļ�֮ǰ����ȷ��д�룬ȷ���ڳ����쳣��������ݲ��ᶪʧ����ر��ļ�֮ǰ�������������쳣��
            }
        }

        /// <summary>
        /// �����е�ǰʵ�����ſ�����ݱ��浽���̣���Engine.MaxChunkSaves�п�ָ��ÿ֡�����ſ����������ʡ�
        /// </summary>
        public static void SaveWorld()
        { // saves the data over multiple frames

            //ʵ�����ü̳��Ը���ķ������첽����浵��ʹ����Unity��Э�̣�
            EngineInstance.StartCoroutine(ChunkDataFiles.SaveAllChunks());
        }

        /// <summary>
        /// �����е�ǰʵ�������ſ����ݱ��浽���̴浵����֡����һ��ȫִ�У���ܿ��ܻ�ʹ��Ϸ���Ἰ���ӣ���˲���������Ϸ������ʹ�ô˹��ܡ�
        /// </summary>
        public static void SaveWorldInstant()
        { // writes data from TempChunkData into region files

            ChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====	

        /// <summary>
        /// ��ȡ����ID��Ӧ�����ؿ�Ԥ����
        /// </summary>
        /// <param name="voxelId">����ID�����ؿ�Ԥ�������ࣩ</param>
        /// <returns>��������ID��Ӧ���ؿ��������Ϸ�����������ID=0��65535ʱ���ؿտ�</returns>
        public static GameObject GetVoxelGameObject(ushort voxelId)
        {
            try
            {
                //�������ID�ﵽushort�������͵����ֵ65535����ô���㣨��ֹ�Ӹ�����ʼ��
                if (voxelId == ushort.MaxValue) voxelId = 0;
                GameObject voxelObject = Blocks[voxelId];//��ȡ����ID��Ӧ���ؿ��������Ϸ�������
                //������ض����ϵ��������
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
                //����ָ����Ч����ID
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return Blocks[0];
            }
        }

        /// <summary>
        /// ��ȡ����ID��Ӧ�����ؿ�Ԥ����������������
        /// </summary>
        /// <param name="voxelId">����ID�����ؿ�Ԥ�������ࣩ</param>
        /// <returns>��������ID��Ӧ���ؿ��ϵ������������������ID=0��65535ʱ���ؿտ��ϵ������������</returns>
        public static Voxel GetVoxelType(ushort voxelId)
        {
            try
            {
                //�������ID�ﵽushort�������͵����ֵ65535����ô���㣨��ֹ�Ӹ�����ʼ��
                if (voxelId == ushort.MaxValue) voxelId = 0;
                Voxel voxel = Blocks[voxelId].GetComponent<Voxel>();//��ȡ����ID��Ӧ���ؿ��ϵ������������
                if (voxel == null)
                {
                    //�������������
                    Debug.LogError("Uniblocks: Voxel id " + voxelId + " does not have the Voxel component attached!");
                    return null;
                }
                else
                {
                    //��������ID��Ӧ���ؿ��ϵ������������
                    return voxel;
                }

            }
            catch (System.Exception)
            {
                //����ָ����Ч����ID
                Debug.LogError("Uniblocks: Invalid voxel id: " + voxelId);
                return null;
            }
        }

        /// <summary>
        /// ʹ��ָ��ԭ�㡢����ͷ�Χִ�й���Ͷ�䣬�������������������а��������ſ����Ϸ����(VoxelInfo.chunk)���������ص�����(VoxelInfo.index)����������������������(VoxelInfo.adjacentindex)��
        /// ��ignoreTransparent��Ϊtrueʱ����Ͷ�佫��͸͸�����͸�������ؿ飬��û�л����򷵻�null��ע�⣺�����ײ�����ɱ����ã��˺������������á�
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static VoxelInfo VoxelRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        { // a raycast which returns the index of the hit voxel and the gameobject of the hit chunk

            RaycastHit hit = new RaycastHit(); //��������Ͷ����hit

            //������������Ͷ����ߣ�hit�Ļ��ƴ�origin�������λ�ã�������direction�������ǰ��������������range
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                //�����hit��ײ������������ܻ�ȡ���ſ���ſ���չ�����������ΪCollider�Ǽ̳�Component�ģ����Կ�ֱ��ʹ�ø����GetComponent������ȡ��ǰ��Ϸ����������ϵ������ֵ������
                if (hit.collider.GetComponent<Chunk>() != null || hit.collider.GetComponent<ChunkExtension>() != null)
                { // check if we're actually hitting a chunk.��������Ƿ���Ļ������ſ�

                    GameObject hitObject = hit.collider.gameObject; //����ײ������л����Ϸ������󲢸�ֵ��hitObject

                    if (hitObject.GetComponent<ChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk.������ǻ��е�����״�����������ſ飨�ж���������״����ӵ�д����չ�������ע�������������ſ��С�ģ������ſ���Ӷ������������ؿ飩
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object.�����������滻Ϊʵ�ʵ��ſ��������������������ĸ�������
                    }

                    //����hit��ײ�淨�߷�����������ƽ�hitλ�ã�����λ��תΪ���ſ�ı��ؾֲ����꣨���λ�ã�����ȡ����������falseָ����ȡ�������أ����ƽ�hit���������ؿ��ڲ��������ս�hit��λ�ý����������������Կ������������Ϊ������������
                    Index hitIndex = hitObject.GetComponent<Chunk>().PositionToVoxelIndex(hit.point, hit.normal, false);

                    //����͸����������δ���ƣ�
                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent voxel is hit.��һ��͸�����ر�����ʱ���ٴ�ͨ������Ͷ�䴩͸͸������
                        ushort hitVoxel = hitObject.GetComponent<Chunk>().GetVoxel(hitIndex.x, hitIndex.y, hitIndex.z); //ͨ�������������ſ���������ID�����ؿ�Ԥ�������ࣩ
                        //������е��������͵�VTransparency����!=ʵ�ģ�˵����͸�����͸��
                        if (GetVoxelType(hitVoxel).VTransparency != Transparency.solid)
                        {
                            Vector3 newOrigin = hit.point; //�洢hit����
                            newOrigin.y -= 0.5f; // push the new raycast down a bit.��hit���¸߶��ƶ�0.5��������hit�ܵ���ѡ���ؿ��ڲ���
                            return VoxelRaycast(newOrigin, Vector3.down, range - hit.distance, true); //�ݹ���ú����������µ㿪ʼ��������������ߣ������ʣ�������ײ��⣨trueָ��ȡ�������أ�
                                                                                                      //��δ���ֻ�ܴ������µ�͸�����أ��������������ϡ��������ҵȣ�Ҳ͸����ô�޷���ȷ�ء���͸��


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
        /// ʹ��ָ�����ߺͷ�Χִ�й���Ͷ�䣬������VoxelInfo�����а��������ſ�GameObject(VoxelInfo.chunk)���������ص�����(VoxelInfo.index)�����������������ص�����(VoxelInfo. adjacentindex)��
        /// ��ignoreTransparent��Ϊtrueʱ����Ͷ�佫��͸͸�����͸�������ؿ顣��û�л����κο飬�򷵻�null��ע�⣺�����ײ�����ɱ����ã��˺������������á�
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
        /// �������������λ�����Ӧ���ſ�����
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
        /// �������������λ�����Ӧ���ſ���Ϸ�������ſ�û��ʵ�����򷵻�null��
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
        /// ��λ��ת����������Ϣ�����а������������λ�ö�Ӧ�����أ������ص��ſ�û��ʵ�����򷵻�null��
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
        /// ����ָ���������ĵ������λ�á�
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
        /// <param name="voxel">����ID�����ؿ�Ԥ�������ࣩ</param>
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
