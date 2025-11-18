#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 处理鼠标旋转输入并应用于角色和相机旋转的Unity脚本.它允许玩家使用鼠标来旋转角色和相机,并提供了多种配置选项,如平滑旋转、垂直旋转限制和鼠标光标锁定.
    /// </summary>
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f; //灵敏度,用于调整鼠标移动对旋转的影响
        public float YSensitivity = 2f; //灵敏度,用于调整鼠标移动对旋转的影响
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
        }


        public void LookRotation(Transform character, Transform camera)
        {
            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            UpdateCursorLock();
        }

        public void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (!lockCursor)
            {//we force unlock the cursor if the user disable the cursor locking helper
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void UpdateCursorLock()
        {
            //if the user set "lockCursor" we check & properly lock the cursos
            if (lockCursor)
                InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_cursorIsLocked = true;
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}

// 这个MouseLook类是一个处理鼠标旋转输入并应用于角色和相机旋转的Unity脚本.它允许玩家使用鼠标来旋转角色和相机,并提供了多种配置选项,如平滑旋转、垂直旋转限制和鼠标光标锁定.

// 下面是关于这个类及其方法的详细解释：

// 成员变量
// XSensitivity 和 YSensitivity：这两个浮点值分别控制水平和垂直方向上的鼠标灵敏度.
// clampVerticalRotation：一个布尔值,用于确定是否要限制相机的垂直旋转.
// MinimumX 和 MaximumX：当clampVerticalRotation为true时,这两个值定义了相机在X轴上的最小和最大旋转角度.
// smooth：一个布尔值,用于确定旋转是否应该是平滑的.
// smoothTime：平滑旋转的时间（以秒为单位).
// lockCursor：一个布尔值,用于确定是否应该锁定鼠标光标.
// m_CharacterTargetRot 和 m_CameraTargetRot：这两个Quaternion变量存储了角色和相机的目标旋转.
// m_cursorIsLocked：一个私有布尔值,表示当前鼠标光标是否锁定.
// 方法

// Init(Transform character, Transform camera)

// 初始化方法,用于设置角色和相机的初始旋转.

// LookRotation(Transform character, Transform camera)

// 主要方法,用于处理鼠标输入并更新角色和相机的旋转.
// 从CrossPlatformInputManager获取鼠标的X和Y轴输入.
// 使用Quaternion.Euler和获取到的输入值来更新角色和相机的目标旋转.
// 若clampVerticalRotation为true,则使用ClampRotationAroundXAxis方法来限制相机的垂直旋转.
// 根据smooth的值,使用Quaternion.Slerp或直接赋值来平滑或立即更新角色和相机的旋转.
// 调用UpdateCursorLock来更新鼠标光标的锁定状态.

// SetCursorLock(bool value)

// 设置lockCursor的值,并根据其值来锁定或解锁鼠标光标.

// UpdateCursorLock()

// 一个私有方法,用于根据lockCursor的值来锁定或解锁鼠标光标.
// 若lockCursor为false,则将鼠标光标的锁定状态设置为CursorLockMode.None,并显示鼠标光标.

// 这个类通常用于第一人称或第三人称射击游戏中,玩家可以使用鼠标来自由地旋转视角和角色.通过调整类的成员变量,开发者可以定制旋转行为,以满足游戏的需求.

#endif