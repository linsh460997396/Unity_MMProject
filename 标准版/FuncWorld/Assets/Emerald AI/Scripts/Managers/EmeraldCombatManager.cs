using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// Handles all Emerald AI combat related functionality.
    /// </summary>
    public static class EmeraldCombatManager
    {
        /// <summary>
        /// Handles the generation of all Emerald AI attacks.
        /// </summary>
        public static void GenerateAttack(EmeraldSystem EmeraldComponent, AttackClass SentAttackClass, bool OverridePickType = false)
        {
            if (SentAttackClass.AttackDataList.Count > 0)
            {
                List<AttackClass.AttackData> AvailableAttacks = new List<AttackClass.AttackData>();

                //Go through the attack list and get all cooled down attacks. Note: This is ignored for the Ordered Pick Type.
                for (int i = 0; i < SentAttackClass.AttackDataList.Count; i++)
                {
                    SentAttackClass.AttackDataList[i].CooldownIgnored = false; //Reset before checking cooldown again

                    float CooldownTime = 0;
                    if (SentAttackClass.AttackDataList[i].AbilityObject != null) CooldownTime = (SentAttackClass.AttackDataList[i].CooldownTimeStamp + SentAttackClass.AttackDataList[i].AbilityObject.CooldownSettings.CooldownLength);

                    if (Time.time >= CooldownTime || SentAttackClass.AttackDataList[i].CooldownTimeStamp == 0 || SentAttackClass.AttackDataList[i].AbilityObject != null && !SentAttackClass.AttackDataList[i].AbilityObject.CooldownSettings.Enabled)
                    {
                        if (!AvailableAttacks.Contains(SentAttackClass.AttackDataList[i]))
                        {
                            if (SentAttackClass.AttackDataList[i].AbilityObject == null)
                            {
                                AvailableAttacks.Add(SentAttackClass.AttackDataList[i]);
                            }
                            else if (!SentAttackClass.AttackDataList[i].AbilityObject.ConditionSettings.Enabled || 
                                SentAttackClass.AttackDataList[i].AbilityObject.ConditionSettings.Enabled && !SentAttackClass.AttackDataList[i].AbilityObject.ConditionSettings.HighPriority && CheckAbilityConditions(EmeraldComponent, SentAttackClass.AttackDataList[i]))
                            {
                                AvailableAttacks.Add(SentAttackClass.AttackDataList[i]);
                            }
                        }
                    }
                }

                //If there are no available attacks, ignore the cooldowns and pick a random ability to avoid an error.
                if (AvailableAttacks.Count == 0)
                {
                    SetAttackValues(EmeraldComponent, SentAttackClass.AttackDataList[Random.Range(0, SentAttackClass.AttackDataList.Count)], true);
                    Debug.Log("All cooldowns are busy with the: '" + EmeraldComponent.gameObject.name + "' AI so the a random attack within its Attack List was used. To avoid this, add more attacks or decrease some of this AI's cooldowns times within the Ability Objects.");
                    return;
                }

                if (SentAttackClass.AttackPickType == AttackPickTypes.Odds) //Pick an ability by using the odds for each available ability
                {
                    List<float> OddsList = new List<float>();
                    for (int i = 0; i < AvailableAttacks.Count; i++)
                    {
                        OddsList.Add(AvailableAttacks[i].AttackOdds);
                    }
                    int OddsIndex = (int)GenerateProbability(OddsList.ToArray());
                    SetAttackValues(EmeraldComponent, AvailableAttacks[OddsIndex], false);
                }
                else if (SentAttackClass.AttackPickType == AttackPickTypes.Order) //Pick an ability by going through the list in order
                {
                    float CooldownTime = 0;
                    AttackClass.AttackData AbilityDataRef = SentAttackClass.AttackDataList[SentAttackClass.AttackListIndex];

                    if (AbilityDataRef.AbilityObject == null || AbilityDataRef.AbilityObject != null && !AbilityDataRef.AbilityObject.CooldownSettings.Enabled && !AbilityDataRef.AbilityObject.ConditionSettings.Enabled)
                    {
                        SetAttackValues(EmeraldComponent, SentAttackClass.AttackDataList[SentAttackClass.AttackListIndex], false);
                        SentAttackClass.AttackListIndex++;
                        if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                    }
                    else
                    {
                        //Start the loop to find the next available attack
                        bool attackFound = false;
                        while (!attackFound)
                        {
                            AbilityDataRef = SentAttackClass.AttackDataList[SentAttackClass.AttackListIndex];
                            if (AbilityDataRef.AbilityObject != null)
                                CooldownTime = (AbilityDataRef.CooldownTimeStamp + AbilityDataRef.AbilityObject.CooldownSettings.CooldownLength);

                            //If no AbilityObject is found, play the attack animation
                            if (AbilityDataRef.AbilityObject == null)
                            {
                                SetAttackValues(EmeraldComponent, AbilityDataRef, false);
                                SentAttackClass.AttackListIndex++;
                                if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                                attackFound = true;
                            }
                            else if (Time.time >= CooldownTime && !AbilityDataRef.AbilityObject.ConditionSettings.Enabled || AbilityDataRef.CooldownTimeStamp == 0) //Check cooldown conditions
                            {
                                //Check Condition Module conditions
                                if (!AbilityDataRef.AbilityObject.ConditionSettings.Enabled || AbilityDataRef.AbilityObject.ConditionSettings.Enabled && !AbilityDataRef.AbilityObject.ConditionSettings.HighPriority && CheckAbilityConditions(EmeraldComponent, AbilityDataRef))
                                {
                                    //If the ability is ready, set the attack values and break the loop
                                    SetAttackValues(EmeraldComponent, AbilityDataRef, false);
                                    SentAttackClass.AttackListIndex++;
                                    if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                                    attackFound = true;
                                }
                                else
                                {
                                    //If the ability is not ready, move to the next one
                                    SentAttackClass.AttackListIndex++;
                                    if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                                }
                            }
                            else
                            {
                                //If the ability is not ready, move to the next one
                                SentAttackClass.AttackListIndex++;
                                if (SentAttackClass.AttackListIndex == SentAttackClass.AttackDataList.Count) SentAttackClass.AttackListIndex = 0;
                            }
                        }
                    }
                }
                else if (SentAttackClass.AttackPickType == AttackPickTypes.Random) //Pick a random ability from the list
                {
                    int RandomIndex = Random.Range(0, AvailableAttacks.Count);
                    SetAttackValues(EmeraldComponent, AvailableAttacks[RandomIndex], false);
                }

                //Overrides the current pick type by picking the closest attack. So far, this is done to force a random attack from the AI's attack list.
                if (OverridePickType)
                {
                    int RandomIndex = Random.Range(0, AvailableAttacks.Count);
                    SetAttackValues(EmeraldComponent, AvailableAttacks[RandomIndex], false);
                }

                CheckConditionalAbiitities(EmeraldComponent, SentAttackClass, OverridePickType);
            }
        }

        /// <summary>
        /// Check for conditional abilities and add them to the AvailableConditionAbilities if their condition is met.
        /// </summary>
        public static void CheckConditionalAbiitities(EmeraldSystem EmeraldComponent, AttackClass SentAttackClass, bool OverridePickType = false)
        {
            EmeraldComponent.CombatComponent.AvailableConditionAbilities.Clear();

            foreach (var attackData in SentAttackClass.AttackDataList)
            {
                //Only get abilities that are using the Condition Module
                if (attackData.AbilityObject != null && attackData.AbilityObject.ConditionSettings.Enabled)
                {
                    //Only get abilities that are not on cooldown
                    if ((Time.time >= attackData.CooldownTimeStamp + attackData.AbilityObject.CooldownSettings.CooldownLength || attackData.CooldownTimeStamp == 0))
                    {
                        //Lastly, check ability conditions then add to AvailableConditionAbilities list
                        if (CheckAbilityConditions(EmeraldComponent, attackData)) EmeraldComponent.CombatComponent.AvailableConditionAbilities.Add(attackData);
                    }
                }
            }

            if (EmeraldComponent.CombatComponent.AvailableConditionAbilities.Count > 0)
            {
                EmeraldComponent.CombatComponent.CancelAllCombatActions();
                EmeraldComponent.CombatComponent.AdjustCooldowns();
                int RandomUseableAbility = Random.Range(0, EmeraldComponent.CombatComponent.AvailableConditionAbilities.Count);
                SetAttackValues(EmeraldComponent, EmeraldComponent.CombatComponent.AvailableConditionAbilities[RandomUseableAbility], false);
            }
        }

        /// <summary>
        /// Check each condition for the passed ability to see if they can be used.
        /// </summary>
        static bool CheckAbilityConditions(EmeraldSystem EmeraldComponent, AttackClass.AttackData AttackData)
        {
            if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.SelfLowHealth)
            {
                return ((float)EmeraldComponent.HealthComponent.CurrentHealth / (float)EmeraldComponent.HealthComponent.StartingHealth) <= (AttackData.AbilityObject.ConditionSettings.LowHealthPercentage * 0.01f);
            }
            else if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.AllyLowHealth)
            {
                if (EmeraldComponent.DetectionComponent.NearbyAllies.Count > 0)
                {
                    for (int j = 0; j < EmeraldComponent.DetectionComponent.NearbyAllies.Count; j++)
                    {
                        bool HasLowHealth = ((float)EmeraldComponent.DetectionComponent.NearbyAllies[j].HealthComponent.CurrentHealth / (float)EmeraldComponent.DetectionComponent.NearbyAllies[j].HealthComponent.StartingHealth) <= (AttackData.AbilityObject.ConditionSettings.LowHealthPercentage * 0.01f);
                        if (HasLowHealth && !EmeraldComponent.DetectionComponent.NearbyAllies[j].AnimationComponent.IsDead)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.NoCurrentSummons)
            {
                return EmeraldComponent.DetectionComponent.CurrentFollowers.Count == 0;
            }
            else if (AttackData.AbilityObject.ConditionSettings.ConditionType == ConditionTypes.DistanceFromTarget)
            {
                if (AttackData.AbilityObject.ConditionSettings.ValueCompareType == AbilityData.ConditionData.ValueCompareTypes.LessThan)
                {
                    return EmeraldComponent.CombatComponent.DistanceFromTarget <= AttackData.AbilityObject.ConditionSettings.DistanceFromTarget;
                }
                else if (AttackData.AbilityObject.ConditionSettings.ValueCompareType == AbilityData.ConditionData.ValueCompareTypes.GreaterThan)
                {
                    return EmeraldComponent.CombatComponent.DistanceFromTarget >= AttackData.AbilityObject.ConditionSettings.DistanceFromTarget;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the data of the currently generated attack.
        /// </summary>
        static void SetAttackValues(EmeraldSystem EmeraldComponent, AttackClass.AttackData AttackData, bool CooldownIgnored)
        {
            EmeraldComponent.CombatComponent.CurrentAttackData = AttackData;

            AttackData.CooldownIgnored = CooldownIgnored;

            EmeraldComponent.CombatComponent.CurrentAnimationIndex = AttackData.AttackAnimation;
            EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility = AttackData.AbilityObject;

            EmeraldComponent.CombatComponent.AttackDistance = AttackData.AttackDistance;
            //Don'transform allow the attack distance from being higher than the detection distance.
            if (EmeraldComponent.CombatComponent.AttackDistance > EmeraldComponent.DetectionComponent.DetectionRadius) EmeraldComponent.CombatComponent.AttackDistance = EmeraldComponent.DetectionComponent.DetectionRadius;
            EmeraldComponent.CombatComponent.TooCloseDistance = AttackData.TooCloseDistance;
            if (EmeraldComponent.CombatComponent.CombatState) EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.CombatComponent.AttackDistance;
        }

        /// <summary>
        /// Generate the next attack using the current attack class and the AI's Pick Type.
        /// </summary>
        public static void GenerateNextAttack (EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type1Attacks);
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type2Attacks);

            EmeraldComponent.AnimationComponent.AttackingTracker = false;
        }

        /// <summary>
        /// Overrides the currently generated attack with the closest attack within an AI's Attack List. 
        /// </summary>
        public static void GenerateClosestAttack(EmeraldSystem EmeraldComponent)
        {
            EmeraldComponent.AnimationComponent.AttackingTracker = false;

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type1Attacks, true);
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
                GenerateAttack(EmeraldComponent, EmeraldComponent.CombatComponent.Type2Attacks, true);
        }

        /// <summary>
        /// Updates the AI's current attack and weapon transforms based on the sent AttackTransformName from an EmeraldAttackEvent Animation Event.
        /// This is done by searching for a matching name within the Attack Transform list. If no match is found, the AI's head transfrom will be used instead.
        /// </summary>
        public static void UpdateAttackTransforms (EmeraldSystem EmeraldComponent, string AttackTransformName)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                Transform AttackTransform = EmeraldComponent.CombatComponent.WeaponType1AttackTransforms.Find(x => x != null && x.name == AttackTransformName);
                if (AttackTransform != null)
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = AttackTransform;
                }
                else
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = EmeraldComponent.DetectionComponent.HeadTransform;
                }
            }
            else if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2)
            {
                Transform AttackTransform = EmeraldComponent.CombatComponent.WeaponType2AttackTransforms.Find(x => x.name == AttackTransformName);
                if (AttackTransform != null)
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = AttackTransform;
                }
                else
                {
                    EmeraldComponent.CombatComponent.CurrentAttackTransform = EmeraldComponent.DetectionComponent.HeadTransform;
                }
            }
        }

        /// <summary>
        /// Returns the weapon transforms based on the sent AttackTransformName from an EmeraldChargeAttack Animation Event.
        /// This is done by searching for a matching name within the Attack Transform list. If no match is found, the EmeraldChargeAttack event will not fire.
        /// </summary>
        public static Transform GetAttackTransform(EmeraldSystem EmeraldComponent, string AttackTransformName)
        {
            Transform WeaponTransform = null;

            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1)
            {
                WeaponTransform = EmeraldComponent.CombatComponent.WeaponType1AttackTransforms.Find(x => x != null && x.name == AttackTransformName);
            }
            else
            {
                WeaponTransform = EmeraldComponent.CombatComponent.WeaponType2AttackTransforms.Find(x => x.name == AttackTransformName);
            }

            return WeaponTransform;
        }

        /// <summary>
        /// Generates the AI's weapon type swap time.
        /// </summary>
        public static void ResetWeaponSwapTime (EmeraldSystem EmeraldComponent)
        {
            EmeraldComponent.CombatComponent.SwitchWeaponTime = Random.Range((float)EmeraldComponent.CombatComponent.SwitchWeaponTimeMin, (float)EmeraldComponent.CombatComponent.SwitchWeaponTimeMax + 1);
            EmeraldComponent.CombatComponent.SwitchWeaponTimer = 0;
        }

        /// <summary>
        /// Activates the AI's Combat State.
        /// </summary>
        public static void ActivateCombatState(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatComponent.CombatState)
                return;

            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.CombatComponent.CombatState = true;
            EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
            EmeraldComponent.AIAnimator.SetBool("Combat State Active", true);
            EmeraldComponent.MovementComponent.CurrentMovementState = EmeraldMovement.MovementStates.Run;
        }

        /// <summary>
        /// Disables an AI's components (called when an AI dies).
        /// </summary>
        public static void DisableComponents(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.SoundDetectorComponent != null) EmeraldComponent.SoundDetectorComponent.enabled = false;

            if (EmeraldComponent.OptimizationComponent != null && EmeraldComponent.OptimizationComponent.m_VisibilityCheck != null)
            {
                EmeraldComponent.OptimizationComponent.enabled = false;
                EmeraldComponent.OptimizationComponent.m_VisibilityCheck.enabled = false;
            }

            EmeraldComponent.CombatComponent.ExitCombat();
            EmeraldComponent.AIBoxCollider.enabled = false;
            EmeraldComponent.DetectionComponent.enabled = false;
            EmeraldComponent.AnimationComponent.enabled = false;
            EmeraldComponent.CombatComponent.enabled = false;
            EmeraldComponent.MovementComponent.enabled = false;
            EmeraldComponent.BehaviorsComponent.enabled = false;
            EmeraldComponent.m_NavMeshAgent.ResetPath();
            EmeraldComponent.m_NavMeshAgent.enabled = false;
            EmeraldComponent.StartCoroutine(AlignAIOnDeath(EmeraldComponent)); //Align the AI on death, even if the alignment feature is disabled.
        }

        /// <summary>
        /// Aligns the AI when the AI dies.
        /// </summary>
        static IEnumerator AlignAIOnDeath (EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.DeathList.Count == 0 ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.DeathList.Count == 0)
                yield break;

            Vector3 SurfaceNormal = Vector3.zero;

            while (EmeraldComponent.AIAnimator.enabled)
            {
                RaycastHit HitDown;
                if (Physics.Raycast(new Vector3(EmeraldComponent.transform.position.x, EmeraldComponent.transform.position.y + 0.25f, EmeraldComponent.transform.position.z), -Vector3.up, out HitDown, 2f, EmeraldComponent.MovementComponent.AlignmentLayerMask))
                {
                    if (HitDown.transform != EmeraldComponent.transform)
                    {
                        float m_MaxNormalAngle = EmeraldComponent.MovementComponent.MaxNormalAngle * 0.01f;
                        SurfaceNormal = HitDown.normal;
                        SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -m_MaxNormalAngle, m_MaxNormalAngle);
                        SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -m_MaxNormalAngle, m_MaxNormalAngle);
                    }
                }

                EmeraldComponent.transform.rotation = Quaternion.Slerp(EmeraldComponent.transform.rotation, Quaternion.FromToRotation(EmeraldComponent.transform.up, SurfaceNormal) * EmeraldComponent.transform.rotation, Time.deltaTime * 5);
                yield return null;
            }
        }


        /// <summary>
        /// Enables an AI's components (called when an AI is reset).
        /// </summary>
        public static void EnableComponents(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.SoundDetectorComponent != null) EmeraldComponent.SoundDetectorComponent.enabled = true;

            if (EmeraldComponent.OptimizationComponent != null && EmeraldComponent.OptimizationComponent.m_VisibilityCheck != null)
            {
                EmeraldComponent.OptimizationComponent.enabled = false;
                EmeraldComponent.OptimizationComponent.m_VisibilityCheck.enabled = true;
            }

            if (EmeraldComponent.InverseKinematicsComponent != null) EmeraldComponent.InverseKinematicsComponent.EnableInverseKinematics();

            EmeraldComponent.m_NavMeshAgent.enabled = true;
            EmeraldComponent.AIBoxCollider.enabled = true;
            EmeraldComponent.DetectionComponent.enabled = true;
            EmeraldComponent.AnimationComponent.enabled = true;
            EmeraldComponent.CombatComponent.enabled = true;
            EmeraldComponent.MovementComponent.enabled = true;
            EmeraldComponent.BehaviorsComponent.enabled = true;
            EmeraldComponent.AIAnimator.enabled = true;
            EmeraldComponent.AIAnimator.Rebind();
            EmeraldComponent.AnimationComponent.ResetSettings();
            if (EmeraldComponent.LBDComponent != null) EmeraldComponent.LBDComponent.ResetLBDComponents();
            if (EmeraldComponent.ItemsComponent != null) EmeraldComponent.ItemsComponent.ResetSettings();
        }

        /// <summary>
        /// Disable colliders and rigidbodies, given they're detected.
        /// </summary>
        public static void DisableRagdoll(EmeraldSystem EmeraldComponent)
        {
            //Return if a LocationBasedDamage component is detected as colliders need to stay active for this feature.
            if (EmeraldComponent.GetComponent<LocationBasedDamage>() != null)
                return;

            foreach (Rigidbody R in EmeraldComponent.GetComponentsInChildren<Rigidbody>())
            {
                R.isKinematic = true;
            }

            if (EmeraldComponent.LBDComponent != null)
                return;

            foreach (Collider C in EmeraldComponent.GetComponentsInChildren<Collider>())
            {
                C.enabled = false;
            }

            EmeraldComponent.GetComponent<BoxCollider>().enabled = true;
        }


        /// <summary>
        /// Enable colliders and rigidbodies inside the AI, given they're detected.
        /// </summary>
        public static void EnableRagdoll(EmeraldSystem EmeraldComponent)
        {
            //Only enable the ragdoll components if the current weapon type death animation lists don'transform have animations in them.
            if (EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type1 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type1Animations.DeathList.Count > 0 ||
                EmeraldComponent.CombatComponent.CurrentWeaponType == EmeraldCombat.WeaponTypes.Type2 && EmeraldComponent.AnimationComponent.m_AnimationProfile.Type2Animations.DeathList.Count > 0)
                return;

            EmeraldComponent.AIAnimator.enabled = false;

            if (EmeraldComponent.LBDComponent == null)
            {
                foreach (Collider C in EmeraldComponent.GetComponentsInChildren<Collider>())
                {
                    if (C.transform != EmeraldComponent.transform)
                    {
                        C.enabled = true;
                    }
                }

                foreach (Rigidbody R in EmeraldComponent.GetComponentsInChildren<Rigidbody>())
                {
                    R.isKinematic = false;
                    R.useGravity = true;
                }
            }
            else
            {
                for (int i = 0; i < EmeraldComponent.LBDComponent.ColliderList.Count; i++)
                {
                    if (EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject != null)
                        EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject.enabled = true;
                }

                for (int i = 0; i < EmeraldComponent.LBDComponent.ColliderList.Count; i++)
                {
                    Rigidbody ColliderRigidbody = EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject.GetComponent<Rigidbody>();

                    if (ColliderRigidbody != null)
                    {
                        ColliderRigidbody.isKinematic = false;
                        ColliderRigidbody.useGravity = true;
                    }

                    EmeraldComponent.LBDComponent.ColliderList[i].ColliderObject.enabled = true;
                }
            }

            EmeraldComponent.StartCoroutine(AddRagdollForceInternal(EmeraldComponent)); //Add force to the ragdoll after enable its components
        }

        static IEnumerator AddRagdollForceInternal(EmeraldSystem EmeraldComponent)
        {
            float t = 0;
            //Used to provide the force to the ragdoll in the opposite direction of the last target to hit the AI.
            Transform LastAttacker = EmeraldComponent.CombatComponent.LastAttacker;
            float Force = EmeraldComponent.CombatComponent.ReceivedRagdollForceAmount;
            Rigidbody m_Rigidbody = null;

            //Use the HeadTransform for the default rigidbody force on death.
            if (EmeraldComponent.CombatComponent.RagdollTransform == null) m_Rigidbody = EmeraldComponent.DetectionComponent.HeadTransform.GetComponent<Rigidbody>();
            //If the RagdollTransform is not null, add the force for the ragdoll death to this as an AI was damaged through a LBD area.
            else m_Rigidbody = EmeraldComponent.CombatComponent.RagdollTransform.GetComponent<Rigidbody>();

            if (m_Rigidbody != null && LastAttacker != null)
            {
                while (t < 0.2f)
                {
                    t += Time.fixedDeltaTime;
                    m_Rigidbody.AddForce((EmeraldComponent.transform.position - LastAttacker.position).normalized * Force + (Vector3.up * Force * 0.05f), ForceMode.Acceleration);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Returns true if the conditions are right to allow the AI to trigger an attack.
        /// </summary>
        public static bool AllowedToAttack(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.DetectionComponent.TargetObstructed || EmeraldComponent.CombatComponent.DeathDelayActive || EmeraldComponent.MovementComponent.DefaultMovementPaused || EmeraldComponent.AnimationComponent.InternalHit || EmeraldComponent.AnimationComponent.InternalDodge || EmeraldComponent.AnimationComponent.InternalBlock)
            {
                return false;
            }
            else if (!WithinStoppingDistanceOfTarget(EmeraldComponent) || !WithinDistanceOfTarget(EmeraldComponent))
            {
                return false;
            }
            else if (EmeraldComponent.AIAnimator.GetBool("Hit") || EmeraldComponent.AIAnimator.GetBool("Strafe Active") || EmeraldComponent.AIAnimator.GetBool("Dodge Triggered") || EmeraldComponent.AIAnimator.GetBool("Blocking") || EmeraldComponent.AnimationComponent.IsSwitchingWeapons ||
                EmeraldComponent.AnimationComponent.IsBackingUp || EmeraldComponent.AnimationComponent.IsBlocking || EmeraldComponent.AnimationComponent.IsAttacking || EmeraldComponent.AnimationComponent.IsRecoiling || EmeraldComponent.AnimationComponent.IsStrafing || EmeraldComponent.AnimationComponent.IsDodging || EmeraldComponent.AnimationComponent.IsGettingHit)
            {
                return false;
            }
            else if (!EmeraldComponent.CombatComponent.TargetWithinAngleLimit() || EmeraldComponent.CurrentTargetInfo.CurrentIDamageable.Health <= 0)
            {
                return false;
            }

            //The AI has passed all checks and can trigger an attack.
            return true;
        }

        public static void CheckAttackHeight(EmeraldSystem EmeraldComponent)
        {
            if (!EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility) return;

            //This puts a height cap on the MeleeAbility to only allow it to trigger an attack if the target is within the MaxAttackHeight range.
            MeleeAbility m_MeleeAbility = EmeraldComponent.CombatComponent.CurrentEmeraldAIAbility as MeleeAbility;

            if (m_MeleeAbility && EmeraldComponent.CombatTarget)
            {
                float heightDifference = Mathf.Abs(EmeraldComponent.transform.position.y - EmeraldComponent.CombatTarget.position.y);
                if (heightDifference > m_MeleeAbility.MeleeSettings.MaxAttackHeight) EmeraldComponent.CombatComponent.TargetOutOfHeightRange = true;
                else EmeraldComponent.CombatComponent.TargetOutOfHeightRange = false;
            }
            else
            {
                EmeraldComponent.CombatComponent.TargetOutOfHeightRange = false;
            }
        }

        /// <summary>
        /// Checks if the AI is within stopping distance of its current target (using Nav Mesh remainingDistance).
        /// </summary>
        static bool WithinStoppingDistanceOfTarget(EmeraldSystem EmeraldComponent)
        {
            return (EmeraldComponent.m_NavMeshAgent.remainingDistance <= EmeraldComponent.m_NavMeshAgent.stoppingDistance);
        }

        /// <summary>
        /// Checks if the AI is within stopping distance of its current target (using Vector3.Distance).
        /// </summary>
        static bool WithinDistanceOfTarget(EmeraldSystem EmeraldComponent)
        {
            return (EmeraldComponent.CombatComponent.DistanceFromTarget <= EmeraldComponent.m_NavMeshAgent.stoppingDistance);
        }

        /// <summary>
        /// Returns the height between the AI and its current target (only calculating using the Y axis).
        /// </summary>
        public static float GetTargetHeight (EmeraldSystem EmeraldComponent)
        {
            Vector3 m_TargetPos = EmeraldComponent.CombatTarget.position;
            m_TargetPos.x = 0;
            m_TargetPos.z = 0;
            Vector3 m_CurrentPos = EmeraldComponent.transform.position;
            m_CurrentPos.x = 0;
            m_CurrentPos.z = 0;
            return Vector3.Distance(m_TargetPos, m_CurrentPos);
        }

        /// <summary>
        /// Generate the probability for the Random Pick Type.
        /// </summary>
        public static float GenerateProbability(float[] probs)
        {
            float total = 0;

            foreach (float elem in probs)
            {
                total += elem;
            }

            float randomPoint = Random.value * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i;
                }
                else
                {
                    randomPoint -= probs[i];
                }
            }
            return probs.Length - 1;
        }

        /// <summary>
        /// Returns the current angle from the passed Transform.
        /// </summary>
        public static float TransformAngle(EmeraldSystem EmeraldComponent, Transform Target)
        {
            if (Target == null)
                return 180;

            Vector3 Direction = new Vector3(Target.position.x, 0, Target.position.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
            float angle = Vector3.Angle(EmeraldComponent.transform.forward, Direction);
            float RotationDifference = EmeraldComponent.transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            float AdjustedAngle = Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
            return AdjustedAngle;
        }

        /// <summary>
        /// Returns the current angle from the AI's target.
        /// </summary>
        public static float TargetAngle(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatTarget == null)
                return 360;
            Vector3 Direction = new Vector3(EmeraldComponent.CombatTarget.position.x, 0, EmeraldComponent.CombatTarget.position.z) - new Vector3(EmeraldComponent.transform.position.x, 0, EmeraldComponent.transform.position.z);
            float angle = Vector3.Angle(EmeraldComponent.transform.forward, Direction);
            float RotationDifference = EmeraldComponent.transform.localEulerAngles.x;
            RotationDifference = (RotationDifference > 180) ? RotationDifference - 360 : RotationDifference;
            return Mathf.Abs(angle) - Mathf.Abs(RotationDifference);
        }

        /// <summary>
        /// Gets the distance between this AI and its current target.
        /// </summary>
        public static float GetDistanceFromTarget(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.CombatTarget == null) return 0;

            Vector3 CurrentTargetPos = EmeraldComponent.CombatTarget.position;
            CurrentTargetPos.y = 0;
            Vector3 CurrentPos = EmeraldComponent.transform.position;
            CurrentPos.y = 0;
            return Vector3.Distance(CurrentTargetPos, CurrentPos);
        }

        /// <summary>
        /// Gets the distance between this AI and its current look at target.
        /// </summary>
        public static float GetDistanceFromLookTarget(EmeraldSystem EmeraldComponent)
        {
            if (EmeraldComponent.LookAtTarget == null) return 0;

            Vector3 CurrentTargetPos = EmeraldComponent.LookAtTarget.position;
            CurrentTargetPos.y = 0;
            Vector3 CurrentPos = EmeraldComponent.transform.position;
            CurrentPos.y = 0;
            return Vector3.Distance(CurrentTargetPos, CurrentPos);
        }
    }
}