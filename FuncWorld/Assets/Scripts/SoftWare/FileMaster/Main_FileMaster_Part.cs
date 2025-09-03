//using MetalMaxSystem;
//using SimpleFileBrowser;
//using System.Diagnostics;
//using System.Text.RegularExpressions;
//using System;
//using System.Threading;
//using TMPro;
//using UnityEditor.ShaderGraph;
//using UnityEngine;
//using static SimpleFileBrowser.FileBrowser;
//using static UnityEngine.Rendering.DebugUI;
//using Button = UnityEngine.UI.Button;
//using Debug = UnityEngine.Debug;

//namespace FileMaster
//{
    //public partial class Main_FileMaster
    //{
    //    //private static bool _userOpEnable = true;
    //    ///// <summary>
    //    ///// 用户操作许可
    //    ///// </summary>
    //    //public static bool UserOpEnable { get => _userOpEnable; set => _userOpEnable = value; }

//    //private static bool _workStatus = false;
//    ///// <summary>
//    ///// 工作状态
//    ///// </summary>
//    //public static bool WorkStatus { get => _workStatus; set => _workStatus = value; }

//    //private static bool _workStop = false;
//    ///// <summary>
//    ///// 打断工作用的状态变量
//    ///// </summary>
//    //public static bool WorkStop { get => _workStop; set => _workStop = value; }

//    //private static Thread _workThread;
//    ///// <summary>
//    ///// 工作专用后台子线程,防止工作时UI主线程界面卡住无法点击等问题
//    ///// </summary>
//    //public static Thread WorkThread { get => _workThread; set => _workThread = value; }

//    //public Form1()
//    //{
//    //    InitializeComponent();
//    //    //label_Tips.ForeColor = Color.Red;
//    //    label_Statistics.ForeColor = Color.Red;
//    //}

//    //private string GetCodeFromMainThread()
//    //{
//    //    if (richTextBox_Code.InvokeRequired)
//    //    {
//    //        return richTextBox_Code.Invoke(GetCodeFromMainThread);
//    //    }
//    //    else
//    //    {
//    //        return richTextBox_Code.Text;
//    //    }
//    //}

//    //private void SetCodeToMainThread(string code)
//    //{
//    //    // 调用 Invoke 方法将操作发送到主线程
//    //    Invoke((MethodInvoker)delegate ()
//    //    {
//    //        richTextBox_Code.Text = code;
//    //    });
//    //}

//    //private int GetSelectedIndexFromMainThread()
//    //{
//    //    if (comboBox_SelectFunc.InvokeRequired)
//    //    {
//    //        return comboBox_SelectFunc.Invoke(GetSelectedIndexFromMainThread);
//    //    }
//    //    else
//    //    {
//    //        return comboBox_SelectFunc.SelectedIndex;
//    //    }
//    //}

//    //private string GetTipsFromMainThread()
//    //{
//    //    if (label_Tips.InvokeRequired)
//    //    {
//    //        return label_Tips.Invoke(GetTipsFromMainThread);
//    //    }
//    //    else
//    //    {
//    //        return label_Tips.Text;
//    //    }
//    //}

//    //private void SetTipsToMainThread(string tips)
//    //{
//    //    // 调用 Invoke 方法将操作发送到主线程
//    //    Invoke((MethodInvoker)delegate ()
//    //    {
//    //        label_Tips.Text = tips;
//    //    });
//    //}

//    //private string GetStatisticsFromMainThread()
//    //{
//    //    if (label_Statistics.InvokeRequired)
//    //    {
//    //        return label_Statistics.Invoke(GetStatisticsFromMainThread);
//    //    }
//    //    else
//    //    {
//    //        return label_Statistics.Text;
//    //    }
//    //}

//    //private string GetExclusionRulesPathFromMainThread()
//    //{
//    //    if (textBox_ExclusionRulesPath.InvokeRequired)
//    //    {
//    //        return textBox_ExclusionRulesPath.Invoke(GetExclusionRulesPathFromMainThread);
//    //    }
//    //    else
//    //    {
//    //        return textBox_ExclusionRulesPath.Text;
//    //    }
//    //}

//    //private void SetExclusionRulesPathToMainThread(string path)
//    //{
//    //    // 调用 Invoke 方法将操作发送到主线程
//    //    Invoke((MethodInvoker)delegate ()
//    //    {
//    //        textBox_ExclusionRulesPath.Text = path;
//    //    });
//    //}

//    //private void SetStatisticsToMainThread(string text)
//    //{
//    //    // 调用 Invoke 方法将操作发送到主线程
//    //    Invoke((MethodInvoker)delegate ()
//    //    {
//    //        label_Statistics.Text = text;
//    //    });
//    //}

//    //private void SetBtnRunTextToMainThread(string text)
//    //{
//    //    // 调用 Invoke 方法将操作发送到主线程
//    //    Invoke((MethodInvoker)delegate ()
//    //    {
//    //        button_Run.Text = text;
//    //    });
//    //}

//    //private void SetPanelBackColorToMainThread(Panel p, Color c)
//    //{
//    //    // 调用 Invoke 方法将操作发送到主线程
//    //    Invoke((MethodInvoker)delegate ()
//    //    {
//    //        p.BackColor = c;
//    //    });
//    //}

//    //private void SetControlEnableToMainThread(Control c, bool torf)
//    //{
//    //    // 调用 Invoke 方法将操作发送到主线程
//    //    Invoke((MethodInvoker)delegate ()
//    //    {
//    //        c.Enabled = torf;
//    //    });
//    //}

//    //private Type GetControlTypeFromMainThread(Control control)
//    //{
//    //    Type type = null;
//    //    if (control.InvokeRequired) // 判断当前线程是否为UI主线程
//    //    {
//    //        control.BeginInvoke((MethodInvoker)(() => GetControlTypeFromMainThread(control))); // 将操作放入UI主线程的消息队列中
//    //    }
//    //    else
//    //    {
//    //        type = control.GetType(); // 获取控件的类型信息
//    //    }
//    //    return type;
//    //}

//    ///// <summary>
//    ///// 按钮点击后处理主要内容,本函数交由后台线程运行
//    ///// </summary>
//    //private void ButtonRun()
//    //{
//    //    for (int i = 0; i < 1; i++)
//    //    {
//    //        Stopwatch stopwatch = new Stopwatch();
//    //        stopwatch.Start();
//    //        switch (GetSelectedIndexFromMainThread())
//    //        {
//    //            case -1:
//    //                SetTipsToMainThread("功能未选择！");
//    //                break;
//    //            case 0:
//    //                //SetTipsToMainThread("对Galaxy代码进行混肴");
//    //                SelectedFunc_0(GetExclusionRulesPathFromMainThread());
//    //                break;
//    //            default:
//    //                SetTipsToMainThread("功能无效！");
//    //                break;
//    //        }
//    //        stopwatch.Stop();
//    //        //Debug.WriteLine(stopwatch.Elapsed.ToString());
//    //        SetStatisticsToMainThread(" 时耗:" + stopwatch.Elapsed.ToString());
//    //    }
//    //    //放弃了线程注销做法,程序将始终运行至此,可以知道是用户中断还是正常运行结束
//    //    WorkStatus = false;//重置工作状态
//    //    if (WorkStop) { SetStatisticsToMainThread("用户取消！"); }
//    //    WorkStop = false;//重置_workStop状态,若是用户取消的,打印告知
//    //    UserOpEnableChange(true);//重置用户操作状态
//    //    SetBtnRunTextToMainThread("执行");
//    //    //Debug.WriteLine("子线程已经完成！");
//    //    //线程清除(若不放心,Abort方法能在目标线程中抛出一个ThreadAbortException异常从而导致目标线程的终止)
//    //    //WorkThread.Abort();
//    //}

//    ///// <summary>
//    ///// The Button of Run.
//    ///// 点击执行按钮后应创建一个后台线程进行复杂任务处理,防止对UI所在的主线程造成卡顿.
//    ///// </summary>
//    ///// <param name="sender"></param>
//    ///// <param name="e"></param>
//    //private void button_Run_Click(object sender, EventArgs e)
//    //{
//    //    //正则匹配抓取全部函数名(包含重复)
//    //    if (richTextBox_Code.Text != "")
//    //    {
//    //        for (int i = 0; i < 1; i++)
//    //        {
//    //            //开始工作,大部分界面置灰(用户不可操作)
//    //            UserOpEnableChange(false);

//    //            if (button_Run.Text == "执行" && WorkStatus == false)
//    //            {
//    //                WorkStatus = true;
//    //                button_Run.Text = "取消";
//    //                //创建后台线程实例来运行复杂任务
//    //                WorkThread = new Thread(ButtonRun) { IsBackground = true };
//    //                WorkThread.Start();
//    //                // 等待子线程完成↓
//    //                //WorkThread.Join();
//    //            }
//    //            else if (button_Run.Text == "取消" && WorkStatus == true)
//    //            {
//    //                WorkStop = true;
//    //            }
//    //        }
//    //    }

//    //}

//    ///// <summary>
//    ///// 第1个功能索引的执行方法:对Galaxy代码进行混肴
//    ///// </summary>
//    ///// <param name="exclusionRulesPath"></param>
//    //private void SelectedFunc_0(string exclusionRulesPath)
//    //{
//    //    //排除规则文件使用系统默认的情况:1)文件路径为空；2)文件路径虽不为空但后缀非.txt；3)文件路径非法.
//    //    if (
//    //        exclusionRulesPath == ""
//    //        || !(Regex.IsMatch(exclusionRulesPath, @"^(.*)(\.txt)$"))
//    //        || !MMCore.IsDFPath(exclusionRulesPath)
//    //    )
//    //    {
//    //        //文本路径错误,重置为系统默认
//    //        exclusionRulesPath = AppDomain.CurrentDomain.BaseDirectory + @"exclusion_rules.txt";
//    //        SetExclusionRulesPathToMainThread(exclusionRulesPath);
//    //        //SetTipsToMainThread("排除规则文件路径错误,重置为系统默认！");
//    //    }
//    //    //去除代码里的注释
//    //    string mainCode = MMCore.RemoveEmptyLines(MMCore.RemoveComments(GetCodeFromMainThread()));
//    //    // 定义正则表达式模式,匹配函数名
//    //    string pattern = @"(?<=^|[^a-zA-Z_])[a-zA-Z_][\w]*(?=\s*\([^\)]*\)(\s+|\n|$))";
//    //    MatchCollection matches = Regex.Matches(mainCode, pattern);
//    //    Obfuscation obfuscator = new Obfuscation();
//    //    //读取排除规则文本,添加混肴规则时会防止它们参与混肴(格式:每行一个函数名)
//    //    obfuscator.LoadExclusionRules(exclusionRulesPath);
//    //    //遍历全部函数名
//    //    foreach (Match match in matches)
//    //    {
//    //        //Debug.WriteLine("Function Name: " + match.Value);
//    //        //构建正则表达式,除了"排除规则文本"指定的函数名,让Lib、GAx3开头的函数名也避开混肴
//    //        if (!Regex.IsMatch(match.Value, "^(Lib|lib|GAx3).*"))
//    //        {
//    //            //添加函数名及混肴后名称到混肴规则(这过程会自动去重,也不会生成相同混肴名称)
//    //            obfuscator.AddReplacement(match.Value);
//    //        }

//    //    }
//    //    //要解决:Lib_gf_A和gf_A,如后者加到混肴规则,前者也会被替换一部分,所以混肴前得检查函数名并对混肴规则修改:如第一遍混肴规则里的键名是按库名匹配的函数名的一部分则剔除该键
//    //    foreach (Match match in matches)
//    //    {
//    //        if (Regex.IsMatch(match.Value, "^(Lib|lib|GAx3).*"))
//    //        {
//    //            //混肴规则字典里的键名是match.Value的一部分则从字典里剔除该键
//    //            //检查match.Value是否包含字典中的任何键
//    //            foreach (var key in obfuscator.Replacements.Keys)
//    //            {
//    //                if (match.Value.Contains(key))
//    //                {
//    //                    //若match.Value包含字典中的键,则从字典中删除该键
//    //                    obfuscator.Replacements.Remove(key);
//    //                }
//    //            }
//    //        }
//    //    }

//    //    //第一遍混肴后的代码
//    //    string obfuscatedCode = obfuscator.ObfuscateCode(mainCode);

//    //    string temp = "";

//    //    //第二遍混肴
//    //    //对代码文本中的字符串进行混肴
//    //    //MatchCollection matches2 = Regex.Matches(obfuscatedCode, @"""(.*?)""");
//    //    MatchCollection matches2 = Regex.Matches(obfuscatedCode, @"""[^\\""]*""");
//    //    foreach (Match match in matches2)
//    //    {
//    //        //匹配到的字符串的内容(.*?)放在match.Groups[1].Value,内容非空则进行添加规则
//    //        if (match.Groups[1].Value != "")
//    //        {
//    //            if (Regex.IsMatch(match.Groups[1].Value, @".*\\.*")) { Debug.WriteLine(match.Groups[1].Value); }
//    //            //构建正则表达式,带有以下指定字符的字符串会避开混肴
//    //            if (!Regex.IsMatch(match.Groups[1].Value, "(\\|bnet:)"))
//    //            {
//    //                //添加到混肴规则2(要替换的字符串为键,混肴成8进制或18进制后的字符串为值)
//    //                temp = MMCore.ConvertStringToHOMixed(match.Groups[1].Value, 0.7); //这里是内容的混肴
//    //                                                                                  //Debug.WriteLine($"Found string: {match.Groups[1].Value}, Value: {temp}");
//    //                temp = "\"" + temp + "\""; //重新套上引号
//    //                                           //注意此处第二项规则的键要带""不能光内容字符作为键
//    //                obfuscator.AddReplacement2(match.Value, temp);
//    //            }
//    //        }
//    //    }
//    //    obfuscatedCode = obfuscator.ObfuscateCode2(obfuscatedCode);

//    //    //第三遍
//    //    //正则表达式匹配void InitMap () { 到 }的内容
//    //    //pattern = @"void\s*InitMap\s*\(\)\s*\{(.*\S)*\}";
//    //    ////使用 RegexOptions.Multiline 选项来指定模式应在多个行上进行匹配,并使用 RegexOptions.Singleline 选项来指定模式应在单个连续字符串上进行匹配
//    //    //RegexOptions options = RegexOptions.Multiline | RegexOptions.Singleline;
//    //    //MatchCollection matches3 = Regex.Matches(obfuscatedCode, pattern, options);
//    //    //foreach (Match match in matches3)
//    //    //{
//    //    //    if (match.Groups[1].Value != "")
//    //    //    {
//    //    //        temp = match.Groups[1].Value;
//    //    //        Debug.WriteLine(temp);
//    //    //    }
//    //    //}
//    //    //obfuscatedCode = Regex.Replace(obfuscatedCode, pattern, string.Empty, options);

//    //    string sCHeadPath = AppDomain.CurrentDomain.BaseDirectory + @"SCHead";
//    //    string sCEndPath = AppDomain.CurrentDomain.BaseDirectory + @"SCEnd";
//    //    string sCHead = File.ReadAllText(sCHeadPath);
//    //    string sCEnd = File.ReadAllText(sCEndPath);
//    //    obfuscatedCode = sCHead + obfuscatedCode + sCEnd;

//    //    SetCodeToMainThread(obfuscatedCode);
//    //}

//    //private void UserOpEnableChange(bool torf)
//    //{
//    //    UserOpEnable = torf;
//    //    //遍历全控件并获取类型(可跨线程不用回调),控件属性读写操作仍需要回调
//    //    foreach (Control a in Controls)
//    //    {
//    //        if (a is Panel)
//    //        {
//    //            Panel p = a as Panel;  //取出Panel
//    //            if (!torf)
//    //            {
//    //                // 改变Panel的颜色,执行时是灰色
//    //                SetPanelBackColorToMainThread(p, Color.Gray);
//    //            }
//    //            else
//    //            {
//    //                //不执行时是白色
//    //                SetPanelBackColorToMainThread(p, Color.Transparent);
//    //            }

//    //            foreach (Control c in p.Controls) //遍历面板中的每一个控件
//    //            {
//    //                //Debug.WriteLine(c.GetType().Name);
//    //                if (c.GetType().Name.Equals("TextBox"))
//    //                {
//    //                    //禁用文本框
//    //                    SetControlEnableToMainThread(c, torf);
//    //                }
//    //                if (c.GetType().Name.Equals("CheckBox"))
//    //                {
//    //                    //禁用复选框
//    //                    SetControlEnableToMainThread(c, torf);
//    //                }
//    //                if (c.GetType().Name.Equals("ComboBox"))
//    //                {
//    //                    //禁用下拉框
//    //                    SetControlEnableToMainThread(c, torf);
//    //                }
//    //                if (c.GetType().Name.Equals("Label"))
//    //                {
//    //                    //禁用下拉框
//    //                    SetControlEnableToMainThread(c, torf);
//    //                }
//    //                if (c.GetType().Name.Equals("Button"))
//    //                {
//    //                    if (!c.Name.Equals("button_Run"))
//    //                    {
//    //                        //禁用除运行按钮外的其他按钮
//    //                        SetControlEnableToMainThread(c, torf);
//    //                    }
//    //                }

//    //            }
//    //        }
//    //    }
//    //    //上面Controls未读到富格式文本框,单独设置
//    //    SetControlEnableToMainThread(richTextBox_Code, torf);
//    //}

//    //private void button_SelectExclusionRulesFile_Click(object sender, EventArgs e)
//    //{
//    //    OpenFileDialog ofd = new OpenFileDialog();
//    //    ofd.ShowDialog();
//    //    string path = ofd.FileName;
//    //    textBox_ExclusionRulesPath.Text = path;
//    //}

//    //private void richTextBox_Code_TextChanged(object sender, EventArgs e)
//    //{
//    //    //文本改变时不需要写任何动作
//    //}

//    //private void comboBox_SelectFunc_SelectedIndexChanged(object sender, EventArgs e)
//    //{
//    //    switch (GetSelectedIndexFromMainThread())
//    //    {
//    //        case -1:
//    //            SetTipsToMainThread("功能未选择！");
//    //            break;
//    //        case 0:
//    //            //SetTipsToMainThread("对Galaxy代码进行混肴");
//    //            break;
//    //        default:
//    //            SetTipsToMainThread("功能无效！");
//    //            break;
//    //    }
//    //}

//    //private void Form1_Load(object sender, EventArgs e)
//    //{
//    //    comboBox_SelectFunc.Items.Add("对Galaxy代码进行混肴(采用默认规则)");
//    //    comboBox_SelectFunc.SelectedIndex = 0;
//    //}

//}
//}