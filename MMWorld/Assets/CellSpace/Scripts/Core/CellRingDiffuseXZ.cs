using System.Collections.Generic;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 填充圆形扩散的格子偏移量数组，主用于更高效的2D范围内找最近网格容器（单元），生成一系列均匀分布在多个圆周上的点，并将这些点的坐标及每个圆周上的点数量存储起来。
    /// </summary>
    public class CellRingDiffuseXZ
    {
        public List<CellCountRadiusInfo> lens = new();
        public List<CellXZInfo> idxzs = new();

        /// <summary>
        /// 填充圆形扩散的格子偏移量数组，主用于更高效的2D范围内找最近网格容器（单元），生成一系列均匀分布在多个圆周上的点，并将这些点的坐标及每个圆周上的点数量存储起来。
        /// </summary>
        /// <param name="gridNumRows">网格行数</param>
        /// <param name="cellSize">单元大小</param>
        public CellRingDiffuseXZ(int gridNumRows, int cellSize)
        {
            lens.Add(new CellCountRadiusInfo { count = 0, radius = 0f });
            idxzs.Add(new CellXZInfo());
            HashSet<ulong> set = new();
            set.Add(0);
            for (float radius = 0; radius < cellSize * gridNumRows; radius += cellSize)
            {
                var lenBak = idxzs.Count;
                var radians = Mathf.Asin(0.5f / radius) * 2;
                var step = (int)(Mathf.PI * 2 / radians);
                var inc = Mathf.PI * 2 / step;
                for (int i = 0; i < step; ++i)
                {
                    var a = inc * i;
                    var cos = Mathf.Cos(a);
                    var sin = Mathf.Sin(a);
                    var ix = (int)(cos * radius) / cellSize;
                    var iz = (int)(sin * radius) / cellSize;
                    var kez = ((ulong)iz << 32) + (ulong)ix;
                    if (set.Add(kez))
                    {
                        idxzs.Add(new CellXZInfo { x = ix, z = iz });
                    }
                }
                if (idxzs.Count > lenBak)
                {
                    lens.Add(new CellCountRadiusInfo { count = idxzs.Count, radius = radius });
                }
            }
        }
    }
}
