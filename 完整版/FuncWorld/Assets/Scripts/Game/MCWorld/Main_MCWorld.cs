using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace MCWorld
{
    public class Main_MCWorld : MonoBehaviour
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
        public void ABLoadFromFile(string path, string resName, out AssetBundle assetBundle, out GameObject gameObject)
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
        public void ABLoadFromMemory(string path, string resName, out AssetBundle assetBundle, out GameObject gameObject)
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
        public void ABLoadFromStream(string path, string resName, out AssetBundle assetBundle, out GameObject gameObject)
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
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
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
        /// GameObject.Instantiate(gameObjectGroup[0], Main_MCWorld.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
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
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
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
        /// GameObject.Instantiate(gameObjectGroup[0], Main_MCWorld.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程(需要一个循环体让协程继续往下跑).
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
        GameObject odinMech;

        private void Awake()
        {
            //Awake函数主要用于初始化对象的状态,在场景加载时执行.
            //当你禁用组件时,它将不会调用Start、Update、LateUpdate等其他生命周期函数,也不会执行任何渲染操作.
            //若希望在禁用组件时完全停止脚本的执行,可使用StopCoroutine函数来停止协程,或在脚本中手动检查组件是否被禁用,从而避免执行不必要的代码
            if (enabled)
            {
                Debug.Log(Application.dataPath);

                //开启一个协程进行资源加载
                LoadAllFromMemoryAsync(Application.dataPath + "/AssetBundle/abtest");
                //协程结束前尚无法马上取得素材,请等待

                //Unity编辑器中Application.dataPath返回Assets文件夹路径,打包后为应用程序所在路径
                //LoadAllFromMemoryAsync(Application.dataPath + "/AssetBundle/abtest");
            }
        }

        // Use this for initialization
        private void Start()
        {
            mainPlayer = GameObject.Find("Player");
            if (mainPlayer != null)
            {
                Vector3 targetPosition = mainPlayer.transform.position;
                Debug.Log("Target Object Position: " + targetPosition.ToString());
            }
            else
            {
                Debug.LogError("Could not find target object with name '" + mainPlayer.name + "'");
            }

            //摧毁诞生平台
            GameObject platform = GameObject.Find("PlatformForBirth");
            if (platform)
            {
                Destroy(platform, 10f); // 定时摧毁游戏对象
            }
        }

        // Update is called once per frame
        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("按下了Q键,执行功能测试");

                //实例化资源中第一个游戏物体(前提是资源已经在协程里加载完毕,这里要进行检查)
                if (!isCoroutineRunning)
                {
                    Debug.Log("gameObjectGroup.Length => " + gameObjectGroup.Length.ToString());
                    for (int i = 0; i < gameObjectGroup.Length; i++)
                    {
                        Debug.Log("读取AB包中第" + i.ToString() + "个元素成功！");
                        Debug.Log("GameObject " + i + " Name: " + gameObjectGroup[i].name);
                    }
                    //gameObjectGroup[0]是奥丁,gameObjectGroup[1]是跳虫,目前AB包(abtest)内这只有2个预制体.
                    odinMech = Instantiate(gameObjectGroup[0], mainPlayer.transform.position, Quaternion.identity);

                    //删除预制体内的刚体,防止子物体参与物理引擎,让子模型完全按主体的Transform行动
                    //Rigidbody odinMechRigidbody = odinMech.GetComponent<Rigidbody>();
                    //Destroy(odinMechRigidbody);

                    //子游戏物体的旋转和位置与要衔接的主体保持一致
                    odinMech.transform.position = mainPlayer.transform.position;
                    odinMech.transform.rotation = mainPlayer.transform.rotation;

                    //local是相对
                    //odinMech.transform.localPosition = mainPlayer.transform.position;
                    //odinMech.transform.localRotation = mainPlayer.transform.rotation;

                    // 将odinMech设置为mainPlayer的子对象(直接拼装了,比下方的每帧修正更省事)
                    odinMech.transform.parent = mainPlayer.transform;

                    //动画测试
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

                    //odinMech.transform.localPosition = mainPlayer.transform.position;
                    //odinMech.transform.localRotation = mainPlayer.transform.rotation;
                    //odinMech.transform.localScale = mainPlayer.transform.localScale;
                }
                else { Debug.Log("协程未完成！依然读取AB包中..."); }
            }

            if (odinMech != null && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
            {
                odinMech.GetComponent<Animation>().Play("Walk");
            }
        }
    }
}