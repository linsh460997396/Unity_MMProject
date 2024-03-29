using UnityEngine;

namespace Uniblocks
{
    /// <summary>
    /// 体素类型（存储每个体素的属性并提供直接访问特定体素的功能），与VoxelInfo区别在于少了体素索引（位置信息）。
    /// </summary>
    public class Voxel : MonoBehaviour
    {

        public string VName;
        public Mesh VMesh;
        public bool VCustomMesh;
        public bool VCustomSides;
        public Vector2[] VTexture; // index of the texture. Array index specifies face (VTexture[0] is the up-facing texture, for example)
        public Transparency VTransparency;
        public ColliderType VColliderType;
        public int VSubmeshIndex;
        public MeshRotation VRotation;

        /// <summary>
        /// 将体素设置为空块(id 0)，更新块的网格并触发OnBlockDestroy事件。
        /// </summary>
        /// <param name="voxelInfo"></param>
        public static void DestroyBlock(VoxelInfo voxelInfo)
        {

            // multiplayer - send change to server
            if (Engine.EnableMultiplayer)
            {
                Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendPlaceBlock(voxelInfo, 0);
            }

            // single player - apply change locally
            else
            {
                GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(voxelInfo.GetVoxel())) as GameObject;
                if (voxelObject.GetComponent<VoxelEvents>() != null)
                {
                    voxelObject.GetComponent<VoxelEvents>().OnBlockDestroy(voxelInfo);
                }
                voxelInfo.chunk.SetVoxel(voxelInfo.index, 0, true);
                Destroy(voxelObject);
            }
        }
        /// <summary>
        /// 将体素设置为指定的id，更新块的网格并触发OnBlockPlace事件。
        /// </summary>
        /// <param name="voxelInfo"></param>
        /// <param name="data"></param>
        public static void PlaceBlock(VoxelInfo voxelInfo, ushort data)
        {

            // multiplayer - send change to server
            if (Engine.EnableMultiplayer)
            {
                Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendPlaceBlock(voxelInfo, data);
            }

            // single player - apply change locally
            else
            {
                voxelInfo.chunk.SetVoxel(voxelInfo.index, data, true);

                GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
                if (voxelObject.GetComponent<VoxelEvents>() != null)
                {
                    voxelObject.GetComponent<VoxelEvents>().OnBlockPlace(voxelInfo);
                }
                Destroy(voxelObject);
            }
        }
        /// <summary>
        /// 将体素设置为指定的id，更新块的网格并触发OnBlockChange事件。
        /// </summary>
        /// <param name="voxelInfo"></param>
        /// <param name="data"></param>
        public static void ChangeBlock(VoxelInfo voxelInfo, ushort data)
        {

            // multiplayer - send change to server
            if (Engine.EnableMultiplayer)
            {
                Engine.UniblocksNetwork.GetComponent<UniblocksClient>().SendChangeBlock(voxelInfo, data);
            }

            // single player - apply change locally
            else
            {
                voxelInfo.chunk.SetVoxel(voxelInfo.index, data, true);

                GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
                if (voxelObject.GetComponent<VoxelEvents>() != null)
                {
                    voxelObject.GetComponent<VoxelEvents>().OnBlockChange(voxelInfo);
                }
                Destroy(voxelObject);
            }
        }

        // multiplayer

        /// <summary>
        /// 将体素设置为空块(id 0)，更新块的网格并触发OnBlockDestroy事件。如果启用多人模式，将体素更改发送给其他连接的玩家。
        /// </summary>
        /// <param name="voxelInfo"></param>
        /// <param name="sender"></param>
        public static void DestroyBlockMultiplayer(VoxelInfo voxelInfo, NetworkPlayer sender)
        { // received from server, don't use directly

            GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(voxelInfo.GetVoxel())) as GameObject;
            VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
            if (events != null)
            {
                events.OnBlockDestroy(voxelInfo);
                events.OnBlockDestroyMultiplayer(voxelInfo, sender);
            }
            voxelInfo.chunk.SetVoxel(voxelInfo.index, 0, true);
            Destroy(voxelObject);
        }
        /// <summary>
        /// 将体素设置为指定的id，更新块的网格并触发OnBlockPlace事件。如果启用多人模式，将体素更改发送给其他连接的玩家。
        /// </summary>
        /// <param name="voxelInfo"></param>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        public static void PlaceBlockMultiplayer(VoxelInfo voxelInfo, ushort data, NetworkPlayer sender)
        { // received from server, don't use directly

            voxelInfo.chunk.SetVoxel(voxelInfo.index, data, true);

            GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
            VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
            if (events != null)
            {
                events.OnBlockPlace(voxelInfo);
                events.OnBlockPlaceMultiplayer(voxelInfo, sender);
            }
            Destroy(voxelObject);
        }
        /// <summary>
        /// 将体素设置为指定的id，更新块的网格并触发OnBlockChange事件。如果启用多人模式，将体素更改发送给其他连接的玩家。
        /// </summary>
        /// <param name="voxelInfo"></param>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        public static void ChangeBlockMultiplayer(VoxelInfo voxelInfo, ushort data, NetworkPlayer sender)
        { // received from server, don't use directly

            voxelInfo.chunk.SetVoxel(voxelInfo.index, data, true);

            GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(data)) as GameObject;
            VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
            if (events != null)
            {
                events.OnBlockChange(voxelInfo);
                events.OnBlockChangeMultiplayer(voxelInfo, sender);
            }
            Destroy(voxelObject);
        }


        // block editor functions

        /// <summary>
        /// 获取体素ID（体素块预制体种类）
        /// </summary>
        /// <returns></returns>
        public ushort GetID()
        {
            return ushort.Parse(this.gameObject.name.Split('_')[1]);

        }
        /// <summary>
        /// 设定体素ID（体素块预制体种类）
        /// </summary>
        /// <param name="id"></param>
        public void SetID(ushort id)
        {
            this.gameObject.name = "block_" + id.ToString();
        }

    }

}