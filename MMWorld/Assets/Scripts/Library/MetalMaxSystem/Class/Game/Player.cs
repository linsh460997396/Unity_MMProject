#if UNITY_EDITOR || UNITY_STANDALONE
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
    /// 玩家类，为每个玩家创建它并初始化所需信息，内置字段以外的临时属性可用数据表添加修改
    /// </summary>
    public static class Player
    {
        #region 字段

        private static Unit[] _hero = new Unit[Game.c_maxPlayers + 1];
        private static Unit[,] vehicle = new Unit[Game.c_maxPlayers + 1, Game.c_vehicleTypeMax];
        private static Unit[] currentVehicle = new Unit[Game.c_maxPlayers + 1];
        private static Unit[] unitMain = new Unit[Game.c_maxPlayers + 1];
        private static Unit[] unitControl = new Unit[Game.c_maxPlayers + 1];
        private static bool[] _canNotOperation = new bool[Game.c_maxPlayers + 1];

        private static bool[,] _keyDown = new bool[Game.c_maxPlayers + 1, MMCore.c_keyMax + 1];
        private static bool[,] _keyDownState = new bool[Game.c_maxPlayers + 1, MMCore.c_keyMax + 1];
        private static bool[] _keyDownLoop = new bool[Game.c_maxPlayers + 1];
        private static int[] _keyDownLoopOneBitNum = new int[Game.c_maxPlayers + 1];

        private static bool[] _mouseDownLeft = new bool[Game.c_maxPlayers + 1];
        private static bool[] _mouseDownMiddle = new bool[Game.c_maxPlayers + 1];
        private static bool[] _mouseDownRight = new bool[Game.c_maxPlayers + 1];
        private static bool[,] _mouseDown = new bool[Game.c_maxPlayers + 1, MMCore.c_mouseMax + 1];
        private static bool[,] _mouseDownState = new bool[Game.c_maxPlayers + 1, MMCore.c_mouseMax + 1];
        private static bool[] _mouseDownLoop = new bool[Game.c_maxPlayers + 1];
        private static int[] _mouseDownLoopOneBitNum = new int[Game.c_maxPlayers + 1];

        private static bool[,] _keyDoubleClick = new bool[Game.c_maxPlayers + 1, MMCore.c_mouseMax + 1];
        private static bool[,] _mouseDoubleClick = new bool[Game.c_maxPlayers + 1, MMCore.c_mouseMax + 1];

        private static int[] _mouseUIX = new int[Game.c_maxPlayers + 1];
        private static int[] _mouseUIY = new int[Game.c_maxPlayers + 1];

        private static float[] _mouseVectorX = new float[Game.c_maxPlayers + 1];
        private static float[] _mouseVectorY = new float[Game.c_maxPlayers + 1];
        private static float[] _mouseVectorZ = new float[Game.c_maxPlayers + 1];
        private static float[] _mouseVectorZFixed = new float[Game.c_maxPlayers + 1];
        private static float[] _mouseToUnitControlAngle = new float[Game.c_maxPlayers + 1];
        private static float[] _mouseToUnitControlRange = new float[Game.c_maxPlayers + 1];
        private static float[] _mouseToUnitControlRange3F = new float[Game.c_maxPlayers + 1];
        private static Vector3F[] _cameraVector3F = new Vector3F[Game.c_maxPlayers + 1];
        private static Vector3F[] _mouseVector3FFixed = new Vector3F[Game.c_maxPlayers + 1];
        private static Vector3F[] _mouseVector3F = new Vector3F[Game.c_maxPlayers + 1];
        private static Vector3F[] _mouseVector3FUnitTerrain = new Vector3F[Game.c_maxPlayers + 1];
        private static Vector3F[] _mouseVector3FTerrain = new Vector3F[Game.c_maxPlayers + 1];
        private static Vector2F[] _mouseVector2F = new Vector2F[Game.c_maxPlayers + 1];

        private static string[] _type = new string[Game.c_maxPlayers + 1];
        private static string[] _handle = new string[Game.c_maxPlayers + 1];//句柄格式："A1-A1-A1-A0000001"

        private static bool[] _localUser = new bool[Game.c_maxPlayers + 1];
        private static string[] _status = new string[Game.c_maxPlayers + 1];
        private static int[] _teamID = new int[Game.c_maxPlayers + 1];

        #endregion

        #region 属性方法

        /// <summary>
        /// 英雄单位
        /// </summary>
        public static Unit[] Hero
        {
            get
            {
                return _hero;
            }

            set
            {
                _hero = value;
            }
        }
        /// <summary>
        /// 私人载具
        /// </summary>
        public static Unit[,] Vehicle
        {
            get
            {
                return vehicle;
            }

            set
            {
                vehicle = value;
            }
        }
        /// <summary>
        /// 当前载具
        /// </summary>
        public static Unit[] CurrentVehicle
        {
            get
            {
                return currentVehicle;
            }

            set
            {
                currentVehicle = value;
            }
        }
        /// <summary>
        /// 主单位
        /// </summary>
        public static Unit[] UnitMain
        {
            get
            {
                return unitMain;
            }

            set
            {
                unitMain = value;
            }
        }
        /// <summary>
        /// 控制单位
        /// </summary>
        public static Unit[] UnitControl
        {
            get
            {
                return unitControl;
            }

            set
            {
                unitControl = value;
            }
        }
        /// <summary>
        /// 禁止操作
        /// </summary>
        public static bool[] CanNotOperation
        {
            get
            {
                return _canNotOperation;
            }

            set
            {
                _canNotOperation = value;
            }
        }

        /// <summary>
        /// 键盘按键按下[键,玩家]
        /// </summary>
        public static bool[,] KeyDown
        {
            get
            {
                return _keyDown;
            }

            set
            {
                _keyDown = value;
            }
        }
        /// <summary>
        /// 键盘按键按下的有效状态（因为即便按下也能逻辑否决，所以真实有效按键必须键按下+有效状态同时符合）。禁用玩家操作时总是false，某些情况可设计针对某键禁止操作
        /// </summary>
        public static bool[,] KeyDownState
        {
            get
            {
                return _keyDownState;
            }

            set
            {
                _keyDownState = value;
            }
        }
        /// <summary>
        /// 键盘按键队列
        /// </summary>
        public static bool[] KeyDownLoop
        {
            get
            {
                return _keyDownLoop;
            }

            set
            {
                _keyDownLoop = value;
            }
        }
        /// <summary>
        /// 键盘按键队列数
        /// </summary>
        public static int[] KeyDownLoopOneBitNum
        {
            get
            {
                return _keyDownLoopOneBitNum;
            }

            set
            {
                _keyDownLoopOneBitNum = value;
            }
        }

        /// <summary>
        /// 鼠标左键按下
        /// </summary>
        public static bool[] MouseDownLeft
        {
            get
            {
                return _mouseDownLeft;
            }

            set
            {
                _mouseDownLeft = value;
            }
        }
        /// <summary>
        /// 鼠标中键按下
        /// </summary>
        public static bool[] MouseDownMiddle
        {
            get
            {
                return _mouseDownMiddle;
            }

            set
            {
                _mouseDownMiddle = value;
            }
        }
        /// <summary>
        /// 鼠标右键按下
        /// </summary>
        public static bool[] MouseDownRight
        {
            get
            {
                return _mouseDownRight;
            }

            set
            {
                _mouseDownRight = value;
            }
        }
        /// <summary>
        /// 鼠标按键按下
        /// </summary>
        public static bool[,] MouseDown
        {
            get
            {
                return _mouseDown;
            }

            set
            {
                _mouseDown = value;
            }
        }
        /// <summary>
        /// 鼠标按键按下的有效状态（因为即便按下也能逻辑否决，所以真实有效按键必须键按下+有效状态同时符合）
        /// </summary>
        public static bool[,] MouseDownState
        {
            get
            {
                return _mouseDownState;
            }

            set
            {
                _mouseDownState = value;
            }
        }
        /// <summary>
        /// 鼠标按键队列
        /// </summary>
        public static bool[] MouseDownLoop
        {
            get
            {
                return _mouseDownLoop;
            }

            set
            {
                _mouseDownLoop = value;
            }
        }
        /// <summary>
        /// 鼠标按键队列数
        /// </summary>
        public static int[] MouseDownLoopOneBitNum
        {
            get
            {
                return _mouseDownLoopOneBitNum;
            }

            set
            {
                _mouseDownLoopOneBitNum = value;
            }
        }

        /// <summary>
        /// 按键双击
        /// </summary>
        public static bool[,] KeyDoubleClick
        {
            get
            {
                return _keyDoubleClick;
            }

            set
            {
                _keyDoubleClick = value;
            }
        }
        /// <summary>
        /// 鼠标双击
        /// </summary>
        public static bool[,] MouseDoubleClick
        {
            get
            {
                return _mouseDoubleClick;
            }

            set
            {
                _mouseDoubleClick = value;
            }
        }

        /// <summary>
        /// 鼠标在UI的X坐标
        /// </summary>
        public static int[] MouseUIX
        {
            get
            {
                return _mouseUIX;
            }

            set
            {
                _mouseUIX = value;
            }
        }
        /// <summary>
        /// 鼠标在UI的Y坐标
        /// </summary>
        public static int[] MouseUIY
        {
            get
            {
                return _mouseUIY;
            }

            set
            {
                _mouseUIY = value;
            }
        }
        /// <summary>
        /// 鼠标在世界的X坐标
        /// </summary>
        public static float[] MouseVectorX
        {
            get
            {
                return _mouseVectorX;
            }

            set
            {
                _mouseVectorX = value;
            }
        }
        /// <summary>
        /// 鼠标在世界的Y坐标
        /// </summary>
        public static float[] MouseVectorY
        {
            get
            {
                return _mouseVectorY;
            }

            set
            {
                _mouseVectorY = value;
            }
        }
        /// <summary>
        /// 鼠标点高度，mouseVectorZ=MapHeight+TerrainHeight+Unit.TerrainHeight+Unit.Height
        /// 悬崖、地形物件及单位在移动、诞生摧毁时应将高度信息刷新，以便实时获取
        /// </summary>
        public static float[] MouseVectorZ
        {
            get
            {
                return _mouseVectorZ;
            }

            set
            {
                _mouseVectorZ = value;
            }
        }
        /// <summary>
        /// 修正后的鼠标点高度（扣减了地面高度，所以这是相对地面的高度），mouseVectorZFixed=mouseVectorZ-MapHeight=TerrainHeight+Unit.TerrainHeight+Unit.Height
        /// </summary>
        public static float[] MouseVectorZFixed
        {
            get
            {
                return _mouseVectorZFixed;
            }

            set
            {
                _mouseVectorZFixed = value;
            }
        }
        /// <summary>
        /// 鼠标与玩家控制单位在世界中的2D角度，象限分布：右=0度，上=90°，左=180°，下=270°，可用于调整行走方向
        /// </summary>
        public static float[] MouseToUnitControlAngle
        {
            get
            {
                return _mouseToUnitControlAngle;
            }

            set
            {
                _mouseToUnitControlAngle = value;
            }
        }
        /// <summary>
        /// 鼠标与玩家控制单位在世界中的2D距离
        /// </summary>
        public static float[] MouseToUnitControlRange
        {
            get
            {
                return _mouseToUnitControlRange;
            }

            set
            {
                _mouseToUnitControlRange = value;
            }
        }
        /// <summary>
        /// 鼠标与玩家控制单位在世界中的3D角度，常用于调整鼠标自动镜头
        /// </summary>
        public static float[] MouseToUnitControlRange3F
        {
            get
            {
                return _mouseToUnitControlRange3F;
            }

            set
            {
                _mouseToUnitControlRange3F = value;
            }
        }
        /// <summary>
        /// 相机位置
        /// </summary>
        public static Vector3F[] CameraVector3F
        {
            get
            {
                return _cameraVector3F;
            }

            set
            {
                _cameraVector3F = value;
            }
        }
        /// <summary>
        /// 鼠标3D点向量坐标，修正了鼠标点高度（扣减了地图高度，所以这是相对地面的高度），mouseVectorZFixed=mouseVectorZ-MapHeight=TerrainHeight+Unit.TerrainHeight+Unit.Height
        /// </summary>
        public static Vector3F[] MouseVector3FFixed
        {
            get
            {
                return _mouseVector3FFixed;
            }

            set
            {
                _mouseVector3FFixed = value;
            }
        }
        /// <summary>
        /// 鼠标3D点向量坐标，鼠标Z点在单位高度顶部，Z=MapHeight+TerrainHeight+Unit.TerrainHeight+Unit.Height
        /// </summary>
        public static Vector3F[] MouseVector3F
        {
            get
            {
                return _mouseVector3F;
            }

            set
            {
                _mouseVector3F = value;
            }
        }
        /// <summary>
        /// 鼠标3D点向量坐标，鼠标Z点在单位层地形物件高度顶部（单位脚底），Z=MapHeight+TerrainHeight+Unit.TerrainHeight
        /// </summary>
        public static Vector3F[] MouseVector3FUnitTerrain
        {
            get
            {
                return _mouseVector3FUnitTerrain;
            }

            set
            {
                _mouseVector3FUnitTerrain = value;
            }
        }
        /// <summary>
        /// 鼠标3D点向量坐标，鼠标Z点在悬崖、地形物件顶部，Z=MapHeight+TerrainHeight
        /// </summary>
        public static Vector3F[] MouseVector3FTerrain
        {
            get
            {
                return _mouseVector3FTerrain;
            }

            set
            {
                _mouseVector3FTerrain = value;
            }
        }
        /// <summary>
        /// 鼠标2D点向量坐标
        /// </summary>
        public static Vector2F[] MouseVector2F
        {
            get
            {
                return _mouseVector2F;
            }

            set
            {
                _mouseVector2F = value;
            }
        }

        /// <summary>
        /// 玩家类型（中立Neutral、电脑Ai、用户User、玩家Player、敌人Enemy）
        /// </summary>
        public static string[] Type
        {
            get
            {
                return _type;
            }

            set
            {
                _type = value;
            }
        }
        /// <summary>
        /// 玩家句柄
        /// </summary>
        public static string[] Handle
        {
            get
            {
                return _handle;
            }

            set
            {
                _handle = value;
            }
        }
        /// <summary>
        /// 判断Player是否为本机用户
        /// </summary>
        public static bool[] LocalUser
        {
            get
            {
                return _localUser;
            }

            set
            {
                _localUser = value;
            }
        }
        /// <summary>
        /// 用户状态（"Online"、"Offline"、其他状态字符）
        /// </summary>
        public static string[] Status
        {
            get
            {
                return _status;
            }

            set
            {
                _status = value;
            }
        }
        /// <summary>
        /// 所属队伍ID
        /// </summary>
        public static int[] TeamID
        {
            get
            {
                return _teamID;
            }

            set
            {
                _teamID = value;
            }
        }

        #endregion

        public static void ModifyProperty(int player, PlayerProp playerProp, PlayerPropOp playerPropOp, double value) { }

    }
}
