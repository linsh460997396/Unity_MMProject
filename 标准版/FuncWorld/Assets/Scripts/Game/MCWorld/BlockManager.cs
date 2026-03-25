using UnityEngine;
using Uniblocks;

namespace MCWorld
{
    /// <summary>
    /// 体素块管理器,利用主摄像机注视增删体素块,也管理着选择框的实时位置更新.
    /// 组件用法:Unity中随便新建一个空对象“Manager”,把脚本拖到组件位置即挂载(Unity要求一个cs文件只能一个类,且类名须与文件名一致).
    /// (注:本类属于自定义测试用,非Uniblocks插件核心组件,效果等同CameraEventsSender.cs).
    /// </summary>
    public class BlockManager : MonoBehaviour
    {
        private ushort blockID = 0;
        private Transform SelectedBoxTransform;

        void Start()
        {
            SelectedBoxTransform = GameObject.Find("SelectedBox").transform;//获取选择框
            SelectedBoxTransform.gameObject.SetActive(false);//选择框状态不激活(会隐藏起来)
        }

        void Update()
        {
            SelectBlockID();

            VoxelInfo info = Engine.VoxelRaycast(Camera.main.transform.position, Camera.main.transform.forward, 10, false);
            if (info != null)
            {
                //print(info.index);
                if (Input.GetMouseButtonDown(0))
                {
                    Voxel.DestroyBlock(info);
                }
                if (Input.GetMouseButtonDown(1))
                {
                    VoxelInfo newInfo = new VoxelInfo(info.adjacentIndex, info.chunk);
                    Voxel.PlaceBlock(newInfo, blockID);
                }
            }
            UpdateSelectedBox(info);
        }

        private void SelectBlockID()
        {
            for (ushort i = 1; i < 10; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                {
                    blockID = i;
                }
            }
        }

        private void UpdateSelectedBox(VoxelInfo info)
        {
            if (info != null)
            {
                SelectedBoxTransform.gameObject.SetActive(true);
                SelectedBoxTransform.position = info.chunk.VoxelIndexToPosition(info.index);
            }
            else
            {
                SelectedBoxTransform.gameObject.SetActive(false);
            }
        }
    }
}