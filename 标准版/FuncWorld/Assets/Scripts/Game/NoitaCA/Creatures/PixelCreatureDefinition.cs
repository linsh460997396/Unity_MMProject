using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoitaCA
{
    public enum PixelCreatureBodyPlan
    {
        Custom,
        Biped,
        Quadruped
    }

    [Serializable]
    public struct PixelBodyCell
    {
        public Vector2Int Offset;
        public MaterialType Material;

        public PixelBodyCell(int x, int y, MaterialType material)
        {
            Offset = new Vector2Int(x, y);
            Material = material;
        }
    }

    [Serializable]
    public struct PixelCreatureDrop
    {
        public MaterialType Material;
        public int Amount;
        public int Radius;
        public PixelEquipmentDefinition Equipment;
    }

    [CreateAssetMenu(menuName = "NoitaCA/Pixel Creature", fileName = "Pixel Creature")]
    public sealed class PixelCreatureDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Pixel Creature";
        [SerializeField] private float maxHealth = 36f;
        [SerializeField] private float moveSpeed = 2.4f;
        [SerializeField] private float jumpSpeed = 5.8f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float detectionRangeCells = 64f;
        [SerializeField] private float attackRangeCells = 9f;
        [SerializeField] private float contactDamage = 8f;
        [SerializeField] private float bodyLossDamage = 0f;
        [SerializeField] private float wanderSpeedMultiplier = 0.32f;
        [SerializeField] private float wanderTurnIntervalMin = 1.8f;
        [SerializeField] private float wanderTurnIntervalMax = 4.2f;
        [SerializeField] private PixelCreatureBodyPlan bodyPlan = PixelCreatureBodyPlan.Custom;
        [SerializeField] private MaterialType coreMaterial = MaterialType.Wood;
        [SerializeField] private MaterialType accentMaterial = MaterialType.Debris;
        [SerializeField] private MaterialType limbMaterial = MaterialType.Wood;
        [SerializeField] private MaterialType eyeMaterial = MaterialType.Fire;
        [SerializeField] private int bodyLength = 6;
        [SerializeField] private int bodyHeight = 4;
        [SerializeField] private float gaitFramesPerSecond = 7f;
        [SerializeField] private PixelBodyCell[] bodyCells = new PixelBodyCell[0];
        [SerializeField] private PixelCreatureDrop[] drops = new PixelCreatureDrop[0];

        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public float MaxHealth => Mathf.Max(1f, maxHealth);
        public float MoveSpeed => Mathf.Max(0f, moveSpeed);
        public float JumpSpeed => Mathf.Max(0f, jumpSpeed);
        public float Gravity => gravity;
        public float DetectionRangeCells => Mathf.Max(1f, detectionRangeCells);
        public float AttackRangeCells => Mathf.Max(1f, attackRangeCells);
        public float ContactDamage => Mathf.Max(0f, contactDamage);
        public float BodyLossDamage => Mathf.Max(0f, bodyLossDamage);
        public float WanderSpeedMultiplier => Mathf.Clamp(wanderSpeedMultiplier, 0f, 1f);
        public float WanderTurnIntervalMin => Mathf.Max(0.1f, wanderTurnIntervalMin);
        public float WanderTurnIntervalMax => Mathf.Max(WanderTurnIntervalMin, wanderTurnIntervalMax);
        public float GaitFramesPerSecond => Mathf.Max(0.1f, gaitFramesPerSecond);
        public PixelBodyCell[] BodyCells => bodyCells;
        public PixelCreatureDrop[] Drops => drops;

        public void BuildBodyCells(List<PixelBodyCell> output, float gaitPhase, bool moving, bool grounded)
        {
            if (output == null)
            {
                return;
            }

            output.Clear();
            if (bodyPlan == PixelCreatureBodyPlan.Biped)
            {
                BuildBiped(output, gaitPhase, moving, grounded);
                NormalizeBodyToGround(output);
                return;
            }

            if (bodyPlan == PixelCreatureBodyPlan.Quadruped)
            {
                BuildQuadruped(output, gaitPhase, moving, grounded);
                NormalizeBodyToGround(output);
                return;
            }

            if (bodyCells != null && bodyCells.Length > 0)
            {
                output.AddRange(bodyCells);
                NormalizeBodyToGround(output);
                return;
            }

            BuildBiped(output, gaitPhase, moving, grounded);
            NormalizeBodyToGround(output);
        }

        public static PixelCreatureDefinition CreateRuntimeCrawler(PixelEquipmentDefinition rareDrop)
        {
            PixelCreatureDefinition definition = CreateInstance<PixelCreatureDefinition>();
            definition.name = "Runtime Pixel Crawler";
            definition.displayName = "Sand Crawler";
            definition.maxHealth = 34f;
            definition.moveSpeed = 2.1f;
            definition.jumpSpeed = 4.8f;
            definition.gravity = -18f;
            definition.contactDamage = 7f;
            definition.wanderSpeedMultiplier = 0.3f;
            definition.bodyPlan = PixelCreatureBodyPlan.Quadruped;
            definition.coreMaterial = MaterialType.Sand;
            definition.accentMaterial = MaterialType.Debris;
            definition.limbMaterial = MaterialType.Sand;
            definition.eyeMaterial = MaterialType.Fire;
            definition.bodyLength = 7;
            definition.bodyHeight = 3;
            definition.gaitFramesPerSecond = 8f;
            definition.drops = BuildDrops(MaterialType.Sand, 18, 3, rareDrop);
            definition.hideFlags = HideFlags.HideAndDontSave;
            return definition;
        }

        public static PixelCreatureDefinition CreateRuntimeComposite(
            string creatureName,
            PixelCreatureBodyPlan plan,
            MaterialType core,
            MaterialType accent,
            MaterialType limb,
            MaterialType eye,
            float health,
            float speed,
            float jump,
            PixelEquipmentDefinition rareDrop)
        {
            PixelCreatureDefinition definition = CreateInstance<PixelCreatureDefinition>();
            definition.name = "Runtime " + creatureName;
            definition.displayName = creatureName;
            definition.maxHealth = health;
            definition.moveSpeed = speed;
            definition.jumpSpeed = jump;
            definition.gravity = plan == PixelCreatureBodyPlan.Biped ? -20f : -18f;
            definition.contactDamage = Mathf.Lerp(6f, 14f, Mathf.Clamp01(health / 70f));
            definition.wanderSpeedMultiplier = plan == PixelCreatureBodyPlan.Biped ? 0.24f : 0.3f;
            definition.bodyPlan = plan;
            definition.coreMaterial = core;
            definition.accentMaterial = accent;
            definition.limbMaterial = limb;
            definition.eyeMaterial = eye;
            definition.bodyLength = plan == PixelCreatureBodyPlan.Biped ? 5 : 6 + Mathf.RoundToInt(speed);
            definition.bodyHeight = plan == PixelCreatureBodyPlan.Biped ? 5 : 3;
            definition.gaitFramesPerSecond = Mathf.Lerp(5f, 9f, Mathf.Clamp01(speed / 3.5f));
            definition.drops = BuildDrops(core, core == MaterialType.Fire ? 8 : 14, 3, rareDrop);
            definition.hideFlags = HideFlags.HideAndDontSave;
            return definition;
        }

        public static PixelCreatureDefinition CreateRuntimeJumper(PixelEquipmentDefinition rareDrop)
        {
            PixelCreatureDefinition definition = CreateInstance<PixelCreatureDefinition>();
            definition.name = "Runtime Pixel Jumper";
            definition.displayName = "Frost Hopper";
            definition.maxHealth = 42f;
            definition.moveSpeed = 2.8f;
            definition.jumpSpeed = 7.2f;
            definition.gravity = -20f;
            definition.contactDamage = 9f;
            definition.wanderSpeedMultiplier = 0.24f;
            definition.bodyPlan = PixelCreatureBodyPlan.Biped;
            definition.coreMaterial = MaterialType.Ice;
            definition.accentMaterial = MaterialType.Water;
            definition.limbMaterial = MaterialType.Ice;
            definition.eyeMaterial = MaterialType.Fire;
            definition.bodyLength = 5;
            definition.bodyHeight = 5;
            definition.gaitFramesPerSecond = 9f;
            definition.drops = BuildDrops(MaterialType.Water, 18, 4, rareDrop);
            definition.hideFlags = HideFlags.HideAndDontSave;
            return definition;
        }

        public static PixelCreatureDefinition CreateRuntimeEmber(PixelEquipmentDefinition rareDrop)
        {
            PixelCreatureDefinition definition = CreateInstance<PixelCreatureDefinition>();
            definition.name = "Runtime Pixel Ember Beast";
            definition.displayName = "Ember Beast";
            definition.maxHealth = 58f;
            definition.moveSpeed = 1.8f;
            definition.jumpSpeed = 5.2f;
            definition.gravity = -17f;
            definition.contactDamage = 14f;
            definition.wanderSpeedMultiplier = 0.28f;
            definition.bodyPlan = PixelCreatureBodyPlan.Quadruped;
            definition.coreMaterial = MaterialType.Wood;
            definition.accentMaterial = MaterialType.Fire;
            definition.limbMaterial = MaterialType.Wood;
            definition.eyeMaterial = MaterialType.Fire;
            definition.bodyLength = 8;
            definition.bodyHeight = 4;
            definition.gaitFramesPerSecond = 6f;
            definition.drops = BuildDrops(MaterialType.Fire, 10, 3, rareDrop);
            definition.hideFlags = HideFlags.HideAndDontSave;
            return definition;
        }

        private void BuildBiped(List<PixelBodyCell> output, float gaitPhase, bool moving, bool grounded)
        {
            int stride = moving && grounded ? Mathf.RoundToInt(Mathf.Sin(gaitPhase) * 1.2f) : 0;
            int counterStride = -stride;
            int headBob = moving && grounded && Mathf.Sin(gaitPhase * 2f) > 0f ? 1 : 0;

            AddRect(output, -1, 1 + headBob, 3, 3, coreMaterial);
            AddRect(output, -2, 1 + headBob, 1, 2, accentMaterial);
            AddRect(output, 2, 1 + headBob, 1, 2, accentMaterial);
            AddRect(output, -1, 4 + headBob, 3, 2, coreMaterial);
            Add(output, 1, 5 + headBob, eyeMaterial);

            AddRect(output, -2, 0, 2, 1, limbMaterial);
            AddRect(output, 1, 0, 2, 1, limbMaterial);
            AddLeg(output, -1, -1, stride, limbMaterial);
            AddLeg(output, 1, -1, counterStride, limbMaterial);
            Add(output, -2, 2 + headBob, limbMaterial);
            Add(output, 2, 2 + headBob, limbMaterial);
        }

        private void BuildQuadruped(List<PixelBodyCell> output, float gaitPhase, bool moving, bool grounded)
        {
            int length = Mathf.Clamp(bodyLength, 5, 10);
            int height = Mathf.Clamp(bodyHeight, 2, 5);
            int half = length / 2;
            int bob = moving && grounded && Mathf.Sin(gaitPhase * 2f) > 0f ? 1 : 0;

            AddRect(output, -half, 1 + bob, length, height, coreMaterial);
            AddRect(output, half - 1, 2 + bob, 3, Mathf.Max(2, height - 1), coreMaterial);
            Add(output, half + 1, 3 + bob, eyeMaterial);
            Add(output, -half - 1, 2 + bob, accentMaterial);
            Add(output, -half - 2, 2 + bob, accentMaterial);

            int liftA = moving && grounded && Mathf.Sin(gaitPhase) > 0f ? 1 : 0;
            int liftB = moving && grounded && Mathf.Sin(gaitPhase) <= 0f ? 1 : 0;
            AddLeg(output, -half + 1, 0, -liftA, limbMaterial);
            AddLeg(output, -1, 0, liftB, limbMaterial);
            AddLeg(output, 1, 0, liftA, limbMaterial);
            AddLeg(output, half - 1, 0, -liftB, limbMaterial);
        }

        private static void AddLeg(List<PixelBodyCell> output, int x, int y, int stride, MaterialType material)
        {
            Add(output, x, y, material);
            Add(output, x + Mathf.Clamp(stride, -1, 1), y - 1, material);
        }

        private static void AddRect(List<PixelBodyCell> output, int x, int y, int width, int height, MaterialType material)
        {
            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    Add(output, x + px, y + py, material);
                }
            }
        }

        private static void Add(List<PixelBodyCell> output, int x, int y, MaterialType material)
        {
            output.Add(new PixelBodyCell(x, y, material));
        }

        private static void NormalizeBodyToGround(List<PixelBodyCell> output)
        {
            if (output == null || output.Count == 0)
            {
                return;
            }

            int minY = output[0].Offset.y;
            for (int i = 1; i < output.Count; i++)
            {
                minY = Mathf.Min(minY, output[i].Offset.y);
            }

            if (minY == 0)
            {
                return;
            }

            for (int i = 0; i < output.Count; i++)
            {
                PixelBodyCell cell = output[i];
                cell.Offset.y -= minY;
                output[i] = cell;
            }
        }

        private static PixelCreatureDrop[] BuildDrops(MaterialType material, int amount, int radius, PixelEquipmentDefinition equipment)
        {
            return new PixelCreatureDrop[]
            {
                new PixelCreatureDrop { Material = material, Amount = amount, Radius = radius, Equipment = equipment }
            };
        }
    }
}
