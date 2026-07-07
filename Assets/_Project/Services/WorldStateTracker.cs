using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Core.Persistence;

namespace Game.Services
{
    /// <summary>
    /// Tracks world objects that have been consumed (picked up, destroyed, used).
    /// Persists a set of PersistentGUID ids so those objects are not re-spawned after load.
    /// </summary>
    public class WorldStateTracker : MonoBehaviour, ISaveable
    {
        public string SaveKey => "WorldStateTracker";

        private readonly HashSet<string> _consumedGuids = new();

        // ── Public API ────────────────────────────────────────────────────────

        public void MarkConsumed(string guid)
        {
            if (!string.IsNullOrEmpty(guid))
                _consumedGuids.Add(guid);
        }

        public bool IsConsumed(string guid) => _consumedGuids.Contains(guid);

        /// <summary>
        /// Call after load to destroy world objects whose GUID is already consumed.
        /// Pass all PersistentGUID components in the scene.
        /// </summary>
        public void ApplyToScene(IEnumerable<PersistentGUID> sceneGuids)
        {
            foreach (var pg in sceneGuids)
            {
                if (pg.HasId && _consumedGuids.Contains(pg.Id))
                    Destroy(pg.gameObject);
            }
        }

        // ── ISaveable ─────────────────────────────────────────────────────────

        public string CaptureState()
        {
            var data = new WorldStateData { consumedGuids = new List<string>(_consumedGuids) };
            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            _consumedGuids.Clear();
            var data = JsonUtility.FromJson<WorldStateData>(json);
            if (data?.consumedGuids != null)
                foreach (var id in data.consumedGuids)
                    _consumedGuids.Add(id);
        }

        // ── Serialization helper ──────────────────────────────────────────────

        [Serializable]
        private class WorldStateData
        {
            public List<string> consumedGuids;
        }
    }
}
