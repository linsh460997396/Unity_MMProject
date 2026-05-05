using System.Collections.Generic;

namespace MetalMaxSystem
{
    /// <summary>
    /// 普通字典封装的数据表(单线程读写,跨线程仅只读安全).
    /// 效率：字典 > 哈希表 >> 字典.ToString() > 跨线程字典
    /// </summary>
    /// <typeparam name="T">字典中存储的值的类型</typeparam>
    public static class DataTable<T>
    {
        //根据T不同,每次调用DataTable<T>都会创建一个新的静态类实例,因此每个T都有独立的字典存储空间.

        private static Dictionary<string, T> _globalDictionary = new Dictionary<string, T>();
        /// <summary>
        /// 全局字典<string, T> (不排泄,直到程序结束)
        /// </summary>
        public static Dictionary<string, T> GlobalDictionary
        {
            get { return _globalDictionary; }
            set { _globalDictionary = value; }
        }

        private static Dictionary<string, T> _localDictionary = new Dictionary<string, T>();
        /// <summary>
        /// 临时字典<string, T> (函数或动作集结束时应手动排泄)
        /// </summary>
        public static Dictionary<string, T> LocalDictionary
        {
            get { return _localDictionary; }
            set { _localDictionary = value; }
        }

        #region 普通字典操作方法

        #region 基础方法 - 添加/获取/移除

        /// <summary>
        /// 添加字典键值对(重复添加则覆盖)
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        private static void DictionarySet(bool place, string key, T value)
        {
            if (place)
            {
                // 存入全局字典
                _globalDictionary[key] = value;
            }
            else
            {
                // 存入局部字典
                _localDictionary[key] = value;
            }
        }

        /// <summary>
        /// 判断字典键是否存在
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        public static bool DictionaryKeyExists(bool place, string key)
        {
            if (place) { return _globalDictionary.ContainsKey(key); }
            else { return _localDictionary.ContainsKey(key); }
        }

        /// <summary>
        /// 判断字典值是否存在
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="value">值</param>
        /// <returns>是否存在</returns>
        public static bool DictionaryValueExists(bool place, T value)
        {
            if (place) { return _globalDictionary.ContainsValue(value); }
            else { return _localDictionary.ContainsValue(value); }
        }

        /// <summary>
        /// 获取字典键对应的值
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <returns>对应的值</returns>
        public static T DictionaryGetValue(bool place, string key)
        {
            if (place) { return _globalDictionary[key]; }
            else { return _localDictionary[key]; }
        }

        /// <summary>
        /// 移除字典键值对
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        private static void DictionaryRemove(bool place, string key)
        {
            if (place) { _globalDictionary.Remove(key); }
            else { _localDictionary.Remove(key); }
        }

        #endregion

        #region 数组模拟操作 - 清除方法

        /// <summary>
        /// 从字典中移除Key
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        public static void DictionaryClear0(bool place, string key)
        {
            DictionaryRemove(place, key);
        }

        /// <summary>
        /// 从字典中移除Key[], 模拟1维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        public static void DictionaryClear1(bool place, string key, int lp_1)
        {
            DictionaryRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1));
        }

        /// <summary>
        /// 从字典中移除Key[,], 模拟2维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        public static void DictionaryClear2(bool place, string key, int lp_1, int lp_2)
        {
            DictionaryRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2));
        }

        /// <summary>
        /// 从字典中移除Key[,,], 模拟3维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="lp_3">第三维索引</param>
        public static void DictionaryClear3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            DictionaryRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3));
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
        public static void DictionaryClear4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            DictionaryRemove(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4));
        }

        #endregion

        #region 数组模拟操作 - 保存方法

        /// <summary>
        /// 保存字典键值对
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        public static void DictionarySave0(bool place, string key, T val)
        {
            DictionarySet(place, key, val);
        }

        /// <summary>
        /// 保存字典键值对, 模拟1维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="val">值</param>
        public static void DictionarySave1(bool place, string key, int lp_1, T val)
        {
            DictionarySet(place, ThreadStringBuilder.Concat(key, '_', lp_1), val);
        }

        /// <summary>
        /// 保存字典键值对, 模拟2维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <param name="val">值</param>
        public static void DictionarySave2(bool place, string key, int lp_1, int lp_2, T val)
        {
            DictionarySet(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2), val);
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
        public static void DictionarySave3(bool place, string key, int lp_1, int lp_2, int lp_3, T val)
        {
            DictionarySet(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3), val);
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
        public static void DictionarySave4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4, T val)
        {
            DictionarySet(place, ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4), val);
        }

        #endregion

        #region 数组模拟操作 - 读取方法

        /// <summary>
        /// 读取字典键值对
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键</param>
        /// <returns>错误时返回default(T)</returns>
        public static T DictionaryLoad0(bool place, string key)
        {
            if (!DictionaryKeyExists(place, key))
            {
                return default(T);
            }
            return DictionaryGetValue(place, key);
        }

        /// <summary>
        /// 读取字典键值对, 模拟1维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T DictionaryLoad1(bool place, string key, int lp_1)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1);
            if (!DictionaryKeyExists(place, fullKey))
            {
                return default(T);
            }
            return DictionaryGetValue(place, fullKey);
        }

        /// <summary>
        /// 读取字典键值对, 模拟2维数组
        /// </summary>
        /// <param name="place">true=全局, false=局部</param>
        /// <param name="key">键前缀</param>
        /// <param name="lp_1">第一维索引</param>
        /// <param name="lp_2">第二维索引</param>
        /// <returns>错误时返回default(T)</returns>
        public static T DictionaryLoad2(bool place, string key, int lp_1, int lp_2)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2);
            if (!DictionaryKeyExists(place, fullKey))
            {
                return default(T);
            }
            return DictionaryGetValue(place, fullKey);
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
        public static T DictionaryLoad3(bool place, string key, int lp_1, int lp_2, int lp_3)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3);
            if (!DictionaryKeyExists(place, fullKey))
            {
                return default(T);
            }
            return DictionaryGetValue(place, fullKey);
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
        public static T DictionaryLoad4(bool place, string key, int lp_1, int lp_2, int lp_3, int lp_4)
        {
            string fullKey = ThreadStringBuilder.Concat(key, '_', lp_1, '_', lp_2, '_', lp_3, '_', lp_4);
            if (!DictionaryKeyExists(place, fullKey))
            {
                return default(T);
            }
            return DictionaryGetValue(place, fullKey);
        }

        #endregion

        #endregion

    }
}
