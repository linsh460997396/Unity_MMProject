namespace SimWorld
{
    public class SWMonsterGenerator
    {
        public string name;
        public Main_SimWorld scene;
        public SWStage stage;

        public int activeTime, destroyTime;
        public int generateDelay, nextGenerateTime;

        protected SWMonsterGenerator(string name_, SWStage stage_, int activeTime_, int destroyTime_, int generateDelay_)
        {
            name = name_;
            stage = stage_;
            scene = stage_.scene;
            activeTime = activeTime_;
            destroyTime = destroyTime_;
            generateDelay = generateDelay_;
        }

        public virtual void Update()
        {
            //var time = scene.time;
            //if (time > nextGenerateTime) {
            //    nextGenerateTime = time + generateDelay;
            //
            //    //new SWMonster( rnd pos ? );
            //}
        }

        public virtual void Destroy()
        {
        }
    }
}

//public void GenRndMonster() {
//    //// 每一种创建 ?? 只
//    //foreach (var ss in spritess) {
//    //    for (int i = 0; i < 5000; i++) {
//    //        var pixelX = gridChunkCenterX + UnityEngine.Random.Range(-Main_SimWorld.designWidth_2, Main_SimWorld.designWidth_2);
//    //        var pixelY = gridChunkCenterY + UnityEngine.Random.Range(-Main_SimWorld.designHeight_2, Main_SimWorld.designHeight_2);
//    //        new SWMonster(this, ss, pixelX, pixelY);
//    //    }
//    //}
//    // todo: 补怪逻辑, 阶段性试图凑够多少只同屏
//    var ss = spritess[Random.Range(0, spritess.Count)];
//    var p = GetRndPosOutSideTheArea();
//    new SWMonster(this, ss, p.pixelX, p.pixelY);
//}
