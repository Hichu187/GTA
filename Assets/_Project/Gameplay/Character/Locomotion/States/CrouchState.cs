namespace Game.Gameplay.Character.Locomotion.States
{
    public class CrouchState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            ctx.MoveSpeed = ctx.Config.CrouchSpeed;
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            if (ctx.IsGrounded) ctx.VerticalVelocity = -2f;
            if (!ctx.IsEffectivelyGrounded) return LocomotionStateId.Fall;

            if (!ctx.CrouchRequested)
            {
                return ctx.Command.MoveAxis.magnitude > 0.01f
                    ? LocomotionStateId.Walk
                    : LocomotionStateId.Idle;
            }

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
