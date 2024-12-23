using UnityEngine;

namespace CellSpace.Examples
{
    //如果希望使用默认事件和自定义事件，则从CellEvents类继承
    public class DefaultCellEvents : CellEvents
    {
        public override void OnMouseDown(int mouseButton, CellInfo cellInfo)
        {
            if (mouseButton == 0)
            { // destroy a block with LMB
                Cell.DestroyBlock(cellInfo);
            }
            else if (mouseButton == 1)
            { // place a block with RMB
                if (cellInfo.GetCellID() == 8)
                { // if we're looking at a tall grass block, replace it with the held block
                    Cell.PlaceBlock(cellInfo, CPExampleInventory.HeldBlock);
                }
                else
                { // else put the block next to the one we're looking at
                    CellInfo newInfo = new CellInfo(cellInfo.adjacentIndex, cellInfo.chunk); // use adjacentIndex to place the block
                    Cell.PlaceBlock(newInfo, CPExampleInventory.HeldBlock);
                }
            }
        }

        public override void OnLook(CellInfo cellInfo)
        {
            // move the selected block ui to the block that's being looked at (convert the index of the hit voxel to absolute world position)
            GameObject blockSelection = GameObject.Find("SelectedBox");
            if (blockSelection != null)
            {
                blockSelection.transform.position = cellInfo.chunk.CellIndexToPosition(cellInfo.index);
                blockSelection.GetComponent<Renderer>().enabled = true;
                blockSelection.transform.rotation = cellInfo.chunk.transform.rotation;
            }
        }

        public override void OnBlockPlace(CellInfo cellInfo)
        {
            CPIndex indexBelow;
            // if the block below is grass, change it to dirt
            if (CPEngine.HorizontalMode)
            {
                indexBelow = new CPIndex(cellInfo.index.x, cellInfo.index.y - 1);
            }
            else
            {
                indexBelow = new CPIndex(cellInfo.index.x, cellInfo.index.y - 1, cellInfo.index.z);
            }
            if (cellInfo.GetCellType().VTransparency == Transparency.solid && cellInfo.chunk.GetCellID(indexBelow) == 2)
            {
                cellInfo.chunk.SetCell(indexBelow, 1, true);
            }
        }

        public override void OnBlockDestroy(CellInfo cellInfo)
        {
            CPIndex indexAbove;
            // if the block above is tall grass, destroy it
            if (CPEngine.HorizontalMode)
            {
                indexAbove = new CPIndex(cellInfo.index.x, cellInfo.index.y + 1);
            }
            else
            {
                indexAbove = new CPIndex(cellInfo.index.x, cellInfo.index.y + 1, cellInfo.index.z);
            }
            if (cellInfo.chunk.GetCellID(indexAbove) == 8)
            {
                cellInfo.chunk.SetCell(indexAbove, 0, true);
            }
        }

        public override void OnBlockEnter(GameObject enteringObject, CellInfo cellInfo)
        {
            Debug.Log("OnBlockEnter at " + cellInfo.chunk.ChunkIndex.ToString() + " / " + cellInfo.index.ToString());
        }
    }
}

