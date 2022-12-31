using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class ComboInfo : StateMachineBehaviour
    {
        public Combo[] combos;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //if (sm == null)
            //{
            //    sm = animator.transform.GetComponentInParent<CharacterStateManager>();
            //}

            Controller controller = animator.GetComponentInParent<Controller>();
            controller.LoadCombos(combos);

            //sm.currentCombo = combos;
        }
    }
}