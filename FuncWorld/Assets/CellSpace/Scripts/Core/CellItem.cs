namespace CellSpace
{
    /// <summary>
    /// 单元体(或称单元项)基类.是团块空间中双向链表管理的逻辑节点对象.
    /// 用于绑定框架静态网格团地面上那些活动个体网格对象(如2D精灵),所以可视为一种逻辑上的网格容器.
    /// 在角色、怪物类继承基类后,再造一继承单元体的新类带角色、怪物类型字段即可,其实例化后的对象绑定角色、怪物并支持在团块(空间)中通过检索等方法管理它们所属的单元体网格容器.
    /// 团块空间通过双向链表管理单元体对象就相当于在管理角色、怪物们.基类不会明确要管理对象的类型,而是通过继承后再添加类型字段来实现.
    /// </summary>
    public class CellItem
    {
        /// <summary>
        /// 所属空间容器(存储单元体、划分单元格的团块)
        /// </summary>
        public CellChunk chunk;
        /// <summary>
        /// 链表邻居:前驱节点对应的网格容器(单元体)
        /// </summary>
        public CellItem nodePrev;
        /// <summary>
        /// 链表邻居:后继节点对应的网格容器(单元体)
        /// </summary>
        public CellItem nodeNext;
        /// <summary>
        /// 在所属空间容器中的索引,未分配时默认值为-1(有效索引从0起),物体跨越团块时以新团块为父级容器则索引重新计算
        /// </summary>
        public int index = -1;
        /// <summary>
        /// 在父级容器中的相对坐标(世界坐标与本地相对坐标的转换由transform.InverseTransformPoint(世界坐标)方法解决,Unity默认坐标系以左下为原点,所以这个方法正好可以直接用于获取相对位置).
        /// </summary>
        public float x, y, z;
        /// <summary>
        /// 半径范围大小.默认=空间容器下的单元格大小/2,影响缩放和自定义体积碰撞(若有)
        /// </summary>
        public float radius;
    }
}
