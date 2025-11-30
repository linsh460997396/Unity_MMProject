using System.Collections.Generic;

namespace SimWorld
{
    public class SWPlayerBullet1 : SWPlayerBullet
    {

        public List<KeyValuePair<SWGridItem, int>> hitBlackList = new();   // 带超时的穿透黑名单

        // 这些属性从 skill copy
        public int pierceCount;                         // 最大可穿透次数
        public int pierceDelay;                         // 穿透时间间隔 帧数( 针对相同目标 )
        public int knockbackForce;                      // 击退强度( 退多少帧, 多远 )

        public SWPlayerBullet1(SWPlayerSkill1 ps) : base(ps)
        {
            pierceCount = ps.pierceCount;
            pierceDelay = ps.pierceDelay;
            knockbackForce = ps.knockbackForce;
        }

        public override bool Update()
        {

            // 维护 超时黑名单. 这步先把超时的删光
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
                // 在 9 宫范围内查询 首个相交
                var m = monstersSpaceContainer.FindFirstCrossBy9(x, y, radius);
                if (m != null)
                {
                    ((SWMonster)m).Hurt(damage, knockbackForce);
                    return true;    // 和怪一起死
                }
            }
            else
            {
                // 遍历九宫 挨个处理相交, 消耗 穿刺
                monstersSpaceContainer.Foreach9All(x, y, HitCheck);
                if (pierceCount <= 0) return true;
            }

            // 让子弹直线移动
            x += incX;
            y += incY;

            // 坐标超出 grid地图 范围: 自杀
            if (x < 0 || x >= Main_SimWorld.gridWidth || y < 0 || y >= Main_SimWorld.gridHeight) return true;

            // 生命周期完结: 自杀
            return lifeEndTime < scene.time;
        }

        public bool HitCheck(SWGridItem m)
        {
            var vx = m.x - x;
            var vy = m.y - y;
            var r = m.radius + radius;
            if (vx * vx + vy * vy < r * r)
            {

                // 判断当前怪有没有存在于 超时黑名单
                var listLen = hitBlackList.Count;
                for (var i = 0; i < listLen; ++i)
                {
                    if (hitBlackList[i].Key == m) return false;     // 存在: 不产生伤害, 继续遍历下一只怪
                }

                // 不存在:加入列表
                hitBlackList.Add(new KeyValuePair<SWGridItem, int>(m, scene.time + pierceDelay));

                // 伤害怪
                ((SWMonster)m).Hurt(damage, knockbackForce);

                // 若穿刺计数 已用光,停止遍历
                if (pierceCount-- == 0)
                {
                    // 放点特效?
                    return true;
                }
            }
            // 未命中:继续遍历下一只怪
            return false;
        }

    }
}