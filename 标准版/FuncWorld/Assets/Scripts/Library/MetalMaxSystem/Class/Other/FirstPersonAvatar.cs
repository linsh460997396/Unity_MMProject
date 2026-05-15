//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE

using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 这是一个调试用的简易第一人称/自由视角控制系统,适用于开发阶段测试和调试.
    /// 它允许你创建一个简单的Avatar小人,并通过键盘和鼠标控制其移动和视角.
    /// 你可以在游戏中按V键切换第一人称视角模式,在该模式下使用WASD键移动,鼠标控制视角,Space键跳跃或上升,C键下降,Shift键加速,G键切换重力效果.
    /// 再次按V键退出第一人称视角模式.
    /// 这个系统还会自动隐藏UI画布,以提供更沉浸的体验.
    /// 请注意,这个系统是为了快速测试和调试而设计的,并不适合用于正式发布的游戏中.
    /// </summary>
    [System.Serializable]
    public class FirstPersonAvatar
    {
        [Header("=== Avatar 小人设置 ===")]
        public bool useAvatar = true;
        public float avatarHeight = 1.8f;
        public GameObject avatarPrefab;
        public Camera avatarCamera;
        public float cameraDistance = 0.3f;

        [Header("=== 重力设置 ===")]
        public bool hasGravity = false;
        public float gravity = -20f;
        public float jumpForce = 8f;

        [Header("=== 视角控制 ===")]
        public float mouseSensitivity = 2f;
        public float smoothTime = 0.1f;
        public float minVerticalAngle = -89f;
        public float maxVerticalAngle = 89f;

        [Header("=== 移动速度控制 ===")]
        public float moveSpeed = 8f;
        public float sprintMultiplier = 2f;
        public float minMoveSpeed = 2f;
        public float maxMoveSpeed = 50f;

        [Header("=== UI设置 ===")]
        public bool hideUIInFirstPersonMode = true;

        private GameObject avatar;
        private Rigidbody avatarRb;
        private CapsuleCollider avatarCollider;
        private Camera mainCamera;

        private bool isFirstPersonMode = false;
        private float rotationX = 0f;
        private float rotationY = 0f;
        private float targetRotationX = 0f;
        private float targetRotationY = 0f;
        private Vector3 currentVelocity;
        private Vector3 initialAvatarPosition;
        private Quaternion initialAvatarRotation;
        private bool isGrounded = true;

        private Canvas[] uiCanvases;
        private bool[] uiCanvasStates;

        public FirstPersonAvatar() { }

        public FirstPersonAvatar(Camera camera)
        {
            mainCamera = camera;
        }

        public void SetDependencies(Camera camera)
        {
            mainCamera = camera;
        }

        void FindUICanvases()
        {
            uiCanvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
            uiCanvasStates = new bool[uiCanvases.Length];
            for (int i = 0; i < uiCanvases.Length; i++)
            {
                uiCanvasStates[i] = uiCanvases[i].gameObject.activeSelf;
            }
            Debug.Log($"FirstPersonAvatar 找到 {uiCanvases.Length} 个UI画布"); //找画布是为了隐藏UI界面
        }

        void ToggleUICanvases(bool show)
        {
            if (!hideUIInFirstPersonMode)
                return;

            if (uiCanvases == null)
            {
                FindUICanvases();
            }

            for (int i = 0; i < uiCanvases.Length; i++)
            {
                if (uiCanvases[i] != null)
                {
                    uiCanvases[i].gameObject.SetActive(show ? uiCanvasStates[i] : false);
                }
            }
        }

        public void SetUIVisibility(bool show)
        {
            ToggleUICanvases(show);
        }

        public void Create()
        {
            FindUICanvases();

            if (!useAvatar)
                return;

            avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "FirstPersonAvatar";
            avatar.transform.localScale = new Vector3(0.5f, avatarHeight / 2f, 0.5f);

            if (mainCamera != null)
            {
                avatar.transform.position = mainCamera.transform.position;
            }
            else
            {
                avatar.transform.position = new Vector3(0, avatarHeight / 2f, 0);
            }

            avatarRb = avatar.AddComponent<Rigidbody>();
            avatarRb.freezeRotation = true;
            avatarRb.useGravity = false;
            avatarRb.isKinematic = true;

            avatarCollider = avatar.GetComponent<CapsuleCollider>();
            if (avatarCollider != null)
            {
                avatarCollider.material = new PhysicMaterial("AvatarMaterial");
                avatarCollider.material.dynamicFriction = 0.6f;
                avatarCollider.material.staticFriction = 0.6f;
                avatarCollider.material.bounciness = 0f;
            }

            GameObject camObj = new GameObject("AvatarCamera");
            camObj.transform.SetParent(avatar.transform);
            camObj.transform.localPosition = new Vector3(0, avatarHeight * 0.8f, cameraDistance);
            camObj.transform.localRotation = Quaternion.identity;

            avatarCamera = camObj.AddComponent<Camera>();
            avatarCamera.nearClipPlane = 0.1f;
            avatarCamera.enabled = false;

            initialAvatarPosition = avatar.transform.position;
            initialAvatarRotation = avatar.transform.rotation;

            Debug.Log($"Avatar小人已创建,高度: {avatarHeight}m");
            Debug.Log("按V键进入第一人称视角模式");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.V) && avatar != null)
            {
                ToggleFirstPersonMode();
            }

            if (Input.GetKeyDown(KeyCode.G) && isFirstPersonMode)
            {
                ToggleGravity();
            }

            if (isFirstPersonMode)
            {
                HandleMouseLook();
                HandleMovement();

                if (Input.GetKeyDown(KeyCode.Home))
                {
                    ResetPosition();
                }
            }
        }

        void CheckGrounded()
        {
            if (avatar == null)
                return;

            float rayLength = 0.2f;
            Vector3 rayOrigin = avatar.transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength);

            Debug.DrawRay(
                rayOrigin,
                Vector3.down * rayLength,
                isGrounded ? Color.green : Color.red
            );
        }

        void HandleMouseLook()
        {
            if (avatar == null || !isFirstPersonMode || avatarCamera == null)
                return;

            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            targetRotationX += mouseX;
            targetRotationY += mouseY;
            targetRotationY = Mathf.Clamp(targetRotationY, minVerticalAngle, maxVerticalAngle);

            rotationX = Mathf.SmoothDamp(
                rotationX,
                targetRotationX,
                ref currentVelocity.y,
                smoothTime
            );
            rotationY = Mathf.SmoothDamp(
                rotationY,
                targetRotationY,
                ref currentVelocity.x,
                smoothTime
            );

            avatar.transform.rotation = Quaternion.Euler(0, rotationX, 0);
            avatarCamera.transform.localRotation = Quaternion.Euler(-rotationY, 0, 0);
        }

        void HandleMovement()
        {
            if (avatar == null || !isFirstPersonMode)
                return;

            float targetMoveSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                targetMoveSpeed *= sprintMultiplier;
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 forward = avatar.transform.forward;
            Vector3 right = avatar.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            if (hasGravity)
            {
                Vector3 moveDirection = (forward * vertical + right * horizontal) * targetMoveSpeed;

                Vector3 newVelocity = avatarRb.velocity;
                newVelocity.x = moveDirection.x;
                newVelocity.z = moveDirection.z;
                // 有重力时 - 使用 Rigidbody 的 velocity
                if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                {
                    newVelocity.y = jumpForce; // 跳跃
                    isGrounded = false;
                }
                // 直接设置 Rigidbody 速度
                avatarRb.velocity = newVelocity;
            }
            else
            {
                Vector3 moveDirection = (forward * vertical + right * horizontal) * targetMoveSpeed;
                // 无重力时 - 直接修改位置
                if (Input.GetKey(KeyCode.Space))
                {
                    moveDirection.y = targetMoveSpeed; // 上升
                }
                else if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl))
                {
                    moveDirection.y = -targetMoveSpeed; // 下降
                }

                avatarRb.MovePosition(avatar.transform.position + moveDirection * Time.deltaTime);
            }

            CheckGrounded();
        }

        void ToggleGravity()
        {
            hasGravity = !hasGravity;
            if (avatarRb != null)
            {
                avatarRb.useGravity = hasGravity;
                avatarRb.isKinematic = false;
            }

            if (avatarCollider != null)
            {
                avatarCollider.enabled = hasGravity;
            }

            if (hasGravity)
            {
                Debug.Log("重力已开启 - 小人会下坠,按Space跳跃,有碰撞");
                CheckGrounded();
            }
            else
            {
                if (avatarRb != null)
                {
                    avatarRb.velocity = Vector3.zero;
                }
                Debug.Log("重力已关闭 - 小人悬浮,可穿墙自由飞行");
            }
        }

        void ToggleFirstPersonMode()
        {
            if (avatar == null || avatarCamera == null)
            {
                Debug.LogError("Avatar未创建,无法进入第一人称视角！");
                return;
            }

            isFirstPersonMode = !isFirstPersonMode;

            if (isFirstPersonMode)
            {
                avatarCamera.enabled = true;
                avatarRb.isKinematic = false;

                rotationX = avatar.transform.eulerAngles.y;
                rotationY = 0;
                targetRotationX = rotationX;
                targetRotationY = rotationY;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                ToggleUICanvases(false);

                Debug.Log("=== 第一人称视角模式已激活 ===");
                Debug.Log("控制说明: 鼠标移动视角 | WASD移动 | Space上升/跳跃 | C下降 | Shift加速 | G切换重力 | V退出第一人称");
            }
            else
            {
                avatarCamera.enabled = false;
                avatarRb.isKinematic = true;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ToggleUICanvases(true);

                Debug.Log("第一人称视角模式已退出");
            }
        }

        void ResetPosition()
        {
            if (avatar != null && isFirstPersonMode)
            {
                avatar.transform.position = initialAvatarPosition;
                avatar.transform.rotation = initialAvatarRotation;
                rotationX = avatar.transform.eulerAngles.y;
                rotationY = 0;
                targetRotationX = rotationX;
                targetRotationY = rotationY;
                Debug.Log("Avatar位置已重置");
            }
        }
    }
}

#endif

//使用示范
//public class MonoGo : MonoBehaviour
//{
//    [Header("=== 第一人称/自由视角 ===")]
//    public FirstPersonAvatar avatar;

//    private void Start()
//    {
//        // 初始化第一人称/自由视角
//        if (avatar == null)
//        {
//            avatar = new FirstPersonAvatar(Camera.main);
//        }
//        else
//        {
//            avatar.SetDependencies(Camera.main);
//        }
//        avatar.Create();
//    }

//    private void Update()
//    {
//        // 更新第一人称/自由视角
//        if (avatar != null)
//        {
//            avatar.Update();
//        }
//    }
//}
