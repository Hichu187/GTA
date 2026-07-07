using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Glider
{
    public class GliderInputAdapter : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Vehicle_Glider";

        private float   _pitch;
        private float   _roll;
        private float   _brake;
        private Vector2 _look;
        private bool    _exitPending;

        public GliderMoveCommand Command =>
            new GliderMoveCommand(_pitch, _roll, _brake, _look);

        public bool ConsumeExitPressed()
        {
            if (!_exitPending) return false;
            _exitPending = false;
            return true;
        }

        public void BindActions(IInputBinder binder)
        {
            binder.BindAxis1D("Pitch",
                onPerformed: v  => _pitch = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _pitch = 0f);

            binder.BindAxis1D("Roll",
                onPerformed: v  => _roll = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _roll = 0f);

            binder.BindAxis1D("Brake",
                onPerformed: v  => _brake = Mathf.Clamp01(v),
                onCanceled:  () => _brake = 0f);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look = v,
                onCanceled:  () => _look = Vector2.zero);

            binder.BindButton("Exit",
                onStarted: () => _exitPending = true);
        }
    }
}
