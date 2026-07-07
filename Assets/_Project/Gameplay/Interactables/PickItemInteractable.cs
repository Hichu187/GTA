using UnityEngine;
using Game.Core.Interaction;

namespace Game.Gameplay.Interactables
{
    /// <summary>
    /// Destroys (picks up) the GameObject when interacted with.
    /// Extend or replace with inventory logic in a later phase.
    /// </summary>
    public class PickItemInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(IInteractor actor) => true;

        public void Interact(IInteractor actor) => Destroy(gameObject);
    }
}
