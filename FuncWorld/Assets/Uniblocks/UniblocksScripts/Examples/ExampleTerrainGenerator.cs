using UnityEngine;

namespace Uniblocks
{
    /// <summary>
    /// ��������������
    /// </summary>
    public class ExampleTerrainGenerator : TerrainGenerator
    {
        /// <summary>
        /// [override]�����������ݣ����ؿ�����ࣩ
        /// </summary>
        public override void GenerateVoxelData()
        {
            //��ȡ�ſ�������Yֵ
            int chunky = chunk.ChunkIndex.y;
            //��ȡ�ſ�ĳ���
            int SideLength = Engine.ChunkSideLength;
            Engine.KeepTerrainHeight = true;
            Engine.TerrainHeight = 32;

            // debug
            int random = Random.Range(0, 10);

            //�����ſ鳤�����������ؿ�����������벼��ǰ�ſ�������ض��ǿտ飩
            for (int x = 0; x < SideLength; x++)
            {
                for (int y = 0; y < SideLength; y++)
                {
                    for (int z = 0; z < SideLength; z++)
                    { // for all voxels in the chunk

                        Vector3 voxelPos = chunk.VoxelIndexToPosition(x, y, z); // get absolute position for the voxel.��ȡ���������ľ�������λ��
                        voxelPos = new Vector3(voxelPos.x + seed, voxelPos.y, voxelPos.z + seed); // offset by seed.���������ӽ�������������������30625��������

                        //Mathf.PerlinNoise()�����̶����γɹ̶���������ֻ����������������ؽ���ӽ�ĳ�ָ߶ȣ��ٱ����ſ������ؿ�߶����ȶԣ��Ϳɲ��øø߶���״�ϵ����ؿ������γɹ̶���ò

                        //������Ҫ���Σ���ɽ��Сɽ��
                        float perlin1 = Mathf.PerlinNoise(voxelPos.x * 0.010f, voxelPos.z * 0.010f) * 70.1f; // major (mountains & big hills)��22���Ҳ�������ֵ�仯ƽ�ȣ�����ϵ��0.010f��
                        //�����Ҫ���Σ����µ�ϸ�ڣ�
                        float perlin2 = Mathf.PerlinNoise(voxelPos.x * 0.085f, voxelPos.z * 0.085f) * 9.1f; // minor (fine detail)��3-8���Ҳ�������ֵ�仯���ң�����ϵ��0.085f��

                        int currentHeight = y + (SideLength * chunky); // get absolute height for the voxel.������صģ��������꣩����߶�

                        //���������22��һ�������ĸ߶�ֵ������x y z ���ڱ����ſ���ÿ�����ؿ��������Ȼ��ת��Ϊ����߶ȣ��������ؿ�߶ȶԱȺ���ϸ߶�����о��岼��

                        if (Engine.KeepTerrainHeight)
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
                                    //����ָ�����������������ݣ����޸����ؿ�����ࣩ
                                    chunk.SetVoxelSimple(x, y, z, 2);   // set grass.����Ϊ�ݵأ�2�ǲݵص�����ID

                                }
                            }

                            // dirt pass.�������飨����1���ڵ�ǰ�߶�ʱ��
                            currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).��������ʱƫ��1(��Ϊ������Ҫ�ݸ���1��)
                            if (perlin1 > currentHeight)
                            {
                                //�������1��������2+��ǰ�߶�
                                if (perlin1 > perlin2 + currentHeight)
                                {
                                    //����ָ�����������������ݣ����޸����ؿ�����ࣩ
                                    chunk.SetVoxelSimple(x, y, z, 1); // set dirt.����Ϊ���飬1�����������ID
                                }
                            }

                            // debug.�����1ʱ
                            if (random == 1)
                            {
                                //chunk.SetVoxelSimple(x,y,z, 3); // set stone or whatever.����ʯͷ������������ؿ�
                            }
                        }
                    }
                }
            }
        }
    }

}