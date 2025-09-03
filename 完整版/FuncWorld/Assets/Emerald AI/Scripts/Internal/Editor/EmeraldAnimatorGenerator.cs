using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.IO;

namespace EmeraldAI.Utility
{
    public class EmeraldAnimatorGenerator
    {
        /// <summary>
        /// Creates a unique copy of the passed Animation Profile and clears the copy's Animator Controller.
        /// </summary>
        public static void CopyAnimationProfile (AnimationProfile AnimationProfileRef)
        {
            var SerializedReferenceAP = new SerializedObject(AnimationProfileRef);

            SerializedReferenceAP.Update();

            //Get the path of the selected asset
            string assetPath = AssetDatabase.GetAssetPath(AnimationProfileRef);
            string assetDirectory = Path.GetDirectoryName(assetPath);
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string assetExtension = Path.GetExtension(assetPath);

            //Create a unique path for the duplicate asset
            string duplicateAssetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(assetDirectory, assetName + " Copy" + assetExtension));

            //Duplicate the asset
            AssetDatabase.CopyAsset(assetPath, duplicateAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //Select the duplicated asset in the Project window
            UnityEngine.Object duplicateAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(duplicateAssetPath);
            Selection.activeObject = duplicateAsset;

            //Clear the copy's Animator Controller.
            AnimationProfile AnimationProfileCopy = AssetDatabase.LoadAssetAtPath<AnimationProfile>(duplicateAssetPath);
            var SerializedCopyAP = new SerializedObject(AnimationProfileCopy);
            SerializedCopyAP.Update();
            AnimationProfileCopy.AIAnimator = null;
            AnimationProfileCopy.AnimatorControllerGenerated = false;
            AnimationProfileCopy.FilePath = "";
            SerializedCopyAP.ApplyModifiedProperties();

            SerializedReferenceAP.ApplyModifiedProperties();

            EditorUtility.SetDirty(AnimationProfileCopy);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Generates an Animator Controller using the passed EmeraldAnimation values. This is done by searching each state by name and applying the proper animation, animation speed, and mirroring values.
        /// </summary>
        public static void GenerateAnimatorController(AnimationProfile EmeraldAnimationComponent)
        {
            AnimatorController m_AnimatorController = EmeraldAnimationComponent.AIAnimator as AnimatorController;

            ApplyNonCombatAnimations(EmeraldAnimationComponent, m_AnimatorController);
            ApplyType1Animations(EmeraldAnimationComponent, m_AnimatorController);
            ApplyType2Animations(EmeraldAnimationComponent, m_AnimatorController);
            
            EmeraldAnimationComponent.AnimatorControllerGenerated = true;
            EmeraldAnimationComponent.AnimationsUpdated = false;
            EmeraldAnimationComponent.AnimationListsChanged = false;
        }

        /// <summary>
        /// Set each sub-state's states with an animation from the passed AnimationClass list.
        /// </summary>
        static void SetState(ChildAnimatorStateMachine childAnimatorStateMachine, string stateName, ChildAnimatorState[] childAnimatorState, List<AnimationClass> animationList)
        {
            if (childAnimatorStateMachine.stateMachine.name == stateName)
            {
                for (int f = 0; f < animationList.Count; f++)
                {
                    childAnimatorState[f].state.motion = animationList[f].AnimationClip;
                    childAnimatorState[f].state.speed = animationList[f].AnimationSpeed;
                }

                //Set each state that's greater than the passed list count to null so it is no longer preset on the state.
                for (int f = animationList.Count; f < childAnimatorState.Length; f++)
                {
                    childAnimatorState[f].state.motion = null;
                }
            }
        }

        /// <summary>
        /// Assigns all non-combat animations to the Animator Controller.
        /// </summary>
        static void ApplyNonCombatAnimations(AnimationProfile EmeraldAnimationComponent, AnimatorController m_AnimatorController)
        {
            //Go through each sub-state by name and assign the animation to each state within the sub-state using an index
            for (int i = 0; i < m_AnimatorController.layers[0].stateMachine.stateMachines.Length; i++)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine = m_AnimatorController.layers[0].stateMachine.stateMachines[i];
                ChildAnimatorState[] childAnimatorState = m_AnimatorController.layers[0].stateMachine.stateMachines[i].stateMachine.states;

                SetState(childAnimatorStateMachine, "Idle States", childAnimatorState, EmeraldAnimationComponent.NonCombatAnimations.IdleList);
                SetState(childAnimatorStateMachine, "Hit States", childAnimatorState, EmeraldAnimationComponent.NonCombatAnimations.HitList);
                SetState(childAnimatorStateMachine, "Death States", childAnimatorState, EmeraldAnimationComponent.NonCombatAnimations.DeathList);

                if (m_AnimatorController.layers[0].stateMachine.stateMachines[i].stateMachine.name == "Emote States")
                {
                    for (int f = 0; f < EmeraldAnimationComponent.EmoteAnimationList.Count; f++)
                    {
                        m_AnimatorController.layers[0].stateMachine.stateMachines[i].stateMachine.states[f].state.motion = EmeraldAnimationComponent.EmoteAnimationList[f].EmoteAnimationClip;
                        //m_AnimatorController.layers[0].stateMachine.stateMachines[i].stateMachine.states[f].state.speed = EmeraldAnimationComponent.EmoteAnimationList[f].AnimationSpeed; //TODO: Need speed parameter
                    }
                }
            }

            //Go through each state by name and assign the animation to the proper state
            for (int i = 0; i < m_AnimatorController.layers[0].stateMachine.states.Length; i++)
            {
                if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Turn Left")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.NonCombatAnimations.TurnLeft.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.NonCombatAnimations.TurnLeft.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.NonCombatAnimations.TurnLeft.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Turn Right")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.NonCombatAnimations.TurnRight.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.NonCombatAnimations.TurnRight.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.NonCombatAnimations.TurnRight.AnimationSpeed;
                }
            }

            //Get and assign Movement Blend Tree animations
            AnimatorState m_StateMachine = m_AnimatorController.layers[0].stateMachine.states[0].state;
            BlendTree MovementBlendTree = m_StateMachine.motion as BlendTree;

            var SerializedIdleBlendTreeRef = new SerializedObject(MovementBlendTree);
            var MovementBlendTreeChildren = SerializedIdleBlendTreeRef.FindProperty("m_Childs");

            //Assign the Idle animation and settings to the Idle Blend Tree
            var MovementMotionSlot1 = MovementBlendTreeChildren.GetArrayElementAtIndex(0);
            var MovementMotion1 = MovementMotionSlot1.FindPropertyRelative("m_Motion");
            BlendTree IdleBlendTree = MovementMotion1.objectReferenceValue as BlendTree;
            var SerializedIdleBlendTree = new SerializedObject(IdleBlendTree);
            var IdleBlendTreeChildren = SerializedIdleBlendTree.FindProperty("m_Childs");
            var IdleMotionSlot = IdleBlendTreeChildren.GetArrayElementAtIndex(0);
            var IdleAnimation = IdleMotionSlot.FindPropertyRelative("m_Motion");
            var IdleAnimationSpeed = IdleMotionSlot.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.NonCombatAnimations.IdleStationary.AnimationSpeed != 0) IdleAnimationSpeed.floatValue = EmeraldAnimationComponent.NonCombatAnimations.IdleStationary.AnimationSpeed;
            IdleAnimation.objectReferenceValue = EmeraldAnimationComponent.NonCombatAnimations.IdleStationary.AnimationClip;
            SerializedIdleBlendTree.ApplyModifiedProperties();

            //Assign the Walk animations and settings to the Walk Blend Tree; one for walk left, walk straight, and walk right.
            var MovementMotionSlot2 = MovementBlendTreeChildren.GetArrayElementAtIndex(1);
            var MovementMotion2 = MovementMotionSlot2.FindPropertyRelative("m_Motion");
            BlendTree WalkBlendTree = MovementMotion2.objectReferenceValue as BlendTree;
            var SerializedWalkBlendTree = new SerializedObject(WalkBlendTree);
            var WalkBlendTreeChildren = SerializedWalkBlendTree.FindProperty("m_Childs");

            //Adjust the non-combat movement thresholds depending on which Animator Type is being used.
            var WalkMovementMotionThreshold = MovementBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Threshold");
            var RunMovementMotionThreshold = MovementBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Threshold");

            SerializedIdleBlendTreeRef.ApplyModifiedProperties();

            //Walk Left
            var WalkMotionSlot1 = WalkBlendTreeChildren.GetArrayElementAtIndex(0);
            var WalkLeftAnimation = WalkMotionSlot1.FindPropertyRelative("m_Motion");
            var WalkLeftAnimationSpeed = WalkMotionSlot1.FindPropertyRelative("m_TimeScale");
            var WalkLeftMirror = WalkMotionSlot1.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.NonCombatAnimations.WalkLeft.AnimationSpeed != 0) WalkLeftAnimationSpeed.floatValue = EmeraldAnimationComponent.NonCombatAnimations.WalkLeft.AnimationSpeed;
            WalkLeftAnimation.objectReferenceValue = EmeraldAnimationComponent.NonCombatAnimations.WalkLeft.AnimationClip;
            WalkLeftMirror.boolValue = EmeraldAnimationComponent.NonCombatAnimations.WalkLeft.Mirror;

            //Walk Straight
            var WalkMotionSlot2 = WalkBlendTreeChildren.GetArrayElementAtIndex(1);
            var WalkStraightAnimation = WalkMotionSlot2.FindPropertyRelative("m_Motion");
            var WalkStraightAnimationSpeed = WalkMotionSlot2.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.NonCombatAnimations.WalkForward.AnimationSpeed != 0) WalkStraightAnimationSpeed.floatValue = EmeraldAnimationComponent.NonCombatAnimations.WalkForward.AnimationSpeed;
            WalkStraightAnimation.objectReferenceValue = EmeraldAnimationComponent.NonCombatAnimations.WalkForward.AnimationClip;

            //Walk Right
            var WalkMotionSlot3 = WalkBlendTreeChildren.GetArrayElementAtIndex(2);
            var WalkRightAnimation = WalkMotionSlot3.FindPropertyRelative("m_Motion");
            var WalkRightAnimationSpeed = WalkMotionSlot3.FindPropertyRelative("m_TimeScale");
            var WalkRightMirror = WalkMotionSlot3.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.NonCombatAnimations.WalkRight.AnimationSpeed != 0) WalkRightAnimationSpeed.floatValue = EmeraldAnimationComponent.NonCombatAnimations.WalkRight.AnimationSpeed;
            WalkRightAnimation.objectReferenceValue = EmeraldAnimationComponent.NonCombatAnimations.WalkRight.AnimationClip;
            WalkRightMirror.boolValue = EmeraldAnimationComponent.NonCombatAnimations.WalkRight.Mirror;

            SerializedWalkBlendTree.ApplyModifiedProperties();

            //Assign the Run animations and settings to the Run Blend Tree; one for run left, run straight, and run right.
            var MovementMotionSlot3 = MovementBlendTreeChildren.GetArrayElementAtIndex(2);
            var MovementMotion3 = MovementMotionSlot3.FindPropertyRelative("m_Motion");
            BlendTree RunBlendTree = MovementMotion3.objectReferenceValue as BlendTree;
            var SerializedRunBlendTree = new SerializedObject(RunBlendTree);
            var RunBlendTreeChildren = SerializedRunBlendTree.FindProperty("m_Childs");

            //Run Left
            var RunMotionSlot1 = RunBlendTreeChildren.GetArrayElementAtIndex(0);
            var RunLeftAnimation = RunMotionSlot1.FindPropertyRelative("m_Motion");
            var RunLeftAnimationSpeed = RunMotionSlot1.FindPropertyRelative("m_TimeScale");
            var RunLeftMirror = RunMotionSlot1.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.NonCombatAnimations.RunLeft.AnimationSpeed != 0) RunLeftAnimationSpeed.floatValue = EmeraldAnimationComponent.NonCombatAnimations.RunLeft.AnimationSpeed;
            RunLeftAnimation.objectReferenceValue = EmeraldAnimationComponent.NonCombatAnimations.RunLeft.AnimationClip;
            RunLeftMirror.boolValue = EmeraldAnimationComponent.NonCombatAnimations.RunLeft.Mirror;

            //Run Straight
            var RunMotionSlot2 = RunBlendTreeChildren.GetArrayElementAtIndex(1);
            var RunStraightAnimation = RunMotionSlot2.FindPropertyRelative("m_Motion");
            var RunStraightAnimationSpeed = RunMotionSlot2.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.NonCombatAnimations.RunForward.AnimationSpeed != 0) RunStraightAnimationSpeed.floatValue = EmeraldAnimationComponent.NonCombatAnimations.RunForward.AnimationSpeed;
            RunStraightAnimation.objectReferenceValue = EmeraldAnimationComponent.NonCombatAnimations.RunForward.AnimationClip;

            //Run Right
            var RunMotionSlot3 = RunBlendTreeChildren.GetArrayElementAtIndex(2);
            var RunRightAnimation = RunMotionSlot3.FindPropertyRelative("m_Motion");
            var RunRightAnimationSpeed = RunMotionSlot3.FindPropertyRelative("m_TimeScale");
            var RunRightMirror = RunMotionSlot3.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.NonCombatAnimations.RunRight.AnimationSpeed != 0) RunRightAnimationSpeed.floatValue = EmeraldAnimationComponent.NonCombatAnimations.RunRight.AnimationSpeed;
            RunRightAnimation.objectReferenceValue = EmeraldAnimationComponent.NonCombatAnimations.RunRight.AnimationClip;
            RunRightMirror.boolValue = EmeraldAnimationComponent.NonCombatAnimations.RunRight.Mirror;

            SerializedRunBlendTree.ApplyModifiedProperties();
        }

        /// <summary>
        /// Assigns all type 1 weapon type animations to the Animator Controller.
        /// </summary>
        static void ApplyType1Animations(AnimationProfile EmeraldAnimationComponent, AnimatorController m_AnimatorController)
        {
            //Go through each sub-state by name and assign the animation to each state within the sub-state using an index
            for (int i = 0; i < m_AnimatorController.layers[0].stateMachine.stateMachines.Length; i++)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine = m_AnimatorController.layers[0].stateMachine.stateMachines[i];
                ChildAnimatorState[] childAnimatorState = m_AnimatorController.layers[0].stateMachine.stateMachines[i].stateMachine.states;

                SetState(childAnimatorStateMachine, "Combat Hit States (Type 1)", childAnimatorState, EmeraldAnimationComponent.Type1Animations.HitList);
                SetState(childAnimatorStateMachine, "Attack States (Type 1)", childAnimatorState, EmeraldAnimationComponent.Type1Animations.AttackList);
                SetState(childAnimatorStateMachine, "Death States (Type 1)", childAnimatorState, EmeraldAnimationComponent.Type1Animations.DeathList);

                AnimationClass[] Type1StrafeList = new AnimationClass[2] { EmeraldAnimationComponent.Type1Animations.StrafeLeft, EmeraldAnimationComponent.Type1Animations.StrafeRight };
                SetState(childAnimatorStateMachine, "Strafe States (Type 1)", childAnimatorState, Type1StrafeList.ToList());
                AnimationClass[] Type1DodgeList = new AnimationClass[3] { EmeraldAnimationComponent.Type1Animations.DodgeLeft, EmeraldAnimationComponent.Type1Animations.DodgeRight, EmeraldAnimationComponent.Type1Animations.DodgeBack };
                SetState(childAnimatorStateMachine, "Dodge States (Type 1)", childAnimatorState, Type1DodgeList.ToList());
            }

            //Go through each state by name and assign the animation to the proper state
            for (int i = 0; i < m_AnimatorController.layers[0].stateMachine.states.Length; i++)
            {
                if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Pull Out Weapon (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.PullOutWeapon.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.PullOutWeapon.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Put Away Weapon (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.PutAwayWeapon.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.PutAwayWeapon.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Combat Turn Left (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.TurnLeft.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.Type1Animations.TurnLeft.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.TurnLeft.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Combat Turn Right (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.TurnRight.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.Type1Animations.TurnRight.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.TurnRight.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Walk Backwards (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.WalkBack.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.Type1Animations.WalkBack.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.WalkBack.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Block (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.BlockIdle.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.BlockIdle.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Block Hit (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.BlockHit.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.BlockHit.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Warning (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.IdleWarning.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.IdleWarning.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Recoil (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.Recoil.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.Recoil.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Stunned (Type 1)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type1Animations.Stunned.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type1Animations.Stunned.AnimationSpeed;
                }
            }

            //Get and assign Combat Movement Blend Tree animations
            AnimatorState m_StateMachine_Combat = m_AnimatorController.layers[0].stateMachine.states.ToList().Find(x => x.state.name == "Combat Movement (Type 1)").state;
            BlendTree CombatMovementBlendTree = m_StateMachine_Combat.motion as BlendTree;

            var SerializedCombatIdleBlendTreeRef = new SerializedObject(CombatMovementBlendTree);
            var CombatMovementBlendTreeChildren = SerializedCombatIdleBlendTreeRef.FindProperty("m_Childs");

            //Assign the Idle animation and settings to the Idle Blend Tree
            var CombatMovementMotionSlot1 = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatMovementMotion1 = CombatMovementMotionSlot1.FindPropertyRelative("m_Motion");
            BlendTree CombatIdleBlendTree = CombatMovementMotion1.objectReferenceValue as BlendTree;
            var SerializedCombatIdleBlendTree = new SerializedObject(CombatIdleBlendTree);
            var CombatIdleBlendTreeChildren = SerializedCombatIdleBlendTree.FindProperty("m_Childs");
            var CombatIdleMotionSlot = CombatIdleBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatIdleAnimation = CombatIdleMotionSlot.FindPropertyRelative("m_Motion");
            var CombatIdleAnimationSpeed = CombatIdleMotionSlot.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.Type1Animations.IdleStationary.AnimationSpeed != 0) CombatIdleAnimationSpeed.floatValue = EmeraldAnimationComponent.Type1Animations.IdleStationary.AnimationSpeed;
            CombatIdleAnimation.objectReferenceValue = EmeraldAnimationComponent.Type1Animations.IdleStationary.AnimationClip;
            SerializedCombatIdleBlendTree.ApplyModifiedProperties();

            //Assign the Walk animations and settings to the Walk Blend Tree; one for walk left, walk straight, and walk right.
            var CombatMovementMotionSlot2 = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(1);
            var CombatMovementMotion2 = CombatMovementMotionSlot2.FindPropertyRelative("m_Motion");
            BlendTree CombatWalkBlendTree = CombatMovementMotion2.objectReferenceValue as BlendTree;
            var SerializedCombatWalkBlendTree = new SerializedObject(CombatWalkBlendTree);
            var CombatWalkBlendTreeChildren = SerializedCombatWalkBlendTree.FindProperty("m_Childs");

            //Adjust the combat movement thresholds depending on which Animator Type is being used.
            var CombatWalkMovementMotionThreshold = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Threshold");
            var CombatRunMovementMotionThreshold = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Threshold");

            SerializedCombatIdleBlendTreeRef.ApplyModifiedProperties();

            //Walk Left
            var CombatWalkMotionSlot1 = CombatWalkBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatWalkLeftAnimation = CombatWalkMotionSlot1.FindPropertyRelative("m_Motion");
            var CombatWalkLeftAnimationSpeed = CombatWalkMotionSlot1.FindPropertyRelative("m_TimeScale");
            var CombatWalkLeftMirror = CombatWalkMotionSlot1.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type1Animations.WalkLeft.AnimationSpeed != 0) CombatWalkLeftAnimationSpeed.floatValue = EmeraldAnimationComponent.Type1Animations.WalkLeft.AnimationSpeed;
            CombatWalkLeftAnimation.objectReferenceValue = EmeraldAnimationComponent.Type1Animations.WalkLeft.AnimationClip;
            CombatWalkLeftMirror.boolValue = EmeraldAnimationComponent.Type1Animations.WalkLeft.Mirror;

            //Walk Straight
            var CombatWalkMotionSlot2 = CombatWalkBlendTreeChildren.GetArrayElementAtIndex(1);
            var CombatWalkStraightAnimation = CombatWalkMotionSlot2.FindPropertyRelative("m_Motion");
            var CombatWalkStraightAnimationSpeed = CombatWalkMotionSlot2.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.Type1Animations.WalkForward.AnimationSpeed != 0) CombatWalkStraightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type1Animations.WalkForward.AnimationSpeed;
            CombatWalkStraightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type1Animations.WalkForward.AnimationClip;

            //Walk Right
            var CombatWalkMotionSlot3 = CombatWalkBlendTreeChildren.GetArrayElementAtIndex(2);
            var CombatWalkRightAnimation = CombatWalkMotionSlot3.FindPropertyRelative("m_Motion");
            var CombatWalkRightAnimationSpeed = CombatWalkMotionSlot3.FindPropertyRelative("m_TimeScale");
            var CombatWalkRightMirror = CombatWalkMotionSlot3.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type1Animations.WalkRight.AnimationSpeed != 0) CombatWalkRightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type1Animations.WalkRight.AnimationSpeed;
            CombatWalkRightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type1Animations.WalkRight.AnimationClip;
            CombatWalkRightMirror.boolValue = EmeraldAnimationComponent.Type1Animations.WalkRight.Mirror;

            SerializedCombatWalkBlendTree.ApplyModifiedProperties();

            //Assign the Run animations and settings to the Run Blend Tree; one for run left, run straight, and run right.
            var CombatMovementMotionSlot3 = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(2);
            var CombatMovementMotion3 = CombatMovementMotionSlot3.FindPropertyRelative("m_Motion");
            BlendTree CombatRunBlendTree = CombatMovementMotion3.objectReferenceValue as BlendTree;
            var SerializedCombatRunBlendTree = new SerializedObject(CombatRunBlendTree);
            var CombatRunBlendTreeChildren = SerializedCombatRunBlendTree.FindProperty("m_Childs");

            //Run Left
            var CombatRunMotionSlot1 = CombatRunBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatRunLeftAnimation = CombatRunMotionSlot1.FindPropertyRelative("m_Motion");
            var CombatRunLeftAnimationSpeed = CombatRunMotionSlot1.FindPropertyRelative("m_TimeScale");
            var CombatRunLeftMirror = CombatRunMotionSlot1.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type1Animations.RunLeft.AnimationSpeed != 0) CombatRunLeftAnimationSpeed.floatValue = EmeraldAnimationComponent.Type1Animations.RunLeft.AnimationSpeed;
            CombatRunLeftAnimation.objectReferenceValue = EmeraldAnimationComponent.Type1Animations.RunLeft.AnimationClip;
            CombatRunLeftMirror.boolValue = EmeraldAnimationComponent.Type1Animations.RunLeft.Mirror;

            //Run Straight
            var CombatRunMotionSlot2 = CombatRunBlendTreeChildren.GetArrayElementAtIndex(1);
            var CombatRunStraightAnimation = CombatRunMotionSlot2.FindPropertyRelative("m_Motion");
            var CombatRunStraightAnimationSpeed = CombatRunMotionSlot2.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.Type1Animations.RunForward.AnimationSpeed != 0) CombatRunStraightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type1Animations.RunForward.AnimationSpeed;
            CombatRunStraightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type1Animations.RunForward.AnimationClip;

            //Run Right
            var CombatRunMotionSlot3 = CombatRunBlendTreeChildren.GetArrayElementAtIndex(2);
            var CombatRunRightAnimation = CombatRunMotionSlot3.FindPropertyRelative("m_Motion");
            var CombatRunRightAnimationSpeed = CombatRunMotionSlot3.FindPropertyRelative("m_TimeScale");
            var CombatRunRightMirror = CombatRunMotionSlot3.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type1Animations.RunRight.AnimationSpeed != 0) CombatRunRightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type1Animations.RunRight.AnimationSpeed;
            CombatRunRightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type1Animations.RunRight.AnimationClip;
            CombatRunRightMirror.boolValue = EmeraldAnimationComponent.Type1Animations.RunRight.Mirror;

            SerializedCombatRunBlendTree.ApplyModifiedProperties();
        }

        /// <summary>
        /// Assigns all type 2 weapon type animations to the Animator Controller.
        /// </summary>
        static void ApplyType2Animations(AnimationProfile EmeraldAnimationComponent, AnimatorController m_AnimatorController)
        {
            //Go through each sub-state by name and assign the animation to each state within the sub-state using an index
            for (int i = 0; i < m_AnimatorController.layers[0].stateMachine.stateMachines.Length; i++)
            {
                ChildAnimatorStateMachine childAnimatorStateMachine = m_AnimatorController.layers[0].stateMachine.stateMachines[i];
                ChildAnimatorState[] childAnimatorState = m_AnimatorController.layers[0].stateMachine.stateMachines[i].stateMachine.states;

                SetState(childAnimatorStateMachine, "Combat Hit States (Type 2)", childAnimatorState, EmeraldAnimationComponent.Type2Animations.HitList);
                SetState(childAnimatorStateMachine, "Attack States (Type 2)", childAnimatorState, EmeraldAnimationComponent.Type2Animations.AttackList);
                SetState(childAnimatorStateMachine, "Death States (Type 2)", childAnimatorState, EmeraldAnimationComponent.Type2Animations.DeathList);

                AnimationClass[] Type2StrafeList = new AnimationClass[2] { EmeraldAnimationComponent.Type2Animations.StrafeLeft, EmeraldAnimationComponent.Type2Animations.StrafeRight };
                SetState(childAnimatorStateMachine, "Strafe States (Type 2)", childAnimatorState, Type2StrafeList.ToList());
                AnimationClass[] Type2DodgeList = new AnimationClass[3] { EmeraldAnimationComponent.Type2Animations.DodgeLeft, EmeraldAnimationComponent.Type2Animations.DodgeRight, EmeraldAnimationComponent.Type2Animations.DodgeBack };
                SetState(childAnimatorStateMachine, "Dodge States (Type 2)", childAnimatorState, Type2DodgeList.ToList());
            }

            //Go through each state by name and assign the animation to the proper state
            for (int i = 0; i < m_AnimatorController.layers[0].stateMachine.states.Length; i++)
            {
                if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Pull Out Weapon (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.PullOutWeapon.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.PullOutWeapon.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Put Away Weapon (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.PutAwayWeapon.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.PutAwayWeapon.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Combat Turn Left (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.TurnLeft.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.Type2Animations.TurnLeft.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.TurnLeft.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Combat Turn Right (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.TurnRight.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.Type2Animations.TurnRight.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.TurnRight.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Walk Backwards (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.WalkBack.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.mirror = EmeraldAnimationComponent.Type2Animations.WalkBack.Mirror;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.WalkBack.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Block (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.BlockIdle.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.BlockIdle.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Block Hit (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.BlockHit.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.BlockHit.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Warning (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.IdleWarning.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.IdleWarning.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Recoil (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.Recoil.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.Recoil.AnimationSpeed;
                }
                else if (m_AnimatorController.layers[0].stateMachine.states[i].state.name == "Stunned (Type 2)")
                {
                    m_AnimatorController.layers[0].stateMachine.states[i].state.motion = EmeraldAnimationComponent.Type2Animations.Stunned.AnimationClip;
                    m_AnimatorController.layers[0].stateMachine.states[i].state.speed = EmeraldAnimationComponent.Type2Animations.Stunned.AnimationSpeed;
                }
            }

            //Get and assign Combat Movement Blend Tree animations
            AnimatorState m_StateMachine_Combat = m_AnimatorController.layers[0].stateMachine.states.ToList().Find(x => x.state.name == "Combat Movement (Type 2)").state;
            BlendTree CombatMovementBlendTree = m_StateMachine_Combat.motion as BlendTree;

            var SerializedCombatIdleBlendTreeRef = new SerializedObject(CombatMovementBlendTree);
            var CombatMovementBlendTreeChildren = SerializedCombatIdleBlendTreeRef.FindProperty("m_Childs");

            //Assign the Idle animation and settings to the Idle Blend Tree
            var CombatMovementMotionSlot1 = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatMovementMotion1 = CombatMovementMotionSlot1.FindPropertyRelative("m_Motion");
            BlendTree CombatIdleBlendTree = CombatMovementMotion1.objectReferenceValue as BlendTree;
            var SerializedCombatIdleBlendTree = new SerializedObject(CombatIdleBlendTree);
            var CombatIdleBlendTreeChildren = SerializedCombatIdleBlendTree.FindProperty("m_Childs");
            var CombatIdleMotionSlot = CombatIdleBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatIdleAnimation = CombatIdleMotionSlot.FindPropertyRelative("m_Motion");
            var CombatIdleAnimationSpeed = CombatIdleMotionSlot.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.Type2Animations.IdleStationary.AnimationSpeed != 0) CombatIdleAnimationSpeed.floatValue = EmeraldAnimationComponent.Type2Animations.IdleStationary.AnimationSpeed;
            CombatIdleAnimation.objectReferenceValue = EmeraldAnimationComponent.Type2Animations.IdleStationary.AnimationClip;
            SerializedCombatIdleBlendTree.ApplyModifiedProperties();

            //Assign the Walk animations and settings to the Walk Blend Tree; one for walk left, walk straight, and walk right.
            var CombatMovementMotionSlot2 = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(1);
            var CombatMovementMotion2 = CombatMovementMotionSlot2.FindPropertyRelative("m_Motion");
            BlendTree CombatWalkBlendTree = CombatMovementMotion2.objectReferenceValue as BlendTree;
            var SerializedCombatWalkBlendTree = new SerializedObject(CombatWalkBlendTree);
            var CombatWalkBlendTreeChildren = SerializedCombatWalkBlendTree.FindProperty("m_Childs");

            //Adjust the combat movement thresholds depending on which Animator Type is being used.
            var CombatWalkMovementMotionThreshold = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Threshold");
            var CombatRunMovementMotionThreshold = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Threshold");

            SerializedCombatIdleBlendTreeRef.ApplyModifiedProperties();

            //Walk Left
            var CombatWalkMotionSlot1 = CombatWalkBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatWalkLeftAnimation = CombatWalkMotionSlot1.FindPropertyRelative("m_Motion");
            var CombatWalkLeftAnimationSpeed = CombatWalkMotionSlot1.FindPropertyRelative("m_TimeScale");
            var CombatWalkLeftMirror = CombatWalkMotionSlot1.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type2Animations.WalkLeft.AnimationSpeed != 0) CombatWalkLeftAnimationSpeed.floatValue = EmeraldAnimationComponent.Type2Animations.WalkLeft.AnimationSpeed;
            CombatWalkLeftAnimation.objectReferenceValue = EmeraldAnimationComponent.Type2Animations.WalkLeft.AnimationClip;
            CombatWalkLeftMirror.boolValue = EmeraldAnimationComponent.Type2Animations.WalkLeft.Mirror;

            //Walk Straight
            var CombatWalkMotionSlot2 = CombatWalkBlendTreeChildren.GetArrayElementAtIndex(1);
            var CombatWalkStraightAnimation = CombatWalkMotionSlot2.FindPropertyRelative("m_Motion");
            var CombatWalkStraightAnimationSpeed = CombatWalkMotionSlot2.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.Type2Animations.WalkForward.AnimationSpeed != 0) CombatWalkStraightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type2Animations.WalkForward.AnimationSpeed;
            CombatWalkStraightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type2Animations.WalkForward.AnimationClip;

            //Walk Right
            var CombatWalkMotionSlot3 = CombatWalkBlendTreeChildren.GetArrayElementAtIndex(2);
            var CombatWalkRightAnimation = CombatWalkMotionSlot3.FindPropertyRelative("m_Motion");
            var CombatWalkRightAnimationSpeed = CombatWalkMotionSlot3.FindPropertyRelative("m_TimeScale");
            var CombatWalkRightMirror = CombatWalkMotionSlot3.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type2Animations.WalkRight.AnimationSpeed != 0) CombatWalkRightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type2Animations.WalkRight.AnimationSpeed;
            CombatWalkRightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type2Animations.WalkRight.AnimationClip;
            CombatWalkRightMirror.boolValue = EmeraldAnimationComponent.Type2Animations.WalkRight.Mirror;

            SerializedCombatWalkBlendTree.ApplyModifiedProperties();

            //Assign the Run animations and settings to the Run Blend Tree; one for run left, run straight, and run right.
            var CombatMovementMotionSlot3 = CombatMovementBlendTreeChildren.GetArrayElementAtIndex(2);
            var CombatMovementMotion3 = CombatMovementMotionSlot3.FindPropertyRelative("m_Motion");
            BlendTree CombatRunBlendTree = CombatMovementMotion3.objectReferenceValue as BlendTree;
            var SerializedCombatRunBlendTree = new SerializedObject(CombatRunBlendTree);
            var CombatRunBlendTreeChildren = SerializedCombatRunBlendTree.FindProperty("m_Childs");

            //Run Left
            var CombatRunMotionSlot1 = CombatRunBlendTreeChildren.GetArrayElementAtIndex(0);
            var CombatRunLeftAnimation = CombatRunMotionSlot1.FindPropertyRelative("m_Motion");
            var CombatRunLeftAnimationSpeed = CombatRunMotionSlot1.FindPropertyRelative("m_TimeScale");
            var CombatRunLeftMirror = CombatRunMotionSlot1.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type2Animations.RunLeft.AnimationSpeed != 0) CombatRunLeftAnimationSpeed.floatValue = EmeraldAnimationComponent.Type2Animations.RunLeft.AnimationSpeed;
            CombatRunLeftAnimation.objectReferenceValue = EmeraldAnimationComponent.Type2Animations.RunLeft.AnimationClip;
            CombatRunLeftMirror.boolValue = EmeraldAnimationComponent.Type2Animations.RunLeft.Mirror;

            //Run Straight
            var CombatRunMotionSlot2 = CombatRunBlendTreeChildren.GetArrayElementAtIndex(1);
            var CombatRunStraightAnimation = CombatRunMotionSlot2.FindPropertyRelative("m_Motion");
            var CombatRunStraightAnimationSpeed = CombatRunMotionSlot2.FindPropertyRelative("m_TimeScale");
            if (EmeraldAnimationComponent.Type2Animations.RunForward.AnimationSpeed != 0) CombatRunStraightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type2Animations.RunForward.AnimationSpeed;
            CombatRunStraightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type2Animations.RunForward.AnimationClip;

            //Run Right
            var CombatRunMotionSlot3 = CombatRunBlendTreeChildren.GetArrayElementAtIndex(2);
            var CombatRunRightAnimation = CombatRunMotionSlot3.FindPropertyRelative("m_Motion");
            var CombatRunRightAnimationSpeed = CombatRunMotionSlot3.FindPropertyRelative("m_TimeScale");
            var CombatRunRightMirror = CombatRunMotionSlot3.FindPropertyRelative("m_Mirror");
            if (EmeraldAnimationComponent.Type2Animations.RunRight.AnimationSpeed != 0) CombatRunRightAnimationSpeed.floatValue = EmeraldAnimationComponent.Type2Animations.RunRight.AnimationSpeed;
            CombatRunRightAnimation.objectReferenceValue = EmeraldAnimationComponent.Type2Animations.RunRight.AnimationClip;
            CombatRunRightMirror.boolValue = EmeraldAnimationComponent.Type2Animations.RunRight.Mirror;

            SerializedCombatRunBlendTree.ApplyModifiedProperties();
        }
    }
}