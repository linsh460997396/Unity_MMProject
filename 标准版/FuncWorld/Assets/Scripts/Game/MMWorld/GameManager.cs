using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellSpace;
using SpriteSpace;
using MMWorld.HexSphere;
using MMWorld.RimWorld;

namespace MMWorld
{
    /// <summary>
    /// 游戏管理器 - 负责游戏的整体生命周期管理
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region 单例

        /// <summary>
        /// 单例实例
        /// </summary>
        public static GameManager Instance { get; private set; }

        #endregion

        #region RimWorld系统

        /// <summary>
        /// RimWorld游戏集成器
        /// </summary>
        private RimWorldGameIntegrator rimWorldIntegrator;

        #endregion

        #region 游戏状态

        /// <summary>
        /// 游戏状态
        /// </summary>
        public enum GameState
        {
            StartMenu,      // 开局菜单
            PlanetSelect,   // 星球选择（HexSphere）
            Loading,        // 加载中
            Playing,        // 游戏中
            Paused,         // 暂停
            MapEditing      // 地图编辑模式
        }

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState currentState { get; private set; } = GameState.StartMenu;

        /// <summary>
        /// 当前选中的星球预设
        /// </summary>
        private PlanetPreset currentPlanetPreset;

        #endregion

        #region 游戏对象引用

        /// <summary>
        /// 玩家角色
        /// </summary>
        public GameObject player { get; private set; }

        /// <summary>
        /// 玩家控制器脚本
        /// </summary>
        public SimplePlayerController playerController { get; private set; }

        /// <summary>
        /// NPC列表
        /// </summary>
        private List<GameObject> npcs = new List<GameObject>();

        /// <summary>
        /// NPC预设
        /// </summary>
        public GameObject npcPrefab;

        /// <summary>
        /// NPC数量
        /// </summary>
        public int npcCount = 5;

        #endregion

        #region HexSphere星球系统

        /// <summary>
        /// 星球根对象
        /// </summary>
        private GameObject planetRoot;

        /// <summary>
        /// HexPlanet控制器
        /// </summary>
        private HexPlanetController hexPlanetController;

        #endregion

        #region 地形系统

        /// <summary>
        /// 地形尺寸 - 256x256
        /// </summary>
        private const int TERRAIN_SIZE = 256;

        /// <summary>
        /// 当前地图ID
        /// </summary>
        public int currentMapTileId { get; private set; }

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // 确保SpriteSpacePrefab已初始化
            if (!SpriteSpace.SpriteSpacePrefab.initialized)
            {
                SpriteSpace.SpriteSpacePrefab.Init();
            }
        }

        private void Update()
        {
            // ESC键暂停/继续
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            // M键切换地图编辑器
            if (Input.GetKeyDown(KeyCode.M) && currentState == GameState.Playing)
            {
                ToggleMapEditor();
            }
        }

        #endregion

        #region 游戏流程控制

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame(PlanetPreset preset)
        {
            Debug.Log($"[GameManager] 开始游戏，星球类型: {preset.displayName}");
            currentPlanetPreset = preset;
            currentState = GameState.Loading;

            StartCoroutine(InitializeHexSphere(preset));
        }

        /// <summary>
        /// 初始化HexSphere星球
        /// </summary>
        private IEnumerator InitializeHexSphere(PlanetPreset preset)
        {
            Debug.Log("[GameManager] 初始化HexSphere星球...");

            // 隐藏开局菜单
            HideStartMenu();

            // 更新加载进度
            GameStartMenu menu = FindObjectOfType<GameStartMenu>();
            if (menu != null)
            {
                menu.UpdateLoadingProgress(10f, "正在创建星球...");
            }
            yield return new WaitForSeconds(0.5f);

            // 创建星球根对象
            planetRoot = new GameObject("HexPlanetRoot");
            planetRoot.transform.position = Vector3.zero;

            // 添加HexPlanetController
            hexPlanetController = planetRoot.AddComponent<HexPlanetController>();

            // 根据星球预设配置材质
            ConfigurePlanetMaterial(preset);

            if (menu != null)
            {
                menu.UpdateLoadingProgress(30f, "星球创建完成! 请点击地图区域...");
            }
            yield return new WaitForSeconds(0.5f);

            // 等待玩家点击星球区域
            currentState = GameState.PlanetSelect;
            Debug.Log("[GameManager] 进入星球选择模式，请在星球上点击一个区域...");
        }

        /// <summary>
        /// 根据星球预设配置材质
        /// </summary>
        private void ConfigurePlanetMaterial(PlanetPreset preset)
        {
            if (hexPlanetController == null) return;

            Material mat = new Material(Shader.Find("Standard"));

            switch (preset.id)
            {
                case "EarthLike":
                    mat.color = new Color(0.3f, 0.6f, 0.3f); // 绿色
                    break;
                case "Desert":
                    mat.color = new Color(0.8f, 0.6f, 0.3f); // 沙色
                    break;
                case "Ice":
                    mat.color = new Color(0.7f, 0.8f, 0.9f); // 冰蓝
                    break;
                case "Volcanic":
                    mat.color = new Color(0.5f, 0.2f, 0.1f); // 火红
                    break;
                default:
                    mat.color = new Color(0.4f, 0.6f, 0.3f);
                    break;
            }

            hexPlanetController.SetPlanetMaterial(mat);
        }

        /// <summary>
        /// HexPlanet区域被选中后继续游戏初始化
        /// </summary>
        public void OnPlanetAreaSelected(int tileId)
        {
            if (currentState == GameState.PlanetSelect)
            {
                Debug.Log($"[GameManager] 玩家已选择区域: {tileId}，开始初始化游戏...");
                currentMapTileId = tileId;
                StartCoroutine(ContinueGameInitialization(tileId));
            }
        }

        /// <summary>
        /// 继续游戏初始化（在选择星球区域后）
        /// </summary>
        private IEnumerator ContinueGameInitialization(int tileId)
        {
            currentState = GameState.Loading;

            // 更新加载进度
            GameStartMenu menu = FindObjectOfType<GameStartMenu>();
            if (menu != null)
            {
                menu.UpdateLoadingProgress(40f, "正在初始化CellSpace...");
            }
            yield return new WaitForSeconds(0.3f);

            // 确保CellSpacePrefab已初始化
            if (!CellSpace.CellSpacePrefab.initialized)
            {
                CellSpace.CellSpacePrefab.Init();
            }
            CPEngine.Active();

            if (menu != null)
            {
                menu.UpdateLoadingProgress(60f, "正在创建256x256地形...");
            }
            yield return new WaitForSeconds(0.3f);

            // 2. 创建地形（使用MapIndex）
            yield return StartCoroutine(CreateTerrainWithMapIndex(tileId));

            if (menu != null)
            {
                menu.UpdateLoadingProgress(75f, "正在创建玩家...");
            }
            yield return new WaitForSeconds(0.3f);

            // 3. 创建玩家
            yield return StartCoroutine(CreatePlayer(tileId));

            if (menu != null)
            {
                menu.UpdateLoadingProgress(90f, "正在生成NPC...");
            }
            yield return new WaitForSeconds(0.3f);

            // 4. 生成NPC
            yield return StartCoroutine(CreateNPCs(tileId));

            if (menu != null)
            {
                menu.UpdateLoadingProgress(100f, "游戏初始化完成!");
            }
            yield return new WaitForSeconds(0.5f);

            // 隐藏加载面板
            if (menu != null)
            {
                menu.HideStartMenu();
            }

            // 隐藏HexSphere星球（进入地面模式）
            if (planetRoot != null)
            {
                planetRoot.SetActive(false);
            }

            // 禁用HexPlanetController的旋转
            if (hexPlanetController != null)
            {
                hexPlanetController.SetCanRotate(false);
            }

            // 5. 初始化RimWorld系统
            InitializeRimWorldSystems();

            // 6. 初始化完成
            currentState = GameState.Playing;
            Debug.Log("[GameManager] 游戏初始化完成!");
        }

        /// <summary>
        /// 初始化RimWorld系统
        /// </summary>
        private void InitializeRimWorldSystems()
        {
            Debug.Log("[GameManager] 初始化RimWorld系统...");

            // 创建RimWorld游戏集成器
            GameObject integratorObj = new GameObject("RimWorldGameIntegrator");
            rimWorldIntegrator = integratorObj.AddComponent<RimWorldGameIntegrator>();

            // 初始化并开始RimWorld游戏
            rimWorldIntegrator.StartGame();

            Debug.Log("[GameManager] RimWorld系统初始化完成!");
        }

        /// <summary>
        /// 使用MapIndex创建地形
        /// </summary>
        private IEnumerator CreateTerrainWithMapIndex(int tileId)
        {
            Debug.Log("[GameManager] 使用MapIndex创建地形...");

            // 确保MapIndex存在
            MapIndex mapIndex = FindObjectOfType<MapIndex>();
            if (mapIndex == null)
            {
                GameObject mapIndexObj = new GameObject("MapIndex");
                mapIndex = mapIndexObj.AddComponent<MapIndex>();
            }

            Debug.Log($"[GameManager] 创建地图 {tileId}: {TERRAIN_SIZE}x{TERRAIN_SIZE}");
            yield return StartCoroutine(mapIndex.CreateMap(tileId, TERRAIN_SIZE, TERRAIN_SIZE));
            mapIndex.SwitchToMap(tileId);

            Debug.Log("[GameManager] 地形创建完成!");
        }

        /// <summary>
        /// 创建玩家
        /// </summary>
        private IEnumerator CreatePlayer(int tileId)
        {
            Debug.Log("[GameManager] 创建玩家...");

            // 创建玩家GameObject
            player = new GameObject("Player");

            // 添加简单玩家控制器
            playerController = player.AddComponent<SimplePlayerController>();

            // 设置玩家位置到地形中心
            Vector3 playerPos = new Vector3(TERRAIN_SIZE / 2f, 10f, TERRAIN_SIZE / 2f);
            player.transform.position = playerPos;

            // 添加一个简单的可视化组件（胶囊体）
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "PlayerBody";
            body.transform.SetParent(player.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

            // 设置摄像机跟随
            if (SpriteSpace.SpriteSpacePrefab.MainCamera != null)
            {
                Camera cam = SpriteSpace.SpriteSpacePrefab.MainCamera.GetComponent<Camera>();
                if (cam != null)
                {
                    // 简单的摄像机跟随
                    CameraFollow cameraFollow = cam.gameObject.AddComponent<CameraFollow>();
                    cameraFollow.target = player.transform;
                    cameraFollow.offset = new Vector3(0, 10, -15);
                }
            }

            Debug.Log("[GameManager] 玩家创建完成!");
            yield return null;
        }

        /// <summary>
        /// 创建NPC
        /// </summary>
        private IEnumerator CreateNPCs(int tileId)
        {
            Debug.Log($"[GameManager] 创建 {npcCount} 个NPC...");

            for (int i = 0; i < npcCount; i++)
            {
                CreateNPC(i, tileId);
                yield return null;
            }

            Debug.Log("[GameManager] NPC创建完成!");
        }

        /// <summary>
        /// 创建单个NPC
        /// </summary>
        private void CreateNPC(int index, int tileId)
        {
            GameObject npc;

            if (npcPrefab != null)
            {
                npc = Instantiate(npcPrefab);
            }
            else
            {
                // 创建默认NPC（类似环世界的小人）
                npc = CreateDefaultNPC($"NPC_{index}");
            }

            // 随机位置
            float x = Random.Range(10f, TERRAIN_SIZE - 10f);
            float z = Random.Range(10f, TERRAIN_SIZE - 10f);
            npc.transform.position = new Vector3(x, 0.5f, z);

            // 随机颜色（区分不同NPC）
            Renderer renderer = npc.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(
                    Random.Range(0.3f, 1f),
                    Random.Range(0.3f, 1f),
                    Random.Range(0.3f, 1f)
                );
            }

            npcs.Add(npc);
            Debug.Log($"[GameManager] 创建NPC: {npc.name}");
        }

        /// <summary>
        /// 创建默认NPC（环世界风格小人）
        /// </summary>
        private GameObject CreateDefaultNPC(string name)
        {
            GameObject npc = new GameObject(name);

            // 身体（胶囊形状）
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(npc.transform);
            body.transform.localPosition = new Vector3(0, 0.5f, 0);
            body.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);

            // 头部
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(npc.transform);
            head.transform.localPosition = new Vector3(0, 1.1f, 0);
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            // 腿（两个立方体）
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(npc.transform);
            leftLeg.transform.localPosition = new Vector3(-0.1f, 0.1f, 0);
            leftLeg.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);

            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(npc.transform);
            rightLeg.transform.localPosition = new Vector3(0.1f, 0.1f, 0);
            rightLeg.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);

            // 添加NPC移动组件
            npc.AddComponent<NPCController>();

            return npc;
        }

        #endregion

        #region UI控制

        /// <summary>
        /// 显示开局菜单
        /// </summary>
        public void ShowStartMenu()
        {
            // GameStartMenu由MMWorldInitializer确保存在，直接获取
            GameStartMenu menu = FindObjectOfType<GameStartMenu>();
            if (menu != null)
            {
                menu.ShowStartMenu();
                currentState = GameState.StartMenu;
            }
        }

        /// <summary>
        /// 隐藏开局菜单
        /// </summary>
        public void HideStartMenu()
        {
            GameStartMenu menu = FindObjectOfType<GameStartMenu>();
            if (menu != null)
            {
                menu.HideStartMenu();
            }
        }

        /// <summary>
        /// 切换暂停状态
        /// </summary>
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                Time.timeScale = 0;
                Debug.Log("[GameManager] 游戏暂停");
            }
            else if (currentState == GameState.Paused)
            {
                currentState = GameState.Playing;
                Time.timeScale = 1;
                Debug.Log("[GameManager] 游戏继续");
            }
        }

        /// <summary>
        /// 切换地图编辑器
        /// </summary>
        public void ToggleMapEditor()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.MapEditing;
                Debug.Log("[GameManager] 进入地图编辑模式");
            }
            else if (currentState == GameState.MapEditing)
            {
                currentState = GameState.Playing;
                Debug.Log("[GameManager] 退出地图编辑模式");
            }
        }

        #endregion

        #region 存档功能

        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame()
        {
            Debug.Log("[GameManager] 保存游戏...");
            // TODO: 实现存档系统
        }

        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame()
        {
            Debug.Log("[GameManager] 加载游戏...");
            // TODO: 实现读档系统
        }

        #endregion
    }

    /// <summary>
    /// 简单玩家控制器 - 处理WASD移动
    /// </summary>
    public class SimplePlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;

        private void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;
            transform.position += movement * moveSpeed * Time.deltaTime;

            // 边界检测
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, 1f, 254f),
                transform.position.y,
                Mathf.Clamp(transform.position.z, 1f, 254f)
            );
        }
    }

    /// <summary>
    /// 摄像机跟随脚本
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 10, -15);
        public float smoothSpeed = 5f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.LookAt(target);
        }
    }

    /// <summary>
    /// NPC控制器 - 简单的随机移动AI
    /// </summary>
    public class NPCController : MonoBehaviour
    {
        public float moveSpeed = 2f;
        public float changeDirectionInterval = 3f;

        private Vector3 moveDirection;
        private float nextDirectionChange;

        private void Start()
        {
            ChangeDirection();
        }

        private void Update()
        {
            // 随机改变方向
            if (Time.time >= nextDirectionChange)
            {
                ChangeDirection();
            }

            // 移动
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // 边界检测（保持在地形范围内）
            ClampToTerrain();
        }

        private void ChangeDirection()
        {
            float angle = Random.Range(0f, 360f);
            moveDirection = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;
            nextDirectionChange = Time.time + changeDirectionInterval;
        }

        private void ClampToTerrain()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, 1f, 254f);
            pos.z = Mathf.Clamp(pos.z, 1f, 254f);
            transform.position = pos;
        }
    }
}