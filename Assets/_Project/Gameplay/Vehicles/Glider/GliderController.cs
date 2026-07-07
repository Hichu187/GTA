using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Glider
{
    /// <summary>
    /// Physics-inspired glider: no engine, gains speed by diving, loses speed when climbing.
    /// Spawns already in-air. Player steers with pitch / roll; roll drives yaw turn.
    /// Based on GliderFlyingSystem's velocity-override + gravity/air-drag model.
    /// </summary>
    [RequireComponent(typeof(GliderInputAdapter))]
    [RequireComponent(typeof(GliderCameraProvider))]
    [RequireComponent(typeof(GliderHUDProvider))]
    public class GliderController : FlyingVehicleBase, IGliderStats
    {
        [SerializeField] private GliderConfig _config = new GliderConfig();

        private GliderInputAdapter   _inputAdapter;
        private GliderCameraProvider _cameraProvider;
        private GliderHUDProvider    _hudProvider;

        private float _speed;           // forward speed (m/s)
        private float _verticalSpeed;   // Y component (m/s), negative = falling
        private float _targetPitch;
        private float _targetRoll;
        private bool  _diving;
        private float _originalSpeed;   // speed captured at dive start

        // IGliderStats
        public float SpeedKmh        => _speed * 3.6f;
        public float AltitudeM       => transform.position.y;
        public float VerticalSpeedMs => _verticalSpeed;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<GliderInputAdapter>();
            _cameraProvider = GetComponent<GliderCameraProvider>();
            _hudProvider    = GetComponent<GliderHUDProvider>();
            _hudProvider.StatsSource = this;
            _rb.isKinematic = true;
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            _rb.isKinematic = false;
            _speed          = _config.LaunchSpeedKmh / 3.6f;
            _originalSpeed  = _speed;
            _verticalSpeed  = 0f;
            _targetPitch    = 0f;
            _targetRoll     = 0f;
            _diving         = false;

            // Glider starts in-air immediately
            BeginFlight();
        }

        public override void OnUnpossess(PossessionContext context)
        {
            base.OnUnpossess(context);
            _rb.isKinematic = true;
        }

        // ── Air (always in air) ──────────────────────────────────────────────

        protected override void OnAirUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnAirFixedUpdate()
        {
            var   cmd = _inputAdapter.Command;
            float dt  = Time.fixedDeltaTime;

            // ── Pitch / Roll (accumulate) ─────────────────────────────────────
            _targetPitch += cmd.Pitch * _config.PitchSpeed * dt;
            _targetPitch  = Mathf.Clamp(_targetPitch, -_config.MaxPitchAngle, _config.MaxPitchAngle);
            _targetRoll  -= cmd.Roll * _config.RollSpeed * dt;
            _targetRoll   = Mathf.Clamp(_targetRoll, -_config.MaxRollAngle, _config.MaxRollAngle);

            if (Mathf.Abs(cmd.Pitch) < 0.01f)
                _targetPitch = Mathf.MoveTowards(_targetPitch, 0f, _config.PitchSpeed * 0.3f * dt);
            if (Mathf.Abs(cmd.Roll) < 0.01f)
                _targetRoll  = Mathf.MoveTowards(_targetRoll, 0f, _config.RollSpeed * 0.3f * dt);

            // ── Visual tilt ──────────────────────────────────────────────────
            ApplyPitchVisual(_targetPitch, _config.PitchSmooth);
            ApplyRollVisual(_targetRoll,   _config.RollSmooth);

            // ── Speed from gravity/pitch physics ─────────────────────────────
            float pitchRad = _targetPitch * Mathf.Deg2Rad;

            // pitchAngle > 0 → nose down (diving) → accelerate
            if (_targetPitch > _config.DiveStartAngle && _targetPitch < 90f)
            {
                if (!_diving)
                {
                    _diving        = true;
                    _originalSpeed = _speed;
                }
                float gravContrib = _config.Gravity * Mathf.Sin(Mathf.Abs(pitchRad));
                _speed = Mathf.Clamp(_speed + gravContrib * dt, 0f, _config.MaxGlideSpeedKmh / 3.6f);
            }
            else
            {
                _diving = false;

                // Climbing (pitch < 0) or level → bleed speed
                if (_targetPitch < -5f)
                {
                    float bleed = _config.Gravity * Mathf.Sin(Mathf.Abs(pitchRad))
                                + _config.ClimbDeceleration;
                    _speed = Mathf.Max(_speed - bleed * dt, _config.StallSpeedKmh / 3.6f);
                }
                else if (_speed > _originalSpeed)
                {
                    // Recovering from dive — gradually return to original speed
                    _speed = Mathf.MoveTowards(_speed, _originalSpeed, _config.PostDiveDecel * dt);
                }
            }

            // ── Brake / spoilers ─────────────────────────────────────────────
            if (cmd.Brake > 0.01f)
                _speed = Mathf.Max(_speed - _config.BrakeDrag * cmd.Brake * dt, _config.StallSpeedKmh / 3.6f);

            // ── Vertical component (gravity vs air drag) ──────────────────────
            float vertAccel = -_config.Gravity + _config.AirDrag * Mathf.Cos(Mathf.Abs(pitchRad));
            _verticalSpeed += vertAccel * dt;
            float maxGlideMs = _config.MaxGlideSpeedKmh / 3.6f;
            _verticalSpeed   = Mathf.Clamp(_verticalSpeed, -maxGlideMs, maxGlideMs);

            // When going level or up, don't accumulate downward speed from prior dive
            if (_verticalSpeed < 0f && _targetPitch > -5f && _targetPitch < _config.DiveStartAngle)
                _verticalSpeed = Mathf.MoveTowards(_verticalSpeed, 0f, _config.AirDrag * dt);

            // ── Turn from roll bank ───────────────────────────────────────────
            float rollFraction = _targetRoll / Mathf.Max(_config.MaxRollAngle, 0.01f);
            ApplyYawTurn(rollFraction * _config.RollTurnFactor, 0f, _config.TurningSpeed);

            // ── Velocity override ─────────────────────────────────────────────
            Vector3 fwd = FlightForward;
            _rb.linearVelocity = fwd * _speed + new Vector3(0f, _verticalSpeed, 0f);

            // ── Auto-land when touching ground ────────────────────────────────
            if (IsOnGround() && _verticalSpeed <= 0f)
            {
                _speed         = 0f;
                _verticalSpeed = 0f;
                EndFlight();
            }
        }

        private bool IsOnGround() =>
            Physics.Raycast(transform.position, Vector3.down, 1.2f);
    }
}
