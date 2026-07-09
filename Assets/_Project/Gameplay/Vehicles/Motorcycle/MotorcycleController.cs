using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    [RequireComponent(typeof(MotorcycleInputAdapter))]
    [RequireComponent(typeof(MotorcycleCameraProvider))]
    [RequireComponent(typeof(MotorcycleHUDProvider))]
    public class MotorcycleController : VehicleControllerBase, IMotorcycleStats, IVehicleRiderSource, IVehicleRiderState
    {
        [SerializeField] private MotorcycleConfig _config = new MotorcycleConfig();

        [Header("Wheels")]
        [SerializeField] private WheelCollider _frontWheelCollider;
        [SerializeField] private WheelCollider _rearWheelCollider;
        [SerializeField] private Transform     _frontWheelMesh;
        [SerializeField] private Transform     _rearWheelMesh;

        [Header("Visuals")]
        [SerializeField] private Transform _handlerBar;   // optional — rotates with steer

        [Header("Drivetrain")]
        [Tooltip("Crankset axle — rotates around local X proportional to speed.")]
        [SerializeField] private Transform _crankset;
        [Tooltip("Left pedal — parented to crankset, counter-rotates to stay level.")]
        [SerializeField] private Transform _leftPedal;
        [Tooltip("Right pedal — parented to crankset, counter-rotates to stay level.")]
        [SerializeField] private Transform _rightPedal;

        [Header("Rider IK Anchors")]
        [Tooltip("Left foot peg — IK pins character's left foot here while riding.")]
        [SerializeField] private Transform _leftFootPeg;
        [Tooltip("Right foot peg — IK pins character's right foot here while riding.")]
        [SerializeField] private Transform _rightFootPeg;
        [Tooltip("Left ground stand — where character's left foot touches the ground when stopped.")]
        [SerializeField] private Transform _leftStandTarget;
        [Tooltip("Right ground stand — where character's right foot touches the ground when stopped.")]
        [SerializeField] private Transform _rightStandTarget;
        [Tooltip("Left handlebar grip — optional IK anchor for character's left hand.")]
        [SerializeField] private Transform _leftHandGrip;
        [Tooltip("Right handlebar grip — optional IK anchor for character's right hand.")]
        [SerializeField] private Transform _rightHandGrip;
        [Tooltip("Left knee hint — empty placed in front of the left knee to guide bend direction.")]
        [SerializeField] private Transform _leftKneeHint;
        [Tooltip("Right knee hint — empty placed in front of the right knee to guide bend direction.")]
        [SerializeField] private Transform _rightKneeHint;
        [Tooltip("Left elbow hint — empty placed behind/outside the left elbow.")]
        [SerializeField] private Transform _leftElbowHint;
        [Tooltip("Right elbow hint — empty placed behind/outside the right elbow.")]
        [SerializeField] private Transform _rightElbowHint;
        [Tooltip("Point the spine leans toward while riding. Leave empty to auto-use midpoint of hand grips.")]
        [SerializeField] private Transform _spineLookTarget;
        [Tooltip("Hip anchor — place at pelvis height on the seat. Forces body position so arms can reach the handlebar.")]
        [SerializeField] private Transform _seatAnchor;

        private MotorcycleInputAdapter   _inputAdapter;
        private MotorcycleCameraProvider _cameraProvider;
        private MotorcycleHUDProvider    _hudProvider;
        private float                    _smoothedTargetLean;
        private Quaternion               _handlerBarInitRot;

        // IMotorcycleStats
        private float _speedKmh;
        private float _rpm = 800f;
        public  float SpeedKmh => _speedKmh;
        public  float RPM      => _rpm;

        // IVehicleRiderState
        public bool  IsMoving    => Mathf.Abs(_speedKmh) > 1f;
        public bool  TiltToRight => WrapAngle(transform.eulerAngles.z) <= 0f;
        public float SpeedNorm   => Mathf.Clamp01(Mathf.Abs(_speedKmh) / _config.TopSpeedKmh);

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

            _handlerBarInitRot = _handlerBar != null ? _handlerBar.localRotation : Quaternion.identity;
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            bool wasKinematic = _rb.isKinematic;
            _rb.isKinematic = false;
            if (wasKinematic)
            {
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        public override void OnUnpossess(PossessionContext context)
        {
            // Reset wheel forces so no phantom torque on next possession.
            if (_rearWheelCollider)  { _rearWheelCollider.motorTorque = 0f;  _rearWheelCollider.brakeTorque = 0f; }
            if (_frontWheelCollider) { _frontWheelCollider.steerAngle = 0f;  _frontWheelCollider.brakeTorque = 0f; }
            base.OnUnpossess(context);
            // Leave physics alive — motorcycle coasts to a stop naturally via rolling resistance.
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
                        _handlerBarInitRot * Quaternion.Euler(0f, _frontWheelCollider.steerAngle, 0f);
            }

            // ── Aerodynamics ─────────────────────────────────────────────────
            _rb.linearDamping  = speed * _config.AirResistance + _config.MinDamping;
            _rb.angularDamping = _config.AngularStability + speed * 0.05f;

            // ── Lean (PD torque controller) ───────────────────────────────────
            ApplyLean(cmd, speed, steerFactor);

            // ── Wheel mesh sync ───────────────────────────────────────────────
            SyncWheelMesh(_frontWheelCollider, _frontWheelMesh);
            SyncWheelMesh(_rearWheelCollider,  _rearWheelMesh);

            // ── Crankset + pedals ─────────────────────────────────────────────
            UpdateCrankset(speedFwd);

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

        // IVehicleRiderSource
        public VehicleRiderData GetRiderData() => new VehicleRiderData
        {
            HideCharacter    = _config.HideCharacter,
            LeftFootTarget   = _leftFootPeg,
            RightFootTarget  = _rightFootPeg,
            LeftStandTarget  = _leftStandTarget,
            RightStandTarget = _rightStandTarget,
            LeftHandTarget   = _leftHandGrip,
            RightHandTarget  = _rightHandGrip,
            LeftKneeHint     = _leftKneeHint,
            RightKneeHint    = _rightKneeHint,
            LeftElbowHint    = _leftElbowHint,
            RightElbowHint   = _rightElbowHint,
            SpineLookTarget  = _spineLookTarget,
            SeatAnchor       = _seatAnchor,
            StateSource      = this,
        };

        private void UpdateCrankset(float speedFwd)
        {
            if (_crankset == null) return;

            // delta in degrees this fixed frame; sign follows drive direction
            float delta = speedFwd * 3.6f * _config.CranksetDegreesPerKmh * Time.fixedDeltaTime;
            var spin = Quaternion.Euler(delta, 0f, 0f);

            _crankset.localRotation *= spin;

            // Pedals are children of the crankset; counter-rotate so they stay level.
            var counterSpin = Quaternion.Euler(-delta, 0f, 0f);
            if (_leftPedal  != null) _leftPedal.localRotation  *= counterSpin;
            if (_rightPedal != null) _rightPedal.localRotation *= counterSpin;
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
