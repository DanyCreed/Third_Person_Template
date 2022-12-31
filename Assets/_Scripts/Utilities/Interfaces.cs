using UnityEngine;
using System.Collections;

namespace TPT
{
    public interface ILockable
    {
        bool IsAlive();
        Transform GetLockOnTarget(Transform from);
    }

    public interface IDamageable
    {
        void OnDamage(ActionContainer action);
    }

    public interface IInteractable
    {
        void OnInteract(InputManager inp);

        InteractionType GetInteractionType();
    }

    public interface IHaveAction
    {
        ActionContainer GetActionContainer();
    }
}