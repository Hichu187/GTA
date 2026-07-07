using UnityEngine;

namespace Game.Gameplay.Vehicles.Glider
{
    public readonly struct GliderMoveCommand
    {
        public readonly float   Pitch;  // ↑ = nose up (lose speed), ↓ = nose down (gain speed)
        public readonly float   Roll;   // ←→ = bank left/right
        public readonly float   Brake;  // 0..1 — spoilers / air brakes
        public readonly Vector2 Look;   // camera

        public GliderMoveCommand(float pitch, float roll, float brake, Vector2 look)
        {
            Pitch = pitch;
            Roll  = roll;
            Brake = brake;
            Look  = look;
        }
    }
}
