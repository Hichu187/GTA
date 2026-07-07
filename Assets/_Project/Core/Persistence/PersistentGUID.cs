using UnityEngine;

namespace Game.Core.Persistence
{
    /// <summary>
    /// Attach to world objects that need a stable identity across save/load cycles
    /// (e.g. weapon pickups, item spawners).
    /// Use the Inspector button or the Editor tool (Game → Assign Persistent GUIDs)
    /// to generate IDs — never assign manually to avoid collisions.
    /// </summary>
    public class PersistentGUID : MonoBehaviour
    {
        [SerializeField] private string _id;

        public string Id => _id;

        public bool HasId => !string.IsNullOrEmpty(_id);

#if UNITY_EDITOR
        // Called by PersistentGUIDAssigner editor tool.
        public void AssignNewGUID()
        {
            _id = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
