using UnityEngine;

namespace Game.Gameplay.Vehicles.Common
{
    [System.Serializable]
    public class FlyingVehicleConfig
    {
        [Header("Ground")]
        [Tooltip("Acceleration on ground (m/s²).")]
        public float GroundAcceleration  = 20f;
        [Tooltip("Maximum ground speed before takeoff is possible (km/h).")]
        public float GroundTopSpeedKmh   = 126f;
        [Tooltip("Ground speed (km/h) that triggers automatic takeoff. Set 0 for instant (helicopter/rocket).")]
        public float TakeoffSpeedKmh     = 90f;

        [Header("Flight — Speed")]
        [Tooltip("Cruising speed at normal throttle (km/h).")]
        public float NormalFlySpeedKmh   = 216f;
        [Tooltip("Maximum speed with full throttle (km/h).")]
        public float MaxFlySpeedKmh      = 324f;
        [Tooltip("Acceleration when throttle held (m/s²).")]
        public float FlyAcceleration     = 12f;
        [Tooltip("Deceleration when throttle released (m/s²).")]
        public float FlyDeceleration     = 8f;

        [Header("Flight — Turning")]
        [Tooltip("World Y-rotation speed (deg/s) derived from combined yaw + roll input.")]
        public float TurningSpeed        = 25f;

        [Header("Visual Tilt")]
        [Tooltip("Maximum pitch angle for mesh visual (degrees).")]
        public float MaxPitchAngle       = 30f;
        [Tooltip("Maximum roll angle for mesh visual (degrees).")]
        public float MaxRollAngle        = 45f;
        [Tooltip("Pitch visual Lerp factor per second (higher = snappier).")]
        [Range(0.5f, 20f)]
        public float PitchSmooth         = 4f;
        [Tooltip("Roll visual Lerp factor per second.")]
        [Range(0.5f, 20f)]
        public float RollSmooth          = 2f;
    }
}
