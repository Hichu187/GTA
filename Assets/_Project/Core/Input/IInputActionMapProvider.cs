namespace Game.Core.Input
{
    public interface IInputActionMapProvider
    {
        string ActionMapName { get; }
        void BindActions(IInputBinder binder);
    }
}
