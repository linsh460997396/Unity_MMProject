using System.Collections.Generic;
using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 怪物
    /// </summary>
    public class Monster : SpaceItem
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
        /// 玩家
        /// </summary>
        public Player player;
        /// <summary>
        /// 关卡怪物数组
        /// </summary>
        public List<Monster> monsters;
        /// <summary>
        /// 关卡怪数组的下标
        /// </summary>
        public int indexOfContainer;
        /// <summary>
        /// 动画帧数组
        /// </summary>
        public Sprite[] sprites;
        /// <summary>
        /// 底层资源对象池（如怪物）
        /// </summary>
        public GO go;
        /// <summary>
        /// 底层资源对象池（小地图用到的那份）
        /// </summary>
        public GO mgo;
        /// <summary>
        /// [逻辑坐标]原始移动速度
        /// </summary>
        public const float defaultMoveSpeed = 20;
        /// <summary>
        /// [逻辑坐标]原始移动速度的倒数
        /// </summary>
        public const float _1_defaultMoveSpeed = 1f / defaultMoveSpeed; //使除法转为乘法
        /// <summary>
        /// 帧动画前进速率（仅针对原始移动速度）
        /// </summary>
        public const float frameAnimIncrease = 1f / 5;
        /// <summary>
        /// 基础显示缩放比例（默认值=1f）
        /// </summary>
        public const float displayBaseScale = 1f;
        /// <summary>
        /// [逻辑坐标]原始半径范围（数字越大则实际采用越小），默认是Scene.gridSize/2大小
        /// </summary>
        public const float defaultRadius = Scene.gridSize / 2;
        /// <summary>
        /// [逻辑坐标]原始半径范围的倒数
        /// </summary>
        public const float _1_defaultRadius = 1f / defaultRadius;
        /// <summary>
        /// 当前动画帧下标
        /// </summary>
        public float frameIndex = 0;
        /// <summary>
        /// 根据移动方向判断要不要反转精灵的X方向
        /// </summary>
        public bool flipX;
        /// <summary>
        /// 弧度（最后移动朝向）
        /// </summary>
        public float radians;
        /// <summary>
        /// [逻辑坐标]追赶玩家时的目标偏移量(防止放风筝时重叠到一起)
        /// </summary>
        public float tarOffsetX, tarOffsetY;
        /// <summary>
        /// 瞄准玩家的延迟（0~59）（获取玩家逻辑坐标用history，这里是下标）
        /// </summary>
        public int aimDelay = 29;
        /// <summary>
        /// 怪物伤害值
        /// </summary>
        public int damage = 10;
        /// <summary>
        /// 怪物生命值
        /// </summary>
        public int hp = 20;
        /// <summary>
        /// [逻辑坐标]每一帧的移动距离
        /// </summary>
        public float moveSpeed = 5f;
        /// <summary>
        /// 被击退结束时间点
        /// </summary>
        public int knockbackEndTime;
        /// <summary>
        /// 被击退移动增量衰减值
        /// </summary>
        public float knockbackDecay = 0.01f;
        /// <summary>
        /// 被击退移动增量倍率，每帧-=decay
        /// </summary>
        public float knockbackIncRate = 1f;
        /// <summary>
        /// [逻辑坐标]被击退移动增量，实际pixelX+=inc*rate
        /// </summary>
        public float knockbackIncX, knockbackIncY;
        /// <summary>
        /// 受伤变白结束时间
        /// </summary>
        public int whiteEndTime;
        /// <summary>
        /// 受伤变白的时长（反复受伤就会重置变白结束时间）
        /// </summary>
        public const int whiteDelay = 10;

        //todo：血量，显示伤害文字支持

        /// <summary>
        /// [构造函数]怪物
        /// </summary>
        /// <param name="stage_"></param>
        protected Monster(Stage stage_)
        {
            //初始化容器为关卡参数的怪物空间容器
            spaceContainer = stage_.monstersSpaceContainer;
            stage = stage_;//初始化关卡
            player = stage_.player;//初始化关卡玩家
            scene = stage_.scene;//初始化关卡场景
            monsters = stage_.monsters;//初始化关卡怪物
            indexOfContainer = monsters.Count;//初始化关卡怪物数组下标上限
            monsters.Add(this);//添加怪物（一般是继承怪物父类的子类怪物）
                               //取出结构体对象
            GO.Pop(ref go);
            //去除小地图专用结构体对象
            GO.Pop(ref mgo, 3);
            //设置小地图对象的材质和缩放
            mgo.spriteRenderer.material = scene.minimapMaterial;
            mgo.transform.localScale = new Vector3(1, 1, 1);
            //mgo.spriteRenderer.color = Color.red; //设置小地图怪物对象是红色
        }

        /// <summary>
        /// 怪物初始化
        /// </summary>
        /// <param name="sprites_"></param>
        /// <param name="lv_pixelX">逻辑坐标</param>
        /// <param name="lv_pixelY">逻辑坐标</param>
        public void Init(Sprite[] sprites_, float lv_pixelX, float lv_pixelY)
        {
            sprites = sprites_;//精灵数组赋值
            mgo.spriteRenderer.sprite = sprites[0];//动画帧重置为第一个
            flipX = lv_pixelX >= player.pixelX;//精灵图片默认朝右，如x坐标超过了玩家则翻转
                                               //记录逻辑坐标
            pixelX = lv_pixelX;
            pixelY = lv_pixelY;
            radius = defaultRadius;     //初始化SpaceItem（空间物体）的半径字段为怪物默认半径
            spaceContainer.Add(this);   //放入空间容器
            ResetTargetOffsetXY();      //重置偏移
        }

        /// <summary>
        /// [虚函数]追赶并贴身直接伤害玩家
        /// </summary>
        /// <returns>目前总是返回假，若返回true表示怪需要自杀(自爆消散啥的)，派生类需要处理击退: if (knockbackEndTime >= scene.time) return base.Update();</returns>
        public virtual bool Update()
        {
            if (knockbackEndTime >= scene.time)
            {//时间点处于被击退中
             //逻辑坐标位移
                pixelX += knockbackIncX * knockbackIncRate;
                pixelY += knockbackIncY * knockbackIncRate;
                //衰减
                knockbackIncRate -= knockbackDecay;
                //更新自己在空间索引容器中的（逻辑坐标）位置
                spaceContainer.Update(this);
                return false;
            }

            //以下使用逻辑坐标判断是否已接触到玩家，接触到就造成伤害否则就继续移动
            var dx = player.pixelX - pixelX;
            var dy = player.pixelY - pixelY;
            var dd = dx * dx + dy * dy; //值等于怪物与玩家直线距离的平方
            var r2 = player.radius + radius;//怪物容器半径+玩家体积半径
            if (dd < r2 * r2)
            {//怪物与玩家发生体积碰撞（相切不算）
                player.Hurt(damage);//令玩家受伤或死亡
            }
            else
            {
                //判断是否已到达偏移点，若已到达则重新选择偏移点，否则继续移动
                var ph = player.positionHistory[aimDelay];     //获取玩家历史（逻辑）坐标来追击以显得怪笨
                dx = ph.x - pixelX + tarOffsetX;
                dy = ph.y - pixelY + tarOffsetY;
                dd = dx * dx + dy * dy;
                if (dd < radius * radius)
                {//怪物在玩家历史坐标处（加上偏移后）发生预判碰撞
                    ResetTargetOffsetXY();//重置偏移
                }
                //计算移动方向并增量
                radians = Mathf.Atan2(dy, dx);
                var cos = Mathf.Cos(radians);
                //以移动速度及朝向玩家的角度，刷新怪物坐标到计算点（如果碰到下一帧自会执行碰撞判断）
                pixelX += cos * moveSpeed;
                pixelY += Mathf.Sin(radians) * moveSpeed;
                flipX = cos < 0; //根据cos值判断怪物朝向是否要左右翻转
            }

            //todo:不能太边缘，需留一段安全距离，一是屏幕边缘生成，二是击退，都要留出余量

            //若玩家快速移动导致怪被甩在后面很远可将怪"挪"到玩家前方去，或许直接重新随机坐标位置会比较科学
            //怪移出逻辑边境时强行修正，xy设计为以左下为原点的父级空间容器的本地（相对）坐标就不会<0，容器最大边界=该方向网格数量*网格逻辑像素尺寸。制作出边界事件，可让符合条件的怪物去另一团块（若存在）
            if (pixelX < 0) pixelX = 0;
            else if (pixelX >= Scene.gridChunkWidth) pixelX = Scene.gridChunkWidth - float.Epsilon;
            if (pixelY < 0) pixelY = 0;
            else if (pixelY >= Scene.gridChunkHeight) pixelY = Scene.gridChunkHeight - float.Epsilon;

            //根据移动速度步进动画帧下标
            frameIndex += frameAnimIncrease * moveSpeed * _1_defaultMoveSpeed;
            var len = sprites.Length;//精灵数组长度
            if (frameIndex >= len)
            {//达到动画长度后重置
                frameIndex -= len;
            }
            //更新在空间容器中的位置
            spaceContainer.Update(this);
            return false;
        }

        /// <summary>
        /// [虚函数]怪物根据玩家位置绘制图形（播放精灵）
        /// </summary>
        /// <param name="cx">玩家逻辑位置</param>
        /// <param name="cy">玩家逻辑位置</param>
        public virtual void Draw(float cx, float cy)
        {
            if (pixelX < cx - Scene.designWidth_2 || pixelX > cx + Scene.designWidth_2 || pixelY < cy - Scene.designHeight_2 || pixelY > cy + Scene.designHeight_2)
            {//因人始终是在屏幕中间，只要不在屏幕内就不显示
                go.Disable();//禁用游戏物体
            }
            else
            {//在屏幕内
                go.Enable();//激活游戏物体
                            //同步帧下标
                go.spriteRenderer.sprite = sprites[(int)frameIndex];
                //同步&坐标系转换
                go.transform.position = new Vector3(pixelX / Scene.gridSize, pixelY / Scene.gridSize, 0);
                //同步尺寸缩放(根据半径推送算)
                var s = displayBaseScale * radius * _1_defaultRadius;
                go.transform.localScale = new Vector3(s, s, s);
                //看情况变色
                if (scene.time >= whiteEndTime)
                {
                    go.SetColorNormal();//原色
                }
                else
                {
                    go.SetColorWhite();//白色
                }
            }
            if (pixelX < cx - Scene.designWidth * 2 || pixelX > cx + Scene.designWidth * 2 || pixelY < cy - Scene.designHeight * 2 || pixelY > cy + Scene.designHeight * 2)
            {//怪物出了4倍摄像头范围时，[渲染层]禁用小地图中的游戏物体（但[逻辑层]结构体对象仍可移动）
                mgo.Disable();//[渲染层]禁用游戏物体
            }
            else
            {
                mgo.Enable();//[渲染层]激活游戏物体
                mgo.transform.position = new Vector3(pixelX / Scene.gridSize, pixelY / Scene.gridSize, 0);//[渲染层]设置小地图对象位置
            }
        }

        /// <summary>
        /// [虚函数]绘制编辑器场景窗口小玩意
        /// </summary>
        public virtual void DrawGizmos()
        {
            //绘制编辑器模式下出现在场景的对象
            Gizmos.DrawWireSphere(new Vector3(pixelX / Scene.gridSize, pixelY / Scene.gridSize, 0), radius / Scene.gridSize);
        }

        /// <summary>
        /// [虚函数]摧毁怪物
        /// </summary>
        /// <param name="needRemoveFromContainer">需要从关卡容器删除时为真（不填则默认），否则为假</param>
        public virtual void Destroy(bool needRemoveFromContainer = true)
        {
#if UNITY_EDITOR
            if (go.gameObject != null)
#endif
            {
                GO.Push(ref go);//退回对象池
            }
#if UNITY_EDITOR
            if (mgo.gameObject != null)
#endif
            {
                GO.Push(ref mgo);//退回对象池
            }

            //从空间容器移除
            spaceContainer.Remove(this);

            //需要从关卡容器删除时
            if (needRemoveFromContainer)
            {
                var ms = stage.monsters;//刷新关卡怪物列表
                var lastIndex = ms.Count - 1;//怪物数量-1
                var last = ms[lastIndex];//获取关卡怪物列表最后一个怪物（修改前）
                last.indexOfContainer = indexOfContainer;//刷新关卡怪物列表最后一个怪物的下标（修改后）
                ms[indexOfContainer] = last;//刷新关卡怪物列表最后一个怪物（修改后）
                ms.RemoveAt(lastIndex);//移除怪物列表最后的怪物索引
            }
        }

        /// <summary>
        /// 重置逻辑偏移量（获取玩家体积10倍范围甜甜圈里的随机点）
        /// </summary>
        void ResetTargetOffsetXY()
        {
            //获取玩家体积10被范围甜甜圈里的随机逻辑坐标点（安全距离0.1f）
            var p = stage.GetRndPosDoughnut(player.radius * 10, 0.1f);
            tarOffsetX = p.x;//作为新的逻辑偏移量
            tarOffsetY = p.y;//作为新的逻辑偏移量
        }

        /// <summary>
        /// 令怪受伤并播特效，怪已死亡将从数组移除该怪。
        /// </summary>
        /// <param name="playerBulletDamage"></param>
        /// <param name="knockbackForce"></param>
        /// <returns>返回怪是否已死亡</returns>
        public bool Hurt(int playerBulletDamage, int knockbackForce)
        {
            //结合玩家基础倍率算最终伤害值
            var d = playerBulletDamage * player.damage;
            var b = Random.value <= player.criticalRate;//是否打出暴击
            if (b)
            {
                //暴击修正
                d = (int)(d * player.criticalDamageRatio);
            }
            if (hp <= d)
            {//怪被打死
                new EffectExplosion(stage, pixelX, pixelY, radius * _1_defaultRadius);//播特效
                new EffectNumber(stage, pixelX, pixelY, 0.5f, hp, b);//播特效
                Destroy();//摧毁怪物
                return true;
            }
            else
            {//怪没死
                hp -= d;//扣血
                new EffectNumber(stage, pixelX, pixelY, 0.5f, d, b); //播飙血特效(todo)
                                                                     // 判断击退
                if (knockbackForce > 0)
                {
                    knockbackEndTime = scene.time + knockbackForce;
                    knockbackIncRate = 1;
                    knockbackDecay = 1 / knockbackForce;
                    knockbackIncX = -Mathf.Cos(radians) * moveSpeed;
                    knockbackIncY = -Mathf.Sin(radians) * moveSpeed;
                }
                // 变白一会儿
                whiteEndTime = scene.time + whiteDelay;
                return false;
            }
        }
    }
}