namespace EmeraldAI.SoundDetection
{
    /// <summary>
    /// 威胁级别.定义了 AI 或角色在游戏中的威胁感知级别,这些级别用于描述 AI 对周围环境或特定目标的警觉程度.
    /// </summary>
    public enum ThreatLevels
    {
        /// <summary>
        /// 无意识.表示 AI 完全没有意识到周围环境中存在潜在的威胁或目标.
        /// </summary>
        Unaware,
        /// <summary>
        /// 怀疑.表示 AI 感觉到周围环境中可能存在威胁,但还没有确凿的证据.
        /// </summary>
        Suspicious,
        /// <summary>
        /// 意识到.表示 AI 已经确认周围环境中存在威胁或目标.
        /// </summary>
        Aware,
    }
}

// 枚举 `ThreatLevels` 定义了 AI 或角色在游戏中的威胁感知级别.
// 这些级别用于描述 AI 对周围环境或特定目标的警觉程度.

// 枚举值及其含义

// 1. Unaware
//    - 含义: 无意识.表示 AI 完全没有意识到周围环境中存在潜在的威胁或目标.
//    - 用途: 
//      - 初始状态:AI 刚开始时通常处于这个状态.
//      - 安全状态:当 AI 没有检测到任何异常或威胁时,保持在这个状态.
//      - 用于触发 AI 的常规巡逻或探索行为.

// 2. Suspicious
//    - 含义: 怀疑.表示 AI 感觉到周围环境中可能存在威胁,但还没有确凿的证据.
//    - 用途:
//      - 检测到可疑声音或视觉线索:AI 会变得更加警觉,可能会停下来观察或调查.
//      - 用于触发 AI 的调查行为,如四处查看、靠近声源等.
//      - 可以结合 `ReactionTypes` 中的 `LookAtLoudestTarget` 或 `MoveAroundLoudestTarget` 反应类型,使 AI 更仔细地调查潜在威胁.

// 3. Aware
//    - 含义: 意识到.表示 AI 已经确认周围环境中存在威胁或目标.
//    - 用途:
//      - 检测到明确的威胁:AI 会立即采取行动,如进入战斗状态、攻击目标等.
//      - 用于触发 AI 的战斗行为,如 `EnterCombatState`、`MoveToLoudestTarget` 或 `SetLoudestTargetAsCombatTarget`.
//      - 可以结合 `IDamageable` 接口,使 AI 对目标造成伤害或进行防御.

// 应用场景 

// 1. AI 行为树:
//    - 在 AI 行为树中,`ThreatLevels` 可以用于决定 AI 的行为路径.例如:
//      - 当 `ThreatLevel` 为 `Unaware` 时,AI 可能会执行巡逻或探索行为.
//      - 当 `ThreatLevel` 为 `Suspicious` 时,AI 可能会执行调查行为.
//      - 当 `ThreatLevel` 为 `Aware` 时,AI 会进入战斗状态并执行攻击行为.

// 2. 状态机:
//    - 在有限状态机(FSM)中,`ThreatLevels` 可以用于定义状态之间的转换规则.例如:
//      - 从 `Unaware` 状态转换到 `Suspicious` 状态:当 AI 检测到可疑线索时.
//      - 从 `Suspicious` 状态转换到 `Aware` 状态:当 AI 确认威胁存在时.
//      - 从 `Aware` 状态转换到 `Unaware` 状态:当威胁消失或被消灭时.

// 3. 事件驱动系统:
//    - 当 AI 检测到特定事件(如听到声音、看到敌人)时,可以根据 `ThreatLevels` 来决定如何响应.例如:
//      - 听到轻微的声音:将 `ThreatLevel` 设为 `Suspicious`,并进行调查.
//      - 看到敌人:将 `ThreatLevel` 设为 `Aware`,并进入战斗状态.

// 4. 调试和测试:
//    - 开发者可以使用 `ThreatLevels` 来记录 AI 的警觉状态,帮助诊断和优化 AI 系统.例如:
//      - 记录 AI 在不同时间段的 `ThreatLevel` 变化,分析其行为是否合理.
//      - 使用 `DebugLogMessage` 反应类型记录 `ThreatLevel` 的变化,便于调试.

// 示例代码 

// 以下是一个简单的示例,展示如何在 AI 控制器中使用 `ThreatLevels`:

// ```csharp
// using UnityEngine;

// public class AIController : MonoBehaviour
// {
//     private ThreatLevels currentThreatLevel = ThreatLevels.Unaware;

//     void Update()
//     {
//         switch (currentThreatLevel)
//         {
//             case ThreatLevels.Unaware:
//                 Patrol();
//                 break;
//             case ThreatLevels.Suspicious:
//                 Investigate();
//                 break;
//             case ThreatLevels.Aware:
//                 EnterCombat();
//                 break;
//         }
//     }

//     private void Patrol()
//     {
//         // AI 巡逻行为
//     }

//     private void Investigate()
//     {
//         // AI 调查行为
//     }

//     private void EnterCombat()
//     {
//         // AI 进入战斗状态
//     }

//     public void OnDetectSound(Vector3 soundPosition)
//     {
//         if (Vector3.Distance(transform.position, soundPosition) < 10f)
//         {
//             currentThreatLevel = ThreatLevels.Suspicious;
//         }
//     }

//     public void OnSeeEnemy(GameObject enemy)
//     {
//         currentThreatLevel = ThreatLevels.Aware;
//     }
// }
// ```

// 通过这种方式,`ThreatLevels` 可以帮助开发者更精细地控制 AI 的行为,使其在不同情境下做出合理的反应.