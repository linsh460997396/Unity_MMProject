using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EmeraldAI
{
    /// <summary>
    /// A modular action giving AI the ability to generate a random waypoint around their targets to move to during combat.
    /// </summary>
    [CreateAssetMenu(fileName = "Random Movement Action", menuName = "Emerald AI/Combat Action/Random Movement Action")]
    public class RandomMovementAction : EmeraldAction
    {
        [Range(4, 20)] [Tooltip("The radius a random movement waypoint can be generated in.")] public int MinWaypointRadius = 4;
        [Range(4, 20)] [Tooltip("The radius a random movement waypoint can be generated in.")] public int MaxWaypointRadius = 7;
        [Range(0f, 6)] [Tooltip("The length (in seconds) the AI will wait before resuming attacking.")] public float MinWaitSeconds = 0.5f;
        [Range(0f, 6)] [Tooltip("The length (in seconds) the AI will wait before resuming attacking.")] public float MaxWaitSeconds = 2f;
        [Range(0, 1)] [Tooltip("The odds for a random movement to happen, given the needed conditions are met.")] public float OddsToMove = 0.5f;

        /// <summary>
        /// Continiously updates the EmeraldAction. This acts like an Update function that can run within this action using the information from the passed EmeraldComponent and its ActionClass.
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            RandomMovementActionUpdate(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// Updates the strafe action using the UpdateAction.
        /// </summary>
        void RandomMovementActionUpdate(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!ActionClass.IsActive)
            {
                if (CanExecute(EmeraldComponent, ActionClass))
                {
                    GenerateRandomPositionWithinRadius(EmeraldComponent, ActionClass);
                }
            }
            else
            {
                if (CanCancel(EmeraldComponent))
                {
                    if (ActionClass.ActionCoroutine != null)
                        EmeraldComponent.GetComponent<MonoBehaviour>().StopCoroutine(ActionClass.ActionCoroutine);

                    ActionClass.IsActive = false;
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                }
            }
        }

        void GenerateRandomPositionWithinRadius(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            float Roll = Random.Range(0f, 1f);
            if (Roll > OddsToMove)
            {
                ActionClass.CooldownLengthTimer = 0;
                return;
            }

            EmeraldComponent.CombatComponent.CancelAllCombatActions();
            ActionClass.IsActive = true;
            ActionClass.CooldownLengthTimer = 0;
            EmeraldComponent.MovementComponent.StopBackingUp();
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;
            int Radius = Random.Range(MinWaypointRadius, MaxWaypointRadius + 1);

            Vector3 Dir = (EmeraldComponent.CombatTarget.position - EmeraldComponent.transform.position).normalized;
            int OffsetAmount = Random.Range(1, 3);
            var DirectionOffset = Quaternion.Euler(0, OffsetAmount == 1 ? -50 : 50, 0) * Dir;
            Vector3 GeneratedDestination = EmeraldComponent.CombatTarget.position + (DirectionOffset * Radius);

            RaycastHit HitDown;
            if (Physics.Raycast(GeneratedDestination + Vector3.up * 2, -Vector3.up, out HitDown, 5f))
            {
                GeneratedDestination.y = HitDown.point.y;
            }

            EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;

            Coroutine MoveCoroutine = ActionClass.ActionCoroutine;

            if (MoveCoroutine != null)
                EmeraldComponent.GetComponent<MonoBehaviour>().StopCoroutine(MoveCoroutine);
            MoveCoroutine = EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(Moving(EmeraldComponent, ActionClass));

            ActionClass.ActionCoroutine = MoveCoroutine;
        }

        IEnumerator Moving(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
            yield return new WaitForSeconds(0.5f);

            while (!CanCancel(EmeraldComponent) && EmeraldComponent.m_NavMeshAgent.enabled && !EmeraldComponent.AnimationComponent.IsDead && EmeraldComponent.m_NavMeshAgent.remainingDistance >= 0.75f)
            {
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;
                Vector3 Direction = new Vector3(EmeraldComponent.m_NavMeshAgent.steeringTarget.x, 0, EmeraldComponent.m_NavMeshAgent.steeringTarget.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                EmeraldComponent.MovementComponent.UpdateRotations(Direction);
                yield return null;
            }

            if (!EmeraldComponent.m_NavMeshAgent.enabled || EmeraldComponent.AnimationComponent.IsDead)
            {
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                ActionClass.IsActive = false;
                yield break;
            }

            yield return new WaitForSeconds(0.5f);

            //Wait according for the randomly generated seconds amount and rotate towards the target while doing so.
            float WaitSeconds = Random.Range(MinWaitSeconds, MaxWaitSeconds);
            float t = 0;

            while (t < WaitSeconds && EmeraldComponent.CombatTarget != null)
            {
                t += Time.deltaTime;

                Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                EmeraldComponent.MovementComponent.UpdateRotations(Direction);

                yield return null;
            }

            ActionClass.IsActive = false;
            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
        }

        bool CanCancel(EmeraldSystem EmeraldComponent)
        {
            return (((int)ExitConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
        }

        /// <summary>
        /// Conditions required for the EmeraldAction to execute.
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return (Conditions && ActionClass.CooldownLengthTimer >= CooldownLength && EmeraldComponent.CombatComponent.DistanceFromTarget < 15 && !EmeraldComponent.CurrentTargetInfo.CurrentICombat.IsAttacking() && !EmeraldComponent.AnimationComponent.IsBlocking && !EmeraldComponent.AIAnimator.GetBool("Attack"));
        }
    }
}