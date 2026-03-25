using UnityEngine;

namespace EmeraldAI.Utility
{
    public class EmeraldDamageOverTime : MonoBehaviour
    {
        float DamageTimer;
        float ActiveLengthTimer;
        EmeraldAbilityObject m_AbilityObject;
        AudioSource m_AudioSource;
        Transform m_TargetTransform;
        Transform m_AttackerTransform;

        float TickRate;
        int DamagePerTick;
        float DamageOverTimeLength;
        GameObject DamageOverTimeEffect;
        float OverTimeEffectTimeOutSeconds;
        AudioClip DamageOverTimeSound;

        void Start()
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Initiaizes the damage over time component.
        /// </summary>
        public void Initialize(EmeraldAbilityObject AbilityObject, AbilityData.DamageData DamageData, Transform TargetTransform, Transform AttackerTransform)
        {
            DamageTimer = 0;
            ActiveLengthTimer = 0;
            m_TargetTransform = TargetTransform;
            m_AttackerTransform = AttackerTransform;
            m_AbilityObject = AbilityObject;

            TickRate = DamageData.DamageOverTimeSettings.TickRate;
            DamagePerTick = DamageData.DamageOverTimeSettings.DamagePerTick;
            DamageOverTimeEffect = DamageData.DamageOverTimeSettings.DamageOverTimeEffect;
            DamageOverTimeLength = DamageData.DamageOverTimeSettings.DamageOverTimeLength;
            OverTimeEffectTimeOutSeconds = DamageData.DamageOverTimeSettings.OverTimeEffectTimeOutSeconds;
            if (DamageData.DamageOverTimeSettings.OverTimeSounds.Count > 0) DamageOverTimeSound = DamageData.DamageOverTimeSettings.OverTimeSounds[Random.Range(0, DamageData.DamageOverTimeSettings.OverTimeSounds.Count)];
        }

        /// <summary>
        /// Updates the EmeraldAIDamageOverTime timers.
        /// </summary>
        void Update()
        {
            DamageTimer += Time.deltaTime;
            ActiveLengthTimer += Time.deltaTime;

            if (ActiveLengthTimer >= DamageOverTimeLength + 0.05f)
            {
                IDamageableHelper.RemoveAbilityActiveEffect(m_TargetTransform.gameObject, m_AbilityObject); //Removes the current projectile from the target's active effects list.

                if (!m_AudioSource.isPlaying)
                {
                    EmeraldObjectPool.Despawn(gameObject); //Despawn the EmeraldAIDamageOverTime component.
                }
            }

            if (DamageTimer >= TickRate && ActiveLengthTimer <= DamageOverTimeLength + 0.05f)
            {
                DamageTarget(); //Damage the target once the DamageTimer is equal to the AbilityDamageIncrement.
                DamageTimer = 0;
            }
        }

        /// <summary>
        /// Damage the target through its IDamageable using the current Ability Object data.
        /// </summary>
        void DamageTarget ()
        {
            var m_IDamageable = m_TargetTransform.GetComponent<IDamageable>();
            var m_ICombat = m_TargetTransform.GetComponent<ICombat>();

            if (m_IDamageable != null && m_ICombat != null)
            {
                //Damage the m_TargetTransform through its IDamageable component.
                m_IDamageable.Damage(DamagePerTick, m_AttackerTransform, 50, false);

                //Spawn the DamageOverTimeEffect based on the m_IDamageable position.
                if (DamageOverTimeEffect != null)
                {
                    EmeraldObjectPool.SpawnEffect(DamageOverTimeEffect, m_ICombat.DamagePosition(), Quaternion.identity, OverTimeEffectTimeOutSeconds);
                }

                //Play the DamageOverTimeSound.
                if (DamageOverTimeSound != null)
                {
                    m_AudioSource.PlayOneShot(DamageOverTimeSound);
                }
            }
        }
    }
}