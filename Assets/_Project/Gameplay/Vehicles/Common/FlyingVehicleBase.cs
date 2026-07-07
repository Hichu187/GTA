using UnityEngine;
using Game.Core.Possession;

namespace Game.Gameplay.Vehicles.Common
{
    /// <summary>
    /// Abstract base for all flying vehicles (Airplane, Helicopter, Glider, Rocket …).
    /// Provides:
    ///   • _inAir state with BeginFlight / EndFlight transitions
    ///   • Velocity-override helper SetFlightVelocity (arcade-style, no physics forces)
    ///   • Visual tilt helpers for pitch/roll on child transforms
    ///   • Auto root-yaw turning from combined yaw+roll input (ApplyYawTurn)
    ///   • Dispatch to OnGround*/OnAir* hooks so subclasses stay small
    /// </summary>
    public abstract class FlyingVehicleBase : VehicleControllerBase
    {
        [Header("Flying — Visual Transforms")]
        [Tooltip("Child Transform for pitch visual (mesh lives here). Flight direction follows its forward.")]
        [SerializeField] protected Transform _meshRoot;
        [Tooltip("Child Transform for roll visual (parent of _meshRoot). Optional.")]
        [SerializeField] protected Transform _rollRoot;

        protected bool  _inAir;
        protected float _currentSpeed;

        // ── State transitions ────────────────────────────────────────────────

        protected void BeginFlight()
        {
            _inAir              = true;
            _rb.useGravity      = false;
            _rb.angularVelocity = Vector3.zero;
        }

        protected void EndFlight()
        {
            _inAir        = false;
            _rb.useGravity = true;
            _currentSpeed  = 0f;
        }

        // ── Flight helpers ───────────────────────────────────────────────────

        // Sets rb.linearVelocity directly (kinematic-style, bypasses physics forces).
        protected void SetFlightVelocity(Vector3 direction, float speed)
        {
            _rb.linearVelocity = direction.normalized * speed;
        }

        // Convenience: flight direction is _meshRoot.forward when available.
        protected Vector3 FlightForward =>
            (_meshRoot != null) ? _meshRoot.forward : transform.forward;

        // Apply pitch tilt to _meshRoot (X-axis rotation).
        protected void ApplyPitchVisual(float targetDeg, float smooth)
        {
            if (_meshRoot == null) return;
            _meshRoot.localRotation = Quaternion.Lerp(
                _meshRoot.localRotation,
                Quaternion.Euler(targetDeg, 0f, 0f),
                smooth * Time.deltaTime);
        }

        // Apply roll tilt to _rollRoot (Z-axis rotation, preserves other axes).
        protected void ApplyRollVisual(float targetDeg, float smooth)
        {
            if (_rollRoot == null) return;
            var e = _rollRoot.localRotation.eulerAngles;
            _rollRoot.localRotation = Quaternion.Lerp(
                _rollRoot.localRotation,
                Quaternion.Euler(e.x, e.y, targetDeg),
                smooth * Time.deltaTime);
        }

        // Rotate vehicle root around world Y using combined yaw+roll (mirrors AircraftFlyingSystem).
        protected void ApplyYawTurn(float yawInput, float rollFraction, float turningSpeed)
        {
            float total = yawInput - rollFraction;
            if (Mathf.Abs(total) > 180f) total = -(total % 180f);
            transform.Rotate(
                Vector3.up,
                turningSpeed * Mathf.Clamp(total / 180f, -1f, 1f) * Time.deltaTime,
                Space.World);
        }

        // ── VehicleControllerBase dispatch ───────────────────────────────────

        protected override void OnOccupiedUpdate()
        {
            if (_inAir) OnAirUpdate();
            else        OnGroundUpdate();
        }

        protected override void OnOccupiedFixedUpdate()
        {
            if (_inAir) OnAirFixedUpdate();
            else        OnGroundFixedUpdate();
        }

        // ── Subclass hooks ───────────────────────────────────────────────────
        protected virtual void OnGroundUpdate()      { }
        protected virtual void OnAirUpdate()         { }
        protected virtual void OnGroundFixedUpdate() { }
        protected virtual void OnAirFixedUpdate()    { }

        // ── Possession lifecycle ─────────────────────────────────────────────
        public override void OnUnpossess(PossessionContext context)
        {
            if (_inAir) EndFlight();
            base.OnUnpossess(context);
        }
    }
}
