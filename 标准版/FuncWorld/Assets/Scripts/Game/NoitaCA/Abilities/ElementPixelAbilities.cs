using UnityEngine;

namespace NoitaCA
{
    public sealed class FirePixelAbility : PixelAbility
    {
        public override void UseRuntimeDefaults()
        {
            ConfigureBase("Fire Staff", new Color32(255, 116, 30, 255), 9.5f, 1.8f, 0.2f, 0.46f, 1.05f);
        }

        public override void CastSpecial(PixelAbilityContext context)
        {
            CastBeam(
                context,
                126,
                2,
                (cell, distance, widthOffset) =>
                {
                    context.Grid.SetMaterial(cell.x, cell.y, widthOffset == 0 || (distance & 1) == 0 ? MaterialType.Fire : MaterialType.Smoke);
                },
                (cell, material) =>
                {
                    if (material == MaterialType.Wood)
                    {
                        PixelReactionUtility.Ignite(context.Grid, cell, 4, 12f);
                    }
                    else
                    {
                        context.Grid.SpawnMaterial(cell, MaterialType.Fire, 8, 3);
                        context.Grid.SpawnMaterial(cell, MaterialType.Smoke, 6, 3);
                        PixelCreatureRegistry.ApplyDamageInCircle(cell, 4f, 12f, 0f);
                    }
                });
        }

        public override void CastUltimate(PixelAbilityContext context)
        {
            CastCone(context, 58, 43, 96f);
        }

        public override void LeaveProjectileTrail(PixelAbilityContext context, PixelProjectile projectile, Vector2Int cell)
        {
            projectile.TrailTimer = 0.055f;
            MaterialType material = ((cell.x + cell.y) % 3) == 0 ? MaterialType.Fire : MaterialType.Smoke;
            context.Grid.SetMaterial(cell.x, cell.y, material);
            PixelCreatureRegistry.ApplyDamageInCircle(cell, 2f, 2f, 0f);
        }

        public override void ResolveImpact(PixelAbilityContext context, Vector2Int cell)
        {
            if (!context.Grid.InBounds(cell.x, cell.y))
            {
                return;
            }

            if (context.Grid.GetMaterial(cell.x, cell.y) == MaterialType.Wood)
            {
                PixelReactionUtility.Ignite(context.Grid, cell, 6, 18f);
                context.Grid.SpawnMaterial(cell, MaterialType.Fire, 16, 4);
                context.Grid.SpawnMaterial(cell, MaterialType.Smoke, 10, 5);
                return;
            }

            PixelReactionUtility.Explosion(context.Grid, cell, 4, 28f, 3.6f);
            PixelReactionUtility.Ignite(context.Grid, cell, 6, 12f);
        }

        protected override void PaintProjectileSprite(Color32[] colors, int width, int height)
        {
            Color32 hot = new Color32(255, 225, 86, 255);
            Color32 fire = new Color32(255, 96, 26, 255);
            Color32 ember = new Color32(170, 31, 15, 255);
            SetPixel(colors, width, height, 2, 4, hot);
            FillRect(colors, width, height, 1, 2, 3, 2, fire);
            FillRect(colors, width, height, 2, 1, 2, 2, ember);
            SetPixel(colors, width, height, 0, 2, ember);
            SetPixel(colors, width, height, 2, 2, hot);
        }

        private void CastCone(PixelAbilityContext context, int range, int rays, float coneDegrees)
        {
            Vector2 direction = context.AimDirection;
            float baseAngle = Mathf.Atan2(direction.y, direction.x);
            float coneRadians = coneDegrees * Mathf.Deg2Rad;

            for (int ray = 0; ray < rays; ray++)
            {
                float t = rays <= 1 ? 0.5f : ray / (float)(rays - 1);
                float angle = baseAngle + Mathf.Lerp(-coneRadians * 0.5f, coneRadians * 0.5f, t);
                Vector2 rayDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                for (int distance = 2; distance <= range; distance += 2)
                {
                    Vector2Int cell = new Vector2Int(
                        Mathf.RoundToInt(context.OriginCell.x + rayDirection.x * distance),
                        Mathf.RoundToInt(context.OriginCell.y + rayDirection.y * distance));

                    if (!context.Grid.InBounds(cell.x, cell.y))
                    {
                        break;
                    }

                    if (context.Grid.GetMaterial(cell.x, cell.y) == MaterialType.Wood)
                    {
                        PixelReactionUtility.Ignite(context.Grid, cell, 3, 8f);
                    }
                    else if (!context.Grid.IsSolid(cell.x, cell.y))
                    {
                        context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Fire);
                        if ((distance & 3) == 0)
                        {
                            context.Grid.SpawnMaterial(cell, MaterialType.Smoke, 4, 2);
                        }
                    }
                }
            }
        }
    }

    public sealed class FrostPixelAbility : PixelAbility
    {
        public override void UseRuntimeDefaults()
        {
            ConfigureBase("Frost Staff", new Color32(132, 218, 255, 255), 10f, 1.9f, 0.24f, 0.52f, 1.12f);
        }

        public override void CastSpecial(PixelAbilityContext context)
        {
            CastBeam(
                context,
                156,
                1,
                (cell, distance, widthOffset) =>
                {
                    PixelReactionUtility.ExtinguishCircle(context.Grid, cell, 1);
                    context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Water);
                    if (widthOffset == 0 && (distance % 5) == 0)
                    {
                        PixelReactionUtility.FreezeSolidEdges(context.Grid, cell, 1);
                    }
                },
                (cell, material) =>
                {
                    PixelReactionUtility.ExtinguishCircle(context.Grid, cell, 4);
                    PixelReactionUtility.FreezeSolidEdges(context.Grid, cell, 3);
                    context.Grid.SpawnMaterial(cell, MaterialType.Water, 12, 3);
                    PixelCreatureRegistry.ApplyDamageInCircle(cell, 4f, 8f, 0f);
                });
        }

        public override void CastUltimate(PixelAbilityContext context)
        {
            CastBeam(
                context,
                76,
                5,
                (cell, distance, widthOffset) =>
                {
                    PixelReactionUtility.ExtinguishCircle(context.Grid, cell, 2);
                    if (!context.Grid.IsSolid(cell.x, cell.y))
                    {
                        context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Water);
                    }

                    if ((distance % 5) == 0)
                    {
                        PixelReactionUtility.FreezeSolidEdges(context.Grid, cell, 2);
                    }
                },
                (cell, material) =>
                {
                    PixelReactionUtility.ExtinguishCircle(context.Grid, cell, 5);
                    PixelReactionUtility.FreezeSolidEdges(context.Grid, cell, 4);
                });
        }

        public override void LeaveProjectileTrail(PixelAbilityContext context, PixelProjectile projectile, Vector2Int cell)
        {
            projectile.TrailTimer = 0.055f;
            context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Water);
        }

        public override void ResolveImpact(PixelAbilityContext context, Vector2Int cell)
        {
            PixelReactionUtility.ExtinguishCircle(context.Grid, cell, 6);
            context.Grid.SpawnMaterial(cell, MaterialType.Water, 46, 6);
            PixelReactionUtility.FreezeSolidEdges(context.Grid, cell, 4);
            PixelCreatureRegistry.ApplyDamageInCircle(cell, 6f, 10f, 0f);
        }

        protected override void PaintProjectileSprite(Color32[] colors, int width, int height)
        {
            Color32 water = new Color32(56, 146, 236, 220);
            Color32 shine = new Color32(168, 236, 255, 255);
            FillRect(colors, width, height, 2, 0, 1, 5, water);
            FillRect(colors, width, height, 0, 2, 5, 1, water);
            SetPixel(colors, width, height, 1, 3, shine);
            SetPixel(colors, width, height, 3, 1, shine);
            SetPixel(colors, width, height, 2, 2, shine);
        }
    }

    public sealed class PoisonPixelAbility : PixelAbility
    {
        public override void UseRuntimeDefaults()
        {
            ConfigureBase("Poison Staff", new Color32(72, 214, 62, 255), 8.6f, 1.8f, 0.22f, 0.48f, 1.05f);
        }

        public override void CastSpecial(PixelAbilityContext context)
        {
            CastBeam(
                context,
                138,
                2,
                (cell, distance, widthOffset) =>
                {
                    context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Poison);
                    if (widthOffset == 0 && (distance % 4) == 0)
                    {
                        context.Grid.SpawnMaterial(cell, MaterialType.Poison, 3, 1);
                    }
                },
                (cell, material) =>
                {
                    context.Grid.SpawnMaterial(cell, MaterialType.Poison, 14, 3);
                    PixelCreatureRegistry.ApplyDamageInCircle(cell, 4f, 15f, 0f);
                });
        }

        public override void CastUltimate(PixelAbilityContext context)
        {
            CastBeam(
                context,
                72,
                5,
                (cell, distance, widthOffset) =>
                {
                    if (!context.Grid.IsSolid(cell.x, cell.y))
                    {
                        context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Poison);
                        if ((distance % 5) == 0)
                        {
                            context.Grid.SpawnMaterial(cell, MaterialType.Poison, 5, 2);
                        }
                    }
                },
                (cell, material) => context.Grid.SpawnMaterial(cell, MaterialType.Poison, 8, 2));
        }

        public override void LeaveProjectileTrail(PixelAbilityContext context, PixelProjectile projectile, Vector2Int cell)
        {
            projectile.TrailTimer = 0.035f;
            context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Poison);
            PixelCreatureRegistry.ApplyDamageInCircle(cell, 2f, 3f, 0f);
        }

        public override void ResolveImpact(PixelAbilityContext context, Vector2Int cell)
        {
            context.Grid.SpawnMaterial(cell, MaterialType.Poison, 56, 6);
            context.Grid.SpawnMaterial(cell, MaterialType.Smoke, 6, 4);
            PixelCreatureRegistry.ApplyDamageInCircle(cell, 7f, 24f, 0f);
        }

        protected override void PaintProjectileSprite(Color32[] colors, int width, int height)
        {
            Color32 poison = new Color32(68, 222, 66, 235);
            Color32 dark = new Color32(26, 116, 38, 255);
            Color32 shine = new Color32(168, 255, 112, 255);
            FillRect(colors, width, height, 1, 1, 3, 3, poison);
            SetPixel(colors, width, height, 2, 4, poison);
            SetPixel(colors, width, height, 0, 2, dark);
            SetPixel(colors, width, height, 4, 2, dark);
            SetPixel(colors, width, height, 2, 2, shine);
        }
    }

    public sealed class ExplosionPixelAbility : PixelAbility
    {
        public override void UseRuntimeDefaults()
        {
            ConfigureBase("Explosion Staff", new Color32(255, 190, 72, 255), 7.6f, 1.45f, 0.42f, 0.75f, 1.35f);
        }

        public override void CastSpecial(PixelAbilityContext context)
        {
            CastBeam(
                context,
                70,
                0,
                (cell, distance, widthOffset) =>
                {
                    if ((distance % 9) == 0)
                    {
                        context.Grid.SpawnMaterial(cell, MaterialType.Smoke, 3, 2);
                    }
                },
                (cell, material) => PixelReactionUtility.Explosion(context.Grid, cell, 5, 34f, 4.8f));
        }

        public override void CastUltimate(PixelAbilityContext context)
        {
            Vector2 direction = context.AimDirection;
            for (int i = 10; i <= 44; i += 8)
            {
                Vector2Int cell = new Vector2Int(
                    Mathf.RoundToInt(context.OriginCell.x + direction.x * i),
                    Mathf.RoundToInt(context.OriginCell.y + direction.y * i));
                PixelReactionUtility.Explosion(context.Grid, cell, 4 + i / 18, 30f, 4.5f);
            }
        }

        public override void LeaveProjectileTrail(PixelAbilityContext context, PixelProjectile projectile, Vector2Int cell)
        {
            projectile.TrailTimer = 0.07f;
            context.Grid.SpawnMaterial(cell, MaterialType.Smoke, 2, 1);
        }

        public override void ResolveImpact(PixelAbilityContext context, Vector2Int cell)
        {
            PixelReactionUtility.Explosion(context.Grid, cell, 7, 48f, 6.5f);
        }
    }

    public sealed class LightningPixelAbility : PixelAbility
    {
        public override void UseRuntimeDefaults()
        {
            ConfigureBase("Lightning Staff", new Color32(142, 232, 255, 255), 13f, 0.9f, 0.18f, 0.38f, 0.95f);
        }

        public override void CastNormal(PixelAbilityContext context, System.Collections.Generic.List<PixelProjectile> projectiles)
        {
            CastArc(context, 42, 3, 16f);
        }

        public override void CastSpecial(PixelAbilityContext context)
        {
            CastArc(context, 88, 5, 28f);
        }

        public override void CastUltimate(PixelAbilityContext context)
        {
            Vector2 direction = context.AimDirection;
            for (int spread = -3; spread <= 3; spread++)
            {
                Vector2 rotated = Quaternion.Euler(0f, 0f, spread * 9f) * direction;
                PixelAbilityContext arcContext = context;
                arcContext.AimWorld = context.OriginWorld + (Vector3)(rotated * 10f);
                CastArc(arcContext, 92, 6, 30f);
            }
        }

        public override void LeaveProjectileTrail(PixelAbilityContext context, PixelProjectile projectile, Vector2Int cell)
        {
            context.Grid.SetMaterial(cell.x, cell.y, MaterialType.Fire);
        }

        public override void ResolveImpact(PixelAbilityContext context, Vector2Int cell)
        {
            PixelReactionUtility.Explosion(context.Grid, cell, 3, 26f, 3f);
            PixelReactionUtility.Ignite(context.Grid, cell, 5, 18f);
        }

        protected override void PaintProjectileSprite(Color32[] colors, int width, int height)
        {
            Color32 bolt = new Color32(142, 232, 255, 255);
            Color32 core = new Color32(255, 255, 210, 255);
            SetPixel(colors, width, height, 3, 4, bolt);
            SetPixel(colors, width, height, 2, 3, core);
            SetPixel(colors, width, height, 3, 2, bolt);
            SetPixel(colors, width, height, 1, 2, core);
            SetPixel(colors, width, height, 2, 1, bolt);
            SetPixel(colors, width, height, 1, 0, bolt);
        }

        private void CastArc(PixelAbilityContext context, int range, int branchEvery, float damage)
        {
            Vector2 direction = context.AimDirection;
            Vector2Int previous = context.OriginCell;

            for (int distance = 2; distance <= range; distance++)
            {
                float wobble = Mathf.Sin(distance * 1.73f + context.OriginCell.x * 0.31f) * 1.5f;
                Vector2 perpendicular = new Vector2(-direction.y, direction.x);
                Vector2 point = context.OriginCell + direction * distance + perpendicular * wobble;
                Vector2Int cell = new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));

                if (!context.Grid.InBounds(cell.x, cell.y))
                {
                    break;
                }

                DrawSegment(context.Grid, previous, cell);
                previous = cell;

                if ((distance % branchEvery) == 0)
                {
                    context.Grid.SpawnMaterial(cell, MaterialType.Fire, 3, 1);
                    PixelCreatureRegistry.ApplyDamageInCircle(cell, 4f, damage, 1.5f);
                }

                if (context.Grid.IsSolid(cell.x, cell.y))
                {
                    PixelReactionUtility.Explosion(context.Grid, cell, 2, damage, 2f);
                    break;
                }
            }
        }

        private static void DrawSegment(PixelGrid grid, Vector2Int from, Vector2Int to)
        {
            int steps = Mathf.Max(Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y)), 1);
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
                if (grid.InBounds(x, y) && !grid.IsSolid(x, y))
                {
                    grid.SetMaterial(x, y, ((x + y) & 1) == 0 ? MaterialType.Fire : MaterialType.Smoke);
                }
            }
        }
    }
}
