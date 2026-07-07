using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core.Possession;

namespace Game.Services
{
    // Phase 2 milestone only — press F to toggle possession between Character and DummyVehicle.
    // Replaced by EnterVehicleInteractable in Phase 4.
    public class PossessionTester : MonoBehaviour
    {
        [SerializeField] private PossessionManager _possessionManager;
        [SerializeField] private MonoBehaviour     _character;
        [SerializeField] private MonoBehaviour     _vehicle;

        private bool _inVehicle;

        private void Update()
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
                Toggle();
        }

        private void Toggle()
        {
            _inVehicle = !_inVehicle;
            var source = _inVehicle ? _vehicle : _character;
            if (source != null && source.TryGetComponent(out IPossessable target))
                _possessionManager.Possess(target);
        }
    }
}
