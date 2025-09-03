using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldDetection))]
    [CanEditMultipleObjects]
    public class EmeraldDetectionEditor : Editor
    {
        #region Variables
        GUIStyle FoldoutStyle;
        EmeraldBehaviors BehaviorsComponent;
        Texture DetectionEditorIcon;

        //Ints
        SerializedProperty FieldOfViewAngleProp, DetectionRadiusProp, CurrentFactionProp;

        //Floats
        SerializedProperty ObstructionDetectionFrequencyProp;

        //Reorderable List
        ReorderableList FactionsList;

        //String
        SerializedProperty PlayerTagProp, RagdollTagProp;

        //Bool
        SerializedProperty HideSettingsFoldout, DetectionFoldout, TagFoldout, FactionFoldout;

        //Float
        SerializedProperty DetectionFrequencyProp, ObstructionSecondsProp;

        //Object
        SerializedProperty HeadTransformProp;

        //LayerMasks
        SerializedProperty DetectionLayerMaskProp, ObstructionDetectionLayerMaskProp;
        #endregion

        void OnEnable()
        {
            EmeraldDetection self = (EmeraldDetection)target;
            BehaviorsComponent = self.GetComponent<EmeraldBehaviors>();
            if (DetectionEditorIcon == null) DetectionEditorIcon = Resources.Load("Editor Icons/EmeraldDetection") as Texture;

            RefreshFactionData();
        }

        void RefreshFactionData ()
        {
            LoadFactionData();
            InitializeProperties();
            InitializeFactionList();
        }

        void MissingComponentsMessage (EmeraldDetection self)
        {
            if (!self.HeadTransform)
            {
                CustomEditorProperties.DisplaySetupWarning("The AI's Head Transform has not been applied and is needed for accurate raycast calculations, please apply it. This is located within the Detection Settings foldout.");
            }
            else if (self.FactionRelationsList.Count == 0 && BehaviorsComponent.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
            {
                CustomEditorProperties.DisplaySetupWarning("This AI needs at least 1 Faction Relation to function properly. Please apply one through the Faction Settings foldout below.");
            }
        }

        void InitializeProperties()
        {
            //Ints
            FieldOfViewAngleProp = serializedObject.FindProperty("FieldOfViewAngle");
            DetectionRadiusProp = serializedObject.FindProperty("DetectionRadius");
            CurrentFactionProp = serializedObject.FindProperty("CurrentFaction");

            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            DetectionFoldout = serializedObject.FindProperty("DetectionFoldout");
            TagFoldout = serializedObject.FindProperty("TagFoldout");
            FactionFoldout = serializedObject.FindProperty("FactionFoldout");

            //String
            PlayerTagProp = serializedObject.FindProperty("PlayerTag");
            RagdollTagProp = serializedObject.FindProperty("RagdollTag");

            //Float
            DetectionFrequencyProp = serializedObject.FindProperty("DetectionFrequency");
            ObstructionDetectionFrequencyProp = serializedObject.FindProperty("ObstructionDetectionFrequency");
            ObstructionSecondsProp = serializedObject.FindProperty("ObstructionSeconds");

            //Object
            HeadTransformProp = serializedObject.FindProperty("HeadTransform");

            //LayerMasks
            DetectionLayerMaskProp = serializedObject.FindProperty("DetectionLayerMask");
            ObstructionDetectionLayerMaskProp = serializedObject.FindProperty("ObstructionDetectionLayerMask");
        }

        void InitializeFactionList()
        {
            //Factions List
            FactionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("FactionRelationsList"), true, true, true, true);
            FactionsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = FactionsList.serializedProperty.GetArrayElementAtIndex(index);
                    FactionsList.elementHeight = EditorGUIUtility.singleLineHeight * 3.75f;

                    if (element.FindPropertyRelative("RelationType").intValue == 0)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - 16, rect.y + 2f, rect.width + 17, EditorGUIUtility.singleLineHeight * 3.5f), new Color(1.0f, 0.0f, 0.0f, 0.15f));
                    }
                    else if (element.FindPropertyRelative("RelationType").intValue == 1)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - 16, rect.y + 2f, rect.width + 17, EditorGUIUtility.singleLineHeight * 3.5f), new Color(0.1f, 0.1f, 0.1f, 0.1f));
                    }
                    else if (element.FindPropertyRelative("RelationType").intValue == 2)
                    {
                        EditorGUI.DrawRect(new Rect(rect.x - 16, rect.y + 2f, rect.width + 17, EditorGUIUtility.singleLineHeight * 3.5f), new Color(0.0f, 1.0f, 0.0f, 0.15f));
                    }
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + 35, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("RelationType"), new GUIContent("Relation Type", "The type of relation this AI has with this faction."));

                    CustomEditorProperties.CustomListPopup(new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight), new GUIContent(), element.FindPropertyRelative("FactionIndex"), "Faction", EmeraldDetection.StringFactionList.ToArray());

                    EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight),
                        new GUIContent("Faction", "Factions are based on all factions within the Faction Manager. An AI can have as many faction relations as needed."));
                };

            FactionsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "AI Faction Relations", EditorStyles.boldLabel);
            };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldDetection self = (EmeraldDetection)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Detection", DetectionEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingComponentsMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DetectionSettings(self);
                EditorGUILayout.Space();
                TagSettings(self);
                EditorGUILayout.Space();
                FactionSettings(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void DetectionSettings(EmeraldDetection self)
        {
            DetectionFoldout.boolValue = EditorGUILayout.Foldout(DetectionFoldout.boolValue, "Detection Settings", true, FoldoutStyle);

            if (DetectionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Detection Settings", "Controls various detection settings such as radius distances, target detection, and field of view.", true);


                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), FieldOfViewAngleProp, "Field of View", 1, 360);
                CustomEditorProperties.CustomHelpLabelField("Controls the field of view an AI uses to detect targets.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), DetectionRadiusProp, "Detection Distance", 1, 100);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance of the field of view as well as the AI's detection radius.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DetectionFrequencyProp, "Detection Frequency", 0.1f, 2f);
                CustomEditorProperties.CustomHelpLabelField("Controls how often the AI's detection calculations update.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), ObstructionSecondsProp, "Obstruction Seconds", 0.5f, 5f);
                CustomEditorProperties.CustomHelpLabelField("Controls how many seconds must pass, while obstructed, before an AI will switch to a new target.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), ObstructionDetectionFrequencyProp, "Obstruction Detection Frequency", 0.05f, 1f);
                CustomEditorProperties.CustomHelpLabelField("Controls how often the AI checks for obstructions between them and their current target.", false);

                CustomEditorProperties.BeginIndent();
                EditorGUILayout.PropertyField(ObstructionDetectionLayerMaskProp, new GUIContent("Obstruction Ignore Layers"));
                CustomEditorProperties.CustomHelpLabelField("The layers that should be ignored when an AI is using its obstruction detection for attacking." +
                    "These are objects that may prevent an AI from seeing its target. If your target has nothing that will block the AI's sight, you can " +
                    "set the layermask to Nothing.", true);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(HeadTransformProp, new GUIContent("Head Transform"));
                CustomEditorProperties.CustomHelpLabelField("The head transform of your AI. This is used for accurate head looking and raycast calculations related to sight and obstruction detection. " +
                    "This should be your AI's head object within its bone objects.", false);

                CustomEditorProperties.AutoFindHeadTransform(new Rect(), new GUIContent(), HeadTransformProp, self.transform);
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void TagSettings(EmeraldDetection self)
        {
            TagFoldout.boolValue = EditorGUILayout.Foldout(TagFoldout.boolValue, "Tag & Layer Settings", true, FoldoutStyle);

            if (TagFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Tag & Layer Settings", "Controls an AI's Detection Layers. These are used to allow the AI to know what Layers are detectable targets.", true);

                CustomEditorProperties.TutorialButton("For a tutorial on setting up an AI's Detection Layers and Player Tag, please see the tutorial below.",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component/setting-up-the-detection-layers-and-player-tag");

                CustomEditorProperties.NoticeTextTitleWithDescription("Important", "The Player Relation is handled through the AI's Faction Relations List (within the Faction Settings foldout below). The Player Unity Tag is used to determine certain internal functionality.", false);
                CustomEditorProperties.CustomTagField(new Rect(), new GUIContent(), PlayerTagProp, "Player Unity Tag");
                CustomEditorProperties.CustomHelpLabelField("The Unity Tag used to define Player objects. This is the tag that was assigned using Unity's Tag pulldown at the top of " +
                    "the gameobject.", true);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(DetectionLayerMaskProp, new GUIContent("Detection Layers"));
                CustomEditorProperties.CustomHelpLabelField("The Detection Layers controls what layers this AI can detect as possible targets.", false);

                if (DetectionLayerMaskProp.intValue == 0 || DetectionLayerMaskProp.intValue == 1)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("The Detection Layers cannot contain Nothing, Default, or Everything.", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void FactionSettings(EmeraldDetection self)
        {
            FactionFoldout.boolValue = EditorGUILayout.Foldout(FactionFoldout.boolValue, "Faction Settings", true, FoldoutStyle);

            if (FactionFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Faction Settings", "The Faction Settings allow you to control which Factions your AI " +
                "sees as enemies or allies, including the relations with the AI and the player.", true);

                CustomEditorProperties.TutorialButton("For a tutorial on setting up an AI's faction relations, please see the tutorial below.",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-required/detection-component/faction-relations#setting-up-an-ais-faction-relations");

                EditorGUILayout.Space();

                CustomEditorProperties.CustomEnum(new Rect(), new GUIContent(), CurrentFactionProp, "Faction");
                CustomEditorProperties.CustomHelpLabelField("An AI's Faction is the name used to control combat reaction with other AI. This is the name other AI will use when " +
                    "looking for opposing targets.", true);

                CustomEditorProperties.CustomHelpLabelField("Factions can be created and removed using the Faction Manager. ", false);
                if (GUILayout.Button("Open Faction Manager"))
                {
                    EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager));
                    APS.minSize = new Vector2(600f, 775f);
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("AI Faction Relations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls which factions this AI sees as enemies and allies. You can hover the mouse over each setting to view its tooltip.", false);
                GUI.backgroundColor = new Color(1f, 1, 0.25f, 0.25f);
                EditorGUILayout.LabelField("Note: The AI Faction Relations use an AI's Faction not Unity tags. You can add and remove factions through the Faction Manager. " +
                    "This can be opened by pressing the button below.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space();
                if (GUILayout.Button("Open Faction Manager"))
                {
                    EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldFactionManager));
                    APS.minSize = new Vector2(600f, 775f);
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                FactionsList.DoLayoutList();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        private void OnSceneGUI()
        {
            EmeraldDetection self = (EmeraldDetection)target;
            DrawDetectionSettings(self);
        }

        public Vector3 DirFromAngle(Transform transform, float angleInDegrees, bool angleIsGlobal, EmeraldDetection self)
        {
            if (!angleIsGlobal)
                angleInDegrees += transform.eulerAngles.y;
            return transform.rotation * Quaternion.Euler(new Vector3(0, -transform.eulerAngles.y, 0)) * new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        void DrawDetectionSettings (EmeraldDetection self)
        {
            if (DetectionFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                //Red areas not covered by the line of sight, but the areas in yellow are.
                Handles.color = Color.red;
                Handles.DrawWireArc(self.transform.position, self.transform.up, self.transform.forward, (float)self.FieldOfViewAngle / 2f, self.DetectionRadius, 3f);
                Handles.DrawWireArc(self.transform.position, self.transform.up, self.transform.forward, -(float)self.FieldOfViewAngle / 2f, self.DetectionRadius, 3f);

                Handles.color = Color.yellow;
                Handles.DrawWireArc(self.transform.position, self.transform.up, -self.transform.forward, (360 - self.FieldOfViewAngle) / 2f, self.DetectionRadius, 3f);
                Handles.DrawWireArc(self.transform.position, self.transform.up, -self.transform.forward, -(360 - self.FieldOfViewAngle) / 2f, self.DetectionRadius, 3f);

                Vector3 viewAngleA = DirFromAngle(self.transform, -self.FieldOfViewAngle / 2f, false, self);
                Vector3 viewAngleB = DirFromAngle(self.transform, self.FieldOfViewAngle / 2f, false, self);

                Handles.color = Color.red;
                if (self.FieldOfViewAngle < 360)
                {
                    Handles.DrawLine(self.transform.position, self.transform.position + viewAngleA * self.DetectionRadius, 3f);
                    Handles.DrawLine(self.transform.position, self.transform.position + viewAngleB * self.DetectionRadius, 3f);
                }
                Handles.color = Color.white;
            }
        }

        void LoadFactionData()
        {
            EmeraldDetection.StringFactionList.Clear();
            string path = AssetDatabase.GetAssetPath(Resources.Load("Faction Data"));
            EmeraldFactionData FactionData = (EmeraldFactionData)AssetDatabase.LoadAssetAtPath(path, typeof(EmeraldFactionData));

            if (FactionData != null)
            {
                foreach (string s in FactionData.FactionNameList)
                {
                    if (!EmeraldDetection.StringFactionList.Contains(s) && s != "")
                    {
                        EmeraldDetection.StringFactionList.Add(s);
                    }
                }
            }
        }

        void CustomTag(Rect position, GUIContent label, SerializedProperty property)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.TagField(position, property.stringValue);

            if (EditorGUI.EndChangeCheck())
                property.stringValue = newValue;

            EditorGUI.EndProperty();
        }
    }
}