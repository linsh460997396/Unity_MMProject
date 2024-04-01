using UnityEngine;

namespace Uniblocks
{
    /// <summary>
    /// 地形生成器举例
    /// </summary>
    public class ExampleTerrainGenerator : TerrainGenerator
    {
        /// <summary>
        /// [override]生成体素数据（体素块的种类）
        /// </summary>
        public override void GenerateVoxelData()
        {
            //获取团块索引的Y值
            int chunky = chunk.ChunkIndex.y;
            //获取团块的长度
            int SideLength = Engine.ChunkSideLength;

            // debug
            int random = Random.Range(0, 10);

            //遍历团块长度内所有体素块的索引
            for (int x = 0; x < SideLength; x++)
            {
                for (int y = 0; y < SideLength; y++)
                {
                    for (int z = 0; z < SideLength; z++)
                    { // for all voxels in the chunk

                        Vector3 voxelPos = chunk.VoxelIndexToPosition(x, y, z); // get absolute position for the voxel.获取体素索引中心的绝对世界位置
                        voxelPos = new Vector3(voxelPos.x + seed, voxelPos.y, voxelPos.z + seed); // offset by seed.用世界种子进行位置修正，种子是30625这种数字
                        //制造主要地形（大山和小山）
                        float perlin1 = Mathf.PerlinNoise(voxelPos.x * 0.010f, voxelPos.z * 0.010f) * 70.1f; // major (mountains & big hills)，22左右波动（数值变化平稳，噪声系数0.010f）
                        //制造次要地形（精致的细节）
                        float perlin2 = Mathf.PerlinNoise(voxelPos.x * 0.085f, voxelPos.z * 0.085f) * 9.1f; // minor (fine detail)，3-8左右波动（数值变化剧烈，噪声系数0.085f）


                        int currentHeight = y + (SideLength * chunky); // get absolute height for the voxel.获得体素的绝对高度

                        // grass pass.布置草（噪声1大于当前高度时）
                        if (perlin1 > currentHeight)
                        {
                            //如果噪声1大于噪声2+当前高度
                            if (perlin1 > perlin2 + currentHeight)
                            {
                                //更改指定索引处的体素数据（即修改体素块的种类）
                                chunk.SetVoxelSimple(x, y, z, 2);   // set grass.设置为草，2是草的种类ID

                            }
                        }

                        // dirt pass.布置土块（噪声1大于当前高度时）
                        currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).布置时将泥土偏移1(因为我们想要草更高1块)
                        if (perlin1 > currentHeight)
                        {
                            //如果噪声1大于噪声2+当前高度
                            if (perlin1 > perlin2 + currentHeight)
                            {
                                //更改指定索引处的体素数据（即修改体素块的种类）
                                chunk.SetVoxelSimple(x, y, z, 1); // set dirt.设置为土块，1是图块的种类ID
                            }
                        }

                        // debug.随机到1时
                        if (random == 1)
                        {
                            //chunk.SetVoxelSimple(x,y,z, 3); // set stone or whatever.设置石头或其他类别体素块
                        }

                    }
                }
            }
        }
    }

}