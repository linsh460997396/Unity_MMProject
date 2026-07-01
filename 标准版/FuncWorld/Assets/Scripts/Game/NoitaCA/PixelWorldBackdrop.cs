using UnityEngine;

namespace NoitaCA
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelWorldBackdrop : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Texture2D texture;
        private Sprite sprite;

        public void Initialize(Vector2 worldSize, int worldWidth, int worldHeight)
        {
            ReleaseGeneratedObjects();

            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = -30;

            int textureWidth = Mathf.Clamp(worldWidth / 2, 96, 256);
            int textureHeight = Mathf.Clamp(worldHeight / 2, 54, 160);
            Color32[] colors = new Color32[textureWidth * textureHeight];

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    colors[y * textureWidth + x] = EvaluateBackdropColor(x, y, textureWidth, textureHeight);
                }
            }

            texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                name = "Alchemy Backdrop Texture",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(colors);
            texture.Apply(false);

            Vector2 targetWorldSize = worldSize + new Vector2(2.5f, 1.6f);
            float pixelsPerUnit = textureWidth / Mathf.Max(0.01f, targetWorldSize.x);
            sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), Vector2.zero, pixelsPerUnit);
            sprite.name = "Alchemy Backdrop Sprite";
            spriteRenderer.sprite = sprite;

            transform.localPosition = new Vector3(-targetWorldSize.x * 0.5f, -targetWorldSize.y * 0.5f, 0.25f);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private static Color32 EvaluateBackdropColor(int x, int y, int width, int height)
        {
            float u = width <= 1 ? 0f : x / (float)(width - 1);
            float v = height <= 1 ? 0f : y / (float)(height - 1);
            float grain = Hash01(x, y, 9);
            float slowNoise = Mathf.Sin(u * Mathf.PI * 5.5f + v * 3.1f) * 0.5f + 0.5f;

            Color32 bottom = new Color32(7, 9, 18, 255);
            Color32 middle = new Color32(14, 27, 34, 255);
            Color32 top = new Color32(24, 19, 38, 255);
            Color32 color = v < 0.55f
                ? Mix(bottom, middle, v / 0.55f)
                : Mix(middle, top, (v - 0.55f) / 0.45f);

            float lowerSilhouette = 0.17f + Mathf.Sin(u * Mathf.PI * 2.7f + 0.4f) * 0.055f + Mathf.Sin(u * Mathf.PI * 9.4f) * 0.025f;
            float upperSilhouette = 0.84f + Mathf.Sin(u * Mathf.PI * 3.1f + 1.9f) * 0.045f + Mathf.Sin(u * Mathf.PI * 11.0f) * 0.018f;
            if (v < lowerSilhouette || v > upperSilhouette)
            {
                color = Mix(color, new Color32(3, 5, 10, 255), 0.78f);
            }

            float band = Mathf.Sin(y * 0.28f + Mathf.Floor(x * 0.08f) * 0.61f);
            if (band > 0.76f)
            {
                color = Mix(color, new Color32(34, 53, 49, 255), 0.24f);
            }
            else if (band < -0.8f)
            {
                color = Mix(color, new Color32(43, 27, 51, 255), 0.20f);
            }

            if (grain > 0.986f && v > 0.32f && v < 0.88f)
            {
                color = Mix(color, new Color32(91, 161, 148, 255), 0.42f);
            }

            return Mix(color, new Color32(16, 30, 38, 255), slowNoise * 0.08f);
        }

        private static float Hash01(int x, int y, int salt)
        {
            unchecked
            {
                uint n = (uint)(x * 374761393) ^ (uint)(y * 668265263) ^ ((uint)salt * 2246822519u);
                n = (n ^ (n >> 13)) * 1274126177u;
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

        private void OnDestroy()
        {
            ReleaseGeneratedObjects();
        }

        private void ReleaseGeneratedObjects()
        {
            if (sprite != null)
            {
                DestroyObject(sprite);
                sprite = null;
            }

            if (texture != null)
            {
                DestroyObject(texture);
                texture = null;
            }
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
    }
}
