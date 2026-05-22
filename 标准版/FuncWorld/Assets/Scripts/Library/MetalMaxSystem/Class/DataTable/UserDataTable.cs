using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace MetalMaxSystem
{
    /// <summary>
    /// 数据源类型枚举
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// 普通字典(单线程,最高性能)
        /// </summary>
        Dictionary,
        
        /// <summary>
        /// 线程安全字典(多线程安全)
        /// </summary>
        ConcurrentDictionary,
        
        /// <summary>
        /// 哈希表
        /// </summary>
        HashTable,

        /// <summary>
        /// 线程本地字典(每个线程独有一份字典,无锁竞争)
        /// </summary>
        ThreadDictionary
    }

    /// <summary>
    /// 数据源接口定义
    /// </summary>
    internal interface IDataSource<T>
    {
        bool KeyExists(bool isGlobal, string key);
        T GetValue(bool isGlobal, string key);
        void SetValue(bool isGlobal, string key, T value);
        bool Remove(bool isGlobal, string key);
        void ClearAll(bool isGlobal);
    }

    /// <summary>
    /// 普通字典数据源实现
    /// </summary>
    internal class DictionarySource<T> : IDataSource<T>
    {
        private readonly Dictionary<string, T> _globalDict = new Dictionary<string, T>();
        private readonly Dictionary<string, T> _localDict = new Dictionary<string, T>();

        private Dictionary<string, T> GetDictionary(bool isGlobal)
        {
            return isGlobal ? _globalDict : _localDict;
        }

        public bool KeyExists(bool isGlobal, string key)
        {
            return GetDictionary(isGlobal).ContainsKey(key);
        }

        public T GetValue(bool isGlobal, string key)
        {
            T value;
            return GetDictionary(isGlobal).TryGetValue(key, out value) ? value : default(T);
        }

        public void SetValue(bool isGlobal, string key, T value)
        {
            GetDictionary(isGlobal)[key] = value;
        }

        public bool Remove(bool isGlobal, string key)
        {
            return GetDictionary(isGlobal).Remove(key);
        }

        public void ClearAll(bool isGlobal)
        {
            GetDictionary(isGlobal).Clear();
        }
    }

    /// <summary>
    /// 线程安全字典数据源实现
    /// </summary>
    internal class ConcurrentSource<T> : IDataSource<T>
    {
        private readonly ConcurrentDictionary<string, T> _globalDict = new ConcurrentDictionary<string, T>();
        private readonly ConcurrentDictionary<string, T> _localDict = new ConcurrentDictionary<string, T>();

        private ConcurrentDictionary<string, T> GetDictionary(bool isGlobal)
        {
            return isGlobal ? _globalDict : _localDict;
        }

        public bool KeyExists(bool isGlobal, string key)
        {
            return GetDictionary(isGlobal).ContainsKey(key);
        }

        public T GetValue(bool isGlobal, string key)
        {
            T value;
            return GetDictionary(isGlobal).TryGetValue(key, out value) ? value : default(T);
        }

        public void SetValue(bool isGlobal, string key, T value)
        {
            GetDictionary(isGlobal).AddOrUpdate(key, value, (k, v) => value);
        }

        public bool Remove(bool isGlobal, string key)
        {
            T removed;
            return GetDictionary(isGlobal).TryRemove(key, out removed);
        }

        public void ClearAll(bool isGlobal)
        {
            // ConcurrentDictionary没有Clear方法，需要逐个移除
            var dict = GetDictionary(isGlobal);
            string[] keys = new string[dict.Count];
            dict.Keys.CopyTo(keys, 0);
            foreach (string key in keys)
            {
                T removed;
                dict.TryRemove(key, out removed);
            }
        }
    }

    /// <summary>
    /// 哈希表数据源实现
    /// </summary>
    internal class HashTableSource<T> : IDataSource<T>
    {
        private readonly Hashtable _globalHash = new Hashtable();
        private readonly Hashtable _localHash = new Hashtable();

        private Hashtable GetHashTable(bool isGlobal)
        {
            return isGlobal ? _globalHash : _localHash;
        }

        public bool KeyExists(bool isGlobal, string key)
        {
            return GetHashTable(isGlobal).ContainsKey(key);
        }

        public T GetValue(bool isGlobal, string key)
        {
            object value = GetHashTable(isGlobal)[key];
            return value == null ? default(T) : (T)value;
        }

        public void SetValue(bool isGlobal, string key, T value)
        {
            GetHashTable(isGlobal)[key] = value;
        }

        public bool Remove(bool isGlobal, string key)
        {
            bool exists = GetHashTable(isGlobal).ContainsKey(key);
            if (exists)
            {
                GetHashTable(isGlobal).Remove(key);
            }
            return exists;
        }

        public void ClearAll(bool isGlobal)
        {
            GetHashTable(isGlobal).Clear();
        }
    }

    /// <summary>
    /// 线程本地字典数据源实现(每个线程独有一份字典,无锁竞争)
    /// </summary>
    internal class ThreadDictionarySource<T> : IDataSource<T>
    {
        private readonly ThreadLocal<Dictionary<string, T>> _globalTLDictionary = new ThreadLocal<Dictionary<string, T>>(() => new Dictionary<string, T>());
        private readonly ThreadLocal<Dictionary<string, T>> _localTLDictionary = new ThreadLocal<Dictionary<string, T>>(() => new Dictionary<string, T>());

        private Dictionary<string, T> GetDictionary(bool isGlobal)
        {
            return isGlobal ? _globalTLDictionary.Value : _localTLDictionary.Value;
        }

        public bool KeyExists(bool isGlobal, string key)
        {
            return GetDictionary(isGlobal).ContainsKey(key);
        }

        public T GetValue(bool isGlobal, string key)
        {
            T value;
            return GetDictionary(isGlobal).TryGetValue(key, out value) ? value : default(T);
        }

        public void SetValue(bool isGlobal, string key, T value)
        {
            GetDictionary(isGlobal)[key] = value;
        }

        public bool Remove(bool isGlobal, string key)
        {
            return GetDictionary(isGlobal).Remove(key);
        }

        public void ClearAll(bool isGlobal)
        {
            GetDictionary(isGlobal).Clear();
        }
    }

    /// <summary>
    /// 用户数据表封装类
    /// 支持通过参数切换Dictionary、ConcurrentDictionary、HashTable、ThreadDictionary四种数据源
    /// </summary>
    /// <typeparam name="T">存储值类型</typeparam>
    public static class UserDataTable<T>
    {
        /// <summary>
        /// 当前使用的数据源类型
        /// </summary>
        public static DataSourceType CurrentDataSourceType { get; set; } = DataSourceType.ThreadDictionary;

        // 数据源实例缓存
        private static readonly Dictionary<DataSourceType, IDataSource<T>> _dataSources = 
            new Dictionary<DataSourceType, IDataSource<T>>();

        /// <summary>
        /// 获取当前数据源实例
        /// </summary>
        private static IDataSource<T> CurrentDataSource
        {
            get
            {
                IDataSource<T> source;
                if (!_dataSources.TryGetValue(CurrentDataSourceType, out source))
                {
                    source = CreateDataSource(CurrentDataSourceType);
                    _dataSources[CurrentDataSourceType] = source;
                }
                return source;
            }
        }

        /// <summary>
        /// 根据类型创建数据源实例
        /// </summary>
        private static IDataSource<T> CreateDataSource(DataSourceType type)
        {
            switch (type)
            {
                case DataSourceType.ConcurrentDictionary:
                    return new ConcurrentSource<T>();
                case DataSourceType.HashTable:
                    return new HashTableSource<T>();
                case DataSourceType.ThreadDictionary:
                    return new ThreadDictionarySource<T>();
                case DataSourceType.Dictionary:
                default:
                    return new DictionarySource<T>();
            }
        }

        /// <summary>
        /// 获取指定类型的数据源实例
        /// </summary>
        private static IDataSource<T> GetDataSource(DataSourceType type)
        {
            IDataSource<T> source;
            if (!_dataSources.TryGetValue(type, out source))
            {
                source = CreateDataSource(type);
                _dataSources[type] = source;
            }
            return source;
        }

        #region 基础操作方法

        /// <summary>
        /// 判断键是否存在
        /// </summary>
        /// <param name="isGlobal">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="dataSourceType">数据源类型(默认ThreadDictionary)</param>
        /// <returns>是否存在</returns>
        public static bool KeyExists(bool isGlobal, string key, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetDataSource(dataSourceType).KeyExists(isGlobal, key);
        }

        /// <summary>
        /// 获取键对应的值
        /// </summary>
        /// <param name="isGlobal">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="dataSourceType">数据源类型(默认ThreadDictionary)</param>
        /// <returns>对应的值</returns>
        public static T GetValue(bool isGlobal, string key, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetDataSource(dataSourceType).GetValue(isGlobal, key);
        }

        /// <summary>
        /// 设置键值对
        /// </summary>
        /// <param name="isGlobal">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="dataSourceType">数据源类型(默认ThreadDictionary)</param>
        public static void SetValue(bool isGlobal, string key, T value, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            GetDataSource(dataSourceType).SetValue(isGlobal, key, value);
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        /// <param name="isGlobal">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="dataSourceType">数据源类型(默认ThreadDictionary)</param>
        /// <returns>是否成功移除</returns>
        public static bool Remove(bool isGlobal, string key, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetDataSource(dataSourceType).Remove(isGlobal, key);
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        /// <param name="isGlobal">true=全局, false=局部</param>
        /// <param name="dataSourceType">数据源类型(默认ThreadDictionary)</param>
        public static void ClearAll(bool isGlobal, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            GetDataSource(dataSourceType).ClearAll(isGlobal);
        }

        #endregion

        #region 数组模拟操作 - 清除方法

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        public static void Clear0(bool isGlobal, string key, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            Remove(isGlobal, key, dataSourceType);
        }

        /// <summary>
        /// 从字典中移除Key[], 模拟1维数组
        /// </summary>
        public static void Clear1(bool isGlobal, string key, int lp_1, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            Remove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1), dataSourceType);
        }

        /// <summary>
        /// 从字典中移除Key[,], 模拟2维数组
        /// </summary>
        public static void Clear2(bool isGlobal, string key, int lp_1, int lp_2, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            Remove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2), dataSourceType);
        }

        /// <summary>
        /// 从字典中移除Key[,,], 模拟3维数组
        /// </summary>
        public static void Clear3(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            Remove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3), dataSourceType);
        }

        /// <summary>
        /// 从字典中移除Key[,,,], 模拟4维数组
        /// </summary>
        public static void Clear4(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, int lp_4, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            Remove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4), dataSourceType);
        }

        #endregion

        #region 数组模拟操作 - 保存方法

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        public static void Save0(bool isGlobal, string key, T val, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            SetValue(isGlobal, key, val, dataSourceType);
        }

        /// <summary>
        /// 保存字典键值对, 模拟1维数组
        /// </summary>
        public static void Save1(bool isGlobal, string key, int lp_1, T val, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            SetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1), val, dataSourceType);
        }

        /// <summary>
        /// 保存字典键值对, 模拟2维数组
        /// </summary>
        public static void Save2(bool isGlobal, string key, int lp_1, int lp_2, T val, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            SetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2), val, dataSourceType);
        }

        /// <summary>
        /// 保存字典键值对, 模拟3维数组
        /// </summary>
        public static void Save3(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, T val, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            SetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3), val, dataSourceType);
        }

        /// <summary>
        /// 保存字典键值对, 模拟4维数组
        /// </summary>
        public static void Save4(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, int lp_4, T val, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            SetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4), val, dataSourceType);
        }

        #endregion

        #region 数组模拟操作 - 读取方法

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        public static T Load0(bool isGlobal, string key, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetValue(isGlobal, key, dataSourceType);
        }

        /// <summary>
        /// 读取字典键值对, 模拟1维数组
        /// </summary>
        public static T Load1(bool isGlobal, string key, int lp_1, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1), dataSourceType);
        }

        /// <summary>
        /// 读取字典键值对, 模拟2维数组
        /// </summary>
        public static T Load2(bool isGlobal, string key, int lp_1, int lp_2, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2), dataSourceType);
        }

        /// <summary>
        /// 读取字典键值对, 模拟3维数组
        /// </summary>
        public static T Load3(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3), dataSourceType);
        }

        /// <summary>
        /// 读取字典键值对, 模拟4维数组
        /// </summary>
        public static T Load4(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, int lp_4, DataSourceType dataSourceType = DataSourceType.ThreadDictionary)
        {
            return GetValue(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4), dataSourceType);
        }

        #endregion

        #region 切换数据源

        /// <summary>
        /// 切换数据源类型
        /// </summary>
        /// <param name="newType">新的数据源类型</param>
        /// <param name="migrateData">是否迁移现有数据(仅在必要时使用)</param>
        public static void SwitchDataSource(DataSourceType newType, bool migrateData = false)
        {
            if (CurrentDataSourceType == newType)
                return;

            if (migrateData)
            {
                // 迁移数据逻辑（可选功能）
                MigrateData(CurrentDataSourceType, newType);
            }

            CurrentDataSourceType = newType;
        }

        /// <summary>
        /// 数据迁移辅助方法
        /// </summary>
        private static void MigrateData(DataSourceType fromType, DataSourceType toType)
        {
            // 实现数据迁移逻辑（如需）
            // 注意：这是一个复杂操作，需要考虑线程安全
        }

        #endregion
    }
}
