namespace Game.Gameplay.Character.Locomotion.States
{
    public class SprintState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            ctx.MoveSpeed = ctx.Config.SprintSpeed;
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
            if (!ctx.IsEffectivelyGrounded) return LocomotionStateId.Fall;

            if (ctx.Command.JumpPressed) return LocomotionStateId.Jump;

            var mag = ctx.Command.MoveAxis.magnitude;
            if (mag < 0.01f)           return LocomotionStateId.Idle;
            if (!ctx.Command.SprintHeld) return LocomotionStateId.Run;

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
