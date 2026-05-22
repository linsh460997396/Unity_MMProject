using System.Collections.Generic;
using System.Threading;

namespace MetalMaxSystem
{
    /// <summary>
    /// 线程本地字典封装的数据表(每个线程独有一份字典,无锁竞争).
    /// 效率: ThreadLocal字典 > ConcurrentDictionary
    /// 特点: 每个线程有独立的字典实例,线程间数据隔离,无需锁,适合线程本地临时数据存储.
    /// </summary>
    /// <typeparam name="T">字典中存储的值的类型</typeparam>
    public static class TDataTable<T>
    {
        //根据T不同,每次调用TDataTable<T>都会创建一个新的静态类实例,因此每个T类型都有独立的字典存储空间.

        private static ThreadLocal<Dictionary<string, T>> _globalTLDictionary = new ThreadLocal<Dictionary<string, T>>(() => new Dictionary<string, T>());
        /// <summary>
        /// 线程本地全局字典<string, T> (不排泄,直到程序结束或线程销毁)
        /// </summary>
        public static Dictionary<string, T> GlobalTLDictionary
        {
            get { return _globalTLDictionary.Value; }
        }

        private static ThreadLocal<Dictionary<string, T>> _localTLDictionary = new ThreadLocal<Dictionary<string, T>>(() => new Dictionary<string, T>());
        /// <summary>
        /// 线程本地临时字典<string, T> (函数或动作集结束时应手动排泄)
        /// </summary>
        public static Dictionary<string, T> LocalTLDictionary
        {
            get { return _localTLDictionary.Value; }
        }

        #region 线程本地字典操作方法

        #region 基础方法 - 添加/获取/移除

        /// <summary>
        /// 添加字典键值对(重复添加则覆盖)
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        private static void Set(bool place, string key, T value)
        {
            if (place)
            {
                _globalTLDictionary.Value[key] = value;
            }
            else
            {
                _localTLDictionary.Value[key] = value;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        public static bool KeyExists(bool place, string key)
        {
            if (place) { return _globalTLDictionary.Value.ContainsKey(key); }
            else { return _localTLDictionary.Value.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="value">值</param>
        /// <returns>是否存在</returns>
        public static bool ValueExists(bool place, T value)
        {
            if (place) { return _globalTLDictionary.Value.ContainsValue(value); }
            else { return _localTLDictionary.Value.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <returns>对应的值</returns>
        public static T GetValue(bool place, string key)
        {
            if (place) { return _globalTLDictionary.Value[key]; }
            else { return _localTLDictionary.Value[key]; }
        }

        /// <summary>
        /// 从字典移除键值对
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        private static void Remove(bool place, string key)
        {
            if (place) { _globalTLDictionary.Value.Remove(key); }
            else { _localTLDictionary.Value.Remove(key); }
        }

        #endregion

        #region 数组模拟操作 - 清除方法

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        public static void Clear0(bool place, string key)
        {
            Remove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[], 模拟1维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        public static void Clear1(bool place, string key, int lp_1)
        {
            Remove(place, ThreadStringBuilder.Concat(key, '_', lp_1));
        }

        /// <summary>
        /// 从字典中移除Key[,], 模拟2维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        public static void Clear2(bool place, string key, int lp_1, int lp_2)
        {
            Remove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2));
        }

        /// <summary>
        /// 从字典中移除Key[,,], 模拟3维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        public static void Clear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            Remove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3));
        }

        /// <summary>
        /// 从字典中移除Key[,,,], 模拟4维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="lp_4">第四维索引</param>
        public static void Clear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            Remove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4));
        }

        #endregion

        #region 数组模拟操作 - 保存方法

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        public static void Save0(bool place, string key, T val)
        {
            Set(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对, 模拟1维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="val">值</param>
        public static void Save1(bool place, string key, int lp_1, T val)
        {
            Set(place, ThreadStringBuilder.Concat(key, '_', lp_1), val);
        }

        /// <summary>
        /// 保存字典键值对, 模拟2维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="val">值</param>
        public static void Save2(bool place, string key, int lp_1, int lp_2, T val)
        {
            Set(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2), val);
        }

        /// <summary>
        /// 保存字典键值对, 模拟3维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="val">值</param>
        public static void Save3(bool place, string key, int lp_1, int lp_2, int lp_3, T val)
        {
            Set(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3), val);
        }

        /// <summary>
        /// 保存字典键值对, 模拟4维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="lp_4">第四维索引</param>
        /// <param name="val">值</param>
        public static void Save4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, T val)
        {
            Set(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4), val);
        }

        #endregion

        #region 数组模拟操作 - 读取方法

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <returns>错误时返回default(T)</returns>
        public static T Load0(bool place, string key)
        {
            if (!KeyExists(place, key))
            {
                return default(T);
            }
            return GetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对, 模拟1维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T Load1(bool place, string key, int lp_1)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1);
            if (!KeyExists(place, fullKey))
            {
                return default(T);
            }
            return GetValue(place, fullKey);
        }

        /// <summary>
        /// 读取字典键值对, 模拟2维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T Load2(bool place, string key, int lp_1, int lp_2)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2);
            if (!KeyExists(place, fullKey))
            {
                return default(T);
            }
            return GetValue(place, fullKey);
        }

        /// <summary>
        /// 读取字典键值对, 模拟3维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T Load3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3);
            if (!KeyExists(place, fullKey))
            {
                return default(T);
            }
            return GetValue(place, fullKey);
        }

        /// <summary>
        /// 读取字典键值对, 模拟4维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="lp_4">第四维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T Load4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4);
            if (!KeyExists(place, fullKey))
            {
                return default(T);
            }
            return GetValue(place, fullKey);
        }

        #endregion

        #region 线程本地字典管理方法

        /// <summary>
        /// 清空线程本地字典
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        public static void ClearAll(bool place)
        {
            if (place)
            {
                _globalTLDictionary.Value.Clear();
            }
            else
            {
                _localTLDictionary.Value.Clear();
            }
        }

        /// <summary>
        /// 获取线程本地字典的键集合
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <returns>键集合</returns>
        public static Dictionary<string, T>.KeyCollection GetKeys(bool place)
        {
            if (place) { return _globalTLDictionary.Value.Keys; }
            else { return _localTLDictionary.Value.Keys; }
        }

        /// <summary>
        /// 获取线程本地字典的值集合
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <returns>值集合</returns>
        public static Dictionary<string, T>.ValueCollection GetValues(bool place)
        {
            if (place) { return _globalTLDictionary.Value.Values; }
            else { return _localTLDictionary.Value.Values; }
        }

        /// <summary>
        /// 获取线程本地字典的键值对集合
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <returns>键值对集合</returns>
        public static Dictionary<string, T>.Enumerator GetEnumerator(bool place)
        {
            if (place) { return _globalTLDictionary.Value.GetEnumerator(); }
            else { return _localTLDictionary.Value.GetEnumerator(); }
        }

        /// <summary>
        /// 获取线程本地字典的计数
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <returns>键值对数量</returns>
        public static int Count(bool place)
        {
            if (place) { return _globalTLDictionary.Value.Count; }
            else { return _localTLDictionary.Value.Count; }
        }

        /// <summary>
        /// 释放当前线程的本地字典(释放后再次访问会重新创建)
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        public static void Dispose(bool place)
        {
            if (place)
            {
                _globalTLDictionary.Dispose();
                _globalTLDictionary = new ThreadLocal<Dictionary<string, T>>(() => new Dictionary<string, T>());
            }
            else
            {
                _localTLDictionary.Dispose();
                _localTLDictionary = new ThreadLocal<Dictionary<string, T>>(() => new Dictionary<string, T>());
            }
        }

        #endregion

        #endregion

    }
}