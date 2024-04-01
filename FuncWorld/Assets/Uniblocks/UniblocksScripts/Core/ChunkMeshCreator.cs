using UnityEngine;
using System.Collections.Generic;

// Handles mesh creation and all related functions.

namespace Uniblocks
{
    /// <summary>
    /// 团块旋转
    /// </summary>
    public enum MeshRotation
    {
        /// <summary>
        /// 团块无旋转
        /// </summary>
        none, 
        /// <summary>
        /// 团块向后旋转
        /// </summary>
        back, 
        /// <summary>
        /// 团块向左旋转
        /// </summary>
        left, 
        /// <summary>
        /// 团块向右旋转
        /// </summary>
        right
    }

    /// <summary>
    /// 团块网格创建器组件
    /// </summary>
    public class ChunkMeshCreator : MonoBehaviour
    {
        /// <summary>
        /// 团块
        /// </summary>
        private Chunk chunk;
        /// <summary>
        /// 团块边长
        /// </summary>
        private int SideLength;
        /// <summary>
        /// 无碰撞碰撞体
        /// </summary>
        private GameObject noCollideCollider;
        /// <summary>
        /// 立方体网格
        /// </summary>
        public Mesh Cube;

        // variables for storing the mesh data.存储网格数据的变量

        /// <summary>
        /// 存储网格顶点的数组
        /// </summary>
        private List<Vector3> Vertices = new List<Vector3>();
        /// <summary>
        /// 存储网格面（多个顶点组）的数组
        /// </summary>
        private List<List<int>> Faces = new List<List<int>>();
        /// <summary>
        /// 存储网格上纹理的数组
        /// </summary>
        private List<Vector2> UVs = new List<Vector2>();
        /// <summary>
        /// 网格面的计数
        /// </summary>
        private int FaceCount;

        // variables for storing collider data.存储碰撞体数据的变量

        /// <summary>
        /// 存储实心碰撞体顶点的数组
        /// </summary>
        private List<Vector3> SolidColliderVertices = new List<Vector3>();
        /// <summary>
        /// 存储实心碰撞体的面（多个顶点组）的数组
        /// </summary>
        private List<int> SolidColliderFaces = new List<int>();
        /// <summary>
        /// 实心（完全不透明）的面的计数
        /// </summary>
        private int SolidFaceCount;
        /// <summary>
        /// 存储无碰撞（透明或半透明的体素块）顶点的数组
        /// </summary>
        private List<Vector3> NoCollideVertices = new List<Vector3>();
        /// <summary>
        /// 存储无碰撞的面（多个顶点组）的数组
        /// </summary>
        private List<int> NoCollideFaces = new List<int>();
        /// <summary>
        /// 无碰撞的面的计数
        /// </summary>
        private int NoCollideFaceCount;
        /// <summary>
        /// 团块网格创建器组件初始化状态
        /// </summary>
        private bool initialized;

        /// <summary>
        /// 团块网格创建器组件初始化
        /// </summary>
        public void Initialize()
        {
            // set variables.读取团块及其长度
            chunk = GetComponent<Chunk>();
            SideLength = chunk.SideLength;

            // make a list for each material (each material is a submesh).为每一种材质列一个清单(每一种材质都是子网格)
            for (int i = 0; i < GetComponent<Renderer>().materials.Length; i++)
            {
                Faces.Add(new List<int>()); //将空的清单添加到存储网格面（多个顶点组）的数组
            }
            //初始化完毕
            initialized = true;
        }

        // ==== Voxel updates =====================================================================================


        /// <summary>
        /// 立即重建团块网格，在大多数情况下应使用Chunk.FlagToUpdate()
        /// </summary>
        public void RebuildMesh()
        {
            //如果团块网格创建器组件还没初始化则进行初始化
            if (!initialized)
            {
                Initialize();
            }

            // destroy additional mesh containers.销毁额外的网状容器
            foreach (Transform child in transform)
            {
                //遍历子物体的游戏物体进行摧毁
                Destroy(child.gameObject);
            }

            int x = 0, y = 0, z = 0;

            // Refresh neighbor chunks.刷新相邻团块
            chunk.GetNeighbors();

            // for each voxel in Voxels, check if any of the voxel's faces are exposed, and if so, add their faces to the main mesh arrays (named Vertices and Faces)

            //意思是对于Voxels中的每一个体素（voxel），检查它是否有任何一面是暴露在外的，如果有就把这一面添加到体素网格数组（名为 Vertices 和 Faces的列表）中
            //Vertices和Faces是两个数组，分别表示三维空间中的顶点坐标和多边形的面信息
            //Vertices 数组存储了所有顶点的坐标，而 Faces 数组则存储了由这些顶点构成的多边形的面信息
            //如果一个体素的某个面暴露在外，那么就会把该面的顶点坐标添加到 Vertices 数组中，并把该面的信息（即哪些顶点构成了这个面）添加到 Faces 数组中
            //通过这种方式，可以构建出一个包含所有体素的三维模型，便于后续的渲染和显示

            while (x < SideLength)
            {
                while (y < SideLength)
                {
                    while (z < SideLength)
                    {
                        //获取体素数据（体素块的种类）
                        ushort voxel = chunk.GetVoxel(x, y, z); // the current voxel data
                        if (voxel != 0)
                        { // don't render empty blocks.不渲染空块
                            //获取体素数据（体素块的种类）对应的体素（类型）
                            Voxel voxelType = Engine.GetVoxelType(voxel);
                            //体素的自定义网格未启用时
                            if (voxelType.VCustomMesh == false)
                            { // if cube.如果是立方体

                                //Transparency transparency = Engine.GetVoxelType (chunk.GetVoxel(x,y,z)).VTransparency;
                                Transparency transparency = voxelType.VTransparency; //获取体素透明度
                                ColliderType colliderType = voxelType.VColliderType; //获取体素碰撞类型
                                //检查相邻体素然后决定这个面是否需要被创建
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

                                // if no collider, create a trigger cube collider.如果没有碰撞体，创建一个立方体碰撞体
                                if (colliderType == ColliderType.none && Engine.GenerateColliders)
                                {
                                    //将立方体的顶点和面添加到所选列表中(对于Solid或nocollision碰撞器)
                                    AddCubeMesh(x, y, z, false);
                                }
                            }
                            else
                            { // if not cube.如果不是立方体
                                if (CheckAllAdjacent(x, y, z) == false) //检查所有相邻体素块是否实体
                                { // if any adjacent voxel isn't opaque, we render the mesh.如果任何相邻体素不是不透明的，自定义渲染网格
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
        /// 检查相邻体素然后决定这个面是否需要被创建
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="direction"></param>
        /// <param name="transparency"></param>
        /// <returns>如果一个面应该被创建时返回true</returns>
        private bool CheckAdjacent(int x, int y, int z, Direction direction, Transparency transparency)
        { // returns true if a face should be spawned

            Index index = chunk.GetAdjacentIndex(x, y, z, direction);
            ushort adjacentVoxel = chunk.GetVoxel(index.x, index.y, index.z);

            if (adjacentVoxel == ushort.MaxValue)
            { // if the neighbor chunk is missing.如果相邻团块是缺失状态
                //如果设置需要显示边界面或者这个面的朝向是向上的
                if (Engine.ShowBorderFaces || direction == Direction.up)
                {
                    //面是需要创建的，返回真
                    return true;
                }
                else
                {
                    return false;
                }

            }

            Transparency result = Engine.GetVoxelType(adjacentVoxel).VTransparency; // get the transparency of the adjacent voxel.获取相邻体素的透明度

            // parse the result (taking into account the transparency of the adjacent block as well as the one doing this check)
            //解析结果(考虑相邻体素块及执行此检查的体素块的透明度)
            if (transparency == Transparency.transparent)
            {
                //如果是透明的
                if (result == Transparency.transparent)
                    return false; // don't draw a transparent block next to another transparent block.禁止在一个完全透明体素块旁边绘制另一个透明块
                else
                    return true; // draw a transparent block next to a solid or semi-transparent.允许在实体或半透明旁边画一个透明的块
            }
            else
            {
                if (result == Transparency.solid)
                    return false; // don't draw a solid block or a semi-transparent block next to a solid block.禁止在实体体素块旁边画实体或半透明体素块
                else
                    return true; // draw a solid block or a semi-transparent block next to both transparent and semi-transparent.允许在透明和半透明体素块旁绘制一个实心或半透明体素块
            }
        }

        /// <summary>
        /// 检查所有相邻体素块是否实体
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>如果所有相邻体素块都是实体，则返回true</returns>
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
            Vector2 tOffset = Engine.GetTextureOffset(voxel, facing); // get texture offset for this voxel type.获取此体素类型的纹理偏移点

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
        /// 创建自定义网格
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
        /// 将立方体的顶点和面添加到所选列表中(对于Solid或nocollision碰撞器)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="solid">是否实心</param>
        private void AddCubeMesh(int x, int y, int z, bool solid)
        { // adds cube verts and faces to the chosen lists (for Solid or NoCollide colliders)

            //透明度是实心的情况
            if (solid)
            {
                // vertices
                foreach (Vector3 vertex in Cube.vertices)
                {
                    //遍历每个立方体网格顶点，按体素索引的位置修正（移动），然后将它们存储到实心碰撞体顶点数组中
                    SolidColliderVertices.Add(vertex + new Vector3(x, y, z)); // add all vertices from the mesh.从网格添加所有顶点
                }

                // faces
                foreach (int face in Cube.triangles)
                {
                    //遍历每个面（三角形顶点索引组012023这样的排列，第二个立方体会接续）
                    SolidColliderFaces.Add(SolidFaceCount + face);
                }

                // Add to the face count.一个立方体顶点计数完成后进行叠加，直到形成一个大块整体
                SolidFaceCount += Cube.vertexCount;
            }
            //其他情况（透明或半透明，就是无碰撞了）
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