using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Melee Ability", menuName = "Emerald AI/Ability/Melee Ability")]
    public class MeleeAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.MeleeData MeleeSettings;
        public AbilityData.StunnedData StunnedSettings;
        public AbilityData.DamageData DamageSettings;

        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            EmeraldWeaponCollision WeaponCollision = EmeraldComponent.CombatComponent.CurrentWeaponCollision;
            Transform Target = EmeraldComponent.CombatTarget;
            float TargetAngle = EmeraldComponent.CombatComponent.TargetAngle;
            float TargetDistance = EmeraldComponent.CombatComponent.DistanceFromTarget;

            //Return if the damage angle or damage distance is not met, but only if there's no currently active Weapon Collider components.
            if (TargetAngle > MeleeSettings.MaxDamageAngle || TargetDistance > MeleeSettings.MaxDamageDistance || WeaponCollision != null || Target == null) return;

            var m_ICombat = Target.GetComponentInParent<ICombat>();

            //If stuns are enabled, roll for a stun
            if (StunnedSettings.Enabled && StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(StunnedSettings.StunLength);
            }

            //Only cause damage if it's enabled
            if (!DamageSettings.Enabled) return;

            var m_IDamageable = Target.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                bool IsCritHit = DamageSettings.GenerateCritHit();
                m_IDamageable.Damage(DamageSettings.GenerateDamage(IsCritHit), Owner.transform, DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                DamageSettings.DamageTargetOverTime(this, DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                EmeraldComponent.AnimationComponent.PlayRecoilAnimation();
                if (EmeraldComponent.CombatComponent.DeathDelayTimer < 0.1f && !m_ICombat.IsBlocking() && !m_ICombat.IsDodging()) AbilityData.SpawnEffectAndSound(Owner, Target.GetComponent<ICombat>().DamagePosition(), MeleeSettings.ImpactEffect, MeleeSettings.ImpactEffectTimeoutSeconds, MeleeSettings.ImpactSoundsList);
            }
            else
            {
                Debug.Log(Target.gameObject + " is missing IDamageable Component, apply one");
            }
        }

        /// <summary>
        /// Called through the EmeraldWeaponCollision during a successful collision with a target. This is the only ability that has a dependency (which is the EmeraldWeaponCollision script).
        /// </summary>
        public void MeleeDamage (GameObject Owner, GameObject Target, Transform TargetRoot)
        {
            //Return if the target is teleporting.
            if (TargetRoot.transform.localScale == Vector3.one * 0.003f) return;

            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            LocationBasedDamageArea m_LocationBasedDamageArea = Target.GetComponent<LocationBasedDamageArea>();
            ICombat m_ICombat = TargetRoot.GetComponent<ICombat>();

            //If stuns are enabled, roll for a stun
            if (StunnedSettings.Enabled && StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(StunnedSettings.StunLength);
            }

            //Only cause damage if it's enabled
            if (!DamageSettings.Enabled) return;

            if (m_LocationBasedDamageArea == null)
            {
                var m_IDamageable = TargetRoot.GetComponent<IDamageable>();

                if (m_IDamageable != null)
                {
                    bool IsCritHit = DamageSettings.GenerateCritHit();
                    m_IDamageable.Damage(DamageSettings.GenerateDamage(IsCritHit), Owner.transform, DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                    DamageSettings.DamageTargetOverTime(this, DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                    EmeraldComponent.AnimationComponent.PlayRecoilAnimation();
                    if (EmeraldComponent.CombatComponent.DeathDelayTimer < 0.1f && !m_ICombat.IsBlocking()) AbilityData.SpawnEffectAndSound(Owner, Target.GetComponent<ICombat>().DamagePosition(), MeleeSettings.ImpactEffect, MeleeSettings.ImpactEffectTimeoutSeconds, MeleeSettings.ImpactSoundsList);
                }
                else
                {
                    Debug.Log(Target.gameObject + " is missing IDamageable Component, apply one");
                }
            }
            else if (m_LocationBasedDamageArea != null)
            {
                bool IsCritHit = DamageSettings.GenerateCritHit();
                m_LocationBasedDamageArea.DamageArea(DamageSettings.GenerateDamage(IsCritHit), Owner.transform, DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                DamageSettings.DamageTargetOverTime(this, DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                EmeraldComponent.AnimationComponent.PlayRecoilAnimation();
                if (EmeraldComponent.CombatComponent.DeathDelayTimer < 0.1f && !m_ICombat.IsBlocking() && !m_ICombat.IsDodging()) AbilityData.SpawnEffectAndSound(Owner, Target.transform.position, MeleeSettings.ImpactEffect, MeleeSettings.ImpactEffectTimeoutSeconds, MeleeSettings.ImpactSoundsList);
            }
        }

        /// <summary>
        /// Gets the target's root transform. This is used to get a reference to the ICombat interface as well as tracking which targets have been hit.
        /// </summary>
        public Transform GetTargetRoot (GameObject Target)
        {
            Transform TargetTransform = null;
            LocationBasedDamageArea m_LocationBasedDamageArea = Target.GetComponent<LocationBasedDamageArea>();
            if (m_LocationBasedDamageArea != null && m_LocationBasedDamageArea.EmeraldComponent.transform.localScale == Vector3.one * 0.003f || Target.transform.localScale == Vector3.one * 0.003f) return null;
            var m_ICombat = Target.GetComponentInParent<ICombat>();
            if (m_ICombat != null) TargetTransform = m_ICombat.TargetTransform();
            return TargetTransform;
        }
    }
}