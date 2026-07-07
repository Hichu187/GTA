using UnityEngine;

namespace Game.Core.Interaction
{
    public interface IInteractor
    {
        Transform InteractorTransform { get; }

        /// <summary>Called by interactables that need to freeze/unfreeze movement (Push, Sit...).</summary>
        void SetLocomotionLocked(bool locked);
    }
}
