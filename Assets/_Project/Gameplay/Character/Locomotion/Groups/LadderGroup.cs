namespace Game.Gameplay.Character.Locomotion.Groups
{
    public class LadderGroup : ILocomotionGroup
    {
        public LocomotionStateId EntryState => LocomotionStateId.Climb;
        public LocomotionStateId ExitState  => LocomotionStateId.Fall;

        public bool ShouldForceEnter(LocomotionContext ctx) => ctx.IsOnLadder;

        public bool Contains(LocomotionStateId id) => id == LocomotionStateId.Climb;
    }
}
