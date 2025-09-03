using System.Collections.Generic;

namespace EmeraldAI
{
    /// <summary>
    /// 动画父类(用于统一管理游戏角色动画).它存储和组织不同类型的动画,以便在游戏开发过程中方便地管理和使用这些动画.
    /// 由于大多数动画都是重复的,每个动画类别(非战斗类,类型1和类型2)都使用父类,不使用的动画根本不显示,这也使得在未来的更新中更容易添加新的动画.
    /// </summary>
    [System.Serializable]
    public class AnimationParentClass
    {//Since most animations are repeated, a parent class is used for each animation category (NonCombat, Type 1, and Type 2).
     //Animations that are not used are simply not displayed.This also makes for adding new animations with future updates easier.
 
        /// <summary>
        /// List of Animations (NonCombat Only).闲置动画列表(非战斗)
        /// </summary>
        public List<AnimationClass> IdleList = new List<AnimationClass>();

        /// <summary>
        /// Stationary Idle Animation.站立静止闲置动画
        /// </summary>
        public AnimationClass IdleStationary;

        /// <summary>
        /// Warning Animation (Type 1 and Type 2 Only).警示闲置动画(只适用于第一类及第二类)
        /// </summary>
        public AnimationClass IdleWarning;

        /// <summary>
        /// Walk Animations.步行动画
        /// </summary>
        public AnimationClass WalkLeft, WalkForward, WalkRight, WalkBack;

        /// <summary>
        /// Run Animations.跑步动画
        /// </summary>
        public AnimationClass RunLeft, RunForward, RunRight;

        /// <summary>
        /// Turn Animations.转向动画
        /// </summary>
        public AnimationClass TurnLeft, TurnRight;

        /// <summary>
        /// List of Hit Animations.命中动画列表
        /// </summary>
        public List<AnimationClass> HitList = new List<AnimationClass>();

        /// <summary>
        /// List of Death Animations.死亡动画列表
        /// </summary>
        public List<AnimationClass> DeathList = new List<AnimationClass>();

        /// <summary>
        /// Strafe Animations (Type 1 and Type 2 Only).
        /// 侧移动画(仅限类型1和类型2).是指角色在即横向移动时所使用的动画.
        /// 角色在不前进或后退的情况下,向左或向右移动,这种动画在许多类型的游戏中都非常常见,尤其是在射击游戏和动作游戏中,用于增加角色的机动性和战术灵活性.
        /// </summary>
        public AnimationClass StrafeLeft, StrafeRight;

        /// <summary>
        /// Block Animations (Type 1 and Type 2 Only).招架动画(仅限类型1和类型2).是指角色在进行格挡或防御动作时所播放的动画.
        /// </summary>
        public AnimationClass BlockIdle, BlockHit;

        /// <summary>
        /// Dodge Animations (Type 1 and Type 2 Only).闪避动画(仅限类型1和类型2)
        /// </summary>
        public AnimationClass DodgeLeft, DodgeBack, DodgeRight;

        /// <summary>
        /// Recoil Animation (Type 1 and Type 2 Only).反冲动画(仅限类型1和类型2).模拟射击或受到打击时的后坐力效果的动画.
        /// </summary>
        public AnimationClass Recoil;

        /// <summary>
        /// Stunned Animation (Type 1 and Type 2 Only).
        /// 眩晕动画(仅限类型1和类型2).当角色受到某些攻击或处于特定状态时,角色会进入昏迷或无法行动的状态,并播放相应的动画效果,这种动画通常表现为角色身体僵硬、倒地或摇晃等.
        /// </summary>
        public AnimationClass Stunned;

        /// <summary>
        /// Equip and Unequip Animations (Type 1 and Type 2 Only).装备和解除装备动画(仅限类型1和类型2)
        /// </summary>
        public AnimationClass PutAwayWeapon, PullOutWeapon;

        /// <summary>
        /// List of Attack Animations (Type 1 and Type 2 Only).攻击动画列表
        /// </summary>
        public List<AnimationClass> AttackList = new List<AnimationClass>(); 
    }
}
