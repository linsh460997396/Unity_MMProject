using UnityEngine;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 殖民者(Pawn)- 游戏中的核心角色
    /// 类似环世界的殖民者系统
    /// </summary>
    public class Pawn : MonoBehaviour
    {
        #region 基础信息

        /// <summary>
        /// 殖民者名称
        /// </summary>
        public string name;

        /// <summary>
        /// 性别
        /// </summary>
        public Gender gender;

        /// <summary>
        /// 年龄
        /// </summary>
        public int age;

        /// <summary>
        /// 健康状态
        /// </summary>
        public HealthState healthState;

        /// <summary>
        /// 当前生命值 (0-100)
        /// </summary>
        public float health = 100f;

        #endregion

        #region 组件引用

        /// <summary>
        /// 属性组件
        /// </summary>
        public PawnStats stats;

        /// <summary>
        /// 技能组件
        /// </summary>
        public PawnSkills skills;

        /// <summary>
        /// 需求组件
        /// </summary>
        public PawnNeeds needs;

        /// <summary>
        /// 工作分配组件
        /// </summary>
        public PawnWork work;

        /// <summary>
        /// 背包组件
        /// </summary>
        public PawnInventory inventory;

        #endregion

        #region 状态

        /// <summary>
        /// 当前状态
        /// </summary>
        public PawnStatus currentStatus;

        /// <summary>
        /// 当前任务
        /// </summary>
        public Job currentJob;

        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool isDead;

        /// <summary>
        /// 是否昏迷
        /// </summary>
        public bool isUnconscious;

        #endregion

        #region 事件

        public event Action<Pawn> OnHealthChanged;
        public event Action<Pawn> OnStatusChanged;
        public event Action<Pawn> OnDeath;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 初始化组件
            stats = GetComponent<PawnStats>();
            skills = GetComponent<PawnSkills>();
            needs = GetComponent<PawnNeeds>();
            work = GetComponent<PawnWork>();
            inventory = GetComponent<PawnInventory>();

            // 如果组件不存在,自动添加
            if (stats == null) stats = gameObject.AddComponent<PawnStats>();
            if (skills == null) skills = gameObject.AddComponent<PawnSkills>();
            if (needs == null) needs = gameObject.AddComponent<PawnNeeds>();
            if (work == null) work = gameObject.AddComponent<PawnWork>();
            if (inventory == null) inventory = gameObject.AddComponent<PawnInventory>();

            // 初始化默认状态
            currentStatus = PawnStatus.Idle;
        }

        private void Start()
        {
            // 绑定需求变化事件
            needs.OnNeedChanged += HandleNeedChanged;
        }

        private void Update()
        {
            if (isDead) return;

            // 更新需求
            needs.UpdateNeeds();

            // 检查死亡条件
            CheckDeathConditions();
        }

        #endregion

        #region 健康系统

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            health = Mathf.Max(0, health - damage);
            OnHealthChanged?.Invoke(this);

            if (health <= 0)
            {
                Die();
            }
            else if (health < 30)
            {
                healthState = HealthState.Critical;
            }
            else if (health < 60)
            {
                healthState = HealthState.Wounded;
            }
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(float amount)
        {
            health = Mathf.Min(100, health + amount);
            OnHealthChanged?.Invoke(this);

            if (health >= 60)
            {
                healthState = HealthState.Healthy;
            }
            else if (health >= 30)
            {
                healthState = HealthState.Wounded;
            }
        }

        /// <summary>
        /// 检查死亡条件
        /// </summary>
        private void CheckDeathConditions()
        {
            // 饥饿致死
            if (needs.hunger <= 0)
            {
                Die();
            }
            // 口渴致死
            if (needs.thirst <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 死亡
        /// </summary>
        public void Die()
        {
            isDead = true;
            currentStatus = PawnStatus.Dead;
            OnDeath?.Invoke(this);
            Debug.Log($"{name} 死亡了");
        }

        #endregion

        #region 需求处理

        private void HandleNeedChanged(NeedType needType, float value)
        {
            // 根据需求状态改变殖民者行为
            if (needType == NeedType.Hunger && value < 30)
            {
                // 饥饿时停止当前任务去吃东西
                if (currentJob != null && currentJob.jobType != JobType.Eat)
                {
                    CancelCurrentJob();
                    FindFood();
                }
            }
            else if (needType == NeedType.Rest && value < 20)
            {
                // 极度疲劳时强制休息
                if (currentJob != null && currentJob.jobType != JobType.Rest)
                {
                    CancelCurrentJob();
                    Rest();
                }
            }
        }

        #endregion

        #region 任务系统

        /// <summary>
        /// 分配任务
        /// </summary>
        public void AssignJob(Job job)
        {
            if (isDead || isUnconscious) return;

            currentJob = job;
            currentStatus = PawnStatus.Working;
            OnStatusChanged?.Invoke(this);

            Debug.Log($"{name} 开始任务: {job.jobType}");
        }

        /// <summary>
        /// 取消当前任务
        /// </summary>
        public void CancelCurrentJob()
        {
            currentJob = null;
            currentStatus = PawnStatus.Idle;
            OnStatusChanged?.Invoke(this);
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        public void CompleteJob()
        {
            if (currentJob != null)
            {
                Debug.Log($"{name} 完成任务: {currentJob.jobType}");
            }
            CancelCurrentJob();
        }

        /// <summary>
        /// 寻找食物
        /// </summary>
        private void FindFood()
        {
            Job foodJob = new Job(JobType.Eat);
            AssignJob(foodJob);
        }

        /// <summary>
        /// 休息
        /// </summary>
        private void Rest()
        {
            Job restJob = new Job(JobType.Rest);
            AssignJob(restJob);
        }

        #endregion

        #region 保存/加载

        public PawnData Save()
        {
            return new PawnData
            {
                name = this.name,
                gender = this.gender,
                age = this.age,
                health = this.health,
                healthState = this.healthState,
                stats = stats.Save(),
                skills = skills.Save(),
                needs = needs.Save(),
                work = work.Save(),
                inventory = inventory.Save()
            };
        }

        public void Load(PawnData data)
        {
            name = data.name;
            gender = data.gender;
            age = data.age;
            health = data.health;
            healthState = data.healthState;

            stats.Load(data.stats);
            skills.Load(data.skills);
            needs.Load(data.needs);
            work.Load(data.work);
            inventory.Load(data.inventory);
        }

        #endregion
    }

    #region 枚举类型

    public enum Gender
    {
        Male,
        Female
    }

    public enum HealthState
    {
        Healthy,
        Wounded,
        Critical,
        Dead
    }

    public enum PawnStatus
    {
        Idle,
        Working,
        Moving,
        Resting,
        Eating,
        Dead,
        Unconscious
    }

    public enum JobType
    {
        None,
        Construct,      // 建造
        Harvest,        // 收割
        Mine,           // 采矿
        Hunt,           // 狩猎
        Cook,           // 烹饪
        Eat,            // 进食
        Rest,           // 休息
        Sleep,          // 睡觉
        Haul,           // 搬运
        Craft,          // 制作
        Repair,         // 修理
        Research,       // 研究
        Train,          // 训练
        Fight           // 战斗
    }

    public enum NeedType
    {
        Hunger,     // 饥饿
        Thirst,     // 口渴
        Rest,       // 休息
        Happiness,  // 心情
        Hygiene,    // 卫生
        Comfort     // 舒适
    }

    #endregion

    #region 数据类

    [Serializable]
    public class PawnData
    {
        public string name;
        public Gender gender;
        public int age;
        public float health;
        public HealthState healthState;
        public PawnStatsData stats;
        public PawnSkillsData skills;
        public PawnNeedsData needs;
        public PawnWorkData work;
        public PawnInventoryData inventory;
    }

    #endregion
}