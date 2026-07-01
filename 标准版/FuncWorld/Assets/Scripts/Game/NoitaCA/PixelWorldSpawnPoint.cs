using System;
using UnityEngine;

namespace NoitaCA
{
    public enum PixelWorldSpawnCategory
    {
        Monster,
        Item,
        Treasure,
        Hazard,
        Ambient
    }

    [Serializable]
    public struct PixelWorldSpawnPoint
    {
        public PixelWorldSpawnCategory Category;
        public Vector2Int Cell;
        public Vector3 WorldPosition;
        public int SegmentIndex;
        public float Weight;

        public PixelWorldSpawnPoint(PixelWorldSpawnCategory category, Vector2Int cell, Vector3 worldPosition, int segmentIndex, float weight)
        {
            Category = category;
            Cell = cell;
            WorldPosition = worldPosition;
            SegmentIndex = segmentIndex;
            Weight = Mathf.Max(0f, weight);
        }
    }
}
