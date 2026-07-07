using UnityEngine;

namespace Game.Gameplay.Character.Input
{
    public readonly struct CharacterMoveCommand
    {
        public readonly Vector2 MoveAxis;
        public readonly Vector2 LookAxis;
        public readonly bool    JumpPressed;
        public readonly bool    SprintHeld;
        public readonly bool    CrouchPressed;
        public readonly bool    InteractPressed;

        public CharacterMoveCommand(
            Vector2 moveAxis, Vector2 lookAxis,
            bool jumpPressed, bool sprintHeld,
            bool crouchPressed, bool interactPressed)
        {
            MoveAxis        = moveAxis;
            LookAxis        = lookAxis;
            JumpPressed     = jumpPressed;
            SprintHeld      = sprintHeld;
            CrouchPressed   = crouchPressed;
            InteractPressed = interactPressed;
        }
    }
}
