using UnityEngine;

namespace CellSpace.Examples
{
    /// <summary>
    /// 地形生成器举例
    /// </summary>
    public class CPExampleTerrainGenerator : CPTerrainGenerator
    {
        /// <summary>
        /// 生成单元数据（体素块的种类）
        /// </summary>
        public override void GenerateCellData()
        {
            //获取团块索引的Y值
            int chunky = chunk.ChunkIndex.y;
            //获取团块的长度
            int SideLength = CPEngine.ChunkSideLength;
            CPEngine.KeepTerrainHeight = true;
            CPEngine.TerrainHeight = 8;

            // debug
            int random = Random.Range(0, 10);

            //遍历团块长度内所有体素块的索引（进入布置前团块里的单元都是空块）
            for (int x = 0; x < SideLength; x++)
            {
                for (int y = 0; y < SideLength; y++)
                {
                    if (CPEngine.HorizontalMode)
                    {
                     // for all voxels in the chunk
                        Vector3 voxelPos = chunk.CellIndexToPosition(x, y); //获取单元索引的世界绝对位置
                        int currentHeight = y + (SideLength * chunky); //获得单元的（绝对坐标）世界高度
                        //未完待续
                    }
                    else
                    {
                        for (int z = 0; z < SideLength; z++)
                        { // for all Items in the chunk

                            Vector3 cellPos = chunk.CellIndexToPosition(x, y, z); // get absolute position for the cell.获取单元索引的世界绝对位置
                            cellPos = new Vector3(cellPos.x + seed, cellPos.y, cellPos.z + seed); // offset by seed.用世界种子进行噪声修正，种子是30625这种数字

                            //Mathf.PerlinNoise()参数固定会形成固定的噪声，只需调整噪声函数返回结果接近某种高度，再遍历团块内体素块高度来比对，就可布置该高度形状上的体素块种类形成固定地貌

                            //制造主要地形（大山和小山）
                            float perlin1 = Mathf.PerlinNoise(cellPos.x * 0.010f, cellPos.z * 0.010f) * 70.1f; // major (mountains & big hills)，22左右波动（数值变化平稳，噪声系数0.010f）
                                                                                                               //制造次要地形（精致的细节）
                            float perlin2 = Mathf.PerlinNoise(cellPos.x * 0.085f, cellPos.z * 0.085f) * 9.1f; // minor (fine detail)，3-8左右波动（数值变化剧烈，噪声系数0.085f）

                            int currentHeight = y + (SideLength * chunky); // get absolute height for the cell.获得单元的（绝对坐标）世界高度

                            //噪声结果如22是一个波动的高度值，遍历x pixelY z 是在遍历团块内每个体素块的索引（然后转换为世界高度），跟体素块高度对比后符合高度则进行具体布置

                            if (CPEngine.KeepTerrainHeight)
                            {

                            }
                            else
                            {
                                // grass pass.布置草地（噪声1大于当前高度时）
                                if (perlin1 > currentHeight)
                                {
                                    //如果噪声1大于噪声2+当前高度
                                    if (perlin1 > perlin2 + currentHeight)
                                    {
                                        //更改指定索引处的单元数据（即修改体素块的种类）
                                        chunk.SetCellSimple(x, y, z, 2);   // set grass.设置为草地，2是草地的种类ID

                                    }
                                }

                                // dirt pass.布置土块（噪声1大于当前高度时）
                                currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).布置泥土时偏移1(因为我们想要草更高1块)
                                if (perlin1 > currentHeight)
                                {
                                    //如果噪声1大于噪声2+当前高度
                                    if (perlin1 > perlin2 + currentHeight)
                                    {
                                        //更改指定索引处的单元数据（即修改体素块的种类）
                                        chunk.SetCellSimple(x, y, z, 1); // set dirt.设置为土块，1是土块的种类ID
                                    }
                                }

                                // debug.随机到1时
                                if (random == 1)
                                {
                                    //chunk.SetCellSimple(pixelX,pixelY,z, 3); // set stone or whatever.设置石头或其他类别体素块
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}