using UnityEngine;

namespace Uniblocks
{

    public class ExampleTerrainGeneratorWithTrees : TerrainGenerator
    {

        public override void GenerateVoxelData()
        {
            //��ȡ�ſ�������Yֵ
            int chunky = chunk.ChunkIndex.y;
            //��ȡ�ſ�ĳ���
            int SideLength = Engine.ChunkSideLength;

            //�������ã��Ƿ�ƽ����
            Engine.KeepTerrainHeight = true;
            //���ú㶨���θ߶ȣ��������꣩
            Engine.TerrainHeight = 32;

            //�����ſ鳤�����������ؿ�����������벼��ǰ�ſ�������ض��ǿտ飩
            for (int x = 0; x < SideLength; x++)
            {
                for (int y = 0; y < SideLength; y++)
                {
                    for (int z = 0; z < SideLength; z++)
                    { // for all voxels in the chunk

                        Vector3 voxelPos = chunk.VoxelIndexToPosition(x, y, z); // get absolute position for the voxel.��ȡ���������ľ�������λ��
                        voxelPos = new Vector3(voxelPos.x + seed, voxelPos.y, voxelPos.z + seed); // offset by seed.���������ӽ�������������������30625��������

                        float perlin1 = Mathf.PerlinNoise(voxelPos.x * 0.010f, voxelPos.z * 0.010f) * 70.1f; // major (mountains & big hills)��22���Ҳ�������ֵ�仯ƽ�ȣ�����ϵ��0.010f��
                        float perlin2 = Mathf.PerlinNoise(voxelPos.x * 0.085f, voxelPos.z * 0.085f) * 9.1f; // minor (fine detail)��3-8���Ҳ�������ֵ�仯���ң�����ϵ��0.085f��

                        int currentHeight = y + (SideLength * chunky); // get absolute height for the voxel.������صģ��������꣩����߶�
                        bool setToGrass = false;

                        //���������22��һ�������ĸ߶�ֵ������x y z ���ڱ����ſ���ÿ�����ؿ��������Ȼ��ת��Ϊ����߶ȣ��������ؿ�߶ȶԱȺ���ϸ߶�����о��岼��

                        if (Engine.KeepTerrainHeight)
                        {
                            if (Engine.TerrainHeight > currentHeight)
                            {
                                //����ָ�����������������ݣ����޸����ؿ�����ࣩ
                                chunk.SetVoxelSimple(x, y, z, 2);   // set grass.����Ϊ�ݵأ�2�ǲݵص�����ID
                                setToGrass = true;
                            }
                            currentHeight = currentHeight + 1; //��������ʱƫ��1(��Ϊ������Ҫ�ݸ���1��)
                            if (Engine.TerrainHeight > currentHeight)
                            {
                                //����ָ�����������������ݣ����޸����ؿ�����ࣩ
                                chunk.SetVoxelSimple(x, y, z, 1); // set dirt.����Ϊ���飬1�����������ID
                                setToGrass = false;
                            }
                            //�������ڳ��͵ĵ����ϲ�����
                            if (setToGrass && TreeCanFit(x, y, z))
                            {
                                //1%���ʲ�����
                                if (Random.Range(0.0f, 1.0f) < 0.01f)
                                {
                                    AddTree(x, y + 1, z);
                                }
                            }
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
                                    setToGrass = true;
                                }
                            }

                            // dirt pass
                            currentHeight = currentHeight + 1; // offset dirt by 1 (since we want grass 1 block higher).��������ʱƫ��1(��Ϊ������Ҫ�ݸ���1��)
                            if (perlin1 > currentHeight)
                            {
                                //�������1���ڣ�����2+��ǰ�߶ȣ�
                                if (perlin1 > perlin2 + currentHeight)
                                {
                                    //����ָ�����������������ݣ����޸����ؿ�����ࣩ
                                    chunk.SetVoxelSimple(x, y, z, 1); // set dirt.����Ϊ���飬1�����������ID
                                    setToGrass = false;
                                }
                            }

                            // tree pass.�������ڳ��͵ĵ����ϲ�����
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