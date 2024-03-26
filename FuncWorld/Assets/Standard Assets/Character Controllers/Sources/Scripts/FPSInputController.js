//这里是一个JavaScript

private var motor: CharacterMotor;

// Use this for initialization.使用本脚本来完成初始化
function Awake() {
	//motor组件通常用于在脚本中控制角色的移动和其他动作
	motor = GetComponent(CharacterMotor);
}

// Update is called once per frame
function Update () {
	// Get the input vector from kayboard or analog stick.从键盘或模拟摇杆获得输入矢量
	var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
	
	if (directionVector != Vector3.zero) {
		// Get the length of the directon vector and then normalize it.得到方向向量的长度，然后将其归一化
		// Dividing by the length is cheaper than normalizing when we already have the length anyway.当我们已经有长度时，除以长度比归一化更便宜
		var directionLength = directionVector.magnitude;
		directionVector = directionVector / directionLength;
		
		// Make sure the length is no bigger than 1.确保长度不大于1
		directionLength = Mathf.Min(1, directionLength);
		
		// Make the input vector more sensitive towards the extremes and less sensitive in the middle.使输入向量对极值更敏感，而对中间更不敏感
		// This makes it easier to control slow speeds when using analog sticks.这使得使用模拟摇杆时更容易控制慢速
		directionLength = directionLength * directionLength;
		
		// Multiply the normalized direction vector by the modified length.将归一化方向向量乘以修改后的长度
		directionVector = directionVector * directionLength;
	}
	
	// Apply the direction to the CharacterMotor.将方向应用到CharacterMotor
	motor.inputMoveDirection = transform.rotation * directionVector;
	motor.inputJump = Input.GetButton("Jump");
}

// 更新 (Update 函数):

// 输入处理:
// 使用 Input.GetAxis("Horizontal") 和 Input.GetAxis("Vertical") 从键盘或模拟摇杆获取输入向量。这些值表示玩家在水平和垂直方向上的输入（例如，左右移动和前后移动）。
// 如果输入向量不为零（即玩家正在移动），则执行一系列操作来调整和标准化输入向量，使其更适合游戏控制。

// 向量调整:
// 计算输入向量的长度（directionLength）。
// 标准化输入向量，使其长度为1。
// 确保向量的长度不超过1。
// 通过将长度平方，使向量在极端值上更敏感，而在中间值上不太敏感。这有助于在使用模拟摇杆时更容易控制慢速移动。
// 将调整后的长度与标准化的方向向量相乘，得到最终的输入向量。

// 应用输入:
// 将调整后的输入向量（directionVector）与角色的旋转（transform.rotation）相结合，得到最终的移动方向。
// 将这个移动方向赋值给 motor.inputMoveDirection，以告诉 CharacterMotor 组件角色应该如何移动。
// 使用 Input.GetButton("Jump") 检查玩家是否按下了跳跃按钮，并将结果赋值给 motor.inputJump，以告诉 CharacterMotor 组件是否应该让角色跳跃。


// Require a character controller to be attached to the same game object.要求角色控制器连接到相同的游戏对象

//注解确保此脚本附加的游戏对象上有一个 CharacterMotor 组件，如没有，Unity将自动添加一个
@script RequireComponent(CharacterMotor)
//注解将此脚本添加到Unity的组件菜单中，使其更容易通过编辑器添加到游戏对象上
@script AddComponentMenu ("Character/FPS Input Controller")
