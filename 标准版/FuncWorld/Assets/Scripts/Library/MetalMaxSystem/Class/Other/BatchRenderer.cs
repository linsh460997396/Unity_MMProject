//#define UNITY_STANDALONE
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MetalMaxSystem.Unity
{
    public enum BlendMode
    {
        Additive,
        AlphaBlend,
        Opaque
    }

    public enum CoordinateSystem
    {
        ScreenPixels,
        World,
        Normalized
    }

    /// <summary>
    /// 批处理渲染器.轻量便捷渲染组件,基于Unity内置的GL立即绘制接口封装.
    /// 支持不同纹理的四边形渲染,自动合并相同纹理的四边形.
    /// 无管线强依赖,兼容Unity Editor和Standalone平台,直接调用底层GL命令不受SRP版本限制.
    /// 针对2D小体量精灵、UI元素、线条的快速绘制,简化手动绘制开发流程,面向轻量级即时渲染需求.
    /// </summary>
    public class BatchRenderer : MonoBehaviour
    {
        private static BatchRenderer _instance;

        public static BatchRenderer Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = GameObject.Find("BatchRenderer");
                    if (obj == null) obj = new GameObject("BatchRenderer");
                    if (obj.GetComponent<BatchRenderer>() == null) _instance = obj.AddComponent<BatchRenderer>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        [Tooltip("混合模式")]
        public BlendMode blendMode = BlendMode.Additive;

        [Tooltip("坐标系")]
        public CoordinateSystem coordinateSystem = CoordinateSystem.ScreenPixels;

        [Tooltip("自动渲染")]
        public bool autoRender = true;

        [Tooltip("渲染相机(坐标系为World时使用)")]
        public Camera renderCamera;

        [Tooltip("初始批处理容量")]
        public int initialBatchCapacity = 64;

        [Tooltip("每个批处理的初始四边形容量")]
        public int initialQuadCapacity = 256;

        Material material;

        struct Quad
        {
            public Vector2 Position;
            public Vector2 Origin;
            public float Rotation;
            public Vector2 Scale;
            public Color Color;
        }

        class Batch
        {
            public Texture Texture;
            public Quad[] Quads;
            public int QuadCount;

            public Batch(int capacity)
            {
                Quads = new Quad[capacity];
            }

            public void EnsureCapacity(int count)
            {
                if (Quads.Length < count)
                {
                    int newCapacity = Mathf.Max(Quads.Length * 2, count);
                    System.Array.Resize(ref Quads, newCapacity);
                }
            }

            public void Clear()
            {
                QuadCount = 0;
            }
        }

        Dictionary<Texture, Batch> batches = new Dictionary<Texture, Batch>();
        Batch[] batchArray;
        int batchCount;

        void Awake()
        {
            string shaderName = blendMode switch
            {
                BlendMode.Additive => "Sprites/Additive",
                BlendMode.AlphaBlend => "Sprites/Default",
                BlendMode.Opaque => "Sprites/Default",
                _ => "Sprites/Default"
            };

            var shader = Shader.Find(shaderName);
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            material = new Material(shader);

            batchArray = new Batch[initialBatchCapacity];

            if (renderCamera == null) renderCamera = Camera.main;
        }

        void OnEnable()
        {
            if (autoRender)
                StartCoroutine(RenderAtEndOfFrame());
        }

        IEnumerator RenderAtEndOfFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                RenderBatched();
            }
        }

        public void Draw(Texture2D texture, Vector2 position, Vector2 origin, float rotation, Vector2 scale, Color color)
        {
            if (texture == null) return;

            if (!batches.TryGetValue(texture, out var batch))
            {
                batch = new Batch(initialQuadCapacity);
                batch.Texture = texture;
                batches[texture] = batch;

                if (batchCount >= batchArray.Length)
                {
                    System.Array.Resize(ref batchArray, batchArray.Length * 2);
                }
                batchArray[batchCount++] = batch;
            }

            batch.EnsureCapacity(batch.QuadCount + 1);
            batch.Quads[batch.QuadCount++] = new Quad
            {
                Position = position,
                Origin = origin,
                Rotation = rotation,
                Scale = scale,
                Color = color
            };
        }

        public void Draw(Texture2D texture, Vector2 position, Vector2 origin, float rotation, float scale, Color color)
        {
            Draw(texture, position, origin, rotation, new Vector2(scale, scale), color);
        }

        public void Draw(Texture2D texture, Vector2 position, float rotation, Vector2 scale, Color color)
        {
            if (texture == null) return;
            Draw(texture, position, new Vector2(texture.width * 0.5f, texture.height * 0.5f), rotation, scale, color);
        }

        public void Draw(Texture2D texture, Vector2 position, float rotation, float scale, Color color)
        {
            Draw(texture, position, rotation, new Vector2(scale, scale), color);
        }

        public void Draw(Texture2D texture, Vector2 position, Color color)
        {
            Draw(texture, position, 0f, Vector2.one, color);
        }

        public void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float thickness = 2f)
        {
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length < 0.0001f) return;
            float angle = Mathf.Atan2(delta.y, delta.x);
            Draw(pixel, start, new Vector2(0f, 0.5f), angle, new Vector2(length, thickness), color);
        }

        public void RenderNow()
        {
            RenderBatched();
        }

        void RenderBatched()
        {
            if (batchCount == 0) return;

            GL.PushMatrix();

            if (coordinateSystem == CoordinateSystem.ScreenPixels || coordinateSystem == CoordinateSystem.Normalized)
            {
                GL.LoadOrtho();
            }
            else if (coordinateSystem == CoordinateSystem.World && renderCamera != null)
            {
                GL.LoadProjectionMatrix(renderCamera.projectionMatrix);
                GL.modelview = renderCamera.worldToCameraMatrix;
            }

            float invW = 1f / Screen.width;
            float invH = 1f / Screen.height;

            for (int b = 0; b < batchCount; b++)
            {
                var batch = batchArray[b];
                if (batch.QuadCount == 0) continue;

                material.mainTexture = batch.Texture;
                material.SetPass(0);

                GL.Begin(GL.QUADS);

                float texW = batch.Texture.width;
                float texH = batch.Texture.height;
                var quads = batch.Quads;

                for (int i = 0; i < batch.QuadCount; i++)
                {
                    var q = quads[i];
                    float cos = Mathf.Cos(q.Rotation);
                    float sin = Mathf.Sin(q.Rotation);

                    float w = texW * q.Scale.x;
                    float h = texH * q.Scale.y;
                    float ox = q.Origin.x * q.Scale.x;
                    float oy = q.Origin.y * q.Scale.y;

                    float x0 = -ox, y0 = -oy;
                    float x1 = w - ox, y1 = -oy;
                    float x2 = w - ox, y2 = h - oy;
                    float x3 = -ox, y3 = h - oy;

                    float sx0 = x0 * cos - y0 * sin + q.Position.x;
                    float sy0 = x0 * sin + y0 * cos + q.Position.y;
                    float sx1 = x1 * cos - y1 * sin + q.Position.x;
                    float sy1 = x1 * sin + y1 * cos + q.Position.y;
                    float sx2 = x2 * cos - y2 * sin + q.Position.x;
                    float sy2 = x2 * sin + y2 * cos + q.Position.y;
                    float sx3 = x3 * cos - y3 * sin + q.Position.x;
                    float sy3 = x3 * sin + y3 * cos + q.Position.y;

                    GL.Color(q.Color);

                    if (coordinateSystem == CoordinateSystem.ScreenPixels)
                    {
                        GL.TexCoord2(0f, 1f);
                        GL.Vertex3(sx0 * invW, 1f - sy0 * invH, 0f);
                        GL.TexCoord2(1f, 1f);
                        GL.Vertex3(sx1 * invW, 1f - sy1 * invH, 0f);
                        GL.TexCoord2(1f, 0f);
                        GL.Vertex3(sx2 * invW, 1f - sy2 * invH, 0f);
                        GL.TexCoord2(0f, 0f);
                        GL.Vertex3(sx3 * invW, 1f - sy3 * invH, 0f);
                    }
                    else if (coordinateSystem == CoordinateSystem.Normalized)
                    {
                        GL.TexCoord2(0f, 1f);
                        GL.Vertex3(sx0, 1f - sy0, 0f);
                        GL.TexCoord2(1f, 1f);
                        GL.Vertex3(sx1, 1f - sy1, 0f);
                        GL.TexCoord2(1f, 0f);
                        GL.Vertex3(sx2, 1f - sy2, 0f);
                        GL.TexCoord2(0f, 0f);
                        GL.Vertex3(sx3, 1f - sy3, 0f);
                    }
                    else
                    {
                        GL.TexCoord2(0f, 1f);
                        GL.Vertex3(sx0, sy0, 0f);
                        GL.TexCoord2(1f, 1f);
                        GL.Vertex3(sx1, sy1, 0f);
                        GL.TexCoord2(1f, 0f);
                        GL.Vertex3(sx2, sy2, 0f);
                        GL.TexCoord2(0f, 0f);
                        GL.Vertex3(sx3, sy3, 0f);
                    }
                }

                GL.End();
            }

            GL.PopMatrix();

            for (int i = 0; i < batchCount; i++)
                batchArray[i].Clear();
        }

        public int BatchCount => batchCount;

        public int TotalQuadCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < batchCount; i++)
                    count += batchArray[i].QuadCount;
                return count;
            }
        }

        void OnDestroy()
        {
            if (material != null)
                Destroy(material);
        }
    }
}
#endif

/*
## BatchRenderer 使用示范
#### 方式一：Inspector 配置
1. 创建空GameObject挂载BatchRenderer组件
2. 在 Inspector 中设置参数：
   - **BlendMode**: 混合模式（Additive/AlphaBlend/Opaque）
   - **CoordinateSystem**: 坐标系（ScreenPixels/World/Normalized）
   - **AutoRender**: 是否自动帧末渲染
3. 在代码中获取并使用：
public BatchRenderer batchRenderer;
void LateUpdate()
{
    batchRenderer.Draw(texture, position, rotation, scale, color);
}
// 新方式：无需手动建立GameObject和组件,直接通过静态方法Instance访问
void LateUpdate()
{
    BatchRenderer.Instance.Draw(texture, position, rotation, scale, color);
}
#### 方式二：纯代码创建
var go = new GameObject("BatchRenderer");
var renderer = go.AddComponent<BatchRenderer>();
renderer.blendMode = BlendMode.AlphaBlend;
renderer.autoRender = true;
### Draw 方法重载对照表
| 方法签名 | 参数说明 | 适用场景 |
|---------|---------|---------|
| `Draw(tex, pos, color)` | 位置 + 颜色 | 静态 UI 图标 |
| `Draw(tex, pos, rotation, scale, color)` | 位置 + 旋转 + 缩放 + 颜色 | 游戏实体渲染 |
| `Draw(tex, pos, rotation, Vector2 scale, color)` | 非均匀缩放 | 拉伸变形效果 |
| `Draw(tex, pos, origin, rotation, scale, color)` | 自定义原点 | 非中心旋转/锚点对齐 |
| `DrawLine(pixel, start, end, color, thickness)` | 起点 + 终点 + 粗细 | 画线、网格、边框 |
### 示例代码解读
// 1. 最简单的绘制：位置 + 颜色
batchRenderer.Draw(testSprite, center, Color.white);
// 2. 添加缩放
batchRenderer.Draw(testSprite, center + offset, 0.5f, Color.blue);
// 3. 添加旋转
batchRenderer.Draw(testSprite, pos, time * 2, Vector2.one, Color.white);
// 4. 非均匀缩放（压扁/拉伸）
batchRenderer.Draw(testSprite, pos, time * 3, new Vector2(1.2f, 0.5f), Color.cyan);
// 5. 自定义原点（右下角旋转）
Vector2 customOrigin = new Vector2(tex.width * 0.8f, tex.height * 0.2f);
batchRenderer.Draw(testSprite, pos, customOrigin, rotation, Vector2.one, Color.yellow);
// 6. 画线（粗细3像素）
batchRenderer.DrawLine(pixelTexture, start, end, Color.green, 3f);
### 常用场景示例
#### 场景一：2D 游戏实体渲染
void DrawEntity(Texture2D sprite, Vector2 position, float angle, float scale, Color tint)
{
    batchRenderer.Draw(sprite, position, angle, scale, tint);
}
// 渲染玩家
DrawEntity(playerSprite, playerPos, playerAngle, 1f, Color.white);
// 渲染受伤的敌人（红色闪烁）
DrawEntity(enemySprite, enemyPos, enemyAngle, 1f, Color.Lerp(Color.white, Color.red, flashPhase));
#### 场景二：粒子效果
void DrawExplosion(Vector2 center, int count)
{
    for (int i = 0; i < count; i++)
    {
        float angle = Random.Range(0, Mathf.PI * 2);
        float distance = Random.Range(10, 80);
        Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        
        float alpha = 1 - distance / 80f;
        batchRenderer.Draw(particleSprite, pos, angle, alpha * 0.8f, Color.yellow * alpha);
    }
}
#### 场景三：UI 元素
void DrawHealthBar(Vector2 position, float percent)
{
    // 背景
    batchRenderer.DrawLine(pixelTexture, position, position + new Vector2(100, 0), Color.gray, 20);
    
    // 血量条
    batchRenderer.DrawLine(pixelTexture, position, position + new Vector2(100 * percent, 0), Color.green, 16);
}
#### 场景四：网格/线条绘制
void DrawGrid(int cellSize)
{
    for (int x = 0; x < Screen.width; x += cellSize)
        batchRenderer.DrawLine(pixelTexture, new Vector2(x, 0), new Vector2(x, Screen.height), Color.black * 0.3f);
    
    for (int y = 0; y < Screen.height; y += cellSize)
        batchRenderer.DrawLine(pixelTexture, new Vector2(0, y), new Vector2(Screen.width, y), Color.black * 0.3f);
}
### 坐标系模式对比
| 模式 | 坐标范围 | Y轴方向 | 适用场景 |
|------|---------|--------|---------|
| **ScreenPixels** | (0,0)~(Screen.width, Screen.height) | 向下 | UI、HUD、像素级精确绘制 |
| **World** | 世界坐标 | 向上 | 游戏世界实体 |
| **Normalized** | 0~1 | 向下 | 百分比布局 |
// ScreenPixels 模式（默认）
batchRenderer.Draw(sprite, new Vector2(100, 200), Color.white);
// Normalized 模式（0~1 范围）
batchRenderer.coordinateSystem = CoordinateSystem.Normalized;
batchRenderer.Draw(sprite, new Vector2(0.5f, 0.5f), Color.white); // 屏幕中心
### 混合模式对比
| 模式 | 效果 | 适用场景 |
|------|------|---------|
| **Additive** | 颜色叠加，发光效果 | 霓虹灯、粒子、光效 |
| **AlphaBlend** | 透明度混合 | 普通精灵、UI |
| **Opaque** | 不透明覆盖 | 固体物体、背景 |
### 性能注意事项
1. **同纹理批处理**：相同纹理的绘制会自动合并为一次 GPU 调用，不同纹理会触发新的 DrawCall
2. **避免动态创建纹理**：每帧创建新 Texture2D 会产生 GC 分配，应预缓存
3. **推荐在 LateUpdate 中绘制**：确保所有变换更新完成后再入队
4. **减少批次数量**：尽量复用相同纹理，减少纹理切换
// 推荐：缓存纹理引用
Texture2D cachedSprite = Resources.Load<Texture2D>("Sprites/Player");
// 不推荐：每帧加载
// batchRenderer.Draw(Resources.Load<Texture2D>("Sprites/Player"), pos, color);
// 完整示范↓
public class BatchRendererExample : MonoBehaviour
{
    [Tooltip("目标BatchRenderer组件")]
    public BatchRenderer batchRenderer;

    [Tooltip("测试纹理")]
    public Texture2D testSprite;

    private Texture2D pixelTexture;
    private float time;

    void Awake()
    {
        if (batchRenderer == null)
            batchRenderer = GetComponent<BatchRenderer>();

        pixelTexture = new Texture2D(1, 1);
        pixelTexture.SetPixels(new[] { Color.white });
        pixelTexture.Apply();
    }

    void Update()
    {
        time += Time.deltaTime;
    }

    void LateUpdate()
    {
        if (batchRenderer == null) return;

        DemoBasicDraw();
        DemoTransformations();
        DemoLines();
        DemoParticleEffect();
        DemoAnimation();
    }

    void DemoBasicDraw()
    {
        if (testSprite == null) return;

        Vector2 center = new Vector2(Screen.width * 0.15f, Screen.height * 0.5f);

        batchRenderer.Draw(testSprite, center, Color.white);

        batchRenderer.Draw(testSprite, center + new Vector2(60, 0), 0.5f, Color.blue);

        batchRenderer.Draw(testSprite, center + new Vector2(120, 0), 1.5f, Color.red);
    }

    void DemoTransformations()
    {
        if (testSprite == null) return;

        Vector2 center = new Vector2(Screen.width * 0.35f, Screen.height * 0.5f);

        batchRenderer.Draw(testSprite, center, time * 2, Vector2.one, Color.white);

        batchRenderer.Draw(testSprite, center + new Vector2(80, 0), time * 3, new Vector2(1.2f, 0.5f), Color.cyan);

        Vector2 customOrigin = new Vector2(testSprite.width * 0.8f, testSprite.height * 0.2f);
        batchRenderer.Draw(testSprite, center + new Vector2(160, 0), customOrigin, time * 4, Vector2.one, Color.yellow);
    }

    void DemoLines()
    {
        Vector2 center = new Vector2(Screen.width * 0.55f, Screen.height * 0.5f);

        float radius = 50;
        int segments = 12;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * Mathf.PI * 2 / segments;
            float angle2 = (i + 1) * Mathf.PI * 2 / segments;

            Vector2 p1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 p2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

            Color color = Color.Lerp(Color.green, Color.yellow, i / (float)segments);
            batchRenderer.DrawLine(pixelTexture, p1, p2, color, 3f);
        }

        Vector2 start = center + new Vector2(-60, 80);
        Vector2 end = center + new Vector2(60, 80);
        batchRenderer.DrawLine(pixelTexture, start, end, Color.magenta, 8f);

        start = center + new Vector2(-40, -80);
        end = center + new Vector2(40, -80);
        batchRenderer.DrawLine(pixelTexture, start, end, Color.white, 1f);
    }

    void DemoParticleEffect()
    {
        Vector2 center = new Vector2(Screen.width * 0.75f, Screen.height * 0.5f);
        int particleCount = 20;

        for (int i = 0; i < particleCount; i++)
        {
            float angle = i * Mathf.PI * 2 / particleCount + time * 3;
            float distance = 40 + Mathf.Sin(time * 5 + i) * 20;

            Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

            float scale = 0.5f + Mathf.Sin(time * 10 + i) * 0.3f;
            float alpha = 0.6f + Mathf.Sin(time * 8 + i) * 0.4f;

            Color color = new Color(1, 0.5f, 0, alpha);

            if (testSprite != null)
                batchRenderer.Draw(testSprite, pos, angle, scale, color);
        }
    }

    void DemoAnimation()
    {
        if (testSprite == null) return;

        Vector2 center = new Vector2(Screen.width * 0.9f, Screen.height * 0.5f);

        float bounce = Mathf.Sin(time * 3) * 20;
        float pulse = 1 + Mathf.Sin(time * 5) * 0.3f;

        Vector2 pos = center + new Vector2(0, bounce);

        batchRenderer.Draw(testSprite, pos, time * 2, pulse, Color.white);

        batchRenderer.Draw(testSprite, pos + new Vector2(-30, 0), time * -2, pulse * 0.6f, Color.gray);
        batchRenderer.Draw(testSprite, pos + new Vector2(30, 0), time * -2, pulse * 0.6f, Color.gray);
    }
}
 */