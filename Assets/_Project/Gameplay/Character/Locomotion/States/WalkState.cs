namespace Game.Gameplay.Character.Locomotion.States
{
    public class WalkState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            ctx.MoveSpeed = ctx.Config.WalkSpeed;
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
            else return LocomotionStateId.Fall;

            if (ctx.Command.JumpPressed)   return LocomotionStateId.Jump;
            if (ctx.Command.CrouchPressed) return LocomotionStateId.Crouch;
            if (ctx.Command.SprintHeld && ctx.Command.MoveAxis.magnitude > 0.01f) return LocomotionStateId.Sprint;

            var mag = ctx.Command.MoveAxis.magnitude;
            if (mag < 0.01f) return LocomotionStateId.Idle;
            if (mag > ctx.Config.RunThreshold) return LocomotionStateId.Run;

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
