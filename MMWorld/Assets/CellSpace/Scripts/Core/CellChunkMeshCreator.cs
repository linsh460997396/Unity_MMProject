using UnityEngine;
using System.Collections.Generic;

// Handles mesh creation and all related functions.
// 体素单元（Cell/Voxel）

namespace CellSpace
{
    /// <summary>
    /// 网格旋转
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
    public class CellChunkMeshCreator : MonoBehaviour
    {
        /// <summary>
        /// 抗闪烁网格半径
        /// </summary>
        const float antiFlickerRadius = meshRadius + antiFlickerOffset;
        /// <summary>
        /// 网格半径
        /// </summary>
        const float meshRadius = 0.5f;
        /// <summary>
        /// 抗闪烁偏移量。当两个面非常接近时，在渲染时可能会出现Z-fighting即两个面因为深度值非常接近而在屏幕上闪烁，这里通过微小的额外偏移来防止
        /// </summary>
        const float antiFlickerOffset = 0.0001f;
        /// <summary>
        /// 网格位置偏移量（往正值方向进行偏移回插入点，否则默认的网格插入在原点负值方向偏移1个网格半径的位置）
        /// </summary>
        const float meshOffset = meshRadius;
        /// <summary>
        /// 团块
        /// </summary>
        private CellChunk chunk;
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
        /// 存储网格面（多个三角形顶点组）的数组，List<int>中3个元素对应一个三角形，由2个三角形或4个顶点对应1个面
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
            chunk = GetComponent<CellChunk>();
            SideLength = chunk.SideLength;
            //Debug.Log(GetComponent<Renderer>().materials.Length);
            // make a list for each material (each material is a submesh).为团块的网格渲染器组件上每一种材质列一个清单(每一种材质都是子网格)
            for (int i = 0; i < GetComponent<Renderer>().materials.Length; i++)
            {
                Faces.Add(new List<int>()); //将空的清单添加到存储网格面（多个顶点组）的数组
            }
            //初始化完毕
            initialized = true;
        }

        // ==== Cell updates =====================================================================================

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
                //遍历子游戏物体进行摧毁
                Destroy(child.gameObject);
            }

            int x = 0, y = 0, z = 0;

            // Refresh neighbor chunks.刷新相邻团块
            chunk.GetNeighbors();

            // for each cellId in Voxels, check if any of the cellId's faces are exposed, and if so, add their faces to the main mesh arrays (named Vertices and Faces)

            //意思是对于Voxels中的每一个体素，检查它是否有任何一面是暴露在外的，如果有就把这一面添加到体素网格数组（名为 Vertices 和 Faces的列表）中
            //Vertices和Faces是两个数组，分别表示三维空间中的顶点坐标和多边形的面信息
            //Vertices 数组存储了所有顶点的坐标，而 Faces 数组则存储了由这些顶点构成的多边形的面信息
            //如果一个体素的某个面暴露在外，那么就会把该面的顶点坐标添加到 Vertices 数组中，并把该面的信息（即哪些顶点构成了这个面）添加到 Faces 数组中
            //通过这种方式，可以构建出一个包含所有体素的三维模型，便于后续的渲染和显示
            if (CPEngine.HorizontalMode)
            {
                while (x < SideLength)
                {
                    while (y < SideLength)
                    {
                        //获取体素数据（体素块的种类）
                        ushort cellId = chunk.GetCellID(x, y); // the current cellId data
                        if (cellId != 0)
                        { // don'transform render empty blocks.不渲染空块
                          //获取体素数据（体素块的种类）对应的体素（类型）
                            Cell cellType = CPEngine.GetCellType(cellId);
                            //体素的自定义网格未启用时
                            if (cellType.VCustomMesh == false)
                            { // if cube.如果是立方体
                                //Transparency transparency = CPEngine.GetCellType (chunk.GetCellID(pixelX,pixelY,z)).VTransparency;
                                Transparency transparency = cellType.VTransparency; //获取体素透明度
                                ColliderType colliderType = cellType.VColliderType; //获取体素碰撞类型

                                if (CPEngine.MutiHorizontal)
                                {//多维横版功能开启后对于上下左右4个侧面，应显示最边缘的面
                                    //检查相邻体素然后决定这个面是否需要被创建（横版模式忽略前后面）
                                    ////可设计该面若是视野内边界则条件始终为真（让看到的边界封闭），也可对要封闭边界处相邻块设空块让下方动作自行判断创建封闭面
                                    if (CheckAdjacent(x, y, Direction.up, transparency) == true)
                                        CreateFace(cellId, Facing.up, colliderType, x, y);
                                    if (CheckAdjacent(x, y, Direction.down, transparency) == true)
                                        CreateFace(cellId, Facing.down, colliderType, x, y);
                                    if (CheckAdjacent(x, y, Direction.right, transparency) == true)
                                        CreateFace(cellId, Facing.right, colliderType, x, y);
                                    if (CheckAdjacent(x, y, Direction.left, transparency) == true)
                                        CreateFace(cellId, Facing.left, colliderType, x, y);
                                }

                                //CreateFace(cellId, Facing.up, colliderType, pixelX, pixelY);
                                //CreateFace(cellId, Facing.down, colliderType, pixelX, pixelY);
                                //CreateFace(cellId, Facing.right, colliderType, pixelX, pixelY);
                                //CreateFace(cellId, Facing.left, colliderType, pixelX, pixelY);
                                //CreateFace(cellId, Facing.forward, colliderType, pixelX, pixelY);

                                //多维横版功能开启，总是显示前面
                                if (CPEngine.MutiHorizontal) CreateFace(cellId, Facing.forward, colliderType, x, y);

                                //横版模式下总是显示后面（玩家屏幕看到的面）
                                CreateFace(cellId, Facing.back, colliderType, x, y);

                                // if no collider, create a trigger cube collider.如果没有碰撞体，创建一个立方体碰撞体
                                if (colliderType == ColliderType.none && CPEngine.GenerateColliders)
                                {
                                    //将立方体的顶点和面添加到所选列表中(对于Solid或nocollision碰撞器)
                                    AddCubeMesh(x, y, false);
                                }
                            }
                            else
                            { // if not cube.如果不是立方体
                                if (CheckAllAdjacent(x, y) == false) //检查所有相邻体素块是否实体
                                { // if any adjacent cellId isn'transform opaque, we render the mesh.如果任何相邻体素不是不透明的，自定义渲染网格
                                    CreateCustomMesh(cellId, x, y, cellType.VMesh);
                                }
                            }
                        }
                        y += 1;
                    }
                    y = 0;
                    x += 1;
                }
            }
            else
            {
                while (x < SideLength)
                {
                    while (y < SideLength)
                    {
                        while (z < SideLength)
                        {
                            //获取体素数据（体素块的种类）
                            ushort voxel = chunk.GetCellID(x, y, z); // the current cellId data
                            if (voxel != 0)
                            { // don'transform render empty blocks.不渲染空块
                              //获取体素数据（体素块的种类）对应的体素（类型）
                                Cell voxelType = CPEngine.GetCellType(voxel);
                                //体素的自定义网格未启用时
                                if (voxelType.VCustomMesh == false)
                                { // if cube.如果是立方体

                                    //Transparency transparency = CPEngine.GetCellType (chunk.GetCellID(pixelX,pixelY,z)).VTransparency;
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
                                    if (colliderType == ColliderType.none && CPEngine.GenerateColliders)
                                    {
                                        //将立方体的顶点和面添加到所选列表中(对于Solid或nocollision碰撞器)
                                        AddCubeMesh(x, y, z, false);
                                    }
                                }
                                else
                                { // if not cube.如果不是立方体
                                    if (CheckAllAdjacent(x, y, z) == false) //检查所有相邻体素块是否实体
                                    { // if any adjacent cellId isn'transform opaque, we render the mesh.如果任何相邻体素不是不透明的，自定义渲染网格
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
            if (CPEngine.HorizontalMode)
            {
                return CheckAdjacent(x, y, direction, transparency);
            }
            else
            {
                CPIndex index = chunk.GetAdjacentIndex(x, y, z, direction);
                ushort adjacentVoxel = chunk.GetCellID(index.x, index.y, index.z);
                if (adjacentVoxel == ushort.MaxValue)
                { // if the neighbor chunk is missing.如果相邻团块是缺失状态（CellID返回了65535）
                  //如果设置需要显示边界面或者这个面的朝向是向上的
                    if (CPEngine.ShowBorderFaces || direction == Direction.up)
                    {
                        //面是需要创建的，返回真
                        //Debug.Log("0");
                        return true;
                    }
                    else
                    {
                        //Debug.Log("1");
                        return false;
                    }
                }
                //如果邻团存在
                Transparency result = CPEngine.GetCellType(adjacentVoxel).VTransparency; // get the transparency of the adjacent cellId.获取相邻体素的透明度
                // parse the result (taking into account the transparency of the adjacent block as well as the one doing this check)
                //解析结果(考虑相邻体素块及执行此检查的体素块的透明度)
                if (transparency == Transparency.transparent)
                {//执行此检查的体素块完全透明
                    if (result == Transparency.transparent)
                    {//如果邻块是完全透明
                        // don'transform draw a transparent block next to another transparent block.禁止在一个完全透明体素块旁边绘制另一个透明块
                        //Debug.Log("2");
                        return false;
                    }
                    else
                    {//如果邻块是非完全透明（半透明或固体）
                        //Debug.Log("3");
                        return true; // draw a transparent block next to a solid or semi-transparent.允许在实体或半透明旁边画一个透明的块
                    }

                }
                else
                {//执行此检查的体素块非完全透明（半透明或固体）
                    if (result == Transparency.solid)
                    {//如果邻块是固体
                        //Debug.Log("4");
                        return false; // don'transform draw a solid block or a semi-transparent block next to a solid block.禁止在实体体素块旁边画实体或半透明体素块
                    }
                    else
                    {//如果邻块非固体（透明或半透明），允许绘制执行此检查的体素块的检查面
                        //Debug.Log("5");
                        return true; // draw a solid block or a semi-transparent block next to both transparent and semi-transparent.允许在透明和半透明体素块旁绘制一个实心或半透明体素块
                    }
                }
            }
        }
        /// <summary>
        /// 检查相邻体素然后决定这个面是否需要被创建
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="transparency"></param>
        /// <returns>如果一个面应该被创建时返回true</returns>
        private bool CheckAdjacent(int x, int y, Direction direction, Transparency transparency)
        { // returns true if a face should be spawned
            CPIndex index = chunk.GetAdjacentIndex(x, y, direction);
            //adjacentVoxel == 1表示邻块类型是土，2是草，它们都是固体的,0是半透明的默认体素，预制体中暂没有完全透明的（除非手动修改或团块不存在时是这种状态）
            ushort adjacentVoxel = chunk.GetCellID(index.x, index.y);
            if (adjacentVoxel == ushort.MaxValue)
            { // if the neighbor chunk is missing.如果相邻团块是缺失状态（CellID返回了65535）
              //如果设置需要显示边界面或者这个面的朝向是向上的
                if (CPEngine.ShowBorderFaces || direction == Direction.up)
                {
                    //面是需要创建的，返回真
                    //Debug.Log("0");
                    return true;
                }
                else
                {
                    //Debug.Log("1");
                    return false;
                }
            }
            //如果邻团存在
            Transparency result = CPEngine.GetCellType(adjacentVoxel).VTransparency; // get the transparency of the adjacent cellId.获取相邻体素的透明度
            // parse the result (taking into account the transparency of the adjacent block as well as the one doing this check)
            //解析结果(考虑相邻体素块及执行此检查的体素块的透明度)
            if (transparency == Transparency.transparent)
            {//执行此检查的体素块完全透明
                if (result == Transparency.transparent)
                {//如果邻块是完全透明
                    //Debug.Log("2");
                    return false; // don'transform draw a transparent block next to another transparent block.禁止在一个完全透明体素块旁边绘制另一个透明块}
                }
                else
                {//如果邻块是非完全透明（半透明或固体）
                    //Debug.Log("3");
                    return true; // draw a transparent block next to a solid or semi-transparent.允许在实体或半透明旁边画一个透明的块}
                }
            }
            else
            {//执行此检查的体素块非完全透明（半透明或固体）
                if (result == Transparency.solid)
                {//如果邻块是固体
                    //Debug.Log("4"+ direction.ToString() + adjacentVoxel.ToString()+ "Index:" + index.ToString());
                    //此处可以设计该面若是视野内边界则返回真，让看到的边界封闭
                    return false; // don'transform draw a solid block or a semi-transparent block next to a solid block.禁止在实体体素块旁边画实体或半透明体素块}
                }
                else
                {//如果邻块非固体（透明或半透明），允许绘制执行此检查的体素块的检查面
                    //Debug.Log("5" + direction.ToString() + adjacentVoxel.ToString() +"Index:"+ index.ToString());
                    return true; // draw a solid block or a semi-transparent block next to both transparent and semi-transparent.允许在透明和半透明体素块旁绘制一个实心或半透明体素块
                }
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
            if (CPEngine.HorizontalMode)
            {
                return CheckAllAdjacent(x, y);
            }
            else
            {
                for (int direction = 0; direction < 6; direction++)
                {
                    if (CPEngine.GetCellType(chunk.GetCellID(chunk.GetAdjacentIndex(x, y, z, (Direction)direction))).VTransparency != Transparency.solid)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// 检查所有相邻体素块是否实体
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>如果所有相邻体素块都是实体，则返回true</returns>
        public bool CheckAllAdjacent(int x, int y)
        { // returns true if all adjacent voxels are solid
            for (int direction = 0; direction < 4; direction++)
            {
                if (CPEngine.GetCellType(chunk.GetCellID(chunk.GetAdjacentIndex(x, y, (Direction)direction))).VTransparency != Transparency.solid)
                {
                    return false;
                }
            }
            return true;
        }

        // ==== mesh generation =======================================================================================

        /// <summary>
        /// 创建渲染器和碰撞器的网格所需顶点
        /// </summary>
        /// <param name="facing"></param>
        /// <param name="colliderType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z">不填则默认为0</param>
        private void CreateFaceMesh(Facing facing, ColliderType colliderType, int x, int y, int z = 0)
        {
            // ==== Vertices ====
            // add the positions of the vertices depending on the facing of the face
            //↓往正值方向进行偏移回插入点（否则默认插入在原点负值方向偏移1个网格半径的位置）
            float xV = x + meshOffset; float yV = y + meshOffset; float zV = z + meshOffset;
            if (facing == Facing.forward)
            {
                //创建网格渲染器材质mesh.Vertices
                //当两个面非常接近时，在渲染时可能会出现Z-fighting即两个面因为深度值非常接近而在屏幕上闪烁，这里通过微小的额外偏移来防止
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV + antiFlickerRadius, zV + meshRadius));
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV + antiFlickerRadius, zV + meshRadius));
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV - antiFlickerRadius, zV + meshRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV - antiFlickerRadius, zV + meshRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    //创建网格碰撞器材质mesh.Vertices
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV + meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV + meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV - meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV - meshRadius, zV + meshRadius));
                }
            }
            else if (facing == Facing.up)
            {
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV + meshRadius, zV + antiFlickerRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV + meshRadius, zV + antiFlickerRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV + meshRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV + meshRadius, zV - antiFlickerRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV + meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV + meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV + meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV + meshRadius, zV - meshRadius));
                }
            }
            else if (facing == Facing.right)
            {
                Vertices.Add(new Vector3(xV + meshRadius, yV + antiFlickerRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV + meshRadius, yV + antiFlickerRadius, zV + antiFlickerRadius));
                Vertices.Add(new Vector3(xV + meshRadius, yV - antiFlickerRadius, zV + antiFlickerRadius));
                Vertices.Add(new Vector3(xV + meshRadius, yV - antiFlickerRadius, zV - antiFlickerRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV + meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV + meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV - meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV - meshRadius, zV - meshRadius));
                }
            }
            else if (facing == Facing.back)
            {
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV + antiFlickerRadius, zV - meshRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV + antiFlickerRadius, zV - meshRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV - antiFlickerRadius, zV - meshRadius));
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV - antiFlickerRadius, zV - meshRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV + meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV + meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV - meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV - meshRadius, zV - meshRadius));
                }
            }
            else if (facing == Facing.down)
            {
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV - meshRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV - meshRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV - meshRadius, zV + antiFlickerRadius));
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV - meshRadius, zV + antiFlickerRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV - meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV - meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV + meshRadius, yV - meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV - meshRadius, zV + meshRadius));
                }
            }
            else if (facing == Facing.left)
            {
                //创建网格渲染器材质mesh.Vertices
                //当两个面非常接近时，在渲染时可能会出现Z-fighting即两个面因为深度值非常接近而在屏幕上闪烁，这里通过微小的额外偏移来防止
                Vertices.Add(new Vector3(xV - meshRadius, yV + antiFlickerRadius, zV + antiFlickerRadius));
                Vertices.Add(new Vector3(xV - meshRadius, yV + antiFlickerRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV - meshRadius, yV - antiFlickerRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV - meshRadius, yV - antiFlickerRadius, zV + antiFlickerRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    //创建网格碰撞器材质mesh.Vertices
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV + meshRadius, zV + meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV + meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV - meshRadius, zV - meshRadius));
                    SolidColliderVertices.Add(new Vector3(xV - meshRadius, yV - meshRadius, zV + meshRadius));
                }
            }
        }
        private void CreateFace(ushort cellId, Facing facing, ColliderType colliderType, int x, int y, int z)
        {
            if (CPEngine.HorizontalMode)
            {
                CreateFace(cellId, facing, colliderType, x, y);
            }
            else
            {
                Cell cellComponent = CPEngine.GetCellType(cellId);
                List<int> FacesList = Faces[cellComponent.VSubmeshIndex];
                // ==== Vertices ====
                // add the positions of the vertices depending on the facing of the face
                CreateFaceMesh(facing, colliderType, x, y, z);
                // ==== UVs =====
                ushort id = CPEngine.GetSubMeshIndex(cellId);
                float tUnitX = 1f / CPEngine.TextureUnitX[id]; float tUnitY = 1f / CPEngine.TextureUnitY[id];
                Vector2 tOffset = CPEngine.GetTextureOffset(cellId, facing); //获取此体素类型的纹理偏移点
                float padX = tUnitX * CPEngine.TexturePadX; float padY = tUnitY * CPEngine.TexturePadY;
                //用大量二维点给大纹理图分区并添加到UV数组
                UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + tUnitY - padY)); //左上（原点）
                UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + tUnitY - padY)); //右上
                UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + padY)); //右下
                UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + padY)); //左下
                //四个二维点划分了1格纹理区域（贴到下方的一个面）

                // ==== Faces ====
                //添加面（2个三角形，0-3是面的四个顶点序号，对应一开始添加的顶点数组的索引值）
                FacesList.Add(FaceCount + 0);
                FacesList.Add(FaceCount + 1);
                FacesList.Add(FaceCount + 3);
                FacesList.Add(FaceCount + 1);
                FacesList.Add(FaceCount + 2);
                FacesList.Add(FaceCount + 3);
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    //添加碰撞面
                    SolidColliderFaces.Add(SolidFaceCount + 0);
                    SolidColliderFaces.Add(SolidFaceCount + 1);
                    SolidColliderFaces.Add(SolidFaceCount + 3);
                    SolidColliderFaces.Add(SolidFaceCount + 1);
                    SolidColliderFaces.Add(SolidFaceCount + 2);
                    SolidColliderFaces.Add(SolidFaceCount + 3);
                }
                //添加面的计数
                FaceCount += 4; // we're adding 4 because there are 4 vertices in each face.我们设计了每四个顶点形成一个面，所以每次计数增4
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    SolidFaceCount += 4;
                }
                // Check the amount of vertices so far and create a new mesh if necessary.检查顶点数量是否满足创建1个新网格
                //需注意减小mesh的subMeshCount值会调整Mesh索引缓冲区的大小，新的索引缓冲区大小将设置为第一个被移除子网格的SubMeshDescriptor.indexStart
                //默认情况下，Mesh的索引缓冲区为16位，支持最多65535个顶点，若需要支持更多顶点，可以设置为32位，但需注意并非所有平台都支持32位索引
                //在修改Mesh的子网格数量或索引格式时，应充分考虑其对性能和兼容性的影响
                if (Vertices.Count > 65530)
                {
                    CreateNewMeshObject();
                }
            }
        }
        private void CreateFace(ushort cellId, Facing facing, ColliderType colliderType, int x, int y)
        {
            Cell cellComponent = CPEngine.GetCellType(cellId);
            List<int> FacesList = Faces[cellComponent.VSubmeshIndex];
            // ==== Vertices ====
            // add the positions of the vertices depending on the facing of the face
            CreateFaceMesh(facing, colliderType, x, y);
            // ==== UVs =====
            ushort id = CPEngine.GetSubMeshIndex(cellId);
            float tUnitX = 1f / CPEngine.TextureUnitX[id]; float tUnitY = 1f / CPEngine.TextureUnitY[id];
            Vector2 tOffset = CPEngine.GetTextureOffset(cellId, facing); //获取此体素类型的纹理偏移点
            float padX = tUnitX * CPEngine.TexturePadX; float padY = tUnitY * CPEngine.TexturePadY;
            //用大量二维点给大纹理图分区并添加到UV数组
            UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + tUnitY - padY)); //左上（原点）
            UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + tUnitY - padY)); //右上
            UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + padY)); //右下
            UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + padY)); //左下
                                                                                        //四个二维点划分了1格纹理区域（贴到下方的一个面）

            // ==== Faces ====
            //添加面（2个三角形，0-3是面的四个顶点序号，对应一开始添加的顶点数组的索引值）
            FacesList.Add(FaceCount + 0);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 3);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 2);
            FacesList.Add(FaceCount + 3);
            if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
            {
                //添加碰撞面
                SolidColliderFaces.Add(SolidFaceCount + 0);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 3);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 2);
                SolidColliderFaces.Add(SolidFaceCount + 3);
            }
            //添加面的计数
            FaceCount += 4; // we're adding 4 because there are 4 vertices in each face.我们设计了每四个顶点形成一个面，所以每次计数增4
            if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
            {
                SolidFaceCount += 4;
            }
            // Check the amount of vertices so far and create a new mesh if necessary.检查顶点数量是否满足创建1个新网格
            //需注意减小mesh的subMeshCount值会调整Mesh索引缓冲区的大小，新的索引缓冲区大小将设置为第一个被移除子网格的SubMeshDescriptor.indexStart
            //默认情况下，Mesh的索引缓冲区为16位，支持最多65535个顶点，若需要支持更多顶点，可以设置为32位，但需注意并非所有平台都支持32位索引
            //在修改Mesh的子网格数量或索引格式时，应充分考虑其对性能和兼容性的影响
            if (Vertices.Count > 65530)
            {
                CreateNewMeshObject();
            }
        }

        /// <summary>
        /// 创建自定义网格
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="mesh">2D模式下请填入XY平面的mesh</param>
        private void CreateCustomMesh(ushort cellId, int x, int y, int z, Mesh mesh)
        {
            if (CPEngine.HorizontalMode)
            {
                CreateCustomMesh(cellId, x, y, mesh);
            }
            else
            {
                //获取Cell组件
                Cell cellComponent = CPEngine.GetCellType(cellId);
                //存储网格面（多个顶点组）的数组，VSubmeshIndex=MaterialIndex（没额外添加的话就一个材质，默认索引值0）
                List<int> FacesList = Faces[cellComponent.VSubmeshIndex];
                // check if mesh exists
                if (mesh == null)
                {
                    Debug.LogError("CellSpace: The cellId id " + cellId + " uses a custom mesh, but no mesh has been assigned!");
                    return;
                }
                // === mesh
                // check if we still have room for more vertices in the mesh.检查顶点数量是否满足创建1个新网格
                //需注意减小mesh的subMeshCount值会调整Mesh索引缓冲区的大小，新的索引缓冲区大小将设置为第一个被移除子网格的SubMeshDescriptor.indexStart
                //默认情况下，Mesh的索引缓冲区为16位，支持最多65535个顶点，若需要支持更多顶点，可以设置为32位，但需注意并非所有平台都支持32位索引
                //在修改Mesh的子网格数量或索引格式时，应充分考虑其对性能和兼容性的影响
                if (Vertices.Count + mesh.vertices.Length > 65534)
                {
                    CreateNewMeshObject();
                }
                // rotate vertices depending on the mesh rotation setting
                List<Vector3> rotatedVertices = new List<Vector3>();
                //↓获取立方体Cell组件的旋转属性，它默认是在Cell_TypeNum对应的预制体里设定好的，当然程序中也可以手动干预旋转
                MeshRotation rotation = cellComponent.VRotation;
                // 180 horizontal (reverse all pixelX and z)
                if (rotation == MeshRotation.back)
                {
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        rotatedVertices.Add(new Vector3(-vertex.x, vertex.y, -vertex.z));//绕Y轴转180°使立方体后面与前面互换
                    }
                }
                // 90 right
                else if (rotation == MeshRotation.right)
                {
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        rotatedVertices.Add(new Vector3(vertex.z, vertex.y, -vertex.x));//绕Y轴往右转90°
                    }
                }
                // 90 left
                else if (rotation == MeshRotation.left)
                {
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        rotatedVertices.Add(new Vector3(-vertex.z, vertex.y, vertex.x));//绕Y轴往左转90°
                    }
                }
                // no rotation 无旋转
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
                if (CPEngine.GenerateColliders)
                {
                    ColliderType colliderType = CPEngine.GetCellType(cellId).VColliderType;  // get collider type (solid/cube/none)

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
                    else if (cellId != 0)
                    { // only make a collider for non-empty voxels
                        AddCubeMesh(x, y, z, false); // if no cube collider, add a cube to the nocollide mesh (we still need a collider on noCollide blocks for raycasts and such)
                    }
                }
            }
        }
        /// <summary>
        /// 创建自定义网格
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mesh">请填入XY平面的mesh</param>
        private void CreateCustomMesh(ushort cellId, int x, int y, Mesh mesh)
        {

            Cell cellComponent = CPEngine.GetCellType(cellId);
            List<int> FacesList = Faces[cellComponent.VSubmeshIndex];

            // check if mesh exists
            if (mesh == null)
            {
                Debug.LogError("CellSpace: The cellId id " + cellId + " uses a custom mesh, but no mesh has been assigned!");
                return;
            }


            // === mesh
            // check if we still have room for more vertices in the mesh.检查顶点数量是否满足创建1个新网格
            //需注意减小mesh的subMeshCount值会调整Mesh索引缓冲区的大小，新的索引缓冲区大小将设置为第一个被移除子网格的SubMeshDescriptor.indexStart
            //默认情况下，Mesh的索引缓冲区为16位，支持最多65535个顶点，若需要支持更多顶点，可以设置为32位，但需注意并非所有平台都支持32位索引
            //在修改Mesh的子网格数量或索引格式时，应充分考虑其对性能和兼容性的影响
            if (Vertices.Count + mesh.vertices.Length > 65534)
            {
                CreateNewMeshObject();
            }

            // rotate vertices depending on the mesh rotation setting
            List<Vector3> rotatedVertices = new List<Vector3>();
            foreach (Vector3 vertex in mesh.vertices)
            {
                //2D模式无旋转直接添加
                rotatedVertices.Add(vertex);
            }

            // vertices
            foreach (Vector3 vertex in rotatedVertices)
            {
                Vertices.Add(vertex + new Vector3(x, y, 0f)); // add all vertices from the mesh
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
            if (CPEngine.GenerateColliders)
            {
                ColliderType colliderType = CPEngine.GetCellType(cellId).VColliderType;  // get collider type (solid/cube/none)

                // mesh collider	
                if (colliderType == ColliderType.mesh)
                {
                    foreach (Vector3 vertex1 in rotatedVertices)
                    {
                        SolidColliderVertices.Add(vertex1 + new Vector3(x, y, 0f)); // if mesh collider, just add the vertices & faces from this mesh to the solid collider mesh
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
                    AddCubeMesh(x, y, 0, true); // if cube collider, add a cube to the solid mesh
                }
                // nocollide collider (for both ColliderType.mesh and ColliderType.none, but not for ColliderType.cube since it's redundant)
                else if (cellId != 0)
                { // only make a collider for non-empty voxels
                    AddCubeMesh(x, y, 0, false); // if no cube collider, add a cube to the nocollide mesh (we still need a collider on noCollide blocks for raycasts and such)
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
            if (CPEngine.HorizontalMode)
            {
                AddCubeMesh(x, y, solid);
            }
            else
            {
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
        }
        /// <summary>
        /// 将立方体的顶点和面添加到所选列表中(对于Solid或nocollision碰撞器)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="solid">是否实心</param>
        private void AddCubeMesh(int x, int y, bool solid)
        { // adds cube verts and faces to the chosen lists (for Solid or NoCollide colliders)
          //透明度是实心的情况
            if (solid)
            {
                // vertices
                foreach (Vector3 vertex in Cube.vertices)
                {
                    //遍历每个立方体网格顶点，按体素索引的位置修正（移动），然后将它们存储到实心碰撞体顶点数组中
                    SolidColliderVertices.Add(vertex + new Vector3(x, y)); // add all vertices from the mesh.从网格添加所有顶点
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
                    NoCollideVertices.Add(vertex1 + new Vector3(x, y));
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
            //附加网格碰撞器预制体CellChunkAdditionalMesh的渲染器挂载的材质数量（要与网格渲染器的材质数量保持一致）
            mesh.subMeshCount = GetComponent<Renderer>().materials.Length;

            for (int i = 0; i < Faces.Count; ++i)
            {
                //将团块预制体的几个材质的面加入到网格碰撞器预制体（碰撞面使用渲染面）
                mesh.SetTriangles(Faces[i].ToArray(), i);
            }

            mesh.uv = UVs.ToArray();//UVs.ToBuiltin(Vector2F) as Vector2F[]	
            //刷新法线数据
            mesh.RecalculateNormals();

            if (CPEngine.GenerateColliders)
            {

                // Update solid collider
                Mesh colMesh = new Mesh();

                colMesh.vertices = SolidColliderVertices.ToArray();
                colMesh.triangles = SolidColliderFaces.ToArray();

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
            //附加网格碰撞器预制体CellChunkAdditionalMesh实例化并添加到团块（多少个邻居就加入多少次）
            GameObject meshContainer = Instantiate(chunk.MeshContainer, transform.position, transform.rotation) as GameObject;
            meshContainer.transform.parent = this.transform;
            //更新该组件的网格
            UpdateMesh(meshContainer.GetComponent<MeshFilter>().mesh);
        }
    }
}
