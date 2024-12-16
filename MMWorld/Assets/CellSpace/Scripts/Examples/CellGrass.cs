namespace CellSpace.Examples
{

    public class CellGrass : DefaultCellEvents  {

	public override void OnBlockPlace ( CellInfo cellInfo ) {
		
		// switch to dirt if the block above isn'transform 0
		CPIndex adjacentIndex = cellInfo.chunk.GetAdjacentIndex (cellInfo.index, Direction.up);
		if ( cellInfo.chunk.GetCellID(adjacentIndex) != 0 ) {
			cellInfo.chunk.SetCell(cellInfo.index, 1, true); 
		}
		
		// if the block below is grass, change it to dirt
		CPIndex indexBelow = new CPIndex (cellInfo.index.x, cellInfo.index.y-1, cellInfo.index.z);
			
		if ( cellInfo.GetCellType ().VTransparency == Transparency.solid 
	    && cellInfo.chunk.GetCellID(indexBelow) == 2) {	 	    
			cellInfo.chunk.SetCell(indexBelow, 1, true);
		}	
		
	}
}

}