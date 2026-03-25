using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EmeraldAI.Utility
{
    public class EmeraldSetupManager : EditorWindow
    {
        public GameObject ObjectToSetup;
        public LayerMask AILayer = 4;
        public string AITag = "Untagged";
        public List<GameObject> ObjectsToSetup = new List<GameObject>();

        //Required
        public bool m_EmeraldSystem = true;
        public bool m_EmeraldAnimation = true;
        public bool m_EmeraldCombat = true;
        public bool m_EmeraldSounds = true;
        public bool m_EmeraldMovement = true;
        public bool m_EmeraldHealth = true;
        public bool m_EmeraldBehaviors = true;
        public bool m_EmeraldDetection = true;

        //Optional
        public bool m_EmeraldEvents = false;
        public bool m_EmeraldItems = false;
        public bool m_EmeraldInverseKinematics = false;
        public bool m_EmeraldOptimization = false;
        public bool m_EmeraldSoundDetector = false;
        public bool m_EmeraldUI = false;
        public bool m_EmeraldFootsteps = false;
        public bool m_EmeraldDebugger = false;
        public bool m_TargetPositionModifier = false;

        public AnimationProfile m_AnimationProfile;
        public EmeraldSoundProfile m_SoundProfile;

        public AnimatorTypeState AnimatorType = AnimatorTypeState.RootMotion;
        public enum AnimatorTypeState { RootMotion, NavMeshDriven }

        GUIStyle TitleStyle;
        Vector2 scrollPos;
        Texture SettingsIcon;
        bool DisplayConfirmation = false;
        bool IsInScene;
        static bool DontShowDisplayConfirmation = false;

        void OnInspectorUpdate()
        {
            Repaint();
        }


        [MenuItem("Window/Emerald AI/Setup Manager #%e", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldSetupManager), false, "Setup Manager");
            APS.minSize = new Vector2(300, 250f); //500
        }

        void OnEnable()
        {
            if (SettingsIcon == null) SettingsIcon = Resources.Load("Editor Icons/EmeraldSetupManager") as Texture;
        }

        void OnGUI()
        {
            if (ObjectToSetup != null) IsInScene = ObjectToSetup.scene.IsValid();
            else IsInScene = true;

            GUILayout.Space(10);
            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); //Top Left Side Indent
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "Setup Manager", SettingsIcon), TitleStyle);
            EditorGUIUtility.SetIconSize(new Vector2(16, 16));
            EditorGUILayout.EndVertical();
            GUILayout.Space(15);  //Top Right Side Indent
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15); //Bottom Left Side Indent
            EditorGUILayout.BeginVertical();
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription("Setup Settings", "Set the settings this AI wil be setup with. If you do not assign an Animation Profile or Sound Profile here, you will need to do so through their editors after the setup process has been complete.", true);
            
            if (ObjectToSetup == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("This field cannot be left blank.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            ObjectToSetup = (GameObject)EditorGUILayout.ObjectField("Object to Setup", ObjectToSetup, typeof(GameObject), true);
            if (!IsInScene) CustomEditorProperties.DisplayImportantMessage("The Setup Manager will not work with objects from the Project Tab. Please ensure your object is within the Scene before assigning it to the Setup Manager.");
            DescriptionElement("The object that the Setup Manager will create an AI with.");
            GUILayout.Space(10);


            AITag = EditorGUILayout.TagField("Tag for AI", AITag);
            DescriptionElement("The Unity Tag that will be applied to your AI. Note: Untagged cannot be used.");
            GUILayout.Space(10);

            AILayer = EditorGUILayout.LayerField("Layer for AI", AILayer);
            DescriptionElement("The Unity Layer that will be applied to your AI. Note: Default cannot be used.");
            GUILayout.Space(10);


            m_AnimationProfile = (AnimationProfile)EditorGUILayout.ObjectField("Animation Profile", m_AnimationProfile, typeof(AnimationProfile), false);
            DescriptionElement("(Optional) You can assign an Animation Profile to this AI that will be applied during the setup process.");
            GUILayout.Space(10);

            m_SoundProfile = (EmeraldSoundProfile)EditorGUILayout.ObjectField("Sound Profile", m_SoundProfile, typeof(EmeraldSoundProfile), false);
            DescriptionElement("(Optional) You can assign an Sound Profile to this AI that will be applied during the setup process.");
            GUILayout.Space(10);

            
            AnimatorType = (AnimatorTypeState)EditorGUILayout.EnumPopup("Animator Type", AnimatorType);
            DescriptionElement("Controls whether this AI will use Root Motion or NavMesh for its movement and speed.");
            GUILayout.Space(10);

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            //Displays the toggle settings that control which components are added to an object.
            DisplayToggleOptions();

            if (ObjectToSetup == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("You must have an object applied to the AI Object slot before you can complete the setup process.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            if (!IsInScene)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("The Setup Manager will not work with objects from the Project Tab. Please ensure your object is within the Scene before assigning it as the Object to Setup.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            EditorGUI.BeginDisabledGroup(ObjectToSetup == null || !IsInScene);
            if (GUILayout.Button("Setup AI", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Emerald Setup Manager", "Are you sure you'd like to setup an AI on this object?", "Setup", "Cancel"))
                {
                    UnpackPrefab(ObjectToSetup);
                    InitializeSetup();
                }
            }
            GUILayout.Space(25);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(15); //Bottom Right Side Indent
            GUILayout.EndHorizontal();

            if (DisplayConfirmation && !DontShowDisplayConfirmation)
            {
                if (EditorUtility.DisplayDialog("Setup Manager", "Your AI has been successfully created. Several components still need to be configured, which is indicated by a warning message at the top some components. " +
                    "These warning messages will tell you how to configure the components that need it.", "Okay", "Okay, Don'transform Show Again"))
                {
                    DisplayConfirmation = false;
                }
                else
                {
                    DisplayConfirmation = false;
                    DontShowDisplayConfirmation = true;
                }
            }
        }

        /// <summary>
        /// Unpack the passed GameObject if it is a prefab.
        /// </summary>
        void UnpackPrefab(GameObject ObjectToUnpack)
        {
            PrefabAssetType m_AssetType = PrefabUtility.GetPrefabAssetType(ObjectToUnpack);

            //Only unpack prefab if the ObjectToUnpack is a prefab.
            if (m_AssetType != PrefabAssetType.NotAPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(ObjectToUnpack, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        /// <summary>
        /// Displays the toggle settings that control which components are added to an object.
        /// </summary>
        void DisplayToggleOptions()
        {
            if (position.width > 500)
            {
                //Required
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                RequiredSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                //Optional
                GUILayout.Space(10);
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                OptionalSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(15);
            }
            else
            {
                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                RequiredSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();

                GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
                GUILayout.Space(-18);
                OptionalSettings();
                GUILayout.Space(5);
                EditorGUILayout.EndVertical();
            }
        }

        void RequiredSettings()
        {
            CustomEditorProperties.TextTitleWithDescription("(Required Components)", "The components that are required to make Emerald AI run. " +
                    "Enabled components will automatically be added to the assigned object during the setting up process. Each component has its own editor with various settings related to its functionality. You can hover over the ? icon for a tooltip description of each component.", true);
            m_EmeraldSystem = ToggleComponentElement(true, "Emerald System", "The base Emerald AI script that controls the management and updating off all other required scripts.");
            m_EmeraldAnimation = ToggleComponentElement(true, "Emerald Animation", "Handles the management of an AI's animations through Animation Profiles.");
            m_EmeraldCombat = ToggleComponentElement(true, "Emerald Combat", "Gives AI the ability to engage in combat, setup an AI's abilities, and other combat related settings.");
            m_EmeraldSounds = ToggleComponentElement(true, "Emerald Sounds", "Handles the management of an AI's sounds through Sound Profiles.");
            m_EmeraldMovement = ToggleComponentElement(true, "Emerald Movement", "Controls various movement, turn, and alignement related settings.");
            m_EmeraldHealth = ToggleComponentElement(true, "Emerald Health", "Gives AI the ability to take damage. Users can access this component through the IDamageable interface component.");
            m_EmeraldBehaviors = ToggleComponentElement(true, "Emerald Behaviors", "Gives AI the ability to use preset behaviors. Users can create custom behavior script derived from the EmeraldBehavior class, if desired.");
            m_EmeraldDetection = ToggleComponentElement(true, "Emerald Detection", "Gives AI the ability to see and detect targets.");
        }

        void OptionalSettings ()
        {
            CustomEditorProperties.TextTitleWithDescription("(Optional Components)", "The components that add optional features to Emerald AI. " +
                    "Enabled components will automatically be added to the assigned object during the setting up process. Each component has its own editor with various settings related to its functionality. You can hover over the ? icon for a tooltip description of each component.", true);
            m_EmeraldEvents = ToggleComponentElement(m_EmeraldEvents, "Emerald Events", "Adds the ability for users to create custom events for most of an AI's actions.", false);
            m_EmeraldItems = ToggleComponentElement(m_EmeraldItems, "Emerald Items", "Adds the ability for users to setup droppable items, equippable items, and more on their AI.", false);
            m_EmeraldInverseKinematics = ToggleComponentElement(m_EmeraldInverseKinematics, "Emerald Inverse Kinematics", "Adds the ability for AI to use Inverse Kinematics using Unity's Animation Rigging system.", false);
            m_EmeraldOptimization = ToggleComponentElement(m_EmeraldOptimization, "Emerald Optimization", "Adds the ability for AI to be disabled when culled or not in view of the camera. This increases performance by disabling AI that are not currently seen.", false);
            m_EmeraldUI = ToggleComponentElement(m_EmeraldUI, "Emerald UI", "Adds the ability for AI to use Emerald AI's built-in UI system which can display health bars and an AI's name above their head.", false);
            m_EmeraldFootsteps = ToggleComponentElement(m_EmeraldFootsteps, "Emerald Footsteps", "Adds the ability to play footstep sounds and effects based on the detected surfaces. Surfaces can be customized through Footstep Surface Objects. " +
                "Both textures from Unity Terrains and tags from game objects can be used.\n\nNote: Footstep Animation Events are required for footsteps to trigger. Ensure that these have been set up on your AI's movement animations.", false);
            m_EmeraldSoundDetector = ToggleComponentElement(m_EmeraldSoundDetector, "Emerald Sound Detector", "Adds the ability for AI to hear player targets and other sounds made by external sources.", false);
            m_EmeraldDebugger = ToggleComponentElement(m_EmeraldDebugger, "Emerald Debugger", "Adds the ability for AI to use and display debugging information (debug logs, debug lines, pathfinding information, etc). This can help with development or identifying possible issues.", false);
            m_TargetPositionModifier = ToggleComponentElement(m_TargetPositionModifier, "Target Position Modifier", "Adds the ability to modify the target height of targets allowing AI agents to target the modified position, which can improve targeting accuracy.", false);
        }

        void InitializeSetup()
        {
            if (ObjectToSetup != null && ObjectToSetup.GetComponent<EmeraldSystem>() == null && ObjectToSetup.GetComponent<Animator>() != null)
            {
                AssignComponents();
                SetupBoxCollider();
                SetupAudio();
                SetupTagsAndLayers();
                SetupMovement();
                AutoFindHeadTransform();

                if (!DontShowDisplayConfirmation)
                {
                    DisplayConfirmation = true;
                }

                ObjectToSetup = null;
            }
            else if (ObjectToSetup == null)
            {
                EditorUtility.DisplayDialog("Emerald Setup Manager - Oops!", "There is no object assigned to the AI Object slot. Please assign one and try again.", "Okay");
                return;
            }
            else if (ObjectToSetup.GetComponent<EmeraldSystem>() != null)
            {
                EditorUtility.DisplayDialog("Emerald Setup Manager - Oops!", "There is already an Emerald AI component on this object. Please choose another object to apply an Emerald AI component to.", "Okay");
                return;
            }
            else if (ObjectToSetup.GetComponent<Animator>() == null)
            {
                EditorUtility.DisplayDialog("Emerald Setup Manager - Oops!", "There is no Animator component attached to your AI. Please assign one and try again.", "Okay");
                return;
            }
        }

        /// <summary>
        /// Assigns and order the assigned componenets.
        /// </summary>
        void AssignComponents()
        {
            ObjectToSetup.AddComponent<EmeraldSystem>();
            if (m_EmeraldEvents) ObjectToSetup.AddComponent<EmeraldEvents>();
            if (m_EmeraldItems) ObjectToSetup.AddComponent<EmeraldItems>();
            if (m_EmeraldInverseKinematics) ObjectToSetup.AddComponent<EmeraldInverseKinematics>();
            if (m_EmeraldOptimization) ObjectToSetup.AddComponent<EmeraldOptimization>();
            if (m_EmeraldUI) ObjectToSetup.AddComponent<EmeraldUI>();
            if (m_EmeraldFootsteps) ObjectToSetup.AddComponent<EmeraldFootsteps>();
            if (m_EmeraldSoundDetector) ObjectToSetup.AddComponent<SoundDetection.EmeraldSoundDetector>();
            if (m_EmeraldDebugger) ObjectToSetup.AddComponent<EmeraldDebugger>();
            if (m_TargetPositionModifier) ObjectToSetup.AddComponent<TargetPositionModifier>();

            if (m_AnimationProfile != null) ObjectToSetup.GetComponent<EmeraldAnimation>().m_AnimationProfile = m_AnimationProfile;
            if (m_SoundProfile != null) ObjectToSetup.GetComponent<EmeraldSounds>().SoundProfile = m_SoundProfile;

            MoveToBottom(ObjectToSetup.GetComponent<EmeraldSystem>());
            MoveToBottom(ObjectToSetup.GetComponent<Animator>());
            MoveToBottom(ObjectToSetup.GetComponent<BoxCollider>());
            MoveToBottom(ObjectToSetup.GetComponent<UnityEngine.AI.NavMeshAgent>());
            MoveToBottom(ObjectToSetup.GetComponent<AudioSource>());
        }

        void MoveToBottom (Component ComponentToMove)
        {
            Component[] AllComponents = ObjectToSetup.GetComponents<Component>();

            for (int i = 0; i < AllComponents.Length; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(ComponentToMove);
            }
        }

        /// <summary>
        /// Sets up the AI's movement as some settings are dependent on Root Motion.
        /// </summary>
        void SetupMovement ()
        {
            EmeraldMovement EmeraldMovement = ObjectToSetup.GetComponent<EmeraldMovement>();
            EmeraldMovement.MovementType = (EmeraldMovement.MovementTypes)AnimatorType;
            if (EmeraldMovement.MovementType == EmeraldMovement.MovementTypes.RootMotion)
            {
                EmeraldMovement.StationaryTurningSpeedCombat = 30;
                EmeraldMovement.StationaryTurningSpeedNonCombat = 30;
            }
        }

        /// <summary>
        /// Sets up the AI's user set tags and layers.
        /// </summary>
        void SetupTagsAndLayers ()
        {
            ObjectToSetup.tag = AITag;
            ObjectToSetup.layer = AILayer;
            ObjectToSetup.GetComponent<EmeraldDetection>().DetectionLayerMask = (1 << LayerMask.NameToLayer("Water"));
        }

        /// <summary>
        /// Attempts to automatically find the AI's head transform.
        /// </summary>
        void AutoFindHeadTransform ()
        {
            foreach (Transform root in ObjectToSetup.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT") //Only look in the root transform - 3 child index in
                    {
                        foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                        {
                            if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD")) //Look for the word head within all transforms within the root transform
                            {
                                ObjectToSetup.GetComponent<EmeraldDetection>().HeadTransform = t;
                            }
                        }
                    }
                }
            }

            //If no head transform was found, the model may not have a root named bone, try again with less conditions.
            if (ObjectToSetup.GetComponent<EmeraldDetection>().HeadTransform == null)
            {
                foreach (Transform t in ObjectToSetup.GetComponentsInChildren<Transform>())
                {
                    //Ignore transforms with a MultiAimConstraint as these can be for the Animation Rigging system/constraint.
                    if (t.GetComponent<UnityEngine.Animations.Rigging.MultiAimConstraint>() == null)
                    {
                        if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD")) //Look for the word head within all transforms within all the transforms
                        {
                            ObjectToSetup.GetComponent<EmeraldDetection>().HeadTransform = t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the AI's audio and its defaul AudioSource settings.
        /// </summary>
        void SetupAudio ()
        {
            ObjectToSetup.GetComponent<AudioSource>().spatialBlend = 1;
            ObjectToSetup.GetComponent<AudioSource>().dopplerLevel = 0;
            ObjectToSetup.GetComponent<AudioSource>().rolloffMode = AudioRolloffMode.Linear;
            ObjectToSetup.GetComponent<AudioSource>().minDistance = 1;
            ObjectToSetup.GetComponent<AudioSource>().maxDistance = 50;
        }

        /// <summary>
        /// Sets up the AI's default box collider based on its main renderer's bounds.
        /// </summary>
        void SetupBoxCollider ()
        {
            List<SkinnedMeshRenderer> TempSkinnedMeshes = new List<SkinnedMeshRenderer>();
            List<float> TempSkinnedMeshBounds = new List<float>();

            foreach (SkinnedMeshRenderer SMR in ObjectToSetup.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!TempSkinnedMeshes.Contains(SMR))
                {
                    TempSkinnedMeshes.Add(SMR);
                    TempSkinnedMeshBounds.Add(SMR.bounds.size.sqrMagnitude);
                }
            }

            float m_LargestBounds = TempSkinnedMeshBounds.Max();
            var AIRenderer = TempSkinnedMeshes[TempSkinnedMeshBounds.IndexOf(m_LargestBounds)];

            ObjectToSetup.GetComponent<BoxCollider>().size = new Vector3(AIRenderer.bounds.size.x / 3 / ObjectToSetup.transform.localScale.y, AIRenderer.bounds.size.y / ObjectToSetup.transform.localScale.y, AIRenderer.bounds.size.z / 3 / ObjectToSetup.transform.localScale.y);
            ObjectToSetup.GetComponent<BoxCollider>().center = new Vector3(ObjectToSetup.GetComponent<BoxCollider>().center.x, ObjectToSetup.GetComponent<BoxCollider>().size.y / 2, ObjectToSetup.GetComponent<BoxCollider>().center.z);
        }

        bool ToggleComponentElement (bool Setting, string Name, string Description, bool RequiredComponent = true)
        {
            EditorGUI.BeginDisabledGroup(RequiredComponent);
            EditorGUILayout.BeginHorizontal();
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(Name));
            Setting = EditorGUILayout.ToggleLeft(new GUIContent(Name), Setting, GUILayout.ExpandWidth(false), GUILayout.Width(textDimensions.x + 13.5f));
            EditorGUILayout.LabelField(new GUIContent(EditorGUIUtility.IconContent("_Help").image, Description), GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(5);
            return Setting;
        }

        void DescriptionElement (string DescriptionText)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.HelpBox(DescriptionText, MessageType.None, true);
            GUI.backgroundColor = Color.white;
        }
    }
}
