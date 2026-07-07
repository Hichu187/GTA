using UnityEngine;
using UnityEditor;
using Game.Core.Persistence;

namespace Game.Editor
{
    /// <summary>
    /// Batch-assigns stable GUIDs to all PersistentGUID components in the active scene
    /// that do not yet have an ID.
    ///
    /// Run via: Game → Assign Persistent GUIDs
    /// </summary>
    public static class PersistentGUIDAssigner
    {
        [MenuItem("Game/Assign Persistent GUIDs")]
        private static void AssignAll()
        {
            var all = Object.FindObjectsByType<PersistentGUID>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int assigned = 0;
            foreach (var pg in all)
            {
                if (pg.HasId) continue;
                Undo.RecordObject(pg, "Assign Persistent GUID");
                pg.AssignNewGUID();
                EditorUtility.SetDirty(pg);
                assigned++;
            }

            if (assigned > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                Debug.Log($"[PersistentGUIDAssigner] Assigned {assigned} new GUIDs. Save the scene to persist them.");
            }
            else
            {
                Debug.Log("[PersistentGUIDAssigner] All PersistentGUID components already have IDs.");
            }
        }

        /// <summary>Inspector button — appears on the PersistentGUID component in the Inspector.</summary>
        [CustomEditor(typeof(PersistentGUID))]
        public class PersistentGUIDEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var pg = (PersistentGUID)target;
                if (!pg.HasId)
                {
                    EditorGUILayout.HelpBox("No GUID assigned. Click below or run Game → Assign Persistent GUIDs.", MessageType.Warning);
                    if (GUILayout.Button("Assign GUID"))
                    {
                        Undo.RecordObject(pg, "Assign Persistent GUID");
                        pg.AssignNewGUID();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"GUID: {pg.Id}", MessageType.Info);
                    if (GUILayout.Button("Re-assign GUID (breaks existing save data)"))
                    {
                        if (EditorUtility.DisplayDialog(
                                "Re-assign GUID",
                                "This will invalidate any save data that references this object. Continue?",
                                "Re-assign", "Cancel"))
                        {
                            Undo.RecordObject(pg, "Re-assign Persistent GUID");
                            pg.AssignNewGUID();
                        }
                    }
                }
            }
        }
    }
}
