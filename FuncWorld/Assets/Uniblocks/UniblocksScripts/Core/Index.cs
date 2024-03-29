using UnityEngine;

// a Vector3F using ints instead of floats, for storing indexes and stuff

namespace Uniblocks
{
    /// <summary>
    /// �������洢���ؿ����ſ��е�λ����Ϣ���ſ��������е�λ����Ϣ���������Ϊ����Ϊ����������ڵ㡣
    /// ����һ�ֽ�����x��y��z����ֵ�����ݽṹ������Vector3�ĵ�Ч�ֻ���������Ͷ����Ǹ��������Ա��⾫ȷת���������󣩡�
    /// </summary>
    public class Index
    {
        /// <summary>
        /// �������ֶ�ֵ
        /// </summary>
        public int x, y, z;

        /// <summary>
        /// �ø�����x,y,zֵ����һ���µ�����
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        /// <param name="setZ"></param>
        public Index(int setX, int setY, int setZ)
        {
            this.x = setX;
            this.y = setY;
            this.z = setZ;
        }

        /// <summary>
        /// �ø�������������һ���µ�����
        /// </summary>
        /// <param name="setIndex"></param>
        public Index(Vector3 setIndex)
        {
            this.x = (int)setIndex.x;
            this.y = (int)setIndex.y;
            this.z = (int)setIndex.z;
        }
        /// <summary>
        /// ʹ��������x,y,zֵ����һ��Vector3��
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// ��" x,y,z "����ʽ���������ַ�����
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
        public bool IsEqual(Index to)
        {

            if (to == null)
            {
                return false;
            }

            if (this.x == to.x &&
                this.y == to.y &&
                this.z == to.z)
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// ��������������ϵ�index���ڵ���������
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Index GetAdjacentIndex(Direction direction)
        {
            if (direction == Direction.down) return new Index(x, y - 1, z);
            else if (direction == Direction.up) return new Index(x, y + 1, z);
            else if (direction == Direction.left) return new Index(x - 1, y, z);
            else if (direction == Direction.right) return new Index(x + 1, y, z);
            else if (direction == Direction.back) return new Index(x, y, z - 1);
            else if (direction == Direction.forward) return new Index(x, y, z + 1);
            else return null;
        }
        /// <summary>
        /// �����ȶ�
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Compare(Index a, Index b)
        {

            if (b == null)
            {
                return false;
            }

            if (a.x == b.x &&
                a.y == b.y &&
                a.z == b.z)
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// ���شӡ�x,y,z����ʽ���ַ���ת�������������������硰5,0,-5��������һ���µ�index(5,0,-5)��
        /// </summary>
        /// <param name="indexString"></param>
        /// <returns></returns>
        public static Index FromString(string indexString)
        {

            string[] splitString = indexString.Split(',');

            try
            {
                return new Index(int.Parse(splitString[0]), int.Parse(splitString[1]), int.Parse(splitString[2]));
            }
            catch (System.Exception)
            {
                //Uniblocks:���ַ���ת���������õĸ�ʽ��Ч���ַ���������\"x,y,z\"��ʽ
                Debug.LogError("Uniblocks: Index.FromString: Invalid format. String must be in \"x,y,z\" format.");
                return null;
            }

        }

    }

}
