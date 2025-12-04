using CellSpace;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpriteSpace
{
    /// <summary>
    /// 玩家.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// 场景
        /// </summary>
        public Scene scene;
        /// <summary>
        /// 舞台
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 保存底层游戏物体相关资源,内含静态对象池Stack<GO>
        /// </summary>
        public GO go;
        /// <summary>
        /// 保存底层游戏物体相关资源(minimap用到的那份),内含静态对象池Stack<GO>
        /// </summary>
        public GO mgo;
        /// <summary>
        /// 原始动画帧播放速度(Value = 4.5f * Scene.gridSize / Scene.tps),大约每秒移动4.5格(若设计一格一帧那就要播放4.5帧动画)
        /// </summary>
        public float defaultMoveSpeed;
        /// <summary>
        /// 原始动画帧播放速度的倒数
        /// </summary>
        public float _1_defaultMoveSpeed;
        /// <summary>
        /// 帧动画前进速度(针对defaultMoveSpeed)
        /// </summary>
        public float frameAnimIncrease;
        /// <summary>
        /// 显示放大修正
        /// </summary>
        public float displayScale = 1f;
        /// <summary>
        /// [逻辑坐标]原始半径(数字越大则实际采用越小),默认是Scene.gridSize/2大小
        /// </summary>
        public float defaultRadius;
        /// <summary>
        /// 当前动画帧下标
        /// </summary>
        public float frameIndex = 0;
        /// <summary>
        /// 根据移动方向判断要不要反转精灵图片X方向
        /// </summary>
        public bool flipX;
        /// <summary>
        /// [逻辑坐标]半径. 该数值和玩家体积同步
        /// </summary>
        public float radius;
        /// <summary>
        /// 逻辑坐标
        /// </summary>
        public float pixelRow;
        /// <summary>
        /// 逻辑坐标
        /// </summary>
        public float pixelColumn;
        /// <summary>
        /// [逻辑坐标]玩家历史位置数组(用来追击以显得怪笨)
        /// </summary>
        public List<Vector2> positionHistory = new List<Vector2>();                       //todo:需要自己实现一个 ring buffer 避免 move
        /// <summary>
        /// 俯视角下的角色前进方向弧度值(可理解为朝向)
        /// </summary>
        public float Radians
        {
            get { return Mathf.Atan2(Scene.inputActions.PlayerDirection.y, Scene.inputActions.PlayerDirection.x); }
        }
        /// <summary>
        /// 退出无敌状态的时间点(单位:帧)
        /// </summary>
        public int quitInvincibleTime;
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int hp = 1000000;
        /// <summary>
        /// //生命值上限
        /// </summary>
        public int maxHp = 1000000;
        /// <summary>
        /// 当前基础伤害倍率(技能上的为实际伤害值,此值可作各种倍率如暴击)
        /// </summary>
        public int damage = 10;
        /// <summary>
        /// 防御力
        /// </summary>
        public int defense = 10;
        /// <summary>
        /// 暴击率
        /// </summary>
        public float criticalRate = 0.2f;
        /// <summary>
        /// 暴击伤害倍率
        /// </summary>
        public float criticalDamageRatio = 1.5f;
        /// <summary>
        /// 闪避率
        /// </summary>
        public float dodgeRate = 0.05f;
        /// <summary>
        /// [逻辑坐标]移动速度(当前每帧移动距离),默认值=0.2f* scene.gridSize
        /// </summary>
        public float moveSpeed;
        /// <summary>
        /// 受伤短暂无敌时长( 帧 )
        /// </summary>
        public int getHurtInvincibleTimeSpan = 6;
        /// <summary>
        /// 玩家技能数组
        /// </summary>
        public List<PlayerSkill> skills = new();
        /// <summary>
        /// 复用型坐标缓存.用于每帧刷新玩家坐标时不产生GC(如果每次new Vector3会导致大量GC).
        /// </summary>
        public Vector3 posCache = Vector3.one;

        /// <summary>
        /// [构造函数]玩家
        /// </summary>
        /// <param name="scene_">场景</param>
        public Player(Scene scene_)
        {
            scene = scene_;

            defaultMoveSpeed = (int)(4.5f * scene.gridSize / scene.tps);
            _1_defaultMoveSpeed = 1f / defaultMoveSpeed;
            frameAnimIncrease = _1_defaultMoveSpeed;
            defaultRadius = scene.gridSize / 2;
            radius = defaultRadius;
            moveSpeed = 0.2f * scene.gridSize;

            GO.Pop(ref go);//取出结构体
            go.Enable();//激活游戏物体

            if (scene.mGOCreate)
            {
                GO.Pop(ref mgo, 3);//取出小地图专用结构体覆盖到mgo,游戏物体所在层设为3
                mgo.Enable();//激活游戏物体
                mgo.spriteRenderer.sprite = scene.sprites_player[0];//初始化玩家精灵
                mgo.spriteRenderer.material = scene.minimapMaterial;//初始化材质,拖入编辑器GUI字段的预制体会在这里赋值时自动实例化,频繁调用多次应使用Instance()后的实例来赋值以节省内存
                mgo.transform.localScale = new Vector3(1, 1, 1);//初始化本地缩放
                if (CPEngine.singleLayerTerrainMode)
                {//3D单层地形模式
                    mgo.transform.rotation = Quaternion.Euler(90, 0, 0);
                }
                else
                {//正常3D模式
                    mgo.transform.rotation = Quaternion.Euler(90, 0, 0);
                }
            }
        }

        /// <summary>
        /// 玩家初始化
        /// </summary>
        /// <param name="lv_stage">舞台</param>
        /// <param name="lv_pixelRow">逻辑坐标</param>
        /// <param name="lv_pixelColumn">逻辑坐标</param>
        public void Init(Stage lv_stage, float lv_pixelRow, float lv_pixelColumn)
        {
            stage = lv_stage;
            pixelRow = lv_pixelRow;
            pixelColumn = lv_pixelColumn;

            //预填充一些玩家历史(逻辑)坐标数据防越界
            positionHistory.Clear();
            var p = new Vector2(pixelRow, pixelColumn);
            for (int i = 0; i < scene.tps; i++)
            {
                positionHistory.Add(p);
            }
        }

        /// <summary>
        /// 玩家逻辑坐标初始化
        /// </summary>
        /// <param name="lv_pixelRow">逻辑坐标</param>
        /// <param name="lv_pixelColumn">逻辑坐标</param>
        public void InitPosition(float lv_pixelRow, float lv_pixelColumn)
        {
            pixelRow = lv_pixelRow;
            pixelColumn = lv_pixelColumn;

            //预填充一些玩家历史(逻辑)坐标数据防越界
            positionHistory.Clear();
            var p = new Vector2(pixelRow, pixelColumn);
            for (int i = 0; i < scene.tps; i++)
            {
                positionHistory.Add(p);
            }
        }

        /// <summary>
        /// 更新玩家
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            // 玩家控制移动(条件:还活着)
            if (hp > 0 && Scene.inputActions.playerMoving)
            {
                var mv = Scene.inputActions.playerMoveValue;//得到方向向量
                pixelRow += mv.x * moveSpeed;
                pixelColumn += mv.y * moveSpeed;
                //Debug.Log("(" + pixelRow + "," + pixelColumn + ")");
                //判断绘制X坐标要不要翻转
                if (flipX && mv.x > 0)
                {
                    flipX = false;
                }
                else if (mv.x < 0)
                {
                    flipX = true;
                }
                //根据移动速度步进动画帧下标
                frameIndex += frameAnimIncrease * moveSpeed * _1_defaultMoveSpeed;
                var len = scene.sprites_player.Length;
                if (frameIndex >= len)
                {
                    frameIndex -= len;
                }

                //强行修正移动范围(理论上讲也可设计一些临时限制,比如boss禁锢)
                if (pixelRow < 0)
                {
                    pixelRow = 0;
                    //Debug.Log("pixelRow < 0 " + "(" + pixelRow + "," + pixelColumn + ")");
                }
                else if (pixelRow >= scene.gridWidth)
                {
                    pixelRow = scene.gridWidth - float.Epsilon;
                    //Debug.Log("pixelRow >=" + scene.gridChunkWidth.ToString() + " (" + pixelRow + "," + pixelColumn + ")");
                }
                if (pixelColumn < 0)
                {
                    pixelColumn = 0;
                    //Debug.Log("pixelColumn < 0 " + "(" + pixelRow + "," + pixelColumn + ")");
                }
                else if (pixelColumn >= scene.gridHeight)
                {
                    pixelColumn = scene.gridHeight - float.Epsilon;
                    //Debug.Log("pixelColumn >=" + scene.gridChunkWidth.ToString() + " (" + pixelRow + "," + pixelColumn + ")");
                }
            }
            //将(逻辑)坐标写入历史记录( 限定长度 )
            positionHistory.Insert(0, new Vector2(pixelRow, pixelColumn));
            if (positionHistory.Count > scene.tps)
            {
                positionHistory.RemoveAt(positionHistory.Count - 1);
            }
            //驱动技能
            {
                var len = skills.Count;
                for (int i = 0; i < len; ++i)
                {
                    skills[i].Update();
                }
            }
            return false;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        public void Draw()
        {
            // 同步帧下标
            go.spriteRenderer.sprite = scene.sprites_player[(int)frameIndex];
            // 同步翻转状态
            go.spriteRenderer.flipX = flipX;
            // 同步 & 坐标系转换
            if (CPEngine.horizontalMode)
            {
                posCache.Set(pixelRow / scene.gridSize, pixelColumn / scene.gridSize, 0);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {//3D单层地形模式
                posCache.Set(pixelRow / scene.gridSize, 1 + scene.aboveHeight, pixelColumn / scene.gridSize); //3D模式地图刷在方块顶面,该高度是1.0(绝对世界坐标)
                go.transform.rotation = Quaternion.Euler(90, 0, 0); //3D模式下把图片转90度
                if (mgo.gameObject != null) mgo.transform.rotation = Quaternion.Euler(90, 0, 0); //小地图游戏物体也转90度
            }
            else
            {//正常3D模式
                Debug.LogError("SpriteSpace框架仅支持2D横板模式（X-Y平面）、3D单层地形模式（X-Z平面）");
            }
            go.transform.position = posCache;
            go.transform.localScale = new Vector3(displayScale, displayScale, displayScale);
            // 短暂无敌用变白表达
            if (scene.time <= quitInvincibleTime)
            {
                go.SetColorWhite();
            }
            else
            {
                go.SetColorDefault();
            }
            if (mgo.gameObject != null) mgo.transform.position = posCache;//小地图游戏物体对象也贴在同一位置
        }

        /// <summary>
        /// [虚方法]绘制编辑器场景小玩意
        /// </summary>
        public void DrawGizmos()
        {
            //绘制实际位置和大小的球形物时,需把逻辑坐标值转为空间的本地相对坐标值(除以scene.gridSize)
            if (CPEngine.horizontalMode)
            {//2D横板模式
                Gizmos.DrawWireSphere(new Vector3(pixelRow / scene.gridSize, pixelColumn / scene.gridSize, 0), radius / scene.gridSize);
            }
            else if (CPEngine.singleLayerTerrainMode)
            {//3D单层地形模式
                //SpriteSpace框架最早时按横板设计的,从X-Y转X-Z需要参数填的时候原Y与原Z交换,角色高度=地图所在高度+aboveHeight
                Gizmos.DrawWireSphere(new Vector3(pixelRow / scene.gridSize, 1 + scene.aboveHeight, pixelColumn / scene.gridSize), radius / scene.gridSize);
            }
            else
            {//正常3D模式
                Debug.LogError("SpriteSpace框架仅支持2D横板模式（X-Y平面）、3D单层地形模式（X-Z平面）");
            }
        }
        /// <summary>
        /// 摧毁
        /// </summary>
        public void Destroy()
        {
            foreach (var skill in skills)
            {
                skill.Destroy();
            }
#if UNITY_EDITOR
            if (go.gameObject != null)
#endif
            {
                GO.Push(ref go);
            }
#if UNITY_EDITOR
            if (mgo.gameObject != null)
#endif
            {
                GO.Push(ref mgo);
            }
        }

        /// <summary>
        /// 令玩家受伤或死亡
        /// </summary>
        /// <param name="monsterDamage"></param>
        public void Hurt(int monsterDamage)
        {
            if (scene.time < quitInvincibleTime) return;    //无敌中直接返回

            var d = (float)monsterDamage;
            d = d * d / (d + defense);
            monsterDamage = (int)d;

            if (hp <= d)
            {
                hp = 0;//玩家死亡
                       //todo:播死亡特效
            }
            else
            {
                hp -= monsterDamage;//玩家减血
                quitInvincibleTime = scene.time + getHurtInvincibleTimeSpan;
                //todo:播受伤特效
            }
        }
    }
}