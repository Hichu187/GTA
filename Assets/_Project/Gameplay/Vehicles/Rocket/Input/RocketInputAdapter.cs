using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Rocket
{
    public class RocketInputAdapter : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Vehicle_Rocket";

        private float   _throttle;
        private float   _pitch;
        private float   _roll;
        private Vector2 _look;
        private bool    _exitPending;

        public RocketMoveCommand Command =>
            new RocketMoveCommand(_throttle, _pitch, _roll, _look);

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

            binder.BindAxis2D("Look",
                onPerformed: v  => _look = v,
                onCanceled:  () => _look = Vector2.zero);

            binder.BindButton("Exit",
                onStarted: () => _exitPending = true);
        }
    }
}
