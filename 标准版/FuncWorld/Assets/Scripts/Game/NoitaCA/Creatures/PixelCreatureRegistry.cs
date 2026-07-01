using System.Collections.Generic;
using UnityEngine;

namespace NoitaCA
{
    public static class PixelCreatureRegistry
    {
        private static readonly List<PixelCreature> Creatures = new List<PixelCreature>(64);

        public static IReadOnlyList<PixelCreature> ActiveCreatures => Creatures;

        public static void Register(PixelCreature creature)
        {
            if (creature != null && !Creatures.Contains(creature))
            {
                Creatures.Add(creature);
            }
        }

        public static void Unregister(PixelCreature creature)
        {
            Creatures.Remove(creature);
        }

        public static bool HasCreatureInCircle(Vector2Int center, float radius)
        {
            float radiusSquared = radius * radius;
            for (int i = Creatures.Count - 1; i >= 0; i--)
            {
                PixelCreature creature = Creatures[i];
                if (creature == null)
                {
                    Creatures.RemoveAt(i);
                    continue;
                }

                Vector2 delta = creature.CenterCell - center;
                if (delta.sqrMagnitude <= radiusSquared)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ApplyDamageInCircle(Vector2Int center, float radius, float damage, float pushForce)
        {
            float radiusSquared = radius * radius;
            for (int i = Creatures.Count - 1; i >= 0; i--)
            {
                PixelCreature creature = Creatures[i];
                if (creature == null)
                {
                    Creatures.RemoveAt(i);
                    continue;
                }

                Vector2 delta = creature.CenterCell - center;
                float distanceSquared = delta.sqrMagnitude;
                if (distanceSquared > radiusSquared)
                {
                    continue;
                }

                float distance01 = radius <= 0f ? 0f : Mathf.Sqrt(distanceSquared) / radius;
                float strength = 1f - Mathf.Clamp01(distance01);
                creature.TakeDamage(damage * Mathf.Max(0.2f, strength));

                if (pushForce > 0f && delta.sqrMagnitude > 0.0001f)
                {
                    creature.AddImpulse(delta.normalized * pushForce * strength);
                }
            }
        }
    }
}
