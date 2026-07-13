using UnityEngine;
using Game.Gameplay.Character.Input;

namespace Game.Gameplay.Character.Locomotion
{
    public class LocomotionContext
    {
        public CharacterMoveCommand Command;
        public CharacterController  Controller;
        public CharacterConfig      Config;
        public float                VerticalVelocity;
        public float                MoveSpeed;
        public float                StateTimer;

        public bool  IsGrounded          => Controller.isGrounded;
        public float GroundGraceTimer;
        public bool  IsEffectivelyGrounded => IsGrounded || GroundGraceTimer > 0f;
        public bool  CrouchRequested;  // set by CrouchAbility, read by FSM states

        // Water — written every frame by Character.Update() from CharacterWaterDetector.
        public bool  IsInWater;
        public float SubmersionDepth; // meters below the surface (0 = at/above surface)
        public float WaterSurfaceY;

        // Ladder — written every frame by Character.Update() from CharacterLadderDetector.
        public bool    IsOnLadder;
        public Vector3 LadderMountPosition; // X/Z to snap onto when climbing
        public Vector3 LadderFacing;        // fixed facing direction while climbing
        public float   LadderTopY;          // world Y of the ladder trigger's top bound
        public float   LadderBottomY;       // world Y of the ladder trigger's bottom bound

        // Set by ClimbState on dismount; counted down by Character.Update(), which forces
        // IsOnLadder false while active — prevents instantly re-entering Climb if the
        // dismount step-off still overlaps (or drifts back into) the ladder trigger.
        public float LadderReentryCooldown;

        // One-shot outward kick set by ClimbState's Jump-bail; Character.Update() folds this
        // into its existing exit-launch velocity (same decaying impulse used for vehicle
        // ejection) right after the FSM tick, then clears it.
        public Vector3 PendingLaunchVelocity;
    }
}
