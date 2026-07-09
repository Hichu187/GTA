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
    }
}
