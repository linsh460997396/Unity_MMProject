using UnityEngine;
using UnityEngine.UI;

public class CircularTextureTest : MonoBehaviour
{
    void Start()
    {
        // 创建一个5x5的Texture2D对象
        Texture2D texture = new Texture2D(5, 5);

        // 设置Texture的FilterMode和WrapMode
        //texture.filterMode = FilterMode.Point;
        //texture.wrapMode = TextureWrapMode.Clamp;

        // 圆的半径（以像素为单位）
        int radius = 2; // 对于5x5的纹理，半径为2会创建一个完整的圆形区域

        // 遍历纹理中的每个像素
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                // 计算像素中心到纹理中心的距离
                float distance = Mathf.Sqrt(Mathf.Pow(x - texture.width / 2, 2) + Mathf.Pow(y - texture.height / 2, 2));

                // 如果像素在圆形区域内，则设置为红色
                if (distance <= radius)
                {
                    texture.SetPixel(x, y, new Color32(255, 0, 0, 255));
                }
                else
                {
                    // 否则设置为透明或你想要的背景色
                    texture.SetPixel(x, y, new Color32(0, 0, 0, 0));
                }
            }
        }

        // 应用像素更改到Texture
        texture.Apply();

        // 将Texture赋给一个Material或其他需要它的地方
        // 例如：GetComponent<Renderer>().material.mainTexture = texture;

        RawImage rawImage = gameObject.AddComponent<RawImage>();
        rawImage.texture = texture;
        //transform.localScale = Vector3.one;
    }
}