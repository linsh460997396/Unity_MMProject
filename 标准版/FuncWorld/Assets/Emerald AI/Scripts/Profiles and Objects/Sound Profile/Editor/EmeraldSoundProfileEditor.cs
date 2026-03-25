using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldSoundProfile))]
    [CanEditMultipleObjects]
    public class EmeraldSoundProfileEditor : Editor
    {
        GUIStyle HelpButtonStyle;
        GUIStyle FoldoutStyle;
        Texture SoundsEditorIcon;

        #region SerializedProperties
        //Ints
        SerializedProperty IdleSoundsSecondsMinProp, IdleSoundsSecondsMaxProp;

        //Floats
        SerializedProperty WalkFootstepVolumeProp, RunFootstepVolumeProp, BlockVolumeProp, AttackVolumeProp, DeathVolumeProp, IdleVolumeProp, InjuredVolumeProp, InjuredSoundOddsProp,
            EquipVolumeProp, UnequipVolumeProp, WarningVolumeProp, RangedEquipVolumeProp, RangedUnequipVolumeProp;

        //Objects
        SerializedProperty SheatheWeaponProp, UnsheatheWeaponProp, RangedSheatheWeaponProp, RangedUnsheatheWeaponProp;

        //Bools
        SerializedProperty IdleSoundsFoldout, FootstepSoundsFoldout, InteractSoundsFoldout, EquipAndUnequipSoundsFoldout, AttackSoundsFoldout, InjuredSoundsFoldout, BlockSoundsFoldout, WarningSoundsFoldout, DeathSoundsFoldout;

        //List
        SerializedProperty AttackSoundsProp, InjuredSoundsProp, WarningSoundsProp, DeathSoundsProp, FootStepSoundsProp, IdleSoundsProp, BlockingSoundsProp;

        //Reorderable List
        ReorderableList InteractSoundsList;
        #endregion

        void OnEnable()
        {
            if (SoundsEditorIcon == null) SoundsEditorIcon = Resources.Load("Editor Icons/EmeraldSounds") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            //Ints
            IdleSoundsSecondsMinProp = serializedObject.FindProperty("IdleSoundsSecondsMin");
            IdleSoundsSecondsMaxProp = serializedObject.FindProperty("IdleSoundsSecondsMax");

            //Floats
            WalkFootstepVolumeProp = serializedObject.FindProperty("WalkFootstepVolume");
            RunFootstepVolumeProp = serializedObject.FindProperty("RunFootstepVolume");
            BlockVolumeProp = serializedObject.FindProperty("BlockVolume");
            InjuredVolumeProp = serializedObject.FindProperty("InjuredVolume");
            InjuredSoundOddsProp = serializedObject.FindProperty("InjuredSoundOdds");
            AttackVolumeProp = serializedObject.FindProperty("AttackVolume");
            DeathVolumeProp = serializedObject.FindProperty("DeathVolume");
            EquipVolumeProp = serializedObject.FindProperty("EquipVolume");
            UnequipVolumeProp = serializedObject.FindProperty("UnequipVolume");
            RangedEquipVolumeProp = serializedObject.FindProperty("RangedEquipVolume");
            RangedUnequipVolumeProp = serializedObject.FindProperty("RangedUnequipVolume");
            IdleVolumeProp = serializedObject.FindProperty("IdleVolume");
            WarningVolumeProp = serializedObject.FindProperty("WarningVolume");

            //Bools
            IdleSoundsFoldout = serializedObject.FindProperty("IdleSoundsFoldout");
            FootstepSoundsFoldout = serializedObject.FindProperty("FootstepSoundsFoldout");
            InteractSoundsFoldout = serializedObject.FindProperty("InteractSoundsFoldout");
            EquipAndUnequipSoundsFoldout = serializedObject.FindProperty("EquipAndUnequipSoundsFoldout");
            AttackSoundsFoldout = serializedObject.FindProperty("AttackSoundsFoldout");
            InjuredSoundsFoldout = serializedObject.FindProperty("InjuredSoundsFoldout");
            BlockSoundsFoldout = serializedObject.FindProperty("BlockSoundsFoldout");
            WarningSoundsFoldout = serializedObject.FindProperty("WarningSoundsFoldout");
            DeathSoundsFoldout = serializedObject.FindProperty("DeathSoundsFoldout");

            //Objects
            SheatheWeaponProp = serializedObject.FindProperty("SheatheWeapon");
            UnsheatheWeaponProp = serializedObject.FindProperty("UnsheatheWeapon");
            RangedSheatheWeaponProp = serializedObject.FindProperty("RangedSheatheWeapon");
            RangedUnsheatheWeaponProp = serializedObject.FindProperty("RangedUnsheatheWeapon");

            //Lists
            AttackSoundsProp = serializedObject.FindProperty("AttackSounds");
            InjuredSoundsProp = serializedObject.FindProperty("InjuredSounds");
            WarningSoundsProp = serializedObject.FindProperty("WarningSounds");
            DeathSoundsProp = serializedObject.FindProperty("DeathSounds");
            FootStepSoundsProp = serializedObject.FindProperty("FootStepSounds");
            IdleSoundsProp = serializedObject.FindProperty("IdleSounds");
            BlockingSoundsProp = serializedObject.FindProperty("BlockingSounds");

            InitializeInteractSoundsList();
        }

        void InitializeInteractSoundsList()
        {
            //Interact Sounds
            InteractSoundsList = new ReorderableList(serializedObject, serializedObject.FindProperty("InteractSounds"), true, true, true, true);
            InteractSoundsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var element = InteractSoundsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("SoundEffectClip"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("SoundEffectID"), GUIContent.none);
                };

            InteractSoundsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "   ID  " + "         Interact Sound Clip", EditorStyles.boldLabel);
            };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldSoundProfile self = (EmeraldSoundProfile)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeader("Sound Profile", SoundsEditorIcon);

            EditorGUILayout.Space();
            IdleSounds(self);
            EditorGUILayout.Space();
            FootstepSounds(self);
            EditorGUILayout.Space();
            InteractSounds(self);
            EditorGUILayout.Space();
            EquipAndUnequipSounds(self);
            EditorGUILayout.Space();
            AttackSounds(self);
            EditorGUILayout.Space();
            InjuredSounds(self);
            EditorGUILayout.Space();
            BlockSounds(self);
            EditorGUILayout.Space();
            DeathSounds(self);
            EditorGUILayout.Space();
            WarningSounds(self);
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();

            CustomEditorProperties.EndScriptHeader();
        }

        void IdleSounds(EmeraldSoundProfile self)
        {
            IdleSoundsFoldout.boolValue = EditorGUILayout.Foldout(IdleSoundsFoldout.boolValue, "Idle Sounds", true, FoldoutStyle);

            if (IdleSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Idle Sounds", "Idle sounds will play randomly based on the Min and Max Idle Sound Seconds when not in combat.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), IdleVolumeProp, "Idle Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of idle sounds.", true);

                CustomEditorProperties.CustomListPropertyField(IdleSoundsProp, "Idle Sounds", "Controls how many idle sounds this AI will use.", true);

                if (self.IdleSounds.Count != 0)
                {
                    EditorGUILayout.Space();
                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), IdleSoundsSecondsMinProp, "Min Idle Sound Seconds");
                    CustomEditorProperties.CustomHelpLabelField("Controls the minimum amount of seconds needed before playing a random idle sound from the list below. This amuont will be " +
                        "randomized with the Max Idle Sound Seconds.", true);

                    CustomEditorProperties.CustomIntField(new Rect(), new GUIContent(), IdleSoundsSecondsMaxProp, "Max Idle Sound Seconds");
                    CustomEditorProperties.CustomHelpLabelField("Controls the maximum amount of seconds needed before playing a random idle sound from the list below. This amuont will be " +
                        "randomized with the Min Idle Sound Seconds.", true);
                }
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void FootstepSounds(EmeraldSoundProfile self)
        {
            FootstepSoundsFoldout.boolValue = EditorGUILayout.Foldout(FootstepSoundsFoldout.boolValue, "Footstep Sounds", true, FoldoutStyle);

            if (FootstepSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Footstep Sounds", "Controls the sounds and settings an AI will use when playing footstep sounds. The must be added through Animation Events. See the button below for a tutorial.", true);

                CustomEditorProperties.TutorialButton("Note: You will need to manually create a WalkFootstepSound and/or RunFootstepSound Animation Event to use this feature. " +
                    "For a tutorial on this, please press the 'See Tutorial' button below.", "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-footstep-sounds");

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WalkFootstepVolumeProp, "Walk Footsteps Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of walk footstep sounds.", false);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RunFootstepVolumeProp, "Run Footsteps Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of Run footstep sounds.", true);

                EditorGUILayout.Space();
                CustomEditorProperties.CustomListPropertyField(FootStepSoundsProp, "Footstep Sounds", "Controls how many footstep sounds this AI will use.", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void InteractSounds(EmeraldSoundProfile self)
        {
            InteractSoundsFoldout.boolValue = EditorGUILayout.Foldout(InteractSoundsFoldout.boolValue, "Interact Sounds", true, FoldoutStyle);

            if (InteractSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Interact Sounds", "Various sounds that can be called through Animation Events or programmatically using the sound effect ID. " +
                    "This can be useful for quests, dialouge, animation sound effects, etc.", true);

                InteractSoundsList.DoLayoutList();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void EquipAndUnequipSounds(EmeraldSoundProfile self)
        {
            EquipAndUnequipSoundsFoldout.boolValue = EditorGUILayout.Foldout(EquipAndUnequipSoundsFoldout.boolValue, "Equip & Unequip Sounds", true, FoldoutStyle);

            if (EquipAndUnequipSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Equip & Unequip Sounds", "Controls the sounds that play when the AI is equiping or unequiping their weapon.", true);

                CustomEditorProperties.TutorialButton("Note: This is automatically called through the EquipWeapon and UnequipWeapon Animation Events. For a tutorial on this, please press the 'See Tutorial' button below.",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/items-component/creating-equippable-weapons");

                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), EquipVolumeProp, "Type 1 Equip Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of the Equip Sound.", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), UnsheatheWeaponProp, "Type 1 Equip Sound", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("The sound that plays when this AI is equiping their weapon.", true);
                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), UnequipVolumeProp, "Type 1 Unequip Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of the Unequip Sound.", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), SheatheWeaponProp, "Type 1 Unequip Sound", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("The sound that plays when this AI is unequiping their weapon.", true);
                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RangedEquipVolumeProp, "Type 2 Equip Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of the Equip Sound.", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), RangedUnsheatheWeaponProp, "Type 2 Equip Sound", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("The sound that plays when this AI is equiping their weapon.", true);
                EditorGUILayout.Space();
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), RangedUnequipVolumeProp, "Type 2 Unequip Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of the Unequip Sound.", false);
                CustomEditorProperties.CustomObjectField(new Rect(), new GUIContent(), RangedSheatheWeaponProp, "Type 2 Unequip Sound", typeof(AudioClip), false);
                CustomEditorProperties.CustomHelpLabelField("The sound that plays when this AI is unequiping their weapon.", true);
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void AttackSounds(EmeraldSoundProfile self)
        {
            AttackSoundsFoldout.boolValue = EditorGUILayout.Foldout(AttackSoundsFoldout.boolValue, "Attack Sounds", true, FoldoutStyle);

            if (AttackSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Attack Sounds", "Controls the sounds (roars, grunts, shouts, growls, etc) that play when an AI triggers an attack. These must be added through Animation Events. See the button below for a tutorial.", true);

                CustomEditorProperties.TutorialButton("Note: Attack Sounds are used through Animation Events. For a tutorial on this, please press the 'See Tutorial' button below.",
                    "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-attack-sounds");

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttackVolumeProp, "Attack Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of Attack Sounds.", true);

                CustomEditorProperties.CustomListPropertyField(AttackSoundsProp, "Attack Sounds", "Controls how many attack sounds this AI will use.", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void InjuredSounds(EmeraldSoundProfile self)
        {
            InjuredSoundsFoldout.boolValue = EditorGUILayout.Foldout(InjuredSoundsFoldout.boolValue, "Injured Sounds", true, FoldoutStyle);

            if (InjuredSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Injured Sounds", "Controls the sounds that play when an AI receives damage (grunts, growls, etc). These are different than impact sounds (which are played through an Ability Object).", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), InjuredSoundOddsProp, "Injured Sound Odds", 1, 100);
                CustomEditorProperties.CustomHelpLabelField("Controls the odds of Injured Sounds playing (as they may not need to be played every time an AI is injured).", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), InjuredVolumeProp, "Injured Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of Injured Sounds.", true);

                CustomEditorProperties.CustomListPropertyField(InjuredSoundsProp, "Injured Sounds", "Controls how many injured sounds this AI will use.", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void BlockSounds(EmeraldSoundProfile self)
        {
            BlockSoundsFoldout.boolValue = EditorGUILayout.Foldout(BlockSoundsFoldout.boolValue, "Block Sounds", true, FoldoutStyle);

            if (BlockSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Block Sounds", "Controls the sounds that play when an AI receives damage while blocking.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), BlockVolumeProp, "Block Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of Block Sounds.", true);

                CustomEditorProperties.CustomListPropertyField(BlockingSoundsProp, "Block Sounds", "Controls the sounds that happens when this AI is hit while blocking. " +
                    "Note: Blocking must be enabled with the proper animations setup in order for this to work.", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void WarningSounds(EmeraldSoundProfile self)
        {
            WarningSoundsFoldout.boolValue = EditorGUILayout.Foldout(WarningSoundsFoldout.boolValue, "Warning Sounds", true, FoldoutStyle);

            if (WarningSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Warning Sounds", "Controls the sounds that play when an AI receives damage.", true);

                CustomEditorProperties.TutorialButton("Note: Warning Sounds are only used by Cautious Behavior Types with a Confidence greater than Coward and are done through Animation Events. For a tutorial on this, " +
                    "please press the 'See Tutorial' button below.", "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-warning-sounds");

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WarningVolumeProp, "Warning Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of Warning Sounds.", true);

                CustomEditorProperties.CustomListPropertyField(WarningSoundsProp, "Warning Sounds", "Controls how many warning sounds this AI will use.", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void DeathSounds(EmeraldSoundProfile self)
        {
            DeathSoundsFoldout.boolValue = EditorGUILayout.Foldout(DeathSoundsFoldout.boolValue, "Death Sounds", true, FoldoutStyle);

            if (DeathSoundsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Death Sounds", "Controls the sounds that play when an AI dies.", true);

                CustomEditorProperties.TutorialButton("Note: Death Sounds are used through Animation Events. For a tutorial on this, please press the 'See Tutorial' button below.",
                "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-profiles-and-objects/sound-profile/setting-up-death-sounds");

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DeathVolumeProp, "Death Volume", 0, 1);
                CustomEditorProperties.CustomHelpLabelField("Controls the volume of Death Sounds.", true);

                CustomEditorProperties.CustomListPropertyField(DeathSoundsProp, "Death Sounds", "Controls how many death sounds this AI will use.", true);
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Draws a sound list based on the passed parameters.
        /// </summary>
        void DrawSoundList(ReorderableList ListRef, string DisplayName)
        {
            ListRef.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, DisplayName, EditorStyles.boldLabel);
            };
            ListRef.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = ListRef.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }
    }
}