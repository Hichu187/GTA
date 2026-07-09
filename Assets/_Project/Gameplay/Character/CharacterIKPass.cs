using UnityEngine;

namespace Game.Gameplay.Character
{
    // Placed (auto-added at runtime) on the same GameObject as the Animator.
    // Relays OnAnimatorIK to CharacterVehicleRider on the character root,
    // because OnAnimatorIK is only sent to scripts on the Animator's own GameObject.
    [RequireComponent(typeof(Animator))]
    public class CharacterIKPass : MonoBehaviour
    {
        public event System.Action<int> IKUpdate;
        private void OnAnimatorIK(int layer) => IKUpdate?.Invoke(layer);
    }
}
