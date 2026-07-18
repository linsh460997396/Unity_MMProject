using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMWorld.HexSphere;

namespace MMWorld
{
    /// <summary>
    /// HexPlanet星球控制器 - 管理星球的交互、旋转和区域选择
    /// </summary>
    public class HexPlanetController : MonoBehaviour
    {
        #region 字段

        /// <summary>
        /// 单例实例
        /// </summary>
        public static HexPlanetController Instance { get; private set; }

        /// <summary>
        /// HexPlanet管理器组件
        /// </summary>
        private HexPlanetManager planetManager;

        /// <summary>
        /// HexPlanet数据
        /// </summary>
        private HexPlanet hexPlanet;

        /// <summary>
        /// MapIndex地图索引管理器
        /// </summary>
        private MapIndex mapIndex;

        /// <summary>
        /// 射线检测层掩码
        /// </summary>
        private int raycastMask;

        /// <summary>
        /// 当前选中的Tile
        /// </summary>
        private HexTile selectedTile;

        /// <summary>
        /// 是否可以旋转
        /// </summary>
        private bool canRotate = true;

        /// <summary>
        /// 上次鼠标位置
        /// </summary>
        private Vector3 lastMousePosition;

        /// <summary>
        /// 旋转速度
        /// </summary>
        public float rotationSpeed = 5f;

        /// <summary>
        /// 是否正在拖拽
        /// </summary>
        private bool isDragging = false;

        /// <summary>
        /// 点击选中Tile的事件
        /// </summary>
        public System.Action<HexTile> onTileSelected;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            raycastMask = LayerMask.GetMask("HexPlanet");

            InitializePlanet();
        }

        private void Update()
        {
            HandleMouseInput();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化星球
        /// </summary>
        private void InitializePlanet()
        {
            planetManager = gameObject.GetComponent<HexPlanetManager>();
            if (planetManager == null)
            {
                planetManager = gameObject.AddComponent<HexPlanetManager>();
            }

            CreateHexPlanet();

            planetManager.UpdateRenderObjects();

            Debug.Log("[HexPlanetController] 星球初始化完成!");
        }

        /// <summary>
        /// 创建HexPlanet数据
        /// </summary>
        private void CreateHexPlanet()
        {
            hexPlanet = new HexPlanet();

            hexPlanet.radius = 50f;
            hexPlanet.subdivisions = 3;
            hexPlanet.chunkSubdivisions = 2;

            if (hexPlanet.chunkMaterial == null)
            {
                hexPlanet.chunkMaterial = new Material(Shader.Find("Standard"));
                hexPlanet.chunkMaterial.color = new Color(0.4f, 0.6f, 0.3f);
            }

            if (hexPlanet.terrainGenerator == null)
            {
                hexPlanet.terrainGenerator = CreateRandomTerrainGenerator();
            }

            planetManager.hexPlanet = hexPlanet;

            Debug.Log($"[HexPlanetController] 创建星球: radius={hexPlanet.radius}, subdivisions={hexPlanet.subdivisions}");
        }

        /// <summary>
        /// 创建随机地形生成器
        /// </summary>
        private BaseTerrainGenerator CreateRandomTerrainGenerator()
        {
            RandomTerrainGenerator gen = new RandomTerrainGenerator();
            gen.minHeight = 0f;
            gen.maxHeight = 5f;
            gen.colors = new List<Color32>
            {
                new Color32(60, 140, 60, 255),
                new Color32(100, 80, 50, 255),
                new Color32(120, 160, 120, 255)
            };
            return gen;
        }

        #endregion

        #region 鼠标输入处理

        /// <summary>
        /// 处理鼠标输入
        /// </summary>
        private void HandleMouseInput()
        {
            if (!canRotate) return;

            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
                isDragging = false;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;

                if (delta.magnitude > 5f)
                {
                    isDragging = true;

                    float rotX = delta.y * rotationSpeed * Time.deltaTime;
                    float rotY = -delta.x * rotationSpeed * Time.deltaTime;

                    transform.Rotate(Vector3.right, rotX, Space.World);
                    transform.Rotate(Vector3.up, rotY, Space.World);
                }

                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!isDragging)
                {
                    TrySelectTile();
                }
                isDragging = false;
            }
        }

        /// <summary>
        /// 尝试选中Tile
        /// </summary>
        private void TrySelectTile()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 10000f, raycastMask))
            {
                HexChunkRenderer hcr = hit.collider.GetComponent<HexChunkRenderer>();
                if (hcr != null)
                {
                    HexChunk hc = hcr.GetHexChunk();
                    if (hc != null)
                    {
                        Vector3 localHit = hit.point - hcr.transform.position;
                        HexTile tile = hc.GetClosestTileAngle(localHit);

                        if (tile != null)
                        {
                            SelectTile(tile);
                        }
                    }
                }
            }
        }

        #endregion

        #region Tile选择

        /// <summary>
        /// 选中Tile
        /// </summary>
        private void SelectTile(HexTile tile)
        {
            if (selectedTile == tile) return;

            selectedTile = tile;
            Debug.Log($"[HexPlanetController] 选中Tile: ID={tile.id}, Height={tile.height:F2}");

            onTileSelected?.Invoke(tile);

            HandleTileSelection(tile);
        }

        /// <summary>
        /// 处理Tile选中后的逻辑
        /// </summary>
        private void HandleTileSelection(HexTile tile)
        {
            int tileId = tile.id;

            if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.PlanetSelect)
            {
                Debug.Log($"[HexPlanetController] 星球区域已选择: Tile {tileId},通知GameManager继续初始化...");
                GameManager.Instance.OnPlanetAreaSelected(tileId);
                return;
            }

            if (mapIndex == null)
            {
                mapIndex = FindObjectOfType<MapIndex>();
            }

            if (mapIndex != null)
            {
                if (mapIndex.HasMap(tileId))
                {
                    Debug.Log($"[HexPlanetController] 地图 {tileId} 已存在,切换至该地图");
                    mapIndex.SwitchToMap(tileId);
                }
                else
                {
                    Debug.Log($"[HexPlanetController] 地图 {tileId} 不存在,创建新地图");
                    StartCoroutine(CreateNewMap(tileId));
                }
            }
        }

        /// <summary>
        /// 创建新地图
        /// </summary>
        private IEnumerator CreateNewMap(int tileId)
        {
            Debug.Log($"[HexPlanetController] 开始创建地图 {tileId}...");

            GameStartMenu menu = FindObjectOfType<GameStartMenu>();
            if (menu != null)
            {
                menu.UpdateLoadingProgress(0, $"正在创建地图 {tileId}...");
            }

            yield return null;

            yield return StartCoroutine(mapIndex.CreateMap(tileId, 256, 256));

            Debug.Log($"[HexPlanetController] 地图 {tileId} 创建完成!");

            mapIndex.SwitchToMap(tileId);
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 设置是否可旋转
        /// </summary>
        public void SetCanRotate(bool canRotate)
        {
            this.canRotate = canRotate;
        }

        /// <summary>
        /// 获取当前选中的Tile
        /// </summary>
        public HexTile GetSelectedTile()
        {
            return selectedTile;
        }

        /// <summary>
        /// 获取HexPlanet
        /// </summary>
        public HexPlanet GetHexPlanet()
        {
            return hexPlanet;
        }

        /// <summary>
        /// 设置星球材质
        /// </summary>
        public void SetPlanetMaterial(Material mat)
        {
            if (hexPlanet != null)
            {
                hexPlanet.chunkMaterial = mat;
                planetManager.UpdateRenderObjects();
            }
        }

        /// <summary>
        /// 刷新星球渲染
        /// </summary>
        public void RefreshPlanet()
        {
            if (planetManager != null)
            {
                planetManager.UpdateRenderObjects();
            }
        }

        #endregion
    }
}