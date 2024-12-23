using UnityEngine;

//sends CellEvents such as OnLook, OnMouseDown, etc.发送体素事件如OnLook、 OnMouseDown等
namespace CellSpace
{
    /// <summary>
    /// 利用主摄像机镜头注视（含鼠标指针）事件发送功能增删体素块，也管理着选择框的实时位置更新。
    /// 组件用法：Unity中随便新建一个空对象“Manager”，把脚本拖到组件位置即挂载（Unity要求一个cs文件只能一个类，且类名须与文件名一致）
    /// </summary>
    public class CPCameraEventsSender : MonoBehaviour
    {
        //Unity对公开字段会默认序列化，显示在挂载游戏物体的GUI界面（可在该界面输入来完成初始赋值）
        public float CameraLookRange;
        private GameObject SelectedBlockGraphics;

        public void Awake()
        {
            if (CameraLookRange <= 0)
            {
                Debug.LogWarning("CellSpace: CameraEventSender.CameraLookRange must be greater than 0. Setting CameraLookRange to 5." +
                    "Range必须大于0，将默认设置5");
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