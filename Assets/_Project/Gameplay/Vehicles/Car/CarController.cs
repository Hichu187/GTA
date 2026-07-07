using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Car
{
    [RequireComponent(typeof(CarInputAdapter))]
    [RequireComponent(typeof(CarCameraProvider))]
    [RequireComponent(typeof(CarHUDProvider))]
    public class CarController : VehicleControllerBase, ICarStats
    {
        [SerializeField] private CarConfig _config = new CarConfig();

        [Header("Wheels — assign in Inspector")]
        [SerializeField] private WheelCollider _wheelFL;
        [SerializeField] private WheelCollider _wheelFR;
        [SerializeField] private WheelCollider _wheelRL;
        [SerializeField] private WheelCollider _wheelRR;

        [Header("Wheel Meshes — assign in Inspector")]
        [SerializeField] private Transform _meshFL;
        [SerializeField] private Transform _meshFR;
        [SerializeField] private Transform _meshRL;
        [SerializeField] private Transform _meshRR;

        private CarInputAdapter  _inputAdapter;
        private CarCameraProvider _cameraProvider;
        private CarHUDProvider   _hudProvider;

        // ICarStats
        private float     _speedKmh;
        private GearState _currentGear = GearState.Neutral;
        public  float     SpeedKmh    => _speedKmh;
        public  GearState CurrentGear => _currentGear;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<CarInputAdapter>();
            _cameraProvider = GetComponent<CarCameraProvider>();
            _hudProvider    = GetComponent<CarHUDProvider>();
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
            ResetWheelForces();
            base.OnUnpossess(context);
            _rb.isKinematic = true;
            _currentGear = GearState.Neutral;
        }

        protected override void OnOccupiedFixedUpdate()
        {
            var   cmd      = _inputAdapter.Command;
            float speedFwd = Vector3.Dot(_rb.linearVelocity, transform.forward);
            float speedKmh = speedFwd * 3.6f;
            float speed    = _rb.linearVelocity.magnitude;

            float steerFactor = _config.SteerRestrictionCurve.Evaluate(Mathf.Abs(speedKmh));

            // ── Gear state ────────────────────────────────────────────────────
            UpdateGear(cmd, speedFwd);

            // ── Drive + Brake ─────────────────────────────────────────────────
            ApplyDrive(cmd, speedFwd, steerFactor);

            // ── Steer (front wheels) ──────────────────────────────────────────
            float targetSteer = cmd.Steer * _config.MaxSteerAngle * steerFactor;
            ApplySteer(targetSteer);

            // ── Aerodynamics ──────────────────────────────────────────────────
            _rb.linearDamping = speed * _config.AirResistance + _config.MinDamping;

            // ── Anti-roll bars ────────────────────────────────────────────────
            if (_wheelFL && _wheelFR) ApplyAntiRoll(_wheelFL, _wheelFR);
            if (_wheelRL && _wheelRR) ApplyAntiRoll(_wheelRL, _wheelRR);

            // ── Wheel mesh sync ───────────────────────────────────────────────
            SyncWheelMesh(_wheelFL, _meshFL);
            SyncWheelMesh(_wheelFR, _meshFR);
            SyncWheelMesh(_wheelRL, _meshRL);
            SyncWheelMesh(_wheelRR, _meshRR);

            // ── Stats ─────────────────────────────────────────────────────────
            _speedKmh = speedFwd * 3.6f;

            // ── Exit ──────────────────────────────────────────────────────────
            if (_inputAdapter.ConsumeExitPressed())
                _onExitRequested?.Invoke();
        }

        private void UpdateGear(CarMoveCommand cmd, float speedFwd)
        {
            if (cmd.Throttle > 0.01f)
                _currentGear = GearState.Drive;
            else if (cmd.Brake > 0.01f && speedFwd < 0.5f)
                _currentGear = GearState.Reverse;
            else if (Mathf.Abs(speedFwd) < 0.1f && cmd.Throttle < 0.01f && cmd.Brake < 0.01f)
                _currentGear = GearState.Neutral;
        }

        private void ApplyDrive(CarMoveCommand cmd, float speedFwd, float steerFactor)
        {
            float motorTorque = 0f;
            float brakeTorque = 0f;
            float frontBrake  = 0f;

            if (cmd.Throttle > 0.01f && _currentGear == GearState.Drive)
            {
                // Taper torque near top speed
                float speedRatio = Mathf.Clamp01(speedFwd / _config.TopSpeed);
                motorTorque = cmd.Throttle * _config.MotorTorque * (1f - speedRatio);
            }
            else if (cmd.Brake > 0.01f)
            {
                if (_currentGear == GearState.Reverse && speedFwd > -_config.ReverseSpeed)
                {
                    // Reverse gear: negative torque on driven wheels
                    motorTorque = -cmd.Brake * _config.MotorTorque * 0.5f;
                }
                else if (speedFwd > 0.2f)
                {
                    // Braking while moving forward: rear 60%, front 40%
                    brakeTorque = cmd.Brake * _config.BrakeTorque * 0.6f;
                    frontBrake  = cmd.Brake * _config.BrakeTorque * 0.4f;
                }
            }

            // Apply motor torque based on drive type
            switch (_config.Drive)
            {
                case DriveType.RearWheelDrive:
                    SetWheelTorque(_wheelRL, motorTorque, brakeTorque);
                    SetWheelTorque(_wheelRR, motorTorque, brakeTorque);
                    SetWheelTorque(_wheelFL, 0f,          frontBrake);
                    SetWheelTorque(_wheelFR, 0f,          frontBrake);
                    break;
                case DriveType.FrontWheelDrive:
                    SetWheelTorque(_wheelFL, motorTorque, frontBrake);
                    SetWheelTorque(_wheelFR, motorTorque, frontBrake);
                    SetWheelTorque(_wheelRL, 0f,          brakeTorque);
                    SetWheelTorque(_wheelRR, 0f,          brakeTorque);
                    break;
                case DriveType.AllWheelDrive:
                    float awd = motorTorque * 0.5f;
                    SetWheelTorque(_wheelFL, awd,         frontBrake);
                    SetWheelTorque(_wheelFR, awd,         frontBrake);
                    SetWheelTorque(_wheelRL, awd,         brakeTorque);
                    SetWheelTorque(_wheelRR, awd,         brakeTorque);
                    break;
            }
        }

        private void ApplySteer(float targetAngle)
        {
            if (_wheelFL)
                _wheelFL.steerAngle = Mathf.Lerp(_wheelFL.steerAngle, targetAngle, _config.SteerSmooth);
            if (_wheelFR)
                _wheelFR.steerAngle = Mathf.Lerp(_wheelFR.steerAngle, targetAngle, _config.SteerSmooth);
        }

        // Standard Unity anti-roll bar: resists differential suspension travel left vs right.
        private void ApplyAntiRoll(WheelCollider left, WheelCollider right)
        {
            WheelHit hit;
            float leftTravel  = 1f;
            float rightTravel = 1f;

            bool leftGnd  = left.GetGroundHit(out hit);
            if (leftGnd)
                leftTravel  = (-left.transform.InverseTransformPoint(hit.point).y  - left.radius)  / left.suspensionDistance;

            bool rightGnd = right.GetGroundHit(out hit);
            if (rightGnd)
                rightTravel = (-right.transform.InverseTransformPoint(hit.point).y - right.radius) / right.suspensionDistance;

            float force = (leftTravel - rightTravel) * _config.AntiRollForce;
            if (leftGnd)  _rb.AddForceAtPosition(left.transform.up  *  force, left.transform.position);
            if (rightGnd) _rb.AddForceAtPosition(right.transform.up * -force, right.transform.position);
        }

        private void SetupCenterOfMass()
        {
            // Lower CoM improves handling. Adjust Y to sit slightly above the ground.
            _rb.centerOfMass = new Vector3(0f, -0.3f, 0.1f);
        }

        private void ResetWheelForces()
        {
            foreach (var w in new[] { _wheelFL, _wheelFR, _wheelRL, _wheelRR })
            {
                if (w == null) continue;
                w.motorTorque  = 0f;
                w.brakeTorque  = 0f;
                w.steerAngle   = 0f;
            }
        }

        private static void SetWheelTorque(WheelCollider w, float motor, float brake)
        {
            if (w == null) return;
            w.motorTorque = motor;
            w.brakeTorque = brake;
        }

        private static void SyncWheelMesh(WheelCollider col, Transform mesh)
        {
            if (col == null || mesh == null) return;
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }
    }
}
