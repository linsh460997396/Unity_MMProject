using UnityEngine;

namespace CellSpace.Examples
{
    // 角色移动相关的结构体，包含各种移动属性 
    public struct CPCharacterMotorMovement
    {
        // 最大水平速度（向前、向侧、向后） 
        public float maxForwardSpeed;
        public float maxSidewaysSpeed;
        public float maxBackwardsSpeed;
        // 基于坡度的速度曲线 
        public AnimationCurve slopeSpeedMultiplier;
        // 地面和空中的最大加速度 
        public float maxGroundAcceleration;
        public float maxAirAcceleration;
        // 重力和最大下落速度 
        public float gravity;
        public float maxFallSpeed;
        // 碰撞标志、速度相关变量、碰撞点相关变量 
        public CollisionFlags collisionFlags;
        public Vector3 velocity;
        public Vector3 frameVelocity;
        public Vector3 hitPoint;
        public Vector3 lastHitPoint;
    }

    // 跳跃相关的结构体，包含跳跃属性 
    public struct CPCharacterMotorJumping
    {
        // 能否跳跃、基础跳跃高度、额外跳跃高度 
        public bool enabled;
        public float baseHeight;
        public float extraHeight;
        // 在可行走表面和陡峭表面垂直跳跃的程度 
        public float perpAmount;
        public float steepPerpAmount;
        // 跳跃相关的状态变量 
        public bool jumping;
        public bool holdingJumpButton;
        public float lastStartTime;
        public float lastButtonDownTime;
        public Vector3 jumpDir;
    }

    // 移动平台相关的结构体，包含移动平台交互的属性 
    public struct CPCharacterMotorMovingPlatform
    {
        public bool enabled;
        public CPMovementTransferOnJump movementTransfer;
        public Transform hitPlatform;
        public Transform activePlatform;
        public Vector3 activeLocalPoint;
        public Vector3 activeGlobalPoint;
        public Quaternion activeLocalRotation;
        public Quaternion activeGlobalRotation;
        public Matrix4x4 lastMatrix;
        public Vector3 platformVelocity;
        public bool newPlatform;
    }

    // 滑动相关的结构体，包含滑动属性 
    public struct CPCharacterMotorSliding
    {
        public bool enabled;
        public float slidingSpeed;
        public float sidewaysControl;
        public float speedControl;
    }

    // 跳跃时速度传递的枚举类型 
    public enum CPMovementTransferOnJump
    {
        None,
        InitTransfer,
        PermaTransfer,
        PermaLocked
    }

    //这个脚本实现了角色的移动、跳跃、与移动平台交互以及一些物理相关的操作等功能。 
    public class CPCharacterMotor : MonoBehaviour
    {
        // 判断脚本是否响应输入 
        public bool canControl = true;
        public bool useFixedUpdate = true;
        // 输入移动方向和跳跃状态 
        public Vector3 inputMoveDirection = Vector3.zero;
        public bool inputJump = false;
        // 各个功能模块的实例 
        public CPCharacterMotorMovement movement = new CPCharacterMotorMovement();
        public CPCharacterMotorJumping jumping = new CPCharacterMotorJumping();
        public CPCharacterMotorMovingPlatform movingPlatform = new CPCharacterMotorMovingPlatform();
        public CPCharacterMotorSliding sliding = new CPCharacterMotorSliding();
        // 是否在地面上和地面法线向量 
        public bool grounded = true;
        public Vector3 groundNormal = Vector3.zero;
        private Vector3 lastGroundNormal = Vector3.zero;
        private Transform tr;
        private CharacterController controller;

        private void Awake()
        {
            // 获取角色控制器 
            controller = GetComponent<CharacterController>();
            tr = transform;
        }

        private void Update()
        {
            if (useFixedUpdate)
            {
                return;
            }
            UpdateFunction();
        }

        private void FixedUpdate()
        {
            if (!useFixedUpdate)
            {
                return;
            }
            UpdateFunction();
        }

        private void UpdateFunction()
        {
            // 复制实际速度到临时变量以便操作 
            Vector3 velocity = movement.velocity;
            // 根据输入更新速度 
            velocity = ApplyInputVelocityChange(velocity);
            // 应用重力和跳跃力 
            velocity = ApplyGravityAndJumping(velocity);

            // 移动平台支持 
            Vector3 moveDistance = Vector3.zero;
            if (MoveWithPlatform())
            {
                Vector3 newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
                moveDistance = newGlobalPoint - movingPlatform.activeGlobalPoint;
                if (moveDistance != Vector3.zero)
                {
                    controller.Move(moveDistance);
                }

                // 支持移动平台旋转 
                Quaternion newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
                Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);
                float yRotation = rotationDiff.eulerAngles.y;
                if (yRotation != 0)
                {
                    // 防止局部向上向量旋转 
                    tr.Rotate(0, yRotation, 0);
                }
            }

            // 保存上一位置用于速度计算 
            Vector3 lastPosition = tr.position;

            // 使移动独立于帧率 
            Vector3 currentMovementOffset = velocity * Time.deltaTime;

            // 计算向下推动的偏移量以避免失去接地 
            float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
            if (grounded)
            {
                currentMovementOffset -= pushDownOffset * Vector3.up;
            }

            // 重置将由碰撞函数设置的变量 
            movingPlatform.hitPlatform = null;
            groundNormal = Vector3.zero;

            // 移动角色 
            movement.collisionFlags = controller.Move(currentMovementOffset);
            movement.lastHitPoint = movement.hitPoint;
            lastGroundNormal = groundNormal;

            if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform)
            {
                if (movingPlatform.hitPlatform != null)
                {
                    movingPlatform.activePlatform = movingPlatform.hitPlatform;
                    movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
                    movingPlatform.newPlatform = true;
                }
            }

            // 根据当前和之前的位置计算速度 
            Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.x);
            movement.velocity = (tr.position - lastPosition) / Time.deltaTime;
            Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);

            // 防止角色控制器在碰撞时受到不必要方向的影响 
            if (oldHVelocity == Vector3.zero)
            {
                movement.velocity = new Vector3(0, movement.velocity.y, 0);
            }
            else
            {
                float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
                movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;
            }

            if (movement.velocity.y < velocity.y - 0.001)
            {
                if (movement.velocity.y < 0)
                {
                    // 速度被异常向下改变时，忽略该改变 
                    movement.velocity.y = velocity.y;
                }
                else
                {
                    // 向上移动被阻挡，视为碰到天花板，停止进一步跳跃 
                    jumping.holdingJumpButton = false;
                }
            }

            // 从接地状态变为非接地状态时的处理 
            if (grounded && !IsGroundedTest())
            {
                grounded = false;
                // 应用平台惯性 
                if (movingPlatform.enabled && (movingPlatform.movementTransfer == CPMovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == CPMovementTransferOnJump.PermaTransfer))
                {
                    movement.frameVelocity = movingPlatform.platformVelocity;
                    movement.velocity += movingPlatform.platformVelocity;
                }
                SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
                // 取消向下的偏移量使下落更平滑 
                tr.position += pushDownOffset * Vector3.up;
            }
        }

        // 根据输入改变速度的方法（这里需要根据具体游戏输入逻辑完善） 
        private Vector3 ApplyInputVelocityChange(Vector3 velocity)
        {
            // 这里只是示例，实际需要根据输入系统获取正确的输入方向 
            if (canControl)
            {
                velocity.x = inputMoveDirection.x;
                velocity.z = inputMoveDirection.z;
            }
            return velocity;
        }

        // 应用重力和跳跃力的方法（这里需要根据具体物理逻辑完善跳跃部分） 
        private Vector3 ApplyGravityAndJumping(Vector3 velocity)
        {
            if (!grounded)
            {
                velocity.y -= movement.gravity * Time.deltaTime;
                if (velocity.y < -movement.maxFallSpeed)
                {
                    velocity.y = -movement.maxFallSpeed;
                }
            }
            else if (inputJump && jumping.enabled)
            {
                // 这里只是简单示例，实际跳跃逻辑更复杂 
                velocity.y = Mathf.Sqrt(2 * jumping.baseHeight * movement.gravity);
                jumping.jumping = true;
                jumping.lastStartTime = Time.time;
                jumping.lastButtonDownTime = Time.time;
            }
            return velocity;
        }

        // 与移动平台交互的方法 
        private bool MoveWithPlatform()
        {
            return movingPlatform.enabled && movingPlatform.activePlatform != null;
        }

        // 检测是否接地的测试方法（这里需要根据具体碰撞检测逻辑完善） 
        private bool IsGroundedTest()
        {
            return false;
        }
    }
}

//1.结构体和枚举的定义
//   - 首先定义了`CPCharacterMotorMovement`、`CPCharacterMotorJumping`、`CPCharacterMotorMovingPlatform`和`CPCharacterMotorSliding`结构体，它们分别包含了与角色移动、跳跃、与移动平台交互以及滑动相关的属性。 
//   - 定义了`CPMovementTransferOnJump`枚举类型，用于表示跳跃时速度传递的方式。 
//2. 类的定义和初始化 
//   - 定义了`CPCharacterMotor`类，它继承自`MonoBehaviour`。在类中定义了各种公共和私有变量，包括表示角色状态的变量（如`canControl`、`grounded`等）以及各个功能模块的实例。 
//   - 在`Awake`方法中获取`CharacterController`组件并初始化`tr`（角色的`Transform`）。 
//3. 更新逻辑 
//   - 在`Update`和`FixedUpdate`方法中根据`useFixedUpdate`变量决定调用`UpdateFunction`方法。 
//   - `UpdateFunction`方法包含了角色移动、跳跃、与移动平台交互等一系列逻辑的实现。 
//     - 首先处理速度的更新，包括根据输入更新速度、应用重力和跳跃力。 
//     - 接着处理与移动平台的交互，包括移动和旋转相关的逻辑。 
//     - 然后进行角色的移动操作，包括处理碰撞相关的逻辑、计算速度等。 
//     - 最后处理角色从接地到非接地状态的转换逻辑。 
//4. 功能方法 
//   - `ApplyInputVelocityChange`方法根据输入改变速度，但这里只是简单示例，实际需要根据游戏的输入系统进行完善。 
//   - `ApplyGravityAndJumping`方法应用重力和跳跃力，跳跃部分的逻辑也只是简单示例，需要进一步完善。 
//   - `MoveWithPlatform`方法用于判断是否与移动平台交互。 
//   - `IsGroundedTest`方法用于检测角色是否接地，这里只是简单返回`false`，实际需要根据具体的碰撞检测逻辑来实现。