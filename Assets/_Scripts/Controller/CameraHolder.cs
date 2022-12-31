using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TPT
{
    public class CameraHolder : MonoBehaviour
    {
        public Transform target;
        public float speed = 9;

        public static CameraHolder singleton;

        void Awake()
        {
            singleton = this;
        }

        private void FixedUpdate()
        {
            if(target == null)
            {
                Debug.Log("no player found");
                return;
            }

            Vector3 p = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
            transform.position = p;
        }
    }
}