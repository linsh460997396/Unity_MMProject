using CellSpace;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSpace
{
    /// <summary>
    /// 怪物.
    /// </summary>
    public class Monster : GridItem
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
        /// 玩家
        /// </summary>
        public Player player;
        /// <summary>
        /// 舞台怪物数组
        /// </summary>
        public List<Monster> monsters;
        /// <summary>
        /// 舞台怪数组的下标
        /// </summary>
        public int indexOfContainer;
        /// <summary>
        /// 动画帧数组
        /// </summary>
        public Sprite[] sprites;
        /// <summary>
        /// 底层资源对象池(如怪物)
        /// </summary>
        public GO go;
        /// <summary>
        /// 底层资源对象池(小地图用到的那份)
        /// </summary>
        public GO mgo;
        /// <summary>
        /// [逻辑坐标]原始移动速度
        /// </summary>
        public float defaultMoveSpeed = 20;
        /// <summary>
        /// [逻辑坐标]原始移动速度的倒数
        /// </summary>
        public float _1_defaultMoveSpeed;
        /// <summary>
        /// 帧动画前进速率(仅针对原始移动速度)
        /// </summary>
        public float frameAnimIncrease;
        /// <summary>
        /// 基础显示缩放比例(默认值=1f)
        /// </summary>
        public float displayBaseScale = 1f;
        /// <summary>
        /// [逻辑坐标]原始半径范围(数字越大则实际采用越小),默认是scene.gridSize/2大小
        /// </summary>
        public float defaultRadius;
        /// <summary>
        /// [逻辑坐标]原始半径范围的倒数
        /// </summary>
        public float _1_defaultRadius;
        /// <summary>
        /// 当前动画帧下标
        /// </summary>
        public float frameIndex = 0;
        /// <summary>
        /// 根据移动方向判断要不要反转精灵的X方向
        /// </summary>
        public bool flipX;
        /// <summary>
        /// 弧度(最后移动朝向)
        /// </summary>
        public float radians;
        /// <summary>
        /// [逻辑坐标]追赶玩家时的目标偏移量(防止放风筝时重叠到一起)
        /// </summary>
        public float tarOffsetRow, tarOffsetColumn;
        /// <summary>
        /// 瞄准玩家的延迟(aimDelay必须在设计帧率fps范围内)(获取玩家逻辑坐标用history,这里是下标)
        /// </summary>
        public int aimDelay;
        /// <summary>
        /// 怪物伤害值
        /// </summary>
        public int damage = 10;
        /// <summary>
        /// 怪物生命值
        /// </summary>
        public int hp = 100;
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
        /// 被击退移动增量倍率,每帧-=decay
        /// </summary>
        public float knockbackIncRate = 1f;
        /// <summary>
        /// [逻辑坐标]被击退移动增量,实际pixelRow+=inc*rate
        /// </summary>
        public float knockbackIncX, knockbackIncY;
        /// <summary>
        /// 受伤变白结束时间
        /// </summary>
        public int whiteEndTime;
        /// <summary>
        /// 受伤变白的时长(反复受伤就会重置变白结束时间)
        /// </summary>
        public int whiteDelay = 10;

        //todo:血量,显示伤害文字支持

        /// <summary>
        /// [构造函数]怪物
        /// </summary>
        /// <param name="stage_"></param>
        protected Monster(Stage stage_)
        {
            

            //初始化容器为舞台参数的怪物空间容器
            container = stage_.monstersGridContainer;
            stage = stage_;//初始化舞台
            player = stage_.player;//初始化舞台玩家
            scene = stage_.scene;//初始化舞台场景

            _1_defaultMoveSpeed = 1f / defaultMoveSpeed; //使除法转为乘法
            defaultRadius = scene.gridSize / 2;
            _1_defaultRadius = 1f / defaultRadius;
            aimDelay = scene.tps / 2;
            frameAnimIncrease = 1f / 5;

            monsters = stage_.monsters;//初始化舞台怪物
            indexOfContainer = monsters.Count;//初始化舞台怪物数组下标上限
            monsters.Add(this);//添加怪物(一般是继承怪物父类的子类怪物)
                               //取出结构体对象
            GO.Pop(ref go);

            if (scene.mGOCreate)
            {
                GO.Pop(ref mgo, 3);
                mgo.spriteRenderer.material = scene.minimapMaterial; //设置小地图对象的材质和缩放
                mgo.transform.localScale = new Vector3(1, 1, 1);
                mgo.spriteRenderer.color = Color.red; //设置小地图怪物对象是红色
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
        /// 怪物初始化
        /// </summary>
        /// <param name="sprites_"></param>
        /// <param name="lv_pixelRow">逻辑坐标</param>
        /// <param name="lv_pixelColumn">逻辑坐标</param>
        public void Init(Sprite[] sprites_, float lv_pixelRow, float lv_pixelColumn)
        {
            sprites = sprites_;//精灵数组赋值
            if (mgo.gameObject != null) mgo.spriteRenderer.sprite = sprites[0];//小地图用精灵初始化为第一个,且不刷新变化
            flipX = lv_pixelRow >= player.pixelRow;//精灵图片默认朝右,如x坐标超过了玩家则翻转
                                               //记录逻辑坐标
            pixelRow = lv_pixelRow;
            pixelColumn = lv_pixelColumn;
            range = defaultRadius;     //初始化GridItem(空间物体)的半径字段为怪物默认半径
            container.Add(this);   //放入空间容器
            ResetTargetOffsetRC();      //重置偏移
        }

        /// <summary>
        /// [虚函数]追赶并贴身直接伤害玩家
        /// </summary>
        /// <returns>目前总是返回假,若返回true表示怪需要自杀(自爆消散啥的),派生类需要处理击退: if (knockbackEndTime >= scene.time) return base.Update();</returns>
        public virtual bool Update()
        {
            if (knockbackEndTime >= scene.time)
            {//时间点处于被击退中
             //逻辑坐标位移
                pixelRow += knockbackIncX * knockbackIncRate;
                pixelColumn += knockbackIncY * knockbackIncRate;
                //修正不能出边界
                pixelRow = Mathf.Max(0, pixelRow);
                pixelColumn = Mathf.Max(0, pixelColumn);
                //衰减
                knockbackIncRate -= knockbackDecay;
                //更新自己在空间索引容器中的(逻辑坐标)位置
                container.Update(this);
                return false;
            }

            //以下使用逻辑坐标判断是否已接触到玩家,接触到就造成伤害否则就继续移动
            var dRow = player.pixelRow - pixelRow;
            var dColumn = player.pixelColumn - pixelColumn;
            var dd = dRow * dRow + dColumn * dColumn; //值等于怪物与玩家直线距离的平方
            var r2 = player.radius + range;//怪物容器半径+玩家体积半径
            if (dd < r2 * r2)
            {//怪物与玩家发生体积碰撞(相切不算)
                player.Hurt(damage);//令玩家受伤或死亡
            }
            else
            {
                //判断是否已到达偏移点,若已到达则重新选择偏移点,否则继续移动
                var ph = player.positionHistory[aimDelay];     //获取玩家历史(逻辑)坐标来追击以显得怪笨
                dRow = ph.x - pixelRow + tarOffsetRow;
                dColumn = ph.y - pixelColumn + tarOffsetColumn;
                dd = dRow * dRow + dColumn * dColumn;
                if (dd < range * range)
                {//怪物在玩家历史坐标处(加上偏移后)发生预判碰撞
                    ResetTargetOffsetRC();//重置偏移
                }
                //计算移动方向并增量
                radians = Mathf.Atan2(dColumn, dRow);
                var cos = Mathf.Cos(radians);
                //以移动速度及朝向玩家的角度,刷新怪物坐标到计算点(若碰到下一帧自会执行碰撞判断)
                pixelRow += cos * moveSpeed;
                pixelColumn += Mathf.Sin(radians) * moveSpeed;
                flipX = cos < 0; //根据cos值判断怪物朝向是否要左右翻转
            }

            //todo:不能太边缘,需留一段安全距离,一是屏幕边缘生成,二是击退,都要留出余量

            //若玩家快速移动导致怪被甩在后面很远可将怪"挪"到玩家前方去,或许直接重新随机坐标位置会比较科学
            //怪移出逻辑边境时强行修正,xy设计为以左下为原点的父级空间容器的本地(相对)坐标就不会<0,容器最大边界=该方向网格数量*网格逻辑像素尺寸.制作出边界事件,可让符合条件的怪物去另一团块(若存在)
            if (pixelRow < 0) pixelRow = 0;
            else if (pixelRow >= scene.gridWidth) pixelRow = scene.gridWidth - float.Epsilon;
            if (pixelColumn < 0) pixelColumn = 0;
            else if (pixelColumn >= scene.gridHeight) pixelColumn = scene.gridHeight - float.Epsilon;

            //根据移动速度步进动画帧下标
            frameIndex += frameAnimIncrease * moveSpeed * _1_defaultMoveSpeed;
            var len = sprites.Length;//精灵数组长度
            if (frameIndex >= len)
            {//达到动画长度后重置
                frameIndex -= len;
            }
            //更新在空间容器中的位置
            container.Update(this);
            return false;
        }

        /// <summary>
        /// [虚函数]怪物根据玩家位置绘制图形(播放精灵)
        /// </summary>
        /// <param name="row">玩家逻辑位置</param>
        /// <param name="column">玩家逻辑位置</param>
        public virtual void Draw(float row, float column)
        {
            if (pixelRow < row - scene.designWidth_2 || pixelRow > row + scene.designWidth_2 || pixelColumn < column - scene.designHeight_2 || pixelColumn > column + scene.designHeight_2)
            {//因人始终是在屏幕中间,只要不在屏幕内就不显示
                go.Disable();//禁用游戏物体
            }
            else
            {//在屏幕内
                go.Enable();//激活游戏物体
                go.spriteRenderer.sprite = sprites[(int)frameIndex]; //同步帧下标

                if (CPEngine.horizontalMode)
                {
                    //同步&坐标系转换
                    go.transform.position = new Vector3(pixelRow / scene.gridSize, pixelColumn / scene.gridSize, 0);
                }
                else if (CPEngine.singleLayerTerrainMode)
                {//3D单层地形模式
                    go.transform.position = new Vector3(pixelRow / scene.gridSize, 1 + scene.aboveHeight, pixelColumn / scene.gridSize);
                    go.transform.rotation = Quaternion.Euler(90, 0, 0); //3D模式下把图片转90度
                    if (mgo.gameObject != null) mgo.transform.rotation = Quaternion.Euler(90, 0, 0); //小地图游戏物体也转90度
                }
                else
                {//正常3D模式
                    Debug.LogError("SpriteSpace框架仅支持2D横板模式（X-Y平面）、3D单层地形模式（X-Z平面）");
                }
                //同步尺寸缩放(根据半径推送算)
                var s = displayBaseScale * range * _1_defaultRadius;
                go.transform.localScale = new Vector3(s, s, s);
                //看情况变色
                if (scene.time >= whiteEndTime)
                {
                    go.SetColorDefault();//原色
                }
                else
                {
                    go.SetColorWhite();//白色
                }
            }

            if (mgo.gameObject != null)
            {
                if (pixelRow < row - scene.designWidth * 2 || pixelRow > row + scene.designWidth * 2 || pixelColumn < column - scene.designHeight * 2 || pixelColumn > column + scene.designHeight * 2)
                {//怪物出了4倍摄像头范围时,[渲染层]禁用小地图中的游戏物体(但[逻辑层]结构体对象仍可移动)
                    mgo.Disable();//[渲染层]禁用游戏物体
                }
                else
                {
                    mgo.Enable();//[渲染层]激活游戏物体
                    if (CPEngine.horizontalMode)
                    {
                        mgo.transform.position = new Vector3(pixelRow / scene.gridSize, pixelColumn / scene.gridSize, 0);//[渲染层]设置小地图对象位置
                    }
                    else if (CPEngine.singleLayerTerrainMode)
                    {//3D单层地形模式
                        mgo.transform.position = new Vector3(pixelRow / scene.gridSize, 1 + scene.aboveHeight, pixelColumn / scene.gridSize);//3D模式调整
                    }
                    else
                    {//正常3D模式
                        Debug.LogError("SpriteSpace框架仅支持2D横板模式（X-Y平面）、3D单层地形模式（X-Z平面）");
                    }
                }
            }
        }

        /// <summary>
        /// [虚函数]绘制编辑器场景窗口小玩意
        /// </summary>
        public virtual void DrawGizmos()
        {
            if (CPEngine.horizontalMode)
            {
                //绘制编辑器模式下出现在场景的对象
                Gizmos.DrawWireSphere(new Vector3(pixelRow / scene.gridSize, pixelColumn / scene.gridSize, 0), range / scene.gridSize);
            }
            else
            {
                //3D模式调整
                Gizmos.DrawWireSphere(new Vector3(pixelRow / scene.gridSize, 1 + scene.aboveHeight, pixelColumn / scene.gridSize), range / scene.gridSize);
            }
        }

        /// <summary>
        /// [虚函数]摧毁怪物
        /// </summary>
        /// <param name="needRemoveFromContainer">需要从舞台容器删除时为真(不填则默认),否则为假</param>
        public virtual void Destroy(bool needRemoveFromContainer = true)
        {
            if (go.gameObject != null)
            {
                GO.Push(ref go);//退回对象池
            }
            if (mgo.gameObject != null)
            {
                GO.Push(ref mgo);//退回对象池
            }

            //从空间容器移除
            container.Remove(this);

            //需要从舞台容器删除时
            if (needRemoveFromContainer)
            {
                var ms = stage.monsters;//刷新舞台怪物列表
                var lastIndex = ms.Count - 1;//怪物数量-1
                var last = ms[lastIndex];//获取舞台怪物列表最后一个怪物(修改前)
                last.indexOfContainer = indexOfContainer;//刷新舞台怪物列表最后一个怪物的下标(修改后)
                ms[indexOfContainer] = last;//刷新舞台怪物列表最后一个怪物(修改后)
                ms.RemoveAt(lastIndex);//移除怪物列表最后的怪物索引
            }
        }

        /// <summary>
        /// 重置逻辑偏移量(获取玩家体积10倍范围甜甜圈里的随机点)
        /// </summary>
        void ResetTargetOffsetRC()
        {
            //获取玩家体积10被范围甜甜圈里的随机逻辑坐标点(安全距离0.1f)
            var p = stage.GetRndPosDoughnut(player.radius * 10, 0.1f);
            tarOffsetRow = p.x;//作为新的逻辑偏移量
            tarOffsetColumn = p.y;//作为新的逻辑偏移量
        }

        /// <summary>
        /// 令怪受伤并播特效,怪已死亡将从数组移除该怪.
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
                new EffectExplosion(stage, pixelRow, pixelColumn, range * _1_defaultRadius);//播特效
                new EffectNumber(stage, pixelRow, pixelColumn, 0.5f, hp, b);//播特效
                Destroy();//摧毁怪物
                return true;
            }
            else
            {//怪没死
                hp -= d;//扣血
                new EffectNumber(stage, pixelRow, pixelColumn, 0.5f, d, b); //播飙血特效(todo)
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