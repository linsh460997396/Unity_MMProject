// 职责：在压力测试场景中绘制区块边界和活跃区域覆盖层，帮助观察优化效果。
// Responsibility: Draws chunk boundaries and active-region overlays in the stress-test scene to visualize optimization behavior.
using UnityEngine;

namespace NoitaCA
{
    public sealed class StressTestDebugOverlay : MonoBehaviour
    {
        // 限制活跃像素矩形数量，避免调试绘制本身拖垮性能。
        // Limit active-pixel rectangles so debug drawing does not dominate performance.
        private const int MaxActivePixelRects = 8000;
        private static Texture2D whiteTexture;

        private StressTestBootstrap bootstrap;
        private Camera targetCamera;

        public void Initialize(StressTestBootstrap stressTestBootstrap, Camera cameraToUse)
        {
            // 覆盖层只读取启动器、网格和相机状态。
            // The overlay only reads bootstrap, grid, and camera state.
            bootstrap = stressTestBootstrap;
            targetCamera = cameraToUse;
            EnsureTexture();
        }

        private void OnGUI()
        {
            if (bootstrap == null || bootstrap.Grid == null || bootstrap.Renderer == null || targetCamera == null)
            {
                return;
            }

            // 根据配置独立绘制区块线和活跃区域。
            // Draw chunk lines and active regions independently based on config.
            StressTestConfig config = bootstrap.Config;
            if (config.showChunkBoundaries)
            {
                DrawChunkBoundaries();
            }

            if (config.showActiveRegions)
            {
                DrawActiveRegions();
            }
        }

        private void DrawChunkBoundaries()
        {
            // 按当前区块尺寸把整张网格划成 GUI 矩形。
            // Split the grid into GUI rectangles using the current chunk size.
            PixelGrid grid = bootstrap.Grid;
            Color lineColor = new Color(1f, 1f, 1f, 0.22f);
            for (int cy = 0; cy < grid.ChunkRows; cy++)
            {
                for (int cx = 0; cx < grid.ChunkColumns; cx++)
                {
                    Rect rect = CellRectToGuiRect(cx * grid.ChunkSize, cy * grid.ChunkSize, grid.ChunkSize, grid.ChunkSize);
                    DrawRectOutline(rect, lineColor, 1f);
                }
            }
        }

        private void DrawActiveRegions()
        {
            PixelGrid grid = bootstrap.Grid;
            if (bootstrap.Mode == PixelSimulationMode.ChunkBased)
            {
                // 区块模式下高亮活跃区块。
                // In chunk mode, highlight active chunks.
                for (int i = 0; i < grid.ActiveChunkCount; i++)
                {
                    Vector2Int chunk = grid.GetActiveChunk(i);
                    Rect rect = CellRectToGuiRect(chunk.x * grid.ChunkSize, chunk.y * grid.ChunkSize, grid.ChunkSize, grid.ChunkSize);
                    DrawRect(rect, new Color(0.1f, 1f, 0.35f, 0.13f));
                    DrawRectOutline(rect, new Color(0.2f, 1f, 0.45f, 0.55f), 2f);
                }

                return;
            }

            if (bootstrap.Mode == PixelSimulationMode.ActivePixels)
            {
                // 活跃像素模式下高亮被唤醒的单格，数量做上限保护。
                // In active-pixel mode, highlight awakened cells with a count cap.
                int count = Mathf.Min(MaxActivePixelRects, grid.ActivePixelCount);
                for (int i = 0; i < count; i++)
                {
                    Vector2Int cell = grid.GetActiveCell(i);
                    Rect rect = CellRectToGuiRect(cell.x, cell.y, 1, 1);
                    DrawRect(rect, new Color(0.1f, 1f, 0.35f, 0.3f));
                }
            }
        }

        private Rect CellRectToGuiRect(int cellX, int cellY, int cellWidth, int cellHeight)
        {
            // 网格坐标先转世界坐标，再转屏幕坐标，最后翻转成 IMGUI 的左上原点坐标。
            // Grid coordinates convert to world, then screen, then flip into IMGUI's top-left origin.
            PixelWorldRenderer renderer = bootstrap.Renderer;
            float ppu = Mathf.Max(1, renderer.PixelsPerUnit);
            Vector3 minWorld = renderer.transform.TransformPoint(new Vector3(cellX / ppu, cellY / ppu, 0f));
            Vector3 maxWorld = renderer.transform.TransformPoint(new Vector3((cellX + cellWidth) / ppu, (cellY + cellHeight) / ppu, 0f));
            Vector3 minScreen = targetCamera.WorldToScreenPoint(minWorld);
            Vector3 maxScreen = targetCamera.WorldToScreenPoint(maxWorld);

            float xMin = Mathf.Min(minScreen.x, maxScreen.x);
            float xMax = Mathf.Max(minScreen.x, maxScreen.x);
            float yMin = Screen.height - Mathf.Max(minScreen.y, maxScreen.y);
            float yMax = Screen.height - Mathf.Min(minScreen.y, maxScreen.y);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static void DrawRectOutline(Rect rect, Color color, float thickness)
        {
            // 用四个细矩形组成边框，避免依赖额外绘图库。
            // Compose the outline from four thin rectangles without extra drawing libraries.
            DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            // 复用 1x1 白色纹理，通过 GUI.color 控制颜色和透明度。
            // Reuse a 1x1 white texture and tint it through GUI.color.
            EnsureTexture();
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previous;
        }

        private static void EnsureTexture()
        {
            if (whiteTexture != null)
            {
                return;
            }

            // 延迟创建白色纹理，只在首次绘制时分配。
            // Lazily create the white texture only when first drawn.
            whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply(false);
        }
    }
}
