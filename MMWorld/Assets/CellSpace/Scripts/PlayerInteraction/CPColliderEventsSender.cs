using UnityEngine;

namespace CellSpace
{
    public class CPColliderEventsSender : MonoBehaviour
    {
        private CPIndex LastIndex;
        private CellChunk LastChunk;

        public void Update()
        {
            //check if chunk is not null
            GameObject chunkObject = CPEngine.PositionToChunk(transform.position);
            if (chunkObject == null) return;
            //get the cellInfo from the transform's position
            CellChunk chunk = chunkObject.GetComponent<CellChunk>();
            CPIndex cellIndex = chunk.PositionToCellIndex(transform.position);
            CellInfo cellInfo = new CellInfo(cellIndex, chunk);
            //print(cellInfo.GetCellID());
            //create a local copy of the collision voxel so we can call functions on it.����һ����ײ���صı��ظ������������ǾͿ��������ϵ��ú���
            GameObject cellObject = Instantiate(CPEngine.GetCellGameObject(cellInfo.GetCellID())) as GameObject;
            CellEvents events = cellObject.GetComponent<CellEvents>();
            if (events != null)
            {
                // OnEnter
                if (chunk != LastChunk || cellIndex.IsEqual(LastIndex) == false)
                {
                    events.OnBlockEnter(this.gameObject, cellInfo);
                }
                // OnStay
                else
                {
                    events.OnBlockStay(this.gameObject, cellInfo);
                }
            }
            LastChunk = chunk;
            LastIndex = cellIndex;
            Destroy(cellObject);
        }
    }
}

























