using UnityEngine;
using UnityEditor;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(TargetPositionModifier))]
    [CanEditMultipleObjects]
    [System.Serializable]
    public class TargetPositionModifierEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture TPMEditorIcon;
        SerializedProperty PositionModifierProp, TransformSourceProp, GizmoRadiusProp, GizmoColorProp, TPMSettingsFoldout, HideSettingsFoldout;

        private void OnEnable()
        {
            InitializeProperties();
        }

        void InitializeProperties()
        {
            if (TPMEditorIcon == null) TPMEditorIcon = Resources.Load("Editor Icons/EmeraldTPM") as Texture;
            PositionModifierProp = serializedObject.FindProperty("PositionModifier");
            TransformSourceProp = serializedObject.FindProperty("TransformSource");
            GizmoRadiusProp = serializedObject.FindProperty("GizmoRadius");
            GizmoColorProp = serializedObject.FindProperty("GizmoColor");
            TPMSettingsFoldout = serializedObject.FindProperty("TPMSettingsFoldout");
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();

            if (TransformSourceProp.objectReferenceValue == null)
            {
                CustomEditorProperties.DisplaySetupWarning("A Transform Source is required when using the Transform Position Source. Please assign one in order to use the Target Position Modifier.");
            }

            CustomEditorProperties.BeginScriptHeaderNew("Target Position Modifier", TPMEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                TPMSettings();
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void TPMSettings ()
        {
            TPMSettingsFoldout.boolValue = EditorGUILayout.Foldout(TPMSettingsFoldout.boolValue, "Target Position Modifier Settings", true, FoldoutStyle);

            if (TPMSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Target Position Modifier Settings", "This system modifies the target height of targets allowing AI agents to target the sphere gizmo shown.", false);

                CustomEditorProperties.NoticeTextDescription("Enusre that the gizmo does not go into the ground or this could make a target undetectable to AI. If you can'transform see the gizmo, ensure that Unity' Gizmos are enabled.", false);

                CustomEditorProperties.TutorialButton("For a detailed tutorial on using the Target Position Modifier, please see the tutorial below.", "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/target-position-modifier-component");
                EditorGUILayout.Space();

                if (TransformSourceProp.objectReferenceValue == null)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("This field cannot be left blank", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.PropertyField(TransformSourceProp, new GUIContent("Transform Source"));
                CustomEditorProperties.CustomHelpLabelField("The Transform Source should be the transform you want the Target Position Modifier based off of. It is recommended that this is a bone transform that is close the the center of your AI such as its chest or spine.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), PositionModifierProp, "Height Modifier", -5, 5);
                CustomEditorProperties.CustomHelpLabelField("Controls the height offset of the position modifier.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), GizmoRadiusProp, "Gizmo Radius", 0.05f, 2.5f);
                CustomEditorProperties.CustomHelpLabelField("Controls the radius of the sphere gizmo.", true);

                CustomEditorProperties.CustomColorField(new Rect(), new GUIContent(), GizmoColorProp, "Gizmo Color");
                CustomEditorProperties.CustomHelpLabelField("Controls the color of the sphere gizmo.", true);

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();

            }
        }
    }
}