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
        public float LandDuration = 0.15f;
        public float RunThreshold = 0.5f;
    }
}
