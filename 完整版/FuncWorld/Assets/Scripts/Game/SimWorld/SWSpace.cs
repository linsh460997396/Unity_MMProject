using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimWorld
{
    public class SWGridItem
    {
        public SWSpaceContainer spaceContainer;
        public SWGridItem spacePrev, spaceNext;
        public int spaceIndex = -1;
        public float x, y;
        public float radius;
    }

    public class SWSpaceContainer
    {
        public int numRows, numCols;                // gridSize info
        public float cellSize, _1_cellSize;         // = 1 / gridSize
        public float maxY, maxX;                    // edge position
        public int numItems;                        // for state
        public SWGridItem[] cells;                  // grids container( gridChunkNumRows * gridChunkNumCols )


        public SWSpaceContainer(int numRows_, int numCols_, float cellSize_)
        {
#if UNITY_EDITOR
            Debug.Assert(numRows_ > 0);
            Debug.Assert(numCols_ > 0);
            Debug.Assert(cellSize_ > 0);
#endif
            numRows = numRows_;
            numCols = numCols_;
            cellSize = cellSize_;
            _1_cellSize = 1f / cellSize_;
            maxY = cellSize * numRows;
            maxX = cellSize * numCols;
            if (cells == null)
            {
                cells = new SWGridItem[numRows * numCols];
            }
            else
            {
                Array.Fill(cells, null);
                Array.Resize(ref cells, numRows * numCols);
            }
        }


        public void Add(SWGridItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.spaceContainer == this);
            Debug.Assert(c.spaceIndex == -1);
            Debug.Assert(c.spacePrev == null);
            Debug.Assert(c.spaceNext == null);
            Debug.Assert(c.x >= 0 && c.x < maxX);
            Debug.Assert(c.y >= 0 && c.y < maxY);
#endif

            // calc rIdx & cIdx
            var idx = PosToIndex(c.x, c.y);
#if UNITY_EDITOR
            Debug.Assert(cells[idx] == null || cells[idx].spacePrev == null);
#endif

            // link
            if (cells[idx] != null)
            {
                cells[idx].spacePrev = c;
            }
            c.spaceNext = cells[idx];
            c.spaceIndex = idx;
            cells[idx] = c;
#if UNITY_EDITOR
            Debug.Assert(cells[idx].spacePrev == null);
            Debug.Assert(c.spaceNext != c);
            Debug.Assert(c.spacePrev != c);
#endif

            // stat
            ++numItems;
        }


        public void Remove(SWGridItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.spaceContainer == this);
            Debug.Assert(c.spacePrev == null && cells[c.spaceIndex] == c || c.spacePrev.spaceNext == c && cells[c.spaceIndex] != c);
            Debug.Assert(c.spaceNext == null || c.spaceNext.spacePrev == c);
            //Debug.Assert(Items[c.index] include c);
#endif

            // unlink
            if (c.spacePrev != null)
            {  // isn'transform header
#if UNITY_EDITOR
                Debug.Assert(cells[c.spaceIndex] != c);
#endif
                c.spacePrev.spaceNext = c.spaceNext;
                if (c.spaceNext != null)
                {
                    c.spaceNext.spacePrev = c.spacePrev;
                    c.spaceNext = null;
                }
                c.spacePrev = null;
            }
            else
            {
#if UNITY_EDITOR
                Debug.Assert(cells[c.spaceIndex] == c);
#endif
                cells[c.spaceIndex] = c.spaceNext;
                if (c.spaceNext != null)
                {
                    c.spaceNext.spacePrev = null;
                    c.spaceNext = null;
                }
            }
#if UNITY_EDITOR
            Debug.Assert(cells[c.spaceIndex] != c);
#endif
            c.spaceIndex = -1;
            c.spaceContainer = null;

            // stat
            --numItems;
        }


        public void Update(SWGridItem c)
        {
#if UNITY_EDITOR
            Debug.Assert(c != null);
            Debug.Assert(c.spaceContainer == this);
            Debug.Assert(c.spaceIndex > -1);
            Debug.Assert(c.spaceNext != c);
            Debug.Assert(c.spacePrev != c);
            //Debug.Assert(Items[c.index] include c);
#endif

            var x = c.x;
            var y = c.y;
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxX);
            Debug.Assert(y >= 0 && y < maxY);
#endif
            int cIdx = (int)(x * _1_cellSize);
            int rIdx = (int)(y * _1_cellSize);
            int idx = rIdx * numCols + cIdx;
#if UNITY_EDITOR
            Debug.Assert(idx <= cells.Length);
#endif

            if (idx == c.spaceIndex) return;  // no change

            // unlink
            if (c.spacePrev != null)
            {  // isn'transform header
#if UNITY_EDITOR
                Debug.Assert(cells[c.spaceIndex] != c);
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
                Debug.Assert(cells[c.spaceIndex] == c);
#endif
                cells[c.spaceIndex] = c.spaceNext;
                if (c.spaceNext != null)
                {
                    c.spaceNext.spacePrev = null;
                    //c.nodeNext = {};
                }
            }
            //c.index = -1;
#if UNITY_EDITOR
            Debug.Assert(cells[c.spaceIndex] != c);
            Debug.Assert(idx != c.spaceIndex);
#endif

            // link
            if (cells[idx] != null)
            {
                cells[idx].spacePrev = c;
            }
            c.spacePrev = null;
            c.spaceNext = cells[idx];
            cells[idx] = c;
            c.spaceIndex = idx;
#if UNITY_EDITOR
            Debug.Assert(cells[idx].spacePrev == null);
            Debug.Assert(c.spaceNext != c);
            Debug.Assert(c.spacePrev != c);
#endif
        }

        // return Items index
        public int PosToIndex(float x, float y)
        {
#if UNITY_EDITOR
            Debug.Assert(x >= 0 && x < maxX);
            Debug.Assert(y >= 0 && y < maxY);
#endif
            int cIdx = (int)(x * _1_cellSize);
            int rIdx = (int)(y * _1_cellSize);
            int idx = rIdx * numCols + cIdx;
#if UNITY_EDITOR
            Debug.Assert(idx <= cells.Length);
#endif
            return idx;
        }



        // 在 9 宫内找出 第1个 相交物 并返回
        public SWGridItem FindFirstCrossBy9(float x, float y, float radius)
        {
            // 5
            int cIdx = (int)(x * _1_cellSize);
            if (cIdx < 0 || cIdx >= numCols) return null;
            int rIdx = (int)(y * _1_cellSize);
            if (rIdx < 0 || rIdx >= numRows) return null;
            int idx = rIdx * numCols + cIdx;
            var c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 6
            ++cIdx;
            if (cIdx >= numCols) return null;
            ++idx;
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 3
            ++rIdx;
            if (rIdx >= numRows) return null;
            idx += numCols;
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 2
            --idx;
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
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
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 4
            idx -= numCols;
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
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
            idx -= numCols;
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 8
            ++idx;
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            // 9
            ++idx;
            c = cells[idx];
            while (c != null)
            {
                var vx = c.x - x;
                var vy = c.y - y;
                var r = c.radius + radius;
                if (vx * vx + vy * vy < r * r)
                {
                    return c;
                }
                c = c.spaceNext;
            }
            return null;
        }

        // 遍历坐标所在格子 + 周围  九宫. handler 返回 true 结束遍历( Func 可能产生 gc, 但这种应该是无所谓的, 里面只要不含 unity 资源 )
        public void Foreach9All(float x, float y, Func<SWGridItem, bool> handler)
        {
            // 5
            int cIdx = (int)(x * _1_cellSize);
            if (cIdx < 0 || cIdx >= numCols) return;
            int rIdx = (int)(y * _1_cellSize);
            if (rIdx < 0 || rIdx >= numRows) return;
            int idx = rIdx * numCols + cIdx;
            var c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 6
            ++cIdx;
            if (cIdx >= numCols) return;
            ++idx;
            c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 3
            ++rIdx;
            if (rIdx >= numRows) return;
            idx += numCols;
            c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 2
            --idx;
            c = cells[idx];
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
            c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 4
            idx -= numCols;
            c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 7
            rIdx -= 2;
            if (rIdx < 0) return;
            idx -= numCols;
            c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 8
            ++idx;
            c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
            // 9
            ++idx;
            c = cells[idx];
            while (c != null)
            {
                var next = c.spaceNext;
                if (handler(c)) return;
                c = next;
            }
        }


        // 圆形扩散遍历找出 边距最近的 1 个并返回
        public SWGridItem FindNearestByRange(SWSpaceRingDiffuseData d, float x, float y, float maxDistance)
        {
            int cIdxBase = (int)(x * _1_cellSize);
            if (cIdxBase < 0 || cIdxBase >= numCols) return null;
            int rIdxBase = (int)(y * _1_cellSize);
            if (rIdxBase < 0 || rIdxBase >= numRows) return null;
            var searchRange = maxDistance + cellSize;

            SWGridItem rtv = null;
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
                    if (cIdx < 0 || cIdx >= numCols) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= numRows) continue;
                    var cidx = rIdx * numCols + cIdx;

                    var c = cells[cidx];
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
                        c = c.spaceNext;
                    }
                }
                if (lens[i].radius > searchRange) break;
            }
            return rtv;
        }


        // 圆形扩散遍历 找出范围内 ??? 最多 n 个 的结果容器
        public List<SWDistanceGridItem> result_FindNearestN = new();

        // 圆形扩散遍历 找出范围内 边缘最近的 最多 n 个, 返回实际个数.searchRange 决定了要扫多远的格子. maxDistance 限制了结果集最大边距
        public int FindNearestNByRange(SWSpaceRingDiffuseData d, float x, float y, float maxDistance, int n)
        {
            int cIdxBase = (int)(x * _1_cellSize);
            if (cIdxBase < 0 || cIdxBase >= numCols) return 0;
            int rIdxBase = (int)(y * _1_cellSize);
            if (rIdxBase < 0 || rIdxBase >= numRows) return 0;
            var searchRange = maxDistance + cellSize;

            var os = result_FindNearestN;
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
                    if (cIdx < 0 || cIdx >= numCols) continue;
                    var rIdx = rIdxBase + tmp.y;
                    if (rIdx < 0 || rIdx >= numRows) continue;
                    var cidx = rIdx * numCols + cIdx;

                    var c = cells[cidx];
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
                                os.Add(new SWDistanceGridItem { distance = v, item = c });
                                if (os.Count == n)
                                {
                                    Quick_Sort(0, os.Count - 1);
                                }
                            }
                            else
                            {
                                if (os[0].distance < v)
                                {
                                    os[0] = new SWDistanceGridItem { distance = v, item = c };
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

        // 排序 result_FindNearestN_2D               .Sort(); 函数会造成 128 byte gc
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

    public struct SWSpaceCountRadius
    {
        public int count;
        public float radius;
    };

    public struct SWSpaceXYi
    {
        public int x, y;
    }

    public struct SWDistanceGridItem : IComparable<SWDistanceGridItem>
    {
        public float distance;
        public SWGridItem item;

        public int CompareTo(SWDistanceGridItem o)
        {
            return this.distance.CompareTo(o.distance);
        }
    }

    // 填充 圆形扩散的 格子偏移量 数组. 主用于 更高效的范围内找最近
    public class SWSpaceRingDiffuseData
    {
        public List<SWSpaceCountRadius> lens = new();
        public List<SWSpaceXYi> idxys = new();

        public SWSpaceRingDiffuseData(int gridNumRows, int cellSize)
        {
            lens.Add(new SWSpaceCountRadius { count = 0, radius = 0f });
            idxys.Add(new SWSpaceXYi());
            HashSet<ulong> set = new();
            set.Add(0);
            for (float radius = 0; radius < cellSize * gridNumRows; radius += cellSize)
            {
                var lenBak = idxys.Count;
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
                        idxys.Add(new SWSpaceXYi { x = ix, y = iy });
                    }
                }
                if (idxys.Count > lenBak)
                {
                    lens.Add(new SWSpaceCountRadius { count = idxys.Count, radius = radius });
                }
            }
        }

    }
}