using UnityEngine;
using System.Collections;

// Triggers chunk spawning around the player.����ҽ�ɫ��Χ�����Զ������ſ�����

namespace CellSpace
{
    /// <summary>
    /// �ſ������������ҽ�ɫ��Χ�����Զ������ſ����ɡ�
    /// ����÷����ѽű��ϵ����ƶ�����ҽ�ɫ�������λ�ü����أ�UnityҪ��һ��cs�ļ�ֻ��һ���࣬�����������ļ���һ�£������λ�������Χ���������ɫ�ƶ�ʵʱˢ�¡�
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
            // �������ǵ�ǰ���ڵ��ſ飬�������ǰһ֡��ͬ�����ڵ�ǰλ�������ſ�
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