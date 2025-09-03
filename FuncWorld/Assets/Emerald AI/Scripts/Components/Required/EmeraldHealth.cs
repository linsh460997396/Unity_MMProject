using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    /// <summary>
    /// Handles how an receives damage and track health. The Damage function is called through the IDamageable interface script.
    /// </summary>
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/health-component")]
    public class EmeraldHealth : MonoBehaviour, IDamageable
    {
        #region Health variables       
        public int CurrentHealth = 50;
        public int StartingHealth = 50;
        public int HealRate = 0;
        public bool AttachHitEffects = false;
        public bool Immortal = false;
        public List<string> CurrentActiveEffects;
        public bool HitEffectFoldout;
        public YesOrNo UseHitEffect = YesOrNo.No;
        public Vector3 HitEffectPosOffset;
        public float HitEffectTimeoutSeconds = 3f;
        public List<GameObject> HitEffectsList = new List<GameObject>();
        public delegate void DamageHandler();
        public event DamageHandler OnTakeDamage;
        public delegate void TakeCritDamageHandler();
        public event TakeCritDamageHandler OnTakeCritDamage;
        public delegate void AnyDamageHandler();
        public event DamageHandler OnTakeAnyDamage;
        public delegate void BlockHandler();
        public event BlockHandler OnBlock;
        public delegate void DodgeHandler();
        public event DodgeHandler OnDodge;
        public delegate void DeathHandler();
        public event DeathHandler OnDeath;
        public delegate void HealRateTickHandler();
        public event HealRateTickHandler OnHealRateTick;
        public delegate void OnHealingReceivedHandler();
        public event OnHealingReceivedHandler OnHealingReceived;
        public delegate void HealthChangeHandler();
        public event HealthChangeHandler OnHealthChange;
        EmeraldSystem EmeraldComponent;

        public List<string> ActiveEffects { get => CurrentActiveEffects; set => CurrentActiveEffects = value; }
        public int Health { get => CurrentHealth; set => CurrentHealth = value; }
        public int StartHealth { get => StartingHealth; set => StartingHealth = value; }
        #endregion

        #region Editor Variables
        public bool HideSettingsFoldout;
        public bool HealthFoldout;
        #endregion

        void Start ()
        {
            CurrentHealth = StartingHealth;
            EmeraldComponent = GetComponent<EmeraldSystem>();
            EmeraldComponent.CombatComponent.OnExitCombat += RecoverHealth; //Subscribe to the OnExitCombat event for RecoverHealth
        }

        /// <summary>
        /// Damages the AI and allows it to block and mitigate damage, if enabled. To use the ragdoll feature, all
        /// parameters need to be used where AttackerTransform is the current attacker.
        /// </summary>
        /// <param name="DamageAmount">Amount of damage caused during attack.</param>
        /// <param name="AttackerTransform">The transform of the current attacker.</param>
        /// <param name="RagdollForce">The amount of force to apply to this AI when they die. (Use Ragdoll must be enabled on this AI)</param>
        public void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false)
        {
            if (EmeraldComponent.AnimationComponent.IsDead || transform.localScale == Vector3.one * 0.003f || AttackerTransform && AttackerTransform == EmeraldComponent.TargetToFollow || AttackerTransform && EmeraldComponent.DetectionComponent.GetTargetFactionRelation(AttackerTransform) == "Friendly") return;

            //Check for an attacker if there's no current target.
            if (AttackerTransform != null) CheckForAttacker(AttackerTransform);

            //Cache the reference to the newest/current attacker.
            EmeraldComponent.CombatComponent.LastAttacker = AttackerTransform;

            //Get the angle from the current attacker to determine if the incoming hit can be blocked or dodge.
            float AttackerAngle = EmeraldCombatManager.TransformAngle(EmeraldComponent, AttackerTransform);

            //Check to see if the current attack is being blocked
            bool Blocked = (EmeraldComponent.AnimationComponent.IsBlocking && AttackerAngle <= EmeraldComponent.CombatComponent.MaxMitigationAngle && EmeraldComponent.AIAnimator.GetBool("Blocking"));

            //Check to see if the current attack is being dodged
            bool Dodged = (EmeraldComponent.AnimationComponent.IsDodging && AttackerAngle <= EmeraldComponent.CombatComponent.MaxMitigationAngle);

            //Set the base calculated damage equal to DamageAmount. If it is dodged or blocked, the amount will be adjusted below.
            int CalculatedDamage = DamageAmount;

            if (Blocked) CalculatedDamage = Mathf.FloorToInt(Mathf.Abs((DamageAmount * ((EmeraldComponent.CombatComponent.MitigationAmount) * 0.01f)) - DamageAmount)); //Mitigate the damage from blocking
            if (Dodged) CalculatedDamage = Mathf.FloorToInt(Mathf.Abs((DamageAmount * ((EmeraldComponent.CombatComponent.MitigationAmount) * 0.01f)) - DamageAmount)); //Mitigate the damage from dodging 

            //Don'transform reduce an AI's health if Immortal is enabled
            if (!Immortal)
                Health -= CalculatedDamage;

            //Display the damage dealt through the Combat Text System, given that it's enabled.
            if (CalculatedDamage > 0) CombatTextSystem.Instance.CreateCombatTextAI(CalculatedDamage, EmeraldComponent.CombatComponent.DamagePosition(), CriticalHit, false);

            //In order to have the most reliable On Do Damage events, simply invoke the attacker's OnDoDoamage callback through a public function, given it is an Emerald AI agent.
            if (AttackerTransform != null)
            {
                EmeraldSystem AttackEmeraldComponent = AttackerTransform.GetComponent<EmeraldSystem>();
                if (AttackEmeraldComponent != null) AttackEmeraldComponent.CombatComponent.InvokeDoDamage();
                if (AttackEmeraldComponent != null && CriticalHit) AttackEmeraldComponent.CombatComponent.InvokeDoCritDamage();
            }

            //Invoke the damage delegates
            if (!CriticalHit && CalculatedDamage > 0) OnTakeDamage?.Invoke();
            else if (CriticalHit && CalculatedDamage > 0) OnTakeCritDamage?.Invoke();
            OnTakeAnyDamage?.Invoke();

            //Create hit effect, if it's enabled, the AI is not blocking or dodging, and the damage is greater than 0.
            if (!Blocked && !Dodged && CalculatedDamage > 0) CreateHitEffect();
            
            if (Blocked) OnBlock?.Invoke(); //Invoke the block delegate
            else if (Dodged) OnDodge?.Invoke(); //Invoke the dodge delegate

            //The AI has died, initialize its death state.
            if (Health <= 0 && !EmeraldComponent.AnimationComponent.IsDead)
            {
                EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount = RagdollForce;
                Health = 0;
                Death();
            }
        }

        /// <summary>
        /// Called when an AI receives damage, but has no current target (typically from an unseen attacker). When this happenss, assign the 
        /// attacker as the current target (if the have a Neutral Relation Type or higher) or search for any visible targets within the AI's Line of Sight.
        /// </summary>
        void CheckForAttacker (Transform AttackerTransform)
        {
            if (EmeraldComponent.CombatTarget == null && !EmeraldComponent.CombatComponent.CombatState)
            {
                StartCoroutine(DelaySetDetectedTarget(AttackerTransform));
            }
        }

        /// <summary>
        /// Delay SetDetectedTarget to give the AI time to play its non-combat hit animation and transfer to its combat state.
        /// </summary>
        IEnumerator DelaySetDetectedTarget(Transform AttackerTransform)
        {
            string RelationName = EmeraldComponent.DetectionComponent.GetTargetFactionRelation(AttackerTransform);

            yield return new WaitForSeconds(0.6f);

            if (RelationName == "Neutral" || RelationName == "Enemy")
            {
                EmeraldComponent.DetectionComponent.SetDetectedTarget(AttackerTransform);
            }
            else
            {
                EmeraldComponent.DetectionComponent.SearchForTarget(PickTargetTypes.Closest);
            }
        }

        /// <summary>
        /// Called when the AI's health reaches 0. The OnDeath delegate is also invoked and is responsible for triggering any death functionality to external subscribers.
        /// </summary>
        void Death()
        {
            OnDeath?.Invoke(); //Invoke the AI death event.
            EmeraldComponent.AnimationComponent.IsDead = true;
            EmeraldCombatManager.DisableComponents(EmeraldComponent);
            EmeraldCombatManager.EnableRagdoll(EmeraldComponent);
        }

        /// <summary>
        /// Refills the AI's health to full instantly
        /// </summary>
        public void InstantlyRefillAIHealth()
        {
            Health = StartHealth;
            CurrentHealth = StartHealth;
            OnHealthChange?.Invoke();
        }

        /// <summary>
        /// Instantly kills this AI.
        /// </summary>
        public void KillAI()
        {
            EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount = 1;
            Health = 0;
            Death();
        }

        /// <summary>
        /// Called through the OnExitCombat callback when an AI exits combat. This heals the AI over time according to its HealRate.
        /// </summary>
        void RecoverHealth ()
        {
            StartCoroutine(RecoverHealthInternal());
        }

        /// <summary>
        /// Called through the StartHealing function which increases the AI's health each HealRate. This gets canceled if the target enters combat.
        /// </summary>
        IEnumerator RecoverHealthInternal()
        {
            float t = 0;

            while (CurrentHealth < StartingHealth)
            {
                t += Time.deltaTime;

                if (t >= 1)
                {
                    CurrentHealth = CurrentHealth + HealRate;
                    OnHealRateTick?.Invoke();
                    t = 0;
                }

                if (EmeraldComponent.CombatComponent.CombatState) yield break;

                yield return null;
            }

            CurrentHealth = StartingHealth;
        }

        public void UpdateHealTick ()
        {
            OnHealRateTick?.Invoke();
        }

        public void UpdateHealingReceived()
        {
            OnHealingReceived?.Invoke();
        }

        /// <summary>
        /// Updates the AI's Max Health and Current Health.
        /// </summary>
        public void UpdateHealth(int MaxHealth, int CurrentHealth)
        {
            Health = CurrentHealth;
            StartHealth = MaxHealth;
            OnHealthChange?.Invoke();
        }

        /// <summary>
        /// Creates (an optional) hit effect (outside of an AI's Ability Objects) when an AI takes damage.
        /// </summary>
        void CreateHitEffect()
        {
            if (EmeraldComponent.HealthComponent.UseHitEffect == YesOrNo.Yes && !EmeraldComponent.LBDComponent && EmeraldComponent.HealthComponent.HitEffectsList.Count > 0 && !EmeraldComponent.AnimationComponent.IsDead)
            {
                GameObject RandomBloodEffect = EmeraldComponent.HealthComponent.HitEffectsList[UnityEngine.Random.Range(0, EmeraldComponent.HealthComponent.HitEffectsList.Count)];
                if (RandomBloodEffect != null)
                {
                    GameObject SpawnedBlood = EmeraldObjectPool.SpawnEffect(RandomBloodEffect, Vector3.zero, EmeraldComponent.transform.rotation, EmeraldComponent.HealthComponent.HitEffectTimeoutSeconds) as GameObject;
                    if (AttachHitEffects) SpawnedBlood.transform.SetParent(EmeraldComponent.transform);
                    SpawnedBlood.transform.position = EmeraldComponent.CombatComponent.DamagePosition() + EmeraldComponent.HealthComponent.HitEffectPosOffset;
                }
            }
        }
    }
}