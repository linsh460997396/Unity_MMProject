using System;
using System.Text;

namespace SimWorld
{
    public static partial class SWHelpers
    {

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
        "恒河沙",
        "阿僧祇",
        "那由他",
        "不可思议",
        "无量",
        "大数",
        // ... 继续造？
    };

        // condition: d >= 0
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

        // condition: d >= 0
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
                o[0] = (char)(n + 48);
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


        // not thread safe
        public static StringBuilder ToStringResult = new();

        public static StringBuilder ToStringEN(double d)
        {
            ToStringEN(d, ref ToStringResult);
            return ToStringResult;
        }

        public static StringBuilder ToStringCN(double d)
        {
            ToStringCN(d, ref ToStringResult);
            return ToStringResult;
        }

    }
}