using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    [CreateAssetMenu(fileName = "New Animation Profile", menuName = "Emerald AI/Animation Profile")]
    public class AnimationProfile : ScriptableObject
    {
        public EmeraldAnimation EmeraldAnimationComponent; 
        public RuntimeAnimatorController AIAnimator;
        public bool AnimatorControllerGenerated;
        public bool AnimationsUpdated;
        public bool AnimationListsChanged = false;
        public bool MissingRuntimeController;
        public string FilePath;
        public AnimatorCullingMode AnimatorCullingMode = AnimatorCullingMode.AlwaysAnimate;

        public bool WalkFoldout;
        public bool RunFoldout;
        public bool TurnFoldout;
        public bool NonCombatDeathFoldout;
        public bool NonCombatAnimationsFoldout;
        public bool NonCombatIdleFoldout;
        public bool NonCombatHitFoldout;
        public bool EmotesFoldout;

        public bool Type1IdleFoldout;
        public bool Type2IdleFoldout;
        public bool Type1AttacksFoldout;
        public bool Type2AttacksFoldout;
        public bool Type1EquipsFoldout;
        public bool Type2EquipsFoldout;
        public bool Type1CombatAnimationsFoldout;
        public bool Type2CombatAnimationsFoldout;
        public bool Type1DeathFoldout;
        public bool Type2DeathFoldout;
        public bool Type1HitFoldout;
        public bool Type2HitFoldout;
        public bool Type1BlockFoldout;
        public bool Type2BlockFoldout;
        public bool Type1CombatWalkFoldout;
        public bool Type1CombatRunFoldout;
        public bool Type1CombatTurnFoldout;
        public bool Type2CombatWalkFoldout;
        public bool Type2CombatRunFoldout;
        public bool Type2CombatTurnFoldout;
        public bool Type1StrafeFoldout;
        public bool Type2StrafeFoldout;
        public bool Type1DodgeFoldout;
        public bool Type2DodgeFoldout;
        public bool AnimationProfileFoldout;
        public bool AnimatorSettingsFoldout;

        public AnimationStateTypes Type1HitConditions = AnimationStateTypes.Everything;
        public AnimationStateTypes Type2HitConditions = AnimationStateTypes.Everything;

        public float Type1HitAnimationCooldown = 0.1f;
        public float Type2HitAnimationCooldown = 0.1f;

        public List<EmoteAnimationClass> EmoteAnimationList = new List<EmoteAnimationClass>();

        [SerializeField]
        public AnimationParentClass NonCombatAnimations;
        [SerializeField]
        public AnimationParentClass Type1Animations;
        [SerializeField]
        public AnimationParentClass Type2Animations;
    }
}