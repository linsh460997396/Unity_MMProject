using System;
using System.Collections.Generic;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 单元网格容器.是团块空间中双向链表管理的核心类,负责维护单元体数组和相关索引逻辑.
    /// </summary>
    public class CellGridContainer
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

        private float _cellSize;
        /// <summary>
        /// 单元网格尺寸倍率(三维各向一致).构造时设定,设计时可根据实际需求调整.
        /// 若值为100f,设计摄像头覆盖范围或角色移动逻辑时,19200f=实际192单元格.
        /// 修改会导致重新初始化空间,请谨慎调整.
        /// </summary>
        public float CellSize
        {
            set
            {
                _cellSize = value;
                if (_cellSize <= 0) { _cellSize = 1f; }
                Init(_cellSize);
            }

            get { return _cellSize; }
        }

        private float _invCellSize;
        /// <summary>
        /// 团块空间内单元网格尺寸的倒数(invCellSize = 1 / cellSize).
        /// </summary>
        public float InvCellSize
        {
            get { return _invCellSize; }
        }

        private int _sideLength = CPEngine.lChunkSideLength;
        /// <summary>
        /// 团块空间边长(世界绝对坐标长度值).修改会导致重新初始化空间,请谨慎调整.
        /// </summary>
        public int SideLength
        {
            set
            {
                _sideLength = value;
                if (_sideLength <= 0) { _sideLength = CPEngine.lChunkSideLength; }
                Init(_cellSize);
            }
            get { return _sideLength; }
        }

        private float _maxSize;
        /// <summary>
        /// 团块空间最大边界尺寸.在空间构造时自动计算,maxSize=cellSize*sideLength.
        /// </summary>
        public float MaxSize
        {
            get { return _maxSize; }
        }

        private int _count;
        /// <summary>
        /// 团块空间内CellItem的已注册数量.尽管容器没固定最大容量,但可用于状态检查是否为空或达到设计限值.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        private CellItem[] _items;
        /// <summary>
        /// 存放逻辑单元体的数组.可看作团块空间内(专由双向链表管理)的单位组.
        /// 2D横板、3D单层地形模式下元素数量按sideLength平方,正常3D模式按sideLength三次方.
        /// 双向链表通过CellItemManager提供的方法修改刷新,自动记录前驱后驱节点.
        /// </summary>
        public CellItem[] Items
        {
            get { return _items; }
        }

        #endregion

        #region 双向链表管理CellItem_函数部分

        //若想构造带有双向链表管理CellItem的团块空间,可开启CPEngine.useCellItem,之后通过团块实例的Add函数为空间添加单元体(怪物子弹特效等基类继承CellItem即可成为单元体).

        /// <summary>
        /// [构造函数]单元网格管理器.是团块空间中双向链表管理的核心类,负责维护一个单元单元体的数组和相关索引逻辑.
        /// </summary>
        /// <param name="gridSize">单元网格尺寸倍率,默认1f</param>
        public CellGridContainer(float gridSize = 1f, bool torf = true)
        {
            Init(gridSize, torf);
        }

        /// <summary>
        /// [初始化函数]单元网格管理器.在构造函数中调用,也可在运行时调整单元网格尺寸时调用以刷新空间.
        /// </summary>
        /// <param name="gridSize">单元网格尺寸倍率,默认1f</param>
        /// <param name="torf">CellItem[]存在时是否用null重新填充,默认true</param>
        public void Init(float gridSize = 1f, bool torf = true)
        {
            _cellSize = gridSize;
            //启用双向链表管理CellItem,则在空间构造时初始化双向链表池,SpriteSpace框架的怪物等对象在空间内增减会自动刷新链表
            //不管何时何地调用检索方法,每个怪物类的AI都能找到谁离它最近,适用于管理上万活动对象索引位置.
#if UNITY_EDITOR
            //条件失败时进行断言,容器空间内的相对坐标化为索引的计算要求坐标必须是正值(容器左下角始终为原点插入点)
            Debug.Assert(_sideLength > 0, "sideLength must be greater than 0.");
            Debug.Assert(_cellSize > 0, "cellSize must be greater than 0.");
#endif
            //初始化字段
            _invCellSize = 1f / _cellSize;
            _maxSize = _cellSize * _sideLength;

            //初始化cellItems[],数组大小为边长的个数的幂次方(即sideLength^边长个数).若数组未初始化则创建一个新的数组;若已存在,则用null填充并调整大小.
            if (_items == null)
            {
                if (CPEngine.horizontalMode)
                {
                    //2D横板模式(2D-XY)
                    _items = new CellItem[_sideLength * _sideLength];
                }
                else if (CPEngine.singleLayerTerrainMode)
                {
                    //3D单层地形模式(2D-XZ)
                    _items = new CellItem[_sideLength * _sideLength];
                }
                else
                {
                    //正常3D模式
                    _items = new CellItem[_sideLength * _sideLength * _sideLength];
                }
            }
            else
            {
                //用null填充数组cellItems
#if !BEPINEX
                if (torf) Array.Fill(_items, null);
#else
                    //用如下代码替换以兼容旧版.NET/Unity(没Array.Fill情况):
                    if (torf) 
                    {
                        for (int i = 0; i < cellItems.Length; i++)
                        {
                            cellItems[i] = null;
                        }
                    }
#endif
                //重新调整数组cellItems[]的大小,新的大小为SideLength^边长个数,并将调整后的数组重新赋值给cellItems.
                //这一步可能会创建一个新的数组,若原数组大小与新的大小不同,原数组的内容将被复制到新数组中(或部分复制,取决于大小变化).
                //若新大小大于原大小,新元素将被设置为默认值(对于引用类型是null,对于值类型是0或相应的默认值).

                if (CPEngine.horizontalMode)
                {
                    //2D横板模式
                    Array.Resize(ref _items, _sideLength * _sideLength);
                }
                else if (CPEngine.singleLayerTerrainMode)
                {
                    //3D单层地形模式
                    Array.Resize(ref _items, _sideLength * _sideLength);
                }
                else
                {
                    //正常3D模式
                    Array.Resize(ref _items, _sideLength * _sideLength * _sideLength);
                }
            }

        }

        /// <summary>
        /// 为双向链表添加单元体,如添加一些继承CellItem的怪物类对象.
        /// </summary>
        /// <param name="c">CellItem</param>
        public void Add(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            //Debug.Assert(c.chunk == this); //不属于这个空间则添加、刷新动作均无效
            Debug.Assert(c.index == -1);
            Debug.Assert(c.nodePrev == null);
            Debug.Assert(c.nodeNext == null);
            if (CPEngine.horizontalMode)
            {
                //2D横板模式
                Debug.Assert(c.x >= 0 && c.x < _maxSize); //空间内的相对位置需有效(不能超过最大索引)
                Debug.Assert(c.y >= 0 && c.y < _maxSize);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式
                Debug.Assert(c.x >= 0 && c.x < _maxSize);
                Debug.Assert(c.z >= 0 && c.z < _maxSize);
            }
            else
            {
                //正常3D模式
                Debug.Assert(c.x >= 0 && c.x < _maxSize);
                Debug.Assert(c.y >= 0 && c.y < _maxSize);
                Debug.Assert(c.z >= 0 && c.z < _maxSize);
            }

#endif
            int index; //空间内的相对坐标化为索引
            // 从坐标返回单元体在空间内的索引
            if (CPEngine.horizontalMode)
            {
                //2D横板模式
                index = PosToIndex2D(c.x, c.y);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式
                index = PosToIndex2D(c.x, c.z);
            }
            else
            {
                //正常3D模式
                index = PosToIndex(c.x, c.y, c.z);
            }

#if UNITY_EDITOR
            Debug.Assert(_items[index] == null || _items[index].nodePrev == null);
#endif

            // 进行Link
            if (_items[index] != null)
            {
                //若空间索引对应单元体存在,则将新单元体作为该单元体的前驱节点
                _items[index].nodePrev = c;
            }
            //将空间索引对应单元体作为新单元体的后驱节点(可为null)
            c.nodeNext = _items[index];
            c.index = index; //刷新新单元体的空间索引为idx
            _items[index] = c; //新单元体作为空间索引对应单元体
                                   //若只有1个节点,作为头部节点呈现:【Prev=null】【C】【Next=null】
#if UNITY_EDITOR
            Debug.Assert(_items[index].nodePrev == null);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
#endif
            //空间中单元体数量自增
            ++_count;
        }
        /// <summary>
        /// 从空间的双向链表移除单元体,如移除一些继承CellItem的怪物类对象
        /// </summary>
        /// <param name="c">CellItem</param>
        public void Remove(CellItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            //Debug.Assert(c.chunk == this);
            Debug.Assert(c.nodePrev == null && _items[c.index] == c || c.nodePrev.nodeNext == c && _items[c.index] != c);
            Debug.Assert(c.nodeNext == null || c.nodeNext.nodePrev == c);
            //Debug.Assert(cellItems[c.index] include c);
#endif

            //unlink
            if (c.nodePrev != null)
            {//若目标单元体有前驱节点(说明它不是头部节点)
#if UNITY_EDITOR
                Debug.Assert(_items[c.index] != c);
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
                Debug.Assert(_items[c.index] == c);
#endif
                //目标位置的空间单元体被后驱节点替换(目标单元体作为头部节点被移除,所以后驱节点占位)
                _items[c.index] = c.nodeNext;
                if (c.nodeNext != null)
                {
                    //若目标单元体的后驱节点不为null,该后驱节点的前驱节点设置为null(后驱节点作为头部节点了)
                    c.nodeNext.nodePrev = null;
                    c.nodeNext = null; //清空要删除的目标单元体的后驱节点
                }
            }
#if UNITY_EDITOR
            Debug.Assert(_items[c.index] != c);
#endif
            c.index = -1; //初始化目标单元体的空间索引
            //c.chunk = null; //清空目标单元体的空间

            //空间中单元体数量自减
            --_count;
        }
        /// <summary>
        /// 更新一个Cell对象在空间中的索引位置(同时更新双向链表),一般用于活动物体(继承CellItem的角色怪物类对象)在容器频繁移动时的刷新
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
                Debug.Assert(c.x >= 0 && c.x < _maxSize); //容器内相对位置需有效,不能超过最大索引
                Debug.Assert(c.y >= 0 && c.y < _maxSize);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式(2D-XZ)
                Debug.Assert(c.x >= 0 && c.x < _maxSize);
                Debug.Assert(c.z >= 0 && c.z < _maxSize);
            }
            else
            {
                //正常3D模式
                Debug.Assert(c.x >= 0 && c.x < _maxSize);
                Debug.Assert(c.y >= 0 && c.y < _maxSize);
                Debug.Assert(c.z >= 0 && c.z < _maxSize);
            }
#endif
            //将当前坐标转换为空间内的索引
            int cIdx = (int)(c.x * _invCellSize);
            int rIdx = (int)(c.y * _invCellSize);
            int hIdx = (int)(c.z * _invCellSize);
            int index;
            if (CPEngine.horizontalMode)
            {
                //2D横板模式(2D-XY)
                index = rIdx * _sideLength + cIdx;
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地形模式(2D-XZ)
                index = hIdx * _sideLength + cIdx;
            }
            else
            {
                //正常3D模式
                index = hIdx * _sideLength * _sideLength + rIdx * _sideLength + cIdx;
            }
            //若单元格尺寸是2.0,那么_1_cellSize=0.5,得到的cIdx是修正后的值,最终单元体所在(1.5,1.5)对应单元格索引是0,因为(1.5,1.5)落在第一个单元格内,没毛病
#if UNITY_EDITOR
            Debug.Assert(index <= _items.Length); //超限报停
#endif
            //单元体索引位置与其index字段相同
            if (index == c.index) return;  //空间索引无变化时直接返回

            // 开始更新变化↓
            // unlink
            if (c.nodePrev != null)
            {  // isn'transform header 非头部单元体
#if UNITY_EDITOR
                Debug.Assert(_items[c.index] != c);
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
                Debug.Assert(_items[c.index] == c);
#endif
                _items[c.index] = c.nodeNext;
                if (c.nodeNext != null)
                {
                    c.nodeNext.nodePrev = null;
                    //c.nodeNext = {};
                }
            }
            //c.index = -1;
#if UNITY_EDITOR
            Debug.Assert(_items[c.index] != c);
            Debug.Assert(index != c.index);
#endif

            // link
            if (_items[index] != null)
            {
                _items[index].nodePrev = c;
            }
            c.nodePrev = null;
            c.nodeNext = _items[index];
            _items[index] = c;
            c.index = index;
#if UNITY_EDITOR
            Debug.Assert(_items[index].nodePrev == null);
            Debug.Assert(c.nodeNext != c);
            Debug.Assert(c.nodePrev != c);
#endif
        }

        #region 双向链表下的空间检索方法
        /// <summary>
        /// [2D横版模式XY][3D单层地面模式XZ]返回单元体在空间中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndex2D(float x, float y)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < _maxSize);
            Debug.Assert(y >= 0 && y < _maxSize);
#endif
            int cIdx = (int)(x * _invCellSize); //直接取整
            int rIdx = (int)(y * _invCellSize);
            int idx = rIdx * _sideLength + cIdx; //化作2D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= _items.Length); //超限报停
#endif
            return idx;
        }
        /// <summary>
        /// [正常3D模式]XYZ平面返回单元体在空间中的索引
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>return cellItems index</returns>
        public int PosToIndex(float x, float y, float z)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < _maxSize);
            Debug.Assert(y >= 0 && y < _maxSize);
            Debug.Assert(z >= 0 && z < _maxSize);
#endif
            int cIdx = (int)(x * _invCellSize); //直接取整
            int rIdx = (int)(y * _invCellSize);
            int hIdx = (int)(z * _invCellSize);
            int idx = hIdx * _sideLength * _sideLength + rIdx * _sideLength + cIdx; //化作3D空间索引
#if UNITY_EDITOR
            Debug.Assert(idx <= _items.Length); //超限报停
#endif
            return idx;
        }

        /// <summary>
        /// [2D横版模式]X-Y平面在九宫范围内找出第1个单元体并返回
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByNineBoxGridY2D(float x, float y, float radius)
        {
            // 5
            int cIdx = (int)(x * _invCellSize);
            if (cIdx < 0 || cIdx >= _sideLength) return null;
            int rIdx = (int)(y * _invCellSize);
            if (rIdx < 0 || rIdx >= _sideLength) return null;
            int idx = rIdx * _sideLength + cIdx;
            var c = _items[idx];
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
            if (cIdx >= _sideLength) return null;
            ++idx;
            c = _items[idx];
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
            if (rIdx >= _sideLength) return null;
            idx += _sideLength;
            c = _items[idx];
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
            c = _items[idx];
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
            c = _items[idx];
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
            idx -= _sideLength;
            c = _items[idx];
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
            idx -= _sideLength;
            c = _items[idx];
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
            c = _items[idx];
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
            c = _items[idx];
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
        /// [3D单层地面模式]X-Z平面在九宫范围内找出第1个单元体并返回
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByNineBoxGridZ2D(float x, float z, float radius)
        {
            // 5
            int cIdx = (int)(x * _invCellSize);
            if (cIdx < 0 || cIdx >= _sideLength) return null;
            int rIdx = (int)(z * _invCellSize);
            if (rIdx < 0 || rIdx >= _sideLength) return null;
            int idx = rIdx * _sideLength + cIdx;
            var c = _items[idx];
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
            if (cIdx >= _sideLength) return null;
            ++idx;
            c = _items[idx];
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
            if (rIdx >= _sideLength) return null;
            idx += _sideLength;
            c = _items[idx];
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
            c = _items[idx];
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
            c = _items[idx];
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
            idx -= _sideLength;
            c = _items[idx];
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
            idx -= _sideLength;
            c = _items[idx];
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
            c = _items[idx];
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
            c = _items[idx];
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
        /// [2D横版模式XY][3D单层地面模式XZ]在九宫范围内找出第1个单元体并返回
        /// </summary>
        /// <param name="column">列索引</param>
        /// <param name="row">行索引</param>
        /// <param name="radius">半径</param>
        /// <returns>第一个碰撞的单元体</returns>
        public CellItem FindFirstCrossByNineBoxGrid2D(float column, float row, float radius) 
        {
            if (CPEngine.horizontalMode)
            {
                //2D横板模式
                return FindFirstCrossByNineBoxGridY2D(column, row, radius);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地面模式
                return FindFirstCrossByNineBoxGridZ2D(column, row, radius);
            }
            else
            {
                //正常3D模式不适用九宫格搜索,请使用FindFirstCrossByTwentySevenBoxGrid函数
                return null;
            }
        }
        /// <summary>
        /// [正常3D模式]在三维九宫(27单元)网格范围内找出第1个单元体并返回.
        /// 采用从中心向外扩展的搜索顺序,优先检测最近的单元.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public CellItem FindFirstCrossByNineBoxGrid(float x, float y, float z, float radius)
        {
            // 计算初始网格索引
            int cIdx = (int)(x * _invCellSize);
            if (cIdx < 0 || cIdx >= _sideLength) return null;
            int rIdx = (int)(y * _invCellSize);
            if (rIdx < 0 || rIdx >= _sideLength) return null;
            int dIdx = (int)(z * _invCellSize);
            if (dIdx < 0 || dIdx >= _sideLength) return null;

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
                if (searchCIdx < 0 || searchCIdx >= _sideLength ||
                    searchRIdx < 0 || searchRIdx >= _sideLength ||
                    searchDIdx < 0 || searchDIdx >= _sideLength)
                    continue;

                // 计算一维索引
                int idx = searchDIdx * _sideLength * _sideLength + searchRIdx * _sideLength + searchCIdx;
                var c = _items[idx];

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
        /// [2D横版模式]X-Y平面遍历坐标周围九宫格内的单元体
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC,但这种应该是无所谓的,里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGridY2D(float x, float y, Func<CellItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * _invCellSize);
            if (cIdx < 0 || cIdx >= _sideLength) return;
            int rIdx = (int)(y * _invCellSize);
            if (rIdx < 0 || rIdx >= _sideLength) return;
            int idx = rIdx * _sideLength + cIdx;
            var c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= _sideLength) return;
            ++idx;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= _sideLength) return;
            idx += _sideLength;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = _items[idx];
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
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= _sideLength;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= _sideLength;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
        }
        /// <summary>
        /// [3D单层地面模式]X-Z平面遍历坐标周围九宫格内的单元体
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC,但这种应该是无所谓的,里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGridZ2D(float x, float z, Func<CellItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * _invCellSize);
            if (cIdx < 0 || cIdx >= _sideLength) return;
            int rIdx = (int)(z * _invCellSize);
            if (rIdx < 0 || rIdx >= _sideLength) return;
            int idx = rIdx * _sideLength + cIdx;
            var c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= _sideLength) return;
            ++idx;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= _sideLength) return;
            idx += _sideLength;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = _items[idx];
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
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= _sideLength;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= _sideLength;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = _items[idx];
            while (c != null)
            {
                var next = c.nodeNext;
                if (handler(c)) return;
                c = next;
            }
        }
        /// <summary>
        /// [2D横版模式XY][3D单层地面模式XZ]遍历坐标周围九宫格内的单元体.
        /// </summary>
        /// <param name="column">列坐标</param>
        /// <param name="row">行坐标</param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC,但这种应该是无所谓的,里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGrid2D(float column, float row, Func<CellItem, bool> handler) 
        {
            if (CPEngine.horizontalMode)
            {
                //2D横板模式
                ForeachAllByNineBoxGridY2D(column, row, handler);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //3D单层地面模式
                ForeachAllByNineBoxGridZ2D(column, row, handler);
            }
            else
            {
                //正常3D模式不适用九宫格搜索,请使用ForeachAllByTwentySevenBoxGrid函数
                return;
            }
        }
        /// <summary>
        /// [正常3D模式]在三维九宫(27单元)网格范围内遍历单元体
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="handler">返回true结束遍历(Func可能产生GC,但这种应该是无所谓的,里面只要不含Unity资源)</param>
        public void ForeachAllByNineBoxGrid(float x, float y, float z, Func<CellItem, bool> handler)
        {
            // 计算初始网格索引
            int cIdx = (int)(x * _invCellSize);
            if (cIdx < 0 || cIdx >= _sideLength) return;
            int rIdx = (int)(y * _invCellSize);
            if (rIdx < 0 || rIdx >= _sideLength) return;
            int dIdx = (int)(z * _invCellSize);
            if (dIdx < 0 || dIdx >= _sideLength) return;

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
                if (searchCIdx < 0 || searchCIdx >= _sideLength ||
                    searchRIdx < 0 || searchRIdx >= _sideLength ||
                    searchDIdx < 0 || searchDIdx >= _sideLength)
                    continue;

                // 计算一维索引(3D数组转1D)
                int idx = searchDIdx * _sideLength * _sideLength + searchRIdx * _sideLength + searchCIdx;
                var c = _items[idx];

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
        /// [2D横版模式]X-Y平面圆形扩散遍历找出边距最近的1个单元体并返回
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns></returns>
        public CellItem FindNearestByRangeY2D(CellRingDiffuseData d, float x, float y, float maxDistance)
        {
            int cIdxBase = (int)(x * _invCellSize);
            if (cIdxBase < 0 || cIdxBase >= _sideLength) return null;
            int rIdxBase = (int)(y * _invCellSize);
            if (rIdxBase < 0 || rIdxBase >= _sideLength) return null;
            var searchRange = maxDistance + _cellSize;

            CellItem rtv = null;
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
                    var cIdx = cIdxBase + tmp.column;
                    if (cIdx < 0 || cIdx >= _sideLength) continue;
                    var rIdx = rIdxBase + tmp.row;
                    if (rIdx < 0 || rIdx >= _sideLength) continue;
                    var cidx = rIdx * _sideLength + cIdx;
                    var c = _items[cidx];
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
        /// [3D单层地面模式]X-Z平面圆形扩散遍历找出边距最近的1个单元体并返回
        /// </summary>
        /// <param name="d"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns></returns>
        public CellItem FindNearestByRangeZ2D(CellRingDiffuseData d, float x, float z, float maxDistance)
        {
            int cIdxBase = (int)(x * _invCellSize);
            if (cIdxBase < 0 || cIdxBase >= _sideLength) return null;
            int rIdxBase = (int)(z * _invCellSize);
            if (rIdxBase < 0 || rIdxBase >= _sideLength) return null;
            var searchRange = maxDistance + _cellSize;

            CellItem rtv = null;
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
                    var cIdx = cIdxBase + tmp.column;
                    if (cIdx < 0 || cIdx >= _sideLength) continue;
                    var rIdx = rIdxBase + tmp.row;
                    if (rIdx < 0 || rIdx >= _sideLength) continue;
                    var cidx = rIdx * _sideLength + cIdx;

                    var c = _items[cidx];
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
        /// [2D横版模式XY][3D单层地面模式XZ]在九宫范围内找出边距最近的1个单元体并返回
        /// </summary>
        /// <param name="d">圆形扩散数据</param>
        /// <param name="column">列坐标</param>
        /// <param name="row">行坐标</param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns>返回最近的单元体</returns>
        public CellItem FindNearestByRange2D(CellRingDiffuseData d, float column, float row, float maxDistance) 
        {
            if (CPEngine.horizontalMode) 
            {
                //2D横板模式
                return FindNearestByRangeY2D(d, column, row, maxDistance);
            }
            else if (CPEngine.singleLayerTerrainMode) 
            {
                //3D单层地面模式
                return FindNearestByRangeZ2D(d, column, row, maxDistance);
            }
            else 
            {
                //正常3D模式不适用九宫格搜索,请使用FindNearestByRange函数
                return null;
            }
        }
        /// <summary>
        /// [正常3D模式]在三维九宫(27单元)网格范围内遍历找出边距最近的1个单元体并返回
        /// </summary>
        /// <param name="d">球形扩散数据</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="z">Z坐标</param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns>返回最近的单元体</returns>
        public CellItem FindNearestByRange(CellRingDiffuseXYZ d, float x, float y, float z, float maxDistance)
        {
            int cIdxBase = (int)(x * _invCellSize);
            if (cIdxBase < 0 || cIdxBase >= _sideLength) return null;
            int rIdxBase = (int)(y * _invCellSize);
            if (rIdxBase < 0 || rIdxBase >= _sideLength) return null;
            int hIdxBase = (int)(z * _invCellSize);
            if (hIdxBase < 0 || hIdxBase >= _sideLength) return null;

            var searchRange = maxDistance + _cellSize;

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
                    if (cIdx < 0 || cIdx >= _sideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= _sideLength) continue;
                    var hIdx = hIdxBase + tmp.z;
                    if (hIdx < 0 || hIdx >= _sideLength) continue;

                    var cidx = hIdx * _sideLength * _sideLength + rIdx * _sideLength + cIdx;

                    var c = _items[cidx];
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
        /// 2D圆形或3D球形扩散遍历找范围内最多n个单元体的结果的存储容器
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
        public int FindNearestNByRangeY2D(CellRingDiffuseData d, float x, float y, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * _invCellSize);
            if (cIdxBase < 0 || cIdxBase >= _sideLength) return 0;
            int rIdxBase = (int)(y * _invCellSize);
            if (rIdxBase < 0 || rIdxBase >= _sideLength) return 0;
            //searchRange决定了要扫多远的格子
            var searchRange = maxDistance + _cellSize;

            var os = resultFindNearest;
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
                    var cIdx = cIdxBase + tmp.column;
                    if (cIdx < 0 || cIdx >= _sideLength) continue;
                    var rIdx = rIdxBase + tmp.row;
                    if (rIdx < 0 || rIdx >= _sideLength) continue;
                    var cidx = rIdx * _sideLength + cIdx;

                    var c = _items[cidx];
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
                                os.Add(new CellDistanceInfo { distance = v, item = c });
                                if (os.Count == n)
                                {
                                    QuickSort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, item = c };
                                    QuickSort(0, os.Count - 1);
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
        public int FindNearestNByRangeZ2D(CellRingDiffuseData d, float x, float z, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * _invCellSize);
            if (cIdxBase < 0 || cIdxBase >= _sideLength) return 0;
            int rIdxBase = (int)(z * _invCellSize);
            if (rIdxBase < 0 || rIdxBase >= _sideLength) return 0;
            //searchRange决定了要扫多远的格子
            var searchRange = maxDistance + _cellSize;

            var os = resultFindNearest;
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
                    var cIdx = cIdxBase + tmp.column;
                    if (cIdx < 0 || cIdx >= _sideLength) continue;
                    var rIdx = rIdxBase + tmp.row;
                    if (rIdx < 0 || rIdx >= _sideLength) continue;
                    var cidx = rIdx * _sideLength + cIdx;

                    var c = _items[cidx];
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
                                os.Add(new CellDistanceInfo { distance = v, item = c });
                                if (os.Count == n)
                                {
                                    QuickSort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, item = c };
                                    QuickSort(0, os.Count - 1);
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
        /// [2D横版模式XY][3D单层地面模式XZ]平面圆形扩散遍历找出范围内边缘最近的最多n个结果
        /// </summary>
        /// <param name="d"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="maxDistance"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public int FindNearestNByRange2D(CellRingDiffuseData d, float column, float row, float maxDistance, int n) 
        { 
            if (CPEngine.horizontalMode) 
            {
                //2D横板模式
                return FindNearestNByRangeY2D(d, column, row, maxDistance, n);
            }
            else if (CPEngine.singleLayerTerrainMode) 
            {
                //3D单层地面模式
                return FindNearestNByRangeZ2D(d, column, row, maxDistance, n);
            }
            else 
            {
                //正常3D模式不适用九宫格搜索,请使用FindNearestNByRange函数
                return 0;
            }
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
            int cIdxBase = (int)(x * _invCellSize);
            if (cIdxBase < 0 || cIdxBase >= _sideLength) return 0;
            int rIdxBase = (int)(y * _invCellSize);
            if (rIdxBase < 0 || rIdxBase >= _sideLength) return 0;
            int hIdxBase = (int)(z * _invCellSize);
            if (hIdxBase < 0 || hIdxBase >= _sideLength) return 0;

            var searchRange = maxDistance + _cellSize;

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
                    if (cIdx < 0 || cIdx >= _sideLength) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= _sideLength) continue;
                    var hIdx = hIdxBase + tmp.z;
                    if (hIdx < 0 || hIdx >= _sideLength) continue;

                    var cidx = hIdx * _sideLength * _sideLength + rIdx * _sideLength + cIdx;

                    var c = _items[cidx];
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
                                os.Add(new CellDistanceInfo { distance = v, item = c });
                                if (os.Count == n)
                                {
                                    QuickSort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new CellDistanceInfo { distance = v, item = c };
                                    QuickSort(0, os.Count - 1);
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
        private void QuickSort(int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(left, right);
                if (pivot > 1)
                {
                    QuickSort(left, pivot - 1);
                }
                if (pivot + 1 < right)
                {
                    QuickSort(pivot + 1, right);
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

