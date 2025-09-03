using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 动画类(用于封装和管理与动画相关数据).主要作用是提供一种结构化的方式来管理和存储与单个动画相关的所有信息.
    /// 通过此类,开发者可轻松地创建和管理动画实例,包括设置动画的播放速度、指定要播放的动画剪辑以及控制动画是否应该被镜像.
    /// 在 Unity 游戏开发中,这样的类特别有用,因为它允许开发者在脚本中以一种清晰和可维护的方式处理动画.
    /// 例如,可在游戏对象上附加一个包含 AnimationClass 实例的组件,然后在运行时通过修改这个实例的字段来控制动画的行为.
    /// 此外,由于这个类是可序列化的,它还可被用于 Unity 的序列化系统,使得在 Unity 编辑器中直接编辑和查看这些动画属性成为可能.
    /// </summary>
    [System.Serializable] //类支持可序列化(通过 [Serializable] 特性标记),这意味着它的实例可被轻松地保存和加载,例如在 Unity 的 Inspector 面板中显示或者通过 JSON、XML 等格式进行存储和传输.
    public class AnimationClass
    {
        /// <summary>
        /// 通过构造函数创建AnimationClass实例(用于封装和管理与动画相关的数据)
        /// </summary>
        /// <param name="NewAnimationSpeed">动画的播放速度</param>
        /// <param name="NewAnimationClip">要播放的动画剪辑</param>
        /// <param name="NewMirror">指示动画是否应该被镜像(即左右翻转)</param>
        public AnimationClass(float NewAnimationSpeed, AnimationClip NewAnimationClip, bool NewMirror)
        {
            AnimationSpeed = NewAnimationSpeed;
            AnimationClip = NewAnimationClip;
            Mirror = NewMirror;
        }
        /// <summary>
        /// 动画的播放速度,默认值为 1,表示正常速度播放.
        /// </summary>
        public float AnimationSpeed = 1;
        /// <summary>
        /// 要播放的动画剪辑.
        /// </summary>
        public AnimationClip AnimationClip;
        /// <summary>
        /// 指示动画是否应该被镜像(即左右翻转),默认值为 false,表示不镜像.
        /// </summary>
        public bool Mirror = false;
    }
}
