using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class CinemachineBrainHook : MonoBehaviour
    {
        public Cinemachine.CinemachineBrain brain;

        public static CinemachineBrainHook singleton;

        private void Awake()
        {
            singleton = this;
            brain = GetComponent<Cinemachine.CinemachineBrain>();
        }
    }
}