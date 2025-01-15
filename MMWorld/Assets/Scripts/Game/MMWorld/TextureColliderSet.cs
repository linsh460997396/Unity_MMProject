using MetalMaxSystem;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using TextAsset = UnityEngine.TextCore.Text.TextAsset;
using TextAsset = UnityEngine.TextAsset;
using CellSpace;
using UnityEngine.UI;
using TMPro;
using Vector3 = UnityEngine.Vector3;

namespace MMWorld
{
    /// <summary>
    /// 纹理碰撞设置器，利用主摄像机注视增删碰撞，也管理着碰撞文件更新。
    /// </summary>
    public class TextureColliderSet : MonoBehaviour
    {
        #region 字段及其属性方法

        /// <summary>
        /// 玩家
        /// </summary>
        public static Player player;
        /// <summary>
        /// 网格数组，用于存储碰撞标记
        /// </summary>
        private static GameObject[,,] grids;
        /// <summary>
        /// 主地形空间团块的游戏物体
        /// </summary>
        private static GameObject chunkGO;
        /// <summary>
        /// 主地形空间团块的游戏物体上的CellChunk组件
        /// </summary>
        private static CellChunk chunk;
        /// <summary>
        /// 碰撞标记用预制体，AO代表人碰撞，A1代表车碰撞
        /// </summary>
        private static GameObject prefabA0, prefabA1;
        /// <summary>
        /// 碰撞文件资源对象
        /// </summary>
        private static TextAsset textAsset;
        /// <summary>
        /// 游戏入口对象
        /// </summary>
        private static GameObject gameMain;
        /// <summary>
        /// 游戏主摄像机
        /// </summary>
        private static GameObject mainCamera;
        /// <summary>
        /// 主画布游戏物体
        /// </summary>
        private static GameObject mainCanvaGO;
        /// <summary>
        /// 主画布组件
        /// </summary>
        private static Canvas mainCanva;
        /// <summary>
        /// 决定特征纹理是否选择的开关
        /// </summary>
        private static Toggle TextureModeToggle;
        /// <summary>
        /// 决定标记是否显示的开关
        /// </summary>
        private static Toggle ShowColliderToggle;
        /// <summary>
        /// 执行碰撞保存的按钮
        /// </summary>
        private static GameObject button_run;
        /// <summary>
        /// 主UI顶部提示标签
        /// </summary>
        private static GameObject label_headTip;
        /// <summary>
        /// 主UI的功能选择下拉框
        /// </summary>
        private static GameObject comboBox_selectFunc;
        /// <summary>
        /// 主UI的输入框，暂用于读取碰撞文件并显示
        /// </summary>
        private static GameObject textBox_input;
        /// <summary>
        /// 主UI界面的工作路径输入框，目前用于填写地图或纹理集编号数字来工作
        /// </summary>
        private static GameObject textBox_workPath;
        /// <summary>
        /// 主UI界面的工作文件输入框，目前用于显示碰撞文件路径
        /// </summary>
        private static GameObject textBox_ruleFilePath;
        /// <summary>
        /// 主UI界面的选择工作路径按钮，目前用于切换地图或纹理集
        /// </summary>
        private static GameObject button_selectWorkPath;
        /// <summary>
        /// 主UI界面的选择工作文件按钮，目前用于点击读取碰撞文件到输入框展示
        /// </summary>
        private static GameObject button_selectWorkFile;
        /// <summary>
        /// 主UI界面的特征纹理勾选框，用于决定工作编号是地图还是特征纹理（默认勾选）
        /// </summary>
        private static GameObject checkBox_TextureMode;
        /// <summary>
        /// 主UI界面的特征纹理勾选框，用于决定工作编号是地图还是特征纹理（默认勾选）
        /// </summary>
        private static GameObject checkBox_ShowCollider;
        /// <summary>
        /// 用户当前设置的碰撞操作类型
        /// </summary>
        private static string colliderID;
        /// <summary>
        /// 鼠标点击单元时按照坐标计算获取的纹理ID
        /// </summary>
        private static string textureID;
        /// <summary>
        /// 纹理ID对应的碰撞类型
        /// </summary>
        private static string colliderValue;
        /// <summary>
        /// 地形网格宽度
        /// </summary>
        private static int gridWidth = 8;
        /// <summary>
        /// 地形网格高度
        /// </summary>
        private static int gridHeight = 170;
        /// <summary>
        /// 存储纹理ID及其碰撞信息的字典
        /// </summary>
        public static Dictionary<string, string> mapColliderDictionary = new Dictionary<string, string>();

        private static int _mapID = 1;
        /// <summary>
        /// 碰撞设置器的工作用图编号，需结合特征纹理勾选框来决定是地图编号还是特征纹理集编号
        /// </summary>
        public static int MapID { get => _mapID; set => _mapID = value; }

        #endregion

        private void Awake()
        {
            //gameMain = GameObject.Find("GameMain");
            gameMain = this.gameObject;

            //预创建网格数组用于存储碰撞标记
            grids = new GameObject[256, 256, 2];

            //创建两个碰撞标记用预制体
            prefabA0 = CreatePrefab("prefabA0", 4, 0.5f, "Custom/CShader");
            prefabA1 = CreatePrefab("prefabA1", 4, 0.5f, "Custom/CShader_1");
            HideObject(prefabA0); HideObject(prefabA1);

            //碰撞设置时，摄像机的投影模式应该是正交投影（以获取正确的鼠标点击位置）
            if (Camera.main != null)
            {
                //将摄像机的投影模式设置为正交投影
                Camera.main.orthographic = true;
                //设置正交投影的大小
                //Camera.main.orthographicSize = 5f;
            }
            else
            {
                Debug.LogError("没有找到主摄像机！");
            }
        }

        private void Start()
        {
            LoadColliderFile();

            if (gameMain != null)
            {
                mainCamera = GameObject.Find("MainCamera");
                mainCanvaGO = mainCamera.transform.Find("MainCanva").gameObject;
                mainCanvaGO.SetActive(true);
                mainCanva = GameObject.Find("MainCanva").GetComponent<Canvas>();

                //Variable Init
                label_headTip = GameObject.Find("label_headTip");
                comboBox_selectFunc = GameObject.Find("comboBox_selectFunc");
                textBox_input = GameObject.Find("textBox_input");
                textBox_workPath = GameObject.Find("textBox_workPath");
                textBox_ruleFilePath = GameObject.Find("textBox_ruleFilePath");
                button_selectWorkPath = GameObject.Find("button_selectWorkPath");
                button_selectWorkFile = GameObject.Find("button_selectWorkFile");
                button_run = GameObject.Find("button_run");
                checkBox_TextureMode = GameObject.Find("checkBox_TextureMode");
                TextureModeToggle = checkBox_TextureMode.GetComponent<Toggle>();
                checkBox_ShowCollider = GameObject.Find("checkBox_ShowCollider");
                ShowColliderToggle = checkBox_ShowCollider.GetComponent<Toggle>();

                //Value Dft
                label_headTip.GetComponent<TextMeshProUGUI>().color = Color.red;
                if (comboBox_selectFunc.GetComponent<TMP_Dropdown>().value < 0)
                {
                    //当值为-1（未选）时，重置为第一个元素选项
                    comboBox_selectFunc.GetComponent<TMP_Dropdown>().value = 0;
                }

                //Event Register
                button_selectWorkPath.GetComponent<Button>().onClick.AddListener(button1_Click);
                button_selectWorkFile.GetComponent<Button>().onClick.AddListener(button2_Click);
                textBox_workPath.GetComponent<TMP_InputField>().onValueChanged.AddListener(OnTextChanged);

                //other
                textBox_ruleFilePath.GetComponent<TMP_InputField>().text = Application.dataPath + "/Resources/ColliderFiles/MapCollider.txt";//显示默认路径
                textBox_input.GetComponent<TMP_InputField>().lineType = TMP_InputField.LineType.MultiLineNewline;//多行
            }
        }

        private void Update()
        {
            if (mainCanvaGO.activeSelf == false && TextureModeToggle.isOn && MapID >= 0 && MapID < 2)
            {
                Update_TextureCollider();
            }
            else if (mainCanvaGO.activeSelf == false && !TextureModeToggle.isOn && MapID >= 0 && MapID < 240)
            {
                Update_MapCollider();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {//按回车键保存碰撞文件
                SaveColliderFile();
            }
            else if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote) && mainCanvaGO != null)
            {//按波浪或反引号键来显隐界面
                colliderID = "0";//显隐时总是重置用户选择的碰撞类型，以免误操作
                mainCanvaGO.SetActive(!mainCanvaGO.activeSelf);
            }
        }
        /// <summary>
        /// 适用于大小地图纹理集碰撞标记操作
        /// </summary>
        private void Update_TextureCollider()
        {
            int startID = -1;
            if (TextureModeToggle.isOn)
            {
                switch (MapID)
                {
                    case 0:
                        startID = 11;
                        break;
                    case 1:
                        startID = 163;
                        break;
                    default:
                        startID = -1;
                        break;
                }
                if (startID != -1)
                {
                    SelectColliderID();//获取用户设置的碰撞操作类型
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);//获取鼠标点击位置（正交相机获取XY，Z始终等于相机的近裁剪面位置）
                    if (mousePosition.x >= 0 && mousePosition.x < gridWidth && mousePosition.y >= 0 && mousePosition.y < gridHeight)
                    {//如鼠标碰到有效单元（没出当前网格范围）
                        if (Input.GetMouseButtonDown(0))
                        {//左键则放置标记在单元位置
                            textureID = ((int)mousePosition.x + 8 * (int)mousePosition.y + startID).ToString();
                            if (colliderID == "1" && !mapColliderDictionary.TryGetValue(textureID, out colliderValue))
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && !mapColliderDictionary.TryGetValue(textureID, out colliderValue))
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue != colliderID && colliderValue != "0")
                            {
                                mapColliderDictionary[textureID] = "3";
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue != colliderID && colliderValue != "0")
                            {
                                mapColliderDictionary[textureID] = "3";
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "0")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "0")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && !mapColliderDictionary.TryGetValue(textureID, out colliderValue))
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "1")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "2")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "0")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                                CreateCollider(mousePosition, prefabA1);
                            }
                            Debug.Log("添加 textureID = " + textureID + " colliderID = " + colliderID);
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {//右键则删除标记
                            textureID = ((int)mousePosition.x + 8 * (int)mousePosition.y + startID).ToString();
                            if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == colliderID)
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == colliderID)
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "3")
                            {
                                mapColliderDictionary[textureID] = "2";
                                DestroyCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "3")
                            {
                                mapColliderDictionary[textureID] = "1";
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "3")
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA0);
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "1")
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "2")
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            Debug.Log("移除 textureID = " + textureID + " colliderID = " + colliderID);
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {//按回车键保存碰撞文件
                        SaveColliderFile();
                    }
                    else if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote) && mainCanvaGO != null)
                    {//按波浪或反引号键来显隐界面
                        colliderID = "0";//显隐时总是重置用户选择的碰撞类型，以免误操作
                        mainCanvaGO.SetActive(!mainCanvaGO.activeSelf);
                    }
                }
            }
        }
        /// <summary>
        /// 用于实际场景地图的碰撞标记操作
        /// </summary>
        private void Update_MapCollider()
        {
            int mapIndex; int startID = -1;
            if (!TextureModeToggle.isOn)
            {
                switch (MapID)
                {
                    case 0:
                        startID = 10;
                        break;
                    default:
                        startID = 162;
                        break;
                }

                if (startID != -1 && MapID >= 0 && MapID < 240)
                {
                    SelectColliderID();//获取用户设置的碰撞操作类型
                    Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);//获取鼠标点击位置（正交相机获取XY，Z始终等于相机的近裁剪面位置）
                    if (mousePosition.x >= 0 && mousePosition.x < gridWidth && mousePosition.y >= 0 && mousePosition.y < gridHeight)
                    {//如鼠标碰到有效单元（没出当前网格范围）
                        if (Input.GetMouseButtonDown(0))
                        {//左键则放置标记在单元位置
                            mapIndex = (int)mousePosition.x + gridWidth * (int)mousePosition.y;//如果是拉多镇21宽度，鼠标点击第一格是0+21*0的索引[0]
                            textureID = (CPEngine.mapIDs[MapID][mapIndex] + startID).ToString();
                            if (colliderID == "1" && !mapColliderDictionary.TryGetValue(textureID, out colliderValue))
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && !mapColliderDictionary.TryGetValue(textureID, out colliderValue))
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue != colliderID && colliderValue != "0")
                            {
                                mapColliderDictionary[textureID] = "3";
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue != colliderID && colliderValue != "0")
                            {
                                mapColliderDictionary[textureID] = "3";
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "0")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "0")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && !mapColliderDictionary.TryGetValue(textureID, out colliderValue))
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "1")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "2")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "0")
                            {
                                mapColliderDictionary[textureID] = colliderID;
                                CreateCollider(mousePosition, prefabA0);
                                CreateCollider(mousePosition, prefabA1);
                            }
                            Debug.Log("添加 textureID = " + textureID + " colliderID = " + colliderID);
                        }
                        else if (Input.GetMouseButtonDown(1))
                        {//右键则删除标记
                            mapIndex = (int)mousePosition.x + gridWidth * (int)mousePosition.y;//如果是拉多镇21宽度，鼠标点击第一格是0+21*0的索引[0]
                            textureID = (CPEngine.mapIDs[MapID][mapIndex] + startID).ToString();
                            if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == colliderID)
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == colliderID)
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "1" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "3")
                            {
                                mapColliderDictionary[textureID] = "2";
                                DestroyCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "2" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "3")
                            {
                                mapColliderDictionary[textureID] = "1";
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "3")
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA0);
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "1")
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA0);
                            }
                            else if (colliderID == "3" && mapColliderDictionary.TryGetValue(textureID, out colliderValue) && colliderValue == "2")
                            {
                                mapColliderDictionary[textureID] = "0";
                                DestroyCollider(mousePosition, prefabA1);
                            }
                            Debug.Log("移除 textureID = " + textureID + " colliderID = " + colliderID);
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {//按回车键保存碰撞文件
                        SaveColliderFile();
                    }
                    else if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote) && mainCanvaGO != null)
                    {//按波浪或反引号键来显隐界面
                        colliderID = "0";//显隐时总是重置用户选择的碰撞类型，以免误操作
                        mainCanvaGO.SetActive(!mainCanvaGO.activeSelf);
                    }
                }
            }
        }

        /// <summary>
        /// 读取碰撞文件，若存在则将纹理ID及其对应的碰撞信息保存到mapColliderDictionary。
        /// 开局初始化时，默认是小地图纹理集，将初始化创建碰撞标记。
        /// </summary>
        private void LoadColliderFile()
        {
            textAsset = Resources.Load<TextAsset>("ColliderFiles/MapCollider");
            if (textAsset != null)
            {//若存在文件则进行赋值
                int a, b, c;
                string tempContent = textAsset.text;
                string[] fields = tempContent.Split(',');
                for (int i = 0; i < fields.Length; i += 2)
                {
                    mapColliderDictionary[fields[i]] = fields[i + 1];//等于后面一位的值
                    c = int.Parse(fields[i]) - 162;
                    if (c >= 163 && c <= 1522)
                    {
                        //开局初始化时，默认是小地图纹理集，这里创建碰撞标记
                        a = (int.Parse(fields[i]) - 162) % 8;
                        b = (int.Parse(fields[i]) - 162) / 8;
                        if (fields[i + 1] == "1")
                        {
                            CreateCollider(new Vector3(a, b, 0), prefabA0);
                        }
                        else if (fields[i + 1] == "2")
                        {
                            CreateCollider(new Vector3(a, b, 0), prefabA1);
                        }
                        else if (fields[i + 1] == "3")
                        {
                            CreateCollider(new Vector3(a, b, 0), prefabA0);
                            CreateCollider(new Vector3(a, b, 0), prefabA1);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据mapColliderDictionary并结合当前地图编号进行碰撞标记的初始化显示
        /// </summary>
        private void SetColliderByTextureID(string textureID, Vector3 vector)
        {
            string colliderValue;
            if (mapColliderDictionary.TryGetValue(textureID, out colliderValue))
            {
                switch (colliderValue)
                {
                    case "1":
                        CreateCollider(vector, prefabA0);
                        break;
                    case "2":
                        CreateCollider(vector, prefabA1);
                        break;
                    case "3":
                        CreateCollider(vector, prefabA0);
                        CreateCollider(vector, prefabA1);
                        break;
                    default:
                        break;
                }
            }
        }

        private void SaveColliderFile()
        {
            MMCore.Write(string.Join(",", mapColliderDictionary.Select(kvp => $"{kvp.Key},{kvp.Value}")));
            MMCore.fileWriter.Close(Application.dataPath + "/Resources/ColliderFiles/MapCollider.txt", false, Encoding.UTF8);
            Debug.Log("执行保存..若未见文件更新，请点击VS2022界面再切回Unity窗口！");
        }

        /// <summary>
        /// 用户设置的碰撞操作类型：0没有碰撞，1=人碰撞，2=车碰撞，3=人和车碰撞，每种都是左键添加右键移除
        /// </summary>
        private void SelectColliderID()
        {
            string key;
            for (ushort i = 0; i < 4; i++)
            {
                key = i.ToString();
                if (Input.GetKeyDown(key))
                {
                    colliderID = key;
                }
            }
        }

        /// <summary>
        /// 创建碰撞标记
        /// </summary>
        /// <param name="position"></param>
        /// <param name="prefab"></param>
        private void CreateCollider(Vector3 position, GameObject prefab)
        {
            int index = 0;
            if (prefab == prefabA0) { index = 1; }
            else if (prefab == prefabA1) { index = 2; }

            float x = (int)position.x + 0.5f;
            float y = (int)position.y + 0.5f;

            if (index == 1)
            {
                Vector3 newPosition = new Vector3(x - 0.2f, y, -0.8f);
                if (grids[(int)position.x, (int)position.y, index - 1] == null)
                {//实际物理位置不存在该对象时进行创建
                    GameObject collider = Instantiate(prefab, newPosition, Quaternion.identity);
                    ShowObject(collider);
                    //为新生成的A0设置唯一名称
                    collider.name = "人_" + ((int)position.x + 8 * (int)position.y).ToString();
                    //尺寸修改
                    collider.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    grids[(int)position.x, (int)position.y, index - 1] = collider;
                }
                else
                {//实际物理位置存在该对象时进行显示
                    ShowObject(grids[(int)position.x, (int)position.y, index - 1]);
                }
            }
            else if (index == 2)
            {
                Vector3 newPosition = new Vector3(x + 0.2f, y, -0.8f);
                if (grids[(int)position.x, (int)position.y, index - 1] == null)
                {
                    GameObject collider = Instantiate(prefab, newPosition, Quaternion.identity);
                    ShowObject(collider);
                    //为新生成的A0设置唯一名称
                    collider.name = "车_" + ((int)position.x + 8 * (int)position.y).ToString();
                    //尺寸修改
                    collider.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    grids[(int)position.x, (int)position.y, index - 1] = collider;
                }
                else
                {
                    ShowObject(grids[(int)position.x, (int)position.y, index - 1]);
                }
            }
        }

        /// <summary>
        /// 摧毁碰撞标记
        /// </summary>
        /// <param name="position"></param>
        /// <param name="prefab"></param>
        private void DestroyCollider(Vector3 position, GameObject prefab)
        {
            int index = 0;
            if (prefab == prefabA0) { index = 1; }
            else if (prefab == prefabA1) { index = 2; }

            if (grids[(int)position.x, (int)position.y, index - 1] != null)
            {//实际物理位置存在该对象时进行摧毁（这里修改为隐藏）
                //Destroy(grids[(int)position.x, (int)position.y, index - 1]);
                HideObject(grids[(int)position.x, (int)position.y, index - 1]);
            }
        }

        /// <summary>
        /// 创建一个平面圆形游戏物体（默认仅背面渲染，摄像机从Z低处向上看得到），会立马出现在场景，可手动隐藏或保存为预制体（素材文件）后清除它。
        /// </summary>
        /// <param name="name">预制体名称</param>
        /// <param name="segments">分段数决定圆的平滑程度，越少圆看起来就越像是多边形，越多圆就越接近于真正的圆形，少于4个会看不到圆而是正方形，2~3个则是三角，1个是条线</param>
        /// <param name="radius">半径</param>
        /// <param name="shaderName">Shader名称</param>
        /// <returns></returns>
        private GameObject CreatePrefab(string name, int segments, float radius, string shaderName)
        {
            GameObject prefab = new GameObject(name);
            MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateCircleMeshBoth(segments, radius, 1.0f);
            MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
            //纹理数据会被加载并传递给Shader，但由于Shader被固定为输出红色，后面即使meshRenderer.material.mainTexture=各种纹理都不会影响
            meshRenderer.material = new Material(Shader.Find(shaderName));
            return prefab;
        }

        /// <summary>
        /// 翘角双面网格
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Mesh CreateCircleMeshBoth(int segments, float radius, float height)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3 * 2];

            vertices[0] = Vector3.zero; // 中心点保持不变

            for (int i = 1; i <= segments; i++)
            {
                float angle = 2 * Mathf.PI * (i - 1) / segments;
                // 非中心点的顶点在Z轴上添加一个凸起
                vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, height);
            }

            // 生成三角形索引，与原始平面网格相同
            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = (i + 1) % segments + 1;
                triangles[i * 3 + 2] = i + 1;
            }

            // 生成反面三角形索引 
            for (int i = 0; i < segments; i++)
            {
                triangles[segments * 3 + i * 3] = 0;
                triangles[segments * 3 + i * 3 + 1] = (i + 1) % segments + 1;
                triangles[segments * 3 + i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); // 重新计算法线以确保光照正确

            return mesh;
        }

        /// <summary>
        /// 隐藏物体
        /// </summary>
        /// <param name="obj"></param>
        private void HideObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        /// <summary>
        /// 显示物体
        /// </summary>
        /// <param name="obj"></param>
        private void ShowObject(GameObject obj)
        {
            obj.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        private void button1_Click()
        {
            ////另一种查找方法
            //CellChunk[] allCellChunks = FindObjectsOfType<CellChunk>();
            //Debug.Log(allCellChunks.Length);
            //foreach (CellChunk cellChunk in allCellChunks)
            //{
            //    // 检查Fresh属性是否为false
            //    if (cellChunk.Fresh == false)
            //    {
            //        //找到目标对象
            //        Debug.Log("Found CellChunk with Fresh = false on GameObject: " + cellChunk.gameObject.name);
            //        chunkGO = cellChunk.gameObject;
            //        chunk = chunkGO.GetComponent<CellChunk>();
            //        break;
            //    }
            //}

            if (CPEngine.userCellChunks.Count > 0)
            {
                if (chunk == null)
                {
                    Debug.Log("CPEngine.userCellChunks.Count = " + CPEngine.userCellChunks.Count);
                    foreach (CellChunk cellChunk in CPEngine.userCellChunks)
                    {
                        if (!cellChunk.Fresh)
                        {
                            Debug.Log("Found a CellChunk with Fresh = false");
                            chunkGO = cellChunk.gameObject;
                            chunkGO.name = "CellChunk(Main)";
                            chunk = chunkGO.GetComponent<CellChunk>();
                            break;
                        }
                    }
                }
                if (chunk != null)
                {
                    //根据输入内容来切换地图
                    string temp = textBox_workPath.GetComponent<TMP_InputField>().text;
                    if (temp != null || temp != "" && MMCore.IsNumeric(temp))
                    {
                        int id = int.Parse(temp);
                        Debug.Log("id = " + id);
                        if (id >= 0 && id < 240 && !TextureModeToggle.isOn)
                        {//数字是有效的，且是地图编号
                            LoadMap(id);
                        }
                        else if (id >= 0 && id < 2 && TextureModeToggle.isOn)
                        {//数字是有效的，且是重装机兵特征纹理编号
                            LoadTexture(id);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 点击加载碰撞文本
        /// </summary>
        private void button2_Click()
        {
            textBox_input.GetComponent<TMP_InputField>().text = Resources.Load<TextAsset>("ColliderFiles/MapCollider").text;
        }

        /// <summary>
        /// 输入框中纹理、地图编号改变事件下的函数引用
        /// </summary>
        /// <param name="newText"></param>
        private void OnTextChanged(string newText)
        {
            if (MMCore.IsNumeric(newText))
            {
                MapID = int.Parse(newText);
            }
            colliderID = "0";//重置用户定义的碰撞类型
        }

        /// <summary>
        /// mapID=0为大地图，小地图从1~239开始（1是拉多镇），240是龙珠大地图测试
        /// </summary>
        /// <param name="mapID"></param>
        private void LoadMap(int mapID)
        {
            int i = -1;
            if (mapID == 0)
            {//刷大地图
                gridWidth = 256; gridHeight = 256;
                MapColliderClear();
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCell(x, y, (ushort)(CPEngine.mapIDs[0][i] + 10), true);//重装机兵大地图第一个纹理编号从11开始
                            SetColliderByTextureID((CPEngine.mapIDs[0][i] + 10).ToString(), new Vector3(x, y, 0));
                        }
                    }
                }
                //进行角色位置重置
                player.InitPosition(350, 350);
            }
            else if (mapID > 0 && mapID < 240)
            {//刷小地图
                int width = CPEngine.mapWidths[mapID - 1];//拉多是mapId=1，格子宽度=CPEngine.mapWidths[0]
                int currentX = 0; // 当前列的索引
                int currentY = 0; // 当前行的索引

                gridWidth = width; gridHeight = (CPEngine.mapIDs[mapID].Count + width - 1) / width;
                MapClear(width, (CPEngine.mapIDs[mapID].Count + width - 1) / width); MapColliderClear();

                // 由于我们不知道总格子数，我们将使用一个条件来检查是否应该停止
                bool shouldStop = false;

                while (!shouldStop)
                {
                    i++; // 增加计数
                    if (CPEngine.HorizontalMode)
                    {
                        chunk.SetCell(currentX, currentY, (ushort)(CPEngine.mapIDs[mapID][i] + 162), true);//重装机兵小地图第一个纹理编号从163开始
                        SetColliderByTextureID((CPEngine.mapIDs[mapID][i] + 162).ToString(), new Vector3(currentX, currentY, 0));
                    }
                    currentX++;

                    // 如果达到行宽，则换行
                    if (currentX >= width)
                    {
                        currentX = 0; // 重置列索引
                        currentY++;   // 增加行索引

                        // 检查是否应该停止
                        if (i + 1 >= CPEngine.mapIDs[mapID].Count) //如拉多的格子数是384，达到就停止
                        {
                            shouldStop = true;
                        }
                    }
                }
                //进行角色位置重置
                player.InitPosition(350, 350);
            }
            //else if (mapID == 240)
            //{//刷龙珠大地图
            //    for (int y = 0; y < 349; y++)
            //    {
            //        for (int x = 0; x < 512; x++)
            //        {
            //            i++;
            //            if (CPEngine.HorizontalMode)
            //            {
            //                chunk.SetCell(x, y, (ushort)(CPEngine.mapIDs[240][i] + 1522), true);//龙珠大地图第一个纹理编号从1523开始
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 0是大地图纹理，1是小地图纹理
        /// </summary>
        /// <param name="textureID"></param>
        private void LoadTexture(int textureID)
        {
            int i = -1;
            if (textureID == 0)
            {//刷大地图纹理
                Debug.Log("刷大地图纹理");
                gridWidth = 8; gridHeight = 19;
                MapClear(8, 19); MapColliderClear();
                for (int y = 0; y < 19; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCell(x, y, (ushort)(x + 8 * y + 11), true);//重装机兵大地图第一个纹理编号从11开始
                            SetColliderByTextureID((x + 8 * y + 11).ToString(), new Vector3(x, y, 0));
                        }
                    }
                }
                //进行角色位置重置
                player.InitPosition(350, 350);
            }
            else if (textureID == 1)
            {//刷小地图纹理
                Debug.Log("刷小地图纹理");
                gridWidth = 8; gridHeight = 170;
                MapClear(8, 170); MapColliderClear();
                for (int y = 0; y < 170; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        i++;
                        if (CPEngine.HorizontalMode)
                        {
                            chunk.SetCell(x, y, (ushort)(x + 8 * y + 163), true);//重装机兵大地图第一个纹理编号从163开始
                            SetColliderByTextureID((x + 8 * y + 163).ToString(), new Vector3(x, y, 0));
                        }
                    }
                }
                //进行角色位置重置
                player.InitPosition(350, 350);
            }
        }

        /// <summary>
        /// 清理地图
        /// </summary>
        /// <param name="xStart"></param>
        /// <param name="yStart"></param>
        private void MapClear(int xStart, int yStart)
        {
            for (int y = 0; y < CPEngine.ChunkSideLength; y++)
            {
                for (int x = 0; x < CPEngine.ChunkSideLength; x++)
                {
                    // 检查当前坐标是否在排除区域内
                    if (x < xStart && y < yStart)
                    {
                        //如果在排除区域内，跳过这个坐标
                        continue;
                    }

                    if (CPEngine.HorizontalMode)
                    {
                        if (chunk.GetCellID(x, y) != 0)
                        {
                            chunk.SetCell(x, y, 0, true);//0是空块
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清理地图碰撞标记。根据当前gridWidth和gridHeight进行遍历查找存在的对象并隐藏。
        /// </summary>
        private void MapColliderClear()
        {
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        if (grids[x, y, z] != null)
                        {
                            grids[x, y, z].SetActive(false);
                        }
                    }
                }
            }
        }
    }
}