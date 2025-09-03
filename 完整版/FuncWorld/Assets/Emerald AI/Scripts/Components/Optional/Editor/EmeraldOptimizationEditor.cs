using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldOptimization))]
    [CanEditMultipleObjects]
    public class EmeraldOptimizationEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture OptimizationEditorIcon;

        #region SerializedProperties
        //Bool
        SerializedProperty HideSettingsFoldout, OptimizationFoldout;

        //Int
        SerializedProperty DeactivateDelayProp;

        //Object
        SerializedProperty AIRendererProp;

        //Enum
        SerializedProperty OptimizeAIProp, UseDeactivateDelayProp, TotalLODsProp, MeshTypeProp;
        #endregion

        void OnEnable()
        {
            if (OptimizationEditorIcon == null) OptimizationEditorIcon = Resources.Load("Editor Icons/EmeraldOptimization") as Texture;

            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            OptimizationFoldout = serializedObject.FindProperty("OptimizationFoldout");

            //Int
            DeactivateDelayProp = serializedObject.FindProperty("DeactivateDelay");

            //Object
            AIRendererProp = serializedObject.FindProperty("AIRenderer");

            //Enum
            OptimizeAIProp = serializedObject.FindProperty("OptimizeAI");
            UseDeactivateDelayProp = serializedObject.FindProperty("UseDeactivateDelay");
            TotalLODsProp = serializedObject.FindProperty("TotalLODsRef");
            MeshTypeProp = serializedObject.FindProperty("MeshType");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldOptimization self = (EmeraldOptimization)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Optimization", OptimizationEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingRendererMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                OptimizationSettings(self);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader();
        }

        void MissingRendererMessage(EmeraldOptimization self)
        {
            if (self.OptimizeAI == YesOrNo.Yes && self.MeshType == EmeraldOptimization.MeshTypes.SingleMesh && !self.AIRenderer)
            {
                CustomEditorProperties.DisplayWarningMessage("When using the Single Mesh Type, an AI needs to have an assigned Renderer. Please assign your AI's Skinned Mesh Renderer to the AI Renderer slot within the Optimization Settings foldout.");
            }
        }

        void OptimizationSettings (EmeraldOptimization self)
        {
            OptimizationFoldout.boolValue = EditorGUILayout.Foldout(OptimizationFoldout.boolValue, "Optimization Settings", true, FoldoutStyle);

            if (OptimizationFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Optimization Settings", "The optimization component will optimize an AI by disabling certain scripts, functionality, and animations when an AI's model is culled or not visible.", true);

                EditorGUILayout.PropertyField(OptimizeAIProp, new GUIContent("Optimize AI"));
                CustomEditorProperties.CustomHelpLabelField("Controls whether or not this AI will be optimized when off screen or culled.", true);

                if (self.OptimizeAI == YesOrNo.Yes)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.PropertyField(UseDeactivateDelayProp, new GUIContent("Use Deactivate Delay"));
                    CustomEditorProperties.CustomHelpLabelField("Controls whether or not there is a delay when using the Disable when Off-Screen feature. If set to No, the AI will be disabled instantly.", true);

                    if (self.UseDeactivateDelay == YesOrNo.Yes)
                    {
                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), DeactivateDelayProp, "Deactivate Delay", 1, 30);
                        CustomEditorProperties.CustomHelpLabelField("Controls the amount of seconds until the AI will be disabled when either culled or off-screen.", true);
                    }

                    CustomEditorProperties.CustomPropertyField(MeshTypeProp, "Mesh Type", "Controls whether this AI uses a single Skinned Mesh Renderer or a LOD Group.", true);

                    if (self.MeshType == EmeraldOptimization.MeshTypes.LODGroup)
                    {
                        CustomEditorProperties.DisplayImportantMessage("Info - The LOD Group option requires that an AI has a LOD Group component attached with at least 1 LOD Level. Each level needs to have at least 1 mesh assigned. " +
                            "The Optimization Component will automatically grab all needed information from the LOD Group during Start. Ensure your AI meets these requirements or the Optimization Component will be disabled.");
                        EditorGUILayout.Space();
                    }

                    if (self.MeshType == EmeraldOptimization.MeshTypes.SingleMesh)
                    {
                        CustomEditorProperties.BeginIndent();
                        EditorGUILayout.PropertyField(AIRendererProp, new GUIContent("AI Main Renderer"));
                        CustomEditorProperties.CustomHelpLabelField("The AI's Main Renderer should be a single Skinned Mesh Renderer an AI uses. If an AI has multiple Skinned Mesh Renderers, an LOD Group should be used instead.", true);
                        CustomEditorProperties.EndIndent();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}