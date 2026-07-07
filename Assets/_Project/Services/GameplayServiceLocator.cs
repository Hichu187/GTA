using UnityEngine;
using Game.Systems.Persistence;

namespace Game.Services
{
    public class GameplayServiceLocator : MonoBehaviour
    {
        public static GameplayServiceLocator Current { get; private set; }

        [SerializeField] private PossessionManager  _possessionManager;
        [SerializeField] private SaveService        _saveService;
        [SerializeField] private WorldStateTracker  _worldStateTracker;

        public PossessionManager PossessionManager => _possessionManager;
        public SaveService       SaveService       => _saveService;
        public WorldStateTracker WorldStateTracker => _worldStateTracker;

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
