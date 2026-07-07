using UnityEngine;

namespace Game.Gameplay.Vehicles.Car
{
    [System.Serializable]
    public class CarConfig
    {
        [Header("Drive")]
        [Tooltip("Peak motor torque on driven wheels (Nm). 1000–2000 for a typical car.")]
        public float MotorTorque    = 1500f;
        [Tooltip("Brake torque per wheel (Nm). Rear gets 60%, front 40%.")]
        public float BrakeTorque    = 2000f;
        [Tooltip("Max forward speed (m/s). Motor tapers to 0 at this speed.")]
        public float TopSpeed       = 50f;   // ≈ 180 km/h
        [Tooltip("Max reverse speed (m/s).")]
        public float ReverseSpeed   = 12f;
        [Tooltip("Which axle receives motor torque.")]
        public DriveType Drive      = DriveType.RearWheelDrive;

        [Header("Steering")]
        [Tooltip("Front wheel max steer angle (degrees) at low speed.")]
        public float MaxSteerAngle  = 30f;
        [Tooltip("Steer angle lerp speed per FixedUpdate (0 = instant, 1 = no movement).")]
        [Range(0.01f, 0.5f)]
        public float SteerSmooth    = 0.08f;
        [Tooltip("Steer restriction by speed (km/h). Full at 0, limited at highway speeds.")]
        public AnimationCurve SteerRestrictionCurve = new AnimationCurve(
            new Keyframe(0f,   1.00f),
            new Keyframe(40f,  0.50f),
            new Keyframe(120f, 0.20f),
            new Keyframe(200f, 0.10f));

        [Header("Stability")]
        [Tooltip("Anti-roll bar force (N). Prevents body from rolling over in corners.")]
        public float AntiRollForce  = 8000f;
        [Tooltip("Air drag: linearDamping = speed * AirResistance + MinDamping.")]
        public float AirResistance  = 0.04f;
        public float MinDamping     = 0.01f;
    }

    public enum DriveType { FrontWheelDrive, RearWheelDrive, AllWheelDrive }
}
