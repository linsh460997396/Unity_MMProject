using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component")]
    public class EmeraldDetection : MonoBehaviour, IFaction
    {
        #region Detection Variables
        public List<Collider> IgnoredColliders = new List<Collider>();
        public static LayerMask LBDLayers;
        public Transform CurrentObstruction;
        public Transform HeadTransform;
        public float DetectionFrequency = 1;
        public LayerMask DetectionLayerMask = 3;
        public LayerMask ObstructionDetectionLayerMask = 4;
        LayerMask InternalObstructionLayerMask = 4;
        public string PlayerTag = "Player";
        public float ObstructionDetectionFrequency = 0.1f;
        public float ObstructionDetectionUpdateTimer;
        public float ObstructionSeconds = 1.5f;
        public int StartingDetectionRadius;
        public int DetectionRadius = 18;
        public int StartingChaseDistance;
        public int FieldOfViewAngle = 270;
        public int StartingFieldOfViewAngle;
        public enum DetectionStates { Alert = 0, Unaware = 1 };
        public DetectionStates CurrentDetectionState = DetectionStates.Unaware;
        public PickTargetTypes PickTargetType = PickTargetTypes.Closest;
        public bool TargetObstructed = false;
        public enum ObstructedTypes { AI, Other, None };
        public ObstructedTypes ObstructionType = ObstructedTypes.None;
        public List<Collider> LineOfSightTargets = new List<Collider>();
        public List<Transform> CurrentFollowers = new List<Transform>();
        public List<EmeraldSystem> NearbyAllies = new List<EmeraldSystem>();
        public List<EmeraldSystem> LowHealthAllies = new List<EmeraldSystem>();
        public delegate void OnDetectTargetHandler();
        public event OnDetectTargetHandler OnDetectionUpdate;
        public delegate void OnEnemyTargetDetectedHandler();
        public event OnEnemyTargetDetectedHandler OnEnemyTargetDetected;
        public delegate void OnNullTargetHandler();
        public event OnNullTargetHandler OnNullTarget;
        public delegate void OnPlayerDetectedHandler();
        public event OnPlayerDetectedHandler OnPlayerDetected;
        public static List<Transform> IgnoredTargetsList = new List<Transform>();
        #endregion

        #region Faction Variables
        [SerializeField]
        public int CurrentFaction;
        public static EmeraldFactionData FactionData;
        [SerializeField]
        public static List<string> StringFactionList = new List<string>();
        public List<int> FactionRelations = new List<int>();
        [SerializeField]
        public List<FactionClass> FactionRelationsList = new List<FactionClass>();
        [SerializeField]
        public List<int> AIFactionsList = new List<int>();      
        #endregion

        #region Editor Specific Variables
        public bool HideSettingsFoldout;
        public bool DetectionFoldout;
        public bool TagFoldout;
        public bool FactionFoldout;
        #endregion

        #region Private Variables
        float DetectionTimer;
        Vector3 TargetDirection;
        float ObstructionTimer;
        EmeraldSystem EmeraldComponent;
        #endregion

        void Start()
        {
            InitializeDetection();
            Invoke(nameof(InitializeLayers), 0.1f);
        }

        /// <summary>
        /// Copy the layers from the ObstructionDetectionLayerMask. This also adds the AI's internal
        /// collider layer to its layers so its own colliders don'transform cause a false obstruction.
        /// </summary>
        void InitializeLayers ()
        {
            InternalObstructionLayerMask = ObstructionDetectionLayerMask;

            for (int i = 0; i < 32; i++)
            {
                if (LBDLayers == (LBDLayers | (1 << i)))
                {
                    InternalObstructionLayerMask |= (1 << i);
                }
            }
        }

        /// <summary>
        /// Initialize all detection related settings.
        /// </summary>
        void InitializeDetection ()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            EmeraldComponent.CombatComponent.OnExitCombat += ReturnToDefaultState; //Subscribe the ReturnToDefaultState function to the OnExitCombat delegate 
            EmeraldComponent.HealthComponent.OnDeath += ClearTargetToFollow; //Subscribe the RemoveTargetToFollow function to the OnDeath delegate 
            EmeraldComponent.HealthComponent.OnDeath += NearbyAllyDeathHandler; //Subscribe the NearbyAllyDeathHandler function to the OnDeath delegate 
            OnNullTarget += NullNonCombatTarget; //Subscribe the NullNonCombatTarget function to the OnNullTarget delegate 

            if (FactionData == null) FactionData = Resources.Load("Faction Data") as EmeraldFactionData;
            if (EmeraldComponent.LBDComponent == null) Utility.EmeraldCombatManager.DisableRagdoll(EmeraldComponent);

            StartingDetectionRadius = DetectionRadius;
            TargetObstructed = true;
            StartingFieldOfViewAngle = FieldOfViewAngle;
            StartingDetectionRadius = DetectionRadius;

            //If the user forgot to add a head transform, create a temporary one to avoid an error and still allow the AI to function.
            if (HeadTransform == null)
            {
                Transform TempHeadTransform = new GameObject("AI Head Transform").transform;
                TempHeadTransform.SetParent(transform);
                TempHeadTransform.localPosition = new Vector3(0, 1, 0);
                HeadTransform = TempHeadTransform;
            }

            SetupFactions();
            Invoke(nameof(CheckFactionRelations), 0.1f);
        }

        /// <summary>
        /// Used during initialization to check and notify the user if an AI has its own faction as an Enemy Relation.
        /// </summary>
        void CheckFactionRelations()
        {
            if (AIFactionsList.Contains(CurrentFaction) && FactionRelations[AIFactionsList.IndexOf(CurrentFaction)] == 0)
            {
                Debug.LogError("The AI '" + gameObject.name + "' contains an Enemy Faction Relation of its own Faction '" + GetTargetFactionName(transform) +
                    "'. Please remove the faction from the AI Faction Relation List (within the AI's Detection Component) or change it to Friendly to avoid incorrect target detection.");
            }
        }

        /// <summary>
        /// Sets up the Factions to be used during runtime.
        /// </summary>
        void SetupFactions()
        {
            for (int i = 0; i < FactionRelationsList.Count; i++)
            {
                AIFactionsList.Add(FactionRelationsList[i].FactionIndex);
                FactionRelations.Add((int)FactionRelationsList[i].RelationType);
            }
        }

        void FixedUpdate()
        {
            if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType == EmeraldBehaviors.BehaviorTypes.Passive) return; //Don'transform allow passive AI to use line of sight

            if (!EmeraldComponent.CombatComponent.CombatState || EmeraldComponent.CombatComponent.DeathDelayActive)
            {
                LineOfSightDetection();
            }
        }

        /// <summary>
        /// A custom update function for the EmeraldDetection script called through the EmeraldAISystem script.
        /// </summary>
        public void DetectionUpdate()
        {
            if (EmeraldComponent.CombatComponent.CombatState) CheckForObstructions(EmeraldComponent.CombatTarget); //When in combat, check for obstructions by casting a ray from the AI's Head Transform to its target.
            else if (!EmeraldComponent.CombatComponent.CombatState) CheckForObstructions(EmeraldComponent.LookAtTarget); //When in not in combat, check for obstructions by casting a ray from the AI's Head Transform to its look at target.

            //Update the AI's OverlapShere function based on the DetectionFrequency
            if (EmeraldComponent.CombatComponent.TargetDetectionActive && !EmeraldComponent.MovementComponent.ReturningToStartInProgress)
            {
                DetectionTimer += Time.deltaTime;

                if (DetectionTimer >= DetectionFrequency)
                {
                    UpdateAIDetection(); //Casts a Physics.OverlapSphere and only searches for layers based on the user set DetectionLayerMask.
                    LookAtTargetDistanceCheck(); //Check that the LookAtTarget is within the AI's DetectionRadius.
                    OnDetectionUpdate?.Invoke(); //Invoke the OnDetectionUpdate event.
                    DetectionTimer = 0;
                }
            }

            CheckForNullTarget(); //Monitors the AI's TargetSource to see if it becomes null. If it does, invoke the OnNullTarget callback.
            CheckLookAtTarget(); //Monitors the AI's LookAtTarget to see if its health reaches 0. If it does, clear the LookAtTarget information.
            ObstructionAction(); //Controls what happens depending on if the AI is obstructed by another AI or by something else, while in combat.
        }

        /// <summary>
        /// Monitors the AI's TargetSource to see if it becomes null. If it does, invoke the OnNullTarget callback.
        /// </summary>
        void CheckForNullTarget()
        {
            if (EmeraldComponent.CurrentTargetInfo.TargetSource == null && EmeraldComponent.CurrentTargetInfo.CurrentICombat != null)
            {
                OnNullTarget?.Invoke();
            }
        }

        /// <summary>
        /// Called through the OnNullTarget callback when a target becomes null. If this happens, clear all non-combat targets.
        /// </summary>
        void NullNonCombatTarget ()
        {
            if (!EmeraldComponent.CombatComponent.CombatState)
            {
                EmeraldComponent.TargetToFollow = null;
                EmeraldComponent.LookAtTarget = null;
                EmeraldComponent.CurrentTargetInfo.TargetSource = null;
                EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = null;
                EmeraldComponent.CurrentTargetInfo.CurrentICombat = null;
            }
        }

        /// <summary>
        /// Monitors the AI's LookAtTarget to see if its health reaches 0. If it does, clear the LookAtTarget information.
        /// </summary>
        void CheckLookAtTarget ()
        {
            if (EmeraldComponent.LookAtTarget && !EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.CurrentTargetInfo.CurrentIDamageable.Health <= 0)
            {
                EmeraldComponent.LookAtTarget = null;
                EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = null;
                EmeraldComponent.CurrentTargetInfo.CurrentICombat = null;
            }
        }

        /// <summary>
        /// Controls what happens depending on if the AI is obstructed by another AI or by something else, while in combat.
        /// </summary>
        void ObstructionAction ()
        {
            if (TargetObstructed && EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.MovementComponent.CanReachTarget)
            {
                ObstructionTimer += Time.deltaTime;
                if (ObstructionTimer >= ObstructionSeconds)
                {
                    if (ObstructionType == ObstructedTypes.AI)
                    {
                        SearchForTarget(PickTargetTypes.Random);
                    }
                    else if (ObstructionType == ObstructedTypes.Other)
                    {
                        EmeraldComponent.m_NavMeshAgent.stoppingDistance = 3;
                        EmeraldAPI.Internal.GenerateRandomPositionWithinRadius(EmeraldComponent);
                    }

                    ObstructionTimer = 0;
                }
            }
            else if (!TargetObstructed && EmeraldComponent.CombatComponent.CombatState && !EmeraldComponent.AnimationComponent.IsBackingUp)
            {
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
            }
        }

        /// <summary>
        /// Casts a Physics.OverlapSphere and only searches for layers based on the user set DetectionLayerMask.
        /// </summary>
        public void UpdateAIDetection()
        {
            if (LineOfSightTargets.Count > 0) LineOfSightTargetsDistanceCheck();

            if (NearbyAllies.Count > 0) NearbyAlliesDistanceCheck();

            Collider[] CurrentlyDetectedTargets = Physics.OverlapSphere(transform.position, DetectionRadius, DetectionLayerMask);

            foreach (Collider C in CurrentlyDetectedTargets)
            {
                if (C.gameObject != this.gameObject && IsValidTarget(C.transform))
                {
                    DetectTarget(C.transform);
                }
            }
        }

        /// <summary>
        /// Assigns a target based on the passed parameter and an AI's settings. Store the target within LineOfSightTargets to be used elsewhere.
        /// </summary>
        void DetectTarget(Transform Target)
        {
            if (IgnoredTargetsList.Contains(Target))
                return;

            //Clean up
            //if (Target != EmeraldComponent.TargetToFollow && !CurrentFollowers.Contains(Target) && IsEnemyTarget(Target) && EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
            //if (Target != EmeraldComponent.TargetToFollow && !CurrentFollowers.Contains(Target) && EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
            if (EmeraldComponent.BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
            {
                if (IsEnemyTarget(Target))
                {
                    CurrentDetectionState = DetectionStates.Alert;
                    if (!LineOfSightTargets.Contains(Target.GetComponent<Collider>()))
                        LineOfSightTargets.Add(Target.GetComponent<Collider>());
                }
                //Get nearby allies (Allies can only be other AI).
                else if (IsFriendlyTarget(Target))
                {
                    EmeraldSystem AllyEmeraldComponent = Target.GetComponent<EmeraldSystem>();
                    if (AllyEmeraldComponent && !NearbyAllies.Contains(AllyEmeraldComponent)) NearbyAllies.Add(AllyEmeraldComponent);
                }
            }

            if (EmeraldComponent.LookAtTarget == null && EmeraldComponent.CombatTarget == null)
            {
                if (IsLookAtTarget(Target))
                {
                    EmeraldComponent.LookAtTarget = Target;
                    GetTargetInfo(EmeraldComponent.LookAtTarget);
                    OnPlayerDetected?.Invoke();
                }
            }
        }

        /// <summary>
        /// Calculates the AI's line of sight mechanics. For each target that is within the AI's LineOfSightTargets, cast a raycast. If a target is unobstructed, and within the AI's line of sight angle, call the SearchForTarget function.
        /// </summary>
        void LineOfSightDetection ()
        {
            if (CurrentDetectionState == DetectionStates.Alert && EmeraldComponent.CombatTarget == null && !EmeraldComponent.AnimationComponent.IsDead)
            {
                for (int i = LineOfSightTargets.Count - 1; i >= 0; --i)
                {
                    if (LineOfSightTargets[i] == null)
                    {
                        LineOfSightTargets.RemoveAt(i);
                    }
                    else
                    {
                        Vector3 direction = LineOfSightTargets[i].bounds.center - HeadTransform.position;
                        float angle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), transform.forward);

                        //Only check targets that are within the AI's line of sight.
                        //if (angle < FieldOfViewAngle * 0.5f)
                        float fieldOfView = EmeraldComponent.CombatComponent.CombatState ? 360f : FieldOfViewAngle * 0.5f;
                        if (angle < fieldOfView)
                        {
                            if (!EmeraldComponent.CombatComponent.CombatState)
                            {
                                RaycastHit hit;
                                //Use a special layer mask that also includes the layers of internal colliders (from the LDB component) as these can block the AI's line of sight.
                                if (Physics.Raycast(HeadTransform.position, direction, out hit, DetectionRadius, ~InternalObstructionLayerMask))
                                {
                                    if (hit.collider != null && LineOfSightTargets.Contains(hit.collider))
                                    {
                                        SearchForTarget(PickTargetType);
                                        break;
                                    }
                                }
                            }
                            else if (EmeraldComponent.CombatComponent.CombatState && LineOfSightTargets.Count > 0)
                            {
                                SearchForTarget(PickTargetType);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return all currently visible targets within the AI's detection radius.
        /// </summary>
        public List<Transform> GetVisibleTargets()
        {
            List<Transform> VisibleTargets = new List<Transform>();

            foreach (Collider C in LineOfSightTargets.ToArray())
            {
                RaycastHit hit;
                Vector3 direction = C.bounds.center - HeadTransform.position;
                
                if (Physics.Raycast(HeadTransform.position, direction, out hit, DetectionRadius, ~InternalObstructionLayerMask))
                {
                    if (hit.collider != null && LineOfSightTargets.Contains(hit.collider))
                    {
                        if (!VisibleTargets.Contains(hit.collider.transform) && EmeraldComponent.CombatTarget != hit.collider.transform || hit.collider.CompareTag(PlayerTag))
                        {
                            VisibleTargets.Add(hit.collider.transform);
                        }
                    }
                }
            }

            return VisibleTargets;
        }

        /// <summary>
        /// Searches for a currently visible target within the LineOfSightTargets list using passed PickTargetType. This can be assigned using EmeraldDetection.PickTargetTypes.
        /// </summary>
        public void SearchForTarget (PickTargetTypes pickTargetType)
        {
            List<Transform> VisibleTargets = GetVisibleTargets();
            if (EmeraldComponent.CombatTarget != null) VisibleTargets.Remove(EmeraldComponent.CombatTarget); //Remove the current target so it isn'transform picked again

            if (VisibleTargets.Count > 0)
            {
                if (pickTargetType == PickTargetTypes.Closest)
                {
                    VisibleTargets = VisibleTargets.OrderBy(Target => (Target.position - transform.position).sqrMagnitude).ToList();
                    SetDetectedTarget(VisibleTargets[0]);
                }
                else if (pickTargetType == PickTargetTypes.Random)
                {
                    SetDetectedTarget(VisibleTargets[Random.Range(0, VisibleTargets.Count)]);
                }
                else if (pickTargetType == PickTargetTypes.FirstDetected)
                {
                    SetDetectedTarget(VisibleTargets[0]);
                }
            }
        }

        /// <summary>
        /// Check for obstructions by casting a ray from the AI's Head Transform to its target.
        /// </summary>
        void CheckForObstructions (Transform TargetSource)
        {
            ObstructionDetectionUpdateTimer += Time.deltaTime;

            if (ObstructionDetectionUpdateTimer >= ObstructionDetectionFrequency && TargetSource != null && EmeraldComponent.CurrentTargetInfo.CurrentICombat != null)
            {
                TargetDirection = EmeraldComponent.CurrentTargetInfo.CurrentICombat.DamagePosition() - HeadTransform.position;

                RaycastHit hit;

                //Check for obstructions and incrementally lower our AI's stopping distance until one is found. If none are found when the distance has reached 5 or below, search for a new target to see if there is a better option
                if (Physics.Raycast(HeadTransform.position, (TargetDirection), out hit, EmeraldComponent.CombatComponent.DistanceFromTarget, ~InternalObstructionLayerMask))
                {
                    if (!hit.collider.transform.IsChildOf(TargetSource) && !hit.collider.transform.IsChildOf(this.transform) && hit.collider.transform != TargetSource && !IgnoredColliders.Contains(hit.collider))
                    {
                        //Set the ObstructionType so different actions can be taken when an AI's line of sight becomes obstructed.
                        if ((LBDLayers & (1 << hit.collider.gameObject.layer)) != 0 || (DetectionLayerMask & (1 << hit.collider.gameObject.layer)) != 0)
                        {
                            ObstructionType = ObstructedTypes.AI;
                        }
                        else
                        {
                            ObstructionType = ObstructedTypes.Other;
                        }
                        
                        EmeraldComponent.AIAnimator.ResetTrigger("Attack");
                        TargetObstructed = true;
                        CurrentObstruction = hit.collider.transform;
                    }
                    else
                    {
                        TargetObstructed = false;
                        CurrentObstruction = null;
                        ObstructionType = ObstructedTypes.None;
                    }
                }
                else
                {
                    TargetObstructed = false;
                    CurrentObstruction = null;
                    ObstructionType = ObstructedTypes.None;
                }

                ObstructionDetectionUpdateTimer = 0;
            }
        }

        /// <summary>
        /// Detects the passed target's Target Type and assigns it as the AI's current target.
        /// </summary>
        public void SetDetectedTarget (Transform DetectedTarget)
        {
            //Don'transform assign the newly detected target if it's the same as the current target. 
            if (EmeraldComponent.CombatTarget == DetectedTarget) return;

            EmeraldAI.Utility.EmeraldCombatManager.ActivateCombatState(EmeraldComponent); //Active the Combat State
            ResetDetectionValues(); //Once a target has been found, reset some of its settings back to their defaults.
            GetTargetInfo(DetectedTarget);
            EmeraldComponent.CombatTarget = DetectedTarget;
            OnEnemyTargetDetected?.Invoke(); //Invoke the OnEnemyTargetDetected when an enemy target has been found.
        }

        /// <summary>
        /// Once a target has been found, reset some of its settings back to their defaults.
        /// </summary>
        void ResetDetectionValues ()
        {
            DetectionRadius = StartingDetectionRadius;
            FieldOfViewAngle = StartingFieldOfViewAngle;
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
            EmeraldComponent.AnimationComponent.IsTurning = false;
            EmeraldComponent.CombatComponent.DeathDelayActive = false;
            EmeraldComponent.CombatComponent.DeathDelayTimer = 0;
        }

        /// <summary>
        /// Get the target's info (from the passed Target parameter).
        /// </summary>
        public void GetTargetInfo (Transform Target, bool? OverrideFactionRequirement = false)
        {
            if (Target != null)
            {
                EmeraldComponent.CurrentTargetInfo.TargetSource = Target;
                EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = Target.GetComponent<IDamageable>();
                EmeraldComponent.CurrentTargetInfo.CurrentICombat = Target.GetComponent<ICombat>();
            }
        }

        /// <summary>
        /// Check that each LineOfSightTarget is within the AI's DetectionRadius. If not, remove it from the list.
        /// </summary>
        void LineOfSightTargetsDistanceCheck()
        {
            for (int i = 0; i < LineOfSightTargets.Count; i++)
            {
                //Remove any targets that become null or who's dead during the distance check.
                if (LineOfSightTargets[i] == null || LineOfSightTargets[i].GetComponent<IDamageable>().Health  <=0 )
                {
                    LineOfSightTargets.RemoveAt(i);
                }
                else
                {
                    float distance = Vector3.Distance(LineOfSightTargets[i].transform.position, transform.position);

                    //If the distance of the detected target is greater than the DetectionRadius, remove it from the LineOfSightTargets list.
                    if (distance > DetectionRadius)
                        LineOfSightTargets.Remove(LineOfSightTargets[i]);
                }
            }
        }

        /// <summary>
        /// Check that each LineOfSightTarget is within the AI's DetectionRadius. If not, remove it from the list.
        /// </summary>
        void NearbyAlliesDistanceCheck()
        {
            for (int i = 0; i < NearbyAllies.Count; i++)
            {
                //Remove any targets that become null during the distance check.
                if (NearbyAllies[i] == null)
                {
                    NearbyAllies.RemoveAt(i);
                }
                else
                {
                    float distance = Vector3.Distance(NearbyAllies[i].transform.position, transform.position);

                    //If the distance of the detected friendly is greater than the DetectionRadius, remove it from the NearbyAllies list.
                    if (distance > DetectionRadius)
                        NearbyAllies.Remove(NearbyAllies[i]);
                }
            }
        }

        /// <summary>
        /// Check that the LookAtTarget is within the AI's DetectionRadius. If not, remove it as the current LookAtTarget.
        /// </summary>
        void LookAtTargetDistanceCheck ()
        {
            if (EmeraldComponent.LookAtTarget != null)
            {
                float distance = Vector3.Distance(EmeraldComponent.LookAtTarget.transform.position, transform.position);

                //If the distance of the detected LookAtTarget is greater than the DetectionRadius, remove it as the current LookAtTarget.
                if (distance > DetectionRadius)
                    NullNonCombatTarget();
            }
        }

        /// <summary>
        /// Return to the default state and assign the Look At Target info, if it's not null. This is called through the OnExitCombat callback.
        /// </summary>
        void ReturnToDefaultState ()
        {
            CurrentDetectionState = DetectionStates.Unaware;
            if (EmeraldComponent.LookAtTarget != null)
                GetTargetInfo(EmeraldComponent.LookAtTarget);
        }

        /// <summary>
        /// Removes this AI from all of its allies NearbyAllies lists (called through the OnDeath callback).
        /// </summary>
        void NearbyAllyDeathHandler ()
        {
            if (NearbyAllies.Count > 0)
            {
                for (int i = 0; i < NearbyAllies.Count; i++)
                {
                    if (NearbyAllies[i].DetectionComponent.NearbyAllies.Contains(EmeraldComponent)) NearbyAllies[i].DetectionComponent.NearbyAllies.Remove(EmeraldComponent);
                }
            }
        }

        /// <summary>
        /// Gets the Faction Relation name of the passed target and this AI in the form of a string (Enemy, Neutral, or Friendly). If a faction cannot be found, or if it is not a valid target, you will receive a value of Invalid Target.
        /// </summary>
        public string GetTargetFactionRelation (Transform Target)
        {
            return EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, Target);
        }

        /// <summary>
        /// Gets the faction name of the passed AI target.
        /// </summary>
        public string GetTargetFactionName(Transform Target)
        {
            return EmeraldAPI.Faction.GetTargetFactionName(Target);
        }

        /// <summary>
        /// Assigns a new follow target for an AI to follow.
        /// </summary>
        public void SetTargetToFollow(Transform Target, bool CopyFactionData = true)
        {
            EmeraldSystem TargetEmeraldComponent = Target.GetComponent<EmeraldSystem>(); //Attempt to get the Target's EmeraldComponent
            if (TargetEmeraldComponent != null)
            {
                if (TargetEmeraldComponent.CombatTarget == transform) TargetEmeraldComponent.CombatComponent.ClearTarget(); //If the Target is another AI, clear its targets
                TargetEmeraldComponent.DetectionComponent.CurrentFollowers.Add(transform); //Add this AI as a follower of the leader AI
                if (TargetEmeraldComponent.CombatComponent.CombatState) TargetEmeraldComponent.CombatComponent.DeathDelayActive = true;

                //Copies the Target to Follow's Faction Data so it will react the same way the follower does to detected targets.
                if (CopyFactionData)
                {
                    CurrentFaction = TargetEmeraldComponent.DetectionComponent.CurrentFaction; //Make the Current Faction the same as the AI's new Target to Follow
                    AIFactionsList = TargetEmeraldComponent.DetectionComponent.AIFactionsList; //Make the Faction List the same as the AI's new Target to Follow
                    FactionRelations = TargetEmeraldComponent.DetectionComponent.FactionRelations; //Make the Faction Relations the same as the AI's new Target to Follow
                    FactionRelationsList = TargetEmeraldComponent.DetectionComponent.FactionRelationsList; //Make the Faction Relations List the same as the AI's new Target to Follow
                }
            }

            if (Target == EmeraldComponent.CombatTarget) EmeraldComponent.CombatComponent.ClearTarget();
            if (EmeraldComponent.CombatComponent.CombatState) EmeraldComponent.CombatComponent.DeathDelayActive = true;

            EmeraldComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.ResetState();
            EmeraldComponent.MovementComponent.CurrentMovementState = EmeraldMovement.MovementStates.Run;
            EmeraldComponent.MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
        }

        /// <summary>
        /// Initializes a summoned target.
        /// </summary>
        public void InitializeSummonTarget(Transform Target, bool CopyFactionData = true)
        {
            EmeraldSystem TargetEmeraldComponent = Target.GetComponent<EmeraldSystem>(); //Attempt to get the Target's EmeraldComponent
            if (TargetEmeraldComponent != null)
            {
                if (TargetEmeraldComponent.CombatTarget == transform) TargetEmeraldComponent.CombatComponent.ClearTarget(); //If the Target is another AI, clear its targets
                TargetEmeraldComponent.DetectionComponent.CurrentFollowers.Add(transform); //Add this AI as a follower of the leader AI

                //Copies the Target to Follow's Faction Data so it will react the same way the follower does to detected targets.
                if (CopyFactionData)
                {
                    CurrentFaction = TargetEmeraldComponent.DetectionComponent.CurrentFaction; //Make the Current Faction the same as the AI's new Target to Follow
                    AIFactionsList = TargetEmeraldComponent.DetectionComponent.AIFactionsList; //Make the Faction List the same as the AI's new Target to Follow
                    FactionRelations = TargetEmeraldComponent.DetectionComponent.FactionRelations; //Make the Faction Relations the same as the AI's new Target to Follow
                    FactionRelationsList = TargetEmeraldComponent.DetectionComponent.FactionRelationsList; //Make the Faction Relations List the same as the AI's new Target to Follow
                }
            }

            if (Target == EmeraldComponent.CombatTarget) EmeraldComponent.CombatComponent.ClearTarget();

            EmeraldComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.TargetToFollow = Target;
            EmeraldComponent.BehaviorsComponent.ResetState();
            EmeraldComponent.MovementComponent.CurrentMovementState = EmeraldMovement.MovementStates.Run;
            EmeraldComponent.MovementComponent.WanderType = EmeraldMovement.WanderTypes.Stationary;
            Invoke(nameof(UpdateAIDetection), 0.1f); //Manually update UpdateAIDetection in case something happens before the Detection Update tick.
        }

        /// <summary>
        /// Clears the AI's Target to Follow transform so it will be no longer following it. This will also stop the AI from being a Companion AI.
        /// </summary>
        public void ClearTargetToFollow()
        {
            //This is also called through the OnDeath callback and removes this AI as a follower of its Target to Follow.
            if (EmeraldComponent.TargetToFollow)
            {
                EmeraldSystem TargetEmeraldComponent = EmeraldComponent.TargetToFollow.GetComponent<EmeraldSystem>(); //Attempt to get this AI's Target to Follow

                if (TargetEmeraldComponent)
                {
                    TargetEmeraldComponent.DetectionComponent.CurrentFollowers.Remove(transform); //Remove this AI as a follower of the leader AI
                }
            }

            EmeraldComponent.BehaviorsComponent.TargetToFollow = null;
            EmeraldComponent.TargetToFollow = null;

            if (!EmeraldComponent.AnimationComponent.IsDead)
            {
                EmeraldComponent.BehaviorsComponent.ResetState();
            }
        }

        /// <summary>
        /// Returns true if the currently passed transform is an enemy target.
        /// </summary>
        bool IsEnemyTarget (Transform Target)
        {
            int ReceivedFaction = Target.GetComponent<IFaction>().GetFaction();
            return AIFactionsList.Contains(ReceivedFaction) && FactionRelations[AIFactionsList.IndexOf(ReceivedFaction)] == 0;
        }

        /// <summary>
        /// Returns true if the currently passed transform is an enemy target.
        /// </summary>
        bool IsFriendlyTarget(Transform Target)
        {
            int ReceivedFaction = Target.GetComponent<IFaction>().GetFaction();
            return AIFactionsList.Contains(ReceivedFaction) && FactionRelations[AIFactionsList.IndexOf(ReceivedFaction)] == 2 || ReceivedFaction == CurrentFaction;
        }

        /// <summary>
        /// Returns true if the currently passed transform is a valid look at target.
        /// </summary>
        bool IsLookAtTarget(Transform Target)
        {
            return Target.gameObject.CompareTag(PlayerTag) && GetTargetFactionRelation(Target) != "Enemy";
        }

        /// <summary>
        /// Checks to see if the passed target is a valid player, AI, or non-AI target.
        /// </summary>
        bool IsValidTarget (Transform Target)
        {
            if (Target.GetComponent<IFaction>() != null)
            {
                return true;
            }
            else
            {
                Debug.Log("The " + Target.name + " object is set as a valid target (both Tag and Layer), but does not have a Faction Extension component on it. Please add one in order for this target to be properly detected.");
                return false;
            }
        }

        public int GetFaction()
        {
            return CurrentFaction;
        }
    }
}