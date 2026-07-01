// 职责：根据材料密度和移动规则推进粉末、液体、气体等像素的空间移动。
// Responsibility: Advances spatial movement for powders, liquids, gases, and other pixels using density and movement rules.
using System;
using UnityEngine;

namespace NoitaCA
{
    public sealed class MovementSystem
    {
        // 随机源用于打散扫描偏向和移动方向，减少可见条纹。
        // Randomness breaks scan bias and movement direction bias, reducing visible streaks.
        private readonly System.Random random;

        public MovementSystem(System.Random random)
        {
            // 允许外部传入共享随机源；为空时创建本系统自己的随机源。
            // Accepts a shared random source; creates a local one when none is provided.
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
                // 活跃像素模式只检查被唤醒的格子。
                // Active-pixel mode checks only awakened cells.
                StepActivePixels(grid, maxPixelsPerStep);
            }
            else if (mode == PixelSimulationMode.ChunkBased)
            {
                // 区块模式扫描活跃脏区块内的所有格子。
                // Chunk mode scans every cell inside active dirty chunks.
                StepActiveChunks(grid, maxPixelsPerStep);
            }
            else
            {
                StepFullScan(grid, maxPixelsPerStep);
            }
        }

        private void StepFullScan(PixelGrid grid, int maxPixelsPerStep)
        {
            // 全图元胞自动机：即使是静止空气和石头也会访问。
            // Naive cellular automata: visits every pixel, including quiet air and stone.
            // 行扫描方向随机化，减少大面积水体向单侧偏移。
            // Row direction is randomized to reduce one-sided bias in large water bodies.
            for (int y = 0; y < grid.Height; y++)
            {
                if (random.Next(0, 2) == 0)
                {
                    for (int x = 0; x < grid.Width; x++)
                    {
                        if (!StepCell(grid, x, y, maxPixelsPerStep))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    for (int x = grid.Width - 1; x >= 0; x--)
                    {
                        if (!StepCell(grid, x, y, maxPixelsPerStep))
                        {
                            return;
                        }
                    }
                }
            }
        }

        private void StepActivePixels(PixelGrid grid, int maxPixelsPerStep)
        {
            // 活跃像素优化：只访问附近发生变化后被唤醒的像素。
            // Active Pixel optimization: only pixels woken by nearby changes are visited.
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
            // 区块/脏区域优化：扫描整个活跃区块，但跳过休眠区块。
            // Chunk / dirty-region optimization: scan whole chunks, but skip sleeping chunks.
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
                    if (random.Next(0, 2) == 0)
                    {
                        for (int x = minX; x < maxX; x++)
                        {
                            if (!StepCell(grid, x, y, maxPixelsPerStep))
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        for (int x = maxX - 1; x >= minX; x--)
                        {
                            if (!StepCell(grid, x, y, maxPixelsPerStep))
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        private bool StepCell(PixelGrid grid, int x, int y, int maxPixelsPerStep)
        {
            if (!grid.TryConsumePixelBudget(maxPixelsPerStep))
            {
                // 达到预算时返回 false，让外层扫描立即停止。
                // Returning false tells the outer scan to stop when the budget is reached.
                return false;
            }

            Pixel pixel = grid.GetCell(x, y);
            if (pixel.UpdatedThisFrame)
            {
                // 本帧已移动过的像素不再处理，避免一帧内连续下落多格。
                // Pixels already moved this frame are skipped to avoid multi-cell movement in one frame.
                return true;
            }

            MaterialDefinition definition = MaterialDatabase.Get(pixel.MaterialType);
            if (definition.MovementMode == PixelMovementMode.Static || random.NextDouble() > definition.MoveProbability)
            {
                // 静态材料或概率未命中的材料仍要标记为已处理。
                // Static materials or failed probability rolls are still marked as processed.
                MarkUpdated(grid, x, y, pixel);
                return true;
            }

            // 左右优先顺序随机化，避免斜向/横向流动固定偏向。
            // Randomizes left/right priority to avoid fixed diagonal or lateral drift.
            int firstSide = random.Next(0, 2) == 0 ? -1 : 1;

            if (definition.CanMoveVertical && TryMove(grid, x, y, 0, definition.VerticalDirection, definition))
            {
                return true;
            }

            if (definition.CanMoveDiagonal)
            {
                // 沙子等粉末随机优先尝试左下或右下。
                // Sand and other powders randomly choose left-down or right-down first.
                if (TryMove(grid, x, y, firstSide, definition.VerticalDirection, definition))
                {
                    return true;
                }

                if (TryMove(grid, x, y, -firstSide, definition.VerticalDirection, definition))
                {
                    return true;
                }
            }

            if (definition.CanMoveHorizontal && random.NextDouble() <= definition.LateralProbability)
            {
                // 水等液体随机优先向左或向右扩散，避免可见扫描偏向。
                // Water randomly chooses left or right first, avoiding visible scanning bias.
                if (TryHorizontalSpread(grid, x, y, firstSide, definition))
                {
                    return true;
                }

                if (TryHorizontalSpread(grid, x, y, -firstSide, definition))
                {
                    return true;
                }
            }

            pixel.FallingFrames = 0;
            // 所有移动尝试失败后，像素保持原位并标记本帧处理完毕。
            // After all movement attempts fail, the pixel stays in place and is marked processed.
            MarkUpdated(grid, x, y, pixel);
            return true;
        }

        private bool TryHorizontalSpread(PixelGrid grid, int x, int y, int direction, MaterialDefinition definition)
        {
            // 横向搜索允许液体跨过多个同类/空气格子，形成更自然的流动。
            // Lateral search lets liquids span several same-type/air cells for more natural flow.
            int maxDistance = Mathf.Max(1, definition.HorizontalSearchDistance);
            for (int distance = 1; distance <= maxDistance; distance++)
            {
                if (TryMove(grid, x, y, direction * distance, 0, definition))
                {
                    return true;
                }

                int checkX = x + direction * distance;
                if (!grid.InBounds(checkX, y))
                {
                    // 到达世界边界时停止继续搜索。
                    // Stop searching when reaching the world edge.
                    return false;
                }

                MaterialDefinition checkedDefinition = MaterialDatabase.Get(grid.GetCell(checkX, y).MaterialType);
                if (!checkedDefinition.IsAir && checkedDefinition.Type != definition.Type)
                {
                    // 遇到不同的非空气材料时视为被阻挡。
                    // A different non-air material blocks lateral spreading.
                    return false;
                }
            }

            return false;
        }

        private bool TryMove(PixelGrid grid, int fromX, int fromY, int offsetX, int offsetY, MaterialDefinition definition)
        {
            int toX = fromX + offsetX;
            int toY = fromY + offsetY;

            if (!grid.InBounds(toX, toY))
            {
                if (toY >= grid.Height && definition.MovementMode == PixelMovementMode.Gas)
                {
                    // 气体向上离开世界时不删除，避免顶部边缘闪烁式损失。
                    // Gas leaving upward is not deleted, preventing flickery loss at the top edge.
                    return false;
                }

                // 非气体越界时清空原位置，模拟流出世界边界。
                // Non-gas pixels moving out of bounds clear their source, simulating leaving the world.
                grid.SetCell(fromX, fromY, Pixel.FromMaterial(MaterialType.Air));
                grid.MarkChanged(fromX, fromY);
                return true;
            }

            Pixel source = grid.GetCell(fromX, fromY);
            Pixel target = grid.GetCell(toX, toY);
            MaterialDefinition targetDefinition = MaterialDatabase.Get(target.MaterialType);

            if (targetDefinition.IsAir)
            {
                // 目标为空气时直接搬移，并清空原位置。
                // If the target is air, move directly and clear the source.
                source.UpdatedThisFrame = true;
                source.FallingFrames = offsetY == -1 ? Math.Min(source.FallingFrames + 1, 64) : 0;
                grid.SetCell(toX, toY, source);
                grid.SetCell(fromX, fromY, Pixel.FromMaterial(MaterialType.Air));
                grid.MarkChanged(fromX, fromY);
                grid.MarkChanged(toX, toY);
                return true;
            }

            if (!targetDefinition.CanBeDisplaced || !CanDisplace(definition, targetDefinition, offsetY))
            {
                // 目标不可替换或密度关系不允许时，本次移动失败。
                // Movement fails when the target is not displaceable or density rules reject it.
                return false;
            }

            // 可位移材料通过交换完成，例如沙子沉入水中。
            // Displaceable materials move by swapping, such as sand sinking through water.
            source.UpdatedThisFrame = true;
            target.UpdatedThisFrame = true;
            source.FallingFrames = offsetY == -1 ? Math.Min(source.FallingFrames + 1, 64) : 0;
            target.FallingFrames = 0;
            grid.SetCell(toX, toY, source);
            grid.SetCell(fromX, fromY, target);
            grid.MarkChanged(fromX, fromY);
            grid.MarkChanged(toX, toY);
            return true;
        }

        private static bool CanDisplace(MaterialDefinition source, MaterialDefinition target, int offsetY)
        {
            if (offsetY < 0)
            {
                // 向下移动时，高密度材料可以挤开低密度材料。
                // Moving downward, denser material can displace lighter material.
                return source.Density > target.Density;
            }

            if (offsetY > 0)
            {
                // 向上移动时，低密度材料可以挤开高密度材料。
                // Moving upward, lighter material can displace denser material.
                return source.Density < target.Density;
            }

            // 水平位移需要明显密度差，粉末不做横向挤压。
            // Horizontal displacement needs a clear density gap, and powders do not shove sideways.
            return Math.Abs(source.Density - target.Density) >= 8
                && source.MovementMode != PixelMovementMode.Powder;
        }

        private static void MarkUpdated(PixelGrid grid, int x, int y, Pixel pixel)
        {
            // Pixel 是值类型，修改后写回网格才会生效。
            // Pixel is a value type, so changes only take effect after writing back to the grid.
            pixel.UpdatedThisFrame = true;
            grid.SetCell(x, y, pixel);
        }
    }
}
