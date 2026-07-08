using UnityEngine;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Helicopter
{
    [System.Serializable]
    public class HelicopterConfig : FlyingVehicleConfig
    {
        [Header("Helicopter — Horizontal")]
        [Tooltip("Normal horizontal cruise speed (km/h).")]
        public float NormalHorizontalSpeedKmh  = 108f;  // 30 m/s
        [Tooltip("Maximum horizontal speed (km/h).")]
        public float MaxHorizontalSpeedKmh     = 180f;  // 50 m/s
        [Tooltip("Horizontal acceleration (m/s²).")]
        public float HorizontalAcceleration    = 15f;
        [Tooltip("Horizontal deceleration when no input (m/s²).")]
        public float HorizontalDeceleration    = 20f;

        [Header("Helicopter — Vertical")]
        [Tooltip("Normal vertical speed (km/h).")]
        public float NormalVerticalSpeedKmh    = 29f;   // 8 m/s
        [Tooltip("Maximum vertical speed (km/h).")]
        public float MaxVerticalSpeedKmh       = 54f;   // 15 m/s
        [Tooltip("Vertical acceleration (m/s²).")]
        public float VerticalAcceleration      = 10f;

        [Header("Helicopter — Rotation")]
        [Tooltip("Yaw rotation speed in air (deg/s).")]
        public float YawSpeed                  = 60f;
        [Tooltip("Speed at which nose auto-aligns to horizontal velocity direction (deg/s). 0 = off.")]
        public float AutoYawSpeed              = 90f;

        [Header("Helicopter — Visual")]
        [Tooltip("Body tilt angle when moving horizontally (degrees).")]
        public float MaxBodyTiltAngle          = 18f;
        [Tooltip("Body tilt Lerp factor.")]
        [Range(0.5f, 20f)]
        public float TiltSmooth                = 3f;

        public HelicopterConfig()
        {
            TakeoffSpeedKmh    = 0f;
            GroundAcceleration = 0f;
            GroundTopSpeedKmh  = 0f;
            NormalFlySpeedKmh  = 108f;
            MaxFlySpeedKmh     = 180f;
            FlyAcceleration    = 15f;
            FlyDeceleration    = 20f;
            TurningSpeed       = 0f;
            MaxPitchAngle      = MaxBodyTiltAngle;
            MaxRollAngle       = MaxBodyTiltAngle;
            PitchSmooth        = 3f;
            RollSmooth         = 3f;
        }
    }
}
