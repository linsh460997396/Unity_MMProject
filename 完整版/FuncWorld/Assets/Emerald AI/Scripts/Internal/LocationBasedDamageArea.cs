using UnityEngine;

namespace EmeraldAI
{
    public class LocationBasedDamageArea : MonoBehaviour
    {
        [HideInInspector] public float DamageMultiplier = 1;
        [HideInInspector] public EmeraldSystem EmeraldComponent;

        /// <summary>
        /// Damages an AI's location based damage component and applies a multiplier to the damage receieved. The parameters of this are the same as the EmeraldAISystem Damage function. 
        /// Call this function instead if you want to utilize the Location Based Damage feature. Ensure that the AI you are damaging has a Location Based Damage script applied to where 
        /// the EmeraldAISystem script is and that you have pressed the Get Colliders button on the Location Based Damage component.
        /// </summary>
        public void DamageArea(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 0, bool CriticalHit = false)
        {
            int DamageDealt = DamageAmount;
            if (DamageMultiplier > 1) DamageDealt = Mathf.RoundToInt(DamageAmount * DamageMultiplier);
            IDamageable m_IDamageable = EmeraldComponent.GetComponent<IDamageable>();
            m_IDamageable.Damage(DamageDealt, AttackerTransform, RagdollForce, CriticalHit);
            if (!EmeraldComponent.AnimationComponent.IsBlocking) CreateImpactEffect(transform.position, EmeraldComponent.HealthComponent.AttachHitEffects);
            if (m_IDamageable.Health <= 0)
                EmeraldComponent.CombatComponent.RagdollTransform = transform;
        }

        /// <summary>
        /// Creates an impact effect at the ImpactPosition. The Impact Effect is based off of your AI's Hit Effects List (Located under AI's Settings>Combat>Hit Effect).
        /// </summary>
        public void CreateImpactEffect(Vector3 ImpactPosition, bool SetAIAsEffectParent = true)
        {
            if (EmeraldComponent.HealthComponent.UseHitEffect == YesOrNo.Yes && EmeraldComponent.HealthComponent.HitEffectsList.Count > 0)
            {
                GameObject RandomBloodEffect = EmeraldComponent.HealthComponent.HitEffectsList[Random.Range(0, EmeraldComponent.HealthComponent.HitEffectsList.Count)];
                if (RandomBloodEffect != null)
                {
                    GameObject SpawnedBlood = EmeraldAI.Utility.EmeraldObjectPool.SpawnEffect(RandomBloodEffect, ImpactPosition, Quaternion.LookRotation(transform.forward, Vector3.up), EmeraldComponent.HealthComponent.HitEffectTimeoutSeconds) as GameObject;

                    if (SetAIAsEffectParent)
                        SpawnedBlood.transform.SetParent(transform);
                    else
                        SpawnedBlood.transform.SetParent(EmeraldSystem.ObjectPool.transform);
                }
            }
        }
    }
}