namespace CellSpace.Examples
{
    public class CellDoorOpenClose : DefaultCellEvents
    {
        public override void OnMouseDown(int mouseButton, CellInfo cellInfo)
        {
            if (mouseButton == 0)
            {
                Cell.DestroyBlock(cellInfo);    // destroy with left click
            }

            else if (mouseButton == 1)
            { // open/close with right click

                if (cellInfo.GetCellID() == 70)
                { // if open door
                    Cell.ChangeBlock(cellInfo, 7); // set to closed
                }

                else if (cellInfo.GetCellID() == 7)
                { // if closed door
                    Cell.ChangeBlock(cellInfo, 70); // set to open
                }

            }
        }
    }
}
