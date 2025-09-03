using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [RequireComponent(typeof(TargetPositionModifier))]
    [RequireComponent(typeof(FactionExtension))]
    public class EmeraldPlayerBridge : MonoBehaviour, IDamageable, ICombat
    {
        public int StartHealth { get; set; } = 100;
        public int Health { get; set; } = 100;

        [HideInInspector] public bool Immortal = false;

        [Space(5)]
        public UnityEvent OnTakeDamage;
        public UnityEvent OnDeath;

        public List<string> ActiveEffects { get; set; } = new List<string>();

        TargetPositionModifier m_TargetPositionModifier;
        Collider m_Collider;
        bool m_CriticalHit;

        public virtual void Awake()
        {
            m_TargetPositionModifier = GetComponent<TargetPositionModifier>();
            m_Collider = GetComponent<Collider>();
        }

        public virtual void Start()
        {
            Health = StartHealth;

            //You should set your StartHealth and Health variables equal to that of your character controller here.
        }

        /// <summary>
        /// Called internally through the IDamageable interface.
        /// </summary>
        public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
        {
            m_CriticalHit = CriticalHit;
            DamageCharacterController(DamageAmount, AttackerTransform);
        }

        void OnEnable()
        {
            if (Health <= 0) ResetTarget();
        }

        /// <summary>
        /// Displays damage text based on the damage passed. This is a separate function so users can used it when need (by passing block, dodge, etc.).
        /// </summary>
        public virtual void DisplayDamageText(int DamageAmount)
        {
            //Creates damage text on the target's position, if enabled.
            if (CombatTextSystem.Instance != null) CombatTextSystem.Instance.CreateCombatText(DamageAmount, DamagePosition(), m_CriticalHit, false, false);
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

        public virtual void DamageCharacterController(int DamageAmount, Transform Target)
        {
            if (Immortal) return;

            //The code for damaging your character controller should go here.

            OnTakeDamage.Invoke();

            //You should set the Health variables equal to that of your character controller after it was damaged here.

            if (Health <= 0)
            {
                //Controls what happens when your player dies.

                if (m_Collider != null) m_Collider.enabled = false;
                OnDeath.Invoke();
            }
        }

        /// <summary>
        /// Resets this Non-AI target to its default settings before it was killed. This includes health, layer, and tag.
        /// </summary>
        public void ResetTarget()
        {
            Health = StartHealth;
            if (m_Collider != null) m_Collider.enabled = true;
        }

        public virtual Transform TargetTransform()
        {
            return transform;
        }

        /// <summary>
        /// Used for detecting when this target is attacking.
        /// </summary>
        public virtual bool IsAttacking()
        {
            return false;
        }

        /// <summary>
        /// Used for detecting when this target is blocking.
        /// </summary>
        public virtual bool IsBlocking()
        {
            return false;
        }

        /// <summary>
        /// Used for detecting when this target is dodging.
        /// </summary>
        public virtual bool IsDodging()
        {
            return false;
        }

        public virtual void TriggerStun(float StunLength)
        {
            //Custom trigger mechanics can go here, but are not required
        }
    }
}
