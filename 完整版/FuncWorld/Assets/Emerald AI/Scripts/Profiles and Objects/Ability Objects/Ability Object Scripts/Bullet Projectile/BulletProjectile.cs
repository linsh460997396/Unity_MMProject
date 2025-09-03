using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;
using System.Linq;
using UnityEngine.Audio;

namespace EmeraldAI
{
    [RequireComponent(typeof(AudioSource))]
    public class BulletProjectile : MonoBehaviour, IAvoidable
    {
        #region Variables
        BulletProjectileAbility CurrentAbilityData;
        EmeraldSystem EmeraldComponent;
        Transform CurrentTarget;
        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }
        Vector3 InitialTargetPosition;
        ICombat StartingICombat;
        AudioSource m_AudioSource;
        GameObject m_SoundEffect;
        GameObject Owner;
        List<Collider> IgnoredColliders = new List<Collider>();
        float StartTime;
        bool Initialized;
        float TargetAngle;
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
            m_SoundEffect = Resources.Load("Emerald Sound") as GameObject;
        }

        /// <summary>
        /// Initialize the projectile with the passed information.
        /// </summary>
        /// <param name="owner">The owner of this projectile.</param>
        /// <param name="currentTarget">The current target for this projectile.</param>
        /// <param name="abilityData">The current ability data for this projectile.</param>
        public void Initialize (GameObject owner, Transform currentTarget, BulletProjectileAbility abilityData)
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

            GetLBDColliders(); //Get a reference to the Owner's LBD component so internal colliders can be ignored.

            Initialized = false;
            InitializeProjectile(); //Intialize the projectile's settings.

            StartTime = Time.time;
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
            if (EmeraldComponent.CombatComponent.TargetAngle < EmeraldComponent.MovementComponent.CombatAngleToTurn) transform.LookAt(InitialTargetPosition);
            Initialized = true;
            StartCoroutine(MoveProjectile());
        }

        IEnumerator MoveProjectile()
        {
            bool Collided = false;
            float CollisionCheckTimer = 0;
            Vector3 EndPosition = Vector3.zero;
            Vector3 Accuracy = new Vector3(Random.Range(-CurrentAbilityData.BulletProjectileSettings.BulletSpreadX, CurrentAbilityData.BulletProjectileSettings.BulletSpreadX) * 0.001f, 
                Random.Range(-CurrentAbilityData.BulletProjectileSettings.BulletSpreadY, CurrentAbilityData.BulletProjectileSettings.BulletSpreadY) * 0.001f, 0);

            while (!Collided)
            {
                var step = CurrentAbilityData.BulletProjectileSettings.BulletSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward + Accuracy, step);
                CollisionCheckTimer += Time.deltaTime;

                //Only check for collisions every tick according to CollisionCheckSpeed.
                if (CollisionCheckTimer >= CurrentAbilityData.BulletProjectileSettings.CollisionCheckSpeed)
                {
                    //In order to have reliable collisions for fast moving bullets, a raycast needs to be cast continiously while it travels towards its target.
                    //It has a forward offset of 0.65 units, which allows for reliable collision detection.
                    RaycastHit hit;
                    //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 0.5f);
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 0.65f, ~CurrentAbilityData.BulletProjectileSettings.IgnoreLayers))
                    {
                        //When the raycast hits something, call the impact function to check to see if the collider can take damage. Store the end hit.point to be used below.
                        if (hit.collider != null)
                        {
                            EndPosition = hit.point + (transform.TransformDirection(Vector3.forward) * 0.1f);
                            Impact(hit.collider.gameObject, hit.point, hit.normal);
                            Collided = true;
                        }
                    }

                    CollisionCheckTimer = 0;
                }

                yield return null;
            }

            Vector3 StartingPosition = transform.position;
            float t = 0;
            bool Complete = false;

            while (!Complete)
            {
                //Since the raycast is stopped 0.5 units in front of the hit collider, finish up the rest of the distance
                //for the bullet to allow the bullet trail to reach the impact position.
                float Distance = Vector3.Distance(transform.position, EndPosition);
                t += Time.deltaTime * CurrentAbilityData.BulletProjectileSettings.BulletSpeed;
                transform.position = Vector3.Lerp(StartingPosition, EndPosition, t);

                if (Distance <= 0)
                {
                    Complete = true;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Used to move and track the time since the projectile has been spawned.
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); //Track the time since the projectile has been active.
        }

        /// <summary>
        /// Track the time since the projectile has been active. Once the time ProjectileTimeoutSeconds 
        /// has been met, despawn the projectile and spawn an impact effect from its current position.
        /// </summary>
        void ProjectileTimeout ()
        {
            if (!Initialized) return;

            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.BulletProjectileSettings.BulletObjectTimeout)
            {
                this.enabled = false;
                EmeraldObjectPool.Despawn(gameObject);
            }
        }

        /// <summary>
        /// Handles all impact related functionality.
        /// </summary>
        void Impact (GameObject TargetHit, Vector3 HitPosition, Vector3 HitNormal)
        {
            if (!this.enabled) return; //Only allow trigger collisions to work if the script is active

            Initialized = false; //Disable initialization so the projectile stops operating.
            BulletImpact(TargetHit, HitPosition, HitNormal);
            DamageTarget(TargetHit); //Damages the projectile's Target. If a LocationBasedDamageArea is detected, damage it. If not, damage the target's IDamageable component.
            Invoke(nameof(ImpactDespawn), CurrentAbilityData.BulletProjectileSettings.BulletCollisionTimeout);
        }

        void BulletImpact (GameObject TargetHit, Vector3 HitPosition, Vector3 HitNormal)
        {
            if (CurrentAbilityData.BulletProjectileSettings.BulletImpactData.Count == 0)
            {
                CurrentAbilityData.BulletProjectileSettings.SpawnDefaultBulletImpact(TargetHit, HitPosition, HitNormal);
                return;
            }

            for (int i = 0; i < CurrentAbilityData.BulletProjectileSettings.BulletImpactData.Count; i++)
            {
                if (TargetHit.CompareTag(CurrentAbilityData.BulletProjectileSettings.BulletImpactData[i].SurfaceTag))
                {
                    CurrentAbilityData.BulletProjectileSettings.SpawnBulletImpact(TargetHit, CurrentAbilityData.BulletProjectileSettings.BulletImpactData[i], HitPosition, HitNormal);
                    return;
                }
            }

            CurrentAbilityData.BulletProjectileSettings.SpawnDefaultBulletImpact(TargetHit, HitPosition, HitNormal);
        }

        void OnDisable ()
        {
            StopAllCoroutines();
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
        /// Despawns the impact effect after the CollisionTimeout has been met.
        /// </summary>
        void ImpactDespawn ()
        {
            this.enabled = false; //Disable the script so it doesn'transform interfere with other objects that may use the same object through Object Pooling
            EmeraldObjectPool.Despawn(gameObject);
        }
    }
}