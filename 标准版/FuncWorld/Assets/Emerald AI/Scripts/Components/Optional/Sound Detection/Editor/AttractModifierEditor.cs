using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using EmeraldAI.Utility;

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(AttractModifier))]
    public class AttractModifierEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture AttractModifierEditorIcon;
        SerializedProperty PlayerFactionProp, RadiusProp, MinVelocityProp, ReactionCooldownSecondsProp, SoundCooldownSecondsProp, EmeraldAILayerProp, TriggerTypeProp, AttractReactionProp, TriggerLayersProp, EnemyRelationsOnlyProp, HideSettingsFoldout, AttractModifierFoldout;
        ReorderableList TriggerSoundsList;
        EmeraldFactionData FactionData;

        private void OnEnable()
        {
            if (AttractModifierEditorIcon == null) AttractModifierEditorIcon = Resources.Load("AttractModifier") as Texture;
            RadiusProp = serializedObject.FindProperty("Radius");
            PlayerFactionProp = serializedObject.FindProperty("PlayerFaction.FactionIndex");
            MinVelocityProp = serializedObject.FindProperty("MinVelocity");
            ReactionCooldownSecondsProp = serializedObject.FindProperty("ReactionCooldownSeconds");
            SoundCooldownSecondsProp = serializedObject.FindProperty("SoundCooldownSeconds");
            EmeraldAILayerProp = serializedObject.FindProperty("EmeraldAILayer");
            TriggerTypeProp = serializedObject.FindProperty("TriggerType");
            AttractReactionProp = serializedObject.FindProperty("AttractReaction");
            TriggerLayersProp = serializedObject.FindProperty("TriggerLayers");
            EnemyRelationsOnlyProp = serializedObject.FindProperty("EnemyRelationsOnly");
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            AttractModifierFoldout = serializedObject.FindProperty("AttractModifierFoldout");
            FactionData = Resources.Load("Faction Data") as EmeraldFactionData;

            //Trigger Sounds
            TriggerSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("TriggerSounds"), true, true, true, true);
            TriggerSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Trigger Sounds List", EditorStyles.boldLabel);
            };
            TriggerSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = TriggerSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        public override void OnInspectorGUI()
        {
            AttractModifier self = (AttractModifier)target;
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Attract Modifier", AttractModifierEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                AttractModifierSettings();
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void AttractModifierSettings()
        {
            AttractModifierFoldout.boolValue = EditorGUILayout.Foldout(AttractModifierFoldout.boolValue, "Attract Modifier Settings", true, FoldoutStyle);

            if (AttractModifierFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Attract Modifier Settings", "This system will attract all AI that are within range and invoke the 'Attract Reaction'. The object the Attract Modifier is attached to " +
                    "will be the source of attraction. This system is intended to extend the functionality of the Sound Detection component by allowing certain objects, collisions, and custom calls to attract nearby AI.", true);

                CustomEditorProperties.TutorialButton("For a tutorial on using the Attract Modifier, please see the tutorial below.", "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/sound-detector-component/using-an-attract-modifier");

                CustomEditorProperties.CustomPropertyField(EmeraldAILayerProp, "Emerald AI Layer", "The Emerald AI layers used by your AI (Only objects with this layer, and that are Emerald AI agents with a Sound Detection component, will be detected).", true);
                CustomEditorProperties.CustomPropertyField(AttractReactionProp, "Attract Reaction", "The Reaction Object that will be called when this modifier is invoked/triggered " +
                    "(Reaction Objects can be created by right clicking in the project tab and going to Create>Emerald AI>Create>Reaction Object).", true);
                CustomEditorProperties.CustomPropertyField(EnemyRelationsOnlyProp, "Enemy Relations Only", "Controls whether or not this Attract Modifier will only be received by " +
                    "AI with a Player Relation of Enemy. If set to false, all AI within range will receive this Attract Modifier if it's triggered.", false);

                if (EnemyRelationsOnlyProp.boolValue)
                {
                    CustomEditorProperties.BeginIndent();
                    PlayerFactionProp.intValue = EditorGUILayout.Popup("Player Faction", PlayerFactionProp.intValue, FactionData.FactionNameList.ToArray());
                    EditorGUILayout.LabelField("The faction your player uses.", EditorStyles.helpBox);
                    CustomEditorProperties.EndIndent();
                }

                GUILayout.Space(10);

                CustomEditorProperties.CustomPropertyField(RadiusProp, "Radius", "Controls the range of affect for this Attract Modifier. AI within this range will receive the Reaction Object when this Attract Modifier is triggered.", true);
                CustomEditorProperties.CustomPropertyField(ReactionCooldownSecondsProp, "Reaction Cooldown Seconds", "The amount of time (in seconds) until the Attract Reaction can be invoked again.", true);
                CustomEditorProperties.CustomPropertyField(SoundCooldownSecondsProp, "Sound Cooldown Seconds", "The amount of time (in seconds) until the trigger sound can be played again.", true);

                if ((TriggerTypes)TriggerTypeProp.intValue == TriggerTypes.OnCollision)
                {
                    CustomEditorProperties.CustomPropertyField(MinVelocityProp, "Min Velocity", "The minimum velocity required to invoke the attached Attract Reaction (usable only with Collision Trigger Type).", true);
                }

                GUILayout.Space(10);

                CustomEditorProperties.CustomPropertyField(TriggerTypeProp, "Trigger Type", "Controls the how the Attract Modifier will be invoked.", false);

                if (TriggerTypeProp.intValue == (int)TriggerTypes.OnStart)
                {
                    EditorGUILayout.LabelField("OnStart - Invokes the Reaction Object on Start and uses this gameobject as the attraction source.", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnTrigger)
                {
                    EditorGUILayout.LabelField("OnTrigger - Invokes the Reaction Object when a trigger collision happens with this object. This gameobject as the attraction source.", EditorStyles.helpBox);
                    TriggerLayerMaskDrawer();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCollision)
                {
                    EditorGUILayout.LabelField("OnCollision - Invokes the Reaction Object when a non-trigger collision happens with this object. This gameobject as the attraction source.", EditorStyles.helpBox);
                    TriggerLayerMaskDrawer();
                }
                else if (TriggerTypeProp.intValue == (int)TriggerTypes.OnCustomCall)
                {
                    EditorGUILayout.LabelField("OnCustomCall - Invokes the Reaction Object when the ActivateAttraction function, located within the AttractModifier script, is called. This gameobject as the attraction source.", EditorStyles.helpBox);
                    EditorGUILayout.Space();
                }

                GUILayout.Space(5);

                EditorGUILayout.LabelField("A random sound from the Trigger Sounds list will be played when the Trigger Type condition is met.", EditorStyles.helpBox);
                TriggerSoundsList.DoLayoutList();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void OnSceneGUI()
        {
            AttractModifier self = (AttractModifier)target;
            Handles.color = new Color(1f, 0f, 0, 1f);
            Handles.DrawWireDisc(self.transform.position, self.transform.up, (float)self.Radius, 3);
        }

        void TriggerLayerMaskDrawer ()
        {
            CustomEditorProperties.BeginIndent();
            CustomEditorProperties.CustomPropertyField(TriggerLayersProp, "Trigger Layers", "Controls which collision layers are allowed to trigger this Attract Modifier.", true);

            if (TriggerLayersProp.intValue == 0)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("The Trigger Layers LayerMask cannot be set to Nothing", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            CustomEditorProperties.EndIndent();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
    }
}