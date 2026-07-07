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

        public bool IsGrounded     => Controller.isGrounded;
        public bool CrouchRequested;  // set by CrouchAbility, read by FSM states
    }
}
