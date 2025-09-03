namespace EmeraldAI.SoundDetection
{
    /// <summary>
    /// 触发类型
    /// </summary>
    public enum TriggerTypes
    {
        /// <summary>
        /// 在开始时触发.表示在游戏对象初始化或场景加载时触发.
        /// </summary>
        OnStart = 0,
        /// <summary>
        /// 在触发器事件发生时触发.表示在游戏对象进入或离开触发器区域时触发.
        /// </summary>
        OnTrigger = 5,
        /// <summary>
        /// 在碰撞事件发生时触发.表示在游戏对象与其他对象发生物理碰撞时触发.
        /// </summary>
        OnCollision = 10,
        /// <summary>
        /// 在自定义调用时触发.表示在特定的自定义事件或方法调用时触发.
        /// </summary>
        OnCustomCall = 15,
    }
}

// 枚举 `TriggerTypes` 定义了不同类型的触发器,这些触发器用于在游戏开发中控制特定事件或行为的触发时机.
// 每个枚举值都有一个特定的整数值,这可能用于排序或优先级处理.
// 以下是对每个枚举值的详细解释及其应用场景:

// 枚举值及其含义

// 1. OnStart (0)
//    - 含义: 在开始时触发.表示在游戏对象初始化或场景加载时触发.
//    - 用途:
//      - 初始化设置:用于设置初始状态或配置.
//      - 加载资源:在游戏对象创建时加载必要的资源.
//      - 触发初始事件:启动一些初始的行为或动画.

// 2. OnTrigger (5)
//    - 含义: 在触发器事件发生时触发.表示在游戏对象进入或离开触发器区域时触发.
//    - 用途:
//      - 交互行为:用于处理与其他对象的交互,如开门、拾取物品等.
//      - 区域检测:用于检测玩家或其他对象进入特定区域,触发特定事件.
//      - 动画播放:在进入特定区域时播放动画或音效.

// 3. OnCollision (10)
//    - 含义: 在碰撞事件发生时触发.表示在游戏对象与其他对象发生物理碰撞时触发.
//    - 用途:
//      - 损伤处理:用于处理碰撞造成的伤害,如玩家碰撞敌人时减少生命值.
//      - 物理交互:处理物理交互,如反弹、推动等.
//      - 触发特殊效果:在碰撞时播放特效或音效.

// 4. OnCustomCall (15)
//    - 含义: 在自定义调用时触发.表示在特定的自定义事件或方法调用时触发.
//    - 用途:
//      - 自定义行为:用于处理特定的自定义逻辑,如特定条件满足时触发的事件.
//      - 动态事件:在运行时动态触发的事件,如定时器到期、网络事件等.
//      - 脚本调用:通过脚本调用特定的方法或函数,触发相应的行为.

// 应用场景 

// 1. 初始化和加载:
//    - 使用 `OnStart` 触发器来初始化游戏对象的状态或加载必要的资源.例如,在游戏开始时加载玩家的初始装备或设置初始位置.

// 2. 交互和区域检测:
//    - 使用 `OnTrigger` 触发器来处理与其他对象的交互或检测玩家进入特定区域.例如,当玩家进入一个宝箱的触发器区域时,打开宝箱并显示内容.

// 3. 物理交互和碰撞处理:
//    - 使用 `OnCollision` 触发器来处理物理碰撞事件.例如,当玩家碰撞敌人时,减少玩家的生命值并播放受伤动画.

// 4. 自定义事件和动态行为:
//    - 使用 `OnCustomCall` 触发器来处理特定的自定义事件或动态行为.例如,当玩家完成一个任务时,通过自定义调用触发奖励发放.

// 示例代码

// 以下是一个简单的示例,展示如何在 Unity 中使用 `TriggerTypes` 枚举来处理不同的触发事件:

// ```csharp
// using UnityEngine;

// public class TriggerHandler : MonoBehaviour
// {
//     public TriggerTypes triggerType;

//     void Start()
//     {
//         if (triggerType == TriggerTypes.OnStart)
//         {
//             OnStartTrigger();
//         }
//     }

//     void OnTriggerEnter(Collider other)
//     {
//         if (triggerType == TriggerTypes.OnTrigger)
//         {
//             OnTriggerTrigger(other);
//         }
//     }

//     void OnCollisionEnter(Collision collision)
//     {
//         if (triggerType == TriggerTypes.OnCollision)
//         {
//             OnCollisionTrigger(collision);
//         }
//     }

//     public void CustomCall()
//     {
//         if (triggerType == TriggerTypes.OnCustomCall)
//         {
//             OnCustomCallTrigger();
//         }
//     }

//     private void OnStartTrigger()
//     {
//         Debug.Log("OnStart Trigger: Initialization or loading resources.");
//     }

//     private void OnTriggerTrigger(Collider other)
//     {
//         Debug.Log($"OnTrigger Trigger: Entered trigger with {other.name}.");
//     }

//     private void OnCollisionTrigger(Collision collision)
//     {
//         Debug.Log($"OnCollision Trigger: Collided with {collision.gameObject.name}.");
//     }

//     private void OnCustomCallTrigger()
//     {
//         Debug.Log("OnCustomCall Trigger: Custom event or method call.");
//     }
// }
// ```

// 代码说明

// 1. Start 方法:
//    - 若 `triggerType` 是 `OnStart`,则调用 `OnStartTrigger` 方法进行初始化或加载资源.

// 2. OnTriggerEnter 方法:
//    - 若 `triggerType` 是 `OnTrigger`,则调用 `OnTriggerTrigger` 方法处理触发器事件.

// 3. OnCollisionEnter 方法:
//    - 若 `triggerType` 是 `OnCollision`,则调用 `OnCollisionTrigger` 方法处理碰撞事件.

// 4. CustomCall 方法:
//    - 若 `triggerType` 是 `OnCustomCall`,则调用 `OnCustomCallTrigger` 方法处理自定义事件.

// 通过这种方式,`TriggerTypes` 枚举可以帮助开发者更灵活地控制游戏对象在不同事件下的行为,提升游戏的互动性和可玩性.