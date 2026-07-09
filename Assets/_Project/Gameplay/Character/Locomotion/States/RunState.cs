namespace Game.Gameplay.Character.Locomotion.States
{
    public class RunState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            ctx.MoveSpeed = ctx.Config.RunSpeed;
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
            if (!ctx.IsEffectivelyGrounded) return LocomotionStateId.Fall;

            if (ctx.Command.JumpPressed)   return LocomotionStateId.Jump;
            if (ctx.CrouchRequested) return LocomotionStateId.Crouch;
            if (ctx.Command.SprintHeld && ctx.Command.MoveAxis.magnitude > 0.01f) return LocomotionStateId.Sprint;

            var mag = ctx.Command.MoveAxis.magnitude;
            if (mag < 0.01f)     return LocomotionStateId.Idle;
            if (mag < ctx.Config.RunThreshold) return LocomotionStateId.Walk;

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
