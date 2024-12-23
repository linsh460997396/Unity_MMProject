using System;
using System.Text;

namespace MMWorld
{
    public static class NumConverter
    {
        //注意StringBuilder跨线程使用不安全
        public static StringBuilder ToStringResult = new();

        /// <summary>
        /// 以10^4为量级的中文符号（往后没有依据请自行发明）
        /// </summary>
        public static string[] NumNames = new string[] {
        "",
        "万",
        "亿",
        "兆",
        "京",
        "垓",
        "秭",
        "穰",
        "沟",
        "涧",
        "正",
        "载",
        "极",
        "恒",
        "浩",
        "瀚",
        "宇",
        "宙",
        "洪",
        "荒",
        "渺",
        "玄",
        // ... 继续
    };

        /// <summary>
        /// 将浮点数取整后转出20K这样的字符串并存储到StringBuilder对象中
        /// </summary>
        /// <param name="d">浮点数d必须>=0</param>
        /// <param name="o">接收结果用的StringBuilder对象</param>
        public static void ToStringEN(double d, ref StringBuilder o)
        {
            o.Clear();
            if (d < 1)
            {
                o.Append('0');
                return;
            }
            var e = (int)Math.Log10(d);
            if (e < 3)
            {
                o.Length = e + 1;
                var n = (int)d;
                while (n >= 10)
                {
                    var a = n / 10;
                    var b = n - a * 10;
                    o[e--] = (char)(b + 48);
                    n = a;
                }
                o[0] = (char)(n + 48);//比如数字2对应(char)50
            }
            else
            {
                var idx = e / 3;
                d /= Math.Pow(10, idx * 3);
                e = e - idx * 3;
                o.Length = e + 1;
                var n = (int)d;
                var bak = n;
                while (n >= 10)
                {
                    var a = n / 10;
                    var b = n - a * 10;
                    o[e--] = (char)(b + 48);
                    n = a;
                }
                o[0] = (char)(n + 48);
                if (d > bak)
                {
                    var first = (int)((d - bak) * 10);
                    if (first > 0)
                    {
                        o.Append('.');
                        o.Append((char)(first + 48));
                    }
                }
                if (idx < 10)
                {
                    o.Append(" KMGTPEZYB"[idx]);
                }
                else
                {
                    o.Append("e");
                    o.Append(idx * 3);
                }
            }
        }
        /// <summary>
        /// 将浮点数取整后转出20K这样的字符串并存储到StringBuilder对象中
        /// </summary>
        /// <param name="d">浮点数d必须>=0</param>
        /// <param name="o">接收结果用的StringBuilder对象</param>
        public static StringBuilder ToStringEN(double d)
        {
            ToStringEN(d, ref ToStringResult);
            return ToStringResult;
        }

        /// <summary>
        /// 将浮点数取整后转出20万这样的字符串并存储到StringBuilder对象中
        /// </summary>
        /// <param name="d">浮点数d必须>=0</param>
        /// <param name="o">接收结果用的StringBuilder对象</param>
        public static void ToStringCN(double d, ref StringBuilder o)
        {
            o.Clear();
            if (d < 1)
            {
                o.Append('0');
                return;
            }
            var e = (int)Math.Log10(d);
            if (e < 4)
            {
                o.Length = e + 1;
                var n = (int)d;
                while (n >= 10)
                {
                    var a = n / 10;
                    var b = n - a * 10;
                    o[e--] = (char)(b + 48);
                    n = a;
                }
                o[0] = (char)(n + 48);
            }
            else
            {
                var idx = e / 4;
                d /= Math.Pow(10, idx * 4);
                e = e - idx * 4;
                o.Length = e + 1;
                var n = (int)d;
                var bak = n;
                while (n >= 10)
                {
                    var a = n / 10;
                    var b = n - a * 10;
                    o[e--] = (char)(b + 48);
                    n = a;
                }
                o[0] = (char)(n + 48);
                if (d > bak)
                {
                    var first = (int)((d - bak) * 10);
                    if (first > 0)
                    {
                        o.Append('.');
                        o.Append((char)(first + 48));
                    }
                }
                if (idx < NumNames.Length)
                {
                    o.Append(NumNames[idx]);
                }
                else
                {
                    o.Append("e");
                    o.Append(idx * 4);
                }
            }
        }
        /// <summary>
        /// 将浮点数取整后转出20万这样的字符串并存储到StringBuilder对象中
        /// </summary>
        /// <param name="d">浮点数d必须>=0</param>
        /// <param name="o">接收结果用的StringBuilder对象</param>
        public static StringBuilder ToStringCN(double d)
        {
            ToStringCN(d, ref ToStringResult);
            return ToStringResult;
        }

    }
}