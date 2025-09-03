using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/sounds-component")]
    public class EmeraldSounds : MonoBehaviour
    {
        #region Variables
        public Utility.EmeraldSoundProfile SoundProfile;
        public bool SoundProfileFoldout;
        public bool HideSettingsFoldout;
        public int IdleSoundsSeconds;
        public float IdleSoundsTimer;

        public AudioSource m_AudioSource;
        public AudioSource m_SecondaryAudioSource;
        public AudioSource m_EventAudioSource;

        EmeraldSystem EmeraldComponent;
        EmeraldHealth EmeraldHealth;
        EmeraldItems EmeraldItems;
        #endregion

        void Awake()
        {
            InitializeSounds(); //Initialize the EmeraldSounds script.
        }

        /// <summary>
        /// Initializes the sound settings.
        /// </summary>
        public void InitializeSounds()
        {
            EmeraldHealth = GetComponent<EmeraldHealth>();
            EmeraldItems = GetComponent<EmeraldItems>();
            EmeraldComponent = GetComponent<EmeraldSystem>();

            //Do not subscribe to any delegates if the sound profile is null.
            if (SoundProfile == null)
                return;

            EmeraldHealth.OnTakeDamage += PlayInjuredSound; //Subscribe to the OnTakeDamage event for Injured Sounds
            EmeraldHealth.OnTakeCritDamage += PlayInjuredSound; //Subscribe to the OnTakeCritDamage event for Injured Sounds
            EmeraldHealth.OnBlock += PlayBlockSound; //Subscribe to the OnTakeDamage event for Block Sounds
            EmeraldHealth.OnDeath += PlayDeathSound; //Subscribe to the OnDeath event for Death Sounds

            if (EmeraldItems != null)
            {
                EmeraldItems.OnEquipWeapon += PlayEquipSound; //Subscribe to the OnEquipWeapon event for Equip Sounds
                EmeraldItems.OnUnequipWeapon += PlayUnequipSound; //Subscribe to the OnUnequipWeapon event for Unequip Sounds
            }

            IdleSoundsSeconds = Random.Range(SoundProfile.IdleSoundsSecondsMin, SoundProfile.IdleSoundsSecondsMax + 1);
            m_AudioSource = GetComponent<AudioSource>();
            m_SecondaryAudioSource = gameObject.AddComponent<AudioSource>();
            m_SecondaryAudioSource.priority = m_AudioSource.priority;
            m_SecondaryAudioSource.spatialBlend = m_AudioSource.spatialBlend;
            m_SecondaryAudioSource.minDistance = m_AudioSource.minDistance;
            m_SecondaryAudioSource.maxDistance = m_AudioSource.maxDistance;
            m_SecondaryAudioSource.rolloffMode = m_AudioSource.rolloffMode;
            m_EventAudioSource = gameObject.AddComponent<AudioSource>();
            m_EventAudioSource.priority = m_AudioSource.priority;
            m_EventAudioSource.spatialBlend = m_AudioSource.spatialBlend;
            m_EventAudioSource.minDistance = m_AudioSource.minDistance;
            m_EventAudioSource.maxDistance = m_AudioSource.maxDistance;
            m_EventAudioSource.rolloffMode = m_AudioSource.rolloffMode;
        }

        /// <summary>
        /// Play a random idle sound when the IdleSoundsSeconds have been met.
        /// </summary>
        public void IdleSoundsUpdate ()
        {
            IdleSoundsTimer += Time.deltaTime;
            if (IdleSoundsTimer >= IdleSoundsSeconds)
            {
                PlayIdleSound();
                IdleSoundsTimer = 0;
            }
        }

        /// <summary>
        /// Plays a sound clip according to the Clip parameter.
        /// </summary>
        public void PlaySoundClip(AudioClip Clip)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.volume = 1;
                m_AudioSource.PlayOneShot(Clip);
            }
            else if (!m_SecondaryAudioSource.isPlaying)
            {
                m_SecondaryAudioSource.volume = 1;
                m_SecondaryAudioSource.PlayOneShot(Clip);
            }
            else
            {
                m_EventAudioSource.volume = 1;
                m_EventAudioSource.PlayOneShot(Clip);
            }
        }

        /// <summary>
        /// Plays a sound clip according to the Clip parameter with optional volume control.
        /// </summary>
        public void PlayAudioClip(AudioClip Clip, float Volume = 1)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.volume = Volume;
                m_AudioSource.PlayOneShot(Clip);
            }
            else if (!m_SecondaryAudioSource.isPlaying)
            {
                m_SecondaryAudioSource.volume = Volume;
                m_SecondaryAudioSource.PlayOneShot(Clip);
            }
            else
            {
                m_EventAudioSource.volume = Volume;
                m_EventAudioSource.PlayOneShot(Clip);
            }
        }

        /// <summary>
        /// Plays a sound clip according to the Clip parameter with a customizable volume.
        /// </summary>
        public void PlaySoundClipWithVolume(AudioClip Clip, float Volume)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.volume = Volume;
                m_AudioSource.PlayOneShot(Clip);
            }
            else if (!m_SecondaryAudioSource.isPlaying)
            {
                m_SecondaryAudioSource.volume = Volume;
                m_SecondaryAudioSource.PlayOneShot(Clip);
            }
            else
            {
                m_EventAudioSource.volume = Volume;
                m_EventAudioSource.PlayOneShot(Clip);
            }
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayIdleSound()
        {
            if (SoundProfile && SoundProfile.IdleSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    AudioClip m_RandomIdleSoundClip = SoundProfile.IdleSounds[Random.Range(0, SoundProfile.IdleSounds.Count)];
                    if (m_RandomIdleSoundClip != null)
                    {
                        m_AudioSource.volume = SoundProfile.IdleVolume;
                        m_AudioSource.PlayOneShot(m_RandomIdleSoundClip);
                        IdleSoundsSeconds = Random.Range(SoundProfile.IdleSoundsSecondsMin, SoundProfile.IdleSoundsSecondsMax);
                        IdleSoundsSeconds = (int)m_RandomIdleSoundClip.length + IdleSoundsSeconds;
                    }
                }
                else
                {
                    AudioClip m_RandomIdleSoundClip = SoundProfile.IdleSounds[Random.Range(0, SoundProfile.IdleSounds.Count)];
                    if (m_RandomIdleSoundClip != null)
                    {
                        m_SecondaryAudioSource.volume = SoundProfile.IdleVolume;
                        m_SecondaryAudioSource.PlayOneShot(m_RandomIdleSoundClip);
                        IdleSoundsSeconds = Random.Range(SoundProfile.IdleSoundsSecondsMin, SoundProfile.IdleSoundsSecondsMax);
                        IdleSoundsSeconds = (int)m_RandomIdleSoundClip.length + IdleSoundsSeconds;
                    }
                }
            }
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayAttackSound()
        {
            if (SoundProfile.AttackSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.AttackVolume;
                    m_AudioSource.pitch = Mathf.Round(Random.Range(0.9f, 1.1f) * 10) / 10;
                    m_AudioSource.PlayOneShot(SoundProfile.AttackSounds[Random.Range(0, SoundProfile.AttackSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.AttackVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.AttackSounds[Random.Range(0, SoundProfile.AttackSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.AttackVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.AttackSounds[Random.Range(0, SoundProfile.AttackSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a equip sound based on your AI's Equip Weapon sounds (is called automatically through the EquipWeapon Animation Event).
        /// </summary>
        public void PlayEquipSound (string WeaponType)
        {
            if (WeaponType == "Weapon Type 1")
            {
                if (SoundProfile.UnsheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.EquipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.EquipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.UnsheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.UnsheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.UnsheatheWeapon);
                    }
                }
            }
            else if (WeaponType == "Weapon Type 2")
            {
                if (SoundProfile.RangedUnsheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.RangedEquipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.RangedEquipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.RangedUnsheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.RangedUnsheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.RangedUnsheatheWeapon);
                    }
                }
            }
        }

        /// <summary>
        /// Plays a unequip sound based on your AI's Unequip Weapon sounds (is called automatically through the UnequipWeapon Animation Event).
        /// </summary>
        public void PlayUnequipSound(string WeaponType)
        {
            if (WeaponType == "Weapon Type 1")
            {
                if (SoundProfile.SheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.UnequipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.UnequipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.SheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.SheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.SheatheWeapon);
                    }
                }
            }
            else if (WeaponType == "Weapon Type 2")
            {
                if (SoundProfile.RangedSheatheWeapon != null)
                {
                    m_AudioSource.volume = SoundProfile.RangedUnequipVolume;
                    m_SecondaryAudioSource.volume = SoundProfile.RangedUnequipVolume;

                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.PlayOneShot(SoundProfile.RangedSheatheWeapon);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.RangedSheatheWeapon);
                    }
                    else
                    {
                        m_EventAudioSource.PlayOneShot(SoundProfile.RangedSheatheWeapon);
                    }
                }
            }
        }

        /// <summary>
        /// Plays a random attack sound based on your AI's Attack Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayWarningSound()
        {
            if (SoundProfile.WarningSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.WarningVolume;
                    m_AudioSource.PlayOneShot(SoundProfile.WarningSounds[Random.Range(0, SoundProfile.WarningSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.WarningVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.WarningSounds[Random.Range(0, SoundProfile.WarningSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.WarningVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.WarningSounds[Random.Range(0, SoundProfile.WarningSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random block sound based on your AI's Block Sounds list.
        /// </summary>
        public void PlayBlockSound()
        {
            if (SoundProfile.BlockingSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.BlockVolume;
                    m_AudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    m_AudioSource.PlayOneShot(SoundProfile.BlockingSounds[Random.Range(0, SoundProfile.BlockingSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.BlockVolume;
                    m_SecondaryAudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.BlockingSounds[Random.Range(0, SoundProfile.BlockingSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.BlockVolume;
                    m_EventAudioSource.pitch = Mathf.Round(Random.Range(0.7f, 1.1f) * 10) / 10;
                    m_EventAudioSource.PlayOneShot(SoundProfile.BlockingSounds[Random.Range(0, SoundProfile.BlockingSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random injured sound based on your AI's Injured Sounds list.
        /// </summary>
        public void PlayInjuredSound()
        {
            int Odds = Random.Range(1, 101);
            if (Odds > SoundProfile.InjuredSoundOdds) return;

            if (SoundProfile.InjuredSounds.Count > 0 && !EmeraldComponent.AnimationComponent.IsBlocking)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.InjuredVolume;
                    m_AudioSource.pitch = Mathf.Round(Random.Range(0.8f, 1.1f) * 10) / 10;
                    m_AudioSource.PlayOneShot(SoundProfile.InjuredSounds[Random.Range(0, SoundProfile.InjuredSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.InjuredVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.InjuredSounds[Random.Range(0, SoundProfile.InjuredSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.InjuredVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.InjuredSounds[Random.Range(0, SoundProfile.InjuredSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// Plays a random death sound based on your AI's Death Sounds list. Can also be called through Animation Events.
        /// </summary>
        public void PlayDeathSound()
        {
            if (SoundProfile.DeathSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = SoundProfile.DeathVolume;
                    m_AudioSource.PlayOneShot(SoundProfile.DeathSounds[Random.Range(0, SoundProfile.DeathSounds.Count)]);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = SoundProfile.DeathVolume;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.DeathSounds[Random.Range(0, SoundProfile.DeathSounds.Count)]);
                }
                else
                {
                    m_EventAudioSource.volume = SoundProfile.DeathVolume;
                    m_EventAudioSource.PlayOneShot(SoundProfile.DeathSounds[Random.Range(0, SoundProfile.DeathSounds.Count)]);
                }
            }
        }

        /// <summary>
        /// If using a Footstep Component, this creates a footstep effect and sound based on the detected surface of the footstep (requires a Footstep Component that has been set up).
        /// If NOT using a Footstep Component, this plays a random footstep sound based off of your AI's Walk Sound List.
        /// </summary>
        public void Footstep()
        {
            if (GetComponent<EmeraldFootsteps>() != null) return;

            if (EmeraldComponent.MovementComponent.CanPlayWalkFootstepSound() || EmeraldComponent.MovementComponent.CanPlayRunFootstepSound())
            {
                float StepVolume = EmeraldComponent.MovementComponent.CanPlayWalkFootstepSound() ? SoundProfile.WalkFootstepVolume : SoundProfile.RunFootstepVolume;

                if (SoundProfile.FootStepSounds.Count > 0)
                {
                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.volume = StepVolume;
                        m_AudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.volume = StepVolume;
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else
                    {
                        m_EventAudioSource.volume = StepVolume;
                        m_EventAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is walking. This should be setup through an Animation Event.
        /// </summary>
        public void WalkFootstepSound()
        {
            if (GetComponent<EmeraldFootsteps>() != null) return;

            if (EmeraldComponent.MovementComponent.CanPlayWalkFootstepSound())
            {
                if (SoundProfile.FootStepSounds.Count > 0)
                {
                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.volume = SoundProfile.WalkFootstepVolume;
                        m_AudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.volume = SoundProfile.WalkFootstepVolume;
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else
                    {
                        m_EventAudioSource.volume = SoundProfile.WalkFootstepVolume;
                        m_EventAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// Plays a footstep sound from the AI's Footstep Sounds list to use when the AI is running. This should be setup through an Animation Event.
        /// </summary>
        public void RunFootstepSound()
        {
            if (GetComponent<EmeraldFootsteps>() != null) return;

            if (EmeraldComponent.MovementComponent.CanPlayRunFootstepSound())
            {
                if (SoundProfile.FootStepSounds.Count > 0)
                {
                    if (!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.volume = SoundProfile.RunFootstepVolume;
                        m_AudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else if (!m_SecondaryAudioSource.isPlaying)
                    {
                        m_SecondaryAudioSource.volume = SoundProfile.RunFootstepVolume;
                        m_SecondaryAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                    else
                    {
                        m_EventAudioSource.volume = SoundProfile.RunFootstepVolume;
                        m_EventAudioSource.PlayOneShot(SoundProfile.FootStepSounds[Random.Range(0, SoundProfile.FootStepSounds.Count)]);
                    }
                }
            }
        }

        /// <summary>
        /// Plays a random sound effect from the AI's General Sounds list.
        /// </summary>
        public void PlayRandomSoundEffect()
        {
            if (SoundProfile.InteractSounds.Count > 0)
            {
                if (!m_AudioSource.isPlaying)
                {
                    m_AudioSource.volume = 1;
                    m_AudioSource.PlayOneShot(SoundProfile.InteractSounds[Random.Range(0, SoundProfile.InteractSounds.Count)].SoundEffectClip);
                }
                else if (!m_SecondaryAudioSource.isPlaying)
                {
                    m_SecondaryAudioSource.volume = 1;
                    m_SecondaryAudioSource.PlayOneShot(SoundProfile.InteractSounds[Random.Range(0, SoundProfile.InteractSounds.Count)].SoundEffectClip);
                }
                else
                {
                    m_EventAudioSource.volume = 1;
                    m_EventAudioSource.PlayOneShot(SoundProfile.InteractSounds[Random.Range(0, SoundProfile.InteractSounds.Count)].SoundEffectClip);
                }
            }
        }

        /// <summary>
        /// Plays a sound effect from the AI's General Sounds list using the Sound Effect ID as the parameter.
        /// </summary>
        public void PlaySoundEffect(int SoundEffectID)
        {
            if (SoundProfile.InteractSounds.Count > 0)
            {
                for (int i = 0; i < SoundProfile.InteractSounds.Count; i++)
                {
                    if (SoundProfile.InteractSounds[i].SoundEffectID == SoundEffectID)
                    {
                        if (!m_AudioSource.isPlaying)
                        {
                            m_AudioSource.volume = 1;
                            m_AudioSource.PlayOneShot(SoundProfile.InteractSounds[i].SoundEffectClip);
                        }
                        else if (!m_SecondaryAudioSource.isPlaying)
                        {
                            m_SecondaryAudioSource.volume = 1;
                            m_SecondaryAudioSource.PlayOneShot(SoundProfile.InteractSounds[i].SoundEffectClip);
                        }
                        else
                        {
                            m_EventAudioSource.volume = 1;
                            m_EventAudioSource.PlayOneShot(SoundProfile.InteractSounds[i].SoundEffectClip);
                        }
                    }
                }
            }
        }
    }
}