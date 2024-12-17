//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Test.Example
//{
//    /// <summary>
//    /// DrawLine2DGraphic
//    /// </summary>
//    public class TE_DrawLine2D : MaskableGraphic
//    {
//        private List<List<UIVertex>> vertexQuadList = new List<List<UIVertex>>(); //�洢�����ߵ������ı���
//        private Vector3 lastPoint;
//        private Vector3 lastLeftPoint;
//        private Vector3 lastRightPoint;
//        public float lineWidth = 4;
//        private bool isNewLine;

//        protected override void OnPopulateMesh(VertexHelper vh)
//        {
//            vh.Clear();
//            for (int i = 0; i < vertexQuadList.Count; i++)
//            {
//                vh.AddUIVertexQuad(vertexQuadList[i].ToArray());
//            }
//        }

//        private void Update()
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                lastPoint = ScreenPointToLacalPointInRectangle(Input.mousePosition);
//                isNewLine = true;
//                vertexQuadList.Clear();
//            }
//            else
//            {
//                if (Input.GetMouseButton(0))
//                {
//                    Vector3 currentPoint = ScreenPointToLacalPointInRectangle(Input.mousePosition);
//                    Vector3 vec = currentPoint - lastPoint;
//                    //������С<10���ز�������
//                    if (vec.magnitude < 10) { return; }
//                    Vector3 normal = Vector3.Cross(vec.normalized, Vector3.forward).normalized; //��λ������������ƽ���ڣ�

//                    if (isNewLine)
//                    {
//                        isNewLine = false;
//                        lastLeftPoint = lastPoint + normal * lineWidth;
//                        lastRightPoint = lastPoint - normal * lineWidth;
//                    }

//                    Vector3 currentLeftPoint = currentPoint + normal * lineWidth;
//                    Vector3 currentRightPoint = currentPoint - normal * lineWidth;

//                    List<UIVertex> vertexQuad = new List<UIVertex>();
//                    UIVertex uIVertex = new UIVertex();
//                    uIVertex.position = lastLeftPoint;
//                    uIVertex.color = color;
//                    vertexQuad.Add(uIVertex);

//                    UIVertex uIVertex1 = new UIVertex();
//                    uIVertex1.position = lastRightPoint;
//                    uIVertex1.color = color;
//                    vertexQuad.Add(uIVertex1);

//                    UIVertex uIVertex2 = new UIVertex();
//                    uIVertex2.position = currentRightPoint;
//                    uIVertex2.color = color;
//                    vertexQuad.Add(uIVertex2);

//                    UIVertex uIVertex3 = new UIVertex();
//                    uIVertex3.position = currentLeftPoint;
//                    uIVertex3.color = color;
//                    vertexQuad.Add(uIVertex3);

//                    vertexQuadList.Add(vertexQuad);

//                    lastLeftPoint = currentLeftPoint;
//                    lastRightPoint = currentRightPoint;
//                    //��ֹ�Ͻ�
//                    lastPoint = currentPoint;

//                    //֪ͨ��Ⱦ���涥�������Ѹ��ģ��Ա�����������¼���������Ӧ��ͼ������
//                    SetVerticesDirty();
//                }
//            }

//        }

//        private Vector3 ScreenPointToLacalPointInRectangle(Vector3 screenPoint)
//        {

//            RectTransform rectTransform = GetComponent<RectTransform>();
//            Vector2 localPoint = Vector2.zero;
//            switch (canvas.renderMode)
//            {
//                case RenderMode.ScreenSpaceOverlay:
//                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out localPoint);
//                    break;
//                case RenderMode.ScreenSpaceCamera:
//                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvas.worldCamera, out localPoint);
//                    break;
//                case RenderMode.WorldSpace:
//                    RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvas.worldCamera, out localPoint);
//                    break;
//                default:
//                    break;
//            }
//            return localPoint;
//        }

//    }
//}