using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EmeraldAI.SoundDetection
{
    /// <summary>
    /// 吸引修改器.用于在游戏中实现声音检测和吸引机制.依赖AudioSource组件.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AttractModifier : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// 玩家所属的派系
        /// </summary>
        public FactionClass PlayerFaction;
        /// <summary>
        /// 检测范围
        /// </summary>
        public int Radius = 10;
        /// <summary>
        /// 最小碰撞速度
        /// </summary>
        public float MinVelocity = 3.5f;
        /// <summary>
        /// 声音(音效)冷却时间(秒)
        /// </summary>
        public float SoundCooldownSeconds = 1f;
        /// <summary>
        /// 反应冷却时间(秒)
        /// </summary>
        public float ReactionCooldownSeconds = 1f;
        /// <summary>
        /// 触发层.默认情况下,使用所有图层来触发一个AttractModifier
        /// </summary>
        public LayerMask TriggerLayers = ~0; //By default, use all layers for triggering an AttractModifier
        /// <summary>
        /// AI层
        /// </summary>
        public LayerMask EmeraldAILayer;
        /// <summary>
        /// 触发类型(默认值:碰撞事件发生时触发)
        /// </summary>
        public TriggerTypes TriggerType = TriggerTypes.OnCollision;
        /// <summary>
        /// 吸引反应对象
        /// </summary>
        public ReactionObject AttractReaction;
        /// <summary>
        /// 是否仅检测敌对关系的AI(默认为真)
        /// </summary>
        public bool EnemyRelationsOnly = true;
        /// <summary>
        /// 触发声音列表(存储AudioClip(音频切片)类型对象)
        /// </summary>
        public List<AudioClip> TriggerSounds = new List<AudioClip>();
        /// <summary>
        /// 音频源
        /// </summary>
        AudioSource m_AudioSource;
        /// <summary>
        /// 反应已触发
        /// </summary>
        bool ReactionTriggered;
        /// <summary>
        /// 声音已触发
        /// </summary>
        bool SoundTriggered;
        #endregion

        #region Editor Variables.编辑器变量
        /// <summary>
        /// 控制是否折叠“设置”部分
        /// </summary>
        public bool HideSettingsFoldout;
        /// <summary>
        /// 控制是否折叠“吸引修改器”部分
        /// </summary>
        public bool AttractModifierFoldout;
        #endregion

        void Start()
        {
            //获取本类所挂对象的音频源组件实例对象
            m_AudioSource = GetComponent<AudioSource>();

            if (TriggerType == TriggerTypes.OnStart)
            {//若触发类型是设计了在开始时触发
                GetTargets(); //找到指定半径内的所有翡翠AI目标并调用吸引反应(触发层=真)
            }
        }

        /// <summary>
        /// Invokes the specified reaction during a trigger collision.在碰撞时调用指定的反应.
        /// </summary>
        private void OnTriggerEnter(Collider collision)
        {
            if (TriggerType == TriggerTypes.OnTrigger)
            {//若触发类型是设计了在碰撞时触发
                //找到指定半径内的所有翡翠AI目标并调用吸引反应(触发层需位运算判断真假)
                //位运算将1左移碰撞对象所在层数,并与触发层与运算的结果不为0,则触发层=真
                GetTargets(((1 << collision.gameObject.layer) & TriggerLayers) != 0); //括号内验证碰撞对象层是否在触发层
            }
        }

        /// <summary>
        /// Invokes the specified reaction during a collision that meets or exceeds the MinVelocity.
        /// 在满足或超过最小速度的碰撞时调用指定的反应.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (TriggerType == TriggerTypes.OnCollision && collision.relativeVelocity.magnitude >= MinVelocity)
            {//触发类型==在碰撞时触发 且 与碰撞对象的相对速度>=最小碰撞速度
                //找到指定半径内的所有翡翠AI目标并调用吸引反应(触发层需位运算判断真假)
                GetTargets(((1 << collision.gameObject.layer) & TriggerLayers) != 0); //括号内验证碰撞对象层是否在触发层
            }
        }

        /// <summary>
        /// Invokes the specified reaction when called (Requries the OnCustomCall TriggerType).
        /// 触发类型是自定义调用时激活指定的反应(事件是否触发,需要查询OnCustomCall TriggerType).
        /// </summary>
        public void ActivateAttraction ()
        {
            if (TriggerType == TriggerTypes.OnCustomCall)
            {//触发类型是自定义调用时
                GetTargets();//找到指定半径内的所有翡翠AI目标并调用吸引反应(触发层=真)
            }
        }

        /// <summary>
        /// 找到指定半径内的所有翡翠AI目标并调用吸引反应.
        /// </summary>
        /// <param name="HasTriggerLayer">拥有触发层,默认为真</param>
        void GetTargets (bool HasTriggerLayer = true)
        {// Find all Emerald AI targets within the specified radius and invoke the AttractReaction.

            //播放触发声音(音效)
            PlayTriggerSound();
            //若反应已触发或时间<0.5或没有触发层则直接返回
            if (ReactionTriggered || Time.time < 0.5f || !HasTriggerLayer)
                return;
            //在指定范围内检测符合特定层掩码的碰撞体(Colliders)并存储到数组
            Collider[] m_DetectedTargets = Physics.OverlapSphere(transform.position, Radius, EmeraldAILayer);
            //若数组为空则直接返回
            if (m_DetectedTargets.Length == 0)
                return;
            //遍历数组
            for (int i = 0; i < m_DetectedTargets.Length; i++)
            {
                if (m_DetectedTargets[i].GetComponent<EmeraldSoundDetector>() != null)
                {//碰撞对象拥有声音(音效)探测器组件时

                    EmeraldSystem EmeraldComponent = m_DetectedTargets[i].GetComponent<EmeraldSystem>(); //Cache each EmeraldSystem.缓存每个EmeraldSystem实例对象

                    //Don'transform allow AI with follower targets to use Attract Modifiers.不允许有随从目标的AI使用吸引修改器
                    if (EmeraldComponent.TargetToFollow != null) continue;

                    //Only allow AI with an Enemy relation to receive Attract Modifiers.只允许有敌人关系的AI接收吸引修改
                    if (EnemyRelationsOnly && EmeraldComponent.DetectionComponent.FactionRelationsList.Exists(x => x.FactionIndex == PlayerFaction.FactionIndex && x.RelationType != 0)) continue;
                    
                    if (AttractReaction != null)
                    {//吸引反应对象不为空
                        //将检测到的Emerald AI代理对象赋值到声音探测器组件的DetectedAttractModifier(已探测吸引修改器对象)字段
                        EmeraldComponent.SoundDetectorComponent.DetectedAttractModifier = gameObject; //Assign the detected Emerald AI agent as the DetectedAttractModifier
                        //激活声音探测器组件的调用反应列表(若成功,会以协程启动)
                        EmeraldComponent.SoundDetectorComponent.InvokeReactionList(AttractReaction, true); //Invoke the ReactionList.
                    }
                    else
                    {
                        Debug.Log("There's no Reaction Object on the " + gameObject.name + "'s AttractReaction slot. Please add one in order for Attract Modifier to work correctly.");
                    }
                }
            }

            ReactionTriggered = true;
            Invoke("ReactionCooldown", ReactionCooldownSeconds);
        }

        void PlayTriggerSound ()
        {
            if (SoundTriggered || Time.time < 0.5f)
                return;

            if (TriggerSounds.Count > 0)
                m_AudioSource.PlayOneShot(TriggerSounds[Random.Range(0, TriggerSounds.Count)]);

            SoundTriggered = true;
            Invoke("SoundCooldown", SoundCooldownSeconds);
        }

        void SoundCooldown()
        {
            SoundTriggered = false;
        }

        void ReactionCooldown ()
        {
            ReactionTriggered = false;
        }
    }
}

// `AttractModifier` 用于在游戏中实现声音检测和吸引机制.

// 1. 声音触发:当满足特定条件时,播放预定义的声音.
// 2. 目标检测:在指定范围内检测符合条件的目标,并触发相应的反应.
// 3. 冷却时间:设置声音和反应的冷却时间,防止频繁触发.

// 主要功能

// 1. 声音触发
// - 触发条件:
//   - `OnTrigger`:当物体进入触发器区域时.
//   - `OnCollision`:当物体碰撞且相对速度大于等于 `MinVelocity` 时.
//   - `OnCustomCall`:通过调用 `ActivateAttraction` 方法手动触发.
// - 声音播放:
//   - 播放 `TriggerSounds` 列表中的随机声音.
//   - 设置 `SoundTriggered` 标志,防止在冷却时间内重复播放.

// 2. 目标检测
// - 检测范围:在 `Radius` 范围内检测符合条件的目标.
// - 条件过滤:
//   - 目标必须属于 `EmeraldAILayer` 层.
//   - 若 `EnemyRelationsOnly` 为 `true`,则只检测与玩家派系敌对的目标.
//   - 排除已经跟随其他目标的 AI.
// - 反应触发:
//   - 调用目标的 `SoundDetectorComponent` 的 `InvokeReactionList` 方法,触发 `AttractReaction` 反应.

// 3. 冷却时间
// - 声音冷却:设置 `SoundCooldownSeconds` 秒的冷却时间,防止频繁播放声音.
// - 反应冷却:设置 `ReactionCooldownSeconds` 秒的冷却时间,防止频繁触发反应.

// 使用场景

// - 游戏开发:适用于需要实现声音检测和吸引机制的游戏,例如恐怖游戏、潜行游戏等.
// - AI 行为控制:通过声音触发 AI 的特定行为,增加游戏的互动性和沉浸感.

// 代码结构

// - 变量声明:
//   - `PlayerFaction`:玩家所属的派系.
//   - `Radius`:检测范围.
//   - `MinVelocity`:最小碰撞速度.
//   - `SoundCooldownSeconds` 和 `ReactionCooldownSeconds`:冷却时间.
//   - `TriggerLayers` 和 `EmeraldAILayer`:触发层和 AI 层.
//   - `TriggerType`:触发类型.
//   - `AttractReaction`:吸引反应对象.
//   - `EnemyRelationsOnly`:是否仅检测敌对关系的 AI.
//   - `TriggerSounds`:触发声音列表.

// - 方法:
//   - `Start`:初始化音频源,根据 `TriggerType` 调用 `GetTargets`.
//   - `OnTriggerEnter` 和 `OnCollisionEnter`:处理触发器和碰撞事件.
//   - `ActivateAttraction`:手动触发吸引.
//   - `GetTargets`:检测并触发目标的反应.
//   - `PlayTriggerSound`:播放触发声音.
//   - `SoundCooldown` 和 `ReactionCooldown`:重置冷却标志.

// 总结 

// `AttractModifier` 脚本通过声音检测和吸引机制,增强了游戏中的 AI 行为控制,提供了丰富的互动体验.通过合理的配置和使用,可以显著提升游戏的趣味性和挑战性.