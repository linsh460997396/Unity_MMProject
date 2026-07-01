using System.Collections.Generic;
using UnityEngine;

namespace NoitaCA
{
    public sealed class PixelRewardBurstEffect : MonoBehaviour
    {
        private sealed class Spark
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 Velocity;
            public float AngularVelocity;
            public float StartScale;
        }

        private readonly List<Spark> sparks = new List<Spark>(18);
        private Texture2D sparkleTexture;
        private Texture2D coinTexture;
        private Sprite sparkleSprite;
        private Sprite coinSprite;
        private float lifetime = 0.85f;
        private float age;

        public static void Spawn(Vector3 worldPosition, Transform parent, int pixelsPerUnit)
        {
            GameObject effectObject = new GameObject("Reward Burst Visual");
            effectObject.transform.SetParent(parent, true);
            effectObject.transform.position = worldPosition;

            PixelRewardBurstEffect effect = effectObject.AddComponent<PixelRewardBurstEffect>();
            effect.Initialize(Mathf.Max(1, pixelsPerUnit));
        }

        private void Initialize(int pixelsPerUnit)
        {
            sparkleSprite = CreateSparkleSprite(pixelsPerUnit);
            coinSprite = CreateCoinSprite(pixelsPerUnit);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2f / 12f;
                float speed = Mathf.Lerp(0.9f, 1.65f, Hash01(i, 17));
                Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f + 0.55f, 0f) * speed;
                CreateSpark(i, sparkleSprite, velocity, Mathf.Lerp(0.7f, 1.15f, Hash01(i, 23)), 0.02f);
            }

            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Lerp(0.3f, Mathf.PI - 0.3f, i / 5f);
                Vector3 velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) + 0.35f, 0f) * Mathf.Lerp(0.55f, 1.1f, Hash01(i, 41));
                CreateSpark(i + 20, coinSprite, velocity, Mathf.Lerp(0.85f, 1.25f, Hash01(i, 47)), 0.015f);
            }
        }

        private void CreateSpark(int index, Sprite sprite, Vector3 velocity, float scale, float startJitter)
        {
            GameObject sparkObject = new GameObject("Reward Spark");
            sparkObject.transform.SetParent(transform, false);
            sparkObject.transform.localPosition = new Vector3(
                Mathf.Lerp(-startJitter, startJitter, Hash01(index, 61)),
                Mathf.Lerp(-startJitter, startJitter, Hash01(index, 67)),
                0f);

            SpriteRenderer renderer = sparkObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 14;

            sparkObject.transform.localScale = Vector3.one * scale;
            sparks.Add(new Spark
            {
                Transform = sparkObject.transform,
                Renderer = renderer,
                Velocity = velocity,
                AngularVelocity = Mathf.Lerp(-240f, 240f, Hash01(index, 71)),
                StartScale = scale
            });
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, lifetime));
            float fade = 1f - t;
            float gravity = 2.8f;

            for (int i = 0; i < sparks.Count; i++)
            {
                Spark spark = sparks[i];
                if (spark.Transform == null || spark.Renderer == null)
                {
                    continue;
                }

                spark.Velocity += Vector3.down * gravity * Time.deltaTime;
                spark.Transform.localPosition += spark.Velocity * Time.deltaTime;
                spark.Transform.Rotate(0f, 0f, spark.AngularVelocity * Time.deltaTime);

                float pop = Mathf.Sin(Mathf.Clamp01(t * 1.8f) * Mathf.PI);
                spark.Transform.localScale = Vector3.one * spark.StartScale * Mathf.Lerp(0.45f, 1.35f, pop) * Mathf.Lerp(1f, 0.25f, t);

                Color color = spark.Renderer.color;
                color.a = fade * fade;
                spark.Renderer.color = color;
            }

            if (age >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private Sprite CreateSparkleSprite(int pixelsPerUnit)
        {
            const int size = 7;
            sparkleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            sparkleTexture.name = "Reward Sparkle Texture";
            sparkleTexture.filterMode = FilterMode.Point;
            sparkleTexture.wrapMode = TextureWrapMode.Clamp;

            Color32[] colors = new Color32[size * size];
            Color32 clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = clear;
            }

            Color32 gold = new Color32(255, 214, 86, 255);
            Color32 white = new Color32(255, 250, 190, 255);
            Color32 cyan = new Color32(116, 234, 255, 230);
            Set(colors, size, 3, 0, gold);
            Set(colors, size, 3, 1, gold);
            Set(colors, size, 3, 2, white);
            Set(colors, size, 0, 3, cyan);
            Set(colors, size, 1, 3, gold);
            Set(colors, size, 2, 3, white);
            Set(colors, size, 3, 3, white);
            Set(colors, size, 4, 3, white);
            Set(colors, size, 5, 3, gold);
            Set(colors, size, 6, 3, cyan);
            Set(colors, size, 3, 4, white);
            Set(colors, size, 3, 5, gold);
            Set(colors, size, 3, 6, gold);

            sparkleTexture.SetPixels32(colors);
            sparkleTexture.Apply(false);
            return Sprite.Create(sparkleTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private Sprite CreateCoinSprite(int pixelsPerUnit)
        {
            const int size = 5;
            coinTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            coinTexture.name = "Reward Coin Texture";
            coinTexture.filterMode = FilterMode.Point;
            coinTexture.wrapMode = TextureWrapMode.Clamp;

            Color32[] colors = new Color32[size * size];
            Color32 clear = new Color32(0, 0, 0, 0);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = clear;
            }

            Color32 edge = new Color32(186, 122, 28, 255);
            Color32 gold = new Color32(255, 196, 54, 255);
            Color32 shine = new Color32(255, 244, 154, 255);
            Set(colors, size, 1, 0, edge);
            Set(colors, size, 2, 0, gold);
            Set(colors, size, 3, 0, edge);
            Set(colors, size, 0, 1, edge);
            Set(colors, size, 1, 1, gold);
            Set(colors, size, 2, 1, shine);
            Set(colors, size, 3, 1, gold);
            Set(colors, size, 4, 1, edge);
            Set(colors, size, 0, 2, gold);
            Set(colors, size, 1, 2, shine);
            Set(colors, size, 2, 2, gold);
            Set(colors, size, 3, 2, gold);
            Set(colors, size, 4, 2, edge);
            Set(colors, size, 0, 3, edge);
            Set(colors, size, 1, 3, gold);
            Set(colors, size, 2, 3, gold);
            Set(colors, size, 3, 3, edge);
            Set(colors, size, 4, 3, edge);
            Set(colors, size, 1, 4, edge);
            Set(colors, size, 2, 4, edge);
            Set(colors, size, 3, 4, edge);

            coinTexture.SetPixels32(colors);
            coinTexture.Apply(false);
            return Sprite.Create(coinTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        private void OnDestroy()
        {
            DestroyObject(sparkleSprite);
            DestroyObject(coinSprite);
            DestroyObject(sparkleTexture);
            DestroyObject(coinTexture);
        }

        private static void Set(Color32[] colors, int width, int x, int y, Color32 color)
        {
            if (x < 0 || x >= width || y < 0 || y >= width)
            {
                return;
            }

            colors[y * width + x] = color;
        }

        private static float Hash01(int a, int b)
        {
            unchecked
            {
                uint n = (uint)(a * 73856093) ^ (uint)(b * 19349663);
                n ^= n >> 13;
                n *= 1274126177u;
                n ^= n >> 16;
                return (n & 0x00FFFFFF) / 16777215f;
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
