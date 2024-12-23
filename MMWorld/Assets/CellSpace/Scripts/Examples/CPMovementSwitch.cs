using UnityEngine;

namespace CellSpace.Examples
{
    public class CPMovementSwitch : MonoBehaviour
    {
        public CPCharacterMotor motor;
        private bool speedOn;

        void Update()
        {
            //玩家按R切换跑步
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (speedOn)
                {
                    // 设置慢速移动参数
                    motor.movement.maxForwardSpeed = 4f;
                    motor.movement.maxBackwardsSpeed = 4f;
                    motor.movement.maxSidewaysSpeed = 4f;
                    motor.jumping.baseHeight = 0.5f;
                    speedOn = false;
                }
                else
                {
                    // 设置快速移动参数
                    motor.movement.maxForwardSpeed = 15f;
                    motor.movement.maxBackwardsSpeed = 15f;
                    motor.movement.maxSidewaysSpeed = 15f;
                    motor.jumping.baseHeight = 1.5f;
                    speedOn = true;
                }
            }
        }
    }
}

////↓该编译器参数启用严格模式，强制执行某些编译错误，例如未使用的变量、空的脚本方法等。这个指令可以帮助开发者发现并修复代码中的潜在问题
//#pragma strict

//// switches between fast and slow movement when "spriteRenderer" is pressed

//public var motor : CPCharacterMotor;
//private var speedOn : boolean;

//function Update () {
//	if (Input.GetKeyDown ("spriteRenderer")) {
//		if (speedOn) {
//			motor.movement.maxForwardSpeed = 4;
//			motor.movement.maxBackwardsSpeed = 4;
//			motor.movement.maxSidewaysSpeed = 4;
//			motor.jumping.baseHeight = 0.5;

//			speedOn = false;
//		}
//		else {
//			motor.movement.maxForwardSpeed = 15;
//			motor.movement.maxBackwardsSpeed = 15;
//			motor.movement.maxSidewaysSpeed = 15;
//			motor.jumping.baseHeight = 1.5;

//			speedOn = true;
//		}

//	}
//}