using System.Collections.Generic;
using UnityEngine;

namespace MMWorld.HexSphere
{
    [System.Serializable]
    public class BaseTerrainGenerator
    {
        public virtual HexTile CreateHexTile(int id, HexPlanet planet, Vector3 centerPosition, List<Vector3> verts)
        {
            return new HexTile(id, planet, centerPosition, verts);
        }

        public virtual void AfterTileCreation(HexTile newTile) { }
    }
}