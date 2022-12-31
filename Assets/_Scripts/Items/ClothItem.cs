using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    [CreateAssetMenu(menuName = "TPT/Items/Cloth Item")]
    public class ClothItem : Item
    {
        public ClothItemType clothType;
        public Mesh mesh;
        public Material clothMaterial;
        public Material[] materialCloth;
    }
}