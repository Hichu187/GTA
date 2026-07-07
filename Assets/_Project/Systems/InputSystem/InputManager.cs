using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core.Input;

namespace Game.Systems.Input
{
    public class InputManager : MonoBehaviour, IInputBinder
    {
        [SerializeField] private InputActionAsset _actionAsset;

        private InputActionMap _currentMap;
        private readonly List<Action> _unbindActions = new();

        // Called by PossessionManager before each Possess: unbinds old, switches map, ready for BindActions.
        public void SwitchCurrentActionMap(string mapName)
        {
            foreach (var unbind in _unbindActions) unbind();
            _unbindActions.Clear();

            _currentMap?.Disable();
            _currentMap = _actionAsset.FindActionMap(mapName, throwIfNotFound: true);
            _currentMap.Enable();
        }

        public void BindAxis2D(string actionName, Action<Vector2> onPerformed, Action onCanceled = null)
        {
            var action = RequireAction(actionName);

            void Performed(InputAction.CallbackContext ctx) => onPerformed(ctx.ReadValue<Vector2>());
            void Canceled(InputAction.CallbackContext ctx)  => onCanceled?.Invoke();

            action.performed += Performed;
            action.canceled  += Canceled;
            _unbindActions.Add(() => { action.performed -= Performed; action.canceled -= Canceled; });
        }

        public void BindAxis1D(string actionName, Action<float> onPerformed, Action onCanceled = null)
        {
            var action = RequireAction(actionName);

            void Performed(InputAction.CallbackContext ctx) => onPerformed(ctx.ReadValue<float>());
            void Canceled(InputAction.CallbackContext ctx)  => onCanceled?.Invoke();

            action.performed += Performed;
            action.canceled  += Canceled;
            _unbindActions.Add(() => { action.performed -= Performed; action.canceled -= Canceled; });
        }

        public void BindButton(string actionName, Action onStarted = null, Action onPerformed = null, Action onCanceled = null)
        {
            var action = RequireAction(actionName);

            void Started(InputAction.CallbackContext ctx)   => onStarted?.Invoke();
            void Performed(InputAction.CallbackContext ctx) => onPerformed?.Invoke();
            void Canceled(InputAction.CallbackContext ctx)  => onCanceled?.Invoke();

            action.started   += Started;
            action.performed += Performed;
            action.canceled  += Canceled;
            _unbindActions.Add(() =>
            {
                action.started   -= Started;
                action.performed -= Performed;
                action.canceled  -= Canceled;
            });
        }

        private void OnDestroy()
        {
            foreach (var unbind in _unbindActions) unbind();
            _currentMap?.Disable();
        }

        private InputAction RequireAction(string actionName)
        {
            if (_currentMap == null)
                throw new InvalidOperationException($"Call SwitchCurrentActionMap before binding '{actionName}'.");
            return _currentMap.FindAction(actionName, throwIfNotFound: true);
        }
    }
}
