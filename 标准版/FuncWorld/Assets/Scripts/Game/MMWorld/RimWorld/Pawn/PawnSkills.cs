using UnityEngine;
using System;
using System.Collections.Generic;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 殖民者技能组件
    /// 类似环世界的技能系统
    /// </summary>
    public class PawnSkills : MonoBehaviour
    {
        #region 技能定义

        /// <summary>
        /// 烹饪 (0-20)
        /// </summary>
        public Skill cooking = new Skill(SkillType.Cooking);

        /// <summary>
        /// 种植 (0-20)
        /// </summary>
        public Skill growing = new Skill(SkillType.Growing);

        /// <summary>
        /// 采矿 (0-20)
        /// </summary>
        public Skill mining = new Skill(SkillType.Mining);

        /// <summary>
        /// 建造 (0-20)
        /// </summary>
        public Skill construction = new Skill(SkillType.Construction);

        /// <summary>
        /// 手工 (0-20)
        /// </summary>
        public Skill crafting = new Skill(SkillType.Crafting);

        /// <summary>
        /// 狩猎 (0-20)
        /// </summary>
        public Skill hunting = new Skill(SkillType.Hunting);

        /// <summary>
        /// 医疗 (0-20)
        /// </summary>
        public Skill medicine = new Skill(SkillType.Medicine);

        /// <summary>
        /// 社交 (0-20)
        /// </summary>
        public Skill social = new Skill(SkillType.Social);

        /// <summary>
        /// 研究 (0-20)
        /// </summary>
        public Skill research = new Skill(SkillType.Research);

        /// <summary>
        /// 射击 (0-20)
        /// </summary>
        public Skill shooting = new Skill(SkillType.Shooting);

        /// <summary>
        /// 近战 (0-20)
        /// </summary>
        public Skill melee = new Skill(SkillType.Melee);

        /// <summary>
        /// 驯兽 (0-20)
        /// </summary>
        public Skill taming = new Skill(SkillType.Taming);

        #endregion

        #region 事件

        public event Action<SkillType, int> OnSkillChanged;

        #endregion

        #region 技能操作

        /// <summary>
        /// 获取技能
        /// </summary>
        public Skill GetSkill(SkillType type)
        {
            switch (type)
            {
                case SkillType.Cooking: return cooking;
                case SkillType.Growing: return growing;
                case SkillType.Mining: return mining;
                case SkillType.Construction: return construction;
                case SkillType.Crafting: return crafting;
                case SkillType.Hunting: return hunting;
                case SkillType.Medicine: return medicine;
                case SkillType.Social: return social;
                case SkillType.Research: return research;
                case SkillType.Shooting: return shooting;
                case SkillType.Melee: return melee;
                case SkillType.Taming: return taming;
                default: return null;
            }
        }

        /// <summary>
        /// 增加技能经验
        /// </summary>
        public void AddSkillExperience(SkillType type, float experience)
        {
            Skill skill = GetSkill(type);
            if (skill != null)
            {
                skill.AddExperience(experience);
                OnSkillChanged?.Invoke(type, skill.level);
            }
        }

        /// <summary>
        /// 获取所有技能列表
        /// </summary>
        public List<Skill> GetAllSkills()
        {
            return new List<Skill>
            {
                cooking, growing, mining, construction, crafting, hunting,
                medicine, social, research, shooting, melee, taming
            };
        }

        #endregion

        #region 保存/加载

        public PawnSkillsData Save()
        {
            return new PawnSkillsData
            {
                cooking = cooking.Save(),
                growing = growing.Save(),
                mining = mining.Save(),
                construction = construction.Save(),
                crafting = crafting.Save(),
                hunting = hunting.Save(),
                medicine = medicine.Save(),
                social = social.Save(),
                research = research.Save(),
                shooting = shooting.Save(),
                melee = melee.Save(),
                taming = taming.Save()
            };
        }

        public void Load(PawnSkillsData data)
        {
            cooking.Load(data.cooking);
            growing.Load(data.growing);
            mining.Load(data.mining);
            construction.Load(data.construction);
            crafting.Load(data.crafting);
            hunting.Load(data.hunting);
            medicine.Load(data.medicine);
            social.Load(data.social);
            research.Load(data.research);
            shooting.Load(data.shooting);
            melee.Load(data.melee);
            taming.Load(data.taming);
        }

        #endregion
    }

    #region 技能类型

    public enum SkillType
    {
        Cooking,        // 烹饪
        Growing,        // 种植
        Mining,         // 采矿
        Construction,   // 建造
        Crafting,       // 手工
        Hunting,        // 狩猎
        Medicine,       // 医疗
        Social,         // 社交
        Research,       // 研究
        Shooting,       // 射击
        Melee,          // 近战
        Taming          // 驯兽
    }

    #endregion

    #region 技能类

    [Serializable]
    public class Skill
    {
        public SkillType type;
        public int level;           // 等级 (0-20)
        public float experience;    // 当前经验
        public float experienceToNextLevel; // 升级所需经验

        public Skill(SkillType type)
        {
            this.type = type;
            this.level = 0;
            this.experience = 0;
            this.experienceToNextLevel = 100;
        }

        public void AddExperience(float amount)
        {
            experience += amount;
            while (experience >= experienceToNextLevel && level < 20)
            {
                experience -= experienceToNextLevel;
                level++;
                experienceToNextLevel = 100 + level * 50; // 升级所需经验递增
            }
        }

        public SkillData Save()
        {
            return new SkillData
            {
                type = this.type,
                level = this.level,
                experience = this.experience,
                experienceToNextLevel = this.experienceToNextLevel
            };
        }

        public void Load(SkillData data)
        {
            type = data.type;
            level = data.level;
            experience = data.experience;
            experienceToNextLevel = data.experienceToNextLevel;
        }
    }

    [Serializable]
    public class SkillData
    {
        public SkillType type;
        public int level;
        public float experience;
        public float experienceToNextLevel;
    }

    #endregion

    [Serializable]
    public class PawnSkillsData
    {
        public SkillData cooking;
        public SkillData growing;
        public SkillData mining;
        public SkillData construction;
        public SkillData crafting;
        public SkillData hunting;
        public SkillData medicine;
        public SkillData social;
        public SkillData research;
        public SkillData shooting;
        public SkillData melee;
        public SkillData taming;
    }
}