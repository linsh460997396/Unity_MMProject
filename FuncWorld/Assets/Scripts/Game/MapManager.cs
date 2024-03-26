using UnityEngine;
using Uniblocks;

public class MapManager : MonoBehaviour {

    //private bool isGenerate = false;
    private Transform playerTrans;
    //private Vector3F lastPosition;
    private Index lastIndex;

    private void Start()
    {
        playerTrans = GameObject.Find("Player").transform;
        InvokeRepeating("InitMap", 0.0625f, 0.0625f);
    }

    
    private void InitMap()
    {
        if (Engine.Initialized == false || ChunkManager.Initialized == false) return;

        Index currentIndex = Engine.PositionToChunkIndex(playerTrans.position);

        if (lastIndex != currentIndex )
        {
            ChunkManager.SpawnChunks(playerTrans.position);
            lastIndex = currentIndex;
        }

    }
}
