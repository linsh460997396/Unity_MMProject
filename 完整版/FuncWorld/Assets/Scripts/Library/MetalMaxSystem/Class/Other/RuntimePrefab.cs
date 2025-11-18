#if UNITY_EDITOR || UNITY_STANDALONE
using System.Collections.Generic;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 运行时预制体.
    /// 用于存储配置、共享数据或资源引用,是纯粹的数据容器(类似加载AB资源包后的存储)
    /// 示范用法RuntimePrefab runtimePrefab = ScriptableObject.CreateInstance<RuntimePrefab>();
    /// 仅当ScriptableObject引用了Native资源（如纹理、网格）时,这些资源会占用Native内存.
    /// </summary>
    public class RuntimePrefab : ScriptableObject
    {
        [SerializeReference] List<string> _keys = new List<string>();
        [SerializeReference] List<Object> _values = new List<Object>();

        public List<string> Keys => _keys;
        public List<Object> Values => _values;

        public void Add(string key, Object value)
        {
            _keys.Add(key);
            _values.Add(value);
        }

        public Object Get(string key)
        {
            int index = _keys.IndexOf(key);
            return index >= 0 ? _values[index] : null;
        }

        /// <summary>
        /// 通过List.Contains检查key存在性
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return _keys.Contains(key);
        }
    }
}

//SO不接收Unity引擎大部分回调,支持一小部分事件方法包括Awake, OnEnable, OnDestroy和OnDisable
//Editor互动时还会从Inspector调用OnValidate和Reset.

// 带属性时的创建方式（编辑器 + 运行时均可）
//[CreateAssetMenu(fileName = "ObjectData", menuName = "Data/ObjectData")]
//public class ObjectData : ScriptableObject { /*...*/ }
// 不带属性时的创建方式（仅限代码）
//var prefab = ScriptableObject.CreateInstance<RuntimePrefab>();

//new GameObject()+Instance做预制体（不从AB包加载的）
//new的对象在场景托管内存,模拟预制体得失活,在Instance后激活,否则默认激活出现在场景还会运行组件

#endif