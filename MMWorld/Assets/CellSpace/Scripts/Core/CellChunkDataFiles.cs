using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CellSpace
{
    /// <summary>
    /// 团块数据文件组件：处理团块的单元数据的加载和保存。
    /// </summary>
    public class CellChunkDataFiles : MonoBehaviour
    {
        /// <summary>
        /// 正在存储团块的单元数据
        /// </summary>
        public static bool SavingChunks;

        /// <summary>
        /// 该数组临时存储着所有团块数据，以便稍后写入区域文件
        /// </summary>
        public static Dictionary<string, string> TempChunkData; // stores chunk's data to write into a region file later

        /// <summary>
        /// 当前已加载区域组
        /// </summary>
        public static Dictionary<string, string[]> LoadedRegions; // data of currently loaded regions

        /// <summary>
        /// 尝试从文件加载团块的单元数据，如果未找到数据则返回false。
        /// </summary>
        /// <returns>如果团块的单元数据为空则返回false，反之加载有效返回true</returns>
        public bool LoadData()
        { // attempts to load data from file, returns false if data is not found

            CellChunk chunk = GetComponent<CellChunk>();//获取团块组件实例对象
            //根据团块索引获取团块的单元数据
            string chunkData = GetChunkData(chunk.ChunkIndex);
            //如果团块的单元数据不为空
            if (chunkData != "")
            {
                //从先前压缩的字符串中解压缩出团块数据，并将它保存到团块的CellData数组中。
                DecompressData(chunk, GetChunkData(chunk.ChunkIndex));
                //当此团块完成生成或加载单元数据后CellsDone置为True
                chunk.CellsDone = true;
                return true;
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// 保存团块的单元数据到内存（TempChunkData）
        /// </summary>
        public void SaveData()
        {
            //获取团块组件实例对象
            CellChunk chunk = GetComponent<CellChunk>();
            //压缩特定团块的单元数据并将其作为字符串返回。
            string compressedData = CompressData(chunk);
            //将压缩后的数据字符串写入TempChunkData字典
            WriteChunkData(chunk.ChunkIndex, compressedData);
        }

        /// <summary>
        /// 从先前压缩的特定团块的单元数据字符串中解压缩出团块数据，并将它保存到团块的CellData数组中。
        /// </summary>
        /// <param name="chunk">团块</param>
        /// <param name="data">先前压缩的特定团块的单元数据字符串</param>
        public static void DecompressData(CellChunk chunk, string data)
        { // decompresses cell data and loads it into the CellData array

            // check if chunk is empty.检查先前压缩的特定团块的单元数据字符串是否为空
            if (data.Length == 2 && data[1] == (char)0)
            {
                //设置团块是空的状态
                chunk.Empty = true;
            }
            //创建了一个 StringReader 对象，将其初始化为字符串 data
            StringReader reader = new StringReader(data);

            int i = 0;
            int length = chunk.GetDataLength(); // length of CellData array.单元数据数组的长度

            try
            {
                while (i < length)
                { // this loop will stop once the CellData array has been populated completely. Iterates once per count-data block.
                  // 这个循环将在CellData数组被完全填充后停止，每个count-data单元仅迭代一次。

                    //调用reader.Read()方法读取字符串中的下一个字符。由于reader.Read()方法返回读取的字符的Unicode编码，可使用类型转换将返回的值转换为ushort。
                    //这是因为Unicode编码使用两个字节表示每个字符，与ushort数据类型相同。因此代码片段将不断读取字符串data中的每个字符，并将其作为ushort值返回。

                    ushort currentCount = (ushort)reader.Read(); // read the count.读取下个字符转为整数（首次读取则为第一个字符），这个整数代表单元的数组索引
                    ushort currentData = (ushort)reader.Read(); // read the data.读取下个字符转为整数，这个整数代表单元数据（单元的种类）

                    int ii = 0;

                    while (ii < currentCount)
                    {
                        //如此不断循环更改指定数组索引处的单元数据（即修改单元的种类），函数采用平面1D数组索引作为参数（指i）。
                        chunk.SetCellSimple(i, currentData);// write a single voxel for every currentCount.如果团块内大量连续索引的单元是同一种，压缩时currentCount计数会较大，在此处产生循环处理
                        ii++; //处理直到连续索引的单元都设置好种类
                        i++; //最终的i肯定不会超过一个团块拥有的单元数据数组的长度的，每次处理后i也要+1
                    }
                }
            }
            catch (System.Exception)
            {
                //只有一种情况就是存储团块的数据损坏（可能被第三方改档或使用不同的团块大小进行了保存数据）
                Debug.LogError("CellSpace: Corrupt chunk data for chunk: " + chunk.ChunkIndex.ToString() + ". Has the data been saved using a different chunk size?");
                reader.Close();
                return;
            }

            reader.Close();

        }

        /// <summary>
        /// 压缩特定团块的单元数据并将其作为字符串返回。
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static string CompressData(CellChunk chunk)
        { // returns the data of chunk in compressed string format

            StringWriter writer = new StringWriter();

            int i = 0;
            int length = chunk.GetDataLength(); // length of CellData array.团块的单元数据数组长度

            ushort currentCount = 0; // count of consecutive voxels of the same type.待处理的连续索引的同种类的单元数量
            ushort currentData = 0; // data of the current voxel.当前处理的单元应赋予的种类

            for (i = 0; i < length; i++)
            { // for each voxel

                ushort thisData = chunk.GetCellSimple(i); // read raw data at i.读取团块的1维索引下的单元种类

                if (thisData != currentData) //如果团块的单元种类跟当前应赋予的种类不同
                { // if the data is different from the previous data, write the last block and start a new one

                    // write previous block 
                    if (i != 0)
                    { // (don'transform write in the first loop iteration, because count would be 0 (no previous blocks))
                        //不要在第一次循环迭代中写入，因为count为0(没有之前的单元记录)
                        writer.Write((char)currentCount);
                        writer.Write((char)currentData);
                    }
                    // start new block.进行实际单元的记录
                    currentCount = 1;
                    currentData = thisData;
                }

                else
                { // if the data is the same as the last data, simply add to the count.如果数据与最后一个数据相同，只需添加计数
                    currentCount++;
                }

                if (i == length - 1)
                { // if this is the last iteration of the loop, close and write the current block.如果这是循环的最后一次迭代，关闭并写入当前单元
                    writer.Write((char)currentCount);
                    writer.Write((char)currentData);
                }

            }

            string compressedData = writer.ToString();
            writer.Flush(); //使用Flush来保证写入（防止程序异常崩溃等情况导致写入错误）
            writer.Close();
            return compressedData;

        }

        /// <summary>
        /// 返回团块数据(来自内存或文件)，如果找不到数据则返回空字符串
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetChunkData(CPIndex index)
        { // returns the chunk data (from memory or from file), or an empty string if data can'transform be found

            // try to load from TempChunkData.尝试从TempChunkData加载
            string indexString = index.ToString();
            if (TempChunkData.ContainsKey(indexString))
            {
                //如果TempChunkData字典存在index键，返回其对应的值（团块的单元数据）
                return TempChunkData[indexString];
            }

            // try to load from region, return empty if not found.尝试从区域加载，如果未找到则返回空
            int regionIndex = GetChunkRegionIndex(index); //获取团块索引对应区域文件中存储的团块数据数组的一维索引（即团块在区域中属于第几个元素）
            //获取区域的团块数据种类的数组集合
            string[] regionData = GetRegionData(GetParentRegion(index));
            if (regionData == null)
            {
                return "";
            }
            return regionData[regionIndex];

        }

        /// <summary>
        /// 将压缩后的数据字符串写入TempChunkData字典（存储着团块的单元数据）
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <param name="data">压缩后团块的单元数据</param>
        private void WriteChunkData(CPIndex index, string data)
        { // writes the chunk data to the TempChunkData dictionary
            TempChunkData[index.ToString()] = data;
        }

        /// <summary>
        /// 返回团块索引对应区域文件中存储的团块数据数组的一维索引（即团块在区域中属于第几个元素）。存储时，区域边长设定为10，一个区域存储1K个团块（2D模式下为100个）。
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns></returns>
        private static int GetChunkRegionIndex(CPIndex index)
        { // returns the 1d index of a chunk's data in the region file
            int flatIndex;
            CPIndex newIndex = new CPIndex(index.x, index.y, index.z);
            //如果团块索引是负的，以数据原点进行纠正，方便后面计算数组的一维索引
            if (newIndex.x < 0) newIndex.x = -newIndex.x - 1;
            if (newIndex.y < 0) newIndex.y = -newIndex.y - 1;
            if (CPEngine.HorizontalMode)
            {//2D横版模式
                //存储时，区域边长设定为10（存储100个团块），用上面纠正好的新索引计算区域的团块数据数组1D索引
                flatIndex = (newIndex.y * 10) + newIndex.x;
                //保证索引从0~99
                while (flatIndex > 99)
                {
                    flatIndex -= 100;
                }
            }
            else
            {
                if (newIndex.z < 0) newIndex.z = -newIndex.z - 1;
                //存储时，区域边长设定为10（存储1K个团块），用上面纠正好的新索引计算区域的团块数据数组1D索引
                flatIndex = (newIndex.z * 100) + (newIndex.y * 10) + newIndex.x;
                //保证索引从0~999
                while (flatIndex > 999)
                {
                    flatIndex -= 1000;
                }
            }
            return flatIndex;
        }

        /// <summary>
        /// 读取区域索引对应的区域数据成功时，返回区域索引对应的区域（文件名）。
        /// </summary>
        /// <param name="regionIndex">区域索引</param>
        /// <returns>加载区域数据并从文件返回该数据，如果未找到区域文件则返回null</returns>
        private static string[] GetRegionData(CPIndex regionIndex)
        { // loads region data and from file returns it, or returns null if region file is not found
            //读取区域索引对应的区域数据成功时
            if (LoadRegionData(regionIndex) == true)
            {
                //返回区域索引对应的区域（文件名）
                return LoadedRegions[regionIndex.ToString()];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 打开区域索引对应的路径下的文件，读取全部内容并分割进区域数据数组，最后将这个数组作为单个键的值，存储到当前已加载区域组
        /// </summary>
        /// <param name="regionIndex">区域索引</param>
        /// <returns>如果区域文件存在且尚未加载，则将区域数据加载到内存中，如数据存在(且已加载)则返回true，否则返回false</returns>
        private static bool LoadRegionData(CPIndex regionIndex)
        { // loads the region data into memory if file exists and it's not already loaded, returns true if data exists (and is loaded), else false
            //获取区域索引对应的字符串
            string indexString = regionIndex.ToString();
            //如果当前已加载区域组不包含参数指定的区域
            if (LoadedRegions.ContainsKey(indexString) == false)
            { // if not loaded.指定区域还没被加载

                // load data if region file exists.如指定区域的文件存在，则加载数据
                string regionPath = GetRegionPath(regionIndex); //获取指定区域索引对应的区域文件路径
                if (File.Exists(regionPath))
                {
                    StreamReader reader = new StreamReader(regionPath);
                    string temp = reader.ReadToEnd();
                    char splitChar = (char)ushort.MaxValue;
                    string[] regionData = temp.Split(splitChar);
                    reader.Close();
                    LoadedRegions[indexString] = regionData; //将区域数据数组作为单个键的值，存储到当前已加载区域组

                    return true;

                }

                else
                {
                    return false; // return false if region file doesn'transform exist
                }
            }

            return true; // return true if data is already loaded
        }

        /// <summary>
        /// 获取区域索引对应的区域文件路径
        /// </summary>
        /// <param name="regionIndex"></param>
        /// <returns></returns>
        private static string GetRegionPath(CPIndex regionIndex)
        {

            return CPEngine.WorldPath + (regionIndex.ToString() + ",.region");
        }

        /// <summary>
        /// 获取团块索引对应的区域索引
        /// </summary>
        /// <param name="index">团块索引</param>
        /// <returns>返回包含特定团块的区域索引</returns>
        private static CPIndex GetParentRegion(CPIndex index)
        { // returns the index of the region containing a specific chunk
            if (CPEngine.HorizontalMode)
            {
                CPIndex newIndex = new CPIndex(index.x, index.y);
                if (index.x < 0) newIndex.x -= 9;
                if (index.y < 0) newIndex.y -= 9;
                int x = newIndex.x / 10;
                int y = newIndex.y / 10;
                return new CPIndex(x, y);
            }
            else
            {
                CPIndex newIndex = new CPIndex(index.x, index.y, index.z);
                if (index.x < 0) newIndex.x -= 9;
                if (index.y < 0) newIndex.y -= 9;
                if (index.z < 0) newIndex.z -= 9;
                int x = newIndex.x / 10;
                int y = newIndex.y / 10;
                int z = newIndex.z / 10;
                return new CPIndex(x, y, z);
            }
        }

        /// <summary>
        /// 创建一个空的区域文件
        /// </summary>
        /// <param name="index">区域索引</param>
        private static void CreateRegionFile(CPIndex index)
        { // creates an empty region file
            int iMax;
            Directory.CreateDirectory(CPEngine.WorldPath); //创建世界名称文件夹（如果不存在时）
            StreamWriter writer = new StreamWriter(GetRegionPath(index)); //根据区域索引获取区域文件应存在的路径，建立写入流
            if (CPEngine.HorizontalMode)
            {
                iMax = 99;
            }
            else
            {
                iMax = 999;
            }
            for (int i = 0; i < iMax; i++)
            {
                //ushort和char类型数据长度都是16位
                //写入一个65535对应的Char（Unicode中的一个保留字符，它不表示任何特定符号，当尝试显示或读取时应是乱码，因为大多数编码无法正确处理超出其字符范围的值）
                writer.Write((char)ushort.MaxValue); //在没有数据情况下这里预先写入了iMax+1个Char
            }
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// 使用Unity自带协程（异步）进行存档，将团块的单元数据保存到内存和硬盘
        /// </summary>
        /// <returns></returns>
        public static IEnumerator SaveAllChunks()
        {
            if (!CPEngine.SaveCellData)
            {
                //设置里被禁止存档单元数据时，协程终止
                Debug.LogWarning("CellSpace: Saving is disabled. You can enable it in the CPEngine Settings.");
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
            List<CellChunk> chunksToSave = new List<CellChunk>(CellChunkManager.Chunks.Values);  //存储字典中的每个团块

            //遍历字典中的每个团块
            foreach (CellChunk chunk in chunksToSave)
            {
                //获取团块实例对象的ChunkDataFiles组件实例，调用其脚本内的Save方法将团块的单元数据保存到内存
                chunk.gameObject.GetComponent<CellChunkDataFiles>().SaveData();
                count++;
                if (count > CPEngine.MaxChunkSaves)
                {
                    //超过每帧团块数据存储的处理上限则在屏幕所有相机和GUI被渲染完成前等待，下一帧再继续处理
                    yield return new WaitForEndOfFrame();
                    count = 0;
                }
            }

            //从内存写入本地存档
            WriteLoadedChunks();
            SavingChunks = false; //保存动作彻底完成

            Debug.Log("CellSpace: World saved successfully.");
        }

        /// <summary>
        /// 存储所有团块实例：将TempChunkData中的数据写入区域文件
        /// </summary>
        public static void SaveAllChunksInstant()
        {
            //禁用存储单元数据时
            if (!CPEngine.SaveCellData)
            {
                //警告：设置里的保存单元数据没有允许
                Debug.LogWarning("CellSpace: Saving is disabled. You can enable it in the CPEngine Settings.");
                return;
            }

            // for each chunk object, save data to memory.遍历每个团块对象，将其数据保存到内存（TempChunkData）中（同时方便快速加载）
            foreach (CellChunk chunk in CellChunkManager.Chunks.Values)
            {
                //通过ChunkDataFiles中的方法来存储团块的单元数据
                chunk.gameObject.GetComponent<CellChunkDataFiles>().SaveData();
            }

            //将所有团块的单元数据从内存写入磁盘，并清除内存
            WriteLoadedChunks();

            Debug.Log("CellSpace: World saved successfully. (Instant)");

        }

        /// <summary>
        /// 将所有团块的单元数据从内存写入磁盘，并清除内存
        /// </summary>
        public static void WriteLoadedChunks()
        { //writes all chunk data from memory to disk, and clears memory

            // for every chunk loaded in dictionary.遍历存储团块数据的临时内存
            foreach (string chunkIndex in TempChunkData.Keys)
            {
                //获取每个团块索引
                CPIndex index = CPIndex.FromString(chunkIndex);
                //获取团块索引对应的区域索引
                string region = GetParentRegion(index).ToString();

                // check if region is loaded, and load it if it's not.检查区域是否已加载，如果未加载则加载
                if (LoadRegionData(GetParentRegion(index)) == false)
                {
                    //利用团块索引对应的区域索引来创建1个空的区域文件
                    CreateRegionFile(GetParentRegion(index));
                    //利用团块索引对应的区域索引来写入区域数据
                    LoadRegionData(GetParentRegion(index));
                }

                // write chunk data into region dictionary.将团块数据写入区域字典
                int chunkRegionIndex = GetChunkRegionIndex(index); //返回团块索引对应区域文件中存储的团块数据数组的一维索引（即团块在区域中属于第几个元素）
                LoadedRegions[region][chunkRegionIndex] = TempChunkData[chunkIndex]; //将内存中的当前团块数据数组作为1个值写入到已加载区域组对应键值上，遍历以完成区域内全部团块数据的写入
            }
            TempChunkData.Clear();//清空团块数据数组


            // for every region loaded in dictionary.遍历字典中存储的每个已加载区域的键（区域索引类型）
            foreach (string regionIndex in LoadedRegions.Keys)
            {
                //写入区域文件
                WriteRegionFile(regionIndex);
            }
            LoadedRegions.Clear();//清空团块数据数组

        }

        private static void WriteRegionFile(string regionIndex)
        {
            //区域数据数组=已加载区域[区域索引]
            string[] regionData = LoadedRegions[regionIndex];
            //根据区域文件路径，创建写入流
            StreamWriter writer = new StreamWriter(GetRegionPath(CPIndex.FromString(regionIndex)));
            int count = 0;
            //遍历每个区域数据中的团块数据（字符串形式）
            foreach (string chunk in regionData)
            {
                //将每个团块数据写入区域文件
                writer.Write(chunk);
                if (count != regionData.Length - 1)
                {
                    //如果计数没有达到区域数据长度上限，在每个团块数据后面进行字符插值（用于读取时分割子字符串）
                    writer.Write((char)ushort.MaxValue);
                }
                count++;
            }
            writer.Flush(); //使用Flush来保证写入（防止程序异常崩溃等情况导致写入错误）
            writer.Close();
        }
    }

}
