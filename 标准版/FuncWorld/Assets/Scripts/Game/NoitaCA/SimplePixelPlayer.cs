// 职责：提供演示用像素玩家控制器，支持左右移动、跳跃和基于材料的碰撞检测。
// Responsibility: Provides a demo pixel-player controller with movement, jumping, and material-based collision checks.
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NoitaCA
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SimplePixelPlayer : MonoBehaviour
    {
        // 玩家尺寸以像素格为单位，方便与 PixelGrid 碰撞对齐。
        // Player size is measured in grid cells so collision aligns with PixelGrid.
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float jumpSpeed = 7.2f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private int widthInCells = 5;
        [SerializeField] private int heightInCells = 10;

        private PixelGrid grid;
        private PixelWorldRenderer worldRenderer;
        private SpriteRenderer spriteRenderer;
        private Sprite sprite;
        private Texture2D texture;
        private Vector2 velocity;
        private bool grounded;

        public void Initialize(PixelGrid pixelGrid, PixelWorldRenderer renderer, Vector2Int spawnCell)
        {
            // 玩家依赖已有网格和渲染器定位，不直接拥有世界数据。
            // The player depends on an existing grid and renderer for positioning; it does not own world data.
            grid = pixelGrid;
            worldRenderer = renderer;
            spriteRenderer = GetComponent<SpriteRenderer>();
            BuildSprite();
            transform.position = worldRenderer.CellToWorldCenter(spawnCell.x, spawnCell.y);
        }

        private void Update()
        {
            if (grid == null || worldRenderer == null)
            {
                return;
            }

            // 输入只决定目标水平速度，实际移动会经过格子碰撞裁剪。
            // Input sets desired horizontal velocity; actual motion is clipped by grid collision.
            float horizontal = ReadHorizontal();
            velocity.x = horizontal * moveSpeed;

            if (grounded && IsJumpPressed())
            {
                // 只有接地时允许起跳。
                // Jumping is allowed only while grounded.
                velocity.y = jumpSpeed;
                grounded = false;
            }

            velocity.y += gravity * Time.deltaTime;
            // 使用本帧位移推进，并在内部处理碰撞。
            // Advance by this frame's displacement and resolve collision internally.
            MoveWithGridCollision(velocity * Time.deltaTime);
        }

        private void MoveWithGridCollision(Vector2 delta)
        {
            // 分轴移动让水平和垂直碰撞响应更稳定。
            // Axis-separated movement keeps horizontal and vertical collision response stable.
            grounded = false;
            MoveAxis(new Vector2(delta.x, 0f));
            MoveAxis(new Vector2(0f, delta.y));
        }

        private void MoveAxis(Vector2 delta)
        {
            if (delta.sqrMagnitude <= 0f)
            {
                return;
            }

            float cellWorldSize = 1f / Mathf.Max(1, worldRenderer.PixelsPerUnit);
            // 将长位移拆成小步，避免高速时穿过一格厚的墙体。
            // Split long movement into small steps to avoid tunneling through one-cell-thick walls.
            int steps = Mathf.Max(1, Mathf.CeilToInt(delta.magnitude / (cellWorldSize * 0.45f)));
            Vector2 step = delta / steps;

            for (int i = 0; i < steps; i++)
            {
                Vector3 nextPosition = transform.position + (Vector3)step;
                if (OverlapsSolid(nextPosition))
                {
                    // 撞到垂直面时清水平速度，撞到地面/天花板时清垂直速度。
                    // Clear horizontal velocity on side hits and vertical velocity on floor/ceiling hits.
                    if (Mathf.Abs(step.y) > Mathf.Abs(step.x))
                    {
                        if (step.y < 0f)
                        {
                            grounded = true;
                        }

                        velocity.y = 0f;
                    }
                    else
                    {
                        velocity.x = 0f;
                    }

                    return;
                }

                transform.position = nextPosition;
            }
        }

        private bool OverlapsSolid(Vector3 worldPosition)
        {
            // 将玩家包围盒转换为覆盖的网格范围。
            // Convert the player's bounds into the grid-cell range it overlaps.
            float ppu = Mathf.Max(1, worldRenderer.PixelsPerUnit);
            Vector2 halfSize = new Vector2(widthInCells / ppu, heightInCells / ppu) * 0.5f;
            Vector2 min = (Vector2)worldPosition - halfSize;
            Vector2 max = (Vector2)worldPosition + halfSize;

            Vector2Int minCell = worldRenderer.WorldToCell(min);
            Vector2Int maxCell = worldRenderer.WorldToCell(max);

            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                for (int x = minCell.x; x <= maxCell.x; x++)
                {
                    if (!grid.InBounds(x, y))
                    {
                        continue;
                    }

                    MaterialDefinition definition = MaterialDatabase.Get(grid.GetCell(x, y).MaterialType);
                    if (definition.BlocksPlayer)
                    {
                        // 只有材料定义为阻挡玩家时才算碰撞。
                        // Collision occurs only when the material definition blocks the player.
                        return true;
                    }
                }
            }

            return false;
        }

        private void BuildSprite()
        {
            if (spriteRenderer.sprite != null)
            {
                return;
            }

            // 运行时生成一个极小像素风角色贴图，避免额外美术资源依赖。
            // Generates a tiny pixel-art character at runtime to avoid extra art dependencies.
            texture = new Texture2D(8, 12, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            Color32[] colors = new Color32[8 * 12];

            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    bool border = x == 0 || x == 7 || y == 0 || y == 11;
                    bool visor = y >= 7 && y <= 8 && x >= 2 && x <= 5;
                    colors[y * 8 + x] = border
                        ? new Color32(18, 22, 28, 255)
                        : visor
                            ? new Color32(112, 210, 255, 255)
                            : new Color32(238, 221, 170, 255);
                }
            }

            texture.SetPixels32(colors);
            texture.Apply(false);
            sprite = Sprite.Create(texture, new Rect(0, 0, 8, 12), new Vector2(0.5f, 0.5f), worldRenderer.PixelsPerUnit);
            sprite.name = "Pixel Demo Player";
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = 10;
        }

        private static float ReadHorizontal()
        {
            // 同时支持 WASD、方向键和旧输入轴。
            // Supports WASD, arrow keys, and the legacy input axis.
            float horizontal = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                {
                    horizontal -= 1f;
                }

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                {
                    horizontal += 1f;
                }

                return Mathf.Clamp(horizontal, -1f, 1f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
#endif
            return Mathf.Clamp(horizontal, -1f, 1f);
        }

        private static bool IsJumpPressed()
        {
            // 跳跃键兼容新旧输入系统。
            // Jump keys are compatible with both new and legacy input systems.
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.spaceKey.wasPressedThisFrame
                    || Keyboard.current.wKey.wasPressedThisFrame
                    || Keyboard.current.upArrowKey.wasPressedThisFrame;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(KeyCode.Space)
                || UnityEngine.Input.GetKeyDown(KeyCode.W)
                || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow);
#else
            return false;
#endif
        }

        private void OnDestroy()
        {
            // 释放运行时创建的玩家精灵资源。
            // Release runtime-created player sprite resources.
            if (sprite != null)
            {
                Destroy(sprite);
            }

            if (texture != null)
            {
                Destroy(texture);
            }
        }
    }
}
