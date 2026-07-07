using UnityEngine;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Airplane
{
    [System.Serializable]
    public class AirplaneConfig : FlyingVehicleConfig
    {
        [Header("Airplane — Control Surfaces")]
        [Tooltip("Pitch angle change rate (deg/s) at full stick input.")]
        public float PitchSpeed    = 60f;
        [Tooltip("Roll angle change rate (deg/s) at full stick input.")]
        public float RollSpeed     = 75f;
        [Tooltip("Direct yaw rotation speed (deg/s) in air — supplements roll-driven turning.")]
        public float YawSpeed      = 20f;

        [Header("Airplane — Landing")]
        [Tooltip("Raycast height (m) below which holding Brake triggers auto-land.")]
        public float LandingHeight = 3.5f;
        [Tooltip("Deceleration multiplier when landed and braking.")]
        public float BrakeFriction = 2.5f;

        public AirplaneConfig()
        {
            TakeoffSpeedKmh    = 90f;     // 25 m/s
            GroundAcceleration = 20f;
            GroundTopSpeedKmh  = 126f;    // 35 m/s
            NormalFlySpeedKmh  = 216f;    // 60 m/s
            MaxFlySpeedKmh     = 324f;    // 90 m/s
            FlyAcceleration    = 12f;
            FlyDeceleration    = 8f;
            TurningSpeed       = 28f;
            MaxPitchAngle      = 35f;
            MaxRollAngle       = 50f;
            PitchSmooth        = 3f;
            RollSmooth         = 2f;
        }
    }
}
