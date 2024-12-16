using UnityEngine;

namespace CellSpace.Examples
{
    // ��ɫ�ƶ���صĽṹ�壬���������ƶ����� 
    public struct CPCharacterMotorMovement
    {
        // ���ˮƽ�ٶȣ���ǰ����ࡢ��� 
        public float maxForwardSpeed;
        public float maxSidewaysSpeed;
        public float maxBackwardsSpeed;
        // �����¶ȵ��ٶ����� 
        public AnimationCurve slopeSpeedMultiplier;
        // ����Ϳ��е������ٶ� 
        public float maxGroundAcceleration;
        public float maxAirAcceleration;
        // ��������������ٶ� 
        public float gravity;
        public float maxFallSpeed;
        // ��ײ��־���ٶ���ر�������ײ����ر��� 
        public CollisionFlags collisionFlags;
        public Vector3 velocity;
        public Vector3 frameVelocity;
        public Vector3 hitPoint;
        public Vector3 lastHitPoint;
    }

    // ��Ծ��صĽṹ�壬������Ծ���� 
    public struct CPCharacterMotorJumping
    {
        // �ܷ���Ծ��������Ծ�߶ȡ�������Ծ�߶� 
        public bool enabled;
        public float baseHeight;
        public float extraHeight;
        // �ڿ����߱���Ͷ��ͱ��洹ֱ��Ծ�ĳ̶� 
        public float perpAmount;
        public float steepPerpAmount;
        // ��Ծ��ص�״̬���� 
        public bool jumping;
        public bool holdingJumpButton;
        public float lastStartTime;
        public float lastButtonDownTime;
        public Vector3 jumpDir;
    }

    // �ƶ�ƽ̨��صĽṹ�壬�����ƶ�ƽ̨���������� 
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

    // ������صĽṹ�壬������������ 
    public struct CPCharacterMotorSliding
    {
        public bool enabled;
        public float slidingSpeed;
        public float sidewaysControl;
        public float speedControl;
    }

    // ��Ծʱ�ٶȴ��ݵ�ö������ 
    public enum CPMovementTransferOnJump
    {
        None,
        InitTransfer,
        PermaTransfer,
        PermaLocked
    }

    //����ű�ʵ���˽�ɫ���ƶ�����Ծ�����ƶ�ƽ̨�����Լ�һЩ������صĲ����ȹ��ܡ� 
    public class CPCharacterMotor : MonoBehaviour
    {
        // �жϽű��Ƿ���Ӧ���� 
        public bool canControl = true;
        public bool useFixedUpdate = true;
        // �����ƶ��������Ծ״̬ 
        public Vector3 inputMoveDirection = Vector3.zero;
        public bool inputJump = false;
        // ��������ģ���ʵ�� 
        public CPCharacterMotorMovement movement = new CPCharacterMotorMovement();
        public CPCharacterMotorJumping jumping = new CPCharacterMotorJumping();
        public CPCharacterMotorMovingPlatform movingPlatform = new CPCharacterMotorMovingPlatform();
        public CPCharacterMotorSliding sliding = new CPCharacterMotorSliding();
        // �Ƿ��ڵ����Ϻ͵��淨������ 
        public bool grounded = true;
        public Vector3 groundNormal = Vector3.zero;
        private Vector3 lastGroundNormal = Vector3.zero;
        private Transform tr;
        private CharacterController controller;

        private void Awake()
        {
            // ��ȡ��ɫ������ 
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
            // ����ʵ���ٶȵ���ʱ�����Ա���� 
            Vector3 velocity = movement.velocity;
            // ������������ٶ� 
            velocity = ApplyInputVelocityChange(velocity);
            // Ӧ����������Ծ�� 
            velocity = ApplyGravityAndJumping(velocity);

            // �ƶ�ƽ̨֧�� 
            Vector3 moveDistance = Vector3.zero;
            if (MoveWithPlatform())
            {
                Vector3 newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
                moveDistance = newGlobalPoint - movingPlatform.activeGlobalPoint;
                if (moveDistance != Vector3.zero)
                {
                    controller.Move(moveDistance);
                }

                // ֧���ƶ�ƽ̨��ת 
                Quaternion newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
                Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);
                float yRotation = rotationDiff.eulerAngles.y;
                if (yRotation != 0)
                {
                    // ��ֹ�ֲ�����������ת 
                    tr.Rotate(0, yRotation, 0);
                }
            }

            // ������һλ�������ٶȼ��� 
            Vector3 lastPosition = tr.position;

            // ʹ�ƶ�������֡�� 
            Vector3 currentMovementOffset = velocity * Time.deltaTime;

            // ���������ƶ���ƫ�����Ա���ʧȥ�ӵ� 
            float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
            if (grounded)
            {
                currentMovementOffset -= pushDownOffset * Vector3.up;
            }

            // ���ý�����ײ�������õı��� 
            movingPlatform.hitPlatform = null;
            groundNormal = Vector3.zero;

            // �ƶ���ɫ 
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

            // ���ݵ�ǰ��֮ǰ��λ�ü����ٶ� 
            Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.x);
            movement.velocity = (tr.position - lastPosition) / Time.deltaTime;
            Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);

            // ��ֹ��ɫ����������ײʱ�ܵ�����Ҫ�����Ӱ�� 
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
                    // �ٶȱ��쳣���¸ı�ʱ�����Ըøı� 
                    movement.velocity.y = velocity.y;
                }
                else
                {
                    // �����ƶ����赲����Ϊ�����컨�壬ֹͣ��һ����Ծ 
                    jumping.holdingJumpButton = false;
                }
            }

            // �ӽӵ�״̬��Ϊ�ǽӵ�״̬ʱ�Ĵ��� 
            if (grounded && !IsGroundedTest())
            {
                grounded = false;
                // Ӧ��ƽ̨���� 
                if (movingPlatform.enabled && (movingPlatform.movementTransfer == CPMovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == CPMovementTransferOnJump.PermaTransfer))
                {
                    movement.frameVelocity = movingPlatform.platformVelocity;
                    movement.velocity += movingPlatform.platformVelocity;
                }
                SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
                // ȡ�����µ�ƫ����ʹ�����ƽ�� 
                tr.position += pushDownOffset * Vector3.up;
            }
        }

        // ��������ı��ٶȵķ�����������Ҫ���ݾ�����Ϸ�����߼����ƣ� 
        private Vector3 ApplyInputVelocityChange(Vector3 velocity)
        {
            // ����ֻ��ʾ����ʵ����Ҫ��������ϵͳ��ȡ��ȷ�����뷽�� 
            if (canControl)
            {
                velocity.x = inputMoveDirection.x;
                velocity.z = inputMoveDirection.z;
            }
            return velocity;
        }

        // Ӧ����������Ծ���ķ�����������Ҫ���ݾ��������߼�������Ծ���֣� 
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
                // ����ֻ�Ǽ�ʾ����ʵ����Ծ�߼������� 
                velocity.y = Mathf.Sqrt(2 * jumping.baseHeight * movement.gravity);
                jumping.jumping = true;
                jumping.lastStartTime = Time.time;
                jumping.lastButtonDownTime = Time.time;
            }
            return velocity;
        }

        // ���ƶ�ƽ̨�����ķ��� 
        private bool MoveWithPlatform()
        {
            return movingPlatform.enabled && movingPlatform.activePlatform != null;
        }

        // ����Ƿ�ӵصĲ��Է�����������Ҫ���ݾ�����ײ����߼����ƣ� 
        private bool IsGroundedTest()
        {
            return false;
        }
    }
}

//1.�ṹ���ö�ٵĶ���
//   - ���ȶ�����`CPCharacterMotorMovement`��`CPCharacterMotorJumping`��`CPCharacterMotorMovingPlatform`��`CPCharacterMotorSliding`�ṹ�壬���Ƿֱ���������ɫ�ƶ�����Ծ�����ƶ�ƽ̨�����Լ�������ص����ԡ� 
//   - ������`CPMovementTransferOnJump`ö�����ͣ����ڱ�ʾ��Ծʱ�ٶȴ��ݵķ�ʽ�� 
//2. ��Ķ���ͳ�ʼ�� 
//   - ������`CPCharacterMotor`�࣬���̳���`MonoBehaviour`�������ж����˸��ֹ�����˽�б�����������ʾ��ɫ״̬�ı�������`canControl`��`grounded`�ȣ��Լ���������ģ���ʵ���� 
//   - ��`Awake`�����л�ȡ`CharacterController`�������ʼ��`tr`����ɫ��`Transform`���� 
//3. �����߼� 
//   - ��`Update`��`FixedUpdate`�����и���`useFixedUpdate`������������`UpdateFunction`������ 
//   - `UpdateFunction`���������˽�ɫ�ƶ�����Ծ�����ƶ�ƽ̨������һϵ���߼���ʵ�֡� 
//     - ���ȴ����ٶȵĸ��£�����������������ٶȡ�Ӧ����������Ծ���� 
//     - ���Ŵ������ƶ�ƽ̨�Ľ����������ƶ�����ת��ص��߼��� 
//     - Ȼ����н�ɫ���ƶ�����������������ײ��ص��߼��������ٶȵȡ� 
//     - ������ɫ�ӽӵص��ǽӵ�״̬��ת���߼��� 
//4. ���ܷ��� 
//   - `ApplyInputVelocityChange`������������ı��ٶȣ�������ֻ�Ǽ�ʾ����ʵ����Ҫ������Ϸ������ϵͳ�������ơ� 
//   - `ApplyGravityAndJumping`����Ӧ����������Ծ������Ծ���ֵ��߼�Ҳֻ�Ǽ�ʾ������Ҫ��һ�����ơ� 
//   - `MoveWithPlatform`���������ж��Ƿ����ƶ�ƽ̨������ 
//   - `IsGroundedTest`�������ڼ���ɫ�Ƿ�ӵأ�����ֻ�Ǽ򵥷���`false`��ʵ����Ҫ���ݾ������ײ����߼���ʵ�֡�