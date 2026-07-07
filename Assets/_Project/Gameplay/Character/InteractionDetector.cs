using UnityEngine;
using Game.Core.Interaction;

namespace Game.Gameplay.Character
{
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float     _radius          = 2f;
        [SerializeField] private LayerMask _interactableLayer = ~0;

        private readonly Collider[] _buffer = new Collider[8];

        public IInteractable Current { get; private set; }

        private void Update()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _radius, _buffer, _interactableLayer);

            Current          = null;
            float bestDistSq = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (!_buffer[i].TryGetComponent<IInteractable>(out var candidate)) continue;

                float distSq = (transform.position - _buffer[i].transform.position).sqrMagnitude;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    Current    = candidate;
                }
            }
        }

        public void TryInteract(IInteractor actor)
        {
            if (Current != null && Current.CanInteract(actor))
                Current.Interact(actor);
        }
    }
}
