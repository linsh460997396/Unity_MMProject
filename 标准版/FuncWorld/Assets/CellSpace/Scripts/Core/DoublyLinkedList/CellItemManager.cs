using System;
using System.Collections.Generic;
using UnityEngine;

namespace CellSpace
{
    public class CellItemManager
    {
        /// <summary>
        /// CellChunk在团块空间的插入点(索引位置).可视作CellChunk的句柄.
        /// </summary>
        public CPIndex id;

        #region 双向链表

        #region 双向链表管理(声明部分)

        //‌双向链表‌:每个节点包含两个指针,分别指向前驱和后继节点,这使得从任意节点出发都能方便地访问前驱和后继节点,提高了操作的灵活性.
        //适用于已知团块空间索引大小情况下频繁进行前后遍历、插入和删除操作的场景和各种不需要排序的数据管理‌
        //‌链表池‌通过维护一个空闲节点池来减少内存分配和释放的次数,提高内存使用效率.在链表频繁插删操作时能够复用已删除节点,避免内存碎片产生.
        //若怪物、子弹、特效等数量较少且无需频繁刷新或空间大小(细分的单元数量)不确定情况可不使用本双向链表功能模块,可另开单位组遍历,但你需要重新设计一套检索办法.
        //这里主要用来刷新怪物、子弹、特效等活动对象的位置,以便随时调用"最近的XX"等配套方法来追踪对象.

        /// <summary>
        /// 团块空间逻辑计算用的单元网格尺寸(逻辑像素值非真实像素)倍率(三维各向一致).
        /// 若把值设置100f,设计摄像头覆盖范围或角色移动逻辑时,19200f=实际192单元格.
        /// </summary>
        [MetalMaxSystem.Note("重要配置参数")] public float cellSize = 100f;
        /// <summary>
        /// 团块空间内单元网格尺寸的倒数(invCellSize = 1 / cellSize).
        /// </summary>
        private float invCellSize;
        /// <summary>
        /// 团块边长(世界绝对坐标长度值).正常3D模式下或每个空间实际安排在世界坐标系中不重复时,与团块索引相乘可得到团块左下角插入点在世界坐标系的位置.
        /// </summary>
        public int sideLength = CPEngine.chunkSideLength;
        /// <summary>
        /// 团块空间最大边界尺寸.应在空间构造时自动计算,maxSize=cellSize*边长方向的单元数量即CPEngine.chunkSideLength(三维各向一致).
        /// </summary>
        public float maxSize;
        /// <summary>
        /// 团块空间内单元体(CellItem)的已注册数量.尽管容器没固定最大容量,但可用于状态检查是否为空或达到设计限值.
        /// </summary>
        public int count;
        /// <summary>
        /// 存放逻辑网格容器(单元体)的数组.可看作团块空间内(专由双向链表管理)的单位组.
        /// 2D横板、3D单层地形模式下元素数量按sideLength平方,正常3D模式按sideLength三次方.
        /// 双向链表通过CellItemManager提供的方法修改刷新,自动记录前驱后驱节点.
        /// </summary>
        public CellItem[] cellItems;

        #endregion

        #region 双向链表管理单元体(CellItem)_函数部分

        //若想构造带有双向链表管理单元体(CellItem)的团块空间,可开启CPEngine.useCellItem,之后通过团块实例的Add函数为空间容器添加单元体(怪物子弹特效等基类继续继承CellItem即可).

        /// <summary>
        /// [构造函数]双向链表.
        /// </summary>
        public CellItemManager()
        {
            if (CPEngine.useCellItem)
            {//启用双向链表管理单元体(CellItem),则在空间构造时初始化双向链表池,SpriteSpace框架的怪物等对象在空间内增减会自动刷新链表
             //不管何时何地调用检索方法,每个怪物类的AI都能找到谁离它最近,适用于管理上万活动对象索引位置.
#if UNITY_EDITOR
                //条件失败时进行断言,容器空间内的相对坐标化为索引的计算要求坐标必须是正值(容器左下角始终为原点插入点)
                Debug.Assert(sideLength > 0, "sideLength must be greater than 0.");
                Debug.Assert(cellSize > 0, "cellSize must be greater than 0.");
#endif
                //初始化字段
                invCellSize = 1f / cellSize;
                maxSize = cellSize * sideLength;
                //初始化cellItems[],数组大小为边长的个数的幂次方(即sideLength^边长个数).若数组未初始化则创建一个新的数组;若已存在,则用null填充并调整大小.
                if (cellItems == null)
                {
                    if (CPEngine.horizontalMode)
                    {
                        //2D横板模式(2D-XY)
                        cellItems = new CellItem[sideLength * sideLength];
                    }
                    else if (CPEngine.singleLayerTerrainMode)
                    {
                        //3D单层地形模式(2D-XZ)
                        cellItems = new CellItem[sideLength * sideLength];
                    }
                    else
                    {
                        //正常3D模式
                        cellItems = new CellItem[sideLength * sideLength * sideLength];
                    }
                }
                else
                {
                    //用null填充数组cellItems
#if !BEPINEX
                    Array.Fill(cellItems, null);
#else
                    //用如下代码替换以兼容旧版.NET/Unity(没Array.Fill情况):
                    for (int i = 0; i < cellItems.Length; i++)
                    {
                        cellItems[i] = null;
                    }
#endif
                    //重新调整数组cellItems[]的大小,新的大小为SideLength^边长个数,并将调整后的数组重新赋值给cellItems.
                    //这一步可能会创建一个新的数组,若原数组大小与新的大小不同,原数组的内容将被复制到新数组中(或部分复制,取决于大小变化).
                    //若新大小大于原大小,新元素将被设置为默认值(对于引用类型是null,对于值类型是0或相应的默认值).

                    if (CPEngine.horizontalMode)
                    {
                        //2D横板模式
                        Array.Resize(ref cellItems, sideLength * sideLength);
                    }
                    else if (CPEngine.singleLayerTerrainMode)
                    {
                        //3D单层地形模式
                        Array.Resize(ref cellItems, sideLength * sideLength);
                    }
                    else
                    {
                        //正常3D模式
                        Array.Resize(ref cellItems, sideLength * sideLength * sideLength);
                    }
                }
            }
        }

        /// <summary>
        /// 为双向链表添加网格容器(单元体),如添加一些继承CellItem的怪物类对象.
        /// </summary>
        /// <param name="c">单元体(CellItem)</param>
        public void Add(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            //Debug.Assert(c.chunk == this); //不属于这个空间则添加、刷新位置均无效
            Debug.Assert(c.index == -1);
            Debug.Assert(c.nodePrev == null);
            Debug.Assert(c.nodeNext == null);
            if (CPEngine.horizontalMode)
            {
                //2D横板模式
                Debug.Assert(c.x >= 0 && c.x < maxSize); //容器内相对位置需有效,不能超过最大索引
                Debug.Assert(c.y >= 0 && c.y < maxSize);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式
                Debug.Assert(c.x >= 0 && c.x < maxSize);
                Debug.Assert(c.z >= 0 && c.z < maxSize);
            }
            else
            {
                //正常3D模式
                Debug.Assert(c.x >= 0 && c.x < maxSize);
                Debug.Assert(c.y >= 0 && c.y < maxSize);
                Debug.Assert(c.z >= 0 && c.z < maxSize);
            }

#endif
            int index; //空间内相对坐标化为索引
            // 从坐标返回网格容器(单元体)在空间容器内的索引
            if (CPEngine.horizontalMode)
            {
                //2D横板模式
                index = PosToIndexH2D(c.x, c.y);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式
                index = PosToIndexH2D(c.x, c.z);
            }
            else
            {
                //正常3D模式
                index = PosToIndex(c.x, c.y, c.z);
            }

#if UNITY_EDITOR
            Debug.Assert(cellItems[index] == null || cellItems[index].nodePrev == null);
#endif

            // 进行Link
            if (cellItems[index] != null)
            {
                //若空间容器索引对应单元体存在,则将新单元体作为该单元体的前驱节点
                cellItems[index].nodePrev = c;
            }
            //将空间容器索引对应单元体作为新单元体的后驱节点(可为null)
            c.nodeNext = cellItems[index];
            c.index = index; //刷新新单元体的空间索引为idx
            cellItems[index] = c; //新单元体作为空间容器索引对应单元体
                                  //若只有1个节点,作为头部节点呈现:【Prev=null】【C】【Next=null】
#if UNITY_EDITOR
            Debug.Assert(cellItems[index].nodePrev == null);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
#endif
            //空间容器中网格容器(单元体)数量自增
            ++count;
        }
        /// <summary>
        /// 从空间容器的双向链表移除网格容器(单元体),如移除一些继承单元体(CellItem)的怪物类对象
        /// </summary>
        /// <param name="c">单元体(CellItem)</param>
        public void Remove(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            //Debug.Assert(c.chunk == this);
            Debug.Assert(c.nodePrev == null && cellItems[c.index] == c || c.nodePrev.nodeNext == c && cellItems[c.index] != c);
            Debug.Assert(c.nodeNext == null || c.nodeNext.nodePrev == c);
            //Debug.Assert(cellItems[c.index] include c);
#endif

            //unlink
            if (c.nodePrev != null)
            {//若目标单元体有前驱节点(说明它不是头部节点)
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] != c);
#endif
                //将目标单元体前驱节点(对应单元体)的后驱节点更换为目标单元体的后驱节点(目标单元体被移除,所以前后节点相连)
                c.nodePrev.nodeNext = c.nodeNext;
                if (c.nodeNext != null)
                {
                    //若目标单元体的后驱节点不为空(不是最后一个),将后驱节点的前驱节点设置为要移除目标单元体的前驱节点(目标单元体被移除,所以前后节点相连)
                    c.nodeNext.nodePrev = c.nodePrev;
                    c.nodeNext = null; //清空要删除的目标单元体的后驱节点
                }
                c.nodePrev = null; //清空要删除的目标单元体的前驱节点
            }
            else
            {
                //若目标单元体无前驱节点(说明它是头部节点)
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] == c);
#endif
                //目标位置的空间单元体被后驱节点替换(目标单元体作为头部节点被移除,所以后驱节点占位)
                cellItems[c.index] = c.nodeNext;
                if (c.nodeNext != null)
                {
                    //若目标单元体的后驱节点不为null,该后驱节点的前驱节点设置为null(后驱节点作为头部节点了)
                    c.nodeNext.nodePrev = null;
                    c.nodeNext = null; //清空要删除的目标单元体的后驱节点
                }
            }
#if UNITY_EDITOR
            Debug.Assert(cellItems[c.index] != c);
#endif
            c.index = -1; //初始化目标单元体的空间索引
            //c.chunk = null; //清空目标单元体的空间容器

            //空间容器中网格容器(单元体)数量自减
            --count;
        }
        /// <summary>
        /// 更新一个Cell对象在空间容器中的索引位置(同时更新双向链表),一般用于活动物体(继承CellItem的角色怪物类对象)在容器频繁移动时的刷新
        /// </summary>
        /// <param name="c"></param>
        public void Refresh(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            //Debug.Assert(c.chunk == this);
            Debug.Assert(c.index > -1);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
            //Debug.Assert(cellItems[c.index] include c);
#endif
#if UNITY_EDITOR
            if (CPEngine.horizontalMode)
            {
                //2D横板模式(2D-XY)
                Debug.Assert(c.x >= 0 && c.x < maxSize); //容器内相对位置需有效,不能超过最大索引
                Debug.Assert(c.y >= 0 && c.y < maxSize);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式(2D-XZ)
                Debug.Assert(c.x >= 0 && c.x < maxSize);
                Debug.Assert(c.z >= 0 && c.z < maxSize);
            }
            else
            {
                //正常3D模式
                Debug.Assert(c.x >= 0 && c.x < maxSize);
                Debug.Assert(c.y >= 0 && c.y < maxSize);
                Debug.Assert(c.z >= 0 && c.z < maxSize);
            }
#endif
            //将当前坐标转换为空间内的索引
            int cIdx = (int)(c.x * invCellSize);
            int rIdx = (int)(c.y * invCellSize);
            int hIdx = (int)(c.z * invCellSize);
            int index;
            if (CPEngine.horizontalMode)
            {
                //2D横板模式(2D-XY)
                index = rIdx * sideLength + cIdx;
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式(2D-XZ)
                index = hIdx * sideLength + cIdx;
            }
            else
            {
                //正常3D模式
                index = hIdx * sideLength * sideLength + rIdx * sideLength + cIdx;
            }
            //若单元格尺寸是2.0,那么_1_cellSize=0.5,得到的cIdx是修正后的值,最终单元体所在(1.5,1.5)对应单元格索引是0,因为(1.5,1.5)落在第一个单元格内,没毛病
#if UNITY_EDITOR
            Debug.Assert(index <= cellItems.Length); //超限报停
#endif
            //单元体索引位置与其index字段相同
            if (index == c.index) return;  //空间索引无变化时直接返回

            // 开始更新变化↓
            // unlink
            if (c.nodePrev != null)
            {  // isn'transform header 非头部单元体
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] != c);
#endif
                c.nodePrev.nodeNext = c.nodeNext;
                if (c.nodeNext != null)
                { //非尾部单元体
                    c.nodeNext.nodePrev = c.nodePrev;
                    //c.nodeNext = {};
                }
                //c.nodePrev = {};
            }
            else
            {
#if UNITY_EDITOR
                Debug.Assert(cellItems[c.index] == c);
#endif
                cellItems[c.index] = c.nodeNext;
                if (c.nodeNext != null)
                {
                    c.nodeNext.nodePrev = null;
                    //c.nodeNext = {};
                }
            }
            //c.index = -1;
#if UNITY_EDITOR
            Debug.Assert(cellItems[c.index] != c);
            Debug.Assert(index != c.index);
#endif

            // link
            if (cellItems[index] != null)
            {
                cellItems[index].nodePrev = c;
            }
            c.nodePrev = null;
            c.nodeNext = cellItems[index];
            cellItems[index] = c;
            c.index = index;
#if UNITY_EDITOR
            Debug.Assert(cellItems[index].nodePrev == null);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
#endif
        }

        #region 双向链表下的空间检索方法
        /// <summary>
        /// [2D横版模式]X-Y平面返回网格容器(单元体)位置在空间容器中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndexH2D(float x, float y)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxSize);
            Debug.Assert(y >= 0 && y < maxSize);
#endif
            int cIdx = (int)(x * invCellSize); //直接取整
            int rIdx = (int)(y * invCellSize);
            int idx = rIdx * sideLength + cIdx; //化作2D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= cellItems.Length); //超限报停
#endif
            return idx;
        }
        /// <summary>
        /// [3D单层地面模式]X-Z平面返回网格容器(单元体)在空间容器中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndex2D(float x, float z)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxSize);
            Debug.Assert(z >= 0 && z < maxSize);
#endif
            int cIdx = (int)(x * invCellSize); //直接取整
            int rIdx = (int)(z * invCellSize);
            int idx = rIdx * sideLength + cIdx; //化作2D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= cellItems.Length); //超限报停
#endif
            return idx;
        }
        /// <summary>
        /// [正常3D模式]XYZ平面返回网格容器(单元体)在空间容器中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndex(float x, float y, float z)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxSize);
            Debug.Assert(y >= 0 && y < maxSize);
            Debug.Assert(z >= 0 && z < maxSize);
#endif
            int cIdx = (int)(x * invCellSize); //直接取整
            int rIdx = (int)(y * invCellSize);
            int hIdx = (int)(z * invCellSize);
            int idx = hIdx * sideLength * sideLength + rIdx * sideLength + cIdx; //化作3D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= cellItems.Length); //超限报停
#endif
            return idx;
        }

        /// <summary>
        /// [2D横版模式]X-Y平面在九宫范围内找出第1个网格容器(单元体)并返回
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByNineBoxGridH2D(float x, float y, float radius)
        {
            // 5
            int cIdx = (int)(x * invCellSize);
            if (cIdx < 0 || cIdx >= sideLength) return null;
            int rIdx = (int)(y * invCellSize);
            if (rIdx < 0 || rIdx >= sideLength) return null;
            int idx = rIdx * sideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 6
            ++cIdx;
            if (cIdx >= sideLength) return null;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 3
            ++rIdx;
            if (rIdx >= sideLength) return null;
            idx += sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return null;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 4
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return null;
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            return null;
        }
        /// <summary>
        /// [3D单层地面模式]X-Z平面在九宫范围内找出第1个网格容器(单元体)并返回
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByNineBoxGrid2D(float x, float z, float radius)
        {
            // 5
            int cIdx = (int)(x * invCellSize);
            if (cIdx < 0 || cIdx >= sideLength) return null;
            int rIdx = (int)(z * invCellSize);
            if (rIdx < 0 || rIdx >= sideLength) return null;
            int idx = rIdx * sideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 6
            ++cIdx;
            if (cIdx >= sideLength) return null;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 3
            ++rIdx;
            if (rIdx >= sideLength) return null;
            idx += sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return null;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 4
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return null;
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vz = c.z - z;
                var r = c.radius + radius;
                if (vx * vx + vz * vz < r * r)
                {
                    return c;
                }
                c = c.nodeNext;
            }
            return null;
        }
        /// <summary>
        /// [正常3D模式]在27立方体网格范围内找出第1个网格容器(单元体)并返回.
        /// 采用从中心向外扩展的搜索顺序,优先检测最近的网格.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByTwentySevenBoxGrid(float x, float y, float z, float radius)
        {
            // 计算初始网格索引
            int cIdx = (int)(x * invCellSize);
            if (cIdx < 0 || cIdx >= sideLength) return null;
            int rIdx = (int)(y * invCellSize);
            if (rIdx < 0 || rIdx >= sideLength) return null;
            int dIdx = (int)(z * invCellSize);
            if (dIdx < 0 || dIdx >= sideLength) return null;

            // 定义3D搜索顺序(从中心开始向外扩展)
            int[] layerOffsets = {
        0, 0, 0,   // 中心层 - 中心 (14)
        1, 0, 0,   // 中心层 - 右 (15)
        0, 1, 0,   // 中心层 - 下 (17)
        -1, 0, 0,  // 中心层 - 左 (13)
        0, -1, 0,  // 中心层 - 上 (11)
        1, 1, 0,   // 中心层 - 右下 (18)
        -1, 1, 0,  // 中心层 - 左下 (16)
        1, -1, 0,  // 中心层 - 右上 (12)
        -1, -1, 0, // 中心层 - 左上 (10)
        
        0, 0, 1,   // 上层 - 中心 (23)
        1, 0, 1,   // 上层 - 右 (24)
        0, 1, 1,   // 上层 - 下 (26)
        -1, 0, 1,  // 上层 - 左 (22)
        0, -1, 1,  // 上层 - 上 (20)
        1, 1, 1,   // 上层 - 右下 (27)
        -1, 1, 1,  // 上层 - 左下 (25)
        1, -1, 1,  // 上层 - 右上 (21)
        -1, -1, 1, // 上层 - 左上 (19)
        
        0, 0, -1,  // 下层 - 中心 (5)
        1, 0, -1,  // 下层 - 右 (6)
        0, 1, -1,  // 下层 - 下 (8)
        -1, 0, -1, // 下层 - 左 (4)
        0, -1, -1, // 下层 - 上 (2)
        1, 1, -1,  // 下层 - 右下 (9)
        -1, 1, -1, // 下层 - 左下 (7)
        1, -1, -1, // 下层 - 右上 (3)
        -1, -1, -1 // 下层 - 左上 (1)
    };

            // 按顺序搜索27个网格
            for (int i = 0; i < layerOffsets.Length; i += 3)
            {
                int searchCIdx = cIdx + layerOffsets[i];
                int searchRIdx = rIdx + layerOffsets[i + 1];
                int searchDIdx = dIdx + layerOffsets[i + 2];

                // 检查边界
                if (searchCIdx < 0 || searchCIdx >= sideLength ||
                    searchRIdx < 0 || searchRIdx >= sideLength ||
                    searchDIdx < 0 || searchDIdx >= sideLength)
                    continue;

                // 计算一维索引
                int idx = searchDIdx * sideLength * sideLength + searchRIdx * sideLength + searchCIdx;
                var c = cellItems[idx];

                // 遍历链表检测碰撞
                while (c != null)
                {
                    var vx = c.x - x;
                    var vy = c.y - y;
                    var vz = c.z - z;  // 新增Z轴距离计算
                    var r = c.radius + radius;

                    // 3D球体碰撞检测
                    if (vx * vx + vy * vy + vz * vz < r * r)
                    {
                        return c;
                    }
                    c = c.nodeNext;
                }
            }

            return null;
        }

        /// <summary>
        /// [2D横版模式]X-Y平面遍历坐标周围九宫格内的网格容器(单元体)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC,但这种应该是无所谓的,里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGrid(float x, float y, Func<CellItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * invCellSize);
            if (cIdx < 0 || cIdx >= sideLength) return;
            int rIdx = (int)(y * invCellSize);
            if (rIdx < 0 || rIdx >= sideLength) return;
            int idx = rIdx * sideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= sideLength) return;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= sideLength) return;
            idx += sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
        }
        /// <summary>
        /// [3D单层地面模式]X-Z平面遍历坐标周围九宫格内的网格容器(单元体)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC,但这种应该是无所谓的, 里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGrid2D(float x, float z, Func<CellItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * invCellSize);
            if (cIdx < 0 || cIdx >= sideLength) return;
            int rIdx = (int)(z * invCellSize);
            if (rIdx < 0 || rIdx >= sideLength) return;
            int idx = rIdx * sideLength + cIdx;
            var c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= sideLength) return;
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= sideLength) return;
            idx += sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 1
            cIdx -= 2;
            if (cIdx < 0) return;
            --idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= sideLength;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = cellItems[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
        }
        /// <summary>
        /// [正常3D模式]在27立方体网格范围内遍历网格容器(单元体)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC,但这种应该是无所谓的,里面只要不含Unity资源)</param>
        public void ForeachAllByTwentySevenBoxGrid(float x, float y, float z, Func<CellItem, bool> handler)
        {
            // 计算初始网格索引
            int cIdx = (int)(x * invCellSize);
            if (cIdx < 0 || cIdx >= sideLength) return;
            int rIdx = (int)(y * invCellSize);
            if (rIdx < 0 || rIdx >= sideLength) return;
            int dIdx = (int)(z * invCellSize);
            if (dIdx < 0 || dIdx >= sideLength) return;

            // 定义3D搜索顺序(从中心开始向外扩展)
            int[] offsets = {
        // 中心层 (z=0)
        0, 0, 0,   // 中心 (14)
        1, 0, 0,   // 右 (15)
        0, 1, 0,   // 下 (17)
        -1, 0, 0,  // 左 (13)
        0, -1, 0,  // 上 (11)
        1, 1, 0,   // 右下 (18)
        -1, 1, 0,  // 左下 (16)
        1, -1, 0,  // 右上 (12)
        -1, -1, 0, // 左上 (10)
        
        // 上层 (z=1)
        0, 0, 1,   // 上层中心 (23)
        1, 0, 1,   // 上层右 (24)
        0, 1, 1,   // 上层下 (26)
        -1, 0, 1,  // 上层左 (22)
        0, -1, 1,  // 上层上 (20)
        1, 1, 1,   // 上层右下 (27)
        -1, 1, 1,  // 上层左下 (25)
        1, -1, 1,  // 上层右上 (21)
        -1, -1, 1, // 上层左上 (19)
        
        // 下层 (z=-1)
        0, 0, -1,  // 下层中心 (5)
        1, 0, -1,  // 下层右 (6)
        0, 1, -1,  // 下层下 (8)
        -1, 0, -1, // 下层左 (4)
        0, -1, -1, // 下层上 (2)
        1, 1, -1,  // 下层右下 (9)
        -1, 1, -1, // 下层左下 (7)
        1, -1, -1, // 下层右上 (3)
        -1, -1, -1 // 下层左上 (1)
    };

            // 按顺序搜索27个网格
            for (int i = 0; i < offsets.Length; i += 3)
            {
                int searchCIdx = cIdx + offsets[i];
                int searchRIdx = rIdx + offsets[i + 1];
                int searchDIdx = dIdx + offsets[i + 2];

                // 检查边界
                if (searchCIdx < 0 || searchCIdx >= sideLength ||
                    searchRIdx < 0 || searchRIdx >= sideLength ||
                    searchDIdx < 0 || searchDIdx >= sideLength)
                    continue;

                // 计算一维索引(3D数组转1D)
                int idx = searchDIdx * sideLength * sideLength + searchRIdx * sideLength + searchCIdx;
                var c = cellItems[idx];

                // 遍历链表
                while (c != null)
                {
                    var next = c.nodeNext;
                    if (handler(c)) return;
                    c = next;
                }
            }
        }

        /// <summary>
        /// [2D横版模式]X-Y平面圆形扩散遍历找出边距最近的1个网格容器(单元体)并返回
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns></returns>
        public CellItem FindNearestByRangeH2D(CellRingDiffuseXY d, float x, float y, float maxDistance)
        {
            int cIdxBase = (int)(x * invCellSize);
            if (cIdxBase < 0 || cIdxBase >= sideLength) return null;
            int rIdxBase = (int)(y * invCellSize);
            if (rIdxBase < 0 || rIdxBase >= sideLength) return null;
            var searchRange = maxDistance + cellSize;

            CellItem rtv = null;
            float maxV = 0;

            var lens = d.lens;
            var idxs = d.idxys;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= sideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= sideLength) continue;
                    var cidx = rIdx * sideLength + cIdx;
                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vy = c.y - y;
                        var dd = vx * vx + vy * vy;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > maxV)
                        {
                            rtv = c;
                            maxV = v;
                        }
                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return rtv;
        }
        /// <summary>
        /// [3D单层地面模式]X-Z平面圆形扩散遍历找出边距最近的1个网格容器(单元体)并返回
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns></returns>
        public CellItem FindNearestByRange2D(CellRingDiffuseXZ d, float x, float z, float maxDistance)
        {
            int cIdxBase = (int)(x * invCellSize);
            if (cIdxBase < 0 || cIdxBase >= sideLength) return null;
            int rIdxBase = (int)(z * invCellSize);
            if (rIdxBase < 0 || rIdxBase >= sideLength) return null;
            var searchRange = maxDistance + cellSize;

            CellItem rtv = null;
            float maxV = 0;

            var lens = d.lens;
            var idxs = d.idxzs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= sideLength) continue;
                    var rIdx = rIdxBase + tmp.z;
                    if (rIdx < 0 || rIdx >= sideLength) continue;
                    var cidx = rIdx * sideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vz = c.z - z;
                        var dd = vx * vx + vz * vz;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;
                        if (v > maxV)
                        {
                            rtv = c;
                            maxV = v;
                        }
                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return rtv;
        }
        /// <summary>
        /// [正常3D模式]在27立方体网格范围内遍历找出边距最近的1个网格容器(单元体)并返回
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public CellItem FindNearestByRange(CellRingDiffuseXYZ d, float x, float y, float z, float maxDistance)
        {
            int cIdxBase = (int)(x * invCellSize);
            if (cIdxBase < 0 || cIdxBase >= sideLength) return null;
            int rIdxBase = (int)(y * invCellSize);
            if (rIdxBase < 0 || rIdxBase >= sideLength) return null;
            int hIdxBase = (int)(z * invCellSize);
            if (hIdxBase < 0 || hIdxBase >= sideLength) return null;

            var searchRange = maxDistance + cellSize;

            CellItem rtv = null;
            float maxV = 0;

            var lens = d.lens;
            var idxs = d.idxyzs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= sideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= sideLength) continue;
                    var hIdx = hIdxBase + tmp.z;
                    if (hIdx < 0 || hIdx >= sideLength) continue;

                    var cidx = hIdx * sideLength * sideLength + rIdx * sideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vy = c.y - y;
                        var vz = c.z - z;
                        var dd = vx * vx + vy * vy + vz * vz;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > maxV)
                        {
                            rtv = c;
                            maxV = v;
                        }
                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return rtv;
        }

        /// <summary>
        /// 2D圆形或3D球形扩散遍历找范围内最多n个网格容器(单元体)的结果的存储容器
        /// </summary>
        public List<CellDistanceInfo> resultFindNearest = new List<CellDistanceInfo>();

        /// <summary>
        /// [2D横版模式]X-Y平面圆形扩散遍历找出范围内边缘最近的最多n个结果
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="maxDistance">限制结果集的最大边距</param>
        /// <param name="n"></param>
        /// <returns>返回实际个数</returns>
        public int FindNearestNByRangeH2D(CellRingDiffuseXY d, float x, float y, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * invCellSize);
            if (cIdxBase < 0 || cIdxBase >= sideLength) return 0;
            int rIdxBase = (int)(y * invCellSize);
            if (rIdxBase < 0 || rIdxBase >= sideLength) return 0;
            //searchRange决定了要扫多远的格子
            var searchRange = maxDistance + cellSize;

            var os = resultFindNearest;
            os.Clear();

            var lens = d.lens;
            var idxs = d.idxys;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= sideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= sideLength) continue;
                    var cidx = rIdx * sideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vy = c.y - y;
                        var dd = vx * vx + vy * vy;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > 0)
                        {
                            if (os.Count < n)
                            {
                                os.Add(new CellDistanceInfo { distance = v, cell = c });
                                if (os.Count == n)
                                {
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, cell = c };
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                        }

                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return os.Count;
        }
        /// <summary>
        /// [3D单层地面模式]X-Z平面圆形扩散遍历找出范围内边缘最近的最多n个结果
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="maxDistance">限制结果集的最大边距</param>
        /// <param name="n"></param>
        /// <returns>返回实际个数</returns>
        public int FindNearestNByRange2D(CellRingDiffuseXZ d, float x, float z, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * invCellSize);
            if (cIdxBase < 0 || cIdxBase >= sideLength) return 0;
            int rIdxBase = (int)(z * invCellSize);
            if (rIdxBase < 0 || rIdxBase >= sideLength) return 0;
            //searchRange决定了要扫多远的格子
            var searchRange = maxDistance + cellSize;

            var os = resultFindNearest;
            os.Clear();

            var lens = d.lens;
            var idxs = d.idxzs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= sideLength) continue;
                    var rIdx = rIdxBase + tmp.z;
                    if (rIdx < 0 || rIdx >= sideLength) continue;
                    var cidx = rIdx * sideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vz = c.z - z;
                        var dd = vx * vx + vz * vz;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > 0)
                        {
                            if (os.Count < n)
                            {
                                os.Add(new CellDistanceInfo { distance = v, cell = c });
                                if (os.Count == n)
                                {
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, cell = c };
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                        }

                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return os.Count;
        }
        /// <summary>
        /// [正常3D模式]三维圆球扩散遍历找出范围内边缘最近的最多n个结果
        /// </summary>
        /// <param name="d">扩散模式数据</param>
        /// <param name="x">查询点X坐标</param>
        /// <param name="y">查询点Y坐标</param>
        /// <param name="z">查询点Z坐标</param>
        /// <param name="maxDistance">限制结果集的最大边距</param>
        /// <param name="n">最大返回结果数</param>
        /// <returns>返回实际找到的个数</returns>
        public int FindNearestNByRange(CellRingDiffuseXYZ d, float x, float y, float z, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * invCellSize);
            if (cIdxBase < 0 || cIdxBase >= sideLength) return 0;
            int rIdxBase = (int)(y * invCellSize);
            if (rIdxBase < 0 || rIdxBase >= sideLength) return 0;
            int hIdxBase = (int)(z * invCellSize);
            if (hIdxBase < 0 || hIdxBase >= sideLength) return 0;

            var searchRange = maxDistance + cellSize;

            var os = resultFindNearest;
            os.Clear();

            var lens = d.lens;
            var idxs = d.idxyzs;
            for (int i = 1; i < lens.Count; i++)
            {
                var offsets = lens[i - 1].count;
                var size = lens[i].count - lens[i - 1].count;
                for (int j = 0; j < size; ++j)
                {
                    var tmp = idxs[offsets + j];
                    var cIdx = cIdxBase + tmp.x;
                    if (cIdx < 0 || cIdx >= sideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= sideLength) continue;
                    var hIdx = hIdxBase + tmp.z;
                    if (hIdx < 0 || hIdx >= sideLength) continue;

                    var cidx = hIdx * sideLength * sideLength + rIdx * sideLength + cIdx;

                    var c = cellItems[cidx];
                    while (c != null)
                    {
                        var vx = c.x - x;
                        var vy = c.y - y;
                        var vz = c.z - z;
                        var dd = vx * vx + vy * vy + vz * vz;
                        var r = maxDistance + c.radius;
                        var v = r * r - dd;

                        if (v > 0)
                        {
                            if (os.Count < n)
                            {
                                os.Add(new CellDistanceInfo { distance = v, cell = c });
                                if (os.Count == n)
                                {
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, cell = c };
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                        }

                        c = c.nodeNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return os.Count;
        }

        /// <summary>
        /// 排序resultFindNearestN2D,注:若改用.Sort(); 函数会造成 128 byte GC
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
        /// 快速排序左右2个resultFindNearestN2D数组元素,若存在相同距离则结束并返回右侧距离结果,否则进行交换将较小的距离放在左边数组
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private int Partition(int left, int right)
        {
            var arr = resultFindNearest;
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
        #endregion
        #endregion

        #endregion
    }
}

//用法:随意创建一个CellItemManager实例,登记团块的CPIndex,调用Add方法添加一些CellItem对象,
//然后调用FindFirstCrossByNineBoxGrid或FindFirstCrossByTwentySevenBoxGrid方法进行查询,或者调用ForeachAllByNineBoxGrid或ForeachAllByTwentySevenBoxGrid方法进行遍历.
//还可以使用FindNearestByRangeH2D、FindNearestByRange2D或FindNearestByRange方法进行范围内最近的查询,
//以及FindNearestNByRangeH2D、FindNearestNByRange2D或FindNearestNByRange方法进行范围内最近的多个结果查询.
//2个怪物都在空间A的第一个单元,用搜索离怪物1最近的节点对象时,怪物2会登陆在怪物1的前驱或后驱节点,直接就能返回
//范围搜索时不断检索前驱后驱,抓到的节点对象送入"单位组"供遍历,接着只需对比怪物类型来设计使用
