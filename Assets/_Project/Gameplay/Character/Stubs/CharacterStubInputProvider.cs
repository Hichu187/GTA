using UnityEngine;
using Game.Core.Input;

namespace Game.Gameplay.Character.Stubs
{
    public class CharacterStubInputProvider : MonoBehaviour, IInputActionMapProvider
    {
        public string  ActionMapName => "Character";
        public Vector2 MoveInput     { get; private set; }

        public void BindActions(IInputBinder binder)
        {
            binder.BindAxis2D("Move",
                onPerformed: v  => MoveInput = v,
                onCanceled:  () => MoveInput = Vector2.zero);
        }
    }
}
