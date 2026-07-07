namespace Game.Gameplay.Character.Locomotion
{
    public interface ILocomotionState
    {
        void Enter(LocomotionContext ctx);
        LocomotionStateId Update(LocomotionContext ctx);
        void Exit(LocomotionContext ctx);
    }
}
