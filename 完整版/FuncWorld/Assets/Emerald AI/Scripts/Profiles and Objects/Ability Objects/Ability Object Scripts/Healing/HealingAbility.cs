using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Healing Ability", menuName = "Emerald AI/Ability/Healing Ability")]
    public class HealingAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.HealingData HealingSettings;

        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(StartHeals(Owner, AttackTransform));
        }

        IEnumerator StartHeals (GameObject Owner, Transform AttackTransform = null)
        {
            yield return new WaitForSeconds(HealingSettings.Delay);

            Vector3 EffectPosition = new Vector3(AttackTransform.position.x, AttackTransform.position.y, AttackTransform.position.z) + Vector3.up * HealingSettings.EffectHeightOffset;
            GameObject SpawnedEffect = HealingSettings.SpawnHealingEffect(Owner, EffectPosition, HealingSettings.HealingEffect, HealingSettings.HealingEffectTimeoutSeconds, HealingSettings.HealingSoundsList);

            if (HealingSettings.TargetType == AbilityData.HealingData.TargetTypes.Area) IntitailizeAreaHealing(Owner.GetComponent<EmeraldSystem>(), AttackTransform);
            else if (HealingSettings.TargetType == AbilityData.HealingData.TargetTypes.Self) IntitailizeSelfHealing(Owner.GetComponent<EmeraldSystem>(), AttackTransform);
            else if (HealingSettings.TargetType == AbilityData.HealingData.TargetTypes.Target) IntitailizeTargetHealing(Owner.GetComponent<EmeraldSystem>(), AttackTransform);
        }

        void IntitailizeAreaHealing(EmeraldSystem OwnerEmeraldComponent, Transform AttackTransform)
        {
            OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Clear();
            OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Add(OwnerEmeraldComponent); //Add the caster to the list of allies to heal

            for (int i = 0; i < OwnerEmeraldComponent.DetectionComponent.NearbyAllies.Count; i++)
            {
                //Only heal targets in range
                if (Vector3.Distance(OwnerEmeraldComponent.transform.position, OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i].transform.position) < HealingSettings.Radius)
                {
                    OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Add(OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i]);
                }
            }

            for (int i = 0; i < OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Count; i++)
            {
                EmeraldSystem TargetEmeraldComponent = OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].GetComponent<EmeraldSystem>();

                if (TargetEmeraldComponent != null)
                {
                    //Only damage targets that the Owner has an Enemy Relation Type with.
                    if (EmeraldAPI.Faction.GetTargetFactionRelation(OwnerEmeraldComponent, OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].transform) == "Friendly" ||
                        EmeraldAPI.Faction.GetTargetFactionName(OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].transform) == EmeraldAPI.Faction.GetTargetFactionName(OwnerEmeraldComponent.transform))
                    {
                        if (HealingSettings.HealTargetEffect != null)
                        {
                            EmeraldObjectPool.SpawnEffect(HealingSettings.HealTargetEffect, OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].GetComponent<ICombat>().DamagePosition(), OwnerEmeraldComponent.DetectionComponent.LowHealthAllies[i].transform.rotation, HealingSettings.HealTargetEffectTimeoutSeconds);
                        }

                        HealTarget(TargetEmeraldComponent);
                    }
                }
            }
        }

        void IntitailizeSelfHealing(EmeraldSystem OwnerEmeraldComponent, Transform AttackTransform)
        {
            if (HealingSettings.HealTargetEffect != null)
            {
                EmeraldObjectPool.SpawnEffect(HealingSettings.HealTargetEffect, OwnerEmeraldComponent.GetComponent<ICombat>().DamagePosition(), OwnerEmeraldComponent.transform.rotation, HealingSettings.HealTargetEffectTimeoutSeconds);
            }

            HealTarget(OwnerEmeraldComponent);
        }

        void IntitailizeTargetHealing(EmeraldSystem OwnerEmeraldComponent, Transform AttackTransform)
        {
            OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Clear();

            for (int i = 0; i < OwnerEmeraldComponent.DetectionComponent.NearbyAllies.Count; i++)
            {
                //Only look for AI that are not dead.
                if (!OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i].AnimationComponent.IsDead)
                {
                    OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Add(OwnerEmeraldComponent.DetectionComponent.NearbyAllies[i]);
                }
            }

            //Search through nearby allies and only heal the ally target with the lowest health
            if (OwnerEmeraldComponent.DetectionComponent.LowHealthAllies.Count > 0)
            {
                EmeraldSystem TargetEmeraldComponent = null;
                float lowestHealth = float.MaxValue;

                foreach (var Ally in OwnerEmeraldComponent.DetectionComponent.LowHealthAllies)
                {
                    float AllyHealth = (float)Ally.HealthComponent.CurrentHealth / (float)Ally.HealthComponent.StartingHealth;
                    if (AllyHealth < lowestHealth)
                    {
                        lowestHealth = AllyHealth;
                        TargetEmeraldComponent = Ally;
                    }
                }

                if (TargetEmeraldComponent && HealingSettings.HealTargetEffect != null)
                {
                    EmeraldObjectPool.SpawnEffect(HealingSettings.HealTargetEffect, TargetEmeraldComponent.GetComponent<ICombat>().DamagePosition(), TargetEmeraldComponent.transform.rotation, HealingSettings.HealTargetEffectTimeoutSeconds);
                }

                if (TargetEmeraldComponent)
                {
                    HealTarget(TargetEmeraldComponent);
                }
            }
        }

        /// <summary>
        /// Heals each detected friendly target within the AI's healing radius.
        /// </summary>
        void HealTarget(EmeraldSystem TargetEmeraldComponent)
        {
            HealAI(TargetEmeraldComponent, HealingSettings.BaseHealAmount);

            if (HealingSettings.HealingType == AbilityData.HealingData.HealingTypes.OverTime)
            {
                HealAIOverTime(TargetEmeraldComponent);
            }
        }

        /// <summary>
        /// Heals an AI instantly according to the HealAmount. This is also called through healing abilities to heal an AI.
        /// </summary>
        public void HealAI(EmeraldSystem TargetEmeraldComponent, int HealAmount)
        {
            EmeraldHealth HealthRef = TargetEmeraldComponent.HealthComponent;
            HealthRef.CurrentHealth = HealthRef.CurrentHealth + HealAmount;
            //Don'transform allow the heals to heal more than the AI's Starting Health.
            if (HealthRef.CurrentHealth >= HealthRef.StartingHealth) HealthRef.CurrentHealth = HealthRef.StartingHealth;
            CombatTextSystem.Instance.CreateCombatTextAI(HealAmount, TargetEmeraldComponent.CombatComponent.DamagePosition(), false, true);
            HealthRef.UpdateHealingReceived();
            HealthRef.UpdateHealTick();
        }

        /// <summary>
        /// Heals an AI over time. This is also called through heal over time abilities to heal an AI.
        /// </summary>
        public void HealAIOverTime(EmeraldSystem TargetEmeraldComponent)
        {
            TargetEmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(HealAIOverTimeInternal(TargetEmeraldComponent));
        }

        IEnumerator HealAIOverTimeInternal(EmeraldSystem TargetEmeraldComponent)
        {
            float t = 0;
            float LapsedTime = 0;
            EmeraldHealth HealthRef = TargetEmeraldComponent.HealthComponent;

            HealthRef.UpdateHealingReceived();

            while (LapsedTime <= HealingSettings.HealOverTimeLength)
            {
                t += Time.deltaTime;
                LapsedTime += Time.deltaTime;

                if (t >= HealingSettings.TickRate)
                {
                    HealthRef.CurrentHealth = HealthRef.CurrentHealth + HealingSettings.HealsPerTick;
                    Vector3 RefPosition = TargetEmeraldComponent.CombatComponent.DamagePosition();
                    CombatTextSystem.Instance.CreateCombatTextAI(HealingSettings.HealsPerTick, RefPosition, false, true);
                    HealingSettings.SpawnHealingEffect(TargetEmeraldComponent.gameObject, RefPosition, HealingSettings.HealTargetEffect, HealingSettings.HealTargetEffectTimeoutSeconds, HealingSettings.HealTickSounds);
                    HealthRef.UpdateHealTick();
                    t = 0;
                }

                //Stop healing over time if the healing target dies.
                if (TargetEmeraldComponent.HealthComponent.CurrentHealth <= 0) yield break;

                //Don'transform allow the heals to heal more than the AI's Starting Health.
                if (HealthRef.CurrentHealth >= HealthRef.StartingHealth) HealthRef.CurrentHealth = HealthRef.StartingHealth;

                yield return null;
            }
        }
    }
}