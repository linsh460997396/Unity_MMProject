using UnityEngine;
using MMWorld.HexSphere;

namespace MMWorld
{
    /// <summary>
    /// Planet 框架测试场景启动器（纯代码，不依赖任何 prefab/场景配置）。
    /// 用法：在空场景中建一个空 GameObject，挂上此脚本，Play 即可。
    /// 
    /// 自动创建：
    /// - Main Camera（带 tag + 初始位置）
    /// - Directional Light（太阳光）
    /// - PlanetRoot 星球根对象（含 HexPlanetManager + HexPlanetController）
    /// - PlanetTestUI 测试面板（OnGUI，用于调试功能）
    /// </summary>
    [RequireComponent(typeof(PlanetTestUI))]
    public class PlanetTestSceneBootstrapper : MonoBehaviour
    {
        [Header("星球参数（将传递给 HexPlanetController）")]
        public HexPlanetController.TerrainGeneratorType terrainType = HexPlanetController.TerrainGeneratorType.Perlin;
        public float planetRadius = 50f;
        [Range(0, 7)] public int subdivisions = 3;
        [Range(0, 6)] public int chunkSubdivisions = 2;
        public Color basePlanetColor = new Color(0.4f, 0.6f, 0.3f);

        [Header("相机初始距离")]
        public float initialOrbitDistance = 130f;
        public float minOrbitRadius = 70f;
        public float maxOrbitRadius = 300f;

        [Header("是否显示测试UI面板")]
        public bool showTestUI = true;

        private void Awake()
        {
            EnsureMainCamera();
            EnsureLight();
            CreatePlanet();

            PlanetTestUI ui = GetComponent<PlanetTestUI>();
            if (ui != null) ui.enabled = showTestUI;

            Debug.Log("[PlanetTestSceneBootstrapper] 测试场景搭建完成");
        }

        /// <summary>确保场景中有 Main Camera（tag=MainCamera）</summary>
        private void EnsureMainCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = Vector3.forward * initialOrbitDistance;
                mainCam.transform.LookAt(Vector3.zero);
                return;
            }

            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            camGO.transform.position = Vector3.forward * initialOrbitDistance;
            camGO.transform.LookAt(Vector3.zero);
            camGO.AddComponent<AudioListener>();
        }

        /// <summary>确保场景中有方向光</summary>
        private void EnsureLight()
        {
            Light sun = FindObjectOfType<Light>();
            if (sun != null) return;

            GameObject lightGO = new GameObject("Directional Light");
            Light l = lightGO.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = Color.white;
            l.intensity = 1.0f;
            l.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        /// <summary>创建星球根对象并挂载控制器</summary>
        private void CreatePlanet()
        {
            GameObject planetRoot = new GameObject("PlanetRoot");
            planetRoot.transform.position = Vector3.zero;

            planetRoot.AddComponent<HexPlanetManager>();
            HexPlanetController controller = planetRoot.AddComponent<HexPlanetController>();

            // 由于 HexPlanetController 的星球参数是 [SerializeField] private，
            // 这里通过反射赋值（保持 Controller 的 API 封装性）
            SetPrivateField(controller, "terrainType", terrainType);
            SetPrivateField(controller, "planetRadius", planetRadius);
            SetPrivateField(controller, "subdivisions", subdivisions);
            SetPrivateField(controller, "chunkSubdivisions", chunkSubdivisions);
            SetPrivateField(controller, "basePlanetColor", basePlanetColor);
            SetPrivateField(controller, "minOrbitRadius", minOrbitRadius);
            SetPrivateField(controller, "maxOrbitRadius", maxOrbitRadius);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var f = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance);
            if (f != null)
            {
                f.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[PlanetTestSceneBootstrapper] 字段不存在: {fieldName}");
            }
        }
    }
}
