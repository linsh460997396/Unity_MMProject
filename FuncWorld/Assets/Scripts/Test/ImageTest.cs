using UnityEngine;
using UnityEngine.UI;

public class ImageTest : MonoBehaviour
{
    void Start()
    {
        // ʹ�� this �ؼ��ֻ�ȡ��ǰ���ص���Ϸ����(������)
        GameObject currentGameObject = gameObject;
        Debug.Log("��ǰ��Ϸ�����������: " + currentGameObject.name);

        // ʹ�� transform ���Ի�ȡ Transform ���
        Transform currentTransform = transform;
        Debug.Log("��ǰ��Ϸ�����λ����: " + currentTransform.position);

        Image image = currentGameObject.AddComponent<Image>();
        image.color = new Color32(255, 0, 0, 255);
    }
}
