using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 空间物体（一种网格容器）
    /// </summary>
    public class SpaceItem
    {
        /// <summary>
        /// 所属空间容器
        /// </summary>
        public SpaceContainer spaceContainer;
        /// <summary>
        /// 链表邻居：前驱节点对应的网格容器
        /// </summary>
        public SpaceItem spacePrev;
        /// <summary>
        /// 链表邻居：后继节点对应的网格容器
        /// </summary>
        public SpaceItem spaceNext;
        /// <summary>
        /// 在所属空间容器中的索引
        /// </summary>
        public int spaceIndex = -1;
        /// <summary>
        /// 逻辑坐标
        /// </summary>
        public float pixelX, pixelY;
        /// <summary>
        /// [逻辑坐标]半径范围
        /// </summary>
        public float radius;
    }

    /// <summary>
    /// 空间容器（双向链表）
    /// </summary>
    public class SpaceContainer
    {
        /// <summary>
        /// 空间内网格行数
        /// </summary>
        public int gridNumRows;
        /// <summary>
        /// 空间内网格列数
        /// </summary>
        public int gridNumCols;
        /// <summary>
        /// 空间内网格尺寸
        /// </summary>
        public float gridSize;
        /// <summary>
        /// 空间内网格尺寸的倒数（_1_gridSize = 1 / gridSize）
        /// </summary>
        public float _1_gridSize;
        /// <summary>
        /// 空间最大边界尺寸（网格尺寸*各方向网格数量）
        /// </summary>
        public float gridChunkY, gridChunkX;
        /// <summary>
        /// 空间内物体数量（尽管没固定最大容量，但可用于状态检查物体是否为空或已满）
        /// </summary>
        public int numItems;
        /// <summary>
        /// 存放空间物体的数组(容量gridChunkNumRows * gridChunkNumCols)，是个链表池（其元素节点的字段中存着前驱节点和后驱节点）
        /// </summary>
        public SpaceItem[] Items;

        //双向链表和链表池的运用主要提升数据结构灵活性（操作效率）和内存管理的性能
        //‌双向链表‌：每个节点包含两个指针，分别指向前驱和后继节点，这使得从任意节点出发都能方便地访问前驱和后继节点，提高了操作的灵活性。双向链表适用于需要频繁进行前后遍历、插入和删除操作的场景，如各种不需要排序的数据列表管理‌
        //‌链表池‌：通过维护一个空闲节点池来减少内存分配和释放的次数，提高内存使用效率。在链表频繁进行插入和删除操作时，链表池能够复用已删除的节点，避免内存碎片的产生，从而提升性能。链表池的实现通常涉及节点池的初始化、节点的分配与回收等步骤‌。

        /// <summary>
        /// [构造函数]空间容器
        /// </summary>
        /// <param name="numRows_">行</param>
        /// <param name="numCols_">列</param>
        /// <param name="gridSize">网格尺寸</param>
        public SpaceContainer(int numRows_, int numCols_, float gridSize)
        {
#if UNITY_EDITOR
            //条件失败时进行断言
            Debug.Assert(numRows_ > 0, "numRows_ must be greater than 0.");
            Debug.Assert(numCols_ > 0, "numCols_ must be greater than 0.");
            Debug.Assert(gridSize > 0, "gridSize must be greater than 0.");
#endif
            gridNumRows = numRows_;
            gridNumCols = numCols_;
            this.gridSize = gridSize;
            _1_gridSize = 1f / gridSize; //如果网格容器尺寸16就是0.625
                                         //空间内总尺寸
            gridChunkY = this.gridSize * gridNumRows;
            gridChunkX = this.gridSize * gridNumCols;
            if (Items == null)
            {//如果物体数组为空
                Items = new SpaceItem[gridNumRows * gridNumCols]; //创建新的空间物体数组
            }
            else
            {
                // 使用null填充数组Items，这通常是在构造函数中初始化数组时使用的操作。
                // 假设Items已经被声明为一个数组变量，这一步将确保数组中的每个元素都被设置为null
                Array.Fill(Items, null); //物体数组充满

                // 重新调整数组Items的大小，新的大小为gridNumRows * gridChunkNumCols，并将调整后的数组重新赋值给Items。
                // 这一步可能会创建一个新的数组，如果原数组大小与新的大小不同，原数组的内容将被复制到新数组中（或部分复制，取决于大小变化）。
                // 如果新大小大于原大小，新元素将被设置为默认值（对于引用类型是null，对于值类型是零或相应的默认值）。
                Array.Resize(ref Items, gridNumRows * gridNumCols);
            }
        }

        /// <summary>
        /// 为空间物体数组（双向链表）添加节点物体
        /// </summary>
        /// <param name="c">空间物体</param>
        public void Add(SpaceItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.spaceContainer == this);
            Debug.Assert(c.spaceIndex == -1);
            Debug.Assert(c.spacePrev == null);
            Debug.Assert(c.spaceNext == null);
            Debug.Assert(c.pixelX >= 0 && c.pixelX < gridChunkX);
            Debug.Assert(c.pixelY >= 0 && c.pixelY < gridChunkY);
#endif

            //从逻辑坐标返回物体在空间容器内的索引
            var idx = PosToIndex(c.pixelX, c.pixelY); // calc rIdx & cIdx
#if UNITY_EDITOR
            Debug.Assert(Items[idx] == null || Items[idx].spacePrev == null);
#endif

            //进行Link
            if (Items[idx] != null)
            {
                //如果空间索引对应物体存在，将新物体作为该物体的前驱节点
                Items[idx].spacePrev = c;
            }
            //将空间索引对应物体作为新物体的后驱节点（可为null）
            c.spaceNext = Items[idx];
            c.spaceIndex = idx; //刷新新物体的空间索引为idx
            Items[idx] = c; //新单元作为空间索引对应单元
                            //如果只有1个节点，作为头部节点呈现：【Prev=null】【C】【Next=null】
#if UNITY_EDITOR
            Debug.Assert(Items[idx].spacePrev == null);
            Debug.Assert(c.spaceNext != c);
            Debug.Assert(c.spacePrev != c);
#endif
            //空间容器中物体数量自增
            ++numItems;
        }

        /// <summary>
        /// 从空间容器中移除物体
        /// </summary>
        /// <param name="c">空间物体</param>
        public void Remove(SpaceItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.spaceContainer == this);
            Debug.Assert(c.spacePrev == null && Items[c.spaceIndex] == c || c.spacePrev.spaceNext == c && Items[c.spaceIndex] != c);
            Debug.Assert(c.spaceNext == null || c.spaceNext.spacePrev == c);
            //Debug.Assert(Items[c.index] include c);
#endif

            //unlink
            if (c.spacePrev != null)
            {  //如果目标物体有前驱节点（说明它不是头部节点）
#if UNITY_EDITOR
                Debug.Assert(Items[c.spaceIndex] != c);
#endif
                //将目标物体前驱节点（对应物体）的后驱节点更换为目标物体的后驱节点（目标物体被移除，所以前后节点相连）
                c.spacePrev.spaceNext = c.spaceNext;
                if (c.spaceNext != null)
                {
                    //如果目标物体的后驱节点不为空（不是最后一个），将后驱节点的前驱节点设置为要移除目标物体的前驱节点（目标物体被移除，所以前后节点相连）
                    c.spaceNext.spacePrev = c.spacePrev;
                    c.spaceNext = null; //清空要删除的目标物体的后驱节点
                }
                c.spacePrev = null; //清空要删除的目标物体的前驱节点
            }
            else
            {
                //如果目标物体无前驱节点（说明它是头部节点）
#if UNITY_EDITOR
                Debug.Assert(Items[c.spaceIndex] == c);
#endif
                //目标位置的空间物体被后驱节点替换（目标物体作为头部节点被移除，所以后驱节点占位）
                Items[c.spaceIndex] = c.spaceNext;
                if (c.spaceNext != null)
                {
                    //如果目标物体的后驱节点不为null，该后驱节点的前驱节点设置为null（后驱节点作为头部节点了）
                    c.spaceNext.spacePrev = null;
                    c.spaceNext = null; //清空要删除的目标物体的后驱节点
                }
            }
#if UNITY_EDITOR
            Debug.Assert(Items[c.spaceIndex] != c);
#endif
            c.spaceIndex = -1; //初始化目标物体的空间索引
            c.spaceContainer = null; //清空目标物体的空间容器

            //空间容器中物体数量自减
            --numItems;
        }

        /// <summary>
        /// 更新一个SpaceItem对象在空间容器中的位置
        /// </summary>
        /// <param name="c">空间物体</param>
        public void Update(SpaceItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.spaceContainer == this);
            Debug.Assert(c.spaceIndex > -1);
            Debug.Assert(c.spaceNext != c);
            Debug.Assert(c.spacePrev != c);
            //Debug.Assert(Items[c.index] include c);
#endif

            var x = c.pixelX;
            var y = c.pixelY;
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < gridChunkX);
            Debug.Assert(y >= 0 && y < gridChunkY);
#endif
            //逻辑坐标转本地坐标
            int cIdx = (int)(x * _1_gridSize);
            int rIdx = (int)(y * _1_gridSize);
            int idx = rIdx * gridNumCols + cIdx;//逻辑网格索引
#if UNITY_EDITOR
            Debug.Assert(idx <= Items.Length);
#endif

            if (idx == c.spaceIndex) return;  //无改变，直接返回

            //unlink
            if (c.spacePrev != null)
            {//非头部节点
#if UNITY_EDITOR
                Debug.Assert(Items[c.spaceIndex] != c);
#endif
                c.spacePrev.spaceNext = c.spaceNext;
                if (c.spaceNext != null)
                {
                    c.spaceNext.spacePrev = c.spacePrev;
                    //c.nodeNext = {};
                }
                //c.nodePrev = {};
            }
            else
            {
#if UNITY_EDITOR
                Debug.Assert(Items[c.spaceIndex] == c);
#endif
                Items[c.spaceIndex] = c.spaceNext;
                if (c.spaceNext != null)
                {
                    c.spaceNext.spacePrev = null;
                    //c.nodeNext = {};
                }
            }
            //c.index = -1;
#if UNITY_EDITOR
            Debug.Assert(Items[c.spaceIndex] != c);
            Debug.Assert(idx != c.spaceIndex);
#endif

            //link
            if (Items[idx] != null)
            {
                Items[idx].spacePrev = c;
            }
            c.spacePrev = null;
            c.spaceNext = Items[idx];
            Items[idx] = c;
            c.spaceIndex = idx;
#if UNITY_EDITOR
            Debug.Assert(Items[idx].spacePrev == null);
            Debug.Assert(c.spaceNext != c);
            Debug.Assert(c.spacePrev != c);
#endif
        }

        /// <summary>
        /// 返回物体的逻辑坐标在空间容器中的索引
        /// </summary>
        /// <param name="x">逻辑坐标</param>
        /// <param name="y">逻辑坐标</param>
        /// <returns>return Items index</returns>
        public int PosToIndex(float x, float y)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < gridChunkX);
            Debug.Assert(y >= 0 && y < gridChunkY);
#endif
            int cIdx = (int)(x * _1_gridSize); //直接取整
            int rIdx = (int)(y * _1_gridSize);
            int idx = rIdx * gridNumCols + cIdx;
#if UNITY_EDITOR
            Debug.Assert(idx <= Items.Length);
#endif
            return idx;
        }

        /// <summary>
        /// 在逻辑坐标划分的9宫格内找出第1个相交物并返回
        /// </summary>
        /// <param name="x">逻辑坐标</param>
        /// <param name="y">逻辑坐标</param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public SpaceItem FindFirstCrossBy9(float x, float y, float radius)
        {
            // 5
            int cIdx = (int)(x * _1_gridSize);
            if (cIdx < 0 || cIdx >= gridNumCols) return null;
            int rIdx = (int)(y * _1_gridSize);
            if (rIdx < 0 || rIdx >= gridNumRows) return null;
            int idx = rIdx * gridNumCols + cIdx;
            var c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 6
            ++cIdx;
            if (cIdx >= gridNumCols) return null;
            ++idx;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 3
            ++rIdx;
            if (rIdx >= gridNumRows) return null;
            idx += gridNumCols;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 2
            --idx;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return null;
            --idx;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 4
            idx -= gridNumCols;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return null;
            idx -= gridNumCols;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 8
            ++idx;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 9
            ++idx;
            c = Items[idx];
            while (c != null)
            {
                var vx = c.pixelX - x;
                var vy = c.pixelY - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            return null;
        }

        /// <summary>
        /// 遍历逻辑坐标所在格+周围逻辑九宫格的物体
        /// </summary>
        /// <param name="x">逻辑坐标</param>
        /// <param name="y">逻辑坐标</param>
        /// <param name="handler">返回true结束遍历(Func可能产生gc，但这种应该是无所谓的，里面只要不含Unity引擎对象)</param>
        public void Foreach9All(float x, float y, Func<SpaceItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * _1_gridSize);
            if (cIdx < 0 || cIdx >= gridNumCols) return;
            int rIdx = (int)(y * _1_gridSize);
            if (rIdx < 0 || rIdx >= gridNumRows) return;
            int idx = rIdx * gridNumCols + cIdx;
            var c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= gridNumCols) return;
            ++idx;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= gridNumRows) return;
            idx += gridNumCols;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return;
            --idx;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= gridNumCols;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= gridNumCols;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = Items[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
        }

        /// <summary>
        /// 圆形扩散遍历找出边距最近的1个物体并返回
        /// </summary>
        /// <param name="d">SpaceRingDiffuseData对象</param>
        /// <param name="x">逻辑坐标</param>
        /// <param name="y">逻辑坐标</param>
        /// <param name="maxDistance">限制结果集的最大边距（逻辑坐标）</param>
        /// <returns></returns>
        public SpaceItem FindNearestByRange(SpaceRingDiffuseData d, float x, float y, float maxDistance)
        {
            int cIdxBase = (int)(x * _1_gridSize);
            if (cIdxBase < 0 || cIdxBase >= gridNumCols) return null;
            int rIdxBase = (int)(y * _1_gridSize);
            if (rIdxBase < 0 || rIdxBase >= gridNumRows) return null;
            var searchRange = maxDistance + gridSize;

            SpaceItem rtv = null;
            float maxV = 0;

            var lens = d.lens;
            var idxs = d.idxs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= gridNumCols) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= gridNumRows) continue;
                    var cidx = rIdx * gridNumCols + cIdx;

                    var c = Items[cidx];
                    while (c != null)
                    {
                        var vx = c.pixelX - x;
                        var vy = c.pixelY - y;
                        var dd = vx * vx + vy * vy;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;


                        if (v > maxV)
                        {
                            rtv = c;
                            maxV = v;
                        }
                        c = c.spaceNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return rtv;
        }

        /// <summary>
        /// 圆形扩散遍历找范围内最多n个物体的结果存储容器
        /// </summary>
        public List<DistanceSpaceItem> result_FindNearestN = new();

        /// <summary>
        /// 圆形扩散遍历找出范围内边缘最近的最多n个结果
        /// </summary>
        /// <param name="d">SpaceRingDiffuseData对象</param>
        /// <param name="x">逻辑坐标</param>
        /// <param name="y">逻辑坐标</param>
        /// <param name="maxDistance">限制结果集的最大边距（逻辑坐标）</param>
        /// <param name="n"></param>
        /// <returns>返回找到的实际物体个数</returns>
        public int FindNearestNByRange(SpaceRingDiffuseData d, float x, float y, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * _1_gridSize);
            if (cIdxBase < 0 || cIdxBase >= gridNumCols) return 0;
            int rIdxBase = (int)(y * _1_gridSize);
            if (rIdxBase < 0 || rIdxBase >= gridNumRows) return 0;
            var searchRange = maxDistance + gridSize;//searchRange决定了要扫多远的格子

            var os = result_FindNearestN;
            os.Clear();

            var lens = d.lens;
            var idxs = d.idxs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= gridNumCols) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= gridNumRows) continue;
                    var cidx = rIdx * gridNumCols + cIdx;

                    var c = Items[cidx];
                    while (c != null)
                    {
                        var vx = c.pixelX - x;
                        var vy = c.pixelY - y;
                        var dd = vx * vx + vy * vy;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > 0)
                        {
                            if (os.Count < n)
                            {
                                os.Add(new DistanceSpaceItem { distance = v, item = c });
                                if (os.Count == n)
                                {
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new DistanceSpaceItem { distance = v, item = c };
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                        }

                        c = c.spaceNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return os.Count;
        }

        /// <summary>
        /// 排序result_FindNearestN，注：若改用.Sort(); 函数会造成 128 byte GC
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void Quick_Sort(int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(left, right);
                if (pivot > 1)
                {
                    Quick_Sort(left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    Quick_Sort(pivot + 1, right);
                }
            }
        }

        /// <summary>
        /// 快速排序左右2个result_FindNearestN数组元素，如果存在相同距离则结束并返回右侧距离结果，否则进行交换将较小的距离放在左边数组
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private int Partition(int left, int right)
        {
            var arr = result_FindNearestN;
            var pivot = arr[left];
            while (true)
            {
                while (arr[left].distance < pivot.distance)
                {
                    left++;
                }
                while (arr[right].distance > pivot.distance)
                {
                    right--;
                }
                if (left < right)
                {
                    if (arr[left].distance == arr[right].distance) return right;
                    var temp = arr[left];
                    arr[left] = arr[right];
                    arr[right] = temp;
                }
                else return right;
            }
        }
    }
    /// <summary>
    /// 存储计数及范围的结构体
    /// </summary>
    public struct SpaceCountRadius
    {
        public int count;
        public float radius;
    }

    /// <summary>
    /// 储XY逻辑坐标的结构体
    /// </summary>
    public struct SpaceXYi
    {
        public int x, y;
    }

    /// <summary>
    /// 空间容器内的物体距离结构体，可以进行距离的比较
    /// </summary>
    public struct DistanceSpaceItem : IComparable<DistanceSpaceItem>
    {
        public float distance;
        public SpaceItem item;

        public int CompareTo(DistanceSpaceItem o)
        {
            return this.distance.CompareTo(o.distance);
        }
    }

    /// <summary>
    /// 填充圆形扩散的（逻辑坐标划分的）格子的偏移量数组，主用于更高效的范围内找最近
    /// </summary>
    public class SpaceRingDiffuseData
    {
        public List<SpaceCountRadius> lens = new();
        public List<SpaceXYi> idxs = new();
        /// <summary>
        /// [构造函数]填充圆形扩散的（逻辑坐标划分的）格子的偏移量数组，主用于更高效的范围内找最近
        /// </summary>
        /// <param name="gridNumRows">网格列数量</param>
        /// <param name="cellSize">单元格尺寸</param>
        public SpaceRingDiffuseData(int gridNumRows, int cellSize)
        {
            lens.Add(new SpaceCountRadius { count = 0, radius = 0f });
            idxs.Add(new SpaceXYi());
            HashSet<ulong> set = new();
            set.Add(0);
            for (float radius = 0; radius < cellSize * gridNumRows; radius += cellSize)
            {
                var lenBak = idxs.Count;
                var radians = Mathf.Asin(0.5f / radius) * 2;
                var step = (int)(Mathf.PI * 2 / radians);
                var inc = Mathf.PI * 2 / step;
                for (int i = 0; i < step; ++i)
                {
                    var a = inc * i;
                    var cos = Mathf.Cos(a);
                    var sin = Mathf.Sin(a);
                    var ix = (int)(cos * radius) / cellSize;
                    var iy = (int)(sin * radius) / cellSize;
                    var key = ((ulong)iy << 32) + (ulong)ix;
                    if (set.Add(key))
                    {
                        idxs.Add(new SpaceXYi { x = ix, y = iy });
                    }
                }
                if (idxs.Count > lenBak)
                {
                    lens.Add(new SpaceCountRadius { count = idxs.Count, radius = radius });
                }
            }
        }
    }
}