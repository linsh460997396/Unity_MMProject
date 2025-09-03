using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldUI))]
    [CanEditMultipleObjects]
    public class EmeraldUIEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture UIEditorIcon;

        #region SerializedProperties
        //Ints
        SerializedProperty MaxUIScaleSizeProp;

        //Enums
        SerializedProperty CreateHealthBarsProp, UseCustomFontAINameProp, UseCustomFontAILevelProp, CustomizeHealthBarProp, DisplayAINameProp, DisplayAITitleProp, DisplayAILevelProp, UseAINameUIOutlineEffectProp, UseAILevelUIOutlineEffectProp;

        //Bools
        SerializedProperty HideSettingsFoldout, UISettingsFoldoutProp, HealthBarsFoldoutProp, CombatTextFoldoutProp, NameTextFoldoutProp, LevelTextFoldoutProp;

        //Layermask
        SerializedProperty UILayerMaskProp;

        //Float
        SerializedProperty AINameLineSpacingProp;

        //Colors
        SerializedProperty HealthBarColorProp, HealthBarColorDamageProp, HealthBarBackgroundColorProp, NameTextColorProp, LevelTextColorProp, AINameUIOutlineColorProp, AILevelUIOutlineColorProp, AINameFontProp, AILevelFontProp;

        //Vectors
        SerializedProperty AINamePosProp, AILevelPosProp, AINameUIOutlineSizeProp, AILevelUIOutlineSizeProp, HealthBarPosProp, NameTextFontSizeProp, HealthBarScaleProp;

        //Objects
        SerializedProperty HealthBarImageProp, HealthBarBackgroundImageProp;

        //String
        SerializedProperty UITagProp, CameraTagProp, AINameProp, AITitleProp, AILevelProp;
        #endregion

        void OnEnable()
        {
            if (UIEditorIcon == null) UIEditorIcon = Resources.Load("Editor Icons/EmeraldUI") as Texture;
            InitializeProperties();
        }

        void InitializeProperties ()
        {
            //Int
            MaxUIScaleSizeProp = serializedObject.FindProperty("MaxUIScaleSize");

            //Floats
            AINameLineSpacingProp = serializedObject.FindProperty("AINameLineSpacing");

            //Enums
            UseAINameUIOutlineEffectProp = serializedObject.FindProperty("UseAINameUIOutlineEffect");
            UseAILevelUIOutlineEffectProp = serializedObject.FindProperty("UseAILevelUIOutlineEffect");
            CreateHealthBarsProp = serializedObject.FindProperty("AutoCreateHealthBars");
            CustomizeHealthBarProp = serializedObject.FindProperty("UseCustomHealthBar");
            DisplayAINameProp = serializedObject.FindProperty("DisplayAIName");
            DisplayAITitleProp = serializedObject.FindProperty("DisplayAITitle");
            DisplayAILevelProp = serializedObject.FindProperty("DisplayAILevel");
            UseCustomFontAINameProp = serializedObject.FindProperty("UseCustomFontAIName");
            UseCustomFontAILevelProp = serializedObject.FindProperty("UseCustomFontAILevel");

            //Bools
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            UISettingsFoldoutProp = serializedObject.FindProperty("UISettingsFoldout");
            HealthBarsFoldoutProp = serializedObject.FindProperty("HealthBarsFoldout");
            CombatTextFoldoutProp = serializedObject.FindProperty("CombatTextFoldout");
            NameTextFoldoutProp = serializedObject.FindProperty("NameTextFoldout");
            LevelTextFoldoutProp = serializedObject.FindProperty("LevelTextFoldout");

            //Layermask
            UILayerMaskProp = serializedObject.FindProperty("UILayerMask");

            //Vectors
            HealthBarPosProp = serializedObject.FindProperty("HealthBarPos");
            NameTextFontSizeProp = serializedObject.FindProperty("NameTextFontSize");
            HealthBarScaleProp = serializedObject.FindProperty("HealthBarScale");
            AINamePosProp = serializedObject.FindProperty("AINamePos");
            AINameUIOutlineSizeProp = serializedObject.FindProperty("AINameUIOutlineSize");
            AILevelPosProp = serializedObject.FindProperty("AILevelPos");
            AILevelUIOutlineSizeProp = serializedObject.FindProperty("AILevelUIOutlineSize");

            //Color
            HealthBarColorProp = serializedObject.FindProperty("HealthBarColor");
            HealthBarColorDamageProp = serializedObject.FindProperty("HealthBarDamageColor");
            HealthBarBackgroundColorProp = serializedObject.FindProperty("HealthBarBackgroundColor");
            NameTextColorProp = serializedObject.FindProperty("NameTextColor");
            LevelTextColorProp = serializedObject.FindProperty("LevelTextColor");
            AINameUIOutlineColorProp = serializedObject.FindProperty("AINameUIOutlineColor");
            AILevelUIOutlineColorProp = serializedObject.FindProperty("AILevelUIOutlineColor");
            AINameFontProp = serializedObject.FindProperty("AINameFont");
            AILevelFontProp = serializedObject.FindProperty("AILevelFont");

            //String
            UITagProp = serializedObject.FindProperty("UITag");
            CameraTagProp = serializedObject.FindProperty("CameraTag");
            AINameProp = serializedObject.FindProperty("AIName");
            AITitleProp = serializedObject.FindProperty("AITitle");
            AILevelProp = serializedObject.FindProperty("AILevel");

            //Objects
            HealthBarImageProp = serializedObject.FindProperty("HealthBarImage");
            HealthBarBackgroundImageProp = serializedObject.FindProperty("HealthBarBackgroundImage");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldUI self = (EmeraldUI)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("UI", UIEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                UISettings(self);
                EditorGUILayout.Space();
                NameTextSettings(self);
                EditorGUILayout.Space();
                LevelTextSettings(self);
                EditorGUILayout.Space();
                HealthbarSettings(self);
                EditorGUILayout.Space();
                CombatTextSettings(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();
            serializedObject.ApplyModifiedProperties();
        }

        void UISettings (EmeraldUI self)
        {
            UISettingsFoldoutProp.boolValue = CustomEditorProperties.Foldout(UISettingsFoldoutProp.boolValue, "UI Setup", true, FoldoutStyle);

            if (UISettingsFoldoutProp.boolValue)
            {
                 CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("UI Setup", "Controls the use and setup of Emerald's built-in UI. In order for the UI to be visible, a player of the appropriate tag must enter an AI's trigger radius. " +
                "You can set an AI's UI Tag under the Detection and Tag tab.", true);

                GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
                EditorGUILayout.LabelField("In order for the UI system to work correctly, you will need to assign a Tag and Layer. This is typically your Player's Tag and Layer. " +
                    "This is used to make the UI system more efficient by only running when the appropriate objects are detected. You will also need to apply your player's camera Tag so the UI can be properly positioned.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space();
                CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), CameraTagProp, "Camera Tag");
                CustomEditorProperties.CustomHelpLabelField("The Camera Tag is the Unity Tag that your player uses. The Camera is needed to properly position the UI.", true);

                CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), UITagProp, "UI Tag");
                CustomEditorProperties.CustomHelpLabelField("The UI Tag is the Unity Tag that will trigger the AI's UI, when enabled.", true);

                EditorGUILayout.PropertyField(UILayerMaskProp, new GUIContent("UI Layers"));
                CustomEditorProperties.CustomHelpLabelField("The UI Layers controls what layers this AI will detect to enable the their UI, if the object also has the appropriate UI Tag. This is typically used for players.", false);

                if (UILayerMaskProp.intValue == 0 || UILayerMaskProp.intValue == 1)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("The UI Layers cannot contain Nothing, Default, or Everything.", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(MaxUIScaleSizeProp, new GUIContent("Max UI Scale"));
                CustomEditorProperties.CustomHelpLabelField("Controls the max size the UI will be scaled when the player is getting further away from an AI's UI.", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NameTextSettings (EmeraldUI self)
        {
            NameTextFoldoutProp.boolValue = CustomEditorProperties.Foldout(NameTextFoldoutProp.boolValue, "Name Text Settings", true, FoldoutStyle);

            if (NameTextFoldoutProp.boolValue)
            {
                 CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Name Text Settings", "Settings for displaying and positioning this AI's Name using Emerald AI's built-in (Unity-based) UI System.", true);               

                EditorGUILayout.PropertyField(DisplayAINameProp, new GUIContent("Display AI Name"));
                CustomEditorProperties.CustomHelpLabelField("Enables or disables the display of the AI's name. When enabled, the AI's name will be visible above its health bar.", true);

                if (self.DisplayAIName == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    EditorGUILayout.PropertyField(AINameProp, new GUIContent("AI's Name"));
                    CustomEditorProperties.CustomHelpLabelField("The name of the AI. This can be displayed with Emerald's built-in UI system or a custom one.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(UseCustomFontAINameProp, new GUIContent("Use Custom Name Font"));
                    CustomEditorProperties.CustomHelpLabelField("Controls whether or not the Name Text font can be customized.", false);
                    EditorGUILayout.Space();

                    if (self.UseCustomFontAIName == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();
                        EditorGUILayout.PropertyField(AINameFontProp, new GUIContent("Name Font"));
                        CustomEditorProperties.CustomHelpLabelField("Controls the font of the AI's Name Text.", true);
                        CustomEditorProperties.EndIndent();
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.PropertyField(AINamePosProp, new GUIContent("AI Name Position"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the position of the AI's name text.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(NameTextFontSizeProp, new GUIContent("AI Name Font Size"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the size of the AI's name text.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(NameTextColorProp, new GUIContent("AI Name Color"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the color of the AI's name text.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(UseAINameUIOutlineEffectProp, new GUIContent("Use Outline on Name Text"));
                    CustomEditorProperties.CustomHelpLabelField("Controls whether or not the AI's Name UI will use an Outline Effect.", true);
                    EditorGUILayout.Space();

                    if (self.UseAINameUIOutlineEffect == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();

                        EditorGUILayout.PropertyField(AINameUIOutlineColorProp, new GUIContent("Name Text Outline Color"));
                        CustomEditorProperties.CustomHelpLabelField("Controls the color of the AI's Name Text Outline.", true);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(AINameUIOutlineSizeProp, new GUIContent("Name Text Outline Size"));
                        CustomEditorProperties.CustomHelpLabelField("Controls the size of the AI's Name Text Outline.", true);
                        EditorGUILayout.Space();

                        CustomEditorProperties.EndIndent();
                    }

                    EditorGUILayout.PropertyField(DisplayAITitleProp, new GUIContent("Display AI Title"));
                    CustomEditorProperties.CustomHelpLabelField("Enables or disables the display of the AI's title. When enabled, the AI's title will be visible above its health bar.", false);

                    if (self.DisplayAITitle == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();

                        EditorGUILayout.PropertyField(AITitleProp, new GUIContent("AI's Title"));
                        CustomEditorProperties.CustomHelpLabelField("The title of the AI. This can be displayed with Emerald's built-in UI system or a custom one.", true);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(AINameLineSpacingProp, new GUIContent("Name Line Spacing"));
                        CustomEditorProperties.CustomHelpLabelField("Controls the line spacing between the AI's Name and the AI's Title.", true);

                        CustomEditorProperties.EndIndent();
                    }

                    EditorGUILayout.Space();
                    CustomEditorProperties.EndIndent();
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void LevelTextSettings(EmeraldUI self)
        {
            LevelTextFoldoutProp.boolValue = CustomEditorProperties.Foldout(LevelTextFoldoutProp.boolValue, "Level Text Settings", true, FoldoutStyle);

            if (LevelTextFoldoutProp.boolValue)
            {
                 CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Name Text Settings", "Settings for displaying and positioning this AI's Level using Emerald AI's built-in (Unity-based) UI System.", true);

                //TODO: Remove this limitation?
                /*
                if (self.AutoCreateHealthBars == YesOrNo.No)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("You must have Auto Create Health Bars enabled to use this feature.", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                */
                EditorGUILayout.PropertyField(DisplayAILevelProp, new GUIContent("Display AI Level"));
                CustomEditorProperties.CustomHelpLabelField("Enables or disables the display of the AI's level. When enabled, the AI's level will be visible to the left of its health bar.", true);

                if (self.DisplayAILevel == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), AILevelProp, "AI's Level");
                    CustomEditorProperties.CustomHelpLabelField("The level of the AI. This can be displayed with Emerald's built-in UI system or a custom one.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(UseCustomFontAILevelProp, new GUIContent("Use Custom Level Font"));
                    CustomEditorProperties.CustomHelpLabelField("Controls whether or not the Level Text font can be customized.", true);
                    EditorGUILayout.Space();

                    if (self.UseCustomFontAILevel == YesOrNo.Yes)
                    {
                        EditorGUILayout.PropertyField(AILevelFontProp, new GUIContent("Level Font"));
                        CustomEditorProperties.CustomHelpLabelField("Controls the font of the AI's Level Text.", true);
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.PropertyField(AILevelPosProp, new GUIContent("AI Level Position"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the position of the AI's level text.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(LevelTextColorProp, new GUIContent("Level Color"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the color of the AI's Level Text.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(UseAILevelUIOutlineEffectProp, new GUIContent("Use Outline on Level Text"));
                    CustomEditorProperties.CustomHelpLabelField("Controls whether or not the AI's Level UI will use an Outline Effect.", true);
                    EditorGUILayout.Space();

                    if (self.UseAILevelUIOutlineEffect == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();

                        EditorGUILayout.PropertyField(AILevelUIOutlineColorProp, new GUIContent("Level Text Outline Color"));
                        CustomEditorProperties.CustomHelpLabelField("Controls the color of the AI's Level Text Outline.", true);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(AILevelUIOutlineSizeProp, new GUIContent("Level Text Outline Size"));
                        CustomEditorProperties.CustomHelpLabelField("Controls the size of the AI's Level Text Outline.", true);
                        EditorGUILayout.Space();

                        CustomEditorProperties.EndIndent();
                    }

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void HealthbarSettings (EmeraldUI self)
        {
            HealthBarsFoldoutProp.boolValue = CustomEditorProperties.Foldout(HealthBarsFoldoutProp.boolValue, "Health Bar Settings", true, FoldoutStyle);

            if (HealthBarsFoldoutProp.boolValue)
            {
                 CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Health Bar Settings", "Settings for displaying and positioning this AI's Health Bar using Emerald AI's built-in (Unity-based) UI System.", true);

                EditorGUILayout.PropertyField(CreateHealthBarsProp, new GUIContent("Auto Create Health Bars"));
                CustomEditorProperties.CustomHelpLabelField("Enables or disables the use of Emerald automatically creating health bars for your AI. Enabling this will open up additional settings.", true);
                EditorGUILayout.Space();

                if (self.AutoCreateHealthBars == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    EditorGUILayout.PropertyField(HealthBarPosProp, new GUIContent("Health Bar Position"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the starting position of the AI's created health bar.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarScaleProp, new GUIContent("Health Bar Scale"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the scale of the AI's created health bar.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarColorProp, new GUIContent("Health Bar Color"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the color of the AI's health bar.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarColorDamageProp, new GUIContent("Health Bar Damage Color"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the color of the AI's health bar when damaged.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HealthBarBackgroundColorProp, new GUIContent("Background Color"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the background color of the AI's health bar.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(CustomizeHealthBarProp, new GUIContent("Use Custom Health Bar"));
                    CustomEditorProperties.CustomHelpLabelField("Allows you to use custom sprites for the AI's health bar.", true);
                    EditorGUILayout.Space();

                    if (self.UseCustomHealthBar == YesOrNo.Yes)
                    {
                        EditorGUILayout.LabelField("Health Bar Sprites", EditorStyles.boldLabel);
                        EditorGUILayout.Space();

                        CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), HealthBarImageProp, "Bar", typeof(Sprite), true);
                        CustomEditorProperties.CustomHelpLabelField("Customizes the health bar sprite for the AI's health bar.", true);

                        CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), HealthBarBackgroundImageProp, "Bar Background", typeof(Sprite), true);
                        CustomEditorProperties.CustomHelpLabelField("Customizes the health bar's background sprite for the AI's health bar.", true);
                    }

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void CombatTextSettings (EmeraldUI self)
        {
            CombatTextFoldoutProp.boolValue = CustomEditorProperties.Foldout(CombatTextFoldoutProp.boolValue, "Combat Text Settings", true, FoldoutStyle);

            if (CombatTextFoldoutProp.boolValue)
            {
                 CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Combat Text Settings", "A shortcut to Emerald AI's built-in (global) Combat Text System.", true);

                CustomEditorProperties.CustomHelpLabelField("The Combat Text System can be adjusted through the Combat Text Manager. These settings are applied globally.", false);
                var ButtonStyle = new GUIStyle(GUI.skin.button);
                if (GUILayout.Button("Open Combat Text Manager", ButtonStyle))
                {
                    EditorWindow CTM = EditorWindow.GetWindow(typeof(EmeraldCombatTextManager), true, "Combat Text Manager");
                    CTM.minSize = new Vector2(600f, 725f);
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        private void OnSceneGUI()
        {
            EmeraldUI self = (EmeraldUI)target;
            DrawUIPositions(self);
        }

        void DrawUIPositions(EmeraldUI self)
        {
            if (self == null) return;

            if (self.DisplayAIName == YesOrNo.Yes && self.NameTextFoldout)
            {
                Handles.color = self.NameTextColor;
                Handles.DrawLine(new Vector3(self.transform.localPosition.x, self.transform.localPosition.y, self.transform.localPosition.z),
                    new Vector3(self.AINamePos.x, self.AINamePos.y, self.AINamePos.z) + self.transform.localPosition);
                Handles.color = Color.white;
            }

            if (self.AutoCreateHealthBars == YesOrNo.Yes && self.HealthBarsFoldout)
            {
                Handles.color = self.HealthBarColor;
                Handles.DrawLine(new Vector3(self.transform.localPosition.x + 0.25f, self.transform.localPosition.y, self.transform.localPosition.z),
                    new Vector3(self.HealthBarPos.x + 0.25f, self.HealthBarPos.y, self.HealthBarPos.z) + self.transform.localPosition);
                Handles.color = Color.white;
            }
        }
    }
}