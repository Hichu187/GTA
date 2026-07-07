using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Game.Core.Persistence;

namespace Game.Systems.Persistence
{
    /// <summary>
    /// Central save/load orchestrator. Place on the Manager GO alongside
    /// GameplayServiceLocator. ISaveable components register themselves; SaveService
    /// never scans the scene.
    ///
    /// Save path: Application.persistentDataPath/save_slot_{slotIndex}.json
    /// </summary>
    public class SaveService : MonoBehaviour
    {
        [SerializeField] private int _slotIndex = 0;

        private readonly List<ISaveable> _saveables = new();

        public bool HasSave => File.Exists(FilePath);

        private string FilePath =>
            Path.Combine(Application.persistentDataPath, $"save_slot_{_slotIndex}.json");

        // ── Registration ──────────────────────────────────────────────────────

        public void Register(ISaveable saveable)
        {
            if (!_saveables.Contains(saveable))
                _saveables.Add(saveable);
        }

        public void Unregister(ISaveable saveable) => _saveables.Remove(saveable);

        // ── Save ──────────────────────────────────────────────────────────────

        public void Save()
        {
            var entries = new SaveEntry[_saveables.Count];
            for (int i = 0; i < _saveables.Count; i++)
            {
                entries[i] = new SaveEntry
                {
                    key  = _saveables[i].SaveKey,
                    data = _saveables[i].CaptureState(),
                };
            }

            var file = new SaveFile
            {
                version   = SaveFile.CurrentVersion,
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                timestamp = DateTime.UtcNow.ToString("o"),
                entries   = entries,
            };

            File.WriteAllText(FilePath, JsonUtility.ToJson(file, prettyPrint: true));
            Debug.Log($"[SaveService] Saved {entries.Length} entries → {FilePath}");
        }

        // ── Load ──────────────────────────────────────────────────────────────

        public bool Load()
        {
            if (!HasSave)
            {
                Debug.LogWarning($"[SaveService] No save file at {FilePath}");
                return false;
            }

            SaveFile file;
            try
            {
                file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to parse save file: {e.Message}");
                return false;
            }

            if (file.version != SaveFile.CurrentVersion)
            {
                Debug.LogWarning($"[SaveService] Save version mismatch ({file.version} vs {SaveFile.CurrentVersion}). Skipping.");
                return false;
            }

            // Build lookup for O(n) restore
            var lookup = new Dictionary<string, string>(file.entries.Length);
            foreach (var entry in file.entries)
                lookup[entry.key] = entry.data;

            int restored = 0;
            foreach (var s in _saveables)
            {
                if (lookup.TryGetValue(s.SaveKey, out var json))
                {
                    s.RestoreState(json);
                    restored++;
                }
            }

            Debug.Log($"[SaveService] Loaded — restored {restored}/{_saveables.Count} saveables.");
            return true;
        }

        // ── Delete ────────────────────────────────────────────────────────────

        public void DeleteSave()
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
