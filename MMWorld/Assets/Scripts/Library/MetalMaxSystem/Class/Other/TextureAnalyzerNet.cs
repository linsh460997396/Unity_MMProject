#if !(UNITY_EDITOR || UNITY_STANDALONE) && NET8_0_OR_GREATER
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Text;
using System.Threading;
using System.Numerics;
using System.Windows;
#if NET8_0
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
#endif

//↓可使用.Net中的Debug.WriteLine
using Debug = System.Diagnostics.Debug;

namespace MetalMaxSystem
{
    /// <summary>
    /// 纹理分析器（用于场景图片扫描分析，打印纹理编号文本，输出特征图）
    /// </summary>
    public class TextureAnalyzerNet
    {
        //加MonoBehaviour的必须是实例类，可继承使用MonoBehaviour下的方法，只有继承MonoBehaviour的脚本才能被附加到游戏物体上成为其组件，并且可以使用Task和摧毁引擎对象

        ///// <summary>
        ///// 提供给每个Task任务记录像素数组，int部分填写taskNum
        ///// </summary>
        //ConcurrentDictionary<int, Color[]> spritePixels = new ConcurrentDictionary<int, Color[]>();

        private static int _textureSizeLimit = 8192;
        private static int _messageOnceHandleNum = 10000;
        private static bool _defaultTransparent = true;

        /// <summary>
        /// 特征纹理通常的最大纹理尺寸限制，默认8192
        /// </summary>
        public static int TextureSizeLimit { get => _textureSizeLimit; set => _textureSizeLimit = value; }
        /// <summary>
        /// 合并精灵时的处理量（切片计数）上限，达到后在控制台通知，默认10000
        /// </summary>
        public static int MessageOnceHandleNum { get => _messageOnceHandleNum; set => _messageOnceHandleNum = value; }
        /// <summary>
        /// 合并精灵时是否清理图片背景（将颜色通道设置透明），默认true
        /// </summary>
        public static bool DefaultTransparent { get => _defaultTransparent; set => _defaultTransparent = value; }

        //void Start()
        //{
        //  //应用示范
        //  string folderPath = "C:/Users/name/Desktop/地图"; //填写要扫描的目录
        //  string savePathFrontStr01 = "C:/Users/name/Desktop/MapSP/"; //输出纹理集图片的目录前缀字符
        //  string savePathFrontStr02 = "C:/Users/name/Desktop/MapIndex/"; //输出纹理文本的目录前缀字符
        //  StartSliceTextureAndSetSpriteIDMultiMergerAsync(folderPath, "*.png", 0.9f, savePathFrontStr01, savePathFrontStr02, 10, 16, 16, 8); //仅支持png和jpg，目录下多个图片合批特征图
        //  StartSliceTextureAndSetSpriteIDAsync(folderPath, "*.png", 0.9f, savePathFrontStr01, savePathFrontStr02, 10, 16, 16, 8); //仅支持png和jpg，目录下每个图片独立特征图
        //}

        #region 功能函数

        /// <summary>
        /// 启动一个Task处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">Task处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        /// <param name="fromBottomLeft">Net框架下的Draw图片默认以左上为原点(0,0)，但输出纹理集一般以左下为原点所以该参数默认为true</param>
        /// <param name="torf">是否边缘尺寸不足按剩余高宽输出，默认为false（按参数高宽补满默认颜色）</param>
        public static void StartSliceTextureAndSetSpriteIDMultiMergerAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8, bool fromBottomLeft = true, bool torf = false)
        {
            Task.Run(
                () =>
                    SliceTextureAndSetSpriteIDMultiMergerAsync(
                        folderPath,
                        searchPattern,
                        similarity,
                        savePathFrontSP,
                        savePathFrontIndex,
                        handleCountMax,
                        pixelX,
                        pixelY,
                        xCount,
                        fromBottomLeft,
                        torf
                    )
            );
        }

        /// <summary>
        /// Task处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">Task处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        /// <param name="fromBottomLeft">Net框架下的Draw图片默认以左上为原点(0,0)，但输出纹理集一般以左下为原点所以该参数默认为true</param>
        /// <param name="torf">是否边缘尺寸不足按剩余高宽输出，默认为false（按参数高宽补满默认颜色）</param>
        public static async Task SliceTextureAndSetSpriteIDMultiMergerAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8, bool fromBottomLeft = true, bool torf = false)
        {
            Bitmap texture; Bitmap[] sprites; StringBuilder sb = new StringBuilder(); Color[] currentPixels; Color[] comparisonPixels; string fileName;
            string fileSavePath; float sim; int currentID = 1; int handleCount = 0; int jCount = 0; int fileCount = -1; int sliceMaxId = 1;
            //存储精灵编号
            //List<int> sliceIds = new List<int>(); //换成可变列表存放
            //存放特征精灵
            List<Bitmap> spritesList = new List<Bitmap>();
            Dictionary<int, string> DataTableISCP = new Dictionary<int, string>();

            //获取目录下所有指定类型文件的路径 
            string[] filePaths = Directory.GetFiles(folderPath, searchPattern);

            //遍历目录内所有图片
            foreach (string filePath in filePaths)
            {
                fileCount++;
                sb.Clear();
                //加载图片 
                texture = LoadImageAndConvertToBitmap(filePath);
                //处理图片，分割为精灵小组
                sprites = SliceTexture(texture, pixelX, pixelY, fromBottomLeft, torf);
                if (sprites == null || sprites.Length == 0)
                {
                    MMCore.Tell("未找到资源");
                }
                else
                {
                    //MMCore.Tell("分割出 " + sprites.Length + " 个Sprite");
                    //设定纹理文本保存路径
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    fileSavePath = savePathFrontIndex + fileName + ".txt";
                    //新建指定长度数组存储精灵编号
                    int[] sliceIds = new int[sprites.Length + 1];

                    //初始化切片编号数组
                    for (int ei = 0; ei < sprites.Length; ei++)
                    {
                        sliceIds[ei] = 0; //使用0表示尚未分配切片编号
                    }
                    if (fileCount == 0)
                    {
                        sliceIds[0] = 1; //第一个图的第一个切片编号为1
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            if (i != 0)
                            {
                                if (jCount != -1)
                                {
                                    //新的一轮要比对，这里清空对比记录
                                    //MMCore.Tell("新一轮比对，这里清空对比记录（若有）");
                                    for (int ai = 0; ai <= jCount; ai++)
                                    {
                                        if (DataTableISCP.ContainsKey(sliceIds[ai]) && DataTableISCP[sliceIds[ai]] == "true")
                                        {
                                            DataTableISCP[sliceIds[ai]] = "";
                                            //MMCore.Tell("清空i=" + i + " SliceID: " + sliceIds[i] + " jCount " + jCount + " sliceIds[jCount]: " + sliceIds[jCount] + " CV[jCount]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " ai: " + ai + " sliceIds[ai]：" + sliceIds[ai] + " CV[ai]：" + MMCore.HD_ReturnIntCV(sliceIds[ai], "Compared"));
                                        }
                                    }
                                    jCount = -1;
                                }

                                handleCount += 1;

                                //提取当前精灵的像素数据
                                currentPixels = GetPixels(sprites[i]);

                                //与已经编号的精灵进行对比
                                for (int j = 0; j < i; j++)
                                {
                                    //记录jCount，用于每轮清空清空标记的量
                                    jCount = j;

                                    //MMCore.Tell("进入i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                    //读取j的编号，如已比对过该编号则跳过（提升性能，减少不必要的纹理比对），没有比对过则记录并比对一次（下次遇到该编号都跳过）
                                    if (!DataTableISCP.ContainsKey(sliceIds[j]) || DataTableISCP[sliceIds[j]] != "true")
                                    {//这里ContainsKey不要找自己以免双重查找（否则应用TryGetValue）
                                        //如果键不存在，或键存在但值不是true，那么设定为true
                                        DataTableISCP[sliceIds[j]] = "true";
                                        //MMCore.Tell("标记i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                    }
                                    else
                                    {
                                        //MMCore.Tell("跳过i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                        if (i == j + 1)
                                        {
                                            //这种情况是已编号切片对应类型均已比对，直接切片编号+1，以避免A型错误
                                            sliceMaxId++; sliceIds[i] = sliceMaxId;
                                            //if (sliceIds[i] == 0) 
                                            //{ 
                                            //  MMCore.Tell("A型错误！Bitmap " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                            //}
                                            //MMCore.Tell("对比结束！jCount=" + jCount);
                                        }

                                        continue;
                                    }

                                    //提取对比精灵的像素数据（这是从原图上找特征索引来对比，上面的for循环只是找出特征索引j，其实第二切片起，与特征纹理数组逐一对比更效率一些）
                                    comparisonPixels = GetPixels(sprites[j]);

                                    //计算相似度
                                    sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);

                                    //如果相似度达到或以上，则分配相同的切片编号
                                    if (sim >= similarity)
                                    {
                                        sliceIds[i] = sliceIds[j];
                                        if (sliceIds[j] == 0) { MMCore.Tell("B型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                        break;
                                    }
                                    else if (j == i - 1)
                                    {
                                        sliceMaxId++; sliceIds[i] = sliceMaxId;
                                        if (sliceIds[i] == 0) { MMCore.Tell("C型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                    }
                                }
                            }

                            //MMCore.Tell("已处理Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " jCount=" + jCount + " SliceJID: " + sliceIds[jCount] + " CV[j]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " 当前纹理最大编号：" + sliceMaxId);

                            sb.Append(sliceIds[i]);

                            //开始过滤：只取特征精灵存入列表
                            if (sliceIds[i] == currentID)
                            {
                                spritesList.Add(sprites[i]);
                                currentID++;
                            }

                            //不是最后一个整数时添加逗号
                            if (i < sprites.Length - 1)
                            {
                                sb.Append(',');//这里填Char比填字符更高效
                            }
                            if (handleCount >= handleCountMax)
                            {
                                handleCount = 0;
                                MMCore.Tell("图片[" + (fileCount + 1) + "/" + filePaths.Length + "]Task处理中.." + (i + 1) + "/" + sprites.Length + " 当前最大编号：" + (currentID - 1));
                                //达到处理量则暂停一下Task，避免卡顿
                                await Task.Delay(1); //模拟协程延迟
                                //yield return null;
                            }
                        }
                    }
                    else
                    {
                        //第二个图开始
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            handleCount += 1;

                            //提取当前精灵的像素数据
                            currentPixels = GetPixels(sprites[i]);

                            //与已经编号的精灵列表中的精灵进行对比
                            for (int j = 0; j < spritesList.Count; j++)
                            {
                                //提取对比精灵的像素数据
                                comparisonPixels = GetPixels(spritesList[j]);

                                //计算相似度
                                sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);

                                //如果相似度达到或以上，则分配相同的切片编号
                                if (sim >= similarity)
                                {
                                    sliceIds[i] = j + 1; //直接取特征纹理编号
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

                            //不是最后一个整数时添加逗号
                            if (i < sprites.Length - 1)
                            {
                                sb.Append(',');
                            }
                            if (handleCount >= handleCountMax)
                            {
                                handleCount = 0;
                                MMCore.Tell("图片[" + (fileCount + 1) + "/" + filePaths.Length + "]Task处理中.." + (i + 1) + "/" + sprites.Length + " 当前最大编号：" + (currentID - 1));
                                //达到处理量则暂停一下Task，避免卡顿
                                await Task.Delay(1); //模拟协程延迟
                                //yield return null;
                            }
                        }
                    }
                    //将StringBuilder内容写入文件，生成纹理文本
                    MMCore.SaveFile(fileSavePath, sb.ToString());
                    MMCore.Tell("保存成功: " + fileSavePath);
                }
                MMCore.Tell("已处理图片：" + (fileCount + 1));
            }
            //生成最终纹理集
            SpriteMerger(spritesList, "SpriteMerger", xCount, savePathFrontSP, fromBottomLeft);
            MMCore.Tell("处理完成！");
        }

        /// <summary>
        /// 启动一个Task处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">Task处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        /// <param name="fromBottomLeft">Net框架下的Draw图片默认以左上为原点(0,0)，但输出纹理集一般以左下为原点所以该参数默认为true</param>
        /// <param name="torf">是否边缘尺寸不足按剩余高宽输出，默认为false（按参数高宽补满默认颜色）</param>
        public static void StartSliceTextureAndSetSpriteIDAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8, bool fromBottomLeft = true, bool torf = false)
        {
            Task.Run(
                () =>
                    SliceTextureAndSetSpriteIDAsync(
                        folderPath,
                        searchPattern,
                        similarity,
                        savePathFrontSP,
                        savePathFrontIndex,
                        handleCountMax,
                        pixelX,
                        pixelY,
                        xCount,
                        fromBottomLeft,
                        torf
                    )
            );
        }

        /// <summary>
        /// Task处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套纹理集及文本到桌面
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">仅支持这两种图片后缀"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">Task处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        /// <param name="fromBottomLeft">Net框架下的Draw图片默认以左上为原点(0,0)，但输出纹理集一般以左下为原点所以该参数默认为true</param>
        /// <param name="torf">是否边缘尺寸不足按剩余高宽输出，默认为false（按参数高宽补满默认颜色）</param>
        public static async Task SliceTextureAndSetSpriteIDAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8, bool fromBottomLeft = true, bool torf = false)
        {
            Bitmap texture; Bitmap[] sprites; StringBuilder sb = new StringBuilder(); Color[] currentPixels; Color[] comparisonPixels; string fileName;
            string fileSavePath; float sim; int currentID; int handleCount; int jCount; int fileCount = -1; int sliceMaxId;
            //存储精灵编号
            //List<int> sliceIds = new List<int>(); //换成可变列表存放
            //存放特征精灵
            List<Bitmap> spritesList = new List<Bitmap>();
            Dictionary<int, string> DataTableISCP = new Dictionary<int, string>();

            //获取目录下所有BMP文件的路径 
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
                handleCount = 0; //重置Task处理计数
                jCount = 0; //重置jCount

                //加载图片 
                texture = LoadImageAndConvertToBitmap(filePath);
                //处理图片，分割为精灵小组
                sprites = SliceTexture(texture, pixelX, pixelY, fromBottomLeft, torf);

                if (sprites == null || sprites.Length == 0)
                {
                    MMCore.Tell("未找到资源");
                }
                else
                {
                    //MMCore.Tell("共有 " + sprites.Length + " 个Sprite");
                    //设定纹理文本保存路径
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    fileSavePath = savePathFrontIndex + fileName + ".txt";
                    //新建指定长度数组存储精灵编号
                    int[] sliceIds = new int[sprites.Length + 1];

                    //初始化切片编号数组
                    for (int ei = 1; ei < sprites.Length; ei++)
                    {
                        sliceIds[ei] = 0; //使用0表示尚未分配切片编号
                    }
                    sliceIds[0] = 1; //第一个切片编号为1

                    //对每个精灵进行像素对比
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        if (i != 0)
                        {
                            if (jCount != -1)
                            {
                                //新的一轮要比对，这里清空对比记录
                                //MMCore.Tell("新一轮比对，这里清空对比记录（若有）");
                                for (int ai = 0; ai <= jCount; ai++)
                                {
                                    if (DataTableISCP.ContainsKey(sliceIds[ai]) && DataTableISCP[sliceIds[ai]] == "true")
                                    {
                                        DataTableISCP[sliceIds[ai]] = "";
                                        //MMCore.Tell("清空i=" + i + " SliceID: " + sliceIds[i] + " jCount " + jCount + " sliceIds[jCount]: " + sliceIds[jCount] + " CV[jCount]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " ai: " + ai + " sliceIds[ai]：" + sliceIds[ai] + " CV[ai]：" + MMCore.HD_ReturnIntCV(sliceIds[ai], "Compared"));
                                    }
                                }
                                jCount = -1;
                            }

                            handleCount += 1;

                            //提取当前精灵的像素数据
                            currentPixels = GetPixels(sprites[i]);

                            //与已经编号的精灵进行对比
                            for (int j = 0; j < i; j++)
                            {
                                //记录jCount，用于每轮清空清空标记的量
                                jCount = j;

                                //MMCore.Tell("进入i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                //读取j的编号，如已比对过该编号则跳过（提升性能，减少不必要的纹理比对），没有比对过则记录并比对一次（下次遇到该编号都跳过）
                                if (!DataTableISCP.ContainsKey(sliceIds[j]) || DataTableISCP[sliceIds[j]] != "true")
                                {
                                    //如果键不存在，或键存在但值不是true，那么设定为true
                                    DataTableISCP[sliceIds[j]] = "true";
                                    //MMCore.Tell("标记i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                }
                                else
                                {
                                    //MMCore.Tell("跳过i=" + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                    if (i == j + 1)
                                    {
                                        //这种情况是已编号切片对应类型均已比对，直接切片编号+1，以避免A型错误
                                        sliceMaxId++; sliceIds[i] = sliceMaxId;
                                        //if (sliceIds[i] == 0) 
                                        //{ 
                                        //  MMCore.Tell("A型错误！Bitmap " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId);
                                        //}
                                        //MMCore.Tell("对比结束！jCount=" + jCount);
                                    }

                                    continue;
                                }

                                //提取对比精灵的像素数据（这是从原图上找特征索引来对比，上面的for循环只是找出特征索引j，其实第二切片起，与特征纹理数组逐一对比更效率一些）
                                comparisonPixels = GetPixels(sprites[j]);

                                //计算相似度
                                sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);

                                //如果相似度达到或以上，则分配相同的切片编号
                                if (sim >= similarity)
                                {
                                    sliceIds[i] = sliceIds[j];
                                    if (sliceIds[j] == 0) { MMCore.Tell("B型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                    break;
                                }
                                else if (j == i - 1)
                                {
                                    sliceMaxId++; sliceIds[i] = sliceMaxId;
                                    if (sliceIds[i] == 0) { MMCore.Tell("C型错误！Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " j=" + j + " SliceJID: " + sliceIds[j] + " CV[j]：" + DataTableISCP[sliceIds[j]] + " 当前纹理最大编号：" + sliceMaxId); }
                                }
                            }
                        }

                        //MMCore.Tell("已处理Sprite " + i + " SliceID: " + sliceIds[i] + " CV[i]：" + DataTableISCP[sliceIds[i]] + " jCount=" + jCount + " SliceJID: " + sliceIds[jCount] + " CV[j]：" + MMCore.HD_ReturnIntCV(sliceIds[jCount], "Compared") + " 当前纹理最大编号：" + sliceMaxId);

                        sb.Append(sliceIds[i]); //将精灵ID存入

                        //开始过滤：只取特征精灵存入列表
                        //取特征纹理添加到列表
                        if (sliceIds[i] == currentID)
                        {
                            spritesList.Add(sprites[i]);
                            currentID++;
                        }

                        //不是最后一个整数时添加逗号
                        if (i < sprites.Length - 1)
                        {
                            sb.Append(',');
                        }
                        if (handleCount >= handleCountMax)
                        {
                            handleCount = 0;
                            MMCore.Tell("图片[" + (fileCount + 1) + "/" + filePaths.Length + "]Task处理中.." + (i + 1) + "/" + sprites.Length + " 当前最大编号：" + (currentID - 1));
                            //达到处理量则暂停一下Task，避免卡顿
                            await Task.Delay(1); //模拟协程延迟
                            //yield return null;
                        }
                    }

                    //将StringBuilder内容写入文件，生成纹理文本
                    MMCore.SaveFile(fileSavePath, sb.ToString());
                    MMCore.Tell("保存成功: " + fileSavePath);

                    //生成纹理集
                    SpriteMerger(spritesList, fileName, xCount, savePathFrontSP, fromBottomLeft);
                }
                MMCore.Tell("已处理图片：" + (fileCount + 1));

            }
            MMCore.Tell("处理完成！");
        }

        #endregion

        #region 辅助函数
        /// <summary>
        /// 合成纹理集
        /// </summary>
        /// <param name="sprites"></param>
        /// <param name="fileName"></param>
        /// <param name="maxSpritesPerRow">每行最多放置的精灵数量</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="fromBottomLeft">Net框架下的Draw图片默认以左上为原点(0,0)，但输出纹理集一般以左下为原点所以该参数默认为true</param>
        public static void SpriteMerger(List<Bitmap> sprites, string fileName, int maxSpritesPerRow, string savePathFrontSP, bool fromBottomLeft = true)
        {
            int row; int col; int x; int y; int iCount; int spriteWidth; int spriteHeight; int totalRows; int totalWidth; int totalHeight;

            //获取精灵的纹理数据
            Color[] spriteColors;
#if UNITY_EDITOR || UNITY_STANDALONE
            /// <summary>
            /// 如果是精灵编辑器制作的带Meta数据图片，而非直接用函数切割的精灵
            /// Get the reference to the used Texture. If packed this will point to the atlas = ture.If not packed =false（will point to the source Sprite）.
            /// </summary>
            bool packed = false;
#endif
            //检查是否有精灵要合并
            if (sprites.Count == 0)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                MMCore.Tell("No sprites to merge.");
#else
                MMCore.Tell("No sprites to merge.");
#endif
            }
            else
            {
                iCount = 0;
#if UNITY_EDITOR || UNITY_STANDALONE
                //所有精灵的尺寸都必须相同（宽度和高度），采用第一个精灵的单位高宽（单位高宽是像素高宽除以每单位像素大小来的）
                //int spriteWidth = (int)sprites[0].bounds.gridSize.width;
                //int spriteHeight = (int)sprites[0].bounds.gridSize.height;
                if (packed)
                {//如果是精灵编辑器制作的带Meta数据图片，而非直接用函数切割的精灵
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
                //MMCore.Tell("spriteWidth：" + spriteWidth + "spriteHeight：" + spriteHeight);
#else
                spriteWidth = sprites[0].Width;
                spriteHeight = sprites[0].Height;
#endif
                //计算大图的尺寸
                totalRows = (sprites.Count + maxSpritesPerRow - 1) / maxSpritesPerRow; //向上取整得所需行数
                totalWidth = maxSpritesPerRow * spriteWidth;
                totalHeight = totalRows * spriteHeight;

                //检查宽度和高度是否超过限制
                if (totalWidth > TextureSizeLimit || totalHeight > TextureSizeLimit)
                {
                    MMCore.Tell(
                        "totalRows："
                            + totalRows
                            + "totalWidth："
                            + totalWidth
                            + "totalHeight："
                            + totalHeight
                    );
                    //如果两个尺寸都超过了限制，输出警告
                    if (totalWidth > TextureSizeLimit && totalHeight > TextureSizeLimit)
                    {
                        MMCore.Tell(
                            "Both width and height exceed the maximum texture size limit of "
                                + TextureSizeLimit
                        );
                        return;
                    }

                    //调整尺寸以适应限制
                    if (totalWidth > TextureSizeLimit)
                    {
                        //如果宽度超限，尝试将多余的部分分配给高度
                        int excessWidth = totalWidth - TextureSizeLimit;
                        int numWidth = excessWidth / spriteWidth;
                        totalWidth = totalWidth - excessWidth;
                        totalHeight = totalHeight + excessWidth;
                        if (totalHeight > TextureSizeLimit)
                        {
                            //高度超限
                            MMCore.Tell(
                                "totalHeight exceed the maximum texture size limit of "
                                    + TextureSizeLimit
                            );
                            return;
                        }
                    }
                    else if (totalHeight > TextureSizeLimit)
                    {
                        //如果高度超限，尝试将多余的部分分配给宽度
                        int excessWidth = totalHeight - TextureSizeLimit;
                        int numWidth = excessWidth / spriteWidth;
                        totalHeight = totalHeight - excessWidth;
                        totalWidth = totalWidth + excessWidth;
                        if (totalWidth > TextureSizeLimit)
                        {
                            //宽度超限
                            MMCore.Tell(
                                "totalWidth exceed the maximum texture size limit of "
                                    + TextureSizeLimit
                            );
                            return;
                        }
                    }
                }

                //创建一个新的Texture2D来保存合并后的精灵
#if UNITY_EDITOR || UNITY_STANDALONE
                //Unity引擎为了优化性能和资源使用，对纹理的尺寸设置了上限，高宽上限通常是8192x8192像素（即8K分辨率）
                Texture2D mergedTexture = new Texture2D(totalWidth, totalHeight, TextureFormat.RGBA32, false);
                //合并精灵
                for (int i = 0; i < sprites.Count; i++)
                {
                    iCount++;

                    //计算精灵在大图上的位置
                    row = i / maxSpritesPerRow;
                    col = i % maxSpritesPerRow;
                    x = col * spriteWidth;
                    y = row * spriteHeight;

                    //获取精灵的纹理数据
                    spriteColors = sprites[i].texture.GetPixels();
                    //NET框架绘制时左上(0,0)，Unity绘制时左下(0,0)
                    if (!fromBottomLeft)
                    {
                        //如果参数要求左下，那么进行变换
                        y = totalHeight - y - spriteHeight;
                    }
                    //将精灵绘制到合并纹理上
                    for (int spriteY = 0; spriteY < spriteHeight; spriteY++)
                    {
                        for (int spriteX = 0; spriteX < spriteWidth; spriteX++)
                        {
                            mergedTexture.SetPixel(x + spriteX, y + spriteY, spriteColors[spriteY * spriteWidth + spriteX]);
                        }
                    }

                    if (iCount >= iCountMax)
                    {
                        //达到Task处理量则中断，输出日志
                        iCount = 0;
                        //MMCore.Tell("Index：" + i);
                    }
                }
                //应用更改到纹理
                mergedTexture.Apply();
                //将Texture2D保存为PNG文件
                byte[] pngData = mergedTexture.EncodeToPNG();
                string savePath = savePathFrontSP + fileName + ".png";
                MMCore.SaveFile(savePath, pngData);
                //清理临时图片
                //Destroy(mergedTexture);
                //打印消息以确认保存
                MMCore.Tell("Merged sprites saved to: " + savePath);
#else
                //.NET框架下创建一个指定宽度和高度的Bitmap对象
                Bitmap mergedTexture = new Bitmap(totalWidth, totalHeight);

                if (DefaultTransparent == true)
                {
                    //创建一个Graphics对象以便我们可以在Bitmap上绘图（如需对原图修改时）
                    using (Graphics g = Graphics.FromImage(mergedTexture))
                    {
                        //清除后让背景透明（因为默认背景是黑色）
                        g.Clear(Color.Transparent);

                        //此时，Bitmap的PixelFormat默认是Format32bppArgb（即RGBA32）
                        //如果你需要确认，可以检查mergedTexture.PixelFormat属性
                        if (mergedTexture.PixelFormat == PixelFormat.Format32bppArgb)
                        {
                            MMCore.Tell("Bitmap is using RGBA32 format.");
                        }
                        else
                        {
                            MMCore.Tell("Bitmap is not using RGBA32 format. Actual format: " + mergedTexture.PixelFormat);
                        }
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    iCount++;
                    //计算精灵在大图上的位置
                    row = i / maxSpritesPerRow;
                    col = i % maxSpritesPerRow;
                    x = col * spriteWidth;
                    y = row * spriteHeight;
                    //获取精灵的纹理数据
                    spriteColors = GetPixels(sprites[i]);
                    //Unity绘制的时候(0,0)是在左下角，换NET后变成左上(0,0)了
                    if (fromBottomLeft)
                    {
                        //如果参数要求左下，那么进行变换
                        y = totalHeight - y - spriteHeight;
                    }
                    //将精灵绘制到合并纹理上
                    for (int spriteY = 0; spriteY < spriteHeight; spriteY++)
                    {
                        for (int spriteX = 0; spriteX < spriteWidth; spriteX++)
                        {
                            mergedTexture.SetPixel(x + spriteX, y + spriteY, spriteColors[spriteY * spriteWidth + spriteX]);
                        }
                    }
                    if (iCount >= MessageOnceHandleNum)
                    {
                        //这里达到任务处理量后可中断（让主线程先去渲染或可输出日志）
                        iCount = 0;
                        //MMCore.Tell("Index：" + i);
                    }
                }
                string savePath = savePathFrontSP + fileName + ".png";
                string directoryPath = Path.GetDirectoryName(savePath);
                //如果目录不存在，则创建它
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                mergedTexture.Save(savePath, ImageFormat.Png); //无目录不会新建，要自己Debug
#if UNITY_EDITOR || UNITY_STANDALONE
                MMCore.Tell("Merged sprites saved to: " + savePath);
#else
                MMCore.Tell("Merged sprites saved to: " + savePath);
#endif
#endif
            }
        }

        /// <summary>
        /// 读取图片并转Bitmap
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Bitmap LoadImageAndConvertToBitmap(string filePath)
        {
            using (var image = Image.FromFile(filePath))
            {
                return new Bitmap(image);
            }
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
                { //进入遍历
                    if (pixels1[i] != pixels2[i])
                    { //如果不同则深入分析色值
                        if (!AreColorsSimilar(pixels1[i], pixels2[i], similarity))
                        { //色值按相似度对比也不相近
                            differentPixelCount++; //不符计数+1
                            if (differentPixelCount > dNum)
                            { //统计不符数量达标后打断循环，无需继续对比浪费性能
                                torf = false;
                                break;
                            }
                        }
                        else
                        { //2个颜色有相似度
                            similarityPixelCount++; //相似计数+1
                            if (similarityPixelCount > sNum)
                            { //统计相似数量达标后打断循环，无需继续对比浪费性能
                                torf = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        similarityPixelCount++; //相似计数+1
                        if (similarityPixelCount > sNum)
                        { //统计相似数量达标后打断循环，无需继续对比浪费性能
                            torf = true;
                            break;
                        }
                    }
                }
            }
            //MMCore.Tell($"ColorIndex {i}: R={pixels1[i].spriteRenderer}, G={pixels1[i].gameObject}, B={pixels1[i].b}, A={pixels1[i].a}");
            //MMCore.Tell(string.Format("pixels1({0}, {1}, {2}, {3})", pixels1[1].spriteRenderer, pixels1[1].gameObject, pixels1[1].b, pixels1[1].a));
            //MMCore.Tell(string.Format("pixels2({0}, {1}, {2}, {3})", pixels2[1].spriteRenderer, pixels2[1].gameObject, pixels2[1].b, pixels2[1].a));
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
#if UNITY_EDITOR || UNITY_STANDALONE
            //float maxAllowedDifference = 255 * allowedDifference;//Unity中的RGBA是0~1的浮点数，只有1种情况例外需改渲染模式为非URP（或修改Shader）似可处理255参数
            float maxAllowedDifference = allowedDifference;
            return Mathf.Abs(color1.r - color2.r) <= maxAllowedDifference &&
               Mathf.Abs(color1.g - color2.g) <= maxAllowedDifference &&
               Mathf.Abs(color1.b - color2.b) <= maxAllowedDifference &&
               Mathf.Abs(color1.a - color2.a) <= maxAllowedDifference;
#else
            float maxAllowedDifference = 255 * allowedDifference; //System.Drawing.Color的RGBA分量是0~255的整数，因此需要将允许的差异乘以255
            //MMCore.Tell("Math" + Math.Abs(color1.R - color2.R) + " " + Math.Abs(color1.G - color2.G) + " " + Math.Abs(color1.B - color2.B) + " " + Math.Abs(color1.A - color2.A) + "maxAllowedDifference" + maxAllowedDifference);
            return Math.Abs(color1.R - color2.R) <= maxAllowedDifference
                && Math.Abs(color1.G - color2.G) <= maxAllowedDifference
                && Math.Abs(color1.B - color2.B) <= maxAllowedDifference
                && Math.Abs(color1.A - color2.A) <= maxAllowedDifference;
#endif
        }

        /// <summary>
        /// 将纹理切割成多个切片（边缘尺寸不足仍严格按参数高宽输出），并返回包含这些切片的Sprite数组
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="width">像素宽</param>
        /// <param name="height">像素高</param>
        /// <param name="fromBottomLeft">Net框架下的Draw图片默认以左上为原点(0,0)，但输出纹理集一般以左下为原点所以该参数默认为true</param>
        /// <param name="torf">是否边缘尺寸不足按剩余高宽输出，默认为false（按参数高宽补满默认颜色）</param>
        /// <returns>返回Sprite（Bitmap）数组</returns>
        public static Bitmap[] SliceTexture(Bitmap texture, int width, int height, bool fromBottomLeft = true, bool torf = false)
        {
            int sliceWidth; int sliceHeight; int heightFixed; Bitmap sliceTexture;
            //计算切片数量
            int numSlicesX = (int)Math.Ceiling((float)texture.Width / width);
            int numSlicesY = (int)Math.Ceiling((float)texture.Height / height);
            int totalSlices = numSlicesX * numSlicesY;
            MMCore.Tell("SliceTexture：texture.width " + texture.Width + " texture.height " + texture.Height + " totalSlices " + totalSlices);

            //创建一个数组来存储切片
            Bitmap[] slices = new Bitmap[totalSlices];
            int sliceIndex = 0;

            //遍历纹理的每个切片
            for (int y = 0; y < numSlicesY; y++)
            {
                //Unity绘制的时候(0,0)是在左下角，换NET后变成左上(0,0)了
                if (fromBottomLeft)
                {
                    //如果参数要求左下，那么进行变换
                    heightFixed = texture.Height - height - y * height;
                    if (heightFixed < 0)
                    {
                        heightFixed = 0;
                    }
                }
                else
                {
                    heightFixed = y * height;
                }
                for (int x = 0; x < numSlicesX; x++)
                {
                    if (torf)
                    {
                        sliceWidth = Math.Min(width, texture.Width - x * width);
                        sliceHeight = Math.Min(height, texture.Height - y * height);
                    }
                    else
                    {
                        sliceWidth = width;
                        sliceHeight = height;
                    }
                    //创建一个新的中间纹理来存储切片（也可直接写图片区域克隆）
                    sliceTexture = new Bitmap(sliceWidth, sliceHeight);
                    //复制像素到新的纹理切片中（期间可以调整颜色）
                    for (int py = 0; py < sliceHeight; py++)
                    {
                        for (int px = 0; px < sliceWidth; px++)
                        {
                            sliceTexture.SetPixel(px, py, texture.GetPixel(x * width + px, heightFixed + py));
                        }
                    }

                    //创建一个新的Sprite并将其纹理设置为切片纹理（Unity自带精灵创建时有第三个参数确定精灵锚点，System.Draw的Bitmap没有），最后将Sprite添加到数组中
                    //slices[sliceIndex++] = texture.Clone(new Rectangle(x, heightFixed, sliceWidth, sliceHeight), texture.PixelFormat); 
                    slices[sliceIndex++] = sliceTexture;
                }
            }
            //返回切片数组
            return slices;
        }

        /// <summary>
        /// 获取位图的颜色（像素点信息）数组
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Color[] GetPixels(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[y * width + x] = bitmap.GetPixel(x, y);
                }
            }
            return pixels;
        }
        #endregion
    }
}

//System.Drawing.Bitmap类可用于读取多种常见的图片格式并转换为Bitmap对象，但并不是任何图片格式都能直接读取
//对于一些广泛使用的标准图片格式如JPEG、PNG、BMP、GIF等.NET框架提供了较好的支持，可以相对容易地将这些格式的图片文件读取为Bitmap对象
//如果需要支持更多格式或者对特殊格式进行处理，可能需要借助第三方库，如ImageSharp、Magick.NET等

//注意ArrayList和List<T>非跨线程安全（前者更因为非泛型存储任意类型对象会拆装箱，所以几乎不使用）
//可通过ConcurrentBag<T>、ConcurrentQueue<T>、ConcurrentStack<T>、ConcurrentDictionary<TKey, TValue>等线程安全集合类来解决List的线程安全问题
//也可通过lock关键字对List加锁来保证线程安全：在访问List的时候用lock语句锁定一个对象确保在同一时刻只有一个线程可对List操作，从而避免多线程同时改List导致线程安全问题
//List<int> myList = new List<int>();
//object lockObj = new object();//不要作为局部变量，会被摧毁
////在访问List之前加锁
//lock (lockObj)
//{
//   //对List进行操作
//   myList.Add(1);
//   myList.Remove(2);
//}
//如需要一个线程安全的无序集合，且不关心元素的顺序，那么ConcurrentBag<T>更适合
//如需要一个线程安全的先进先出（FIFO）集合，那么ConcurrentQueue<T>更适合
//如需要一个线程安全的后进先出（LIFO）集合，那么ConcurrentStack<T>更适合
//如不需要并发访问，或决定通过其他方式（如锁）来保证线程安全，那么List<Bitmap>更适合（在单线程环境下性能更好）

//await关键字只能在async声明的异步函数内用，作用是等待一个异步操作的完成，并且不会阻塞调用线程
//不使用async的Task
//for (int i = 0; i < 5; i++)
//{
//   Task task = Task.Run(() => DoWork(i));
//   task.Wait(); //会阻塞线程直到task完成
//}
//改为下面的
//async Task ExecuteTasksSequentiallyAsync()
//{
//   for (int i = 0; i < 5; i++)
//   {
//       await Task.Run(() => DoWork(i)); //异步等待任务完成
//   }
//}
////↓在适当的地方调用该方法并使用await等待它完成
//await ExecuteTasksSequentiallyAsync();

#endif
