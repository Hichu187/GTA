using UnityEngine;

namespace Game.Gameplay.Vehicles.Helicopter
{
    public readonly struct HelicopterMoveCommand
    {
        public readonly Vector2 Horizontal; // WASD / left stick — camera-relative forward/back (Y), yaw (X)
        public readonly float   Yaw;        // Arrow keys / shoulders — rotate body in air
        public readonly bool    EngineUp;   // Q / right trigger — hold to spool engine power up
        public readonly bool    EngineDown; // E / left trigger — hold to spool engine power down
        public readonly Vector2 Look;       // camera

        public HelicopterMoveCommand(Vector2 horizontal, float yaw, bool engineUp, bool engineDown, Vector2 look)
        {
            Horizontal = horizontal;
            Yaw        = yaw;
            EngineUp   = engineUp;
            EngineDown = engineDown;
            Look       = look;
        }
    }
}
