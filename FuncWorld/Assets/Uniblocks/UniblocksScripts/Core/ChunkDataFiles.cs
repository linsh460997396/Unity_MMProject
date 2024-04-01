using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Uniblocks
{
    /// <summary>
    /// �ſ������ļ�����������ſ���������ݵļ��غͱ��档
    /// </summary>
    public class ChunkDataFiles : MonoBehaviour
    {
        /// <summary>
        /// ���ڴ洢�ſ����������
        /// </summary>
        public static bool SavingChunks;

        /// <summary>
        /// ��������ʱ�洢�������ſ����ݣ��Ա��Ժ�д�������ļ�
        /// </summary>
        public static Dictionary<string, string> TempChunkData; // stores chunk's data to write into a region file later

        /// <summary>
        /// ��ǰ�Ѽ���������
        /// </summary>
        public static Dictionary<string, string[]> LoadedRegions; // data of currently loaded regions

        /// <summary>
        /// ���Դ��ļ������ſ���������ݣ����δ�ҵ������򷵻�false��
        /// </summary>
        /// <returns>����ſ����������Ϊ���򷵻�false����֮������Ч����true</returns>
        public bool LoadData()
        { // attempts to load data from file, returns false if data is not found

            Chunk chunk = GetComponent<Chunk>();//��ȡ�ſ����ʵ������
            //�����ſ�������ȡ�ſ����������
            string chunkData = GetChunkData(chunk.ChunkIndex);
            //����ſ���������ݲ�Ϊ��
            if (chunkData != "")
            {
                //����ǰѹ�����ַ����н�ѹ�����ſ����ݣ����������浽�ſ��VoxelData�����С�
                DecompressData(chunk, GetChunkData(chunk.ChunkIndex));
                //�����ſ�������ɻ�����������ݺ�VoxelsDone��ΪTrue
                chunk.VoxelsDone = true;
                return true;
            }

            else
            {
                return false;
            }
        }

        /// <summary>
        /// �����ſ���������ݵ��ڴ棨TempChunkData��
        /// </summary>
        public void SaveData()
        {
            //��ȡ�ſ����ʵ������
            Chunk chunk = GetComponent<Chunk>();
            //ѹ���ض��ſ���������ݲ�������Ϊ�ַ������ء�
            string compressedData = CompressData(chunk);
            //��ѹ����������ַ���д��TempChunkData�ֵ�
            WriteChunkData(chunk.ChunkIndex, compressedData);
        }

        /// <summary>
        /// ����ǰѹ�����ض��ſ�����������ַ����н�ѹ�����ſ����ݣ����������浽�ſ��VoxelData�����С�
        /// </summary>
        /// <param name="chunk">�ſ�</param>
        /// <param name="data">��ǰѹ�����ض��ſ�����������ַ���</param>
        public static void DecompressData(Chunk chunk, string data)
        { // decompresses voxel data and loads it into the VoxelData array

            // check if chunk is empty.�����ǰѹ�����ض��ſ�����������ַ����Ƿ�Ϊ��
            if (data.Length == 2 && data[1] == (char)0)
            {
                //�����ſ��ǿյ�״̬
                chunk.Empty = true;
            }
            //������һ�� StringReader ���󣬽����ʼ��Ϊ�ַ��� data
            StringReader reader = new StringReader(data);

            int i = 0;
            int length = chunk.GetDataLength(); // length of VoxelData array.������������ĳ���

            try
            {
                while (i < length)
                { // this loop will stop once the VoxelData array has been populated completely. Iterates once per count-data block.
                  // ���ѭ������VoxelData���鱻��ȫ����ֹͣ��ÿ��count-data���ؿ������һ�Ρ�

                    //����reader.Read()������ȡ�ַ����е���һ���ַ�������reader.Read()�������ض�ȡ���ַ���Unicode���룬��ʹ������ת�������ص�ֵת��Ϊushort��
                    //������ΪUnicode����ʹ�������ֽڱ�ʾÿ���ַ�����ushort����������ͬ����˴���Ƭ�ν����϶�ȡ�ַ���data�е�ÿ���ַ�����������Ϊushortֵ���ء�

                    ushort currentCount = (ushort)reader.Read(); // read the count.��ȡ�¸��ַ�תΪ�������״ζ�ȡ��Ϊ��һ���ַ�������������������ص���������
                    ushort currentData = (ushort)reader.Read(); // read the data.��ȡ�¸��ַ�תΪ������������������������ݣ����ؿ�����ࣩ

                    int ii = 0;

                    while (ii < currentCount)
                    {
                        //��˲���ѭ������ָ���������������������ݣ����޸����ؿ�����ࣩ����������ƽ��1D����������Ϊ������ָi����
                        chunk.SetVoxelSimple(i, currentData);// write a single voxel for every currentCount.����ſ��ڴ����������������ؿ���ͬһ�֣�ѹ��ʱcurrentCount������ϴ��ڴ˴�����ѭ������
                        ii++; //����ֱ���������������ؿ鶼���ú�����
                        i++; //���յ�i�϶����ᳬ��һ���ſ�ӵ�е�������������ĳ��ȵģ�ÿ�δ����iҲҪ+1
                    }
                }
            }
            catch (System.Exception)
            {
                //ֻ��һ��������Ǵ洢�ſ�������𻵣����ܱ��������ĵ���ʹ�ò�ͬ���ſ��С�����˱������ݣ�
                Debug.LogError("Uniblocks: Corrupt chunk data for chunk: " + chunk.ChunkIndex.ToString() + ". Has the data been saved using a different chunk size?");
                reader.Close();
                return;
            }

            reader.Close();

        }

        /// <summary>
        /// ѹ���ض��ſ���������ݲ�������Ϊ�ַ������ء�
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public static string CompressData(Chunk chunk)
        { // returns the data of chunk in compressed string format

            StringWriter writer = new StringWriter();

            int i = 0;
            int length = chunk.GetDataLength(); // length of VoxelData array.�ſ�������������鳤��

            ushort currentCount = 0; // count of consecutive voxels of the same type.�����������������ͬ��������ؿ�����
            ushort currentData = 0; // data of the current voxel.��ǰ��������ؿ�Ӧ���������

            for (i = 0; i < length; i++)
            { // for each voxel

                ushort thisData = chunk.GetVoxelSimple(i); // read raw data at i.��ȡ�ſ��1ά�����µ����ؿ�����

                if (thisData != currentData) //����ſ�����ؿ��������ǰӦ��������಻ͬ
                { // if the data is different from the previous data, write the last block and start a new one

                    // write previous block 
                    if (i != 0)
                    { // (don't write in the first loop iteration, because count would be 0 (no previous blocks))
                        //��Ҫ�ڵ�һ��ѭ��������д�룬��ΪcountΪ0(û��֮ǰ�����ؿ��¼)
                        writer.Write((char)currentCount);
                        writer.Write((char)currentData);
                    }
                    // start new block.����ʵ�����ؿ�ļ�¼
                    currentCount = 1;
                    currentData = thisData;
                }

                else
                { // if the data is the same as the last data, simply add to the count.������������һ��������ͬ��ֻ����Ӽ���
                    currentCount++;
                }

                if (i == length - 1)
                { // if this is the last iteration of the loop, close and write the current block.�������ѭ�������һ�ε������رղ�д�뵱ǰ���ؿ�
                    writer.Write((char)currentCount);
                    writer.Write((char)currentData);
                }

            }

            string compressedData = writer.ToString();
            writer.Flush(); //ʹ��Flush����֤д�루��ֹ�����쳣�������������д�����
            writer.Close();
            return compressedData;

        }

        /// <summary>
        /// �����ſ�����(�����ڴ���ļ�)������Ҳ��������򷵻ؿ��ַ���
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetChunkData(Index index)
        { // returns the chunk data (from memory or from file), or an empty string if data can't be found

            // try to load from TempChunkData.���Դ�TempChunkData����
            string indexString = index.ToString();
            if (TempChunkData.ContainsKey(indexString))
            {
                //���TempChunkData�ֵ����index�����������Ӧ��ֵ���ſ���������ݣ�
                return TempChunkData[indexString];
            }

            // try to load from region, return empty if not found.���Դ�������أ����δ�ҵ��򷵻ؿ�
            int regionIndex = GetChunkRegionIndex(index); //��ȡ�ſ�������Ӧ�����ļ��д洢���ſ����������һά���������ſ������������ڵڼ���Ԫ�أ�
            //��ȡ������ſ�������������鼯��
            string[] regionData = GetRegionData(GetParentRegion(index));
            if (regionData == null)
            {
                return "";
            }
            return regionData[regionIndex];

        }

        /// <summary>
        /// ��ѹ����������ַ���д��TempChunkData�ֵ䣨�洢���ſ���������ݣ�
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <param name="data">ѹ�����ſ����������</param>
        private void WriteChunkData(Index index, string data)
        { // writes the chunk data to the TempChunkData dictionary
            TempChunkData[index.ToString()] = data;
        }

        /// <summary>
        /// �����ſ�������Ӧ�����ļ��д洢���ſ����������һά���������ſ������������ڵڼ���Ԫ�أ����洢ʱ������߳��趨Ϊ10��һ������洢1K���ſ顣
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns></returns>
        private static int GetChunkRegionIndex(Index index)
        { // returns the 1d index of a chunk's data in the region file

            Index newIndex = new Index(index.x, index.y, index.z);
            //����ſ������Ǹ��ģ�������ԭ����о��������������������һά����
            if (newIndex.x < 0) newIndex.x = -newIndex.x - 1;
            if (newIndex.y < 0) newIndex.y = -newIndex.y - 1;
            if (newIndex.z < 0) newIndex.z = -newIndex.z - 1;

            //�洢ʱ������߳��趨Ϊ10���洢1K���ſ飩������������õ�����������������ſ���������1D����
            int flatIndex = (newIndex.z * 100) + (newIndex.y * 10) + newIndex.x;
            //��֤������0~999
            while (flatIndex > 999)
            {
                flatIndex -= 1000;
            }

            return flatIndex;
        }

        /// <summary>
        /// ��ȡ����������Ӧ���������ݳɹ�ʱ����������������Ӧ�������ļ�������
        /// </summary>
        /// <param name="regionIndex">��������</param>
        /// <returns>�����������ݲ����ļ����ظ����ݣ����δ�ҵ������ļ��򷵻�null</returns>
        private static string[] GetRegionData(Index regionIndex)
        { // loads region data and from file returns it, or returns null if region file is not found
            //��ȡ����������Ӧ���������ݳɹ�ʱ
            if (LoadRegionData(regionIndex) == true)
            {
                //��������������Ӧ�������ļ�����
                return LoadedRegions[regionIndex.ToString()];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ������������Ӧ��·���µ��ļ�����ȡȫ�����ݲ��ָ�������������飬������������Ϊ��������ֵ���洢����ǰ�Ѽ���������
        /// </summary>
        /// <param name="regionIndex">��������</param>
        /// <returns>��������ļ���������δ���أ����������ݼ��ص��ڴ��У������ݴ���(���Ѽ���)�򷵻�true�����򷵻�false</returns>
        private static bool LoadRegionData(Index regionIndex)
        { // loads the region data into memory if file exists and it's not already loaded, returns true if data exists (and is loaded), else false
            //��ȡ����������Ӧ���ַ���
            string indexString = regionIndex.ToString();
            //�����ǰ�Ѽ��������鲻��������ָ��������
            if (LoadedRegions.ContainsKey(indexString) == false)
            { // if not loaded.ָ������û������

                // load data if region file exists.��ָ��������ļ����ڣ����������
                string regionPath = GetRegionPath(regionIndex); //��ȡָ������������Ӧ�������ļ�·��
                if (File.Exists(regionPath))
                {
                    StreamReader reader = new StreamReader(regionPath);
                    string temp = reader.ReadToEnd();
                    char splitChar = (char)ushort.MaxValue;
                    string[] regionData = temp.Split(splitChar);
                    reader.Close();
                    LoadedRegions[indexString] = regionData; //����������������Ϊ��������ֵ���洢����ǰ�Ѽ���������

                    return true;

                }

                else
                {
                    return false; // return false if region file doesn't exist
                }
            }

            return true; // return true if data is already loaded
        }

        /// <summary>
        /// ��ȡ����������Ӧ�������ļ�·��
        /// </summary>
        /// <param name="regionIndex"></param>
        /// <returns></returns>
        private static string GetRegionPath(Index regionIndex)
        {

            return Engine.WorldPath + (regionIndex.ToString() + ",.region");
        }

        /// <summary>
        /// ��ȡ�ſ�������Ӧ����������
        /// </summary>
        /// <param name="index">�ſ�����</param>
        /// <returns>���ذ����ض��ſ����������</returns>
        private static Index GetParentRegion(Index index)
        { // returns the index of the region containing a specific chunk

            Index newIndex = new Index(index.x, index.y, index.z);

            //��������߳�=10���ſ飬�������ת��
            if (index.x < 0) newIndex.x -= 9;
            if (index.y < 0) newIndex.y -= 9;
            if (index.z < 0) newIndex.z -= 9;

            int x = newIndex.x / 10;
            int y = newIndex.y / 10;
            int z = newIndex.z / 10;

            return new Index(x, y, z);
        }

        /// <summary>
        /// ����һ���յ������ļ�
        /// </summary>
        /// <param name="index">��������</param>
        private static void CreateRegionFile(Index index)
        { // creates an empty region file

            Directory.CreateDirectory(Engine.WorldPath); //�������������ļ��У����������ʱ��
            StreamWriter writer = new StreamWriter(GetRegionPath(index)); //��������������ȡ�����ļ�Ӧ���ڵ�·��������д����

            for (int i = 0; i < 999; i++)
            {
                //ushort��char�������ݳ��ȶ���16λ
                //д��һ��65535��Ӧ��Char��Unicode�е�һ�������ַ���������ʾ�κ��ض����ţ���������ʾ���ȡʱӦ�����룬��Ϊ����������޷���ȷ���������ַ���Χ��ֵ��
                writer.Write((char)ushort.MaxValue); //��û���������������Ԥ��д����1000��Char
            }

            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// ʹ��Unity�Դ�Э�̣��첽�����д浵�����ſ���������ݱ��浽�ڴ��Ӳ��
        /// </summary>
        /// <returns></returns>
        public static IEnumerator SaveAllChunks()
        {

            if (!Engine.SaveVoxelData)
            {
                //�����ﱻ��ֹ�浵��������ʱ��Э����ֹ
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
                yield break;
            }

            //WaitForEndOfFrameֱ��SavingChunks=false
            while (SavingChunks)
            {
                //��ÿ֡��Ļ���������GUI����Ⱦ���ǰ�ȴ���ȷ����ǰ֡�������ȫ�������������ܽ��б��棩
                yield return new WaitForEndOfFrame();
            }
            SavingChunks = true;

            // for each chunk object, save data to memory

            int count = 0;
            List<Chunk> chunksToSave = new List<Chunk>(ChunkManager.Chunks.Values);  //�洢�ֵ��е�ÿ���ſ�

            //�����ֵ��е�ÿ���ſ�
            foreach (Chunk chunk in chunksToSave)
            {
                //��ȡ�ſ�ʵ�������ChunkDataFiles���ʵ����������ű��ڵ�Save�������ſ���������ݱ��浽�ڴ�
                chunk.gameObject.GetComponent<ChunkDataFiles>().SaveData();
                count++;
                if (count > Engine.MaxChunkSaves)
                {
                    //����ÿ֡�ſ����ݴ洢�Ĵ�������������Ļ���������GUI����Ⱦ���ǰ�ȴ�����һ֡�ټ�������
                    yield return new WaitForEndOfFrame();
                    count = 0;
                }
            }

            //���ڴ�д�뱾�ش浵
            WriteLoadedChunks();
            SavingChunks = false; //���涯���������

            Debug.Log("Uniblocks: World saved successfully.");
        }

        /// <summary>
        /// �洢�����ſ�ʵ������TempChunkData�е�����д�������ļ�
        /// </summary>
        public static void SaveAllChunksInstant()
        {
            //���ô洢��������ʱ
            if (!Engine.SaveVoxelData)
            {
                //���棺������ı�����������û������
                Debug.LogWarning("Uniblocks: Saving is disabled. You can enable it in the Engine Settings.");
                return;
            }

            // for each chunk object, save data to memory.����ÿ���ſ���󣬽������ݱ��浽�ڴ棨TempChunkData���У�ͬʱ������ټ��أ�
            foreach (Chunk chunk in ChunkManager.Chunks.Values)
            {
                //ͨ��ChunkDataFiles�еķ������洢�ſ����������
                chunk.gameObject.GetComponent<ChunkDataFiles>().SaveData();
            }

            //�������ſ���������ݴ��ڴ�д����̣�������ڴ�
            WriteLoadedChunks();

            Debug.Log("Uniblocks: World saved successfully. (Instant)");

        }

        /// <summary>
        /// �������ſ���������ݴ��ڴ�д����̣�������ڴ�
        /// </summary>
        public static void WriteLoadedChunks()
        { //writes all chunk data from memory to disk, and clears memory

            // for every chunk loaded in dictionary.�����洢�ſ����ݵ���ʱ�ڴ�
            foreach (string chunkIndex in TempChunkData.Keys)
            {
                //��ȡÿ���ſ�����
                Index index = Index.FromString(chunkIndex);
                //��ȡ�ſ�������Ӧ����������
                string region = GetParentRegion(index).ToString();

                // check if region is loaded, and load it if it's not.��������Ƿ��Ѽ��أ����δ���������
                if (LoadRegionData(GetParentRegion(index)) == false)
                {
                    //�����ſ�������Ӧ����������������1���յ������ļ�
                    CreateRegionFile(GetParentRegion(index));
                    //�����ſ�������Ӧ������������д����������
                    LoadRegionData(GetParentRegion(index));
                }

                // write chunk data into region dictionary.���ſ�����д�������ֵ�
                int chunkRegionIndex = GetChunkRegionIndex(index); //�����ſ�������Ӧ�����ļ��д洢���ſ����������һά���������ſ������������ڵڼ���Ԫ�أ�
                LoadedRegions[region][chunkRegionIndex] = TempChunkData[chunkIndex]; //���ڴ��еĵ�ǰ�ſ�����������Ϊ1��ֵд�뵽�Ѽ����������Ӧ��ֵ�ϣ����������������ȫ���ſ����ݵ�д��
            }
            TempChunkData.Clear();//����ſ���������


            // for every region loaded in dictionary.�����ֵ��д洢��ÿ���Ѽ�������ļ��������������ͣ�
            foreach (string regionIndex in LoadedRegions.Keys)
            {
                //д�������ļ�
                WriteRegionFile(regionIndex);
            }
            LoadedRegions.Clear();//����ſ���������

        }

        private static void WriteRegionFile(string regionIndex)
        {
            //������������=�Ѽ�������[��������]
            string[] regionData = LoadedRegions[regionIndex];
            //���������ļ�·��������д����
            StreamWriter writer = new StreamWriter(GetRegionPath(Index.FromString(regionIndex)));
            int count = 0;
            //����ÿ�����������е��ſ����ݣ��ַ�����ʽ��
            foreach (string chunk in regionData)
            {
                //��ÿ���ſ�����д�������ļ�
                writer.Write(chunk);
                if (count != regionData.Length - 1)
                {
                    //�������û�дﵽ�������ݳ������ޣ���ÿ���ſ����ݺ�������ַ���ֵ�����ڶ�ȡʱ�ָ����ַ�����
                    writer.Write((char)ushort.MaxValue);
                }
                count++;
            }

            writer.Flush(); //��֤д��
            writer.Close();
        }
    }

}