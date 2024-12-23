//using UnityEngine;

//namespace MMWorld.Test
//{

//    /// <summary>
//    /// 玩家技能1
//    /// </summary>
//    public class TestPlayerSkill1 : PlayerSkill
//    {
//        /// <summary>
//        /// 最大可穿透次数
//        /// </summary>
//        public int pierceCount = 5;
//        /// <summary>
//        /// 穿透时间间隔帧数(针对相同目标)
//        /// </summary>
//        public int pierceDelay = 12;
//        /// <summary>
//        /// 击退强度(影响退多少帧、多远)
//        /// </summary>
//        public int knockbackForce = 0;

//        /// <summary>
//        /// [构造函数]玩家技能1
//        /// </summary>
//        /// <param name="stage_">关卡</param>
//        public TestPlayerSkill1(Stage stage_) : base(stage_) { }

//        /// <summary>
//        /// 初始化
//        /// </summary>
//        public new void Init()
//        {//new关键字表示新方法并隐藏基类中的同名方法
//            castDelay = (int)(Scene.fps * 0.2f);
//            castCount = 3;
//        }

//        //如果有一个基类也定义了一个名为Init的方法，那么在派生类中使用“new”关键字定义的Init方法将会隐藏基类中的那个方法

//        /// <summary>
//        /// 更新
//        /// </summary>
//        public override void Update()
//        {
//            var time = scene.time;
//            if (nextCastTime < time)
//            {
//                nextCastTime = time + castDelay;
//                progress = 0;

//                // 子弹发射逻辑
//                // 找射程内 距离最近的 最多 castCount 只 分别朝向其发射子弹
//                // 如果不足 castCount 只，轮流扫射，直到用光 castCount 发
//                // 0 只 就面对朝向发射
//                // 发射时和 player 保持一个距离，同时随着 count 的减少，距离也变短，以解决同一帧内同一角度发射多粒 完全重叠在一起看不出来的问题

//                var x = player.pixelX;
//                var y = player.pixelY;
//                var shootDistanceStep = maxShootDistance / castCount;
//                var count = castCount;
//                var sc = stage.monstersSpaceContainer;
//                var os = sc.result_FindNearestN;
//                var n = sc.FindNearestNByRange(Scene.spaceRDD, x, y, moveSpeed * life, count);

//                if (n > 0)
//                {
//                    do
//                    {
//                        for (int i = 0; i < n; ++i)
//                        {
//                            var o = os[i].item;
//                            var dy = o.pixelY - y;
//                            var dx = o.pixelX - x;
//                            var r = Mathf.Atan2(dy, dx);
//                            var cos = Mathf.Cos(r);
//                            var sin = Mathf.Sin(r);
//                            var tarX = x + cos * shootDistanceStep * count;
//                            var tarY = y + sin * shootDistanceStep * count;
//                            new TestPlayerBullet1(this).Init(tarX, tarY, r, cos, sin);
//                            --count;
//                            if (count == 0) break;
//                        }
//                    } while (count > 0);
//                }
//                else
//                {
//                    var d = scene.playerDirection;
//                    var r = Mathf.Atan2(d.y, d.x);
//                    var cos = Mathf.Cos(r);
//                    var sin = Mathf.Sin(r);
//                    for (; count > 0; --count)
//                    {
//                        var tarX = x + cos * shootDistanceStep * count;
//                        var tarY = y + sin * shootDistanceStep * count;
//                        new TestPlayerBullet1(this).Init(tarX, tarY, r, cos, sin);
//                    }
//                }
//            }
//            else
//            {
//                progress = 1 - (nextCastTime - time) / castDelay;
//            }
//        }

//    }
//}