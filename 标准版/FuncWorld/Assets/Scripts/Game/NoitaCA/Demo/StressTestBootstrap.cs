// 职责：构建并驱动压力测试场景，用大规模水体、区块优化和调试面板对比模拟性能。
// Responsibility: Builds and drives the stress-test scene, comparing simulation performance with large water volumes, chunk optimization, and debug panels.
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NoitaCA
{
    [DefaultExecutionOrder(-90)]
    public sealed class StressTestBootstrap : MonoBehaviour
    {
        // 所有压力测试参数集中在配置对象，方便 Inspector 调整。
        // All stress-test parameters live in one config object for Inspector tuning.
        [SerializeField] private StressTestConfig config = new StressTestConfig();

        // gateCells 记录可被开闸清空的石块位置。
        // gateCells stores stone cells that are cleared when the gate opens.
        private readonly List<Vector2Int> gateCells = new List<Vector2Int>(256);
        private readonly Stopwatch renderStopwatch = new Stopwatch();
        private PixelGrid grid;
        private PixelSimulation simulation;
        private PixelWorldRenderer worldRenderer;
        private SimplePixelPlayer player;
        private StressTestPerformancePanel performancePanel;
        private StressTestDebugOverlay debugOverlay;
        private Camera targetCamera;
        private float simulationAccumulator;
        private float elapsedSinceBuild;
        private float countAccumulator;
        private bool gateOpened;
        private Vector2Int playerSpawnCell;
        private RectInt tankInterior;

        public StressTestConfig Config => config;
        public PixelGrid Grid => grid;
        public PixelWorldRenderer Renderer => worldRenderer;
        public PixelSimulationStats Stats => simulation != null ? simulation.LastStats : default(PixelSimulationStats);
        public PixelSimulationMode Mode => simulation != null ? simulation.Mode : PixelSimulationMode.FullScan;
        public bool GateOpened => gateOpened;

        private void Awake()
        {
            // 场景启动时直接构建完整压力测试世界。
            // Build the complete stress-test world when the scene starts.
            BuildStressTestWorld();
        }

        private void Update()
        {
            if (grid == null || simulation == null || worldRenderer == null)
            {
                return;
            }

            HandleHotkeys();
            elapsedSinceBuild += Time.deltaTime;
            if (!gateOpened && config.autoStart && elapsedSinceBuild >= Mathf.Max(0f, config.gateOpenDelay))
            {
                // 自动开闸用于无人操作时也能稳定复现实验。
                // Auto-start opens the gate for repeatable unattended tests.
                OpenGate();
            }

            Simulate();
            RenderAndRecordTime();
            RefreshMaterialCounts();
        }

        public void BuildStressTestWorld()
        {
            // 压力测试强制最小尺寸，避免配置过小导致场景结构越界。
            // Stress test enforces a minimum size so scene structures stay in bounds.
            int width = Mathf.Max(64, config.gridWidth);
            int height = Mathf.Max(64, config.gridHeight);
            grid = new PixelGrid(width, height);
            // 根据配置重建区块缓存，便于比较不同 chunk size。
            // Rebuild chunk caches from config so different chunk sizes can be compared.
            grid.ConfigureOptimization(config.chunkSize, config.chunkSleepDelay);

            simulation = new PixelSimulation();
            ApplyMode(config.initialMode);
            simulation.MaxProcessedPixelsPerStep = Mathf.Max(0, config.maxSimulatedPixelsPerStep);

            // 依次建地形、水箱、可燃结构，再全量激活以保证第一步覆盖全场。
            // Build terrain, tank, and flammable structures, then activate all cells for the first step.
            BuildTerrain();
            BuildWaterTank();
            BuildWoodenStructures();
            grid.ActivateAll();

            targetCamera = GetOrCreateCamera();
            worldRenderer = GetOrCreateRenderer();
            // 显示、相机、玩家和调试面板都依赖已建好的网格。
            // Display, camera, player, and debug panels all depend on the built grid.
            ConfigureDisplayTransform();
            worldRenderer.Initialize(grid, config.pixelsPerUnit);
            ConfigureCamera();
            player = GetOrCreatePlayer();
            player.Initialize(grid, worldRenderer, playerSpawnCell);
            performancePanel = GetOrCreatePerformancePanel();
            performancePanel.Initialize(this);
            debugOverlay = GetOrCreateDebugOverlay();
            debugOverlay.Initialize(this, targetCamera);

            gateOpened = false;
            elapsedSinceBuild = 0f;
            simulationAccumulator = 0f;
            countAccumulator = 0f;
            RenderAndRecordTime();
            RefreshMaterialCounts(true);
        }

        public void OpenGate()
        {
            if (gateOpened)
            {
                return;
            }

            // 开闸就是把预先记录的闸门格子改成空气。
            // Opening the gate means turning the recorded gate cells into air.
            gateOpened = true;
            for (int i = 0; i < gateCells.Count; i++)
            {
                Vector2Int cell = gateCells[i];
                grid.SetMaterial(cell.x, cell.y, MaterialType.Air);
            }

            // 演示简化：闸门瞬间消失；正式游戏可改成动画并唤醒更大脏区。
            // Demo simplification: the gate is instant; a production game might animate and wake a wider dirty region.
            if (gateCells.Count > 0)
            {
                Vector2Int first = gateCells[0];
                grid.MarkActiveArea(first.x, first.y, Mathf.Max(8, grid.ChunkSize));
            }
        }

        public void InjectWaterFromTop()
        {
            if (grid == null)
            {
                return;
            }

            // 按住快捷键时从水箱上方持续补水，用于持续施压。
            // Holding the hotkey injects water from the top to keep pressure high.
            int left = Mathf.Clamp(tankInterior.xMin, 1, grid.Width - 2);
            int right = Mathf.Clamp(tankInterior.xMax, left + 1, grid.Width - 1);
            int top = Mathf.Clamp(grid.Height - 6, 1, grid.Height - 2);
            int bottom = Mathf.Max(1, top - 5);

            for (int y = bottom; y <= top; y++)
            {
                for (int x = left; x < right; x++)
                {
                    grid.SetMaterial(x, y, MaterialType.Water);
                }
            }
        }

        public void ApplyMode(PixelSimulationMode mode)
        {
            if (simulation == null)
            {
                return;
            }

            // 如果配置禁用了某类优化，运行时切换会回退到全图扫描。
            // If config disables an optimization, runtime mode switching falls back to full scan.
            if (mode == PixelSimulationMode.ActivePixels && !config.enableActiveRegionOptimization)
            {
                mode = PixelSimulationMode.FullScan;
            }
            else if (mode == PixelSimulationMode.ChunkBased && !config.enableChunkUpdates)
            {
                mode = PixelSimulationMode.FullScan;
            }

            simulation.Mode = mode;
            if (grid != null)
            {
                // 切换模式后全量激活，避免新模式从空活跃集开始。
                // After mode switching, activate all cells so the new mode does not start from an empty active set.
                grid.ActivateAll();
            }
        }

        private void Simulate()
        {
            // 每帧同步预算配置，允许运行时在 Inspector 中调整。
            // Sync budget every frame so Inspector changes take effect at runtime.
            simulation.MaxProcessedPixelsPerStep = Mathf.Max(0, config.maxSimulatedPixelsPerStep);
            simulationAccumulator += Time.deltaTime;
            float interval = Mathf.Max(0.001f, config.simulationStepInterval);
            int ticks = 0;

            while (simulationAccumulator >= interval && ticks < 8)
            {
                // 压力测试允许更高追帧上限，但仍避免低帧率时无限补步。
                // The stress test allows a higher catch-up cap but still avoids unlimited catch-up on slow frames.
                int steps = Mathf.Max(1, config.simulationStepsPerFrame);
                for (int i = 0; i < steps; i++)
                {
                    simulation.Step(grid);
                }

                simulationAccumulator -= interval;
                ticks++;
            }

            if (ticks >= 8)
            {
                simulationAccumulator = 0f;
            }
        }

        private void RenderAndRecordTime()
        {
            // 单独测量渲染耗时，和模拟耗时分开展示。
            // Measure render cost separately from simulation cost.
            renderStopwatch.Reset();
            renderStopwatch.Start();
            worldRenderer.Render();
            renderStopwatch.Stop();
            simulation.SetRenderTime((float)renderStopwatch.Elapsed.TotalMilliseconds);
        }

        private void RefreshMaterialCounts(bool force = false)
        {
            // 材料计数是全图扫描，默认低频刷新减少额外开销。
            // Material counting scans the whole grid, so it refreshes at a lower frequency by default.
            countAccumulator += Time.deltaTime;
            if (!force && countAccumulator < 0.2f)
            {
                return;
            }

            countAccumulator = 0f;
            grid.CountMaterials(out int nonAirPixels, out int waterPixels);
            simulation.SetMaterialCounts(nonAirPixels, waterPixels);
        }

        private void HandleHotkeys()
        {
            // 热键用于现场切换实验状态和可视化层。
            // Hotkeys control experiment state and visualization layers live.
            if (WasKeyPressed(KeyCode.Space))
            {
                OpenGate();
            }

            if (WasKeyPressed(KeyCode.R))
            {
                BuildStressTestWorld();
                return;
            }

            if (WasKeyPressed(KeyCode.Alpha1))
            {
                ApplyMode(PixelSimulationMode.FullScan);
            }
            else if (WasKeyPressed(KeyCode.Alpha2))
            {
                ApplyMode(PixelSimulationMode.ActivePixels);
            }
            else if (WasKeyPressed(KeyCode.Alpha3))
            {
                ApplyMode(PixelSimulationMode.ChunkBased);
            }

            if (WasKeyPressed(KeyCode.F1))
            {
                config.showPerformancePanel = !config.showPerformancePanel;
            }

            if (WasKeyPressed(KeyCode.F2))
            {
                config.showChunkBoundaries = !config.showChunkBoundaries;
            }

            if (WasKeyPressed(KeyCode.F3))
            {
                config.showActiveRegions = !config.showActiveRegions;
            }

            if (IsKeyHeld(KeyCode.W))
            {
                InjectWaterFromTop();
            }
        }

        private void BuildTerrain()
        {
            // 创建多平台、多洞口的石质地形，让水流路径更复杂。
            // Creates stone terrain with platforms and gaps so water paths are more complex.
            int width = grid.Width;
            int height = grid.Height;

            for (int x = 0; x < width; x++)
            {
                int groundHeight = GetStressTerrainHeight(x, width, height);
                for (int y = 0; y <= groundHeight; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Stone);
                }
            }

            BuildStoneRect(42, 58, 82, 5);
            BuildStoneRect(132, 42, 68, 5);
            BuildStoneRect(226, 70, 96, 5);
            BuildStoneRect(292, 35, 52, 5);

            BuildStoneSlope(48, 35, 112, 62, 4);
            BuildStoneSlope(182, 28, 248, 58, 5);
            BuildStoneSlope(310, 74, 358, 42, 4);

            CarveAirRect(width / 2 - 24, 1, 18, 44);
            CarveAirRect(width / 2 + 42, 1, 26, 24);
            CarveAirRect(width - 58, 1, 30, 70);
            CarveAirRect(96, 30, 16, 36);
            CarveAirRect(204, 34, 18, 32);

            BuildStoneRect(width / 2 - 28, 45, 26, 4);
            BuildStoneRect(width / 2 + 28, 28, 40, 4);

            playerSpawnCell = new Vector2Int(Mathf.Clamp(38, 4, width - 5), Mathf.Clamp(GetStressTerrainHeight(38, width, height) + 14, 12, height - 12));
        }

        private void BuildWaterTank()
        {
            // 大水箱是压力测试主体，闸门位置会记录到 gateCells。
            // The large water tank is the main stress source, and gate cells are recorded for opening.
            gateCells.Clear();

            int tankLeft = 8;
            int waterWidth = Mathf.Clamp(config.initialWaterWidth, 16, grid.Width - 48);
            int waterHeight = Mathf.Clamp(config.initialWaterHeight, 16, grid.Height - 48);
            int tankBottom = Mathf.Clamp(grid.Height - waterHeight - 22, 24, grid.Height - 24);
            int tankRight = Mathf.Min(grid.Width - 8, tankLeft + waterWidth + 3);
            int tankTop = Mathf.Min(grid.Height - 4, tankBottom + waterHeight + 3);
            tankInterior = new RectInt(tankLeft + 2, tankBottom + 2, tankRight - tankLeft - 4, tankTop - tankBottom - 4);

            BuildStoneRect(tankLeft, tankBottom, tankRight - tankLeft + 1, 2);
            BuildStoneRect(tankLeft, tankBottom, 2, tankTop - tankBottom + 1);
            BuildStoneRect(tankRight - 1, tankBottom, 2, tankTop - tankBottom + 1);

            int gateHeight = Mathf.Clamp(waterHeight / 3, 12, 32);
            for (int y = tankBottom + 2; y < tankBottom + 2 + gateHeight; y++)
            {
                // 记录两列闸门，开闸时一起清空形成出水口。
                // Record two gate columns so opening clears a wider outlet.
                gateCells.Add(new Vector2Int(tankRight - 1, y));
                gateCells.Add(new Vector2Int(tankRight, y));
            }

            for (int y = tankInterior.yMin; y < tankInterior.yMax; y++)
            {
                for (int x = tankInterior.xMin; x < tankInterior.xMax; x++)
                {
                    grid.SetMaterial(x, y, MaterialType.Water);
                }
            }
        }

        private void BuildWoodenStructures()
        {
            // 可燃结构让压力测试同时覆盖流体和热交互。
            // Flammable structures make the stress test cover both fluid flow and heat interactions.
            BuildWoodRect(168, 76, 48, 4);
            BuildWoodRect(168, 54, 4, 24);
            BuildWoodRect(212, 54, 4, 24);
            BuildWoodRect(266, 82, 54, 4);
            BuildWoodRect(292, 54, 4, 30);

            grid.PaintCircle(169, 80, 2, MaterialType.Fire);
        }

        private int GetStressTerrainHeight(int x, int width, int height)
        {
            // 压力测试地形比普通演示更低，给水流留出更多空间。
            // Stress-test terrain stays lower than the default demo to leave more room for water flow.
            float t = width <= 1 ? 0f : x / (float)(width - 1);
            float baseHeight = Mathf.Lerp(height * 0.18f, height * 0.12f, t);
            float wave = Mathf.Sin(t * Mathf.PI * 5.5f) * 7f;
            float basin = -14f * Mathf.Exp(-Mathf.Pow((t - 0.55f) / 0.16f, 2f));
            float bank = 18f * Mathf.Exp(-Mathf.Pow((t - 0.78f) / 0.07f, 2f));
            return Mathf.Clamp(Mathf.RoundToInt(baseHeight + wave + basin + bank), 6, Mathf.RoundToInt(height * 0.42f));
        }

        private void BuildStoneRect(int x, int y, int width, int height)
        {
            FillRect(x, y, width, height, MaterialType.Stone);
        }

        private void BuildWoodRect(int x, int y, int width, int height)
        {
            FillRect(x, y, width, height, MaterialType.Wood);
        }

        private void CarveAirRect(int x, int y, int width, int height)
        {
            FillRect(x, y, width, height, MaterialType.Air);
        }

        private void FillRect(int x, int y, int width, int height, MaterialType material)
        {
            // 所有矩形建造/挖空最终都通过统一填充函数写入材料。
            // All rectangular construction and carving flows through this material fill helper.
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    grid.SetMaterial(px, py, material);
                }
            }
        }

        private void BuildStoneSlope(int startX, int startY, int endX, int endY, int thickness)
        {
            // 用线性插值采样斜坡，再用小矩形加厚。
            // Sample a slope with linear interpolation, then thicken it with small rectangles.
            int steps = Mathf.Max(1, Mathf.Abs(endX - startX));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(startX, endX, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(startY, endY, t));
                FillRect(x, y, thickness, thickness, MaterialType.Stone);
            }
        }

        private PixelWorldRenderer GetOrCreateRenderer()
        {
            // 复用或创建压力测试专用显示对象。
            // Reuse or create the stress-test display object.
            Transform existingDisplay = transform.Find("Stress Test Pixel World");
            GameObject displayObject = existingDisplay == null
                ? new GameObject("Stress Test Pixel World")
                : existingDisplay.gameObject;
            displayObject.transform.SetParent(transform, false);

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

        private SimplePixelPlayer GetOrCreatePlayer()
        {
            // 压力测试玩家单独命名，避免与普通演示玩家混淆。
            // The stress-test player is named separately to avoid confusing it with the default demo player.
            Transform existingPlayer = transform.Find("Stress Test Player");
            GameObject playerObject = existingPlayer == null
                ? new GameObject("Stress Test Player")
                : existingPlayer.gameObject;
            playerObject.transform.SetParent(transform, false);

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

        private StressTestPerformancePanel GetOrCreatePerformancePanel()
        {
            // 性能面板挂在启动器对象上，直接读取启动器公开状态。
            // The performance panel lives on the bootstrap object and reads its public state.
            if (!TryGetComponent(out StressTestPerformancePanel panel))
            {
                panel = gameObject.AddComponent<StressTestPerformancePanel>();
            }

            return panel;
        }

        private StressTestDebugOverlay GetOrCreateDebugOverlay()
        {
            // 调试覆盖层同样挂在启动器对象上，避免额外场景布置。
            // The debug overlay also lives on the bootstrap object to avoid extra scene setup.
            if (!TryGetComponent(out StressTestDebugOverlay overlay))
            {
                overlay = gameObject.AddComponent<StressTestDebugOverlay>();
            }

            return overlay;
        }

        private Camera GetOrCreateCamera()
        {
            // 优先使用场景已有相机，必要时创建主相机。
            // Prefer an existing scene camera, creating a main camera only when needed.
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
            // 将左下原点的像素 Sprite 平移到世界中心。
            // Translate the bottom-left-origin pixel sprite so the world is centered.
            Vector2 worldSize = new Vector2(grid.Width / (float)config.pixelsPerUnit, grid.Height / (float)config.pixelsPerUnit);
            worldRenderer.transform.position = new Vector3(-worldSize.x * 0.5f, -worldSize.y * 0.5f, 0f);
            worldRenderer.transform.rotation = Quaternion.identity;
            worldRenderer.transform.localScale = Vector3.one;
        }

        private void ConfigureCamera()
        {
            // 相机按网格显示尺寸自动取正交大小。
            // Camera orthographic size is derived from the displayed grid size.
            Vector2 worldSize = new Vector2(grid.Width / (float)config.pixelsPerUnit, grid.Height / (float)config.pixelsPerUnit);
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = worldSize.y * 0.5f + Mathf.Max(0f, config.cameraPadding);
            targetCamera.transform.position = new Vector3(0f, 0f, -10f);
            targetCamera.transform.rotation = Quaternion.identity;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = new Color(0.012f, 0.014f, 0.019f, 1f);
        }

        private static bool WasKeyPressed(KeyCode keyCode)
        {
            // 显式映射压力测试需要的热键，兼容新旧输入系统。
            // Explicitly map stress-test hotkeys for both new and legacy input systems.
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (keyCode)
                {
                    case KeyCode.Space:
                        return Keyboard.current.spaceKey.wasPressedThisFrame;
                    case KeyCode.R:
                        return Keyboard.current.rKey.wasPressedThisFrame;
                    case KeyCode.Alpha1:
                        return Keyboard.current.digit1Key.wasPressedThisFrame;
                    case KeyCode.Alpha2:
                        return Keyboard.current.digit2Key.wasPressedThisFrame;
                    case KeyCode.Alpha3:
                        return Keyboard.current.digit3Key.wasPressedThisFrame;
                    case KeyCode.F1:
                        return Keyboard.current.f1Key.wasPressedThisFrame;
                    case KeyCode.F2:
                        return Keyboard.current.f2Key.wasPressedThisFrame;
                    case KeyCode.F3:
                        return Keyboard.current.f3Key.wasPressedThisFrame;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }

        private static bool IsKeyHeld(KeyCode keyCode)
        {
            // 当前只需要检测 W 持续补水。
            // Currently only W needs held-key detection for continuous water injection.
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && keyCode == KeyCode.W)
            {
                return Keyboard.current.wKey.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(keyCode);
#else
            return false;
#endif
        }
    }
}
