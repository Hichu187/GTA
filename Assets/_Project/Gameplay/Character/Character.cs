using System;
using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Interaction;
using Game.Core.Persistence;
using Game.Core.Possession;
using Game.Core.Weapons;
using Game.Gameplay.Character.Abilities;
using Game.Gameplay.Character.Animation;
using Game.Gameplay.Character.Locomotion;
using Game.Gameplay.Character.Stats;
using Game.Services;

namespace Game.Gameplay.Character
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterInputAdapter))]
    [RequireComponent(typeof(CharacterCameraProvider))]
    [RequireComponent(typeof(CharacterHUDProvider))]
    [RequireComponent(typeof(InteractionDetector))]
    public class Character : MonoBehaviour, IPossessable, ICharacterStats, IInteractor, IDamageable, ISaveable, ICharacterAnimationData
    {
        [SerializeField] private CharacterConfig _config = new CharacterConfig();
        [SerializeField] private float _maxHealth  = 100f;
        [SerializeField] private float _maxStamina = 100f;

        private CharacterController     _controller;
        private CharacterInputAdapter   _inputAdapter;
        private CharacterCameraProvider _cameraProvider;
        private CharacterHUDProvider    _hudProvider;
        private InteractionDetector     _interactionDetector;
        private IWeaponHolder           _weaponHolder;
        private LocomotionStateMachine  _fsm;
        private LocomotionContext       _ctx;
        private Camera                  _mainCamera;

        private AbilitySystem         _abilitySystem;
        private CrouchAbility         _crouchAbility;
        private InteractAbility       _interactAbility;
        private LocomotionLockAbility _lockAbility;

        private float   _health;
        private float   _stamina;
        private bool    _active;
        private Vector3 _launchVelocity;

        // ICharacterAnimationData
        public float             MoveSpeed       => _ctx?.MoveSpeed ?? 0f;
        public float             MaxMoveSpeed    => _config.SprintSpeed;
        public bool              IsGrounded      => _ctx?.IsGrounded ?? false;
        public bool              IsCrouching     => _ctx?.CrouchRequested ?? false;
        public LocomotionStateId LocomotionState => _fsm?.CurrentId ?? LocomotionStateId.Idle;
        public Vector2           MoveInput       => _ctx != null ? _ctx.Command.MoveAxis : Vector2.zero;

        // ICharacterStats
        public float Health     => _health;
        public float MaxHealth  => _maxHealth;
        public float Stamina    => _stamina;
        public float MaxStamina => _maxStamina;

        // IDamageable
        public float CurrentHealth => _health;
        public void TakeDamage(float amount, DamageType type)
        {
            _health = Mathf.Clamp(_health - amount, 0f, _maxHealth);
        }

        // ISaveable
        public string SaveKey => "Character_" + gameObject.name;

        public string CaptureState()
        {
            var pos = transform.position;
            var data = new CharacterSaveData
            {
                health  = _health,
                stamina = _stamina,
                posX    = pos.x,
                posY    = pos.y,
                posZ    = pos.z,
                rotY    = transform.eulerAngles.y,
            };
            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            var data = JsonUtility.FromJson<CharacterSaveData>(json);
            _health  = Mathf.Clamp(data.health,  0f, _maxHealth);
            _stamina = Mathf.Clamp(data.stamina, 0f, _maxStamina);
            transform.SetPositionAndRotation(
                new Vector3(data.posX, data.posY, data.posZ),
                Quaternion.Euler(0f, data.rotY, 0f));
        }

        [Serializable]
        private class CharacterSaveData
        {
            public float health;
            public float stamina;
            public float posX, posY, posZ;
            public float rotY;
        }

        // IInteractor
        public Transform InteractorTransform => transform;
        public void SetLocomotionLocked(bool locked)
        {
            if (locked) _lockAbility.Activate();
            else        _lockAbility.Cancel();
        }

        // IPossessable
        public Transform               EnterAnchor    => null;
        public Transform               ExitAnchor     => null;
        public ICameraContextProvider  CameraProvider => _cameraProvider;
        public IHUDContextProvider     HUDProvider    => _hudProvider;
        public IInputActionMapProvider InputProvider  => _inputAdapter;

        private void Awake()
        {
            _controller          = GetComponent<CharacterController>();
            _inputAdapter        = GetComponent<CharacterInputAdapter>();
            _cameraProvider      = GetComponent<CharacterCameraProvider>();
            _hudProvider         = GetComponent<CharacterHUDProvider>();
            _interactionDetector = GetComponent<InteractionDetector>();
            _weaponHolder        = GetComponent<IWeaponHolder>();   // null if not present
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

            _abilitySystem   = gameObject.AddComponent<AbilitySystem>();
            _crouchAbility   = new CrouchAbility();
            _interactAbility = new InteractAbility(_interactionDetector, this);
            _lockAbility     = new LocomotionLockAbility();
            _abilitySystem.Register(_crouchAbility);
            _abilitySystem.Register(_interactAbility);
            _abilitySystem.Register(_lockAbility);
        }

        private void Start()
        {
            GameplayServiceLocator.Current?.SaveService?.Register(this);
        }

        private void OnDestroy()
        {
            GameplayServiceLocator.Current?.SaveService?.Unregister(this);
        }

        public void OnPossess(PossessionContext context)
        {
            // Unparent from any vehicle seat before re-enabling movement.
            transform.SetParent(null);

            if (context.AnchorPoint != null)
                transform.SetPositionAndRotation(
                    context.AnchorPoint.position,
                    context.AnchorPoint.rotation);

            // Inherit horizontal momentum from the vehicle (XZ only — vertical handled by FSM gravity).
            _launchVelocity = new Vector3(context.ExitVelocity.x, 0f, context.ExitVelocity.z);

            _active = true;
            _controller.enabled = true;
            _fsm.Start(_ctx);
        }

        public void OnUnpossess(PossessionContext context)
        {
            _active = false;
            _controller.enabled = false;

            if (context.AnchorPoint != null)
            {
                // Parent into vehicle seat so character moves with the vehicle.
                transform.SetParent(context.AnchorPoint);
                transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

        private void Update()
        {
            if (!_active) return;

            _ctx.Command = _inputAdapter.Command;

            if (_inputAdapter.ConsumeToggleCamera())
                _cameraProvider.Toggle();

            _cameraProvider.HandleLook(_ctx.Command.LookAxis);

            // FP: horizontal look is body rotation; TP: 0 (no-op).
            transform.Rotate(0f, _cameraProvider.ConsumeFPBodyYawDelta(), 0f);

            if (_inputAdapter.ConsumeCrouch())
            {
                if (_crouchAbility.IsActive) _crouchAbility.Cancel();
                else                         _crouchAbility.Activate();
            }
            _ctx.CrouchRequested = _crouchAbility.IsActive;

            if (_inputAdapter.ConsumeInteract())
            {
                if (_abilitySystem.IsLocomotionLocked)
                    _abilitySystem.CancelAllLocking();
                else
                    _interactAbility.Activate();
            }

            if (!_abilitySystem.IsLocomotionLocked)
                _fsm.Tick(_ctx);
            else
                _ctx.MoveSpeed = 0f;

            // Camera-relative movement: project camera forward/right onto XZ plane.
            var camForward = _mainCamera != null
                ? Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized
                : Vector3.forward;
            var camRight = _mainCamera != null
                ? Vector3.ProjectOnPlane(_mainCamera.transform.right, Vector3.up).normalized
                : Vector3.right;

            var worldMove = camForward * _ctx.Command.MoveAxis.y
                          + camRight   * _ctx.Command.MoveAxis.x;

            // Decay exit impulse (15 m/s² deceleration — e.g. 150 km/h tumble stops in ~2.8 s).
            if (_launchVelocity.sqrMagnitude > 0.01f)
                _launchVelocity = Vector3.MoveTowards(_launchVelocity, Vector3.zero, 15f * Time.deltaTime);
            else
                _launchVelocity = Vector3.zero;

            var motion = worldMove * _ctx.MoveSpeed + Vector3.up * _ctx.VerticalVelocity + _launchVelocity;
            if (_controller.enabled)
                _controller.Move(motion * Time.deltaTime);

            // TP only: rotate body to face movement direction.
            // FP: body already tracks look direction via ConsumeFPBodyYawDelta above.
            if (_cameraProvider.CurrentMode == CharacterCameraProvider.CameraMode.ThirdPerson
                && worldMove.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(worldMove),
                    15f * Time.deltaTime);
            }

            // Weapon tick — WeaponHolder handles its own logic when present
            _weaponHolder?.Tick(_inputAdapter.WeaponCommand);
        }
    }
}
