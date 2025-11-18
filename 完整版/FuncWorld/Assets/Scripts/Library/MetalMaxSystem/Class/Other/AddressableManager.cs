#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// Unity 2022 LTS 资源管理器
    /// 封装 Addressables 系统,提供简单易用的API,但对加载速度有极致要求时用旧的AB包系统即可.
    /// Addressables建立在Asset Bundle技术之上,相比Resources系统只能读取规定文件夹素材,它解决了诸多限制.
    /// 开启"Build Remote Catalog"选项可支持热更新.
    /// </summary>
    public static class AddressableManager
    {
        private static Dictionary<string, AsyncOperationHandle> activeHandles = new Dictionary<string, AsyncOperationHandle>();
        private static Dictionary<object, List<AsyncOperationHandle>> labelHandles = new Dictionary<object, List<AsyncOperationHandle>>();

        /// <summary>
        /// 初始化资源系统
        /// </summary>
        public static async Task Initialize()
        {
            // 检查并更新资源目录
            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            await checkHandle.Task;

            if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
            {
                Debug.Log($"Found {checkHandle.Result.Count} catalog updates");
                var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, true);
                await updateHandle.Task;
            }

            Addressables.Release(checkHandle);
        }

        /// <summary>
        /// 加载单个资源
        /// </summary>
        /// <param name="assetKey">资源地址</param>
        /// <param name="callback">加载完成回调</param>
        public static async Task<T> LoadAsset<T>(string assetKey) where T : UnityEngine.Object
        {
            try
            {
                // 检查是否已加载
                if (activeHandles.TryGetValue(assetKey, out var existingHandle))
                {
                    if (existingHandle.IsDone)
                        return (T)existingHandle.Result;

                    await existingHandle.Task;
                    return (T)existingHandle.Result;
                }

                // 创建新加载操作
                var handle = Addressables.LoadAssetAsync<T>(assetKey);
                activeHandles.Add(assetKey, handle);

                // 等待加载完成
                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    activeHandles.Remove(assetKey);
                    throw new Exception($"Failed to load asset: {assetKey}. Error: {handle.OperationException}");
                }

                return handle.Result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Resource load error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 按标签加载资源组
        /// </summary>
        public static async Task<List<T>> LoadAssetsByLabel<T>(object label) where T : UnityEngine.Object
        {
            var results = new List<T>();
            var handle = Addressables.LoadAssetsAsync<T>(label, null);

            // 记录按标签加载的句柄
            if (!labelHandles.ContainsKey(label))
                labelHandles[label] = new List<AsyncOperationHandle>();

            labelHandles[label].Add(handle);

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                results.AddRange(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load assets by label: {label}");
            }

            return results;
        }

        /// <summary>
        /// 实例化游戏对象
        /// </summary>
        public static async Task<GameObject> InstantiateAsync(string assetKey, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            try
            {
                var handle = Addressables.InstantiateAsync(assetKey, position, rotation, parent);
                string instanceId = $"{assetKey}_{Guid.NewGuid()}";
                activeHandles.Add(instanceId, handle);

                await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    activeHandles.Remove(instanceId);
                    throw new Exception($"Failed to instantiate: {assetKey}");
                }

                var go = handle.Result;
                go.AddComponent<ResourceInstance>().Initialize(instanceId);
                return go;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Instantiate error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public static void Release(string assetKey)
        {
            if (activeHandles.TryGetValue(assetKey, out var handle))
            {
                Addressables.Release(handle);
                activeHandles.Remove(assetKey);
            }
        }

        /// <summary>
        /// 按标签释放资源
        /// </summary>
        public static void ReleaseAssetsByLabel(object label)
        {
            if (labelHandles.TryGetValue(label, out var handles))
            {
                foreach (var handle in handles)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                }
                labelHandles.Remove(label);
            }
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var handle in activeHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            activeHandles.Clear();

            foreach (var labelList in labelHandles.Values)
            {
                foreach (var handle in labelList)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                }
            }
            labelHandles.Clear();
        }

        /// <summary>
        /// 检查资源是否存在
        /// </summary>
        public static async Task<bool> AssetExists(string assetKey)
        {
            var handle = Addressables.LoadResourceLocationsAsync(assetKey);
            await handle.Task;
            bool exists = handle.Result != null && handle.Result.Count > 0;
            Addressables.Release(handle);
            return exists;
        }

        /// <summary>
        /// 获取资源下载大小
        /// </summary>
        public static async Task<long> GetDownloadSize(string assetKey)
        {
            return await Addressables.GetDownloadSizeAsync(assetKey).Task;
        }
    }

    /// <summary>
    /// 用于跟踪实例化资源的组件
    /// </summary>
    public class ResourceInstance : MonoBehaviour
    {
        private string instanceId;

        public void Initialize(string id)
        {
            instanceId = id;
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(instanceId))
            {
                AddressableManager.Release(instanceId);
            }
        }
    }
}

//// 初始化资源系统,在游戏启动时调用
//await AddressableManager.Initialize();

//// 加载单个资源
//var material = await AddressableManager.LoadAsset<Material>("Materials/Environment/Rock");
//// 按标签加载一组资源
//var enemyPrefabs = await AddressableManager.LoadAssetsByLabel<GameObject>("Enemies");

//// 实例化预制件
//var player = await AddressableManager.InstantiateAsync("Prefabs/Characters/Player");

//// 释放单个资源
//AddressableManager.Release("Materials/Environment/Rock");
//// 按标签释放资源
//AddressableManager.ReleaseAssetsByLabel("Enemies");
//// 释放所有资源(场景切换时调用)
//AddressableManager.ReleaseAll();

//// 旧AB包系统(对加载速度有极致要求时直接使用)
//var bundle = AssetBundle.LoadFromFile("environment");
//var material = bundle.LoadAsset<Material>("rock");
//// 替换为Addressables
//var material = await Addressables.LoadAssetAsync<Material>("Materials/rock");

////‌热更新
//public static async Task CheckForUpdates()
//{
//    var catalogs = await Addressables.CheckForCatalogUpdates().Task;
//    if (catalogs.Count > 0)
//    {
//        await Addressables.UpdateCatalogs(catalogs).Task;
//    }
//}
#endif