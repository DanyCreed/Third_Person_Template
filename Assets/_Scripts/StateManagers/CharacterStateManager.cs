using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

namespace TPT
{
    public abstract class CharacterStateManager : StateManager, ILockable
    {
        [Header("References")]
        public Animator anim;
        public new Rigidbody rigidbody;
        public AnimatorHook animHook;
        public ClothManager clothManager;
        public WeaponHolderManager weaponHolderManager;
        public NavMeshAgent agent;   //NAVMESH MOVEMENT

        [Header("States")]
        public bool isGrounded;
        public bool isOnAir;
        public bool isInteracting;
        public bool useRootMotion;
        public bool lockOn;
        public bool isTwoHanded;
        public bool canDoCombo;
        public bool canRotate;
        public bool canMove;
        public float groundRayForDistance = 0.2f;
        public float groundDownDistanceOnAir = 0.4f;
        public Transform target;

        [Header("Controller Values")]
        public float vertical;
        public float horizontal;
        public float delta;
        public Vector3 rootMovement;

        [Header("Item Actions")]
        protected ItemActionContainer[] itemActions;
        public ItemActionContainer[] defaultItemActions = new ItemActionContainer[4];

        [Header("Runtime References")]
        public List<ClothItem> startingsCloths;
        public WeaponItem rightWeapon;
        public WeaponItem leftWeapon;

        protected WeaponItem currentWeapontInUse;
        protected ItemActionContainer currentItemAction;

        public Combo[] currentCombo;

        public override void Init()
        {
            anim = GetComponentInChildren<Animator>();
            animHook = GetComponentInChildren<AnimatorHook>();
            rigidbody = GetComponentInChildren<Rigidbody>();
            clothManager = GetComponentInChildren<ClothManager>();
            weaponHolderManager = GetComponentInChildren<WeaponHolderManager>();
            agent = GetComponentInChildren<NavMeshAgent>();    //NAVMESH MOVEMENT
            agent.updateRotation = false;                      //NAVMESH MOVEMENT
            anim.applyRootMotion = false;

            //animHook.Init(this);
            itemActions = new ItemActionContainer[4];
            PopulateListWithDefaultItemActions();
        }

        void PopulateListWithDefaultItemActions()
        {
            for (int i = 0; i < defaultItemActions.Length; i++)
            {
                //ItemActionContainer iac = GetItemActionContainer(defaultItemActions[i].attackInput, itemActions);
                //if (iac != null)
                //{
                    itemActions[i] = defaultItemActions[i];
                //}
            }
        }

        protected ItemActionContainer GetItemActionContainer(AttackInputs ai, ItemActionContainer[] l)
        {
            for (int i = 0; i < l.Length; i++)
            {
                if(l[i].attackInput == ai)
                {
                    /*if(isTwoHanded)
                    {
                        if (l[i].isTwoHanded == true)
                            return l[i];
                    }
                    else
                    {
                        if(l[i].isTwoHanded == false)
                            return l[i];
                    }*/
                    return l[i];
                }
            }

            return null;
        }

        ItemActionContainer GetItemActionContainer(AttackInputs ai, ItemActionContainer[] l, bool isTwoHanded)
        {
            for (int i = 0; i < l.Length; i++)
            {
                if (l[i].attackInput == ai)
                {
                    if (isTwoHanded)
                    {
                        if (l[i].isTwoHanded)
                        {
                            return l[i];
                        }
                    }
                    else
                    {
                        if (l[i].isTwoHanded == false)
                            return l[i];
                    }
                }
            }

            return null;
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting, bool isMirror = false)
        {
            
            anim.SetBool("isMirror", isMirror);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

        public virtual bool PlayTargetItemAction(AttackInputs attackInput)
        {
            return true;
            //ItemActionContainer iac = GetItemActionContainer(attackInput, itemActions, isTwoHanded);

            //Debug.Log(attackInput);
            //if (iac != null)
            //{
            //    iac.ExecuteItemAction(this);
            //}
            //else
            //{
            //    Debug.Log("no action found");
            //}

            /*switch (attackInput)
            {
                case AttackInputs.rb:
                    itemActions[0].ExecuteItemAction(this);
                    break;
                case AttackInputs.lb:
                    itemActions[1].ExecuteItemAction(this);
                    break;
                case AttackInputs.rt:
                    itemActions[2].ExecuteItemAction(this);
                    break;
                case AttackInputs.lt:
                    itemActions[3].ExecuteItemAction(this);
                    break;
                default:
                    break;
            }*/
        }

        public virtual void OnAssignLookOverride(Transform target)
        {
            this.target = target;
            if (target != null)
                lockOn = true;
        }

        public virtual void OnClearLookOverride()
        {
            lockOn = false;
        }

        public virtual void UpdateItemActionsWithCurrent()
        {
            ItemActionContainer[] newItemActions = new ItemActionContainer[4];

            for (int i = 0; i < newItemActions.Length; i++)
            {
                newItemActions[i] = new ItemActionContainer();
                newItemActions[i].animName = defaultItemActions[i].animName;
                newItemActions[i].attackInput = defaultItemActions[i].attackInput;
                newItemActions[i].itemAction = defaultItemActions[i].itemAction;
                newItemActions[i].isMirrored = defaultItemActions[i].isMirrored;
                newItemActions[i].itemActual = defaultItemActions[i].itemActual;
            }

            bool canLoadLeftHandAction = false;

            if(!isTwoHanded)
            {
                canLoadLeftHandAction = true;
            }

            if(weaponHolderManager.rightItem != null)
            {
                if (isTwoHanded)
                {
                    anim.CrossFade(weaponHolderManager.rightItem.twoHanded_anim, 0.2f);
                    anim.SetBool("isMirror", false);
                }
                else
                {
                    anim.CrossFade("R_" + weaponHolderManager.rightItem.oneHanded_anim, 0.2f);
                }

                for (int i = 0; i < weaponHolderManager.rightItem.itemActions.Length; i++)
                {
                    if(isTwoHanded)
                    {
                        if (!weaponHolderManager.rightItem.itemActions[i].isTwoHanded)
                            continue;
                    }
                    else
                    {
                        if (weaponHolderManager.rightItem.itemActions[i].isTwoHanded)
                            continue;
                    }

                    ItemActionContainer iac = GetItemActionContainer(weaponHolderManager.rightItem.itemActions[i].attackInput, newItemActions);
                    /*if (iac == null)
                        continue;*/

                    iac.animName = weaponHolderManager.rightItem.itemActions[i].animName;
                    //iac.attackInput = weaponHolderManager.rightItem.itemActions[i].attackInput;
                    iac.itemAction = weaponHolderManager.rightItem.itemActions[i].itemAction;
                    iac.isTwoHanded = weaponHolderManager.rightItem.itemActions[i].isTwoHanded;
                    iac.itemActual = weaponHolderManager.rightItem;
                }
            }
            else
            {
                anim.CrossFade("R_Empty", 0.2f);
                canLoadLeftHandAction = true;
            }

            //if(isTwoHanded && canLoadLeftHandAction)
            //{
            //    canLoadLeftHandAction = false;
            //}

            if (!canLoadLeftHandAction)
            {
                itemActions = newItemActions;
                return;
            }

            if (weaponHolderManager.leftItem != null)
            {
                if (isTwoHanded)
                {
                    anim.CrossFade(weaponHolderManager.leftItem.twoHanded_anim, 0.2f);
                    anim.SetBool("isMirror", true);
                }
                else
                {
                    anim.CrossFade("L_" + weaponHolderManager.leftItem.oneHanded_anim, 0.2f);
                }

                for (int i = 0; i < weaponHolderManager.leftItem.itemActions.Length; i++)
                {
                    if (isTwoHanded)
                    {
                        if (!weaponHolderManager.leftItem.itemActions[i].isTwoHanded)
                            continue;
                    }
                    else
                    {
                        if (weaponHolderManager.leftItem.itemActions[i].isTwoHanded)
                            continue;
                    }

                    ItemActionContainer weaponAction = weaponHolderManager.leftItem.itemActions[i];
                    /*if (weaponAction == null)
                        continue;*/

                    if (isTwoHanded)
                    {
                        ItemActionContainer iac = GetItemActionContainer(weaponHolderManager.leftItem.itemActions[i].attackInput, newItemActions);
                        iac.animName = weaponHolderManager.leftItem.itemActions[i].animName;
                        //iac.attackInput = weaponHolderManager.leftItem.itemActions[i].attackInput;
                        iac.itemAction = weaponHolderManager.leftItem.itemActions[i].itemAction;
                        iac.isTwoHanded = weaponHolderManager.leftItem.itemActions[i].isTwoHanded;

                    }
                    else
                    {
                        AttackInputs ai = AttackInputs.lb;
                        if (weaponAction.attackInput == AttackInputs.rb)
                            ai = AttackInputs.lb;
                        if (weaponAction.attackInput == AttackInputs.rt)
                            ai = AttackInputs.lt;

                        ItemActionContainer iac = GetItemActionContainer(ai, newItemActions);
                        iac.animName = weaponHolderManager.leftItem.itemActions[i].animName;
                        //iac.attackInput = weaponHolderManager.leftItem.itemActions[i].attackInput;
                        iac.itemAction = weaponHolderManager.leftItem.itemActions[i].itemAction;
                        iac.isTwoHanded = weaponHolderManager.leftItem.itemActions[i].isTwoHanded;
                        iac.itemActual = weaponHolderManager.leftItem;
                    }

                }
            }
            else
            {
                anim.CrossFade("L_Empty", 0.2f);
            }

            itemActions = newItemActions;
        }

        public void HandleTwoHanded()
        {
            if(isTwoHanded)
            {
                isTwoHanded = false;
                anim.Play("B_Empty");
            }
            else
            {
                isTwoHanded = true;
            }
            Debug.Log(isTwoHanded);
            UpdateItemActionsWithCurrent();
        }

        public void AssignCurrentWeaponAndAction(WeaponItem weapon, ItemActionContainer iac)
        {
            currentWeapontInUse = weapon;
            currentItemAction = iac;
        }

        public void HandleDamageCollider(bool status)
        {
            if (currentWeapontInUse == null)
                return;
            currentWeapontInUse.weaponHook.DamageColliderStatus(status);
        }

        public virtual void DoCombo(AttackInputs inp)
        {
            
        }

        public void DisableCombo()
        {
            canDoCombo = false;
        }

        public Transform FindLockableTarget()
        {
            List<Transform> lockablesList = new List<Transform>();

            LayerMask lm = (1 << 9);

            Collider[] colliders = Physics.OverlapSphere(mTransform.position, 20, lm);
            foreach (Collider c in colliders)
            {
                ILockable iLock = c.GetComponentInChildren<ILockable>();
                if(iLock != null)
                {
                    Transform t = iLock.GetLockOnTarget(mTransform);

                    if(t != null)
                        lockablesList.Add(t);
                }
            }

            float minDis = float.MaxValue;
            Transform target = null;

            for (int i = 0; i < lockablesList.Count; i++)
            {
                float tempDis = Vector3.Distance(lockablesList[i].position, mTransform.position);
                if(tempDis < minDis)
                {
                    minDis = tempDis;
                    target = lockablesList[i];
                }
            }

            return target;
        }

        public Transform GetLockOnTarget(Transform from)
        {
            if(from == this.transform)
                return null;

            return mTransform;
        }

        public bool IsAlive()
        {
            throw new System.NotImplementedException();
        }
    }
    
    [System.Serializable]
    public class Combo
    {
        public string animName;
        public AttackInputs inp;
    }
}