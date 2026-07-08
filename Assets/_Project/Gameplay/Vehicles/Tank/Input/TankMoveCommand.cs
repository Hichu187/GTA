using UnityEngine;

namespace Game.Gameplay.Vehicles.Tank
{
    public readonly struct TankMoveCommand
    {
        public readonly float   Throttle;  // -1..1  (negative = reverse)
        public readonly float   Steer;     // -1..1
        public readonly Vector2 Look;
        public readonly bool    Fire;

        public TankMoveCommand(float throttle, float steer, Vector2 look, bool fire)
        {
            Throttle = throttle;
            Steer    = steer;
            Look     = look;
            Fire     = fire;
        }
    }
}
