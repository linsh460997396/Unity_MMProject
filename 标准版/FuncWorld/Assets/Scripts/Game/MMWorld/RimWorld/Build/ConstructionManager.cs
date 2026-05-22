using UnityEngine;
using System.Collections.Generic;
using System;

namespace MMWorld.RimWorld
{
    /// <summary>
    /// 建造管理器
    /// 管理建造队列和建造过程
    /// </summary>
    public class ConstructionManager : MonoBehaviour
    {
        #region 单例

        public static ConstructionManager Instance { get; private set; }

        #endregion

        #region 建造队列

        /// <summary>
        /// 建造队列
        /// </summary>
        public List<ConstructionJob> constructionQueue = new List<ConstructionJob>();

        /// <summary>
        /// 当前正在建造的建筑
        /// </summary>
        public ConstructionJob currentJob;

        #endregion

        #region 设置

        /// <summary>
        /// 建造速度倍率
        /// </summary>
        public float constructionSpeedMultiplier = 1f;

        #endregion

        #region 事件

        public event Action<ConstructionJob> OnJobAdded;
        public event Action<ConstructionJob> OnJobCompleted;
        public event Action<ConstructionJob> OnJobCanceled;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            ProcessConstructionQueue();
        }

        #endregion

        #region 建造队列管理

        /// <summary>
        /// 添加建造任务
        /// </summary>
        public bool AddConstructionJob(BuildingDef buildingDef, Vector3 position, Pawn builder = null)
        {
            // 检查材料是否足够
            if (!HasEnoughMaterials(buildingDef))
            {
                Debug.LogWarning($"材料不足，无法建造 {buildingDef.label}");
                return false;
            }

            // 扣除材料
            ConsumeMaterials(buildingDef);

            // 创建建造任务
            ConstructionJob job = new ConstructionJob
            {
                buildingDef = buildingDef,
                position = position,
                builder = builder,
                progress = 0f,
                status = ConstructionStatus.Ready
            };

            constructionQueue.Add(job);
            OnJobAdded?.Invoke(job);

            Debug.Log($"添加建造任务: {buildingDef.label}");
            return true;
        }

        /// <summary>
        /// 取消建造任务
        /// </summary>
        public void CancelConstructionJob(ConstructionJob job)
        {
            if (constructionQueue.Remove(job))
            {
                // 返还部分材料
                RefundMaterials(job.buildingDef);
                job.status = ConstructionStatus.Canceled;
                OnJobCanceled?.Invoke(job);
                Debug.Log($"取消建造任务: {job.buildingDef.label}");
            }
        }

        /// <summary>
        /// 处理建造队列
        /// </summary>
        private void ProcessConstructionQueue()
        {
            // 如果当前没有正在建造的任务，从队列中获取一个
            if (currentJob == null && constructionQueue.Count > 0)
            {
                // 找到第一个有建造者或可以分配建造者的任务
                foreach (var job in constructionQueue)
                {
                    if (job.builder != null || AssignBuilder(job))
                    {
                        currentJob = job;
                        currentJob.status = ConstructionStatus.InProgress;
                        constructionQueue.Remove(job);
                        break;
                    }
                }
            }

            // 处理当前建造任务
            if (currentJob != null)
            {
                UpdateConstructionProgress();
            }
        }

        /// <summary>
        /// 更新建造进度
        /// </summary>
        private void UpdateConstructionProgress()
        {
            if (currentJob.builder == null) return;

            // 计算建造速度（基于建造者技能）
            float skillBonus = 1 + (currentJob.builder.skills.construction.level * 0.05f);
            float speed = (1f / currentJob.buildingDef.constructionTime) * skillBonus * constructionSpeedMultiplier;

            currentJob.progress += speed * Time.deltaTime;

            // 检查是否完成
            if (currentJob.progress >= 1f)
            {
                CompleteConstruction();
            }
        }

        /// <summary>
        /// 完成建造
        /// </summary>
        private void CompleteConstruction()
        {
            // 创建建筑对象
            CreateBuilding(currentJob.buildingDef, currentJob.position);

            // 通知事件
            currentJob.status = ConstructionStatus.Completed;
            OnJobCompleted?.Invoke(currentJob);

            Debug.Log($"建造完成: {currentJob.buildingDef.label}");

            // 清理当前任务
            currentJob = null;
        }

        /// <summary>
        /// 创建建筑对象
        /// </summary>
        private void CreateBuilding(BuildingDef def, Vector3 position)
        {
            GameObject buildingObj = new GameObject(def.label);
            buildingObj.transform.position = position;

            // 添加建筑组件
            Building building = buildingObj.AddComponent<Building>();
            building.buildingDef = def;
            building.hitPoints = def.maxHitPoints;

            // 如果有预制体，实例化它
            if (def.prefab != null)
            {
                GameObject prefabInstance = Instantiate(def.prefab, position, Quaternion.identity);
                prefabInstance.transform.SetParent(buildingObj.transform);
            }
            else
            {
                // 创建简单的立方体作为占位符
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(buildingObj.transform);
                cube.transform.localPosition = Vector3.zero;
                cube.transform.localScale = new Vector3(def.sizeX, 1f, def.sizeY);
            }
        }

        /// <summary>
        /// 分配建造者
        /// </summary>
        private bool AssignBuilder(ConstructionJob job)
        {
            // 找到有建造工作启用的殖民者
            Pawn[] pawns = FindObjectsOfType<Pawn>();
            foreach (var pawn in pawns)
            {
                if (pawn.work.constructionEnabled && pawn.currentJob == null)
                {
                    job.builder = pawn;
                    pawn.AssignJob(JobUtility.CreateConstructionJob(job.buildingDef, job.position));
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region 材料管理

        /// <summary>
        /// 检查材料是否足够
        /// </summary>
        public bool HasEnoughMaterials(BuildingDef buildingDef)
        {
            if (buildingDef.costList == null || buildingDef.costListAmount == null)
                return true;

            for (int i = 0; i < buildingDef.costList.Length; i++)
            {
                ThingDef material = buildingDef.costList[i];
                int amount = buildingDef.costListAmount[i];

                if (!ResourceManager.Instance.HasEnoughResources(material, amount))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 消耗材料
        /// </summary>
        public void ConsumeMaterials(BuildingDef buildingDef)
        {
            if (buildingDef.costList == null || buildingDef.costListAmount == null)
                return;

            for (int i = 0; i < buildingDef.costList.Length; i++)
            {
                ThingDef material = buildingDef.costList[i];
                int amount = buildingDef.costListAmount[i];

                ResourceManager.Instance.ConsumeResource(material, amount);
            }
        }

        /// <summary>
        /// 返还材料
        /// </summary>
        public void RefundMaterials(BuildingDef buildingDef)
        {
            if (buildingDef.costList == null || buildingDef.costListAmount == null)
                return;

            float refundRatio = buildingDef.deconstructReturnedResources;

            for (int i = 0; i < buildingDef.costList.Length; i++)
            {
                ThingDef material = buildingDef.costList[i];
                int amount = Mathf.RoundToInt(buildingDef.costListAmount[i] * refundRatio);

                ResourceManager.Instance.AddResource(material, amount);
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取建造队列数量
        /// </summary>
        public int GetQueueCount()
        {
            return constructionQueue.Count;
        }

        /// <summary>
        /// 获取是否有正在进行的建造
        /// </summary>
        public bool IsConstructing()
        {
            return currentJob != null;
        }

        #endregion
    }

    #region 建造任务

    [Serializable]
    public class ConstructionJob
    {
        public BuildingDef buildingDef;
        public Vector3 position;
        public Pawn builder;
        public float progress;
        public ConstructionStatus status;
    }

    public enum ConstructionStatus
    {
        Ready,          // 准备就绪
        InProgress,     // 进行中
        Completed,      // 已完成
        Canceled        // 已取消
    }

    #endregion

    #region 建筑组件

    public class Building : MonoBehaviour
    {
        public BuildingDef buildingDef;
        public int hitPoints;
        public bool isPowered;
        public bool isRoofed;

        private void Start()
        {
            hitPoints = buildingDef.maxHitPoints;
        }

        public void TakeDamage(int damage)
        {
            hitPoints -= damage;
            if (hitPoints <= 0)
            {
                DestroyBuilding();
            }
        }

        public void DestroyBuilding()
        {
            // 返还部分材料
            if (buildingDef.canBeDeconstructed)
            {
                ConstructionManager.Instance.RefundMaterials(buildingDef);
            }
            Destroy(gameObject);
        }
    }

    #endregion
}