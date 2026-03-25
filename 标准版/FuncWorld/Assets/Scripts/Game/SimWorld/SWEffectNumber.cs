using UnityEngine;

namespace SimWorld
{
    public class SWEffectNumber
    {
        public Main_SimWorld scene;
        public SWStage stage;

        public GO[] gos = new GO[12];
        public int size;

        public const float incY = -0.5f / 60 * Main_SimWorld.fps;
        public const int life = (int)(Main_SimWorld.fps * 0.5);
        public float x, y, scale;
        public int endLifeTime;

        // todo: color ?
        public SWEffectNumber(SWStage stage_, float x_, float y_, float scale_, double v, bool criticalHit)
        {
            stage = stage_;
            scene = stage_.scene;

            x = x_;
            y = y_;
            scale = scale_;
            endLifeTime = scene.time + life;
            stage.effectNumbers.Add(this);

            var sb = SWHelpers.ToStringEN(v);
            size = sb.Length;
            for (int i = 0; i < size; i++)
            {
                var o = new GO();
                GO.Pop(ref o, 0, "SL2");
                o.spriteRenderer.sprite = scene.sprites_font_outline[sb[i] - 32];
                o.transform.localScale = new Vector3(scale, scale, scale);
                if (criticalHit)
                {
                    o.spriteRenderer.color = Color.red;
                }
                gos[i] = o;
            }

        }

        public bool Update()
        {
            y += incY;
            return endLifeTime < scene.time;
        }

        public virtual void Draw(float cx, float cy)
        {
            if (x < cx - Main_SimWorld.designWidth_2
            || x > cx + Main_SimWorld.designWidth_2
            || y < cy - Main_SimWorld.designHeight_2
            || y > cy + Main_SimWorld.designHeight_2)
            {
                for (int i = 0; i < size; ++i)
                {
                    gos[i].Disable();
                }
            }
            else
            {
                for (int i = 0; i < size; ++i)
                {
                    gos[i].Enable();
                    gos[i].transform.position = new Vector3(
                        (x + i * 8 * scale) * Main_SimWorld.designWidthToCameraRatio
                        , -y * Main_SimWorld.designWidthToCameraRatio
                        , 0);
                }
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < size; ++i)
            {
#if UNITY_EDITOR
                if (gos[i].gameObject != null)
#endif
                {
                    GO.Push(ref gos[i]);
                }
            }
        }
    }
}