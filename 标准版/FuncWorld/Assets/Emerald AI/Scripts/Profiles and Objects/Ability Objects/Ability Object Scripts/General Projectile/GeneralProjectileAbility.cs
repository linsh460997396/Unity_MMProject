using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "General Projectile Ability", menuName = "Emerald AI/Ability/General Projectile Ability")]
    public class GeneralProjectileAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.ProjectileData ProjectileSettings;
        [UnityEngine.Serialization.FormerlySerializedAs("LinearProjectileSettings")] public AbilityData.GeneralProjectileData GeneralProjectileSettings;
        public AbilityData.HomingData HomingSettings;
        public AbilityData.TargetTypeData TargetTypeSettings;
        public AbilityData.SpreadData SpreadSettings;
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
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(SpawnProjectiles(Owner, AttackTransform, GeneralProjectileSettings.TimeBetweenProjectiles));
        }

        IEnumerator SpawnProjectiles (GameObject Owner, Transform AttackTransform, float Delay)
        {
            Transform Target = GetTarget(Owner, TargetTypeSettings.TargetType);

            for (int i = 0; i < GeneralProjectileSettings.TotalProjectiles; i++)
            {
                EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
                if (EmeraldComponent != null)
                {
                    if (EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.IsGettingHit) yield break;
                }

                //Continue to get a new target each time a projectile is created
                if (TargetTypeSettings.TargetType == AbilityData.TargetTypes.MultipleRandomEnemies) Target = GetTarget(Owner, TargetTypeSettings.TargetType);

                Vector3 SpawnPosition = AttackTransform.position;

                if (SpreadSettings.Enabled && SpreadSettings.SpreadType == AbilityData.SpreadTypes.HorizontalRadius) SpawnPosition = Owner.transform.position + Owner.transform.localScale.y * Vector3.up;

                GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(ProjectileSettings.ProjectileEffect, SpawnPosition, ProjectileSettings.ProjectileEffect.transform.rotation);
                SpawnedProjectile.transform.localScale = ProjectileSettings.ProjectileEffect.transform.localScale;
                SpawnedProjectile.name = ProjectileSettings.ProjectileEffect.name;

                if (SpreadSettings.Enabled)
                {
                    //Evenly distributed projectiles based on given angle
                    if (SpreadSettings.SpreadType == AbilityData.SpreadTypes.HorizontalRadius)
                    {
                        float AnglePerStepX = ((SpreadSettings.SpreadAngleX / 2f) * 2) / (float)GeneralProjectileSettings.TotalProjectiles;
                        SpawnedProjectile.transform.LookAt(Owner.transform.position + Owner.transform.forward);
                        Vector3 AimDir = new Vector3(-SpreadSettings.TiltAngleY, (-(SpreadSettings.SpreadAngleX / 2f) + AnglePerStepX / 2f) + AnglePerStepX * i, 0);
                        SpawnedProjectile.transform.eulerAngles = SpawnedProjectile.transform.eulerAngles + AimDir;
                    }
                    //Random Spread Offset
                    else if (SpreadSettings.SpreadType == AbilityData.SpreadTypes.Random)
                    {
                        float spreadX = Random.Range(SpreadSettings.MinSpreadX, SpreadSettings.MaxSpreadX);
                        float spreadY = Random.Range(SpreadSettings.MinSpreadY, SpreadSettings.MaxSpreadY);
                        if (Target != null) SpawnedProjectile.transform.LookAt(Target.position);
                        Vector3 AimDir = new Vector3(-spreadY, spreadX, 0);
                        SpawnedProjectile.transform.eulerAngles = SpawnedProjectile.transform.eulerAngles + AimDir;
                    }
                }

                AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);

                if (Delay > 0) yield return new WaitForSeconds(Delay);
            }
        }

        /// <summary>
        /// Assign the ProjectileMovement script on the newly spawned projectile.
        /// </summary>
        public GeneralProjectile AssignScript(GameObject SpawnedProjectile)
        {
            var generalProjectile = SpawnedProjectile.GetComponent<GeneralProjectile>();
            if (generalProjectile == null) generalProjectile = SpawnedProjectile.AddComponent<GeneralProjectile>();
            generalProjectile.enabled = true;
            return generalProjectile;
        }
    }
}