using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Reflection;

namespace EmeraldAI.Utility
{
    public class AnimationViewerManager : EditorWindow
    {
        public static AnimationViewerManager Instance;

        Color TimelineOutlineColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        public bool UseRootMotion;
        public int CurrentPreviewAnimationIndex = 0;
        public int PresetAnimationEventIndex = 0;

        float TimeScale = 1.0f;
        Vector2 WindowOffset;
        protected float time = 0.0f;
        Texture AnimationProfileEditorIcon;
        Texture PlayButtonIcon;
        Texture PauseButtonIcon;
        Texture AnimationEventIcon;
        public static Vector3 DefaultPosition;
        public static Vector3 DefaultEuler;
        bool AnimationIsPlaying;
        
        bool RootMotionChanged;
        GameObject CurrentAnimationViewerAI = null;
        
        AnimationClip PreviewClip = null;
        List<AnimationClip> PreviewClips = new List<AnimationClip>();
        List<string> AnimationNames = new List<string>();
        List<string> AnimationEventNames = new List<string>();
        List<EmeraldAnimationEventsClass> AnimationEventPresets = new List<EmeraldAnimationEventsClass>();
        Rect AnimationClipTimelineArea;
        Rect AnimationClipTimelinePoint;
        int AnimationEventIndex;
        int PreviousPreviewAnimationIndex;
        public AnimationEvent CurrentAnimationEvent;
        Rect CurrentEventArea;
        GameObject AnimationPreviewParent;
        Transform PreviousParent;
        Vector3 StartingPosition;
        Vector3 StartingEuler;
        bool InitializeTimelineMovement;
        bool InitializeAnimationEventMovement;
        bool EnableDebugging = false; //Internal Use Only

        [SerializeField]
        public List<AnimationClip> DuplicateAnimationEvents = new List<AnimationClip>();
        [SerializeField]
        public List<AnimationEventElement> CurrentAnimationEvents = new List<AnimationEventElement>();
        [System.Serializable]
        public class AnimationEventElement
        {
            public AnimationClip Clip;
            public List<AnimationEvent> AnimationEvents = new List<AnimationEvent>();
            public bool Modified;

            public AnimationEventElement (AnimationClip m_Clip, List<AnimationEvent> m_AnimationEvents)
            {
                Clip = m_Clip;
                AnimationEvents = new List<AnimationEvent>(m_AnimationEvents);
            }
        }

        void OnEnable()
        {
            if (EditorApplication.isPlaying)
                return;

            if (AnimationProfileEditorIcon == null) AnimationProfileEditorIcon = Resources.Load("Editor Icons/EmeraldDetection") as Texture;
            if (PlayButtonIcon == null) PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPlayButton") as Texture;
            if (PauseButtonIcon == null) PauseButtonIcon = Resources.Load("Editor Icons/EmeraldPauseButton") as Texture;
            if (AnimationEventIcon == null) AnimationEventIcon = Resources.Load("Editor Icons/EmeraldAnimationEvent") as Texture;

            this.minSize = new Vector2(Screen.currentResolution.width / 6f, Screen.currentResolution.height / 1.7f);
            this.maxSize = new Vector2(Screen.currentResolution.width / 4f, 1500);

            Instance = this;
            SubscribeCallbacks(); //Subscribe callbacks
            InitiailizeList();
        }

        void OnDisable()
        {
            if (EnableDebugging) Debug.Log("OnDisable");
            UnsubscribeCallbacks(); //Unsubscribe callbacks
        }

        /// <summary>
        /// Subscribe callbacks, which are used for handling the animation preview state when saving, scene changes, and recompiling.
        /// </summary>
        void SubscribeCallbacks ()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// Unsubscribe callbacks, which are used for handling the animation preview state when saving, scene changes, and recompiling.
        /// </summary>
        void UnsubscribeCallbacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        /// <summary>
        /// (Done through using a OnSceneGUI callback) Used for drawing a GUI at the base of the AI when an Animation Event is triggered. 
        /// This allows the user to focus on the animation and what frame the event is triggerd, rather than on the editor window itself.
        /// The GUI is change color while the event is within the time frame of triggering.
        /// </summary>
        private void OnSceneGUI(SceneView obj)
        {
            if (PreviewClip != null)
            {
                Vector3 pos = CurrentAnimationViewerAI.transform.position;
                Color OutLineColor = new Color(0, 0, 0, 1);
                Color FaceColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);

                Vector3[] verts = new Vector3[]
                {
                    new Vector3(pos.x - 0.5f, pos.y, pos.z - 0.5f),
                    new Vector3(pos.x - 0.5f, pos.y, pos.z + 0.5f),
                    new Vector3(pos.x + 0.5f, pos.y, pos.z + 0.5f),
                    new Vector3(pos.x + 0.5f, pos.y, pos.z - 0.5f)
                };

                //Calculate the time so that each Animation Event changes the GUI Handle color when the timeline point is within range. 
                float MouseLerp = (AnimationClipTimelinePoint.x / AnimationClipTimelineArea.width);
                float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x - 2.5f, AnimationClipTimelineArea.min.x + 1f, MouseLerp);
                float ModifiedTime = ((AnimationClipTimelinePoint.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length);

                for (int i = 0; i < CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count; i++)
                {
                    if (ModifiedTime >= (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time - 0.005f) && ModifiedTime <= (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time + 0.005f))
                    {
                        OutLineColor = new Color(0, 0, 0f, 1);
                        FaceColor = new Color(0.5f, 1f, 0.5f, 0.15f);
                    }

                }
                Handles.DrawSolidRectangleWithOutline(verts, FaceColor, OutLineColor);
            }
        }

        /// <summary>
        /// A callback for detecting before a scene has been saved. This is used to disable the animation preview state so it isn'transform included with the save.
        /// </summary>
        private void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();
            SetAnimatorStates(true);
            DeparentPreviewObject();
            if (EnableDebugging) Debug.Log("OnSceneSaving");
        }

        /// <summary>
        /// A callback for detecting when a scene has been saved. This is used to reapply the animation preview state so it wasn'transform included with the save.
        /// </summary>
        void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            SetAnimatorStates(false);

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();

            if (GameObject.Find("Animation Viewer Parent") == null)
            {
                AnimationPreviewParent = new GameObject("Animation Viewer Parent");
                Selection.activeObject = AnimationPreviewParent;
                AnimationPreviewParent.transform.position = CurrentAnimationViewerAI.transform.position;
                AnimationPreviewParent.transform.eulerAngles = CurrentAnimationViewerAI.transform.eulerAngles;
                CurrentAnimationViewerAI.transform.SetParent(AnimationPreviewParent.transform);
            }

            if (EnableDebugging) Debug.Log("OnSceneSaved");
        }

        /// <summary>
        /// Display the ApplyChanges menu. If the user selects apply, write all changes to all modified animation clips. If the user reverts, discard all changes and close the editor window.
        /// </summary>
        void DisplayApplyChangesMenu (GameObject G = null)
        {
            if (G != CurrentAnimationViewerAI && CurrentAnimationEvents.Any(x => x.Modified == true))
            {
                if (EditorUtility.DisplayDialog("Unapplied Changes Detected", "Changes have not been applied on: \n" + CurrentAnimationViewerAI.name + "\n\nWould you like to apply your changes?", "Apply", "Revert"))
                {
                    ApplyChanges(false);
                    if (EnableDebugging) Debug.Log("OnChangesAppliedMenuMessage");
                }
                else
                {
                    if (EnableDebugging) Debug.Log("OnChangesRevertedMenuMessage");
                }
            }
        }

        void ConfirmDiscardingMessage ()
        {
            if (EditorUtility.DisplayDialog("Discard Changes?", "Are you sure you would like to discard your changes, this process cannot be undone?", "Yes", "No"))
            {
                Initialize(CurrentAnimationViewerAI);
                CurrentAnimationEvent = null;
                if (EnableDebugging) Debug.Log("OnConfirmDiscardingMessageYes");
            }
            else
            {
                if (EnableDebugging) Debug.Log("OnConfirmDiscardingMessageNo");
            }
        }

        /// <summary>
        /// Disable animation sampling before compiling to stop AI from getting stuck in the animation sampling state.
        /// </summary>
        public void OnBeforeAssemblyReload()
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            SetAnimatorStates(true);
        }

        /// <summary>
        /// Re-enable animation sampling after compiling to put an AI back in the animation sampling state.
        /// </summary>
        public void OnAfterAssemblyReload()
        {
            SetAnimatorStates(false);
            
            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();
        }

        /// <summary>
        /// Close the editor window and animation sampling before entering play mode to stop AI from getting stuck in the animation sampling state.
        /// </summary>
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            this.Close();
            if (EnableDebugging) Debug.Log("OnPlayModeChanged");
        }

        /// <summary>
        /// Initialize the Editor Window lists with their needed data.
        /// </summary>
        void InitiailizeList ()
        {
            //Get all of the commonly used Emerald AI Event so they can be automatically added through an enum.
            AnimationEventPresets = AnimationEventInitializer.GetEmeraldAnimationEvents();

            //Add each animation event display name and add it to a list so it can be displayed through an enum.
            for (int i = 0; i < AnimationEventPresets.Count; i++)
            {
                AnimationEventNames.Add(AnimationEventPresets[i].eventDisplayName);
            }
        }

        /// <summary>
        /// Initialize the Editor Window with the data from the Emerald AI Animation Editor and its Animation Profile.
        /// </summary>
        public void Initialize (GameObject G)
        {
            DisplayApplyChangesMenu(G); //Ask to save changes before initializing another AI.

            SetAnimatorStates(false); //The Animator has to be set to Always Animate to avoid bug. Can be set back to default after closing.

            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            DeparentPreviewObject(); //Deparent the previous AI, given that one exists, before initializing a new one.
            CurrentAnimationViewerAI = G; //Cache the current object the user is editing.

            //Store the starting position and euler angles on initialization.
            StartingPosition = CurrentAnimationViewerAI.transform.position;
            StartingEuler = CurrentAnimationViewerAI.transform.eulerAngles;

            ParentPreviewObject(); //Create a temporary parent object to house the AI in while the user is previewing its animations. This allows Root Motion animations to play at the starting position and not at the (0,0,0) position.

            InitializeAnimationData(); //Initialize the animation enum with all animations form the current AI's Animation Profile.

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();
        }

        /// <summary>
        /// Initialize the animation enum with all the animations from its source. 
        /// </summary>
        void InitializeAnimationData ()
        {
            PreviewClips.Clear();
            AnimationNames.Clear();
            CurrentAnimationEvents.Clear();
            var m_AnimationProfile = CurrentAnimationViewerAI.GetComponent<EmeraldAnimation>().m_AnimationProfile;

            AssignAnimationNames(m_AnimationProfile.NonCombatAnimations, ""); //Non-Combat
            AssignAnimationNames(m_AnimationProfile.Type1Animations, "Type 1 -"); //Type 1
            AssignAnimationNames(m_AnimationProfile.Type2Animations, "Type 2 -"); //Type 2

            //Cancel the Animation Viewer Manager if there are no detected animations.
            if (CurrentAnimationEvents.Count == 0)
            {
                Close();
                if (EditorUtility.DisplayDialog("No Animations", "The attached Animation Profile doesn'transform have any animations. Press the 'Edit Animation Profile' button to add some animations.", "Okay"))
                {
                    Selection.activeGameObject = CurrentAnimationViewerAI;
                    return;
                }
            }
        }

        /// <summary>
        /// Assign the names of each animation using reflection.
        /// </summary>
        void AssignAnimationNames (AnimationParentClass AnimationCategory, string AnimationCategoryName)
        {
            foreach (var field in AnimationCategory.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (AnimationCategory.GetType().GetField(field.Name.ToString()).FieldType.ToString() == "System.Collections.Generic.List`1[EmeraldAI.AnimationClass]")
                {
                    List<AnimationClass> m_AnimationClass = (List<AnimationClass>)AnimationCategory.GetType().GetField(field.Name.ToString()).GetValue(AnimationCategory);

                    for (int i = 0; i < m_AnimationClass.Count; i++)
                    {
                        AnimationClip m_AnimationClip = m_AnimationClass[i].AnimationClip;

                        if (m_AnimationClip != null) // && !PreviewClips.Contains(m_AnimationClip)
                        {
                            CurrentAnimationEvents.Add(new AnimationEventElement(m_AnimationClip, m_AnimationClip.events.ToList()));
                            PreviewClips.Add(m_AnimationClip);

                            string TempName = field.Name.Replace("List", "");
                            TempName = TempName.Replace("Animation", "");
                            TempName = System.Text.RegularExpressions.Regex.Replace(TempName, "[A-Z]", " $0");
                            TempName = "(" + AnimationCategoryName + TempName + " " + (i + 1) + ")";
                            TempName = TempName.Replace("( ", "(");
                            AnimationNames.Add(m_AnimationClip.name + " - " + TempName);
                        }
                    }
                }

                if (AnimationCategory.GetType().GetField(field.Name.ToString()).FieldType.ToString() == "EmeraldAI.AnimationClass")
                {
                    AnimationClass m_AnimationClass = (AnimationClass)AnimationCategory.GetType().GetField(field.Name.ToString()).GetValue(AnimationCategory);
                    AnimationClip m_AnimationClip = m_AnimationClass.AnimationClip;

                    if (m_AnimationClip != null)
                    {
                        CurrentAnimationEvents.Add(new AnimationEventElement(m_AnimationClip, m_AnimationClip.events.ToList()));
                        PreviewClips.Add(m_AnimationClip);

                        string TempName = field.Name.Replace("List", " ");
                        TempName = TempName.Replace("Animation", "");
                        TempName = System.Text.RegularExpressions.Regex.Replace(TempName, "[A-Z]", " $0");
                        TempName = "(" + AnimationCategoryName + TempName + ")";
                        TempName = TempName.Replace("( ", "(");
                        AnimationNames.Add(m_AnimationClip.name + " - " + TempName);
                    }
                }
            }
        }

        /// <summary>
        /// Save the changes to all modified animation clips.
        /// </summary>
        void ApplyChanges (bool AnimationModeEnabled)
        {
            List<string> PathList = new List<string>();

            //Store all clip file paths that have been modified
            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Modified)
                {
                    if (!PathList.Contains(AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip)))
                    {
                        PathList.Add(AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip));
                    }
                }
            }

            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Modified && !DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip))
                {
                    var path = AssetDatabase.GetAssetPath(CurrentAnimationEvents[i].Clip);
                    string PathType = AssetImporter.GetAtPath(path).ToString(); //Get the path type, this will help determine whether the clip is from an FBX or an individual animation clip.
                    PathType = PathType.Replace(" ", "");
                    PathType = PathType.Replace("(", "");
                    PathType = PathType.Replace(")", "");

                    if (PathType == "UnityEngine.FBXImporter")
                    {
                        var modelImporter = (ModelImporter)AssetImporter.GetAtPath(path) as ModelImporter;
                        SerializedObject so = new SerializedObject(modelImporter);
                        SerializedProperty clips = so.FindProperty("m_ClipAnimations");

                        //Set the events equal to the events from the CurrentAnimationEvents for each clip, given that it has been modified.
                        for (int m = 0; m < modelImporter.clipAnimations.Length; m++)
                        {
                            if (clips.GetArrayElementAtIndex(m).displayName == CurrentAnimationEvents[i].Clip.name)
                            {
                                Debug.Log(clips.GetArrayElementAtIndex(m).displayName + "'s Animation Events have been updated");
                                SerializedProperty prop = clips.GetArrayElementAtIndex(m).FindPropertyRelative("events");
                                if (CurrentAnimationEvents[i].AnimationEvents.Count == 0)
                                {
                                    prop.ClearArray();
                                    if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                                }
                                else
                                {
                                    SetEvents(clips.GetArrayElementAtIndex(m), CurrentAnimationEvents[i].AnimationEvents.ToArray(), CurrentAnimationEvents[i].Clip);
                                    if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                                }
                            }
                        }

                        CurrentAnimationEvents[i].Modified = false;
                        so.ApplyModifiedProperties();
                    }
                    else
                    {
                        //Get the single animation clip's path
                        AnimationClip animClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip));
                        SerializedObject so = new SerializedObject(animClip);

                        if (CurrentAnimationEvents[i].AnimationEvents.Count == 0)
                        {
                            if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                        }
                        else
                        {
                            Debug.Log(animClip.name + "'s Animation Events have been updated");
                            SetEventsOnClip(CurrentAnimationEvents[i].AnimationEvents.ToArray(), CurrentAnimationEvents[i].Clip);
                            if (!DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip)) DuplicateAnimationEvents.Add(CurrentAnimationEvents[i].Clip);
                        }

                        CurrentAnimationEvents[i].Modified = false;
                        so.ApplyModifiedProperties();
                    }
                }
                else if (DuplicateAnimationEvents.Contains(CurrentAnimationEvents[i].Clip))
                {
                    CurrentAnimationEvents[i].Modified = false;
                }
            }

            for (int i = 0; i < PathList.Count; i++)
            {
                string PathType = AssetImporter.GetAtPath(PathList[i]).ToString();
                PathType = PathType.Replace(" ", "");
                PathType = PathType.Replace("(", "");
                PathType = PathType.Replace(")", "");

                if (PathType == "UnityEngine.FBXImporter")
                {
                    var modelImporter = (ModelImporter)AssetImporter.GetAtPath(PathList[i]) as ModelImporter;
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(modelImporter));
                    AssetDatabase.SaveAssets();
                }
            }

            if (!AnimationMode.InAnimationMode() && AnimationModeEnabled)
                AnimationMode.StartAnimationMode();

            //Remove the focus of the currently selected Animation Event
            CurrentAnimationEvent = null;
            CurrentEventArea = new Rect();
            GUI.FocusControl(null);
            Repaint();
            DuplicateAnimationEvents.Clear();
        }

        // Main editor window
        public void OnGUI()
        {
            GUIStyle Style = EditorStyles.wordWrappedLabel;
            Style.fontStyle = FontStyle.Bold;
            Style.fontSize = 16;
            Style.padding.top = -11;
            Style.alignment = TextAnchor.UpperCenter;

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("Window");
            GUI.backgroundColor = Color.white;
            EditorGUILayout.LabelField(new GUIContent(AnimationProfileEditorIcon), Style, GUILayout.ExpandWidth(true), GUILayout.Height(32));
            EditorGUILayout.LabelField("Animation Viewer", Style, GUILayout.ExpandWidth(true));            
            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-18);

            CustomEditorProperties.TextTitleWithDescription("Animation Clip Selection", "Select an animation clip (pulled from the AI's current Animation Profile) to preview the animation on the recntly selected AI right within the scene.", false);
            CustomEditorProperties.NoticeTextDescription("Note: Inverse Kinematics and animation blending are not used during this process. Animations will have better quality during runtime usage.", true);

            //Populate an enum with the selected AI's current animations.
            CurrentPreviewAnimationIndex = EditorGUILayout.Popup("Current Animation", CurrentPreviewAnimationIndex, AnimationNames.ToArray());
            PreviewClip = CurrentAnimationEvents[CurrentPreviewAnimationIndex].Clip;

            if (PreviousPreviewAnimationIndex != CurrentPreviewAnimationIndex)
            {
                CurrentAnimationEvent = null;
                PreviousPreviewAnimationIndex = CurrentPreviewAnimationIndex;
            }
            
            GUILayout.Space(15);

            CustomEditorProperties.CustomHelpLabelField("Makes the currently selected object the Current Animation Clip within the Project tab.", false);
            if (GUILayout.Button("View Current Clip in Project"))
            {
                Selection.activeObject = PreviewClip;
            }

            GUILayout.Space(10);
            UseRootMotion = EditorGUILayout.Toggle("Use Root Motion", UseRootMotion);
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window");
            GUILayout.Space(-18);

            if (PreviewClip != null)
            {
                CustomEditorProperties.TextTitleWithDescription("Animation Timeline", "You can select anywhere within the timeline area below to cycle through an animation clip. Pressing the play button will allow the animation to play in real-time, continiously, " +
                    "until the pause button is pressed. You can add preset, and custom, Animation Events by using the Event Type popup. You can hover over the Event Type dropdown for a tooltip description of the currently selected event.", false);
                GUILayout.Space(2.5f);

                GUIStyle BoldStyle = GUI.skin.button;
                BoldStyle.fontStyle = FontStyle.Bold;

                if (AnimationIsPlaying)
                {
                    GUI.backgroundColor = new Color(1.5f, 0.1f, 0f, 0.75f);
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                if (GUILayout.Button(new GUIContent(PlayButtonIcon, "Play and pause the Current Animation."), BoldStyle))
                {
                    AnimationIsPlaying = !AnimationIsPlaying;
                    if (AnimationIsPlaying)
                        PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPauseButton") as Texture;
                    else
                        PlayButtonIcon = Resources.Load("Editor Icons/EmeraldPlayButton") as Texture;
                }

                GUI.backgroundColor = Color.white;

                //Add Event Button
                if (GUILayout.Button(new GUIContent(AnimationEventIcon, "Adds the current Event Type at the current frame. This will also apply some needed parameters."), BoldStyle))
                {
                    GUI.FocusControl(null); //Remove the focus of the currently selected Animation Event
                    var m_event = new AnimationEvent();
                    m_event.functionName = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.functionName;
                    m_event.stringParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.stringParameter;
                    m_event.floatParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.floatParameter;
                    m_event.intParameter = AnimationEventPresets[PresetAnimationEventIndex].animationEvent.intParameter;
                    m_event.time = time + Mathf.Lerp(0.009f, -0.0111f, time / PreviewClip.length);

                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Add(m_event); //Add the event to the AnimationEvents list
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;

                    UpdateIdenticalAnimationClips();

                    //Set the CurrentAnimationEvent and other info to the newly created event.
                    CurrentAnimationEvent = m_event;
                    AnimationEventIndex = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.IndexOf(m_event);
                    Repaint();
                }

                Rect r2 = EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);
                PresetAnimationEventIndex = EditorGUILayout.Popup("", PresetAnimationEventIndex, AnimationEventNames.ToArray(), GUILayout.MinWidth(100));
                Rect LastRect = GUILayoutUtility.GetLastRect();
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Space(10);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(20);
                
                TimeScale = EditorGUILayout.Slider("", TimeScale, 0.01f, 1f, GUILayout.MaxHeight(0), GUILayout.MinWidth(100));
                
                EditorGUILayout.EndVertical();
                EditorGUI.LabelField(new Rect((r2.min.x + 45), r2.position.y - 2, (r2.width), 20), new GUIContent("Event Type"));
                EditorGUI.LabelField(new Rect((r2.min.x + 6), r2.position.y + 18, LastRect.width, 20), new GUIContent("", AnimationEventPresets[PresetAnimationEventIndex].eventDescription));
                EditorGUI.LabelField(new Rect((r2.max.x - 265) + r2.min.x, r2.position.y - 2, 100, 20), "Playback Speed");
                EditorGUI.LabelField(new Rect((r2.min.x + r2.width - 40), r2.position.y + 19, (r2.width), 20), System.Math.Round(TimeScale, 2) + "pixelX");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                float stopTime = PreviewClip.length;
                Rect r = EditorGUILayout.BeginVertical(); 
                WindowOffset.x = 5;
                var EditedTime = time * PreviewClip.frameRate;
                var niceTime = Mathf.Floor(EditedTime / PreviewClip.frameRate).ToString("00") + ":" + Mathf.FloorToInt(EditedTime % PreviewClip.frameRate).ToString("00");
                var ClipPercentage = ((time / stopTime) * 100f).ToString("F2");

                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x - 3, r.position.y - 3f, r.width - (WindowOffset.x * 2) + 6, 105.5f), TimelineOutlineColor); //Draws the outline around the whole timeline area
                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x, r.position.y + 0.25f, r.width - (WindowOffset.x * 2), 100f), new Color(0.3f, 0.3f, 0.3f, 1f)); //Draws the background over the whole timeline area

                AnimationClipTimelineArea = new Rect(r.x + WindowOffset.x - 3, r.position.y - 0.75f, r.width - (WindowOffset.x * 2) + 3, 50f);
                float AdjustedTime = (time / stopTime);
                EditorGUI.DrawRect(new Rect((r.x + WindowOffset.x), r.position.y + 0.25f, (r.width - (WindowOffset.x * 2) - 2) * AdjustedTime, 50), new Color(0.1f, 0.25f, 0.5f, 1f)); //Draws the timeline progress

                //---Draw the timeline info---
                GUIStyle LabelStyle = new GUIStyle();
                LabelStyle.alignment = TextAnchor.MiddleCenter;
                LabelStyle.fontStyle = FontStyle.Bold;
                LabelStyle.normal.textColor = Color.white;
                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x , r.position.y + 50f, r.width - (WindowOffset.x * 2), 50f), new Color(0.1f, 0.1f, 0.1f, 1f)); //Draws the background of the timeline
                EditorGUI.LabelField(new Rect(r.x + WindowOffset.x, r.position.y + 50, r.width - (WindowOffset.x * 2), 50), niceTime + "   (" + (ClipPercentage + "%") + ")    Frame " + Mathf.Round(time * PreviewClip.frameRate).ToString(), LabelStyle); //Draw Animation Info on the Timeline
                //---Draw the timeline info---

                //-------------ANIMATION EVENT TIMELINE----------------
                GUIStyle AnimationEventStyle = GUI.skin.box;

                //Draws a line for each 10% within the Animation Event timeline.
                for (int i = 1; i <= 60; i++)
                {
                    EditorGUI.DrawRect(new Rect(((r.x + WindowOffset.x) + (float)(i / 60f) * (r.width - (WindowOffset.x * 2) - (r.min.x * 2.5f))), r.position.y + 37.5f, 1f, 12f), new Color(0.6f, 0.6f, 0.6f, 1f)); //Draws the Animation Event timeline Dashes
                }

                for (int i = 0; i < CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count; i++)
                {
                    if (CurrentAnimationEvent != null && CurrentAnimationEvent.time == CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time)
                        GUI.backgroundColor = new Color(1, 1, 1, 2f);
                    else
                        GUI.backgroundColor = new Color(3, 3, 3, 3);

                    //Draw each Animation Event to the Animation Event timeline based on its time within the PreviewAnimation clip.
                    Rect AnimationEventRect = new Rect((WindowOffset.x - AnimationClipTimelineArea.min.x / WindowOffset.x) + (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time / stopTime) * (r.width - (WindowOffset.x - (AnimationClipTimelineArea.min.x / WindowOffset.x) * 2) - (r.min.x)), r.position.y + 19.5f, 7.5f, 30f); //Was 41

                    if (CurrentAnimationEvent != null && CurrentAnimationEvent.time == CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i].time)
                        EditorGUI.DrawRect(AnimationEventRect, new Color(1f, 0.25f, 0.25f, 1f));
                    else
                        EditorGUI.DrawRect(AnimationEventRect, Color.white);

                    if (AnimationEventRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;
                        CurrentAnimationEvent = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[i];
                        UpdateIdenticalAnimationClips();
                        AnimationEventIndex = i;
                        CurrentEventArea = AnimationEventRect;
                        InitializeAnimationEventMovement = true;
                        Repaint();
                        GUI.FocusControl(null); //Remove the focus of the currently selected Animation Event
                    }
                }

                //Draws the timeline point
                AnimationClipTimelinePoint = new Rect(((r.x + WindowOffset.x) + (time / stopTime) * (r.width - (WindowOffset.x * 2) - (r.min.x))), r.position.y, 3.5f, 50f);
                EditorGUI.DrawRect(AnimationClipTimelinePoint, new Color(0.8f, 0.8f, 0.8f, 1f));
                //Draws the timeline point

                ChangeEventTime();

                EditorGUI.DrawRect(new Rect(r.x + WindowOffset.x - 3, r.position.y + 50, r.width - (WindowOffset.x * 2) + 6, 2.5f), TimelineOutlineColor); //Draws the outline around the whole timeline area

                GUI.backgroundColor = Color.white;
                //-------------ANIMATION EVENT TIMELINE----------------
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(120);

            //-------------ANIMATION EVENT DATA--------------------
            if (CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.Count > 0 && CurrentAnimationEvent != null)
            {
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].functionName = EditorGUILayout.TextField("Function Name", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].functionName);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].floatParameter = EditorGUILayout.FloatField("Float", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].floatParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].intParameter = EditorGUILayout.IntField("Int", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].intParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].stringParameter = EditorGUILayout.TextField("String", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].stringParameter);
                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].objectReferenceParameter = EditorGUILayout.ObjectField("Object", CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].objectReferenceParameter, typeof(object), false);

                GUILayout.Space(25);
            }

            EditorGUI.BeginDisabledGroup(CurrentAnimationEvents.Count > 0 && !CurrentAnimationEvents.Any(x => x.Modified == true));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("Apply Changes", GUILayout.MaxHeight(30)))
            {
                ApplyChanges(true);
            }

            if (GUILayout.Button("Discard Changes", GUILayout.MaxHeight(30)))
            {
                ConfirmDiscardingMessage();
            }
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            //-------------ANIMATION EVENT DATA--------------------

            GUILayout.Space(5);
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// Update all of the passed AnimationClip's events to be equal to those of the CurrentAnimationEvents (based on its index).
        /// </summary>
        public void SetEvents(SerializedProperty sp, AnimationEvent[] newEvents, AnimationClip clip)
        {
            SerializedProperty serializedProperty = sp.FindPropertyRelative("events");
            if (serializedProperty != null && serializedProperty.isArray && newEvents != null && newEvents.Length > 0)
            {
                serializedProperty.ClearArray();
                for (int i = 0; i < newEvents.Length; i++)
                {
                    AnimationEvent animationEvent = newEvents[i];
                    serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);

                    SerializedProperty eventProperty = serializedProperty.GetArrayElementAtIndex(i);
                    eventProperty.FindPropertyRelative("floatParameter").floatValue = animationEvent.floatParameter;
                    eventProperty.FindPropertyRelative("functionName").stringValue = animationEvent.functionName;
                    eventProperty.FindPropertyRelative("intParameter").intValue = animationEvent.intParameter;
                    eventProperty.FindPropertyRelative("objectReferenceParameter").objectReferenceValue = animationEvent.objectReferenceParameter;
                    eventProperty.FindPropertyRelative("data").stringValue = animationEvent.stringParameter;
                    eventProperty.FindPropertyRelative("time").floatValue = animationEvent.time / clip.length;
                }
            }
        }

        /// <summary>
        /// Update a single passed AnimationClip's events to be equal to those of the CurrentAnimationEvents.
        /// </summary>
        public void SetEventsOnClip(AnimationEvent[] newEvents, AnimationClip clip)
        {
            if (newEvents != null && newEvents.Length > 0)
            {
                AnimationUtility.SetAnimationEvents(clip, newEvents);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));
                AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Track the mouse input and position to modify animation events on the current animation clip.
        /// </summary>
        void ChangeEventTime ()
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive); //Allows Event.current to work outside of the Editor Window.

            var current = Event.current;
            if (current != null)
            {
                switch (current.type)
                {
                    case EventType.MouseDrag:
                        if (CurrentEventArea != new Rect())
                        {
                            if (InitializeAnimationEventMovement)
                            {
                                GUIUtility.hotControl = controlId;
                                float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                                float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x - 1.75f, AnimationClipTimelineArea.min.x + 3f, MouseLerp);
                                CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents[AnimationEventIndex].time = Mathf.Clamp(((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length), 0.015f, PreviewClip.length - 0.015f);
                                Repaint();
                            }
                        }
                        else if (CurrentEventArea == new Rect() && InitializeTimelineMovement)
                        {
                            GUIUtility.hotControl = controlId;
                            float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                            float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x + 2.5f, AnimationClipTimelineArea.min.x - 2.5f, MouseLerp);
                            time = Mathf.Clamp(((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length), 0, PreviewClip.length);
                            Repaint();
                        }
                        break;
                    case EventType.MouseUp:
                        if (EnableDebugging) Debug.Log("MouseUp");
                        InitializeTimelineMovement = false;
                        InitializeAnimationEventMovement = false;

                        if (CurrentEventArea != new Rect())
                        {
                            UpdateIdenticalAnimationClips();
                            CurrentEventArea = new Rect();
                            Repaint();
                        }
                        break;
                    case EventType.MouseDown:
                        if (EnableDebugging) Debug.Log("MouseDown");
                        if (new Rect(AnimationClipTimelineArea.x + 3.5f, AnimationClipTimelineArea.position.y, AnimationClipTimelineArea.width - 6.5f, AnimationClipTimelineArea.height).Contains(current.mousePosition) && CurrentEventArea == new Rect())
                        {
                            GUIUtility.hotControl = controlId;
                            float MouseLerp = (Event.current.mousePosition.x / AnimationClipTimelineArea.width);
                            float MouseOffset = Mathf.LerpAngle(AnimationClipTimelineArea.min.x + 2.5f, AnimationClipTimelineArea.min.x - 2.5f, MouseLerp);
                            time = ((Event.current.mousePosition.x - MouseOffset) / (AnimationClipTimelineArea.width)) * (PreviewClip.length);
                            InitializeTimelineMovement = true; //The mouse has been detected and pressed within the timeline area. The timeline will continusouly detect movement until the mouse button has been lifted. This is to allow smooth movement outside of the window area.
                            Repaint();
                        }
                        break;
                }
            }

            if (Event.current.isKey)
            {
                KeyCode key = Event.current.keyCode;

                if (key == KeyCode.Delete && CurrentAnimationEvent != null)
                {
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents.RemoveAt(AnimationEventIndex);
                    CurrentAnimationEvents[CurrentPreviewAnimationIndex].Modified = true;

                    UpdateIdenticalAnimationClips();

                    Repaint();
                    CurrentAnimationEvent = null;
                }
            }
        }

        /// <summary>
        /// Update everything while animation sampling is enabled, given that the EditorApplication is not playing.
        /// </summary>
        void Update()
        {
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode() && PreviewClip != null)
            {
                //Reset the AI back to the parent's root position when disabling Root Motion.
                if (UseRootMotion != RootMotionChanged)
                {
                    CurrentAnimationViewerAI.transform.localPosition = Vector3.zero;
                    CurrentAnimationViewerAI.transform.localEulerAngles = Vector3.zero;
                    RootMotionChanged = UseRootMotion;
                }

                //Update the timeline when an animation is playing.
                if (AnimationIsPlaying)
                {
                    time += Time.deltaTime * TimeScale;
                    if (time >= PreviewClip.length)
                        time = 0;
                }

                AnimationMode.BeginSampling();
                DefaultPosition = CurrentAnimationViewerAI.transform.position;
                DefaultEuler = CurrentAnimationViewerAI.transform.eulerAngles;
                AnimationMode.SampleAnimationClip(CurrentAnimationViewerAI, PreviewClip, time);

                //Keep the AI in-placw when Root Motion is disabled.
                if (!UseRootMotion)
                {
                    CurrentAnimationViewerAI.transform.position = DefaultPosition;
                    CurrentAnimationViewerAI.transform.eulerAngles = DefaultEuler;
                }

                AnimationMode.EndSampling();

                //Always repaint when sampling an animation to have smooth UI
                Repaint();
            }
        }

        /// <summary>
        /// Reset all states back to their original values and positions when closing the editor window.
        /// </summary>
        void OnDestroy()
        {
            DisplayApplyChangesMenu();

            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            SetAnimatorStates(true);
            DeparentPreviewObject();
        }

        /// <summary>
        /// Update all identical animation clip and their modifications so they are present on all duplicate clips.
        /// </summary>
        void UpdateIdenticalAnimationClips ()
        {
            for (int i = 0; i < CurrentAnimationEvents.Count; i++)
            {
                if (CurrentAnimationEvents[i].Clip == CurrentAnimationEvents[CurrentPreviewAnimationIndex].Clip)
                {
                    CurrentAnimationEvents[i].AnimationEvents = CurrentAnimationEvents[CurrentPreviewAnimationIndex].AnimationEvents;
                    CurrentAnimationEvents[i].Modified = true;
                }
            }
        }

        /// <summary>
        /// Deparent the AI from the Parent Holder object. This is used to sample Root Motion animations while retaining the AI's starting position.
        /// </summary>
        public void DeparentPreviewObject()
        {
            if (CurrentAnimationViewerAI != null)
            {
                CurrentAnimationViewerAI.transform.SetParent(PreviousParent);
                CurrentAnimationViewerAI.transform.position = StartingPosition;
                CurrentAnimationViewerAI.transform.eulerAngles = StartingEuler;
                if (AnimationPreviewParent != null && AnimationPreviewParent.transform.childCount == 0)
                    DestroyImmediate(AnimationPreviewParent);
            }
        }

        /// <summary>
        /// Create a temporary parent object to house the AI in while the user is previewing its animations. This allows Root Motion animations to play at the starting position and not at the (0,0,0) position.
        /// </summary>
        public void ParentPreviewObject ()
        {
            AnimationPreviewParent = new GameObject("Animation Viewer Parent");
            Selection.activeObject = AnimationPreviewParent;
            AnimationPreviewParent.transform.position = CurrentAnimationViewerAI.transform.position;
            AnimationPreviewParent.transform.eulerAngles = CurrentAnimationViewerAI.transform.eulerAngles;
            PreviousParent = CurrentAnimationViewerAI.transform.parent;
            CurrentAnimationViewerAI.transform.SetParent(AnimationPreviewParent.transform);
        }

        /// <summary>
        /// Due to a Unity bug, all Animator Controllers have to be disabled within the scene when testing animations using AnimationMode. 
        /// Callbacks for saving, recompiling, and entering play mode are used to avoid unwanted modifications of Animator Controllers' enabled states.
        /// </summary>
        public void SetAnimatorStates (bool state)
        {
            var AnimatorsInScene = GameObject.FindObjectsOfType<Animator>();
            for (int i = 0; i < AnimatorsInScene.Length; i++)
            {
                AnimatorsInScene[i].enabled = state;
            }
        }
    }
}