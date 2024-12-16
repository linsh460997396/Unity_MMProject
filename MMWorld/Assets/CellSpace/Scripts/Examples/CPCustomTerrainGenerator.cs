
namespace CellSpace.Examples
{
    /// <summary>
    /// 自定义地形布置（进入布置前团块里的单元都是空块）
    /// </summary>
    public class CPCustomTerrainGenerator : CPTerrainGenerator
    {
        /// <summary>
        /// mapID=0为大地图，小地图从1~239开始（1是拉多镇）,240是龙珠大地图测试
        /// </summary>
        /// <param name="mapID"></param>
        void LoadMap(int mapID)
        {
            int i = -1;
            if (mapID == 0)
            {//刷大地图
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[0][i] + 10));//重装机兵大地图第一个纹理编号从11开始
                        }
                    }
                }
            }
            else if (mapID > 0 && mapID<240)
            {//刷小地图
                int width = CPEngine.mapWidths[mapID - 1];//拉多是mapId=1，格子宽度=mapWidths[0]
                int currentX = 0; // 当前列的索引
                int currentY = 0; // 当前行的索引

                // 由于我们不知道总格子数，我们将使用一个条件来检查是否应该停止
                bool shouldStop = false;

                while (!shouldStop)
                {
                    i++; // 增加计数
                    if (CPEngine.HorizontalMode)
                    {
                        chunk.SetCellSimple(currentX, currentY, (ushort)(CPEngine.mapIDs[mapID][i] + 162));//重装机兵小地图第一个纹理编号从163开始
                    }
                    currentX++;

                    // 如果达到行宽，则换行
                    if (currentX >= width)
                    {
                        currentX = 0; // 重置列索引
                        currentY++;   // 增加行索引

                        // 检查是否应该停止
                        if (i + 1 >= CPEngine.mapIDs[mapID].Count) //如拉多的格子数是384，达到就停止
                        {
                            shouldStop = true;
                        }
                    }
                }
            }
            else if (mapID == 240)
            {//刷龙珠大地图
                for (int y = 0; y < 349; y++)
                {
                    for (int x = 0; x < 512; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[240][i] + 1522));//龙珠大地图第一个纹理编号从1523开始
                        }
                    }
                }
            }
        }

        public override void GenerateCellData()
        {
            
            ////获取团块索引的Y值
            //int chunky = chunk.ChunkIndex.y;
            ////获取团块的长度
            //int SideLength = CPEngine.ChunkSideLength;
            ////引擎设置：是否平地面
            //CPEngine.KeepTerrainHeight = false;
            ////设置恒定地形高度（世界坐标）
            //CPEngine.TerrainHeight = 8;

            LoadMap(0);

            //遍历团块长度内所有单元的索引（配合纹理文本所以从左上原点开始）
            //int i = -1;
            //for (int pixelY = 0; pixelY < SideLength; pixelY++)
            //{
            //    for (int pixelX = 0; pixelX < SideLength; pixelX++)
            //    {
            //        i++;
            //        if (CPEngine.HorizontalMode)
            //        {
            //            //chunk.SetCellSimple(pixelX, pixelY, 19);
            //            chunk.SetCellSimple(pixelX, pixelY, (ushort)(CPEngine.mapIDs[0][i] + 10));
            //        }
            //    }
            //}

            //for (int pixelY = SideLength - 1; pixelY >= 0; pixelY--)
            //{
            //    for (int pixelX = 0; pixelX < SideLength; pixelX++)
            //    {
            //        i++;
            //        if (CPEngine.HorizontalMode)
            //        {
            //            //chunk.SetCellSimple(pixelX, pixelY, 14);
            //            chunk.SetCellSimple(pixelX, pixelY, (ushort)(CPEngine.mapIDs[0][i] + 10));
            //        }
            //    }
            //}

            #region
            //for (int pixelX = 0; pixelX < SideLength; pixelX++)
            //{
            //    for (int pixelY = 0; pixelY < SideLength; pixelY++)
            //    {
            //        if (CPEngine.HorizontalMode)
            //        {
            //            Vector3 voxelPos = chunk.CellIndexToPosition(pixelX, pixelY); //获取单元索引的世界绝对位置
            //            int currentHeight = pixelY + (SideLength * chunky); //获得单元的（绝对坐标）世界高度
            //            if (CPEngine.KeepTerrainHeight)
            //            {
            //                if (CPEngine.TerrainHeight > currentHeight)
            //                {
            //                    //更改指定索引处的单元数据（即修改单元的种类）
            //                    chunk.SetCellSimple(pixelX, pixelY, 2);   // set grass.设置为草地，2是草地的种类ID

            //                }
            //                currentHeight = currentHeight + 1; //布置泥土时偏移1(因为我们想要草更高1块)
            //                if (CPEngine.TerrainHeight > currentHeight)
            //                {
            //                    //更改指定索引处的单元数据（即修改单元的种类）
            //                    chunk.SetCellSimple(pixelX, pixelY, 1); // set dirt.设置为土块，1是土块的种类ID

            //                }
            //            }
            //            else
            //            {
            //                //index = ConvertLeftBottomToTopLeft(pixelY*256+pixelX+1,256,256);//获取以左上角扫描时的该单元位置编号
            //                //id = int.Parse(Main_MMWorld.mapContents[0][index])+10;//用编号得到大地图纹理ID，如果是1对应cell_11
            //                //Debug.Log(index+" "+id);
            //                chunk.SetCellSimple(pixelX, pixelY, 1);
            //            }
            //        }
            //        else
            //        {
            //            for (int z = 0; z < SideLength; z++)
            //            { // for all voxels in the chunk
            //                Vector3 voxelPos = chunk.CellIndexToPosition(pixelX, pixelY, z); // get absolute position for the voxel.获取单元索引的世界绝对位置
            //                voxelPos = new Vector3(voxelPos.pixelX + seed, voxelPos.pixelY, voxelPos.z + seed); // offset by seed.用世界种子进行噪声修正，种子是30625这种数字
            //                                                                                          //Mathf.PerlinNoise()参数固定会形成固定的噪声，只需调整噪声函数返回结果接近某种高度，再遍历团块内单元高度来比对，就可布置该高度形状上的单元种类形成固定地貌
            //                                                                                          //制造主要地形（大山和小山）
            //                float perlin1 = Mathf.PerlinNoise(voxelPos.pixelX * 0.010f, voxelPos.z * 0.010f) * 70.1f; // major (mountains & big hills)，22左右波动（数值变化平稳，噪声系数0.010f）
            //                                                                                                     //制造次要地形（精致的细节）
            //                float perlin2 = Mathf.PerlinNoise(voxelPos.pixelX * 0.085f, voxelPos.z * 0.085f) * 9.1f; // minor (fine detail)，3-8左右波动（数值变化剧烈，噪声系数0.085f）
            //                int currentHeight = pixelY + (SideLength * chunky); // get absolute height for the voxel.获得单元的（绝对坐标）世界高度
            //                bool setToGrass = false;
            //                //噪声结果如22是一个波动的高度值，遍历x pixelY z 是在遍历团块内每个单元的索引（然后转换为世界高度），跟单元高度对比后符合高度则进行具体布置
            //                if (CPEngine.KeepTerrainHeight)
            //                {
            //                    if (CPEngine.TerrainHeight > currentHeight)
            //                    {
            //                        //更改指定索引处的单元数据（即修改单元的种类）
            //                        chunk.SetCellSimple(pixelX, pixelY, z, 2);   // set grass.设置为草地，2是草地的种类ID
            //                        setToGrass = true;
            //                    }
            //                    currentHeight = currentHeight + 1; //布置泥土时偏移1(因为我们想要草更高1块)
            //                    if (CPEngine.TerrainHeight > currentHeight)
            //                    {
            //                        //更改指定索引处的单元数据（即修改单元的种类）
            //                        chunk.SetCellSimple(pixelX, pixelY, z, 1); // set dirt.设置为土块，1是土块的种类ID
            //                        setToGrass = false;
            //                    }
            //                    //接下来在成型的地面上布置树
            //                    //if (setToGrass && TreeCanFit(pixelX, pixelY, z))
            //                    //{
            //                    //    //1%概率布置树
            //                    //    if (Random.Range(0.0f, 1.0f) < 0.01f)
            //                    //    {
            //                    //        AddTree(pixelX, pixelY + 1, z);
            //                    //    }
            //                    //}
            //                }
            //                else
            //                {
            //                    // grass pass.布置草地（噪声1大于当前高度时）
            //                    if (perlin1 > currentHeight)
            //                    {
            //                        //如果噪声1大于噪声2+当前高度
            //                        if (perlin1 > perlin2 + currentHeight)
            //                        {
            //                            //更改指定索引处的单元数据（即修改单元的种类）
            //                            chunk.SetCellSimple(pixelX, pixelY, z, 2);   // set grass.设置为草地，2是草地的种类ID
            //                            setToGrass = true;
            //                        }
            //                    }
            //                    // dirt pass
            //                    currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).布置泥土时偏移1(因为我们想要草更高1块)
            //                    if (perlin1 > currentHeight)
            //                    {
            //                        //如果噪声1大于（噪声2+当前高度）
            //                        if (perlin1 > perlin2 + currentHeight)
            //                        {
            //                            //更改指定索引处的单元数据（即修改单元的种类）
            //                            chunk.SetCellSimple(pixelX, pixelY, z, 1); // set dirt.设置为土块，1是土块的种类ID
            //                            setToGrass = false;
            //                        }
            //                    }
            //                    // tree pass.接下来在成型的地面上布置树
            //                    //if (setToGrass && TreeCanFit(pixelX, pixelY, z))
            //                    //{ // only add a tree if the current block has been set to grass and if there is room for the tree in the chunk
            //                    //    if (Random.Range(0.0f, 1.0f) < 0.01f)
            //                    //    { // 1% chance to add a tree
            //                    //        AddTree(pixelX, pixelY + 1, z);
            //                    //    }
            //                    //}
            //                }
            //            }
            //        }
            //    }
            //}
            #endregion
        }

        /// <summary>
        /// 该单元经计算可以种树
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        bool TreeCanFit(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                return TreeCanFit(x, y);
            }
            else
            {
                if (x > 0 && x < CPEngine.ChunkSideLength - 1 && z > 0 && z < CPEngine.ChunkSideLength - 1 && y + 5 < CPEngine.ChunkSideLength)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 该单元经计算可以种树
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool TreeCanFit(int x, int y)
        {
            return false;
        }

        /// <summary>
        /// 添加树（可经TreeCanFit验证后再执行）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void AddTree(int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                AddTree(x, y);
            }
            else
            {
                // first, create a trunk
                for (int trunkHeight = 0; trunkHeight < 4; trunkHeight++)
                {
                    chunk.SetCellSimple(x, y + trunkHeight, z, 6); // set wood at pixelY from 0 to 4
                }

                // then create leaves around the top
                for (int offsetY = 2; offsetY < 4; offsetY++) // leaves should start from pixelY=2 (vertical coordinate)
                {
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
                        {
                            if ((offsetX == 0 && offsetZ == 0) == false)
                            {
                                chunk.SetCellSimple(x + offsetX, y + offsetY, z + offsetZ, 9);
                            }
                        }
                    }
                }
                // add one more leaf block on top
                chunk.SetCell(x, y + 4, z, 9, false);
            }
        }
        /// <summary>
        /// 添加树（可经TreeCanFit验证后再执行）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void AddTree(int x, int y)
        {

        }
    }
}