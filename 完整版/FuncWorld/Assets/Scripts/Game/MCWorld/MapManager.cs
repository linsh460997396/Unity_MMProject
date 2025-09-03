using UnityEngine;
using System;
using System.Collections;
using Uniblocks;
using Index = Uniblocks.Index;

namespace MCWorld
{

    /// <summary>
    /// 地图管理器,控制地图生成.
    /// 组件用法:Unity中随便新建一个空对象“Manager”,把脚本拖到组件位置即挂载(Unity要求一个cs文件只能一个类,且类名须与文件名一致),地形会在角色周围产生且随角色移动实时刷新.
    /// (注:本类属于自定义测试用,非Uniblocks插件核心组件,效果等同ChunkLoader.cs).
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        private Transform playerTransform;
        private Index LastPos;
        private Index currentPos;

        private void Start()
        {
            playerTransform = GameObject.Find("Player").transform; //找到Player的转换组件,届时在玩家周围刷地图

            //InvokeRepeating用于特定时间间隔内执行与游戏自动更新频率不同步的情况,如在游戏开始后1秒执行操作来让主要组件充分调度完毕,然后每隔指定秒执行一次
            InvokeRepeating("InitMap", 1.0f, 0.00625f);//不会新开线程,它是在Unity主线程中间隔执行,且第三个参数在运行过程修改无效(类似GE的周期计时器但有办法重写其调用的内部方法来支持变量)

            //若有主线程创建的实例,在子线程中需使用回调,不然会报错
            //TimerUpdate timerUpdate = new TimerUpdate();
            //timerUpdate.Update += TestFunc;
            //timerUpdate.Duetime = 1000;//前摇等待1s
            //timerUpdate.Period = 500;//500ms执行一次
            //timerUpdate.TriggerStart(true);//后台运行触发
        }

        private void test()
        {
            Debug.Log("timerUpdate!");
        }


        //private void Update()
        //{
        //    //使用InvokeRepeating时关闭Update
        //    InitMap();
        //}

        /// <summary>
        /// TimerUpdate事件下自动函数引用(委托),因无法使用TimerUpdate暂废弃
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitMapFunc(object sender, EventArgs e) { test(); }

        private void InitMap()
        {
            //don'transform load chunks if engine isn'transform initialized yet.刷地图前保证前置核心组件已经初始化完成
            if (!Engine.Initialized || !ChunkManager.Initialized)
            {
                return;
            }

            //don'transform load chunks if multiplayer is enabled but the connection isn'transform established yet
            if (Engine.EnableMultiplayer)
            {
                if (!Network.isClient && !Network.isServer)
                {
                    return;
                }
            }

            //track which chunk we're currently in. If it's different from previous frame, spawn chunks at current position.跟踪我们当前所在的团块,若它与前一帧不同,则在当前位置生成团块
            currentPos = Engine.PositionToChunkIndex(playerTransform.position);

            if (currentPos.IsEqual(LastPos) == false)
            {
                ChunkManager.SpawnChunks(currentPos.x, currentPos.y, currentPos.z);

                // (Multiplayer) update server position.多人游戏模式下更新玩家位置
                if (Engine.EnableMultiplayer && Engine.MultiplayerTrackPosition && Engine.UniblocksNetwork != null)
                {
                    UniblocksClient.UpdatePlayerPosition(currentPos);
                }
            }

            LastPos = currentPos;
        }

        // multiplayer
        public void OnConnectedToServer()
        {
            if (Engine.EnableMultiplayer && Engine.MultiplayerTrackPosition)
            {
                StartCoroutine(InitialPositionAndRangeUpdate());
            }
        }

        IEnumerator InitialPositionAndRangeUpdate()
        {
            while (Engine.UniblocksNetwork == null)
            {
                yield return new WaitForEndOfFrame();
            }
            UniblocksClient.UpdatePlayerPosition(currentPos);
            UniblocksClient.UpdatePlayerRange(Engine.ChunkSpawnDistance);
        }
    }
}