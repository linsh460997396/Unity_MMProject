namespace EmeraldAI
{
    /// <summary>
    /// 动画状态类型.表示角色动画的各种状态
    /// </summary>
    [System.Flags]
    public enum AnimationStateTypes
    {
        /// <summary>
        /// 无任何动画状态
        /// </summary>
        None = 0,
        /// <summary>
        /// 空闲状态
        /// </summary>
        Idling = 1 << 1,
        /// <summary>
        /// 移动状态
        /// </summary>
        Moving = 1 << 2,
        /// <summary>
        /// 后退状态
        /// </summary>
        BackingUp = 1 << 3,
        /// <summary>
        /// 左转状态
        /// </summary>
        TurningLeft = 1 << 4,
        /// <summary>
        /// 右转状态
        /// </summary>
        TurningRight = 1 << 5,
        /// <summary>
        /// 攻击状态
        /// </summary>
        Attacking = 1 << 6,
        /// <summary>
        /// 侧移状态
        /// </summary>
        Strafing = 1 << 7,
        /// <summary>
        /// 格挡状态
        /// </summary>
        Blocking = 1 << 8,
        /// <summary>
        /// 闪避状态
        /// </summary>
        Dodging = 1 << 9,
        /// <summary>
        /// 反冲状态(后退、后坐力)
        /// </summary>
        Recoiling = 1 << 10,
        /// <summary>
        /// 昏迷状态
        /// </summary>
        Stunned = 1 << 11,
        /// <summary>
        /// 受击状态
        /// </summary>
        GettingHit = 1 << 12,
        /// <summary>
        /// 装备状态
        /// </summary>
        Equipping = 1 << 13,
        /// <summary>
        /// 切换武器状态
        /// </summary>
        SwitchingWeapons = 1 << 14,
        /// <summary>
        /// 死亡状态
        /// </summary>
        Dead = 1 << 15,
        /// <summary>
        /// 表情(执行中)状态
        /// </summary>
        Emoting = 1 << 16,
        /// <summary>
        /// 任意状态
        /// </summary>
        Everything = ~0,
    }
}