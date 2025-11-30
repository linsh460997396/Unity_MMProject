using UnityEngine;

namespace SpriteSpace
{
    /// <summary>
    /// 玩家技能1号.
    /// </summary>
    public class PlayerSkill1 : PlayerSkill
    {
        /// <summary>
        /// 最大可穿透次数
        /// </summary>
        public int pierceCount = 1;
        /// <summary>
        /// 穿透时间间隔帧数(针对相同目标)
        /// </summary>
        public int pierceDelay = 12;
        /// <summary>
        /// 击退强度(影响退多少帧、多远)
        /// </summary>
        public int knockbackForce = 6;

        /// <summary>
        /// [构造函数]MM测试专用玩家技能
        /// </summary>
        /// <param name="stage_">舞台</param>
        public PlayerSkill1(Stage stage_) : base(stage_) { }

        /// <summary>
        /// 初始化
        /// </summary>
        public new void Init()
        {//new关键字表示新方法并隐藏基类中的同名方法
            castDelay = (int)(Scene.fps * 0.2f);
            castCount = 3;
        }

        //若有一个基类也定义了一个名为Init的方法,那么在派生类中使用“new”关键字定义的Init方法将会隐藏基类中的那个方法

        /// <summary>
        /// 更新
        /// </summary>
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

                var row = player.pixelRow;
                var column = player.pixelColumn;
                var shootDistanceStep = maxShootDistance / castCount;
                var count = castCount;
                var sc = stage.monstersGridContainer;
                var os = sc.result_FindNearestN;
                var n = sc.FindNearestNByRange(Scene.spaceRDD, row, column, moveSpeed * life, count);

                if (n > 0)
                {
                    do
                    {
                        for (int i = 0; i < n; ++i)
                        {
                            var o = os[i].item;
                            var dColumn = o.pixelColumn - column;
                            var dRow = o.pixelRow - row;
                            var r = Mathf.Atan2(dColumn, dRow);
                            var cos = Mathf.Cos(r);
                            var sin = Mathf.Sin(r);
                            var tarRow = row + cos * shootDistanceStep * count;
                            var tarColumn = column + sin * shootDistanceStep * count;
                            new PlayerBullet1(this).Init(tarRow, tarColumn, r, cos, sin);
                            //Debug.Log($"PlayerSkill1 Shoot To Monster {o.id} At ({tarRow}, {tarColumn}) With Angle {r} Rad");
                            --count;
                            if (count == 0) break;
                        }
                    } while (count > 0);
                }
                else
                {
                    var d = scene.PlayerDirection;
                    var r = Mathf.Atan2(d.y, d.x);
                    var cos = Mathf.Cos(r);
                    var sin = Mathf.Sin(r);
                    for (; count > 0; --count)
                    {
                        var tarRow = row + cos * shootDistanceStep * count;
                        var tarColumn = column + sin * shootDistanceStep * count;
                        new PlayerBullet1(this).Init(tarRow, tarColumn, r, cos, sin);
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