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
        [Tooltip("Extra pitch angle (degrees) applied when climbing/descending at max vertical speed.")]
        public float ClimbTiltAngle            = 8f;

        [Header("Helicopter — Engine")]
        [Tooltip("Engine power change per second while holding EngineUp/EngineDown (0-100 scale).")]
        public float EnginePowerRampSpeed      = 40f;
        [Tooltip("Engine power (0-100) above which the helicopter lifts off / stays airborne. Below it, it descends and lands.")]
        public float LiftoffThreshold          = 45f;
        [Tooltip("Maximum height above ground (meters) reachable at full engine power.")]
        public float MaxAltitudeAboveGround    = 150f;
        [Tooltip("Distance below the altitude ceiling (meters) over which climb speed eases from max down to 0.")]
        public float CeilingSoftZone           = 30f;
        [Tooltip("Height above ground (meters) at which the helicopter is considered landed — below this, engine power under LiftoffThreshold fully cuts the engine and settles it on the ground.")]
        public float LandingHeight             = 4f;

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
