using UnityEngine;
using Game.Core.Camera;
using Game.Core.HUD;
using Game.Core.Input;

namespace Game.Core.Possession
{
    public interface IPossessable
    {
        void OnPossess(PossessionContext context);
        void OnUnpossess(PossessionContext context);

        /// <summary>Seat position inside this entity. Null if entity has no seat (e.g. Character).</summary>
        Transform EnterAnchor { get; }
        /// <summary>Spawn point when exiting this entity (e.g. vehicle door). Null if not applicable.</summary>
        Transform ExitAnchor  { get; }

        ICameraContextProvider  CameraProvider { get; }
        IHUDContextProvider     HUDProvider    { get; }
        IInputActionMapProvider InputProvider  { get; }
    }
}
