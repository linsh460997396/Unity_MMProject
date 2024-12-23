using System.Collections.Generic;

namespace MMWorld.Test
{

    /// <summary>
    /// MM测试专用玩家子弹（发射物）
    /// </summary>
    public class MMTestPlayerBullet : PlayerBullet
    {
        /// <summary>
        /// 超时的穿透黑名单
        /// </summary>
        public List<KeyValuePair<SpaceItem, int>> hitBlackList = new();

        //这些属性从 skill copy

        /// <summary>
        /// 最大可穿透次数
        /// </summary>
        public int pierceCount;
        /// <summary>
        /// 穿透时间间隔帧数(针对相同目标)
        /// </summary>
        public int pierceDelay;
        /// <summary>
        /// 击退强度(影响退多少帧、多远)
        /// </summary>
        public int knockbackForce;

        /// <summary>
        /// [构造函数]MM测试专用玩家子弹（发射物）
        /// </summary>
        /// <param name="ps">MMTestPlayerSkill对象</param>
        public MMTestPlayerBullet(MMTestPlayerSkill ps) : base(ps)
        {
            pierceCount = ps.pierceCount;
            pierceDelay = ps.pierceDelay;
            knockbackForce = ps.knockbackForce;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public override bool Update()
        {

            // 维护超时黑名单，先把超时的删光
            var now = scene.time;
            var newTIme = now + pierceDelay;
            for (var i = hitBlackList.Count - 1; i >= 0; --i)
            {
                if (hitBlackList[i].Value < now)
                {
                    var lastIndex = hitBlackList.Count - 1;
                    hitBlackList[i] = hitBlackList[lastIndex];
                    hitBlackList.RemoveAt(lastIndex);
                }
            }

            if (pierceCount <= 1)
            {
                //在9宫范围内查询首个相交
                var m = monstersSpaceContainer.FindFirstCrossBy9(pixelX, pixelY, radius);
                if (m != null)
                {
                    ((Monster)m).Hurt(damage, knockbackForce);
                    return true;    //和怪一起死
                }
            }
            else
            {
                //遍历九宫挨个处理相交，消耗穿刺数量
                monstersSpaceContainer.Foreach9All(pixelX, pixelY, HitCheck);
                if (pierceCount <= 0) return true;
            }

            //让子弹直线移动
            pixelX += incX;
            pixelY += incY;

            //坐标超出grid地图范围：自杀
            if (pixelX < 0 || pixelX >= Scene.gridChunkWidth || pixelY < 0 || pixelY >= Scene.gridChunkHeight) return true;

            //生命周期完结：自杀
            return lifeEndTime < scene.time;
        }

        /// <summary>
        /// 碰撞检查
        /// </summary>
        /// <param name="m">空间物体</param>
        /// <returns></returns>
        public bool HitCheck(SpaceItem m)
        {
            var vx = m.pixelX - pixelX;
            var vy = m.pixelY - pixelY;
            var r = m.radius + radius;
            if (vx * vx + vy * vy < r * r)
            {
                //判断当前怪有没有存在于超时黑名单
                var listLen = hitBlackList.Count;
                for (var i = 0; i < listLen; ++i)
                {
                    if (hitBlackList[i].Key == m) return false;     //存在：不产生伤害，继续遍历下一只怪
                }

                //不存在：加入列表
                hitBlackList.Add(new KeyValuePair<SpaceItem, int>(m, scene.time + pierceDelay));

                //伤害怪
                ((Monster)m).Hurt(damage, knockbackForce);

                //如果穿刺计数已用光，停止遍历
                if (pierceCount-- == 0)
                {
                    //todo：这儿可放点特效
                    return true;
                }
            }
            //未命中：继续遍历下一只怪
            return false;
        }

    }
}