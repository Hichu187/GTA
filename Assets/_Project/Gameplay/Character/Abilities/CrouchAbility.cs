using Game.Core.Abilities;

namespace Game.Gameplay.Character.Abilities
{
    public class CrouchAbility : ICharacterAbility
    {
        public bool LocksLocomotion => false;
        public bool IsActive { get; private set; }

        public void Activate() => IsActive = true;
        public void Cancel()   => IsActive = false;
    }
}
