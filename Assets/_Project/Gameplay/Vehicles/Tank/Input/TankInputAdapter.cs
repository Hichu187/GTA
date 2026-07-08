using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Tank
{
    public class TankInputAdapter : MonoBehaviour, IInputActionMapProvider, ILookInjectable
    {
        public string ActionMapName => "Vehicle_Tank";

        private float   _throttle;
        private float   _steer;
        private Vector2 _look;
        private bool    _firePending;
        private bool    _exitPending;

        public TankMoveCommand Command =>
            new TankMoveCommand(_throttle, _steer, _look, _firePending);

        public void InjectLook(Vector2 delta) => _look = delta;

        public bool ConsumeFirePressed()
        {
            if (!_firePending) return false;
            _firePending = false;
            return true;
        }

        public bool ConsumeExitPressed()
        {
            if (!_exitPending) return false;
            _exitPending = false;
            return true;
        }

        public void BindActions(IInputBinder binder)
        {
            // Throttle: W/S or left stick Y — positive forward, negative reverse
            binder.BindAxis1D("Throttle",
                onPerformed: v  => _throttle    = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _throttle    = 0f);

            binder.BindAxis1D("Steer",
                onPerformed: v  => _steer       = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _steer        = 0f);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look        = v,
                onCanceled:  () => _look         = Vector2.zero);

            binder.BindButton("Fire",
                onStarted:  () => _firePending  = true);

            binder.BindButton("Exit",
                onStarted:  () => _exitPending  = true);
        }
    }
}
