using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Ground Projectile Ability", menuName = "Emerald AI/Ability/Ground Projectile Ability")]
    public class GroundProjectileAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.ProjectileData ProjectileSettings;
        public AbilityData.GroundProjectileData GroundProjectileSettings;
        public AbilityData.HomingData HomingSettings;
        public AbilityData.TargetTypeData TargetTypeSettings;
        public AbilityData.ColliderData ColliderSettings;
        public AbilityData.StunnedData StunnedSettings;
        public AbilityData.DamageData DamageSettings;

        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, TargetTypeSettings.TargetType);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(SpawnProjectiles(Owner, Target, GroundProjectileSettings.TimeBetweenProjectiles));
        }

        IEnumerator SpawnProjectiles(GameObject Owner, Transform Target, float Delay)
        {
            for (int i = 0; i < GroundProjectileSettings.TotalProjectiles; i++)
            {
                //Continue to get a new target each time a projectile is created
                if (TargetTypeSettings.TargetType == AbilityData.TargetTypes.MultipleRandomEnemies) Target = GetTarget(Owner, TargetTypeSettings.TargetType);

                Vector3 SpawnPosition = Owner.transform.position;
                GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
                SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
                SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;

                float AnglePerStepX = ((GroundProjectileSettings.AngleSpread / 2f) * 2) / (float)GroundProjectileSettings.TotalProjectiles;
                SpawnedProjectile.transform.LookAt(Owner.transform.position + Owner.transform.forward);
                Vector3 AimDir = new Vector3(0, (-(GroundProjectileSettings.AngleSpread / 2f) + AnglePerStepX / 2f) + AnglePerStepX * i, 0);
                SpawnedProjectile.transform.eulerAngles = SpawnedProjectile.transform.eulerAngles + AimDir;

                AssignAbilityScript(SpawnedProjectile).Initialize(Owner, Target, this);

                if (Delay > 0) yield return new WaitForSeconds(Delay);
            }

            yield return new WaitForSeconds(0f);
        }

        /// <summary>
        /// Assign the GroundProjectile script on the newly spawned projectile.
        /// </summary>
        public GroundProjectile AssignAbilityScript(GameObject SpawnedAbility)
        {
            var groundProjectile = SpawnedAbility.GetComponent<GroundProjectile>();
            if (groundProjectile == null) groundProjectile = SpawnedAbility.AddComponent<GroundProjectile>();
            return groundProjectile;
        }
    }
}