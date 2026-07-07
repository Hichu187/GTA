using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;
using Game.Core.Possession;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Helicopter
{
    [RequireComponent(typeof(HelicopterInputAdapter))]
    [RequireComponent(typeof(HelicopterCameraProvider))]
    [RequireComponent(typeof(HelicopterHUDProvider))]
    public class HelicopterController : FlyingVehicleBase, IHelicopterStats
    {
        [SerializeField] private HelicopterConfig _config = new HelicopterConfig();

        [Header("Rotors (visual)")]
        [SerializeField] private Transform _mainRotor;
        [SerializeField] private Transform _tailRotor;
        [SerializeField] private float     _rotorSpinSpeed = 2000f;

        private HelicopterInputAdapter   _inputAdapter;
        private HelicopterCameraProvider _cameraProvider;
        private HelicopterHUDProvider    _hudProvider;

        private Vector3 _horizontalVelocity;
        private float   _verticalVelocity;

        // IHelicopterStats
        public float SpeedKmh        => new Vector3(_horizontalVelocity.x, 0f, _horizontalVelocity.z).magnitude * 3.6f;
        public float AltitudeM       => transform.position.y;
        public float VerticalSpeedMs => _verticalVelocity;

        // IPossessable
        public override ICameraContextProvider  CameraProvider => _cameraProvider;
        public override IHUDContextProvider     HUDProvider    => _hudProvider;
        public override IInputActionMapProvider InputProvider  => _inputAdapter;

        protected override void Awake()
        {
            base.Awake();
            _inputAdapter   = GetComponent<HelicopterInputAdapter>();
            _cameraProvider = GetComponent<HelicopterCameraProvider>();
            _hudProvider    = GetComponent<HelicopterHUDProvider>();
            _hudProvider.StatsSource = this;
            _rb.isKinematic = true;
        }

        public override void OnPossess(PossessionContext context)
        {
            base.OnPossess(context);
            _rb.isKinematic    = false;
            _rb.useGravity     = false;   // helicopter manages its own Y via vertical input
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity   = 0f;
        }

        public override void OnUnpossess(PossessionContext context)
        {
            base.OnUnpossess(context);   // EndFlight if inAir → restores gravity
            _rb.isKinematic = true;
        }

        // ── Ground: waiting for TakeOff input ───────────────────────────────

        protected override void OnGroundUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);
            if (_inputAdapter.ConsumeTakeOff()) TakeOff();
            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnGroundFixedUpdate()
        {
            SpinRotors(0.2f);
        }

        private void TakeOff()
        {
            BeginFlight();
            // useGravity already false from OnPossess; keep it that way
            _rb.useGravity = false;
        }

        // ── Air ─────────────────────────────────────────────────────────────

        protected override void OnAirUpdate()
        {
            _cameraProvider.HandleLook(_inputAdapter.Command.Look);

            // Land when TakeOff pressed again and near ground
            if (_inputAdapter.ConsumeTakeOff() && IsNearGround())
                Land();

            if (_inputAdapter.ConsumeExitPressed()) _onExitRequested?.Invoke();
        }

        protected override void OnAirFixedUpdate()
        {
            var   cmd = _inputAdapter.Command;
            float dt  = Time.fixedDeltaTime;

            // ── Yaw ──────────────────────────────────────────────────────────
            if (Mathf.Abs(cmd.Yaw) > 0.01f)
                transform.Rotate(Vector3.up, cmd.Yaw * _config.YawSpeed * dt, Space.World);

            // ── Horizontal movement (camera-relative) ─────────────────────────
            Vector3 desiredHorizontal = Vector3.zero;
            if (cmd.Horizontal.magnitude > 0.01f)
            {
                // Use camera provider's VCam forward for camera-relative direction
                Vector3 camFwd   = Vector3.ProjectOnPlane(GetCameraForward(), Vector3.up).normalized;
                Vector3 camRight = Vector3.Cross(Vector3.up, camFwd);
                desiredHorizontal = (camFwd * cmd.Horizontal.y + camRight * cmd.Horizontal.x).normalized
                                  * (_config.NormalHorizontalSpeedKmh / 3.6f);
            }

            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity, desiredHorizontal,
                (desiredHorizontal.magnitude > 0.01f ? _config.HorizontalAcceleration : _config.HorizontalDeceleration) * dt);

            // ── Vertical movement ─────────────────────────────────────────────
            float desiredVertical = cmd.Vertical * (_config.NormalVerticalSpeedKmh / 3.6f);
            _verticalVelocity = Mathf.MoveTowards(
                _verticalVelocity, desiredVertical,
                _config.VerticalAcceleration * dt);

            // ── Velocity override ─────────────────────────────────────────────
            _rb.linearVelocity = new Vector3(
                _horizontalVelocity.x,
                _verticalVelocity,
                _horizontalVelocity.z);

            // ── Visual body tilt from horizontal movement ─────────────────────
            Vector3 localVel = transform.InverseTransformDirection(_horizontalVelocity);
            float normalHorizMs = _config.NormalHorizontalSpeedKmh / 3.6f;
            float speedRatio    = _horizontalVelocity.magnitude / Mathf.Max(normalHorizMs, 0.01f);
            ApplyPitchVisual( localVel.z > 0f ? -speedRatio * _config.MaxBodyTiltAngle
                                               :  speedRatio * _config.MaxBodyTiltAngle,
                              _config.TiltSmooth);
            ApplyRollVisual( -localVel.x / Mathf.Max(normalHorizMs, 0.01f)
                              * _config.MaxBodyTiltAngle,
                              _config.TiltSmooth);

            SpinRotors(1f);
        }

        private void Land()
        {
            _horizontalVelocity = Vector3.zero;
            _verticalVelocity   = 0f;
            EndFlight();
            _rb.useGravity = false;   // keep gravity off so it doesn't drop through ground
            _rb.linearVelocity = Vector3.zero;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool IsNearGround() =>
            Physics.Raycast(transform.position, Vector3.down, 4f);

        private void SpinRotors(float throttle)
        {
            float deg = throttle * _rotorSpinSpeed * Time.fixedDeltaTime;
            if (_mainRotor != null) _mainRotor.Rotate(Vector3.up,     deg, Space.Self);
            if (_tailRotor  != null) _tailRotor.Rotate(Vector3.right,  deg, Space.Self);
        }

        private Vector3 GetCameraForward()
        {
            // Fallback to vehicle forward if camera VCam isn't queryable
            return transform.forward;
        }
    }
}
