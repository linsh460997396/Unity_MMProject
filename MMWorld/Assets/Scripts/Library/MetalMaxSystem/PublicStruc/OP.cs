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
        /// 对象池(静态字段,内存唯一)
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
        /// 从对象池拿OP并返回. 没有就新建(ref方便用外面对象接收结果).
        /// </summary>
        /// <param name="objectPool"></param>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        public static void Pop(ref OP objectPool, bool createGameObject = true)
        {
#if UNITY_EDITOR
            Debug.Assert(objectPool.gameObject == null);
#endif
            if (!pool.TryPop(out objectPool))
            {
                objectPool = New(createGameObject);
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
            //清空(主要针对堆上的引用类型)防止对象池也摧毁情况下还存在着堆数据,导致这个结构体对象不再使用时,引发内存泄露.
            //引用类型的主要内存依然在堆上占用,跟刚退入栈的值类型结构体GameObject字段仍挂钩
            //Push方法后再读这个结构体,它除了对象池什么复杂引用都不挂钩了.若还有字段在关联着堆数据,那么垃圾就一直在,这里需要清空.
            objectPool.gameObject = null;
            objectPool.transform = null;
        }

        /// <summary>
        /// 新建OP结构体类型对象并返回.
        /// 如参数createGameObject为true,那么每个OP.New()诞生结构体对象都会新建一个GameObject与之绑定.
        /// </summary>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        /// <param name="actived">结构体激活状态</param>
        /// <returns></returns>
        public static OP New(bool createGameObject = true, bool actived = true)
        {
            OP objectPool = new();
            objectPool.actived = actived;
            if (createGameObject)
            {
                objectPool.gameObject = new GameObject();
                //objectPool.gameObject.SetActive(true); //创建后默认是激活状态
                objectPool.transform = objectPool.gameObject.transform;
            }
            return objectPool;
        }

        /// <summary>
        /// 初始化创建底层对象池(预填充).会调用OP.New().
        /// </summary>
        /// <param name="count">数量</param>
        /// <param name="createGameObject">是否创建GameObject,默认创建</param>
        public static OP[] Init(int count, bool createGameObject = true)
        {
            pool = new(count);
            for (int i = 0; i < count; i++)
            {
                pool.Push(New(createGameObject));
            }
            return pool.ToArray();
        }

        /// <summary>
        /// 释放池资源
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
