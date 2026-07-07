using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;

namespace Game.Gameplay.Character.Stubs
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterStubInputProvider))]
    [RequireComponent(typeof(CharacterStubCameraProvider))]
    [RequireComponent(typeof(CharacterStubHUDProvider))]
    public class CharacterStub : MonoBehaviour, IPossessable
    {
        private CharacterController        _controller;
        private CharacterStubInputProvider _inputProvider;
        private CharacterStubCameraProvider _cameraProvider;
        private CharacterStubHUDProvider   _hudProvider;

        private bool _active;

        public ICameraContextProvider  CameraProvider => _cameraProvider;
        public IHUDContextProvider     HUDProvider    => _hudProvider;
        public IInputActionMapProvider InputProvider  => _inputProvider;

        private void Awake()
        {
            _controller     = GetComponent<CharacterController>();
            _inputProvider  = GetComponent<CharacterStubInputProvider>();
            _cameraProvider = GetComponent<CharacterStubCameraProvider>();
            _hudProvider    = GetComponent<CharacterStubHUDProvider>();
        }

        public void OnPossess(PossessionContext context)
        {
            _active = true;
            _controller.enabled = true;
        }

        public void OnUnpossess()
        {
            _active = false;
            _controller.enabled = false;
        }

        private void Update()
        {
            if (!_active) return;

            var horizontal = new Vector3(_inputProvider.MoveInput.x, 0f, _inputProvider.MoveInput.y);
            _controller.Move(horizontal * 5f * Time.deltaTime);
            _controller.Move(Physics.gravity * Time.deltaTime);
        }
    }
}
