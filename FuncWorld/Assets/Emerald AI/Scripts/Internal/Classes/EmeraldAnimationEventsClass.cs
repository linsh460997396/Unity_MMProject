using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 动画事件类(帮助管理和触发动画事件)
    /// </summary>
    public class EmeraldAnimationEventsClass
    {
        /// <summary>
        /// 事件的显示名称,用于在编辑器或日志中标识该事件
        /// </summary>
        public string eventDisplayName;
        /// <summary>
        /// 事件的描述,提供关于该事件的详细信息
        /// </summary>
        public string eventDescription;
        /// <summary>
        /// 一个Unity的`AnimationEvent`对象,包含触发事件的具体信息,如时间点、函数名、参数等
        /// </summary>
        public AnimationEvent animationEvent;

        /// <summary>
        /// 动画事件类(帮助管理和触发动画事件)
        /// </summary>
        /// <param name="m_eventDisplayName">事件的显示名称,用于在编辑器或日志中标识该事件</param>
        /// <param name="m_animationEvent">一个Unity的`AnimationEvent`对象,包含触发事件的具体信息,如时间点、函数名、参数等</param>
        /// <param name="m_eventDescription">事件的描述,提供关于该事件的详细信息</param>
        public EmeraldAnimationEventsClass(string m_eventDisplayName, AnimationEvent m_animationEvent, string m_eventDescription)
        {//构造函数
            //(new)构建时给3个字段赋值
            eventDisplayName = m_eventDisplayName;
            animationEvent = m_animationEvent;
            eventDescription = m_eventDescription;
        }
    }
}

//使用 `EmeraldAnimationEventsClass` 来管理动画事件范例:
//     public class AnimationEventManager : MonoBehaviour
//     {
//         public List animationEvents = new List();

//         void Start()
//         {
//             // 创建并添加Unity官方动画事件
//             AnimationEvent playSoundEvent = new AnimationEvent
//             {
//                 functionName = "PlaySound",
//                 time = 1.5f,
//                 objectReferenceParameter = GetComponent()
//             };

//             EmeraldAnimationEventsClass soundEvent = new EmeraldAnimationEventsClass(
//                 "Play Sound Event",
//                 playSoundEvent,
//                 "Plays a sound effect at 1.5 seconds into the animation"
//             );

//             animationEvents.Add(soundEvent);

//             // 注册动画事件(动画切片与1或N个事件进行绑定,在事件发生时自动播放)
//             AnimationClip clip = GetComponent().clip;
//             clip.AddEvent(playSoundEvent);
//         }

//         void PlaySound(object obj)
//         {
//             AudioSource audioSource = obj as AudioSource;
//             if (audioSource != null)
//             {
//                 audioSource.Play();
//             }
//         }
//     }
//-------------------------------------------------------------------------------------------------
// `AnimationClip` 是 Unity 引擎中的一个重要类,用于表示和管理动画数据.它在游戏开发中广泛应用于角色动画、UI 动画、物体运动等多种场景.

// 1. 基本概念 
// - 定义: `AnimationClip` 是一个包含动画数据的类,这些数据描述了对象在一段时间内的变化.
// - 用途: 用于存储和播放动画,可以应用于角色、UI 元素、环境物体等.

// 2. 主要属性
// - length: 动画的总时长(以秒为单位).
// - frameRate: 动画的帧率(每秒帧数).
// - wrapMode: 动画的循环模式,常见的有 `Once`(播放一次)、`Loop`(循环播放)、`PingPong`(来回播放)等.
// - localBounds: 动画的边界框,用于物理碰撞检测等.
// - events: 动画事件列表,用于在动画的特定时间点触发事件.

// 3. 主要方法
// - SampleAnimation: 采样动画的关键帧数据,用于手动控制动画播放.
// - AddEvent: 添加一个动画事件,指定在动画的某个时间点触发.
// - GetCurves: 获取动画曲线,用于自定义动画数据.
// - SetCurve: 设置动画曲线,用于修改动画数据.

// 4. 使用场景
// - 角色动画: 为游戏角色添加行走、跑步、跳跃等动画.
// - UI 动画: 为 UI 元素添加过渡效果,如按钮点击动画、菜单展开动画等.
// - 物体运动: 为环境物体添加运动动画,如门的开关、风车的旋转等.
// - 动画事件: 在动画的特定时间点触发事件,如播放音效、改变角色状态等.

// 示例代码

// 以下是一个简单的示例,展示如何在 Unity 中使用 `AnimationClip`:

// 1. 创建和配置 `AnimationClip`
// ```csharp
// using UnityEngine;

// public class AnimationClipExample : MonoBehaviour
// {
//     public AnimationClip walkAnimation;
//     public AnimationClip jumpAnimation;

//     private Animation anim;

//     void Start()
//     {
//         // 获取组件 
//         anim = GetComponent();

//         // 添加动画剪辑
//         anim.AddClip(walkAnimation, "Walk");
//         anim.AddClip(jumpAnimation, "Jump");

//         // 播放行走动画
//         anim.Play("Walk");
//     }

//     void Update()
//     {
//         // 检测输入,切换动画
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             anim.Play("Jump");
//         }
//     }
// }
// ```

// 2. 添加动画事件
// ```csharp
// using UnityEngine;

// public class AnimationEventExample : MonoBehaviour 
// {
//     public AnimationClip animatedClip;

//     private Animation anim;

//     void Start()
//     {
//         // 获取组件 
//         anim = GetComponent();

//         // 添加动画剪辑
//         anim.AddClip(animatedClip, "AnimatedClip");

//         // 添加动画事件
//         AnimationEvent event1 = new AnimationEvent
//         {
//             functionName = "OnFootstep",
//             time = 0.5f 
//         };

//         AnimationEvent event2 = new AnimationEvent 
//         {
//             functionName = "OnFootstep",
//             time = 1.0f
//         };

//         animatedClip.AddEvent(event1);
//         animatedClip.AddEvent(event2);

//         // 播放动画
//         anim.Play("AnimatedClip");
//     }

//     void OnFootstep()
//     {
//         Debug.Log("Footstep sound played at " + Time.time);
//     }
// }
// ```

// 解释
// 1. 创建和配置 `AnimationClip`:
//    - 在 `Start` 方法中,获取 `Animation` 组件并添加两个动画剪辑:`walkAnimation` 和 `jumpAnimation`.
//    - 使用 `anim.Play` 方法播放指定的动画.
//    - 在 `Update` 方法中,检测按键输入并切换动画.

// 2. 添加动画事件:
//    - 在 `Start` 方法中,创建两个 `AnimationEvent` 对象,分别在动画的 0.5 秒和 1.0 秒处触发 `OnFootstep` 方法.
//    - 使用 `animatedClip.AddEvent` 方法将动画事件添加到动画剪辑中.
//    - 定义 `OnFootstep` 方法,在事件触发时输出日志信息.

// 通过以上示例,可以看出 `AnimationClip` 在 Unity 中的强大功能,不仅可以用于播放动画,还可以在动画的特定时间点触发事件,实现更复杂的游戏逻辑.