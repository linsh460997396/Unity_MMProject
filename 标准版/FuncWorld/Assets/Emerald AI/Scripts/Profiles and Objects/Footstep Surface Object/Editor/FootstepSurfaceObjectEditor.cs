using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(FootstepSurfaceObject))]
    [CanEditMultipleObjects]
    public class FootstepSurfaceObjectEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture FootstepsEditorIcon;

        SerializedProperty HideSettingsFoldout, SurfaceSettingsFoldout, SurfaceType, SurfaceTexture, SurfaceTag, StepVolume, StepSounds, StepEffectTimeout, StepEffects, FootprintTimeout, Footprints;

        void OnEnable()
        {
            if (FootstepsEditorIcon == null) FootstepsEditorIcon = Resources.Load("Editor Icons/EmeraldFootsteps") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            //Variables
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            SurfaceSettingsFoldout = serializedObject.FindProperty("SurfaceSettingsFoldout");
            SurfaceType = serializedObject.FindProperty("SurfaceType");
            SurfaceTexture = serializedObject.FindProperty("SurfaceTextures");
            SurfaceTag = serializedObject.FindProperty("SurfaceTag");
            StepVolume = serializedObject.FindProperty("StepVolume");
            StepSounds = serializedObject.FindProperty("StepSounds");
            StepEffectTimeout = serializedObject.FindProperty("StepEffectTimeout");
            StepEffects = serializedObject.FindProperty("StepEffects");
            FootprintTimeout = serializedObject.FindProperty("FootprintTimeout");
            Footprints = serializedObject.FindProperty("Footprints");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            FootstepSurfaceObject self = (FootstepSurfaceObject)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Footstep Surface Settings", FootstepsEditorIcon, new GUIContent(), HideSettingsFoldout);

            EditorGUILayout.Space();
            FootstepSurfaceSettings(self);
            EditorGUILayout.Space();

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void FootstepSurfaceSettings (FootstepSurfaceObject self)
        {
            SurfaceSettingsFoldout.boolValue = EditorGUILayout.Foldout(SurfaceSettingsFoldout.boolValue, "Footstep Surface Settings", true, FoldoutStyle);

            if (SurfaceSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Footstep Surface Settings", "Controls the Footstep Surface Object settings. You can hover over each setting to get a tooltip of its usage.", true);

                CustomEditorProperties.BeginIndent(12);

                EditorGUILayout.PropertyField(SurfaceType);

                if (self.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Tag)
                {
                    EditorGUILayout.PropertyField(SurfaceTag);
                }
                else if (self.SurfaceType == FootstepSurfaceObject.SurfaceTypes.Texture)
                {
                    EditorGUILayout.PropertyField(SurfaceTexture);
                }

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(StepVolume);

                EditorGUILayout.PropertyField(StepSounds);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(StepEffectTimeout);

                EditorGUILayout.PropertyField(StepEffects);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(FootprintTimeout);

                EditorGUILayout.PropertyField(Footprints);

                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}