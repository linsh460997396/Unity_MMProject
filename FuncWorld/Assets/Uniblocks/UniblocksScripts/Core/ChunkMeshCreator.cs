using UnityEngine;
using System.Collections.Generic;

// Handles mesh creation and all related functions.

namespace Uniblocks
{
    /// <summary>
    /// �ſ���ת
    /// </summary>
    public enum MeshRotation
    {
        /// <summary>
        /// �ſ�����ת
        /// </summary>
        none, 
        /// <summary>
        /// �ſ������ת
        /// </summary>
        back, 
        /// <summary>
        /// �ſ�������ת
        /// </summary>
        left, 
        /// <summary>
        /// �ſ�������ת
        /// </summary>
        right
    }

    /// <summary>
    /// �ſ����񴴽������
    /// </summary>
    public class ChunkMeshCreator : MonoBehaviour
    {
        /// <summary>
        /// �ſ�
        /// </summary>
        private Chunk chunk;
        /// <summary>
        /// �ſ�߳�
        /// </summary>
        private int SideLength;
        /// <summary>
        /// ����ײ��ײ��
        /// </summary>
        private GameObject noCollideCollider;
        /// <summary>
        /// ����������
        /// </summary>
        public Mesh Cube;

        // variables for storing the mesh data.�洢�������ݵı���

        /// <summary>
        /// �洢���񶥵������
        /// </summary>
        private List<Vector3> Vertices = new List<Vector3>();
        /// <summary>
        /// �洢�����棨��������飩������
        /// </summary>
        private List<List<int>> Faces = new List<List<int>>();
        /// <summary>
        /// �洢���������������
        /// </summary>
        private List<Vector2> UVs = new List<Vector2>();
        /// <summary>
        /// ������ļ���
        /// </summary>
        private int FaceCount;

        // variables for storing collider data.�洢��ײ�����ݵı���

        /// <summary>
        /// �洢ʵ����ײ�嶥�������
        /// </summary>
        private List<Vector3> SolidColliderVertices = new List<Vector3>();
        /// <summary>
        /// �洢ʵ����ײ����棨��������飩������
        /// </summary>
        private List<int> SolidColliderFaces = new List<int>();
        /// <summary>
        /// ʵ�ģ���ȫ��͸��������ļ���
        /// </summary>
        private int SolidFaceCount;
        /// <summary>
        /// �洢����ײ��͸�����͸�������ؿ飩���������
        /// </summary>
        private List<Vector3> NoCollideVertices = new List<Vector3>();
        /// <summary>
        /// �洢����ײ���棨��������飩������
        /// </summary>
        private List<int> NoCollideFaces = new List<int>();
        /// <summary>
        /// ����ײ����ļ���
        /// </summary>
        private int NoCollideFaceCount;
        /// <summary>
        /// �ſ����񴴽��������ʼ��״̬
        /// </summary>
        private bool initialized;

        /// <summary>
        /// �ſ����񴴽��������ʼ��
        /// </summary>
        public void Initialize()
        {
            // set variables.��ȡ�ſ鼰�䳤��
            chunk = GetComponent<Chunk>();
            SideLength = chunk.SideLength;

            // make a list for each material (each material is a submesh).Ϊÿһ�ֲ�����һ���嵥(ÿһ�ֲ��ʶ���������)
            for (int i = 0; i < GetComponent<Renderer>().materials.Length; i++)
            {
                Faces.Add(new List<int>()); //���յ��嵥��ӵ��洢�����棨��������飩������
            }
            //��ʼ�����
            initialized = true;
        }

        // ==== Voxel updates =====================================================================================


        /// <summary>
        /// �����ؽ��ſ������ڴ���������Ӧʹ��Chunk.FlagToUpdate()
        /// </summary>
        public void RebuildMesh()
        {
            //����ſ����񴴽��������û��ʼ������г�ʼ��
            if (!initialized)
            {
                Initialize();
            }

            // destroy additional mesh containers.���ٶ������״����
            foreach (Transform child in transform)
            {
                //�������������Ϸ������дݻ�
                Destroy(child.gameObject);
            }

            int x = 0, y = 0, z = 0;

            // Refresh neighbor chunks.ˢ�������ſ�
            chunk.GetNeighbors();

            // for each voxel in Voxels, check if any of the voxel's faces are exposed, and if so, add their faces to the main mesh arrays (named Vertices and Faces)

            //��˼�Ƕ���Voxels�е�ÿһ�����أ�voxel����������Ƿ����κ�һ���Ǳ�¶����ģ�����оͰ���һ����ӵ������������飨��Ϊ Vertices �� Faces���б���
            //Vertices��Faces���������飬�ֱ��ʾ��ά�ռ��еĶ�������Ͷ���ε�����Ϣ
            //Vertices ����洢�����ж�������꣬�� Faces ������洢������Щ���㹹�ɵĶ���ε�����Ϣ
            //���һ�����ص�ĳ���汩¶���⣬��ô�ͻ�Ѹ���Ķ���������ӵ� Vertices �����У����Ѹ������Ϣ������Щ���㹹��������棩��ӵ� Faces ������
            //ͨ�����ַ�ʽ�����Թ�����һ�������������ص���άģ�ͣ����ں�������Ⱦ����ʾ

            while (x < SideLength)
            {
                while (y < SideLength)
                {
                    while (z < SideLength)
                    {
                        //��ȡ�������ݣ����ؿ�����ࣩ
                        ushort voxel = chunk.GetVoxel(x, y, z); // the current voxel data
                        if (voxel != 0)
                        { // don't render empty blocks.����Ⱦ�տ�
                            //��ȡ�������ݣ����ؿ�����ࣩ��Ӧ�����أ����ͣ�
                            Voxel voxelType = Engine.GetVoxelType(voxel);
                            //���ص��Զ�������δ����ʱ
                            if (voxelType.VCustomMesh == false)
                            { // if cube.�����������

                                //Transparency transparency = Engine.GetVoxelType (chunk.GetVoxel(x,y,z)).VTransparency;
                                Transparency transparency = voxelType.VTransparency; //��ȡ����͸����
                                ColliderType colliderType = voxelType.VColliderType; //��ȡ������ײ����
                                //�����������Ȼ�����������Ƿ���Ҫ������
                                if (CheckAdjacent(x, y, z, Direction.forward, transparency) == true)
                                    CreateFace(voxel, Facing.forward, colliderType, x, y, z);

                                if (CheckAdjacent(x, y, z, Direction.back, transparency) == true)
                                    CreateFace(voxel, Facing.back, colliderType, x, y, z);

                                if (CheckAdjacent(x, y, z, Direction.up, transparency) == true)
                                    CreateFace(voxel, Facing.up, colliderType, x, y, z);

                                if (CheckAdjacent(x, y, z, Direction.down, transparency) == true)
                                    CreateFace(voxel, Facing.down, colliderType, x, y, z);

                                if (CheckAdjacent(x, y, z, Direction.right, transparency) == true)
                                    CreateFace(voxel, Facing.right, colliderType, x, y, z);

                                if (CheckAdjacent(x, y, z, Direction.left, transparency) == true)
                                    CreateFace(voxel, Facing.left, colliderType, x, y, z);

                                // if no collider, create a trigger cube collider.���û����ײ�壬����һ����������ײ��
                                if (colliderType == ColliderType.none && Engine.GenerateColliders)
                                {
                                    //��������Ķ��������ӵ���ѡ�б���(����Solid��nocollision��ײ��)
                                    AddCubeMesh(x, y, z, false);
                                }
                            }
                            else
                            { // if not cube.�������������
                                if (CheckAllAdjacent(x, y, z) == false) //��������������ؿ��Ƿ�ʵ��
                                { // if any adjacent voxel isn't opaque, we render the mesh.����κ��������ز��ǲ�͸���ģ��Զ�����Ⱦ����
                                    CreateCustomMesh(voxel, x, y, z, voxelType.VMesh);
                                }
                            }
                        }
                        z += 1;
                    }
                    z = 0;
                    y += 1;

                }
                y = 0;
                x += 1;
            }

            // update mesh using the values from the arrays
            UpdateMesh(GetComponent<MeshFilter>().mesh);
        }

        /// <summary>
        /// �����������Ȼ�����������Ƿ���Ҫ������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="direction"></param>
        /// <param name="transparency"></param>
        /// <returns>���һ����Ӧ�ñ�����ʱ����true</returns>
        private bool CheckAdjacent(int x, int y, int z, Direction direction, Transparency transparency)
        { // returns true if a face should be spawned

            Index index = chunk.GetAdjacentIndex(x, y, z, direction);
            ushort adjacentVoxel = chunk.GetVoxel(index.x, index.y, index.z);

            if (adjacentVoxel == ushort.MaxValue)
            { // if the neighbor chunk is missing.��������ſ���ȱʧ״̬
                //���������Ҫ��ʾ�߽�����������ĳ��������ϵ�
                if (Engine.ShowBorderFaces || direction == Direction.up)
                {
                    //������Ҫ�����ģ�������
                    return true;
                }
                else
                {
                    return false;
                }

            }

            Transparency result = Engine.GetVoxelType(adjacentVoxel).VTransparency; // get the transparency of the adjacent voxel.��ȡ�������ص�͸����

            // parse the result (taking into account the transparency of the adjacent block as well as the one doing this check)
            //�������(�����������ؿ鼰ִ�д˼������ؿ��͸����)
            if (transparency == Transparency.transparent)
            {
                //�����͸����
                if (result == Transparency.transparent)
                    return false; // don't draw a transparent block next to another transparent block.��ֹ��һ����ȫ͸�����ؿ��Ա߻�����һ��͸����
                else
                    return true; // draw a transparent block next to a solid or semi-transparent.������ʵ����͸���Ա߻�һ��͸���Ŀ�
            }
            else
            {
                if (result == Transparency.solid)
                    return false; // don't draw a solid block or a semi-transparent block next to a solid block.��ֹ��ʵ�����ؿ��Ա߻�ʵ����͸�����ؿ�
                else
                    return true; // draw a solid block or a semi-transparent block next to both transparent and semi-transparent.������͸���Ͱ�͸�����ؿ��Ի���һ��ʵ�Ļ��͸�����ؿ�
            }
        }

        /// <summary>
        /// ��������������ؿ��Ƿ�ʵ��
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>��������������ؿ鶼��ʵ�壬�򷵻�true</returns>
        public bool CheckAllAdjacent(int x, int y, int z)
        { // returns true if all adjacent voxels are solid

            for (int direction = 0; direction < 6; direction++)
            {
                if (Engine.GetVoxelType(chunk.GetVoxel(chunk.GetAdjacentIndex(x, y, z, (Direction)direction))).VTransparency != Transparency.solid)
                {
                    return false;
                }
            }
            return true;
        }


        // ==== mesh generation =======================================================================================

        private void CreateFace(ushort voxel, Facing facing, ColliderType colliderType, int x, int y, int z)
        {

            Voxel voxelComponent = Engine.GetVoxelType(voxel);
            List<int> FacesList = Faces[voxelComponent.VSubmeshIndex];

            // ==== Vertices ====

            // add the positions of the vertices depending on the facing of the face
            if (facing == Facing.forward)
            {
                Vertices.Add(new Vector3(x + 0.5001f, y + 0.5001f, z + 0.5f));
                Vertices.Add(new Vector3(x - 0.5001f, y + 0.5001f, z + 0.5f));
                Vertices.Add(new Vector3(x - 0.5001f, y - 0.5001f, z + 0.5f));
                Vertices.Add(new Vector3(x + 0.5001f, y - 0.5001f, z + 0.5f));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                }
            }
            else if (facing == Facing.up)
            {
                Vertices.Add(new Vector3(x - 0.5001f, y + 0.5f, z + 0.5001f));
                Vertices.Add(new Vector3(x + 0.5001f, y + 0.5f, z + 0.5001f));
                Vertices.Add(new Vector3(x + 0.5001f, y + 0.5f, z - 0.5001f));
                Vertices.Add(new Vector3(x - 0.5001f, y + 0.5f, z - 0.5001f));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                }
            }
            else if (facing == Facing.right)
            {
                Vertices.Add(new Vector3(x + 0.5f, y + 0.5001f, z - 0.5001f));
                Vertices.Add(new Vector3(x + 0.5f, y + 0.5001f, z + 0.5001f));
                Vertices.Add(new Vector3(x + 0.5f, y - 0.5001f, z + 0.5001f));
                Vertices.Add(new Vector3(x + 0.5f, y - 0.5001f, z - 0.5001f));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                }
            }
            else if (facing == Facing.back)
            {
                Vertices.Add(new Vector3(x - 0.5001f, y + 0.5001f, z - 0.5f));
                Vertices.Add(new Vector3(x + 0.5001f, y + 0.5001f, z - 0.5f));
                Vertices.Add(new Vector3(x + 0.5001f, y - 0.5001f, z - 0.5f));
                Vertices.Add(new Vector3(x - 0.5001f, y - 0.5001f, z - 0.5f));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                }
            }
            else if (facing == Facing.down)
            {
                Vertices.Add(new Vector3(x - 0.5001f, y - 0.5f, z - 0.5001f));
                Vertices.Add(new Vector3(x + 0.5001f, y - 0.5f, z - 0.5001f));
                Vertices.Add(new Vector3(x + 0.5001f, y - 0.5f, z + 0.5001f));
                Vertices.Add(new Vector3(x - 0.5001f, y - 0.5f, z + 0.5001f));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                }
            }
            else if (facing == Facing.left)
            {
                Vertices.Add(new Vector3(x - 0.5f, y + 0.5001f, z + 0.5001f));
                Vertices.Add(new Vector3(x - 0.5f, y + 0.5001f, z - 0.5001f));
                Vertices.Add(new Vector3(x - 0.5f, y - 0.5001f, z - 0.5001f));
                Vertices.Add(new Vector3(x - 0.5f, y - 0.5001f, z + 0.5001f));
                if (colliderType == ColliderType.cube && Engine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                    SolidColliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                }
            }

            // ==== UVs =====
            float tUnit = Engine.TextureUnit;
            Vector2 tOffset = Engine.GetTextureOffset(voxel, facing); // get texture offset for this voxel type.��ȡ���������͵�����ƫ�Ƶ�

            float pad = tUnit * Engine.TexturePadding;
            UVs.Add(new Vector2(tUnit * tOffset.x + pad, tUnit * tOffset.y + tUnit - pad)); // top left
            UVs.Add(new Vector2(tUnit * tOffset.x + tUnit - pad, tUnit * tOffset.y + tUnit - pad)); // top right
            UVs.Add(new Vector2(tUnit * tOffset.x + tUnit - pad, tUnit * tOffset.y + pad)); // bottom right
            UVs.Add(new Vector2(tUnit * tOffset.x + pad, tUnit * tOffset.y + pad)); // bottom left

            // ==== Faces ====

            // add the faces
            FacesList.Add(FaceCount + 0);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 3);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 2);
            FacesList.Add(FaceCount + 3);
            if (colliderType == ColliderType.cube && Engine.GenerateColliders)
            {
                SolidColliderFaces.Add(SolidFaceCount + 0);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 3);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 2);
                SolidColliderFaces.Add(SolidFaceCount + 3);
            }

            // Add to the face count
            FaceCount += 4; // we're adding 4 because there are 4 vertices in each face.
            if (colliderType == ColliderType.cube && Engine.GenerateColliders)
            {
                SolidFaceCount += 4;
            }

            // Check the amount of vertices so far and create a new mesh if necessary
            if (Vertices.Count > 65530)
            {
                CreateNewMeshObject();
            }
        }

        /// <summary>
        /// �����Զ�������
        /// </summary>
        /// <param name="voxel"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="mesh"></param>
        private void CreateCustomMesh(ushort voxel, int x, int y, int z, Mesh mesh)
        {

            Voxel voxelComponent = Engine.GetVoxelType(voxel);
            List<int> FacesList = Faces[voxelComponent.VSubmeshIndex];

            // check if mesh exists
            if (mesh == null)
            {
                Debug.LogError("Uniblocks: The voxel id " + voxel + " uses a custom mesh, but no mesh has been assigned!");
                return;
            }


            // === mesh
            // check if we still have room for more vertices in the mesh
            if (Vertices.Count + mesh.vertices.Length > 65534)
            {
                CreateNewMeshObject();
            }

            // rotate vertices depending on the mesh rotation setting
            List<Vector3> rotatedVertices = new List<Vector3>();
            MeshRotation rotation = voxelComponent.VRotation;

            // 180 horizontal (reverse all x and z)
            if (rotation == MeshRotation.back)
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(new Vector3(-vertex.x, vertex.y, -vertex.z));
                }
            }

            // 90 right
            else if (rotation == MeshRotation.right)
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(new Vector3(vertex.z, vertex.y, -vertex.x));
                }
            }

            // 90 left
            else if (rotation == MeshRotation.left)
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(new Vector3(-vertex.z, vertex.y, vertex.x));
                }
            }

            // no rotation
            else
            {
                foreach (Vector3 vertex in mesh.vertices)
                {
                    rotatedVertices.Add(vertex);
                }
            }

            // vertices
            foreach (Vector3 vertex in rotatedVertices)
            {
                Vertices.Add(vertex + new Vector3(x, y, z)); // add all vertices from the mesh
            }

            // UVs
            foreach (Vector2 uv in mesh.uv)
            {
                UVs.Add(uv);
            }

            // faces
            foreach (int face in mesh.triangles)
            {
                FacesList.Add(FaceCount + face);
            }

            // Add to the face count
            FaceCount += mesh.vertexCount;


            // === collider
            if (Engine.GenerateColliders)
            {
                ColliderType colliderType = Engine.GetVoxelType(voxel).VColliderType;  // get collider type (solid/cube/none)

                // mesh collider	
                if (colliderType == ColliderType.mesh)
                {
                    foreach (Vector3 vertex1 in rotatedVertices)
                    {
                        SolidColliderVertices.Add(vertex1 + new Vector3(x, y, z)); // if mesh collider, just add the vertices & faces from this mesh to the solid collider mesh
                    }
                    foreach (int face1 in mesh.triangles)
                    {
                        SolidColliderFaces.Add(SolidFaceCount + face1);
                    }
                    SolidFaceCount += mesh.vertexCount;
                }

                // cube collider
                if (colliderType == ColliderType.cube)
                {
                    AddCubeMesh(x, y, z, true); // if cube collider, add a cube to the solid mesh
                }
                // nocollide collider (for both ColliderType.mesh and ColliderType.none, but not for ColliderType.cube since it's redundant)
                else if (voxel != 0)
                { // only make a collider for non-empty voxels
                    AddCubeMesh(x, y, z, false); // if no cube collider, add a cube to the nocollide mesh (we still need a collider on noCollide blocks for raycasts and such)
                }
            }
        }

        /// <summary>
        /// ��������Ķ��������ӵ���ѡ�б���(����Solid��nocollision��ײ��)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="solid">�Ƿ�ʵ��</param>
        private void AddCubeMesh(int x, int y, int z, bool solid)
        { // adds cube verts and faces to the chosen lists (for Solid or NoCollide colliders)

            //͸������ʵ�ĵ����
            if (solid)
            {
                // vertices
                foreach (Vector3 vertex in Cube.vertices)
                {
                    //����ÿ�����������񶥵㣬������������λ���������ƶ�����Ȼ�����Ǵ洢��ʵ����ײ�嶥��������
                    SolidColliderVertices.Add(vertex + new Vector3(x, y, z)); // add all vertices from the mesh.������������ж���
                }

                // faces
                foreach (int face in Cube.triangles)
                {
                    //����ÿ���棨�����ζ���������012023���������У��ڶ���������������
                    SolidColliderFaces.Add(SolidFaceCount + face);
                }

                // Add to the face count.һ�������嶥�������ɺ���е��ӣ�ֱ���γ�һ���������
                SolidFaceCount += Cube.vertexCount;
            }
            //���������͸�����͸������������ײ�ˣ�
            else
            {
                // vertices
                foreach (Vector3 vertex1 in Cube.vertices)
                {
                    NoCollideVertices.Add(vertex1 + new Vector3(x, y, z));
                }

                // faces
                foreach (int face1 in Cube.triangles)
                {
                    NoCollideFaces.Add(NoCollideFaceCount + face1);
                }

                // Add to the face count 
                NoCollideFaceCount += Cube.vertexCount;
            }
        }

        private void UpdateMesh(Mesh mesh)
        {

            // Update the mesh
            mesh.Clear();
            mesh.vertices = Vertices.ToArray();
            mesh.subMeshCount = GetComponent<Renderer>().materials.Length;

            for (int i = 0; i < Faces.Count; ++i)
            {
                mesh.SetTriangles(Faces[i].ToArray(), i);
            }

            mesh.uv = UVs.ToArray();//UVs.ToBuiltin(Vector2F) as Vector2F[]	
            ;
            mesh.RecalculateNormals();

            if (Engine.GenerateColliders)
            {

                // Update solid collider
                Mesh colMesh = new Mesh();

                colMesh.vertices = SolidColliderVertices.ToArray();
                colMesh.triangles = SolidColliderFaces.ToArray();
                ;
                colMesh.RecalculateNormals();

                GetComponent<MeshCollider>().sharedMesh = null;
                GetComponent<MeshCollider>().sharedMesh = colMesh;

                // Update nocollide collider
                if (NoCollideVertices.Count > 0)
                {

                    // make mesh
                    Mesh nocolMesh = new Mesh();
                    nocolMesh.vertices = NoCollideVertices.ToArray();
                    nocolMesh.triangles = NoCollideFaces.ToArray();
                    ;
                    nocolMesh.RecalculateNormals();

                    noCollideCollider = Instantiate(chunk.ChunkCollider, transform.position, transform.rotation) as GameObject;
                    noCollideCollider.transform.parent = this.transform;
                    noCollideCollider.GetComponent<MeshCollider>().sharedMesh = nocolMesh;

                }
                else if (noCollideCollider != null)
                {
                    Destroy(noCollideCollider); // destroy the existing collider if there is no NoCollide vertices
                }
            }


            // clear the main arrays for future use.
            Vertices.Clear();
            UVs.Clear();
            foreach (List<int> faceList in Faces)
            {
                faceList.Clear();
            }

            SolidColliderVertices.Clear();
            SolidColliderFaces.Clear();

            NoCollideVertices.Clear();
            NoCollideFaces.Clear();


            FaceCount = 0;
            SolidFaceCount = 0;
            NoCollideFaceCount = 0;


        }



        private void CreateNewMeshObject()
        { // in case the amount of vertices exceeds the maximum for one mesh, we need to create a new mesh

            GameObject meshContainer = Instantiate(chunk.MeshContainer, transform.position, transform.rotation) as GameObject;
            meshContainer.transform.parent = this.transform;

            UpdateMesh(meshContainer.GetComponent<MeshFilter>().mesh);
        }


    }

}