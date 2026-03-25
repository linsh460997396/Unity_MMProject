using UnityEngine;

namespace SimWorld
{
    public class SWPlayerSkill1 : SWPlayerSkill
    {

        public int pierceCount = 5;                             // 最大可穿透次数
        public int pierceDelay = 12;                            // 穿透时间间隔 帧数( 针对相同目标 )
        public int knockbackForce = 0;                          // 击退强度( 退多少帧, 多远 )

        public SWPlayerSkill1(SWStage stage_) : base(stage_)
        {
        }

        public new void Init()
        {
            castDelay = (int)(Main_SimWorld.fps * 0.2f);
            castCount = 3;
        }

        public override void Update()
        {
            var time = scene.time;
            if (nextCastTime < time)
            {
                nextCastTime = time + castDelay;
                progress = 0;

                // 子弹发射逻辑
                // 找射程内 距离最近的 最多 castCount 只 分别朝向其发射子弹
                // 若不足 castCount 只,轮流扫射,直到用光 castCount 发
                // 0 只 就面对朝向发射
                // 发射时和 player 保持一个距离,同时随着 count 的减少,距离也变短,以解决同一帧内同一角度发射多粒 完全重叠在一起看不出来的问题

                var x = player.x;
                var y = player.y;
                var shootDistanceStep = maxShootDistance / castCount;
                var count = castCount;
                var sc = stage.monstersSpaceContainer;
                var os = sc.result_FindNearestN;
                var n = sc.FindNearestNByRange(Main_SimWorld.spaceRDD, x, y, moveSpeed * life, count);

                if (n > 0)
                {
                    do
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            var o = os[i].item;
                            var dy = o.y - y;
                            var dx = o.x - x;
                            var r = Mathf.Atan2(dy, dx);
                            var cos = Mathf.Cos(r);
                            var sin = Mathf.Sin(r);
                            var tarX = x + cos * shootDistanceStep * count;
                            var tarY = y + sin * shootDistanceStep * count;
                            new SWPlayerBullet1(this).Init(tarX, tarY, r, cos, sin);
                            --count;
                            if (count == 0) break;
                        }
                    } while (count > 0);
                }
                else
                {
                    var d = scene.playerDirection;
                    var r = Mathf.Atan2(d.y, d.x);
                    var cos = Mathf.Cos(r);
                    var sin = Mathf.Sin(r);
                    for (; count > 0; --count)
                    {
                        var tarX = x + cos * shootDistanceStep * count;
                        var tarY = y + sin * shootDistanceStep * count;
                        new SWPlayerBullet1(this).Init(tarX, tarY, r, cos, sin);
                    }
                }
            }
            else
            {
                progress = 1 - (nextCastTime - time) / castDelay;
            }
        }

    }
}