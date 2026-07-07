using Game.Core.Abilities;

namespace Game.Gameplay.Character.Abilities
{
    // Generic locomotion lock used by interactables (Sit, PushObject) via IInteractor.SetLocomotionLocked.
    public class LocomotionLockAbility : ICharacterAbility
    {
        public bool LocksLocomotion => true;
        public bool IsActive { get; private set; }

        public void Activate() => IsActive = true;
        public void Cancel()   => IsActive = false;
    }
}
