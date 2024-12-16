using System.Collections.Generic;
using UnityEngine;

namespace MMWorld
{
    /// <summary>
    /// 关卡
    /// </summary>
    public class Stage
    {
        /// <summary>
        /// 场景
        /// </summary>
        public Scene scene;
        /// <summary>
        /// 玩家
        /// </summary>
        public Player player;
        /// <summary>
        /// 关卡索引
        /// </summary>
        public int state;
        /// <summary>
        /// 相机转换
        /// </summary>
        public Transform camTrans;
        /// <summary>
        /// 玩家子弹列表
        /// </summary>
        public List<PlayerBullet> playerBullets = new();
        /// <summary>
        /// 怪物列表
        /// </summary>
        public List<Monster> monsters = new();
        /// <summary>
        /// 怪物空间容器
        /// </summary>
        public SpaceContainer monstersSpaceContainer;
        /// <summary>
        /// 爆炸特效列表
        /// </summary>
        public List<EffectExplosion> effectExplosions = new();
        /// <summary>
        /// 爆炸数字列表
        /// </summary>
        public List<EffectNumber> effectNumbers = new();
        /// <summary>
        /// 怪物生成器列表
        /// </summary>
        public List<MonsterGenerator> monsterGenerators = new();


        /*************************************************************************************************************************/
        /*************************************************************************************************************************/

        //在某些情况下，基类可能包含一些纯虚函数（即没有实现的虚函数），使其成为抽象类。通过将构造函数设为protected（或私有），可以确保这个抽象类不会被直接实例化

        /// <summary>
        /// [构造函数]关卡
        /// </summary>
        /// <param name="scene_"></param>
        protected Stage(Scene scene_)
        {//将构造函数设为protected意味着这个构造函数不能在类的外部直接被调用，从而防止了类的直接实例化
            scene = scene_;
            player = scene_.player;
            monstersSpaceContainer = new(Scene.gridChunkNumRows, Scene.gridChunkNumCols, Scene.gridSize);
            camTrans = Camera.main.transform;
        }


        /// <summary>
        /// [虚方法]关卡更新函数，在派生类可按需要覆盖（使用override）
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        public virtual void Update()
        {
            throw new System.Exception("need impl");
        }

        /// <summary>
        /// [虚方法]绘制图形（播放怪物、玩家子弹（发射物）、爆炸特效、数字等精灵切片），在派生类可按需要覆盖（使用override）
        /// </summary>
        public virtual void Draw()
        {
            // 同步镜头的位置
            camTrans.position = new Vector3(player.pixelX / Scene.gridSize, player.pixelY / Scene.gridSize, camTrans.position.z);

            // 剔除 & 同步 GO

            //玩家逻辑位置
            var cx = player.pixelX;
            var cy = player.pixelY;

            //怪物、玩家子弹（发射物）、爆炸特性、数字特性出现在玩家镜头内则进行绘制
            var len = monsters.Count;
            for (int i = 0; i < len; ++i)
            {
                monsters[i].Draw(cx, cy);
            }
            len = playerBullets.Count;
            for (int i = 0; i < len; ++i)
            {
                playerBullets[i].Draw(cx, cy);
            }
            len = effectExplosions.Count;
            for (int i = 0; i < len; ++i)
            {
                effectExplosions[i].Draw(cx, cy);
            }
            len = effectNumbers.Count;
            for (int i = 0; i < len; ++i)
            {
                effectNumbers[i].Draw(cx, cy);
            }

            //绘制角色自己的图像
            player.Draw();
        }

        /// <summary>
        /// [虚方法]绘制编辑器视图下的场景辅助，在派生类可按需要覆盖（使用override）
        /// </summary>
        public virtual void DrawGizmos()
        {
            var len = monsters.Count;
            for (int i = 0; i < len; ++i)
            {
                monsters[i].DrawGizmos();
            }
            len = playerBullets.Count;
            for (int i = 0; i < len; ++i)
            {
                playerBullets[i].DrawGizmos();
            }
            player.DrawGizmos();
        }

        /// <summary>
        /// [虚方法]关卡摧毁，在派生类可按需要覆盖（使用override）
        /// </summary>
        public virtual void Destroy()
        {
            foreach (var o in monsters)
            {
                o.Destroy(false);             // 纯 destroy，不从 monsters 移除自己
            }
            foreach (var o in playerBullets)
            {
                o.Destroy();
            }
            foreach (var o in effectExplosions)
            {
                o.Destroy();
            }
            foreach (var o in effectNumbers)
            {
                o.Destroy();
            }
            foreach (var o in monsterGenerators)
            {
                o.Destroy();
            }
            // ...

            Debug.Assert(monstersSpaceContainer.numItems == 0);
            monsters.Clear();
            playerBullets.Clear();
            effectExplosions.Clear();
            effectNumbers.Clear();
            // ...
        }


        /*************************************************************************************************************************/
        /*************************************************************************************************************************/



        /// <summary>
        /// 执行怪生成配置并返回是否已经全部执行完毕
        /// </summary>
        /// <returns></returns>
        public int Update_MonstersGenerators()
        {
            var time = scene.time;
            var os = monsterGenerators;
            for (int i = os.Count - 1; i >= 0; i--)
            {
                var mg = os[i];
                if (mg.activeTime <= time)
                {
                    if (mg.destroyTime >= time)
                    {
                        mg.Update();
                    }
                    else
                    {
                        int lastIndex = os.Count - 1;
                        os[i] = os[lastIndex];
                        os.RemoveAt(lastIndex);
                    }
                }
            }
            return os.Count;
        }

        /// <summary>
        /// 驱动所有怪
        /// </summary>
        /// <returns></returns>
        public int Update_Monsters()
        {
            var os = monsters;
            for (int i = os.Count - 1; i >= 0; i--)
            {
                var o = os[i];
                if (o.Update())
                {
                    o.Destroy();    // 会从 容器 自动移除自己
                }
            }
            return os.Count;
        }

        /// <summary>
        /// 驱动所有爆炸特效
        /// </summary>
        /// <returns></returns>
        public int Update_Effect_Explosions()
        {
            var os = effectExplosions;
            for (int i = os.Count - 1; i >= 0; i--)
            {
                var o = os[i];
                if (o.Update())
                {
                    int lastIndex = os.Count - 1;
                    os[i] = os[lastIndex];
                    os.RemoveAt(lastIndex);
                    o.Destroy();
                }
            }
            return os.Count;
        }

        /// <summary>
        /// 驱动所有数字特效
        /// </summary>
        /// <returns></returns>
        public int Update_Effect_Numbers()
        {
            var os = effectNumbers;
            for (int i = os.Count - 1; i >= 0; i--)
            {
                var o = os[i];
                if (o.Update())
                {
                    int lastIndex = os.Count - 1;
                    os[i] = os[lastIndex];
                    os.RemoveAt(lastIndex);
                    o.Destroy();
                }
            }
            return os.Count;
        }

        /// <summary>
        /// 驱动所有玩家子弹
        /// </summary>
        /// <returns></returns>
        public int Update_PlayerBullets()
        {
            var os = playerBullets;
            for (int i = os.Count - 1; i >= 0; i--)
            {
                var o = os[i];
                if (o.Update())
                {
                    int lastIndex = os.Count - 1;
                    os[i] = os[lastIndex];
                    os.RemoveAt(lastIndex);
                    o.Destroy();
                }
            }
            return os.Count;
        }

        /// <summary>
        /// 驱动所有玩家
        /// </summary>
        /// <returns></returns>
        public int Update_Player()
        {
            player.Update();
            return 1;
        }

        /// <summary>
        /// [逻辑坐标]当前玩家所在屏幕区域边缘随机一个点返回
        /// </summary>
        /// <returns></returns>
        public Vector2 GetRndPosOutSideTheArea()
        {
            var e = Random.Range(0, 4);
            switch (e)
            {
                case 0:
                    return new Vector2(player.pixelX + Random.Range(-Scene.designWidth_2, Scene.designWidth_2), player.pixelY - Scene.designHeight_2);
                case 1:
                    return new Vector2(player.pixelX + Random.Range(-Scene.designWidth_2, Scene.designWidth_2), player.pixelY + Scene.designHeight_2);
                case 2:
                    return new Vector2(player.pixelX - Scene.designWidth_2, player.pixelY + Random.Range(-Scene.designWidth_2, Scene.designWidth_2));
                case 3:
                    return new Vector2(player.pixelX + Scene.designWidth_2, player.pixelY + Random.Range(-Scene.designWidth_2, Scene.designWidth_2));
            }
            return Vector2.zero;
        }

        /// <summary>
        /// [逻辑坐标]获取甜甜圈形状里的随机点
        /// </summary>
        /// <param name="maxRadius"></param>
        /// <param name="safeRadius"></param>
        /// <returns></returns>
        public Vector2 GetRndPosDoughnut(float maxRadius, float safeRadius)
        {
            var len = maxRadius - safeRadius;
            var len_radius = len / maxRadius;
            var safeRadius_radius = safeRadius / maxRadius;
            var radius = Mathf.Sqrt(Random.Range(0, len_radius) + safeRadius_radius) * maxRadius;
            var radians = Random.Range(-Mathf.PI, Mathf.PI);
            return new Vector2(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius);
        }

    }
}