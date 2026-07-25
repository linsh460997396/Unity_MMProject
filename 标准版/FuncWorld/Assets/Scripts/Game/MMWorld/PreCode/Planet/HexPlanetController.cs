using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MMWorld.HexSphere;

namespace MMWorld
{
    /// <summary>
    /// HexPlanet星球控制器 - 管理星球的交互、旋转、区域选择、高亮与缩放。
    /// 纯代码复刻 HexSphere 的 CameraOrbit + TileRaycast 功能，不依赖编辑器/prefab。
    /// </summary>
    public class HexPlanetController : MonoBehaviour
    {
        #region 字段

        /// <summary>单例实例</summary>
        public static HexPlanetController Instance { get; private set; }

        /// <summary>HexPlanet管理器组件</summary>
        private HexPlanetManager planetManager;

        /// <summary>HexPlanet数据</summary>
        private HexPlanet hexPlanet;

        /// <summary>MapIndex地图索引管理器</summary>
        private MapIndex mapIndex;

        /// <summary>射线检测层掩码</summary>
        private int raycastMask;

        /// <summary>当前主选中的Tile（外部通过 GetSelectedTile 访问）</summary>
        private HexTile selectedTile;

        /// <summary>是否可以旋转/缩放</summary>
        private bool canRotate = true;

        /// <summary>上次鼠标位置（拖拽计算用）</summary>
        private Vector3 lastMousePosition;

        /// <summary>旋转速度</summary>
        public float rotationSpeed = 5f;

        /// <summary>是否正在拖拽（区分点击与拖拽）</summary>
        private bool isDragging = false;

        /// <summary>点击选中Tile的事件</summary>
        public System.Action<HexTile> onTileSelected;

        // === 地形类型切换 ===
        public enum TerrainGeneratorType { Random, Perlin }

        [Header("星球参数")]
        [SerializeField] private TerrainGeneratorType terrainType = TerrainGeneratorType.Perlin;
        [SerializeField] private float planetRadius = 50f;
        [SerializeField] private int subdivisions = 3;
        [SerializeField] private int chunkSubdivisions = 2;
        [SerializeField] private Color basePlanetColor = new Color(0.4f, 0.6f, 0.3f);

        // === 高亮系统 ===
        private LineRenderer hoverHighlightLR;       // 悬停瓦片边框（黄色）
        private LineRenderer selectionHighlightLR;   // 选中瓦片边框（红色）
        private Material wireframeMaterial;          // 共享线框材质 Hidden/Internal-Colored
        private HexTile hoveredTile;                 // 当前悬停的 Tile
        private readonly Vector3[] lrPositionsBuffer = new Vector3[8]; // 预分配避免 GC

        // === 游标对象池 ===
        private readonly List<GameObject> cursorPool = new List<GameObject>();
        private Material cursorMaterial;             // 游标共享材质

        // === 多选系统 ===
        private readonly HashSet<HexTile> selectedTiles = new HashSet<HexTile>();

        // === 旋转四元数累积（修复万向锁）===
        private Quaternion targetRotation;
        private bool targetRotationInitialized;

        // === 缩放 ===
        private float targetOrbitRadius;
        private float currentOrbitRadius;
        public float minOrbitRadius = 70f;
        public float maxOrbitRadius = 300f;
        public float zoomSpeed = 15f;

        // === 平滑参数 ===
        public float rotationDamping = 8f;   // Slerp 插值系数
        public float zoomDamping = 6f;       // Lerp 插值系数

        // === 缓存 ===
        private Camera cachedCamera;
        private Transform cachedCameraTransform;

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
            // 悬停高亮：每帧执行（星球旋转时鼠标静止，悬停瓦片也会变）
            UpdateHoverHighlight();

            if (canRotate)
            {
                HandleDragRotation();
                HandleZoom();
            }

            HandleClick();
            UpdateSmoothTransform();
        }

        private void OnDestroy()
        {
            if (wireframeMaterial != null) Destroy(wireframeMaterial);
            if (cursorMaterial != null) Destroy(cursorMaterial);
            foreach (GameObject c in cursorPool)
            {
                if (c != null) Destroy(c);
            }
            cursorPool.Clear();
            if (Instance == this) Instance = null;
        }

        #endregion

        #region 初始化

        /// <summary>初始化星球</summary>
        private void InitializePlanet()
        {
            planetManager = gameObject.GetComponent<HexPlanetManager>();
            if (planetManager == null)
            {
                planetManager = gameObject.AddComponent<HexPlanetManager>();
            }

            CreateHexPlanet();

            planetManager.UpdateRenderObjects();

            InitHighlight();
            InitCamera();

            targetRotation = transform.rotation;
            targetRotationInitialized = true;

            Debug.Log("[HexPlanetController] 星球初始化完成!");
        }

        /// <summary>创建HexPlanet数据</summary>
        private void CreateHexPlanet()
        {
            hexPlanet = new HexPlanet();

            hexPlanet.radius = planetRadius;
            hexPlanet.subdivisions = subdivisions;
            hexPlanet.chunkSubdivisions = chunkSubdivisions;

            if (hexPlanet.chunkMaterial == null)
            {
                hexPlanet.chunkMaterial = new Material(Shader.Find("Standard"));
                hexPlanet.chunkMaterial.color = basePlanetColor;
            }

            if (hexPlanet.terrainGenerator == null)
            {
                hexPlanet.terrainGenerator = terrainType switch
                {
                    TerrainGeneratorType.Perlin => CreatePerlinTerrainGenerator(),
                    _ => CreateRandomTerrainGenerator(),
                };
            }

            planetManager.hexPlanet = hexPlanet;

            Debug.Log($"[HexPlanetController] 创建星球: radius={hexPlanet.radius}, subdivisions={hexPlanet.subdivisions}, terrain={terrainType}");
        }

        /// <summary>创建随机地形生成器</summary>
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

        /// <summary>创建Perlin噪声地形生成器（海洋→沙滩→草地→山地→雪峰分层配色）</summary>
        private BaseTerrainGenerator CreatePerlinTerrainGenerator()
        {
            PerlinTerrainGenerator gen = new PerlinTerrainGenerator();
            gen.octaves = 4;
            gen.persistence = 0.5f;
            gen.lacunarity = 2f;
            gen.minHeight = 0f;
            gen.maxHeight = 5f;
            gen.noiseScaling = 1.5f;
            gen.colorHeights = new List<PerlinTerrainGenerator.ColorHeight>
            {
                new PerlinTerrainGenerator.ColorHeight { color = new Color32(40, 90, 160, 255), maxHeight = 0.5f },   // 深海
                new PerlinTerrainGenerator.ColorHeight { color = new Color32(80, 140, 200, 255), maxHeight = 1.5f },  // 浅海
                new PerlinTerrainGenerator.ColorHeight { color = new Color32(220, 200, 140, 255), maxHeight = 2.0f }, // 沙滩
                new PerlinTerrainGenerator.ColorHeight { color = new Color32(60, 140, 60, 255), maxHeight = 3.5f },   // 草地
                new PerlinTerrainGenerator.ColorHeight { color = new Color32(110, 90, 60, 255), maxHeight = 4.5f },   // 山地
                new PerlinTerrainGenerator.ColorHeight { color = new Color32(180, 180, 200, 255), maxHeight = 999f }, // 雪峰
            };
            return gen;
        }

        /// <summary>初始化高亮系统（一次性创建材质与LineRenderer）</summary>
        private void InitHighlight()
        {
            try
            {
                // 1. 共享线框材质
                wireframeMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                wireframeMaterial.hideFlags = HideFlags.HideAndDontSave;
                wireframeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                wireframeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                wireframeMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                wireframeMaterial.SetInt("_ZWrite", 0);

                // 2. 悬停/选中 LineRenderer
                hoverHighlightLR = CreateLineRenderer("HoverHighlight", new Color(1f, 1f, 0f, 0.9f), 0.15f);
                selectionHighlightLR = CreateLineRenderer("SelectionHighlight", new Color(1f, 0.2f, 0.2f, 0.95f), 0.2f);

                // 3. 游标共享材质
                cursorMaterial = new Material(Shader.Find("Standard"));
                cursorMaterial.color = Color.red;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HexPlanetController] InitHighlight 失败: {e}");
            }
        }

        /// <summary>创建LineRenderer辅助方法</summary>
        private LineRenderer CreateLineRenderer(string name, Color color, float width)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = true;
            lr.positionCount = 0;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.numCornerVertices = 0;
            lr.numCapVertices = 0;
            lr.sharedMaterial = wireframeMaterial;
            lr.startColor = color;
            lr.endColor = color;
            lr.enabled = false;
            return lr;
        }

        /// <summary>初始化相机（缓存引用，设置初始距离）</summary>
        private void InitCamera()
        {
            cachedCamera = Camera.main;
            if (cachedCamera != null)
            {
                cachedCameraTransform = cachedCamera.transform;
                float radius = hexPlanet != null ? hexPlanet.radius : 50f;
                currentOrbitRadius = targetOrbitRadius = Mathf.Max(radius * 2.5f, 130f);
                cachedCameraTransform.position = transform.position + Vector3.forward * currentOrbitRadius;
                cachedCameraTransform.LookAt(transform);
            }
            else
            {
                Debug.LogWarning("[HexPlanetController] Camera.main 为空，缩放功能不可用");
            }
        }

        #endregion

        #region 高亮系统

        /// <summary>每帧更新悬停瓦片的高亮边框</summary>
        private void UpdateHoverHighlight()
        {
            if (hoverHighlightLR == null || cachedCamera == null) return;

            Ray ray = cachedCamera.ScreenPointToRay(Input.mousePosition);
            HexTile tile = RaycastToTile(ray);

            if (tile == null)
            {
                if (hoverHighlightLR.enabled) hoverHighlightLR.enabled = false;
                hoveredTile = null;
                return;
            }

            if (hoveredTile != tile)
            {
                hoveredTile = tile;
                ApplyTileVerticesToLR(hoverHighlightLR, tile);
            }
            if (!hoverHighlightLR.enabled) hoverHighlightLR.enabled = true;
        }

        /// <summary>射线检测并返回命中的Tile</summary>
        private HexTile RaycastToTile(Ray ray)
        {
            if (!Physics.Raycast(ray, out RaycastHit hit, 10000f, raycastMask)) return null;
            HexChunkRenderer hcr = hit.collider.GetComponent<HexChunkRenderer>();
            if (hcr == null) return null;
            HexChunk hc = hcr.GetHexChunk();
            if (hc == null) return null;
            Vector3 localHit = hit.point - hcr.transform.position;
            return hc.GetClosestTileAngle(localHit);
        }

        /// <summary>把tile顶点（含高度外推）写入LineRenderer，使用预分配数组避免GC</summary>
        private void ApplyTileVerticesToLR(LineRenderer lr, HexTile tile)
        {
            int count = tile.vertices.Count;
            if (count > lrPositionsBuffer.Length) return;

            if (lr.positionCount != count)
            {
                lr.positionCount = count;
            }

            Vector3 heightOffset = tile.center.normalized * tile.height;
            Vector3 worldOffset = transform.position;
            for (int i = 0; i < count; i++)
            {
                lrPositionsBuffer[i] = tile.vertices[i] + heightOffset + worldOffset;
            }
            lr.SetPositions(lrPositionsBuffer);
        }

        #endregion

        #region 多选与选中

        /// <summary>处理点击选中（区分Shift多选/Ctrl切换/普通单选）</summary>
        private void HandleClick()
        {
            if (!Input.GetMouseButtonUp(0)) return;
            if (isDragging)
            {
                isDragging = false;
                return;
            }
            if (cachedCamera == null) return;

            Ray ray = cachedCamera.ScreenPointToRay(Input.mousePosition);
            HexTile tile = RaycastToTile(ray);
            if (tile == null) return;

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrl)
            {
                // Ctrl：已选则取消，未选则加入
                if (!selectedTiles.Remove(tile))
                {
                    selectedTiles.Add(tile);
                    selectedTile = tile;
                }
                else if (selectedTile == tile)
                {
                    selectedTile = null;
                }
            }
            else if (shift)
            {
                // Shift：追加
                selectedTiles.Add(tile);
                selectedTile = tile;
            }
            else
            {
                // 普通点击：早退优化（与原 SelectTile 行为一致）
                if (selectedTile == tile && selectedTiles.Count == 1) return;
                selectedTiles.Clear();
                selectedTiles.Add(tile);
                selectedTile = tile;
            }

            UpdateSelectionHighlight();
            Debug.Log($"[HexPlanetController] 选中Tile: ID={tile.id}, Height={tile.height:F2}, 共{selectedTiles.Count}个");

            onTileSelected?.Invoke(tile);
            HandleTileSelection(tile);
        }

        /// <summary>更新选中边框与游标显示</summary>
        private void UpdateSelectionHighlight()
        {
            // 选中边框（用主选中tile）
            if (selectionHighlightLR != null)
            {
                if (selectedTile == null)
                {
                    if (selectionHighlightLR.enabled) selectionHighlightLR.enabled = false;
                }
                else
                {
                    ApplyTileVerticesToLR(selectionHighlightLR, selectedTile);
                    if (!selectionHighlightLR.enabled) selectionHighlightLR.enabled = true;
                }
            }

            // 游标对象池
            EnsureCursorPoolCount(selectedTiles.Count);
            int i = 0;
            foreach (HexTile tile in selectedTiles)
            {
                GameObject cursor = cursorPool[i];
                cursor.transform.position = tile.center + tile.center.normalized * tile.height + transform.position;
                if (!cursor.activeSelf) cursor.SetActive(true);
                i++;
            }
            for (; i < cursorPool.Count; i++)
            {
                if (cursorPool[i].activeSelf) cursorPool[i].SetActive(false);
            }
        }

        /// <summary>按需扩容游标对象池</summary>
        private void EnsureCursorPoolCount(int need)
        {
            while (cursorPool.Count < need)
            {
                GameObject c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c.name = $"TileCursor_{cursorPool.Count}";
                c.transform.SetParent(transform, false);
                c.SetActive(false);
                Renderer r = c.GetComponent<Renderer>();
                r.sharedMaterial = cursorMaterial;
                cursorPool.Add(c);
            }
        }

        /// <summary>处理Tile选中后的游戏逻辑（联动GameManager/MapIndex）</summary>
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

        /// <summary>创建新地图</summary>
        private IEnumerator CreateNewMap(int tileId)
        {
            Debug.Log($"[HexPlanetController] 开始创建地图 {tileId}...");

            GameUI.UpdateLoadingProgress(0, $"正在创建地图 {tileId}...");

            yield return null;

            yield return StartCoroutine(mapIndex.CreateMap(tileId, 256, 256));

            Debug.Log($"[HexPlanetController] 地图 {tileId} 创建完成!");

            mapIndex.SwitchToMap(tileId);
        }

        #endregion

        #region 相机控制

        /// <summary>处理左键拖拽旋转（四元数累积，规避万向锁）</summary>
        private void HandleDragRotation()
        {
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

                    float rotX = delta.y * rotationSpeed * 0.1f;
                    float rotY = -delta.x * rotationSpeed * 0.1f;

                    // 四元数累积：绕世界轴旋转，避免欧拉角万向锁
                    Quaternion rotXQ = Quaternion.AngleAxis(rotX, Vector3.right);
                    Quaternion rotYQ = Quaternion.AngleAxis(rotY, Vector3.up);
                    targetRotation = rotXQ * rotYQ * targetRotation;
                }

                lastMousePosition = Input.mousePosition;
            }
        }

        /// <summary>处理滚轮缩放</summary>
        private void HandleZoom()
        {
            if (cachedCamera == null) return;

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 0.001f)
            {
                // 兜底：某些平台 mouseScrollDelta 为 0
                scroll = Input.GetAxis("Mouse ScrollWheel") * 10f;
            }
            if (Mathf.Abs(scroll) < 0.001f) return;

            targetOrbitRadius -= scroll * zoomSpeed;
            targetOrbitRadius = Mathf.Clamp(targetOrbitRadius, minOrbitRadius, maxOrbitRadius);
        }

        /// <summary>应用平滑插值（旋转Slerp + 缩放Lerp）</summary>
        private void UpdateSmoothTransform()
        {
            // 旋转：Slerp 平滑趋向目标
            if (targetRotationInitialized)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Mathf.Clamp01(rotationDamping * Time.deltaTime)
                );
            }

            // 缩放：Lerp 平滑相机距离
            if (cachedCameraTransform != null)
            {
                currentOrbitRadius = Mathf.Lerp(
                    currentOrbitRadius,
                    targetOrbitRadius,
                    Mathf.Clamp01(zoomDamping * Time.deltaTime)
                );
                Vector3 dir = cachedCameraTransform.position - transform.position;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
                else dir.Normalize();
                cachedCameraTransform.position = transform.position + dir * currentOrbitRadius;
                cachedCameraTransform.LookAt(transform);
            }
        }

        #endregion

        #region 公开方法

        /// <summary>设置是否可旋转/缩放</summary>
        public void SetCanRotate(bool canRotate)
        {
            this.canRotate = canRotate;
        }

        /// <summary>获取当前主选中的Tile</summary>
        public HexTile GetSelectedTile()
        {
            return selectedTile;
        }

        /// <summary>获取所有选中Tile（多选场景）</summary>
        public IReadOnlyCollection<HexTile> GetSelectedTiles()
        {
            return selectedTiles;
        }

        /// <summary>获取HexPlanet</summary>
        public HexPlanet GetHexPlanet()
        {
            return hexPlanet;
        }

        /// <summary>设置星球材质</summary>
        public void SetPlanetMaterial(Material mat)
        {
            if (hexPlanet != null)
            {
                hexPlanet.chunkMaterial = mat;
                planetManager.UpdateRenderObjects();
            }
        }

        /// <summary>刷新星球渲染</summary>
        public void RefreshPlanet()
        {
            if (planetManager != null)
            {
                planetManager.UpdateRenderObjects();
            }
        }

        /// <summary>设置当前主选中Tile的高度</summary>
        public void SetSelectedTileHeight(float newHeight)
        {
            if (selectedTile == null) return;
            try
            {
                selectedTile.SetHeight(newHeight);
                // SetHeight 已通过 onTileChange → chunk dirty → 自动重网格
                // 选中边框需立即更新（顶点高度变了）
                if (selectionHighlightLR != null && selectionHighlightLR.enabled)
                {
                    ApplyTileVerticesToLR(selectionHighlightLR, selectedTile);
                }
                UpdateCursorsPositions();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HexPlanetController] SetSelectedTileHeight 失败: {e}");
            }
        }

        /// <summary>设置所有多选Tile的高度</summary>
        public void SetSelectedTilesHeight(float newHeight)
        {
            if (selectedTiles == null || selectedTiles.Count == 0) return;
            try
            {
                foreach (HexTile tile in selectedTiles)
                {
                    tile.SetHeight(newHeight);
                }
                UpdateSelectionHighlight();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HexPlanetController] SetSelectedTilesHeight 失败: {e}");
            }
        }

        /// <summary>更新所有游标位置（高度变化后调用）</summary>
        private void UpdateCursorsPositions()
        {
            int i = 0;
            foreach (HexTile tile in selectedTiles)
            {
                if (i >= cursorPool.Count) break;
                cursorPool[i].transform.position = tile.center + tile.center.normalized * tile.height + transform.position;
                i++;
            }
        }

        /// <summary>切换地形生成器类型并重新生成整个星球</summary>
        public void SetTerrainType(TerrainGeneratorType type)
        {
            if (terrainType == type && hexPlanet != null && hexPlanet.terrainGenerator != null) return;
            terrainType = type;
            RebuildPlanet();
        }

        /// <summary>销毁并重建整个星球（数据 + 渲染）</summary>
        public void RebuildPlanet()
        {
            if (hexPlanet != null)
            {
                hexPlanet = null;
            }
            CreateHexPlanet();
            if (planetManager != null)
            {
                planetManager.hexPlanet = hexPlanet;
                planetManager.UpdateRenderObjects();
            }
            ClearSelection();
        }

        /// <summary>清空所有选中</summary>
        public void ClearSelection()
        {
            selectedTile = null;
            selectedTiles.Clear();
            if (selectionHighlightLR != null) selectionHighlightLR.enabled = false;
            foreach (var go in cursorPool)
            {
                if (go != null) go.SetActive(false);
            }
        }

        #endregion
    }
}
