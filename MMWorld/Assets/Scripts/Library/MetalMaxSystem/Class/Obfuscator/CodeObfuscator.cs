using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MetalMaxSystem
{
    /// <summary>
    /// 代码混肴器
    /// </summary>
    public class CodeObfuscator
    {
        private System.Random random;
        private HashSet<string> usedNames;
        private HashSet<string> exclusionSet;

        /// <summary>
        /// 自定义的混肴规则属性，用于正则匹配代码正文进行替换
        /// </summary>
        public Dictionary<string, string> Replacements { get; set; }
        /// <summary>
        /// 自定义的混肴规则属性，用于正则匹配代码正文进行第二遍替换
        /// </summary>
        public Dictionary<string, string> Replacements2 { get; set; }

        /// <summary>
        /// 创建代码混肴器类实例，此后可使用"实例名.Replacements"属性自定义混肴规则以及使用"ObfuscateCode"方法进行代码混肴。
        /// </summary>
        public CodeObfuscator()
        {
            Replacements = new Dictionary<string, string>();
            Replacements2 = new Dictionary<string, string>();
            usedNames = new HashSet<string>();
            exclusionSet = new HashSet<string>();
            random = new System.Random(Guid.NewGuid().GetHashCode());
        }

        public void LoadExclusionRules(string exclusionFilePath)
        {
            if (!File.Exists(exclusionFilePath))
            {
                //Debug.WriteLine("排除规则文件不存在！");
                return;
            }

            string[] exclusionRules = File.ReadAllLines(exclusionFilePath);
            foreach (string rule in exclusionRules)
            {
                exclusionSet.Add(rule.Trim());
                //Console.WriteLine(rule.Trim());
            }
        }

        /// <summary>
        /// 手动添加混肴规则（以字典中的键值对，字符形式添加）
        /// </summary>
        /// <param name="originalName"></param>
        /// <param name="obfuscatedName"></param>
        public void AddReplacement2(string originalName, string obfuscatedName)
        {
            if (originalName != "")
            {
                // 检查是否已经存在相同的键
                if (Replacements2.ContainsKey(originalName))
                {
                    // 可以选择跳过重复的函数名或者生成一个不同的唯一替换名称
                    //Debug.WriteLine($"函数名 {originalName} 已经存在相同的替换名称，将跳过该函数名。");
                    return;
                }
                Replacements2.Add(originalName, obfuscatedName);
            }
        }

        /// <summary>
        /// 自动添加混肴规则：指定函数名会自动生成其混肴名称到混肴规则（过程中会自动去重，也不会生成相同混肴名称）
        /// </summary>
        /// <param name="originalName"></param>
        public void AddReplacement(string originalName)
        {
            if (originalName != "")
            {
                if (exclusionSet.Contains(originalName))
                {
                    //Debug.WriteLine($"函数名 {originalName} 在排除规则中，将跳过该函数名。");
                    return;
                }

                // 检查是否已经存在相同的键
                if (Replacements.ContainsKey(originalName))
                {
                    //可以选择跳过重复的函数名或者生成一个不同的唯一替换名称
                    //Debug.WriteLine($"函数名 {originalName} 已经存在相同的替换名称，将跳过该函数名。");
                    return;
                }
                string obfuscatedName = GenerateRandomString(8); // 生成8个字符的随机字符串作为替换名称

                System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
                int randomIndex = random.Next(65, 91); // 65-90 为大写字母 A-Z 的 ASCII 码
                char randomLetter = Convert.ToChar(randomIndex);
                obfuscatedName = randomLetter.ToString() + obfuscatedName;//防止函数名开头是数字，这里加一个随机大写字母

                Replacements.Add(originalName, obfuscatedName);
            }
        }

        //使用指定长度的随机字符串生成算法，生成一个包含字母和数字的随机字符串。
        private string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string randomString = new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());

            while (usedNames.Contains(randomString))
            {
                randomString = new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            usedNames.Add(randomString);
            return randomString;
        }

        /// <summary>
        /// 以字典中自定义的混肴规则，进行代码文本中的函数名混肴
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string ObfuscateCode(string code)
        {
            //以分组形式创建正则匹配替换规则：将Replacements字典中所有键里的具有特殊含义的符号通过正则表达式进行转义（使用时不需要加@）
            string pattern = string.Join("|", (string[])Replacements.Keys.Select(Regex.Escape));

            //创建名为replacementDelegate的委托实例，它接受一个字符串参数（匹配项）
            //在委托内部检查Replacements字典中是否存在该键，如果存在则返回相应的值，如果不存在则输出一条错误消息并将原始匹配项返回
            Func<string, string> replacementDelegate = (match) =>
            {
                if (Replacements.ContainsKey(match))
                {
                    //键存在且没有指定前缀字符时返回键对应的替换值
                    //要解决：Lib_gf_A和gf_A，如后者加到混肴规则，前者也会被替换一部分，需调用本函数前检查全函数名，如第一遍混肴规则里的键名是函数名的一部分则剔除该键
                    return Replacements[match];
                }
                else
                {
                    //否则返回原字符（届时替换相同字符）
                    //Debug.WriteLine("Key not found: " + match);
                    return match;
                }
            };

            //将代码中经过pattern正则匹配到的函数名替换为Replacements字典中以函数名为键对应的值
            string obfuscatedCode = Regex.Replace(code, pattern,
                (Match m) => replacementDelegate(m.Value)
                );

            return obfuscatedCode;
        }

        /// <summary>
        /// 以字典（Replacements2）中自定义的混肴规则，进行代码文本中被双引号包围的字符串混肴
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string ObfuscateCode2(string code)
        {
            //以分组形式创建正则匹配替换规则：将Replacements2字典中所有键里的具有特殊含义的符号通过正则表达式进行转义（使用时不需要加@）
            string pattern = string.Join("|", (string[])Replacements2.Keys.Select(Regex.Escape));

            //创建名为replacementDelegate2的委托实例，它接受一个字符串参数（匹配项）
            //在委托内部检查Replacements2字典中是否存在该键，如果存在则返回相应的值，如果不存在则输出一条错误消息并将原始匹配项返回
            Func<string, string> replacementDelegate = (match) =>
            {
                if (Replacements2.ContainsKey(match))
                {
                    //键存在且没有指定前缀字符时返回键对应的替换值
                    //要解决：："A"+"A"和"+"，如后者加到混肴规则，会出问题，所幸分组显示混肴规则中+或&不打反斜线转义匹配不出来，所以大概无需改良
                    return Replacements2[match];
                }
                else
                {
                    //否则返回原字符（届时替换相同字符）
                    //Debug.WriteLine("Key not found: " + match);
                    return match;
                }
            };

            //将代码中经过pattern正则匹配到的函数名替换为字典（Replacements2）中以函数名为键对应的值
            string obfuscatedCode = Regex.Replace(code, pattern,
                (Match m) => replacementDelegate(m.Value)
                );

            return obfuscatedCode;
        }

    }
}
