using UnityEngine;

namespace Game.Services
{
    public class GameplayServiceLocator : MonoBehaviour
    {
        public static GameplayServiceLocator Current { get; private set; }

        [SerializeField] private PossessionManager _possessionManager;
        public PossessionManager PossessionManager => _possessionManager;

        private void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }
            Current = this;
        }

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }
    }
}
