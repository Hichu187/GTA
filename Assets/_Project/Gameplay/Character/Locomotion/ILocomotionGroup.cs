namespace Game.Gameplay.Character.Locomotion
{
    // A parent-level guard checked every Tick() before the current leaf state runs.
    // Lets a condition (e.g. "in water") pre-empt whatever the active leaf state would
    // otherwise decide, without threading that condition through every leaf state.
    public interface ILocomotionGroup
    {
        LocomotionStateId EntryState { get; }
        LocomotionStateId ExitState  { get; }

        bool ShouldForceEnter(LocomotionContext ctx);
        bool Contains(LocomotionStateId id);
    }
}
