using UnityEngine;

namespace Game.Gameplay.Vehicles.Tank
{
    [System.Serializable]
    public class TankConfig
    {
        [Header("Drive")]
        [Tooltip("Force applied forward/backward (N). Tank mass ~8000 kg → needs large values.")]
        public float DriveForce     = 80000f;
        [Tooltip("Torque applied for pivot steering (Nm).")]
        public float TurnTorque     = 60000f;
        [Tooltip("Max forward speed (km/h).")]
        public float TopSpeedKmh    = 50f;

        [Header("Stability")]
        // v_terminal = DriveForce / (mass × LinearDamping). At 80000/(8000×0.5)=20 m/s, capped at TopSpeedKmh.
        public float LinearDamping  = 0.5f;
        public float AngularDamping = 2f;

        [Header("Turret")]
        [Tooltip("Turret rotation speed (deg/s).")]
        public float TurretRotSpeed  = 60f;
        [Tooltip("Barrel minimum pitch angle (degrees, negative = down).")]
        public float BarrelPitchMin  = -5f;
        [Tooltip("Barrel maximum pitch angle (degrees, positive = up).")]
        public float BarrelPitchMax  = 30f;

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
