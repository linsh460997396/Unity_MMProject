namespace MMWorld.Test
{

    /// <summary>
    /// MM测试关卡
    /// </summary>
    public class MMTestStage : Stage
    {
        /// <summary>
        /// 超时指标（用于切换游玩模块）
        /// </summary>
        public int timeout;

        /// <summary>
        /// [构造函数]MM测试关卡
        /// </summary>
        /// <param name="scene"></param>
        public MMTestStage(Scene scene) : base(scene)
        {//这里可判断是不是切关,然后对 player 或啥的做相应处理

            //开启小地图
            scene.EnableMinimap(true);

            //先给自己创建一些初始技能
            //todo：通过配置来创建，纯技能并没有什么意义，只有进了关卡之后才能实例化、开始工作，也就是说技能依附于关卡存在
            //玩家在游戏过程中，技能可能会 增加，成长，都应该写进 配置。 这样切换关卡后，可以根据配置 再次创建技能
            {
                var ps = new MMTestPlayerSkill(this);
                ps.Init();
                player.skills.Add(ps);
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <exception cref="System.Exception"></exception>
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
                    throw new System.Exception("Error");
            }
        }

        /// <summary>
        /// 测试P0（关卡出现时运行一次初始化）
        /// </summary>
        public void P0()
        {//关卡开局
         //配置怪生成器
            var time = scene.time; //等同运行游戏逻辑的次数
            //创建怪物或NPC
            monsterGenerators.Add(new MMTestMonsterGenerator("stage0_1", this, time + Scene.fps * 0, time + Scene.fps * 10, 10));
            //重置玩家位置（关卡,逻辑坐标X,逻辑坐标Y）
            //第一个镇子场景如要出现在本地(1,1)位置，则填逻辑坐标=本地坐标*gridSize即(100,100)，目前gridSize=100表示将1个渲染层实际是1.0边长的单元格从逻辑上又划分了100格，空间内本地距离0.01作为游戏逻辑用的逻辑距离1
            player.Init(this, 350, 350);
            state = 1;//设置下一个自动轮入的关卡索引
        }

        /// <summary>
        /// 测试P1（可反复运行）
        /// </summary>
        public void P1()
        {
            Update_Effect_Explosions();
            Update_Effect_Numbers();
            Update_Monsters();
            //怪物没了就进下个state索引
            //if (Update_MonstersGenerators() == 0)
            //{         // 怪生成器 已经没了
            //    timeout = scene.time + Scene.fps * 60;      // 设置 60 秒超时
            //    state = 2;
            //}
            Update_PlayerBullets();
            player.Update();
            //不再有新关卡索引时，就一直循环该方法，玩家自由移动和射击、更新位置和动画
        }

        /// <summary>
        /// 测试P2
        /// </summary>
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

        /// <summary>
        /// 测试P3
        /// </summary>
        public void P3()
        {
            scene.SetStage(new MMTestStage(scene));          // 已超时：切到新关卡
        }
    }
}