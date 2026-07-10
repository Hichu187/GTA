namespace Game.Core.Input
{
    public interface IAimToggleable
    {
        void ToggleAim();
        bool AimToggleActive { get; }
    }
}
