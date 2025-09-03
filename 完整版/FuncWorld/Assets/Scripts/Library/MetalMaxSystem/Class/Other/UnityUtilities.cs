#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// Unity通用方法类.
    /// </summary>
    public class UnityUtilities : MonoBehaviour
    {
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
    }
}
#endif
