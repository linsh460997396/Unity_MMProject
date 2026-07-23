using System.Collections.Generic;
using UnityEngine;

namespace MMWorld.HexSphere
{
    [System.Serializable]
    public class RandomTerrainGenerator : BaseTerrainGenerator
    {
        public float minHeight;
        public float maxHeight;

        public List<Color32> colors;

        public override void AfterTileCreation(HexTile newTile)
        {
            newTile.height = Random.Range(minHeight, maxHeight);
            newTile.color = colors[(int)Mathf.Floor(Random.Range(0, colors.Count))];
        }
    }
}