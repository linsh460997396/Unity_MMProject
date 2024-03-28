using System;
using UnityEngine;


namespace UnityStandardAssets.Utility
{
    //���߶���������
    [Serializable]
    public class CurveControlledBob
    {
        public float HorizontalBobRange = 0.33f;
        public float VerticalBobRange = 0.33f;

        // AnimationCurve �� Unity ���������ڱ�ʾ��ʱ��仯�����ߵ�һ���ࡣ��ͨ�����ڶ���ϵͳ���������ߴ������ӵĲ�ֵ���ߣ��Կ��Ƹ������Եı仯����λ�á���ת�����ŵȡ�
        // �����ṩ�Ĵ����У�Bobcurve ��һ�� AnimationCurve ����������ʼ��Ϊһ����ʾ��ͷ�������������Ҳ����ߡ�������˵��������߶�����ʱ�䣨x�ᣩ��ͷ�������ķ��ȣ�y�ᣩ֮��Ĺ�ϵ��
        // ����������߹ؼ�֡�Ľ��ͣ�
        // new Keyframe(0f, 0f): ��ʱ�� 0 ��ʱ�������ķ����� 0����û�ж�������
        // new Keyframe(0.5f, 1f): ��ʱ�� 0.5 ��ʱ�������ķ��ȴﵽ���ֵ 1��
        // new Keyframe(1f, 0f): ��ʱ�� 1 ��ʱ�������ķ��Ȼص� 0��
        // new Keyframe(1.5f, -1f): ��ʱ�� 1.5 ��ʱ�������ķ��ȴﵽ��Сֵ -1�������¶�������
        // new Keyframe(2f, 0f): ��ʱ�� 2 ��ʱ�������ķ����ٴλص� 0��
        // ������ߴ�����һ�������ԵĶ���Ч�������������Ҳ�������Ϸ�У��������߿������ڿ��ƽ�ɫͷ�������¶����������ӽ�ɫ���߻��ܲ�ʱ����ʵ�С�
        // ��������߱������������ߵ�����ʱ�������Ը���ʱ�����һ��ֵ�����ֵ���Ա�����������ɫ��ͷ��λ�ã�ʵ�ֶ���Ч����
        // �����֮��Bobcurve �������Ƕ�����һ����ʾͷ�����������Ҳ����ߣ���������Ϸ��Ϊ��ɫ��Ӹ�����ʵ�Ķ������֡�
        public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                            new Keyframe(2f, 0f)); // sin curve for head bob.����bob��Sin����
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

            // get the length of the curve in time.��ʱ�õ����ߵĳ���
            m_Time = Bobcurve[Bobcurve.length - 1].time;
        }


        public Vector3 DoHeadBob(float speed)
        {
            float xPos = m_OriginalCameraPosition.x + (Bobcurve.Evaluate(m_CyclePositionX)*HorizontalBobRange);
            float yPos = m_OriginalCameraPosition.y + (Bobcurve.Evaluate(m_CyclePositionY)*VerticalBobRange);

            m_CyclePositionX += (speed*Time.deltaTime)/m_BobBaseInterval;
            m_CyclePositionY += ((speed*Time.deltaTime)/m_BobBaseInterval)*VerticaltoHorizontalRatio;

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
