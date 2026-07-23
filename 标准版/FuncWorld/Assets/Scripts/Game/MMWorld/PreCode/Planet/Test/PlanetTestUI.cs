using UnityEngine;
using System.Collections.Generic;
using MMWorld.HexSphere;

namespace MMWorld
{
    /// <summary>
    /// Planet 框架测试 UI 面板（OnGUI 实现，零依赖）。
    /// 功能：
    /// - 地形类型切换（Random / Perlin）
    /// - 选中 Tile 高度调节
    /// - 多选 Tile 批量高度调节
    /// - 当前选中信息显示
    /// - 一键清空选中 / 重建星球
    /// </summary>
    public class PlanetTestUI : MonoBehaviour
    {
        private Vector2 scrollPos;
        private float heightSlider = 2.5f;
        private bool showPanel = true;
        private bool uiNeedsRepaint;

        private HexPlanetController controller;

        private void Start()
        {
            controller = HexPlanetController.Instance;
            if (controller != null)
            {
                controller.onTileSelected += OnTileSelected;
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

            GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20), "Planet 测试面板", GUI.skin.window);
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
                GUILayout.Label("HexPlanetController 未就绪");
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
                controller.SetTerrainType(HexPlanetController.TerrainGeneratorType.Random);
            }
            if (GUILayout.Button("切换到 Perlin 地形"))
            {
                controller.SetTerrainType(HexPlanetController.TerrainGeneratorType.Perlin);
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
            foreach (HexTile t in sel)
            {
                // 遍历调用 SetHeight，但批量的话直接 SetSelectedTilesHeight 更高效
                break;
            }
            // 用一个笨办法：每个 tile 单独设高度
            // 实际上 controller 只有 SetSelectedTilesHeight(newHeight)，所以我们先读一个基准再加减
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
    }
}
