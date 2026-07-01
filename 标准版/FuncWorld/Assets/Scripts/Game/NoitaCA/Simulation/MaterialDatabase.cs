// 职责：集中管理材料定义，并把静态材料参数转换成可模拟的 Pixel 实例。
// Responsibility: Centralizes material definitions and converts static material parameters into simulated Pixel instances.
using System;
using UnityEngine;

namespace NoitaCA
{
    public enum PixelMovementMode
    {
        // 不移动、粉末下落、液体流动、气体上升。
        // Static, powder falling, liquid flowing, and gas rising.
        Static,
        Powder,
        Liquid,
        Gas
    }

    public readonly struct MaterialDefinition
    {
        // 基础显示与物理属性。
        // Core display and physical properties.
        public readonly MaterialType Type;
        public readonly string DisplayName;
        public readonly Color32 Color;
        public readonly int Density;
        public readonly float StartTemperature;
        public readonly int StartLifetime;
        public readonly PixelMovementMode MovementMode;
        // 移动能力与概率控制。
        // Movement capability and probability controls.
        public readonly int VerticalDirection;
        public readonly bool CanMoveVertical;
        public readonly bool CanMoveDiagonal;
        public readonly bool CanMoveHorizontal;
        public readonly int HorizontalSearchDistance;
        public readonly float MoveProbability;
        public readonly float LateralProbability;
        public readonly bool CanBeDisplaced;
        public readonly bool BlocksPlayer;
        // 寿命、热量和燃烧转换规则。
        // Lifetime, heat, and combustion conversion rules.
        public readonly bool ConsumesLifetime;
        public readonly int LifetimeDecay;
        public readonly float HeatEmission;
        public readonly float IgniteTemperature;
        public readonly float Flammability;
        public readonly MaterialType BurnMaterial;
        public readonly MaterialType BurnoutMaterial;
        public readonly MaterialType AlternateBurnoutMaterial;
        public readonly float AlternateBurnoutChance;
        public readonly int BurnLifetimeMin;
        public readonly int BurnLifetimeMax;

        public MaterialDefinition(
            MaterialType type,
            string displayName,
            Color32 color,
            int density,
            float startTemperature,
            int startLifetime,
            PixelMovementMode movementMode,
            int verticalDirection,
            bool canMoveVertical,
            bool canMoveDiagonal,
            bool canMoveHorizontal,
            int horizontalSearchDistance,
            float moveProbability,
            float lateralProbability,
            bool canBeDisplaced,
            bool blocksPlayer,
            bool consumesLifetime,
            int lifetimeDecay,
            float heatEmission,
            float igniteTemperature,
            float flammability,
            MaterialType burnMaterial,
            MaterialType burnoutMaterial,
            MaterialType alternateBurnoutMaterial,
            float alternateBurnoutChance,
            int burnLifetimeMin,
            int burnLifetimeMax)
        {
            // 对输入参数做安全夹取，保证材料定义不会产生无效模拟状态。
            // Clamps incoming parameters so definitions cannot produce invalid simulation state.
            Type = type;
            DisplayName = displayName;
            Color = color;
            Density = density;
            StartTemperature = startTemperature;
            StartLifetime = startLifetime;
            MovementMode = movementMode;
            VerticalDirection = Math.Sign(verticalDirection);
            CanMoveVertical = canMoveVertical;
            CanMoveDiagonal = canMoveDiagonal;
            CanMoveHorizontal = canMoveHorizontal;
            HorizontalSearchDistance = Mathf.Max(1, horizontalSearchDistance);
            MoveProbability = Mathf.Clamp01(moveProbability);
            LateralProbability = Mathf.Clamp01(lateralProbability);
            CanBeDisplaced = canBeDisplaced;
            BlocksPlayer = blocksPlayer;
            ConsumesLifetime = consumesLifetime;
            LifetimeDecay = Mathf.Max(1, lifetimeDecay);
            HeatEmission = heatEmission;
            IgniteTemperature = igniteTemperature;
            Flammability = Mathf.Clamp01(flammability);
            BurnMaterial = burnMaterial;
            BurnoutMaterial = burnoutMaterial;
            AlternateBurnoutMaterial = alternateBurnoutMaterial;
            AlternateBurnoutChance = Mathf.Clamp01(alternateBurnoutChance);
            BurnLifetimeMin = Mathf.Max(1, burnLifetimeMin);
            BurnLifetimeMax = Mathf.Max(BurnLifetimeMin, burnLifetimeMax);
        }

        public bool IsAir => Type == MaterialType.Air;
        public bool IsFlammable => Flammability > 0f;
    }

    public static class MaterialDatabase
    {
        private const float AmbientTemperature = 20f;
        // 启动时构建一次表；之后所有查询都是按枚举下标访问。
        // Build once at startup; all lookups then index by enum value.
        private static readonly MaterialDefinition[] Definitions = BuildDefinitions();

        public static float Ambient => AmbientTemperature;

        public static MaterialDefinition Get(MaterialType type)
        {
            int index = (int)type;
            if (index < 0 || index >= Definitions.Length)
            {
                // 非法材料回退为空气，避免越界或坏存档导致模拟崩溃。
                // Invalid materials fall back to air to avoid crashes from bad data.
                return Definitions[(int)MaterialType.Air];
            }

            return Definitions[index];
        }

        public static Pixel CreatePixel(MaterialType type)
        {
            MaterialDefinition definition = Get(type);
            // 将材料默认值复制进像素，之后像素可以独立改变温度/寿命/颜色。
            // Copies material defaults into a pixel, which can then mutate independently.
            return new Pixel(
                definition.Type,
                definition.Density,
                definition.StartTemperature,
                definition.StartLifetime,
                definition.Color,
                false,
                0,
                0,
                0,
                definition.BurnoutMaterial);
        }

        public static Pixel CreateBurningPixel(MaterialDefinition source, MaterialDefinition fire, MaterialType decayMaterial, int lifetime)
        {
            // 燃烧像素记住最终衰变材料，用于火焰寿命结束后的转换。
            // Burning pixels remember their decay material for conversion after lifetime expires.
            return new Pixel(
                fire.Type,
                fire.Density,
                fire.StartTemperature,
                Mathf.Max(1, lifetime),
                fire.Color,
                true,
                0,
                0,
                0,
                decayMaterial == MaterialType.Air ? MaterialType.Air : decayMaterial);
        }

        private static MaterialDefinition[] BuildDefinitions()
        {
            // 表长度必须覆盖 MaterialType 中所有有效枚举值。
            // The table length must cover every valid MaterialType enum value.
            MaterialDefinition[] definitions = new MaterialDefinition[8];

            // 空气：可被替换、不阻挡玩家，是所有空格子的默认材料。
            // Air: replaceable, non-blocking, and the default material for empty cells.
            definitions[(int)MaterialType.Air] = new MaterialDefinition(
                MaterialType.Air, "Air", new Color32(8, 10, 14, 255), 0, AmbientTemperature, 0,
                PixelMovementMode.Static, 0, false, false, false, 1, 0f, 0f,
                true, false, false, 1, 0f, 0f, 0f, MaterialType.Fire, MaterialType.Air,
                MaterialType.Air, 0f, 1, 1);

            // 沙子：高密度粉末，主要向下和斜下移动。
            // Sand: dense powder that mainly moves downward and diagonally downward.
            definitions[(int)MaterialType.Sand] = new MaterialDefinition(
                MaterialType.Sand, "Sand", new Color32(213, 183, 104, 255), 70, AmbientTemperature, 0,
                PixelMovementMode.Powder, -1, true, true, false, 1, 1f, 1f,
                false, true, false, 1, 0f, 0f, 0f, MaterialType.Fire, MaterialType.Air,
                MaterialType.Air, 0f, 1, 1);

            // 水：中等密度液体，可横向搜索以形成流动效果。
            // Water: medium-density liquid with lateral search for flowing behavior.
            definitions[(int)MaterialType.Water] = new MaterialDefinition(
                MaterialType.Water, "Water", new Color32(44, 134, 214, 255), 30, AmbientTemperature, 0,
                PixelMovementMode.Liquid, -1, true, true, true, 6, 1f, 0.78f,
                true, false, false, 1, 0f, 0f, 0f, MaterialType.Fire, MaterialType.Air,
                MaterialType.Air, 0f, 1, 1);

            // 烟：低密度气体，向上扩散并随寿命消散。
            // Smoke: low-density gas that rises, spreads, and fades with lifetime.
            definitions[(int)MaterialType.Smoke] = new MaterialDefinition(
                MaterialType.Smoke, "Smoke", new Color32(104, 112, 116, 180), -10, 55f, 150,
                PixelMovementMode.Gas, 1, true, true, true, 3, 0.92f, 0.86f,
                true, false, true, 1, 0f, 0f, 0f, MaterialType.Fire, MaterialType.Air,
                MaterialType.Air, 0f, 1, 1);

            // 火：释放热量并在寿命结束后变成烟或其他衰变材料。
            // Fire: emits heat and turns into smoke or another decay material when its lifetime ends.
            definitions[(int)MaterialType.Fire] = new MaterialDefinition(
                MaterialType.Fire, "Fire", new Color32(255, 104, 28, 255), -20, 420f, 30,
                PixelMovementMode.Static, 0, false, false, false, 1, 0f, 0f,
                true, false, true, 1, 34f, 145f, 0f, MaterialType.Fire, MaterialType.Smoke,
                MaterialType.Air, 0f, 18, 42);

            // 石头：静态且阻挡玩家，用作地形和容器。
            // Stone: static and player-blocking, used for terrain and containers.
            definitions[(int)MaterialType.Stone] = new MaterialDefinition(
                MaterialType.Stone, "Stone", new Color32(92, 84, 74, 255), 100, AmbientTemperature, 0,
                PixelMovementMode.Static, 0, false, false, false, 1, 0f, 0f,
                false, true, false, 1, 0f, 0f, 0f, MaterialType.Fire, MaterialType.Air,
                MaterialType.Air, 0f, 1, 1);

            // 木头：静态可燃材料，点燃后进入火焰/灰烬链路。
            // Wood: static flammable material that enters the fire/ash chain when ignited.
            definitions[(int)MaterialType.Wood] = new MaterialDefinition(
                MaterialType.Wood, "Wood", new Color32(126, 78, 38, 255), 82, AmbientTemperature, 0,
                PixelMovementMode.Static, 0, false, false, false, 1, 0f, 0f,
                false, true, false, 1, 0f, 125f, 0.64f, MaterialType.Fire, MaterialType.Ash,
                MaterialType.Smoke, 0.35f, 34, 70);

            // 灰烬：轻质粉末，是燃烧后的残留物。
            // Ash: light powder used as post-combustion residue.
            definitions[(int)MaterialType.Ash] = new MaterialDefinition(
                MaterialType.Ash, "Ash", new Color32(78, 74, 68, 255), 18, AmbientTemperature, 0,
                PixelMovementMode.Powder, -1, true, true, false, 1, 0.75f, 0.55f,
                true, false, false, 1, 0f, 0f, 0f, MaterialType.Fire, MaterialType.Air,
                MaterialType.Air, 0f, 1, 1);

            return definitions;
        }
    }
}
