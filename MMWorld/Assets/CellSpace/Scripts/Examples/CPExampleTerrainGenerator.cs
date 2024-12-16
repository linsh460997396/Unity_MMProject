using UnityEngine;

namespace CellSpace.Examples
{
    /// <summary>
    /// ��������������
    /// </summary>
    public class CPExampleTerrainGenerator : CPTerrainGenerator
    {
        /// <summary>
        /// ���ɵ�Ԫ���ݣ����ؿ�����ࣩ
        /// </summary>
        public override void GenerateCellData()
        {
            //��ȡ�ſ�������Yֵ
            int chunky = chunk.ChunkIndex.y;
            //��ȡ�ſ�ĳ���
            int SideLength = CPEngine.ChunkSideLength;
            CPEngine.KeepTerrainHeight = true;
            CPEngine.TerrainHeight = 8;

            // debug
            int random = Random.Range(0, 10);

            //�����ſ鳤�����������ؿ�����������벼��ǰ�ſ���ĵ�Ԫ���ǿտ飩
            for (int x = 0; x < SideLength; x++)
            {
                for (int y = 0; y < SideLength; y++)
                {
                    if (CPEngine.HorizontalMode)
                    {
                     // for all voxels in the chunk
                        Vector3 voxelPos = chunk.CellIndexToPosition(x, y); //��ȡ��Ԫ�������������λ��
                        int currentHeight = y + (SideLength * chunky); //��õ�Ԫ�ģ��������꣩����߶�
                        //δ�����
                    }
                    else
                    {
                        for (int z = 0; z < SideLength; z++)
                        { // for all Items in the chunk

                            Vector3 cellPos = chunk.CellIndexToPosition(x, y, z); // get absolute position for the cell.��ȡ��Ԫ�������������λ��
                            cellPos = new Vector3(cellPos.x + seed, cellPos.y, cellPos.z + seed); // offset by seed.���������ӽ�������������������30625��������

                            //Mathf.PerlinNoise()�����̶����γɹ̶���������ֻ����������������ؽ���ӽ�ĳ�ָ߶ȣ��ٱ����ſ������ؿ�߶����ȶԣ��Ϳɲ��øø߶���״�ϵ����ؿ������γɹ̶���ò

                            //������Ҫ���Σ���ɽ��Сɽ��
                            float perlin1 = Mathf.PerlinNoise(cellPos.x * 0.010f, cellPos.z * 0.010f) * 70.1f; // major (mountains & big hills)��22���Ҳ�������ֵ�仯ƽ�ȣ�����ϵ��0.010f��
                                                                                                               //�����Ҫ���Σ����µ�ϸ�ڣ�
                            float perlin2 = Mathf.PerlinNoise(cellPos.x * 0.085f, cellPos.z * 0.085f) * 9.1f; // minor (fine detail)��3-8���Ҳ�������ֵ�仯���ң�����ϵ��0.085f��

                            int currentHeight = y + (SideLength * chunky); // get absolute height for the cell.��õ�Ԫ�ģ��������꣩����߶�

                            //���������22��һ�������ĸ߶�ֵ������x pixelY z ���ڱ����ſ���ÿ�����ؿ��������Ȼ��ת��Ϊ����߶ȣ��������ؿ�߶ȶԱȺ���ϸ߶�����о��岼��

                            if (CPEngine.KeepTerrainHeight)
                            {

                            }
                            else
                            {
                                // grass pass.���òݵأ�����1���ڵ�ǰ�߶�ʱ��
                                if (perlin1 > currentHeight)
                                {
                                    //�������1��������2+��ǰ�߶�
                                    if (perlin1 > perlin2 + currentHeight)
                                    {
                                        //����ָ���������ĵ�Ԫ���ݣ����޸����ؿ�����ࣩ
                                        chunk.SetCellSimple(x, y, z, 2);   // set grass.����Ϊ�ݵأ�2�ǲݵص�����ID

                                    }
                                }

                                // dirt pass.�������飨����1���ڵ�ǰ�߶�ʱ��
                                currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).��������ʱƫ��1(��Ϊ������Ҫ�ݸ���1��)
                                if (perlin1 > currentHeight)
                                {
                                    //�������1��������2+��ǰ�߶�
                                    if (perlin1 > perlin2 + currentHeight)
                                    {
                                        //����ָ���������ĵ�Ԫ���ݣ����޸����ؿ�����ࣩ
                                        chunk.SetCellSimple(x, y, z, 1); // set dirt.����Ϊ���飬1�����������ID
                                    }
                                }

                                // debug.�����1ʱ
                                if (random == 1)
                                {
                                    //chunk.SetCellSimple(pixelX,pixelY,z, 3); // set stone or whatever.����ʯͷ������������ؿ�
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}