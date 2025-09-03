using UnityEngine;
using System.Collections;

namespace EmeraldAI
{
    /// <summary>
    /// A modular action giving AI the ability to detect melee and projectiles attacks and dodge them.
    /// </summary>
    [CreateAssetMenu(fileName = "Dodge Action", menuName = "Emerald AI/Combat Action/Dodge Action")]
    public class DodgeAction : EmeraldAction
    {
        [Range(1, 8)] [Tooltip("The radius for detecting close range attacks.")] public float MeleeDetectionRadius = 3f;
        [Range(0, 360f)] [Tooltip("The max angle for detecting attacks that will trigger a dodge. Any angles greater than this will not allow a dodge to trigger.")] public float MaxDodgeAngle = 60f;
        [Range(0, 100)] [Tooltip("The percentage of damage that will be mitigated when an AI is dodging and receives damage.")] public int MitigationAmount = 100;
        [Range(1, 10)] [Tooltip("The radius for detecting incoming projectiles.")] public float ProjectileDetectionRadius = 3.5f;
        [Tooltip("The projectile layers needed to trigger a dodge. This is based off of the Projectile Layer set within each projectile. By default, this is Ignore Raycast.")] public LayerMask ProjectileLayers = 1 << 2;
        [Range(0, 1)] [Tooltip("The odds for a dodge, given the needed conditions are met.")] public float OddsToDodge = 0.5f;

        /// <summary>
        /// Continiously updates the EmeraldAction. This acts like an Update function that can run within this action using the information from the passed EmeraldComponent and its ActionClass.
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            DodgeActionUpdate(EmeraldComponent, ActionClass);
        }

        void DodgeActionUpdate(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            ActionClass.IsActive = EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.InternalDodge;

            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CurrentTargetInfo.CurrentICombat.IsAttacking() && EmeraldComponent.CombatComponent.DistanceFromTarget <= MeleeDetectionRadius && EmeraldComponent.CombatComponent.TargetAngle < MaxDodgeAngle / 2f)
                {
                    TriggerDodge(EmeraldComponent, ActionClass);
                }

                //Look for the IAvoidable interface script on objects who's layers are within the ProjectileLayers layermask
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CombatComponent.TargetAngle < MaxDodgeAngle / 2f)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(EmeraldComponent.transform.position, ProjectileDetectionRadius, ProjectileLayers);

                    for (int i = 0; i < hitColliders.Length; i++)
                    {
                        var IAvoidableRef = hitColliders[i].GetComponent<IAvoidable>();

                        if (IAvoidableRef != null && IAvoidableRef.AbilityTarget == EmeraldComponent.transform)
                        {
                            TriggerDodge(EmeraldComponent, ActionClass);
                            break;
                        }
                    }
                }
            }

            if (ActionClass.IsActive)
            {
                EmeraldComponent.CombatComponent.CombatActionActive = ActionClass.IsActive;

                var Conditions = (((int)ExitConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
                if (Conditions)
                {
                    EmeraldComponent.AnimationComponent.ResetTriggers(0);
                }
            }
        }

        void TriggerDodge (EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            //Roll for a chance to block
            float Roll = Random.Range(0f, 1f);
            ActionClass.CooldownLengthTimer = 0;
            if (Roll > OddsToDodge)
            {
                return;
            }

            if (EmeraldComponent.AnimationComponent.InternalHit) return;

            EmeraldComponent.AnimationComponent.InternalDodge = true;
            //Set the current mitigation amount and angle equal to those from this action
            EmeraldComponent.CombatComponent.MitigationAmount = MitigationAmount;
            EmeraldComponent.CombatComponent.MaxMitigationAngle = MaxDodgeAngle;
            EmeraldComponent.AIAnimator.SetBool("Blocking", false); //Stop block in case it was triggered at the same time as a dodge
            EmeraldComponent.AnimationComponent.TriggerDodgeState();
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AnimationComponent.AttackTriggered = false;
            EmeraldComponent.MovementComponent.SetActionDirection();
            if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.BehaviorsComponent.IsAiming = true;
            EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(CancelAttack(EmeraldComponent));
        }

        /// <summary>
        /// Reset Attack in case it was triggered during the dodge (which can sometimes happen with delays and animation transitions).
        /// </summary>
        IEnumerator CancelAttack (EmeraldSystem EmeraldComponent)
        {
            yield return new WaitForSeconds(0.4f);
            EmeraldComponent.AnimationComponent.InternalDodge = false;
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
        }

        /// <summary>
        /// Conditions required for the EmeraldAction to execute.
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return EmeraldComponent.DetectionComponent.ObstructionType != EmeraldDetection.ObstructedTypes.Other && ActionClass.CooldownLengthTimer >= CooldownLength && Conditions &&
                !EmeraldComponent.AIAnimator.GetBool("Attack") && !EmeraldComponent.AIAnimator.GetBool("Blocking") && !EmeraldComponent.AIAnimator.GetBool("Hit") && !EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") && !EmeraldComponent.AnimationComponent.InternalHit;

        }
    }
}