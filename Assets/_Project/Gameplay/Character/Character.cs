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
using Game.Gameplay.Character.Water;
using Game.Gameplay.Character.Ladder;
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

        private CharacterVehicleRider   _vehicleRider;
        private CharacterWaterDetector  _waterDetector;
        private CharacterLadderDetector _ladderDetector;
        private AbilitySystem         _abilitySystem;
        private CrouchAbility         _crouchAbility;
        private InteractAbility       _interactAbility;
        private LocomotionLockAbility _lockAbility;

        private float       _health;
        private float       _stamina;
        private float       _oxygen;
        private bool        _active;
        private Vector3     _launchVelocity;
        private float       _peakFallSpeed;
        private bool        _wasGrounded;
        private bool        _isDead;
        private bool        _isArmed;
        private bool        _isAiming;
        private float       _swimVerticalInput;
        private bool        _isDrowned;
        private float       _climbVerticalInput;
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
        public bool              IsArmed           => _isArmed;
        public bool              IsAiming          => _isAiming;
        public int               WeaponType        => (_weaponHolder?.CurrentWeapon as IWeaponStats)?.WeaponTypeId ?? 0;
        public float             SwimVerticalInput => _swimVerticalInput;
        public bool              IsDrowned         => _isDrowned;
        public bool              IsClimbing         => _fsm?.CurrentId == LocomotionStateId.Climb;
        public float             ClimbVerticalInput => _climbVerticalInput;

        // ICharacterStats
        public float Health     => _health;
        public float MaxHealth  => _maxHealth;
        public float Stamina    => _stamina;
        public float MaxStamina => _maxStamina;
        public float Oxygen     => _oxygen;
        public float MaxOxygen  => _config.MaxOxygen;

        // IDamageable
        public float CurrentHealth => _health;
        public void TakeDamage(float amount, DamageType type)
        {
            if (_isDead || amount <= 0f) return;
            _health = Mathf.Clamp(_health - amount, 0f, _maxHealth);
            if (_health <= 0f) Die(type);
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
            _oxygen  = _config.MaxOxygen;

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

            _vehicleRider    = GetComponent<CharacterVehicleRider>() ?? gameObject.AddComponent<CharacterVehicleRider>();
            _waterDetector   = GetComponent<CharacterWaterDetector>() ?? gameObject.AddComponent<CharacterWaterDetector>();
            _ladderDetector  = GetComponent<CharacterLadderDetector>() ?? gameObject.AddComponent<CharacterLadderDetector>();
            if (GetComponent<CharacterWeaponIK>() == null) gameObject.AddComponent<CharacterWeaponIK>();
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

            // Read weapon input once — WeaponCommand consumes one-shot flags on read
            var weaponCmd = _inputAdapter.WeaponCommand;
            _isArmed  = _weaponHolder?.CurrentWeapon != null;
            _isAiming = weaponCmd.AimHeld; // DEBUG: bỏ _isArmed để test camera

            if (weaponCmd.AimHeld)
                Debug.Log($"[AimTest] AimHeld=true | isAiming={_isAiming} | mode={_cameraProvider.CurrentMode}");

            if (_inputAdapter.ConsumeToggleCamera())
                _cameraProvider.Toggle();

            // Aim camera: only in TP mode (FP has no shoulder offset)
            _cameraProvider.SetAimMode(
                _isAiming && _cameraProvider.CurrentMode == CharacterCameraProvider.CameraMode.ThirdPerson);

            _cameraProvider.HandleLook(_ctx.Command.LookAxis, _ctx.Command.MoveAxis);

            // FP: horizontal look is body rotation; TP: 0 (no-op).
            transform.Rotate(0f, _cameraProvider.ConsumeFPBodyYawDelta(), 0f);

            if (_inputAdapter.ConsumeCrouch())
            {
                if (_crouchAbility.IsActive) _crouchAbility.Cancel();
                else                         _crouchAbility.Activate();
            }
            _ctx.CrouchRequested = _crouchAbility.IsActive;

            _ctx.IsInWater       = _waterDetector.IsInWater;
            _ctx.SubmersionDepth = _waterDetector.SubmersionDepth;
            _ctx.WaterSurfaceY   = _waterDetector.SurfaceY;

            if (_ctx.LadderReentryCooldown > 0f)
            {
                _ctx.LadderReentryCooldown -= Time.deltaTime;
                _ctx.IsOnLadder = false; // suppressed — just dismounted, ignore overlap for a moment
            }
            else
            {
                _ctx.IsOnLadder = _ladderDetector.IsOnLadder;
            }
            _ctx.LadderMountPosition = _ladderDetector.MountPosition;
            _ctx.LadderFacing        = _ladderDetector.ClimbFacing;
            _ctx.LadderTopY          = _ladderDetector.TopY;
            _ctx.LadderBottomY       = _ladderDetector.BottomY;

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

            // Fold in one-shot outward launches requested by FSM states this tick (e.g.
            // ClimbState's Jump-bail) into the same decaying exit-launch impulse used for
            // vehicle ejection, so it carries the character outward instead of just hovering.
            if (_ctx.PendingLaunchVelocity.sqrMagnitude > 0.0001f)
            {
                _launchVelocity += _ctx.PendingLaunchVelocity;
                _ctx.PendingLaunchVelocity = Vector3.zero;
            }

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

            // Oxygen / drowning — drains while diving, regenerates otherwise (swimming or on land).
            if (_fsm.CurrentId == LocomotionStateId.Dive)
            {
                _oxygen = Mathf.Max(0f, _oxygen - _config.OxygenDrainRate * Time.deltaTime);
                if (_oxygen <= 0f)
                    TakeDamage(_config.DrownDamagePerSecond * Time.deltaTime, DamageType.Drown);
            }
            else
            {
                _oxygen = Mathf.Min(_config.MaxOxygen, _oxygen + _config.OxygenRegenRate * Time.deltaTime);
            }

            // Camera-relative movement: flattened to the XZ plane on land so pitch doesn't
            // affect walking; full 3D while Swim/Dive so swim direction follows exactly
            // where the camera looks (pitch up/down swims up/down — no button needed to
            // dive, see SwimState's auto-transition based on held input + depth).
            bool isWaterborne = _fsm.CurrentId == LocomotionStateId.Swim || _fsm.CurrentId == LocomotionStateId.Dive;
            bool isClimbing   = _fsm.CurrentId == LocomotionStateId.Climb;

            var camForward = _mainCamera == null ? Vector3.forward
                : isWaterborne ? _mainCamera.transform.forward
                : Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;
            var camRight = _mainCamera == null ? Vector3.right
                : isWaterborne ? _mainCamera.transform.right
                : Vector3.ProjectOnPlane(_mainCamera.transform.right, Vector3.up).normalized;

            // Climbing ignores camera-relative input entirely — ClimbState drives pure
            // vertical movement along the ladder rail via VerticalVelocity instead.
            var worldMove = isClimbing ? Vector3.zero
                : camForward * _ctx.Command.MoveAxis.y + camRight * _ctx.Command.MoveAxis.x;

            _swimVerticalInput  = isWaterborne ? Mathf.Clamp(worldMove.y, -1f, 1f) : 0f;
            _climbVerticalInput = isClimbing   ? Mathf.Clamp(_ctx.Command.MoveAxis.y, -1f, 1f) : 0f;

            // Decay exit impulse (15 m/s² deceleration — e.g. 150 km/h tumble stops in ~2.8 s).
            if (_launchVelocity.sqrMagnitude > 0.01f)
                _launchVelocity = Vector3.MoveTowards(_launchVelocity, Vector3.zero, 15f * Time.deltaTime);
            else
                _launchVelocity = Vector3.zero;

            // Slope-stick: when grounded and moving, push down hard enough to follow any walkable slope.
            // -2f alone is insufficient on steep slopes at high speed (45° @ 5 m/s needs -5 m/s down).
            // Swim/Dive/Climb skip this — VerticalVelocity there already means the intended vertical
            // motion directly (buoyancy assist, 0, or ladder climb speed), not gravity to be clamped.
            var slopeStickVertical = isWaterborne || isClimbing ? _ctx.VerticalVelocity
                : _controller.isGrounded && _ctx.MoveSpeed > 0f && _ctx.VerticalVelocity < 0f
                    ? Mathf.Min(_ctx.VerticalVelocity, -_ctx.MoveSpeed)
                    : _ctx.VerticalVelocity;

            // Climbing excludes the exit-launch impulse too — nothing should be able to
            // yank the character sideways off the ladder rail while climbing.
            var motion = isClimbing
                ? Vector3.up * slopeStickVertical
                : worldMove * _ctx.MoveSpeed + Vector3.up * slopeStickVertical + _launchVelocity;
            if (_controller.enabled)
                _controller.Move(motion * Time.deltaTime);

            // TP body rotation:
            // - Climbing → snap to the ladder's fixed facing (ignores camera entirely)
            // - Aiming   → snap to camera forward (strafe movement)
            // - Moving   → rotate toward camera forward (existing behaviour)
            // FP: body already tracks look direction via ConsumeFPBodyYawDelta above.
            if (_cameraProvider.CurrentMode == CharacterCameraProvider.CameraMode.ThirdPerson)
            {
                if (isClimbing)
                {
                    transform.rotation = Quaternion.LookRotation(_ctx.LadderFacing);
                }
                else if (_isAiming)
                {
                    // Read orbital yaw directly — not Camera.main.forward which creates feedback loop
                    // (character rotates → camera shifts → camForward changes → character rotates more)
                    var targetRot = Quaternion.Euler(0f, _cameraProvider.GetAimYaw(), 0f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 20f * Time.deltaTime);
                }
                else if (worldMove.magnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(camForward),
                        15f * Time.deltaTime);
                }
            }

            // Weapon tick — pass the already-read command so one-shot flags aren't double-consumed
            _weaponHolder?.Tick(weaponCmd);
        }

        private void Die(DamageType cause)
        {
            _isDead = true;
            _active = false;
            _controller.enabled = false;

            if (cause == DamageType.Drown)
            {
                // Underwater death — ragdoll physics has no buoyancy and looks wrong
                // submerged, so freeze in the SwimDrowned pose instead (driven by
                // ICharacterAnimationData.IsDrowned in CharacterAnimationDriver).
                _isDrowned = true;
                Debug.Log("[Character] Drowned");
                return;
            }

            // Capture velocity before disabling everything
            var deathVelocity = _launchVelocity
                              + Vector3.up * _ctx.VerticalVelocity;

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

