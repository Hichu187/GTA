using UnityEngine;
using Game.Core.Interaction;
using Game.Core.Possession;
using Game.Services;

namespace Game.Gameplay.Interactables
{
    /// <summary>
    /// Attach to a Vehicle GO. When Character walks up and presses E,
    /// PossessionManager switches control to the vehicle.
    /// </summary>
    public class EnterVehicleInteractable : MonoBehaviour, IInteractable
    {
        // Assign the vehicle MonoBehaviour (which implements IPossessable) in Inspector.
        [SerializeField] private MonoBehaviour _vehicle;

        private IPossessable _possessable;

        private void Awake()
        {
            _possessable = _vehicle as IPossessable
                        ?? _vehicle?.GetComponent<IPossessable>();
        }

        public bool CanInteract(IInteractor actor)
        {
            if (_possessable == null) return false;

            var pm = GameplayServiceLocator.Current?.PossessionManager;
            // Cannot enter if already the current possessable (already inside).
            return pm != null && !ReferenceEquals(_possessable, pm.Current);
        }

        public void Interact(IInteractor actor)
        {
            GameplayServiceLocator.Current?.PossessionManager.Possess(_possessable);
        }
    }
}
