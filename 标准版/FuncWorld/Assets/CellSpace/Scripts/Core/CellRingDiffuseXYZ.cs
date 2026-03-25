using System.Collections.Generic;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 填充球形扩散的格子偏移量数组,主用于更高效的3D范围内找最近网格容器(单元体),生成一系列均匀分布在多个球面上的点,并将这些点的坐标及每个球面上的点数量存储起来.
    /// </summary>
    public class CellRingDiffuseXYZ
    {
        public List<CellCountRadiusInfo> lens = new();
        public List<CellXYZInfo> idxyzs = new();

        /// <summary>
        /// 填充球形扩散的格子偏移量数组，用于3D范围内找最近网格容器
        /// 生成一系列均匀分布在多个球面上的点，并将这些点的坐标及每个球面上的点数量存储起来
        /// </summary>
        /// <param name="gridNum">网格行或列数</param>
        /// <param name="cellSize">单元大小</param>
        public CellRingDiffuseXYZ(int gridNum, float cellSize)
        {
            // 添加中心点 (0,0,0)
            lens.Add(new CellCountRadiusInfo { count = 0, radius = 0f });
            idxyzs.Add(new CellXYZInfo());

            HashSet<ulong> set = new();
            set.Add(0); // 中心点的唯一标识

            for (float radius = 0; radius < cellSize * gridNum; radius += cellSize)
            {
                var lenBak = idxyzs.Count;

                if (radius <= 0)
                {
                    continue; // 跳过半径为0的情况（中心点已添加）
                }

                // 计算球面上的采样点数量（基于立体角）
                var solidAngleStep = CalculateSolidAngleStep(radius, cellSize);
                var thetaSteps = (int)(Mathf.PI / solidAngleStep);
                var phiSteps = (int)(2 * Mathf.PI / solidAngleStep);

                for (int thetaIdx = 0; thetaIdx < thetaSteps; ++thetaIdx)
                {
                    var theta = Mathf.PI * thetaIdx / thetaSteps; // [0, π]

                    for (int phiIdx = 0; phiIdx < phiSteps; ++phiIdx)
                    {
                        var phi = 2 * Mathf.PI * phiIdx / phiSteps; // [0, 2π]

                        // 球坐标转笛卡尔坐标
                        var sinTheta = Mathf.Sin(theta);
                        var cosTheta = Mathf.Cos(theta);
                        var sinPhi = Mathf.Sin(phi);
                        var cosPhi = Mathf.Cos(phi);

                        var x = (int)(sinTheta * cosPhi * radius / cellSize);
                        var y = (int)(sinTheta * sinPhi * radius / cellSize);
                        var z = (int)(cosTheta * radius / cellSize);

                        // 生成唯一标识（使用48位存储三个16位坐标）
                        var key = Generate3DKey(x, y, z);

                        if (set.Add(key))
                        {
                            idxyzs.Add(new CellXYZInfo { x = x, y = y, z = z });
                        }
                    }
                }

                // 如果本轮添加了新点，记录该球面的信息
                if (idxyzs.Count > lenBak)
                {
                    lens.Add(new CellCountRadiusInfo { count = idxyzs.Count, radius = radius });
                }
            }
        }

        /// <summary>
        /// 计算立体角步长，用于确定球面上的采样密度
        /// </summary>
        private float CalculateSolidAngleStep(float radius, float cellSize)
        {
            // 基于网格大小和半径计算合适的立体角步长
            if (radius <= cellSize)
            {
                return Mathf.PI / 4; // 小半径时使用较稀疏的采样
            }

            // 目标：每个采样点代表的面积约等于cellSize²
            var targetArea = cellSize * cellSize;
            var sphereArea = 4 * Mathf.PI * radius * radius;
            var approximateSteps = sphereArea / targetArea;

            return Mathf.Sqrt(4 * Mathf.PI / approximateSteps);
        }

        /// <summary>
        /// 生成3D坐标的唯一标识（使用48位：每个坐标16位）
        /// </summary>
        private ulong Generate3DKey(int x, int y, int z)
        {
            // 将每个坐标映射到16位无符号整数范围
            uint ux = (uint)(x + 32768); // 映射到 [0, 65535]
            uint uy = (uint)(y + 32768);
            uint uz = (uint)(z + 32768);

            return ((ulong)ux << 32) | ((ulong)uy << 16) | uz;
        }
    }
}


