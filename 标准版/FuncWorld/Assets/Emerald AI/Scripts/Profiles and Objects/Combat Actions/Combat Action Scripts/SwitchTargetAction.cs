using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// A modular action giving AI the ability to switch targets.
    /// </summary>
    [CreateAssetMenu(fileName = "Switch Target Action", menuName = "Emerald AI/Combat Action/Switch Target Action")]
    public class SwitchTargetAction : EmeraldAction
    {
        [Tooltip("The method that will be used for switching targets.")] public PickTargetTypes PickTargetType = PickTargetTypes.Random;

        /// <summary>
        /// Continiously updates the EmeraldAction. This acts like an Update function that can run within this action using the information from the passed EmeraldComponent and its ActionClass.
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!CanExecute(EmeraldComponent, ActionClass))
                return;

            SwitchTarget(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// Conditions required for the EmeraldAction to execute.
        /// </summary>
        bool CanExecute (EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return ActionClass.CooldownLengthTimer >= CooldownLength && Conditions && EmeraldComponent.transform.localScale != Vector3.one * 0.003f;

        }

        /// <summary>
        /// Executes the Action becuse all conditions from CanExecute have been met.
        /// </summary>
        void SwitchTarget (EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (EmeraldComponent.AnimationComponent.IsAttacking || EmeraldComponent.AIAnimator.GetBool("Attack") || EmeraldComponent.DetectionComponent.LineOfSightTargets.Count <= 1)
                return;

            EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetType);
            ActionClass.CooldownLengthTimer = 0;
        }
    }
}