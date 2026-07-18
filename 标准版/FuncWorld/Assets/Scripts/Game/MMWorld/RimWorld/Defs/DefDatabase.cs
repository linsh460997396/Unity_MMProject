using UnityEngine;
using System.Collections.Generic;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 定义数据库
    /// 用于管理所有游戏定义
    /// </summary>
    public static class ThingDefDatabase
    {
        private static Dictionary<string, ThingDef> thingDefs = new Dictionary<string, ThingDef>();

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public static void Initialize()
        {
            // 从Resources加载所有ThingDef
            ThingDef[] defs = Resources.LoadAll<ThingDef>("Defs/ThingDefs");
            foreach (var def in defs)
            {
                if (!string.IsNullOrEmpty(def.defName))
                {
                    thingDefs[def.defName] = def;
                }
            }

            // 如果没有资源,创建默认定义
            if (thingDefs.Count == 0)
            {
                CreateDefaultDefs();
            }
        }

        /// <summary>
        /// 获取物品定义
        /// </summary>
        public static ThingDef GetThingDef(string defName)
        {
            if (thingDefs.TryGetValue(defName, out ThingDef def))
            {
                return def;
            }
            return null;
        }

        /// <summary>
        /// 注册物品定义
        /// </summary>
        public static void RegisterThingDef(ThingDef def)
        {
            if (!string.IsNullOrEmpty(def.defName))
            {
                thingDefs[def.defName] = def;
            }
        }

        /// <summary>
        /// 获取所有物品定义
        /// </summary>
        public static List<ThingDef> GetAllThingDefs()
        {
            return new List<ThingDef>(thingDefs.Values);
        }

        /// <summary>
        /// 获取指定分类的物品定义
        /// </summary>
        public static List<ThingDef> GetThingDefsByCategory(ThingCategory category)
        {
            List<ThingDef> result = new List<ThingDef>();
            foreach (var def in thingDefs.Values)
            {
                if (def.category == category)
                {
                    result.Add(def);
                }
            }
            return result;
        }

        /// <summary>
        /// 创建默认物品定义
        /// </summary>
        private static void CreateDefaultDefs()
        {
            // 基础材料
            CreateThingDef("Wood", "木材", "基础建筑材料", ThingCategory.Resource, MaterialCategory.Wood);
            CreateThingDef("Stone", "石头", "基础建筑材料", ThingCategory.Resource, MaterialCategory.Stone);
            CreateThingDef("Steel", "钢材", "高强度建筑材料", ThingCategory.Resource, MaterialCategory.Metal);
            CreateThingDef("Cloth", "布料", "制作服装的材料", ThingCategory.Resource, MaterialCategory.Fabric);
            CreateThingDef("Leather", "皮革", "制作服装和装备的材料", ThingCategory.Resource, MaterialCategory.Leather);

            // 食物
            CreateThingDef("RawMeat", "生肉", "未加工的肉类", ThingCategory.Food, MaterialCategory.Organic)
                .edible = true;
            CreateThingDef("Vegetable", "蔬菜", "新鲜蔬菜", ThingCategory.Food, MaterialCategory.Organic)
                .edible = true;
            CreateThingDef("MealSimple", "简单餐食", "基础食物", ThingCategory.Food, MaterialCategory.Organic)
                .edible = true;

            // 药品
            CreateThingDef("MedicineHerbal", "草药", "基础医疗用品", ThingCategory.Medicine, MaterialCategory.Organic);
            CreateThingDef("MedicineIndustrial", "工业药品", "高效医疗用品", ThingCategory.Medicine, MaterialCategory.Organic);
        }

        private static ThingDef CreateThingDef(string defName, string label, string description, 
            ThingCategory category, MaterialCategory materialCategory)
        {
            ThingDef def = ScriptableObject.CreateInstance<ThingDef>();
            def.defName = defName;
            def.label = label;
            def.description = description;
            def.category = category;
            def.materialCategory = materialCategory;
            def.maxStackSize = 75;
            def.weight = 1f;
            def.marketValue = 1;

            thingDefs[defName] = def;
            return def;
        }
    }

    /// <summary>
    /// 建筑定义数据库
    /// </summary>
    public static class BuildingDefDatabase
    {
        private static Dictionary<string, BuildingDef> buildingDefs = new Dictionary<string, BuildingDef>();

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public static void Initialize()
        {
            // 从Resources加载所有BuildingDef
            BuildingDef[] defs = Resources.LoadAll<BuildingDef>("Defs/BuildingDefs");
            foreach (var def in defs)
            {
                if (!string.IsNullOrEmpty(def.defName))
                {
                    buildingDefs[def.defName] = def;
                }
            }

            // 如果没有资源,创建默认定义
            if (buildingDefs.Count == 0)
            {
                CreateDefaultDefs();
            }
        }

        /// <summary>
        /// 获取建筑定义
        /// </summary>
        public static BuildingDef GetBuildingDef(string defName)
        {
            if (buildingDefs.TryGetValue(defName, out BuildingDef def))
            {
                return def;
            }
            return null;
        }

        /// <summary>
        /// 注册建筑定义
        /// </summary>
        public static void RegisterBuildingDef(BuildingDef def)
        {
            if (!string.IsNullOrEmpty(def.defName))
            {
                buildingDefs[def.defName] = def;
            }
        }

        /// <summary>
        /// 获取所有建筑定义
        /// </summary>
        public static List<BuildingDef> GetAllBuildingDefs()
        {
            return new List<BuildingDef>(buildingDefs.Values);
        }

        /// <summary>
        /// 获取指定分类的建筑定义
        /// </summary>
        public static List<BuildingDef> GetBuildingDefsByCategory(BuildingCategory category)
        {
            List<BuildingDef> result = new List<BuildingDef>();
            foreach (var def in buildingDefs.Values)
            {
                if (def.buildingCategory == category)
                {
                    result.Add(def);
                }
            }
            return result;
        }

        /// <summary>
        /// 创建默认建筑定义
        /// </summary>
        private static void CreateDefaultDefs()
        {
            // 墙壁
            BuildingDef wall = CreateBuildingDef("Wall", "墙壁", "基础墙体", BuildingCategory.Misc);
            wall.sizeX = 1;
            wall.sizeY = 1;
            wall.constructionTime = 30f;
            wall.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Wood") };
            wall.costListAmount = new int[] { 2 };

            // 门
            BuildingDef door = CreateBuildingDef("Door", "门", "可通行的门", BuildingCategory.Misc);
            door.sizeX = 1;
            door.sizeY = 1;
            door.constructionTime = 45f;
            door.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Wood") };
            door.costListAmount = new int[] { 3 };

            // 床铺
            BuildingDef bed = CreateBuildingDef("Bed", "床铺", "用于休息的床", BuildingCategory.Furniture);
            bed.sizeX = 2;
            bed.sizeY = 1;
            bed.constructionTime = 60f;
            bed.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Wood") };
            bed.costListAmount = new int[] { 4 };
            bed.beauty = 5;

            // 桌子
            BuildingDef table = CreateBuildingDef("Table", "桌子", "用于工作和用餐", BuildingCategory.Furniture);
            table.sizeX = 2;
            table.sizeY = 1;
            table.constructionTime = 40f;
            table.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Wood") };
            table.costListAmount = new int[] { 3 };

            // 椅子
            BuildingDef chair = CreateBuildingDef("Chair", "椅子", "用于坐下休息", BuildingCategory.Furniture);
            chair.sizeX = 1;
            chair.sizeY = 1;
            chair.constructionTime = 20f;
            chair.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Wood") };
            chair.costListAmount = new int[] { 1 };

            // 研究台
            BuildingDef researchBench = CreateBuildingDef("ResearchBench", "研究台", "用于进行研究", BuildingCategory.Research);
            researchBench.sizeX = 2;
            researchBench.sizeY = 1;
            researchBench.constructionTime = 120f;
            researchBench.requiredSkillLevel = 5;
            researchBench.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Steel"), ThingDefDatabase.GetThingDef("Wood") };
            researchBench.costListAmount = new int[] { 5, 3 };

            // 炉灶
            BuildingDef stove = CreateBuildingDef("Stove", "炉灶", "用于烹饪食物", BuildingCategory.Cooking);
            stove.sizeX = 1;
            stove.sizeY = 1;
            stove.constructionTime = 80f;
            stove.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Steel") };
            stove.costListAmount = new int[] { 4 };
            stove.requiresPower = true;
            stove.powerConsumption = 100;

            // 仓库
            BuildingDef storage = CreateBuildingDef("Storage", "仓库", "用于存储物品", BuildingCategory.Storage);
            storage.sizeX = 3;
            storage.sizeY = 3;
            storage.constructionTime = 100f;
            storage.costList = new ThingDef[] { ThingDefDatabase.GetThingDef("Wood") };
            storage.costListAmount = new int[] { 10 };
        }

        private static BuildingDef CreateBuildingDef(string defName, string label, string description, BuildingCategory category)
        {
            BuildingDef def = ScriptableObject.CreateInstance<BuildingDef>();
            def.defName = defName;
            def.label = label;
            def.description = description;
            def.buildingCategory = category;
            def.canBeDeconstructed = true;
            def.deconstructReturnedResources = 0.5f;
            def.maxHitPoints = 100;

            buildingDefs[defName] = def;
            return def;
        }
    }
}