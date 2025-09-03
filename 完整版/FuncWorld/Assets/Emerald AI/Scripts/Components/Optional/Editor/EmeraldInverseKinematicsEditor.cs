using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using UnityEngine.Animations.Rigging;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldInverseKinematics))]
    [CanEditMultipleObjects]
    public class EmeraldInverseKinematicsEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture InverseKinematicsEditorIcon;

        SerializedProperty WanderingLookAtLimit, WanderingLookSpeed, WanderingLookDistance, WanderingLookHeightOffset, CombatLookAtLimit, CombatLookSpeed, CombatLookDistance, CombatLookHeightOffset;
        SerializedProperty HideSettingsFoldout, GeneralIKSettingsFoldout, RigSettingsFoldout;
        ReorderableList UpperBodyRigsList;

        private void OnEnable()
        {
            if (InverseKinematicsEditorIcon == null) InverseKinematicsEditorIcon = Resources.Load("Editor Icons/EmeraldInverseKinematics") as Texture;
            InitializeProperties(); 
            InitializeLists();
        }

        void InitializeProperties ()
        {
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            GeneralIKSettingsFoldout = serializedObject.FindProperty("GeneralIKSettingsFoldout");
            RigSettingsFoldout = serializedObject.FindProperty("RigSettingsFoldout");

            WanderingLookAtLimit = serializedObject.FindProperty("WanderingLookAtLimit");
            WanderingLookSpeed = serializedObject.FindProperty("WanderingLookSpeed");
            WanderingLookDistance = serializedObject.FindProperty("WanderingLookDistance");
            WanderingLookHeightOffset = serializedObject.FindProperty("WanderingLookHeightOffset");
            CombatLookAtLimit = serializedObject.FindProperty("CombatLookAtLimit");
            CombatLookSpeed = serializedObject.FindProperty("CombatLookSpeed");
            CombatLookDistance = serializedObject.FindProperty("CombatLookDistance");
            CombatLookHeightOffset = serializedObject.FindProperty("CombatLookHeightOffset");
        }

        void InitializeLists ()
        {
            UpperBodyRigsList = new ReorderableList(serializedObject, serializedObject.FindProperty("UpperBodyRigsList"), true, true, true, true);
            UpperBodyRigsList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Upper Body Rigs List", EditorStyles.boldLabel);
            };
            UpperBodyRigsList.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = UpperBodyRigsList.serializedProperty.GetArrayElementAtIndex(index);
                    EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                };
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldInverseKinematics self = (EmeraldInverseKinematics)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Inverse Kinematics", InverseKinematicsEditorIcon, new GUIContent(), HideSettingsFoldout);

            MissingRigMessage(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                RigSettings(self);
                EditorGUILayout.Space();
                GeneralIKSettings(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Displays a missing rig message within the EmeraldInverseKinematicsEditor.
        /// </summary>
        void MissingRigMessage(EmeraldInverseKinematics self)
        {
            if (self.UpperBodyRigsList.Count == 0)
            {
                CustomEditorProperties.DisplaySetupWarning("This AI doesn'transform any applied Rigs so they will not be controlled by this IK component. Please add the Rigs you would like to be controlled to the UpperBodyRigsList within the Rig Settings Foldout.");
            }
        }

        void RigSettings(EmeraldInverseKinematics self)
        {
            RigSettingsFoldout.boolValue = CustomEditorProperties.Foldout(RigSettingsFoldout.boolValue, "Rig Settings", true, FoldoutStyle);

            if (RigSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("Rig Settings", "Assign all Rig components this AI will be using for its IK. Upper Body Rigs will be used for looking and aiming at targets. Lower Body Rigs will be used for positioning an AI's " +
                    "legs and feet with the ground. You will be able to control these settings further with the settings below", true);

                if (GUILayout.Button(new GUIContent("Create New Rig", "Creates a new Rig, in addition to any others that have been created, and automatically assigns and parents it to this AI." +
                    "\n\nNote: You will still need to add a constraint component on a child object within your Rig.")))
                {
                    CustomRigSetup(self);
                }

                EditorGUILayout.Space();
                CustomEditorProperties.CustomHelpLabelField("The Upper Body Rig components (Head, Spine, Chest, Arms, etc.) that this AI uses for its aiming and looking IK.", false);
                UpperBodyRigsList.DoLayoutList();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void CustomRigSetup (EmeraldInverseKinematics self)
        {
            var rigBuilder = self.transform.GetComponent<RigBuilder>();

            if (rigBuilder == null)
                rigBuilder = Undo.AddComponent<RigBuilder>(self.transform.gameObject);
            else
                Undo.RecordObject(rigBuilder, "Rig Builder Component Added.");

            var name = "Rig";
            var cnt = 1;
            while (rigBuilder.transform.Find(string.Format("{0} {1}", name, cnt)) != null)
            {
                cnt++;
            }
            name = string.Format("{0} {1}", name, cnt);
            var rigGameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(rigGameObject, name);
            rigGameObject.transform.SetParent(rigBuilder.transform);
            rigGameObject.transform.localPosition = Vector3.zero;
            rigGameObject.transform.localScale = Vector3.one;

            var rig = Undo.AddComponent<Rig>(rigGameObject);
            rigBuilder.layers.Add(new RigLayer(rig));

            if (PrefabUtility.IsPartOfPrefabInstance(rigBuilder))
                EditorUtility.SetDirty(rigBuilder);

            self.UpperBodyRigsList.Add(rig);
        }

        void GeneralIKSettings (EmeraldInverseKinematics self)
        {
            GeneralIKSettingsFoldout.boolValue = CustomEditorProperties.Foldout(GeneralIKSettingsFoldout.boolValue, "General IK Settings", true, FoldoutStyle);

            if (GeneralIKSettingsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();

                CustomEditorProperties.TextTitleWithDescription("General IK Settings", "Controls the speeds and angles for this AI's IK.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), WanderingLookAtLimit, "Wandering Angle Limit", 1, 90);
                CustomEditorProperties.CustomHelpLabelField("Controls the angle limit for looking at targets.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatLookAtLimit, "Combat Angle Limit", 1, 90);
                CustomEditorProperties.CustomHelpLabelField("Controls the angle limit for looking at targets.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WanderingLookSpeed, "Wandering Look Speed", 1f, 15f);
                CustomEditorProperties.CustomHelpLabelField("Controls how quickly an AI will look at targets.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), CombatLookSpeed, "Combat Look Speed", 1f, 15f);
                CustomEditorProperties.CustomHelpLabelField("Controls how quickly an AI will look at targets.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), WanderingLookDistance, "Wandering Look Distance", 5, 40);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance for looking at targets.", true);

                CustomEditorProperties.CustomIntSlider(new Rect(), new GUIContent(), CombatLookDistance, "Combat Look Distance", 5, 40);
                CustomEditorProperties.CustomHelpLabelField("Controls the distance for looking at targets.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), WanderingLookHeightOffset, "Wandering Look Height Offset", -3f, 3f);
                CustomEditorProperties.CustomHelpLabelField("Controls the height offset for the default position when looking at targets.", true);

                CustomEditorProperties.CustomFloatSlider(new Rect(), new GUIContent(), CombatLookHeightOffset, "Combat Look Height Offset", -3f, 3f);
                CustomEditorProperties.CustomHelpLabelField("Controls the height offset for the default position when looking at targets.", true);

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }
    }
}