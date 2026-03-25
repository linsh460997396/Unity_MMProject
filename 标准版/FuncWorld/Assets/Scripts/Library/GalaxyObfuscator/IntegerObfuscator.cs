using System;
using System.Text;

namespace GalaxyObfuscator
{
    /// <summary>
    /// 内部混肴器
    /// </summary>
    internal class IntegerObfuscator
    {
        /// <summary>
        /// 简单混肴
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private string obfuscateSimple(int number)
        {
            //根据随机数生成器的结果,选择不同的方式来表示整数
            switch (this.random.Next(7))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    //以八进制形式表示整数,并在前面加上'0'
                    return "0" + Convert.ToString(number, 8);
                case 4:
                    //以十六进制形式表示整数,并在前面加上'0x'
                    return "0x" + Convert.ToString(number, 16);
                case 5:
                    //以十六进制形式表示整数,并在前面加上'0X'
                    return "0X" + Convert.ToString(number, 16);
                default:
                    //直接返回整数的字符串表示 
                    return number.ToString();
            }
        }

        /// <summary>
        /// 生成混淆后的加减法表达式.number的混淆结果以字符串的形式存储在StringBuilder对象str中
        /// </summary>
        /// <param name="str"></param>
        /// <param name="number"></param>
        /// <param name="canBeFixed">设置三组范围值(num3, num4, num5).若canBeFixed为true则范围较小,反之范围较大.这些范围值用于后续生成随机数时限制其大小</param>
        private void obfuscateAddition(StringBuilder str, int number, bool canBeFixed)
        {
            int num = 0;
            int num2 = this.random.Next(1, 8);//随机生成一个1到7之间的整数
            if (num2 >= 2)
            {
                num2 /= 2;//若 num2大于等于2,则将其除以2 
            }
            //定义范围值,根据canBeFixed的值决定范围
            int num3;
            int num4;
            int num5;
            if (canBeFixed)
            {
                num3 = -524287;
                num4 = 524287;
                num5 = 262143;
            }
            else
            {
                num3 = -2147483548;
                num4 = 2147483547;
                num5 = 1073676288;
            }
            //循环num2次,生成加减法表达式
            for (int i = 0; i < num2; i++)
            {
                int num7;
                if (i < num2 - 1)
                {
                    //如不是最后一次循环,生成一个随机long类型数值num6,该值基于number和一个在-num5到num5范围内的随机数相加得到
                    long num6 = (long)number + (long)this.random.Next(-num5, num5);
                    //然后确保num6在num3和num4指定的范围内
                    num6 = Math.Max(num6, (long)num3);
                    num6 = Math.Min(num6, (long)num4);
                    num7 = (int)num6;
                }
                else
                {
                    //若是最后一次循环直接使用原始的number值
                    num7 = number;
                }
                //计算当前数字num7与之前累计的结果num的差值并根据差值的正负决定是否在StringBuilder中添加'+'或'-'符号
                num = num7 - num;
                if (num < 0)
                {
                    str.Append('-');
                    num = -num;
                }
                else if (i > 0)
                {
                    str.Append('+');//若不是第一次循环,添加加号
                }
                //obfuscateSimple处理差值num,结果添加到StringBuilder中
                str.Append(this.obfuscateSimple(num));
                num = num7;//更新当前数字
            }
        }

        /// <summary>
        /// 混肴一个整数
        /// </summary>
        /// <param name="number">用于混肴的整数</param>
        /// <returns>返回混淆后的字符串</returns>
        public string Obfuscate(int number)
        {
            if (number == 2147483647)
            {
                //若输入的number是Int32的最大值(2147483647)则直接返回其十六进制表示形式"0x7fffffff"
                return "0x7fffffff";
            }
            //初始化StringBuilder对象用于构建混淆后的字符串
            StringBuilder stringBuilder = new StringBuilder();
            //判断number是否在一个较小的范围内(-524287 到 524287)并设置flag
            bool flag = -524287 <= number && number <= 524287;
            //在StringBuilder对象中追加一个左括号'('表示混淆表达式的开始
            stringBuilder.Append('(');
            //初始化num为0,它将用于存储异或操作的累积结果
            int num = 0;
            //随机生成一个1到6之间的整数num2表示将要进行的异或和加法操作的次数
            int num2 = this.random.Next(1, 7);
            //若num2大于等于3则将其调整为(num2+1)/2确保操作次数在1到3之间
            if (num2 >= 3)
            {
                num2 = (num2 + 1) / 2;
            }
            //循环num2次,每次执行以下操作:
            for (int i = 0; i < num2; i++)
            {
                //若不是第一次循环则在StringBuilder对象中追加'^'符号表示异或操作
                if (i > 0)
                {
                    stringBuilder.Append('^');
                }
                int num3;
                if (i < num2 - 1)
                {
                    //若当前不是最后一次循环则生成一个随机数num3,根据flag的值决定随机数的范围
                    //若flag为true则范围较小(0 到 524287)否则范围较大(0 到 2147418112)
                    num3 = this.random.Next(0, flag ? 524287 : 2147418112);
                }
                else
                {
                    //若是最后一次循环则直接使用原始的number值作为num3
                    num3 = number;
                }
                //对num和num3执行异或操作并将结果存回num中
                num ^= num3;
                //调用obfuscateAddition方法将异或操作的结果以加减法表达式的形式追加到StringBuilder对象中
                //传参包括StringBuilder对象、当前的num值及flag标志
                this.obfuscateAddition(stringBuilder, num, flag);
                //更新num值为当前的num3以便下一次循环中使用
                num = num3;
            }
            //在StringBuilder对象中追加一个右括号')'表示混淆表达式的结束
            stringBuilder.Append(')');
            //返回StringBuilder对象中构建的混淆后的字符串
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 固定范围的最小值
        /// </summary>
        private const int MinFixed = -524287;
        /// <summary>
        /// 固定范围的最大值
        /// </summary>
        private const int MaxFixed = 524287;
        /// <summary>
        /// 随机数生成器
        /// </summary>
        private Random random = new Random();
    }
}
