// 职责：协调一次像素世界模拟，将移动、交互和统计收集串成完整步骤。
// Responsibility: Coordinates one pixel-world simulation step by chaining movement, interaction, and statistics collection.
using System.Diagnostics;

namespace NoitaCA
{
    public sealed class PixelSimulation
    {
        // 移动与交互拆分成两个系统，便于讲解和单独优化。
        // Movement and interaction are split into separate systems for teaching and optimization.
        private readonly MovementSystem movementSystem;
        private readonly InteractionSystem interactionSystem;
        private readonly Stopwatch stopwatch = new Stopwatch();

        public PixelSimulationMode Mode { get; set; } = PixelSimulationMode.FullScan;
        public int MaxProcessedPixelsPerStep { get; set; }
        public PixelSimulationStats LastStats { get; private set; }

        public PixelSimulation(int seed = 0)
        {
            // 两个系统共享随机源，保证同一 seed 下行为可复现。
            // Both systems share the same random source so behavior is reproducible with a seed.
            System.Random random = seed == 0 ? new System.Random() : new System.Random(seed);
            movementSystem = new MovementSystem(random);
            interactionSystem = new InteractionSystem(random);
        }

        public void Step(PixelGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            // Stopwatch 只包围模拟逻辑，渲染耗时由外部单独写入。
            // Stopwatch covers simulation only; render time is recorded externally.
            stopwatch.Reset();
            stopwatch.Start();

            // 网格在每步开始时清理统计、下一帧活跃集合和脏区块标记。
            // The grid clears counters, next active buffers, and dirty chunks at step start.
            grid.BeginSimulationStep();
            grid.ClearUpdatedFlags(Mode);

            // 移动只处理密度/流动；交互只处理热量/寿命/材料转换。
            // Movement handles density and flow; interaction handles heat, lifetime, and material conversion.
            movementSystem.Step(grid, Mode, MaxProcessedPixelsPerStep);
            interactionSystem.Step(grid, Mode, MaxProcessedPixelsPerStep);

            // 根据模式提交下一帧活跃像素或活跃区块。
            // Commits next-frame active pixels or chunks based on the active mode.
            grid.EndSimulationStep(Mode);

            stopwatch.Stop();
            PixelSimulationStats stats = LastStats;
            // 统计数据保持在一个结构中，方便 UI 面板一次性读取。
            // Stats are kept in one struct so UI panels can read them in one pass.
            stats.TotalPixels = grid.Width * grid.Height;
            stats.ActivePixels = Mode == PixelSimulationMode.ChunkBased
                ? grid.ActiveChunkCount * grid.ChunkSize * grid.ChunkSize
                : grid.ActivePixelCount;
            stats.ActiveChunks = grid.ActiveChunkCount;
            stats.ProcessedPixels = grid.ProcessedPixelsThisStep;
            stats.SimulationMs = (float)stopwatch.Elapsed.TotalMilliseconds;
            stats.Mode = Mode;
            stats.UseChunkOptimization = Mode == PixelSimulationMode.ChunkBased;
            stats.UseActiveRegionOptimization = Mode == PixelSimulationMode.ActivePixels;
            LastStats = stats;
        }

        public void SetRenderTime(float renderMs)
        {
            // 渲染系统在完成贴图上传后回填渲染耗时。
            // The renderer writes render cost back after texture upload finishes.
            PixelSimulationStats stats = LastStats;
            stats.RenderMs = renderMs;
            LastStats = stats;
        }

        public void SetMaterialCounts(int nonAirPixels, int waterPixels)
        {
            // 材料计数可能低频刷新，因此独立于主模拟步骤写入。
            // Material counts may refresh at a lower rate, so they are written independently.
            PixelSimulationStats stats = LastStats;
            stats.NonAirPixels = nonAirPixels;
            stats.WaterPixels = waterPixels;
            LastStats = stats;
        }
    }
}
