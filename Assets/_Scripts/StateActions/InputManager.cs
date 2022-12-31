using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class InputManager : MonoBehaviour
    {
        public CameraManager cameraManager;
        public Controller controller;
        public Transform camTransform;

        //Triggers & bumpers
        bool Rb, Rt, Lb, Lt, isAttacking, b_Input, y_Input, x_Input, inventoryInput,
            leftArrow, rightArrow, upArrow, downArrow, lockInput;

        float vertical;
        float horizontal;
        float moveAmount;
        float mouseX;
        float mouseY;
        bool rollFlag;
        float rollTimer;

        Vector2 moveDirection;
        Vector2 cameraDirection;
        PlayerController keys;

        public PlayerProfile playerProfile;

        ILockable currentLockable;

        public ExecutionOrder cameraMovement;

        public enum ExecutionOrder
        {
            fixedUpdate, update, lateUpdate
        }

        private void Start()
        {
            //To do check if you have the controller assigned and if not, instansiate it
            camTransform = Camera.main.transform;

            ResourcesManager rm = Settings.resourcesManager;
            for (int i = 0; i < playerProfile.startingClothes.Length; i++)
            {
                Item item = rm.GetItem(playerProfile.startingClothes[i]);
                if (item is ClothItem)
                {
                    controller.startingCloth.Add((ClothItem)item);
                }
            }

            controller.Init();
            controller.SetWeapons(rm.GetItem(playerProfile.rightHandWeapon), rm.GetItem(playerProfile.leftHandWeapon));
            cameraManager.targetTransform = controller.transform;

            keys = new PlayerController();
            keys.Player.Movement.performed += i => moveDirection = i.ReadValue<Vector2>();
            keys.Player.Camera.performed += i => cameraDirection = i.ReadValue<Vector2>();
            //keys.Player.RB.performed += i => Rb = i.phase == UnityEngine.InputSystem.InputActionPhase.Performed;
            //keys.Player.RB.started += i => Rb = true;
            //keys.Player.RB.canceled += i => Rb = false;
            keys.Player.Lock.started += i => lockInput = true;
            //keys.Player.Lock.canceled += i => lockInput = false;
            keys.Enable();

            Settings.interactionsLayer = (1 << 15);
        }

        private void OnDisable()
        {
            keys.Disable();
        }

        private void FixedUpdate()
        {
            if (controller == null)
                return;

            float delta = Time.deltaTime;

            HandleMovement(Time.fixedDeltaTime);
            cameraManager.FollowTarget(delta);

            if (cameraMovement == ExecutionOrder.fixedUpdate)
            {
                cameraManager.HandleRotation(delta, mouseX, mouseY);
                }
        }

        private void Update()
        {
            if (controller == null)
                return;

            float delta = Time.deltaTime;

            HandleInput();

            if(b_Input)
            {
                rollFlag = true;
                rollTimer += delta;
            }

            if (cameraMovement == ExecutionOrder.update)
            {
                //HandleMovement(Time.deltaTime);
                cameraManager.HandleRotation(Time.deltaTime, mouseX, mouseY);
            }
        }

        private void LateUpdate()
        {
            if (cameraMovement == ExecutionOrder.lateUpdate)
            {
                //cameraManager.FollowTarget(Time.deltaTime);
            }
            if(keys.Player.Interact.phase == UnityEngine.InputSystem.InputActionPhase.Started)
            {
                HandleInteractions();
            }

            HandleInteractionsDetections();
        }

        void HandleMovement(float delta)
        {
            Vector3 movementDirection = camTransform.right * horizontal;
            movementDirection += camTransform.forward * vertical;
            movementDirection.Normalize();

            controller.MoveCharacter(vertical, horizontal, movementDirection, delta);
        }

        bool GetButtonStatus(UnityEngine.InputSystem.InputActionPhase phase)
        {
            return phase == UnityEngine.InputSystem.InputActionPhase.Started;
        }

        void HandleInput()
        {
            bool retVal = false;
            isAttacking = false;

            vertical = moveDirection.y;
            horizontal = moveDirection.x;
            mouseX = cameraDirection.x;
            mouseY = cameraDirection.y;

            Rb = GetButtonStatus(keys.Player.RB.phase);
            Rt = GetButtonStatus(keys.Player.RT.phase);
            Lb = GetButtonStatus(keys.Player.LB.phase);
            Lt = GetButtonStatus(keys.Player.LT.phase);
            lockInput = GetButtonStatus(keys.Player.Lock.phase);
            b_Input = GetButtonStatus(keys.Player.Roll.phase);
            //Debug.Log(keys.Player.RB.phase);

            /*horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            Rb = Input.GetButton("RB");
            Rt = Input.GetButton("RT");
            Lb = Input.GetButton("LB");
            Lt = Input.GetButton("LT");
            inventoryInput = Input.GetButton("Inventory");
            b_Input = Input.GetButton("B");
            y_Input = Input.GetButtonDown("Y");  //two handle button
            x_Input = Input.GetButton("X");
            leftArrow = Input.GetButton("Left");
            rightArrow = Input.GetButton("Right");
            upArrow = Input.GetButton("Up");
            downArrow = Input.GetButton("Down");
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");*/

            moveAmount = Mathf.Clamp01(Mathf.Abs(vertical) + Mathf.Abs(horizontal));


            if (b_Input)
            {
                rollFlag = true;
            }

            //controller.moveAmount = Mathf.Clamp01(Mathf.Abs(controller.horizontal) + Mathf.Abs(controller.vertical));
            if (!controller.isInteracting)
            {
                if (retVal == false)
                    retVal = HandleRolls();
            }
            if (retVal == false)
                retVal = HandleAttacking();


            if (lockInput) //enemy target Input.GetKeyDown(KeyCode.F)
            {
                
                lockInput = false;
                

                if (controller.lockOn)
                {
                    //        controller.OnClearLookOverride();

                    DisableLockOn();
                    
                }
                else
                {
                    Transform lockTarget = null;

                    currentLockable = controller.FindLockableTarget();
                    if(currentLockable != null)
                    {
                        lockTarget = currentLockable.GetLockOnTarget(controller.mTransform);
                    }

                    if(lockTarget != null)
                    {
                        cameraManager.lockTarget = lockTarget;
                        controller.lockOn = true;
                        controller.currentLockTarget = lockTarget;
                        Debug.Log("Lock Enemy");

                    }
                    else
                    {
                        cameraManager.lockTarget = null;
                        controller.lockOn = false;
                    }
            //        controller.target = controller.FindLockableTarget();

            //        if(controller.target != null)
            //            controller.OnAssignLookOverride(controller.target);
                }
            }

            if(controller.lockOn)
            {
                if(!currentLockable.IsAlive())
                {
                    DisableLockOn();
                    Debug.Log("Unlock");
                }
            }

            //if (controller.canDoCombo)
            //{
            //    bool isinteracting = controller.anim.GetBool("isInteracting");
            //    if (!isinteracting)
            //    {
            //        controller.canDoCombo = false;
            //    }
            }

        void DisableLockOn()
        {
            cameraManager.lockTarget = null;
            controller.lockOn = false;
            controller.currentLockTarget = null;
            currentLockable = null;
        }
            
        

        bool HandleAttacking()
        {
            AttackInputs attackInput = AttackInputs.none;

            if (Rb || Rt || Lb || Lt)
            {
                isAttacking = true;

                if(Rb)
                {
                    attackInput = AttackInputs.rb;
                }

                if (Rt)
                {
                    attackInput = AttackInputs.rt;
                }

                if (Lb)
                {
                    attackInput = AttackInputs.lb;
                }

                if (Lt)
                {
                    attackInput = AttackInputs.lt;
                }
            }

            if (y_Input)
            {
                //isAttacking = false;
                Debug.Log("two handed");
                //controller.HandleTwoHanded();
            }
            if (attackInput != AttackInputs.none)
            {
                if (!controller.isInteracting)
                {

                    //if (attackInput != AttackInputs.none)
                    //{
                        //find the actual attack animation from the items etc.
                        //play animation
                        //s.PlayTargetAnimation("Attack 1", true);
                        controller.PlayTargetItemAction(attackInput);
                        //if (hasAttack)
                        //{
                        //    //controller.vertical = 0;
                        //    //controller.horizontal = 0;
                        //    //controller.moveAmount = 0;
                        //    //controller.ChangeState(controller.attackStateId);
                        //}
                        //else
                        //{
                        //    isAttacking = false;
                        //}
                }
                else
                {
                    if(controller.animatorHook.canDoCombo)
                    {
                        controller.DoCombo(attackInput);
                    }
                }
            }
            return isAttacking;
        }

        bool HandleRolls()
        {
            if(b_Input == false && rollFlag)
            {
                rollFlag = false;
                if (moveAmount > 0) //rollTimer > 0.5f || 
                {
                    Vector3 movementDirection = camTransform.right * horizontal;
                    movementDirection += camTransform.forward * vertical;
                    movementDirection.Normalize();
                    movementDirection.y = 0;

                    Quaternion dir = Quaternion.LookRotation(movementDirection);
                    controller.transform.rotation = dir;
                    controller.PlayTargetAnimation("Roll", true, false, 1.5f);
                }
                else
                {
                    controller.PlayTargetAnimation("Step Back", true, false);
                }
                //Vector3 targetDir = Vector3.zero;
                //targetDir = controller.GetComponent<Camera>().transform.forward * controller.vertical;
                //targetDir += controller.GetComponent<Camera>().transform.right * controller.horizontal;

                //if(targetDir.z != 0)
                //{
                //    if(targetDir.z > 0)
                //    {
                //        targetDir.z = 1;
                //    }
                //    else
                //    {
                //        targetDir.z = -1;
                //    }
                //}

                //if(targetDir.x != 0)
                //{
                //    if(targetDir.x > 0)
                //    {
                //        targetDir.x = 1;
                //    }
                //    else
                //    {
                //        targetDir.x = -1;
                //    }
                //}
                //if (targetDir != Vector3.zero)
                //{

                //    controller.rollDirection = targetDir;

                //    controller.mTransform.rotation = Quaternion.LookRotation(controller.rollDirection);
                //    controller.PlayTargetAnimation("Roll", true, false);
                //    controller.ChangeState(controller.rollStateId);
                //    controller.isRolling = false;
                //}
                //else
                //{
                //    controller.rollDirection = Vector3.zero;
                //    controller.PlayTargetAnimation("Step Back", true, false);
                //    controller.ChangeState(controller.attackStateId);
                //}

                //controller.vertical = 0;
                //controller.horizontal = 0;
                //controller.moveAmount = 0;
                return true;
            }

            return false;
        }

        IInteractable currentInteractable;
        void HandleInteractionsDetections()
        {
            GameUI gameUI = GameUI.singleton;
            currentInteractable = null;
            gameUI.ResetInteraction();

            Collider[] colliders = Physics.OverlapSphere(controller.mTransform.position, 1.5f, Settings.interactionsLayer);

            for (int i = 0; i < colliders.Length; i++)
            {
                IInteractable interactable = colliders[i].transform.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    currentInteractable = interactable;
                    gameUI.LoadInteraction(interactable.GetInteractionType());
                    break;
                }
            }

        }    

        void HandleInteractions()
        {
            if(currentInteractable != null)
            {
                currentInteractable.OnInteract(this);
            }
        }
    }
}
