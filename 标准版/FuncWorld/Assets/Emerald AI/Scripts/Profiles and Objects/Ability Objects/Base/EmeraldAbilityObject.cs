using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EmeraldAI
{
    public class EmeraldAbilityObject : ScriptableObject
    {
        public string AbilityName = "New Ability";
        public string AbilityDescription = "Ability Description";
        public Texture2D AbilityIcon;
        public bool InfoSettingsFoldout = true;
        public bool DerivedSettingsFoldout;
        public bool ModularSettingsFoldout;
        public bool HideSettingsFoldout;

        /// <summary>
        /// This is used internally as a way to generate abilities with cooldown restrictions.
        /// It is recommended that this is used if custom abilities are created that require cooldowns.
        /// However, this variable is entirely optional.
        /// </summary>
        public AbilityData.CooldownData CooldownSettings;

        //Testing
        public AbilityData.ConditionData ConditionSettings;

        public virtual void ChargeAbility(GameObject Owner, Transform AttackTransform = null) { }

        public virtual void InvokeAbility(GameObject Owner, Transform AttackTransform = null) { }

        /// <summary>
        /// Get the ability's current target depending on the TargetType module.
        /// </summary>
        public virtual Transform GetTarget(GameObject Owner, AbilityData.TargetTypes TargetType)
        {
            Transform Target = null;

            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();

            if (TargetType == AbilityData.TargetTypes.MultipleRandomEnemies || TargetType == AbilityData.TargetTypes.SingleRandomEnemy)
            {
                if (EmeraldComponent.DetectionComponent.LineOfSightTargets.Count > 0) Target = EmeraldComponent.DetectionComponent.LineOfSightTargets[Random.Range(0, EmeraldComponent.DetectionComponent.LineOfSightTargets.Count)].transform;
            }
            else if (TargetType == AbilityData.TargetTypes.CurrentTarget)
            {
                Target = EmeraldComponent.CombatTarget;
            }

            return Target;
        }
    }
}