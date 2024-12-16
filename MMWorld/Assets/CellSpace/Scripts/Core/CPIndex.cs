using UnityEngine;

// a Vector3F using ints instead of floats, for storing indexes and stuff

namespace CellSpace
{
    /// <summary>
    /// �������洢���ؿ����ſ��е�λ����Ϣ���ſ��������е�λ����Ϣ���������Ϊ����Ϊ����������ڵ㡣
    /// ����һ�ֽ�����x��y��z����ֵ�����ݽṹ������Vector�ĵ�Ч�ﵫ����������������ֻ꣬���������Ͷ����Ǹ��������Ա��⾫ȷת���������󣩡�
    /// 2D���ģʽ��HorizontalModeΪ�棩ʱ����XYƽ�棨����Z�ᣬZֵĬ��Ϊ0��
    /// </summary>
    public class CPIndex
    {
        /// <summary>
        /// �������ֶ�ֵ
        /// </summary>
        public int x, y, z;

        /// <summary>
        /// �ø�����x,pixelY,zֵ����һ���µ�����
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setZ"></param>
        public CPIndex(int setX, int setY, int setZ)
        {
            this.x = setX;
            this.y = setY;
            if (!CPEngine.HorizontalMode)
            {
                this.z = setZ;
            }
        }
        /// <summary>
        /// �ø�����x,yֵ����һ���µ�����
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        public CPIndex(int setX, int setY)
        {
            this.x = setX;
            this.y = setY;
        }

        /// <summary>
        /// �ø�������������һ���µ�����
        /// </summary>
        /// <param name="setIndex"></param>
        public CPIndex(Vector3 setIndex)
        {
            this.x = (int)setIndex.x;
            this.y = (int)setIndex.y;
            if (!CPEngine.HorizontalMode)
            {
                this.z = (int)setIndex.z;
            }
        }
        /// <summary>
        /// �ø�������������һ���µ�����
        /// </summary>
        /// <param name="setIndex"></param>
        public CPIndex(Vector2 setIndex)
        {
            this.x = (int)setIndex.x;
            this.y = (int)setIndex.y;
        }

        /// <summary>
        /// ʹ��������x,pixelY,zֵ����һ��Vector3��
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// ʹ��������x,yֵ����һ��Vector2��
        /// </summary>
        /// <returns></returns>
        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        /// <summary>
        /// ��" pixelX,pixelY,z "����ʽ���������ַ�����
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (x.ToString() + "," + y.ToString() + "," + z.ToString());
        }

        /// <summary>
        /// �����ȶ�
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool IsEqual(CPIndex to)
        {
            if (to == null)
            {
                return false;
            }
            if (CPEngine.HorizontalMode)
            {
                if (this.x == to.x && this.y == to.y)
                {
                    return true;
                }
                else return false;
            }
            else
            {
                if (this.x == to.x && this.y == to.y && this.z == to.z)
                {
                    return true;
                }
                else return false;
            }
        }

        /// <summary>
        /// ��������������ϵ�index���ڵ���������
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public CPIndex GetAdjacentIndex(Direction direction)
        {
            if (CPEngine.HorizontalMode)
            {
                if (direction == Direction.down) return new CPIndex(x, y - 1);
                else if (direction == Direction.up) return new CPIndex(x, y + 1);
                else if (direction == Direction.left) return new CPIndex(x - 1, y);
                else if (direction == Direction.right) return new CPIndex(x + 1, y);
                else return null;
            }
            else
            {
                if (direction == Direction.down) return new CPIndex(x, y - 1, z);
                else if (direction == Direction.up) return new CPIndex(x, y + 1, z);
                else if (direction == Direction.left) return new CPIndex(x - 1, y, z);
                else if (direction == Direction.right) return new CPIndex(x + 1, y, z);
                else if (direction == Direction.back) return new CPIndex(x, y, z - 1);
                else if (direction == Direction.forward) return new CPIndex(x, y, z + 1);
                else return null;
            }
        }

        /// <summary>
        /// �����ȶ�
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Compare(CPIndex a, CPIndex b)
        {
            if (b == null)
            {
                return false;
            }
            if (CPEngine.HorizontalMode)
            {
                if (a.x == b.x && a.y == b.y)
                {
                    return true;
                }
                else return false;
            }
            else
            {
                if (a.x == b.x && a.y == b.y && a.z == b.z)
                {
                    return true;
                }
                else return false;
            }
        }

        /// <summary>
        /// ���شӡ�pixelX,pixelY,z������pixelX,pixelY����ʽ���ַ���ת�����������������硰5,2,0������(5,2,0)����5,2������(5,2)��
        /// </summary>
        /// <param name="indexString"></param>
        /// <returns></returns>
        public static CPIndex FromString(string indexString)
        {
            string[] splitString = indexString.Split(',');
            if (CPEngine.HorizontalMode)
            {
                try
                {
                    return new CPIndex(int.Parse(splitString[0]), int.Parse(splitString[1]));
                }
                catch (System.Exception)
                {
                    //CellSpace:���ַ���ת���������õĸ�ʽ��Ч���ַ���������\"pixelX,pixelY\"��ʽ
                    Debug.LogError("CellSpace: CPIndex.FromString: Invalid format. String must be in \"pixelX,pixelY\" format.");
                    return null;
                }
            }
            else
            {
                try
                {
                    return new CPIndex(int.Parse(splitString[0]), int.Parse(splitString[1]), int.Parse(splitString[2]));
                }
                catch (System.Exception)
                {
                    //CellSpace:���ַ���ת���������õĸ�ʽ��Ч���ַ���������\"pixelX,pixelY,z\"��ʽ
                    Debug.LogError("CellSpace: CPIndex.FromString: Invalid format. String must be in \"pixelX,pixelY,z\" format.");
                    return null;
                }
            }
        }
    }
}
