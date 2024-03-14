using UnityEngine;
using UnityEngine.UI;

public class ImageTest : MonoBehaviour
{
    void Start()
    {
        // 使用 this 关键字获取当前挂载的游戏物体(可隐藏)
        GameObject currentGameObject = gameObject;
        Debug.Log("当前游戏物体的名称是: " + currentGameObject.name);

        // 使用 transform 属性获取 Transform 组件
        Transform currentTransform = transform;
        Debug.Log("当前游戏物体的位置是: " + currentTransform.position);

        Image image = currentGameObject.AddComponent<Image>();
        image.color = new Color32(255, 0, 0, 255);
    }
}
