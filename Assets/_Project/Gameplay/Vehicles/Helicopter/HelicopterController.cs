using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Helicopter
{
    [RequireComponent(typeof(HelicopterInputAdapter))]
    [RequireComponent(typeof(HelicopterCameraProvider))]
    [RequireComponent(typeof(HelicopterHUDProvider))]
    public class HelicopterController : FlyingVehicleBase, IHelicopterStats
    {
        [SerializeField] private HelicopterConfig _config = new HelicopterConfig();

        [Header("Rotors (visual)")]
        [SerializeField] private Transform _mainRotor;
        [SerializeField] private Transform _tailRotor;
        [SerializeField] private float     _rotorSpinSpeed = 2000f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask _groundMask = -1;

        private HelicopterInputAdapter   _inputAdapter;
        private HelicopterCameraProvider _cameraProvider;
        private HelicopterHUDProvider    _hudProvider;

        private Vector3 _horizontalVelocity;
        private float   _verticalVelocity;
        private float   _enginePower; // 0-100, ramped by holding EngineUp/EngineDown

        // IHelicopterStats
        public float SpeedKmh        => new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z).magnitude * 3.6f;
        public float AltitudeM       => transform.position.y;
        public float VerticalSpeedMs => _verticalVelocity;

        // Distance to ground below, from a raycast raised above the pivot (Infinity if nothing hit).
        public float DistanceToGround { get; private set; } = float.PositiveInfinity;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<HelicopterInputAdapter>();
            _cameraProvider = GetComponent<HelicopterCameraProvider>();
            _hudProvider    = GetComponent<HelicopterHUDProvider>();
            _hudProvider.StatsSource = this;
            _rb.isKinematic = true;
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            bool wasKinematic = _rb.isKinematic;
            _rb.isKinematic     = false;
            _rb.useGravity      = false;   // helicopter manages its own Y via vertical input
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity   = 0f;

            if (!wasKinematic && !IsNearGround())
            {
                // Re-entering while falling — jump straight into air mode, holding altitude.
                _enginePower = _config.LiftoffThreshold;
                BeginFlight();
            }
            else
            {
                // Grounded/near-ground entry (or freshly parked from Awake) — engine off,
                // gravity on so it settles onto the ground physically if not already resting.
                _enginePower   = 0f;
                _rb.useGravity = true;
            }
        }

        public override void OnUnpossess(PossessionContext context)
        {
            base.OnUnpossess(context);   // EndFlight if inAir → restores gravity so helicopter falls.
            // No isKinematic — helicopter drops under gravity until it hits ground.
        }

        // ── Ground: spooling engine up until it clears LiftoffThreshold ──────

        protected override void OnGroundUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnGroundFixedUpdate()
        {
            UpdateEnginePower(Time.fixedDeltaTime);
            SpinRotors(_enginePower / 100f); // power 0 → rotor animation stops naturally

            if (_enginePower >= _config.LiftoffThreshold)
                BeginFlight();
        }

        private void UpdateEnginePower(float dt)
        {
            var cmd = _inputAdapter.Command;
            if (cmd.EngineUp && !cmd.EngineDown)
                _enginePower = Mathf.Min(100f, _enginePower + _config.EnginePowerRampSpeed * dt);
            else if (cmd.EngineDown && !cmd.EngineUp)
                _enginePower = Mathf.Max(0f,   _enginePower - _config.EnginePowerRampSpeed * dt);
            // Neither held: engine power holds steady (hover at current altitude).
        }

        // ── Air ─────────────────────────────────────────────────────────────

        protected override void OnAirUpdate()
        {
            var cmd = _inputAdapter.Command;
            bool isManualTurning = Mathf.Abs(cmd.Yaw) > 0.01f || Mathf.Abs(cmd.Horizontal.x) > 0.01f;
            _cameraProvider.HandleLook(cmd.Look, isManualTurning);

            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnAirFixedUpdate()
        {
            var   cmd = _inputAdapter.Command;
            float dt  = Time.fixedDeltaTime;

            UpdateEnginePower(dt);
            bool nearGround = IsNearGround();

            // ── Manual Yaw: phím mũi tên (Yaw) + A/D (Horizontal.x, chỉ xoay — không strafe) ──
            float yawInput = Mathf.Clamp(cmd.Yaw + cmd.Horizontal.x, -1f, 1f);
            if (Mathf.Abs(yawInput) > 0.01f)
                transform.Rotate(Vector3.up, yawInput * _config.YawSpeed * dt, Space.World);

            // ── Horizontal movement (camera-relative, chỉ tiến/lùi — W/S) ─────
            Vector3 desiredHorizontal = Vector3.zero;
            if (Mathf.Abs(cmd.Horizontal.y) > 0.01f)
            {
                Vector3 camFwd = Vector3.ProjectOnPlane(GetCameraForward(), Vector3.up).normalized;
                desiredHorizontal = camFwd * (cmd.Horizontal.y * _config.NormalHorizontalSpeedKmh / 3.6f);
            }

            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity, desiredHorizontal,
                (desiredHorizontal.magnitude > 0.01f ? _config.HorizontalAcceleration : _config.HorizontalDeceleration) * dt);

            // ── Auto-yaw: xoay mũi theo hướng bay, chỉ khi đang chủ động giữ tiến.
            // Không auto-yaw khi lùi (chỉ ngóc mũi lên, xem forwardPitch bên dưới)
            // hoặc khi chỉ xoay tại chỗ bằng Yaw/A/D (không có input tiến).
            if (_config.AutoYawSpeed > 0f
                && cmd.Horizontal.y > 0.1f
                && _horizontalVelocity.sqrMagnitude > 0.25f   // > 0.5 m/s
                && Mathf.Abs(yawInput) < 0.05f)
            {
                float targetYaw = Mathf.Atan2(_horizontalVelocity.x, _horizontalVelocity.z) * Mathf.Rad2Deg;
                float yawDiff   = Mathf.DeltaAngle(transform.eulerAngles.y, targetYaw);
                float step      = Mathf.Sign(yawDiff)
                                * Mathf.Min(Mathf.Abs(yawDiff), _config.AutoYawSpeed * dt);
                transform.Rotate(Vector3.up, step, Space.World);
            }

            // ── Vertical movement: only moves while a button is actively held ──
            // Hold EngineUp (with enough power) → climbs, eased out near MaxAltitudeAboveGround.
            // Hold EngineDown → descends. Release either → holds current altitude (hover).
            float desiredVertical;
            if (cmd.EngineUp && _enginePower >= _config.LiftoffThreshold)
            {
                float headroom      = _config.MaxAltitudeAboveGround - DistanceToGround;
                float ceilingFactor = Mathf.Clamp01(headroom / Mathf.Max(_config.CeilingSoftZone, 0.01f));
                desiredVertical = _config.MaxVerticalSpeedKmh / 3.6f * ceilingFactor;
            }
            else if (cmd.EngineDown)
            {
                desiredVertical = -(_config.NormalVerticalSpeedKmh / 3.6f);
            }
            else
            {
                desiredVertical = 0f; // hover — hold current altitude
            }

            _verticalVelocity = Mathf.MoveTowards(
                _verticalVelocity, desiredVertical,
                _config.VerticalAcceleration * dt);

            // ── Velocity override ─────────────────────────────────────────────
            _rb.linearVelocity = new Vector3(
                _horizontalVelocity.x,
                _verticalVelocity,
                _horizontalVelocity.z);

            // ── Visual body tilt (cần _meshRoot / _rollRoot assign trong Inspector) ──
            Vector3 localVel    = transform.InverseTransformDirection(_horizontalVelocity);
            float   normalMs    = _config.NormalHorizontalSpeedKmh / 3.6f;
            float   speedRatio  = _horizontalVelocity.magnitude / Mathf.Max(normalMs, 0.01f);

            // Pitch: cắm đầu xuống khi tiến (localVel.z > 0), ngẩng lên khi lùi,
            // cộng thêm ngóc mũi khi leo cao / cắm mũi khi hạ theo _verticalVelocity.
            float forwardPitch = localVel.z > 0f
                ?  speedRatio * _config.MaxBodyTiltAngle
                : -speedRatio * _config.MaxBodyTiltAngle;
            float climbRatio = _verticalVelocity / Mathf.Max(_config.MaxVerticalSpeedKmh / 3.6f, 0.01f);
            float climbPitch = -climbRatio * _config.ClimbTiltAngle;

            ApplyPitchVisual(forwardPitch + climbPitch, _config.TiltSmooth);

            // Roll: nghiêng theo hướng ngang
            ApplyRollVisual(localVel.x / Mathf.Max(normalMs, 0.01f) * _config.MaxBodyTiltAngle,
                _config.TiltSmooth);

            SpinRotors(_enginePower / 100f);

            // Engine below liftoff threshold and touching ground — settle down.
            if (_enginePower < _config.LiftoffThreshold && nearGround)
                Land();
        }

        private void Land()
        {
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity   = 0f;
            _rb.linearVelocity  = Vector3.zero;
            EndFlight(); // _inAir = false, enables gravity so it settles onto the ground physically
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        // Ray origin raised 1 m above pivot to avoid self-collision with own colliders.
        // Also refreshes DistanceToGround for callers that only need the raw distance.
        private bool IsNearGround()
        {
            bool hasHit = Physics.Raycast(
                transform.position + Vector3.up,
                Vector3.down,
                out RaycastHit hit,
                300f,
                _groundMask);

            DistanceToGround = hasHit ? hit.distance - 1f : float.PositiveInfinity;
            return hasHit && DistanceToGround <= _config.LandingHeight;
        }

        private void SpinRotors(float throttle)
        {
            float deg = throttle * _rotorSpinSpeed * Time.fixedDeltaTime;
            if (_mainRotor != null) _mainRotor.Rotate(Vector3.up,     deg, Space.Self);
            if (_tailRotor  != null) _tailRotor.Rotate(Vector3.right,  deg, Space.Self);
        }

        private Vector3 GetCameraForward()
        {
            if (Camera.main != null)
                return Camera.main.transform.forward;
            return transform.forward;
        }
    }
}
