using UnityEngine;

namespace SimWorld
{
    public class SWEffectExplosion
    {
        public Main_SimWorld scene;                                 // 指向场景
        public SWStage stage;                                 // 指向舞台
        public Sprite[] sprites;                            // 指向动画帧[]

        public GO go;                                       // 保存底层 u3d 资源

        public const float displayBaseScale = 1f;           // 显示放大修正
        public const float frameIndexStep = 0.2f;           // 帧步进值
        public float frameIndex;                            // 帧下标
        public float x, y;                                  // 逻辑坐标

        public SWEffectExplosion(SWStage stage_, float x_, float y_, float scale_)
        {
            // 各种基础初始化
            stage = stage_;
            scene = stage_.scene;
            sprites = scene.sprites_explosions;
            stage.effectExplosions.Add(this);

            // 从对象池分配 u3d 底层对象
            GO.Pop(ref go, 0, "ForeGround");
            go.transform.SetPositionAndRotation(new Vector3(x_ * Main_SimWorld.designWidthToCameraRatio, -y_ * Main_SimWorld.designWidthToCameraRatio, 0)
                , Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
            var s = displayBaseScale * scale_;
            go.transform.localScale = new Vector3(s, s, s);
            x = x_;
            y = y_;
        }

        public bool Update()
        {
            // 步进动画
            frameIndex += frameIndexStep;

            // 动画放完就该销毁了
            return (int)frameIndex >= sprites.Length;
        }

        public virtual void Draw(float cx, float cy)
        {
            if (x < cx - Main_SimWorld.designWidth_2
        || x > cx + Main_SimWorld.designWidth_2
        || y < cy - Main_SimWorld.designHeight_2
        || y > cy + Main_SimWorld.designHeight_2)
            {
                go.Disable();
            }
            else
            {
                go.Enable();

                // 同步动画帧
                go.spriteRenderer.sprite = sprites[(int)frameIndex];
            }
        }

        public void Destroy()
        {
#if UNITY_EDITOR
            if (go.gameObject != null)           // unity 点击停止按钮后,这些变量似乎有可能提前变成 null
#endif
            {
                // 将 u3d 底层对象返回池
                GO.Push(ref go);
            }
        }
    }
}