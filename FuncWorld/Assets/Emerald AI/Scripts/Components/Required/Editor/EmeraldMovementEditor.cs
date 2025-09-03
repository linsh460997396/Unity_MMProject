using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldMovement))]
    [CanEditMultipleObjects]
    public class EmeraldMovementEditor : Editor
    {
        GUIStyle FoldoutStyle;
        EmeraldAnimation EmeraldAnimation;
        Texture MovementEditorIcon;
        int CurrentWaypointIndex = -1;

        #region SerializedProperties
        //Foldouts
        SerializedProperty HideSettingsFoldout, WanderFoldout, WaypointsFoldout, WaypointsListFoldout, MovementFoldout, AlignmentFoldout, TurnFoldout;

        //Int
        SerializedProperty StationaryIdleSecondsMinProp, StationaryIdleSecondsMaxProp, WanderRadiusProp, MaxSlopeLimitProp, WalkSpeedProp, RunSpeedProp, MinimumWaitTimeProp, MaximumWaitTimeProp,
            WalkBackwardsSpeedProp, StationaryTurningSpeedCombatProp, MovingTurningSpeedCombatProp, BackupTurningSpeedProp;

        //Floats
        SerializedProperty StoppingDistanceProp, NonCombatAngleToTurnProp, CombatAngleToTurnProp, StationaryTurningSpeedNonCombatProp, MovingTurnSpeedNonCombatProp, MovementTurningSensitivityProp, MaxNormalAngleProp, NonCombatAlignSpeedProp,
            CombatAlignSpeedProp, ForceWalkDistanceProp, DecelerationDampTimeProp;

        //Enums
        SerializedProperty WanderTypeProp, WaypointTypeProp, AlignAIWithGroundProp, CurrentMovementStateProp, AnimatorTypeProp, AlignmentQualityProp, AlignAIOnStartProp;

        //LayerMask
        SerializedProperty DynamicWanderLayerMaskProp, BackupLayerMaskProp, AlignmentLayerMaskProp;

        //Bool
        SerializedProperty UseRandomRotationOnStartProp, AnimationsUpdatedProp;

        //Objects
        SerializedProperty WaypointObjectProp;
        #endregion

        void OnEnable()
        {
            EmeraldMovement self = (EmeraldMovement)target;
            EmeraldAnimation = self.GetComponent<EmeraldAnimation>();
            if (MovementEditorIcon == null) MovementEditorIcon = Resources.Load("Editor Icons/EmeraldMovement") as Texture;

            InitializeProperties();
        }

        void InitializeProperties ()
        {
            //Enums
            WanderTypeProp = serializedObject.FindProperty("WanderType");
            WaypointTypeProp = serializedObject.FindProperty("WaypointType");
            AlignAIWithGroundProp = serializedObject.FindProperty("AlignAIWithGround");
            CurrentMovementStateProp = serializedObject.FindProperty("CurrentMovementState");
            AnimatorTypeProp = serializedObject.FindProperty("MovementType");
            AlignmentQualityProp = serializedObject.FindProperty("AlignmentQuality");
            AlignAIOnStartProp = serializedObject.FindProperty("AlignAIOnStart");

            //Ints
            StationaryIdleSecondsMinProp = serializedObject.FindProperty("StationaryIdleSecondsMin");
            StationaryIdleSecondsMaxProp = serializedObject.FindProperty("StationaryIdleSecondsMax");
            WanderRadiusProp = serializedObject.FindProperty("WanderRadius");
            MaxSlopeLimitProp = serializedObject.FindProperty("MaxSlopeLimit");
            WanderRadiusProp = serializedObject.FindProperty("WanderRadius");
            MinimumWaitTimeProp = serializedObject.FindProperty("MinimumWaitTime");
            MaximumWaitTimeProp = serializedObject.FindProperty("MaximumWaitTime");
            WalkSpeedProp = serializedObject.FindProperty("WalkSpeed");
            WalkBackwardsSpeedProp = serializedObject.FindProperty("WalkBackwardsSpeed");
            RunSpeedProp = serializedObject.FindProperty("RunSpeed");
            BackupTurningSpeedProp = serializedObject.FindProperty("BackupTurningSpeed");

            CombatAngleToTurnProp = serializedObject.FindProperty("CombatAngleToTurn");
            NonCombatAngleToTurnProp = serializedObject.FindProperty("NonCombatAngleToTurn");
            StationaryTurningSpeedNonCombatProp = serializedObject.FindProperty("StationaryTurningSpeedNonCombat");
            StationaryTurningSpeedCombatProp = serializedObject.FindProperty("StationaryTurningSpeedCombat");           
            MovingTurnSpeedNonCombatProp = serializedObject.FindProperty("MovingTurnSpeedNonCombat");
            MovingTurningSpeedCombatProp = serializedObject.FindProperty("MovingTurnSpeedCombat");

            //Floats
            StoppingDistanceProp = serializedObject.FindProperty("StoppingDistance");       
            MovementTurningSensitivityProp = serializedObject.FindProperty("MovementTurningSensitivity");
            DecelerationDampTimeProp = serializedObject.FindProperty("DecelerationDampTime");
            MaxNormalAngleProp = serializedObject.FindProperty("MaxNormalAngle");
            NonCombatAlignSpeedProp = serializedObject.FindProperty("NonCombatAlignmentSpeed");
            CombatAlignSpeedProp = serializedObject.FindProperty("CombatAlignmentSpeed");
            ForceWalkDistanceProp = serializedObject.FindProperty("ForceWalkDistance");

            //LayerMask
            DynamicWanderLayerMaskProp = serializedObject.FindProperty("DynamicWanderLayerMask");
            BackupLayerMaskProp = serializedObject.FindProperty("BackupLayerMask");
            AlignmentLayerMaskProp = serializedObject.FindProperty("AlignmentLayerMask");

            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            WanderFoldout = serializedObject.FindProperty("WanderFoldout");
            WaypointsFoldout = serializedObject.FindProperty("WaypointsFoldout");
            WaypointsListFoldout = serializedObject.FindProperty("WaypointsListFoldout");
            MovementFoldout = serializedObject.FindProperty("MovementFoldout");
            AlignmentFoldout = serializedObject.FindProperty("AlignmentFoldout");
            TurnFoldout = serializedObject.FindProperty("TurnFoldout");
            UseRandomRotationOnStartProp = serializedObject.FindProperty("UseRandomRotationOnStart");
            AnimationsUpdatedProp = serializedObject.FindProperty("AnimationsUpdated"); //Note: Used by multiple scripts currently, ensure this doesn'transform cause issues.

            //Objects
            WaypointObjectProp = serializedObject.FindProperty("m_WaypointObject");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldMovement self = (EmeraldMovement)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Movement", MovementEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                MovementSettings(self);
                EditorGUILayout.Space();
                TurnSettings(self);
                EditorGUILayout.Space();
                AlignmentSettings(self);
                EditorGUILayout.Space();
                WanderSettings(self);
                EditorGUILayout.Space();
                WaypointSettings(self);
                if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints) EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Holds all waypoint related settings and displays them through the EmeraldAIMovementEditor.
        /// </summary>
        void WaypointSettings (EmeraldMovement self)
        {
            if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints)
            {
                WaypointsFoldout.boolValue = EditorGUILayout.Foldout(WaypointsFoldout.boolValue, "Waypoint Settings", true, FoldoutStyle);

                if (WaypointsFoldout.boolValue)
                {
                    CustomEditorProperties.BeginFoldoutWindowBox();

                    CustomEditorProperties.TextTitleWithDescription("Waypoint Editor", "Below you can define waypoints for your AI to follow. Simply press the 'Add Waypoint' button to create a waypoint. The AI will follow each created waypoint in the order " +
                    "they are created. A line will be drawn to visually represent this.", true);

                    if (self.WaypointsList != null && Selection.objects.Length == 1)
                    {
                        EditorGUILayout.LabelField("Controls what an AI will do when it reaches its last waypoint.", EditorStyles.helpBox);
                        EditorGUILayout.PropertyField(WaypointTypeProp, new GUIContent("Waypoint Type"));
                        GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                        GUI.backgroundColor = Color.white;

                        if (self.WaypointType == (EmeraldMovement.WaypointTypes.Loop))
                        {
                            CustomEditorProperties.CustomHelpLabelField("Loop - Allows an AI to continiously move to each waypoint, in order, without ever stopping. When an AI reaches its last waypoint, it will set the first waypoint as its next waypoint thus creating a loop.", false);
                        }
                        else if (self.WaypointType == (EmeraldMovement.WaypointTypes.Reverse))
                        {
                            CustomEditorProperties.CustomHelpLabelField("Reverse - Allows an AI to continiously move to each waypoint, in order, without stopping until it reaches its last waypoint. When this happens, it will idle " +
                                "for the length of its Wait Time seconds then reverse the AI's waypoints making the last waypoint its first and repeat this process.", false);
                        }
                        else if (self.WaypointType == (EmeraldMovement.WaypointTypes.Random))
                        {
                            CustomEditorProperties.CustomHelpLabelField("Random - Allows an AI to patrol randomly through all waypoints. An AI will stop and idle each time it reaches a waypoint for as long as its Wait Time seconds are set.", false);
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        CustomEditorProperties.CustomHelpLabelField("Imports all waypoints from the current Waypoint Object.", false);
                        EditorGUILayout.PropertyField(WaypointObjectProp);

                        if (GUILayout.Button("Import Waypoint Data") && EditorUtility.DisplayDialog("Import Waypoint Data?", "Are you sure you want to clear all of this AI's waypoints and import waypoints from the applied Waypoint Object? This process cannot be undone.", "Yes", "Cancel"))
                        {
                            if (self.m_WaypointObject == null)
                            {
                                Debug.LogError("There's no Waypoint Object applied. Please apply one to import waypoint data.");
                                return;
                            }

                            self.WaypointsList = new List<Vector3>(self.m_WaypointObject.Waypoints);
                            EditorUtility.SetDirty(self);
                        }
                        EditorGUILayout.Space();
                        CustomEditorProperties.CustomHelpLabelField("Exports all waypoints to a Waypoint Object to be imported and shared with other AI so waypoints don'transform have to be recreated manually.", false);
                        if (GUILayout.Button("Export Waypoint Data"))
                        {
                            //Export all of the AI's current waypoints to a Waypoint Object so it can be imported to other AI.
                            string SavePath = EditorUtility.SaveFilePanelInProject("Save Waypoint Data", "New Waypoint Object", "asset", "Please enter a file name to save the file to");
                            if (SavePath != string.Empty)
                            {
                                var m_WaypointObject = CreateInstance<EmeraldWaypointObject>();
                                m_WaypointObject.Waypoints = new List<Vector3>(self.WaypointsList);
                                AssetDatabase.CreateAsset(m_WaypointObject, SavePath);
                            }

                            //For some reason, EditorUtility.SaveFilePanelInProject throws an incorrect EditorGUILayout error when it's used with some custom properties. This fixes it... 
                            CustomEditorProperties.BeginScriptHeader("", null);
                            CustomEditorProperties.BeginFoldoutWindowBox();
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        if (GUILayout.Button("Add Waypoint"))
                        {
                            Vector3 newPoint = new Vector3(0, 0, 0);

                            if (self.WaypointsList.Count == 0)
                            {
                                newPoint = self.transform.position + Vector3.forward * (self.StoppingDistance * 2);
                            }
                            else if (self.WaypointsList.Count > 0)
                            {
                                newPoint = self.WaypointsList[self.WaypointsList.Count - 1] + Vector3.forward * (self.StoppingDistance * 2);
                            }

                            Undo.RecordObject(self, "Add Waypoint");
                            self.WaypointsList.Add(newPoint);
                            EditorUtility.SetDirty(self);
                        }

                        var style = new GUIStyle(GUI.skin.button);
                        style.normal.textColor = Color.red;

                        if (GUILayout.Button("Clear All Waypoints", style) && EditorUtility.DisplayDialog("Clear Waypoints?", "Are you sure you want to clear all of this AI's waypoints? This process cannot be undone.", "Yes", "Cancel"))
                        {
                            self.WaypointsList.Clear();
                            EditorUtility.SetDirty(self);
                        }
                        GUI.contentColor = Color.white;
                        GUI.backgroundColor = Color.white;


                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        CustomEditorProperties.BeginIndent();
                        WaypointsListFoldout.boolValue = CustomEditorProperties.Foldout(WaypointsListFoldout.boolValue, "Waypoints List", true, FoldoutStyle);

                        if (WaypointsListFoldout.boolValue)
                        {
                            CustomEditorProperties.BeginFoldoutWindowBox();
                            CustomEditorProperties.TextTitleWithDescription("Waypoints List", "All of this AI's current waypoints. Waypoints can be individually removed by pressing the ''Remove Point'' button.", true);
                            EditorGUILayout.Space();

                            if (self.WaypointsList.Count > 0)
                            {
                                for (int j = 0; j < self.WaypointsList.Count; ++j)
                                {
                                    GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                                    EditorGUILayout.LabelField("Waypoint " + (j + 1), EditorStyles.toolbarButton);
                                    GUI.backgroundColor = Color.white;

                                    EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                                    if (j < self.WaypointsList.Count - 1)
                                    {
                                        if (GUILayout.Button(new GUIContent("Insert", "Inserts a point between this point and the next point."), EditorStyles.miniButton, GUILayout.Height(18)))
                                        {
                                            Undo.RecordObject(self, "Insert Waypoint Above this Point");
                                            self.WaypointsList.Insert(j + 1, (self.WaypointsList[j] + self.WaypointsList[j + 1]) / 2f);
                                            CurrentWaypointIndex = j + 1;
                                            EditorUtility.SetDirty(self);
                                            HandleUtility.Repaint();
                                        }
                                    }

                                    if (GUILayout.Button(new GUIContent("Remove", "Remove this point from the waypoint list."), EditorStyles.miniButton, GUILayout.Height(18)))
                                    {
                                        Undo.RecordObject(self, "Remove Point");
                                        self.WaypointsList.RemoveAt(j);
                                        EditorUtility.SetDirty(self);
                                        HandleUtility.Repaint();
                                    }
                                    EditorGUILayout.EndHorizontal();



                                    GUILayout.Space(10);
                                }
                            }
                            CustomEditorProperties.EndFoldoutWindowBox();
                        }

                        CustomEditorProperties.EndIndent();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                    else if (self.WaypointsList != null && Selection.objects.Length > 1)
                    {
                        CustomEditorProperties.DisplayWarningMessage("Waypoints do not support multi-object editing. If you'd like to edit an AI's waypoints, please only have 1 AI selected at a time.");
                    }

                    CustomEditorProperties.EndFoldoutWindowBox();
                }
            }
        }

        /// <summary>
        /// Holds all movement related settings and displays them through the EmeraldAIMovementEditor.
        /// </summary>
        void MovementSettings (EmeraldMovement self)
        {
            MovementFoldout.boolValue = EditorGUILayout.Foldout(MovementFoldout.boolValue, "Movement Settings", true, FoldoutStyle);

            if (MovementFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Movement Settings", "Controls all speed and distance related settings.", true);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(AnimatorTypeProp, new GUIContent("Movement Type"));
                CustomEditorProperties.CustomHelpLabelField("Controls how an AI is moved. This is either driven by the Root Motion animation or by the NavMesh component.", true);
                if (EditorGUI.EndChangeCheck())
                {
                    if (EmeraldAnimation.m_AnimationProfile.AnimatorControllerGenerated)
                    {
                        //Assign this directly as AnimatorTypeProp becomes desynced with self.AnimatorType when regenerating the Animator for this setting.
                        self.MovementType = (EmeraldMovement.MovementTypes)AnimatorTypeProp.intValue;
                        EmeraldAnimatorGenerator.GenerateAnimatorController(EmeraldAnimation.m_AnimationProfile);
                    }
                }

                //Movement Type
                CustomEditorProperties.BeginIndent();

                GUI.backgroundColor = new Color(5f, 0.5f, 0.5f, 1f);
                EditorGUILayout.LabelField("When using the Root Motion Movement Type, an AI's Movement Speed is controlled by its animation speed. You can adjust this through an AI's Animation Profile.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUI.BeginDisabledGroup(self.MovementType == EmeraldMovement.MovementTypes.RootMotion);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WalkSpeedProp, "Walk Speed", 0.5f, 5);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI walks.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RunSpeedProp, "Run Speed", 0.5f, 10);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI runs.", true);

                CustomFloatAnimationField(new Rect(), new GUIContent(), WalkBackwardsSpeedProp, "Walk Backwards Speed", 0.5f, 3f);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI walks backwards.", true);

                //Update the Animator as this is required when updating NavMesh speed settings.
                if (EmeraldAnimation.m_AnimationProfile != null && EmeraldAnimation.m_AnimationProfile.AnimatorControllerGenerated && self.MovementType == EmeraldMovement.MovementTypes.NavMeshDriven)
                {
                    if (EmeraldAnimation.m_AnimationProfile.AnimationsUpdated || EmeraldAnimation.m_AnimationProfile.AnimationListsChanged)
                    {
                        EmeraldAnimatorGenerator.GenerateAnimatorController(EmeraldAnimation.m_AnimationProfile);
                    }
                }

                EditorGUI.EndDisabledGroup();
                CustomEditorProperties.EndIndent();
                //Movement Type

                EditorGUILayout.PropertyField(CurrentMovementStateProp, new GUIContent("Movement Animation"));
                CustomEditorProperties.CustomHelpLabelField("Controls the type of animation your AI will use when using waypoints, moving to its destination, or wandering. " +
                    "Note: If needed, this can be changed programmatically during runtime.", true);
                EditorGUILayout.Space();

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), ForceWalkDistanceProp, "Force Walk Distance", 0.0f, 8.0f);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance in which an AI will start walking instead of running as it approaches its target or destination. This can be set to 0 if you would like this feature disabled.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), StoppingDistanceProp, "Stopping Distance", 0.25f, 40);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance in which an AI will stop before waypoints and non-combat related destinations.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DecelerationDampTimeProp, "Deceleration Damp Time", 0.1f, 0.4f);
                CustomEditorProperties.CustomHelpLabelField("Controls the damp time of an AI's animations when decelerating. Lower values mean faster blending of animations between movement and stopping.", true);

                EditorGUILayout.PropertyField(BackupLayerMaskProp, new GUIContent("Backup Layers"));
                CustomEditorProperties.CustomHelpLabelField("Controls which layers will affect the AI's backing up process. Colliders detected within a few units behind the AI will stop the backing up process.", true);

                if (BackupLayerMaskProp.intValue == 0)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("The Backup LayerMask cannot contain Nothing.", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void TurnSettings (EmeraldMovement self)
        {
            TurnFoldout.boolValue = EditorGUILayout.Foldout(TurnFoldout.boolValue, "Turn Settings", true, FoldoutStyle);

            if (TurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Turn Settings", "Controls all settings and speeds related to turning.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), NonCombatAngleToTurnProp, "Turning Angle", 15, 90);
                CustomEditorProperties.CustomHelpLabelField("Controls the angle needed to play a turn animation while an AI is not in combat. Emerald can automatically detect whether an AI is " +
                    "turing left or right. Note: You can use a walking animation in place of a turning animation if your AI doesn'transform one.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatAngleToTurnProp, "Combat Turning Angle", 20, 90);
                CustomEditorProperties.CustomHelpLabelField("Controls the angle needed to play a turn animation while an AI is in combat. Emerald can automatically detect whether an AI is " +
                    "turing left or right. Note: You can use a walking animation in place of a turning animation if your AI doesn'transform one.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), StationaryTurningSpeedNonCombatProp, "Stationary Turn Speed", 1, 200);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI turns while not in combat and is stationary. Note: Lower speeds are meant for the Root Motion setting" +
                    " where the turning animations help assist an AI's turning. If you find an AI not turning quick enough while wandering, even with Root Motion enabled, you will most likely need to increasing this setting.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), StationaryTurningSpeedCombatProp, "Stationary Combat Turn Speed", 1, 200);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI turns while in combat and is stationary.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MovingTurnSpeedNonCombatProp, "Moving Turn Speed", 50, 750);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI turns while not in combat and is moving. Note: Lower speeds are meant for the Root Motion setting" +
                    " where the turning animations help assist an AI's turning. If you find an AI not turning quick enough while wandering, even with Root Motion enabled, you will most likely need to increasing this setting.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MovingTurningSpeedCombatProp, "Moving Combat Turn Speed", 50, 750);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI turns while in combat and is moving.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), BackupTurningSpeedProp, "Backup Turn Speed", 5, 750);
                CustomEditorProperties.CustomHelpLabelField("Controls how fast your AI turns while backing up.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), MovementTurningSensitivityProp, "Movement Turning Sensitivity", 0.5f, 3f);
                CustomEditorProperties.CustomHelpLabelField("Controls how sensitive the movement blend trees are when playing movement turning animations. This is especially noticeable for quadruped models with turning animations.", true);

                EditorGUILayout.PropertyField(UseRandomRotationOnStartProp, new GUIContent("Random Roation on Start"));
                CustomEditorProperties.CustomHelpLabelField("Controls whether or not AI will be randomly rotated on Start.", true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void AlignmentSettings (EmeraldMovement self)
        {
            AlignmentFoldout.boolValue = EditorGUILayout.Foldout(AlignmentFoldout.boolValue, "Alignment Settings", true, FoldoutStyle);

            if (AlignmentFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Alignment Settings", "Allows AI to align themselves to slopes and surfaces (Disable if you are using a full body IK system like Final IK)", true);

                EditorGUILayout.PropertyField(AlignAIWithGroundProp, new GUIContent("Align AI"));
                CustomEditorProperties.CustomHelpLabelField("Aligns the AI to the angle of the terrain and other objects for added realism. Disable this feature for improved performance per AI.", true);

                if (self.AlignAIWithGround == YesOrNo.Yes)
                {
                    CustomEditorProperties.BeginIndent();

                    EditorGUILayout.PropertyField(AlignmentLayerMaskProp, new GUIContent("Alignment Layers"));
                    CustomEditorProperties.CustomHelpLabelField("The layers the AI will use for aligning itself with the angles of surfaces. Any layers not included above will be ignred.", true);

                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(AlignmentQualityProp, new GUIContent("Align Quality"));
                    CustomEditorProperties.CustomHelpLabelField("Controls the quality of the Align AI feature by controlling how often it's updated.", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), NonCombatAlignSpeedProp, "Non-Combat Align Speed", 5, 200);
                    CustomEditorProperties.CustomHelpLabelField("Controls the speed in which the AI is aligned with the ground while not in combat.", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatAlignSpeedProp, "Combat Align Speed", 5, 200);
                    CustomEditorProperties.CustomHelpLabelField("Controls the speed in which the AI is aligned with the ground while in combat.", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxNormalAngleProp, "Max Angle", 5, 50);
                    CustomEditorProperties.CustomHelpLabelField("Controls the maximum angle for an AI to rotate to when aligning with the ground.", true);

                    EditorGUILayout.PropertyField(AlignAIOnStartProp, new GUIContent("Align on Start"));
                    CustomEditorProperties.CustomHelpLabelField("Calculates the Align AI feature on Start.", true);

                    CustomEditorProperties.EndIndent();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Holds all wander type settings and displays them through the EmeraldAIMovementEditor.
        /// </summary>
        void WanderSettings (EmeraldMovement self)
        {
            WanderFoldout.boolValue = EditorGUILayout.Foldout(WanderFoldout.boolValue, "Wander Type Settings", true, FoldoutStyle);

            if (WanderFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Wander Type Settings", "Controls how an AI wanders when not in combat. Using the Waypoints Wander Type will make the waypoint editor visible.", true);
                EditorGUILayout.LabelField("Controls the type of wandering mechanics this AI will use. While wandering, AI will react to targets according to their Behavior Type, given they are visible and within their field of view.", EditorStyles.helpBox);
                EditorGUILayout.PropertyField(WanderTypeProp, new GUIContent("Wander Type"));

                CustomEditorProperties.BeginIndent();
                if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic)
                {
                    CustomEditorProperties.CustomHelpLabelField("Dynamic - Allows an AI to randomly wander by dynamically generate waypoints around their Wander Radius.", true);
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints)
                {
                    CustomEditorProperties.CustomHelpLabelField("Waypoints - Allows you to define waypoints that the AI will move between. Note: The Waypoint Settings can be found in the foldout below this foldout.", true);
                    if (GUILayout.Button("Open Waypoint Settings"))
                    {
                        self.WanderFoldout = false;
                        self.WaypointsFoldout = true;
                    }
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Stationary)
                {
                    CustomEditorProperties.CustomHelpLabelField("Stationary - Allows an AI to stay stationary in the same position and will not move unless a target enters their trigger radius.", true);

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), StationaryIdleSecondsMinProp, "Min Idle Animation Seconds");
                    CustomEditorProperties.CustomHelpLabelField("When using more than 1 idle animation, this controls the minimum amount of seconds needed before switching to the next idle " +
                        "animation. This will be randomized with the Max Idle Animation Seconds.", true);

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), StationaryIdleSecondsMaxProp, "Max Idle Animation Seconds");
                    CustomEditorProperties.CustomHelpLabelField("When using more than 1 idle animation, this controls the maximum amount of seconds needed before switching to the next idle " +
                        "animation. This will be randomized with the Min Idle Animation Seconds.", true);
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Destination)
                {
                    CustomEditorProperties.CustomHelpLabelField("Destination - Allows an AI to travel to a single destination relying on Unity's NavMesh Pathfinding to get there. Once it reaches the destination, it will stay stationary.", true);

                    if (GUILayout.Button("Reset Destination Point"))
                    {
                        self.SingleDestination = self.transform.position + self.transform.forward * 2;
                    }
                }
                else if (self.WanderType == EmeraldMovement.WanderTypes.Custom)
                {
                    CustomEditorProperties.CustomHelpLabelField("Custom - Allows an AI to travel to a destination set through code, which relies on Unity's NavMesh Pathfinding to get there. Once it reaches the destination, it will stay stationary.", false);
                }
                CustomEditorProperties.EndIndent();

                EditorGUILayout.Space();

                if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic)
                {
                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), WanderRadiusProp, "Dynamic Wander Radius", ((int)self.StoppingDistance + 3), 300);
                    CustomEditorProperties.CustomHelpLabelField("Controls the radius that the AI uses to wander. The AI will randomly pick waypoints within this radius.", true);

                    CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxSlopeLimitProp, "Max Slope Limit", 10, 60);
                    CustomEditorProperties.CustomHelpLabelField("Controls the maximum slope that a waypoint can be generated on.", true);

                    EditorGUILayout.PropertyField(DynamicWanderLayerMaskProp, new GUIContent("Dynamic Wander Layers"));
                    CustomEditorProperties.CustomHelpLabelField("Controls what layers will be used when generating Dynamic Waypoints.", false);

                    if (DynamicWanderLayerMaskProp.intValue == 0)
                    {
                        GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                        EditorGUILayout.LabelField("The Dynamic Wander LayerMask cannot contain Nothing.", EditorStyles.helpBox);
                        GUI.backgroundColor = Color.white;
                    }
                }

                EditorGUILayout.Space();

                if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic || self.WanderType == EmeraldMovement.WanderTypes.Waypoints)
                {
                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), MinimumWaitTimeProp, "Min Wait Time");
                    CustomEditorProperties.CustomHelpLabelField("Controls the minimum amount of seconds before generating a new waypoint, when using the Dynamic and Random waypoint Wander Type. This amount is " +
                        "randomized with the Maximim Wait Time.", true);

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), MaximumWaitTimeProp, "Max Wait Time");
                    CustomEditorProperties.CustomHelpLabelField("Controls the maximum amount of seconds before generating a new waypoint, when using the Dynamic and Random waypoint Wander Type. This amount " +
                        "is randomized with the Minimum Wait Time.", true);
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }

            if (self.WanderType == EmeraldMovement.WanderTypes.Destination)
            {
                if (self.SingleDestination == Vector3.zero)
                {
                    self.SingleDestination = new Vector3(self.transform.position.x, self.transform.position.y, self.transform.position.z + 5);
                }
            }
        }

        /// <summary>
        /// Used for drawing all of the Emerald AI Movement Settings.
        /// </summary>
        void OnSceneGUI()
        {
            EmeraldMovement self = (EmeraldMovement)target;
            DrawWaypoints(self);
            DrawWanderArea(self);
            DrawSingleDestinationPoint(self);
        }

        /// <summary>
        /// Draw the user created waypoints, when using the Waypoint Wander Type.
        /// </summary>
        void DrawWaypoints (EmeraldMovement self)
        {
            if (Event.current != null && Event.current.isKey && Event.current.type.Equals(EventType.KeyDown) && Event.current.keyCode == KeyCode.Delete)
            {
                Event.current.Use();

                if (CurrentWaypointIndex != -1)
                {
                    Undo.RecordObject(self, "Deleted Waypoint");
                    self.WaypointsList.RemoveAt(CurrentWaypointIndex);
                }
            }

            if (self.WanderType == EmeraldMovement.WanderTypes.Waypoints && WaypointsFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                if (self.WaypointsList.Count > 0 && self.WaypointsList != null)
                {
                    Handles.color = Color.blue;
                    Handles.DrawLine(self.transform.position, self.WaypointsList[0]);
                    Handles.color = Color.white;

                    Handles.color = Color.green;
                    if (self.WaypointType != (EmeraldMovement.WaypointTypes.Random))
                    {
                        for (int i = 0; i < self.WaypointsList.Count - 1; i++)
                        {
                            Handles.DrawLine(self.WaypointsList[i], self.WaypointsList[i + 1]);
                        }
                    }
                    else if (self.WaypointType == (EmeraldMovement.WaypointTypes.Random))
                    {
                        for (int i = 0; i < self.WaypointsList.Count; i++)
                        {
                            for (int j = (i + 1); j < self.WaypointsList.Count; j++)
                            {
                                Handles.DrawLine(self.WaypointsList[i], self.WaypointsList[j]);
                            }
                        }
                    }
                    Handles.color = Color.white;

                    Handles.color = Color.green;
                    if (self.WaypointType == (EmeraldMovement.WaypointTypes.Loop))
                    {
                        Handles.DrawLine(self.WaypointsList[0], self.WaypointsList[self.WaypointsList.Count - 1]);
                    }

                    //Track last grabbed waypoint. If delete button is pressed (using EventType) delete point (will need undo and redo)
                    Handles.color = new Color(0, 1, 0, 0.25f);
                    for (int i = 0; i < self.WaypointsList.Count; i++)
                    {
                        if (CurrentWaypointIndex != i)
                            Handles.color = new Color(1, 1, 1, 0.05f);
                        else
                            Handles.color = new Color(1, 1, 0, 0.05f);
                        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                            Handles.DrawSolidDisc(self.WaypointsList[i], Vector3.up, self.StoppingDistance);
                        Handles.color = new Color(0, 0, 0, 0.5f);
                        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                        Handles.DrawSolidDisc(self.WaypointsList[i], Vector3.up, 0.25f);
                    }

                    //Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                    for (int i = 0; i < self.WaypointsList.Count; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector3 Pos = Handles.PositionHandle(self.WaypointsList[i], Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(self, "Changed Waypoint Position");
                            self.WaypointsList[i] = Pos;
                            CurrentWaypointIndex = i;
                        }

                        Handles.color = Color.white;
                        CustomEditorProperties.DrawString("Waypoint " + (i + 1), self.WaypointsList[i] + Vector3.up, Color.white);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the wander area, when using the Dynamic Wander Type.
        /// </summary>
        void DrawWanderArea (EmeraldMovement self)
        {
            if (self.WanderType == EmeraldMovement.WanderTypes.Dynamic && WanderFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                Handles.color = new Color(0, 0.6f, 0, 1f);
                Handles.DrawWireDisc(self.transform.position, Vector3.up, (float)self.WanderRadius, 3f);
                Handles.color = Color.white;
            }
        }

        /// <summary>
        /// Draws the destination point, when using the Destination Wander Type.
        /// </summary>
        void DrawSingleDestinationPoint (EmeraldMovement self)
        {
            if (self.WanderType == EmeraldMovement.WanderTypes.Destination && self.SingleDestination != Vector3.zero && WanderFoldout.boolValue && !HideSettingsFoldout.boolValue)
            {
                Handles.color = Color.green;
                Handles.DrawLine(self.transform.position, self.SingleDestination);
                Handles.color = Color.white;

                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.SphereHandleCap(0, self.SingleDestination, Quaternion.identity, 0.5f, EventType.Repaint);
                CustomEditorProperties.DrawString("Destination Point", self.SingleDestination + Vector3.up, Color.white);

                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                self.SingleDestination = Handles.PositionHandle(self.SingleDestination, Quaternion.identity);

                EditorGUI.BeginChangeCheck();
                Vector3 Pos = Handles.PositionHandle(self.SingleDestination, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(self, "Changed Destination Position");
                    self.SingleDestination = Pos;
                }

#if UNITY_EDITOR
                EditorUtility.SetDirty(self);
#endif
            }
        }

        void CustomFloatAnimationField(Rect position, GUIContent label, SerializedProperty property, string Name, float Min, float Max)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUILayout.Slider(Name, property.floatValue, Min, Max);

            if (newValue != property.floatValue)
            {
                AnimationsUpdatedProp.boolValue = true;
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = newValue;
            }

            EditorGUI.EndProperty();
        }
    }
}