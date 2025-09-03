using UnityEngine;
using System.Collections.Generic;

namespace EmeraldAI
{
    /// <summary>
    /// 伤害能力接口.
    /// An interafce script used for monitoring and tracking a target. This allows other AI to see any target's information by using customizable functions.
    /// 用于监视和跟踪目标的接口脚本.这允许其他AI通过使用可定制的功能来查看任何目标的信息
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 伤害.DamageAmount`:伤害值`AttackerTransform`: 攻击者的变换(可选)`RagdollForce`: 刚体物理力(可选,默认为100)`CriticalHit`: 是否为暴击(可选,默认为`false`)
        /// Used for passing damage to any script that has an IDamageable component.
        /// 用于将损害传递给任何具有idamagable组件的脚本.
        /// </summary>
        void Damage(int DamageAmount, Transform AttackerTransform = null, int RagdollForce = 100, bool CriticalHit = false);
        /// <summary>
        /// 生命值
        /// </summary>
        int Health { get; set; }
        /// <summary>
        /// 初始生命值
        /// </summary>
        int StartHealth { get; set; }

        /// <summary>
        /// 激活效果列表.
        /// Used for tracking active damage over time effects on targets.
        /// 用于跟踪主动伤害随时间对目标的影响.
        /// </summary>
        List<string> ActiveEffects { get; set; }
    }

    /// <summary>
    /// 伤害辅助
    /// </summary>
    public static class IDamageableHelper
    {
        /// <summary>
        /// 目标是否已死亡
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns></returns>
        public static bool IsDead (this GameObject receiver)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null) return m_IDamageable.Health <= 0;
            else return false;
        }
        /// <summary>
        /// 目标是否有指定的技能效果
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="AbilityData"></param>
        /// <returns></returns>
        public static bool CheckAbilityActiveEffects (this GameObject receiver, EmeraldAbilityObject AbilityData)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                return !m_IDamageable.ActiveEffects.Contains(AbilityData.AbilityName) && AbilityData.AbilityName != string.Empty;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 为目标添加指定的技能效果
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="AbilityData"></param>
        public static void AddAbilityActiveEffect(this GameObject receiver, EmeraldAbilityObject AbilityData)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                if (!m_IDamageable.ActiveEffects.Contains(AbilityData.AbilityName) && AbilityData.AbilityName != string.Empty)
                {
                    m_IDamageable.ActiveEffects.Add(AbilityData.AbilityName);
                }
            }
        }
        /// <summary>
        /// 为目标移除指定的技能效果
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="AbilityData"></param>
        public static void RemoveAbilityActiveEffect(this GameObject receiver, EmeraldAbilityObject AbilityData)
        {
            var m_IDamageable = receiver.GetComponent<IDamageable>();
            if (m_IDamageable != null)
            {
                if (m_IDamageable.ActiveEffects.Contains(AbilityData.AbilityName) && AbilityData.AbilityName != string.Empty)
                {
                    m_IDamageable.ActiveEffects.Remove(AbilityData.AbilityName);
                }
            }
        }
    }
}

// 作用概述 

// 该代码片段定义了一个用于监测和追踪目标的接口 `IDamageable` 及其辅助类 `IDamageableHelper`.这些组件在 Unity 游戏开发中用于处理角色或对象的伤害、生命值管理和持续效果追踪.以下是各部分的具体作用:

// 1. 接口 `IDamageable`

// `IDamageable` 接口定义了以下几个方法和属性:

// - 方法 `Damage`:
//   - 作用: 用于传递伤害给实现了 `IDamageable` 接口的脚本.
//   - 参数:
//     - `DamageAmount`: 伤害值.
//     - `AttackerTransform`: 攻击者的变换(可选).
//     - `RagdollForce`: 刚体物理力(可选,默认为100).
//     - `CriticalHit`: 是否为暴击(可选,默认为`false`).

// - 属性 `Health`:
//   - 作用: 获取或设置当前生命值.

// - 属性 `StartHealth`:
//   - 作用: 获取或设置初始生命值.

// - 属性 `ActiveEffects`:
//   - 作用: 用于跟踪目标上的活跃持续效果,类型为 `List`.

// 2. 辅助类 `IDamageableHelper`

// `IDamageableHelper` 类提供了几个静态扩展方法,用于简化对 `IDamageable` 接口的常用操作:

// - 方法 `IsDead`:
//   - 作用: 检查目标是否已死亡.
//   - 参数:
//     - `receiver`: 要检查的目标对象.
//   - 返回值: 若目标的生命值小于等于0,则返回 `true`,否则返回 `false`.

// - 方法 `CheckAbilityActiveEffects`:
//   - 作用: 检查目标是否有指定的技能效果.
//   - 参数:
//     - `receiver`: 要检查的目标对象.
//     - `AbilityData`: 技能数据对象,包含技能名称等信息.
//   - 返回值: 若目标没有该技能效果且技能名称不为空,则返回 `true`,否则返回 `false`.

// - 方法 `AddAbilityActiveEffect`:
//   - 作用: 为目标添加指定的技能效果.
//   - 参数:
//     - `receiver`: 要添加效果的目标对象.
//     - `AbilityData`: 技能数据对象,包含技能名称等信息.
//   - 操作: 若目标没有该技能效果且技能名称不为空,则将技能名称添加到 `ActiveEffects` 列表中.

// - 方法 `RemoveAbilityActiveEffect`:
//   - 作用: 为目标移除指定的技能效果.
//   - 参数:
//     - `receiver`: 要移除效果的目标对象.
//     - `AbilityData`: 技能数据对象,包含技能名称等信息.
//   - 操作: 若目标有该技能效果且技能名称不为空,则将技能名称从 `ActiveEffects` 列表中移除.

// 应用场景

// 1. 角色伤害管理:
//    - 在战斗系统中,可以使用 `Damage` 方法来处理角色受到的伤害,并更新其生命值.
//    - 使用 `IsDead` 方法来判断角色是否已经死亡,以便进行相应的逻辑处理(如显示死亡动画、移除角色等).

// 2. 持续效果管理:
//    - 使用 `AddAbilityActiveEffect` 和 `RemoveAbilityActiveEffect` 方法来管理角色身上的持续效果(如中毒、燃烧等).
//    - 使用 `CheckAbilityActiveEffects` 方法来检查角色是否已经受到某种持续效果的影响,避免重复施加相同的效果.

// 3. AI 行为决策:
//    - AI 系统可以通过 `IDamageable` 接口获取目标的信息,从而做出更智能的决策(如选择攻击目标、评估威胁等级等).

// 通过这些功能,开发者可以更灵活地管理和控制游戏中的角色状态,提升游戏的可玩性和互动性.