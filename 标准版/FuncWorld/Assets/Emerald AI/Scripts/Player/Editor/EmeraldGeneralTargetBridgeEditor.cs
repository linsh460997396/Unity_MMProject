using UnityEngine;
using UnityEditor;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldGeneralTargetBridge))]
    [CanEditMultipleObjects]
    public class EmeraldGeneralTargetBridgeEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture HealthEditorIcon;
        SerializedProperty StartHealthProp, ImmortalProp, OnTakeDamageProp, OnDeathProp, HideSettingsFoldout, HealthSettingsFoldout;

        void OnEnable()
        {
            if (HealthEditorIcon == null) HealthEditorIcon = Resources.Load("Editor Icons/EmeraldHealth") as Texture;
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            StartHealthProp = serializedObject.FindProperty("StartingHealth");
            ImmortalProp = serializedObject.FindProperty("Immortal");
            OnTakeDamageProp = serializedObject.FindProperty("OnTakeDamage");
            OnDeathProp = serializedObject.FindProperty("OnDeath");
            HealthSettingsFoldout = serializedObject.FindProperty("HealthSettingsFoldout");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldGeneralTargetBridge self = (EmeraldGeneralTargetBridge)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Target Bridge", HealthEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                HealthSetting(self);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader(); 
        }

        void HealthSetting (EmeraldGeneralTargetBridge self)
        {
            HealthSettingsFoldout.boolValue = EditorGUILayout.Foldout(HealthSettingsFoldout.boolValue, "Health Settings", true, FoldoutStyle);

            if (HealthSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Faction Settings", "Allows AI to identify this gameobject without having to rely on Unity's Tag system. This means all potential targets can share the same Unity Tag and Unity Layer.", true);

                CustomEditorProperties.CustomPropertyField(ImmortalProp, "Immortal", "Controls whether or not an AI is immune to damage and is unkillable.", true);

                EditorGUI.BeginDisabledGroup(self.Immortal);
                CustomEditorProperties.CustomPropertyField(StartHealthProp, "Start Health", "Controls the health this AI will start with.", true);

                CustomEditorProperties.CustomHelpLabelField("Triggers an event when this target is damaged.", false);
                EditorGUILayout.PropertyField(OnTakeDamageProp, new GUIContent("On Take Damage Event"));
                EditorGUILayout.Space();
                //CustomEditorProperties.CustomPropertyField(OnTakeDamageProp, "On Take Damage Event", "Triggers an event when this target is damaged.", true);

                CustomEditorProperties.CustomHelpLabelField("Triggers an event when this target is dies.", false);
                EditorGUILayout.PropertyField(OnDeathProp, new GUIContent("On Death Event"));
                //CustomEditorProperties.CustomPropertyField(OnDeathProp, "On Death Event", "Triggers an event when this target is dies.", true);
                EditorGUI.EndDisabledGroup();

                DrawHealthBar(self);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void DrawHealthBar(EmeraldGeneralTargetBridge self)
        {
            GUILayout.Space(45);
            GUIStyle LabelStyle = new GUIStyle();
            LabelStyle.alignment = TextAnchor.MiddleCenter;
            LabelStyle.padding.bottom = 4;
            LabelStyle.fontStyle = FontStyle.Bold;
            LabelStyle.normal.textColor = Color.white;

            Rect r = EditorGUILayout.BeginVertical();
            GUI.backgroundColor = Color.white;
            float CurrentHealth = ((float)self.Health / (float)self.StartHealth);

            if (!Application.isPlaying)
            {
                self.Health = self.StartHealth;
            }

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
            }

            EditorGUI.DrawRect(new Rect(r.x, r.position.y - 39f, ((r.width)), 32), new Color(0.05f, 0.05f, 0.05f, 0.5f)); //Health Bar BG Outline
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8)), 24), new Color(0.16f, 0.16f, 0.16f, 1f)); //Health Bar BG
            Color HealthBarColor = Color.Lerp(new Color(0.6f, 0.1f, 0.1f, 1f), new Color(0.15f, 0.42f, 0.15f, 1f), CurrentHealth);
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8) * CurrentHealth), 24), HealthBarColor); //Health Bar Main

            if (CurrentHealth > 0)
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "Current Health: " + self.Health + "/" + self.StartHealth, LabelStyle);
            }
            else
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "Current Health: " + 0 + "/" + self.StartHealth + " (Dead)", LabelStyle);
            }

            EditorGUILayout.EndVertical();
        }
    }
}