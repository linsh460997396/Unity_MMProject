using UnityEngine;
using System;
using System.Collections.Generic;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 殖民者工作分配组件
    /// 类似环世界的工作系统
    /// </summary>
    public class PawnWork : MonoBehaviour
    {
        #region 工作类型定义

        /// <summary>
        /// 是否启用烹饪工作
        /// </summary>
        public bool cookingEnabled = true;

        /// <summary>
        /// 是否启用作坊工作
        /// </summary>
        public bool craftingEnabled = true;

        /// <summary>
        /// 是否启用建造工作
        /// </summary>
        public bool constructionEnabled = true;

        /// <summary>
        /// 是否启用种植工作
        /// </summary>
        public bool growingEnabled = true;

        /// <summary>
        /// 是否启用采矿工作
        /// </summary>
        public bool miningEnabled = true;

        /// <summary>
        /// 是否启用狩猎工作
        /// </summary>
        public bool huntingEnabled = true;

        /// <summary>
        /// 是否启用医疗工作
        /// </summary>
        public bool medicineEnabled = true;

        /// <summary>
        /// 是否启用社交工作
        /// </summary>
        public bool socialEnabled = true;

        /// <summary>
        /// 是否启用研究工作
        /// </summary>
        public bool researchEnabled = true;

        /// <summary>
        /// 是否启用搬运工作
        /// </summary>
        public bool haulingEnabled = true;

        /// <summary>
        /// 是否启用清洁工作
        /// </summary>
        public bool cleaningEnabled = true;

        /// <summary>
        /// 是否启用修理工作
        /// </summary>
        public bool repairingEnabled = true;

        #endregion

        #region 工作优先级

        /// <summary>
        /// 工作优先级列表
        /// </summary>
        public List<WorkType> workPriorities = new List<WorkType>();

        #endregion

        #region 事件

        public event Action<WorkType, bool> OnWorkEnabledChanged;

        #endregion

        #region 工作操作

        /// <summary>
        /// 设置工作启用状态
        /// </summary>
        public void SetWorkEnabled(WorkType workType, bool enabled)
        {
            switch (workType)
            {
                case WorkType.Cooking: cookingEnabled = enabled; break;
                case WorkType.Crafting: craftingEnabled = enabled; break;
                case WorkType.Construction: constructionEnabled = enabled; break;
                case WorkType.Growing: growingEnabled = enabled; break;
                case WorkType.Mining: miningEnabled = enabled; break;
                case WorkType.Hunting: huntingEnabled = enabled; break;
                case WorkType.Medicine: medicineEnabled = enabled; break;
                case WorkType.Social: socialEnabled = enabled; break;
                case WorkType.Research: researchEnabled = enabled; break;
                case WorkType.Hauling: haulingEnabled = enabled; break;
                case WorkType.Cleaning: cleaningEnabled = enabled; break;
                case WorkType.Repairing: repairingEnabled = enabled; break;
            }

            OnWorkEnabledChanged?.Invoke(workType, enabled);
        }

        /// <summary>
        /// 检查工作是否启用
        /// </summary>
        public bool IsWorkEnabled(WorkType workType)
        {
            switch (workType)
            {
                case WorkType.Cooking: return cookingEnabled;
                case WorkType.Crafting: return craftingEnabled;
                case WorkType.Construction: return constructionEnabled;
                case WorkType.Growing: return growingEnabled;
                case WorkType.Mining: return miningEnabled;
                case WorkType.Hunting: return huntingEnabled;
                case WorkType.Medicine: return medicineEnabled;
                case WorkType.Social: return socialEnabled;
                case WorkType.Research: return researchEnabled;
                case WorkType.Hauling: return haulingEnabled;
                case WorkType.Cleaning: return cleaningEnabled;
                case WorkType.Repairing: return repairingEnabled;
                default: return false;
            }
        }

        /// <summary>
        /// 设置工作优先级
        /// </summary>
        public void SetWorkPriority(WorkType workType, int priority)
        {
            // 确保优先级在有效范围内
            priority = Mathf.Clamp(priority, 1, 10);

            // 移除旧的优先级
            workPriorities.Remove(workType);

            // 在正确位置插入
            if (workPriorities.Count == 0)
            {
                workPriorities.Add(workType);
            }
            else
            {
                // 简单实现:按优先级顺序排列
                workPriorities.Add(workType);
            }
        }

        /// <summary>
        /// 获取工作优先级
        /// </summary>
        public int GetWorkPriority(WorkType workType)
        {
            int index = workPriorities.IndexOf(workType);
            return index >= 0 ? index + 1 : 5; // 默认优先级为5
        }

        /// <summary>
        /// 获取所有启用的工作类型
        /// </summary>
        public List<WorkType> GetEnabledWorks()
        {
            List<WorkType> enabled = new List<WorkType>();

            if (cookingEnabled) enabled.Add(WorkType.Cooking);
            if (craftingEnabled) enabled.Add(WorkType.Crafting);
            if (constructionEnabled) enabled.Add(WorkType.Construction);
            if (growingEnabled) enabled.Add(WorkType.Growing);
            if (miningEnabled) enabled.Add(WorkType.Mining);
            if (huntingEnabled) enabled.Add(WorkType.Hunting);
            if (medicineEnabled) enabled.Add(WorkType.Medicine);
            if (socialEnabled) enabled.Add(WorkType.Social);
            if (researchEnabled) enabled.Add(WorkType.Research);
            if (haulingEnabled) enabled.Add(WorkType.Hauling);
            if (cleaningEnabled) enabled.Add(WorkType.Cleaning);
            if (repairingEnabled) enabled.Add(WorkType.Repairing);

            return enabled;
        }

        #endregion

        #region 保存/加载

        public PawnWorkData Save()
        {
            return new PawnWorkData
            {
                cookingEnabled = this.cookingEnabled,
                craftingEnabled = this.craftingEnabled,
                constructionEnabled = this.constructionEnabled,
                growingEnabled = this.growingEnabled,
                miningEnabled = this.miningEnabled,
                huntingEnabled = this.huntingEnabled,
                medicineEnabled = this.medicineEnabled,
                socialEnabled = this.socialEnabled,
                researchEnabled = this.researchEnabled,
                haulingEnabled = this.haulingEnabled,
                cleaningEnabled = this.cleaningEnabled,
                repairingEnabled = this.repairingEnabled,
                workPriorities = this.workPriorities.ToArray()
            };
        }

        public void Load(PawnWorkData data)
        {
            cookingEnabled = data.cookingEnabled;
            craftingEnabled = data.craftingEnabled;
            constructionEnabled = data.constructionEnabled;
            growingEnabled = data.growingEnabled;
            miningEnabled = data.miningEnabled;
            huntingEnabled = data.huntingEnabled;
            medicineEnabled = data.medicineEnabled;
            socialEnabled = data.socialEnabled;
            researchEnabled = data.researchEnabled;
            haulingEnabled = data.haulingEnabled;
            cleaningEnabled = data.cleaningEnabled;
            repairingEnabled = data.repairingEnabled;
            workPriorities = new List<WorkType>(data.workPriorities);
        }

        #endregion
    }

    #region 工作类型

    public enum WorkType
    {
        Cooking,        // 烹饪
        Crafting,       // 作坊
        Construction,   // 建造
        Growing,        // 种植
        Mining,         // 采矿
        Hunting,        // 狩猎
        Medicine,       // 医疗
        Social,         // 社交
        Research,       // 研究
        Hauling,        // 搬运
        Cleaning,       // 清洁
        Repairing       // 修理
    }

    #endregion

    [Serializable]
    public class PawnWorkData
    {
        public bool cookingEnabled;
        public bool craftingEnabled;
        public bool constructionEnabled;
        public bool growingEnabled;
        public bool miningEnabled;
        public bool huntingEnabled;
        public bool medicineEnabled;
        public bool socialEnabled;
        public bool researchEnabled;
        public bool haulingEnabled;
        public bool cleaningEnabled;
        public bool repairingEnabled;
        public WorkType[] workPriorities;
    }
}