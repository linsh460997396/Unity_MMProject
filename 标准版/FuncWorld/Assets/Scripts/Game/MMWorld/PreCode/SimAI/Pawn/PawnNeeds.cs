using UnityEngine;
using System;

namespace MMWorld.SimAI
{
    /// <summary>
    /// 殖民者需求组件
    /// 类似环世界的需求系统
    /// </summary>
    public class PawnNeeds : MonoBehaviour
    {
        #region 需求值

        /// <summary>
        /// 饥饿 (0-100)
        /// </summary>
        public float hunger = 100;

        /// <summary>
        /// 口渴 (0-100)
        /// </summary>
        public float thirst = 100;

        /// <summary>
        /// 休息 (0-100)
        /// </summary>
        public float rest = 100;

        /// <summary>
        /// 心情 (0-100)
        /// </summary>
        public float happiness = 80;

        /// <summary>
        /// 卫生 (0-100)
        /// </summary>
        public float hygiene = 100;

        /// <summary>
        /// 舒适 (0-100)
        /// </summary>
        public float comfort = 100;

        #endregion

        #region 消耗速率

        private float hungerRate = 0.02f;    // 每帧消耗
        private float thirstRate = 0.015f;
        private float restRate = 0.01f;
        private float hygieneRate = 0.005f;

        #endregion

        #region 事件

        public event Action<NeedType, float> OnNeedChanged;

        #endregion

        #region 更新

        public void UpdateNeeds()
        {
            // 消耗需求
            hunger = Mathf.Max(0, hunger - hungerRate);
            thirst = Mathf.Max(0, thirst - thirstRate);
            rest = Mathf.Max(0, rest - restRate);
            hygiene = Mathf.Max(0, hygiene - hygieneRate);

            // 触发事件
            OnNeedChanged?.Invoke(NeedType.Hunger, hunger);
            OnNeedChanged?.Invoke(NeedType.Thirst, thirst);
            OnNeedChanged?.Invoke(NeedType.Rest, rest);
            OnNeedChanged?.Invoke(NeedType.Hygiene, hygiene);
        }

        #endregion

        #region 需求操作

        /// <summary>
        /// 设置需求值
        /// </summary>
        public void SetNeed(NeedType type, float value)
        {
            float clampedValue = Mathf.Clamp(value, 0, 100);

            switch (type)
            {
                case NeedType.Hunger:
                    hunger = clampedValue;
                    break;
                case NeedType.Thirst:
                    thirst = clampedValue;
                    break;
                case NeedType.Rest:
                    rest = clampedValue;
                    break;
                case NeedType.Happiness:
                    happiness = clampedValue;
                    break;
                case NeedType.Hygiene:
                    hygiene = clampedValue;
                    break;
                case NeedType.Comfort:
                    comfort = clampedValue;
                    break;
            }

            OnNeedChanged?.Invoke(type, clampedValue);
        }

        /// <summary>
        /// 增加需求值
        /// </summary>
        public void AddNeed(NeedType type, float amount)
        {
            switch (type)
            {
                case NeedType.Hunger:
                    hunger = Mathf.Min(100, hunger + amount);
                    OnNeedChanged?.Invoke(type, hunger);
                    break;
                case NeedType.Thirst:
                    thirst = Mathf.Min(100, thirst + amount);
                    OnNeedChanged?.Invoke(type, thirst);
                    break;
                case NeedType.Rest:
                    rest = Mathf.Min(100, rest + amount);
                    OnNeedChanged?.Invoke(type, rest);
                    break;
                case NeedType.Happiness:
                    happiness = Mathf.Min(100, happiness + amount);
                    OnNeedChanged?.Invoke(type, happiness);
                    break;
                case NeedType.Hygiene:
                    hygiene = Mathf.Min(100, hygiene + amount);
                    OnNeedChanged?.Invoke(type, hygiene);
                    break;
                case NeedType.Comfort:
                    comfort = Mathf.Min(100, comfort + amount);
                    OnNeedChanged?.Invoke(type, comfort);
                    break;
            }
        }

        /// <summary>
        /// 获取需求值
        /// </summary>
        public float GetNeed(NeedType type)
        {
            switch (type)
            {
                case NeedType.Hunger: return hunger;
                case NeedType.Thirst: return thirst;
                case NeedType.Rest: return rest;
                case NeedType.Happiness: return happiness;
                case NeedType.Hygiene: return hygiene;
                case NeedType.Comfort: return comfort;
                default: return 0;
            }
        }

        /// <summary>
        /// 获取最低需求
        /// </summary>
        public NeedType GetLowestNeed()
        {
            NeedType lowest = NeedType.Hunger;
            float lowestValue = hunger;

            if (thirst < lowestValue) { lowest = NeedType.Thirst; lowestValue = thirst; }
            if (rest < lowestValue) { lowest = NeedType.Rest; lowestValue = rest; }
            if (happiness < lowestValue) { lowest = NeedType.Happiness; lowestValue = happiness; }
            if (hygiene < lowestValue) { lowest = NeedType.Hygiene; lowestValue = hygiene; }
            if (comfort < lowestValue) { lowest = NeedType.Comfort; lowestValue = comfort; }

            return lowest;
        }

        /// <summary>
        /// 获取需求等级描述
        /// </summary>
        public string GetNeedLevelDescription(NeedType type)
        {
            float value = GetNeed(type);
            if (value >= 80) return "极佳";
            if (value >= 60) return "良好";
            if (value >= 40) return "一般";
            if (value >= 20) return "较差";
            return "危急";
        }

        #endregion

        #region 保存/加载

        public PawnNeedsData Save()
        {
            return new PawnNeedsData
            {
                hunger = this.hunger,
                thirst = this.thirst,
                rest = this.rest,
                happiness = this.happiness,
                hygiene = this.hygiene,
                comfort = this.comfort
            };
        }

        public void Load(PawnNeedsData data)
        {
            hunger = data.hunger;
            thirst = data.thirst;
            rest = data.rest;
            happiness = data.happiness;
            hygiene = data.hygiene;
            comfort = data.comfort;
        }

        #endregion
    }

    [Serializable]
    public class PawnNeedsData
    {
        public float hunger;
        public float thirst;
        public float rest;
        public float happiness;
        public float hygiene;
        public float comfort;
    }
}