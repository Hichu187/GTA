using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Vehicles.Helicopter
{
    public class HelicopterInputAdapter : MonoBehaviour, IInputActionMapProvider, ILookInjectable
    {
        public string ActionMapName => "Vehicle_Helicopter";

        private Vector2 _horizontal;
        private float   _yaw;
        private bool    _engineUp;
        private bool    _engineDown;
        private bool    _exitPending;
        private Vector2 _look;

        public HelicopterMoveCommand Command =>
            new HelicopterMoveCommand(_horizontal, _yaw, _engineUp, _engineDown, _look);

        public void InjectLook(Vector2 delta) => _look = delta;

        public bool ConsumeExitPressed()
        {
            if (!_exitPending) return false;
            _exitPending = false;
            return true;
        }

        public void BindActions(IInputBinder binder)
        {
            binder.BindAxis2D("Horizontal",
                onPerformed: v  => _horizontal = v,
                onCanceled:  () => _horizontal = Vector2.zero);

            binder.BindAxis1D("Yaw",
                onPerformed: v  => _yaw = Mathf.Clamp(v, -1f, 1f),
                onCanceled:  () => _yaw = 0f);

            binder.BindButton("EngineUp",
                onStarted:  () => _engineUp = true,
                onCanceled: () => _engineUp = false);

            binder.BindButton("EngineDown",
                onStarted:  () => _engineDown = true,
                onCanceled: () => _engineDown = false);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look = v,
                onCanceled:  () => _look = Vector2.zero);

            binder.BindButton("Exit",
                onStarted: () => _exitPending = true);
        }
    }
}
