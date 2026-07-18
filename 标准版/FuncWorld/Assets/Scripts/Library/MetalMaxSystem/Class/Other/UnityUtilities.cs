//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// Unity通用方法类.
    /// </summary>
    public class UnityUtilities : MonoBehaviour
    {
        /// <summary>
        /// 等待当前帧结束,在下一帧的Update前运行一次(仅自动运行1次,在当前帧反复使用同一个实例无效).
        /// </summary>
        public static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// 预制体字典.
        /// 当RuntimePrefab.Add(string key, Object obj,bool clone = false)中的clone参数为true时,资源为副本存储,
        /// 当clone参数为false时,直接存储对象,摧毁原对象会影响该字典内容,场景切换时未被DontDestroyOnLoad保护的实例会被Unity自动销毁‌,请做好保护.
        /// </summary>
        public static RuntimePrefab runtimePrefab = ScriptableObject.CreateInstance<RuntimePrefab>();

        /// <summary>
        /// 查找物体.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static GameObject FindGameObject(GameObject parent, string childName)
        {
            //获取所有Transform组件实例(包括隐藏的)
            Transform[] allChildren = parent.transform.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                //Debug.Log($"Checking child: {child.gameObject.name}");
                if (child.gameObject.name == childName)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// 检测游戏物体是否包含Transform组件外的组件,有则返回true.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>当发现任何非Transform组件时返回true,否则全部遍历结束返回false</returns>
        public static bool HasEssentialComponents(GameObject obj)
        {
            return obj.GetComponents<Component>().Where(c => c != null).Any(c => !(c is Transform));
        }
        /// <summary>
        /// 检测游戏物体是否含指定名称外的组件,有则返回true.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="excludeTypeNames">任意组件类型名称字符串</param>
        /// <returns></returns>
        public static bool HasComponentsExcluding(GameObject obj, string[] excludeComponentNames)
        {
            return obj.GetComponents<Component>()
                .Where(c => c != null)
                .Any(c => !excludeComponentNames.Contains(c.GetType().Name)
                       && !excludeComponentNames.Contains(c.GetType().FullName));
        }

        /// <summary>
        /// 加载外部图片(PNG或JPG)并按指定网格行列数分割为精灵数组.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="isTopLeftOrigin"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Sprite[] SplitImageToSprites(string path, int gridX, int gridY, bool isTopLeftOrigin = false, float pixelsPerUnit = 100f)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D sourceTexture = new Texture2D(2, 2);
            sourceTexture.LoadImage(bytes);
            sourceTexture.filterMode = FilterMode.Bilinear;

            int cellWidth = sourceTexture.width / gridX;
            int cellHeight = sourceTexture.height / gridY;
            List<Sprite> sprites = new List<Sprite>();

            for (int y = 0; y < gridY; y++)
            {
                int currentY = isTopLeftOrigin ?
                    (gridY - 1 - y) : y;

                for (int x = 0; x < gridX; x++)
                {
                    Rect rect = new Rect(
                        x * cellWidth,
                        currentY * cellHeight,
                        cellWidth,
                        cellHeight
                    );

                    Sprite sprite = Sprite.Create(
                        sourceTexture,
                        rect,
                        new Vector2(0.5f, 0.5f),
                        pixelsPerUnit
                    );
                    sprite.name = $"slice_{x}_{y}";
                    sprites.Add(sprite);
                }
            }
            return sprites.ToArray();
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
        /// 返回材质名为materialName的预制体.此预制体包含路径texturePath的主纹理图,采用名为"Standard"的Shader.
        /// </summary>
        /// <param name="materialName">材质名</param>
        /// <param name="texturePath">纹理图片地址</param>
        /// <returns></returns>
        public static Material CPMat(string materialName, string texturePath)
        {
            return CPMat(materialName, texturePath, "Standard");
        }
        /// <summary>
        /// 返回材质名为materialName的预制体.此预制体包含路径texturePath的主纹理图,采用名为shaderName的Shader.
        /// </summary>
        /// <param name="materialName">材质名</param>
        /// <param name="texturePath">纹理图片地址</param>
        /// <param name="shaderName">Shader名</param>
        /// <returns></returns>
        public static Material CPMat(string materialName, string texturePath, string shaderName)
        {
            Material cachedMaterial;
            Shader shader = Shader.Find(shaderName);
            if (!runtimePrefab.ContainsKey(materialName))
            {
                // 创建新材质并配置基础属性
                if (shader == null)
                {
                    Debug.LogError(shaderName + "未包含在构建中!");
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
        /// 获取或创建预制体通用方法.
        /// </summary>
        /// <param name="name">预制体名称</param>
        /// <param name="onCreate">对象不存在时,创建后的配置回调(仅在新创建时调用)</param>
        /// <returns>预制体GameObject</returns>
        private static GameObject GetOrCreatePrefab(string name, System.Action<GameObject> onCreate = null)
        {
            if (runtimePrefab.ContainsKey(name))
                return runtimePrefab.Get(name) as GameObject;

            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                onCreate?.Invoke(go);
            }
            runtimePrefab.Add(name, go);
            return go;
        }
        /// <summary>
        /// 获取Sun预制体.作为单例直接使用.
        /// 如不存在,会创建名为"Sun"的游戏物体并添加Light组件.
        /// 首次创建的预制体不激活.
        /// </summary>
        public static GameObject Sun
        {
            get
            {
                return GetOrCreatePrefab("Sun", go =>
                {
                    // 太阳位置在场景上方(Y=50),朝向地面倾斜照射
                    go.transform.SetPositionAndRotation(new Vector3(0f, 50f, 0f), Quaternion.Euler(60f, 28.5f, 90f));
                    Light light = go.AddComponent<Light>();
                    light.type = LightType.Directional; // 平行光(模拟太阳光)
                    light.intensity = 1.0f; // 光源强度(1.0为满强度)
                    light.range = 100f; // 光源范围(平行光此参数实际不影响光照)
                    light.color = Color.white; // 白色光源
                    // light.shadowStrength = 0.5f; // 阴影强度(0-1),当前关闭阴影
                    // light.shadowBias = 0.001f; // 阴影偏移,防止阴影贴图瑕疵
                    // light.shadowNormalBias = 0.001f; // 阴影法线偏移
                    light.cullingMask = ~0; // 照亮所有层(~0即-1,二进制取反)
                    go.SetActive(true); // 光源需要立即激活才能照亮场景
                    Debug.Log($"预制体已创建: Sun");
                });
            }
        }
        /// <summary>
        /// 获取MainCamera预制体.作为单例直接使用.
        /// 如不存在,会创建名为"MainCamera"的游戏物体并添加Camera组件.
        /// </summary>
        public static GameObject MainCamera
        {
            get
            {
                return GetOrCreatePrefab("MainCamera", go =>
                {
                    // 主摄像机位置在正后方(Z=-20)
                    go.transform.SetPositionAndRotation(new Vector3(0f, 0f, -20f), Quaternion.identity);
                    Camera camera = go.AddComponent<Camera>();
                    camera.tag = "MainCamera"; // 设置标签以便Camera.main能识别
                    camera.clearFlags = CameraClearFlags.SolidColor; // 纯色背景,确保光源生效
                    camera.backgroundColor = new Color(0.2f, 0.4f, 0.6f, 1f); // 蓝色背景(天空色)
                    // cullingMask使用位运算指定渲染层:(1 << 层序号) 表示包含该层
                    // Default(0) | TransparentFX(1) | IgnoreRaycast(2) | Water(4) | UI(5)
                    camera.cullingMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5);
                    camera.depth = 0f; // 相机渲染顺序,数值越大越后渲染(-1为小地图相机)
                    camera.nearClipPlane = 0.3f; // 近裁剪面
                    camera.farClipPlane = 1000f; // 远裁剪面
                    camera.allowMSAA = true; // 开启多重采样抗锯齿
                    go.SetActive(true); // 主摄像机需要立即激活
                    Debug.Log($"预制体已创建: MainCamera");
                });
            }
        }
        /// <summary>
        /// 获取SubCamera预制体.作为单例直接使用.
        /// 如不存在,会创建名为"SubCamera"的游戏物体并添加Camera组件.
        /// 首次创建不激活.
        /// </summary>
        public static GameObject SubCamera
        {
            get
            {
                return GetOrCreatePrefab("SubCamera", go =>
                {
                    // 主摄像机位置在正后方(Z=-20)
                    go.transform.SetPositionAndRotation(new Vector3(0f, 0f, -20f), Quaternion.identity);
                    Camera camera = go.AddComponent<Camera>();
                    camera.tag = "SubCamera"; // 设置标签
                    camera.clearFlags = CameraClearFlags.SolidColor; // 纯色背景,确保光源生效
                    camera.backgroundColor = new Color(0.2f, 0.4f, 0.6f, 1f); // 蓝色背景(天空色)
                    // cullingMask使用位运算指定渲染层:(1 << 层序号) 表示包含该层
                    // Default(0) | TransparentFX(1) | IgnoreRaycast(2) | Water(4) | UI(5)
                    camera.cullingMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5);
                    camera.depth = 0f; // 相机渲染顺序,数值越大越后渲染(-1为小地图相机)
                    camera.nearClipPlane = 0.3f; // 近裁剪面
                    camera.farClipPlane = 1000f; // 远裁剪面
                    camera.allowMSAA = true; // 开启多重采样抗锯齿
                    go.SetActive(false);
                });
            }
        }
    }
}
#endif
