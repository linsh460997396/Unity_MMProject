using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace EmeraldAI.Utility
{
    public class EmeraldCombatTextManager : EditorWindow
    {
        EmeraldCombatTextData m_EmeraldAICombatTextData;
        GUIStyle TitleStyle;
        Texture CombatTextIcon;
        int CurrentTab = 0;

        SerializedObject serializedObject;
        SerializedProperty TextFont;
        SerializedProperty CombatTextState;
        SerializedProperty PlayerTextColor;
        SerializedProperty PlayerCritTextColor;
        SerializedProperty PlayerTakeDamageTextColor;
        SerializedProperty AITextColor;
        SerializedProperty AICritTextColor;
        SerializedProperty HealingTextColor;
        SerializedProperty FontSize;
        SerializedProperty MaxFontSize;
        SerializedProperty AnimationType;
        SerializedProperty CombatTextTargets;
        SerializedProperty OutlineEffect;
        SerializedProperty UseAnimateFontSize;
        SerializedProperty TextHeight;
        SerializedProperty VRSupport;

        [MenuItem("Window/Emerald AI/Combat Text Manager #%c", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldCombatTextManager), false, "Combat Text Manager");
            APS.minSize = new Vector2(600f, 650);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        protected virtual void OnEnable()
        {
            m_EmeraldAICombatTextData = (EmeraldCombatTextData)Resources.Load("Combat Text Data") as EmeraldCombatTextData;
            if (CombatTextIcon == null) CombatTextIcon = Resources.Load("Editor Icons/EmeraldCTM") as Texture;

            serializedObject = new SerializedObject(m_EmeraldAICombatTextData);
            TextFont = serializedObject.FindProperty("TextFont");
            CombatTextState = serializedObject.FindProperty("CombatTextState");
            PlayerTextColor = serializedObject.FindProperty("PlayerTextColor");
            PlayerCritTextColor = serializedObject.FindProperty("PlayerCritTextColor");
            PlayerTakeDamageTextColor = serializedObject.FindProperty("PlayerTakeDamageTextColor");
            AITextColor = serializedObject.FindProperty("AITextColor");
            AICritTextColor = serializedObject.FindProperty("AICritTextColor");
            HealingTextColor = serializedObject.FindProperty("HealingTextColor");
            FontSize = serializedObject.FindProperty("FontSize");
            MaxFontSize = serializedObject.FindProperty("MaxFontSize");
            AnimationType = serializedObject.FindProperty("AnimationType");
            CombatTextTargets = serializedObject.FindProperty("CombatTextTargets");
            OutlineEffect = serializedObject.FindProperty("OutlineEffect");
            UseAnimateFontSize = serializedObject.FindProperty("UseAnimateFontSize");
            TextHeight = serializedObject.FindProperty("DefaultHeight");
            VRSupport = serializedObject.FindProperty("VRSupport");
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); //Top Left Side Indent
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "Combat Text Manager", CombatTextIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  //Top Right Side Indent
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); //Bottom Left Side Indent
            EditorGUILayout.BeginVertical();

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription("Combat Text Manager", "With the Combat Text Manager, you can globally handle all combat text settings such as text size, text font, text color, text animation, and more.", true);


            GUIContent[] CombatTextManagerButtons = new GUIContent[2] { new GUIContent("Combat Text Settings"), new GUIContent("Combat Text Colors") };
            CurrentTab = GUILayout.Toolbar(CurrentTab, CombatTextManagerButtons, EditorStyles.miniButton, GUILayout.Height(25));

            CombatTextSettings();
            ColorSettings();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
            GUILayout.Space(15); //Bottom Right Side Indent
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying && GUI.changed)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        void CombatTextSettings()
        {
            if (CurrentTab == 0)
            {
                GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Combat Text Settings", EditorStyles.boldLabel);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(CombatTextState, new GUIContent("Combat Text State"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("Controls the whether the Combat Text System is enabled or disabled.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(VRSupport, new GUIContent("VR Support"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("Changes the Combat Text System's canvas to World Space to allow it to work with VR (this setting cannot be changed during runtime as it's set up when the CTS is initialized).", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(TextHeight, new GUIContent("Combat Text Height"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("Controls the overall height the Combat Text will spawn above targets.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(CombatTextTargets, new GUIContent("Combat Text Targets"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the target types that will be able to have the Combat Text displayed.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(OutlineEffect, new GUIContent("Use Outline Effect"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the whether the Combat Text System will use a text outline effect.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(AnimationType, new GUIContent("Animation Type"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the Combat Text's Animation Type.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), TextFont, "Text Font", typeof(Font), true);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("The Combat Text's font.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), FontSize, "Font Size", 10, 50);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("Controls the size of the Combat Text's font.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(UseAnimateFontSize, new GUIContent("Use Animate Font"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls whether or not the font size is animated.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                if (m_EmeraldAICombatTextData.UseAnimateFontSize == EmeraldCombatTextData.UseAnimateFontSizeEnum.Enabled)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxFontSize, "Animated Font Size", 1, 30);
                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                    EditorGUILayout.HelpBox("Controls the additional size of the Combat Text's font when Animate Size is enabled. After the text has been animated, it will return to the Font Size.", MessageType.None, true);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space();

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
        }

        void ColorSettings ()
        {
            if (CurrentTab == 1)
            {
                GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Combat Text Colors", EditorStyles.boldLabel);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(PlayerTextColor, new GUIContent("Player Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("The Combat Text's color when used by the player.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(PlayerTakeDamageTextColor, new GUIContent("Take Damage Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("The Combat Text's color when a player takes damage.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(PlayerCritTextColor, new GUIContent("Player Crit Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("The Combat Text's critical hit color when used by the player.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(AITextColor, new GUIContent("AI Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("The Combat Text's AI color when used between other AI.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(AICritTextColor, new GUIContent("AI Crit Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("The Combat Text's critical hit color when used between other AI.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(HealingTextColor, new GUIContent("Healing Color"));
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.HelpBox("The Combat Text's color when used for healing.", MessageType.None, true);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
            }
        }
    }
}
