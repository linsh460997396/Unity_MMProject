// 职责：处理像素间的非空间交互，包括寿命衰减、热量传播、点燃和颜色变化。
// Responsibility: Handles non-spatial pixel interactions, including lifetime decay, heat transfer, ignition, and color changes.
using UnityEngine;

namespace NoitaCA
{
    public sealed class InteractionSystem
    {
        // 八邻域用于热量传播和点燃检测。
        // Eight-neighbor offsets are used for heat transfer and ignition checks.
        private static readonly Vector2Int[] NeighborOffsets =
        {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0),                         new Vector2Int(1, 0),
            new Vector2Int(-1, 1),  new Vector2Int(0, 1),  new Vector2Int(1, 1)
        };

        private readonly System.Random random;

        public InteractionSystem(System.Random random)
        {
            // 与移动系统共享随机源时，整场模拟可通过同一 seed 复现。
            // Sharing the random source with movement makes the whole simulation reproducible by seed.
            this.random = random ?? new System.Random();
        }

        public void Step(PixelGrid grid, PixelSimulationMode mode, int maxPixelsPerStep)
        {
            if (grid == null)
            {
                return;
            }

            if (mode == PixelSimulationMode.ActivePixels)
            {
                // 活跃像素模式减少静止区域的交互检查。
                // Active-pixel mode reduces interaction checks in quiet regions.
                StepActivePixels(grid, maxPixelsPerStep);
            }
            else if (mode == PixelSimulationMode.ChunkBased)
            {
                // 区块模式只扫描活跃脏区块内的交互。
                // Chunk mode scans interactions only inside active dirty chunks.
                StepActiveChunks(grid, maxPixelsPerStep);
            }
            else
            {
                StepFullScan(grid, maxPixelsPerStep);
            }
        }

        private void StepFullScan(PixelGrid grid, int maxPixelsPerStep)
        {
            // 全图交互扫描：每一步检查每个像素，适合做基准对照。
            // Naive cellular automata interaction pass: every pixel is checked every step.
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (!StepCell(grid, x, y, maxPixelsPerStep))
                    {
                        return;
                    }
                }
            }
        }

        private void StepActivePixels(PixelGrid grid, int maxPixelsPerStep)
        {
            // 活跃像素优化：休眠像素同样跳过交互检查。
            // Active Pixel optimization: interaction is also skipped for sleeping pixels.
            int count = grid.ActivePixelCount;
            for (int i = 0; i < count; i++)
            {
                Vector2Int cell = grid.GetActiveCell(i);
                if (!StepCell(grid, cell.x, cell.y, maxPixelsPerStep))
                {
                    return;
                }
            }
        }

        private void StepActiveChunks(PixelGrid grid, int maxPixelsPerStep)
        {
            // 区块/脏区域优化：教学版本故意保持简单，便于观察效果。
            // Chunk / dirty-region optimization: this is intentionally simple for teaching.
            int count = grid.ActiveChunkCount;
            for (int i = 0; i < count; i++)
            {
                Vector2Int chunk = grid.GetActiveChunk(i);
                int minY = chunk.y * grid.ChunkSize;
                int maxY = Mathf.Min(minY + grid.ChunkSize, grid.Height);
                int minX = chunk.x * grid.ChunkSize;
                int maxX = Mathf.Min(minX + grid.ChunkSize, grid.Width);

                for (int y = minY; y < maxY; y++)
                {
                    for (int x = minX; x < maxX; x++)
                    {
                        if (!StepCell(grid, x, y, maxPixelsPerStep))
                        {
                            return;
                        }
                    }
                }
            }
        }

        private bool StepCell(PixelGrid grid, int x, int y, int maxPixelsPerStep)
        {
            if (!grid.TryConsumePixelBudget(maxPixelsPerStep))
            {
                // 预算耗尽后停止本次交互扫描。
                // Stop this interaction pass once the processing budget is exhausted.
                return false;
            }

            Pixel pixel = grid.GetCell(x, y);
            if (pixel.IsCreatureBody)
            {
                return true;
            }

            MaterialDefinition definition = MaterialDatabase.Get(pixel.MaterialType);
            bool changed = false;

            if (definition.ConsumesLifetime)
            {
                // 火焰/烟雾等临时材料会随时间衰减。
                // Temporary materials such as fire and smoke decay over time.
                pixel.Lifetime -= definition.LifetimeDecay;
                changed = true;
                if (pixel.Lifetime <= 0)
                {
                    // 寿命结束后转换成像素自身记录的衰变材料，缺省则使用材料定义。
                    // When lifetime ends, convert to the pixel's decay material, falling back to the definition.
                    MaterialType decay = pixel.DecayMaterial == MaterialType.Air
                        ? definition.BurnoutMaterial
                        : pixel.DecayMaterial;
                    grid.SetMaterial(x, y, decay);
                    return true;
                }
            }

            if (definition.HeatEmission > 0f)
            {
                // 热源材料每步向邻居释放热量。
                // Heat-source materials release heat to neighbors each step.
                ReleaseHeat(grid, x, y, definition, pixel.IsPlayerSpell);
                changed = true;
            }

            float oldTemperature = pixel.Temperature;
            Color32 oldColor = pixel.Color;
            // 非热源逐渐回到环境温度，挥发材料按寿命改变颜色/透明度。
            // Non-heat sources cool toward ambient; volatile materials tint/fade by lifetime.
            CoolTowardAmbient(ref pixel, definition);
            TintVolatilePixel(ref pixel, definition);

            // 只有温度或颜色真的变化时才唤醒周围区域。
            // Wake nearby regions only when temperature or color actually changed.
            changed = changed
                || !Mathf.Approximately(oldTemperature, pixel.Temperature)
                || !oldColor.Equals(pixel.Color);

            grid.SetCell(x, y, pixel);
            if (changed)
            {
                grid.MarkChanged(x, y);
            }

            return true;
        }

        private void ReleaseHeat(PixelGrid grid, int x, int y, MaterialDefinition heatSource, bool sourceIsPlayerSpell)
        {
            // 对八邻域逐个加热，非空气邻居才会吸收热量。
            // Heat each of the eight neighbors; only non-air neighbors absorb heat.
            for (int i = 0; i < NeighborOffsets.Length; i++)
            {
                int nx = x + NeighborOffsets[i].x;
                int ny = y + NeighborOffsets[i].y;
                if (!grid.InBounds(nx, ny))
                {
                    continue;
                }

                Pixel neighbor = grid.GetCell(nx, ny);
                if (neighbor.IsCreatureBody)
                {
                    continue;
                }

                MaterialDefinition neighborDefinition = MaterialDatabase.Get(neighbor.MaterialType);
                if (neighborDefinition.IsAir)
                {
                    continue;
                }

                neighbor.Temperature += heatSource.HeatEmission;

                if (neighborDefinition.IsFlammable
                    && neighbor.Temperature >= neighborDefinition.IgniteTemperature
                    && random.NextDouble() <= neighborDefinition.Flammability)
                {
                    // 达到点燃温度后再按可燃概率决定是否转成火。
                    // After reaching ignition temperature, flammability probability decides conversion to fire.
                    Ignite(grid, nx, ny, neighborDefinition, sourceIsPlayerSpell);
                }
                else
                {
                    // 未点燃也要写回升温后的邻居并标记变化。
                    // Even without ignition, write back the heated neighbor and mark it changed.
                    grid.SetCell(nx, ny, neighbor);
                    grid.MarkChanged(nx, ny);
                }
            }
        }

        private void Ignite(PixelGrid grid, int x, int y, MaterialDefinition source, bool isPlayerSpell)
        {
            // 点燃时根据源材料决定火焰寿命和最终衰变材料。
            // Ignition chooses flame lifetime and final decay material from the source material.
            MaterialDefinition fire = MaterialDatabase.Get(source.BurnMaterial);
            MaterialType decayMaterial = random.NextDouble() <= source.AlternateBurnoutChance
                ? source.AlternateBurnoutMaterial
                : source.BurnoutMaterial;
            int lifetime = random.Next(source.BurnLifetimeMin, source.BurnLifetimeMax + 1);
            Pixel burningPixel = MaterialDatabase.CreateBurningPixel(source, fire, decayMaterial, lifetime);
            burningPixel.IsPlayerSpell = isPlayerSpell;
            grid.SetCell(x, y, burningPixel);
            grid.MarkChanged(x, y);
        }

        private static void CoolTowardAmbient(ref Pixel pixel, MaterialDefinition definition)
        {
            if (definition.HeatEmission > 0f)
            {
                // 热源不在这里冷却，否则会抵消它自己的发热效果。
                // Heat sources do not cool here, or they would cancel their own emission.
                return;
            }

            // 普通像素温度缓慢回归环境温度。
            // Ordinary pixels slowly return toward ambient temperature.
            pixel.Temperature = Mathf.MoveTowards(pixel.Temperature, MaterialDatabase.Ambient, 3f);
        }

        private static void TintVolatilePixel(ref Pixel pixel, MaterialDefinition definition)
        {
            if (!definition.ConsumesLifetime || definition.StartLifetime <= 0)
            {
                // 只有带寿命的挥发材料需要按生命值变色。
                // Only lifetime-based volatile materials need life-driven tinting.
                return;
            }

            float life01 = Mathf.Clamp01(pixel.Lifetime / (float)definition.StartLifetime);
            if (definition.Type == MaterialType.Fire)
            {
                // 火焰快熄灭时颜色变暗。
                // Fire darkens as it approaches burnout.
                pixel.Color = (Color32)Color.Lerp(new Color32(120, 26, 12, 255), definition.Color, life01);
            }
            else if (definition.Type == MaterialType.Smoke)
            {
                // 烟雾随寿命降低而变透明。
                // Smoke becomes more transparent as lifetime decreases.
                Color32 faded = definition.Color;
                faded.a = (byte)Mathf.RoundToInt(Mathf.Lerp(35f, definition.Color.a, life01));
                pixel.Color = faded;
            }
        }
    }
}
