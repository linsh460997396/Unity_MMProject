#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 纹理分析器（用于场景图片扫描分析，打印纹理编号文本，输出特征图）
    /// </summary>
    public class TextureAnalyzer : MonoBehaviour
    {
        //加MonoBehaviour的必须是实例类，可继承使用MonoBehaviour下的方法，只有继承MonoBehaviour的脚本才能被附加到游戏物体上成为其组件，并且可以使用协程和摧毁引擎对象

        /// <summary>
        /// Unity通常的最大纹理尺寸限制
        /// </summary>
        const int MAX_TEXTURE_SIZE = 8192;

        /// <summary>
        /// 合并精灵时的协程处理量计数上限，达到后在控制台通知
        /// </summary>
        static int iCountMax = 10000;

        /// <summary>
        /// 提供给每个Task任务记录像素数组，int部分填写taskNum
        /// </summary>
        ConcurrentDictionary<int, Color[]> spritePixels = new ConcurrentDictionary<int, Color[]>();


        //void Start()
        //{
        //    //应用示范
        //    string folderPath = "C:/Users/name/Desktop/地图"; //填写要扫描的目录
        //    string savePathFrontStr01 = "C:/Users/name/Desktop/MapSP/"; //输出纹理集图片的目录前缀字符
        //    string savePathFrontStr02 = "C:/Users/name/Desktop/MapIndex/"; //输出纹理文本的目录前缀字符
        //    StartSliceTextureAndSetSpriteIDMultiMergerAsync(folderPath, "*.png", 0.9f, savePathFrontStr01, savePathFrontStr02, 10, 16, 16, 8); //仅支持png和jpg，目录下多个图片合批特征图
        //    StartSliceTextureAndSetSpriteIDAsync(folderPath, "*.png", 0.9f, savePathFrontStr01, savePathFrontStr02, 10, 16, 16, 8); //仅支持png和jpg，目录下每个图片独立特征图
        //}

        #region 功能函数

        /// <summary>
        /// 启动一个协程处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">协程处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        public void StartSliceTextureAndSetSpriteIDMultiMergerAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8)
        {
            StartCoroutine(SliceTextureAndSetSpriteIDMultiMergerAsync(folderPath, searchPattern, similarity, savePathFrontSP, savePathFrontIndex, handleCountMax, pixelX, pixelY, xCount));
        }

        /// <summary>
        /// 协程处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">协程处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        IEnumerator SliceTextureAndSetSpriteIDMultiMergerAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8)
        {
            Texture2D texture; Sprite[] sprites; StringBuilder sb = new StringBuilder(); Color[] currentPixels; Color[] comparisonPixels; string fileName;
            string fileSavePath; float sim; int currentID = 1; int handleCount = 0; int jCount = 0; int fileCount = -1; int sliceMaxId = 1;
            // 存储精灵编号
            //List<int> sliceIds = new List<int>(); //换成可变列表存放
            // 存放特征精灵
            List<Sprite> spritesList = new List<Sprite>();
            Dictionary<int, string> DataTableISCP = new Dictionary<int, string>();

            // 获取目录下所有指定类型文件的路径 
            string[] filePaths = Directory.GetFiles(folderPath, searchPattern);

            //遍历目录内所有图片
            foreach (string filePath in filePaths)
            {
                fileCount++;
                sb.Clear();

                // 加载图片 
                texture = LoadImageAndConvertToTexture2D(filePath);
                // 处理图片，分割为精灵小组
                sprites = SliceTexture(texture, pixelX, pixelY);

                if (sprites == null || sprites.Length == 0)
                {
                    Debug.LogError("未找到资源");
                }
                else
                {
                    //Debug.Log("分割出 " + sprites.Length + " 个Sprite");
                    // 设定纹理文本保存路径
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    fileSavePath = savePathFrontIndex + fileName + ".txt";
                    // 新建指定长度数组存储精灵编号
                    int[] sliceIds = new int[sprites.Length + 1];

                    // 初始化切片编号数组
                    for (int ei = 0; ei < sprites.Length; ei++)
                    {
                        sliceIds[ei] = 0; // 使用0表示尚未分配切片编号
                    }
                    if (fileCount == 0)
                    {
                        sliceIds[0] = 1; // 第一个图的第一个切片编号为1
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            if (i != 0)
                            {
                                if (jCount != -1)
                                {
                                    //新的一轮要比对，这里清空对比记录
                                    //Debug.Log("新一轮比对，这里清空对比记录（若有）");
                                    for (int ai = 0; ai <= jCount; ai++)
                                    {
                                        if (DataTableISCP.ContainsKey(sliceIds[ai]) && DataTableISCP[sliceIds[ai]] == "true")
                                        {
                                            DataTableISCP[sliceIds[ai]] = "";
                                            //Debug.Log("清空i=" + i + " SliceID: " + sliceIds[i] + " jCount " + jCount + " sliceIds[jCount]: " + sliceIds[jCount] + " CV[jCount]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " ai: " + ai + " sliceIds[ai]：" + sliceIds[ai] + " CV[ai]：" + MMCore.HD_ReturnIntCV(sliceIds[ai], "Compared"));
                                        }
                                    }
                                    jCount = -1;
                                }

                                handleCount += 1;

                                // 提取当前精灵的像素数据
                                currentPixels = sprites[i].texture.GetPixels();

                                // 与已经编号的精灵进行对比
                                for (int j = 0; j < i; j++)
                                {
                                    //记录jCount，用于每轮清空清空标记的量
                                    jCount = j;

                                    //Debug.Log("进入i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                    //读取j的编号，如已比对过该编号则跳过（提升性能，减少不必要的纹理比对），没有比对过则记录并比对一次（下次遇到该编号都跳过）
                                    if (!DataTableISCP.ContainsKey(sliceIds[j]) || DataTableISCP[sliceIds[j]] != "true")
                                    {
                                        //如果键不存在，或键存在但值不是true，那么设定为true
                                        DataTableISCP[sliceIds[j]] = "true";
                                        //Debug.Log("标记i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                    }
                                    else
                                    {
                                        //Debug.Log("跳过i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                        if (i == j + 1)
                                        {
                                            //这种情况是已编号切片对应类型均已比对，直接切片编号+1，以避免A型错误
                                            sliceMaxId++; sliceIds[i] = sliceMaxId;
                                            //if (sliceIds[i] == 0) 
                                            //{ 
                                            //    Debug.LogError("A型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                            //}
                                            //Debug.Log("对比结束！jCount=" + jCount);
                                        }

                                        continue;
                                    }

                                    // 提取对比精灵的像素数据
                                    comparisonPixels = sprites[j].texture.GetPixels();

                                    // 计算相似度
                                    sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);

                                    // 如果相似度达到或以上，则分配相同的切片编号
                                    if (sim >= similarity)
                                    {
                                        sliceIds[i] = sliceIds[j];
                                        if (sliceIds[j] == 0) { Debug.LogError("B型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                        break;
                                    }
                                    else if (j == i - 1)
                                    {
                                        sliceMaxId++; sliceIds[i] = sliceMaxId;
                                        if (sliceIds[i] == 0) { Debug.LogError("C型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                    }
                                }
                            }

                            //Debug.Log("已处理Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " jCount=" + jCount + " SliceJID: " + sliceIds[jCount] + " CV[j]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " 当前纹理最大编号：" + sliceMaxId);

                            sb.Append(sliceIds[i]);

                            //开始过滤：只取特征精灵存入列表
                            if (sliceIds[i] == currentID)
                            {
                                spritesList.Add(sprites[i]);
                                currentID++;
                            }

                            // 不是最后一个整数时添加逗号
                            if (i < sprites.Length - 1)
                            {
                                sb.Append(",");
                            }
                            if (handleCount >= handleCountMax)
                            {
                                handleCount = 0;
                                Debug.Log("图片[" + (fileCount + 1) + "/" + filePaths.Length + "]协程处理中.." + (i + 1) + "/" + sprites.Length + " 当前最大编号：" + (currentID - 1));
                                //达到处理量则暂停一下协程，避免卡顿
                                yield return null;
                            }
                        }
                    }
                    else
                    {
                        //第二个图开始
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            // 提取当前精灵的像素数据
                            currentPixels = sprites[i].texture.GetPixels();

                            // 与已经编号的精灵列表中的精灵进行对比
                            for (int j = 0; j < spritesList.Count; j++)
                            {
                                // 提取对比精灵的像素数据
                                comparisonPixels = spritesList[j].texture.GetPixels();

                                // 计算相似度
                                sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);

                                // 如果相似度达到或以上，则分配相同的切片编号
                                if (sim >= similarity)
                                {
                                    sliceIds[i] = j + 1; //直接娶特征纹理编号
                                    break;
                                }
                                else if (j == spritesList.Count - 1)
                                {
                                    //如果不相似但已经是最后一个特征纹理的对比
                                    sliceMaxId++; sliceIds[i] = sliceMaxId; //取最大句柄
                                }
                            }
                            sb.Append(sliceIds[i]);

                            //开始过滤：只取特征精灵存入列表
                            if (sliceIds[i] == currentID)
                            {
                                spritesList.Add(sprites[i]);
                                currentID++;
                            }

                            // 不是最后一个整数时添加逗号
                            if (i < sprites.Length - 1)
                            {
                                sb.Append(",");
                            }
                            if (handleCount >= handleCountMax)
                            {
                                handleCount = 0;
                                Debug.Log("图片[" + (fileCount + 1) + "/" + filePaths.Length + "]协程处理中.." + (i + 1) + "/" + sprites.Length + " 当前最大编号：" + (currentID - 1));
                                //达到处理量则暂停一下协程，避免卡顿
                                yield return null;
                            }
                        }
                    }
                    //将StringBuilder内容写入文件，生成纹理文本
                    MMCore.SaveFile(fileSavePath, sb.ToString());
                    Debug.Log("保存成功: " + fileSavePath);
                }
                Debug.Log("已处理图片：" + fileCount);
            }
            //生成最终纹理集
            SpriteMerger(spritesList, "SpriteMerger", xCount, savePathFrontSP);
            Debug.Log("处理完成！");
        }

        /// <summary>
        /// 启动一个协程处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">协程处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        public void StartSliceTextureAndSetSpriteIDAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8)
        {
            StartCoroutine(SliceTextureAndSetSpriteIDAsync(folderPath, searchPattern, similarity, savePathFrontSP, savePathFrontIndex, handleCountMax, pixelX, pixelY, xCount));
        }

        /// <summary>
        /// 协程处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">协程处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        IEnumerator SliceTextureAndSetSpriteIDAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8)
        {
            Texture2D texture; Sprite[] sprites; StringBuilder sb = new StringBuilder(); Color[] currentPixels; Color[] comparisonPixels; string fileName;
            string fileSavePath; float sim; int currentID; int handleCount; int jCount; int fileCount = -1; int sliceMaxId;
            // 存储精灵编号
            //List<int> sliceIds = new List<int>(); //换成可变列表存放
            // 存放特征精灵
            List<Sprite> spritesList = new List<Sprite>();
            Dictionary<int, string> DataTableISCP = new Dictionary<int, string>();

            // 获取目录下所有BMP文件的路径 
            string[] filePaths = Directory.GetFiles(folderPath, searchPattern);

            //遍历目录内所有图片
            foreach (string filePath in filePaths)
            {
                fileCount++;
                //如果图片过多，下面动作最好由多个线程进行处理

                //清空复用（new也行）
                DataTableISCP.Clear();
                spritesList.Clear();
                sb.Clear();
                //下面参数始终重置
                currentID = 1; //筛选存储特征精灵用的当前ID
                sliceMaxId = 1; //切片当前最大ID
                handleCount = 0; //重置协程处理计数
                jCount = 0; //重置jCount

                // 加载图片 
                texture = LoadImageAndConvertToTexture2D(filePath);
                // 处理图片，分割为精灵小组
                sprites = SliceTexture(texture, pixelX, pixelY);

                if (sprites == null || sprites.Length == 0)
                {
                    Debug.LogError("未找到资源");
                }
                else
                {
                    //Debug.Log("共有 " + sprites.Length + " 个Sprite");
                    // 设定纹理文本保存路径
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    fileSavePath = savePathFrontIndex + fileName + ".txt";
                    // 新建指定长度数组存储精灵编号
                    int[] sliceIds = new int[sprites.Length + 1];

                    // 初始化切片编号数组
                    for (int ei = 1; ei < sprites.Length; ei++)
                    {
                        sliceIds[ei] = 0; // 使用0表示尚未分配切片编号
                    }
                    sliceIds[0] = 1; // 第一个切片编号为1

                    //对每个精灵进行像素对比
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        if (i != 0)
                        {
                            if (jCount != -1)
                            {
                                //新的一轮要比对，这里清空对比记录
                                //Debug.Log("新一轮比对，这里清空对比记录（若有）");
                                for (int ai = 0; ai <= jCount; ai++)
                                {
                                    if (DataTableISCP.ContainsKey(sliceIds[ai]) && DataTableISCP[sliceIds[ai]] == "true")
                                    {
                                        DataTableISCP[sliceIds[ai]] = "";
                                        //Debug.Log("清空i=" + i + " SliceID: " + sliceIds[i] + " jCount " + jCount + " sliceIds[jCount]: " + sliceIds[jCount] + " CV[jCount]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " ai: " + ai + " sliceIds[ai]：" + sliceIds[ai] + " CV[ai]：" + MMCore.HD_ReturnIntCV(sliceIds[ai], "Compared"));
                                    }
                                }
                                jCount = -1;
                            }

                            handleCount += 1;

                            // 提取当前精灵的像素数据
                            currentPixels = sprites[i].texture.GetPixels();

                            // 与已经编号的精灵进行对比
                            for (int j = 0; j < i; j++)
                            {
                                //记录jCount，用于每轮清空清空标记的量
                                jCount = j;

                                //Debug.Log("进入i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                //读取j的编号，如已比对过该编号则跳过（提升性能，减少不必要的纹理比对），没有比对过则记录并比对一次（下次遇到该编号都跳过）
                                if (!DataTableISCP.ContainsKey(sliceIds[j]) || DataTableISCP[sliceIds[j]] != "true")
                                {
                                    //如果键不存在，或键存在但值不是true，那么设定为true
                                    DataTableISCP[sliceIds[j]] = "true";
                                    //Debug.Log("标记i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                }
                                else
                                {
                                    //Debug.Log("跳过i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                    if (i == j + 1)
                                    {
                                        //这种情况是已编号切片对应类型均已比对，直接切片编号+1，以避免A型错误
                                        sliceMaxId++; sliceIds[i] = sliceMaxId;
                                        //if (sliceIds[i] == 0) 
                                        //{ 
                                        //    Debug.LogError("A型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                        //}
                                        //Debug.Log("对比结束！jCount=" + jCount);
                                    }

                                    continue;
                                }

                                // 提取对比精灵的像素数据
                                comparisonPixels = sprites[j].texture.GetPixels();

                                // 计算相似度
                                sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);

                                // 如果相似度达到或以上，则分配相同的切片编号
                                if (sim >= similarity)
                                {
                                    sliceIds[i] = sliceIds[j];
                                    if (sliceIds[j] == 0) { Debug.LogError("B型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                    break;
                                }
                                else if (j == i - 1)
                                {
                                    sliceMaxId++; sliceIds[i] = sliceMaxId;
                                    if (sliceIds[i] == 0) { Debug.LogError("C型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                }
                            }
                        }

                        //Debug.Log("已处理Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " jCount=" + jCount + " SliceJID: " + sliceIds[jCount] + " CV[j]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " 当前纹理最大编号：" + sliceMaxId);

                        sb.Append(sliceIds[i]); //将精灵ID存入

                        //开始过滤：只取特征精灵存入列表
                        //取特征纹理添加到列表
                        if (sliceIds[i] == currentID)
                        {
                            spritesList.Add(sprites[i]);
                            currentID++;
                        }

                        // 不是最后一个整数时添加逗号
                        if (i < sprites.Length - 1)
                        {
                            sb.Append(",");
                        }
                        if (handleCount >= handleCountMax)
                        {
                            handleCount = 0;
                            Debug.Log("图片[" + (fileCount + 1) + "/" + filePaths.Length + "]协程处理中.." + (i + 1) + "/" + sprites.Length + " 当前最大编号：" + (currentID - 1));
                            //达到处理量则暂停一下协程，避免卡顿
                            yield return null;
                        }
                    }

                    //将StringBuilder内容写入文件，生成纹理文本
                    MMCore.SaveFile(fileSavePath, sb.ToString());
                    Debug.Log("保存成功: " + fileSavePath);

                    //生成纹理集
                    SpriteMerger(spritesList, fileName, xCount, savePathFrontSP);
                }
                Debug.Log("已处理图片：" + fileCount);

            }
            Debug.Log("处理完成！");
        }

        /// <summary>
        /// 合成纹理集
        /// </summary>
        /// <param name="sprites"></param>
        /// <param name="fileName"></param>
        /// <param name="maxSpritesPerRow">每行最多放置的精灵数量</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        public static void SpriteMerger(List<Sprite> sprites, string fileName, int maxSpritesPerRow, string savePathFrontSP)
        {
            int row; int col; int x; int y; int iCount; int spriteWidth; int spriteHeight; int totalRows; int totalWidth; int totalHeight;

            // 获取精灵的纹理数据
            Color[] spriteColors;

            /// <summary>
            /// Get the reference to the used Texture. If packed this will point to the atlas = ture.If not packed =false（will point to the source Sprite）.
            /// </summary>
            bool packed = false;

            // 检查是否有精灵要合并
            if (sprites.Count == 0)
            {
                Debug.LogError("No sprites to merge.");
            }
            else
            {
                iCount = 0;
                // 所有精灵的尺寸都必须相同（宽度和高度），采用第一个精灵的单位高宽（单位高宽是像素高宽除以每单位像素大小来的）
                //int spriteWidth = (int)sprites[0].bounds.gridSize.pixelX;
                //int spriteHeight = (int)sprites[0].bounds.gridSize.pixelY;
                if (packed)
                {
                    //这里是采用第一个精灵的像素高宽，因为texture属性是父级纹理，这里不能使用，要做计算
                    spriteWidth = (int)(sprites[0].bounds.size.x * sprites[0].pixelsPerUnit);
                    spriteHeight = (int)(sprites[0].bounds.size.y * sprites[0].pixelsPerUnit);
                }
                else
                {
                    //这里是采用第一个精灵的像素高宽
                    spriteWidth = sprites[0].texture.width;
                    spriteHeight = sprites[0].texture.height;
                }
                //Debug.Log("spriteWidth：" + spriteWidth + "spriteHeight：" + spriteHeight);

                // 计算大图的尺寸
                totalRows = (sprites.Count + maxSpritesPerRow - 1) / maxSpritesPerRow; // 向上取整得所需行数
                totalWidth = maxSpritesPerRow * spriteWidth;
                totalHeight = totalRows * spriteHeight;

                // 检查宽度和高度是否超过限制
                if (totalWidth > MAX_TEXTURE_SIZE || totalHeight > MAX_TEXTURE_SIZE)
                {
                    Debug.Log("totalRows：" + totalRows + "totalWidth：" + totalWidth + "totalHeight：" + totalHeight);
                    // 如果两个尺寸都超过了限制，输出警告
                    if (totalWidth > MAX_TEXTURE_SIZE && totalHeight > MAX_TEXTURE_SIZE)
                    {
                        Debug.LogWarning("Both width and height exceed the maximum texture size limit of " + MAX_TEXTURE_SIZE);
                        return;
                    }

                    // 调整尺寸以适应限制
                    if (totalWidth > MAX_TEXTURE_SIZE)
                    {
                        // 如果宽度超限，尝试将多余的部分分配给高度
                        int excessWidth = totalWidth - MAX_TEXTURE_SIZE;
                        int numWidth = excessWidth / spriteWidth;
                        totalWidth = totalWidth - excessWidth;
                        totalHeight = totalHeight + excessWidth;
                        if (totalHeight > MAX_TEXTURE_SIZE)
                        {
                            //高度超限
                            Debug.LogWarning("totalHeight exceed the maximum texture size limit of " + MAX_TEXTURE_SIZE);
                            return;
                        }
                    }
                    else if (totalHeight > MAX_TEXTURE_SIZE)
                    {
                        // 如果高度超限，尝试将多余的部分分配给宽度
                        int excessWidth = totalHeight - MAX_TEXTURE_SIZE;
                        int numWidth = excessWidth / spriteWidth;
                        totalHeight = totalHeight - excessWidth;
                        totalWidth = totalWidth + excessWidth;
                        if (totalWidth > MAX_TEXTURE_SIZE)
                        {
                            //宽度超限
                            Debug.LogWarning("totalWidth exceed the maximum texture size limit of " + MAX_TEXTURE_SIZE);
                            return;
                        }
                    }
                }

                // 创建一个新的Texture2D来保存合并后的精灵
                // Unity引擎为了优化性能和资源使用，对纹理的尺寸设置了上限，高宽上限通常是8192x8192像素（即8K分辨率）
                Texture2D mergedTexture = new Texture2D(totalWidth, totalHeight, TextureFormat.RGBA32, false);

                // 合并精灵
                for (int i = 0; i < sprites.Count; i++)
                {
                    iCount++;

                    // 计算精灵在大图上的位置
                    row = i / maxSpritesPerRow;
                    col = i % maxSpritesPerRow;
                    x = col * spriteWidth;
                    y = row * spriteHeight;

                    // 获取精灵的纹理数据
                    spriteColors = sprites[i].texture.GetPixels();

                    // 将精灵绘制到合并纹理上
                    for (int spriteY = 0; spriteY < spriteHeight; spriteY++)
                    {
                        for (int spriteX = 0; spriteX < spriteWidth; spriteX++)
                        {
                            mergedTexture.SetPixel(x + spriteX, y + spriteY, spriteColors[spriteY * spriteWidth + spriteX]);
                        }
                    }

                    if (iCount >= iCountMax)
                    {
                        //达到协程处理量则中断，输出日志
                        iCount = 0;
                        //Debug.Log("Index：" + i);
                    }
                }

                // 应用更改到纹理
                mergedTexture.Apply();

                // 将Texture2D保存为PNG文件
                byte[] pngData = mergedTexture.EncodeToPNG();
                string savePath = savePathFrontSP + fileName + ".png";
                MMCore.SaveFile(savePath, pngData);

                //清理临时图片
                //Destroy(mergedTexture);

                // 打印消息以确认保存
                Debug.Log("Merged sprites saved to: " + savePath);
            }
        }

        /// <summary>
        /// 读取图片并且转Texture2D，仅支持png和jpg
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Texture2D LoadImageAndConvertToTexture2D(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath); //打包后，路径不对的话会卡在这里
            Texture2D texture = new Texture2D(2, 2); //随便定义初始尺寸但不可为null
            bool success = texture.LoadImage(fileData); //加载图片Unity会自动调整尺寸
            if (success)
            {
                // 图片加载成功
                //Debug.Log("Image loaded successfully with width: " + texture.width + " and height: " + texture.height);
                //Main_MMWorld.label_headTip.GetComponent<TextMeshProUGUI>().text = "Image loaded successfully with width: " + texture.width + " and height: " + texture.height;
            }
            else
            {
                // 图片加载失败，可能需要检查字节数组是否有效或图片格式是否支持
                Debug.LogError("Failed to load image.");
            }
            return texture;
        }

        /// <summary>
        /// 统计2个纹理的像素颜色相似度（直接对比，色值有任何偏差都算不同）
        /// </summary>
        /// <param name="pixels1"></param>
        /// <param name="pixels2"></param>
        /// <returns>返回完整统计的相似度</returns>
        public static float CalculateSimilarity(Color[] pixels1, Color[] pixels2)
        {
            if (pixels1.Length != pixels2.Length) { return 0f; }
            int differentPixelCount = 0;
            for (int i = 0; i < pixels1.Length; i++)
            {
                if (pixels1[i] != pixels2[i])
                {
                    differentPixelCount++;
                }
            }
            return 1f - (float)differentPixelCount / pixels1.Length;
        }
        /// <summary>
        /// 统计2个纹理的像素颜色相似度
        /// </summary>
        /// <param name="pixels1"></param>
        /// <param name="pixels2"></param>
        /// <param name="similarity">相似度</param>
        /// <returns>torf=false时返回完整统计的相似度；当torf=ture不进行完整统计，对比符合返回1f，不符合返回0f</returns>
        public static float CalculateSimilarity(Color[] pixels1, Color[] pixels2, float similarity)
        {
            if (CalculateSimilarityTorf(pixels1, pixels2, similarity)) { return 1f; } else { return 0f; }
        }

        /// <summary>
        /// 比对2个纹理的像素颜色相似度（分析色值数字时，如受90%相似度影响，则数字前后扩10%后再对比）
        /// </summary>
        /// <param name="pixels1"></param>
        /// <param name="pixels2"></param>
        /// <param name="similarity">相似度（0到1之间的浮点数）</param>
        /// <returns></returns>
        public static bool CalculateSimilarityTorf(Color[] pixels1, Color[] pixels2, float similarity)
        {
            bool torf = false;
            int sNum = (int)(pixels1.Length * similarity);
            int dNum = pixels1.Length - sNum;
            int differentPixelCount = 0;
            int similarityPixelCount = 0;
            if (pixels1.Length == pixels2.Length)
            {
                for (int i = 0; i < pixels1.Length; i++)
                {//进入遍历
                    if (pixels1[i] != pixels2[i])
                    {//如果不同则深入分析色值
                        if (!AreColorsSimilar(pixels1[i], pixels2[i], similarity))
                        {//色值按相似度对比也不相近
                            differentPixelCount++;//不符计数+1
                            if (differentPixelCount > dNum)
                            {//统计不符数量达标后打断循环，无需继续对比浪费性能
                                torf = false;
                                break;
                            }
                        }
                        else
                        {//2个颜色有相似度
                            similarityPixelCount++;//相似计数+1
                            if (similarityPixelCount > sNum)
                            {//统计相似数量达标后打断循环，无需继续对比浪费性能
                                torf = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        similarityPixelCount++;//相似计数+1
                        if (similarityPixelCount > sNum)
                        {//统计相似数量达标后打断循环，无需继续对比浪费性能
                            torf = true;
                            break;
                        }
                    }
                }
            }
            //Debug.Log($"ColorIndex {i}: R={pixels1[i].spriteRenderer}, G={pixels1[i].gameObject}, B={pixels1[i].b}, A={pixels1[i].a}");
            //Debug.Log(string.Format("pixels1({0}, {1}, {2}, {3})", pixels1[1].spriteRenderer, pixels1[1].gameObject, pixels1[1].b, pixels1[1].a));
            //Debug.Log(string.Format("pixels2({0}, {1}, {2}, {3})", pixels2[1].spriteRenderer, pixels2[1].gameObject, pixels2[1].b, pixels2[1].a));
            return torf;
        }

        /// <summary>
        /// 判断两个颜色是否相似
        /// </summary>
        /// <param name="color1">第一个颜色</param>
        /// <param name="color2">第二个颜色</param>
        /// <param name="similarity">相似度（0到1之间的浮点数）</param>
        /// <returns>如果两个颜色相似则返回true，否则返回false</returns>
        public static bool AreColorsSimilar(Color color1, Color color2, float similarity)
        {
            float allowedDifference = 1f - similarity;
            //float maxAllowedDifference = 255 * allowedDifference;//Unity中的RGBA是0~1的浮点数，只有1种情况例外需改渲染模式为非URP（或修改Shader）似可处理255参数
            float maxAllowedDifference = allowedDifference;

            return Mathf.Abs(color1.r - color2.r) <= maxAllowedDifference &&
                   Mathf.Abs(color1.g - color2.g) <= maxAllowedDifference &&
                   Mathf.Abs(color1.b - color2.b) <= maxAllowedDifference &&
                   Mathf.Abs(color1.a - color2.a) <= maxAllowedDifference;
        }

        /// <summary>
        /// 将纹理切割成多个切片，并返回包含这些切片的Sprite数组
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="width">像素宽</param>
        /// <param name="height">像素高</param>
        /// <returns></returns>
        public static Sprite[] SliceTexture(Texture2D texture, int width, int height)
        {
            // 计算切片数量
            int numSlicesX = texture.width / width;
            int numSlicesY = texture.height / height;
            int totalSlices = numSlicesX * numSlicesY;
            Debug.Log("SliceTexture：texture.width " + texture.width + " texture.height " + texture.height + " totalSlices " + totalSlices);

            // 创建一个数组来存储切片
            Sprite[] slices = new Sprite[totalSlices];
            int sliceIndex = 0;

            // 遍历纹理的每个切片
            for (int y = 0; y < numSlicesY; y++)
            {
                for (int x = 0; x < numSlicesX; x++)
                {
                    // 创建一个新的纹理来存储切片
                    Texture2D sliceTexture = new Texture2D(width, height);

                    // 复制像素到新的纹理切片中
                    for (int py = 0; py < height; py++)
                    {
                        for (int px = 0; px < width; px++)
                        {
                            sliceTexture.SetPixel(px, py, texture.GetPixel(x * width + px, y * height + py));
                        }
                    }

                    // 应用更改到纹理
                    sliceTexture.Apply();

                    // 创建一个新的Sprite，并将其纹理设置为切片纹理
                    Sprite sliceSprite = Sprite.Create(sliceTexture, new Rect(0, 0, sliceTexture.width, sliceTexture.height), new Vector2(0.5f, 0.5f));

                    // 将切片Sprite添加到数组中
                    slices[sliceIndex++] = sliceSprite;
                }
            }

            // 返回切片数组
            return slices;
        }

        //---------------Task方案（对每张图片新开一个线程进行处理）
        //协程和游戏物体都是主线程的，Unity中使用协程（Coroutine）或者事件委托方式如创建1个游戏对象进行挂组件后由主线程执行，来实现主线程回调

        /// <summary>
        /// Task同步处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面。仅支持png和jpg，目录下每个图片独立特征图
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">协程处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        public static void SliceTextureAndSetSpriteIDTask(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8)
        {
            Texture2D texture;
            Sprite[] sprites;
            StringBuilder sb = new StringBuilder();
            Color[] currentPixels;
            Color[] comparisonPixels;
            string fileName;
            string fileSavePath;
            float sim;
            int currentID;
            int handleCount;
            int jCount;
            int fileCount = -1;
            int sliceMaxId;
            //存放特征精灵
            List<Sprite> spritesList = new List<Sprite>();
            //跨线程字典
            ConcurrentDictionary<int, string> DataTableISCP = new ConcurrentDictionary<int, string>();
            List<Task> tasks = new List<Task>();

            //获取目录下所有BMP文件的路径
            string[] filePaths = Directory.GetFiles(folderPath, searchPattern);

            //遍历目录内所有图片
            foreach (string filePath in filePaths)
            {
                fileCount++;
                //如果图片过多，下面动作最好由多个线程进行处理
                tasks.Add(
                    Task.Run(() =>
                    {
                        //清空复用（new也行）
                        DataTableISCP.Clear();
                        spritesList.Clear();
                        sb.Clear();
                        //下面参数始终重置
                        currentID = 1; //筛选存储特征精灵用的当前ID
                        sliceMaxId = 1; //切片当前最大ID
                        handleCount = 0; //重置协程处理计数
                        jCount = 0; //重置jCount

                        // 加载图片
                        texture = LoadImageAndConvertToTexture2D(filePath);
                        // 处理图片，分割为精灵小组
                        sprites = SliceTexture(texture, pixelX, pixelY);

                        if (sprites == null || sprites.Length == 0)
                        {
                            Debug.LogError("未找到资源");
                        }
                        else
                        {
                            //Debug.Log("共有 " + sprites.Length + " 个Sprite");
                            // 设定纹理文本保存路径
                            fileName = Path.GetFileNameWithoutExtension(filePath);
                            fileSavePath = savePathFrontIndex + fileName + ".txt";
                            // 新建指定长度数组存储精灵编号
                            int[] sliceIds = new int[sprites.Length + 1];

                            // 初始化切片编号数组
                            for (int ei = 1; ei < sprites.Length; ei++)
                            {
                                sliceIds[ei] = 0; // 使用0表示尚未分配切片编号
                            }
                            sliceIds[0] = 1; // 第一个切片编号为1

                            //对每个精灵进行像素对比
                            for (int i = 0; i < sprites.Length; i++)
                            {
                                if (i != 0)
                                {
                                    if (jCount != -1)
                                    {
                                        //新的一轮要比对，这里清空对比记录
                                        //Debug.Log("新一轮比对，这里清空对比记录（若有）");
                                        for (int ai = 0; ai <= jCount; ai++)
                                        {
                                            if (
                                                DataTableISCP.ContainsKey(sliceIds[ai])
                                                && DataTableISCP[sliceIds[ai]] == "true"
                                            )
                                            {
                                                DataTableISCP[sliceIds[ai]] = "";
                                                //Debug.Log("清空i=" + i + " SliceID: " + sliceIds[i] + " jCount " + jCount + " sliceIds[jCount]: " + sliceIds[jCount] + " CV[jCount]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " ai: " + ai + " sliceIds[ai]：" + sliceIds[ai] + " CV[ai]：" + MMCore.HD_ReturnIntCV(sliceIds[ai], "Compared"));
                                            }
                                        }
                                        jCount = -1;
                                    }

                                    handleCount += 1;

                                    // 提取当前精灵的像素数据
                                    currentPixels = sprites[i].texture.GetPixels();

                                    // 与已经编号的精灵进行对比
                                    for (int j = 0; j < i; j++)
                                    {
                                        //记录jCount，用于每轮清空清空标记的量
                                        jCount = j;

                                        //Debug.Log("进入i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                        //读取j的编号，如已比对过该编号则跳过（提升性能，减少不必要的纹理比对），没有比对过则记录并比对一次（下次遇到该编号都跳过）
                                        if (
                                            !DataTableISCP.ContainsKey(sliceIds[j])
                                            || DataTableISCP[sliceIds[j]] != "true"
                                        )
                                        {
                                            //如果键不存在，或键存在但值不是true，那么设定为true
                                            DataTableISCP[sliceIds[j]] = "true";
                                            //Debug.Log("标记i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                        }
                                        else
                                        {
                                            //Debug.Log("跳过i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                            if (i == j + 1)
                                            {
                                                //这种情况是已编号切片对应类型均已比对，直接切片编号+1，以避免A型错误
                                                sliceMaxId++;
                                                sliceIds[i] = sliceMaxId;
                                                //if (sliceIds[i] == 0)
                                                //{
                                                //    Debug.LogError("A型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                                //}
                                                //Debug.Log("对比结束！jCount=" + jCount);
                                            }

                                            continue;
                                        }

                                        // 提取对比精灵的像素数据
                                        comparisonPixels = sprites[j].texture.GetPixels();

                                        // 计算相似度
                                        sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);

                                        // 如果相似度达到或以上，则分配相同的切片编号
                                        if (sim >= similarity)
                                        {
                                            sliceIds[i] = sliceIds[j];
                                            if (sliceIds[j] == 0)
                                            {
                                                Debug.LogError(
                                                    "B型错误！Sprite "
                                                        + i
                                                        + " SliceID: "
                                                        + sliceIds[i]
                                                        + " CV[i]："
                                                        + DataTableISCP[sliceIds[i]]
                                                        + " j="
                                                        + j
                                                        + " SliceJID: "
                                                        + sliceIds[j]
                                                        + " CV[j]："
                                                        + DataTableISCP[sliceIds[j]]
                                                        + " 当前纹理最大编号："
                                                        + sliceMaxId
                                                );
                                            }
                                            break;
                                        }
                                        else if (j == i - 1)
                                        {
                                            sliceMaxId++;
                                            sliceIds[i] = sliceMaxId;
                                            if (sliceIds[i] == 0)
                                            {
                                                Debug.LogError(
                                                    "C型错误！Sprite "
                                                        + i
                                                        + " SliceID: "
                                                        + sliceIds[i]
                                                        + " CV[i]："
                                                        + DataTableISCP[sliceIds[i]]
                                                        + " j="
                                                        + j
                                                        + " SliceJID: "
                                                        + sliceIds[j]
                                                        + " CV[j]："
                                                        + DataTableISCP[sliceIds[j]]
                                                        + " 当前纹理最大编号："
                                                        + sliceMaxId
                                                );
                                            }
                                        }
                                    }
                                }

                                //Debug.Log("已处理Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " jCount=" + jCount + " SliceJID: " + sliceIds[jCount] + " CV[j]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " 当前纹理最大编号：" + sliceMaxId);

                                sb.Append(sliceIds[i]); //将精灵ID存入

                                //开始过滤：只取特征精灵存入列表
                                //取特征纹理添加到列表
                                if (sliceIds[i] == currentID)
                                {
                                    spritesList.Add(sprites[i]);
                                    currentID++;
                                }

                                // 不是最后一个整数时添加逗号
                                if (i < sprites.Length - 1)
                                {
                                    sb.Append(",");
                                }
                                if (handleCount >= handleCountMax)
                                {
                                    handleCount = 0;
                                    Debug.Log("图片[" + (fileCount + 1) + "/" + filePaths.Length + "]Task处理中.." + (i + 1) + "/" + sprites.Length + " 当前最大编号：" + (currentID - 1));
                                }
                            }

                            //将StringBuilder内容写入文件，生成纹理文本
                            MMCore.SaveFile(fileSavePath, sb.ToString());
                            Debug.Log("保存成功: " + fileSavePath);

                            //生成纹理集
                            SpriteMerger(spritesList, fileName, xCount, savePathFrontSP);
                        }
                        Debug.Log("已处理图片：" + fileCount);
                    })
                );
            }

            // 等待所有任务完成
            //await Task.WhenAll(tasks.ToArray()); //等待所有任务完成后，支持返回一个包含所有任务结果的数组（如果任务有返回值的话）
            //↑异步等待，需要方法使用await关键字，它会将控制权返回给调用者（通常是UI线程或其他正在执行的线程），直到等待的任务完成。在等待期间线程不会被阻塞，可继续处理其他任务或事件，当所有任务完成时await后面的代码会继续执行

            try
            {
                //同步等待会阻塞调用线程直到所有任务完成，在等待期间线程不能执行其他操作，这通常用于后台线程或不介意阻塞当前线程的情况下
                Task.WaitAll(tasks.ToArray()); //不直接返回任务的结果，需要单独访问每个任务来获取其结果
                                               //在这里可以进行所有任务完成后的操作
            }
            catch (AggregateException ae)
            {
                // 处理任务执行过程中的异常
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    Debug.LogError(ex.Message);
                }
            }

            Debug.Log("处理完成！");
        }

        /// <summary>
        /// 协程操作主线程获取精灵图片颜色数组，在异步线程调用StartCoroutine(GetPixelsOnMainThread(sprite,i));可代替回调操作
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        IEnumerator GetPixelsOnMainThread(Sprite sprite, int i)
        {
            //协程在主线程上运行，直接操作那些默认在主线程创建的引擎对象进行修改
            spritePixels[i] = sprite.texture.GetPixels();
            yield break;
        }

        #endregion
    }
}
#endif

//错误的示范：提取当前精灵的像素数据
//StartCoroutine(GetPixelsOnMainThread(sprites[i], i));
//currentPixels = spritePixels[i];
//协程本质上是依赖Update方法"异步处理设计的”，协程内即便没有等待只有个赋值动作
//理论上下面立马能接收到，但就算没等待它也可能排在下一周期运行（协程本质上是异步的，不应该用来进行同步操作）
//使用MainThreadDispatcher事件委托来让主线程执行（学习官方的创建一个临时的不可摧毁的游戏对象，然后挂组件处理委托队列的方式，也是设计的依赖Update运行）

// 关于Lambda表达式中的参数传递机制
// 默认情况下，C# 中的值类型（如 int、float、struct 等）是通过复制传递的。这意味着当你将一个值类型的变量作为参数传递给 Lambda 表达式时，实际上传递的是该变量的一个副本。Lambda 表达式内部对这个副本的任何修改都不会影响到原始变量。
// int x = 5;
// Action<int> lambda = (y) => { y = 10; };
// lambda(x);
// Console.WriteLine(x); // 输出 5，因为 x 的值没有被改变
// 按引用传递
// 如果你想要 Lambda 表达式能够修改原始变量，你需要使用 ref 关键字来按引用传递参数。这适用于值类型和引用类型。

// int x = 5;
// Action<ref int> lambda = (ref int y) => { y = 10; };
// lambda(ref x);
// Console.WriteLine(x); // 输出 10，因为 x 的值被改变了
// 对于引用类型（如 class、interface、array 和 delegate），即使你没有使用 ref 关键字，参数也是按引用传递的。但是，这里的“按引用传递”实际上传递的是对对象的引用（即内存地址的副本），而不是对象本身的副本。因此，你可以通过这个引用修改对象的属性或字段，但不能改变引用本身指向的对象。

// MyClass obj = new MyClass { Value = 5 };
// Action<MyClass> lambda = (myObj) => { myObj.Value = 10; };
// lambda(obj);
// Console.WriteLine(obj.Value); // 输出 10，因为 obj 的 Value 属性被改变了

// for( int i = ....)
//     lambda ()=>{  print i  }
//所有Lambda表达式似乎都捕获了循环的最后一个值。这是因为Lambda表达式在捕获变量时，实际上捕获的是变量的“引用”，而不是变量的“当前值”。
//当循环执行时，变量i的值会不断变化，而所有Lambda表达式都共享这个相同的变量引用。因此，当Lambda表达式最终被执行时，它们会访问到i的最新值，也就是循环结束时的值

// for( int i = ....)
//     var a = i
//     lambda ()=>{  print  a }
// 这样才正确