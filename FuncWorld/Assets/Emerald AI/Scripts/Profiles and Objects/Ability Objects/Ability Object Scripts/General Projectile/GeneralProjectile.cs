using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;
using UnityEngine.Audio;

namespace EmeraldAI
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Rigidbody))]
    public class GeneralProjectile : MonoBehaviour, IAvoidable
    {
        #region Variables
        GeneralProjectileAbility CurrentAbilityData;
        EmeraldSystem EmeraldComponent;
        Transform CurrentTarget;
        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }
        Vector3 InitialTargetPosition;
        ICombat StartingICombat;
        AudioSource m_AudioSource;
        GameObject m_SoundEffect;
        Collider m_Collider;
        SphereCollider m_SphereCollider;
        Rigidbody m_Rigidbody;
        GameObject Owner;
        List<Collider> IgnoredColliders = new List<Collider>();
        float StartTime;
        float HomingTimer;
        bool Initialized;
        bool MinHomingDistMet;
        float TargetAngle;

        [SerializeField]
        public List<ProjectileEffectsClass> m_ProjectileObjects = new List<ProjectileEffectsClass>();
        #endregion

        /// <summary>
        /// Used to initialize the needed components and settings the first time the projectile is used.
        /// </summary>
        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.loop = true;
            m_AudioSource.rolloffMode = AudioRolloffMode.Linear;
            m_AudioSource.spatialBlend = 1;
            m_AudioSource.maxDistance = 20;

            //If a collider already exists on the projectile object, use it as the projectile's collider.
            m_Collider = GetComponent<Collider>();
            if (m_Collider != null)
            {
                m_Collider.isTrigger = true;
                m_Collider.enabled = false;
            }
            //If not, create a SphereCollider.
            else
            {
                m_Collider = gameObject.AddComponent<SphereCollider>();
                m_SphereCollider = gameObject.GetComponent<SphereCollider>();
                m_Collider.isTrigger = true;
                m_Collider.enabled = false;
            }  

            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.useGravity = true;
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            m_SoundEffect = Resources.Load("Emerald Sound") as GameObject;

            //Go through all effect objects on Awake and cache them to be used through the Projectile Module.
            m_ProjectileObjects.Add(new ProjectileEffectsClass(gameObject.GetComponent<ParticleSystemRenderer>(), gameObject));
            
            foreach (Transform child in transform)
            {
                m_ProjectileObjects.Add(new ProjectileEffectsClass(child.GetComponent<ParticleSystemRenderer>(), child.gameObject));
            }
        }

        /// <summary>
        /// Initialize the projectile with the passed information.
        /// </summary>
        /// <param name="owner">The owner of this projectile.</param>
        /// <param name="currentTarget">The current target for this projectile.</param>
        /// <param name="abilityData">The current ability data for this projectile.</param>
        public void Initialize (GameObject owner, Transform currentTarget, GeneralProjectileAbility abilityData)
        {
            StartCoroutine(InitializeInternal(owner, currentTarget, abilityData));
        }

        IEnumerator InitializeInternal (GameObject owner, Transform currentTarget, GeneralProjectileAbility abilityData)
        {
            Owner = owner;
            CurrentTarget = currentTarget;
            if (CurrentTarget != null)
            {
                StartingICombat = CurrentTarget.GetComponent<ICombat>();
                InitialTargetPosition = StartingICombat.DamagePosition();
            }

            EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            CurrentAbilityData = abilityData;
            m_Collider.enabled = false; //Don'transform enable colliders until after they have launched

            GetLBDColliders(); //Get a reference to the Owner's LBD component so internal colliders can be ignored.

            if (m_SphereCollider != null)
            {
                m_SphereCollider.radius = CurrentAbilityData.ColliderSettings.ColliderRadius;
                m_SphereCollider.center = Vector3.forward * CurrentAbilityData.ColliderSettings.ZOffet;
            }

            gameObject.layer = CurrentAbilityData.ColliderSettings.ProjectileLayer;
            Initialized = false;
            MinHomingDistMet = false;
            HomingTimer = 0;

            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0) SetEffectsState(true); //Enable the specified effects by comparing the names from the EffectsToDisable list.
            InitializeProjectile(); //Intialize the projectile's settings.

            yield return new WaitForSeconds(CurrentAbilityData.ProjectileSettings.LaunchProjectileDelay);

            StartTime = Time.time;
            m_Collider.enabled = true; //Enable the collider now this is has launched
            if (CurrentAbilityData.ProjectileSettings.TravelSound != null) m_AudioSource.PlayOneShot(CurrentAbilityData.ProjectileSettings.TravelSound);
            CurrentAbilityData.ProjectileSettings.SpawnLaunchProjectileEffect(Owner, transform.position);
            Initialized = true;
        }

        void GetLBDColliders()
        {
            //Get a reference to the Owner's LBD component so internal colliders can be ignored.
            LocationBasedDamage LBDComponent = Owner.GetComponent<LocationBasedDamage>();
            if (LBDComponent)
            {
                IgnoredColliders.Clear();
                for (int i = 0; i < LBDComponent.ColliderList.Count; i++)
                {
                    IgnoredColliders.Add(LBDComponent.ColliderList[i].ColliderObject);
                }
            }
        }

        void InitializeProjectile ()
        {
            if (CurrentAbilityData.SpreadSettings.Enabled && CurrentAbilityData.SpreadSettings.SpreadType == EmeraldAI.AbilityData.SpreadTypes.HorizontalRadius)
                transform.position += transform.forward * CurrentAbilityData.SpreadSettings.SpawnDistance;

            TargetAngle = EmeraldCombatManager.TransformAngle(EmeraldComponent, CurrentTarget);

            if (TargetAngle < (CurrentAbilityData.GeneralProjectileSettings.ProjectileMaxLaunchAngle / 2f))
            {
                if (!CurrentAbilityData.SpreadSettings.Enabled)
                {
                    transform.LookAt(InitialTargetPosition);
                }
            }
            else
            {
                transform.LookAt(transform.position + Owner.transform.forward);
            }

            CurrentAbilityData.ProjectileSettings.SpawnEffect(Owner, transform.position);
        }

        /// <summary>
        /// Used to move and track the time since the projectile has been spawned.
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); //Track the time since the projectile has been active.
            MoveProjectile(); //Move the projectile towards its current target.
        }

        /// <summary>
        /// Track the time since the projectile has been active. Once the time ProjectileTimeoutSeconds 
        /// has been met, despawn the projectile and spawn an impact effect from its current position.
        /// </summary>
        void ProjectileTimeout ()
        {
            if (!Initialized) return;

            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.ProjectileSettings.ProjectileTimeoutSeconds && m_Collider.enabled)
            {
                CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position);
                this.enabled = false;
                EmeraldObjectPool.Despawn(gameObject);
            }
        }

        /// <summary>
        /// Move the projectile towards its current target, but only if the ability is initialized.
        /// </summary>
        void MoveProjectile ()
        {
            if (Initialized)
            {
                var step = CurrentAbilityData.GeneralProjectileSettings.ProjectileSpeed * Time.deltaTime;

                if (CurrentTarget != null)
                {
                    float DistFromTarget = (Vector3.Distance(transform.position, StartingICombat.DamagePosition()));

                    if (!MinHomingDistMet && DistFromTarget < CurrentAbilityData.HomingSettings.MinimumHomingDistance) MinHomingDistMet = true;

                    if (CurrentAbilityData.HomingSettings.Enabled && HomingTimer < CurrentAbilityData.HomingSettings.HomingSeconds && !MinHomingDistMet && CurrentTarget.transform.localScale != Vector3.one * 0.003f && TargetAngle < (CurrentAbilityData.GeneralProjectileSettings.ProjectileMaxLaunchAngle / 2f))
                    {
                        Vector3 ForwardMovement = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
                        transform.position = ForwardMovement;
                        float DistFormOwner = Vector3.Distance(Owner.transform.position, transform.position);

                        if (DistFormOwner > 1f || DistFromTarget < 1)
                        {
                            HomingTimer += Time.deltaTime;
                            RotateTowardsTarget();
                        }
                    }
                    else
                    {
                        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
                    }
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
                }
            }
        }

        /// <summary>
        /// Rotate towards the projectile's Current Target's position.
        /// </summary>
        void RotateTowardsTarget ()
        {
            //Determine which direction to rotate towards
            Vector3 targetDirection = StartingICombat.DamagePosition() - transform.position;
            //Vector3 targetDirection = CurrentTarget.position - transform.position;

            //The step size is equal to speed times frame time.
            float singleStep = Time.deltaTime * CurrentAbilityData.HomingSettings.HomingSpeed;

            //Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

            //Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        /// <summary>
        /// Handles the impact functionality, given the collision object is not this or another projectile.
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (!this.enabled) return; //Only allow trigger collisions to work if the script is active
            if (((1 << other.gameObject.layer) & CurrentAbilityData.ColliderSettings.CollidableLayers) != 0 && other.gameObject != Owner && !IgnoredColliders.Contains(other)) Impact(other.gameObject);
        }

        /// <summary>
        /// Handles all impact related functionality.
        /// </summary>
        void Impact (GameObject TargetHit)
        {
            m_Collider.enabled = false; //Disable the ability's collider so unintended collisions don'transform happen.
            Initialized = false; //Disable initialization so the projectile stops operating.
            CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position); //Spawns the ability's impact effect and impact sound.
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.ColliderSettings.CollisionTimeout); //Despawn the projectile according to its CollisionTimeout. This gives the projectile extra time to finish before being despawned.
            DamageTarget(TargetHit); //Damages the projectile's Target. If a LocationBasedDamageArea is detected, damage it. If not, damage the target's IDamageable component.

            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0)
            {
                if (!CurrentAbilityData.GeneralProjectileSettings.AttachToTarget) SetEffectsState(false); //Disables the specified effects by comparing the names from the EffectsToDisable list.
                else if (TargetHit.activeSelf) StartCoroutine(SetEffectsStateDelay(false)); //Disables the specified effects by comparing the names from the EffectsToDisable list (with delay).
            }
        }

        /// <summary>
        /// Delay the SetEffectsState call.
        /// </summary>
        IEnumerator SetEffectsStateDelay (bool State)
        {
            if (!State) yield return new WaitForSeconds(1);
            SetEffectsState(State);
        }

        void OnDisable ()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Sets the specified effects' state by comparing the names from the EffectsToDisable list.
        /// </summary>
        void SetEffectsState (bool State)
        {
            for (int i = 0; i < m_ProjectileObjects.Count; i++)
            {
                for (int j = 0; j < CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count; j++)
                {
                    if (m_ProjectileObjects[i].EffectObject.name == CurrentAbilityData.ProjectileSettings.EffectsToDisable[j])
                    {
                        if (m_ProjectileObjects[i].EffectParticle != null) m_ProjectileObjects[i].EffectParticle.enabled = State;
                        else if (m_ProjectileObjects[i].EffectObject != null && m_ProjectileObjects[i].EffectObject != this.gameObject) m_ProjectileObjects[i].EffectObject.SetActive(State);
                    }
                }
            }
        }

        /// <summary>
        /// Damages the projectile's Target. If a LocationBasedDamageArea is detected, damage it. If not, damage the target's IDamageable component.
        /// </summary>
        void DamageTarget(GameObject Target)
        {
            LocationBasedDamageArea m_LocationBasedDamageArea = Target.GetComponent<LocationBasedDamageArea>();

            //Only damage layers that are in the AI's DetectionLayerMask or if the target has a LBD component on it.
            if (!m_LocationBasedDamageArea && ((1 << Target.layer) & EmeraldComponent.DetectionComponent.DetectionLayerMask) == 0) return;

            //Return if the target is teleporting.
            if (m_LocationBasedDamageArea != null && m_LocationBasedDamageArea.EmeraldComponent.transform.localScale == Vector3.one * 0.003f || Target.transform.localScale == Vector3.one * 0.003f) return;

            var m_ICombat = Target.GetComponentInParent<ICombat>();

            //If stuns are enabled, roll for a stun
            if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
            }

            //Only cause damage if it's enabled
            if (!CurrentAbilityData.DamageSettings.Enabled) return;

            if (m_LocationBasedDamageArea == null)
            {
                var m_IDamageable = Target.GetComponent<IDamageable>();
                if (m_IDamageable != null)
                {
                    bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();
                    m_IDamageable.Damage(CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit), Owner.transform, CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                    CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, Target);
                    m_AudioSource.Stop();
                }
                else
                {
                    Debug.Log(Target.gameObject + " is missing IDamageable Component, apply one");
                }
            }
            else if (m_LocationBasedDamageArea != null)
            {
                bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();
                m_LocationBasedDamageArea.DamageArea(CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit), Owner.transform, CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce, IsCritHit);
                CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, m_ICombat.TargetTransform().gameObject);
                m_AudioSource.Stop();
            }

            if (CurrentAbilityData.GeneralProjectileSettings.AttachToTarget)
            {
                AttachToCollider(Target.transform);
            }
        }

        /// <summary>
        /// Attaches the projectile to the passed collider.
        /// </summary>
        void AttachToCollider(Transform Target)
        {
            transform.SetParent(Target);
        }

        /// <summary>
        /// Despawns the impact effect after the CollisionTimeout has been met.
        /// </summary>
        void ImpactDespawn ()
        {
            this.enabled = false; //Disable the script so it doesn'transform interfere with other objects that may use the same object through Object Pooling
            EmeraldObjectPool.Despawn(gameObject);
        }
    }
}