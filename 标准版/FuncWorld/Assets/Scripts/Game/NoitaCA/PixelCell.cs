// 职责：保留旧版像素单元结构，兼容早期场景脚本，新的模拟应使用 Pixel。
// Responsibility: Keeps the legacy pixel-cell structure for old scene scripts; new simulation code should use Pixel.
using System;
using UnityEngine;

namespace NoitaCA
{
    [Obsolete("Use Pixel instead. PixelCell is kept only for older scene scripts.")]
    public struct PixelCell
    {
        // 旧版字段保持原样，降低迁移旧场景时的破坏性。
        // Legacy fields are kept as-is to reduce migration risk for old scenes.
        public MaterialType MaterialType;
        public Color32 Color;
        public bool UpdatedThisFrame;
        public int FallingFrames;
        public int Lifetime;
        public sbyte VelocityX;
        public sbyte VelocityY;

        public PixelCell(MaterialType materialType, Color32 color)
        {
            // 旧结构只保存最小默认状态，复杂物理属性由新 Pixel 负责。
            // The legacy struct stores minimal defaults; richer physical state lives in Pixel.
            MaterialType = materialType;
            Color = color;
            UpdatedThisFrame = false;
            FallingFrames = 0;
            Lifetime = 0;
            VelocityX = 0;
            VelocityY = 0;
        }

        public static PixelCell FromMaterial(MaterialType materialType)
        {
            // 从统一材料数据库取颜色，避免旧新系统显示不一致。
            // Reads color from the shared material database to keep old and new visuals aligned.
            MaterialDefinition definition = MaterialDatabase.Get(materialType);
            return new PixelCell(definition.Type, definition.Color);
        }
    }
}
