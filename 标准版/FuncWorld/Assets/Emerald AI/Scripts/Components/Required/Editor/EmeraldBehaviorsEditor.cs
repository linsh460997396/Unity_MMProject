using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldBehaviors), true)]
    [CanEditMultipleObjects]
    public class EmeraldBehaviorsEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture BehaviorsEditorIcon;
        FieldInfo[] CustomFields;
        SerializedProperty HideSettingsFoldout, BehaviorSettingsFoldout, CurrentBehaviorType, CustomSettingsFoldout, TargetToFollow, CautiousSeconds, ChaseSeconds, 
            FleeSeconds, RequireObstruction, InfititeChase, FleeOnLowHealth, StayNearStartingArea, MaxDistanceFromStartingArea, UpdateFleePositionSeconds, PercentToFlee, FollowingStoppingDistance;

        void OnEnable()
        {
            if (BehaviorsEditorIcon == null) BehaviorsEditorIcon = Resources.Load("Editor Icons/EmeraldBehaviors") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            BehaviorSettingsFoldout = serializedObject.FindProperty("BehaviorSettingsFoldout");
            CustomSettingsFoldout = serializedObject.FindProperty("CustomSettingsFoldout");
            CurrentBehaviorType = serializedObject.FindProperty("CurrentBehaviorType");
            TargetToFollow = serializedObject.FindProperty("TargetToFollow");
            CautiousSeconds = serializedObject.FindProperty("CautiousSeconds");
            ChaseSeconds = serializedObject.FindProperty("ChaseSeconds");
            FleeSeconds = serializedObject.FindProperty("FleeSeconds");
            RequireObstruction = serializedObject.FindProperty("RequireObstruction");
            InfititeChase = serializedObject.FindProperty("InfititeChase");
            FleeOnLowHealth = serializedObject.FindProperty("FleeOnLowHealth");
            StayNearStartingArea = serializedObject.FindProperty("StayNearStartingArea");
            UpdateFleePositionSeconds = serializedObject.FindProperty("UpdateFleePositionSeconds");
            PercentToFlee = serializedObject.FindProperty("PercentToFlee");
            MaxDistanceFromStartingArea = serializedObject.FindProperty("MaxDistanceFromStartingArea");
            FollowingStoppingDistance = serializedObject.FindProperty("FollowingStoppingDistance");

            //Get all variables that are not part of the parent class.
            CustomFields = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldBehaviors self = (EmeraldBehaviors)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Behaviors", BehaviorsEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                BehaviorSettings(self);
                if (self.GetType().ToString() != "EmeraldAI.EmeraldBehaviors")
                {
                    EditorGUILayout.Space();
                    CustomSettings(self);
                }
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void BehaviorSettings (EmeraldBehaviors self)
        {
            BehaviorSettingsFoldout.boolValue = EditorGUILayout.Foldout(BehaviorSettingsFoldout.boolValue, "Behavior Settings", true, FoldoutStyle);

            if (BehaviorSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Behavior Settings", "Choose from 1 of the 3 available base behavior types. Companion and Pet options are avaialble within these options by setting a Target to Follow.", true);
                EditorGUILayout.Space();

                EditorGUILayout.Space();
                CustomEditorProperties.CustomPropertyField(CurrentBehaviorType, "Current Behavior Type", "The behavior this AI will use.", true); //Overridding the method will allow users to create their own version of the specified behavior.

                PassiveSettings(self);
                CowardSettings(self);
                AggressiveSettings(self);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Displays all options that are intended for the Passive Behavior Type.
        /// </summary>
        void PassiveSettings (EmeraldBehaviors self)
        {
            if (self.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Passive)
                return;

            CustomEditorProperties.TextTitleWithDescription("Passive Settings", "Passive AI will not attack or flee from targets. They will wander according to their Wander Type set within the Movement Component.", true);

            CustomEditorProperties.CustomPropertyField(TargetToFollow, "Target to Follow", "Assigning a Target to Follow will turn an AI into a Pet AI (or a non-combat Componanion AI). Note: If a Target to Follow is assigned, they will ignore their Wander Type and follow their follower instead.", true);

            if (self.TargetToFollow)
            {
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), FollowingStoppingDistance, "Following Stopping Distance", 1, 15);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance in which an AI will stop from their Target to Follow.", true);
            }
        }

        /// <summary>
        /// Displays all options that are intended for the Aggressive Behavior Type.
        /// </summary>
        void CowardSettings(EmeraldBehaviors self)
        {
            if (self.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Coward)
                return;

            CustomEditorProperties.TextTitleWithDescription("Coward Settings", "Coward AI will flee from targets who they have an Enemy Relation Type with.", true);

            CustomEditorProperties.CustomIntSliderPropertyField(CautiousSeconds, "Cautious Seconds", "Controls the amount of time an AI will remain in the Cautious State before fleeing from their target. " +
                "If an AI has a warning animation, this will automatically be played while in this state. If this value is set to 0, the cautious state will be ignored.", 0, 15, false);

            if (self.CautiousSeconds > 0)
            {
                CustomEditorProperties.DisplayImportantMessage("Be aware that AI who are in a cautious state will not flee from a detected target until after the duration of their Cautious Seconds (unless they're attacked).");
            }

            EditorGUI.BeginDisabledGroup(self.InfititeChase);
            CustomEditorProperties.CustomIntSliderPropertyField(FleeSeconds, "Flee Seconds", "Controls the amount of time an AI will flee from a target for returning its non-combat state. This happens when the current target is outside of an AI's detection radius.", 1, 60, true);

            CustomEditorProperties.CustomPropertyField(RequireObstruction, "Require Obstruction", "Only allow the flee time to increase if the AI's current target is obstructed. This allows the AI to continuously flee the target while they are visible, " +
                "but give up if the target has been obstructed (or not visible) for the duration of the Flee Seconds.", true);

            EditorGUILayout.Space();
            CustomEditorProperties.CustomFloatSliderPropertyField(UpdateFleePositionSeconds, "Update Flee Position Seconds", "Controls how often the flee position will be updated.", 0.25f, 5f, true);

            EditorGUILayout.Space();
        }

        /// <summary>
        /// Displays all options that are intended for the Aggressive Behavior Type.
        /// </summary>
        void AggressiveSettings (EmeraldBehaviors self)
        {
            if (self.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive)
                return;

            CustomEditorProperties.TextTitleWithDescription("Aggressive Settings", "Aggressive AI will attack targets who they have an Enemy Relation Type with.", true);

            CustomEditorProperties.CustomPropertyField(TargetToFollow, "Target to Follow", "Assigning a Target to Follow will turn an AI into a Companion AI. They will also ignore their Wander Type and follow their the specified instead. Note: AI who have currently have a Target to Follow cannot use any of the settings below.", true);

            if (self.TargetToFollow)
            {
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), FollowingStoppingDistance, "Following Stopping Distance", 1, 15);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance in which an AI will stop from their Target to Follow.", true);
            }

            EditorGUI.BeginDisabledGroup(self.TargetToFollow);
            CustomEditorProperties.CustomIntSliderPropertyField(CautiousSeconds, "Cautious Seconds", "Controls the amount of time an AI will remain in the Cautious State before attacking their target. " +
                "If an AI has a warning animation, this will automatically be played while in this state. If this value is set to 0, the cautious state will be ignored.", 0, 15, false);

            if (self.CautiousSeconds > 0)
            {
                CustomEditorProperties.DisplayImportantMessage("Be aware that AI who are in a cautious state will not attack a detected target until after the duration of their Cautious Seconds (unless they're attacked).");
            }

            EditorGUILayout.Space();
            CustomEditorProperties.CustomPropertyField(InfititeChase, "Infitite Chase", "Controls whether or not the AI will chase their target without any distance or time resrtictions. Note: This will disable the Chase Seconds and Stay Near Starting Area settings.", true);

            EditorGUI.BeginDisabledGroup(self.InfititeChase);
            CustomEditorProperties.CustomIntSliderPropertyField(ChaseSeconds, "Chase Seconds", "Controls the amount of time an AI will chase a target for before giving up and exiting its combat state. This happens when the current target is outside of an AI's detection radius.", 1, 60, true);
            
            CustomEditorProperties.CustomPropertyField(RequireObstruction, "Require Obstruction", "Only allow the chase time to increase if the AI's current target is obstructed. This allows the AI to continuously chase the target while they are visible, " +
                "but give up if the target has been obstructed (or not visible) for the duration of the Chase Seconds.", true);

            CustomEditorProperties.CustomPropertyField(StayNearStartingArea, "Stay Near Starting Area", "Controls whether or not an AI will give up on a target if it gets too far away from its starting area.", true);

            if (self.StayNearStartingArea == YesOrNo.Yes)
            {
                CustomEditorProperties.BeginIndent();
                CustomEditorProperties.CustomIntSliderPropertyField(MaxDistanceFromStartingArea, "Max Distance From Starting Area", "Controls the maximum distance an AI is allowed to be from its starting area before giving up on a target and return to its starting position or area.", 10, 100, true);
                CustomEditorProperties.EndIndent();
            }          
            EditorGUI.EndDisabledGroup();

            CustomEditorProperties.CustomPropertyField(FleeOnLowHealth, "Flee on Low Health", "Controls whether or not an AI will flee upon low health while in combat.", true);

            if (self.FleeOnLowHealth == YesOrNo.Yes)
            {
                CustomEditorProperties.BeginIndent();
                CustomEditorProperties.CustomIntSliderPropertyField(PercentToFlee, "Percent to Flee", "Controls the percentage of low health needed to flee.", 1, 99, true);
                CustomEditorProperties.CustomFloatSliderPropertyField(UpdateFleePositionSeconds, "Update Flee Position Seconds", "Controls how often the flee position will be updated.", 0.25f, 5f, true);
                CustomEditorProperties.EndIndent();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        void OnSceneGUI()
        {
            EmeraldBehaviors self = (EmeraldBehaviors)target;
            DrawStartingAreaDistance(self);
        }

        /// <summary>
        /// Draws the wander area, when using the Dynamic Wander Type.
        /// </summary>
        void DrawStartingAreaDistance(EmeraldBehaviors self)
        {
            if (self.StayNearStartingArea == YesOrNo.Yes && BehaviorSettingsFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                Handles.color = new Color(0, 0.6f, 0, 1f);
                Handles.DrawWireDisc(self.transform.position, Vector3.up, (float)self.MaxDistanceFromStartingArea, 3f);
                Handles.color = Color.white;
            }
        }

        /// <summary>
        /// Displays all custom variables in a separate part of the editor.
        /// </summary>
        void CustomSettings(EmeraldBehaviors self)
        {
            CustomSettingsFoldout.boolValue = EditorGUILayout.Foldout(CustomSettingsFoldout.boolValue, "Custom Settings", true, FoldoutStyle);

            if (CustomSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Custom Settings", "Any variables added through a child class of EmeraldBehavior will be added here.", true);

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
                    else if (field.FieldType.IsClass && field.FieldType.ToString() != "System.String" && !field.FieldType.ToString().Contains("Unity"))
                    {
                        Debug.Log(field.FieldType);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        GUILayout.Space(1);
                        EditorGUILayout.EndHorizontal();
                    }
                    //Don'transform apply an offset to single variables
                    else
                    {
                        if (serializedObject.FindProperty(field.Name) != null)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                        }
                    }

                }
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}