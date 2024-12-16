using MetalMaxSystem.Unity;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// ���ص�Ԫ��Cell/Voxel�����洢3D�������ص��ֶ����Բ��ṩ�����ض����ض���ȷ�������CellInfo����������������������λ����Ϣ��
    /// </summary>
    public class Cell : MonoBehaviour
    {
        /// <summary>
        /// ��������
        /// </summary>
        public string VName;
        /// <summary>
        /// ��������
        /// </summary>
        public Mesh VMesh;
        /// <summary>
        /// ����ʹ���Զ�������
        /// </summary>
        public bool VCustomMesh;
        /// <summary>
        /// ����6������Զ�����������ͨ��ʹ����ͬ����
        /// </summary>
        public bool VCustomSides;
        /// <summary>
        /// ��������[����]����������ָ���������� (��VTexture[0]�������嶥����������0~5�ֱ��ʾ��������ǰ��)
        /// </summary>
        public Vector2[] VTexture;
        /// <summary>
        /// ����͸����
        /// </summary>
        public Transparency VTransparency;
        /// <summary>
        /// ������ײ�����ͣ���Ϊ���ޡ����������塣
        /// ������ײ����ʶ���Զ���ģ���е����񣨸߸��Ӷ�����½ϳ����ܣ�����Ԥ�Ƶ���������ײ����������������Ϊ����
        /// �����������֣���Unity�ȿ�ƾ�մӶ��������δ�����Ҳ���޸ı༭��������
        /// </summary>
        public ColliderType VColliderType;
        /// <summary>
        /// ��������������������GUI����������Զ������������Material Index�������ſ�Ԥ����ֻ�ж�����ʼ���+1�����ʸ���ʱ��������һ�����͵Ĳ����������Ÿ�����ʵ��ſ�Ԥ������򱨴�
        /// </summary>
        public int VSubmeshIndex;
        /// <summary>
        /// ������ת
        /// </summary>
        public MeshRotation VRotation;

        /// <summary>
        /// �ݻ����ؿ飬ʵ���ǽ���Ԫ����Ϊ��(id 0)�����µ�Ԫ�����񲢴���OnBlockDestroy�¼���
        /// </summary>
        /// <param name="cellInfo"></param>
        public static void DestroyBlock(CellInfo cellInfo)
        {
            // multiplayer - send change to server
            if (CPEngine.EnableMultiplayer)
            {
                //�����紦�����CPEngine�����ŵ�Client����е���SendPlaceBlock����
                CPEngine.Network.GetComponent<Client>().SendPlaceBlock(cellInfo, 0);
            }
            // single player - apply change locally
            else
            {
                //��CellInfoȡ������ID��Ȼ���Block[����ID]ȡ��Ԥ���岢����һ��ʵ����
                //��ֻ��Ϊ����ȡ�����֤�������¼����������ʹݻٷǳ���Ч�ʣ������GameObject��һ����Ҫ�Ż��ĵط�
                //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(cellInfo.GetCellID()));
                OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //��ջȡ��OP����
                CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //��ȡ����OP���󼤻������Ϸ����Ҳ�ἤ�
                GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //��Ϸ���帳ֵ
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    //�����ID�������CellEvents�����ʹ�ø���������Ե�Ԫִ��һ�δݻ��¼�
                    cellObject.GetComponent<CellEvents>().OnBlockDestroy(cellInfo);
                }
                //���øõ�Ԫ�������ĵ�λ����Ϊ0
                cellInfo.chunk.SetCell(cellInfo.index, 0, true);
                //�ݻ�ʵ����Ҳû�л��յ�����أ�����д�ܲ�Ч�ʣ�
                //Destroy(cellObject);
                OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //�����˻ض����
            }
        }
        /// <summary>
        /// �������ؿ飬ʵ���ǽ���Ԫ����Ϊָ����id�����µ�Ԫ�����񲢴���OnBlockPlace�¼���
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        public static void PlaceBlock(CellInfo cellInfo, ushort data)
        {
            // multiplayer - send change to server
            if (CPEngine.EnableMultiplayer)
            {
                //�����紦�����CPEngine�����ŵ�Client����е���SendPlaceBlock����
                CPEngine.Network.GetComponent<Client>().SendPlaceBlock(cellInfo, data);
            }
            // single player - apply change locally
            else
            {
                cellInfo.chunk.SetCell(cellInfo.index, data, true);
                //��ֻ��Ϊ����ȡ�����֤�������¼����������ʹݻٷǳ���Ч�ʣ������GameObject��һ����Ҫ�Ż��ĵط�
                //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
                OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //��ջȡ��OP����
                CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //��ȡ����OP���󼤻������Ϸ����Ҳ�ἤ�
                GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //��Ϸ���帳ֵ
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    cellObject.GetComponent<CellEvents>().OnBlockPlace(cellInfo);
                }
                //Destroy(cellObject);
                OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //�����˻ض����
            }
        }
        /// <summary>
        /// �������ؿ飬ʵ���ǽ���Ԫ����Ϊָ����id�����µ�Ԫ�����񲢴���OnBlockChange�¼���
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        public static void ChangeBlock(CellInfo cellInfo, ushort data)
        {
            // multiplayer - send change to server
            if (CPEngine.EnableMultiplayer)
            {
                //�����紦�����CPEngine�����ŵ�Client����е���SendChangeBlock����
                CPEngine.Network.GetComponent<Client>().SendChangeBlock(cellInfo, data);
            }
            // single player - apply change locally
            else
            {
                cellInfo.chunk.SetCell(cellInfo.index, data, true);
                //��ֻ��Ϊ����ȡ�����֤�������¼����������ʹݻٷǳ���Ч�ʣ������GameObject��һ����Ҫ�Ż��ĵط�
                //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
                OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //��ջȡ��OP����
                CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //��ȡ����OP���󼤻������Ϸ����Ҳ�ἤ�
                GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //��Ϸ���帳ֵ
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    cellObject.GetComponent<CellEvents>().OnBlockChange(cellInfo);
                }
                //Destroy(cellObject);
                OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //�����˻ض����
            }
        }

        // multiplayer

        /// <summary>
        /// �ݻ����ؿ飬����Ԫ����Ϊ�յ�Ԫ(id 0)�����µ�Ԫ�����񲢴���OnBlockDestroy�¼���������ö���ģʽ������Ԫ���ķ��͸��������ӵ���ҡ�
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="sender"></param>
        public static void DestroyBlockMultiplayer(CellInfo cellInfo, NetworkPlayer sender)
        { // received from server, don'transform use directly

            //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(cellInfo.GetCellID()));
            OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //��ջȡ��OP����
            CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //��ȡ����OP���󼤻������Ϸ����Ҳ�ἤ�
            GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //��Ϸ���帳ֵ
            CellEvents events = cellObject.GetComponent<CellEvents>();
            if (events != null)
            {
                events.OnBlockDestroy(cellInfo);
                events.OnBlockDestroyMultiplayer(cellInfo, sender);
            }
            cellInfo.chunk.SetCell(cellInfo.index, 0, true);
            //Destroy(cellObject);
            OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //�����˻ض����
        }
        /// <summary>
        /// �������ؿ飬����Ԫ����Ϊָ����id�����µ�Ԫ�����񲢴���OnBlockPlace�¼���������ö���ģʽ������Ԫ���ķ��͸��������ӵ���ҡ�
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        public static void PlaceBlockMultiplayer(CellInfo cellInfo, ushort data, NetworkPlayer sender)
        { // received from server, don'transform use directly

            cellInfo.chunk.SetCell(cellInfo.index, data, true);
            //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
            OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //��ջȡ��OP����
            CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //��ȡ����OP���󼤻������Ϸ����Ҳ�ἤ�
            GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //��Ϸ���帳ֵ
            CellEvents events = cellObject.GetComponent<CellEvents>();
            if (events != null)
            {
                events.OnBlockPlace(cellInfo);
                events.OnBlockPlaceMultiplayer(cellInfo, sender);
            }
            //Destroy(cellObject);
            OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //�����˻ض����
        }
        /// <summary>
        /// �������ؿ飬����Ԫ����Ϊָ����id�����µ�Ԫ�����񲢴���OnBlockChange�¼���������ö���ģʽ������Ԫ���ķ��͸��������ӵ���ҡ�
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        public static void ChangeBlockMultiplayer(CellInfo cellInfo, ushort data, NetworkPlayer sender)
        { // received from server, don'transform use directly

            cellInfo.chunk.SetCell(cellInfo.index, data, true);
            //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
            OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //��ջȡ��OP����
            CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //��ȡ����OP���󼤻������Ϸ����Ҳ�ἤ�
            GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //��Ϸ���帳ֵ
            CellEvents events = cellObject.GetComponent<CellEvents>();
            if (events != null)
            {
                events.OnBlockChange(cellInfo);
                events.OnBlockChangeMultiplayer(cellInfo, sender);
            }
            //Destroy(cellObject);
            OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //�����˻ض����
        }

        // block editor functions

        /// <summary>
        /// ��ȡ����ID����Ԫ���ࣩ
        /// </summary>
        /// <returns></returns>
        public ushort GetID()
        {
            return ushort.Parse(this.gameObject.name.Split('_')[1]);
        }
        /// <summary>
        /// �趨����ID����Ԫ���ࣩ
        /// </summary>
        /// <param name="id"></param>
        public void SetID(ushort id)
        {
            this.gameObject.name = "cell_" + id.ToString();
        }
    }
}
