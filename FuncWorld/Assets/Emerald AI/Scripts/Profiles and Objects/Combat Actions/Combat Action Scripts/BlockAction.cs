using UnityEngine;
using EmeraldAI.Utility;
using System.Collections.Generic;
namespace EmeraldAI
{
    /// <summary>
    /// A modular action giving AI the ability to detect melee and projectiles attacks and block them.
    /// </summary>
    [CreateAssetMenu(fileName = "Block Action", menuName = "Emerald AI/Combat Action/Block Action")]
    public class BlockAction : EmeraldAction
    {
        [Range(1, 3)] [Tooltip("Controls how long the a successfully generated block will last.")] public float BlockLength = 1;
        [Range(1, 8)] [Tooltip("The radius for detecting close range attacks.")] public float MeleeDetectionRadius = 3f;
        [Range(0, 360)] [Tooltip("The max angle for detecting attacks that will trigger a block. Any angles greater than this will not allow a block to trigger.")] public float MaxBlockAngle = 60f;
        [Range(0, 100)] [Tooltip("The percentage of damage that will be mitigated when an AI is blocking and receives damage.")] public int MitigationAmount = 50;
        [Range(1, 10)] [Tooltip("The radius for detecting incoming projectiles.")] public float ProjectileDetectionRadius = 3.5f;
        [Tooltip("The projectile layers needed to trigger a block. This is based off of the Projectile Layer set within each projectile. By default, this is Ignore Raycast.")] public LayerMask ProjectileLayers = 1 << 2;
        [Range(0, 1)][Tooltip("The odds for a block, given the needed conditions are met.")] public float OddsToBlock = 0.5f;

        /// <summary>
        /// Continiously updates the EmeraldAction. This acts like an Update function that can run within this action using the information from the passed EmeraldComponent and its ActionClass.
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            BlockActionUpdate(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// Continuously updates to check for incoming attacks from the current target.
        /// </summary>
        void BlockActionUpdate (EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CurrentTargetInfo.CurrentICombat.IsAttacking() && EmeraldComponent.CombatComponent.DistanceFromTarget <= MeleeDetectionRadius && EmeraldComponent.CombatComponent.TargetAngle < MaxBlockAngle / 2f)
                {
                    SetBlockState(EmeraldComponent, ActionClass, true);
                }
                if (CanExecute(EmeraldComponent, ActionClass) && EmeraldComponent.CombatComponent.DistanceFromTarget > MeleeDetectionRadius && EmeraldComponent.CombatComponent.TargetAngle < MaxBlockAngle / 2f)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(EmeraldComponent.transform.position, ProjectileDetectionRadius, ProjectileLayers);

                    for (int i = 0; i < hitColliders.Length; i++)
                    {
                        var IAvoidableRef = hitColliders[i].GetComponent<IAvoidable>();

                        if (IAvoidableRef != null && IAvoidableRef.AbilityTarget == EmeraldComponent.transform)
                        {
                            SetBlockState(EmeraldComponent, ActionClass, true);
                            break;
                        }
                    }
                }
            }
            else
            {
                ActionClass.ActionLengthTimer += Time.deltaTime;

                //Note: Exit conditions for blocking are handled internally below
                if (CanExit(EmeraldComponent, ActionClass))
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }

                //Due to Unity's Animator and transitions, these need to be checked in order to reliably catch hits between transitions and states.
                if (EmeraldComponent.AnimationComponent.IsGettingHit)
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }
                if (!EmeraldComponent.AnimationComponent.IsBlocking && EmeraldComponent.AIAnimator.GetBool("Hit"))
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }    
                if (!EmeraldComponent.AIAnimator.GetBool("Blocking") && EmeraldComponent.AIAnimator.GetBool("Hit"))
                {
                    SetBlockState(EmeraldComponent, ActionClass, false);
                }
            }
        }

        /// <summary>
        /// Sets the block state according to the passed State parameter.
        /// </summary>
        void SetBlockState (EmeraldSystem EmeraldComponent, ActionsClass ActionClass, bool State)
        {
            //Roll for a chance to block
            float Roll = Random.Range(0f, 1f);

            if (Roll > OddsToBlock && State)
            {
                EmeraldComponent.AIAnimator.SetBool("Blocking", false);
                ActionClass.IsActive = false;
                ActionClass.ActionLengthTimer = 0;
                ActionClass.CooldownLengthTimer = 0;
                return;
            }

            if (State)
            {
                EmeraldComponent.CombatComponent.AdjustCooldowns();
                //Set the current mitigation amount and angle equal to those from this action
                EmeraldComponent.CombatComponent.MitigationAmount = MitigationAmount;
                EmeraldComponent.CombatComponent.MaxMitigationAngle = MaxBlockAngle;
                EmeraldComponent.AnimationComponent.AttackTriggered = false;
                EmeraldComponent.AIAnimator.ResetTrigger("Dodge Triggered"); //Stop dodge in case it was triggered at the same time as a block
                EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            }

            EmeraldComponent.AnimationComponent.InternalBlock = State;
            EmeraldComponent.AIAnimator.SetBool("Blocking", State);
            ActionClass.IsActive = State;
            ActionClass.ActionLengthTimer = 0;
            ActionClass.CooldownLengthTimer = 0;

        }

        /// <summary>
        /// Conditions required for the EmeraldAction to execute.
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return EmeraldComponent.DetectionComponent.ObstructionType != EmeraldDetection.ObstructedTypes.Other && ActionClass.CooldownLengthTimer >= CooldownLength && !EmeraldComponent.AIAnimator.GetBool("Attack") && Conditions && 
                !EmeraldComponent.AIAnimator.GetBool("Blocking") && !EmeraldComponent.AIAnimator.GetBool("Hit") && !EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") && !EmeraldComponent.AnimationComponent.InternalHit;
        }

        /// <summary>
        /// Check for other conditions to exit blocking.
        /// </summary>
        bool CanExit(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            return (ActionClass.ActionLengthTimer >= BlockLength || EmeraldComponent.CombatTarget == null || EmeraldComponent.AnimationComponent.IsStunned || EmeraldComponent.AIAnimator.GetBool("Stunned Active") || EmeraldComponent.CombatComponent.TargetAngle > MaxBlockAngle);
        }
    }
}