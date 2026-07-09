using System;
using System.Linq;
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
using Game.Core;
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

        private CharacterVehicleRider _vehicleRider;
        private AbilitySystem         _abilitySystem;
        private CrouchAbility         _crouchAbility;
        private InteractAbility       _interactAbility;
        private LocomotionLockAbility _lockAbility;

        private float       _health;
        private float       _stamina;
        private bool        _active;
        private Vector3     _launchVelocity;
        private float       _peakFallSpeed;
        private bool        _wasGrounded;
        private bool        _isDead;
        private Rigidbody[] _ragdollBodies;
        private Collider[]  _ragdollColliders;

        // ICharacterAnimationData
        public bool              IsAnimationActive => _active;
        public float             MoveSpeed         => _ctx?.MoveSpeed ?? 0f;
        public float             MaxMoveSpeed      => _config.SprintSpeed;
        public bool              IsGrounded        => _ctx?.IsGrounded ?? false;
        public bool              IsCrouching       => _ctx?.CrouchRequested ?? false;
        public LocomotionStateId LocomotionState   => _fsm?.CurrentId ?? LocomotionStateId.Idle;
        public Vector2           MoveInput         => _ctx != null ? _ctx.Command.MoveAxis : Vector2.zero;

        // ICharacterStats
        public float Health     => _health;
        public float MaxHealth  => _maxHealth;
        public float Stamina    => _stamina;
        public float MaxStamina => _maxStamina;

        // IDamageable
        public float CurrentHealth => _health;
        public void TakeDamage(float amount, DamageType type)
        {
            if (_isDead || amount <= 0f) return;
            _health = Mathf.Clamp(_health - amount, 0f, _maxHealth);
            if (_health <= 0f) Die();
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

            // Cache bone Rigidbodies + Colliders for ragdoll (created via Ragdoll Wizard).
            // Colliders are disabled while alive so they don't interfere with vehicles.
            _ragdollBodies    = GetComponentsInChildren<Rigidbody>(true);
            // Only colliders on bone GameObjects (those with a Rigidbody) —
            // avoids disabling the CharacterController capsule on the root.
            _ragdollColliders = _ragdollBodies
                .SelectMany(rb => rb.GetComponents<Collider>())
                .ToArray();
            SetRagdollActive(false);

            _vehicleRider    = gameObject.AddComponent<CharacterVehicleRider>();
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
            _vehicleRider.OnExitVehicle();

            // Unparent from any vehicle seat before re-enabling movement.
            transform.SetParent(null);

            if (context.AnchorPoint != null)
                transform.SetPositionAndRotation(
                    context.AnchorPoint.position,
                    context.AnchorPoint.rotation);

            // Inherit momentum only when ejected at speed — normal exits stay clean.
            var exitHorizontal = new Vector3(context.ExitVelocity.x, 0f, context.ExitVelocity.z);
            _launchVelocity = exitHorizontal.magnitude > _config.SafeExitSpeed ? exitHorizontal : Vector3.zero;

            _active = true;
            _controller.enabled = true;
            _fsm.Start(_ctx);

            // Vehicle exit damage — horizontal impact when thrown from a moving vehicle
            float exitSpeed = exitHorizontal.magnitude;
            if (exitSpeed > _config.SafeExitSpeed)
            {
                float raw    = (exitSpeed - _config.SafeExitSpeed) * _config.DamagePerMps;
                float damage = Mathf.Min(raw, _health - _config.MinSurviveHealth);
                if (damage > 0f)
                {
                    TakeDamage(damage, DamageType.Fall);
                    Debug.Log($"[VehicleExit] speed={exitSpeed:F1} m/s  dmg={damage:F1}  hp={_health:F1}/{_maxHealth}");
                }
            }
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

            // Read rider config from the vehicle we just entered (if it exposes one).
            // Null source → default: hide character.
            var riderSource = context.AnchorPoint?.GetComponentInParent<IVehicleRiderSource>();
            _vehicleRider.OnEnterVehicle(riderSource?.GetRiderData());
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

            // Fall damage — track peak fall speed; apply on landing
            bool grounded = _controller.isGrounded;
            if (!grounded)
            {
                _peakFallSpeed = Mathf.Max(_peakFallSpeed, -_ctx.VerticalVelocity);
            }
            else
            {
                if (!_wasGrounded && _peakFallSpeed > _config.SafeFallSpeed)
                {
                    float raw    = (_peakFallSpeed - _config.SafeFallSpeed) * _config.DamagePerMps;
                    float damage = Mathf.Min(raw, _health - _config.MinSurviveHealth);
                    if (damage > 0f)
                    {
                        TakeDamage(damage, DamageType.Fall);
                        Debug.Log($"[FallDamage] speed={_peakFallSpeed:F1} m/s  dmg={damage:F1}  hp={_health:F1}/{_maxHealth}");
                    }
                }
                _peakFallSpeed = 0f;
            }
            _wasGrounded = grounded;

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

            // Slope-stick: when grounded and moving, push down hard enough to follow any walkable slope.
            // -2f alone is insufficient on steep slopes at high speed (45° @ 5 m/s needs -5 m/s down).
            var slopeStickVertical = _controller.isGrounded && _ctx.MoveSpeed > 0f && _ctx.VerticalVelocity < 0f
                ? Mathf.Min(_ctx.VerticalVelocity, -_ctx.MoveSpeed)
                : _ctx.VerticalVelocity;

            var motion = worldMove * _ctx.MoveSpeed + Vector3.up * slopeStickVertical + _launchVelocity;
            if (_controller.enabled)
                _controller.Move(motion * Time.deltaTime);

            // TP only: rotate body to face camera forward so 8-dir blend tree axes align with input.
            // FP: body already tracks look direction via ConsumeFPBodyYawDelta above.
            if (_cameraProvider.CurrentMode == CharacterCameraProvider.CameraMode.ThirdPerson
                && worldMove.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(camForward),
                    15f * Time.deltaTime);
            }

            // Weapon tick — WeaponHolder handles its own logic when present
            _weaponHolder?.Tick(_inputAdapter.WeaponCommand);
        }

        private void Die()
        {
            _isDead = true;
            _active = false;

            // Capture velocity before disabling everything
            var deathVelocity = _launchVelocity
                              + Vector3.up * _ctx.VerticalVelocity;

            _controller.enabled = false;

            // Animator must be disabled so it stops driving bones —
            // otherwise it fights the ragdoll physics every frame.
            var animator = GetComponentInChildren<Animator>(true);
            if (animator != null) animator.enabled = false;

            // Hand back to physics
            SetRagdollActive(true);

            // Give all bones the character's current velocity so ragdoll
            // inherits momentum instead of dropping straight down.
            foreach (var rb in _ragdollBodies)
                rb.linearVelocity = deathVelocity;

            Debug.Log("[Character] Dead");
        }

        private void SetRagdollActive(bool active)
        {
            foreach (var rb in _ragdollBodies)
                rb.isKinematic = !active;
            foreach (var col in _ragdollColliders)
                col.enabled = active;
        }
    }
}

