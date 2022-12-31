using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI singleton;

        public GameObject pickupText;

        private void Awake()
        {
            singleton = this;
        }

        public void ResetInteraction()
        {
            pickupText.SetActive(false);
        }
        public void LoadInteraction(InteractionType interactionType)
        {
            switch (interactionType)
            {
                case InteractionType.pickup:
                    pickupText.SetActive(true);
                    break;
                case InteractionType.talk:
                    break;
                case InteractionType.open:
                    break;
                default:
                    break;
            }
        }
    }
}