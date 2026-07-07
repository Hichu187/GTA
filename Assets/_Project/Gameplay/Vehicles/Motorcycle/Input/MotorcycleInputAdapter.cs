using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    public class MotorcycleInputAdapter : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Vehicle_Motorcycle";

        private float   _throttle;
        private float   _brake;
        private float   _steer;
        private Vector2 _look;
        private bool    _exitPending;

        public MotorcycleMoveCommand Command =>
            new MotorcycleMoveCommand(_throttle, _brake, _steer, _look);

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

            binder.BindAxis1D("Brake",
                onPerformed: v  => _brake = Mathf.Clamp01(v),
                onCanceled:  () => _brake = 0f);

            binder.BindAxis1D("Steer",
                onPerformed: v  => _steer = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _steer = 0f);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look = v,
                onCanceled:  () => _look = Vector2.zero);

            binder.BindButton("Exit",
                onStarted: () => _exitPending = true);
        }
    }
}
