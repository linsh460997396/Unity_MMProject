using System.Collections.Generic;
using UnityEngine;

namespace SimWorld
{
    public class SWMonster : SWSpaceItem
    {
        // 各种指向
        public Main_SimWorld scene;                                 // 指向场景
        public SWStage stage;                                 // 指向关卡
        public SWPlayer player;                               // 指向玩家
        public List<SWMonster> monsters;                      // 指向关卡怪数组
        public int indexOfContainer;                        // 自己位于关卡怪数组的下标
        public Sprite[] sprites;                            // 指向动画帧集合
        public GO go, mgo;                                  // 保存底层 u3d 资源. mgo: mini map 用到的那份

        public const float defaultMoveSpeed = 20;           // 原始移动速度
        public const float _1_defaultMoveSpeed = 1f / defaultMoveSpeed; // 倒数, 转除法为乘法
        public const float frameAnimIncrease = 1f / 5;      // 帧动画前进速度( 针对 defaultMoveSpeed )
        public const float displayBaseScale = 1f;           // 显示放大修正
        public const float defaultRadius = 1f;             // 原始半径(数字越大则实际采用越小)
        public const float _1_defaultRadius = 1f / defaultRadius;

        public float frameIndex = 0;                        // 当前动画帧下标
        public bool flipX;                                  // 根据移动方向判断要不要反转 pixelX 显示
        public float radians;                               // 最后移动朝向

        public float tarOffsetX, tarOffsetY;                // 追赶玩家时的目标坐标偏移量( 防止放风筝时重叠到一起 )
        public int aimDelay = 29;                           // 瞄准玩家的延迟( 0 ~ 59 )( 获取玩家坐标走 history, 这是下标 )
        public int damage = 10;                             // 怪伤害值
        public int hp = 20;                                 // 怪血量
        public float moveSpeed = 5;                         // 每一帧的移动距离

        public int knockbackEndTime;                        // 被击退结束时间点
        public float knockbackDecay = 0.01f;                // 被击退移动增量衰减值
        public float knockbackIncRate = 1;                  // 被击退移动增量倍率 每帧 -= decay
        public float knockbackIncX, knockbackIncY;          // 被击退移动增量 实际 pixelX += inc * rate

        public int whiteEndTime;                            // 变白结束时间( 受伤会变白 )
        public const int whiteDelay = 10;                   // 受伤变白的时长. 反复受伤就会重置变白结束时间

        // todo: 血量,显示伤害文字支持

        protected SWMonster(SWStage stage_)
        {
            spaceContainer = stage_.monstersSpaceContainer;
            stage = stage_;
            player = stage_.player;
            scene = stage_.scene;
            monsters = stage_.monsters;

            indexOfContainer = monsters.Count;
            monsters.Add(this);

            GO.Pop(ref go);

            GO.Pop(ref mgo, 3);
            mgo.spriteRenderer.material = scene.minimap_material;
            mgo.transform.localScale = new Vector3(4, 4, 4);
            //mgo.spriteRenderer.color = Color.red;
        }

        public void Init(Sprite[] sprites_, float x_, float y_)
        {
            sprites = sprites_;
            mgo.spriteRenderer.sprite = sprites[0];
            flipX = x_ >= player.x;
            x = x_;
            y = y_;
            radius = defaultRadius;
            spaceContainer.Add(this);   // 放入空间索引容器
            ResetTargetOffsetXY();
        }

        // 返回 true 表示 怪需要自杀( 自爆 消散 啥的? ). 派生类需要处理击退: if (knockbackEndTime >= scene.time) return base.Update();
        // 追赶并贴身直接伤害玩家
        public virtual bool Update()
        {
            if (knockbackEndTime >= scene.time)
            {  // 被击退中
                x += knockbackIncX * knockbackIncRate;   // 位移
                y += knockbackIncY * knockbackIncRate;
                knockbackIncRate -= knockbackDecay;  // 衰减

                spaceContainer.Update(this);    // 更新在空间索引容器中的位置
                return false;
            }

            // 判断是否已接触到 玩家. 接触到就造成伤害, 没接触到就继续移动
            var dx = player.x - x;
            var dy = player.y - y;
            var dd = dx * dx + dy * dy;
            var r2 = player.radius + radius;
            if (dd < r2 * r2)
            {
                player.Hurt(damage);
            }
            else
            {
                // 判断是否已到达 偏移点. 已到达: 重新选择偏移点. 未到达: 移动
                var ph = player.positionHistory[aimDelay];     // 获取玩家历史坐标来追击 以显得怪笨
                dx = ph.x - x + tarOffsetX;
                dy = ph.y - y + tarOffsetY;
                dd = dx * dx + dy * dy;
                if (dd < radius * radius)
                {
                    ResetTargetOffsetXY();
                }
                // 计算移动方向并增量
                radians = Mathf.Atan2(dy, dx);
                var cos = Mathf.Cos(radians);
                x += cos * moveSpeed;
                y += Mathf.Sin(radians) * moveSpeed;
                flipX = cos < 0;
            }

            // todo: 不能太边缘,需要留一段安全距离,一是屏幕边缘生成,二是击退,都要留出余量
            // 若玩家快速移动导致怪被甩在后面很远,可以将怪 "挪" 到玩家前方去. 或许直接重新随机坐标位置会比较科学
            // 强行限制移动范围
            if (x < 0) x = 0;
            else if (x >= Main_SimWorld.gridWidth) x = Main_SimWorld.gridWidth - float.Epsilon;
            if (y < 0) y = 0;
            else if (y >= Main_SimWorld.gridHeight) y = Main_SimWorld.gridHeight - float.Epsilon;

            // 根据移动速度步进动画帧下表
            frameIndex += frameAnimIncrease * moveSpeed * _1_defaultMoveSpeed;
            var len = sprites.Length;
            if (frameIndex >= len)
            {
                frameIndex -= len;
            }

            spaceContainer.Update(this);    // 更新在空间索引容器中的位置
            return false;
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

                // 同步帧下标
                go.spriteRenderer.sprite = sprites[(int)frameIndex];

                // 同步 & 坐标系转换( pixelY 坐标需要反转 )
                go.transform.position = new Vector3(x * Main_SimWorld.designWidthToCameraRatio, -y * Main_SimWorld.designWidthToCameraRatio, 0);

                // 同步尺寸缩放( 根据半径推送算 )
                var s = displayBaseScale * radius * _1_defaultRadius;
                go.transform.localScale = new Vector3(s, s, s);

                // 看情况变色
                if (scene.time >= whiteEndTime)
                {
                    go.SetColorDefault();
                }
                else
                {
                    go.SetColorWhite();
                }
            }

            if (x < cx - Main_SimWorld.designWidth * 2
                || x > cx + Main_SimWorld.designWidth * 2
                || y < cy - Main_SimWorld.designHeight * 2
                || y > cy + Main_SimWorld.designHeight * 2)
            {
                mgo.Disable();
            }
            else
            {
                mgo.Enable();
                mgo.transform.position = new Vector3(x * Main_SimWorld.designWidthToCameraRatio, -y * Main_SimWorld.designWidthToCameraRatio, 0);
            }
        }

        public virtual void DrawGizmos()
        {
            Gizmos.DrawWireSphere(new Vector3(x * Main_SimWorld.designWidthToCameraRatio, -y * Main_SimWorld.designWidthToCameraRatio, 0), radius * Main_SimWorld.designWidthToCameraRatio);
        }

        public virtual void Destroy(bool needRemoveFromContainer = true)
        {
#if UNITY_EDITOR
            if (go.gameObject != null)
#endif
            {
                GO.Push(ref go);
            }
#if UNITY_EDITOR
            if (mgo.gameObject != null)
#endif
            {
                GO.Push(ref mgo);
            }

            // 从空间索引容器移除
            spaceContainer.Remove(this);

            // 从 stage 容器交换删除
            if (needRemoveFromContainer)
            {
                var ms = stage.monsters;
                var lastIndex = ms.Count - 1;
                var last = ms[lastIndex];
                last.indexOfContainer = indexOfContainer;
                ms[indexOfContainer] = last;
                ms.RemoveAt(lastIndex);
            }
        }

        // 重置偏移
        void ResetTargetOffsetXY()
        {
            var p = stage.GetRndPosDoughnut(player.radius * 10, 0.1f);
            tarOffsetX = p.x;
            tarOffsetY = p.y;
        }

        // 令怪受伤, 播特效. 返回怪是否 已死亡. 已死亡将从数组移除该怪( !!! 重要 : 需位于 倒循环 for 内 )
        public bool Hurt(int playerBulletDamage, int knockbackForce)
        {

            // 结合暴击算最终伤害值
            var d = playerBulletDamage * player.damage;
            var b = Random.value <= player.criticalRate;
            if (b)
            {
                d = (int)(d * player.criticalDamageRatio);
            }

            if (hp <= d)
            {
                // 怪被打死: 删, 播特效
                new SWEffectExplosion(stage, x, y, radius * _1_defaultRadius);
                new SWEffectNumber(stage, x, y, 0.5f, hp, b);
                Destroy();
                return true;
            }
            else
            {
                // 怪没死: 播飙血特效( todo )
                hp -= d;
                new SWEffectNumber(stage, x, y, 0.5f, d, b);

                // 击退?
                if (knockbackForce > 0)
                {
                    knockbackEndTime = scene.time + knockbackForce;
                    knockbackIncRate = 1;
                    knockbackDecay = 1 / knockbackForce;
                    knockbackIncX = -Mathf.Cos(radians) * moveSpeed;
                    knockbackIncY = -Mathf.Sin(radians) * moveSpeed;
                }

                // 变白一会儿
                whiteEndTime = scene.time + whiteDelay;

                return false;

            }

        }
    }
}