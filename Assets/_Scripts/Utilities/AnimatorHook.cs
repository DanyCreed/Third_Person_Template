using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class AnimatorHook : MonoBehaviour
    {
        public Controller controller;
        public bool isAI;

        Animator animator;

        public Vector3 deltaPosition;

        public bool hasLookAtTarget;
        public bool canRotate;
        public bool canDoCombo;
        public bool canMove;
        public bool openDamageCollider;
        public Vector3 lookAtPosition;

        Rigidbody[] ragdollRigis;
        Collider[] ragdollColliders;

        private void Start()
        {
            animator = GetComponent<Animator>();
            controller = GetComponentInParent<Controller>();
            if (controller == null)
            {
                isAI = true;
            }
            else
            {
                isAI = false;
            }

            RagdollStatus(false);
        }

        void RagdollStatus(bool status)
        {
            Rigidbody[] ragdollRigis = GetComponentsInChildren<Rigidbody>();
            Collider[] ragdollColliders = GetComponentsInChildren<Collider>();

            foreach (Rigidbody r in ragdollRigis)
            {
                r.isKinematic = !status;
                r.gameObject.layer = 10;
            }

            foreach (Collider c in ragdollColliders)
            {
                c.isTrigger = !status;
            }

            animator.enabled = !status;
        }

        public void OnAnimatorMove()
        {
            OnAnimatorMoveOverride();
        }

        protected virtual void OnAnimatorMoveOverride()
        {
            float delta = Time.deltaTime;

            if (!isAI)
            {
                if (controller == null)
                    return;

                if (controller.isInteracting == false)
                    return;

                if (controller.isGrounded && delta > 0)
                {
                    deltaPosition = (animator.deltaPosition) / delta;
                }
            }
            else
            {
                deltaPosition = (animator.deltaPosition) / delta;
            }
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if(hasLookAtTarget)
            {
                animator.SetLookAtWeight(1, 0.9f, 0.95f, 1, 1);
                animator.SetLookAtPosition(lookAtPosition);
            }
        }

        public void OpenCanMove()
        {
            canMove = true;
        }

        public void OpenDamageCollider()
        {
            openDamageCollider = true;
            //controller.HandleDamageCollider(true);
        }

        public void CloseDamageCollider()
        {
            openDamageCollider = false;
            //controller.HandleDamageCollider(false);
        }

        public void EnableCombo()
        {
            //Debug.Log("enables combo");
            canDoCombo = true;
            //controller.canDoCombo = true;
        }

        /*public void DisableCombo()
        {
            controller.DisableCombo();
            controller.canDoCombo = false;
        }

        public void ExecuteCombo()
        {
            controller.CheckForComboPrompt();
        }*/

        public void EnableRotation()
        {
            canRotate = true;
            //controller.canRotate = true;
        }

        public void DisableRotation()
        {
            canRotate = false;
            //controller.canRotate = false;
        }

        public void EnableRagdoll()
        {
            RagdollStatus(true);
        }
    }
}
