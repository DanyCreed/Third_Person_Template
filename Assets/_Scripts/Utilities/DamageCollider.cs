using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPT
{
    public class DamageCollider : MonoBehaviour
    {
        IHaveAction owner;
        private void Start()
        {
            owner = GetComponentInParent<IHaveAction>();
        }

        private void OnTriggerEnter(Collider other)
        {
            IDamageable damageAble = other.GetComponentInParent<IDamageable>();
            if(damageAble != null)
            {
                damageAble.OnDamage(owner.GetActionContainer());
                Debug.Log("hit");
            }
        }
    }
}