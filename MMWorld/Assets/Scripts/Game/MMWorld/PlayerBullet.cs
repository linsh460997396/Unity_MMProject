using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 玩家子弹（发射物）
    /// </summary>
    public class PlayerBullet
    {
        //快捷指向

        /// <summary>
        /// 场景
        /// </summary>
        public Scene scene;
        /// <summary>
        /// 关卡
        /// </summary>
        public Stage stage;
        /// <summary>
        /// 玩家
        /// </summary>
        public Player player;
        /// <summary>
        /// 玩家技能
        /// </summary>
        public PlayerSkill skill;
        /// <summary>
        /// 怪物空间容器
        /// </summary>
        public SpaceContainer monstersSpaceContainer;
        /// <summary>
        /// 对象池结构体
        /// </summary>
        public GO go;
        /// <summary>
        /// 显示放大修正
        /// </summary>
        public const float displayBaseScale = 1f;
        /// <summary>
        /// [逻辑坐标]原始半径（数字越大则实际采用越小），默认是Scene.gridSize/2大小
        /// </summary>
        public const float defaultRadius = Scene.gridSize / 2;
        /// <summary>
        /// [逻辑坐标]原始半径的倒数
        /// </summary>
        public const float _1_defaultRadius = 1f / defaultRadius;
        /// <summary>
        /// 逻辑坐标
        /// </summary>
        public float pixelX, pixelY;
        /// <summary>
        /// 弧度
        /// </summary>
        public float radians;
        /// <summary>
        /// [逻辑坐标]每帧的移动增量
        /// </summary>
        public float incX, incY;
        /// <summary>
        /// 自杀时间点（单位：帧）
        /// </summary>
        public int lifeEndTime;

        //下面这些属性从skill赋值

        /// <summary>
        /// [逻辑坐标]碰撞检测半径(和显示放大修正配套)
        /// </summary>
        public float radius;
        /// <summary>
        /// 伤害(倍率)
        /// </summary>
        public int damage;
        /// <summary>
        /// [逻辑坐标]按照 fps 来算的每一帧的移动距离
        /// </summary>
        public float moveSpeed;
        /// <summary>
        /// 子弹存在时长( 帧 ): fps * 秒
        /// </summary>
        public int life;

        /// <summary>
        /// [构造函数]玩家子弹（发射物）
        /// </summary>
        /// <param name="ps"></param>
        public PlayerBullet(PlayerSkill ps)
        {
            skill = ps;//初始化玩家技能
            player = ps.player;//初始化玩家
            stage = ps.stage;//初始化关卡
            scene = ps.scene;//初始化场景
            monstersSpaceContainer = stage.monstersSpaceContainer;//初始化怪物空间容器
            stage.playerBullets.Add(this);//添加自己到关卡的玩家发射物列表

            //属性复制
            radius = ps.radius;
            damage = ps.damage;
            moveSpeed = ps.moveSpeed;
            life = ps.life;
            // ...
        }

        /// <summary>
        /// 初始化玩家子弹（发射物）
        /// </summary>
        /// <param name="x_">逻辑坐标</param>
        /// <param name="y_">逻辑坐标</param>
        /// <param name="radians_">朝向</param>
        /// <param name="cos_"></param>
        /// <param name="sin_"></param>
        /// <returns></returns>
        public PlayerBullet Init(float x_, float y_, float radians_, float cos_, float sin_)
        {
            //从对象池分配U3D底层对象
            GO.Pop(ref go);
            go.spriteRenderer.sprite = scene.sprites_bullets[1];//默认使用第二个子弹（第一个是箭头状）
            go.transform.rotation = Quaternion.Euler(0, 0, -radians_ * (180f / Mathf.PI));
            lifeEndTime = life + scene.time;
            radians = radians_;
            pixelX = x_;
            pixelY = y_;
            incX = cos_ * moveSpeed;
            incY = sin_ * moveSpeed;
            return this;
        }

        /// <summary>
        /// [虚方法]更新玩家子弹（发射物）
        /// </summary>
        /// <returns></returns>
        public virtual bool Update()
        {

            //在9宫范围内查询首个相交物体
            var m = monstersSpaceContainer.FindFirstCrossBy9(pixelX, pixelY, radius);
            if (m != null)
            {
                ((Monster)m).Hurt(damage, 0);
                return true;    //销毁自己
            }

            //让子弹直线移动
            pixelX += incX;
            pixelY += incY;

            //坐标超出grid地图范围：自杀（或转移到下一个团块空间）
            if (pixelX < 0 || pixelX >= Scene.gridChunkWidth || pixelY < 0 || pixelY >= Scene.gridChunkHeight) return true;

            //生命周期完结：自杀
            return lifeEndTime < scene.time;
        }

        /// <summary>
        /// [虚方法]绘制玩家子弹（发射物）
        /// </summary>
        /// <param name="cx">玩家逻辑位置</param>
        /// <param name="cy">玩家逻辑位置</param>
        public virtual void Draw(float cx, float cy)
        {
            //因为人始终是在屏幕中间，只要不在屏幕内就不显示
            if (pixelX < cx - Scene.designWidth_2 || pixelX > cx + Scene.designWidth_2 || pixelY < cy - Scene.designHeight_2 || pixelY > cy + Scene.designHeight_2)
            {
                go.Disable();
            }
            else
            {
                go.Enable();

                // 同步 & 坐标系转换( pixelY 坐标需要反转 )
                go.transform.position = new Vector3(pixelX / Scene.gridSize, pixelY / Scene.gridSize, 0);

                // 根据半径同步缩放
                var s = displayBaseScale * radius * _1_defaultRadius;
                go.transform.localScale = new Vector3(s, s, s);
            }
        }

        /// <summary>
        /// [虚方法]绘制编辑器场景小玩意
        /// </summary>
        public virtual void DrawGizmos()
        {
            Gizmos.DrawWireSphere(new Vector3(pixelX / Scene.gridSize, pixelY / Scene.gridSize, 0), radius / Scene.gridSize);
        }

        /// <summary>
        /// [虚方法]摧毁玩家子弹（发射物）
        /// </summary>
        public void Destroy()
        {
#if UNITY_EDITOR
            if (go.gameObject != null)           // unity 点击停止按钮后，这些变量似乎有可能提前变成 null
#endif
            {
                //将U3D底层对象（游戏物体）失活后退回池
                GO.Push(ref go);
            }
            //hitBlackList = null;
        }
    }
}