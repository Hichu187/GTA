using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Helicopter
{
    public class HelicopterInputAdapter : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Vehicle_Helicopter";

        private Vector2 _horizontal;
        private float   _vertical;
        private float   _yaw;
        private bool    _takeOffPending;
        private bool    _exitPending;
        private Vector2 _look;

        public HelicopterMoveCommand Command =>
            new HelicopterMoveCommand(_horizontal, _vertical, _yaw, _takeOffPending, _look);

        public bool ConsumeExitPressed()
        {
            if (!_exitPending) return false;
            _exitPending = false;
            return true;
        }

        public bool ConsumeTakeOff()
        {
            if (!_takeOffPending) return false;
            _takeOffPending = false;
            return true;
        }

        public void BindActions(IInputBinder binder)
        {
            binder.BindAxis2D("Horizontal",
                onPerformed: v  => _horizontal = v,
                onCanceled:  () => _horizontal = Vector2.zero);

            binder.BindAxis1D("Vertical",
                onPerformed: v  => _vertical = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _vertical = 0f);

            binder.BindAxis1D("Yaw",
                onPerformed: v  => _yaw = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _yaw = 0f);

            binder.BindButton("TakeOff",
                onStarted: () => _takeOffPending = true);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look = v,
                onCanceled:  () => _look = Vector2.zero);

            binder.BindButton("Exit",
                onStarted: () => _exitPending = true);
        }
    }
}
