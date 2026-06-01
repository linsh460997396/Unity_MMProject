using CellSpace;
using SpriteSpace;
using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 主入口
    /// </summary>
    public class Main_MMWorld : MonoBehaviour
    {
        public static Scene scene;
        public static bool sceneEnabled = false;
        public static bool initializeFrameworks = false;

        ///// <summary>
        ///// 挂上组件的当前帧运行(无论是否激活,仅自动运行1次).
        ///// </summary>
        public void Awake()
        {
            // Awake中尽量少写东西，主要逻辑在Start
        }

        ///// <summary>
        ///// 组件激活时的当前帧运行一次(反复激活反复运行).
        ///// </summary>
        //public void OnEnable(){}

        /// <summary>
        /// OnEnable后在下一帧的Update前运行一次(仅自动运行1次,反复激活无效)
        /// </summary>
        public void Start()
        {
            // 游戏入口 - 显示开局菜单
            ShowStartMenu();

            //测试体素世界
            //InitializeFrameworks();
        }

        /// <summary>
        /// 显示开局菜单
        /// </summary>
        private void ShowStartMenu()
        {
            // 创建临时相机用于渲染开局菜单UI
            CreateTempCamera();

            // 获取或创建GameStartMenu
            GameStartMenu menu = FindObjectOfType<GameStartMenu>();
            if (menu == null)
            {
                GameObject menuObj = new GameObject("GameStartMenu");
                menu = menuObj.AddComponent<GameStartMenu>();
            }

            // 显示菜单
            menu.ShowStartMenu();
        }

        /// <summary>
        /// 创建临时相机用于渲染开局菜单
        /// </summary>
        private void CreateTempCamera()
        {
            if (Camera.main == null)
            {
                GameObject cameraObj = new GameObject("StartMenuCamera");
                Camera camera = cameraObj.AddComponent<Camera>();
                camera.tag = "MainCamera";
                camera.backgroundColor = Color.black;
            }
        }

        /// <summary>
        /// 初始化框架 - 在玩家选择六边形区域后调用
        /// </summary>
        public static void InitializeFrameworks()
        {
            SpriteSpacePrefab.Init();
            if (scene == null)
            {
                Main_MMWorld main = FindObjectOfType<Main_MMWorld>();
                if (main != null)
                {
                    scene = main.gameObject.GetComponent<Scene>();
                    if (scene != null)
                    {
                        scene.enabled = false;
                    }
                }
            }
            CPEngine.Active();
            sceneEnabled = false;
            initializeFrameworks = true;
        }

        /// <summary>
        /// 每帧运行.刚挂上组件时的第一帧不运行.
        /// </summary>
        public void Update()
        {
            if (initializeFrameworks && sceneEnabled == false && CellChunkManager.SpawningChunks == false)
            {//场景组件未启用且团块空间停止生成时
                sceneEnabled = true;
                gameObject.GetComponent<Scene>().enabled = true;//启用场景组件
            }
            if (initializeFrameworks)
            {
                CPEngine.Tick();
            }
        }
    }
}
