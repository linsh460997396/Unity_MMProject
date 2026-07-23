using System.Collections.Generic;
using UnityEngine;

namespace MMWorld.HexSphere
{
    /// <summary>
    /// 基于 3D Perlin 噪声的地形生成器（纯代码复刻 HexSphere 的 PerlinTerrainGenerator）。
    /// 去掉了 ScriptableObject / CreateAssetMenu 依赖，改用普通 [Serializable] 类。
    /// </summary>
    [System.Serializable]
    public class PerlinTerrainGenerator : BaseTerrainGenerator
    {
        [System.Serializable]
        public class ColorHeight
        {
            public Color32 color;
            public float maxHeight;
        }

        [Range(1, 8)]
        public int octaves = 4;

        [Range(0, 1)]
        public float persistence = 0.5f;

        [Range(1, 10)]
        public float lacunarity = 2f;

        public float minHeight = 0f;
        public float maxHeight = 5f;
        public float noiseScaling = 1.5f;

        public List<ColorHeight> colorHeights = new List<ColorHeight>();

        public override void AfterTileCreation(HexTile newTile)
        {
            Vector3 n = newTile.center.normalized;
            float height = Mathf.Floor(
                3 * (((maxHeight - minHeight) * GetNoise(n.x, n.y, n.z)) + minHeight)
            ) / 3.0f;
            newTile.height = height;

            for (int i = colorHeights.Count - 1; i >= 0; i--)
            {
                if (height < colorHeights[i].maxHeight)
                {
                    newTile.color = colorHeights[i].color;
                }
            }
        }

        private float GetNoise(float x, float y, float z)
        {
            float value = 0f;
            float scale = noiseScaling;
            float effect = 1f;
            for (int i = 0; i < octaves; i++)
            {
                value += effect * PerlinNoise3D(scale * x, scale * y, scale * z);
                scale *= lacunarity;
                effect *= (1f - persistence);
            }
            return value;
        }

        private static float PerlinNoise3D(float x, float y, float z)
        {
            x += 15f;
            y += 25f;
            z += 35f;
            float xy = Mathf.PerlinNoise(x, y);
            float xz = Mathf.PerlinNoise(x, z);
            float yz = Mathf.PerlinNoise(y, z);
            float yx = Mathf.PerlinNoise(y, x);
            float zx = Mathf.PerlinNoise(z, x);
            float zy = Mathf.PerlinNoise(z, y);
            return (xy + xz + yz + yx + zx + zy) / 6f;
        }
    }
}
