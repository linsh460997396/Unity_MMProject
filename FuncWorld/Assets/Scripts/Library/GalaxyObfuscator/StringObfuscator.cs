using System;
using System.Text;

namespace GalaxyObfuscator
{
    internal class StringObfuscator
    {
        public string Obfuscate(string str)
        {
            StringBuilder stringBuilder = new StringBuilder(str.Length * 5 + 2);
            stringBuilder.Append('"');
            foreach (char c in str)
            {
                int num = this.random.Next(5);
                stringBuilder.Append("\\");
                switch (num)
                {
                    case 0:
                    case 1:
                    case 2:
                        stringBuilder.Append(Convert.ToString((int)c, 8));
                        break;
                    case 3:
                        {
                            stringBuilder.Append("x");
                            StringBuilder stringBuilder2 = stringBuilder;
                            int num2 = (int)c;
                            stringBuilder2.Append(num2.ToString("x2"));
                            break;
                        }
                    case 4:
                        {
                            stringBuilder.Append("x");
                            StringBuilder stringBuilder3 = stringBuilder;
                            int num3 = (int)c;
                            stringBuilder3.Append(num3.ToString("x3"));
                            break;
                        }
                }
            }
            stringBuilder.Append('"');
            return stringBuilder.ToString();
        }

        private Random random = new Random();
    }
}
