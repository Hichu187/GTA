using UnityEngine;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Rocket
{
    [System.Serializable]
    public class RocketConfig : FlyingVehicleConfig
    {
        [Header("Rocket — Thrust")]
        [Tooltip("Initial speed at launch (km/h).")]
        public float LaunchSpeedKmh      = 108f;  // 30 m/s
        [Tooltip("Acceleration when throttle held (m/s²).")]
        public float ThrustAcceleration  = 40f;
        [Tooltip("Deceleration when throttle released (m/s²).")]
        public float CoastDeceleration   = 5f;
        [Tooltip("Maximum rocket speed (km/h).")]
        public float MaxSpeedKmh         = 720f;  // 200 m/s

        [Header("Rocket — Control")]
        [Tooltip("Pitch change rate (deg/s) at full stick.")]
        public float PitchSpeed          = 80f;
        [Tooltip("Roll change rate (deg/s) at full stick.")]
        public float RollSpeed           = 60f;

        [Header("Rocket — Visual")]
        [Tooltip("Exhaust pulse timing multiplier.")]
        public float ExhaustPulseSpeed   = 0.1f;

        public RocketConfig()
        {
            TakeoffSpeedKmh    = 0f;
            GroundAcceleration = 0f;
            GroundTopSpeedKmh  = 0f;
            NormalFlySpeedKmh  = 288f;  // 80 m/s
            MaxFlySpeedKmh     = 720f;  // 200 m/s
            FlyAcceleration    = 40f;
            FlyDeceleration    = 5f;
            TurningSpeed       = 0f;
            MaxPitchAngle      = 85f;
            MaxRollAngle       = 180f;
            PitchSmooth        = 6f;
            RollSmooth         = 4f;
        }
    }
}
