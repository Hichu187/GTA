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
    }
}
