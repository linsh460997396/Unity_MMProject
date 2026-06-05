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

        /// <summary>
        /// OnEnable后在下一帧的Update前运行一次(仅自动运行1次,反复激活无效)
        /// </summary>
        public void Start()
        {
            // 游戏入口 - 显示开局菜单
            //ShowStartMenu();

            // 直接测试体素世界
            Init();
        }

        /// <summary>
        /// 显示开局菜单
        /// </summary>
        private void ShowStartMenu()
        {
            // 创建临时相机用于渲染开局菜单UI
            CreateSubCamera();

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
        private void CreateSubCamera()
        {
            if (Camera.main == null)
            {
                SpriteSpace.SpriteSpacePrefab.SubCamera.SetActive(true);
            }
        }

        /// <summary>
        /// 初始化框架 - 在玩家选择六边形区域后调用
        /// </summary>
        public static void Init()
        {
            SpriteSpacePrefab.Init();
            scene = FindObjectOfType<Main_MMWorld>().gameObject.AddComponent<Scene>();
            scene.Init(new CellGridContainer(100));
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
