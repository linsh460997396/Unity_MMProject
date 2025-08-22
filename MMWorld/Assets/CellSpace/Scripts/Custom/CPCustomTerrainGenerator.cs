using MetalMaxSystem;
using UnityEngine;


namespace CellSpace
{
    /// <summary>
    /// 地形生成器.
    /// 仅用于地形首次生成时刷块,如已经生成过,则不再执行GenerateCellData()方法,而是从硬盘的区域文件加载.
    /// 注意:进入布置前团块里的单元都是空块(id=0),在开启单一空间重复利用模式时地形生成器应停止刷新工作.
    /// </summary>
    public class CPCustomTerrainGenerator : CPTerrainGenerator
    {
        public override void GenerateCellData()
        {
            Debug.Log("启用地形生成器（硬盘无区域存档时进行首次地形生成）");
            if (CPEngine.horizontalMode == true)
            {//2D横版模式
                Debug.Log("启用2D横版模式");
                //侧面刷图
                if (chunk.ChunkIndex.y == 0)
                {
                    if (chunk.ChunkIndex.x >= 0)
                    {
                        //场景空间刷在正X轴上
                        if (CPEngine.OneSapceMode == false)
                        {
                            //未启用单一空间复用模式,每个团块(空间)都刷一个自动计算索引对应的场景.
                            Debug.Log("CPEngine.OneSapceMode == false");
                            LoadMap(chunk.ChunkIndex.x);//这里设计地图空间布置和存储在X坐标一条直线上
                        }
                        else
                        {
                            //启用单一空间复用模式,始终在当前团块内刷场景的模式(需手动指定刷什么,本类是首次初始化地形专用刷块,所以不再根据指示器自动刷)
                            Debug.LogWarning("启用单一空间复用模式,但尚未布置地图");
                        }
                    }
                }
            }
            else if (CPEngine.singleLayerTerrainMode == true)
            {//3D单层地形模式,KeepSingleChunkTerrainHeight为false时,默认将地图刷在空间团底部
                Debug.Log("启用3D单层地形模式");
                //3D模式下刷图在顶面（X-Z）
                if (chunk.ChunkIndex.z >= 0)
                {
                    if (chunk.ChunkIndex.y >= 0)
                    {
                        if (chunk.ChunkIndex.x >= 0)
                        {
                            //地图刷在正坐标轴上
                            if (CPEngine.OneSapceMode == false)
                            {
                                //未启用单一空间复用模式,每个团块(空间)都刷一个自动计算索引对应的场景.
                                Debug.Log("CPEngine.OneSapceMode == false");
                                //Load3DMap();
                                LoadUniverse();
                            }
                            else
                            {
                                //启用单一空间复用模式,始终在当前团块内刷场景的模式(需手动指定刷什么,本类是首次初始化地形专用刷块,所以不再根据指示器自动刷)
                                //虽然可在地图初始化时就刷一个初始大地图,但开局可能要进菜单的,得按剧情推进需要去刷,不要马上出来.
                                Debug.LogWarning("启用单一空间复用模式,但尚未布置地图");
                            }
                        }
                    }
                }
            }
            else
            {
                //正常3D的MC框架(默认是这个模式)
                if (CPEngine.OneSapceMode == false)
                {
                    Debug.Log("启用正常3D模式");
                    //未启用单一空间复用模式,每个团块(空间)都刷一个自动计算索引对应的场景
                    //获取团块索引的Y值
                    int chunky = chunk.ChunkIndex.y;
                    //获取团块的长度
                    int SideLength = CPEngine.chunkSideLength;
                    //CPEngine.keepTerrainHeight = true; //强制平地面
                    //CPEngine.TerrainHeight = 8; //地面不超过世界高度8米
                    //遍历团块长度内所有体素块的索引(进入布置前团块里的单元都是空块)
                    for (int x = 0; x < SideLength; x++)
                    {
                        for (int y = 0; y < SideLength; y++)
                        {
                            for (int z = 0; z < SideLength; z++)
                            { // for all voxels in the chunk
                                Vector3 voxelPos = chunk.CellIndexToPosition(x, y, z); // get absolute position for the voxel.获取单元索引的世界绝对位置
                                voxelPos = new Vector3(voxelPos.x + seed, voxelPos.y, voxelPos.z + seed); // offset by seed.用世界种子进行噪声修正,种子是30625这种数字
                                                                                                          //Mathf.PerlinNoise()参数固定会形成固定的噪声,只需调整噪声函数返回结果接近某种高度,再遍历团块内单元高度来比对,就可布置该高度形状上的单元种类形成固定地貌
                                                                                                          //制造主要地形(大山和小山)
                                float perlin1 = Mathf.PerlinNoise(voxelPos.x * 0.010f, voxelPos.z * 0.010f) * 70.1f; // major (mountains & big hills),22左右波动(数值变化平稳,噪声系数0.010f)
                                                                                                                     //制造次要地形(精致的细节)
                                float perlin2 = Mathf.PerlinNoise(voxelPos.x * 0.085f, voxelPos.z * 0.085f) * 9.1f; // minor (fine detail),3-8左右波动(数值变化剧烈,噪声系数0.085f)
                                int currentHeight = y + (SideLength * chunky); // get absolute height for the voxel.获得单元的(绝对坐标)世界高度
                                bool setToGrass = false;
                                //噪声结果如22是一个波动的高度值,遍历x pixelY z 是在遍历团块内每个单元的索引(然后转换为世界高度),跟单元高度对比后符合高度则进行具体布置
                                if (CPEngine.keepTerrainHeight)
                                {
                                    if (CPEngine.TerrainHeight > currentHeight)
                                    {
                                        //更改指定索引处的单元数据(即修改单元的种类)
                                        chunk.SetCellSimple(x, y, z, 2);   // set grass.设置为草地,2是草地的种类ID
                                        setToGrass = true;
                                    }
                                    currentHeight = currentHeight + 1; //布置泥土时偏移1(因为我们想要草更高1块)
                                    if (CPEngine.TerrainHeight > currentHeight)
                                    {
                                        //更改指定索引处的单元数据(即修改单元的种类)
                                        chunk.SetCellSimple(x, y, z, 1); // set dirt.设置为土块,1是土块的种类ID
                                        setToGrass = false;
                                    }
                                    //接下来在成型的地面上布置树
                                    if (setToGrass && TreeCanFit(x, y, z))
                                    {
                                        //1%概率布置树
                                        if (Random.Range(0.0f, 1.0f) < 0.01f)
                                        {
                                            //AddTree(x, y + 1, z);
                                        }
                                    }
                                }
                                else
                                {
                                    // grass pass.布置草地(噪声1大于当前高度时)
                                    if (perlin1 > currentHeight)
                                    {
                                        //如果噪声1大于噪声2+当前高度
                                        if (perlin1 > perlin2 + currentHeight)
                                        {
                                            //更改指定索引处的单元数据(即修改单元的种类)
                                            chunk.SetCellSimple(x, y, z, 2);   // set grass.设置为草地,2是草地的种类ID
                                            setToGrass = true;
                                        }
                                    }
                                    // dirt pass
                                    currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).布置泥土时偏移1(因为我们想要草更高1块)
                                    if (perlin1 > currentHeight)
                                    {
                                        //如果噪声1大于(噪声2+当前高度)
                                        if (perlin1 > perlin2 + currentHeight)
                                        {
                                            //更改指定索引处的单元数据(即修改单元的种类)
                                            chunk.SetCellSimple(x, y, z, 1); // set dirt.设置为土块,1是土块的种类ID
                                            setToGrass = false;
                                        }
                                    }
                                    // tree pass.接下来在成型的地面上布置树
                                    if (setToGrass && TreeCanFit(x, y, z))
                                    { // only add a tree if the current block has been set to grass and if there is room for the tree in the chunk
                                        if (Random.Range(0.0f, 1.0f) < 0.01f)
                                        { // 1% chance to add a tree
                                          //AddTree(x, y + 1, z);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                else
                {
                    //启用单一空间复用模式,始终在当前团块内刷场景的模式(需手动指定刷什么,本类是首次初始化地形专用刷块,所以不再根据指示器自动刷)
                    Debug.LogWarning("启用单一空间复用模式,但尚未布置地图");
                }
            }
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
            if (CPEngine.horizontalMode)
            {
                return TreeCanFit(x, y);
            }
            else
            {
                if (x > 0 && x < CPEngine.chunkSideLength - 1 && z > 0 && z < CPEngine.chunkSideLength - 1 && y + 5 < CPEngine.chunkSideLength)
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
        /// 添加树(可经TreeCanFit验证后再执行)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        void AddTree(int x, int y, int z)
        {
            if (CPEngine.horizontalMode)
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
        /// 添加树(可经TreeCanFit验证后再执行)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void AddTree(int x, int y) { }

        /// <summary>
        /// [2D横版模式专用]mapID=0为大地图,小地图从1~239开始(1是拉多镇),240是龙珠大地图
        /// </summary>
        /// <param name="mapID"></param>
        public void LoadMap(int mapID)
        {
            int i = -1;
            if (mapID == 0)
            {//刷大地图
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        i++;
                        if (CPEngine.horizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[0][i] + 10));//重装机兵大地图第一个纹理编号从11开始
                        }
                    }
                }
            }
            else if (mapID > 0 && mapID < 240)
            {//刷小地图
                int width = CPEngine.mapWidths[mapID - 1];//拉多是mapId=1,格子宽度=mapWidths[0]
                int currentX = 0; // 当前列的索引
                int currentY = 0; // 当前行的索引

                // 由于我们不知道总格子数,我们将使用一个条件来检查是否应该停止
                bool shouldStop = false;

                while (!shouldStop)
                {
                    i++; // 增加计数
                    if (CPEngine.horizontalMode)
                    {
                        chunk.SetCellSimple(currentX, currentY, (ushort)(CPEngine.mapIDs[mapID][i] + 162));//重装机兵小地图第一个纹理编号从163开始
                    }
                    currentX++;

                    // 如果达到行宽,则换行
                    if (currentX >= width)
                    {
                        currentX = 0; // 重置列索引
                        currentY++;   // 增加行索引

                        // 检查是否应该停止
                        if (i + 1 >= CPEngine.mapIDs[mapID].Count) //如拉多的格子数是384,达到就停止
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
                        if (CPEngine.horizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[mapID][i] + 1522));//龙珠大地图第一个纹理编号从1523开始
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [2D横版模式或地图编辑器专用]0是大地图纹理,1是小地图纹理
        /// </summary>
        /// <param name="textureID"></param>
        public void LoadTexture(int textureID)
        {
            int i = -1;
            if (textureID == 0)
            {//刷大地图纹理
                for (int y = 0; y < 19; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        i++;
                        if (CPEngine.horizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(x + 1 + 8 * y + 10));//重装机兵大地图第一个纹理编号从11开始
                        }
                    }
                }
            }
            else if (textureID == 1)
            {//刷小地图纹理
                for (int y = 0; y < 170; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        i++;
                        if (CPEngine.horizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(x + 8 * y + 163));//重装机兵大地图第一个纹理编号从163开始
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [3D单层地形模式]将场景纹理布置在空间团内的平地面.
        /// 地图是根据团块索引计算出来的,每个团块(空间)都刷一个自动计算索引对应的场景.
        /// mapID = chunkx + chunky * 10 + chunkz * 100.
        /// </summary>
        public void Load3DMap()
        {
            int height = 1; int i = -1;

            //获取团块索引值
            int chunkx = chunk.ChunkIndex.x;
            int chunky = chunk.ChunkIndex.y;
            int chunkz = chunk.ChunkIndex.z;
            //计算要刷的地图ID
            int mapID = chunkx + chunky * 10 + chunkz * 100;

            //获取团块的长度
            int SideLength = CPEngine.chunkSideLength;

            if (CPEngine.keepSingleChunkTerrainHeight)
            {
                height = CPEngine.SingleChunkTerrainHeight;//默认高度是0
            }
            int y = height - 1;
            //MMCore.Tell($"Load3DMap: chunkx={chunkx}, chunky={chunky}, chunkz={chunkz}, mapID={mapID}, height={height}, y={y}");

            //遍历团块长度内所有体素块的索引(进入布置前团块里的单元都是空块)
            if (mapID == 0)
            {//刷大地图
                for (int z = 0; z < 256; z++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        i++;
                        chunk.SetCellSimple(x, y, z, (ushort)(CPEngine.mapIDs[0][i] + 10));//重装机兵大地图第一个纹理编号从11开始
                    }
                }
            }
            else if (mapID > 0 && mapID < 240)
            {//刷小地图
                int width = CPEngine.mapWidths[mapID - 1];//拉多是mapId=1,格子宽度=mapWidths[0]
                int currentX = 0; // 当前列的索引
                int currentZ = 0; // 当前行的索引

                // 由于我们不知道总格子数,我们将使用一个条件来检查是否应该停止
                bool shouldStop = false;

                while (!shouldStop)
                {
                    i++; // 增加计数
                    chunk.SetCellSimple(currentX, y, currentZ, (ushort)(CPEngine.mapIDs[mapID][i] + 162));//重装机兵小地图第一个纹理编号从163开始
                    currentX++;

                    // 如果达到行宽,则换行
                    if (currentX >= width)
                    {
                        currentX = 0; // 重置列索引
                        currentZ++;   // 增加行索引

                        // 检查是否应该停止
                        if (i + 1 >= CPEngine.mapIDs[mapID].Count) //如拉多的格子数是384,达到就停止
                        {
                            shouldStop = true;
                        }
                    }
                }
            }
            else if (mapID == 240)
            {//刷龙珠大地图
                for (int z = 0; z < 349; z++)
                {
                    for (int x = 0; x < 512; x++)
                    {
                        i++;
                        chunk.SetCellSimple(x, y, z, (ushort)(CPEngine.mapIDs[mapID][i] + 1522));//龙珠大地图第一个纹理编号从1523开始
                    }
                }
            }
        }

        public void LoadUniverse()
        {
            int height = 1;
            //获取团块索引值
            int chunkx = chunk.ChunkIndex.x;
            int chunky = chunk.ChunkIndex.y;
            int chunkz = chunk.ChunkIndex.z;
            //计算要刷的地图ID
            int mapID = chunkx + chunky * 10 + chunkz * 100;

            //获取团块的长度
            int SideLength = CPEngine.chunkSideLength;

            if (CPEngine.keepSingleChunkTerrainHeight)
            {
                height = CPEngine.SingleChunkTerrainHeight;//默认高度是0
            }
            int y = height - 1;
            MMCore.Tell($"LoadUniverse: chunkx={chunkx}, chunky={chunky}, chunkz={chunkz}, mapID={mapID}, height={height}, y={y}");

            //遍历团块长度内所有体素块的索引(进入布置前团块里的单元都是空块)
            if (mapID == 0)
            {//刷原点空间

                chunk.SetCellSimple(8, 8, 8, 8);//所在空间放一土球

            }
            else if (mapID > 0 && mapID < 241)
            {

                chunk.SetCellSimple(8, 8, 8, 8);//所在空间放一土球
            }
        }
    }
}
