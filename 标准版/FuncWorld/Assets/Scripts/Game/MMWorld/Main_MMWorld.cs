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

        ///// <summary>
        ///// 挂上组件的当前帧运行(无论是否激活,仅自动运行1次).
        ///// </summary>
        //public void Awake(){}

        ///// <summary>
        ///// 组件激活时的当前帧运行一次(反复激活反复运行).
        ///// </summary>
        //public void OnEnable(){}

        /// <summary>
        /// OnEnable后在下一帧的Update前运行一次(仅自动运行1次,反复激活无效)
        /// </summary>
        public void Start()
        {
            SpriteSpacePrefab.Init();
            scene = gameObject.GetComponent<Scene>();
            CPEngine.Active();
        }

        /// <summary>
        /// 每帧运行.刚挂上组件时的第一帧不运行.
        /// </summary>
        public void Update()
        {
            if (sceneEnabled == false && CellChunkManager.SpawningChunks == false)
            {//场景组件未启用且团块空间停止生成时
                sceneEnabled = true;
                gameObject.GetComponent<Scene>().enabled = true;//启用场景组件
            }
            CPEngine.Tick();
        }
    }
}
