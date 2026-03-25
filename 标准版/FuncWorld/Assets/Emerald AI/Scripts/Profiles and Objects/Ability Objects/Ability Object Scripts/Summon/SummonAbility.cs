using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Summon Ability", menuName = "Emerald AI/Ability/Summon Ability")]
    public class SummonAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.SummonData SummonSettings;

        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(IntitailizeSummon(Owner, OwnerMonoBehaviour, AttackTransform));
        }

        IEnumerator IntitailizeSummon(GameObject Owner, MonoBehaviour OwnerMonoBehaviour, Transform AttackTransform = null)
        {
            yield return new WaitForSeconds(SummonSettings.SummonDelay);

            Vector3 EffectPosition = new Vector3(AttackTransform.position.x, AttackTransform.position.y, AttackTransform.position.z);
            SummonSettings.SpawnEffect(Owner, EffectPosition, SummonSettings.CastEffect, SummonSettings.CastEffectTimeoutSeconds, SummonSettings.CastSounds, false);

            for (int i = 0; i < SummonSettings.SummonAmount; i++)
            {
                //Calculate the angle for each object
                float angle = i * Mathf.PI * 2f / SummonSettings.SummonAmount;

                //Get the summon position (depending on the setting)
                Vector3 SummonPosition = Vector3.zero;
                if (SummonSettings.SummonPosition == AbilityData.SummonData.SummonPositions.Self) SummonPosition = Owner.transform.position;
                else if (SummonSettings.SummonPosition == AbilityData.SummonData.SummonPositions.Target)
                {
                    EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
                    if (EmeraldComponent.CombatTarget != null) SummonPosition = EmeraldComponent.CombatTarget.position;
                    else SummonPosition = Owner.transform.position; //If for some reason the CombatTarget is null, fallback to summoning around the caster.
                }

                //Calculate the position based on the angle
                Vector3 SpawnPosition = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * SummonSettings.SummonRadius + SummonPosition;

                //Get a random index from the AIPrefabs list
                int RandomIndex = Random.Range(0, SummonSettings.AIPrefabs.Count);

                //Spawn the AI prefab - Skipp the summon sound after the first index to avoid too many sounds playing at once.
                if (i == 0) OwnerMonoBehaviour.StartCoroutine(SummonAlly(Owner, RandomIndex, SpawnPosition, false));
                else OwnerMonoBehaviour.StartCoroutine(SummonAlly(Owner, RandomIndex, SpawnPosition, true));
            }
        }

        /// <summary>
        /// Summons an AI for the caster.
        /// </summary>
        IEnumerator SummonAlly(GameObject Owner, int RandomIndex, Vector3 SpawnPosition, bool SkipSummonSound)
        {
            yield return new WaitForSeconds(SummonSettings.SummonDelay);

            EmeraldSystem AllyEmeraldComponent = EmeraldObjectPool.Spawn(SummonSettings.AIPrefabs[RandomIndex], SpawnPosition, Quaternion.identity).GetComponent<EmeraldSystem>();

            yield return new WaitForSeconds(0.01f);

            EmeraldAPI.Detection.InitializeSummonTarget(AllyEmeraldComponent, Owner.transform);

            SummonSettings.SpawnEffect(Owner, AllyEmeraldComponent.GetComponent<ICombat>().DamagePosition() + (Vector3.up * SummonSettings.SummonEffectHeightOffset), SummonSettings.SummonEffect, SummonSettings.SummonEffectTimeoutSeconds, SummonSettings.SummonSounds, SkipSummonSound);

            if (SummonSettings.IsTimedSummon)
            {
                yield return new WaitForSeconds(SummonSettings.SummonLength);
                EmeraldAPI.Combat.KillAI(AllyEmeraldComponent);
            }

            if (SummonSettings.IsTimedSummon)
            {
                yield return new WaitForSeconds(SummonSettings.DespawnLength);
                EmeraldObjectPool.Despawn(AllyEmeraldComponent.gameObject);
            }
            else
            {
                yield return new WaitUntil(() => AllyEmeraldComponent.AnimationComponent.IsDead);
                yield return new WaitForSeconds(SummonSettings.DespawnLength);
                EmeraldObjectPool.Despawn(AllyEmeraldComponent.gameObject);
            }
        }
    }
}