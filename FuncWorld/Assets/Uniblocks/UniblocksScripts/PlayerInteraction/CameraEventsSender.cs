using UnityEngine;

// sends VoxelEvents such as OnLook, OnMouseDown, etc.发送体素事件如OnLook、 OnMouseDown等

namespace Uniblocks
{
    /// <summary>
    /// 摄像机镜头注视（包含鼠标指针）事件发送功能
    /// </summary>
    public class CameraEventsSender : MonoBehaviour
    {

        //Unity对公开字段会默认序列化，显示在挂载游戏物体的GUI界面（可在该界面输入来完成初始赋值）

        public float Range;
        private GameObject SelectedBlockGraphics;

        public void Awake()
        {
            if (Range <= 0)
            {
                Debug.LogWarning("Uniblocks: CameraEventSender.Range must be greater than 0. Setting Range to 5." +
                    "Range必须大于0，将默认设置范围:5");
                Range = 5.0f;
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

            //Vector3F pos = new Vector3F (Input.mousePosition.x, Input.mousePosition.y, 10f);
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

            VoxelInfo raycast = Engine.VoxelRaycast(Camera.main.transform.position, Camera.main.transform.forward, Range, false);

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