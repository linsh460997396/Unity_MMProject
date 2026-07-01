// 职责：存储像素网格状态，并维护活跃像素/活跃区块缓冲以支持多种模拟优化。
// Responsibility: Stores pixel-grid state and maintains active pixel/chunk buffers for multiple simulation optimizations.
using System.Collections.Generic;
using UnityEngine;

namespace NoitaCA
{
    public class PixelGrid
    {
        // cells 保存真实像素；active/next 缓冲决定本帧和下一帧要检查哪些像素。
        // cells stores real pixels; active/next buffers decide which pixels are checked this frame and next frame.
        private readonly Pixel[,] cells;
        private readonly bool[,] activeCells;
        private readonly bool[,] nextActiveCells;
        private readonly List<Vector2Int> activeCellList = new List<Vector2Int>(1024);
        private readonly List<Vector2Int> nextActiveCellList = new List<Vector2Int>(1024);

        // 区块缓冲用于脏区域优化，比逐像素活跃列表更粗但更便宜。
        // Chunk buffers support dirty-region optimization, coarser but cheaper than per-pixel active lists.
        private bool[,] activeChunks;
        private bool[,] nextActiveChunks;
        private bool[,] changedChunks;
        private int[,] chunkSleepFrames;
        private readonly List<Vector2Int> activeChunkList = new List<Vector2Int>(64);
        private readonly List<Vector2Int> nextActiveChunkList = new List<Vector2Int>(64);
        private int playerSpellWriteDepth;

        public int Width { get; }
        public int Height { get; }
        public int ChunkSize { get; private set; } = 16;
        public int ChunkColumns { get; private set; }
        public int ChunkRows { get; private set; }
        public int ChunkSleepDelay { get; private set; } = 2;
        public int ProcessedPixelsThisStep { get; private set; }
        public int ChangedPixelsThisStep { get; private set; }
        public int ActivePixelCount => activeCellList.Count;
        public int ActiveChunkCount => activeChunkList.Count;

        public PixelGrid(int width, int height)
        {
            // 尺寸最小为 1，避免后续数组和坐标计算出现零尺寸网格。
            // Dimensions are clamped to at least 1 to avoid zero-sized arrays and coordinate math.
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            cells = new Pixel[Width, Height];
            activeCells = new bool[Width, Height];
            nextActiveCells = new bool[Width, Height];
            ConfigureOptimization(16, 2);
            Clear();
        }

        public void ConfigureOptimization(int chunkSize, int chunkSleepDelay)
        {
            // 重新配置区块大小时需要重建所有区块级缓存。
            // Reconfiguring chunk size requires rebuilding all chunk-level caches.
            ChunkSize = Mathf.Max(4, chunkSize);
            ChunkSleepDelay = Mathf.Max(0, chunkSleepDelay);
            ChunkColumns = Mathf.CeilToInt(Width / (float)ChunkSize);
            ChunkRows = Mathf.CeilToInt(Height / (float)ChunkSize);
            activeChunks = new bool[ChunkColumns, ChunkRows];
            nextActiveChunks = new bool[ChunkColumns, ChunkRows];
            changedChunks = new bool[ChunkColumns, ChunkRows];
            chunkSleepFrames = new int[ChunkColumns, ChunkRows];
            activeChunkList.Clear();
            nextActiveChunkList.Clear();
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Pixel GetCell(int x, int y)
        {
            return cells[x, y];
        }

        public void SetCell(int x, int y, Pixel pixel)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            // SetCell 只写入数据；是否唤醒周围区域由调用者决定。
            // SetCell only writes data; callers decide whether nearby regions should wake up.
            ApplyPlayerSpellFlag(ref pixel);
            cells[x, y] = pixel;
        }

        public bool IsAir(int x, int y)
        {
            return InBounds(x, y) && MaterialDatabase.Get(cells[x, y].MaterialType).IsAir;
        }

        public bool IsEmpty(int x, int y)
        {
            return IsAir(x, y);
        }

        public MaterialType GetMaterial(int x, int y)
        {
            return InBounds(x, y) ? cells[x, y].MaterialType : MaterialType.Air;
        }

        public bool IsSolid(int x, int y)
        {
            return InBounds(x, y) && MaterialDatabase.Get(cells[x, y].MaterialType).BlocksPlayer;
        }

        public void SetMaterial(int x, int y, MaterialType materialType)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            // 材料替换是用户绘制和场景生成的主要入口，必须标记变化区域。
            // Material replacement is the main entry for painting and world generation, so it marks changed regions.
            Pixel pixel = Pixel.FromMaterial(materialType);
            ApplyPlayerSpellFlag(ref pixel);
            cells[x, y] = pixel;
            MarkChanged(x, y);
        }

        public void SetMaterialSilent(int x, int y, MaterialType materialType)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            Pixel pixel = Pixel.FromMaterial(materialType);
            ApplyPlayerSpellFlag(ref pixel);
            cells[x, y] = pixel;
            MarkChangedForRender(x, y);
        }

        public void SetCreatureBodyMaterialSilent(int x, int y, MaterialType materialType)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            Pixel pixel = Pixel.FromMaterial(materialType);
            pixel.IsCreatureBody = true;
            cells[x, y] = pixel;
            MarkChangedForRender(x, y);
        }

        public void BeginPlayerSpellWrites()
        {
            playerSpellWriteDepth++;
        }

        public void EndPlayerSpellWrites()
        {
            playerSpellWriteDepth = Mathf.Max(0, playerSpellWriteDepth - 1);
        }

        public void SwapCells(int firstX, int firstY, int secondX, int secondY)
        {
            if (!InBounds(firstX, firstY) || !InBounds(secondX, secondY))
            {
                return;
            }

            Pixel first = cells[firstX, firstY];
            Pixel second = cells[secondX, secondY];
            // 交换用于位移逻辑，例如重材料下沉、轻材料上浮。
            // Swapping supports displacement, such as dense materials sinking and light materials rising.
            cells[firstX, firstY] = second;
            cells[secondX, secondY] = first;
            MarkChanged(firstX, firstY);
            MarkChanged(secondX, secondY);
        }

        public void BeginSimulationStep()
        {
            // 每步重置工作量统计，并清空下一帧缓冲。
            // Reset per-step workload stats and clear next-frame buffers.
            ProcessedPixelsThisStep = 0;
            ChangedPixelsThisStep = 0;
            ClearNextActiveCells();
            ClearNextActiveChunks();
            ClearChangedChunks();
        }

        public void EndSimulationStep(PixelSimulationMode mode)
        {
            if (mode == PixelSimulationMode.ActivePixels)
            {
                // 活跃像素模式把 next 列表提交为下一步 active 列表。
                // Active-pixel mode commits the next list as the next step's active list.
                SwapActiveCellBuffers();
            }
            else if (mode == PixelSimulationMode.ChunkBased)
            {
                // 区块模式先根据脏区块/睡眠计数生成下一帧区块，再提交。
                // Chunk mode builds next-frame chunks from dirty chunks and sleep counters, then commits.
                BuildNextChunkFrame();
                SwapActiveChunkBuffers();
            }
        }

        public bool TryConsumePixelBudget(int maxPixelsPerStep)
        {
            if (maxPixelsPerStep > 0 && ProcessedPixelsThisStep >= maxPixelsPerStep)
            {
                // 预算耗尽时让调用者提前结束本次扫描。
                // When the budget is exhausted, callers stop scanning early.
                return false;
            }

            // 预算为 0 表示不限制；否则每访问一个像素都计数。
            // A zero budget means unlimited; otherwise each visited pixel is counted.
            ProcessedPixelsThisStep++;
            return true;
        }

        public void ClearUpdatedFlags(PixelSimulationMode mode)
        {
            if (mode == PixelSimulationMode.ActivePixels)
            {
                // 优化模式只清理会被访问的像素，避免全图循环。
                // Optimized mode clears only pixels that will be visited, avoiding a full-grid pass.
                for (int i = 0; i < activeCellList.Count; i++)
                {
                    Vector2Int cellPosition = activeCellList[i];
                    ClearUpdatedFlag(cellPosition.x, cellPosition.y);
                }

                return;
            }

            if (mode == PixelSimulationMode.ChunkBased)
            {
                // 区块模式清理活跃区块内的像素标记。
                // Chunk mode clears flags inside active chunks.
                for (int i = 0; i < activeChunkList.Count; i++)
                {
                    Vector2Int chunk = activeChunkList[i];
                    ForEachCellInChunk(chunk.x, chunk.y, ClearUpdatedFlag);
                }

                return;
            }

            ClearUpdatedFlags();
        }

        public void ClearUpdatedFlags()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    ClearUpdatedFlag(x, y);
                }
            }
        }

        public void MarkChanged(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            ChangedPixelsThisStep++;
            // 任一像素变化都会唤醒周围像素和相邻区块，保持局部传播不断链。
            // Any pixel change wakes nearby pixels and neighboring chunks so local propagation continues.
            MarkActiveArea(x, y, 1);
            MarkChunkAndNeighborsActive(x, y);
        }

        private void MarkChangedForRender(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            int chunkX = x / ChunkSize;
            int chunkY = y / ChunkSize;
            if (chunkX >= 0 && chunkX < ChunkColumns && chunkY >= 0 && chunkY < ChunkRows)
            {
                changedChunks[chunkX, chunkY] = true;
            }
        }

        public void MarkActiveArea(int centerX, int centerY, int radius)
        {
            // 同时写入当前和下一帧缓冲，保证刚画出的材料会立刻参与模拟并延续到下一步。
            // Writes both current and next buffers so newly changed material simulates immediately and continues next step.
            int safeRadius = Mathf.Max(0, radius);
            for (int y = centerY - safeRadius; y <= centerY + safeRadius; y++)
            {
                for (int x = centerX - safeRadius; x <= centerX + safeRadius; x++)
                {
                    MarkCurrentActiveCell(x, y);
                    MarkNextActiveCell(x, y);
                }
            }
        }

        public void ActivateAll()
        {
            // 全量激活用于初始化、切换优化模式或重建世界。
            // Full activation is used for initialization, mode switching, or rebuilding the world.
            ClearActiveCells();
            ClearActiveChunks();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (!activeCells[x, y])
                    {
                        activeCells[x, y] = true;
                        activeCellList.Add(new Vector2Int(x, y));
                    }
                }
            }

            for (int cy = 0; cy < ChunkRows; cy++)
            {
                for (int cx = 0; cx < ChunkColumns; cx++)
                {
                    activeChunks[cx, cy] = true;
                    activeChunkList.Add(new Vector2Int(cx, cy));
                }
            }
        }

        public Vector2Int GetActiveCell(int index)
        {
            return activeCellList[index];
        }

        public Vector2Int GetActiveChunk(int index)
        {
            return activeChunkList[index];
        }

        public bool IsCellActive(int x, int y)
        {
            return InBounds(x, y) && activeCells[x, y];
        }

        public bool IsChunkActive(int chunkX, int chunkY)
        {
            return chunkX >= 0
                && chunkX < ChunkColumns
                && chunkY >= 0
                && chunkY < ChunkRows
                && activeChunks[chunkX, chunkY];
        }

        public void ForEachCellInChunk(int chunkX, int chunkY, System.Action<int, int> action)
        {
            // 将区块坐标转换成实际像素范围，边缘区块会被裁剪到网格内。
            // Converts chunk coordinates to pixel bounds, clipping edge chunks to the grid.
            int minX = chunkX * ChunkSize;
            int minY = chunkY * ChunkSize;
            int maxX = Mathf.Min(minX + ChunkSize, Width);
            int maxY = Mathf.Min(minY + ChunkSize, Height);

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    action(x, y);
                }
            }
        }

        public void CountMaterials(out int nonAirPixels, out int waterPixels)
        {
            // 统计给性能面板使用，不参与物理决策。
            // Counts are for the performance panel and do not affect physics decisions.
            nonAirPixels = 0;
            waterPixels = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    MaterialType type = cells[x, y].MaterialType;
                    if (type == MaterialType.Water)
                    {
                        waterPixels++;
                    }

                    if (!MaterialDatabase.Get(type).IsAir)
                    {
                        nonAirPixels++;
                    }
                }
            }
        }

        public int DestroyCircle(Vector2Int center, int radius)
        {
            return DestroyCircle(center.x, center.y, radius);
        }

        public int DestroyCircle(int centerX, int centerY, int radius)
        {
            int safeRadius = Mathf.Max(1, radius);
            int radiusSquared = safeRadius * safeRadius;
            int changed = 0;

            for (int y = centerY - safeRadius; y <= centerY + safeRadius; y++)
            {
                for (int x = centerX - safeRadius; x <= centerX + safeRadius; x++)
                {
                    if (!InBounds(x, y))
                    {
                        continue;
                    }

                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy > radiusSquared || MaterialDatabase.Get(cells[x, y].MaterialType).IsAir)
                    {
                        continue;
                    }

                    SetMaterial(x, y, MaterialType.Air);
                    changed++;
                }
            }

            if (changed > 0)
            {
                MarkActiveArea(centerX, centerY, safeRadius + 2);
            }

            return changed;
        }

        public int SpawnMaterial(Vector2Int center, MaterialType materialType, int amount, int radius)
        {
            return SpawnMaterial(center.x, center.y, materialType, amount, radius);
        }

        public int SpawnMaterial(int centerX, int centerY, MaterialType materialType, int amount, int radius)
        {
            int safeAmount = Mathf.Max(0, amount);
            int safeRadius = Mathf.Max(1, radius);
            int placed = 0;
            const float GoldenAngle = 2.39996323f;

            int attempts = Mathf.Max(safeAmount * 8, safeRadius * safeRadius * 4);
            for (int i = 0; i < attempts && placed < safeAmount; i++)
            {
                float t = attempts <= 1 ? 0f : i / (float)(attempts - 1);
                float distance = Mathf.Sqrt(t) * safeRadius;
                float angle = i * GoldenAngle;
                int x = centerX + Mathf.RoundToInt(Mathf.Cos(angle) * distance);
                int y = centerY + Mathf.RoundToInt(Mathf.Sin(angle) * distance);

                if (!InBounds(x, y) || !CanSpawnInto(x, y, materialType))
                {
                    continue;
                }

                SetMaterial(x, y, materialType);
                placed++;
            }

            if (placed > 0)
            {
                MarkActiveArea(centerX, centerY, safeRadius + 2);
            }

            return placed;
        }

        public int IgniteCircle(Vector2Int center, int radius)
        {
            return IgniteCircle(center.x, center.y, radius);
        }

        public int IgniteCircle(int centerX, int centerY, int radius)
        {
            int safeRadius = Mathf.Max(1, radius);
            int radiusSquared = safeRadius * safeRadius;
            int changed = 0;

            for (int y = centerY - safeRadius; y <= centerY + safeRadius; y++)
            {
                for (int x = centerX - safeRadius; x <= centerX + safeRadius; x++)
                {
                    if (!InBounds(x, y))
                    {
                        continue;
                    }

                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy > radiusSquared)
                    {
                        continue;
                    }

                    MaterialType material = cells[x, y].MaterialType;
                    if (material == MaterialType.Wood)
                    {
                        MaterialDefinition source = MaterialDatabase.Get(material);
                        MaterialDefinition fire = MaterialDatabase.Get(source.BurnMaterial);
                        int lifetime = Mathf.RoundToInt((source.BurnLifetimeMin + source.BurnLifetimeMax) * 0.5f);
                        SetCell(x, y, MaterialDatabase.CreateBurningPixel(source, fire, source.BurnoutMaterial, lifetime));
                        MarkChanged(x, y);
                        changed++;
                    }
                    else if (MaterialDatabase.Get(material).IsAir && ((x + y) & 1) == 0)
                    {
                        SetMaterial(x, y, MaterialType.Fire);
                        changed++;
                    }
                }
            }

            if (changed > 0)
            {
                MarkActiveArea(centerX, centerY, safeRadius + 3);
            }

            return changed;
        }

        public void ExplodeCircle(Vector2Int center, int radius)
        {
            ExplodeCircle(center.x, center.y, radius);
        }

        public void ExplodeCircle(int centerX, int centerY, int radius)
        {
            int safeRadius = Mathf.Max(1, radius);
            DestroyCircle(centerX, centerY, safeRadius);
            SpawnMaterial(centerX, centerY, MaterialType.Debris, Mathf.Max(4, safeRadius * 2), safeRadius + 1);
            SpawnMaterial(centerX, centerY, MaterialType.Smoke, safeRadius * safeRadius / 2, safeRadius + 2);
            SpawnMaterial(centerX, centerY, MaterialType.Fire, Mathf.Max(2, safeRadius), Mathf.Max(2, safeRadius / 2));
            MarkActiveArea(centerX, centerY, safeRadius + 4);
        }

        public void PaintCircle(int centerX, int centerY, int radius, MaterialType materialType)
        {
            // 圆形笔刷用平方距离判断，避免每个格子开方。
            // The circular brush uses squared distance to avoid square roots per cell.
            int safeRadius = Mathf.Max(1, radius);
            int radiusSquared = safeRadius * safeRadius;

            for (int y = centerY - safeRadius; y <= centerY + safeRadius; y++)
            {
                for (int x = centerX - safeRadius; x <= centerX + safeRadius; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        SetMaterial(x, y, materialType);
                    }
                }
            }
        }

        private bool CanSpawnInto(int x, int y, MaterialType materialType)
        {
            if (cells[x, y].IsCreatureBody)
            {
                return false;
            }

            MaterialDefinition target = MaterialDatabase.Get(cells[x, y].MaterialType);
            if (target.IsAir || target.CanBeDisplaced)
            {
                return true;
            }

            return materialType == MaterialType.Fire && cells[x, y].MaterialType == MaterialType.Wood;
        }

        private void ApplyPlayerSpellFlag(ref Pixel pixel)
        {
            if (playerSpellWriteDepth <= 0 || MaterialDatabase.Get(pixel.MaterialType).IsAir)
            {
                return;
            }

            pixel.IsPlayerSpell = true;
        }

        private void Clear()
        {
            // 初始网格全部填充空气，然后激活一次让首帧可以完整渲染/模拟。
            // Initial grid is filled with air, then activated once for first-frame render/simulation.
            Pixel air = Pixel.FromMaterial(MaterialType.Air);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    cells[x, y] = air;
                }
            }

            ActivateAll();
        }

        private void ClearUpdatedFlag(int x, int y)
        {
            if (!InBounds(x, y))
            {
                return;
            }

            Pixel pixel = cells[x, y];
            // Pixel 是结构体，修改字段后必须写回数组。
            // Pixel is a struct, so field changes must be written back to the array.
            pixel.UpdatedThisFrame = false;
            cells[x, y] = pixel;
        }

        private void MarkNextActiveCell(int x, int y)
        {
            if (!InBounds(x, y) || nextActiveCells[x, y])
            {
                return;
            }

            // bool 表负责去重，list 负责保持可迭代的紧凑集合。
            // The bool grid deduplicates while the list keeps a compact iterable set.
            nextActiveCells[x, y] = true;
            nextActiveCellList.Add(new Vector2Int(x, y));
        }

        private void MarkChunkAndNeighborsActive(int x, int y)
        {
            // 一个像素变化可能影响相邻区块边界，因此唤醒 3x3 区块邻域。
            // A changed pixel can affect chunk borders, so a 3x3 chunk neighborhood is awakened.
            int chunkX = Mathf.Clamp(x / ChunkSize, 0, ChunkColumns - 1);
            int chunkY = Mathf.Clamp(y / ChunkSize, 0, ChunkRows - 1);

            if (chunkX >= 0 && chunkX < ChunkColumns && chunkY >= 0 && chunkY < ChunkRows)
            {
                changedChunks[chunkX, chunkY] = true;
            }

            for (int cy = chunkY - 1; cy <= chunkY + 1; cy++)
            {
                for (int cx = chunkX - 1; cx <= chunkX + 1; cx++)
                {
                    MarkCurrentActiveChunk(cx, cy);
                    MarkNextActiveChunk(cx, cy);
                }
            }
        }

        private void MarkCurrentActiveCell(int x, int y)
        {
            if (!InBounds(x, y) || activeCells[x, y])
            {
                return;
            }

            activeCells[x, y] = true;
            activeCellList.Add(new Vector2Int(x, y));
        }

        private void MarkNextActiveChunk(int chunkX, int chunkY)
        {
            if (chunkX < 0 || chunkX >= ChunkColumns || chunkY < 0 || chunkY >= ChunkRows || nextActiveChunks[chunkX, chunkY])
            {
                return;
            }

            nextActiveChunks[chunkX, chunkY] = true;
            nextActiveChunkList.Add(new Vector2Int(chunkX, chunkY));
        }

        private void MarkCurrentActiveChunk(int chunkX, int chunkY)
        {
            if (chunkX < 0 || chunkX >= ChunkColumns || chunkY < 0 || chunkY >= ChunkRows || activeChunks[chunkX, chunkY])
            {
                return;
            }

            activeChunks[chunkX, chunkY] = true;
            activeChunkList.Add(new Vector2Int(chunkX, chunkY));
        }

        private void BuildNextChunkFrame()
        {
            // 区块/脏区域教学简化：变化区块继续唤醒，未变化区块短暂停留后休眠。
            // Chunk/dirty-region teaching simplification: changed chunks wake; unchanged chunks linger briefly, then sleep.
            for (int i = 0; i < activeChunkList.Count; i++)
            {
                Vector2Int chunk = activeChunkList[i];
                if (changedChunks[chunk.x, chunk.y])
                {
                    // 有变化的区块重置睡眠计数，下一帧继续扫描。
                    // Changed chunks reset their sleep counter and stay active next frame.
                    chunkSleepFrames[chunk.x, chunk.y] = 0;
                    MarkNextActiveChunk(chunk.x, chunk.y);
                    continue;
                }

                chunkSleepFrames[chunk.x, chunk.y]++;
                if (chunkSleepFrames[chunk.x, chunk.y] <= ChunkSleepDelay)
                {
                    // 未变化区块保留几帧，避免刚停下的流体马上被错误休眠。
                    // Unchanged chunks stay alive for a few frames so settling fluids do not sleep too early.
                    MarkNextActiveChunk(chunk.x, chunk.y);
                }
            }
        }

        private void ClearActiveCells()
        {
            // 按列表反向清理 bool 表，比扫描整张网格便宜。
            // Clears the bool grid through the list, cheaper than scanning the whole grid.
            for (int i = 0; i < activeCellList.Count; i++)
            {
                Vector2Int cell = activeCellList[i];
                activeCells[cell.x, cell.y] = false;
            }

            activeCellList.Clear();
        }

        private void ClearNextActiveCells()
        {
            for (int i = 0; i < nextActiveCellList.Count; i++)
            {
                Vector2Int cell = nextActiveCellList[i];
                nextActiveCells[cell.x, cell.y] = false;
            }

            nextActiveCellList.Clear();
        }

        private void SwapActiveCellBuffers()
        {
            // 提交 next 缓冲时复用现有集合，减少每帧分配。
            // Commits the next buffer while reusing collections to reduce per-frame allocations.
            ClearActiveCells();
            for (int i = 0; i < nextActiveCellList.Count; i++)
            {
                Vector2Int cell = nextActiveCellList[i];
                activeCells[cell.x, cell.y] = true;
                activeCellList.Add(cell);
            }
        }

        private void ClearActiveChunks()
        {
            // 区块缓冲同样用 list 清理 bool 表，保持清理成本随活跃量变化。
            // Chunk buffers also clear bool grids through lists, keeping cleanup proportional to activity.
            for (int i = 0; i < activeChunkList.Count; i++)
            {
                Vector2Int chunk = activeChunkList[i];
                activeChunks[chunk.x, chunk.y] = false;
            }

            activeChunkList.Clear();
        }

        private void ClearNextActiveChunks()
        {
            for (int i = 0; i < nextActiveChunkList.Count; i++)
            {
                Vector2Int chunk = nextActiveChunkList[i];
                nextActiveChunks[chunk.x, chunk.y] = false;
            }

            nextActiveChunkList.Clear();
        }

        private void SwapActiveChunkBuffers()
        {
            // 将下一帧区块列表切换成当前活跃区块列表。
            // Promotes the next-frame chunk list to the current active chunk list.
            ClearActiveChunks();
            for (int i = 0; i < nextActiveChunkList.Count; i++)
            {
                Vector2Int chunk = nextActiveChunkList[i];
                activeChunks[chunk.x, chunk.y] = true;
                activeChunkList.Add(chunk);
            }
        }

        private void ClearChangedChunks()
        {
            // 脏区块矩阵很小，直接全清简单且成本可控。
            // The dirty-chunk matrix is small, so a full clear is simple and cheap enough.
            for (int y = 0; y < ChunkRows; y++)
            {
                for (int x = 0; x < ChunkColumns; x++)
                {
                    changedChunks[x, y] = false;
                }
            }
        }
    }
}
