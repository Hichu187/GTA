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
        public  IPossessable Current => _current;

        public void Possess(IPossessable target)
        {
            if (target == null || ReferenceEquals(target, _current)) return;

            if (_current != null)
            {
                _current.CameraProvider.CameraRigChanged -= OnCameraRigChanged;
                _current.OnUnpossess();
            }

            _inputManager.SwitchCurrentActionMap(target.InputProvider.ActionMapName);
            target.OnPossess(PossessionContext.Default);
            target.InputProvider.BindActions(_inputManager);

            _cameraManager.ApplyContext(target.CameraProvider);
            _hudManager.ApplyContext(target.HUDProvider);

            target.CameraProvider.CameraRigChanged += OnCameraRigChanged;
            _current = target;
        }

        public void Unpossess()
        {
            if (_current != null)
            {
                _current.CameraProvider.CameraRigChanged -= OnCameraRigChanged;
                _current.OnUnpossess();
                _current = null;
            }
        }

        private void OnCameraRigChanged() => _cameraManager.ApplyContext(_current.CameraProvider);
    }
}
