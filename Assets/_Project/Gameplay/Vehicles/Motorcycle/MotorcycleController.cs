using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    [RequireComponent(typeof(MotorcycleInputAdapter))]
    [RequireComponent(typeof(MotorcycleCameraProvider))]
    [RequireComponent(typeof(MotorcycleHUDProvider))]
    public class MotorcycleController : VehicleControllerBase, IMotorcycleStats
    {
        [SerializeField] private MotorcycleConfig _config = new MotorcycleConfig();

        [Header("Wheels")]
        [SerializeField] private WheelCollider _frontWheelCollider;
        [SerializeField] private WheelCollider _rearWheelCollider;
        [SerializeField] private Transform     _frontWheelMesh;
        [SerializeField] private Transform     _rearWheelMesh;

        [Header("Visuals")]
        [SerializeField] private Transform _handlerBar;   // optional — rotates with steer

        private MotorcycleInputAdapter   _inputAdapter;
        private MotorcycleCameraProvider _cameraProvider;
        private MotorcycleHUDProvider    _hudProvider;
        private float                    _smoothedTargetLean;

        // IMotorcycleStats
        private float _speedKmh;
        private float _rpm = 800f;
        public  float SpeedKmh => _speedKmh;
        public  float RPM      => _rpm;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<MotorcycleInputAdapter>();
            _cameraProvider = GetComponent<MotorcycleCameraProvider>();
            _hudProvider    = GetComponent<MotorcycleHUDProvider>();
            _hudProvider.StatsSource = this;

            SetupCenterOfMass();
            _rb.isKinematic = true;
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            _rb.isKinematic = false;
        }

        public override void OnUnpossess(PossessionContext context)
        {
            // Reset wheel forces before freezing so no phantom torque on next possession.
            if (_rearWheelCollider)  { _rearWheelCollider.motorTorque = 0f;  _rearWheelCollider.brakeTorque = 0f; }
            if (_frontWheelCollider) { _frontWheelCollider.steerAngle = 0f;  _frontWheelCollider.brakeTorque = 0f; }
            base.OnUnpossess(context);
            _rb.isKinematic = true;
        }

        protected override void OnOccupiedUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
        }

        protected override void OnOccupiedFixedUpdate()
        {
            var   cmd      = _inputAdapter.Command;
            float speedFwd = Vector3.Dot(_rb.linearVelocity, transform.forward);
            float speedKmh = speedFwd * 3.6f;
            float speed    = _rb.linearVelocity.magnitude;

            // Steer factor from AnimationCurve: 1.0 at standstill → ~0.1 at 100+ km/h
            float steerFactor = _config.SteerRestrictionCurve.Evaluate(Mathf.Abs(speedKmh));

            // ── Drive / Brake / Reverse ───────────────────────────────────────
            ApplyDrive(cmd, speedFwd, steerFactor);

            // ── Steer (AnimationCurve restricted, smoothed) ───────────────────
            float targetSteer = cmd.Steer * _config.MaxSteerAngle * steerFactor;
            if (_frontWheelCollider)
            {
                _frontWheelCollider.steerAngle = Mathf.Lerp(
                    _frontWheelCollider.steerAngle, targetSteer, _config.SteerSmooth);

                if (_handlerBar != null)
                    _handlerBar.localRotation =
                        Quaternion.Euler(0f, _frontWheelCollider.steerAngle, 0f);
            }

            // ── Aerodynamics ─────────────────────────────────────────────────
            _rb.linearDamping  = speed * _config.AirResistance + _config.MinDamping;
            _rb.angularDamping = _config.AngularStability + speed * 0.05f;

            // ── Lean (PD torque controller) ───────────────────────────────────
            ApplyLean(cmd, speed, steerFactor);

            // ── Wheel mesh sync ───────────────────────────────────────────────
            SyncWheelMesh(_frontWheelCollider, _frontWheelMesh);
            SyncWheelMesh(_rearWheelCollider,  _rearWheelMesh);

            // ── HUD stats ─────────────────────────────────────────────────────
            _speedKmh = speedFwd * 3.6f;
            _rpm      = Mathf.Lerp(800f, 8000f,
                            Mathf.Clamp01(Mathf.Abs(speedFwd) / (_config.TopSpeedKmh / 3.6f)));

            // ── Exit ─────────────────────────────────────────────────────────
            if (_inputAdapter.ConsumeExitPressed())
                _onExitRequested?.Invoke();
        }

        private void ApplyDrive(MotorcycleMoveCommand cmd, float speedFwd, float steerFactor)
        {
            if (_rearWheelCollider == null) return;

            if (cmd.Throttle > 0.01f)
            {
                // Forward — taper motor torque near top speed so it doesn't overshoot
                float speedRatio  = Mathf.Clamp01(speedFwd / (_config.TopSpeedKmh / 3.6f));
                float tapered     = cmd.Throttle * _config.MotorTorque * (1f - speedRatio);
                _rearWheelCollider.motorTorque  = tapered > 5f ? tapered : 0f;
                _rearWheelCollider.brakeTorque  = 0f;
                if (_frontWheelCollider) _frontWheelCollider.brakeTorque = 0f;
            }
            else if (cmd.Brake > 0.01f)
            {
                if (speedFwd > 0.5f)
                {
                    // Moving forward → brake
                    _rearWheelCollider.motorTorque  = 0f;
                    _rearWheelCollider.brakeTorque  = cmd.Brake * _config.BrakeTorque;
                    if (_frontWheelCollider)
                        _frontWheelCollider.brakeTorque = cmd.Brake * _config.BrakeTorque * 0.5f;
                }
                else if (speedFwd > -(_config.ReverseSpeedKmh / 3.6f))
                {
                    // Slow/stopped → reverse (40% of motor torque, negative)
                    _rearWheelCollider.brakeTorque  = 0f;
                    if (_frontWheelCollider) _frontWheelCollider.brakeTorque = 0f;
                    _rearWheelCollider.motorTorque  = -cmd.Brake * _config.MotorTorque * 0.4f;
                }
                else
                {
                    // At max reverse speed → coast
                    _rearWheelCollider.motorTorque  = 0f;
                    _rearWheelCollider.brakeTorque  = 0f;
                }
            }
            else
            {
                // Coast — air resistance (linearDamping) decelerates naturally
                _rearWheelCollider.motorTorque  = 0f;
                _rearWheelCollider.brakeTorque  = 0f;
                if (_frontWheelCollider) _frontWheelCollider.brakeTorque = 0f;
            }
        }

        private void ApplyLean(MotorcycleMoveCommand cmd, float speed, float steerFactor)
        {
            bool isGrounded = (_frontWheelCollider != null && _frontWheelCollider.isGrounded)
                           || (_rearWheelCollider  != null && _rearWheelCollider.isGrounded);

            float desiredLean = isGrounded
                ? -cmd.Steer * _config.MaxLeanAngle * steerFactor
                : 0f;

            // Smooth the target so releasing the steer doesn't cause an instant large error.
            _smoothedTargetLean = Mathf.MoveTowards(
                _smoothedTargetLean, desiredLean,
                _config.LeanReturnSpeed * Time.fixedDeltaTime);

            float targetLean = _smoothedTargetLean;

            float currentLean = WrapAngle(transform.eulerAngles.z);
            float leanError   = targetLean - currentLean;

            // Angular velocity around local forward (Z) axis = roll rate
            float rollRate = Vector3.Dot(_rb.angularVelocity, transform.forward);

            // PD controller: proportional error + derivative damping
            float leanTorque = leanError * _config.LeanTorque
                             - rollRate  * _config.LeanDamping;

            _rb.AddRelativeTorque(new Vector3(0f, 0f, leanTorque), ForceMode.Acceleration);

            // At very low speed, bias toward upright so the bike doesn't tip from physics noise
            if (speed < 1.5f)
            {
                float uprightBias = -currentLean * _config.LeanTorque * 0.4f
                                  - rollRate     * _config.LeanDamping;
                _rb.AddRelativeTorque(new Vector3(0f, 0f, uprightBias), ForceMode.Acceleration);
            }
        }

        private void SetupCenterOfMass()
        {
            if (_frontWheelCollider == null || _rearWheelCollider == null) return;
            Vector3 com = Vector3.zero;
            // Midpoint between wheels on Z, slightly below axle height for stability
            com.z = (_rearWheelCollider.transform.localPosition.z
                   + _frontWheelCollider.transform.localPosition.z) * 0.5f;
            com.y = -0.2f;
            _rb.centerOfMass = com;
        }

        private static void SyncWheelMesh(WheelCollider col, Transform mesh)
        {
            if (col == null || mesh == null) return;
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }

        // Maps runtime Euler angle [0,360) to signed [-180,180).
        private static float WrapAngle(float angle)
        {
            angle %= 360f;
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
