using UnityEngine;

namespace EmeraldAI
{
    [System.Serializable]
    public class ActionsClass
    {
        public EmeraldAction emeraldAction;
        public bool Enabled = true; //Used to store the whether or not this action is enabled as it cannot be stored within the EmeraldAction ScriptableObject
        public bool IsActive; //Used to store the current active state of this action as it cannot be stored within the EmeraldAction ScriptableObject
        public float CooldownLengthTimer; //Used to track the current cooldown length as it cannot be stored within the EmeraldAction ScriptableObject
        public float ActionLength; //Used to store the generated action length (if used) as it cannot be stored within the EmeraldAction ScriptableObject
        public float ActionLengthTimer; //Used to track the current action length as it cannot be stored within the EmeraldAction ScriptableObject
        public float Timer; //Used to track time within a custom action as it cannot be stored within the EmeraldAction ScriptableObject
        public int TimesUsed; //Used to track of the amount of times this action has been used as it cannot be stored within the EmeraldAction ScriptableObject
        public Coroutine ActionCoroutine; //Used to keep a reference to this action's coroutine as it cannot be stored within the EmeraldAction ScriptableObject
    }
}