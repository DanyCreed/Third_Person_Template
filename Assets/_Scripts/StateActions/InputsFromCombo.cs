using UnityEngine;
using System.Collections;

namespace TPT
{
    public class InputsForCombo : StateAction
    {
        bool Rb, Rt, Lb, Lt, isAttacking;
        PlayerStateManager states;

        public InputsForCombo(PlayerStateManager playerStates)
        {
            states = playerStates;
        }

        public override bool Execute()
        {
            states.horizontal = Input.GetAxis("Horizontal");
            states.vertical = Input.GetAxis("Vertical");

            Debug.Log("inputs from combo");

            if (states.canDoCombo == false)
                return false;

            Rb = Input.GetButton("RB");
            Rt = Input.GetButton("RT");
            Lb = Input.GetButton("LB");
            Lt = Input.GetButton("LT");

            AttackInputs attackInput = AttackInputs.none;

            if (Rb || Rt || Lb || Lt)
            {
                Debug.Log("there is combo input");
                isAttacking = true;

                if (Rb)
                {
                    attackInput = AttackInputs.rb;
                }

                if (Rt)
                {
                    attackInput = AttackInputs.rt;
                }

                if (Lb)
                {
                    attackInput = AttackInputs.lb;
                }

                if (Lt)
                {
                    attackInput = AttackInputs.lt;
                }
            }

            if(attackInput != AttackInputs.none)
            {
                states.DoCombo(attackInput);
                isAttacking = false;
            }
            
            return false;
        }
    }
}
