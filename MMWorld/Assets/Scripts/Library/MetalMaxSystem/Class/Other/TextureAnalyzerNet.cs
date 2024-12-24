#if !(UNITY_EDITOR || UNITY_STANDALONE)
#if NET8_0 || NET9_0
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Text;
#if NET8_0
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System;
#endif

//↓可使用.Net中的Debug.WriteLine
using Debug = System.Diagnostics.Debug;

namespace MetalMaxSystem.Net
{
    public class TextureAnalyzerNet
    {
        const int MAX_TEXTURE_SIZE = 8192;
        static int iCountMax = 10000;
        ConcurrentDictionary<int, Color[]> spritePixels = new ConcurrentDictionary<int, Color[]>();

        #region 功能函数
        /// <summary>
        /// 启动一个Task处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套（特征）纹理集及文本到指定目录
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">要搜索的图片后缀如"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">协程处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        public void StartSliceTextureAndSetSpriteIDMultiMergerAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8)
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
                        xCount
                    )
            );
        }
        /// <summary>
        /// Task处理目标目录下指定后缀图片并分割成精灵，然后根据纹理像素相似度给组中精灵编号并保存配套（特征）纹理集及文本到指定目录
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern">要搜索的图片后缀如"*.png"、"*.jpg"</param>
        /// <param name="similarity">相似度</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/name/Desktop/MapSP/"</param>
        /// <param name="savePathFrontIndex">输出纹理文本的目录前缀字符如"C:/Users/name/Desktop/MapIndex/"</param>
        /// <param name="handleCountMax">协程处理量</param>
        /// <param name="pixelX">格子像素尺寸</param>
        /// <param name="pixelY">格子像素尺寸</param>
        /// <param name="xCount">合并纹理图时的X方向输出格子数，超过换行</param>
        public async Task SliceTextureAndSetSpriteIDMultiMergerAsync(string folderPath, string searchPattern, float similarity, string savePathFrontSP, string savePathFrontIndex, int handleCountMax = 10, int pixelX = 16, int pixelY = 16, int xCount = 8)
        {
            Bitmap texture;
            List<Bitmap> sprites;
            StringBuilder sb = new StringBuilder();
            Color[] currentPixels;
            Color[] comparisonPixels;
            string fileName;
            string fileSavePath;
            float sim;
            int currentID = 1;
            int handleCount = 0;
            int jCount = 0;
            int fileCount = -1;
            int sliceMaxId = 1;
            List<Bitmap> spritesList = new List<Bitmap>();
            Dictionary<int, string> DataTableISCP = new Dictionary<int, string>();
            string[] filePaths = Directory.GetFiles(folderPath, searchPattern);

            foreach (string filePath in filePaths)
            {
                fileCount++;
                sb.Clear();
                texture = LoadImageAndConvertToBitmap(filePath);
                sprites = SliceBitmap(texture, pixelX, pixelY);
                if (sprites == null || sprites.Count == 0)
                {
#if UNITY_EDITOR || UNITY_STANDALONE
                Debug.Log("未找到资源");
#else
                    Debug.WriteLine("未找到资源");
#endif
                    continue;
                }
                else
                {
                    fileName = Path.GetFileNameWithoutExtension(filePath);
                    fileSavePath = savePathFrontIndex + fileName + ".txt";
                    int[] sliceIds = new int[sprites.Count + 1];
                    for (int ei = 0; ei < sprites.Count; ei++)
                    {
                        sliceIds[ei] = 0;
                    }
                    if (fileCount == 0)
                    {
                        sliceIds[0] = 1;
                        for (int i = 0; i < sprites.Count; i++)
                        {
                            if (i != 0)
                            {
                                if (jCount != -1)
                                {
                                    for (int ai = 0; ai <= jCount; ai++)
                                    {
                                        if (
                                            DataTableISCP.ContainsKey(sliceIds[ai])
                                            && DataTableISCP[sliceIds[ai]] == "true"
                                        )
                                        {
                                            DataTableISCP[sliceIds[ai]] = "";
                                        }
                                    }
                                    jCount = -1;
                                }
                                handleCount += 1;
                                currentPixels = GetPixels(sprites[i]);
                                for (int j = 0; j < i; j++)
                                {
                                    jCount = j;
                                    if (
                                        !DataTableISCP.ContainsKey(sliceIds[j])
                                        || DataTableISCP[sliceIds[j]] != "true"
                                    )
                                    {
                                        DataTableISCP[sliceIds[j]] = "true";
                                    }
                                    else
                                    {
                                        if (i == j + 1)
                                        {
                                            sliceMaxId++;
                                            sliceIds[i] = sliceMaxId;
                                        }
                                        continue;
                                    }
                                    comparisonPixels = GetPixels(sprites[j]);
                                    sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);
                                    if (sim >= similarity)
                                    {
                                        sliceIds[i] = sliceIds[j];
                                        if (sliceIds[j] == 0)
                                        {
#if UNITY_EDITOR || UNITY_STANDALONE
                                        Debug.Log(
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
#else
                                            Debug.WriteLine(
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
#endif
                                        }
                                        break;
                                    }
                                    else if (j == i - 1)
                                    {
                                        sliceMaxId++;
                                        sliceIds[i] = sliceMaxId;
                                        if (sliceIds[i] == 0)
                                        {
#if UNITY_EDITOR || UNITY_STANDALONE
                                        Debug.Log(
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
#else
                                            Debug.WriteLine(
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
#endif
                                        }
                                    }
                                }
                            }
                            sb.Append(sliceIds[i]);
                            if (sliceIds[i] == currentID)
                            {
                                spritesList.Add(sprites[i]);
                                currentID++;
                            }
                            if (i < sprites.Count - 1)
                            {
                                sb.Append(",");
                            }
                            if (handleCount >= handleCountMax)
                            {
                                handleCount = 0;
#if UNITY_EDITOR || UNITY_STANDALONE
                            Debug.Log(
                                "图片["
                                    + (fileCount + 1)
                                    + "/"
                                    + filePaths.Length
                                    + "]处理中.."
                                    + (i + 1)
                                    + "/"
                                    + sprites.Count
                                    + " 当前最大编号："
                                    + (currentID - 1)
                            );
#else
                                Debug.WriteLine(
                                    "图片["
                                        + (fileCount + 1)
                                        + "/"
                                        + filePaths.Length
                                        + "]处理中.."
                                        + (i + 1)
                                        + "/"
                                        + sprites.Count
                                        + " 当前最大编号："
                                        + (currentID - 1)
                                );
#endif
                                await Task.Delay(1); //模拟协程延迟
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < sprites.Count; i++)
                        {
                            //处理后续文件的逻辑
                            currentPixels = GetPixels(sprites[i]);
                            for (int j = 0; j < spritesList.Count; j++)
                            {
                                comparisonPixels = GetPixels(sprites[j]);
                                sim = CalculateSimilarity(currentPixels, comparisonPixels, similarity);
                                if (sim >= similarity)
                                {
                                    sliceIds[i] = j + 1;
                                    break;
                                }
                                else if (j == spritesList.Count - 1)
                                {
                                    sliceMaxId++;
                                    sliceIds[i] = sliceMaxId;
                                }
                            }
                            sb.Append(sliceIds[i]);
                            if (sliceIds[i] == currentID)
                            {
                                spritesList.Add(sprites[i]);
                                currentID++;
                            }
                            if (i < sprites.Count - 1)
                            {
                                sb.Append(",");
                            }
                            if (handleCount >= handleCountMax)
                            {
                                handleCount = 0;
#if UNITY_EDITOR || UNITY_STANDALONE
                            Debug.Log(
                                "图片["
                                    + (fileCount + 1)
                                    + "/"
                                    + filePaths.Length
                                    + "]协程处理中.."
                                    + (i + 1)
                                    + "/"
                                    + sprites.Count
                                    + " 当前最大编号："
                                    + (currentID - 1)
                            );
#else
                                Debug.WriteLine(
                                    "图片["
                                        + (fileCount + 1)
                                        + "/"
                                        + filePaths.Length
                                        + "]协程处理中.."
                                        + (i + 1)
                                        + "/"
                                        + sprites.Count
                                        + " 当前最大编号："
                                        + (currentID - 1)
                                );
#endif
                                await Task.Delay(1); //模拟协程延迟
                            }
                        }
                    }
                    MMCore.SaveFile(fileSavePath, sb.ToString());
#if UNITY_EDITOR || UNITY_STANDALONE
                Debug.Log("保存成功: " + fileSavePath);
#else
                    Debug.WriteLine("保存成功: " + fileSavePath);
#endif
                }
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("已处理图片：" + fileCount);
#else
                Debug.WriteLine("已处理图片：" + fileCount);
#endif
            }
            SpriteMerger(spritesList, "SpriteMerger", xCount, savePathFrontSP);
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log("处理完成！");
#else
            Debug.WriteLine("处理完成！");
#endif
        }

        #endregion

        #region 辅助函数
        /// <summary>
        /// 合成纹理集
        /// </summary>
        /// <param name="sprites"></param>
        /// <param name="fileName"></param>
        /// <param name="maxSpritesPerRow">每行最多放置的精灵数量</param>
        /// <param name="savePathFrontSP">输出纹理集图片的目录前缀字符如"C:/Users/userName/Desktop/MapSP/"</param>
        public static void SpriteMerger(List<Bitmap> sprites, string fileName, int maxSpritesPerRow, string savePathFrontSP)
        {
            int row;
            int col;
            int x;
            int y;
            int iCount;
            int spriteWidth;
            int spriteHeight;
            int totalRows;
            int totalWidth;
            int totalHeight;
            Color[] spriteColors;
            if (sprites.Count == 0)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("No sprites to merge.");
#else
                Debug.WriteLine("No sprites to merge.");
#endif
            }
            else
            {
                iCount = 0;
                spriteWidth = sprites[0].Width; ;
                spriteHeight = sprites[0].Height;
                totalRows = (sprites.Count + maxSpritesPerRow - 1) / maxSpritesPerRow;
                totalWidth = maxSpritesPerRow * spriteWidth;
                totalHeight = totalRows * spriteHeight;
                if (totalWidth > MAX_TEXTURE_SIZE || totalHeight > MAX_TEXTURE_SIZE)
                {
#if UNITY_EDITOR || UNITY_STANDALONE
                Debug.Log(
                    "totalRows："
                        + totalRows
                        + "totalWidth："
                        + totalWidth
                        + "totalHeight："
                        + totalHeight
                );
#else
                    Debug.WriteLine(
                        "totalRows："
                            + totalRows
                            + "totalWidth："
                            + totalWidth
                            + "totalHeight："
                            + totalHeight
                    );
#endif
                    if (totalWidth > MAX_TEXTURE_SIZE && totalHeight > MAX_TEXTURE_SIZE)
                    {
#if UNITY_EDITOR || UNITY_STANDALONE
                    Debug.Log(
                        "Both width and height exceed the maximum texture size limit of "
                            + MAX_TEXTURE_SIZE
                    );
#else
                        Debug.WriteLine(
                            "Both width and height exceed the maximum texture size limit of "
                                + MAX_TEXTURE_SIZE
                        );
#endif
                        return;
                    }
                    if (totalWidth > MAX_TEXTURE_SIZE)
                    {
                        int excessWidth = totalWidth - MAX_TEXTURE_SIZE;
                        int numWidth = excessWidth / spriteWidth;
                        totalWidth = totalWidth - excessWidth;
                        totalHeight = totalHeight + excessWidth;
                        if (totalHeight > MAX_TEXTURE_SIZE)
                        {
#if UNITY_EDITOR || UNITY_STANDALONE
                        Debug.Log(
                            "totalHeight exceed the maximum texture size limit of "
                                + MAX_TEXTURE_SIZE
                        );
#else
                            Debug.WriteLine(
                                "totalHeight exceed the maximum texture size limit of "
                                    + MAX_TEXTURE_SIZE
                            );
#endif
                            return;
                        }
                    }
                    else if (totalHeight > MAX_TEXTURE_SIZE)
                    {
                        int excessWidth = totalHeight - MAX_TEXTURE_SIZE;
                        int numWidth = excessWidth / spriteWidth;
                        totalHeight = totalHeight - excessWidth;
                        totalWidth = totalWidth + excessWidth;
                        if (totalWidth > MAX_TEXTURE_SIZE)
                        {
#if UNITY_EDITOR || UNITY_STANDALONE
                        Debug.Log(
                            "totalWidth exceed the maximum texture size limit of "
                                + MAX_TEXTURE_SIZE
                        );
#else
                            Debug.WriteLine(
                                "totalWidth exceed the maximum texture size limit of "
                                    + MAX_TEXTURE_SIZE
                            );
#endif
                            return;
                        }
                    }
                }

                //创建一个指定宽度和高度的Bitmap对象
                Bitmap mergedTexture = new Bitmap(totalWidth, totalHeight);

                //创建一个Graphics对象以便我们可以在Bitmap上绘图
                using (Graphics g = Graphics.FromImage(mergedTexture))
                {
                    //清除背景为透明（因为默认背景是黑色）
                    g.Clear(Color.Transparent);

                    //此时，Bitmap的PixelFormat默认是Format32bppArgb（即RGBA32）
                    //如果你需要确认，可以检查mergedTexture.PixelFormat属性
                    if (mergedTexture.PixelFormat == PixelFormat.Format32bppArgb)
                    {
#if UNITY_EDITOR || UNITY_STANDALONE
                    Debug.Log("Bitmap is using RGBA32 format.");
#else
                        Debug.WriteLine("Bitmap is using RGBA32 format.");
#endif
                    }
                    else
                    {
#if UNITY_EDITOR || UNITY_STANDALONE
                    Debug.Log("Bitmap is not using RGBA32 format. Actual format: " + mergedTexture.PixelFormat);
#else
                        Debug.WriteLine("Bitmap is not using RGBA32 format. Actual format: " + mergedTexture.PixelFormat);
#endif
                    }
                }
                for (int i = 0; i < sprites.Count; i++)
                {
                    iCount++;
                    row = i / maxSpritesPerRow;
                    col = i % maxSpritesPerRow;
                    x = col * spriteWidth;
                    y = row * spriteHeight;
                    spriteColors = GetPixels(sprites[i]);
                    for (int spriteY = 0; spriteY < spriteHeight; spriteY++)
                    {
                        for (int spriteX = 0; spriteX < spriteWidth; spriteX++)
                        {
                            mergedTexture.SetPixel(
                                x + spriteX,
                                y + spriteY,
                                spriteColors[spriteY * spriteWidth + spriteX]
                            );
                        }
                    }
                    if (iCount >= iCountMax)
                    {
                        iCount = 0;
                    }
                }
                string savePath = savePathFrontSP + fileName + ".png";
                mergedTexture.Save(savePath, ImageFormat.Png);
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("Merged sprites saved to: " + savePath);
#else
                Debug.WriteLine("Merged sprites saved to: " + savePath);
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
        private float CalculateSimilarity(Color[] pixels1, Color[] pixels2)
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
#if UNITY_EDITOR || UNITY_STANDALONE
        //float maxAllowedDifference = 255 * allowedDifference;//Unity中的RGBA是0~1的浮点数，只有1种情况例外需改渲染模式为非URP（或修改Shader）似可处理255参数
        float maxAllowedDifference = allowedDifference;
        return Mathf.Abs(color1.r - color2.r) <= maxAllowedDifference &&
               Mathf.Abs(color1.g - color2.g) <= maxAllowedDifference &&
               Mathf.Abs(color1.b - color2.b) <= maxAllowedDifference &&
               Mathf.Abs(color1.a - color2.a) <= maxAllowedDifference;
#else
            float maxAllowedDifference = 255 * allowedDifference;//System.Drawing.Color的RGBA分量是0~255的整数，因此需要将允许的差异乘以255
            return Math.Abs(color1.R - color2.R) <= maxAllowedDifference &&
                   Math.Abs(color1.G - color2.G) <= maxAllowedDifference &&
                   Math.Abs(color1.B - color2.B) <= maxAllowedDifference &&
                   Math.Abs(color1.A - color2.A) <= maxAllowedDifference;
#endif
        }

        /// <summary>
        /// 将纹理切割成多个切片，并返回包含这些切片的Bitmap数组
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="pixelX">像素宽</param>
        /// <param name="pixelY">像素高</param>
        /// <returns></returns>
        public static List<Bitmap> SliceBitmap(Bitmap bitmap, int pixelX, int pixelY)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            List<Bitmap> slices = new List<Bitmap>();
            for (int y = 0; y < height; y += pixelY)
            {
                for (int x = 0; x < width; x += pixelX)
                {
                    int sliceWidth = Math.Min(pixelX, width - x);
                    int sliceHeight = Math.Min(pixelY, height - y);
                    var slice = bitmap.Clone(new Rectangle(x, y, sliceWidth, sliceHeight), bitmap.PixelFormat);
                    slices.Add(slice);
                }
            }
            return slices;
        }

        /// <summary>
        /// 获取位图的（像素点）颜色数组
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
#endif
#endif
