using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Core.Possession
{
    public interface IPossessable
    {
        void OnPossess(PossessionContext context);
        void OnUnpossess();
        ICameraContextProvider CameraProvider { get; }
        IHUDContextProvider    HUDProvider    { get; }
        IInputActionMapProvider InputProvider { get; }
    }
}
