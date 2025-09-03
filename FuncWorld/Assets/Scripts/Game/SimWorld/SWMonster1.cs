namespace SimWorld
{
    public class SWMonster1 : SWMonster
    {
        public SWMonster1(SWStage stage_) : base(stage_)
        {
        }

        // 供 SWMonsterGenerator 调用
        public SWMonster1 Init(float x, float y)
        {
            Init(scene.sprites_monster01, x, y);
            return this;
        }

        // todo: 独特逻辑
    }
}