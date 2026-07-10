using UnityEngine;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    [System.Serializable]
    public class TwoWheelConfig
    {
        [Header("Drive")]
        [Tooltip("Peak motor torque on rear wheel (Nm). 200–400 for a sport bike.")]
        public float MotorForce = 300f;
        [Tooltip("Total brake torque distributed across front/rear by BrakePower ratios.")]
        public float BrakeForce = 500f;
        [Range(0f, 1f), Tooltip("Fraction of BrakeForce applied to front wheel.")]
        public float FrontBrakePower = 0.5f;
        [Range(0f, 1f), Tooltip("Fraction of BrakeForce applied to rear wheel.")]
        public float RearBrakePower = 1f;
        [Tooltip("Top forward speed (km/h). Motor torque tapers to zero at this speed.")]
        public float TopSpeedKmh = 108f;
        [Tooltip("Maximum reverse speed (km/h). Reverse at 40 % of MotorForce.")]
        public float ReverseSpeedKmh = 18f;

        [Header("Steering")]
        [Tooltip("Maximum steer angle (degrees) at low speed.")]
        public float MaxSteerAngle = 35f;
        [Tooltip("Minimum steer angle (degrees) at top speed.")]
        public float MinSteerAngle = 5f;
        [Range(0f, 1f), Tooltip("How strongly speed reduces steering. 0 = no reduction, 1 = full reduction to MinSteerAngle at top speed.")]
        public float SteerReductorAmount = 0.8f;
        [Range(0.001f, 1f), Tooltip("Steer response (×0.1 internally, matching BicycleSystem). 0.8 = gradual, 1.0 = instant.")]
        public float TurnSmoothing = 0.8f;

        [Header("Lean")]
        [Tooltip("Maximum visual lean angle when at full steer (degrees).")]
        public float MaxLeanAngle = 35f;
        [Range(0.01f, 1f), Tooltip("Lean lerp speed (×0.1 internally). 0.8 = gradual, 1.0 = instant.")]
        public float LeanSmoothing = 0.8f;

        [Header("Drivetrain")]
        [Tooltip("Crankset rotation speed (degrees per second per km/h).")]
        public float CranksetDegreesPerKmh = 10f;

        [Header("Physics")]
        [Tooltip("Center of mass offset relative to the midpoint between the two wheels. Y=-0.2 lowers the COG for stability.")]
        public Vector3 CenterOfMassOffset = new Vector3(0f, -0.2f, 0f);

        [Header("Rider")]
        [Tooltip("If true, character mesh is hidden while riding (e.g. for a tank or enclosed vehicle).")]
        public bool HideCharacter = false;

        [Header("Aerodynamics")]
        [Tooltip("Air drag coefficient. linearDamping = speed × AirResistance + MinDamping.")]
        public float AirResistance = 0.002f;
        [Tooltip("Minimum linearDamping always applied (rolling resistance).")]
        public float MinDamping = 0.005f;
        [Tooltip("Angular damping that stabilises yaw wobble. Increases slightly with speed.")]
        public float AngularStability = 2f;
    }
}
