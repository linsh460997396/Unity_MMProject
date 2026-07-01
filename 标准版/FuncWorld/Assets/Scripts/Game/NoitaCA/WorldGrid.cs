// 职责：为旧代码保留世界网格类型入口，实际网格逻辑由 PixelGrid 实现。
// Responsibility: Keeps the legacy world-grid type entry point while PixelGrid owns the real grid logic.
namespace NoitaCA
{
    public sealed class WorldGrid : PixelGrid
    {
        // 职责：把世界尺寸直接转交给基础像素网格。
        // Responsibility: Forwards world dimensions directly to the base pixel grid.
        public WorldGrid(int width, int height)
            : base(width, height)
        {
        }
    }
}
