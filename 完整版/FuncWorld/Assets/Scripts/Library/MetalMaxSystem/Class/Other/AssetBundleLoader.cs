#if UNITY_EDITOR || UNITY_STANDALONE
using System.IO;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 资源方法类,提供AssetBundle资源管理方案.当使用异步加载时,该类只能有1个正在加载资源的协程,届时用Resource.isCoroutineRunning来检查是否可以开启新协程.
    /// 协程必须在MonoBehaviour组件实例中创建和运行，所以本类必须作为组件挂载到一个GameObject上才能使用.
    /// </summary>
    public class AssetBundleLoader : MonoBehaviour
    {
        private static AssetBundleLoader _instance; //这个处理AB包的类只需单例
        private static Queue<Action> actions = new Queue<Action>();
        private static Queue<IEnumerator> coroutines = new Queue<IEnumerator>();

        //资源字典,用于存储加载的资源实例.string用于区分多个不同存储键区.
        //以下字段在协程实例才有结果存储,加不加静态修饰都无所谓,但加静态后可通过类名直接访问,少写一个Instance.

        /// <summary>
        /// 全局存储协程实例成功加载到的素材数组.
        /// </summary>
        private static Dictionary<string, Object[]> _objectGroup = new Dictionary<string, Object[]>();
        /// <summary>
        /// 全局存储协程实例成功加载到的单个素材.
        /// </summary>
        private static Dictionary<string, Object> _object = new Dictionary<string, Object>();
        /// <summary>
        /// 全局存储协程实例成功加载到的AB包.
        /// </summary>
        private static Dictionary<string, AssetBundle> _assetBundle = new Dictionary<string, AssetBundle>();
        /// <summary>
        /// 全局存储的协程枚举器对象.
        /// </summary>
        private static Dictionary<string, IEnumerator> _iEnumerator = new Dictionary<string, IEnumerator>();
        /// <summary>
        /// 全局存储的协程实例.
        /// </summary>
        private static Dictionary<string, Coroutine> _coroutine = new Dictionary<string, Coroutine>();

        /// <summary>
        /// 临时存储当前协程实例成功加载到的素材数组,注意本字段会被覆盖.
        /// </summary>
        public static Object[] currentObjectGroup;
        /// <summary>
        /// 临时存储当前协程实例成功加载到的单个素材,注意本字段会被覆盖.
        /// </summary>
        public static Object currentObject;
        /// <summary>
        /// 临时存储当前协程实例成功加载到的AB包,注意本字段会被覆盖.
        /// </summary>
        public static AssetBundle currentAssetBundle;
        /// <summary>
        /// 临时存储的当前协程枚举器对象,注意本字段会被覆盖.
        /// </summary>
        public static IEnumerator currentIEnumerator;
        /// <summary>
        /// 临时存储的当前协程实例,注意本字段会被覆盖.
        /// </summary>
        public static Coroutine currentCoroutine;

        /// <summary>
        /// 通过静态方法创建AssetBundleLoader的单例实例.
        /// 本组件实例挂载于名为"AssetBundleLoader"的GameObject上.
        /// 本类所封装的处理AB包素材异步加载的协程依赖本组件实例运行.
        /// </summary>
        public static AssetBundleLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("AssetBundleLoader");
                    _instance = obj.AddComponent<AssetBundleLoader>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        private void Update()
        {
            //即使Enqueue已加锁,Update遍历队列时若不加锁可能遇到Enqueue执行到一半时主线程的Update开始遍历队列->读取到不一致的队列状态

            lock (actions)
            {
                while (actions.Count > 0)
                {
                    actions.Dequeue()?.Invoke();
                }
            }

            lock (coroutines)
            {
                while (coroutines.Count > 0)
                {
                    StartCoroutine(coroutines.Dequeue());
                }
            }
        }

        //↓封装的回调函数(支持在子线程使用,然后由主线程回调执行)

        /// <summary>
        /// 回调函数.
        /// 对Unity引擎组件实例相关动作执行回调(回到主线程调用Action).
        /// 示例AssetBundleLoader.Instance.Invoke(() =>{涉及主线程对象的动作});
        /// </summary>
        /// <param name="action">这个匿名委托将被添加到队列,由一个专门处理回调的MonoBehaviour组件实例来跑</param>
        public static void Invoke(Action action)
        {
            if (action == null) return;

            lock (actions)
            {
                actions.Enqueue(action);
            }
        }

        /// <summary>
        /// 回调函数.
        /// 对Unity引擎组件实例相关动作执行回调(回到主线程调用协程).
        /// 示例AssetBundleLoader.Instance.Invoke(MyCoroutine());
        /// </summary>
        public static void Invoke(IEnumerator coroutine)
        {
            if (coroutine == null) return;

            lock (coroutines)
            {
                coroutines.Enqueue(coroutine);
            }
        }

        /// <summary>
        /// 回调函数.直接传入协程方法体.
        /// 对Unity引擎组件实例相关动作执行回调(回到主线程调用协程).
        /// 示例AssetBundleLoader.Instance.Invoke(() => {
        ///     yield return new WaitForSeconds(1);
        ///     Debug.Log("Delayed log");
        /// });
        /// </summary>
        public static void Invoke(Func<IEnumerator> coroutineFunc)
        {
            if (coroutineFunc == null) return;
            Invoke(coroutineFunc());
        }

        //↓获取协程读取资源时存储在字典的素材对象

        /// <summary>
        /// 获取字典中存储的AssetBundle实例.
        /// </summary>
        /// <param name="key"></param>
        public static AssetBundle GetAssetBundle(string key)
        {
            if (!_assetBundle.ContainsKey("ABLoader_AssetBundle" + key))
            {
                return null;
            }
            return _assetBundle["ABLoader_AssetBundle" + key];
        }

        /// <summary>
        /// 获取字典中存储的Object实例.
        /// </summary>
        /// <param name="key"></param>
        public static T GetObject<T>(string key) where T : Object
        {
            //注意:若Object是GameObject类型,则返回的Object实例可以直接转换为GameObject类型
            if (!_object.ContainsKey("ABLoader_Object" + key))
            {
                return null;
            }
            return _object["ABLoader_Object" + key] as T;
        }

        /// <summary>
        /// 获取字典中存储的Object实例数组.
        /// </summary>
        /// <param name="key"></param>
        public static T[] GetObjectGroup<T>(string key) where T : Object
        {
            if (!_objectGroup.ContainsKey("ABLoader_ObjectGroup" + key))
            {
                return null;
            }
            return _objectGroup["ABLoader_ObjectGroup" + key] as T[];
        }

        /// <summary>
        /// 获取字典中存储的IEnumerator实例.
        /// </summary>
        /// <param name="key"></param>
        public static IEnumerator GetIEnumerator(string key)
        {
            if (!_iEnumerator.ContainsKey("ABLoader_IEnumerator" + key))
            {
                return null;
            }
            return _iEnumerator["ABLoader_IEnumerator" + key];
        }

        /// <summary>
        /// 获取字典中存储的Coroutine实例.
        /// </summary>
        /// <param name="key"></param>
        public static Coroutine GetCoroutine(string key)
        {
            if (!_coroutine.ContainsKey("ABLoader_Coroutine" + key))
            {
                return null;
            }
            return _coroutine["ABLoader_Coroutine" + key];
        }

        //↑因为是单例模式,填Static也无妨,且可通过类名直接访问,少写一个Instance.

        #region 这些代码是用来加载AB包的具体方法(若协程调用失败,请用Instance方法创建组件实例来运行,协程必须依赖1个MonoBehaviour实例来运行)

        //函数在Unity中使用正常,素材也实例化成功,但进行BepInEx制作MOD时报错:①读取文件报header问题；②读取字节则内存数据无法成功解压
        //故障原因跟Unity版本有关,建议用游戏同版本Unity去转AB包素材

        /// <summary>
        /// 以文件的形式加载AssetBundle中指定名称的单个素材.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName">AB包根目录下素材的原始路径,只填名称则搜索到第一个同名元素即返回</param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public static T ABLoadFromFile<T>(string path, string resName, out AssetBundle assetBundle) where T : Object
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            return assetBundle.LoadAsset<T>(resName);
        }
        /// <summary>
        /// 以byte[]形式加载AssetBundle中指定名称的单个素材.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName">AB包根目录下素材的原始路径,只填名称则搜索到第一个同名元素即返回</param>
        /// <param name="assetBundle"></param>
        public static T ABLoadFromMemory<T>(string path, string resName, out AssetBundle assetBundle) where T : Object
        {
            assetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
            return assetBundle.LoadAsset<T>(resName);
        }
        /// <summary>
        /// 以流的形式加载AssetBundle中指定名称的单个素材.文件流FileStream采用系统默认缓冲区大小(4KB)进行流式读取.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName">AB包根目录下素材的原始路径,只填名称则搜索到第一个同名元素即返回</param>
        /// <param name="assetBundle"></param>
        /// <param name="bufferSize">文件流缓冲区大小</param>
        /// <returns></returns>
        public static T ABLoadFromStream<T>(string path, string resName, out AssetBundle assetBundle, int bufferSize = 4096) where T : Object
        {
            //可自行指定FileStream缓冲区大小
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            {
                assetBundle = AssetBundle.LoadFromStream(stream);
                return assetBundle.LoadAsset<T>(resName);
            }
        }
        /// <summary>
        /// 以文件的形式加载AssetBundle中指定类型的全部素材.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetBundle"></param>
        /// <returns></returns>
        public static T[] ABLoadFromFile<T>(string path, out AssetBundle assetBundle) where T : Object
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            return assetBundle.LoadAllAssets<T>();
        }
        /// <summary>
        /// 以byte[]形式加载AssetBundle中指定类型的全部素材.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetBundle"></param>
        public static T[] ABLoadFromMemory<T>(string path, out AssetBundle assetBundle) where T : Object
        {
            assetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(path));
            return assetBundle.LoadAllAssets<T>();
        }
        /// <summary>
        /// 以流的形式加载AssetBundle中指定类型的全部素材.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="assetBundle"></param>
        /// <param name="bufferSize">文件流缓冲区大小</param>
        public static T[] ABLoadFromStream<T>(string path, out AssetBundle assetBundle, int bufferSize = 4096) where T : Object
        {
            //可自行指定FileStream缓冲区大小
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            {
                assetBundle = AssetBundle.LoadFromStream(stream);
                return assetBundle.LoadAllAssets<T>();
            }
        }

        //↓异步方法全部是实例方法,必须依赖MonoBehaviour组件实例来运行协程

        /// <summary>
        /// 异步以文件的形式加载AssetBundle中指定名称的单个素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObject(key)获取加载的素材实例,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName">AB包根目录下素材的原始路径,只填名称则搜索到第一个同名元素即返回</param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        private static IEnumerator ABLoadFromFileAsync<T>(string path, string resName, string key = "") where T : Object
        {
            //AssetBundleCreateRequest 类是 AssetBundle 的一个实例,它表示一个异步加载请求.
            //AssetBundle.LoadFromFileAsync 方法使用指定路径的文件来创建一个新的 AssetBundleCreateRequest 实例.

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;
            if (request.assetBundle == null)
            {
                Debug.LogError("AssetBundle加载失败: " + path);
                yield break;
            }
            if (key != null && key != "")
            {
                _assetBundle["ABLoader_AssetBundle" + key] = request.assetBundle;
                _object["ABLoader_GameObject" + key] = request.assetBundle.LoadAsset<T>(resName);
            }
            else
            {
                currentAssetBundle = request.assetBundle;
                currentObject = request.assetBundle.LoadAsset<T>(resName);
            }
            //在Unity中,协程(Coroutine)会自动管理其生命周期,协程中的IEnumerator方法执行完毕,协程就会自行结束,不需要显式地调用Stop方法.
        }
        /// <summary>
        /// 异步以byte[]形式加载AssetBundle中指定名称的单个素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObject(key)获取加载的素材实例,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName">AB包根目录下素材的原始路径,只填名称则搜索到第一个同名元素即返回</param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        private static IEnumerator ABLoadFromMemoryAsync<T>(string path, string resName, string key = "") where T : Object
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
            yield return request;
            if (request.assetBundle == null)
            {
                Debug.LogError("AssetBundle加载失败: " + path);
                yield break;
            }
            if (key != null && key != "")
            {
                _assetBundle["ABLoader_AssetBundle" + key] = request.assetBundle;
                _object["ABLoader_GameObject" + key] = request.assetBundle.LoadAsset<T>(resName);
            }
            else
            {
                currentAssetBundle = request.assetBundle;
                currentObject = request.assetBundle.LoadAsset<T>(resName);
            }
        }
        /// <summary>
        /// 异步以流的形式加载AssetBundle中指定名称的单个素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObject(key)获取加载的素材实例,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="key">存储键区</param>
        /// <param name="bufferSize">文件流缓冲区大小</param>
        /// <returns></returns>
        private static IEnumerator ABLoadFromStreamAsync<T>(string path, string resName, string key = "", int bufferSize = 4096) where T : Object
        {
            //可自行指定FileStream缓冲区大小
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            {
                AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
                //yield return 语句用于将控制权交还调用上下文,直到异步加载请求完成.一旦请求完成,后续将使用 LoadAsset 方法从 AssetBundle 中加载指定名称的游戏对象.
                yield return request;
                if (request.assetBundle == null)
                {
                    Debug.LogError("AssetBundle加载失败: " + path);
                    yield break;
                }
                if (key != null && key != "")
                {
                    _assetBundle["ABLoader_AssetBundle" + key] = request.assetBundle;
                    _objectGroup["ABLoader_ObjectGroup" + key] = request.assetBundle.LoadAllAssets<T>();
                }
                else
                {
                    currentAssetBundle = request.assetBundle;
                    currentObjectGroup = request.assetBundle.LoadAllAssets<T>();
                }
            }
        }
        /// <summary>
        /// 异步以文件的形式加载AssetBundle中指定名称的单个素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObject(key)获取加载的素材实例,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        public void LoadFromFileAsync<T>(string path, string resName, string key = "") where T : Object
        {
            //启动协程并保存其引用
            if (key != null && key != "")
            {
                _iEnumerator["ABLoader_IEnumerator" + key] = ABLoadFromFileAsync<T>(path, resName, key);
                _coroutine["ABLoader_Coroutine" + key] = StartCoroutine(_iEnumerator["ABLoader_IEnumerator" + key]);
            }
            else
            {
                currentIEnumerator = ABLoadFromFileAsync<T>(path, resName, key);
                currentCoroutine = StartCoroutine(currentIEnumerator);
            }
        }
        /// <summary>
        /// 异步以byte[]形式加载AssetBundle中指定名称的单个素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObject(key)获取加载的素材实例,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        public void LoadFromMemoryAsync<T>(string path, string resName, string key = "") where T : Object
        {
            //启动协程并保存其引用
            if (key != null && key != "")
            {
                _iEnumerator["ABLoader_IEnumerator" + key] = ABLoadFromMemoryAsync<T>(path, resName, key);
                _coroutine["ABLoader_Coroutine" + key] = StartCoroutine(_iEnumerator["ABLoader_IEnumerator" + key]);
            }
            else
            {
                currentIEnumerator = ABLoadFromMemoryAsync<T>(path, resName, key);
                currentCoroutine = StartCoroutine(currentIEnumerator);
            }
        }
        /// <summary>
        /// 异步以流的形式加载AssetBundle中指定名称的单个素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObject(key)获取加载的素材实例,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="resName"></param>
        /// <param name="key">存储键区</param>
        /// <param name="bufferSize">文件流缓冲区大小</param>
        /// <returns></returns>
        public void LoadFromStreamAsync<T>(string path, string resName, string key = "", int bufferSize = 4096) where T : Object
        {
            //启动协程并保存其引用
            if (key != null && key != "")
            {
                _iEnumerator["ABLoader_IEnumerator" + key] = ABLoadFromStreamAsync<T>(path, resName, key, bufferSize);
                _coroutine["ABLoader_Coroutine" + key] = StartCoroutine(_iEnumerator["ABLoader_IEnumerator" + key]);
            }
            else
            {
                currentIEnumerator = ABLoadFromStreamAsync<T>(path, resName, key);
                currentCoroutine = StartCoroutine(currentIEnumerator);
            }
        }

        /// <summary>
        /// 异步以文件的形式加载AssetBundle中指定类型的全部素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObjectGroup(key)获取加载的素材实例数组,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        private static IEnumerator ABLoadAllFromFileAsync<T>(string path, string key = "") where T : Object
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            yield return request;
            if (request.assetBundle == null)
            {
                Debug.LogError("AssetBundle加载失败: " + path);
                yield break;
            }
            if (!string.IsNullOrEmpty(key))
            {
                _assetBundle["ABLoader_AssetBundle" + key] = request.assetBundle;
                _objectGroup["ABLoader_ObjectGroup" + key] = request.assetBundle.LoadAllAssets<T>();
                Debug.Log($"成功加载{_objectGroup["ABLoader_ObjectGroup" + key].Length}个{typeof(T).Name}类型资源");
            }
            else
            {
                currentAssetBundle = request.assetBundle;
                currentObjectGroup = request.assetBundle.LoadAllAssets<T>();
                Debug.Log($"成功加载{currentObjectGroup.Length}个{typeof(T).Name}类型资源");
            }
        }
        /// <summary>
        /// 异步以文件的形式加载AssetBundle中指定类型的全部素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObjectGroup(key)获取加载的素材实例数组,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        public void LoadAllFromFileAsync<T>(string path, string key = "") where T : Object
        {
            //启动协程并保存其引用
            if (!string.IsNullOrEmpty(key))
            {
                _iEnumerator["ABLoader_IEnumerator" + key] = ABLoadAllFromFileAsync<T>(path, key);
                _coroutine["ABLoader_Coroutine" + key] = StartCoroutine(_iEnumerator["ABLoader_IEnumerator" + key]);
            }
            else
            {
                currentIEnumerator = ABLoadAllFromFileAsync<T>(path, key);
                currentCoroutine = StartCoroutine(currentIEnumerator);
            }
        }
        /// <summary>
        /// 异步以byte[]形式加载AssetBundle中指定类型的全部素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObjectGroup(key)获取加载的素材实例数组,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        private static IEnumerator ABLoadAllFromMemoryAsync<T>(string path, string key = "") where T : Object
        {
            //Debug.Log($"AssetBundleLoader<{typeof(T).Name}>: 进入泛型协程");
            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
            yield return request;
            if (request.assetBundle == null)
            {
                Debug.LogError("AssetBundle加载失败: " + path);
                yield break;
            }
            if (!string.IsNullOrEmpty(key))
            {
                _assetBundle["ABLoader_AssetBundle" + key] = request.assetBundle;
                _objectGroup["ABLoader_ObjectGroup" + key] = request.assetBundle.LoadAllAssets<T>();
                Debug.Log($"成功加载{_objectGroup["ABLoader_ObjectGroup" + key].Length}个{typeof(T).Name}类型资源");
            }
            else
            {
                currentAssetBundle = request.assetBundle;
                currentObjectGroup = request.assetBundle.LoadAllAssets<T>();
                Debug.Log($"成功加载{currentObjectGroup.Length}个{typeof(T).Name}类型资源");
            }
        }
        /// <summary>
        /// 异步以byte[]形式加载AssetBundle中指定类型的全部素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObjectGroup(key)获取加载的素材实例数组,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key">存储键区</param>
        /// <returns></returns>
        public void LoadAllFromMemoryAsync<T>(string path, string key = "") where T : Object
        {
            //启动协程并保存其引用
            if (!string.IsNullOrEmpty(key))
            {
                _iEnumerator["ABLoader_IEnumerator" + key] = ABLoadAllFromMemoryAsync<T>(path, key);
                _coroutine["ABLoader_Coroutine" + key] = StartCoroutine(_iEnumerator["ABLoader_IEnumerator" + key]);
            }
            else
            {
                currentIEnumerator = ABLoadAllFromMemoryAsync<T>(path, key);
                currentCoroutine = StartCoroutine(currentIEnumerator);
            }
        }
        /// <summary>
        /// 异步以流的形式加载AssetBundle中指定类型的全部素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObjectGroup(key)获取加载的素材实例数组,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key">存储键区</param>
        /// <param name="bufferSize">文件流缓冲区大小</param>
        /// <returns></returns>
        private static IEnumerator ABLoadAllFromStreamAsync<T>(string path, string key = "", int bufferSize = 4096) where T : Object
        {
            //可自行指定FileStream缓冲区大小
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
            {
                AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
                yield return request;
                if (request.assetBundle == null)
                {
                    Debug.LogError("AssetBundle加载失败: " + path);
                    yield break;
                }
                if (!string.IsNullOrEmpty(key))
                {
                    _assetBundle["ABLoader_AssetBundle" + key] = request.assetBundle;
                    _objectGroup["ABLoader_ObjectGroup" + key] = request.assetBundle.LoadAllAssets<T>();
                    Debug.Log($"成功加载{_objectGroup["ABLoader_ObjectGroup" + key].Length}个{typeof(T).Name}类型资源");
                }
                else
                {
                    currentAssetBundle = request.assetBundle;
                    currentObjectGroup = request.assetBundle.LoadAllAssets<T>();
                    Debug.Log($"成功加载{currentObjectGroup.Length}个{typeof(T).Name}类型资源");
                }
            }

        }
        /// <summary>
        /// 异步以流的形式加载AssetBundle中指定类型的全部素材并存储在字典.要注意所有的异步加载都会开启协程(依赖MonoBehaviour实例来运行).
        /// 加载完成可通过GetGameObjectGroup(key)获取加载的素材实例数组,或通过GetAssetBundle(key)获取加载的AssetBundle实例.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key">存储键区</param>
        /// <param name="bufferSize">文件流缓冲区大小</param>
        /// <returns></returns>
        public void LoadAllFromStreamAsync<T>(string path, string key = "", int bufferSize = 4096) where T : Object
        {
            //启动协程并保存其引用
            if (!string.IsNullOrEmpty(key))
            {
                _iEnumerator["ABLoader_IEnumerator" + key] = ABLoadAllFromStreamAsync<T>(path, key);
                _coroutine["ABLoader_Coroutine" + key] = StartCoroutine(_iEnumerator["ABLoader_IEnumerator" + key]);
            }
            else
            {
                currentIEnumerator = ABLoadAllFromStreamAsync<T>(path, key);
                currentCoroutine = StartCoroutine(currentIEnumerator);
            }
        }

        /// <summary>
        /// 步进协程(异步加载资源),让没完成的协程继续往下跑.
        /// MoveNext()方法一般不是直接调用的,在Unity中协程的推进是由Unity的引擎自动管理的.
        /// 当协程挂起(yield return)等待某个操作完成时,引擎会在适当的时候自动调用MoveNext()来恢复协程的执行,故不需要(也不应该)手动调用MoveNext().
        /// 若同一函数特征的协程方法被执行多次(绑定多个协程实例)的话,无法精准操作每个实例个体的步进,会形成批量一起催促(慎用).
        /// </summary>
        public static void MoveNext(IEnumerator currentIEnumerator)
        {
            currentIEnumerator.MoveNext(); //内部检索所有绑定的Coroutine并执行,如绑多个Coroutine的话无法精准操作每个个体只能批量一起催促
        }

        #endregion
    }
}

// 在Unity中,Coroutine 和 IEnumerator 是两个不同的概念,但它们紧密相关,通常一起使用来实现协程的功能.
// 理解它们的区别和用途对于正确使用协程是非常重要.

// IEnumerator
// IEnumerator 是一个接口,定义在C#的 System.Collections 命名空间中.
// 它通常用于枚举集合的元素,但在Unity的上下文中,IEnumerator 被用作协程的基础.
// 一个实现了 IEnumerator 的方法必须包含 Current 属性和 MoveNext() 方法.
// 在Unity的协程中,Current 属性通常不被使用,而 MoveNext() 方法则用于控制协程的执行流程.
// 当你声明一个协程时,你实际上是在创建一个返回类型为 IEnumerator 的方法.
// 例如:
// private IEnumerator MyCoroutine()
// {
//     //协程逻辑
//     yield return null; //暂停协程
// }

// Coroutine
// 在Unity中,Coroutine 是一个类,它代表了一个正在运行的协程实例.
// 你不能直接创建一个 Coroutine 实例；相反,你通过调用 MonoBehaviour 的 StartCoroutine 方法来启动一个协程,该方法接受一个返回类型为 IEnumerator 的方法作为参数.
// StartCoroutine 方法返回一个 Coroutine 对象,该对象代表了正在运行的协程,你可以用它来停止协程(通过调用 StopCoroutine 方法).
// 例如:
// private void Start()
// {
//     //启动协程,并获取返回的Coroutine对象
//     Coroutine myCoroutine = StartCoroutine(MyCoroutine());

//     //在某个时刻,你可能想要停止这个协程
//     StopCoroutine(myCoroutine);
// }

// private IEnumerator MyCoroutine()
// {
//     //协程逻辑
//     yield return null; //暂停协程
// }

// 为什么需要Coroutine类
// Coroutine 类是Unity协程系统的核心部分,它负责管理协程的生命周期和执行流程.
// Unity在每一帧都会调用协程的 MoveNext 方法来检查是否应该继续执行协程,或者是否应该等待某个条件(例如,等待一段时间或等待某个事件发生).
// 通过将协程逻辑封装在实现了 IEnumerator 接口的方法中,并通过 Coroutine 类来管理这些协程,
// Unity提供了一种灵活且强大的机制,允许开发者在游戏的运行过程中以非阻塞的方式执行复杂的逻辑,如延迟操作、等待用户输入或异步加载资源等.
// 简而言之,IEnumerator 是协程的基础接口,定义了协程的执行流程,而 Coroutine 类则是Unity用来管理协程实例的生命周期和执行状态的类.
// 两者结合使用,使得开发者能够方便地在Unity游戏中实现异步操作和复杂逻辑控制.

//备注:当协程所在类提供程序入口方法并启动的情况下,不用实例化自己直接调用本类方法执行协程,不需要手动步进协程发现资源已经加载完了(大概是等同静态方式调用模板方法,内部把结果跑完了).

//MoveNext()方法一般不是直接调用的.在Unity中,协程的推进是由Unity的引擎自动管理的.
//当协程挂起(yield return)等待某个操作完成时,引擎会在适当的时候自动调用MoveNext()来恢复协程的执行,故不需要(也不应该)手动调用MoveNext().

// 在Unity中,每次调用StartCoroutine方法都会创建一个新的协程实例,即使你传递的是同一个IEnumerator方法的引用.
// 这意味着,若你有多次调用StartCoroutine(ABLoadFromStreamAsync(path, resName)),每次都会启动一个新的协程,并且每个协程都会有自己的执行上下文和状态.

// MoveNext是IEnumerator接口的一个方法,当你有一个实现了该接口的实例(比如一个List<T>.Enumerator或者一个自定义的协程IEnumerator)时,你可以调用它来推进枚举器的状态.
// 实现了接口的类的实例虽然可以调用接口方法,但currentIEnumerator.MoveNext()这样的调用通常是不正确的,因为IEnumerator实例本身并没有MoveNext方法.
// 对Unity的协程来说,MoveNext的调用是由Unity引擎内部管理的,并且是在协程挂起(yield语句)之后由引擎自动处理的,用户不需要(也不应该)直接调用MoveNext来推进协程的执行.

// 若你尝试多次调用StartCoroutine同一个协程方法,并且想要控制这些协程的执行,应该为每个协程保存一个独立的Coroutine实例.
// 例如:
// public class MyMonoBehaviour : MonoBehaviour
// {
//     private List<Coroutine> activeCoroutines = new List<Coroutine>(); //保存所有活动协程的列表

//     //启动协程的方法
//     public void StartLoadingAssetBundle(string path, string resName)
//     {
//         Coroutine newCoroutine = StartCoroutine(ABLoadFromStreamAsync(path, resName));
//         activeCoroutines.Add(newCoroutine); //将新协程添加到活动列表
//     }

//     //停止所有协程的方法
//     public void StopAllLoadingAssetBundles()
//     {
//         foreach (Coroutine _iEnumerator in activeCoroutines.ToArray())
//         {
//             StopCoroutine(_iEnumerator); //停止每个活动协程
//         }
//         activeCoroutines.Clear(); //清空活动列表
//     }

//     //协程方法
//     private IEnumerator ABLoadFromStreamAsync(string path, string resName)
//     {
//         //异步加载逻辑
//         yield return new WaitForSeconds(1.0f); //示例:等待1秒

//         //处理加载完成后的逻辑
//         //...
//     }
// }
// 在这个例子中,activeCoroutines列表保存了所有通过StartCoroutine启动的协程实例.你可以通过遍历这个列表并使用StopCoroutine方法来停止特定的协程.注意,一旦协程完成,它会自动从活动列表中移除,因为Coroutine实例会在协程结束时被Unity销毁.

// 在C#中,IEnumerator接口通常用于定义协程的行为.
// 当你声明两个 IEnumerator 类型的变量 currentIEnumerator01 和 currentIEnumerator02 并让它们都引用同一个协程方法时,这两个变量实际上将指向同一个 IEnumerator 实例.
// 这里有一个重要的概念需要理解:IEnumerator 实例本身不是由你创建的,而是由Unity在调用 StartCoroutine 时创建的.
// 当你将一个协程方法(返回 IEnumerator)传递给 StartCoroutine 时,Unity会创建一个新的 IEnumerator 实例来执行该协程方法.
// 因此,即使你有多个IEnumerator类型变量引用同一个协程方法,它们也都会指向由Unity创建的同一个 IEnumerator 实例.
// 例如:
// IEnumerator MyCoroutineMethod()
// {
//     //协程逻辑
//     yield return null;
// }

// void Start()
// {
//     IEnumerator currentIEnumerator01 = MyCoroutineMethod();
//     IEnumerator currentIEnumerator02 = MyCoroutineMethod();

//     //currentIEnumerator01 和 currentIEnumerator02 实际上引用的是同一个协程实例
//     //因为它们都是对 MyCoroutineMethod() 的调用,而 MyCoroutineMethod() 只返回一个 IEnumerator 实例

//     Coroutine coroutine01 = StartCoroutine(currentIEnumerator01);
//     Coroutine coroutine02 = StartCoroutine(currentIEnumerator02);

//     //此时,coroutine01 和 coroutine02 是两个不同的 Coroutine 实例
//     //因为 StartCoroutine 每次调用都会返回一个新的 Coroutine 实例,即使它们基于相同的 IEnumerator
// }

//在Unity中,StopCoroutine 方法用于停止一个正在运行的协程,该方法参数可用Coroutine或IEnumerator实例.
//但是,通常推荐的做法是使用 Coroutine 实例来停止协程,因为 Coroutine 实例是Unity内部用来跟踪和管理协程的.
//若你传递一个 IEnumerator 实例给 StopCoroutine 方法,Unity会尝试找到与这个 IEnumerator 实例相关联的 Coroutine 实例并停止它.
//这通常是通过在内部搜索当前正在运行的协程来完成的,找到与提供的 IEnumerator 匹配的协程后停止它.
//然而,这种做法并不是最推荐的,因为它可能不如直接使用 Coroutine 实例来得直接和高效.
//若你持有 Coroutine 实例(即 currentCoroutine),那么最好直接将它传递给 StopCoroutine 方法,因为这样可以避免Unity进行内部额外的搜索操作.

//这里是一个简单的解释为什么这两种方式都可以工作:
//当你使用 StartCoroutine(currentIEnumerator) 启动一个协程时,Unity会创建一个 Coroutine 实例来管理这个协程的生命周期,并将这个 Coroutine 实例与你提供的 IEnumerator 实例关联起来.
//StopCoroutine 方法可以接受一个 IEnumerator 参数,是因为Unity可以使用这个 IEnumerator 实例来查找并停止与之关联的 Coroutine 实例,这是通过比较内部存储的 IEnumerator 实例来完成的.
//直接使用 Coroutine 实例(currentCoroutine)来停止协程是更直接的方法,因为它直接引用了Unity内部用来管理协程的实例.

//若你使用 IEnumerator协程方法体的实例作为参数来尝试停止协程,并且多个协程实例Coroutine实际上使用了相同的 IEnumerator 实例,那么所有使用相同 IEnumerator 实例的协程都将被停止.
//这是因为 StopCoroutine 方法会根据提供的 IEnumerator 实例来查找和停止所有相关的协程.
//(这在实践中是不常见的,每个协程实例都应设计成同一时刻有其自己的 IEnumerator 实现)

#endif