#if UNITY_EDITOR || UNITY_STANDALONE || NET6_0_OR_GREATER
using System;

namespace MetalMaxSystem
{
    /// <summary>
    /// 用Span方式高效构建字符串.适用于频繁构建特定格式字符串的场景.
    /// FastSpan.BuildString对于堆栈上值类型拼接非常高效,纯字符串拼接请用string.Concat,但对于混合值、引用类型的复杂格式拼接请用ThreadStringBuilder.
    /// </summary>
    public static class FastSpan
    {
        //常用的重载请在这里添加

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildString(string key, char separator, int id)
        {
            // 预计算id字符长度（支持负数）
            int idLen = id < 0 ? (int)Math.Floor(Math.Log10(-id)) + 2 : (id == 0 ? 1 : (int)Math.Floor(Math.Log10(id)) + 1);
            int length = key.Length + 1 + idLen; // key + '_' + id

            return string.Create(length, (key, separator, id, idLen), (span, state) =>
            {
                var (p1, sep, num, len) = state;
                int pos = 0;

                // 复制key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;

                // 直接写入separator
                span[pos++] = sep;

                // 将int直接写入span尾部（从后向前填充）
                int endPos = pos + len;
                int temp = num;
                bool negative = false;
                if (temp < 0)
                {
                    negative = true;
                    temp = -temp;
                }

                // 从后向前填充数字
                for (int i = endPos - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp % 10));
                    temp /= 10;
                }

                if (negative) span[pos] = '-';
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildString(string key, char separator1, int id1, char separator2, int id2)
        {
            // 预计算各id的字符长度
            int id1Len = id1 < 0 ? (int)Math.Floor(Math.Log10(-id1)) + 2 : (id1 == 0 ? 1 : (int)Math.Floor(Math.Log10(id1)) + 1);
            int id2Len = id2 < 0 ? (int)Math.Floor(Math.Log10(-id2)) + 2 : (id2 == 0 ? 1 : (int)Math.Floor(Math.Log10(id2)) + 1);

            // 总长度: key + '_' + id1 + '_' + id2
            int length = key.Length + 1 + id1Len + 1 + id2Len;

            return string.Create(length, (key, separator1, id1, id1Len, separator2, id2, id2Len), (span, state) =>
            {
                var (p1, sep1, num1, len1, sep2, num2, len2) = state;
                int pos = 0;

                // 复制key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;

                // 直接写入separator
                span[pos++] = sep1;
                // 将id1直接写入span
                int endPos1 = pos + len1;
                int temp1 = num1;
                bool negative1 = false;
                if (temp1 < 0)
                {
                    negative1 = true;
                    temp1 = -temp1;
                }
                for (int i = endPos1 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp1 % 10));
                    temp1 /= 10;
                }
                if (negative1) span[pos] = '-';
                pos = endPos1;

                // 直接写入separator
                span[pos++] = sep2;
                // 将id2直接写入span
                int endPos2 = pos + len2;
                int temp2 = num2;
                bool negative2 = false;
                if (temp2 < 0)
                {
                    negative2 = true;
                    temp2 = -temp2;
                }
                for (int i = endPos2 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp2 % 10));
                    temp2 /= 10;
                }
                if (negative2) span[pos] = '-';
            });
        }


        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildString(string key, char separator1, int id1, char separator2, int id2, char separator3, int id3)
        {
            // 预计算各id的字符长度
            int id1Len = id1 < 0 ? (int)Math.Floor(Math.Log10(-id1)) + 2 : (id1 == 0 ? 1 : (int)Math.Floor(Math.Log10(id1)) + 1);
            int id2Len = id2 < 0 ? (int)Math.Floor(Math.Log10(-id2)) + 2 : (id2 == 0 ? 1 : (int)Math.Floor(Math.Log10(id2)) + 1);
            int id3Len = id3 < 0 ? (int)Math.Floor(Math.Log10(-id3)) + 2 : (id3 == 0 ? 1 : (int)Math.Floor(Math.Log10(id3)) + 1);

            // 总长度: key + '_' + id1 + '_' + id2 + '_' + id3
            int length = key.Length + 1 + id1Len + 1 + id2Len + 1 + id3Len;

            return string.Create(length, (key, separator1, id1, id1Len, separator2, id2, id2Len, separator3, id3, id3Len), (span, state) =>
            {
                var (p1, sep1, num1, len1, sep2, num2, len2, sep3, num3, len3) = state;
                int pos = 0;

                // 复制key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;

                // 直接写入separator
                span[pos++] = sep1;
                // 将id1直接写入span
                int endPos1 = pos + len1;
                int temp1 = num1;
                bool negative1 = false;
                if (temp1 < 0)
                {
                    negative1 = true;
                    temp1 = -temp1;
                }
                for (int i = endPos1 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp1 % 10));
                    temp1 /= 10;
                }
                if (negative1) span[pos] = '-';
                pos = endPos1;

                // 直接写入separator
                span[pos++] = sep2;
                // 将id2直接写入span
                int endPos2 = pos + len2;
                int temp2 = num2;
                bool negative2 = false;
                if (temp2 < 0)
                {
                    negative2 = true;
                    temp2 = -temp2;
                }
                for (int i = endPos2 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp2 % 10));
                    temp2 /= 10;
                }
                if (negative2) span[pos] = '-';
                pos = endPos2;

                // 直接写入separator
                span[pos++] = sep3;
                // 将id3直接写入span
                int endPos3 = pos + len3;
                int temp3 = num3;
                bool negative3 = false;
                if (temp3 < 0)
                {
                    negative3 = true;
                    temp3 = -temp3;
                }
                for (int i = endPos3 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp3 % 10));
                    temp3 /= 10;
                }
                if (negative3) span[pos] = '-';
            });
        }

        /// <summary>
        /// 高效构建字符串
        /// </summary>
        public static string BuildString(string key, char separator1, int id1, char separator2, int id2, char separator3, int id3, char separator4, int id4)
        {
            // 预计算各id的字符长度
            int id1Len = id1 < 0 ? (int)Math.Floor(Math.Log10(-id1)) + 2 : (id1 == 0 ? 1 : (int)Math.Floor(Math.Log10(id1)) + 1);
            int id2Len = id2 < 0 ? (int)Math.Floor(Math.Log10(-id2)) + 2 : (id2 == 0 ? 1 : (int)Math.Floor(Math.Log10(id2)) + 1);
            int id3Len = id3 < 0 ? (int)Math.Floor(Math.Log10(-id3)) + 2 : (id3 == 0 ? 1 : (int)Math.Floor(Math.Log10(id3)) + 1);
            int id4Len = id4 < 0 ? (int)Math.Floor(Math.Log10(-id4)) + 2 : (id4 == 0 ? 1 : (int)Math.Floor(Math.Log10(id4)) + 1);

            // 总长度: key + '_' + id1 + '_' + id2 + '_' + id3 + '_' + id4
            int length = key.Length + 1 + id1Len + 1 + id2Len + 1 + id3Len + 1 + id4Len;

            return string.Create(length, (key, separator1, id1, id1Len, separator2, id2, id2Len, separator3, id3, id3Len, separator4, id4, id4Len), (span, state) =>
            {
                var (p1, sep1, num1, len1, sep2, num2, len2, sep3, num3, len3, sep4, num4, len4) = state;
                int pos = 0;

                // 复制key
                p1.AsSpan().CopyTo(span);
                pos += p1.Length;

                // 直接写入separator
                span[pos++] = sep1;
                // 将id1直接写入span
                int endPos1 = pos + len1;
                int temp1 = num1;
                bool negative1 = false;
                if (temp1 < 0)
                {
                    negative1 = true;
                    temp1 = -temp1;
                }
                for (int i = endPos1 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp1 % 10));
                    temp1 /= 10;
                }
                if (negative1) span[pos] = '-';
                pos = endPos1;

                // 直接写入separator
                span[pos++] = sep2;
                // 将id2直接写入span
                int endPos2 = pos + len2;
                int temp2 = num2;
                bool negative2 = false;
                if (temp2 < 0)
                {
                    negative2 = true;
                    temp2 = -temp2;
                }
                for (int i = endPos2 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp2 % 10));
                    temp2 /= 10;
                }
                if (negative2) span[pos] = '-';
                pos = endPos2;

                // 直接写入separator
                span[pos++] = sep3;
                // 将id3直接写入span
                int endPos3 = pos + len3;
                int temp3 = num3;
                bool negative3 = false;
                if (temp3 < 0)
                {
                    negative3 = true;
                    temp3 = -temp3;
                }
                for (int i = endPos3 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp3 % 10));
                    temp3 /= 10;
                }
                if (negative3) span[pos] = '-';
                pos = endPos3;

                // 直接写入separator
                span[pos++] = sep4;
                // 将id4直接写入span
                int endPos4 = pos + len4;
                int temp4 = num4;
                bool negative4 = false;
                if (temp4 < 0)
                {
                    negative4 = true;
                    temp4 = -temp4;
                }
                for (int i = endPos4 - 1; i >= pos; i--)
                {
                    span[i] = (char)('0' + (temp4 % 10));
                    temp4 /= 10;
                }
                if (negative4) span[pos] = '-';
            });
        }

    }
}

#endif
