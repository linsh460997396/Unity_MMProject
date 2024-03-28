using UnityEngine;
using Uniblocks;

/// <summary>
/// 体素块管理器
/// </summary>
public class BlockManager : MonoBehaviour 
{
    private ushort blockID = 0;
    private Transform selectedBlockEffect;

	void Start () {
        selectedBlockEffect = GameObject.Find("SelectedBox").transform;//获取选择框
        selectedBlockEffect.gameObject.SetActive(false);//选择框状态不激活（会隐藏起来）
	}
	
	void Update () {
        SelectBlockID();

        VoxelInfo info = Engine.VoxelRaycast(Camera.main.transform.position, Camera.main.transform.forward, 10, false);
        if (info != null)
        {
            //print(info.index);
            if( Input.GetMouseButtonDown(0))
            {
                Voxel.DestroyBlock(info);
            }
            if (Input.GetMouseButtonDown(1))
            {
                VoxelInfo newInfo = new VoxelInfo(info.adjacentIndex, info.chunk);
                Voxel.PlaceBlock(newInfo, blockID);
            }
        }
        UpdateSelectedBlockEffect(info);
    }

    private void SelectBlockID()
    {
        for(ushort i = 1; i < 10; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                blockID = i;
            }
        }
    }

    private void UpdateSelectedBlockEffect(VoxelInfo info)
    {
        if (info != null)
        {
            selectedBlockEffect.gameObject.SetActive(true);
            selectedBlockEffect.position = info.chunk.VoxelIndexToPosition(info.index);
        }
        else
        {
            selectedBlockEffect.gameObject.SetActive(false);
        }
    }
}
