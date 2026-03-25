using UnityEngine;
using UnityEngine.Events;

namespace EmeraldAI
{
    /// <summary>
    /// Allows UnityEvents to work through Emerald's various usable callbacks.
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/events-component")]
    public class EmeraldEvents : MonoBehaviour
    {
        #region Events Variables
        public UnityEvent OnEnabledEvent;
        public UnityEvent OnStartEvent;
        public UnityEvent OnEnemyTargetDetectedEvent;
        public UnityEvent OnStartCombatEvent;
        public UnityEvent OnEndCombatEvent;
        public UnityEvent OnAttackStartEvent;
        public UnityEvent OnAttackEndEvent;
        public UnityEvent OnTakeDamageEvent;
        public UnityEvent OnTakeCritDamageEvent;
        public UnityEvent OnDoDamageEvent;
        public UnityEvent OnDoCritDamageEvent;
        public UnityEvent OnKilledTargetEvent;
        public UnityEvent OnDeathEvent;
        public UnityEvent OnReachedDestinationEvent;
        public UnityEvent OnReachedWaypointEvent;
        public UnityEvent OnGeneratedWaypointEvent;
        public UnityEvent OnPlayerDetectedEvent;
        public UnityEvent OnFleeEvent;
        EmeraldSystem EmeraldComponent;
        #endregion

        #region Editor Variables
        public bool HideSettingsFoldout;
        public bool GeneralEventsFoldout;
        public bool CombatEventsFoldout;
        #endregion

        void Start()
        {
            OnStartEvent.Invoke(); //Invoke the OnStartEvent
            InitializeEvents();
        }

        /// <summary>
        /// Initialize the Events Component.
        /// </summary>
        void InitializeEvents ()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            EmeraldComponent.MovementComponent.OnReachedDestination += OnReachedDestinationEvent.Invoke; //Subscribe the OnReachedDestinationEvent to the OnReachedDestination delegate.
            EmeraldComponent.MovementComponent.OnReachedWaypoint += OnReachedWaypointEvent.Invoke; //Subscribe the OnReachedWaypointEvent to the OnReachedWaypoint delegate.
            EmeraldComponent.MovementComponent.OnGeneratedWaypoint += OnGeneratedWaypointEvent.Invoke; //Subscribe the OnGeneratedWaypointEvent to the OnGeneratedWaypoint delegate.
            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += OnEnemyTargetDetectedEvent.Invoke; //Subscribe the OnEnemyTargetDetectedEvent to the OnEnemyTargetDetected delegate.
            EmeraldComponent.DetectionComponent.OnPlayerDetected += OnPlayerDetectedEvent.Invoke; //Subscribe the OnPlayerDetectedEvent to the OnPlayerDetected delegate.
            EmeraldComponent.HealthComponent.OnDeath += OnDeathEvent.Invoke; //Subscribe the OnDeathEvent to the OnDeath delegate.
            EmeraldComponent.HealthComponent.OnTakeDamage += OnTakeDamageEvent.Invoke; //Subscribe the OnTakeDamageEvent to the OnTakeDamage delegate.
            EmeraldComponent.HealthComponent.OnTakeCritDamage += OnTakeCritDamageEvent.Invoke; //Subscribe the OnTakeCritDamageEvent to the OnTakeCritDamage delegate.
            EmeraldComponent.CombatComponent.OnKilledTarget += OnKilledTargetEvent.Invoke; //Subscribe the OnKilledTargetEvent to the OnKilledTarget delegate.
            EmeraldComponent.AnimationComponent.OnStartAttackAnimation += OnAttackStartEvent.Invoke; //Subscribe the OnAttackStartEvent to the OnAttackStartEvent delegate.
            EmeraldComponent.AnimationComponent.OnEndAttackAnimation += OnAttackEndEvent.Invoke; //Subscribe the OnAttackEndEvent to the OnAttackEndEvent delegate.
            EmeraldComponent.CombatComponent.OnDoDamage += OnDoDamageEvent.Invoke; //Subscribe the OnDoDamageEvent to the OnDoDamage delegate.
            EmeraldComponent.CombatComponent.OnDoCritDamage += OnDoCritDamageEvent.Invoke; //Subscribe the OnDoCritDamageEvent to the OnDoCritDamage delegate.
            EmeraldComponent.CombatComponent.OnStartCombat += OnStartCombatEvent.Invoke; //Subscribe the OnStartCombatEvent to the OnStartCombat delegate.
            EmeraldComponent.CombatComponent.OnEndCombat += OnEndCombatEvent.Invoke; //Subscribe the OnEndCombatEvent to the OnEndCombat delegate.
            EmeraldComponent.BehaviorsComponent.OnFlee += OnFleeEvent.Invoke; //Subscribe the OnFleeEvent to the OnFlee delegate.
        }

        void OnEnable()
        {
            OnEnabledEvent.Invoke(); //Invoke the OnEnabledEvent.
        }
    }
}