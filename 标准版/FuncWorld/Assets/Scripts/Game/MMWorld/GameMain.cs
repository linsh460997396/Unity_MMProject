using CellSpace;
using SpriteSpace;
using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 主入口
    /// </summary>
    public class GameMain : MonoBehaviour
    {
        private static string _name = "GameMain";
        public static string Name
        {
            get { if (string.IsNullOrEmpty(_name)) return "GameMain"; return _name; }
            set { if (!string.IsNullOrEmpty(value)) _name = value; }
        }
        private static GameMain _instance;
        public static GameMain Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = GameObject.Find(Name);
                    if (obj == null) obj = new GameObject(Name);
                    if (obj.GetComponent<GameMain>() == null) _instance = obj.AddComponent<GameMain>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        public static Scene scene;

        //在场景加载前执行
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnBeforeSceneLoad()
        {
            var temp = Instance;
        }

        /// <summary>
        /// OnEnable后在下一帧的Update前运行一次(仅自动运行1次,反复激活无效)
        /// </summary>
        public void Start()
        {
            // 精灵框架初始化
            SpriteSpacePrefab.Init();
            // 创建精灵框架的相机并激活
            SpriteSpacePrefab.MainCamera.SetActive(true);

            // 创建菜单
            GameUI.Create();

            // 直接测试游戏
            //Run();
        }

        /// <summary>
        /// 初始化游戏框架.在玩家选择星球区域后调用.
        /// </summary>
        public static void Run()
        {
            scene = GameMain.Instance.gameObject.AddComponent<Scene>();
            scene.Init(new CellGridContainer(100));

            //MC框架激活
            CPEngine.Active();
        }

        /// <summary>
        /// 每帧运行.刚挂上组件时的第一帧不运行.
        /// </summary>
        public void Update()
        {
            if (CPEngine.initialized)
            {
                CPEngine.Tick();
            }
        }
    }
}
