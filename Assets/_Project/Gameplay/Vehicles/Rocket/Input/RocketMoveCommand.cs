using UnityEngine;

namespace Game.Gameplay.Vehicles.Rocket
{
    public readonly struct RocketMoveCommand
    {
        public readonly float   Throttle; // 0..1 — W / trigger
        public readonly float   Pitch;    // -1..1 — ↑↓
        public readonly float   Roll;     // -1..1 — ←→
        public readonly Vector2 Look;     // camera

        public RocketMoveCommand(float throttle, float pitch, float roll, Vector2 look)
        {
            Throttle = throttle;
            Pitch    = pitch;
            Roll     = roll;
            Look     = look;
        }
    }
}
