using UnityEngine;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 物品定义
    /// 类似环世界的ThingDef
    /// </summary>
    [CreateAssetMenu(fileName = "NewThingDef", menuName = "RimWorld/ThingDef")]
    public class ThingDef : ScriptableObject
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

        #region 分类

        /// <summary>
        /// 物品类型
        /// </summary>
        public ThingCategory category;

        /// <summary>
        /// 材料类型
        /// </summary>
        public MaterialCategory materialCategory;

        #endregion

        #region 属性

        /// <summary>
        /// 体积（格子数）
        /// </summary>
        public float volume = 1f;

        /// <summary>
        /// 重量
        /// </summary>
        public float weight = 1f;

        /// <summary>
        /// 价值
        /// </summary>
        public int marketValue = 1;

        /// <summary>
        /// 堆叠上限
        /// </summary>
        public int maxStackSize = 75;

        /// <summary>
        /// 是否可食用
        /// </summary>
        public bool edible;

        /// <summary>
        /// 营养值
        /// </summary>
        public float nutrition = 0;

        /// <summary>
        /// 是否易燃
        /// </summary>
        public bool flammable = true;

        /// <summary>
        /// 是否可交易
        /// </summary>
        public bool traderPriceable = true;

        #endregion

        #region 纹理

        /// <summary>
        /// 地面纹理
        /// </summary>
        public Texture2D groundTex;

        /// <summary>
        /// 物品纹理
        /// </summary>
        public Texture2D thingTex;

        #endregion

        #region 保存/加载

        public ThingDefData Save()
        {
            return new ThingDefData
            {
                defName = this.defName,
                label = this.label,
                description = this.description,
                category = this.category,
                materialCategory = this.materialCategory,
                volume = this.volume,
                weight = this.weight,
                marketValue = this.marketValue,
                maxStackSize = this.maxStackSize,
                edible = this.edible,
                nutrition = this.nutrition,
                flammable = this.flammable,
                traderPriceable = this.traderPriceable
            };
        }

        #endregion
    }

    #region 枚举类型

    public enum ThingCategory
    {
        None,
        Resource,       // 资源
        Food,           // 食物
        Weapon,         // 武器
        Apparel,        // 服装
        Medicine,       // 药品
        Furniture,      // 家具
        Building,       // 建筑
        Tool,           // 工具
        Misc            // 杂物
    }

    public enum MaterialCategory
    {
        None,
        Wood,           // 木材
        Stone,          // 石材
        Metal,          // 金属
        Fabric,         // 布料
        Leather,        // 皮革
        Plastic,        // 塑料
        Organic,        // 有机物
        Crystal         // 晶体
    }

    #endregion

    [Serializable]
    public class ThingDefData
    {
        public string defName;
        public string label;
        public string description;
        public ThingCategory category;
        public MaterialCategory materialCategory;
        public float volume;
        public float weight;
        public int marketValue;
        public int maxStackSize;
        public bool edible;
        public float nutrition;
        public bool flammable;
        public bool traderPriceable;
    }
}