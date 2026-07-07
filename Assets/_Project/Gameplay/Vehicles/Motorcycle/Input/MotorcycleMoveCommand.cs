using UnityEngine;

namespace Game.Gameplay.Vehicles.Motorcycle
{
    public readonly struct MotorcycleMoveCommand
    {
        public readonly float   Throttle;  // 0..1
        public readonly float   Brake;     // 0..1
        public readonly float   Steer;     // -1..1
        public readonly Vector2 Look;      // camera delta

        public MotorcycleMoveCommand(float throttle, float brake, float steer, Vector2 look)
        {
            Throttle = throttle;
            Brake    = brake;
            Steer    = steer;
            Look     = look;
        }
    }
}
