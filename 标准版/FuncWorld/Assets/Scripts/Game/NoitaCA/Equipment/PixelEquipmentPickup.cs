using UnityEngine;

namespace NoitaCA
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PixelEquipmentPickup : MonoBehaviour
    {
        private PixelEquipmentDefinition definition;
        private PixelGrid grid;
        private PixelWorldRenderer renderer;
        private PlayerController player;
        private SpriteRenderer spriteRenderer;
        private Sprite generatedSprite;
        private Texture2D generatedTexture;

        public PixelEquipmentDefinition Definition => definition;

        public void Initialize(PixelEquipmentDefinition equipmentDefinition, PixelGrid pixelGrid, PixelWorldRenderer worldRenderer, PlayerController targetPlayer, Vector2Int spawnCell)
        {
            definition = equipmentDefinition;
            grid = pixelGrid;
            renderer = worldRenderer;
            player = targetPlayer;
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = BuildSprite();
            spriteRenderer.sortingOrder = 11;

            if (renderer != null)
            {
                transform.position = renderer.CellToWorldCenter(spawnCell.x, spawnCell.y);
            }
        }

        private void Update()
        {
            if (definition == null || player == null)
            {
                return;
            }

            float pickupDistance = definition.PickupRadius;
            if ((player.transform.position - transform.position).sqrMagnitude > pickupDistance * pickupDistance)
            {
                BobInMaterial();
                return;
            }

            PixelEquipmentController equipmentController = player.GetComponent<PixelEquipmentController>();
            if (equipmentController != null && equipmentController.Equip(definition))
            {
                if (grid != null && renderer != null)
                {
                    Vector2Int cell = renderer.WorldToCell(transform.position);
                    grid.SpawnMaterial(cell, MaterialType.Smoke, 4, 2);
                    grid.MarkActiveArea(cell.x, cell.y, 4);
                }

                Destroy(gameObject);
            }
        }

        private void BobInMaterial()
        {
            if (grid == null || renderer == null)
            {
                return;
            }

            Vector2Int cell = renderer.WorldToCell(transform.position);
            if (!grid.InBounds(cell.x, cell.y - 1) || !MaterialDatabase.Get(grid.GetMaterial(cell.x, cell.y - 1)).CanBeDisplaced)
            {
                return;
            }

            transform.position += Vector3.down * (Time.deltaTime * 0.2f);
        }

        private Sprite BuildSprite()
        {
            const int width = 9;
            const int height = 9;
            generatedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            generatedTexture.name = (definition != null ? definition.DisplayName : "Equipment") + " Pickup Texture";
            generatedTexture.filterMode = FilterMode.Point;
            generatedTexture.wrapMode = TextureWrapMode.Clamp;

            Color32[] colors = new Color32[width * height];
            Color32 transparent = new Color32(0, 0, 0, 0);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = transparent;
            }

            Color32 outline = new Color32(22, 18, 18, 255);
            Color32 shaft = new Color32(116, 74, 38, 255);
            Color32 core = definition != null ? definition.PickupColor : new Color32(236, 198, 92, 255);
            FillRect(colors, width, height, 4, 1, 1, 7, outline);
            FillRect(colors, width, height, 4, 2, 1, 5, shaft);
            FillRect(colors, width, height, 2, 6, 5, 1, outline);
            SetPixel(colors, width, height, 4, 8, outline);
            FillRect(colors, width, height, 3, 0, 3, 3, outline);
            SetPixel(colors, width, height, 4, 1, core);
            SetPixel(colors, width, height, 3, 1, core);
            SetPixel(colors, width, height, 5, 1, core);

            generatedTexture.SetPixels32(colors);
            generatedTexture.Apply(false);
            generatedSprite = Sprite.Create(generatedTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), renderer != null ? renderer.PixelsPerUnit : 16);
            generatedSprite.name = generatedTexture.name.Replace("Texture", "Sprite");
            return generatedSprite;
        }

        private static void FillRect(Color32[] colors, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color)
        {
            for (int py = y; py < y + rectHeight; py++)
            {
                for (int px = x; px < x + rectWidth; px++)
                {
                    SetPixel(colors, width, height, px, py, color);
                }
            }
        }

        private static void SetPixel(Color32[] colors, int width, int height, int x, int y, Color32 color)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            colors[y * width + x] = color;
        }

        private void OnDestroy()
        {
            if (generatedSprite != null)
            {
                DestroyObject(generatedSprite);
            }

            if (generatedTexture != null)
            {
                DestroyObject(generatedTexture);
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
