using UnityEngine;
using Game.Core.Interaction;

namespace Game.Gameplay.Interactables
{
    /// <summary>
    /// Teleports the interactor to a seat position and locks locomotion.
    /// Press E again to stand up (Character handles the unlock).
    /// </summary>
    public class SitInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _seatPoint;

        public bool CanInteract(IInteractor actor) => true;

        public void Interact(IInteractor actor)
        {
            if (_seatPoint != null)
                actor.InteractorTransform.SetPositionAndRotation(
                    _seatPoint.position, _seatPoint.rotation);

            actor.SetLocomotionLocked(true);
        }
    }
}
