using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Airplane
{
    public class AirplaneInputAdapter : MonoBehaviour, IInputActionMapProvider, ILookInjectable
    {
        public string ActionMapName => "Vehicle_Airplane";

        private float   _throttle;
        private float   _pitch;
        private float   _roll;
        private float   _yaw;
        private bool    _brake;
        private Vector2 _look;
        private bool    _exitPending;

        public AirplaneMoveCommand Command =>
            new AirplaneMoveCommand(_throttle, _pitch, _roll, _yaw, _brake, _look);

        public void InjectLook(Vector2 delta) => _look = delta;

        public bool ConsumeExitPressed()
        {
            if (!_exitPending) return false;
            _exitPending = false;
            return true;
        }

        public void BindActions(IInputBinder binder)
        {
            binder.BindAxis1D("Throttle",
                onPerformed: v  => _throttle = Mathf.Clamp01(v),
                onCanceled:  () => _throttle = 0f);

            binder.BindAxis1D("Pitch",
                onPerformed: v  => _pitch = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _pitch = 0f);

            binder.BindAxis1D("Roll",
                onPerformed: v  => _roll = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _roll = 0f);

            binder.BindAxis1D("Yaw",
                onPerformed: v  => _yaw = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _yaw = 0f);

            binder.BindButton("Brake",
                onStarted:  () => _brake = true,
                onCanceled: () => _brake = false);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look = v,
                onCanceled:  () => _look = Vector2.zero);

            binder.BindButton("Exit",
                onStarted: () => _exitPending = true);
        }
    }
}
