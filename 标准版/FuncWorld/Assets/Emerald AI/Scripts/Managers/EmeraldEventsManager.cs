using UnityEngine;
using EmeraldAI.Utility;
using static UnityEngine.GraphicsBuffer;

namespace EmeraldAI
{
    public class EmeraldEventsManager : MonoBehaviour
    {
        EmeraldSystem EmeraldComponent;
        EmeraldMovement MovementComponent;
        EmeraldUI EmeraldUI;

        void Awake()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            MovementComponent = GetComponent<EmeraldMovement>();
            EmeraldUI = GetComponent<EmeraldUI>();
        }

        /// <summary>
        /// Plays a sound clip according to the Clip parameter.
        /// </summary>
        public void PlaySoundClip(AudioClip Clip)
        {
            EmeraldComponent.SoundComponent.PlaySoundClip(Clip);
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayIdleSound()
        {
            EmeraldComponent.SoundComponent.PlayIdleSound();
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayAttackSound()
        {
            EmeraldComponent.SoundComponent.PlayAttackSound();
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayWarningSound()
        {
            EmeraldComponent.SoundComponent.PlayWarningSound();
        }

        /// <summary>
        /// Plays a random block sound based on your AI's Block Sounds list.
        /// </summary>
        public void PlayBlockSound()
        {
            EmeraldComponent.SoundComponent.PlayBlockSound();
        }

        /// <summary>
        /// Plays a random injured sound based on your AI's Injured Sounds list.
        /// </summary>
        public void PlayInjuredSound()
        {
            EmeraldComponent.SoundComponent.PlayInjuredSound();
        }

        /// <summary>
        /// Plays a random death sound based on your AI's Death Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayDeathSound()
        {
            EmeraldComponent.SoundComponent.PlayDeathSound();
        }

        /// <summary>
        /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is walking. This should be setup through an Animation Event.
        /// </summary>
        public void WalkFootstepSound()
        {
            EmeraldComponent.SoundComponent.WalkFootstepSound();
        }

        /// <summary>
        /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is running. This should be setup through an Animation Event.
        /// </summary>
        public void RunFootstepSound()
        {
            EmeraldComponent.SoundComponent.RunFootstepSound();
        }

        /// <summary>
        /// Plays a random sound effect from the AI's General Sounds list.
        /// </summary>
        public void PlayRandomSoundEffect()
        {
            EmeraldComponent.SoundComponent.PlayRandomSoundEffect();
        }

        /// <summary>
        /// Plays a sound effect from the AI's General Sounds list using the Sound Effect ID as the parameter.
        /// </summary>
        public void PlaySoundEffect(int SoundEffectID)
        {
            EmeraldComponent.SoundComponent.PlaySoundEffect(SoundEffectID);
        }

        /// <summary>
        /// Instantly kills this AI.
        /// </summary>
        public void KillAI()
        {
            if (!EmeraldComponent.AnimationComponent.IsDead)
            {
                EmeraldComponent.GetComponent<IDamageable>().Damage(9999999);
            }
        }

        /// <summary>
        /// Manually sets the AI's next Idle animation instead of being generated randomly. This is useful for functionality such as playing a particular idle animation
        /// at a certain location such as for an AI's schedule. Note: The animation numbers are from 1 to 6 and must exist in your AI's Idle Animation list. You must call 
        /// DisableOverrideIdleAnimation() to have idle animations randomly generate again and to disable this feature.
        /// </summary>
        public void OverrideIdleAnimation(int IdleIndex)
        {
            EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = true;
            EmeraldComponent.AIAnimator.SetInteger("Idle Index", IdleIndex);
        }

        /// <summary>
        /// Disables the OverrideIdleAnimation feature.
        /// </summary>
        public void DisableOverrideIdleAnimation()
        {
            EmeraldComponent.AnimationComponent.m_IdleAnimaionIndexOverride = false;
        }

        /// <summary>
        /// Changes the AI's Wander Type
        /// </summary>
        public void ChangeWanderType(EmeraldMovement.WanderTypes NewWanderType)
        {
            EmeraldComponent.MovementComponent.ChangeWanderType(NewWanderType);
        }

        /// <summary>
        /// Clears all ignored targets from the static EmeraldAISystem IgnoredTargetsList.
        /// </summary>
        public void ClearAllIgnoredTargets()
        {
            EmeraldDetection.IgnoredTargetsList.Clear();
        }

        /// <summary>
        /// Adds the specified ignored target to the static EmeraldAISystem IgnoredTargetsList.
        /// </summary>
        public void SetIgnoredTarget(Transform TargetTransform)
        {
            if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
            {
                EmeraldDetection.IgnoredTargetsList.Add(TargetTransform);
            }
        }

        /// <summary>
        /// Removes the specified ignored target from the static EmeraldAISystem IgnoredTargetsList.
        /// </summary>
        public void ClearIgnoredTarget(Transform TargetTransform)
        {
            if (!EmeraldDetection.IgnoredTargetsList.Contains(TargetTransform))
            {
                Debug.Log("The TargetTransform did not exist in the EmeraldAISystem IgnoreTargetsList list.");
                return;
            }
 
            EmeraldDetection.IgnoredTargetsList.Remove(TargetTransform);
        }

        /// <summary>
        /// Returns the current distance between the AI and their current target (Returns -1 if the Current Target is null).
        /// </summary>
        /// <returns></returns>
        public float GetDistanceFromTarget ()
        {
            if (EmeraldComponent.CombatTarget != null)
            {
                return EmeraldComponent.CombatComponent.DistanceFromTarget;
            }
            else
            {
                Debug.Log("This AI's Current Target is null");
                return -1;
            }
        }

        /// <summary>
        /// Returns the AI's current target.
        /// </summary>
        public Transform GetCombatTarget()
        {
            return EmeraldComponent.CombatTarget;
        }

        /// <summary>
        /// Assigns a specified combat target for your AI to attack within the AI's Detection Radius. Note: Targets outside of an AI's Detection Radius will be ignored. If you want no distance limitations, use OverrideCombatTarget(Transform Target).
        /// </summary>
        public void SetCombatTarget(Transform Target)
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
        /// Assigns a specified combat target for your AI to attack ignoring any distance limitations. If the target is not within attacking range, the AI will move to the target's position and attack based on its attack distance.
        /// </summary>
        public void OverrideCombatTarget(Transform Target)
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
        /// Makes an AI flee from the specified target by overiding their behavior.
        /// </summary>
        public void FleeFromTarget(Transform FleeTarget)
        {
            if (FleeTarget != null)
            {
                EmeraldComponent.BehaviorsComponent.CurrentBehaviorType = EmeraldBehaviors.BehaviorTypes.Coward;
                EmeraldComponent.CombatTarget = FleeTarget;
                EmeraldComponent.DetectionComponent.GetTargetInfo(EmeraldComponent.CombatTarget, true);
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldCombatManager.ActivateCombatState(EmeraldComponent);
            }
            else if (FleeTarget == null)
            {
                Debug.Log("The FleeTarget paramter is null. Ensure that the target exists before calling this function.");
            }
        }

        /// <summary>
        /// Assigns a new follow target for an AI to follow.
        /// </summary>
        public void SetFollowerTarget(Transform Target)
        {
            EmeraldComponent.DetectionComponent.SetTargetToFollow(Target);
        }

        /// <summary>
        /// Tames the AI to become the Target's companion. Note: The tameable AI must have a Cautious Behavior Type and 
        /// a Brave or Foolhardy Confidence Type. The AI must be tamed before the AI turns Aggressive to be successful.
        /// </summary>
        public void TameAI(Transform Target)
        {
            EmeraldComponent.CombatComponent.ClearTarget();
            EmeraldComponent.DetectionComponent.SetTargetToFollow(Target);
        }

        /// <summary>
        /// Returns the transform that last attacked the AI.
        /// </summary>
        public Transform GetLastAttacker ()
        {
            return EmeraldComponent.CombatComponent.LastAttacker;
        }

        /// <summary>
        /// Updates the AI's Health Bar color
        /// </summary>
        public void UpdateUIHealthBarColor(Color NewColor)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes)
            {
                GameObject HealthBarChild = EmeraldUI.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                UnityEngine.UI.Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<UnityEngine.UI.Image>();
                HealthBarRef.color = NewColor;
                UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                HealthBarBackgroundImageRef.color = EmeraldUI.HealthBarBackgroundColor;
            }
        }

        /// <summary>
        /// Updates the AI's Health Bar Background color
        /// </summary>
        public void UpdateUIHealthBarBackgroundColor(Color NewColor)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes)
            {
                GameObject HealthBarChild = EmeraldUI.HealthBar.transform.Find("AI Health Bar Background").gameObject;
                UnityEngine.UI.Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<UnityEngine.UI.Image>();
                HealthBarBackgroundImageRef.color = NewColor;
            }
        }

        /// <summary>
        /// Updates the AI's Name color
        /// </summary>
        public void UpdateUINameColor(Color NewColor)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes && EmeraldUI.DisplayAIName == YesOrNo.Yes)
            {
                EmeraldUI.AINameUI.color = NewColor;
            }
        }

        /// <summary>
        /// Updates the AI's Name text
        /// </summary>
        public void UpdateUINameText(string NewName)
        {
            if (EmeraldUI.AutoCreateHealthBars == YesOrNo.Yes && EmeraldUI.DisplayAIName == YesOrNo.Yes)
            {
                EmeraldUI.AINameUI.text = NewName;
            }
        }

        /// <summary>
        /// Updates the AI's dynamic wandering position to the AI's current positon.
        /// </summary>
        public void UpdateDynamicWanderPosition()
        {
            MovementComponent.StartingDestination = this.transform.position;
        }

        /// <summary>
        /// Sets the AI's dynamic wandering position to the position of the Destination transform. 
        /// This is useful for functionality such as custom AI schedules. Note: This will automatically change
        /// your AI's Wander Type to Dynamic.
        /// </summary>
        public void SetDynamicWanderPosition(Transform Destination)
        {
            MovementComponent.ChangeWanderType(EmeraldMovement.WanderTypes.Dynamic);
            MovementComponent.StartingDestination = Destination.position;
        }

        /// <summary>
        /// Updates the AI's starting position to the AI's current position.
        /// </summary>
        public void UpdateStartingPosition()
        {
            MovementComponent.StartingDestination = this.transform.position;
        }

        /// <summary>
        /// Sets the AI's destination using the transform's position.
        /// </summary>
        public void SetDestination(Transform Destination)
        {
            //EmeraldComponent.MovementComponent.SetDestinationPosition(Destination.position);
        }

        /// <summary>
        /// Sets the AI's destination using a Vector3 position.
        /// </summary>
        public void SetDestinationPosition(Vector3 DestinationPosition)
        {
            //EmeraldComponent.MovementComponent.SetDestinationPosition(DestinationPosition);
        }

        /// <summary>
        /// Generates a new position to move to within the specified radius based on the AI's current position.
        /// </summary>
        public void GenerateNewWaypointCurrentPosition(int Radius)
        {
            Vector3 NewDestination = transform.position + new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) * Radius;
            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(NewDestination.x, NewDestination.y + 5, NewDestination.z), -transform.up, out HitDown, 10, MovementComponent.DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
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
        public void AddWaypoint(Transform Waypoint)
        {
            MovementComponent.WaypointsList.Add(Waypoint.position);
        }

        /// <summary>
        /// Removes a waypoint from the AI's Wapoint List according to the specified index.
        /// </summary>
        public void RemoveWaypoint(int WaypointIndex)
        {
            MovementComponent.WaypointsList.RemoveAt(WaypointIndex);
        }

        /// <summary>
        /// Clears all of an AI's current waypoints. Note: When an AI's waypoints are cleared, it will be set to the Stationary wander type to avoid an error. 
        /// If you want the AI to follow newly created waypoints, you will need to set it's Wander Type back to Waypoint with the ChangeWanderType functio (located within the EmeraldAIEventsManager script).
        /// </summary>
        public void ClearAllWaypoints()
        {
            MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
            MovementComponent.WaypointsList.Clear();
        }

        /// <summary>
        /// Stops an AI from moving. This is useful for functionality like dialogue.
        /// </summary>
        public void StopMovement()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = true;
        }

        /// <summary>
        /// Resumes an AI's movement after using the StopMovement function.
        /// </summary>
        public void ResumeMovement()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// Stops a Companion AI from moving.
        /// </summary>
        public void StopFollowing()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = true;
        }

        /// <summary>
        /// Allows a Companion AI to resume following its follower.
        /// </summary>
        public void ResumeFollowing()
        {
            EmeraldComponent.m_NavMeshAgent.isStopped = false;
        }

        /// <summary>
        /// Allows a Companion AI to guard the assigned position.
        /// </summary>
        public void StartCompanionGuardPosition(Vector3 PositionToGuard)
        {
            EmeraldComponent.MovementComponent.DefaultMovementPaused = true;
            EmeraldComponent.m_NavMeshAgent.SetDestination(PositionToGuard);
        }

        /// <summary>
        /// Stops a Companion AI from guarding and returns it to their current follower.
        /// </summary>
        public void CancelCompanionGuardPosition()
        {
            EmeraldComponent.MovementComponent.DefaultMovementPaused = false;
        }

        /// <summary>
        /// Searches for a new target within the AI's Attacking Range clostest to the AI.
        /// </summary>
        public void SearchForClosestTarget()
        {
            EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Closest);
            EmeraldComponent.DetectionComponent.SetDetectedTarget(EmeraldComponent.CombatTarget);
        }

        /// <summary>
        /// Searches for a new random target within the AI's Attacking Range.
        /// </summary>
        public void SearchForRandomTarget()
        {
            EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Random);
        }

        /// <summary>
        /// Changes the relation of the given faction. Note: The faction must be available in the AI's faction list.
        /// </summary>
        /// <param name="Faction"> The name of the faction to change.</param>
        /// <param name="FactionLevel">The level to set the faction to typed as a string. The options are Enemy, Neutral, or Friendly</param>
        public void SetFactionLevel(string Faction, RelationTypes RelationType)
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
        /// <param name="Faction"> The name of the faction to change.</param>
        /// <param name="FactionLevel">The level to set the faction to typed as a string. The options are Enemy, Neutral, or Friendly</param>
        public void AddFactionRelation(string Faction, RelationTypes RelationType)
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
        /// Returns the relation of the EmeraldTarget with this AI in the form of a string (Enemy, Neutral, or Friendly). If a faction cannot be found, or if it is not a valid target, you will receive a value of Invalid Target.
        /// </summary>
        public string GetTargetRelation(Transform Target)
        {
            return EmeraldComponent.DetectionComponent.GetTargetFactionRelation(Target);
        }

        /// <summary>
        /// Changes the AI's faction. (Note: The FactionName must exists within the Faction Manager's Current Faction list)
        /// </summary>
        public void ChangeFaction(string FactionName)
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
        /// Checks to see if the player is currently within the AI's detection radius by returning true or false (this can be true even if the player is an enemy).
        /// </summary>
        public bool CheckForPlayerDetection ()
        {
            return EmeraldComponent.CombatTarget != null && EmeraldComponent.CombatTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag) || EmeraldComponent.LookAtTarget != null && EmeraldComponent.LookAtTarget.CompareTag(EmeraldComponent.DetectionComponent.PlayerTag);
        }

        /// <summary>
        /// Gets the faction name of the passed AI target. The AI's own transform can also be passed to get its own faction name.
        /// </summary>
        public string GetTargetFactionName(Transform Target)
        {
            return EmeraldComponent.DetectionComponent.GetTargetFactionName(Target);
        }

        /// <summary>
        /// Debug logs a message to the Unity Console for testing purposes.
        /// </summary>
        public void DebugLogMessage (string Message)
        {
            Debug.Log(Message);
        }

        /// <summary>
        /// Enables the passed gameobject.
        /// </summary>
        public void EnableObject(GameObject Object)
        {
            Object.SetActive(true);
        }

        /// <summary>
        /// Disables the passed gameobject.
        /// </summary>
        public void DisableObject(GameObject Object)
        {
            Object.SetActive(false);
        }

        /// <summary>
        /// Resets an AI to its default state. This is useful if an AI is being respawned. 
        /// </summary>
        public void ResetAI()
        {
            EmeraldComponent.ResetAI();
        }
    }
}