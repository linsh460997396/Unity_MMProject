using UnityEngine;

public class MovementSwitch : MonoBehaviour
{
    public CharacterMotor motor;
    private bool speedOn;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            if (speedOn) {
                // 设置慢速移动参数
                motor.movement.maxForwardSpeed = 4f;
                motor.movement.maxBackwardsSpeed = 4f;
                motor.movement.maxSidewaysSpeed = 4f;
                motor.jumping.baseHeight = 0.5f;
                speedOn = false;
            }
            else {
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

////启用严格模式，强制执行某些编译错误，例如未使用的变量、空的脚本方法等。这个指令可以帮助开发者发现并修复代码中的潜在问题
//#pragma strict

//// switches between fast and slow movement when "r" is pressed

//public var motor : CharacterMotor;
//private var speedOn : boolean;

//function Update () {
//	if (Input.GetKeyDown ("r")) {
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