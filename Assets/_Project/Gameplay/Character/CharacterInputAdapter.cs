using UnityEngine;
using Game.Core.Input;
using Game.Gameplay.Character.Input;

namespace Game.Gameplay.Character
{
    public class CharacterInputAdapter : MonoBehaviour, IInputActionMapProvider
    {
        public string ActionMapName => "Character";

        private Vector2 _move;
        private Vector2 _look;
        private bool    _jumpPressed;
        private bool    _sprintHeld;
        private bool    _crouchPressed;
        private bool    _interactPressed;
        private bool    _toggleCameraConsumed;
        private bool    _toggleCameraPending;

        public CharacterMoveCommand Command => new CharacterMoveCommand(
            _move, _look, _jumpPressed, _sprintHeld, _crouchPressed, _interactPressed);

        public bool ConsumeToggleCamera()
        {
            if (!_toggleCameraPending) return false;
            _toggleCameraPending = false;
            return true;
        }

        public void BindActions(IInputBinder binder)
        {
            binder.BindAxis2D("Move",
                onPerformed: v  => _move = v,
                onCanceled:  () => _move = Vector2.zero);

            binder.BindAxis2D("Look",
                onPerformed: v  => _look = v,
                onCanceled:  () => _look = Vector2.zero);

            binder.BindButton("Jump",
                onStarted:   () => _jumpPressed   = true,
                onCanceled:  () => _jumpPressed   = false);

            binder.BindButton("Sprint",
                onStarted:   () => _sprintHeld    = true,
                onCanceled:  () => _sprintHeld    = false);

            binder.BindButton("Crouch",
                onStarted:   () => _crouchPressed = true,
                onCanceled:  () => _crouchPressed = false);

            binder.BindButton("Interact",
                onStarted:   () => _interactPressed = true,
                onCanceled:  () => _interactPressed = false);

            binder.BindButton("ToggleCamera",
                onStarted:   () => _toggleCameraPending = true);
        }
    }
}
