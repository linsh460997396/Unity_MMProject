using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Grenade Ability", menuName = "Emerald AI/Ability/Grenade Ability")]
    public class GrenadeAbility : EmeraldAbilityObject
    {
        public AbilityData.GrenadeData GrenadeSettings;
        public AbilityData.StunnedData StunnedSettings;
        public AbilityData.DamageData DamageSettings;

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null) 
        {
            SpawnProjectiles(Owner, AttackTransform);
        }

        void SpawnProjectiles (GameObject Owner, Transform AttackTransform)
        {
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);

            Vector3 SpawnPosition = AttackTransform.position;
            GameObject SpawnedProjectile = EmeraldObjectPool.Spawn(GrenadeSettings.GrenadeObject, SpawnPosition, GrenadeSettings.GrenadeObject.transform.rotation);
            SpawnedProjectile.transform.localScale = GrenadeSettings.GrenadeObject.transform.localScale;
            SpawnedProjectile.name = GrenadeSettings.GrenadeObject.name;

            AssignScript(SpawnedProjectile).Initialize(Owner, Target, this);
        }

        /// <summary>
        /// Assign the ProjectileMovement script on the newly spawned projectile.
        /// </summary>
        public Grenade AssignScript(GameObject SpawnedProjectile)
        {
            var grenade = SpawnedProjectile.GetComponent<Grenade>();
            if (grenade == null) grenade = SpawnedProjectile.AddComponent<Grenade>();
            grenade.enabled = true;
            return grenade;
        }
    }
}