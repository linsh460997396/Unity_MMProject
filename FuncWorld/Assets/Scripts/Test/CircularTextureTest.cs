using UnityEngine;
using UnityEngine.UI;

public class CircularTextureTest : MonoBehaviour
{
    void Start()
    {
        // ����һ��5x5��Texture2D����
        Texture2D texture = new Texture2D(5, 5);

        // ����Texture��FilterMode��WrapMode
        //texture.filterMode = FilterMode.Point;
        //texture.wrapMode = TextureWrapMode.Clamp;

        // Բ�İ뾶��������Ϊ��λ��
        int radius = 2; // ����5x5�������뾶Ϊ2�ᴴ��һ��������Բ������

        // ���������е�ÿ������
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                // �����������ĵ��������ĵľ���
                float distance = Mathf.Sqrt(Mathf.Pow(x - texture.width / 2, 2) + Mathf.Pow(y - texture.height / 2, 2));

                // ���������Բ�������ڣ�������Ϊ��ɫ
                if (distance <= radius)
                {
                    texture.SetPixel(x, y, new Color32(255, 0, 0, 255));
                }
                else
                {
                    // ��������Ϊ͸��������Ҫ�ı���ɫ
                    texture.SetPixel(x, y, new Color32(0, 0, 0, 0));
                }
            }
        }

        // Ӧ�����ظ��ĵ�Texture
        texture.Apply();

        // ��Texture����һ��Material��������Ҫ���ĵط�
        // ���磺GetComponent<Renderer>().material.mainTexture = texture;

        RawImage rawImage = gameObject.AddComponent<RawImage>();
        rawImage.texture = texture;
        //transform.localScale = Vector3.one;
    }
}