#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// Unity通用方法类.
    /// </summary>
    public class UnityUtilities : MonoBehaviour
    {
        public static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// 查找物体.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static GameObject FindGameObject(GameObject parent, string childName)
        {
            //获取所有Transform组件实例（包括隐藏的）
            Transform[] allChildren = parent.transform.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                //Debug.Log($"Checking child: {child.gameObject.name}");
                if (child.gameObject.name == childName)
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// 检测游戏物体是否包含Transform组件外的组件，有则返回true.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>当发现任何非Transform组件时返回true,否则全部遍历结束返回false</returns>
        public static bool HasEssentialComponents(GameObject obj)
        {
            return obj.GetComponents<Component>().Where(c => c != null).Any(c => !(c is Transform));
        }
        /// <summary>
        /// 检测游戏物体是否含指定名称外的组件，有则返回true.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="excludeTypeNames">任意组件类型名称字符串</param>
        /// <returns></returns>
        public static bool HasComponentsExcluding(GameObject obj, string[] excludeComponentNames)
        {
            return obj.GetComponents<Component>()
                .Where(c => c != null)
                .Any(c => !excludeComponentNames.Contains(c.GetType().Name)
                       && !excludeComponentNames.Contains(c.GetType().FullName));
        }

        /// <summary>
        /// 加载外部图片并分割为精灵数组.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="isTopLeftOrigin"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Sprite[] SplitImageToSprites(string path, int gridX, int gridY, bool isTopLeftOrigin = false, float pixelsPerUnit = 100f)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(path);

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D sourceTexture = new Texture2D(2, 2);
            sourceTexture.LoadImage(bytes);
            sourceTexture.filterMode = FilterMode.Bilinear;

            int cellWidth = sourceTexture.width / gridX;
            int cellHeight = sourceTexture.height / gridY;
            List<Sprite> sprites = new List<Sprite>();

            for (int y = 0; y < gridY; y++)
            {
                int currentY = isTopLeftOrigin ?
                    (gridY - 1 - y) : y;

                for (int x = 0; x < gridX; x++)
                {
                    Rect rect = new Rect(
                        x * cellWidth,
                        currentY * cellHeight,
                        cellWidth,
                        cellHeight
                    );

                    Sprite sprite = Sprite.Create(
                        sourceTexture,
                        rect,
                        new Vector2(0.5f, 0.5f),
                        pixelsPerUnit
                    );
                    sprite.name = $"slice_{x}_{y}";
                    sprites.Add(sprite);
                }
            }
            return sprites.ToArray();
        }
    }
}
#endif
