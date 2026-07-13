using UnityEngine;

namespace Game.Gameplay.Character.Locomotion.States
{
    // Climbing a ladder. Entry into this state's group (Ladder) is handled by LadderGroup's
    // guard in LocomotionStateMachine.Tick(). Exit is NOT purely passive like Swim/Dive —
    // climbing is vertical-only (no horizontal input), so simply climbing past the top/bottom
    // of the trigger just means gravity pulls straight back down through the same column,
    // re-triggering Climb instantly. Instead, this state explicitly steps the character off
    // (along LadderFacing) once near the top or bottom while still holding the climb
    // direction, so they land clear of the trigger instead of falling back into it.
    public class ClimbState : ILocomotionState
    {
        public void Enter(LocomotionContext ctx)
        {
            // Snap onto the ladder's rail (X/Z only — keep current height) so climbing
            // doesn't drift sideways off the ladder.
            var pos = ctx.Controller.transform.position;
            ctx.Controller.transform.position = new Vector3(
                ctx.LadderMountPosition.x, pos.y, ctx.LadderMountPosition.z);

            ctx.MoveSpeed = 0f; // no camera-relative horizontal movement while climbing
        }

        public LocomotionStateId Update(LocomotionContext ctx)
        {
            ctx.MoveSpeed = 0f;

            // Bail out mid-climb — push clear of the ladder and add an outward launch
            // velocity on top of Jump's vertical impulse, so it reads as a real jump away
            // from the ladder instead of a snap-then-hover-in-place. The character faces
            // the ladder while climbing (LadderFacing points at it), so jumping off is a
            // push backward relative to that facing, not forward — hence reverse: true.
            if (ctx.Command.JumpPressed)
            {
                DismountOffLadder(ctx, extraUp: 0f, launchSpeed: ctx.Config.ClimbJumpAwaySpeed, reverse: true);
                return LocomotionStateId.Jump;
            }

            float currentY = ctx.Controller.transform.position.y;
            float margin   = ctx.Config.ClimbDismountMargin;

            if (ctx.Command.MoveAxis.y > 0.1f && currentY >= ctx.LadderTopY - margin)
            {
                DismountOffLadder(ctx, extraUp: 0.1f);
                return LocomotionStateId.Idle;
            }

            if (ctx.Command.MoveAxis.y < -0.1f && currentY <= ctx.LadderBottomY + margin)
            {
                DismountOffLadder(ctx, extraUp: 0f);
                return LocomotionStateId.Idle;
            }

            ctx.VerticalVelocity = ctx.Command.MoveAxis.y * ctx.Config.ClimbSpeed;
            return LocomotionStateId.Self;
        }

        public void Exit(LocomotionContext ctx) { }

        private static void DismountOffLadder(LocomotionContext ctx, float extraUp, float launchSpeed = 0f, bool reverse = false)
        {
            var baseFacing = ctx.LadderFacing.sqrMagnitude > 0.0001f
                ? ctx.LadderFacing.normalized
                : Vector3.forward;
            var facing = reverse ? -baseFacing : baseFacing;

            ctx.Controller.transform.position +=
                facing * ctx.Config.ClimbDismountDistance + Vector3.up * extraUp;
            ctx.VerticalVelocity = 0f;

            if (launchSpeed > 0f)
                ctx.PendingLaunchVelocity = facing * launchSpeed;

            // Suppress re-entering Climb for a moment — otherwise drifting back toward the
            // ladder (or the step-off itself still grazing the trigger) instantly re-mounts.
            ctx.LadderReentryCooldown = ctx.Config.ClimbReentryCooldown;
        }
    }
}
