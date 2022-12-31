using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TPT
{
    public class Controller : MonoBehaviour, IHaveAction, IDamageable
    {
        public bool lockOn;
        public bool isOnAir;
        public bool isGrounded;
        //public bool canRotate;
        //public bool canMove;
        public bool isRolling;
        public bool isInteracting;
        //public bool canDoCombo;
        public AnimationCurve rollCurve;
        public AnimationClip rollClip;

        [Header("Controller")]
        public float movementSpeed = 3;
        public float rollSpeed = 1;
        public float adaptSpeed = 1;
        public float rotationSpeed = 10;
        public float attackRotationSpeed = 3;
        //public float groundRayForDistance = 0.2f;
        public float groundDownDistanceOnAir = 0.4f;
        //public float navMeshDetectDistance = 1.0f;
        //public float frontRayOffset = 0.5f;
        public float groundedSpeed = 0.1f;
        public float groundedDistancerRay = 0.5f;
        public float velocityMultiplier = 1f;

        Animator anim;
        new Rigidbody rigidbody;
        //public NavMeshAgent agent;

        //[HideInInspector]
        public Transform currentLockTarget;
        //[HideInInspector]
        public Transform mTransform;

        LayerMask ignoreForGroundCheck;
        

        public List<ClothItem> startingCloth;
        public ItemActionContainer[] currentActions;
        public ItemActionContainer[] defaultActions;
        ItemActionContainer currentAction;
        WeaponHolderManager weaponHolderManager;
        ClothManager clothManager;
        public AnimatorHook animatorHook;
        Vector3 currentNormal;

        ActionContainer _lastAction;
        public ActionContainer lastAction
        {
            get
            {
                if(_lastAction == null)
                {
                    _lastAction = new ActionContainer();
                }

                _lastAction.owner = mTransform;
                _lastAction.damage = currentAction.damage;
                _lastAction.overrideReactAnim = currentAction.overrideReactAnim;
                _lastAction.reactAnim = currentAction.reactAnim;

                return _lastAction;
            }
        }

    
        public void SetWeapons(Item rh, Item lh)
        {
           
            weaponHolderManager.Init();

            LoadWeapon(rh, false);
            LoadWeapon(lh, true);
        }

        public void Init()
        {
            mTransform = this.transform;
            rigidbody = GetComponent<Rigidbody>();
            //agent = GetComponent<NavMeshAgent>();
            //agent.updateRotation = false;
            anim = GetComponentInChildren<Animator>();
            weaponHolderManager = GetComponent<WeaponHolderManager>();
            //weaponHolderManager.Init();
            animatorHook = GetComponentInChildren<AnimatorHook>();

            clothManager = GetComponent<ClothManager>();
            clothManager.Init();
            clothManager.LoadListOfItems(startingCloth);

            ResetCurrentActions();

            currentPosition = mTransform.position;
            ignoreForGroundCheck = ~(1 << 9 | 1 << 10);

            
        }

        

        private void Update()
        {
            isInteracting = anim.GetBool("isInteracting");

            if (animatorHook.canDoCombo)
            {
                if (!isInteracting)
                {
                    animatorHook.canDoCombo = false;
                }
            }

            if(hitTimer > 0)
            {
                hitTimer -= Time.deltaTime;
                if(hitTimer < 0)
                {
                    isHit = false;
                }
            }
        }

        #region Movement
        public void HandleCombo()
        {

        }

        Vector3 currentPosition;

        public void MoveCharacter(float vertical, float horizontal, Vector3 moveDirection, float delta)
        {
            CheckGround();

            float moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            HandleDamageCollider();
            //HANDLE ROTATION
            if (!isInteracting || animatorHook.canRotate)
            {
                Vector3 rotationDir = moveDirection;
                if (lockOn)
                {
                    //to do change rotate direction
                    rotationDir = currentLockTarget.position - mTransform.position;
                    //rotationDir.y = 0;
                    //rotationDir.Normalize();
                }

                HandleRotation(moveAmount, rotationDir, delta);
            }
            

            Vector3 targetVelocity = Vector3.zero;

            if (lockOn)
            {
                targetVelocity = mTransform.forward * vertical * movementSpeed;
                targetVelocity += mTransform.right * horizontal * movementSpeed;
            }
            else
            {
                targetVelocity = moveDirection * movementSpeed;
            }

            if (isInteracting)
            {
                targetVelocity = animatorHook.deltaPosition * velocityMultiplier;
            }

            //CheckGround(ref targetVelocity);

            //HANDLE MOVEMENT
            if (isGrounded)
            {
                //CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.LateUpdate;

                //if (rigidbody.isKinematic == false)
                //    rigidbody.isKinematic = true;

                //if (agent.isActiveAndEnabled == false)
                //    agent.enabled = true;

                //agent.velocity = targetVelocity;

                //RaycastHit hitInfo;
                //if(Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitInfo, 2))
                //{
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, currentNormal);
                //}
                rigidbody.velocity = targetVelocity;

                Vector3 groundedPosition = mTransform.position;
                groundedPosition.y = currentPosition.y;
                mTransform.position = Vector3.Lerp(mTransform.position, groundedPosition, delta / groundedSpeed);
            }
            //else
            //{
            //    //CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.FixedUpdate;
            //    //if (rigidbody.isKinematic == true)
            //    //    rigidbody.isKinematic = false;

            //    //if (agent.isActiveAndEnabled)
            //    //    agent.enabled = false;

            //    //rigidbody.drag = 0;

            //    MoveWithPhysics(targetVelocity, moveAmount);
            //}

            HandleAnimations(vertical, horizontal, moveAmount);
        }

       /* void MoveWithAgent(float delta, float vertical, float horizontal, float moveAmount)
        {
            Vector3 targetVelocity = Vector3.zero;

            if (lockOn)
            {
                targetVelocity = mTransform.forward * vertical * movementSpeed;
                targetVelocity += mTransform.right * horizontal * movementSpeed;
            }
            else
            {
                targetVelocity = mTransform.forward * moveAmount * movementSpeed;
            }

            CheckGround(ref targetVelocity);

            if (isGrounded)
            {
                CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.LateUpdate;

                if (rigidbody.isKinematic == false)
                    rigidbody.isKinematic = true;

                //if (agent.isActiveAndEnabled == false)
                //    agent.enabled = true;

                //agent.velocity = targetVelocity;

                //Debug.Log(targetVelocity);
            }
            else
            {
                CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.FixedUpdate;
                if (rigidbody.isKinematic == true)
                    rigidbody.isKinematic = false;

                //if (agent.isActiveAndEnabled)
                //    agent.enabled = false;

                rigidbody.drag = 0;
                targetVelocity.y = rigidbody.velocity.y;
                rigidbody.velocity = targetVelocity;
            }
        }*/

        void CheckGround()
        {
            RaycastHit hit;
            Vector3 origin = mTransform.position;
            origin.y += 0.5f;

            Debug.DrawRay(origin, mTransform.forward * 0.4f, Color.magenta);
            //if (Physics.Raycast(origin, mTransform.forward, out hit, 0.4f))
            //{
            //    v = Vector3.zero;
            //}

            //Vector3 dir = v;
            //dir.Normalize();
            //origin += dir * groundRayForDistance;

            float dis = groundedDistancerRay;
            if (isOnAir)
            {
                dis = groundDownDistanceOnAir;
            }

            Debug.DrawRay(origin, Vector3.down * dis, Color.red);
            if (Physics.SphereCast(origin, 0.2f, Vector3.down, out hit, dis, ignoreForGroundCheck)) //change Raycast for Spherecast
            {
                //Vector3 tp = hit.point;

                isGrounded = true;
                currentPosition = hit.point;
                currentNormal = hit.normal;
                //Debug.Log(currentNormal);

                float angle = Vector3.Angle(Vector3.up, currentNormal);
                if(angle > 45)
                {
                    isGrounded = false;

                }
                //Debug.Log(angle);
                
                //NavMeshHit navHit;
                //if (NavMesh.SamplePosition(tp, out navHit, navMeshDetectDistance, NavMesh.AllAreas))
                //{
                //    isGrounded = true;
                //}
                //else
                //{
                //    isGrounded = false;
                //}

                if (isOnAir)
                {
                    isOnAir = false;
                    PlayTargetAnimation("Empty", false, false);
                }
            }
            else
            {
                if (isGrounded)
                {
                    isGrounded = false;
                }

                if (isOnAir == false)
                {
                    isOnAir = true;
                    //anim.Play("OnAir");
                    Debug.Log("OnAir");
                    PlayTargetAnimation("OnAir", true, false);
                }
            }
        }

        void HandleDamageCollider()
        {
            if(currentAction != null)
            {
                if(currentAction.weaponHook != null)
                    currentAction.weaponHook.DamageColliderStatus(animatorHook.openDamageCollider);
            }
        }

        /*void MoveWithPhysics(Vector3 targetVelocity, float moveAmount)
        {
            float frontY = 0;
            RaycastHit hit;
            
            Vector3 origin = mTransform.position + (targetVelocity.normalized * frontRayOffset);
            origin.y += .5f;
            Debug.DrawRay(origin, -Vector3.up, Color.red, 0.01f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, 1, ignoreForGroundCheck))
            {
                float y = hit.point.y;
                frontY = y - mTransform.position.y;
            }

            Vector3 currentVelocity = rigidbody.velocity;
            
            if (isGrounded)
            {

                if (moveAmount > 0.1f)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.drag = 0;
                    if (Mathf.Abs(frontY) > 0.02f)
                    {
                        targetVelocity.y = ((frontY > 0) ? frontY + 0.2f : frontY - 0.2f) * movementSpeed;
                    }
                }
                else
                {
                    float abs = Mathf.Abs(frontY);

                    if (abs > 0.02f)
                    {
                        rigidbody.isKinematic = true;
                        targetVelocity.y = 0;
                        rigidbody.drag = 4;
                    }
                }
            }
            else
            {
                rigidbody.isKinematic = false;
                rigidbody.drag = 0;
                targetVelocity.y = currentVelocity.y;
            }
            Debug.DrawRay((mTransform.position + Vector3.up * .2f), targetVelocity, Color.green, 0.01f, false);
            rigidbody.velocity = targetVelocity;
        }*/

        public void HandleRotation(float moveAmount, Vector3 targetDir, float delta)
        {
            
            float moveOverride = moveAmount;
            if (lockOn)
            {
                moveOverride = 1;
            }

            targetDir.Normalize();
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;

            float actualRotationSpeed = rotationSpeed;
            if (isInteracting)
                rotationSpeed = attackRotationSpeed;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(
                mTransform.rotation, tr,
                delta * moveOverride * rotationSpeed);

            mTransform.rotation = targetRotation;
        }

        public void HandleAnimations(float vertical, float horizontal, float moveAmount)
        {
            if (isGrounded)
            {
                if (lockOn)
                {
                    float v = Mathf.Abs(vertical);
                    float f = 0;
                    if (v > 0 && v <= .5f)
                        f = .5f;
                    else if (v > 0.5f)
                        f = 1;

                    if (vertical < 0)
                        f = -f;

                    anim.SetFloat("forward", f); //.2f, delta);

                    float h = Mathf.Abs(horizontal);
                    float s = 0;
                    if (h > 0 && h <= .5f)
                        s = .5f;
                    else if (h > 0.5f)
                        s = 1;

                    if (horizontal < 0)
                        s = -1;

                    anim.SetFloat("sideways", s); //.2f, delta);
                }
                else
                {
                    //Debug.Log("no lock on");
                    float m = moveAmount;
                    float f = 0;
                    if (m > 0 && m <= .5f)
                        f = .5f;
                    else if (m > 0.5f)
                        f = 1;

                    //Debug.Log(moveAmount);
                    anim.SetFloat("forward", f, .02f, Time.deltaTime);
                    anim.SetFloat("sideways", 0); //0, delta);
                }
            }
            else
            {

            }
        }

        #endregion

        #region Items & Actions
        void ResetCurrentActions()
        {
            currentActions = new ItemActionContainer[defaultActions.Length];

            for (int i = 0; i < defaultActions.Length; i++)
            {
                currentActions[i] = new ItemActionContainer();
                currentActions[i].animName = defaultActions[i].animName;
                currentActions[i].attackInput = defaultActions[i].attackInput;
                currentActions[i].isMirrored = defaultActions[i].isMirrored;
                currentActions[i].itemAction = defaultActions[i].itemAction;
                currentActions[i].itemActual = defaultActions[i].itemActual;
            }
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting, bool isMirror = false, float velocityMultiplier = 1)
        {

            anim.SetBool("isMirror", isMirror);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
            this.isInteracting = isInteracting;
            this.velocityMultiplier = velocityMultiplier;
        }

        public void PlayTargetItemAction(AttackInputs attackInput)
        {
            animatorHook.canRotate = false;

            //PlayTargetAnimation("Attack_1", true);

            currentAction = GetItemActionContainer(attackInput, currentActions);

            if (!string.IsNullOrEmpty(currentAction.animName))
            {
                currentAction.ExecuteItemAction(this);
            }
        }

        protected ItemActionContainer GetItemActionContainer(AttackInputs ai, ItemActionContainer[] l)
        {
            if (l == null)
                return null;

            for (int i = 0; i < currentActions.Length; i++)
            {
                if (l[i].attackInput == ai)
                {
                    return l[i];
                }
            }

            return null;
        }

        public void LoadWeapon(Item item, bool isLeft)
        {

            if (!(item is WeaponItem))
                return;

            WeaponItem weaponItem = (WeaponItem)item;

            WeaponHook weaponHook = weaponHolderManager.LoadWeaponOnHook(weaponItem, isLeft);

            if (weaponItem == null)
            {
                ItemActionContainer da = GetItemActionContainer(GetAttackInput(AttackInputs.rb, isLeft), defaultActions);
                ItemActionContainer ta = GetItemActionContainer(AttackInputs.rt, currentActions);
                CopyItemActionContainer(da, ta);
                ta.isMirrored = isLeft;
                ta.weaponHook = weaponHook;
                return;
            }

            for (int i = 0; i < weaponItem.itemActions.Length; i++)
            {
                ItemActionContainer wa = weaponItem.itemActions[i];
                ItemActionContainer ic = GetItemActionContainer(GetAttackInput(wa.attackInput, isLeft), currentActions);

                ic.isMirrored = (isLeft);
                CopyItemActionContainer(wa, ic);
                ic.weaponHook = weaponHook;
            }
        }

        public void LoadCloth(Item item)
        {
            if(item is ClothItem)
            {
                clothManager.LoadItem((ClothItem)item);
            }
        }
        void CopyItemActionContainer(ItemActionContainer from, ItemActionContainer to)
        {
            to.animName = from.animName;
            to.itemAction = from.itemAction;
            to.itemActual = from.itemActual;
        }

        AttackInputs  GetAttackInput(AttackInputs inp, bool isLeft)
        {
            if(!isLeft)
            {
                return inp;
            }
            else
            {
                switch (inp)
                {
                    case AttackInputs.rb:
                        return AttackInputs.lb;
                    case AttackInputs.lb:
                        return AttackInputs.rb;
                    case AttackInputs.rt:
                        return AttackInputs.lt;
                    case AttackInputs.lt:
                        return AttackInputs.rt;
                    default:
                        return inp;
                }
            }
        }

        #region Combos
        private Combo[] combos;
        public void LoadCombos(Combo[] targetCombo)
        {
            combos = targetCombo;
        }

        public void DoCombo(AttackInputs inp)
        {

            Combo c = GetComboFromInp(inp);
            if (c == null)
            {
                Debug.Log("no combo input for " + inp);
                return;
            }

            //Debug.Log("Do Combo");
            PlayTargetAnimation(c.animName, true, false);
            animatorHook.canDoCombo = false;
        }


        Combo GetComboFromInp(AttackInputs inp)
        {
            if (combos == null)
                return null;

            for (int i = 0; i < combos.Length; i++)
            {
                if (combos[i].inp == inp)
                    return combos[i];
            }
            return null;
        }
        #endregion

        public ILockable FindLockableTarget()
        {
            Collider[] cols = Physics.OverlapSphere(mTransform.position, 20);
            for(int i = 0; i < cols.Length; i++)
            {
                ILockable lockable = cols[i].GetComponentInParent<ILockable>();
                if(lockable != null)
                {
                    return lockable;
                }
            }

            return null;
        }

        public ActionContainer GetActionContainer()
        {
            return lastAction;
        }

        bool isHit;
        float hitTimer;
        public void OnDamage(ActionContainer action)
        {
            if (action.owner == mTransform)
                return;

            if (!isHit)
            {
                animatorHook.openDamageCollider = false;
                isHit = true;
                hitTimer = 1;

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
        #endregion
    }
}