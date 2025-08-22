using MetalMaxSystem.Unity;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// CellSpace预制体类.用来模拟不在场景的类似从AB包素材加载后的预制体特性.这些预制体都是代码组装起来的.
    /// 注意场景切换时未被DontDestroyOnLoad保护的预制体实例会被Unity自动销毁‌,请对存储在RuntimePrefab的预制体做好保护.
    /// </summary>
    public class CellSpacePrefab : MonoBehaviour
    {
        /// <summary>
        /// 预制体字典.可模拟AB包素材加载后进Native内存,存储GameObject后Destroy原对象也不影响Native内存已存储的预制体,容器内的预制体不会出现在Unity场景.
        /// 摧毁该字典内容时请调用Resources.UnloadAsset(runtimePrefab[key])来标记销毁.
        /// </summary>
        public static RuntimePrefab runtimePrefab = ScriptableObject.CreateInstance<RuntimePrefab>();

        /// <summary>
        /// 用来阻止挂组件时自动Awake一次.而Start、Update那些就不用阻止了,因为runtimePrefab不在场景,即便预制体上组件Enable也不起作用.
        /// </summary>
        public static Dictionary<string, bool> awakeEnable = new Dictionary<string, bool>();
        /// <summary>
        /// 预制体实例化后的父级GameObject.
        /// </summary>
        public static GameObject group;
        /// <summary>
        /// 预制体初始化是否完成.
        /// </summary>
        public static bool initialized;
        /// <summary>
        /// 外部纹理图片路径(目前框架唯一用到的素材就是图片).
        /// 留空则使用Unity编辑器内部路径.
        /// </summary>
        public static string externalTexturePath;

        /// <summary>
        /// 预制体初始化方法.在使用CellSpacePrefab前必须调用此方法来确保预制体已被创建.
        /// </summary>
        public static void Init()
        {
            if (initialized) return;
            group = new GameObject("CellSpacePrefab");
            DontDestroyOnLoad(group);
            runtimePrefab.hideFlags = HideFlags.DontUnloadUnusedAsset; //资源持久化标记
            var temp = CPEngine; //顺带初始化其他预制体
            initialized = true;
        }

        /// <summary>
        /// 获取CPEngine预制体.作为单例直接使用.
        /// 如不存在,会创建名为"CPEngine"的游戏物体并添加CPEngine、CellChunkManager、CPConnectionInitializer组件.
        /// 首次创建的预制体不激活.
        /// </summary>
        public static GameObject CPEngine
        {
            get
            {
                string name = "CPEngine";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = new GameObject(name);
                    tempGameObject.SetActive(false);
                    tempGameObject.transform.parent = group.transform;
                    tempGameObject.AddComponent<CPConnectionInitializer>();
                    CPConnectionInitializer.NetworkPrefab = CPNetwork;
                    CellSpace.CPEngine.chunkManagerInstance = tempGameObject.AddComponent<CellChunkManager>();
                    CellChunkManager.ChunkPrefab = CellChunk;
                    tempGameObject.AddComponent<CPEngine>();
                    runtimePrefab.Add(name, tempGameObject);
                    awakeEnable[name] = true;
                    Debug.Log($"预制体已创建: {name}");
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 获取CPNetwork预制体.此预制体包含Client和Server组件,用于网络通信.作为单例直接使用.
        /// </summary>
        public static GameObject CPNetwork
        {
            get
            {
                string name = "CPNetwork";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = new GameObject(name);
                    tempGameObject.SetActive(false);
                    tempGameObject.transform.parent = group.transform;
                    tempGameObject.AddComponent<Client>();
                    tempGameObject.AddComponent<Server>();
                    runtimePrefab.Add(name, tempGameObject);
                    awakeEnable[name] = true;
                    Debug.Log($"预制体已创建: {name}");
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 获取CellChunk预制体.此预制体包含CellChunk相关组件,用于处理团块空间网格数据.作为模板使用,后续会创建多份实例复制体.
        /// </summary>
        public static GameObject CellChunk
        {
            get
            {
                string name = "CellChunk";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = new GameObject(name);
                    tempGameObject.SetActive(false);
                    tempGameObject.transform.parent = group.transform;
                    tempGameObject.AddComponent<MeshFilter>();
                    tempGameObject.AddComponent<MeshRenderer>();
                    if (string.IsNullOrEmpty(externalTexturePath))
                    {
                        //Unity编辑器模式下的路径
                        externalTexturePath = Application.dataPath + @"/CellSpace/Res/Textures";
                    }
                    tempGameObject.GetComponent<MeshRenderer>().materials = new Material[4]
                    {
                    CPMat("CPMat", externalTexturePath+@"/CPTextureSheet.png"), //第1个材质
                    CPMat("CPMat1", externalTexturePath+@"/CPTextureSheet1.png"), //第2个材质
                    CPMat("CPMat2", externalTexturePath+@"/CPTextureSheet2.png"), //第3个材质
                    CPMat("CPMat3", externalTexturePath+@"/CPTextureSheet3.png")  //第4个材质
                    };
                    tempGameObject.AddComponent<MeshCollider>();
                    tempGameObject.AddComponent<CellChunk>();
                    tempGameObject.GetComponent<CellChunk>().MeshContainer = CellChunkAdditionalMesh;
                    tempGameObject.GetComponent<CellChunk>().ChunkCollider = CellChunkTriggerCollider;
                    tempGameObject.AddComponent<CellChunkMeshCreator>();
                    tempGameObject.GetComponent<CellChunkMeshCreator>().Cube = GetCubeMesh();
                    tempGameObject.AddComponent<CellChunkDataFiles>();
                    tempGameObject.AddComponent<CPCustomTerrainGenerator>();
                    runtimePrefab.Add(name, tempGameObject);
                    awakeEnable[name] = true;
                    Debug.Log($"预制体已创建: {name}");
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 获取CellChunk预制体.此预制体包含CellChunk相关组件,用于处理团块空间网格数据.作为模板使用,后续会创建多份实例复制体.
        /// </summary>
        public static GameObject CellChunkAdditionalMesh
        {
            get
            {
                string name = "CellChunkAdditionalMesh";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = new GameObject(name);
                    tempGameObject.SetActive(false);
                    tempGameObject.transform.parent = group.transform;
                    tempGameObject.AddComponent<MeshFilter>();
                    tempGameObject.AddComponent<MeshRenderer>();
                    if (string.IsNullOrEmpty(externalTexturePath))
                    {
                        //Unity编辑器模式下的路径或本地测试路径
                        externalTexturePath = Application.dataPath + @"/CellSpace/Res/Textures";
                    }
                    tempGameObject.GetComponent<MeshRenderer>().materials = new Material[4]
                    {
                    CPMat("CPMat", externalTexturePath+@"/CPTextureSheet.png"), //第1个材质
                    CPMat("CPMat1", externalTexturePath+@"/CPTextureSheet1.png"), //第2个材质
                    CPMat("CPMat2", externalTexturePath+@"/CPTextureSheet2.png"), //第3个材质
                    CPMat("CPMat3", externalTexturePath+@"/CPTextureSheet3.png")  //第4个材质
                    };
                    tempGameObject.AddComponent<MeshCollider>();
                    tempGameObject.AddComponent<CellChunkExtension>();
                    runtimePrefab.Add(name, tempGameObject);
                    awakeEnable[name] = true;
                    Debug.Log($"预制体已创建: {name}");
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 获取CellChunk预制体.此预制体包含CellChunk相关组件,用于处理团块空间网格数据.作为模板使用,后续会创建多份实例复制体.
        /// </summary>
        public static GameObject CellChunkTriggerCollider
        {
            get
            {
                string name = "CellChunkTriggerCollider";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = new GameObject(name);
                    tempGameObject.SetActive(false);
                    tempGameObject.transform.parent = group.transform;
                    tempGameObject.AddComponent<MeshCollider>();
                    tempGameObject.AddComponent<CellChunkExtension>();
                    runtimePrefab.Add(name, tempGameObject);
                    awakeEnable[name] = true;
                    Debug.Log($"预制体已创建: {name}");
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        /// <summary>
        /// 获取材质"CPMat"的预制体.此预制体包含"CPTextureSheet"主纹理图,采用名为"Standard"的Shader.
        /// 其余CPMat1~3挂载在CellChunk和CellChunkAdditionalMesh的MeshRenderer组件上,是第2~4个材质,分别有不同主纹理图.
        /// 框架所用到的所有地块uv都是从主纹理图上划取.
        /// </summary>
        /// <param name="materialName">材质名</param>
        /// <param name="texturePath">纹理图片地址</param>
        /// <returns></returns>
        public static Material CPMat(string materialName, string texturePath)
        {
            Material cachedMaterial = null;
            Shader shader = Shader.Find("Standard");
            if (!runtimePrefab.ContainsKey(materialName))
            {
                // 创建新材质并配置基础属性
                if (shader == null)
                {
                    Debug.LogError("Standard Shader未包含在构建中!");
                }
                else
                {
                    cachedMaterial = new Material(shader)
                    {
                        name = materialName,
                        mainTexture = LoadImageAndConvertToTexture2D(texturePath)
                    };
                    runtimePrefab.Add(materialName, cachedMaterial);
                    Debug.Log($"材质已创建: {materialName}");
                }
            }
            return runtimePrefab.Get(materialName) as Material;
        }

        /// <summary>
        /// 读取图片并转Texture2D,仅支持png和jpg
        /// </summary>
        /// <param name="filePath">完整的图片文件路径</param>
        /// <returns></returns>
        public static Texture2D LoadImageAndConvertToTexture2D(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning(filePath);
                return null;
            }
            byte[] fileData = File.ReadAllBytes(filePath); //打包后的程序中运行,路径不对的话会卡在这里
            Texture2D texture = new Texture2D(2, 2); //随便定义初始尺寸但不可为null
            bool success = texture.LoadImage(fileData); //加载图片Unity会自动调整尺寸
            if (success)
            {
                //图片加载成功
                Debug.Log("Image loaded successfully with width: " + texture.width + " and height: " + texture.height);
            }
            else
            {
                //图片加载失败,可能需要检查字节数组是否有效或图片格式是否支持
                Debug.LogError("Failed to load image.");
            }
            return texture;
        }

        /// <summary>
        /// 获取Cube网格（优先查找内置资源，不存在则动态生成）
        /// </summary>
        public static Mesh GetCubeMesh()
        {
            // 1. 尝试获取内置Cube资源
            Mesh builtinMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            if (builtinMesh != null)
            {
                return builtinMesh;
            }

            // 2. 动态生成Cube网格
            Mesh dynamicMesh = new Mesh();
            dynamicMesh.name = "DynamicCube";

            // 定义8个顶点（中心点(0,0,0)，边长1）
            Vector3[] vertices = {
            // 前面
            new Vector3(-0.5f, -0.5f, -0.5f), // 0 左下
            new Vector3( 0.5f, -0.5f, -0.5f), // 1 右下
            new Vector3( 0.5f,  0.5f, -0.5f), // 2 右上
            new Vector3(-0.5f,  0.5f, -0.5f), // 3 左上
            // 后面
            new Vector3(-0.5f, -0.5f,  0.5f), // 4
            new Vector3( 0.5f, -0.5f,  0.5f), // 5
            new Vector3( 0.5f,  0.5f,  0.5f), // 6
            new Vector3(-0.5f,  0.5f,  0.5f)  // 7
        };

            // 定义12个三角形（6个面×2个三角）
            int[] triangles = {
            // 前面
            0, 2, 1, 0, 3, 2,
            // 后面
            5, 6, 4, 4, 6, 7,
            // 左面
            4, 7, 0, 0, 7, 3,
            // 右面
            1, 2, 5, 5, 2, 6,
            // 上面
            3, 6, 2, 3, 7, 6,
            // 下面
            0, 1, 4, 4, 1, 5
        };

            // 应用几何数据
            dynamicMesh.vertices = vertices;
            dynamicMesh.triangles = triangles;
            dynamicMesh.RecalculateNormals();
            dynamicMesh.RecalculateBounds();

            return dynamicMesh;
        }

        /// <summary>
        /// 获取支持uv的球体网格（优先查找内置资源,不存在则动态生成）.
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="segments">分段数,越多越像球</param>
        /// <param name="torf">为true时查找内置资源来生成球体(注意内置资源不支持后面2个参数且大小固定)</param>
        /// <returns></returns>
        public static Mesh GetSphereMeshWithUV(bool torf = false, float radius = 1.0f, int segments = 32)
        {
            if (torf)
            {
                Mesh builtinMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
                if (builtinMesh != null)
                {
                    return builtinMesh;
                }
            }

            // 动态创建球体网格
            Mesh dynamicMesh = new Mesh();
            dynamicMesh.name = $"DynamicSphere_{radius}";

            // 生成顶点数据
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            float pi = Mathf.PI;
            float deltaPhi = pi / segments;
            float deltaTheta = 2f * pi / segments;

            // 生成顶点和UV坐标
            for (int i = 0; i <= segments; i++)
            {
                float phi = i * deltaPhi;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                for (int j = 0; j <= segments; j++)
                {
                    float theta = j * deltaTheta;
                    float sinTheta = Mathf.Sin(theta);
                    float cosTheta = Mathf.Cos(theta);

                    // 球面坐标转笛卡尔坐标
                    Vector3 vertex = new Vector3(
                        radius * sinPhi * cosTheta,
                        radius * cosPhi,
                        radius * sinPhi * sinTheta
                    );
                    vertices.Add(vertex);

                    // 等距柱状投影UV映射
                    uvs.Add(new Vector2(
                        theta / (2f * pi) + 0.5f,  // 经度(0-1)
                        1 - phi / pi               // 纬度(0-1)
                    ));
                }
            }

            // 生成三角面
            for (int i = 0; i < segments; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int v1 = i * (segments + 1) + j;
                    int v2 = v1 + segments + 1;
                    int v3 = v1 + 1;
                    int v4 = v2 + 1;

                    // 添加两个三角形组成四边形
                    triangles.Add(v1);
                    triangles.Add(v2);
                    triangles.Add(v3);

                    triangles.Add(v3);
                    triangles.Add(v2);
                    triangles.Add(v4);
                }
            }

            // 应用网格数据
            dynamicMesh.vertices = vertices.ToArray();
            dynamicMesh.uv = uvs.ToArray();
            dynamicMesh.triangles = triangles.ToArray();
            dynamicMesh.RecalculateNormals();
            dynamicMesh.RecalculateBounds();

            return dynamicMesh;
        }
        /// <summary>
        /// 获取纯色球体.
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="segments">分段数,越多越像球</param>
        /// <returns></returns>
        public static Mesh GetSphereMesh(float radius = 1.0f, int segments = 32)
        {
            // 动态生成球体网格
            Mesh dynamicMesh = new Mesh();
            dynamicMesh.name = $"DynamicSphere_{radius}";

            // 定义球体顶点（极角/方位角分段）
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            float pi = Mathf.PI;
            float deltaPhi = pi / segments;
            float deltaTheta = 2f * pi / segments;

            for (int i = 0; i <= segments; i++)
            {
                float phi = i * deltaPhi;
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                for (int j = 0; j <= segments; j++)
                {
                    float theta = j * deltaTheta;
                    float sinTheta = Mathf.Sin(theta);
                    float cosTheta = Mathf.Cos(theta);

                    // 球面坐标转笛卡尔坐标
                    Vector3 vertex = new Vector3(
                        radius * sinPhi * cosTheta,
                        radius * cosPhi,
                        radius * sinPhi * sinTheta
                    );
                    vertices.Add(vertex);
                }
            }

            // 生成三角面（四边形剖分）
            for (int i = 0; i < segments; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int v1 = i * (segments + 1) + j;
                    int v2 = v1 + segments + 1;
                    int v3 = v1 + 1;
                    int v4 = v2 + 1;

                    // 添加两个三角形组成四边形
                    triangles.Add(v1);
                    triangles.Add(v2);
                    triangles.Add(v3);

                    triangles.Add(v3);
                    triangles.Add(v2);
                    triangles.Add(v4);
                }
            }

            // 应用几何数据
            dynamicMesh.vertices = vertices.ToArray();
            dynamicMesh.triangles = triangles.ToArray();
            dynamicMesh.RecalculateNormals();
            dynamicMesh.RecalculateBounds();

            return dynamicMesh;
        }
        /// <summary>
        /// 创建指定缩放因子的球体网格,如果内置资源不存在则使用GetSphereMeshWithUV默认参数方法生成.
        /// </summary>
        /// <param name="scaleFactor"></param>
        /// <returns></returns>
        public static Mesh GetScaledSphereMeshWithUV(float scaleFactor = 1f)
        {
            Mesh builtinMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            if (builtinMesh == null)
            {
                builtinMesh = GetSphereMeshWithUV();
            }

            //创建缩放后的网格副本
            Mesh scaledMesh = Object.Instantiate(builtinMesh);
            Vector3[] vertices = builtinMesh.vertices;

            // 应用缩放
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] *= scaleFactor;
            }

            scaledMesh.vertices = vertices;
            scaledMesh.RecalculateBounds();
            scaledMesh.RecalculateNormals();

            return scaledMesh;
        }
    }
}