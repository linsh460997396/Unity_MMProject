using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using StormLib;

namespace GalaxyObfuscator
{
    /// <summary>
    /// 混肴器
    /// </summary>
    internal class Obfuscator
    {
        /// <summary>
        /// 启动混淆过程
        /// </summary>
        /// <param name="filename"></param>
        public static void Obfuscate(string filename)
        {
            Obfuscator obfuscator = new Obfuscator();
            obfuscator.ObfuscateFile(filename);
        }
        /// <summary>
        /// 混淆指定文件
        /// </summary>
        /// <param name="filename"></param>
        public void ObfuscateFile(string filename)
        {
            //生成混淆后的文件名
            string text = filename.Substring(0, filename.LastIndexOf('.')) + " Obfuscated.SC2Map";
            //若目标文件已存在则删除
            if (File.Exists(text))
            {
                File.Delete(text);
            }
            //复制原始文件到目标文件
            File.Copy(filename, text); using (MpqArchive mpqArchive = new MpqArchive(text))
            {
                //打开MPQ文件中的MapScript.galaxy
                using (StreamReader streamReader = new StreamReader(new BufferedStream(mpqArchive.OpenFile("MapScript.galaxy"))))
                {
                    this.script = streamReader.ReadToEnd();
                }
                //混淆脚本内容 
                this.script = this.obfuscateScript();
                //创建临时文件
                string tempFileName = Path.GetTempFileName();
                string tempFileName2 = Path.GetTempFileName();
                //将混淆后的脚本写入临时文件
                File.WriteAllText(tempFileName, this.script);
                //创建XML文件,包含混淆后的脚本
                using (FileStream fileStream = new FileStream(tempFileName2, FileMode.OpenOrCreate))
                {
                    XmlWriter xmlWriter = XmlWriter.Create(fileStream);
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("TriggerData");
                    xmlWriter.WriteStartElement("Root");
                    xmlWriter.WriteStartElement("Item");
                    xmlWriter.WriteAttributeString("Type", "CustomScript");
                    xmlWriter.WriteAttributeString("Id", "CFE7E55E");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("Element");
                    xmlWriter.WriteAttributeString("Type", "CustomScript");
                    xmlWriter.WriteAttributeString("Id", "CFE7E55E");
                    xmlWriter.WriteStartElement("ScriptCode");
                    xmlWriter.WriteString(this.script);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                    xmlWriter.Close();
                }
                //将临时文件添加到MPQ存档中
                mpqArchive.AddFile(tempFileName, "MapScript.galaxy", true);
                mpqArchive.AddFile(tempFileName2, "Triggers", true);
                //删除临时文件
                File.Delete(tempFileName2);
                File.Delete(tempFileName);
            }
        }

        /// <summary>
        /// 混淆脚本的方法
        /// </summary>
        /// <returns></returns>
        private string obfuscateScript()
        {
            //初始化标识符生成器和字典,扫描脚本并构建混淆表
            this.identifierGenerator = new IdentifierGenerator("lI1", 16); this.identifierTable = new Dictionary<Sequence, string>(); this.literalTable = new Dictionary<Sequence, string>(); this.scan(); foreach (string str in Obfuscator.ReservedIdentifiers)
            {
                Sequence key = new Sequence(str);
                if (this.identifierTable.ContainsKey(key))
                {
                    //移除保留标识符
                    this.identifierTable.Remove(key);
                }
            }
            //构建混淆后的脚本
            return this.construct();
        }
        /// <summary>
        /// 判断是否为独立的令牌
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool isSeparateToken(Token token)
        {
            return token.Type == TokenType.Identifier || token.Type == TokenType.HexLiteral || token.Type == TokenType.IntegerLiteral || token.Type == TokenType.RealLiteral;
        }
        /// <summary>
        /// 构建混淆后的脚本 
        /// </summary>
        /// <returns></returns>
        private string construct()
        {
            //初始化扫描器和字符串构建器
            this.scanner = new Scanner(this.script);
            StringBuilder stringBuilder = new StringBuilder(this.script.Length * 10);
            //处理include指令
            while (this.scanner.Read().Type == TokenType.Identifier && this.scanner.Current.Sequence == "include")
            {
                stringBuilder.AppendLine("include " + this.scanner.Read().Sequence);
            }
            //初始化当前令牌
            Token token = new Token
            {
                Type = TokenType.None
            };
            //逐个处理令牌 
            for (; ; )
            {
                Token token2 = this.scanner.Current;
                if (token2.Type != TokenType.None)
                {
                    //处理独立令牌之间的换行 
                    if (Obfuscator.isSeparateToken(token2) && Obfuscator.isSeparateToken(token))
                    {
                        stringBuilder.AppendLine();
                    }
                    //根据令牌类型进行处理
                    switch (token2.Type)
                    {
                        case TokenType.Identifier:
                            if (this.identifierTable.ContainsKey(token2.Sequence))
                            {
                                stringBuilder.Append(this.identifierTable[token2.Sequence]);
                            }
                            else
                            {
                                stringBuilder.Append(token2.ToString());
                            }
                            break;
                        case TokenType.StringLiteral:
                            if (this.literalTable.ContainsKey(token2.Sequence))
                            {
                                stringBuilder.Append(this.literalTable[token2.Sequence]);
                            }
                            else
                            {
                                stringBuilder.Append(this.stringObfuscator.Obfuscate(token2.ParseStringLiteral()));
                            }
                            break;
                        case TokenType.CharLiteral:
                        case TokenType.RealLiteral:
                        case TokenType.Symbol:
                            goto IL_19D;
                        case TokenType.IntegerLiteral:
                        case TokenType.HexLiteral:
                            stringBuilder.Append(this.integerObfuscator.Obfuscate(token2.ParseIntegerLiteral()));
                            break;
                        default:
                            goto IL_19D;
                    }
                IL_1B1:
                    token = token2;
                    if (!this.scanner.MoveNext())
                    {
                        break;
                    }
                    continue;
                IL_19D:
                    stringBuilder.Append(token2.ToString());
                    goto IL_1B1;
                }
                break;
            }
            //返回构建好的混淆脚本
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 添加标识符到表中 
        /// </summary>
        private void addIdentifier()
        {
            this.identifierTable.Add(this.scanner.Current.Sequence, this.identifierGenerator.Generate());
        }

        /// <summary>
        /// 扫描脚本并构建混淆表
        /// </summary>
        /// <exception cref="SyntaxErrorException"></exception>
        private void scan()
        {
            this.scanner = new Scanner(this.script);
            //逐个处理令牌
            while (this.scanner.MoveNext())
            {
                Token token = this.scanner.Current;
                if (token.Type == TokenType.None)
                {
                    return;
                }
                if (token.Type != TokenType.Identifier)
                {
                    throw new SyntaxErrorException(this.scanner);
                }
                string a;
                if ((a = token.ToString()) != null)
                {
                    if (a == "include")
                    {
                        this.scanner.ReadExpectedToken(TokenType.StringLiteral);
                        continue;
                    }
                    if (!(a == "typedef"))
                    {
                        if (a == "trigger")
                        {
                            this.scanTriggerDefinition();
                            continue;
                        }
                        if (a == "struct")
                        {
                            this.scanStructDefinition();
                            continue;
                        }
                    }
                    else
                    {
                        this.scanner.ReadExpectedToken(TokenType.Identifier);
                        this.scanVariableDeclaration(false);
                        if (this.scanner.Current.Sequence != ";")
                        {
                            throw new SyntaxErrorException(this.scanner);
                        }
                        continue;
                    }
                }
                this.scanDeclaration();
            }
        }

        /// <summary>
        /// 扫描触发器定义 
        /// </summary>
        private void scanTriggerDefinition()
        {
            //读取一个预期的标识符标记
            this.scanner.ReadExpectedToken(TokenType.Identifier);
            //将读取的标识符添加到某个地方(可能是标识符表或类似的结构)
            this.addIdentifier();
            //初始化字符串数组用于存储处理后的标识符
            string[] array = new string[2];
            string[] array2 = array;
            int num = 0;
            //获取当前标记(是之前读取的标识符)
            Token token = this.scanner.Current;
            //将当前标识符加上"_Func"后缀,并存入数组的第一个位置
            array2[num] = token.ToString() + "_Func";
            string[] array3 = array;
            int num2 = 1;
            //再次获取当前标记
            Token token2 = this.scanner.Current;
            //将当前标识符加上"Func"后缀,并存入数组的第二个位置
            array3[num2] = token2.ToString() + "Func";
            //遍历处理后的标识符数组
            foreach (string text in array)
            {
                string text2;
                //检查标识符表中是否已经存在该标识符(使用Sequence作为键)
                if (!this.identifierTable.ContainsKey(new Sequence(text)))
                {
                    //若不存在,则生成一个新的标识符
                    text2 = this.identifierGenerator.Generate();
                    //将新生成的标识符添加到标识符表中
                    this.identifierTable.Add(new Sequence(text), text2);
                }
                else
                {
                    //若存在则直接使用已有的标识符
                    text2 = this.identifierTable[new Sequence(text)];
                }
                //将处理后的标识符(新生成的或是已有的)添加到字面量表中
                //注意:这里将标识符用双引号括起来,并进行了混淆处理
                this.literalTable.Add(new Sequence("\"" + text + "\""), this.stringObfuscator.Obfuscate(text2));
            }
            //跳过至下一个分号,表示触发器定义的结束
            this.scanner.SkipBlock(";");
        }

        /// <summary>
        /// 跳过表达式直至遭遇特定的终止符号(分号、逗号或右括号)
        /// </summary>
        private void skipExpression()
        {
            //利用无限循环不断读取并处理标记,直至满足退出条件
            for (; ; )
            {
                //读取下一个预期的标记并立即检查其类型是否为符号
                if (this.scanner.ReadExpectedToken().Type == TokenType.Symbol)
                {
                    //获取当前标记的引用
                    Token token = this.scanner.Current;
                    //将标记转换为字符串形式以便进行后续比较
                    //此处的转换通常不会返回null因为Token的ToString方法应当总是返回有效的字符串
                    string a;
                    if ((a = token.ToString()) != null)
                    {
                        //首先检查标记是否为左括号
                        if (!(a == "("))
                        {
                            //若不是左括号则进一步检查是否为终止符号(分号、逗号或右括号)
                            if (a == ";" || a == "," || a == ")")
                            {
                                //若是终止符号则退出循环
                                break;
                            }
                        }
                        else
                        {
                            //若是左括号则调用SkipBlock方法跳过由左右括号包围的内容
                            //通常用于跳过括号内的表达式或语句块
                            this.scanner.SkipBlock("(", ")");
                        }
                    }
                    //若标记的字符串形式为null(虽然这种情况很少发生但理论上应当处理)
                    //或者不是需要关注的符号则循环将继续,读取下一个标记
                }
                //若读取的标记不是符号类型则循环也将继续
            }
            //当遇到终止符号并退出循环后方法执行完毕
        }

        /// <summary>
        /// 扫描变量声明语句
        /// </summary>
        /// <param name="allowMultiple">表示是否允许声明多个变量(通过逗号分隔)</param>
        /// <exception cref="SyntaxErrorException"></exception>
        private void scanVariableDeclaration(bool allowMultiple = false)
        {
            if (this.scanner.Current.Sequence == "const")
            {
                //若当前扫描到的标记是"const"则读取接下来的标识符(变量名)
                this.scanner.ReadExpectedToken(TokenType.Identifier);
            }
            //读取并跳过变量类型(假设类型已经在之前被读取)
            this.scanner.ReadExpectedToken();
            //处理可能存在的数组维度(例如 int[] arr)
            while (this.scanner.Current.Type == TokenType.Symbol && this.scanner.Current.Sequence == "[")
            {
                // 跳过数组维度内的所有内容直到找到闭合的']'
                do
                {
                    this.scanner.ReadExpectedToken();
                }
                while (this.scanner.Current.Type != TokenType.Symbol || this.scanner.Current.Sequence != "]");
                //读取并跳过闭合的']'
                this.scanner.ReadExpectedToken();
            }
            //检查当前标记是否为标识符(变量名)
            if (this.scanner.Current.Type != TokenType.Identifier)
            {
                //若不是则抛出语法错误异常
                throw new SyntaxErrorException(this.scanner);
            }
            //若标识符表中不包含当前标识符则将其添加进去
            if (!this.identifierTable.ContainsKey(this.scanner.Current.Sequence))
            {
                this.addIdentifier();
            }
            //读取并跳过'='符号(若存在表示有初始化表达式)
            this.scanner.ReadExpectedToken(TokenType.Symbol);
            //若当前标记是'='则跳过初始化表达式
            if (this.scanner.Current.Sequence == "=")
            {
                this.skipExpression();
            }
            //若允许声明多个变量则继续处理逗号分隔的变量声明
            if (allowMultiple)
            {
                //循环处理每个逗号分隔的变量声明
                while (this.scanner.Current.Sequence == ",")
                {
                    //读取并跳过逗号
                    this.scanner.ReadExpectedToken(TokenType.Identifier);
                    //添加新的标识符到标识符表中
                    this.addIdentifier();
                    //读取并跳过'='符号(若存在)
                    this.scanner.ReadExpectedToken(TokenType.Symbol);
                    //若当前标记是'='则跳过初始化表达式
                    if (this.scanner.Current.Sequence == "=")
                    {
                        this.skipExpression();
                    }
                }
            }
        }

        /// <summary>
        /// 解析并处理结构体定义
        /// </summary>
        /// <exception cref="SyntaxErrorException"></exception>
        private void scanStructDefinition()
        {
            //读取结构体名称
            this.scanner.ReadExpectedToken(TokenType.Identifier);
            this.addIdentifier();
            //读取{符号
            this.scanner.ReadExpectedSymbol("{");
            TokenType type;
            for (; ; )
            {
                type = this.scanner.Read().Type;
                if (type != TokenType.Identifier)
                {
                    break;
                }
                //解析成员变量声明
                this.scanVariableDeclaration(true);
                //检查分号
                if (this.scanner.Current.Sequence != ";")
                {
                    goto Block_3;
                }
            }
            //确保读取到}符号
            if (type != TokenType.Symbol)
            {
                throw new SyntaxErrorException(this.scanner);
            }
            if (this.scanner.Current.Sequence != "}")
            {
                throw new SyntaxErrorException(this.scanner);
            }
            //确保读取到;符号
            this.scanner.ReadExpectedSymbol(";");
            return;
        Block_3:
            throw new SyntaxErrorException(this.scanner);
        }

        /// <summary>
        /// 扫描和处理声明语句(可能是变量声明或函数声明)
        /// </summary>
        /// <exception cref="SyntaxErrorException"></exception>
        private void scanDeclaration()
        {
            //首先以允许声明多个变量的模式扫描变量声明
            this.scanVariableDeclaration(true);
            //获取当前扫描器指向的标记(token)并尝试将其转换为字符串
            Token token = this.scanner.Current;
            string a;
            //若转换后的字符串为空则直接返回,不进行后续处理
            if ((a = token.ToString()) == null)
            {
                return;
            }
            //检查当前标记是否为分号或不是左括号
            //若是分号表示声明语句结束,不是左括号则可能不是函数声明,直接返回.
            if (a == ";" || !(a == "("))
            {
                return;
            }
            //若当前标记是左括号则读取并跳过它,准备解析函数参数
            this.scanner.ReadExpectedToken();
            //若接下来的标记是标识符(可能表示函数参数的类型)则以不允许声明多个变量的模式再次扫描变量声明(处理函数参数)
            if (this.scanner.Current.Type == TokenType.Identifier)
            {
                this.scanVariableDeclaration(false);
                //若后续还有逗号表示有多个参数,继续扫描每个参数
                while (this.scanner.Current.Sequence == ",")
                {
                    //读取并跳过逗号
                    this.scanner.ReadExpectedToken(TokenType.Identifier);
                    //扫描下一个参数声明
                    this.scanVariableDeclaration(false);
                }
            }
            //此时应期望一个右括号来结束函数参数列表,若不是右括号则抛出语法错误异常
            if (this.scanner.Current.Type != TokenType.Symbol && this.scanner.Current.Sequence != ")")
            {
                throw new SyntaxErrorException(this.scanner);
            }
            //读取并跳过右括号
            this.scanner.ReadExpectedToken(TokenType.Symbol);
            //接下来的标记应该是分号表示声明语句结束或者是左大括号表示函数体的开始
            if (this.scanner.Current.Sequence == ";")
            {
                //若是分号则直接返回,声明语句结束
                return;
            }
            if (this.scanner.Current.Sequence == "{")
            {
                //若是左大括号则调用scanBlock方法扫描函数体
                this.scanBlock();
                return;
            }
            // 若既不是分号也不是左大括号则抛出语法错误异常
            throw new SyntaxErrorException(this.scanner);
        }

        /// <summary>
        /// 跳过特定的指令(如for循环的初始化语句)
        /// </summary>
        private void skipInstruction()
        {
            //获取当前扫描器所指的标记(token)
            Token token = this.scanner.Current;
            //若当前标记是"for"关键字,设置num为2
            //因为for循环通常包含初始化语句、条件判断语句和迭代语句,此处只想跳过初始化语句所以需要额外处理两个分号
            int num = (token.ToString() == "for") ? 2 : 0;
            //使用无限循环来持续读取标记直至满足退出条件
            for (; ; )
            {
                //检查当前标记是否为符号
                if (this.scanner.Current.Type == TokenType.Symbol)
                {
                    //获取当前符号标记
                    Token token2 = this.scanner.Current;
                    string a;
                    //若符号不是null(即确实读取到了符号)
                    if ((a = token2.ToString()) != null)
                    {
                        //若符号是左大括号{表示代码块开始,应退出循环
                        if (a == "{")
                        {
                            break;
                        }
                        //若符号是分号;且num减为0表示已经跳过了for循环的初始化语句,应返回
                        if (a == ";")
                        {
                            if (num-- == 0)
                            {
                                return;
                            }
                        }
                    }
                }
                //读取并跳过当前标记,继续检查下一个标记
                this.scanner.ReadExpectedToken();
            }
            //当退出循环时说明遇到了左大括号{表示代码块开始,此时调用scanBlock方法来扫描并处理该代码块
            this.scanBlock();
        }

        /// <summary>
        /// 扫描并处理代码块(例如函数体、循环体等)直至遇到右大括号}表示代码块结束
        /// </summary>
        /// <exception cref="SyntaxErrorException"></exception>
        private void scanBlock()
        {
            //使用无限循环来持续读取标记直至满足退出条件
            for (; ; )
            {
                //读取并跳过当前标记然后检查下一个标记
                this.scanner.ReadExpectedToken();
                //若当前标记是符号则进一步判断
                if (this.scanner.Current.Type == TokenType.Symbol)
                {
                    //若符号是右大括号}表示代码块结束,应退出循环
                    if (this.scanner.Current.Sequence == "}")
                    {
                        break;
                    }
                    //若符号是左大括号{表示嵌套的代码块开始,递归调用scanBlock方法处理
                    if (this.scanner.Current.Sequence == "{")
                    {
                        this.scanBlock();
                    }
                    else
                    {
                        //若符号是分号表示一条语句结束,继续检查下一条语句
                        if (this.scanner.Current.Sequence == ";")
                        {
                            continue;
                        }
                        //若符号不是上述三种情况之一则跳过以分号为结尾的代码块
                        //这可能是为了处理如单行注释或特定语法结构的跳过
                        this.scanner.SkipBlock(";");
                    }
                }
                //若当前标记不是标识符则跳转到Block_4标签,抛出语法错误异常
                if (this.scanner.Current.Type != TokenType.Identifier)
                {
                    goto Block_4;
                }
                //获取关键字列表用于后续判断当前标识符是否为关键字
                IEnumerable<string> keywords = Obfuscator.Keywords;
                //获取当前标识符标记
                Token token = this.scanner.Current;
                //若当前标识符是关键字则跳过该指令
                if (keywords.Contains(token.ToString()))
                {
                    this.skipInstruction();
                }
                else
                {
                    //若不是关键字则尝试扫描变量声明
                    try
                    {
                        this.scanVariableDeclaration(true);
                    }
                    //若遇到UnexpectedEndOfFileException异常则直接抛出
                    catch (UnexpectedEndOfFileException ex)
                    {
                        throw ex;
                    }
                    //若遇到SyntaxErrorException异常则跳过当前指令并继续检查下一个标记
                    catch (SyntaxErrorException)
                    {
                        this.skipInstruction();
                        continue;
                    }
                    //若扫描变量声明后当前标记不是分号则跳转到Block_7标签,抛出语法错误异常
                    if (this.scanner.Current.Sequence != ";")
                    {
                        goto Block_7;
                    }
                }
            }
            return;
        Block_4:
            throw new SyntaxErrorException(this.scanner);
        Block_7:
            throw new SyntaxErrorException(this.scanner);
        }

        /// <summary>
        /// 定义脚本文件的名称常量
        /// </summary>
        private const string ScriptFileName = "MapScript.galaxy";
        /// <summary>
        /// 定义关键字数组,这些关键字在脚本解析中具有特殊含义
        /// </summary>
        private static readonly string[] Keywords = new string[]
{
            "for",
            "while",
            "if",
            "else",
            "return"
};
        /// <summary>
        /// 定义保留标识符数组,这些标识符在脚本中可能具有特殊用途,不应被混淆
        /// </summary>
        private static readonly string[] ReservedIdentifiers = new string[]
{
            "InitMap"
};
        /// <summary>
        /// 标识符生成器对象,可能用于生成唯一的标识符或进行某种形式的标识符混淆
        /// </summary>
        private IdentifierGenerator identifierGenerator;
        /// <summary>
        /// 标识符表,存储已声明的标识符及其相关信息(如混淆后的名称)
        /// </summary>
        private IDictionary<Sequence, string> identifierTable;
        /// <summary>
        /// 字面量表,存储已出现的字面量(如字符串、数字)及其相关信息(如混淆后的值)
        /// </summary>
        private IDictionary<Sequence, string> literalTable;
        /// <summary>
        /// 扫描器对象,用于逐个读取脚本文件的标记(token)
        /// </summary>
        private Scanner scanner;
        /// <summary>
        /// 存储脚本内容的字符串
        /// </summary>
        private string script;
        /// <summary>
        /// 随机数生成器对象,可能用于混淆过程中的随机化操作
        /// </summary>
        private Random random = new Random();
        /// <summary>
        /// 字符串混淆器对象,用于对字符串字面量进行混淆处理
        /// </summary>
        private StringObfuscator stringObfuscator = new StringObfuscator();
        /// <summary>
        /// 整数混淆器对象,用于对整数字面量进行混淆处理
        /// </summary>
        private IntegerObfuscator integerObfuscator = new IntegerObfuscator();
    }
}

















