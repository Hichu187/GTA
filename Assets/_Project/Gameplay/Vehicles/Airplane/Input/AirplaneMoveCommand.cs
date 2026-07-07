using UnityEngine;

namespace Game.Gameplay.Vehicles.Airplane
{
    public readonly struct AirplaneMoveCommand
    {
        public readonly float   Throttle;  // 0..1  — W / right trigger
        public readonly float   Pitch;     // -1..1 — up arrow = nose up (-1), down arrow = nose down (+1)
        public readonly float   Roll;      // -1..1 — left arrow = roll left (-1), right = roll right (+1)
        public readonly float   Yaw;       // -1..1 — Q = yaw left (-1), E = yaw right (+1)
        public readonly bool    Brake;     // landing brake — Space
        public readonly Vector2 Look;      // camera

        public AirplaneMoveCommand(float throttle, float pitch, float roll, float yaw, bool brake, Vector2 look)
        {
            Throttle = throttle;
            Pitch    = pitch;
            Roll     = roll;
            Yaw      = yaw;
            Brake    = brake;
            Look     = look;
        }
    }
}
