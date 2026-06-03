//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    public class SpriteLoader
    {
        [Serializable]
        public class MetaData
        {
            public string name;
            public Rect rect;
            public Vector2 pivot;
            public Vector4 border;
        }

        [Serializable]
        public class SheetConfig
        {
            public List<MetaData> sprites;
            public float pixelsPerUnit = 32f; // 从 meta 文件中获取的 spritePixelsToUnits
        }

        /// <summary>
        /// 从 PNG 图片和对应的 meta 文件加载 Sprite 数组
        /// </summary>
        /// <param name="pngPath">PNG 图片文件路径</param>
        /// <param name="metaPath">meta 文件路径（可以是 .meta 或 .txt）</param>
        /// <returns>Sprite 数组，按名称排序</returns>
        public static Sprite[] LoadSprites(string pngPath, string metaPath)
        {
            // 1. 加载 PNG 图片
            if (!File.Exists(pngPath))
            {
                Debug.LogError($"PNG 文件不存在: {pngPath}");
                return null;
            }

            byte[] pngData = File.ReadAllBytes(pngPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!texture.LoadImage(pngData))
            {
                Debug.LogError("无法加载 PNG 图片数据");
                return null;
            }

            texture.filterMode = FilterMode.Point; // 像素风格游戏常用
            texture.Apply();

            // 2. 解析 meta 文件
            if (!File.Exists(metaPath))
            {
                Debug.LogError($"Meta 文件不存在: {metaPath}");
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            string metaContent = File.ReadAllText(metaPath);
            SheetConfig config = ParseMetaFile(metaContent);

            if (config == null || config.sprites == null || config.sprites.Count == 0)
            {
                Debug.LogError("无法解析 meta 文件或未找到精灵数据");
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            // 3. 创建 Sprite 数组
            Sprite[] sprites = new Sprite[config.sprites.Count];

            for (int i = 0; i < config.sprites.Count; i++)
            {
                var spriteData = config.sprites[i];

                // 注意：Unity 纹理坐标原点在左下角，Y 轴向上
                // meta 文件中的坐标已经是正确的 Unity 坐标系统
                sprites[i] = Sprite.Create(
                    texture,
                    spriteData.rect,
                    spriteData.pivot,
                    config.pixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect,
                    spriteData.border
                );
                sprites[i].name = spriteData.name;
            }

            // 4. 按名称排序（可选，保持一致性）
            Array.Sort(sprites, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

            return sprites;
        }

        /// <summary>
        /// 解析 Unity meta 文件，提取精灵数据
        /// </summary>
        private static SheetConfig ParseMetaFile(string metaContent)
        {
            SheetConfig config = new SheetConfig();
            config.sprites = new List<MetaData>();

            // 提取 spritePixelsToUnits
            int pixelsPerUnitIndex = metaContent.IndexOf("spritePixelsToUnits:");
            if (pixelsPerUnitIndex != -1)
            {
                string pixelsPerUnitLine = metaContent.Substring(pixelsPerUnitIndex);
                int colonIndex = pixelsPerUnitLine.IndexOf(":");
                int newlineIndex = pixelsPerUnitLine.IndexOf("\n");

                if (colonIndex != -1 && newlineIndex != -1)
                {
                    string valueStr = pixelsPerUnitLine.Substring(colonIndex + 1, newlineIndex - colonIndex - 1).Trim();
                    if (float.TryParse(valueStr, out float pixelsPerUnit))
                    {
                        config.pixelsPerUnit = pixelsPerUnit;
                    }
                }
            }

            // 查找 spriteSheet 部分
            int spriteSheetIndex = metaContent.IndexOf("spriteSheet:");
            if (spriteSheetIndex == -1) return config;

            // 查找 sprites 数组开始
            int spritesStartIndex = metaContent.IndexOf("sprites:", spriteSheetIndex);
            if (spritesStartIndex == -1) return config;

            // 解析每个精灵
            int currentIndex = spritesStartIndex;

            while (true)
            {
                // 查找下一个精灵的开始
                int spriteStart = metaContent.IndexOf("- serializedVersion: 2", currentIndex);
                if (spriteStart == -1) break;

                // 查找这个精灵的结束位置
                int spriteEnd = metaContent.IndexOf("\n    - serializedVersion: 2", spriteStart + 1);
                if (spriteEnd == -1)
                {
                    // 最后一个精灵
                    spriteEnd = metaContent.IndexOf("\n    outline: []", spriteStart);
                    if (spriteEnd == -1) break;
                    spriteEnd = metaContent.IndexOf("\n    physicsShape: []", spriteEnd);
                    if (spriteEnd == -1) break;
                    spriteEnd = metaContent.IndexOf("\n    bones: []", spriteEnd);
                    if (spriteEnd == -1) break;
                    spriteEnd = metaContent.IndexOf("\n    spriteID:", spriteEnd);
                    if (spriteEnd == -1) break;
                    spriteEnd = metaContent.IndexOf("\n", spriteEnd + 1);
                }

                if (spriteEnd == -1) break;

                string spriteBlock = metaContent.Substring(spriteStart, spriteEnd - spriteStart);
                MetaData spriteData = ParseSpriteBlock(spriteBlock);

                if (spriteData != null)
                {
                    config.sprites.Add(spriteData);
                }

                currentIndex = spriteEnd;
            }

            return config;
        }

        /// <summary>
        /// 解析单个精灵的数据块
        /// </summary>
        private static MetaData ParseSpriteBlock(string spriteBlock)
        {
            MetaData data = new MetaData();

            // 提取名称
            int nameIndex = spriteBlock.IndexOf("name:");
            if (nameIndex != -1)
            {
                int nameStart = spriteBlock.IndexOf(" ", nameIndex) + 1;
                int nameEnd = spriteBlock.IndexOf("\n", nameStart);
                if (nameEnd != -1)
                {
                    data.name = spriteBlock.Substring(nameStart, nameEnd - nameStart).Trim();
                }
            }

            // 提取 rect
            int rectIndex = spriteBlock.IndexOf("rect:");
            if (rectIndex != -1)
            {
                // 提取 x
                int xIndex = spriteBlock.IndexOf("x:", rectIndex);
                if (xIndex != -1)
                {
                    int xStart = spriteBlock.IndexOf(" ", xIndex) + 1;
                    int xEnd = spriteBlock.IndexOf("\n", xIndex);
                    if (xEnd != -1 && float.TryParse(spriteBlock.Substring(xStart, xEnd - xStart).Trim(), out float x))
                    {
                        // 提取 y
                        int yIndex = spriteBlock.IndexOf("y:", xEnd);
                        if (yIndex != -1)
                        {
                            int yStart = spriteBlock.IndexOf(" ", yIndex) + 1;
                            int yEnd = spriteBlock.IndexOf("\n", yIndex);
                            if (yEnd != -1 && float.TryParse(spriteBlock.Substring(yStart, yEnd - yStart).Trim(), out float y))
                            {
                                // 提取 width
                                int widthIndex = spriteBlock.IndexOf("width:", yEnd);
                                if (widthIndex != -1)
                                {
                                    int widthStart = spriteBlock.IndexOf(" ", widthIndex) + 1;
                                    int widthEnd = spriteBlock.IndexOf("\n", widthIndex);
                                    if (widthEnd != -1 && float.TryParse(spriteBlock.Substring(widthStart, widthEnd - widthStart).Trim(), out float width))
                                    {
                                        // 提取 height
                                        int heightIndex = spriteBlock.IndexOf("height:", widthEnd);
                                        if (heightIndex != -1)
                                        {
                                            int heightStart = spriteBlock.IndexOf(" ", heightIndex) + 1;
                                            int heightEnd = spriteBlock.IndexOf("\n", heightIndex);
                                            if (heightEnd != -1 && float.TryParse(spriteBlock.Substring(heightStart, heightEnd - heightStart).Trim(), out float height))
                                            {
                                                data.rect = new Rect(x, y, width, height);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 提取 pivot
            int pivotIndex = spriteBlock.IndexOf("pivot:");
            if (pivotIndex != -1)
            {
                int xStart = spriteBlock.IndexOf("{x:", pivotIndex) + 3;
                int xEnd = spriteBlock.IndexOf(",", xStart);
                int yStart = spriteBlock.IndexOf("y:", xEnd) + 2;
                int yEnd = spriteBlock.IndexOf("}", yStart);

                if (xEnd != -1 && yEnd != -1)
                {
                    string xStr = spriteBlock.Substring(xStart, xEnd - xStart).Trim();
                    string yStr = spriteBlock.Substring(yStart, yEnd - yStart).Trim();

                    if (float.TryParse(xStr, out float pivotX) && float.TryParse(yStr, out float pivotY))
                    {
                        data.pivot = new Vector2(pivotX, pivotY);
                    }
                }
            }

            // 提取 border（默认为 0）
            data.border = Vector4.zero;
            int borderIndex = spriteBlock.IndexOf("border:");
            if (borderIndex != -1)
            {
                int xStart = spriteBlock.IndexOf("{x:", borderIndex) + 3;
                int xEnd = spriteBlock.IndexOf(",", xStart);
                int yStart = spriteBlock.IndexOf("y:", xEnd) + 2;
                int yEnd = spriteBlock.IndexOf(",", yStart);
                int zStart = spriteBlock.IndexOf("z:", yEnd) + 2;
                int zEnd = spriteBlock.IndexOf(",", zStart);
                int wStart = spriteBlock.IndexOf("w:", zEnd) + 2;
                int wEnd = spriteBlock.IndexOf("}", wStart);

                if (xEnd != -1 && yEnd != -1 && zEnd != -1 && wEnd != -1)
                {
                    string xStr = spriteBlock.Substring(xStart, xEnd - xStart).Trim();
                    string yStr = spriteBlock.Substring(yStart, yEnd - yStart).Trim();
                    string zStr = spriteBlock.Substring(zStart, zEnd - zStart).Trim();
                    string wStr = spriteBlock.Substring(wStart, wEnd - wStart).Trim();

                    if (float.TryParse(xStr, out float borderX) &&
                        float.TryParse(yStr, out float borderY) &&
                        float.TryParse(zStr, out float borderZ) &&
                        float.TryParse(wStr, out float borderW))
                    {
                        data.border = new Vector4(borderX, borderY, borderZ, borderW);
                    }
                }
            }

            return data;
        }
    }
}

#region 使用示例
//public static void ExampleUsage()
//{
//    // 假设你的文件路径
//    string pngPath = @"C:\YourMod\Resources\MiscRes.png";
//    string metaPath = @"C:\YourMod\Resources\MiscRes.png.meta.txt"; // 或者 .meta

//    // 加载精灵数组
//    Sprite[] allSprites = LoadSprites(pngPath, metaPath);

//    if (allSprites != null)
//    {
//        Debug.Log($"成功加载 {allSprites.Length} 个精灵");

//        // 按类别筛选（根据你的需求）
//        List<Sprite> fontSprites = new List<Sprite>();
//        List<Sprite> explosionSprites = new List<Sprite>();

//        foreach (var sprite in allSprites)
//        {
//            if (sprite.name.StartsWith("font_outline_"))
//            {
//                fontSprites.Add(sprite);
//            }
//            else if (sprite.name.StartsWith("explosion_"))
//            {
//                explosionSprites.Add(sprite);
//            }
//        }

//        // 转换为数组
//        Sprite[] sprites_font_outline = fontSprites.ToArray();
//        Sprite[] sprites_explosions = explosionSprites.ToArray();

//        // 现在你可以使用这些数组了
//        Debug.Log($"字体轮廓精灵数量: {sprites_font_outline.Length}");
//        Debug.Log($"爆炸效果精灵数量: {sprites_explosions.Length}");
//    }
//}
#endregion

#endif
