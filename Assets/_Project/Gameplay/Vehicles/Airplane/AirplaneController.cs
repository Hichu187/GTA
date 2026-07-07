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
    public class AirplaneController : VehicleControllerBase, IAirplaneStats
    {
        [SerializeField] private AirplaneConfig _config = new AirplaneConfig();

        [Header("Landing Gear (optional)")]
        [Tooltip("WheelColliders on the landing gear — used for taxi/takeoff ground physics.")]
        [SerializeField] private WheelCollider[] _landingGearWheels;
        [SerializeField] private Transform[]     _landingGearMeshes;

        [Header("Propeller (optional visual)")]
        [SerializeField] private Transform _propeller;
        [SerializeField] private float     _propellerSpinSpeed = 3000f;  // deg/s at full throttle

        private AirplaneInputAdapter  _inputAdapter;
        private AirplaneCameraProvider _cameraProvider;
        private AirplaneHUDProvider   _hudProvider;

        // Throttle smoothing
        private float _currentThrottle;

        // IAirplaneStats
        private float _speedKmh;
        private float _altitudeM;
        private float _headingDeg;
        private float _throttlePct;

        public float SpeedKmh    => _speedKmh;
        public float AltitudeM   => _altitudeM;
        public float HeadingDeg  => _headingDeg;
        public float ThrottlePct => _throttlePct;

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
        }

        public override void OnUnpossess(PossessionContext context)
        {
            _currentThrottle = 0f;
            base.OnUnpossess(context);
            _rb.isKinematic = true;
        }

        protected override void OnOccupiedUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
        }

        protected override void OnOccupiedFixedUpdate()
        {
            var   cmd   = _inputAdapter.Command;
            float dt    = Time.fixedDeltaTime;
            float speed = _rb.linearVelocity.magnitude;

            // Control surface effectiveness scales with airspeed
            float effectiveness = _config.ControlEffectiveness.Evaluate(speed);

            // ── Throttle (smoothed) ───────────────────────────────────────────
            _currentThrottle = Mathf.Lerp(_currentThrottle, cmd.Throttle, _config.ThrottleSmooth);

            // ── Thrust ────────────────────────────────────────────────────────
            float speedRatio = Mathf.Clamp01(speed / _config.TopSpeed);
            float thrust     = _currentThrottle * _config.MaxThrust * (1f - speedRatio * speedRatio);
            _rb.AddForce(transform.forward * thrust, ForceMode.Force);

            // ── Lift (wings — requires minimum airspeed) ──────────────────────
            float speedFwd  = Vector3.Dot(_rb.linearVelocity, transform.forward);
            if (speedFwd > _config.StallSpeed)
            {
                // Lift = v² × coefficient, acts in local up direction
                float liftForce = speedFwd * speedFwd * _config.LiftCoefficient;
                _rb.AddForce(transform.up * liftForce, ForceMode.Force);
            }

            // ── Air drag (speed-proportional linearDamping) ───────────────────
            float drag = speed * _config.AirResistance;
            if (cmd.Brake && IsOnGround())
                drag += _config.BrakeDrag;
            _rb.linearDamping = drag;

            // ── Control surfaces (pitch / roll / yaw) ─────────────────────────
            // Pitch: positive input = nose down (pull back = nose up = negative pitch)
            _rb.AddRelativeTorque(
                new Vector3(-cmd.Pitch * _config.PitchTorque * effectiveness,
                             cmd.Yaw   * _config.YawTorque   * effectiveness,
                            -cmd.Roll  * _config.RollTorque  * effectiveness),
                ForceMode.Acceleration);

            // Angular damping: resists unwanted drift and stabilises flight
            _rb.angularDamping = _config.AngularDamping;

            // ── Landing gear sync ─────────────────────────────────────────────
            if (_landingGearWheels != null)
            {
                for (int i = 0; i < _landingGearWheels.Length; i++)
                {
                    var w = _landingGearWheels[i];
                    if (w == null) continue;
                    w.brakeTorque = cmd.Brake && IsOnGround() ? 5000f : 0f;
                    if (i < _landingGearMeshes.Length && _landingGearMeshes[i] != null)
                    {
                        w.GetWorldPose(out Vector3 pos, out Quaternion rot);
                        _landingGearMeshes[i].SetPositionAndRotation(pos, rot);
                    }
                }
            }

            // ── Propeller visual ──────────────────────────────────────────────
            if (_propeller != null)
                _propeller.Rotate(Vector3.forward,
                    _currentThrottle * _propellerSpinSpeed * dt, Space.Self);

            // ── Stats ─────────────────────────────────────────────────────────
            _speedKmh    = speedFwd * 3.6f;
            _altitudeM   = transform.position.y;
            _headingDeg  = (transform.eulerAngles.y + 360f) % 360f;
            _throttlePct = _currentThrottle * 100f;

            // ── Exit ──────────────────────────────────────────────────────────
            if (_inputAdapter.ConsumeExitPressed())
                _onExitRequested?.Invoke();
        }

        // True when any landing gear wheel is touching the ground.
        private bool IsOnGround()
        {
            if (_landingGearWheels == null || _landingGearWheels.Length == 0)
                return false;
            foreach (var w in _landingGearWheels)
                if (w != null && w.isGrounded) return true;
            return false;
        }
    }
}
