using UnityEngine;
using System.Collections;
using MetalMaxSystem.Unity;

// Triggers chunk spawning around the player.在玩家角色周围触发自动化的团块生成

namespace Uniblocks
{
    /// <summary>
    /// 团块加载器,在玩家角色周围触发自动化的团块生成.
    /// 组件用法:把脚本拖到控制对象(玩家角色)的组件位置即挂载(Unity要求一个cs文件只能一个类,且类名须与文件名一致),地形会在其周围产生且随角色移动实时刷新.
    /// </summary>
    public class ChunkLoader : MonoBehaviour
    {

        private Index LastPos;
        private Index currentPos;

        void Awake()
        {

        }


        public void Update()
        {

            // don'transform load chunks if engine isn'transform initialized yet
            if (!Engine.Initialized || !ChunkManager.Initialized)
            {
                return;
            }

            // don'transform load chunks if multiplayer is enabled but the connection isn'transform established yet
            if (Engine.EnableMultiplayer)
            {
                if (!Network.isClient && !Network.isServer)
                {
                    return;
                }
            }



            // track which chunk we're currently in. If it's different from previous frame, spawn chunks at current position.跟踪我们当前所在的团块,若它与前一帧不同,则在当前位置生成团块

            currentPos = Engine.PositionToChunkIndex(transform.position);

            if (currentPos.IsEqual(LastPos) == false)
            {
                ChunkManager.SpawnChunks(currentPos.x, currentPos.y, currentPos.z);

                // (Multiplayer) update server position
                if (Engine.EnableMultiplayer && Engine.MultiplayerTrackPosition && Engine.UniblocksNetwork != null)
                {
                    UniblocksClient.UpdatePlayerPosition(currentPos);
                }
            }

            LastPos = currentPos;

        }

        // multiplayer
        public void OnConnectedToServer()
        {
            if (Engine.EnableMultiplayer && Engine.MultiplayerTrackPosition)
            {
                StartCoroutine(InitialPositionAndRangeUpdate());
            }
        }

        IEnumerator InitialPositionAndRangeUpdate()
        {
            while (Engine.UniblocksNetwork == null)
            {
                yield return UnityUtilities.waitForEndOfFrame;
            }
            UniblocksClient.UpdatePlayerPosition(currentPos);
            UniblocksClient.UpdatePlayerRange(Engine.ChunkSpawnDistance);
        }
    }

}