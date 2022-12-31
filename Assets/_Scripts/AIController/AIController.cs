using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TPT
{
    public class AIController : MonoBehaviour, ILockable, IDamageable, IHaveAction
    {
        new Rigidbody rigidbody;
        Animator animator;
        NavMeshAgent agent;
        AnimatorHook animatorHook;
        Transform mTransform;

        public int health = 100;

        public float fovRadius = 20;
        //public float attackDistance = 5;
        public float rotationSpeed = 1;
        public float moveSpeed = 1;
        public bool canRotate;
        public float recoveryTimer;
        public int hardcodeAction = 1;
        LayerMask detectionLayer;

        Controller currentTarget;
        bool isInteracting = false;
        bool actionFlag;


        ActionSnapshot currentSnapshot;

        public ActionSnapshot[] actionSnapshots;

        public ActionSnapshot GetAction(float distance, float angle)
        {
            //Debug.Log("distance = " + distance);
            //Debug.Log("angle = " + angle);

            if(hardcodeAction != -1)
            {
                int index = hardcodeAction;
                hardcodeAction = -1;
                return actionSnapshots[index];
            }

            int maxScore = 0;
            for (int i = 0; i < actionSnapshots.Length; i++)
            {

                ActionSnapshot a = actionSnapshots[i];

                if (distance <= a.maxDist && distance >= a.minDist)
                {
                    if (angle <= a.maxAngle && angle >= a.minAngle)
                    {
                        maxScore += a.score;
                    }
                }
            }

            int ran = Random.Range(0, maxScore + 1);
            int temp = 0;

            for (int i = 0; i < actionSnapshots.Length; i++)
            {
                ActionSnapshot a = actionSnapshots[i];

                if (a.score == 0)
                    continue;


                if (distance <= a.maxDist && distance >= a.minDist)
                {
                    //Debug.Log("has action");
                    if (angle <= a.maxAngle && angle >= a.minAngle)
                    {
                        temp += a.score;

                        if (temp > ran)
                        {
                            return a;
                        }
                    }
                }
            }
            return null;
        }

        private void Start()
        {
            detectionLayer = (1 << 8);
            mTransform = this.transform;
            rigidbody = GetComponentInChildren<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
            agent = GetComponentInChildren<NavMeshAgent>();
            //agent.enabled = false;
            rigidbody.isKinematic = false;
            animatorHook = GetComponentInChildren<AnimatorHook>();

            agent.stoppingDistance = 3;
        }

        private void Update()
        {
            float delta = Time.deltaTime;

            isInteracting = animator.GetBool("isInteracting");

            if(isHit)
            {
                if(hitTimer > 0)
                {
                    hitTimer -= delta;
                }
                else
                {
                    isHit = false;
                }
            }

            if(currentTarget == null)
            {
                HandleDetection();
            }
            else
            {
                if(agent.isActiveAndEnabled)
                    agent.SetDestination(currentTarget.mTransform.position);

                Vector3 relativeDirection = mTransform.InverseTransformDirection(agent.desiredVelocity);

                if (!isInteracting)
                {
                    if(actionFlag)
                    {
                        recoveryTimer -= delta;
                        if(recoveryTimer <= 0)
                        {
                            actionFlag = false;
                        }
                    }

                    animator.SetFloat("Movement", relativeDirection.z, 0.1f, delta);

                    Vector3 dir = currentTarget.mTransform.position - mTransform.position;
                    dir.y = 0;
                    dir.Normalize();

                    float dis = Vector3.Distance(mTransform.position, currentTarget.mTransform.position);
                    float angle = Vector3.Angle(mTransform.forward, dir);
                    float dot = Vector3.Dot(mTransform.right, dir);
                    if (dot < 0)
                        angle *= -1;
                    
                    currentSnapshot = GetAction(dis, angle);

                    if(currentSnapshot != null && !actionFlag)
                    //HandleRotation(delta);
                        { 
                            PlayTargetAnimation(currentSnapshot.anim, true);
                            actionFlag = true;
                            recoveryTimer = currentSnapshot.recoveryTime;
                        }
                    else
                    {
                        animator.SetFloat("Sideways", relativeDirection.x, 0.1f, delta);
                    }
                }

                if (!isInteracting)
                {
                    agent.enabled = true;
                    mTransform.rotation = agent.transform.rotation;
                    Vector3 lookPosition = currentTarget.mTransform.position;
                    lookPosition.y += 1.2f;
                    animatorHook.lookAtPosition = lookPosition;

                    
                    //Debug.Log(animatorHook.deltaPosition);
                }

                if(isInteracting)
                {
                    if(animatorHook.canRotate)
                        HandleRotation(delta);

                    agent.enabled = false;
                    animator.SetFloat("Movement", 0, 0.1f, delta);
                    animator.SetFloat("Sideways", 0, 0.1f, delta);

                    if(currentSnapshot != null)
                    {
                        currentSnapshot.damageCollider.SetActive(animatorHook.openDamageCollider);
                    }
                }

                Vector3 targetVel = animatorHook.deltaPosition * moveSpeed;
                rigidbody.velocity = targetVel;
            }
        }

        void LateUpdate()
        {
            agent.transform.localPosition = Vector3.zero;
            agent.transform.localRotation = Quaternion.identity;
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting)
        {
            animator.SetBool("isInteracting", isInteracting);
            animator.CrossFade(targetAnim, 0.2f);
        }

        void HandleRotation(float delta)
        {
            Vector3 dir = currentTarget.mTransform.position - mTransform.position;
            dir.y = 0;
            dir.Normalize();

            if(dir == Vector3.zero)
            {
                dir = mTransform.forward;
            }

            float angle = Vector3.Angle(dir, mTransform.forward);
            if(angle > 5)
            {
                animator.SetFloat("Sideways", Vector3.Dot(dir, mTransform.right), 0.1f, delta);
            }
            else
            {
                animator.SetFloat("Sideways", 0, 0.1f, delta);
            }

            Quaternion targetRot = Quaternion.LookRotation(dir);
            mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotationSpeed);

        }

        void HandleDetection()
        {
            Collider[] cols = Physics.OverlapSphere(mTransform.position, fovRadius, detectionLayer);

            for (int i = 0; i < cols.Length; i++)
            {
                Controller controller = cols[i].transform.GetComponentInParent<Controller>();
                if(controller != null)
                {
                    currentTarget = controller;
                    animatorHook.hasLookAtTarget = true;
                    return;
                }
            }
        }

        public Transform lockOnTarget;
        public Transform GetLockOnTarget(Transform from)
        {
            return lockOnTarget;
        }


        bool isHit;
        float hitTimer;
        public void OnDamage(ActionContainer action)
        {
            if (action.owner == mTransform)
                return;

            if (!isHit)
            {
                isHit = true;
                hitTimer = 1;
                //PlayTargetAnimation("Damage 1", true);
                Debug.Log(isHit);

                health -= action.damage;
                animatorHook.openDamageCollider = false;

                if (health <= 0)
                {
                    PlayTargetAnimation("death", true);
                    animator.transform.parent = null;
                    this.enabled = false;
                    gameObject.SetActive(false);
                }
                else
                {

                    Vector3 direction = action.owner.position - mTransform.position;
                    float dot = Vector3.Dot(mTransform.forward, direction);

                    if (action.overrideReactAnim)
                    {
                        PlayTargetAnimation(action.reactAnim, true);
                    }
                    else
                    {
                        if (dot > 0)
                        {
                            PlayTargetAnimation("Hit From Front", true);
                        }
                        else
                        {
                            PlayTargetAnimation("Hit From Back", true);
                        }
                    }
                }      
            }
        }

        public bool IsAlive()
        {
            return health > 0;
        }

        public ActionContainer GetActionContainer()
        {
            return lastAction;
        }

        ActionContainer _lastAction;
        public ActionContainer lastAction
        {
            get
            {
                if (_lastAction == null)
                {
                    _lastAction = new ActionContainer();
                }

                _lastAction.owner = mTransform;
                _lastAction.damage = currentSnapshot.damage;
                _lastAction.overrideReactAnim = currentSnapshot.overrideReactAnim;
                _lastAction.reactAnim = currentSnapshot.reactAnim;

                return _lastAction;
            }
        }
    }

    [System.Serializable]
    public class ActionSnapshot
    {
        public string anim;
        public int score = 5;
        public float recoveryTime;
        public float minDist = 2;
        public float maxDist = 5;
        public float minAngle = -35;
        public float maxAngle = 35;
        public GameObject damageCollider;
        public int damage = 15;
        public bool overrideReactAnim;
        public string reactAnim;
    }
}