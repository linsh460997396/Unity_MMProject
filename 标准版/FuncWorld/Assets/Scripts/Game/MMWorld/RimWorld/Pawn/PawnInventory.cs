using UnityEngine;
using System;
using System.Collections.Generic;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 殖民者背包组件
    /// 类似环世界的背包系统
    /// </summary>
    public class PawnInventory : MonoBehaviour
    {
        #region 背包设置

        /// <summary>
        /// 最大物品数量
        /// </summary>
        public int maxItemCount = 15;

        /// <summary>
        /// 当前物品列表
        /// </summary>
        public List<InventoryItem> items = new List<InventoryItem>();

        #endregion

        #region 事件

        public event Action OnInventoryChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 当前物品数量
        /// </summary>
        public int CurrentItemCount => items.Count;

        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => items.Count >= maxItemCount;

        #endregion

        #region 物品操作

        /// <summary>
        /// 添加物品
        /// </summary>
        public bool AddItem(ThingDef thingDef, int count = 1)
        {
            if (IsFull) return false;
            if (count <= 0) return false;

            // 检查是否已有相同类型的物品
            InventoryItem existingItem = items.Find(item => item.thingDef == thingDef);

            if (existingItem != null)
            {
                // 叠加到现有物品
                existingItem.count += count;
            }
            else
            {
                // 创建新物品
                InventoryItem newItem = new InventoryItem
                {
                    thingDef = thingDef,
                    count = count
                };
                items.Add(newItem);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        public bool RemoveItem(ThingDef thingDef, int count = 1)
        {
            InventoryItem item = items.Find(i => i.thingDef == thingDef);
            if (item == null) return false;
            if (item.count < count) return false;

            item.count -= count;
            if (item.count <= 0)
            {
                items.Remove(item);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 获取物品数量
        /// </summary>
        public int GetItemCount(ThingDef thingDef)
        {
            InventoryItem item = items.Find(i => i.thingDef == thingDef);
            return item?.count ?? 0;
        }

        /// <summary>
        /// 检查是否有足够的物品
        /// </summary>
        public bool HasEnoughItems(ThingDef thingDef, int count)
        {
            return GetItemCount(thingDef) >= count;
        }

        /// <summary>
        /// 清空背包
        /// </summary>
        public void Clear()
        {
            items.Clear();
            OnInventoryChanged?.Invoke();
        }

        #endregion

        #region 保存/加载

        public PawnInventoryData Save()
        {
            List<InventoryItemData> itemDataList = new List<InventoryItemData>();
            foreach (var item in items)
            {
                itemDataList.Add(item.Save());
            }

            return new PawnInventoryData
            {
                maxItemCount = this.maxItemCount,
                items = itemDataList.ToArray()
            };
        }

        public void Load(PawnInventoryData data)
        {
            maxItemCount = data.maxItemCount;
            items.Clear();

            foreach (var itemData in data.items)
            {
                InventoryItem item = new InventoryItem();
                item.Load(itemData);
                items.Add(item);
            }
        }

        #endregion
    }

    #region 物品定义

    [Serializable]
    public class InventoryItem
    {
        public ThingDef thingDef;
        public int count;

        public InventoryItemData Save()
        {
            return new InventoryItemData
            {
                thingDefName = thingDef.defName,
                count = count
            };
        }

        public void Load(InventoryItemData data)
        {
            thingDef = ThingDefDatabase.GetThingDef(data.thingDefName);
            count = data.count;
        }
    }

    [Serializable]
    public class InventoryItemData
    {
        public string thingDefName;
        public int count;
    }

    #endregion

    [Serializable]
    public class PawnInventoryData
    {
        public int maxItemCount;
        public InventoryItemData[] items;
    }
}