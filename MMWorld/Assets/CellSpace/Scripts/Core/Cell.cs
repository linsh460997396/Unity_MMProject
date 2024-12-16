using MetalMaxSystem.Unity;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 体素单元（Cell/Voxel），存储3D立方体素的字段属性并提供访问特定体素对象等方法，与CellInfo区别在于少了体素索引（位置信息）
    /// </summary>
    public class Cell : MonoBehaviour
    {
        /// <summary>
        /// 体素名称
        /// </summary>
        public string VName;
        /// <summary>
        /// 体素网格
        /// </summary>
        public Mesh VMesh;
        /// <summary>
        /// 体素使用自定义网格
        /// </summary>
        public bool VCustomMesh;
        /// <summary>
        /// 体素6面采用自定义纹理（而非通体使用相同纹理）
        /// </summary>
        public bool VCustomSides;
        /// <summary>
        /// 体素纹理[索引]，数组索引指定了纹理面 (如VTexture[0]是立方体顶面纹理，索引0~5分别表示上下右左前后)
        /// </summary>
        public Vector2[] VTexture;
        /// <summary>
        /// 体素透明度
        /// </summary>
        public Transparency VTransparency;
        /// <summary>
        /// 体素碰撞器类型，分为：无、网格、立方体。
        /// 网格碰撞器可识别自定义模型中的网格（高复杂度情况下较吃性能），而预制的立方体碰撞器则网格面数被简化为最优
        /// 无论上述哪种，在Unity既可凭空从顶点三角形创建，也可修改编辑已有网格
        /// </summary>
        public ColliderType VColliderType;
        /// <summary>
        /// 体素子网格索引，它是GUI界面输入的自定义材质索引（Material Index）。当团块预制体只有额外材质计数+1个材质附着时，需设置一个更低的材质索引或附着更多材质到团块预制体否则报错！
        /// </summary>
        public int VSubmeshIndex;
        /// <summary>
        /// 体素旋转
        /// </summary>
        public MeshRotation VRotation;

        /// <summary>
        /// 摧毁体素块，实际是将单元设置为空(id 0)，更新单元的网格并触发OnBlockDestroy事件。
        /// </summary>
        /// <param name="cellInfo"></param>
        public static void DestroyBlock(CellInfo cellInfo)
        {
            // multiplayer - send change to server
            if (CPEngine.EnableMultiplayer)
            {
                //从网络处理对象（CPEngine）挂着的Client组件中调用SendPlaceBlock方法
                CPEngine.Network.GetComponent<Client>().SendPlaceBlock(cellInfo, 0);
            }
            // single player - apply change locally
            else
            {
                //从CellInfo取得体素ID，然后从Block[体素ID]取得预制体并进行一次实例化
                //↓只是为了提取组件验证并发送事件，最后用完就摧毁非常不效率，这里的GameObject是一个需要优化的地方
                //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(cellInfo.GetCellID()));
                OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //从栈取出OP对象
                CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //将取出的OP对象激活（里面游戏物体也会激活）
                GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //游戏物体赋值
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    //如果该ID对象具有CellEvents组件，使用该组件方法对单元执行一次摧毁事件
                    cellObject.GetComponent<CellEvents>().OnBlockDestroy(cellInfo);
                }
                //设置该单元索引处的单位种类为0
                cellInfo.chunk.SetCell(cellInfo.index, 0, true);
                //摧毁实例（也没有回收到对象池，这样写很不效率）
                //Destroy(cellObject);
                OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //用完退回对象池
            }
        }
        /// <summary>
        /// 放置体素块，实际是将单元设置为指定的id，更新单元的网格并触发OnBlockPlace事件。
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        public static void PlaceBlock(CellInfo cellInfo, ushort data)
        {
            // multiplayer - send change to server
            if (CPEngine.EnableMultiplayer)
            {
                //从网络处理对象（CPEngine）挂着的Client组件中调用SendPlaceBlock方法
                CPEngine.Network.GetComponent<Client>().SendPlaceBlock(cellInfo, data);
            }
            // single player - apply change locally
            else
            {
                cellInfo.chunk.SetCell(cellInfo.index, data, true);
                //↓只是为了提取组件验证并发送事件，最后用完就摧毁非常不效率，这里的GameObject是一个需要优化的地方
                //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
                OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //从栈取出OP对象
                CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //将取出的OP对象激活（里面游戏物体也会激活）
                GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //游戏物体赋值
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    cellObject.GetComponent<CellEvents>().OnBlockPlace(cellInfo);
                }
                //Destroy(cellObject);
                OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //用完退回对象池
            }
        }
        /// <summary>
        /// 更换体素块，实际是将单元设置为指定的id，更新单元的网格并触发OnBlockChange事件。
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        public static void ChangeBlock(CellInfo cellInfo, ushort data)
        {
            // multiplayer - send change to server
            if (CPEngine.EnableMultiplayer)
            {
                //从网络处理对象（CPEngine）挂着的Client组件中调用SendChangeBlock方法
                CPEngine.Network.GetComponent<Client>().SendChangeBlock(cellInfo, data);
            }
            // single player - apply change locally
            else
            {
                cellInfo.chunk.SetCell(cellInfo.index, data, true);
                //↓只是为了提取组件验证并发送事件，最后用完就摧毁非常不效率，这里的GameObject是一个需要优化的地方
                //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
                OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //从栈取出OP对象
                CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //将取出的OP对象激活（里面游戏物体也会激活）
                GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //游戏物体赋值
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    cellObject.GetComponent<CellEvents>().OnBlockChange(cellInfo);
                }
                //Destroy(cellObject);
                OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //用完退回对象池
            }
        }

        // multiplayer

        /// <summary>
        /// 摧毁体素块，将单元设置为空单元(id 0)，更新单元的网格并触发OnBlockDestroy事件。如果启用多人模式，将单元更改发送给其他连接的玩家。
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="sender"></param>
        public static void DestroyBlockMultiplayer(CellInfo cellInfo, NetworkPlayer sender)
        { // received from server, don'transform use directly

            //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(cellInfo.GetCellID()));
            OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //从栈取出OP对象
            CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //将取出的OP对象激活（里面游戏物体也会激活）
            GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //游戏物体赋值
            CellEvents events = cellObject.GetComponent<CellEvents>();
            if (events != null)
            {
                events.OnBlockDestroy(cellInfo);
                events.OnBlockDestroyMultiplayer(cellInfo, sender);
            }
            cellInfo.chunk.SetCell(cellInfo.index, 0, true);
            //Destroy(cellObject);
            OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //用完退回对象池
        }
        /// <summary>
        /// 放置体素块，将单元设置为指定的id，更新单元的网格并触发OnBlockPlace事件。如果启用多人模式，将单元更改发送给其他连接的玩家。
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        public static void PlaceBlockMultiplayer(CellInfo cellInfo, ushort data, NetworkPlayer sender)
        { // received from server, don'transform use directly

            cellInfo.chunk.SetCell(cellInfo.index, data, true);
            //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
            OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //从栈取出OP对象
            CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //将取出的OP对象激活（里面游戏物体也会激活）
            GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //游戏物体赋值
            CellEvents events = cellObject.GetComponent<CellEvents>();
            if (events != null)
            {
                events.OnBlockPlace(cellInfo);
                events.OnBlockPlaceMultiplayer(cellInfo, sender);
            }
            //Destroy(cellObject);
            OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //用完退回对象池
        }
        /// <summary>
        /// 更换体素块，将单元设置为指定的id，更新单元的网格并触发OnBlockChange事件。如果启用多人模式，将单元更改发送给其他连接的玩家。
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        public static void ChangeBlockMultiplayer(CellInfo cellInfo, ushort data, NetworkPlayer sender)
        { // received from server, don'transform use directly

            cellInfo.chunk.SetCell(cellInfo.index, data, true);
            //GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(data));
            OP.Pop(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //从栈取出OP对象
            CPEngine.PrefabOPs[cellInfo.GetCellID()].Enable(); //将取出的OP对象激活（里面游戏物体也会激活）
            GameObject cellObject = CPEngine.PrefabOPs[cellInfo.GetCellID()].gameObject; //游戏物体赋值
            CellEvents events = cellObject.GetComponent<CellEvents>();
            if (events != null)
            {
                events.OnBlockChange(cellInfo);
                events.OnBlockChangeMultiplayer(cellInfo, sender);
            }
            //Destroy(cellObject);
            OP.Push(ref CPEngine.PrefabOPs[cellInfo.GetCellID()]); //用完退回对象池
        }

        // block editor functions

        /// <summary>
        /// 获取体素ID（单元种类）
        /// </summary>
        /// <returns></returns>
        public ushort GetID()
        {
            return ushort.Parse(this.gameObject.name.Split('_')[1]);
        }
        /// <summary>
        /// 设定体素ID（单元种类）
        /// </summary>
        /// <param name="id"></param>
        public void SetID(ushort id)
        {
            this.gameObject.name = "cell_" + id.ToString();
        }
    }
}
