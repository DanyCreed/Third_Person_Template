using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public abstract class ItemAction : ScriptableObject
    {
        public abstract void ExecuteAction(ItemActionContainer itemContainer, Controller characterStateManager);


    }
}