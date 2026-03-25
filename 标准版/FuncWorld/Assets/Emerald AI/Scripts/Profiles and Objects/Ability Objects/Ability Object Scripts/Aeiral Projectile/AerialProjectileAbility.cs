using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Aerial Projectile Ability", menuName = "Emerald AI/Ability/Aerial Projectile Ability")]
    public class AerialProjectileAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.ProjectileData ProjectileSettings;
        public AbilityData.HomingData HomingSettings;
        public AbilityData.AerialProjectileData AerialProjectileSettings;
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
            OwnerMonoBehaviour.StartCoroutine(SpawnAerialProjectiles(Owner, Target, AerialProjectileSettings.TimeBetweenProjectiles));
        }

        IEnumerator SpawnAerialProjectiles(GameObject Owner, Transform Target, float Delay)
        {
            yield return new WaitForSeconds(0.5f);

            if (Target == null) Target = Owner.GetComponent<EmeraldSystem>().CombatTarget;
            //If the target is still null after attempting to get the current target, cancel the ability. 
            if (Target == null) yield break;

            AerialProjectileSettings.SpawnAerialEffect(Owner, Target); //Spawns an aerial effect, given it's enabled.

            Vector3 StartingPosition = GetStartingPosition(Owner, Target);

            yield return new WaitForSeconds(0.25f);

            float theta = Mathf.PI * (3 - Mathf.Sqrt(5));

            for (int i = 0; i < AerialProjectileSettings.TotalProjectiles; i++)
            {
                //Continue to get a new target each time a projectile is created
                if (TargetTypeSettings.TargetType == AbilityData.TargetTypes.MultipleRandomEnemies)
                {
                    Target = GetTarget(Owner, TargetTypeSettings.TargetType);
                    if (AerialProjectileSettings.SpawnSource != AbilityData.AerialProjectileData.SpawnSources.AboveSelf)
                        AerialProjectileSettings.SpawnAerialEffect(Owner, Target);
                }

                Vector3 SpawnPosition = GetSpawnPosition(StartingPosition, theta, i);
                GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
                SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
                SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;
                SpawnedProjectile.transform.LookAt(Vector3.down);

                Vector3 AimDir = new Vector3(90 + Random.Range(-AerialProjectileSettings.LaunchAngle, AerialProjectileSettings.LaunchAngle), Random.Range(0, 360), 0);
                SpawnedProjectile.transform.eulerAngles = AimDir;

                AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);

                if (Delay > 0) yield return new WaitForSeconds(Delay);
            }

            yield return new WaitForSeconds(0f);
        }

        /// <summary>
        /// Assign the AerialProjectile script on the newly spawned projectile.
        /// </summary>
        public AerialProjectile AssignScript(GameObject SpawnedProjectile)
        {
            var aerialProjectile = SpawnedProjectile.GetComponent<AerialProjectile>();
            if (aerialProjectile == null) aerialProjectile = SpawnedProjectile.AddComponent<AerialProjectile>();
            aerialProjectile.enabled = true;
            return aerialProjectile;
        }

        /// <summary>
        /// Used to get the position when the ability is first called (so that if the StartingPosition moves, the projectiles remain above where they were originally cast).
        /// </summary>
        Vector3 GetStartingPosition (GameObject Owner, Transform Target)
        {
            if (AerialProjectileSettings.SpawnSource == AbilityData.AerialProjectileData.SpawnSources.AboveTarget)
            {
                return Target.transform.position;
            }
            else
            {
                return Owner.transform.position;
            }
        }

        /// <summary>
        /// Get the Spawn Position depending on the index and the total projectiles. This will allow all positions to be evenly distributed.
        /// </summary>
        Vector3 GetSpawnPosition (Vector3 StartingPosition, float theta, int Index)
        {
            float r = (AerialProjectileSettings.Radius) * Mathf.Sqrt(Index) / Mathf.Sqrt(AerialProjectileSettings.TotalProjectiles);
            float a = theta * Index;
            return StartingPosition + new Vector3(Mathf.Cos(a) * r, AerialProjectileSettings.HeightOffset, Mathf.Sin(a) * r);
        }
    }
}