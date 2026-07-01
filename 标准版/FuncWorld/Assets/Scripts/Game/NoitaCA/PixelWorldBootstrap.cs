// 职责：在普通演示场景中创建像素世界、模拟器、渲染器、输入控制和演示地形。
// Responsibility: Builds the default demo scene with the pixel world, simulator, renderer, input controller, and terrain.
using UnityEngine;

namespace NoitaCA
{
    [DefaultExecutionOrder(-100)]
    public sealed class PixelWorldBootstrap : MonoBehaviour
    {
        // 世界尺寸、显示比例和模拟节奏都可在 Inspector 中调试。
        // World size, display scale, and simulation cadence are tunable in the Inspector.
        [Header("World")]
        [SerializeField] private int worldWidth = 256;
        [SerializeField] private int worldHeight = 144;
        [SerializeField] private int pixelsPerUnit = 16;
        [SerializeField] private int simulationStepsPerFrame = 1;
        [SerializeField] private float simulationStepInterval = 0.035f;
        [SerializeField] private float cameraPadding = 0.5f;

        [Header("Demo")]
        [SerializeField] private bool buildDemoTerrain = true;
        [SerializeField] private bool buildDemoPlayer = true;
        [SerializeField] private int bottomlessHoleCenterX = -1;
        [SerializeField] private int bottomlessHoleWidth = 10;

        private PixelGrid grid;
        private PixelSimulation simulation;
        private PixelWorldRenderer worldRenderer;
        private InputController inputController;
        private SimplePixelPlayer player;
        private Camera targetCamera;
        private float simulationAccumulator;
        private Vector2Int playerSpawnCell;

        private void Awake()
        {
            // Awake 中完成世界搭建，确保其他组件 Update 前已有可用引用。
            // Build in Awake so references are ready before other components update.
            BuildWorld();
        }

        private void Update()
        {
            if (grid == null || simulation == null || worldRenderer == null || inputController == null)
            {
                return;
            }

            inputController.Tick();

            // 使用累积器按固定间隔推进模拟，避免帧率变化直接改变物理速度。
            // Use an accumulator to step simulation at a fixed interval independent of frame rate.
            simulationAccumulator += Time.deltaTime;
            float safeStepInterval = Mathf.Max(0.001f, simulationStepInterval);
            int simulatedTicks = 0;

            while (simulationAccumulator >= safeStepInterval && simulatedTicks < 4)
            {
                // 每个固定 tick 可执行多步，用于加快像素世界演化。
                // Each fixed tick may run multiple steps to speed up world evolution.
                int steps = Mathf.Max(1, simulationStepsPerFrame);
                for (int i = 0; i < steps; i++)
                {
                    simulation.Step(grid);
                }

                simulationAccumulator -= safeStepInterval;
                simulatedTicks++;
            }

            if (simulatedTicks >= 4)
            {
                // 帧率过低时丢弃积压，防止单帧补太多步导致卡死。
                // Drop backlog on slow frames to avoid huge catch-up spikes.
                simulationAccumulator = 0f;
            }

            // 模拟后把最新网格颜色刷新到贴图。
            // After simulation, refresh the texture with the latest grid colors.
            worldRenderer.Render();
        }

        private void BuildWorld()
        {
            // 创建核心数据与模拟器，然后按需填充演示地形。
            // Create core data and simulator, then optionally fill demo terrain.
            grid = new PixelGrid(worldWidth, worldHeight);
            simulation = new PixelSimulation();

            if (buildDemoTerrain)
            {
                BuildDemoTerrain();
            }

            targetCamera = GetOrCreateCamera();
            worldRenderer = GetOrCreateRenderer();
            inputController = GetOrCreateInputController();

            // 先摆放显示对象，再初始化渲染器和相机，保证坐标换算正确。
            // Position the display before initializing renderer/camera so coordinate conversion is correct.
            ConfigureDisplayTransform();
            worldRenderer.Initialize(grid, pixelsPerUnit);
            ConfigureCamera();
            inputController.Initialize(grid, worldRenderer, targetCamera);

            if (buildDemoPlayer)
            {
                // 玩家出生点由地形生成阶段写入。
                // The player spawn cell is written during terrain generation.
                player = GetOrCreatePlayer();
                player.Initialize(grid, worldRenderer, playerSpawnCell);
            }

            worldRenderer.Render();
        }

        private PixelWorldRenderer GetOrCreateRenderer()
        {
            // 查找或创建显示子物体，避免重复添加渲染节点。
            // Find or create the display child object to avoid duplicate render nodes.
            Transform existingDisplay = transform.Find("Pixel World Display");
            GameObject displayObject;

            if (existingDisplay == null)
            {
                displayObject = new GameObject("Pixel World Display");
                displayObject.transform.SetParent(transform, false);
            }
            else
            {
                displayObject = existingDisplay.gameObject;
            }

            if (!displayObject.TryGetComponent(out SpriteRenderer _))
            {
                displayObject.AddComponent<SpriteRenderer>();
            }

            if (!displayObject.TryGetComponent(out PixelWorldRenderer renderer))
            {
                renderer = displayObject.AddComponent<PixelWorldRenderer>();
            }

            return renderer;
        }

        private InputController GetOrCreateInputController()
        {
            // 输入控制器挂在启动器同一对象上，方便场景配置。
            // The input controller lives on the bootstrap object for easy scene setup.
            if (!TryGetComponent(out InputController controller))
            {
                controller = gameObject.AddComponent<InputController>();
            }

            return controller;
        }

        private SimplePixelPlayer GetOrCreatePlayer()
        {
            // 玩家作为子物体存在，重建世界时可复用已有组件。
            // The player is a child object so world rebuilds can reuse its component.
            Transform existingPlayer = transform.Find("Demo Player");
            GameObject playerObject;

            if (existingPlayer == null)
            {
                playerObject = new GameObject("Demo Player");
                playerObject.transform.SetParent(transform, false);
            }
            else
            {
                playerObject = existingPlayer.gameObject;
            }

            if (!playerObject.TryGetComponent(out SpriteRenderer _))
            {
                playerObject.AddComponent<SpriteRenderer>();
            }

            if (!playerObject.TryGetComponent(out SimplePixelPlayer playerController))
            {
                playerController = playerObject.AddComponent<SimplePixelPlayer>();
            }

            return playerController;
        }

        private Camera GetOrCreateCamera()
        {
            // 优先使用主相机；没有相机时才创建默认相机。
            // Prefer the main camera; create a default camera only if none exists.
            Camera camera = Camera.main;
            if (camera != null)
            {
                return camera;
            }

            camera = Object.FindObjectOfType<Camera>();
            if (camera != null)
            {
                return camera;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private void ConfigureDisplayTransform()
        {
            // Sprite 原点在左下角，因此把显示对象移动到世界中心对齐原点。
            // The sprite origin is bottom-left, so move the display object to center the world around origin.
            Vector2 worldSize = new Vector2(worldWidth / (float)pixelsPerUnit, worldHeight / (float)pixelsPerUnit);
            worldRenderer.transform.position = new Vector3(-worldSize.x * 0.5f, -worldSize.y * 0.5f, 0f);
            worldRenderer.transform.rotation = Quaternion.identity;
            worldRenderer.transform.localScale = Vector3.one;
        }

        private void ConfigureCamera()
        {
            // 正交相机完整框住世界高度，并留一点边距。
            // The orthographic camera frames the full world height with a small padding.
            Vector2 worldSize = new Vector2(worldWidth / (float)pixelsPerUnit, worldHeight / (float)pixelsPerUnit);
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = worldSize.y * 0.5f + Mathf.Max(0f, cameraPadding);
            targetCamera.transform.position = new Vector3(0f, 0f, -10f);
            targetCamera.transform.rotation = Quaternion.identity;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = new Color(0.015f, 0.018f, 0.025f, 1f);
        }

        private void BuildDemoTerrain()
        {
            // 先生成基础石质地形高度，再雕刻空间并播撒材料。
            // First generate stone terrain heights, then carve spaces and seed materials.
            int[] surfaceHeights = new int[worldWidth];

            for (int x = 0; x < worldWidth; x++)
            {
                surfaceHeights[x] = GetTerrainHeight(x);

                for (int y = 0; y < surfaceHeights[x]; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Stone);
                }
            }

            CarveArena(surfaceHeights);
            CarveBottomlessHole();
            SeedWater(surfaceHeights);
            SeedSand(surfaceHeights);
            BuildWoodenChainReaction(surfaceHeights);
            SeedSmokePocket(surfaceHeights);
            playerSpawnCell = new Vector2Int(Mathf.Clamp(worldWidth / 7, 4, worldWidth - 5), Mathf.Min(worldHeight - 10, surfaceHeights[worldWidth / 7] + 12));
        }

        private int GetTerrainHeight(int x)
        {
            // 多个波形和高斯形状叠加，形成可玩的起伏地形。
            // Multiple waves and Gaussian shapes combine into playable uneven terrain.
            float t = worldWidth <= 1 ? 0f : x / (float)(worldWidth - 1);
            float descendingSlope = Mathf.Lerp(worldHeight * 0.42f, worldHeight * 0.2f, t);
            float longWave = Mathf.Sin(t * Mathf.PI * 3.4f) * 7f;
            float shortWave = Mathf.Sin(t * Mathf.PI * 12.5f + 0.7f) * 3f;
            float centerBasin = -16f * Mathf.Exp(-Mathf.Pow((t - 0.58f) / 0.13f, 2f));
            float rightBank = 12f * Mathf.Exp(-Mathf.Pow((t - 0.76f) / 0.05f, 2f));
            float leftShelf = 8f * Mathf.Exp(-Mathf.Pow((t - 0.18f) / 0.08f, 2f));

            int minHeight = Mathf.Max(6, Mathf.RoundToInt(worldHeight * 0.08f));
            int maxHeight = Mathf.Max(minHeight + 1, Mathf.RoundToInt(worldHeight * 0.68f));
            return Mathf.Clamp(Mathf.RoundToInt(descendingSlope + longWave + shortWave + centerBasin + rightBank + leftShelf), minHeight, maxHeight);
        }

        private void CarveArena(int[] surfaceHeights)
        {
            // 沿地表挖出活动空间，中部额外加深形成盆地。
            // Carve playable space along the surface, with a deeper middle basin.
            for (int x = 0; x < worldWidth; x++)
            {
                float t = worldWidth <= 1 ? 0f : x / (float)(worldWidth - 1);
                int carveDepth = Mathf.RoundToInt(Mathf.Lerp(3f, 1f, t));

                if (t > 0.42f && t < 0.68f)
                {
                    carveDepth += 3;
                }

                for (int y = surfaceHeights[x] - carveDepth; y < surfaceHeights[x] + 18; y++)
                {
                    if (grid.InBounds(x, y))
                    {
                        grid.SetMaterial(x, y, MaterialType.Air);
                    }
                }
            }

            CarveRoom(worldWidth * 3 / 5, worldHeight / 2, 32, 18);
        }

        private void CarveRoom(int centerX, int centerY, int radiusX, int radiusY)
        {
            // 椭圆方程用于挖出地下房间。
            // Ellipse math carves an underground room.
            for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
            {
                for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
                {
                    float dx = (x - centerX) / (float)Mathf.Max(1, radiusX);
                    float dy = (y - centerY) / (float)Mathf.Max(1, radiusY);
                    if (dx * dx + dy * dy <= 1f)
                    {
                        grid.SetMaterial(x, y, MaterialType.Air);
                    }
                }
            }
        }

        private void CarveBottomlessHole()
        {
            // 无底洞用于演示材料流出世界后的边界行为。
            // The bottomless hole demonstrates boundary behavior when materials leave the world.
            int centerX = bottomlessHoleCenterX < 0 ? worldWidth / 2 : bottomlessHoleCenterX;
            int safeWidth = Mathf.Max(1, bottomlessHoleWidth);
            int left = Mathf.Clamp(centerX - safeWidth / 2, 0, worldWidth - 1);
            int right = Mathf.Clamp(left + safeWidth - 1, 0, worldWidth - 1);

            for (int x = left; x <= right; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Air);
                }
            }

            int bevelHeight = Mathf.Max(3, safeWidth / 2);
            for (int i = 1; i <= bevelHeight; i++)
            {
                ClearColumnBand(left - i, bevelHeight - i + 1);
                ClearColumnBand(right + i, bevelHeight - i + 1);
            }
        }

        private void ClearColumnBand(int x, int height)
        {
            if (!grid.InBounds(x, 0))
            {
                return;
            }

            // 清理底部列带，给无底洞边缘做斜切过渡。
            // Clear a bottom column band to bevel the bottomless-hole edge.
            int safeHeight = Mathf.Clamp(height, 0, worldHeight);
            for (int y = 0; y < safeHeight; y++)
            {
                grid.SetMaterial(x, y, MaterialType.Air);
            }
        }

        private void SeedWater(int[] surfaceHeights)
        {
            // 在左侧生成蓄水区，观察液体向低处流动。
            // Seed a left-side reservoir to observe water flowing downhill.
            int reservoirStart = Mathf.Max(2, worldWidth / 14);
            int reservoirEnd = Mathf.Min(worldWidth - 3, worldWidth / 4);

            for (int x = reservoirStart; x <= reservoirEnd; x++)
            {
                int top = Mathf.Min(worldHeight - 4, surfaceHeights[x] + 22);
                int bottom = Mathf.Min(worldHeight - 5, surfaceHeights[x] + 5);

                for (int y = bottom; y <= top; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Water);
                }
            }
        }

        private void SeedSand(int[] surfaceHeights)
        {
            // 三角形沙堆用于展示粉末下落和堆积。
            // A triangular sand pile demonstrates powder falling and piling.
            int center = Mathf.RoundToInt(worldWidth * 0.36f);
            for (int x = center - 14; x <= center + 14; x++)
            {
                if (x < 0 || x >= worldWidth)
                {
                    continue;
                }

                int pileHeight = Mathf.RoundToInt(15f * Mathf.Clamp01(1f - Mathf.Abs(x - center) / 14f));
                for (int y = surfaceHeights[x] + 1; y <= surfaceHeights[x] + pileHeight; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Sand);
                }
            }
        }

        private void BuildWoodenChainReaction(int[] surfaceHeights)
        {
            // 木结构和初始火点用于展示燃烧链式反应。
            // Wooden structures and an initial fire source demonstrate combustion chain reactions.
            int startX = Mathf.RoundToInt(worldWidth * 0.62f);
            int baseY = Mathf.Min(worldHeight - 20, surfaceHeights[startX] + 3);

            for (int x = startX; x < startX + 38 && x < worldWidth - 2; x++)
            {
                grid.SetMaterial(x, baseY, MaterialType.Wood);
                if (x % 6 == 0)
                {
                    for (int y = baseY - 9; y <= baseY; y++)
                    {
                        grid.SetMaterial(x, y, MaterialType.Wood);
                    }
                }
            }

            for (int y = baseY + 1; y < baseY + 12 && y < worldHeight - 2; y++)
            {
                grid.SetMaterial(startX + 34, y, MaterialType.Wood);
            }

            grid.PaintCircle(startX + 2, baseY + 2, 2, MaterialType.Fire);
        }

        private void SeedSmokePocket(int[] surfaceHeights)
        {
            // 烟雾口袋用于展示气体上浮和寿命淡出。
            // A smoke pocket demonstrates gas rising and lifetime fade-out.
            int centerX = Mathf.RoundToInt(worldWidth * 0.62f);
            int centerY = Mathf.Min(worldHeight - 8, surfaceHeights[centerX] + 30);

            for (int x = centerX - 8; x <= centerX + 8; x++)
            {
                for (int y = centerY - 5; y <= centerY + 5; y++)
                {
                    if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) < 52)
                    {
                        grid.SetMaterial(x, y, MaterialType.Smoke);
                    }
                }
            }
        }
    }
}
