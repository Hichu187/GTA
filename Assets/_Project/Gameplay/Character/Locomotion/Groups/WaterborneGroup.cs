namespace Game.Gameplay.Character.Locomotion.Groups
{
    public class WaterborneGroup : ILocomotionGroup
    {
        public LocomotionStateId EntryState => LocomotionStateId.Swim;
        public LocomotionStateId ExitState  => LocomotionStateId.Fall;

        public bool ShouldForceEnter(LocomotionContext ctx) => ctx.IsInWater;

        public bool Contains(LocomotionStateId id) =>
            id == LocomotionStateId.Swim || id == LocomotionStateId.Dive;
    }
}
