using UnityEngine;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    [System.Serializable]
    public class MotorcycleConfig
    {
        [Header("Drive")]
        [Tooltip("Peak motor torque applied to rear wheel (Nm). 200-400 for a sport bike.")]
        public float MotorTorque  = 300f;
        [Tooltip("Brake torque on rear wheel (Nm). Front gets 50% of this.")]
        public float BrakeTorque  = 500f;
        [Tooltip("Max forward speed (m/s). Motor torque tapers to 0 at this speed.")]
        public float TopSpeed     = 30f;   // ≈ 108 km/h
        [Tooltip("Max reverse speed (m/s). Reverse at 40% motor torque.")]
        public float ReverseSpeed = 5f;

        [Header("Steering")]
        [Tooltip("Max front wheel steer angle at low speed (degrees).")]
        public float MaxSteerAngle = 35f;
        [Tooltip("How fast the steer angle changes. 0 = instant, 1 = no change.")]
        [Range(0.01f, 0.5f)]
        public float SteerSmooth   = 0.12f;
        [Tooltip("Multiplies MaxSteerAngle by speed (km/h). Full angle at 0, restricted at high speed.")]
        public AnimationCurve SteerRestrictionCurve = new AnimationCurve(
            new Keyframe(0f,  1.00f),
            new Keyframe(20f, 0.40f),
            new Keyframe(60f, 0.12f),
            new Keyframe(120f, 0.06f));

        [Header("Lean")]
        [Tooltip("Maximum lean angle (degrees). Matches MaxSteerAngle visually.")]
        public float MaxLeanAngle = 35f;
        [Tooltip("PD controller proportional gain — how aggressively bike leans.")]
        public float LeanTorque   = 18f;
        [Tooltip("PD controller derivative gain — damps lean oscillation.")]
        public float LeanDamping  = 7f;

        [Header("Aerodynamics")]
        [Tooltip("Air drag coefficient. linearDamping = speed * AirResistance + MinDamping.")]
        public float AirResistance = 0.07f;
        [Tooltip("Minimum linearDamping always applied (simulates rolling resistance).")]
        public float MinDamping    = 0.03f;
        [Tooltip("Angular damping that stabilizes yaw wobble at speed. Applied additively.")]
        public float AngularStability = 2f;
    }
}
