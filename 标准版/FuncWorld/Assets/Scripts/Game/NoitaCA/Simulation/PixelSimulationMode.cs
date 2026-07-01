// 职责：列出像素模拟可切换的扫描/优化模式。
// Responsibility: Lists the scan and optimization modes available to the pixel simulation.
namespace NoitaCA
{
    public enum PixelSimulationMode
    {
        // 全图扫描：每步检查整个网格，最直观但成本最高。
        // Full scan: checks the whole grid every step; clearest but most expensive.
        FullScan,
        // 活跃像素：只访问变化附近被唤醒的像素。
        // Active pixels: visits only pixels awakened near recent changes.
        ActivePixels,
        // 区块更新：按脏区块唤醒一片区域，而不是逐个唤醒像素。
        // Chunk based: wakes dirty regions instead of individual pixels.
        ChunkBased
    }
}
