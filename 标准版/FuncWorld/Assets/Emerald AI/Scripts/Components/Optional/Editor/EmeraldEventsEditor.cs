using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldEvents))]
    [CanEditMultipleObjects]
    public class EmeraldEventsEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture EventsEditorIcon;

        //Bools
        SerializedProperty HideSettingsFoldout, GeneralEventsFoldout, CombatEventsFoldout;

        //Events
        SerializedProperty OnDeathEventProp, OnTakeDamageEventProp, OnTakeCritDamageEventProp, OnReachedDestinationEventProp, OnReachedWaypointEventProp, OnGeneratedWaypointEventProp, OnStartEventProp, OnAttackStartEventProp, OnFleeEventProp, OnStartCombatEventProp, OnEndCombatEventProp, 
            OnEnabledEventProp, OnPlayerDetectedEventProp, OnKilledTargetEventProp, OnDoDamageEventProp, OnDoCritDamageEventProp, OnAttackEndEventProp, OnEnemyTargetDetectedEventProp;

        void OnEnable()
        {
            if (EventsEditorIcon == null) EventsEditorIcon = Resources.Load("Editor Icons/EmeraldEvents") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            //Bools
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            GeneralEventsFoldout = serializedObject.FindProperty("GeneralEventsFoldout");
            CombatEventsFoldout = serializedObject.FindProperty("CombatEventsFoldout");

            //Events
            OnDeathEventProp = serializedObject.FindProperty("OnDeathEvent");
            OnTakeDamageEventProp = serializedObject.FindProperty("OnTakeDamageEvent");
            OnTakeCritDamageEventProp = serializedObject.FindProperty("OnTakeCritDamageEvent");
            OnDoDamageEventProp = serializedObject.FindProperty("OnDoDamageEvent");
            OnReachedDestinationEventProp = serializedObject.FindProperty("OnReachedDestinationEvent");
            OnReachedWaypointEventProp = serializedObject.FindProperty("OnReachedWaypointEvent");
            OnGeneratedWaypointEventProp = serializedObject.FindProperty("OnGeneratedWaypointEvent");
            OnStartEventProp = serializedObject.FindProperty("OnStartEvent");
            OnPlayerDetectedEventProp = serializedObject.FindProperty("OnPlayerDetectedEvent");
            OnEnemyTargetDetectedEventProp = serializedObject.FindProperty("OnEnemyTargetDetectedEvent");
            OnEnabledEventProp = serializedObject.FindProperty("OnEnabledEvent");
            OnAttackStartEventProp = serializedObject.FindProperty("OnAttackStartEvent");
            OnAttackEndEventProp = serializedObject.FindProperty("OnAttackEndEvent");
            OnFleeEventProp = serializedObject.FindProperty("OnFleeEvent");
            OnStartCombatEventProp = serializedObject.FindProperty("OnStartCombatEvent");
            OnEndCombatEventProp = serializedObject.FindProperty("OnEndCombatEvent");
            OnKilledTargetEventProp = serializedObject.FindProperty("OnKilledTargetEvent");
            OnDoCritDamageEventProp = serializedObject.FindProperty("OnDoCritDamageEvent");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldEvents self = (EmeraldEvents)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Events", EventsEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                GeneralEvents(self);
                EditorGUILayout.Space();
                CombatEvents(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void GeneralEvents(EmeraldEvents self)
        {
            GeneralEventsFoldout.boolValue = EditorGUILayout.Foldout(GeneralEventsFoldout.boolValue, "General Events", true, FoldoutStyle);

            if (GeneralEventsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("General Events", "Holds all general related events.", true);

                CustomEditorProperties.CustomHelpLabelField("Triggers an event when this AI is enabled. This can be useful for events that need to be called when an AI is being respawned.", false);
                EditorGUILayout.PropertyField(OnEnabledEventProp);

                CustomEditorProperties.CustomHelpLabelField("Triggers an event on Start. This can be useful for initializing custom mechanics and quests as well as spawning animations.", false);
                EditorGUILayout.PropertyField(OnStartEventProp);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when this AI reaches their destination when using the Destination Wander Type.", false);
                EditorGUILayout.PropertyField(OnReachedDestinationEventProp, new GUIContent("On Reached Destination Event"));

                CustomEditorProperties.CustomHelpLabelField("Triggers an event each time this AI arrives at a waypoint (for both Dynamic and Waypoint Wander Types).", false);
                EditorGUILayout.PropertyField(OnReachedWaypointEventProp, new GUIContent("On Reached Waypoint Event"));

                CustomEditorProperties.CustomHelpLabelField("Triggers an event each time this AI generates a waypoint (for both Dynamic and Waypoint Wander Types).", false);
                EditorGUILayout.PropertyField(OnGeneratedWaypointEventProp, new GUIContent("On Generated Waypoint Event"));

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when this AI detects the player when not in combat mode. This can be useful for quests, initializing dialogue, or greetings. " +
                    "This event is dependent on the AI's Detection Radius and is triggered when the player enters it.", false);
                EditorGUILayout.PropertyField(OnPlayerDetectedEventProp);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void CombatEvents(EmeraldEvents self)
        {
            CombatEventsFoldout.boolValue = EditorGUILayout.Foldout(CombatEventsFoldout.boolValue, "Combat Events", true, FoldoutStyle);

            if (CombatEventsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Combat Events", "Holds all combat related events.", true);

                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI first starts combat and will not be called again until the AI re-enters combat.", false);
                EditorGUILayout.PropertyField(OnStartCombatEventProp);

                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI ends combat and there are no detectable enemy targets nearby.", false);
                EditorGUILayout.PropertyField(OnEndCombatEventProp);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event each time the AI successfully detects a target when while in combat.", false);
                EditorGUILayout.PropertyField(OnEnemyTargetDetectedEventProp, new GUIContent("On Detect Target Event"));

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI's attack starts. Note: This event will trigger even if the AI misses its target.", false);
                EditorGUILayout.PropertyField(OnAttackStartEventProp, new GUIContent("On Attack Start Event"));

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI's attack ends. Note: This event will trigger even if the AI misses its target.", false);
                EditorGUILayout.PropertyField(OnAttackEndEventProp);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI is damaged.", false);
                EditorGUILayout.PropertyField(OnTakeDamageEventProp, new GUIContent("On Take Damage Event"));

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI is damaged and takes a critical hit.", false);
                EditorGUILayout.PropertyField(OnTakeCritDamageEventProp, new GUIContent("On Take Crit Damage Event"));

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI successfully deals any kind of damage.", false);
                EditorGUILayout.PropertyField(OnDoDamageEventProp);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI deals damage and it's a critical hit.", false);
                EditorGUILayout.PropertyField(OnDoCritDamageEventProp);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI flees. This can be useful for fleeing sounds or other added functionality.", false);
                EditorGUILayout.PropertyField(OnFleeEventProp);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI kills a target.", false);
                EditorGUILayout.PropertyField(OnKilledTargetEventProp);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("Triggers an event when the AI dies. This can be useful for triggering loot generation, quest mechanics, or other death related events.", false);
                EditorGUILayout.PropertyField(OnDeathEventProp, new GUIContent("On Death Event"));

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}