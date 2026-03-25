using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Threading;
using System.Linq;

namespace EmeraldAI.Utility
{
    public class EmeraldAIDuplicatorManager : EditorWindow
    {
        public GameObject[] AIToDuplicateTo;
        SerializedProperty AIToDuplicateToProp;

        //Main Components
        public bool m_EmeraldSystem = true;
        public bool m_EmeraldAnimation = true;
        public bool m_EmeraldCombat = true;
        public bool m_EmeraldSounds = true;
        public bool m_EmeraldMovement = true;
        public bool m_EmeraldHealth = true;
        public bool m_EmeraldBehaviors = true;
        public bool m_EmeraldDetection = true;

        //Optional Components
        public bool m_EmeraldEvents = false;
        public bool m_EmeraldItems = false;
        public bool m_EmeraldInverseKinematics = false;
        public bool m_EmeraldOptimization = false;
        public bool m_EmeraldSoundDetector = false;
        public bool m_EmeraldUI = false;
        public bool m_EmeraldFootsteps = false;
        public bool m_EmeraldDebugger = false;
        public bool m_RagdollComponents = false;
        public bool m_LBDComponents = false;
        public bool m_TargetPositionModifier = false;

        GUIStyle TitleStyle;
        Texture AIDuplicatorIcon;
        Vector2 scrollPos;
        GameObject ReferenceAI;
        SerializedObject serializedObject;

        [MenuItem("Window/Emerald AI/AI Duplicator Manager #%d", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow APS = EditorWindow.GetWindow(typeof(EmeraldAIDuplicatorManager), false, "AI Duplicator Manager");
            APS.minSize = new Vector2(300, 250f); //500
        }

        void OnEnable()
        {
            if (AIDuplicatorIcon == null) AIDuplicatorIcon = Resources.Load("Editor Icons/EmeraldDuplicatorManager") as Texture;
            serializedObject = new SerializedObject(this);
            AIToDuplicateToProp = serializedObject.FindProperty("AIToDuplicateTo");
        }

        void OnGUI()
        {
            serializedObject.Update();
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.62f, 0.62f, 0.62f, 1f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15); //Top Left Side Indent
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(45));
            GUI.backgroundColor = Color.white;
            TitleStyle = CustomEditorProperties.UpdateTitleStyle();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            EditorGUILayout.LabelField(new GUIContent("    " + "AI Duplicator Manager", AIDuplicatorIcon), TitleStyle);
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
            CustomEditorProperties.TextTitleWithDescription("AI Duplicator Settings", "The AI Duplicator Manager is a powerful tool that allows you to duplicate AI from a Reference AI. This can greatly speed up development by not having to " +
                "reassign certain components that have already been already been set up on other AI. The AI Duplicator can even copy complex components and information such as weapon items, Inverse Kinematics, Ragdoll Components, and more.", true);

            if (ReferenceAI == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("This field cannot be left blank.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }
            ReferenceAI = (GameObject)EditorGUILayout.ObjectField("Reference AI", ReferenceAI, typeof(GameObject), true);
            DescriptionElement("The AI that will be referenced when copying to all other AI.");
            GUILayout.Space(10);
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            DisplayToggleOptions();

            //GUILayout.Space(10);

            GUI.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            EditorGUILayout.BeginVertical("Window", GUILayout.Height(50));
            GUILayout.Space(-18);
            CustomEditorProperties.TextTitleWithDescription("AI List", "You can drag and drop as many AI objects as needed to the list below. All objects within this list will have the data from the ReferenceAI applied to them.", true);
            EditorGUILayout.PropertyField(AIToDuplicateToProp);
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            DisplayDuplicateAIButton();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(15); //Bottom Right Side Indent
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
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
            m_EmeraldAnimation = ToggleComponentElement(m_EmeraldAnimation, "Emerald Animation", "Handles the management of an AI's animations through Animation Profiles.", false);
            m_EmeraldCombat = ToggleComponentElement(m_EmeraldCombat, "Emerald Combat", "Gives AI the ability to engage in combat, setup an AI's abilities, and other combat related settings.", false);
            m_EmeraldSounds = ToggleComponentElement(m_EmeraldSounds, "Emerald Sounds", "Handles the management of an AI's sounds through Sound Profiles.", false);
            m_EmeraldMovement = ToggleComponentElement(m_EmeraldMovement, "Emerald Movement", "Controls various movement, turn, and alignement related settings.", false);
            m_EmeraldHealth = ToggleComponentElement(m_EmeraldHealth, "Emerald Health", "Gives AI the ability to take damage. Users can access this component through the IDamageable interface component.", false);
            m_EmeraldBehaviors = ToggleComponentElement(m_EmeraldBehaviors, "Emerald Behaviors", "Gives AI the ability to use preset behaviors. Users can create custom behavior script derived from the EmeraldBehavior class, if desired.", false);
            m_EmeraldDetection = ToggleComponentElement(m_EmeraldDetection, "Emerald Detection", "Gives AI the ability to see and detect targets.", false);
        }

        void OptionalSettings()
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
            m_RagdollComponents = ToggleComponentElement(m_RagdollComponents, "Ragdoll Components", "Copies all ragdoll components (colliders, joints, and rigibodies).\n\nNote: Requires indentically named bones. If this condition is not me, this setting will be ignored.", false);
            m_LBDComponents = ToggleComponentElement(m_LBDComponents, "Location Based Damage Components", "Assigns and initializes a Location Based Damage component. Any matching colliders from the Reference AI will have the Damage Multiplier values copied." +
                "\n\nNote: Copying Damage Multiplier values requires indentically named bones. Bones that do not have the same name will not have the Damage Multiplier values copied.", false);
            m_TargetPositionModifier = ToggleComponentElement(m_TargetPositionModifier, "Target Position Modifier", "Adds the ability to modify the target height of targets allowing AI agents to target the modified position, which can improve targeting accuracy." +
                "\n\nNote: Requires indentically named bones. If this condition is not me, this setting will be ignored.", false);
        }

        void DisplayDuplicateAIButton ()
        {
            GUILayout.Space(15);
            if (ReferenceAI == null)
            {
                GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("You must have an object applied to the Reference AI slot before you can complete the setup process.", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
            }

            EditorGUI.BeginDisabledGroup(ReferenceAI == null);
            //GUI.backgroundColor = new Color(0.25f, 0.75f, 0.25f, 1f);
            if (GUILayout.Button("Duplicate AI", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("AI Duplicator Manager", "Are you sure you'd like to duplicate the selected settings to all AI within the AI List? This process cannot be undone.", "Yes", "Cancel"))
                {
                    DuplicateAI();
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(25);
            EditorGUI.EndDisabledGroup();
        }

        void DuplicateAI()
        {
            for (int i = 0; i < AIToDuplicateTo.Length; i++)
            {
                GameObject G = AIToDuplicateTo[i];
                //Display a progress bar to show the progress of the duplication process
                EditorUtility.DisplayProgressBar("Duplicating AI...", (i + 1) + " of " + (AIToDuplicateTo.Length + 1), (float)i / (float)AIToDuplicateTo.Length);

                if (AIToDuplicateTo[i].GetComponent<Animator>() != null)
                {
                    UnpackPrefab(AIToDuplicateTo[i]);

                    CopyAnimationComponent(G);
                    CopySoundsComponent(G);
                    CopyItemsComponent(G);
                    CopySoundDetectorComponent(G);
                    CopyUIComponent(G);
                    CopyFootstepsComponent(G);
                    CopyDebuggerComponent(G);
                    CopyTPMComponent(G);
                    CopyBehaviorsComponent(G);
                    CopyDetectionComponent(G);
                    CopyMovementComponent(G);
                    CopyHealthComponent(G);
                    CopyEmeraldBaseComponent(G);
                    CopyEventsComponent(G);
                    CopyCombatComponent(G);
                    CopyInverseKinematicsComponent(G);
                    CopyRagdollComponents(G);
                    CopyLBDComponent(G);
                    CopyOptimizationComponent(G);

                    CopyBoxCollider(G);
                    CopyAudioSource(G);
                    CopyTagAndLayer(G);

                    G.GetComponent<BoxCollider>().size = ReferenceAI.GetComponent<BoxCollider>().size;
                    G.GetComponent<BoxCollider>().center = ReferenceAI.GetComponent<BoxCollider>().center;

                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<EmeraldSystem>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<Animator>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<BoxCollider>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<UnityEngine.AI.NavMeshAgent>());
                    MoveToBottom(AIToDuplicateTo[i], AIToDuplicateTo[i].GetComponent<AudioSource>());
                }
                else
                {
                    Debug.Log("The '" + G.name + "' object does not have an Animator Controller on it so it has been skipped.");
                }
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Moves the passed component to the bottom of the inspector.
        /// </summary>
        void MoveToBottom(GameObject AIToDuplicateTo, Component ComponentToMove)
        {
            Component[] AllComponents = AIToDuplicateTo.GetComponents<Component>();

            for (int i = 0; i < AllComponents.Length; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(ComponentToMove);
            }
        }

        /// <summary>
        /// Unpack the passed GameObject if it is a prefab.
        /// </summary>
        void UnpackPrefab (GameObject ObjectToUnpack)
        {
            PrefabAssetType m_AssetType = PrefabUtility.GetPrefabAssetType(ObjectToUnpack);

            //Only unpack prefab if the ObjectToUnpack is a prefab.
            if (m_AssetType != PrefabAssetType.NotAPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(ObjectToUnpack, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        /// <summary>
        /// Copy an AI's Box Collider
        /// </summary>
        void CopyBoxCollider(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<BoxCollider>());

            if (G.GetComponent<BoxCollider>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<BoxCollider>());
            }
            else
            {
                G.AddComponent<BoxCollider>();
                ComponentUtility.PasteComponentValues(G.GetComponent<BoxCollider>());
            }
        }

        void CopyAudioSource (GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<AudioSource>());

            if (G.GetComponent<AudioSource>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<AudioSource>());
            }
            else
            {
                G.AddComponent<AudioSource>();
                ComponentUtility.PasteComponentValues(G.GetComponent<AudioSource>());
            }
        }

        void CopyTagAndLayer (GameObject G)
        {
            G.tag = ReferenceAI.tag;
            G.layer = ReferenceAI.layer;
        }

        void CopyEmeraldBaseComponent(GameObject G)
        {
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldSystem>());

            if (G.GetComponent<EmeraldSystem>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSystem>());
            }
            else
            {
                G.AddComponent<EmeraldSystem>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSystem>());
            }
        }

        void CopyMovementComponent(GameObject G)
        {
            if (!m_EmeraldMovement)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldMovement>());

            if (G.GetComponent<EmeraldMovement>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldMovement>());
            }
            else
            {
                G.AddComponent<EmeraldMovement>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldMovement>());
            }
        }

        void CopyBehaviorsComponent(GameObject G)
        {
            if (!m_EmeraldBehaviors)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldBehaviors>());

            if (G.GetComponent<EmeraldBehaviors>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldBehaviors>());
            }
            else
            {
                G.AddComponent<EmeraldBehaviors>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldBehaviors>());
            }
        }

        void CopyDetectionComponent (GameObject G)
        {
            if (!m_EmeraldDetection)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldDetection>());

            if (G.GetComponent<EmeraldDetection>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDetection>());
            }
            else
            {
                G.AddComponent<EmeraldDetection>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDetection>());
            }

            AutoFindHeadTransform(G);
        }

        void CopyAnimationComponent(GameObject G)
        {
            if (!m_EmeraldAnimation)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldAnimation>());

            if (G.GetComponent<EmeraldAnimation>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldAnimation>());
            }
            else
            {
                G.AddComponent<EmeraldAnimation>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldAnimation>());
            }

            //Cache the AI to copy's Avatar so it can be applied back to the duplicated Animator.
            Avatar m_Avatar = G.GetComponent<Animator>().avatar;
            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<Animator>());
            ComponentUtility.PasteComponentValues(G.GetComponent<Animator>());
            G.GetComponent<Animator>().avatar = m_Avatar;
        }

        void CopyCombatComponent(GameObject G)
        {
            if (!m_EmeraldCombat)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldCombat>());

            if (G.GetComponent<EmeraldCombat>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldCombat>());
            }
            else
            {
                G.AddComponent<EmeraldCombat>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldCombat>());
            }

            CopyAttackTransforms(G);
        }

        void CopySoundsComponent(GameObject G)
        {
            if (!m_EmeraldSounds)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldSounds>());

            if (G.GetComponent<EmeraldSounds>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSounds>());
            }
            else
            {
                G.AddComponent<EmeraldSounds>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldSounds>());
            }
        }

        void CopyHealthComponent(GameObject G)
        {
            if (!m_EmeraldHealth)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldHealth>());

            if (G.GetComponent<EmeraldHealth>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldHealth>());
            }
            else
            {
                G.AddComponent<EmeraldHealth>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldHealth>());
            }
        }

        void CopyEventsComponent(GameObject G)
        {
            if (!m_EmeraldEvents)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldEvents>());

            if (G.GetComponent<EmeraldEvents>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldEvents>());
            }
            else
            {
                G.AddComponent<EmeraldEvents>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldEvents>());
            }
        }

        void CopyItemsComponent(GameObject G)
        {
            if (!m_EmeraldItems)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldItems>());

            if (G.GetComponent<EmeraldItems>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldItems>());
            }
            else
            {
                G.AddComponent<EmeraldItems>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldItems>());
            }

            CopyWeaponObjects(G);
        }

        void CopySoundDetectorComponent(GameObject G)
        {
            if (!m_EmeraldSoundDetector)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<SoundDetection.EmeraldSoundDetector>());

            if (G.GetComponent<SoundDetection.EmeraldSoundDetector>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<SoundDetection.EmeraldSoundDetector>());
            }
            else
            {
                G.AddComponent<SoundDetection.EmeraldSoundDetector>();
                ComponentUtility.PasteComponentValues(G.GetComponent<SoundDetection.EmeraldSoundDetector>());
            }
        }

        void CopyUIComponent(GameObject G)
        {
            if (!m_EmeraldUI)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldUI>());

            if (G.GetComponent<EmeraldUI>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldUI>());
            }
            else
            {
                G.AddComponent<EmeraldUI>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldUI>());
            }
        }

        void CopyFootstepsComponent(GameObject G)
        {
            if (!m_EmeraldFootsteps)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldFootsteps>());

            if (G.GetComponent<EmeraldFootsteps>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldFootsteps>());
            }
            else
            {
                G.AddComponent<EmeraldFootsteps>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldFootsteps>());
            }

            CopyFeetTransforms(G);
        }

        void CopyFeetTransforms(GameObject G)
        {
            var m_FootstepsComponent = G.GetComponent<EmeraldFootsteps>();
            m_FootstepsComponent.FeetTransforms.Clear();
            var m_FootstepsReference = ReferenceAI.GetComponent<EmeraldFootsteps>();

            if (m_FootstepsReference != null)
            {
                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                {
                    for (int i = 0; i < m_FootstepsReference.FeetTransforms.Count; i++)
                    {
                        if (m_FootstepsReference.FeetTransforms[i].name == t.name)
                        {
                            m_FootstepsComponent.FeetTransforms.Add(t);
                        }
                    }
                }
            }
        }

        void CopyDebuggerComponent(GameObject G)
        {
            if (!m_EmeraldDebugger)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldDebugger>());

            if (G.GetComponent<EmeraldDebugger>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDebugger>());
            }
            else
            {
                G.AddComponent<EmeraldDebugger>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldDebugger>());
            }
        }

        void CopyTPMComponent(GameObject G)
        {
            if (!m_TargetPositionModifier)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<TargetPositionModifier>());

            if (G.GetComponent<TargetPositionModifier>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<TargetPositionModifier>());
            }
            else
            {
                G.AddComponent<TargetPositionModifier>();
                ComponentUtility.PasteComponentValues(G.GetComponent<TargetPositionModifier>());
            }

            CopyTPM(G);
        }

        void CopyTPM (GameObject G)
        {
            var m_TMPComponent = G.GetComponent<TargetPositionModifier>();
            var TMPReference = ReferenceAI.GetComponent<TargetPositionModifier>();

            if (TMPReference != null)
            {
                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                {
                    if (t.name == TMPReference.TransformSource.name)
                    {
                        m_TMPComponent.TransformSource = t;
                    }
                }
            }
        }

        void CopyInverseKinematicsComponent (GameObject G)
        {
            if (!m_EmeraldInverseKinematics)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldInverseKinematics>());

            if (G.GetComponent<EmeraldInverseKinematics>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldInverseKinematics>());
            }
            else
            {
                G.AddComponent<EmeraldInverseKinematics>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldInverseKinematics>());
            }

            CopyInverseKinematicsComponents(G);
        }

        void CopyRagdollComponents(GameObject G)
        {
            if (!m_RagdollComponents)
                return;

            CopyRagdollConfigurableJointComponents(G);
            CopyRagdollCharacterJointComponents(G);
        }

        void CopyLBDComponent(GameObject G)
        {
            if (!m_LBDComponents)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<LocationBasedDamage>());

            if (G.GetComponent<LocationBasedDamage>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<LocationBasedDamage>());
            }
            else
            {
                G.AddComponent<LocationBasedDamage>();
                ComponentUtility.PasteComponentValues(G.GetComponent<LocationBasedDamage>());
            }

            IntializeLBDComponent(G);
        }

        void CopyOptimizationComponent (GameObject G)
        {
            if (!m_EmeraldOptimization)
                return;

            ComponentUtility.CopyComponent(ReferenceAI.GetComponent<EmeraldOptimization>());

            if (G.GetComponent<EmeraldOptimization>() != null)
            {
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldOptimization>());
            }
            else
            {
                G.AddComponent<EmeraldOptimization>();
                ComponentUtility.PasteComponentValues(G.GetComponent<EmeraldOptimization>());
            }

            FindSingleRenderer(G);
        }

        void FindSingleRenderer (GameObject G)
        {
            EmeraldOptimization LocalOptimization = G.GetComponent<EmeraldOptimization>();
            EmeraldOptimization RefOptimization = ReferenceAI.GetComponent<EmeraldOptimization>();

            //Return for these conditions
            if (RefOptimization == null || RefOptimization.AIRenderer == null || RefOptimization.MeshType == EmeraldOptimization.MeshTypes.LODGroup)
                return;

            LocalOptimization.AIRenderer = null;

            //Get All children
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            for (int i = 0; i < AllChildTransforms.Count; i++)
            {
                if (AllChildTransforms[i].gameObject.name == RefOptimization.AIRenderer.gameObject.name)
                {
                    if (AllChildTransforms[i].gameObject.GetComponent<Renderer>() != null)
                        LocalOptimization.AIRenderer = AllChildTransforms[i].gameObject.GetComponent<Renderer>();
                }
            }

            if (LocalOptimization.AIRenderer == null)
            {
                Debug.Log("Could not find a Renderer match for the '" + G.name + "' object and the Reference AI. You will need to assign an AI Renderer manually through the Optimization Component for this object.");
            }
        }

        /// <summary>
        /// Copies all ragdoll components, given they use Configurable Joints.
        /// </summary>
        void CopyRagdollConfigurableJointComponents(GameObject G)
        {
            //Get
            List<ConfigurableJoint> ConfigurableJointList = new List<ConfigurableJoint>(ReferenceAI.GetComponentsInChildren<ConfigurableJoint>());

            //Apply
            var m_ConfigurableJoints = G.GetComponentsInChildren<ConfigurableJoint>().ToList();

            //Get All children
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            Thread.Sleep((25));

            if (m_ConfigurableJoints.Count == 0)
            {
                for (int i = 0; i < AllChildTransforms.Count; i++)
                {
                    if (ConfigurableJointList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name) && AllChildTransforms[i].gameObject.GetComponent<ConfigurableJoint>() == null)
                    {
                        AllChildTransforms[i].gameObject.AddComponent<ConfigurableJoint>();
                    }
                }

                m_ConfigurableJoints = G.GetComponentsInChildren<ConfigurableJoint>().ToList();
            }

            foreach (ConfigurableJoint C in m_ConfigurableJoints)
            {
                if (C != null && C.gameObject != G)
                {
                    if (C != null && ConfigurableJointList.Find(go => go.name == C.name))
                    {
                        var RefJoint = ConfigurableJointList.Find(go => go.name == C.name);

                        var LocalJoint = m_ConfigurableJoints.Find(go => go.name == RefJoint.name);

                        //Copy ConfigurableJoint values
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint);
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint);

                        //Copy Colliders from the reference AI and apply them to each local ragdoll component
                        if (RefJoint.GetComponent<Collider>() != null)
                        {
                            UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Collider>());
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Collider>());
                        }

                        //Find all Connected Bodies of the same name and apply them to the copied AI
                        if (RefJoint != null && RefJoint.connectedBody != null)
                        {
                            var m_ConnectedBody = AllChildTransforms.Find(go => go.name == RefJoint.connectedBody.transform.name);
                            LocalJoint.connectedBody = m_ConnectedBody.GetComponent<Rigidbody>();
                        }

                        //Copy Rigidbody values
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Rigidbody>());
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Rigidbody>());
                    }
                }
            }
        }

        /// <summary>
        /// Copies all ragdoll components, given they use Character Joints.
        /// </summary>
        void CopyRagdollCharacterJointComponents(GameObject G)
        {
            List<Rigidbody> RigidbodyList = new List<Rigidbody>();
            var m_RigidbodysAIToCopy = ReferenceAI.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody C in m_RigidbodysAIToCopy)
            {
                if (C != null)
                {
                    if (!RigidbodyList.Contains(C) && C.gameObject != ReferenceAI)
                    {
                        RigidbodyList.Add(C);
                    }
                }
            }

            List<CharacterJoint> CharacterJointList = new List<CharacterJoint>(ReferenceAI.GetComponentsInChildren<CharacterJoint>());

            //Apply
            var m_CharacterJoints = G.GetComponentsInChildren<CharacterJoint>().ToList();

            //Get All children
            List<Transform> AllChildTransforms = new List<Transform>(G.GetComponentsInChildren<Transform>());

            Thread.Sleep((25));

            if (m_CharacterJoints.Count == 0)
            {
                for (int i = 0; i < AllChildTransforms.Count; i++)
                {
                    if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name && go.GetComponent<CharacterJoint>() != null))
                    {
                        AllChildTransforms[i].gameObject.AddComponent<CharacterJoint>();
                    }
                    else if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name && go.GetComponent<CharacterJoint>() == null))
                    {
                        //Copy the components from the Root object as it should be the only internal object with a rigidbody and collider component
                        if (RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Rigidbody>() != null && RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Collider>() != null)
                        {
                            //Copy the rigidbody component
                            UnityEditorInternal.ComponentUtility.CopyComponent(RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Rigidbody>());

                            if (AllChildTransforms[i].gameObject.GetComponent<Rigidbody>() == null)
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(AllChildTransforms[i].gameObject);
                            }
                            else
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(AllChildTransforms[i].gameObject.GetComponent<Rigidbody>());
                            }

                            //Copy the collider component
                            UnityEditorInternal.ComponentUtility.CopyComponent(RigidbodyList.Find(go => go.gameObject.name == AllChildTransforms[i].gameObject.name).GetComponent<Collider>());

                            if (AllChildTransforms[i].gameObject.GetComponent<Collider>() == null)
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(AllChildTransforms[i].gameObject);
                            }
                            else
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(AllChildTransforms[i].gameObject.GetComponent<Collider>());
                            }
                        }
                    }
                }

                m_CharacterJoints = G.GetComponentsInChildren<CharacterJoint>().ToList();
            }

            foreach (CharacterJoint C in m_CharacterJoints)
            {
                if (C != null && C.gameObject != G)
                {
                    if (C != null && CharacterJointList.Find(go => go.name == C.name))
                    {
                        var RefJoint = CharacterJointList.Find(go => go.name == C.name);

                        var LocalJoint = m_CharacterJoints.Find(go => go.name == RefJoint.name);

                        //Copy CharacterJoint values
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint);
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint);

                        //Copy Colliders from AI to Copy
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Collider>());

                        //If they don'transform exist, add them as new components
                        if (LocalJoint.GetComponent<Collider>() == null)
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(LocalJoint.gameObject);
                        }
                        //If they do exist, copy the values
                        else
                        {
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Collider>());
                        }

                        //Find all Connected Bodies of the same name and apply them to the copied AI
                        if (RefJoint.connectedBody != null)
                        {
                            var m_ConnectedBody = AllChildTransforms.Where(go => go.name == RefJoint.connectedBody.transform.name).Single();
                            LocalJoint.connectedBody = m_ConnectedBody.GetComponent<Rigidbody>();
                        }

                        //Copy Rigidbody values
                        UnityEditorInternal.ComponentUtility.CopyComponent(RefJoint.GetComponent<Rigidbody>());
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(LocalJoint.GetComponent<Rigidbody>());
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to automatically find the AI's head transform.
        /// </summary>
        void AutoFindHeadTransform(GameObject G)
        {
            EmeraldDetection TempDetectionComponent = G.GetComponent<EmeraldDetection>();
            TempDetectionComponent.HeadTransform = null;

            foreach (Transform root in G.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT") //Only look in the root transform - 3 child indexes in
                    {
                        foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                        {
                            if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD")) //Look for the word head within all transforms within the root transform
                            {
                                TempDetectionComponent.HeadTransform = t;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copy any weapon objects an AI has and position them according to the ReferenceAI. Note: This will only work if the AI and the ReferenceAI share the same rigging.
        /// </summary>
        void CopyWeaponObjects(GameObject G)
        {
            var m_ItemComponent = G.GetComponent<EmeraldItems>();

            for (int i = 0; i < m_ItemComponent.Type1EquippableWeapons.Count; i++)
            {
                GameObject HolsteredReference = m_ItemComponent.Type1EquippableWeapons[i].HolsteredObject;

                if (HolsteredReference != null)
                {
                    GameObject HolsteredCopy = Instantiate(HolsteredReference, Vector3.zero, Quaternion.identity);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == HolsteredReference.transform.parent.name)
                        {
                            HolsteredCopy.transform.SetParent(t);
                            ComponentUtility.CopyComponent(HolsteredReference.transform);
                            ComponentUtility.PasteComponentValues(HolsteredCopy.transform);
                            HolsteredCopy.name = HolsteredReference.name;
                            m_ItemComponent.Type1EquippableWeapons[i].HolsteredObject = HolsteredCopy;
                        }
                    }
                }

                GameObject HeldReference = m_ItemComponent.Type1EquippableWeapons[i].HeldObject;

                if (HeldReference != null)
                {
                    GameObject HeldCopy = Instantiate(HeldReference, Vector3.zero, Quaternion.identity);

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == HeldReference.transform.parent.name)
                        {
                            HeldCopy.transform.SetParent(t);
                            ComponentUtility.CopyComponent(HeldReference.transform);
                            ComponentUtility.PasteComponentValues(HeldCopy.transform);
                            HeldCopy.name = HeldReference.name;
                            m_ItemComponent.Type1EquippableWeapons[i].HeldObject = HeldCopy;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copy any Attack Transform objects an AI has and position them according to the ReferenceAI. Note: This will only work if the AI and the ReferenceAI share the same rigging.
        /// </summary>
        void CopyAttackTransforms(GameObject G)
        {
            var m_EmeraldCombat = G.GetComponent<EmeraldCombat>();

            //Weapon Type 1 Attack Transforms
            for (int i = 0; i < m_EmeraldCombat.WeaponType1AttackTransforms.Count; i++)
            {
                var TransformReference = m_EmeraldCombat.WeaponType1AttackTransforms[i];

                if (TransformReference != null)
                {
                    var TransformCopy = Instantiate(TransformReference, Vector3.zero, Quaternion.identity);
                    TransformCopy.name = TransformReference.name;
                    TransformCopy.transform.SetParent(G.transform);    

                    foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name == TransformReference.parent.name)
                        {
                            TransformCopy.SetParent(t);
                            ComponentUtility.CopyComponent(TransformReference);
                            ComponentUtility.PasteComponentValues(TransformCopy.transform);
                            m_EmeraldCombat.WeaponType1AttackTransforms[i] = TransformCopy;
                        }
                    }
                }
            }

            //Weapon Type 2 Attack Transforms
            for (int i = 0; i < m_EmeraldCombat.WeaponType2AttackTransforms.Count; i++)
            {
                var TransformReference = m_EmeraldCombat.WeaponType2AttackTransforms[i];

                if (TransformReference != null)
                {
                    var TransformCopy = Instantiate(TransformReference, Vector3.zero, Quaternion.identity);
                    TransformCopy.name = TransformReference.name;
                    TransformCopy.transform.SetParent(G.transform);
                    ComponentUtility.CopyComponent(TransformReference);
                    ComponentUtility.PasteComponentValues(TransformCopy.transform);
                    m_EmeraldCombat.WeaponType2AttackTransforms[i] = TransformCopy;
                }
            }
        }

        /// <summary>
        /// Intialize the Location Based Damage component to assign the colliders from the AI that's being duplicated.
        /// </summary>
        void IntializeLBDComponent (GameObject G)
        {
            var m_LocationBasedDamageComponent = G.GetComponent<LocationBasedDamage>();
            m_LocationBasedDamageComponent.ColliderList.Clear();
            var m_ReferenceLBD = G.GetComponent<LocationBasedDamage>();
            var m_Colliders = G.GetComponentsInChildren<Collider>();
            m_LocationBasedDamageComponent.LBDComponentsLayer = m_ReferenceLBD.LBDComponentsLayer;

            foreach (Collider C in m_Colliders)
            {
                if (C != null)
                {
                    LocationBasedDamage.LocationBasedDamageClass lbdc = new LocationBasedDamage.LocationBasedDamageClass(C, 1);
                    if (!LocationBasedDamage.LocationBasedDamageClass.Contains(m_LocationBasedDamageComponent.ColliderList, lbdc) && C.gameObject != G)
                    {
                        m_LocationBasedDamageComponent.ColliderList.Add(lbdc);
                    }
                }
            }

            for (int i = 0; i < m_ReferenceLBD.ColliderList.Count; i++)
            {
                for (int j = 0; j < m_LocationBasedDamageComponent.ColliderList.Count; j++)
                {
                    if (m_ReferenceLBD.ColliderList[i].ColliderObject.name == m_LocationBasedDamageComponent.ColliderList[j].ColliderObject.name)
                    {
                        m_LocationBasedDamageComponent.ColliderList[j].DamageMultiplier = m_ReferenceLBD.ColliderList[i].DamageMultiplier;
                    }
                }
            }
        }

        void CopyInverseKinematicsComponents (GameObject G)
        {
            var m_InverseKinematicsComponent = G.GetComponent<EmeraldInverseKinematics>();
            var m_RigBuilderComponent = G.GetComponent<UnityEngine.Animations.Rigging.RigBuilder>();

            if (m_RigBuilderComponent != null)
                m_RigBuilderComponent.layers.Clear();

            for (int i = 0; i < m_InverseKinematicsComponent.UpperBodyRigsList.Count; i++)
            {
                if (m_InverseKinematicsComponent.UpperBodyRigsList[i] != null)
                {
                    var RigReference = m_InverseKinematicsComponent.UpperBodyRigsList[i];

                    if (RigReference != null)
                    {
                        var RigCopy = Instantiate(RigReference, Vector3.zero, Quaternion.identity);
                        RigCopy.name = RigReference.name;
                        RigCopy.transform.SetParent(G.transform);
                        ComponentUtility.CopyComponent(RigReference.transform);
                        ComponentUtility.PasteComponentValues(RigCopy.transform);
                        RigCopy.name = RigReference.name;
                        m_InverseKinematicsComponent.UpperBodyRigsList[i] = RigCopy;

                        if (m_RigBuilderComponent != null)
                        {
                            m_RigBuilderComponent.layers.Add(new UnityEngine.Animations.Rigging.RigLayer(m_InverseKinematicsComponent.UpperBodyRigsList[i]));
                        }
                        else
                        {
                            m_RigBuilderComponent = G.AddComponent<UnityEngine.Animations.Rigging.RigBuilder>();
                            m_RigBuilderComponent.layers.Add(new UnityEngine.Animations.Rigging.RigLayer(m_InverseKinematicsComponent.UpperBodyRigsList[i]));
                        }

                        //Copy MultiAimConstraints
                        var m_MultiAimConstraints = RigCopy.GetComponentsInChildren<UnityEngine.Animations.Rigging.MultiAimConstraint>();

                        for (int j = 0; j < m_MultiAimConstraints.Length; j++)
                        {
                            var m_SourceData = m_MultiAimConstraints[j].data;
                            Transform ConstrainedObjectReference = m_SourceData.constrainedObject;

                            foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == ConstrainedObjectReference.name)
                                {
                                    m_SourceData.constrainedObject = t;
                                    m_MultiAimConstraints[j].data = m_SourceData;
                                }
                            }
                        }

                        //Copy TwoBoneIKConstraint
                        var m_TwoBoneIKConstraint = RigCopy.GetComponentsInChildren<UnityEngine.Animations.Rigging.TwoBoneIKConstraint>();

                        for (int j = 0; j < m_TwoBoneIKConstraint.Length; j++)
                        {
                            var m_SourceData = m_TwoBoneIKConstraint[j].data;

                            //Source - Create a copy of the target and hint objects and place them on the same transform equivalent as the AI that's being copied to.
                            Transform HintObjectReference = m_SourceData.hint;

                            if (HintObjectReference != null)
                            {
                                Transform HintObjectCopy = Instantiate(m_SourceData.hint, Vector3.zero, Quaternion.identity);
                                HintObjectCopy.name = m_SourceData.target.name;

                                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == HintObjectReference.parent.name)
                                    {
                                        HintObjectCopy.SetParent(t);
                                        ComponentUtility.CopyComponent(HintObjectReference.transform);
                                        ComponentUtility.PasteComponentValues(HintObjectCopy.transform);
                                        m_SourceData.hint = HintObjectCopy;
                                    }
                                }
                            }

                            Transform TargetObjectReference = m_SourceData.target;

                            if (TargetObjectReference != null)
                            {
                                Transform TargetObjectCopy = Instantiate(m_SourceData.target, Vector3.zero, Quaternion.identity);
                                TargetObjectCopy.name = m_SourceData.target.name;

                                foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == TargetObjectReference.parent.name)
                                    {
                                        TargetObjectCopy.SetParent(t);
                                        ComponentUtility.CopyComponent(TargetObjectReference.transform);
                                        ComponentUtility.PasteComponentValues(TargetObjectCopy.transform);
                                        m_SourceData.target = TargetObjectCopy;
                                    }
                                }
                            }

                            //Bones - Find the transform equivalent bones from the ReferenceAI and assign them to the AI that's being copied to.
                            Transform RootObjectReference = m_SourceData.root;
                            Transform MidObjectReference = m_SourceData.mid;
                            Transform TipObjectReference = m_SourceData.tip;

                            //Loop through all bones within the AI that's being copied to and find the same bones as the ReferenceAI using the transform name
                            foreach (Transform t in G.transform.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == RootObjectReference.name)
                                {
                                    m_SourceData.root = t; 
                                }
                                else if (t.name == MidObjectReference.name)
                                {
                                    m_SourceData.mid = t;
                                }
                                else if (t.name == TipObjectReference.name)
                                {
                                    m_SourceData.tip = t;
                                }
                            }

                            //Apply the sourcedata back to the TwoBoneIKConstraint.
                            m_TwoBoneIKConstraint[j].data = m_SourceData;
                        }
                    }
                }
            }
        }

        bool ToggleComponentElement(bool Setting, string Name, string Description, bool RequiredComponent = true)
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

        void DescriptionElement(string DescriptionText)
        {
            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.19f);
            EditorGUILayout.HelpBox(DescriptionText, MessageType.None, true);
            GUI.backgroundColor = Color.white;
        }
    }
}