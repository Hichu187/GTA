using UnityEngine;
using Game.Core.Possession;
using Game.Systems.Input;
using Game.Systems.Camera;
using Game.Systems.HUD;

namespace Game.Services
{
    public class PossessionManager : MonoBehaviour
    {
        [SerializeField] private InputManager  _inputManager;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private HUDManager    _hudManager;

        private IPossessable _current;
        private IPossessable _previous;

        public IPossessable Current  => _current;
        public IPossessable Previous => _previous;

        public void Possess(IPossessable target)
        {
            if (target == null || ReferenceEquals(target, _current)) return;

            // Capture velocity BEFORE OnUnpossess so the receiving entity can inherit momentum.
            Vector3 exitVelocity = Vector3.zero;
            if (_current != null)
            {
                var rb = (_current as MonoBehaviour)?.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                    exitVelocity = rb.linearVelocity;

                _current.CameraProvider.CameraRigChanged -= OnCameraRigChanged;
                _current.OnUnpossess(new PossessionContext(0, target.EnterAnchor));
            }

            _previous = _current;
            _inputManager.SwitchCurrentActionMap(target.InputProvider.ActionMapName);

            // Inject exit callback so possessed entity can trigger return to previous.
            target.OnPossess(new PossessionContext(
                playerIndex:     0,
                anchorPoint:     _previous?.ExitAnchor,
                onExitRequested: PossessPrevious,
                exitVelocity:    exitVelocity));

            target.InputProvider.BindActions(_inputManager);
            _cameraManager.ApplyContext(target.CameraProvider);
            _hudManager.ApplyContext(target.HUDProvider);

            target.CameraProvider.CameraRigChanged += OnCameraRigChanged;
            _current = target;
        }

        public void PossessPrevious()
        {
            if (_previous != null) Possess(_previous);
        }

        public void Unpossess()
        {
            if (_current != null)
            {
                _current.CameraProvider.CameraRigChanged -= OnCameraRigChanged;
                _current.OnUnpossess(PossessionContext.Default);
                _current = null;
            }
        }

        private void OnCameraRigChanged() => _cameraManager.ApplyContext(_current.CameraProvider);
    }
}
