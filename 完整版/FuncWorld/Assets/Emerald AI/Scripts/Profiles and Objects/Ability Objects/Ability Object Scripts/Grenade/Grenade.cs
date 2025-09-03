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
    public class Grenade : MonoBehaviour, IAvoidable
    {
        #region Variables
        GrenadeAbility CurrentAbilityData;
        EmeraldSystem EmeraldComponent;
        Transform CurrentTarget;
        public Transform AbilityTarget { get => CurrentTarget; set => CurrentTarget = value; }
        Vector3 InitialTargetPosition;
        AudioSource m_AudioSource;
        GameObject m_SoundEffect;
        Rigidbody m_Rigidbody;
        GameObject Owner;
        Collider GrenadeCollider;
        float StartTime;
        bool Initialized;
        bool HasCollided;
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
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.drag = 0.1f;
            m_Rigidbody.angularDrag = 0.05f;
            GrenadeCollider = GetComponent<Collider>();
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }

        /// <summary>
        /// Initialize the projectile with the passed information.
        /// </summary>
        /// <param name="owner">The owner of this projectile.</param>
        /// <param name="currentTarget">The current target for this projectile.</param>
        /// <param name="abilityData">The current ability data for this projectile.</param>
        public void Initialize (GameObject owner, Transform currentTarget, GrenadeAbility abilityData)
        {
            Owner = owner;
            CurrentTarget = currentTarget;
            EmeraldComponent = Owner.GetComponent<EmeraldSystem>();
            CurrentAbilityData = abilityData;
            GrenadeCollider.enabled = false;
            gameObject.layer = CurrentAbilityData.GrenadeSettings.GrenadeLayer;
            Invoke(nameof(EnableCollider), 0.1f);
            Initialized = false;
            HasCollided = false;
            InitializeProjectile(); //Intialize the projectile's settings.
            StartTime = Time.time;
        }

        /// <summary>
        /// Enables the grenade's collider 0.1f seconds after being thrown. This gives the grenade time to leave the owner's hand so it doesn'transform interfere with its colliders.
        /// </summary>
        void EnableCollider ()
        {
            GrenadeCollider.enabled = true;
        }

        void InitializeProjectile ()
        {
            InitialTargetPosition = CurrentTarget.transform.position;
            transform.LookAt(InitialTargetPosition);
            Initialized = true;
            if (CurrentAbilityData.GrenadeSettings.ThrowSound) m_AudioSource.PlayOneShot(CurrentAbilityData.GrenadeSettings.ThrowSound);
            Vector3 TargetDirection = InitialTargetPosition - transform.position;
            float Distance = Vector3.Distance(InitialTargetPosition, transform.position);
            m_Rigidbody.velocity = new Vector3(0f, 0f, 0f);
            m_Rigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
            float RandomizedDepth = Random.Range(0.85f, 1f);
            float ThrowHeightOffset = Mathf.Lerp(1f, 0.5f, CurrentAbilityData.GrenadeSettings.ThrowHeight / 10f);
            m_Rigidbody.AddForce((TargetDirection.normalized * Distance * (ThrowHeightOffset * RandomizedDepth)) + (Vector3.up * CurrentAbilityData.GrenadeSettings.ThrowHeight), ForceMode.Impulse);
        }

        /// <summary>
        /// Used to move and track the time since the projectile has been spawned.
        /// </summary>
        void Update()
        {
            ProjectileTimeout(); //Track the time since the projectile has been active.
        }

        void FixedUpdate ()
        {
            if (!HasCollided && CurrentAbilityData.GrenadeSettings.RotateOnThrow)
            {
                Quaternion deltaRotation = Quaternion.Euler(new Vector3(300, 20, 20) * Time.fixedDeltaTime);
                m_Rigidbody.MoveRotation(m_Rigidbody.rotation * deltaRotation);
            }
        }

        /// <summary>
        /// Track the time since the projectile has been active. Once the time ProjectileTimeoutSeconds 
        /// has been met, despawn the projectile and spawn an impact effect from its current position.
        /// </summary>
        void ProjectileTimeout ()
        {
            if (!Initialized) return;

            float TimeAlive = Time.time - StartTime;
            if (TimeAlive > CurrentAbilityData.GrenadeSettings.ExplosionTime)
            {
                Explode();
                Initialized = false;
            }
        }

        void OnCollisionEnter (Collision col)
        {
            if (!HasCollided) HasCollided = true;

            m_AudioSource.pitch = Random.Range(0.85f, 1.15f);
            m_AudioSource.volume = Random.Range(0.6f, 1f);

            if (col.relativeVelocity.magnitude > 0.4f)
                m_AudioSource.PlayOneShot(CurrentAbilityData.GrenadeSettings.ImpactSoundsList[Random.Range(0, CurrentAbilityData.GrenadeSettings.ImpactSoundsList.Count)]);
        }

        /// <summary>
        /// Call this function when you want to damage surrounding AI based on the set public variables within this script. 
        /// </summary>
        public void Explode()
        {
            //Create explosion effect
            CurrentAbilityData.GrenadeSettings.ExplodeGrenade(EmeraldComponent.gameObject, transform.position);

            Invoke(nameof(Despawn), 0.11f);

            List<Collider> TargetsInRange = Physics.OverlapSphere(transform.position, CurrentAbilityData.GrenadeSettings.ExplosionRadius, EmeraldComponent.DetectionComponent.DetectionLayerMask).ToList();
            TargetsInRange.Remove(Owner.GetComponent<Collider>()); //Remove the owner's collider if it happens to be detected.

            for (int i = 0; i < TargetsInRange.Count; i++)
            {
                //Only damage targets that the Owner has an Enemy Relation Type with. Ignore this setting if CanDamageFriendlies is enabled.
                if (CurrentAbilityData.GrenadeSettings.CanDamageFriendlies || EmeraldAPI.Faction.GetTargetFactionRelation(EmeraldComponent, TargetsInRange[i].transform) == "Enemy")
                {
                    ICombat m_ICombat = TargetsInRange[i].GetComponent<ICombat>();
                    DamageTarget(TargetsInRange[i].gameObject, m_ICombat);
                }
            }

            List<Collider> RigidbodiesInRange = Physics.OverlapSphere(transform.position, CurrentAbilityData.GrenadeSettings.ExplosionRadius, CurrentAbilityData.GrenadeSettings.RigidbodyLayers).ToList();
            RigidbodiesInRange.Remove(Owner.GetComponent<Collider>()); //Remove the owner's collider if it happens to be detected.

            for (int i = 0; i < RigidbodiesInRange.Count; i++)
            {
                Rigidbody TargetRigidbody = RigidbodiesInRange[i].GetComponent<Rigidbody>();
                if (TargetRigidbody != null) StartCoroutine(AddRagdollForce(TargetRigidbody));
            }
        }

        /// <summary>
        /// Damages each target within range of the explosion, given that it has a IDamageable.
        /// </summary>
        void DamageTarget(GameObject Target, ICombat m_ICombat)
        {
            if (Target.transform.localScale == Vector3.one * 0.003f) return;

            if (CurrentAbilityData.StunnedSettings.Enabled && CurrentAbilityData.StunnedSettings.RollForStun())
            {
                if (m_ICombat != null) m_ICombat.TriggerStun(CurrentAbilityData.StunnedSettings.StunLength);
            }

            //Only cause damage if it's enabled
            if (!CurrentAbilityData.DamageSettings.Enabled) return;

            var m_IDamageable = Target.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                bool IsCritHit = CurrentAbilityData.DamageSettings.GenerateCritHit();
                int DamageMitigation = Mathf.RoundToInt((1f - Vector3.Distance(Target.transform.position, transform.position) / CurrentAbilityData.GrenadeSettings.ExplosionRadius) * CurrentAbilityData.DamageSettings.GenerateDamage(IsCritHit));
                m_IDamageable.Damage(DamageMitigation, transform, 0, IsCritHit);
                CurrentAbilityData.DamageSettings.DamageTargetOverTime(CurrentAbilityData, CurrentAbilityData.DamageSettings, Owner, Target);

                if (m_IDamageable.Health <= 0)
                {
                    EmeraldSystem DetectedEmeraldAgent = Target.GetComponent<EmeraldSystem>();
                    if (DetectedEmeraldAgent != null && DetectedEmeraldAgent.LBDComponent != null) StartCoroutine(AddRagdollForceAI(DetectedEmeraldAgent));
                }
            }
            else
            {
                Debug.Log(Target.gameObject + " is missing a IDamageable and/or ICombat Component, apply one");
            }
        }

        IEnumerator AddRagdollForceAI(EmeraldSystem EmeraldTarget)
        {
            float t = 0;
            float ForceMitigation = Mathf.RoundToInt((1f - Vector3.Distance(EmeraldTarget.transform.position, transform.position) / CurrentAbilityData.GrenadeSettings.ExplosionRadius) * CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce) * 4f;

            Rigidbody TargetRigidbody = null;
            if (EmeraldTarget != null) TargetRigidbody = EmeraldTarget.DetectionComponent.HeadTransform.GetComponent<Rigidbody>();

            if (TargetRigidbody != null)
            {
                while (t < 0.1f)
                {
                    t += Time.fixedDeltaTime;
                    TargetRigidbody.AddForce((transform.position - EmeraldTarget.transform.position).normalized * -ForceMitigation * 1f + (Vector3.up * ForceMitigation * 1f), ForceMode.Acceleration);
                    yield return null;
                }
            }
        }

        IEnumerator AddRagdollForce(Rigidbody TargetRigidbody)
        {
            float t = 0;
            float ForceMitigation = Mathf.RoundToInt((1f - Vector3.Distance(TargetRigidbody.transform.position, transform.position) / (float)CurrentAbilityData.GrenadeSettings.ExplosionRadius * 1.5f) * CurrentAbilityData.DamageSettings.BaseDamageSettings.RagdollForce) * 4f;

            if (TargetRigidbody != null)
            {
                while (t < 0.1f)
                {
                    t += Time.fixedDeltaTime;
                    TargetRigidbody.AddForce((transform.position - TargetRigidbody.transform.position).normalized * -ForceMitigation * 0.15f + (Vector3.up * ForceMitigation * 0.15f), ForceMode.Acceleration);
                    yield return null;
                }
            }
        }

        void Despawn ()
        {
            this.enabled = false;
            EmeraldObjectPool.Despawn(gameObject);
        }

        void OnDisable ()
        {
            StopAllCoroutines();
        }
    }
}