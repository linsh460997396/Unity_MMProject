using System.Text;
using System.Threading;

namespace MetalMaxSystem
{
    public static class ThreadStringBuilder
    {
        private static bool _concatType = false;
        /// <summary>
        /// 组合类型.用于Concat方法切换,默认false采用sb.Append(),为true时切换为string.Concat().
        /// </summary>
        public static bool ConcatType
        {
            get
            {
                return _concatType;
            }
            set
            {
                _concatType = value;
            }
        }

        // 定义 ThreadLocal 变量
        // 这里的 () => new StringBuilder(256) 是工厂函数,仅在首次访问时执行
        private static ThreadLocal<StringBuilder> _threadLocalBuilder =
            new ThreadLocal<StringBuilder>(() => new StringBuilder(256));

        // 获取当前线程复用的 StringBuilder
        public static StringBuilder Get()
        {
            // .Value 属性会自动检测:
            // - 如果当前线程没有实例 -> 创建并缓存
            // - 如果当前线程已有实例 -> 直接返回
            var sb = _threadLocalBuilder.Value;

            // 重要:使用前建议 Clear,确保没有残留数据
            sb.Clear();
            return sb;
        }

        // 程序退出时释放资源
        public static void Dispose()
        {
            _threadLocalBuilder.Dispose();
        }

        /// <summary>
        /// 针对常见场景的优化重载:string + char + int序列.
        /// 例如:key + '_' + index1 + '_' + index2 + '_' + index3 + '_' + index4
        /// </summary>
        public static string Concat(string str1, char sep1, int val1, char sep2, int val2, char sep3, int val3, char sep4, int val4)
        {
            if (ConcatType)
            {
                return string.Concat(str1, sep1, val1, sep2, val2, sep3, val3, sep4, val4);
            }
            var sb = Get();
            sb.Append(str1);
            sb.Append(sep1);
            sb.Append(val1);
            sb.Append(sep2);
            sb.Append(val2);
            sb.Append(sep3);
            sb.Append(val3);
            sb.Append(sep4);
            sb.Append(val4);
            return sb.ToString();
        }
        /// <summary>
        /// 针对常见场景的优化重载:string + char + int序列.
        /// 例如:key + '_' + index1 + '_' + index2 + '_' + index3
        /// </summary>
        public static string Concat(string str1, char sep1, int val1, char sep2, int val2, char sep3, int val3)
        {
            if (ConcatType)
            {
                return string.Concat(str1, sep1, val1, sep2, val2, sep3, val3);
            }
            var sb = Get();
            sb.Append(str1);
            sb.Append(sep1);
            sb.Append(val1);
            sb.Append(sep2);
            sb.Append(val2);
            sb.Append(sep3);
            sb.Append(val3);
            return sb.ToString();
        }
        /// <summary>
        /// 针对常见场景的优化重载:string + char + int序列.
        /// 例如:key + '_' + index1 + '_' + index2
        /// </summary>
        public static string Concat(string str1, char sep1, int val1, char sep2, int val2)
        {
            if (ConcatType)
            {
                return string.Concat(str1, sep1, val1, sep2, val2);
            }
            var sb = Get();
            sb.Append(str1);
            sb.Append(sep1);
            sb.Append(val1);
            sb.Append(sep2);
            sb.Append(val2);
            return sb.ToString();
        }
        /// <summary>
        /// 针对常见场景的优化重载:string + char + int序列.
        /// 例如:key + '_' + index1
        /// </summary>
        public static string Concat(string str1, char sep1, int val1)
        {
            if (ConcatType)
            {
                return string.Concat(str1, sep1, val1);
            }
            var sb = Get();
            sb.Append(str1);
            sb.Append(sep1);
            sb.Append(val1);
            return sb.ToString();
        }

        //按需追加重载↓

    }
}

// 使用示例
//class Program
//{
//    static void Main()
//    {
//        // 线程 1 使用
//        Thread t1 = new Thread(() =>
//        {
//            var sb = ThreadStringBuilder.Get();
//            sb.Append("Hello from Thread 1");
//            MMCor.Tell(sb.ToString());
//        });

//        // 线程 2 使用
//        Thread t2 = new Thread(() =>
//        {
//            var sb = ThreadStringBuilder.Get();
//            sb.Append("Hello from Thread 2");
//            MMCor.Tell(sb.ToString());
//        });

//        t1.Start();
//        t2.Start();
//        t1.Join();
//        t2.Join();
//    }
//}
