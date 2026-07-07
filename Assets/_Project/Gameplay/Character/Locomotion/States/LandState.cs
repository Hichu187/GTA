namespace Game.Gameplay.Character.Locomotion.States
{
    public class LandState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            ctx.VerticalVelocity = -2f;
            ctx.MoveSpeed        = 0f;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            if (ctx.StateTimer < ctx.Config.LandDuration) return LocomotionStateId.Self;

            return ctx.Command.MoveAxis.magnitude > 0.01f
                ? LocomotionStateId.Walk
                : LocomotionStateId.Idle;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
