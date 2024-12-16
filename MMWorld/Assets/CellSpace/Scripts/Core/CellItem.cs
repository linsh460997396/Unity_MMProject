namespace CellSpace
{
    /// <summary>
    /// 单元体，是单元（2D单元格或3D体素块）内的活动物体，建立角色或怪物类来继承，实例化后的对象可在单元团块类下调用检索等方法进行管理
    /// </summary>
    public class CellItem
    {
        /// <summary>
        /// 所属空间容器（存储单元体、划分单元格的团块）
        /// </summary>
        public CellChunk chunk;
        /// <summary>
        /// 链表邻居：前驱节点对应的网格容器（单元体）
        /// </summary>
        public CellItem nodePrev;
        /// <summary>
        /// 链表邻居：后继节点对应的网格容器（单元体）
        /// </summary>
        public CellItem nodeNext;
        /// <summary>
        /// 在所属空间容器中的索引，未分配时默认值为-1（有效索引从0起），物体跨越团块时以新团块为父级容器则索引重新计算
        /// </summary>
        public int index = -1;
        /// <summary>
        /// 在父级容器中的相对位置（由transform.InverseTransformPoint(世界坐标)获取，Unity默认坐标系以左下为原点，所以这个方法正好可以直接用于获取相对位置）。
        /// </summary>
        public float x, y, z;
        /// <summary>
        /// 半径范围大小。默认=空间容器下的单元体格大小/2，影响缩放和自定义体积碰撞（若有）
        /// </summary>
        public float radius;
    }
}
