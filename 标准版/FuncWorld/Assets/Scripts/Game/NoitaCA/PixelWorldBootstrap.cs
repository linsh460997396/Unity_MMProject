using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NoitaCA
{
    [DefaultExecutionOrder(-100)]
    public sealed class PixelWorldBootstrap : MonoBehaviour
    {
        [Header("World")]
        [SerializeField] private int worldWidth = 256;
        [SerializeField] private int worldHeight = 144;
        [SerializeField] private int pixelsPerUnit = 16;
        [SerializeField] private int simulationStepsPerFrame = 1;
        [SerializeField] private float simulationStepInterval = 0.035f;
        [SerializeField] private float cameraPadding = 0.5f;

        [Header("Alchemy Art Direction")]
        [SerializeField] private bool enableAlchemyArtDirection = true;
        [SerializeField] private bool enableProceduralBackdrop = true;
        [SerializeField] private bool enablePostProcessing = true;
        [SerializeField] private PixelWorldRenderSettings renderSettings = PixelWorldRenderSettings.CreateAlchemyDefault();

        [Header("Demo")]
        [SerializeField] private bool buildDemoTerrain = true;
        [SerializeField] private bool buildDemoPlayer = true;
        [SerializeField] private bool enableDebugPaintingWithPlayer;
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private int playerWidthInCells = 7;
        [SerializeField] private int playerHeightInCells = 14;
        [SerializeField] private float playerVisualHeightInCells = 18f;
        [SerializeField] private int bottomlessHoleCenterX = -1;
        [SerializeField] private int bottomlessHoleWidth = 10;

        [Header("Infinite Cave")]
        [SerializeField] private bool enableInfiniteCave = true;
        [SerializeField] private int infiniteCaveSeed = 1729;
        [SerializeField] private int transitionTriggerRow = 8;
        [SerializeField] private int segmentEntryMarginFromTop = 20;
        [SerializeField] private int plannedSpawnPointsPerSegment = 12;
        [SerializeField] private bool spawnPixelCreaturesAndEquipment = true;
        [SerializeField] private int guaranteedNearbyMonsters = 0;
        [SerializeField] private int guaranteedNearbyEquipment = 1;
        [SerializeField] private PixelCreatureDefinition[] creatureDefinitions;
        [SerializeField] private PixelEquipmentDefinition[] equipmentDefinitions;
        [SerializeField] private bool enableCameraFollow = true;
        [SerializeField] private float cameraFollowSmoothTime = 0.22f;
        [SerializeField] private float cameraFollowHorizontalDeadZone = 1.25f;
        [SerializeField] private float cameraFollowVerticalLookAhead = -0.6f;
        [SerializeField] private float cameraFollowOrthographicSize = 5.2f;

        private PixelGrid grid;
        private PixelSimulation simulation;
        private PixelWorldRenderer worldRenderer;
        private InputController inputController;
        private PlayerController player;
        private PixelWorldSpawner worldSpawner;
        private Camera targetCamera;
        private VolumeProfile generatedVolumeProfile;
        private readonly List<PixelWorldSpawnPoint> spawnPoints = new List<PixelWorldSpawnPoint>(32);
        private Vector3 cameraFollowVelocity;
        private float simulationAccumulator;
        private Vector2Int playerSpawnCell;
        private int currentCaveSegment;
        private int segmentHeight;
        private float segmentWorldStep;

        public int CurrentCaveSegment => currentCaveSegment;
        public IReadOnlyList<PixelWorldSpawnPoint> SpawnPoints => spawnPoints;

        private void Awake()
        {
            BuildWorld();
        }

        private void Update()
        {
            if (grid == null || simulation == null || worldRenderer == null || inputController == null)
            {
                return;
            }

            if (!buildDemoPlayer || enableDebugPaintingWithPlayer)
            {
                inputController.Tick();
            }

            simulationAccumulator += Time.deltaTime;
            float safeStepInterval = Mathf.Max(0.001f, simulationStepInterval);
            int simulatedTicks = 0;

            while (simulationAccumulator >= safeStepInterval && simulatedTicks < 4)
            {
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
                simulationAccumulator = 0f;
            }

            UpdateInfiniteCaveTransition();
            if (enableInfiniteCave && enableCameraFollow)
            {
                worldRenderer.RenderVisible(targetCamera, Mathf.Max(8, worldHeight / 12));
            }
            else
            {
                worldRenderer.Render();
            }
        }

        private void LateUpdate()
        {
            UpdateCameraFollow();
        }

        private void UpdateInfiniteCaveTransition()
        {
            if (!enableInfiniteCave || !buildDemoPlayer || player == null || worldRenderer == null)
            {
                return;
            }

            Vector2Int playerCell = worldRenderer.WorldToCell(player.transform.position);
            if (playerCell.y < segmentHeight && IsPreviousSegmentFullyOutOfView())
            {
                AdvanceToNextCaveSegment();
            }
        }

        private bool IsPreviousSegmentFullyOutOfView()
        {
            if (targetCamera == null)
            {
                return true;
            }

            Vector3 cameraTop = targetCamera.transform.position + Vector3.up * targetCamera.orthographicSize;
            Vector2Int topCell = worldRenderer.WorldToCell(cameraTop);
            return topCell.y < segmentHeight - Mathf.Max(2, transitionTriggerRow);
        }

        private void AdvanceToNextCaveSegment()
        {
            currentCaveSegment++;
            CopySegment(0, segmentHeight);
            BuildInfiniteCaveSegment(currentCaveSegment + 1, 0);
            CarveSegmentConnector(currentCaveSegment, currentCaveSegment + 1);
            ConfigureDisplayTransform();
            GenerateSpawnPointsForSegment(currentCaveSegment, GetCurrentSegmentOffset());
            SpawnCurrentSegmentContent();
            ConfigureProceduralBackdrop();
            ConfigureAlchemyLights();

            Vector2Int currentCell = worldRenderer.WorldToCell(player.transform.position);
            player.SetRespawnCell(currentCell);
            grid.MarkActiveArea(currentCell.x, currentCell.y, 16);
        }

        private void UpdateCameraFollow()
        {
            if (!enableCameraFollow || !buildDemoPlayer || player == null || targetCamera == null || worldRenderer == null)
            {
                return;
            }

            Vector3 cameraPosition = targetCamera.transform.position;
            Vector3 playerPosition = player.transform.position;
            float targetX = cameraPosition.x;
            if (Mathf.Abs(playerPosition.x - cameraPosition.x) > Mathf.Max(0f, cameraFollowHorizontalDeadZone))
            {
                targetX = playerPosition.x;
            }

            float targetY = playerPosition.y + cameraFollowVerticalLookAhead;
            Vector3 target = new Vector3(targetX, targetY, cameraPosition.z);
            targetCamera.transform.position = Vector3.SmoothDamp(
                cameraPosition,
                target,
                ref cameraFollowVelocity,
                Mathf.Max(0.01f, cameraFollowSmoothTime));
        }

        private void BuildInfiniteCaveSegment(int segmentIndex, int yOffset)
        {
            ClearSegment(yOffset, MaterialType.Stone);
            BuildInfiniteMineralStrata(segmentIndex, yOffset);

            Vector2Int entry = GetSegmentEntryCell(segmentIndex, yOffset);
            Vector2Int exit = GetSegmentExitCell(segmentIndex, yOffset);
            int tunnelRadius = Mathf.Clamp(worldWidth / 28, 6, 11);

            for (int localY = segmentHeight - 4; localY >= 0; localY--)
            {
                float depthT = 1f - localY / (float)Mathf.Max(1, segmentHeight - 1);
                float bend = Mathf.Sin(depthT * Mathf.PI * (2.4f + (segmentIndex % 4) * 0.35f) + segmentIndex * 1.17f) * worldWidth * 0.18f;
                float wobble = Mathf.Sin(depthT * Mathf.PI * 9.5f + infiniteCaveSeed * 0.013f + segmentIndex) * worldWidth * 0.045f;
                int centerX = Mathf.RoundToInt(Mathf.Lerp(entry.x, exit.x, depthT) + bend + wobble);
                centerX = Mathf.Clamp(centerX, tunnelRadius + 3, worldWidth - tunnelRadius - 4);
                int radius = tunnelRadius + Mathf.RoundToInt((Mathf.Sin(localY * 0.19f + segmentIndex) * 0.5f + 0.5f) * 3f);
                grid.PaintCircle(centerX, yOffset + localY, radius, MaterialType.Air);
            }

            CarveRoom(entry.x, entry.y - 4, 24, 12);
            CarveRoom(exit.x, Mathf.Max(10, exit.y + 7), 20, 11);
            CarveProceduralSideRooms(segmentIndex, yOffset);
            BuildSegmentHazardsAndMaterials(segmentIndex, yOffset);
            playerSpawnCell = entry;
        }

        private void ClearWorld(MaterialType material)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    grid.SetMaterial(x, y, material);
                }
            }
        }

        private void ClearSegment(int yOffset, MaterialType material)
        {
            for (int y = yOffset; y < yOffset + segmentHeight && y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    grid.SetMaterial(x, y, material);
                }
            }
        }

        private void CopySegment(int sourceYOffset, int destinationYOffset)
        {
            for (int y = 0; y < segmentHeight; y++)
            {
                int sourceY = sourceYOffset + y;
                int destinationY = destinationYOffset + y;
                if (sourceY < 0 || sourceY >= worldHeight || destinationY < 0 || destinationY >= worldHeight)
                {
                    continue;
                }

                for (int x = 0; x < worldWidth; x++)
                {
                    grid.SetCell(x, destinationY, grid.GetCell(x, sourceY));
                    grid.MarkChanged(x, destinationY);
                }
            }
        }

        private void CarveSegmentConnector(int upperSegmentIndex, int lowerSegmentIndex)
        {
            Vector2Int upperExit = GetSegmentExitCell(upperSegmentIndex, GetCurrentSegmentOffset());
            Vector2Int lowerEntry = GetSegmentEntryCell(lowerSegmentIndex, 0);
            CarveSlopeTunnel(upperExit.x, upperExit.y, lowerEntry.x, lowerEntry.y, Mathf.Clamp(worldWidth / 30, 5, 9));
            CarveRoom(upperExit.x, upperExit.y + 4, 16, 9);
            CarveRoom(lowerEntry.x, lowerEntry.y - 4, 18, 10);
        }

        private int GetCurrentSegmentOffset()
        {
            return enableInfiniteCave ? segmentHeight : 0;
        }

        private Vector2Int GetSegmentEntryCell(int segmentIndex, int yOffset)
        {
            int margin = Mathf.Clamp(segmentEntryMarginFromTop, 12, segmentHeight - 12);
            int x = Mathf.Clamp(Mathf.RoundToInt(worldWidth * (0.36f + Hash01(segmentIndex, 11) * 0.28f)), 16, worldWidth - 17);
            return new Vector2Int(x, yOffset + segmentHeight - margin);
        }

        private Vector2Int GetSegmentExitCell(int segmentIndex, int yOffset)
        {
            int x = Mathf.Clamp(Mathf.RoundToInt(worldWidth * (0.32f + Hash01(segmentIndex, 47) * 0.36f)), 16, worldWidth - 17);
            return new Vector2Int(x, yOffset + 4);
        }

        private void BuildInfiniteMineralStrata(int segmentIndex, int yOffset)
        {
            for (int localY = 0; localY < segmentHeight; localY++)
            {
                int y = yOffset + localY;
                for (int x = 0; x < worldWidth; x++)
                {
                    float vein = Mathf.Sin((x + segmentIndex * 31) * 0.055f + localY * 0.19f)
                        + Mathf.Sin(x * 0.17f - (localY + segmentIndex * 13) * 0.073f) * 0.55f;
                    if (vein > 1.18f)
                    {
                        grid.SetMaterial(x, y, MaterialType.Debris);
                    }
                    else if (vein < -1.2f)
                    {
                        grid.SetMaterial(x, y, MaterialType.Ash);
                    }
                    else if (Hash01(x + segmentIndex * 97, y * 3 + 5) > 0.992f)
                    {
                        grid.SetMaterial(x, y, MaterialType.Ice);
                    }
                }
            }
        }

        private void CarveProceduralSideRooms(int segmentIndex, int yOffset)
        {
            int roomCount = 4 + segmentIndex % 3;
            for (int i = 0; i < roomCount; i++)
            {
                float t = (i + 1f) / (roomCount + 1f);
                int centerY = yOffset + Mathf.RoundToInt(Mathf.Lerp(segmentHeight - 30, 28, t));
                int centerX = Mathf.RoundToInt(Mathf.Lerp(worldWidth * 0.22f, worldWidth * 0.78f, Hash01(segmentIndex, 100 + i)));
                int radiusX = Mathf.RoundToInt(Mathf.Lerp(15f, 34f, Hash01(segmentIndex, 210 + i)));
                int radiusY = Mathf.RoundToInt(Mathf.Lerp(8f, 18f, Hash01(segmentIndex, 320 + i)));
                CarveRoom(centerX, centerY, radiusX, radiusY);
                if ((i + segmentIndex) % 2 == 0)
                {
                    CarveSlopeTunnel(centerX, centerY, worldWidth / 2, yOffset + Mathf.Clamp(centerY - yOffset - 10, 12, segmentHeight - 16), 4);
                }
            }
        }

        private void BuildSegmentHazardsAndMaterials(int segmentIndex, int yOffset)
        {
            int waterX = Mathf.RoundToInt(Mathf.Lerp(worldWidth * 0.2f, worldWidth * 0.44f, Hash01(segmentIndex, 501)));
            int waterY = yOffset + Mathf.RoundToInt(Mathf.Lerp(segmentHeight * 0.18f, segmentHeight * 0.48f, Hash01(segmentIndex, 502)));
            FillEllipse(waterX, waterY, 14, 5, MaterialType.Water, true);

            if ((segmentIndex & 1) == 0)
            {
                int poisonX = Mathf.RoundToInt(Mathf.Lerp(worldWidth * 0.56f, worldWidth * 0.82f, Hash01(segmentIndex, 601)));
                int poisonY = yOffset + Mathf.RoundToInt(Mathf.Lerp(segmentHeight * 0.22f, segmentHeight * 0.54f, Hash01(segmentIndex, 602)));
                FillEllipse(poisonX, poisonY, 13, 5, MaterialType.Poison, true);
            }

            if (segmentIndex % 3 == 0)
            {
                int lavaX = Mathf.RoundToInt(Mathf.Lerp(worldWidth * 0.58f, worldWidth * 0.86f, Hash01(segmentIndex, 701)));
                int lavaY = yOffset + Mathf.RoundToInt(Mathf.Lerp(segmentHeight * 0.11f, segmentHeight * 0.28f, Hash01(segmentIndex, 702)));
                FillEllipse(lavaX, lavaY, 15, 6, MaterialType.Lava, true);
            }

            int ruinX = Mathf.RoundToInt(Mathf.Lerp(worldWidth * 0.28f, worldWidth * 0.68f, Hash01(segmentIndex, 801)));
            int ruinY = yOffset + Mathf.RoundToInt(Mathf.Lerp(segmentHeight * 0.32f, segmentHeight * 0.68f, Hash01(segmentIndex, 802)));
            BuildWoodRect(ruinX - 14, ruinY, 28, 2);
            BuildWoodRect(ruinX - 12, ruinY - 10, 3, 12);
            BuildWoodRect(ruinX + 10, ruinY - 8, 3, 10);
            if ((segmentIndex % 2) == 1)
            {
                grid.PaintCircle(ruinX - 12, ruinY + 3, 2, MaterialType.Fire);
            }
        }

        private void GenerateSpawnPointsForSegment(int segmentIndex, int yOffset)
        {
            spawnPoints.Clear();
            AddStandableMonsterSpawnPoints(segmentIndex, yOffset);

            int targetUtilityCount = Mathf.Clamp(plannedSpawnPointsPerSegment, 0, 64);
            int attempts = Mathf.Max(160, targetUtilityCount * 40);
            int utilityCount = 0;

            for (int i = 0; i < attempts && utilityCount < targetUtilityCount; i++)
            {
                int x = 8 + Mathf.FloorToInt(Hash01(segmentIndex, 900 + i * 2) * Mathf.Max(1, worldWidth - 16));
                int y = yOffset + 12 + Mathf.FloorToInt(Hash01(segmentIndex, 901 + i * 2) * Mathf.Max(1, segmentHeight - 30));
                if (!IsSpawnCandidate(x, y))
                {
                    continue;
                }

                float roll = Hash01(segmentIndex, 1000 + i);
                PixelWorldSpawnCategory category = roll < 0.62f
                    ? PixelWorldSpawnCategory.Item
                    : roll < 0.84f
                        ? PixelWorldSpawnCategory.Treasure
                        : PixelWorldSpawnCategory.Ambient;
                Vector2Int cell = new Vector2Int(x, y);
                spawnPoints.Add(new PixelWorldSpawnPoint(
                    category,
                    cell,
                    worldRenderer != null ? worldRenderer.CellToWorldCenter(x, y) : Vector3.zero,
                    segmentIndex,
                    Mathf.Lerp(0.35f, 1f, Hash01(segmentIndex, 1100 + i))));
                utilityCount++;
            }
        }

        private void AddStandableMonsterSpawnPoints(int segmentIndex, int yOffset)
        {
            int minY = Mathf.Max(1, yOffset);
            int maxY = Mathf.Min(grid.Height - 1, yOffset + segmentHeight - 1);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    if (!IsSpawnCandidate(x, y))
                    {
                        continue;
                    }

                    Vector2Int cell = new Vector2Int(x, y);
                    spawnPoints.Add(new PixelWorldSpawnPoint(
                        PixelWorldSpawnCategory.Monster,
                        cell,
                        worldRenderer != null ? worldRenderer.CellToWorldCenter(x, y) : Vector3.zero,
                        segmentIndex,
                        Mathf.Lerp(0.35f, 1f, Hash01(segmentIndex, 1300 + x * 17 + y))));
                }
            }
        }

        private bool IsSpawnCandidate(int x, int y)
        {
            if (!grid.InBounds(x, y) || !MaterialDatabase.Get(grid.GetMaterial(x, y)).IsAir)
            {
                return false;
            }

            if (!grid.InBounds(x, y - 1) || !grid.IsSolid(x, y - 1))
            {
                return false;
            }

            int halfWidth = Mathf.Max(1, playerWidthInCells / 2);
            int clearHeight = Mathf.Max(1, playerHeightInCells);
            for (int ox = -halfWidth; ox <= halfWidth; ox++)
            {
                for (int oy = 0; oy < clearHeight; oy++)
                {
                    if (!grid.InBounds(x + ox, y + oy) || !MaterialDatabase.Get(grid.GetMaterial(x + ox, y + oy)).IsAir)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void BuildWorld()
        {
            segmentHeight = Mathf.Max(32, worldHeight);
            int gridHeight = enableInfiniteCave ? segmentHeight * 2 : segmentHeight;
            grid = new PixelGrid(worldWidth, gridHeight);
            worldWidth = grid.Width;
            worldHeight = grid.Height;
            segmentWorldStep = segmentHeight / (float)Mathf.Max(1, pixelsPerUnit);
            currentCaveSegment = 0;
            spawnPoints.Clear();
            simulation = new PixelSimulation();
            if (enableInfiniteCave)
            {
                simulation.Mode = PixelSimulationMode.ChunkBased;
            }

            if (enableInfiniteCave)
            {
                BuildInfiniteCaveSegment(currentCaveSegment, GetCurrentSegmentOffset());
                BuildInfiniteCaveSegment(currentCaveSegment + 1, 0);
                CarveSegmentConnector(currentCaveSegment, currentCaveSegment + 1);
                playerSpawnCell = GetSegmentEntryCell(currentCaveSegment, GetCurrentSegmentOffset());
                grid.ActivateAll();
            }
            else if (buildDemoTerrain)
            {
                BuildDemoTerrain();
            }
            else
            {
                grid.ActivateAll();
                playerSpawnCell = new Vector2Int(Mathf.Clamp(worldWidth / 7, 4, worldWidth - 5), Mathf.Clamp(worldHeight / 2, 8, worldHeight - 8));
            }

            targetCamera = GetOrCreateCamera();
            worldRenderer = GetOrCreateRenderer();
            inputController = GetOrCreateInputController();
            worldSpawner = GetOrCreateSpawner();

            ConfigureDisplayTransform();
            PixelWorldRenderSettings activeRenderSettings = enableAlchemyArtDirection
                ? renderSettings ?? PixelWorldRenderSettings.CreateAlchemyDefault()
                : PixelWorldRenderSettings.CreateClassicDefault();
            worldRenderer.Initialize(grid, pixelsPerUnit, activeRenderSettings);
            GenerateSpawnPointsForSegment(currentCaveSegment, GetCurrentSegmentOffset());
            ConfigureCamera();
            ConfigureArtPresentation();
            inputController.Initialize(grid, worldRenderer, targetCamera);

            if (buildDemoPlayer)
            {
                player = GetOrCreatePlayer();
                player.ConfigureSize(playerWidthInCells, playerHeightInCells, playerVisualHeightInCells);
                player.Initialize(grid, worldRenderer, targetCamera, playerSpawnCell, playerSprite);
            }

            if (spawnPixelCreaturesAndEquipment && worldSpawner != null)
            {
                worldSpawner.Initialize(grid, worldRenderer, player, creatureDefinitions, equipmentDefinitions);
                SpawnCurrentSegmentContent();
                worldSpawner.SpawnIntroContent(playerSpawnCell, guaranteedNearbyMonsters, guaranteedNearbyEquipment);
            }

            worldRenderer.Render();
        }

        private void SpawnCurrentSegmentContent()
        {
            if (!spawnPixelCreaturesAndEquipment || worldSpawner == null)
            {
                return;
            }

            worldSpawner.SpawnFromPoints(spawnPoints, currentCaveSegment);
        }

        private PixelWorldRenderer GetOrCreateRenderer()
        {
            Transform existingDisplay = transform.Find("Pixel World Display");
            GameObject displayObject = existingDisplay == null
                ? new GameObject("Pixel World Display")
                : existingDisplay.gameObject;
            displayObject.transform.SetParent(transform, false);

            if (!displayObject.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer = displayObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sortingOrder = 0;

            if (!displayObject.TryGetComponent(out PixelWorldRenderer renderer))
            {
                renderer = displayObject.AddComponent<PixelWorldRenderer>();
            }

            return renderer;
        }

        private InputController GetOrCreateInputController()
        {
            if (!TryGetComponent(out InputController controller))
            {
                controller = gameObject.AddComponent<InputController>();
            }

            return controller;
        }

        private PixelWorldSpawner GetOrCreateSpawner()
        {
            Transform existingSpawner = transform.Find("Pixel World Spawner");
            GameObject spawnerObject = existingSpawner == null
                ? new GameObject("Pixel World Spawner")
                : existingSpawner.gameObject;
            spawnerObject.transform.SetParent(transform, false);

            if (!spawnerObject.TryGetComponent(out PixelWorldSpawner spawner))
            {
                spawner = spawnerObject.AddComponent<PixelWorldSpawner>();
            }

            return spawner;
        }

        private PlayerController GetOrCreatePlayer()
        {
            Transform existingPlayer = transform.Find("Demo Player");
            GameObject playerObject = existingPlayer == null
                ? new GameObject("Demo Player")
                : existingPlayer.gameObject;
            playerObject.transform.SetParent(transform, false);

            if (!playerObject.TryGetComponent(out SpriteRenderer _))
            {
                playerObject.AddComponent<SpriteRenderer>();
            }

            if (playerObject.TryGetComponent(out SimplePixelPlayer legacyPlayer))
            {
                legacyPlayer.enabled = false;
            }

            if (!playerObject.TryGetComponent(out PlayerController playerController))
            {
                playerController = playerObject.AddComponent<PlayerController>();
            }

            return playerController;
        }

        private Camera GetOrCreateCamera()
        {
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
            Vector2 worldSize = new Vector2(worldWidth / (float)pixelsPerUnit, worldHeight / (float)pixelsPerUnit);
            float infiniteYOffset = enableInfiniteCave ? -(currentCaveSegment + 1) * segmentWorldStep : -worldSize.y * 0.5f;
            worldRenderer.transform.position = new Vector3(-worldSize.x * 0.5f, infiniteYOffset, 0f);
            worldRenderer.transform.rotation = Quaternion.identity;
            worldRenderer.transform.localScale = Vector3.one;
        }

        private void ConfigureCamera()
        {
            Vector2 worldSize = new Vector2(worldWidth / (float)pixelsPerUnit, worldHeight / (float)pixelsPerUnit);
            float segmentWorldHeight = segmentHeight / (float)Mathf.Max(1, pixelsPerUnit);
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = enableInfiniteCave && enableCameraFollow
                ? Mathf.Clamp(cameraFollowOrthographicSize, 2.2f, segmentWorldHeight * 0.5f + Mathf.Max(0f, cameraPadding))
                : worldSize.y * 0.5f + Mathf.Max(0f, cameraPadding);
            targetCamera.transform.position = new Vector3(0f, 0f, -10f);
            targetCamera.transform.rotation = Quaternion.identity;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.backgroundColor = enableAlchemyArtDirection
                ? new Color(0.012f, 0.015f, 0.026f, 1f)
                : new Color(0.015f, 0.018f, 0.025f, 1f);
        }

        private void ConfigureArtPresentation()
        {
            ConfigureProceduralBackdrop();
            ConfigureAlchemyLights();
            ConfigurePostProcessing();
        }

        private void ConfigureProceduralBackdrop()
        {
            Transform existingBackdrop = transform.Find("Alchemy Backdrop");
            if (!enableAlchemyArtDirection || !enableProceduralBackdrop)
            {
                if (existingBackdrop != null)
                {
                    existingBackdrop.gameObject.SetActive(false);
                }

                return;
            }

            GameObject backdropObject = existingBackdrop == null
                ? new GameObject("Alchemy Backdrop")
                : existingBackdrop.gameObject;
            backdropObject.transform.SetParent(transform, false);
            backdropObject.SetActive(true);

            if (!backdropObject.TryGetComponent(out SpriteRenderer _))
            {
                backdropObject.AddComponent<SpriteRenderer>();
            }

            if (!backdropObject.TryGetComponent(out PixelWorldBackdrop backdrop))
            {
                backdrop = backdropObject.AddComponent<PixelWorldBackdrop>();
            }

            backdrop.Initialize(worldRenderer.WorldSize, worldWidth, worldHeight);
            backdropObject.transform.position += new Vector3(0f, -currentCaveSegment * segmentWorldStep, 0f);
        }

        private void ConfigureAlchemyLights()
        {
            bool active = enableAlchemyArtDirection;
            ConfigureArtLight("Alchemy Key Glow", active, worldWidth * 0.18f, worldHeight * 0.54f, new Color(0.18f, 0.72f, 0.92f, 1f), 0.38f, 4.2f);
            ConfigureArtLight("Alchemy Poison Glow", active, worldWidth * 0.72f, worldHeight * 0.38f, new Color(0.26f, 1f, 0.42f, 1f), 0.34f, 3.6f);
            ConfigureArtLight("Alchemy Ember Glow", active, worldWidth * 0.84f, worldHeight * 0.24f, new Color(1f, 0.32f, 0.12f, 1f), 0.42f, 4.8f);
        }

        private void ConfigureArtLight(string objectName, bool active, float cellX, float cellY, Color color, float intensity, float outerRadius)
        {
            Transform existing = transform.Find(objectName);
            if (!active)
            {
                if (existing != null)
                {
                    existing.gameObject.SetActive(false);
                }

                return;
            }

            GameObject lightObject = existing == null ? new GameObject(objectName) : existing.gameObject;
            lightObject.transform.SetParent(transform, true);
            lightObject.SetActive(true);

            if (!lightObject.TryGetComponent(out Light2D light2D))
            {
                light2D = lightObject.AddComponent<Light2D>();
            }

            Vector3 position = worldRenderer.CellToWorldCenter(Mathf.RoundToInt(cellX), Mathf.RoundToInt(cellY));
            position.z = -0.1f;
            lightObject.transform.position = position;
            light2D.lightType = Light2D.LightType.Point;
            light2D.color = color;
            light2D.intensity = intensity;
            light2D.pointLightInnerRadius = outerRadius * 0.2f;
            light2D.pointLightOuterRadius = outerRadius;
        }

        private void ConfigurePostProcessing()
        {
            if (targetCamera == null)
            {
                return;
            }

            UniversalAdditionalCameraData cameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                cameraData = targetCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            cameraData.renderPostProcessing = enableAlchemyArtDirection && enablePostProcessing;

            Transform existingVolume = transform.Find("Alchemy Post Process Volume");
            if (!enableAlchemyArtDirection || !enablePostProcessing)
            {
                if (existingVolume != null)
                {
                    existingVolume.gameObject.SetActive(false);
                }

                return;
            }

            GameObject volumeObject = existingVolume == null
                ? new GameObject("Alchemy Post Process Volume")
                : existingVolume.gameObject;
            volumeObject.transform.SetParent(transform, false);
            volumeObject.SetActive(true);

            if (!volumeObject.TryGetComponent(out Volume volume))
            {
                volume = volumeObject.AddComponent<Volume>();
            }

            if (generatedVolumeProfile != null)
            {
                DestroyObject(generatedVolumeProfile);
            }

            generatedVolumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            generatedVolumeProfile.name = "Runtime Alchemy Volume Profile";
            generatedVolumeProfile.hideFlags = HideFlags.HideAndDontSave;
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.profile = generatedVolumeProfile;

            Bloom bloom = generatedVolumeProfile.Add<Bloom>(true);
            bloom.threshold.Override(0.72f);
            bloom.intensity.Override(0.42f);
            bloom.scatter.Override(0.62f);

            Vignette vignette = generatedVolumeProfile.Add<Vignette>(true);
            vignette.intensity.Override(0.24f);
            vignette.smoothness.Override(0.56f);
            vignette.color.Override(new Color(0.01f, 0.012f, 0.02f, 1f));

            ColorAdjustments colorAdjustments = generatedVolumeProfile.Add<ColorAdjustments>(true);
            colorAdjustments.postExposure.Override(-0.08f);
            colorAdjustments.contrast.Override(18f);
            colorAdjustments.saturation.Override(14f);
            colorAdjustments.colorFilter.Override(new Color(0.92f, 1f, 0.96f, 1f));

            ChromaticAberration chromaticAberration = generatedVolumeProfile.Add<ChromaticAberration>(true);
            chromaticAberration.intensity.Override(0.045f);

            FilmGrain filmGrain = generatedVolumeProfile.Add<FilmGrain>(true);
            filmGrain.intensity.Override(0.08f);
            filmGrain.response.Override(0.72f);
        }

        private void BuildDemoTerrain()
        {
            int[] surfaceHeights = new int[worldWidth];

            for (int x = 0; x < worldWidth; x++)
            {
                surfaceHeights[x] = GetTerrainHeight(x);
                for (int y = 0; y < surfaceHeights[x]; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Stone);
                }
            }

            BuildMineralStrata(surfaceHeights);
            CarveAlchemyArena(surfaceHeights);
            CarveBottomlessHole();
            SeedWater(surfaceHeights);
            SeedSand(surfaceHeights);
            SeedPoisonPool();
            SeedLavaPocket();
            BuildAlchemyRuins(surfaceHeights);
            SeedSmokePocket(surfaceHeights);
            PreparePlayerSpawn(surfaceHeights);
            grid.ActivateAll();
        }

        private int GetTerrainHeight(int x)
        {
            float t = worldWidth <= 1 ? 0f : x / (float)(worldWidth - 1);
            float descendingSlope = Mathf.Lerp(worldHeight * 0.52f, worldHeight * 0.25f, t);
            float longWave = Mathf.Sin(t * Mathf.PI * 3.1f + 0.35f) * 9f;
            float shortWave = Mathf.Sin(t * Mathf.PI * 13.0f + 1.2f) * 3.5f;
            float leftMesa = 11f * Mathf.Exp(-Mathf.Pow((t - 0.18f) / 0.08f, 2f));
            float ritualBasin = -18f * Mathf.Exp(-Mathf.Pow((t - 0.56f) / 0.16f, 2f));
            float rightSpire = 15f * Mathf.Exp(-Mathf.Pow((t - 0.79f) / 0.055f, 2f));

            int minHeight = Mathf.Max(8, Mathf.RoundToInt(worldHeight * 0.11f));
            int maxHeight = Mathf.Max(minHeight + 1, Mathf.RoundToInt(worldHeight * 0.68f));
            return Mathf.Clamp(Mathf.RoundToInt(descendingSlope + longWave + shortWave + leftMesa + ritualBasin + rightSpire), minHeight, maxHeight);
        }

        private void BuildMineralStrata(int[] surfaceHeights)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 4; y < surfaceHeights[x] - 4; y++)
                {
                    float layer = Mathf.Sin(y * 0.24f + x * 0.035f) + Mathf.Sin(y * 0.09f - x * 0.065f) * 0.5f;
                    float crystal = Mathf.Sin(x * 0.19f + y * 0.41f) * Mathf.Sin(x * 0.047f - y * 0.18f);

                    if (layer > 1.12f)
                    {
                        grid.SetMaterial(x, y, MaterialType.Debris);
                    }
                    else if (layer < -1.12f)
                    {
                        grid.SetMaterial(x, y, MaterialType.Ash);
                    }
                    else if (crystal > 0.82f && y < surfaceHeights[x] - 13 && ((x + y) % 3) == 0)
                    {
                        grid.SetMaterial(x, y, MaterialType.Ice);
                    }
                }
            }
        }

        private void CarveAlchemyArena(int[] surfaceHeights)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                float t = worldWidth <= 1 ? 0f : x / (float)(worldWidth - 1);
                int carveDepth = Mathf.RoundToInt(Mathf.Lerp(5f, 2f, t));
                if (t > 0.38f && t < 0.7f)
                {
                    carveDepth += 5;
                }

                for (int y = surfaceHeights[x] - carveDepth; y < surfaceHeights[x] + 23; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Air);
                }
            }

            CarveRoom(Mathf.RoundToInt(worldWidth * 0.28f), Mathf.RoundToInt(worldHeight * 0.39f), 28, 15);
            CarveRoom(Mathf.RoundToInt(worldWidth * 0.56f), Mathf.RoundToInt(worldHeight * 0.46f), 37, 22);
            CarveRoom(Mathf.RoundToInt(worldWidth * 0.76f), Mathf.RoundToInt(worldHeight * 0.33f), 28, 15);
            CarveRoom(Mathf.RoundToInt(worldWidth * 0.86f), Mathf.RoundToInt(worldHeight * 0.58f), 18, 10);
            CarveSlopeTunnel(Mathf.RoundToInt(worldWidth * 0.18f), Mathf.RoundToInt(worldHeight * 0.31f), Mathf.RoundToInt(worldWidth * 0.42f), Mathf.RoundToInt(worldHeight * 0.5f), 4);
            CarveSlopeTunnel(Mathf.RoundToInt(worldWidth * 0.63f), Mathf.RoundToInt(worldHeight * 0.24f), Mathf.RoundToInt(worldWidth * 0.88f), Mathf.RoundToInt(worldHeight * 0.43f), 5);
        }

        private void CarveRoom(int centerX, int centerY, int radiusX, int radiusY)
        {
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

        private void CarveSlopeTunnel(int startX, int startY, int endX, int endY, int radius)
        {
            int steps = Mathf.Max(1, Mathf.Abs(endX - startX));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(startX, endX, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(startY, endY, t));
                grid.PaintCircle(x, y, radius, MaterialType.Air);
            }
        }

        private void CarveBottomlessHole()
        {
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

            int safeHeight = Mathf.Clamp(height, 0, worldHeight);
            for (int y = 0; y < safeHeight; y++)
            {
                grid.SetMaterial(x, y, MaterialType.Air);
            }
        }

        private void SeedWater(int[] surfaceHeights)
        {
            int reservoirStart = Mathf.Max(2, worldWidth / 14);
            int reservoirEnd = Mathf.Min(worldWidth - 3, worldWidth / 4);

            for (int x = reservoirStart; x <= reservoirEnd; x++)
            {
                int top = Mathf.Min(worldHeight - 5, surfaceHeights[x] + 15);
                int bottom = Mathf.Max(2, surfaceHeights[x] - 4);
                for (int y = bottom; y <= top; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Water);
                }
            }
        }

        private void SeedSand(int[] surfaceHeights)
        {
            int center = Mathf.RoundToInt(worldWidth * 0.35f);
            for (int x = center - 17; x <= center + 17; x++)
            {
                if (x < 0 || x >= worldWidth)
                {
                    continue;
                }

                int pileHeight = Mathf.RoundToInt(18f * Mathf.Clamp01(1f - Mathf.Abs(x - center) / 17f));
                for (int y = surfaceHeights[x] + 1; y <= surfaceHeights[x] + pileHeight; y++)
                {
                    grid.SetMaterial(x, y, MaterialType.Sand);
                }
            }
        }

        private void SeedPoisonPool()
        {
            FillEllipse(Mathf.RoundToInt(worldWidth * 0.72f), Mathf.RoundToInt(worldHeight * 0.30f), 19, 7, MaterialType.Poison, true);
            FillEllipse(Mathf.RoundToInt(worldWidth * 0.62f), Mathf.RoundToInt(worldHeight * 0.43f), 10, 4, MaterialType.Poison, true);
        }

        private void SeedLavaPocket()
        {
            FillEllipse(Mathf.RoundToInt(worldWidth * 0.84f), Mathf.RoundToInt(worldHeight * 0.19f), 18, 8, MaterialType.Lava, true);
            BuildStoneRect(Mathf.RoundToInt(worldWidth * 0.84f) - 22, Mathf.RoundToInt(worldHeight * 0.19f) - 8, 44, 2);
        }

        private void BuildAlchemyRuins(int[] surfaceHeights)
        {
            int startX = Mathf.RoundToInt(worldWidth * 0.58f);
            int baseY = Mathf.Min(worldHeight - 24, surfaceHeights[startX] + 8);

            BuildWoodRect(startX, baseY, 43, 2);
            BuildWoodRect(startX + 5, baseY - 12, 3, 14);
            BuildWoodRect(startX + 22, baseY - 17, 3, 19);
            BuildWoodRect(startX + 38, baseY - 10, 3, 12);
            BuildWoodSlope(startX + 6, baseY - 1, startX + 23, baseY - 15, 2);
            BuildWoodSlope(startX + 24, baseY - 15, startX + 40, baseY - 1, 2);

            BuildWoodRect(startX + 50, baseY + 7, 4, 21);
            BuildWoodRect(startX + 44, baseY + 26, 16, 3);
            grid.PaintCircle(startX + 4, baseY + 3, 2, MaterialType.Fire);
        }

        private void SeedSmokePocket(int[] surfaceHeights)
        {
            int centerX = Mathf.RoundToInt(worldWidth * 0.62f);
            int centerY = Mathf.Min(worldHeight - 8, surfaceHeights[centerX] + 31);

            for (int x = centerX - 9; x <= centerX + 9; x++)
            {
                for (int y = centerY - 5; y <= centerY + 5; y++)
                {
                    if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) < 58)
                    {
                        grid.SetMaterial(x, y, MaterialType.Smoke);
                    }
                }
            }
        }

        private void PreparePlayerSpawn(int[] surfaceHeights)
        {
            int spawnX = Mathf.Clamp(worldWidth / 7, 10, worldWidth - 12);
            int spawnY = Mathf.Clamp(surfaceHeights[spawnX] + 16, 20, worldHeight - 16);
            CarveRoom(spawnX + 8, spawnY - 2, 27, 13);
            BuildStoneRect(spawnX - 10, spawnY - 11, 30, 3);
            playerSpawnCell = new Vector2Int(spawnX, spawnY);
        }

        private void FillEllipse(int centerX, int centerY, int radiusX, int radiusY, MaterialType material, bool lowerHalfOnly)
        {
            for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
            {
                if (lowerHalfOnly && y > centerY)
                {
                    continue;
                }

                for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
                {
                    float dx = (x - centerX) / (float)Mathf.Max(1, radiusX);
                    float dy = (y - centerY) / (float)Mathf.Max(1, radiusY);
                    if (dx * dx + dy * dy <= 1f)
                    {
                        grid.SetMaterial(x, y, material);
                    }
                }
            }
        }

        private void BuildStoneRect(int x, int y, int width, int height)
        {
            FillRect(x, y, width, height, MaterialType.Stone);
        }

        private void BuildWoodRect(int x, int y, int width, int height)
        {
            FillRect(x, y, width, height, MaterialType.Wood);
        }

        private void FillRect(int x, int y, int width, int height, MaterialType material)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    grid.SetMaterial(px, py, material);
                }
            }
        }

        private void BuildWoodSlope(int startX, int startY, int endX, int endY, int thickness)
        {
            int steps = Mathf.Max(1, Mathf.Abs(endX - startX));
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(startX, endX, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(startY, endY, t));
                FillRect(x, y, thickness, thickness, MaterialType.Wood);
            }
        }

        private float Hash01(int a, int b)
        {
            unchecked
            {
                uint n = (uint)(a * 73856093) ^ (uint)(b * 19349663) ^ ((uint)infiniteCaveSeed * 83492791u);
                n ^= n >> 13;
                n *= 1274126177u;
                n ^= n >> 16;
                return (n & 0x00FFFFFF) / 16777215f;
            }
        }

        private void OnDestroy()
        {
            if (generatedVolumeProfile != null)
            {
                DestroyObject(generatedVolumeProfile);
                generatedVolumeProfile = null;
            }
        }

        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
