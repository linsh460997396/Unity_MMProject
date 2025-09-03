using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParticleWorld
{
    public class Main_ParticleWorld : MonoBehaviour
    {
        GameObject mainPlayer;
        GameObject mainCamera;
        public float speedA0 = 1.0f; // A0移动速度
        public GameObject prefabA0; // 预制体A0
        public float speedA1 = 1.0f; // A1最大速度
        public float minRadius = 1.0f; // 最小半径
        public float maxRadius = 2.0f; // 最大半径
        public float spawnInterval = 0.5f; // 生成间隔
        public int maxCount = 20; // 最大数量
        public int currentCount; // 当前数量
        private float spawnTimer; // 计时器

        private void Awake()
        {
            GameObject tempPrefabA0 = CreatePrefab("prefabA0", 4, 0.5f, "Custom/CShader"); // 内存诞生1个预制体A0对象实例,但Unity会自动把它加载到场景(另存为预制体文件后可进行删除)
            prefabA0 = tempPrefabA0;
            HideObject(tempPrefabA0); //隐藏这个物体
        }

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = GameObject.Find("Main Camera");

            // 初始化计数器和计时器
            currentCount = 0;
            spawnTimer = 0f;

            // 检查预制体是否加载成功
            if (prefabA0 == null)
            {
                Debug.LogError("Prefab A0 not found!");
            }
            else
            {
                // 创建1个预制体A0的实例个体,作为mainPlayer被玩家操作
                mainPlayer = Instantiate(prefabA0, transform.position, Quaternion.identity);
                ShowObject(mainPlayer);
                // 实例名字为默认,这里指定新名字
                mainPlayer.name = "A0";
                // 尺寸归一化
                mainPlayer.transform.localScale = Vector3.one;
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (mainPlayer != null)
            {
                // 更新生成计时器
                spawnTimer += Time.deltaTime;

                //Debug.Log(spawnTimer.ToString()); 

                // 检查是否达到生成A1的时间
                if (spawnTimer >= spawnInterval && currentCount < maxCount)
                {
                    // 生成A1
                    SpawnA1(prefabA0);
                    // 重置计时器
                    spawnTimer = 0f;
                }

                // 更新现有A1的位置
                UpdateA1Positions();

                // 检查WASD按键并移动Player
                if (Input.GetKey(KeyCode.W))
                {
                    mainPlayer.transform.Translate(Vector3.up * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(mainPlayer.transform.position.x, mainPlayer.transform.position.y, mainCamera.transform.position.z);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    mainPlayer.transform.Translate(Vector3.down * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(mainPlayer.transform.position.x, mainPlayer.transform.position.y, mainCamera.transform.position.z);
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    mainPlayer.transform.Translate(Vector3.left * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(mainPlayer.transform.position.x, mainPlayer.transform.position.y, mainCamera.transform.position.z);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    mainPlayer.transform.Translate(Vector3.right * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(mainPlayer.transform.position.x, mainPlayer.transform.position.y, mainCamera.transform.position.z);
                }

            }
        }

        /// <summary>
        /// 用预制体中创建A1
        /// </summary>
        void SpawnA1(GameObject prefab)
        {
            // 在Player的位置生成A1
            GameObject a1 = Instantiate(prefab, mainPlayer.transform.position, Quaternion.identity);
            ShowObject(a1);
            // 为新生成的A1设置唯一名称
            a1.name = "A1_" + currentCount;
            // 尺寸修改
            a1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            // 增加计数器
            currentCount++;

            //添加meshCollider(3D碰撞器)的碰撞网格
            a1.AddComponent<MeshCollider>().sharedMesh = a1.GetComponent<MeshFilter>().mesh; //可换成BoxCollider等预制碰撞体组件进行测试
            a1.GetComponent<MeshCollider>().convex = true; //告诉它现在是凸面(有时候Unity判断不准)
        }

        /// <summary>
        /// 获取指定点周围X-Y平面上1.0到3.0范围内的一个随机向量坐标点.
        /// </summary>
        /// <param name="center">中心点.</param>
        /// <returns>周围的随机向量坐标点.</returns>
        public static Vector3 GetRandomVectorAroundPoint(Vector3 point, float min, float max)
        {
            // 随机生成一个角度(0到2π之间)
            float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

            // 随机生成一个距离(min到max之间)
            float distance = UnityEngine.Random.Range(min, max);

            // 使用极坐标转换为笛卡尔坐标
            Vector3 randomVector = new Vector3(
                point.x + distance * Mathf.Cos(angle),
                point.y + distance * Mathf.Sin(angle),
                point.z
            );

            return randomVector;
        }

        /// <summary>
        /// 更新所有A1位置
        /// </summary>
        void UpdateA1Positions()
        {
            // 更新所有A1的位置
            for (int i = 0; i < currentCount; i++)
            {
                GameObject a1 = GameObject.Find("A1_" + i);
                if (a1 != null)
                {
                    // 计算相对位置偏移量
                    Vector3 localOffset = mainPlayer.transform.position - a1.transform.position;

                    // 若mainPlayer移动,A1会持续找过去
                    // 但是要限制A1的移动范围
                    Vector3 newA1Position = a1.transform.position + localOffset * speedA1 * Time.deltaTime;

                    if (mainPlayer != null)
                    {
                        // 计算A1和A0之间的距离
                        float distanceToA0 = (newA1Position - mainPlayer.transform.position).magnitude;

                        // 检查A1是否超出了允许的轨道范围
                        if (distanceToA0 > maxRadius)
                        {
                            // 若超出了,将A1的位置限制在轨道范围内
                            newA1Position = mainPlayer.transform.position + (newA1Position - mainPlayer.transform.position).normalized * maxRadius;
                        }
                        else if (distanceToA0 == 0f)
                        {
                            // 若在A0原点
                            newA1Position = GetRandomVectorAroundPoint(mainPlayer.transform.position, minRadius, maxRadius);
                        }
                        else if (distanceToA0 < minRadius)
                        {
                            // 若进入了A0内部,将A1的位置限制在A0的边缘
                            newA1Position = mainPlayer.transform.position + (newA1Position - mainPlayer.transform.position).normalized * minRadius;
                        }
                    }
                    else
                    {
                        Debug.LogError("A0 not found in the project.");
                    }

                    // 更新A1的位置
                    //a1.transform.position = newA1Position;
                    a1.transform.Translate((newA1Position - a1.transform.position) * speedA1 * (localOffset.magnitude + 0.5f) * Time.deltaTime);
                }
                else
                {
                    Debug.LogError("A1_" + i + " not found in the project.");
                }
            }
        }

        /// <summary>
        /// 绘制圆形网格,仅背面渲染(从Z低处往上看得到)
        /// </summary>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        Mesh CreateCircleMeshBack(int segments, float radius)
        {
            Mesh mesh = new Mesh();

            // 为这个数组声明segments+1个元素
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            }

            // 生成反面三角形索引
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = (i + 1) % segments + 1;
                triangles[i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }
        /// <summary>
        /// 翘角网格
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Mesh CreateCircleMeshBack(int segments, float radius, float height)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero; // 中心点保持不变

            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                // 非中心点的顶点在Z轴上添加一个凸起
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, height);
            }

            // 生成三角形索引,与原始平面网格相同
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = (i + 1) % segments + 1;
                triangles[i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); // 重新计算法线以确保光照正确

            return mesh;
        }

        /// <summary>
        /// 绘制圆形网格,仅正面渲染(从Z高处往下看得到)
        /// </summary>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        Mesh CreateCircleMeshFront(int segments, float radius)
        {
            Mesh mesh = new Mesh();

            //为这个数组声明segments+1个元素
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            }

            // 生成正面三角形索引 
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// 绘制圆形网格,支持双面渲染
        /// </summary>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        Mesh CreateCircleMeshDouble(int segments, float radius)
        {
            Mesh mesh = new Mesh();

            // 为这个数组声明segments+1个元素 
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 6]; // 双面渲染需要两倍的三角形索引 

            vertices[0] = Vector3.zero;
            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            }

            // 生成正面三角形索引 
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }

            // 生成反面三角形索引 
            for (int i = 0; i < segments; i++)
            {
                triangles[segments * 3 + i * 3] = 0;
                triangles[segments * 3 + i * 3 + 1] = (i + 1) % segments + 1;
                triangles[segments * 3 + i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// 创建一个平面圆形游戏物体(默认仅背面渲染,摄像机从Z低处向上看得到),会立马出现在场景,可手动隐藏或保存为预制体(素材文件)后清除它.
        /// </summary>
        /// <param name="name">预制体名称</param>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <param name="shaderName">Shader名称</param>
        /// <returns></returns>
        GameObject CreatePrefab(string name, int segments, float radius, string shaderName)
        {
            GameObject prefab = new GameObject(name);
            MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateCircleMeshBack(segments, radius, 1.0f);
            MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
            //纹理数据会被加载并传递给Shader,但由于Shader被固定为输出红色,后面即使meshRenderer.material.mainTexture=各种纹理都不会影响
            meshRenderer.material = new Material(Shader.Find(shaderName));
            return prefab;
        }

        /// <summary>
        /// 隐藏物体
        /// </summary>
        /// <param name="obj"></param>
        void HideObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        /// <summary>
        /// 显示物体
        /// </summary>
        /// <param name="obj"></param>
        void ShowObject(GameObject obj)
        {
            obj.SetActive(true);
        }
    }
}
