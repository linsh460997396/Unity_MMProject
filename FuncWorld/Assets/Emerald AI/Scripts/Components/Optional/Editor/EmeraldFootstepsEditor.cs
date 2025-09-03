using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace EmeraldAI.Utility
{
    [CustomEditor(typeof(EmeraldFootsteps))]
    [CanEditMultipleObjects]
    public class EmeraldFootstepsEditor : Editor
    {
        GUIStyle FoldoutStyle;
        Texture FootstepsEditorIcon;

        //Bools
        SerializedProperty HideSettingsFoldout, FootstepsFoldout, SurfaceFoldout;

        //Variables
        SerializedProperty FootstepSurfaces, IgnoreLayers, FeetTransforms;

        void OnEnable()
        {
            if (FootstepsEditorIcon == null) FootstepsEditorIcon = Resources.Load("Editor Icons/EmeraldFootsteps") as Texture;
            InitializeProperties();
        }

        void InitializeProperties()
        {
            //Bools
            HideSettingsFoldout = serializedObject.FindProperty("HideSettingsFoldout");
            FootstepsFoldout = serializedObject.FindProperty("FootstepsFoldout");
            SurfaceFoldout = serializedObject.FindProperty("SurfaceFoldout");

            //Variables
            FootstepSurfaces = serializedObject.FindProperty("FootstepSurfaces");
            IgnoreLayers = serializedObject.FindProperty("IgnoreLayers");
            FeetTransforms = serializedObject.FindProperty("FeetTransforms");
        }

        void DisplayWarningMessages (EmeraldFootsteps self)
        {
            if (self.FeetTransforms.Count == 0)
            {
                CustomEditorProperties.DisplaySetupWarning("The Feet Transforms list is empty, please assign your AI's feet transforms to Feet Transforms list.");
            }
            else if (self.FootstepSurfaces.Count == 0)
            {
                CustomEditorProperties.DisplaySetupWarning("The Footstep Surfaces list is empty, please assign at least 1 Footstep Surface Object.");
            }
        }

        public override void OnInspectorGUI()
        {
            FoldoutStyle = CustomEditorProperties.UpdateEditorStyles();
            EmeraldFootsteps self = (EmeraldFootsteps)target;
            serializedObject.Update();

            CustomEditorProperties.BeginScriptHeaderNew("Footsteps", FootstepsEditorIcon, new GUIContent(), HideSettingsFoldout);

            DisplayWarningMessages(self);

            if (!HideSettingsFoldout.boolValue)
            {
                EditorGUILayout.Space();
                FootstepSettings(self);
                EditorGUILayout.Space();
                SurfaceSettings(self);
                EditorGUILayout.Space();
            }

            CustomEditorProperties.EndScriptHeader();

            serializedObject.ApplyModifiedProperties();
        }

        void FootstepSettings(EmeraldFootsteps self)
        {
            FootstepsFoldout.boolValue = EditorGUILayout.Foldout(FootstepsFoldout.boolValue, "Footstep Settings", true, FoldoutStyle);

            if (FootstepsFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Foostep Settings", "Controls various settings for the Foosteps component.", false);
                CustomEditorProperties.ImportantTutorialButton("You will need to create Footstep Animation Events on your AI's animations in order for footsteps to trigger.", "https://black-horizon-studios.gitbook.io/emerald-ai-wiki/emerald-components-optional/footsteps-component/setting-up-the-footsteps-component");
                EditorGUILayout.Space();

                CustomEditorProperties.CustomPropertyField(IgnoreLayers, "Ignore Layers", "Controls which layers will be ignored when calculating footsteps. Your AI's own layer and its LBD's Collider Layer will automatically be included during runtime.", true);
                EditorGUILayout.Space();

                if (GUILayout.Button(new GUIContent("Auto Grab Feet Transforms", "Attemps to automatically grab the AI's feet transforms.")))
                {
                    GetFeetTransforms(self);
                }

                EditorGUILayout.Space();
                CustomEditorProperties.BeginIndent(12); 
                CustomEditorProperties.CustomHelpLabelField("Controls the transforms for this AI's feet. Raycasts will be fired from these points to calculate footsteps. If Step Effects are used, they will be spawned at the position of the foot closest to the ground.", false);
                EditorGUILayout.PropertyField(FeetTransforms);
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void SurfaceSettings(EmeraldFootsteps self)
        {
            SurfaceFoldout.boolValue = EditorGUILayout.Foldout(SurfaceFoldout.boolValue, "Surface Settings", true, FoldoutStyle);

            if (SurfaceFoldout.boolValue)
            {
                CustomEditorProperties.BeginFoldoutWindowBox();
                CustomEditorProperties.TextTitleWithDescription("Surface Settings", "Controls the Footstep Surface Objects that will be used. A Footstep Surface Object can be created by right clicking in the Project tab and going to Create>Emerald AI>Footstep Surface Object.", true);

                CustomEditorProperties.CustomHelpLabelField("A list of Footstep Surfaces used to determine which footstep sound and effect should play given the received information.", false);
                CustomEditorProperties.BeginIndent(12);
                EditorGUILayout.PropertyField(FootstepSurfaces);
                EditorGUILayout.Space();
                CustomEditorProperties.EndIndent();
                EditorGUILayout.Space();

                CustomEditorProperties.EndFoldoutWindowBox();
            }
        }

        void GetFeetTransforms (EmeraldFootsteps self)
        {
            //Search all the transforms within an AI and look for the word root
            foreach (Transform t in self.GetComponentsInChildren<Transform>())
            {
                if (t.name.Contains("foot") || t.name.Contains("Foot") || t.name.Contains("FOOT")) //Look for the word foot within all transforms within the AI
                {
                    if (!t.name.Contains("ik") && !t.name.Contains("Ik") && !t.name.Contains("IK") && !t.name.Contains("Foot Collider"))
                    {
                        if (!self.FeetTransforms.Contains(t))
                        {
                            self.FeetTransforms.Add(t);
                        }
                    }
                }
            }

            foreach (Transform root in self.GetComponentsInChildren<Transform>())
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i < root.childCount && root.GetChild(i).name == "root" || i < root.childCount && root.GetChild(i).name == "Root" || i < root.childCount && root.GetChild(i).name == "ROOT") //Only look in the root transform - 3 child index in
                    {
                        foreach (Transform t in root.GetChild(i).GetComponentsInChildren<Transform>())
                        {
                            if (t.name.Contains("foot") || t.name.Contains("Foot") || t.name.Contains("FOOT")) //Look for the word foot within all transforms within the AI
                            {
                                //Exclude transforms with IK, as well as the word Foot Collider, as these aren'transform usually bone transforms.
                                if (!t.name.Contains("ik") && !t.name.Contains("Ik") && !t.name.Contains("IK") && !t.name.Contains("Foot Collider"))
                                {
                                    if (!self.FeetTransforms.Contains(t))
                                    {
                                        self.FeetTransforms.Add(t);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}