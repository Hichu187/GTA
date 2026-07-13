namespace Game.Gameplay.Character.Locomotion.States
{
    // Submerged swimming. Only reachable from SwimState when Config.CanDive is true and
    // the player actively swims past the surface — see SwimState's auto-dive check. No
    // separate button; that one check in SwimState is the entire "enable/disable diving"
    // gate, no other system needs to know about the toggle.
    //
    // Movement direction (including vertical) follows the camera exactly — see the
    // isWaterborne branch in Character.Update() that skips flattening camForward/camRight
    // to the XZ plane for both Swim and Dive. This state only sets MoveSpeed; there is no
    // buoyancy while diving — no input means holding perfectly still, same as land.
    public class DiveState : ILocomotionState
    {
        // Entering Dive happens right past the surface (SwimState's auto-dive threshold is
        // only 0.15m deep), so SubmersionDepth starts small. Without a short grace window
        // the very first Update() right after Enter() could immediately bounce back to
        // Swim before the player has had a chance to keep swimming down.
        private const float SurfaceCheckGrace = 0.25f;

        public void Enter(LocomotionContext ctx)
        {
            ctx.MoveSpeed        = ctx.Config.DiveSpeed;
            ctx.VerticalVelocity = 0f; // no buoyancy while diving — clear any leftover from Swim
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            ctx.MoveSpeed = ctx.Config.DiveSpeed;

            if (ctx.StateTimer > SurfaceCheckGrace && ctx.SubmersionDepth <= 0.05f)
                return LocomotionStateId.Swim;

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
