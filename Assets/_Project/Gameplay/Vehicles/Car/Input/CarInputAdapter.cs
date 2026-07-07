using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Car
{
    public class CarInputAdapter : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Vehicle_Car";

        private float   _throttle;
        private float   _brake;
        private float   _steer;
        private Vector2 _look;
        private bool    _hornPressed;
        private bool    _exitPending;

        public CarMoveCommand Command =>
            new CarMoveCommand(_throttle, _brake, _steer, _look, _hornPressed);

        public bool ConsumeExitPressed()
        {
            if (!_exitPending) return false;
            _exitPending = false;
            return true;
        }

        public void BindActions(IInputBinder binder)
        {
            binder.BindAxis1D("Throttle",
                onPerformed: v  => _throttle    = Mathf.Clamp01(v),
                onCanceled:  () => _throttle    = 0f);

            binder.BindAxis1D("Brake",
                onPerformed: v  => _brake       = Mathf.Clamp01(v),
                onCanceled:  () => _brake        = 0f);

            binder.BindAxis1D("Steer",
                onPerformed: v  => _steer       = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _steer        = 0f);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look        = v,
                onCanceled:  () => _look         = Vector2.zero);

            binder.BindButton("Horn",
                onStarted:  () => _hornPressed  = true,
                onCanceled: () => _hornPressed   = false);

            binder.BindButton("Exit",
                onStarted:  () => _exitPending  = true);
        }
    }
}
