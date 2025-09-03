using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    [HelpURL("https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/ui-component")]
    public class EmeraldUI : MonoBehaviour
    {
        #region Variables
        public string AIName = "AI Name";
        public string AITitle = "AI Title";
        public int AILevel = 1;
        public bool HideSettingsFoldout;
        public bool UISettingsFoldout;
        public bool HealthBarsFoldout;
        public bool CombatTextFoldout;
        public bool NameTextFoldout;
        public bool LevelTextFoldout;
        public YesOrNo UseCustomHealthBar = YesOrNo.No;
        public YesOrNo DisplayAIName = YesOrNo.No;
        public YesOrNo DisplayAITitle = YesOrNo.No;
        public YesOrNo DisplayAILevel = YesOrNo.No;
        public YesOrNo UseAINameUIOutlineEffect = YesOrNo.Yes;
        public YesOrNo UseAILevelUIOutlineEffect = YesOrNo.Yes;
        public YesOrNo UseCustomFontAIName = YesOrNo.No;
        public YesOrNo UseCustomFontAILevel = YesOrNo.No;
        public YesOrNo AutoCreateHealthBars = YesOrNo.No;
        public Canvas HealthBarCanvasRef;
        public GameObject HealthBar;
        public GameObject HealthBarCanvas;
        public string CameraTag = "MainCamera";
        public EmeraldHealthBar m_HealthBarComponent;
        public string UITag = "Player";
        public LayerMask UILayerMask = 0;
        public int MaxUIScaleSize = 16;
        public Text AINameUI;
        public Font AINameFont;
        public float AINameLineSpacing = 0.75f;
        public Vector2 AINameUIOutlineSize = new Vector2(0.35f, -0.35f);
        public Color AINameUIOutlineColor = Color.black;
        public Text AILevelUI;
        public Font AILevelFont;
        public Vector2 AILevelUIOutlineSize = new Vector2(0.35f, -0.35f);
        public Color AILevelUIOutlineColor = Color.black;
        public Sprite HealthBarImage;
        public Sprite HealthBarBackgroundImage;
        public Vector3 HealthBarPos = new Vector3(0, 1.75f, 0);
        public Color HealthBarColor = new Color32(197, 41, 41, 255);
        public Color HealthBarDamageColor = new Color32(248, 217, 4, 255);
        public Color HealthBarBackgroundColor = new Color32(36, 36, 36, 255);
        public Color NameTextColor = new Color32(255, 255, 255, 255);
        public Color LevelTextColor = new Color32(255, 255, 255, 255);
        public Vector3 HealthBarScale = new Vector3(0.75f, 1, 1);
        public int NameTextFontSize = 7;
        public GameObject WaypointParent;
        public string WaypointOrigin;
        public Vector3 AINamePos = new Vector3(0, 3, 0);
        public Vector3 AILevelPos = new Vector3(1.5f, 0, 0);
        EmeraldSystem EmeraldComponent;
        #endregion

        void Start()
        {
            InitializeUI(); //Initialize the EmeraldUI script.
        }

        /// <summary>
        /// Initialize the UI settings.
        /// </summary>
        void InitializeUI ()
        {
            EmeraldComponent = GetComponent<EmeraldSystem>();
            EmeraldComponent.DetectionComponent.OnDetectionUpdate += UpdateAIUI; //Subscribe to the OnDetectionUpdate event for updating the UI's state.

            if (AutoCreateHealthBars == YesOrNo.Yes && HealthBarCanvas == null || DisplayAIName == YesOrNo.Yes && HealthBarCanvas == null)
            {
                HealthBarCanvas = Resources.Load("AI Health Bar Canvas") as GameObject;
            }

            if (AutoCreateHealthBars == YesOrNo.Yes && HealthBarCanvas != null || DisplayAIName == YesOrNo.Yes && HealthBarCanvas != null)
            {
                HealthBar = Instantiate(HealthBarCanvas, Vector3.zero, Quaternion.identity) as GameObject;
                GameObject HealthBarParent = new GameObject();
                HealthBarParent.name = "HealthBarParent";
                HealthBarParent.transform.SetParent(this.transform);
                HealthBarParent.transform.localPosition = new Vector3(0, 0, 0);

                HealthBar.transform.SetParent(HealthBarParent.transform);
                HealthBar.transform.localPosition = HealthBarPos;
                HealthBar.AddComponent<EmeraldHealthBar>();
                EmeraldHealthBar HealthBarScript = HealthBar.GetComponent<EmeraldHealthBar>();
                m_HealthBarComponent = HealthBarScript;
                HealthBar.name = "AI Health Bar Canvas";

                GameObject HealthBarChild = HealthBar.transform.Find("AI Health Bar Background").gameObject;
                HealthBarChild.transform.localScale = HealthBarScale;

                Image HealthBarRef = HealthBarChild.transform.Find("AI Health Bar").GetComponent<Image>();
                HealthBarRef.color = HealthBarColor;

                Image HealthBarDamageRef = HealthBarChild.transform.Find("AI Health Bar (Damage)").GetComponent<Image>();
                HealthBarDamageRef.color = HealthBarDamageColor;

                Image HealthBarBackgroundImageRef = HealthBarChild.GetComponent<Image>();
                HealthBarBackgroundImageRef.color = HealthBarBackgroundColor;

                HealthBarCanvasRef = HealthBar.GetComponent<Canvas>();

                if (AutoCreateHealthBars == YesOrNo.No)
                {
                    HealthBarChild.GetComponent<Image>().enabled = false;
                    HealthBarRef.gameObject.SetActive(false);
                }

                if (UseCustomHealthBar == YesOrNo.Yes && HealthBarBackgroundImage != null && HealthBarImage != null)
                {
                    HealthBarBackgroundImageRef.sprite = HealthBarBackgroundImage;
                    HealthBarRef.sprite = HealthBarImage;
                }

                //Displays and colors our AI's name text, if enabled.
                if (DisplayAIName == YesOrNo.Yes)
                {
                    AINameUI = HealthBar.transform.Find("AI Name Text").gameObject.GetComponent<Text>();

                    if (UseAINameUIOutlineEffect == YesOrNo.Yes)
                    {
                        Outline AINameOutline = AINameUI.GetComponent<Outline>();
                        AINameOutline.effectDistance = AINameUIOutlineSize;
                        AINameOutline.effectColor = AINameUIOutlineColor;
                    }
                    else
                    {
                        AINameUI.GetComponent<Outline>().enabled = false;
                    }

                    if (DisplayAITitle == YesOrNo.Yes)
                    {
                        AIName = AIName + "\\n" + AITitle;
                        AIName = AIName.Replace("\\n", "\n");
                        AINamePos.y += 0.25f;

                        if (UseAINameUIOutlineEffect == YesOrNo.Yes)
                            AINameUI.lineSpacing = AINameLineSpacing;
                    }

                    AINameUI.transform.localPosition = new Vector3(AINamePos.x, AINamePos.y - HealthBarPos.y, AINamePos.z);
                    AINameUI.text = AIName;
                    AINameUI.fontSize = NameTextFontSize;
                    AINameUI.color = NameTextColor;

                    if (UseCustomFontAIName == YesOrNo.Yes)
                        AINameUI.font = AINameFont;
                }

                //Displays and colors our AI's level text, if enabled.
                if (DisplayAILevel == YesOrNo.Yes)
                {
                    AILevelUI = HealthBar.transform.Find("AI Level Text").gameObject.GetComponent<Text>();
                    AILevelUI.text = "   " + AILevel.ToString();
                    AILevelUI.color = LevelTextColor;
                    AILevelUI.transform.localPosition = new Vector3(AILevelPos.x, AILevelPos.y, AILevelPos.z);

                    if (UseCustomFontAILevel == YesOrNo.Yes)
                        AILevelUI.font = AILevelFont;

                    if (UseAINameUIOutlineEffect == YesOrNo.Yes)
                    {
                        Outline AINameOutline = AINameUI.GetComponent<Outline>();
                        AINameOutline.effectDistance = AINameUIOutlineSize;
                        AINameOutline.effectColor = AINameUIOutlineColor;
                    }
                    else
                    {
                        AILevelUI.GetComponent<Outline>().enabled = false;
                    }
                }

                HealthBarCanvasRef.enabled = false;
                if (AutoCreateHealthBars == YesOrNo.No)
                {
                    HealthBarBackgroundImageRef.gameObject.SetActive(false);
                }
                if (AINameUI != null && DisplayAIName == YesOrNo.Yes)
                {
                    AINameUI.gameObject.SetActive(false);
                }
                if (AILevelUI != null && DisplayAILevel == YesOrNo.Yes)
                {
                    AILevelUI.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Updates the AI's UI state, if it's enabled.
        /// </summary>
        void UpdateAIUI()
        {
            if (AutoCreateHealthBars == YesOrNo.Yes || DisplayAIName == YesOrNo.Yes)
            {
                Collider[] CurrentlyDetectedTargets = Physics.OverlapSphere(transform.position, EmeraldComponent.DetectionComponent.DetectionRadius, UILayerMask);
                if (CurrentlyDetectedTargets.Length > 0)
                {
                    List<Collider> TargetList = new List<Collider>();
                    for (int i = 0; i < CurrentlyDetectedTargets.Length; i++)
                    {
                        if (CurrentlyDetectedTargets[i].CompareTag(EmeraldComponent.DetectionComponent.PlayerTag))
                        {
                            TargetList.Add(CurrentlyDetectedTargets[i]);
                        }
                    }

                    if (TargetList.Count > 0)
                    {
                        SetUI(true);
                    }
                    else
                    {
                        SetUI(false);
                    }
                }
                else
                {
                    SetUI(false);
                }
            }
        }

        public void SetUI(bool Enabled)
        {
            if (EmeraldComponent.AnimationComponent.IsDead || HealthBarCanvas == null) return;

            m_HealthBarComponent.CalculateUI();
            HealthBarCanvasRef.enabled = Enabled;
            if (AutoCreateHealthBars == YesOrNo.Yes)
            {
                HealthBar.SetActive(Enabled);

                if (DisplayAILevel == YesOrNo.Yes)
                {
                    AILevelUI.gameObject.SetActive(Enabled);
                }
            }

            if (DisplayAIName == YesOrNo.Yes)
            {
                AINameUI.gameObject.SetActive(Enabled);
            }
        }
    }
}