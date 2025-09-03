using UnityEngine;

public class FPSInputController : MonoBehaviour
{
    private CharacterMotor motor;

    private void Awake()
    {
        // 获取CharacterMotor组件 
        motor = GetComponent<CharacterMotor>();
    }

    private void Update()
    {
        // 从键盘或模拟摇杆获取输入矢量 
        Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (directionVector != Vector3.zero)
        {
            // 计算输入向量的长度 
            float directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;

            // 确保长度不大于1 
            directionLength = Mathf.Min(1, directionLength);

            // 使输入向量对极值更敏感,对中间更不敏感 
            directionLength = directionLength * directionLength;

            // 得到最终的输入向量 
            directionVector = directionVector * directionLength;
        }

        // 将方向应用到CharacterMotor 
        motor.inputMoveDirection = transform.rotation * directionVector;
        motor.inputJump = Input.GetKey(KeyCode.Space);
    }
}