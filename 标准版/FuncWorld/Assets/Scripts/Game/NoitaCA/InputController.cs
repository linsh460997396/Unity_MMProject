// 职责：读取鼠标、滚轮和数字键输入，用于绘制材料、擦除材料和缩放相机。
// Responsibility: Reads mouse, scroll, and number-key input to paint materials, erase materials, and zoom the camera.
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NoitaCA
{
    public sealed class InputController : MonoBehaviour
    {
        // 画笔和相机参数由 Inspector 调节，运行时会被安全夹取。
        // Brush and camera values are tuned in the Inspector and clamped at runtime.
        [SerializeField] private int brushSize = 2;
        [SerializeField] private int minBrushSize = 1;
        [SerializeField] private int maxBrushSize = 16;
        [SerializeField] private float zoomSpeed = 1.2f;
        [SerializeField] private float minOrthographicSize = 1.5f;
        [SerializeField] private float maxOrthographicSize = 40f;
        [SerializeField] private MaterialType selectedMaterial = MaterialType.Water;

        private PixelGrid grid;
        private PixelWorldRenderer worldRenderer;
        private Camera targetCamera;

        public int BrushSize => brushSize;
        public MaterialType SelectedMaterial => selectedMaterial;

        public void Initialize(PixelGrid worldGrid, PixelWorldRenderer renderer, Camera cameraToControl)
        {
            // 输入控制器只保存外部引用，不创建世界对象。
            // The input controller only stores external references; it does not create world objects.
            grid = worldGrid;
            worldRenderer = renderer;
            targetCamera = cameraToControl;
            brushSize = Mathf.Clamp(brushSize, minBrushSize, maxBrushSize);
        }

        public void Tick()
        {
            if (grid == null || worldRenderer == null || targetCamera == null)
            {
                return;
            }

            // 滚轮默认调画笔，按住 Ctrl/Command 时改为缩放相机。
            // Scroll changes brush size by default; Ctrl/Command turns it into camera zoom.
            float scroll = ReadScrollDelta();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                if (IsZoomModifierHeld())
                {
                    ZoomCamera(scroll);
                }
                else
                {
                    ResizeBrush(scroll);
                }
            }

            ReadMaterialHotkeys();

            if (IsPrimaryButtonPressed())
            {
                // 左键绘制当前选中材料。
                // Left mouse paints the selected material.
                PaintAtPointer(selectedMaterial);
            }

            if (IsSecondaryButtonPressed())
            {
                // 右键绘制空气，相当于橡皮擦。
                // Right mouse paints air, acting as an eraser.
                PaintAtPointer(MaterialType.Air);
            }
        }

        private void PaintAtPointer(MaterialType materialType)
        {
            // 屏幕坐标通过相机转世界坐标，再由渲染器转成网格坐标。
            // Screen coordinates go through camera-to-world conversion, then renderer-to-grid conversion.
            Vector3 screenPosition = ReadPointerScreenPosition();
            screenPosition.z = Mathf.Abs(targetCamera.transform.position.z - worldRenderer.transform.position.z);

            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
            Vector2Int cell = worldRenderer.WorldToCell(worldPosition);
            grid.PaintCircle(cell.x, cell.y, brushSize, materialType);
        }

        private void ReadMaterialHotkeys()
        {
            // 数字键快速切换绘制材料。
            // Number keys quickly switch the painting material.
            if (WasKeyPressed(KeyCode.Alpha1))
            {
                selectedMaterial = MaterialType.Sand;
            }
            else if (WasKeyPressed(KeyCode.Alpha2))
            {
                selectedMaterial = MaterialType.Water;
            }
            else if (WasKeyPressed(KeyCode.Alpha3))
            {
                selectedMaterial = MaterialType.Smoke;
            }
            else if (WasKeyPressed(KeyCode.Alpha4))
            {
                selectedMaterial = MaterialType.Fire;
            }
            else if (WasKeyPressed(KeyCode.Alpha5))
            {
                selectedMaterial = MaterialType.Stone;
            }
            else if (WasKeyPressed(KeyCode.Alpha6))
            {
                selectedMaterial = MaterialType.Wood;
            }
            else if (WasKeyPressed(KeyCode.Alpha7))
            {
                selectedMaterial = MaterialType.Poison;
            }
            else if (WasKeyPressed(KeyCode.Alpha8))
            {
                selectedMaterial = MaterialType.Ice;
            }
            else if (WasKeyPressed(KeyCode.Alpha9))
            {
                selectedMaterial = MaterialType.Lava;
            }
            else if (WasKeyPressed(KeyCode.Alpha0))
            {
                selectedMaterial = MaterialType.Air;
            }
        }

        private void ResizeBrush(float scroll)
        {
            // 滚轮方向只改变一档，避免不同设备滚轮幅度造成跳变。
            // Scroll direction changes one step only, avoiding device-dependent jump sizes.
            int direction = scroll > 0f ? 1 : -1;
            brushSize = Mathf.Clamp(brushSize + direction, minBrushSize, maxBrushSize);
        }

        private void ZoomCamera(float scroll)
        {
            // 正滚轮缩小正交尺寸以实现放大，结果限制在配置范围内。
            // Positive scroll reduces orthographic size to zoom in, clamped to configured bounds.
            targetCamera.orthographicSize = Mathf.Clamp(
                targetCamera.orthographicSize - scroll * zoomSpeed,
                minOrthographicSize,
                maxOrthographicSize);
        }

        private static Vector3 ReadPointerScreenPosition()
        {
            // 同时兼容新 Input System 和旧 Input Manager。
            // Supports both the new Input System and the legacy Input Manager.
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                Vector2 position = Mouse.current.position.ReadValue();
                return new Vector3(position.x, position.y, 0f);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.mousePosition;
#else
            return Vector3.zero;
#endif
        }

        private static bool IsPrimaryButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.leftButton.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButton(0);
#else
            return false;
#endif
        }

        private static bool IsSecondaryButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.rightButton.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButton(1);
#else
            return false;
#endif
        }

        private static float ReadScrollDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.scroll.ReadValue().y / 120f;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.mouseScrollDelta.y;
#else
            return 0f;
#endif
        }

        private static bool IsZoomModifierHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return Keyboard.current.leftCtrlKey.isPressed
                    || Keyboard.current.rightCtrlKey.isPressed
                    || Keyboard.current.leftCommandKey.isPressed
                    || Keyboard.current.rightCommandKey.isPressed;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKey(KeyCode.LeftControl)
                || UnityEngine.Input.GetKey(KeyCode.RightControl)
                || UnityEngine.Input.GetKey(KeyCode.LeftCommand)
                || UnityEngine.Input.GetKey(KeyCode.RightCommand);
#else
            return false;
#endif
        }

        private static bool WasKeyPressed(KeyCode keyCode)
        {
            // 新输入系统没有直接接收 KeyCode，这里显式映射演示所需按键。
            // The new Input System does not consume KeyCode directly, so demo keys are mapped explicitly.
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                switch (keyCode)
                {
                    case KeyCode.Alpha0:
                        return Keyboard.current.digit0Key.wasPressedThisFrame;
                    case KeyCode.Alpha1:
                        return Keyboard.current.digit1Key.wasPressedThisFrame;
                    case KeyCode.Alpha2:
                        return Keyboard.current.digit2Key.wasPressedThisFrame;
                    case KeyCode.Alpha3:
                        return Keyboard.current.digit3Key.wasPressedThisFrame;
                    case KeyCode.Alpha4:
                        return Keyboard.current.digit4Key.wasPressedThisFrame;
                    case KeyCode.Alpha5:
                        return Keyboard.current.digit5Key.wasPressedThisFrame;
                    case KeyCode.Alpha6:
                        return Keyboard.current.digit6Key.wasPressedThisFrame;
                    case KeyCode.Alpha7:
                        return Keyboard.current.digit7Key.wasPressedThisFrame;
                    case KeyCode.Alpha8:
                        return Keyboard.current.digit8Key.wasPressedThisFrame;
                    case KeyCode.Alpha9:
                        return Keyboard.current.digit9Key.wasPressedThisFrame;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }
    }
}
