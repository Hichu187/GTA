using UnityEngine;

namespace Game.Gameplay.Vehicles.Airplane
{
    [System.Serializable]
    public class AirplaneConfig
    {
        [Header("Engine")]
        [Tooltip("Maximum engine thrust (N). ~40000 for a small aircraft, ~80000 for a jet.")]
        public float MaxThrust      = 60000f;
        [Tooltip("How fast throttle response builds up (0=instant, lerp factor).")]
        [Range(0.01f, 0.3f)]
        public float ThrottleSmooth = 0.05f;
        [Tooltip("Max forward speed (m/s). Engine thrust tapers off near this.")]
        public float TopSpeed       = 120f;   // ≈ 432 km/h

        [Header("Aerodynamics")]
        [Tooltip("Lift per (m/s)². Lift = speed² × LiftCoefficient × angle-of-attack. Tune so plane lifts off at StallSpeed.")]
        public float LiftCoefficient  = 0.025f;
        [Tooltip("Speed below which wings produce no useful lift (m/s).")]
        public float StallSpeed       = 25f;
        [Tooltip("Air drag: linearDamping = speed × AirResistance. Limits top speed passively.")]
        public float AirResistance    = 0.015f;
        [Tooltip("Landing brake drag multiplier — applied to linearDamping when Brake held on ground.")]
        public float BrakeDrag        = 0.5f;

        [Header("Control Surfaces")]
        [Tooltip("Pitch torque (Nm/unit input). Controls nose up/down.")]
        public float PitchTorque      = 12f;
        [Tooltip("Roll torque (Nm/unit input). Controls banking left/right.")]
        public float RollTorque       = 8f;
        [Tooltip("Yaw torque (Nm/unit input). Controls rudder left/right.")]
        public float YawTorque        = 4f;
        [Tooltip("Angular damping applied always — resists unintended rotation.")]
        public float AngularDamping   = 3f;
        [Tooltip("Multiplies control effectiveness below stall speed (0 = no control when slow).")]
        public AnimationCurve ControlEffectiveness = new AnimationCurve(
            new Keyframe(0f,   0.0f),
            new Keyframe(15f,  0.3f),
            new Keyframe(30f,  1.0f),
            new Keyframe(120f, 1.0f));
    }
}
