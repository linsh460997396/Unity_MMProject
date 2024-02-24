namespace MetalMaxSystem
{
    //枚举是值类型

    /// <summary>
    /// 【MM_函数库】主副循环的入口枚举
    /// </summary>
    public enum Entry
    {
        /// <summary>
        /// 【MM_函数库】主循环唤醒阶段
        /// </summary>
        MainAwake,
        /// <summary>
        /// 【MM_函数库】主循环开始阶段
        /// </summary>
        MainStart,
        /// <summary>
        /// 【MM_函数库】主循环周期循环阶段
        /// </summary>
        MainUpdate,
        /// <summary>
        /// 【MM_函数库】主循环结束阶段
        /// </summary>
        MainEnd,
        /// <summary>
        /// 【MM_函数库】主循环摧毁阶段
        /// </summary>
        MainDestroy,
        /// <summary>
        /// 【MM_函数库】副循环唤醒阶段
        /// </summary>
        SubAwake,
        /// <summary>
        /// 【MM_函数库】副循环开始阶段
        /// </summary>
        SubStart,
        /// <summary>
        /// 【MM_函数库】副循环周期循环阶段
        /// </summary>
        SubUpdate,
        /// <summary>
        /// 【MM_函数库】副循环结束阶段
        /// </summary>
        SubEnd,
        /// <summary>
        /// 【MM_函数库】副循环摧毁阶段
        /// </summary>
        SubDestroy
    }
}
