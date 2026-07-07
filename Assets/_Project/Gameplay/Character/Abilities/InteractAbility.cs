using Game.Core.Abilities;
using Game.Core.Interaction;

namespace Game.Gameplay.Character.Abilities
{
    // One-shot: Activate() fires TryInteract immediately, never stays active.
    public class InteractAbility : ICharacterAbility
    {
        public bool LocksLocomotion => false;
        public bool IsActive        => false;

        private readonly InteractionDetector _detector;
        private readonly IInteractor         _interactor;

        public InteractAbility(InteractionDetector detector, IInteractor interactor)
        {
            _detector   = detector;
            _interactor = interactor;
        }

        public void Activate() => _detector.TryInteract(_interactor);
        public void Cancel()   { }
    }
}
