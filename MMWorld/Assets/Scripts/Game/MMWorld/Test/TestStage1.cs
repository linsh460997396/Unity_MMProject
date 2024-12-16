//using UnityEngine;

//namespace MMWorld.Test
//{

//    /// <summary>
//    /// 测试关卡1
//    /// </summary>
//    public class TestStage1 : Stage
//    {
//        public int timeout;
//        public System.Random random = new();

//        /// <summary>
//        /// [构造函数]测试关卡1
//        /// </summary>
//        /// <param name="scene"></param>
//        public TestStage1(Scene scene) : base(scene)
//        {//这里可判断是不是切关然后对player或啥的做相应处理

//            //关闭小地图
//            scene.EnableMinimap(false);
//        }

//        /// <summary>
//        /// 更新
//        /// </summary>
//        /// <exception cref="System.Exception"></exception>
//        public override void Update()
//        {
//            switch (state)
//            {
//                case 0:
//                    P0();
//                    return;
//                case 1:
//                    P1();
//                    return;
//                default:
//                    throw new System.Exception("can'transform be here");
//            }
//        }

//        /// <summary>
//        /// 测试P0
//        /// </summary>
//        public void P0()
//        {
//            var cx = Scene.gridChunkCenterX;
//            var cy = Scene.gridChunkCenterY;

//            // 重置 Player 坐标
//            player.Init(this, cx, cy);

//            //// 验证一下这个表的数据是否正确
//            //var d = Scene.spaceRDD;
//            //foreach (var i in d.idxys) {
//            //    // 根据 格子 offset 计算 pixelX, pixelY 并设置怪的坐标
//            //    new TestMonster1(this).Init(cx + i.pixelX * 16, cy + i.pixelY * 16).radius = 5;
//            //}

//            state = 1;
//        }

//        /// <summary>
//        /// 测试P1（玩家屏幕范围产生大量随机数字特性）
//        /// </summary>
//        public void P1()
//        {
//            Update_Effect_Numbers();
//            Update_Player();

//            // 测试一下伤害数字的效果
//            for (int i = 0; i < 50; i++)
//            {
//                var x = Random.Range(-Scene.designWidth_2, Scene.designWidth_2);
//                var y = Random.Range(-Scene.designHeight_2, Scene.designHeight_2);
//                //var v = Random.Range(0, 1000000000) * System.Math.Pow(10, Random.Range(1, 20 - 10));   // 307 - 10
//                var v = random.NextDouble() * System.Math.Pow(10, Random.Range(2, 30 - 10));
//                new EffectNumber(this, player.pixelX + x, player.pixelY + y, 0.5f, v, Random.value > 0.5f);
//            }
//        }

//    }
//}