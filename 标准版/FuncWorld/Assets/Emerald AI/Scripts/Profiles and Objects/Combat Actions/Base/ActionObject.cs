using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI
{
    /// <summary>
    /// A modular action component that can be extended to create custom AI actions.
    /// </summary>
    [System.Serializable]
    public class EmeraldAction : ScriptableObject
    {
        #region Emerald Action Variables
        [Tooltip("Which states will allow this action to enter or start?")]
        public AnimationStateTypes EnterConditions = AnimationStateTypes.None;

        [Tooltip("Which states will allow this action to exit or be canceled?")]
        public AnimationStateTypes ExitConditions = AnimationStateTypes.None;

        [Tooltip("Which states allow the cooldown timer to tick with this action?")]
        public AnimationStateTypes CooldownConditions = AnimationStateTypes.None;

        [Range(0.25f, 30f)]
        [Tooltip("The length, in seconds, this action takes to be used again, given the Enter Conditions are me.")]
        public float CooldownLength = 2;

        [Tooltip("Does this action use cooldowns?")]
        public bool UseCooldown = true;
        #endregion

        #region Editor Variables
        [HideInInspector] public bool HideSettingsFoldout;
        [HideInInspector] public bool DefaultSettingsFoldout = true;
        [HideInInspector] public bool CustomSettingsFoldout = true;
        [HideInInspector] public bool InfoSettingsFoldout = true;
        [HideInInspector] public string ActionName;
        [HideInInspector] public string ActionDescription = "An Action Description can be customed through the Info Foldout.";
        #endregion

        /// <summary>
        /// Initializes the action. Can be usedful for setting interal Emerald AI or Action Class values
        /// </summary>
        public virtual void InitializeAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass) { }

        /// <summary>
        /// Continiously updates the EmeraldAction. This acts like an Update function that can run within this action using the information from the passed EmeraldComponent and its ActionClass.
        /// </summary>
        public virtual void UpdateAction (EmeraldSystem EmeraldComponent, ActionsClass ActionClass) { }

        /// <summary>
        /// Called when an action can be canceled from internal conditions such as when an AI dies.
        /// </summary>
        public virtual void CancelAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass) 
        {
            EmeraldComponent.AnimationComponent.ResetTriggers(0);
            ActionClass.IsActive = false;
        }
    }
}
