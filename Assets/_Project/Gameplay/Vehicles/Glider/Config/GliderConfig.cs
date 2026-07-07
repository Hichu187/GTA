using UnityEngine;
using Game.Gameplay.Vehicles.Common;

namespace Game.Gameplay.Vehicles.Glider
{
    [System.Serializable]
    public class GliderConfig : FlyingVehicleConfig
    {
        [Header("Glider — Launch")]
        [Tooltip("Initial speed when glider is first possessed / launched (km/h).")]
        public float LaunchSpeedKmh     = 72f;   // 20 m/s

        [Header("Glider — Physics")]
        [Tooltip("Gravity constant used internally (m/s²).")]
        public float Gravity            = 9.8f;
        [Tooltip("Air drag coefficient — counteracts gravity when flying horizontally. Higher = slower sink.")]
        public float AirDrag            = 9f;
        [Tooltip("Max forward glide speed (km/h). Gained by diving.")]
        public float MaxGlideSpeedKmh   = 288f;  // 80 m/s
        [Tooltip("Minimum forward speed before stall (km/h). Below this, controls become sluggish.")]
        public float StallSpeedKmh      = 18f;   // 5 m/s
        [Tooltip("Pitch angle (degrees) beyond which diving begins accelerating.")]
        public float DiveStartAngle     = 20f;
        [Tooltip("Speed bleed when climbing (extra deceleration on top of gravity, m/s²).")]
        public float ClimbDeceleration  = 3f;
        [Tooltip("Deceleration after dive when returning to level flight (m/s²).")]
        public float PostDiveDecel      = 2f;

        [Header("Glider — Control")]
        [Tooltip("Pitch angle change rate (deg/s) at full input.")]
        public float PitchSpeed         = 55f;
        [Tooltip("Roll angle change rate (deg/s) at full input.")]
        public float RollSpeed          = 40f;
        [Tooltip("How strongly roll drives yaw turn.")]
        public float RollTurnFactor     = 1f;

        [Header("Glider — Brake / Spoilers")]
        [Tooltip("Extra drag applied when spoilers/brake held (m/s² speed reduction).")]
        public float BrakeDrag          = 15f;

        public GliderConfig()
        {
            TakeoffSpeedKmh    = 0f;
            GroundAcceleration = 0f;
            GroundTopSpeedKmh  = 0f;
            NormalFlySpeedKmh  = 72f;
            MaxFlySpeedKmh     = 288f;
            FlyAcceleration    = 0f;
            FlyDeceleration    = 0f;
            TurningSpeed       = 45f;
            MaxPitchAngle      = 85f;
            MaxRollAngle       = 45f;
            PitchSmooth        = 4f;
            RollSmooth         = 2f;
        }
    }
}
