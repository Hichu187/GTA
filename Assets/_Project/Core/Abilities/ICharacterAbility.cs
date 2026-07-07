namespace Game.Core.Abilities
{
    public interface ICharacterAbility
    {
        bool LocksLocomotion { get; }
        bool IsActive        { get; }
        void Activate();
        void Cancel();
    }
}
