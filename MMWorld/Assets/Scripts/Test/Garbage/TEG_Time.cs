//using MetalMaxSystem;
//using System;
//using System.Diagnostics;
//using System.IO;
//using UnityEngine;
//using Debug = UnityEngine.Debug;

//namespace Test.Example.Garbage
//{
//    public class TEG_Time : MonoBehaviour //加MonoBehaviour的必须是实例类，可继承使用MonoBehaviour下的方法，只有继承MonoBehaviour的脚本才能被附加到游戏物体上成为其组件
//    {
//        public Sprite[] spritesMap;
//        public string[] mapSpriteIDs;

//        void Awake()
//        {

//        }
//        // Start is called before the first frame update
//        void Start()
//        {
//            Stopwatch stopwatch = new Stopwatch();
//            stopwatch.Start();
//            spritesMap = Resources.LoadAll<Sprite>("Textures/大地图纹理集");
//            stopwatch.Stop();
//            if (spritesMap == null || spritesMap.Length == 0)
//            {
//                Debug.LogError("大地图纹理集加载失败！");
//            }
//            else
//            {
//                //约2ms处理完毕
//                Debug.Log("大地图纹理集加载成功！共有 " + spritesMap.Length + " 个精灵" + $"耗时：{stopwatch.ElapsedMilliseconds}ms");
//            }

//            //下面这个读取文本分割出特征纹理ID并写入数组，可能需要1~12ms不等。如果不急等结果的，由于不涉及引擎对象，逻辑可全部交给子线程去跑
//            //如果处理时间更长，要防止主线程阻塞的可以交给协程慢慢跑（给玩家看一个读条），可设置协程状态每帧判断协程处理到哪了，素材有没有加载好等等..

//            #region 文本读取和分割

//            //DateTime startTime = DateTime.Now;
//            //string content = File.ReadAllText(Application.dataPath + "/MapIndex/大地图纹理编号.txt");
//            //DateTime endTime = DateTime.Now;
//            //TimeSpan elapsedTime = endTime - startTime;
//            //Debug.Log("加载大地图纹理编号.txt " + $"耗时：{elapsedTime.TotalMilliseconds}ms");

//            //startTime = DateTime.Now;
//            ////mapSpriteIDs是按大地图网格（256行256列）从左上角逐行扫描下来的特征纹理ID集合
//            ////mapSpriteIDs长度为65536，mapSpriteIDs[Index]=大地图特征纹理ID（Index范围：0~65535），目前按90%相似度扫描出来的特征纹理ID范围：1~162
//            //string[] mapSpriteIDs = content.Split(',');
//            //endTime = DateTime.Now;
//            //elapsedTime = endTime - startTime;
//            //if (mapSpriteIDs == null || mapSpriteIDs.Length == 0)
//            //{
//            //    Debug.LogError("从/MapIndex/大地图纹理编号.txt取得的特征纹理ID数组不能为空");
//            //}
//            //else
//            //{
//            //    Debug.Log("大地图纹理编号分析完毕！长度：" + mapSpriteIDs.Length + " " + $"耗时：{elapsedTime.TotalMilliseconds}ms");
//            //}

//            #endregion

//            //Unity主线程一般默认分配在CPU-0，引擎固定对象实例默认均创建在主线程（便于抓推渲染），在子线程中动用它们需使用回调方法不然会报错
//            //下面我们用这个子线程（底层会随机分配空闲CPU）运行一些纯文本读取动作（减轻主线程压力）
//            TimerUpdate timerUpdate = new TimerUpdate(); //创建1个触发器实例（以子线程运行）
//            timerUpdate.Awake += Trigger_CustomAction_Func; //将需要委托运行的函数注册到Awake事件（该事件在触发器实例启动后运行1次，可在触发器实例属性中设置启动前摇）
//            timerUpdate.TriggerStart(true);//以后台模式运行这个触发器
//                                           //等待子线程完成（要使用精确结果进行下一步时）
//            timerUpdate.Thread.Join();

//            //读取剩余资源
//            //

//            //接下来利用MC插件生成地图，并替换体素上部纹理，只要控制MC插件来管理地图生成和保存读取
//        }

//        // Update is called once per frame
//        void Update()
//        {

//        }

//        #region 功能函数

//        void Trigger_CustomAction_Func(object sender, EventArgs e)
//        {
//            DateTime startTime = DateTime.Now;
//            string content = File.ReadAllText(Application.dataPath + "/MapIndex/大地图纹理编号.txt");
//            DateTime endTime = DateTime.Now;
//            TimeSpan elapsedTime = endTime - startTime;
//            Debug.Log("加载大地图纹理编号.txt " + $"耗时：{elapsedTime.TotalMilliseconds}ms");

//            startTime = DateTime.Now;
//            //mapSpriteIDs是按大地图网格（256行256列）从左上角逐行扫描下来的特征纹理ID集合
//            //mapSpriteIDs长度为65536，mapSpriteIDs[Index]=大地图特征纹理ID（Index范围：0~65535），目前按90%相似度扫描出来的特征纹理ID范围：1~162
//            mapSpriteIDs = content.Split(',');
//            endTime = DateTime.Now;
//            elapsedTime = endTime - startTime;
//            if (mapSpriteIDs == null || mapSpriteIDs.Length == 0)
//            {
//                Debug.LogError("从/MapIndex/大地图纹理编号.txt取得的特征纹理ID数组不能为空");
//            }
//            else
//            {
//                Debug.Log("大地图纹理编号分析完毕！长度：" + mapSpriteIDs.Length + " " + $"耗时：{elapsedTime.TotalMilliseconds}ms");
//            }
//        }

//        #endregion
//    }
//}