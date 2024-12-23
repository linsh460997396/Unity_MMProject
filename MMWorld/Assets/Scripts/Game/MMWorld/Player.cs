using System.Collections.Generic;
using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class Player
    {
        /// <summary>
        /// 场景
        /// </summary>
        public Scene scene;
        /// <summary>
        /// 关卡
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 保存底层游戏物体相关资源，内含静态对象池Stack<GO>
        /// </summary>
        public GO go;
        /// <summary>
        /// 保存底层游戏物体相关资源（minimap用到的那份），内含静态对象池Stack<GO>
        /// </summary>
        public GO mgo;
        /// <summary>
        /// 原始动画帧播放速度（Value = 4.5f * Scene.gridSize / Scene.fps），大约每秒移动4.5格（如果设计一格一帧那就要播放4.5帧动画）
        /// </summary>
        public const float defaultMoveSpeed = (int)(4.5f * Scene.gridSize / Scene.fps);
        /// <summary>
        /// 原始动画帧播放速度的倒数
        /// </summary>
        public const float _1_defaultMoveSpeed = 1f / defaultMoveSpeed;
        /// <summary>
        /// 帧动画前进速度（针对defaultMoveSpeed）
        /// </summary>
        public const float frameAnimIncrease = _1_defaultMoveSpeed;
        /// <summary>
        /// 显示放大修正
        /// </summary>
        public const float displayScale = 1f;
        /// <summary>
        /// [逻辑坐标]原始半径（数字越大则实际采用越小），默认是Scene.gridSize/2大小
        /// </summary>
        public const float defaultRadius = Scene.gridSize / 2;
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
        public float radius = defaultRadius;
        /// <summary>
        /// 逻辑坐标（gridChunk内左下原点，大Y向上）
        /// </summary>
        public float pixelX, pixelY;
        /// <summary>
        /// [逻辑坐标]玩家历史位置数组（用来追击以显得怪笨）
        /// </summary>
        public List<Vector2> positionHistory = new();                       //todo：需要自己实现一个 ring buffer 避免 move
        /// <summary>
        /// 俯视角下的角色前进方向弧度值(可理解为朝向)
        /// </summary>
        public float radians
        {
            get { return Mathf.Atan2(scene.playerDirection.y, scene.playerDirection.x); }
        }
        /// <summary>
        /// 退出无敌状态的时间点（单位：帧）
        /// </summary>
        public int quitInvincibleTime;
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int hp = 100;
        /// <summary>
        /// //生命值上限
        /// </summary>
        public int maxHp = 100;
        /// <summary>
        /// 当前基础伤害倍率(技能上的为实际伤害值，此值可作各种倍率如暴击)
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
        /// [逻辑坐标]移动速度（当前每帧移动距离），默认值=0.2f* Scene.gridSize
        /// </summary>
        public float moveSpeed = 0.2f * Scene.gridSize;
        /// <summary>
        /// 受伤短暂无敌时长( 帧 )
        /// </summary>
        public int getHurtInvincibleTimeSpan = 6;
        /// <summary>
        /// 玩家技能数组
        /// </summary>
        public List<PlayerSkill> skills = new();

        /// <summary>
        /// [构造函数]玩家
        /// </summary>
        /// <param name="scene_">场景</param>
        public Player(Scene scene_)
        {
            scene = scene_;
            GO.Pop(ref go);//取出结构体
            go.Enable();//激活游戏物体
                        //小地图的
            GO.Pop(ref mgo, 3);//取出小地图专用结构体，游戏物体所在层设为3
            mgo.Enable();//激活游戏物体
            mgo.spriteRenderer.sprite = scene.sprites_player[0];//初始化玩家精灵
            mgo.spriteRenderer.material = scene.minimapMaterial;//初始化材质
            mgo.transform.localScale = new Vector3(1, 1, 1);//初始化本地缩放
        }

        /// <summary>
        /// 玩家初始化
        /// </summary>
        /// <param name="lv_stage">关卡</param>
        /// <param name="lv_pixelX">逻辑坐标</param>
        /// <param name="lv_pixelY">逻辑坐标</param>
        public void Init(Stage lv_stage, float lv_pixelX, float lv_pixelY)
        {
            stage = lv_stage;
            pixelX = lv_pixelX;
            pixelY = lv_pixelY;

            //预填充一些玩家历史（逻辑）坐标数据防越界
            positionHistory.Clear();
            var p = new Vector2(pixelX, pixelY);
            for (int i = 0; i < Scene.fps; i++)
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
            // 玩家控制移动(条件：还活着)
            if (hp > 0 && scene.playerMoving)
            {
                var mv = scene.playerMoveValue;//得到方向向量
                pixelX += mv.x * moveSpeed;
                pixelY += mv.y * moveSpeed;
                //Debug.Log("(" + pixelX + "," + pixelY + ")");
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

                //强行修正移动范围(理论上讲也可设计一些临时限制，比如boss禁锢)
                if (pixelX < 0)
                {
                    pixelX = 0;
                    //Debug.Log("pixelX < 0 " + "(" + pixelX + "," + pixelY + ")");
                }
                else if (pixelX >= Scene.gridChunkWidth)
                {
                    pixelX = Scene.gridChunkWidth - float.Epsilon;
                    //Debug.Log("pixelX >=" + Scene.gridChunkWidth.ToString() + " (" + pixelX + "," + pixelY + ")");
                }
                if (pixelY < 0)
                {
                    pixelY = 0;
                    //Debug.Log("pixelY < 0 " + "(" + pixelX + "," + pixelY + ")");
                }
                else if (pixelY >= Scene.gridChunkHeight)
                {
                    pixelY = Scene.gridChunkHeight - float.Epsilon;
                    //Debug.Log("pixelY >=" + Scene.gridChunkWidth.ToString() + " (" + pixelX + "," + pixelY + ")");
                }
            }
            //将（逻辑）坐标写入历史记录( 限定长度 )
            positionHistory.Insert(0, new Vector2(pixelX, pixelY));
            if (positionHistory.Count > Scene.fps)
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
            var p = new Vector3(pixelX / Scene.gridSize, pixelY / Scene.gridSize, 0);
            go.transform.position = p;
            go.transform.localScale = new Vector3(displayScale, displayScale, displayScale);
            // 短暂无敌用变白表达
            if (scene.time <= quitInvincibleTime)
            {
                go.SetColorWhite();
            }
            else
            {
                go.SetColorNormal();
            }
            mgo.transform.position = p;//小地图游戏物体对象也贴在同一位置
        }

        /// <summary>
        /// [虚方法]绘制编辑器场景小玩意
        /// </summary>
        public void DrawGizmos()
        {
            //绘制实际位置和大小的球形物时，需把逻辑值转为本地值（除以Scene.gridSize）
            Gizmos.DrawWireSphere(new Vector3(pixelX / Scene.gridSize, pixelY / Scene.gridSize, 0), radius / Scene.gridSize);
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
                       //todo：播死亡特效
            }
            else
            {
                hp -= monsterDamage;//玩家减血
                quitInvincibleTime = scene.time + getHurtInvincibleTimeSpan;
                //todo：播受伤特效
            }
        }
    }
}