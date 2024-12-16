#if UNITY_EDITOR|| UNITY_STANDALONE
//Unity编辑器、独立应用程序（不包括Web播放器）
using Vector2F = UnityEngine.Vector2;
using Vector3F = UnityEngine.Vector3;
#elif MonoGame
//使用VS2022的MonoGame插件框架
using Vector2F = Microsoft.Xna.Framework.Vector2;
using Vector3F = Microsoft.Xna.Framework.Vector3;
#else
using Vector2F = System.Numerics.Vector2;
using Vector3F = System.Numerics.Vector3;
#endif

namespace MetalMaxSystem
{
    /// <summary>
    /// 单位
    /// </summary>
    public class Unit
    {
        #region 字段

        //字段用于每个实例存储不同的值，这里带_字段表示其拥有属性方法（可进行安全修正）

        private Vector3F _vector3F;
        private Vector2F _vector2F;
        private double _carryWeight = 300.0;
        private double _carryWeightMax = 300.0;
        private double _terrainHeight = 0.0;
        private double _height = 1.8;
        private double _radius = 0.5;
        private double _selectedRadius = 0.5;

        #endregion

        #region 构造函数

        /// <summary>
        /// [构造函数]单位
        /// </summary>
        public Unit()
        {
            //创建新类时的初始化动作
            Tag = Game.CurrentUnitHandle + 1;
            Game.UnitLastCreated = this;
        }
        /// <summary>
        /// [构造函数]单位
        /// </summary>
        public Unit(string name)
        {
            //创建新类时的初始化动作
            Tag = Game.CurrentUnitHandle + 1;
            Game.UnitLastCreated = this;
            Name = name;
        }

        #endregion

        #region 属性方法

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public double Age { get; set; }
        /// <summary>
        /// 移动速度（每帧行动距离换算到每秒），由游戏内部针对当前帧速进行修改（如每帧50ms时，实际每帧移动距离=50*Speed/1000）
        /// </summary>
        public double Speed { get; set; }
        /// <summary>
        /// ASPD：物理攻击速度（每帧行动次数换算到每秒），受到多个因素同时影响如AGI、DEX、职业、武器、药水等（设计ASPD=100即每秒攻击5次，则每4帧攻击1次），由游戏内部针对当前帧速进行修改（如每帧50ms时，实际每次攻击等待毫秒数=50*ASPD/（100/5）=200ms）
        /// </summary>
        public double AttackSpeed { get; set; }
        /// <summary>
        /// STR：力量，影响物理攻击（每点加2）、负重上限（每点加30，按牛顿为单位约50~60个鸡蛋）、武器发挥（当STR很低时，武器攻击无法完全发挥，在STR为0时只能发挥25%），但不影响暴击和抵挡几率和每秒伤害输出DPS(Attack Power)
        /// 当用斧、书、剑、拳刃、拳套、钝器、矛等近战武器或空手时ATK +1、武器ATK+0.5%）仅对基础武器ATK有效，且强制无属性），当用弓、枪械、乐器、鞭等远程武器时ATK+0.2
        /// </summary>
        public double Strength { get; set; }
        /// <summary>
        /// VIT：耐力（体力），影响生命值上限和物理防御，每点增加HP上限30或1%、HP恢复+0.2、物理防御0.5~1、魔防0.2、HP恢复类道具效果+2%，每5点增加HP回复1
        /// 状态效果影响：中毒状态成功率-1%且持续时间减少，VIT≥100时免疫中毒状态，混乱状态成功率减少，昏迷状态成功率-1%且持续时间减少，VIT≥100时免疫昏迷状态，沉默状态成功率 -1%且持续时间-0.01秒，诅咒状态持续时间-0.01秒，霜冻状态成功率减少且持续时间-0.05秒，冷冻状态持续时间-0.1秒
        /// </summary>
        public double Vitality { get; set; }
        /// <summary>
        /// AGI：敏捷，每点增加攻击速度1%、闪避1、防御力0.2、致命一击或暴击
        /// 状态效果影响：出血状态成功率和持续时间减少，移动不可状态中部分陷阱类状态成功率和持续时间减少，灼烧状态成功率和持续时间减少
        /// </summary>
        public double Agility { get; set; }
        /// <summary>
        /// INT：智力，影响魔攻、魔法值和魔法致命一击几率，每点加魔攻2、魔防0.5~1、魔法值1%，每6点增Sp恢复速度（INT<120时，SP自然恢复 +0.6，INT≥120时，SP自然恢复+1.3），增至偶数时Sp上限加7、奇数时Sp上限加6，可变吟唱时间减少，恢复类道具效果+1%
        /// 状态效果影响：冰冻状态持续时间减少，对口职业睡眠状态成功率 -1%，持续时间减少，黑暗状态成功率减少，持续时间减少，恐惧状态成功率减少，持续时间减少，沉睡状态成功率减少，持续时间减少
        /// </summary>
        public double Intelligence { get; set; }
        /// <summary>
        /// DEX：灵巧，影响玩家命中和可变吟唱时间，每点加命中1，每3点减0.1秒可变咏唱时间并加物理攻击1，每5点加物理攻击1，每30点减1秒可变咏唱时间（游戏中有个重要名词叫“不可变吟唱时间”也就是说某个技能存在一个最低下限）
        /// 当用弓、枪械、乐器、鞭等远程武器时ATK+1、武器ATK+0.5%（仅对基础武器ATK有效且强制无属性），当用斧、书、剑、拳刃、拳套、钝器、矛等近战武器或空手时ATK+0.2、MATK+0.2、HIT+1、MDEF+1、可变吟唱时间减少、按比例增加ASPD
        /// 状态效果影响：装备卸除状态持续时间减少，霜冻状态成功率减少且持续时间-0.05秒
        /// </summary>
        public double Dexterity { get; set; }
        /// <summary>
        /// LUK：幸运（厄运为负），影响装备道具的出现率&探索发现率、暴击率、完全回避等，每点CRI+0.3、ATK+0.3、MATK+0.3、HIT+0.3、回避+0.2、完全回避+0.1、免暴率+1，每3点加暴击1，每5点加物理攻击1和防暴击1
        /// 状态效果影响：中毒状态成功率&持续时间减少，冰冻状态成功率减少，混乱状态成功率减少，石化状态成功率减少，睡眠状态成功率减少，昏迷状态成功率减少，持续时间减少，沉默状态成功率减少，诅咒状态成功率减少，黑暗状态成功率减少，沉睡状态成功率减少
        /// </summary>
        public double Luck { get; set; }
        /// <summary>
        /// Atk：物理攻击力，力量的主要相关因素以及受其他辅助因素影响
        /// </summary>
        public double Atk { get; set; }
        /// <summary>
        /// DEF：防御力，敏捷的主要相关因素以及受其他辅助因素影响
        /// </summary>
        public double Def { get; set; }
        /// <summary>
        /// Matk：魔法攻击力
        /// </summary>
        public double Matk { get; set; }
        /// <summary>
        /// Mdef ：魔法防御力
        /// </summary>
        public double Mdef { get; set; }
        /// <summary>
        /// CRI：暴击率，一般根据人物等级设计攻击力来计算（按一定比率采取反推方式完成）
        /// </summary>
        public double Critical { get; set; }
        /// <summary>
        /// 抗暴率，实际暴击率按抗暴率程度将暴击率消减再修正数值得到实际暴击率，简单点就直接相减
        /// </summary>
        public double AntiCritical { get; set; }
        /// <summary>
        /// Maspd：魔法攻击速度，MASPD受到多个因素同时影响：INT、AGI、DEX、职业、武器、药水等，其中敏捷和常规物理攻击速度关联较大，施法频率参考ASPD
        /// </summary>
        public double Maspd { get; set; }
        /// <summary>
        /// 生命值
        /// </summary>
        public double Hp { get; set; }
        /// <summary>
        /// 魔法点：ManaPoint，当低于所需时，角色无法使用相关武器装备和常规技能
        /// </summary>
        public double Mp { get; set; }
        /// <summary>
        /// 特殊技能点：当低于所需时，角色无法使用特殊技能
        /// </summary>
        public double Sp { get; set; }
        /// <summary>
        /// 计时阶段状态条（可有多个），用于单位工作或施法阶段进行计时等用途
        /// </summary>
        public double[] Tp { get; set; }
        /// <summary>
        /// 经验值：experience，积累到一定数量可提高等级、能力
        /// </summary>
        public double Exp { get; set; }
        /// <summary>
        /// 生命值恢复速度（每帧），延时恢复另详设计
        /// </summary>
        public double Hps { get; set; }
        /// <summary>
        /// 魔法点恢复速度（每帧），延时恢复另详设计
        /// </summary>
        public double Mps { get; set; }
        /// <summary>
        /// 技能点恢复速度（每帧），延时恢复另详设计
        /// </summary>
        public double Sps { get; set; }
        /// <summary>
        /// 工作、施法时间阶段的状态值恢复或倒计速度
        /// </summary>
        public double[] Tps { get; set; }
        /// <summary>
        /// 闪避
        /// </summary>
        public double Evasion { get; set; }
        /// <summary>
        /// 闪避率，其值=1 / (1 + 100 / 闪避面板数值)，允许超过100的原因是面对命中率时可扣减，并不是最终结果
        /// </summary>
        public double EvasionRate { get; set; }
        /// <summary>
        /// 完美闪避，此项仅受幸运影响
        /// </summary>
        public double PerfectEvasion { get; set; }
        /// <summary>
        /// 完美闪避率，根据完美闪避计算后修正得出，并用来参与结果
        /// </summary>
        public double PerfectEvasionRate { get; set; }
        /// <summary>
        /// 命中
        /// </summary>
        public double Hit { get; set; }
        /// <summary>
        /// 命中率
        /// </summary>
        public double HitRate { get; set; }
        /// <summary>
        /// 完美命中，此项仅受幸运影响
        /// </summary>
        public double PerfectHit { get; set; }
        /// <summary>
        /// 完美命中率，根据完美命中计算后修正得出，并用来参与结果
        /// </summary>
        public double PerfectHitRate { get; set; }
        /// <summary>
        /// 致命一击率，允许超过100的原因是遇对方有抗即死率时进行扣减，并不是最终结果
        /// </summary>
        public double KillRate { get; set; }
        /// <summary>
        /// 抗即死率，用来防御致命一击
        /// </summary>
        public double AntiKilledRate { get; set; }
        /// <summary>
        /// 单位负重
        /// </summary>
        public double CarryWeight
        {
            get
            {
                return _carryWeight;
            }

            set
            {
                _carryWeight = value;
            }
        }

        //↓属性上限

        public double AgeMax { get; set; }
        public double SpeedMax { get; set; }
        public double StrengthMax { get; set; }
        public double VitalityMax { get; set; }
        public double AgilityMax { get; set; }
        public double IntelligenceMax { get; set; }
        public double DexterityMax { get; set; }
        public double LuckMax { get; set; }
        public double AtkMax { get; set; }
        public double DefMax { get; set; }
        public double MatkMax { get; set; }
        public double MdefMax { get; set; }
        public double CriticalMax { get; set; }
        public double AntiCriticalMax { get; set; }
        public double MaspdMax { get; set; }
        public double HpMax { get; set; }
        public double MpMax { get; set; }
        public double SpMax { get; set; }
        public double[] TpMax { get; set; }
        public double ExpMax { get; set; }
        public double HpsMax { get; set; }
        public double MpsMax { get; set; }
        public double SpsMax { get; set; }
        public double[] TpsMax { get; set; }
        public double EvasionMax { get; set; }
        public double EvasionRateMax { get; set; }
        public double PerfectEvasionMax { get; set; }
        public double PerfectEvasionRateMax { get; set; }
        public double HitMax { get; set; }
        public double HitRateMax { get; set; }
        public double PerfectHitMax { get; set; }
        public double PerfectHitRateMax { get; set; }
        public double KillRateMax { get; set; }
        public double AntiKilledRateMax { get; set; }
        public double CarryWeightMax
        {
            get
            {
                return _carryWeightMax;
            }

            set
            {
                _carryWeightMax = value;
            }
        }

        //↓其他属性

        /// <summary>
        /// 单位标签（句柄）
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// 单位类型在编辑器的名字
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 单位脚底坐标向量(三维)，Z坐标是根据计算得到（Z=MapHeight+TerrainHeight+Unit.TerrainHeight），平时只要实时更新平面坐标即可根据该二维点高度信息更新3D高度
        /// </summary>
        public Vector3F Vector3F
        {
            get
            {
                return _vector3F;
            }

            set
            {
                _vector3F = value;
            }
        }

        /// <summary>
        /// 单位脚底坐标向量(二维)
        /// </summary>
        public Vector2F Vector2F
        {
            get
            {
                return _vector2F;
            }

            set
            {
                _vector2F = value;
            }
        }

        /// <summary>
        /// 单位层地形物件高度（区别地图层地形物件），其值使单位浮空或嵌入地面
        /// </summary>
        public double TerrainHeight
        {
            get
            {
                return _terrainHeight;
            }

            set
            {
                _terrainHeight = value;
            }
        }

        /// <summary>
        /// 单位高度，注意当鼠标划过或点击单位时返回的Z坐标是在其头顶的（mouseVectorZ=MapHeight+TerrainHeight+Unit.TerrainHeight+Unit.Height）
        /// </summary>
        public double Height
        {
            get
            {
                return _height;
            }

            set
            {
                _height = value;
            }
        }

        /// <summary>
        /// 单位碰撞检查半径，默认与模型半径一致，可修改每个单位的碰撞范围
        /// </summary>
        public double Radius
        {
            get
            {
                return _radius;
            }

            set
            {
                _radius = value;
            }
        }

        /// <summary>
        /// 鼠标点击单位时的选择及搜索范围
        /// </summary>
        public double SelectedRadius
        {
            get
            {
                return _selectedRadius;
            }

            set
            {
                _selectedRadius = value;
            }
        }

        /// <summary>
        /// 单位所有者的玩家编号
        /// </summary>
        public int Owner { get; set; }

        /// <summary>
        /// 单位控制者的玩家编号
        /// </summary>
        public int Controller { get; set; }

        /// <summary>
        /// 单位朝向角度
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// 单位活体状态
        /// </summary>
        public bool Alive { get; set; }

        #endregion

        #region 函数

        /// <summary>
        /// 发布指令
        /// </summary>
        /// <param name="inOrder"></param>
        /// <param name="inQueueType"></param>
        public void IssueOrder(Order inOrder, int inQueueType)
        {

        }

        /// <summary>
        /// 设置单位属性
        /// </summary>
        /// <param name="unitProp"></param>
        /// <param name="value"></param>
        public void SetProperty(UnitProp unitProp, double value)
        {

        }

        /// <summary>
        /// 返回单位的世界坐标点（二维）
        /// </summary>
        /// <returns></returns>
        public Vector2F GetPosition()
        {
            return Vector2F;
        }

        #endregion

    }
}
