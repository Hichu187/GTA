using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;

namespace Game.Gameplay.Vehicles.Common.Stubs
{
    [RequireComponent(typeof(DummyVehicleInputProvider))]
    [RequireComponent(typeof(DummyVehicleCameraProvider))]
    [RequireComponent(typeof(DummyVehicleHUDProvider))]
    public class DummyVehicle : MonoBehaviour, IPossessable
    {
        private DummyVehicleInputProvider  _inputProvider;
        private DummyVehicleCameraProvider _cameraProvider;
        private DummyVehicleHUDProvider    _hudProvider;

        public ICameraContextProvider  CameraProvider => _cameraProvider;
        public IHUDContextProvider     HUDProvider    => _hudProvider;
        public IInputActionMapProvider InputProvider  => _inputProvider;

        private void Awake()
        {
            _inputProvider  = GetComponent<DummyVehicleInputProvider>();
            _cameraProvider = GetComponent<DummyVehicleCameraProvider>();
            _hudProvider    = GetComponent<DummyVehicleHUDProvider>();
        }

        public void OnPossess(PossessionContext context)  { }
        public void OnUnpossess() { }
    }
}
