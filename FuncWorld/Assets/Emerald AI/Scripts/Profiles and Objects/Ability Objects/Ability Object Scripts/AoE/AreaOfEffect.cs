using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    public class AreaOfEffect : MonoBehaviour
    {
        public LayerMask Enemies;
        AreaOfEffectAbility CurrentAbilityData;
        GameObject Owner;
        EmeraldSystem EmeraldComponent;

        public void Initialize (GameObject owner, Transform AttackTransform, AreaOfEffectAbility abilityData)
        {
            EmeraldComponent = owner.GetComponent<EmeraldSystem>();
            Enemies = EmeraldComponent.DetectionComponent.DetectionLayerMask;
            CurrentAbilityData = abilityData;
            Owner = owner;
            IntitailizeInternal(Owner, AttackTransform);
        }

        void IntitailizeInternal (GameObject Owner, Transform AttackTransform)
        {
            List<Collider> DetectedAOETargets = Physics.OverlapSphere(AttackTransform.position, CurrentAbilityData.AreaOfEffectSettings.Radius, Enemies).ToList(); //Only looks for targets that have the same layer as the layers from the AI's DetectionLayerMask.
            DetectedAOETargets.Remove(Owner.GetComponent<Collider>()); //Remove the owner's collider if it happens to be detected.

            for (int i = 0; i < DetectedAOETargets.Count; i++)
            {
                //Only damage targets that the Owner has an Enemy Relation Type with.
                if (EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, DetectedAOETargets[i].transform) == "Enemy")
                {
                    ICombat m_ICombat = DetectedAOETargets[i].GetComponent<ICombat>();

                    if (CurrentAbilityData.AreaOfEffectSettings.HitTargetEffect != null)
                    {
                        if (m_ICombat != null && !m_ICombat.IsDodging() && !m_ICombat.IsBlocking() && DetectedAOETargets[i].transform.localScale != Vector3.one * 0.003f)
                            EmeraldObjectPool.SpawnEffect(CurrentAbilityData.AreaOfEffectSettings.HitTargetEffect, DetectedAOETargets[i].GetComponent<ICombat>().DamagePosition(), DetectedAOETargets[i].transform.rotation, CurrentAbilityData.AreaOfEffectSettings.HitTargetEffectTimeoutSeconds);
                    }

                    DamageTarget(DetectedAOETargets[i].gameObject);
                }
            }
        }

        /// <summary>
        /// Damages the projectile's StartingTarget, given that it has a IDamageable.
        /// </summary>
        void DamageTarget(GameObject Target)
        {
            if (Target.transform.localScale == Vector3.one * 0.003f) return;

            if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
            {
                var m_ICombat = Target.GetComponentInParent<ICombat>();
                if (m_ICombat != null) m_ICombat.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
            }

            //Only cause damage if it's enabled
            if (!CurrentAbilityData.DamageSettings.Enabled) return;

            var m_IDamageable = Target.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();
                m_IDamageable.Damage(CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit), Owner.transform, CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, Target);
            }
            else
            {
                Debug.Log(Target.gameObject + " is missing a IDamageable and/or ICombat Component, apply one");
            }
        }
    }
}