using UnityEngine;
using UnityEditor;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldDebugger))]
    [CanEditMultipleObjects]
    public class EmeraldDebuggerEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture EventsEditorIcon;

        //Bools
        SerializedProperty SettingsFoldoutProp, HideSettingsFoldoutProp;
       
        SerializedProperty EnableDebuggingToolsProp, DrawLineOfSightLinesProp, DrawNavMeshPathProp, DrawNavMeshDestinationProp, DrawLookAtPointsProp, DrawUndetectedTargetsLineProp, DebugLogTargetsProp, DebugLogObstructionsProp, NavMeshPathColorProp, NavMeshDestinationColorProp,
            DrawFootstepPositions, DebugLogFootsteps;

        void OnEnable()
        {
            if (EventsEditorIcon == null) EventsEditorIcon = Resources.Load("Editor Icons/EmeraldDebugger") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            //Bool
            SettingsFoldoutProp = serializedObject.FindProperty("SettingsFoldout");
            HideSettingsFoldoutProp = serializedObject.FindProperty("HideSettingsFoldout");

            EnableDebuggingToolsProp = serializedObject.FindProperty("EnableDebuggingTools");
            DrawLineOfSightLinesProp = serializedObject.FindProperty("DrawLineOfSightLines");
            DrawNavMeshPathProp = serializedObject.FindProperty("DrawNavMeshPath");
            DrawNavMeshDestinationProp = serializedObject.FindProperty("DrawNavMeshDestination");
            DrawLookAtPointsProp = serializedObject.FindProperty("DrawLookAtPoints");
            DrawUndetectedTargetsLineProp = serializedObject.FindProperty("DrawUndetectedTargetsLine");
            DebugLogTargetsProp = serializedObject.FindProperty("DebugLogTargets");
            DebugLogObstructionsProp = serializedObject.FindProperty("DebugLogObstructions");
            NavMeshPathColorProp = serializedObject.FindProperty("NavMeshPathColor");
            NavMeshDestinationColorProp = serializedObject.FindProperty("NavMeshDestinationColor");
            DrawFootstepPositions = serializedObject.FindProperty("DrawFootstepPositions");
            DebugLogFootsteps = serializedObject.FindProperty("DebugLogFootsteps");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Debugger", EventsEditorIcon, new GUIContent(), HideSettingsFoldoutProp);

            if (!HideSettingsFoldoutProp.boolValue)
            {
                EditorGUILayout.Space();
                GeneralEvents();
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void GeneralEvents()
        {
            EmeraldDebugger self = (EmeraldDebugger)target;

            SettingsFoldoutProp.boolValue = EditorGUILayout.Foldout(SettingsFoldoutProp.boolValue, "Debugging Settings", true, FoldoutStyle);

            if (SettingsFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Debugging Settings", "Allows users to see lots of internal functionality and useful information to help identify issues or bugs. Control which debugging settings will be enabled. " +
                    "You can use the Enable Debugging Tools setting to disable all options so you can keep this component on AI until it's needed.", true);

                EditorGUILayout.PropertyField(EnableDebuggingToolsProp);
                CustomEditorProperties.CustomHelpLabelField("Controls whether or not the debugging tools are enabled.", true);

                if (self.EnableDebuggingTools == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent(15);
                    EditorGUILayout.PropertyField(DrawLineOfSightLinesProp);
                    CustomEditorProperties.CustomHelpLabelField("Allows Line of Sight raycasts to be draw, while in the Unity Editor. This can be useful for ensuring raycast are being positioned correctly as well as seeing if an AI's sight is being obstructed.", true);

                    EditorGUILayout.PropertyField(DrawNavMeshPathProp);
                    CustomEditorProperties.CustomHelpLabelField("Allows an AI's current path to be drawn.", true);
                    if (self.DrawNavMeshPath == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent();
                        EditorGUILayout.PropertyField(NavMeshPathColorProp);
                        CustomEditorProperties.CustomHelpLabelField("Controls the color of the NavMesh Path.", true);
                        CustomEditorProperties.EndIndent();
                    }

                    EditorGUILayout.PropertyField(DrawNavMeshDestinationProp);
                    CustomEditorProperties.CustomHelpLabelField("Allows an AI's current destination to be drawn. Note: This requires the Emerald Debugger to not be minimized.", true);
                    if (self.DrawNavMeshDestination == YesOrNo.Yes)
                    {
                        CustomEditorProperties.BeginIndent(15);
                        EditorGUILayout.PropertyField(NavMeshDestinationColorProp);
                        CustomEditorProperties.CustomHelpLabelField("Controls the color of the NavMesh Destination.", true);
                        CustomEditorProperties.EndIndent();
                    }

                    EditorGUILayout.PropertyField(DrawLookAtPointsProp);
                    CustomEditorProperties.CustomHelpLabelField("Allows the AI's current look at point to be draw (when using the InverseK inematics component). " +
                        "This can be useful to ensuring AI are looking at the right points of a target, including positions that are modified with a Target Position Modifier component.", true);

                    EditorGUILayout.PropertyField(DrawUndetectedTargetsLineProp);
                    CustomEditorProperties.CustomHelpLabelField("Allows targets within an AI's detection radius, that haven'transform been detected yet, to have a line drawn to them. This can happen if an undetected target is still obstructed or is outside of an AI's Field of View.", true);

                    EditorGUILayout.PropertyField(DebugLogObstructionsProp);
                    CustomEditorProperties.CustomHelpLabelField("Allows the AI's obstructions to be displayed in the Unity Console. This can be useful for identifying the AI's current obstruction between it and its target.", true);

                    EditorGUILayout.PropertyField(DebugLogTargetsProp);
                    CustomEditorProperties.CustomHelpLabelField("Allows the target objects to be displayed in the Unity Console. This can be useful for ensuring the proper object is being detected when the AI is targeting an object.", true);

                    EditorGUILayout.PropertyField(DrawFootstepPositions);
                    CustomEditorProperties.CustomHelpLabelField("Allows the AI's footsteps collision positions to be displayed (drawn as yellow circles). If no Footsteps Component is present, this setting will be ignored.", true);

                    EditorGUILayout.PropertyField(DebugLogFootsteps);
                    CustomEditorProperties.CustomHelpLabelField("Allows the AI's footsteps collision objects and used Footstep Surface Object to be displayed in the Unity Console. This can be helpful to ensure there are no unintended collisions " +
                        "and that the footstep caculations are detecting properly. If no Footsteps Component is present, this setting will be ignored.", true);
                    CustomEditorProperties.EndIndent();
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}