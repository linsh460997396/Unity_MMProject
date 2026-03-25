using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Rigidbody))]
    public class GroundProjectile : MonoBehaviour, IAvoidable
    {
        #region Variables
        GroundProjectileAbility CurrentAbilityData;
        Transform CurrentTarget;
        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }
        Vector3 InitialTargetPosition;
        ICombat StartingICombat;
        EmeraldSystem EmeraldComponent;
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
        float TravelDistance;
        Vector3 StartingPosition;
        bool MinHomingDistMet;
        Vector3 SurfaceNormal;
        Quaternion qTarget;
        Quaternion qGround;
        Vector3 TargetDirection;

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
        public void Initialize(GameObject owner, Transform currentTarget, GroundProjectileAbility abilityData)
        {
            StartCoroutine(InitializeInternal(owner, currentTarget, abilityData));
        }

        IEnumerator InitializeInternal(GameObject owner, Transform currentTarget, GroundProjectileAbility abilityData)
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
            m_Collider.enabled = true;

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

            if (m_SphereCollider != null)
            {
                m_SphereCollider.radius = abilityData.ColliderSettings.ColliderRadius;
                m_SphereCollider.center = Vector3.forward * abilityData.ColliderSettings.ZOffet;
            }

            gameObject.layer = CurrentAbilityData.ColliderSettings.ProjectileLayer;
            CurrentAbilityData.GroundProjectileSettings.AllignmentLayers &= ~(1 << CurrentAbilityData.ColliderSettings.ProjectileLayer); //Remove the ProjectileLayer if it was mistakenly added to the AllignmentLayers
            Initialized = false;
            MinHomingDistMet = false;
            HomingTimer = 0;
            StartTime = Time.time;

            //Reset the rotation and target variables (not doing so causes the starting direction to be incorrect).
            TargetDirection = Vector3.zero;
            qGround = new Quaternion();
            qTarget = new Quaternion();

            SetEffectsState(true); //Disables the specified effects by comparing the names from the EffectsToDisable list.
            InitializeProjectile(); //Initialize the projectile

            yield return new WaitForSeconds(CurrentAbilityData.ProjectileSettings.LaunchProjectileDelay);

            if (CurrentAbilityData.ProjectileSettings.TravelSound != null) m_AudioSource.PlayOneShot(CurrentAbilityData.ProjectileSettings.TravelSound);
            CurrentAbilityData.ProjectileSettings.SpawnLaunchProjectileEffect(Owner, transform.position);
            Initialized = true;
        }

        void InitializeProjectile()
        {
            CurrentAbilityData.ProjectileSettings.SpawnEffect(Owner, transform.position);
            StartingPosition = transform.position;
        }

        /// <summary>
        /// Used to move and track the time since the projectile has been spawned.
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); //Track the time since the projectile has been active.
            MoveGroundProjectile(); //Move the projectile towards its current target.
        }

        /// <summary>
        /// Track the time since the projectile has been active. Once the time ProjectileTimeoutSeconds 
        /// has been met, despawn the projectile and spawn an impact effect from its current position.
        /// </summary>
        void ProjectileTimeout()
        {
            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.ProjectileSettings.ProjectileTimeoutSeconds && m_Collider.enabled)
            {
                StopGroundProjectile();
            }
        }

        /// <summary>
        /// Move the projectile towards its current target, but only if the ability is initialized.
        /// </summary>
        void MoveGroundProjectile()
        {
            if (Initialized)
            {
                TravelDistance = Vector3.Distance(StartingPosition, transform.position);

                if (TravelDistance < CurrentAbilityData.GroundProjectileSettings.MaxTravelDistance || CurrentTarget == null)
                {
                    var step = CurrentAbilityData.GroundProjectileSettings.ProjectileSpeed * Time.deltaTime;
                    float DistFromTarget = (Vector3.Distance(transform.position, StartingICombat.DamagePosition()));
                    float DistFormOwner = Vector3.Distance(Owner.transform.position, transform.position);
                    if (!MinHomingDistMet && DistFromTarget < CurrentAbilityData.HomingSettings.MinimumHomingDistance) MinHomingDistMet = true;

                    if (CurrentAbilityData.HomingSettings.Enabled && HomingTimer < CurrentAbilityData.HomingSettings.HomingSeconds && !MinHomingDistMet && CurrentTarget.transform.localScale != Vector3.one * 0.003f)
                    {
                        if (DistFormOwner > 2f || DistFromTarget < 1)
                        {
                            HomingTimer += Time.deltaTime;
                            GetSurfaceNormal();
                            TargetDirection = StartingICombat.DamagePosition() - transform.position;
                            TargetDirection.y = 0;
                            SetMovementAndRotation(step);
                        }
                        else
                        {
                            GetSurfaceNormal();
                            SetMovementAndRotation(step); 
                        }
                    }
                    else
                    {
                        GetSurfaceNormal();
                        SetMovementAndRotation(step);
                    }
                }
                else
                {
                    StopGroundProjectile();
                }
            }
        }

        /// <summary>
        /// Sets the movement and rotation for the ground projectiles. The target source is only used while the ability is actively homing.
        /// </summary>
        void SetMovementAndRotation (float step)
        {
            qGround = Quaternion.Slerp(qGround, Quaternion.FromToRotation(Vector3.up, SurfaceNormal), Time.deltaTime * CurrentAbilityData.GroundProjectileSettings.AlignmentSpeed);
            if (TargetDirection != Vector3.zero) qTarget = Quaternion.Slerp(qTarget, Quaternion.LookRotation(TargetDirection, Vector3.up), Time.deltaTime * CurrentAbilityData.HomingSettings.HomingSpeed);
            transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, step);
            transform.rotation = Quaternion.Slerp(transform.rotation, qGround * qTarget, Time.deltaTime * CurrentAbilityData.GroundProjectileSettings.AlignmentSpeed);

            float SurfaceAngle = Vector3.Angle(SurfaceNormal, Vector3.up);
            if (SurfaceAngle >= CurrentAbilityData.GroundProjectileSettings.KillAngle) StopGroundProjectile();
        }

        /// <summary>
        /// Return the current surface normal by casting a ray from the center of the projectile.
        /// </summary>
        public Vector3 GetSurfaceNormal()
        {
            RaycastHit HitDown;
            if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out HitDown, 2f, CurrentAbilityData.GroundProjectileSettings.AllignmentLayers))
            {
                if (HitDown.transform != this.transform)
                {
                    float MaxNormalAngle = CurrentAbilityData.GroundProjectileSettings.MaxAlignmentAngle * 0.01f;
                    transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, HitDown.point.y + CurrentAbilityData.GroundProjectileSettings.HeightOffset, Time.deltaTime * 20), transform.position.z);
                    SurfaceNormal = HitDown.normal;
                    SurfaceNormal.x = Mathf.Clamp(SurfaceNormal.x, -MaxNormalAngle, MaxNormalAngle);
                    SurfaceNormal.z = Mathf.Clamp(SurfaceNormal.z, -MaxNormalAngle, MaxNormalAngle);
                }
            }
            else
            {
                //Stop the projectile if there's no detectable ground. This is to prevent moving on cliffs or edges. 
                StopGroundProjectile();
            }

            RaycastHit HitForward;
            if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), transform.forward, out HitForward, CurrentAbilityData.ColliderSettings.ColliderRadius, CurrentAbilityData.GroundProjectileSettings.AllignmentLayers))
            {
                if (HitForward.transform != this.transform)
                {
                    float SurfaceAngle = Vector3.Angle(HitForward.normal, Vector3.up);
                    if (SurfaceAngle >= CurrentAbilityData.GroundProjectileSettings.KillAngle)
                    {
                        //CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position); //Spawns the ability's impact effect and impact sound.
                        StopGroundProjectile();
                    }
                }
            }

            return SurfaceNormal;
        }

        /// <summary>
        /// Handles the impact functionality, given the collision object is not this or another projectile.
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & CurrentAbilityData.ColliderSettings.CollidableLayers) != 0 && other.gameObject != Owner && !IgnoredColliders.Contains(other)) Impact(other.gameObject);
        }

        /// <summary>
        /// Handles all impact related functionality.
        /// </summary>
        void Impact(GameObject TargetHit)
        {
            m_Collider.enabled = false; //Disable the ability's collider so unintended collisions don'transform happen.
            Initialized = false; //Disable initialization so the projectile stops operating.
            CurrentAbilityData.ProjectileSettings.SpawnImpactEffect(Owner, transform.position); //Spawns the ability's impact effect and impact sound.
            DamageTarget(TargetHit); //Damages the projectile's Target. If a LocationBasedDamageArea is detected, damage it. If not, damage the target's IDamageable component.
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.ColliderSettings.CollisionTimeout); //Despawn the projectile according to its CollisionTimeout. This gives the projectile extra time to finish before being despawned.
            SetEffectsState(false); //Disables the specified effects by comparing the names from the EffectsToDisable list.
        }

        void StopGroundProjectile ()
        {
            m_Collider.enabled = false; //Disable the ability's collider so unintended collisions don'transform happen.
            Initialized = false; //Disable initialization so the projectile stops operating.
            m_AudioSource.Stop();
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.ColliderSettings.CollisionTimeout); //Despawn the projectile according to its CollisionTimeout. This gives the projectile extra time to finish before being despawned.
            SetEffectsState(false); //Disables the specified effects by comparing the names from the EffectsToDisable list.
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
        }

        /// <summary>
        /// Sets the specified effects' state by comparing the names from the EffectsToDisable list.
        /// </summary>
        void SetEffectsState(bool State)
        {
            if (CurrentAbilityData.ProjectileSettings.EffectsToDisable.Count > 0)
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
        }

        /// <summary>
        /// Despawns the impact effect after the CollisionTimeout has been met.
        /// </summary>
        void ImpactDespawn()
        {
            EmeraldObjectPool.Despawn(gameObject);
        }
    }
}