using UnityEngine;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 建筑定义
    /// 类似环世界的BuildableDef
    /// </summary>
    [CreateAssetMenu(fileName = "NewBuildingDef", menuName = "RimWorld/BuildingDef")]
    public class BuildingDef : ScriptableObject
    {
        #region 基本信息

        /// <summary>
        /// 定义名称（唯一标识符）
        /// </summary>
        public string defName;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string label;

        /// <summary>
        /// 描述
        /// </summary>
        [TextArea]
        public string description;

        /// <summary>
        /// 图标
        /// </summary>
        public Sprite icon;

        #endregion

        #region 建筑属性

        /// <summary>
        /// 建筑类型
        /// </summary>
        public BuildingCategory buildingCategory;

        /// <summary>
        /// 建筑大小（格子数）
        /// </summary>
        public int sizeX = 1;
        public int sizeY = 1;

        /// <summary>
        /// 建造时间（秒）
        /// </summary>
        public float constructionTime = 60f;

        /// <summary>
        /// 所需建造技能等级
        /// </summary>
        public int requiredSkillLevel = 0;

        /// <summary>
        /// 材料成本列表
        /// </summary>
        public ThingDef[] costList;

        /// <summary>
        /// 材料数量列表
        /// </summary>
        public int[] costListAmount;

        /// <summary>
        /// 是否需要屋顶
        /// </summary>
        public bool requiresRoof = false;

        /// <summary>
        /// 是否需要电力
        /// </summary>
        public bool requiresPower = false;

        /// <summary>
        /// 电力消耗（W）
        /// </summary>
        public float powerConsumption = 0;

        /// <summary>
        /// 是否可拆除
        /// </summary>
        public bool canBeDeconstructed = true;

        /// <summary>
        /// 拆除后返还材料比例
        /// </summary>
        [Range(0f, 1f)]
        public float deconstructReturnedResources = 0.5f;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int maxHitPoints = 100;

        /// <summary>
        /// 美观度
        /// </summary>
        public int beauty = 0;

        /// <summary>
        /// 污染值
        /// </summary>
        public int pollution = 0;

        #endregion

        #region 纹理与模型

        /// <summary>
        /// 预制体
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// 地面纹理
        /// </summary>
        public Texture2D groundTex;

        #endregion

        #region 保存/加载

        public BuildingDefData Save()
        {
            return new BuildingDefData
            {
                defName = this.defName,
                label = this.label,
                description = this.description,
                buildingCategory = this.buildingCategory,
                sizeX = this.sizeX,
                sizeY = this.sizeY,
                constructionTime = this.constructionTime,
                requiredSkillLevel = this.requiredSkillLevel,
                costListNames = GetCostListNames(),
                costListAmount = this.costListAmount,
                requiresRoof = this.requiresRoof,
                requiresPower = this.requiresPower,
                powerConsumption = this.powerConsumption,
                canBeDeconstructed = this.canBeDeconstructed,
                deconstructReturnedResources = this.deconstructReturnedResources,
                maxHitPoints = this.maxHitPoints,
                beauty = this.beauty,
                pollution = this.pollution
            };
        }

        private string[] GetCostListNames()
        {
            if (costList == null) return new string[0];
            string[] names = new string[costList.Length];
            for (int i = 0; i < costList.Length; i++)
            {
                names[i] = costList[i]?.defName ?? "";
            }
            return names;
        }

        #endregion
    }

    #region 枚举类型

    public enum BuildingCategory
    {
        None,
        Production,     // 生产设施
        Storage,        // 存储设施
        Furniture,      // 家具
        Medical,        // 医疗设施
        Research,       // 研究设施
        Power,          // 电力设施
        Defense,        // 防御设施
        Farming,        // 农业设施
        Cooking,        // 烹饪设施
        Crafting,       // 制作设施
        Misc            // 其他
    }

    #endregion

    [Serializable]
    public class BuildingDefData
    {
        public string defName;
        public string label;
        public string description;
        public BuildingCategory buildingCategory;
        public int sizeX;
        public int sizeY;
        public float constructionTime;
        public int requiredSkillLevel;
        public string[] costListNames;
        public int[] costListAmount;
        public bool requiresRoof;
        public bool requiresPower;
        public float powerConsumption;
        public bool canBeDeconstructed;
        public float deconstructReturnedResources;
        public int maxHitPoints;
        public int beauty;
        public int pollution;
    }
}