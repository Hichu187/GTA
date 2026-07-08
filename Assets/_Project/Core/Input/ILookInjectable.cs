using UnityEngine;

namespace Game.Core.Input
{
    // Implemented by any InputAdapter that accepts drag-based look input from mobile controls.
    public interface ILookInjectable
    {
        void InjectLook(Vector2 delta);
    }
}
