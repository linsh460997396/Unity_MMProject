using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// A modular action giving AI the ability to strafe around their targets.
    /// </summary>
    [CreateAssetMenu(fileName = "Strafe Action", menuName = "Emerald AI/Combat Action/Strafe Action")]    
    public class StrafeAction : EmeraldAction
    {
        [Range(0.5f, 8)] [Tooltip("The minimum length (in seconds) a strafe can last.")] public float StrafingLengthMin = 1;
        [Range(1, 8)] [Tooltip("The maximum length (in seconds) a strafe can last.")] public float StrafingLengthMax = 2f;
        [Range(0, 1)] [Tooltip("The odds for a strafe, given the needed conditions are met.")] public float OddsToStrafe = 0.5f;

        /// <summary>
        /// Continiously updates the EmeraldAction. This acts like an Update function that can run within this action using the information from the passed EmeraldComponent and its ActionClass.
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            StrafeActionUpdate(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// Updates the strafe action using the UpdateAction.
        /// </summary>
        void StrafeActionUpdate (EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass))
                {
                    SetStrafeState(EmeraldComponent, ActionClass, true);
                }
            }
            else
            {
                ActionClass.ActionLengthTimer += Time.deltaTime;
                var Conditions = (((int)ExitConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;

                //Check conditions for exiting strafe
                if (Conditions || CanExit(EmeraldComponent, ActionClass))
                {
                    SetStrafeState(EmeraldComponent, ActionClass, false);
                }
            }
        }

        /// <summary>
        /// Cast a raycast in the direction the AI is strafing and returns the condition.
        /// </summary>
        bool StrafeAvoidance(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            int Direction = EmeraldComponent.AIAnimator.GetInteger("Strafe Direction");
            return Direction == 0 && Physics.Raycast(EmeraldComponent.DetectionComponent.HeadTransform.position, -EmeraldComponent.transform.right * 3, 1, EmeraldComponent.MovementComponent.BackupLayerMask) ||
                Direction == 1 && Physics.Raycast(EmeraldComponent.DetectionComponent.HeadTransform.position, EmeraldComponent.transform.right * 3, 1, EmeraldComponent.MovementComponent.BackupLayerMask);
        }

        /// <summary>
        /// Conditions required for the EmeraldAction to execute.
        /// </summary>
        bool CanExecute (EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return (Mathf.Round(EmeraldComponent.CombatComponent.DistanceFromTarget * 10) / 10) <= (Mathf.Round(EmeraldComponent.m_NavMeshAgent.stoppingDistance * 10) / 10) + 0.1f && ActionClass.CooldownLengthTimer >= CooldownLength &&
                (Mathf.Round(EmeraldComponent.m_NavMeshAgent.remainingDistance * 10) / 10) >= (Mathf.Round(EmeraldComponent.CombatComponent.TooCloseDistance * 10) / 10) / 2f && 
                Conditions && !EmeraldComponent.AIAnimator.GetBool("Attack") && !EmeraldComponent.AIAnimator.GetBool("Walk Backwards") && !EmeraldComponent.AIAnimator.GetBool("Blocking") && 
                !EmeraldComponent.AIAnimator.GetBool("Hit") && !EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") && EmeraldComponent.DetectionComponent.ObstructionType != EmeraldDetection.ObstructedTypes.Other &&
                EmeraldComponent.CombatComponent.TargetAngle < 60 && EmeraldComponent.CombatTarget.localScale != Vector3.one * 0.003f && EmeraldComponent.transform.localScale != Vector3.one * 0.003f;
        }

        /// <summary>
        /// Check for other conditions to exit strafe.
        /// </summary>
        bool CanExit(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            return (ActionClass.ActionLengthTimer >= ActionClass.ActionLength || EmeraldComponent.AIAnimator.GetBool("Hit") || EmeraldComponent.CombatTarget == null || StrafeAvoidance(EmeraldComponent, ActionClass) || EmeraldComponent.CombatTarget.localScale == Vector3.one * 0.003f || 
                EmeraldComponent.transform.localScale == Vector3.one * 0.003f || EmeraldComponent.AnimationComponent.IsBackingUp || EmeraldComponent.AnimationComponent.IsEquipping || EmeraldComponent.CombatComponent.TargetAngle > 60);
        }

        /// <summary>
        /// Sets the starfe state according to the passed bool.
        /// </summary>
        void SetStrafeState(EmeraldSystem EmeraldComponent, ActionsClass ActionClass, bool State)
        {
            //Roll for a chance to strafe
            float Roll = Random.Range(0f, 1f);

            if (Roll <= OddsToStrafe && State)
            {
                EmeraldComponent.CombatComponent.AdjustCooldowns();
                EmeraldComponent.AnimationComponent.SetStrafeState(State);
                ActionClass.IsActive = State;
            }
            
            if (!State) EmeraldComponent.AnimationComponent.SetStrafeState(State);
            if (!State) ActionClass.IsActive = State;
            ActionClass.ActionLength = Random.Range(StrafingLengthMin, StrafingLengthMax);
            ActionClass.ActionLengthTimer = 0;
            ActionClass.CooldownLengthTimer = 0;
        }
    }
}