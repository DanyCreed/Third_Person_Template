using UnityEngine;
using System.Collections;

namespace TPT
{
    [CreateAssetMenu(menuName = "TPT/Items/Cloth Item Type")]
    public class ClothItemType : ScriptableObject
    {
        public bool isDisableWhenNoItem;
    }
}