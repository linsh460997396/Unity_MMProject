//#define UNITY_STANDALONE //BepInEx制作UnityMOD时可手动启用
#if UNITY_EDITOR || UNITY_STANDALONE
using System.Collections.Generic;
using UnityEngine;

namespace MetalMaxSystem.Unity
{
    /// <summary>
    /// 运行时预制体.
    /// 用于存储配置、共享数据或资源引用,是纯粹的数据容器(模拟加载AB包资源后的状态).
    /// 示范用法RuntimePrefab runtimePrefab = ScriptableObject.CreateInstance<RuntimePrefab>();
    /// 当RuntimePrefab.Add(string key, Object obj,bool clone = false)中的clone参数为true时,资源为副本存储,
    /// 当clone参数为false时,直接存储对象,摧毁原对象会影响该字典内容,场景切换时未被DontDestroyOnLoad保护的实例会被Unity自动销毁‌,请做好保护.
    /// </summary>
    public class RuntimePrefab : ScriptableObject
    {
        [SerializeReference] List<string> _keys = new List<string>();
        [SerializeReference] List<Object> _values = new List<Object>();

        public List<string> Keys => _keys;
        public List<Object> Values => _values;

        /// <summary>
        /// 添加资源到RuntimePrefab.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="clone">是否克隆,默认false,资源直接存储,反之以副本存储</param>
        public void Add(string key, Object obj,bool clone = false)
        {
            _keys.Add(key);
            if (!clone)
                _values.Add(obj);
            else
                _values.Add(Object.Instantiate(obj, null));
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
#endif
