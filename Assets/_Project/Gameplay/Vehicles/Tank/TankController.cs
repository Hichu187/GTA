using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Tank
{
    [RequireComponent(typeof(TankInputAdapter))]
    [RequireComponent(typeof(TankCameraProvider))]
    [RequireComponent(typeof(TankHUDProvider))]
    public class TankController : VehicleControllerBase, ITankStats
    {
        [SerializeField] private TankConfig _config = new TankConfig();

        [Header("Turret")]
        [SerializeField] private Transform _turretRoot;   // horizontal pivot
        [SerializeField] private Transform _barrelRoot;   // pitch pivot (child of turret)
        [SerializeField] private Transform _barrelTip;    // shell spawn point

        [Header("Shell")]
        [SerializeField] private GameObject _shellPrefab;

        [Header("Tracks")]
        [SerializeField] private WheelCollider[] _leftWheels;    // physics colliders, left track
        [SerializeField] private WheelCollider[] _rightWheels;   // physics colliders, right track
        [SerializeField] private Transform[]     _leftWheelMeshes;   // visual meshes synced via GetWorldPose
        [SerializeField] private Transform[]     _rightWheelMeshes;

        [Header("Center of Mass")]
        [SerializeField] private Transform _centerOfMass;

        private TankInputAdapter   _inputAdapter;
        private TankCameraProvider _cameraProvider;
        private TankHUDProvider    _hudProvider;

        private float _fireTimer;
        private int   _ammoCount;

        // ITankStats
        private float _speedKmh;
        public float  SpeedKmh          => _speedKmh;
        public int    AmmoCount         => _ammoCount;
        public float  FireCooldownRatio => _config.FireCooldown > 0f
                                           ? Mathf.Clamp01(_fireTimer / _config.FireCooldown)
                                           : 0f;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<TankInputAdapter>();
            _cameraProvider = GetComponent<TankCameraProvider>();
            _hudProvider    = GetComponent<TankHUDProvider>();
            _hudProvider.StatsSource = this;

            _rb.isKinematic    = true;
            _rb.angularDamping = _config.AngularDamping;

            if (_centerOfMass != null)
                _rb.centerOfMass = _centerOfMass.localPosition;

            SetupWheelFriction();
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            bool wasKinematic  = _rb.isKinematic;
            _rb.isKinematic    = false;
            _rb.linearDamping  = 0f;
            _rb.angularDamping = _config.AngularDamping;
            if (wasKinematic)
            {
                _rb.linearVelocity  = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
            _ammoCount = _config.StartingAmmo;
        }

        public override void OnUnpossess(PossessionContext context)
        {
            base.OnUnpossess(context);
            // Park the tank when player exits
            ApplyWheels(_leftWheels,  0f, _config.MaxBrakeTorque);
            ApplyWheels(_rightWheels, 0f, _config.MaxBrakeTorque);
        }

        // ── Input / camera ───────────────────────────────────────────────────
        protected override void OnOccupiedUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);

            _fireTimer -= Time.deltaTime;

            if (_inputAdapter.ConsumeFirePressed())
                TryFire();
        }

        // ── Physics (WheelCollider differential drive) ───────────────────────
        protected override void OnOccupiedFixedUpdate()
        {
            var   cmd      = _inputAdapter.Command;
            bool  anyInput = Mathf.Abs(cmd.Throttle) > 0.01f || Mathf.Abs(cmd.Steer) > 0.01f;
            float speedFwd = Vector3.Dot(_rb.linearVelocity, transform.forward);

            // Differential drive: left/right tracks get independent torques
            // W+D → left faster, right slower → turn right while moving
            // D only → left forward, right backward → pivot right in place
            float leftRate  = Mathf.Clamp(cmd.Throttle + cmd.Steer, -1f, 1f);
            float rightRate = Mathf.Clamp(cmd.Throttle - cmd.Steer, -1f, 1f);

            // Top speed cap on total velocity magnitude (catches diagonal/sliding motion too)
            float totalSpeedKmh = _rb.linearVelocity.magnitude * 3.6f;
            if (totalSpeedKmh >= _config.TopSpeedKmh)
            {
                if (leftRate  * speedFwd > 0f) leftRate  = 0f;
                if (rightRate * speedFwd > 0f) rightRate = 0f;
            }

            float motor = anyInput ? _config.MaxMotorTorque : 0f;
            float brake = anyInput ? 0f                     : _config.MaxBrakeTorque;

            ApplyWheels(_leftWheels,  leftRate  * motor, brake);
            ApplyWheels(_rightWheels, rightRate * motor, brake);

            // Extra torque to help pivot against WheelCollider sideways friction scrubbing
            if (Mathf.Abs(cmd.Steer) > 0.01f)
                _rb.AddTorque(Vector3.up * cmd.Steer * _config.SteerTorque, ForceMode.Force);

            // Anti-slip: cancel lateral velocity proportionally (doesn't fight pivot, just removes drift)
            float lateralSpeed = Vector3.Dot(_rb.linearVelocity, transform.right);
            _rb.AddForce(-transform.right * lateralSpeed * _config.AntiSlipForce, ForceMode.Force);

            _speedKmh = speedFwd * 3.6f;

            if (_inputAdapter.ConsumeExitPressed())
                _onExitRequested?.Invoke();
        }

        // ── Friction setup ────────────────────────────────────────────────────
        // Center wheel (index 1) = high stiffness → pivot anchor.
        // Outer wheels (index 0, 2) = low stiffness → scrub freely during pivot.
        private void SetupWheelFriction()
        {
            ApplyFrictionSplit(_leftWheels);
            ApplyFrictionSplit(_rightWheels);
        }

        private void ApplyFrictionSplit(WheelCollider[] wheels)
        {
            if (wheels == null || wheels.Length == 0) return;
            int mid = wheels.Length / 2;
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] == null) continue;
                var f = wheels[i].sidewaysFriction;
                f.stiffness = (i == mid)
                    ? _config.CenterWheelSideStiffness
                    : _config.OuterWheelSideStiffness;
                wheels[i].sidewaysFriction = f;
            }
        }

        protected override void OnAlwaysLateUpdate()
        {
            SyncWheelMeshes(_leftWheels,  _leftWheelMeshes);
            SyncWheelMeshes(_rightWheels, _rightWheelMeshes);
        }

        private static void SyncWheelMeshes(WheelCollider[] wheels, Transform[] meshes)
        {
            if (wheels == null || meshes == null) return;
            int n = Mathf.Min(wheels.Length, meshes.Length);
            for (int i = 0; i < n; i++)
            {
                if (wheels[i] == null || meshes[i] == null) continue;
                wheels[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
                meshes[i].SetPositionAndRotation(pos, rot);
            }
        }

        private static void ApplyWheels(WheelCollider[] wheels, float motorTorque, float brakeTorque)
        {
            if (wheels == null) return;
            foreach (var w in wheels)
            {
                if (w == null) continue;
                w.motorTorque = motorTorque;
                w.brakeTorque = brakeTorque;
            }
        }

        // ── Turret (after Cinemachine LateUpdate) ────────────────────────────
        protected override void OnOccupiedLateUpdate()
        {
            RotateTurretToCamera();
        }

        private void RotateTurretToCamera()
        {
            var   aimDir   = _cameraProvider.GetAimDirection();
            float rotSpeed = _config.TurretRotSpeed;

            // Project aimDir into tank's local XZ plane → turret yaw relative to tank body
            if (_turretRoot != null)
            {
                var localDir = transform.InverseTransformDirection(aimDir);
                localDir.y = 0f;
                if (localDir.sqrMagnitude > 0.001f)
                {
                    _turretRoot.localRotation = Quaternion.RotateTowards(
                        _turretRoot.localRotation,
                        Quaternion.LookRotation(localDir.normalized),
                        rotSpeed * Time.deltaTime);
                }
            }

            // Barrel pitch in turret-local space
            if (_barrelRoot != null)
            {
                float targetPitch = Mathf.Clamp(
                    Mathf.Asin(Mathf.Clamp(aimDir.y, -1f, 1f)) * Mathf.Rad2Deg,
                    _config.BarrelPitchMin, _config.BarrelPitchMax);

                _barrelRoot.localRotation = Quaternion.RotateTowards(
                    _barrelRoot.localRotation,
                    Quaternion.Euler(-targetPitch, 0f, 0f),
                    rotSpeed * Time.deltaTime);
            }
        }

        // ── Fire ─────────────────────────────────────────────────────────────
        private void TryFire()
        {
            if (_shellPrefab == null) return;
            if (_ammoCount == 0)      return;
            if (_fireTimer  > 0f)     return;

            var spawnPoint = _barrelTip != null ? _barrelTip : (_barrelRoot != null ? _barrelRoot : transform);
            var fireDir    = spawnPoint.forward;

            var go    = Instantiate(_shellPrefab, spawnPoint.position, Quaternion.LookRotation(fireDir));
            var shell = go.GetComponent<TankShell>();
            if (shell != null)
                shell.Init(_config.ShellDamage, _config.ExplosionRadius, _config.ExplosionForce);

            var shellRb = go.GetComponent<Rigidbody>();
            if (shellRb != null)
                shellRb.linearVelocity = fireDir * _config.ShellSpeed;

            _fireTimer = _config.FireCooldown;
            if (_ammoCount > 0) _ammoCount--;
            _cameraProvider.TriggerFireShake();
        }
    }
}
