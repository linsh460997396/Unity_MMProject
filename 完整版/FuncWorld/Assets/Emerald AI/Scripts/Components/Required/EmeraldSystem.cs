using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using EmeraldAI.Utility;
using EmeraldAI.SoundDetection;

namespace EmeraldAI
{
    /// <summary>
    /// Updates most required Emerald Components so they are only using 1 update function. This also allows all components to easily be accessed from one source.
    /// </summary>
    #region Required Components
    [RequireComponent(typeof(EmeraldAnimation))]
    [RequireComponent(typeof(EmeraldDetection))]
    [RequireComponent(typeof(EmeraldSounds))]
    [RequireComponent(typeof(EmeraldCombat))]
    [RequireComponent(typeof(EmeraldBehaviors))]
    [RequireComponent(typeof(EmeraldMovement))]
    [RequireComponent(typeof(EmeraldHealth))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AudioSource))]
    [SelectionBase]
    #endregion
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/")]
    public class EmeraldSystem : MonoBehaviour
    {
        #region Target Info
        //Since these are evenly used across multiple components, these are kept in the main Emerald System script.
        public Transform CombatTarget;
        [HideInInspector] public Transform TargetToFollow;
        [HideInInspector] public Transform LookAtTarget;
        [HideInInspector] [SerializeField] public CurrentTargetInfoClass CurrentTargetInfo = null;
        [System.Serializable]
        public class CurrentTargetInfoClass
        {
            public Transform TargetSource;
            public IDamageable CurrentIDamageable;
            public ICombat CurrentICombat;
        }
        #endregion

        #region Internal Components
        public static GameObject ObjectPool;
        public static GameObject CombatTextSystemObject;
        [HideInInspector] public NavMeshAgent m_NavMeshAgent;
        [HideInInspector] public BoxCollider AIBoxCollider;
        [HideInInspector] public Animator AIAnimator;
        [HideInInspector] public float TimeSinceEnabled;
        #endregion

        #region AI Components
        [HideInInspector] public EmeraldDetection DetectionComponent;
        [HideInInspector] public EmeraldBehaviors BehaviorsComponent;
        [HideInInspector] public EmeraldMovement MovementComponent;
        [HideInInspector] public EmeraldAnimation AnimationComponent;
        [HideInInspector] public EmeraldCombat CombatComponent;
        [HideInInspector] public EmeraldSounds SoundComponent;
        [HideInInspector] public EmeraldHealth HealthComponent;
        [HideInInspector] public EmeraldOptimization OptimizationComponent;
        [HideInInspector] public EmeraldInverseKinematics InverseKinematicsComponent;
        [HideInInspector] public EmeraldEvents EventsComponent;
        [HideInInspector] public EmeraldDebugger DebuggerComponent;
        [HideInInspector] public EmeraldUI UIComponent;
        [HideInInspector] public EmeraldItems ItemsComponent;
        [HideInInspector] public EmeraldSoundDetector SoundDetectorComponent;
        [HideInInspector] public TargetPositionModifier TPMComponent;
        [HideInInspector] public LocationBasedDamage LBDComponent;
        #endregion

        //Initialize Emerald AI and its components
        void Awake()
        {
            MovementComponent = GetComponent<EmeraldMovement>();
            AnimationComponent = GetComponent<EmeraldAnimation>();
            SoundComponent = GetComponent<EmeraldSounds>();          
            DetectionComponent = GetComponent<EmeraldDetection>();
            BehaviorsComponent = GetComponent<EmeraldBehaviors>();          
            CombatComponent = GetComponent<EmeraldCombat>();
            HealthComponent = GetComponent<EmeraldHealth>();
            OptimizationComponent = GetComponent<EmeraldOptimization>();
            EventsComponent = GetComponent<EmeraldEvents>();
            DebuggerComponent = GetComponent<EmeraldDebugger>();
            UIComponent = GetComponent<EmeraldUI>();
            ItemsComponent = GetComponent<EmeraldItems>();
            SoundDetectorComponent = GetComponent<EmeraldSoundDetector>();
            InverseKinematicsComponent = GetComponent<EmeraldInverseKinematics>();
            TPMComponent = GetComponent<TargetPositionModifier>();
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            AIBoxCollider = GetComponent<BoxCollider>();
            AIAnimator = GetComponent<Animator>();
            InitializeEmeraldObjectPool();
            InitializeCombatText();
        }

        void OnEnable()
        {
            TimeSinceEnabled = Time.time; //Time stamp when the AI was enabled.

            //When the AI is enabled, and it has been killed, reset the AI to its default settings. 
            //This is intended for being used with Object Pooling or spawning systems such as Crux.
            if (AnimationComponent.IsDead)
            {
                ResetAI();
            }
        }

        /// <summary>
        /// Initialize the Emerald Object Pool. The ObjectPool is a static variable so it's only done once.
        /// </summary>
        void InitializeEmeraldObjectPool()
        {
            if (EmeraldSystem.ObjectPool == null)
            {
                EmeraldSystem.ObjectPool = new GameObject();
                EmeraldSystem.ObjectPool.name = "Emerald AI Pool";
                EmeraldObjectPool.Clear();
            }
        }

        /// <summary>
        /// Initialize the Emerald Combat Text System. The CombatTextSystemObject is a static variable so it's only done once.
        /// </summary>
        void InitializeCombatText()
        {
            if (EmeraldSystem.CombatTextSystemObject == null)
            {
                GameObject m_CombatTextSystem = Instantiate((GameObject)Resources.Load("Combat Text System") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextSystem.name = "Combat Text System";
                GameObject m_CombatTextCanvas = Instantiate((GameObject)Resources.Load("Combat Text Canvas") as GameObject, Vector3.zero, Quaternion.identity);
                m_CombatTextCanvas.name = "Combat Text Canvas";
                EmeraldSystem.CombatTextSystemObject = m_CombatTextCanvas;
                CombatTextSystem.Instance.CombatTextCanvas = m_CombatTextCanvas;
                CombatTextSystem.Instance.Initialize();
            }
        }

        /// <summary>
        /// Update all scripts through the EmeraldSystem's update function.
        /// </summary>
        void Update()
        {
            if (HealthComponent.CurrentHealth <= 0) return;

            AnimationComponent.AnimationUpdate(); //A custom update function for the EmeraldAnimation called through the EmeraldAISystem script.
            MovementComponent.MovementUpdate(); //A custom update function for the EmeraldMovement called through the EmeraldAISystem script.
            BehaviorsComponent.BehaviorUpdate(); //A custom update function for the EmeraldBehaviors script called through the EmeraldAISystem script.
            DetectionComponent.DetectionUpdate(); //A custom update function for the EmeraldDetection script called through the EmeraldAISystem script.
            CombatComponent.CombatUpdate(); //A custom update function for the EmeraldCombat script called through the EmeraldAISystem script.
            if (DebuggerComponent) DebuggerComponent.DebuggerUpdate(); //A custom update function for the EmeraldDebugger script called through the EmeraldAISystem script.
        }

        /// <summary>
        /// Resets an AI to its default state. This is useful if an AI is being respawned. 
        /// </summary>
        public void ResetAI()
        {
            EmeraldAPI.Combat.ResetAI(this);
        }
    }
}