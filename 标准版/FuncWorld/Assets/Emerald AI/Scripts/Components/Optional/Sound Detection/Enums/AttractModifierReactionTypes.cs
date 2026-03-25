namespace EmeraldAI.SoundDetection
{
    /// <summary>
    /// 吸引修改器反应类型.定义了角色对吸引源(Attract Source)可能的反应类型.
    /// 每个枚举值都有一个对应的整数值,表示不同的行为模式.
    /// </summary>
    public enum AttractModifierReactionTypes
    {
        /// <summary>
        /// 角色会看向吸引源的方向.这种反应通常用于表示角色注意到了某个吸引源,但不会移动位置.
        /// </summary>
        LookAtAttractSource = 0,
        /// <summary>
        /// 角色会在吸引源周围移动.这种反应适用于角色需要围绕吸引源进行巡逻或探索的情况.
        /// </summary>
        MoveAroundAttractSource = 25,
        /// <summary>
        /// 角色会直接移动到吸引源的位置.这种反应适用于角色需要快速响应并到达吸引源的情况.
        /// </summary>
        MoveToAttractSource = 50,
    }
}