// 职责：把 PixelGrid 的颜色数据上传到 Texture2D，并通过 SpriteRenderer 显示像素世界。
// Responsibility: Uploads PixelGrid color data into a Texture2D and displays the pixel world through a SpriteRenderer.
using UnityEngine;

namespace NoitaCA
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelWorldRenderer : MonoBehaviour
    {
        // 运行时创建的纹理和精灵由本组件持有并在销毁时释放。
        // Runtime-created texture and sprite are owned here and released on destroy.
        private SpriteRenderer spriteRenderer;
        private Texture2D texture;
        private Sprite sprite;
        private Color32[] pixels;
        private PixelGrid grid;

        public int PixelsPerUnit { get; private set; } = 16;

        public Vector2 WorldSize
        {
            get
            {
                if (grid == null)
                {
                    // 未初始化时返回零尺寸，避免调用方拿到无效世界大小。
                    // Before initialization, return zero size to avoid invalid world-size use.
                    return Vector2.zero;
                }

                return new Vector2(grid.Width / (float)PixelsPerUnit, grid.Height / (float)PixelsPerUnit);
            }
        }

        public void Initialize(PixelGrid worldGrid, int pixelsPerUnit)
        {
            // 初始化时绑定网格、准备像素缓冲，并重建显示纹理。
            // Initialization binds the grid, prepares the pixel buffer, and rebuilds the display texture.
            grid = worldGrid;
            PixelsPerUnit = Mathf.Max(1, pixelsPerUnit);

            spriteRenderer = GetComponent<SpriteRenderer>();
            pixels = new Color32[grid.Width * grid.Height];

            if (sprite != null)
            {
                // 重新初始化时释放旧 Sprite，避免编辑器播放中泄漏对象。
                // On reinitialization, release old sprites to avoid leaks during editor play mode.
                Destroy(sprite);
            }

            if (texture != null)
            {
                Destroy(texture);
            }

            texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false);
            // 点过滤保持像素艺术边缘清晰。
            // Point filtering keeps pixel-art edges crisp.
            texture.name = "Pixel World Texture";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, grid.Width, grid.Height),
                Vector2.zero,
                PixelsPerUnit);
            sprite.name = "Pixel World Sprite";
            spriteRenderer.sprite = sprite;
        }

        public void Render()
        {
            if (grid == null || texture == null)
            {
                return;
            }

            // 将网格中每个像素的当前颜色写入一维贴图缓冲。
            // Writes each grid cell's current color into the flat texture buffer.
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    pixels[y * grid.Width + x] = grid.GetCell(x, y).Color;
                }
            }

            // 一次性上传整张贴图，避免逐像素调用 SetPixel 的高开销。
            // Upload the whole texture at once, avoiding the cost of per-pixel SetPixel calls.
            texture.SetPixels32(pixels);
            texture.Apply(false);
        }

        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            // 世界坐标先转到显示物体局部空间，再按 PPU 还原到格子坐标。
            // World coordinates are converted to display local space, then scaled by PPU into cell coordinates.
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            return new Vector2Int(
                Mathf.FloorToInt(localPosition.x * PixelsPerUnit),
                Mathf.FloorToInt(localPosition.y * PixelsPerUnit));
        }

        public Vector3 CellToWorldCenter(int x, int y)
        {
            // 加 0.5 取格子中心，玩家出生点等对象会对齐像素格中央。
            // Adding 0.5 targets the cell center so objects spawn aligned to pixel cells.
            Vector3 localPosition = new Vector3(
                (x + 0.5f) / PixelsPerUnit,
                (y + 0.5f) / PixelsPerUnit,
                0f);
            return transform.TransformPoint(localPosition);
        }

        private void OnDestroy()
        {
            // 清理由运行时创建的 UnityEngine.Object，避免编辑器中残留隐藏资源。
            // Clean up runtime-created UnityEngine.Objects to avoid hidden editor leftovers.
            if (sprite != null)
            {
                Destroy(sprite);
            }

            if (texture != null)
            {
                Destroy(texture);
            }
        }
    }
}
