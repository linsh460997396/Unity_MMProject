#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using Random = UnityEngine.Random;

namespace MetalMaxSystem.Unity
{
    //第一人称控制器
    //[RequireComponent(typeof(CharacterController))]
    //[RequireComponent(typeof(AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed = 4f;
        [SerializeField] private float m_RunSpeed = 6f;
        [SerializeField][Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed = 8f;
        [SerializeField] private float m_StickToGroundForce = 2f;
        [SerializeField] private float m_GravityMultiplier = 2f; //重力倍数
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval = 5f;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        /// <summary>
        /// 跳跃状态
        /// </summary>
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        void Awake()
        {
            if (GetComponent<CharacterController>() == null)
            {
                //如果没有角色控制器组件,则添加一个
                m_CharacterController = gameObject.AddComponent<CharacterController>();
                m_CharacterController.stepOffset = 0.3f;
                m_CharacterController.skinWidth = 0.08f;
                m_CharacterController.center = new Vector3(0, 0, 0);
                m_CharacterController.height = 1.8f;
                m_CharacterController.radius = 0.5f;
                m_CharacterController.slopeLimit = 45f;
                m_CharacterController.minMoveDistance = 0f;
            }
            if (GetComponent<AudioSource>() == null)
            {
                //如果没有AudioSource组件,则添加一个
                m_AudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>(); //获取角色控制器实例

            if (Camera.main == null)
            {
                Debug.LogWarning("Camera.main == null. Create a new one.");
                m_Camera = CreatePerspectiveCamera("MCMainCamera", new Vector3(0, 40, 0));
                m_Camera.tag = "MainCamera";
                m_Camera.gameObject.transform.parent = this.transform;
                m_Camera.gameObject.transform.localPosition = new Vector3(0, 0.8f, 0); //相机位置设定在角色的头部
            }
            else
            {
                m_Camera = Camera.main;//获取主摄像机
            }

            m_OriginalCameraPosition = m_Camera.transform.localPosition;//获取主摄像机的本地坐标
            m_FovKick.Setup(m_Camera);//视野（Field of View, FOV)的“踢动”或变化效果,在射击游戏或其他需要模拟武器后坐力的游戏中,FOV踢动是一种常见的技术,用于模拟玩家在射击时视线的震动或偏移.
            m_HeadBob.Setup(m_Camera, m_StepInterval);//走路时的头部晃动效果
            m_StepCycle = 0f;//用于跟踪头部晃动的周期或进度
            m_NextStep = m_StepCycle / 2f;//用于确定下一次晃动何时发生,设置为 m_StepCycle 的一半这可能是为了在某个时间点触发晃动效果,比如当前晃动周期的一半时,若 m_StepCycle 是用来跟踪当前晃动周期的话,那么 m_NextStep 可能用于确定何时开始下一个晃动周期
            m_Jumping = false;//跳跃状态
            m_AudioSource = GetComponent<AudioSource>();//获取音频组件的实例
            m_MouseLook.Init(transform, m_Camera.transform);//鼠标注视功能的初始化
        }


        /// <summary>
        /// 处理角色的跳跃、着陆和空中移动行为,确保角色在不同的状态下有正确的响应和动画
        /// </summary>
        private void Update()
        {
            RotateView(); //旋转或更新角色的视角

            // the jump state needs to read here to make sure it is not missed

            //检查m_Jump变量是否为false.若是,它会使用Input来检测玩家是否按下了“Jump”按钮.若玩家按下了跳跃按钮,m_Jump会被设置为true
            if (!m_Jump)
            {
                m_Jump = Input.GetButtonDown("Jump");
            }
            //处理着陆情况
            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            { //检查角色是否刚刚从非着陆状态变为着陆状态.若是,它会执行以下动作
                StartCoroutine(m_JumpBob.DoBobCycle()); //使用协程来播放一个跳跃后的抖动动画
                PlayLandingSound();//播放着陆声效
                m_MoveDir.y = 0f;//将m_MoveDir.y设置为0f,可能意味着角色在着陆时停止垂直移动
                m_Jumping = false;//将m_Jumping设置为false,表示角色当前不在跳跃状态
            }
            //处理角色在空中时的移动
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                //检查角色是否在空中（即非着陆状态)并且没有在跳跃,但之前是在着陆状态.若是它会将m_MoveDir.y设置为0f,意味着角色在空中时不会进行垂直移动
                m_MoveDir.y = 0f;
            }
            //更新着陆状态（m_PreviouslyGrounded),使其与m_CharacterController.isGrounded的值相同
            //m_CharacterController.isGrounded是表示角色是否着陆的布尔值,在下一次Update调用时,m_PreviouslyGrounded将包含上一次Update时的着陆状态
            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        /// <summary>
        /// 创建基础透视镜头
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="position"></param>
        /// <param name="lookAt"></param>
        /// <param name="fieldOfView"></param>
        /// <param name="nearClipPlane"></param>
        /// <param name="farClipPlane"></param>
        /// <returns></returns>
        public static Camera CreatePerspectiveCamera(
            string cameraName = "PerspectiveCamera",
            Vector3 position = default,
            Vector3 lookAt = default,
            float fieldOfView = 60f,
            float nearClipPlane = 0.3f,
            float farClipPlane = 1000f)
        {
            // 创建新的游戏对象
            GameObject cameraObject = new GameObject(cameraName);

            // 添加Camera组件
            Camera camera = cameraObject.AddComponent<Camera>();

            // 设置透视参数
            camera.orthographic = false; // 确保是透视模式
            camera.fieldOfView = fieldOfView;
            camera.nearClipPlane = nearClipPlane;
            camera.farClipPlane = farClipPlane;

            // 设置位置和朝向
            camera.transform.position = position == default ? Vector3.zero : position;
            camera.transform.LookAt(lookAt == default ? Vector3.forward : lookAt);

            return camera;
        }

        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x * speed;
            m_MoveDir.z = desiredMove.z * speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }

        // 这段代码是一个名为 FixedUpdate 的私有方法,通常在 Unity 的游戏开发中用作物理更新的固定频率调用.从代码的结构和逻辑来看,这个方法主要用于控制一个角色的移动和跳跃行为,并更新其视角和碰撞状态.以下是对代码的详细解读：

        // 初始化变量:

        // speed: 角色的移动速度.
        // desiredMove: 根据输入计算出的期望移动方向.
        // 获取输入:

        // 调用 GetInput 方法获取玩家的输入,并将结果存储在 speed 变量中.
        // 计算期望的移动方向:

        // 结合玩家输入和角色的朝向,计算出一个期望的移动方向 desiredMove.
        // RaycastHit 获取表面信息:

        // 使用 Physics.SphereCast 发射一个球形射线,检查角色下方是否有可碰撞的表面.
        // 若检测到表面,将 desiredMove 投影到这个表面上,确保角色沿着这个表面移动.
        // 设置移动方向:

        // 根据投影后的 desiredMove 和玩家的移动速度 speed,设置 m_MoveDir（即角色实际的移动方向).
        // 处理地面状态和跳跃:

        // 若角色站立在地面上 (m_CharacterController.isGrounded 为真)：
        // 向下施加一个较小的力 (m_StickToGroundForce),使角色紧贴地面.
        // 若玩家按下跳跃键 (m_Jump 为真),则增加向上的跳跃力,播放跳跃声音,并更新跳跃状态.
        // 若角色不在地面上,则考虑重力影响,增加向下的力.
        // 移动角色并处理碰撞:

        // 使用 m_CharacterController.Move 方法移动角色,并考虑时间步长 (Time.fixedDeltaTime).
        // 获取并存储碰撞信息 (m_CollisionFlags).
        // 更新其他状态:

        // 调用 ProgressStepCycle 方法,可能用于更新角色的步伐或动画状态.
        // 调用 UpdateCameraPosition 方法,更新角色的视角或相机位置.
        // 调用 m_MouseLook.UpdateCursorLock 方法,更新鼠标锁定状态,可能用于第一人称视角的控制.
        // 总体而言,这个 FixedUpdate 方法是控制角色移动和跳跃行为的核心,同时处理与环境的物理交互和视觉反馈.


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }

        /// <summary>
        /// 旋转视角
        /// </summary>
        private void RotateView()
        {
            m_MouseLook.LookRotation(transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}

// 这段代码是Unity中第一人称控制器（First Person Controller)的实现,用于处理玩家的移动、跳跃、视角旋转以及其他一些动作.代码使用了Unity的内置组件和API.
// 下面是对代码主要部分的解释：
// 命名空间（Namespace)：UnityStandardAssets.Characters.FirstPerson 表示这个脚本属于Unity标准资源包中的第一人称角色控制器.
// 类定义（Class Definition)：FirstPersonController 类继承自 MonoBehaviour,这意味着它是一个Unity组件,可以附加到游戏对象上.
// 属性（Properties)：
// m_IsWalking：玩家是否正在行走.
// m_WalkSpeed 和 m_RunSpeed：玩家行走和奔跑的速度.
// m_RunstepLenghten：奔跑时每一步的长度.
// m_JumpSpeed：玩家跳跃的初速度.
// m_StickToGroundForce：玩家贴近地面的力（角色跳跃时增加值使更快地落地,或在斜坡上行走时通过调整这个值来使角色更容易沿着斜坡滑动)
// m_GravityMultiplier：重力乘数,用于调整玩家受到的重力影响.
// m_MouseLook：用于处理鼠标视角旋转的组件.
// m_UseFovKick 和 m_FovKick：是否使用视野抖动效果,以及视野抖动的参数.
// m_UseHeadBob、m_HeadBob 和 m_JumpBob：是否使用头部抖动效果,以及头部抖动的参数.
// m_StepInterval：每一步之间的时间间隔.
// m_FootstepSounds：脚步声数组,随机选择播放.
// m_JumpSound 和 m_LandSound：跳跃和落地时的声音.
// 私有变量（Private Variables)：
// m_Camera：玩家的相机.
// m_Jump：一个标记,表示玩家是否应该跳跃.
// m_YRotation：玩家的Y轴旋转（通常用于视角).
// m_Input：玩家输入（例如,键盘和鼠标输入).
// m_MoveDir：玩家的移动方向.
// m_CharacterController：CharacterController 组件,用于处理物理碰撞和移动.
// m_CollisionFlags：碰撞标志,用于检测与环境的碰撞.
// m_PreviouslyGrounded：一个标记,表示玩家上一帧是否在地面上.
// m_OriginalCameraPosition：相机原始位置,用于处理头部抖动.
// m_StepCycle 和 m_NextStep：用于处理步伐周期的变量.
// m_Jumping：一个标记,表示玩家当前是否在跳跃.
// m_AudioSource：音频源组件,用于播放声音.

#endif