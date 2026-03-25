using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldAction), true)]
    public class ActionObjectEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture ActionEditorIcon;
        FieldInfo[] CustomFields;
        SerializedProperty HideSettingsFoldout, DefaultSettingsFoldout, CustomSettingsFoldout, InfoSettingsFoldout, EnterConditions, ExitConditions, CooldownConditions, CooldownLength, UseCooldown, ActionName;

        void OnEnable()
        {
            if (ActionEditorIcon == null) ActionEditorIcon = Resources.Load("Editor Icons/EmeraldBehaviors") as Texture;
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            DefaultSettingsFoldout = serializedObject.FindProperty("DefaultSettingsFoldout");
            CustomSettingsFoldout = serializedObject.FindProperty("CustomSettingsFoldout");
            InfoSettingsFoldout = serializedObject.FindProperty("InfoSettingsFoldout");

            EnterConditions = serializedObject.FindProperty("EnterConditions");
            ExitConditions = serializedObject.FindProperty("ExitConditions");
            CooldownConditions = serializedObject.FindProperty("CooldownConditions");
            CooldownLength = serializedObject.FindProperty("CooldownLength");
            UseCooldown = serializedObject.FindProperty("UseCooldown");
            ActionName = serializedObject.FindProperty("ActionName");

            //Get all variables that are not part of the parent class.
            CustomFields = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public override void OnInspectorGUI()
        {
            EmeraldAction self = (EmeraldAction)target;
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();
            CustomEditorProperties.BeginScriptHeaderNew(self.ActionName, ActionEditorIcon, new GUIContent(), HideSettingsFoldout, false);

            /*
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(-15f);
            CustomEditorProperties.TextTitleWithDescription(self.ActionName + " Action", self.ActionDescription, false);
            GUILayout.Space(-4f);
            EditorGUILayout.EndHorizontal();
            */

            EditorGUILayout.Space();
            DefaultSettings(self);
            EditorGUILayout.Space();
            CustomSettings(self);
            EditorGUILayout.Space();
            InfoSettings(self);
            EditorGUILayout.Space();
            CustomEditorProperties.EndScriptHeader();
            serializedObject.ApplyModifiedProperties();
        }

        void DefaultSettings(EmeraldAction self)
        {
            DefaultSettingsFoldout.boolValue = EditorGUILayout.Foldout(DefaultSettingsFoldout.boolValue, "Default Settings", true, FoldoutStyle);

            if (DefaultSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Default Settings", "All default parent variables. Any child variables will be added to the foldout below. " +
                    "You can customize the name and description of this action through the Info Settings foldout.", true);

                EditorGUILayout.PropertyField(EnterConditions);
                EditorGUILayout.PropertyField(ExitConditions);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(UseCooldown);

                if (self.UseCooldown)
                {
                    CustomEditorProperties.BeginIndent();
                    EditorGUILayout.PropertyField(CooldownConditions);
                    EditorGUILayout.PropertyField(CooldownLength);
                    CustomEditorProperties.EndIndent();
                }
                else
                {
                    CustomEditorProperties.NoticeTextDescription("Disabling Cooldowns will allow an Action to run continiously based on its Enter Conditions or/and coded functionality.", false);
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Displays all custom variables in a separate part of the editor.
        /// </summary>
        void CustomSettings (EmeraldAction self)
        {
            CustomSettingsFoldout.boolValue = EditorGUILayout.Foldout(CustomSettingsFoldout.boolValue, self.ActionName + " Settings", true, FoldoutStyle);

            if (CustomSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription(self.ActionName + " Settings", self.ActionDescription, true);

                foreach (FieldInfo field in CustomFields)
                {
                    //Offset Arrays with extra space
                    if (field.FieldType.GetElementType() != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    //Offset Lists with extra space
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    //Don'transform apply an offset to single variables
                    else
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                    }
                }
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void InfoSettings (EmeraldAction self)
        {
            InfoSettingsFoldout.boolValue = EditorGUILayout.Foldout(InfoSettingsFoldout.boolValue, "Info Settings", true, FoldoutStyle);

            if (InfoSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Info Settings", "Customize the name and description of this action.", true);

                CustomEditorProperties.CustomStringPropertyField(ActionName, "Action Name", "", true);

                GUIStyle style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Action Description");
                string Value = EditorGUILayout.TextArea(self.ActionDescription, style);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(self, "Changed Action Description");
                    self.ActionDescription = Value;
                }
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}