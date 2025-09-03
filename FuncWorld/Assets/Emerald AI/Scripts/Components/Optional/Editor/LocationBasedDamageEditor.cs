using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace EmeraldAI.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(LocationBasedDamage))]
    [CanEditMultipleObjects]
    public class LocationBasedDamageEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture LBDEditorIcon;
        ReorderableList ColliderList;
        string ColliderListState;
        SerializedProperty HideSettingsFoldout, LBDSettingsFoldout, LBDComponentsTag, SetCollidersLayerAndTag;
        List<string> layers = new List<string>();

        private void OnEnable()
        {
            if (LBDEditorIcon == null) LBDEditorIcon = Resources.Load("Editor Icons/EmeraldLBD") as Texture;
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            LBDSettingsFoldout = serializedObject.FindProperty("LBDSettingsFoldout");
            LBDComponentsTag = serializedObject.FindProperty("LBDComponentsTag");
            SetCollidersLayerAndTag = serializedObject.FindProperty("SetCollidersLayerAndTag");
            InitializeList();
            InitializeLayers();
        }

        /// <summary>
        /// Get all the layers in the project so the user can choose one to apply to their LBD components.
        /// This layer will also be added to a static layermask and automatically added to every AI's obstruction detection layermask.
        /// This is needed to stop the LBD components from causing obstructions. Doing it globally allows all AI to receive the change automatically
        /// so users don'transform have to manually track or set layers for every AI in their project. 
        /// </summary>
        void InitializeLayers()
        {
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) != "")
                    layers.Add(LayerMask.LayerToName(i));
                else
                    layers.Add("Empty");
            }
        }

        void InitializeList ()
        {
            //Label Style
            var LabelStyle = new GUIStyle();
            LabelStyle.fontStyle = FontStyle.Bold;
            LabelStyle.active.textColor = Color.white;
            LabelStyle.normal.textColor = Color.white;

            ColliderList = new ReorderableList(serializedObject, serializedObject.FindProperty("ColliderList"), false, true, false, true);
            ColliderList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Collider List", EditorStyles.boldLabel);
            };
            ColliderList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = ColliderList.serializedProperty.GetArrayElementAtIndex(index);
                    ColliderList.elementHeight = EditorGUIUtility.singleLineHeight * 2.5f;

                    //Label
                    if (element.FindPropertyRelative("ColliderObject").objectReferenceValue != null)
                    {
                        EditorGUI.PrefixLabel(new Rect(rect.x + 120, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight),
                            new GUIContent(element.FindPropertyRelative("ColliderObject").objectReferenceValue.name), LabelStyle);

                        //Select Button
                        if (GUI.Button(new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight), "Select Collider"))
                        {
                            Selection.activeObject = element.FindPropertyRelative("ColliderObject").objectReferenceValue;
                        }

                        //Multiplier
                        element.FindPropertyRelative("DamageMultiplier").floatValue = EditorGUI.Slider(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("ColliderObject").objectReferenceValue.name + " Multiplier", element.FindPropertyRelative("DamageMultiplier").floatValue, 0, 25);
                    }
                    else
                    {
                        GUI.color = Color.red;
                        EditorGUI.PrefixLabel(new Rect(rect.x + 120, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), new GUIContent("Null - Please Remove"), LabelStyle);
                        GUILayout.FlexibleSpace();
                        GUI.color = Color.white;

                        GUI.contentColor = Color.red;
                        //Select Button
                        if (GUI.Button(new Rect(rect.x, rect.y, 110, EditorGUIUtility.singleLineHeight), "Remove"))
                        {
                            LocationBasedDamage self = (LocationBasedDamage)target;
                            self.ColliderList.RemoveAt(index);
                        }
                        GUI.contentColor = Color.white;

                        EditorGUI.BeginDisabledGroup(true);
                        element.FindPropertyRelative("DamageMultiplier").floatValue = EditorGUI.Slider(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight), "Null" + " Multiplier", element.FindPropertyRelative("DamageMultiplier").floatValue, 0, 25);
                        EditorGUI.EndDisabledGroup();
                    }
                };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            LocationBasedDamage self = (LocationBasedDamage)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Location Based Damage", LBDEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                LBDSettings(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void LBDSettings (LocationBasedDamage self)
        {
            LBDSettingsFoldout.boolValue = EditorGUILayout.Foldout(LBDSettingsFoldout.boolValue, "Location Based Damage Settings", true, FoldoutStyle);

            if (LBDSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Location Based Damage Settings", "The Location Based Damage component allows each collider to detect damage and apply a customizable damage multiplier based on the damage receieved. The hit effect that will play upon impact, " +
                "and at the position the hit is detected, is based off of the AI's Hit Effects List (Located under AI's Settings>Combat>Hit Effect)", false);

                CustomEditorProperties.TutorialButton("For a tutorial on using the Location Based Damage component, please see the tutorial below. Note: The LBD uses different code to damage an AI.",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/location-based-damage-component");
                EditorGUILayout.Space();

                CustomEditorProperties.CustomPropertyField(SetCollidersLayerAndTag, "Set Colliders Layer and Tag", "Controls whether or not the Location Based Dmaage component will set the detected collider's tag and layer. Users can customize the tag and layer by enabling this setting.", true);

                if (SetCollidersLayerAndTag.boolValue)
                {
                    CustomEditorProperties.BeginIndent();
                    CustomEditorProperties.FactionListEnum(new Rect(), GUIContent.none, serializedObject.FindProperty("LBDComponentsLayer"), "Collider Layer", layers);
                    CustomEditorProperties.CustomHelpLabelField("Sets the layer of the Location Based Damage colliders. This is layer will automatically be added to every AI's Obstruction Detection Layermask so the components will not obstruct their line of sight." +
                        " It is very important that this layer is not your targets' layers and that it is not Default.", true);
                    EditorGUILayout.Space();

                    CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), LBDComponentsTag, "Collider Tag");
                    CustomEditorProperties.CustomHelpLabelField("Sets the tag of the Location Based Damage colliders. It is recommended that the tag is set to something other than Untagged.", true);
                    EditorGUILayout.Space();

                    CustomEditorProperties.FactionListEnum(new Rect(), GUIContent.none, serializedObject.FindProperty("DeadLBDComponentsLayer"), "Dead Collider Layer", layers);
                    CustomEditorProperties.CustomHelpLabelField("Sets the layer of the Location Based Damage colliders when an AI dies.", true);
                    EditorGUILayout.Space();
                    CustomEditorProperties.EndIndent();
                }
                else
                {
                    GUILayout.Space(-15);
                    CustomEditorProperties.DisplayWarningMessage("It is recommended that this setting is kept on, unless you are handling the colliders' layer and tag manually. The layer should not be set to Default. Not properly managing this can result in detection issues");
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Gets all colliders within the AI and applies Location Based Damage Area components to them.", EditorStyles.helpBox);
                if (GUILayout.Button("Get Colliders"))
                {
                    var m_Colliders = self.GetComponentsInChildren<Collider>();

                    foreach (Collider C in m_Colliders)
                    {
                        if (C != null && C.gameObject != self.gameObject)
                        {
                            if (!self.ColliderList.Exists(x => x.ColliderObject == C))
                            {
                                LocationBasedDamage.LocationBasedDamageClass lbdc = new LocationBasedDamage.LocationBasedDamageClass(C, 1);
                                self.ColliderList.Add(lbdc);
                            }
                        }
                    }

                    serializedObject.Update();
                    serializedObject.ApplyModifiedProperties();

                    if (self.ColliderList.Count == 0)
                    {
                        Debug.Log("There are no colliders within this AI. Please ensure that you have setup the AI with Unity's Ragdoll Wizard or a 3rd party ragdoll tool.");
                    }
                }

                if (GUILayout.Button("Clear Colliders") && EditorUtility.DisplayDialog("Clear Collider List?", "Are you sure you want to clear the AI's Collider List? This process cannot be undone.", "Clear", "Do Not Clear"))
                {
                    self.ColliderList.Clear();
                    serializedObject.Update();
                }

                EditorGUILayout.HelpBox("You can remove an undesired collider by selecting the collider within the Collider List and pressing the - button on the bottom of the Collider List area.", MessageType.Info);

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                ColliderList.DoLayoutList();

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}