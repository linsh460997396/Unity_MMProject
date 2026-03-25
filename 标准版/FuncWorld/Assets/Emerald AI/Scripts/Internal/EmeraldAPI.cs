using EmeraldAI.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace EmeraldAI
{
    /// <summary>
    /// An easy and convenient way to access all practical API for Emerald AI through a static script. There's a class for each category of API. The EmeraldSystem of the AI is required for each function.
    /// </summary>
    public static class EmeraldAPI
    {
        /// <summary>
        /// Contains all usable Detection related API.
        /// </summary>
        public class Detection
        {
            /// <summary>
            /// Assigns a new follow target for an AI to follow. This will turn the AI into a Pet or Companion AI depending on its Behavior Type.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            /// <param name="CopyFactionData">Copies the Target to Follow's Faction Data so it will react the same way the follower does to detected targets. This requires the Target to Follow to be another AI (if this condition is not met, this parameter will be ignored).</param>
            public static void SetTargetToFollow(EmeraldSystem EmeraldComponent, Transform Target, bool CopyFactionData = true)
            {
                EmeraldComponent.DetectionComponent.SetTargetToFollow(Target, CopyFactionData);
            }

            /// <summary>
            /// Initializes a summoned target.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem of the summoned AI.</param>
            /// <param name="Target"> The target the summoned AI will follow and protect.</param>
            public static void InitializeSummonTarget(EmeraldSystem EmeraldComponent, Transform Target)
            {
                EmeraldComponent.DetectionComponent.InitializeSummonTarget(Target, true);
            }

            /// <summary>
            /// Clears the AI's Target to Follow transform so it will be no longer following it. This will also stop the AI from being a Companion or Pet AI.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ClearTargetToFollow(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.DetectionComponent.ClearTargetToFollow();
            }

            /// <summary>
            /// Checks to see if the player is currently within the AI's detection radius by returning true or false (this can be true even if the player is an enemy).
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static bool CheckForPlayerDetection(EmeraldSystem EmeraldComponent)
            {
                return EmeraldComponent.CombatTarget != null && EmeraldComponent.CombatTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag) || EmeraldComponent.LookAtTarget != null && EmeraldComponent.LookAtTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag);
            }

            /// <summary>
            /// Clears all ignored targets from the static EmeraldDetection IgnoredTargetsList.
            /// </summary>
            public static void ClearAllIgnoredTargets()
            {
                EmeraldDetection.IgnoredTargetsList.Clear();
            }

            /// <summary>
            /// Adds the specified ignored target to the static EmeraldDetection IgnoredTargetsList.
            /// </summary>
            public static void SetIgnoredTarget(Transform TargetTransform)
            {
                if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
                {
                    EmeraldDetection.IgnoredTargetsList.Add(TargetTransform);
                }
            }

            /// <summary>
            /// Removes the specified ignored target from the static EmeraldDetection IgnoredTargetsList.
            /// </summary>
            public static void ClearIgnoredTarget(Transform TargetTransform)
            {
                if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
                {
                    Debug.Log("The TargetTransform did not exist in the EmeraldAISystem IgnoreTargetsList list.");
                    return;
                }

                EmeraldDetection.IgnoredTargetsList.Remove(TargetTransform);
            }
        }

        /// <summary>
        /// Contains all usable Behaviors related API.
        /// </summary>
        public class Behaviors
        {
            /// <summary>
            /// Changes the AI's current behavior to the one specified. By default, the internal BehaviorState is set back to its default "Non Combat" which will then allow the AI to update its current state based on its new behavior.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ChangeBehavior (EmeraldSystem EmeraldComponent, EmeraldBehaviors.BehaviorTypes BehaviorType)
            {
                EmeraldComponent.BehaviorsComponent.CurrentBehaviorType = BehaviorType;
                EmeraldComponent.BehaviorsComponent.BehaviorState = "Non Combat";
            }
        }

        /// <summary>
        /// Contains all usable Combat related API.
        /// </summary>
        public class Combat
        {
            /// <summary>
            /// Overrides the AI's current ability and attack distance so the ability can be used when called (given within distance).
            /// </summary>
            /// <param name="EmeraldComponent">The AI's EmeraldSystem who will use this API call.</param>
            /// <param name="AbilityObject">The Ability Object that will be used and override the AI's current one.</param>
            /// <param name="AttackDistance">The new distance needed to trigger the ability. If high enough, the AI will trigger it right from their current position.</param>
            /// <param name="AttackAnimationIndex">The AI's attack animation index (from an AI's Animation Profile's Attack List). If the attack animation index does not exist within the AI's Attack List, there will be an error.</param>
            public static void TriggerAbility(EmeraldSystem EmeraldComponent, EmeraldAbilityObject AbilityObject, int AttackDistance, int AttackAnimationIndex)
            {
                EmeraldComponent.CombatComponent.CurrentAnimationIndex = AttackAnimationIndex;
                EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility = AbilityObject;
                EmeraldComponent.CombatComponent.CancelAllCombatActions();
                EmeraldComponent.CombatComponent.AttackDistance = AttackDistance;
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = AttackDistance;
                EmeraldComponent.AnimationComponent.PlayAttackAnimation();
            }

            /// <summary>
            /// Instantly kills this AI.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void KillAI(EmeraldSystem EmeraldComponent)
            {
                if (!EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.HealthComponent.KillAI();
                }
            }

            /// <summary>
            /// Resets the AI so it can be reused (typically called after an AI has been killed). This is automatically called when an AI is killed and then re-enabled.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ResetAI(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.HealthComponent.InstantlyRefillAIHealth();
                EmeraldCombatManager.EnableComponents(EmeraldComponent);
                EmeraldComponent.AnimationComponent.IsDead = false;
                EmeraldCombatManager.DisableRagdoll(EmeraldComponent);
            }

            /// <summary>
            /// Returns the current distance between the AI and their combat target (Returns -1 if the Combat Target is null).
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static float GetDistanceFromTarget(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.CombatTarget != null)
                {
                    return EmeraldComponent.CombatComponent.DistanceFromTarget;
                }
                else
                {
                    Debug.Log("This AI's Combat Target is null");
                    return -1;
                }
            }

            /// <summary>
            /// Returns the AI's combat target.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static Transform GetCombatTarget(EmeraldSystem EmeraldComponent)
            {
                return EmeraldComponent.CombatTarget;
            }

            /// <summary>
            /// Assigns a specified combat target for your AI to attack within the AI's Detection Radius. Note: Targets outside of an AI's Detection Radius will be ignored. If you want no distance limitations, use OverrideCombatTarget(Transform Target).
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void SetCombatTarget(EmeraldSystem EmeraldComponent, Transform Target)
            {
                if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive) return;

                if (Target != null)
                {
                    EmeraldComponent.DetectionComponent.SetDetectedTarget(Target);
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                    EmeraldComponent.m_NavMeshAgent.destination = Target.position;
                }
                else if (Target == null)
                {
                    Debug.Log("The SetCombatTarget paramter is null. Ensure that the target exists before calling this function.");
                }
            }

            /// <summary>
            /// Assigns a specified combat target for your AI to attack ignoring any distance limitations. If the target is not within attacking range, the AI will move to the target's position and attack based on its attack distance, even if it is outside of its detection radius.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void OverrideCombatTarget(EmeraldSystem EmeraldComponent, Transform Target)
            {
                if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive) return;

                if (Target != null)
                {
                    EmeraldComponent.DetectionComponent.SetDetectedTarget(Target);
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
                    EmeraldComponent.m_NavMeshAgent.destination = Target.position;
                    EmeraldComponent.BehaviorsComponent.InfititeChase = true;
                }
                else if (Target == null)
                {
                    Debug.Log("The OverrideCombatTarget paramter is null. Ensure that the target exists before calling this function.");
                }
            }

            /// <summary>
            /// Makes an AI flee from the specified target by switching their behavior to Coward. The AI will remain with the Coward until it is reset manually.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void FleeFromTarget(EmeraldSystem EmeraldComponent, Transform FleeTarget)
            {
                if (FleeTarget != null)
                {
                    EmeraldComponent.BehaviorsComponent.CurrentBehaviorType = EmeraldBehaviors.BehaviorTypes.Coward;
                    EmeraldComponent.DetectionComponent.SetDetectedTarget(FleeTarget);
                }
                else if (FleeTarget == null)
                {
                    Debug.Log("The FleeTarget paramter is null. Ensure that the target exists before calling this function.");
                }
            }

            /// <summary>
            /// Searches for a new target within the AI's Detection Radius closest to the AI.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void SearchForClosestTarget(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Closest);
                EmeraldComponent.DetectionComponent.SetDetectedTarget(EmeraldComponent.CombatTarget);
            }

            /// <summary>
            /// Searches for a new random target within the AI's Detection Radius.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void SearchForRandomTarget(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Random);
            }

            /// <summary>
            /// Returns the transform that last attacked the AI.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static Transform GetLastAttacker(EmeraldSystem EmeraldComponent)
            {
                return EmeraldComponent.CombatComponent.LastAttacker;
            }
        }

        /// <summary>
        /// Contains all usable Health related API.
        /// </summary>
        public class Health
        {
            /// <summary>
            /// Instantly heals this AI back to full health.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void InstantlyRefillAIHealth(EmeraldSystem EmeraldComponent)
            {
                if (!EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.HealthComponent.InstantlyRefillAIHealth();
                }
            }

            /// <summary>
            /// Updates the AI's Current and Max Health.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void UpdateHealth(EmeraldSystem EmeraldComponent, int MaxHealth, int CurrentHealth)
            {
                if (!EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.HealthComponent.UpdateHealth(MaxHealth, CurrentHealth);
                }
            }
        }

        /// <summary>
        /// Contains all usable Faction related API.
        /// </summary>
        public class Faction
        {
            /// <summary>
            /// Changes the relation of the given faction. Note: The faction must be available in the AI's faction list.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            /// <param name="Faction"> The name of the faction to change.</param>
            /// <param name="FactionLevel">The level to set the faction to typed as a string. The options are Enemy, Neutral, or Friendly</param>
            public static void SetFactionLevel(EmeraldSystem EmeraldComponent, string Faction, RelationTypes RelationType)
            {
                EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

                for (int i = 0; i < EmeraldComponent.DetectionComponent.FactionRelationsList.Count; i++)
                {
                    if (EmeraldComponent.DetectionComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                    {
                        EmeraldComponent.DetectionComponent.FactionRelationsList[i].RelationType = RelationType;
                    }
                    else
                    {
                        Debug.Log("The faction '" + Faction + "' does not exist in the AI's Faction Relations list. Please add it using the Faction Settings Foldout through the Emerald Detection editor of this AI.");
                    }
                }
            }

            /// <summary>
            /// Adds the Faction and Faction Relation to the AI's Faction Relations List. Note: The faction must exist within the Emerald Faction Manager's Faction List.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            /// <param name="Faction"> The name of the faction to change.</param>
            /// <param name="FactionLevel">The level to set the faction to typed as a string. The options are Enemy, Neutral, or Friendly</param>
            public static void AddFactionRelation(EmeraldSystem EmeraldComponent, string Faction, RelationTypes RelationType)
            {
                EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

                if (!EmeraldDetection.FactionData.FactionNameList.Contains(Faction))
                {
                    Debug.Log("The faction '" + Faction + "' does not exist in the Faction Manager. Please add it using the Emerald Faction Manager.");
                    return;
                }

                for (int i = 0; i < EmeraldComponent.DetectionComponent.FactionRelationsList.Count; i++)
                {
                    if (EmeraldComponent.DetectionComponent.FactionRelationsList[i].FactionIndex == FactionData.FactionNameList.IndexOf(Faction))
                    {
                        Debug.Log("This AI already contains the faction '" + Faction + "'. If you would like to modify an AI's existing faction, please use SetFactionLevel(string Faction, RelationTypes RelationType) instead.");
                        return;
                    }
                }

                EmeraldComponent.DetectionComponent.FactionRelationsList.Add(new FactionClass(FactionData.FactionNameList.IndexOf(Faction), (int)RelationType));
            }

            /// <summary>
            /// Gets the relation of the passed target and this AI in the form of a string (Enemy, Neutral, or Friendly). If a faction cannot be found, or if it is not a valid target, you will receive a value of Invalid Target.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static string GetTargetFactionRelation(EmeraldSystem EmeraldComponent, Transform Target)
            {
                IFaction m_IFaction = Target.GetComponent<IFaction>();

                if (m_IFaction != null)
                {
                    int ReceivedFaction = m_IFaction.GetFaction();
                    if (EmeraldComponent.DetectionComponent.AIFactionsList.Contains(ReceivedFaction))
                    {
                        var Faction = (RelationTypes)EmeraldComponent.DetectionComponent.FactionRelations[EmeraldComponent.DetectionComponent.AIFactionsList.IndexOf(ReceivedFaction)];
                        return Faction.ToString();
                    }
                    else return "Invalid Target";
                }
                else return "Invalid Target";
            }

            /// <summary>
            /// Gets the faction name of the passed AI target. The AI's own transform can also be passed to get its own faction name.
            /// </summary>
            public static string GetTargetFactionName(Transform Target)
            {
                IFaction m_IFaction = Target.GetComponent<IFaction>();

                if (m_IFaction != null)
                {
                    int ReceivedFaction = m_IFaction.GetFaction();
                    return EmeraldDetection.FactionData.FactionNameList[ReceivedFaction];
                }
                else return "Invalid Target";
            }

            /// <summary>
            /// Changes the AI's faction (Note: The FactionName must exists within the Faction Manager's Current Faction list).
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ChangeFaction(EmeraldSystem EmeraldComponent, string FactionName)
            {
                EmeraldFactionData FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

                if (FactionData.FactionNameList.Contains(FactionName))
                {
                    EmeraldComponent.DetectionComponent.CurrentFaction = FactionData.FactionNameList.IndexOf(FactionName);
                }
                else
                {
                    Debug.Log("Faction not Found");
                }
            }

            /// <summary>
            /// Copies all faction data from the FactionDataToCopy and applies it to the EmeraldComponent. This is the AI Faction Relations List (within the Faction Settings foldout of the Detection Component).
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call and receive the FactionDataToCopy AI's faction data.</param>
            public static void CopyFactionData(EmeraldSystem EmeraldComponent, EmeraldSystem FactionDataToCopy)
            {
                EmeraldComponent.DetectionComponent.CurrentFaction = FactionDataToCopy.DetectionComponent.CurrentFaction;
                EmeraldComponent.DetectionComponent.AIFactionsList = FactionDataToCopy.DetectionComponent.AIFactionsList;
                EmeraldComponent.DetectionComponent.FactionRelations = FactionDataToCopy.DetectionComponent.FactionRelations;
                EmeraldComponent.DetectionComponent.FactionRelationsList = FactionDataToCopy.DetectionComponent.FactionRelationsList;
            }
        }

        /// <summary>
        /// Contains all usable Movement related API.
        /// </summary>
        public class Movement
        {
            /// <summary>
            /// Changes the AI's Wander Type. If the Dynamic Wander Type is used, the AI's current position will be updated as the AI's new starting position, which is used by the Dynamic Wander position.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ChangeWanderType(EmeraldSystem EmeraldComponent, EmeraldMovement.WanderTypes NewWanderType)
            {
                if (NewWanderType == EmeraldMovement.WanderTypes.Dynamic) UpdateDynamicWanderPosition(EmeraldComponent);
                EmeraldComponent.MovementComponent.ChangeWanderType(NewWanderType);
            }

            /// <summary>
            /// Updates the AI's dynamic wandering position to the AI's current position.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void UpdateDynamicWanderPosition(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.StartingDestination = EmeraldComponent.transform.position;
            }

            /// <summary>
            /// Sets the AI's dynamic wandering position to the position of the Destination transform. 
            /// This is useful for functionality such as custom AI schedules. Note: This will automatically change
            /// your AI's Wander Type to Dynamic.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void SetDynamicWanderPosition(EmeraldSystem EmeraldComponent, Vector3 DestinationPosition)
            {
                EmeraldComponent.MovementComponent.ChangeWanderType(EmeraldMovement.WanderTypes.Dynamic);
                EmeraldComponent.MovementComponent.StartingDestination = DestinationPosition;
            }

            /// <summary>
            /// Updates the AI's starting position to the AI's current position.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void UpdateStartingPosition(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.StartingDestination = EmeraldComponent.transform.position;
            }

            /// <summary>
            /// Overrides the AI's Wander Type to Custom and sets the AI's destination to the Vector3 position. This is useful functionality like point and click movement, schedules, and more.
            /// Because this modifies the AI's Wander Type, the ChangeWanderType function will need to be called again to set the desired Wander Type, if something other than Custom is wanted.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void SetCustomDestination(EmeraldSystem EmeraldComponent, Vector3 DestinationPosition)
            {
                EmeraldComponent.MovementComponent.ResetWanderSettings();
                if (EmeraldComponent.MovementComponent.WanderType != EmeraldMovement.WanderTypes.Custom) ChangeWanderType(EmeraldComponent, EmeraldMovement.WanderTypes.Custom);
                EmeraldComponent.m_NavMeshAgent.SetDestination(DestinationPosition);
            }

            /// <summary>
            /// Sets the AI's destination to the Vector3 position.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void SetDestination(EmeraldSystem EmeraldComponent, Vector3 DestinationPosition)
            {
                EmeraldComponent.m_NavMeshAgent.SetDestination(DestinationPosition);
                EmeraldComponent.MovementComponent.ResetWanderSettings();
            }

            /// <summary>
            /// Generates a new position to move to within the specified radius based on the AI's current position.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void GenerateRandomDestination(EmeraldSystem EmeraldComponent, int Radius)
            {
                Vector3 NewDestination = EmeraldComponent.transform.position + new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * Radius;
                RaycastHit HitDown;
                if (Physics.Raycast(new Vector3(NewDestination.x, NewDestination.y + 5, NewDestination.z), -EmeraldComponent.transform.up, out HitDown, 10, EmeraldComponent.MovementComponent.DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
                {
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(NewDestination, out hit, 4, EmeraldComponent.m_NavMeshAgent.areaMask))
                    {
                        EmeraldComponent.m_NavMeshAgent.SetDestination(NewDestination);
                    }
                }
            }

            /// <summary>
            /// Adds a waypoint to an AI's Waypoint List.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void AddWaypoint(EmeraldSystem EmeraldComponent, Transform Waypoint)
            {
                EmeraldComponent.MovementComponent.WaypointsList.Add(Waypoint.position);
            }

            /// <summary>
            /// Removes a waypoint from the AI's Waypoint List according to the specified index.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void RemoveWaypoint(EmeraldSystem EmeraldComponent, int WaypointIndex)
            {
                EmeraldComponent.MovementComponent.WaypointsList.RemoveAt(WaypointIndex);
            }

            /// <summary>
            /// Clears all of an AI's current waypoints. Note: When an AI's waypoints are cleared, it will be set to the Stationary wander type to avoid an error. 
            /// If you want the AI to follow newly created waypoints, you will need to set its Wander Type back to Waypoint with the ChangeWanderType function (located at EmeraldAPI.Movement.ChangeWanderType).
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ClearAllWaypoints(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
                EmeraldComponent.MovementComponent.WaypointsList.Clear();
            }

            /// <summary>
            /// Stops an AI from moving when out of combat. This is useful for functionality like dialogue.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void StopMovement(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.isStopped = true;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
            }

            /// <summary>
            /// Resumes an AI's movement after using the StopMovement function.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ResumeMovement(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.DefaultMovement();
                EmeraldComponent.m_NavMeshAgent.isStopped = false;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
            }

            /// <summary>
            /// Stops an AI with a follow target from following. This will only work if an AI has a Current Follow Target.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void StopFollowing(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.TargetToFollow == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.gameObject.name + "' does not have a Current Follow Target. Please have one before calling this function.");
                    return;
                }

                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.isStopped = true;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
            }

            /// <summary>
            /// Allows an AI with a follow target to resume following its follower. This will only work if an AI has a Current Follow Target.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ResumeFollowing(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.TargetToFollow == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.gameObject.name + "' does not have a Current Follow Target. Please have one before calling this function.");
                    return;
                }

                EmeraldComponent.m_NavMeshAgent.isStopped = false;
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
            }

            /// <summary>
            /// Allows a Companion AI (an AI with Follow Target) to guard the assigned position.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void StartCompanionGuardPosition(EmeraldSystem EmeraldComponent, Vector3 PositionToGuard)
            {
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
                EmeraldComponent.m_NavMeshAgent.SetDestination(PositionToGuard);
            }

            /// <summary>
            /// Stops a Companion AI (an AI with Follow Target) from guarding and returns it to their current follower.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void StopCompanionGuardPosition(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
            }

            /// <summary>
            /// Rotates the AI towards the specified target position using the AI's turning animations. The angle in which the AI will stop rotating is based off of an AI's Turning Angle set within the Emerald Movement Component editor. 
            /// The AI will be unable to move during the duration of the turning process. If an AI is wandering, it is recommended that StopMovement is called first to stop an AI from wandering as an AI can still wander after the rotating has finished.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            /// <param name="TargetPosition">The position the AI will rotate towards. This can be an object, a player, another AI, a custom position, etc.</param>
            public static void RotateTowardsPosition(EmeraldSystem EmeraldComponent, Vector3 TargetPosition)
            {
                EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(RotateTowardsPositionInternal(EmeraldComponent, TargetPosition));
            }

            static IEnumerator RotateTowardsPositionInternal(EmeraldSystem EmeraldComponent, Vector3 TargetPosition)
            {
                if (EmeraldComponent.MovementComponent.RotateTowardsTarget) yield break; //Don'transform allow the AI to rotate towards the TargetPosition if it's already doing so

                EmeraldComponent.MovementComponent.RotateTowardsTarget = true; //Stops certain functionality while in this state
                EmeraldComponent.MovementComponent.LockTurning = false; //This is used for preventing an AI from getting stuck between two turn animations
                EmeraldComponent.m_NavMeshAgent.isStopped = true; //Stop the AI's NavMesh
                yield return new WaitForSeconds(0.1f); //Add a delay so the changes above aren'transform missed

                while (!EmeraldComponent.MovementComponent.LockTurning)
                {
                    Vector3 Direction = new Vector3(TargetPosition.x, 0, TargetPosition.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                    EmeraldComponent.MovementComponent.UpdateRotations(Direction);
                    EmeraldComponent.AnimationComponent.CalculateTurnAnimations(true);
                    yield return null;
                }

                EmeraldComponent.MovementComponent.RotateTowardsTarget = false;
                EmeraldComponent.m_NavMeshAgent.isStopped = false;
            }
        }

        /// <summary>
        /// Contains all usable Animation related API.
        /// </summary>
        public class Animation
        {
            /// <summary>
            /// Plays an emote animation according to the EmoteAnimationID parameter (from the AI's Emote Animation List). Note: This function will only work if
            /// an AI is not in active combat mode.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayEmoteAnimation(EmeraldSystem EmeraldComponent, int EmoteAnimationID)
            {
                EmeraldComponent.AnimationComponent.PlayEmoteAnimation(EmoteAnimationID);
            }

            /// <summary>
            /// Loops an emote animation according to the EmoteAnimationID parameter (from the AI's Emote Animation List). 
            /// StopLoopEmoteAnimation must be called for the animation to stop playing. Note: This function will only work if an AI is not in active combat mode.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void LoopEmoteAnimation(EmeraldSystem EmeraldComponent, int EmoteAnimationID)
            {
                EmeraldComponent.AnimationComponent.LoopEmoteAnimation(EmoteAnimationID);
            }

            /// <summary>
            /// Stops an emote animation from looping using the EmoteAnimationID parameter (from the AI's Emote Animation List). Note: This function will only work if an AI is not in active combat mode.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void StopLoopEmoteAnimation(EmeraldSystem EmeraldComponent, int EmoteAnimationID)
            {
                EmeraldComponent.AnimationComponent.StopLoopEmoteAnimation(EmoteAnimationID);
            }

            /// <summary>
            /// Manually sets the AI's next Idle animation instead of being generated randomly. This is useful for functionality such as playing a particular idle animation
            /// at a certain location such as for an AI's schedule. Note: The animation numbers are from 1 to 6 and must exist in your AI's Idle Animation list. You must call 
            /// DisableOverrideIdleAnimation() to have idle animations randomly generate again and to disable this feature.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void OverrideIdleAnimation(EmeraldSystem EmeraldComponent, int IdleIndex)
            {
                EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = true;
                EmeraldComponent.AIAnimator.SetInteger("Idle Index", IdleIndex);
            }

            /// <summary>
            /// Disables the OverrideIdleAnimation feature.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void DisableOverrideIdleAnimation(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = false;
            }
        }

        /// <summary>
        /// Contains all usable Sound related API.
        /// </summary>
        public class Sound
        {
            /// <summary>
            /// Plays a sound clip according to the Clip parameter.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlaySoundClip(EmeraldSystem EmeraldComponent, AudioClip Clip)
            {
                EmeraldComponent.SoundComponent.PlaySoundClip(Clip);
            }

            /// <summary>
            /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayIdleSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayIdleSound();
            }

            /// <summary>
            /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayAttackSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayAttackSound();
            }

            /// <summary>
            /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayWarningSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayWarningSound();
            }

            /// <summary>
            /// Plays a random block sound based on your AI's Block Sounds list.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayBlockSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayBlockSound();
            }

            /// <summary>
            /// Plays a random injured sound based on your AI's Injured Sounds list.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayInjuredSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayInjuredSound();
            }

            /// <summary>
            /// Plays a random death sound based on your AI's Death Sounds list. Can also be called through Animation Events.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayDeathSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayDeathSound();
            }

            /// <summary>
            /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is walking. This should be setup through an Animation Event.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void WalkFootstepSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.WalkFootstepSound();
            }

            /// <summary>
            /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is running. This should be setup through an Animation Event.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void RunFootstepSound(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.RunFootstepSound();
            }

            /// <summary>
            /// Plays a random sound effect from the AI's General Sounds list.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlayRandomSoundEffect(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.SoundComponent.PlayRandomSoundEffect();
            }

            /// <summary>
            /// Plays a sound effect from the AI's General Sounds list using the Sound Effect ID as the parameter.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void PlaySoundEffect(EmeraldSystem EmeraldComponent, int SoundEffectID)
            {
                EmeraldComponent.SoundComponent.PlaySoundEffect(SoundEffectID);
            }
        }

        /// <summary>
        /// Contains all usable UI related API (Requires EmeraldUI component).
        /// </summary>
        public class UI
        {
            /// <summary>
            /// Updates the AI's Health Bar color.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void UpdateUIHealthBarColor(EmeraldSystem EmeraldComponent, Color NewColor)
            {
                CheckForUIComponent(EmeraldComponent); //Checks that the EmeraldUI component exists

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes)
                {
                    GameObject HealthBarChild = EmeraldComponent.UIComponent.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                    UnityEngine.UI.Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<UnityEngine.UI.Image>();
                    HealthBarRef.color = NewColor;
                    UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                    HealthBarBackgroundImageRef.color = EmeraldComponent.UIComponent.HealthBarBackgroundColor;
                }
            }

            /// <summary>
            /// Updates the AI's Health Bar Background color.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void UpdateUIHealthBarBackgroundColor(EmeraldSystem EmeraldComponent, Color NewColor)
            {
                CheckForUIComponent(EmeraldComponent); //Checks that the EmeraldUI component exists

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes)
                {
                    GameObject HealthBarChild = EmeraldComponent.UIComponent.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                    UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                    HealthBarBackgroundImageRef.color = NewColor;
                }
            }

            /// <summary>
            /// Updates the AI's Name color
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void UpdateUINameColor(EmeraldSystem EmeraldComponent, Color NewColor)
            {
                CheckForUIComponent(EmeraldComponent); //Checks that the EmeraldUI component exists

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes && EmeraldComponent.UIComponent.DisplayAIName == YesOrNo.Yes)
                {
                    EmeraldComponent.UIComponent.AINameUI.color = NewColor;
                }
            }

            /// <summary>
            /// Updates the AI's Name text
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void UpdateUINameText(EmeraldSystem EmeraldComponent, string NewName)
            {
                CheckForUIComponent(EmeraldComponent); //Checks that the EmeraldUI component exists

                if (EmeraldComponent.UIComponent.AutoCreateHealthBars == YesOrNo.Yes && EmeraldComponent.UIComponent.DisplayAIName == YesOrNo.Yes)
                {
                    EmeraldComponent.UIComponent.AINameUI.text = NewName;
                }
            }

            /// <summary>
            /// Checks that the EmeraldUI component exists.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            static void CheckForUIComponent (EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.UIComponent == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.name + "' AI does not have a EmeraldUI component. Please attach one to said AI before calling this function.");
                    return;
                }
            }
        }

        /// <summary>
        /// Contains all usable Items related API (Requires EmeraldItems component).
        /// </summary>
        public class Items
        {
            /// <summary>
            /// Enables an item from your AI's Item list using the Item ID.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void EnableItem(EmeraldSystem EmeraldComponent, int ItemID)
            {
                CheckForItemsComponent(EmeraldComponent); //Checks that the EmeraldItems component exists
                EmeraldComponent.ItemsComponent.EnableItem(ItemID);
            }

            /// <summary>
            /// Disables an item from your AI's Item list using the Item ID.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void DisableItem(EmeraldSystem EmeraldComponent, int ItemID)
            {
                CheckForItemsComponent(EmeraldComponent); //Checks that the EmeraldItems component exists
                EmeraldComponent.ItemsComponent.DisableItem(ItemID);
            }

            /// <summary>
            /// Disables all items from your AI's Item list.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void DisableAllItems(EmeraldSystem EmeraldComponent)
            {
                CheckForItemsComponent(EmeraldComponent); //Checks that the EmeraldItems component exists
                EmeraldComponent.ItemsComponent.DisableAllItems();
            }

            /// <summary>
            /// Reset any equipped items to their defaults.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            public static void ResetSettings(EmeraldSystem EmeraldComponent)
            {
                CheckForItemsComponent(EmeraldComponent); //Checks that the EmeraldItems component exists
                EmeraldComponent.ItemsComponent.ResetSettings();
            }

            /// <summary>
            /// Checks that the EmeraldItems component exists.
            /// </summary>
            /// <param name="EmeraldComponent"> The AI's EmeraldSystem who will use this API call.</param>
            static void CheckForItemsComponent(EmeraldSystem EmeraldComponent)
            {
                if (EmeraldComponent.ItemsComponent == null)
                {
                    Debug.LogError("The '" + EmeraldComponent.name + "' AI does not have a EmeraldItems component. Please attach one to said AI before calling this function.");
                    return;
                }
            }
        }

        /// <summary>
        /// Contains internal API that is used for specific internal mechanics. It is not recommended to be used directly.
        /// </summary>
        public class Internal
        {
            public static void GenerateRandomPositionWithinRadius(EmeraldSystem EmeraldComponent)
            {
                //Return if the CombatTarget is null (which can happen if the CombatTarget gets cleared rght before this function gets called).
                if (EmeraldComponent.CombatTarget == null) return;

                EmeraldComponent.CombatComponent.CancelAllCombatActions();
                EmeraldComponent.MovementComponent.StopBackingUp();
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;
                int Radius = Random.Range(2, 5);

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
                EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(Moving(EmeraldComponent));
            }

            static IEnumerator Moving(EmeraldSystem EmeraldComponent)
            {
                EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
                yield return new WaitForSeconds(0.25f);
                float CanSeeTargetTimer = 0;

                while (EmeraldComponent.m_NavMeshAgent.enabled && !EmeraldComponent.AnimationComponent.IsDead && EmeraldComponent.m_NavMeshAgent.remainingDistance >= 0.75f && CanSeeTargetTimer < 1)
                {
                    if (EmeraldComponent.DetectionComponent.ObstructionType == EmeraldDetection.ObstructedTypes.None)
                    {
                        CanSeeTargetTimer += Time.deltaTime;
                    }

                    EmeraldComponent.m_NavMeshAgent.stoppingDistance = 0.5f;
                    Vector3 Direction = new Vector3(EmeraldComponent.m_NavMeshAgent.steeringTarget.x, 0, EmeraldComponent.m_NavMeshAgent.steeringTarget.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                    EmeraldComponent.MovementComponent.UpdateRotations(Direction);
                    yield return null;
                }

                if (!EmeraldComponent.m_NavMeshAgent.enabled || EmeraldComponent.AnimationComponent.IsDead)
                {
                    EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);

                float t = 0;

                while (t < 1f && EmeraldComponent.CombatTarget != null)
                {
                    t += Time.deltaTime;

                    Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
                    EmeraldComponent.MovementComponent.UpdateRotations(Direction);

                    yield return null;
                }

                EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
            }
        }
    }
}