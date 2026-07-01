using System;
using UnityEngine;

namespace NoitaCA
{
    [Serializable]
    public sealed class PixelWorldRenderSettings
    {
        [Tooltip("Use the alchemy art palette instead of raw material colors.")]
        public bool EnableAlchemyPalette = true;

        [Range(0f, 1f)]
        public float MaterialVariation = 0.42f;

        [Range(0f, 1f)]
        public float DepthShadow = 0.36f;

        [Range(0f, 1f)]
        public float EdgeLighting = 0.34f;

        [Range(0f, 1f)]
        public float GlowBoost = 0.28f;

        [Range(0f, 1f)]
        public float AmbientVeil = 0.16f;

        public static PixelWorldRenderSettings CreateAlchemyDefault()
        {
            return new PixelWorldRenderSettings();
        }

        public static PixelWorldRenderSettings CreateClassicDefault()
        {
            return new PixelWorldRenderSettings
            {
                EnableAlchemyPalette = false,
                MaterialVariation = 0f,
                DepthShadow = 0f,
                EdgeLighting = 0f,
                GlowBoost = 0f,
                AmbientVeil = 0f
            };
        }

        public PixelWorldRenderSettings Clone()
        {
            return new PixelWorldRenderSettings
            {
                EnableAlchemyPalette = EnableAlchemyPalette,
                MaterialVariation = MaterialVariation,
                DepthShadow = DepthShadow,
                EdgeLighting = EdgeLighting,
                GlowBoost = GlowBoost,
                AmbientVeil = AmbientVeil
            };
        }
    }
}
