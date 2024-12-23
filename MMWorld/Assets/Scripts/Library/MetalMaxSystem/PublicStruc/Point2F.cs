#define NETFRAMEWORK

#if UNITY_EDITOR|| UNITY_STANDALONE
using Mathf = UnityEngine.Mathf;
#else
#if NETFRAMEWORK
using Mathf = System.Math;
#else
using Mathf = System.MathF;
#endif
#endif

namespace MetalMaxSystem
{
    //结构是值类型

    /// <summary>
    /// 二维点结构体
    /// </summary>
    public struct Point2F
    {
        //全局变量，不加static则意味着每个结构实例都有自己的变量副本（否则共享）

        public float x;
        public float y;

        public Point2F(float newX, float newY)
        {
            x = newX;
            y = newY;
        }

        public void Move(float dx, float dy)
        {
            x += dx;
            y += dy;
        }

        public float DistanceTo(Point2F otherPoint)
        {
            float dx = otherPoint.x - x;
            float dy = otherPoint.y - y;
#if NETFRAMEWORK
            return (float)Mathf.Sqrt(dx * dx + dy * dy);
#else
            return Mathf.Sqrt(dx * dx + dy * dy);
#endif
        }

        public static Point2F operator +(Point2F point1, Point2F point2)
        {
            return new Point2F(point1.x + point2.x, point1.y + point2.y);
        }

        public static Point2F operator -(Point2F point1, Point2F point2)
        {
            return new Point2F(point1.x - point2.x, point1.y - point2.y);
        }
    }
}

