using UnityEngine;
using System.Collections.Generic;

// Handles mesh creation and all related functions.
// ���ص�Ԫ��Cell/Voxel��

namespace CellSpace
{
    /// <summary>
    /// ������ת
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
    public class CellChunkMeshCreator : MonoBehaviour
    {
        /// <summary>
        /// ����˸����뾶
        /// </summary>
        const float antiFlickerRadius = meshRadius + antiFlickerOffset;
        /// <summary>
        /// ����뾶
        /// </summary>
        const float meshRadius = 0.5f;
        /// <summary>
        /// ����˸ƫ��������������ǳ��ӽ�ʱ������Ⱦʱ���ܻ����Z-fighting����������Ϊ���ֵ�ǳ��ӽ�������Ļ����˸������ͨ��΢С�Ķ���ƫ������ֹ
        /// </summary>
        const float antiFlickerOffset = 0.0001f;
        /// <summary>
        /// ����λ��ƫ����������ֵ�������ƫ�ƻز���㣬����Ĭ�ϵ����������ԭ�㸺ֵ����ƫ��1������뾶��λ�ã�
        /// </summary>
        const float meshOffset = meshRadius;
        /// <summary>
        /// �ſ�
        /// </summary>
        private CellChunk chunk;
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
        /// �洢�����棨��������ζ����飩�����飬List<int>��3��Ԫ�ض�Ӧһ�������Σ���2�������λ�4�������Ӧ1����
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
            chunk = GetComponent<CellChunk>();
            SideLength = chunk.SideLength;
            //Debug.Log(GetComponent<Renderer>().materials.Length);
            // make a list for each material (each material is a submesh).Ϊ�ſ��������Ⱦ�������ÿһ�ֲ�����һ���嵥(ÿһ�ֲ��ʶ���������)
            for (int i = 0; i < GetComponent<Renderer>().materials.Length; i++)
            {
                Faces.Add(new List<int>()); //���յ��嵥��ӵ��洢�����棨��������飩������
            }
            //��ʼ�����
            initialized = true;
        }

        // ==== Cell updates =====================================================================================

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
                //��������Ϸ������дݻ�
                Destroy(child.gameObject);
            }

            int x = 0, y = 0, z = 0;

            // Refresh neighbor chunks.ˢ�������ſ�
            chunk.GetNeighbors();

            // for each cellId in Voxels, check if any of the cellId's faces are exposed, and if so, add their faces to the main mesh arrays (named Vertices and Faces)

            //��˼�Ƕ���Voxels�е�ÿһ�����أ�������Ƿ����κ�һ���Ǳ�¶����ģ�����оͰ���һ����ӵ������������飨��Ϊ Vertices �� Faces���б���
            //Vertices��Faces���������飬�ֱ��ʾ��ά�ռ��еĶ�������Ͷ���ε�����Ϣ
            //Vertices ����洢�����ж�������꣬�� Faces ������洢������Щ���㹹�ɵĶ���ε�����Ϣ
            //���һ�����ص�ĳ���汩¶���⣬��ô�ͻ�Ѹ���Ķ���������ӵ� Vertices �����У����Ѹ������Ϣ������Щ���㹹��������棩��ӵ� Faces ������
            //ͨ�����ַ�ʽ�����Թ�����һ�������������ص���άģ�ͣ����ں�������Ⱦ����ʾ
            if (CPEngine.HorizontalMode)
            {
                while (x < SideLength)
                {
                    while (y < SideLength)
                    {
                        //��ȡ�������ݣ����ؿ�����ࣩ
                        ushort cellId = chunk.GetCellID(x, y); // the current cellId data
                        if (cellId != 0)
                        { // don'transform render empty blocks.����Ⱦ�տ�
                          //��ȡ�������ݣ����ؿ�����ࣩ��Ӧ�����أ����ͣ�
                            Cell cellType = CPEngine.GetCellType(cellId);
                            //���ص��Զ�������δ����ʱ
                            if (cellType.VCustomMesh == false)
                            { // if cube.�����������
                                //Transparency transparency = CPEngine.GetCellType (chunk.GetCellID(pixelX,pixelY,z)).VTransparency;
                                Transparency transparency = cellType.VTransparency; //��ȡ����͸����
                                ColliderType colliderType = cellType.VColliderType; //��ȡ������ײ����

                                if (CPEngine.MutiHorizontal)
                                {//��ά��湦�ܿ����������������4�����棬Ӧ��ʾ���Ե����
                                    //�����������Ȼ�����������Ƿ���Ҫ�����������ģʽ����ǰ���棩
                                    ////����Ƹ���������Ұ�ڱ߽�������ʼ��Ϊ�棨�ÿ����ı߽��գ���Ҳ�ɶ�Ҫ��ձ߽紦���ڿ���տ����·����������жϴ��������
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

                                //��ά��湦�ܿ�����������ʾǰ��
                                if (CPEngine.MutiHorizontal) CreateFace(cellId, Facing.forward, colliderType, x, y);

                                //���ģʽ��������ʾ���棨�����Ļ�������棩
                                CreateFace(cellId, Facing.back, colliderType, x, y);

                                // if no collider, create a trigger cube collider.���û����ײ�壬����һ����������ײ��
                                if (colliderType == ColliderType.none && CPEngine.GenerateColliders)
                                {
                                    //��������Ķ��������ӵ���ѡ�б���(����Solid��nocollision��ײ��)
                                    AddCubeMesh(x, y, false);
                                }
                            }
                            else
                            { // if not cube.�������������
                                if (CheckAllAdjacent(x, y) == false) //��������������ؿ��Ƿ�ʵ��
                                { // if any adjacent cellId isn'transform opaque, we render the mesh.����κ��������ز��ǲ�͸���ģ��Զ�����Ⱦ����
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
                            //��ȡ�������ݣ����ؿ�����ࣩ
                            ushort voxel = chunk.GetCellID(x, y, z); // the current cellId data
                            if (voxel != 0)
                            { // don'transform render empty blocks.����Ⱦ�տ�
                              //��ȡ�������ݣ����ؿ�����ࣩ��Ӧ�����أ����ͣ�
                                Cell voxelType = CPEngine.GetCellType(voxel);
                                //���ص��Զ�������δ����ʱ
                                if (voxelType.VCustomMesh == false)
                                { // if cube.�����������

                                    //Transparency transparency = CPEngine.GetCellType (chunk.GetCellID(pixelX,pixelY,z)).VTransparency;
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
                                    if (colliderType == ColliderType.none && CPEngine.GenerateColliders)
                                    {
                                        //��������Ķ��������ӵ���ѡ�б���(����Solid��nocollision��ײ��)
                                        AddCubeMesh(x, y, z, false);
                                    }
                                }
                                else
                                { // if not cube.�������������
                                    if (CheckAllAdjacent(x, y, z) == false) //��������������ؿ��Ƿ�ʵ��
                                    { // if any adjacent cellId isn'transform opaque, we render the mesh.����κ��������ز��ǲ�͸���ģ��Զ�����Ⱦ����
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
            if (CPEngine.HorizontalMode)
            {
                return CheckAdjacent(x, y, direction, transparency);
            }
            else
            {
                CPIndex index = chunk.GetAdjacentIndex(x, y, z, direction);
                ushort adjacentVoxel = chunk.GetCellID(index.x, index.y, index.z);
                if (adjacentVoxel == ushort.MaxValue)
                { // if the neighbor chunk is missing.��������ſ���ȱʧ״̬��CellID������65535��
                  //���������Ҫ��ʾ�߽�����������ĳ��������ϵ�
                    if (CPEngine.ShowBorderFaces || direction == Direction.up)
                    {
                        //������Ҫ�����ģ�������
                        //Debug.Log("0");
                        return true;
                    }
                    else
                    {
                        //Debug.Log("1");
                        return false;
                    }
                }
                //������Ŵ���
                Transparency result = CPEngine.GetCellType(adjacentVoxel).VTransparency; // get the transparency of the adjacent cellId.��ȡ�������ص�͸����
                // parse the result (taking into account the transparency of the adjacent block as well as the one doing this check)
                //�������(�����������ؿ鼰ִ�д˼������ؿ��͸����)
                if (transparency == Transparency.transparent)
                {//ִ�д˼������ؿ���ȫ͸��
                    if (result == Transparency.transparent)
                    {//����ڿ�����ȫ͸��
                        // don'transform draw a transparent block next to another transparent block.��ֹ��һ����ȫ͸�����ؿ��Ա߻�����һ��͸����
                        //Debug.Log("2");
                        return false;
                    }
                    else
                    {//����ڿ��Ƿ���ȫ͸������͸������壩
                        //Debug.Log("3");
                        return true; // draw a transparent block next to a solid or semi-transparent.������ʵ����͸���Ա߻�һ��͸���Ŀ�
                    }

                }
                else
                {//ִ�д˼������ؿ����ȫ͸������͸������壩
                    if (result == Transparency.solid)
                    {//����ڿ��ǹ���
                        //Debug.Log("4");
                        return false; // don'transform draw a solid block or a semi-transparent block next to a solid block.��ֹ��ʵ�����ؿ��Ա߻�ʵ����͸�����ؿ�
                    }
                    else
                    {//����ڿ�ǹ��壨͸�����͸�������������ִ�д˼������ؿ�ļ����
                        //Debug.Log("5");
                        return true; // draw a solid block or a semi-transparent block next to both transparent and semi-transparent.������͸���Ͱ�͸�����ؿ��Ի���һ��ʵ�Ļ��͸�����ؿ�
                    }
                }
            }
        }
        /// <summary>
        /// �����������Ȼ�����������Ƿ���Ҫ������
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="transparency"></param>
        /// <returns>���һ����Ӧ�ñ�����ʱ����true</returns>
        private bool CheckAdjacent(int x, int y, Direction direction, Transparency transparency)
        { // returns true if a face should be spawned
            CPIndex index = chunk.GetAdjacentIndex(x, y, direction);
            //adjacentVoxel == 1��ʾ�ڿ�����������2�ǲݣ����Ƕ��ǹ����,0�ǰ�͸����Ĭ�����أ�Ԥ��������û����ȫ͸���ģ������ֶ��޸Ļ��ſ鲻����ʱ������״̬��
            ushort adjacentVoxel = chunk.GetCellID(index.x, index.y);
            if (adjacentVoxel == ushort.MaxValue)
            { // if the neighbor chunk is missing.��������ſ���ȱʧ״̬��CellID������65535��
              //���������Ҫ��ʾ�߽�����������ĳ��������ϵ�
                if (CPEngine.ShowBorderFaces || direction == Direction.up)
                {
                    //������Ҫ�����ģ�������
                    //Debug.Log("0");
                    return true;
                }
                else
                {
                    //Debug.Log("1");
                    return false;
                }
            }
            //������Ŵ���
            Transparency result = CPEngine.GetCellType(adjacentVoxel).VTransparency; // get the transparency of the adjacent cellId.��ȡ�������ص�͸����
            // parse the result (taking into account the transparency of the adjacent block as well as the one doing this check)
            //�������(�����������ؿ鼰ִ�д˼������ؿ��͸����)
            if (transparency == Transparency.transparent)
            {//ִ�д˼������ؿ���ȫ͸��
                if (result == Transparency.transparent)
                {//����ڿ�����ȫ͸��
                    //Debug.Log("2");
                    return false; // don'transform draw a transparent block next to another transparent block.��ֹ��һ����ȫ͸�����ؿ��Ա߻�����һ��͸����}
                }
                else
                {//����ڿ��Ƿ���ȫ͸������͸������壩
                    //Debug.Log("3");
                    return true; // draw a transparent block next to a solid or semi-transparent.������ʵ����͸���Ա߻�һ��͸���Ŀ�}
                }
            }
            else
            {//ִ�д˼������ؿ����ȫ͸������͸������壩
                if (result == Transparency.solid)
                {//����ڿ��ǹ���
                    //Debug.Log("4"+ direction.ToString() + adjacentVoxel.ToString()+ "Index:" + index.ToString());
                    //�˴�������Ƹ���������Ұ�ڱ߽��򷵻��棬�ÿ����ı߽���
                    return false; // don'transform draw a solid block or a semi-transparent block next to a solid block.��ֹ��ʵ�����ؿ��Ա߻�ʵ����͸�����ؿ�}
                }
                else
                {//����ڿ�ǹ��壨͸�����͸�������������ִ�д˼������ؿ�ļ����
                    //Debug.Log("5" + direction.ToString() + adjacentVoxel.ToString() +"Index:"+ index.ToString());
                    return true; // draw a solid block or a semi-transparent block next to both transparent and semi-transparent.������͸���Ͱ�͸�����ؿ��Ի���һ��ʵ�Ļ��͸�����ؿ�
                }
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
        /// ��������������ؿ��Ƿ�ʵ��
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>��������������ؿ鶼��ʵ�壬�򷵻�true</returns>
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
        /// ������Ⱦ������ײ�����������趥��
        /// </summary>
        /// <param name="facing"></param>
        /// <param name="colliderType"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z">������Ĭ��Ϊ0</param>
        private void CreateFaceMesh(Facing facing, ColliderType colliderType, int x, int y, int z = 0)
        {
            // ==== Vertices ====
            // add the positions of the vertices depending on the facing of the face
            //������ֵ�������ƫ�ƻز���㣨����Ĭ�ϲ�����ԭ�㸺ֵ����ƫ��1������뾶��λ�ã�
            float xV = x + meshOffset; float yV = y + meshOffset; float zV = z + meshOffset;
            if (facing == Facing.forward)
            {
                //����������Ⱦ������mesh.Vertices
                //��������ǳ��ӽ�ʱ������Ⱦʱ���ܻ����Z-fighting����������Ϊ���ֵ�ǳ��ӽ�������Ļ����˸������ͨ��΢С�Ķ���ƫ������ֹ
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV + antiFlickerRadius, zV + meshRadius));
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV + antiFlickerRadius, zV + meshRadius));
                Vertices.Add(new Vector3(xV - antiFlickerRadius, yV - antiFlickerRadius, zV + meshRadius));
                Vertices.Add(new Vector3(xV + antiFlickerRadius, yV - antiFlickerRadius, zV + meshRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    //����������ײ������mesh.Vertices
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
                //����������Ⱦ������mesh.Vertices
                //��������ǳ��ӽ�ʱ������Ⱦʱ���ܻ����Z-fighting����������Ϊ���ֵ�ǳ��ӽ�������Ļ����˸������ͨ��΢С�Ķ���ƫ������ֹ
                Vertices.Add(new Vector3(xV - meshRadius, yV + antiFlickerRadius, zV + antiFlickerRadius));
                Vertices.Add(new Vector3(xV - meshRadius, yV + antiFlickerRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV - meshRadius, yV - antiFlickerRadius, zV - antiFlickerRadius));
                Vertices.Add(new Vector3(xV - meshRadius, yV - antiFlickerRadius, zV + antiFlickerRadius));
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    //����������ײ������mesh.Vertices
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
                Vector2 tOffset = CPEngine.GetTextureOffset(cellId, facing); //��ȡ���������͵�����ƫ�Ƶ�
                float padX = tUnitX * CPEngine.TexturePadX; float padY = tUnitY * CPEngine.TexturePadY;
                //�ô�����ά���������ͼ��������ӵ�UV����
                UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + tUnitY - padY)); //���ϣ�ԭ�㣩
                UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + tUnitY - padY)); //����
                UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + padY)); //����
                UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + padY)); //����
                //�ĸ���ά�㻮����1���������������·���һ���棩

                // ==== Faces ====
                //����棨2�������Σ�0-3������ĸ�������ţ���Ӧһ��ʼ��ӵĶ������������ֵ��
                FacesList.Add(FaceCount + 0);
                FacesList.Add(FaceCount + 1);
                FacesList.Add(FaceCount + 3);
                FacesList.Add(FaceCount + 1);
                FacesList.Add(FaceCount + 2);
                FacesList.Add(FaceCount + 3);
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    //�����ײ��
                    SolidColliderFaces.Add(SolidFaceCount + 0);
                    SolidColliderFaces.Add(SolidFaceCount + 1);
                    SolidColliderFaces.Add(SolidFaceCount + 3);
                    SolidColliderFaces.Add(SolidFaceCount + 1);
                    SolidColliderFaces.Add(SolidFaceCount + 2);
                    SolidColliderFaces.Add(SolidFaceCount + 3);
                }
                //�����ļ���
                FaceCount += 4; // we're adding 4 because there are 4 vertices in each face.���������ÿ�ĸ������γ�һ���棬����ÿ�μ�����4
                if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
                {
                    SolidFaceCount += 4;
                }
                // Check the amount of vertices so far and create a new mesh if necessary.��鶥�������Ƿ����㴴��1��������
                //��ע���Сmesh��subMeshCountֵ�����Mesh�����������Ĵ�С���µ�������������С������Ϊ��һ�����Ƴ��������SubMeshDescriptor.indexStart
                //Ĭ������£�Mesh������������Ϊ16λ��֧�����65535�����㣬����Ҫ֧�ָ��ඥ�㣬��������Ϊ32λ������ע�Ⲣ������ƽ̨��֧��32λ����
                //���޸�Mesh��������������������ʽʱ��Ӧ��ֿ���������ܺͼ����Ե�Ӱ��
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
            Vector2 tOffset = CPEngine.GetTextureOffset(cellId, facing); //��ȡ���������͵�����ƫ�Ƶ�
            float padX = tUnitX * CPEngine.TexturePadX; float padY = tUnitY * CPEngine.TexturePadY;
            //�ô�����ά���������ͼ��������ӵ�UV����
            UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + tUnitY - padY)); //���ϣ�ԭ�㣩
            UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + tUnitY - padY)); //����
            UVs.Add(new Vector2(tUnitX * tOffset.x + tUnitX - padX, tUnitY * tOffset.y + padY)); //����
            UVs.Add(new Vector2(tUnitX * tOffset.x + padX, tUnitY * tOffset.y + padY)); //����
                                                                                        //�ĸ���ά�㻮����1���������������·���һ���棩

            // ==== Faces ====
            //����棨2�������Σ�0-3������ĸ�������ţ���Ӧһ��ʼ��ӵĶ������������ֵ��
            FacesList.Add(FaceCount + 0);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 3);
            FacesList.Add(FaceCount + 1);
            FacesList.Add(FaceCount + 2);
            FacesList.Add(FaceCount + 3);
            if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
            {
                //�����ײ��
                SolidColliderFaces.Add(SolidFaceCount + 0);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 3);
                SolidColliderFaces.Add(SolidFaceCount + 1);
                SolidColliderFaces.Add(SolidFaceCount + 2);
                SolidColliderFaces.Add(SolidFaceCount + 3);
            }
            //�����ļ���
            FaceCount += 4; // we're adding 4 because there are 4 vertices in each face.���������ÿ�ĸ������γ�һ���棬����ÿ�μ�����4
            if (colliderType == ColliderType.cube && CPEngine.GenerateColliders)
            {
                SolidFaceCount += 4;
            }
            // Check the amount of vertices so far and create a new mesh if necessary.��鶥�������Ƿ����㴴��1��������
            //��ע���Сmesh��subMeshCountֵ�����Mesh�����������Ĵ�С���µ�������������С������Ϊ��һ�����Ƴ��������SubMeshDescriptor.indexStart
            //Ĭ������£�Mesh������������Ϊ16λ��֧�����65535�����㣬����Ҫ֧�ָ��ඥ�㣬��������Ϊ32λ������ע�Ⲣ������ƽ̨��֧��32λ����
            //���޸�Mesh��������������������ʽʱ��Ӧ��ֿ���������ܺͼ����Ե�Ӱ��
            if (Vertices.Count > 65530)
            {
                CreateNewMeshObject();
            }
        }

        /// <summary>
        /// �����Զ�������
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="mesh">2Dģʽ��������XYƽ���mesh</param>
        private void CreateCustomMesh(ushort cellId, int x, int y, int z, Mesh mesh)
        {
            if (CPEngine.HorizontalMode)
            {
                CreateCustomMesh(cellId, x, y, mesh);
            }
            else
            {
                //��ȡCell���
                Cell cellComponent = CPEngine.GetCellType(cellId);
                //�洢�����棨��������飩�����飬VSubmeshIndex=MaterialIndex��û������ӵĻ���һ�����ʣ�Ĭ������ֵ0��
                List<int> FacesList = Faces[cellComponent.VSubmeshIndex];
                // check if mesh exists
                if (mesh == null)
                {
                    Debug.LogError("CellSpace: The cellId id " + cellId + " uses a custom mesh, but no mesh has been assigned!");
                    return;
                }
                // === mesh
                // check if we still have room for more vertices in the mesh.��鶥�������Ƿ����㴴��1��������
                //��ע���Сmesh��subMeshCountֵ�����Mesh�����������Ĵ�С���µ�������������С������Ϊ��һ�����Ƴ��������SubMeshDescriptor.indexStart
                //Ĭ������£�Mesh������������Ϊ16λ��֧�����65535�����㣬����Ҫ֧�ָ��ඥ�㣬��������Ϊ32λ������ע�Ⲣ������ƽ̨��֧��32λ����
                //���޸�Mesh��������������������ʽʱ��Ӧ��ֿ���������ܺͼ����Ե�Ӱ��
                if (Vertices.Count + mesh.vertices.Length > 65534)
                {
                    CreateNewMeshObject();
                }
                // rotate vertices depending on the mesh rotation setting
                List<Vector3> rotatedVertices = new List<Vector3>();
                //����ȡ������Cell�������ת���ԣ���Ĭ������Cell_TypeNum��Ӧ��Ԥ�������趨�õģ���Ȼ������Ҳ�����ֶ���Ԥ��ת
                MeshRotation rotation = cellComponent.VRotation;
                // 180 horizontal (reverse all pixelX and z)
                if (rotation == MeshRotation.back)
                {
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        rotatedVertices.Add(new Vector3(-vertex.x, vertex.y, -vertex.z));//��Y��ת180��ʹ�����������ǰ�滥��
                    }
                }
                // 90 right
                else if (rotation == MeshRotation.right)
                {
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        rotatedVertices.Add(new Vector3(vertex.z, vertex.y, -vertex.x));//��Y������ת90��
                    }
                }
                // 90 left
                else if (rotation == MeshRotation.left)
                {
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        rotatedVertices.Add(new Vector3(-vertex.z, vertex.y, vertex.x));//��Y������ת90��
                    }
                }
                // no rotation ����ת
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
        /// �����Զ�������
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="mesh">������XYƽ���mesh</param>
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
            // check if we still have room for more vertices in the mesh.��鶥�������Ƿ����㴴��1��������
            //��ע���Сmesh��subMeshCountֵ�����Mesh�����������Ĵ�С���µ�������������С������Ϊ��һ�����Ƴ��������SubMeshDescriptor.indexStart
            //Ĭ������£�Mesh������������Ϊ16λ��֧�����65535�����㣬����Ҫ֧�ָ��ඥ�㣬��������Ϊ32λ������ע�Ⲣ������ƽ̨��֧��32λ����
            //���޸�Mesh��������������������ʽʱ��Ӧ��ֿ���������ܺͼ����Ե�Ӱ��
            if (Vertices.Count + mesh.vertices.Length > 65534)
            {
                CreateNewMeshObject();
            }

            // rotate vertices depending on the mesh rotation setting
            List<Vector3> rotatedVertices = new List<Vector3>();
            foreach (Vector3 vertex in mesh.vertices)
            {
                //2Dģʽ����תֱ�����
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
        /// ��������Ķ��������ӵ���ѡ�б���(����Solid��nocollision��ײ��)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="solid">�Ƿ�ʵ��</param>
        private void AddCubeMesh(int x, int y, int z, bool solid)
        { // adds cube verts and faces to the chosen lists (for Solid or NoCollide colliders)
            if (CPEngine.HorizontalMode)
            {
                AddCubeMesh(x, y, solid);
            }
            else
            {
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
        }
        /// <summary>
        /// ��������Ķ��������ӵ���ѡ�б���(����Solid��nocollision��ײ��)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="solid">�Ƿ�ʵ��</param>
        private void AddCubeMesh(int x, int y, bool solid)
        { // adds cube verts and faces to the chosen lists (for Solid or NoCollide colliders)
          //͸������ʵ�ĵ����
            if (solid)
            {
                // vertices
                foreach (Vector3 vertex in Cube.vertices)
                {
                    //����ÿ�����������񶥵㣬������������λ���������ƶ�����Ȼ�����Ǵ洢��ʵ����ײ�嶥��������
                    SolidColliderVertices.Add(vertex + new Vector3(x, y)); // add all vertices from the mesh.������������ж���
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
            //����������ײ��Ԥ����CellChunkAdditionalMesh����Ⱦ�����صĲ���������Ҫ��������Ⱦ���Ĳ�����������һ�£�
            mesh.subMeshCount = GetComponent<Renderer>().materials.Length;

            for (int i = 0; i < Faces.Count; ++i)
            {
                //���ſ�Ԥ����ļ������ʵ�����뵽������ײ��Ԥ���壨��ײ��ʹ����Ⱦ�棩
                mesh.SetTriangles(Faces[i].ToArray(), i);
            }

            mesh.uv = UVs.ToArray();//UVs.ToBuiltin(Vector2F) as Vector2F[]	
            //ˢ�·�������
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
            //����������ײ��Ԥ����CellChunkAdditionalMeshʵ��������ӵ��ſ飨���ٸ��ھӾͼ�����ٴΣ�
            GameObject meshContainer = Instantiate(chunk.MeshContainer, transform.position, transform.rotation) as GameObject;
            meshContainer.transform.parent = this.transform;
            //���¸����������
            UpdateMesh(meshContainer.GetComponent<MeshFilter>().mesh);
        }
    }
}
