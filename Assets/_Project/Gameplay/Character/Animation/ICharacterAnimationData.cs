using UnityEngine;
using Game.Gameplay.Character.Locomotion;

namespace Game.Gameplay.Character.Animation
{
    public interface ICharacterAnimationData
    {
        bool              IsAnimationActive { get; }  // false when in vehicle / unpossessed
        float             MoveSpeed         { get; }
        float             MaxMoveSpeed      { get; }
        bool              IsGrounded        { get; }
        bool              IsCrouching       { get; }
        LocomotionStateId LocomotionState   { get; }
        Vector2           MoveInput         { get; }
        bool              IsArmed           { get; }
        bool              IsAiming          { get; }
        int               WeaponType        { get; }
        // 0 on land; -1..1 while Swim/Dive (negative = diving down, positive = swimming up).
        // Optional third axis for a 3D swim blend tree — safe to ignore if not used.
        float             SwimVerticalInput { get; }
        // True after dying from DamageType.Drown — frozen in the SwimDrowned pose instead
        // of ragdolling (ragdoll physics without buoyancy looks wrong underwater).
        bool              IsDrowned         { get; }
        // True while climbing a ladder (LocomotionState == Climb).
        bool              IsClimbing        { get; }
        // 0 when not climbing; -1..1 while on a ladder (negative = climbing down, positive
        // = climbing up). Optional — lets the animator reverse playback direction.
        float             ClimbVerticalInput { get; }
    }
}
