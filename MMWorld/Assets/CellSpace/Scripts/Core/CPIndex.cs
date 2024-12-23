using UnityEngine;

// a Vector3F using ints instead of floats, for storing indexes and stuff

namespace CellSpace
{
    /// <summary>
    /// 索引（存储体素块在团块中的位置信息或团块在世界中的位置信息），可理解为参数为整数的坐标节点。
    /// 它是一种仅包含x、y和z整数值的数据结构，就像Vector的等效物但不代表绝对世界坐标，只是它用整型而不是浮点数（以避免精确转换发生错误）。
    /// 2D横版模式（HorizontalMode为真）时采用XY平面（禁用Z轴，Z值默认为0）
    /// </summary>
    public class CPIndex
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
        /// 用给定的x,y值创建一个新的索引
        /// </summary>
        /// <param name="setX"></param>
        /// <param name="setY"></param>
        public CPIndex(int setX, int setY)
        {
            this.x = setX;
            this.y = setY;
        }

        /// <summary>
        /// 用给定的向量创建一个新的索引
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
        /// 用给定的向量创建一个新的索引
        /// </summary>
        /// <param name="setIndex"></param>
        public CPIndex(Vector2 setIndex)
        {
            this.x = (int)setIndex.x;
            this.y = (int)setIndex.y;
        }

        /// <summary>
        /// 使用索引的x,pixelY,z值返回一个Vector3。
        /// </summary>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// 使用索引的x,y值返回一个Vector2。
        /// </summary>
        /// <returns></returns>
        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        /// <summary>
        /// 以" pixelX,pixelY,z "的形式返回索引字符串。
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
        /// 返回与给定方向上的index相邻的新索引。
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
        /// 索引比对
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
        /// 返回从“pixelX,pixelY,z”、“pixelX,pixelY”格式的字符串转换而来的新索引。如“5,2,0”返回(5,2,0)、“5,2”返回(5,2)。
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
                    //CellSpace:从字符串转换成索引用的格式无效，字符串必须是\"pixelX,pixelY\"格式
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
                    //CellSpace:从字符串转换成索引用的格式无效，字符串必须是\"pixelX,pixelY,z\"格式
                    Debug.LogError("CellSpace: CPIndex.FromString: Invalid format. String must be in \"pixelX,pixelY,z\" format.");
                    return null;
                }
            }
        }
    }
}
