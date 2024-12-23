using System.Collections.Generic;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 填充圆形扩散的格子偏移量数组，主用于更高效的2D范围内找最近网格容器（单元），生成一系列均匀分布在多个圆周上的点，并将这些点的坐标及每个圆周上的点数量存储起来。
    /// </summary>
    public class CellRingDiffuseXY
    {
        public List<CellCountRadiusInfo> lens = new();
        public List<CellXYInfo> idxys = new();

        /// <summary>
        /// 填充圆形扩散的格子偏移量数组，主用于更高效的2D范围内找最近网格容器（单元），生成一系列均匀分布在多个圆周上的点，并将这些点的坐标及每个圆周上的点数量存储起来。
        /// </summary>
        /// <param name="gridNumRows">网格行数</param>
        /// <param name="cellSize">单元大小</param>
        public CellRingDiffuseXY(int gridNumRows, int cellSize)
        {
            lens.Add(new CellCountRadiusInfo { count = 0, radius = 0f });
            idxys.Add(new CellXYInfo());
            HashSet<ulong> set = new();
            set.Add(0);
            for (float radius = 0; radius < cellSize * gridNumRows; radius += cellSize)
            {
                var lenBak = idxys.Count;
                var radians = Mathf.Asin(0.5f / radius) * 2;
                var step = (int)(Mathf.PI * 2 / radians);
                var inc = Mathf.PI * 2 / step;
                for (int i = 0; i < step; ++i)
                {
                    var a = inc * i;
                    var cos = Mathf.Cos(a);
                    var sin = Mathf.Sin(a);
                    var ix = (int)(cos * radius) / cellSize;
                    var iy = (int)(sin * radius) / cellSize;
                    var key = ((ulong)iy << 32) + (ulong)ix;
                    if (set.Add(key))
                    {
                        idxys.Add(new CellXYInfo { x = ix, y = iy });
                    }
                }
                if (idxys.Count > lenBak)
                {
                    lens.Add(new CellCountRadiusInfo { count = idxys.Count, radius = radius });
                }
            }
            //‌循环计算‌：
            //外层循环通过 radius 从 0 开始，每次增加 gridSize，直到 gridSize * gridChunkNumRows。这表示在不同的半径上进行迭代。
            //在每个半径上，计算一个环上的点，这些点均匀分布在一个圆周上。radians 计算了环上相邻点之间的角度间隔（基于半径和某个固定的比例，这里是 0.5f / radius 的反正弦值的两倍）。
            //step 是圆周上点的数量，inc 是每个点之间的角度增量。
            //内层循环通过角度 a 计算圆周上每个点的 cos 和 sin 值，进而得到网格坐标 ix 和 iy。
            //使用 key（由 ix 和 iy 组成的唯一标识符）来检查该点是否已经存在于 set 中。如果不存在，则添加到 idxys 中，并且更新 set。
            //‌更新 lens‌：
            //如果 idxys 的数量在当前半径迭代后有所增加，更新 lens 列表，添加新的 CellCountRadiusInfo 对象，记录当前点的数量和半径。
        }
    }
}
