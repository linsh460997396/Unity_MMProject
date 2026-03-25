using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(FactionExtension))]
    [CanEditMultipleObjects]
    public class FactionExtensionEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture FactionExtensionEditorIcon;
        SerializedProperty CurrentFactionProp, HideSettingsFoldout, FactionFoldout;

        void OnEnable()
        {
            if (FactionExtensionEditorIcon == null) FactionExtensionEditorIcon = Resources.Load("Editor Icons/FactionExtension") as Texture;
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            CurrentFactionProp = serializedObject.FindProperty("CurrentFaction");
            FactionFoldout = serializedObject.FindProperty("FactionFoldout");
            LoadFactionData();
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            FactionExtension self = (FactionExtension)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Faction Extension", FactionExtensionEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                FactionSetting(self);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader(); 
        }

        void FactionSetting (FactionExtension self)
        {
            FactionFoldout.boolValue = EditorGUILayout.Foldout(FactionFoldout.boolValue, "Faction Settings", true, FoldoutStyle);

            if (FactionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Faction Settings", "Allows AI to identify this gameobject without having to rely on Unity's Tag system. This means all potential targets can share the same Unity Tag and Unity Layer.", true);

                CustomEditorProperties.FactionListEnum(new Rect(), new GUIContent(), CurrentFactionProp, "Faction", FactionExtension.StringFactionList);
                CustomEditorProperties.CustomHelpLabelField("This Faction is used to identify this gameobject and is indended to be used on non-AI objects such as players. This is the name that AI will use when " +
                    "looking for targets.", true);

                CustomEditorProperties.CustomHelpLabelField("Factions can be created and removed using the Faction Manager. ", false);
                if (GUILayout.Button("Open Faction Manager"))
                {
                    EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager));
                    APS.minSize = new Vector2(600f, 775f);
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }


        void LoadFactionData()
        {
            FactionExtension.StringFactionList.Clear();
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));

            if (FactionData != null)
            {
                foreach (string s in FactionData.FactionNameList)
                {
                    if (!FactionExtension.StringFactionList.Contains(s) && s != "")
                    {
                        FactionExtension.StringFactionList.Add(s);
                    }
                }
            }
        }
    }
}