using CellSpace;
using MetalMaxSystem.Unity;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SpriteSpace
{
    /// <summary>
    /// 资源组件.挂载到GameObject上,在编辑器中拖拽素材到对应字段,运行时会自动加载素材供全局使用.
    /// </summary>
    public class SpriteSpacePrefab : MonoBehaviour
    {
        /// <summary>
        /// 预制体字典.
        /// 当RuntimePrefab.Add(string key, Object obj,bool clone = false)中的clone参数为true时,资源为副本存储,
        /// 当clone参数为false时,直接存储对象,摧毁原对象会影响该字典内容,场景切换时未被DontDestroyOnLoad保护的实例会被Unity自动销毁‌,请做好保护.
        /// </summary>
        public static RuntimePrefab runtimePrefab = ScriptableObject.CreateInstance<RuntimePrefab>();
        /// <summary>
        /// 用来阻止挂组件时自动Awake一次.而Start、Update那些就不用阻止了,因为runtimePrefab不在场景,即便预制体上组件Enable也不起作用.
        /// </summary>
        public static Dictionary<string, bool> awakeEnable = new Dictionary<string, bool>();
        /// <summary>
        /// 预制体实例化后的父级GameObject.
        /// </summary>
        public static GameObject group;
        /// <summary>
        /// 预制体初始化是否完成.
        /// </summary>
        public static bool initialized;
        /// <summary>
        /// 外部素材目录.
        /// </summary>
        public static string externalAssetsPath;
        public static Material material;

        //内置精灵素材
        public static List<Sprite>[] Vehicle;
        public static List<Sprite>[] characters;
        public static List<Sprite>[] monsters;

        /// <summary>
        /// 预制体初始化方法.在使用SpriteSpacePrefab前必须调用此方法来确保预制体已被创建.
        /// </summary>
        public static void Init()
        {
            if (initialized) return;
            material = new Material(Shader.Find("Sprites/Default"));
            group = GameObject.Find("SpriteSpacePrefab") ?? new GameObject("SpriteSpacePrefab"); //创建存放SpriteSpace预制体实例的父级容器
            DontDestroyOnLoad(group);
            runtimePrefab.hideFlags = HideFlags.DontUnloadUnusedAsset; //资源持久化标记

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            LoadAssets();
            stopwatch.Stop();
            Debug.Log("LoadAllAssets" + $"耗时:{stopwatch.ElapsedMilliseconds}ms");

            //初始化底层绘制对象池(用于大量NPC、怪物等活动精灵个体对象复用GameObject,防止频繁创建摧毁导致掉帧问题)
            GameObject tempGroup = new GameObject("GOGroup");
            DontDestroyOnLoad(tempGroup);
            GO.Init(material, 20000, tempGroup);

            initialized = true;
        }

        /// <summary>
        /// 获取Sun预制体.作为单例直接使用.
        /// 如不存在,会创建名为"Sun"的游戏物体并添加Light组件.
        /// 首次创建的预制体不激活.
        /// </summary>
        public static GameObject Sun
        {
            get
            {
                string name = "Sun";
                if (!runtimePrefab.ContainsKey(name))
                {
                    GameObject tempGameObject = GameObject.Find(name);
                    if (tempGameObject == null)
                    {
                        tempGameObject = new GameObject(name);
                        tempGameObject.SetActive(false); //阻止后续添加组件时执行Awake以外的方法
                        tempGameObject.transform.SetPositionAndRotation(new Vector3(0f, 50f, 0f), Quaternion.Euler(60f, 28.5f, 90f));
                        tempGameObject.transform.parent = group.transform; //作为group的子物体"存放"
                        tempGameObject.AddComponent<Light>().type = LightType.Directional;
                        tempGameObject.GetComponent<Light>().intensity = 0.7f;
                        tempGameObject.GetComponent<Light>().range = 1f;
                        runtimePrefab.Add(name, tempGameObject); //存入预制体字典
                        //awakeEnable[name] = true; //Sun的组件无自定义部分,不需设计阻止或允许Awake
                        Debug.Log($"预制体已创建: {name}");
                    }
                    else
                    {
                        runtimePrefab.Add(name, tempGameObject); //存入预制体字典
                    }
                }
                return runtimePrefab.Get(name) as GameObject;
            }
        }

        #region 功能函数

        /// <summary>
        /// 读取素材的总动作
        /// </summary>
        public static void LoadAssets()
        {
            //读取余下资源
            LoadAllResources();
        }

        /// <summary>
        /// 读取所有资源(怪物、角色、载具的纹理和精灵切片)
        /// </summary>
        public static void LoadAllResources()
        {
            LoadAllVehicle();
            LoadAllCharacters();
            LoadAllMonsters();
        }

        /// <summary>
        /// 读取怪物纹理和精灵切片
        /// </summary>
        public static void LoadAllMonsters()
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
        public static void LoadAllCharacters()
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
        public static void LoadAllVehicle()
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

        #endregion

    }
}
