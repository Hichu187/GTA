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
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            bool wasKinematic = _rb.isKinematic;
            _rb.isKinematic    = false;
            _rb.linearDamping  = _config.LinearDamping;
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
            // Leave physics alive — tank coasts to a stop.
        }

        // ── Input / camera ───────────────────────────────────────────────────
        protected override void OnOccupiedUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);

            _fireTimer -= Time.deltaTime;

            if (_inputAdapter.ConsumeFirePressed())
                TryFire();
        }

        // ── Physics ──────────────────────────────────────────────────────────
        protected override void OnOccupiedFixedUpdate()
        {
            var   cmd         = _inputAdapter.Command;
            float speedFwd    = Vector3.Dot(_rb.linearVelocity, transform.forward);
            float topSpeedMs  = _config.TopSpeedKmh / 3.6f;

            // Drive — taper force near top speed; brake when no input
            if (Mathf.Abs(cmd.Throttle) > 0.01f)
            {
                float speedRatio = Mathf.Clamp01(Mathf.Abs(speedFwd) / topSpeedMs);
                float force      = cmd.Throttle * _config.DriveForce * (1f - speedRatio * 0.9f);
                _rb.AddForce(transform.forward * force, ForceMode.Force);
            }
            else if (Mathf.Abs(speedFwd) > 0.05f)
            {
                // Counter-force to stop naturally when no throttle input
                _rb.AddForce(-transform.forward * speedFwd * _config.DriveForce * 0.3f, ForceMode.Force);
            }

            // Pivot steer — always active (tank can rotate in place)
            if (Mathf.Abs(cmd.Steer) > 0.01f)
                _rb.AddTorque(Vector3.up * cmd.Steer * _config.TurnTorque, ForceMode.Force);

            _speedKmh = speedFwd * 3.6f;

            if (_inputAdapter.ConsumeExitPressed())
                _onExitRequested?.Invoke();
        }

        // ── Turret (after Cinemachine LateUpdate) ────────────────────────────
        protected override void OnOccupiedLateUpdate()
        {
            RotateTurretToCamera();
        }

        private void RotateTurretToCamera()
        {
            var aimDir = _cameraProvider.GetAimDirection();

            // Turret body — horizontal rotation only
            if (_turretRoot != null)
            {
                var flatDir = new Vector3(aimDir.x, 0f, aimDir.z);
                if (flatDir.sqrMagnitude > 0.001f)
                {
                    var targetRot = Quaternion.LookRotation(flatDir);
                    _turretRoot.rotation = Quaternion.RotateTowards(
                        _turretRoot.rotation, targetRot,
                        _config.TurretRotSpeed * Time.deltaTime);
                }
            }

            // Barrel — pitch up/down
            if (_barrelRoot != null)
            {
                float pitch      = Mathf.Asin(Mathf.Clamp(aimDir.y, -1f, 1f)) * Mathf.Rad2Deg;
                pitch            = Mathf.Clamp(pitch, _config.BarrelPitchMin, _config.BarrelPitchMax);

                var   localEul   = _barrelRoot.localEulerAngles;
                float currentX   = localEul.x > 180f ? localEul.x - 360f : localEul.x;
                float targetX    = -pitch;   // negative because up = negative local X
                float newX       = Mathf.MoveTowards(currentX, targetX,
                                       _config.TurretRotSpeed * Time.deltaTime);
                _barrelRoot.localEulerAngles = new Vector3(newX, localEul.y, localEul.z);
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
        }
    }
}
