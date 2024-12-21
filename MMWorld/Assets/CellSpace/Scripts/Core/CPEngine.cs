using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using MetalMaxSystem.Unity;

//���繦�ܴ��ھɰ�UNet������Obsolete���־��棩������ʹ���������
namespace CellSpace
{
    #region ö��

    /// <summary>
    /// ��Ԫ��6����
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
    /// �洢ȫ���������ã����ṩһЩ��̬������������ת���ȡ�����÷���Unity������½�һ���ն���CPEngine�����ѽű��ϵ����λ�ü����أ�UnityҪ��һ��cs�ļ�ֻ��һ���࣬�����������ļ���һ�£�
    /// Start��Update�������ڷ�����Unity���ƽű�ִ�еĹؼ�����ʹ����Awake����������������ʱ�ڱ༭�����治�ṩ���������ѡ��
    /// </summary>
    public class CPEngine : MonoBehaviour
    {
        //˽�л�̬�ֶβ����Զ����л���Inspetor����Ҫ�����л���ʹ��[SerializeField]���ԣ��벻�����л���ʹ��[NonSerialized]����

        #region �ֶΡ����Է�����Iǰ׺�ı�����ʾ�ӿڱ������ɱ���д��

        // CellSpaceEngine��ÿ����̬��������һ���Ǿ�̬�ĵȼ���Ǿ�̬�����������뾲̬������ͬ��ֻ���ڿ�ͷ��Сд��L��ʹ����Щ������Ϊ���ܹ���Unity�б༭��Щ�������������������ô����С�
        // �ڱ༭����Awake�����У��Ǿ�̬������Ӧ�������ǵľ�̬��Ӧ(ͨ�������е�Engine��Ϸ����)������������ʱ�ı�Ǿ�̬������������κ�Ӱ�졣

        //OP�ṹ������̬�����Ĺ������أ����ɴ洢Ԥ����ʵ�������GameObject
        public static OP[] PrefabOPs;
        /// <summary>
        /// ����ID�ַ��б�0Ϊ���ͼ����2���ʣ���1~239ΪС��ͼ����3���ʣ�,240Ϊ���������ͼ����4���ʣ���һ��241��
        /// </summary>
        [NonSerialized] public static List<string>[] mapContents = new List<string>[241];
        /// <summary>
        /// ����ID�б�0Ϊ���ͼ����2���ʣ���1~239ΪС��ͼ����3���ʣ�,240Ϊ���������ͼ����4���ʣ���һ��241��
        /// </summary>
        [NonSerialized] public static List<ushort>[] mapIDs = new List<ushort>[241];
        /// <summary>
        /// �洢����ͼƬ�����Ϣ���б����飬����ͼ��������ʹ��
        /// </summary>
        [NonSerialized] public static List<ushort> mapWidths = new List<ushort>();
        /// <summary>
        /// �洢Ĭ����ײ����ΪCube������ID�б����飬��������0����1�ǳ���
        /// ��ײ��������Ԫ����ID���˳�Ĭ����ײ�б� �� �ó�����ԪID�ɽ����ֶ�=�٣�Ĭ�϶��Ǽ٣����س�����������С��ͼ���ӽ��������棩��
        /// ���ͼ��������Ӷ��ǿ��Բȵģ�ֻ��С��ͼ�ϵ���������ܲ��ܲ���Ҫ����״̬�жϡ�
        /// </summary>
        [NonSerialized]
        public static List<ushort>[] cubeColliderIDs = new List<ushort>[]
        {
            new List<ushort>(),
            new List<ushort>()
        };

        /// <summary>
        /// ��ǰ���������ƣ���Ӧ�洢�������ݵ��ļ��У�
        /// </summary>
        public static string WorldName;

        /// <summary>
        /// ���������ļ���·����Ĭ��·��Ϊ/application_root/world_name/������ͨ��ֱ�ӱ༭Engine�ű��ڵ�UpdateWorldPath˽�к�������������·����
        /// </summary>
        public static string WorldPath;

        /// <summary>
        /// ��Ԫ·����Unity��Ŀ�п�Ԥ�����·���������ǿ�༭���������ҿ�ġ�
        /// </summary>
        public static string BlocksPath;

        /// <summary>
        /// ��ǰ���������ӣ������ڳ���������ɣ����Ӵ洢�����������ļ�����
        /// </summary>
        public static int WorldSeed;

        /// <summary>
        /// [GUI��������]���������ļ���·����Ĭ��·��Ϊ/application_root/world_name/������ͨ��ֱ�ӱ༭Engine�ű��ڵ�UpdateWorldPath˽�к�������������·����
        /// </summary>
        public string lWorldName = "DefaultCellSpace";

        /// <summary>
        /// [GUI��������]��Ԫ·����Unity��Ŀ�п�Ԥ�����·���������ǿ�༭���������ҿ�ġ�
        /// </summary>
        public string lBlocksPath;

        /// <summary>
        /// ��ά�������ؿ飨��Ԫ��Ԥ���壬�ڿ�༭���ж��壬����������Ӧ�ڿ������ID����ԪԤ�������ࣩ��
        /// </summary>
        public static GameObject[] Blocks;
        /// <summary>
        /// [GUI��������]��ά�������ؿ飨��Ԫ��Ԥ���壬�ڿ�༭���ж��壬����������Ӧ�ڿ������ID����ԪԤ�������ࣩ��
        /// </summary>
        public GameObject[] lBlocks;

        // �ſ鴴�����ã��ſ������Щ��Ԫ�ļ��ϡ���С�����ɵĴ�飩

        /// <summary>
        /// �����ſ�����������ֱ�����������ſ��Զ�����ʱ�߶ȷ�Χ��ֵ�������3����ʾԭʼ�ſ����¿��Բ���3���ſ飩
        /// </summary>
        public static int HeightRange;
        /// <summary>
        /// ��ԭ�㵽�����ſ��ˮƽ����(���ſ�Ϊ��λ)�����ſ��Զ�����ʱ�ľ������ƣ������8����ʼ�������ԭʼ�ſ���Χ��֤��8��Χ���ſ飩
        /// </summary>
        public static int ChunkSpawnDistance;
        /// <summary>
        /// һ���ſ�ı߳�(�Ե�ԪΪ��λ)�����ſ��Զ�����ʱ�������ε��߳ߴ�߳��������16�����ſ���16^3����Ԫ��ɣ�
        /// </summary>
        public static int ChunkSideLength;
        /// <summary>
        /// �ſ��Զ��ݻ�ʱ���жϾ��루�����3�����ſ���ھ������ChunkSpawnDistance+3���ſ����ʱ���дݻ٣��ݻ�ʱ����뵵���Ա�����߹���ʱ��ȡ���ɣ�
        /// </summary>
        public static int ChunkDespawnDistance;

        /// <summary>
        /// ���θ߶ȣ��������꣩
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
        /// [GUI��������]�����ſ�����������ֱ�����������ſ��Զ�����ʱ�߶ȷ�Χ��ֵ�������3����ʾ�߶�Y�����3���ſ鷶Χ�����ſ���16�߳���ôY���߶�48��
        /// </summary>
        public int lHeightRange;
        /// <summary>
        /// [GUI��������]��ԭ�㵽�����ſ��ˮƽ����(���ſ�Ϊ��λ)�����ſ��Զ�����ʱ�ľ������ƣ������8����ʼ���������Χ��֤��8��Χ���ſ飩
        /// </summary>
        public int lChunkSpawnDistance;
        /// <summary>
        /// [GUI��������]һ���ſ�ı߳�(�Ե�ԪΪ��λ)�����ſ��Զ�����ʱ�������ε��߳ߴ�߳��������16�����ſ���16*16*16����Ԫ��ɣ�
        /// </summary>
        public int lChunkSideLength;
        /// <summary>
        /// [GUI��������]�ſ��Զ��ݻ�ʱ���жϾ��루�����3�����ſ���ھ������ChunkSpawnDistance+3���ſ����ʱ���дݻ٣��ݻ�ʱ����뵵���Ա�����߹���ʱ��ȡ���ɣ�
        /// </summary>
        public int lChunkDespawnDistance;

        // ��������

        /// <summary>
        /// �����߳�/����Ԫ�߳��ı��ʣ����ڼ��㵥Ԫ��������С��������˵���Ļ��൱��ÿ������������ͼƬ�еı����ĵ�������8��X�������8������Ԫ
        /// </summary>
        public static float[] TextureUnitX;
        /// <summary>
        /// �����߳�/����Ԫ�߳��ı��ʣ����ڼ��㵥Ԫ��������С��������˵���Ļ��൱��ÿ������������ͼƬ�еı����ĵ�������8��Y�������8������Ԫ
        /// </summary>
        public static float[] TextureUnitY;
        /// <summary>
        /// �����������֮�����䣬��Ϊ������Ԫ�����С��һС���֡�
        /// ��ÿ������Ԫ֮������Ĵ�С��UV����ʱ��������������������Ա���ȡ���������Ԫ
        /// </summary>
        public static float TexturePadX;
        /// <summary>
        /// �����������֮�����䣬��Ϊ������Ԫ�����С��һС���֡�
        /// ��ÿ������Ԫ֮������Ĵ�С��UV����ʱ��������������������Ա���ȡ���������Ԫ
        /// </summary>
        public static float TexturePadY;

        // �������ã�GUI��������

        /// <summary>
        /// [GUI��������]�����߳�/����Ԫ�߳��ı��ʣ����ڼ��㵥Ԫ��������С��������˵���Ļ��൱��ÿ������������ͼƬ�еı����ĵ�������8��X�������8������Ԫ
        /// </summary>
        public float[] lTextureUnitX;
        /// <summary>
        /// [GUI��������]�����߳�/����Ԫ�߳��ı��ʣ����ڼ��㵥Ԫ��������С��������˵���Ļ��൱��ÿ������������ͼƬ�еı����ĵ�������8��X�������8������Ԫ
        /// </summary>
        public float[] lTextureUnitY;
        /// <summary>
        /// [GUI��������]�����������֮�����䣬��Ϊ������Ԫ�����С��һС���֡�
        /// ��ÿ������Ԫ֮������Ĵ�С��UV����ʱ��������������������Ա���ȡ���������Ԫ
        /// </summary>
        public float lTexturePadX;
        /// <summary>
        /// [GUI��������]�����������֮�����䣬��Ϊ������Ԫ�����С��һС���֡�
        /// ��ÿ������Ԫ֮������Ĵ�С��UV����ʱ��������������������Ա���ȡ���������Ԫ
        /// </summary>
        public float lTexturePadY;

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
        /// [GUI��������]�ſ������������ޣ�ÿ���ͻ���һ�ο����ڷ��������Ŷӵ�����ſ�����������(0=������)��
        /// ��ͻ����������ݿ���ٶ�̫�����㷢����ķ������޷���������������ٶȣ���ô����������ơ�
        /// </summary>
        public int lMaxChunkDataRequests;

        // ȫ������
        /// <summary>
        /// Unetģʽ������״̬Ϊtrue��ʹ�þɰ�Unet���繦�ܣ����Ƽ�������֮ʹ���°�NetCode
        /// </summary>
        public static bool UnetMode = true;
        /// <summary>
        /// ���ģʽ������ʱ����ʹ��XYƽ������ϵ��Ĭ�����߶�Z-�����������XZƽ�棨Ĭ�����߶�Y+�������ģʽĬ��ȡ����Z�����죬����ֻ�����1���ؿ�(������Ϊԭ��)������Z=0�ģ�pixelX,pixelY�������㡣
        /// ����CellChunkMeshCreator�����޸ľ���Ҫ��ʾ�����ؿ���棬Ĭ�Ͻ��������ؿ��back�棨͸����Ļֱ�ӿ��������ؿ鱳�棩��
        /// </summary>
        public static bool HorizontalMode = true;
        /// <summary>
        /// ��ά������ԡ������󣬺��ģʽ�¿ɽ����ؿ��ǰ�����ô������������򲻴�������������������ҪCheckAdjacent���ж��Ƿ񴴽������رգ����ģʽ�½��������ؿ��back�档
        /// </summary>
        public static bool MutiHorizontal = false;
        /// <summary>
        /// ���ֵ��θ߶ȣ����ⴴ����òʱ���������������߶��ϵ������HorizontalMode����Ĭ�����߶�Y+����Z-��������������TerrainHeightȷ���ر�߶ȡ�
        /// ע�⣺�������ε�CPTerrainGeneratorʾ�����пɴ������Ը��ǣ��ڴ����ò�����Ϊ����ֵ���밴����ơ�
        /// </summary>
        public static bool KeepTerrainHeight = false;
        /// <summary>
        /// �����ſ�ʱ����ʹ������Χʹ���κ��ſ���������Ч��������(0,0,0)����һ���ſ�
        /// </summary>
        public static bool KeepOneChunk = true;
        /// <summary>
        /// ��Ԫ�Ĳ���ɼ���ǰ����û�������ǽ������ſ�ʵ������
        /// ���ú�������Ų��������CheckAdjacent()�������Ƿ���������öԱȳ����Ƿ��ϲŷ����档
        /// �������Ŵ�����������������ؿ鼰ִ�д˼������ؿ��͸���ȣ�
        /// 1��������͸��������������Ҳ͸�����ؼ٣���ֹ��һ����ȫ͸�����ؿ��Ա߻�����һ��͸���飩�������棨������ʵ����͸���Ա߻�һ��͸���Ŀ飩��
        /// 2�������ط���ȫ͸������������������ǹ�̬���ؼ٣���ֹ��ʵ�����ؿ��Ա߻�ʵ����͸�����ؿ飩�������棨������͸���Ͱ�͸�����ؿ��Ի���һ��ʵ�Ļ��͸�����ؿ飩��
        /// </summary>
        public static bool ShowBorderFaces;
        /// <summary>
        /// ������ײ�壨Ϊfalse���ſ齫���������κ�Colliders��
        /// </summary>
        public static bool GenerateColliders;
        /// <summary>
        /// ���;�ͷע���¼������Ϊtrue, CameraEventsSender��������¼����͵���������ӳ�����ָ�ŵĵ�Ԫ��
        /// </summary>
        public static bool SendCameraLookEvents;
        /// <summary>
        /// �������ָ���¼������Ϊtrue, CameraEventsSender����������¼����͵���ǰ�����ָ�ŵĵ�Ԫ��
        /// </summary>
        public static bool SendCursorEvents;
        /// <summary>
        /// ���������ң��ſ齫�ӷ���������Ԫ���ݶ����Ǵ�Ӳ�����ɻ���أ�����Voxel.ChangeBlock��Cell.PlaceBlock��Voxel.DestroyBlock�Ὣ��Ԫ�仯���͵��������Ա����·ַ����������ӵ���ң�
        /// </summary>
        public static bool EnableMultiplayer;
        /// <summary>
        /// ����ȷ������ͬ�����λ�õĴ���ʽ�������������ҵ�λ����ȷ���Ƿ���Ҫ����Ԫ���ķ��͸�����ң��ͻ������ͨ��ChunkLoader�ű������������һ�����λ�ø��¡�
        /// �ڶ�����Ϸ�У�ͨ����Ҫ�������λ��ͬ���������ͻ��ˡ����λ����ָ��������һ��·�������ƶ�ʱ������λ�á������MultiplayerTrackPosition�ֶ�����Ϊtrue�����ʾ�������λ�ý��������Ͻ���ͬ������ÿ���ͻ��˶������ٸ�����Ĺ��λ�á�
        /// �����MultiplayerTrackPosition�ֶ�����Ϊfalse�����ʾ�������λ�ò����������Ͻ���ͬ�������ͻ��˽������������λ�á�
        /// �ھ��д����ƶ�����Ķ�����Ϸ�У�ʹ��MultiplayerTrackPosition�ֶο��Լ�������ͨ������������ܡ����磬���ĳ�������ڳ����о�ֹ����������MultiplayerTrackPosition�ֶ�����Ϊfalse���Ա��ⲻ��Ҫ������ͬ����
        /// </summary>
        public static bool MultiplayerTrackPosition;
        /// <summary>
        /// ���浥Ԫ���ݡ�Ϊfalse���ſ齫������ػ򱣴浥Ԫ���ݣ���֮�������ſ�ʱ���ǻ������µ����ݡ�
        /// </summary>
        public static bool SaveCellData;
        /// <summary>
        /// ��������
        /// </summary>
        public static bool GenerateMeshes;

        // [GUI��������]ȫ������

        /// <summary>
        /// [GUI��������]��Ԫ�Ĳ���ɼ���ǰ����û�������ǽ������ſ�ʵ����
        /// </summary>
        public bool lShowBorderFaces;
        /// <summary>
        /// [GUI��������]������ײ�壨Ϊfalse���ſ齫���������κ�Colliders��
        /// </summary>
        public bool lGenerateColliders;
        /// <summary>
        /// [GUI��������]���;�ͷע���¼������Ϊtrue, CameraEventsSender��������¼����͵���������ӳ�����ָ�ŵĵ�Ԫ��
        /// </summary>
        public bool lSendCameraLookEvents;
        /// <summary>
        /// [GUI��������]�������ָ���¼������Ϊtrue, CameraEventsSender����������¼����͵���ǰ�����ָ�ŵĵ�Ԫ��
        /// </summary>
        public bool lSendCursorEvents;
        /// <summary>
        /// [GUI��������]���������ң��ſ齫�ӷ���������Ԫ���ݶ����Ǵ�Ӳ�����ɻ���أ�����Voxel.ChangeBlock��Cell.PlaceBlock��Voxel.DestroyBlock�Ὣ��Ԫ�仯���͵��������Ա����·ַ����������ӵ���ң�
        /// </summary>
        public bool lEnableMultiplayer;
        /// <summary>
        /// [GUI��������]����ȷ������ͬ�����λ�õĴ���ʽ�������������ҵ�λ����ȷ���Ƿ���Ҫ����Ԫ���ķ��͸�����ң��ͻ������ͨ��ChunkLoader�ű������������һ�����λ�ø��¡�
        /// �ڶ�����Ϸ�У�ͨ����Ҫ�������λ��ͬ���������ͻ��ˡ����λ����ָ��������һ��·�������ƶ�ʱ������λ�á������MultiplayerTrackPosition�ֶ�����Ϊtrue�����ʾ�������λ�ý��������Ͻ���ͬ������ÿ���ͻ��˶������ٸ�����Ĺ��λ�á�
        /// �����MultiplayerTrackPosition�ֶ�����Ϊfalse�����ʾ�������λ�ò����������Ͻ���ͬ�������ͻ��˽������������λ�á�
        /// �ھ��д����ƶ�����Ķ�����Ϸ�У�ʹ��MultiplayerTrackPosition�ֶο��Լ�������ͨ������������ܡ����磬���ĳ�������ڳ����о�ֹ����������MultiplayerTrackPosition�ֶ�����Ϊfalse���Ա��ⲻ��Ҫ������ͬ����
        /// </summary>
        public bool lMultiplayerTrackPosition;
        /// <summary>
        /// [GUI��������]���浥Ԫ���ݡ�Ϊfalse���ſ齫������ػ򱣴浥Ԫ���ݣ���֮�������ſ�ʱ���ǻ������µ����ݡ�
        /// </summary>
        public bool lSaveCellData;
        /// <summary>
        /// [GUI��������]��������
        /// </summary>
        public bool lGenerateMeshes;

        /// <summary>
        /// �ſ鳬ʱ����һ�ſ�ͨ��ChunkManager.SpawnChunk��������������ChunkTimeout���ʱ����û�б����ʣ�����������(�ڱ������ĵ�Ԫ���ݺ�)��
        /// ���Կͻ��˵ĵ�Ԫ����������ſ��еĵ�Ԫ�仯�����ü�ʱ������ֵΪ0�����ô˹��ܡ�
        /// </summary>
        public static float ChunkTimeout;
        /// <summary>
        /// �ſ鳬ʱ����һ�ſ�ͨ��ChunkManager.SpawnChunk��������������ChunkTimeout���ʱ����û�б����ʣ�����������(�ڱ������ĵ�Ԫ���ݺ�)��
        /// ���Կͻ��˵ĵ�Ԫ����������ſ��еĵ�Ԫ�仯�����ü�ʱ������ֵΪ0�����ô˹��ܡ�
        /// </summary>
        public float lChunkTimeout;
        /// <summary>
        /// �����ſ鳬ʱ����Engine.ChunkTimeout>0��˱������Զ���Ϊtrue��
        /// </summary>
        public static bool EnableChunkTimeout;

        //����

        /// <summary>
        /// �ſ�߳���ƽ�������ڶ���������С��
        /// </summary>
        public static int SquaredSideLength;
        /// <summary>
        /// ��������ͨ�ź�ͬ������Ϸ�������
        /// </summary>
        public static GameObject Network;
        /// <summary>
        /// ��������ʵ��
        /// </summary>
        public static CPEngine EngineInstance;
        /// <summary>
        /// �ſ������ʵ��
        /// </summary>
        public static CellChunkManager ChunkManagerInstance;
        /// <summary>
        /// �ſ�Ԥ����Ĵ�С�����ű�����
        /// </summary>
        public static Vector3 ChunkScale;
        /// <summary>
        /// �����ʼ��״̬
        /// </summary>
        public static bool Initialized;

        #endregion

        // ==== initialization ====

        /// <summary>
        /// ����Ԥ����ʵ������GUI����ؿ�Ԥ����̫���������ýű���������Ԥ����ʵ�������GameObject��
        /// ��Ȼ�����ڴ���������������ڳ���������Ҫ�ظ��������Դ������������ػ�ʧ�����أ�������Ҫʱȡ��������GameObject����
        /// ע�Ȿ����������Ԥ����ʵ��������ڲ����õ�Blocks���顣
        /// </summary>
        /// <param name="cellID"></param>
        /// <param name="vector"></param>
        /// <param name="subMeshIndex"></param>
        /// <param name="torf">Ĭ��true����Ϸ���崴�����˻�OP����أ���ʹ��ʱȡ��������֮���������Ϸ�����ڳ������Ǽ���״̬</param>
        static void CreatePrefab(ushort cellID, ushort subMeshIndex, Vector2 vector, bool torf = true)
        {
            string name = "cell_" + cellID;
            CPEngine.PrefabOPs[cellID].gameObject = new GameObject(name);
            Cell cell = CPEngine.PrefabOPs[cellID].gameObject.AddComponent<Cell>();//��GameObject����ӵ�Ԫ�������
            cell.VName = name;
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = vector;
            cell.VTransparency = Transparency.solid;
            //��GUI�༭����д���ǲ�����д¼���Ĭ�ϰ�cube��ײ����
            cell.VColliderType = ColliderType.cube;
            //���������������õ�Ԫ��ײ����
            //if (cubeColliderIDs.Contains(cellID))
            //{//����ײ����
            //    cell.VColliderType = ColliderType.cube;
            //}
            //else
            //{//����ײ����
            //    cell.VColliderType = ColliderType.none;
            //}
            cell.VSubmeshIndex = subMeshIndex; //���ͼ����������1�Ĳ�����
            cell.VRotation = MeshRotation.none;
            Blocks[cellID] = PrefabOPs[cellID].gameObject;//����ڲ����õ�Blocks����
            OP.pool.Push(PrefabOPs[cellID]);//��OP�ϵ���Ϸ��������˻�ջ����ʹ��ʱȡ��
        }

        /// <summary>
        /// ������Ԥ����ʵ�����Զ�ʶ��������Ⱦ����Ӧ���������������зָ�UV����ת��ΪԤ����ʵ��������CPEngine.PrefabOPs����
        /// </summary>
        /// <param name="cellID">����ͼ�ĵ�һ��CellID</param>
        /// <param name="endID">����ͼ�����һ��CellID</param>
        /// <param name="textureRow">Y��������</param>
        /// <param name="textureCol">X��������</param>
        /// <param name="subMeshIndex">������Ⱦ���Ĳ�������</param>
        /// <param name="torf">Ĭ��true��lBlocksΪnullʱ�Ž���CreatePrefab����lBlocks����GUI����ĵؿ�Ԥ������ֱ��ʹ�ã�����������CreatePrefab����ʹ��GUI��ĵؿ�Ԥ���壩</param>
        /// <param name="XIncrement">Ĭ��true��UV����ʱ������Ϊԭ�㣩����X������������Ϊflase������Y��������</param>
        static void CreateTexPrefabBatch(ushort cellID, ushort endID, ushort subMeshIndex, ushort textureCol, ushort textureRow, bool torf = true, bool XIncrement = true)
        {
            ushort index = cellID;
            ushort x = 0; //��ǰX����
            ushort y = 0; //��ǰY����
            if (XIncrement)
            {
                //����������Ƭ��ע���������Ƿ�Խ��
                for (ushort row = 0; row < textureRow; row++)
                {//Y����ʱ��X����Ϊ0
                    x = 0;
                    for (ushort col = 0; col < textureCol; col++)
                    {
                        //��������Ƿ�Խ��
                        if (index >= PrefabOPs.Length)
                        {
                            Debug.LogError("Index out of range!");
                            return; //���ػ���Խ�����
                        }
                        if (torf == false || (torf == true && Blocks[index] == null))
                        {//torfΪfalseʱ����CreatePrefab��Ϊtrueʱ����lBlocks��ֵ�Ž���CreatePrefab
                            CreatePrefab(index, subMeshIndex, new Vector2(x, y));
                        }
                        else
                        {//������GUI����ĵؿ�Ԥ������ֱ��ʹ��
                            PrefabOPs[index].gameObject = Instantiate(GetCellGameObject(index)); //OP����󶨸���Ԥ����ʵ�������GameObject��������GUI����ģ�Cell�������Զ���䣩
                        }
                        index++;
                        //�¸�����ID��������ͼ���CellIDʱֱ����������
                        if (index > endID) { return; }
                        x++;
                    }
                    y++;
                }
            }
            else
            {
                //����������Ƭ��ע���������Ƿ�Խ��
                for (ushort col = 0; col < textureCol; col++)
                {//X����ʱ��Y����Ϊ0
                    y = 0;
                    for (ushort row = 0; row < textureRow; row++)
                    {
                        //��������Ƿ�Խ��
                        if (index >= PrefabOPs.Length)
                        {
                            Debug.LogError("Index out of range!");
                            return; //���ػ���Խ�����
                        }
                        if (torf == false || (torf == true && Blocks[index] == null))
                        {//torfΪfalseʱ����CreatePrefab��Ϊtrueʱ����lBlocks��ֵ�Ž���CreatePrefab
                            CreatePrefab(index, subMeshIndex, new Vector2(x, y));
                        }
                        else
                        {//������GUI����ĵؿ�Ԥ������ֱ��ʹ��
                            PrefabOPs[index].gameObject = Instantiate(GetCellGameObject(index)); //OP����󶨸���Ԥ����ʵ�������GameObject��������GUI����ģ�Cell�������Զ���䣩
                        }
                        index++;
                        //�¸�����ID��������ͼ���CellIDʱֱ����������
                        if (index > endID) { return; }
                        y++;
                    }
                    x++;
                }
            }
        }

        /// <summary>
        /// ������������ID�ı������ڵ�ͼ�Զ����ƣ�
        /// </summary>
        static void LoadTXT()
        {
            //��ȡID�ı�ǰ��ʼ�������е�ÿ��ListԪ�أ����ڴ�ų�������ID��
            for (int i = 0; i < mapContents.Length; i++)
            {
                mapContents[i] = new List<string>();
            }
            for (int i = 0; i < mapIDs.Length; i++)
            {
                mapIDs[i] = new List<ushort>();
            }

            //0����װ�������ͼ
            TextAsset textAsset = Resources.Load<TextAsset>("MapIndex/World");
            string tempContent = textAsset.text;
            string[] fields = tempContent.Split(',');
            mapContents[0].AddRange(fields); //�ָ�õ���������ID�ŵ�����0
                                             //string combinedString = string.Join(",", mapContents[0]);
                                             //Debug.Log(combinedString);
                                             // ���ַ���ת��Ϊushort���洢��mapIDs������
            for (int i = 0; i < fields.Length; i++)
            {
                mapIDs[0].Add(ushort.Parse(fields[i]));
            }
            //string joinedString = string.Join(",", mapIDs[0].Select(pixelX => pixelX.ToString()));
            //Debug.Log(joinedString); //Debug.Log(mapIDs[0].Count);

            //1~239����Ӧ��װ����С��ͼ0~238.txt��
            string filePath;
            for (int i = 0; i <= 238; i++)
            {
                filePath = "MapIndex/" + i.ToString();//ʹ��Resources������·������Ҫ�ļ���׺
                textAsset = Resources.Load<TextAsset>(filePath);
                tempContent = textAsset.text;
                fields = tempContent.Split(',');
                mapContents[i + 1].AddRange(fields); //239��С��ͼ��������ID�������������1~239
                                                     // ���ַ���ת��Ϊushort���洢��mapIDs������
                for (int j = 0; j < fields.Length; j++)
                {
                    // ʹ��ushort.Parse��ת���ַ�����ushort
                    mapIDs[i + 1].Add(ushort.Parse(fields[j]));
                }
            }
            //С��ͼ239������ͼƬ�����Ϣ�ı�
            textAsset = Resources.Load<TextAsset>("MapIndex/Width");
            tempContent = textAsset.text;
            fields = tempContent.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                mapWidths.Add(ushort.Parse(fields[i])); //�״������Add�����Ǹ�ֵ����
            }

            //240��������ͼ
            textAsset = Resources.Load<TextAsset>("MapIndex/LZWorld");
            tempContent = textAsset.text;
            fields = tempContent.Split(',');
            mapContents[240].AddRange(fields); //�ָ�õ���������ID�ŵ�����0
                                               //string combinedString = string.Join(",", mapContents[0]);
                                               //Debug.Log(combinedString);
                                               // ���ַ���ת��Ϊushort���洢��mapIDs������
            for (int i = 0; i < fields.Length; i++)
            {
                mapIDs[240].Add(ushort.Parse(fields[i]));
            }
        }

        /// <summary>
        /// ���¿飨������ʹ��ǰ����ˢ�µؿ����ݣ�
        /// </summary>
        static void BlocksRefresh()
        {
            //Unity����ֱ���ж�һ��δʵ������Ԥ�������Ƿ񸽼����ض����������Ϊδʵ������Ԥ���屾��ֻ��һ��ģ�壨���ڴ��������GameObject����ʵ����
            //Ԥ�����ϵ����ֻ����ʵ������Ż��������ڲ��ɱ����ʣ��˴����ǽ�Ƶ��ʹ�õ�Ԥ����ʵ�������������
            //���ݲ��������������й滮
            ushort num0 = 11; //����[0]������Ĭ����11���ؿ�������GUI�����ֶ������CellԤ������������Щ���������Ԥ���壬����ʵ�������������
            //���������������������������ֵĵؿ������������������޶࣬�����ֶ���GUI���룩
            ushort num1 = (ushort)(num0 + 152);//163 
            ushort num2 = (ushort)(num1 + 1360);//1523
            ushort num3 = (ushort)(num2 + 892);//2415�������õ�������ͼ��������

            PrefabOPs = new OP[Blocks.Length]; //����GUI����lBlocksԤ�������������Ա�˴��Զ�����ͬ��Blocks.Length��OP��������OP����������
            OP.pool = new(Blocks.Length); //�����Ԥ��䣬����ΪBlocks.Length������Push����������ʼ������ջҲ���Զ����ݣ�

            //�ֶ������CellԤ����������num0����cell_0~10������[0]�������ϵģ�
            CreateTexPrefabBatch(0, (ushort)(num0 - 1), 0, (ushort)TextureUnitX[0], (ushort)TextureUnitY[0]);

            //�������152�����ͼ����cell_11~162������[1]�������ϵģ�
            //�����ͼ19��8�л��Զ�����ʼ�ͽ�β����ת����152��Ԥ����ʵ��������������ǵ�OP����gameObject�ֶ�
            CreateTexPrefabBatch(num0, (ushort)(num1 - 1), 1, (ushort)TextureUnitX[1], (ushort)TextureUnitY[1]);

            //�������1360��С��ͼ����cell_163~1522������[2]�������ϵģ�
            //��С��ͼ170��8�л��Զ�ת����1360��Ԥ����ʵ��
            CreateTexPrefabBatch(num1, (ushort)(num2 - 1), 2, (ushort)TextureUnitX[2], (ushort)TextureUnitY[2]);

            //�������892С��ͼ����cell_1523~2414������[3]�������ϵģ�������CellID_0��2415��Block����Ԫ��
            //�������ͼĿǰ����ʱ�����õģ�50��18�л��Զ�ת����892��Ԥ����ʵ��
            CreateTexPrefabBatch(num2, (ushort)(num3 - 1), 3, (ushort)TextureUnitX[3], (ushort)TextureUnitY[3]);

            //��ȡ��װ����ȫ��������ID�ı�
            LoadTXT();
        }

        /// <summary>
        /// ��ȡ����ID��Ӧ������Ⱦ���ϵĲ�������
        /// </summary>
        /// <param name="cellId"></param>
        /// <returns></returns>
        public static ushort GetSubMeshIndex(ushort cellId)
        {
            ushort torf = 0;
            if (cellId >= 11)
            {
                if (cellId < 163) { torf = 1; }
                else if (cellId < 1523) { torf = 2; }
                else if (cellId < 2415) { torf = 3; }
                else { Debug.Log("����ID���������������������ޣ�"); }
            }
            return torf;
        }

        //Awake�����ڽű�ʵ��������ʱ�����ã�ͨ����������Ϸ���󱻴�����ű�����ӵ���Ϸ������
        public void Awake()
        {
            //���������������ʼ��

            //��¼��Ĭ����ײ����ײ��������Ԫ����ID���˳�Ĭ����ײ�б� �� �ó�����ԪID�ɽ����ֶ�=�٣�Ĭ�϶��Ǽ٣����س�����������С��ͼ���ӽ��������棩��

            //�˵�Ĭ����ײ����
            for (ushort i = 11; i <= 20; i++) { cubeColliderIDs[0].Add(i); }//���ͼ���µ�һ�������11�������
            for (ushort i = 23; i <= 24; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 23; i <= 24; i++) { cubeColliderIDs[0].Add(i); }
            cubeColliderIDs[0].Add(29);//դ��
            for (ushort i = 34; i <= 37; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 39; i <= 46; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 48; i <= 53; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 55; i <= 57; i++) { cubeColliderIDs[0].Add(i); }
            cubeColliderIDs[0].Add(60);
            cubeColliderIDs[0].Add(62);
            cubeColliderIDs[0].Add(67);
            for (ushort i = 80; i <= 84; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 90; i <= 102; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 109; i <= 112; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 115; i <= 116; i++) { cubeColliderIDs[0].Add(i); }
            cubeColliderIDs[0].Add(118);
            for (ushort i = 121; i <= 124; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 151; i <= 155; i++) { cubeColliderIDs[0].Add(i); }
            for (ushort i = 159; i <= 162; i++) { cubeColliderIDs[0].Add(i); }
            //�����ͼ��ײ¼�����

            //����Ĭ����ײ����
            for (ushort i = 11; i <= 20; i++) { cubeColliderIDs[1].Add(i); }//ɽ��
            for (ushort i = 23; i <= 25; i++) { cubeColliderIDs[1].Add(i); }//����С��
            cubeColliderIDs[1].Add(29);//դ��
            for (ushort i = 34; i <= 37; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 48; i <= 53; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 55; i <= 57; i++) { cubeColliderIDs[1].Add(i); }
            cubeColliderIDs[1].Add(60);
            cubeColliderIDs[1].Add(62);
            cubeColliderIDs[1].Add(67);
            cubeColliderIDs[1].Add(72); cubeColliderIDs[1].Add(73);
            for (ushort i = 80; i <= 84; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 90; i <= 102; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 109; i <= 112; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 115; i <= 116; i++) { cubeColliderIDs[1].Add(i); }
            cubeColliderIDs[1].Add(118);
            for (ushort i = 121; i <= 124; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 151; i <= 155; i++) { cubeColliderIDs[1].Add(i); }
            for (ushort i = 159; i <= 162; i++) { cubeColliderIDs[1].Add(i); }
            //�����ͼ��ײ¼�����

            EngineInstance = this; //this�ؼ��������˵�ǰ���һ��ʵ���������������ھ�̬�ֶεĳ�ʼ���У�����д����
            //��ȡ�����ϵ��ſ���������ʵ��������ָ��Ϊ"CellChunkManager"�Ľű��������ʵ������Ķ���
            ChunkManagerInstance = GetComponent<CellChunkManager>();
            //��ȡGUI���������������������
            WorldName = lWorldName;
            //��������浵��·��
            UpdateWorldPath();

            #region ��GUI�����������ݸ�ֵ��ʵ���������ֶ�����

            BlocksPath = lBlocksPath;
            Blocks = lBlocks; //��GUI������ק��Engine��CellԤ���壨ֻ��ק��һ���֣�ʣ��̫�������Խ������ô���׷�ӣ�

            TargetFPS = lTargetFPS;
            MaxChunkSaves = lMaxChunkSaves;
            MaxChunkDataRequests = lMaxChunkDataRequests;

            TextureUnitX = lTextureUnitX;
            TextureUnitY = lTextureUnitY;
            TexturePadX = lTexturePadX;
            TexturePadY = lTexturePadY;
            GenerateColliders = lGenerateColliders;
            ShowBorderFaces = lShowBorderFaces;
            EnableMultiplayer = lEnableMultiplayer;
            MultiplayerTrackPosition = lMultiplayerTrackPosition;
            SaveCellData = lSaveCellData;
            GenerateMeshes = lGenerateMeshes;

            ChunkSpawnDistance = lChunkSpawnDistance;
            HeightRange = lHeightRange;
            ChunkDespawnDistance = lChunkDespawnDistance;

            SendCameraLookEvents = lSendCameraLookEvents;
            SendCursorEvents = lSendCursorEvents;

            ChunkSideLength = lChunkSideLength;
            SquaredSideLength = lChunkSideLength * lChunkSideLength;

            #endregion

            BlocksRefresh();

            //�����Ѽ��ص������飨�ֵ�<string, string[]>��
            CellChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
            //������ʱ�ſ������飨�ֵ�<string, string>��
            CellChunkDataFiles.TempChunkData = new Dictionary<string, string>();

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
                Debug.LogWarning("CellSpace: Layer 26 is reserved for CellSpace, it is automatically set to ignore collision with all layers." +
                                 "��26����ΪUniblocks�����ģ������Զ�����Ϊ����������ͼ�����ײ��");
            }
            for (int i = 0; i < 31; i++)
            {
                //Unity��32�����õĲ�0~31���˴����õ�i~26֮��Ķ��󲻷�����ײ�����ܴ����˴˶�����ײ�¼���
                Physics.IgnoreLayerCollision(i, 26);
            }

            #region ����ſ�

            //���GUI����������Ԥ����ĵ�Ԫ�������
            if (Blocks.Length < 1)
            {
                Debug.LogError("CellSpace: The blocks array is empty! Use the Block Editor to update the blocks array." +
                    "��Ԫ�ǿյģ�ʹ�ÿ�༭�������£�");
                Debug.Break();
            }

            //����һ����Ԫ���տ飩�Ƿ���ڣ��粻���ڻ�û�е�Ԫ����򱨴�
            if (Blocks[0] == null)
            {
                Debug.LogError("CellSpace: Cannot find the empty block prefab (id 0)!" +
                    "�Ҳ����տ�Ԥ���壨id 0����");
                Debug.Break();
            }
            else if (Blocks[0].GetComponent<Cell>() == null)
            {
                Debug.LogError("CellSpace: Empty block prefab (id 0) does not have the Cell component attached!" +
                    "�տ�Ԥ����(id 0)û�е�Ԫ���");
                Debug.Break();
            }

            #endregion

            #region �������

            //����ſ�߳�������Ϊ1����Ч��
            if (ChunkSideLength < 1)
            {
                Debug.LogError("CellSpace: CellChunk side length must be greater than 0!" +
                    "�ſ�߳��������0");
                Debug.Break(); //��ͣ�༭������
            }

            //����ſ����ɾ���<1����Ϊ0���������ɣ���Ĭ����8
            if (ChunkSpawnDistance < 1)
            {
                ChunkSpawnDistance = 0;
                if (KeepOneChunk == false)
                {
                    Debug.LogWarning("CellSpace: CellChunk spawn distance is 0." + "�ſ����ɾ���Ϊ0��KeepOneChunk=�٣��޷����ɿռ��ſ飡");
                }
            }

            //����߶ȷ�ΧС��0����߶ȷ�Χ������Ϊ0��Ĭ����3
            if (HeightRange < 0)
            {
                HeightRange = 0;
                Debug.LogWarning("CellSpace: CellChunk height range can'transform be a negative number! Setting chunk height range to 0." +
                    "�ſ�߶ȷ�Χ������һ���������ѱ�����Ϊ0");
            }

            //����ſ�������������
            if (MaxChunkDataRequests < 0)
            {
                MaxChunkDataRequests = 0;
                Debug.LogWarning("CellSpace: Max chunk data requests can'transform be a negative number! Setting max chunk data requests to 0." +
                    "�ſ������������޲����Ǹ������ѱ�����Ϊ0");
            }

            #endregion

            //������
            GameObject chunkPrefab = GetComponent<CellChunkManager>().ChunkObject; //��ȡ�ſ�������й������ſ����������Ϊ�ſ�Ԥ����
            int materialCount = chunkPrefab.GetComponent<Renderer>().sharedMaterials.Length - 1; //�����⣩���ʼ���=�ſ�Ԥ������Ⱦ����Ĺ����������-1

            //����������Ԥ����ĵ�Ԫ
            for (ushort i = 0; i < Blocks.Length; i++)
            {
                if (Blocks[i] != null)
                {
                    //��ȡ��Ԫ
                    Cell cell = Blocks[i].GetComponent<Cell>();

                    //�����Ԫ����������<0�򱨴�
                    if (cell.VSubmeshIndex < 0)
                    {
                        Debug.LogError("CellSpace: Cell " + i + " has a material index lower than 0! Material index must be 0 or greater." +
                            "��Ԫ�Ĳ�������С��0��������ڵ���0");
                        Debug.Break();
                    }

                    //�絥Ԫ�������������ڣ����⣩���ʼ����򱨴�ʹ���Զ���Ķ������������û�����ϲ��ʣ�
                    if (cell.VSubmeshIndex > materialCount)
                    {
                        //��Ԫʹ����GUI�����������Զ���������������ſ�Ԥ����ֻ�У����⣩���ʼ���+1�����ʸ��ţ�����һ�����͵Ĳ����������Ÿ�����ʵ��ſ�Ԥ���壡
                        Debug.LogError("CellSpace: Cell " + i + " uses material index " + cell.VSubmeshIndex + ", but the chunk prefab only has " + (materialCount + 1) + " material(s) attached. Set a lower material index or attach more materials to the chunk prefab.");
                        Debug.Break();
                    }
                }
            }

            //�������ã���鿹��ݹ��ܣ��رպ�ɷ�ֹ��Ե������Ӿ���϶��ӦĬ�Ϲر�
            if (QualitySettings.antiAliasing > 0)
            {
                Debug.LogWarning("CellSpace: Anti-aliasing is enabled. This may cause seam lines to appear between blocks. If you see lines between blocks, try disabling anti-aliasing, switching to deferred rendering path, or adding some texture padding in the engine settings." +
                    "�����˿���ݣ�����ܵ����ڿ�֮����ֽӷ��ߣ�����㿴����֮������������Ž��ÿ���ݣ��л����ӳ���Ⱦ·�����������������������һЩ������䡣");
            }

            //�����ʼ��״̬
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
                                                                                //WorldPath = "/mnt/sdcard/UniblocksWorlds/" + CPEngine.WorldName + "/"; // example mobile path for Android
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

            //if (Application.isWebPlayer) { // don'transform save to file if webplayer		
            //	CPEngine.WorldSeed = Random.CameraLookRange (ushort.MinValue, ushort.MaxValue);
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
            EngineInstance.StartCoroutine(CellChunkDataFiles.SaveAllChunks());
        }

        /// <summary>
        /// �����е�ǰʵ�������ſ����ݱ��浽���̴浵����֡����һ��ȫִ�У���ܿ��ܻ�ʹ��Ϸ���Ἰ���ӣ���˲���������Ϸ������ʹ�ô˹��ܡ�
        /// </summary>
        public static void SaveWorldInstant()
        { // writes data from TempChunkData into region files

            CellChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====	

        /// <summary>
        /// ��ȡ����ID��Ӧ�ĵ�ԪԤ����
        /// </summary>
        /// <param name="cellId">����ID����ԪԤ�������ࣩ</param>
        /// <returns>��������ID��Ӧ��Ԫ�������Ϸ�����������ID=0��65535ʱ���ؿտ�</returns>
        public static GameObject GetCellGameObject(ushort cellId)
        {
            try
            {
                //�������ID�ﵽushort�������͵����ֵ65535����ô���㣨��ֹ�Ӹ�����ʼ��
                if (cellId == ushort.MaxValue) cellId = 0;
                GameObject cellObject = Blocks[cellId];//��ȡ����ID��Ӧ��Ԫ�����Ԥ����
                //��鵥Ԫ�����ϵĵ�Ԫ���
                if (cellObject.GetComponent<Cell>() == null)
                {
                    Debug.LogError("CellSpace: Cell id " + cellId + " does not have the Cell component attached!" +
                        "��Ϸ�������ĵ�Ԫ��������ڣ����ؿտ飡");
                    return Blocks[0];
                }
                else
                {
                    return cellObject;
                }

            }
            catch (System.Exception)
            {
                //����ָ����Ч����ID
                Debug.LogError("CellSpace: Invalid cell id: " + cellId);
                return Blocks[0];
            }
        }

        /// <summary>
        /// ��ȡ����ID��Ӧ�ĵ�ԪԤ����ĵ�Ԫ�������
        /// </summary>
        /// <param name="cellID">����ID����ԪԤ�������ࣩ</param>
        /// <returns>��������ID��Ӧ��Ԫ�ϵĵ�Ԫ�������������ID=0��65535ʱ���ؿտ��ϵĵ�Ԫ�������</returns>
        public static Cell GetCellType(ushort cellID)
        {
            try
            {
                //�������ID�ﵽushort�������͵����ֵ65535����ô���㣨��ֹ�Ӹ�����ʼ��
                if (cellID == ushort.MaxValue) cellID = 0;
                Cell cell = Blocks[cellID].GetComponent<Cell>();//��ȡ����ID��Ӧ��Ԫ�ϵĵ�Ԫ�������
                if (cell == null)
                {
                    //��Ԫ���������
                    Debug.LogError("CellSpace: Cell ID " + cellID + " does not have the Cell component attached!");
                    return null;
                }
                else
                {
                    //��������ID��Ӧ��Ԫ�ϵĵ�Ԫ�������
                    return cell;
                }

            }
            catch (System.Exception)
            {
                //����ָ����Ч����ID
                Debug.LogError("CellSpace: Invalid Cell ID: " + cellID);
                return null;
            }
        }

        /// <summary>
        /// ʹ��ָ��ԭ�㡢����ͷ�Χִ�й���Ͷ�䣬�����ص�Ԫ���������а��������ſ����Ϸ����(CellInfo.chunk)�����е�Ԫ������(CellInfo.index)�������������ڵ�Ԫ����(CellInfo.adjacentindex)��
        /// ��ignoreTransparent��Ϊtrueʱ����Ͷ�佫��͸͸�����͸���ĵ�Ԫ����û�л����򷵻�null��ע�⣺�����ײ�����ɱ����ã��˺������������ã�����2Dģʽ��Z=0��Ĭ�ϲ���һ�������壬ֻ��������Up���ΪForward
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static CellInfo CellRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        { // a raycast which returns the index of the hit cell and the gameobject of the hit chunk

            RaycastHit hit = new RaycastHit(); //��������Ͷ����hit

            //������������Ͷ����ߣ�hit�Ļ��ƴ�origin�������λ�ã�������direction�������ǰ��������������range
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                //�����hit��ײ������������ܻ�ȡ���ſ���ſ���չ�����������ΪCollider�Ǽ̳�Component�ģ����Կ�ֱ��ʹ�ø����GetComponent������ȡ��ǰ��Ϸ����������ϵ������ֵ������
                if (hit.collider.GetComponent<CellChunk>() != null || hit.collider.GetComponent<CellChunkExtension>() != null)
                { // check if we're actually hitting a chunk.��������Ƿ���Ļ������ſ�

                    GameObject hitObject = hit.collider.gameObject; //����ײ������л����Ϸ������󲢸�ֵ��hitObject

                    if (hitObject.GetComponent<CellChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk.������ǻ��е�����״�����������ſ飨�ж���������״����ӵ�д����չ�������ע�������������ſ��С�ģ������ſ���Ӷ��������ǵ�Ԫ��
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object.�����������滻Ϊʵ�ʵ��ſ��������������������ĸ�������
                    }

                    //����hit��ײ�淨�߷�����������ƽ�hitλ�ã�����λ��תΪ���ſ�ı��ؾֲ����꣨���λ�ã�����ȡ��Ԫ������falseָ����ȡ���ڵ�Ԫ�����ƽ�hit��������Ԫ�ڲ��������ս�hit��λ�ý����������������Կ������������Ϊ��Ԫ��������
                    CPIndex hitIndex = hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, false);

                    //����͸����������δ���ƣ�
                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent cell is hit.��һ��͸����Ԫ������ʱ���ٴ�ͨ������Ͷ�䴩͸͸����Ԫ
                        ushort hitCell = hitObject.GetComponent<CellChunk>().GetCellID(hitIndex.x, hitIndex.y, hitIndex.z); //ͨ����Ԫ�������ſ���������ID����ԪԤ�������ࣩ
                        //������еĵ�Ԫ���͵�VTransparency����!=ʵ�ģ�˵����͸�����͸��
                        if (GetCellType(hitCell).VTransparency != Transparency.solid)
                        {
                            Vector3 newOrigin = hit.point; //�洢hit����
                            newOrigin.y -= 0.5f; // push the new raycast down a bit.��hit���¸߶��ƶ�0.5��������hit�ܵ���ѡ��Ԫ�ڲ���
                            return CellRaycast(newOrigin, Vector3.down, range - hit.distance, true); //�ݹ���ú����������µ㿪ʼ��������������ߣ������ʣ�������ײ��⣨trueָ��ȡ���ڵ�Ԫ��
                                                                                                     //��δ���ֻ�ܴ������µ�͸����Ԫ���������������ϡ��������ҵȣ�Ҳ͸����ô�޷���ȷ�ء���͸��


                        }
                    }

                    return new CellInfo(
                                         hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, false), // get hit cell index.��ȡ���е�Ԫ������
                                         hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, true), // get adjacent cell index.��ȡ���ڵ�Ԫ������
                                         hitObject.GetComponent<CellChunk>()); // get chunk.��ȡ�ſ�
                }
            }

            //�������
            return null;
        }

        /// <summary>
        /// ʹ��ָ�����ߺͷ�Χִ�й���Ͷ�䣬������VoxelInfo�����а��������ſ�GameObject(CellInfo.chunk)�����е�Ԫ������(CellInfo.index)�������������ڵ�Ԫ������(CellInfo. adjacentindex)��
        /// ��ignoreTransparent��Ϊtrueʱ����Ͷ�佫��͸͸�����͸���ĵ�Ԫ����û�л����κο飬�򷵻�null��ע�⣺�����ײ�����ɱ����ã��˺������������á�
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static CellInfo CellRaycast(Ray ray, float range, bool ignoreTransparent)
        {
            return CellRaycast(ray.origin, ray.direction, range, ignoreTransparent);
        }

        /// <summary>
        /// �������������λ�����Ӧ���ſ�����
        /// </summary>
        /// <param name="position">�ſ�λ��</param>
        /// <returns></returns>
        public static CPIndex PositionToChunkIndex(Vector3 position)
        {
            CPIndex chunkIndex;
            if (CPEngine.HorizontalMode)
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength);
            }
            else
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            }
            return chunkIndex;
        }

        /// <summary>
        /// �������������λ�����Ӧ���ſ���Ϸ�������ſ�û��ʵ�����򷵻�null��
        /// </summary>
        /// <param name="position">�ſ�λ��</param>
        /// <returns></returns>
        public static GameObject PositionToChunk(Vector3 position)
        {
            CPIndex chunkIndex;
            if (CPEngine.HorizontalMode)
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength);
            }
            else
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / ChunkScale.x) / ChunkSideLength,
                                          Mathf.RoundToInt(position.y / ChunkScale.y) / ChunkSideLength,
                                          Mathf.RoundToInt(position.z / ChunkScale.z) / ChunkSideLength);
            }

            return CellChunkManager.GetChunk(chunkIndex);

        }

        /// <summary>
        /// ��λ��ת���ɵ�Ԫ��Ϣ�����а������������λ�ö�Ӧ�ĵ�Ԫ���絥Ԫ���ſ�û��ʵ�����򷵻�null��
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static CellInfo PositionToCellInfo(Vector3 position)
        {
            GameObject chunkObject = PositionToChunk(position);
            if (chunkObject != null)
            {
                CellChunk chunk = chunkObject.GetComponent<CellChunk>();
                CPIndex cellIndex = chunk.PositionToCellIndex(position);
                return new CellInfo(cellIndex, chunk);
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// ����ָ����Ԫ���ĵ������λ�á�
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <returns></returns>
        public static Vector3 CellInfoToPosition(CellInfo cellInfo)
        {
            return cellInfo.chunk.GetComponent<CellChunk>().CellIndexToPosition(cellInfo.index);
        }

        // ==== mesh creator ====

        //��༭���������ѡCustom Mesh������Ĭ�ϲ��ʣ�ֻ�в���ѡCustom Meshʱ�����Զ�����ʣ�Ҳ���ǿ���ѡ��ͼƬ����ƫ��
        //ͼƬ����ƫ�������֣���һ���ǹ�ѡ���嵥�����������6����������һ��д����ƫ�Ƶ㣻�ڶ����ǲ���ѡ����ô6���������ͬ����ƫ�Ƶ�
        //ʹ������ƫ�Ƶ��ȡԤ��ͼƬ�ϵ��������½Ǿ�����(0,0)����(1,0)��ʾ�ұ߾��飬ÿ������Ĵ�С������������Texture unit����
        //Texture unit��ֵ��8���ڲ�UV����ʱ�ǵ�������0.125����ʾ������ͼƬ�и�Ϊ8*8=64�����ֵľ��飬MC���ħ�ĺ��ΪCellSpace�⣬֧�����в�ͬ����������ʶ��������������

        /// <summary>
        /// ��ȡ����ƫ�Ƶ�
        /// </summary>
        /// <param name="cellID">����ID����ԪԤ�������ࣩ</param>
        /// <param name="facing">����</param>
        /// <returns>��û���������򷵻�Vector2(0, 0)���絥Ԫû���Զ��嵥�������򷵻ض�������㣬������һ��û�����������ץȡ����������������</returns>
        public static Vector2 GetTextureOffset(ushort cellID, Facing facing)
        {
            //��ȡ��Ԫ������
            Cell cellType = GetCellType(cellID);
            //��ȡ�������飨��ά��������С�飩
            Vector2[] textureArray = cellType.VTexture;
            if (textureArray.Length == 0)
            { // in case there are no textures defined, return a default texture.�Է���һ���û�ж��������򷵻�Ĭ�ϵ�(0, 0)
                Debug.LogWarning("CellSpace: Block " + cellID.ToString() + " has no defined textures! Using default texture.");
                return new Vector2(0, 0);
            }
            else if (cellType.VCustomSides == false)
            { // if this cell isn'transform using custom side textures, return the Up texture.��������Ԫû��ʹ���Զ��嵥������ֱ�ӷ��ص�Ԫ���ϵ���������㣨6���湲�ã�
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
