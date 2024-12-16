using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池 
/// </summary>
public struct GO {
    /// <summary>
    /// 游戏物体对象
    /// </summary>
    public GameObject gameObject;
    /// <summary>
    /// 精灵渲染器
    /// </summary>
    public SpriteRenderer spriteRenderer;
    /// <summary>
    /// 转换
    /// </summary>
    public Transform transform;
    /// <summary>
    /// 激活状态
    /// </summary>
    public bool actived;

    /******************************************************************************************/
    /******************************************************************************************/

    /// <summary>
    /// 启用
    /// </summary>
    public void Enable() {
        if (!actived) {
            actived = true;
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 禁用
    /// </summary>
    public void Disable() {
        if (actived) {
            actived = false;
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 将SpriteRenderer（字段r）的颜色设置为原色不透明。
    /// </summary>
    public void SetColorNormal() {
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    // 复制原始 shader 改了一下顶点 color 的值为 1/color 取倒数，从而可大幅度放大颜色值. 这样可实现全白
    // 可能是因为精度问题，编辑器中颜色码小于 8 会整个变黑. 故先暂定最小值为 8/255 即 0.0314

    /// <summary>
    /// 将SpriteRenderer（字段r）的颜色设置为一个接近白色的颜色
    /// </summary>
    public void SetColorWhite() {
        //const float minVal = 0.0314f;
        const float minVal = 31.875f;
        spriteRenderer.color = new Color(minVal, minVal, minVal, 1f); //Unity内部使用0~1范围浮点数表示红蓝绿分量，以线性颜色空间进行处理
    }

    /******************************************************************************************/
    /******************************************************************************************/

    /// <summary>
    /// 对象池（静态字段，内存唯一）
    /// </summary>
    public static Stack<GO> pool;

    /// <summary>
    /// 统一材质（静态字段，内存唯一）
    /// </summary>
    public static Material material;

    /******************************************************************************************/
    /******************************************************************************************/

    /// <summary>
    /// 从对象池拿 GO 并返回. 没有就新建（ref方便在外面写个Go接收用）
    /// </summary>
    /// <param name="o">游戏物体名称</param>
    /// <param name="layer">游戏物体所在层</param>
    /// <param name="sortingLayerName">游戏物体精灵渲染器排序图层名称</param>
    public static void Pop(ref GO o, int layer = 0, string sortingLayerName = "Default") {
#if UNITY_EDITOR
        Debug.Assert(o.gameObject == null);
#endif
        if (!pool.TryPop(out o)) {
            o = New();
        }
        o.gameObject.layer = layer;
        o.spriteRenderer.sortingLayerName = sortingLayerName;
    }

    // 将 GO 退回对象池（ref的好处是可以从外面接收到函数修改后的值，结果会体现到原始结构体实例）
    public static void Push(ref GO o) {
#if UNITY_EDITOR
        Debug.Assert(o.gameObject != null);
#endif
        //退回对象池之前的准备工作
        o.Disable();
        o.SetColorNormal();
        o.spriteRenderer.material = material;
        //Debug.Assert(o.spriteRenderer.material == material); //不一致，说明其实是r.mat = mat.Instantiate()
        o.gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        o.gameObject.transform.localScale = Vector3.one;
        //推入栈顶
        pool.Push(o);
        //清空（主要针对堆上的引用类型）防止对象池也摧毁情况下还存在着堆数据，导致这个结构体对象不再使用时，引发内存泄露
        //引用类型内存依然在堆上，跟刚推入栈的结构体复制体值类型数据中的GameObject内存索引仍挂钩，这个结构体实例清空后只留下一个静态pool字段即可（里面存着大量原结构体的复制体）
        //退回后再读这个结构体，除了pool什么都没有
        o.gameObject = null;
        o.spriteRenderer = null;
        o.transform = null;
        //↑光清空了池内复制体值类型，外面若还有个原结构体值类型在关联着堆数据，那么垃圾就一直在
        o.actived = false;
    }

    // 新建 GO 并返回( 顺便设置统一的材质球 排序 pivot )
    public static GO New() {
        GO o = new();
        o.gameObject = new GameObject();
        o.spriteRenderer = o.gameObject.AddComponent<SpriteRenderer>();
        o.spriteRenderer.material = material;
        o.spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
        o.transform = o.gameObject.GetComponent<Transform>();
        o.gameObject.SetActive(false);
        return o;
    }


    /******************************************************************************************/
    /******************************************************************************************/


    /// <summary>
    /// 预填充
    /// </summary>
    /// <param name="material"></param>
    /// <param name="count"></param>
    public static void Init(Material material, int count) {
#if UNITY_EDITOR
        Debug.Assert(GO.material == null);
#endif
        GO.material = material;
        GO.pool = new(count);
        for (int i = 0; i < count; i++) {
            pool.Push(New());
        }
    }

    /// <summary>
    /// 释放池资源
    /// </summary>
    public static void Destroy() {
        foreach (var o in pool) {
            GameObject.Destroy(o.gameObject);
        }
        pool.Clear();
    }
}

// 一、整体功能概述 
// 这个`GO`结构体主要用于管理游戏对象（`GameObject`）相关的操作，包括对象的激活与禁用、颜色设置、对象的获取与回收以及初始化和销毁等操作。 

// 二、具体功能分析 

// （一）对象状态管理 
// 1. 激活与禁用功能 
//    - `Enable`方法 
//      - 功能：如果对象当前未激活（`actived`为`false`），则将`actived`设置为`true`，并通过`gameObject.SetActive(true)`激活对应的游戏对象。 
//    - `Disable`方法 
//      - 功能：如果对象当前已激活（`actived`为`true`），则将`actived`设置为`false`，并通过`gameObject.SetActive(false)`禁用对应的游戏对象。 
// 2. 颜色设置功能 
//    - `SetColorNormal`方法 
//      - 功能：将`SpriteRenderer`（`spriteRenderer`）的颜色设置为白色（`new Color(1f, 1f, 1f, 1f)`）。 
//    - `SetColorWhite`方法 
//      - 功能：将`SpriteRenderer`（`spriteRenderer`）的颜色设置为一个接近黑色的颜色（`new Color(minVal, minVal, minVal, 1f)`，其中`minVal = 0.0314f`）。 

// （二）对象池管理 
// 1. 对象获取（`Pop`方法） 
//    - 功能： 
//      - 如果对象池（`pool`）中无法弹出对象（`!pool.TryPop(out o)`），则通过`New`方法创建一个新的`GO`对象。然后设置对象的层（`o.gameObject.layer = layer`）和排序层名称（`o.spriteRenderer.sortingLayerName = sortingLayerName`）。同时在编辑器模式下（`#if UNITY_EDITOR`），会进行断言（`Debug.Assert(o.gameObject == null)`），确保初始状态符合预期。 
// 2. 对象回收（`Push`方法） 
//    - 功能： 
//      - 首先禁用对象（`o.Disable()`），然后将颜色设置为正常（`o.SetColorNormal()`），设置`SpriteRenderer`的材质（`o.spriteRenderer.material = material`），将游戏对象的位置和旋转重置为初始状态（`o.gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity)`），将缩放设置为单位缩放（`o.gameObject.transform.localScale = Vector3.one`）。最后将对象推回对象池（`pool.Push(o)`），并将`GO`结构体中的相关引用设置为`null`（`o.gameObject = null; o.spriteRenderer = null; o.transform = null; o.actived = false;`），在编辑器模式下（`#if UNITY_EDITOR`），会进行断言（`Debug.Assert(o.gameObject!= null)`），确保对象在回收前处于有效状态。 
// 3. 对象创建（`New`方法） 
//    - 功能： 
//      - 创建一个新的`GO`对象实例。创建一个新的游戏对象（`o.gameObject = new GameObject()`），添加`SpriteRenderer`组件（`o.spriteRenderer = o.gameObject.AddComponent()`），设置`SpriteRenderer`的材质（`o.spriteRenderer.material = material`）、精灵排序点（`o.spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot`），获取游戏对象的变换组件（`o.transform = o.gameObject.GetComponent()`），并初始将游戏对象设置为禁用状态（`o.gameObject.SetActive(false)`）。 
// 4. 对象池初始化（`Init`方法） 
//    - 功能： 
//      - 在编辑器模式下（`#if UNITY_EDITOR`），会进行断言（`Debug.Assert(GO.material == null)`），确保材质未被初始化。然后初始化结构体中的静态材质（`GO.material = material`），创建指定大小（`count`）的对象池（`GO.pool = new(count)`），并将`count`个新创建的`GO`对象推到对象池中（`pool.Push(New())`）。 
// 5. 对象池销毁（`Destroy`方法） 
//    - 功能： 
//      - 遍历对象池中的每个对象（`foreach (var o in pool)`），销毁对应的游戏对象（`GameObject.Destroy(o.gameObject)`），然后清空对象池（`pool.Clear()`）。
