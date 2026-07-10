using UnityEngine;
using UnityEngine.Scripting;
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
        [SerializeField] private TwoWheelConfig _config = new TwoWheelConfig();

        [Header("Wheels")]
        [SerializeField] private WheelCollider _frontWheelCollider;
        [SerializeField] private WheelCollider _rearWheelCollider;
        [SerializeField] private Transform     _frontWheelMesh;
        [SerializeField] private Transform     _rearWheelMesh;

        [Header("Visuals")]
        [SerializeField] private Transform _handlerBar;

        [Header("Drivetrain")]
        [SerializeField] private Transform _crankset;
        [SerializeField] private Transform _leftPedal;
        [SerializeField] private Transform _rightPedal;

        [Header("Rider IK Anchors")]
        [SerializeField] private Transform _leftFootPeg;
        [SerializeField] private Transform _rightFootPeg;
        [SerializeField] private Transform _leftStandTarget;
        [SerializeField] private Transform _rightStandTarget;
        [SerializeField] private Transform _leftHandGrip;
        [SerializeField] private Transform _rightHandGrip;
        [SerializeField] private Transform _leftKneeHint;
        [SerializeField] private Transform _rightKneeHint;
        [SerializeField] private Transform _leftElbowHint;
        [SerializeField] private Transform _rightElbowHint;
        [SerializeField] private Transform _spineLookTarget;
        [SerializeField] private Transform _seatAnchor;

        private MotorcycleInputAdapter   _inputAdapter;
        private MotorcycleCameraProvider _cameraProvider;
        private MotorcycleHUDProvider    _hudProvider;

        private float      _currentSteerAngle;
        private float      _currentLeanAngle;
        private Quaternion _handleBarInitRot;

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

        // ── Lifecycle ─────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<MotorcycleInputAdapter>();
            _cameraProvider = GetComponent<MotorcycleCameraProvider>();
            _hudProvider    = GetComponent<MotorcycleHUDProvider>();
            _hudProvider.StatsSource = this;

            _rb.isKinematic   = true;
            _handleBarInitRot = _handlerBar != null ? _handlerBar.localRotation : Quaternion.identity;

            SetupWheels();
            SetupCenterOfMass();
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
            if (_rearWheelCollider)  { _rearWheelCollider.motorTorque = 0f;  _rearWheelCollider.brakeTorque = 0f; }
            if (_frontWheelCollider) { _frontWheelCollider.steerAngle = 0f;  _frontWheelCollider.brakeTorque = 0f; }
            _rb.constraints = RigidbodyConstraints.None;
            base.OnUnpossess(context);
        }

        protected override void OnOccupiedUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
        }

        protected override void OnOccupiedFixedUpdate()
        {
            var cmd = _inputAdapter.Command;

            HandleDrive(cmd.Throttle, cmd.Brake);
            HandleSteering(cmd.Steer);
            LeanOnTurnLocal();
            ConstrainRotation(IsOnGround());

            SyncHandleBar();
            SyncWheelMesh(_frontWheelCollider, _frontWheelMesh);
            SyncWheelMesh(_rearWheelCollider,  _rearWheelMesh);
            UpdateCrankset();

            float speedFwd     = Vector3.Dot(_rb.linearVelocity, transform.forward);
            float speed        = _rb.linearVelocity.magnitude;
            _speedKmh          = speedFwd * 3.6f;
            _rpm               = Mathf.Lerp(800f, 8000f, Mathf.Clamp01(Mathf.Abs(speedFwd) / (_config.TopSpeedKmh / 3.6f)));
            _rb.linearDamping  = speed * _config.AirResistance + _config.MinDamping;
            _rb.angularDamping = _config.AngularStability + speed * 0.05f;

            if (_inputAdapter.ConsumeExitPressed())
                _onExitRequested?.Invoke();
        }

        // ── Drive ─────────────────────────────────────────────────────────────

        private void HandleDrive(float throttle, float brake)
        {
            if (_rearWheelCollider == null) return;
            float speedFwd = Vector3.Dot(_rb.linearVelocity, transform.forward);

            if (throttle > 0.01f)
            {
                float speedRatio = Mathf.Clamp01(speedFwd / (_config.TopSpeedKmh / 3.6f));
                float tapered    = throttle * _config.MotorForce * (1f - speedRatio);
                _rearWheelCollider.motorTorque = tapered > 5f ? tapered : 0f;
                _rearWheelCollider.brakeTorque = 0f;
                if (_frontWheelCollider) _frontWheelCollider.brakeTorque = 0f;
            }
            else if (brake > 0.01f)
            {
                if (speedFwd > 0.5f)
                {
                    _rearWheelCollider.motorTorque = 0f;
                    _rearWheelCollider.brakeTorque = brake * _config.BrakeForce * _config.RearBrakePower;
                    if (_frontWheelCollider)
                        _frontWheelCollider.brakeTorque = brake * _config.BrakeForce * _config.FrontBrakePower;
                }
                else if (speedFwd > -(_config.ReverseSpeedKmh / 3.6f))
                {
                    _rearWheelCollider.brakeTorque = 0f;
                    if (_frontWheelCollider) _frontWheelCollider.brakeTorque = 0f;
                    _rearWheelCollider.motorTorque = -brake * _config.MotorForce * 0.4f;
                }
                else
                {
                    _rearWheelCollider.motorTorque = 0f;
                    _rearWheelCollider.brakeTorque = 0f;
                }
            }
            else
            {
                _rearWheelCollider.motorTorque = 0f;
                _rearWheelCollider.brakeTorque = 0f;
                if (_frontWheelCollider) _frontWheelCollider.brakeTorque = 0f;
            }
        }

        // ── Steering ──────────────────────────────────────────────────────────

        private void HandleSteering(float steerInput)
        {
            float t        = Mathf.Clamp01(_rb.linearVelocity.magnitude / (_config.TopSpeedKmh / 3.6f)) * _config.SteerReductorAmount;
            float maxSteer = Mathf.LerpAngle(_config.MaxSteerAngle, _config.MinSteerAngle, t);
            // × 0.1f matches BicycleSystem convention: TurnSmoothing is 0–1 as a percentage
            _currentSteerAngle = Mathf.Lerp(_currentSteerAngle, maxSteer * steerInput, _config.TurnSmoothing * 0.1f);
            if (_frontWheelCollider) _frontWheelCollider.steerAngle = _currentSteerAngle;
        }

        // ── Lean (direct transform — no physics torque) ───────────────────────
        // Inspired by BicycleVehicle.LeanOnTurnLocal().
        // FreezeRotationZ on ground hands lean control entirely to us.

        private void LeanOnTurnLocal()
        {
            Vector3 localRot = transform.localEulerAngles;
            float   speed    = _rb.linearVelocity.magnitude;
            float   smooth   = _config.LeanSmoothing * 0.1f;

            if (speed < 2f)
            {
                _currentLeanAngle = Mathf.LerpAngle(localRot.z, 0f, 0.05f);
            }
            else if (_currentSteerAngle < 0.5f && _currentSteerAngle > -0.5f)
            {
                _currentLeanAngle = Mathf.LerpAngle(localRot.z, 0f, smooth);
            }
            else
            {
                float t          = Mathf.Clamp01(Mathf.Abs(_currentSteerAngle) / Mathf.Max(_config.MaxSteerAngle, 0.01f));
                float targetLean = -Mathf.Sign(_currentSteerAngle) * _config.MaxLeanAngle * t;
                _currentLeanAngle = Mathf.LerpAngle(_currentLeanAngle, targetLean, smooth);
            }

            transform.localRotation = Quaternion.Euler(localRot.x, localRot.y, _currentLeanAngle);
        }

        private void ConstrainRotation(bool onGround)
        {
            _rb.constraints = onGround
                ? RigidbodyConstraints.FreezeRotationZ
                : RigidbodyConstraints.None;
        }

        private bool IsOnGround() =>
            (_frontWheelCollider != null && _frontWheelCollider.isGrounded) ||
            (_rearWheelCollider  != null && _rearWheelCollider.isGrounded);

        // ── Setup ─────────────────────────────────────────────────────────────

        private void SetupWheels()
        {
            if (_frontWheelCollider) _frontWheelCollider.ConfigureVehicleSubsteps(5, 12, 15);
            if (_rearWheelCollider)  _rearWheelCollider.ConfigureVehicleSubsteps(5, 12, 15);
        }

        private void SetupCenterOfMass()
        {
            if (_frontWheelCollider == null || _rearWheelCollider == null) return;
            float midZ = (_rearWheelCollider.transform.localPosition.z + _frontWheelCollider.transform.localPosition.z) * 0.5f;
            _rb.centerOfMass = new Vector3(
                _config.CenterOfMassOffset.x,
                _config.CenterOfMassOffset.y,
                midZ + _config.CenterOfMassOffset.z);
        }

        // ── Visuals ───────────────────────────────────────────────────────────

        private void SyncHandleBar()
        {
            if (_handlerBar == null) return;
            _handlerBar.localRotation = _handleBarInitRot * Quaternion.Euler(0f, _currentSteerAngle, 0f);
        }

        private static void SyncWheelMesh(WheelCollider col, Transform mesh)
        {
            if (col == null || mesh == null) return;
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }

        private void UpdateCrankset()
        {
            if (_crankset == null) return;
            float speedFwd = Vector3.Dot(_rb.linearVelocity, transform.forward);
            float delta    = speedFwd * 3.6f * _config.CranksetDegreesPerKmh * Time.fixedDeltaTime;
            _crankset.localRotation *= Quaternion.Euler(delta, 0f, 0f);
            var counter = Quaternion.Euler(-delta, 0f, 0f);
            if (_leftPedal)  _leftPedal.localRotation  *= counter;
            if (_rightPedal) _rightPedal.localRotation *= counter;
        }

        // ── IVehicleRiderSource ───────────────────────────────────────────────

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

        // ── WheelCollider Suspension & Friction Presets ───────────────────────
        // Right-click the MotorcycleController component header in Inspector to run.
        // Formulas taken from RayznGames/BicycleSystem/BicycleVehicle.cs.

        [Preserve, ContextMenu("Setup WheelColliders — Motorbike")]
        private void SetupMotorbike()
        {
            var rb = GetComponent<Rigidbody>();
            if (!ValidateSetup(rb)) return;
            ApplySuspension(_frontWheelCollider, 750f * rb.mass, 32.5f * rb.mass);
            ApplySuspension(_rearWheelCollider,  350f * rb.mass, 22.5f * rb.mass);
            // forward: low extremumSlip for early peak, high value for strong traction
            ApplyFriction(_frontWheelCollider,
                fwdExSlip: 0.30f, fwdExVal: 1.25f, fwdAsSlip: 0.5f, fwdAsVal: 1.0f,
                swExSlip:  0.35f, swExVal:  1.56f, swAsSlip:  0.5f, swAsVal:  1.0f);
            ApplyFriction(_rearWheelCollider,
                fwdExSlip: 0.15f, fwdExVal: 2.25f, fwdAsSlip: 0.5f, fwdAsVal: 1.0f,
                swExSlip:  0.30f, swExVal:  2.15f, swAsSlip:  0.5f, swAsVal:  1.0f);
            Debug.Log("[MotorcycleController] Motorbike suspension & friction applied.", this);
        }

        [Preserve, ContextMenu("Setup WheelColliders — Bicycle")]
        private void SetupBicycle()
        {
            var rb = GetComponent<Rigidbody>();
            if (!ValidateSetup(rb)) return;
            ApplySuspension(_frontWheelCollider, 256f * rb.mass, 16.0f * rb.mass);
            ApplySuspension(_rearWheelCollider,  219f * rb.mass, 11.7f * rb.mass);
            ApplyFriction(_frontWheelCollider,
                fwdExSlip: 0.30f, fwdExVal: 1.00f, fwdAsSlip: 0.5f,  fwdAsVal: 1.0f,
                swExSlip:  0.35f, swExVal:  1.25f, swAsSlip:  0.5f,  swAsVal:  1.0f);
            ApplyFriction(_rearWheelCollider,
                fwdExSlip: 0.15f, fwdExVal: 2.50f, fwdAsSlip: 0.6f,  fwdAsVal: 1.75f,
                swExSlip:  0.30f, swExVal:  2.25f, swAsSlip:  0.6f,  swAsVal:  1.75f);
            Debug.Log("[MotorcycleController] Bicycle suspension & friction applied.", this);
        }

        [Preserve, ContextMenu("Setup WheelColliders — Unity Default")]
        private void SetupDefault()
        {
            var rb = GetComponent<Rigidbody>();
            if (!ValidateSetup(rb)) return;
            float spring = 23.33f * rb.mass;
            float damper = 3.00f  * rb.mass;
            ApplySuspension(_frontWheelCollider, spring, damper);
            ApplySuspension(_rearWheelCollider,  spring, damper);
            ApplyFriction(_frontWheelCollider,
                fwdExSlip: 0.30f, fwdExVal: 1.25f, fwdAsSlip: 0.5f, fwdAsVal: 1.0f,
                swExSlip:  0.35f, swExVal:  1.56f, swAsSlip:  0.5f, swAsVal:  1.0f);
            ApplyFriction(_rearWheelCollider,
                fwdExSlip: 0.15f, fwdExVal: 2.25f, fwdAsSlip: 0.5f, fwdAsVal: 1.0f,
                swExSlip:  0.30f, swExVal:  2.15f, swAsSlip:  0.5f, swAsVal:  1.0f);
            Debug.Log("[MotorcycleController] Default suspension & motorbike friction applied.", this);
        }

        private bool ValidateSetup(Rigidbody rb)
        {
            if (rb == null)
            {
                Debug.LogError("[MotorcycleController] Rigidbody not found.", this);
                return false;
            }
            if (_frontWheelCollider == null || _rearWheelCollider == null)
            {
                Debug.LogError("[MotorcycleController] WheelColliders not assigned.", this);
                return false;
            }
            return true;
        }

        private static void ApplySuspension(WheelCollider wheel, float spring, float damper)
        {
            var s = wheel.suspensionSpring;
            s.spring = spring;
            s.damper = damper;
            wheel.suspensionSpring = s;
        }

        private static void ApplyFriction(WheelCollider wheel,
            float fwdExSlip, float fwdExVal, float fwdAsSlip, float fwdAsVal,
            float swExSlip,  float swExVal,  float swAsSlip,  float swAsVal)
        {
            wheel.forwardFriction  = MakeFriction(fwdExSlip, fwdExVal, fwdAsSlip, fwdAsVal);
            wheel.sidewaysFriction = MakeFriction(swExSlip,  swExVal,  swAsSlip,  swAsVal);
        }

        private static WheelFrictionCurve MakeFriction(float exSlip, float exVal, float asSlip, float asVal)
        {
            var c = new WheelFrictionCurve
            {
                extremumSlip   = exSlip,
                extremumValue  = exVal,
                asymptoteSlip  = asSlip,
                asymptoteValue = asVal,
                stiffness      = 1f
            };
            return c;
        }

        private static float WrapAngle(float angle)
        {
            angle %= 360f;
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
