using System.Collections.Generic;
using UnityEngine;
using Game.Core.HUD;

namespace Game.Systems.HUD
{
    public class HUDManager : MonoBehaviour
    {
        [SerializeField] private Canvas _rootCanvas;

        private readonly List<GameObject> _activeObjects = new();

        // Called by PossessionManager after each Possess.
        public void ApplyContext(IHUDContextProvider provider)
        {
            ClearAll();

            foreach (var handle in provider.GetActiveHUDModules())
            {
                if (!handle.IsValid) continue;

                var go     = Instantiate(handle.ModulePrefab, _rootCanvas.transform);
                var module = go.GetComponent<IHUDModule>();
                module?.Bind(handle.DataSource);
                module?.Show();
                _activeObjects.Add(go);
            }
        }

        private void ClearAll()
        {
            foreach (var go in _activeObjects)
                if (go != null) Destroy(go);
            _activeObjects.Clear();
        }
    }
}
