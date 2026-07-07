using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Weapons
{
    /// <summary>
    /// ScriptableObject that maps a weapon's type name to its prefab.
    /// Required by WeaponHolder.RestoreState to re-instantiate saved weapons.
    ///
    /// Create via: Assets → Create → GTA → Weapon Registry
    /// </summary>
    [CreateAssetMenu(menuName = "GTA/Weapon Registry", fileName = "WeaponRegistry")]
    public class WeaponRegistry : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string     typeName;
            public GameObject prefab;
        }

        [SerializeField] private List<Entry> _entries = new();

        private Dictionary<string, GameObject> _lookup;

        private void OnEnable() => BuildLookup();

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, GameObject>(_entries.Count);
            foreach (var e in _entries)
                if (!string.IsNullOrEmpty(e.typeName) && e.prefab != null)
                    _lookup[e.typeName] = e.prefab;
        }

        public bool TryGetPrefab(string typeName, out GameObject prefab)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(typeName, out prefab);
        }
    }
}
