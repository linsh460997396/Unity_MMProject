using UnityEngine;

// sends VoxelEvents such as OnLook, OnMouseDown, etc.发送体素事件如OnLook、 OnMouseDown等

namespace Uniblocks
{
    /// <summary>
    /// 利用主摄像机镜头注视(含鼠标指针)事件发送功能增删体素块,也管理着选择框的实时位置更新.
    /// 组件用法:Unity中随便新建一个空对象“Manager”,把脚本拖到组件位置即挂载(Unity要求一个cs文件只能一个类,且类名须与文件名一致)
    /// </summary>
    public class CameraEventsSender : MonoBehaviour
    {

        //Unity对公开字段会默认序列化,显示在挂载游戏物体的GUI界面(可在该界面输入来完成初始赋值)

        public float CameraLookRange;
        private GameObject SelectedBlockGraphics;

        public void Awake()
        {
            if (CameraLookRange <= 0)
            {
                Debug.LogWarning("Uniblocks: CameraEventSender.CameraLookRange must be greater than 0. Setting CameraLookRange to 5." +
                    "Range必须大于0,将默认设置范围:5");
                CameraLookRange = 5.0f;
            }

            SelectedBlockGraphics = GameObject.Find("SelectedBox");
        }

        public void Update()
        {

            if (Engine.SendCameraLookEvents)
            {
                CameraLookEvents();
            }
            if (Engine.SendCursorEvents)
            {
                MouseCursorEvents();
            }
        }

        private void MouseCursorEvents()
        { // cursor position

            //Vector3F pos = new Vector3F (Input.mousePosition.pixelX, Input.mousePosition.pixelY, 10f);
            VoxelInfo raycast = Engine.VoxelRaycast(Camera.main.ScreenPointToRay(Input.mousePosition), 9999f, false);

            if (raycast != null)
            {

                // create a local copy of the hit voxel so we can call functions on it
                GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(raycast.GetVoxel()));

                // only execute this if the voxel actually has any events (either VoxelEvents component, or any component that inherits from it)
                if (voxelObject.GetComponent<VoxelEvents>() != null)
                {

                    voxelObject.GetComponent<VoxelEvents>().OnLook(raycast);

                    // for all mouse buttons, send events
                    for (int i = 0; i < 3; i++)
                    {
                        if (Input.GetMouseButtonDown(i))
                        {
                            voxelObject.GetComponent<VoxelEvents>().OnMouseDown(i, raycast);
                        }
                        if (Input.GetMouseButtonUp(i))
                        {
                            voxelObject.GetComponent<VoxelEvents>().OnMouseUp(i, raycast);
                        }
                        if (Input.GetMouseButton(i))
                        {
                            voxelObject.GetComponent<VoxelEvents>().OnMouseHold(i, raycast);
                        }
                    }
                }

                Destroy(voxelObject);

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

            VoxelInfo raycast = Engine.VoxelRaycast(Camera.main.transform.position, Camera.main.transform.forward, CameraLookRange, false);

            if (raycast != null)
            {

                // create a local copy of the hit voxel so we can call functions on it
                GameObject voxelObject = Instantiate(Engine.GetVoxelGameObject(raycast.GetVoxel()));

                // only execute this if the voxel actually has any events (either VoxelEvents component, or any component that inherits from it)
                if (voxelObject.GetComponent<VoxelEvents>() != null)
                {

                    voxelObject.GetComponent<VoxelEvents>().OnLook(raycast);

                    // for all mouse buttons, send events
                    for (int i = 0; i < 3; i++)
                    {
                        if (Input.GetMouseButtonDown(i))
                        {
                            voxelObject.GetComponent<VoxelEvents>().OnMouseDown(i, raycast);
                        }
                        if (Input.GetMouseButtonUp(i))
                        {
                            voxelObject.GetComponent<VoxelEvents>().OnMouseUp(i, raycast);
                        }
                        if (Input.GetMouseButton(i))
                        {
                            voxelObject.GetComponent<VoxelEvents>().OnMouseHold(i, raycast);
                        }
                    }
                }

                Destroy(voxelObject);

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