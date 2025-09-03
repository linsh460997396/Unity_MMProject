namespace EmeraldAI
{
    /// <summary>
    /// 条件类型
    /// </summary>
    public enum ConditionTypes 
    {
        /// <summary>
        /// 自身健康值低
        /// </summary>
        SelfLowHealth,
        /// <summary>
        /// 盟友健康值低
        /// </summary>
        AllyLowHealth,
        /// <summary>
        /// 与目标的距离
        /// </summary>
        DistanceFromTarget,
        /// <summary>
        /// 当前没有召唤物
        /// </summary>
        NoCurrentSummons,
    }
}