using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

// 用到MetalMaxSystem.Unity里写好的OP对象池(修正框架在老外原型时期采用频繁创建引起掉帧及大量摧毁造成GC压力).该对象池也可换Unity自带的,看个人习惯.
using MetalMaxSystem.Unity;
using CellSpace.Examples;

// 目前框架中网络功能是旧版UNet方法(Obsolete黄字警告),待禁用并升级(推荐用NetCode做一遍).

namespace CellSpace
{
    #region 枚举

    /// <summary>
    /// 网络模式
    /// </summary>
    public enum NetMode : ushort
    {
        /// <summary>
        /// 无网络
        /// </summary>
        none,
        /// <summary>
        /// UNet网络(旧版)
        /// </summary>
        unet,
        /// <summary>
        /// NetCode网络(新版)
        /// </summary>
        netCode,
        /// <summary>
        /// 其他网络
        /// </summary>
        other
    }

    /// <summary>
    /// 单元(体素)的6个面
    /// </summary>
    public enum Facing : ushort
    {
        /// <summary>
        /// 上面
        /// </summary>
        up,
        /// <summary>
        /// 下面
        /// </summary>
        down,
        /// <summary>
        /// 右面
        /// </summary>
        right,
        /// <summary>
        /// 左面
        /// </summary>
        left,
        /// <summary>
        /// 前面
        /// </summary>
        forward,
        /// <summary>
        /// 后面
        /// </summary>
        back
    }

    /// <summary>
    /// 朝向
    /// </summary>
    public enum Direction : ushort
    {
        /// <summary>
        /// 朝上
        /// </summary>
        up,
        /// <summary>
        /// 朝下
        /// </summary>
        down,
        /// <summary>
        /// 朝右
        /// </summary>
        right,
        /// <summary>
        /// 朝左
        /// </summary>
        left,
        /// <summary>
        /// 朝前
        /// </summary>
        forward,
        /// <summary>
        /// 朝后
        /// </summary>
        back
    }

    /// <summary>
    /// 透明度
    /// </summary>
    public enum Transparency : ushort
    {
        /// <summary>
        /// 实心
        /// </summary>
        solid,
        /// <summary>
        /// 半透明
        /// </summary>
        semiTransparent,
        /// <summary>
        /// 透明
        /// </summary>
        transparent
    }

    /// <summary>
    /// 碰撞类型
    /// </summary>
    public enum ColliderType : ushort
    {
        /// <summary>
        /// 立方体
        /// </summary>
        cube,
        /// <summary>
        /// 网格
        /// </summary>
        mesh,
        /// <summary>
        /// 无
        /// </summary>
        none
    }

    #endregion

    // 本框架魔改自Uniblocks项目(处理速度提升百倍),是一个体素(三维立体像素)空间引擎框架,可用于制作类似Minecraft的游戏或其他体素空间设计应用.
    // 本框架内"单元"均指体素块(默认边长为Unity世界坐标系的1.0长度),每种地块预制体实例占一个GameObject,创建后隐藏在对象池.空间形成时读预制体网格纹理等信息去计算并将结果赋值给组件以刷新.
    // 本框架内"团块"均指由多个单元组成的立体空间(顶点少则占1个GameObject否则会分摊给子物体以迎合Unity默认的16位Mesh缓冲区).
    // 团块用作2D平面使用时,边长默认为256即65536个单元,作3D设计时推荐16*16*16=4096个单元组成一个团块空间(更大会自动拆分故无意义,边长仅用来规定基础最小空间尺寸,周围要显示的空间数可自定义).
    // 每次创建新世界,其团块边长应在初始化前确认,存储为区域文件后就固定下来了,中途不可再改.
    // 所有已创建团块实例将被存储在团块管理器类的静态字段容器内,只有完整设计MC世界多团块时,距离过远团块数据从内存转移到硬盘(需开启框架的地块存储功能).
    // 地形生成器类只负责创建没探索过区域的团块,如果世界坐标转换的索引对应团块存在于团块管理器容器内或硬盘区域文件已存在该索引则转而使用已有数据刷地块(单元).

    /// <summary>
    /// 单元(体素)空间引擎.
    /// 地面空间框架的主入口类.该类设计为单例模式,请用CPEingine.Create()进行创建.
    /// </summary>
    public class CPEngine : MonoBehaviour
    {
        #region 字段、属性方法(I前缀表示可被覆写的接口方法,但注意下方小写L开头字段是用于编辑器GUI界面"接口属性"而非前者,脱离编辑器后该字段可用于接收外来数据并在初始化时统一赋值给游戏变量)

        // 启动前考虑:运行模式、要预填充的地块种类数)

        /// <summary>
        /// Unity编辑器交互模式.
        /// 决定框架中字段是否与互动窗口进行交互,并利用编辑器C#脚本组织的窗口完成预制体文件的创建及覆盖.
        /// 若框架独立于Unity编辑器外运行则该字段应置为false,且不需要用到预制体文件(改为程序生成).
        /// </summary>
        public static bool unityEditorInteraction = false;
        /// <summary>
        /// 没接收到外部预制体数据,从代码组装预制体时blocks数组要预填充的地块(单元)种类数.内置默认1675种(ID=0~1674),增加纹理后可进行修改.
        /// </summary>
        public static ushort blocksNum = 1675;
        /// <summary>
        /// CPEnigine组件实例的当前协程方法.用于控制启停.
        /// </summary>
        private static IEnumerator currentCoroutine;

        // 团块创建位置指示器、对象池、内置地图信息相关字段

        private static Transform _chunkCreationIndicator;
        /// <summary>
        /// 团块(空间)创建指示器.
        /// 用于指示空间自动化刷新的位置,可将玩家角色或其他游戏物体的Transform作为指示器.
        /// 单一空间复用模式下几乎用不到(因创建点无需移动且该模式都是手动刷块).
        /// 非单一空间复用时,指示器移动后地形生成器设计为根据位置自动计算场景索引,并将地块(单元)刷在不同位置,同时支持区域自动保存变化的场景.
        /// 地形生成器见CPTerrainGenerator类及其子类(必须作为组件实例来发挥作用,当前使用的是CPCustomTerrainGenerator挂在CellChunk,请自行修改).
        /// 注:当启用存储功能,根据已有区域文件刷新地形(单元)时不会再调用地形生成器按噪声种子或内置方法去生成.
        /// </summary>
        public static Transform ChunkCreationIndicator
        {
            get
            {
                return _chunkCreationIndicator;
            }
            set
            {
                _chunkCreationIndicator = value;
            }
        }

        // 下方静态字段变量有非静态的等价物(开头为小写L)是为了在Unity编辑器窗口中修改,脱离编辑器打包后这些小写L开头的字段应从配置文件读信息来用.
        // 如果是Unity编辑器模式进行开发,可通过菜单、场景中组件对象来编辑这些带小写L的等价物,并在Awake方法中,将结果初始化赋值给实际游戏变量.

        /// <summary>
        /// 预制体实例数组(OP结构体类型).内含静态声明的共享对象池,该对象池主要存储地块预制体.
        /// 以前没本字段时需编辑器GUI界面去创建地块预制体文件,当数量上千时拖拽填很没效率,因为大部分只是递增改下纹理ID,故转代码自动填充,用到此字段.
        /// </summary>
        public static OP[] prefabOPs;
        /// <summary>
        /// 内置地图的纹理ID字符列表.数组索引:0用于重装机兵大地图(纹理集在第2材质上),1~239为其小地图(纹理集在第3材质上);240作为预留,共241个.
        /// 在内置地图外可通过地图编辑器界面制作保存地图文本或直接改代码如整个地面空间全刷草地形成新场景数据而不用地图文本.
        /// </summary>
        public static List<string>[] mapContents = new List<string>[241];
        /// <summary>
        /// 内置地图的纹理ID列表.数组索引:0用于重装机兵大地图(纹理集在第2材质上),1~239为其小地图(纹理集在第3材质上);240作为预留,共241个.
        /// 在内置地图外可通过地图编辑器界面制作保存地图文本或直接改代码如整个地面空间全刷草地形成新场景数据而不用地图文本.
        /// 本列表中的元素即纹理ID用ushort类型(0~65535)是因为地块(单元)种类不考虑超过65536种.
        /// 本列表方便刷地块(单元)种类时直接交互读取,并不用来存储文件故不需要压缩转ushort为char(框架只在存储已探索的团块空间时才这么做).
        /// </summary>
        public static List<ushort>[] mapIDs = new List<ushort>[241];
        /// <summary>
        /// 存储全部内置地图的图片宽度信息的列表数组,供地形生成器用地图文本自动化刷地块时进行回行.
        /// </summary>
        public static List<ushort> mapWidths = new List<ushort>();

        // 创建世界时的团块(空间)边长应作为配置参数之一,边长无法中途更改因为整个框架已按此工作.

        /// <summary>
        /// (从GUI界面、配置文件输入)世界名称.
        /// </summary>
        public static string lWorldName = "DefaultWorld";
        /// <summary>
        /// 世界名称.
        /// 用于合成世界目录的路径(默认路径为/application_root/world_name/),可通过UpdateWorldPath函数来更改最终路径.
        /// 每个世界目录下存放有区域存档、世界种子及世界配置等文件.
        /// </summary>
        public static string worldName;
        /// <summary>
        /// (从GUI界面、配置文件输入)地块(单元)预制体文件的路径.
        /// Unity编辑器开发环境下用于块编辑器查找预制体的默认路径为"Assets\CellSpace\Res\Cells\".
        /// 当框架独立于Unity编辑器外制作开发如环世界Mod时,可留空不去加载预制体文件,改用CreatePrefab方法来创建预制体实例.
        /// </summary>
        public static string lBlocksPath;
        /// <summary>
        /// 地块(单元)预制体文件的路径.
        /// Unity编辑器开发环境下用于块编辑器查找预制体的默认路径为"Assets\CellSpace\Res\Cells\".
        /// 当框架独立于Unity编辑器外制作开发如环世界Mod时,可留空不去加载预制体文件,改用CreatePrefab方法来创建预制体实例.
        /// </summary>
        public static string blocksPath;
        /// <summary>
        /// 从GUI界面、配置文件输入的地块预制体小组.
        /// </summary>
        public static GameObject[] lBlocks;
        /// <summary>
        /// 团块(空间)刷新时实际使用的地块预制体实例小组.
        /// 空间形成期间会读取其上信息进行网格编织计算.数组索引对应于地块ID(单元预制体种类).
        /// </summary>
        public static GameObject[] blocks;
        /// <summary>
        /// 世界目录路径.由UpdateWorldPath函数确定其最终路径.
        /// </summary>
        public static string worldPath;
        /// <summary>
        /// 世界种子.用于程序化生成随机地形,种子存储在世界数据文件夹中
        /// </summary>
        public static int worldSeed;

        // 团块(空间)创建设置

        /// <summary>
        /// 高度范围.
        /// 控制团块(空间)能够创建的最大正负Y索引范围(以团块为单位),即团块自动创建时高度范围限值(如果是3,表示原始团块上下还可以产生3个团块).
        /// </summary>
        public static int heightRange;
        /// <summary>
        /// 创建距离.
        /// 控制团块(空间)诞生的水平距离(以团块为单位),是团块自动创建时的距离限制(如果是8,则始终在玩家原始团块周围保证有额外8范围的团块).
        /// 高度方向受heightRange限制.
        /// </summary>
        public static int chunkSpawnDistance;
        /// <summary>
        /// 团块边长(以单元为单位).
        /// </summary>
        public static int chunkSideLength;
        /// <summary>
        /// 团块自动摧毁时的判断距离.
        /// 如果是3,则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁.
        /// 摧毁时如启用存档,会从内存转硬盘,当玩家再次靠近会读取硬盘存档重新加载.
        /// </summary>
        public static int chunkDespawnDistance;
        /// <summary>
        /// 平地面模式设计时的地形高度(世界绝对坐标),在keepTerrainHeight=true时生效.
        /// 3D模式下,地形高度会被设计保持在这个值(低于这个高度的空间会整个刷满土块).
        /// 当然具体由代码决定要做什么,横版模式下和单层地形模式下无效.
        /// </summary>
        private static int _terrainHeight;
        /// <summary>
        /// 平地面模式设计时的地形高度(世界绝对坐标),在keepTerrainHeight=true时生效.
        /// 3D模式下,地形高度会被设计保持在这个值(低于这个高度的空间会整个刷满土块).
        /// 当然具体由代码决定要做什么,横版模式下和单层地形模式下无效.
        /// </summary>
        public static int TerrainHeight
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
        /// 最大地形高度(世界绝对坐标).
        /// 3D模式下,使地形高度不超过这个值,具体哪些地方要限制由代码决定,横版模式下和单层地形模式下无效.
        /// 注意heightRange只用来限制从指示器位置起算的上下高度范围,而本字段是绝对高度限制.
        /// </summary>
        private static int _maxTerrainHeight;
        /// <summary>
        /// 最大地形高度(世界绝对坐标).
        /// 3D模式下,使地形高度不超过这个值,具体哪些地方要限制由代码决定,横版模式下和单层地形模式下无效.
        /// 注意heightRange只用来限制从指示器位置起算的上下高度范围,而本字段是绝对高度限制.
        /// </summary>
        public static int MaxTerrainHeight
        {
            get
            {
                return _maxTerrainHeight;
            }

            set
            {
                _maxTerrainHeight = value;
            }
        }
        /// <summary>
        /// 单层地形模式下的平地面地形高度(团块内相对高度索引).
        /// 让程序仅在SingleChunkTerrainHeight高度处铺上一层地形.
        /// 仅3D模式下可用,横版模式下无效.
        /// </summary>
        private static int _singleChunkTerrainHeight;
        /// <summary>
        /// 单层地形模式下的平地面地形高度(团块内相对高度索引).
        /// 让程序仅在SingleChunkTerrainHeight高度处铺上一层地形.
        /// 仅3D模式下可用,横版模式下无效.
        /// 本属性控制地皮高度小于1则纠正为1,避免导致地形生成时出错(因单个团块空间内最底层格子的顶面高度是1,不能为0或负数)
        /// </summary>
        public static int SingleChunkTerrainHeight
        {
            get
            {
                if (_singleChunkTerrainHeight > chunkSideLength) { _singleChunkTerrainHeight = chunkSideLength; }
                if (_singleChunkTerrainHeight < 1) { _singleChunkTerrainHeight = 1; }
                return _singleChunkTerrainHeight;
            }

            set
            {
                _singleChunkTerrainHeight = value;
            }
        }

        // 从GUI界面、配置文件输入的团块创建设置

        /// <summary>
        /// (从GUI界面、配置文件输入)高度范围.
        /// 控制团块(空间)能够创建的最大正负Y索引范围(以团块为单位),即团块自动创建时高度范围限值(如果是3,表示原始团块上下还可以产生3个团块).
        /// </summary>
        public static int lHeightRange = 0;
        /// <summary>
        /// (从GUI界面、配置文件输入)创建距离.
        /// 控制团块(空间)诞生的水平距离(以团块为单位),是团块自动创建时的距离限制(如果是8,则始终在玩家原始团块周围保证有额外8范围的团块).
        /// 高度方向受heightRange限制.
        /// </summary>
        public static int lChunkSpawnDistance = 0;
        /// <summary>
        /// (从GUI界面、配置文件输入)团块边长(以单元为单位).
        /// </summary>
        public static int lChunkSideLength = 16;
        /// <summary>
        /// (从GUI界面、配置文件输入)团块自动摧毁时的判断距离.
        /// 如果是3,则团块会在距离玩家ChunkSpawnDistance+3个团块距离时进行摧毁.
        /// 摧毁时如启用存档,会从内存转硬盘,当玩家再次靠近会读取硬盘存档重新加载.
        /// </summary>
        public static int lChunkDespawnDistance = 0;

        // 纹理设置

        /// <summary>
        /// X方向纹理单元数量.
        /// 纹理表边长/纹理单元边长的倍率(用于计算单元纹理划区大小),形象说明的话相当于每个精灵在整张图片中的比例的倒数,填8则X方向均分8个纹理单元
        /// </summary>
        public static float[] textureUnitX;
        /// <summary>
        /// Y方向纹理单元数量.
        /// 纹理表边长/纹理单元边长的倍率(用于计算单元纹理划区大小),形象说明的话相当于每个精灵在整张图片中的比例的倒数,填8则Y方向均分8个纹理单元
        /// </summary>
        public static float[] textureUnitY;
        /// <summary>
        /// X方向对主纹理UV划区时方形单元内缩率.
        /// 满尺寸取纹理表上纹理时,纹理之间的填充不尽人意,故按内缩率取修正后的纹理UV.
        /// 每个纹理单元之间填充缝的大小=UV划区时向内缩进比例,填充亦可避免取到别的纹理单元.
        /// </summary>
        public static float texturePadX;
        /// <summary>
        /// Y方向对主纹理UV划区时方形单元内缩率.
        /// 满尺寸取纹理表上纹理时,纹理之间的填充不尽人意,故按内缩率取修正后的纹理UV.
        /// 每个纹理单元之间填充缝的大小=UV划区时向内缩进比例,填充亦可避免取到别的纹理单元.
        /// </summary>
        public static float texturePadY;

        // (从GUI界面、配置文件输入)纹理设置

        /// <summary>
        /// (从GUI界面、配置文件输入)X方向纹理单元数量.
        /// 纹理表边长/纹理单元边长的倍率(用于计算单元纹理划区大小),形象说明的话相当于每个精灵在整张图片中的比例的倒数,填8则X方向均分8个纹理单元
        /// </summary>
        public static float[] lTextureUnitX;
        /// <summary>
        /// (从GUI界面、配置文件输入)Y方向纹理单元数量.
        /// 纹理表边长/纹理单元边长的倍率(用于计算单元纹理划区大小),形象说明的话相当于每个精灵在整张图片中的比例的倒数,填8则Y方向均分8个纹理单元
        /// </summary>
        public static float[] lTextureUnitY;
        /// <summary>
        /// (从GUI界面、配置文件输入)X方向对主纹理UV划区时方形单元内缩率.
        /// 满尺寸取纹理表上纹理时,纹理之间的填充不尽人意,故按内缩率取修正后的纹理UV.
        /// 每个纹理单元之间填充缝的大小=UV划区时向内缩进比例,填充亦可避免取到别的纹理单元.
        /// </summary>
        public static float lTexturePadX = 0.01f;
        /// <summary>
        /// (从GUI界面、配置文件输入)Y方向对主纹理UV划区时方形单元内缩率.
        /// 满尺寸取纹理表上纹理时,纹理之间的填充不尽人意,故按内缩率取修正后的纹理UV.
        /// 每个纹理单元之间填充缝的大小=UV划区时向内缩进比例,填充亦可避免取到别的纹理单元.
        /// </summary>
        public static float lTexturePadY = 0.01f;

        // 平台设置

        /// <summary>
        /// 目标(预期)帧率.
        /// 非实际帧率,计时器记录每帧处理用时,超过它则可将动作放在下一帧继续步进,防止卡在这一帧(为了让外围团块慢慢生成),该值并非期望越高越好而是应尽量贴近实际.
        /// </summary>
        public static int targetFPS;
        /// <summary>
        /// 每帧的团块保存上限.
        /// 决定每帧保存团块的最大处理速率,用于跟ChunkManager当前帧团块已保存数量SavesThisFrame进行比对.
        /// </summary>
        public static int maxChunkSaves;
        /// <summary>
        /// 团块数据请求上限.
        /// 每个客户端一次可以在服务器中排队的最大团块数据请求数(0=无限制).
        /// 如客户端生成数据速度太快且服务器无法跟上数据请求速度时,那么可由服务器进行限制,也能避免服务器被攻击搞事.
        /// </summary>
        public static int maxChunkDataRequests;

        // (从GUI界面、配置文件输入)平台设置

        /// <summary>
        /// (从GUI界面、配置文件输入)目标(预期)帧率.
        /// 非实际帧率,计时器记录每帧处理用时,超过它则可将动作放在下一帧继续步进,防止卡在这一帧(为了让外围团块慢慢生成),该值并非期望越高越好而是应尽量贴近实际.
        /// </summary>
        public static int lTargetFPS = 60;
        /// <summary>
        /// (从GUI界面、配置文件输入)每帧的团块保存上限.
        /// 决定每帧保存团块的最大处理速率,用于跟ChunkManager当前帧团块已保存数量SavesThisFrame进行比对.
        /// </summary>
        public static int lMaxChunkSaves = 50;
        /// <summary>
        /// (从GUI界面、配置文件输入)团块数据请求上限.
        /// 每个客户端一次可以在服务器中排队的最大团块数据请求数(0=无限制).
        /// 如客户端生成数据速度太快且服务器无法跟上数据请求速度时,那么可由服务器进行限制,也能避免攻击服务器被攻击搞事.
        /// </summary>
        public static int lMaxChunkDataRequests;

        // 全局设置

        /// <summary>
        /// 网络模式.
        /// </summary>
        public static NetMode netMode = NetMode.unet;
        /// <summary>
        /// 伪3D模式.
        /// 用于设计2.5D模式,摄像机设置透视(即依然维持3D的广视角)并与地面呈一定角度(默认45°由pseudo3DAngle修改),且地面上其他对象Transform旋转值与该角度同步.
        /// 本字段默认值为false,请按需启用.
        /// </summary>
        [MetalMaxSystem.Note("待设计")] public static bool pseudo3DMode = false;
        /// <summary>
        /// 伪3D角度.
        /// pseudo3DMode = true时,摄像机与地面须呈一定角度,该值为摄像机与地面之间的夹角(单位:度),默认45°.
        /// 地面上角色等对象要与镜头视角正交可改其Transform旋转值与镜头角度同步.
        /// </summary>
        public static float pseudo3DAngle = 45f;
        /// <summary>
        /// (3D)单层地形模式.
        /// 横版模式下失效.首次创建空间时地形生成器会根据指示器位置计算索引对应场景并自动铺在当前空间的指定高度层.
        /// 仅生效于地形首次初始化且不从区域文件读取时,每个空间仅在其相对高度(SingleChunkTerrainHeight)创建一层平地面.
        /// 用于重装机兵等格子游戏2D单层平地面复刻,但依然使用框架完整3D功能,设计时控制地图铺设在X-Z平面上.
        /// 当SingleLayerTerrainMode = true时,探索的任何新场景,其团块(空间)会创建在世界绝对坐标的新位置(按区域文件排满10^3个空间,X+方向先排10个然后Y+然后Z+,第1001个排到第二个区域),存储时每个区域对应一份硬盘区域存档,
        /// 启用后如果keepSingleChunkTerrainHeight=false,则默认仅在空间团底部建立一层地形,否则按SingleChunkTerrainHeight执行.代表特殊地图的团块(空间)建议创建在负的(未用的)坐标轴象限.
        /// 本字段默认值为false,请按需启用.
        /// </summary>
        public static bool singleLayerTerrainMode = true;
        /// <summary>
        /// 保持单层地形高度(团块内相对高度).
        /// (3D)地形首次初始化且不从区域文件读取时,控制每个空间仅在其相对高度(SingleChunkTerrainHeight)创建一层平地面.
        /// (2D)横版模式下无效,仅用于(3D)单层地形模式.为false时,地形生成器仅在每个空间的底部创建一层地形.
        /// </summary>
        public static bool keepSingleChunkTerrainHeight = false;
        /// <summary>
        /// (2D)横版模式.
        /// 启用时地面使用XY平面坐标系(正高度变成Z-),关闭时为3D模式设计采用XZ平面(默认正高度Y+).
        /// 横版模式默认取消了Z轴延伸,该轴只留最大1个单元体素块(以左下为原点)插入在Z=0处的(pixelX,pixelY)索引点.
        /// 可在CellChunkMeshCreator类中修改具体要显示的体素块的面,横版模式默认仅创建体素块的back面(透过屏幕直接看到的面是方块的背面).
        /// 横版模式探索的任何新场景,其团块(空间)会创建在世界绝对坐标的新位置(按X+方向排满10个便算1个区域,目前的设计中Y+Z+方向暂不考虑创建空间,所以第11个会排到第二区域),一区域一存储文件,
        /// 特殊场景代表的团块(空间)建议创建在负的(未用的)坐标X轴.启用后如果OneSapceMode为false那么地形生成器会按指示器位置索引计算的地图ID进行自动刷新,否则需另行设计刷什么地图.
        /// 本字段默认值为false,请按需启用.
        /// </summary>
        public static bool horizontalMode = false;
        /// <summary>
        /// (2D)多维横版(立体渲染模式,仅供开发测试).
        /// 在horizontalMode = true 即(2D)横版模式下生效.
        /// 启用后,横版模式下将体素块背向屏幕的面也永久创建显示,但上下左右仍要按CheckAdjacent来判断是否封闭.
        /// 若不启用,横版模式下仅创建体素块的back面(即朝向屏幕的面).
        /// 更多细节处理应在逻辑脚本中预设好.本字段默认值为false,请按需启用.
        /// </summary>
        public static bool mutiHorizontal = false;
        /// <summary>
        /// 平地面模式.用于(3D)保持地形高度(世界绝对坐标).
        /// 3D模式下地形高度会被设计保持在这个值(低于这个高度的空间会整个刷满土块).
        /// (2D)横版模式下和(3D)单层地形模式下无效.本字段默认值为false,请按需启用.
        /// </summary>
        public static bool keepTerrainHeight = false;
        /// <summary>
        /// (3D)限制最大地形高度(世界绝对坐标).
        /// 3D模式下地形高度会被限制在MaxTerrainHeight.
        /// (2D)横版模式下和(3D)单层地形模式下无效.本字段默认值为false,请按需启用.
        /// </summary>
        public static bool LimitMaxTerrainHeight = false;
        /// <summary>
        /// 保持至少1个团块(空间).
        /// 创建团块时即使创建范围使得任何团块索引都无效,至少在指示器或世界原点位置插入一个团块.
        /// </summary>
        public static bool keepOneChunk = true;
        /// <summary>
        /// 单一空间复用模式.
        /// 强制复用同一团块(空间),无论3D还是(2D)横版模式,此模式的理念是制作游戏时所有地图都反复刷在位置指示器所指空间,指示器往往不再需要移动.
        /// 开启此模式不会限制指示器移动,可设计指示器始终在(0,0,0)点,也可不限定指示器移动仅无论空间在哪始终在当前空间刷指定地图.
        /// 注意:该设计模式下无法利用框架区域文件自动存储功能,因为同一空间不断变化即便开启该功能也只会被覆写.
        /// 本字段启用时建议关闭框架存储功能,改为重新定制存储方案.本字段默认值为false.
        /// </summary>
        public static bool OneSapceMode = false;
        /// <summary>
        /// 单元的侧面可见(前提是没有与它们接壤的团块实例).
        /// 启用后对于邻块不存在情况CheckAdjacent()函数总是返回真而不用对比面朝向方向是否有邻块.
        /// 对于邻块存在则检查其透明度:
        /// 1)透明,若其相邻体素也透明返回假(禁止在一个完全透明体素块旁边绘制另一个透明块),否则真(允许在实体或半透明旁边画一个透明的块)；
        /// 2)非完全透明情况,若其相邻体素是固态返回假(禁止在实体体素块旁边画实体或半透明体素块),否则真(允许在透明和半透明体素块旁绘制一个实心或半透明体素块).
        /// </summary>
        public static bool showBorderFaces;
        /// <summary>
        /// 产生碰撞体(为false则团块将不会生成任何碰撞体).
        /// 碰撞体使用渲染网格,当然渲染网格可能是自定义如门的形状,否则都是方形.
        /// </summary>
        public static bool generateColliders;
        /// <summary>
        /// 发送镜头注视事件(如果为true, CameraEventsSender组件将把事件发送到主摄像机视场中心指着的单元).
        /// </summary>
        public static bool sendCameraLookEvents;
        /// <summary>
        /// 发送鼠标指针事件(如果为true, CameraEventsSender组件将将把事件发送到当前鼠标光标指着的单元).
        /// </summary>
        public static bool sendCursorEvents;
        /// <summary>
        /// 允许多人模式.
        /// 团块将从服务器请求单元数据而不是从硬盘加载,另外Cell.ChangeBlock、Cell.PlaceBlock和Cell.DestroyBlock会将单元变化发送到服务器以便重新分发给其他连接的玩家.
        /// </summary>
        public static bool enableMultiplayer;
        /// <summary>
        /// 用于确定网络同步轨道位置的处理方式.服务器检查玩家的位置以确定是否需要将单元更改发送给该玩家,客户端则会通过ChunkLoader脚本向服务器发送一个玩家位置更新.
        /// 在多人游戏中,通常需要将物体的位置同步到其他客户端.轨道位置是指物体沿着一条路径或轨道移动时所处的位置.
        /// 如果将MultiplayerTrackPosition字段设置为true,则表示该物体的位置将在网络上进行同步,且每个客户端都将跟踪该物体的轨道位置.
        /// 如果将MultiplayerTrackPosition字段设置为false,则表示该物体的位置不会在网络上进行同步,而客户端将不会跟踪其轨道位置.
        /// 在具有大量移动物体的多人游戏中,使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能.
        /// 例如,某个物体在场景中静止不动,则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步.
        /// </summary>
        public static bool multiplayerTrackPosition;
        /// <summary>
        /// 保存单元数据.为false则团块将不会互动硬盘区域文件及保存地块(单元)数据.
        /// </summary>
        public static bool saveCellData;
        /// <summary>
        /// 产生网格.这决定了团块(空间)是否会生成可见的网格.
        /// </summary>
        public static bool generateMeshes;

        // (从GUI界面、配置文件输入)全局设置

        /// <summary>
        /// (从GUI界面、配置文件输入)单元的侧面可见(前提是没有与它们接壤的团块实例).
        /// 启用后对于邻块不存在情况CheckAdjacent()函数总是返回真而不用对比面朝向方向是否有邻块.
        /// 对于邻块存在则检查其透明度:
        /// 1)透明,若其相邻体素也透明返回假(禁止在一个完全透明体素块旁边绘制另一个透明块),否则真(允许在实体或半透明旁边画一个透明的块)；
        /// 2)非完全透明情况,若其相邻体素是固态返回假(禁止在实体体素块旁边画实体或半透明体素块),否则真(允许在透明和半透明体素块旁绘制一个实心或半透明体素块).
        /// </summary>
        public static bool lShowBorderFaces;
        /// <summary>
        /// (从GUI界面、配置文件输入)产生碰撞体(为false则团块将不会生成任何碰撞体).
        /// 碰撞体使用渲染网格,当然渲染网格可能是自定义如门的形状,否则都是方形.
        /// </summary>
        public static bool lGenerateColliders;
        /// <summary>
        /// (从GUI界面、配置文件输入)发送镜头注视事件(如果为true, CameraEventsSender组件将把事件发送到主摄像机视场中心指着的单元)
        /// </summary>
        public static bool lSendCameraLookEvents;
        /// <summary>
        /// (从GUI界面、配置文件输入)发送鼠标指针事件(如果为true, CameraEventsSender组件将将把事件发送到当前鼠标光标指着的单元)
        /// </summary>
        public static bool lSendCursorEvents;
        /// <summary>
        /// (从GUI界面、配置文件输入)允许多人模式.
        /// 团块将从服务器请求单元数据而不是从硬盘加载,另外Cell.ChangeBlock、Cell.PlaceBlock和Cell.DestroyBlock会将单元变化发送到服务器以便重新分发给其他连接的玩家.
        /// </summary>
        public static bool lEnableMultiplayer;
        /// <summary>
        /// (从GUI界面、配置文件输入)用于确定网络同步轨道位置的处理方式.服务器检查玩家的位置以确定是否需要将单元更改发送给该玩家,客户端则会通过ChunkLoader脚本向服务器发送一个玩家位置更新.
        /// 在多人游戏中,通常需要将物体的位置同步到其他客户端.轨道位置是指物体沿着一条路径或轨道移动时所处的位置.
        /// 如果将MultiplayerTrackPosition字段设置为true,则表示该物体的位置将在网络上进行同步,且每个客户端都将跟踪该物体的轨道位置.
        /// 如果将MultiplayerTrackPosition字段设置为false,则表示该物体的位置不会在网络上进行同步,而客户端将不会跟踪其轨道位置.
        /// 在具有大量移动物体的多人游戏中,使用MultiplayerTrackPosition字段可以减少网络通信量并提高性能.
        /// 例如,某个物体在场景中静止不动,则将其MultiplayerTrackPosition字段设置为false可以避免不必要的网络同步.
        /// </summary>
        public static bool lMultiplayerTrackPosition;
        /// <summary>
        /// (从GUI界面、配置文件输入)保存单元数据.为false则团块将不会互动硬盘区域文件及保存地块(单元)数据.
        /// </summary>
        public static bool lSaveCellData;
        /// <summary>
        /// (从GUI界面、配置文件输入)产生网格.这决定了团块(空间)是否会生成可见的网格.
        /// </summary>
        public static bool lGenerateMeshes;

        // 超时处理

        /// <summary>
        /// 团块超时.
        /// 如一团块通过ChunkManager.SpawnChunk动作创建,但在ChunkTimeout这段时间内没有被访问,它将被销毁(在保存它的单元数据到硬盘后,从内存排泄).
        /// 来自客户端的单元数据请求和团块中的单元变化将重置计时器.当值为0将禁用此功能.
        /// </summary>
        public static float chunkTimeout;
        /// <summary>
        /// (从GUI界面、配置文件输入)团块超时.
        /// 如一团块通过ChunkManager.SpawnChunk动作创建,但在ChunkTimeout这段时间内没有被访问,它将被销毁(在保存它的单元数据到硬盘后,从内存排泄).
        /// 来自客户端的单元数据请求和团块中的单元变化将重置计时器.当值为0将禁用此功能.
        /// </summary>
        public static float lChunkTimeout;
        /// <summary>
        /// 允许团块超时.
        /// 若Engine.chunkTimeout>0则此变量会自动设为true.
        /// </summary>
        public static bool enableChunkTimeout;

        // 重要组件或物体对象

        /// <summary>
        /// 处理网络通信和同步的游戏物体对象.详见预制体"CPNetwork",是由空GameObject挂载Client和Server组件脚本形成,用于UNet网络(旧版).
        /// </summary>
        public static GameObject network;
        /// <summary>
        /// CPEngine组件实例
        /// </summary>
        private static CPEngine _instance;
        /// <summary>
        /// 团块管理器组件实例
        /// </summary>
        public static CellChunkManager chunkManagerInstance;

        // 其他

        /// <summary>
        /// 团块边长的平方.
        /// </summary>
        public static int squaredSideLength;
        /// <summary>
        /// 团块预制体大小(缩放比例).
        /// 默认各维度1.0,即1米边长的立方体(Unity绝对世界坐标系尺寸).
        /// </summary>
        public static Vector3 chunkScale;
        /// <summary>
        /// CPEngine初始化状态.
        /// </summary>
        public static bool initialized;
        /// <summary>
        /// 团块创建指示器当前的位置.
        /// </summary>
        public static CPIndex currentPos;
        /// <summary>
        /// 团块创建指示器最后的位置.
        /// </summary>
        public static CPIndex lastPos;

        #endregion

        #region 框架函数部分(尽量写静态方法)

        /// <summary>
        /// 私有化构造函数.确保外部无法通过new实例化本类.
        /// </summary>
        private CPEngine() { }
        /// <summary>
        /// 静态构造函数.在静态字段初始化赋值后补充执行.
        /// </summary>
        static CPEngine()
        {
            CPEngine.lTextureUnitX = new float[4];
            CPEngine.lTextureUnitX[0] = 8;
            CPEngine.lTextureUnitX[1] = 8;
            CPEngine.lTextureUnitX[2] = 8;
            CPEngine.lTextureUnitX[3] = 8;
            CPEngine.lTextureUnitY = new float[4];
            CPEngine.lTextureUnitY[0] = 8;
            CPEngine.lTextureUnitY[1] = 19;
            CPEngine.lTextureUnitY[2] = 170;
            CPEngine.lTextureUnitY[3] = 19;
        }

        /// <summary>
        /// 获取CPEngine组件实例.
        /// 不存在时函数会创建名为"CPEngine"的游戏物体并添加CPEngine、CellChunkManager、CPConnectionInitializer组件.
        /// (Read Only)
        /// </summary>
        /// <returns>返回CPEngine组件实例</returns>
        public static CPEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    CellSpacePrefab.Init();
                    _instance = CellSpacePrefab.CPEngine.GetComponent<CPEngine>();
                }
                return _instance;
            }
        }
        /// <summary>
        /// 创建CPEngine.屏蔽了new方法,推荐用本方法或Instance属性方法进行CPEngine组件实例的创建和获取.
        /// </summary>
        /// <returns>返回CPEngine组件实例</returns>
        public static CPEngine Create()
        {
            return Instance;
        }
        /// <summary>
        /// 初始化.组件运行前进行检查、外部字段赋值给游戏变量等最终的初始化操作.
        /// 若组件对象不存在则执行预制体初始化以确保完整性,结束后CPEngine.initialized = true;
        /// </summary>
        public static void Init()
        {
            if (Instance != null && CellSpacePrefab.awakeEnable.ContainsKey("CPEngine"))
            {
                #region 将GUI界面或从配置文件接收的数据赋值给实际运作字段

                //注意某些字段在脱离Unity编辑器后会加载不到如预制体路径和预制体,此时根据外部路径素材由代码组装预制体

                worldName = lWorldName; //读取GUI界面或从配置文件接收的世界名称,赋值给游戏用字段变量
                blocksPath = lBlocksPath;
                //开始对blocks进行填充
                if (lBlocks == null)
                {
                    //如果没有接收到预制体,从代码组装预制体
                    BlocksPreFill(blocksNum);
                }
                else
                {
                    blocks = lBlocks; //编辑器GUI界面拖拽到Engine的Cell预制体(只拖拽了一部分,剩下太多了后续也会用代码追加)
                }
                targetFPS = lTargetFPS;
                maxChunkSaves = lMaxChunkSaves;
                maxChunkDataRequests = lMaxChunkDataRequests;
                textureUnitX = lTextureUnitX;
                textureUnitY = lTextureUnitY;
                texturePadX = lTexturePadX;
                texturePadY = lTexturePadY;
                generateColliders = lGenerateColliders;
                showBorderFaces = lShowBorderFaces;
                enableMultiplayer = lEnableMultiplayer;
                multiplayerTrackPosition = lMultiplayerTrackPosition;
                saveCellData = lSaveCellData;
                generateMeshes = lGenerateMeshes;
                chunkSpawnDistance = lChunkSpawnDistance;
                heightRange = lHeightRange;
                chunkDespawnDistance = lChunkDespawnDistance;
                sendCameraLookEvents = lSendCameraLookEvents;
                sendCursorEvents = lSendCursorEvents;
                chunkSideLength = lChunkSideLength;
                squaredSideLength = chunkSideLength * chunkSideLength;

                #endregion

                //更新世界存档路径
                UpdateWorldPath();
                //刷新地块数据
                BlocksRefresh();

                //建立已加载的区域组(字典<string, string[]>)
                CellChunkDataFiles.LoadedRegions = new Dictionary<string, string[]>();
                //建立临时团块数据组(字典<string, string>)
                CellChunkDataFiles.TempChunkData = new Dictionary<string, string>();

                //如GUI界面输入的lChunkTimeout<= 0.00001,则不允许团块处理超时,否则允许超时并将lChunkTimeout赋值给游戏逻辑频繁互动用的属性字段
                if (lChunkTimeout <= 0.00001f)
                {
                    enableChunkTimeout = false;
                }
                else
                {
                    enableChunkTimeout = true;
                    chunkTimeout = lChunkTimeout;
                }

#if UNITY_WEBPLAYER
            //当前是WebPlayer,本地化存储应取消
            lSaveVoxelData = false;
            SaveVoxelData = false;
#else
                //当前不是WebPlayer
#endif

                //遮罩层设置

                //如果26层名不为空则输出警告
                if (LayerMask.LayerToName(26) != "" && LayerMask.LayerToName(26) != "CellSpaceNoCollide")
                {
                    Debug.LogWarning("CellSpace: Layer 26 is reserved for CellSpace, it is automatically set to ignore collision with all layers." +
                                     "第26层是为CellSpace保留的,它被自动设置为忽略与所有图层的碰撞！");
                }
                for (int i = 0; i < 31; i++)
                {
                    //Unity有32个可用的层0~31,此处设置第i~26之间的对象不发生碰撞(亦能穿过彼此而无碰撞事件)
                    Physics.IgnoreLayerCollision(i, 26);
                }

                #region 检查团块

                //检查地形单元种类计数
                if (blocks.Length < 1)
                {
                    Debug.LogError("CellSpace: The blocks array is empty! Use the Block Editor to update the blocks array." +
                        "单元是空的！使用块编辑器来更新！");
                    Debug.Break();
                }

                //检查第一个地形单元(空块)是否存在,如不存在或没有单元组件则报错
                if (blocks[0] == null)
                {
                    Debug.LogError("CellSpace: Cannot find the empty block prefab (id 0)!" +
                        "找不到空块预制体(id 0)！");
                    Debug.Break();
                }
                else if (blocks[0].GetComponent<Cell>() == null)
                {
                    Debug.LogError("CellSpace: Empty block prefab (id 0) does not have the Cell component attached!" +
                        "空块预制体(id 0)没有单元组件");
                    Debug.Break();
                }

                #endregion

                #region 检查设置

                //检查团块边长(至少为1才有效)
                if (chunkSideLength < 1)
                {
                    Debug.LogError("CellSpace: CellChunk side length must be greater than 0!" +
                        "团块边长必须大于0");
                    Debug.Break(); //暂停编辑器运行
                }

                //如果团块生成距离<1则被置为0(不再生成),MC玩法默认8即可,单一空间玩法可设置0(表示四周的邻居团块均不创建)
                if (chunkSpawnDistance < 1)
                {
                    chunkSpawnDistance = 0;
                    if (keepOneChunk == false)
                    {
                        Debug.LogWarning("CellSpace: CellChunk spawn distance is 0." + "团块生成距离为0且KeepOneChunk=假,无法生成团块(空间)！");
                    }
                }

                //如果高度范围小于0,则高度范围将被置为0,默认是3
                if (heightRange < 0)
                {
                    heightRange = 0;
                    Debug.LogWarning("CellSpace: CellChunk height range can'transform be a negative number! Setting chunk height range to 0." +
                        "团块高度范围不能是一个负数！已被重置为0");
                }

                //检查团块数据请求上限
                if (maxChunkDataRequests < 0)
                {
                    maxChunkDataRequests = 0;
                    Debug.LogWarning("CellSpace: Max chunk data requests can'transform be a negative number! Setting max chunk data requests to 0." +
                        "团块数据请求上限不能是负数！已被重置为0");
                }

                #endregion

                //检查材质
                int materialCount = CellChunkManager.ChunkPrefab.GetComponent<Renderer>().sharedMaterials.Length - 1; //(额外)材质计数=团块预制体渲染组件的共享材质数量-1

                //检查全部blocks
                for (ushort i = 0; i < blocks.Length; i++)
                {
                    if (blocks[i] != null)
                    {
                        //获取单元
                        Cell cell = blocks[i].GetComponent<Cell>();

                        //如果单元子网格索引<0则报错
                        if (cell.VSubmeshIndex < 0)
                        {
                            Debug.LogError("CellSpace: Cell " + i + " has a material index lower than 0! Material index must be 0 or greater." +
                                "单元的材质索引小于0！必须大于等于0");
                            Debug.Break();
                        }

                        //如单元子网格索引大于(额外)材质计数则报错(使用自定义的额外材质索引后没给它上材质)
                        if (cell.VSubmeshIndex > materialCount)
                        {
                            //单元使用了GUI界面输入中自定义材质索引,但团块预制体只有(额外)材质计数+1个材质附着,设置一个更低的材质索引或附着更多材质到团块预制体！
                            Debug.LogError("CellSpace: Cell " + i + " uses material index " + cell.VSubmeshIndex + ", but the chunk prefab only has " + (materialCount + 1) + " material(s) attached. Set a lower material index or attach more materials to the chunk prefab.");
                            Debug.Break();
                        }
                    }
                }

                //质量配置,检查抗锯齿功能,关闭后可防止边缘混叠和视觉缝隙,应默认关闭
                if (QualitySettings.antiAliasing > 0)
                {
                    Debug.LogWarning("CellSpace: Anti-aliasing is enabled. This may cause seam lines to appear between blocks. If you see lines between blocks, try disabling anti-aliasing, switching to deferred rendering path, or adding some texture padding in the engine settings." +
                        "启用了抗锯齿,这可能导致在块之间出现接缝线！如果你看到块之间的线条,试着禁用抗锯齿,切换到延迟渲染路径,或者在引擎设置中添加一些纹理填充.");
                }

                //引擎初始化完成,记录状态
                initialized = true;
            }
        }
        /// <summary>
        /// 激活CPEngine组件上的游戏物体.
        /// 若未激活则将其设为激活状态,若已激活则什么也不干.
        /// 本函数会执行组件完整性检查,若组件对象不存在则执行预制体初始化以确保完整性后再激活.
        /// 本函数在同步线程反复调用不会产生问题,异步反复调用前应先手动调用Init()确保CPEngine.initialized = true,否则会造成重复初始化.
        /// 若不希望执行组件完整性检查,仅激活/禁用CPEngine的游戏物体,可使用带参数的Active(true/false)变体方法.
        /// </summary>
        public static void Active()
        {
            if (!initialized)
            {
                Init();
            }

            if (!CPEngine.Instance.gameObject.activeSelf) { CPEngine.Instance.gameObject.SetActive(true); }
        }
        /// <summary>
        /// 激活/禁用CPEngine的游戏物体.不执行组件完整性检查.
        /// </summary>
        /// <param name="torf">激活与否</param>
        public static void Active(bool torf)
        {
            CPEngine.Instance.gameObject.SetActive(torf);
        }

        /// <summary>
        /// 启动协程按间隔执行地形空间团块的动态更新.用法: CPEngine.Instance.Run();
        /// </summary>
        /// <param name="repeatRate">默认0.0625f</param>
        /// <param name="time">默认0f</param>
        public void Run(float repeatRate = 0.0625f, float time = 0f)
        {
            currentCoroutine = RepeatCoroutine(time, repeatRate);
            StartCoroutine(currentCoroutine);
        }
        /// <summary>
        /// 停止刷图.用法: CPEngine.Instance.Stop();
        /// </summary>
        public void Stop()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
        }
        /// <summary>
        /// 按间隔执行地形空间团块的动态更新.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static IEnumerator RepeatCoroutine(float delay, float interval)
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                CPEngine.Tick();
                yield return new WaitForSeconds(interval);
            }
        }
        /// <summary>
        /// 运行时控制器.每调用一次便处理一次地形空间团块的动态更新.
        /// </summary>
        public static void Tick()
        {
            //刷地图前保证前置核心组件已经初始化完成
            if (!CPEngine.initialized || !CellChunkManager.Initialized) { return; }
            //如果多人模式已启用但连接尚未建立则不加载团块
            if (CPEngine.enableMultiplayer) { if (!Network.isClient && !Network.isServer) { return; } }

            if (CPEngine.ChunkCreationIndicator == null)
            {
                Debug.LogWarning("ChunkCreationIndicator is null, setting it to this CPEngine's transform.");
                CPEngine.ChunkCreationIndicator = CPEngine.Instance.gameObject.transform; //没有设置团块创建指示器则用当前CPEngine组件的Transform作为指示器
            }
            TickFunc(CPEngine.ChunkCreationIndicator.position);
        }
        /// <summary>
        /// 运行时控制器.每调用一次便处理一次地形空间团块的动态更新.
        /// </summary>
        /// <param name="position">默认使用团块创建指示器的位置(CPEngine.ChunkCreationIndicator.position),可自行更换</param>
        public static void TickFunc(Vector3 position)
        {
            #region 其他情况
            //跟踪人物当前所在团块位置索引(先在originalTransform创建,等控制人行走后换playerTransform)
            //if (scene.enabled && scene.player != null && scene.player.go.gameObject != null)
            //{
            //    currentPos = CPEngine.PositionToChunkIndex(scene.player.go.gameObject.transform.position);
            //    //Debug.Log("Player Position: " + currentPos.x + " " + currentPos.y + " " + currentPos.z);
            //}
            //else
            //{
            //    currentPos = CPEngine.PositionToChunkIndex(chunkCreationIndicator.position);
            //}

            //让CPEngine.OneSapceMode控制团块创建位置指示器始终在世界坐标零点
            //if (CPEngine.OneSapceMode == false)
            //{
            //    currentPos = CPEngine.PositionToChunkIndex(chunkCreationIndicator.position);
            //}
            //else { currentPos = CPIndex.Zero; }
            #endregion

            currentPos = CPEngine.PositionToChunkIndex(position);
            if (currentPos.IsEqual(lastPos) == false)
            {
                //如(世界坐标转换的)索引与前一帧不同,则在当前索引位置刷新团块
                CellChunkManager.SpawnChunks(currentPos.x, currentPos.y, currentPos.z);
                //if (CPEngine.enableMultiplayer && CPEngine.multiplayerTrackPosition && CPEngine.network != null)
                //{
                //    //多人游戏模式下更新玩家位置
                //    Client.UpdatePlayerPosition(currentPos);
                //}
            }
            lastPos = currentPos;
        }

        #region Multiplayer

        /// <summary>
        /// 当连接到服务器时调用的函数.
        /// </summary>
        public void OnConnectedToServer()
        {
            if (CPEngine.enableMultiplayer && CPEngine.multiplayerTrackPosition)
            {
                StartCoroutine(InitialPositionAndRangeUpdate());
            }
        }
        /// <summary>
        /// 当连接到服务器时,更新玩家位置和范围.
        /// </summary>
        /// <returns></returns>
        public IEnumerator InitialPositionAndRangeUpdate()
        {
            while (CPEngine.network == null)
            {
                yield return new WaitForEndOfFrame();
            }
            Client.UpdatePlayerPosition(CPEngine.currentPos);
            Client.UpdatePlayerRange(CPEngine.chunkSpawnDistance);
        }

        #endregion

        // ==== 地图初始化时扩增纹理集的相关函数 ====

        /// <summary>
        /// 当没有预制体时,通过代码填充地块数组blocks,填充第0个元素为一个空的地形单元(空块),其他元素为null或自行设计.
        /// </summary>
        public static void BlocksPreFill(ushort num)
        {
            blocks = new GameObject[num]; //预计内置1675种地块（第4材质即材质索引3上的纹理是备用重复的,实际地块种类包括空块要减去152种=1523种）
            //填充第1个元素为一个空的地形单元(空块),只有0~10地块预制体是特别定制的,其余若只是换个uv则可以批量制作
            blocks[0] = new GameObject("cell_0");
            Cell cell = blocks[0].AddComponent<Cell>();
            cell.VName = "empty";
            cell.VTexture = new Vector2[6];
            cell.VTransparency = Transparency.semiTransparent;
            cell.VColliderType = ColliderType.none;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            //填充第2个元素为土块
            blocks[1] = new GameObject("cell_1");
            cell = blocks[1].AddComponent<Cell>();
            cell.VName = "dirt";
            cell.VTexture = new Vector2[6];
            cell.VTransparency = Transparency.solid;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            blocks[1].AddComponent<DefaultCellEvents>(); //不使用框架事件点击功能来更换方块的话可不添加该组件(余同)
            //填充第3个元素为草块
            blocks[2] = new GameObject("cell_2");
            cell = blocks[2].AddComponent<Cell>();
            cell.VName = "grass";
            cell.VCustomSides = true; //自定义6面纹理,否则所有纹理同Top面
            cell.VTexture = new Vector2[6]; //未填写的均为默认(0,0)
            cell.VTexture[0] = new Vector2(0, 2); //Top
            cell.VTexture[2] = new Vector2(0, 1); //Right
            cell.VTexture[3] = cell.VTexture[2]; //Left
            cell.VTexture[4] = cell.VTexture[2]; //Forward
            cell.VTexture[5] = cell.VTexture[2]; //Back
            cell.VTransparency = Transparency.solid;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            blocks[2].AddComponent<CellGrass>();
            //填充第4个元素为鹅卵石
            blocks[3] = new GameObject("cell_3");
            cell = blocks[3].AddComponent<Cell>();
            cell.VName = "cobblestone";
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = new Vector2(0, 3); //Top
            cell.VTransparency = Transparency.solid;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            blocks[3].AddComponent<DefaultCellEvents>();
            //填充第5个元素为长满青苔的鹅卵石
            blocks[4] = new GameObject("cell_4");
            cell = blocks[4].AddComponent<Cell>();
            cell.VName = "mossy cobblestone";
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = new Vector2(1, 3); //Top
            cell.VTransparency = Transparency.solid;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            blocks[4].AddComponent<DefaultCellEvents>();
            //填充第6个元素为石板瓷砖
            blocks[5] = new GameObject("cell_5");
            cell = blocks[5].AddComponent<Cell>();
            cell.VName = "stone tiles";
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = new Vector2(1, 4); //Top
            cell.VTransparency = Transparency.solid;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            blocks[5].AddComponent<DefaultCellEvents>();
            //填充第7个元素为木块
            blocks[6] = new GameObject("cell_6");
            cell = blocks[6].AddComponent<Cell>();
            cell.VName = "wood";
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = new Vector2(0, 4); //Top
            cell.VTransparency = Transparency.solid;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            blocks[6].AddComponent<DefaultCellEvents>();
            //填充第8个元素为叶子
            blocks[7] = new GameObject("cell_7");
            cell = blocks[7].AddComponent<Cell>();
            cell.VName = "leaves";
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = new Vector2(0, 5); //Top
            cell.VTransparency = Transparency.semiTransparent;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            blocks[7].AddComponent<DefaultCellEvents>();
            //填充第9个元素为球体
            blocks[8] = new GameObject("cell_8");
            cell = blocks[8].AddComponent<Cell>();
            cell.VName = "cell_8";
            cell.VTexture = new Vector2[6];
            cell.VTransparency = Transparency.semiTransparent;
            cell.VColliderType = ColliderType.mesh;
            cell.VCustomMesh = true; //使用自定义网格
            cell.VMesh = CellSpacePrefab.GetSphereMeshWithUV(false,10,32); //使用球体网格
            cell.VSubmeshIndex = 3;
            cell.VRotation = MeshRotation.none;
            //填充第10个元素为空块
            blocks[9] = new GameObject("cell_9");
            cell = blocks[9].AddComponent<Cell>();
            cell.VName = "cell_9";
            cell.VTexture = new Vector2[6];
            cell.VTransparency = Transparency.semiTransparent;
            cell.VColliderType = ColliderType.none;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            //填充第11个元素为空块
            blocks[10] = new GameObject("cell_10");
            cell = blocks[10].AddComponent<Cell>();
            cell.VName = "cell_10";
            cell.VTexture = new Vector2[6];
            cell.VTransparency = Transparency.semiTransparent;
            cell.VColliderType = ColliderType.none;
            cell.VSubmeshIndex = 0;
            cell.VRotation = MeshRotation.none;
            //后续让材质2的纹理从Cell_11开始,即从第12个元素开始填充,直到第1675个元素为止
        }
        /// <summary>
        /// 创建预制体实例.
        /// </summary>
        /// <param name="vName"></param>
        /// <param name="cellID"></param>
        /// <param name="subMeshIndex"></param>
        /// <param name="createGameObject"></param>
        /// <param name="gameObject"></param>
        /// <param name="vCustomMesh"></param>
        /// <param name="vMesh"></param>
        /// <param name="vRotation"></param>
        /// <param name="vTransparency"></param>
        /// <param name="vColliderType"></param>
        /// <param name="vCustomSides"></param>
        /// <param name="vector0"></param>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="vector3"></param>
        /// <param name="vector4"></param>
        /// <param name="vector5"></param>
        public static void CreatePrefab(string vName, ushort cellID, ushort subMeshIndex, bool createGameObject = true, GameObject gameObject = null, bool vCustomMesh = false, Mesh vMesh = null, MeshRotation vRotation = MeshRotation.none, Transparency vTransparency = Transparency.solid, ColliderType vColliderType = ColliderType.none, bool vCustomSides = false, Vector2 vector0 = default, Vector2 vector1 = default, Vector2 vector2 = default, Vector2 vector3 = default, Vector2 vector4 = default, Vector2 vector5 = default)
        {
            string name = "cell_" + cellID; //地块预制体名称
            if (createGameObject)
            {//如果强制创建游戏物体
                if (gameObject == null)
                {//参数未填写已有游戏物体
                    CPEngine.prefabOPs[cellID].gameObject = new GameObject(name);
                }
                else
                {
                    CPEngine.prefabOPs[cellID].gameObject = gameObject;
                }
                CPEngine.prefabOPs[cellID].transform = CPEngine.prefabOPs[cellID].gameObject.transform;
            }
            else { CPEngine.prefabOPs[cellID].gameObject.name = name; }
            Cell cell = CPEngine.prefabOPs[cellID].gameObject.AddComponent<Cell>();//在GameObject上添加单元组件对象
            cell.VName = vName;
            cell.VCustomSides = vCustomSides; //自定义6面纹理,否则所有纹理同Top面
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = vector0; //Top
            cell.VTexture[1] = vector1; //Bottom
            cell.VTexture[2] = vector2; //Right
            cell.VTexture[3] = vector3; //Left
            cell.VTexture[4] = vector4; //Forward
            cell.VTexture[5] = vector5; //Back
            cell.VTransparency = vTransparency; //透明度
            cell.VColliderType = vColliderType; //碰撞类型
            cell.VSubmeshIndex = subMeshIndex; //材质索引
            cell.VRotation = vRotation; //旋转
            cell.VCustomMesh = vCustomMesh; //是否使用自定义网格
            cell.VMesh = vMesh;
            CPEngine.blocks[cellID] = CPEngine.prefabOPs[cellID].gameObject;//Awake时已填充了IBlocks,第11个元素开始都是空的GameObject,要填充覆盖剩下的
            OP.pool.Push(prefabOPs[cellID]);//退回栈,待使用时取出
        }
        /// <summary>
        /// [内部地图专用]创建只替换uv的地块预制体实例(情况参数默认不透明、碰撞体为cube、无旋转等).因GUI手填地块预制体太慢,这里用脚本批处理创建预制体实例化后的GameObject.
        /// 虽然它们在创建瞬间会立即出现在场景,但设计退入对象池会失活隐藏(请在需要时取出并激活GameObject).
        /// 注意本函数创建的预制体实例会重新填充覆盖blocks数组.
        /// </summary>
        /// <param name="cellID"></param>
        /// <param name="subMeshIndex"></param>
        /// <param name="createGameObject"></param>
        /// <param name="gameObject"></param>
        /// <param name="vector0"></param>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="vector3"></param>
        /// <param name="vector4"></param>
        /// <param name="vector5"></param>
        public static void CreatePrefab(ushort cellID, ushort subMeshIndex, bool createGameObject = true, GameObject gameObject = null, Vector2 vector0 = default, Vector2 vector1 = default, Vector2 vector2 = default, Vector2 vector3 = default, Vector2 vector4 = default, Vector2 vector5 = default)
        {
            string name = "cell_" + cellID;
            if (createGameObject)
            {
                if (gameObject == null)
                {
                    CPEngine.prefabOPs[cellID].gameObject = new GameObject(name);
                }
                else
                {
                    CPEngine.prefabOPs[cellID].gameObject = gameObject;
                }
                CPEngine.prefabOPs[cellID].transform = CPEngine.prefabOPs[cellID].gameObject.transform;
            }
            else { CPEngine.prefabOPs[cellID].gameObject.name = name; }
            Cell cell = CPEngine.prefabOPs[cellID].gameObject.AddComponent<Cell>();//在GameObject上添加单元组件对象
            cell.VName = name;
            cell.VTexture = new Vector2[6];
            cell.VTexture[0] = vector0;
            cell.VTexture[1] = vector1;
            cell.VTexture[2] = vector2;
            cell.VTexture[3] = vector3;
            cell.VTexture[4] = vector4;
            cell.VTexture[5] = vector5;
            cell.VTransparency = Transparency.solid;
            cell.VColliderType = ColliderType.cube;
            cell.VSubmeshIndex = subMeshIndex;
            cell.VRotation = MeshRotation.none;
            CPEngine.blocks[cellID] = CPEngine.prefabOPs[cellID].gameObject;//Awake时已填充了IBlocks,第11个元素开始都是空的GameObject,要填充覆盖剩下的
            OP.pool.Push(prefabOPs[cellID]);//退回栈,待使用时取出
        }
        /// <summary>
        /// 批创建预制体实例.自动识别网格渲染器对应材质主纹理并按行列分割UV,批转化为预制体实例并存入CPEngine.PrefabOPs数组.
        /// </summary>
        /// <param name="cellID">特征图的第一个CellID</param>
        /// <param name="endID">特征图的最后一个CellID</param>
        /// <param name="subMeshIndex">网格渲染器的材质索引</param>
        /// <param name="textureRow">Y方向行数</param>
        /// <param name="textureCol">X方向列数</param>
        /// <param name="torf">默认true当lBlocks为null时才进行CreatePrefab(若lBlocks已有GUI填入的地块预制体则直接使用),否则总是CreatePrefab(不使用GUI填的地块预制体)</param>
        /// <param name="XIncrement">默认true则UV划区时(左下为原点)先以X方向自增,若为flase则先以Y方向自增</param>
        public static void CreatePrefabBatch(ushort cellID, ushort endID, ushort subMeshIndex, ushort textureCol, ushort textureRow, bool torf = true, bool XIncrement = true)
        {
            ushort index = cellID;
            ushort x = 0; //当前X坐标
            ushort y = 0; //当前Y坐标
            if (XIncrement)
            {
                //遍历所有切片,注意检查索引是否越界
                for (ushort row = 0; row < textureRow; row++)
                {//Y自增时,X重置为0
                    x = 0;
                    for (ushort col = 0; col < textureCol; col++)
                    {
                        //检查索引是否越界
                        if (index >= prefabOPs.Length)
                        {
                            Debug.LogError("Index out of range!");
                            return; //返回或处理越界情况
                        }
                        if (torf == false || (torf == true && blocks[index] == null))
                        {//torf为false时总是CreatePrefab,为true时仅当lBlocks无值才进行CreatePrefab
                            CreatePrefab(index, subMeshIndex, true, null, new Vector2(x, y));
                        }
                        else
                        {//若已有GUI填入的地块预制体则直接使用
                            //prefabOPs[index].gameObject = blocks[index]; //必须实例化这个预制体GameObject才会出现在场景,预制体占着Native内存
                            //prefabOPs[index].gameObject = Instantiate(GetCellGameObject(index)); //对GUI填入的Cell预制体块实例化
                            prefabOPs[index].gameObject = GetCellGameObject(index); //直接使用代码预创建的Cell预制体块
                            if (prefabOPs[index].gameObject != null)
                            {
                                prefabOPs[index].transform = prefabOPs[index].gameObject.transform;
                                OP.pool.Push(prefabOPs[cellID]);//退回栈,待使用时取出
                            }
                            else { Debug.LogError("未获得GUI填入的Cell预制体块"); }
                        }
                        index++;
                        //下个处理ID超过特征图最后CellID时直接跳出函数
                        if (index > endID) { return; }
                        x++;
                    }
                    y++;
                }
            }
            else
            {
                //遍历所有切片,注意检查索引是否越界
                for (ushort col = 0; col < textureCol; col++)
                {//X自增时,Y重置为0
                    y = 0;
                    for (ushort row = 0; row < textureRow; row++)
                    {
                        //检查索引是否越界
                        if (index >= prefabOPs.Length)
                        {
                            Debug.LogError("Index out of range!");
                            return; //返回或处理越界情况
                        }
                        if (torf == false || (torf == true && blocks[index] == null))
                        {//torf为false时总是CreatePrefab,为true时仅当lBlocks无值才进行CreatePrefab
                            CreatePrefab(index, subMeshIndex, true, null, new Vector2(x, y));
                        }
                        else
                        {//若已有GUI填入的地块预制体则直接使用
                            //prefabOPs[index].gameObject = blocks[index]; //必须实例化这个预制体GameObject才会出现在场景
                            //prefabOPs[index].gameObject = Instantiate(GetCellGameObject(index)); //对GUI填入的Cell预制体块实例化
                            prefabOPs[index].gameObject = GetCellGameObject(index); //直接使用代码预创建的Cell预制体块
                            if (prefabOPs[index].gameObject != null)
                            {
                                prefabOPs[index].transform = prefabOPs[index].gameObject.transform;
                                OP.pool.Push(prefabOPs[cellID]);//退回栈,待使用时取出
                            }
                            else { Debug.LogError("未获得GUI填入的Cell预制体块"); }
                        }
                        index++;
                        //下个处理ID超过特征图最后CellID时直接跳出函数
                        if (index > endID) { return; }
                        y++;
                    }
                    x++;
                }
            }
        }
        /// <summary>
        /// 读各预制场景纹理ID文本(用于后续自动绘制)
        /// </summary>
        public static void LoadPrefabSceneTextureIDs()
        {
            //读取ID文本前初始化数组中的每个List元素(用于存放场景纹理ID)
            for (int i = 0; i < mapContents.Length; i++)
            {
                mapContents[i] = new List<string>();
            }
            for (int i = 0; i < mapIDs.Length; i++)
            {
                mapIDs[i] = new List<ushort>();
            }

            //0,重装机兵大地图
            TextAsset textAsset = Resources.Load<TextAsset>("MapIndex/World");
            string tempContent = textAsset.text;
            string[] fields = tempContent.Split(',');
            mapContents[0].AddRange(fields); //分割好的世界纹理ID放到数组0
                                             //string combinedString = string.Join(",", mapContents[0]);
                                             //Debug.Log(combinedString);
                                             //将字符串转换为ushort并存储到mapIDs数组中
            for (int i = 0; i < fields.Length; i++)
            {
                mapIDs[0].Add(ushort.Parse(fields[i]));
            }
            //string joinedString = string.Join(",", mapIDs[0].Select(pixelX => pixelX.ToString()));
            //Debug.Log(joinedString); //Debug.Log(mapIDs[0].Count);

            //1~239(对应重装机兵小地图0~238.txt)
            string filePath;
            for (int i = 0; i <= 238; i++)
            {
                filePath = "MapIndex/" + i.ToString();//使用Resources方法的路径不需要文件后缀
                textAsset = Resources.Load<TextAsset>(filePath);
                tempContent = textAsset.text;
                fields = tempContent.Split(',');
                mapContents[i + 1].AddRange(fields); //239个小地图场景纹理ID存放在数组索引1~239
                                                     //将字符串转换为ushort并存储到mapIDs数组中
                for (int j = 0; j < fields.Length; j++)
                {
                    //使用ushort.Parse来转换字符串到ushort
                    mapIDs[i + 1].Add(ushort.Parse(fields[j]));
                }
            }
            //1~239小地图宽度信息文本
            textAsset = Resources.Load<TextAsset>("MapIndex/Width");
            tempContent = textAsset.text;
            fields = tempContent.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                mapWidths.Add(ushort.Parse(fields[i])); //首次添加用Add而不是赋值动作
            }

            //240,备用地图信息文本采用大地图
            textAsset = Resources.Load<TextAsset>("MapIndex/World");
            tempContent = textAsset.text;
            fields = tempContent.Split(',');
            mapContents[240].AddRange(fields); //分割好的世界纹理ID放到数组0
                                               //string combinedString = string.Join(",", mapContents[0]);
                                               //Debug.Log(combinedString);
                                               //将字符串转换为ushort并存储到mapIDs数组中
            for (int i = 0; i < fields.Length; i++)
            {
                mapIDs[240].Add(ushort.Parse(fields[i]));
            }
        }
        /// <summary>
        /// 更新预制体块(必须在使用前完整刷新地块数据)
        /// </summary>
        public static void BlocksRefresh()
        {
            //Unity不能直接判断一个未实例化的预制体上是否附加了特定的组件(这种情况是加载AB包中的预制体或从GUI拖到字段上的预制体)
            //预制体上的组件只有在实例化后才会真正存在并可被访问,此处我们将频繁使用的预制体都是实例化后存入对象池.
            //现在根据材质纹理数量进行规划
            ushort num0 = 11; //材质[0]主纹理默认有11个地块纹理,Unity编辑器开发环境下GUI界面手动拖入Cell预制体或从外部AB包加载的预制体都要实例化后存入对象池,其他情况是由BlocksPreFill创建
            //接下来是其他材质主纹理所划分的地块纹理数量(因数量巨多,不再手动从GUI拖入字段或从AB包加载)
            ushort num1 = (ushort)(num0 + 152);//163 
            ushort num2 = (ushort)(num1 + 1360);//1523
            ushort num3 = (ushort)(num2 + 152);//1675（内置地块ID为0~1674）

            int length = blocks.Length; //数组长度为1675
            if (length == 0)
            {
                //Unity开发时在GUI界面lBlocks预填数组容量后,传到这里blocks.Length是有值的,如果没有则进行Debug为内置数量
                length = num3;
                BlocksPreFill((ushort)length); //出错则重新填充
            }

            //此处自动创建同样个数的OP对象,所有OP对象共享对象池

            //对象池预填充容量为上述length(即便Push数量超过初始容量,栈也会自动扩容,只要不频繁扩容,游戏中途要增加地块种类是不会有问题的)
            CPEngine.prefabOPs = OP.Init(length, false); //OP对象池预填充但暂时不用创建游戏对象(由CreateTexPrefabBatch及CreatePrefab控制是否创建并绑定)

            //手动拖入的Cell预制体数量是num0:cell_0~10(其中0是空块,1~10的uv取自材质[0]主纹理)
            CreatePrefabBatch(0, (ushort)(num0 - 1), 0, (ushort)textureUnitX[0], (ushort)textureUnitY[0]); //如果已经有填充,该函数会直接使用代码创建的Cell预制体块并加入对象池

            //额外添加152个大地图纹理:cell_11~162(这些预制体块的uv均是材质[1]主纹理上的)
            //↓大地图19行8列会自动按起始和结尾参数转换出152个预制体实例,函数会绑定它们到OP对象gameObject字段
            CreatePrefabBatch(num0, (ushort)(num1 - 1), 1, (ushort)textureUnitX[1], (ushort)textureUnitY[1]);

            //额外添加1360个小地图纹理:cell_163~1522(这些预制体块的uv均是材质[2]主纹理上的)
            //↓小地图170行8列会自动转换出1360个预制体实例
            CreatePrefabBatch(num1, (ushort)(num2 - 1), 2, (ushort)textureUnitX[2], (ushort)textureUnitY[2]);

            //额外添加64个:cell_1523~1674(材质[3]主纹理上的),包括CellID_0共1675个Block数组元素
            //↓备用地图8行19列会自动转换出152个预制体实例
            CreatePrefabBatch(num2, (ushort)(num3 - 1), 3, (ushort)textureUnitX[3], (ushort)textureUnitY[3]);

            //此处可继续导入第5材质及纹理集...用上述相同方法划分uv,创建新编号地块预制体

            //读取重装机兵等预制场景纹理ID文本
            LoadPrefabSceneTextureIDs();
        }
        /// <summary>
        /// 获取地块(单元)预制体ID对应的团块材质渲染器组件上的材质索引.
        /// </summary>
        /// <param name="cellId">单元ID</param>
        /// <returns>地块纹理uv从哪个材质主纹理上划取,就返回哪个材质索引</returns>
        public static ushort GetSubMeshIndex(ushort cellId)
        {
            ushort torf = 0; //0~10为第1个材质
            if (cellId >= 11)
            {
                if (cellId < 163) { torf = 1; }//重装机兵大地图特征纹理
                else if (cellId < 1523) { torf = 2; }//重装机兵小地图特征纹理
                else if (cellId < 1675) { torf = 3; }//最后1个材质上的是备用纹理
                else
                {
                    //目前内置的地块ID最大是1674,想要超过这个值需自行修改.
                    Debug.Log("纹理ID超出内置材质子网格索引上限！");
                }
            }
            return torf;
        }

        // ==== world data ====

        /// <summary>
        /// 更新世界路径(定位到名为 “Worlds” 的存档目录,不同系统位置不一)
        /// </summary>
        public static void UpdateWorldPath()
        {
            //"../"是通用的文件系统路径表示法,用于表示上一级目录
            //下列动作意味着从 Application.dataPath 所指向的目录开始,导航到上一级目录(即 Application.dataPath 的父目录),再定位到名为 “Worlds” 的目录
            worldPath = Application.dataPath + "/../Worlds/" + worldName + "/"; // you can set World Path here
                                                                                //worldPath = "/mnt/sdcard/UniblocksWorlds/" + CPEngine.worldName + "/"; // example mobile path for Android
        }
        /// <summary>
        /// 设置活动世界名称(设置后世界种子将被重置为0,并刷新用于档案存储的世界路径).可用本函数在运行时更改世界名称.
        /// </summary>
        /// <param name="worldName"></param>
        public static void SetWorldName(string name)
        {
            worldName = name;
            worldSeed = 0;
            UpdateWorldPath();
        }
        /// <summary>
        /// 从文件中读取当前活动世界的种子,或者如果没有找到种子文件则随机生成一个新的种子,并将其存储在Engine.WorldSeed变量中.
        /// </summary>
        public static void GetSeed()
        { // reads the world seed from file if it exists, else creates a new seed and saves it to file

            //if (Application.isWebPlayer) { // don'transform save to file if webplayer		
            //	CPEngine.worldSeed = Random.CameraLookRange (ushort.MinValue, ushort.MaxValue);
            //	return;
            //}		

#if UNITY_WEBPLAYER
            //当前是WebPlayer
            Engine.WorldSeed = Random.Range (ushort.MinValue, ushort.MaxValue
            return;
#else
            //当前不是WebPlayer
#endif
            //存在种子文件则读取
            if (File.Exists(worldPath + "seed"))
            {
                //创建文件的读取流
                StreamReader reader = new StreamReader(worldPath + "seed");
                worldSeed = int.Parse(reader.ReadToEnd()); //读取全部字符串后转为数字,作为世界种子
                reader.Close();
            }
            else
            {
                //循环的目的是确保生成的 worldSeed 值不为 0
                while (worldSeed == 0)
                {
                    //创建一个新的种子
                    worldSeed = Random.Range(ushort.MinValue, ushort.MaxValue);
                }
                Directory.CreateDirectory(worldPath); //如文件夹存在则不会创建新的,该动作不会抛出异常无需用if (!Directory.Exists(worldPath))判断
                StreamWriter writer = new StreamWriter(worldPath + "seed"); //指定文件路径,创建一个写入流
                writer.Write(worldSeed.ToString()); //为文件写入内容字符串
                //writer.Flush();
                writer.Close();
            }
        }
        /// <summary>
        /// 将所有当前实例化团块的数据保存到磁盘,在Engine.MaxChunkSaves中可指定每帧保存团块的最大处理速率.
        /// </summary>
        public static void SaveWorld()
        { // saves the data over multiple frames

            //实例调用继承自父类的方法来异步处理存档(使用了Unity的协程)
            _instance.StartCoroutine(CellChunkDataFiles.SaveAllChunks());
        }
        /// <summary>
        /// 将所有当前实例化的团块数据保存到磁盘存档,单帧动作一次全执行,这很可能会使游戏冻结几秒钟,因此不建议在游戏过程中使用此功能.
        /// </summary>
        public static void SaveWorldInstant()
        { // writes data from TempChunkData into region files

            CellChunkDataFiles.SaveAllChunksInstant();
        }

        // ==== other ====	

        /// <summary>
        /// 获取体素ID对应的单元预制体.返回blocks[cellId].
        /// </summary>
        /// <param name="cellId">体素ID(单元预制体种类)</param>
        /// <returns>返回体素ID对应单元种类的游戏物体对象,体素ID=0或65535时返回空块</returns>
        public static GameObject GetCellGameObject(ushort cellId)
        {
            try
            {
                //如果体素ID达到ushort数据类型的最大值65535那么归零
                if (cellId == ushort.MaxValue) { cellId = 0; }
                GameObject cellObject = blocks[cellId];//获取体素ID对应单元种类的预制体
                //检查单元对象上的单元组件
                if (cellObject.GetComponent<Cell>() == null)
                {
                    Debug.LogError("CellSpace: Cell id " + cellId + " does not have the Cell component attached!" +
                        "游戏物体对象的单元组件不存在！返回空块！");
                    return blocks[0];
                }
                else
                {
                    return cellObject;
                }

            }
            catch (System.Exception)
            {
                //报错并指出无效体素ID
                Debug.LogError("CellSpace: Invalid cell id: " + cellId);
                return blocks[0];
            }
        }
        /// <summary>
        /// 获取体素ID对应的单元预制体的单元类型组件
        /// </summary>
        /// <param name="cellID">体素ID(单元预制体种类)</param>
        /// <returns>返回体素ID对应单元上的单元类型组件,体素ID=0或65535时返回空块上的单元类型组件</returns>
        public static Cell GetCellType(ushort cellID)
        {
            try
            {
                //如果体素ID达到ushort数据类型的最大值65535,那么归零(防止从负数开始)
                if (cellID == ushort.MaxValue) cellID = 0;
                Cell cell = blocks[cellID].GetComponent<Cell>();//获取体素ID对应单元上的单元类型组件
                if (cell == null)
                {
                    //单元组件不存在
                    Debug.LogError("CellSpace: Cell ID " + cellID + " does not have the Cell component attached!");
                    return null;
                }
                else
                {
                    //返回体素ID对应单元上的单元类型组件
                    return cell;
                }

            }
            catch (System.Exception)
            {
                //报错并指出无效体素ID
                Debug.LogError("CellSpace: Invalid Cell ID: " + cellID);
                return null;
            }
        }
        /// <summary>
        /// 使用指定原点、方向和范围执行光线投射,并返回单元索引,其中包含命中团块的游戏物体(CellInfo.chunk)、命中单元的索引(CellInfo.index)及与命中面相邻单元索引(CellInfo.adjacentindex).
        /// “ignoreTransparent”为true时光线投射将穿透透明或半透明的单元,若没有击中则返回null.注意:如果碰撞体生成被禁用,此函数将不起作用,另在2D模式下Z=0但默认插入一个立方体,只是纹理在Up面改为Forward
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static CellInfo CellRaycast(Vector3 origin, Vector3 direction, float range, bool ignoreTransparent)
        { // a raycast which returns the index of the hit cell and the gameobject of the hit chunk

            RaycastHit hit = new RaycastHit(); //创建射线投射器hit

            //利用物理引擎投射光线,hit的绘制从origin(摄像机位置)出发沿direction(摄像机前方)方向,最大距离range
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                //如果从hit碰撞体组件对象里能获取到团块或团块扩展组件(这里因为Collider是继承Component的,所以可直接使用父类的GetComponent方法获取当前游戏物体对象身上的其他兄弟组件)
                if (hit.collider.GetComponent<CellChunk>() != null || hit.collider.GetComponent<CellChunkExtension>() != null)
                { // check if we're actually hitting a chunk.检查我们是否真的击中了团块

                    GameObject hitObject = hit.collider.gameObject; //从碰撞体组件中获得游戏物体对象并赋值给hitObject

                    if (hitObject.GetComponent<CellChunkExtension>() != null)
                    { // if we hit a mesh container instead of a chunk.如果我们击中的是网状容器而不是团块(判断依据是网状容器拥有大块扩展组件),注意网格容器是团块大小的,虽是团块的子对象但它不是单元)
                        hitObject = hitObject.transform.parent.gameObject; // swap the mesh container for the actual chunk object.将网格容器替换为实际的团块对象(它是网格容器对象的父级对象)
                    }

                    //根据hit碰撞面法线方向来推离或推进hit位置,后将新位置转为在团块的本地局部坐标(相对位置)来获取单元索引(false指不获取相邻单元,则推进hit到所碰单元内部),最终将hit新位置进行四舍五入修正以靠近最近顶点作为单元索引返回
                    CPIndex hitIndex = hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, false);

                    //忽略透明(功能尚未完善)
                    if (ignoreTransparent)
                    { // punch through transparent voxels by raycasting again when a transparent cell is hit.当一个透明单元被击中时,再次通过光线投射穿透透明单元
                        ushort hitCell = hitObject.GetComponent<CellChunk>().GetCellID(hitIndex.x, hitIndex.y, hitIndex.z); //通过单元索引从团块里获得体素ID(单元预制体种类)
                        //如果命中的单元类型的VTransparency属性!=实心,说明是透明或半透明
                        if (GetCellType(hitCell).VTransparency != Transparency.solid)
                        {
                            Vector3 newOrigin = hit.point; //存储hit坐标
                            newOrigin.y -= 0.5f; // push the new raycast down a bit.将hit向下高度移动0.5(基本上hit跑到所选单元内部)
                            return CellRaycast(newOrigin, Vector3.down, range - hit.distance, true); //递归调用函数自身,以新点开始重新向下射出射线,来完成剩余距离碰撞检测(true指获取相邻单元)
                                                                                                     //这段代码只能处理向下的透明单元,其他方向(如向上、向左、向右等)也透明那么无法正确地“穿透”


                        }
                    }

                    return new CellInfo(
                                         hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, false), // get hit cell index.获取击中单元的索引
                                         hitObject.GetComponent<CellChunk>().PositionToCellIndex(hit.point, hit.normal, true), // get adjacent cell index.获取相邻单元的索引
                                         hitObject.GetComponent<CellChunk>()); // get chunk.获取团块
                }
            }

            //其他情况
            return null;
        }
        /// <summary>
        /// 使用指定射线和范围执行光线投射,并返回VoxelInfo,其中包含命中团块GameObject(CellInfo.chunk)、命中单元的索引(CellInfo.index)及与命中面相邻单元的索引(CellInfo. adjacentindex).
        /// “ignoreTransparent”为true时光线投射将穿透透明或半透明的单元.若没有击中任何块,则返回null.注意:如果碰撞体生成被禁用,此函数将不起作用.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="range"></param>
        /// <param name="ignoreTransparent"></param>
        /// <returns></returns>
        public static CellInfo CellRaycast(Ray ray, float range, bool ignoreTransparent)
        {
            return CellRaycast(ray.origin, ray.direction, range, ignoreTransparent);
        }
        /// <summary>
        /// 返回与给定世界位置相对应的团块索引
        /// </summary>
        /// <param name="position">团块位置</param>
        /// <returns></returns>
        public static CPIndex PositionToChunkIndex(Vector3 position)
        {
            CPIndex chunkIndex;
            if (CPEngine.horizontalMode)
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / chunkScale.x) / chunkSideLength,
                                          Mathf.RoundToInt(position.y / chunkScale.y) / chunkSideLength);
            }
            else
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / chunkScale.x) / chunkSideLength,
                                          Mathf.RoundToInt(position.y / chunkScale.y) / chunkSideLength,
                                          Mathf.RoundToInt(position.z / chunkScale.z) / chunkSideLength);
            }
            return chunkIndex;
        }
        /// <summary>
        /// 返回与给定世界位置相对应的团块游戏对象,若团块没有实例化则返回null.
        /// </summary>
        /// <param name="position">团块位置</param>
        /// <returns></returns>
        public static GameObject PositionToChunk(Vector3 position)
        {
            CPIndex chunkIndex;
            if (CPEngine.horizontalMode)
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / chunkScale.x) / chunkSideLength,
                                          Mathf.RoundToInt(position.y / chunkScale.y) / chunkSideLength);
            }
            else
            {
                chunkIndex = new CPIndex(Mathf.RoundToInt(position.x / chunkScale.x) / chunkSideLength,
                                          Mathf.RoundToInt(position.y / chunkScale.y) / chunkSideLength,
                                          Mathf.RoundToInt(position.z / chunkScale.z) / chunkSideLength);
            }

            return CellChunkManager.GetChunk(chunkIndex);

        }
        /// <summary>
        /// 将位置转换成单元信息(其中包含与给定世界位置对应的单元,如单元的团块没被实例化则返回null)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static CellInfo PositionToCellInfo(Vector3 position)
        {
            GameObject chunkObject = PositionToChunk(position);
            if (chunkObject != null)
            {
                CellChunk chunk = chunkObject.GetComponent<CellChunk>();
                CPIndex cellIndex = chunk.PositionToCellIndex(position);
                return new CellInfo(cellIndex, chunk);
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// 返回指定单元中心点的世界位置.
        /// </summary>
        /// <param name="cellInfo"></param>
        /// <returns></returns>
        public static Vector3 CellInfoToPosition(CellInfo cellInfo)
        {
            return cellInfo.chunk.GetComponent<CellChunk>().CellIndexToPosition(cellInfo.index);
        }

        // ==== mesh creator ====

        //块编辑器中如果勾选Custom Mesh则启用默认材质,只有不勾选Custom Mesh时可以自定义材质,也就是可以选择图片纹理偏移
        //图片纹理偏移有两种:第一种是勾选定义单面纹理,会出现6个面让你逐一填写纹理偏移点；第二种是不勾选,那么6个面采用相同纹理偏移点
        //使用纹理偏移点获取预制图片上的纹理:左下角精灵是(0,0),而(1,0)表示右边精灵,每个精灵的大小由引擎设置中Texture unit决定
        //Texture unit数值填8(内部UV划区时是倒数比例0.125)表示对整张图片切割为8*8=64个均分的精灵,MC插件魔改后成为CellSpace库,支持行列不同划区数量及识别多个材质主纹理

        /// <summary>
        /// 获取纹理偏移点
        /// </summary>
        /// <param name="cellID">体素ID(单元预制体种类)</param>
        /// <param name="facing">面向</param>
        /// <returns>如没定义纹理则返回Vector2(0, 0),如单元没用自定义单面纹理则返回顶部纹理点,如请求一个没定义的纹理则抓取最后定义纹理点来返回</returns>
        public static Vector2 GetTextureOffset(ushort cellID, Facing facing)
        {
            //获取单元的类型
            Cell cellType = GetCellType(cellID);
            //获取纹理数组(二维向量点阵小组)
            Vector2[] textureArray = cellType.VTexture;
            if (textureArray.Length == 0)
            { // in case there are no textures defined, return a default texture.以防万一如果没有定义纹理,则返回默认的(0, 0)
                Debug.LogWarning("CellSpace: Block " + cellID.ToString() + " has no defined textures! Using default texture.");
                return new Vector2(0, 0);
            }
            else if (cellType.VCustomSides == false)
            { // if this cell isn'transform using custom side textures, return the Up texture.如果这个单元没有使用自定义单面纹理,直接返回单元朝上的那面纹理点(6个面共用)
                return textureArray[0];
            }
            //将面向这个枚举类型转整型(上下左右前后默认对应0~5),如面向所代表的数值超过纹理点阵长度-1,将被判定为没有定义纹理(自定义6面却忘了填完它们)
            else if ((int)facing > textureArray.Length - 1)
            { // if we're asking for a texture that's not defined, grab the last defined texture instead.如果我们请求了一个没有定义的纹理,则抓取最后定义的纹理点(剩下的面会采用它代表的精灵的纹理)
                return textureArray[textureArray.Length - 1];
            }
            else
            {
                //正常返回一个对应面向索引的纹理点
                return textureArray[(int)facing];
            }
        }

        #endregion
    }
}

// VS2022编写本脚本,在右侧解决方案窗口,引用中添加dll等方法库文件,之后可用其内指定命名空间的具体方法进行编程
// 在C#编程中,命名空间(Namespace)是一种组织代码的方式,它可以帮助我们避免类名、方法名等之间的冲突,并提供一种逻辑上的分组机制
// UnityEngine 命名空间
// 主要作用:提供Unity游戏引擎的核心功能.UnityEngine命名空间包含了创建和管理Unity游戏所需的所有基础类和接口.
// 包含内容:这个命名空间包含用于场景管理、对象操作、渲染、物理、输入处理、网络、音频、用户界面、动画、脚本生命周期管理等功能的类.例如,Transform 类用于表示和操作游戏对象的位置、旋转和缩放；GameObject 类是Unity场景中的基本构建块；MonoBehaviour 类是所有脚本组件的基类,它提供了如Start和Update等生命周期方法.
// System.Collections.Generic 命名空间
// 主要作用:提供了一系列泛型集合类,这些类用于存储和管理数据集合,如列表、字典、集合、队列等.
// 包含内容:这个命名空间包含了如List<T>(泛型列表)、Dictionary<TKey, TValue>(键值对集合)、HashSet<T>(集合,不包含重复元素)、Queue<T>(队列)等类.这些类为数据存储和操作提供了高效和灵活的方式.
// System.IO 命名空间
// 主要作用:提供文件和数据流的基本输入/输出功能.System.IO命名空间包含用于文件和数据流操作的类,如文件读写、目录管理、数据流处理等.
// 包含内容:这个命名空间中的类允许你创建文件、读取文件内容、写入文件、删除文件、管理目录结构、处理数据流等.例如,File 类提供了静态方法用于文件的创建、复制、删除、移动和打开；Directory 类用于创建、删除和移动目录；StreamReader 和 StreamWriter 类用于从文件中读取文本和向文件中写入文本.
// 在Unity项目中,通常会通过引用这些命名空间来使用它们提供的类和功能.例如,在脚本文件的开头使用using UnityEngine;语句,就可以让你在脚本中直接使用Unity引擎提供的所有类和功能,而无需每次都写出完整的命名空间路径.

//一些提示:
//框架启用后,在创建空间团的瞬间,整个空间都是空块(id=0),所有的场景地形图片全凭想象,目前虽然支持6面不同,但需要通过块编辑器进行一个个定制.
//重装机兵大地图规定只刷侧面是我批量用代码定制的(比手动菜单要快).从横版切换为原版3D框架设计游戏地形后,默认会恢复显示顶部纹理.
//在关闭单一空间重复利用模式下,只要移动团块位置指示器,自动计算应该刷什么场景图.
//在开启单一空间重复利用模式时,地形生成器停止刷新工作,此时当前空间充满透明空块按自己想象随便刷指定纹理块即可,直接使用chunk.SetID(位置索引,id).

//单一空间重复利用模式下,其实也能模拟多场景NPC活动,需要做一套虚拟场景坐标.
//目前横版幸存者框架里写了一套虚拟坐标,只是与真实世界坐标比例100:1而已.新的虚拟坐标模拟邻居城镇场景各不相同时需要一个转换函数根据虚拟坐标和团块索引得到真实世界坐标(使各场景都重复刷在单个绝对世界空间).
//重复利用空间的性能高一些,而分开创建摧毁大空间团时会明显掉帧.
//单一空间重复利用模式下应关闭框架自动存档功能,改为手动刷需要的场景并按需存档信息。