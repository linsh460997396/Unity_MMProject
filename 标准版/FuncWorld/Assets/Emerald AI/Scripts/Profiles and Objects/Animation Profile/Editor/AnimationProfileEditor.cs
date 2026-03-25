using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(AnimationProfile))]
    [CanEditMultipleObjects]
    public class AnimationProfileEditor : Editor
    {
        #region SerializedProperties
        GUIStyle FoldoutStyle;
        GUIStyle HelpButtonStyle;
        Texture AnimationProfileEditorIcon;

        SerializedProperty AnimatorCullingModeProp, AIAnimatorProp;

        //Bool
        SerializedProperty AnimationListsChangedProp, AnimationsUpdatedProp, WalkFoldout, RunFoldout, TurnFoldout, Type1CombatWalkFoldout, Type1CombatRunFoldout, Type1CombatTurnFoldout, EmotesFoldout,
            Type2CombatWalkFoldout, Type2CombatRunFoldout, Type2CombatTurnFoldout, Type1StrafeFoldout, Type2StrafeFoldout, Type1DodgeFoldout, Type2DodgeFoldout;
        SerializedProperty Type1CombatAnimationsFoldout, Type2CombatAnimationsFoldout, Type1EquipsFoldout, Type2EquipsFoldout, Type1AttacksFoldout, Type2AttacksFoldout, Type1IdleFoldout, Type2IdleFoldout, NonCombatAnimationsFoldout, NonCombatIdleFoldout, NonCombatDeathFoldout,
           AnimatorSettingsFoldout, NonCombatHitFoldout, Type1HitFoldout, Type2HitFoldout, Type1BlockFoldout, Type2BlockFoldout, Type1DeathFoldout, Type2DeathFoldout;

        //NonCombat
        ReorderableList NonCombatHitAnimationList, NonCombatIdleAnimationList, EmoteAnimationList, NonCombatDeathAnimationList;

        //Type 1
        ReorderableList Type1CombatHitAnimationList, Type1AttackAnimationList, Type1DeathAnimationList;

        //Type 2
        ReorderableList Type2CombatHitAnimationList, Type2AttackAnimationList, Type2DeathAnimationList;

        SerializedProperty Type1HitConditionsProp, Type2HitConditionsProp, Type1HitAnimationCooldownProp, Type2HitAnimationCooldownProp;
        #endregion

        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoCallback;
            if (AnimationProfileEditorIcon == null) AnimationProfileEditorIcon = Resources.Load("Editor Icons/EmeraldAnimation") as Texture;
            InitializeProperties();
            InitializeAnimationLists();

            AnimationProfile self = (AnimationProfile)target;

            if (self.AIAnimator != null)
            {
                self.FilePath = AssetDatabase.GetAssetPath(self.AIAnimator);
            }

            //Fail-safe for if a user deletes an Animator Controller that belongs to an Animation Profile.
            if (self.AnimatorControllerGenerated && self.AIAnimator == null)
            {
                Debug.LogError("It looks like the '" + self.name + "' Animation Profile has lost its Animator Controller. This is likely due to it being mistakenly deleted. All animations have been retained. Please regenerate an Animator Controller.");
                self.AnimatorControllerGenerated = false;
            }
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoCallback;

        }

        /// <summary>
        /// A callback for registering undos. When this happens, update the Animation Profile so the changes made can be undone.
        /// </summary>
        void UndoCallback()
        {
        #if UNITY_EDITOR
            AnimationProfile self = (AnimationProfile)target;
            EmeraldAnimatorGenerator.GenerateAnimatorController(self);
        #endif
        }

        void InitializeProperties()
        {
            AnimationListsChangedProp = serializedObject.FindProperty("AnimationListsChanged");
            AnimationsUpdatedProp = serializedObject.FindProperty("AnimationsUpdated");

            WalkFoldout = serializedObject.FindProperty("WalkFoldout");
            RunFoldout = serializedObject.FindProperty("RunFoldout");
            TurnFoldout = serializedObject.FindProperty("TurnFoldout");
            NonCombatDeathFoldout = serializedObject.FindProperty("NonCombatDeathFoldout");
            NonCombatAnimationsFoldout = serializedObject.FindProperty("NonCombatAnimationsFoldout");
            NonCombatIdleFoldout = serializedObject.FindProperty("NonCombatIdleFoldout");

            Type1CombatWalkFoldout = serializedObject.FindProperty("Type1CombatWalkFoldout");
            Type1CombatRunFoldout = serializedObject.FindProperty("Type1CombatRunFoldout");
            Type1CombatTurnFoldout = serializedObject.FindProperty("Type1CombatTurnFoldout");
            Type2CombatWalkFoldout = serializedObject.FindProperty("Type2CombatWalkFoldout");
            Type2CombatRunFoldout = serializedObject.FindProperty("Type2CombatRunFoldout");
            Type2CombatTurnFoldout = serializedObject.FindProperty("Type2CombatTurnFoldout");
            Type1StrafeFoldout = serializedObject.FindProperty("Type1StrafeFoldout");
            Type2StrafeFoldout = serializedObject.FindProperty("Type2StrafeFoldout");
            Type1DodgeFoldout = serializedObject.FindProperty("Type1DodgeFoldout");
            Type2DodgeFoldout = serializedObject.FindProperty("Type2DodgeFoldout");
            EmotesFoldout = serializedObject.FindProperty("EmotesFoldout");
            AnimatorSettingsFoldout = serializedObject.FindProperty("AnimatorSettingsFoldout");
            NonCombatHitFoldout = serializedObject.FindProperty("NonCombatHitFoldout");
            Type1HitFoldout = serializedObject.FindProperty("Type1HitFoldout");
            Type2HitFoldout = serializedObject.FindProperty("Type2HitFoldout");
            Type1BlockFoldout = serializedObject.FindProperty("Type1BlockFoldout");
            Type2BlockFoldout = serializedObject.FindProperty("Type2BlockFoldout");
            Type1DeathFoldout = serializedObject.FindProperty("Type1DeathFoldout");
            Type2DeathFoldout = serializedObject.FindProperty("Type2DeathFoldout");
            Type1CombatAnimationsFoldout = serializedObject.FindProperty("Type1CombatAnimationsFoldout");
            Type2CombatAnimationsFoldout = serializedObject.FindProperty("Type2CombatAnimationsFoldout");
            Type1EquipsFoldout = serializedObject.FindProperty("Type1EquipsFoldout");
            Type2EquipsFoldout = serializedObject.FindProperty("Type2EquipsFoldout");
            Type1AttacksFoldout = serializedObject.FindProperty("Type1AttacksFoldout");
            Type2AttacksFoldout = serializedObject.FindProperty("Type2AttacksFoldout");
            Type1IdleFoldout = serializedObject.FindProperty("Type1IdleFoldout");
            Type2IdleFoldout = serializedObject.FindProperty("Type2IdleFoldout");

            Type1HitConditionsProp = serializedObject.FindProperty("Type1HitConditions");
            Type2HitConditionsProp = serializedObject.FindProperty("Type2HitConditions");
            Type1HitAnimationCooldownProp = serializedObject.FindProperty("Type1HitAnimationCooldown");
            Type2HitAnimationCooldownProp = serializedObject.FindProperty("Type2HitAnimationCooldown");

            AnimatorCullingModeProp = serializedObject.FindProperty("AnimatorCullingMode");
            AIAnimatorProp = serializedObject.FindProperty("AIAnimator");
        }

        void InitializeAnimationLists()
        {
            //NonCombat Hit Animations
            NonCombatHitAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("NonCombatAnimations.HitList"), true, true, true, true);
            DrawAnimationList(NonCombatHitAnimationList);
            NonCombatHitAnimationList.onChangedCallback = (HitAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //NonCombat Idle
            NonCombatIdleAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("NonCombatAnimations.IdleList"), true, true, true, true);
            DrawAnimationList(NonCombatIdleAnimationList);
            NonCombatIdleAnimationList.onChangedCallback = (IdleAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //NonCombat Idle
            NonCombatDeathAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("NonCombatAnimations.DeathList"), true, true, true, true);
            DrawAnimationList(NonCombatDeathAnimationList);
            NonCombatDeathAnimationList.onChangedCallback = (DeathAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //Combat Hit Animations
            Type1CombatHitAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Animations.HitList"), true, true, true, true);
            DrawAnimationList(Type1CombatHitAnimationList);
            Type1CombatHitAnimationList.onChangedCallback = (Type1CombatHitAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //Type 2 Combat Hit Animations
            Type2CombatHitAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Animations.HitList"), true, true, true, true);
            DrawAnimationList(Type2CombatHitAnimationList);
            Type2CombatHitAnimationList.onChangedCallback = (Type2CombatHitAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //Type 1 Attack Animations
            Type1AttackAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Animations.AttackList"), true, true, true, true);
            DrawAnimationList(Type1AttackAnimationList);
            Type1AttackAnimationList.onChangedCallback = (Type1AttackAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //Type 2 Attack Animations
            Type2AttackAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Animations.AttackList"), true, true, true, true);
            DrawAnimationList(Type2AttackAnimationList);
            Type2AttackAnimationList.onChangedCallback = (RangedAttackAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //Type 1 Animations
            Type1DeathAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Animations.DeathList"), true, true, true, true);
            DrawAnimationList(Type1DeathAnimationList);
            Type1DeathAnimationList.onChangedCallback = (Type1DeathAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //Type 2 Death Animations
            Type2DeathAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Animations.DeathList"), true, true, true, true);
            DrawAnimationList(Type2DeathAnimationList);
            Type2DeathAnimationList.onChangedCallback = (Type2DeathAnimationList) => { AnimationListsChangedProp.boolValue = true; };

            //Emote Animations
            EmoteAnimationList = new ReorderableList(serializedObject, serializedObject.FindProperty("EmoteAnimationList"), true, true, true, true);
            EmoteAnimationList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = EmoteAnimationList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("EmoteAnimationClip"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AnimationID"), GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        AnimationListsChangedProp.boolValue = true;
                    }
                };

            EmoteAnimationList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "   ID  " + "         Emote Animation Clip", EditorStyles.boldLabel);
            };
            EmoteAnimationList.onChangedCallback = (EmoteAnimationList) =>
            {
                AnimationListsChangedProp.boolValue = true;
            };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            HelpButtonStyle = CustomEditorProperties.UpdateHelpButtonStyle();
            AnimationProfile self = (AnimationProfile)target;
            serializedObject.Update();

            UpdateAnimatorController(self);
            NoGeneratedAnimatorMessage(self);

            CustomEditorProperties.BeginScriptHeader("Animation Profile", AnimationProfileEditorIcon);

            EditorGUI.BeginDisabledGroup(!self.AnimatorControllerGenerated);
            EditorGUILayout.Space();
            AnimatorControllerSettings(self);
            EditorGUILayout.Space();
            NonCombatAnimations(self);
            EditorGUILayout.Space();
            Type1CombatAnimations(self);
            EditorGUILayout.Space();
            Type2CombatAnimations(self);
            EditorGUILayout.Space();
            EmoteAnimations(self);
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
            UpdateEditor(self);
            serializedObject.ApplyModifiedProperties();

            CustomEditorProperties.EndScriptHeader();
        }

        /// <summary>
        /// Handles all non-combat related animations
        /// </summary>
        void NonCombatAnimations(AnimationProfile self)
        {
            NonCombatAnimationsFoldout.boolValue = EditorGUILayout.Foldout(NonCombatAnimationsFoldout.boolValue, "Non-Combat Animations", true, FoldoutStyle);

            if (NonCombatAnimationsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Non-Combat Animations", "Controls all animations for when an AI is not in combat or wandering.", true);
                CustomEditorProperties.BeginIndent(20);
                NonCombatIdleAnimations(self);
                EditorGUILayout.Space();
                NonCombatMovementAnimations(self);
                EditorGUILayout.Space();
                NonCombatCombatHitAnimations(self);
                EditorGUILayout.Space();
                NonCombatDeathAnimations(self);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Handles all Type 1 combat related animations
        /// </summary>
        void Type1CombatAnimations(AnimationProfile self)
        {
            Type1CombatAnimationsFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatAnimationsFoldout.boolValue, "Type 1 Combat Animations", true, FoldoutStyle);

            if (Type1CombatAnimationsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Type 1 Combat Animations", "Controls all combat animations for the Type 1 Weapon Type. If you are only using one weapon type, this will be all your default combat animations. If your AI " +
                    "has the same non-combat and combat animations, you have the option to auto-fill your non-combat animations to all of your combat animation slots.", true);
                CustomEditorProperties.BeginIndent(20);
                Type1CombatIdleAnimations(self);
                EditorGUILayout.Space();
                Type1CombatMovement();
                EditorGUILayout.Space();
                Type1StrafeAnimations(self);
                EditorGUILayout.Space();
                Type1DodgeAnimations(self);
                EditorGUILayout.Space();
                Type1CombatHitAnimations(self);
                EditorGUILayout.Space();
                Type1BlockAnimations(self);
                EditorGUILayout.Space();
                Type1DeathAnimations(self);
                EditorGUILayout.Space();
                Type1AttackAnimations(self);
                EditorGUILayout.Space();
                Type1EquipAnimations(self);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Handles all Type 2 combat related animations
        /// </summary>
        void Type2CombatAnimations(AnimationProfile self)
        {
            Type2CombatAnimationsFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatAnimationsFoldout.boolValue, "Type 2 Combat Animations", true, FoldoutStyle);

            if (Type2CombatAnimationsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Type 2 Combat Animations", "Controls all combat animations for the Type 2 Weapon Type.", true);
                CustomEditorProperties.BeginIndent(20);
                Type2CombatIdleAnimations(self);
                EditorGUILayout.Space();
                Type2CombatMovement();
                EditorGUILayout.Space();
                Type2StrafeAnimations(self);
                EditorGUILayout.Space();
                Type2DodgeAnimations(self);
                EditorGUILayout.Space();
                Type2CombatHitAnimations(self);
                EditorGUILayout.Space();
                Type2BlockAnimations(self);
                EditorGUILayout.Space();
                Type2DeathAnimations(self);
                EditorGUILayout.Space();
                Type2AttackAnimations(self);
                EditorGUILayout.Space();
                Type2EquipAnimations(self);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatMovementAnimations(AnimationProfile self)
        {
            //Non-Combat Movement
            WalkFoldout.boolValue = EditorGUILayout.Foldout(WalkFoldout.boolValue, "Walk Animations", true, FoldoutStyle);

            if (WalkFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Walk Forward
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.WalkForward, "Walk Forward", "The walk animation that plays when your AI is walking forward when not in combat.", 2, false, true);

                //Walk Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.WalkLeft, "Walk Left", "The walk animation that plays when your AI is walking left when not in combat. If you do not have a Walk Left animation, the Walk Forward animation can be used instead.", 2, true, true);

                //Walk Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.WalkRight, "Walk Right", "The walk animation that plays when your AI is walking right when not in combat. If you do not have a Walk Right animation, the Walk Forward animation can be used instead.", 0, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            RunFoldout.boolValue = EditorGUILayout.Foldout(RunFoldout.boolValue, "Run Animations", true, FoldoutStyle);

            if (RunFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Run Forward
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.RunForward, "Run Forward", "The run animation that plays when your AI is running forward when not in combat.", 2, false, true);

                //Run Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.RunLeft, "Run Left", "The run animation that plays when your AI is running left when not in combat. If you do not have a Run Left animation, the Run Forward animation can be used instead.", 2, true, true);

                //Run Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.RunRight, "Run Right", "The run animation that plays when your AI is running right when not in combat. If you do not have a Run Right animation, the Run Forward animation can be used instead.", 0, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            TurnFoldout.boolValue = EditorGUILayout.Foldout(TurnFoldout.boolValue, "Turn Animations", true, FoldoutStyle);

            if (TurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Turn Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.TurnLeft, "Turn Left", "The animation clip for turning left (and is stationary) when not in combat.", 2, true, true);

                //Turn Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.TurnRight, "Turn Right", "The animation clip for turning right (and is stationary) when not in combat.", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
            //Non-Combat Movement
        }

        void Type1CombatMovement()
        {
            AnimationProfile self = (AnimationProfile)target;
            Type1CombatWalkFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatWalkFoldout.boolValue, "Combat Walk Animations", true, FoldoutStyle);

            //Type 1 Combat Walks
            if (Type1CombatWalkFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Combat Walk Forward
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkForward, "Combat Walk Forward", "The walk animation that plays when your AI is walking forward when in combat.", 2, false, true);

                //Combat Walk Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkLeft, "Combat Walk Left", "The walk animation that plays when your AI is walking left when in combat. If you do not have a Walk Left animation, the Walk Forward animation can be used instead.", 2, true, true);

                //Combat Walk Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkRight, "Combat Walk Right", "The walk animation that plays when your AI is walking right when in combat. If you do not have a Walk Right animation, the Walk Forward animation can be used instead.", 2, true, true);

                //Walk Back
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.WalkBack, "Combat Walk Back", "The walk animation that plays when your AI is walking backwards when in combat.", 1, true, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type1CombatRunFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatRunFoldout.boolValue, "Combat Run Animations", true, FoldoutStyle);

            //Combat Runs
            if (Type1CombatRunFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Combat Run Forward
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.RunForward, "Combat Run Forward ", "The run animation that plays when your AI is running forward when in combat.", 2, false, true);

                //Combat Run Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.RunLeft, "Combat Run Left ", "The run animation that plays when your AI is running left when in combat. If you do not have a Run Left animation, the Run Forward animation can be used instead.", 2, true, true);

                //Combat Run Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.RunRight, "Combat Run Right ", "The run animation that plays when your AI is running right when in combat. If you do not have a Run Right animation, the Run Forward animation can be used instead.", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type1CombatTurnFoldout.boolValue = EditorGUILayout.Foldout(Type1CombatTurnFoldout.boolValue, "Combat Turn Animations", true, FoldoutStyle);

            //Combat Stationary Turns
            if (Type1CombatTurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Combat Turn Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.TurnLeft, "Combat Turn Left", "The turn animation that plays when your AI is turning left (and is stationary) when in combat.", 2, true, true);

                //Combat Turn Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.TurnRight, "Combat Turn Right", "The turn animation that plays when your AI is turning right (and is stationary) when in combat.", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2CombatMovement()
        {
            AnimationProfile self = (AnimationProfile)target;

            Type2CombatWalkFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatWalkFoldout.boolValue, "Combat Walk Animations", true, FoldoutStyle);

            //Type 2 Walks
            if (Type2CombatWalkFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Type 2 Combat Walk Forward
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkForward, "Combat Walk Forward", "The walk animation that plays when your AI is walking forward when in combat.", 2, false, true);

                //Type 2 Combat Walk Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkLeft, "Combat Walk Left", "The walk animation that plays when your AI is walking left when in combat. If you do not have a Walk Left animation, the Walk Forward animation can be used instead.", 2, true, true);

                //Type 2 Combat Walk Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkRight, "Combat Walk Right", "The walk animation that plays when your AI is walking right when in combat. If you do not have a Walk Right animation, the Walk Forward animation can be used instead.", 2, true, true);

                //Type 2 Walk Back
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.WalkBack, "Combat Walk Back", "The walk animation that plays when your AI is walking backwards when in combat.", 1, true, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type2CombatRunFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatRunFoldout.boolValue, "Combat Run Animations", true, FoldoutStyle);

            //Type 2 Combat Runs
            if (Type2CombatRunFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Type 2 Combat Run Forward
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.RunForward, "Combat Run Forward", "The run animation that plays when your AI is running forward when in combat.", 2, false, true);

                //Type 2 Combat Run Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.RunLeft, "Combat Run Left", "The run animation that plays when your AI is running left when in combat. If you do not have a Run Left animation, the Run Forward animation can be used instead.", 2, true, true);

                //Type 2 Combat Run Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.RunRight, "Combat Run Right", "The run animation that plays when your AI is running right when in combat. If you do not have a Run Right animation, the Run Forward animation can be used instead.", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }

            EditorGUILayout.Space();
            Type2CombatTurnFoldout.boolValue = EditorGUILayout.Foldout(Type2CombatTurnFoldout.boolValue, "Combat Turn Animations", true, FoldoutStyle);

            //Type 2 Combat Stationary Turns
            if (Type2CombatTurnFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                //Ranhed Combat Stationary Turn Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.TurnLeft, "Combat Turn Left", "The turn animation that plays when your AI is turning left (and is stationary) when in combat.", 2, true, true);

                //Ranhed Combat Stationary Turn Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.TurnRight, "Combat Turn Right", "The turn animation that plays when your AI is turning right (and is stationary) when in combat.", 1, true, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1EquipAnimations(AnimationProfile self)
        {
            Type1EquipsFoldout.boolValue = EditorGUILayout.Foldout(Type1EquipsFoldout.boolValue, "Equip & Unequip Animations", true, FoldoutStyle);

            if (Type1EquipsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Equip & Unequip Animations", "Controls the animations that will be used for equipping and unequipping an AI's weapon objects. If you do not apply animations here, this feature will be ignored.", true);

                CustomEditorProperties.TutorialButton("Note: This requires an AI to have an EquipWeapon and UnequipWeapon Animation Events setup on the equip and unequip animations. " +
                    "For a guide on how to do this, refer to the Emerald AI Documentation, if you haven'transform yet set them up and would like to use this feature.",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component/creating-equippable-weapons");

                //Pullout Weapon
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.PullOutWeapon, "Equip Weapon", "The animation that plays when the AI is pulling out their weapon.", 0, false, false);
                //Put Away Weapon
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.PutAwayWeapon, "Unequip Weapon", "The animation that plays when the AI is putting away their weapon.", 1, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2EquipAnimations(AnimationProfile self)
        {
            Type2EquipsFoldout.boolValue = EditorGUILayout.Foldout(Type2EquipsFoldout.boolValue, "Equip & Unequip Animations", true, FoldoutStyle);

            if (Type2EquipsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Equip & Unequip Animations", "Controls the animations that will be used for equipping and unequipping an AI's weapon objects. If you do not apply animations here, this feature will be ignored.", true);

                CustomEditorProperties.TutorialButton("Note: This requires an AI to have an EquipWeapon and UnequipWeapon Animation Events setup on the equip and unequip animations. " +
                    "For a guide on how to do this, refer to the Emerald AI Documentation, if you haven'transform yet set them up and would like to use this feature.",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component/creating-equippable-weapons");

                //Pullout Weapon
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.PullOutWeapon, "Equip Weapon", "The animation that plays when the AI is pulling out their weapon.", 0, false, false);
                //Put Away Weapon
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.PutAwayWeapon, "Unequip Weapon", "The animation that plays when the AI is putting away their weapon.", 1, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1AttackAnimations(AnimationProfile self)
        {
            Type1AttacksFoldout.boolValue = EditorGUILayout.Foldout(Type1AttacksFoldout.boolValue, "Attack Animations", true, FoldoutStyle);

            if (Type1AttacksFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Attack Animations", "Controls the attack animations that an will use when the AI in combat. A max of 12 can be used. Attack animations should have 'Loop Time' unchecked.", true);

                CustomEditorProperties.ImportantTutorialButton("Note: You will need to manually create a CreateAbility Animation Event on each of your AI's attack animations to allow your AI to trigger an attack. " +
                "Please refer to Emerlad's documentation for a tutorial on how to do this.", "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-managers/animation-viewer-manager/creating-attack-animation-events");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Type 1 Attack Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the attack animations that an will use when the AI in combat. A max of 12 can be used. Attack animations should have 'Loop Time' unchecked.", false);
                CustomEditorProperties.NoticeTextDescription("Note: Reordering this list, or removing an animation from this list, will change the order of the generated Attack Animation names within the AI's Type 1 Attacks (given they have already been assigned).", false);

                EditorGUILayout.Space();
                Type1AttackAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Attack
                if (self.Type1Animations.AttackList.Count == 12)
                {
                    Type1AttackAnimationList.displayAdd = false;
                }
                else
                {
                    Type1AttackAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2AttackAnimations(AnimationProfile self)
        {
            Type2AttacksFoldout.boolValue = EditorGUILayout.Foldout(Type2AttacksFoldout.boolValue, "Attack Animations", true, FoldoutStyle);

            if (Type2AttacksFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Attack Animations", "Controls the attack animations that an will use when the AI in combat. A max of 12 can be used. Attack animations should have 'Loop Time' unchecked.", true);

                CustomEditorProperties.TutorialButton("Note: You will need to manually create an CreateAbility Animation Event on each of your AI's attack animations to allow your AI to trigger an attack. " +
                "Please refer to Emerlad's documentation for a tutorial on how to do this.", "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-managers/animation-viewer-manager/creating-attack-animation-events");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Type 2 Attack Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the attack animations that an will use when the AI in combat. A max of 12 can be used. Attack animations should have 'Loop Time' unchecked.", false);

                EditorGUILayout.Space();
                Type2AttackAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Attack
                if (self.Type2Animations.AttackList.Count == 6)
                {
                    Type2AttackAnimationList.displayAdd = false;
                }
                else
                {
                    Type2AttackAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatIdleAnimations(AnimationProfile self)
        {
            NonCombatIdleFoldout.boolValue = EditorGUILayout.Foldout(NonCombatIdleFoldout.boolValue, "Idle Animations", true, FoldoutStyle);

            if (NonCombatIdleFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Idle Animations", "Controls idle animations that this AI will use when wandering.", true);
                EditorGUILayout.LabelField("Idle Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the idle animations that will randomly play when the AI is wandering or grazing. A max of 6 can be used.", false);
                NonCombatIdleAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Idle
                if (self.NonCombatAnimations.IdleList.Count == 6)
                {
                    NonCombatIdleAnimationList.displayAdd = false;
                }
                else
                {
                    NonCombatIdleAnimationList.displayAdd = true;
                }

                //Non-Combat Idke
                CustomEditorProperties.DrawAnimationClassVariables(self, self.NonCombatAnimations.IdleStationary, "Idle Non-Combat", "Controls the default idle animation.", 0, false, true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1CombatIdleAnimations(AnimationProfile self)
        {
            Type1IdleFoldout.boolValue = EditorGUILayout.Foldout(Type1IdleFoldout.boolValue, "Combat Idle Animations", true, FoldoutStyle);

            if (Type1IdleFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.IdleStationary, "Combat Idle", "Controls the idle animation that the AI will play while an AI is in Combat Mode.", 2, false, false);

                //Type 1 Idle Warning
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.IdleWarning, "Type 1 Idle Warning", "Controls the animation that the AI will play to warn a target that they will attack, if the target doesn'transform leave their attack radius soon.", 1, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2CombatIdleAnimations(AnimationProfile self)
        {
            Type2IdleFoldout.boolValue = EditorGUILayout.Foldout(Type2IdleFoldout.boolValue, "Idle Animations", true, FoldoutStyle);

            if (Type2IdleFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.IdleStationary, "Combat Idle", "Controls the ranged idle animation that the AI will play while an AI is in Combat Mode.", 2, false, true);

                //Type 2 Idle Warning
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.IdleWarning, "Type 2 Idle Warning", "Controls the animation that the AI will play to warn a target that they will attack, if the target doesn'transform leave their attack radius soon.", 0, false, true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatCombatHitAnimations(AnimationProfile self)
        {
            NonCombatHitFoldout.boolValue = EditorGUILayout.Foldout(NonCombatHitFoldout.boolValue, "Hit Animations", true, FoldoutStyle);

            if (NonCombatHitFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                EditorGUILayout.LabelField("Hit Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the animations that will play when an AI receives damage when not in combat.", false);
                NonCombatHitAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Hit
                if (self.NonCombatAnimations.HitList.Count == 6)
                {
                    NonCombatHitAnimationList.displayAdd = false;
                }
                else
                {
                    NonCombatHitAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1CombatHitAnimations(AnimationProfile self)
        {
            Type1HitFoldout.boolValue = EditorGUILayout.Foldout(Type1HitFoldout.boolValue, "Combat Hit Animations", true, FoldoutStyle);

            if (Type1HitFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Stunned
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.Stunned, "Stunned", "Controls the stunned animation that plays after an AI gets struck by a stunning hit. " +
                    "Hit animations will be blended with the Stunned Animation while in this state (this feature can be disabled by excluding the Stunned state from the Hit Conditions below).", 2, false, false);

                CustomEditorProperties.CustomPropertyField(Type1HitConditionsProp, "Hit Conditions", "Controls which states are allowed to be canceled to play a hit animation. Note: Dodge and Equipping are automatically excluded.", false);
                CustomEditorProperties.CustomFloatSliderPropertyField(Type1HitAnimationCooldownProp, "Hit Animation Cooldown", "Controls the time (in seconds) that need to pass in order to play another hit animation.", 0f, 4f, true);

                EditorGUILayout.LabelField("Combat Hit Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the animations that will play when an AI receives damage when not in combat.", false);
                Type1CombatHitAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Combat Hit
                if (self.Type1Animations.HitList.Count == 6)
                {
                    Type1CombatHitAnimationList.displayAdd = false;
                }
                else
                {
                    Type1CombatHitAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2CombatHitAnimations(AnimationProfile self)
        {
            Type2HitFoldout.boolValue = EditorGUILayout.Foldout(Type2HitFoldout.boolValue, "Combat Hit Animations", true, FoldoutStyle);

            if (Type2HitFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Stunned
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.Stunned, "Stunned", "Controls the stunned animation that plays after an AI gets struck by a stunning hit. " +
                    "Hit animations will be blended with the Stunned Animation while in this state (this feature can be disabled by excluding the Stunned state from the Hit Conditions below).", 2, false, false);

                CustomEditorProperties.CustomPropertyField(Type2HitConditionsProp, "Hit Conditions", "Controls which states are allowed to be canceled to play a hit animation. Note: Dodge and Equipping are automatically excluded.", false);
                CustomEditorProperties.CustomFloatSliderPropertyField(Type2HitAnimationCooldownProp, "Hit Animation Cooldown", "Controls the time (in seconds) that need to pass in order to play another hit animation.", 0f, 4f, true);

                EditorGUILayout.LabelField("Combat Hit Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the animations that will play when an AI receives damage when not in combat.", false);
                Type2CombatHitAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Combat Hit
                if (self.Type2Animations.HitList.Count == 6)
                {
                    Type2CombatHitAnimationList.displayAdd = false;
                }
                else
                {
                    Type2CombatHitAnimationList.displayAdd = true;
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1BlockAnimations(AnimationProfile self)
        {
            Type1BlockFoldout.boolValue = EditorGUILayout.Foldout(Type1BlockFoldout.boolValue, "Combat Block Animations", true, FoldoutStyle);

            if (Type1BlockFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Block Idle
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.BlockIdle, "Block Idle Animation", "The looping animation that plays when your AI is blocking.", 2, false, false);
                //Block Hit
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.BlockHit, "Block Impact Animation", "The animation that plays when your AI is blocking and is hit with an attack.", 2, false, false);
                //Recoil
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.Recoil, "Recoil", "Controls the recoil animation that plays after an AI hits a blocking target.", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2BlockAnimations(AnimationProfile self)
        {
            Type2BlockFoldout.boolValue = EditorGUILayout.Foldout(Type2BlockFoldout.boolValue, "Combat Block Animations", true, FoldoutStyle);

            if (Type2BlockFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Block Idle
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.BlockIdle, "Block Animation", "The animation that plays when your AI is blocking.", 2, false, false);
                //Block Hit
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.BlockHit, "Block Impact Animation", "The animation that plays when your AI is blocking and is hit with an attack.", 2, false, false);
                //Recoil
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.Recoil, "Recoil", "Controls the recoil animation that plays after an AI hits a blocking target.", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void NonCombatDeathAnimations(AnimationProfile self)
        {
            NonCombatDeathFoldout.boolValue = EditorGUILayout.Foldout(NonCombatDeathFoldout.boolValue, "Death Animations", true, FoldoutStyle);

            if (NonCombatDeathFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                EditorGUILayout.LabelField("Death Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the animations that will play when an AI dies. Note: If no death animations are applied, an AI will use ragdoll deaths.", false);
                CustomEditorProperties.NoticeTextDescription("Note: If no death animations are applied, an AI will automatically use ragdoll deaths, which requires an AI to be setup through Unity's Ragdoll Wizard or another 3rd party ragdoll tool.", false);
                NonCombatDeathAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Death
                if (self.NonCombatAnimations.DeathList.Count == 6)
                {
                    NonCombatDeathAnimationList.displayAdd = false;
                }
                else
                {
                    NonCombatDeathAnimationList.displayAdd = true;
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1DeathAnimations(AnimationProfile self)
        {
            Type1DeathFoldout.boolValue = EditorGUILayout.Foldout(Type1DeathFoldout.boolValue, "Combat Death Animations", true, FoldoutStyle);

            if (Type1DeathFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                EditorGUILayout.LabelField("Death Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the animations that will play when an AI dies. Note: If no death animations are applied, an AI will use ragdoll deaths.", false);
                CustomEditorProperties.NoticeTextDescription("Note: If no death animations are applied, an AI will automatically use ragdoll deaths, which requires an AI to be setup through Unity's Ragdoll Wizard or another 3rd party ragdoll tool.", false);
                Type1DeathAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Death
                if (self.Type1Animations.DeathList.Count == 6)
                {
                    Type1DeathAnimationList.displayAdd = false;
                }
                else
                {
                    Type1DeathAnimationList.displayAdd = true;
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2DeathAnimations(AnimationProfile self)
        {
            Type2DeathFoldout.boolValue = EditorGUILayout.Foldout(Type2DeathFoldout.boolValue, "Combat Death Animations", true, FoldoutStyle);

            if (Type2DeathFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                EditorGUILayout.LabelField("Death Animations", EditorStyles.boldLabel);
                CustomEditorProperties.CustomHelpLabelField("Controls the animations that will play when an AI dies.", false);
                CustomEditorProperties.NoticeTextDescription("Note: If no death animations are applied, an AI will automatically use ragdoll deaths, which requires an AI to be setup through Unity's Ragdoll Wizard or another 3rd party ragdoll tool.", false);
                Type2DeathAnimationList.DoLayoutList();
                EditorGUILayout.Space();

                //Death
                if (self.Type1Animations.DeathList.Count == 6)
                {
                    Type2DeathAnimationList.displayAdd = false;
                }
                else
                {
                    Type2DeathAnimationList.displayAdd = true;
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1StrafeAnimations(AnimationProfile self)
        {
            Type1StrafeFoldout.boolValue = EditorGUILayout.Foldout(Type1StrafeFoldout.boolValue, "Combat Strafe Animations", true, FoldoutStyle);

            if (Type1StrafeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Strafe Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.StrafeLeft, "Strafe Left", "Controls the strafe animation when strafing left.", 2, false, true);

                //Strafe Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.StrafeRight, "Strafe Right", "Controls the strafe animation when strafing right.", 2, false, true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2StrafeAnimations(AnimationProfile self)
        {
            Type2StrafeFoldout.boolValue = EditorGUILayout.Foldout(Type2StrafeFoldout.boolValue, "Combat Strafe Animations", true, FoldoutStyle);

            if (Type2StrafeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Strafe Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.StrafeLeft, "Strafe Left", "Controls the strafe animation when strafing left.", 2, false, true);

                //Strafe Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.StrafeRight, "Strafe Right", "Controls the strafe animation when strafing right.", 2, false, true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type1DodgeAnimations(AnimationProfile self)
        {
            Type1DodgeFoldout.boolValue = EditorGUILayout.Foldout(Type1DodgeFoldout.boolValue, "Combat Dodge Animations", true, FoldoutStyle);

            if (Type1DodgeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Dodge Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.DodgeLeft, "Dodge Left", "Controls the dodge animation when dodging left.", 2, false, false);

                //Dodge Back
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.DodgeBack, "Dodge Back", "Controls the dodge animation when dodging back.", 2, false, false);

                //Dodge Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type1Animations.DodgeRight, "Dodge Right", "Controls the dodge animation when dodging right.", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void Type2DodgeAnimations(AnimationProfile self)
        {
            Type2DodgeFoldout.boolValue = EditorGUILayout.Foldout(Type2DodgeFoldout.boolValue, "Combat Dodge Animations", true, FoldoutStyle);

            if (Type2DodgeFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                //Dodge Left
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.DodgeLeft, "Dodge Left", "Controls the dodge animation when dodging left.", 2, false, false);

                //Dodge Back
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.DodgeBack, "Dodge Back", "Controls the dodge animation when dodging back.", 2, false, false);

                //Dodge Right
                CustomEditorProperties.DrawAnimationClassVariables(self, self.Type2Animations.DodgeRight, "Dodge Right", "Controls the dodge animation when dodging right.", 2, false, false);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void EmoteAnimations(AnimationProfile self)
        {
            EmotesFoldout.boolValue = EditorGUILayout.Foldout(EmotesFoldout.boolValue, "Emote Animations", true, FoldoutStyle);

            if (EmotesFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Emote Animations", "Controls the animations an AI can use as emotes.", true);

                EditorGUILayout.LabelField("Emote Animations", EditorStyles.boldLabel);
                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
                EditorGUILayout.LabelField("Controls the emote animations that will play when an AI's PlayEmoteAnimation function is called and passing the emote ID as the parameter. The speed of each animation can be adjusted by changing the speed parameter. A max of 10 can be used.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EmoteAnimationList.DoLayoutList();

                //Emote
                if (self.EmoteAnimationList.Count == 10)
                {
                    EmoteAnimationList.displayAdd = false;
                }
                else
                {
                    EmoteAnimationList.displayAdd = true;
                }

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void AnimatorControllerSettings(AnimationProfile self)
        {
            AnimatorSettingsFoldout.boolValue = EditorGUILayout.Foldout(AnimatorSettingsFoldout.boolValue, "Animator Settings", true, FoldoutStyle);

            if (AnimatorSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Animator Settings", "The Animation system will automatically update an AI's Animator Controller as changes are made so there's no need to apply animations manually to it.", true);

                DisplayAnimatorController();
                EditorGUILayout.PropertyField(AnimatorCullingModeProp, new GUIContent("Animator Culling Mode"));
                CustomEditorProperties.CustomHelpLabelField("Controls what type of Culling Mode this AI's Animator will use. Always Animate is recommended when using animated deaths as an AI can sometimes get stuck in T-pose if they die while off-screen.", true);

                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Copy Non-Combat Animations to Type 1 Combat Animations"), GUILayout.Height(23)))
                {
                    CopyNonCombatAnimationsToType1(self);
                }
                if (GUILayout.Button(new GUIContent("Copy Non-Combat Animations to Type 2 Combat Animations"), GUILayout.Height(23)))
                {
                    CopyNonCombatAnimationsToType2(self);
                }
                CustomEditorProperties.CustomHelpLabelField("Use this to copy all non-combat animations (idles, turns, movement, hits, and death) so the same animations don'transform have to be reassigned as the combat animations." +
                    " Note: This will only work for the Type 1 Weapon Type and is practical for AI who use the same animations for the non-combat animations and their Type 1 Combat Animations.", true);

                CopyAnimationProfileButton(self);
                RegenerateAnimatorControllerButton(self);
                ClearAnimatorControllerButton(self);

                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void CheckForMissingAnimationsButton(AnimationProfile self)
        {
            CustomEditorProperties.NoticeTextTitleWithDescription("Reminder", "All animation slots that are enabled must have animations applied to avoid errors. Please ensure you have applied all of the neccesary " +
                    "animations before using this AI. Note: You can press the 'Check for Missing Animations' button below to have Emerald AI debug log missing animations to the Unity Console so you don'transform have to manually " +
                    "look through each animation tab.", false);

            GUI.backgroundColor = new Color(1.5f, 0f, 0f, 0.5f);
            if (GUILayout.Button("Check for Missing Animations", HelpButtonStyle, GUILayout.Height(23)))
            {
                CheckForMissingAnimations();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Updates the Animator Controller automatically when changes are made.
        /// </summary>
        /// <param name="self"></param>
        void UpdateAnimatorController(AnimationProfile self)
        {
            //Only auto-update the Animator Controller if inside the Unity Editor as runtime auto-updating is not possible.
#if UNITY_EDITOR
            if (self.AnimatorControllerGenerated && self.AIAnimator != null)
            {
                if (self.AnimationsUpdated || self.AnimationListsChanged)
                {
                    EmeraldAnimatorGenerator.GenerateAnimatorController(self);
                }
            }
#endif
        }

        void NoGeneratedAnimatorMessage(AnimationProfile self)
        {
            if (!self.AnimatorControllerGenerated)
            {
                EditorGUILayout.Space();
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.35f);
                EditorGUILayout.HelpBox("There is currently no generated Animator Controller for this Animation Profile. You will need to press the Create Animator Controller button below first before you can apply animations.", MessageType.Warning);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space();
                CreateAnimatorControllerButton(self); //Draws the Create Animator Controller button 
            }
        }

        void CheckForMissingAnimations()
        {
            //TODO: Instead of checking all aniations, only check essential ones.
            //EmeraldAI.Internal.AnimationCheck.CheckForMissingAnimations(EmeraldComp);
        }

        void CreateAnimatorControllerButton(AnimationProfile self)
        {
            if (!self.AnimatorControllerGenerated || self.MissingRuntimeController)
            {
                if (GUILayout.Button("Create Animator Controller", GUILayout.Height(23)))
                {
                    self.FilePath = EditorUtility.SaveFilePanelInProject("Save as OverrideController", "", "overrideController", "Please enter a file name to save the file to");
                    if (self.FilePath != string.Empty)
                    {
                        string UserFilePath = self.FilePath;
                        string SourceFilePath = AssetDatabase.GetAssetPath(Resources.Load("Emerald Animator Controller"));
                        AssetDatabase.CopyAsset(SourceFilePath, UserFilePath);
                        self.AIAnimator = AssetDatabase.LoadAssetAtPath(UserFilePath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                        EmeraldAnimatorGenerator.GenerateAnimatorController(self);
                        serializedObject.Update();
                        self.AnimatorControllerGenerated = true;
                        AnimationsUpdatedProp.boolValue = false;
                        EditorUtility.SetDirty(self);
                        self.MissingRuntimeController = false;
                    }
                }
            }
        }

        void DisplayAnimatorController ()
        {
            EditorGUILayout.PropertyField(AIAnimatorProp, new GUIContent("Animator Controller"));
            CustomEditorProperties.CustomHelpLabelField("The current Animator Controller for this Animation Profile. Any AI that is using this Animation Profile will have this Animator Controller applied to it at runtime.", true);
        }

        void RegenerateAnimatorControllerButton(AnimationProfile self)
        {
            EditorGUILayout.Space();
            CustomEditorProperties.CustomHelpLabelFieldWithType("Regenerates the current Animator Controller. Use only if there have been changes to the master Emerald AI controller and you'd like to update to the newest version. " +
                "Note: This will overwrite any custom changes you've made to this Animator Controller.", false, new Color(145f, 145f, 0f, 0.6f), MessageType.Info);

            GUI.backgroundColor = new Color(1.5f, 1.3f, 0, 1f);
            if (GUILayout.Button("Regenerate Animator Controller", HelpButtonStyle, GUILayout.Height(23)) && EditorUtility.DisplayDialog("Regenerate Animator Controller?", "Are you sure you want to regenerate this Animator Controller? " +
                "This will overwrite any custom changes you've made to this Animator Controller. This process cannot be undone.", "Yes", "Cancel"))
            {
                string SourceFilePath = AssetDatabase.GetAssetPath(Resources.Load("Emerald Animator Controller"));
                string ControllerPath = self.FilePath;
                AssetDatabase.CopyAsset(SourceFilePath, ControllerPath);
                var TempRuntimeAnimator = AssetDatabase.LoadAssetAtPath(ControllerPath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                self.AIAnimator = TempRuntimeAnimator;
                EmeraldAnimatorGenerator.GenerateAnimatorController(self); //Regenerate the Animator Controller using the animations from the Animation Profile.
                ApplyRuntimeAnimatorController();
            }
            GUI.backgroundColor = Color.white;
        }

        void CopyNonCombatAnimationsToType1 (AnimationProfile self)
        {
            if (EditorUtility.DisplayDialog("Copy Non-Combat Animations?", "Are you sure you want to copy your non-combat animations to the Type 1 combat animations? This process cannot be undone.", "Okay", "Cancel"))
            {
                self.Type1Animations.IdleStationary = new AnimationClass(self.NonCombatAnimations.IdleStationary.AnimationSpeed, self.NonCombatAnimations.IdleStationary.AnimationClip, self.NonCombatAnimations.IdleStationary.Mirror);

                self.Type1Animations.WalkForward = new AnimationClass(self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, self.NonCombatAnimations.WalkForward.Mirror);
                self.Type1Animations.WalkLeft = new AnimationClass(self.NonCombatAnimations.WalkLeft.AnimationSpeed, self.NonCombatAnimations.WalkLeft.AnimationClip, self.NonCombatAnimations.WalkLeft.Mirror);
                self.Type1Animations.WalkRight = new AnimationClass(self.NonCombatAnimations.WalkRight.AnimationSpeed, self.NonCombatAnimations.WalkRight.AnimationClip, self.NonCombatAnimations.WalkRight.Mirror);
                self.Type1Animations.WalkBack = new AnimationClass(-self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, false);

                self.Type1Animations.RunForward = new AnimationClass(self.NonCombatAnimations.RunForward.AnimationSpeed, self.NonCombatAnimations.RunForward.AnimationClip, self.NonCombatAnimations.RunForward.Mirror);
                self.Type1Animations.RunLeft = new AnimationClass(self.NonCombatAnimations.RunLeft.AnimationSpeed, self.NonCombatAnimations.RunLeft.AnimationClip, self.NonCombatAnimations.RunLeft.Mirror);
                self.Type1Animations.RunRight = new AnimationClass(self.NonCombatAnimations.RunRight.AnimationSpeed, self.NonCombatAnimations.RunRight.AnimationClip, self.NonCombatAnimations.RunRight.Mirror);

                self.Type1Animations.TurnLeft = new AnimationClass(self.NonCombatAnimations.TurnLeft.AnimationSpeed, self.NonCombatAnimations.TurnLeft.AnimationClip, self.NonCombatAnimations.TurnLeft.Mirror);
                self.Type1Animations.TurnRight = new AnimationClass(self.NonCombatAnimations.TurnRight.AnimationSpeed, self.NonCombatAnimations.TurnRight.AnimationClip, self.NonCombatAnimations.TurnRight.Mirror);

                self.Type1Animations.HitList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.HitList.Count; i++)
                {
                    self.Type1Animations.HitList.Add(new AnimationClass(self.NonCombatAnimations.HitList[i].AnimationSpeed, self.NonCombatAnimations.HitList[i].AnimationClip, self.NonCombatAnimations.HitList[i].Mirror));
                }

                self.Type1Animations.DeathList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.DeathList.Count; i++)
                {
                    self.Type1Animations.DeathList.Add(new AnimationClass(self.NonCombatAnimations.DeathList[i].AnimationSpeed, self.NonCombatAnimations.DeathList[i].AnimationClip, self.NonCombatAnimations.DeathList[i].Mirror));
                }

                serializedObject.Update();
                AnimationsUpdatedProp.boolValue = true;
                serializedObject.ApplyModifiedProperties();
            }
        }

        void CopyNonCombatAnimationsToType2(AnimationProfile self)
        {
            if (EditorUtility.DisplayDialog("Copy Non-Combat Animations?", "Are you sure you want to copy your non-combat animations to the Type 2 combat animations? This process cannot be undone.", "Okay", "Cancel"))
            {
                self.Type2Animations.IdleStationary = new AnimationClass(self.NonCombatAnimations.IdleStationary.AnimationSpeed, self.NonCombatAnimations.IdleStationary.AnimationClip, self.NonCombatAnimations.IdleStationary.Mirror);

                self.Type2Animations.WalkForward = new AnimationClass(self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, self.NonCombatAnimations.WalkForward.Mirror);
                self.Type2Animations.WalkLeft = new AnimationClass(self.NonCombatAnimations.WalkLeft.AnimationSpeed, self.NonCombatAnimations.WalkLeft.AnimationClip, self.NonCombatAnimations.WalkLeft.Mirror);
                self.Type2Animations.WalkRight = new AnimationClass(self.NonCombatAnimations.WalkRight.AnimationSpeed, self.NonCombatAnimations.WalkRight.AnimationClip, self.NonCombatAnimations.WalkRight.Mirror);
                self.Type2Animations.WalkBack = new AnimationClass(-self.NonCombatAnimations.WalkForward.AnimationSpeed, self.NonCombatAnimations.WalkForward.AnimationClip, false);

                self.Type2Animations.RunForward = new AnimationClass(self.NonCombatAnimations.RunForward.AnimationSpeed, self.NonCombatAnimations.RunForward.AnimationClip, self.NonCombatAnimations.RunForward.Mirror);
                self.Type2Animations.RunLeft = new AnimationClass(self.NonCombatAnimations.RunLeft.AnimationSpeed, self.NonCombatAnimations.RunLeft.AnimationClip, self.NonCombatAnimations.RunLeft.Mirror);
                self.Type2Animations.RunRight = new AnimationClass(self.NonCombatAnimations.RunRight.AnimationSpeed, self.NonCombatAnimations.RunRight.AnimationClip, self.NonCombatAnimations.RunRight.Mirror);

                self.Type2Animations.TurnLeft = new AnimationClass(self.NonCombatAnimations.TurnLeft.AnimationSpeed, self.NonCombatAnimations.TurnLeft.AnimationClip, self.NonCombatAnimations.TurnLeft.Mirror);
                self.Type2Animations.TurnRight = new AnimationClass(self.NonCombatAnimations.TurnRight.AnimationSpeed, self.NonCombatAnimations.TurnRight.AnimationClip, self.NonCombatAnimations.TurnRight.Mirror);

                self.Type2Animations.HitList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.HitList.Count; i++)
                {
                    self.Type2Animations.HitList.Add(new AnimationClass(self.NonCombatAnimations.HitList[i].AnimationSpeed, self.NonCombatAnimations.HitList[i].AnimationClip, self.NonCombatAnimations.HitList[i].Mirror));
                }

                self.Type2Animations.DeathList.Clear();
                for (int i = 0; i < self.NonCombatAnimations.DeathList.Count; i++)
                {
                    self.Type2Animations.DeathList.Add(new AnimationClass(self.NonCombatAnimations.DeathList[i].AnimationSpeed, self.NonCombatAnimations.DeathList[i].AnimationClip, self.NonCombatAnimations.DeathList[i].Mirror));
                }

                serializedObject.Update();
                AnimationsUpdatedProp.boolValue = true;
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Applies the Runtime Animator Controller from the Animation Profile to the AI's Animator.
        /// </summary>
        void ApplyRuntimeAnimatorController()
        {
            AnimationProfile self = (AnimationProfile)target;

            if (self.EmeraldAnimationComponent == null || self.EmeraldAnimationComponent.m_AnimationProfile != self)
                return;

            if (self.EmeraldAnimationComponent.AIAnimator != null && self.EmeraldAnimationComponent.AIAnimator.runtimeAnimatorController == null && self.EmeraldAnimationComponent.m_AnimationProfile != null && self.EmeraldAnimationComponent.m_AnimationProfile.AIAnimator != null)
                self.EmeraldAnimationComponent.AIAnimator.runtimeAnimatorController = self.EmeraldAnimationComponent.m_AnimationProfile.AIAnimator;
        }

        void CopyAnimationProfileButton(AnimationProfile self)
        {
            EditorGUILayout.Space();
            CustomEditorProperties.CustomHelpLabelFieldWithType("Creates a copy of this Animation Profile (keeping all of the applied animations) and clears its Animator Controller so a new one can be created.", false, new Color(0.25f, 2f, 0f, 0.75f), MessageType.Info);

            GUI.backgroundColor = new Color(0.1f, 1.2f, 0f, 0.5f);
            if (GUILayout.Button("Copy Animation Profile", HelpButtonStyle, GUILayout.Height(23)) && EditorUtility.DisplayDialog("Copy Animation Profile?", "Are you sure you want to copy this Animation Profile? This process cannot be undone.", "Yes", "Cancel"))
            {
                EmeraldAnimatorGenerator.CopyAnimationProfile(self);
            }
            GUI.backgroundColor = Color.white;
        }

        void ClearAnimatorControllerButton(AnimationProfile self)
        {
            EditorGUILayout.Space();
            CustomEditorProperties.CustomHelpLabelFieldWithType("Clears the current Animator Controller so a new one can be created.", false, new Color(1.5f, 0f, 0f, 0.75f), MessageType.Info);

            GUI.backgroundColor = new Color(1.5f, 0f, 0f, 0.5f);
            if (GUILayout.Button("Clear Animator Controller", HelpButtonStyle, GUILayout.Height(23)) && EditorUtility.DisplayDialog("Clear Animator Controller?", "Are you sure you want to clear this Animator Controller? This process cannot be undone.", "Yes", "Cancel"))
            {
                self.AIAnimator = null;
                self.AnimatorControllerGenerated = false;
            }
            GUI.backgroundColor = Color.white;
        }

        void DrawAnimationList(ReorderableList ListRef)
        {
            ListRef.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = ListRef.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AnimationClip"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AnimationSpeed"), GUIContent.none);

                    if (element.FindPropertyRelative("AnimationSpeed").floatValue == 0)
                    {
                        element.FindPropertyRelative("AnimationSpeed").floatValue = 1;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        AnimationListsChangedProp.boolValue = true;
                    }
                };

            ListRef.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "   Speed  " + "     Clip", EditorStyles.boldLabel);
            };
        }

        /// <summary>
        /// Clears the EmeraldAnimationComponent from the Animation Profile.
        /// </summary>
        void OnDestroy()
        {
            AnimationProfile self = (AnimationProfile)target;
            self.EmeraldAnimationComponent = null;
        }

        void UpdateEditor(AnimationProfile self)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RecordObject(self, "Undo");

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(target);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
#endif
        }
    }
}