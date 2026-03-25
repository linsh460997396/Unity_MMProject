using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 可回避的.该接口的主要作用是允许AI系统检测并避开某些特定的目标或能力
    /// </summary>
    public interface IAvoidable
    {
        // Used to allow AI to detect which objects/abilities they should avoid. 
        // The AbilityTarget is used to determine which target the ability is intended for.

        /// <summary>
        /// 技能目标(的变换属性)
        /// </summary>
        Transform AbilityTarget { get; set; }
    }
}