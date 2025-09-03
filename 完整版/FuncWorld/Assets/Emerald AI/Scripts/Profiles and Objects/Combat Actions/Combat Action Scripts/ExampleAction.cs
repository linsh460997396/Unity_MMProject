using UnityEngine;
using System.Collections;

namespace EmeraldAI
{
    /// <summary>
    /// An example Combat Action that Debug Logs a random number to the Unity Console.
    /// </summary>
    [CreateAssetMenu(fileName = "Example Action", menuName = "Emerald AI/Combat Action/Example Action")]
    public class ExampleAction : EmeraldAction
    {
        /// <summary>
        /// Continiously updates the EmeraldAction. This acts like an Update function that can run 
        /// within this action using the information from the passed EmeraldComponent and its ActionClass.
        /// </summary>
        public override void UpdateAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            if (!CanExecute(EmeraldComponent, ActionClass))
                return;

            Execute(EmeraldComponent, ActionClass);
        }

        /// <summary>
        /// Conditions required for the EmeraldAction to execute.
        /// </summary>
        bool CanExecute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            var Conditions = (((int)EnterConditions) & ((int)EmeraldComponent.AnimationComponent.CurrentAnimationState)) != 0;
            return ActionClass.CooldownLengthTimer >= CooldownLength && Conditions && !ActionClass.IsActive;
        }

        /// <summary>
        /// Executes the Action because all conditions from CanExecute have been met.
        /// </summary>
        void Execute(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            //Use the monobehavior from EmeraldComponent to create an individual coroutine. This can allow localized variables if needed.
            EmeraldComponent.GetComponent<MonoBehaviour>().StartCoroutine(cAction(EmeraldComponent, ActionClass));
        }

        IEnumerator cAction(EmeraldSystem EmeraldComponent, ActionsClass ActionClass)
        {
            //Set the Action as active
            ActionClass.IsActive = true;

            //Debug log a random number using a local variable from the passed EmeraldComponent's MonoBehaviour.
            int LocalVariableExample = Random.Range(0, 256);
            Debug.Log(EmeraldComponent.gameObject.name + "  " + LocalVariableExample);

            //Wait 1 second
            yield return new WaitForSeconds(1);

            //After waiting a second, do the same thing.
            LocalVariableExample = Random.Range(0, 256);
            Debug.Log(EmeraldComponent.gameObject.name + "  " + LocalVariableExample);

            //Reset the cooldown timer and set the IsActive to false
            ActionClass.CooldownLengthTimer = 0;
            ActionClass.IsActive = false;
        }
    }
}