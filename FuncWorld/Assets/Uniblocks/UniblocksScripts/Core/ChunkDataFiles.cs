using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Uniblocks
{
    /// <summary>
    /// 处理团块内体素数据的加载和保存。
    /// </summary>
    public class ChunkDataFiles : MonoBehaviour
    {
        /// <summary>
        /// 正在存储团块内体素数据
        /// </summary>
        public static bool SavingChunks;

        /// <summary>
        /// 临时存储着团块内体素数据，以便稍后写入区域文件
        /// </summary>
        public static Dictionary<string, string> TempChunkData; // stores chunk's data to write into a region file later

        /// <summary>
        /// 当前加载区域的数据
        /// </summary>
        public static Dictionary<string, string[]> LoadedRegions; // data of currently loaded regions

        /// <summary>
        /// 尝试从文件加载团块内体素数据，如果未找到数据则返回false。
        /// </summary>
        /// <returns>如果团块内体素数据为空则返回false，反之加载有效返回true</returns>
        public bool LoadData()
        { // attempts to load data from file, returns false if data is not found

            Chunk chunk = GetComponent<Chunk>();//获取团块组件实例对象
            //根据团块索引获取团块内体素数据
            string chunkData = GetChunkData(chunk.ChunkIndex);
            //如果团块内体素数据不为空
            if (chunkData != "")
            {
                //从先前压缩的字符串中解压缩出团块数据，并将它保存到团块的VoxelData数组中。
                DecompressData(chunk, GetChunkData(chunk.ChunkIndex));
                //当此团块完成生成或加载体素数据后VoxelsDone置为True
                chunk.VoxelsDone = true;
                return true;
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// 保存团块内体素数据到硬盘。
        /// </summary>
        public void SaveData()
        {
            //获取团块组件实例对象
            Chunk chunk = GetComponent<Chunk>();
            //压缩特定团块内体素数据并将其作为字符串返回。
            string compressedData = CompressData(chunk);
            //将压缩后的数据字符串写入TempChunkData字典
            WriteChunkData(chunk.ChunkIndex, compressedData);
        }

        /// <summary>
        /// 从先前压缩的特定团块内体素数据字符串中解压缩出团块数据，并将它保存到团块的VoxelData数组中。
        /// </summary>
        /// <param name="chunk">团块</param>
        /// <param name="data">先前压缩的特定团块内体素数据字符串</param>
        public static void DecompressData(Chunk chunk, string data)
        { // decompresses voxel data and loads it into the VoxelData array

            // check if chunk is empty.检查先前压缩的特定团块内体素数据字符串是否为空
            if (data.Length == 2 && data[1] == (char)0)
            {
                //设置团块是空的状态
                chunk.Empty = true;
            }
            //创建了一个 StringReader 对象，将其初始化为字符串 data
            StringReader reader = new StringReader(data);

            int i = 0;
            int length = chunk.GetDataLength(); // length of VoxelData array.体素数据数组的长度

            try
            {
                while (i < length)
                { // this loop will stop once the VoxelData array has been populated completely. Iterates once per count-data block.
                  // 这个循环将在VoxelData数组被完全填充后停止，每个count-data体素块仅迭代一次。

                    //调用reader.Read()方法读取字符串中的下一个字符。由于reader.Read()方法返回读取的字符的Unicode编码，可使用类型转换将返回的值转换为ushort。
                    //这是因为Unicode编码使用两个字节表示每个字符，与ushort数据类型相同。因此代码片段将不断读取字符串data中的每个字符，并将其作为ushort值返回。

                    ushort currentCount = (ushort)reader.Read(); // read the count.读取下个字符转为整数（首次读取则为第一个字符），这个整数代表体素的数组索引
                    ushort currentData = (ushort)reader.Read(); // read the data.读取下个字符转为整数，这个整数代表体素数据（体素块的种类）

                    int ii = 0;

                    while (ii < currentCount)
                    {
                        //如此不断循环更改指定数组索引处的体素数据（即修改体素块的种类），函数采用平面1D数组索引作为参数（指i）。
                        chunk.SetVoxelSimple(i, currentData);// write a single voxel for every currentCount.如果团块内大量连续索引的体素块是同一种，压缩时currentCount计数会较大，在此处产生循环处理
                        ii++; //处理直到连续索引的体素块都设置好种类
                        i++; //最终的i肯定不会超过一个团块拥有的体素数据数组的长度的，每次处理后i也要+1
                    }
                }
            }
            catch (System.Exception)
            {
                //只有一种情况就是存储团块的数据损坏（可能被第三方改档或使用不同的团块大小进行了保存数据）
                Debug.LogError("Uniblocks: Corrupt chunk data for chunk: " + chunk.ChunkIndex.ToString() + ". Has the data been saved using a different chunk size?");
                reader.Close();
                return;
            }

            reader.Close();

        }

        /// <summary>
        /// 压缩特定团块内体素数据并将其作为字符串返回。
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static string CompressData(Chunk chunk)
        { // returns the data of chunk in compressed string format

            StringWriter writer = new StringWriter();

            int i = 0;
            int length = chunk.GetDataLength(); // length of VoxelData array

            ushort currentCount = 0; // count of consecutive voxels of the same type
            ushort currentData = 0; // data of the current voxel

            for (i = 0; i < length; i++)
            { // for each voxel

                ushort thisData = chunk.GetVoxelSimple(i); // read raw data at i

                if (thisData != currentData)
                { // if the data is different from the previous data, write the last block and start a new one

                    // write previous block 
                    if (i != 0)
                    { // (don't write in the first loop iteration, because count would be 0 (no previous blocks))
                        writer.Write((char)currentCount);
                        writer.Write((char)currentData);
                    }
                    // start new block
                    currentCount = 1;
                    currentData = thisData;
                }

                else
                { // if the data is the same as the last data, simply add to the count
                    currentCount++;
                }

                if (i == length - 1)
                { // if this is the last iteration of the loop, close and write the current block
                    writer.Write((char)currentCount);
                    writer.Write((char)currentData);
                }

            }

            string compressedData = writer.ToString();
            writer.Flush(); //使用Flush来保证写入（防止程序异常崩溃等情况导致写入错误）
            writer.Close();
            return compressedData;

        }

        private string GetChunkData(Index index)
        { // returns the chunk data (from memory or from file), or an empty string if data can't be found

            // try to load from TempChunkData
            string indexString = index.ToString();
            if (TempChunkData.ContainsKey(indexString))
            {
                return TempChunkData[indexString];
            }

            // try to load from region, return empty if not found
            int regionIndex = GetChunkRegionIndex(index);
            string[] regionData = GetRegionData(GetParentRegion(index));
            if (regionData == null)
            {
                return "";
            }
            return regionData[regionIndex];

        }

        /// <summary>
        /// 将压缩后的数据字符串写入TempChunkData字典
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        private void WriteChunkData(Index index, string data)
        { // writes the chunk data to the TempChunkData dictionary
            TempChunkData[index.ToString()] = data;
        }

        private static int GetChunkRegionIndex(Index index)
        { // returns the 1d index of a chunk's data in the region file

            Index newIndex = new Index(index.x, index.y, index.z);
            if (newIndex.x < 0) newIndex.x = -newIndex.x - 1;
            if (newIndex.y < 0) newIndex.y = -newIndex.y - 1;
            if (newIndex.z < 0) newIndex.z = -newIndex.z - 1;

            int flatIndex = (newIndex.z * 100) + (newIndex.y * 10) + newIndex.x;

            while (flatIndex > 999)
            {
                flatIndex -= 1000;
            }

            return flatIndex;
        }

        private static string[] GetRegionData(Index regionIndex)
        { // loads region data and from file returns it, or returns null if region file is not found

            if (LoadRegionData(regionIndex) == true)
            {
                return LoadedRegions[regionIndex.ToString()];
            }
            else
            {
                return null;
            }
        }

        private static bool LoadRegionData(Index regionIndex)
        { // loads the region data into memory if file exists and it's not already loaded, returns true if data exists (and is loaded), else false

            string indexString = regionIndex.ToString();
            if (LoadedRegions.ContainsKey(indexString) == false)
            { // if not loaded

                // load data if region file exists
                string regionPath = GetRegionPath(regionIndex);
                if (File.Exists(regionPath))
                {

                    StreamReader reader = new StreamReader(regionPath);
                    string[] regionData = reader.ReadToEnd().Split((char)ushort.MaxValue);
                    reader.Close();
                    LoadedRegions[indexString] = regionData;

                    return true;

                }

                else
                {
                    return false; // return false if region file doesn't exist
                }
            }

            return true; // return true if data is already loaded
        }

        private static string GetRegionPath(Index regionIndex)
        {

            return Engine.WorldPath + (regionIndex.ToString() + ",.region");
        }

        private static Index GetParentRegion(Index index)
        { // returns the index of the region containing a specific chunk

            Index newIndex = new Index(index.x, index.y, index.z);

            if (index.x < 0) newIndex.x -= 9;
            if (index.y < 0) newIndex.y -= 9;
            if (index.z < 0) newIndex.z -= 9;

            int x = newIndex.x / 10;
            int y = newIndex.y / 10;
            int z = newIndex.z / 10;

            return new Index(x, y, z);
        }

        private static void CreateRegionFile(Index index)
        { // creates an empty region file

            Directory.CreateDirectory(Engine.WorldPath);
            StreamWriter writer = new StreamWriter(GetRegionPath(index));

            for (int i = 0; i < 999; i++)
            {
                writer.Write((char)ushort.MaxValue);
            }

            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// 使用Unity自带协程（异步）进行存档
        /// </summary>
        /// <returns></returns>
        public static IEnumerator SaveAllChunks()
        {

            if (!Engine.SaveVoxelData)
            {
                //设置里被禁止存档时，进行协程终止
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
                yield break;
            }

            //WaitForEndOfFrame直到SavingChunks=false
            while (SavingChunks)
            {
                //在每帧屏幕所有相机和GUI被渲染完成前等待（确保当前帧网格对象全被创造出来后才能进行保存）
                yield return new WaitForEndOfFrame();
            }
            SavingChunks = true;

            // for each chunk object, save data to memory
            int count = 0;
            List<Chunk> chunksToSave = new List<Chunk>(ChunkManager.Chunks.Values);  //字典中的每个团块


            foreach (Chunk chunk in chunksToSave)
            {
                //调用团块实例对象的ChunkDataFiles组件（脚本）内的Save方法
                chunk.gameObject.GetComponent<ChunkDataFiles>().SaveData();
                count++;
                if (count > Engine.MaxChunkSaves)
                {
                    //超过团块存储上限则在每帧屏幕所有相机和GUI被渲染完成前等待之后再步进（应该是限制每帧存储动作数量）
                    yield return new WaitForEndOfFrame();
                    count = 0;
                }
            }

            //从内存写入本地存档
            WriteLoadedChunks();
            SavingChunks = false; //保存动作彻底完成

            Debug.Log("Uniblocks: World saved successfully.");
        }

        /// <summary>
        /// writes data from TempChunkData into region files
        /// </summary>
        public static void SaveAllChunksInstant()
        {
            if (!Engine.SaveVoxelData)
            {
                //设置里的保存体素数据没有允许
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
                return;
            }

            // for each chunk object, save data to memory
            foreach (Chunk chunk in ChunkManager.Chunks.Values)
            {
                chunk.gameObject.GetComponent<ChunkDataFiles>().SaveData();
            }

            //从内存写入本地存档
            WriteLoadedChunks();

            Debug.Log("Uniblocks: World saved successfully. (Instant)");

        }

        /// <summary>
        /// writes all chunk data from memory to disk, and clears memory
        /// </summary>
        public static void WriteLoadedChunks()
        {
            // for every chunk loaded in dictionary
            foreach (string chunkIndex in TempChunkData.Keys)
            {

                Index index = Index.FromString(chunkIndex);
                string region = GetParentRegion(index).ToString();

                // check if region is loaded, and load it if it's not
                if (LoadRegionData(GetParentRegion(index)) == false)
                {
                    CreateRegionFile(GetParentRegion(index));
                    LoadRegionData(GetParentRegion(index));
                }

                // write chunk data into region dictionary
                int chunkRegionIndex = GetChunkRegionIndex(index);
                LoadedRegions[region][chunkRegionIndex] = TempChunkData[chunkIndex];
            }
            TempChunkData.Clear();


            // for every region loaded in dictionary
            foreach (string regionIndex in LoadedRegions.Keys)
            {
                WriteRegionFile(regionIndex);
            }
            LoadedRegions.Clear();

        }

        private static void WriteRegionFile(string regionIndex)
        {

            string[] regionData = LoadedRegions[regionIndex];

            StreamWriter writer = new StreamWriter(GetRegionPath(Index.FromString(regionIndex)));
            int count = 0;
            foreach (string chunk in regionData)
            {
                writer.Write(chunk);
                if (count != regionData.Length - 1)
                {
                    writer.Write((char)ushort.MaxValue);
                }
                count++;
            }

            writer.Flush();
            writer.Close();
        }
    }

}