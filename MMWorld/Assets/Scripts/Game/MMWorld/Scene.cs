using UnityEngine;
using MMWorld.Test;

namespace MMWorld
{
    /// <summary>
    /// 场景
    /// </summary>
    public class Scene : MonoBehaviour
    {
        // 编辑器中拖拽 带法线的材质球到此 ( texture packer 插件 生成的那个, 需要核查法线贴图是否正确 )
        public Material material;

        // 这个拖 Packages\Universal\Materials\Sprite-Unlit-Default
        public Material minimapMaterial;

        // 这个拖 Mini Map Canvas 节点
        public Canvas minimapCanvas;

        // 这个拖 Mini Map Camera 节点
        public Camera minimapCamera;

        // 编辑器中 分组, 多选 拖拽 精灵图集到此 ( texture packer 插件 生成的那个, 展开再 shift 多选 )
        public Sprite[] sprites_font_outline;
        public Sprite[] sprites_explosions;
        public Sprite[] sprites_player;
        public Sprite[] sprites_bullets;
        public Sprite[] sprites_monster01;
        public Sprite[] sprites_monster02;
        public Sprite[] sprites_monster03;
        public Sprite[] sprites_monster04;
        public Sprite[] sprites_monster05;
        public Sprite[] sprites_monster06;
        public Sprite[] sprites_monster07;
        public Sprite[] sprites_monster08;
        // ...

        //注：下述grid（网格）与CellSpace中的怪物容器不混用，可继续将一个Cell划分更细小来检索怪物在grid上的位置，同时用来设计屏幕范围（逻辑像素）在grid空间总的逻辑像素尺寸下的比例
        //坐标分世界（绝对）坐标、本地（local/相对父级物体/局部）坐标、逻辑（像素）坐标三种，前2种直接关联渲染位置，逻辑坐标则方便参与游戏逻辑的尺寸距离直接判断
        //CellSpace里还有个怪物容器是本地坐标的（不冲突），注意区别即可

        /// <summary>
        /// 网格团块内的网格行、列数量（如不再逻辑划分怪物区域，默认同单元空间团块的边长）
        /// </summary>
        internal const int gridChunkNumRows = 256, gridChunkNumCols = 256;

        /// <summary>
        /// 网格团块内的网格最大宽度尺寸（单位是逻辑像素非真实像素）
        /// </summary>
        internal const float gridChunkWidth = gridChunkNumCols * gridSize;

        /// <summary>
        /// 网格团块内的网格最大高度尺寸（单位是逻辑像素非真实像素）
        /// </summary>
        internal const float gridChunkHeight = gridChunkNumRows * gridSize;

        /// <summary>
        /// 网格团块的宽度尺寸中心（单位是逻辑像素非真实像素）
        /// </summary>
        internal const float gridChunkWidth_2 = gridChunkWidth / 2, gridHeight_2 = gridChunkHeight / 2;
        /// <summary>
        /// 网格团块的高度尺寸中心（单位是逻辑像素非真实像素）
        /// </summary>
        internal const float gridChunkCenterX = gridChunkWidth_2, gridChunkCenterY = gridHeight_2;

        /// <summary>
        /// 设计帧率（逻辑值）
        /// </summary>
        internal const int fps = 60;

        /// <summary>
        /// 帧率间隔（逻辑帧率的倒数）
        /// </summary>
        internal const float frameDelay = 1.0f / fps;

        /// <summary>
        /// 设计分辨率（单位是逻辑像素非真实像素），决定了屏幕（摄像机镜头）范围占空间总大小多少。
        /// 为了达到现实中屏幕分辨率，逻辑像素范围可取1920*1080这样的默认值。修改gridSize和grid行列数可调整网格团块一角或全部被屏幕看到的比例。
        /// </summary>
        internal const float designWidth = 1920, designHeight = 1080;

        /// <summary>
        /// 设计分辨率的一半（方便计算和使用）
        /// </summary>
        internal const float designWidth_2 = designWidth / 2, designHeight_2 = designHeight / 2;

        /// <summary>
        /// 设计分辨率到摄像头坐标的转换系数（允许渲染位置与逻辑位置不同，如戴森球那游戏的庞大星系不按实际距离尺寸制作时） 
        /// </summary>
        internal const float designWidthToCameraRatio = 1f;//无转换（渲染位置=逻辑位置）
                                                           //internal const float designWidthToCameraRatio = (1920f*100f) / designWidth;    //todo：需要进一步找准这个数据

        /// <summary>
        /// 网格尺寸（单位是逻辑像素非真实像素），决定空间或地图总的逻辑像素大小及屏幕（摄像头）占据的大小。
        /// </summary>
        internal const float gridSize = 100f;

        /// <summary>
        /// [常数]根号二大小
        /// </summary>
        internal const float sqrt2 = 1.414213562373095f;
        /// <summary>
        /// [常数]根号二大小的一半
        /// </summary>
        internal const float sqrt2_1 = 0.7071067811865475f;

        /// <summary>
        /// 当前是否已开启小地图
        /// </summary>
        internal bool minimapEnabled = true;

        /// <summary>
        /// 当前帧（总的运行帧编号）
        /// </summary>
        internal int time = 0;

        /// <summary>
        /// 用于稳定调用逻辑Update的时间累计变量
        /// </summary>
        internal float timePool = 0;

        /// <summary>
        /// 当前玩家（可跨关卡存在故放置在此类）
        /// </summary>
        internal Player player;

        /// <summary>
        /// 当前关卡
        /// </summary>
        internal Stage stage;

        /// <summary>
        /// 空间容器索引时要用到的找最近所需格子的偏移数组（所有关卡公用）
        /// </summary>
        internal static SpaceRingDiffuseData spaceRDD = new(100, (int)gridSize);

        void Start()
        {
            //初始化玩家输入系统
            InitInputAction();

            //初始化 HDR 显示模式
            //try
            //{
            //    //启用引擎高动态范围成像提供更广泛亮度范围和更高对比度，使图像亮暗部细节更丰富从而呈现出更加真实和生动的画面效果
            //    HDROutputSettings.main.RequestHDRModeChange(true);
            //}
            //catch (Exception e)
            //{
            //    Debug.Log(e); //Unity6.0不会报这个错
            //}

            //初始化底层绘制对象池
            GO.Init(material, 20000);

            //初始化玩家
            player = new Player(this);
            TextureColliderSet.player = player;//碰撞录制器切图时角色位置重制用

            //初始化起始关卡
            stage = new MMTestStage(this);

            //stage = new TestStage1(this); //测试大量数字特效
            //stage = new TestStage2(this); //正常测试
        }

        void Update()
        {
            //处理玩家输入
            HandlePlayerInput();

            var timeBak = time;
            //按设计帧率驱动游戏逻辑
            timePool += Time.deltaTime;
            if (timePool > frameDelay)
            {
                timePool -= frameDelay;
                ++time;
                stage.Update();
            }
            //若启用了小地图那么只在update执行过的帧才启用minimap的camera（提升点性能）
            if (minimapEnabled)
            {
                //若游戏逻辑被运行且次数是偶数那么启用小地图镜头
                minimapCamera.enabled = timeBak != time && (time & 1) == 0;
            }

            //同步显示
            stage.Draw();
        }

        /// <summary>
        /// 绘制编辑器场景对象
        /// </summary>
        void OnDrawGizmos()
        {
            //验证是否有关卡，有则绘制无则返回
            if (stage == null) return;
            stage.DrawGizmos();
        }
        /// <summary>
        /// 摧毁关卡对象和对象池资源
        /// </summary>
        void OnDestroy()
        {
            stage.Destroy();
            GO.Destroy();
        }
        /// <summary>
        /// 设置关卡
        /// </summary>
        /// <param name="newStage">新的关卡</param>
        internal void SetStage(Stage newStage)
        {
            stage.Destroy();//清理旧关卡
            stage = newStage;//设置新的关卡
        }

        /// <summary>
        /// 启禁用小地图(可间歇开关minimapCamera来提升性能 )
        /// </summary>
        /// <param name="b">启动小地图（不填则默认为真）</param>
        internal void EnableMinimap(bool b = true)
        {
            if (minimapEnabled == b) return;//没变化时直接返回
            minimapEnabled = b;//刷新场景minimapEnabled字段
            minimapCanvas.enabled = b;//决定画布是否启用
            minimapCamera.enabled = b;//决定小地图摄像机是否启用
        }

        //处理玩家输入

        /// <summary>
        /// InputActions.PlayerActions（玩家动作输入）
        /// </summary>
        internal InputActions.PlayerActions iapa;           //不能在这直接new（会为空）
        /// <summary>
        /// 键盘 W/UP
        /// </summary>
        internal bool playerKBMovingUp;
        /// <summary>
        /// 键盘 S/Down
        /// </summary>
        internal bool playerKBMovingDown;
        /// <summary>
        /// 键盘 A/Left
        /// </summary>
        internal bool playerKBMovingLeft;
        /// <summary>
        /// 键盘 D/Right
        /// </summary>
        internal bool playerKBMovingRight;

        //主要用下面这几个

        /// <summary>
        /// 键盘=true，手柄=false
        /// </summary>
        internal bool playerUsingKeyboard;
        /// <summary>
        /// 键盘Space或手柄按钮A/X
        /// </summary>
        internal bool playerJumping;
        /// <summary>
        /// 是否正在移动(键盘ASDW或手柄左joy均能触发)
        /// </summary>
        internal bool playerMoving;
        /// <summary>
        /// 归一化之后的移动方向(读前先判断playerMoving)
        /// </summary>
        internal Vector2 playerMoveValue;
        /// <summary>
        /// 上一个非零移动值的备份(比如当前值为0而上次备份值为1时可供变化参考)，初值=new(1, 0)
        /// </summary>
        internal Vector2 playerLastMoveValue = new(1, 0);
        /// <summary>
        /// 获取玩家朝向。如果移动中，获取归一化后的移动方向；不在移动，则返回上一个非零移动值的备份
        /// </summary>
        internal Vector2 playerDirection
        {
            get
            {
                if (playerMoving)
                {//如果移动中，获取归一化后的移动方向
                    return playerMoveValue;
                }
                else
                {//不在移动，则返回上一个非零移动值的备份
                    return playerLastMoveValue;
                }
            }
        }

        /// <summary>
        /// 初始化输入动作
        /// </summary>
        internal void InitInputAction()
        {
            var ia = new InputActions();//新的输入动作
            iapa = ia.Player;//绑定输入动作玩家
            iapa.Enable();//启用

            //↓记录各种玩家操作事件下的状态值

            // keyboard键盘
            iapa.KBJump.started += c =>
            {
                playerUsingKeyboard = true;
                playerJumping = true;
            };
            iapa.KBJump.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerJumping = false;
            };

            iapa.KBMoveUp.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingUp = true;
            };
            iapa.KBMoveUp.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingUp = false;
            };

            iapa.KBMoveDown.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingDown = true;
            };
            iapa.KBMoveDown.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingDown = false;
            };

            iapa.KBMoveLeft.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingLeft = true;
            };
            iapa.KBMoveLeft.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingLeft = false;
            };

            iapa.KBMoveRight.started += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingRight = true;
            };
            iapa.KBMoveRight.canceled += c =>
            {
                playerUsingKeyboard = true;
                playerKBMovingRight = false;
            };

            // gamepad游戏手柄
            iapa.GPJump.started += c =>
            {
                playerUsingKeyboard = false;
                playerJumping = true;
            };
            iapa.GPJump.canceled += c =>
            {
                playerUsingKeyboard = false;
                playerJumping = false;
            };

            iapa.GPMove.started += c =>
            {
                playerUsingKeyboard = false;
                playerMoving = true;
            };
            iapa.GPMove.performed += c =>
            {
                playerUsingKeyboard = false;
                playerMoving = true;
            };
            iapa.GPMove.canceled += c =>
            {
                playerUsingKeyboard = false;
                playerMoving = false;
            };
        }

        /// <summary>
        /// 处理玩家输入(只是填充playerMoving等状态值)
        /// </summary>
        internal void HandlePlayerInput()
        {
            if (playerUsingKeyboard)
            {//使用键盘情况：需每帧判断、合并方向，计算最终矢量
                if (!playerKBMovingUp && !playerKBMovingDown && !playerKBMovingLeft && !playerKBMovingRight
                    || playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 0f;
                    playerMoving = false;
                }
                else if (!playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = -1f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && !playerKBMovingDown && playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 1f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingDown && !playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 1f;
                    playerMoveValue.y = 0f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingDown && playerKBMovingLeft && !playerKBMovingRight)
                {
                    playerMoveValue.x = -1f;
                    playerMoveValue.y = 0f;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingDown
                      || playerKBMovingLeft && playerKBMovingRight)
                {
                    playerMoveValue.x = 0f;
                    playerMoveValue.y = 0f;
                    playerMoving = false;
                }
                else if (playerKBMovingUp && playerKBMovingLeft)
                {
                    playerMoveValue.x = -sqrt2_1;
                    playerMoveValue.y = sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingUp && playerKBMovingRight)
                {
                    playerMoveValue.x = sqrt2_1;
                    playerMoveValue.y = sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown && playerKBMovingLeft)
                {
                    playerMoveValue.x = -sqrt2_1;
                    playerMoveValue.y = -sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown && playerKBMovingRight)
                {
                    playerMoveValue.x = sqrt2_1;
                    playerMoveValue.y = -sqrt2_1;
                    playerMoving = true;
                }
                else if (playerKBMovingUp)
                {
                    playerMoveValue.x = 0;
                    playerMoveValue.y = 1;
                    playerMoving = true;
                }
                else if (playerKBMovingDown)
                {
                    playerMoveValue.x = 0;
                    playerMoveValue.y = -1;
                    playerMoving = true;
                }
                else if (playerKBMovingLeft)
                {
                    playerMoveValue.x = -1;
                    playerMoveValue.y = 0;
                    playerMoving = true;
                }
                else if (playerKBMovingRight)
                {
                    playerMoveValue.x = 1;
                    playerMoveValue.y = 0;
                    playerMoving = true;
                }
                //if (playerMoving)
                //{
                //    Debug.Log(playerKBMovingUp + " " + playerKBMovingDown + " " + playerKBMovingLeft + " " + playerKBMovingRight + " " + playerMoveValue);
                //}
            }
            else
            {//手柄不需要判断
                var v = iapa.GPMove.ReadValue<Vector2>();
                //v.Normalize();//归一化
                playerMoveValue.x = v.x;
                playerMoveValue.y = v.y;
                //todo：playerMoving = 距离 > 死区长度 ？
            }
            if (playerMoving)
            {//如果移动成功
             //记录最后一次移动方向
                playerLastMoveValue = playerMoveValue;
            }
        }
    }
}

////利用反射来读取怪物配置
//var st = typeof(Scene);
//var fs = st.GetFields(BindingFlags.Public | BindingFlags.Instance);
//foreach (var f in fs) {
//    if (f.FieldType.Name == "Sprite[]") {
//        if (f.Name.StartsWith("sprites_monster")) {
//            var ss = f.GetValue(scene) as Sprite[];
//            if (ss.Length > 0) {
//                spritess.Add(ss);
//            }
//        }
//    }
//}
