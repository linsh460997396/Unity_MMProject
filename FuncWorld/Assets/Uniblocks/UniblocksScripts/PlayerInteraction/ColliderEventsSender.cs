using UnityEngine;

namespace Uniblocks
{

    public class ColliderEventsSender : MonoBehaviour
    {

        private Index LastIndex;
        private Chunk LastChunk;

        public void Update()
        {

            // check if chunk is not null
            GameObject chunkObject = Engine.PositionToChunk(transform.position);
            if (chunkObject == null) return;

            // get the voxelInfo from the transform's position
            Chunk chunk = chunkObject.GetComponent<Chunk>();
            Index voxelIndex = chunk.PositionToVoxelIndex(transform.position);
            VoxelInfo voxelInfo = new VoxelInfo(voxelIndex, chunk);
            //print(voxelInfo.GetVoxel());

            // create a local copy of the collision voxel so we can call functions on it.创建一个碰撞体素的本地副本，这样我们就可以在其上调用函数
            GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(voxelInfo.GetVoxel())) as GameObject;

            VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
            if (events != null)
            {

                // OnEnter
                if (chunk != LastChunk || voxelIndex.IsEqual(LastIndex) == false)
                {
                    events.OnBlockEnter(this.gameObject, voxelInfo);
                }

                // OnStay
                else
                {
                    events.OnBlockStay(this.gameObject, voxelInfo);
                }
            }

            LastChunk = chunk;
            LastIndex = voxelIndex;

            Destroy(voxelObject);

        }

    }

}

























