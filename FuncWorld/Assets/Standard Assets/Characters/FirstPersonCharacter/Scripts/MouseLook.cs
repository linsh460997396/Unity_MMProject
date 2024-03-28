using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    /// <summary>
    /// ���������ת���벢Ӧ���ڽ�ɫ�������ת��Unity�ű������������ʹ���������ת��ɫ����������ṩ�˶�������ѡ���ƽ����ת����ֱ��ת���ƺ������������
    /// </summary>
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f; //�����ȣ����ڵ�������ƶ�����ת��Ӱ��
        public float YSensitivity = 2f; //�����ȣ����ڵ�������ƶ�����ת��Ӱ��
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
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

            if(clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);

            if(smooth)
            {
                character.localRotation = Quaternion.Slerp (character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot,
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
            if(!lockCursor)
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
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                m_cursorIsLocked = false;
            }
            else if(Input.GetMouseButtonUp(0))
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

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}

// ���MouseLook����һ�����������ת���벢Ӧ���ڽ�ɫ�������ת��Unity�ű������������ʹ���������ת��ɫ����������ṩ�˶�������ѡ���ƽ����ת����ֱ��ת���ƺ������������

// �����ǹ�������༰�䷽������ϸ���ͣ�

// ��Ա����
// XSensitivity �� YSensitivity������������ֵ�ֱ����ˮƽ�ʹ�ֱ�����ϵ���������ȡ�
// clampVerticalRotation��һ������ֵ������ȷ���Ƿ�Ҫ��������Ĵ�ֱ��ת��
// MinimumX �� MaximumX����clampVerticalRotationΪtrueʱ��������ֵ�����������X���ϵ���С�������ת�Ƕȡ�
// smooth��һ������ֵ������ȷ����ת�Ƿ�Ӧ����ƽ���ġ�
// smoothTime��ƽ����ת��ʱ�䣨����Ϊ��λ����
// lockCursor��һ������ֵ������ȷ���Ƿ�Ӧ����������ꡣ
// m_CharacterTargetRot �� m_CameraTargetRot��������Quaternion�����洢�˽�ɫ�������Ŀ����ת��
// m_cursorIsLocked��һ��˽�в���ֵ����ʾ��ǰ������Ƿ�������
// ����

// Init(Transform character, Transform camera)

// ��ʼ���������������ý�ɫ������ĳ�ʼ��ת��

// LookRotation(Transform character, Transform camera)

// ��Ҫ���������ڴ���������벢���½�ɫ���������ת��
// ��CrossPlatformInputManager��ȡ����X��Y�����롣
// ʹ��Quaternion.Euler�ͻ�ȡ��������ֵ�����½�ɫ�������Ŀ����ת��
// ���clampVerticalRotationΪtrue����ʹ��ClampRotationAroundXAxis��������������Ĵ�ֱ��ת��
// ����smooth��ֵ��ʹ��Quaternion.Slerp��ֱ�Ӹ�ֵ��ƽ�����������½�ɫ���������ת��
// ����UpdateCursorLock����������������״̬��

// SetCursorLock(bool value)

// ����lockCursor��ֵ����������ֵ���������������ꡣ

// UpdateCursorLock()

// һ��˽�з��������ڸ���lockCursor��ֵ���������������ꡣ
// ���lockCursorΪfalse��������������״̬����ΪCursorLockMode.None������ʾ����ꡣ

// �����ͨ�����ڵ�һ�˳ƻ�����˳������Ϸ�У���ҿ���ʹ����������ɵ���ת�ӽǺͽ�ɫ��ͨ��������ĳ�Ա�����������߿��Զ�����ת��Ϊ����������Ϸ������
