using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 战斗.用于监视和跟踪目标的战斗行动(以及它的伤害位置)的接口脚本,这使得其他AI可以通过功能看到任何目标的信息.
    /// 注意:这个接口是必需的(在第三方或自定义角色控制器上)使用玩家目标的阻挡和躲避功能.
    /// </summary>
    public interface ICombat
    {
        // An interafce script used for monitoring and tracking a target's combat actions (as well as its damage position). This allows other AI to see any target's information through functions. 
        // Note: This interface is required (on 3rd party or custom character controllers) to use the blocking and dodging features for player targets.

        /// <summary>
        /// Used for getting the transform of the target.获取目标变换属性
        /// </summary>
        Transform TargetTransform();

        /// <summary>
        /// Used for getting the damage position of the target.获取目标伤害位置
        /// </summary>
        Vector3 DamagePosition();

        /// <summary>
        /// Used for detecting when a target is attacking.检测目标是否正在攻击
        /// </summary>
        bool IsAttacking();

        /// <summary>
        /// Used for detecting when a target is blocking.检测目标是否正在格挡
        /// </summary>
        bool IsBlocking();

        /// <summary>
        /// Used for detecting when a target is dodging.检测目标是否正在闪避
        /// </summary>
        bool IsDodging();

        /// <summary>
        /// Used through Emerald AI to trigger stunned mechanics, however, can also be extended to trigger stunned mechanics through custom character controllers, given they have them.
        /// 通过Emerald AI触发眩晕机制(当然你也可以通过自定义角色控制器来扩展触发眩晕机制)
        /// </summary>
        void TriggerStun(float StunLength);
    }
}