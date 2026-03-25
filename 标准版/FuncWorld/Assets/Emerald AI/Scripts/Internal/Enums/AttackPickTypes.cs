namespace EmeraldAI
{
    /// <summary>
    /// 攻击选择类型(定义了不同的攻击目标筛选策略)
    /// </summary>
    public enum AttackPickTypes 
    {
        /// <summary>
        /// 根据概率选择目标
        /// </summary>
        Odds,
        /// <summary>
        /// 按顺序选择目标
        /// </summary>
        Order,
        /// <summary>
        /// 随机选择目标
        /// </summary>
        Random
    }
}