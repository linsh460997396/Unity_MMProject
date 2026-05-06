using System.Collections.Concurrent;

namespace MetalMaxSystem
{
    /// <summary>
    /// 普通字典封装的数据表(单线程读写,跨线程仅只读安全).
    /// 效率:字典 > 哈希表 >> 字典.ToString() > 跨线程字典
    /// </summary>
    /// <typeparam name="T">字典中存储的值的类型</typeparam>
    public static class DataTableConcurrent<T>
    {
        //根据T不同,每次调用DataTableConcurrent<T>都会创建一个新的静态类实例,因此每个T都有独立的字典存储空间.

        private static ConcurrentDictionary<string, T> _globalCDictionary = new ConcurrentDictionary<string, T>();
        /// <summary>
        /// 全局跨线程字典<string, T> (不排泄,直到程序结束)
        /// </summary>
        public static ConcurrentDictionary<string, T> GlobalCDictionary
        {
            get { return _globalCDictionary; }
            set { _globalCDictionary = value; }
        }

        private static ConcurrentDictionary<string, T> _localCDictionary = new ConcurrentDictionary<string, T>();
        /// <summary>
        /// 临时跨线程字典<string, T> (函数或动作集结束时应手动排泄)
        /// </summary>
        public static ConcurrentDictionary<string, T> LocalCDictionary
        {
            get { return _localCDictionary; }
            set { _localCDictionary = value; }
        }

        #region 跨线程字典操作方法

        #region 基础方法 - 添加/获取/移除

        /// <summary>
        /// 向跨线程字典添加或更新值
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        private static void ConcurrentDictionarySet(bool isGlobal, string key, T value)
        {
            if (isGlobal)
            {
                _globalCDictionary.AddOrUpdate(key, value, (k, oldValue) => value);
            }
            else
            {
                _localCDictionary.AddOrUpdate(key, value, (k, oldValue) => value);
            }
        }

        /// <summary>
        /// 判断跨线程字典键是否存在
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        public static bool ConcurrentDictionaryKeyExists(bool isGlobal, string key)
        {
            if (isGlobal) { return _globalCDictionary.ContainsKey(key); }
            else { return _localCDictionary.ContainsKey(key); }
        }

        /// <summary>
        /// 判断跨线程字典值是否存在
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="value">值</param>
        /// <returns>是否存在</returns>
        public static bool ConcurrentDictionaryValueExists(bool isGlobal, T value)
        {
            if (isGlobal) { return _globalCDictionary.Values.Contains(value); }
            else { return _localCDictionary.Values.Contains(value); }
        }

        /// <summary>
        /// 获取跨线程字典键对应的值
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键</param>
        /// <returns>对应的值</returns>
        public static T ConcurrentDictionaryGetValue(bool isGlobal, string key)
        {
            if (isGlobal) { return _globalCDictionary[key]; }
            else { return _localCDictionary[key]; }
        }

        /// <summary>
        /// 从跨线程字典移除键值对
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键</param>
        /// <returns>是否成功移除</returns>
        private static bool ConcurrentDictionaryRemove(bool isGlobal, string key)
        {
            T removedValue;
            if (isGlobal) { return _globalCDictionary.TryRemove(key, out removedValue); }
            else { return _localCDictionary.TryRemove(key, out removedValue); }
        }

        #endregion

        #region 数组模拟操作 - 清除方法

        /// <summary>
        /// 从跨线程字典中移除Key
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键</param>
        public static void ConcurrentDictionaryClear0(bool isGlobal, string key)
        {
            ConcurrentDictionaryRemove(isGlobal, key);
        }

        /// <summary>
        /// 从跨线程字典中移除Key[], 模拟1维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        public static void ConcurrentDictionaryClear1(bool isGlobal, string key, int lp_1)
        {
            ConcurrentDictionaryRemove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1));
        }

        /// <summary>
        /// 从跨线程字典中移除Key[,], 模拟2维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        public static void ConcurrentDictionaryClear2(bool isGlobal, string key, int lp_1, int lp_2)
        {
            ConcurrentDictionaryRemove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2));
        }

        /// <summary>
        /// 从跨线程字典中移除Key[,,], 模拟3维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        public static void ConcurrentDictionaryClear3(bool isGlobal, string key, int lp_1, int lp_2, int lp_3)
        {
            ConcurrentDictionaryRemove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3));
        }

        /// <summary>
        /// 从跨线程字典中移除Key[,,,], 模拟4维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="lp_4">第四维索引</param>
        public static void ConcurrentDictionaryClear4(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            ConcurrentDictionaryRemove(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4));
        }


        #endregion

        #region 数组模拟操作 - 保存方法

        /// <summary>
        /// 保存跨线程字典键值对
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        public static void ConcurrentDictionarySave0(bool isGlobal, string key, T val)
        {
            ConcurrentDictionarySet(isGlobal, key, val);
        }

        /// <summary>
        /// 保存跨线程字典键值对, 模拟1维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="val">值</param>
        public static void ConcurrentDictionarySave1(bool isGlobal, string key, int lp_1, T val)
        {
            ConcurrentDictionarySet(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1), val);
        }

        /// <summary>
        /// 保存跨线程字典键值对, 模拟2维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="val">值</param>
        public static void ConcurrentDictionarySave2(bool isGlobal, string key, int lp_1, int lp_2, T val)
        {
            ConcurrentDictionarySet(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2), val);
        }

        /// <summary>
        /// 保存跨线程字典键值对, 模拟3维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="val">值</param>
        public static void ConcurrentDictionarySave3(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, T val)
        {
            ConcurrentDictionarySet(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3), val);
        }

        /// <summary>
        /// 保存跨线程字典键值对, 模拟4维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="lp_4">第四维索引</param>
        /// <param name="val">值</param>
        public static void ConcurrentDictionarySave4(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, int lp_4, T val)
        {
            ConcurrentDictionarySet(isGlobal, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4), val);
        }


        #endregion

        #region 数组模拟操作 - 读取方法

        /// <summary>
        /// 读取跨线程字典键值对
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键</param>
        /// <returns>错误时返回default(T)</returns>
        public static T ConcurrentDictionaryLoad0(bool isGlobal, string key)
        {
            if (!ConcurrentDictionaryKeyExists(isGlobal, key))
            {
                return default(T);
            }
            return ConcurrentDictionaryGetValue(isGlobal, key);
        }

        /// <summary>
        /// 读取跨线程字典键值对, 模拟1维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T ConcurrentDictionaryLoad1(bool isGlobal, string key, int lp_1)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1);
            if (!ConcurrentDictionaryKeyExists(isGlobal, fullKey))
            {
                return default(T);
            }
            return ConcurrentDictionaryGetValue(isGlobal, fullKey);
        }

        /// <summary>
        /// 读取跨线程字典键值对, 模拟2维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T ConcurrentDictionaryLoad2(bool isGlobal, string key, int lp_1, int lp_2)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2);
            if (!ConcurrentDictionaryKeyExists(isGlobal, fullKey))
            {
                return default(T);
            }
            return ConcurrentDictionaryGetValue(isGlobal, fullKey);
        }

        /// <summary>
        /// 读取跨线程字典键值对, 模拟3维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T ConcurrentDictionaryLoad3(bool isGlobal, string key, int lp_1, int lp_2, int lp_3)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3);
            if (!ConcurrentDictionaryKeyExists(isGlobal, fullKey))
            {
                return default(T);
            }
            return ConcurrentDictionaryGetValue(isGlobal, fullKey);
        }

        /// <summary>
        /// 读取跨线程字典键值对, 模拟4维数组
        /// </summary>
        /// <param name="isGlobal">true=全局跨线程字典, false=临时跨线程字典</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        /// <param name="lp_4">第四维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T ConcurrentDictionaryLoad4(bool isGlobal, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4);
            if (!ConcurrentDictionaryKeyExists(isGlobal, fullKey))
            {
                return default(T);
            }
            return ConcurrentDictionaryGetValue(isGlobal, fullKey);
        }


        #endregion 

        #endregion 

    }
}
