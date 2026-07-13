namespace Game.Gameplay.Character.Locomotion
{
    [System.Serializable]
    public class CharacterConfig
    {
        public float WalkSpeed    = 3f;
        public float RunSpeed     = 5f;
        public float SprintSpeed  = 9f;
        public float CrouchSpeed  = 2f;
        public float JumpForce    = 8f;
        public float Gravity      = -20f;
        public float LandDuration       = 0.45f;
        public float RunThreshold       = 0.5f;
        public float SlopeGraceDuration = 0.15f;

        [UnityEngine.Header("Fall Damage")]
        public float SafeFallSpeed    = 6f;    // m/s — no damage below this (~2 floors)
        public float SafeExitSpeed    = 5f;    // m/s — safe vehicle exit speed (~18 km/h)
        public float DamagePerMps     = 10f;   // damage per m/s above safe threshold (shared)
        public float MinSurviveHealth = 1f;    // impact can't kill — leaves at least this HP

        [UnityEngine.Header("Swim")]
        public bool  CanDive              = true;  // enable/disable diving underwater
        public float SwimSpeed            = 2.5f;
        public float DiveSpeed            = 3f;
        public float SwimSurfaceOffset    = 0.3f;  // how high above the water surface to float while swimming
        public float BuoyancyStrength     = 0.5f;  // 0-1 — how hard buoyancy pulls back to the surface; kept below 1 so actively swimming down (camera + W) can overpower it and submerge
        public float MaxOxygen            = 10f;   // seconds of breath while diving
        public float OxygenDrainRate      = 1f;    // per second while submerged (Dive)
        public float OxygenRegenRate      = 3f;    // per second at surface / on land
        public float DrownDamagePerSecond = 10f;   // damage once oxygen hits 0 while still diving

        [UnityEngine.Header("Ladder")]
        public float ClimbSpeed             = 2f;
        public float ClimbDismountMargin    = 0.15f; // how close to top/bottom before auto-dismount kicks in
        public float ClimbDismountDistance  = 1f;    // step-off distance along LadderFacing when dismounting
        public float ClimbReentryCooldown   = 0.5f;  // seconds after dismount before the ladder can re-trigger Climb
        public float ClimbJumpAwaySpeed     = 4f;    // outward launch speed (m/s) when bailing off the ladder with Jump
    }
}
