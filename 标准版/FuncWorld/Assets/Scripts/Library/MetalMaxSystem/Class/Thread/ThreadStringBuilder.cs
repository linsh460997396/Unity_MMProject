using System.Text;
using System.Threading;

namespace MetalMaxSystem
{
    /// <summary>
    /// 线程安全的StringBuilder复用工具类.
    /// FastSpan.BuildString对于堆栈上值类型拼接非常高效,纯字符串拼接请用string.Concat,但对于混合值、引用类型的复杂格式拼接请用ThreadStringBuilder.
    /// </summary>
    public static class ThreadStringBuilder
    {
        // 定义 ThreadLocal 变量
        // 这里的 () => new StringBuilder(256) 是工厂函数,仅在首次访问时执行
        private static ThreadLocal<StringBuilder> _threadLocalBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder(256));

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
        /// 组合string + [char + int]序列(4层).
        /// 例如:key + '_' + index1 + '_' + index2 + '_' + index3 + '_' + index4
        /// </summary>
        public static string Concat(string str1, char sep1, int val1, char sep2, int val2, char sep3, int val3, char sep4, int val4)
        {
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
        /// 组合string + [char + int]序列(3层).
        /// 例如:key + '_' + index1 + '_' + index2 + '_' + index3
        /// </summary>
        public static string Concat(string str1, char sep1, int val1, char sep2, int val2, char sep3, int val3)
        {
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
        /// 组合string + [char + int]序列(2层).
        /// 例如:key + '_' + index1 + '_' + index2
        /// </summary>
        public static string Concat(string str1, char sep1, int val1, char sep2, int val2)
        {
            var sb = Get();
            sb.Append(str1);
            sb.Append(sep1);
            sb.Append(val1);
            sb.Append(sep2);
            sb.Append(val2);
            return sb.ToString();
        }
        /// <summary>
        /// 组合string + char + int.
        /// 例如:key + '_' + index1
        /// </summary>
        public static string Concat(string str1, char sep1, int val1)
        {
            var sb = Get();
            sb.Append(str1);
            sb.Append(sep1);
            sb.Append(val1);
            return sb.ToString();
        }

        //按需追加重载↓

        /// <summary>
        /// 组合string + int + string + int + string[].
        /// </summary>
        public static string Concat(string str1, int val1, string str2, int val2, params string[] strs)
        {
            var sb = Get();
            sb.Append(str1);
            sb.Append(val1);
            sb.Append(str2);
            sb.Append(val2);
            foreach (var s in strs)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 组合string + string + int[].
        /// </summary>
        public static string Concat(string str1, string str2, params int[] vals)
        {
            var sb = Get();
            sb.Append(str1);
            sb.Append(str2);
            foreach (var v in vals)
            {
                sb.Append(v);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 组合string + int + char + int[].
        /// </summary>
        public static string Concat(string str, int val1, char sep1, params int[] vals)
        {
            var sb = Get();
            sb.Append(str);
            sb.Append(val1);
            sb.Append(sep1);
            foreach (var v in vals)
            {
                sb.Append(v);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 组合string + int + string[].
        /// </summary>
        public static string Concat(string str1, int val1, params string[] strs)
        {
            var sb = Get();
            sb.Append(str1);
            sb.Append(val1);
            foreach (var s in strs)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 组合string + int[].
        /// </summary>
        public static string Concat(string str, params int[] vals)
        {
            var sb = Get();
            sb.Append(str);
            foreach (var v in vals)
            {
                sb.Append(v);
            }
            return sb.ToString();
        }
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
