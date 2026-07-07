using UnityEngine;

namespace Game.Gameplay.Vehicles.Helicopter
{
    public readonly struct HelicopterMoveCommand
    {
        public readonly Vector2 Horizontal; // WASD / left stick — camera-relative XZ movement
        public readonly float   Vertical;   // Q = +1 (ascend), E = -1 (descend)
        public readonly float   Yaw;        // A/D = -1..1 — rotate body in air
        public readonly bool    TakeOff;    // Space — toggle flight
        public readonly Vector2 Look;       // camera

        public HelicopterMoveCommand(Vector2 horizontal, float vertical, float yaw, bool takeOff, Vector2 look)
        {
            Horizontal = horizontal;
            Vertical   = vertical;
            Yaw        = yaw;
            TakeOff    = takeOff;
            Look       = look;
        }
    }
}
