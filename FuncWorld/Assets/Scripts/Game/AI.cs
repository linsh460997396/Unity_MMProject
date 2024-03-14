using UnityEngine;
using MetalMaxSystem;
using Vector2F = UnityEngine.Vector2;

public class AI : MonoBehaviour
{
    #region 字段

    /// <summary>
    /// 中立阵营
    /// </summary>
    public static int gv_ID_ZL = 0;
    /// <summary>
    /// 外星阵营
    /// </summary>
    public static int gv_ID_WX = 12;
    /// <summary>
    /// 玩家同盟
    /// </summary>
    public static int gv_ID_PA = 13;
    /// <summary>
    /// 异形阵营
    /// </summary>
    public static int gv_ID_YX = 14;
    /// <summary>
    /// 诺亚阵营
    /// </summary>
    public static int gv_ID_NY = 15;


    //以下待换成单位类型UnitType
    public static Unit gv_Ut0001 = new Marine();
    public static Unit gv_Ut0002 = new DominionKillTeam();
    public static Unit gv_Ut0003 = new Firebat();
    public static Unit gv_Ut0004 = new SiegeTank();
    public static Unit gv_Ut0005 = new WarHound();
    public static Unit gv_Ut0006 = new TaurenSpaceMarine();
    public static Unit gv_Ut0007 = new HellionTank();
    public static Unit gv_Ut0008 = new HammerSecurity();
    public static Unit gv_Ut0009 = new Goliath();
    public static Unit gv_Ut0010 = new HERC();
    public static Unit gv_Ut0011 = new Raynor01();
    public static Unit gv_Ut0012 = new Reaper();
    public static Unit gv_Ut0013 = new TychusChaingun();
    public static Unit gv_Ut0014 = new VikingAssault();
    public static Unit gv_Ut0015 = new Vulture();
    public static Unit gv_Ut0016 = new MarineWarfield();
    public static Unit gv_Ut0017 = new Marauder();
    public static Unit gv_Ut0018 = new MengskFirebat();
    public static Unit gv_Ut0019 = new Hellion();
    public static Unit gv_Ut0020 = new Predator();
    public static Unit gv_Ut0021 = new Cyclone();
    public static Unit gv_Ut0022 = new Liberator();
    public static Unit gv_Ut0023 = new Separatist();

    public static Unit[] gv_campBase = new Unit[Game.c_maxPlayers + 1];
    public static UnitGroup[] gv_armyGroup = new UnitGroup[Game.c_maxPlayers + 1];

    /// <summary>
    /// 阵营兵力阀值
    /// </summary>
    public static int[] gv_campForceLimit = new int[Game.c_maxPlayers + 1];

    #endregion

    /// <summary>
    /// 初始化游戏布局
    /// </summary>
    public static void GameInit()
    {
        // Variable Declarations
        int lv_p;

        // Automatic Variable Declarations
        const int autoA_ae = 12;
        int autoA_var;
        int autoB_val;
        int autoC_val;

        // Implementation
        autoA_var = 9;
        for (; autoA_var <= autoA_ae; autoA_var += 1)
        {
            lv_p = autoA_var;
            autoB_val = autoA_var;
            if (autoB_val == 12)
            {
                lv_p = gv_ID_NY;
            }
            else
            {
            }
            MMCore.HD_RegInt_Simple(lv_p, "gv_Nations");
            autoC_val = lv_p;
            if (autoC_val == 9)
            {
                Game.UnitCreate("OrbitalCommand", 0, 9, new Vector2F(10.0f, 10.0f));
            }
            else if (autoC_val == 10)
            {
                Game.UnitCreate("OrbitalCommand", 0, 10, new Vector2F(246.0f, 10.0f));
            }
            else if (autoC_val == 11)
            {
                Game.UnitCreate("OrbitalCommand", 0, 11, new Vector2F(10.0f, 246.0f));
            }
            else
            {
                Game.UnitCreate("OrbitalCommand", 0, gv_ID_NY, new Vector2F(246.0f, 246.0f));
            }
            gv_campBase[lv_p] = Game.UnitLastCreated;
            //if (gv_campBase[lv_p] != null ) 
            //{ 
            //    Debug.WriteLine("gv_campBase[" +lv_p.ToString()+"] = "+ Game.UnitLastCreated.ToString()); 
            //}
            Game.UnitLastCreated.SetProperty(UnitProp.LifeMax, 20000.0);
            Game.UnitLastCreated.SetProperty(UnitProp.LifePercent, 100.0);
            Player.ModifyProperty(lv_p, PlayerProp.SuppliesLimit, PlayerPropOp.SetTo, 200.0);
            Player.ModifyProperty(lv_p, PlayerProp.SuppliesMade, PlayerPropOp.SetTo, 200.0);
        }
        Game.Initialization = 1;
    }

    /// <summary>
    /// 四国随机出兵AI
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void BattleAI()
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
        if (!(Game.Initialization == 1))
        {
            return;
        }

        autoA_var = 9;//9~12属于4个AI阵营，用户1-8可加入
        for (; autoA_var <= autoA_ae; autoA_var += 1)
        {
            lv_p = autoA_var;
            autoB_val = autoA_var;
            if (autoB_val == 12)
            {
                lv_p = gv_ID_NY;
            }
            else
            {
            }
            lv_base = gv_campBase[lv_p];
            if (lv_base.Alive)
            {
                lv_randomNum = MMCore.RandomInt(1, 46);
                if (((lv_randomNum >= 1) && (lv_randomNum <= 23)) && (gv_armyGroup[lv_p].Count() < gv_campForceLimit[lv_p]))
                {
                    autoC_val = lv_randomNum;
                    if (autoC_val == 1)
                    {
                        Game.UnitCreate(gv_Ut0001.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 2)
                    {
                        Game.UnitCreate(gv_Ut0002.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 3)
                    {
                        Game.UnitCreate(gv_Ut0003.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 4)
                    {
                        Game.UnitCreate(gv_Ut0004.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 5)
                    {
                        Game.UnitCreate(gv_Ut0005.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 6)
                    {
                        Game.UnitCreate(gv_Ut0006.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 7)
                    {
                        Game.UnitCreate(gv_Ut0007.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 8)
                    {
                        Game.UnitCreate(gv_Ut0008.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 9)
                    {
                        Game.UnitCreate(gv_Ut0009.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 10)
                    {
                        Game.UnitCreate(gv_Ut0010.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 11)
                    {
                        Game.UnitCreate(gv_Ut0011.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 12)
                    {
                        Game.UnitCreate(gv_Ut0012.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 13)
                    {
                        Game.UnitCreate(gv_Ut0013.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 14)
                    {
                        Game.UnitCreate(gv_Ut0014.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 15)
                    {
                        Game.UnitCreate(gv_Ut0015.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 16)
                    {
                        Game.UnitCreate(gv_Ut0016.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 17)
                    {
                        Game.UnitCreate(gv_Ut0017.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 18)
                    {
                        Game.UnitCreate(gv_Ut0018.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 19)
                    {
                        Game.UnitCreate(gv_Ut0019.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 20)
                    {
                        Game.UnitCreate(gv_Ut0020.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 21)
                    {
                        Game.UnitCreate(gv_Ut0021.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 22)
                    {
                        Game.UnitCreate(gv_Ut0022.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else if (autoC_val == 23)
                    {
                        Game.UnitCreate(gv_Ut0023.TypeName, 0, lv_p, lv_base.Vector2F);
                    }
                    else { }
                    gv_armyGroup[lv_p].Add(Game.UnitLastCreated);
                }

            }

            lv_randomNum = MMCore.RandomInt(9, 12);
            if ((lv_randomNum == 12))
            {
                lv_randomNum = gv_ID_NY;
            }

            lv_targetBase = gv_campBase[lv_randomNum];
            if ((lv_targetBase.Alive) && (lv_targetBase != lv_base))
            {
                Game.UnitLastCreated.IssueOrder(Game.OrderTargetingPoint(Game.AbilityCommand("attack", 0), lv_targetBase.Vector2F), (int)OrderQueue.Replace);
                autoD_g = UnitGroup.Idle(lv_p, false);
                autoD_u = autoD_g.Count();
                for (; ; autoD_u -= 1)
                {
                    autoD_var = UnitGroup.UnitFromEnd(autoD_g, autoD_u);
                    if (autoD_var == null) { break; }
                    autoD_var.IssueOrder(Game.OrderTargetingPoint(Game.AbilityCommand("attack", 0), lv_targetBase.Vector2F), (int)OrderQueue.Replace);
                }
            }

        }
    }

    void Start()
    {
        GameInit();
    }

    void Update()
    {
        BattleAI();
    }
}
