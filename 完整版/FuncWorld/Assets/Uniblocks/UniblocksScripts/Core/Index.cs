using UnityEngine;

// a Vector3F using ints instead of floats, for storing indexes and stuff

namespace Uniblocks
{
    /// <summary>
    /// 索引(存储体素块在团块中的位置信息或团块在世界中的位置信息),可理解为参数为整数的坐标节点.
    /// 它是一种仅包含x、y和z整数值的数据结构,就像Vector3的等效物,只是它用整型而不是浮点数(以避免精确转换发生错误).
    /// </summary>
    public class Index
    {
        /// <summary>
        /// 索引的字段值
        /// </summary>
        public int x, y, z;

        /// <summary>
        /// 用给定的x,pixelY,z值创建一个新的索引
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
        /// 用给定的向量创建一个新的索引
        /// </summary>
        /// <param name="setIndex"></param>
        public Index(Vector3 setIndex)
        {
            this.x = (int)setIndex.x;
            this.y = (int)setIndex.y;
            this.z = (int)setIndex.z;
        }
        /// <summary>
        /// 使用索引的x,pixelY,z值返回一个Vector3.
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// 以" pixelX,pixelY,z "的形式返回索引字符串.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (x.ToString() + "," + y.ToString() + "," + z.ToString());

        }
        /// <summary>
        /// 索引比对
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
        /// 返回与给定方向上的index相邻的新索引.
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
        /// 索引比对
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
        /// 返回从“pixelX,pixelY,z”格式的字符串转换而来的新索引.例如“5,0,-5”将返回一个新的index(5,0,-5).
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
                //Uniblocks:从字符串转换成索引用的格式无效,字符串必须是\"pixelX,pixelY,z\"格式
                Debug.LogError("Uniblocks: Index.FromString: Invalid format. String must be in \"pixelX,pixelY,z\" format.");
                return null;
            }

        }

    }

}
