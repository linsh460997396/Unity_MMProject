using System;

namespace MetalMaxSystem
{
    /// <summary>
    /// 提供高效构建字符串的方法.适用于频繁构建特定格式字符串的场景.
    /// </summary>
    public static class Combi
    {
        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator, int id)
        {
            string idStr = id.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1
            int length = key.Length + 1 + separator.Length + 1 + idStr.Length;

            return string.Create(length, (key, separator, idStr), (span, state) =>
            {
                var (p1, p2, i) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;

                // 添加分隔符
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;

                // 添加分隔符
                span[pos++] = '_';

                // 复制 id1
                i.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, int id1, string separator2, int id2)
        {
            string idStr = id1.ToString();
            string suffixStr = id2.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2
            int length = key.Length + 1 + separator1.Length + 1 + idStr.Length + 1 + separator2.Length + 1 + suffixStr.Length;

            return string.Create(length, (key, separator1, idStr, separator2, suffixStr), (span, state) =>
            {
                var (p1, p2, i, s, sf) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i.AsSpan().CopyTo(span.Slice(pos));
                pos += i.Length;
                span[pos++] = '_';

                // 复制 separator2
                s.AsSpan().CopyTo(span.Slice(pos));
                pos += s.Length;
                span[pos++] = '_';

                // 复制 id2
                sf.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, int id1, string separator2, int id2, string separator3, int id3)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3
            int length = key.Length + 1 + separator1.Length + 1 + id1Str.Length + 1 + separator2.Length + 1 + id2Str.Length + 1 + separator3.Length + 1 + id3Str.Length;

            return string.Create(length, (key, separator1, id1Str, separator2, id2Str, separator3, id3Str), (span, state) =>
            {
                var (p1, p2, i1, s1, i2, s2, i3) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i1.AsSpan().CopyTo(span.Slice(pos));
                pos += i1.Length;
                span[pos++] = '_';

                // 复制 separator2
                s1.AsSpan().CopyTo(span.Slice(pos));
                pos += s1.Length;
                span[pos++] = '_';

                // 复制 id2
                i2.AsSpan().CopyTo(span.Slice(pos));
                pos += i2.Length;
                span[pos++] = '_';

                // 复制 separator3
                s2.AsSpan().CopyTo(span.Slice(pos));
                pos += s2.Length;
                span[pos++] = '_';

                // 复制 id3
                i3.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, int id1, string separator2, int id2, string separator3, int id3, string separator4, int id4)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            string id4Str = id4.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3 + "_" + separator4 + "_" + id4
            int length = key.Length + 1 + separator1.Length + 1 + id1Str.Length + 1 + separator2.Length + 1 + id2Str.Length + 1 + separator3.Length + 1 + id3Str.Length + 1 + separator4.Length + 1 + id4Str.Length;

            return string.Create(length, (key, separator1, id1Str, separator2, id2Str, separator3, id3Str, separator4, id4Str), (span, state) =>
            {
                var (p1, p2, i1, s1, i2, s2, i3, s3, i4) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i1.AsSpan().CopyTo(span.Slice(pos));
                pos += i1.Length;
                span[pos++] = '_';

                // 复制 separator2
                s1.AsSpan().CopyTo(span.Slice(pos));
                pos += s1.Length;
                span[pos++] = '_';

                // 复制 id2
                i2.AsSpan().CopyTo(span.Slice(pos));
                pos += i2.Length;
                span[pos++] = '_';

                // 复制 separator3
                s2.AsSpan().CopyTo(span.Slice(pos));
                pos += s2.Length;
                span[pos++] = '_';

                // 复制 id3
                i3.AsSpan().CopyTo(span.Slice(pos));
                pos += i3.Length;
                span[pos++] = '_';

                // 复制 separator4
                s3.AsSpan().CopyTo(span.Slice(pos));
                pos += s3.Length;
                span[pos++] = '_';

                // 复制 id4
                i4.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator, int id)
        {
            string idStr = id.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1
            int length = key.Length + 1 + 1 + 1 + idStr.Length; // separator 现在长度为1

            return string.Create(
                length,
                (key, separator, idStr),
                (span, state) =>
                {
                    var (p1, sep, i) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;

                    // 添加分隔符
                    span[pos++] = '_';

                    // 写入 separator (char)
                    span[pos++] = sep;

                    // 添加分隔符
                    span[pos++] = '_';

                    // 复制 id1
                    i.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, int id1, char separator2, int id2)
        {
            string idStr = id1.ToString();
            string suffixStr = id2.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2
            int length = key.Length + 1 + 1 + 1 + idStr.Length + 1 + 1 + 1 + suffixStr.Length;

            return string.Create(
                length,
                (key, separator1, idStr, separator2, suffixStr),
                (span, state) =>
                {
                    var (p1, sep1, i, sep2, sf) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i.AsSpan().CopyTo(span.Slice(pos));
                    pos += i.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    sf.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, int id1, char separator2, int id2, char separator3, int id3)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3
            int length =
                key.Length
                + 1
                + 1
                + 1
                + id1Str.Length
                + 1
                + 1
                + 1
                + id2Str.Length
                + 1
                + 1
                + 1
                + id3Str.Length;

            return string.Create(
                length,
                (key, separator1, id1Str, separator2, id2Str, separator3, id3Str),
                (span, state) =>
                {
                    var (p1, sep1, i1, sep2, i2, sep3, i3) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i1.AsSpan().CopyTo(span.Slice(pos));
                    pos += i1.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    i2.AsSpan().CopyTo(span.Slice(pos));
                    pos += i2.Length;
                    span[pos++] = '_';

                    // 写入 separator3 (char)
                    span[pos++] = sep3;

                    span[pos++] = '_';

                    // 复制 id3
                    i3.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, int id1, char separator2, int id2, char separator3, int id3, char separator4, int id4)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            string id4Str = id4.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3 + "_" + separator4 + "_" + id4
            int length =
                key.Length
                + 1
                + 1
                + 1
                + id1Str.Length
                + 1
                + 1
                + 1
                + id2Str.Length
                + 1
                + 1
                + 1
                + id3Str.Length
                + 1
                + 1
                + 1
                + id4Str.Length;

            return string.Create(
                length,
                (
                    key,
                    separator1,
                    id1Str,
                    separator2,
                    id2Str,
                    separator3,
                    id3Str,
                    separator4,
                    id4Str
                ),
                (span, state) =>
                {
                    var (p1, sep1, i1, sep2, i2, sep3, i3, sep4, i4) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i1.AsSpan().CopyTo(span.Slice(pos));
                    pos += i1.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    i2.AsSpan().CopyTo(span.Slice(pos));
                    pos += i2.Length;
                    span[pos++] = '_';

                    // 写入 separator3 (char)
                    span[pos++] = sep3;

                    span[pos++] = '_';

                    // 复制 id3
                    i3.AsSpan().CopyTo(span.Slice(pos));
                    pos += i3.Length;
                    span[pos++] = '_';

                    // 写入 separator4 (char)
                    span[pos++] = sep4;

                    span[pos++] = '_';

                    // 复制 id4
                    i4.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator, char id)
        {
            string idStr = id.ToString();
            // 预计算总长度: key + "" + separator1 + "" + id1
            int length = key.Length + 1 + separator.Length + 1 + idStr.Length;
            return string.Create(length, (key, separator, idStr), (span, state) =>
            {
                var (p1, p2, i) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;

                // 添加分隔符
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;

                // 添加分隔符
                span[pos++] = '_';

                // 复制 id1
                i.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, char id1, string separator2, char id2)
        {
            string idStr = id1.ToString();
            string suffixStr = id2.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2
            int length = key.Length + 1 + separator1.Length + 1 + idStr.Length + 1 + separator2.Length + 1 + suffixStr.Length;

            return string.Create(length, (key, separator1, idStr, separator2, suffixStr), (span, state) =>
            {
                var (p1, p2, i, s, sf) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i.AsSpan().CopyTo(span.Slice(pos));
                pos += i.Length;
                span[pos++] = '_';

                // 复制 separator2
                s.AsSpan().CopyTo(span.Slice(pos));
                pos += s.Length;
                span[pos++] = '_';

                // 复制 id2
                sf.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, char id1, string separator2, char id2, string separator3, char id3)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3
            int length = key.Length + 1 + separator1.Length + 1 + id1Str.Length + 1 + separator2.Length + 1 + id2Str.Length + 1 + separator3.Length + 1 + id3Str.Length;

            return string.Create(length, (key, separator1, id1Str, separator2, id2Str, separator3, id3Str), (span, state) =>
            {
                var (p1, p2, i1, s1, i2, s2, i3) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i1.AsSpan().CopyTo(span.Slice(pos));
                pos += i1.Length;
                span[pos++] = '_';

                // 复制 separator2
                s1.AsSpan().CopyTo(span.Slice(pos));
                pos += s1.Length;
                span[pos++] = '_';

                // 复制 id2
                i2.AsSpan().CopyTo(span.Slice(pos));
                pos += i2.Length;
                span[pos++] = '_';

                // 复制 separator3
                s2.AsSpan().CopyTo(span.Slice(pos));
                pos += s2.Length;
                span[pos++] = '_';

                // 复制 id3
                i3.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, char id1, string separator2, char id2, string separator3, char id3, string separator4, char id4)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            string id4Str = id4.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3 + "_" + separator4 + "_" + id4
            int length = key.Length + 1 + separator1.Length + 1 + id1Str.Length + 1 + separator2.Length + 1 + id2Str.Length + 1 + separator3.Length + 1 + id3Str.Length + 1 + separator4.Length + 1 + id4Str.Length;

            return string.Create(length, (key, separator1, id1Str, separator2, id2Str, separator3, id3Str, separator4, id4Str), (span, state) =>
            {
                var (p1, p2, i1, s1, i2, s2, i3, s3, i4) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i1.AsSpan().CopyTo(span.Slice(pos));
                pos += i1.Length;
                span[pos++] = '_';

                // 复制 separator2
                s1.AsSpan().CopyTo(span.Slice(pos));
                pos += s1.Length;
                span[pos++] = '_';

                // 复制 id2
                i2.AsSpan().CopyTo(span.Slice(pos));
                pos += i2.Length;
                span[pos++] = '_';

                // 复制 separator3
                s2.AsSpan().CopyTo(span.Slice(pos));
                pos += s2.Length;
                span[pos++] = '_';

                // 复制 id3
                i3.AsSpan().CopyTo(span.Slice(pos));
                pos += i3.Length;
                span[pos++] = '_';

                // 复制 separator4
                s3.AsSpan().CopyTo(span.Slice(pos));
                pos += s3.Length;
                span[pos++] = '_';

                // 复制 id4
                i4.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator, char id)
        {
            string idStr = id.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1
            int length = key.Length + 1 + 1 + 1 + idStr.Length; // separator 现在长度为1

            return string.Create(
                length,
                (key, separator, idStr),
                (span, state) =>
                {
                    var (p1, sep, i) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;

                    // 添加分隔符
                    span[pos++] = '_';

                    // 写入 separator (char)
                    span[pos++] = sep;

                    // 添加分隔符
                    span[pos++] = '_';

                    // 复制 id1
                    i.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, char id1, char separator2, char id2)
        {
            string idStr = id1.ToString();
            string suffixStr = id2.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2
            int length = key.Length + 1 + 1 + 1 + idStr.Length + 1 + 1 + 1 + suffixStr.Length;

            return string.Create(
                length,
                (key, separator1, idStr, separator2, suffixStr),
                (span, state) =>
                {
                    var (p1, sep1, i, sep2, sf) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i.AsSpan().CopyTo(span.Slice(pos));
                    pos += i.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    sf.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, char id1, char separator2, char id2, char separator3, char id3)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3
            int length =
                key.Length
                + 1
                + 1
                + 1
                + id1Str.Length
                + 1
                + 1
                + 1
                + id2Str.Length
                + 1
                + 1
                + 1
                + id3Str.Length;

            return string.Create(
                length,
                (key, separator1, id1Str, separator2, id2Str, separator3, id3Str),
                (span, state) =>
                {
                    var (p1, sep1, i1, sep2, i2, sep3, i3) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i1.AsSpan().CopyTo(span.Slice(pos));
                    pos += i1.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    i2.AsSpan().CopyTo(span.Slice(pos));
                    pos += i2.Length;
                    span[pos++] = '_';

                    // 写入 separator3 (char)
                    span[pos++] = sep3;

                    span[pos++] = '_';

                    // 复制 id3
                    i3.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, char id1, char separator2, char id2, char separator3, char id3, char separator4, char id4)
        {
            string id1Str = id1.ToString();
            string id2Str = id2.ToString();
            string id3Str = id3.ToString();
            string id4Str = id4.ToString();
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3 + "_" + separator4 + "_" + id4
            int length =
                key.Length
                + 1
                + 1
                + 1
                + id1Str.Length
                + 1
                + 1
                + 1
                + id2Str.Length
                + 1
                + 1
                + 1
                + id3Str.Length
                + 1
                + 1
                + 1
                + id4Str.Length;

            return string.Create(
                length,
                (
                    key,
                    separator1,
                    id1Str,
                    separator2,
                    id2Str,
                    separator3,
                    id3Str,
                    separator4,
                    id4Str
                ),
                (span, state) =>
                {
                    var (p1, sep1, i1, sep2, i2, sep3, i3, sep4, i4) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i1.AsSpan().CopyTo(span.Slice(pos));
                    pos += i1.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    i2.AsSpan().CopyTo(span.Slice(pos));
                    pos += i2.Length;
                    span[pos++] = '_';

                    // 写入 separator3 (char)
                    span[pos++] = sep3;

                    span[pos++] = '_';

                    // 复制 id3
                    i3.AsSpan().CopyTo(span.Slice(pos));
                    pos += i3.Length;
                    span[pos++] = '_';

                    // 写入 separator4 (char)
                    span[pos++] = sep4;

                    span[pos++] = '_';

                    // 复制 id4
                    i4.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator, string id)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1
            int length = key.Length + 1 + separator.Length + 1 + id.Length;

            return string.Create(length, (key, separator, id), (span, state) =>
            {
                var (p1, p2, i) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;

                // 添加分隔符
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;

                // 添加分隔符
                span[pos++] = '_';

                // 复制 id1
                i.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, string id1, string separator2, string id2)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2
            int length = key.Length + 1 + separator1.Length + 1 + id1.Length + 1 + separator2.Length + 1 + id2.Length;

            return string.Create(length, (key, separator1, id1, separator2, id2), (span, state) =>
            {
                var (p1, p2, i, s, sf) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i.AsSpan().CopyTo(span.Slice(pos));
                pos += i.Length;
                span[pos++] = '_';

                // 复制 separator2
                s.AsSpan().CopyTo(span.Slice(pos));
                pos += s.Length;
                span[pos++] = '_';

                // 复制 id2
                sf.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, string id1, string separator2, string id2, string separator3, string id3)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3
            int length = key.Length + 1 + separator1.Length + 1 + id1.Length + 1 + separator2.Length + 1 + id2.Length + 1 + separator3.Length + 1 + id3.Length;

            return string.Create(length, (key, separator1, id1, separator2, id2, separator3, id3), (span, state) =>
            {
                var (p1, p2, i1, s1, i2, s2, i3) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i1.AsSpan().CopyTo(span.Slice(pos));
                pos += i1.Length;
                span[pos++] = '_';

                // 复制 separator2
                s1.AsSpan().CopyTo(span.Slice(pos));
                pos += s1.Length;
                span[pos++] = '_';

                // 复制 id2
                i2.AsSpan().CopyTo(span.Slice(pos));
                pos += i2.Length;
                span[pos++] = '_';

                // 复制 separator3
                s2.AsSpan().CopyTo(span.Slice(pos));
                pos += s2.Length;
                span[pos++] = '_';

                // 复制 id3
                i3.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, string separator1, string id1, string separator2, string id2, string separator3, string id3, string separator4, string id4)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3 + "_" + separator4 + "_" + id4
            int length = key.Length + 1 + separator1.Length + 1 + id1.Length + 1 + separator2.Length + 1 + id2.Length + 1 + separator3.Length + 1 + id3.Length + 1 + separator4.Length + 1 + id4.Length;

            return string.Create(length, (key, separator1, id1, separator2, id2, separator3, id3, separator4, id4), (span, state) =>
            {
                var (p1, p2, i1, s1, i2, s2, i3, s3, i4) = state;
                int pos = 0;

                // 复制 key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;
                span[pos++] = '_';

                // 复制 separator1
                p2.AsSpan().CopyTo(span.Slice(pos));
                pos += p2.Length;
                span[pos++] = '_';

                // 复制 id1
                i1.AsSpan().CopyTo(span.Slice(pos));
                pos += i1.Length;
                span[pos++] = '_';

                // 复制 separator2
                s1.AsSpan().CopyTo(span.Slice(pos));
                pos += s1.Length;
                span[pos++] = '_';

                // 复制 id2
                i2.AsSpan().CopyTo(span.Slice(pos));
                pos += i2.Length;
                span[pos++] = '_';

                // 复制 separator3
                s2.AsSpan().CopyTo(span.Slice(pos));
                pos += s2.Length;
                span[pos++] = '_';

                // 复制 id3
                i3.AsSpan().CopyTo(span.Slice(pos));
                pos += i3.Length;
                span[pos++] = '_';

                // 复制 separator4
                s3.AsSpan().CopyTo(span.Slice(pos));
                pos += s3.Length;
                span[pos++] = '_';

                // 复制 id4
                i4.AsSpan().CopyTo(span.Slice(pos));
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator, string id)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1
            int length = key.Length + 1 + 1 + 1 + id.Length; // separator 现在长度为1

            return string.Create(
                length,
                (key, separator, id),
                (span, state) =>
                {
                    var (p1, sep, i) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;

                    // 添加分隔符
                    span[pos++] = '_';

                    // 写入 separator (char)
                    span[pos++] = sep;

                    // 添加分隔符
                    span[pos++] = '_';

                    // 复制 id1
                    i.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, string id1, char separator2, string id2)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2
            int length = key.Length + 1 + 1 + 1 + id1.Length + 1 + 1 + 1 + id2.Length;

            return string.Create(
                length,
                (key, separator1, id1, separator2, id2),
                (span, state) =>
                {
                    var (p1, sep1, i, sep2, sf) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i.AsSpan().CopyTo(span.Slice(pos));
                    pos += i.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    sf.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, string id1, char separator2, string id2, char separator3, string id3)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3
            int length =
                key.Length
                + 1
                + 1
                + 1
                + id1.Length
                + 1
                + 1
                + 1
                + id2.Length
                + 1
                + 1
                + 1
                + id3.Length;

            return string.Create(
                length,
                (key, separator1, id1, separator2, id2, separator3, id3),
                (span, state) =>
                {
                    var (p1, sep1, i1, sep2, i2, sep3, i3) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i1.AsSpan().CopyTo(span.Slice(pos));
                    pos += i1.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    i2.AsSpan().CopyTo(span.Slice(pos));
                    pos += i2.Length;
                    span[pos++] = '_';

                    // 写入 separator3 (char)
                    span[pos++] = sep3;

                    span[pos++] = '_';

                    // 复制 id3
                    i3.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildStringEfficiently(string key, char separator1, string id1, char separator2, string id2, char separator3, string id3, char separator4, string id4)
        {
            // 预计算总长度: key + "_" + separator1 + "_" + id1 + "_" + separator2 + "_" + id2 + "_" + separator3 + "_" + id3 + "_" + separator4 + "_" + id4
            int length =
                key.Length
                + 1
                + 1
                + 1
                + id1.Length
                + 1
                + 1
                + 1
                + id2.Length
                + 1
                + 1
                + 1
                + id3.Length
                + 1
                + 1
                + 1
                + id4.Length;

            return string.Create(
                length,
                (
                    key,
                    separator1,
                    id1,
                    separator2,
                    id2,
                    separator3,
                    id3,
                    separator4,
                    id4
                ),
                (span, state) =>
                {
                    var (p1, sep1, i1, sep2, i2, sep3, i3, sep4, i4) = state;
                    int pos = 0;

                    // 复制 key
                    p1.AsSpan().CopyTo(span);
                    pos += p1.Length;
                    span[pos++] = '_';

                    // 写入 separator1 (char)
                    span[pos++] = sep1;

                    span[pos++] = '_';

                    // 复制 id1
                    i1.AsSpan().CopyTo(span.Slice(pos));
                    pos += i1.Length;
                    span[pos++] = '_';

                    // 写入 separator2 (char)
                    span[pos++] = sep2;

                    span[pos++] = '_';

                    // 复制 id2
                    i2.AsSpan().CopyTo(span.Slice(pos));
                    pos += i2.Length;
                    span[pos++] = '_';

                    // 写入 separator3 (char)
                    span[pos++] = sep3;

                    span[pos++] = '_';

                    // 复制 id3
                    i3.AsSpan().CopyTo(span.Slice(pos));
                    pos += i3.Length;
                    span[pos++] = '_';

                    // 写入 separator4 (char)
                    span[pos++] = sep4;

                    span[pos++] = '_';

                    // 复制 id4
                    i4.AsSpan().CopyTo(span.Slice(pos));
                }
            );
        }

    }
}

