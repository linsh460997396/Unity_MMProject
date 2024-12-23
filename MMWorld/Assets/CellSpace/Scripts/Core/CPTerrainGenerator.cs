using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 地形生成器基类
    /// </summary>
    public class CPTerrainGenerator : MonoBehaviour
    {
        /// <summary>
        /// 团块
        /// </summary>
        protected CellChunk chunk;

        /// <summary>
        /// 世界种子
        /// </summary>
        protected int seed;

        /// <summary>
        /// 初始化地形生成器
        /// </summary>
        public void InitializeGenerator()
        {
            // load seed if it's not loaded yet.加载种子文件若还没进行
            while (CPEngine.WorldSeed == 0)
            {
                //从文件中读取当前活动世界的种子，或者如果没有找到种子文件则随机生成一个新的种子，并将其存储在Engine.WorldSeed变量中。
                CPEngine.GetSeed();
            }
            seed = CPEngine.WorldSeed;
            // get chunk component.获取团块组件
            chunk = GetComponent<CellChunk>();
            // generate data.生成体素数据
            GenerateCellData();
            // set empty.设置团块为空状态
            chunk.Empty = true;
            //遍历团块中的每一个体素ID（体素块种类）
            foreach (ushort cellID in chunk.CellData)
            {
                //只要有任意一个体素ID（体素块种类）不为空块
                if (cellID != 0)
                {
                    //团块的空属性置为否
                    chunk.Empty = false;
                    break;
                }
            }
            // flag as done.团块完成生成或加载体素数据后CellsDone属性置为True
            chunk.CellsDone = true;
        }

        public virtual void GenerateCellData()
        {
            //虚函数，待实现
        }
    }

}