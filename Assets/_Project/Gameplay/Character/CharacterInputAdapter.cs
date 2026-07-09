using UnityEngine;
using Game.Core.Input;
using Game.Core.Weapons;
using Game.Gameplay.Character.Input;

namespace Game.Gameplay.Character
{
    public class CharacterInputAdapter : MonoBehaviour, IInputActionMapProvider, ILookInjectable
    {
        public string ActionMapName => "Character";

        private Vector2 _move;
        private Vector2 _look;
        private bool    _jumpPending;
        private bool    _sprintHeld;
        private bool    _crouchPressed;
        private bool    _crouchPending;
        private bool    _interactPressed;
        private bool    _interactPending;
        private bool    _toggleCameraPending;

        // Weapon input
        private bool  _fireHeld;
        private bool  _aimHeld;
        private bool  _reloadPending;
        private float _switchDelta;
        private bool  _throwPending;

        public CharacterMoveCommand Command
        {
            get
            {
                var cmd = new CharacterMoveCommand(
                    _move, _look, _jumpPending, _sprintHeld, _crouchPressed, _interactPressed);
                _jumpPending = false;   // one-shot: consume after read
                return cmd;
            }
        }

        public WeaponCommand WeaponCommand
        {
            get
            {
                var cmd = new WeaponCommand(
                    firePressed:   _fireHeld,
                    aimHeld:       _aimHeld,
                    reloadPressed: _reloadPending,
                    switchDelta:   _switchDelta,
                    throwPressed:  _throwPending);
                _throwPending = false;   // one-shot: consume after read
                return cmd;
            }
        }

        // Called by LookDragHandler (mobile) to override the look axis each frame.
        public void InjectLook(Vector2 v) => _look = v;

        public bool ConsumeToggleCamera()
        {
            if (!_toggleCameraPending) return false;
            _toggleCameraPending = false;
            return true;
        }

        public bool ConsumeCrouch()
        {
            if (!_crouchPending) return false;
            _crouchPending = false;
            return true;
        }

        public bool ConsumeInteract()
        {
            if (!_interactPending) return false;
            _interactPending = false;
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
                onStarted:   () => _jumpPending = true);

            binder.BindButton("Sprint",
                onStarted:   () => _sprintHeld    = true,
                onCanceled:  () => _sprintHeld    = false);

            binder.BindButton("Crouch",
                onStarted:   () => { _crouchPressed = true; _crouchPending = true; },
                onCanceled:  () =>    _crouchPressed = false);

            binder.BindButton("Interact",
                onStarted:   () => { _interactPressed = true;  _interactPending = true; },
                onCanceled:  () =>   _interactPressed = false);

            binder.BindButton("ToggleCamera",
                onStarted:   () => _toggleCameraPending = true);

            binder.BindButton("Fire",
                onStarted:   () => _fireHeld = true,
                onCanceled:  () => _fireHeld = false);

            binder.BindButton("Aim",
                onStarted:   () => _aimHeld = true,
                onCanceled:  () => _aimHeld = false);

            binder.BindButton("Reload",
                onStarted:   () => _reloadPending = true);

            binder.BindAxis1D("SwitchWeapon",
                onPerformed: v  => _switchDelta = v,
                onCanceled:  () => _switchDelta = 0f);

            binder.BindButton("Throw",
                onStarted:   () => _throwPending = true);
        }
    }
}
