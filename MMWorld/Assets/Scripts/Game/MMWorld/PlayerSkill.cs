using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 玩家技能
    /// </summary>
    public class PlayerSkill
    {
        //快捷指向

        /// <summary>
        /// 场景
        /// </summary>
        public Scene scene;
        /// <summary>
        /// 关卡
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 玩家
        /// </summary>
        public Player player;
        /// <summary>
        /// 用于UI展示
        /// </summary>
        public int icon = 123;
        /// <summary>
        /// 同步以展示技能就绪进度(结合nextCastTime、castDelay来计算)
        /// </summary>
        public float progress = 1;
        /// <summary>
        /// [逻辑坐标]子弹发射时与身体的最大距离（默认是Scene.gridSize/2）
        /// </summary>
        public const float maxShootDistance = Scene.gridSize / 2;
        /// <summary>
        /// 技能冷却时长
        /// </summary>
        public int castDelay = (int)(Scene.fps * 0.02f);        // todo: 改为每秒匀速发射多少颗的逻辑
        /// <summary>
        /// 一次发射多少颗
        /// </summary>
        public int castCount = 1;
        /// <summary>
        /// 下一次施展的时间点（单位：帧）
        /// </summary>
        public int nextCastTime = 0;

        //下面是创建子弹时需要复制到子弹上的属性

        /// <summary>
        /// [逻辑坐标]碰撞检测半径(和显示放大修正配套)
        /// </summary>
        public float radius = 20;
        /// <summary>
        /// 伤害(倍率)
        /// </summary>
        public int damage = 1;
        /// <summary>
        /// [逻辑坐标]按照fps来算的每一帧的移动距离
        /// </summary>
        public float moveSpeed = 25;
        /// <summary>
        /// 子弹存在时长(帧)：fps * 秒
        /// </summary>
        public int life = Scene.fps * 1;

        /// <summary>
        /// [构造函数]玩家技能
        /// </summary>
        /// <param name="stage_">关卡</param>
        public PlayerSkill(Stage stage_)
        {
            stage = stage_;
            scene = stage_.scene;
            player = scene.player;
        }

        /// <summary>
        /// 初始化玩家技能
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// [虚方法]更新
        /// </summary>
        public virtual void Update()
        {
            var time = scene.time;
            if (nextCastTime < time)
            {
                nextCastTime = time + castDelay;
                progress = 0;

                // 子弹发射逻辑
                // 找射程内 距离最近的 1 只 朝向其发射 1 子弹

                var x = player.pixelX;
                var y = player.pixelY;
                var o = stage.monstersSpaceContainer.FindNearestByRange(Scene.spaceRDD, x, y, moveSpeed * life);
                if (o != null)
                {
                    var dy = o.pixelY - y;
                    var dx = o.pixelX - x;
                    var r = Mathf.Atan2(dy, dx);
                    var cos = Mathf.Cos(r);
                    var sin = Mathf.Sin(r);
                    var tarX = x + cos * maxShootDistance;
                    var tarY = y + sin * maxShootDistance;
                    new PlayerBullet(this).Init(tarX, tarY, r, cos, sin);
                }

            }
            else
            {
                progress = 1 - (nextCastTime - time) / castDelay;
            }
        }

        /// <summary>
        /// [虚方法]摧毁
        /// </summary>
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
