using UnityEngine;

namespace NoitaCA
{
    public static class PixelReactionUtility
    {
        public static int ExtinguishCircle(PixelGrid grid, Vector2Int center, int radius)
        {
            if (grid == null)
            {
                return 0;
            }

            int safeRadius = Mathf.Max(1, radius);
            int radiusSquared = safeRadius * safeRadius;
            int changed = 0;

            for (int y = center.y - safeRadius; y <= center.y + safeRadius; y++)
            {
                for (int x = center.x - safeRadius; x <= center.x + safeRadius; x++)
                {
                    if (!grid.InBounds(x, y))
                    {
                        continue;
                    }

                    int dx = x - center.x;
                    int dy = y - center.y;
                    if (dx * dx + dy * dy > radiusSquared)
                    {
                        continue;
                    }

                    MaterialType material = grid.GetMaterial(x, y);
                    if (material == MaterialType.Fire || material == MaterialType.Lava)
                    {
                        grid.SetMaterial(x, y, material == MaterialType.Lava ? MaterialType.Stone : MaterialType.Smoke);
                        changed++;
                    }
                }
            }

            if (changed > 0)
            {
                grid.MarkActiveArea(center.x, center.y, safeRadius + 2);
            }

            return changed;
        }

        public static int FreezeSolidEdges(PixelGrid grid, Vector2Int center, int radius)
        {
            if (grid == null)
            {
                return 0;
            }

            int safeRadius = Mathf.Max(1, radius);
            int radiusSquared = safeRadius * safeRadius;
            int changed = 0;

            for (int y = center.y - safeRadius; y <= center.y + safeRadius; y++)
            {
                for (int x = center.x - safeRadius; x <= center.x + safeRadius; x++)
                {
                    if (!grid.InBounds(x, y))
                    {
                        continue;
                    }

                    int dx = x - center.x;
                    int dy = y - center.y;
                    if (dx * dx + dy * dy > radiusSquared || grid.IsSolid(x, y))
                    {
                        continue;
                    }

                    if (IsNearSolid(grid, x, y) && ((x + y) & 1) == 0)
                    {
                        grid.SetMaterial(x, y, MaterialType.Ice);
                        changed++;
                    }
                }
            }

            if (changed > 0)
            {
                grid.MarkActiveArea(center.x, center.y, safeRadius + 2);
            }

            return changed;
        }

        public static void Explosion(PixelGrid grid, Vector2Int center, int radius, float creatureDamage, float pushForce)
        {
            if (grid == null || !grid.InBounds(center.x, center.y))
            {
                return;
            }

            grid.ExplodeCircle(center, radius);
            PixelCreatureRegistry.ApplyDamageInCircle(center, radius + 3, creatureDamage, pushForce);
        }

        public static void Ignite(PixelGrid grid, Vector2Int center, int radius, float creatureDamage)
        {
            if (grid == null || !grid.InBounds(center.x, center.y))
            {
                return;
            }

            grid.IgniteCircle(center, radius);
            PixelCreatureRegistry.ApplyDamageInCircle(center, radius + 1, creatureDamage, 0f);
        }

        public static void SpawnMaterial(PixelGrid grid, Vector2Int center, MaterialType material, int amount, int radius, float creatureDamage)
        {
            if (grid == null || !grid.InBounds(center.x, center.y))
            {
                return;
            }

            grid.SpawnMaterial(center, material, amount, radius);
            if (creatureDamage > 0f)
            {
                PixelCreatureRegistry.ApplyDamageInCircle(center, radius + 1, creatureDamage, 0f);
            }
        }

        public static bool IsNearSolid(PixelGrid grid, int x, int y)
        {
            if (grid == null)
            {
                return false;
            }

            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    if ((ox != 0 || oy != 0) && grid.IsSolid(x + ox, y + oy))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
