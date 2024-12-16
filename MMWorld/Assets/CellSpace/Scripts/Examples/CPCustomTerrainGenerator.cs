
namespace CellSpace.Examples
{
    /// <summary>
    /// �Զ�����β��ã����벼��ǰ�ſ���ĵ�Ԫ���ǿտ飩
    /// </summary>
    public class CPCustomTerrainGenerator : CPTerrainGenerator
    {
        /// <summary>
        /// mapID=0Ϊ���ͼ��С��ͼ��1~239��ʼ��1��������,240��������ͼ����
        /// </summary>
        /// <param name="mapID"></param>
        void LoadMap(int mapID)
        {
            int i = -1;
            if (mapID == 0)
            {//ˢ���ͼ
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[0][i] + 10));//��װ�������ͼ��һ�������Ŵ�11��ʼ
                        }
                    }
                }
            }
            else if (mapID > 0 && mapID<240)
            {//ˢС��ͼ
                int width = CPEngine.mapWidths[mapID - 1];//������mapId=1�����ӿ��=mapWidths[0]
                int currentX = 0; // ��ǰ�е�����
                int currentY = 0; // ��ǰ�е�����

                // �������ǲ�֪���ܸ����������ǽ�ʹ��һ������������Ƿ�Ӧ��ֹͣ
                bool shouldStop = false;

                while (!shouldStop)
                {
                    i++; // ���Ӽ���
                    if (CPEngine.HorizontalMode)
                    {
                        chunk.SetCellSimple(currentX, currentY, (ushort)(CPEngine.mapIDs[mapID][i] + 162));//��װ����С��ͼ��һ�������Ŵ�163��ʼ
                    }
                    currentX++;

                    // ����ﵽ�п�����
                    if (currentX >= width)
                    {
                        currentX = 0; // ����������
                        currentY++;   // ����������

                        // ����Ƿ�Ӧ��ֹͣ
                        if (i + 1 >= CPEngine.mapIDs[mapID].Count) //������ĸ�������384���ﵽ��ֹͣ
                        {
                            shouldStop = true;
                        }
                    }
                }
            }
            else if (mapID == 240)
            {//ˢ������ͼ
                for (int y = 0; y < 349; y++)
                {
                    for (int x = 0; x < 512; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[240][i] + 1522));//������ͼ��һ�������Ŵ�1523��ʼ
                        }
                    }
                }
            }
        }

        public override void GenerateCellData()
        {
            
            ////��ȡ�ſ�������Yֵ
            //int chunky = chunk.ChunkIndex.y;
            ////��ȡ�ſ�ĳ���
            //int SideLength = CPEngine.ChunkSideLength;
            ////�������ã��Ƿ�ƽ����
            //CPEngine.KeepTerrainHeight = false;
            ////���ú㶨���θ߶ȣ��������꣩
            //CPEngine.TerrainHeight = 8;

            LoadMap(0);

            //�����ſ鳤�������е�Ԫ����������������ı����Դ�����ԭ�㿪ʼ��
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
            //            Vector3 voxelPos = chunk.CellIndexToPosition(pixelX, pixelY); //��ȡ��Ԫ�������������λ��
            //            int currentHeight = pixelY + (SideLength * chunky); //��õ�Ԫ�ģ��������꣩����߶�
            //            if (CPEngine.KeepTerrainHeight)
            //            {
            //                if (CPEngine.TerrainHeight > currentHeight)
            //                {
            //                    //����ָ���������ĵ�Ԫ���ݣ����޸ĵ�Ԫ�����ࣩ
            //                    chunk.SetCellSimple(pixelX, pixelY, 2);   // set grass.����Ϊ�ݵأ�2�ǲݵص�����ID

            //                }
            //                currentHeight = currentHeight + 1; //��������ʱƫ��1(��Ϊ������Ҫ�ݸ���1��)
            //                if (CPEngine.TerrainHeight > currentHeight)
            //                {
            //                    //����ָ���������ĵ�Ԫ���ݣ����޸ĵ�Ԫ�����ࣩ
            //                    chunk.SetCellSimple(pixelX, pixelY, 1); // set dirt.����Ϊ���飬1�����������ID

            //                }
            //            }
            //            else
            //            {
            //                //index = ConvertLeftBottomToTopLeft(pixelY*256+pixelX+1,256,256);//��ȡ�����Ͻ�ɨ��ʱ�ĸõ�Ԫλ�ñ��
            //                //id = int.Parse(Main_MMWorld.mapContents[0][index])+10;//�ñ�ŵõ����ͼ����ID�������1��Ӧcell_11
            //                //Debug.Log(index+" "+id);
            //                chunk.SetCellSimple(pixelX, pixelY, 1);
            //            }
            //        }
            //        else
            //        {
            //            for (int z = 0; z < SideLength; z++)
            //            { // for all voxels in the chunk
            //                Vector3 voxelPos = chunk.CellIndexToPosition(pixelX, pixelY, z); // get absolute position for the voxel.��ȡ��Ԫ�������������λ��
            //                voxelPos = new Vector3(voxelPos.pixelX + seed, voxelPos.pixelY, voxelPos.z + seed); // offset by seed.���������ӽ�������������������30625��������
            //                                                                                          //Mathf.PerlinNoise()�����̶����γɹ̶���������ֻ����������������ؽ���ӽ�ĳ�ָ߶ȣ��ٱ����ſ��ڵ�Ԫ�߶����ȶԣ��Ϳɲ��øø߶���״�ϵĵ�Ԫ�����γɹ̶���ò
            //                                                                                          //������Ҫ���Σ���ɽ��Сɽ��
            //                float perlin1 = Mathf.PerlinNoise(voxelPos.pixelX * 0.010f, voxelPos.z * 0.010f) * 70.1f; // major (mountains & big hills)��22���Ҳ�������ֵ�仯ƽ�ȣ�����ϵ��0.010f��
            //                                                                                                     //�����Ҫ���Σ����µ�ϸ�ڣ�
            //                float perlin2 = Mathf.PerlinNoise(voxelPos.pixelX * 0.085f, voxelPos.z * 0.085f) * 9.1f; // minor (fine detail)��3-8���Ҳ�������ֵ�仯���ң�����ϵ��0.085f��
            //                int currentHeight = pixelY + (SideLength * chunky); // get absolute height for the voxel.��õ�Ԫ�ģ��������꣩����߶�
            //                bool setToGrass = false;
            //                //���������22��һ�������ĸ߶�ֵ������x pixelY z ���ڱ����ſ���ÿ����Ԫ��������Ȼ��ת��Ϊ����߶ȣ�������Ԫ�߶ȶԱȺ���ϸ߶�����о��岼��
            //                if (CPEngine.KeepTerrainHeight)
            //                {
            //                    if (CPEngine.TerrainHeight > currentHeight)
            //                    {
            //                        //����ָ���������ĵ�Ԫ���ݣ����޸ĵ�Ԫ�����ࣩ
            //                        chunk.SetCellSimple(pixelX, pixelY, z, 2);   // set grass.����Ϊ�ݵأ�2�ǲݵص�����ID
            //                        setToGrass = true;
            //                    }
            //                    currentHeight = currentHeight + 1; //��������ʱƫ��1(��Ϊ������Ҫ�ݸ���1��)
            //                    if (CPEngine.TerrainHeight > currentHeight)
            //                    {
            //                        //����ָ���������ĵ�Ԫ���ݣ����޸ĵ�Ԫ�����ࣩ
            //                        chunk.SetCellSimple(pixelX, pixelY, z, 1); // set dirt.����Ϊ���飬1�����������ID
            //                        setToGrass = false;
            //                    }
            //                    //�������ڳ��͵ĵ����ϲ�����
            //                    //if (setToGrass && TreeCanFit(pixelX, pixelY, z))
            //                    //{
            //                    //    //1%���ʲ�����
            //                    //    if (Random.Range(0.0f, 1.0f) < 0.01f)
            //                    //    {
            //                    //        AddTree(pixelX, pixelY + 1, z);
            //                    //    }
            //                    //}
            //                }
            //                else
            //                {
            //                    // grass pass.���òݵأ�����1���ڵ�ǰ�߶�ʱ��
            //                    if (perlin1 > currentHeight)
            //                    {
            //                        //�������1��������2+��ǰ�߶�
            //                        if (perlin1 > perlin2 + currentHeight)
            //                        {
            //                            //����ָ���������ĵ�Ԫ���ݣ����޸ĵ�Ԫ�����ࣩ
            //                            chunk.SetCellSimple(pixelX, pixelY, z, 2);   // set grass.����Ϊ�ݵأ�2�ǲݵص�����ID
            //                            setToGrass = true;
            //                        }
            //                    }
            //                    // dirt pass
            //                    currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).��������ʱƫ��1(��Ϊ������Ҫ�ݸ���1��)
            //                    if (perlin1 > currentHeight)
            //                    {
            //                        //�������1���ڣ�����2+��ǰ�߶ȣ�
            //                        if (perlin1 > perlin2 + currentHeight)
            //                        {
            //                            //����ָ���������ĵ�Ԫ���ݣ����޸ĵ�Ԫ�����ࣩ
            //                            chunk.SetCellSimple(pixelX, pixelY, z, 1); // set dirt.����Ϊ���飬1�����������ID
            //                            setToGrass = false;
            //                        }
            //                    }
            //                    // tree pass.�������ڳ��͵ĵ����ϲ�����
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
        /// �õ�Ԫ�������������
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
        /// �õ�Ԫ�������������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool TreeCanFit(int x, int y)
        {
            return false;
        }

        /// <summary>
        /// ��������ɾ�TreeCanFit��֤����ִ�У�
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
        /// ��������ɾ�TreeCanFit��֤����ִ�У�
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void AddTree(int x, int y)
        {

        }
    }
}