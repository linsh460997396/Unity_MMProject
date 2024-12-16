using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 团块扩展组件：团块的网格渲染器为空时，将团块游戏物体设为26层（不碰撞层）。
    /// </summary>
    public class CellChunkExtension : MonoBehaviour
    {
        void Awake()
        {
            //团块的网格渲染器为空时
            if (GetComponent<MeshRenderer>() == null)
            {
                //将团块游戏物体设为26层（不碰撞层）
                gameObject.layer = 26;

            }
        }
    }
}
