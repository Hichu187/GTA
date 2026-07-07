namespace Game.Core.Interaction
{
    public interface IInteractable
    {
        bool CanInteract(IInteractor actor);
        void Interact(IInteractor actor);
    }
}
