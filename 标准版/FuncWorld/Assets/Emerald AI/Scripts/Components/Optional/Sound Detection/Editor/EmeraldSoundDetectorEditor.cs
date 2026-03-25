using UnityEngine;
using UnityEditor;
using EmeraldAI.Utility;

namespace EmeraldAI.SoundDetection.Utility
{
    [System.Serializable]
    [CustomEditor(typeof(EmeraldSoundDetector))]
    public class EmeraldSoundDetectorEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture SoundDetectionEditorIcon;

        SerializedProperty TargetVelocityFactorProp;
        SerializedProperty FalloffDealyProp;
        SerializedProperty DistanceFactorProp;
        SerializedProperty MinVelocityThresholdProp;
        SerializedProperty AttentionRateProp;
        SerializedProperty AttentionFalloffProp;
        SerializedProperty AttractModifierCooldownProp;
        SerializedProperty DelayUnawareSecondsProp;

        SerializedProperty UnawareReactionProp;
        SerializedProperty SuspiciousReactionProp;
        SerializedProperty AwareReactionProp;

        SerializedProperty UnawareThreatLevelProp;
        SerializedProperty SuspiciousThreatLevelProp;
        SerializedProperty AwareThreatLevelProp;

        SerializedProperty UnawareEventProp;
        SerializedProperty SuspiciousEventProp;
        SerializedProperty AwareEventProp;

        SerializedProperty HideSettingsFoldoutProp, SoundDetectorFoldoutProp, UnawareFoldoutProp, SuspiciousFoldoutProp, AwareFoldoutProp;

        private void OnEnable()
        {
            if (SoundDetectionEditorIcon == null) SoundDetectionEditorIcon = Resources.Load("Editor Icons/EmeraldSoundDetector") as Texture;

            TargetVelocityFactorProp = serializedObject.FindProperty("TargetVelocityFactor");
            FalloffDealyProp = serializedObject.FindProperty("FalloffDealy");
            DistanceFactorProp = serializedObject.FindProperty("DistanceFactor");
            MinVelocityThresholdProp = serializedObject.FindProperty("MinVelocityThreshold");
            AttentionRateProp = serializedObject.FindProperty("AttentionRate");
            AttentionFalloffProp = serializedObject.FindProperty("AttentionFalloff");
            DelayUnawareSecondsProp = serializedObject.FindProperty("DelayUnawareSeconds");
            AttractModifierCooldownProp = serializedObject.FindProperty("AttractModifierCooldown");

            UnawareThreatLevelProp = serializedObject.FindProperty("UnawareThreatLevel");
            SuspiciousThreatLevelProp = serializedObject.FindProperty("SuspiciousThreatLevel");
            AwareThreatLevelProp = serializedObject.FindProperty("AwareThreatLevel");

            UnawareEventProp = serializedObject.FindProperty("UnawareEvent");
            SuspiciousEventProp = serializedObject.FindProperty("SuspiciousEvent");
            AwareEventProp = serializedObject.FindProperty("AwareEvent");

            UnawareReactionProp = serializedObject.FindProperty("UnawareReaction");
            SuspiciousReactionProp = serializedObject.FindProperty("SuspiciousReaction");
            AwareReactionProp = serializedObject.FindProperty("AwareReaction");

            HideSettingsFoldoutProp = serializedObject.FindProperty("HideSettingsFoldout");
            SoundDetectorFoldoutProp = serializedObject.FindProperty("SoundDetectorFoldout");
            UnawareFoldoutProp = serializedObject.FindProperty("UnawareFoldout");
            SuspiciousFoldoutProp = serializedObject.FindProperty("SuspiciousFoldout");
            AwareFoldoutProp = serializedObject.FindProperty("AwareFoldout");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldSoundDetector self = (EmeraldSoundDetector)target;

            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Sound Detector", SoundDetectionEditorIcon, new GUIContent(), HideSettingsFoldoutProp);

            if (!HideSettingsFoldoutProp.boolValue)
            {
                EditorGUILayout.Space();
                SoundDetectorSettings(self);
                EditorGUILayout.Space();
                UnawareSettings();
                EditorGUILayout.Space();
                SuspiciousSettings();
                EditorGUILayout.Space();
                AwareSettings();
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void SoundDetectorSettings (EmeraldSoundDetector self)
        {
            SoundDetectorFoldoutProp.boolValue = EditorGUILayout.Foldout(SoundDetectorFoldoutProp.boolValue, "Sound Detector Settings", true, FoldoutStyle);

            if (SoundDetectorFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Sound Detector Settings", "The Sound Detector component gives AI the ability to hear player targets and other sounds made by external sources. When these events happen, " +
                    "it will trigger Reaction Objects that will determine what the AI does. These Reaction Objects can be customized by the user.", false);
                EditorGUILayout.HelpBox("AI will only listen for player targets they have an Enemy Relation Type with. The tags and layers used for this are based on this AI's Emerald AI settings from its Detection Settings.", MessageType.Info); //TODO: Replace with CustomEditorProperties equivalent
                GUILayout.Space(10);

                DisplayThreatLevel(self);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttentionRateProp, "Attention Rate", 0.0025f, 1.0f);
                CustomHelpLabelField("Controls how quickly an AI's Current Threat Amount will increase, given that any detected targets' velocity is at or above the Min Velocity Threshold.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), MinVelocityThresholdProp, "Min Velocity Threshold", 0f, 10f);
                CustomHelpLabelField("Controls the minimum detected velocity 'sound'. Any amount lower than this will be handled by the Attention Falloff and will not be detected.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), TargetVelocityFactorProp, "Target Velocity Factor", 0f, 1f);
                CustomHelpLabelField("Controls how much a detected target's velocity influences the Attention Rate.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DistanceFactorProp, "Target Distance Factor", 0f, 1f);
                CustomHelpLabelField("Controls how much a detected target's distance influences the Attention Rate. The closer the target is, the more it will affect the Attention Rate.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttentionFalloffProp, "Attention Falloff", 0.0025f, 1.0f);
                CustomHelpLabelField("Controls how quickly an AI's Current Threat Amount will decrease, given that all detected targets' velocity is below the Min Velocity Threshold.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), FalloffDealyProp, "Falloff Delay", 0, 10f);
                CustomHelpLabelField("Controls how many seconds must pass before the Attention Falloff happens. This will only happen after all detected targets' velocity is below the Min Velocity Threshold.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AttractModifierCooldownProp, "Attract Modifier Cooldown", 1f, 25f);
                CustomHelpLabelField("Controls how many seconds need to pass before the AI can detect Attract Modifier again, after already detecting one.", true);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void UnawareSettings ()
        {
            UnawareFoldoutProp.boolValue = EditorGUILayout.Foldout(UnawareFoldoutProp.boolValue, "Unaware Settings", true, FoldoutStyle);

            if (UnawareFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Unaware Settings", "An Unaware Reaction will only be triggered after an AI has become Suspicious or Aware. This can happen after a target has been " +
                    "lost or is too quite to be detected. This should be used for resetting an AI back to its original settings, given they've been modified with a Reaction Object.", false);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), UnawareThreatLevelProp, "Unaware Threat Level", 0.0f, 1f);
                CustomHelpLabelField("Controls the Threat Amount that's needed for an AI to reach the Unaware Threat Level.", false);
                GUILayout.Space(15);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), DelayUnawareSecondsProp, "Delay Unaware Seconds", 0f, 25f);
                CustomHelpLabelField("Controls how many seconds need to pass before the Unaware level is invoked, given the Unware Threat Level has been met.", false);
                GUILayout.Space(15);
                EditorGUILayout.PropertyField(UnawareReactionProp, new GUIContent("Unaware Reaction"));
                CustomHelpLabelField("The Reaction Object that will be used for this Reaction. Reaction Objects can be shared between multiple AI so it does not have to be recreated.", false);
                GUILayout.Space(15);
                CustomHelpLabelField("Unaware Events - Controls the custom events that happen when the AI becomes unaware.", false);
                EditorGUILayout.PropertyField(UnawareEventProp, new GUIContent("Unaware Event"));
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void SuspiciousSettings()
        {
            SuspiciousFoldoutProp.boolValue = EditorGUILayout.Foldout(SuspiciousFoldoutProp.boolValue, "Suspicious Settings", true, FoldoutStyle);

            if (SuspiciousFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Suspicious Settings", "A Suspicious Reaction will only be triggered once and after an AI has reached a Suspicious Threat Level. " +
                    "It will not trigger again until after the AI has engaged with a target or if it has reached the Unaware Threat Level.", false);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), SuspiciousThreatLevelProp, "Suspicious Threat Level", 0.0f, 1f);
                CustomHelpLabelField("Controls the Threat Amount that's needed for an AI to reach the Suspicious Threat Level.", false);
                GUILayout.Space(15);
                EditorGUILayout.PropertyField(SuspiciousReactionProp, new GUIContent("Suspicious Reaction"));
                CustomHelpLabelField("The Reaction Object that will be used for this Reaction. Reaction Objects can be shared between multiple AI so it does not have to be recreated.", false);
                GUILayout.Space(15);
                CustomHelpLabelField("Suspicious Events - Controls the custom events that happen when an AI reaches a Suspicious Threat Level.", false);
                EditorGUILayout.PropertyField(SuspiciousEventProp, new GUIContent("Suspicious Event"));
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void AwareSettings()
        {
            AwareFoldoutProp.boolValue = EditorGUILayout.Foldout(AwareFoldoutProp.boolValue, "Aware Settings", true, FoldoutStyle);

            if (AwareFoldoutProp.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Aware Settings", "An Aware Reaction will only be triggered once and after an AI has reached an Aware Threat Level. " +
                    "It will not trigger again until after the AI has engaged with a target or if it has reached the Unaware Threat Level.", false);
                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), AwareThreatLevelProp, "Aware Threat Level", 0.0f, 1f);
                CustomHelpLabelField("Controls the Threat Amount that's needed for an AI to reach the Aware Threat Level.", false);
                GUILayout.Space(15);
                EditorGUILayout.PropertyField(AwareReactionProp, new GUIContent("Aware Reaction"));
                CustomHelpLabelField("The Reaction Object that will be used for this Reaction. Reaction Objects can be shared between multiple AI so it does not have to be recreated.", false);
                GUILayout.Space(15);
                CustomHelpLabelField("Aware Events - Controls the custom events that happen when an AI reaches a Aware Threat Level.", false);
                EditorGUILayout.PropertyField(AwareEventProp, new GUIContent("Aware Event"));
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void DisplayThreatLevel (EmeraldSoundDetector self)
        {
            EditorGUILayout.BeginVertical("Box"); //Begin Title Box

            DisplayTitle("Info"); //Title

            CustomHelpLabelField("Current Threat Level: " + self.CurrentThreatLevel.ToString(), false);

            Rect r = EditorGUILayout.BeginVertical();
            r.height = 25;
            EditorGUI.ProgressBar(r, self.CurrentThreatAmount, "Current Threat Amount: " + (Mathf.Round(self.CurrentThreatAmount * 100f) / 100f).ToString());
            EditorGUILayout.EndVertical();
            GUILayout.Space(35);
            EditorGUILayout.EndVertical(); //End Title Box
            GUILayout.Space(15);
        }

        void CustomHelpLabelField(string TextInfo, bool UseSpace)
        {
            GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);
            EditorGUILayout.LabelField(TextInfo, EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            if (UseSpace)
            {
                EditorGUILayout.Space();
            }
        }

        void DisplayTitle(string Title)
        {
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.25f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }
    }
}