using UnityEngine;

//sends CellEvents such as OnLook, OnMouseDown, etc.���������¼���OnLook�� OnMouseDown��
namespace CellSpace
{
    /// <summary>
    /// �������������ͷע�ӣ������ָ�룩�¼����͹�����ɾ���ؿ飬Ҳ������ѡ����ʵʱλ�ø��¡�
    /// ����÷���Unity������½�һ���ն���Manager�����ѽű��ϵ����λ�ü����أ�UnityҪ��һ��cs�ļ�ֻ��һ���࣬�����������ļ���һ�£�
    /// </summary>
    public class CPCameraEventsSender : MonoBehaviour
    {
        //Unity�Թ����ֶλ�Ĭ�����л�����ʾ�ڹ�����Ϸ�����GUI���棨���ڸý�����������ɳ�ʼ��ֵ��
        public float CameraLookRange;
        private GameObject SelectedBlockGraphics;

        public void Awake()
        {
            if (CameraLookRange <= 0)
            {
                Debug.LogWarning("CellSpace: CameraEventSender.CameraLookRange must be greater than 0. Setting CameraLookRange to 5." +
                    "Range�������0����Ĭ������5");
                CameraLookRange = 5.0f;
            }

            SelectedBlockGraphics = GameObject.Find("SelectedBox");
        }

        public void Update()
        {

            if (CPEngine.SendCameraLookEvents)
            {
                CameraLookEvents();
            }
            if (CPEngine.SendCursorEvents)
            {
                MouseCursorEvents();
            }
        }

        private void MouseCursorEvents()
        { // cursor position
            //Vector3F pos = new Vector3F (Input.mousePosition.pixelX, Input.mousePosition.pixelY, 10f);
            CellInfo raycast = CPEngine.CellRaycast(Camera.main.ScreenPointToRay(Input.mousePosition), 9999f, false);
            if (raycast != null)
            {
                // create a local copy of the hit voxel so we can call functions on it
                GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(raycast.GetCellID()));
                // only execute this if the voxel actually has any events (either CellEvents component, or any component that inherits from it)
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    cellObject.GetComponent<CellEvents>().OnLook(raycast);
                    // for all mouse buttons, send events
                    for (int i = 0; i < 3; i++)
                    {
                        if (Input.GetMouseButtonDown(i))
                        {
                            cellObject.GetComponent<CellEvents>().OnMouseDown(i, raycast);
                        }
                        if (Input.GetMouseButtonUp(i))
                        {
                            cellObject.GetComponent<CellEvents>().OnMouseUp(i, raycast);
                        }
                        if (Input.GetMouseButton(i))
                        {
                            cellObject.GetComponent<CellEvents>().OnMouseHold(i, raycast);
                        }
                    }
                }
                Destroy(cellObject);
            }
            else
            {
                // disable selected block ui when no block is hit
                if (SelectedBlockGraphics != null)
                {
                    SelectedBlockGraphics.GetComponent<Renderer>().enabled = false;
                }
            }
        }

        private void CameraLookEvents()
        { // first person camera
            CellInfo raycast = CPEngine.CellRaycast(Camera.main.transform.position, Camera.main.transform.forward, CameraLookRange, false);
            if (raycast != null)
            {
                // create a local copy of the hit voxel so we can call functions on it
                GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(raycast.GetCellID()));
                // only execute this if the voxel actually has any events (either CellEvents component, or any component that inherits from it)
                if (cellObject.GetComponent<CellEvents>() != null)
                {
                    cellObject.GetComponent<CellEvents>().OnLook(raycast);
                    // for all mouse buttons, send events
                    for (int i = 0; i < 3; i++)
                    {
                        if (Input.GetMouseButtonDown(i))
                        {
                            cellObject.GetComponent<CellEvents>().OnMouseDown(i, raycast);
                        }
                        if (Input.GetMouseButtonUp(i))
                        {
                            cellObject.GetComponent<CellEvents>().OnMouseUp(i, raycast);
                        }
                        if (Input.GetMouseButton(i))
                        {
                            cellObject.GetComponent<CellEvents>().OnMouseHold(i, raycast);
                        }
                    }
                }
                Destroy(cellObject);
            }
            else
            {
                // disable selected block ui when no block is hit
                if (SelectedBlockGraphics != null)
                {
                    SelectedBlockGraphics.GetComponent<Renderer>().enabled = false;
                }
            }
        }
    }
}