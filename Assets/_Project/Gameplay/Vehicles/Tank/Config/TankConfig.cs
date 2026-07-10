using UnityEngine;

namespace Game.Gameplay.Vehicles.Tank
{
    [System.Serializable]
    public class TankConfig
    {
        [Header("Rider")]
        [Tooltip("Hide the character model when inside this vehicle.")]
        public bool HideCharacter = true;

        [Header("Drive")]
        [Tooltip("Motor torque per wheel (Nm). 6 wheels × 5000 = 30000 Nm total.")]
        public float MaxMotorTorque = 5000f;
        [Tooltip("Brake torque per wheel (Nm) when no input.")]
        public float MaxBrakeTorque = 15000f;
        [Tooltip("Supplement Y-axis torque (Nm) for pivot. Friction split handles most of it — keep low.")]
        public float SteerTorque    = 15000f;
        [Tooltip("Force (N per m/s of lateral speed) to dampen sideways drift. Lower = more natural sliding in turns.")]
        public float AntiSlipForce  = 3000f;
        [Tooltip("Max speed cap (km/h), applied to total velocity magnitude.")]
        public float TopSpeedKmh    = 50f;

        [Header("Wheel Friction")]
        [Tooltip("Sideways stiffness for center wheels — high value makes them the pivot anchor point.")]
        public float CenterWheelSideStiffness = 1.5f;
        [Tooltip("Sideways stiffness for front/rear outer wheels — low value lets them scrub during pivot.")]
        public float OuterWheelSideStiffness  = 0.25f;

        [Header("Rigidbody")]
        public float AngularDamping = 2f;

        [Header("Turret")]
        [Tooltip("Turret/barrel rotation speed following camera (deg/s).")]
        public float TurretRotSpeed = 90f;
        [Tooltip("Barrel minimum pitch angle (degrees, negative = down).")]
        public float BarrelPitchMin = -10f;
        [Tooltip("Barrel maximum pitch angle (degrees, positive = up).")]
        public float BarrelPitchMax = 10f;

        [Header("Cannon")]
        [Tooltip("Seconds between shots.")]
        public float FireCooldown    = 2f;
        [Tooltip("Shell launch speed (m/s).")]
        public float ShellSpeed      = 80f;
        [Tooltip("Damage per shell hit.")]
        public float ShellDamage     = 100f;
        [Tooltip("Explosion radius (m).")]
        public float ExplosionRadius = 6f;
        [Tooltip("Explosive force applied to nearby rigidbodies.")]
        public float ExplosionForce  = 20000f;
        [Tooltip("Starting ammo count. -1 = infinite.")]
        public int   StartingAmmo    = 30;
    }
}
