#if UNITY_EDITOR || UNITY_STANDALONE
using System.Collections.Generic;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    //官方池（ObjectPool）只是一些基本功能，进出池部分属性重置预填都没做的，还是用此轮子

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
        /// 对象的空间变换属性，包括位置（Position）、旋转（Rotation）和缩放（Scale）
        /// </summary>
        public Transform transform;

        /// <summary>
        /// 激活状态
        /// </summary>
        public bool actived;

        /// <summary>
        /// 对象池（静态字段，内存唯一）
        /// </summary>
        public static Stack<OP> pool;

        /******************************************************************************************/
        /******************************************************************************************/

        /// <summary>
        /// 启用(激活游戏物体)
        /// </summary>
        public void Enable()
        {
            if (!actived)
            {
                actived = true;
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 禁用(不激活游戏物体)
        /// </summary>
        public void Disable()
        {
            if (actived)
            {
                actived = false;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 从对象池拿OP并返回. 没有就新建（ref方便用外面对象接收结果）
        /// </summary>
        /// <param name="objectPool">游戏物体名称</param>
        public static void Pop(ref OP objectPool)
        {
#if UNITY_EDITOR
            Debug.Assert(objectPool.gameObject == null);
#endif
            if (!pool.TryPop(out objectPool))
            {
                objectPool = New();
            }
        }

        /// <summary>
        /// 将OP退回对象池（ref的好处是可以从外面接收到函数修改后的结果）
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
            //清空（主要针对堆上的引用类型）防止对象池也摧毁情况下还存在着堆数据，导致这个结构体对象不再使用时，引发内存泄露
            //引用类型内存依然在堆上，跟刚推入栈的结构体复制体值类型数据中的GameObject内存索引仍挂钩，这个结构体实例清空后只留下一个静态pool字段即可（里面存着大量原结构体的复制体）
            //退回后再读这个结构体，除了pool什么都没有
            objectPool.gameObject = null;
            objectPool.transform = null;
            //↑光清空了池内复制体值类型，外面若还有个原结构体值类型在关联着堆数据，那么垃圾就一直在
            objectPool.actived = false;
        }

        /// <summary>
        /// 新建OP并返回
        /// </summary>
        /// <returns></returns>
        public static OP New()
        {
            OP objectPool = new();
            objectPool.gameObject = new GameObject();
            objectPool.transform = objectPool.gameObject.GetComponent<Transform>();
            objectPool.gameObject.SetActive(false); //创建后默认隐藏
            return objectPool;
        }

        /// <summary>
        /// 初始化创建底层对象池（预填充）
        /// </summary>
        /// <param name="material"></param>
        /// <param name="count"></param>
        public static void Init(int count)
        {
            OP.pool = new(count);
            for (int i = 0; i < count; i++)
            {
                pool.Push(New());
            }
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
