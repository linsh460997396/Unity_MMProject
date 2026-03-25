namespace EmeraldAI
{
    /// <summary>
    /// 选择目标类型(定义了不同的目标选择策略)
    /// </summary>
    public enum PickTargetTypes
    {
        /// <summary>
        /// 选择最近的目标
        /// </summary>
        Closest = 0,
        /// <summary>
        /// 选择最先检测到的目标
        /// </summary>
        FirstDetected = 1,
        /// <summary>
        /// 随机选择一个目标
        /// </summary>
        Random = 2
    }
}