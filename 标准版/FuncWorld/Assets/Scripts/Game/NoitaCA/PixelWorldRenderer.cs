using UnityEngine;

namespace NoitaCA
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelWorldRenderer : MonoBehaviour
    {
        private static readonly PixelWorldRenderSettings FallbackSettings = PixelWorldRenderSettings.CreateAlchemyDefault();

        private SpriteRenderer spriteRenderer;
        private Texture2D texture;
        private Sprite sprite;
        private Color32[] pixels;
        private Color32[] visiblePixels;
        private PixelGrid grid;
        private PixelWorldRenderSettings renderSettings = PixelWorldRenderSettings.CreateAlchemyDefault();

        public int PixelsPerUnit { get; private set; } = 16;

        public Vector2 WorldSize
        {
            get
            {
                if (grid == null)
                {
                    return Vector2.zero;
                }

                return new Vector2(grid.Width / (float)PixelsPerUnit, grid.Height / (float)PixelsPerUnit);
            }
        }

        public void Initialize(PixelGrid worldGrid, int pixelsPerUnit)
        {
            Initialize(worldGrid, pixelsPerUnit, PixelWorldRenderSettings.CreateAlchemyDefault());
        }

        public void Initialize(PixelGrid worldGrid, int pixelsPerUnit, PixelWorldRenderSettings settings)
        {
            grid = worldGrid;
            PixelsPerUnit = Mathf.Max(1, pixelsPerUnit);
            renderSettings = settings != null ? settings : PixelWorldRenderSettings.CreateAlchemyDefault();

            spriteRenderer = GetComponent<SpriteRenderer>();
            pixels = new Color32[grid.Width * grid.Height];
            visiblePixels = new Color32[grid.Width * grid.Height];

            if (sprite != null)
            {
                DestroyObject(sprite);
            }

            if (texture != null)
            {
                DestroyObject(texture);
            }

            texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false)
            {
                name = "Pixel World Texture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, grid.Width, grid.Height),
                Vector2.zero,
                PixelsPerUnit);
            sprite.name = "Pixel World Sprite";
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = 0;
        }

        public void Render()
        {
            if (grid == null || texture == null)
            {
                return;
            }

            RenderRows(0, grid.Height - 1);
        }

        public void RenderVisible(Camera targetCamera, int verticalPaddingCells)
        {
            if (grid == null || texture == null || targetCamera == null || !targetCamera.orthographic)
            {
                Render();
                return;
            }

            float halfHeight = targetCamera.orthographicSize;
            float halfWidth = halfHeight * targetCamera.aspect;
            Vector3 cameraPosition = targetCamera.transform.position;
            Vector2Int minCell = WorldToCell(new Vector3(cameraPosition.x - halfWidth, cameraPosition.y - halfHeight, 0f));
            Vector2Int maxCell = WorldToCell(new Vector3(cameraPosition.x + halfWidth, cameraPosition.y + halfHeight, 0f));
            int padding = Mathf.Max(0, verticalPaddingCells);
            RenderRows(minCell.y - padding, maxCell.y + padding);
        }

        private void RenderRows(int minY, int maxY)
        {
            if (grid == null || texture == null)
            {
                return;
            }

            int startY = Mathf.Clamp(minY, 0, grid.Height - 1);
            int endY = Mathf.Clamp(maxY, 0, grid.Height - 1);
            if (endY < startY)
            {
                return;
            }

            for (int y = startY; y <= endY; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Pixel pixel = grid.GetCell(x, y);
                    Color32 color = EvaluateVisualColor(pixel, x, y, grid.Width, grid.Height, renderSettings);
                    if (renderSettings != null && renderSettings.EnableAlchemyPalette)
                    {
                        color = ApplyNeighborhoodLighting(color, pixel, x, y);
                    }

                    pixels[y * grid.Width + x] = color;
                }
            }

            int rowCount = endY - startY + 1;
            int visibleLength = grid.Width * rowCount;
            if (visiblePixels == null || visiblePixels.Length != visibleLength)
            {
                visiblePixels = new Color32[visibleLength];
            }

            System.Array.Copy(pixels, startY * grid.Width, visiblePixels, 0, visibleLength);
            texture.SetPixels32(0, startY, grid.Width, rowCount, visiblePixels, 0);
            texture.Apply(false);
        }

        public static Color32 EvaluateVisualColor(Pixel pixel, int x, int y, int gridWidth, int gridHeight, PixelWorldRenderSettings settings)
        {
            PixelWorldRenderSettings safeSettings = settings ?? FallbackSettings;
            if (!safeSettings.EnableAlchemyPalette)
            {
                return pixel.Color;
            }

            float variation = Mathf.Clamp01(safeSettings.MaterialVariation);
            float depth = gridHeight <= 1 ? 0f : 1f - y / (float)(gridHeight - 1);
            float grain = Hash01(x, y, 17);
            float grainB = Hash01(x, y, 83);
            float strata = Mathf.Sin(y * 0.38f + Mathf.Floor(x * 0.0625f) * 0.73f);

            switch (pixel.MaterialType)
            {
                case MaterialType.Air:
                    return ShadeAir(x, y, gridWidth, gridHeight, safeSettings, grain, grainB);
                case MaterialType.Stone:
                    return ShadeStone(depth, grain, grainB, strata, variation, safeSettings);
                case MaterialType.Sand:
                    return ShadeSand(depth, grain, strata, variation);
                case MaterialType.Water:
                    return ShadeWater(x, y, grain, variation, safeSettings);
                case MaterialType.Smoke:
                    return ShadeSmoke(pixel, depth, grain, variation);
                case MaterialType.Fire:
                    return ShadeFire(pixel, x, y, grain, safeSettings);
                case MaterialType.Wood:
                    return ShadeWood(depth, grain, strata, variation);
                case MaterialType.Ash:
                    return ShadeAsh(depth, grain, variation);
                case MaterialType.Poison:
                    return ShadePoison(x, y, grain, safeSettings);
                case MaterialType.Ice:
                    return ShadeIce(depth, grain, strata, variation, safeSettings);
                case MaterialType.Lava:
                    return ShadeLava(pixel, x, y, grain, safeSettings);
                case MaterialType.Debris:
                    return ShadeDebris(depth, grain, strata, variation);
                default:
                    return pixel.Color;
            }
        }

        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            return new Vector2Int(
                Mathf.FloorToInt(localPosition.x * PixelsPerUnit),
                Mathf.FloorToInt(localPosition.y * PixelsPerUnit));
        }

        public Vector3 CellToWorldCenter(int x, int y)
        {
            Vector3 localPosition = new Vector3(
                (x + 0.5f) / PixelsPerUnit,
                (y + 0.5f) / PixelsPerUnit,
                0f);
            return transform.TransformPoint(localPosition);
        }

        private Color32 ApplyNeighborhoodLighting(Color32 color, Pixel pixel, int x, int y)
        {
            MaterialDefinition definition = MaterialDatabase.Get(pixel.MaterialType);
            if (definition.IsAir)
            {
                return color;
            }

            float edge = Mathf.Clamp01(renderSettings.EdgeLighting);
            if (edge <= 0f)
            {
                return color;
            }

            bool airAbove = IsAirOrOutOfBounds(x, y + 1);
            bool airLeft = IsAirOrOutOfBounds(x - 1, y);
            bool airRight = IsAirOrOutOfBounds(x + 1, y);
            bool airBelow = IsAirOrOutOfBounds(x, y - 1);

            if (airAbove)
            {
                Color32 glint = pixel.MaterialType == MaterialType.Lava || pixel.MaterialType == MaterialType.Fire
                    ? new Color32(255, 226, 138, 255)
                    : new Color32(178, 229, 220, 255);
                color = Mix(color, glint, 0.16f * edge);
            }

            if (airLeft)
            {
                color = Adjust(color, 1f + 0.07f * edge);
            }

            if (airRight || airBelow)
            {
                color = Adjust(color, 1f - 0.11f * edge);
            }

            return color;
        }

        private bool IsAirOrOutOfBounds(int x, int y)
        {
            if (grid == null || !grid.InBounds(x, y))
            {
                return true;
            }

            return MaterialDatabase.Get(grid.GetCell(x, y).MaterialType).IsAir;
        }

        private static Color32 ShadeAir(int x, int y, int width, int height, PixelWorldRenderSettings settings, float grain, float grainB)
        {
            float vertical = height <= 1 ? 0f : y / (float)(height - 1);
            float horizontal = width <= 1 ? 0f : x / (float)(width - 1);
            Color32 color = Mix(new Color32(4, 7, 13, 255), new Color32(12, 20, 30, 255), Mathf.Clamp01(vertical * 0.75f + settings.AmbientVeil * 0.35f));

            if (grain > 0.988f && y > height * 0.45f)
            {
                color = Mix(color, new Color32(38, 64, 72, 255), 0.28f);
            }

            float haze = Mathf.Sin(horizontal * Mathf.PI * 3.2f + vertical * 4.7f) * 0.5f + 0.5f;
            return Mix(color, new Color32(16, 26, 34, 255), haze * grainB * 0.08f);
        }

        private static Color32 ShadeStone(float depth, float grain, float grainB, float strata, float variation, PixelWorldRenderSettings settings)
        {
            Color32 color = Mix(new Color32(78, 72, 66, 255), new Color32(36, 38, 48, 255), depth * Mathf.Clamp01(settings.DepthShadow));
            color = Mix(color, grainB > 0.52f ? new Color32(105, 80, 58, 255) : new Color32(47, 72, 82, 255), variation * (0.08f + grain * 0.16f));

            if (strata > 0.68f)
            {
                color = Mix(color, new Color32(120, 95, 71, 255), variation * 0.22f);
            }
            else if (strata < -0.74f)
            {
                color = Mix(color, new Color32(43, 85, 91, 255), variation * 0.18f);
            }

            if (grain > 0.965f)
            {
                color = Mix(color, new Color32(143, 169, 138, 255), variation * 0.4f);
            }

            return color;
        }

        private static Color32 ShadeSand(float depth, float grain, float strata, float variation)
        {
            Color32 color = Mix(new Color32(190, 137, 72, 255), new Color32(230, 197, 116, 255), grain * 0.65f + 0.25f);
            color = Mix(color, new Color32(86, 63, 50, 255), depth * 0.18f);
            return strata > 0.55f ? Mix(color, new Color32(244, 216, 142, 255), variation * 0.26f) : color;
        }

        private static Color32 ShadeWater(int x, int y, float grain, float variation, PixelWorldRenderSettings settings)
        {
            float ripple = Mathf.Sin(x * 0.31f + y * 0.17f) * 0.5f + 0.5f;
            Color32 color = Mix(new Color32(27, 82, 142, 230), new Color32(48, 172, 214, 245), ripple * 0.55f + grain * 0.25f);
            return Mix(color, new Color32(105, 232, 235, 255), variation * settings.GlowBoost * 0.45f);
        }

        private static Color32 ShadeSmoke(Pixel pixel, float depth, float grain, float variation)
        {
            float fade = pixel.Lifetime > 0 ? Mathf.Clamp01(pixel.Lifetime / 150f) : 0.72f;
            Color32 color = Mix(new Color32(58, 60, 70, 120), new Color32(132, 140, 144, 190), grain * variation + 0.15f);
            color.a = (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(72f, 176f, fade) * (1f - depth * 0.25f)), 0, 255);
            return color;
        }

        private static Color32 ShadeFire(Pixel pixel, int x, int y, float grain, PixelWorldRenderSettings settings)
        {
            float ember = Mathf.Sin((x * 13 + y * 7 + pixel.Lifetime) * 0.19f) * 0.5f + 0.5f;
            Color32 color = Mix(new Color32(178, 32, 16, 255), new Color32(255, 121, 32, 255), ember);
            return Mix(color, new Color32(255, 235, 116, 255), Mathf.Clamp01(settings.GlowBoost + grain * 0.42f));
        }

        private static Color32 ShadeWood(float depth, float grain, float strata, float variation)
        {
            Color32 color = Mix(new Color32(82, 46, 31, 255), new Color32(146, 88, 45, 255), grain * 0.6f + 0.25f);
            if (strata > 0.25f)
            {
                color = Mix(color, new Color32(184, 128, 65, 255), variation * 0.24f);
            }

            return Mix(color, new Color32(36, 25, 24, 255), depth * 0.12f);
        }

        private static Color32 ShadeAsh(float depth, float grain, float variation)
        {
            Color32 color = Mix(new Color32(50, 49, 55, 255), new Color32(94, 88, 78, 255), grain * variation + 0.12f);
            return Mix(color, new Color32(24, 23, 26, 255), depth * 0.18f);
        }

        private static Color32 ShadePoison(int x, int y, float grain, PixelWorldRenderSettings settings)
        {
            float pulse = Mathf.Sin(x * 0.27f + y * 0.41f) * 0.5f + 0.5f;
            Color32 color = Mix(new Color32(38, 132, 54, 235), new Color32(84, 238, 82, 255), pulse * 0.5f + grain * 0.25f);
            return Mix(color, new Color32(183, 255, 112, 255), settings.GlowBoost * 0.48f);
        }

        private static Color32 ShadeIce(float depth, float grain, float strata, float variation, PixelWorldRenderSettings settings)
        {
            Color32 color = Mix(new Color32(76, 142, 178, 255), new Color32(169, 237, 255, 255), grain * 0.45f + 0.4f);
            if (strata < -0.3f)
            {
                color = Mix(color, new Color32(224, 255, 246, 255), variation * 0.28f);
            }

            return Mix(color, new Color32(34, 57, 79, 255), depth * settings.DepthShadow * 0.32f);
        }

        private static Color32 ShadeLava(Pixel pixel, int x, int y, float grain, PixelWorldRenderSettings settings)
        {
            float heat = Mathf.Clamp01(pixel.Temperature / 760f);
            float river = Mathf.Sin(x * 0.23f - y * 0.31f + pixel.Temperature * 0.011f) * 0.5f + 0.5f;
            Color32 color = Mix(new Color32(126, 27, 18, 255), new Color32(255, 88, 24, 255), river * 0.55f + heat * 0.25f);
            return Mix(color, new Color32(255, 212, 76, 255), Mathf.Clamp01(settings.GlowBoost * 0.82f + grain * 0.22f));
        }

        private static Color32 ShadeDebris(float depth, float grain, float strata, float variation)
        {
            Color32 color = Mix(new Color32(84, 67, 52, 255), new Color32(145, 117, 82, 255), grain * 0.62f + 0.12f);
            if (strata > 0.62f)
            {
                color = Mix(color, new Color32(86, 111, 98, 255), variation * 0.24f);
            }

            return Mix(color, new Color32(38, 35, 35, 255), depth * 0.18f);
        }

        private static float Hash01(int x, int y, int salt)
        {
            unchecked
            {
                uint n = (uint)(x * 73856093) ^ (uint)(y * 19349663) ^ (uint)(salt * 83492791);
                n ^= n >> 13;
                n *= 1274126177u;
                n ^= n >> 16;
                return (n & 0x00FFFFFF) / 16777215f;
            }
        }

        private static Color32 Mix(Color32 from, Color32 to, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.r, to.r, t)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.g, to.g, t)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.b, to.b, t)), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(from.a, to.a, t)), 0, 255));
        }

        private static Color32 Adjust(Color32 color, float multiplier)
        {
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * multiplier), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * multiplier), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * multiplier), 0, 255),
                color.a);
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void OnDestroy()
        {
            if (sprite != null)
            {
                DestroyObject(sprite);
            }

            if (texture != null)
            {
                DestroyObject(texture);
            }
        }
    }
}
