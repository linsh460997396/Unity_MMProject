using UnityEngine;
using System;

namespace MMWorld.SimAI
{
    /// <summary>
    /// 任务类
    /// 类似环世界的任务系统
    /// </summary>
    [Serializable]
    public class Job
    {
        #region 基本信息

        /// <summary>
        /// 任务类型
        /// </summary>
        public JobType jobType;

        /// <summary>
        /// 任务优先级 (1-10)
        /// </summary>
        public int priority = 5;

        /// <summary>
        /// 任务目标位置
        /// </summary>
        public Vector3 targetPosition;

        /// <summary>
        /// 任务目标对象
        /// </summary>
        public UnityEngine.Object targetObject;

        /// <summary>
        /// 任务参数
        /// </summary>
        public JobParameters parameters;

        /// <summary>
        /// 任务状态
        /// </summary>
        public JobStatus status;

        /// <summary>
        /// 任务进度 (0-1)
        /// </summary>
        public float progress;

        /// <summary>
        /// 任务所有者
        /// </summary>
        public Pawn owner;

        #endregion

        #region 构造函数

        public Job(JobType jobType)
        {
            this.jobType = jobType;
            this.status = JobStatus.Ready;
            this.progress = 0;
            this.parameters = new JobParameters();
        }

        public Job(JobType jobType, Vector3 targetPosition) : this(jobType)
        {
            this.targetPosition = targetPosition;
        }

        public Job(JobType jobType, UnityEngine.Object targetObject) : this(jobType)
        {
            this.targetObject = targetObject;
        }

        #endregion

        #region 任务状态

        public void Start()
        {
            status = JobStatus.InProgress;
            progress = 0;
        }

        public void Update(float progressDelta)
        {
            if (status != JobStatus.InProgress) return;

            progress = Mathf.Min(1, progress + progressDelta);

            if (progress >= 1)
            {
                Complete();
            }
        }

        public void Complete()
        {
            status = JobStatus.Completed;
        }

        public void Cancel()
        {
            status = JobStatus.Canceled;
        }

        public void Fail()
        {
            status = JobStatus.Failed;
        }

        #endregion
    }

    #region 任务状态

    public enum JobStatus
    {
        Ready,          // 准备就绪
        InProgress,     // 进行中
        Completed,      // 已完成
        Canceled,       // 已取消
        Failed          // 失败
    }

    #endregion

    #region 任务参数

    [Serializable]
    public class JobParameters
    {
        /// <summary>
        /// 建造的建筑定义
        /// </summary>
        public BuildingDef buildingDef;

        /// <summary>
        /// 需要的材料列表
        /// </summary>
        public ThingDef[] requiredMaterials;

        /// <summary>
        /// 材料数量列表
        /// </summary>
        public int[] materialAmounts;

        /// <summary>
        /// 目标物品
        /// </summary>
        public ThingDef targetThing;

        /// <summary>
        /// 目标数量
        /// </summary>
        public int targetCount;

        /// <summary>
        /// 工作速度修正
        /// </summary>
        public float workSpeedMultiplier = 1f;

        /// <summary>
        /// 是否需要使用工具
        /// </summary>
        public bool requiresTool;

        /// <summary>
        /// 需要的工具类型
        /// </summary>
        public ThingDef requiredTool;
    }

    #endregion

    #region 任务工具类

    public static class JobUtility
    {
        /// <summary>
        /// 创建建造任务
        /// </summary>
        public static Job CreateConstructionJob(BuildingDef buildingDef, Vector3 position)
        {
            Job job = new Job(JobType.Construct, position);
            job.parameters.buildingDef = buildingDef;
            job.parameters.requiredMaterials = buildingDef.costList;
            job.parameters.materialAmounts = buildingDef.costListAmount;
            return job;
        }

        /// <summary>
        /// 创建搬运任务
        /// </summary>
        public static Job CreateHaulJob(ThingDef thingDef, Vector3 fromPosition, Vector3 toPosition, int count = 1)
        {
            Job job = new Job(JobType.Haul, toPosition);
            job.parameters.targetThing = thingDef;
            job.parameters.targetCount = count;
            return job;
        }

        /// <summary>
        /// 创建狩猎任务
        /// </summary>
        public static Job CreateHuntJob(UnityEngine.Object targetAnimal)
        {
            Job job = new Job(JobType.Hunt, targetAnimal);
            return job;
        }

        /// <summary>
        /// 创建收割任务
        /// </summary>
        public static Job CreateHarvestJob(UnityEngine.Object targetPlant)
        {
            Job job = new Job(JobType.Harvest, targetPlant);
            return job;
        }

        /// <summary>
        /// 创建烹饪任务
        /// </summary>
        public static Job CreateCookJob(ThingDef resultDef, int count = 1)
        {
            Job job = new Job(JobType.Cook);
            job.parameters.targetThing = resultDef;
            job.parameters.targetCount = count;
            return job;
        }
    }

    #endregion
}