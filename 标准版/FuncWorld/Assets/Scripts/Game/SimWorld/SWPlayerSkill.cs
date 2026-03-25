using UnityEngine;

namespace SimWorld
{
    public class SWPlayerSkill
    {
        // 快捷指向
        public Main_SimWorld scene;
        public SWStage stage;
        public SWPlayer player;

        public int icon = 123;                                  // 用于 UI 展示
        public float progress = 1;                              // 同步以展示技能就绪进度( 结合 nextCastTime castDelay 来计算 )

        public const float maxShootDistance = 50;               // 子弹发射时与身体的 最大距离
        public int castDelay = (int)(Main_SimWorld.fps * 0.02f);        // 技能冷却时长       // todo: 改为每秒匀速发射多少颗的逻辑
        public int castCount = 1;                               // 一次发射多少颗
        public int nextCastTime = 0;                            // 下一次施展的时间点

        // 创建子弹时,复制到子弹上
        public float radius = 20;                               // 碰撞检测半径( 和显示放大修正配套 )
        public int damage = 1;                                  // 伤害( 倍率 )
        public float moveSpeed = 25;                            // 按照 tps 来算的每一帧的移动距离
        public int life = Main_SimWorld.fps * 1;                        // 子弹存在时长( 帧 ): tps * 秒

        public SWPlayerSkill(SWStage stage_)
        {
            stage = stage_;
            scene = stage_.scene;
            player = scene.player;
        }

        public void Init()
        {
        }

        public virtual void Update()
        {
            var time = scene.time;
            if (nextCastTime < time)
            {
                nextCastTime = time + castDelay;
                progress = 0;

                // 子弹发射逻辑
                // 找射程内 距离最近的 1 只 朝向其发射 1 子弹

                var x = player.x;
                var y = player.y;
                var o = stage.monstersSpaceContainer.FindNearestByRange(Main_SimWorld.spaceRDD, x, y, moveSpeed * life);
                if (o != null)
                {
                    var dy = o.y - y;
                    var dx = o.x - x;
                    var r = Mathf.Atan2(dy, dx);
                    var cos = Mathf.Cos(r);
                    var sin = Mathf.Sin(r);
                    var tarX = x + cos * maxShootDistance;
                    var tarY = y + sin * maxShootDistance;
                    new SWPlayerBullet(this).Init(tarX, tarY, r, cos, sin);
                }

            }
            else
            {
                progress = 1 - (nextCastTime - time) / castDelay;
            }
        }

        public virtual void Destroy()
        {
        }
    }
}


/*
    var sb = new StringBuilder();
    sb.Append($"n = {n} [ ");
    for (int i = 0; i < n; ++i) {
        var d = os[i].distance;
        sb.Append($"{d}, ");
    }
    sb.Append("]");
    Debug.Log(sb.ToString());
*/
