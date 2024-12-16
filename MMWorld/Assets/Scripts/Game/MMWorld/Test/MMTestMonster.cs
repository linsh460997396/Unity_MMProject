namespace MMWorld.Test
{
    public class MMTestMonster : Monster
    {
        /// <summary>
        /// MM测试专用怪物
        /// </summary>
        /// <param name="stage_"></param>
        public MMTestMonster(Stage stage_) : base(stage_) { }

        /// <summary>
        /// MM测试怪物的初始化方法，供MonsterGenerator调用
        /// </summary>
        /// <param name="x">逻辑坐标</param>
        /// <param name="y">逻辑坐标</param>
        public void Init(float x, float y)
        {
            Init(scene.sprites_monster02, x, y);
        }

        // todo: 独特逻辑
    }
}