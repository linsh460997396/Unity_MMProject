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
    /// 三维点结构体
    /// </summary>
    public struct Point3F
    {
        //全局变量，不加static则意味着每个结构实例都有自己的变量副本（否则共享）

        public float x;
        public float y;
        public float z;

        public Point3F(float newX, float newY, float newZ)
        {
            x = newX;
            y = newY;
            z = newZ;
        }

        public void Move(float dx, float dy, float dz)
        {
            x += dx;
            y += dy;
            z += dz;
        }

        public float DistanceTo(Point3F otherPoint)
        {
            float dx = otherPoint.x - x;
            float dy = otherPoint.y - y;
            float dz = otherPoint.z - z;
#if NETFRAMEWORK
            return (float)Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
#else
            return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
#endif
        }

        public static Point3F operator +(Point3F point1, Point3F point2)
        {
            return new Point3F(point1.x + point2.x, point1.y + point2.y, point1.z + point2.z);
        }

        public static Point3F operator -(Point3F point1, Point3F point2)
        {
            return new Point3F(point1.x - point2.x, point1.y - point2.y, point1.z - point2.z);
        }
    }
}

