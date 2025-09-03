using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    [CreateAssetMenu(fileName = "New Sound Profile", menuName = "Emerald AI/Sound Profile")]
    public class EmeraldSoundProfile : ScriptableObject
    {
        public bool IdleSoundsFoldout;
        public bool FootstepSoundsFoldout;
        public bool InteractSoundsFoldout;
        public bool EquipAndUnequipSoundsFoldout;
        public bool AttackSoundsFoldout;
        public bool InjuredSoundsFoldout;
        public bool BlockSoundsFoldout;
        public bool DeathSoundsFoldout;
        public bool WarningSoundsFoldout;

        public int IdleSoundsSeconds;
        public int IdleSoundsSecondsMin = 5;
        public int IdleSoundsSecondsMax = 10;
        public float IdleSoundsTimer;
        public AudioClip SheatheWeapon;
        public AudioClip UnsheatheWeapon;
        public AudioClip RangedSheatheWeapon;
        public AudioClip RangedUnsheatheWeapon;
        public List<AudioClip> IdleSounds = new List<AudioClip>();
        public List<AudioClip> AttackSounds = new List<AudioClip>();
        public List<AudioClip> InjuredSounds = new List<AudioClip>();
        public List<AudioClip> WarningSounds = new List<AudioClip>();
        public List<AudioClip> DeathSounds = new List<AudioClip>();
        public List<AudioClip> FootStepSounds = new List<AudioClip>();
        public List<AudioClip> BlockingSounds = new List<AudioClip>();
        public float IdleVolume = 1;
        public float WalkFootstepVolume = 0.1f;
        public float RunFootstepVolume = 0.1f;
        public float BlockVolume = 0.65f;
        public int InjuredSoundOdds = 100;
        public float InjuredVolume = 1;
        public float AttackVolume = 1;
        public float WarningVolume = 1;
        public float DeathVolume = 0.7f;
        public float EquipVolume = 1;
        public float UnequipVolume = 1;
        public float RangedEquipVolume = 1;
        public float RangedUnequipVolume = 1;

        [SerializeField]
        public List<InteractSoundClass> InteractSounds = new List<InteractSoundClass>();
        [System.Serializable]
        public class InteractSoundClass
        {
            public int SoundEffectID = 1;
            public AudioClip SoundEffectClip;
        }
    }
}