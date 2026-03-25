using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldDecals))]
    [CanEditMultipleObjects]
    public class EmeraldDecalsEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture EventsEditorIcon;

        SerializedProperty HideSettingsFoldout, DecalsFoldout, BloodEffects, BloodSpawnHeight, BloodSpawnDelay, BloodSpawnRadius, BloodDespawnTime, OddsForBlood;

        void OnEnable()
        {
            if (EventsEditorIcon == null) EventsEditorIcon = Resources.Load("Editor Icons/EmeraldDecals") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            DecalsFoldout = serializedObject.FindProperty("DecalsFoldout");
            BloodEffects = serializedObject.FindProperty("BloodEffects");
            BloodSpawnHeight = serializedObject.FindProperty("BloodSpawnHeight");
            BloodSpawnDelay = serializedObject.FindProperty("BloodSpawnDelay");
            BloodSpawnRadius = serializedObject.FindProperty("BloodSpawnRadius");
            BloodDespawnTime = serializedObject.FindProperty("BloodDespawnTime");
            OddsForBlood = serializedObject.FindProperty("OddsForBlood");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldDecals self = (EmeraldDecals)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Decals", EventsEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DecalSettings(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void DecalSettings(EmeraldDecals self)
        {
            DecalsFoldout.boolValue = EditorGUILayout.Foldout(DecalsFoldout.boolValue, "Decal Settings", true, FoldoutStyle);

            if (DecalsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Decal Settings", "The decal settings gives control over how decal prefabs will be spawned and positioned when an AI is damaged.", true);

                if (UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline == null && !self.MessageDismissed)
                {
                    CustomEditorProperties.DisplayImportantMessage("This componenent is intended to be used with URP's or HDRP's decal systems (or if you have your own solution). This component does not create decals, but spawns and positions prefab decals. You will need to ensure decals are enabled through your Render Pipeline Asset.");
                    if (GUILayout.Button(new GUIContent("Dismiss Message", "Stops this message from being displayed."), GUILayout.Height(20)))
                    {
                        self.MessageDismissed = true;
                    }
                    GUILayout.Space(15);
                }
                //if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name == )   

                EditorGUILayout.PropertyField(BloodSpawnHeight);
                CustomEditorProperties.CustomHelpLabelField("Controls the spawning height of the decal. Additional height can help if decals are clipping through sloped terrains.", true);

                EditorGUILayout.PropertyField(BloodSpawnRadius);
                CustomEditorProperties.CustomHelpLabelField("Controls the radius in which the decal can be spawned from the AI's position.", true);

                EditorGUILayout.PropertyField(BloodSpawnDelay);
                CustomEditorProperties.CustomHelpLabelField("Controls the delay (in seconds) for a decal to spawn after being successfully damaged.", true);

                EditorGUILayout.PropertyField(BloodDespawnTime);
                CustomEditorProperties.CustomHelpLabelField("Controls the length (in seconds) it takes for the decal to despawn.", true);
                GUILayout.Space(15);

                CustomEditorProperties.CustomHelpLabelField("A list of possible decals that can be spawned.", false);
                CustomEditorProperties.BeginIndent(15);
                EditorGUILayout.PropertyField(BloodEffects);
                CustomEditorProperties.EndIndent();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}