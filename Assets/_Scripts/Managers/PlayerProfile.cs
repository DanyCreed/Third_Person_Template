using UnityEngine;
using System.Collections;

namespace TPT
{
    [CreateAssetMenu]
    public class PlayerProfile : ScriptableObject
    {
        public string[] startingClothes;
        public string rightHandWeapon;
        public string leftHandWeapon;
    }
}