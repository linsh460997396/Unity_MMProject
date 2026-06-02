using System.Collections.Generic;
using UnityEngine;

namespace CellSpace
{
    /// <summary>
    /// 团块空间专用(GameObject)对象池结构体.‌
    /// </summary>
    public struct CellChunkOP
    {
        #region 结构体携带的其他信息

        /// <summary>
        /// CellChunk在团块空间的插入点(索引位置).可视作CellChunk的句柄.
        /// </summary>
        public CPIndex id;

        /// <summary>
        /// 以团块空间的CPIndex为键、GameObject为值的字典.CPEngine.useCellChunkOP = true 时会使用该字典来存储CellChunk句柄(CPIndex)对应的GameObject.如不使用则该字典不会被维护.
        /// </summary>
        public static Dictionary<CPIndex, GameObject> dataGO = new Dictionary<CPIndex, GameObject>();
        /// <summary>
        /// 以团块空间的CPIndex为键、CellChunkOP为值的字典.CPEngine.useCellChunkOP = true 时会使用该字典来存储CellChunk句柄(CPIndex)对应的CellChunkOP.如不使用则该字典不会被维护.
        /// </summary>
        public static Dictionary<CPIndex, CellChunkOP> dataOP = new Dictionary<CPIndex, CellChunkOP>();
        /// <summary>
        /// 以团块空间的CPIndex为键、CellItemManager为值的字典.CPEngine.useCellItem = true 时会使用该字典来存储CellChunk句柄(CPIndex)对应的CellItemManager.如不使用则该字典不会被维护.
        /// </summary>
        public static Dictionary<CPIndex, CellGridContainer> dataCGC = new Dictionary<CPIndex, CellGridContainer>();

        #endregion

        /// <summary>
        /// CellChunk游戏物体对象
        /// </summary>
        public GameObject gameObject;

        /// <summary>
        /// 对象的空间变换属性,包括位置(Position)、旋转(Rotation)和缩放(Scale)
        /// </summary>
        public Transform transform;

        /// <summary>
        /// 结构体的激活状态,应与gameobject的激活状态保持一致
        /// </summary>
        public bool actived;

        /// <summary>
        /// 对象池(静态字段,内存唯一).
        /// Stack<CellChunkOP>会存储对象中引用类型字段的副本,Push后对原引用类型字段置null不影响栈内已存储的副本,但会切断外部访问路径,
        /// Pop返回栈内副本可恢复外部访问路径.如CellChunkOP实例字段gameObject不为空并在Push后清空该字段再Pop,会重新恢复不为空的gameObject.
        /// </summary>
        public static Stack<CellChunkOP> pool;

        /// <summary>
        /// 触发自动初始化时栈的初始容量(默认值256).Pop、Push方法调用时若未初始化Stack,则会触发自动初始化.
        /// 自动初始化栈容量后pool.Capacity = autoStackCount但注意此时pool.Count依然是0(尚未存入具体对象)
        /// </summary>
        public static int autoStackCount = 256;

        /// <summary>
        /// 启用(激活游戏物体)
        /// </summary>
        public void Enable()
        {
            actived = true;
            if (gameObject != null)
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 禁用(不激活游戏物体)
        /// </summary>
        public void Disable()
        {
            actived = false;
            if (gameObject != null)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 从对象池(栈顶)拿CellChunkOP并返回. 没有就新建(ref方便用外面对象接收结果).当成功取出时objectPool参数对象的字段会被池中所取对象覆盖.
        /// </summary>
        /// <param name="objectPool"></param>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        /// <param name="active">结构体激活状态,默认激活</param>
        public static void Pop(ref CellChunkOP objectPool, bool createGameObject = true, bool active = true)
        {
            if (pool == null) pool = new Stack<CellChunkOP>(autoStackCount);
#if UNITY_EDITOR
            Debug.Assert(objectPool.gameObject == null);
#endif

#if BEPINEX
            if (pool.Count == 0)
#else
            if (!pool.TryPop(out objectPool)) //旧版TryPop不可用时启用(1/2) if (pool.Count == 0)
#endif
            {
                //Debug没有就新建
                objectPool = New(createGameObject, active);
            }
            else
            {//成功取出
#if BEPINEX
                objectPool = pool.Pop(); //旧版TryPop不可用时启用(2/2)
#endif
                //拿出对象之后的工作
                objectPool.actived = active;//激活停用状态应由对象池统一管理
#if UNITY_EDITOR
                Debug.Assert(objectPool.gameObject != null);
#endif
                //处理游戏物体
                if (objectPool.gameObject != null)
                {
                    objectPool.gameObject.SetActive(active);
                }
                else if (createGameObject)
                {
                    Debug.LogWarning("CellChunkOP.Pop: The gameObject in the CellChunkOP struct taken from the pool is null. A new GameObject will be created.");
                    //Debug若gameObject为空则创建一个新的
                    objectPool.gameObject = GameObject.Instantiate(CellSpacePrefab.CellChunk);
                    objectPool.gameObject.SetActive(active);
                    objectPool.transform = objectPool.gameObject.transform;
                }
            }

        }

        /// <summary>
        /// 将CellChunkOP退回对象池(ref的好处是可以从外面接收到函数修改后的结果).其上的游戏物体则失活处理(不摧毁,等待复用).
        /// </summary>
        /// <param name="objectPool"></param>
        public static void Push(ref CellChunkOP objectPool)
        {
            if (pool == null) pool = new Stack<CellChunkOP>(autoStackCount);
#if UNITY_EDITOR
            Debug.Assert(objectPool.gameObject != null);
#endif
            //退回对象池之前的准备工作
            objectPool.Disable();

            //结构体以当前状态的副本入栈顶
            pool.Push(objectPool);
            //清空主体引用以断开外部访问路径(不影响栈内副本)
            objectPool.gameObject = null;
            objectPool.transform = null;
        }

        /// <summary>
        /// 新建CellChunkOP结构体类型对象并返回.
        /// 如参数createGameObject为true,那么每个CellChunkOP.New()诞生结构体对象都会新建一个GameObject与之绑定.
        /// </summary>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        /// <param name="active">结构体激活状态,默认不激活</param>
        /// <returns></returns>
        public static CellChunkOP New(bool createGameObject = true, bool active = false)
        {
            CellChunkOP objectPool = new CellChunkOP();
            objectPool.actived = active;
            if (createGameObject)
            {
                objectPool.gameObject = GameObject.Instantiate(CellSpacePrefab.CellChunk);
                objectPool.gameObject.SetActive(active);
                objectPool.transform = objectPool.gameObject.transform;
            }
            return objectPool;
        }

        /// <summary>
        /// 初始化创建底层对象池(预填充).会调用CellChunkOP.New()创建GameObject.
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        /// /// <param name="active">结构体激活状态,默认不激活</param>
        public static CellChunkOP[] Init(int count, bool createGameObject = true, bool active = false)
        {
            pool = new Stack<CellChunkOP>(count);
            for (int i = 0; i < count; i++)
            {
                pool.Push(New(createGameObject, active));
            }
            return pool.ToArray();
        }

        /// <summary>
        /// 释放池资源.摧毁CellChunkOP结构体包括绑定的GameObject.
        /// </summary>
        public static void Destroy()
        {
            foreach (var o in pool)
            {
                GameObject.Destroy(o.gameObject);
            }
            pool.Clear();
        }

    }
}
