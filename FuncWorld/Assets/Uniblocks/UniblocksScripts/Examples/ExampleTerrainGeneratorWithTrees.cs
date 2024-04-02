using UnityEngine;

namespace Uniblocks
{

    public class ExampleTerrainGeneratorWithTrees : TerrainGenerator
    {

        public override void GenerateVoxelData()
        {
            //获取团块索引的Y值
            int chunky = chunk.ChunkIndex.y;
            //获取团块的长度
            int SideLength = Engine.ChunkSideLength;

            //引擎设置：是否平地面
            Engine.KeepTerrainHeight = true;
            //设置恒定地形高度（世界坐标）
            Engine.TerrainHeight = 32;

            //遍历团块长度内所有体素块的索引（进入布置前团块里的体素都是空块）
            for (int x = 0; x < SideLength; x++)
            {
                for (int y = 0; y < SideLength; y++)
                {
                    for (int z = 0; z < SideLength; z++)
                    { // for all voxels in the chunk

                        Vector3 voxelPos = chunk.VoxelIndexToPosition(x, y, z); // get absolute position for the voxel.获取体素索引的绝对世界位置
                        voxelPos = new Vector3(voxelPos.x + seed, voxelPos.y, voxelPos.z + seed); // offset by seed.用世界种子进行噪声修正，种子是30625这种数字

                        float perlin1 = Mathf.PerlinNoise(voxelPos.x * 0.010f, voxelPos.z * 0.010f) * 70.1f; // major (mountains & big hills)，22左右波动（数值变化平稳，噪声系数0.010f）
                        float perlin2 = Mathf.PerlinNoise(voxelPos.x * 0.085f, voxelPos.z * 0.085f) * 9.1f; // minor (fine detail)，3-8左右波动（数值变化剧烈，噪声系数0.085f）

                        int currentHeight = y + (SideLength * chunky); // get absolute height for the voxel.获得体素的（绝对坐标）世界高度
                        bool setToGrass = false;

                        //噪声结果如22是一个波动的高度值，遍历x y z 是在遍历团块内每个体素块的索引（然后转换为世界高度），跟体素块高度对比后符合高度则进行具体布置

                        if (Engine.KeepTerrainHeight)
                        {
                            if (Engine.TerrainHeight > currentHeight)
                            {
                                //更改指定索引处的体素数据（即修改体素块的种类）
                                chunk.SetVoxelSimple(x, y, z, 2);   // set grass.设置为草地，2是草地的种类ID
                                setToGrass = true;
                            }
                            currentHeight = currentHeight + 1; //布置泥土时偏移1(因为我们想要草更高1块)
                            if (Engine.TerrainHeight > currentHeight)
                            {
                                //更改指定索引处的体素数据（即修改体素块的种类）
                                chunk.SetVoxelSimple(x, y, z, 1); // set dirt.设置为土块，1是土块的种类ID
                                setToGrass = false;
                            }
                            //接下来在成型的地面上布置树
                            if (setToGrass && TreeCanFit(x, y, z))
                            {
                                //1%概率布置树
                                if (Random.Range(0.0f, 1.0f) < 0.01f)
                                {
                                    AddTree(x, y + 1, z);
                                }
                            }
                        }
                        else
                        {
                            // grass pass.布置草地（噪声1大于当前高度时）
                            if (perlin1 > currentHeight)
                            {
                                //如果噪声1大于噪声2+当前高度
                                if (perlin1 > perlin2 + currentHeight)
                                {
                                    //更改指定索引处的体素数据（即修改体素块的种类）
                                    chunk.SetVoxelSimple(x, y, z, 2);   // set grass.设置为草地，2是草地的种类ID
                                    setToGrass = true;
                                }
                            }

                            // dirt pass
                            currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).布置泥土时偏移1(因为我们想要草更高1块)
                            if (perlin1 > currentHeight)
                            {
                                //如果噪声1大于（噪声2+当前高度）
                                if (perlin1 > perlin2 + currentHeight)
                                {
                                    //更改指定索引处的体素数据（即修改体素块的种类）
                                    chunk.SetVoxelSimple(x, y, z, 1); // set dirt.设置为土块，1是土块的种类ID
                                    setToGrass = false;
                                }
                            }

                            // tree pass.接下来在成型的地面上布置树
                            if (setToGrass && TreeCanFit(x, y, z))
                            { // only add a tree if the current block has been set to grass and if there is room for the tree in the chunk
                                if (Random.Range(0.0f, 1.0f) < 0.01f)
                                { // 1% chance to add a tree
                                    AddTree(x, y + 1, z);
                                }
                            }
                        }
                    }
                }
            }
        }


        bool TreeCanFit(int x, int y, int z)
        {
            if (x > 0 && x < Engine.ChunkSideLength - 1 && z > 0 && z < Engine.ChunkSideLength - 1 && y + 5 < Engine.ChunkSideLength)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void AddTree(int x, int y, int z)
        {

            // first, create a trunk
            for (int trunkHeight = 0; trunkHeight < 4; trunkHeight++)
            {
                chunk.SetVoxelSimple(x, y + trunkHeight, z, 6); // set wood at y from 0 to 4
            }


            // then create leaves around the top
            for (int offsetY = 2; offsetY < 4; offsetY++) // leaves should start from y=2 (vertical coordinate)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
                    {
                        if ((offsetX == 0 && offsetZ == 0) == false)
                        {
                            chunk.SetVoxelSimple(x + offsetX, y + offsetY, z + offsetZ, 9);
                        }
                    }
                }
            }

            // add one more leaf block on top
            chunk.SetVoxel(x, y + 4, z, 9, false);
        }
    }

}