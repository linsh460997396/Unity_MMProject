using System;

namespace CellSpace
{
    /// <summary>
    /// 2D空间容器内的单元距离结构体，可以进行距离的比较
    /// </summary>
    public struct CellDistanceInfo : IComparable<CellDistanceInfo>
    {
        public float distance;
        public CellItem cell;

        public int CompareTo(CellDistanceInfo distanceCell)
        {
            return this.distance.CompareTo(distanceCell.distance);
        }
    }
}
