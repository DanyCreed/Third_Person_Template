using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class ScaleUp : MonoBehaviour
    {
        private void OnEnable()
        {
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            transform.localScale += Vector3.one * Time.deltaTime;
        }
    }
}