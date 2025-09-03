namespace SimWorld
{
    public class SWStage2 : SWStage
    {
        public int timeout;

        public SWStage2(Main_SimWorld scene) : base(scene)
        {
            // 这里可判断是不是 切关, 然后对 player 或啥的做相应处理

            // 开启小地图
            scene.EnableMinimap(true);

            // 先给自己创建一些初始技能
            // todo: 通过配置来创建. 纯技能并没有什么意义,只有进了关卡之后,才能实例化,开始工作. 也就是说,技能依附于关卡存在.
            // 玩家在游戏过程中,技能可能会 增加,成长,都应该写进 配置. 这样切换关卡后,可以根据配置 再次创建技能
            {
                var ps = new SWPlayerSkill(this);
                ps.Init();
                player.skills.Add(ps);
            }
            {
                var ps = new SWPlayerSkill2(this);
                ps.Init();
                player.skills.Add(ps);
            }
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
                case 2:
                    P2();
                    return;
                case 3:
                    P3();
                    return;
                default:
                    throw new System.Exception("can'transform be here");
            }
        }

        public void P0()
        {

            // 配置怪生成器
            var time = scene.time;
            monsterGenerators.Add(new SWMonsterGenerator1("stage0_1", this, time + Main_SimWorld.fps * 0, time + Main_SimWorld.fps * 10, 1));
            monsterGenerators.Add(new SWMonsterGenerator2("stage0_2", this, time + Main_SimWorld.fps * 10, time + Main_SimWorld.fps * 20, 0));

            // 重置 SWPlayer 坐标
            player.Init(this, Main_SimWorld.gridCenterX, Main_SimWorld.gridCenterY);

            state = 1;
        }

        public void P1()
        {
            Update_Effect_Explosions();
            Update_Effect_Numbers();
            Update_Monsters();
            if (Update_MonstersGenerators() == 0)
            {         // 怪生成器 已经没了
                timeout = scene.time + Main_SimWorld.fps * 60;      // 设置 60 秒超时
                state = 2;
            }
            Update_PlayerBullets();
            player.Update();
        }

        public void P2()
        {
            Update_Effect_Explosions();
            Update_Effect_Numbers();
            if (Update_Monsters() == 0)
            {   // 怪杀完
                state = 3;
            }
            Update_PlayerBullets();
            player.Update();
            if (timeout < scene.time)
            {     // 已超时
                state = 3;
            }
        }

        public void P3()
        {
            scene.SetStage(new SWStage1(scene));          // 已超时:切到新关卡
        }
    }
}