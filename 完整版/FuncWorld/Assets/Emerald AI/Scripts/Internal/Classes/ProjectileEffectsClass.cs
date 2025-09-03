using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 发射物效果类.管理和缓存发射物(Projectile)的效果,以便在需要时启用和禁用这些效果
    /// </summary>
    [System.Serializable]
    public class ProjectileEffectsClass
    {// Used through projectiles to cache effects so they can be enabled and disabled when needed.

        /// <summary>
        /// 一个 `ParticleSystemRenderer` 对象,表示发射物的粒子系统效果
        /// </summary>
        public ParticleSystemRenderer EffectParticle;
        /// <summary>
        /// 一个 `GameObject` 对象,表示发射物的其他效果对象,如模型、声音等
        /// </summary>
        public GameObject EffectObject;

        /// <summary>
        /// 通过构造函数创建发射物效果类.管理和缓存发射物(Projectile)的效果,以便在需要时启用和禁用这些效果
        /// </summary>
        /// <param name="m_EffectParticle"></param>
        /// <param name="m_EffectObject"></param>
        public ProjectileEffectsClass(ParticleSystemRenderer m_EffectParticle, GameObject m_EffectObject)
        {
            EffectParticle = m_EffectParticle;
            EffectObject = m_EffectObject;
        }
    }
}