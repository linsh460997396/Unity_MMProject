using UnityEngine;
using System.Collections;
using Uniblocks;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// 地图管理器（控制地图生成，本类是自定义测试用的，非Uniblocks插件的核心组件，效果等同ChunkLoader.cs）
/// </summary>
public class MapManager : MonoBehaviour {

    //private bool isGenerate = false;
    private Transform playerTransform;
    //private Vector3F lastPosition;
    private Index LastPos;
    private Index currentPos;

    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform; //找到Player的转换组件，届时在玩家周围刷地图
        //InvokeRepeating("InitMap", 0.00625f, 0.00625f); //开启一个自定间隔的线程来运行刷地图函数
    }

    private void Update() 
    {
        InitMap();
    }    
    
    private void InitMap()
    {
        //don't load chunks if engine isn't initialized yet.刷地图前保证前置核心组件已经初始化完成
        if (!Engine.Initialized || !ChunkManager.Initialized)
        {
            return;
        }

        //don't load chunks if multiplayer is enabled but the connection isn't established yet
        if (Engine.EnableMultiplayer)
        {
            if (!Network.isClient && !Network.isServer)
            {
                return;
            }
        }

        //track which chunk we're currently in. If it's different from previous frame, spawn chunks at current position.跟踪我们当前所在的团块，如果它与前一帧不同，则在当前位置生成团块
        currentPos = Engine.PositionToChunkIndex(transform.position);

        if (currentPos.IsEqual(LastPos) == false)
        {
            ChunkManager.SpawnChunks(currentPos.x, currentPos.y, currentPos.z);

            // (Multiplayer) update server position.多人游戏模式下更新玩家位置
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
            yield return new WaitForEndOfFrame();
        }
        UniblocksClient.UpdatePlayerPosition(currentPos);
        UniblocksClient.UpdatePlayerRange(Engine.ChunkSpawnDistance);
    }
}
