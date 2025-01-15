using MMWorld;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 自定义地形布置（进入布置前团块里的单元都是空块）
    /// </summary>
    public class CPCustomTerrainGenerator : CPTerrainGenerator
    {
        /// <summary>
        /// mapID=0为大地图，小地图从1~239开始（1是拉多镇），240是龙珠大地图测试
        /// </summary>
        /// <param name="mapID"></param>
        public void LoadMap(int mapID)
        {
            int i = -1;
            if (mapID == 0)
            {//刷大地图
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[0][i] + 10));//重装机兵大地图第一个纹理编号从11开始
                        }
                    }
                }
            }
            else if (mapID > 0 && mapID < 240)
            {//刷小地图
                int width = CPEngine.mapWidths[mapID - 1];//拉多是mapId=1，格子宽度=mapWidths[0]
                int currentX = 0; // 当前列的索引
                int currentY = 0; // 当前行的索引

                // 由于我们不知道总格子数，我们将使用一个条件来检查是否应该停止
                bool shouldStop = false;

                while (!shouldStop)
                {
                    i++; // 增加计数
                    if (CPEngine.HorizontalMode)
                    {
                        chunk.SetCellSimple(currentX, currentY, (ushort)(CPEngine.mapIDs[mapID][i] + 162));//重装机兵小地图第一个纹理编号从163开始
                    }
                    currentX++;

                    // 如果达到行宽，则换行
                    if (currentX >= width)
                    {
                        currentX = 0; // 重置列索引
                        currentY++;   // 增加行索引

                        // 检查是否应该停止
                        if (i + 1 >= CPEngine.mapIDs[mapID].Count) //如拉多的格子数是384，达到就停止
                        {
                            shouldStop = true;
                        }
                    }
                }
            }
            else if (mapID == 240)
            {//刷龙珠大地图
                for (int y = 0; y < 349; y++)
                {
                    for (int x = 0; x < 512; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(CPEngine.mapIDs[240][i] + 1522));//龙珠大地图第一个纹理编号从1523开始
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 0是大地图纹理，1是小地图纹理
        /// </summary>
        /// <param name="textureID"></param>
        public void LoadTexture(int textureID)
        {
            int i = -1;
            if (textureID == 0)
            {//刷大地图纹理
                for (int y = 0; y < 19; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(x + 1 + 8 * y + 10));//重装机兵大地图第一个纹理编号从11开始
                        }
                    }
                }
            }
            else if (textureID == 1)
            {//刷小地图纹理
                for (int y = 0; y < 170; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCellSimple(x, y, (ushort)(x + 8 * y + 163));//重装机兵大地图第一个纹理编号从163开始
                        }
                    }
                }
            }
        }

        public override void GenerateCellData()
        {
            if (!CPEngine.userCellChunks.Contains(chunk))
            {
                CPEngine.userCellChunks.Add(chunk);
            }
            if (GameObject.Find("GameMain").GetComponent<TextureColliderSet>().enabled)
            {//开启碰撞设置模式，刷默认纹理图
                LoadTexture(1);
            }
            else
            {//加载正常游戏地图
                LoadMap(1);
            }
        }
    }
}