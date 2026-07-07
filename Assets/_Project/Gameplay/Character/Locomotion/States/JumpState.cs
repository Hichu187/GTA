using UnityEngine;

namespace Game.Gameplay.Character.Locomotion.States
{
    public class JumpState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            ctx.VerticalVelocity = ctx.Config.JumpForce;
            ctx.MoveSpeed        = ctx.Config.RunSpeed;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            ctx.VerticalVelocity += ctx.Config.Gravity * Time.deltaTime;

            if (ctx.VerticalVelocity <= 0f) return LocomotionStateId.Fall;

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
