using UnityEngine;

namespace Game.Gameplay.Character.Locomotion.States
{
    // Surface swimming. Entry/exit into this state's group (Waterborne) is handled by
    // WaterborneGroup's guard in LocomotionStateMachine.Tick() — this state does not
    // need to check ctx.IsInWater itself.
    //
    // Movement is full 3D camera-relative (same as Dive — see the isWaterborne branch in
    // Character.Update()), so holding forward while looking down swims down directly; no
    // button needed to dive. Buoyancy is a soft, weaker-than-full-speed pull back toward
    // the surface (Config.BuoyancyStrength < 1) so it settles the character when idle but
    // doesn't fight a deliberate downward swim.
    public class SwimState : ILocomotionState
    {
        // How far below the surface (meters) counts as "actually diving", not just a dip
        // from buoyancy correction — must exceed DiveState's re-surface threshold (0.05)
        // to avoid flapping back and forth between the two states.
        private const float DiveEnterDepth = 0.15f;

        public void Enter(LocomotionContext ctx)
        {
            ctx.MoveSpeed = ctx.Config.SwimSpeed;
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            ctx.MoveSpeed = ctx.Config.SwimSpeed;

            // Buoyancy: ease vertical velocity so the head pokes just above the surface.
            // Mirrors CharacterWaterDetector's headY calc (feetY + height), solved for the
            // root transform.position.y that puts the head exactly at the target height.
            var   controller  = ctx.Controller;
            float targetHeadY = ctx.WaterSurfaceY + ctx.Config.SwimSurfaceOffset;
            float targetRootY = targetHeadY - controller.center.y - controller.height * 0.5f;
            float diff        = targetRootY - controller.transform.position.y;
            ctx.VerticalVelocity = Mathf.Clamp(diff * 2f, -ctx.Config.SwimSpeed, ctx.Config.SwimSpeed)
                                 * ctx.Config.BuoyancyStrength;

            // Auto-dive: actively swimming (held input) past the surface submerges —
            // no separate button. Passive dips (e.g. falling into water) don't count
            // since they carry no held input.
            if (ctx.Config.CanDive
                && ctx.Command.MoveAxis.magnitude > 0.1f
                && ctx.SubmersionDepth > DiveEnterDepth)
                return LocomotionStateId.Dive;

            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }
    }
}
