using UnityEngine;

namespace SimWorld
{
    public class SWStage1 : SWStage
    {
        public int timeout;
        public System.Random random = new();

        public SWStage1(Main_SimWorld scene) : base(scene)
        {
            // 这里可判断是不是 切关, 然后对 player 或啥的做相应处理

            // 关闭小地图
            scene.EnableMinimap(false);
        }

        public override void Update()
        {
            switch (state)
            {
                case 0:
                    P0();
                    return;
                case 1:
                    P1();
                    return;
                default:
                    throw new System.Exception("can'transform be here");
            }
        }

        public void P0()
        {
            var cx = Main_SimWorld.gridCenterX;
            var cy = Main_SimWorld.gridCenterY;

            // 重置 SWPlayer 坐标
            player.Init(this, cx, cy);

            //// 验证一下这个表的数据是否正确
            //var d = Main_SimWorld.spaceRDD;
            //foreach (var i in d.idxys) {
            //    // 根据 格子 offset 计算 pixelX, pixelY 并设置怪的坐标
            //    new SWMonster1(this).Init(cx + i.pixelX * 16, cy + i.pixelY * 16).radius = 5;
            //}

            state = 1;
        }

        public void P1()
        {
            Update_Effect_Numbers();
            Update_Player();

            // 测试一下伤害数字的效果
            for (int i = 0; i < 50; i++)
            {
                var x = Random.Range(-Main_SimWorld.designWidth_2, Main_SimWorld.designWidth_2);
                var y = Random.Range(-Main_SimWorld.designHeight_2, Main_SimWorld.designHeight_2);
                //var v = Random.Range(0, 1000000000) * System.Math.Pow(10, Random.Range(1, 20 - 10));   // 307 - 10
                var v = random.NextDouble() * System.Math.Pow(10, Random.Range(2, 30 - 10));
                new SWEffectNumber(this, player.x + x, player.y + y, 0.5f, v, Random.value > 0.5f);
            }
        }

    }
}