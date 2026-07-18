using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellSpace;

namespace MMWorld
{
    /// <summary>
    /// 地图数据 - 存储单个地图的信息
    /// </summary>
    public class MapData
    {
        /// <summary>
        /// 地图ID(对应HexTile的ID)
        /// </summary>
        public int tileId;

        /// <summary>
        /// 地图宽度
        /// </summary>
        public int width;

        /// <summary>
        /// 地图高度
        /// </summary>
        public int height;

        /// <summary>
        /// 地图团块
        /// </summary>
        public CellChunk chunk;

        /// <summary>
        /// 地图创建时间
        /// </summary>
        public float createTime;

        /// <summary>
        /// 地图是否已修改(用于判断是否需要保存)
        /// </summary>
        public bool isDirty;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MapData(int tileId, int width, int height)
        {
            this.tileId = tileId;
            this.width = width;
            this.height = height;
            this.chunk = null;
            this.createTime = Time.time;
            this.isDirty = false;
        }
    }

    /// <summary>
    /// 地图索引管理器 - 管理HexTile与CellSpace地图的映射关系
    /// 每个HexTile对应一个CellSpace地图
    /// </summary>
    public class MapIndex : MonoBehaviour
    {
        #region 单例

        /// <summary>
        /// 单例实例
        /// </summary>
        public static MapIndex Instance { get; private set; }

        #endregion

        #region 地图数据

        /// <summary>
        /// 地图字典 - TileID -> 地图数据
        /// </summary>
        private Dictionary<int, MapData> maps = new Dictionary<int, MapData>();

        /// <summary>
        /// 当前激活的地图ID
        /// </summary>
        private int activeMapId = -1;

        /// <summary>
        /// 当前激活的地图团块
        /// </summary>
        private CellChunk activeChunk;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // 确保CellSpacePrefab已初始化
            if (!CellSpace.CellSpacePrefab.initialized)
            {
                CellSpace.CellSpacePrefab.Init();
            }
        }

        #endregion

        #region 地图管理

        /// <summary>
        /// 检查地图是否存在
        /// </summary>
        public bool HasMap(int tileId)
        {
            return maps.ContainsKey(tileId);
        }

        /// <summary>
        /// 创建新地图
        /// </summary>
        public IEnumerator CreateMap(int tileId, int width, int height)
        {
            if (maps.ContainsKey(tileId))
            {
                Debug.LogWarning($"[MapIndex] 地图 {tileId} 已存在!");
                yield break;
            }

            Debug.Log($"[MapIndex] 创建地图 {tileId}: {width}x{height}");

            // 创建地图数据
            MapData mapData = new MapData(tileId, width, height);

            // 使用CellSpacePrefab创建CellChunk预制体实例
            // 注意:CellChunk的位置决定了它的ChunkIndex,ChunkIndex = transform.position
            // 对于256x256地图,我们需要计算位置:每个chunk是16x16x16单元
            GameObject chunkObj = Instantiate(CellSpace.CellSpacePrefab.CellChunk);
            chunkObj.name = $"Map_{tileId}";

            // 设置团块位置(CellChunk的Awake会根据位置自动设置ChunkIndex)
            // tileId 作为团块的x坐标索引
            chunkObj.transform.position = new Vector3(tileId * CPEngine.chunkSideLength, 0, 0);
            chunkObj.SetActive(true);

            CellChunk chunk = chunkObj.GetComponent<CellChunk>();

            // 等待团块初始化完成(等待Awake执行完毕)
            yield return new WaitUntil(() => chunk.chunkIndex != null && chunk.cellData != null);

            // 填充随机草地或土
            yield return StartCoroutine(FillTerrainData(chunk, width, height));

            // 等待团块生成完成
            while (!chunk.cellsDone)
            {
                yield return null;
            }

            // 保存地图数据
            mapData.chunk = chunk;
            maps.Add(tileId, mapData);

            Debug.Log($"[MapIndex] 地图 {tileId} 创建完成! 大小: {width}x{height}");
        }

        /// <summary>
        /// 填充地形数据 - 随机草地或土
        /// </summary>
        private IEnumerator FillTerrainData(CellChunk chunk, int width, int height)
        {
            // Cell ID: 0 = 草地, 1 = 土 (或其他定义)
            int totalCells = chunk.cellData.Length;
            for (int i = 0; i < totalCells; i++)
            {
                // 随机选择草地(0)或土(1)
                chunk.cellData[i] = (ushort)(Random.value > 0.5f ? 0 : 1);

                // 每帧处理一部分,避免卡顿
                if (i % 10000 == 0)
                {
                    yield return null;
                }
            }

            // 标记团块需要更新
            chunk.flaggedToUpdate = true;
        }

        /// <summary>
        /// 切换到指定地图
        /// </summary>
        public void SwitchToMap(int tileId)
        {
            if (!maps.ContainsKey(tileId))
            {
                Debug.LogError($"[MapIndex] 地图 {tileId} 不存在,无法切换!");
                return;
            }

            // 隐藏当前地图
            if (activeChunk != null)
            {
                activeChunk.gameObject.SetActive(false);
            }

            // 显示新地图
            MapData mapData = maps[tileId];
            if (mapData.chunk != null)
            {
                mapData.chunk.gameObject.SetActive(true);
                activeChunk = mapData.chunk;
                activeMapId = tileId;

                // 更新玩家位置到新地图中心
                UpdatePlayerPosition(tileId, mapData.width, mapData.height);

                Debug.Log($"[MapIndex] 已切换到地图 {tileId}");
            }
            else
            {
                Debug.LogError($"[MapIndex] 地图 {tileId} 的团块为空!");
            }
        }

        /// <summary>
        /// 更新玩家位置到地图中心
        /// </summary>
        private void UpdatePlayerPosition(int tileId, int width, int height)
        {
            if (GameManager.Instance != null && GameManager.Instance.player != null)
            {
                Vector3 newPos = new Vector3(width / 2f, 10f, height / 2f);
                GameManager.Instance.player.transform.position = newPos;
                Debug.Log($"[MapIndex] 玩家移动到地图 {tileId} 中心: {newPos}");
            }
        }

        /// <summary>
        /// 获取当前地图ID
        /// </summary>
        public int GetActiveMapId()
        {
            return activeMapId;
        }

        /// <summary>
        /// 获取当前地图团块
        /// </summary>
        public CellChunk GetActiveChunk()
        {
            return activeChunk;
        }

        /// <summary>
        /// 获取地图数量
        /// </summary>
        public int GetMapCount()
        {
            return maps.Count;
        }

        #endregion

        #region 存档功能

        /// <summary>
        /// 保存地图到文件
        /// </summary>
        public void SaveMap(int tileId)
        {
            if (!maps.ContainsKey(tileId))
            {
                Debug.LogError($"[MapIndex] 地图 {tileId} 不存在,无法保存!");
                return;
            }

            MapData mapData = maps[tileId];
            if (mapData.chunk != null && mapData.chunk.cellData != null)
            {
                // 保存逻辑(使用CellChunkDataFiles组件的实例方法)
                CellChunkDataFiles dataFiles = mapData.chunk.GetComponent<CellChunkDataFiles>();
                if (dataFiles != null)
                {
                    dataFiles.SaveData();
                    mapData.isDirty = false;
                    Debug.Log($"[MapIndex] 地图 {tileId} 已保存!");
                }
                else
                {
                    Debug.LogError($"[MapIndex] 地图 {tileId} 的团块没有CellChunkDataFiles组件!");
                }
            }
        }

        /// <summary>
        /// 从文件加载地图
        /// </summary>
        public bool LoadMap(int tileId)
        {
            // TODO: 实现从文件加载地图
            Debug.Log($"[MapIndex] 地图 {tileId} 加载功能待实现!");
            return false;
        }

        #endregion
    }
}