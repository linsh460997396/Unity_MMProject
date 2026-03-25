using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldHealth))]
    [CanEditMultipleObjects]
    public class EmeraldHealthEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture HealthEditorIcon;

        //Int
        SerializedProperty StartingHealthProp, HealRateProp;

        //Enum
        SerializedProperty UseHitEffectProp;

        //Bool
        SerializedProperty HideSettingsFoldout, HealthFoldout, HitEffectFoldout, ImmortalProp, AttachHitEffectsProp;

        //Float
        SerializedProperty HitEffectTimeoutSecondsProp;

        //Vector
        SerializedProperty HitEffectPosOffsetProp;

        ReorderableList HitEffectsList;

        void OnEnable()
        {
            if (HealthEditorIcon == null) HealthEditorIcon = Resources.Load("Editor Icons/EmeraldHealth") as Texture;
            InitializeProperties();
            InitializeList();
        }

        void InitializeProperties()
        {
            //Ints
            StartingHealthProp = serializedObject.FindProperty("StartingHealth");
            HealRateProp = serializedObject.FindProperty("HealRate");

            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            HealthFoldout = serializedObject.FindProperty("HealthFoldout");
            HitEffectFoldout = serializedObject.FindProperty("HitEffectFoldout");
            ImmortalProp = serializedObject.FindProperty("Immortal");
            AttachHitEffectsProp = serializedObject.FindProperty("AttachHitEffects");

            //Float
            HitEffectTimeoutSecondsProp = serializedObject.FindProperty("HitEffectTimeoutSeconds");

            //Vector
            HitEffectPosOffsetProp = serializedObject.FindProperty("HitEffectPosOffset");

            //Enum
            UseHitEffectProp = serializedObject.FindProperty("UseHitEffect");
        }

        void InitializeList ()
        {
            //Hit Effects List
            HitEffectsList = new ReorderableList(serializedObject, serializedObject.FindProperty("HitEffectsList"), true, true, true, true);
            HitEffectsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Hit Effects List", EditorStyles.boldLabel);
            };
            HitEffectsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = HitEffectsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldHealth self = (EmeraldHealth)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Health", HealthEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                HealthSettings(self);
                EditorGUILayout.Space();
                HitEffectSettings(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void HealthSettings(EmeraldHealth self)
        {
            HealthFoldout.boolValue = EditorGUILayout.Foldout(HealthFoldout.boolValue, "Health Settings", true, FoldoutStyle);

            if (HealthFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Health Settings", "Controls various health related settings.", true);

                EditorGUILayout.PropertyField(ImmortalProp, new GUIContent("Immortal"));
                CustomEditorProperties.CustomHelpLabelField("Controls whether or not an AI is immune to damage and is unkillable. If this is enabled, it will disable other settings.", true);

                EditorGUI.BeginDisabledGroup(self.Immortal);
                CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), StartingHealthProp, "Starting Health");
                CustomEditorProperties.CustomHelpLabelField("Controls how much starting health an AI will have.", true);

                CustomEditorProperties.CustomPropertyField(HealRateProp, "Heal Rate", "Controls how much an AI will heal per second when not actively in combat, given their health is below its max.", true);
                EditorGUI.EndDisabledGroup();

                DrawHealthBar(self);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void DrawHealthBar (EmeraldHealth self)
        {
            GUILayout.Space(45);
            GUIStyle LabelStyle = new GUIStyle();
            LabelStyle.alignment = TextAnchor.MiddleCenter;
            LabelStyle.padding.bottom = 4;
            LabelStyle.fontStyle = FontStyle.Bold;
            LabelStyle.normal.textColor = Color.white;

            Rect r = EditorGUILayout.BeginVertical();
            GUI.backgroundColor = Color.white;
            float CurrentHealth = ((float)self.CurrentHealth / (float)self.StartingHealth);

            EditorGUI.DrawRect(new Rect(r.x, r.position.y - 39f, ((r.width)), 32), new Color(0.05f, 0.05f, 0.05f, 0.5f)); //Health Bar BG Outline
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8)), 24), new Color(0.16f, 0.16f, 0.16f, 1f)); //Health Bar BG
            Color HealthBarColor = Color.Lerp(new Color(0.6f, 0.1f, 0.1f, 1f), new Color(0.15f, 0.42f, 0.15f, 1f), CurrentHealth);
            EditorGUI.DrawRect(new Rect(r.x + 4, r.position.y - 35f, ((r.width - 8) * CurrentHealth), 24), HealthBarColor); //Health Bar Main

            if (self.CurrentHealth > 0)
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "Current Health: " + self.CurrentHealth + "/" + self.StartingHealth, LabelStyle);
            }
            else
            {
                EditorGUI.LabelField(new Rect(r.x, r.position.y - 35f, (r.width), 26), "Current Health: " + self.CurrentHealth + "/" + self.StartingHealth + " (Dead)", LabelStyle);
            }

            EditorGUILayout.EndVertical();

            if (!Application.isPlaying)
            {
                self.CurrentHealth = self.StartingHealth;
            }
        }

        void HitEffectSettings(EmeraldHealth self)
        {
            HitEffectFoldout.boolValue = EditorGUILayout.Foldout(HitEffectFoldout.boolValue, "Hit Effect Settings", true, FoldoutStyle);

            if (HitEffectFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Hit Effect Settings", "Allows an AI to display a random hit affect after receiving damage.", true);

                EditorGUILayout.PropertyField(UseHitEffectProp, new GUIContent("Use Hit Effect"));
                CustomEditorProperties.CustomHelpLabelField("Controls whether or not this AI will use a hit effect when it receives melee damage.", true);

                if (self.UseHitEffect == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    CustomEditorProperties.CustomPropertyField(AttachHitEffectsProp, "Attach Hit Effects", "Controls whether or not the Hit Effects will be attached to the AI when it's damaged.", true);
                    EditorGUILayout.Space();

                    CustomEditorProperties.CustomHelpLabelField("The hit effect that will appear when this AI receives damage.", true);
                    HitEffectsList.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HitEffectTimeoutSecondsProp, new GUIContent("Hit Effect Timeout Seconds"));
                    CustomEditorProperties.CustomHelpLabelField("Controls how long the hit effect will be visible before being deactivated.", true);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(HitEffectPosOffsetProp, new GUIContent("Hit Effect Position Offset"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the offset position of the hit effect using the AI's Hit Transform position.", true);
                    EditorGUILayout.Space();

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}