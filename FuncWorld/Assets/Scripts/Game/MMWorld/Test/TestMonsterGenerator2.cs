//namespace MMWorld.Test
//{

//    /// <summary>
//    /// 怪物生成器2
//    /// </summary>
//    public class TestMonsterGenerator2 : MonsterGenerator
//    {
//        /// <summary>
//        /// [构造函数]怪物生成器2
//        /// </summary>
//        /// <param name="name_">名字</param>
//        /// <param name="stage_">关卡</param>
//        /// <param name="activeTime_">激活时间</param>
//        /// <param name="destroyTime_">摧毁时间</param>
//        /// <param name="generateDelay_">创建间隔</param>
//        public TestMonsterGenerator2(string name_, Stage stage_, int activeTime_, int destroyTime_, int generateDelay_) : base(name_, stage_, activeTime_, destroyTime_, generateDelay_) { }

//        /// <summary>
//        /// 更新怪物生成器2
//        /// </summary>
//        public override void Update()
//        {
//            var time = scene.time;
//            if (time > nextGenerateTime)
//            {
//                nextGenerateTime = time + generateDelay;

//                var pos = stage.GetRndPosOutSideTheArea();
//                new TestMonster2(stage).Init(pos.x, pos.y);
//            }
//        }
//    }
//}