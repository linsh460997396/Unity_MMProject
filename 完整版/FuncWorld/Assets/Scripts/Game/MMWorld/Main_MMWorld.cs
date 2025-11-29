using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using CellSpace;
using SpriteSpace;

namespace MMWorld
{
    /// <summary>
    /// 主入口
    /// </summary>
    public class Main_MMWorld : MonoBehaviour
    {
        //[NonSerialized] public static List<Sprite>[] mapSprites; //由于CellSpace使用了材质纹理UV自动划区,该精灵数组不再使用
        [NonSerialized] public static List<Sprite>[] Vehicle;
        [NonSerialized] public static List<Sprite>[] characters;
        [NonSerialized] public static List<Sprite>[] monsters;

        public static Scene scene;
        public static bool sceneEnabled = false;

        ///// <summary>
        ///// 挂上组件时运行(无论是否激活,仅自动运行1次)
        ///// </summary>
        //public void Awake()
        //{

        //}

        ///// <summary>
        ///// 组件激活时运行一次(反复激活反复运行)
        ///// </summary>
        //public void OnEnable()
        //{

        //}

        /// <summary>
        /// OnEnable后运行一次(仅自动运行1次,反复激活无效)
        /// </summary>
        public void Start()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            LoadAssets();
            stopwatch.Stop();
            Debug.Log("LoadAllAssets" + $"耗时:{stopwatch.ElapsedMilliseconds}ms");

            //TE_AIBehavior.Main(); //unsafe模式下测试亿次AI行为,前往Scripts/Test/Example找到并激活使用(取消注释)

            //测试触发器(子线程运行)
            //Trigger timerUpdate = new Trigger();
            //timerUpdate.Update += TestFunc;
            //timerUpdate.Duetime = 1000;//前摇等待1s
            //timerUpdate.Period = 500;//500ms执行一次
            //timerUpdate.TriggerStart(true);//后台运行触发

            scene = gameObject.GetComponent<Scene>();
            CPEngine.Active();
        }

        /// <summary>
        /// 每帧运行
        /// </summary>
        public void Update()
        {
            if (sceneEnabled == false && CellChunkManager.SpawningChunks == false)
            {//幸存者场景组件未启用且单元团块空间停止生成时
                sceneEnabled = true;
                gameObject.GetComponent<Scene>().enabled = true;//启用幸存者场景组件
            }

            CPEngine.Tick();
        }

        #region 功能函数

        /// <summary>
        /// 读取素材的总动作
        /// </summary>
        void LoadAssets()
        {
            #region 
            //注意Application.dataPath打包前是Assets文件夹下的路径,打包后是识别exe程序名称_Data文件夹下的路径
            //Unity自带切割上万精灵应用到meta要几个小时情况时,可换此加载方式
            //没用Resources方式的不会被做成素材包,玩家可直观修改替换这些素材文件
            //Texture2D tmpWP = TextureAnalyzer.LoadImageAndConvertToTexture2D(Application.dataPath + "/Resources/Textures/WorldSP.png");
            //Texture2D tmpMP = TextureAnalyzer.LoadImageAndConvertToTexture2D(Application.dataPath + "/Resources/Textures/MapSP.png");
            //Sprite[] tmpWSP = TextureAnalyzer.SliceTexture(tmpWP, 16, 16);
            //Sprite[] tmpMSP = TextureAnalyzer.SliceTexture(tmpMP, 16, 16);

            //Texture2D tWorldSP = Resources.Load<Texture2D>("Textures/WorldSP");
            //Texture2D tMapSP = Resources.Load<Texture2D>("Textures/WorldSP");

            //不沾引擎主线程对象情况可让子线程来处理(同时也是测试子线程运用),若是ECS设计模式则专门有支持引擎对象通过的官方线程处理
            //Trigger loadTXT = new Trigger(1); //创建1个循环1次的触发器实例(以子线程运行)
            //loadTXT.Awake += Trigger_LoadTXT_Func; //将需要委托运行的函数注册到Awake事件(该事件在触发器实例启动后运行1次,可在触发器实例属性中设置启动前摇)
            //loadTXT.Run(true);//以后台模式运行这个触发器
            ////等待子线程完成(要使用精确结果进行下一步时)
            //loadTXT.Thread.Join();

            //由于使用了材质纹理UV划区,这里的精灵可以不再使用
            //mapSprites = new List<Sprite>[2];

            //官方Resources方法(推荐),Resources文件夹内的素材会自动打包,不用担心打包后路径问题(这种方式素材打包后玩家没法直观修改和替换,但可以提供Mod接口)
            //Sprite[] tmpWSP = Resources.LoadAll<Sprite>("Textures/WorldSP");
            //Sprite[] tmpMSP = Resources.LoadAll<Sprite>("Textures/MapSP");

            //初始化数组中的每个List元素
            //for (int i = 0; i < mapSprites.Length; i++)
            //{
            //    mapSprites[i] = new List<Sprite>();
            //}
            //mapSprites[0].AddRange(tmpWSP);
            //mapSprites[1].AddRange(tmpMSP);
            #endregion

            //读取余下资源
            LoadAllResources();
        }

        /// <summary>
        /// 读取所有资源(怪物、角色、载具的纹理和精灵切片)
        /// </summary>
        public void LoadAllResources()
        {
            LoadAllVehicle();
            LoadAllCharacters();
            LoadAllMonsters();
            //还差UI素材读取,幸存者游戏暂时不需要
        }

        /// <summary>
        /// 读取怪物纹理和精灵切片
        /// </summary>
        public void LoadAllMonsters()
        {
            monsters = new List<Sprite>[132];
            // 初始化数组中的每个List元素
            for (int i = 0; i < monsters.Length; i++)
            {
                monsters[i] = new List<Sprite>();
            }
            //以下不是手敲的..扫描文件夹进行的打印,散图读取效率还行就懒得合并了,因为Unity已经切片,读完不用分割精灵直接可以使用数组monsters[TypeIndex][SpriteIndex]
            monsters[0].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E01_火焰枪"));
            monsters[1].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E01_火焰炮"));
            monsters[2].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E02_加农炮"));
            monsters[3].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E02_野战炮"));
            monsters[4].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E03_扫描仪"));
            monsters[5].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E03_监视器"));
            monsters[6].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E04_催眠器"));
            monsters[7].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E04_声纳车"));
            monsters[8].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E05_歼灭者"));
            monsters[9].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E05_防御器"));
            monsters[10].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E06_暗堡"));
            monsters[11].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E06_碉堡"));
            monsters[12].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E07_神秘人"));
            monsters[13].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E07_神风弹"));
            monsters[14].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E08_合金鸟"));
            monsters[15].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E08_机器鸟"));
            monsters[16].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E09_光防御器"));
            monsters[17].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E10_磁铁"));
            monsters[18].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E11_追踪弹"));
            monsters[19].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E12_僵尸"));
            monsters[20].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E12_弗朗"));
            monsters[21].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E13_激光系统"));
            monsters[22].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E14_智能人"));
            monsters[23].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/E15_机器人"));
            monsters[24].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L01_巨蚁"));
            monsters[25].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L01_酸蚁"));
            monsters[26].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L02_变形虫"));
            monsters[27].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L02_杀人虫"));
            monsters[28].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L02_超导虫"));
            monsters[29].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L03_彷生蜗牛"));
            monsters[30].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L03_毒蜗牛"));
            monsters[31].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L04_地雷龟"));
            monsters[32].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L04_炸弹龟"));
            monsters[33].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L05_毒蜘蛛"));
            monsters[34].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L06_喷火鳄"));
            monsters[35].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L06_异形鱼"));
            monsters[36].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L07_流氓"));
            monsters[37].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L07_瓦鲁部下"));
            monsters[38].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L08_马歇尔"));
            monsters[39].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L09_反坦克兵G"));
            monsters[40].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L09_反坦克兵O"));
            monsters[41].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L10_侦查者"));
            monsters[42].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L10_异形"));
            monsters[43].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L11_激光虫"));
            monsters[44].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L11_离子虫"));
            monsters[45].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L12_声波蛇"));
            monsters[46].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L12_声纳蛇"));
            monsters[47].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L13_喷火怪"));
            monsters[48].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L13_水怪"));
            monsters[49].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L13_波特"));
            monsters[50].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L14_雷达花"));
            monsters[51].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L15_帕鲁"));
            monsters[52].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L16_戈麦斯"));
            monsters[53].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L17_军蚁"));
            monsters[54].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L17_金蚁毯"));
            monsters[55].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L18_金蚁"));
            monsters[56].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L18_食人蚁"));
            monsters[57].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L19_水鬼"));
            monsters[58].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L19_水鬼H"));
            monsters[59].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L19_蛙人"));
            monsters[60].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L20_电磁花"));
            monsters[61].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L20_食人花"));
            monsters[62].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L20_魔鬼花"));
            monsters[63].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L21_反坦克炮H"));
            monsters[64].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L21_反坦克炮R"));
            monsters[65].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/L21_战狗"));
            monsters[66].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S01_章鱼坦克"));
            monsters[67].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S01_章鱼炮"));
            monsters[68].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S02_步枪鸟"));
            monsters[69].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S02_飞狗"));
            monsters[70].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S03_机械虫"));
            monsters[71].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S03_激光蚓"));
            monsters[72].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S04_化学炮"));
            monsters[73].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S04_生物炮"));
            monsters[74].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S05_后备车"));
            monsters[75].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S05_狙击鸟"));
            monsters[76].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S06_侦查蜂"));
            monsters[77].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S06_毒蜂"));
            monsters[78].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S07_毒蝙蝠"));
            monsters[79].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S08_地狱"));
            monsters[80].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S08_大象"));
            monsters[81].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S08_蜈蚣"));
            monsters[82].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S09_导弹蛙"));
            monsters[83].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S10_巨蟹"));
            monsters[84].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S10_机械蟹"));
            monsters[85].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S11_巨型河马"));
            monsters[86].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S11_机械河马"));
            monsters[87].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/S12_铁甲炮"));
            monsters[88].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_85自行炮"));
            monsters[89].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_坦克"));
            monsters[90].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_导弹车"));
            monsters[91].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T01_音速车"));
            monsters[92].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_ATM战车"));
            monsters[93].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_指挥车"));
            monsters[94].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_瓦鲁"));
            monsters[95].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T02_高速车"));
            monsters[96].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T03_抓吊"));
            monsters[97].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T03_粉碎机"));
            monsters[98].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T03_起重机"));
            monsters[99].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T04_急救车"));
            monsters[100].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T05_噪音车"));
            monsters[101].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T05_声波炮"));
            monsters[102].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T05_铲车"));
            monsters[103].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T06_导弹卡车"));
            monsters[104].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T07_无坐力炮"));
            monsters[105].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T07_狙击车"));
            monsters[106].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T08_沙漠虎"));
            monsters[107].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T08_沙漠车"));
            monsters[108].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T09_AT坦克B"));
            monsters[109].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T09_AT坦克R"));
            monsters[110].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T10_沙漠之舟"));
            monsters[111].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T10_猎杀者"));
            monsters[112].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T11_戈斯战车"));
            monsters[113].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T11_离子坦克"));
            monsters[114].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T12_两栖车"));
            monsters[115].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T12_龟式战车"));
            monsters[116].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T13_攻击机"));
            monsters[117].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T13_防御机器"));
            monsters[118].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T14_古炮"));
            monsters[119].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T14_神武炮"));
            monsters[120].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T15_装甲车"));
            monsters[121].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T15_车载炮"));
            monsters[122].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T16_侦查碟"));
            monsters[123].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/T16_拦截碟"));
            monsters[124].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_电脑墙"));
            monsters[125].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_诺亚v1"));
            monsters[126].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_诺亚v2"));
            monsters[127].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X01_诺亚v3"));
            monsters[128].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X02_毒液枪"));
            monsters[129].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X02_生物枪"));
            monsters[130].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X03_鬼手"));
            monsters[131].AddRange(Resources.LoadAll<Sprite>("Textures/Monsters/X04_巨型炮"));
        }

        /// <summary>
        /// 读取角色纹理和精灵切片
        /// </summary>
        public void LoadAllCharacters()
        {
            characters = new List<Sprite>[54];
            // 初始化数组中的每个List元素
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = new List<Sprite>();
            }
            characters[0].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/主角"));
            characters[1].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/主角2"));
            characters[2].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/主角3"));
            characters[3].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/佛像"));
            characters[4].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/修理师傅"));
            characters[5].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/勇士"));
            characters[6].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/勇士2"));
            characters[7].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/勇士3"));
            characters[8].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商人"));
            characters[9].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商店员"));
            characters[10].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商店员2"));
            characters[11].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/商店员3"));
            characters[12].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/大象"));
            characters[13].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/头"));
            characters[14].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/女孩"));
            characters[15].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/女孩2"));
            characters[16].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/妇女"));
            characters[17].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/小孩"));
            characters[18].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女"));
            characters[19].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女2"));
            characters[20].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女3"));
            characters[21].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民女4"));
            characters[22].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男"));
            characters[23].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男2"));
            characters[24].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男3"));
            characters[25].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/平民男4"));
            characters[26].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/异型鱼"));
            characters[27].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/战狗"));
            characters[28].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/按摩师"));
            characters[29].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/旅行者"));
            characters[30].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/旅馆服务员"));
            characters[31].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/明奇博士"));
            characters[32].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/普通勇士"));
            characters[33].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/普通的尸体"));
            characters[34].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人"));
            characters[35].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人2"));
            characters[36].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人3"));
            characters[37].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/机器人4"));
            characters[38].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/死人"));
            characters[39].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/气功师"));
            characters[40].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/气功师2"));
            characters[41].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/流氓"));
            characters[42].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/烧焦的尸体"));
            characters[43].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/狼"));
            characters[44].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/红狼"));
            characters[45].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/老年人"));
            characters[46].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/老者"));
            characters[47].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/船夫"));
            characters[48].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人"));
            characters[49].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人2"));
            characters[50].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人3"));
            characters[51].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/行人4"));
            characters[52].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/记录员"));
            characters[53].AddRange(Resources.LoadAll<Sprite>("Textures/Characters/马歇尔"));
        }

        /// <summary>
        /// 读取载具纹理和精灵切片
        /// </summary>
        public void LoadAllVehicle()
        {
            Vehicle = new List<Sprite>[9];
            // 初始化数组中的每个List元素
            for (int i = 0; i < Vehicle.Length; i++)
            {
                Vehicle[i] = new List<Sprite>();
            }
            Vehicle[0].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/1号战车"));
            Vehicle[1].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/2号战车"));
            Vehicle[2].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/3号战车"));
            Vehicle[3].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/4号战车"));
            Vehicle[4].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/5号战车"));
            Vehicle[5].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/6号战车"));
            Vehicle[6].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/7号战车"));
            Vehicle[7].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/8号战车"));
            Vehicle[8].AddRange(Resources.LoadAll<Sprite>("Textures/Vehicle/船"));
        }

        #region 未使用或测试结束
        ///// <summary>
        ///// 场景图片宽度扫描
        ///// </summary>
        //void ScanTexWidth() 
        //{
        //    List<int> contents = new List<int>();
        //    int imageWidth; string combinedString;
        //    for (int i = 0; i < 239; i++)
        //    {
        //        Texture2D tmpMP = TextureAnalyzer.LoadImageAndConvertToTexture2D(Application.dataPath + "/Textures/MM1/Map/" + i.ToString() + ".png");
        //        contents.Add(tmpMP.width/16);
        //    }
        //    combinedString = string.Join(",", contents);
        //    Debug.Log(combinedString);
        //}

        ///// <summary>
        ///// Trigger事件下自动函数引用(委托).注意:引擎对象默认在主线程创建,所以涉及它们的动作在子线程要回调使用,不然报错！
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void TestFunc(object sender, EventArgs e) { }

        ///// <summary>
        ///// 注册到子线程的动作(处理240个纹理文本的读取)
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void Trigger_LoadTXT_Func(object sender, EventArgs e)
        //{
        //    string filePath;
        //    string tempContent = File.ReadAllText(Application.dataPath + "/Resources/MapIndex/World.txt");//打包后自己放入的路径
        //    CPEngine.mapContents = new List<string>[240];
        //    // 初始化数组中的每个List元素
        //    for (int i = 0; i < CPEngine.mapContents.Length; i++)
        //    {
        //        CPEngine.mapContents[i] = new List<string>();
        //    }
        //    string[] fields = tempContent.Split(',');
        //    CPEngine.mapContents[0].AddRange(fields); //分割好的世界纹理ID放到数组0
        //    // 将字符串转换为ushort并存储到mapIDs数组中
        //    for (int i = 0; i < fields.Length; i++)
        //    {
        //        // 使用ushort.Parse来转换字符串到ushort
        //        CPEngine.mapIDs[0][i] = ushort.Parse(fields[i]);
        //    }

        //    for (int i = 0; i <= 238; i++)
        //    {
        //        filePath = Application.dataPath + "/Resources/MapIndex/" + i + ".txt";
        //        tempContent = File.ReadAllText(filePath);
        //        fields = tempContent.Split(',');
        //        CPEngine.mapContents[i + 1].AddRange(fields); //239个小地图场景纹理ID存放在数组索引1~240
        //                                                      // 将字符串转换为ushort并存储到mapIDs数组中
        //        for (int j = 0; j < fields.Length; j++)
        //        {
        //            // 使用ushort.Parse来转换字符串到ushort
        //            CPEngine.mapIDs[i + 1][j] = ushort.Parse(fields[j]);
        //        }
        //    }
        //}
        #endregion

        #endregion
    }
}

// Unity脚本生命周期和组件
// 继承MonoBehaviour的类可以作为Unity组件挂载到游戏物体上,并在Unity编辑器中显示其公共非静态字段和属性(可通过Inspector面板编辑).
// 组件用法:Unity中随便新建一个空对象,Inspector面板把脚本拖到组件位置即挂载(自定义类要作为组件挂载必须一个cs文件一个类,且类名须与文件名一致).或代码直接给游戏物体添加组件.
// 私有或静态字段不被自动序列化到Inspetor,想要强制被序列化请使用[SerializeField]特性,想不被序列化请使用[NonSerialized]特性.
// Start、Update等生命周期方法是Unity控制脚本执行的关键,即使存在Awake但上述方法不存在时在编辑器界面不提供组件启禁用选项.
// AddComponent时,Awake方法会在添加的那一帧立即执行,但Start方法则在下一帧的Update之前执行‌.