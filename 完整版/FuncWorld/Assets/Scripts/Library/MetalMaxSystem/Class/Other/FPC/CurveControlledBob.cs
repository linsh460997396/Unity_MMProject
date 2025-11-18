#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    //曲线抖动控制器
    [Serializable]
    public class CurveControlledBob
    {
        public float HorizontalBobRange = 0.33f;
        public float VerticalBobRange = 0.33f;

        // AnimationCurve 是 Unity 引擎中用于表示随时间变化的曲线的一个类.它通常用于动画系统,允许开发者创建复杂的插值曲线,以控制各种属性的变化,如位置、旋转、缩放等.
        // 在你提供的代码中,Bobcurve 是一个 AnimationCurve 对象,它被初始化为一个表示“头部抖动”的正弦波曲线.具体来说,这个曲线定义了时间（x轴)与头部抖动的幅度（y轴)之间的关系.
        // 下面这个曲线关键帧的解释：
        // new Keyframe(0f, 0f): 在时间 0 秒时,抖动的幅度是 0（即没有抖动).
        // new Keyframe(0.5f, 1f): 在时间 0.5 秒时,抖动的幅度达到最大值 1.
        // new Keyframe(1f, 0f): 在时间 1 秒时,抖动的幅度回到 0.
        // new Keyframe(1.5f, -1f): 在时间 1.5 秒时,抖动的幅度达到最小值 -1（即向下抖动).
        // new Keyframe(2f, 0f): 在时间 2 秒时,抖动的幅度再次回到 0.
        // 这个曲线创建了一个周期性的抖动效果,类似于正弦波.在游戏中,这种曲线可以用于控制角色头部的上下抖动,以增加角色行走或跑步时的真实感.
        // 当这个曲线被用作动画曲线的输入时,它可以根据时间产生一个值,这个值可以被用来调整角色的头部位置,实现抖动效果.
        // 简而言之,Bobcurve 的作用是定义了一个表示头部抖动的正弦波曲线,用于在游戏中为角色添加更加真实的动作表现.
        public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                            new Keyframe(2f, 0f)); // sin curve for head bob.正面bob的Sin曲线
        /// <summary>
        /// vertical to horizontal speed ratio.垂直到水平速度比.
        /// </summary>
        public float VerticaltoHorizontalRatio = 1f;

        private float m_CyclePositionX;
        private float m_CyclePositionY;
        private float m_BobBaseInterval;
        private Vector3 m_OriginalCameraPosition;
        private float m_Time;


        public void Setup(Camera camera, float bobBaseInterval)
        {
            m_BobBaseInterval = bobBaseInterval;
            m_OriginalCameraPosition = camera.transform.localPosition;

            // get the length of the curve in time.及时得到曲线的长度
            m_Time = Bobcurve[Bobcurve.length - 1].time;
        }


        public Vector3 DoHeadBob(float speed)
        {
            float xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX) * HorizontalBobRange);
            float yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY) * VerticalBobRange);

            m_CyclePositionX += (speed * Time.deltaTime) / m_BobBaseInterval;
            m_CyclePositionY += ((speed * Time.deltaTime) / m_BobBaseInterval) * VerticaltoHorizontalRatio;

            if (m_CyclePositionX > m_Time)
            {
                m_CyclePositionX = m_CyclePositionX - m_Time;
            }
            if (m_CyclePositionY > m_Time)
            {
                m_CyclePositionY = m_CyclePositionY - m_Time;
            }

            return new Vector3(xPos, yPos, 0f);
        }
    }
}
#endif