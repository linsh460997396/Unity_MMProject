#if UNITY_EDITOR|| UNITY_STANDALONE
using System.IO;
using System.Collections;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 资源方法类，提供AssetBundle管理方案。当使用异步加载时，该类只能有1个正在加载资源的协程，届时用Resource.isCoroutineRunning来检查是否可以开启新协程。
    /// </summary>
    public class Resource : MonoBehaviour
    {
        #region 这些代码是用来加载AB包的具体方法（如果协程调用失败，请复制到主线程入口所在类使用）

        //↓--------------------------------------------------------------------------------------------------------------------↓
        //↓--------------------------------------------Unity_ABTestFuncStart---------------------------------------------------↓
        //↓--------------------------------------------------------------------------------------------------------------------↓
        //注：当这些函数在Unity中使用正常，素材也实例化成功，但进行BepInEx制作MOD时报错：①读取文件报header问题；②读取字节则内存数据无法成功解压
        //上述故障原因跟Unity版本有关，建议用游戏同版本Unity去转AB包素材

        //ABTest_CustomGlobalValues，这些全局字段在下面函数里接收获取到的素材
        public GameObject[] gameObjectGroup;
        public AssetBundle assetBundle;
        public IEnumerator currentIEnumerator;
        public Coroutine currentCoroutine;
        public bool isCoroutineRunning = false;
        /// <summary>
        /// 保存所有活动协程的列表，需要Resource.multiCoroutine = true时可用
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
        /// 同步加载: 以文件的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。
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
        /// 同步加载: 以byte[] 形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。
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
        /// 同步加载: 以流的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。
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
        /// 异步加载: 以文件的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载: 以byte[] 形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载AB包中全资源（存储在Resource.gameObjectGroup、Resource.assetBundle）：
        /// 以byte[] 形式加载整个AssetBundle到内存中，而不是从流中加载它，这可能更高效，用于AssetBundle较小或需快速访问AssetBundle中的所有资产。
        /// 但AssetBundle很大则可能会导致内存使用增加，该方法结尾使用StopCoroutine动作在完成加载后停止协程防止方法完成前、被多次调用时继续运行不必要的加载。
        /// 在主角位置实例化示范：
        /// GameObject.Instantiate(gameObjectGroup[0], GameMain.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
            Debug.Log("Prinny: 协程处理完成，素材已加载！");
            assetBundle = request.assetBundle;
            gameObjectGroup = assetBundle.LoadAllAssets<GameObject>();
            Debug.Log("Prinny: 素材已赋值给实例字段！");
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
            Debug.Log("Prinny: 协程已关闭！");
        }

        /// <summary>
        /// 异步加载: 以流的形式加载AssetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
            // AssetBundleCreateRequest 类是 AssetBundle 的一个实例，它表示一个异步加载请求。AssetBundle.LoadFromStreamAsync 方法使用指定路径的文件流来创建一个新的 AssetBundleCreateRequest 实例。
            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(File.OpenRead(path));
            // yield return 语句用于将控制权交还调用上下文，直到异步加载请求完成。一旦请求完成，该方法将使用 LoadAsset 方法从 AssetBundle 中加载指定名称的游戏对象。
            // 加载游戏对象后，该方法将通过 yield return 语句返回对 obj 变量的引用，这使得调用代码可以使用该对象。
            // 总的来说，此方法可用于在游戏或应用程序中异步加载 AssetBundle，并检索其中的游戏对象。这可以在需要时帮助优化内存使用和加载时间。
            yield return request;
            assetBundle = request.assetBundle;
            gameObjectGroup[0] = request.assetBundle.LoadAsset<GameObject>(resName);
            //在Unity中，协程（Coroutine）会自动管理其生命周期，这意味着一旦协程中的IEnumerator方法执行完毕，协程就会自行结束，不需要显式地调用Stop方法。
            isCoroutineRunning = false;
        }

        /// <summary>
        /// 异步加载: 以文件的形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
        /// 异步加载: 以byte[] 形式加载AssetBundle，并存储在Resource.gameObjectGroup[0]、Resource.assetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
        /// 异步加载AB包中全资源（存储在Resource.gameObjectGroup、Resource.assetBundle）：
        /// 以byte[] 形式加载整个AssetBundle到内存中，而不是从流中加载它，这可能更高效，用于AssetBundle较小或需快速访问AssetBundle中的所有资产。
        /// 但AssetBundle很大则可能会导致内存使用增加，该方法结尾使用StopCoroutine动作在完成加载后停止协程防止方法完成前、被多次调用时继续运行不必要的加载。
        /// 在主角位置实例化示范：
        /// GameObject.Instantiate(gameObjectGroup[0], GameMain.mainPlayer.position, Quaternion.identity);要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
        /// 异步加载: 以流的形式加载AssetBundle。要注意所有的异步加载都会开启协程（需要一个循环体让协程继续往下跑）。
        /// 让没完成的协程继续往下跑：
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
        /// 停止协程（不再异步加载资源，无法对同步加载起效）。
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
        /// 步进协程（异步加载资源），让没完成的协程继续往下跑。
        /// MoveNext()方法一般不是直接调用的，在Unity中协程的推进是由Unity的引擎自动管理的。
        /// 当协程挂起（yield return）等待某个操作完成时，引擎会在适当的时候自动调用MoveNext()来恢复协程的执行，故不需要（也不应该）手动调用MoveNext()。
        /// 另如果同一特征的协程方法被执行多次（绑定多个协程实例）的话，无法精准操作每个实例个体的步进，会形成批量操作（慎用）。
        /// </summary>
        public void MoveNext()
        {
            //协程实例在运行（即使协程方法相同，每次诞生的协程实例并不一致）
            if (currentCoroutine != null && isCoroutineRunning)
            {
                //这里只能找代表协程方法的实例（同一协程方法的该实例仅有1个）去完成下一步
                currentIEnumerator.MoveNext(); //内部检索所有绑定的Coroutine并执行，如绑多个Coroutine的话无法精准操作每个个体只能批量操作（就算取消了本类的多协程方式也无法解决，慎用）
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
    }
}
#endif

// 在Unity中，Coroutine 和 IEnumerator 是两个不同的概念，但它们紧密相关，通常一起使用来实现协程的功能。
// 理解它们的区别和用途对于正确使用协程是非常重要。

// IEnumerator
// IEnumerator 是一个接口，定义在C#的 System.Collections 命名空间中。
// 它通常用于枚举集合的元素，但在Unity的上下文中，IEnumerator 被用作协程的基础。
// 一个实现了 IEnumerator 的方法必须包含 Current 属性和 MoveNext() 方法。
// 在Unity的协程中，Current 属性通常不被使用，而 MoveNext() 方法则用于控制协程的执行流程。
// 当你声明一个协程时，你实际上是在创建一个返回类型为 IEnumerator 的方法。
// 例如：
// private IEnumerator MyCoroutine()
// {
//     // 协程逻辑
//     yield return null; // 暂停协程
// }

// Coroutine
// 在Unity中，Coroutine 是一个类，它代表了一个正在运行的协程实例。
// 你不能直接创建一个 Coroutine 实例；相反，你通过调用 MonoBehaviour 的 StartCoroutine 方法来启动一个协程，该方法接受一个返回类型为 IEnumerator 的方法作为参数。
// StartCoroutine 方法返回一个 Coroutine 对象，该对象代表了正在运行的协程，你可以用它来停止协程（通过调用 StopCoroutine 方法）。
// 例如：
// private void Start()
// {
//     // 启动协程，并获取返回的Coroutine对象
//     Coroutine myCoroutine = StartCoroutine(MyCoroutine());

//     // 在某个时刻，你可能想要停止这个协程
//     StopCoroutine(myCoroutine);
// }

// private IEnumerator MyCoroutine()
// {
//     // 协程逻辑
//     yield return null; // 暂停协程
// }

// 为什么需要Coroutine类
// Coroutine 类是Unity协程系统的核心部分，它负责管理协程的生命周期和执行流程。
// Unity在每一帧都会调用协程的 MoveNext 方法来检查是否应该继续执行协程，或者是否应该等待某个条件（例如，等待一段时间或等待某个事件发生）。
// 通过将协程逻辑封装在实现了 IEnumerator 接口的方法中，并通过 Coroutine 类来管理这些协程，
// Unity提供了一种灵活且强大的机制，允许开发者在游戏的运行过程中以非阻塞的方式执行复杂的逻辑，如延迟操作、等待用户输入或异步加载资源等。
// 简而言之，IEnumerator 是协程的基础接口，定义了协程的执行流程，而 Coroutine 类则是Unity用来管理协程实例的生命周期和执行状态的类。
// 两者结合使用，使得开发者能够方便地在Unity游戏中实现异步操作和复杂逻辑控制。

//备注：当协程所在类提供程序入口方法并启动的情况下，不用实例化自己直接调用本类方法执行协程，不需要手动步进协程发现资源已经加载完了（大概是等同静态方式调用模板方法，内部把结果跑完了）。

//MoveNext()方法一般不是直接调用的。在Unity中，协程的推进是由Unity的引擎自动管理的。
//当协程挂起（yield return）等待某个操作完成时，引擎会在适当的时候自动调用MoveNext()来恢复协程的执行，故不需要（也不应该）手动调用MoveNext()。

// 在Unity中，每次调用StartCoroutine方法都会创建一个新的协程实例，即使你传递的是同一个IEnumerator方法的引用。
// 这意味着，如果你有多次调用StartCoroutine(ABLoadFromStreamAsync(path, resName))，每次都会启动一个新的协程，并且每个协程都会有自己的执行上下文和状态。

// MoveNext是IEnumerator接口的一个方法，当你有一个实现了该接口的实例（比如一个List<T>.Enumerator或者一个自定义的协程IEnumerator）时，你可以调用它来推进枚举器的状态。
// 实现了接口的类的实例虽然可以调用接口方法，但currentIEnumerator.MoveNext()这样的调用通常是不正确的，因为IEnumerator实例本身并没有MoveNext方法。
// 对Unity的协程来说，MoveNext的调用是由Unity引擎内部管理的，并且是在协程挂起（yield语句）之后由引擎自动处理的，用户不需要（也不应该）直接调用MoveNext来推进协程的执行。

// 如果你尝试多次调用StartCoroutine同一个协程方法，并且想要控制这些协程的执行，应该为每个协程保存一个独立的Coroutine实例。
// 例如：
// public class MyMonoBehaviour : MonoBehaviour
// {
//     private List<Coroutine> activeCoroutines = new List<Coroutine>(); // 保存所有活动协程的列表

//     // 启动协程的方法
//     public void StartLoadingAssetBundle(string path, string resName)
//     {
//         Coroutine newCoroutine = StartCoroutine(ABLoadFromStreamAsync(path, resName));
//         activeCoroutines.Add(newCoroutine); // 将新协程添加到活动列表
//     }

//     // 停止所有协程的方法
//     public void StopAllLoadingAssetBundles()
//     {
//         foreach (Coroutine coroutine in activeCoroutines.ToArray())
//         {
//             StopCoroutine(coroutine); // 停止每个活动协程
//         }
//         activeCoroutines.Clear(); // 清空活动列表
//     }

//     // 协程方法
//     private IEnumerator ABLoadFromStreamAsync(string path, string resName)
//     {
//         // 异步加载逻辑
//         yield return new WaitForSeconds(1.0f); // 示例：等待1秒

//         // 处理加载完成后的逻辑
//         // ...
//     }
// }
// 在这个例子中，activeCoroutines列表保存了所有通过StartCoroutine启动的协程实例。你可以通过遍历这个列表并使用StopCoroutine方法来停止特定的协程。注意，一旦协程完成，它会自动从活动列表中移除，因为Coroutine实例会在协程结束时被Unity销毁。

// 在C#中，IEnumerator接口通常用于定义协程的行为。
// 当你声明两个 IEnumerator 类型的变量 currentIEnumerator01 和 currentIEnumerator02 并让它们都引用同一个协程方法时，这两个变量实际上将指向同一个 IEnumerator 实例。
// 这里有一个重要的概念需要理解：IEnumerator 实例本身不是由你创建的，而是由Unity在调用 StartCoroutine 时创建的。
// 当你将一个协程方法（返回 IEnumerator）传递给 StartCoroutine 时，Unity会创建一个新的 IEnumerator 实例来执行该协程方法。
// 因此，即使你有多个IEnumerator类型变量引用同一个协程方法，它们也都会指向由Unity创建的同一个 IEnumerator 实例。
// 例如：
// IEnumerator MyCoroutineMethod()
// {
//     // 协程逻辑
//     yield return null;
// }

// void Start()
// {
//     IEnumerator currentIEnumerator01 = MyCoroutineMethod();
//     IEnumerator currentIEnumerator02 = MyCoroutineMethod();

//     // currentIEnumerator01 和 currentIEnumerator02 实际上引用的是同一个协程实例
//     // 因为它们都是对 MyCoroutineMethod() 的调用，而 MyCoroutineMethod() 只返回一个 IEnumerator 实例

//     Coroutine coroutine01 = StartCoroutine(currentIEnumerator01);
//     Coroutine coroutine02 = StartCoroutine(currentIEnumerator02);

//     // 此时，coroutine01 和 coroutine02 是两个不同的 Coroutine 实例
//     // 因为 StartCoroutine 每次调用都会返回一个新的 Coroutine 实例，即使它们基于相同的 IEnumerator
// }

//在Unity中，StopCoroutine 方法用于停止一个正在运行的协程，该方法参数可用Coroutine或IEnumerator实例。
//但是，通常推荐的做法是使用 Coroutine 实例来停止协程，因为 Coroutine 实例是Unity内部用来跟踪和管理协程的。
//如果你传递一个 IEnumerator 实例给 StopCoroutine 方法，Unity会尝试找到与这个 IEnumerator 实例相关联的 Coroutine 实例并停止它。
//这通常是通过在内部搜索当前正在运行的协程来完成的，找到与提供的 IEnumerator 匹配的协程后停止它。
//然而，这种做法并不是最推荐的，因为它可能不如直接使用 Coroutine 实例来得直接和高效。
//如果你持有 Coroutine 实例（即 currentCoroutine），那么最好直接将它传递给 StopCoroutine 方法，因为这样可以避免Unity进行内部额外的搜索操作。

//这里是一个简单的解释为什么这两种方式都可以工作：
//当你使用 StartCoroutine(currentIEnumerator) 启动一个协程时，Unity会创建一个 Coroutine 实例来管理这个协程的生命周期，并将这个 Coroutine 实例与你提供的 IEnumerator 实例关联起来。
//StopCoroutine 方法可以接受一个 IEnumerator 参数，是因为Unity可以使用这个 IEnumerator 实例来查找并停止与之关联的 Coroutine 实例，这是通过比较内部存储的 IEnumerator 实例来完成的。
//直接使用 Coroutine 实例（currentCoroutine）来停止协程是更直接的方法，因为它直接引用了Unity内部用来管理协程的实例。

//如果你使用 IEnumerator协程方法体的实例作为参数来尝试停止协程，并且多个协程实例Coroutine实际上使用了相同的 IEnumerator 实例，那么所有使用相同 IEnumerator 实例的协程都将被停止。
//这是因为 StopCoroutine 方法会根据提供的 IEnumerator 实例来查找和停止所有相关的协程。
//（这在实践中是不常见的，每个协程实例都应设计成同一时刻有其自己的 IEnumerator 实现）

