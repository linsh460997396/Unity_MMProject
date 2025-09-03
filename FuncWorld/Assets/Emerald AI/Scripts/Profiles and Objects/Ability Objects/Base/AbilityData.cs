using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [System.Serializable]
    public class AbilityData
    {
        [System.Serializable]
        public class CreateSettingsData
        {
            [Tooltip("Controls whether or not this module is enabled.")]
            [HideInInspector] public bool Enabled;
            [Tooltip("Controls the effect that happens when the ability is created.")]
            public GameObject CreateEffect;
            [Range(0.5f, 8f)]
            [Tooltip("Controls the how long the Create Effect will last before being despanwed.")]
            public float CreateEffectTimeout = 2;
            [Tooltip("Controls the sound that's played when the Create Effect is spawned.")]
            public List<AudioClip> CreateSoundsList = new List<AudioClip>();

            /// <summary>
            /// Spawns a Create Effect at the specified location.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Create Effect.</param>
            public void SpawnCreateEffect(GameObject Owner, Transform SpawnPosition)
            {
                if (Enabled)
                {
                    if (CreateEffect != null)
                    {
                        GameObject SpawnedCreateEffect = EmeraldObjectPool.SpawnEffect(CreateEffect, SpawnPosition.position, Owner.transform.rotation, CreateEffectTimeout);
                        SpawnedCreateEffect.name = CreateEffect.name;
                        SpawnedCreateEffect.transform.localScale = CreateEffect.transform.localScale;
                    }

                    if (CreateSoundsList.Count > 0)
                    {
                        AudioClip Clip = CreateSoundsList[Random.Range(0, CreateSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition.position, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }

        [System.Serializable]
        public class ChargeSettingsData
        {
            [Tooltip("Controls whether or not this module is enabled.")]
            [HideInInspector] public bool Enabled;
            [Tooltip("Controls the effect that happens when the ability is charging or being casted.\n\nNote: This must be a single object with a Particle System on it.")]
            public GameObject ChargeEffect;
            [Range(0.5f, 8f)]
            [Tooltip("Controls the how long the Charge Effect will last before being faded out. This is done by setting the Duration of the Charge Effect equal to that of the Charge Length. The Charge Effect's Particle System should have Loop set to false.")]
            public float ChargeLength = 2;
            [Tooltip("Controls the sound that's played when the Charge Effect is spawned.")]
            public List<AudioClip> ChargeSoundsList = new List<AudioClip>();

            /// <summary>
            /// Spawns a Charge Effect at the specified location.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Charge Effect.</param>
            public void SpawnChargeEffect(GameObject Owner, Transform SpawnPosition)
            {
                if (Enabled)
                {
                    if (ChargeEffect != null)
                    {
                        GameObject SpawnedChargeEffect = EmeraldObjectPool.SpawnEffect(ChargeEffect, SpawnPosition.position, Owner.transform.rotation, ChargeLength+1f);
                        SpawnedChargeEffect.name = ChargeEffect.name;
                        SpawnedChargeEffect.transform.localScale = ChargeEffect.transform.localScale;
                        SpawnedChargeEffect.transform.SetParent(SpawnPosition);
                        ParticleSystem ParticleSystemRef = SpawnedChargeEffect.GetComponent<ParticleSystem>();
                        if (ParticleSystemRef)
                        {
                            ParticleSystemRef.Stop();
                            var main = ParticleSystemRef.main;
                            main.duration = ChargeLength;
                            ParticleSystemRef.Play();
                        }
                        else
                        {
                            Debug.LogError("The " + ChargeEffect.name + " does not have a Particle System on it so it will not be used. A Particle System is required to be used as an ability's Charge Effect.");
                        }
                    }

                    if (ChargeSoundsList.Count > 0)
                    {
                        AudioClip Clip = ChargeSoundsList[Random.Range(0, ChargeSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition.position, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }

        [System.Serializable]
        public class MeleeData
        {
            [HideInInspector] public bool Enabled;
            [Tooltip("Controls the effect that happens when the ability hits a target.")]
            public GameObject ImpactEffect;
            [Range(0.5f, 10)]
            [Tooltip("Controls the time (in seconds) before the Impact Effect is disabled after it has been spawned.")]
            public float ImpactEffectTimeoutSeconds = 2;
            [Tooltip("The list of possible sounds that will play when the ability hits a target.")]
            public List<AudioClip> ImpactSoundsList = new List<AudioClip>();

            [Space(15)]
            [Range(1, 30)]
            [Tooltip("Controls the max distance allowed to deal damage with this ability.\n\nNote: If your melee attack animation is using Weapon Collision Events, " +
                "this setting will be ignored and this ability will rely on a successful collision from the weapon instead.")]
            public float MaxDamageDistance = 4;
            [Range(1, 20)]
            [Tooltip("Controls the max allowed height the AI can attack with this ability.\n\nNote: If this height is exceeded, the AI will wait to attack the current target until they are within range.")]
            public float MaxAttackHeight = 5;
            [Range(5, 360)]
            [Tooltip("Controls the max angle allowed to deal damage with this ability.\n\nNote: If your melee attack animation is using Weapon Collision Events, " +
                "this setting will be ignored and this ability will rely on a successful collision from the weapon instead.")]
            public float MaxDamageAngle = 90;
        }

        [System.Serializable]
        public class ProjectileData
        {
            [HideInInspector] public bool Enabled = true;
            public GameObject SpawnProjectileEffect;
            [Range(1, 15)]
            public float SpawnProjectileTimeoutSeconds = 2;
            public List<AudioClip> SpawnProjectileSoundsList = new List<AudioClip>();

            [Space(15)]
            [Tooltip("(Required) The effect that will be used for the projectile object.")]
            public GameObject ProjectileEffect;
            [Range(1, 15)]
            [Tooltip("Controls the time (in seconds) the Projectile Effect will time out. When this happens, the projectile will play its Impact Effect, given it isn'transform empty.")]
            public float ProjectileTimeoutSeconds = 6;
            [Tooltip("The sound that will play (and loop) while the projectile is moving. This will stop once the projectile has collided with something or ended.")]
            public AudioClip TravelSound;
            [Tooltip("A list of particle effect names that will be disabled on impact (after the Effects Disable Time has passed). This gives effects like trails time to finish playing, but still allows the main part of the projectile to be disabled on impact." +
                "\n\nNote: This is based on the Projectile Effect object.")]
            public List<string> EffectsToDisable = new List<string>();

            [Space(15)]
            [Tooltip("Controls the effect that happens when the projectile starts moving towards its target.\n\nNote: This setting is more useful when there the Launch Projectile Delay is used.")]
            public GameObject LaunchProjectileEffect;
            [Range(1, 15)]
            [Tooltip("Controls the time (in seconds) before the Launch Projectile Effect is disabled after it has been spawned.")]
            public float LaunchProjectileTimeoutSeconds = 2;

            [Range(0, 4)]
            [Tooltip("Controls the time (in seconds) the projectile's launch will be delayed, after the Projectile Effect has been spawnwed.")]
            public float LaunchProjectileDelay = 0f;

            public List<AudioClip> LaunchProjectileSoundsList = new List<AudioClip>();

            [Space(15)]
            [Tooltip("Controls the effect that happens after the projectile has collided with a target or surface.")]
            public GameObject ImpactEffect;
            [Range(1, 15)]
            [Tooltip("Controls the time (in seconds) before the Impact Projectile Effect is disabled after it has been spawned.")]
            public float ImpactTimeoutSeconds = 6;
            public List<AudioClip> ImpactSoundsList = new List<AudioClip>();

            /// <summary>
            /// Spawns a Launch Effect at the specified location.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Launch Effect.</param>
            public void SpawnLaunchProjectileEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (LaunchProjectileEffect != null)
                    {
                        GameObject SpawnedLaunchEffect = EmeraldObjectPool.SpawnEffect(LaunchProjectileEffect, SpawnPosition, LaunchProjectileEffect.transform.rotation, LaunchProjectileTimeoutSeconds);
                        SpawnedLaunchEffect.name = LaunchProjectileEffect.name;
                        SpawnedLaunchEffect.transform.localScale = LaunchProjectileEffect.transform.localScale;
                    }

                    if (LaunchProjectileSoundsList.Count > 0)
                    {
                        AudioClip Clip = LaunchProjectileSoundsList[Random.Range(0, LaunchProjectileSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }

            /// <summary>
            /// Spawns a Spawn Effect at the specified location when the projectile is being created.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Spawn Effect.</param>
            public void SpawnEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (SpawnProjectileEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(SpawnProjectileEffect, SpawnPosition, SpawnProjectileEffect.transform.rotation, SpawnProjectileTimeoutSeconds);
                        SpawnedEffect.name = SpawnProjectileEffect.name;
                        SpawnedEffect.transform.localScale = SpawnProjectileEffect.transform.localScale;
                    }

                    if (SpawnProjectileSoundsList.Count > 0)
                    {
                        AudioClip Clip = SpawnProjectileSoundsList[Random.Range(0, SpawnProjectileSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }

            /// <summary>
            /// Spawns a Spawn Effect at the specified location when the projectile is being created.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Spawn Effect.</param>
            public void SpawnImpactEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (ImpactEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(ImpactEffect, SpawnPosition, ImpactEffect.transform.rotation, ImpactTimeoutSeconds);
                        SpawnedEffect.name = ImpactEffect.name;
                        SpawnedEffect.transform.localScale = ImpactEffect.transform.localScale;
                    }

                    if (ImpactSoundsList.Count > 0)
                    {
                        AudioClip Clip = ImpactSoundsList[Random.Range(0, ImpactSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }

        [System.Serializable]
        public class GeneralProjectileData
        {
            [HideInInspector] public bool Enabled = true;
            [Range(1, 100)]
            [Tooltip("Controls the speed of the projectile.")]
            public int ProjectileSpeed = 30;
            [Range(10, 360)]
            [Tooltip("Controls the launching cap for the projectile.")]
            public int ProjectileMaxLaunchAngle = 140;
            [Range(1, 40)]
            [Tooltip("Controls the number of projectiles that will be created when the ability is created.")]
            public int TotalProjectiles = 2;
            [Range(0, 1)]
            [Tooltip("Controls the time (in seconds) between each created projectile, given there's more than 1.")]
            public float TimeBetweenProjectiles = 0.2f;
            [Tooltip("Controls whether or not this projectile will stick into what it hits.\n\nNote: The length in which a projectile stays stuck is based on this ability's " +
                "Collision Timeout value (from the Collider Module). For best results, use with AI targets who have a Location Based Damage component.")]
            public bool AttachToTarget = false;
        }

        [System.Serializable]
        public class GrenadeData
        {
            [HideInInspector] public bool Enabled = true;
            [Tooltip("Controls object used as the grenade.")]
            public GameObject GrenadeObject;
            [Tooltip("Controls the layer that this grenade will be assigned.\n\nNote: In order for AI to dodge this grenade, ensure that this layer is included on their Projectile Layers. By default this is Ignore Layermask.")]
            [Layer] public int GrenadeLayer = 2;
            [Range(1, 8)]
            [Tooltip("Controls the throwing height of the grenade.")]
            public float ThrowHeight = 5f;
            [Tooltip("Controls the sound used when throwing the grenade.")]
            public AudioClip ThrowSound;
            [Tooltip("Controls whether or not the grenade will dynamically rotate in the air after being thrown.\n\nNote: This process stops as soon as the grenade collides with something.")]
            public bool RotateOnThrow;
            [Tooltip("Controls the sounds that play when the grenade collides with surfaces.")]
            public List<AudioClip> ImpactSoundsList = new List<AudioClip>();

            [Space(15)]
            [Tooltip("Controls the layers that the grenade can affect when exploding. If a rigidbody component is detected, force will be added to the rigidbody.\n\nNote: Damage is automatically applied to any AI with an Enemy relation from the thrower (unless Can Damage Friendlies is enabled). The layer for this is based off of the thrower's Detection Layers within its Detection Component.")]
            public LayerMask RigidbodyLayers = 0;
            [Range(1, 15)]
            [Tooltip("Controls the radius of the explosion. Anything within this radius will be affected by the grenade.")]
            public int ExplosionRadius = 4;
            [Range(0, 10)]
            [Tooltip("Controls the time (in seconds) it takes for the grenade to explode.")]
            public float ExplosionTime = 2.5f;
            public GameObject ExplosionEffect;
            [Range(1, 15)]
            [Tooltip("Controls the time (in seconds) the Explosion Effect will time out.")]
            public float ExplosionTimeoutSeconds = 3;
            [Tooltip("Controls whether or not this grenade can damage friendly targets as well as the thrower.")]
            public bool CanDamageFriendlies = false;
            public List<AudioClip> ExplosionSoundsList = new List<AudioClip>();

            /// <summary>
            /// Spawns an explosion effect and sound at the specified location.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the explosion effect and sound.</param>
            public void ExplodeGrenade(GameObject Owner, Vector3 SpawnPosition)
            {
                if (Enabled)
                {
                    if (ExplosionEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(ExplosionEffect, SpawnPosition, ExplosionEffect.transform.rotation, ExplosionTimeoutSeconds);
                        SpawnedEffect.name = ExplosionEffect.name;
                        SpawnedEffect.transform.localScale = ExplosionEffect.transform.localScale;
                    }

                    if (ExplosionSoundsList.Count > 0)
                    {
                        AudioClip Clip = ExplosionSoundsList[Random.Range(0, ExplosionSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }

        [System.Serializable]
        public class BulletProjectileData
        {
            [HideInInspector] public bool Enabled = true;
            [Tooltip("The layers that bullets will ignore. This is useful for bypassing collisions with certain object or colliders that you don'transform want interfering with the bullets.")]
            public LayerMask IgnoreLayers;
            [Tooltip("The game object used for the bullet.\n\nNote: This object should have no colliders attached as collision detection is handled internally. Users can have a Trail Renderer attached to act as a bullet trail effect.")]
            public GameObject BulletObject;
            [Range(1, 10)]
            [Tooltip("Controls the length (in seconds) for the Bullet Object to despawn, given that it never collided with anything.")]
            public int BulletObjectTimeout = 4;
            [Range(0.05f, 0.75f)]
            [Tooltip("Controls the length (in seconds) for the Bullet Object to despawn after successfully colliding with something. This can give additional time for bullet trails to finish playing, if needed.")]
            public float BulletCollisionTimeout = 0.15f;
            [Range(20, 75)]
            [Tooltip("Controls the speed of the bullets.")]
            public int BulletSpeed = 50;
            [Range(0, 0.006f)]
            [Tooltip("Controls the speed that checks are made for a collision. Lower values are more accurate." +
                "\n\nNote: The higher the Bullet Speed, the more checks need to be made to ensure collisions are not missed. If you find bullets not colliding, slowly decrease this setting until you find a value that works for your bullets.")]
            public float CollisionCheckSpeed = 0.003f;
            [Range(0, 100)]
            [Tooltip("Controls the spread of the bullets on the X axis. The higher the vlaue, the more inaccurate an AI will be.")]
            public int BulletSpreadX = 5;
            [Range(0, 100)]
            [Tooltip("Controls the spread of the bullets on the Y axis. The higher the vlaue, the more inaccurate an AI will be.")]
            public int BulletSpreadY = 5;
            [Range(1, 40)]
            [Tooltip("Controls the number of bullets that will be created when the ability is created. This can be used for auto or semi-auto weapons.")]
            public int TotalBullets = 1;
            [Range(0, 1)]
            [Tooltip("Controls the time (in seconds) between each created bullet, given there's more than 1.")]
            public float TimeBetweenBullets = 0.2f;

            [Space(15)]
            [Tooltip("Controls the effect that happens when the bullet is fired.\n\nNote: The Muzzle Flash position will be taken from the AI's current Attack Transfrom.")]
            public GameObject MuzzleFlashEffect;
            [Range(1, 10)]
            [Tooltip("Controls the time (in seconds) before the Muzzle Flash Effect is disabled after it has been spawned.")]
            public float MuzzleFlashEffectTimeoutSeconds = 2;
            [Range(0, 1)]
            [Tooltip("Controls the volume of the Fire Sounds.")]
            public float FireSoundsVolume = 1;
            [Tooltip("The sounds that play each time a projectile is fired.")]
            public List<AudioClip> FireSoundsList = new List<AudioClip>();

            [Space(15)]
            [Tooltip("The default impact effect that will happen if no impact data is used or found.")]
            public GameObject DefaultImpactEffect;
            [Range(1, 20)]
            [Tooltip("Controls the time (in seconds) before the Default Impact Effect is disabled after it has been spawned.")]
            public float DefaultImpactEffectTimeoutSeconds = 8;
            [Range(0, 1)]
            [Tooltip("Controls the volume of the Default Impact Sounds.")]
            public float DefaultImpactSoundsVolume = 1;
            [Tooltip("The default impact sounds that will happen if no impact data is used or found.")]
            public List<AudioClip> DefaultImpactSounds = new List<AudioClip>();

            [System.Serializable]
            public class BulletImpactClass
            {
                [Tooltip("The tag for identifying this impact.")]
                [Tag] public string SurfaceTag = "Untagged";
                [Tooltip("The impact effect that will happen if impacted with the above tag.")]
                public GameObject ImpactEffect;
                [Range(1, 20)]
                [Tooltip("Controls the time (in seconds) before the Impact Effect is disabled after it has been spawned.")]
                public float ImpactEffectTimeoutSeconds = 8;
                [Range(0, 1)]
                [Tooltip("Controls the volume of the Impact Sounds.")]
                public float ImpactSoundsVolume = 1;
                [Tooltip("The impact sounds that will happen if impacted with the above tag.")]
                public List<AudioClip> ImpactSounds = new List<AudioClip>();
                //public List<GameObject> ImpactDecals = new List<GameObject>();
            }
            [Space(15)]
            [SerializeField]
            [Tooltip("Bullet Impact Data allows users to have different impact effects and sounds to play depending on the tag the bullet collides with.\n\nNote: If no impact data is used or if no impact data was found, the Default Impact Effect and Default Impact Sounds will be used instead.")]
            public List<BulletImpactClass> BulletImpactData = new List<BulletImpactClass>();

            /// <summary>
            /// Spawns a Muzzle Flash Effect and bullet sound at the specified location.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Muzzle Effect and bullet sound.</param>
            public void SpawnBulletEffect(GameObject Owner, Vector3 SpawnPosition, Transform AttackTransform)
            {
                if (Enabled)
                {
                    if (MuzzleFlashEffect != null)
                    {
                        GameObject SpawnedLaunchEffect = EmeraldObjectPool.SpawnEffect(MuzzleFlashEffect, AttackTransform.position, AttackTransform.rotation, MuzzleFlashEffectTimeoutSeconds);
                        SpawnedLaunchEffect.name = MuzzleFlashEffect.name;
                        SpawnedLaunchEffect.transform.localScale = MuzzleFlashEffect.transform.localScale;
                    }

                    if (FireSoundsList.Count > 0)
                    {
                        AudioClip Clip = FireSoundsList[Random.Range(0, FireSoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f) * FireSoundsVolume;
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }

            /// <summary>
            /// Spawns an impact effect and sound at the specified location.
            /// </summary>
            /// <param name="Target">The target hit by the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Muzzle Effect and bullet sound.</param>
            public void SpawnBulletImpact(GameObject Target, BulletImpactClass ImpactData, Vector3 SpawnPosition, Vector3 HitNormal)
            {
                if (Enabled)
                {
                    if (ImpactData.ImpactEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(ImpactData.ImpactEffect, SpawnPosition, ImpactData.ImpactEffect.transform.rotation, ImpactData.ImpactEffectTimeoutSeconds);
                        SpawnedEffect.name = ImpactData.ImpactEffect.name;
                        SpawnedEffect.transform.localScale = ImpactData.ImpactEffect.transform.localScale;
                        SpawnedEffect.transform.rotation = Quaternion.FromToRotation(SpawnedEffect.transform.forward, HitNormal) * SpawnedEffect.transform.rotation;
                        SpawnedEffect.transform.SetParent(Target.transform);
                    }

                    if (ImpactData.ImpactSounds.Count > 0)
                    {
                        AudioClip Clip = ImpactData.ImpactSounds[Random.Range(0, ImpactData.ImpactSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f) * ImpactData.ImpactSoundsVolume;
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }

                    /*
                    if (ImpactData.ImpactDecals.Count > 0)
                    {
                        GameObject RandomDecal = ImpactData.ImpactDecals[Random.Range(0, ImpactData.ImpactDecals.Count)];
                        if (RandomDecal)
                        {
                            GameObject SpawnedDecal = EmeraldObjectPool.SpawnEffect(RandomDecal, SpawnPosition, Quaternion.identity, 20);
                            SpawnedDecal.transform.position = SpawnPosition;
                            SpawnedDecal.transform.rotation = Quaternion.FromToRotation(SpawnedDecal.transform.up, HitNormal) * SpawnedDecal.transform.rotation;
                        }
                    }
                    */
                }
            }

            /// <summary>
            /// Spawns an impact effect and sound at the specified location.
            /// </summary>
            /// <param name="Target">The target hit by the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the Muzzle Effect and bullet sound.</param>
            public void SpawnDefaultBulletImpact(GameObject Target, Vector3 SpawnPosition, Vector3 HitNormal)
            {
                if (Enabled)
                {
                    if (DefaultImpactEffect != null)
                    {
                        GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(DefaultImpactEffect, SpawnPosition, DefaultImpactEffect.transform.rotation, DefaultImpactEffectTimeoutSeconds);
                        SpawnedEffect.name = DefaultImpactEffect.name;
                        SpawnedEffect.transform.localScale = DefaultImpactEffect.transform.localScale;
                        SpawnedEffect.transform.rotation = Quaternion.FromToRotation(SpawnedEffect.transform.forward, HitNormal) * SpawnedEffect.transform.rotation;
                        SpawnedEffect.transform.SetParent(Target.transform);
                    }

                    if (DefaultImpactSounds.Count > 0)
                    {
                        AudioClip Clip = DefaultImpactSounds[Random.Range(0, DefaultImpactSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f) * DefaultImpactSoundsVolume;
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }
            }
        }



        [System.Serializable]
        public class ArrowProjectileData
        {
            [HideInInspector] public bool Enabled = true;
            [Range(1, 100)]
            [Tooltip("Controls the speed of the projectile.")]
            public int ProjectileSpeed = 30;
            [Tooltip("Controls whether or not this projectile will stick into what it hits.\n\nNote: The length in which a projectile stays stuck is based on this ability's " +
            "Collision Timeout value (from the Collider Module). For best results, use with AI targets who have a Location Based Damage component.")]
            public bool AttachToTarget = true;
        }

        [System.Serializable]
        public class AerialProjectileData
        {
            [HideInInspector] public bool Enabled = true;
            [Tooltip("Controls the effect that will happen at the Spawn Source position.")]
            public GameObject AerialEffect;
            [Range(0.5f, 15)]
            public float AerialEffectTimeoutSeconds = 2;
            public enum SpawnSources { AboveSelf, AboveTarget }
            [Tooltip("Controls the source in which the aerial ability will spawn from.")]
            public SpawnSources SpawnSource = SpawnSources.AboveSelf;
            public enum AimSources { TargetPosition, GroundPosition }
            [Tooltip("Controls the projectile's aim source.\n\nNote: If Homing Seconds is set to a value higher than 0 (and is enabled), the Ground Position option be ignored.")]
            public AimSources AimSource = AimSources.TargetPosition;

            [Space(15)]
            [Range(0f, 30f)]
            [Tooltip("Controls the extra height in which the aerial ability will spawn from its Spawn Source.")]
            public float HeightOffset = 12;
            [Range(0f, 45f)]
            [Tooltip("Controls the max randomized angle in which the projectile will rotate towards its Aim Source.")]
            public float LaunchAngle = 0;
            [Range(0f, 30f)]
            [Tooltip("Controls the maximum radius an aerial projectile can spawned from its Spawn Source.")]
            public float Radius = 4;

            [Space(15)]
            [Range(1, 100)]
            [Tooltip("Controls the speed of the projectile.")]
            public int ProjectileSpeed = 30;
            [Range(1, 40)]
            [Tooltip("Controls the number of projectiles that will be created when the ability is created.")]
            public int TotalProjectiles = 2;
            [Range(0, 1)]
            [Tooltip("Controls the time (in seconds) between each created projectile, given there's more than 1.")]
            public float TimeBetweenProjectiles = 0.2f;
            public bool AttachToTarget = false;

            /// <summary>
            /// Spawns an Aerial Effect at the specified location when the aerial projectile is being created.
            /// </summary>
            public void SpawnAerialEffect(GameObject Owner, Transform Target)
            {
                if (Enabled && AerialEffect != null)
                {
                    if (SpawnSource == SpawnSources.AboveTarget)
                    {
                        EmeraldObjectPool.SpawnEffect(AerialEffect, new Vector3(Target.position.x, Target.position.y + HeightOffset, Target.position.z), AerialEffect.transform.rotation, AerialEffectTimeoutSeconds);
                    }
                    else if (SpawnSource == SpawnSources.AboveSelf)
                    {
                        EmeraldObjectPool.SpawnEffect(AerialEffect, new Vector3(Owner.transform.position.x, Owner.transform.position.y + HeightOffset, Owner.transform.position.z), AerialEffect.transform.rotation, AerialEffectTimeoutSeconds);
                    }
                }
            }
        }

        [System.Serializable]
        public class BarrageProjectileData
        {
            [HideInInspector] public bool Enabled = true;
            [Range(1, 100)]
            [Tooltip("Controls the speed of the projectile.")]
            public int ProjectileSpeed = 30;
            [Range(1, 40)]
            [Tooltip("Controls the number of projectiles that will be created when the ability is created.")]
            public int TotalProjectiles = 2;
            [Range(0, 1)]
            [Tooltip("Controls the time (in seconds) between each created projectile, given there's more than 1.")]
            public float TimeBetweenProjectiles = 0.2f;
        }


        [System.Serializable]
        public class GroundProjectileData
        {
            [HideInInspector] public bool Enabled = true;
            [Tooltip("The layers the ability will use to align itself.\n\nNote: This should only be surface layers like the ground and environment. This should exclude target layers like players and AI.")]
            public LayerMask AllignmentLayers;
            [Range(1, 50)]
            [Tooltip("The speed the ability will use to align itself while moving.")]
            public float AlignmentSpeed = 10;
            [Range(1, 90)]
            [Tooltip("The max angle the ability will align to while moving.")]
            public float MaxAlignmentAngle = 45;
            [Range(5, 90)]
            [Tooltip("The angle (that if met while traveling) will stop the ability.")]
            public float KillAngle = 90;
            [Range(-5, 5)]
            [Tooltip("The height offset the ability will use while moving. This can help better position an effect if it is too high or too low to the ground.")]
            public float HeightOffset = 0f;

            [Space(15)]
            [Range(1, 50)]
            [Tooltip("Controls the max distance the Ground Effect will travel before stopping.")]
            public float MaxTravelDistance = 10;
            [Range(0, 360)]
            [Tooltip("Controls the angle the projectiles will be eveny spread across, given there's more than 1 Total Projectiles.")]
            public float AngleSpread = 45;

            [Space(15)]
            [Range(1, 100)]
            [Tooltip("Controls the speed of the projectile.")]
            public int ProjectileSpeed = 30;
            [Range(1, 40)]
            [Tooltip("Controls the number of projectiles that will be created when the ability is created.")]
            public int TotalProjectiles = 2;
            [Range(0, 1)]
            [Tooltip("Controls the time (in seconds) between each created projectile, given there's more than 1.")]
            public float TimeBetweenProjectiles = 0.2f;
        }

        [System.Serializable]
        public class HomingData
        {
            [HideInInspector] public bool Enabled;
            [Range(0, 15)]
            [Tooltip("Controls the length (in seconds) a projectile will follow its target.")]
            public float HomingSeconds = 0;
            [Range(0, 10)]
            [Tooltip("Controls the minimum distance a projectile will follow its target.\n\nNote:A value of 0 can be used to disable this setting.")]
            public float MinimumHomingDistance = 0;
            [Range(1, 10)]
            [Tooltip("Controls the speed in which a projectiles will rotate towards its target.")]
            public float HomingSpeed = 4;
        }

        [System.Serializable]
        public class AreaOfEffectData
        {
            [HideInInspector] public bool Enabled = true;
            //Coming with update
            //public enum LocationTypes { Self, RandomWithinRadiusOfSelf }
            //public LocationTypes LocationType = LocationTypes.Self;
            [Tooltip("Controls the visual effect used for the Area of Effect ability.")]
            public GameObject VisualEffect;
            [Range(0.5f, 15)]
            [Tooltip("Controls the time (in seconds) the Visual Effect will be disabled after it has been spawned.")]
            public float VisualEffectTimeoutSeconds = 6;
            [Range(-5, 5)]
            [Tooltip("Controls the height offset the Visual Effect will be spawned.")]
            public float HeightOffset = 0;
            [Range(1, 20)]
            [Tooltip("Controls the damage radius of the Area of Effect ability.")]
            public float Radius = 3;
            [Range(0f, 5f)]
            [Tooltip("Controls the delay it takes for the AOE target detection to trigger.")]
            public float Delay = 0;
            [Tooltip("The effect that will happen when a valid target is hit with this ability.")]
            public GameObject HitTargetEffect;
            [Range(0.5f, 15)]
            public float HitTargetEffectTimeoutSeconds = 2;
            [Tooltip("The list of possible sounds that will play when the Visual Effect is spawned.")]
            public List<AudioClip> AOESoundsList = new List<AudioClip>();

            /// <summary>
            /// Spawns a AOE Effect at the specified location.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the AOE Effect.</param>
            public GameObject SpawnAOEEffect(GameObject Owner, Vector3 SpawnPosition)
            {
                GameObject SpawnedVisualEffect = null;

                if (Enabled)
                {
                    if (VisualEffect != null)
                    {
                        SpawnedVisualEffect = EmeraldObjectPool.SpawnEffect(VisualEffect, SpawnPosition, VisualEffect.transform.rotation, VisualEffectTimeoutSeconds);
                        SpawnedVisualEffect.name = VisualEffect.name;
                        SpawnedVisualEffect.transform.localScale = VisualEffect.transform.localScale;
                    }

                    if (AOESoundsList.Count > 0)
                    {
                        AudioClip Clip = AOESoundsList[Random.Range(0, AOESoundsList.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }

                return SpawnedVisualEffect;
            }
        }

        [System.Serializable]
        public class SummonData
        {
            [HideInInspector] public bool Foldout = true;
            [HideInInspector] public bool Enabled = true;

            [Tooltip("Controls the visual effect used when casting the ability.")]
            public GameObject CastEffect;
            [Range(0.5f, 10)]
            [Tooltip("Controls the time (in seconds) the Summon Effect will be disabled after it has been spawned.")]
            public float CastEffectTimeoutSeconds = 4;
            [Tooltip("Controls the sound that will play when the abiliy is casted.")]
            public List<AudioClip> CastSounds = new List<AudioClip>();

            [Tooltip("Controls the visual effect used when the summong ability is first created.")]
            public GameObject SummonEffect;
            [Range(0.5f, 10)]
            [Tooltip("Controls the time (in seconds) the Summon Effect will be disabled after it has been spawned.")]
            public float SummonEffectTimeoutSeconds = 4;
            [Range(-3f, 3f)]
            [Tooltip("Controls the Y position offset for the SummonEffect.")]
            public float SummonEffectHeightOffset = 0f;
            [Tooltip("Controls the sound that will play when an AI is summoned.")]
            public List<AudioClip> SummonSounds = new List<AudioClip>();

            [Tooltip("Controls how many AI will be summoned when using this ability.")]
            [Range(1, 6)]
            public int SummonAmount = 1;
            public enum SummonPositions { Self, Target };
            [Tooltip("Controls the position in which the AI will be summoned to." +
                "\n\nSelf - Summons AI around the caster." +
                "\n\nTarget - Summons AI around the caster's current target.")]
            public SummonPositions SummonPosition = SummonPositions.Self;
            [Tooltip("Controls the radius in which summoned AI will spawn around the Summon Position.")]
            [Range(2f, 12f)]
            public float SummonRadius = 4f;
            [Range(0f, 1f)]
            [Tooltip("Controls the delay length (in seconds) of when the AI will be summoned after the ability has been casted.")]
            public float SummonDelay = 0;
            [Tooltip("A list of possible AI prefabs that will be picked when the caster is using this ability. The total amount of AI spawned is determined by the Summon Amount.")]
            public List<GameObject> AIPrefabs = new List<GameObject>();

            [Tooltip("Controls whether or not this AI will be automatically killed after a certain amount of time.")]
            public bool IsTimedSummon = false;
            [Range(10, 180)]
            [Tooltip("Controls the length (in seconds) until the AI is killed.")]
            public int SummonLength = 20;

            [Tooltip("Controls whether or not this AI's body (after it has died) will be despawned (through Object Pooling) so it can be reused. The AI object will automatically be reset to its starting state during this process.")]
            public bool DespawnAfterKilled = false;
            [Range(5, 120)]
            [Tooltip("Controls the length (in seconds) until the AI is despawned back to the Object Pool after it has been killed.")]
            public int DespawnLength = 20;

            /// <summary>
            /// A universal function for spawning various sounds and effects for this ability.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position for the effect.</param>
            public GameObject SpawnEffect(GameObject Owner, Vector3 SpawnPosition, GameObject VisualEffect, float TimeoutSeconds, List<AudioClip> EffectSounds, bool SkipSound)
            {
                GameObject SpawnedVisualEffect = null;

                if (Enabled)
                {
                    if (VisualEffect != null)
                    {
                        SpawnedVisualEffect = EmeraldObjectPool.SpawnEffect(VisualEffect, SpawnPosition, VisualEffect.transform.rotation, TimeoutSeconds);
                        SpawnedVisualEffect.name = VisualEffect.name;
                        SpawnedVisualEffect.transform.localScale = VisualEffect.transform.localScale;
                    }

                    if (EffectSounds.Count > 0 && !SkipSound)
                    {
                        AudioClip Clip = EffectSounds[Random.Range(0, EffectSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }

                return SpawnedVisualEffect;
            }
        }

        [System.Serializable]
        public class HealingData
        {
            [HideInInspector] public bool Foldout = true;
            [HideInInspector] public bool Enabled = true;

            [Tooltip("Controls the visual effect used when the healing ability is first created.")]
            public GameObject HealingEffect;
            [Range(0.5f, 15)]
            [Tooltip("Controls the time (in seconds) the Healing Effect will be disabled after it has been spawned.")]
            public float HealingEffectTimeoutSeconds = 6;

            public enum TargetTypes { Self, Target, Area };
            [Tooltip("Controls the type of target this ability will affect." +
                "\n\nSelf - The healing will only affect the caster." +
                "\n\nTarget - The healing will affect a single friendly target with the lowest amount of health within radius of the caster." +
                "\n\nArea - The healing will affect any friendly targets within radius of the caster. The layer for this is based off of the caster's Detection Layers from its Detection Component.")]
            public TargetTypes TargetType = TargetTypes.Self;

            public enum HealingTypes { Instant, OverTime};
            [Tooltip("Controls the type of healing this ability will use." +
                "\n\nInstant - Instantly heals the target according to the Base Heal Amount." +
                "\n\nOver Time - Instantly heals the target according to the Base Heal Amount AND over time based on the amount and rate. " +
                "Base Heal Amount can be set to 0 if no initial heal is desired.")]
            public HealingTypes HealingType = HealingTypes.Instant;

            //Base Heals
            [Tooltip("Controls the base healing or initial healing amount for this ability.")]
            public int BaseHealAmount = 20;
            //Base Heals

            //Heals Over Time
            [Tooltip("Controls the length (in seconds) how often Heal Over Time is applied.")]
            [Range(0.1f, 10f)]
            public float TickRate = 1;
            [Tooltip("Controls the amount of heals that happens per Tick Rate.")]
            public int HealsPerTick = 5;
            [Range(0f, 10f)]
            [Tooltip("Controls the length (in seconds) the Heal Over Time effect will last.")]
            public float HealOverTimeLength = 3;
            [Tooltip("Controls the sound that will play each heal tick.")]
            public List<AudioClip> HealTickSounds = new List<AudioClip>();
            //Heals Over Time

            [Range(-5, 5)]
            [Tooltip("Controls the height offset the Healing Effect will be spawned.")]
            public float EffectHeightOffset = 0;
            [Range(1, 20)]
            [Tooltip("Controls the healing radius of the healing ability.")]
            public float Radius = 3;
            [Range(0f, 5f)]
            [Tooltip("Controls the delay it takes for the target detection to trigger for area healing.")]
            public float Delay = 0;
            [Tooltip("The visual effect that will happen when a valid target is healed with this ability.")]
            public GameObject HealTargetEffect;
            [Range(0.5f, 15)]
            public float HealTargetEffectTimeoutSeconds = 2;
            [Tooltip("The list of possible sounds that will play when the Visual Effect is spawned.")]
            public List<AudioClip> HealingSoundsList = new List<AudioClip>();

            /// <summary>
            /// A universal function for spawning various sounds and effects for healing abilities.
            /// </summary>
            /// <param name="Owner">The owner of the ability.</param>
            /// <param name="SpawnPosition">The spawn position of the AOE Effect.</param>
            public GameObject SpawnHealingEffect(GameObject Owner, Vector3 SpawnPosition, GameObject VisualEffect, float TimeoutSeconds, List<AudioClip> EffectSounds)
            {
                GameObject SpawnedVisualEffect = null;

                if (Enabled)
                {
                    if (VisualEffect != null)
                    {
                        SpawnedVisualEffect = EmeraldObjectPool.SpawnEffect(VisualEffect, SpawnPosition, VisualEffect.transform.rotation, TimeoutSeconds);
                        SpawnedVisualEffect.name = VisualEffect.name;
                        SpawnedVisualEffect.transform.localScale = VisualEffect.transform.localScale;
                    }

                    if (EffectSounds.Count > 0)
                    {
                        AudioClip Clip = EffectSounds[Random.Range(0, EffectSounds.Count)];
                        if (Clip)
                        {
                            AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                            TempSound.volume = Random.Range(0.75f, 1f);
                            TempSound.pitch = Random.Range(0.9f, 1.1f);
                            TempSound.PlayOneShot(Clip);
                        }
                    }
                }

                return SpawnedVisualEffect;
            }
        }

        [System.Serializable]
        public class ConditionData
        {
            [HideInInspector] public bool Foldout = true;
            [HideInInspector] public bool Enabled;
            [Tooltip("Controls the condition that is needed in order for this ability to trigger. Enable High Priority to allow this ability to be picked before other abilities." +
                "\n\nSelf Low Health - Triggers when an AI's own health reaches the Low Health Percentage threshold." +
                "\n\nTarget Low Health - Triggers when a nearby Friendly AI reaches the Low Health Percentage threshold." +
                "\n\nNo Current Summons - Triggers when an AI has 0 currently summoned AI.")] 
            public ConditionTypes ConditionType = ConditionTypes.SelfLowHealth;
            public enum ValueCompareTypes { GreaterThan, LessThan };
            [Tooltip("Controls the type of value comparison.")]
            public ValueCompareTypes ValueCompareType = ValueCompareTypes.LessThan;
            [Tooltip("Controls whether or not this condition is High Priority. A High Priority ability will take priority over other abilities and will ignore an AI's Pick Type, given the condition is met. This is especially helpful for support abiltiies such as healing." +
                "\n\nNote: If High Priority is disabled, this ability will follow an AI's Pick Type, but the condition will need to be met. If the condition is not met, the ability will be skipped.")]
            public bool HighPriority = false;
            [Range(1, 40)]
            [Tooltip("Controls the distance threshold needed to meet the Distance from Target condition.")]
            public float DistanceFromTarget = 3f;
            [Range(1, 99)]
            [Tooltip("Controls the low health amount (in percentage) needed to trigger this condition.")]
            public float LowHealthPercentage = 60f;
        }

        [System.Serializable]
        public class ColliderData
        {
            [HideInInspector] public bool Enabled = true;
            [Tooltip("Controls the layers that this projectile can collide with.\n\nNote: Ensure target layers (as well as Location Based Damage layers) are included or they will be ignored by the projectile.")]
            public LayerMask CollidableLayers = ~0;
            [Tooltip("Controls the layer that this projectile will be assigned.\n\nNote: It is recommended that projectiles ignore each other to avoid unintended collisions with other projectiles.")]
            [Layer] public int ProjectileLayer = 2;
            [Range(0f, 30f)]
            [Tooltip("Controls the time (in seconds) it will take to disable the projectile object after it has collided with a target or object.")]
            public float CollisionTimeout = 3;

            [Range(0, 1)]
            [Tooltip("Controls the radius of the projectile's Sphere Collider.")]
            [DrawIf("AutoCreateSphereCollider", true)]
            public float ColliderRadius = 0.05f;
            [Range(-2, 2)]
            [Tooltip("Controls the forward position offset of the projectile's Sphere Collider.")]
            public float ZOffet = 0;
            
        }

        [System.Serializable]
        public class SpreadData
        {
            [HideInInspector] public bool Enabled;
            [Tooltip("Controls how projectiles are spread.")]
            public SpreadTypes SpreadType = SpreadTypes.Random;
            [CompareEnumWithRange("SpreadType", 0f, 180f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MinSpreadX = 0;
            [CompareEnumWithRange("SpreadType", -180f, 0, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MaxSpreadX = 0;
            [CompareEnumWithRange("SpreadType", 0f, 180f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MinSpreadY = 0;
            [CompareEnumWithRange("SpreadType", -180f, 0, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.Random)]
            public float MaxSpreadY = 0;

            [Tooltip("Controls the horizontal angle for spreading projectiles. This will be evently distributed based on the total amount of projectiles and the given angle.")]
            [CompareEnumWithRange("SpreadType", 0f, 360f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.HorizontalRadius)]
            public float SpreadAngleX = 180;
            [Tooltip("Controls the vertical tilt angle for spreading projectiles evenly.")]
            [CompareEnumWithRange("SpreadType", 0f, 90f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.HorizontalRadius)]
            public float TiltAngleY = 0;
            [Tooltip("Controls the distance the projectiles will spawn away from the owner.")]
            [CompareEnumWithRange("SpreadType", 0f, 6f, CompareEnumWithRangeAttribute.StyleType.FloatSlider, SpreadTypes.HorizontalRadius)]
            public float SpawnDistance = 0.5f;
        }

        [System.Serializable]
        public class TargetTypeData
        {
            [HideInInspector] public bool Enabled = true;
            [Tooltip("Controls how this ability will pick its target.\n\nNote: The Multiple option will allow an ability to affect and target more than 1 target. If you are unsure, it's best to stick with the default option of Current Target.")]
            public TargetTypes TargetType = TargetTypes.CurrentTarget;
        }

        [System.Serializable]
        public class CooldownData
        {
            [HideInInspector] public bool Enabled;
            [Range(0, 60)]
            [Tooltip("Controls the length (in seconds) it will take before this ability can be used again. This is useful to help keep certain abilities from being used too often or in a row." +
                "\n\nNote: This setting is ignored if an ability is overriden through an Create Ability (Animation Event).")]
            public float CooldownLength = 1;
            
        }

        //TODO: Will add this feature with a future update
        /*
        [System.Serializable]
        public class BranchData
        {
            [Tooltip("Controls whether or not this module is enabled.")]
            [HideInInspector] public bool Enabled;
            [Tooltip("Controls the type of target this ability will affect.")]
            public TargetTypes TargetType = TargetTypes.CurrentTarget;
            [Range(0.25f, 10)]
            [Tooltip("Controls the radius used for detecting targets to branch to.")]
            public float BranchRadius = 1f;
            [Range(1, 100)]
            [Tooltip("Controls the odds for a successful branch.")]
            public int BranchOdds = 20;
            [Range(1, 25)]
            [Tooltip("Controls the cap for a successful branch for a sinlge projecile.")]
            public int BranchCap = 5;
        }
        */

        [System.Serializable]
        public class TeleportData
        {
            [HideInInspector] public bool Enabled = true;
            [Tooltip("Controls the effect that happens when the AI disappears when teleporting.")]
            public GameObject DisappearEffect;
            [Range(0.5f, 15)]
            public float DisappearEffectTimeoutSeconds = 2;
            public List<AudioClip> DisappearSoundsList = new List<AudioClip>();

            [Tooltip("Controls whether or not this ability will trigger an avoidable call when reappearing.\n\nNote: An avoidable call will allow nearby targets to attempt to dodge or block, given they have the needed combat actions.")]
            public bool ReappearTriggersAvoidable;
            [Tooltip("Controls the effect that happens when the AI is about to reappear.\n\nNote: This can be used to briefly show where an AI will reappear before it's visible.")]
            public GameObject ReappearIndicatorEffect;
            [Range(0.5f, 15)]
            public float ReappearIndicatorEffectTimeoutSeconds = 2;
            [Range(0f, 2.5f)]
            [Tooltip("The length (in seconds) the reappear functionality will be delayed.")]
            public float ReappearDelay = 0.15f;
            public List<AudioClip> ReappearIndicatorSoundsList = new List<AudioClip>();

            [Space(10)]
            [Tooltip("Controls the effect that happens when the AI reappears when teleporting.")]
            public GameObject ReappearEffect;
            [Range(0.5f, 15)]
            public float ReappearEffectTimeoutSeconds = 2;
            public List<AudioClip> ReappearSoundsList = new List<AudioClip>();

            [Space(10)]
            [Range(0f, 10)]
            [Tooltip("The length (in seconds) it takes for an AI to reappear after teleporting.")]
            public float TeleportTime = 1;

            [Range(0f, 10)]
            [Tooltip("Controls the radius used when generating a teleport destination. The destination is based on the Target Type.")]
            public float TeleportRadius = 3;
        }

        [System.Serializable]
        public class StunnedData
        {
            [HideInInspector] public bool Enabled = false;
            [Range(0, 100)]
            [Tooltip("Controls the odds for triggering a stun when this ability hits an enemy target.")]
            public float OddsToStun = 50;
            [Range(1, 15)]
            [Tooltip("Controls the length (in seconds) how long a target will be stunned.")]
            public float StunLength = 3;

            /// <summary>
            /// Rolls for a stun (using the odds through an ability) and returns true if successful.
            /// </summary>
            /// <returns></returns>
            public bool RollForStun ()
            {
                int Roll = Random.Range(1, 101);
                return (Roll <= OddsToStun);
            }
        }

        /// <summary>
        /// Spawns a Spawn Effect at the specified location when the projectile is being created.
        /// </summary>
        /// <param name="Owner">The owner of the ability.</param>
        /// <param name="SpawnPosition">The spawn position of the Spawn Effect.</param>
        public static void SpawnEffectAndSound(GameObject Owner, Vector3 SpawnPosition, GameObject Effect, float TimeoutSeconds, List<AudioClip> SoundsList)
        {
            if (Effect != null)
            {
                GameObject SpawnedEffect = EmeraldObjectPool.SpawnEffect(Effect, SpawnPosition, Effect.transform.rotation, TimeoutSeconds);
                SpawnedEffect.name = Effect.name;
                SpawnedEffect.transform.localScale = Effect.transform.localScale;
            }

            if (SoundsList.Count > 0)
            {
                AudioClip Clip = SoundsList[Random.Range(0, SoundsList.Count)];
                if (Clip)
                {
                    AudioSource TempSound = EmeraldObjectPool.SpawnEffect(Resources.Load("Emerald Sound") as GameObject, SpawnPosition, Quaternion.identity, Clip.length).GetComponent<AudioSource>();
                    TempSound.volume = Random.Range(0.75f, 1f);
                    TempSound.pitch = Random.Range(0.9f, 1.1f);
                    TempSound.PlayOneShot(Clip);
                }
            }
        }

        [System.Serializable]
        public class DamageData
        {
            [HideInInspector] public bool Foldout;
            [HideInInspector] public bool Enabled = true;

            [Space(10)]
            [Tooltip("Controls whether or not Critical Hits are used for this ability.")]
            public bool UseCriticalHits;
            [Space(10)]
            [Tooltip("Controls whether or not Damage Over Time is used for this ability.")]
            public bool UseDamageOverTime;

            public BaseDamageClass BaseDamageSettings;

            [System.Serializable]
            public class BaseDamageClass
            {
                [Tooltip("Controls whether or not Randomized Damage is used for this ability.")]
                public bool UseRandomAmounts;
                [DrawIf("UseRandomAmounts", false)]
                [Tooltip("Controls the base damage for this ability.")]
                public int BaseAmount = 5;
                [DrawIf("UseRandomAmounts", true)]
                [Tooltip("Controls the minimum damage that can be generated for this ability.")]
                public int MinAmount = 5;
                [DrawIf("UseRandomAmounts", true)]
                [Tooltip("Controls the maxmimum damage that can be generated for this ability.")]
                public int MaxAmount = 10;
                [Tooltip("Controls the force that will be applied to AI using ragdolls at the time of death.")]
                public int RagdollForce = 50;
            }

            public CriticalHitClass CriticalHitSettings;

            [System.Serializable]
            public class CriticalHitClass
            {
                [Tooltip("Controls the odds for a critical hit.")]
                [Range(0f, 100f)]
                public float CriticalHitOdds = 6.25f;
                [Tooltip("Controls the multiplier for a critical hit (GeneratedDamage + (GeneratedDamage * (%)CriticalHitMultiplier) = MultipliedDamage).")]
                public float CriticalHitMultiplier = 1.1f;
                [Tooltip("Controls the sound that will play when a critical hit is successful.")]
                public List<AudioClip> CriticalHitSounds = new List<AudioClip>();
            }

            public DamageOverTimeClass DamageOverTimeSettings;

            [System.Serializable]
            public class DamageOverTimeClass
            {
                [Tooltip("Controls the effect that is spawned each tick.")]
                public GameObject DamageOverTimeEffect;
                [Range(0.5f, 5)]
                public float OverTimeEffectTimeOutSeconds = 1.5f;
                [Tooltip("Controls the length (in seconds) how often Damage Over Time is applied.")]
                [Range(0.1f, 10f)]
                public float TickRate = 1;
                [Tooltip("Controls the amount of damage that happens per Tick Rate.")]
                public int DamagePerTick = 1;
                [Range(0f, 10f)]
                [Tooltip("Controls the length (in seconds) the Damage Over Time effect will last.")]
                public float DamageOverTimeLength = 3;
                [Tooltip("Controls the sound that will play each tick.")]
                public List<AudioClip> OverTimeSounds = new List<AudioClip>();
            }

            /// <summary>
            /// Generates the initial damage dealt by an ability (including base damage, randomized damage, and/or critical Hits). Note: This calculation excludes damage over time.
            /// </summary>
            /// <returns></returns>
            public int GenerateDamage (bool IsCritHit)
            {
                int DamageAmount = 0;
                if (!BaseDamageSettings.UseRandomAmounts) DamageAmount = BaseDamageSettings.BaseAmount;
                else if (BaseDamageSettings.UseRandomAmounts) DamageAmount = Random.Range(BaseDamageSettings.MinAmount, BaseDamageSettings.MaxAmount + 1);
                if (UseCriticalHits && IsCritHit) DamageAmount = DamageAmount + Mathf.FloorToInt(DamageAmount * (CriticalHitSettings.CriticalHitMultiplier * 0.01f));

                return DamageAmount;
            }

            /// <summary>
            /// (Uses the AbilityData.DamageData class) Initializes a damage over time component to continuously damage the target, given the ability uses damage overtime.
            /// </summary>
            public void DamageTargetOverTime(EmeraldAbilityObject AbilityObject, DamageData DamageDataInfo, GameObject Owner, GameObject Target)
            {
                if (UseDamageOverTime)
                {
                    if (IDamageableHelper.CheckAbilityActiveEffects(Target, AbilityObject))
                    {
                        IDamageableHelper.AddAbilityActiveEffect(Target, AbilityObject); //Add the ability data to the target's ActiveEffect list, given that it doesn'transform already exist.
                        GameObject SpawnedDamageOverTimeComponent = EmeraldObjectPool.Spawn(Resources.Load("Damage Over Time Component") as GameObject, Target.transform.position, Quaternion.identity);
                        SpawnedDamageOverTimeComponent.GetComponent<EmeraldDamageOverTime>().Initialize(AbilityObject, DamageDataInfo, Target.transform, Owner.transform);
                    }
                }
            }

            /// <summary>
            /// Returns true if the hit was a critical hit (uses CriticalHitOdds from the Damage Module).
            /// </summary>
            public bool GenerateCritHit()
            {
                bool CriticalHit = false;
                float m_GeneratedOdds = Random.Range(0.0f, 1.0f);
                m_GeneratedOdds = Mathf.RoundToInt(m_GeneratedOdds * 100);

                if (m_GeneratedOdds <= CriticalHitSettings.CriticalHitOdds)
                {
                    CriticalHit = true;
                }

                return CriticalHit;
            }
        }

        public enum TargetSources { Enemy, Ally }

        public enum TargetTypes
        {
            CurrentTarget,
            ClosestEnemy,
            SingleRandomEnemy,
            MultipleRandomEnemies,
            //TODO: Will add with update
            //ClosestAlly,
            //SingleRandomAlly,
            //MultipleRandomAllies,
            //RandomGroundPosition,
        }

        public enum SpreadTypes
        {
            Random,
            HorizontalRadius,
        }
    }
}