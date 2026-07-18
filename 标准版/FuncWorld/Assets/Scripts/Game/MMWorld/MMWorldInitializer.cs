using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// MMWorld初始化脚本 - 游戏启动时自动创建必要组件
    /// </summary>
    public class MMWorldInitializer : MonoBehaviour
    {
        private void Awake()
        {
            // 确保GameManager存在
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }

            // 确保MapIndex存在
            if (MapIndex.Instance == null)
            {
                GameObject mapIndexObj = new GameObject("MapIndex");
                mapIndexObj.AddComponent<MapIndex>();
            }

            // 确保GameStartMenu存在
            if (FindObjectOfType<GameStartMenu>() == null)
            {
                GameObject menuObj = new GameObject("GameStartMenu");
                menuObj.AddComponent<GameStartMenu>();
            }
        }

        private void Start()
        {
            // 初始化完成,开局菜单会自动显示
        }
    }
}