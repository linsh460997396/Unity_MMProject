namespace MetalMaxSystem
{
    //枚举是值类型

    /// <summary>
    /// 主副循环的入口枚举
    /// </summary>
    public enum Entry
    {
        /// <summary>
        /// 主循环唤醒阶段
        /// </summary>
        MainAwake,
        /// <summary>
        /// 主循环开始阶段
        /// </summary>
        MainStart,
        /// <summary>
        /// 主循环周期循环阶段
        /// </summary>
        MainUpdate,
        /// <summary>
        /// 主循环结束阶段
        /// </summary>
        MainEnd,
        /// <summary>
        /// 主循环摧毁阶段
        /// </summary>
        MainDestroy,
        /// <summary>
        /// 副循环唤醒阶段
        /// </summary>
        SubAwake,
        /// <summary>
        /// 副循环开始阶段
        /// </summary>
        SubStart,
        /// <summary>
        /// 副循环周期循环阶段
        /// </summary>
        SubUpdate,
        /// <summary>
        /// 副循环结束阶段
        /// </summary>
        SubEnd,
        /// <summary>
        /// 副循环摧毁阶段
        /// </summary>
        SubDestroy
    }
}
