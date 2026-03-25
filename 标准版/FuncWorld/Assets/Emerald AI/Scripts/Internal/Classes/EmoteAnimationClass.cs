using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 表情动画类(管理和存储角色表情动画的相关信息)
    /// </summary>
    [System.Serializable]
    public class EmoteAnimationClass
    {
        /// <summary>
        /// 通过构造函数创建表情动画类(管理和存储角色表情动画的相关信息)
        /// </summary>
        /// <param name="NewAnimationID">表情动画ID</param>
        /// <param name="NewEmoteAnimationClip">一个 `AnimationClip` 对象,表示具体的表情动画片段</param>
        public EmoteAnimationClass(int NewAnimationID, AnimationClip NewEmoteAnimationClip)
        {
            AnimationID = NewAnimationID;
            EmoteAnimationClip = NewEmoteAnimationClip;
        }
        /// <summary>
        /// 表情动画ID
        /// </summary>
        public int AnimationID = 1;
        /// <summary>
        /// 一个 `AnimationClip` 对象,表示具体的表情动画片段
        /// </summary>
        public AnimationClip EmoteAnimationClip;
    }
}