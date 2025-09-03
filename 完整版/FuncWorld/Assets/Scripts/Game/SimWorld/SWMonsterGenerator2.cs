namespace SimWorld
{
    public class SWMonsterGenerator2 : SWMonsterGenerator
    {
        public SWMonsterGenerator2(string name_, SWStage stage_, int activeTime_, int destroyTime_, int generateDelay_) : base(name_, stage_, activeTime_, destroyTime_, generateDelay_)
        {
        }

        public override void Update()
        {
            var time = scene.time;
            if (time > nextGenerateTime)
            {
                nextGenerateTime = time + generateDelay;

                var pos = stage.GetRndPosOutSideTheArea();
                new SWMonster2(stage).Init(pos.x, pos.y);
            }
        }
    }
}