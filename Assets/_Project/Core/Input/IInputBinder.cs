using System;
using UnityEngine;

namespace Game.Core.Input
{
    // Implemented by InputManager (Game.Systems.Input).
    // Uses plain C# delegates + UnityEngine.Vector2 so Core stays free
    // of the Unity.InputSystem package dependency.
    public interface IInputBinder
    {
        void BindAxis2D(string actionName, Action<Vector2> onPerformed, Action onCanceled = null);
        void BindAxis1D(string actionName, Action<float>   onPerformed, Action onCanceled = null);
        void BindButton(string actionName, Action onStarted = null, Action onPerformed = null, Action onCanceled = null);
    }
}
