using UnityEngine;
using System.Collections.Generic;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 资源管理器
    /// 管理游戏中的所有资源
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        #region 单例

        public static ResourceManager Instance { get; private set; }

        #endregion

        #region 资源存储

        /// <summary>
        /// 全局资源存储
        /// </summary>
        public Dictionary<ThingDef, int> globalResources = new Dictionary<ThingDef, int>();

        /// <summary>
        /// 存储设施列表
        /// </summary>
        public List<StorageBuilding> storageBuildings = new List<StorageBuilding>();

        #endregion

        #region 设置

        /// <summary>
        /// 资源过期时间(秒)
        /// </summary>
        public float resourceDecayTime = 3600f;

        #endregion

        #region 事件

        public event Action<ThingDef, int> OnResourceChanged;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultResources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化默认资源
        /// </summary>
        private void InitializeDefaultResources()
        {
            // 添加初始资源
            AddResource(ThingDefDatabase.GetThingDef("Wood"), 50);
            AddResource(ThingDefDatabase.GetThingDef("Stone"), 30);
            AddResource(ThingDefDatabase.GetThingDef("Steel"), 20);
            AddResource(ThingDefDatabase.GetThingDef("RawMeat"), 20);
            AddResource(ThingDefDatabase.GetThingDef("Vegetable"), 30);
            AddResource(ThingDefDatabase.GetThingDef("MedicineHerbal"), 10);
        }

        #endregion

        #region 资源操作

        /// <summary>
        /// 添加资源
        /// </summary>
        public void AddResource(ThingDef thingDef, int amount)
        {
            if (thingDef == null || amount <= 0) return;

            if (globalResources.ContainsKey(thingDef))
            {
                globalResources[thingDef] += amount;
            }
            else
            {
                globalResources[thingDef] = amount;
            }

            OnResourceChanged?.Invoke(thingDef, globalResources[thingDef]);
            Debug.Log($"添加资源: {thingDef.label} x{amount}");
        }

        /// <summary>
        /// 消耗资源
        /// </summary>
        public bool ConsumeResource(ThingDef thingDef, int amount)
        {
            if (thingDef == null || amount <= 0) return false;
            if (!HasEnoughResources(thingDef, amount)) return false;

            globalResources[thingDef] -= amount;
            OnResourceChanged?.Invoke(thingDef, globalResources[thingDef]);

            Debug.Log($"消耗资源: {thingDef.label} x{amount}");
            return true;
        }

        /// <summary>
        /// 检查资源是否足够
        /// </summary>
        public bool HasEnoughResources(ThingDef thingDef, int amount)
        {
            if (thingDef == null || amount <= 0) return true;

            if (globalResources.TryGetValue(thingDef, out int count))
            {
                return count >= amount;
            }
            return false;
        }

        /// <summary>
        /// 获取资源数量
        /// </summary>
        public int GetResourceCount(ThingDef thingDef)
        {
            if (thingDef == null) return 0;

            if (globalResources.TryGetValue(thingDef, out int count))
            {
                return count;
            }
            return 0;
        }

        /// <summary>
        /// 转移资源到存储设施
        /// </summary>
        public bool TransferToStorage(ThingDef thingDef, int amount, StorageBuilding storage)
        {
            if (!ConsumeResource(thingDef, amount)) return false;

            storage.AddItem(thingDef, amount);
            return true;
        }

        /// <summary>
        /// 从存储设施取出资源
        /// </summary>
        public bool TransferFromStorage(ThingDef thingDef, int amount, StorageBuilding storage)
        {
            if (!storage.RemoveItem(thingDef, amount)) return false;

            AddResource(thingDef, amount);
            return true;
        }

        #endregion

        #region 存储设施管理

        /// <summary>
        /// 注册存储设施
        /// </summary>
        public void RegisterStorage(StorageBuilding storage)
        {
            if (!storageBuildings.Contains(storage))
            {
                storageBuildings.Add(storage);
            }
        }

        /// <summary>
        /// 注销存储设施
        /// </summary>
        public void UnregisterStorage(StorageBuilding storage)
        {
            storageBuildings.Remove(storage);
        }

        /// <summary>
        /// 获取所有存储设施的总容量
        /// </summary>
        public int GetTotalStorageCapacity()
        {
            int total = 0;
            foreach (var storage in storageBuildings)
            {
                total += storage.maxCapacity;
            }
            return total;
        }

        /// <summary>
        /// 获取所有存储设施的已使用容量
        /// </summary>
        public int GetTotalStorageUsed()
        {
            int total = 0;
            foreach (var storage in storageBuildings)
            {
                total += storage.CurrentUsedCapacity;
            }
            return total;
        }

        #endregion

        #region 保存/加载

        public ResourceData Save()
        {
            List<ResourceItemData> resources = new List<ResourceItemData>();
            foreach (var pair in globalResources)
            {
                resources.Add(new ResourceItemData
                {
                    thingDefName = pair.Key.defName,
                    count = pair.Value
                });
            }

            return new ResourceData
            {
                resources = resources.ToArray()
            };
        }

        public void Load(ResourceData data)
        {
            globalResources.Clear();
            foreach (var item in data.resources)
            {
                ThingDef def = ThingDefDatabase.GetThingDef(item.thingDefName);
                if (def != null)
                {
                    globalResources[def] = item.count;
                }
            }
        }

        #endregion
    }

    #region 存储设施

    public class StorageBuilding : MonoBehaviour
    {
        public int maxCapacity = 100;
        public Dictionary<ThingDef, int> items = new Dictionary<ThingDef, int>();

        public int CurrentUsedCapacity
        {
            get
            {
                int total = 0;
                foreach (var pair in items)
                {
                    total += pair.Value;
                }
                return total;
            }
        }

        public bool IsFull => CurrentUsedCapacity >= maxCapacity;

        private void Awake()
        {
            ResourceManager.Instance.RegisterStorage(this);
        }

        private void OnDestroy()
        {
            ResourceManager.Instance.UnregisterStorage(this);
        }

        public bool AddItem(ThingDef thingDef, int count)
        {
            if (thingDef == null || count <= 0) return false;
            if (IsFull) return false;

            int availableSpace = maxCapacity - CurrentUsedCapacity;
            int actualCount = Mathf.Min(count, availableSpace);

            if (items.ContainsKey(thingDef))
            {
                items[thingDef] += actualCount;
            }
            else
            {
                items[thingDef] = actualCount;
            }

            return true;
        }

        public bool RemoveItem(ThingDef thingDef, int count)
        {
            if (thingDef == null || count <= 0) return false;

            if (!items.TryGetValue(thingDef, out int currentCount))
                return false;

            if (currentCount < count) return false;

            items[thingDef] -= count;
            if (items[thingDef] <= 0)
            {
                items.Remove(thingDef);
            }

            return true;
        }

        public int GetItemCount(ThingDef thingDef)
        {
            if (thingDef == null) return 0;
            items.TryGetValue(thingDef, out int count);
            return count;
        }
    }

    #endregion

    #region 数据类

    [Serializable]
    public class ResourceData
    {
        public ResourceItemData[] resources;
    }

    [Serializable]
    public class ResourceItemData
    {
        public string thingDefName;
        public int count;
    }

    #endregion
}