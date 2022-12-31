using UnityEngine;
using System.Collections;

namespace TPT
{
    public class InterruptAnimationIfMoveInput : StateAction
    {
        PlayerStateManager states;
        string locomotionId;

        public InterruptAnimationIfMoveInput(PlayerStateManager s, string locomotion)
        {
            states = s;
            locomotionId = locomotion;
        }

        public override bool Execute()
        {
            if(states.canMove)
            {
                if(states.horizontal != 0 || states.vertical != 0)
                {
                    states.anim.Play("Empty");
                    states.ChangeState(locomotionId);
                    return true;
                }
            }

            return false;
        }
    }
}