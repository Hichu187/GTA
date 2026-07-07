using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Common.Stubs
{
    [RequireComponent(typeof(DummyVehicleInputProvider))]
    [RequireComponent(typeof(DummyVehicleCameraProvider))]
    [RequireComponent(typeof(DummyVehicleHUDProvider))]
    public class DummyVehicle : VehicleControllerBase
    {
        private DummyVehicleInputProvider  _inputProvider;
        private DummyVehicleCameraProvider _cameraProvider;
        private DummyVehicleHUDProvider    _hudProvider;

        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputProvider;

        protected override void Awake()
        {
            base.Awake();
            _inputProvider  = GetComponent<DummyVehicleInputProvider>();
            _cameraProvider = GetComponent<DummyVehicleCameraProvider>();
            _hudProvider    = GetComponent<DummyVehicleHUDProvider>();
        }

        protected override void OnOccupiedUpdate()
        {
            if (_inputProvider.ExitPressed)
                _onExitRequested?.Invoke();
        }
    }
}
