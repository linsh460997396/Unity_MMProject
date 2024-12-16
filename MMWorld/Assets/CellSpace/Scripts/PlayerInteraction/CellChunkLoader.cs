using UnityEngine;
using System.Collections;

// Triggers chunk spawning around the player.在玩家角色周围触发自动化的团块生成

namespace CellSpace
{
    /// <summary>
    /// 团块加载器，在玩家角色周围触发自动化的团块生成。
    /// 组件用法：把脚本拖到控制对象（玩家角色）的组件位置即挂载（Unity要求一个cs文件只能一个类，且类名须与文件名一致），地形会在其周围产生且随角色移动实时刷新。
    /// </summary>
    public class CellChunkLoader : MonoBehaviour
    {

        private CPIndex LastPos;
        private CPIndex currentPos;

        void Awake()
        {

        }

        public void Update()
        {
            // don'transform load chunks if engine isn'transform initialized yet
            if (!CPEngine.Initialized || !CellChunkManager.Initialized)
            {
                return;
            }
            // don'transform load chunks if multiplayer is enabled but the connection isn'transform established yet
            if (CPEngine.EnableMultiplayer)
            {
                if (!Network.isClient && !Network.isServer)
                {
                    return;
                }
            }
            // track which chunk we're currently in. If it's different from previous frame, spawn chunks at current position.
            // 跟踪我们当前所在的团块，如果它与前一帧不同，则在当前位置生成团块
            currentPos = CPEngine.PositionToChunkIndex(transform.position);
            if (currentPos.IsEqual(LastPos) == false)
            {
                if (CPEngine.HorizontalMode)
                {
                    CellChunkManager.SpawnChunks(currentPos.x, currentPos.y);
                }
                else
                {
                    CellChunkManager.SpawnChunks(currentPos.x, currentPos.y, currentPos.z);
                }
                
                // (Multiplayer) update server position
                if (CPEngine.EnableMultiplayer && CPEngine.MultiplayerTrackPosition && CPEngine.Network != null)
                {
                    Client.UpdatePlayerPosition(currentPos);
                }
            }
            LastPos = currentPos;
        }

        // multiplayer
        public void OnConnectedToServer()
        {
            if (CPEngine.EnableMultiplayer && CPEngine.MultiplayerTrackPosition)
            {
                StartCoroutine(InitialPositionAndRangeUpdate());
            }
        }

        IEnumerator InitialPositionAndRangeUpdate()
        {
            while (CPEngine.Network == null)
            {
                yield return new WaitForEndOfFrame();
            }
            Client.UpdatePlayerPosition(currentPos);
            Client.UpdatePlayerRange(CPEngine.ChunkSpawnDistance);
        }
    }

}