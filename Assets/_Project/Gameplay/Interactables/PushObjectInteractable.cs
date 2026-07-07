using UnityEngine;
using Game.Core.Interaction;

namespace Game.Gameplay.Interactables
{
    /// <summary>
    /// Locks the interactor's locomotion while this interaction is active.
    /// Press E to start pushing; press E again to stop (Character handles the unlock).
    /// </summary>
    public class PushObjectInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(IInteractor actor) => true;

        public void Interact(IInteractor actor)
        {
            actor.SetLocomotionLocked(true);
        }
    }
}
