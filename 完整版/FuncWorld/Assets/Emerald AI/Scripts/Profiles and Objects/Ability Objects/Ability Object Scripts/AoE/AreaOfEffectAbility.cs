using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Area of Effect Ability", menuName = "Emerald AI/Ability/Area of Effect Ability")]
    public class AreaOfEffectAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.AreaOfEffectData AreaOfEffectSettings;
        public AbilityData.StunnedData StunnedSettings;
        public AbilityData.DamageData DamageSettings;

        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(StartAOE(Owner, AttackTransform));
        }

        IEnumerator StartAOE (GameObject Owner, Transform AttackTransform = null)
        {
            yield return new WaitForSeconds(AreaOfEffectSettings.Delay);

            Vector3 SpawnPosition = new Vector3(AttackTransform.position.x, Owner.transform.position.y, AttackTransform.position.z) + Vector3.up * AreaOfEffectSettings.HeightOffset;
            GameObject SpawnedAbility = AreaOfEffectSettings.SpawnAOEEffect(Owner, SpawnPosition);
            AssignScript(SpawnedAbility).Initialize(Owner, AttackTransform, this);
        }

        /// <summary>
        /// Assign the AreaOfEffect script on the newly spawned AOE effect.
        /// </summary>
        public AreaOfEffect AssignScript(GameObject SpawnedAbility)
        {
            var areaOfEffect = SpawnedAbility.GetComponent<AreaOfEffect>();
            if (areaOfEffect == null) areaOfEffect = SpawnedAbility.AddComponent<AreaOfEffect>();
            return areaOfEffect;
        }
    }
}