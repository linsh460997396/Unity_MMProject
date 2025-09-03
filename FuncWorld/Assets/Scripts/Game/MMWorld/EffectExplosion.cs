using CellSpace;
using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 爆炸特效
    /// </summary>
    public class EffectExplosion
    {
        /// <summary>
        /// 场景
        /// </summary>
        public Scene scene;
        /// <summary>
        /// 关卡
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 动画帧[]
        /// </summary>
        public Sprite[] sprites;
        /// <summary>
        /// 对象池结构体
        /// </summary>
        public GO go;
        /// <summary>
        /// 基础显示缩放比例(默认值1f)
        /// </summary>
        public const float displayBaseScale = 1f;
        /// <summary>
        /// 帧步进值(决定精灵动画播放速度)
        /// </summary>
        public const float frameIndexStep = 0.2f;
        /// <summary>
        /// 动画帧下标(决定动画播放的精灵索引)
        /// </summary>
        public float frameIndex;
        /// <summary>
        /// 空间内精灵锚点的逻辑坐标(以空间左下为原点),与设计分辨率对比时用于隐藏超出屏幕范围的怪物游戏物体
        /// </summary>
        public float pixelRaw, pixelColumn;

        /// <summary>
        /// [构造函数]爆炸特效
        /// </summary>
        /// <param name="lv_stage">关卡</param>
        /// <param name="lv_pixelRow"></param>
        /// <param name="lv_pixelColumn"></param>
        /// <param name="lv_scale">缩放值,用来跟基础显示缩放比例(1f)相乘</param>
        public EffectExplosion(Stage lv_stage, float lv_pixelRow, float lv_pixelColumn, float lv_scale)
        {
            stage = lv_stage;//初始化关卡
            scene = lv_stage.scene;//初始化场景
            sprites = scene.sprites_explosions;//初始化爆炸精灵组
            stage.effectExplosions.Add(this);//将自身添加到关卡爆炸特效列表
            //从对象池分配对象,游戏物体层0,设置精灵渲染器排序图层名称为FG(Foreground是前景图层,通常指那些位于画面最前、离观众最近的元素部分)
            GO.Pop(ref go, 0, "FG");
            //设置对象位置和旋转(Quaternion.identity = Quaternion.Euler(0,0,0)//无任何旋转)
            if (CPEngine.horizontalMode)
            {
                //横版模式Z=0
                go.transform.SetPositionAndRotation(new Vector3(lv_pixelRow / Scene.gridSize, lv_pixelColumn / Scene.gridSize, 0), Quaternion.identity);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {
                //单层地形模式Y=1+aboveHeight
                go.transform.SetPositionAndRotation(new Vector3(lv_pixelRow / Scene.gridSize, 1 + Scene.aboveHeight, lv_pixelColumn / Scene.gridSize), Quaternion.Euler(90, 0, 0));
            }
            else
            {
                Debug.LogError("Can not support normal 3D mode");
            }
            var s = displayBaseScale * lv_scale; //计算最终缩放量
            go.transform.localScale = new Vector3(s, s, s);  //进行缩放

            //记录逻辑坐标
            pixelRaw = lv_pixelRow;
            pixelColumn = lv_pixelColumn;
        }

        /// <summary>
        /// 爆炸特效更新:步进1次动画帧索引
        /// </summary>
        /// <returns>达到最大帧索引则返回true(动画完销毁),否则返回false</returns>
        public bool Update()
        {
            //步进动画
            frameIndex += frameIndexStep;

            //动画完就该销毁了(达到了最大帧索引时返回真,否则返回假)
            return (int)frameIndex >= sprites.Length;
        }

        /// <summary>
        /// [虚函数]绘制图形(播放精灵动画)
        /// </summary>
        /// <param name="row">玩家逻辑坐标</param>
        /// <param name="column">玩家逻辑坐标</param>
        public virtual void Draw(float row, float column)
        {
            if (pixelRaw < row - Scene.designWidth_2 || pixelRaw > row + Scene.designWidth_2 || pixelColumn < column - Scene.designHeight_2 || pixelColumn > column + Scene.designHeight_2)
            {//因人始终在屏幕中间,特效对象不超过玩家逻辑位置+半个屏幕的逻辑像素距离时显示,不在屏幕内的对象就不显示(屏幕范围是角色摄像头范围,用designWidthToCameraRatio调整摄像头)
                go.Disable();//屏幕外禁用结构体上的游戏物体
            }
            else
            {
                go.Enable();//屏幕内激活结构体上的游戏物体
                            //同步对象精灵为当前动画帧的精灵
                go.spriteRenderer.sprite = sprites[(int)frameIndex];
            }
        }

        /// <summary>
        /// 摧毁爆炸特效(退回对象池)
        /// </summary>
        public void Destroy()
        {
#if UNITY_EDITOR
            if (go.gameObject != null)           //Unity点击停止按钮后这些变量似乎有可能提前变成null
#endif
            {
                //退回对象池
                GO.Push(ref go);
            }
        }
    }
}