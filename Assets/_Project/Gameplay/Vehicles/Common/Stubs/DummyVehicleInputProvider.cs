using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Common.Stubs
{
    public class DummyVehicleInputProvider : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Dummy_Vehicle";

        // Stub: direct keyboard poll until a proper "Dummy_Vehicle" input map is wired.
        public bool ExitPressed =>
            Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;

        public void BindActions(IInputBinder binder) { }
    }
}
