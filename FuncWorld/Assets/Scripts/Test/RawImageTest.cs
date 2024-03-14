using UnityEngine;
using UnityEngine.UI;

public class RawImageTest : MonoBehaviour
{
    void Start()
    {
        // ʹ�� this �ؼ��ֻ�ȡ��ǰ���ص���Ϸ����(������)
        GameObject currentGameObject = gameObject;
        Debug.Log("��ǰ��Ϸ�����������: " + currentGameObject.name);

        // ʹ�� transform ���Ի�ȡ Transform ���
        Transform currentTransform = transform;
        Debug.Log("��ǰ��Ϸ�����λ����: " + currentTransform.position);

        Texture2D texture = new Texture2D(5, 5);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, new Color32(255, 0, 0, 255));
            }
        }

        texture.Apply();

        RawImage rawImage = currentGameObject.AddComponent<RawImage>();
        rawImage.texture = texture;
    }
}
