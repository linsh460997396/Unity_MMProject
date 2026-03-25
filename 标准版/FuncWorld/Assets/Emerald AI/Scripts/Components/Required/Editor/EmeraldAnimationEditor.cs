using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldAnimation))]
    [CanEditMultipleObjects]
    public class EmeraldAnimationEditor : Editor
    {
        public static EditorWindow EditorWindowRef;
        #region SerializedProperties
        List<string> Type1AttackAnimationEnum = new List<string>();
        List<string> Type2AttackAnimationEnum = new List<string>();
        GUIStyle FoldoutStyle;
        Texture AnimationsEditorIcon;

        bool IsInScene;
        bool IsInPrefabInstance;

        SerializedProperty AnimationProfileProp, HideSettingsFoldout, AnimationProfileFoldout;
        #endregion

        void OnEnable()
        {
            EmeraldAnimation self = (EmeraldAnimation)target;
            self.AIAnimator = self.GetComponent<Animator>();
            if (AnimationsEditorIcon == null) AnimationsEditorIcon = Resources.Load("Editor Icons/EmeraldAnimation") as Texture;

            ApplyRuntimeAnimatorController(self);
            UpdateAbilityAnimationEnums();
            InitializeProperties();

            IsInScene = self.gameObject.scene.IsValid();
            IsInPrefabInstance = StageUtility.GetStage(self.gameObject) != StageUtility.GetMainStage();
        }

        void InitializeProperties()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            AnimationProfileFoldout = serializedObject.FindProperty("AnimationProfileFoldout");
            AnimationProfileProp = serializedObject.FindProperty("m_AnimationProfile");
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldAnimation self = (EmeraldAnimation)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Animations", AnimationsEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingAnimationProfileMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                AnimationProfiles(self);
                EditorGUILayout.Space();
                UpdateEditor(self);
            }

            serializedObject.ApplyModifiedProperties();
            CustomEditorProperties.EndScriptHeader();
        }

        /// <summary>
        /// Displays a missing Animation Profile message within the EmeraldAnimation.
        /// </summary>
        void MissingAnimationProfileMessage(EmeraldAnimation self)
        {
            if (self.m_AnimationProfile == null)
            {
                CustomEditorProperties.DisplaySetupWarning("This AI needs to have an Animation Profile. Press the 'Create New Animation Profile' button below to create a new one or assign one that has already been created.");
            }
            else if (self.m_AnimationProfile.AIAnimator == null)
            {
                CustomEditorProperties.DisplaySetupWarning("This AI has an Animation Profile, but an Animator Controller has not been generated for it. Please create one and assign all needed animations through the Animation Profile object. " +
                    "You can press the 'Edit Animation Profile' to open up an editor window to begin editing.");
            }
        }

        void UpdateAbilityAnimationEnums()
        {
            EmeraldAnimation self = (EmeraldAnimation)target;

            if (self.m_AnimationProfile == null)
                return;

            //Populate the Type1AttackEnumAnimations array with the proper animation name.
            if (self.m_AnimationProfile.Type1Animations.AttackList.Count > 0)
            {
                for (int i = 0; i < self.m_AnimationProfile.Type1Animations.AttackList.Count; i++)
                {
                    if (self.m_AnimationProfile.Type1Animations.AttackList[i].AnimationClip != null)
                        Type1AttackAnimationEnum.Add(self.m_AnimationProfile.Type1Animations.AttackList[i].AnimationClip.name);
                }
            }

            //Populate the Type2AttackEnumAnimations array with the proper animation name.
            if (self.m_AnimationProfile.Type2Animations.AttackList.Count > 0)
            {
                for (int i = 0; i < self.m_AnimationProfile.Type2Animations.AttackList.Count; i++)
                {
                    if (self.m_AnimationProfile.Type2Animations.AttackList[i].AnimationClip != null)
                        Type2AttackAnimationEnum.Add(self.m_AnimationProfile.Type2Animations.AttackList[i].AnimationClip.name);
                }
            }

            //Pass the array to the EmeraldAnimation script so it can be stored
            self.Type1AttackEnumAnimations = Type1AttackAnimationEnum.ToArray();
            self.Type2AttackEnumAnimations = Type2AttackAnimationEnum.ToArray();
        }

        void AnimationProfiles(EmeraldAnimation self)
        {
            AnimationProfileFoldout.boolValue = CustomEditorProperties.Foldout(AnimationProfileFoldout.boolValue, "Animation Profile Settings", true, FoldoutStyle);

            if (AnimationProfileFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Animation Profile", "An Animation Profile holds all of an AI's animation data, including the Animator Controller this AI will use. This allows AI to share the same animation data with only needing to rely on a single " +
                    "Animation Profile. Any changes made to an Animation Profile will affect any AI using that Animation Profile.", false);

                CustomEditorProperties.TextTitleWithDescription("Note", "The animations must be compatible with this model and share the same Rig Type. If your AI doesn'transform play animations correctly, or falls through the floor, it is likely that you are missing an animation, the " +
                    "Rig Type is not compatible, or that the animation is not compatible.", true);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(AnimationProfileProp);
                CustomEditorProperties.CustomHelpLabelField("The Animation Profile this AI is using. All animations, including the Animator Controller, will be used for this AI and any other AI using it.", false);

                if (!IsInScene)
                {
                    CustomEditorProperties.DisplayImportantMessage("The Animation Viewer can'transform be used in the Project tab. The AI must be within the Scene and in the Hierarchy tab.");
                }

                if (IsInPrefabInstance)
                {
                    CustomEditorProperties.DisplayImportantMessage("The Animation Viewer can'transform be used while editing a prefab.");
                }

                EditorGUI.BeginDisabledGroup(!IsInScene || IsInPrefabInstance);//AA
                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile == null || self.m_AnimationProfile.AIAnimator == null);
                EditorGUILayout.Space();
                if (GUILayout.Button(new GUIContent("Open Animation Viewer", "Preview all animations on the current Animation Profile, in real-time, on this AI within the Unity Scene."), GUILayout.Height(20)))
                {
                    OpenAnimationPreview(self);
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile == null);
                if (GUILayout.Button(new GUIContent("Edit Animation Profile", "Edit the current Animation Profile in a separate window so you can preview animations while keeping a reference to the current Animation Profile."), GUILayout.Height(20)))
                {
                    EditAnimationProfile(self);
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile == null);
                if (GUILayout.Button(new GUIContent("Clear Animation Profile", "Clears the Animation Profile slot so a new one can be created. Note: The current Animation Profile object will remain in your project at its current path."), GUILayout.Height(20)))
                {
                    AnimationProfileProp.objectReferenceValue = null;
                    serializedObject.FindProperty("AIAnimator").objectReferenceValue = null;
                    self.AnimatorControllerGenerated = false;
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(self.m_AnimationProfile != null);
                if (GUILayout.Button(new GUIContent("Create New Animation Profile", "Creates a new Animation Profile within the Emerald AI/Animation Profiles folder. If you would like to create a new Animation Profile, remove the one in the current slot by pressing the 'Clear Animation Profile' button."), GUILayout.Height(20)))
                {
                    CreateAnimationProfile(self);
                }
                GUILayout.Space(2.5f);
                EditorGUI.EndDisabledGroup();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        /// <summary>
        /// Applies the Runtime Animator Controller from the Animation Profile to the AI's Animator.
        /// </summary>
        void ApplyRuntimeAnimatorController (EmeraldAnimation self)
        {
            if (self.AIAnimator != null && self.m_AnimationProfile != null && self.AIAnimator.runtimeAnimatorController == null && self.m_AnimationProfile.AIAnimator != null ||
                self.AIAnimator != null && self.m_AnimationProfile != null && self.m_AnimationProfile.AIAnimator != null && self.AIAnimator != self.m_AnimationProfile.AIAnimator)
                self.AIAnimator.runtimeAnimatorController = self.m_AnimationProfile.AIAnimator;
        }

        /// <summary>
        /// Creates a new Animation Profile object, using the object's name, to the user set folder.
        /// </summary>
        void CreateAnimationProfile(EmeraldAnimation self)
        {
            string FilePath = EditorUtility.SaveFilePanelInProject("Save as Animation Profile", "", "asset", "Please enter a file name to save the file to");

            if (string.IsNullOrEmpty(FilePath))
            {
                //For some reason, EditorUtility.SaveFilePanelInProject throws an incorrect EditorGUILayout error when it's used with some custom properties. This fixes it... 
                CustomEditorProperties.BeginScriptHeader("", null);
                CustomEditorProperties.BeginFoldoutWindowBox();
                return;
            }

            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(FilePath)))
            {
                AnimationProfile NewAnimationProfile = CreateInstance<AnimationProfile>();
                AssetDatabase.CreateAsset(NewAnimationProfile, FilePath);
                self.m_AnimationProfile = NewAnimationProfile;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                var ExistingAnimationProfile = AssetDatabase.LoadAssetAtPath(FilePath, typeof(AnimationProfile));
                self.m_AnimationProfile = (AnimationProfile)ExistingAnimationProfile;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //For some reason, EditorUtility.SaveFilePanelInProject throws an incorrect EditorGUILayout error when it's used with some custom properties. This fixes it... 
            CustomEditorProperties.BeginScriptHeader("", null);
            CustomEditorProperties.BeginFoldoutWindowBox();
        }

        void OpenAnimationPreview (EmeraldAnimation self)
        {
            var m_AnimationPreviewEditor = (AnimationViewerManager)EditorWindow.GetWindow(typeof(AnimationViewerManager), true, "Animation Viewer Manager");
            m_AnimationPreviewEditor.Initialize(self.gameObject);
        }

        void EditAnimationProfile (EmeraldAnimation self)
        {
            if (self.m_AnimationProfile == null)
                return;

            //Close the static reference to any other Animation Profile PropertyEditors before creating a new one
            if (EditorWindowRef != null && EditorWindowRef.name == "Animation Profile")
                EditorWindowRef.Close();

            System.Type propertyEditorType = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor");
            System.Type[] callTypes = new[] { typeof(Object), typeof(bool) };
            object[] callOpenBuffer = { null, true };

            //Use reflection to create a PropertyEditor, as there's no API to do so before Unity 2021.2, and pass the Animation Profile to open it in a separate tab.
            MethodInfo openPropertyEditorInfo;
            openPropertyEditorInfo = propertyEditorType.GetMethod("OpenPropertyEditor", BindingFlags.Static | BindingFlags.NonPublic, null, callTypes, null);
            self.m_AnimationProfile.EmeraldAnimationComponent = self; //Used for updating changes the currently edited AI (given that it isn'transform null)
            callOpenBuffer[0] = self.m_AnimationProfile;
            openPropertyEditorInfo.Invoke(null, callOpenBuffer);

            //Cache the PropertyEditor and name it Sound Profile (only one can be active at a time)
            EditorWindowRef = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor"));
            EditorWindowRef.name = "Animation Profile";
            EditorWindowRef.minSize = new Vector2(Screen.currentResolution.width / 4f, Screen.currentResolution.height / 2f);
        }

        void UpdateEditor (EmeraldAnimation self)
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