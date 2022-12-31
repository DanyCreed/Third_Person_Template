using UnityEngine;
using System.Collections;


namespace TPT
{
    public class MonitorInteractingAnimation : StateAction
    {
        CharacterStateManager states;
        string targetBool;
        string targetStates;

        public MonitorInteractingAnimation(CharacterStateManager characterStateManager, string targetBool, string targetState)
        {
            states = characterStateManager;
            this.targetBool = targetBool; //posible error
            this.targetStates = targetState; //posible error
        }

        public override bool Execute()
        {
            bool isInteracting = states.anim.GetBool(targetBool);
            states.isInteracting = isInteracting;

            if (isInteracting)
            {
                return false;
            }
            else
            {
                states.ChangeState(targetStates);
                return true;
            }
        }
    }
}
