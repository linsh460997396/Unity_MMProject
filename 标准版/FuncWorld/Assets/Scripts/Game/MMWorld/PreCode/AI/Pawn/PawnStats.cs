using UnityEngine;
using System;

namespace MMWorld.SimAI
{
    /// <summary>
    /// 殖民者属性组件
    /// 类似环世界的属性系统
    /// </summary>
    public class PawnStats : MonoBehaviour
    {
        #region 属性定义

        /// <summary>
        /// 移动速度 (0-100)
        /// </summary>
        public int moveSpeed = 50;

        /// <summary>
        /// 承载能力 (0-100)
        /// </summary>
        public int carryCapacity = 50;

        /// <summary>
        /// 食物消耗速率 (0-100) - 越高消耗越快
        /// </summary>
        public int foodConsumption = 50;

        /// <summary>
        /// 休息效率 (0-100) - 越高恢复越快
        /// </summary>
        public int restEfficiency = 50;

        /// <summary>
        /// 医疗能力 (0-100) - 影响治疗效果
        /// </summary>
        public int medicalCapacity = 50;

        /// <summary>
        /// 社交能力 (0-100)
        /// </summary>
        public int socialSkill = 50;

        /// <summary>
        /// 心理韧性 (0-100) - 影响情绪波动
        /// </summary>
        public int mentalToughness = 50;

        /// <summary>
        /// 免疫力 (0-100) - 抵抗疾病
        /// </summary>
        public int immunity = 50;

        #endregion

        #region 事件

        public event Action<string, int> OnStatChanged;

        #endregion

        #region 属性操作

        /// <summary>
        /// 设置属性值
        /// </summary>
        public void SetStat(string statName, int value)
        {
            int clampedValue = Mathf.Clamp(value, 0, 100);

            switch (statName)
            {
                case nameof(moveSpeed):
                    moveSpeed = clampedValue;
                    break;
                case nameof(carryCapacity):
                    carryCapacity = clampedValue;
                    break;
                case nameof(foodConsumption):
                    foodConsumption = clampedValue;
                    break;
                case nameof(restEfficiency):
                    restEfficiency = clampedValue;
                    break;
                case nameof(medicalCapacity):
                    medicalCapacity = clampedValue;
                    break;
                case nameof(socialSkill):
                    socialSkill = clampedValue;
                    break;
                case nameof(mentalToughness):
                    mentalToughness = clampedValue;
                    break;
                case nameof(immunity):
                    immunity = clampedValue;
                    break;
            }

            OnStatChanged?.Invoke(statName, clampedValue);
        }

        /// <summary>
        /// 增加属性值
        /// </summary>
        public void AddStat(string statName, int amount)
        {
            switch (statName)
            {
                case nameof(moveSpeed):
                    moveSpeed = Mathf.Clamp(moveSpeed + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, moveSpeed);
                    break;
                case nameof(carryCapacity):
                    carryCapacity = Mathf.Clamp(carryCapacity + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, carryCapacity);
                    break;
                case nameof(foodConsumption):
                    foodConsumption = Mathf.Clamp(foodConsumption + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, foodConsumption);
                    break;
                case nameof(restEfficiency):
                    restEfficiency = Mathf.Clamp(restEfficiency + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, restEfficiency);
                    break;
                case nameof(medicalCapacity):
                    medicalCapacity = Mathf.Clamp(medicalCapacity + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, medicalCapacity);
                    break;
                case nameof(socialSkill):
                    socialSkill = Mathf.Clamp(socialSkill + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, socialSkill);
                    break;
                case nameof(mentalToughness):
                    mentalToughness = Mathf.Clamp(mentalToughness + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, mentalToughness);
                    break;
                case nameof(immunity):
                    immunity = Mathf.Clamp(immunity + amount, 0, 100);
                    OnStatChanged?.Invoke(statName, immunity);
                    break;
            }
        }

        #endregion

        #region 保存/加载

        public PawnStatsData Save()
        {
            return new PawnStatsData
            {
                moveSpeed = this.moveSpeed,
                carryCapacity = this.carryCapacity,
                foodConsumption = this.foodConsumption,
                restEfficiency = this.restEfficiency,
                medicalCapacity = this.medicalCapacity,
                socialSkill = this.socialSkill,
                mentalToughness = this.mentalToughness,
                immunity = this.immunity
            };
        }

        public void Load(PawnStatsData data)
        {
            moveSpeed = data.moveSpeed;
            carryCapacity = data.carryCapacity;
            foodConsumption = data.foodConsumption;
            restEfficiency = data.restEfficiency;
            medicalCapacity = data.medicalCapacity;
            socialSkill = data.socialSkill;
            mentalToughness = data.mentalToughness;
            immunity = data.immunity;
        }

        #endregion
    }

    [Serializable]
    public class PawnStatsData
    {
        public int moveSpeed;
        public int carryCapacity;
        public int foodConsumption;
        public int restEfficiency;
        public int medicalCapacity;
        public int socialSkill;
        public int mentalToughness;
        public int immunity;
    }
}