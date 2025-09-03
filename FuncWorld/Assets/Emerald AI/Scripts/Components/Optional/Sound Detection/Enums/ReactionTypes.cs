namespace EmeraldAI.SoundDetection
{
    /// <summary>
    /// 反应类型
    /// </summary>
    public enum ReactionTypes
    {
        /// <summary>
        /// 无反应.表示 AI 不执行任何特定的行动.
        /// </summary>
        None = 0,
        /// <summary>
        /// 吸引修改器.可能用于增加 AI 对某个目标的兴趣或吸引力.
        /// </summary>
        AttractModifier = 25,
        /// <summary>
        /// 调试日志消息.用于记录调试信息.
        /// </summary>
        DebugLogMessage = 50,
        /// <summary>
        /// 延迟.使 AI 暂停一段时间.
        /// </summary>
        Delay = 75,
        /// <summary>
        /// 进入战斗状态.使 AI 进入战斗模式.
        /// </summary>
        EnterCombatState = 100,
        /// <summary>
        /// 退出战斗状态.使 AI 退出战斗模式.
        /// </summary>
        ExitCombatState = 125,
        /// <summary>
        /// 扩大检测距离.增加 AI 的感知范围.
        /// </summary>
        ExpandDetectionDistance = 150,
        /// <summary>
        /// 逃离最响亮的目标.使 AI 逃离声音最大的目标.
        /// </summary>
        FleeFromLoudestTarget = 162,
        /// <summary>
        /// 注视最响亮的目标.使 AI 注视声音最大的目标.
        /// </summary>
        LookAtLoudestTarget = 175,
        /// <summary>
        /// 绕当前位置移动.使 AI 在当前位置附近移动.
        /// </summary>
        MoveAroundCurrentPosition = 200,
        /// <summary>
        /// 绕最响亮的目标移动.使 AI 在最响亮的目标附近移动.
        /// </summary>
        MoveAroundLoudestTarget = 225,
        /// <summary>
        /// 移动到最响亮的目标.使 AI 直接移动到声音最大的目标位置.
        /// </summary>
        MoveToLoudestTarget = 250,
        /// <summary>
        /// 将最响亮的目标设为战斗目标.使 AI 将声音最大的目标设为攻击对象.
        /// </summary>
        SetLoudestTargetAsCombatTarget = 260,
        /// <summary>
        /// 播放表情动画.使 AI 播放特定的表情动画.
        /// </summary>
        PlayEmoteAnimation = 275,
        /// <summary>
        /// 播放声音.使 AI 播放特定的声音效果.
        /// </summary>
        PlaySound = 300,
        /// <summary>
        /// 重置所有设置为默认值.使 AI 的所有状态和参数恢复到初始值.
        /// </summary>
        ResetAllToDefault = 325,
        /// <summary>
        /// 重置检测距离.恢复 AI 的初始感知范围.
        /// </summary>
        ResetDetectionDistance = 350,
        /// <summary>
        /// 重置注视位置.恢复 AI 的初始注视位置.
        /// </summary>
        ResetLookAtPosition = 375,
        /// <summary>
        /// 返回起始位置.使 AI 返回到初始位置.
        /// </summary>
        ReturnToStartingPosition = 400,
        /// <summary>
        /// 设置移动状态.改变 AI 的移动状态(如行走、跑步、静止等).
        /// </summary>
        SetMovementState = 425,
    }
}

// 枚举 `ReactionTypes` 定义了一系列反应类型,这些反应类型通常用于 AI 系统中,特别是在处理 AI 的行为和决策时.
// 每个枚举值都有一个特定的整数值,这可能用于排序或优先级处理.

// 枚举值及其含义 

// 1. None (0)
//    - 含义: 无反应.表示 AI 不执行任何特定的行动.
//    - 用途: 用于初始化或重置 AI 的反应状态.

// 2. AttractModifier (25)
//    - 含义: 吸引修饰符.可能用于增加 AI 对某个目标的兴趣或吸引力.
//    - 用途: 使 AI 更容易注意到某个目标或位置.

// 3. DebugLogMessage (50)
//    - 含义: 调试日志消息.用于记录调试信息.
//    - 用途: 帮助开发者调试 AI 行为,记录关键事件或状态变化.

// 4. Delay (75)
//    - 含义: 延迟.使 AI 暂停一段时间.
//    - 用途: 用于控制 AI 行动的时间间隔,模拟思考或反应时间.

// 5. EnterCombatState (100)
//    - 含义: 进入战斗状态.使 AI 进入战斗模式.
//    - 用途: 当检测到敌人或威胁时,AI 会切换到战斗状态,准备攻击或防御.

// 6. ExitCombatState (125)
//    - 含义: 退出战斗状态.使 AI 退出战斗模式.
//    - 用途: 当威胁解除或战斗结束时,AI 会恢复到正常状态.

// 7. ExpandDetectionDistance (150)
//    - 含义: 扩大检测距离.增加 AI 的感知范围.
//    - 用途: 使 AI 能够更早地发现远处的目标或威胁.

// 8. FleeFromLoudestTarget (162)
//    - 含义: 逃离最响亮的目标.使 AI 逃离声音最大的目标.
//    - 用途: 用于模拟 AI 的恐惧反应,使其在听到大声响时逃跑.

// 9. LookAtLoudestTarget (175)
//    - 含义: 注视最响亮的目标.使 AI 注视声音最大的目标.
//    - 用途: 用于模拟 AI 的注意力集中,使其在听到声音时转向声源.

// 10. MoveAroundCurrentPosition (200)
//     - 含义: 绕当前位置移动.使 AI 在当前位置附近移动.
//     - 用途: 用于模拟 AI 的探索或巡逻行为,使其在一定范围内活动.

// 11. MoveAroundLoudestTarget (225)
//     - 含义: 绕最响亮的目标移动.使 AI 在最响亮的目标附近移动.
//     - 用途: 用于模拟 AI 的搜索行为,使其在声源附近寻找目标.

// 12. MoveToLoudestTarget (250)
//     - 含义: 移动到最响亮的目标.使 AI 直接移动到声音最大的目标位置.
//     - 用途: 用于模拟 AI 的快速响应,使其迅速接近声源.

// 13. SetLoudestTargetAsCombatTarget (260)
//     - 含义: 将最响亮的目标设为战斗目标.使 AI 将声音最大的目标设为攻击对象.
//     - 用途: 用于确定 AI 的主要攻击目标,通常是最近或最明显的威胁.

// 14. PlayEmoteAnimation (275)
//     - 含义: 播放表情动画.使 AI 播放特定的表情动画.
//     - 用途: 用于增强 AI 的表现力,使其在特定情况下表现出情感或意图.

// 15. PlaySound (300)
//     - 含义: 播放声音.使 AI 播放特定的声音效果.
//     - 用途: 用于模拟 AI 的声音反馈,增强游戏的真实感和沉浸感.

// 16. ResetAllToDefault (325)
//     - 含义: 重置所有设置为默认值.使 AI 的所有状态和参数恢复到初始值.
//     - 用途: 用于初始化或重置 AI 的状态,确保其在新任务或场景中恢复正常行为.

// 17. ResetDetectionDistance (350)
//     - 含义: 重置检测距离.恢复 AI 的初始感知范围.
//     - 用途: 用于取消之前对检测距离的修改,使 AI 的感知能力回到默认状态.

// 18. ResetLookAtPosition (375)
//     - 含义: 重置注视位置.恢复 AI 的初始注视位置.
//     - 用途: 用于取消之前对注视位置的修改,使 AI 的视线回到默认方向.

// 19. ReturnToStartingPosition (400)
//     - 含义: 返回起始位置.使 AI 返回到初始位置.
//     - 用途: 用于重置 AI 的位置,使其回到任务开始时的位置.

// 20. SetMovementState (425)
//     - 含义: 设置移动状态.改变 AI 的移动状态(如行走、跑步、静止等).
//     - 用途: 用于控制 AI 的移动方式,根据不同的情况调整其移动速度和方式.

// 应用场景

// 1. AI 行为树:
//    - 在 AI 行为树中,这些反应类型可以用作节点,根据不同的条件触发不同的行为.

// 2. 事件驱动系统:
//    - 当 AI 检测到特定事件(如听到声音、看到敌人)时,可以根据预设的反应类型执行相应的动作.

// 3. 状态机:
//    - 在有限状态机(FSM)中,这些反应类型可以用于定义状态之间的转换规则,使 AI 在不同状态下执行不同的行为.

// 4. 调试和测试:
//    - 开发者可以使用 `DebugLogMessage` 和其他调试相关的反应类型来记录 AI 的行为,帮助诊断和优化 AI 系统.

// 通过这些反应类型,开发者可以灵活地设计和控制 AI 的行为,使其更加智能化和多样化.