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

        [Header("Landing Gear")]
        [SerializeField] private WheelCollider[] _landingGearWheels;
        [SerializeField] private Transform[]     _landingGearMeshes;

        [Header("Propeller (optional visual)")]
        [SerializeField] private Transform _propeller;
        [SerializeField] private float     _propellerSpinSpeed = 3000f;

        private AirplaneInputAdapter   _inputAdapter;
        private AirplaneCameraProvider _cameraProvider;
        private AirplaneHUDProvider    _hudProvider;

        private float _groundSpeed;
        private float _targetPitch;
        private float _targetRoll;

        // IAirplaneStats
        public float SpeedKmh    => _currentSpeed * 3.6f;
        public float AltitudeM   => transform.position.y;
        public float HeadingDeg  => (transform.eulerAngles.y + 360f) % 360f;
        public float ThrottlePct => _inputAdapter != null ? _inputAdapter.Command.Throttle * 100f : 0f;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

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
            _rb.isKinematic = false;
            _rb.useGravity  = true;
            _groundSpeed    = 0f;
            _targetPitch    = 0f;
            _targetRoll     = 0f;
        }

        public override void OnUnpossess(PossessionContext context)
        {
            ResetLandingGear();
            base.OnUnpossess(context);
            _rb.isKinematic = true;
        }

        // ── Ground ──────────────────────────────────────────────────────────

        protected override void OnGroundUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnGroundFixedUpdate()
        {
            var cmd = _inputAdapter.Command;

            // Throttle → accelerate; Brake → decelerate faster; coast → slow roll
            if (cmd.Throttle > 0.01f)
                _groundSpeed = Mathf.MoveTowards(_groundSpeed, _config.GroundTopSpeedKmh / 3.6f,
                    _config.GroundAcceleration * Time.fixedDeltaTime);
            else if (cmd.Brake)
                _groundSpeed = Mathf.MoveTowards(_groundSpeed, 0f,
                    _config.GroundAcceleration * _config.BrakeFriction * Time.fixedDeltaTime);
            else
                _groundSpeed = Mathf.MoveTowards(_groundSpeed, 0f,
                    _config.GroundAcceleration * 0.4f * Time.fixedDeltaTime);

            // Ground steering
            if (Mathf.Abs(cmd.Yaw) > 0.01f)
                transform.Rotate(Vector3.up, cmd.Yaw * 40f * Time.fixedDeltaTime, Space.World);

            // Drive wheels via velocity (WheelColliders still provide ground contact)
            _rb.linearVelocity = new Vector3(
                transform.forward.x * _groundSpeed,
                _rb.linearVelocity.y,
                transform.forward.z * _groundSpeed);

            SyncLandingGear();
            SpinPropeller(cmd.Throttle);

            // Auto-takeoff
            if (_groundSpeed >= _config.TakeoffSpeedKmh / 3.6f)
            {
                _currentSpeed = _groundSpeed;
                BeginFlight();
            }
        }

        // ── Air ─────────────────────────────────────────────────────────────

        protected override void OnAirUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnAirFixedUpdate()
        {
            var   cmd = _inputAdapter.Command;
            float dt  = Time.fixedDeltaTime;

            // ── Speed ────────────────────────────────────────────────────────
            if (cmd.Throttle > 0.01f)
            {
                float target = Mathf.Lerp(_config.NormalFlySpeedKmh, _config.MaxFlySpeedKmh, cmd.Throttle) / 3.6f;
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, target, _config.FlyAcceleration * dt);
            }
            else
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _config.FlyDeceleration * dt);

            // ── Pitch / Roll (accumulate visual angles) ───────────────────────
            _targetPitch += cmd.Pitch * _config.PitchSpeed * dt;
            _targetPitch  = Mathf.Clamp(_targetPitch, -_config.MaxPitchAngle, _config.MaxPitchAngle);
            _targetRoll  -= cmd.Roll * _config.RollSpeed * dt;
            _targetRoll   = Mathf.Clamp(_targetRoll, -_config.MaxRollAngle, _config.MaxRollAngle);

            // Neutral return when no input
            if (Mathf.Abs(cmd.Pitch) < 0.01f)
                _targetPitch = Mathf.MoveTowards(_targetPitch, 0f, _config.PitchSpeed * 0.4f * dt);
            if (Mathf.Abs(cmd.Roll) < 0.01f)
                _targetRoll  = Mathf.MoveTowards(_targetRoll,  0f, _config.RollSpeed  * 0.4f * dt);

            // ── Visual tilt ──────────────────────────────────────────────────
            ApplyPitchVisual(_targetPitch, _config.PitchSmooth);
            ApplyRollVisual(_targetRoll,   _config.RollSmooth);

            // ── World turn: roll banks into yaw turn; Yaw input adds directly ─
            float rollFraction = _targetRoll / _config.MaxRollAngle;
            float yawAdd       = cmd.Yaw * _config.YawSpeed * dt;
            ApplyYawTurn(rollFraction + yawAdd, 0f, _config.TurningSpeed);

            // ── Velocity override ─────────────────────────────────────────────
            SetFlightVelocity(FlightForward, _currentSpeed);

            SpinPropeller(cmd.Throttle);

            // ── Auto-land ────────────────────────────────────────────────────
            if (cmd.Brake && IsNearGround())
            {
                _groundSpeed = _currentSpeed;
                EndFlight();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool IsNearGround() =>
            Physics.Raycast(transform.position, Vector3.down, _config.LandingHeight + 1f);

        private void SpinPropeller(float throttle)
        {
            if (_propeller == null || throttle < 0.01f) return;
            _propeller.Rotate(Vector3.forward,
                throttle * _propellerSpinSpeed * Time.fixedDeltaTime, Space.Self);
        }

        private void SyncLandingGear()
        {
            if (_landingGearWheels == null) return;
            for (int i = 0; i < _landingGearWheels.Length; i++)
            {
                var w = _landingGearWheels[i];
                if (w == null) continue;
                if (_landingGearMeshes != null && i < _landingGearMeshes.Length && _landingGearMeshes[i] != null)
                {
                    w.GetWorldPose(out Vector3 pos, out Quaternion rot);
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
