using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldSounds))]
    [CanEditMultipleObjects]
    public class EmeraldSoundsEditor : Editor
    {
        public static EditorWindow EditorWindowRef;
        GUIStyle FoldoutStyle;
        Texture SoundsEditorIcon;

        #region SerializedProperties
        SerializedProperty HideSettingsFoldout, SoundProfileProp, SoundProfileFoldout;
        #endregion

        void OnEnable()
        {
            if (SoundsEditorIcon == null) SoundsEditorIcon = Resources.Load("Editor Icons/EmeraldSounds") as Texture;
            InitializeProperties();
        }

        void InitializeProperties ()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            SoundProfileFoldout = serializedObject.FindProperty("SoundProfileFoldout");
            SoundProfileProp = serializedObject.FindProperty("SoundProfile");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldSounds self = (EmeraldSounds)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Sounds", SoundsEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingSoundProfileMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                DisplaySoundProfile(self);
                EditorGUILayout.Space();
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader();
        }

        /// <summary>
        /// Displays a missing Sound Profile message within the EmeraldAISoundsEditor.
        /// </summary>
        void MissingSoundProfileMessage(EmeraldSounds self)
        {
            if (self.SoundProfile == null)
            {
                CustomEditorProperties.DisplaySetupWarning("This AI needs to have a Sound Profile. Press the 'Create New Sound Profile' button below to create a new one or assign one that has already been created.");
            }
        }

        void DisplaySoundProfile(EmeraldSounds self)
        {
            SoundProfileFoldout.boolValue = CustomEditorProperties.Foldout(SoundProfileFoldout.boolValue, "Sound Profile Settings", true, FoldoutStyle);

            if (SoundProfileFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Sound Profile", "A Sound Profile holds all of an AI's sound data. This allows AI to share the same sound data with only needing to rely on a single " +
                    "Sound Profile. Any changes made to a Sound Profile will affect any AI using that Sound Profile. However, as many sound profiles can be created as needed. You can hover over the buttons below for more info.", true);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(SoundProfileProp);
                CustomEditorProperties.CustomHelpLabelField("The Sound Profile this AI is using. All sounds and volumes will be used for this AI and any other AI using it.", false);

                EditorGUI.BeginDisabledGroup(self.SoundProfile == null);
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Edit Sound Profile", "Edit the current Sound Profile in a separate window so you can preview sounds while keeping a reference to the current Sound Profile."), GUILayout.Height(20)))
                {
                    EditSoundProfile(self);
                }
                GUILayout.Space(2.5f);

                if (GUILayout.Button(new GUIContent("Clear Sound Profile", "Clears the Sound Profile slot so a new one can be created. Note: The current Sound Profile object will remain in your project at its current path."), GUILayout.Height(20)))
                {
                    SoundProfileProp.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(2.5f);

                EditorGUI.BeginDisabledGroup(self.SoundProfile != null);
                if (GUILayout.Button(new GUIContent("Create New Sound Profile", "Creates a new Sound Profile within the Emerald AI/Sound Profiles folder. If you would like to create a new Sound Profile, remove the one in the current slot by pressing the 'Clear Sound Profile' button."), GUILayout.Height(20)))
                {
                    CreateSoundProfile(self);
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(2.5f);

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Creates a new Sound Profile object, using the object's name, to the user set folder.
        /// </summary>
        void CreateSoundProfile(EmeraldSounds self)
        {
            string FilePath = EditorUtility.SaveFilePanelInProject("Save as Sound Profile", "", "asset", "Please enter a file name to save the file to");

            if (string.IsNullOrEmpty(FilePath))
            {
                //For some reason, EditorUtility.SaveFilePanelInProject throws an incorrect EditorGUILayout error when it's used with some custom properties. This fixes it... 
                CustomEditorProperties.BeginScriptHeader("", null);
                CustomEditorProperties.BeginFoldoutWindowBox();
                return;
            }

            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(FilePath)))
            {
                EmeraldSoundProfile NewSoundProfile = CreateInstance<EmeraldSoundProfile>();
                AssetDatabase.CreateAsset(NewSoundProfile, FilePath);
                self.SoundProfile = NewSoundProfile;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var ExistingSoundProfile = AssetDatabase.LoadAssetAtPath(FilePath, typeof(EmeraldSoundProfile));
                self.SoundProfile = (EmeraldSoundProfile)ExistingSoundProfile;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //For some reason, EditorUtility.SaveFilePanelInProject throws an incorrect EditorGUILayout error when it's used with some custom properties. This fixes it... 
            CustomEditorProperties.BeginScriptHeader("", null);
            CustomEditorProperties.BeginFoldoutWindowBox();
        }

        /// <summary>
        /// Opens the current Sound Profile in a separate window so users can preview sounds while keeping a reference to the Sound Profile.
        /// </summary>
        void EditSoundProfile (EmeraldSounds self)
        {
            if (self.SoundProfile == null)
                return;

            //Close the static reference to any other Sound Profile PropertyEditors before creating a new one
            if (EditorWindowRef != null && EditorWindowRef.name == "Sound Profile")
                EditorWindowRef.Close();

            System.Type propertyEditorType = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor");
            System.Type[] callTypes = new[] { typeof(Object), typeof(bool) };
            object[] callOpenBuffer = { null, true };

            //Use reflection to create a PropertyEditor, as there's no API to do so before Unity 2021.2, and pass the Sound Profile to open it in a separate tab.
            MethodInfo openPropertyEditorInfo;
            openPropertyEditorInfo = propertyEditorType.GetMethod("OpenPropertyEditor",BindingFlags.Static | BindingFlags.NonPublic, null, callTypes, null);
            callOpenBuffer[0] = self.SoundProfile;
            openPropertyEditorInfo.Invoke(null, callOpenBuffer);

            //Cache the PropertyEditor and name it Sound Profile (only one can be active at a time)
            EditorWindowRef = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor"));
            EditorWindowRef.name = "Sound Profile";
            EditorWindowRef.minSize = new Vector2(Screen.currentResolution.width / 4f, Screen.currentResolution.height / 2f);
        }
    }
}