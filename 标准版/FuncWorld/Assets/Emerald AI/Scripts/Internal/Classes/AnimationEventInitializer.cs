using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Utility
{
    /// <summary>
    /// 动画事件初始化类.
    /// 用预设动画事件数据填充动画预览编辑器(提供一系列预设动画事件,简化动画预览编辑器中的事件管理),
    /// 使开发者在 Unity 编辑器中更方便地管理和配置动画事件而不需要手动一个个地设置.
    /// 这些事件涵盖了 AI 的多种行为,开发者可更高效地配置和测试 AI 的动画和行为.
    /// </summary>
    public static class AnimationEventInitializer
    {// Populates the Animation Preview Editor with the preset Animation Event data.

        /// <summary>
        /// 获取动画事件列表
        /// </summary>
        /// <returns>返回一个包含多个预设动画事件的列表,这些动画事件涵盖了 AI 的各种行为,如攻击、装备武器、播放声音等</returns>
        public static List<EmeraldAnimationEventsClass> GetEmeraldAnimationEvents ()
        {
            //字段:预设的动画事件列表
            List<EmeraldAnimationEventsClass> EmeraldAnimationEvents = new List<EmeraldAnimationEventsClass>();

            //Custom
            // - 自定义事件:
            //   - 函数名:`---YOUR FUNCTION NAME HERE---`
            //   - 描述:一个自定义或默认事件,没有添加参数.
            AnimationEvent Custom = new AnimationEvent();
            Custom.functionName = "---YOUR FUNCTION NAME HERE---";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Custom", Custom, "A custom/default event with no added parameters"));

            //Emerald Attack Event.攻击事件
            AnimationEvent EmeraldAttack = new AnimationEvent();
            EmeraldAttack.functionName = "CreateAbility";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Create Ability", EmeraldAttack, "An event used for creating an AI's current ability (previously called EmeraldAttackEvent). This is required for Ability Objects to be triggered and should be done for all attack animations.\n\nNote: If your AI uses Attack Transform, " +
                "you should include the name of the Attack Transform in the String Paramter of this event. This will allow an ability to spawn from the Attack Transform location."));

            //Charge Ability
            // - 变更技能事件:
            //   - 函数名:`ChargeEffect`
            //   - 描述:触发 AI 当前技能的变更效果.需要在字符串参数中指定变更效果出现的位置.
            AnimationEvent ChargeEffect = new AnimationEvent();
            ChargeEffect.functionName = "ChargeEffect";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Charge Effect", ChargeEffect, "An event used for triggering an AI's current abilty's Charge Effect. You will need to add the Attack Transform you would like the charge effect to spawn at. " +
                "This is done through the String Parameter and is based off of the AI's Attack Transform list within its Combat Component. An Ability Object must have a Charge Module and have it enabled or this event will be skipped." +
                "\n\nNote: This will not create an ability. The CreateAbility event still needs to be assigned, which should be after a Charge Effect event. This Animation Event is completely optional."));

            //Fade Out IK
            // - 淡出 IK 事件:
            //   - 函数名:`FadeOutIK`
            //   - 描述:逐渐淡出 AI 的逆向运动学(IK).可以通过浮点参数设置淡出时间,通过字符串参数指定要淡出的骨骼名称.
            AnimationEvent FadeOutIK = new AnimationEvent();
            FadeOutIK.functionName = "FadeOutIK";
            FadeOutIK.floatParameter = 5f;
            FadeOutIK.stringParameter = "---YOUR RIG NAME TO FADE HERE---";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Fade Out IK", FadeOutIK, "Fade out an AI's IK overtime. This is helpful if an AI's IK is interfering with certain animations " +
                "(such as hit, equipping, certain attacks, and death animations).\n\nFloatParamer = Fade Out Time (In Seconds)\n\nStringParameter = The name of your rig you'd like to fade out"));

            //Fade In IK
            // - 淡入 IK 事件:
            //   - 函数名:`FadeInIK`
            //   - 描述:逐渐淡入 AI 的逆向运动学(IK).通常在淡出 IK 之后使用.
            AnimationEvent FadeInIK = new AnimationEvent();
            FadeInIK.functionName = "FadeInIK";
            FadeInIK.floatParameter = 5f;
            FadeInIK.stringParameter = "---YOUR RIG NAME TO FADE HERE---";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Fade In IK", FadeInIK, "Fade in an AI's IK overtime. This should be used after Fade Out IK has been used.\n\nFloatParamer = Fade In Time (In Seconds)\n\nStringParameter = The name of your rig you'd like to fade in"));

            //Enable Weapon Collider
            // - 启用武器碰撞体事件:
            //   - 函数名:`EnableWeaponCollider`
            //   - 描述:启用 AI 武器的碰撞体.需要在字符串参数中指定武器的名称.
            AnimationEvent EnableWeaponCollider = new AnimationEvent();
            EnableWeaponCollider.functionName = "EnableWeaponCollider";
            EnableWeaponCollider.stringParameter = "---THE NAME OF YOUR AI'S WEAPON HERE---";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Enable Weapon Collider", EnableWeaponCollider, "Enables an AI's weapon's collider (The weapon object must also have a WeaponCollider component and be set up through an AI's EmeraldItems component)." +
                "\n\nNote: You must also assign the gameobject name of your AI's weapon to the String parameter of this Animation Event. This is used to search through an AI's Items Component to find which weapon to enable. For a detailed tutorial on this, see the Emerald AI Wiki."));

            //Disable Weapon Collider
            // - 禁用武器碰撞体事件:
            //   - 函数名:`DisableWeaponCollider`
            //   - 描述:禁用 AI 武器的碰撞体.需要在字符串参数中指定武器的名称.
            AnimationEvent DisableWeaponCollider = new AnimationEvent();
            DisableWeaponCollider.functionName = "DisableWeaponCollider";
            DisableWeaponCollider.stringParameter = "---THE NAME OF YOUR AI'S WEAPON HERE---";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Disable Weapon Collider", DisableWeaponCollider, "Disables an AI's weapon's collider (The weapon object must also have a WeaponCollider component and be set up through an AI's EmeraldItems component)." +
                "\n\nNote: You must also assign the gameobject name of your AI's weapon to the String parameter of this Animation Event. This is used to search through an AI's Items Component to find which weapon to disable. For a detailed tutorial on this, see the Emerald AI Wiki."));

            //Equip Weapon 1
            // - 装备武器 1 事件:
            //   - 函数名:`EquipWeapon`
            //   - 描述:装备 AI 的第一种武器类型.
            AnimationEvent EquipWeapon1 = new AnimationEvent();
            EquipWeapon1.functionName = "EquipWeapon";
            EquipWeapon1.stringParameter = "Weapon Type 1";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Equip Weapon Type 1", EquipWeapon1, "Equip an AI's Type 1 Weapon (The weapon object must be setup through Emerald AI)"));

            //Equip Weapon 2
            // - 装备武器 2 事件:
            //   - 函数名:`EquipWeapon`
            //   - 描述:装备 AI 的第二种武器类型.
            AnimationEvent EquipWeapon2 = new AnimationEvent();
            EquipWeapon2.functionName = "EquipWeapon";
            EquipWeapon2.stringParameter = "Weapon Type 2";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Equip Weapon Type 2", EquipWeapon2, "Equip an AI's Type 2 Weapon (The weapon object must be setup through Emerald AI)"));

            //Unequip Weapon 1
            // - 卸下武器 1 事件:
            //   - 函数名:`UnequipWeapon`
            //   - 描述:卸下 AI 的第一种武器类型.
            AnimationEvent UnequipWeapon1 = new AnimationEvent();
            UnequipWeapon1.functionName = "UnequipWeapon";
            UnequipWeapon1.stringParameter = "Weapon Type 1";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Unequip Weapon Type 1", UnequipWeapon1, "Unquip an AI's Type 1 Weapon (The weapon object must be setup through Emerald AI)"));

            //Unequip Weapon 2
            // - 卸下武器 2 事件:
            //   - 函数名:`UnequipWeapon`
            //   - 描述:卸下 AI 的第二种武器类型.
            AnimationEvent UnequipWeapon2 = new AnimationEvent();
            UnequipWeapon2.functionName = "UnequipWeapon";
            UnequipWeapon2.stringParameter = "Weapon Type 2";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Unequip Weapon Type 2", UnequipWeapon2, "Unequip an AI's Type 2 Weapon (The weapon object must be setup through Emerald AI)"));

            //Enable Item
            // - 启用物品事件:
            //   - 函数名:`EnableItem`
            //   - 描述:通过传递物品 ID 启用 AI 的物品.
            AnimationEvent EnableItem = new AnimationEvent();
            EnableItem.functionName = "EnableItem";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Enable Item", EnableItem, "Enable an Item by passing the ItemID. This is based off of an AI's Item List and an AI must have an EmeraldAIItem component.\n\nIntParameter = ItemID"));

            //Disable Item
            // - 禁用物品事件:
            //   - 函数名:`DisableItem`
            //   - 描述:通过传递物品 ID 禁用 AI 的物品.
            AnimationEvent DisableItem = new AnimationEvent();
            DisableItem.functionName = "DisableItem";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Disable Item", DisableItem, "Disable an Item by passing the ItemID. This is based off of an AI's Item List and an AI must have an EmeraldAIItem component.\n\nIntParameter = ItemID"));

            //Footstep Sound
            // - 脚步声事件:
            //   - 函数名:`Footstep`
            //   - 描述:根据检测到的表面创建脚步效果和声音,或者播放随机的脚步声.
            AnimationEvent Footstep = new AnimationEvent();
            Footstep.functionName = "Footstep";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Footstep", Footstep, "If using a Footstep Component:\nCreates a footstep effect and sound based on the detected surface of the footstep (requires a Footstep Component that has been set up).\n\n" +
                "If NOT using a Footstep Component:\nPlay a random footstep sound based off of your AI's Walk Sound List."));

            //Play Attack Sound
            // - 播放攻击声音事件:
            //   - 函数名:`PlayAttackSound`
            //   - 描述:播放随机的攻击声音.
            AnimationEvent PlayAttackSound = new AnimationEvent();
            PlayAttackSound.functionName = "PlayAttackSound";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Play Attack Sound", PlayAttackSound, "Play a random attack sound based off of your AI's Attack Sound List."));

            //Play Sound Effect
            // - 播放音效事件:
            //   - 函数名:`PlaySoundEffect`
            //   - 描述:通过传递音效 ID 播放指定的音效.
            AnimationEvent PlaySoundEffect = new AnimationEvent();
            PlaySoundEffect.functionName = "PlaySoundEffect";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Play Sound Effect", PlaySoundEffect, "Play a specified sound from an AI's Sounds List by passing the SoundEffectID.\n\nIntParameter = SoundEffectID"));

            //Play Warning Sound
            // - 播放警告声音事件:
            //   - 函数名:`PlayWarningSound`
            //   - 描述:播放随机的警告声音.
            AnimationEvent PlayWarningSound = new AnimationEvent();
            PlayWarningSound.functionName = "PlayWarningSound";
            EmeraldAnimationEvents.Add(new EmeraldAnimationEventsClass("Play Warning Sound", PlayWarningSound, "Play a random warning sound based off of your AI's Warning Sound List."));

            //返回动画事件列表
            return EmeraldAnimationEvents;
        }
    }
}

// 一、AnimationEvent用于在动画播放过程中触发特定的事件,这些事件可以用于执行脚本中的方法、播放音效、改变物体的状态等
// 1. 定义 `AnimationEvent`
// `AnimationEvent` 类包含以下主要属性:
// - `functionName`:要调用的方法的名称(字符串).
// - `time`:事件触发的时间点(以秒为单位).
// - `objectReferenceParameter`:传递给方法的对象引用参数.
// - `intParameter`:传递给方法的整数参数.
// - `floatParameter`:传递给方法的浮点数参数.
// - `messageOptions`:事件的发送选项,如是否发送到所有监听者.

// 2. 在动画中添加 `AnimationEvent`
// ①. 打开动画剪辑:
//    - 在 Unity 编辑器中,选择一个动画剪辑(`.anim` 文件).
//    - 双击动画剪辑文件,打开动画窗口.
// ②. 添加事件:
//    - 在动画窗口的时间轴上,右键点击你希望事件触发的时间点,选择“Add Event”.
//    - 选中刚添加的事件,你会看到右侧的 Inspector 窗口中出现了 `AnimationEvent` 的属性.
// ③. 配置 `AnimationEvent`:
//    - `Function Name`:输入你希望调用的方法的名称.
//    - `Time`:设置事件触发的时间点.
//    - `Object Reference Parameter`:设置传递给方法的对象引用参数.
//    - `Int Parameter`:设置传递给方法的整数参数.
//    - `Float Parameter`:设置传递给方法的浮点数参数.

// 3. 在脚本中处理 `AnimationEvent`
// 为了响应 `AnimationEvent`,你需要在脚本中定义一个方法,并确保该方法的签名与 `AnimationEvent` 中指定的函数名称匹配.

// ```csharp 
// using UnityEngine;
// public class AnimationEventHandler : MonoBehaviour
// {
//     // 定义一个方法,名称必须与 AnimationEvent 中的 functionName 匹配
//     public void OnAttack()
//     {
//         Debug.Log("攻击事件被触发！");
//         // 在这里执行你的逻辑,例如播放音效、改变状态等
//     }

//     public void OnJump(float jumpHeight)
//     {
//         Debug.Log($"跳跃事件被触发,跳跃高度为 {jumpHeight} 米");
//         // 在这里执行跳跃逻辑
//     }

//     public void OnTakeDamage(int damageAmount, GameObject attacker)
//     {
//         Debug.Log($"受到伤害,伤害值为 {damageAmount},攻击者为 {attacker.name}");
//         // 在这里执行受伤逻辑
//     }
// }
// ```

// 4. 示例 
// 假设你有一个角色在攻击时需要播放音效,并且在跳跃时需要改变角色的位置.你可以按照以下步骤设置 `AnimationEvent` 并编写相应的脚本.
// ①. 创建动画剪辑:
//    - 创建一个名为 `CharacterAttack.anim` 的动画剪辑.
//    - 在动画剪辑中添加一个 `AnimationEvent`,设置 `Function Name` 为 `OnAttack`,`Time` 为 0.5 秒.
// ②. 编写脚本:
//    - 创建一个名为 `CharacterController.cs` 的脚本,并附着到角色对象上.

// ```csharp 
// using UnityEngine;
// public class CharacterController : MonoBehaviour
// {
//     // 定义一个方法,名称必须与 AnimationEvent 中的 functionName 匹配
//     public void OnAttack()
//     {
//         Debug.Log("攻击事件被触发！");
//         // 播放攻击音效
//         AudioSource audioSource = GetComponent();
//         if (audioSource != null)
//         {
//             audioSource.Play();
//         }
//     }

//     public void OnJump(float jumpHeight)
//     {
//         Debug.Log($"跳跃事件被触发,跳跃高度为 {jumpHeight} 米");
//         // 执行跳跃逻辑
//         Vector3 jumpVector = new Vector3(0, jumpHeight, 0);
//         transform.position += jumpVector;
//     }
// }
// ```

// ③. 配置动画剪辑:
//    - 在 `CharacterAttack.anim` 动画剪辑中,添加一个 `AnimationEvent`,设置 `Function Name` 为 `OnAttack`,`Time` 为 0.5 秒.

// ④. 测试:
//    - 运行场景,当角色执行攻击动画时,你应该会看到控制台输出“攻击事件被触发！”并且听到攻击音效.

//-----------------------------------------------------------------------------------------------------------------------------
// 二、在Unity中,Animation 组件是用于在GameObject上播放动画的,然而需要注意的是Animation 组件是基于Unity的旧动画系统,也称为“Legacy Animation System”.
// Unity后来引入了新的动画系统,即“Animator”组件和“Mecanim”动画系统,它提供了更强大和灵活的动画功能.
// 尽管如此,若你正在使用或维护一个基于旧动画系统的项目,了解Animation组件的用法仍然是有价值的.以下是一些关于Animation组件基本用法的概述:
// 添加Animation组件
// ‌选择GameObject‌:在Unity编辑器中,选择你想要添加动画的GameObject.
// ‌添加组件‌:在Inspector窗口中,点击“Add Component”按钮,然后搜索并选择“Animation”.
// 创建和配置动画剪辑
// ‌创建动画剪辑‌:在Unity的Project视图中,右键点击你想要存储动画的文件夹,然后选择“Create” > “Animation”.为新动画命名并保存.
// ‌配置动画剪辑‌:双击动画剪辑以在Animation窗口中打开它.在这里,你可以设置动画的属性,如帧数、播放速度等.
// ‌添加关键帧‌:在Animation窗口中,选择你想要动画化的属性(如位置、旋转、缩放等),然后在时间轴上添加关键帧来定义动画的变化.
// 将动画剪辑分配给Animation组件
// ‌选择动画剪辑‌:在Inspector窗口中,找到你的Animation组件,然后在“Animation”字段中拖放你的动画剪辑.
// ‌配置播放设置‌:你可以设置动画是否自动播放、循环播放、播放速度等.
// 控制动画播放

// 在脚本中,你可以通过访问Animation组件来控制动画的播放.例如:
// csharp
// Copy Code
// // 获取Animation组件
// Animation anim = GetComponent<Animation>();
// // 播放动画
// anim.Play("animationName");
// // 停止动画
// anim.Stop("animationName");
// // 检查动画是否在播放
// bool isPlaying = anim.IsPlaying("animationName");
// // 设置动画播放速度
// anim["animationName"].speed = 2.0f;
// // 淡入淡出动画
// anim.CrossFade("newAnimationName", 0.5f);

// 注意事项
// 虽然Animation组件仍然可以在Unity中使用,但建议新项目使用“Animator”组件和Mecanim动画系统,因为它们提供了更多的功能和更好的性能.
// 若你正在维护一个使用Animation组件的旧项目,并且想要迁移到新的动画系统,Unity提供了迁移工具来帮助你转换动画剪辑和动画状态机.
// Animation组件和“Animator”组件不能同时附加到同一个GameObject上.你需要选择其中一个来使用.

//-----------------------------------------------------------------------------------------------------------
// 三、Unity 的 `Animator` 组件和 Mecanim 动画系统
// - `Animator` 组件:用于控制动画的状态和过渡,与 `Animator Controller` 配合使用.
// - Mecanim 动画系统:提供了状态机、混合树、参数驱动、IK 支持等高级功能,使得动画管理和控制更加灵活和高效.

// 1. `Animator` 组件 
// `Animator` 组件是 Unity 中用于控制动画的核心组件.
// 主要功能 
// - 状态机:`Animator` 组件使用状态机来管理不同的动画状态.每个状态代表一个动画剪辑或一组动画剪辑.
// - 过渡:状态之间可以通过条件和事件进行平滑的过渡.
// - 参数:可以使用参数(如布尔值、整数、浮点数等)来控制状态机的行为.
// - 混合树:支持通过混合树将多个动画剪辑混合在一起,实现更复杂的动画效果.
// - IK:支持逆向运动学,可以在动画播放时调整角色的姿势,使其更自然地与环境互动.
// 常用属性 
// - `runtimeAnimatorController`:当前使用的 `Animator Controller`.
// - `isHuman`:布尔值,表示该动画控制器是否使用了人形模型.
// - `avatar`:当前使用的 `Avatar`,用于定义模型的骨骼结构.
// - `parameters`:当前控制器中的所有参数.
// 常用方法
// - `SetTrigger(string name)`:触发一个名为 `name` 的触发器参数.
// - `SetBool(string name, bool value)`:设置一个布尔参数的值.
// - `SetInteger(string name, int value)`:设置一个整数参数的值.
// - `SetFloat(string name, float value)`:设置一个浮点数参数的值.
// - `GetCurrentAnimatorStateInfo(int layerIndex)`:获取当前层的动画状态信息.
// - `GetNextAnimatorStateInfo(int layerIndex)`:获取下一个层的动画状态信息.

// 2. Mecanim 动画系统
// Mecanim 是 Unity 中的高级动画系统,它提供了一系列强大的工具和功能,使得动画的管理和控制更加灵活和高效.
// 主要特点
// - 状态机:Mecanim 使用状态机来管理动画状态,每个状态可以是一个单独的动画剪辑或一组动画剪辑.
// - 混合树:通过混合树,可以将多个动画剪辑混合在一起,实现平滑的过渡和复杂的动画效果.
// - 参数驱动:动画状态和过渡可以通过参数(如布尔值、整数、浮点数等)进行控制,使得动画逻辑更加灵活.
// - IK 支持:支持逆向运动学,可以在动画播放时调整角色的姿势,使其更自然地与环境互动.
// - 人形模型支持:对于人形模型,Mecanim 提供了额外的功能,如自动绑定骨骼、人形动画重定向等.
// 工作流程
// 1) 创建动画剪辑:
//    - 在 Unity 编辑器中,创建一个或多个动画剪辑(`.anim` 文件).
//    - 将这些动画剪辑拖动到项目的 `Assets` 文件夹中.
// 2) 创建 `Animator Controller`:
//    - 在 `Assets` 文件夹中,右键点击,选择 `Create > Animator Controller`.
//    - 将动画剪辑拖动到 `Animator Controller` 中,创建状态机.
// 3) 配置状态机:
//    - 在 `Animator Controller` 中,添加状态节点,每个状态节点代表一个动画剪辑.
//    - 配置状态之间的过渡条件,可以使用参数(如布尔值、整数、浮点数等)来控制过渡.
// 4) 添加 `Animator` 组件:
//    - 选择你要添加动画的 GameObject.
//    - 在 Inspector 窗口中,点击 “Add Component” 按钮,搜索并添加 `Animator` 组件.
//    - 将创建的 `Animator Controller` 拖动到 `Animator` 组件的 `Controller` 属性中.
// 5) 控制动画状态:
//    - 通过脚本控制 `Animator` 组件的状态,使用 `SetTrigger`、`SetBool`、`SetInteger`、`SetFloat` 等方法.

// 示例代码
// 假设你有一个角色对象,需要在脚本中控制其动画状态.以下是一个简单的示例:
// ```csharp
// using UnityEngine;
// public class PlayerController : MonoBehaviour
// {
//     private Animator animator;
//     void Start()
//     {
//         // 获取 Animator 组件 
//         animator = GetComponent();
//     }
//     void Update()
//     {
//         // 检测按键输入来控制动画
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             Jump();
//         }
//         if (Input.GetAxis("Vertical") > 0)
//         {
//             MoveForward();
//         }
//         else 
//         {
//             Idle();
//         }
//     }
//     void Jump()
//     {
//         // 触发 "Jump" 动画
//         animator.SetTrigger("Jump");
//     }
//     void MoveForward()
//     {
//         // 设置 "Speed" 参数,表示角色在移动 
//         animator.SetFloat("Speed", 1.0f);
//     }
//     void Idle()
//     {
//         // 设置 "Speed" 参数,表示角色处于静止状态
//         animator.SetFloat("Speed", 0.0f);
//     }
// }
// ```
