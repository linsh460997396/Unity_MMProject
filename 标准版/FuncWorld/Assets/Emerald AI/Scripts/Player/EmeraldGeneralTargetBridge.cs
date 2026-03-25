using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [RequireComponent(typeof(TargetPositionModifier))]
    [RequireComponent(typeof(FactionExtension))]
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/getting-started/setting-up-a-player-with-emerald-ai")]
    public class EmeraldGeneralTargetBridge : MonoBehaviour, IDamageable, ICombat
    {
        public int StartingHealth = 50;
        public bool Immortal = false;
        public UnityEvent OnTakeDamage;
        public UnityEvent OnDeath;
        
        public bool DebugLogDeath = true;
        public bool HideSettingsFoldout;
        public bool HealthSettingsFoldout = true;

        public int StartHealth { get => StartingHealth; set => StartingHealth = value; }
        [field: SerializeField] public int Health { get; set; }
        [field: SerializeField] public List<string> ActiveEffects { get; set; }

        TargetPositionModifier m_TargetPositionModifier;
        Collider m_Collider;

        void Start()
        {
            Health = StartingHealth;
            m_TargetPositionModifier = GetComponent<TargetPositionModifier>();
            m_Collider = GetComponent<Collider>();
        }

        public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
        {
            DefaultDamage(DamageAmount, AttackerTransform);

            //Creates damage text on the target's position, if enabled.
            if (CombatTextSystem.Instance != null) CombatTextSystem.Instance.CreateCombatText(DamageAmount, DamagePosition(), CriticalHit, false, false);
        }

        void OnEnable ()
        {
            if (Health <= 0) ResetTarget();
        }

        /// <summary>
        /// Used for referencing the damage position for this object when an AI takes damage from external sources.
        /// </summary>
        public Vector3 DamagePosition()
        {
            if (m_TargetPositionModifier != null)
                return new Vector3(m_TargetPositionModifier.TransformSource.position.x, m_TargetPositionModifier.TransformSource.position.y + m_TargetPositionModifier.PositionModifier, m_TargetPositionModifier.TransformSource.position.z);
            else
                return transform.position + new Vector3(0, transform.localScale.y / 2, 0);
        }

        void DefaultDamage(int DamageAmount, Transform Target)
        {
            if (Immortal) return;

            Health -= DamageAmount;
            OnTakeDamage.Invoke();

            if (Health <= 0)
            {
                if (DebugLogDeath)
                    Debug.Log("The Non-AI Target has died.");

                if (m_Collider != null) m_Collider.enabled = false;
                gameObject.layer = 0;
                gameObject.tag = "Untagged";
                OnDeath.Invoke();
            }
        }

        /// <summary>
        /// Resets this Non-AI target to its default settings before it was killed. This includes health, layer, and tag.
        /// </summary>
        public void ResetTarget ()
        {
            Health = StartingHealth;
            if (m_Collider != null) m_Collider.enabled = true;
        }

        public Transform TargetTransform()
        {
            return transform;
        }

        /// <summary>
        /// Used for detecting when this target is attacking.
        /// </summary>
        public bool IsAttacking()
        {
            return false;
        }

        /// <summary>
        /// Used for detecting when this target is blocking.
        /// </summary>
        public bool IsBlocking()
        {
            return false;
        }

        /// <summary>
        /// Used for detecting when this target is dodging.
        /// </summary>
        public bool IsDodging()
        {
            return false;
        }

        public void TriggerStun(float StunLength)
        {
            //Custom trigger mechanics can go here, but are not required
        }
    }
}
