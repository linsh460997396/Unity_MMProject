#if UNITY_EDITOR || UNITY_STANDALONE
using System.Collections.Generic;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    //官方池(ObjectPool)只是一些基本功能,进出池部分属性重置预填都没做的,还是用此轮子

    /// <summary>
    /// 对象池
    /// </summary>
    public struct OP
    {
        /// <summary>
        /// 游戏物体对象
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
        /// Stack<OP>会存储对象中引用类型字段的副本,Push后对原引用类型字段置null不影响栈内已存储的副本,但会切断外部访问路径,
        /// Pop返回栈内副本可恢复外部访问路径.如OP实例字段gameObject不为空并在Push后清空该字段再Pop,会重新恢复不为空的gameObject.
        /// </summary>
        public static Stack<OP> pool;

        /******************************************************************************************/
        /******************************************************************************************/

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
        /// 从对象池拿OP并返回. 没有就新建(ref方便用外面对象接收结果).当成功取出时objectPool参数对象的字段会被池中所取对象覆盖.
        /// </summary>
        /// <param name="objectPool"></param>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        /// <param name="active">结构体激活状态,默认激活</param>
        public static void Pop(ref OP objectPool, bool createGameObject = true, bool active = true)
        {
#if UNITY_EDITOR
            Debug.Assert(objectPool.gameObject == null);
#endif
            if (!pool.TryPop(out objectPool))
            {
                //Debug没有就新建
                objectPool = New(createGameObject, active);
            }
            else
            {//成功取出
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
                    Debug.LogWarning("OP.Pop: The gameObject in the OP struct taken from the pool is null. A new GameObject will be created.");
                    //Debug若gameObject为空则创建一个新的
                    objectPool.gameObject = new GameObject();
                    objectPool.gameObject.SetActive(active);
                    objectPool.transform = objectPool.gameObject.transform;
                }
            }
            
        }

        /// <summary>
        /// 将OP退回对象池(ref的好处是可以从外面接收到函数修改后的结果).其上的游戏物体则失活处理(不摧毁,等待复用).
        /// </summary>
        /// <param name="objectPool"></param>
        public static void Push(ref OP objectPool)
        {
#if UNITY_EDITOR
            Debug.Assert(objectPool.gameObject != null);
#endif
            //退回对象池之前的准备工作
            objectPool.Disable();

            //推入栈顶
            pool.Push(objectPool);
            //清空主体引用以断开外部访问路径(不影响栈内副本)
            objectPool.gameObject = null;
            objectPool.transform = null;
        }

        /// <summary>
        /// 新建OP结构体类型对象并返回.
        /// 如参数createGameObject为true,那么每个OP.New()诞生结构体对象都会新建一个GameObject与之绑定.
        /// </summary>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        /// <param name="active">结构体激活状态,默认不激活</param>
        /// <returns></returns>
        public static OP New(bool createGameObject = true, bool active = false)
        {
            OP objectPool = new();
            objectPool.actived = active;
            if (createGameObject)
            {
                objectPool.gameObject = new GameObject();
                objectPool.gameObject.SetActive(active);
                objectPool.transform = objectPool.gameObject.transform;
            }
            return objectPool;
        }

        /// <summary>
        /// 初始化创建底层对象池(预填充).会调用OP.New()创建GameObject.
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        /// /// <param name="active">结构体激活状态,默认不激活</param>
        public static OP[] Init(int count, bool createGameObject = true, bool active = false)
        {
            pool = new(count);
            for (int i = 0; i < count; i++)
            {
                pool.Push(New(createGameObject, active));
            }
            return pool.ToArray();
        }

        /// <summary>
        /// 释放池资源.摧毁OP结构体包括绑定的GameObject.
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
#endif
