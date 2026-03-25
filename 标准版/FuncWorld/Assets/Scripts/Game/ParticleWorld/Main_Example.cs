using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ParticleWorld
{
    public class Main_Example : MonoBehaviour
    {
        #region 这些代码是用来加载AB包的具体方法(若协程调用失败,请复制到主线程入口所在类使用)

        //↓--------------------------------------------------------------------------------------------------------------------↓
        //↓--------------------------------------------Unity_ABTestFuncStart---------------------------------------------------↓
        //↓--------------------------------------------------------------------------------------------------------------------↓
        //注:当这些函数在Unity中使用正常,素材也实例化成功,但进行BepInEx制作MOD时报错:①读取文件报header问题；②读取字节则内存数据无法成功解压
        //上述故障原因跟Unity版本有关,建议用游戏同版本Unity去转AB包素材

        //ABTest_CustomGlobalValues,这些全局字段在下面函数里接收获取到的素材
        [NonSerialized]
        public GameObject[] gameObjectGroup;

        [NonSerialized]
        public AssetBundle assetBundle;
        public IEnumerator currentIEnumerator;
        public Coroutine currentCoroutine;

        [NonSerialized]
        public bool isCoroutineRunning = false;

        /// <summary>
        /// 保存所有活动协程的列表,需要Resource.multiCoroutine = true时可用
        /// </summary>
        //public List<Coroutine> activeCoroutines = new List<Coroutine>(); //取消多协程
        //public bool multiCoroutine = false;

        //public Resource()
        //{
        //    Debug.Log("Prinny: Resource 对象已建立！");
        //}

        //~Resource()
        //{
        //    Debug.Log("Prinny: Resource 对象已摧毁！");
        //}

        //ABTest_CustomFuncTemplates

        /// <summary>
        /// 同步加载: 以文件的形式加载AssetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromFile(
            string path,
            string resName,
            out AssetBundle assetBundle,
            out GameObject gameObject
        )
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            gameObject = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以文件的形式加载AssetBundle,并存储在Resource.gameObjectGroup[0]、Resource.assetBundle.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        public void ABLoadFromFile(string path, string resName)
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            gameObjectGroup[0] = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以byte[] 形式加载AssetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromMemory(
            string path,
            string resName,
            out AssetBundle assetBundle,
            out GameObject gameObject
        )
        {
            assetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
            gameObject = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以byte[] 形式加载AssetBundle,并存储在Resource.gameObjectGroup[0]、Resource.assetBundle.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromMemory(string path, string resName)
        {
            assetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
            gameObjectGroup[0] = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以流的形式加载AssetBundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromStream(
            string path,
            string resName,
            out AssetBundle assetBundle,
            out GameObject gameObject
        )
        {
            assetBundle = AssetBundle.LoadFromStream(File.OpenRead(path));
            gameObject = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 同步加载: 以流的形式加载AssetBundle,并存储在Resource.gameObjectGroup[0]、Resource.assetBundle.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="assetBundle"></param>
        /// <param name="gameObject"></param>
        public void ABLoadFromStream(string path, string resName)
        {
            assetBundle = AssetBundle.LoadFromStream(File.OpenRead(path));
            gameObjectGroup[0] = assetBundle.LoadAsset<GameObject>(resName);
        }

        /// <summary>
        /// 异步加载: 以文件的形式加载AssetBundle,并存储在Resource.gameObjectGroup[0]、Resource.assetBundle.要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        private IEnumerator ABLoadFromFileAsync(string path, string resName)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;
            assetBundle = request.assetBundle;
            gameObjectGroup[0] = request.assetBundle.LoadAsset<GameObject>(resName);
            //在Unity中,协程(Coroutine)会自动管理其生命周期,这意味着一旦协程中的IEnumerator方法执行完毕,协程就会自行结束,不需要显式地调用Stop方法.
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载: 以byte[] 形式加载AssetBundle,并存储在Resource.gameObjectGroup[0]、Resource.assetBundle.要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        private IEnumerator ABLoadFromMemoryAsync(string path, string resName)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(
                File.ReadAllBytes(path)
            );
            yield return request;
            assetBundle = request.assetBundle;
            gameObjectGroup[0] = request.assetBundle.LoadAsset<GameObject>(resName);
            //在Unity中,协程(Coroutine)会自动管理其生命周期,这意味着一旦协程中的IEnumerator方法执行完毕,协程就会自行结束,不需要显式地调用Stop方法.
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载AB包中全资源(存储在Resource.gameObjectGroup、Resource.assetBundle):
        /// 以byte[] 形式加载整个AssetBundle到内存中,而不是从流中加载它,这可能更高效,用于AssetBundle较小或需快速访问AssetBundle中的所有资产.
        /// 但AssetBundle很大则可能会导致内存使用增加,该方法结尾使用StopCoroutine动作在完成加载后停止协程防止方法完成前、被多次调用时继续运行不必要的加载.
        /// 在主角位置实例化示范:
        /// GameObject.Instantiate(gameObjectGroup[0], Main_ParticleWorld.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerator ABLoadAllFromMemoryAsync(string path)
        {
            Debug.Log("Prinny: 进入协程！");
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(
                File.ReadAllBytes(path)
            );
            yield return request;
            Debug.Log("Prinny: 协程处理完成,素材已加载！");
            assetBundle = request.assetBundle;
            gameObjectGroup = assetBundle.LoadAllAssets<GameObject>();
            Debug.Log("Prinny: 素材已赋值给实例字段！");
            //在Unity中,协程(Coroutine)会自动管理其生命周期,这意味着一旦协程中的IEnumerator方法执行完毕,协程就会自行结束,不需要显式地调用Stop方法.
            isCoroutineRunning = false;
            Debug.Log("Prinny: 协程已关闭！");
        }

        /// <summary>
        /// 异步加载: 以流的形式加载AssetBundle.要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        private IEnumerator ABLoadFromStreamAsync(string path, string resName)
        {
            // AssetBundleCreateRequest 类是 AssetBundle 的一个实例,它表示一个异步加载请求.AssetBundle.LoadFromStreamAsync 方法使用指定路径的文件流来创建一个新的 AssetBundleCreateRequest 实例.
            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(File.OpenRead(path));
            // yield return 语句用于将控制权交还调用上下文,直到异步加载请求完成.一旦请求完成,该方法将使用 LoadAsset 方法从 AssetBundle 中加载指定名称的游戏对象.
            // 加载游戏对象后,该方法将通过 yield return 语句返回对 obj 变量的引用,这使得调用代码可以使用该对象.
            // 总的来说,此方法可用于在游戏或应用程序中异步加载 AssetBundle,并检索其中的游戏对象.这可以在需要时帮助优化内存使用和加载时间.
            yield return request;
            assetBundle = request.assetBundle;
            gameObjectGroup[0] = request.assetBundle.LoadAsset<GameObject>(resName);
            //在Unity中,协程(Coroutine)会自动管理其生命周期,这意味着一旦协程中的IEnumerator方法执行完毕,协程就会自行结束,不需要显式地调用Stop方法.
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载: 以文件的形式加载AssetBundle,并存储在Resource.gameObjectGroup[0]、Resource.assetBundle.要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        public void LoadFromFileAsync(string path, string resName)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                currentIEnumerator = ABLoadFromFileAsync(path, resName);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 异步加载: 以byte[] 形式加载AssetBundle,并存储在Resource.gameObjectGroup[0]、Resource.assetBundle.要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        public void LoadFromMemoryAsync(string path, string resName)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                currentIEnumerator = ABLoadFromMemoryAsync(path, resName);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 异步加载AB包中全资源(存储在Resource.gameObjectGroup、Resource.assetBundle):
        /// 以byte[] 形式加载整个AssetBundle到内存中,而不是从流中加载它,这可能更高效,用于AssetBundle较小或需快速访问AssetBundle中的所有资产.
        /// 但AssetBundle很大则可能会导致内存使用增加,该方法结尾使用StopCoroutine动作在完成加载后停止协程防止方法完成前、被多次调用时继续运行不必要的加载.
        /// 在主角位置实例化示范:
        /// GameObject.Instantiate(gameObjectGroup[0], Main_ParticleWorld.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public void LoadAllFromMemoryAsync(string path)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                Debug.Log("Prinny: LoadAllFromMemoryAsync => " + path);
                currentIEnumerator = ABLoadAllFromMemoryAsync(path);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 异步加载: 以流的形式加载AssetBundle.要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
        /// 让没完成的协程继续往下跑:
        /// if (testResource.currentIEnumerator != null)
        /// {
        ///    testResource.currentIEnumerator.MoveNext();
        ///    //检查协程是否完成
        ///    if (testResource.currentIEnumerator == null)
        ///    {
        ///        //完成则手动停止
        ///        StopCoroutine(testResource.currentIEnumerator);
        ///    }
        /// }
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <returns></returns>
        public void LoadFromStreamAsync(string path, string resName)
        {
            if (!isCoroutineRunning)
            {
                isCoroutineRunning = true;
                // 启动协程并保存其引用
                currentIEnumerator = ABLoadFromStreamAsync(path, resName);
                currentCoroutine = StartCoroutine(currentIEnumerator);
                //if (multiCoroutine) { activeCoroutines += currentCoroutine; } //取消多协程
            }
        }

        /// <summary>
        /// 停止协程(不再异步加载资源,无法对同步加载起效).
        /// </summary>
        public void StopCoroutine()
        {
            if (currentCoroutine != null && isCoroutineRunning)
            {
                //有协程实例在运行则停止
                StopCoroutine(currentCoroutine);
                //变量重置
                currentIEnumerator = null;
                isCoroutineRunning = false;
            }
        }

        /// <summary>
        /// 步进协程(异步加载资源),让没完成的协程继续往下跑.
        /// MoveNext()方法一般不是直接调用的,在Unity中协程的推进是由Unity的引擎自动管理的.
        /// 当协程挂起(yield return)等待某个操作完成时,引擎会在适当的时候自动调用MoveNext()来恢复协程的执行,故不需要(也不应该)手动调用MoveNext().
        /// 另若同一特征的协程方法被执行多次(绑定多个协程实例)的话,无法精准操作每个实例个体的步进,会形成批量操作(慎用).
        /// </summary>
        public void MoveNext()
        {
            //协程实例在运行(即使协程方法相同,每次诞生的协程实例并不一致)
            if (currentCoroutine != null && isCoroutineRunning)
            {
                //这里只能找代表协程方法的实例(同一协程方法的该实例仅有1个)去完成下一步
                currentIEnumerator.MoveNext(); //内部检索所有绑定的Coroutine并执行,如绑多个Coroutine的话无法精准操作每个个体只能批量操作(就算取消了本类的多协程方式也无法解决,慎用)
                // 再次检查协程是否完成
                if (currentCoroutine == null)
                {
                    //完成则变量重置
                    currentIEnumerator = null;
                    isCoroutineRunning = false;
                }
            }
        }

        //↑--------------------------------------------------------------------------------------------------------------------↑
        //↑--------------------------------------------Unity_ABTestFuncEnd-----------------------------------------------------↑
        //↑--------------------------------------------------------------------------------------------------------------------↑

        #endregion

        GameObject mainPlayer;
        GameObject mainCamera;
        public float speedA0 = 1.0f; // A0移动速度
        public GameObject prefabA0; // 预制体A0
        public float speedA1 = 1.0f; // A1最大速度
        public float minRadius = 1.0f; // 最小半径
        public float maxRadius = 2.0f; // 最大半径
        public float spawnInterval = 0.5f; // 生成间隔
        public int maxCount = 20; // 最大数量
        public int currentCount; // 当前数量
        private float spawnTimer; // 计时器

        private void Awake()
        {
            //Awake函数主要用于初始化对象的状态,在场景加载时执行.
            //当你禁用组件时,它将不会调用Start、Update、LateUpdate等其他生命周期函数,也不会执行任何渲染操作.
            //若希望在禁用组件时完全停止脚本的执行,可使用StopCoroutine函数来停止协程,或在脚本中手动检查组件是否被禁用,从而避免执行不必要的代码
            //if (enabled){ }

            //开启一个协程进行资源加载,Unity编辑器中Application.dataPath返回Assets文件夹路径,打包后为应用程序所在路径
            //LoadAllFromMemoryAsync(Application.dataPath + "/AssetBundle/abtest");

            //协程结束前尚无法马上取得素材,请等待...

            GameObject tempPrefabA0 = CreatePrefab("prefabA0", 4, 0.5f, "Custom/CShader"); // 内存诞生1个预制体A0对象实例,但Unity会自动把它加载到场景(另存为预制体文件后可进行删除)
            prefabA0 = tempPrefabA0;
            HideObject(tempPrefabA0);
            //↓保存后网格丢失,需要先保存网格文件,解决之前先用隐藏大法.
            //SaveAsPrefab(tempPrefabA0, Application.dataPath + "/Prefabs/A0.prefab");
            //Destroy(tempPrefabA0);
        }

        // Start is called before the first frame update
        void Start()
        {
            #region 测试

            #region 下载网页数据

            //HtmlDocument doc = new();
            //doc.LoadHtml(MMCore.CreateGetHttpResponse("https://ac.qq.com/Comic/ComicInfo/id/542330"));

            ////单元素下载:
            //HtmlNode obj = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div[3]/div[1]/div[1]/div[1]/a/img");
            //string objUal = obj.Attributes["src"].Value;
            //MMCore.Download(objUal, "123.jpg", @"C:\Users\linsh\Desktop\Download\", true);
            //Debug.Log("下载完成！");
            //MMCore.DelDirectory(@"C:\Users\linsh\Desktop\Download\temp");

            //批量下载网页全部对象元素:
            //HtmlNodeCollection objNodes = doc.DocumentNode.SelectNodes("//img");
            //if (objNodes != null)
            //{
            //    foreach (HtmlNode objNode in objNodes)
            //    {
            //        string objUrl = objNode.GetAttributeValue("src", string.Empty);
            //        if (!string.IsNullOrEmpty(objUrl))
            //        {
            //            ////检查对象URL的扩展名是否为.jfif
            //            ////if (objUrl.EndsWith(".jfif", StringComparison.OrdinalIgnoreCase)) { }
            //            //string filePath = Path.Combine(@"C:\Users\linsh\Desktop\Download\", Path.GetFileName(new Uri(objUrl).LocalPath));
            //            //MMCore.Download(objUrl, Path.GetFileName(filePath), @"C:\Users\linsh\Desktop\Download\", true);
            //            //Debug.Log($"下载完成: {filePath}");

            //            // 读取对象扩展名
            //            string objectExtension = Path.GetExtension(objUrl);
            //            // 建立正则表达式参数
            //            //string objectExtensionPattern = @"\.gif$";

            //            string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png|jfif)$";
            //            //(? i):这是一个内联模式修饰符,用于指定接下来的匹配应该是不区分大小写的.
            //            //\.:匹配一个字面的点号(.),因为在正则表达式中点号是一个特殊字符,所以需要用反斜杠进行转义.
            //            //(gif | jpg | jpeg | png):这是一个捕获组,它匹配gif、jpg、jpeg或png中的任何一个.| 是逻辑或操作符,用于分隔不同的选项.
            //            //$:匹配字符串的结尾,确保整个扩展名都匹配并且没有其他字符跟在其后.

            //            // 检查对象扩展名是否匹配.gif, .jpg, .jpeg, .png等(不区分大小写)
            //            if (Regex.IsMatch(objectExtension, objectExtensionPattern))
            //            {
            //                string filePath = Path.Combine(@"C:\Users\linsh\Desktop\Download\", Path.GetFileName(new Uri(objUrl).LocalPath));
            //                MMCore.Download(objUrl, Path.GetFileName(new Uri(objUrl).LocalPath), @"C:\Users\linsh\Desktop\Download\", true);
            //                Debug.Log($"下载完成: {filePath}");
            //                MMCore.DelDirectory(@"C:\Users\linsh\Desktop\Download\temp");
            //            }

            //            //在模式字符串中指定不区分大小写:
            //            //string objectExtensionPattern = @"(?i)\.(gif|jpg|jpeg|png)$";
            //            //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern);
            //            //在Regex.IsMatch方法中指定不区分大小写:
            //            //string objectExtensionPattern = @"\.(gif|jpg|jpeg|png)$";
            //            //bool isValidExtension = Regex.IsMatch(fileExtension, objectExtensionPattern, RegexOptions.IgnoreCase);
            //            //两种方法都会得到相同的结果,但第一种方法更加简洁和直观
            //        }
            //    }
            //}
            //else
            //{
            //    Debug.Log("没有找到对象元素.");
            //}

            #endregion

            //mainPlayer = GameObject.Find("SWPlayer");
            //if (mainPlayer != null)
            //{
            //    Vector3 targetPosition = mainPlayer.transform.position;
            //    Debug.Log("Target Object Position: " + targetPosition.ToString());
            //}
            //else
            //{
            //    Debug.LogError("Could not find target object with name '" + mainPlayer.name + "'");
            //}

            //InvokeRepeating用于特定时间间隔内执行与游戏自动更新频率不同步的情况,如在游戏开始后1秒执行操作来让主要组件充分调度完毕,然后每隔指定秒执行一次
            //InvokeRepeating("InitMap", 1.0f, 0.00625f);//不会新开线程,它是在Unity主线程中间隔执行,且第三个参数在运行过程修改无效(类似GE的周期计时器但有办法重写其调用的内部方法来支持变量)

            //若有主线程创建的实例,在子线程中需使用回调,不然会报错
            //TimerUpdate timerUpdate = new TimerUpdate();
            //timerUpdate.Update += TestFunc;
            //timerUpdate.Duetime = 1000;//前摇等待1s
            //timerUpdate.Period = 500;//500ms执行一次
            //timerUpdate.TriggerStart(true);//后台运行触发

            #endregion

            mainCamera = GameObject.Find("Main Camera");

            //本来是程序化生成后使用的,但程序生成保存的预制体缺失一些网格组件,故作废不读取
            //if (prefabA0 == null)
            //{
            //    prefabA0 = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/A0.prefab");
            //    Debug.Log("Create Prefab A0");
            //}

            #region 搜索本地文件夹内指定名称的预制体

            //// 假设预制体的名字为 "A0"
            //string prefabName = "A0";

            //// 搜索所有资产中的指定名称预制体
            //string[] guids = AssetDatabase.FindAssets(prefabName, new string[] { "Assets" });

            //foreach (string guid in guids)
            //{
            //    // 获取预制体的完整路径
            //    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            //    // 加载预制体
            //    prefabA0 = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            //    if (prefabA0 != null)
            //    {
            //        Debug.Log("Prefab A0 found and loaded successfully.");
            //        Debug.Log("PrefabPath: " + assetPath); //路径是Assets/Prefabs/A0.prefab
            //        break; // 若找到了就停止搜索
            //    }
            //}

            #endregion

            // 初始化计数器和计时器
            currentCount = 0;
            spawnTimer = 0f;

            // 检查预制体是否加载成功
            if (prefabA0 == null)
            {
                Debug.LogError("Prefab A0 not found!");
            }
            else
            {
                // 创建1个预制体A0的实例个体,作为mainPlayer被玩家操作
                mainPlayer = Instantiate(prefabA0, transform.position, Quaternion.identity);
                ShowObject(mainPlayer);
                // 实例名字为默认,这里指定新名字
                mainPlayer.name = "A0";
                // 尺寸归一化
                mainPlayer.transform.localScale = Vector3.one;

                //不希望被推动的地面、团块(空间容器)不需要设置刚体只需设置网格碰撞器,有3D刚体和碰撞的对象自然会踩在上面

                //// 添加2D刚体(Unity内置的2D物理系统)
                //mainPlayer.AddComponent<Rigidbody2D>();
                //// 设置刚体的重力规模为0,使其不受重力影响
                //mainPlayer.GetComponent<Rigidbody2D>().gravityScale = 0f;

                //// 添加 Circle Collider 2D 组件(或meshCollider)
                //mainPlayer.AddComponent<CircleCollider2D>();

                //// 设置碰撞体的半径范围(受缩放影响)
                //mainPlayer.GetComponent<CircleCollider2D>().radius = 0.5f;

                //// 设置碰撞体相对物体中心位置
                //mainPlayer.GetComponent<CircleCollider2D>().offset = Vector2.zero;

                //// 设置碰撞事件(可选),true是默认的
                //mainPlayer.GetComponent<CircleCollider2D>().enabled = true;
                //// 启用触发器事件(不使用物理系统)
                //mainPlayer.GetComponent<CircleCollider2D>().isTrigger = true;

                //// 给游戏物体挂上自定义脚本(执行推开动作,防止大小球重叠在一起)
                //mainPlayer.AddComponent<PreventOverlap>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (mainPlayer != null)
            {
                // 更新生成计时器
                spawnTimer += Time.deltaTime;

                //Debug.Log(spawnTimer.ToString());

                // 检查是否达到生成A1的时间
                if (spawnTimer >= spawnInterval && currentCount < maxCount)
                {
                    // 生成A1
                    SpawnA1(prefabA0);
                    // 重置计时器
                    spawnTimer = 0f;
                }

                // 更新现有A1的位置
                UpdateA1Positions();

                // 检查WASD按键并移动Player
                if (Input.GetKey(KeyCode.W))
                {
                    mainPlayer.transform.Translate(Vector3.up * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(
                        mainPlayer.transform.position.x,
                        mainPlayer.transform.position.y,
                        mainCamera.transform.position.z
                    );
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    mainPlayer.transform.Translate(Vector3.down * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(
                        mainPlayer.transform.position.x,
                        mainPlayer.transform.position.y,
                        mainCamera.transform.position.z
                    );
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    mainPlayer.transform.Translate(Vector3.left * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(
                        mainPlayer.transform.position.x,
                        mainPlayer.transform.position.y,
                        mainCamera.transform.position.z
                    );
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    mainPlayer.transform.Translate(Vector3.right * speedA0 * Time.deltaTime);
                    mainCamera.transform.position = new Vector3(
                        mainPlayer.transform.position.x,
                        mainPlayer.transform.position.y,
                        mainCamera.transform.position.z
                    );
                }
            }
        }

        /// <summary>
        /// 当物体进入触发器时调用.
        /// </summary>
        /// <param name="triggerObject"></param>
        //void OnTriggerEnter2D(Collider2D triggerObject) { }

        /// <summary>
        /// 当物体持续留在触发器内时,每帧调用一次.
        /// </summary>
        /// <param name="triggerObject"></param>
        //void OnTriggerStay2D(Collider2D triggerObject) { }

        /// <summary>
        /// 当物体离开触发器时调用.
        /// </summary>
        /// <param name="triggerObject"></param>
        //void OnTriggerExit2D(Collider2D triggerObject) { }

        /// <summary>
        /// 用预制体中创建A1
        /// </summary>
        void SpawnA1(GameObject prefab)
        {
            // 在Player的位置生成A1
            GameObject a1 = Instantiate(prefab, mainPlayer.transform.position, Quaternion.identity);
            ShowObject(a1);
            // 为新生成的A1设置唯一名称
            a1.name = "A1_" + currentCount;
            // 尺寸修改
            a1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            // 增加计数器
            currentCount++;

            //↓测试3D自定义纸片网格碰撞情况
            //添加3D刚体(Unity内置的3D物理系统)
            //a1.AddComponent<Rigidbody>().isKinematic = false;//关闭自定义运动学,启用内置的
            ////禁用3D刚体重力
            //a1.GetComponent<Rigidbody>().useGravity = false;
            //a1.GetComponent<Rigidbody>().mass = 0f;//质量=0
            //a1.GetComponent<Rigidbody>().AddForce(Vector3.zero);//各向推力
            //Unity3D刚体质量和推力为0还是会被撞出去,若需要碰到别的物体后自己不动,就写自定义事件吧
            //a1.GetComponent<Rigidbody>().drag = 10000f;//增大线性阻力(减缓直线运动)
            //a1.GetComponent<Rigidbody>().angularDrag = 10000f;//增大角阻力(减缓旋转)
            //a1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;//禁止物理引擎产生的各方向移动(冻结运动,但不能防止A进入B)

            //a1.AddComponent<BoxCollider>().enabled = true;
            //a1.GetComponent<BoxCollider>().size = new Vector3(0.5f, 0.5f, 0.5f); //没有半径属性,碰撞器大小填三维向量
            //a1.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, 0f);//中心偏移量为0(碰撞体的中心与游戏对象的原点重合)

            //添加meshCollider(3D碰撞器)的碰撞网格
            a1.AddComponent<MeshCollider>().sharedMesh = a1.GetComponent<MeshFilter>().mesh; //可换成BoxCollider等预制碰撞体组件进行测试
            a1.GetComponent<MeshCollider>().convex = true; //告诉它现在是凸面(有时候Unity判断不准)

            //MeshCollider没有碰撞半径、中心属性但可通过调整Mesh本身来改变碰撞体的形状和大小(通常碰撞体中心会自动与物体的中心对齐除非有特殊需求要调整)
            //“中心”是BoxCollider等预制碰撞体类型所具有的属性(以便用户后续调整)
            //注意:3D平面网格若是平面非凸的,只能勾选IsKinematic后自己写物理轨迹,因为3D物理系统只对凸面起作用,Unity5取消了对非凸面的支持(否则导致3D物理引擎不稳定)
            //结论:平面2D的碰撞面果然要配合2D物理引擎,不过渲染组件依然可以3D
            //MC项目已挂了3D物理引擎所以碰撞面不能在Unity5之后版本里使用2D碰撞体,要强行使用就得撤掉团块的3D物理引擎,我得魔改下插件(追加一个2D/3D物理引擎切换)
            // 要解决这个问题,还有几个选项:
            // ‌1、使Rigidbody成为运动学的‌:
            // 将Rigidbody的IsKinematic属性设置为true.这意味着该物体将不会受到物理引擎的作用力(如重力、碰撞冲击等),但你仍然可以通过脚本或动画来控制它的移动.
            // ‌2、移除Rigidbody组件‌:
            // 若你的物体不需要进行物理模拟(例如,它只是一个静态的障碍物),你可以完全移除Rigidbody组件.
            // 3、使用凸形Mesh‌:
            // 若你的Mesh不是凸形的,你可以尝试修改它使其成为凸形.凸形Mesh是指所有顶点都位于同一个半空间内的Mesh,这样的Mesh在进行碰撞检测时更加高效和稳定.
            // 你可以使用3D建模软件来修改Mesh的形状,或者使用Unity的ProBuilder等工具来创建一个凸形的Mesh.
            // ‌4、使用其他碰撞体类型‌:
            // 若修改Mesh的形状不可行,你可以考虑使用其他类型的碰撞体(如BoxCollider、SphereCollider或CapsuleCollider)来近似表示你的物体.这些碰撞体类型通常都是凸形的,并且更容易与物理引擎兼容.
            // 5、分割Mesh‌:
            // 若你的Mesh非常复杂且不能简单地修改为凸形,你可以考虑将其分割成多个较小的、凸形的部分,并为每个部分分别添加MeshCollider和(若需要的话)Rigidbody组件.

            //启用碰撞事件(默认情况下就是启用的)
            //a1.GetComponent<MeshCollider>().enabled = true; //多余的(AddComponent时已启用组件)
            //设置isTrigger为false表示这不是一个触发器碰撞体
            //a1.GetComponent<MeshCollider>().isTrigger = false;

            //↓3D网格碰撞器的碰撞面mesh可独立设置,注意即使对象用了3D网格渲染器+网格过滤器,代码仍可添加2D碰撞器组件(即便Unity发出警告与3D渲染器冲突,不过建议还是统一)

            //// 添加刚体(Unity内置的物理系统)
            //a1.AddComponent<Rigidbody2D>();
            //// 设置刚体的重力规模为0,使其不受重力影响
            //a1.GetComponent<Rigidbody2D>().gravityScale = 0f;

            //// 添加 Circle Collider 2D 组件(或meshCollider)
            //a1.AddComponent<CircleCollider2D>();

            //// 设置碰撞体的半径范围(受缩放影响)
            //a1.GetComponent<CircleCollider2D>().radius = 0.5f;

            //// 设置碰撞体相对物体中心位置
            //a1.GetComponent<CircleCollider2D>().offset = Vector2.zero;

            //// 设置碰撞事件(可选),true是默认的
            //a1.GetComponent<CircleCollider2D>().enabled = true;
            //// 启用触发器事件(不使用物理系统)
            ////a1.GetComponent<CircleCollider2D>().isTrigger = true;
        }

        /// <summary>
        /// 获取指定点周围X-Y平面上1.0到3.0范围内的一个随机向量坐标点.
        /// </summary>
        /// <param name="center">中心点.</param>
        /// <returns>周围的随机向量坐标点.</returns>
        public static Vector3 GetRandomVectorAroundPoint(Vector3 point, float min, float max)
        {
            // 随机生成一个角度(0到2π之间)
            float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

            // 随机生成一个距离(min到max之间)
            float distance = UnityEngine.Random.Range(min, max);

            // 使用极坐标转换为笛卡尔坐标
            Vector3 randomVector = new Vector3(
                point.x + distance * Mathf.Cos(angle),
                point.y + distance * Mathf.Sin(angle),
                point.z
            );

            return randomVector;
        }

        /// <summary>
        /// 更新所有A1位置
        /// </summary>
        void UpdateA1Positions()
        {
            // 更新所有A1的位置
            for (int i = 0; i < currentCount; i++)
            {
                GameObject a1 = GameObject.Find("A1_" + i);
                if (a1 != null)
                {
                    // 计算相对位置偏移量
                    Vector3 localOffset = mainPlayer.transform.position - a1.transform.position;

                    // 若mainPlayer移动,A1会持续找过去
                    // 但是要限制A1的移动范围
                    Vector3 newA1Position =
                        a1.transform.position + localOffset * speedA1 * Time.deltaTime;

                    if (mainPlayer != null)
                    {
                        // 计算A1和A0之间的距离
                        float distanceToA0 = (
                            newA1Position - mainPlayer.transform.position
                        ).magnitude;

                        // 检查A1是否超出了允许的轨道范围
                        if (distanceToA0 > maxRadius)
                        {
                            // 若超出了,将A1的位置限制在轨道范围内
                            newA1Position =
                                mainPlayer.transform.position
                                + (newA1Position - mainPlayer.transform.position).normalized
                                    * maxRadius;
                        }
                        else if (distanceToA0 == 0f)
                        {
                            // 若在A0原点
                            newA1Position = GetRandomVectorAroundPoint(
                                mainPlayer.transform.position,
                                minRadius,
                                maxRadius
                            );
                        }
                        else if (distanceToA0 < minRadius)
                        {
                            // 若进入了A0内部,将A1的位置限制在A0的边缘
                            newA1Position =
                                mainPlayer.transform.position
                                + (newA1Position - mainPlayer.transform.position).normalized
                                    * minRadius;
                        }
                    }
                    else
                    {
                        Debug.LogError("A0 not found in the project.");
                    }

                    // 更新A1的位置
                    //a1.transform.position = newA1Position;
                    a1.transform.Translate(
                        (newA1Position - a1.transform.position)
                            * speedA1
                            * (localOffset.magnitude + 0.5f)
                            * Time.deltaTime
                    );
                }
                else
                {
                    Debug.LogError("A1_" + i + " not found in the project.");
                }
            }
        }

        /// <summary>
        /// 绘制圆形网格,仅背面渲染(从Z低处往上看得到)
        /// </summary>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        Mesh CreateCircleMeshBack(int segments, float radius)
        {
            Mesh mesh = new Mesh();

            // 为这个数组声明segments+1个元素
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            }

            // 生成反面三角形索引
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = (i + 1) % segments + 1;
                triangles[i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// 翘角网格
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Mesh CreateCircleMeshBack(int segments, float radius, float height)
        {
            //碰撞体问题:
            //1)横版模式下撤掉3D刚体,挂2D刚体,渲染仍用3D组件；
            //2)退回Unity5之前(该版本允许3D非凸面碰撞)；
            //3)坚持不改,空气墙碰撞；
            //4)欺骗Unity,偷偷给3D纸片网格往Z轴凸一角；
            //5)开启刚体运动学(屏蔽物理引擎表现),改为自定义(仍可利用物理引擎的边界碰撞事件)；
            //6)静态地面允许移除刚体但仍作为3D碰撞面,雷班纳那边要改为凸面网格；
            //7)强行使用立体网格(跟4一样只是可以使用官方预制的立方体块)；
            //8)既然4和7可行,干脆雷班纳和怪物们的独立碰撞网格做成带凸面或翘个角,MC系统改都不用改,方案4节省点三角面
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero; // 中心点保持不变

            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                // 非中心点的顶点在Z轴上添加一个凸起
                vertices[i] = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    height
                );
            }

            // 生成三角形索引,与原始平面网格相同
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = (i + 1) % segments + 1;
                triangles[i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); // 重新计算法线以确保光照正确

            return mesh;
        }

        /// <summary>
        /// 绘制圆形网格,仅正面渲染(从Z高处往下看得到)
        /// </summary>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        Mesh CreateCircleMeshFront(int segments, float radius)
        {
            Mesh mesh = new Mesh();

            //为这个数组声明segments+1个元素
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            }

            // 生成正面三角形索引
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// 绘制圆形网格,支持双面渲染
        /// </summary>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        Mesh CreateCircleMeshDouble(int segments, float radius)
        {
            Mesh mesh = new Mesh();

            // 为这个数组声明segments+1个元素
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 6]; // 双面渲染需要两倍的三角形索引

            vertices[0] = Vector3.zero;
            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            }

            // 生成正面三角形索引
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % segments + 1;
            }

            // 生成反面三角形索引
            for (int i = 0; i < segments; i++)
            {
                triangles[segments * 3 + i * 3] = 0;
                triangles[segments * 3 + i * 3 + 1] = (i + 1) % segments + 1;
                triangles[segments * 3 + i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// 创建一个平面圆形游戏物体(默认仅背面渲染,摄像机从Z低处向上看得到),会立马出现在场景,可手动隐藏或保存为预制体(素材文件)后清除它.
        /// </summary>
        /// <param name="name">预制体名称</param>
        /// <param name="segments">分段数决定圆的平滑程度,越少圆看起来就越像是多边形,越多圆就越接近于真正的圆形,少于4个会看不到圆而是正方形,2~3个则是三角,1个是条线</param>
        /// <param name="radius">半径</param>
        /// <param name="shaderName">Shader名称</param>
        /// <returns></returns>
        GameObject CreatePrefab(string name, int segments, float radius, string shaderName)
        {
            GameObject prefab = new GameObject(name);
            MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateCircleMeshBack(segments, radius, 1.0f);
            MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
            //纹理数据会被加载并传递给Shader,但由于Shader被固定为输出红色,后面即使meshRenderer.material.mainTexture=各种纹理都不会影响
            meshRenderer.material = new Material(Shader.Find(shaderName));

            //设置材质属性为双面渲染
            //meshRenderer.material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            //meshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            //meshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            //meshRenderer.material.SetInt("_ZWrite", 1);

            return prefab;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 将游戏物体保存为预制体(素材文件)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        void SaveAsPrefab(GameObject obj, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            //↓销毁内存中的GameObject,因为它已经被保存为预制体了,不需要这个GameObject在内存中占用空间
            //Destroy(obj);
        }
#endif

        /// <summary>
        /// 隐藏物体
        /// </summary>
        /// <param name="obj"></param>
        void HideObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        /// <summary>
        /// 显示物体
        /// </summary>
        /// <param name="obj"></param>
        void ShowObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        /// <summary>
        /// 使用 DontDestroyOnLoad() 防止物体在场景切换时被销毁
        /// </summary>
        /// <param name="obj"></param>
        void KeepObjectInMemory(GameObject obj)
        {
            DontDestroyOnLoad(obj);
        }

        /// <summary>
        /// 使用 SceneManager.MoveGameObjectToScene() 方法将一个 GameObject 从一个场景移动到另一个场景.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetScene"></param>
        void MoveObjectToScene(GameObject obj, UnityEngine.SceneManagement.Scene targetScene)
        {
            //UnityEngine.SceneManagement.Scene以防有些类名起名Scene的犯冲,所以第二个参数给加上完整地址
            SceneManager.MoveGameObjectToScene(obj, targetScene);
        }

        /// <summary>
        /// 创建第一个AB包中的素材对象(仅演示勿使用),并拼装到主物体上
        /// </summary>
        void CreateFirstABUnit()
        {
            GameObject odinMech = null;
            //实例化资源中第一个游戏物体(前提是资源已经在协程里加载完毕,这里要进行检查)
            if (!isCoroutineRunning)
            {
                Debug.Log("gameObjectGroup.Length => " + gameObjectGroup.Length.ToString());
                for (int i = 0; i < gameObjectGroup.Length; i++)
                {
                    Debug.Log("读取AB包中第" + i.ToString() + "个元素成功！");
                    Debug.Log("GameObject " + i + " Name: " + gameObjectGroup[i].name);
                }
                //gameObjectGroup[0]是奥丁,gameObjectGroup[1]是跳虫,假设AB包(abtest)内这只有2个预制体.
                odinMech = Instantiate(
                    gameObjectGroup[0],
                    mainPlayer.transform.position,
                    Quaternion.identity
                );

                //删除预制体内的刚体,防止子物体参与物理系统,让子模型完全随主体行动
                //Rigidbody odinMechRigidbody = odinMech.GetComponent<Rigidbody>();
                //Destroy(odinMechRigidbody);

                //子游戏物体的旋转和位置与要衔接的主体保持一致
                odinMech.transform.position = mainPlayer.transform.position;
                odinMech.transform.rotation = mainPlayer.transform.rotation;

                // 将odinMech设置为mainPlayer的子对象(直接拼装,比每帧修正更省事)
                odinMech.transform.parent = mainPlayer.transform;

                #region 动画测试

                //Animation odinMechAnimation = odinMech.GetComponent<Animation>();
                //if (odinMechAnimation == null)
                //{
                //    Debug.LogError("The target GameObject does not have an Animation component.");
                //    return;
                //}
                //AnimationClip[] animationClips = odinMechAnimation.GetComponents<AnimationClip>();
                //foreach (AnimationClip clip in animationClips)
                //{
                //    Debug.Log("Animation Name: " + clip.name);
                //}
                //odinMech.GetComponent<Animator>().SetTrigger("Walk");
                //new WaitForSecondsRealtime(3.0f);
                //odinMech.GetComponent<Animator>().SetTrigger("Attack");
                //new WaitForSecondsRealtime(3.0f);
                //odinMech.GetComponent<Animator>().SetTrigger("Dead");
                //Animator animator = odinMech.GetComponent<Animator>();
                //if (animator != null)
                //{
                //    Debug.Log("Animator is attached to the GameObject.");
                //    AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
                //    if (animatorController != null)
                //    {
                //        Debug.Log("AnimatorController is assigned.");
                //        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
                //        foreach (AnimatorState animatorState in stateMachine.states)
                //        {
                //            if (animatorState.motion != null)
                //            {
                //                Debug.Log(animatorState.motion.name); // 输出Animation Clip的名称
                //            }
                //        }
                //    }
                //    else
                //    {
                //        Debug.LogError("AnimatorController is not assigned.");
                //    }
                //}
                //else
                //{
                //    Debug.LogError("Animator is not attached to the GameObject.");
                //}

                #endregion
            }
            else
            {
                Debug.Log("协程未完成！依然读取AB包中...");
            }

            if (
                odinMech != null
                && (
                    Input.GetKey(KeyCode.W)
                    || Input.GetKey(KeyCode.A)
                    || Input.GetKey(KeyCode.S)
                    || Input.GetKey(KeyCode.D)
                )
            )
            {
                odinMech.GetComponent<Animation>().Play("Walk");
            }
        }
    }
}

// 在Unity的物理引擎中,刚体(Rigidbody)的运动学属性(Kinematic)和碰撞器(Collider)的isTrigger属性是两个不同的概念,它们分别用于控制物体的不同行为.
// ‌刚体的运动学属性(Is Kinematic)‌:
// 当一个刚体的Is Kinematic属性被设置为true时,该刚体将不再受物理引擎的力(如重力、摩擦力等)和碰撞冲量的影响.这意味着,即使有其他物体与它碰撞,它也不会因为碰撞而产生移动或旋转.
// 运动学刚体仍然可以参与碰撞检测,因此可以用于触发碰撞事件,但它们不会因碰撞而改变位置或速度.
// 运动学刚体常用于需要精确控制移动和旋转的物体,如角色控制、摄像机移动等.
// ‌碰撞器的isTrigger属性‌:
// 官方物理引擎还是好用的,比如做个透明网格还可以用来拾取物品(碰撞器组件事件检测到进/出/待着的事件时,让被碰物体吸过来)
// 碰撞器(Collider)的isTrigger属性用于确定该碰撞器是作为一个触发器还是作为一个实际的物理碰撞体.
// 当isTrigger被设置为true时,碰撞器将不再作为物理碰撞体,而是作为一个触发器.这意味着,当有其他物体进入、停留或离开该触发器的范围时,会触发相应的碰撞事件(如OnTriggerEnter、OnTriggerStay、OnTriggerExit),但不会产生物理碰撞效果(即不会使物体移动或旋转).
// 触发器常用于检测物体的进入、离开或交互,如门开关、触发机关、检测玩家是否进入某个区域等.

//‌静态碰撞体(Static Collider)‌:
//静态碰撞体不会受到物理力的影响,也不会被其他物体推动
//在游戏对象上添加一个碰撞体组件(如BoxCollider、MeshCollider等),但不用添加Rigidbody组件
//它成为一个静态障碍物,其他物体可以在其上碰撞但不会推动它,并且静态碰撞体的网格会与地面网格合并.
