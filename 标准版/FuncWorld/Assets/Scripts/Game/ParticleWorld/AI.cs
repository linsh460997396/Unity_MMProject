using MetalMaxSystem;
using System;

#if UNITY_EDITOR || UNITY_STANDALONE
//Unity编辑器、独立应用程序(不包括Web播放器)
using UnityEngine;
using Vector2F = UnityEngine.Vector2;
#else
using Vector2F = System.Numerics.Vector2;
#endif

namespace BattleTest
{
    public class AI
#if UNITY_EDITOR || UNITY_STANDALONE
        : MonoBehaviour
#endif
    {
        #region 字段

        //玩家=1~8,备用=9~11,触发器(上帝)=16

        /// <summary>
        /// 中立阵营
        /// </summary>
        public static int gv_ID_ZL = 0;
        /// <summary>
        /// 玩家同盟
        /// </summary>
        public static int gv_ID_PA = 12;
        /// <summary>
        /// 外星阵营
        /// </summary>
        public static int gv_ID_WX = 13;
        /// <summary>
        /// 异形阵营
        /// </summary>
        public static int gv_ID_YX = 14;
        /// <summary>
        /// 诺亚阵营
        /// </summary>
        public static int gv_ID_NY = 15;

        /// <summary>
        /// 阵营主基地(死亡后自动寻找分支基地或其他球作为主基地,亦可手动选择)
        /// </summary>
        public static Unit[] gv_campBase = new Unit[Game.c_maxPlayers + 1];
        /// <summary>
        /// 玩家军队单位组
        /// </summary>
        public static UnitGroup[] gv_armyGroup = new UnitGroup[Game.c_maxPlayers + 1];

        /// <summary>
        /// 阵营兵力阀值(上限)
        /// </summary>
        public static int[] gv_campForceLimit = new int[Game.c_maxPlayers + 1];

        #endregion

        /// <summary>
        /// 初始化游戏布局
        /// </summary>
        public static void GameInit()
        {
            // Variable Declarations
            int lv_p; //代表阵营玩家

            // Automatic Variable Declarations
            const int autoA_ae = 12;
            int autoA_var;

            // Implementation
            for (autoA_var = 9; autoA_var <= autoA_ae; autoA_var += 1)
            {

                if (autoA_var == 12)
                {
                    //挑选到12时,玩家ID切换到异形阵营
                    lv_p = gv_ID_YX;
                }
                else
                {
                    lv_p = autoA_var;
                }
                //简单注册玩家ID,之后固有状态激活为true,表示阵营活体
                MMCore.HD_RegInt_Simple(lv_p, "Nation");

                if (autoA_var == 9)
                {
                    Game.UnitCreate("OrbitalCommand", 0, lv_p, new Vector2F(10.0f, 10.0f));
                    MMCore.Tell("创建了玩家9的 OrbitalCommand");
                }
                else if (autoA_var == 10)
                {
                    Game.UnitCreate("OrbitalCommand", 0, lv_p, new Vector2F(246.0f, 10.0f));
                    MMCore.Tell("创建了玩家10的 OrbitalCommand");
                }
                else if (autoA_var == 11)
                {
                    Game.UnitCreate("OrbitalCommand", 0, lv_p, new Vector2F(10.0f, 246.0f));
                    MMCore.Tell("创建了玩家11的 OrbitalCommand");
                }
                else
                {
                    Game.UnitCreate("OrbitalCommand", 0, lv_p, new Vector2F(246.0f, 246.0f));
                    MMCore.Tell("创建了异形玩家的 OrbitalCommand");
                }
                //把上面创建的4个阵营主基地存入到gv_campBase,势力玩家开局默认只能有1个主基地(可发展为超大球),后期考虑添加分支基地亦可以发展为超大球
                gv_campBase[lv_p] = Game.UnitLastCreated;
                //设置主基地血量、玩家补给等属性
                Game.UnitLastCreated.SetProperty(UnitProp.LifeMax, 20000.0);
                Game.UnitLastCreated.SetProperty(UnitProp.LifePercent, 100.0);
                MetalMaxSystem.Player.ModifyProperty(lv_p, PlayerProp.SuppliesLimit, PlayerPropOp.SetTo, 200.0);
                MetalMaxSystem.Player.ModifyProperty(lv_p, PlayerProp.SuppliesMade, PlayerPropOp.SetTo, 200.0);
                //玩家兵力阀值
                gv_campForceLimit[lv_p] = 350;
            }
            Game.Initialization = 1;
        }

        /// <summary>
        /// 主基地随机产兵AI,出生后赋予一次巡防、探索、建造、战斗等指令
        /// </summary>
        public static void BattleAI(object sender, EventArgs e)
        {
            // Variable Declarations
            Unit lv_base, lv_targetBase;
            int lv_randomNum, lv_p;

            // Automatic Variable Declarations
            const int autoA_ae = 12;
            int autoA_var;
            int autoB_val;
            int autoC_val;
            UnitGroup autoD_g;
            int autoD_u;
            Unit autoD_var;

            // Conditions
            if (Game.Initialization != 1)
            {
                return;
            }

            //4个开局AI阵营在周围随机产兵
            for (autoA_var = 9; autoA_var <= autoA_ae; autoA_var += 1)
            {
                autoB_val = autoA_var;
                if (autoB_val == 12)
                {
                    //挑选到12时,玩家ID切换到异形阵营
                    lv_p = gv_ID_YX;
                }
                else
                {
                    lv_p = autoA_var;
                }

                //当前的玩家主基地
                lv_base = gv_campBase[lv_p];
                if (lv_base.Alive) //单位刚创建时Alive默认为true,这里主基地没死亡且玩家军队计数小于阀值时产兵
                {
                    lv_randomNum = MMCore.RandomInt(1, 51);
                    MMCore.Tell("randomNum = " + lv_randomNum);
                    MMCore.Tell("armyGroup.Count[" + lv_p + "] = " + gv_armyGroup[lv_p].Count());
                    MMCore.Tell("campForceLimit[" + lv_p + "] = " + gv_campForceLimit[lv_p]);
                    if (lv_randomNum >= 1 && lv_randomNum <= 25 && gv_armyGroup[lv_p].Count() < gv_campForceLimit[lv_p])
                    {
                        autoC_val = lv_randomNum;
                        if (autoC_val == 1)
                        {
                            //2%概率诞生一个特殊兵种
                            Game.UnitCreate("UnitType", 0, lv_p, lv_base.Vector2F);
                            MMCore.Tell("诞生了特殊兵种 名为UnitType");
                        }
                        else if (autoC_val >= 2 && autoC_val <= 25)
                        {
                            //48%诞生一个基础兵种
                            Game.UnitCreate("Any", 0, lv_p, lv_base.Vector2F);
                            MMCore.Tell("诞生了基础兵种 名为Any");
                        }
                        else
                        {
                            //50%概率什么也不做
                            MMCore.Tell("什么也不做");
                        }
                        if (autoC_val <= 25)
                        {
                            //新诞生的兵种加入玩家军队数组
                            gv_armyGroup[lv_p].Add(Game.UnitLastCreated);
                            MMCore.Tell("新诞生的兵种加入玩家" + lv_p + "的军队");
                        }
                    }
                }
                else { MMCore.Tell("玩家" + lv_p + "的基地已死亡"); }

                //随机进行操作
                lv_randomNum = MMCore.RandomInt(9, 13);
                if (lv_randomNum == 12)
                {
                    //挑选到12时切换到异形阵营
                    lv_randomNum = gv_ID_YX;
                }
                //确定目标基地
                lv_targetBase = gv_campBase[lv_randomNum];
                //目标基地存活且非己方基地时
                if (lv_targetBase.Alive && lv_targetBase != lv_base)
                {
                    //对最后创建的单位发布命令,如进攻目标基地点
                    Game.UnitLastCreated.IssueOrder(Game.OrderTargetingPoint(Game.AbilityCommand("attack", 0), lv_targetBase.Vector2F), (int)OrderQueue.Replace);//立即覆盖当前指令(序号0的指令是当前的,1~29为队列的)
                    autoD_g = UnitGroup.Idle(lv_p, false);//玩家闲置(没有指令)的单位组
                    autoD_u = autoD_g.Count();//单位组内元素计数(若有10个,那么数组元素的序号为1~10)
                    for (; ; autoD_u -= 1)
                    {
                        autoD_var = UnitGroup.UnitFromEnd(autoD_g, autoD_u);//返回元素序号对应的单位
                        if (autoD_var == null) { break; } //若返回不到则打断循环体
                                                          //对有效单位发布指令
                        autoD_var.IssueOrder(Game.OrderTargetingPoint(Game.AbilityCommand("attack", 0), lv_targetBase.Vector2F), (int)OrderQueue.Replace);
                        MMCore.Tell("单位命令进攻lv_targetBase");
                    }
                }

            }
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        void Start()
        {
            //初始化
            GameInit();
            Trigger gt_testAI = new Trigger();//创建一个周期触发器
            gt_testAI.Update += AI.BattleAI;//注册触发器循环事件给指定函数
            gt_testAI.Period = 1000;//周期间隔
            gt_testAI.Run(true);//触发器后台执行
        }
#endif
    }
}
