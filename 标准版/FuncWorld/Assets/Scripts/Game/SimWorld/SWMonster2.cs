namespace SimWorld
{
    public class SWMonster2 : SWMonster
    {
        public SWMonster2(SWStage stage_) : base(stage_)
        {
        }

        // 供 SWMonsterGenerator 调用
        public void Init(float x, float y)
        {
            Init(scene.sprites_monster02, x, y);
        }

        // todo: 独特逻辑
    }
}