using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class ThirdPersonController : MonoBehaviour
    {
        float horizontal;
        float vertical;
        Vector3 moveDiretion;
        float moveAmount;
        Vector3 camYforward;

        Transform camHolder;

        Rigidbody rigid;
        Collider col;
        Animator anim;

        public float moveSpeed = 4;
        public float rotateSpeed = 9;
        public float jumpSpeed = 10;

        bool onGround;
        bool keepOffGround;
        float savedTime;

        public bool isClimbing;

        FreeClimb freeClimb;

        void Start()
        {
            rigid = GetComponent<Rigidbody>();
            rigid.angularDrag = 999;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            col = GetComponent<Collider>();

            camHolder = CameraHolder.singleton.transform;
            anim = GetComponentInChildren<Animator>();
            freeClimb = GetComponent<FreeClimb>();
        }

        void FixedUpdate()
        {
            if (isClimbing)
                return;

            onGround = OnGround();
            Movement();
        }

        void Movement()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            camYforward = camHolder.forward;
            Vector3 v = vertical * camHolder.forward;
            Vector3 h = horizontal * camHolder.right;

            moveDiretion = (v + h).normalized;
            moveAmount = Mathf.Clamp01((Mathf.Abs(horizontal) + Mathf.Abs(vertical)));

            Vector3 targetDir = moveDiretion;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            Quaternion lookDir = Quaternion.LookRotation(targetDir);
            Quaternion targetRot = Quaternion.Slerp(transform.rotation, lookDir, Time.deltaTime * rotateSpeed);
            transform.rotation = targetRot;

            Vector3 dir = transform.forward * (moveSpeed * moveAmount);
            dir.y = rigid.velocity.y;
            rigid.velocity = dir;
        }

        void Update()
        {
            if (isClimbing)
            {
                freeClimb.Tick(Time.deltaTime);
                return;
            }

            onGround = OnGround();

            if(keepOffGround)
            {
                if(Time.realtimeSinceStartup - savedTime > 0.5f)
                {
                    keepOffGround = false;
                }
            }

            //Debug.Log(onGround);

            Jump();

            if(!onGround && !keepOffGround)
            {
                //isClimbing = freeClimb.CheckForClimb();
                if(isClimbing)
                {
                    DisableController();
                }
            }

            anim.SetFloat("move", moveAmount);
            anim.SetBool("onAir", !onGround);
        }

        void Jump()
        {
            if (onGround)
            {
                bool jump = Input.GetButton("Jump");
                if (jump)
                {
                    Vector3 v = rigid.velocity;
                    v.y = jumpSpeed;
                    rigid.velocity = v;
                    savedTime = Time.realtimeSinceStartup;
                    keepOffGround = true;
                }
            }
        }

        bool OnGround()
        {
            if (keepOffGround)
                return false;

            Vector3 origin = transform.position;
            origin.y += 0.4f;
            Vector3 direction = -transform.up;
            RaycastHit hit;
            if(Physics.Raycast(origin,direction, out hit, 0.41f))
            {
                return true;
            }

            return false;

        }

        public void DisableController()
        {
            rigid.isKinematic = true;
            col.enabled = false;
        }

        public void EnableController()
        {
            rigid.isKinematic = true;
            col.enabled = false;
        }
    }
}