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

            var mag = ctx.Command.MoveAxis.magnitude;
            if (mag < 0.01f)                                        return LocomotionStateId.Idle;
            if (ctx.Command.SprintHeld)                             return LocomotionStateId.Sprint;
            return mag > ctx.Config.RunThreshold ? LocomotionStateId.Run : LocomotionStateId.Walk;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
