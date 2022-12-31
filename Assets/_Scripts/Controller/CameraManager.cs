using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class CameraManager : MonoBehaviour
    {
        public Transform targetTransform;
        public Transform lockTarget;
        public Transform pivot;
        public Transform camTransform;
        float defaultPosition;
        float targetPosition;
        public float followSpeed = 0.1f;
        public float resetSpeed = 3;

        Transform mTransform;
        Vector3 camTransPosition;
        public float camCollisionOffset = 0.2f;
        public float minCollisionOffset = 0.2f;
        public float camSphereRadius = 0.2f;

        public float lookSpeed = 1;
        public float pivotSpeed = 0.03f;
        public float velSpeed = 0.1f;
        float lookAngle;
        float pivotAngle;
        public float minPivot = -35;
        public float maxPivot = 45;

        LayerMask ignoreLayers;

        public void Start()
        {
            mTransform = this.transform;
            defaultPosition = camTransform.localPosition.z;
            ignoreLayers = ~(1 << 8 | 1 << 9 | 1 << 10);
        }

        public void FollowTarget(float delta)
        {
            Vector3 targetPosition = Vector3.Lerp(transform.position, targetTransform.position, delta / followSpeed);
            mTransform.position = targetPosition;
            HandleCollisions(delta);
        }

        public void HandleRotation(float delta, float mouseX, float mouseY)
        {
            

            if (lockTarget == null)
            {
                lookAngle += (mouseX * lookSpeed) / delta;
                pivotAngle -= (mouseY * pivotSpeed) / delta;
                pivotAngle = Mathf.Clamp(pivotAngle, minPivot, maxPivot);

                Vector3 euler = Vector3.zero;
                euler.y = lookAngle;
                Quaternion targetRotation = Quaternion.Euler(euler);
                mTransform.rotation = targetRotation;

                euler = Vector3.zero;
                euler.x = pivotAngle;
                targetRotation = Quaternion.Euler(euler);
                pivot.localRotation = targetRotation;

                Quaternion resetRotation = Quaternion.Slerp(mTransform.rotation, targetTransform.rotation, delta / resetSpeed);
                lookAngle = resetRotation.eulerAngles.y;

                //Vector3 resetEuler = targetTransform.eulerAngles;
                //lookAngle = Mathf.Lerp(lookAngle, resetEuler.y, delta / resetSpeed);
            }
            else
            {
                Vector3 dir = lockTarget.position - mTransform.position;
                dir.Normalize();
                dir.y = 0;

                Quaternion targetRotation = Quaternion.LookRotation(dir);
                mTransform.rotation = targetRotation;

                dir = lockTarget.position - pivot.position;
                dir.Normalize();
                //dir.x = 0;
                //dir.z = 0;

                targetRotation = Quaternion.LookRotation(dir);
                Vector3 e = targetRotation.eulerAngles;
                e.y = 0;
                pivot.localEulerAngles = e;

                pivotAngle = pivot.localEulerAngles.x;
                lookAngle = mTransform.eulerAngles.y;
            }
        }
    
        void HandleCollisions(float delta)
        {
            targetPosition = defaultPosition;

            RaycastHit hit;
            Vector3 direction = camTransform.position - pivot.position;
            direction.Normalize();

            Debug.DrawRay(pivot.position, direction * Mathf.Abs(targetPosition));

            /*/Collider[] colliders = Physics.OverlapSphere(camTransform.position, camSphereRadius, ignoreLayers);
            for (int i = 0; i < colliders.Length; i++)
            {
                Vector3 closestPoint = colliders[i].ClosestPoint(camTransform.position);
                float dis = Vector3.Distance(pivot.position, closestPoint);
                targetPosition = dis;
                break;
            }*/

            if (Physics.SphereCast(pivot.position, camSphereRadius, direction, out hit, Mathf.Abs(defaultPosition), ignoreLayers))
            {
                float dis = Vector3.Distance(pivot.position, hit.point);
                targetPosition = -(dis - camCollisionOffset);
            }

            if(Mathf.Abs(targetPosition) < minCollisionOffset)
            {
                targetPosition = minCollisionOffset;
            }
            

            camTransPosition.z = Mathf.Lerp(camTransform.localPosition.z, targetPosition, delta / 0.2f);

            camTransform.localPosition = camTransPosition;
        }
    }
}