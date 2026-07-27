using UnityEngine;
using System.Collections.Generic;
using UnityHexPlanet;

namespace MMWorld
{
    /// <summary>
    /// 星球UI面板 - 管理星球参数菜单和交互面板。
    /// 功能：
    /// - Init方法在指定位置创建星球
    /// - 地形类型切换（Random / Perlin）
    /// - 选中 Tile 高度调节
    /// - 多选 Tile 批量高度调节
    /// - 当前选中信息显示
    /// - 一键清空选中 / 重建星球
    /// </summary>
    public class PlanetUI : MonoBehaviour
    {
        private Vector2 scrollPos;
        private float heightSlider = 2.5f;
        private bool showPanel = true;
        private bool uiNeedsRepaint;

        private PlanetController controller;

        public System.Action<HexTile> onTileSelected;

        public void Init(Vector3 position, PlanetController.TerrainGeneratorType terrainType = PlanetController.TerrainGeneratorType.Perlin,
            float planetRadius = 50f, int subdivisions = 3, int chunkSubdivisions = 2,
            Color basePlanetColor = default, float minOrbitRadius = 70f, float maxOrbitRadius = 300f)
        {
            if (basePlanetColor == default)
            {
                basePlanetColor = new Color(0.4f, 0.6f, 0.3f);
            }

            GameObject planetRoot = new GameObject("PlanetRoot");
            planetRoot.transform.position = position;

            planetRoot.AddComponent<HexPlanetManager>();
            controller = planetRoot.AddComponent<PlanetController>();

            SetPrivateField(controller, "terrainType", terrainType);
            SetPrivateField(controller, "planetRadius", planetRadius);
            SetPrivateField(controller, "subdivisions", subdivisions);
            SetPrivateField(controller, "chunkSubdivisions", chunkSubdivisions);
            SetPrivateField(controller, "basePlanetColor", basePlanetColor);
            SetPrivateField(controller, "minOrbitRadius", minOrbitRadius);
            SetPrivateField(controller, "maxOrbitRadius", maxOrbitRadius);

            if (controller != null)
            {
                controller.onTileSelected += OnTileSelected;
            }

            Debug.Log("[PlanetUI] 星球初始化完成");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance);
            if (f != null)
            {
                f.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[PlanetUI] 字段不存在: {fieldName}");
            }
        }

        private void Awake()
        {
            if (controller == null)
            {
                controller = PlanetController.Instance;
            }
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.onTileSelected -= OnTileSelected;
            }
        }

        private void OnTileSelected(HexTile tile)
        {
            uiNeedsRepaint = true;
            onTileSelected?.Invoke(tile);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                showPanel = !showPanel;
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20), "Planet 控制面板", GUI.skin.window);
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            DrawStatusSection();
            GUILayout.Space(8);
            DrawTerrainSection();
            GUILayout.Space(8);
            DrawHeightSection();
            GUILayout.Space(8);
            DrawSelectionSection();
            GUILayout.Space(8);
            DrawActionsSection();
            GUILayout.Space(8);
            DrawHelpSection();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawStatusSection()
        {
            GUILayout.Label("=== 状态 ===", GUILayout.Height(20));
            if (controller == null)
            {
                GUILayout.Label("PlanetController 未就绪");
                return;
            }
            HexPlanet hp = controller.GetHexPlanet();
            if (hp != null)
            {
                GUILayout.Label($"半径: {hp.radius:F1}");
                GUILayout.Label($"细分: {hp.subdivisions}");
                GUILayout.Label($"瓦片总数: {hp.tiles?.Count ?? 0}");
                GUILayout.Label($"Chunk 数量: {hp.chunks?.Count ?? 0}");
            }
            GUILayout.Label($"当前地形: {GetTerrainTypeName()}");
        }

        private string GetTerrainTypeName()
        {
            if (controller == null) return "N/A";
            HexPlanet hp = controller.GetHexPlanet();
            if (hp == null || hp.terrainGenerator == null) return "None";
            return hp.terrainGenerator.GetType().Name;
        }

        private void DrawTerrainSection()
        {
            GUILayout.Label("=== 地形切换 ===", GUILayout.Height(20));
            if (controller == null) return;

            if (GUILayout.Button("切换到 Random 地形"))
            {
                controller.SetTerrainType(PlanetController.TerrainGeneratorType.Random);
            }
            if (GUILayout.Button("切换到 Perlin 地形"))
            {
                controller.SetTerrainType(PlanetController.TerrainGeneratorType.Perlin);
            }
            if (GUILayout.Button("重建星球（重新生成）"))
            {
                controller.RebuildPlanet();
            }
        }

        private void DrawHeightSection()
        {
            GUILayout.Label("=== 高度调节 ===", GUILayout.Height(20));
            if (controller == null) return;

            GUILayout.Label($"目标高度: {heightSlider:F2}");
            heightSlider = GUILayout.HorizontalSlider(heightSlider, -5f, 15f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("设置主选中"))
            {
                controller.SetSelectedTileHeight(heightSlider);
            }
            if (GUILayout.Button("设置全部选中"))
            {
                controller.SetSelectedTilesHeight(heightSlider);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+0.5"))
            {
                AdjustSelectedHeight(0.5f);
            }
            if (GUILayout.Button("-0.5"))
            {
                AdjustSelectedHeight(-0.5f);
            }
            GUILayout.EndHorizontal();
        }

        private void AdjustSelectedHeight(float delta)
        {
            IReadOnlyCollection<HexTile> sel = controller.GetSelectedTiles();
            if (sel == null || sel.Count == 0)
            {
                HexTile main = controller.GetSelectedTile();
                if (main != null)
                {
                    controller.SetSelectedTileHeight(main.height + delta);
                }
                return;
            }
            using (var e = sel.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    float targetH = e.Current.height + delta;
                    controller.SetSelectedTilesHeight(targetH);
                }
            }
        }

        private void DrawSelectionSection()
        {
            GUILayout.Label("=== 选中信息 ===", GUILayout.Height(20));
            if (controller == null) return;

            HexTile main = controller.GetSelectedTile();
            IReadOnlyCollection<HexTile> all = controller.GetSelectedTiles();

            if (main != null)
            {
                GUILayout.Label($"主选中: Tile #{main.id}");
                GUILayout.Label($"  高度: {main.height:F3}");
                var nbrs = main.GetNeighbors();
                GUILayout.Label($"  邻居数: {nbrs?.Count ?? 0}");
            }
            else
            {
                GUILayout.Label("主选中: 无");
            }

            GUILayout.Label($"多选数量: {all?.Count ?? 0}");
        }

        private void DrawActionsSection()
        {
            GUILayout.Label("=== 操作 ===", GUILayout.Height(20));
            if (controller == null) return;

            if (GUILayout.Button("清空选中"))
            {
                controller.ClearSelection();
            }
            if (GUILayout.Button("刷新渲染"))
            {
                controller.RefreshPlanet();
            }
        }

        private void DrawHelpSection()
        {
            GUILayout.Label("=== 操作说明 ===", GUILayout.Height(20));
            GUILayout.Label("H: 显示/隐藏面板");
            GUILayout.Label("左键拖拽: 旋转星球");
            GUILayout.Label("滚轮: 缩放");
            GUILayout.Label("左键单击: 选中瓦片");
            GUILayout.Label("Shift+单击: 追加多选");
            GUILayout.Label("Ctrl+单击: 切换选中");
        }

        public PlanetController GetController()
        {
            return controller;
        }

        public void SetShowPanel(bool show)
        {
            showPanel = show;
        }
    }
}