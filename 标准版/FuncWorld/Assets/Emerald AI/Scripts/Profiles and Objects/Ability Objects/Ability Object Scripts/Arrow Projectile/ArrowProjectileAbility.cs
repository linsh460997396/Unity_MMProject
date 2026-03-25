using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Arrow Projectile Ability", menuName = "Emerald AI/Ability/Arrow Projectile Ability")]
    public class ArrowProjectileAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.ProjectileData ProjectileSettings;
        public AbilityData.ArrowProjectileData ArrowProjectileSettings;
        public AbilityData.ColliderData ColliderSettings;
        public AbilityData.StunnedData StunnedSettings;
        public AbilityData.DamageData DamageSettings;

        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null) 
        {
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            SpawnProjectiles(Owner, AttackTransform);
        }

        void SpawnProjectiles (GameObject Owner, Transform AttackTransform)
        {
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);

            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            if (EmeraldComponent != null)
            {
                if (EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.IsGettingHit) return;
            }

            Vector3 SpawnPosition = AttackTransform.position;
            GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
            SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
            SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;

            AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);
        }

        /// <summary>
        /// Assign the ArrowProjectile script on the newly spawned projectile.
        /// </summary>
        public ArrowProjectile AssignScript(GameObject SpawnedProjectile)
        {
            var arrowProjectile = SpawnedProjectile.GetComponent<ArrowProjectile>();
            if (arrowProjectile == null) arrowProjectile = SpawnedProjectile.AddComponent<ArrowProjectile>();
            arrowProjectile.enabled = true;
            return arrowProjectile;
        }
    }
}