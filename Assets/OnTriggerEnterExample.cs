using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TPT
{
    public class OnTriggerEnterExample : MonoBehaviour
    {
        public Animator anim;
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnTriggerEnter (Collider other)
        {
            if(other.tag == "Player") //PLAYER PUEDE SER CUALQUEIR OBJETOS PERO DEBE COINCIDIR EN EL TAG
            {
                Debug.Log("On Trigger");
                anim.SetBool("Move", true);
            }
        }
    }
}