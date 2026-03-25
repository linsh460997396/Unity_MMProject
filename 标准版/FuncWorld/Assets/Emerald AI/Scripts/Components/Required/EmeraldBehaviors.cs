using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    /// <summary>
    /// This script handles all of Emerald AI's behaviors and states. Most functions can be overridden to create custom behaviors or functionality.
    /// </summary>
    [System.Serializable]
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/behaviors-component")]
    public class EmeraldBehaviors : MonoBehaviour
    {
        #region Behavior Variables
        protected EmeraldSystem EmeraldComponent;
        public enum BehaviorTypes { Passive = 0, Coward = 1, Aggressive = 2};
        public BehaviorTypes CurrentBehaviorType = BehaviorTypes.Aggressive;

        public Transform TargetToFollow;
        public int CautiousSeconds = 0;
        public bool InfititeChase;
        public int ChaseSeconds = 5;
        public int FleeSeconds = 5;
        public bool RequireObstruction;
        public int PercentToFlee = 20;
        public float UpdateFleePositionSeconds = 1.5f;
        public int MaxDistanceFromStartingArea = 30;
        public float FollowingStoppingDistance = 2f;
        public bool IsAiming;

        public delegate void StartFleeHandler();
        public event StartFleeHandler OnFlee;

        public YesOrNo FleeOnLowHealth = YesOrNo.No;
        public YesOrNo StayNearStartingArea = YesOrNo.No;

        /// <summary>
        /// A timer used for tracking how long an AI is in the cautious state.
        /// </summary>
        protected float CautiousTimer;
        /// <summary>
        /// A timer used for controlling how often flee positions are updated.
        /// </summary>
        protected float UpdateFleePositionTimer;
        /// <summary>
        /// A timer used for tracking how long a target is outside of an AI's detection radius.
        /// </summary>
        protected float GiveUpTimer;
        /// <summary>
        /// A timer used for tracking the cooldown length of an AI's attacks.
        /// </summary>
        protected float AttackTimer;
        /// <summary>
        /// A string used for tracking an AI's current behavior state. This is a string so it can be customized as needed, given that a behvaior has multiple stages or states.
        /// </summary>
        public string BehaviorState = "Non Combat";
        #endregion

        #region Editor Variables
        [HideInInspector] public bool HideSettingsFoldout;
        [HideInInspector] public bool BehaviorSettingsFoldout;
        [HideInInspector] public bool CustomSettingsFoldout;
        #endregion

        public virtual void Start()
        {
            InitailizeBehaviors();
        }

        /// <summary>
        /// Initialize the Behavior Component.
        /// </summary>
        public virtual void InitailizeBehaviors ()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();

            ResetState();

            if (TargetToFollow != null)
            {
                if (!TargetToFollow.gameObject.activeSelf)
                {
                    Debug.LogError("The '" + gameObject.name + "' AI's Follower Target '" + TargetToFollow.name + "' is disabled so it has been removed as the AI's follower. You can enable said gameobject or use the SetFollowerTarget(Transform) API to assign a follower through code if needed.");
                    TargetToFollow = null;
                }
                else
                {
                    StartCoroutine(SetFollowerTargetInternal()); //Use a slight delay to ensure all other components have been initialized before assigning the AI's Target to Follow.
                }
            }

            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += OnDetectTarget;
            EmeraldComponent.DetectionComponent.OnNullTarget += ResetState;
            EmeraldComponent.CombatComponent.OnKilledTarget += OnKilledTarget;
            EmeraldComponent.HealthComponent.OnTakeDamage += OnTakeDamage;

            if (CurrentBehaviorType == BehaviorTypes.Passive)
            {
                if (!gameObject.CompareTag("Untagged"))
                {
                    gameObject.tag = "Untagged";
                }
                if (gameObject.layer != 0)
                {
                    gameObject.layer = 0;
                }
            }
        }

        /// <summary>
        /// Use a slight delay to ensure all other components have been initialized before assigning the AI's Target to Follow.
        /// </summary>
        IEnumerator SetFollowerTargetInternal ()
        {
            yield return new WaitForSeconds(0.1f);
            EmeraldComponent.DetectionComponent.SetTargetToFollow(TargetToFollow);
        }

        /// <summary>
        /// Continiously updates the BehaviorObject. This acts like an Update function that can run within this behavior using the information from the passed EmeraldComponent and its 
        /// </summary>
        public virtual void BehaviorUpdate()
        {
            if (EmeraldComponent.AnimationComponent.IsDead)
                return;

            switch (BehaviorState)
            {
                case "Non Combat":
                    WanderBehavior();
                    break;
                case "Cautious":
                    CautiousBehavior();
                    break;
                case "Aggressive":
                    AggressiveBehavior();
                    break;
                case "Flee":
                    CowardBehavior();
                    break;
            }

            //Update the DetectTargetTracker virtual method (which tracks when targets are within the detection radius and clears them when needed)
            //This can be overridden if these mechanics need to be customized.
            DetectTargetTracker();
        }

        /// <summary>
        /// Play a warning animation and look at the current target. If the CautiousSeconds are met, change the state to be aggressive.
        /// </summary>
        public virtual void CautiousBehavior()
        {
            if (EmeraldComponent.CombatTarget && EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.MovementComponent.AIAgentActive && CurrentBehaviorType != BehaviorTypes.Passive)
            {
                //Bypass the cautious timer if the AI has a follower target.
                if (CurrentBehaviorType == BehaviorTypes.Aggressive && EmeraldComponent.TargetToFollow) BehaviorState = "Aggressive";

                CautiousTimer += Time.deltaTime;

                if (CautiousTimer >= CautiousSeconds)
                {
                    if (CurrentBehaviorType == BehaviorTypes.Aggressive)
                    {
                        BehaviorState = "Aggressive";
                    }   
                    else if (CurrentBehaviorType == BehaviorTypes.Coward)
                    {
                        OnFlee?.Invoke(); //Invoke the OnFlee delegate.
                        BehaviorState = "Flee";
                    }
                        
                    CautiousTimer = 0;
                }

                if (CautiousTimer > 2)
                {
                    EmeraldComponent.AnimationComponent.PlayWarningAnimation();
                }
            }
        }

        /// <summary>
        /// Actively chase and attack the current target.
        /// </summary>
        public virtual void AggressiveBehavior()
        {
            if (EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.MovementComponent.AIAgentActive && EmeraldComponent.CombatTarget)
            {
                //Only attempt to chase and attack the current target if a path exists to them.
                bool CanReachTarget = EmeraldComponent.MovementComponent.CanReachTarget;

                //Use Emerald AI's built-in Combat Movement (which simply sets the AI's destination equal to the current target's position). A custom function can be used for added functionality, if desired.
                //This will also backup the AI if they get too close to their target.
                if (!EmeraldComponent.MovementComponent.DefaultMovementPaused)
                {
                    EmeraldComponent.MovementComponent.CombatMovement();
                }
                else if (!EmeraldComponent.MovementComponent.DefaultMovementPaused && CanReachTarget)
                {
                    EmeraldComponent.m_NavMeshAgent.SetDestination(transform.position + transform.forward * 2);
                }

                //Updates an AI's list of Combat Actions, but only while in the Aggressive state (Combat State).
                //Don'transform allow Combat Actions to update for 2 seconds after being enabled or spawned to avoid them triggering before the AI is fully initialized.
                if (Time.time - EmeraldComponent.TimeSinceEnabled > 2f) EmeraldComponent.CombatComponent.UpdateActions();

                Attack(); //Continuously check to see if the conditions are right to trigger an attack, given the AI is in the Aggressive State. 

                //If FleeOnLowHealth is enabled, and the AI's health reaches the threshold, set the AI's BehaviorState to Flee.
                if (FleeOnLowHealth == YesOrNo.Yes && ((float)EmeraldComponent.HealthComponent.CurrentHealth / (float)EmeraldComponent.HealthComponent.StartingHealth) < (PercentToFlee * 0.01f))
                {
                    EmeraldComponent.AnimationComponent.ResetTriggers(0);
                    OnFlee?.Invoke(); //Invoke the OnFlee delegate.
                    BehaviorState = "Flee";
                }
            }
        }

        /// <summary>
        /// Use Emerald AI's built-in Flee Movement (which simply generates a destination opposite to the current target's position). This is updated based 
        /// on the UpdateFleePositionSeconds or if the AI gets close the generated waypoint. A custom function can be used for added functionality, if desired.
        /// </summary>
        public virtual void CowardBehavior()
        {
            if (!EmeraldComponent.MovementComponent.DefaultMovementPaused)
            {
                UpdateFleePositionTimer += Time.deltaTime;
                if (UpdateFleePositionTimer > UpdateFleePositionSeconds || EmeraldComponent.m_NavMeshAgent.remainingDistance <= EmeraldComponent.MovementComponent.StoppingDistance)
                {
                    EmeraldComponent.MovementComponent.FleeMovement();
                    UpdateFleePositionTimer = 0;
                }
            }
        }

        /// <summary>
        /// Use Emerald AI's built-in Wandering when not in combat (based off of the user set WanderType within the Emerald AI Movement Editor)
        /// </summary>
        public virtual void WanderBehavior()
        {
            if (EmeraldComponent.MovementComponent.AIAgentActive && !EmeraldComponent.CombatComponent.CombatState && !EmeraldComponent.CombatComponent.DeathDelayActive)
            {
                if (!EmeraldComponent.TargetToFollow)
                {
                    EmeraldComponent.MovementComponent.Wander();
                }
                else
                {
                    EmeraldComponent.MovementComponent.FollowCompanionTarget(FollowingStoppingDistance);
                }
            }
        }

        /// <summary>
        /// Used for tracking when a target is outside of an AI's detection radius.
        /// </summary>
        public virtual void DetectTargetTracker()
        {
            if (!EmeraldComponent.CombatComponent.CombatState || EmeraldComponent.CombatComponent.DeathDelayActive || InfititeChase || EmeraldComponent.TargetToFollow)
                return;

            //Track how long a target is outside of the AI's detection radius. If the time is exceeded, give up on the target and set the AI to its defaul state.
            if (EmeraldComponent.CombatComponent.DistanceFromTarget > EmeraldComponent.DetectionComponent.DetectionRadius && !RequireObstruction || RequireObstruction && EmeraldComponent.DetectionComponent.TargetObstructed)
            {
                GiveUpTimer += Time.deltaTime;

                if (GiveUpTimer >= ChaseSeconds && CurrentBehaviorType == BehaviorTypes.Aggressive || GiveUpTimer >= FleeSeconds && CurrentBehaviorType == BehaviorTypes.Coward || BehaviorState == "Cautious")
                {
                    CancelCombat(); //Stops the AI from fighting and chasing, or fleeing from, its current target.
                }
            }
            else
            {
                GiveUpTimer = 0;
            }

            //Used for tracking when an AI's distance from its starting position is exceeded.
            if (CurrentBehaviorType == BehaviorTypes.Aggressive && StayNearStartingArea == YesOrNo.Yes && Vector3.Distance(EmeraldComponent.MovementComponent.StartingDestination, transform.position) > MaxDistanceFromStartingArea)
            {
                EmeraldComponent.MovementComponent.EnableReturnToStart(); //Returns the AI to its starting area
                CancelCombat(); //Stops the AI from fighting and chasing its current target
            }
        }

        /// <summary>
        /// Continuously check to see if the conditions are right to trigger an attack, given the AI is in the Aggressive State. This is a priority state.
        /// </summary>
        public virtual void Attack()
        {
            var EnterConditions = EmeraldComponent.AnimationComponent.IsIdling || EmeraldComponent.AnimationComponent.IsMoving;
            var CooldownConditions = EmeraldComponent.AnimationComponent.IsIdling || EmeraldComponent.AnimationComponent.IsMoving || EmeraldComponent.AnimationComponent.IsBackingUp ||
                EmeraldComponent.AnimationComponent.IsTurningLeft || EmeraldComponent.AnimationComponent.IsTurningRight || EmeraldComponent.AnimationComponent.IsGettingHit;

            if (CooldownConditions) AttackTimer += Time.deltaTime;

            if (EmeraldCombatManager.AllowedToAttack(EmeraldComponent) && EnterConditions && !IsAiming && AttackTimer >= EmeraldComponent.CombatComponent.CurrentAttackCooldown)
            {
                EmeraldCombatManager.CheckAttackHeight(EmeraldComponent);

                if (!EmeraldComponent.CombatComponent.TargetOutOfHeightRange)
                {
                    EmeraldComponent.AnimationComponent.IsMoving = false;
                    EmeraldComponent.CombatComponent.AdjustCooldowns();
                    EmeraldComponent.CombatComponent.AttackPosition = EmeraldComponent.CombatTarget.position - transform.position;
                    EmeraldComponent.CombatComponent.AttackPosition.y = 0;
                    EmeraldComponent.AnimationComponent.PlayAttackAnimation();
                    AttackTimer = 0;
                }
                else
                {
                    AttackTimer = 0;
                }
            }

            //Cancel the attack if it's triggered and the target is out of range
            if (AttackTimer >= EmeraldComponent.CombatComponent.CurrentAttackCooldown)
            {
                if (EmeraldComponent.m_NavMeshAgent.remainingDistance > EmeraldComponent.m_NavMeshAgent.stoppingDistance && EmeraldComponent.AIAnimator.GetBool("Attack"))
                {
                    EmeraldComponent.AIAnimator.ResetTrigger("Attack");
                    AttackTimer = 0;
                }
            }
        }

        /// <summary>
        /// Stops the AI from fighting and chasing, or fleeing from, its current target.
        /// </summary>
        public virtual void CancelCombat()
        {
            EmeraldComponent.CombatComponent.ClearTarget();
            EmeraldComponent.CombatComponent.DeathDelayActive = true;
            EmeraldComponent.m_NavMeshAgent.SetDestination(transform.position + EmeraldComponent.transform.forward * (EmeraldComponent.MovementComponent.StoppingDistance * 1.5f));
            EmeraldComponent.AnimationComponent.ResetTriggers(0);
            ResetState();
        }

        /// <summary>
        /// Reset the settings back to their default values.
        /// </summary>
        public virtual void ResetState ()
        {
            BehaviorState = "Non Combat";
            CautiousTimer = 0;
            UpdateFleePositionTimer = 0;
            GiveUpTimer = 0;
            AttackTimer = 0;
        }

        /// <summary>
        /// When killing a target, return the BehaviorState back to Non Combat. If another target is found, it will be updated.
        /// </summary>
        public virtual void OnKilledTarget()
        {
            BehaviorState = "Non Combat";
            GiveUpTimer = 0;
            EmeraldComponent.AnimationComponent.WarningAnimationTriggered = false;
        }

        /// <summary>
        /// When detecting a target, update the AI's current destination to be equal to their current position until the combat movement code can take over.
        /// </summary>
        public virtual void OnDetectTarget()
        {
            BehaviorState = "Cautious";
            if (isActiveAndEnabled) EmeraldComponent.m_NavMeshAgent.SetDestination(transform.position);
        }

        /// <summary>
        /// If an AI takes damage before its Cautious State has finished, change the BehaviorState so it can handle its attack according to its behavior.
        /// </summary>
        public virtual void OnTakeDamage()
        {
            if (BehaviorState == "Cautious" && CurrentBehaviorType == BehaviorTypes.Aggressive) BehaviorState = "Aggressive";
            else if (BehaviorState == "Cautious" && CurrentBehaviorType == BehaviorTypes.Coward) BehaviorState = "Flee";
        }
    }
}