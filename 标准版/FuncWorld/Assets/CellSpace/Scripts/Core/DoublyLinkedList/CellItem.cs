namespace CellSpace
{
    /// <summary>
    /// 单元体基类.是团块空间中双向链表管理的节点对象.
    /// 给怪物、子弹、特效等多种单位类型继承此基类,此时CellItem可视为一种代表它们的逻辑网格容器.
    /// 最终通过双向链表管理单元体在团块空间的状态和位置.
    /// </summary>
    public class CellItem
    {
        /// <summary>
        /// 双向链表中用于验证单元体所属团块空间的字段.该字段让单位体所属空间唯一,避免不必要的链表操作.
        /// </summary>
        public CellGridContainer cellGridContainer;
        /// <summary>
        /// 链表邻居:前驱节点对应的网格容器(单元体)
        /// </summary>
        public CellItem nodePrev;
        /// <summary>
        /// 链表邻居:后继节点对应的网格容器(单元体)
        /// </summary>
        public CellItem nodeNext;
        /// <summary>
        /// 在所属团块空间中的个体索引,未分配时默认值为-1(有效索引从0起),物体跨越团块时以新团块为父级容器则索引重新计算.
        /// </summary>
        public int index = -1;
        /// <summary>
        /// 在所属团块空间中的相对坐标(以左下为原点).
        /// 世界坐标与本地相对坐标的转换由transform.InverseTransformPoint(世界坐标)解决,Unity默认坐标系以左下为原点,所以该方法正好可以直接用于获取相对位置.
        /// 精灵实际世界位置(256.5,256.5,256.5),团块空间边长256(左下插入点0,0,0),那么经transform.InverseTransformPoint转成相对坐标(0.5,0.5,0.5),即双向链表第一个单元索引0
        /// </summary>
        public float x, y, z;
        /// <summary>
        /// 半径范围大小.默认=团块空间单元格大小/2,影响缩放和自定义体积碰撞(若有)
        /// </summary>
        public float radius;
    }
}
