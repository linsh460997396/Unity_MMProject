//using System.Collections.Generic;
//using UnityEngine;

//namespace Test.Example
//{
//    public class TE_DrawMesh : MonoBehaviour
//    {
//        public Transform handle; // 手柄
//        public Material mat; // 材质
//        public float lineWidth = 0.05f;
//        private bool isDraw; // 标记当前是否正在画线
//        private bool isNewLine;
//        private Vector3 lastPos; // 记录上一帧手柄的位置
//        private Vector3 lastPosLeft;
//        private Vector3 lastPosRight;
//        private Mesh mesh; // Mesh对象
//        private MeshFilter meshFilter; // MeshFilter组件
//        private MeshRenderer meshRenderer; // MeshRenderer组件
//        private List<Vector3> vertices = new List<Vector3>(); // 用于存储顶点的列表
//        private List<int> triangles = new List<int>(); // 用于存储三角形索引的列表

//        void Update()
//        {
//            if (Input.GetKeyDown(KeyCode.M) && !isDraw)
//            {
//                isDraw = true;
//                isNewLine = true;

//                vertices.Clear();
//                triangles.Clear();

//                // 初始化Mesh和组件
//                mesh = new Mesh();
//                // 创建一个新的GameObject
//                GameObject meshObject = new GameObject("MeshObject");
//                // 为GameObject添加MeshFilter和MeshRenderer组件
//                meshFilter = meshObject.AddComponent<MeshFilter>();
//                meshRenderer = meshObject.AddComponent<MeshRenderer>();
//                mat.doubleSidedGI = true;
//                meshRenderer.material = mat;

//                lastPos = handle.position; // 记录当前帧手柄的位置
//            }

//            if (isDraw)
//            {
//                // 判断两帧手柄的距离，超过一定阈值再添加线段
//                if (Vector3.Distance(lastPos, handle.position) > 0.03f)
//                {
//                    RealTimeDrawLine(handle.position);
//                    // 更新上一个位置为新的线段结束点，防止断节
//                    lastPos = handle.position;
//                }
//            }

//            if (Input.GetKeyDown(KeyCode.N) && isDraw)
//            {
//                isDraw = false;
//            }
//        }

//        public void RealTimeDrawLine(Vector3 newPoint)
//        {
//            // 计算线段的两个方向向量
//            Vector3 direction = newPoint - lastPos;
//            Vector3 normal = Vector3.Cross(Vector3.up, direction).normalized;
//            if (normal == Vector3.zero)
//            {
//                //如果垂直运动的话，换个法向量
//                GameObject mainPlayer = GameObject.Find("Player");
//                if (mainPlayer != null)
//                {
//                    normal = mainPlayer.transform.forward;
//                }
//                else { normal = Vector3.Cross(Vector3.forward, direction).normalized; }
//            }


//            // 计算线段的四个顶点
//            if (isNewLine)
//            {
//                isNewLine = false;
//                lastPosLeft = lastPos - normal * lineWidth / 2;
//                lastPosRight = lastPos + normal * lineWidth / 2;
//            }

//            Vector3 currentSideLeft = newPoint - normal * lineWidth / 2;
//            Vector3 currentSideRight = newPoint + normal * lineWidth / 2;

//            // 添加新的顶点到列表
//            vertices.Add(lastPosLeft);
//            vertices.Add(currentSideLeft);
//            vertices.Add(currentSideRight);
//            vertices.Add(lastPosRight);

//            // 添加新的三角形索引到列表，每个线段由两个三角形组成
//            int startIndex = vertices.Count - 4; // 四个顶点对应的索引头

//            triangles.Add(startIndex);
//            triangles.Add(startIndex + 1);
//            triangles.Add(startIndex + 2); // 第一个三角形

//            triangles.Add(startIndex);
//            triangles.Add(startIndex + 2);
//            triangles.Add(startIndex + 3); // 第二个三角形

//            // 更新Mesh数据
//            mesh.vertices = vertices.ToArray();
//            mesh.triangles = triangles.ToArray();

//            // 可选的UV坐标（如果不需要纹理，这些可以随意设置）
//            // mesh.uv = ...;

//            // 可选的法线计算（对于光照效果）
//            mesh.RecalculateNormals();

//            // 应用更改到Mesh
//            mesh.UploadMeshData(false); // 上传Mesh数据并标记为非动态（不经常更改）

//            meshFilter.mesh = mesh;

//            lastPosLeft = currentSideLeft;
//            lastPosRight = currentSideRight;

//        }
//    }
//}