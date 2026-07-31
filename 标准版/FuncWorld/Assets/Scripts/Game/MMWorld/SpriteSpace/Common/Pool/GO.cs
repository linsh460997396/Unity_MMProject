using MetalMaxSystem.Unity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏精灵渲染器专用(GameObject)对象池结构体.
/// 定位是用于大量角色子弹特效等活动精灵个体对象复用GameObject,防止频繁创建摧毁导致掉帧问题.
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

    //配套的Shader"Custom/ReciprocalColorFixed"复制自原始shader,改了顶点color的值为1/color,取倒数从而可大幅度放大颜色值实现全白
    //可能因精度问题,编辑器中颜色码小于8会整个变黑,故先暂定最小值为8/255即0.0314

    //Shader"Custom/ReciprocalColorFixed"在「Universal 2D主光照Pass」和「UniversalForward 无光照Pass」的顶点着色器中实现了o.color = (1.0f / v.color) * _Color* _RendererColor的运算
    //其中v.color是SpriteRenderer自带的模型顶点颜色,通过spriteRenderer.color赋值颜色分量,Shader会自动对这个原生顶点颜色做取倒数运算
    //再和调用SetColor方法通过MaterialPropertyBlock传入的_Color属性相乘,最终得到顶点的输出颜色值

    /// <summary>
    /// 将spriteRenderer颜色设为原色不透明(默认颜色状态).
    /// </summary>
    public void SetColorDefault()
    {
        spriteRenderer.color = dft; 
        //SetColor(spriteRenderer, dft);
    }

    /// <summary>
    /// 将spriteRenderer颜色设为接近白色.若打包前有效打包后丢失GPU实例化效果,那么去编辑器菜单-项目设置-图形-找到"实例化变体"选择"保持全部"即可解决.
    /// 凡运行时动态新建材质‌并赋值Shader的,在打包构建时这些动态生成的材质并不存在,Unity的静态分析器无法预知会用到哪些关键字组合,从而默认剥离未使用的变体.
    /// </summary>
    public void SetColorWhite()
    {
        spriteRenderer.color = flash; //Unity内部使用0~1范围浮点数表示红蓝绿分量,以线性颜色空间进行处理
        //SetColor(spriteRenderer, flash);
    }

    /******************************************************************************************/

    //const float minVal = 0.0314f;//URP使用
    const float minVal = 31.875f;  //无管线使用名为"Custom/ReciprocalColorFixed"的Shader,取倒数为白闪效果
                                   //把顶点颜色设为趋近纯黑,触发Shader的1/颜色逻辑,瞬间输出纯白硬闪
                                   //Shader需用"Custom/ReciprocalColorFixed",利用顶点颜色倒数的特性,实现纯黑→纯白的瞬间切换
                                   //但无法做渐变过渡,也不能自定义闪烁色,适合只需要硬闪全白、追求极致轻量的受击反馈场景

    private static Color dft = new Color(1f, 1f, 1f, 1f);
    private static Color flash = new Color(minVal, minVal, minVal, 1f);

    private static MaterialPropertyBlock _flashPropertyBlock;
    public static MaterialPropertyBlock FlashPropertyBlock
    {
        get
        {
            if (_flashPropertyBlock == null) _flashPropertyBlock = new MaterialPropertyBlock();
            return _flashPropertyBlock;
        }
    }

    /// <summary>
    /// 给单个角色触发受击硬闪效果,不会破坏全局实例化合批.
    /// </summary>
    /// <param name="spriteRenderer"></param>
    /// <param name="color"></param>
    public void SetColor(SpriteRenderer lv_spriteRenderer, Color lv_color)
    {
        FlashPropertyBlock.Clear(); //直接复用全局单例对象,清空上次残留属性再写入新的颜色值
        FlashPropertyBlock.SetColor("_Color", lv_color);
        lv_spriteRenderer.SetPropertyBlock(FlashPropertyBlock);
    }

    /******************************************************************************************/

    /// <summary>
    /// 对象池(静态字段,内存唯一).
    /// Stack<OP>会存储对象中引用类型字段的副本,Push后对原引用类型字段置null不影响栈内已存储的副本,但会切断外部访问路径,
    /// Pop返回栈内副本可恢复外部访问路径.如OP实例字段gameObject不为空并在Push后清空该字段再Pop,会重新恢复不为空的gameObject.
    /// </summary>
    public static Stack<GO> pool;

    public static Material _mat;
    /// <summary>
    /// 统一材质(静态字段,内存唯一).
    /// </summary>
    public static Material Mat
    {
        get
        {
            if (_mat == null)
            {
                _mat = UKit.Material;
            }
            return _mat;
        }
        set { _mat = value; }
    }
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
        o.spriteRenderer.material = Mat;
        //注意避免:Unity 中产生材质副本(即生成 <MaterialName> (Instance))的典型操作是访问 renderer.material 属性(getter/setter)
        //Debug.Assert(o.spriteRenderer.material == material); //实测不一致,说明其实是spriteRenderer.material = material.Instantiate()
        o.gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        o.gameObject.transform.localScale = Vector3.one;

        //结构体以当前状态的副本入栈顶
        pool.Push(o);

        //清空主体引用以断开外部访问路径(不影响栈内副本,Stack中的副本依然是拥有值的以下字段)
        o.gameObject = null;
        o.spriteRenderer = null;
        o.transform = null;
        //o.actived = false; //函数前面调用Disable的时候已经是false了
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
        o.spriteRenderer.material = Mat;
        o.spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
        o.transform = o.gameObject.GetComponent<Transform>();
        return o;
    }

    /******************************************************************************************/
    /******************************************************************************************/

    /// <summary>
    /// 预填充.初始化材质和GO对象池.空GameObject会统一创建并绑定GO结构体对象.池内GO对象默认是禁用状态.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="material"></param>
    /// <param name="gp">GameObject实例化后的父级收纳容器</param>
    public static void Init(int count, Material material = null, GameObject gp = null)
    {
        if (material != null) GO.Mat = material;
#if UNITY_EDITOR
        Debug.Assert(GO.Mat != null);
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
    /// 预填充.初始化材质和GO对象池.空GameObject会统一创建并绑定GO结构体对象.池内GO对象默认是禁用状态.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="gp">GameObject实例化后的父级收纳容器</param>
    public static void Init(int count, GameObject gp = null)
    {
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
