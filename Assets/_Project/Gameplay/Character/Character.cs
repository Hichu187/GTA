using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Interaction;
using Game.Core.Possession;
using Game.Gameplay.Character.Locomotion;
using Game.Gameplay.Character.Stats;

namespace Game.Gameplay.Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterInputAdapter))]
    [RequireComponent(typeof(CharacterCameraProvider))]
    [RequireComponent(typeof(CharacterHUDProvider))]
    public class Character : MonoBehaviour, IPossessable, ICharacterStats, IInteractor
    {
        [SerializeField] private CharacterConfig _config = new CharacterConfig();
        [SerializeField] private float _maxHealth  = 100f;
        [SerializeField] private float _maxStamina = 100f;

        private CharacterController     _controller;
        private CharacterInputAdapter   _inputAdapter;
        private CharacterCameraProvider _cameraProvider;
        private CharacterHUDProvider    _hudProvider;
        private LocomotionStateMachine  _fsm;
        private LocomotionContext       _ctx;
        private Camera                  _mainCamera;

        private float _health;
        private float _stamina;
        private bool  _active;

        // ICharacterStats
        public float Health     => _health;
        public float MaxHealth  => _maxHealth;
        public float Stamina    => _stamina;
        public float MaxStamina => _maxStamina;

        // IInteractor
        public Transform InteractorTransform => transform;

        // IPossessable
        public ICameraContextProvider  CameraProvider => _cameraProvider;
        public IHUDContextProvider     HUDProvider    => _hudProvider;
        public IInputActionMapProvider InputProvider  => _inputAdapter;

        private void Awake()
        {
            _controller     = GetComponent<CharacterController>();
            _inputAdapter   = GetComponent<CharacterInputAdapter>();
            _cameraProvider = GetComponent<CharacterCameraProvider>();
            _hudProvider    = GetComponent<CharacterHUDProvider>();
            _hudProvider.StatsSource = this;

            _health  = _maxHealth;
            _stamina = _maxStamina;

            _ctx = new LocomotionContext
            {
                Controller = _controller,
                Config     = _config,
            };
            _fsm        = new LocomotionStateMachine();
            _mainCamera = Camera.main;
        }

        public void OnPossess(PossessionContext context)
        {
            _active = true;
            _controller.enabled = true;
            _fsm.Start(_ctx);
        }

        public void OnUnpossess()
        {
            _active = false;
            _controller.enabled = false;
        }

        private void Update()
        {
            if (!_active) return;

            _ctx.Command = _inputAdapter.Command;

            if (_inputAdapter.ConsumeToggleCamera())
                _cameraProvider.Toggle();

            _cameraProvider.HandleLook(_ctx.Command.LookAxis);

            _fsm.Tick(_ctx);

            // Camera-relative movement: project camera forward/right onto XZ plane.
            var camForward = _mainCamera != null
                ? Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized
                : Vector3.forward;
            var camRight = _mainCamera != null
                ? Vector3.ProjectOnPlane(_mainCamera.transform.right, Vector3.up).normalized
                : Vector3.right;

            var worldMove = camForward * _ctx.Command.MoveAxis.y
                          + camRight   * _ctx.Command.MoveAxis.x;
            var motion    = worldMove * _ctx.MoveSpeed + Vector3.up * _ctx.VerticalVelocity;
            _controller.Move(motion * Time.deltaTime);

            // Rotate character body to face movement direction smoothly.
            if (worldMove.magnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(worldMove),
                    15f * Time.deltaTime);
        }
    }
}
