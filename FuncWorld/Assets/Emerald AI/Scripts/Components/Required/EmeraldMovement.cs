using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/movement-component")]
    public class EmeraldMovement : MonoBehaviour
    {
        #region Movement Variables
        public bool CanReachTarget;
        public float DistanceFromFollower;
        public float CalculatePathSeconds = 1f; //Controls how often the NavMesh Calculate Path is updated.
        float CalculatePathTimer = 0f; //Controls how often the NavMesh Calculate Path is updated.
        public bool DefaultMovementPaused; //A bool used to pause any built-in movement functions (Wander and CombatMovement). This can be used during custom actions or behaviors if needed.
        public enum WanderTypes { Dynamic = 0, Waypoints = 1, Stationary = 2, Destination = 3, Custom = 4 };
        public WanderTypes WanderType = WanderTypes.Dynamic;
        public enum WaypointTypes { Loop = 0, Reverse = 1, Random = 2 };
        public WaypointTypes WaypointType = WaypointTypes.Random;
        public enum MovementTypes { RootMotion, NavMeshDriven };
        public MovementTypes MovementType = MovementTypes.RootMotion;
        public enum MovementStates { Walk = 0, Run };
        public MovementStates StartingMovementState = MovementStates.Run;
        public MovementStates CurrentMovementState = MovementStates.Walk;
        public YesOrNo UseRandomRotationOnStart = YesOrNo.No;
        public YesOrNo AlignAIOnStart = YesOrNo.No;
        public YesOrNo AlignAIWithGround = YesOrNo.No;
        public enum AlignmentQualities { Low = 0, Medium = 1, High = 2 };
        public AlignmentQualities AlignmentQuality = AlignmentQualities.Medium;
        public float WalkBackwardsSpeed = 1;
        public float WalkSpeed = 2;
        public float RunSpeed = 5;
        public float ForceWalkDistance = 2.5f;
        public float StoppingDistance = 2;
        public int NonCombatAngleToTurn = 20;
        public int CombatAngleToTurn = 30;
        public int StationaryTurningSpeedNonCombat = 10;
        public int StationaryTurningSpeedCombat = 10;
        public int MovingTurnSpeedNonCombat = 200;
        public int MovingTurnSpeedCombat = 200;
        public int BackupTurningSpeed = 150;
        public int MaxNormalAngle = 20;
        public int MaxSlopeLimit = 30;
        public int NonCombatAlignmentSpeed = 15;
        public int CombatAlignmentSpeed = 25;
        public float MovementTurningSensitivity = 2f;
        public int MinimumWaitTime = 3;
        public int MaximumWaitTime = 6;
        public LayerMask AlignmentLayerMask = 1;
        public int AngleToTurn;
        public float DecelerationDampTime = 0.15f;
        public float WaypointTimer;
        public int WaypointIndex = 0;
        public List<Vector3> WaypointsList = new List<Vector3>();
        public float DestinationAdjustedAngle;
        public Vector3 DestinationDirection;
        public bool RotateTowardsTarget = false;
        public int StationaryIdleSecondsMin = 3;
        public int StationaryIdleSecondsMax = 6;
        public int GeneratedBackupOdds;
        public int StartingWanderingType;
        public bool ReturnToStationaryPosition;
        public Vector3 SingleDestination;
        public Vector3 StartingDestination;
        public EmeraldWaypointObject m_WaypointObject;
        public int WanderRadius = 25;
        public LayerMask DynamicWanderLayerMask = ~0;
        public LayerMask BackupLayerMask = 1;
        public bool LockTurning;
        public bool ReturningToStartInProgress = false;
        public bool AIAgentActive = false;
        public delegate void ReachedDestinationHandler();
        public event ReachedDestinationHandler OnReachedDestination;
        public delegate void ReachedWaypointHandler();
        public event ReachedWaypointHandler OnReachedWaypoint;
        public delegate void GeneratedWaypointHandler();
        public event GeneratedWaypointHandler OnGeneratedWaypoint;
        #endregion

        #region Editor Variables
        public bool HideSettingsFoldout;
        public bool WanderFoldout = false;
        public bool WaypointsFoldout = false;
        public bool WaypointsListFoldout = false;
        public bool BackupFoldout = false;
        public bool MovementFoldout = false;
        public bool AlignmentFoldout = false;
        public bool TurnFoldout = false;
        #endregion

        #region Private Variables
        EmeraldSystem EmeraldComponent;
        EmeraldAnimation AnimationComponent;
        NavMeshAgent m_NavMeshAgent;
        NavMeshPath AIPath;
        Animator AIAnimator;
        float DirectionDampTime = 0.25f;
        float RayCastUpdateTimer;
        float StationaryIdleTimer = 0;
        Quaternion NormalRotation;
        bool WaypointReverseActive;
        bool ReachedDestination;
        int m_LastWaypointIndex = 0;
        bool BackupDelayActive;
        float BackupDistance;
        Coroutine m_RotateTowards;
        float BackingUpTimer;
        int StationaryIdleSeconds;
        Vector3 SurfaceNormal;
        float SurfaceDistance;
        float RayCastUpdateSeconds = 0.1f;
        float WaitTime = 5;
        bool IdleActivated;
        bool ReachedWaypoint;
        bool MovementInitialized;
        Quaternion qTarget;
        Quaternion qGround;
        Quaternion Slope;
        Quaternion Final;
        Vector3 BackupDirection;
        float BackupTimer;
        Coroutine BackupCoroutine;
        Vector3 ActionDirection;
        #endregion

        void Start()
        {
            InitializeMovement(); //Initialize the EmeraldMovement script.
        }

        /// <summary>
        /// Initialize the movement settings.
        /// </summary>
        public void InitializeMovement ()
        {
            AIPath = new NavMeshPath();
            AIAnimator = GetComponent<Animator>();
            EmeraldComponent = GetComponent<EmeraldSystem>();
            AnimationComponent = GetComponent<EmeraldAnimation>();
            EmeraldComponent.CombatComponent.OnExitCombat += DefaultMovement; //Subscribe to the OnExitCombat event to set an AI's DefaultMovement state.
            StartingMovementState = CurrentMovementState;
            WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
            StationaryIdleSeconds = Random.Range(StationaryIdleSecondsMin, StationaryIdleSecondsMax + 1);
            StartingWanderingType = (int)WanderType;
            StartingDestination = transform.position;
            SetupNavMeshAgent();

            if (AlignmentQuality == AlignmentQualities.Low)
            {
                RayCastUpdateSeconds = 0.3f;
            }
            else if (AlignmentQuality == AlignmentQualities.Medium)
            {
                RayCastUpdateSeconds = 0.2f;
            }
            else if (AlignmentQuality == AlignmentQualities.High)
            {
                RayCastUpdateSeconds = 0.1f;
            }

            if (UseRandomRotationOnStart == YesOrNo.Yes)
            {
                transform.rotation = Quaternion.AngleAxis(Random.Range(5, 360), Vector3.up);
            }

            if (AlignAIOnStart == YesOrNo.Yes && AlignAIWithGround == YesOrNo.Yes)
            {
                AlignOnStart();
            }
        }

        /// <summary>
        /// Sets up all NavMesh settings and values.
        /// </summary>
        public void SetupNavMeshAgent()
        {   
            m_NavMeshAgent = GetComponent<NavMeshAgent>();          

            if (GetComponent<Rigidbody>())
            {
                Rigidbody RigidbodyComp = GetComponent<Rigidbody>();
                RigidbodyComp.isKinematic = true;
                RigidbodyComp.useGravity = false;
            }

            if (m_NavMeshAgent == null)
            {
                gameObject.AddComponent<NavMeshAgent>();
                m_NavMeshAgent = GetComponent<NavMeshAgent>();
            }

            AIPath = new NavMeshPath();
            m_NavMeshAgent.CalculatePath(transform.position, AIPath);
            m_NavMeshAgent.stoppingDistance = StoppingDistance;
            m_NavMeshAgent.updateRotation = false;
            m_NavMeshAgent.updateUpAxis = false;
            m_NavMeshAgent.speed = 0;
            if (MovementType == MovementTypes.NavMeshDriven) m_NavMeshAgent.acceleration = 75;

            if (m_NavMeshAgent.enabled)
            {
                if (WanderType == WanderTypes.Destination)
                {
                    m_NavMeshAgent.autoBraking = false;
                    ReachedDestination = true;
                    StartCoroutine(SetDelayedDestination(SingleDestination));
                    CheckPath(SingleDestination);
                }
                else if (WanderType == WanderTypes.Waypoints)
                {
                    if (WaypointType != WaypointTypes.Random)
                    {
                        if (WaypointsList.Count > 0)
                        {
                            m_NavMeshAgent.stoppingDistance = 0.1f;
                            m_NavMeshAgent.autoBraking = false;
                            StartCoroutine(SetDelayedDestination(WaypointsList[WaypointIndex]));
                        }
                    }
                    else if (WaypointType == WaypointTypes.Random)
                    {
                        if (WaypointsList.Count > 0)
                        {
                            WaypointIndex = Random.Range(0, WaypointsList.Count);
                            StartCoroutine(SetDelayedDestination(WaypointsList[WaypointIndex]));
                            m_NavMeshAgent.autoBraking = false;
                        }
                    }

                    if (WaypointsList.Count == 0)
                    {
                        WanderType = WanderTypes.Stationary;
                        ReachedDestination = true;
                        m_NavMeshAgent.stoppingDistance = StoppingDistance;
                        MovementInitialized = true;
                    }
                }
                else if (WanderType == WanderTypes.Stationary || WanderType == WanderTypes.Dynamic || WanderType == WanderTypes.Custom)
                {
                    ReachedDestination = true;
                    m_NavMeshAgent.autoBraking = false;
                    StartCoroutine(SetDelayedDestination(StartingDestination));
                }
            }
        }

        IEnumerator SetDelayedDestination(Vector3 Destination)
        {
            m_NavMeshAgent.destination = Destination;
            yield return new WaitForSeconds(1);
            LockTurning = false;
            AIAnimator.SetBool("Idle Active", false);
            MovementInitialized = true;
        }

        /// <summary>
        /// Check our AI's path to ensure if it is reachable. If it isn'transform, regenerate, depending on the Wander Type.
        /// </summary>
        void CheckPath(Vector3 Destination)
        {
            NavMeshPath path = new NavMeshPath();
            m_NavMeshAgent.CalculatePath(Destination, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                //Path is valid
            }
            else if (path.status == NavMeshPathStatus.PathPartial)
            {
                if (WanderType == WanderTypes.Destination)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Destination is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Destination and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
                else if (WanderType == WanderTypes.Waypoints)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Waypoint #" + (WaypointIndex + 1) + " is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Waypoint #" + (WaypointIndex + 1) + " and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
            }
            else if (path.status == NavMeshPathStatus.PathInvalid)
            {
                if (WanderType == WanderTypes.Destination)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Destination is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Destination and make sure it is on the NavMesh.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
                else if (WanderType == WanderTypes.Waypoints)
                {
                    Debug.LogError("The AI ''" + gameObject.name + "'s'' Waypoint #" + (WaypointIndex + 1) + " is not reachable. " +
                        "The AI's Wander Type has been set to Stationary. Please check the Waypoint #" + (WaypointIndex + 1) + " and make sure it is on the NavMesh and is reachable.");
                    m_NavMeshAgent.stoppingDistance = StoppingDistance;
                    StartingDestination = transform.position + (transform.forward * StoppingDistance);
                    WanderType = WanderTypes.Stationary;
                }
            }
            else
            {
                Debug.Log("Path Invalid");
            }
        }

        /// <summary>
        /// Moves our AI using Unity's NavMesh Agent 
        /// </summary>
        void MoveAINavMesh()
        {
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * m_NavMeshAgent.desiredVelocity;
            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //Handles all of the AI's movement and speed calculations for NavMesh movement.
            if (AIAgentActive && m_NavMeshAgent.isOnNavMesh && MovementInitialized)
            {
                AIAnimator.SetFloat("Direction", angle * MovementTurningSensitivity, DirectionDampTime, Time.deltaTime);

                if (m_NavMeshAgent.isStopped || AIAnimator.GetBool("Hit") || m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance || CanIdle())
                {
                    AIAnimator.SetFloat("Speed", 0, 0.3f, Time.deltaTime);
                    m_NavMeshAgent.speed = 0;
                }
                else if (!m_NavMeshAgent.isStopped && !AIAnimator.GetBool("Hit") && m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance && CanMove())
                {
                    //Force walk movement if getting close to an AI's destination. This helps prevent the AI from running into its target as Root Motion with NavMesh needs a bit of time to stop.
                    if (m_NavMeshAgent.remainingDistance < (m_NavMeshAgent.stoppingDistance + ForceWalkDistance))
                    {
                        m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, WalkSpeed, Time.deltaTime * 2);
                        AIAnimator.SetFloat("Speed", 0.5f, 0.3f, Time.deltaTime);
                    }
                    else
                    {
                        if (CurrentMovementState == MovementStates.Run)
                        {
                            m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, RunSpeed, Time.deltaTime * 2);
                            AIAnimator.SetFloat("Speed", 1, 0.3f, Time.deltaTime);
                        }
                        else if (CurrentMovementState == MovementStates.Walk)
                        {
                            m_NavMeshAgent.speed = Mathf.Lerp(m_NavMeshAgent.speed, WalkSpeed, Time.deltaTime * 2);
                            AIAnimator.SetFloat("Speed", 0.5f, 0.3f, Time.deltaTime);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Moves our AI when using Root Motion
        /// </summary>
        void MoveAIRootMotion()
        {
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * m_NavMeshAgent.desiredVelocity;
            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //Handles all of the AI's movement and speed calculations for Root Motion
            if (AIAgentActive && m_NavMeshAgent.isOnNavMesh && MovementInitialized)
            {
                AIAnimator.SetFloat("Direction", angle * MovementTurningSensitivity, DirectionDampTime, Time.deltaTime);

                //Stops the AI during various conditions. Adding an offset to the remaining distance is needed because Root Motion needs a little extra room to stop.
                //With out this, the AI will get stuck between two states (moving and idling).
                var MovingTurnLimit = AIAnimator.GetFloat("Speed") <= 0.5f ? 80 : 120;

                if (m_NavMeshAgent.isStopped || RotateTowardsTarget || AIAnimator.GetBool("Hit") || DestinationAdjustedAngle > MovingTurnLimit || m_NavMeshAgent.remainingDistance + 0.2f <= m_NavMeshAgent.stoppingDistance || CanIdle())
                {
                    AIAnimator.SetFloat("Speed", 0, DecelerationDampTime, Time.deltaTime);
                    m_NavMeshAgent.speed = 0;
                }
                //To Move
                else if (!m_NavMeshAgent.isStopped && !AIAnimator.GetBool("Hit") && m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance && CanMove())
                {
                    ReachedDestination = false;
                    m_NavMeshAgent.speed = 0.025f;

                    //Force walk movement if getting close to an AI's destination. This helps prevent the AI from running into its target as Root Motion with NavMesh needs a bit of time to stop.
                    if (m_NavMeshAgent.remainingDistance < (m_NavMeshAgent.stoppingDistance + ForceWalkDistance))
                    {
                        AIAnimator.SetFloat("Speed", 0.5f, 0.3f, Time.deltaTime);
                    }
                    else
                    {
                        if (CurrentMovementState == MovementStates.Run)
                        {
                            AIAnimator.SetFloat("Speed", 1f, 0.35f, Time.deltaTime);
                        }
                        else if (CurrentMovementState == MovementStates.Walk)
                        {
                            AIAnimator.SetFloat("Speed", 0.5f, 0.15f, Time.deltaTime);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks varrious animation states to see if an AI can idle.
        /// </summary>
        bool CanIdle ()
        {
            return AnimationComponent.IsBackingUp || AnimationComponent.IsAttacking || AnimationComponent.IsGettingHit || AnimationComponent.IsEquipping || AnimationComponent.IsEmoting || AnimationComponent.IsBlocking || 
                AnimationComponent.IsTurning || AnimationComponent.IsStrafing || AnimationComponent.IsDodging || AnimationComponent.IsRecoiling || AnimationComponent.IsStunned || AnimationComponent.IsSwitchingWeapons;
        }

        /// <summary>
        /// Checks varrious animation states to see if an AI can move.
        /// </summary>
        bool CanMove ()
        {
            return !AnimationComponent.IsGettingHit && !AnimationComponent.IsAttacking && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsBackingUp && !AnimationComponent.IsEmoting && !AnimationComponent.IsEquipping && 
                !AnimationComponent.IsBlocking && !AnimationComponent.IsTurning && !AnimationComponent.IsStrafing && !AnimationComponent.IsDodging && !AnimationComponent.IsRecoiling && !AnimationComponent.IsStunned;
        }

        /// <summary>
        /// Checks varrious animation states to see if an AI can rotate while stationary.
        /// </summary>
        bool CanRotateStationary()
        {
            return !DefaultMovementPaused && !AnimationComponent.IsMoving && !AnimationComponent.IsBackingUp && !AnimationComponent.IsWarning && !AnimationComponent.IsBlocking && !AnimationComponent.IsGettingHit && 
                !AnimationComponent.IsRecoiling && !AnimationComponent.IsStunned && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping;
        }

        /// <summary>
        /// Checks varrious animation states to see if an AI can rotate while moving.
        /// </summary>
        bool CanRotateMoving()
        {
            return !DefaultMovementPaused && AnimationComponent.IsMoving && !AnimationComponent.IsTurning && !AnimationComponent.IsBackingUp && !AnimationComponent.IsAttacking && !AnimationComponent.IsIdling && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping && !AnimationComponent.IsStrafing;
        }

        /// <summary>
        /// Handles all of the rotations, and alignment of an AI, depending on its current state.
        /// </summary>
        void RotateAI()
        {
            if (AnimationComponent.IsDead || RotateTowardsTarget || m_NavMeshAgent.pathPending) return;

            //Rotate while stationary -  There's certain instances where steeringTarget, destination, or CurrentTarget need to be used.
            if (!AnimationComponent.IsMoving && !AnimationComponent.IsDead && !RotateTowardsTarget || DestinationAdjustedAngle > 110)
            {
                if (EmeraldComponent.CombatComponent.CombatState && EmeraldComponent.CombatTarget)
                {
                    if (CanRotateStationary())
                    {
                        if (CanReachTarget && !EmeraldComponent.AnimationComponent.IsStrafing && !BackupDelayActive || EmeraldComponent.BehaviorsComponent.CurrentBehaviorType == EmeraldBehaviors.BehaviorTypes.Coward)
                        {
                            if (m_NavMeshAgent.remainingDistance > 1 && !EmeraldComponent.DetectionComponent.TargetObstructed)
                            {
                                Vector3 Direction = new Vector3(m_NavMeshAgent.destination.x, 0, m_NavMeshAgent.destination.z) - new Vector3(transform.position.x, 0, transform.position.z);
                                UpdateRotations(Direction);
                            }
                            else if (m_NavMeshAgent.remainingDistance > 1 && EmeraldComponent.DetectionComponent.TargetObstructed)
                            {
                                Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
                                UpdateRotations(Direction);
                            }
                            else
                            {
                                Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
                                UpdateRotations(Direction);
                            }
                        }
                        else
                        {
                            Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
                            UpdateRotations(Direction);
                        }
                    }
                }
                else if (!EmeraldComponent.CombatComponent.CombatState && !AnimationComponent.IsGettingHit)
                {
                    //Once our AI has returned to its stantionary positon, adjust its position so it rotates to its original rotation.
                    if (ReturnToStationaryPosition && AIAgentActive && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
                    {
                        ReturnToStationaryPosition = false;
                    }

                    Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
                    UpdateRotations(Direction);
                }

                EmeraldComponent.AnimationComponent.CalculateTurnAnimations();
            }

            //Rotate while moving
            if (CanRotateMoving() && DestinationAdjustedAngle < 110)
            {
                Vector3 Direction = new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
                UpdateRotations(Direction);
            }
            //Rotate while backing up
            if (AnimationComponent.IsBackingUp)
            {
                Vector3 Direction = (new Vector3(m_NavMeshAgent.steeringTarget.x, 0, m_NavMeshAgent.steeringTarget.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized * -1;
                UpdateRotations(Direction);
            }
        }

        /// <summary>
        /// Instantly rotates the AI towards a target or position. If you are overriding an AI's rotation, it is important that this is used.
        /// </summary>
        public void InstantlyRotateTowards (Vector3 Target)
        {
            Vector3 DestinationDirection = new Vector3(Target.x, 0, Target.z) - new Vector3(transform.position.x, 0, transform.position.z);
            qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
            transform.rotation = qGround * qTarget;
        }

        /// <summary>
        /// Updates the AI's rotations and alignment based on the passed direction.
        /// </summary>
        public void UpdateRotations(Vector3 DirectionSource)
        {
            if (EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.CombatTarget != null && EmeraldComponent.CombatTarget.transform.localScale == Vector3.one * 0.003f || transform.localScale == Vector3.one * 0.003f) return;

            RayCastUpdateTimer += Time.deltaTime;

            if (RayCastUpdateTimer >= RayCastUpdateSeconds)
            {
                GetSurfaceNormal();
            }

            DestinationAdjustedAngle = Mathf.Abs(Vector3.Angle(transform.forward, DirectionSource)); //Get the angle between the current target and the AI.
            DestinationDirection = DirectionSource;
            Final *= transform.rotation;

            float CurrentMovingTurnSpeed = EmeraldComponent.CombatComponent.CombatState ? MovingTurnSpeedCombat : MovingTurnSpeedNonCombat;
            int AlignmentSpeed = EmeraldComponent.CombatComponent.CombatState ? CombatAlignmentSpeed :  NonCombatAlignmentSpeed;
            float StationaryTurningSpeed = EmeraldComponent.CombatComponent.CombatState ? StationaryTurningSpeedCombat : StationaryTurningSpeedNonCombat;
            AngleToTurn = EmeraldComponent.CombatComponent.CombatState ? CombatAngleToTurn : NonCombatAngleToTurn;

            if (DestinationDirection != Vector3.zero && !AnimationComponent.IsStrafing) qGround = Quaternion.Slerp(qGround, Quaternion.FromToRotation(Vector3.up, SurfaceNormal), Time.deltaTime * AlignmentSpeed);
            else if (DestinationDirection != Vector3.zero && AnimationComponent.IsStrafing) qGround = Quaternion.Slerp(qGround, Quaternion.FromToRotation(Vector3.up, SurfaceNormal), Time.deltaTime * AlignmentSpeed * 0.25f);

            if (!AnimationComponent.IsIdling && !AnimationComponent.IsBlocking && !AnimationComponent.IsStunned && !AnimationComponent.IsMoving && !AnimationComponent.IsBackingUp && !AnimationComponent.IsStrafing && !AnimationComponent.IsAttacking && !AnimationComponent.IsDodging && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping && !AnimationComponent.IsGettingHit && DestinationDirection != Vector3.zero) 
                qTarget = Quaternion.Slerp(qTarget * Quaternion.Inverse(Final), Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * StationaryTurningSpeed);
            else if (AnimationComponent.IsStrafing && AIAnimator.GetBool("Strafe Active") && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget, Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * 200);
            else if (AnimationComponent.IsMoving && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget, Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * CurrentMovingTurnSpeed);
            else if (AnimationComponent.IsBackingUp && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget, Quaternion.LookRotation(DestinationDirection, Vector3.up), Time.deltaTime * BackupTurningSpeed);
            else if (AnimationComponent.IsDodging && DestinationDirection != Vector3.zero) qTarget = Quaternion.RotateTowards(qTarget * Quaternion.Inverse(Final), Quaternion.LookRotation(ActionDirection, Vector3.up), Time.deltaTime * StationaryTurningSpeed);

            NormalRotation = Quaternion.FromToRotation(transform.up, SurfaceNormal) * transform.rotation;
            float AlignmentAngle = Quaternion.Angle(transform.rotation, NormalRotation);


            if (!AnimationComponent.IsIdling && !AnimationComponent.IsAttacking && !AnimationComponent.IsSwitchingWeapons && !AnimationComponent.IsEquipping && !AnimationComponent.IsGettingHit)
            {
                if (EmeraldComponent.CombatComponent.CombatState || AnimationComponent.IsTurning || m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, qGround * qTarget, Time.deltaTime * 10);
                }
            }
            else
            {
                if (EmeraldComponent.CombatComponent.CombatState || AnimationComponent.IsTurning || m_NavMeshAgent.remainingDistance > m_NavMeshAgent.stoppingDistance)
                {
                    Slope = Quaternion.Slerp(Slope, Quaternion.FromToRotation(transform.up, SurfaceNormal), Time.deltaTime * AlignmentSpeed);
                    transform.rotation = Quaternion.Slerp(transform.rotation, Slope * transform.rotation, Time.deltaTime * 10);
                }
            }
        }

        public void SetActionDirection ()
        {
            ActionDirection = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
        }

        /// <summary>
        /// Return the current surface normal by casting a ray from the center of the AI and getting the hit.normal amount.
        /// </summary>
        Vector3 GetSurfaceNormal()
        {
            if (AlignAIWithGround == YesOrNo.No)
                return Vector3.zero;

            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.25f, transform.position.z), -Vector3.up, out HitDown, 2f, AlignmentLayerMask))
            {
                if (HitDown.transform != this.transform)
                {
                    float m_MaxNormalAngle = MaxNormalAngle * 0.01f;
                    SurfaceNormal = HitDown.normal;
                    SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -m_MaxNormalAngle, m_MaxNormalAngle);
                    SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -m_MaxNormalAngle, m_MaxNormalAngle);
                    RayCastUpdateTimer = 0;
                }
            }

            return SurfaceNormal;
        }

        /// <summary>
        /// Aligns our AI to the current surface on Start
        /// </summary>
        void AlignOnStart()
        {
            GetSurfaceNormal();
            transform.rotation = Quaternion.FromToRotation(transform.up, SurfaceNormal) * transform.rotation;
        }

        /// <summary>
        /// Handles our AI's waypoints when using the Waypoint Wander Type
        /// </summary>
        void NextWaypoint()
        {
            if (WaypointsList.Count == 0)
                return;

            if (WaypointType != WaypointTypes.Random && WaypointsList.Count > 1 && !WaypointReverseActive && !m_NavMeshAgent.pathPending)
            {
                float WaypointStoppingDistance = WaypointIndex < WaypointsList.Count ? (m_NavMeshAgent.stoppingDistance + 1.25f) : StoppingDistance;

                if (m_NavMeshAgent.remainingDistance <= WaypointStoppingDistance)
                {
                    WaypointIndex++;

                    if (WaypointIndex == WaypointsList.Count)
                    {
                        WaypointIndex = 0;
                        OnReachedWaypoint?.Invoke();

                        if (WaypointType == WaypointTypes.Reverse)
                        {
                            m_NavMeshAgent.destination = WaypointsList[WaypointsList.Count - 1];
                            WaypointsList.Reverse();
                            m_NavMeshAgent.stoppingDistance = 10;
                            WaypointReverseActive = true;
                            LockTurning = false;
                            Invoke(nameof(ReverseDelay), 4);
                        }
                    }

                    if (m_NavMeshAgent.enabled && !WaypointReverseActive)
                    {
                        m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                    }
                }
            }
            else if (WaypointType == WaypointTypes.Random && WaypointsList.Count > 1)
            {
                m_LastWaypointIndex = WaypointIndex;

                do
                {
                    WaypointIndex = Random.Range(0, WaypointsList.Count);
                } while (m_LastWaypointIndex == WaypointIndex);

                if (m_NavMeshAgent.enabled)
                {
                    m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                }
            }

            //Check that our AI's path is valid.
            CheckPath(m_NavMeshAgent.destination);
            OnGeneratedWaypoint?.Invoke();
        }

        /// <summary>
        /// A built-in function that generates a wandering destination, based on the user set WanderType from the Movement Editor. 
        /// Users can use the Custom Wander Type to generate their own destinations.
        /// </summary>
        public void Wander()
        {
            if (DefaultMovementPaused) return;

            if (WanderType == WanderTypes.Dynamic && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance && !m_NavMeshAgent.pathPending && MovementInitialized && !AnimationComponent.IsSwitchingWeapons)
            {
                if (WaypointTimer == 0)
                {
                    if (Vector3.Distance(m_NavMeshAgent.destination, StartingDestination) > 0.25f)
                    {
                        ReachedDestination = true;
                        OnReachedWaypoint?.Invoke();
                    }
                }

                WaypointTimer += Time.deltaTime;

                if (WaypointTimer >= WaitTime)
                {
                    AIAnimator.SetBool("Idle Active", false);
                    GenerateDynamicWaypoint();
                    WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
                    WaypointTimer = 0;
                }
            }
            else if (WanderType == WanderTypes.Destination && m_NavMeshAgent.destination != SingleDestination && !ReachedDestination && !AnimationComponent.IsSwitchingWeapons)
            {
                if (m_NavMeshAgent.remainingDistance <= StoppingDistance && !m_NavMeshAgent.pathPending)
                {
                    ReachedDestination = true;
                    LockTurning = false;
                    OnReachedDestination?.Invoke();
                }
            }
            else if (WanderType == WanderTypes.Waypoints && !WaypointReverseActive && m_NavMeshAgent.destination != WaypointsList[WaypointIndex] && !AnimationComponent.IsSwitchingWeapons)
            {
                if (WaypointType == WaypointTypes.Random)
                {
                    if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance && MovementInitialized)
                    {
                        if (!ReachedWaypoint)
                        {
                            WaypointTimer = 0;
                            ReachedWaypoint = true;
                            OnReachedWaypoint?.Invoke();
                        }

                        WaypointTimer += Time.deltaTime;

                        if (WaypointTimer >= WaitTime)
                        {
                            ReachedWaypoint = false;
                            LockTurning = false;
                            AIAnimator.SetBool("Idle Active", false);
                            WaitTime = Random.Range((float)MinimumWaitTime, MaximumWaitTime + 1);
                            NextWaypoint();
                        }
                    }
                }
                else if (WaypointType != WaypointTypes.Random)
                {
                    if (m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance + 1.5f && MovementInitialized)
                    {
                        NextWaypoint();
                    }
                }
            }
            else if (WanderType == WanderTypes.Stationary && !AnimationComponent.IsMoving && !AnimationComponent.IsSwitchingWeapons)
            {
                StationaryIdleTimer += Time.deltaTime;
                if (StationaryIdleTimer >= StationaryIdleSeconds)
                {
                    EmeraldComponent.AnimationComponent.PlayIdleAnimation();
                    StationaryIdleSeconds = Random.Range(StationaryIdleSecondsMin, StationaryIdleSecondsMax);
                    StationaryIdleTimer = 0;
                }
            }
            else if (WanderType == WanderTypes.Custom && !ReachedDestination && m_NavMeshAgent.remainingDistance <= StoppingDistance && MovementInitialized && !AnimationComponent.IsSwitchingWeapons)
            {
                ReachedDestination = true;
                LockTurning = false;
                OnReachedDestination?.Invoke();
            }

            //Play an idle sound if the AI is not moving and the Idle Seconds have been met. 
            if (!AnimationComponent.IsMoving && EmeraldComponent.SoundComponent != null)
            {
                EmeraldComponent.SoundComponent.IdleSoundsUpdate();
            }

            //If the AI gets moved, for whatever reason, disable Idle Active so it can move back to its current destination.
            if (m_NavMeshAgent.remainingDistance > StoppingDistance + 0.1f && AIAnimator.GetBool("Idle Active"))
            {
                AIAnimator.SetBool("Idle Active", false);
            }

            ClearReturnToStart();
        }

        /// <summary>
        /// Clears the return to start progress. This happens after an AI gets too far away from its starting position.
        /// </summary>
        void ClearReturnToStart ()
        {
            if (ReturningToStartInProgress && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
            {
                ReturningToStartInProgress = false;
            }
        }

        /// <summary>
        /// Dynamically generate a waypoint (only useable with AI who are using the Dynamic Wander Type).
        /// </summary>
        void GenerateDynamicWaypoint()
        {
            LockTurning = false;
            float RandomDegree = Random.Range(0, 360);
            float posX = StartingDestination.x + WanderRadius * Mathf.Cos(RandomDegree);
            float posZ = StartingDestination.z + WanderRadius * Mathf.Sin(RandomDegree);
            Vector3 GeneratedDestination = new Vector3(posX, transform.position.y, posZ);

            RaycastHit HitDown;
            if (Physics.Raycast(new Vector3(GeneratedDestination.x, GeneratedDestination.y + 10, GeneratedDestination.z), -transform.up, out HitDown, 12, DynamicWanderLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (HitDown.transform != this.transform)
                {
                    if (Vector3.Angle(Vector3.up, HitDown.normal) <= MaxSlopeLimit)
                    {
                        GeneratedDestination = new Vector3(GeneratedDestination.x, HitDown.point.y, GeneratedDestination.z);
                        NavMeshHit DestinationHit;

                        if (NavMesh.SamplePosition(GeneratedDestination, out DestinationHit, 4, m_NavMeshAgent.areaMask))
                        {
                            AIAnimator.SetBool("Idle Active", false);
                            m_NavMeshAgent.SetDestination(DestinationHit.position);
                            OnGeneratedWaypoint?.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allows Companion and Pet AI to follow their Follow Target
        /// </summary>
        public void FollowCompanionTarget(float FollowingStoppingDistance)
        {
            if (DefaultMovementPaused) return;

            DistanceFromFollower = Vector3.Distance(EmeraldComponent.TargetToFollow.position, transform.position);
            if (DistanceFromFollower > FollowingStoppingDistance && !AnimationComponent.IsEmoting)
            {
                m_NavMeshAgent.destination = EmeraldComponent.TargetToFollow.position;
            }

            m_NavMeshAgent.stoppingDistance = FollowingStoppingDistance;
            CurrentMovementState = MovementStates.Run;
        }

        /// <summary>
        /// A custom update function for the EmeraldMovement called through the EmeraldAISystem script.
        /// </summary>
        public void MovementUpdate ()
        {
            if (m_NavMeshAgent.isOnOffMeshLink)
            {
                m_NavMeshAgent.speed = 1f;
                return;
            }

            CanReachTarget = CanReachTargetInternal();
            AIAgentActive = m_NavMeshAgent.enabled;

            if (EmeraldComponent.AnimationComponent.BusyBetweenStates) return;

            //Calculates an AI's movement speed when using Root Motion
            if (MovementType == MovementTypes.RootMotion && !AnimationComponent.IsDead) MoveAIRootMotion();

            //Calculates an AI's movement speed when using NavMesh
            else if (MovementType == MovementTypes.NavMeshDriven && !AnimationComponent.IsDead) MoveAINavMesh();

            RotateAI(); //Handles all of the rotations, and alignment of an AI, depending on its current state.
        }

        /// <summary>
        /// A built-in function that handles all combat related movement.
        /// </summary>
        public void CombatMovement()
        {
            if (AnimationComponent.IsBackingUp && !CanReachTarget) StopBackingUp(); //Stop backing up if the target cannot be reached.
            if (CanReachTarget) BackupState(); //Handles all backup related movement.

            if (!AnimationComponent.IsBackingUp && !AnimationComponent.IsEquipping && !AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.CombatComponent.DeathDelayActive && CanReachTarget && EmeraldComponent.m_NavMeshAgent.isOnNavMesh)
            {
                m_NavMeshAgent.destination = EmeraldComponent.CombatTarget.position;
            }

            if (!CanReachTarget && m_NavMeshAgent.isOnNavMesh)
            {
                //m_NavMeshAgent.destination = new Vector3(EmeraldComponent.CombatTarget.position.pixelX, transform.position.pixelY, EmeraldComponent.CombatTarget.position.z);
                m_NavMeshAgent.destination = transform.position;
            }
        }

        /// <summary>
        /// A built-in function that handles all flee related movement. This generates a new random destination, in the opposite direction of the current target, each time it's called.
        /// </summary>
        public void FleeMovement ()
        {
            Vector3 direction = (EmeraldComponent.CombatTarget.position - transform.position).normalized;
            Vector3 GeneratedDestination = transform.position + -direction * 30 + Random.insideUnitSphere * 5f;
            GeneratedDestination.y = transform.position.y;
            m_NavMeshAgent.destination = GeneratedDestination;

            //This finds the closest edge between the generated destination and the AI. If the AI gets too close to an edge, which can only happen when they are somewhere
            //where they can no longer generate new flee points, such as a corner, generate a new destination behind the current target to get out of the orcer.
            NavMeshHit hit;
            if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
            {
                //Stuck, generate a new position behind the current target
                if (Vector3.Distance(m_NavMeshAgent.destination, hit.position) < 3)
                {
                    GeneratedDestination = transform.position + direction * 30 + Random.insideUnitSphere * 5f;
                    GeneratedDestination.y = transform.position.y;
                    EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;
                }
                //Move to the currently generated flee point
                else
                {
                    EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;
                }
            }
        }

        /// <summary>
        /// Rotates the AI towards the specified target position using the AI's turning animations. The angle in which the AI will stop rotating is based off of an AI's Turning Angle set within the Emerald Movmement Component editor. 
        /// The AI will be unable to move during the duration of the turning process.
        /// </summary>
        /// <param name="TargetPosition">The position the AI will rotate towards. This can be an object, a player, or another AI.</param>
        public void RotateTowardsPosition(Vector3 TargetPosition)
        {
            EmeraldAPI.Movement.RotateTowardsPosition(EmeraldComponent, TargetPosition);
        }

        /// <summary>
        /// Handles all backup related movement and will only work if IsBackingUp is true.
        /// </summary>
        void BackupState()
        {
            CalculateBackupState(); //Check the distance between the target and the player. If the player gets too close, attempt a backup.

            if (EmeraldComponent.AIAnimator.GetBool("Walk Backwards") && !BackupDelayActive)
            {   
                if (CancelBackup()) { StopBackingUp(); BackupCoroutine = StartCoroutine(BackupDelay(1)); return; }
            }

            //Don'transform allow the backup movement to run if the AI is currently attacking, dead, or if this AI is teleporting.
            if (!AnimationComponent.IsBackingUp || AnimationComponent.IsDead) return;

            //Reset the attack trigger so an attack doesn'transform play while backing up
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");

            if (BackingUpTimer > 3 && !BackupDelayActive) { StopBackingUp(); BackupCoroutine = StartCoroutine(BackupDelay(2)); EmeraldCombatManager.GenerateClosestAttack(EmeraldComponent); return; };

            //Have a 2 second delay before backing up according to the AI's TooCloseDistance as it can trigger multiple times (which causes a few hiccups).
            if (!BackupDelayActive && !m_NavMeshAgent.pathPending && EmeraldComponent.CombatComponent.DistanceFromTarget > (EmeraldComponent.CombatComponent.TooCloseDistance - m_NavMeshAgent.radius)) { StopBackingUp(); BackupCoroutine = StartCoroutine(BackupDelay(2)); return; };

            //If the AI loses its target, or priority action happens, stop the AI's backup process.
            if (EmeraldComponent.CombatTarget == null || EmeraldComponent.CombatComponent.DeathDelayActive) { StopBackingUp();  return; };

            //Track the time while backing up as the AI will only backup for according to its BackingUpSeconds.
            BackingUpTimer += Time.deltaTime;

            //Generates a backup destination that's in the opposite direction of the AI's current target.
            if (EmeraldComponent.m_NavMeshAgent.hasPath)
                EmeraldComponent.m_NavMeshAgent.destination = GetBackupDestination();
        }

        IEnumerator BackupDelay(float DelayTime)
        {
            BackupDelayActive = true;
            yield return new WaitForSeconds(DelayTime);
            BackupDelayActive = false;
        }

        /// <summary>
        /// Stops the AI's backing up process and resets all of its settings.
        /// </summary>
        public void StopBackingUp()
        {
            EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
            BackingUpTimer = 0;
            BackupTimer = 0;
        }

        /// <summary>
        /// Calculates backing our AI up, when the appropriate conditions are met
        /// </summary>
        void CalculateBackupState()
        {
            if (EmeraldComponent.CombatTarget != null)
            {
                if (!BackupDelayActive)
                    BackupTimer += Time.deltaTime;

                if (CanBackup() && BackupTimer > 1f)
                {
                    BackupTimer = 0;

                    if (EmeraldComponent.CombatComponent.DistanceFromTarget <= EmeraldComponent.CombatComponent.TooCloseDistance && !DefaultMovementPaused)
                    {
                        if (DestinationAdjustedAngle <= 90 && !AIAnimator.GetBool("Blocking"))
                        {
                            //Do a quick raycast to see if behind the AI is clear before calling the backup state.
                            RaycastHit m_BackupHit = BackupRaycast();
                            if (m_BackupHit.collider != null && m_BackupHit.distance > StoppingDistance || m_BackupHit.collider == null)
                            {
                                EmeraldComponent.AIAnimator.SetBool("Walk Backwards", true);
                                EmeraldComponent.m_NavMeshAgent.destination = GetBackupDestination(); //Generates a backup destination that's in the opposite direction of the AI's current target.

                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates a backup destination that's in the opposite direction of the AI's current target.
        /// </summary>
        Vector3 GetBackupDestination()
        {
            Vector3 direction = (EmeraldComponent.CombatTarget.position - transform.position).normalized;
            Vector3 GeneratedDestination = ((direction * -1f) * 3f) + transform.position;
            GeneratedDestination.y = transform.position.y;
            return GeneratedDestination;
        }

        /// <summary>
        /// Returns a raycast that's cast behind the AI to look for obstructions when backing up.
        /// </summary>
        RaycastHit BackupRaycast()
        {
            RaycastHit HitBehind;
            if (Physics.Raycast(EmeraldComponent.DetectionComponent.HeadTransform.position, -transform.forward * 8 - transform.up * 2f, out HitBehind, 10, BackupLayerMask))
            {
                if (HitBehind.collider != null && HitBehind.collider.gameObject != this.gameObject && !HitBehind.transform.IsChildOf(this.transform))
                {
                    BackupDistance = HitBehind.distance;
                }
            }
            return HitBehind;
        }

        /// <summary>
        /// Checks the AI's settings to see if it can calculate a backup.
        /// </summary>
        bool CanBackup()
        {
            if (EmeraldComponent.DetectionComponent.TargetObstructed) return false;
            else if (EmeraldComponent.CombatComponent.DeathDelayActive || !MovementInitialized || !AIAgentActive) return false;
            else if (BackupDelayActive || AnimationComponent.IsSwitchingWeapons || AnimationComponent.IsMoving || AnimationComponent.IsEquipping || AnimationComponent.IsRecoiling || AnimationComponent.IsBackingUp || 
                AnimationComponent.IsAttacking || AnimationComponent.IsTurning || AnimationComponent.IsDodging || AnimationComponent.IsStrafing || AnimationComponent.IsGettingHit || AnimationComponent.IsBlocking) return false; //State Conditions
            else if (EmeraldComponent.AIAnimator.GetBool("Blocking") || EmeraldComponent.AIAnimator.GetBool("Hit")) return false;
            else return true; //If all conditions have passed, backup
        }

        /// <summary>
        /// Cancels backing up if a higher priority action or animation happens.
        /// </summary>
        bool CancelBackup ()
        {
            return DefaultMovementPaused || transform.localScale == Vector3.one * 0.003f || AnimationComponent.IsStunned || EmeraldComponent.AIAnimator.GetBool("Stunned Active") || AnimationComponent.IsTurning || AnimationComponent.IsStrafing || 
                AnimationComponent.IsBlocking || EmeraldComponent.AIAnimator.GetBool("Hit") || AnimationComponent.IsGettingHit || AnimationComponent.IsRecoiling || AnimationComponent.IsDodging || AnimationComponent.IsEquipping || AnimationComponent.IsSwitchingWeapons;
        }

        /// <summary>
        /// Returns whether or not a walk footstep sound can be played.
        /// </summary>
        public bool CanPlayWalkFootstepSound ()
        {
            return MovementType == MovementTypes.RootMotion && AIAnimator.GetFloat("Speed") > 0.05f && AIAnimator.GetFloat("Speed") <= 0.5f || 
                MovementType == MovementTypes.NavMeshDriven && EmeraldComponent.m_NavMeshAgent.velocity.magnitude > 0.05f && EmeraldComponent.m_NavMeshAgent.velocity.magnitude <= WalkSpeed + 0.25f || 
                AnimationComponent.IsTurning || AnimationComponent.IsStrafing || AnimationComponent.IsBackingUp || AnimationComponent.IsDodging || AnimationComponent.IsRecoiling || AnimationComponent.IsGettingHit;
        }

        /// <summary>
        /// Returns whether or not a run footstep sound can be played.
        /// </summary>
        public bool CanPlayRunFootstepSound()
        {
            return MovementType == MovementTypes.RootMotion && AIAnimator.GetFloat("Speed") > 0.5f || 
                MovementType == MovementTypes.NavMeshDriven && EmeraldComponent.m_NavMeshAgent.velocity.magnitude > WalkSpeed + 0.25f || 
                AnimationComponent.IsTurning || AnimationComponent.IsStrafing || AnimationComponent.IsBackingUp || AnimationComponent.IsDodging || AnimationComponent.IsRecoiling || AnimationComponent.IsGettingHit;
        }

        /// <summary>
        /// Sets the AI's current destination.
        /// </summary>
        /// <param name="Destination">The Vector3 destination the AI will make as its current destination.</param>
        public void SetDestination (Vector3 Destination)
        {
            m_NavMeshAgent.SetDestination(Destination);
        }

        /// <summary>
        /// Resets the AI's current path/destination.
        /// </summary>
        public void ResetPath ()
        {
            m_NavMeshAgent.ResetPath();
        }

        void ReverseDelay()
        {
            //If there's a target while in the process of this function, return as the stoppingDistance does not need to be set to 0.1.
            if (EmeraldComponent.CombatTarget != null)
            {
                WaypointReverseActive = false;
                return;
            }

            m_NavMeshAgent.stoppingDistance = 0.1f;
            WaypointReverseActive = false;
        }

        /// <summary>
        /// Resets the movement settings back to their defaults.
        /// </summary>
        public void DefaultMovement ()
        {
            StopBackingUp(); //Reset the backing up settings and state.

            if (!ReturningToStartInProgress)
                CurrentMovementState = StartingMovementState;

            BackingUpTimer = 0;

            //Resets the AI's stopping distances.
            if (WanderType != WanderTypes.Waypoints) m_NavMeshAgent.stoppingDistance = StoppingDistance;
            else m_NavMeshAgent.stoppingDistance = 0.1f;

            //Return the AI to its starting destination to continue wandering based on it WanderType.
            ReturnToStartingDestination();
        }

        /// <summary>
        /// Called when an AI is returning to its starting destination.
        /// </summary>
        void ReturnToStartingDestination ()
        {
            if (EmeraldComponent.TargetToFollow) return; //Don'transform set the AI's wandering position if it has a follow target

            if (WanderType == WanderTypes.Dynamic)
            {
                GenerateDynamicWaypoint();
            }
            else if (WanderType == WanderTypes.Stationary && EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.destination = StartingDestination;
                ReturnToStationaryPosition = true;
            }
            else if (WanderType == WanderTypes.Waypoints && EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                if (WaypointType != WaypointTypes.Random)
                    EmeraldComponent.m_NavMeshAgent.destination = WaypointsList[WaypointIndex];
                else
                    WaypointTimer = 1;
            }
            else if (WanderType == WanderTypes.Destination && EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.destination = SingleDestination;
                ReturnToStationaryPosition = true;
            }
        }

        /// <summary>
        /// Calculates the path to the current target to see if it's accessible.
        /// </summary>
        public bool CanReachTargetInternal()
        {
            if (EmeraldComponent.CombatTarget == null || !m_NavMeshAgent.enabled || !m_NavMeshAgent.isOnNavMesh)
                return false;

            CalculatePathTimer += Time.deltaTime;

            if (CalculatePathTimer >= 0.5f)
            {
                m_NavMeshAgent.CalculatePath(EmeraldComponent.CombatTarget.position, AIPath);
                CalculatePathTimer = 0;
            }

            return AIPath.status == NavMeshPathStatus.PathComplete;
        }

        public void EnableReturnToStart()
        {
            ReturningToStartInProgress = true;
            CurrentMovementState = MovementStates.Run;
        }

        /// <summary>
        /// Changes the AI's Wander Type
        /// </summary>
        public void ChangeWanderType(WanderTypes NewWanderType)
        {
            WanderType = NewWanderType;
        }

        /// <summary>
        /// Resets the wander settings for when a new custom destination is set. 
        /// </summary>
        public void ResetWanderSettings()
        {
            LockTurning = false;
            StationaryIdleTimer = 0;
            WaypointTimer = 0;
            ReachedWaypoint = false;
            ReachedDestination = false;
        }
    }
}