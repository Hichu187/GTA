using UnityEngine;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Airplane
{
    [System.Serializable]
    public class AirplaneConfig : FlyingVehicleConfig
    {
        [Header("Airplane — Roll / Yaw")]
        [Tooltip("Roll angle accumulation rate (deg/s).")]
        public float RollSpeed  = 80f;
        [Tooltip("Direct yaw rotation speed (deg/s) — manual rudder input.")]
        public float YawSpeed   = 15f;

        [Header("Airplane — Vertical")]
        [Tooltip("Vertical speed (m/s) applied when pitch button is held.")]
        public float ClimbSpeed     = 15f;
        [Tooltip("Maximum downward speed (m/s) when fully stalled (speed = 0). Gravity accelerates toward this cap.")]
        public float StallFallSpeed = 30f;

        [Header("Airplane — Brake (air)")]
        [Tooltip("Multiplier on FlyDeceleration when Brake held in air.")]
        public float BrakeFriction = 3f;

        [Header("Airplane — Landing")]
        [Tooltip("Ground-detection ray length (m) below the ray origin.")]
        public float LandingHeight       = 5f;
        [Tooltip("Minimum downward speed (m/s) required for natural auto-land.")]
        public float LandingDescendSpeed = 1f;
        [Tooltip("Seconds after takeoff before landing can trigger (prevents instant re-land).")]
        public float TakeoffCooldown     = 2.5f;

        public AirplaneConfig()
        {
            TakeoffSpeedKmh    = 90f;   // 25 m/s — auto-takeoff + stall threshold
            GroundAcceleration = 20f;
            GroundTopSpeedKmh  = 126f;  // 35 m/s
            NormalFlySpeedKmh  = 216f;  // 60 m/s cruise
            MaxFlySpeedKmh     = 324f;  // 90 m/s max
            FlyAcceleration    = 12f;
            FlyDeceleration    = 8f;
            TurningSpeed       = 30f;
            MaxPitchAngle      = 30f;
            MaxRollAngle       = 45f;
            PitchSmooth        = 5f;
            RollSmooth         = 3f;
        }
    }
}
