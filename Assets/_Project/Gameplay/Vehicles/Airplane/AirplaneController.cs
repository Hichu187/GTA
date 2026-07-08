using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Airplane
{
    [RequireComponent(typeof(AirplaneInputAdapter))]
    [RequireComponent(typeof(AirplaneCameraProvider))]
    [RequireComponent(typeof(AirplaneHUDProvider))]
    public class AirplaneController : FlyingVehicleBase, IAirplaneStats
    {
        [SerializeField] private AirplaneConfig _config = new AirplaneConfig();

        [Header("Ground Detection")]
        [Tooltip("Layers counted as ground. Exclude the vehicle's own layer to prevent self-hit.")]
        [SerializeField] private LayerMask _groundMask = -1;

        [Header("Roll Visual")]
        [Tooltip("Tick nếu chiều nghiêng cánh bị ngược so với mong muốn.")]
        [SerializeField] private bool _invertRoll;

        [Header("Landing Gear")]
        [SerializeField] private WheelCollider[] _landingGearWheels;
        [SerializeField] private Transform[]     _landingGearMeshes;

        [Header("Propeller (optional visual)")]
        [SerializeField] private Transform _propeller;
        [SerializeField] private float     _propellerSpinSpeed = 3000f;

        private AirplaneInputAdapter   _inputAdapter;
        private AirplaneCameraProvider _cameraProvider;
        private AirplaneHUDProvider    _hudProvider;

        // Ground state
        private float _groundSpeed;

        // Air state
        private float _targetRoll;
        private float _velocityY;       // explicit vertical velocity — gravity + lift + pitch
        private float _landingCooldown;

        // ── IAirplaneStats ───────────────────────────────────────────────────────
        public float SpeedKmh    => _currentSpeed * 3.6f;
        public float AltitudeM   => transform.position.y;
        public float HeadingDeg  => (transform.eulerAngles.y + 360f) % 360f;
        public float ThrottlePct => _inputAdapter != null ? _inputAdapter.Command.Throttle * 100f : 0f;

        // ── IPossessable ─────────────────────────────────────────────────────────
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        // ────────────────────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<AirplaneInputAdapter>();
            _cameraProvider = GetComponent<AirplaneCameraProvider>();
            _hudProvider    = GetComponent<AirplaneHUDProvider>();
            _hudProvider.StatsSource = this;
            _rb.isKinematic = true;
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            bool wasKinematic = _rb.isKinematic;
            _rb.isKinematic  = false;
            _rb.useGravity   = true;
            _groundSpeed     = 0f;
            _targetRoll      = 0f;
            _velocityY       = 0f;
            _landingCooldown = 0f;

            // Re-possess mid-air (e.g. player exits and re-enters a flying plane)
            if (!wasKinematic && !IsNearGround())
            {
                _currentSpeed = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z).magnitude;
                _velocityY    = _rb.linearVelocity.y;
                BeginFlight();
            }
        }

        public override void OnUnpossess(PossessionContext context)
        {
            ResetLandingGear();
            base.OnUnpossess(context);
            // EndFlight (called by base when _inAir) restores gravity so plane falls.
        }

        // ── Ground ──────────────────────────────────────────────────────────────

        protected override void OnGroundUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnGroundFixedUpdate()
        {
            var   cmd = _inputAdapter.Command;
            float dt  = Time.fixedDeltaTime;

            if (cmd.Throttle > 0.01f)
                _groundSpeed = Mathf.MoveTowards(_groundSpeed, _config.GroundTopSpeedKmh / 3.6f,
                    _config.GroundAcceleration * dt);
            else if (cmd.Brake)
                _groundSpeed = Mathf.MoveTowards(_groundSpeed, 0f,
                    _config.GroundAcceleration * _config.BrakeFriction * dt);
            else
                _groundSpeed = Mathf.MoveTowards(_groundSpeed, 0f,
                    _config.GroundAcceleration * 0.4f * dt);

            // Rudder steering on ground
            if (Mathf.Abs(cmd.Yaw) > 0.01f)
                transform.Rotate(Vector3.up, cmd.Yaw * 40f * dt, Space.World);

            // Velocity override — keep physics Y so WheelColliders handle ground contact
            _rb.linearVelocity = new Vector3(
                transform.forward.x * _groundSpeed,
                _rb.linearVelocity.y,
                transform.forward.z * _groundSpeed);

            SyncLandingGear();
            SpinPropeller(cmd.Throttle);

            // Auto-takeoff when runway speed reached
            if (_groundSpeed >= _config.TakeoffSpeedKmh / 3.6f)
            {
                _currentSpeed    = _groundSpeed;
                _velocityY       = 0f;
                _landingCooldown = _config.TakeoffCooldown;
                BeginFlight();
            }
        }

        // ── Air ─────────────────────────────────────────────────────────────────

        protected override void OnAirUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnAirFixedUpdate()
        {
            var   cmd = _inputAdapter.Command;
            float dt  = Time.fixedDeltaTime;

            if (_landingCooldown > 0f) _landingCooldown -= dt;

            // ── 1. Forward speed ─────────────────────────────────────────────────
            //   Throttle  → accelerate toward target
            //   Brake     → decelerate fast
            //   No input  → coast (hold current speed)
            if (cmd.Throttle > 0.01f)
            {
                float target = Mathf.Lerp(
                    _config.NormalFlySpeedKmh,
                    _config.MaxFlySpeedKmh,
                    cmd.Throttle) / 3.6f;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, target, _config.FlyAcceleration * dt);
            }
            else if (cmd.Brake)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f,
                    _config.FlyDeceleration * _config.BrakeFriction * dt);
            }

            // ── 2. Roll accumulation ─────────────────────────────────────────────
            //   Left input  → roll left (negative)
            //   Right input → roll right (positive)
            //   No input    → auto-level
            _targetRoll += cmd.Roll * _config.RollSpeed * dt;
            _targetRoll  = Mathf.Clamp(_targetRoll, -_config.MaxRollAngle, _config.MaxRollAngle);
            if (Mathf.Abs(cmd.Roll) < 0.01f)
                _targetRoll = Mathf.MoveTowards(_targetRoll, 0f, _config.RollSpeed * 0.5f * dt);

            // ── 3. Yaw from banking + manual rudder ──────────────────────────────
            float rollFrac = _targetRoll / _config.MaxRollAngle;
            float yawDelta = rollFrac   * _config.TurningSpeed * dt
                           + cmd.Yaw   * _config.YawSpeed      * dt;
            transform.Rotate(Vector3.up, yawDelta, Space.World);

            // ── 4. Vertical velocity — gravity, lift, stall, pitch ───────────────
            //
            //   liftRatio: how much lift the current speed generates
            //     0   = no speed  → full gravity, free fall
            //     0.5 = half stall → half gravity
            //     1+  = at or above TakeoffSpeed → full lift, level flight
            //
            //   Pitch button:
            //     UP(cmd.Pitch=-1) → _velocityY = +ClimbSpeed (climb)
            //     DN(cmd.Pitch=+1) → _velocityY = -ClimbSpeed (descend)
            //     No pitch         → gravity simulation accumulates

            float stallSpeed = _config.TakeoffSpeedKmh / 3.6f;
            float liftRatio  = Mathf.Clamp01(_currentSpeed / stallSpeed);

            if (Mathf.Abs(cmd.Pitch) > 0.01f)
            {
                // Pitch button: direct vertical control
                _velocityY = -cmd.Pitch * _config.ClimbSpeed;
            }
            else if (cmd.Brake)
            {
                // Braking: speed loss = lift loss = altitude loss
                //   brakeSink scales with current speed (fast = more lift being bled off)
                //   At cruise (60 m/s): sink up to ClimbSpeed * 0.5 = 7.5 m/s downward
                //   At stall (<25 m/s): brakeSink → 0, stall gravity dominates
                _velocityY += Physics.gravity.y * (1f - liftRatio) * dt;  // stall gravity still active

                float brakeSink = Mathf.Clamp01(_currentSpeed / (_config.NormalFlySpeedKmh / 3.6f))
                                * _config.ClimbSpeed * 0.5f;
                if (_velocityY > -brakeSink)
                    _velocityY = Mathf.MoveTowards(_velocityY, -brakeSink, _config.ClimbSpeed * dt);

                _velocityY = Mathf.Clamp(_velocityY, -_config.StallFallSpeed, _config.ClimbSpeed);
            }
            else
            {
                // No pitch, no brake: gravity vs lift
                // At liftRatio=1 (≥TakeoffSpeed): netGrav=0 → level flight
                // At liftRatio=0 (speed=0):       netGrav=-9.81 → free fall
                float netGrav = Physics.gravity.y * (1f - liftRatio);
                _velocityY += netGrav * dt;

                if (liftRatio >= 1f)
                    _velocityY = Mathf.MoveTowards(_velocityY, 0f, 20f * dt);

                _velocityY = Mathf.Clamp(_velocityY, -_config.StallFallSpeed, _config.ClimbSpeed);
            }

            // ── 5. Apply velocity override ───────────────────────────────────────
            Vector3 hDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            _rb.linearVelocity = hDir * _currentSpeed + Vector3.up * _velocityY;

            // ── 6. Visual tilt + propeller ───────────────────────────────────────
            ApplyPitchVisual(cmd.Pitch * _config.MaxPitchAngle, _config.PitchSmooth);
            ApplyRollVisual(_invertRoll ? -_targetRoll : _targetRoll, _config.RollSmooth);
            SpinPropeller(cmd.Throttle);

            // ── 7. Landing check ─────────────────────────────────────────────────
            //   Gated by cooldown to prevent re-land immediately after takeoff.
            //   Triggers when:
            //     a) Descending fast enough (natural stall touchdown), OR
            //     b) Player holds Brake near ground (intentional land)
            if (_landingCooldown <= 0f && IsNearGround())
            {
                if (_velocityY < -_config.LandingDescendSpeed || cmd.Brake)
                {
                    _groundSpeed = _currentSpeed;   // carry forward speed into ground roll
                    _velocityY   = 0f;
                    EndFlight();
                }
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        // Ray origin raised 1 m above pivot to avoid self-collision with own colliders.
        private bool IsNearGround() =>
            Physics.Raycast(
                transform.position + Vector3.up,
                Vector3.down,
                _config.LandingHeight + 1f,
                _groundMask);

        private void SpinPropeller(float throttle)
        {
            if (_propeller == null || throttle < 0.01f) return;
            _propeller.Rotate(Vector3.forward, throttle * _propellerSpinSpeed * Time.fixedDeltaTime, Space.Self);
        }

        private void SyncLandingGear()
        {
            if (_landingGearWheels == null) return;
            for (int i = 0; i < _landingGearWheels.Length; i++)
            {
                if (_landingGearWheels[i] == null) continue;
                if (_landingGearMeshes != null && i < _landingGearMeshes.Length && _landingGearMeshes[i] != null)
                {
                    _landingGearWheels[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
                    _landingGearMeshes[i].SetPositionAndRotation(pos, rot);
                }
            }
        }

        private void ResetLandingGear()
        {
            if (_landingGearWheels == null) return;
            foreach (var w in _landingGearWheels)
                if (w != null) { w.motorTorque = 0f; w.brakeTorque = 0f; }
        }
    }
}
