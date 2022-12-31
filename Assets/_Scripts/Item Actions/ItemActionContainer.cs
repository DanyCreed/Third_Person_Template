using UnityEngine;
using System.Collections;

namespace TPT
{
    [System.Serializable]
    public class ItemActionContainer
    {
        //public int animIndex;
        public string animName;
        public ItemAction itemAction;
        public AttackInputs attackInput;
        public bool isMirrored;
        public bool isTwoHanded;
        public Item itemActual;
        public WeaponHook weaponHook;
        public int damage = 15;
        public bool overrideReactAnim;
        public string reactAnim;


        public void ExecuteItemAction(Controller controller)
        {
            Debug.Log("execute item action");
            itemAction.ExecuteAction(this, controller);
        }
    }
}
