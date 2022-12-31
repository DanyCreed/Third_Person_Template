using UnityEngine;
using System.Collections;

namespace TPT
{
    [CreateAssetMenu(menuName = "TPT/Item Actions/Attack Action")]
    public class AttackAction : ItemAction
    {
        public override void ExecuteAction(ItemActionContainer ic, Controller cs)
        {
            //string targetAnim = "Attack_1";

            //if (ic.animIndex < ic.animName.Length - 1 && ic.animIndex > -1)
            //{
            //    if (!string.IsNullOrEmpty(ic.animName[ic.animIndex]))
            //        targetAnim = ic.animName[ic.animIndex];
            //    //return;
            //}

            //if (string.IsNullOrEmpty(targetAnim))
            //{
            //    Debug.LogWarning("Target Animation is null or empty, assigning default value");
            //    targetAnim = "punch 1";
            //} //If the anim not found
            //Debug.Log(targetAnim);


            //cs.AssignCurrentWeaponAndAction((WeaponItem)ic.itemActual, ic);
            cs.PlayTargetAnimation(ic.animName, true, ic.isMirrored);
            Debug.Log(ic.animName);

            //ic.animIndex++;
            /*if (ic.animIndex > ic.animName.Length-1)
            {
                ic.animIndex = 0;
                cs.canDoCombo = false;
            }*/
        }
    }
}