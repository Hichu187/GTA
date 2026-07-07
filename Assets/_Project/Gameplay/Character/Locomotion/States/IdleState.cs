using UnityEngine;

namespace Game.Gameplay.Character.Locomotion.States
{
    public class IdleState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            ctx.MoveSpeed = 0f;
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
            else return LocomotionStateId.Fall;

            if (ctx.CrouchRequested) return LocomotionStateId.Crouch;
            if (ctx.Command.JumpPressed)   return LocomotionStateId.Jump;

            var mag = ctx.Command.MoveAxis.magnitude;
            if (mag > 0.01f)
            {
                if (ctx.Command.SprintHeld) return LocomotionStateId.Sprint;
                return mag > ctx.Config.RunThreshold ? LocomotionStateId.Run : LocomotionStateId.Walk;
            }

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
