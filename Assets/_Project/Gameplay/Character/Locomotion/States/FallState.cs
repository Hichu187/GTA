using UnityEngine;

namespace Game.Gameplay.Character.Locomotion.States
{
    public class FallState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx) { }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            ctx.VerticalVelocity += ctx.Config.Gravity * Time.deltaTime;
            if (ctx.VerticalVelocity < -50f) ctx.VerticalVelocity = -50f;

            if (ctx.IsGrounded) return LocomotionStateId.Land;

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
