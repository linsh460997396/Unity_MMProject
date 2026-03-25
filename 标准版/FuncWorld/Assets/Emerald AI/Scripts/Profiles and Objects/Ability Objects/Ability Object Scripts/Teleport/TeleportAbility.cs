using System.Collections;
using UnityEngine;
using EmeraldAI.Utility;
using UnityEngine.AI;

namespace EmeraldAI
{
    [CreateAssetMenu(fileName = "Teleport Ability", menuName = "Emerald AI/Ability/Teleport Ability")]
    public class TeleportAbility : EmeraldAbilityObject
    {
        public AbilityData.ChargeSettingsData ChargeSettings;
        public AbilityData.CreateSettingsData CreateSettings;
        public AbilityData.TeleportData TeleportSettings;

        public override void ChargeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            ChargeSettings.SpawnChargeEffect(Owner, AttackTransform);
        }

        public override void InvokeAbility(GameObject Owner, Transform AttackTransform = null)
        {
            MonoBehaviour OwnerMonoBehaviour = Owner.GetComponent<MonoBehaviour>();
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            CreateSettings.SpawnCreateEffect(Owner, AttackTransform);
            OwnerMonoBehaviour.StartCoroutine(InitializeTeleport(Owner, Target));
        }

        IEnumerator InitializeTeleport (GameObject Owner, Transform Target)
        {
            AbilityData.SpawnEffectAndSound(Owner, Owner.GetComponent<ICombat>().DamagePosition(), TeleportSettings.DisappearEffect, TeleportSettings.DisappearEffectTimeoutSeconds, TeleportSettings.DisappearSoundsList);
            EmeraldSystem EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            EmeraldComponent.m_NavMeshAgent.enabled = false;
            EmeraldComponent.AIAnimator.speed = 0.2f;

            yield return new WaitForSeconds(0.01f);
            Vector3 StartingScale = Owner.transform.localScale;
            Owner.transform.localScale = Vector3.one * 0.003f; //A scale of 0 cannot be used due to a Unity bug, but this value is small enough that the AI cannot be seen

            void ResetSettings()
            {
                Owner.transform.localScale = StartingScale;
                EmeraldComponent.m_NavMeshAgent.enabled = true;
                EmeraldComponent.AIAnimator.speed = 1;
            }

            //Wait to reappear until after the TeleportTime has lapsed.
            yield return new WaitForSeconds(TeleportSettings.TeleportTime - 0.5f);
            if (EmeraldComponent.CombatComponent == null) { ResetSettings(); yield break; } //If the target is lost while teleporting, cancel it.
            Vector3 TeleportPosition = GetTeleportPosition(Owner);
            EmeraldComponent.m_NavMeshAgent.Warp(TeleportPosition);
            yield return new WaitForSeconds(0.1f);
            if (TeleportSettings.ReappearTriggersAvoidable) EmeraldComponent.AnimationComponent.AttackTriggered = true;

            AbilityData.SpawnEffectAndSound(Owner, Owner.GetComponent<ICombat>().DamagePosition() + Vector3.up * StartingScale.y, TeleportSettings.ReappearIndicatorEffect, TeleportSettings.ReappearIndicatorEffectTimeoutSeconds, TeleportSettings.ReappearIndicatorSoundsList);
            yield return new WaitForSeconds(TeleportSettings.ReappearDelay);

            AbilityData.SpawnEffectAndSound(Owner, Owner.GetComponent<ICombat>().DamagePosition() + Vector3.up * StartingScale.y, TeleportSettings.ReappearEffect, TeleportSettings.ReappearEffectTimeoutSeconds, TeleportSettings.ReappearSoundsList);

            EmeraldComponent.MovementComponent.InstantlyRotateTowards(Target.position);

            ResetSettings();
            if (TeleportSettings.ReappearTriggersAvoidable)
            {
                yield return new WaitForSeconds(0.1f);
                EmeraldComponent.AnimationComponent.AttackTriggered = false;
            }
        }

        Vector3 GetTeleportPosition(GameObject Owner)
        {
            //Generate a random position (within 180 degrees) behind the specified target within the set radius.
            float RandomDegree = Random.Range(0f, 1f);
            Transform Target = GetTarget(Owner, AbilityData.TargetTypes.CurrentTarget);
            if (Target == null) Target = Owner.transform;
            Vector3 TargetPosition = Target.position;
            Vector3 TeleportPosition = Owner.transform.position;
            bool TeleportRight = Random.Range(0f, 1f) <= 0.5f;

            if (TeleportRight)
            {
                TeleportPosition = TargetPosition + ((TargetPosition - Owner.transform.position).normalized + (Vector3.Lerp(Owner.transform.right, Owner.transform.forward, RandomDegree) * TeleportSettings.TeleportRadius));
            }
            else
            {
                TeleportPosition = TargetPosition + ((TargetPosition - Owner.transform.position).normalized + (Vector3.Lerp(-Owner.transform.right, Owner.transform.forward, RandomDegree) * TeleportSettings.TeleportRadius));
            }

            RaycastHit hit;
            if (Physics.Raycast(TeleportPosition, Owner.transform.TransformDirection(Vector3.down), out hit, 10))
            {
                TeleportPosition = new Vector3(TeleportPosition.x, hit.point.y, TeleportPosition.z);
            }

            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(TeleportPosition, out navMeshHit, 10f, NavMesh.AllAreas))
            {
                TeleportPosition = navMeshHit.position;
            }

            return TeleportPosition;
        }
    }
}