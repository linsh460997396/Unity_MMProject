//using UnityEngine;

//namespace Test.Example
//{
//    /// <summary>
//    /// DrawLine3DMesh
//    /// </summary>
//    public class TE_DrawLine3D : MonoBehaviour
//    {
//        public Transform handle;//����������ֱ�
//        public Material mat;
//        private bool isDraw;//��ǵ�ǰ�Ƿ�������
//        private LineRenderer line;
//        private Vector3 lastPos;//��¼һ����һ���ֱ���λ��:

//        void Update()
//        {
//            //�����ֱ��������ÿ�ΰ��µ�ʱ�򶼻ᴴ��һ���µ��߶�
//            if (Input.GetKeyDown(KeyCode.A) && !isDraw)
//            {
//                isDraw = true;
//                //����һ�������壬�������LineRenderer���
//                line = new GameObject().AddComponent<LineRenderer>();
//                line.positionCount = 0;//���õ������Ϊ0����Ϊ�տ�ʼ��ʱ��Ĭ���������������
//                line.startWidth = 0.05f;//���������Ŀ��
//                line.endWidth = 0.05f;
//                line.material = mat;//���������Ĳ���
//                lastPos = handle.position;//��¼��ǰ֡�ֱ���λ��
//            }

//            if (isDraw)
//            {
//                //�ж���֡�ֱ��ľ��룬��������������ӣ���Ȼ�ᵼ�µ����������
//                if (Vector3.Distance(lastPos, handle.position) > 0.03f)
//                {
//                    RealTimeDrawLine(line, handle.position);
//                    lastPos = handle.position;
//                }
//            }

//            if (Input.GetKeyDown(KeyCode.S) && isDraw) { isDraw = false; }

//        }

//        /// <summary>
//        /// ʵʱ��׷�ӻ���
//        /// </summary>
//        /// <param name="lineRenderer"></param>
//        /// <param name="newPoint"></param>
//        public void RealTimeDrawLine(LineRenderer lineRenderer, Vector3 newPoint)
//        {
//            lineRenderer.positionCount++;
//            lineRenderer.SetPosition(lineRenderer.positionCount - 1, newPoint);
//        }
//    }
//}