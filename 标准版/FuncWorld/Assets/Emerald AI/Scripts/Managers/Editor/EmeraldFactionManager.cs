using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    public class EmeraldFactionManager : EditorWindow
    {
        GUIStyle TitleStyle;
        Texture FactionIcon;
        Vector2 scrollPos;
        SerializedObject serializedObject;
        ReorderableList FactionList;
        bool RefreshInspector;

        [MenuItem("Window/Emerald AI/Faction Manager #%spriteRenderer", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager), false, "Faction Manager");
            APS.minSize = new Vector2(300f, 300f);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        protected virtual void OnEnable()
        {
            if (FactionIcon == null) FactionIcon = Resources.Load("FactionExtension") as Texture;
            LoadFactionData();

#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
#endif
        }

        protected virtual void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= OnEditorUpdate;
#endif
        }

        /// <summary>
        /// This is needed (using callback from EditorApplication.update) to reliably update the faction enums within an AI's editor, given factions are being changed while an AI is selected.
        /// </summary>
        protected virtual void OnEditorUpdate()
        {
            if (RefreshInspector)
            {
                UpdateStaticFactionData();
                RefreshInspector = false;
            }
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); //Top Left Side Indent
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "Faction Manager", FactionIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  //Top Right Side Indent
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); //Bottom Left Side Indent
            EditorGUILayout.BeginVertical();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription("Faction Manager", "With the Faction Manager, you can create factions that your AI will use to identify targets. " +
                "Factions created here will be globally available for all Emerald AI agents to use. You can assign factions through an AI's Detection component within the Faction Settings foldout.", true);

            FactionList.DoLayoutList();

            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(15); //Bottom Right Side Indent
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        void UpdateStaticFactionData ()
        {
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));
            EmeraldDetection.StringFactionList = new List<string>(FactionData.FactionNameList);
            FactionExtension.StringFactionList = new List<string>(FactionData.FactionNameList);
            //Repaint();

            EditorUtility.SetDirty(FactionData);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            InternalEditorUtility.RepaintAllViews();
            SceneView.RepaintAll();
            Repaint();
        }

        void LoadFactionData()
        {
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));
            serializedObject = new SerializedObject(FactionData);

            FactionList = new ReorderableList(serializedObject, serializedObject.FindProperty("FactionNameList"), false, true, true, true);
            FactionList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Faction List", EditorStyles.boldLabel);
            };
            FactionList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = FactionList.serializedProperty.GetArrayElementAtIndex(index);
                    FactionList.elementHeight = EditorGUIUtility.singleLineHeight * 1.25f;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.LabelField(new Rect(rect.x, rect.y + 3f, rect.width, EditorGUIUtility.singleLineHeight), "Faction " + (index + 1));
                    EditorGUI.PropertyField(new Rect(rect.x + 75, rect.y + 3f, rect.width - 75, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        RefreshInspector = true;
                        UpdateStaticFactionData();
                    }
                };

            FactionList.onChangedCallback = (FactionList) =>
            {
                UpdateStaticFactionData();
            };
        }
    }
}
