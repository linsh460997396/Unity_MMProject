//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE

using UnityEngine;
using UnityEngine.SceneManagement;

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
    public class FirstPersonAvatar : MonoBehaviour
    {
        private static string _name = "FirstPersonAvatar";
        public static string Name
        {
            get { if (string.IsNullOrEmpty(_name)) return "FirstPersonAvatar"; return _name; }
            set { if (!string.IsNullOrEmpty(value)) _name = value; }
        }
        private static FirstPersonAvatar _instance;
        public static FirstPersonAvatar Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = GameObject.Find(Name);
                    if (obj == null) obj = new GameObject(Name);
                    _instance = obj.GetComponent<FirstPersonAvatar>();
                    if (_instance == null) _instance = obj.AddComponent<FirstPersonAvatar>();
                    GameObject.DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        [Header("=== Avatar 小人设置 ===")]
        public bool useAvatar = true;
        public float avatarHeight = 1.8f;
        public Camera avatarCamera;
        public float cameraDistance = 0.3f;
        [Tooltip("小人是否半透明显示(默认透明),便于第一人称视角下不遮挡视线")]
        public bool transparentAvatar = true;
        [Tooltip("小人半透明度(0=完全透明,1=完全不透明),仅当 transparentAvatar=true 时生效")]
        [Range(0f, 1f)]
        public float avatarAlpha = 0f;

        [Header("=== 重力设置 ===")]
        public bool hasGravity = false;
        public float jumpForce = 8f;

        [Header("=== 视角控制 ===")]
        public float mouseSensitivity = 2f;
        public float smoothTime = 0.1f;
        public float minVerticalAngle = -89f;
        public float maxVerticalAngle = 89f;

        [Header("=== 移动速度控制 ===")]
        public float moveSpeed = 8f;
        public float sprintMultiplier = 2f;

        [Header("=== UI设置 ===")]
        public bool hideUIInFirstPersonMode = true;

        private GameObject avatar;
        private Rigidbody avatarRb;
        private CapsuleCollider avatarCollider;

        // avatar 是否处于激活使用状态(隐藏时为 false,SetActive 复用避免反复 Instantiate/Destroy)
        private bool isAvatarActive = false;
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
        private Camera[] otherCameras;
        private bool[] otherCameraStates;
        private float[] otherCameraDepths;
        private Material cachedMaterial;
        private Coroutine cursorLockCoroutine;

        /// <summary>
        /// 初始化单例并同步创建小人.访问 Instance 触发单例 GameObject 创建,再同步确保 avatar 创建.
        /// Avatar自带专用相机,无需外部相机依赖.
        /// 调用后 Unity 仍会自动调用 Start(),但内部幂等不会重复创建.
        /// </summary>
        /// <param name="alpha">透明度(0=完全透明,1=完全不透明).默认 -1 表示沿用 avatarAlpha 字段值;传 0~1 会覆盖字段值</param>
        /// <returns>单例实例</returns>
        public static FirstPersonAvatar Init(float alpha = -1f)
        {
            var inst = Instance;
            inst.EnsureAvatarCreated(alpha);
            return inst;
        }

        /// <summary>
        /// 幂等创建 avatar.若已存在则跳过.由 Init() 和 Start() 调用.
        /// </summary>
        /// <param name="alpha">透明度,默认 -1 沿用字段值</param>
        void EnsureAvatarCreated(float alpha = -1f)
        {
            if (avatar != null) return;
            if (!useAvatar) return;

            if (alpha >= 0f) avatarAlpha = Mathf.Clamp01(alpha);

            CheckInputMode();
            BuildAvatarGameObject();
            Debug.Log($"Avatar小人已创建,高度: {avatarHeight}m,透明: {(transparentAvatar ? "是(alpha=" + avatarAlpha + ")" : "否")}");
            Debug.Log("按F键显示Avatar,按V键进入第一人称视角模式");
        }

        /// <summary>
        /// 实际构建 Avatar GameObject 及其全部组件(Rigidbody/Collider/AvatarCamera等).
        /// </summary>
        void BuildAvatarGameObject()
        {
            avatar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            avatar.name = "Avatar";
            avatar.transform.localScale = new Vector3(0.5f, avatarHeight / 2f, 0.5f);
            // 应用透明效果(若启用)
            ApplyAvatarTransparency();
            avatar.transform.position = new Vector3(0, avatarHeight / 2f, 0);
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
            avatarCamera.depth = 100f;
            avatarCamera.enabled = false;

            initialAvatarPosition = avatar.transform.position;
            initialAvatarRotation = avatar.transform.rotation;
            avatar.SetActive(false);
            isAvatarActive = false;
        }

        /// <summary>
        /// 应用小人透明效果.根据当前材质 Shader 类型自动选择 Standard 或 URP/Lit 的透明设置.
        /// 仅当 transparentAvatar=true 时生效,运行时修改字段后可手动调用此方法刷新.
        /// </summary>
        public void ApplyAvatarTransparency()
        {
            if (avatar == null || !transparentAvatar)
                return;

            Renderer renderer = avatar.GetComponent<Renderer>();
            if (renderer == null)
                return;

            if (cachedMaterial == null || renderer.material.shader != cachedMaterial.shader)
            {
                if (cachedMaterial != null)
                    UnityEngine.Object.Destroy(cachedMaterial);
                cachedMaterial = new Material(renderer.material);
                renderer.material = cachedMaterial;
            }

            Material mat = cachedMaterial;

            Color color = mat.color;
            color.a = avatarAlpha;
            mat.color = color;

            string shaderName = mat.shader.name;
            if (shaderName == "Standard")
            {
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            else if (shaderName == "Universal Render Pipeline/Lit" || shaderName == "URP/Lit")
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
                mat.EnableKeyword("_SURFACE_TRANSPARENT");
                mat.DisableKeyword("_SURFACE_OPAQUE");
            }
            else
            {
                mat.renderQueue = 3000;
                Debug.LogWarning($"未识别的 Shader: {shaderName},仅设置 renderQueue 和 alpha,可能无法达到透明效果.");
            }
        }

        /// <summary>
        /// 彻底销毁小人及其子物体,并重置内部状态.调用后可再次调用 Init() 重建.
        /// 注意:此为真正的销毁(不同于 F 键的 SetActive 隐藏),会调用 Object.Destroy.
        /// </summary>
        public void DestroyAvatar()
        {
            if (isFirstPersonMode)
            {
                ToggleFirstPersonMode();
            }

            ToggleOtherCameras(true);

            if (avatar != null)
            {
                UnityEngine.Object.Destroy(avatar);
            }

            if (cachedMaterial != null)
            {
                UnityEngine.Object.Destroy(cachedMaterial);
                cachedMaterial = null;
            }

            avatar = null;
            avatarRb = null;
            avatarCollider = null;
            avatarCamera = null;
            isFirstPersonMode = false;
            isAvatarActive = false;
            otherCameras = null;
            otherCameraStates = null;
            otherCameraDepths = null;
            Debug.Log("Avatar小人已销毁,可调用 Init() 重建.");
        }

        void OnDestroy()
        {
            ToggleOtherCameras(true);
            UnlockCursor();

            if (cachedMaterial != null)
            {
                UnityEngine.Object.Destroy(cachedMaterial);
                cachedMaterial = null;
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnlockCursor();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (isAvatarActive)
            {
                FindOtherCameras();
                ToggleOtherCameras(false);
            }
        }

        void LockCursor()
        {
            if (cursorLockCoroutine != null)
                StopCoroutine(cursorLockCoroutine);

            cursorLockCoroutine = StartCoroutine(CursorLockCoroutine());
        }

        System.Collections.IEnumerator CursorLockCoroutine()
        {
            while (isFirstPersonMode)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                yield return null;
            }
        }

        void UnlockCursor()
        {
            if (cursorLockCoroutine != null)
            {
                StopCoroutine(cursorLockCoroutine);
                cursorLockCoroutine = null;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>小人是否已创建(可能已创建但处于隐藏状态).</summary>
        public bool IsCreated => avatar != null;

        /// <summary>小人是否处于激活使用状态(已创建且未被隐藏).</summary>
        public bool IsActive => isAvatarActive;

        /// <summary>
        /// 切换小人的显示/隐藏.隐藏时仅 SetActive(false) 保留实例,避免反复 Instantiate/Destroy.
        /// 内部由 F 键调用,也可手动调用.
        /// </summary>
        void ToggleAvatarActive()
        {
            if (avatar == null)
            {
                EnsureAvatarCreated();
                return;
            }

            if (isAvatarActive)
            {
                isAvatarActive = false;

                if (isFirstPersonMode)
                {
                    ToggleFirstPersonMode();
                }
                else
                {
                    ToggleOtherCameras(true);
                }
                avatar.SetActive(false);
                Debug.Log("Avatar小人已隐藏,再按F键显示");
            }
            else
            {
                FindOtherCameras();
                ToggleOtherCameras(false);
                avatar.SetActive(true);
                isAvatarActive = true;
                Debug.Log("Avatar小人已显示");
            }
        }

        void FindUICanvases()
        {
            uiCanvases = UnityEngine.Object.FindObjectsOfType<Canvas>();
            uiCanvasStates = new bool[uiCanvases.Length];
            for (int i = 0; i < uiCanvases.Length; i++)
            {
                uiCanvasStates[i] = uiCanvases[i].gameObject.activeSelf;
            }
        }

        void FindOtherCameras()
        {
            Camera[] allCameras = UnityEngine.Object.FindObjectsOfType<Camera>();
            System.Collections.Generic.List<Camera> otherCameraList = new System.Collections.Generic.List<Camera>();
            for (int i = 0; i < allCameras.Length; i++)
            {
                if (allCameras[i] != avatarCamera)
                {
                    otherCameraList.Add(allCameras[i]);
                }
            }
            otherCameras = otherCameraList.ToArray();
            otherCameraStates = new bool[otherCameras.Length];
            otherCameraDepths = new float[otherCameras.Length];
            for (int i = 0; i < otherCameras.Length; i++)
            {
                otherCameraStates[i] = otherCameras[i].enabled;
                otherCameraDepths[i] = otherCameras[i].depth;
            }
        }

        UGUITemplate.InputSupportType currentInputMode;

        //当前 CheckInputMode() 只做了检测但未使用。由于完整的新Input System适配需要引入额外命名空间且较为复杂，当前实现保留了诊断功能。如果需要，可以作为后续优化项。
        void CheckInputMode()
        {
            currentInputMode = UGUITemplate.CheckAvailableInputModule();
            Debug.Log($"FirstPersonAvatar 输入模式: {currentInputMode}");
        }

        void ToggleOtherCameras(bool enable)
        {
            if (otherCameras == null)
            {
                FindOtherCameras();
            }

            for (int i = 0; i < otherCameras.Length; i++)
            {
                if (otherCameras[i] != null)
                {
                    otherCameras[i].enabled = enable ? otherCameraStates[i] : false;
                    if (!enable && otherCameraDepths[i] >= avatarCamera.depth)
                    {
                        otherCameras[i].depth = -100f;
                    }
                    else if (enable)
                    {
                        otherCameras[i].depth = otherCameraDepths[i];
                    }
                }
            }
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

        void Start()
        {
            // 幂等创建:若 Init() 已同步创建则跳过,否则由 Unity 生命周期触发首次创建
            EnsureAvatarCreated();
        }

        void Update()
        {
            // F 键:切换小人的显示/隐藏(SetActive 复用,避免反复 Instantiate/Destroy)
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleAvatarActive();
            }

            if (!isAvatarActive && !isFirstPersonMode)
                return;

            // V 键:仅在小人激活时响应(避免对隐藏中的小人切换第一人称)
            if (Input.GetKeyDown(KeyCode.V) && isAvatarActive)
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
                ref currentVelocity.x,
                smoothTime
            );
            rotationY = Mathf.SmoothDamp(
                rotationY,
                targetRotationY,
                ref currentVelocity.y,
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
        }

        void FixedUpdate()
        {
            if (isFirstPersonMode && hasGravity)
            {
                CheckGrounded();
            }
        }

        void ToggleGravity()
        {
            hasGravity = !hasGravity;
            if (avatarRb != null)
            {
                avatarRb.useGravity = hasGravity;
                avatarRb.isKinematic = !hasGravity;
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

                LockCursor();
                ToggleUICanvases(false);

                Debug.Log("=== 第一人称视角模式已激活 ===");
                Debug.Log("控制说明: 鼠标移动视角 | WASD移动 | Space上升/跳跃 | C下降 | Shift加速 | G切换重力 | V退出第一人称");
            }
            else
            {
                avatarCamera.enabled = false;
                avatarRb.isKinematic = true;

                UnlockCursor();
                ToggleUICanvases(true);

                if (isAvatarActive)
                {
                    ToggleOtherCameras(false);
                }
                else
                {
                    ToggleOtherCameras(true);
                }

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
//    private void Start()
//    {
//        // 初始化单例并创建小人(也可传透明度: FirstPersonAvatar.Init(0.5f))
//        // 透明度: transparentAvatar(默认true) / avatarAlpha(默认0f,完全透明)
//        FirstPersonAvatar.Init();
//    }
//    // Update 无需手动调用:FirstPersonAvatar 自身作为 MonoBehaviour,其 Update() 由 Unity 自动执行
//    // 框架内部自动处理 F(显示/隐藏切换,SetActive 复用)、V(第一人称切换)、G(重力)、Home(重置) 等快捷键
//    // 彻底销毁小人请手动调用 FirstPersonAvatar.Instance.DestroyAvatar()(与F键的隐藏不同)
//}
