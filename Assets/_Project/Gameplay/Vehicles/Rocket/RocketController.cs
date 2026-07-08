using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Rocket
{
    /// <summary>
    /// Rocket: instant launch, no ground phase.
    /// Steered exclusively by Pitch / Roll — goes wherever _meshRoot points.
    /// Throttle controls acceleration; coasts when released.
    /// </summary>
    [RequireComponent(typeof(RocketInputAdapter))]
    [RequireComponent(typeof(RocketCameraProvider))]
    [RequireComponent(typeof(RocketHUDProvider))]
    public class RocketController : FlyingVehicleBase, IRocketStats
    {
        [SerializeField] private RocketConfig _config = new RocketConfig();

        [Header("Exhaust (optional visual)")]
        [SerializeField] private ParticleSystem _exhaustParticles;
        [SerializeField] private Transform      _exhaustTransform;

        private RocketInputAdapter   _inputAdapter;
        private RocketCameraProvider _cameraProvider;
        private RocketHUDProvider    _hudProvider;

        private float _targetPitch;
        private float _targetRoll;
        private float _throttlePct;

        // IRocketStats
        public float SpeedKmh    => _currentSpeed * 3.6f;
        public float AltitudeM   => transform.position.y;
        public float ThrottlePct => _throttlePct;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<RocketInputAdapter>();
            _cameraProvider = GetComponent<RocketCameraProvider>();
            _hudProvider    = GetComponent<RocketHUDProvider>();
            _hudProvider.StatsSource = this;
            _rb.isKinematic = true;
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            _rb.isKinematic = false;
            _currentSpeed   = _config.LaunchSpeedKmh / 3.6f;
            _targetPitch    = 0f;
            _targetRoll     = 0f;

            // Rocket always in-air
            BeginFlight();

            if (_exhaustParticles != null) _exhaustParticles.Play();
        }

        public override void OnUnpossess(PossessionContext context)
        {
            if (_exhaustParticles != null) _exhaustParticles.Stop();
            base.OnUnpossess(context);
            // Physics remains active — EndFlight() restores gravity so rocket falls.
        }

        // ── Air (always) ─────────────────────────────────────────────────────

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
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _config.MaxSpeedKmh / 3.6f,
                    _config.ThrustAcceleration * cmd.Throttle * dt);
            else
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f,
                    _config.CoastDeceleration * dt);

            _throttlePct = cmd.Throttle * 100f;

            // ── Pitch / Roll (accumulate — rocket can do full loops) ──────────
            _targetPitch += cmd.Pitch * _config.PitchSpeed * dt;
            _targetPitch  = Mathf.Clamp(_targetPitch, -_config.MaxPitchAngle, _config.MaxPitchAngle);
            _targetRoll  -= cmd.Roll * _config.RollSpeed * dt;
            // Roll is unbounded (360° roll for rocket)
            _targetRoll  %= 360f;

            // ── Visual ───────────────────────────────────────────────────────
            ApplyPitchVisual(_targetPitch, _config.PitchSmooth);
            ApplyRollVisual(_targetRoll,   _config.RollSmooth);

            // ── Velocity: follow _meshRoot forward (pitch steers climb/dive) ─
            SetFlightVelocity(FlightForward, _currentSpeed);

            // ── Exhaust visual ────────────────────────────────────────────────
            if (_exhaustParticles != null)
            {
                var emission = _exhaustParticles.emission;
                emission.rateOverTime = cmd.Throttle * 200f;
            }
        }
    }
}
