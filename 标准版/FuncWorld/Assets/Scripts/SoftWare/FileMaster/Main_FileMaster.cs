using SimpleFileBrowser;
using System.Threading;
using TMPro;
using UnityEngine;
using static SimpleFileBrowser.FileBrowser;
using Button = UnityEngine.UI.Button;
using Debug = UnityEngine.Debug;

namespace FileMaster
{
    public partial class Main_FileMaster : MonoBehaviour
    {
        #region 字段及其属性方法

        private static bool _workStatus = false;
        private static bool _workStop = false;
        private static int _workErrCount = 0;
        private static int _startIndex;
        private static int _strCount;
        private static int _dirCount = 0;
        private static int _proCount;
        private static int _proCountMax;
        private static Thread _workThread;
        private static bool _userOpEnable = true;

        /// <summary>
        /// 工作状态
        /// </summary>
        public static bool WorkStatus { get => _workStatus; set => _workStatus = value; }
        /// <summary>
        /// 打断工作用的状态变量
        /// </summary>
        public static bool WorkStop { get => _workStop; set => _workStop = value; }
        /// <summary>
        /// 工作中无法处理的文件数量
        /// </summary>
        public static int WorkErrCount { get => _workErrCount; set => _workErrCount = value; }
        /// <summary>
        /// 文件名字符起始位置
        /// </summary>
        public static int StartIndex { get => _startIndex; set => _startIndex = value; }
        /// <summary>
        /// 从文件名起始位置开始计算的字符数量
        /// </summary>
        public static int StrCount { get => _strCount; set => _strCount = value; }
        /// <summary>
        /// 决定首次文件夹统计时是否换行的状态变量
        /// </summary>
        public static int DirCount { get => _dirCount; set => _dirCount = value; }
        /// <summary>
        /// 工作进度计数
        /// </summary>
        public static int ProCount { get => _proCount; set => _proCount = value; }
        /// <summary>
        /// 工作进度最大数
        /// </summary>
        public static int ProCountMax { get => _proCountMax; set => _proCountMax = value; }
        /// <summary>
        /// 工作专用后台子线程
        /// </summary>
        public static Thread WorkThread { get => _workThread; set => _workThread = value; }
        /// <summary>
        /// 用户操作许可
        /// </summary>
        public static bool UserOpEnable { get => _userOpEnable; set => _userOpEnable = value; }

        #endregion

        GameObject label_headTip;
        GameObject label_paramDescription3;
        GameObject comboBox_selectFunc;
        GameObject comboBox_DirEmptyMode;
        GameObject comboBox_param8;
        GameObject textBox_workPath;
        GameObject textBox_workFilePath;
        GameObject button_selectWorkPath;
        GameObject button_selectWorkFile;
        GameObject button_run;
        GameObject label_button_run;
        GameObject label_dirStatistics;

        void Awake()
        {
            //Variable Init
            label_headTip = GameObject.Find("label_headTip");
            label_paramDescription3 = GameObject.Find("label_paramDescription3");
            comboBox_selectFunc = GameObject.Find("comboBox_selectFunc");
            comboBox_DirEmptyMode = GameObject.Find("comboBox_DirEmptyMode");
            comboBox_param8 = GameObject.Find("comboBox_param8");
            textBox_workPath = GameObject.Find("textBox_workPath");
            textBox_workFilePath = GameObject.Find("textBox_workFilePath");
            button_selectWorkPath = GameObject.Find("button_selectWorkPath");
            button_selectWorkFile = GameObject.Find("button_openFolder");
            button_run = GameObject.Find("button_outsideSave");
            label_button_run = GameObject.Find("label_button_run");
            label_dirStatistics = GameObject.Find("label_dirStatistics");

            //Value Dft
            label_headTip.GetComponent<TextMeshProUGUI>().color = Color.red;
            label_paramDescription3.GetComponent<TextMeshProUGUI>().text = "检索后缀(如.txt)";
            if (comboBox_selectFunc.GetComponent<TMP_Dropdown>().value < 0)
            {
                //当值为-1(未选)时,重置为第一个元素选项
                comboBox_selectFunc.GetComponent<TMP_Dropdown>().value = 0;
            }
            if (comboBox_DirEmptyMode.GetComponent<TMP_Dropdown>().value < 0)
            {
                comboBox_DirEmptyMode.GetComponent<TMP_Dropdown>().value = 0;
            }
            if (comboBox_param8.GetComponent<TMP_Dropdown>().value < 0)
            {
                comboBox_param8.GetComponent<TMP_Dropdown>().value = 0;
            }

            //Event Register
            button_selectWorkPath.GetComponent<Button>().onClick.AddListener(button1_Click);
            button_selectWorkFile.GetComponent<Button>().onClick.AddListener(button2_Click);
            //button_outsideSave.GetComponent<Button>().onClick.AddListener(button3_Click);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        //#region Work

        /// <summary>
        /// 点击按钮1选择工作文件夹
        /// </summary>
        private void button1_Click()
        {
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.ShowDialog();
            //string path = fbd.SelectedPath;
            //Unity用System.Windows.Forms.dll需将其放入项目根目录而不是Plugins,且兼容性要改成Windows专用的FrameWork而不是兼容各平台的.net标准库

            //string path = UnityEditor.EditorUtility.OpenFilePanel("Load Directory", Application.dataPath, ""); //只能编辑器用,最后参数空字符表示不限制类型

            //这里我们使用SimpleFileBrowser,项目地址 https://github.com/yasirkula/UnitySimpleFileBrowser#example-code
            ShowLoadDialog((paths) => { Debug.Log("Selected: " + paths[0]); textBox_workPath.GetComponent<TMP_InputField>().text = paths[0]; },
                                       () => { Debug.Log("Canceled"); },
                                       PickMode.Folders, false, null, null, "Select Folder", "Select");

        }

        /// <summary>
        /// 点击按钮2选择工作文本
        /// </summary>
        private void button2_Click()
        {
            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.ShowDialog();
            //string path = ofd.FileName;
            //textBox_workFilePath.Text = path;
            FileBrowser.SetFilters(false, new FileBrowser.Filter("文本", ".txt"));
            //FileBrowser.SetDefaultFilter(".txt");
            ShowLoadDialog((paths) => { Debug.Log("Selected: " + paths[0]); textBox_workFilePath.GetComponent<TMP_InputField>().text = paths[0]; },
                                       () => { Debug.Log("Canceled"); },
                                       PickMode.Files, false, null, null, "Select Files", "Select");
        }

        ///// <summary>
        ///// 点击执行所选功能
        ///// </summary>
        //private void button3_Click()
        //{
        //    for (int i = 0; i < 1; i++)
        //    {
        //        if (MMCore.IsDFPath(textBox_workPath.GetComponent<TextMeshProUGUI>().text))
        //        {
        //            UserOpEnableChange(false);
        //        }
        //        else
        //        {
        //            label_dirStatistics.GetComponent<TextMeshProUGUI>().text = "工作目录错误！(若目录名正确,可反馈至作者邮箱)";
        //            break;
        //        }
        //        if (button_outsideSave.GetComponent<TextMeshProUGUI>().text == "执行" && WorkStatus == false)
        //        {
        //            WorkStatus = true;
        //            label_button_run.GetComponent<TextMeshProUGUI>().text = "取消";
        //            WorkThread = new Thread(ButtonRun) { IsBackground = true };
        //            WorkThread.Start();
        //        }
        //        else if (label_button_run.GetComponent<TextMeshProUGUI>().text == "取消" && WorkStatus == true)
        //        {
        //            WorkStop = true;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 下拉选择功能
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void comboBox1_SelectedIndexChanged()
        //{
        //    if (checkBox_protectSuffix.Checked)
        //    {
        //        textBox_param4.Text = "";
        //    } //勾选时,设置参数4为空(即保护任意后缀)
        //    else
        //    {
        //        textBox_param4.Text = "*";
        //    }
        //    switch (comboBox_selectFunc.SelectedIndex)
        //    {
        //        case 0:
        //            label_headTip.Text = "批量将文件(夹)名称打印到工作文件";
        //            panel2.Visible = false;
        //            panel3.Visible = false;
        //            panel5.Visible = false;
        //            panel9.Visible = false;
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 1:
        //            label_headTip.Text = "批量将文件(夹)名称去除固定位数字符(填写要移除的起始位和字符数,字符起始位从0起算)";
        //            label__paramDescription1.Text = "起始位(数字)";
        //            label_paramDescription2.Text = "移除字符数";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = true;
        //            panel3.Visible = true;
        //            panel9.Visible = true;
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 2:
        //            label_headTip.Text = "批量将文件(夹)名称保留固定位数字符(填写要保留的起始位和字符数,字符起始位从0起算)";
        //            label__paramDescription1.Text = "起始位(数字)";
        //            label_paramDescription2.Text = "保留字符数";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = true;
        //            panel3.Visible = true;
        //            panel9.Visible = true;
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 3:
        //            label_headTip.Text = "批量插入字符到文件(夹)名称固定位数字符前(填写固定字符起始位和字符数,字符起始位从0起算)";
        //            label__paramDescription1.Text = "起始位(数字)";
        //            label_paramDescription2.Text = "固定字符数";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = true;
        //            panel3.Visible = true;
        //            panel9.Visible = true;
        //            panel5.Visible = true; //参数5面板
        //            label16.Text = "插入字符"; //参数5说明
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 4:
        //            label_headTip.Text = "批量插入字符到文件(夹)名称固定位数字符后(填写固定字符起始位和字符数,字符起始位从0起算)";
        //            label__paramDescription1.Text = "起始位(数字)";
        //            label_paramDescription2.Text = "固定字符数";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = true;
        //            panel3.Visible = true;
        //            panel9.Visible = true;
        //            panel5.Visible = true; //参数5面板
        //            label16.Text = "插入字符"; //参数5说明
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 5:
        //            label_headTip.Text = "批量插入字符到文件(夹)名称最前(勾选保留后缀则只修改前缀名称)";
        //            label__paramDescription1.Text = "起始位(数字)";
        //            label_paramDescription2.Text = "固定字符数";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = false;
        //            panel3.Visible = false;
        //            panel9.Visible = true;
        //            panel5.Visible = true; //参数5面板
        //            label16.Text = "插入字符"; //参数5说明
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 6:
        //            label_headTip.Text = "批量插入字符到文件(夹)名称最后(注:保护后缀要将参数4留空)";
        //            label__paramDescription1.Text = "起始位(数字)";
        //            label_paramDescription2.Text = "保留字符数";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = false;
        //            panel3.Visible = false;
        //            panel9.Visible = true;
        //            panel5.Visible = true; //参数5面板
        //            label16.Text = "插入字符"; //参数5说明
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 7:
        //            label_headTip.Text = "批量替换文件(夹)名称固定位数字符";
        //            label__paramDescription1.Text = "起始位(数字)";
        //            label_paramDescription2.Text = "替换字符数";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = true;
        //            panel3.Visible = true;
        //            panel9.Visible = true;
        //            panel5.Visible = true; //参数5面板
        //            label16.Text = "替换后缀"; //参数5说明
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 8:
        //            label_headTip.Text = "批量替换文件(夹)名称的指定字符";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = false;
        //            panel3.Visible = false;
        //            panel9.Visible = true;
        //            panel5.Visible = true; //参数5面板
        //            label16.Text = "原字符"; //参数5说明
        //            panel6.Visible = true; //参数6面板
        //            label18.Text = "替换字符"; //参数6说明
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 9:
        //            label_headTip.Text = "批量替换文件(夹)名称的后缀字符";
        //            label1_paramDescription4.Text = "保护后缀";
        //            panel2.Visible = false;
        //            panel3.Visible = false;
        //            panel9.Visible = true;//保护后缀
        //            panel5.Visible = true; //参数5面板
        //            label16.Text = "替换后缀"; //参数5说明
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //        case 10:
        //            label_headTip.Text = "批量删除(移动)指定名称的文件(夹),选此功能时,参数5支持正则表达式";
        //            label1_paramDescription4.Text = "保护该后缀文件不被删";
        //            checkBox_protectSuffix.Checked = false;
        //            panel2.Visible = false;
        //            panel3.Visible = false;
        //            panel9.Visible = true;//保护后缀
        //            panel5.Visible = false; //参数5面板
        //            panel6.Visible = false; //参数6面板
        //            label_param8.Text = "回收目录"; //参数8说明
        //            panel10.Visible = true; //参数8面板
        //            checkBox_specialStr.Checked = true;//选择文件移动删除功能时默认开启正则
        //            break;
        //        default:
        //            label_headTip.Text = "功能未选择！";
        //            panel2.Visible = false;
        //            panel3.Visible = false;
        //            panel5.Visible = false;
        //            panel9.Visible = false;
        //            panel6.Visible = false; //参数6面板
        //            panel10.Visible = false; //参数8面板
        //            break;
        //    }
        //}

        ///// <summary>
        ///// 工作文本变化时检查并修正
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void checkBox7_CheckedChanged()
        //{
        //    if (textBox_workFilePath.Text == "")
        //    {
        //        textBox_workFilePath.Text = AppDomain.CurrentDomain.BaseDirectory + "temp.txt";
        //    }
        //    else if (
        //        checkBox_printTXTOnly.Checked
        //        && (
        //            !textBox_workFilePath.Text.Contains(@".txt")
        //            || !Regex.IsMatch(textBox_workFilePath.Text, @"^(.*)(\.txt)$")
        //        )
        //    )
        //    {
        //        textBox_workFilePath.Text += @".txt";
        //    }
        //}

        ///// <summary>
        ///// 功能1_对删除字符起始位置进行检查并提示
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void textBox4_TextChanged()
        //{
        //    if (!MMCore.IsNumeric(textBox_param1.Text))
        //    {
        //        textBox_param1.Text = "请重填！";
        //    }
        //    else
        //    {
        //        StartIndex = Convert.ToInt32(textBox_param1.Text);
        //        if (StartIndex < 0)
        //        {
        //            textBox_param1.Text = "请重填！";
        //        }
        //    }
        //}

        ///// <summary>
        ///// 功能1_对删除字符数检查并提示
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void textBox3_TextChanged()
        //{
        //    if (!MMCore.IsNumeric(textBox_param2.Text))
        //    {
        //        textBox_param2.Text = "请重填！";
        //    }
        //    else
        //    {
        //        StrCount = Convert.ToInt32(textBox_param2.Text);
        //        if (StrCount < 0)
        //        {
        //            textBox_param2.Text = "请重填！";
        //        }
        //    }
        //}

        ///// <summary>
        ///// 对检索后缀功能检查并重置
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void textBox5_TextChanged()
        //{
        //    if (textBox_param3.Text == "")
        //    {
        //        textBox_param3.Text = "*";
        //    }
        //}

        ///// <summary>
        ///// 用户快捷选择工作文档
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void comboBox2_SelectedIndexChanged()
        //{
        //    string temp;
        //    switch (comboBox_SelectWorkFilePath.SelectedIndex)
        //    {
        //        case 0: //文本>用户自定
        //            break;
        //        case 1: //文本>程序目录
        //            textBox_workFilePath.Text = AppDomain.CurrentDomain.BaseDirectory + @"temp.txt";
        //            break;
        //        case 2: //文本>工作目录(外)
        //            textBox_workFilePath.Text = textBox_workPath.Text;
        //            if (textBox_workFilePath.Text == "")
        //            {
        //                textBox_workFilePath.Text = AppDomain.CurrentDomain.BaseDirectory + "temp.txt";
        //            }
        //            else if (
        //                checkBox_printTXTOnly.Checked
        //                && (
        //                    !textBox_workFilePath.Text.Contains(@".txt")
        //                    || !Regex.IsMatch(textBox_workFilePath.Text, @"^(.*)(\.txt)$")
        //                )
        //            )
        //            {
        //                textBox_workFilePath.Text += @".txt";
        //            }
        //            break;
        //        case 3: //文本>工作目录(内)
        //            textBox_workFilePath.Text = textBox_workPath.Text;
        //            if (textBox_workFilePath.Text == "")
        //            {
        //                textBox_workFilePath.Text = AppDomain.CurrentDomain.BaseDirectory + "temp.txt";
        //            }
        //            else if (
        //                checkBox_printTXTOnly.Checked
        //                && (
        //                    !textBox_workFilePath.Text.Contains(@".txt")
        //                    || !Regex.IsMatch(textBox_workFilePath.Text, @"^(.*)(\.txt)$")
        //                )
        //            )
        //            {
        //                temp = textBox_workFilePath.Text.Substring(textBox_workFilePath.Text.LastIndexOf("\\") + 1);
        //                textBox_workFilePath.Text += @"\" + temp + @".txt";
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //}

        ///// <summary>
        ///// 可点击的标签框提示
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void label11_Click()
        //{
        //    if (WorkStatus)
        //    {
        //        label_dirStatistics.Text = ProCount.ToString() + @"/" + ProCountMax.ToString();
        //        if (label_dirStatistics.Text == @"0/0")
        //        {
        //            label_dirStatistics.Text = "计算中...";
        //        }
        //    }
        //}

        ///// <summary>
        ///// 取消线程间的安全访问
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void Form1_Load()
        //{
        //    // 取消线程间的安全访问(最简单省事,但最不负责)
        //    CheckForIllegalCrossThreadCalls = false;
        //}

        ///// <summary>
        ///// 文件(夹)检索大小(Min)
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void textBox6_TextChanged()
        //{
        //    if (!MMCore.IsNumeric(textBox_rangeMin.Text))
        //    {
        //        textBox_rangeMin.Text = "文件(夹)检索大小(Min)";
        //    }
        //}

        ///// <summary>
        ///// 文件(夹)检索大小(Max)
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void textBox7_TextChanged()
        //{
        //    if (!MMCore.IsNumeric(textBox_rangeMax.Text))
        //    {
        //        textBox_rangeMax.Text = "文件(夹)检索大小(Max)";
        //    }
        //}

        //private void checkBox13_CheckedChanged()
        //{
        //    if (checkBox_protectSuffix.Checked)
        //    {
        //        textBox_param4.Text = "";
        //    } //勾选时,设置参数4为空(即保护任意后缀)
        //    else
        //    {
        //        textBox_param4.Text = "*";
        //    }
        //}

        //private void checkBox20_CheckedChanged()
        //{
        //    if (checkBox_printDirSize.Checked)
        //    {
        //        label_dirStatistics.Text = "你选择了文件夹递归统计大小,目前非常耗时哦！";
        //    }
        //    else
        //    {
        //        label_dirStatistics.Text = "";
        //    }
        //}

        //private void checkBox25_CheckedChanged()
        //{
        //    if (checkBox_dirModification.Checked)
        //    {
        //        checkBox_recursion.Checked = false;
        //        label_dirStatistics.Text = "目前文件夹修改时不支持遍历子文件夹哦！";
        //    }
        //}

        //private void checkBox10_CheckedChanged()
        //{
        //    if (checkBox_recursion.Checked)
        //    {
        //        if (checkBox_dirModification.Checked)
        //        {
        //            checkBox_dirModification.Checked = false;
        //            label_dirStatistics.Text = "目前遍历子文件夹时不支持文件夹修改哦！";
        //        }
        //        else
        //        {

        //            if (comboBox_selectFunc.SelectedIndex == 0) { checkBox_batMethod.Visible = true; label_dirStatistics.Text = "遍历子文件夹很耗时！若只想打印文件(夹)名称,可勾选BAT方式！"; }
        //            else
        //            {
        //                checkBox_batMethod.Checked = false;
        //                checkBox_batMethod.Visible = false; label_dirStatistics.Text = "遍历子文件夹会增加耗时！";
        //            }
        //        }
        //    }
        //    else
        //    {
        //        label_dirStatistics.Text = "";
        //        checkBox_batMethod.Checked = false;
        //        checkBox_batMethod.Visible = false;
        //    }
        //}

        //private void textBox9_TextChanged() { }

        //private void textBox10_TextChanged() { }

        //private void textBox8_TextChanged()
        //{
        //    if (textBox_param4.Text == "*")
        //    {
        //        checkBox_protectSuffix.Checked = false;
        //    }
        //    else if (textBox_param4.Text == "")
        //    {
        //        checkBox_protectSuffix.Checked = true;
        //    }
        //}

        //private void button4_Click()
        //{
        //    FolderBrowserDialog fbd = new FolderBrowserDialog();
        //    fbd.ShowDialog();
        //    string path = fbd.SelectedPath;
        //    textBox_param8.Text = path;
        //}

        //private void comboBox4_SelectedIndexChanged()
        //{
        //    switch (comboBox_param8.SelectedIndex)
        //    {
        //        case 0:
        //            textBox_param8.Enabled = true;
        //            button_param8.Enabled = true;
        //            break;
        //        case 1:
        //            textBox_param8.Enabled = false;
        //            button_param8.Enabled = false;
        //            break;
        //        case 2:
        //            textBox_param8.Enabled = false;
        //            button_param8.Enabled = false;
        //            break;
        //        default:
        //            break;

        //    }
        //}
        //private void checkBox36_CheckedChanged()
        //{
        //    if (checkBox_regular.Checked) { checkBox_specialStrIgnoreCase.Visible = false; } else { checkBox_specialStrIgnoreCase.Visible = true; }
        //}

        //private void checkBox38_CheckedChanged()
        //{
        //    if (checkBox_specialStr.Checked) { panel11.Visible = true; } else { panel11.Visible = false; }
        //}

        //private void checkBox_range_CheckedChanged()
        //{
        //    if (!checkBox_range.Checked) { panel8.Visible = false; }
        //    else { panel8.Visible = true; }
        //}

        //#endregion

        //#region Functions

        ///// <summary>
        ///// 参数1和2是数字
        ///// </summary>
        ///// <returns></returns>
        //private bool Param1And2IsNumeric()
        //{
        //    bool torf = true;
        //    if (!MMCore.IsNumeric(textBox_param2.Text))
        //    {
        //        textBox_param2.Text = "请重填！";
        //        torf = false;
        //    }
        //    else
        //    {
        //        StrCount = Convert.ToInt32(textBox_param2.Text);
        //        if (StrCount < 0)
        //        {
        //            textBox_param2.Text = "请重填！";
        //            torf = false;
        //        }
        //    }
        //    if (!MMCore.IsNumeric(textBox_param1.Text))
        //    {
        //        textBox_param1.Text = "请重填！";
        //        torf = false;
        //    }
        //    else
        //    {
        //        StartIndex = Convert.ToInt32(textBox_param1.Text);
        //        if (StartIndex < 0)
        //        {
        //            textBox_param1.Text = "请重填！";
        //            torf = false;
        //        }
        //    }
        //    return torf;
        //}

        ///// <summary>
        ///// 验证检索文件大小是否在用户定义范围(递归耗时)
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //private bool ParamIsInRange(string path)
        //{
        //    bool torf = false;
        //    long pixelX;
        //    long a;
        //    long b;
        //    for (int i = 0; i < 1; i++)
        //    {
        //        if (MMCore.IsNumeric(textBox_rangeMin.Text) && MMCore.IsNumeric(textBox_rangeMax.Text))
        //        {
        //            a = long.Parse(textBox_rangeMin.Text);
        //            b = long.Parse(textBox_rangeMax.Text);
        //            if (MMCore.IsDir(path))
        //            {
        //                pixelX = MMCore.GetDirectoryLength(path);
        //            }
        //            else
        //            {
        //                pixelX = MMCore.GetFileLength(path);
        //            }
        //            if (pixelX >= a && pixelX < b)
        //            {
        //                torf = true;
        //            }
        //        }
        //    }
        //    return torf;
        //}

        ///// <summary>
        ///// 首次写入文件时标记卷名卷号
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="diskSymbol">工作目录盘符</param>
        //private void FirstWrite(string workFilePath, string diskSymbol)
        //{
        //    bool torf;
        //    if (checkBox_keepHistory.Checked)
        //    {
        //        torf = true;
        //    }
        //    else
        //    {
        //        torf = false;
        //    }
        //    DriveInfo drive = new DriveInfo(diskSymbol);
        //    if (drive.Name == $"{diskSymbol}" + @":\")
        //    {
        //        if (checkBox_printDiskName.Checked)
        //        {
        //            MMCore.WriteLine(
        //                workFilePath,
        //                " Volume in drive " + diskSymbol + " is " + drive.VolumeLabel,
        //                torf
        //            );
        //        }

        //        if (checkBox_printDiskSymbol.Checked)
        //        {
        //            MMCore.WriteLine(
        //                workFilePath,
        //                " Volume Serial Number is " + MMCore.GetHardDiskID(diskSymbol),
        //                true
        //            );
        //        }
        //        MMCore.WriteLine(workFilePath, "", true);
        //    }

        //}

        //private void UserOpEnableChange(bool torf)
        //{
        //    UserOpEnable = torf;
        //    foreach (Control a in Controls)
        //    {
        //        if (a is Panel)
        //        {
        //            Panel p = a as Panel;  //取出Panel
        //            if (!torf)
        //            {
        //                // 改变Panel的颜色,执行时是灰色
        //                p.BackColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        //            }
        //            else
        //            {
        //                //不执行时是白色(背景完全透明)
        //                p.BackColor = Color.white;
        //            }
        //            foreach (Control c in p.Controls) //遍历面板中的每一个控件
        //            {
        //                if (c.GetType().Name.Equals("TextBox"))
        //                {
        //                    c.Enabled = torf;//禁用文本框
        //                }
        //                if (c.GetType().Name.Equals("CheckBox"))
        //                {
        //                    c.Enabled = torf;//禁用复选框
        //                }
        //                if (c.GetType().Name.Equals("ComboBox"))
        //                {
        //                    c.Enabled = torf;//禁用下拉框
        //                }
        //                if (c.GetType().Name.Equals("Button"))
        //                {
        //                    if (!c.Name.Equals("button2"))
        //                    {
        //                        c.Enabled = torf;//禁用除运行按钮外的其他按钮
        //                    }
        //                }

        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 点击执行按钮
        ///// </summary>
        //private void ButtonRun()
        //{
        //    string workPath;
        //    string workFilePath;
        //    string diskSymbol;

        //    for (int i = 0; i < 1; i++)
        //    {
        //        workPath = textBox_workPath.Text;
        //        workFilePath = textBox_workFilePath.Text;

        //        switch (comboBox_selectFunc.SelectedIndex)
        //        {
        //            case 0:
        //                label_headTip.Text = "批量将文件(夹)名称打印到工作文件";
        //                break;
        //            case 1:
        //                label_headTip.Text = "批量将文件(夹)名称去除固定位数字符";
        //                break;
        //            case 2:
        //                label_headTip.Text = "批量将文件(夹)名称保留固定位数字符";
        //                break;
        //            case 3:
        //                label_headTip.Text = "批量插入字符到文件(夹)名称固定位数字符前";
        //                break;
        //            case 4:
        //                label_headTip.Text = "批量插入字符到文件(夹)名称固定位数字符后";
        //                break;
        //            case 5:
        //                label_headTip.Text = "批量插入字符到文件(夹)名称最前";
        //                break;
        //            case 6:
        //                label_headTip.Text = "批量插入字符到文件(夹)名称最后";
        //                break;
        //            case 7:
        //                label_headTip.Text = "批量替换文件(夹)名称固定位数字符";
        //                break;
        //            case 8:
        //                label_headTip.Text = "批量替换文件(夹)名称的指定字符";
        //                break;
        //            case 9:
        //                label_headTip.Text = "批量替换文件(夹)名称的后缀字符";
        //                break;
        //            case 10:
        //                label_headTip.Text = "批量删除(移动)指定名称的文件(夹)";
        //                break;
        //            default:
        //                label_headTip.Text = "功能未选择！";
        //                break;
        //        }
        //        if ((label_headTip.Text == "功能未选择！") || (comboBox_selectFunc.SelectedIndex == -1))
        //        {
        //            break;
        //        }
        //        if (!Directory.Exists(workPath))
        //        {
        //            label_headTip.Text = "工作目录无效！";
        //            break; //调用本函数的动作有验证工作目录路径字符,通过后执行到此处再次验证有无此文件夹,无则打断
        //        }
        //        else
        //        {
        //            diskSymbol = workPath.Substring(0, 1);
        //        }
        //        //生成指定文本到程序目录的情况:1)工作文本为空；2)工作文本路径已存在且未允许覆盖；3)勾选仅输出.txt但工作文本格式错误,虽不为空但后缀非.txt；4)文件路径非法.
        //        if (
        //            workFilePath == ""
        //            || (File.Exists(workFilePath) && !checkBox_overlayWorkFile.Checked)
        //            || (Regex.IsMatch(workFilePath, @"^(.*)(\.txt)$") && !checkBox_printTXTOnly.Checked)
        //            || !MMCore.IsDFPath(workFilePath)
        //        )
        //        {
        //            //工作文本路径错误,重置为系统默认
        //            workFilePath = AppDomain.CurrentDomain.BaseDirectory + @"temp.txt";
        //            textBox_workFilePath.Text = workFilePath;
        //            label_headTip.Text = "工作文本路径错误,按系统默认输出！";
        //        }
        //        Stopwatch stopwatch = new Stopwatch();
        //        stopwatch.Start();
        //        DFRun(workPath, workFilePath, diskSymbol);
        //        stopwatch.Stop();
        //        //MMCore.WriteLine(workFilePath, stopwatch.Elapsed.ToString(), true);
        //        if (checkBox40.Checked) { label_dirStatistics.Text += " 时耗 => " + stopwatch.Elapsed.ToString(); }
        //    }
        //    //放弃了线程注销做法,程序将始终运行至此,可以知道是用户中断还是正常运行结束
        //    DirCount = 0;//影响打印的目录是否换行
        //    WorkStatus = false;//重置工作状态
        //    if (WorkStop) { label_dirStatistics.Text = "用户取消！"; }
        //    WorkStop = false;//重置_workStop状态,若是用户取消的,打印告知
        //    WorkErrCount = 0;//重置错误计数
        //    UserOpEnableChange(true);//重置用户操作状态
        //    button_outsideSave.Text = "执行";
        //    //WorkThread.Abort();
        //}

        ///// <summary>
        ///// 按照功能编号去选择执行检索文件后的动作
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction(string workFilePath, string item)
        //{
        //    switch (comboBox_selectFunc.SelectedIndex)
        //    {
        //        case 1:
        //            DAction_f1(workFilePath, item);
        //            break;
        //        case 2:
        //            DAction_f2(workFilePath, item);
        //            break;
        //        case 3:
        //            DAction_f3456(workFilePath, item);
        //            break;
        //        case 4:
        //            DAction_f3456(workFilePath, item);
        //            break;
        //        case 5:
        //            DAction_f3456(workFilePath, item);
        //            break;
        //        case 6:
        //            DAction_f3456(workFilePath, item);
        //            break;
        //        case 7:
        //            DAction_f7(workFilePath, item);
        //            break;
        //        case 8:
        //            DAction_f8(workFilePath, item);
        //            break;
        //        case 9:
        //            DAction_f9(workFilePath, item);
        //            break;
        //        case 10:
        //            DAction_f10(workFilePath, item);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        ///// <summary>
        ///// 功能编号01,将文件(夹)名称去除固定位数字符
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction_f1(string workFilePath, string item)
        //{
        //    string dirName;
        //    string itemName;
        //    string itemFrontName;
        //    string itemExtension;
        //    string newFileName;
        //    string newFilePath = "";
        //    string reportPath;
        //    bool isDir = false;
        //    for (int i = 0; i < 1; i++)
        //    {
        //        dirName = item.Substring(0, item.LastIndexOf("\\"));
        //        itemName = item.Substring(item.LastIndexOf("\\") + 1);
        //        if (MMCore.IsDir(item))
        //        {
        //            //itemFrontName = Path.GetDirectoryName(item);//返回的是上一级文件夹名称
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //            isDir = true;
        //        }
        //        else
        //        {
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //        }
        //        itemExtension = Path.GetExtension(item);
        //        if (itemExtension == "")
        //        {
        //            if (checkBox_emptySuffixFileIgnore.Checked && !isDir) { break; }//忽略空后缀文件
        //            if (checkBox_emptySuffixDirIgnore.Checked && isDir) { break; }//忽略空后缀文件夹
        //        }
        //        else
        //        {
        //            if (checkBox_nonEmptySuffixFileIgnore.Checked && !isDir) { break; }//忽略非空后缀文件
        //            if (checkBox_nonEmptySuffixDirIgnore.Checked && isDir) { break; }//忽略非空后缀文件夹
        //        }
        //        reportPath =
        //            workFilePath.Substring(0, workFilePath.LastIndexOf("\\"))
        //            + @"\"
        //            + Path.GetFileNameWithoutExtension(workFilePath)
        //            + @"_Report.txt";
        //        if (Param1And2IsNumeric() && textBox_param2.Text != "" && textBox_param1.Text != "" && StrCount > 0)
        //        {
        //            if (itemName.Length > (StrCount + StartIndex)) //文件名长度足以支撑修改,8>(0+7)或(1+6),至少留一个字符
        //            {
        //                if (checkBox_protectSuffix.Checked && textBox_param4.Text != "*") //指定后缀且勾选后缀保护
        //                {
        //                    if (
        //                        textBox_param4.Text == ""
        //                        || (
        //                            string.Compare(
        //                                Path.GetExtension(item),
        //                                textBox_param4.Text,
        //                                checkBox_param4.Checked
        //                            ) == 0
        //                        )
        //                    )
        //                    {
        //                        //勾选保护后缀时的处理
        //                        if (itemFrontName.Length >= (StrCount + StartIndex)) //前缀允许全部删除
        //                        {
        //                            newFileName =
        //                                itemFrontName.Remove(StartIndex, StrCount) + itemExtension;
        //                            newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                        }
        //                        else
        //                        {
        //                            //预判出错文件并汇报,忽略这些文件的重命名,此时newFilePath=""
        //                            if (WorkErrCount == 0)
        //                            {
        //                                MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                            }
        //                            WorkErrCount += 1;
        //                            MMCore.WriteLine(reportPath, "前缀字符数不足以修改:" + item, true);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //不保护后缀(或参数4默认填了*)
        //                    newFileName = itemName.Remove(StartIndex, StrCount);
        //                    newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                }
        //                if (
        //                    newFilePath != ""
        //                    && !File.Exists(newFilePath)
        //                    && !Directory.Exists(newFilePath)
        //                )
        //                {
        //                    //对非空newFilePath且不会发生覆盖的文件,执行重命名
        //                    if (!MMCore.IsDir(item))
        //                    {
        //                        FileInfo fileInfo = new FileInfo(item);
        //                        fileInfo.MoveTo(newFilePath);
        //                    }
        //                    else
        //                    {
        //                        DirectoryInfo directoryInfo = new DirectoryInfo(item);
        //                        directoryInfo.MoveTo(newFilePath);
        //                    }
        //                    MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //                }
        //                else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //                {
        //                    //newFilePath="",说明出现预判出错文件且汇报过,现在仅对预判重叠文件进行汇报
        //                    if (WorkErrCount == 0)
        //                    {
        //                        MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                    }
        //                    WorkErrCount += 1;
        //                    MMCore.WriteLine(reportPath, "预判到路径会重叠文件:" + item, true);
        //                }
        //            }
        //            else
        //            {
        //                if (WorkErrCount == 0)
        //                {
        //                    MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                }
        //                WorkErrCount += 1;
        //                MMCore.WriteLine(reportPath, "文件名长度不足以扣减:" + item, true);
        //            }

        //            if (WorkErrCount > 0)
        //            {
        //                label_headTip.Text = $"{WorkErrCount}个文件处理失败！请查看Report文件";
        //            }
        //            //测试↓
        //            //MMCore.WriteLine( reportPath, "item = " + item, true);
        //            //MMCore.WriteLine( reportPath, "dirName = " + dirName, true);
        //            //MMCore.WriteLine( reportPath, "itemName = " + itemName, true);
        //            //MMCore.WriteLine( reportPath, "itemFrontName = " + itemFrontName, true);
        //            //MMCore.WriteLine( reportPath, "itemExtension = " + itemExtension, true);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 功能编号02,将文件(夹)名称保留固定位数字符
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction_f2(string workFilePath, string item)
        //{
        //    string dirName;
        //    string itemName;
        //    string itemFrontName;
        //    string itemExtension;
        //    string newFileName;
        //    string newFilePath = "";
        //    string reportPath;
        //    bool isDir = false;
        //    for (int i = 0; i < 1; i++)
        //    {
        //        dirName = item.Substring(0, item.LastIndexOf("\\"));
        //        itemName = item.Substring(item.LastIndexOf("\\") + 1);
        //        if (MMCore.IsDir(item))
        //        {
        //            //itemFrontName = Path.GetDirectoryName(item);
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //            isDir = true;
        //        }
        //        else
        //        {
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //        }
        //        itemExtension = Path.GetExtension(item);
        //        if (itemExtension == "")
        //        {
        //            if (checkBox_emptySuffixFileIgnore.Checked && !isDir) { break; }//忽略空后缀文件
        //            if (checkBox_emptySuffixDirIgnore.Checked && isDir) { break; }//忽略空后缀文件夹
        //        }
        //        else
        //        {
        //            if (checkBox_nonEmptySuffixFileIgnore.Checked && !isDir) { break; }//忽略非空后缀文件
        //            if (checkBox_nonEmptySuffixDirIgnore.Checked && isDir) { break; }//忽略非空后缀文件夹
        //        }
        //        reportPath =
        //            workFilePath.Substring(0, workFilePath.LastIndexOf("\\"))
        //            + @"\"
        //            + Path.GetFileNameWithoutExtension(workFilePath)
        //            + @"_Report.txt";
        //        if (Param1And2IsNumeric() && StrCount > 0)
        //        {
        //            if (itemName.Length > (StrCount + StartIndex)) //文件名(带后缀)长度足以支撑修改
        //            {
        //                if (checkBox_protectSuffix.Checked && textBox_param4.Text != "*") //指定后缀且勾选后缀保护
        //                {
        //                    if (
        //                        textBox_param4.Text == ""
        //                        || (
        //                            string.Compare(
        //                                Path.GetExtension(item),
        //                                textBox_param4.Text,
        //                                checkBox_param4.Checked
        //                            ) == 0
        //                        )
        //                    )
        //                    {
        //                        //勾选保护后缀时的处理
        //                        if (itemFrontName.Length >= (StrCount + StartIndex)) //假设ABCD.txt, 8>=(0+8)或8>=(1+7)
        //                        {
        //                            if (StartIndex == 0)
        //                            {
        //                                newFileName =
        //                                    itemFrontName.Substring(StartIndex, StrCount)
        //                                    + itemExtension;
        //                            }
        //                            else
        //                            {
        //                                newFileName =
        //                                    itemFrontName.Substring(StartIndex, StrCount)
        //                                    + itemExtension;
        //                            }
        //                            newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                        }
        //                        else
        //                        {
        //                            //预判出错文件并汇报,忽略这些文件的重命名,此时newFilePath=""
        //                            if (WorkErrCount == 0)
        //                            {
        //                                MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                            }
        //                            WorkErrCount += 1;
        //                            MMCore.WriteLine(reportPath, "前缀字符数不足以修改:" + item, true);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //不保护后缀(或参数4默认填了*)
        //                    if (StartIndex == 0)
        //                    {
        //                        newFileName = itemName.Remove(StrCount);
        //                    }
        //                    else
        //                    {
        //                        newFileName = itemName.Remove(0, StartIndex);
        //                        newFileName = newFileName.Remove(StrCount - StartIndex);
        //                    }
        //                    newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                }
        //                if (
        //                    newFilePath != ""
        //                    && !File.Exists(newFilePath)
        //                    && !Directory.Exists(newFilePath)
        //                )
        //                {
        //                    //对非空newFilePath且不会发生覆盖的文件,执行重命名
        //                    if (!MMCore.IsDir(item))
        //                    {
        //                        FileInfo fileInfo = new FileInfo(item);
        //                        fileInfo.MoveTo(newFilePath);
        //                    }
        //                    else
        //                    {
        //                        DirectoryInfo directoryInfo = new DirectoryInfo(item);
        //                        directoryInfo.MoveTo(newFilePath);
        //                    }
        //                    MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //                }
        //                else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //                {
        //                    //newFilePath="",说明出现预判出错文件且汇报过,现在仅对预判重叠文件进行汇报
        //                    if (WorkErrCount == 0)
        //                    {
        //                        MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                    }
        //                    WorkErrCount += 1;
        //                    MMCore.WriteLine(reportPath, "预判到路径会重叠文件:" + item, true);
        //                }
        //            }
        //            else
        //            {
        //                if (WorkErrCount == 0)
        //                {
        //                    MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                }
        //                WorkErrCount += 1;
        //                MMCore.WriteLine(reportPath, "文件名长度不足以扣减:" + item, true);
        //            }

        //            if (WorkErrCount > 0)
        //            {
        //                label_headTip.Text = $"{WorkErrCount}个文件处理失败！请查看Report文件";
        //            }
        //            //测试↓
        //            //MMCore.WriteLine( reportPath, "item = "+ item, true);
        //            //MMCore.WriteLine( reportPath, "dirName = " + dirName, true);
        //            //MMCore.WriteLine( reportPath, "itemName = " + itemName, true);
        //            //MMCore.WriteLine( reportPath, "itemFrontName = " + itemFrontName, true);
        //            //MMCore.WriteLine( reportPath, "itemExtension = " + itemExtension, true);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 功能编号03~06,插入字符到文件(夹)名称固定位数字符前后或名称最前最后
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction_f3456(string workFilePath, string item)
        //{
        //    string dirName;
        //    string itemName;
        //    string itemFrontName;
        //    string itemExtension;
        //    string newFileName = "";
        //    string newFilePath = "";
        //    string reportPath;
        //    string tempFrontName;
        //    string tempMidName;
        //    string tempEndName;
        //    bool torf = false;
        //    bool isDir = false;
        //    //固定功能5和6的参数
        //    switch (comboBox_selectFunc.SelectedIndex)
        //    {
        //        case 5:
        //            StartIndex = 0;
        //            StrCount = 1;
        //            torf = true;
        //            break;
        //        case 6:
        //            StartIndex = 0;
        //            StrCount = 1;
        //            torf = true;
        //            break;
        //        default:
        //            break;
        //    }

        //    for (int i = 0; i < 1; i++)
        //    {
        //        dirName = item.Substring(0, item.LastIndexOf("\\"));
        //        itemName = item.Substring(item.LastIndexOf("\\") + 1);
        //        if (MMCore.IsDir(item))
        //        {
        //            //itemFrontName = Path.GetDirectoryName(item); //返回的是上一级文件夹名称
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //            isDir = true;
        //        }
        //        else
        //        {
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //        }
        //        itemExtension = Path.GetExtension(item);
        //        if (itemExtension == "")
        //        {
        //            if (checkBox_emptySuffixFileIgnore.Checked && !isDir) { break; }//忽略空后缀文件
        //            if (checkBox_emptySuffixDirIgnore.Checked && isDir) { break; }//忽略空后缀文件夹
        //        }
        //        else
        //        {
        //            if (checkBox_nonEmptySuffixFileIgnore.Checked && !isDir) { break; }//忽略非空后缀文件
        //            if (checkBox_nonEmptySuffixDirIgnore.Checked && isDir) { break; }//忽略非空后缀文件夹
        //        }
        //        reportPath =
        //            workFilePath.Substring(0, workFilePath.LastIndexOf("\\"))
        //            + @"\"
        //            + Path.GetFileNameWithoutExtension(workFilePath)
        //            + @"_Report.txt";
        //        if ((torf || Param1And2IsNumeric()) && StrCount > 0)
        //        {
        //            if (itemName.Length >= (StrCount + StartIndex)) //文件名(带后缀)长度足以支撑修改
        //            {
        //                if (checkBox_protectSuffix.Checked && textBox_param4.Text != "*") //指定后缀且勾选后缀保护
        //                {
        //                    if (
        //                        textBox_param4.Text == ""
        //                        || (
        //                            string.Compare(
        //                                Path.GetExtension(item),
        //                                textBox_param4.Text,
        //                                checkBox_param4.Checked
        //                            ) == 0
        //                        )
        //                    )
        //                    {
        //                        //勾选保护后缀时的处理
        //                        if (itemFrontName.Length >= (StrCount + StartIndex)) //假设ABCD.txt, 8>=(0+8)或8>=(1+7)
        //                        {
        //                            if (StartIndex == 0)
        //                            {
        //                                tempFrontName = "";
        //                                tempMidName = itemFrontName.Substring(0, StrCount);
        //                                if (itemFrontName.Length == StrCount)
        //                                {
        //                                    tempEndName = "";
        //                                }
        //                                else
        //                                {
        //                                    tempEndName = itemFrontName.Substring(StrCount);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                tempFrontName = itemFrontName.Substring(0, StartIndex);
        //                                tempMidName = itemFrontName.Substring(StartIndex, StrCount);
        //                                if (itemFrontName.Length > (StrCount + StartIndex))
        //                                {
        //                                    //字符数有余,如8>(1+6)的情况
        //                                    tempEndName = itemFrontName.Substring(StrCount + 1);
        //                                }
        //                                else
        //                                {
        //                                    //字符数正好,如8=(1+7)的情况
        //                                    tempEndName = "";
        //                                }
        //                            }
        //                            switch (comboBox_selectFunc.SelectedIndex)
        //                            {
        //                                case 3:
        //                                    newFileName =
        //                                        tempFrontName
        //                                        + textBox_param5.Text
        //                                        + tempMidName
        //                                        + tempEndName
        //                                        + itemExtension;
        //                                    break;
        //                                case 4:
        //                                    newFileName =
        //                                        tempFrontName
        //                                        + tempMidName
        //                                        + textBox_param5.Text
        //                                        + tempEndName
        //                                        + itemExtension;
        //                                    break;
        //                                case 5:
        //                                    newFileName =
        //                                        textBox_param5.Text
        //                                        + tempFrontName
        //                                        + tempMidName
        //                                        + tempEndName
        //                                        + itemExtension;
        //                                    break;
        //                                case 6:
        //                                    newFileName =
        //                                        tempFrontName
        //                                        + tempMidName
        //                                        + tempEndName
        //                                        + textBox_param5.Text
        //                                        + itemExtension;
        //                                    break;
        //                                default:
        //                                    break;
        //                            }
        //                            newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                        }
        //                        else
        //                        {
        //                            //预判出错文件并汇报,忽略这些文件的重命名,此时newFilePath=""
        //                            if (WorkErrCount == 0)
        //                            {
        //                                MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                            }
        //                            WorkErrCount += 1;
        //                            MMCore.WriteLine(reportPath, "固定字符数超过前缀字符数:" + item, true);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //不保护后缀(或参数4默认填了*)
        //                    if (StartIndex == 0)
        //                    {
        //                        tempFrontName = "";
        //                        tempMidName = itemName.Substring(0, StrCount);
        //                        if (itemName.Length == StrCount)
        //                        {
        //                            tempEndName = "";
        //                        }
        //                        else
        //                        {
        //                            tempEndName = itemName.Substring(StrCount);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        tempFrontName = itemName.Substring(0, StartIndex);
        //                        tempMidName = itemName.Substring(StartIndex, StrCount);
        //                        if (itemName.Length > (StrCount + StartIndex))
        //                        {
        //                            //字符数有余,如8>(1+6)的情况
        //                            tempEndName = itemName.Substring(StrCount + 1);
        //                        }
        //                        else
        //                        {
        //                            //字符数正好,如8=(1+7)的情况
        //                            tempEndName = "";
        //                        }
        //                    }
        //                    switch (comboBox_selectFunc.SelectedIndex)
        //                    {
        //                        case 3:
        //                            newFileName =
        //                                tempFrontName + textBox_param5.Text + tempMidName + tempEndName;
        //                            break;
        //                        case 4:
        //                            newFileName =
        //                                tempFrontName + tempMidName + textBox_param5.Text + tempEndName;
        //                            break;
        //                        case 5:
        //                            newFileName =
        //                                textBox_param5.Text + tempFrontName + tempMidName + tempEndName;
        //                            break;
        //                        case 6:
        //                            newFileName =
        //                                tempFrontName + tempMidName + tempEndName + textBox_param5.Text;
        //                            break;
        //                        default:
        //                            break;
        //                    }
        //                    newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                }
        //                if (
        //                    newFilePath != ""
        //                    && !File.Exists(newFilePath)
        //                    && !Directory.Exists(newFilePath)
        //                )
        //                {
        //                    //对非空newFilePath且不会发生覆盖的文件,执行重命名
        //                    if (!MMCore.IsDir(item))
        //                    {
        //                        FileInfo fileInfo = new FileInfo(item);
        //                        fileInfo.MoveTo(newFilePath);
        //                    }
        //                    else
        //                    {
        //                        DirectoryInfo directoryInfo = new DirectoryInfo(item);
        //                        directoryInfo.MoveTo(newFilePath);
        //                    }
        //                    MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //                }
        //                else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //                {
        //                    //newFilePath="",说明出现预判出错文件且汇报过,现在仅对预判重叠文件进行汇报
        //                    if (WorkErrCount == 0)
        //                    {
        //                        MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                    }
        //                    WorkErrCount += 1;
        //                    MMCore.WriteLine(reportPath, "预判到路径会重叠文件:" + item, true);
        //                }
        //            }
        //            else
        //            {
        //                if (WorkErrCount == 0)
        //                {
        //                    MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                }
        //                WorkErrCount += 1;
        //                MMCore.WriteLine(reportPath, "固定字符数超过对象字符数:" + item, true);
        //            }

        //            if (WorkErrCount > 0)
        //            {
        //                label_headTip.Text = $"{WorkErrCount}个文件处理失败！请查看Report文件";
        //            }
        //            //测试↓
        //            //MMCore.WriteLine( reportPath, "item = " + item, true);
        //            //MMCore.WriteLine( reportPath, "dirName = " + dirName, true);
        //            //MMCore.WriteLine( reportPath, "itemName = " + itemName, true);
        //            //MMCore.WriteLine( reportPath, "itemFrontName = " + itemFrontName, true);
        //            //MMCore.WriteLine( reportPath, "itemExtension = " + itemExtension, true);
        //            //MMCore.WriteLine( reportPath, "tempFrontName = " + tempFrontName, true);
        //            //MMCore.WriteLine( reportPath, "tempMidName = " + tempMidName, true);
        //            //MMCore.WriteLine( reportPath, "tempEndName = " + tempEndName, true);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 功能编号07,批量替换文件(夹)名称固定位数字符
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction_f7(string workFilePath, string item)
        //{
        //    string dirName;
        //    string itemName;
        //    string itemFrontName;
        //    string itemExtension;
        //    string newFileName;
        //    string newFilePath = "";
        //    string reportPath;
        //    string tempFrontName;
        //    //string tempMidName;
        //    string tempEndName;
        //    bool torf = false;
        //    bool isDir = false;
        //    for (int i = 0; i < 1; i++)
        //    {
        //        dirName = item.Substring(0, item.LastIndexOf("\\"));
        //        itemName = item.Substring(item.LastIndexOf("\\") + 1);
        //        if (MMCore.IsDir(item))
        //        {
        //            //itemFrontName = Path.GetDirectoryName(item); //返回的是上一级文件夹名称
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //            isDir = true;
        //        }
        //        else
        //        {
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //        }
        //        itemExtension = Path.GetExtension(item);
        //        if (itemExtension == "")
        //        {
        //            if (checkBox_emptySuffixFileIgnore.Checked && !isDir) { break; }//忽略空后缀文件
        //            if (checkBox_emptySuffixDirIgnore.Checked && isDir) { break; }//忽略空后缀文件夹
        //        }
        //        else
        //        {
        //            if (checkBox_nonEmptySuffixFileIgnore.Checked && !isDir) { break; }//忽略非空后缀文件
        //            if (checkBox_nonEmptySuffixDirIgnore.Checked && isDir) { break; }//忽略非空后缀文件夹
        //        }
        //        reportPath =
        //            workFilePath.Substring(0, workFilePath.LastIndexOf("\\"))
        //            + @"\"
        //            + Path.GetFileNameWithoutExtension(workFilePath)
        //            + @"_Report.txt";
        //        if (Param1And2IsNumeric() && StrCount > 0)
        //        {
        //            if (textBox_param5.Text == "" && (itemName.Length > (StrCount + StartIndex)))
        //            {
        //                torf = true;
        //            }
        //            else if (textBox_param5.Text != "" && (itemName.Length >= (StrCount + StartIndex)))
        //            {
        //                torf = true;
        //            }
        //            if (torf) //文件名(带后缀)长度足以支撑修改
        //            {
        //                if (checkBox_protectSuffix.Checked && textBox_param4.Text != "*") //指定后缀且勾选后缀保护
        //                {
        //                    if (
        //                        textBox_param4.Text == ""
        //                        || (
        //                            string.Compare(
        //                                Path.GetExtension(item),
        //                                textBox_param4.Text,
        //                                checkBox_param4.Checked
        //                            ) == 0
        //                        )
        //                    )
        //                    {
        //                        //勾选保护后缀时的处理
        //                        if (itemFrontName.Length >= (StrCount + StartIndex)) //假设ABCD.txt, 8>=(0+8)或8>=(1+7)
        //                        {
        //                            if (StartIndex == 0)
        //                            {
        //                                tempFrontName = "";
        //                                //tempMidName = itemFrontName.Substring(0, _strCount);
        //                                if (itemFrontName.Length == StrCount)
        //                                {
        //                                    tempEndName = "";
        //                                }
        //                                else
        //                                {
        //                                    tempEndName = itemFrontName.Substring(StrCount);
        //                                }
        //                            }
        //                            else
        //                            {
        //                                tempFrontName = itemFrontName.Substring(0, StartIndex);
        //                                //tempMidName = itemFrontName.Substring(_startIndex, _strCount);
        //                                if (itemFrontName.Length > (StrCount + StartIndex))
        //                                {
        //                                    //字符数有余,如8>(1+6)的情况
        //                                    tempEndName = itemFrontName.Substring(StrCount + 1);
        //                                }
        //                                else
        //                                {
        //                                    //字符数正好,如8=(1+7)的情况
        //                                    tempEndName = "";
        //                                }
        //                            }
        //                            newFileName =
        //                                tempFrontName + textBox_param5.Text + tempEndName + itemExtension;
        //                            newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                        }
        //                        else
        //                        {
        //                            //预判出错文件并汇报,忽略这些文件的重命名,此时newFilePath=""
        //                            if (WorkErrCount == 0)
        //                            {
        //                                MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                            }
        //                            WorkErrCount += 1;
        //                            MMCore.WriteLine(reportPath, "固定字符数超过前缀字符数:" + item, true);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //不保护后缀(或参数4默认填了*)
        //                    if (StartIndex == 0)
        //                    {
        //                        tempFrontName = "";
        //                        //tempMidName = itemName.Substring(0, _strCount);
        //                        if (itemName.Length == StrCount)
        //                        {
        //                            tempEndName = "";
        //                        }
        //                        else
        //                        {
        //                            tempEndName = itemName.Substring(StrCount);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        tempFrontName = itemName.Substring(0, StartIndex);
        //                        //tempMidName = itemName.Substring(_startIndex, _strCount);
        //                        if (itemName.Length > (StrCount + StartIndex))
        //                        {
        //                            //字符数有余,如8>(1+6)的情况
        //                            tempEndName = itemName.Substring(StrCount + 1);
        //                        }
        //                        else
        //                        {
        //                            //字符数正好,如8=(1+7)的情况
        //                            tempEndName = "";
        //                        }
        //                    }
        //                    newFileName = tempFrontName + textBox_param5.Text + tempEndName;
        //                    newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                }
        //                if (
        //                    newFilePath != ""
        //                    && !File.Exists(newFilePath)
        //                    && !Directory.Exists(newFilePath)
        //                )
        //                {
        //                    //对非空newFilePath且不会发生覆盖的文件,执行重命名
        //                    if (!MMCore.IsDir(item))
        //                    {
        //                        FileInfo fileInfo = new FileInfo(item);
        //                        fileInfo.MoveTo(newFilePath);
        //                    }
        //                    else
        //                    {
        //                        DirectoryInfo directoryInfo = new DirectoryInfo(item);
        //                        directoryInfo.MoveTo(newFilePath);
        //                    }
        //                    MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //                }
        //                else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //                {
        //                    //newFilePath="",说明出现预判出错文件且汇报过,现在仅对预判重叠文件进行汇报
        //                    if (WorkErrCount == 0)
        //                    {
        //                        MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                    }
        //                    WorkErrCount += 1;
        //                    MMCore.WriteLine(reportPath, "预判到路径会重叠文件:" + item, true);
        //                }
        //            }
        //            else
        //            {
        //                if (WorkErrCount == 0)
        //                {
        //                    MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                }
        //                WorkErrCount += 1;
        //                MMCore.WriteLine(reportPath, "固定字符数超过对象字符数:" + item, true);
        //            }

        //            if (WorkErrCount > 0)
        //            {
        //                label_headTip.Text = $"{WorkErrCount}个文件处理失败！请查看Report文件";
        //            }
        //            //测试↓
        //            //MMCore.WriteLine( reportPath, "item = " + item, true);
        //            //MMCore.WriteLine( reportPath, "dirName = " + dirName, true);
        //            //MMCore.WriteLine( reportPath, "itemName = " + itemName, true);
        //            //MMCore.WriteLine( reportPath, "itemFrontName = " + itemFrontName, true);
        //            //MMCore.WriteLine( reportPath, "itemExtension = " + itemExtension, true);
        //            //MMCore.WriteLine( reportPath, "tempFrontName = " + tempFrontName, true);
        //            //MMCore.WriteLine( reportPath, "tempMidName = " + tempMidName, true);
        //            //MMCore.WriteLine( reportPath, "tempEndName = " + tempEndName, true);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 功能编号08,批量替换文件(夹)名称的指定字符
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction_f8(string workFilePath, string item)
        //{
        //    string dirName;
        //    string itemName;
        //    string itemFrontName;
        //    string itemExtension;
        //    string newFileName;
        //    string newFilePath = "";
        //    string reportPath;
        //    //string tempFrontName;
        //    //string tempMidName;
        //    //string tempEndName;
        //    bool torf = false;
        //    bool isDir = false;
        //    //固定功能5和6的参数
        //    StartIndex = 0;
        //    StrCount = 1;

        //    for (int i = 0; i < 1; i++)
        //    {
        //        dirName = item.Substring(0, item.LastIndexOf("\\"));
        //        itemName = item.Substring(item.LastIndexOf("\\") + 1);
        //        if (MMCore.IsDir(item))
        //        {
        //            //itemFrontName = Path.GetDirectoryName(item); //返回的是上一级文件夹名称
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //            isDir = true;
        //        }
        //        else
        //        {
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //        }
        //        itemExtension = Path.GetExtension(item);

        //        if (itemExtension == "")
        //        {
        //            if (checkBox_emptySuffixFileIgnore.Checked && !isDir) { break; }//忽略空后缀文件
        //            if (checkBox_emptySuffixDirIgnore.Checked && isDir) { break; }//忽略空后缀文件夹
        //        }
        //        else
        //        {
        //            if (checkBox_nonEmptySuffixFileIgnore.Checked && !isDir) { break; }//忽略非空后缀文件
        //            if (checkBox_nonEmptySuffixDirIgnore.Checked && isDir) { break; }//忽略非空后缀文件夹
        //        }

        //        reportPath =
        //            workFilePath.Substring(0, workFilePath.LastIndexOf("\\"))
        //            + @"\"
        //            + Path.GetFileNameWithoutExtension(workFilePath)
        //            + @"_Report.txt";

        //        if (textBox_param5.Text == "" && (itemName.Length > (StrCount + StartIndex)))
        //        {
        //            torf = true;
        //        }
        //        else if (textBox_param5.Text != "" && (itemName.Length >= (StrCount + StartIndex)))
        //        {
        //            torf = true;
        //        }
        //        if (torf) //文件名(带后缀)长度足以支撑修改
        //        {
        //            if (checkBox_protectSuffix.Checked && textBox_param4.Text != "*") //指定后缀且勾选后缀保护
        //            {
        //                //如填了空格全保护或指定.txt后遇到.txt情况则不操作
        //                //若指定.txt后遇到.PNG情况,进行操作
        //                if (
        //                    string.Compare(
        //                        Path.GetExtension(item),
        //                        textBox_param4.Text,
        //                        checkBox_param4.Checked
        //                    ) != 0
        //                )
        //                {
        //                    //处理
        //                    if (itemFrontName.Length >= (StrCount + StartIndex)) //假设ABCD.txt, 8>=(0+8)或8>=(1+7)
        //                    {
        //                        //if (_startIndex == 0)
        //                        //{
        //                        //    tempFrontName = "";
        //                        //    tempMidName = itemFrontName.Substring(0, _strCount);
        //                        //    if (itemFrontName.Length == _strCount)
        //                        //    {
        //                        //        tempEndName = "";
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        tempEndName = itemFrontName.Substring(_strCount);
        //                        //    }
        //                        //}
        //                        //else
        //                        //{
        //                        //    tempFrontName = itemFrontName.Substring(0, _startIndex);
        //                        //    tempMidName = itemFrontName.Substring(_startIndex, _strCount);
        //                        //    if (itemFrontName.Length > (_strCount + _startIndex))
        //                        //    {
        //                        //        //字符数有余,如8>(1+6)的情况
        //                        //        tempEndName = itemFrontName.Substring(_strCount + 1);
        //                        //    }
        //                        //    else
        //                        //    {
        //                        //        //字符数正好,如8=(1+7)的情况
        //                        //        tempEndName = "";
        //                        //    }
        //                        //}
        //                        newFileName = itemFrontName.Replace(textBox_param5.Text, textBox_param6.Text) + itemExtension;
        //                        newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                    }
        //                    else
        //                    {
        //                        //预判出错文件并汇报,忽略这些文件的重命名,此时newFilePath=""
        //                        if (WorkErrCount == 0)
        //                        {
        //                            MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                        }
        //                        WorkErrCount += 1;
        //                        MMCore.WriteLine(reportPath, "固定字符数超过前缀字符数:" + item, true);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //不保护后缀(或参数4默认填了*)
        //                newFileName = itemName.Replace(textBox_param5.Text, textBox_param6.Text);
        //                newFilePath = Path.Combine(dirName + @"\", newFileName);
        //            }
        //            if (
        //                newFilePath != ""
        //                && !File.Exists(newFilePath)
        //                && !Directory.Exists(newFilePath)
        //            )
        //            {
        //                //对非空newFilePath且不会发生覆盖的文件,执行重命名
        //                if (!MMCore.IsDir(item))
        //                {
        //                    FileInfo fileInfo = new FileInfo(item);
        //                    fileInfo.MoveTo(newFilePath);
        //                }
        //                else
        //                {
        //                    DirectoryInfo directoryInfo = new DirectoryInfo(item);
        //                    directoryInfo.MoveTo(newFilePath);
        //                }
        //                MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //            }
        //            else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //            {
        //                //newFilePath="",说明出现预判出错文件且汇报过,现在仅对预判重叠文件进行汇报
        //                if (WorkErrCount == 0)
        //                {
        //                    MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                }
        //                WorkErrCount += 1;
        //                MMCore.WriteLine(reportPath, "预判到路径会重叠文件:" + item, true);
        //            }
        //        }
        //        else
        //        {
        //            if (WorkErrCount == 0)
        //            {
        //                MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //            }
        //            WorkErrCount += 1;
        //            MMCore.WriteLine(reportPath, "固定字符数超过对象字符数:" + item, true);
        //        }

        //        if (WorkErrCount > 0)
        //        {
        //            label_headTip.Text = $"{WorkErrCount}个文件处理失败！请查看Report文件";
        //        }
        //        //测试↓
        //        //MMCore.WriteLine( reportPath, "item = " + item, true);
        //        //MMCore.WriteLine( reportPath, "dirName = " + dirName, true);
        //        //MMCore.WriteLine( reportPath, "itemName = " + itemName, true);
        //        //MMCore.WriteLine( reportPath, "itemFrontName = " + itemFrontName, true);
        //        //MMCore.WriteLine( reportPath, "itemExtension = " + itemExtension, true);
        //        //MMCore.WriteLine( reportPath, "tempFrontName = " + tempFrontName, true);
        //        //MMCore.WriteLine( reportPath, "tempMidName = " + tempMidName, true);
        //        //MMCore.WriteLine( reportPath, "tempEndName = " + tempEndName, true);
        //    }
        //}

        ///// <summary>
        ///// 功能编号09,批量替换文件(夹)名称的后缀字符
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction_f9(string workFilePath, string item)
        //{
        //    string dirName;
        //    string itemName;
        //    string itemFrontName;
        //    string itemExtension;
        //    string newFileName;
        //    string newFilePath = "";
        //    string reportPath;
        //    string tempFrontName;
        //    string tempMidName;
        //    string tempEndName;
        //    bool torf = false;
        //    bool isDir = false;
        //    //固定功能5和6的参数
        //    StartIndex = 0;
        //    StrCount = 1;

        //    for (int i = 0; i < 1; i++)
        //    {
        //        dirName = item.Substring(0, item.LastIndexOf("\\"));
        //        itemName = item.Substring(item.LastIndexOf("\\") + 1);
        //        if (MMCore.IsDir(item))
        //        {
        //            //itemFrontName = Path.GetDirectoryName(item); //返回的是上一级文件夹名称
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //            isDir = true;
        //        }
        //        else
        //        {
        //            itemFrontName = Path.GetFileNameWithoutExtension(item);
        //        }
        //        itemExtension = Path.GetExtension(item);

        //        if (itemExtension == "")
        //        {
        //            if (checkBox_emptySuffixFileIgnore.Checked && !isDir) { break; }//忽略空后缀文件
        //            if (checkBox_emptySuffixDirIgnore.Checked && isDir) { break; }//忽略空后缀文件夹
        //        }
        //        else
        //        {
        //            if (checkBox_nonEmptySuffixFileIgnore.Checked && !isDir) { break; }//忽略非空后缀文件
        //            if (checkBox_nonEmptySuffixDirIgnore.Checked && isDir) { break; }//忽略非空后缀文件夹
        //        }

        //        reportPath =
        //            workFilePath.Substring(0, workFilePath.LastIndexOf("\\"))
        //            + @"\"
        //            + Path.GetFileNameWithoutExtension(workFilePath)
        //            + @"_Report.txt";

        //        if (textBox_param5.Text == "" && (itemName.Length > (StrCount + StartIndex)))
        //        {
        //            torf = true;
        //        }
        //        else if (textBox_param5.Text != "" && (itemName.Length >= (StrCount + StartIndex)))
        //        {
        //            torf = true;
        //        }
        //        if (torf) //文件名(带后缀)长度足以支撑修改
        //        {
        //            if (checkBox_protectSuffix.Checked && textBox_param4.Text != "*") //指定后缀且勾选后缀保护
        //            {
        //                //如填了空格全保护或指定.txt后遇到.txt情况则不操作
        //                //若指定.txt后遇到.PNG情况,进行操作
        //                if (
        //                    string.Compare(
        //                        Path.GetExtension(item),
        //                        textBox_param4.Text,
        //                        checkBox_param4.Checked
        //                    ) != 0
        //                )
        //                {
        //                    //处理
        //                    if (itemFrontName.Length >= (StrCount + StartIndex)) //假设ABCD.txt, 8>=(0+8)或8>=(1+7)
        //                    {
        //                        if (StartIndex == 0)
        //                        {
        //                            tempFrontName = "";
        //                            tempMidName = itemFrontName.Substring(0, StrCount);
        //                            if (itemFrontName.Length == StrCount)
        //                            {
        //                                tempEndName = "";
        //                            }
        //                            else
        //                            {
        //                                tempEndName = itemFrontName.Substring(StrCount);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            tempFrontName = itemFrontName.Substring(0, StartIndex);
        //                            tempMidName = itemFrontName.Substring(StartIndex, StrCount);
        //                            if (itemFrontName.Length > (StrCount + StartIndex))
        //                            {
        //                                //字符数有余,如8>(1+6)的情况
        //                                tempEndName = itemFrontName.Substring(StrCount + 1);
        //                            }
        //                            else
        //                            {
        //                                //字符数正好,如8=(1+7)的情况
        //                                tempEndName = "";
        //                            }
        //                        }
        //                        newFileName =
        //                            tempFrontName + tempMidName + tempEndName + textBox_param5.Text;
        //                        newFilePath = Path.Combine(dirName + @"\", newFileName);
        //                    }
        //                    else
        //                    {
        //                        //预判出错文件并汇报,忽略这些文件的重命名,此时newFilePath=""
        //                        if (WorkErrCount == 0)
        //                        {
        //                            MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                        }
        //                        WorkErrCount += 1;
        //                        MMCore.WriteLine(reportPath, "固定字符数超过前缀字符数:" + item, true);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //不保护后缀(或参数4默认填了*)
        //                newFileName = itemFrontName + textBox_param5.Text;
        //                newFilePath = Path.Combine(dirName + @"\", newFileName);
        //            }
        //            if (
        //                newFilePath != ""
        //                && !File.Exists(newFilePath)
        //                && !Directory.Exists(newFilePath)
        //            )
        //            {
        //                //对非空newFilePath且不会发生覆盖的文件,执行重命名
        //                if (!MMCore.IsDir(item))
        //                {
        //                    FileInfo fileInfo = new FileInfo(item);
        //                    fileInfo.MoveTo(newFilePath);
        //                }
        //                else
        //                {
        //                    DirectoryInfo directoryInfo = new DirectoryInfo(item);
        //                    directoryInfo.MoveTo(newFilePath);
        //                }
        //                MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //            }
        //            else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //            {
        //                //newFilePath="",说明出现预判出错文件且汇报过,现在仅对预判重叠文件进行汇报
        //                if (WorkErrCount == 0)
        //                {
        //                    MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                }
        //                WorkErrCount += 1;
        //                MMCore.WriteLine(reportPath, "预判到路径会重叠文件:" + item, true);
        //            }
        //        }
        //        else
        //        {
        //            if (WorkErrCount == 0)
        //            {
        //                MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //            }
        //            WorkErrCount += 1;
        //            MMCore.WriteLine(reportPath, "固定字符数超过对象字符数:" + item, true);
        //        }

        //        if (WorkErrCount > 0)
        //        {
        //            label_headTip.Text = $"{WorkErrCount}个文件处理失败！请查看Report文件";
        //        }
        //        //测试↓
        //        //MMCore.WriteLine( reportPath, "item = " + item, true);
        //        //MMCore.WriteLine( reportPath, "dirName = " + dirName, true);
        //        //MMCore.WriteLine( reportPath, "itemName = " + itemName, true);
        //        //MMCore.WriteLine( reportPath, "itemFrontName = " + itemFrontName, true);
        //        //MMCore.WriteLine( reportPath, "itemExtension = " + itemExtension, true);
        //        //MMCore.WriteLine( reportPath, "tempFrontName = " + tempFrontName, true);
        //        //MMCore.WriteLine( reportPath, "tempMidName = " + tempMidName, true);
        //        //MMCore.WriteLine( reportPath, "tempEndName = " + tempEndName, true);
        //    }
        //}

        ///// <summary>
        ///// 功能编号10,批量删除(移动)指定名称的文件(夹)
        ///// </summary>
        ///// <param name="workFilePath"></param>
        ///// <param name="item"></param>
        //private void DAction_f10(string workFilePath, string item)
        //{
        //    string dirName;
        //    string itemName;
        //    //string itemFrontName;
        //    string itemExtension;
        //    string newFileName;
        //    string newFilePath = "";
        //    string reportPath;
        //    bool isDir = false;
        //    FileInfo fileInfo;
        //    DirectoryInfo directoryInfo;

        //    for (int i = 0; i < 1; i++)
        //    {
        //        //dirName = item.Substring(0, item.LastIndexOf("\\"));
        //        itemName = item.Substring(item.LastIndexOf("\\") + 1);
        //        if (MMCore.IsDir(item))
        //        {
        //            //itemFrontName = Path.GetDirectoryName(item); //返回的是上一级文件夹名称
        //            //itemFrontName = Path.GetFileNameWithoutExtension(item);
        //            isDir = true;
        //        }
        //        else
        //        {
        //            //itemFrontName = Path.GetFileNameWithoutExtension(item);
        //        }
        //        itemExtension = Path.GetExtension(item);

        //        if (itemExtension == "")
        //        {
        //            if (checkBox_emptySuffixFileIgnore.Checked && !isDir) { break; }//忽略空后缀文件
        //            if (checkBox_emptySuffixDirIgnore.Checked && isDir) { break; }//忽略空后缀文件夹
        //        }
        //        else
        //        {
        //            if (checkBox_nonEmptySuffixFileIgnore.Checked && !isDir) { break; }//忽略非空后缀文件
        //            if (checkBox_nonEmptySuffixDirIgnore.Checked && isDir) { break; }//忽略非空后缀文件夹
        //        }

        //        reportPath =
        //            workFilePath.Substring(0, workFilePath.LastIndexOf("\\"))
        //            + @"\"
        //            + Path.GetFileNameWithoutExtension(workFilePath)
        //            + @"_Report.txt";



        //        if (checkBox_protectSuffix.Checked && textBox_param4.Text != "*") //指定后缀且勾选后缀保护
        //        {
        //            //如填了空格全保护或指定.txt后遇到.txt情况则不操作
        //            //若指定.txt后遇到.PNG情况,进行操作
        //            if (
        //                string.Compare(
        //                    Path.GetExtension(item),
        //                    textBox_param4.Text,
        //                    checkBox_param4.Checked
        //                ) != 0
        //            )
        //            {
        //                newFileName = itemName;
        //            }
        //            else
        //            {
        //                //如指定.txt后遇到.txt情况,输出空,然后报告过滤情况,此时newFilePath=""
        //                if (WorkErrCount == 0)
        //                {
        //                    MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                }
        //                WorkErrCount += 1;
        //                MMCore.WriteLine(reportPath, "用户保护:" + item, true);
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            //不保护后缀(或参数4默认填了*)
        //            newFileName = itemName;
        //        }
        //        //用户自定目录的验证和替换
        //        if (comboBox_param8.SelectedIndex == 0)
        //        {
        //            if (MMCore.IsDFPath(textBox_param8.Text) && MMCore.IsDir(textBox_param8.Text))
        //            {
        //                dirName = textBox_param8.Text;
        //            }
        //            else
        //            {
        //                //无效时,按默认输出
        //                dirName = workFilePath.Substring(0, workFilePath.LastIndexOf("\\")) + @"\" + Path.GetFileNameWithoutExtension(workFilePath) + @"_Delete";
        //                textBox_param8.Text = dirName;
        //            }
        //            newFilePath = Path.Combine(dirName + @"\", newFileName);
        //            if (!Directory.Exists(dirName)) { Directory.CreateDirectory(dirName); }
        //        }

        //        if (!MMCore.IsDir(item))
        //        {
        //            switch (comboBox_param8.SelectedIndex)
        //            {
        //                case 0:
        //                    if (!File.Exists(newFilePath) && !Directory.Exists(newFilePath))
        //                    {
        //                        //新路径文件必须不存在,才可以移入
        //                        fileInfo = new FileInfo(item);
        //                        fileInfo.MoveTo(newFilePath);
        //                        MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //                    }
        //                    else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //                    {
        //                        if (WorkErrCount == 0)
        //                        {
        //                            MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                        }
        //                        WorkErrCount += 1;
        //                        MMCore.WriteLine(reportPath, "预判到路径会重叠文件:" + item, true);
        //                    }
        //                    break;
        //                case 1:
        //                    MMCore.DelFileToRecycleBin(item, checkBox_param8.Checked);
        //                    break;
        //                case 2:
        //                    fileInfo = new FileInfo(item);
        //                    fileInfo.Delete();//不能删除只读文件
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            switch (comboBox_param8.SelectedIndex)
        //            {
        //                case 0:
        //                    if (!File.Exists(newFilePath) && !Directory.Exists(newFilePath))
        //                    {
        //                        //新路径文件夹必须不存在,才可以移入
        //                        fileInfo = new FileInfo(item);
        //                        fileInfo.MoveTo(newFilePath);
        //                        MMCore.WriteLine(workFilePath, "【处理完成】" + item + " => " + newFilePath, true);
        //                    }
        //                    else if (File.Exists(newFilePath) || Directory.Exists(newFilePath))
        //                    {
        //                        if (WorkErrCount == 0)
        //                        {
        //                            MMCore.WriteLine(reportPath, "↓未处理文件如下↓", false);
        //                        }
        //                        WorkErrCount += 1;
        //                        MMCore.WriteLine(reportPath, "预判到路径会重叠文件夹:" + item, true);
        //                    }
        //                    break;
        //                case 1:
        //                    MMCore.DelDirectoryToRecycleBin(item, checkBox_param8.Checked);
        //                    break;
        //                case 2:
        //                    directoryInfo = new DirectoryInfo(item);
        //                    MMCore.DelDirectoryRecursively(directoryInfo);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        if (WorkErrCount > 0)
        //        {
        //            label_headTip.Text = $"{WorkErrCount}个文件处理失败！请查看Report文件";
        //        }
        //        //测试↓
        //        //MMCore.WriteLine( reportPath, "item = " + item, true);
        //        //MMCore.WriteLine( reportPath, "dirName = " + dirName, true);
        //        //MMCore.WriteLine( reportPath, "itemName = " + itemName, true);
        //        //MMCore.WriteLine( reportPath, "itemFrontName = " + itemFrontName, true);
        //        //MMCore.WriteLine( reportPath, "itemExtension = " + itemExtension, true);
        //        //MMCore.WriteLine( reportPath, "tempFrontName = " + tempFrontName, true);
        //        //MMCore.WriteLine( reportPath, "tempMidName = " + tempMidName, true);
        //        //MMCore.WriteLine( reportPath, "tempEndName = " + tempEndName, true);
        //    }
        //}

        ///// <summary>
        ///// 检索文件和文件夹并打印或修改
        ///// </summary>
        ///// <param name="workPath"></param>
        ///// <param name="workFilePath"></param>
        ///// <param name="diskSymbol"></param>
        //private void DFRun(string workPath, string workFilePath, string diskSymbol)
        //{
        //    string[] directories;
        //    for (int i = 0; i < 1; i++)
        //    {
        //        label_dirStatistics.Text = "任务执行中...点击此处可查看进度百分比";
        //        ProCount = 0;

        //        FirstWrite(workFilePath, diskSymbol); //首次向工作文本写入信息
        //        if (checkBox_DFDividually.Checked) //勾选文件夹与文件分列
        //        {
        //            if (checkBox__dirFirst.Checked) //勾选优先检索文件夹
        //            {
        //                DirRun(workPath, workFilePath);
        //                MMCore.WriteLine(workFilePath, "", true);
        //                FileRun(workPath, workFilePath);
        //            }
        //            else //否则优先检索文件
        //            {
        //                FileRun(workPath, workFilePath);
        //                MMCore.WriteLine(workFilePath, "", true);
        //                DirRun(workPath, workFilePath);
        //            }
        //        }
        //        else
        //        {
        //            DFPrint(workPath, workFilePath);
        //            if (checkBox_recursion.Checked) //遍历子文件夹
        //            {
        //                directories = Directory.GetDirectories(workPath, "*", SearchOption.AllDirectories);
        //                ProCountMax = directories.Length;
        //                foreach (var item in directories) //处理每个遍历到的文件夹
        //                {
        //                    if (WorkStop) { break; }
        //                    ProCount++;
        //                    DFPrint(item, workFilePath);
        //                    if (WorkStatus)
        //                    {
        //                        label_dirStatistics.Text = ProCount.ToString() + @"/" + ProCountMax.ToString();
        //                    }
        //                }
        //            }
        //        }

        //        MMCore.WriteLine(workFilePath, "████████████████████████████████████████████" + "\spriteRenderer\n" + "", true);//尾行留空
        //    }
        //    if (!WorkStop) { label_dirStatistics.Text = "任务已完成！"; }
        //}

        ///// <summary>
        ///// 决定是否优先检索文件夹的函数
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="workFilePath"></param>
        //private void DFPrint(string path, string workFilePath)
        //{
        //    if (comboBox_selectFunc.SelectedIndex == 0 && checkBox__dirFirst.Checked) //仅检索名称且勾选优先检索文件夹
        //    {
        //        DPrint(path, workFilePath);
        //        FPrint(path, workFilePath);
        //    }
        //    else //否则始终优先检索文件(修改文件的功能需要优先处理文件)
        //    {
        //        FPrint(path, workFilePath);
        //        DPrint(path, workFilePath); //当把文件修改好之后,对文件夹递归修改处理(父目录改完改一层子目录)
        //    }
        //}

        ///// <summary>
        ///// 检索文件夹并按用户功能动作(不含子文件夹遍历)
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="workFilePath"></param>
        //private void DPrint(string path, string workFilePath)
        //{
        //    string[] directories;
        //    string tempStr = "";
        //    string dirName;
        //    string dirSize = "";
        //    string dirTime = "";
        //    string a = "";
        //    string b = "";
        //    int j = 0;
        //    int k = 0;
        //    DirectoryInfo directoryInfo;
        //    if (checkBox_dirSearch.Checked) //允许检索文件夹
        //    {
        //        for (int i = 0; i < 1; i++)
        //        {
        //            if (checkBox_emptyDirIgnore.Checked && MMCore.IsDirectoryEmptyUserDef(path))
        //            {
        //                //MMCore.WriteLine(workFilePath, "忽略空文件夹 = " + path, true);
        //                break;
        //            } //勾选了忽略空文件夹,遇空文件夹时跳出
        //            directories = Directory.GetDirectories(path); //将当前文件夹内所有子文件夹(路径名称)以字符形式存入字符组
        //            if (directories.Length == 0 && comboBox_selectFunc.SelectedIndex == 0) //若不是同一个父目录
        //            {
        //                if (DirCount == 0)
        //                {
        //                    DirCount = 1;
        //                }
        //                else
        //                {
        //                    MMCore.WriteLine(workFilePath, "", true);
        //                }
        //                MMCore.WriteLine(workFilePath, " Directory of " + path, true); //首次输出子文件夹的父目录,表示正在对其检索
        //                if (checkBox_fileStatistics.Checked || checkBox_DirCount.Checked)
        //                {
        //                    if (checkBox_fileStatistics.Checked)
        //                    {
        //                        j = Directory.GetFiles(path).Length;
        //                        a = "               " + j.ToString() + " File(s)";
        //                    }
        //                    if (checkBox_DirCount.Checked)
        //                    {
        //                        k = Directory.GetDirectories(path).Length;
        //                        b = "             " + k.ToString() + " DIR(s)";
        //                    }
        //                    MMCore.WriteLine(workFilePath, a + b, true);
        //                }
        //                if (j + k > 0) { MMCore.WriteLine(workFilePath, "", true); }//目录有内容时换行以隔开内容
        //            }
        //            foreach (var item in directories) //遍历子文件夹字符元素
        //            {
        //                if (WorkStop) { break; }
        //                if (checkBox_emptyDirIgnore.Checked && MMCore.IsDirectoryEmpty(item))
        //                {
        //                    //MMCore.WriteLine(workFilePath, "子目录为空:" + item , true);
        //                    continue; //勾选了忽略空文件夹,但部分子文件夹为空亦跳过
        //                }

        //                if (checkBox_nonEmptyDirIgnore.Checked && !MMCore.IsDirectoryEmptyUserDef(item))
        //                {
        //                    continue; //勾选了忽略非空文件夹(只输出空文件夹),那么非空文件夹将被跳过
        //                }
        //                //通配符错误时重置
        //                if (textBox_param3.Text == "*") { }
        //                else if (
        //                    string.Compare(
        //                        Path.GetExtension(item),
        //                        textBox_param3.Text,
        //                        checkBox_param3.Checked
        //                    ) != 0
        //                )
        //                {
        //                    continue;
        //                }
        //                if (checkBox_specialStr.Checked && textBox_specialStr.Text != "")
        //                {
        //                    if (checkBox_regular.Checked)
        //                    {
        //                        if (!Regex.IsMatch(item.Substring(item.LastIndexOf("\\") + 1), textBox_specialStr.Text)) { continue; }
        //                    }
        //                    else
        //                    {
        //                        //正则或特征不匹配的话,下一个
        //                        if (checkBox_specialStrIgnoreCase.Checked)
        //                        {
        //                            //忽略大小写
        //                            //IndexOf 函数对大小写不敏感,适用于不区分大小写的判断,返回值为int型(在sring中的索引值)
        //                            if (item.Substring(item.LastIndexOf("\\") + 1).IndexOf(textBox_specialStr.Text, StringComparison.OrdinalIgnoreCase) == -1)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            //String.Contains对大小写敏感,适用于区分大小写的判断
        //                            if (!item.Substring(item.LastIndexOf("\\") + 1).Contains(textBox_specialStr.Text))
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                    }
        //                }


        //                if (checkBox_range.Checked && !ParamIsInRange(item))
        //                {
        //                    continue;
        //                }
        //                switch (comboBox_selectFunc.SelectedIndex)
        //                {
        //                    case 0:
        //                        //MMCore.WriteLine(workFilePath, "目录:" + item.Substring(0, item.LastIndexOf("\\"))+" =>上回:"+ tempStr, true);
        //                        if (item.Substring(0, item.LastIndexOf("\\")) != tempStr) //若不是同一个父目录
        //                        {
        //                            tempStr = item.Substring(0, item.LastIndexOf("\\"));
        //                            if (DirCount == 0)
        //                            {
        //                                DirCount = 1;
        //                            }
        //                            else
        //                            {
        //                                MMCore.WriteLine(workFilePath, "", true);
        //                            }
        //                            MMCore.WriteLine(workFilePath, " Directory of " + tempStr, true); //首次输出子文件夹的父目录,表示正在对其检索
        //                            if (checkBox_fileStatistics.Checked || checkBox_DirCount.Checked)
        //                            {
        //                                if (checkBox_fileStatistics.Checked)
        //                                {
        //                                    j = Directory.GetFiles(path).Length;
        //                                    a = "               " + j.ToString() + " File(s)";
        //                                }
        //                                if (checkBox_DirCount.Checked)
        //                                {
        //                                    k = Directory.GetDirectories(path).Length;
        //                                    b = "             " + k.ToString() + " DIR(s)";
        //                                }
        //                                MMCore.WriteLine(workFilePath, a + b, true);
        //                            }
        //                            if (j + k > 0) { MMCore.WriteLine(workFilePath, "", true); }//目录有内容时换行以隔开内容
        //                        }

        //                        if (checkBox_printDirPath.Checked) //勾选文件夹全路径
        //                        {
        //                            dirName = item;
        //                        }
        //                        else
        //                        {
        //                            dirName = item.Substring(item.LastIndexOf("\\") + 1);
        //                        }
        //                        directoryInfo = new DirectoryInfo(item); //根据子文件夹字符元素建立其文件夹信息实例
        //                        if (checkBox_printDirSize.Checked)
        //                        {
        //                            dirSize = MMCore.CountSize(MMCore.GetDirectoryLength(item), checkBox_byteCount.Checked);
        //                        }
        //                        if (checkBox_printDirTime.Checked)
        //                        {
        //                            dirTime = directoryInfo.LastWriteTime.ToString();
        //                        }
        //                        dirName = dirTime + " <DIR> " + dirSize + " " + dirName;
        //                        MMCore.WriteLine(workFilePath, dirName, true);
        //                        break;
        //                    default:
        //                        //修改文件夹动作
        //                        if (checkBox_dirModification.Checked)
        //                        {
        //                            DAction(workFilePath, item);
        //                        }
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 检索文件并按用户功能动作(不含子文件夹遍历)
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="workFilePath"></param>
        //private void FPrint(string path, string workFilePath)
        //{
        //    string[] files;
        //    string fileName;
        //    string newFileName;
        //    string newFilePath;
        //    string fileSize = "";
        //    string fileTime = "";
        //    //string tempStr = "";
        //    //string a = "";
        //    //string b = "";
        //    DirectoryInfo directoryInfo;
        //    FileInfo[] fileInfos;
        //    if (checkBox_fileSearch.Checked) //允许检索文件
        //    {
        //        for (int i = 0; i < 1; i++)
        //        {
        //            if (checkBox_suffix.Checked) //输出文件后缀
        //            {
        //                files = Directory.GetFiles(path);
        //                foreach (var item in files) //处理每个遍历到的文件名字符
        //                {
        //                    if (WorkStop) { break; }
        //                    //MMCore.WriteLine(workFilePath, "文件大小" + MMCore.CountSize(MMCore.GetFileLength(item),checkBox39.Checked), true);//测试专用
        //                    if (checkBox_emptyFileIgnore.Checked && MMCore.CountSize(MMCore.GetFileLength(item), checkBox_byteCount.Checked) == "0 Byte")
        //                    {
        //                        continue; //勾选了忽略空文件,遇空文件时跳过
        //                    }
        //                    if (checkBox_nonEmptyFileIgnore.Checked && !(MMCore.CountSize(MMCore.GetFileLength(item), checkBox_byteCount.Checked) == "0 Byte"))
        //                    {
        //                        continue; //勾选了忽略非空文件,遇非空文件时跳过
        //                    }
        //                    //通配符错误时重置
        //                    if (textBox_param3.Text == "*") { }
        //                    else if (
        //                        string.Compare(
        //                            Path.GetExtension(item),
        //                            textBox_param3.Text,
        //                            checkBox_param3.Checked
        //                        ) != 0
        //                    )
        //                    {
        //                        continue;
        //                    }
        //                    if (checkBox_specialStr.Checked && textBox_specialStr.Text != "")
        //                    {
        //                        if (checkBox_regular.Checked)
        //                        {
        //                            if (!Regex.IsMatch(item.Substring(item.LastIndexOf("\\") + 1), textBox_specialStr.Text)) { continue; }
        //                        }
        //                        else
        //                        {
        //                            //正则或特征不匹配的话,下一个
        //                            if (checkBox_specialStrIgnoreCase.Checked)
        //                            {
        //                                //忽略大小写
        //                                //IndexOf 函数对大小写不敏感,适用于不区分大小写的判断,返回值为int型(在sring中的索引值)
        //                                if (item.Substring(item.LastIndexOf("\\") + 1).IndexOf(textBox_specialStr.Text, StringComparison.OrdinalIgnoreCase) == -1)
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //String.Contains对大小写敏感,适用于区分大小写的判断
        //                                if (!item.Substring(item.LastIndexOf("\\") + 1).Contains(textBox_specialStr.Text))
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    if (checkBox_range.Checked && !ParamIsInRange(item))
        //                    {
        //                        continue;
        //                    }
        //                    switch (comboBox_selectFunc.SelectedIndex)
        //                    {
        //                        case 0:
        //                            if (checkBox_printFilePath.Checked) //勾选文件全路径
        //                            {
        //                                fileName = item;
        //                            }
        //                            else
        //                            {
        //                                fileName = item.Substring(item.LastIndexOf("\\") + 1);
        //                            }
        //                            FileInfo fileInfo = new FileInfo(item);
        //                            if (checkBox_printFileSize.Checked)
        //                            {
        //                                fileSize = MMCore.CountSize(MMCore.GetFileLength(item), checkBox_byteCount.Checked);
        //                            }
        //                            if (checkBox_printFileTime.Checked)
        //                            {
        //                                fileTime = fileInfo.LastWriteTime.ToString();
        //                            }
        //                            fileName = fileTime + " " + fileSize + " " + fileName;
        //                            MMCore.WriteLine(workFilePath, fileName, true);
        //                            break;
        //                        default:
        //                            if (checkBox_fileModification.Checked)
        //                            {
        //                                DAction(workFilePath, item);
        //                            }
        //                            break;
        //                    }
        //                }
        //            }
        //            else //不输出文件后缀
        //            {
        //                directoryInfo = new DirectoryInfo(path);
        //                fileInfos = directoryInfo.GetFiles();
        //                foreach (FileInfo fileInfo in fileInfos)
        //                {
        //                    if (WorkStop) { break; }
        //                    if (
        //                        checkBox_emptyFileIgnore.Checked
        //                        && MMCore.CountSize(MMCore.GetFileLength(fileInfo.Name), checkBox_byteCount.Checked) == "0 Byte"
        //                    )
        //                    {
        //                        continue; //勾选了忽略空文件,遇空文件时跳过
        //                    }
        //                    if (
        //                        checkBox_nonEmptyFileIgnore.Checked
        //                        && !(MMCore.CountSize(MMCore.GetFileLength(fileInfo.Name), checkBox_byteCount.Checked) == "0 Byte")
        //                    )
        //                    {
        //                        continue; //勾选了忽略非空文件,遇非空文件时跳过
        //                    }
        //                    //通配符错误时重置
        //                    if (textBox_param3.Text == "*") { }
        //                    else if (
        //                        string.Compare(
        //                            Path.GetExtension(fileInfo.Name),
        //                            textBox_param3.Text,
        //                            checkBox_param3.Checked
        //                        ) != 0
        //                    )
        //                    {
        //                        continue;
        //                    }
        //                    if (checkBox_specialStr.Checked && textBox_specialStr.Text != "")
        //                    {
        //                        if (checkBox_regular.Checked)
        //                        {
        //                            if (!Regex.IsMatch(fileInfo.Name.Substring(fileInfo.Name.LastIndexOf("\\") + 1), textBox_specialStr.Text)) { continue; }
        //                        }
        //                        else
        //                        {
        //                            //正则或特征不匹配的话,下一个
        //                            if (checkBox_specialStrIgnoreCase.Checked)
        //                            {
        //                                //忽略大小写
        //                                //IndexOf 函数对大小写不敏感,适用于不区分大小写的判断,返回值为int型(在sring中的索引值)
        //                                if (fileInfo.Name.Substring(fileInfo.Name.LastIndexOf("\\") + 1).IndexOf(textBox_specialStr.Text, StringComparison.OrdinalIgnoreCase) == -1)
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //String.Contains对大小写敏感,适用于区分大小写的判断
        //                                if (!fileInfo.Name.Substring(fileInfo.Name.LastIndexOf("\\") + 1).Contains(textBox_specialStr.Text))
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    if (checkBox_range.Checked && !ParamIsInRange(fileInfo.Name))
        //                    {
        //                        continue;
        //                    }
        //                    switch (comboBox_selectFunc.SelectedIndex)
        //                    {
        //                        case 0:
        //                            if (checkBox_printFilePath.Checked) //勾选允许文件全路径
        //                            {
        //                                newFilePath = Path.Combine(
        //                                    fileInfo.DirectoryName,
        //                                    Path.GetFileNameWithoutExtension(fileInfo.Name)
        //                                );
        //                                if (checkBox_printFileSize.Checked)
        //                                {
        //                                    fileSize = MMCore.CountSize(MMCore.GetFileLength(fileInfo.FullName), checkBox_byteCount.Checked);
        //                                }
        //                                if (checkBox_printFileTime.Checked)
        //                                {
        //                                    fileTime = fileInfo.LastWriteTime.ToString();
        //                                }
        //                                newFilePath = fileTime + " " + fileSize + " " + newFilePath;
        //                                MMCore.WriteLine(workFilePath, newFilePath, true);
        //                            }
        //                            else
        //                            {
        //                                newFileName = Path.GetFileNameWithoutExtension(
        //                                    fileInfo.Name
        //                                );
        //                                if (checkBox_printFileSize.Checked)
        //                                {
        //                                    fileSize = MMCore.CountSize(MMCore.GetFileLength(fileInfo.FullName), checkBox_byteCount.Checked);
        //                                }
        //                                if (checkBox_printFileTime.Checked)
        //                                {
        //                                    fileTime = fileInfo.LastWriteTime.ToString();
        //                                }
        //                                newFileName = fileTime + " " + fileSize + " " + newFileName;
        //                                MMCore.WriteLine(workFilePath, newFileName, true);
        //                            }
        //                            break;
        //                        default:
        //                            if (checkBox_fileModification.Checked)
        //                            {
        //                                DAction(workFilePath, fileInfo.FullName);
        //                            }
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 勾选文件夹与文件分列时的文件夹处理动作
        ///// </summary>
        ///// <param name="workPath"></param>
        ///// <param name="workFilePath"></param>
        //private void DirRun(string workPath, string workFilePath)
        //{
        //    string dirName;
        //    string[] directories;
        //    string dirSize = "";
        //    string dirTime = "";
        //    DirectoryInfo directoryInfo;
        //    if (checkBox_dirSearch.Checked) //允许检索文件夹
        //    {
        //        for (int i = 0; i < 1; i++)
        //        {
        //            if (!checkBox_recursion.Checked)
        //            {
        //                //不允许遍历子文件夹,仅遍历当前目录文件夹
        //                directories = Directory.GetDirectories(workPath);
        //            }
        //            else
        //            {
        //                //允许遍历子文件夹下所有文件夹
        //                directories = Directory.GetDirectories(
        //                    workPath,
        //                    "*",
        //                    SearchOption.AllDirectories
        //                );
        //            }
        //            foreach (var item in directories) //处理每个遍历到的文件夹
        //            {
        //                if (WorkStop) { break; }
        //                if (checkBox_emptyDirIgnore.Checked && MMCore.IsDirectoryEmpty(item))
        //                {
        //                    continue; //勾选了忽略空文件夹,但部分子文件夹为空亦跳过
        //                }

        //                if (checkBox_nonEmptyDirIgnore.Checked && !MMCore.IsDirectoryEmptyUserDef(item))
        //                {
        //                    continue; //勾选了忽略非空文件夹(只输出空文件夹),那么非空文件夹将被跳过
        //                }
        //                //通配符错误时重置
        //                if (textBox_param3.Text == "*") { }
        //                else if (
        //                    string.Compare(
        //                        Path.GetExtension(item),
        //                        textBox_param3.Text,
        //                        checkBox_param3.Checked
        //                    ) != 0
        //                )
        //                {
        //                    continue;
        //                }
        //                if (checkBox_specialStr.Checked && textBox_specialStr.Text != "")
        //                {
        //                    if (checkBox_regular.Checked)
        //                    {
        //                        if (!Regex.IsMatch(item.Substring(item.LastIndexOf("\\") + 1), textBox_specialStr.Text)) { continue; }
        //                    }
        //                    else
        //                    {
        //                        //正则或特征不匹配的话,下一个
        //                        if (checkBox_specialStrIgnoreCase.Checked)
        //                        {
        //                            //忽略大小写
        //                            //IndexOf 函数对大小写不敏感,适用于不区分大小写的判断,返回值为int型(在sring中的索引值)
        //                            if (item.Substring(item.LastIndexOf("\\") + 1).IndexOf(textBox_specialStr.Text, StringComparison.OrdinalIgnoreCase) == -1)
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            //String.Contains对大小写敏感,适用于区分大小写的判断
        //                            if (!item.Substring(item.LastIndexOf("\\") + 1).Contains(textBox_specialStr.Text))
        //                            {
        //                                continue;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (checkBox_range.Checked && !ParamIsInRange(item))
        //                {
        //                    continue;
        //                }
        //                switch (comboBox_selectFunc.SelectedIndex)
        //                {
        //                    case 0:
        //                        if (checkBox_printDirPath.Checked)
        //                        {
        //                            //勾选文件夹全路径
        //                            dirName = item;
        //                        }
        //                        else
        //                        {
        //                            dirName = item.Substring(item.LastIndexOf("\\") + 1);
        //                        }
        //                        if (checkBox_printDirSize.Checked)
        //                        {
        //                            dirSize = MMCore.CountSize(MMCore.GetDirectoryLength(item), checkBox_byteCount.Checked);
        //                        }
        //                        if (checkBox_printDirTime.Checked)
        //                        {
        //                            directoryInfo = new DirectoryInfo(item);
        //                            dirTime = directoryInfo.LastWriteTime.ToString();
        //                        }
        //                        dirName = dirTime + " <DIR> " + dirSize + " " + dirName;
        //                        MMCore.WriteLine(workFilePath, dirName, true);
        //                        break;
        //                    default:
        //                        //修改文件夹动作
        //                        if (checkBox_dirModification.Checked)
        //                        {
        //                            DAction(workFilePath, item);
        //                        }
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 勾选文件夹与文件分列时的文件处理动作
        ///// </summary>
        ///// <param name="workPath"></param>
        ///// <param name="workFilePath"></param>
        //private void FileRun(string workPath, string workFilePath)
        //{
        //    string fileName;
        //    string[] files;
        //    string fileSize = "";
        //    string fileTime = "";
        //    string newFileName;
        //    string newFilePath;
        //    DirectoryInfo directoryInfo;
        //    FileInfo[] fileInfos;
        //    if (checkBox_fileSearch.Checked) //允许检索文件
        //    {
        //        for (int i = 0; i < 1; i++)
        //        {
        //            if (!WorkStatus) { break; }
        //            if (!checkBox_recursion.Checked)
        //            {
        //                //不允许遍历子文件夹,仅遍历当前目录文件
        //                files = Directory.GetFiles(workPath);
        //            }
        //            else
        //            {
        //                //允许遍历子文件夹下所有文件
        //                files = Directory.GetFiles(workPath, "*", SearchOption.AllDirectories);
        //            }
        //            if (checkBox_suffix.Checked) //未勾选输出文件后缀
        //            {
        //                foreach (var item in files) //处理每个遍历到的文件
        //                {
        //                    if (WorkStop) { break; }
        //                    if (checkBox_emptyFileIgnore.Checked && MMCore.CountSize(MMCore.GetFileLength(item), checkBox_byteCount.Checked) == "0 Byte")
        //                    {
        //                        continue; //勾选了忽略空文件,遇空文件时跳过
        //                    }
        //                    if (checkBox_nonEmptyFileIgnore.Checked && !(MMCore.CountSize(MMCore.GetFileLength(item), checkBox_byteCount.Checked) == "0 Byte"))
        //                    {
        //                        continue; //勾选了忽略非空文件,遇非空文件时跳过
        //                    }
        //                    //通配符错误时重置
        //                    if (textBox_param3.Text == "*") { }
        //                    else if (
        //                        string.Compare(
        //                            Path.GetExtension(item),
        //                            textBox_param3.Text,
        //                            checkBox_param3.Checked
        //                        ) != 0
        //                    )
        //                    {
        //                        continue;
        //                    }
        //                    if (checkBox_specialStr.Checked && textBox_specialStr.Text != "")
        //                    {
        //                        if (checkBox_regular.Checked)
        //                        {
        //                            if (!Regex.IsMatch(item.Substring(item.LastIndexOf("\\") + 1), textBox_specialStr.Text)) { continue; }
        //                        }
        //                        else
        //                        {
        //                            //正则或特征不匹配的话,下一个
        //                            if (checkBox_specialStrIgnoreCase.Checked)
        //                            {
        //                                //忽略大小写
        //                                //IndexOf 函数对大小写不敏感,适用于不区分大小写的判断,返回值为int型(在sring中的索引值)
        //                                if (item.Substring(item.LastIndexOf("\\") + 1).IndexOf(textBox_specialStr.Text, StringComparison.OrdinalIgnoreCase) == -1)
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //String.Contains对大小写敏感,适用于区分大小写的判断
        //                                if (!item.Substring(item.LastIndexOf("\\") + 1).Contains(textBox_specialStr.Text))
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    if (checkBox_range.Checked && !ParamIsInRange(item))
        //                    {
        //                        continue;
        //                    }
        //                    switch (comboBox_selectFunc.SelectedIndex)
        //                    {
        //                        case 0:
        //                            if (checkBox_printFilePath.Checked)
        //                            {
        //                                //勾选允许文件全路径
        //                                fileName = item;
        //                            }
        //                            else
        //                            {
        //                                fileName = item.Substring(item.LastIndexOf("\\") + 1);
        //                            }

        //                            if (checkBox_printFileSize.Checked)
        //                            {
        //                                fileSize = MMCore.CountSize(MMCore.GetFileLength(item), checkBox_byteCount.Checked);
        //                            }
        //                            if (checkBox_printFileTime.Checked)
        //                            {
        //                                FileInfo fileInfo = new FileInfo(item);
        //                                fileTime = fileInfo.LastWriteTime.ToString();
        //                            }
        //                            fileName = fileTime + " " + fileSize + " " + fileName;
        //                            MMCore.WriteLine(workFilePath, fileName, true);
        //                            break;
        //                        default:
        //                            //修改文件夹动作
        //                            if (checkBox_fileModification.Checked)
        //                            {
        //                                DAction(workFilePath, item);
        //                            }
        //                            break;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                directoryInfo = new DirectoryInfo(workPath);
        //                fileInfos = directoryInfo.GetFiles();
        //                foreach (FileInfo fileInfo in fileInfos)
        //                {
        //                    if (WorkStop) { break; }
        //                    if (
        //                        checkBox_emptyFileIgnore.Checked
        //                        && MMCore.CountSize(MMCore.GetFileLength(fileInfo.Name), checkBox_byteCount.Checked) == "0 Byte"
        //                    )
        //                    {
        //                        continue; //勾选了忽略空文件,遇空文件时跳过
        //                    }
        //                    if (
        //                        checkBox_nonEmptyFileIgnore.Checked
        //                        && !(MMCore.CountSize(MMCore.GetFileLength(fileInfo.Name), checkBox_byteCount.Checked) == "0 Byte")
        //                    )
        //                    {
        //                        continue; //勾选了忽略非空文件,遇非空文件时跳过
        //                    }
        //                    //通配符错误时重置
        //                    if (textBox_param3.Text == "*") { }
        //                    else if (
        //                        string.Compare(
        //                            Path.GetExtension(fileInfo.Name),
        //                            textBox_param3.Text,
        //                            checkBox_param3.Checked
        //                        ) != 0
        //                    )
        //                    {
        //                        continue;
        //                    }
        //                    if (checkBox_specialStr.Checked && textBox_specialStr.Text != "")
        //                    {
        //                        if (checkBox_regular.Checked)
        //                        {
        //                            if (!Regex.IsMatch(fileInfo.Name.Substring(fileInfo.Name.LastIndexOf("\\") + 1), textBox_specialStr.Text)) { continue; }
        //                        }
        //                        else
        //                        {
        //                            //正则或特征不匹配的话,下一个
        //                            if (checkBox_specialStrIgnoreCase.Checked)
        //                            {
        //                                //忽略大小写
        //                                //IndexOf 函数对大小写不敏感,适用于不区分大小写的判断,返回值为int型(在sring中的索引值)
        //                                if (fileInfo.Name.Substring(fileInfo.Name.LastIndexOf("\\") + 1).IndexOf(textBox_specialStr.Text, StringComparison.OrdinalIgnoreCase) == -1)
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //String.Contains对大小写敏感,适用于区分大小写的判断
        //                                if (!fileInfo.Name.Substring(fileInfo.Name.LastIndexOf("\\") + 1).Contains(textBox_specialStr.Text))
        //                                {
        //                                    continue;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    if (checkBox_range.Checked && !ParamIsInRange(fileInfo.Name))
        //                    {
        //                        continue;
        //                    }
        //                    switch (comboBox_selectFunc.SelectedIndex)
        //                    {
        //                        case 0:
        //                            if (checkBox_printFilePath.Checked)
        //                            {
        //                                //勾选允许文件全路径
        //                                newFilePath = Path.Combine(
        //                                    fileInfo.DirectoryName,
        //                                    Path.GetFileNameWithoutExtension(fileInfo.Name)
        //                                );
        //                                if (checkBox_printFileSize.Checked)
        //                                {
        //                                    fileSize = MMCore.CountSize(MMCore.GetFileLength(fileInfo.FullName), checkBox_byteCount.Checked);
        //                                }
        //                                if (checkBox_printFileTime.Checked)
        //                                {
        //                                    fileTime = fileInfo.LastWriteTime.ToString();
        //                                }
        //                                newFilePath = fileTime + " " + fileSize + " " + newFilePath;
        //                                MMCore.WriteLine(workFilePath, newFilePath, true);
        //                            }
        //                            else
        //                            {
        //                                newFileName = Path.GetFileNameWithoutExtension(
        //                                    fileInfo.Name
        //                                );
        //                                if (checkBox_printFileSize.Checked)
        //                                {
        //                                    fileSize = MMCore.CountSize(MMCore.GetFileLength(fileInfo.FullName), checkBox_byteCount.Checked);
        //                                }
        //                                if (checkBox_printFileTime.Checked)
        //                                {
        //                                    fileTime = fileInfo.LastWriteTime.ToString();
        //                                }
        //                                newFileName = fileTime + " " + fileSize + " " + newFileName;
        //                                MMCore.WriteLine(workFilePath, newFileName, true);
        //                            }
        //                            break;
        //                        default:
        //                            if (checkBox_fileModification.Checked)
        //                            {
        //                                DAction(workFilePath, fileInfo.FullName);
        //                            }
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //#endregion

    }
}