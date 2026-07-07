using UnityEngine;

namespace Game.Gameplay.Vehicles.Car
{
    public readonly struct CarMoveCommand
    {
        public readonly float   Throttle;  // 0..1
        public readonly float   Brake;     // 0..1
        public readonly float   Steer;     // -1..1
        public readonly Vector2 Look;
        public readonly bool    HornPressed;

        public CarMoveCommand(float throttle, float brake, float steer, Vector2 look, bool hornPressed)
        {
            Throttle    = throttle;
            Brake       = brake;
            Steer       = steer;
            Look        = look;
            HornPressed = hornPressed;
        }
    }
}
