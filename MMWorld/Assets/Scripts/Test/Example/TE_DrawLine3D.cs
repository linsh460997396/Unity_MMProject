//using UnityEngine;

//namespace Test.Example
//{
//    /// <summary>
//    /// DrawLine3DMesh
//    /// </summary>
//    public class TE_DrawLine3D : MonoBehaviour
//    {
//        public Transform handle;//加入这个是手柄
//        public Material mat;
//        private bool isDraw;//标记当前是否正画线
//        private LineRenderer line;
//        private Vector3 lastPos;//记录一下上一顿手柄的位置:

//        void Update()
//        {
//            //按下手柄扳机键，每次按下的时候都会创建一个新的线段
//            if (Input.GetKeyDown(KeyCode.A) && !isDraw)
//            {
//                isDraw = true;
//                //创建一个空物体，并且添加LineRenderer组件
//                line = new GameObject().AddComponent<LineRenderer>();
//                line.positionCount = 0;//设置点的数量为0，因为刚开始的时候默认里面会有两个点
//                line.startWidth = 0.05f;//设置线条的宽高
//                line.endWidth = 0.05f;
//                line.material = mat;//设置线条的材质
//                lastPos = handle.position;//记录当前帧手柄的位置
//            }

//            if (isDraw)
//            {
//                //判断两帧手柄的距离，超过三厘米再添加，不然会导致点的数量剧增
//                if (Vector3.Distance(lastPos, handle.position) > 0.03f)
//                {
//                    RealTimeDrawLine(line, handle.position);
//                    lastPos = handle.position;
//                }
//            }

//            if (Input.GetKeyDown(KeyCode.S) && isDraw) { isDraw = false; }

//        }

//        /// <summary>
//        /// 实时的追加划线
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