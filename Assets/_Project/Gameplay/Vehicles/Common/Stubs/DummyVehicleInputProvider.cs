using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Common.Stubs
{
    public class DummyVehicleInputProvider : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Dummy_Vehicle";

        public void BindActions(IInputBinder binder) { }
    }
}
