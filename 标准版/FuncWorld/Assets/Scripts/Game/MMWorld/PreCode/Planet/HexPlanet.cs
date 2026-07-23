using System.Collections.Generic;
using UnityEngine;

namespace MMWorld.HexSphere
{
    [System.Serializable]
    public class HexPlanet : ISerializationCallbackReceiver
    {
        public float radius;
        [Range(0, 7)]
        public int subdivisions;
        [Range(0, 6)]
        public int chunkSubdivisions;

        public Material chunkMaterial;

        public BaseTerrainGenerator terrainGenerator;

        [HideInInspector]
        public List<HexTile> tiles;
        [HideInInspector]
        public List<HexChunk> chunks;

        public HexPlanet()
        {
            tiles = new List<HexTile>();
            chunks = new List<HexChunk>();
        }

        public HexTile GetTile(int id)
        {
            return tiles[id];
        }

        public HexChunk GetChunk(int id)
        {
            return chunks[id];
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            foreach (HexTile tile in tiles)
            {
                tile.planet = this;
            }
            foreach (HexChunk chunk in chunks)
            {
                chunk.planet = this;
                chunk.SetupEvents();
            }
        }
    }
}