using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldCombat))]
    [CanEditMultipleObjects]
    public class EmeraldCombatEditor : Editor
    {
        GUIStyle FoldoutStyle;

        float CurrentAttackDistance = 0;
        float CurrentTooCloseDistance = 0;
        bool DrawDistanceActive = false;
        Texture CombatEditorIcon;

        EmeraldAnimation EmeraldAnimation;
        EmeraldBehaviors EmeraldBehaviors;

        SerializedProperty HideSettingsFoldout, DamageSettingsFoldout, WeaponType1SettingsFoldout, WeaponType2SettingsFoldout, SwitchWeaponSettingsFoldout, CombatActionSettingsFoldout;
        
        //Enums
        SerializedProperty Type1PickTargetTypeProp, Type2PickTargetTypeProp, SwitchWeaponTypeProp, StartingWeaponTypeProp, Type1AttackPickTypeProp, Type2AttackPickTypeProp;

        //Int
        SerializedProperty SwitchWeaponTimeMinProp, SwitchWeaponTimeMaxProp, SwitchWeaponTypesDistanceProp, SwitchWeaponTypesCooldownProp, MinResumeWanderProp, MaxResumeWanderProp;

        //Float
        SerializedProperty Type1AttackCooldownProp, Type2AttackCooldownProp;

        ReorderableList Type1Attacks, Type2Attacks, WeaponType1AttackTransforms, WeaponType2AttackTransforms, Type1ActionsList, Type2ActionsList;

        string AttackTransformTooltip = "Each Attack Transform can be used individually during an CreateAbility by passing the Attack Transform's " +
                        "name through the String parameter of the Animation Event. This allows an AI to have customizable points that attacks or abilities can come from, such as a grenade from a hand, a bullet from a barrel, or a spell from an AI's staff." +
                        "\n\nNote: It is best to keep Attack Transform names consistent to allow them to work across multiple AI.";

        void OnEnable()
        {
            EmeraldCombat self = (EmeraldCombat)target;
            EmeraldAnimation = self.GetComponent<EmeraldAnimation>();
            EmeraldBehaviors = self.GetComponent<EmeraldBehaviors>();
            if (CombatEditorIcon == null) CombatEditorIcon = Resources.Load("Editor Icons/EmeraldCombat") as Texture;
            InitializeProperties();
            InitializeLists(self);
        }

        void InitializeProperties()
        {
            //Bool
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            DamageSettingsFoldout = serializedObject.FindProperty("DamageSettingsFoldout");
            CombatActionSettingsFoldout = serializedObject.FindProperty("CombatActionSettingsFoldout");
            SwitchWeaponSettingsFoldout = serializedObject.FindProperty("SwitchWeaponSettingsFoldout");
            WeaponType1SettingsFoldout = serializedObject.FindProperty("WeaponType1SettingsFoldout");
            WeaponType2SettingsFoldout = serializedObject.FindProperty("WeaponType2SettingsFoldout");

            //Enums
            SwitchWeaponTypeProp = serializedObject.FindProperty("SwitchWeaponType");
            StartingWeaponTypeProp = serializedObject.FindProperty("StartingWeaponType");
            Type1AttackPickTypeProp = serializedObject.FindProperty("Type1Attacks.AttackPickType");
            Type2AttackPickTypeProp = serializedObject.FindProperty("Type2Attacks.AttackPickType");
            Type1PickTargetTypeProp = serializedObject.FindProperty("Type1PickTargetType");
            Type2PickTargetTypeProp = serializedObject.FindProperty("Type2PickTargetType");

            //Int
            SwitchWeaponTimeMinProp = serializedObject.FindProperty("SwitchWeaponTimeMin");
            SwitchWeaponTimeMaxProp = serializedObject.FindProperty("SwitchWeaponTimeMax");
            SwitchWeaponTypesDistanceProp = serializedObject.FindProperty("SwitchWeaponTypesDistance");
            SwitchWeaponTypesCooldownProp = serializedObject.FindProperty("SwitchWeaponTypesCooldown");
            MinResumeWanderProp = serializedObject.FindProperty("MinResumeWander");
            MaxResumeWanderProp = serializedObject.FindProperty("MaxResumeWander");

            //Float
            Type1AttackCooldownProp = serializedObject.FindProperty("Type1AttackCooldown");
            Type2AttackCooldownProp = serializedObject.FindProperty("Type2AttackCooldown");
        }

        void InitializeLists (EmeraldCombat self)
        {
            //Type 1 AttackTransforms
            WeaponType1AttackTransforms = new ReorderableList(serializedObject, serializedObject.FindProperty("WeaponType1AttackTransforms"), true, true, true, true);
            WeaponType1AttackTransforms.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Weapon Type 1 Attack Transforms List", EditorStyles.boldLabel);
            };
            WeaponType1AttackTransforms.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {

                    var element = WeaponType1AttackTransforms.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + 3f, rect.width, EditorGUIUtility.singleLineHeight), element, new GUIContent("Attack Transform " + (index + 1), AttackTransformTooltip));
                };

            //Type 2 AttackTransforms
            WeaponType2AttackTransforms = new ReorderableList(serializedObject, serializedObject.FindProperty("WeaponType2AttackTransforms"), true, true, true, true);
            WeaponType2AttackTransforms.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Weapon Type 2 Attack Transforms List", EditorStyles.boldLabel);
            };
            WeaponType2AttackTransforms.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {

                    var element = WeaponType2AttackTransforms.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y + 3f, rect.width, EditorGUIUtility.singleLineHeight), element, new GUIContent("Attack Transform " + (index + 1), AttackTransformTooltip));
                };


            DrawType1Attacks(self);
            DrawType2Attacks(self);

            //Type 1 Action List
            Type1ActionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1CombatActions"), true, true, true, true);
            Type1ActionsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = Type1ActionsList.serializedProperty.GetArrayElementAtIndex(index);
                    Type1ActionsList.elementHeight = EditorGUIUtility.singleLineHeight * 1.35f;

                    EditorGUI.PropertyField(new Rect(rect.x + 5f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Enabled"), GUIContent.none); //Toggle
                    EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Enabled", "Controls whether or not this action is enabled. If disabled, it will be ignored.")); //Toggle
                    EditorGUI.BeginDisabledGroup(!element.FindPropertyRelative("Enabled").boolValue);
                    EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y + 4, rect.width - 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("emeraldAction"), GUIContent.none); //Action Object
                    EditorGUI.EndDisabledGroup();
                };

            Type1ActionsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Type 1 Combat Actions List", EditorStyles.boldLabel);
            };

            //Type 2 Action List
            Type2ActionsList = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2CombatActions"), true, true, true, true);
            Type2ActionsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = Type2ActionsList.serializedProperty.GetArrayElementAtIndex(index);
                    Type2ActionsList.elementHeight = EditorGUIUtility.singleLineHeight * 1.35f;

                    EditorGUI.PropertyField(new Rect(rect.x + 5f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Enabled"), GUIContent.none); //Toggle
                    EditorGUI.LabelField(new Rect(rect.x + 25f, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Enabled", "Controls whether or not this action is enabled. If disabled, it will be ignored.")); //Toggle
                    EditorGUI.BeginDisabledGroup(!element.FindPropertyRelative("Enabled").boolValue);
                    EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y + 4, rect.width - 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("emeraldAction"), GUIContent.none); //Action Object
                    EditorGUI.EndDisabledGroup();
                };

            Type2ActionsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Type 2 Combat Actions List", EditorStyles.boldLabel);
            };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldCombat self = (EmeraldCombat)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Combat", CombatEditorIcon, new GUIContent(), HideSettingsFoldout);

            if (EmeraldBehaviors.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive)
            {
                CustomEditorProperties.DisplayImportantHeaderMessage("Only AI with an Aggressive behavior type use the Combat Component.");
            }

            EditorGUI.BeginDisabledGroup(EmeraldBehaviors.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive);
            DisplayWarningMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DamageSettings(self);
                EditorGUILayout.Space();
                CombatActionSettings(self);
                EditorGUILayout.Space();
                if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
                {
                    SwitchWeaponSettings(self);
                    EditorGUILayout.Space();
                }

                WeaponType1Settings(self);
                EditorGUILayout.Space();

                if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
                {
                    WeaponType2Settings(self);
                    EditorGUILayout.Space();
                }
            }
            EditorGUI.EndDisabledGroup();

            CustomEditorProperties.EndScriptHeader();
            serializedObject.ApplyModifiedProperties();
        }

        void DamageSettings(EmeraldCombat self)
        {
            DamageSettingsFoldout.boolValue = EditorGUILayout.Foldout(DamageSettingsFoldout.boolValue, "Combat Settings", true, FoldoutStyle);

            if (DamageSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Combat Settings", "Controls various combat related settings.", true);

                self.WeaponTypeAmount = (EmeraldCombat.WeaponTypeAmounts)EditorGUILayout.EnumPopup("Weapon Type Amount", self.WeaponTypeAmount);
                CustomEditorProperties.CustomHelpLabelField("Controls whether this AI uses 1 or 2 Weapon Types. This allows an AI to use different animations and abilities for its different weapon types, " +
                    "such as sword attacks for type 1 and shooting spells for type 2. By default, this is set to 1. Combat Actions are used for both weapon types, if desired.", true);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MinResumeWanderProp, "Min Resume Wandering", 0, 6);
                CustomEditorProperties.CustomHelpLabelField("Controls the minimum amount of time an AI will wait before they resume wandering (according to its Wandering Type) after being " +
                    "in combat. This amount will be randomized with the Maximum Resume Wandering Delay.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), MaxResumeWanderProp, "Max Resume Wandering", 0, 6);
                CustomEditorProperties.CustomHelpLabelField("Controls the maximum amount of time an AI will wait before they resume wandering (according to its Wandering Type) after being  " +
                    "in combat. This amount will be randomized with the Minimum Resume Wandering Delay.", true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void DisplayWarningMessage (EmeraldCombat self)
        {
            if (EmeraldBehaviors.CurrentBehaviorType != EmeraldBehaviors.BehaviorTypes.Aggressive) return;

            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two && self.Type2Attacks.AttackDataList.Count == 0)
            {
                CustomEditorProperties.DisplaySetupWarning("There currently aren'transform any Type 2 attacks applied to this AI. Please ensure there is at least 1 attack applied to the Type 2 Attacks List. This " +
                    "can be found within the Weapon Type 2 Settings foldout");
            }
            else if (self.Type1Attacks.AttackDataList.Count == 0)
            {
                CustomEditorProperties.DisplaySetupWarning("There currently aren'transform any Type 1 attacks applied to this AI. Please ensure there is at least 1 attack applied to the Type 1 Attacks List. This " +
                    "can be found within the Weapon Type 1 Settings foldout.");
            }
        }

        void CombatActionSettings(EmeraldCombat self)
        {
            CombatActionSettingsFoldout.boolValue = EditorGUILayout.Foldout(CombatActionSettingsFoldout.boolValue, "Combat Actions Settings", true, FoldoutStyle);

            if (CombatActionSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Combat Actions Settings", "Controls the combat actions an AI will use while in combat. Combat Actions that are disabled will be ignored. Only Aggressive AI can use Combat Actions.", true);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("A list of mudular combat actions an AI can use while actively fighting in combat using its Type 1 Weapon Type.", false);
                GUILayout.Box(new GUIContent("What's This?", "A list of mudular combat actions an AI can use while actively fighting in combat using its Type 1 Weapon Type."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                Type1ActionsList.DoLayoutList();
                EditorGUILayout.Space();
                EditorGUILayout.Space();

                if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
                {
                    EditorGUILayout.Space();
                    CustomEditorProperties.CustomHelpLabelField("A list of mudular combat actions an AI can use while actively fighting in combat using its Type 2 Weapon Type.", false);
                    GUILayout.Box(new GUIContent("What's This?", "A list of mudular combat actions an AI can use while actively fighting in combat using its Type 2 Weapon Type."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    Type2ActionsList.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }    

        void SwitchWeaponSettings (EmeraldCombat self)
        {
            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                SwitchWeaponSettingsFoldout.boolValue = EditorGUILayout.Foldout(SwitchWeaponSettingsFoldout.boolValue, "Switch Weapon Settings", true, FoldoutStyle);

                if (SwitchWeaponSettingsFoldout.boolValue)
                {
                    CustomEditorProperties.BeginFoldoutWindowBox();
                    CustomEditorProperties.TextTitleWithDescription("Switch Weapon Settings", "Switch Weapon Settings.", true);

                    EditorGUILayout.PropertyField(StartingWeaponTypeProp, new GUIContent("Starting Weapon Type"));
                    CustomEditorProperties.CustomHelpLabelField("Controls which weapon type the AI will start with, or transition to first, upon entering comabt.", true);

                    if (self.SwitchWeaponType == EmeraldCombat.SwitchWeaponTypes.Distance)
                    {
                        CustomEditorProperties.DisplayImportantMessage("Important - When using the Distance Switch Type, your Starting Weapon Type needs to be the Weapon Type used for ranged combat. This setting is intended to be used with an AI that has close-range and range weapons.");
                        GUILayout.Space(10);
                    }

                    EditorGUILayout.PropertyField(SwitchWeaponTypeProp, new GUIContent("Switch Type"));
                    CustomEditorProperties.CustomHelpLabelField("Controls how the AI will switch its weapon type between Type 2 and Type 1. If none is used, the AI will always stay on the Starting Weapon Type.", true);

                    if (self.SwitchWeaponType == EmeraldCombat.SwitchWeaponTypes.Timed)
                    {
                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTimeMinProp, "Switch Time Min", 5, 45);
                        CustomEditorProperties.CustomHelpLabelField("Controls the minimum amount of time it takes for this AI to switch its weapon.", false);

                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTimeMaxProp, "Switch Time Min", 10, 90);
                        CustomEditorProperties.CustomHelpLabelField("Controls the minimum amount of time it takes for this AI to switch its weapon.", true);
                    }
                    else if (self.SwitchWeaponType == EmeraldCombat.SwitchWeaponTypes.Distance)
                    {
                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTypesDistanceProp, "Switch Weapon Type Distance", 2, 15);
                        CustomEditorProperties.CustomHelpLabelField("Controls the distance in which the AI will switch to between close-ranged and ranged damage. Any distance at or below this amount will be close-ranged" +
                            " and any value greater will be ranged.", true);

                        CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), SwitchWeaponTypesCooldownProp, "Switch Weapon Type Cooldown", 1, 60);
                        CustomEditorProperties.CustomHelpLabelField("Controls the cooldown in which an AI will switch between ranged and close-ranged combat, if the Switch Weapon Type Distance has been met. This" +
                            " is to stop a weapon switch from happening too often.", true);
                    }

                    CustomEditorProperties.EndFoldoutWindowBox();
                }
            }
        }

        void WeaponType1Settings(EmeraldCombat self)
        {
            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.One || self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                WeaponType1SettingsFoldout.boolValue = EditorGUILayout.Foldout(WeaponType1SettingsFoldout.boolValue, "Weapon Type 1 Settings", true, FoldoutStyle);

                if (WeaponType1SettingsFoldout.boolValue)
                {
                    CustomEditorProperties.BeginFoldoutWindowBox();
                    CustomEditorProperties.TextTitleWithDescription("Weapon Type 1 Settings", "Weapon Type 1 Settings.", true);

                    PickTargetTypeSetting(Type1PickTargetTypeProp);

                    CustomEditorProperties.CustomFloatSliderPropertyField(Type1AttackCooldownProp, "Type 1 Attack Cooldown", "Controls the cooldown needed to trigger an attack. " +
                        "Note: An attack can take longer than the specified cooldown if an AI is busy with another action.", 0.35f, 5, false);
                    GUILayout.Space(12);

                    GUILayout.Box(new GUIContent("What's This?", AttackTransformTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    WeaponType1AttackTransforms.DoLayoutList();
                    GUILayout.Space(12);

                    CustomEditorProperties.CustomHelpLabelField("Controls how Type 1's Attacks are picked.", false);
                    CustomEditorProperties.CustomPropertyField(Type1AttackPickTypeProp, "Attack Pick Type", "", false);

                    if (self.Type1Attacks.AttackPickType == AttackPickTypes.Odds)
                    {
                        CustomEditorProperties.CustomHelpLabelField("Odds - Type 1 Attacks are picked based off of each of the Type 1 Attack's odds.", true);
                    }
                    else if (self.Type1Attacks.AttackPickType == AttackPickTypes.Order)
                    {
                        CustomEditorProperties.CustomHelpLabelField("Order - Type 1 Attacks are picked based on the order of the AI's Type 1 Attacks list.", true);
                    }
                    else if (self.Type1Attacks.AttackPickType == AttackPickTypes.Random)
                    {
                        CustomEditorProperties.CustomHelpLabelField("Random - Type 1 Attacks are picked randomly from the AI's Type 1 Attacks list.", true);
                    }
                    GUILayout.Space(10);

                    if (EmeraldAnimation.m_AnimationProfile != null && EmeraldAnimation.m_AnimationProfile.Type1Animations.AttackList.Count == 0 || EmeraldAnimation.Type1AttackEnumAnimations == null)
                    {
                        CustomEditorProperties.BeginIndent(12);
                        CustomEditorProperties.DisplaySetupWarning("Please add at least one Type 1 Attack animation to the Type 1 Attack Animation list (located within this AI's Animation Profile) to " +
                            "choose the type of animations these attacks will use.");
                        CustomEditorProperties.EndIndent();
                    }

                    GUILayout.Box(new GUIContent("What's This?", "A list of an AI's attacks. You can hover the mouse over each setting to view its tooltip. " +
                        "You can select each attack within the attack list (making it active) to see the attack distance drawn around the AI."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    Type1Attacks.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    CustomEditorProperties.EndFoldoutWindowBox();
                }
            }
        }

        void WeaponType2Settings (EmeraldCombat self)
        {
            if (self.WeaponTypeAmount == EmeraldCombat.WeaponTypeAmounts.Two)
            {
                WeaponType2SettingsFoldout.boolValue = EditorGUILayout.Foldout(WeaponType2SettingsFoldout.boolValue, "Weapon Type 2 Settings", true, FoldoutStyle);

                if (WeaponType2SettingsFoldout.boolValue)
                {
                    EditorGUILayout.BeginVertical("Box");
                    CustomEditorProperties.TextTitleWithDescription("Weapon Type 2 Settings", "Weapon Type 2 Settings.", true);

                    PickTargetTypeSetting(Type2PickTargetTypeProp);

                    CustomEditorProperties.CustomFloatSliderPropertyField(Type2AttackCooldownProp, "Type 2 Attack Cooldown", "Controls the cooldown needed to trigger an attack. " +
                        "Note: An attack can take longer than the specified cooldown if an AI is busy with another action.", 0.35f, 5, false);
                    GUILayout.Space(12);

                    GUILayout.Box(new GUIContent("What's This?", AttackTransformTooltip), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    WeaponType2AttackTransforms.DoLayoutList();
                    GUILayout.Space(12);

                    CustomEditorProperties.CustomHelpLabelField("Controls how Type 2's Attacks are picked.", false);
                    CustomEditorProperties.CustomPropertyField(Type2AttackPickTypeProp, "Attack Pick Type", "", false);

                    if (self.Type2Attacks.AttackPickType == AttackPickTypes.Odds)
                    {
                        CustomEditorProperties.CustomHelpLabelField("Odds - Type 2 Attacks are picked based off of each of the Type 2 Attack's odds.", true);
                    }
                    else if (self.Type2Attacks.AttackPickType == AttackPickTypes.Order)
                    {
                        CustomEditorProperties.CustomHelpLabelField("Order - Type 2 Attacks are picked based on the order of the AI's Type 2 Attacks list.", true);
                    }
                    else if (self.Type2Attacks.AttackPickType == AttackPickTypes.Random)
                    {
                        CustomEditorProperties.CustomHelpLabelField("Random - Type 2 Attacks are picked randomly from the AI's Type 2 Attacks list.", true);
                    }
                    GUILayout.Space(10);

                    if (EmeraldAnimation.m_AnimationProfile != null && EmeraldAnimation.m_AnimationProfile.Type2Animations.AttackList.Count == 0 || EmeraldAnimation.Type2AttackEnumAnimations == null)
                    {
                        CustomEditorProperties.BeginIndent(12);
                        CustomEditorProperties.DisplaySetupWarning("Please add at least one Type 2 Attack animation to the Type 2 Attack Animation list (located within this AI's Animation Profile) to " +
                            "choose the type of animations these attacks will use.");
                        CustomEditorProperties.EndIndent();
                    }

                    GUILayout.Box(new GUIContent("What's This?", "A list of an AI's attacks. You can hover the mouse over each setting to view its tooltip. " +
                        "You can select each attack within the attack list (making it active) to see the attack distance drawn around the AI."), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));
                    Type2Attacks.DoLayoutList();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.EndVertical();
                }
            }
        }

        void PickTargetTypeSetting (SerializedProperty PickTargetTypeProp)
        {
            EditorGUILayout.PropertyField(PickTargetTypeProp);
            CustomEditorProperties.CustomHelpLabelField("Controls the method an AI uses to pick a target.", false);

            if ((PickTargetTypes)PickTargetTypeProp.enumValueIndex == PickTargetTypes.Closest)
            {
                CustomEditorProperties.CustomHelpLabelField("Closest - Picks the tagret that is closest and currently visible to the AI.", true);
            }
            else if ((PickTargetTypes)PickTargetTypeProp.enumValueIndex == PickTargetTypes.FirstDetected)
            {
                CustomEditorProperties.CustomHelpLabelField("First Detected - Picks the tagret that was first detected and currently visible to the AI.", true);
            }
            else if ((PickTargetTypes)PickTargetTypeProp.enumValueIndex == PickTargetTypes.Random)
            {
                CustomEditorProperties.CustomHelpLabelField("Random - Picks a random target from all currently visible targets within an AI's detection radius.", true);
            }
        }

        private void OnSceneGUI()
        {
            EmeraldCombat self = (EmeraldCombat)target;
            DrawCombatRadii(self);
        }

        void DrawType1Attacks(EmeraldCombat self)
        {
            //Type 1 Attacks
            Type1Attacks = new ReorderableList(serializedObject, serializedObject.FindProperty("Type1Attacks").FindPropertyRelative("AttackDataList"), true, true, true, true);

            Type1Attacks.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Type 1 Attacks", EditorStyles.boldLabel);
            };

            Type1Attacks.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = Type1Attacks.serializedProperty.GetArrayElementAtIndex(index);

                    if (self.Type1Attacks.AttackDataList[index].AbilityObject != null && self.Type1Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    {
                        Rect helpBoxRect = new Rect(rect.x, rect.y + 133, rect.width, EditorGUIUtility.singleLineHeight * 1.5f);
                        GUIContent content = new GUIContent(" Condition Module Active (See tooltip)", EditorGUIUtility.IconContent("console.warnicon").image, "Abilities that use a Condition Module will need to have their condition met in order to be triggered. " +
                            "Conditions that are High Priorty will ignore an AI's Pick Type and be picked first, given the condition is met.\n\nNote: If the condition is not met, this ability will be skipped.");
                        EditorGUI.LabelField(helpBoxRect, content, EditorStyles.label);
                    }

                    if (self.Type1Attacks.AttackDataList.Count > 0 && EmeraldAnimation.Type1AttackEnumAnimations != null)
                    {
                        CustomEditorProperties.CustomListPopup(new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight), new GUIContent(), element.FindPropertyRelative("AttackAnimation"), "Attack Animation", EmeraldAnimation.Type1AttackEnumAnimations);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Animation", "The animation that will be used for this attack.\n\nNote: Animations are based off of your AI's Attack Animation List within its Animation Profile."));
                    }
                    else
                    {
                        EditorGUI.Popup(new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight), 0, EmeraldAnimation.Type1AttackBlankOptions);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Animation"));
                    }

                    if (isActive)
                    {
                        CurrentAttackDistance = element.FindPropertyRelative("AttackDistance").floatValue;
                        CurrentTooCloseDistance = element.FindPropertyRelative("TooCloseDistance").floatValue;
                        DrawDistanceActive = true;
                    }

                    if (element.FindPropertyRelative("AttackDistance").floatValue == 0) element.FindPropertyRelative("AttackDistance").floatValue = 2;
                    if (element.FindPropertyRelative("TooCloseDistance").floatValue == 0) element.FindPropertyRelative("TooCloseDistance").floatValue = 0.5f;

                    EditorGUI.ObjectField(new Rect(rect.x, rect.y + 35, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AbilityObject"), new GUIContent("Ability Object", "The Ability Object that will be used for this attack."));
                    CustomEditorProperties.CustomListFloatSlider(new Rect(rect.x, rect.y + 60, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Distance", "Controls the distance that this attack can happen within."), element.FindPropertyRelative("AttackDistance"), 0.5f, 75f);
                    CustomEditorProperties.CustomListFloatSlider(new Rect(rect.x, rect.y + 85, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Too Close Distance", "Controls the distance for when an AI will backup. This is useful for AI keeping their distance from attackers."), element.FindPropertyRelative("TooCloseDistance"), 0f, 35f);
                    EditorGUI.BeginDisabledGroup(self.Type1Attacks.AttackPickType != AttackPickTypes.Odds);
                    CustomEditorProperties.CustomListIntSlider(new Rect(rect.x, rect.y + 110, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Odds", "The odds that this attack will be used (when using the Odds Pick type)."), element.FindPropertyRelative("AttackOdds"), 1, 100);
                    EditorGUI.EndDisabledGroup();
                };

            //Modify the heights of each element to create a cleaner reorderable list. This allows the element with a condition module active to have room to display the icon and info.
            Type1Attacks.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = Type1Attacks.serializedProperty.GetArrayElementAtIndex(index);
                float height = EditorGUIUtility.singleLineHeight * 7.5f;

                if (self.Type1Attacks.AttackDataList[index].AbilityObject && self.Type1Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    height += 26f;

                return height;
            };

            Type1Attacks.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Type 1 Attacks", EditorStyles.boldLabel);
            };
        }

        void DrawType2Attacks(EmeraldCombat self)
        {
            //Type 2 Attacks
            Type2Attacks = new ReorderableList(serializedObject, serializedObject.FindProperty("Type2Attacks").FindPropertyRelative("AttackDataList"), true, true, true, true);

            Type2Attacks.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Type 2 Attacks", EditorStyles.boldLabel);
            };

            Type2Attacks.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) => {
                    var element = Type2Attacks.serializedProperty.GetArrayElementAtIndex(index);

                    if (self.Type2Attacks.AttackDataList[index].AbilityObject != null && self.Type2Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    {
                        Rect helpBoxRect = new Rect(rect.x, rect.y + 133, rect.width, EditorGUIUtility.singleLineHeight * 1.5f);
                        GUIContent content = new GUIContent(" Condition Module Active (See tooltip)", EditorGUIUtility.IconContent("console.warnicon").image, "Abilities that use a Condition Module will need to have their condition met in order to be triggered. " +
                            "Conditions that are High Priorty will ignore an AI's Pick Type and be picked first, given the condition is met.\n\nNote: If the condition is not met, this ability will be skipped.");
                        EditorGUI.LabelField(helpBoxRect, content, EditorStyles.label);
                    }

                    if (self.Type2Attacks.AttackDataList.Count > 0 && EmeraldAnimation.Type2AttackEnumAnimations != null)
                    {
                        CustomEditorProperties.CustomListPopup(new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight), new GUIContent(), element.FindPropertyRelative("AttackAnimation"), "Attack Animation", EmeraldAnimation.Type2AttackEnumAnimations);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Animation", "The animation that will be used for this attack.\n\nNote: Animations are based off of your AI's Attack Animation List within its Animation Profile."));
                    }
                    else
                    {
                        EditorGUI.Popup(new Rect(rect.x + 125, rect.y + 10, rect.width - 125, EditorGUIUtility.singleLineHeight), 0, EmeraldAnimation.Type1AttackBlankOptions);
                        EditorGUI.PrefixLabel(new Rect(rect.x, rect.y + 10, 125, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Animation"));
                    }

                    if (isActive)
                    {
                        CurrentAttackDistance = element.FindPropertyRelative("AttackDistance").floatValue;
                        CurrentTooCloseDistance = element.FindPropertyRelative("TooCloseDistance").floatValue;
                        DrawDistanceActive = true;
                    }

                    EditorGUI.ObjectField(new Rect(rect.x, rect.y + 35, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("AbilityObject"), new GUIContent("Ability Object", "The Ability Object that will be used for this attack."));
                    CustomEditorProperties.CustomListFloatSlider(new Rect(rect.x, rect.y + 60, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Distance", "Controls the distance that this attack can happen within"), element.FindPropertyRelative("AttackDistance"), 0.5f, 75f);
                    CustomEditorProperties.CustomListFloatSlider(new Rect(rect.x, rect.y + 85, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Too Close Distance", "Controls the distance for when an AI will backup. This is useful for AI keeping their distance from attackers."), element.FindPropertyRelative("TooCloseDistance"), 0f, 35f);
                    EditorGUI.BeginDisabledGroup(self.Type2Attacks.AttackPickType != AttackPickTypes.Odds);
                    CustomEditorProperties.CustomListIntSlider(new Rect(rect.x, rect.y + 110, rect.width, EditorGUIUtility.singleLineHeight), new GUIContent("Attack Odds", "The odds that this attack will be used (when using the Odds Pick type)."), element.FindPropertyRelative("AttackOdds"), 1, 100);
                    EditorGUI.EndDisabledGroup();
                };

            //Modify the heights of each element to create a cleaner reorderable list. This allows the element with a condition module active to have room to display the icon and info.
            Type2Attacks.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = Type2Attacks.serializedProperty.GetArrayElementAtIndex(index);
                float height = EditorGUIUtility.singleLineHeight * 7.5f;

                if (self.Type2Attacks.AttackDataList[index].AbilityObject && self.Type2Attacks.AttackDataList[index].AbilityObject.ConditionSettings.Enabled)
                    height += 26f;

                return height;
            };

            Type2Attacks.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Type 2 Attacks", EditorStyles.boldLabel);
            };
        }

        void DrawCombatRadii (EmeraldCombat self)
        {
            if (DrawDistanceActive)
            {
                Handles.color = new Color(255, 0, 0, 1.0f);
                Handles.DrawWireDisc(self.transform.position, Vector3.up, CurrentAttackDistance);
                Handles.color = new Color(1, 0.9f, 0, 1.0f);
                Handles.DrawWireDisc(self.transform.position, Vector3.up, CurrentTooCloseDistance);
            }
        }
    }
}