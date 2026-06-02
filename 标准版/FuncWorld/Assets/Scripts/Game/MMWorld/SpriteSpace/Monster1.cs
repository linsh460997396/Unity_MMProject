namespace SpriteSpace
{
    public class Monster1 : Monster
    {
        /// <summary>
        /// 怪物1号.
        /// </summary>
        /// <param name="stage_"></param>
        public Monster1(Stage stage_) : base(stage_) { }

        /// <summary>
        /// MM测试怪物的初始化方法,供MonsterGenerator调用
        /// </summary>
        /// <param name="row">逻辑坐标</param>
        /// <param name="column">逻辑坐标</param>
        public void Init(float row, float column)
        {
            //MMCore.Tell($"row:{row} column:{column} Torf:{Scene.sprites_monster02 != null}");
            if (Scene.sprites_monster02 != null)
            {
                Init(Scene.sprites_monster02, row, column);
            }
        }

        // todo: 独特逻辑
    }
}