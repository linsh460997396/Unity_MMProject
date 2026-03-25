using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// 攻击类(存储和管理AI攻击信息),用于管控AI攻击行为.
    /// </summary>
    [System.Serializable]
    public class AttackClass // Holds all of an AI's attack attack information.
    {
        /// <summary>
        /// 表示当前攻击列表中的索引位置
        /// </summary>
        public int AttackListIndex = 0;
        /// <summary>
        /// 决定攻击选择的方式(例如按顺序、随机等)
        /// </summary>
        public AttackPickTypes AttackPickType = AttackPickTypes.Order;
        /// <summary>
        /// 存储一系列AttackData(攻击数据)对象的列表,列表每个元素(攻击数据对象)都包含具体的攻击信息
        /// </summary>
        [SerializeField] public List<AttackData> AttackDataList = new List<AttackData>();

        /// <summary>
        /// 攻击数据管理类
        /// </summary>
        [System.Serializable] public class AttackData
        {
            /// <summary>
            /// 攻击技能的目标对象,可能是一个脚本或预设
            /// </summary>
            public EmeraldAbilityObject AbilityObject;
            /// <summary>
            /// 攻击动画的索引
            /// </summary>
            public int AttackAnimation;
            /// <summary>
            /// 攻击发生的概率,默认为 25%
            /// </summary>
            public int AttackOdds = 25;
            /// <summary>
            /// 攻击的有效距离,默认为3
            /// </summary>
            public float AttackDistance = 3f;
            /// <summary>
            /// 过近的距离,AI可能会避免在这个距离内发动攻击,默认为1
            /// </summary>
            public float TooCloseDistance = 1f;
            /// <summary>
            /// 是否忽略冷却时间
            /// </summary>
            public bool CooldownIgnored;
            /// <summary>
            /// 冷却时间的时间戳
            /// </summary>
            public float CooldownTimeStamp;

            /// <summary>
            /// 检查 `AttackDataList`(攻击数据列表) 中是否包含某个特定的 `AttackData`(攻击数据) 对象.
            /// </summary>
            /// <param name="m_AttackDataList">攻击数据列表</param>
            /// <param name="m_AttackDataClass">攻击数据类型</param>
            /// <returns>攻击数据列表包含攻击数据时返回真,否则返回假</returns>
            public bool Contains(List<AttackData> m_AttackDataList, AttackData m_AttackDataClass)
            {
                return m_AttackDataList.Contains(m_AttackDataClass); //检查列表中是否存在某个攻击数据其实应该这样写
            }
            /// <summary>
            /// 检查 `AttackDataList`(攻击数据列表) 中是否包含某个特定的 `AttackData`(攻击数据) 对象.
            /// </summary>
            /// <param name="m_AttackDataList">攻击数据列表</param>
            /// <param name="m_AttackDataClass">攻击数据类型</param>
            /// <returns>攻击数据必须==列表第一个时才返回真,否则均返回假</returns>
            public bool FirstContains(List<AttackData> m_AttackDataList, AttackData m_AttackDataClass)
            {//原Contains,翡翠AI自己demo没有使用这个明显写错的函数,这里留个备份
                foreach (AttackData AttackInfo in m_AttackDataList)
                {
                    //故意只判断攻击数据必须==列表第一个时才返回真
                    return (AttackInfo == m_AttackDataClass);
                }
                //↓若没有找到返回假(上面for循环已写对比第一个就返回真或假所以永远不会轮到,此处为防止报错才添加)
                return false;
            }
        }
    }
}
