using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 数字特效
    /// </summary>
    public class EffectNumber
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
        /// 12个元素的对象池结构体（数字特效尺寸不会超过1.234e56789总长度即12位足够）
        /// </summary>
        public GO[] gos = new GO[12];
        /// <summary>
        /// 字符长度（特效数字char数量或精灵数量）
        /// </summary>
        public int size;
        /// <summary>
        /// [逻辑坐标]每帧在Y轴上的增量，它用于控制特效在垂直方向上的移动速度
        /// </summary>
        public const float incY = Scene.gridSize / 120 * Scene.fps;
        /// <summary>
        /// 生命周期（以帧数为单位），决定了特效显示多久
        /// </summary>
        public const int life = (int)(Scene.fps * 0.5);
        /// <summary>
        /// 逻辑坐标
        /// </summary>
        public float pixelX, pixelY;
        /// <summary>
        /// 缩放比例
        /// </summary>
        public float scale;
        /// <summary>
        /// 结束帧（以帧数为单位的剩余生命时间）,用于跟踪效特效还有多少帧才会消失或结束
        /// </summary>
        public int endLifeTime;

        /// <summary>
        /// [构造函数]数字特效
        /// </summary>
        /// <param name="lv_stage"></param>
        /// <param name="lv_pixelX">逻辑坐标</param>
        /// <param name="lv_pixelY">逻辑坐标</param>
        /// <param name="lv_scale">缩放比例</param>
        /// <param name="lv_value">数值</param>
        /// <param name="lv_criticalHit">是否暴击</param>
        public EffectNumber(Stage lv_stage, float lv_pixelX, float lv_pixelY, float lv_scale, double lv_value, bool lv_criticalHit)
        {
            stage = lv_stage;
            scene = lv_stage.scene;
            pixelX = lv_pixelX;
            pixelY = lv_pixelY;
            scale = lv_scale;
            //结束帧 = 当前帧+周期帧
            endLifeTime = scene.time + life;
            //将自身添加到关卡数字特效列表
            stage.effectNumbers.Add(this);

            var sb = NumConverter.ToStringEN(lv_value);
            size = sb.Length;//sb的尺寸不会超过1.234e56789总长度即12位
            for (int i = 0; i < size; i++)
            {
                var o = new GO();
                //从对象池分配对象，层0，设置精灵渲染器排序图层名称为FG2（Foreground是前景图层，通常指那些位于画面最前、离观众最近的元素部分）
                GO.Pop(ref o, 0, "FG2");
                o.spriteRenderer.sprite = scene.sprites_font_outline[sb[i] - 32];//char可直接与数字加减，如sb[i]='2'，结果是(int)'2' -32=50=32=18，对应精灵数组索引[18]，即2的精灵图片
                o.transform.localScale = new Vector3(scale, scale, scale);
                if (lv_criticalHit)
                {
                    //如果是暴击则设置红色
                    o.spriteRenderer.color = Color.red;
                }
                gos[i] = o;//最长数字特效是12个对象元素（不会超界）
            }
        }

        /// <summary>
        /// 数字特效更新，每帧操作一次数字的Y轴垂直上移（修改逻辑坐标值）
        /// </summary>
        /// <returns>当结束帧=当前帧返回真，否则返回假</returns>
        public bool Update()
        {
            pixelY += incY;
            return endLifeTime < scene.time;
        }

        /// <summary>
        /// [虚函数]绘制数字特效，当达到参数所填逻辑坐标值时消失
        /// </summary>
        /// <param name="cx">玩家逻辑位置</param>
        /// <param name="cy">玩家逻辑位置</param>
        public virtual void Draw(float cx, float cy)
        {
            if (pixelX < cx - Scene.designWidth_2 || pixelX > cx + Scene.designWidth_2 || pixelY < cy - Scene.designHeight_2 || pixelY > cy + Scene.designHeight_2)
            {//数字特效的逻辑坐标达到参数值时进行对象禁用
                for (int i = 0; i < size; ++i)
                {//遍历特效数字长度，禁用每个精灵图片的游戏物体
                    gos[i].Disable();
                }
            }
            else
            {//数字特效的逻辑坐标未达到参数值
                for (int i = 0; i < size; ++i)
                {
                    //启用对象
                    gos[i].Enable();
                    //绘制数字特效，以xy为原点往右列出全部长度的数字（精灵图片）
                    gos[i].transform.position = new Vector3((pixelX + i * 8 * scale) / Scene.gridSize, pixelY / Scene.gridSize, 0);
                }
            }
        }

        /// <summary>
        /// 数字特效摧毁（退回gos对象池）
        /// </summary>
        public void Destroy()
        {
            //遍历全部数字特效
            for (int i = 0; i < size; ++i)
            {
#if UNITY_EDITOR
                if (gos[i].gameObject != null)
#endif
                {
                    GO.Push(ref gos[i]);//退回对象池
                }
            }
        }
    }
}