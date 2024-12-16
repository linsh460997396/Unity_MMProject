namespace MMWorld
{
    /// <summary>
    /// 怪物生成器
    /// </summary>
    public class MonsterGenerator
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string name;
        /// <summary>
        /// 场景
        /// </summary>
        public Scene scene;
        /// <summary>
        /// 关卡
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 激活时间（帧）
        /// </summary>
        public int activeTime;
        /// <summary>
        /// 摧毁时间（帧）
        /// </summary>
        public int destroyTime;
        /// <summary>
        /// 生成间隔（帧）
        /// </summary>
        public int generateDelay;
        /// <summary>
        /// 下次生成时间（帧）
        /// </summary>
        public int nextGenerateTime;

        /// <summary>
        /// [构造函数]怪物生成器
        /// </summary>
        /// <param name="name_">名字</param>
        /// <param name="stage_">关卡</param>
        /// <param name="activeTime_">激活时间（帧）</param>
        /// <param name="destroyTime_">摧毁时间（帧）</param>
        /// <param name="generateDelay_">生成间隔（帧）</param>
        protected MonsterGenerator(string name_, Stage stage_, int activeTime_, int destroyTime_, int generateDelay_)
        {
            name = name_;
            stage = stage_;
            scene = stage_.scene;
            activeTime = activeTime_;
            destroyTime = destroyTime_;
            generateDelay = generateDelay_;
        }

        /// <summary>
        /// [虚函数]更新怪物生成器
        /// </summary>
        public virtual void Update()
        {
            //var time = scene.time;
            //if (time > nextGenerateTime) {
            //    nextGenerateTime = time + generateDelay;
            //
            //    //new Monster( rnd pos ? );
            //}
        }

        /// <summary>
        /// [虚函数]摧毁怪物生成器
        /// </summary>
        public virtual void Destroy() { }
    }
}

//public void GenRndMonster() {
//    //// 每一种创建 ?? 只
//    //foreach (var ss in spritess) {
//    //    for (int i = 0; i < 5000; i++) {
//    //        var pixelX = gridChunkCenterX + UnityEngine.Random.Range(-Scene.designWidth_2, Scene.designWidth_2);
//    //        var pixelY = gridChunkCenterY + UnityEngine.Random.Range(-Scene.designHeight_2, Scene.designHeight_2);
//    //        new Monster(this, ss, pixelX, pixelY);
//    //    }
//    //}
//    // todo: 补怪逻辑, 阶段性试图凑够多少只同屏
//    var ss = spritess[Random.Range(0, spritess.Count)];
//    var p = GetRndPosOutSideTheArea();
//    new Monster(this, ss, p.pixelX, p.pixelY);
//}
