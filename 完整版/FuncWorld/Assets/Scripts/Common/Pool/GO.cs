using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏对象池.用于大量NPC、怪物等活动精灵个体对象复用GameObject,防止频繁创建摧毁导致掉帧问题.
/// </summary>
public struct GO
{
    /// <summary>
    /// 游戏物体对象
    /// </summary>
    public GameObject gameObject;
    /// <summary>
    /// 精灵渲染器
    /// </summary>
    public SpriteRenderer spriteRenderer;
    /// <summary>
    /// 对象的空间变换属性,包括位置(Position)、旋转(Rotation)和缩放(Scale)
    /// </summary>
    public Transform transform;
    /// <summary>
    /// 结构体的激活状态,应与gameobject的激活状态保持一致
    /// </summary>
    public bool actived;

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
    /// 将spriteRenderer颜色设为原色不透明(默认颜色状态).
    /// </summary>
    public void SetColorDefault()
    {
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    // 复制原始 shader 改了一下顶点 color 的值为 1/color 取倒数,从而可大幅度放大颜色值. 这样可实现全白
    // 可能是因为精度问题,编辑器中颜色码小于 8 会整个变黑. 故先暂定最小值为 8/255 即 0.0314

    /// <summary>
    /// 将spriteRenderer颜色设为接近白色
    /// </summary>
    public void SetColorWhite()
    {
        //const float minVal = 0.0314f; //URP使用
        const float minVal = 31.875f;
        spriteRenderer.color = new Color(minVal, minVal, minVal, 1f); //Unity内部使用0~1范围浮点数表示红蓝绿分量,以线性颜色空间进行处理
    }

    /******************************************************************************************/
    /******************************************************************************************/

    /// <summary>
    /// 对象池(静态字段,内存唯一).
    /// Stack<OP>会存储对象中引用类型字段的副本,Push后对原引用类型字段置null不影响栈内已存储的副本,但会切断外部访问路径,
    /// Pop返回栈内副本可恢复外部访问路径.如OP实例字段gameObject不为空并在Push后清空该字段再Pop,会重新恢复不为空的gameObject.
    /// </summary>
    public static Stack<GO> pool;
    /// <summary>
    /// 统一材质(静态字段,内存唯一).应先Instantiate初始化后再赋值给字段,不然使用时Unity会反复隐式实例化再赋值给组件实例的材质字段.
    /// </summary>
    public static Material material;
    /// <summary>
    /// 游戏物体实例化后的父级GameObject.
    /// </summary>
    public static GameObject group;

    /******************************************************************************************/
    /******************************************************************************************/

    /// <summary>
    /// 从对象池拿 GO 并返回. 没有就新建(ref方便在外面写个Go接收用)
    /// </summary>
    /// <param name="o">游戏物体名称</param>
    /// <param name="layer">游戏物体所在层</param>
    /// <param name="sortingLayerName">游戏物体精灵渲染器排序图层名称</param>
    public static void Pop(ref GO o, int layer = 0, string sortingLayerName = "Default")
    {
#if UNITY_EDITOR
        Debug.Assert(o.gameObject == null);
#endif
        if (!pool.TryPop(out o))
        {
            o = New();
        }
        o.gameObject.layer = layer;
        o.spriteRenderer.sortingLayerName = sortingLayerName;
    }

    /// <summary>
    /// 将 GO 退回对象池(ref的好处是可以从外面接收到函数修改后的值,结果会覆盖到原始结构体实例)
    /// </summary>
    /// <param name="o"></param>
    public static void Push(ref GO o)
    {
#if UNITY_EDITOR
        Debug.Assert(o.gameObject != null);
#endif
        //退回对象池之前的准备工作
        o.Disable();
        o.SetColorDefault();
        o.spriteRenderer.material = material;
        //赋值预制体时因其未被Instantiate,Unity会隐式创建材质实例相当于调用material.Instantiate()后再赋值给组件实例的材质字段.
        //Debug.Assert(o.spriteRenderer.material == material); //实测不一致,说明其实是spriteRenderer.material = material.Instantiate(),建议material作为参数填入前确保已被Instantiate初始化.
        o.gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        o.gameObject.transform.localScale = Vector3.one;

        //推入栈顶
        pool.Push(o);

        //清空主体引用以断开外部访问路径(不影响栈内副本)
        o.gameObject = null;
        o.spriteRenderer = null;
        o.transform = null;
        o.actived = false;
    }

    /// <summary>
    /// 新建 GO 并返回( 顺便设置统一的材质球 排序 pivot ).
    /// 创建的GameObject默认为false状态,请通过Enable方法启用.
    /// </summary>
    /// <returns></returns>
    public static GO New()
    {
        GO o = new();
        o.gameObject = new GameObject();
        o.gameObject.SetActive(false);
        if (group != null)
        {
            o.gameObject.transform.SetParent(group.transform);
        }
        o.spriteRenderer = o.gameObject.AddComponent<SpriteRenderer>();
        o.spriteRenderer.material = material;
        o.spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
        o.transform = o.gameObject.GetComponent<Transform>();
        return o;
    }

    /******************************************************************************************/
    /******************************************************************************************/

    /// <summary>
    /// 预填充.初始化材质和GO对象池.空GameObject会统一创建并绑定GO结构体对象.池内GO对象默认是禁用状态.
    /// </summary>
    /// <param name="material">建议材质填入前确保已被Instantiate初始化,否则未初始化的预制体会致Unity反复调用material.Instantiate()再赋值给组件实例的材质字段.</param>
    /// <param name="count"></param>
    /// <param name="gp">GameObject实例化后的父级收纳容器.</param>
    public static void Init(Material material, int count, GameObject gp = null)
    {
        GO.material = material;
#if UNITY_EDITOR
        Debug.Assert(GO.material != null);
#endif
        if (gp != null)
        {
            GO.group = gp;
        }
        GO.pool = new(count);
        for (int i = 0; i < count; i++)
        {
            pool.Push(New());
        }
    }

    /// <summary>
    /// 释放池资源.
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

//使用范例↓
//GameObject tempGroup = new GameObject("GOGroup");
//DontDestroyOnLoad(tempGroup);
//初始化底层绘制对象池
//GO.Init(material, 1, tempGroup); //Unity会反复隐式实例化再赋值给组件实例的材质字段.