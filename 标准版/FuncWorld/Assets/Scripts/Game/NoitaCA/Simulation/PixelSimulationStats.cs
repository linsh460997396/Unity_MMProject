// 职责：保存最近一次模拟与渲染的性能/数量统计，供调试面板显示。
// Responsibility: Stores the latest simulation and rendering metrics for debug UI display.
namespace NoitaCA
{
    public struct PixelSimulationStats
    {
        // 网格与材料计数。
        // Grid and material counts.
        public int TotalPixels;
        public int NonAirPixels;
        public int WaterPixels;
        // 优化模式下的活跃工作量。
        // Active workload under optimized modes.
        public int ActivePixels;
        public int ActiveChunks;
        public int ProcessedPixels;
        // 每帧成本与当前模式。
        // Per-frame costs and the current mode.
        public float SimulationMs;
        public float RenderMs;
        public PixelSimulationMode Mode;
        // 面板使用这些标记直接展示优化开关状态。
        // The panel uses these flags to show optimization state directly.
        public bool UseChunkOptimization;
        public bool UseActiveRegionOptimization;
    }
}
