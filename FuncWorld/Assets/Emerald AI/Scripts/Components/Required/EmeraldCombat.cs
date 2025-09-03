using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/combat-component")]
    public class EmeraldCombat : MonoBehaviour, ICombat
    {
        #region Combat Variables
        public List<EmeraldWeaponCollision> WeaponColliders = new List<EmeraldWeaponCollision>();
        public EmeraldWeaponCollision CurrentWeaponCollision;

        public int MinResumeWander = 2;
        public int MaxResumeWander = 4;

        public float CurrentAttackCooldown;
        public float Type1AttackCooldown = 0.35f;
        public float Type2AttackCooldown = 0.35f;

        public float SwitchWeaponTimer = 0;
        public bool SwitchWeaponTypeTriggered = false;
        public bool CombatActionActive;

        [SerializeField] public List<ActionsClass> CombatActions = new List<ActionsClass>();
        [SerializeField] public List<ActionsClass> Type1CombatActions = new List<ActionsClass>();
        [SerializeField] public List<ActionsClass> Type2CombatActions = new List<ActionsClass>();

        public Vector3 AttackPosition;

        //These can be set through Action Objects so actions like block and dodge can mitigate damage.
        public int MitigationAmount = 50;
        public float MaxMitigationAngle = 75;

        public delegate void KilledTargetHandler();
        public event KilledTargetHandler OnKilledTarget;
        public delegate void DoDamageHandler();
        public event DoDamageHandler OnDoDamage;
        public delegate void DoCritDamageHandler();
        public event DoCritDamageHandler OnDoCritDamage;
        public delegate void StartCombatHandler();
        public event StartCombatHandler OnStartCombat;
        public delegate void EndCombatHandler();
        public event EndCombatHandler OnEndCombat;

        //This differs from OnEndCombat as it's called when AI actually exits their combat state.
        //There are various components that subscribe to this for transitioning back to their non-combat states.
        public delegate void ExitCombatHandler();
        public event ExitCombatHandler OnExitCombat;

        public bool CombatState;

        public enum WeaponTypes { Type1 = 0, Type2 = 1 };
        public WeaponTypes StartingWeaponType = WeaponTypes.Type1;
        public WeaponTypes CurrentWeaponType = WeaponTypes.Type1;

        public PickTargetTypes Type1PickTargetType = PickTargetTypes.Closest;
        public PickTargetTypes Type2PickTargetType = PickTargetTypes.Closest;

        public enum WeaponTypeAmounts { One, Two };
        public WeaponTypeAmounts WeaponTypeAmount = WeaponTypeAmounts.One;

        [SerializeField]
        public AttackClass Type1Attacks;
        [SerializeField]
        public AttackClass Type2Attacks;

        public int SwitchWeaponTypesCooldown = 10;
        public int SwitchWeaponTypesDistance = 8;

        public Transform CurrentAttackTransform;
        public List<Transform> WeaponType1AttackTransforms = new List<Transform>();
        public List<Transform> WeaponType2AttackTransforms = new List<Transform>();

        public enum SwitchWeaponTypes { Distance, Timed, None};
        public SwitchWeaponTypes SwitchWeaponType = SwitchWeaponTypes.Timed;
        public int SwitchWeaponTimeMin = 10;
        public int SwitchWeaponTimeMax = 20;
        public float SwitchWeaponTime = 0;

        public float DistanceFromTarget;
        public float TargetAngle;
        public int ReceivedRagdollForceAmount;
        public Transform RagdollTransform;
        public Vector3 TargetDestination;
        public bool FirstTimeInCombat = true;
        public float DeathDelay;
        public bool DeathDelayActive;
        public float DeathDelayTimer;
        public int CurrentAnimationIndex = 0;
        public bool TargetDetectionActive;
        public bool TargetOutOfHeightRange;
        public float TooCloseDistance = 1;
        public float AttackDistance = 2.5f;
        public EmeraldAbilityObject CurrentEmeraldAIAbility;
        public List<AttackClass.AttackData> AvailableConditionAbilities = new List<AttackClass.AttackData>();
        EmeraldSystem EmeraldComponent;
        public AttackClass.AttackData CurrentAttackData;
        public Transform LastAttacker;
        #endregion

        #region Private Variables
        bool m_WeaponTypeSwitchDelay;
        Coroutine SwitchWeaponCoroutine;
        #endregion

        #region Editor Variable
        public bool HideSettingsFoldout;
        public bool DamageSettingsFoldout;
        public bool CombatActionSettingsFoldout;
        public bool SwitchWeaponSettingsFoldout;
        public bool WeaponType1SettingsFoldout;
        public bool WeaponType2SettingsFoldout;
        #endregion

        void Start()
        {
            InitializeCombat();
        }

        /// <summary>
        /// Initialize the Combat Component.
        /// </summary>
        void InitializeCombat ()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            EmeraldComponent.HealthComponent.OnDeath += CancelAllCombatActions; //Subscribe to the OnDeath event for CancelCombatActions
            EmeraldComponent.DetectionComponent.OnEnemyTargetDetected += EnterCombat; //Subscribe to the OnDeath event for CancelCombatActions
            EmeraldComponent.DetectionComponent.OnNullTarget += NullCombatTarget; //Subscribe to the OnNullTarget event for NullCombatTarget
            OnKilledTarget += CancelAllCombatActions; //Subscribe to the OnKilledTarget event for CancelCombatActions
            TargetDetectionActive = true;
            FirstTimeInCombat = true;
            SwitchWeaponTime = Random.Range((float)SwitchWeaponTimeMin, SwitchWeaponTimeMax + 1);
            DeathDelay = Random.Range(MinResumeWander, MaxResumeWander + 1);
            Invoke(nameof(InitializeAttacks), 0.1f);
        }

        void InitializeAttacks()
        {
            //Generate an attack based on the current weapon type 
            if (CurrentWeaponType == WeaponTypes.Type1)
            {
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type1Attacks);
                EmeraldComponent.DetectionComponent.PickTargetType = Type1PickTargetType;
                CurrentAttackCooldown = Type1AttackCooldown;
                
            }
            else if (CurrentWeaponType == WeaponTypes.Type2)
            {
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type2Attacks);
                EmeraldComponent.DetectionComponent.PickTargetType = Type2PickTargetType;
                CurrentAttackCooldown = Type2AttackCooldown;
            }
        }

        /// <summary>
        /// A custom update function for the EmeraldCombat script called through the EmeraldAISystem script.
        /// </summary>
        public void CombatUpdate()
        {
            if (CombatState)
            {
                DistanceFromTarget = EmeraldCombatManager.GetDistanceFromTarget(EmeraldComponent); //Update current distance from the target.
                TargetAngle = EmeraldCombatManager.TargetAngle(EmeraldComponent); //Update current angle from the target.
            }
            else if (!CombatState)
            {
                DistanceFromTarget = EmeraldCombatManager.GetDistanceFromLookTarget(EmeraldComponent); //Update current distance from the target.
                TargetAngle = EmeraldCombatManager.TransformAngle(EmeraldComponent, EmeraldComponent.LookAtTarget); //Update current angle from the target.
            }

            CheckForTargetDeath(); //Monitor the current target's health for when it dies.
            UpdateWeaponTypeState(); //Check for when to switch weapons.
            UpdateDeathDelay(); //Controls when an AI will go back to its non-combat state after killing a target.
        }

        /// <summary>
        /// Controls when the death delay feature has lapsed.
        /// </summary>
        void UpdateDeathDelay ()
        {
            if (DeathDelayActive)
            {
                DeathDelayTimer += Time.deltaTime;

                if (DeathDelayTimer > DeathDelay)
                {
                    ExitCombat();
                }
            }
        }

        /// <summary>
        /// Returns true if the AI's current target is within the angle limit to be attacked.
        /// </summary>
        /// <returns></returns>
        public bool TargetWithinAngleLimit ()
        {
            return TargetAngle <= EmeraldComponent.MovementComponent.CombatAngleToTurn;
        }

        /// <summary>
        /// Invoke the OnEnterCombat event that happens when the AI starts fighting its first target for the current battle.
        /// </summary>
        public void EnterCombat ()
        {
            if (FirstTimeInCombat) OnStartCombat?.Invoke();
            FirstTimeInCombat = false;
        }

        /// <summary>
        /// Resets various settings when exiting combat.
        /// </summary>
        public void ExitCombat ()
        {
            CombatState = false;
            SwitchWeaponTimer = 0;
            ClearTarget();
            FirstTimeInCombat = true;
            DeathDelayTimer = 0;
            DeathDelayActive = false;
            OnExitCombat?.Invoke(); //This is used in the Movement, Detection, and Animation components to return them to their default states.
        }

        /// <summary>
        /// Updates an AI's list of actions while in combat.
        /// </summary>
        public void UpdateActions()
        {
            if (CurrentWeaponType == WeaponTypes.Type1) CombatActions = Type1CombatActions;
            else if (CurrentWeaponType == WeaponTypes.Type2) CombatActions = Type2CombatActions;

            if (EmeraldComponent.CombatTarget != null && !EmeraldComponent.AnimationComponent.IsDead && !EmeraldComponent.AnimationComponent.IsStunned && !EmeraldComponent.AIAnimator.GetBool("Stunned Active"))
            {
                for (int i = 0; i < CombatActions.Count; i++)
                {
                    if (CombatActions[i].Enabled)
                    {
                        CombatActions[i].emeraldAction.UpdateAction(EmeraldComponent, CombatActions[i]);
                        var Conditions = (((int)CombatActions[i].emeraldAction.CooldownConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;

                        //Only update the cooldown timer if the conditions are met for this action.
                        if (Conditions && !EmeraldComponent.AIAnimator.GetBool("Attack") && !CombatActions[i].IsActive)
                            CombatActions[i].CooldownLengthTimer += Time.deltaTime;
                    }
                }
            }
        }

        /// <summary>
        /// Cancels all combat actions that are currently active.
        /// </summary>
        public void CancelAllCombatActions ()
        {
            if (CombatActions.Count == 0)
                return;

            for (int i = 0; i < CombatActions.Count; i++)
            {
                if (CombatActions[i].IsActive)
                {
                    CombatActions[i].emeraldAction.CancelAction(EmeraldComponent, CombatActions[i]);
                }
            }
        }

        /// <summary>
        /// Adjusts the all action cooldowns so two actions aren'transform triggered simultaneously (due to Animation States needing time to transition). This is called after some actions have been successfully triggered.
        /// </summary>
        public void AdjustCooldowns()
        {
            for (int i = 0; i < CombatActions.Count; i++)
            {
               if (CombatActions[i].CooldownLengthTimer >= CombatActions[i].emeraldAction.CooldownLength - 0.25f)
                    CombatActions[i].CooldownLengthTimer = 0;
            }
        }

        /// <summary>
        /// This function invokes the ability attached to the AI's Ability Object slot, of its Attack List of the Combat Component, when the set attack animation is playing. This should be set through an Animation Event.
        /// </summary>
        public void CreateAbility(AnimationEvent AttackEventParameters)
        {
            //Allow the objectReferenceParameter to override the current ability
            if (AttackEventParameters.objectReferenceParameter != null)
            {
                CurrentEmeraldAIAbility = (EmeraldAbilityObject)AttackEventParameters.objectReferenceParameter;
            }

            EmeraldCombatManager.UpdateAttackTransforms(EmeraldComponent, AttackEventParameters.stringParameter); //Updates the AI's current attack and weapon transforms based on the sent AttackTransformName from an EmeraldAttackEvent Animation Event.
            if (CurrentEmeraldAIAbility != null) CurrentEmeraldAIAbility.InvokeAbility(gameObject, CurrentAttackTransform); //Invoke the ability, if the ability slot is not emepty.
        }

        /// <summary>
        /// This function invokes a charge effect from the attached Ability Object slot, of its Attack List of the Combat Component, when the set attack animation is playing. This should be set through an Animation Event.
        /// </summary>
        public void ChargeEffect(AnimationEvent AttackEventParameters)
        {
            Transform AttackTransform = EmeraldCombatManager.GetAttackTransform(EmeraldComponent, AttackEventParameters.stringParameter); //Gets the weapon transform based on the sent AttackTransformName from an EmeraldChargeAttack Animation Event.
            if (CurrentEmeraldAIAbility != null && AttackTransform != null) CurrentEmeraldAIAbility.ChargeAbility(gameObject, AttackTransform); //Invoke the ability's charge, if the ability slot is not emepty.
        }

        /// <summary>
        /// Called through the OnNullTarget callback when a target becomes null. Clear the Current Target and look for another.
        /// If no new target is found, wait for death delay to lapse before returning to the non-combat state.
        /// </summary>
        void NullCombatTarget()
        {
            if (EmeraldComponent.CombatTarget == null && CombatState && !EmeraldComponent.MovementComponent.ReturningToStartInProgress && !DeathDelayActive)
            {
                DeathDelay = Random.Range(MinResumeWander, MaxResumeWander + 1);
                DeathDelayActive = true;
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                ClearTarget();
            }
        }

        /// <summary>
        /// Watch the CurrentIDamageable for when the health reaches 0. 
        /// </summary>
        void CheckForTargetDeath()
        {
            if (EmeraldComponent.CurrentTargetInfo.CurrentIDamageable != null)
            {
                if (EmeraldComponent.CurrentTargetInfo.CurrentIDamageable.Health <= 0 && !DeathDelayActive)
                {
                    OnKilledTarget?.Invoke();
                    DeathDelay = Random.Range(MinResumeWander, MaxResumeWander + 1);
                    DeathDelayActive = true;
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    Invoke(nameof(ClearTarget), 0.01f); //Was 0.75
                }
            }
        }

        /// <summary>
        /// Clears the AI's current target.
        /// </summary>
        public void ClearTarget()
        {
            if (EmeraldComponent.CombatTarget != null)
            {
                //Remove the CurrentTarget from the AI's LineOfSightTargets list.
                if (EmeraldComponent.DetectionComponent.LineOfSightTargets.Contains(EmeraldComponent.CombatTarget.GetComponent<Collider>()))
                    EmeraldComponent.DetectionComponent.LineOfSightTargets.Remove(EmeraldComponent.CombatTarget.GetComponent<Collider>());
            }
            else
            {
                //The CurrentTarget is null, remove it, and any other null targes, from the list.
                for (int i = 0; i < EmeraldComponent.DetectionComponent.LineOfSightTargets.Count; i++)
                {
                    if (EmeraldComponent.DetectionComponent.LineOfSightTargets[i] == null)
                    {
                        EmeraldComponent.DetectionComponent.LineOfSightTargets.RemoveAt(i);
                    }
                }
                    
            }

            //Clear the current target references
            EmeraldComponent.CombatTarget = null;
            EmeraldComponent.CurrentTargetInfo.TargetSource = null;
            EmeraldComponent.CurrentTargetInfo.CurrentIDamageable = null;
            EmeraldComponent.CurrentTargetInfo.CurrentICombat = null;

            //Invoke the OnEndCombat callback if there's no remaining detectable enemy targets nearby.
            if (EmeraldComponent.DetectionComponent.LineOfSightTargets.Count == 0) OnEndCombat?.Invoke();
        }

        /// <summary>
        /// Invoked below with cooldown delay and controls the AI's weapon type switching from happening too often
        /// </summary>
        void WeaponSwitchCooldown()
        {
            m_WeaponTypeSwitchDelay = false;
        }

        /// <summary>
        /// Updates the AI's weapon type to switch between weapon types.
        /// </summary>
        void UpdateWeaponTypeState()
        {
            if (!CombatState || DeathDelayActive)
                return;

            if (WeaponTypeAmount == WeaponTypeAmounts.Two)
            {
                if (SwitchWeaponTypeTriggered && !m_WeaponTypeSwitchDelay)
                {
                    SwitchWeaponTypeTriggered = false;
                    m_WeaponTypeSwitchDelay = true;
                    Invoke(nameof(WeaponSwitchCooldown), SwitchWeaponTypesCooldown);
                }

                //Switches the current weapon type based on distance.
                if (SwitchWeaponType == SwitchWeaponTypes.Distance && EmeraldComponent.CombatTarget != null && !m_WeaponTypeSwitchDelay && 
                    !EmeraldComponent.AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.AnimationComponent.IsAttacking && !EmeraldComponent.AnimationComponent.IsMoving && !EmeraldComponent.AnimationComponent.IsBackingUp && !EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AIAnimator.GetBool("Strafe Active"))
                {
                    if (DistanceFromTarget > SwitchWeaponTypesDistance && CurrentWeaponType != StartingWeaponType && !SwitchWeaponTypeTriggered)
                    {
                        SwapWeaponType();
                        SwitchWeaponTypeTriggered = true;
                    }
                    if (DistanceFromTarget < SwitchWeaponTypesDistance && CurrentWeaponType == StartingWeaponType && !SwitchWeaponTypeTriggered)
                    {
                        SwapWeaponType();
                        SwitchWeaponTypeTriggered = true;
                    }
                }
                //Switches the current weapon type based on a random time of SwitchWeaponTimeMin and SwitchWeaponTimeMax.
                else if (SwitchWeaponType == SwitchWeaponTypes.Timed)
                {
                    if (!EmeraldComponent.AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.AnimationComponent.IsEquipping && !EmeraldComponent.AnimationComponent.IsMoving)
                        SwitchWeaponTimer += Time.deltaTime;

                    if (EmeraldComponent.CombatTarget != null && SwitchWeaponTimer >= SwitchWeaponTime && 
                        !EmeraldComponent.AnimationComponent.IsSwitchingWeapons && !EmeraldComponent.AnimationComponent.IsEquipping && !EmeraldComponent.AnimationComponent.IsGettingHit && !EmeraldComponent.AnimationComponent.IsAttacking && !EmeraldComponent.AnimationComponent.IsMoving && !EmeraldComponent.AnimationComponent.IsBackingUp && !EmeraldComponent.AnimationComponent.IsTurning && !EmeraldComponent.AIAnimator.GetBool("Strafe Active"))
                    {
                        SwapWeaponType();
                    }
                }
            }
        }

        /// <summary>
        /// Swaps the current weapon type.
        /// </summary>
        public void SwapWeaponType()
        {
            if (CurrentWeaponType == WeaponTypes.Type1)
            {
                if (SwitchWeaponCoroutine != null) StopCoroutine(SwitchWeaponCoroutine);
                SwitchWeaponCoroutine = StartCoroutine(ChangeWeaponType("Type2")); //Switch to the Weapon Type 2
            }
            else if (CurrentWeaponType == WeaponTypes.Type2)
            {
                if (SwitchWeaponCoroutine != null) StopCoroutine(SwitchWeaponCoroutine);
                SwitchWeaponCoroutine = StartCoroutine(ChangeWeaponType("Type1")); //Switch to the Weapon Type 1
            }
        }

        IEnumerator ChangeWeaponType(string WeaponTypeName)
        {
            EmeraldComponent.AnimationComponent.IsSwitchingWeapons = true;
            EmeraldCombatManager.ResetWeaponSwapTime(EmeraldComponent);
            CurrentAnimationIndex = 1;
            EmeraldComponent.AIAnimator.SetInteger("Attack Index", 1);
            EmeraldComponent.AIAnimator.SetInteger("Hit Index", 1);
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            yield return new WaitForSeconds(0.1f);

            if (WeaponTypeName == "Type1")
            {
                yield return new WaitUntil(()=>EmeraldComponent.AnimationComponent.IsIdling);
                EmeraldComponent.AIAnimator.SetInteger("Weapon Type State", 1);
                CurrentAttackCooldown = Type1AttackCooldown;
                EmeraldComponent.DetectionComponent.PickTargetType = Type1PickTargetType;
            }
            else if (WeaponTypeName == "Type2")
            {
                yield return new WaitUntil(() => EmeraldComponent.AnimationComponent.IsIdling);
                EmeraldComponent.AIAnimator.SetInteger("Weapon Type State", 2);
                CurrentAttackCooldown = Type2AttackCooldown;
                EmeraldComponent.DetectionComponent.PickTargetType = Type2PickTargetType;
            }

            CurrentWeaponType = (WeaponTypes)System.Enum.Parse(typeof(WeaponTypes), WeaponTypeName);

            if (EmeraldComponent.AIAnimator.GetBool("Animate Weapon State"))
            {
                while (!EmeraldComponent.AnimationComponent.IsEquipping)
                {
                    yield return null;
                }
            }
            else
            {
                //If an AI doesn'transform have equipping animations, bypass the need for equipping animations and enable/disable them here.
                EmeraldItems m_EmeraldItems = GetComponent<EmeraldItems>();

                if (m_EmeraldItems != null)
                {
                    yield return new WaitForSeconds(0.5f);

                    if (WeaponTypeName == "Type1")
                    {
                        m_EmeraldItems.UnequipWeapon("Weapon Type 2");
                        m_EmeraldItems.EquipWeapon("Weapon Type 1");
                    }
                    else if (WeaponTypeName == "Type2")
                    {
                        m_EmeraldItems.UnequipWeapon("Weapon Type 1");
                        m_EmeraldItems.EquipWeapon("Weapon Type 2");
                    }
                }
            }

            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            UpdateWeaponTypeValues();
            EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
            EmeraldComponent.AnimationComponent.IsSwitchingWeapons = false;
        }

        /// <summary>
        /// Regenerate an attack based on the current weapon type and update the needed settings.
        /// </summary>
        void UpdateWeaponTypeValues()
        {
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AIAnimator.SetInteger("Attack Index", 1);
            if (CurrentWeaponType == WeaponTypes.Type1)
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type1Attacks);
            else if (CurrentWeaponType == WeaponTypes.Type2)
                EmeraldCombatManager.GenerateAttack(EmeraldComponent, Type2Attacks);
        }

        public void InvokeDoDamage ()
        {
            OnDoDamage?.Invoke();
        }

        public void InvokeDoCritDamage()
        {
            OnDoCritDamage?.Invoke();
        }

        /// <summary>
        /// Used for getting the transform of the target.
        /// </summary>
        public Transform TargetTransform()
        {
            return transform;
        }

        /// <summary>
        /// Used for detecting when a target is attacking.
        /// </summary>
        public bool IsAttacking()
        {
            return EmeraldComponent.AnimationComponent.AttackTriggered;
        }

        /// <summary>
        /// Used for detecting when this target is blocking.
        /// </summary>
        public bool IsBlocking()
        {
            return EmeraldComponent.AnimationComponent.IsBlocking && EmeraldComponent.AIAnimator.GetBool("Blocking");
        }

        /// <summary>
        /// Used for detecting when this target is dodging.
        /// </summary>
        public bool IsDodging()
        {
            return EmeraldComponent.AnimationComponent.IsDodging;
        }

        /// <summary>
        /// Used referencing the AI's damage position when an AI takes damage from external sources.
        /// </summary>
        public Vector3 DamagePosition()
        {
            if (EmeraldComponent.TPMComponent != null)
                return new Vector3(EmeraldComponent.TPMComponent.TransformSource.position.x, EmeraldComponent.TPMComponent.TransformSource.position.y + EmeraldComponent.TPMComponent.PositionModifier, EmeraldComponent.TPMComponent.TransformSource.position.z);
            else
                return transform.position + new Vector3(0, transform.localScale.y, 0);
        }

        /// <summary>
        /// Called when an ability generates a stun (used through the ICombat inferface).
        /// </summary>
        public void TriggerStun(float StunLength)
        {
            EmeraldComponent.AnimationComponent.PlayStunnedAnimation(StunLength);
        }
    }
}