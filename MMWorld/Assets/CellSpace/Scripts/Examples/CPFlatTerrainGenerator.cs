namespace CellSpace.Examples
{

    public class CPFlatTerrainGenerator : CPTerrainGenerator
    { // generates a flat terrain

        public override void GenerateCellData()
        {

            int chunky = chunk.ChunkIndex.y;
            int SideLength = CPEngine.ChunkSideLength;

            for (int x = 0; x < SideLength; x++)
            {
                for (int y = 0; y < SideLength; y++)
                {
                    if (CPEngine.HorizontalMode)
                    {
                     // for all Items in the chunk
                        int currentHeight = y + (SideLength * chunky); // get absolute height for the cell
                        if (currentHeight < 8)
                        {
                            chunk.SetCellSimple(x, y, 1); //高度8以下设置土块
                        }
                    }
                    else
                    {
                        for (int z = 0; z < SideLength; z++)
                        { // for all Items in the chunk
                            int currentHeight = y + (SideLength * chunky); // get absolute height for the cell
                            if (currentHeight < 8)
                            {
                                chunk.SetCellSimple(x, y, z, 1); // set dirt
                            }
                        }
                    }
                }
            }
        }
    }
}
