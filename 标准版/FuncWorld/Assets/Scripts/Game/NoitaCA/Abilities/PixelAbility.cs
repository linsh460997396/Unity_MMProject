using System.Collections.Generic;
using UnityEngine;

namespace NoitaCA
{
    public struct PixelAbilityContext
    {
        public PixelGrid Grid;
        public PixelWorldRenderer Renderer;
        public Camera Camera;
        public Transform Caster;
        public Vector3 AimWorld;
        public Vector2Int OriginCell;
        public Vector2Int AimCell;

        public Vector3 OriginWorld => Caster != null ? Caster.position : Vector3.zero;

        public Vector2 AimDirection
        {
            get
            {
                Vector3 delta = AimWorld - OriginWorld;
                delta.z = 0f;
                if (delta.sqrMagnitude <= 0.0001f)
                {
                    return Vector2.right;
                }

                return ((Vector2)delta).normalized;
            }
        }
    }

    public sealed class PixelProjectile
    {
        public PixelAbility Ability;
        public Vector3 Position;
        public Vector3 Velocity;
        public float Gravity;
        public float TimeRemaining;
        public float TrailTimer;
        public GameObject Visual;
    }

    public abstract class PixelAbility : ScriptableObject
    {
        [SerializeField] private string displayName = "Pixel Staff";
        [SerializeField] private Color32 primaryColor = new Color32(255, 255, 255, 255);
        [SerializeField] private float projectileSpeed = 9f;
        [SerializeField] private float projectileLifetime = 1.8f;
        [SerializeField] private float normalCooldown = 0.22f;
        [SerializeField] private float specialCooldown = 0.5f;
        [SerializeField] private float ultimateCooldown = 1.1f;

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Color32 PrimaryColor => primaryColor;
        public float ProjectileSpeed => Mathf.Max(0.1f, projectileSpeed);
        public float ProjectileLifetime => Mathf.Max(0.05f, projectileLifetime);
        public float NormalCooldown => Mathf.Max(0.01f, normalCooldown);
        public float SpecialCooldown => Mathf.Max(0.01f, specialCooldown);
        public float UltimateCooldown => Mathf.Max(0.01f, ultimateCooldown);

        protected void ConfigureBase(string abilityName, Color32 color, float speed, float lifetime, float normal, float special, float ultimate)
        {
            displayName = abilityName;
            primaryColor = color;
            projectileSpeed = speed;
            projectileLifetime = lifetime;
            normalCooldown = normal;
            specialCooldown = special;
            ultimateCooldown = ultimate;
        }

        public abstract void UseRuntimeDefaults();
        public virtual void CastNormal(PixelAbilityContext context, List<PixelProjectile> projectiles)
        {
            if (projectiles == null || context.Caster == null)
            {
                return;
            }

            Vector2 direction = context.AimDirection;
            PixelProjectile projectile = new PixelProjectile
            {
                Ability = this,
                Position = context.OriginWorld + (Vector3)(direction * 0.35f),
                Velocity = direction * ProjectileSpeed,
                Gravity = 0f,
                TimeRemaining = ProjectileLifetime,
                TrailTimer = 0f
            };
            projectiles.Add(projectile);
        }

        public abstract void CastSpecial(PixelAbilityContext context);
        public abstract void CastUltimate(PixelAbilityContext context);
        public abstract void LeaveProjectileTrail(PixelAbilityContext context, PixelProjectile projectile, Vector2Int cell);
        public abstract void ResolveImpact(PixelAbilityContext context, Vector2Int cell);

        public Sprite CreateProjectileSprite(int pixelsPerUnit)
        {
            const int size = 5;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = DisplayName + " Projectile Texture";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color32[] colors = new Color32[size * size];
            Color32 transparent = new Color32(0, 0, 0, 0);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = transparent;
            }

            PaintProjectileSprite(colors, size, size);
            texture.SetPixels32(colors);
            texture.Apply(false);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), Mathf.Max(1, pixelsPerUnit));
            sprite.name = DisplayName + " Projectile Sprite";
            return sprite;
        }

        protected virtual void PaintProjectileSprite(Color32[] colors, int width, int height)
        {
            FillRect(colors, width, height, 1, 1, 3, 3, primaryColor);
            SetPixel(colors, width, height, 2, 4, primaryColor);
        }

        protected void CastBeam(
            PixelAbilityContext context,
            int range,
            int halfWidth,
            System.Action<Vector2Int, int, int> applyCell,
            System.Action<Vector2Int, MaterialType> applyImpact)
        {
            if (context.Grid == null)
            {
                return;
            }

            Vector2 direction = context.AimDirection;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            for (int distance = 2; distance <= range; distance++)
            {
                Vector2 center = context.OriginCell + direction * distance;
                bool blocked = false;

                for (int widthOffset = -halfWidth; widthOffset <= halfWidth; widthOffset++)
                {
                    Vector2Int cell = new Vector2Int(
                        Mathf.RoundToInt(center.x + perpendicular.x * widthOffset),
                        Mathf.RoundToInt(center.y + perpendicular.y * widthOffset));

                    if (!context.Grid.InBounds(cell.x, cell.y))
                    {
                        blocked = true;
                        continue;
                    }

                    MaterialType material = context.Grid.GetMaterial(cell.x, cell.y);
                    if (context.Grid.IsSolid(cell.x, cell.y))
                    {
                        applyImpact?.Invoke(cell, material);
                        blocked = true;
                        continue;
                    }

                    applyCell?.Invoke(cell, distance, widthOffset);
                }

                if (blocked && distance > 4)
                {
                    break;
                }
            }
        }

        protected static void FillRect(Color32[] colors, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color)
        {
            for (int py = y; py < y + rectHeight; py++)
            {
                for (int px = x; px < x + rectWidth; px++)
                {
                    SetPixel(colors, width, height, px, py, color);
                }
            }
        }

        protected static void SetPixel(Color32[] colors, int width, int height, int x, int y, Color32 color)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            colors[y * width + x] = color;
        }
    }
}
