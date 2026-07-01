// 职责：描述单个像素格子的运行时状态，包括材料、温度、寿命和移动标记。
// Responsibility: Describes one pixel cell's runtime state, including material, temperature, lifetime, and movement flags.
using UnityEngine;

namespace NoitaCA
{
    public struct Pixel
    {
        // 材料基础属性和交互状态。
        // Base material properties and interaction state.
        public MaterialType MaterialType;
        public int Density;
        public float Temperature;
        public int Lifetime;
        public Color32 Color;
        // 单帧更新标记与运动状态，避免同一帧重复移动。
        // Per-frame update marker and movement state, preventing double movement in one frame.
        public bool UpdatedThisFrame;
        public int FallingFrames;
        public sbyte VelocityX;
        public sbyte VelocityY;
        public MaterialType DecayMaterial;

        public Pixel(
            MaterialType materialType,
            int density,
            float temperature,
            int lifetime,
            Color32 color,
            bool updatedThisFrame = false,
            int fallingFrames = 0,
            sbyte velocityX = 0,
            sbyte velocityY = 0,
            MaterialType decayMaterial = MaterialType.Air)
        {
            // 构造时把材料数据库给出的定义拍平成运行时像素状态。
            // The constructor flattens a material definition into runtime pixel state.
            MaterialType = materialType;
            Density = density;
            Temperature = temperature;
            Lifetime = lifetime;
            Color = color;
            UpdatedThisFrame = updatedThisFrame;
            FallingFrames = fallingFrames;
            VelocityX = velocityX;
            VelocityY = velocityY;
            DecayMaterial = decayMaterial;
        }

        public static Pixel FromMaterial(MaterialType materialType)
        {
            // 统一从材料数据库创建像素，保证默认温度/颜色/寿命一致。
            // Centralizes pixel creation through the material database so defaults stay consistent.
            return MaterialDatabase.CreatePixel(materialType);
        }
    }
}
